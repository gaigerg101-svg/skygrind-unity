// NIGHT29 builder B (boss1 / boss2 / bossHp): THE LANTERN's floor cycle and light-damage rule (Core/Enemies/Lantern29.cs) and THE GLACIER's other-gun rule, skate lane, attacks and patience gauge
// (Core/Enemies/Glacier29.cs, Core/SkateLane29.cs) in the sim. Implements the lead's declaration-only partials N29BossStartRun / N29BossStep (SurvivalSim.N29.cs). Every effect is gated on
// n29.boss1 / n29.boss2 (the run's blocks); with them off nothing here runs and the NIGHT28 / NIGHT27 bosses are untouched. The boss HP cuts (design.n29.bossHp) ride on the same two blocks:
// the lantern's on boss1, the glacier's on boss2, the worm's on either. No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Lantern29 lantern29;
        public Glacier29 glacier29;
        int lantern29CutId = -1, glacier29CutId = -1, worm29CutId = -1;

        /// <summary>The world's time share while THE LANTERN's floor is live (1 = normal): the lead's SurvivalGame view reads it (slowScale 0.55, eased over slowEaseS).</summary>
        public double Lantern29SlowScale => n29.boss1 && lantern29 != null ? lantern29.SlowScale : 1.0;
        /// <summary>The gap to THE GLACIER in metres along its flee direction (the HUD's gap bar), 0 outside the fight.</summary>
        public double Glacier29Gap => n29.boss2 && glacier29 != null && glacier28 != null && glacier28.Alive ? glacier28.gap : 0;

        double BossHpMult(string key) => n29.boss1 || n29.boss2 ? cfg.Design("n29.bossHp.enabled") ? cfg.Num("design.n29.bossHp." + key) : 1.0 : 1.0;

        partial void N29BossStartRun()
        {
            lantern29CutId = glacier29CutId = worm29CutId = -1;
            lantern29?.Reset();
            glacier29?.Reset();
            if (lantern27 != null) lantern27.rule29 = null;
            if (glacier28 != null) glacier28.rule29 = null;
            if (lantern29 != null && n29.boss1 && lantern27 != null) lantern27.rule29 = lantern29;
            if (glacier29 != null && n29.boss2 && glacier28 != null) { glacier28.rule29 = glacier29; glacier29.NewRun(runSeed); }
        }

        partial void N29BossStep(double dt)
        {
            Lantern29Step(dt);
            Glacier29Step(dt);
            Worm29Cut();
        }

        void Lantern29Step(double dt)
        {
            if (lantern27 == null) return;
            if (!n29.boss1)
            {
                lantern27.rule29 = null;
                return;
            }
            if (lantern29 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                lantern29 = new Lantern29(cfg, lantern27, events, target);
                lantern29.playerRiding = () => mover.state == MoveState.WallRide;
            }
            lantern27.rule29 = lantern29;
            if (!lantern27.Alive) { lantern29.Step(dt); return; }
            // the HP cut: once per fight (a fight is a new body id), before any hit can land (the intro is unhurtable)
            if (lantern27.body.id != lantern29CutId)
            {
                lantern29CutId = lantern27.body.id;
                double m = BossHpMult("lanternMult");
                if (m != 1.0)
                {
                    lantern27.body.maxHp = JsMath.Round(lantern27.body.maxHp * m);
                    lantern27.body.hp = lantern27.body.maxHp;
                }
                lantern29.Reset();
            }
            lantern29.Step(dt);
        }

        void Glacier29Step(double dt)
        {
            if (glacier28 == null) return;
            if (!n29.boss2)
            {
                glacier28.rule29 = null;
                glacier28.bossLat29 = 0;
                glacier28.speedMult29 = 1;
                glacier28.tellMin29 = 0;
                return;
            }
            if (glacier29 == null)
            {
                glacier29 = new Glacier29(cfg, glacier28, events);
                glacier29.NewRun(runSeed);
            }
            glacier28.rule29 = glacier29;
            if (!glacier28.Alive) { glacier29.Idle(); return; }
            if (glacier28.body.id != glacier29CutId)
            {
                glacier29CutId = glacier28.body.id;
                double m = BossHpMult("glacierMult");
                if (m != 1.0)
                {
                    glacier28.body.maxHp = JsMath.Round(glacier28.body.maxHp * m);
                    glacier28.body.hp = glacier28.body.maxHp;
                }
                glacier29.Reset();
            }
            glacier29.Step(dt, mover, glacier28Surf != null);
        }

        /// <summary>The worm's HP cut (design.n29.bossHp.wormMult), once per fight (a fight is a new body id): the pool x mult before any hit can land (the intro is unhurtable). On with either boss block.</summary>
        void Worm29Cut()
        {
            if (!(n29.boss1 || n29.boss2) || wyrm24 == null || !wyrm24.Alive) return;
            if (wyrm24.body.id == worm29CutId) return;
            worm29CutId = wyrm24.body.id;
            double m = BossHpMult("wormMult");
            if (m == 1.0) return;
            wyrm24.body.maxHp = JsMath.Round(wyrm24.body.maxHp * m);
            wyrm24.body.hp = wyrm24.body.maxHp;
        }
    }
}
