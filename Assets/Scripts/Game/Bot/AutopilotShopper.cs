// NIGHT14 U3-27.5: the AUTOPILOT's shopper (a break's plan; the web's bot never shops). Each break step it takes the
// first entry of design.autopilot.shopping (Buyables ids; THE FORGE appended by design.autopilot.forge) that is still
// wanted, not dropped, and affordable with the gates on the cheapest way to its zone (a Bellman-Ford over the zones; a
// locked zone's gates are Buyables "gate:<zoneId>" items, bought from the open side). The goal is the first gate on that
// way, else the item itself. The bot walks there (the brain's nav override), stops in reach, faces the item and holds
// interact while the buy prompt's candidate is that item (Buyables' hold). Nothing affordable: hold interact with no
// candidate in reach, the sim's READY rule (from survival.readyFrom), walking back to the spawn if something is in reach.
// A goal not bought after design.autopilot.shopGiveUpSeconds of trying is dropped for design.autopilot.shopRetryRounds
// rounds (a machine out of reach from where the bot stood; NIGHT14 review B-7: dropped for the whole run, one such spot
// could end the bot's shopping).
// NIGHT20 P0-2 (builder P, design.autopilot.n20.exfilFrom): in a break that offers the exfil radio (Exfil.CanCall) from
// that round on, the radio is the first goal (free: walk, face, hold interact, the same buy path); given up after
// shopGiveUpSeconds, the break shops instead.
// NIGHT20 A3 (builder P2, THE BEACON's farm measure): with beaconShare > 0, a standing beacon (Beacon20Run) is the goal
// after the radio when its coin says so (one draw per beacon on a stream of the bot's own from the run seed and the round:
// beaconShare of them; the game's streams are never drawn); free, the same buy path; given up after shopGiveUpSeconds.
// 0 (the default): the bot never takes one (an ignored beacon changes nothing), so every other judge's bot is unchanged.
// Allocation-free per step (arrays built at construction, delegates called, no strings). No UnityEngine use.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class AutopilotShopper
    {
        /// <summary>One shopping entry: its item, its zone, what it is.</summary>
        sealed class AutopilotShopEntry
        {
            public string id;
            public Buyables.Item item;
            public int zone;
            public string perk; // "perk:<id>": the perk's id (no Substring per step)
            public bool forge;
            public bool dropped;
            public int retryRound; // NIGHT14 B-7: the round from which a dropped goal is tried again
        }

        /// <summary>A gate's buy point: the zone it opens, the zone you stand in to buy it, where to walk.</summary>
        sealed class AutopilotShopGate
        {
            public Buyables.Item item;
            public int opens, standIn;
            public Vec3 stand;
        }

        public enum Mode { None, Buy, Ready }

        readonly SurvivalSim sim;
        readonly AutopilotParams p;
        readonly List<AutopilotShopEntry> entries = new List<AutopilotShopEntry>();
        readonly List<AutopilotShopGate> gates = new List<AutopilotShopGate>();
        readonly double[] cost;
        readonly AutopilotShopGate[] via;
        readonly int giveUpSteps;
        readonly int retryRounds; // design.autopilot.shopRetryRounds
        readonly double reachHeight, buyReach;
        int goalEntry = -1, goalSteps;

        /// <summary>This step's plan: Buy (walk to `walkTo`, buy `goal`), Ready (READY the round) or None.</summary>
        public Mode mode;
        public Buyables.Item goal;
        public Vec3 walkTo;
        /// <summary>In reach of `goal`: stand still facing `faceYaw`.</summary>
        public bool arrived;
        public double faceYaw;
        /// <summary>Hold interact this step.</summary>
        public bool interact;
        /// <summary>Entries in design.autopilot.shopping with no Buyables item on this map (not ported, or off by flag).</summary>
        public readonly List<string> unknown = new List<string>();

        public AutopilotShopper(SurvivalSim sim, AutopilotParams p)
        {
            this.sim = sim;
            this.p = p;
            SurvivalMap map = sim.map;
            foreach (string id in p.shopping)
            {
                Buyables.Item item = null;
                foreach (Buyables.Item it in sim.buyables.list)
                    if (it.id == id)
                    {
                        item = it;
                        break;
                    }
                if (item == null)
                {
                    unknown.Add(id);
                    continue;
                }
                entries.Add(new AutopilotShopEntry
                {
                    id = id, item = item, zone = ZoneMath.ZoneIndexAt(map.zoneParts, item.position.x, item.position.y + 1, item.position.z),
                    perk = item.kind == "perk" && id.StartsWith("perk:", StringComparison.Ordinal) ? id.Substring(5) : null, forge = item.kind == "forge",
                });
            }
            // The gate items were added in zones.spots order (SurvivalSim's constructor): pair them to know each one's sides.
            int s = 0;
            foreach (Buyables.Item it in sim.buyables.list)
            {
                if (it.kind != "gate") continue;
                BuySpot spot = s < sim.zones.spots.Count ? sim.zones.spots[s++] : null;
                if (spot == null) break;
                gates.Add(new AutopilotShopGate
                {
                    item = it, opens = spot.zone, standIn = ZoneMath.ZoneIndexAt(map.zoneParts, spot.spot.pos.x, spot.spot.pos.y + 1, spot.spot.pos.z), stand = spot.spot.pos,
                });
            }
            cost = new double[map.zones.Count];
            via = new AutopilotShopGate[map.zones.Count];
            giveUpSteps = MovementMath.StepsFor(p.shopGiveUpSeconds, sim.step);
            retryRounds = p.shopRetryRounds;
            reachHeight = sim.cfg.Num("buy.reachHeight");
            buyReach = sim.cfg.Num("buy.reach");
        }

        /// <summary>A new run: nothing dropped, no goal.</summary>
        public void Reset()
        {
            foreach (AutopilotShopEntry e in entries)
            {
                e.dropped = false;
                e.retryRound = 0;
            }
            goalEntry = -1;
            goalSteps = 0;
            mode = Mode.None;
            goal = null;
            arrived = interact = false;
            radioGaveUp = 0;
            beaconGaveUp = coinFor = 0;
        }

        /// <summary>The entries dropped this run (out of reach), for the farm's report.</summary>
        /// <summary>NIGHT19 THE PARITY FARM: never buy a gun (the run keeps the gun it was locked to).</summary>
        public bool noGuns;

        public int Dropped
        {
            get
            {
                int n = 0;
                foreach (AutopilotShopEntry e in entries) if (e.dropped) n++;
                return n;
            }
        }

        bool Wanted(AutopilotShopEntry e)
        {
            if (e.perk != null) return !sim.perks.Owns(e.perk);
            if (e.forge)
            {
                string w = sim.loadout.Weapon;
                return sim.forge.Known(w) && !sim.forge.IsForged(w);
            }
            if (e.item.kind == "weapon") return !noGuns && (e.item.available == null || e.item.available()); // a wall gun's rule is "not owned" (no zone in it); NIGHT19: none in a gun-locked farm
            if (e.zone >= 0 && sim.zones.IsLocked(e.zone)) return true; // behind a gate: its own rule is read once it's open
            return e.item.available == null || e.item.available();
        }

        /// <summary>The cheapest gate prices from the open zones to every zone (Bellman-Ford over the gates; 6 zones).</summary>
        void Costs()
        {
            for (int z = 0; z < cost.Length; z++)
            {
                cost[z] = sim.zones.IsLocked(z) ? double.PositiveInfinity : 0;
                via[z] = null;
            }
            for (int pass = 0; pass < cost.Length; pass++)
            {
                bool changed = false;
                for (int i = 0; i < gates.Count; i++)
                {
                    AutopilotShopGate g = gates[i];
                    if (g.standIn < 0 || g.opens < 0 || double.IsPositiveInfinity(cost[g.standIn]) || !sim.zones.IsLocked(g.opens)) continue;
                    double c = cost[g.standIn] + sim.economy.Price(g.item.price());
                    if (c < cost[g.opens])
                    {
                        cost[g.opens] = c;
                        via[g.opens] = g;
                        changed = true;
                    }
                }
                if (!changed) break;
            }
        }

        /// <summary>The gate to buy first on the way to `zone`: the nearest buyable spot of the first locked zone on the way.</summary>
        AutopilotShopGate FirstGate(int zone)
        {
            AutopilotShopGate g = via[zone];
            while (g != null && sim.zones.IsLocked(g.standIn)) g = via[g.standIn];
            if (g == null) return null;
            Vec3 me = sim.mover.position;
            AutopilotShopGate best = null;
            double bestD = double.PositiveInfinity;
            for (int i = 0; i < gates.Count; i++)
            {
                AutopilotShopGate o = gates[i];
                if (o.opens != g.opens || o.standIn < 0 || sim.zones.IsLocked(o.standIn) || !(o.item.available == null || o.item.available())) continue;
                double dx = o.stand.x - me.x, dz = o.stand.z - me.z;
                double d = dx * dx + dz * dz;
                if (d < bestD)
                {
                    bestD = d;
                    best = o;
                }
            }
            return best;
        }

        /// <summary>NIGHT20 P0-2: the first round whose offered exfil the break calls (AutopilotInput.ExfilFrom; 0: never).</summary>
        public int exfilFrom;

        int radioGaveUp; // NIGHT20: the round whose radio walk was given up (shopGiveUpSeconds), so this break shops instead

        /// <summary>NIGHT20 P0-2: this break offers the exfil radio and the bot wants it (from exfilFrom on).</summary>
        bool RadioWanted() => exfilFrom > 0 && sim.exfil != null && sim.exfil.Radio != null && sim.run.round >= exfilFrom
            && radioGaveUp != sim.run.round && sim.exfil.CanCall();

        const int RadioEntry = -2; // goalEntry's mark for the radio (the entries are 0..)

        /// <summary>NIGHT20 A3: the share of standing beacons the bot takes (the farm's THE BEACON measure; 0: never).</summary>
        public double beaconShare;

        const int BeaconEntry = -3; // goalEntry's mark for THE BEACON
        const uint BeaconCoinSalt = 0x2c1b3c6du; // the bot's own coin stream (a measurement's, not a game number)
        int beaconGaveUp, coinFor; // the beacon round given up this break; the beacon round the coin was drawn for
        bool coin;

        /// <summary>NIGHT20 A3: the bot's coin for the beacon standing before `round` (beaconShare of them; a pure function of
        /// the run seed and the round).</summary>
        public static bool TakesBeacon(uint runSeed, int round, double share) =>
            share > 0 && new Mulberry32(unchecked(runSeed ^ BeaconCoinSalt ^ (uint)round * 0x9e3779b1u)).Next() < share;

        /// <summary>NIGHT20 A3: a beacon stands and the bot takes it (its coin, drawn once per beacon; not given up).</summary>
        bool BeaconWanted()
        {
            Beacon20Run b = sim.beacon20;
            if (beaconShare <= 0 || b == null || !b.Standing || beaconGaveUp == b.StandFor) return false;
            if (coinFor != b.StandFor)
            {
                coinFor = b.StandFor;
                coin = TakesBeacon(sim.runSeed, b.StandFor, beaconShare);
            }
            return coin;
        }

        /// <summary>One break step's plan (mode, goal, walkTo, arrived, faceYaw, interact).</summary>
        public void Plan()
        {
            mode = Mode.Ready;
            goal = null;
            arrived = interact = false;
            Costs();
            int chosen = -1;
            AutopilotShopGate gate = null;
            double bank = sim.economy.total;
            if (RadioWanted()) chosen = RadioEntry; // NIGHT20 P0-2: the radio first (free; the shopping after it)
            else if (BeaconWanted()) chosen = BeaconEntry; // NIGHT20 A3: then a beacon the coin takes (free)
            for (int i = 0; i < entries.Count && chosen == -1; i++)
            {
                AutopilotShopEntry e = entries[i];
                if (e.dropped && sim.run.round >= e.retryRound) e.dropped = false; // NIGHT14 B-7: tried again
                if (e.dropped || !Wanted(e)) continue;
                double way = e.zone >= 0 ? cost[e.zone] : 0;
                if (double.IsPositiveInfinity(way)) continue; // no gate leads there
                double price = sim.economy.Price(e.item.price());
                if (bank < way + price) continue;
                if (way > 0)
                {
                    gate = FirstGate(e.zone);
                    if (gate == null) continue;
                }
                chosen = i;
                break;
            }
            if (chosen != goalEntry)
            {
                goalEntry = chosen;
                goalSteps = 0;
            }
            if (chosen == -1) // (RadioEntry is -2, BeaconEntry -3)
            {
                // READY: nothing affordable. Hold BUY with nothing in reach; walk back to the spawn while something is.
                walkTo = sim.map.playerSpawn;
                interact = arrived = sim.buyables.candidate == null; // arrived: stand still where it is (faceYaw: the look now)
                faceYaw = sim.input.Yaw;
                return;
            }
            if (++goalSteps > giveUpSteps)
            {
                if (chosen == RadioEntry) radioGaveUp = sim.run.round; // NIGHT20: this break shops instead
                else if (chosen == BeaconEntry) beaconGaveUp = sim.beacon20.StandFor; // NIGHT20 A3: the beacon left standing
                else
                {
                    entries[chosen].dropped = true; // out of reach for now: the next breaks plan without it
                    entries[chosen].retryRound = sim.run.round + retryRounds;
                }
                goalEntry = -1;
                mode = Mode.None;
                return;
            }
            mode = Mode.Buy;
            goal = gate != null ? gate.item : chosen == RadioEntry ? sim.exfil.Radio : chosen == BeaconEntry ? sim.beacon20.Item : entries[chosen].item;
            walkTo = gate != null ? gate.stand : goal.position;
            Vec3 me = sim.mover.position;
            double dx = goal.position.x - me.x, dz = goal.position.z - me.z;
            double reach = goal.reach > 0 ? goal.reach : buyReach; // Buyables.Pick: the item's reach, else buy.reach
            bool inReach = Math.Sqrt(dx * dx + dz * dz) <= reach && Math.Abs(goal.position.y - me.y) <= reachHeight;
            Buyables.Item candidate = sim.buyables.candidate;
            // In reach: face it and stand still, unless another item wins the prompt (then keep walking in).
            arrived = inReach && (candidate == null || ReferenceEquals(candidate, goal));
            if (inReach) faceYaw = JsMath.Atan2(-dx, -dz);
            interact = ReferenceEquals(candidate, goal);
        }
    }
}
