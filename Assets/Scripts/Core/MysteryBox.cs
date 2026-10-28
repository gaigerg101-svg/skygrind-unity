// NIGHT10 U3-13.8 (decision D-20): THE MYSTERY BOX. New design, not a web port: the web game never had one. The
// reference is Call of Duty Zombies' box (ZOMBIES-ULTRAKILL-RESEARCH.md section 2.1, "The Mystery Box and the
// economy"): pay 950, the box cycles through the guns and offers one at random; on CoD's teddy ladder (by the number of
// pulls since the box last moved: 1-3 0%, 4-7 15%, 8-12 30%, then 50%) a teddy bear comes out instead, the price comes
// back, and the box flies to another spot. Fire Sale is not ported; SKYGRIND has no ammo, so there is no Max Ammo
// analogue.
// Every number is Unity-side (Design/unity-config.json design.box, read once here; CLAUDE.md rule 5). Rolls come from
// the box's own Mulberry32, reseeded per run from runSeed ^ design.box.seedSalt (rule 6). Timers are whole fixed steps
// (stepsFor). Per-step code allocates nothing; event payloads are reused (read them inside the handler).
// The Game's side (SurvivalGame): build the box only when MysteryBox.Enabled(cfg) (flag off: no box, the web's game);
// Reset(runSeed) at run start; a Buyables item with id MysteryBox.ItemId whose buy calls Roll; Step(dt) once per fixed
// step; Take on E while Offering. Events (port-only names, PORT-STATUS's list): boxOpen, boxRoll, boxTeddy, boxMove,
// boxTake, boxSink.
using System;
using System.Collections.Generic;
using System.IO;

namespace Skygrind.Core
{
    public enum BoxState
    {
        Idle, // at `spot`, buyable
        Revealing, // a paid roll: the cycle (revealTime s); the outcome is decided but not shown
        Offering, // the gun waits to be taken (offerTime s), then sinks
        Teddy, // the bear (teddyTime s); the price came back
        Moving, // gone (moveTime s); lands at `spot` (the new one)
    }

    /// <summary>A box spot (design.box.spots): feet, the yaw its front faces ((sin yaw, 0, cos yaw), as data.perkSpots), its zone id.</summary>
    public struct BoxSpot
    {
        public Vec3 pos;
        public double yaw;
        public string zone;
    }

    /// <summary>boxOpen: a paid roll starts (`price`: what was paid; `pull`: pulls since the box last moved, this one included).</summary>
    public sealed class BoxOpenEvent
    {
        public string owner = "player";
        public int spot;
        public int pull;
        public double price;
    }

    /// <summary>boxRoll: the reveal ends on a gun (the catalog id).</summary>
    public sealed class BoxRollEvent
    {
        public string owner = "player";
        public string weapon;
        public int spot;
        public int pull;
    }

    /// <summary>boxTeddy: the reveal ends on the bear; `refund` points came back.</summary>
    public sealed class BoxTeddyEvent
    {
        public string owner = "player";
        public double refund;
        public int spot;
        public int pull;
    }

    /// <summary>boxMove: the box leaves `from` for `to` (`zone`: the new spot's zone id).</summary>
    public sealed class BoxMoveEvent
    {
        public int from;
        public int to;
        public string zone;
    }

    /// <summary>boxTake (the offer was taken) and boxSink (it wasn't, in time).</summary>
    public sealed class BoxOfferEvent
    {
        public string owner = "player";
        public string weapon;
        public int spot;
    }

    public static class BoxMath
    {
        /// <summary>
        /// CoD's teddy ladder: the chance for pull number `pull` (1 = the first since the box last moved). `pulls` holds
        /// the bands' last pulls ([3, 7, 12]); `chance` one more entry than it ([0, 0.15, 0.3, 0.5]).
        /// </summary>
        public static double TeddyChance(int pull, double[] pulls, double[] chance)
        {
            int k = 0;
            while (k < pulls.Length && pull > pulls[k]) k++;
            return chance[k];
        }
    }

    public sealed class MysteryBox : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the stream, the phase and its clocks, the spot (and the one it left), the
        // pulls, who rolled and what it cost, the hidden pick and a teddy's destination, a spend waiting for its roll, and
        // the reveal's cycle (DisplayWeapon reads it).
        public void Save(StateWriter w)
        {
            w.Rng("rng", rng);
            w.D("dt", dt);
            w.I("state", (int)state);
            w.I("spot", spot);
            w.I("fromSpot", fromSpot);
            w.I("pulls", pulls);
            w.I("totalPulls", totalPulls);
            w.S("owner", owner);
            w.D("paid", paid);
            w.S("pick", pick);
            w.I("to", to);
            w.I("phaseSteps", phaseSteps);
            w.I("phaseTotal", phaseTotal);
            w.D("spentPending", spentPending);
            w.Strings("cycle", cycle);
        }

