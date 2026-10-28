// NIGHT56 builder G (P1-wickWindow, wick55's Player half; the r1 critic: "give the Player pilot a core punch inside an OPEN window (bfon's pilot
// prints 'shots 0, rides 0, punches 27' and never lands one)"). a155 FIGHT MODE ran Wick37Pilot bare (Wick44ProofInput); the bot numbers it was
// gated against (n47human wick: 57 % dealt in windows, 57.5 s) come from Wick37Pilot INSIDE n47human's human model, whose trigger habit fires
// the KESTREL (Harness.N47.Human.cs: "the human fires consistently"): the bare pilot only punches, deals nothing, THE WICK never leaves P1, its
// core never opens, so no core punch can land. This wrapper adds that one habit, unchanged, to the reel's pilot: when the pilot has left the
// trigger alone for Wick56TriggerIdleSteps steps (no fire, no alt, no swap), is not punching, holds a gun (not the gauntlet) and THE WICK's
// centre is within Wick56TriggerConeDeg of the look, a click (fire pressed + held for one step: the KESTREL's quick tap) every
// Wick56TriggerEverySteps steps (the harness's 30 / 15 / 20 deg, below: a bot's own constants, as Wick37Pilot's; the harness compiles Bot/).
// Movement, look and punches stay the pilot's. A reel input only (no game rule); no allocation per step.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Wick56FightInput : IPlayerInput
    {
        /// <summary>n47human's human model's trigger habit, unchanged (Harness.N47.Human.cs H47Human: the bot numbers wick55 was gated on): a click
        /// when the pilot left the trigger alone this many steps (30 = 0.25 s at 120 Hz), every this many steps (15: 8 Hz), with THE WICK's centre
        /// inside this cone round the look (20 deg).</summary>
        public const int Wick56TriggerIdleSteps = 30, Wick56TriggerEverySteps = 15;
        public const double Wick56TriggerConeDeg = 20;

        readonly SurvivalSim sim;
        readonly IPlayerInput inner;
        int lastPilotTrigger = -100000;
        /// <summary>Clicks this habit added (the reel prints them).</summary>
        public int clicks;

        public Wick56FightInput(SurvivalSim sim, IPlayerInput inner)
        {
            this.sim = sim;
            this.inner = inner;
        }

        public double Yaw => inner.Yaw;
        public double Pitch => inner.Pitch;
        public void SetLook(double y, double p) => inner.SetLook(y, p);
        public bool Captured => inner.Captured;
        public InputFrame SampleMove(bool active) => inner.SampleMove(active);

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = inner.SampleCombat(active);
            Wick37 w = sim.wick37;
            if (w == null || !w.Alive) return c;
            int step = sim.simStep;
            bool punching = c.punch.held || c.punch.pressed;
            if (c.fire.held || c.fire.pressed || c.altFire.held || c.weapon >= 0) lastPilotTrigger = step;
            if (step - lastPilotTrigger <= Wick56TriggerIdleSteps || punching || sim.loadout.Weapon == "gauntlet" || step % Wick56TriggerEverySteps != 0)
                return c;
            (Vec3 eye, Vec3 look) = sim.Aim();
            Vec3 at = w.Centre;
            double dx = at.x - eye.x, dy = at.y - eye.y, dz = at.z - eye.z, len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (len < 1e-6 || (dx * look.x + dy * look.y + dz * look.z) / len < Math.Cos(Wick56TriggerConeDeg * Math.PI / 180)) return c;
            c.fire = new Button { pressed = true, held = true };
            clicks++;
            return c;
        }
    }
}
