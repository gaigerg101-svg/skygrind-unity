// NIGHT11 U3-24.9: ULTRA HEALTH, a LAB toggle (lab.ultraHealth), off by default. New design, not a web port: ULTRAKILL's
// health (NIGHT11-RESEARCH.md B.1, B.2 and C.5; https://ultrakill.wiki.gg/wiki/Health). Decision D-18 (this as the
// default) stays GG's; the LAB lets him A/B it. Numbers: Design/unity-config.json design.lab.ultraHealth (read once).
// - Hard damage: hardShare of each hit the player takes (`hurt`) goes into Health.hard (capped so hp + hard <= max);
//   Health.Heal and Health.Regen fill only to max - hard. A hit sets the timer to timerBase when it isn't running, and
//   every hit adds damage / timerDivisor s. Once the timer has run out, hard damage drains at drainByRank[rank] HP/s;
//   the timer itself runs timerRateByRank[rank] x as fast; from wipeRank it is wiped at once.
// - The blood heal: each player `hit` within bloodRange m of the player (the player's distance to HitEvent.position)
//   heals blood[weapon] (bloodHead[weapon] on a headshot); an impact wave's share (HitEvent.shock) heals nothing.
// - The parry (`parry`, the player's) heals to the cap (max - hard), after the Loadout's own parry heal.
// With `enabled` false (the game sets it from the run's LAB toggle at the run's start, Lab.Apply) every handler and Step
// return at once and Health.hard stays 0: the web's health. Step allocates nothing.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class UltraHealth : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the switch (the run's LAB toggle), the hard damage's timer, the step length,
        // the blood healed (the hard damage itself is Health's).
        public void Save(StateWriter w)
        {
            w.B("enabled", enabled);
            w.I("timerSteps", timerSteps);
            w.D("dt", dt);
            w.D("bloodHealed", bloodHealed);
        }

        public void Load(StateReader r)
        {
            enabled = r.B("enabled");
            timerSteps = r.Int("timerSteps");
            dt = r.D("dt");
            bloodHealed = r.D("bloodHealed");
        }

        /// <summary>design.lab.ultraHealth, read once.</summary>
        public sealed class P
        {
            public double hardShare, timerBase, timerDivisor, bloodRange;
            public double[] drainByRank, timerRateByRank;
            public int wipeRank;
            public readonly Dictionary<string, double> blood = new Dictionary<string, double>(), bloodHead = new Dictionary<string, double>();

            public static P From(GameConfig c)
            {
                var p = new P
                {
                    hardShare = c.Num("design.lab.ultraHealth.hardShare"), timerBase = c.Num("design.lab.ultraHealth.timerBase"),
                    timerDivisor = c.Num("design.lab.ultraHealth.timerDivisor"), bloodRange = c.Num("design.lab.ultraHealth.bloodRange"),
                    drainByRank = c.Nums("design.lab.ultraHealth.drainByRank"), timerRateByRank = c.Nums("design.lab.ultraHealth.timerRateByRank"),
                    wipeRank = c.Int("design.lab.ultraHealth.wipeRank"),
                };
                foreach (var kv in c.Obj("design.lab.ultraHealth.blood")) p.blood[kv.Key] = PortData.Num(kv.Value);
                foreach (var kv in c.Obj("design.lab.ultraHealth.bloodHead")) p.bloodHead[kv.Key] = PortData.Num(kv.Value);
                return p;
            }
        }

        /// <summary>The blood heal of one hit by `weapon` (the HitEvent's weapon id): bloodHead on a headshot when listed, else blood, else 0.</summary>
        public static double BloodFor(string weapon, bool headshot, P p)
        {
            if (weapon == null) return 0;
            if (headshot && p.bloodHead.TryGetValue(weapon, out double head)) return head;
            return p.blood.TryGetValue(weapon, out double v) ? v : 0;
        }

        /// <summary>The rank's entry of a by-rank table (the last entry past its end; rank 0 under 0).</summary>
        static double ByRank(double[] table, int rank) => table[Math.Min(Math.Max(0, rank), table.Length - 1)];

        public readonly P p;
        readonly Health health;
        readonly Func<Vec3> playerPos;
        readonly Func<int> rank;
        readonly string owner;
        double dt;
        int timerSteps;

        /// <summary>The run's LAB toggle (Lab.Apply sets it at a run's start). False: the web's health.</summary>
        public bool enabled;

        /// <summary>Blood heals this run (HP restored by hits), for the HUD / a check.</summary>
        public double bloodHealed;

        public UltraHealth(GameConfig cfg, EventBus events, Health health, Func<Vec3> playerPos, Func<int> rank, string owner = "player")
        {
            p = P.From(cfg);
            this.health = health;
            this.playerPos = playerPos;
            this.rank = rank ?? (() => 0);
            this.owner = owner;
            dt = cfg.Dt;
            events.On<HurtEvent>("hurt", OnHurt);
            events.On<HitEvent>("hit", OnHit);
            events.On<HitEvent>("parry", OnParry);
        }

        /// <summary>Hard damage now (Health.hard).</summary>
        public double Hard => health.hard;

        /// <summary>What heals fill to now (max - hard): the HP bar's grey cap.</summary>
        public double Cap => health.Cap;

        /// <summary>Seconds before hard damage starts to drain (0: draining or none), at the base rate.</summary>
        public double TimerLeft => timerSteps * dt;

        /// <summary>A new run (or the toggle changed): no hard damage, no timer.</summary>
        public void Reset()
        {
            health.hard = 0;
            timerSteps = 0;
            bloodHealed = 0;
        }

        void OnHurt(HurtEvent e)
        {
            if (!enabled || e.owner != owner || e.amount <= 0) return;
            // The hit's share, never past what hp left room for (hp + hard <= max).
            health.hard = Math.Min(health.hard + p.hardShare * e.amount, Math.Max(0, health.max - health.hp));
            double seconds = (timerSteps > 0 ? timerSteps * dt : p.timerBase) + e.amount / p.timerDivisor;
            timerSteps = MovementMath.StepsFor(seconds, dt);
        }

        void OnHit(HitEvent e)
        {
            if (!enabled || e.owner != owner || e.shock || health.Dead) return;
            double amount = BloodFor(e.weapon, e.headshot, p);
            if (amount <= 0) return;
            Vec3 at = playerPos();
            if (JsMath.Hypot(e.position.x - at.x, e.position.y - at.y, e.position.z - at.z) > p.bloodRange) return;
            bloodHealed += health.Heal(amount);
        }

        void OnParry(HitEvent e)
        {
            if (!enabled || e.owner != owner || health.Dead) return;
            health.Heal(health.Cap - health.hp); // to the cap (Heal does nothing at or over it)
        }

        /// <summary>One fixed step: the timer, then the drain (or the wipe at wipeRank). Allocates nothing.</summary>
        public void Step(double dt)
        {
            this.dt = dt;
            if (!enabled || health.hard <= 0)
            {
                timerSteps = 0;
                return;
            }
            int r = rank();
            if (r >= p.wipeRank)
            {
                health.hard = 0;
                timerSteps = 0;
                return;
            }
            if (timerSteps > 0)
            {
                timerSteps = Math.Max(0, timerSteps - (int)ByRank(p.timerRateByRank, r));
                return;
            }
            health.hard = Math.Max(0, health.hard - ByRank(p.drainByRank, r) * dt);
        }
    }
}