        public void Load(StateReader r)
        {
            int s = r.Int("state"), sp = r.Int("spot"), from = r.Int("fromSpot"), t = r.Int("to");
            if (!Enum.IsDefined(typeof(BoxState), s) || sp < 0 || sp >= spots.Count || from < -1 || from >= spots.Count || t < 0 || t >= spots.Count)
                throw new StateException("snapshot: box: the state or a spot is out of range");
            List<string> c = r.Strings("cycle");
            rng = r.Rng("rng");
            dt = r.D("dt");
            state = (BoxState)s;
            spot = sp;
            fromSpot = from;
            pulls = r.Int("pulls");
            totalPulls = r.Int("totalPulls");
            owner = r.S("owner");
            paid = r.D("paid");
            pick = r.S("pick");
            to = t;
            phaseSteps = r.Int("phaseSteps");
            phaseTotal = r.Int("phaseTotal");
            spentPending = r.D("spentPending");
            cycle.Clear();
            cycle.AddRange(c);
        }

        /// <summary>The Buyables item id the Game gives the box (its `pointsSpent` tells the box what a roll cost).</summary>
        public const string ItemId = "box";

        /// <summary>design.box.enabled: false (or no design config) means no box at all, the web's game.</summary>
        public static bool Enabled(GameConfig cfg) => cfg.Design("box.enabled");

        readonly EventBus events;
        readonly Economy economy; // a teddy's refund (null: none)
        readonly WeaponCatalog catalog;
        readonly List<BoxSpot> spots = new List<BoxSpot>();
        readonly double[] teddyPulls, teddyChance;
        readonly uint seedSalt;
        public readonly double price, revealTime, cycleTime, offerTime, teddyTime, moveTime;
        Mulberry32 rng = new Mulberry32(1);
        double dt;

        public BoxState state;
        /// <summary>The spot the box stands at (while Moving: the one it lands at).</summary>
        public int spot;
        /// <summary>While Moving: the spot it left; otherwise -1.</summary>
        public int fromSpot = -1;
        /// <summary>Paid rolls since the box last moved (the teddy ladder counts these; a move resets it).</summary>
        public int pulls;
        /// <summary>Paid rolls this run.</summary>
        public int totalPulls;
        /// <summary>Who rolled (only they can take the offer).</summary>
        public string owner = "player";
        /// <summary>What this roll cost (a teddy refunds it).</summary>
        public double paid;
        /// <summary>
        /// Optional: may the box move to spot i (e.g. its zone is open)? A teddy only moves the box to another spot this
        /// allows; with none, no teddy can come out. Null: every spot.
        /// </summary>
        public Func<int, bool> spotOpen;

        string pick; // the decided gun (null: a teddy), hidden until the reveal ends
        int to; // a teddy's destination
        int phaseSteps, phaseTotal;
        double spentPending = -1; // the last pointsSpent for ItemId since the last roll (-1: none)
        readonly List<string> pool = new List<string>(); // this roll's candidates (catalog order)
        readonly List<string> cycle = new List<string>(); // the guns the reveal cycles through
        readonly List<int> dests = new List<int>();
        readonly BoxOpenEvent openEvent = new BoxOpenEvent();
        readonly BoxRollEvent rollEvent = new BoxRollEvent();
        readonly BoxTeddyEvent teddyEvent = new BoxTeddyEvent();
        readonly BoxMoveEvent moveEvent = new BoxMoveEvent();
        readonly BoxOfferEvent offerEvent = new BoxOfferEvent();

        /// <param name="mapSpots">NIGHT15: a Unity-authored map's spots (SurvivalMap.spots): its "box" list replaces design.box.spots.</param>
        public MysteryBox(GameConfig cfg, EventBus events, Economy economy, WeaponCatalog catalog = null, Dictionary<string, object> mapSpots = null)
        {
            this.events = events;
            this.economy = economy;
            this.catalog = catalog ?? WeaponCatalog.From(cfg);
            price = cfg.Num("design.box.price");
            teddyPulls = cfg.Nums("design.box.teddyPulls");
            teddyChance = cfg.Nums("design.box.teddyChance");
            if (teddyChance.Length != teddyPulls.Length + 1) throw new InvalidDataException("design.box.teddyChance needs one entry more than design.box.teddyPulls");
            revealTime = cfg.Num("design.box.revealTime");
            cycleTime = cfg.Num("design.box.cycleTime");
            offerTime = cfg.Num("design.box.offerTime");
            teddyTime = cfg.Num("design.box.teddyTime");
            moveTime = cfg.Num("design.box.moveTime");
            seedSalt = (uint)cfg.Num("design.box.seedSalt");
            List<object> spotList = mapSpots != null && mapSpots.TryGetValue("box", out object mb) && mb != null ? PortData.Arr(mb) : cfg.Arr("design.box.spots");
            foreach (object o in spotList)
            {
                var d = PortData.Obj(o);
                spots.Add(new BoxSpot { pos = new Vec3(PortData.Num(d["x"]), PortData.Num(d["y"]), PortData.Num(d["z"])), yaw = PortData.Num(d["yaw"]), zone = (string)d["zone"] });
            }
            if (spots.Count == 0) throw new InvalidDataException("design.box.spots is empty");
            dt = cfg.Dt;
            events.On<SpentEvent>("pointsSpent", e =>
            {
                if (e.what == ItemId) spentPending = e.price;
            });
        }

