// NIGHT33 builder E (row design.n33.bosses33; DESIGN33 E "bosses still too tanky", D-2): each new boss fight's pool is cut to the row's own
// multiplier. design.n33.bosses33.<lantern|stag|wyrm|loom>.hpMult REPLACES NIGHT32's design.n32.bosses32.<boss>.hpMult (the factor applied here is
// n33 / n32 when NIGHT32's Boss32Pool already scaled this fight, else the n33 multiplier itself); THE CROWN's pool becomes design.n33.bosses33.crown.pool
// (in place of design.n32.boss5.pool, x the same ctx.scale.hp). Applied once per fight, at full health, in the step the fight's body first lives
// (N33BossStep runs after N32BossStep, so NIGHT32's cut is already in). The anti-camp rules are untouched: stillness is punished by the NIGHT32 rows.
// Off (the row off, --off33): nothing here runs and NIGHT32's pools stand.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>Harness only (Tools/SimHarness `bosses33`): force the bosses33 bit for every run this process starts (null: the run's own bits). Never set by the game.</summary>
        public static bool? Force33Bosses;

        /// <summary>Harness only (`bosses33 --set boss=value`): a tuning probe's hpMult (crown: pool) in place of the config's. Null in the game.</summary>
        public static System.Collections.Generic.Dictionary<string, double> Probe33Bosses;

        double Bosses33Num(string boss, string key)
        {
            if (Probe33Bosses != null && Probe33Bosses.TryGetValue(boss, out double v)) return v;
            return cfg.Num(Bosses33Root + boss + "." + key);
        }

        int b33Lantern = -1, b33Stag = -1, b33Wyrm = -1, b33Loom = -1, b33Crown = -1;

        /// <summary>The harness's count: the fights whose pool this row cut this run.</summary>
        public int bosses33Cuts;

        const string Bosses33Root = "design.n33.bosses33.";

        // ---- the anti-camp (round 2, the lead 11:06: punish stillness harder, never more health) ----
        // design.n33.bosses33.<boss>.<key> overrides NIGHT32's anti-camp number of the same name (design.n32.bosses32.<boss>.<key>) while the row is on and the boss's
        // NIGHT32 row is on (its Set32 has run); the originals come back at the next run's start with the row off.
        static readonly string[] B33CampKeys =
        {
            "lantern.campStillSeconds", "lantern.campStillSpeed", "lantern.campTellSeconds", "lantern.campRadius", "lantern.campDamage", "lantern.campEverySeconds",
            "stag.stillSeconds", "stag.stillSpeed", "stag.shardEverySeconds", "stag.shardDamage",
            "wyrm.stillSeconds", "wyrm.spitEverySeconds", "wyrm.spitDamage",
            "loom.deckSeconds", "loom.dropTellSeconds", "loom.dropDamage", "loom.dropEverySeconds",
        };
        readonly double[] b33Camp = new double[B33CampKeys.Length], b33Orig = new double[B33CampKeys.Length];
        bool b33Applied, b33Breaks;

        bool B33Live(int i) => i < 6 ? lantern31 != null && lantern31.on32 : i < 10 ? stag31 != null && stag31.on32 : i < 13 ? wyrm31 != null && wyrm31.on32 : loom31 != null && loom31.on32;

        double B33Get(int i)
        {
            switch (i)
            {
                case 0: return lantern31.campStill32;
                case 1: return lantern31.campSpeed32;
                case 2: return lantern31.campTell32;
                case 3: return lantern31.campRadius32;
                case 4: return lantern31.campDamage32;
                case 5: return lantern31.campEvery32;
                case 6: return stag31.p.stillSeconds;
                case 7: return stag31.p.stillSpeed;
                case 8: return stag31.p.shardEverySeconds;
                case 9: return stag31.p.shardDamage;
                case 10: return wyrm31.p.stillSeconds;
                case 11: return wyrm31.p.spitEverySeconds;
                case 12: return wyrm31.p.spitDamage;
                case 13: return loom31.p.deckSeconds;
                case 14: return loom31.p.dropTellSeconds;
                case 15: return loom31.p.dropDamage;
                default: return loom31.p.dropEverySeconds;
            }
        }

        void B33Set(int i, double v)
        {
            switch (i)
            {
                case 0: lantern31.campStill32 = v; break;
                case 1: lantern31.campSpeed32 = v; break;
                case 2: lantern31.campTell32 = v; break;
                case 3: lantern31.campRadius32 = v; break;
                case 4: lantern31.campDamage32 = v; break;
                case 5: lantern31.campEvery32 = v; break;
                case 6: stag31.p.stillSeconds = v; break;
                case 7: stag31.p.stillSpeed = v; break;
                case 8: stag31.p.shardEverySeconds = v; break;
                case 9: stag31.p.shardDamage = v; break;
                case 10: wyrm31.p.stillSeconds = v; break;
                case 11: wyrm31.p.spitEverySeconds = v; break;
                case 12: wyrm31.p.spitDamage = v; break;
                case 13: loom31.p.deckSeconds = v; break;
                case 14: loom31.p.dropTellSeconds = v; break;
                case 15: loom31.p.dropDamage = v; break;
                default: loom31.p.dropEverySeconds = v; break;
            }
        }

        /// <summary>Each step with the row on: the n33 anti-camp numbers over NIGHT32's (the first write keeps the original), and the Lantern's strike through its breaks.</summary>
        void Bosses33Camp()
        {
            for (int i = 0; i < B33CampKeys.Length; i++)
            {
                if (double.IsNaN(b33Camp[i]) || !B33Live(i)) continue;
                if (!b33Applied || double.IsNaN(b33Orig[i])) b33Orig[i] = B33Get(i);
                B33Set(i, b33Camp[i]);
            }
            b33Applied = true;
            if (lantern31 != null) lantern31.breaks33 = b33Breaks && lantern27 != null && lantern27.Alive;
        }

        partial void N33BossStartRun()
        {
            if (Force33Bosses.HasValue) n33.bosses33 = Force33Bosses.Value;
            b33Lantern = b33Stag = b33Wyrm = b33Loom = b33Crown = -1;
            bosses33Cuts = 0;
            if (b33Applied && !n33.bosses33)
            {
                // the row went off on this sim: NIGHT32's numbers back
                for (int i = 0; i < B33CampKeys.Length; i++) if (!double.IsNaN(b33Orig[i]) && B33Live(i)) B33Set(i, b33Orig[i]);
                if (lantern31 != null) lantern31.breaks33 = false;
                b33Applied = false;
            }
            if (!b33Applied) for (int i = 0; i < b33Orig.Length; i++) b33Orig[i] = double.NaN;
            if (!n33.bosses33) return;
            for (int i = 0; i < B33CampKeys.Length; i++)
            {
                string k = Bosses33Root + B33CampKeys[i];
                b33Camp[i] = Probe33Bosses != null && Probe33Bosses.TryGetValue(B33CampKeys[i], out double pv) ? pv : cfg.Has(k) ? cfg.Num(k) : double.NaN;
            }
            b33Breaks = cfg.Design("n33.bosses33.lantern.campThroughBreaks");
            b33ArmorScale = Probe33Bosses != null && Probe33Bosses.TryGetValue("armor", out double av) ? av : cfg.Has(Bosses33Root + "stillArmor.scale") ? cfg.Num(Bosses33Root + "stillArmor.scale") : 1;
            b33ArmorStag = Probe33Bosses != null && Probe33Bosses.TryGetValue("stagArmor", out double sv) ? sv : cfg.Has(Bosses33Root + "stillArmor.stagScale") ? cfg.Num(Bosses33Root + "stillArmor.stagScale") : b33ArmorScale;
            b33ArmorRadius = cfg.Has(Bosses33Root + "stillArmor.stillRadius") ? cfg.Num(Bosses33Root + "stillArmor.stillRadius") : 0;
            b33ArmorSeconds = cfg.Has(Bosses33Root + "stillArmor.stillSeconds") ? cfg.Num(Bosses33Root + "stillArmor.stillSeconds") : 0;
            b33Still = 0;
            b33ArmorBody = null;
            b33ArmorHp = -1;
            bosses33Armor = false;
            bosses33ArmorSteps = 0;
            bosses33ArmorGiven = 0;
        }

        // ---- STILL ARMOR (round 2): a boss shrugs off part of what a STILL player deals (design.n33.bosses33.stillArmor): the pool is not touched; a player who stays
        // within stillRadius m of one spot for stillSeconds lands only `scale` of each hit (the rest of this step's loss goes back on the bar) until he moves again. The moving pilot never meets
        // it; a camper deals half. Event bossStillArmor33 {on} when it starts and ends (the view's tell).
        double b33ArmorRadius, b33ArmorSeconds, b33ArmorScale = 1, b33ArmorStag = 1, b33ArmorNow = 1, b33Still; // b33ArmorStag: THE STAG's own scale (stillArmor.stagScale)
        Enemy b33ArmorBody;

        /// <summary>The view's read (StillArmor33View): the boss body the still armor guards now, or null when it is down.</summary>
        public Enemy Bosses33ArmorBody => bosses33Armor ? b33ArmorBody : null;
        Vec3 b33Anchor;
        double b33ArmorHp = -1;
        /// <summary>Is the still armor up now (the view's tell; the harness's count of steps).</summary>
        public bool bosses33Armor;
        public int bosses33ArmorSteps;
        public double bosses33ArmorGiven;

        Enemy Bosses33Body()
        {
            b33ArmorNow = b33ArmorScale;
            if (lantern27 != null && lantern27.Alive) return lantern27.body;
            if (stag31 != null && stag31.Alive) { b33ArmorNow = b33ArmorStag; return stag31.body; }
            if (wyrm24 != null && wyrm24.Alive) return wyrm24.body;
            if (loom30 != null && loom30.Alive) return loom30.body;
            if (crown32 != null && crown32.Alive) return crown32.body;
            return null;
        }

        void Bosses33Armor(double dt)
        {
            if (b33ArmorScale >= 1 && b33ArmorStag >= 1) return;
            Enemy body = Bosses33Body();
            // still = he stays within stillRadius m of where his stay began (a knock that he walks back from is still camping; NIGHT32's speed test missed it)
            Vec3 me = mover.position;
            if (b33Still <= 0 || JsMath.Hypot(me.x - b33Anchor.x, me.z - b33Anchor.z) > b33ArmorRadius)
            {
                b33Anchor = me;
                b33Still = dt;
            }
            else b33Still += dt;
            bool armor = body != null && b33Still >= b33ArmorSeconds;
            if (body != null && body == b33ArmorBody && armor && b33ArmorHp > body.hp && body.hp > 0)
            {
                double back = (b33ArmorHp - body.hp) * (1 - b33ArmorNow);
                body.hp = System.Math.Min(body.maxHp, body.hp + back);
                bosses33ArmorGiven += back;
            }
            if (armor) bosses33ArmorSteps++;
            if (armor != bosses33Armor)
            {
                bosses33Armor = armor;
                events.Emit("bossStillArmor33", armor);
            }
            b33ArmorBody = body;
            b33ArmorHp = body != null ? body.hp : -1;
        }

        partial void N33BossStep(double dt)
        {
            if (!n33.bosses33) return;
            Bosses33Camp();
            Bosses33Armor(dt);
            if (lantern27 != null && lantern27.Alive && lantern27.body.id != b33Lantern) { b33Lantern = lantern27.body.id; Bosses33Cut(lantern27.body, "lantern", boss32LanternId == lantern27.body.id); }
            if (stag31 != null && stag31.Alive && stag31.body.id != b33Stag) { b33Stag = stag31.body.id; Bosses33Cut(stag31.body, "stag", boss32StagId == stag31.body.id); }
            if (wyrm24 != null && wyrm24.Alive && wyrm24.body.id != b33Wyrm)
            {
                b33Wyrm = wyrm24.body.id;
                Bosses33Cut(wyrm24.body, "wyrm", boss32WyrmId == wyrm24.body.id);
                if (wyrm31 != null && wyrm31.on32) wyrm31.maxHp32 = wyrm24.body.maxHp; // the one-hit cap's pool (N32BossStep sets it again next step)
            }
            if (loom30 != null && loom30.Alive && loom30.body.id != b33Loom) { b33Loom = loom30.body.id; Bosses33Cut(loom30.body, "loom", boss32LoomId == loom30.body.id); }
            if (crown32 != null && crown32.Alive && crown32.body.id != b33Crown)
            {
                b33Crown = crown32.body.id;
                Enemy body = crown32.body;
                double basePool = cfg.Num("design.n32.boss5.pool"), pool = cfg.Has(Bosses33Root + "crown.pool") ? Bosses33Num("crown", "pool") : basePool;
                if (pool != basePool && body.hp >= body.maxHp && basePool > 0)
                {
                    body.maxHp = JsMath.Round(body.maxHp * pool / basePool);
                    body.hp = body.maxHp;
                    bosses33Cuts++;
                }
            }
        }

        /// <summary>One fight's pool: x (n33 hpMult / the NIGHT32 hpMult already applied), at full health only.</summary>
        void Bosses33Cut(Enemy body, string boss, bool n32Applied)
        {
            string k = Bosses33Root + boss + ".hpMult";
            if (!cfg.Has(k) || body.hp < body.maxHp) return;
            double m33 = Bosses33Num(boss, "hpMult"), m32 = n32Applied ? Boss32Cfg.Num(cfg, boss, "hpMult", 1) : 1;
            if (m32 <= 0 || m33 == m32) return;
            body.maxHp = JsMath.Round(body.maxHp * m33 / m32);
            body.hp = body.maxHp;
            bosses33Cuts++;
        }
    }
}
