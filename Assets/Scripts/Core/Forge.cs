// NIGHT8 U2-17: THE FORGE, ported from skygrind/src/survival/forge.js (NIGHT6 B1, Survival's Pack-a-Punch). A cabinet
// in THE GRINDER (it rides its core pillar; map data.forgeSpot); hold BUY with a weapon in hand to forge it for the run:
// x forge.damage on every hit and one bonus of its family (forge.families: a status chance, a headshot, blast or
// tough-target multiplier), a brighter gem and a "+" on its name. The answer to enemy HP growing 8% a round after round
// 10 (director.rounds.hpPerRound) while a gun's damage stays flat. The status rolls draw from the run's own seeded
// stream (mulberry32(runSeed ^ forge.seedSalt)), never an unseeded random. Ends with the run.
// The port: the statuses belong to the arsenal (combat/arsenal.js: BURN, SHOCK, PULL, STAGGER and their CROSS combos);
// NIGHT9 U3-08: `status` is the arsenal's (SurvivalGame), so a forged SHOTGUN (edge: STAGGER 25%) lands STAGGER as the web
// does. The kinetic bonus (headshot x1.25: KESTREL) and the damage work as on the web.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class ForgeMath
    {
        /// <summary>arsenalMath.js weaponOf: hit sources that aren't hotbar weapons map onto one (a ricochet is the KESTREL's, F the GAUNTLET's).</summary>
        static readonly Dictionary<string, string> Source = new Dictionary<string, string> { ["ricochet"] = "revolver", ["punch"] = "gauntlet", ["orb"] = "gauntlet", ["slam"] = "slam" };

        public static string WeaponOf(string weapon) => weapon != null && Source.TryGetValue(weapon, out string id) ? id : weapon;

        /// <summary>One family's bonus (config.forge.families[family]); 0 / null for what it doesn't have.</summary>
        public struct Bonus
        {
            public double headshot, blast, tough, damage, chance;
            public string status;
        }

        public sealed class P
        {
            public bool enabled;
            public double cost, damage, gem, glow;
            public uint seedSalt;
            public readonly List<string> toughKinds = new List<string>();
            public readonly Dictionary<string, Bonus> families = new Dictionary<string, Bonus>();

            public static P From(GameConfig c)
            {
                var p = new P
                {
                    enabled = c.Get("forge.enabled") is bool on && on, cost = c.Num("forge.cost"), damage = c.Num("forge.damage"),
                    gem = c.Num("forge.gem"), glow = c.Num("forge.glow"), seedSalt = (uint)c.Num("forge.seedSalt"),
                };
                p.toughKinds.AddRange(c.Strs("forge.toughKinds"));
                foreach (var kv in c.Obj("forge.families"))
                {
                    var d = PortData.Obj(kv.Value);
                    double N(string k) => d.TryGetValue(k, out object v) ? PortData.Num(v) : 0;
                    p.families[kv.Key] = new Bonus
                    {
                        headshot = N("headshot"), blast = N("blast"), tough = N("tough"), damage = N("damage"), chance = N("chance"),
                        status = d.TryGetValue("status", out object s) ? (string)s : null,
                    };
                }
                return p;
            }
        }

        /// <summary>
        /// forgeMult: the damage multiplier of a hit from a forged weapon (1 when not forged). `tough`: a boss, a
        /// forge.toughKinds kind or an enemy with affixes (the caller decides; the port has neither bosses nor affixes).
        /// </summary>
        public static double Mult(bool headshot, bool blast, bool tough, bool forged, P f, string family)
        {
            if (!forged) return 1;
            Bonus b = family != null && f.families.TryGetValue(family, out Bonus fb) ? fb : default;
            double k = f.damage;
            if (b.headshot != 0 && headshot) k *= b.headshot;
            if (b.blast != 0 && blast) k *= b.blast;
            if (b.tough != 0 && tough) k *= b.tough;
            if (b.damage != 0) k *= b.damage;
            return k;
        }
    }

    public sealed class ForgeEvent
    {
        public string owner = "player";
        public string weapon;
        public double price;
        public bool free;
        public int tier = 1; // NIGHT20 B3: the tier this buy reached (1 without design.adds.b3)
    }

    /// <summary>forge.js createForge: what is forged this run, the bonus hook, the status rolls.</summary>
    public sealed class Forge : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the guns forged (in order; the set is the same ids) and the status stream.
        public void Save(StateWriter w)
        {
            w.Strings("order", order);
            w.Rng("rng", rng);
            // NIGHT20 fix list G2: B3's tiers and FORGE II's proc clocks cross a break (in insertion order: the forged order)
            var ids = new List<string>(tiers.Count);
            var ts = new List<int>(tiers.Count);
            foreach (string id in order)
                if (tiers.TryGetValue(id, out int t))
                {
                    ids.Add(id);
                    ts.Add(t);
                }
            w.Strings("tierIds", ids);
            w.Ints("tiers", ts);
            w.SD("procAt", procAt);
        }

        public void Load(StateReader r)
        {
            List<string> o = r.Strings("order");
            foreach (string id in o) if (!Known(id)) throw new StateException($"snapshot: forge: an unknown gun '{id}'");
            forged.Clear();
            order.Clear();
            foreach (string id in o)
            {
                forged.Add(id);
                order.Add(id);
            }
            rng = r.Rng("rng");
            if (r.Has("tierIds")) // NIGHT20 fix list G2 (absent in an older save: every forged gun at tier I)
            {
                List<string> ids = r.Strings("tierIds");
                List<int> ts = r.Ints("tiers");
                if (ids.Count != ts.Count) throw new StateException("snapshot: forge tiers");
                tiers.Clear();
                for (int i = 0; i < ids.Count; i++) tiers[ids[i]] = ts[i];
                r.SD("procAt", procAt);
            }
        }

        readonly GameConfig cfg;
        readonly EventBus events;
        public readonly ForgeMath.P p;
        readonly HashSet<string> forged = new HashSet<string>();
        public readonly List<string> order = new List<string>(); // forged ids in the order they were forged (the results' row)
        readonly Dictionary<string, string> familyOf = new Dictionary<string, string>();
        Mulberry32 rng = new Mulberry32(1);
        /// <summary>The arsenal's status call (loadout.arsenal.status; NIGHT9 U3-08: set by SurvivalGame).</summary>
        public Action<Enemy, string> status;
        /// <summary>
        /// NIGHT19 U3-33.6 (design.forge, FORGE switch SIDEGRADE; set per run by SurvivalSim.Arsenal19): a forged gun deals a
        /// flat design.forge.mult (x1.4) whatever its family (no family bonus, no status rolls) and fires its variant
        /// (Loadout.forgedVariant): the forge sells what a gun does, not a bigger number (Research/2026-10-06-weapon-sandbox.md 1).
        /// </summary>
        public bool n19Sidegrade;
        readonly double sidegradeMult;

        public Forge(GameConfig cfg, EventBus events)
        {
            this.cfg = cfg;
            this.events = events;
            p = ForgeMath.P.From(cfg);
            TiersInit(); // NIGHT20 B3 (design.adds.b3)
            sidegradeMult = cfg.Has("design.forge.mult") ? cfg.Num("design.forge.mult") : 1;
            foreach (var kv in cfg.Obj("weapons.catalog"))
            {
                var d = PortData.Obj(kv.Value);
                familyOf[kv.Key] = d.TryGetValue("family", out object fam) ? (string)fam : "";
            }
        }

        public bool IsForged(string id) => id != null && forged.Contains(id);

        // --- NIGHT20 U3-34.9 B3 (builder G, design.adds.b3): THE FORGE in tiers, as sidegrades ---------------------------
        // BO6's Pack-a-Punch 5k / 15k / 30k, BO3's alternate ammo types, Hades' aspects (Research/2026-10-06-additions-
        // backlog.md row 11). FORGE I (prices[0]) is NIGHT19's sidegrade forge (the gun's variant, a flat design.forge.mult);
        // FORGE II (prices[1]) adds the gun's family from the web's forge.families: its status as a proc once per
        // procCooldown s per gun (not the web's chance), or its multiplier (kinetic headshots, pierce on tough kinds, blast,
        // impact); FORGE III (prices[2]) adds a finish (the forge event's tier 3: the views' and GG's sound) and a flat
        // finishMult, the same for every gun. The run sets `tiered` (SurvivalSim.Arsenal20) and `now` each step. The tiers
        // and the proc clocks are in the run snapshot (NIGHT20 fix list G2, Save / Load above).

        /// <summary>design.adds.b3 is this run's (the cabinet sells the next tier).</summary>
        public bool tiered;
        /// <summary>The run's clock (s), for FORGE II's proc cooldown.</summary>
        public double now;
        readonly Dictionary<string, int> tiers = new Dictionary<string, int>();
        readonly Dictionary<string, double> procAt = new Dictionary<string, double>(); // FORGE II: when a gun's status may proc again
        double[] tierPrices;
        double procCooldown, finishMult = 1;

        void TiersInit()
        {
            if (!cfg.Has("design.adds.b3.prices")) return;
            tierPrices = cfg.Nums("design.adds.b3.prices");
            procCooldown = cfg.Num("design.adds.b3.procCooldown");
            finishMult = cfg.Num("design.adds.b3.finishMult");
        }

        bool Tiers => tiered && tierPrices != null;

        /// <summary>A gun's tier: 0 not forged, 1 FORGE I .. 3 FORGE III (1 for any forged gun when untiered).</summary>
        public int Tier(string id) => !IsForged(id) ? 0 : tiers.TryGetValue(id, out int t) ? t : 1;

        /// <summary>The next tier's price for `id` (the web's forge.cost when untiered).</summary>
        public double NextPrice(string id) => Tiers && Tier(id) < tierPrices.Length ? tierPrices[Tier(id)] : p.cost;

        /// <summary>Can `id` be forged (untiered: once) or raised a tier now?</summary>
        public bool CanUpgrade(string id) => Known(id) && (Tiers ? Tier(id) < tierPrices.Length : !IsForged(id));

        /// <summary>Forge `id` or raise its tier (the cabinet's buy). False: unknown, or nothing left to buy.</summary>
        public bool Upgrade(string id, double price = 0)
        {
            if (!CanUpgrade(id)) return false;
            if (!IsForged(id)) return Give(id, price);
            int t = Tier(id) + 1;
            tiers[id] = t;
            events.Emit("forge", new ForgeEvent { weapon = id, price = price, free = price == 0, tier = t });
            return true;
        }

        public bool Known(string id) => id != null && familyOf.ContainsKey(id);

        public int Count => forged.Count;

        /// <summary>A new run: nothing forged, the status stream reseeded from the run seed.</summary>
        public void Reset(uint runSeed)
        {
            forged.Clear();
            order.Clear();
            rng = new Mulberry32(runSeed ^ p.seedSalt);
            tiers.Clear(); // NIGHT20 B3
            procAt.Clear();
        }

        /// <summary>Forge a weapon (a buy, or free at price 0: the web's PROTOCOL reward); once per gun per run.</summary>
        public bool Give(string id, double price = 0)
        {
            if (!Known(id) || forged.Contains(id)) return false;
            forged.Add(id);
            order.Add(id);
            events.Emit("forge", new ForgeEvent { weapon = id, price = price, free = price == 0 });
            return true;
        }

        /// <summary>The bonus hook (weapons.js loadout.bonus): x damage of a forged weapon's hit on `e`.</summary>
        public double Mult(string weapon, bool headshot, Enemy e)
        {
            string id = ForgeMath.WeaponOf(weapon);
            if (!IsForged(id)) return 1;
            if (n19Sidegrade) return Tiers ? sidegradeMult * TierMult(id, headshot, e) : sidegradeMult; // NIGHT19: flat, every gun alike (NIGHT20 B3: + the tiers)
            bool tough = e != null && p.toughKinds.Contains(e.kind);
            return ForgeMath.Mult(headshot, false, tough, true, p, familyOf[id]); // no blast weapons are ported
        }

        /// <summary>NIGHT20 B3: FORGE II's family multiplier (not its status) and FORGE III's finish, on top of the sidegrade.</summary>
        double TierMult(string id, bool headshot, Enemy e)
        {
            int t = Tier(id);
            double k = 1;
            if (t >= 2 && p.families.TryGetValue(familyOf[id], out ForgeMath.Bonus b))
            {
                if (b.headshot != 0 && headshot) k *= b.headshot;
                if (b.tough != 0 && e != null && p.toughKinds.Contains(e.kind)) k *= b.tough;
                if (b.damage != 0) k *= b.damage;
                if (b.blast != 0) k *= b.blast; // the blast family is the COMET's: every hit of it is a blast
            }
            if (t >= 3) k *= finishMult;
            return k;
        }

        /// <summary>NIGHT20 B3: FORGE II's status, once per procCooldown s per gun.</summary>
        void TierProc(Enemy e, string id)
        {
            if (Tier(id) < 2 || !e.alive || !p.families.TryGetValue(familyOf[id], out ForgeMath.Bonus b) || b.status == null) return;
            if (procAt.TryGetValue(id, out double at) && now < at) return;
            procAt[id] = now + procCooldown;
            status?.Invoke(e, b.status);
        }

        /// <summary>After a forged weapon's hit: its family's status, on a chance, from the run's stream.</summary>
        public void After(Enemy e, string weapon)
        {
            string id = ForgeMath.WeaponOf(weapon);
            if (Tiers && n19Sidegrade && IsForged(id) && e != null) { TierProc(e, id); return; } // NIGHT20 B3: FORGE II's proc
            if (!IsForged(id) || e == null || n19Sidegrade) return; // NIGHT19 sidegrade: no family status rolls
            if (!p.families.TryGetValue(familyOf[id], out ForgeMath.Bonus b) || b.status == null || !e.alive) return;
            if (rng.Next() < b.chance) status?.Invoke(e, b.status);
        }
    }
}