        public int SpotCount => spots.Count;

        public BoxSpot Spot(int i) => spots[i];

        /// <summary>The spot the box stands at (or lands at, while Moving).</summary>
        public BoxSpot Current => spots[spot];

        /// <summary>Buyable now (the Buyables item's `available`, with PoolCount > 0).</summary>
        public bool CanBuy => state == BoxState.Idle;

        /// <summary>The teddy chance of pull number `pull`.</summary>
        public double TeddyChance(int pull) => BoxMath.TeddyChance(pull, teddyPulls, teddyChance);

        /// <summary>A new run: Idle at the first spot (the starting zone), no pulls, the stream from the run seed.</summary>
        public void Reset(uint runSeed)
        {
            rng = new Mulberry32(runSeed ^ seedSalt);
            state = BoxState.Idle;
            spot = 0;
            fromSpot = -1;
            pulls = totalPulls = 0;
            pick = null;
            paid = 0;
            phaseSteps = phaseTotal = 0;
            spentPending = -1;
            owner = "player";
        }

        /// <summary>How many guns a roll could give: catalog guns (not melee) that `isPorted` and not `owned`.</summary>
        public int PoolCount(IList<string> owned, Func<string, bool> isPorted)
        {
            int n = 0;
            foreach (string id in catalog.order) if (Eligible(id, owned, isPorted)) n++;
            return n;
        }

        bool Eligible(string id, IList<string> owned, Func<string, bool> isPorted) => catalog.Get(id).kind != "melee" && isPorted(id) && PortOk(id) && (owned == null || !owned.Contains(id));

        /// <summary>
        /// NIGHT19 U3-33.6 (design.arsenal, set per run by SurvivalSim.Arsenal19): the guns ported tonight (ZAPPER, BOUNCER) join
        /// the pool. Off: NIGHT18's pool whatever Loadout.Fires says (the design's block may list them at boot).
        /// </summary>
        public bool ports;

        bool PortOk(string id) => (ports || !Loadout.IsPort19(id)) && (ports20 || !Loadout.IsPort20(id));

        /// <summary>NIGHT20 B5 (design.adds.b5, set per run by SurvivalSim.Arsenal20): the TORCH and the VORTEX join the pool.</summary>
        public bool ports20;

        /// <summary>
        /// A paid roll (the Buyables item's buy, after the price was spent). `paid` &lt; 0: what the last `pointsSpent` for
        /// ItemId spent (0 if none: a refund never makes points). The teddy draw comes first (one rng draw every roll),
        /// then the destination (a teddy) or the gun (uniform over the pool, catalog order). An empty pool (every ported
        /// gun owned) refunds what was paid and changes nothing else (no pull, no draw, no event); the Game hides the
        /// prompt then (PoolCount 0), so this is the safety net. Returns whether the box opened.
        /// </summary>
        public bool Roll(IList<string> owned, Func<string, bool> isPorted, double paid = -1, string owner = "player", bool noTeddy = false)
        {
            double cost = paid >= 0 ? paid : spentPending >= 0 ? spentPending : 0;
            spentPending = -1;
            if (state != BoxState.Idle)
            {
                economy?.Refund(cost, "boxRefund");
                return false;
            }
            pool.Clear();
            cycle.Clear();
            foreach (string id in catalog.order)
            {
                if (catalog.Get(id).kind == "melee" || !isPorted(id) || !PortOk(id)) continue;
                cycle.Add(id);
                if (owned == null || !owned.Contains(id)) pool.Add(id);
            }
            if (pool.Count == 0)
            {
                economy?.Refund(cost, "boxRefund");
                return false;
            }
            this.paid = cost;
            this.owner = owner;
            pulls++;
            totalPulls++;
            dests.Clear();
            for (int i = 0; i < spots.Count; i++) if (i != spot && (spotOpen == null || spotOpen(i))) dests.Add(i);
            bool teddy = rng.Next() < TeddyChance(pulls) && dests.Count > 0 && !noTeddy; // NIGHT24: the BIG PICK's free spin is never a bear (the draw is still taken)
            if (teddy)
            {
                pick = null;
                to = dests[(int)Math.Floor(rng.Next() * dests.Count)];
            }
            else
            {
                pick = pool[(int)Math.Floor(rng.Next() * pool.Count)];
                to = spot;
            }
            Phase(BoxState.Revealing, revealTime);
            openEvent.owner = owner;
            openEvent.spot = spot;
            openEvent.pull = pulls;
            openEvent.price = cost;
            events.Emit("boxOpen", openEvent);
            return true;
        }

