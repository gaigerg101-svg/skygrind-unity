// NIGHT32 builder B (design.n32.bosses32; DESIGN32 B): every boss dies in about a minute. Implements the lead's declaration-only partials N32BossStartRun / N32BossStep
// (SurvivalSim.N32.cs). GG 2026-10-10: "the bosses have way too much health ... sometimes they can't even take damage in areas ... these bosses you should defeat in like a
// minute ... you can barely hit them".
// While the row is on (n32.bosses32, a run's bit) each NIGHT31 boss system takes the row's numbers (Lantern31 / Stag31 / Wyrm31 / Loom31 .Set32: every hit lands at least a
// chip share, the identity windows are a x2.5 bonus that lasts at least the PULSE's cooldown + 0.75 s), and each new fight's pool is cut by the row's hpMult (after NIGHT29's
// cut, which runs earlier in the step). Off (--off32, a NIGHT31 tape): nothing here writes the sim; the systems get NIGHT31's numbers back if a run had them on.
// boss32Stats is read only (the harness's boss32 table: hits, full-share hits, steps the boss could not be hurt).
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>NIGHT32 B: what the harness's boss32 sweep reads from a fight (counted, never written into the sim).</summary>
    public sealed class Boss32Stats
    {
        public int hits, fullHits, fightSteps, zeroSteps, paired, lowHits;
        public double dealt, pairedRaw, pairedLanded;
        internal double pendingLanded = -1;

        /// <summary>The mean landed share of a hit (bossHit's landed, rounded by the boss, over the hit's dealt damage before the boss's filter).</summary>
        public double Share => pairedRaw > 0 ? pairedLanded / pairedRaw : 0;

        public void Clear()
        {
            hits = fullHits = fightSteps = zeroSteps = paired = lowHits = 0;
            dealt = pairedRaw = pairedLanded = 0;
            pendingLanded = -1;
        }
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT32 B: the harness's probe (a sim starting its run hands itself over; null in the game).</summary>
        public static System.Action<SurvivalSim> Boss32Probe;

        public readonly Boss32Stats boss32Stats = new Boss32Stats();
        bool boss32Listening, boss32WasLive;
        int boss32LastFull, boss32LanternId = -1, boss32StagId = -1, boss32WyrmId = -1, boss32LoomId = -1;

        partial void N32BossStartRun()
        {
            boss32Stats.Clear();
            boss32LanternId = boss32StagId = boss32WyrmId = boss32LoomId = -1;
            boss32WasLive = false;
            boss32LanternRaw = double.MaxValue;
            boss32LanternCarry = boss32LanternCarried = 0;
            if (!boss32Listening)
            {
                boss32Listening = true;
                events.On<BossEvent>("bossHit", e =>
                {
                    boss32Stats.hits++;
                    boss32Stats.dealt += e.damage;
                    int full = Boss32FullCount();
                    if (full > boss32LastFull) boss32Stats.fullHits++;
                    boss32LastFull = full;
                    boss32Stats.pendingLanded = e.damage;
                    // THE LANTERN's phase clamp (Lantern27.Apply emits bossHit BEFORE it clamps the bar at phase2At / phase3At): the bar as the hit left it
                    if (e.boss == "lantern" && lantern27 != null) boss32LanternRaw = System.Math.Min(boss32LanternRaw, lantern27.body.hp);
                });
                // the hit's own event comes after the boss's (Combat: filter, Damage -> bossHit, then "hit" with the damage before the filter)
                events.On<HitEvent>("hit", e =>
                {
                    if (boss32Stats.pendingLanded < 0 || e.damage <= 0) return;
                    double landed = boss32Stats.pendingLanded;
                    boss32Stats.pendingLanded = -1;
                    boss32Stats.paired++;
                    boss32Stats.pairedRaw += e.damage;
                    boss32Stats.pairedLanded += landed;
                    if (landed / e.damage < 0.245) boss32Stats.lowHits++;
                });
            }
            boss32LastFull = Boss32FullCount();
            Boss32Probe?.Invoke(this);
        }

        /// <summary>The boss systems' full-share hit counters summed (each counts a hit that landed its full or bonus share; some reset per fight).</summary>
        int Boss32FullCount()
        {
            int n = 0;
            if (lantern31 != null) n += lantern31.openHits;
            if (stag31 != null) n += stag31.on32 ? stag31.full32 : stag31.backPunches + stag31.parryHits;
            if (wyrm31 != null) n += wyrm31.fullHits;
            if (loom30 != null) n += loom30.fullHits;
            return n;
        }

        partial void N32BossStep(double dt)
        {
            bool row = n32.bosses32;
            // each boss also has its own switch (design.n32.bosses32.<boss>.enabled): one boss can go back to NIGHT31's rule without the others
            bool onL = row && cfg.Design("n32.bosses32.lantern.enabled"), onS = row && cfg.Design("n32.bosses32.stag.enabled");
            bool onW = row && cfg.Design("n32.bosses32.wyrm.enabled"), onM = row && cfg.Design("n32.bosses32.loom.enabled");
            // the systems' numbers (a system is made at its first fight: one step after, inside the unhurtable intro)
            if (lantern31 != null && lantern31.on32 != onL) lantern31.Set32(cfg, onL);
            if (stag31 != null && stag31.on32 != onS) stag31.Set32(cfg, onS);
            if (wyrm31 != null && wyrm31.on32 != onW)
            {
                wyrm31.Set32(cfg, onW);
                if (wyrm31Hooked != null) wyrm31Hooked.aggroCool31 = wyrm31.p.aggroCooldown;
            }
            if (loom31 != null && loom31.on32 != onM) loom31.Set32(cfg, onM, loom31Hooked);
            if (onW && wyrm31 != null && wyrm24 != null) wyrm31.maxHp32 = wyrm24.body.maxHp; // the one-hit cap's pool
            Boss32LoomJoints(onM);
            // the Lantern's finisher: Lantern27 cuts guns to finisherGunMult; the row's share instead (level 1 has no PULSE)
            if (onL && lantern31 != null && lantern27 != null)
            {
                double fm = lantern27.p.finisherGunMult;
                lantern31.finisherComp32 = lantern27.Finisher && fm > 0 ? Boss32Cfg.Num(cfg, "lantern", "finisherGun", fm) / fm : 1;
            }
            // each new fight's pool (NIGHT29's cut ran earlier this step; the intro is unhurtable)
            if (onL && n31.lantern31 && lantern27 != null && lantern27.Alive && lantern27.body.id != boss32LanternId) { boss32LanternId = lantern27.body.id; Boss32Pool(lantern27.body, "lantern"); boss32LanternCarry = 0; boss32LanternRaw = double.MaxValue; }
            if (onL) Boss32LanternCarry();
            if (onL && lantern31 != null && lantern27 != null) lantern31.Camp32(mover.position, mover.velocity, health, dt, lantern27.Alive && lantern27.Hurtable); // the anti-camp strike
            if (onS && n31.stag31 && stag31 != null && stag31.Alive && stag31.body.id != boss32StagId) { boss32StagId = stag31.body.id; Boss32Pool(stag31.body, "stag"); }
            if (onW && n31.wyrm31 && wyrm24 != null && wyrm24.Alive && wyrm24.body.id != boss32WyrmId) { boss32WyrmId = wyrm24.body.id; Boss32Pool(wyrm24.body, "wyrm"); }
            if (onM && n31.loom31 && loom30 != null && loom30.Alive && loom30.body.id != boss32LoomId) { boss32LoomId = loom30.body.id; Boss32Pool(loom30.body, "loom"); }
            Boss32Count();
        }

        Loom30 boss32JointLoom;
        double boss32JointOrig;

        /// <summary>THE LOOM's joint hit spheres (DESIGN32 B "hurtboxes at least 1.3x the model"; the lead 20:22: grow the sphere, not the drawing): while the row is on each Loom's
        /// p.jointRadius is the row's jointRadius (NIGHT31: design.n30.span.loom.jointRadius 1 m; Boss30View draws the joint 2 x radius x 0.8); NIGHT31's back when off.</summary>
        void Boss32LoomJoints(bool on)
        {
            if (boss32JointLoom != null && (boss32JointLoom != loom30 || !on)) { boss32JointLoom.p.jointRadius = boss32JointOrig; boss32JointLoom = null; }
            if (!on || loom30 == null || boss32JointLoom == loom30) return;
            boss32JointLoom = loom30;
            boss32JointOrig = loom30.p.jointRadius;
            loom30.p.jointRadius = Boss32Cfg.Num(cfg, "loom", "jointRadius", boss32JointOrig);
        }

        double boss32LanternRaw = double.MaxValue, boss32LanternCarry;
        /// <summary>The harness's count: the bar carried over the Lantern's phase clamps this run.</summary>
        public double boss32LanternCarried;

        /// <summary>THE LANTERN's 66 % / 33 % clamps keep their beat but no longer eat the rest of the hit (A1: Lantern27.cs:678-679; GG "too much health"): the part of a hit the
        /// clamp put back is carried and taken off when the next phase can be hurt, never below the next clamp (its beat still comes) and never to 0.</summary>
        void Boss32LanternCarry()
        {
            Lantern27 l = lantern27;
            if (l == null || !l.Alive) { boss32LanternCarry = 0; boss32LanternRaw = double.MaxValue; return; }
            if (boss32LanternRaw < double.MaxValue)
            {
                if (l.body.hp > boss32LanternRaw + 1e-9) boss32LanternCarry += l.body.hp - System.Math.Max(0, boss32LanternRaw);
                boss32LanternRaw = double.MaxValue;
            }
            if (boss32LanternCarry <= 0 || !l.Hurtable || l.phase == "beat") return;
            double floor = l.phase == "p2" ? l.body.maxHp * l.p.phase3At + 1 : 1;
            double to = System.Math.Max(floor, l.body.hp - boss32LanternCarry);
            if (to < l.body.hp) { boss32LanternCarried += l.body.hp - to; l.body.hp = to; }
            boss32LanternCarry = 0;
        }

        void Boss32Pool(Enemy body, string boss)
        {            double m = Boss32Cfg.Num(cfg, boss, "hpMult", 1);
            if (m == 1.0 || body.hp < body.maxHp) return;
            body.maxHp = JsMath.Round(body.maxHp * m);
            body.hp = body.maxHp;
        }

        /// <summary>The harness's counts: a live fight's steps (from its first hurtable step) and those in which no hit could land (not hurtable; NIGHT31's Lantern platform share 0).</summary>
        void Boss32Count()
        {
            bool alive, hurt;
            if (loom30 != null && loom30.Alive) { alive = true; hurt = loom30.Hurtable; }
            else if (stag31 != null && stag31.Alive) { alive = true; hurt = stag31.Hurtable; }
            else if (wyrm24 != null && wyrm24.Alive) { alive = true; hurt = wyrm24.phase != "intro" && wyrm24.phase != "beat" && wyrm24.phase != "dying"; }
            else if (lantern27 != null && lantern27.Alive)
            {
                alive = true;
                hurt = lantern27.Hurtable;
                if (hurt && lantern30 != null && lantern31 != null && !lantern31.on32 && lantern30.PlayerOnPlatform() && lantern31.sinkState < 2 && lantern30.p.platformScale <= 0) hurt = false;
            }
            else { alive = false; hurt = false; }
            if (!alive) { boss32WasLive = false; return; }
            if (hurt) boss32WasLive = true;
            if (!boss32WasLive) return;
            boss32Stats.fightSteps++;
            if (!hurt) boss32Stats.zeroSteps++;
        }
    }
}