        /// <summary>Take the offered gun (E): its catalog id, or null when nothing is on offer for `owner`. The Game swaps it in.</summary>
        public string Take(string owner = "player")
        {
            if (state != BoxState.Offering || owner != this.owner) return null;
            string id = pick;
            Offer("boxTake", id);
            Idle();
            return id;
        }

        /// <summary>One fixed step: the phase timers and their ends. Allocates nothing.</summary>
        public void Step(double dt)
        {
            this.dt = dt;
            if (state == BoxState.Idle || --phaseSteps > 0) return;
            switch (state)
            {
                case BoxState.Revealing:
                    if (pick != null)
                    {
                        Phase(BoxState.Offering, offerTime);
                        rollEvent.owner = owner;
                        rollEvent.weapon = pick;
                        rollEvent.spot = spot;
                        rollEvent.pull = pulls;
                        events.Emit("boxRoll", rollEvent);
                    }
                    else
                    {
                        Phase(BoxState.Teddy, teddyTime);
                        economy?.Refund(paid, "boxRefund");
                        teddyEvent.owner = owner;
                        teddyEvent.refund = paid;
                        teddyEvent.spot = spot;
                        teddyEvent.pull = pulls;
                        events.Emit("boxTeddy", teddyEvent);
                    }
                    break;
                case BoxState.Offering:
                    Offer("boxSink", pick);
                    Idle();
                    break;
                case BoxState.Teddy:
                    fromSpot = spot;
                    spot = to;
                    pulls = 0;
                    Phase(BoxState.Moving, moveTime);
                    moveEvent.from = fromSpot;
                    moveEvent.to = spot;
                    moveEvent.zone = spots[spot].zone;
                    events.Emit("boxMove", moveEvent);
                    break;
                case BoxState.Moving:
                    Idle();
                    break;
            }
        }

        void Phase(BoxState s, double seconds)
        {
            state = s;
            phaseTotal = phaseSteps = MovementMath.StepsFor(seconds, dt);
        }

        void Idle()
        {
            state = BoxState.Idle;
            fromSpot = -1;
            pick = null;
            phaseSteps = phaseTotal = 0;
        }

        void Offer(string name, string weapon)
        {
            offerEvent.owner = owner;
            offerEvent.weapon = weapon;
            offerEvent.spot = spot;
            events.Emit(name, offerEvent);
        }

        /// <summary>0..1 through the current phase (0 when Idle).</summary>
        public double PhaseProgress => phaseTotal > 0 ? 1 - phaseSteps / (double)phaseTotal : 0;

        /// <summary>Seconds left in the current phase (0 when Idle).</summary>
        public double PhaseLeft => phaseSteps * dt;

        /// <summary>The reveal's 0..1 (1 once it has ended on a gun or the bear; 0 when Idle or Moving).</summary>
        public double RevealProgress => state == BoxState.Revealing ? PhaseProgress : state == BoxState.Offering || state == BoxState.Teddy ? 1 : 0;

        /// <summary>Seconds before the offer sinks (0 when nothing is on offer).</summary>
        public double OfferLeft => state == BoxState.Offering ? PhaseLeft : 0;

        /// <summary>The gun on offer (Offering), else null.</summary>
        public string Offered => state == BoxState.Offering ? pick : null;

        /// <summary>
        /// The gun the box shows: while Revealing, the cycle (every ported gun, owned or not, in catalog order, one per
        /// cycleTime s from the roll; a function of the step count); while Offering, the offer; else null (a teddy: the bear).
        /// </summary>
        public string DisplayWeapon
        {
            get
            {
                if (state == BoxState.Offering) return pick;
                if (state != BoxState.Revealing || cycle.Count == 0) return null;
                int elapsed = phaseTotal - phaseSteps;
                return cycle[(elapsed / MovementMath.StepsFor(cycleTime, dt)) % cycle.Count];
            }
        }
    }
}
