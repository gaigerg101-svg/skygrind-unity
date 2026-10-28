// NIGHT43 builder M1 (design.n43.boost43, N43Blocks bit 2; GG 2026-10-16, G26 in Research/2026-10-16-wave43/GG-REQUEST.md; CODE-FACTS-B G26;
// research/movement-web.md C13-C15 (TF2: a down-shot just before the takeoff is treated as airborne)): jump + THUNDERCLAP shot combine, high and
// forgiving. Today (boost42 off) the shot boosts only when the mover is airborne at the shot (velocity -= aim x 10); charge40's ground rule holds a
// ground press until its release, so a press a moment before the jump is a ground shot (no boost) or, held past 0.15 s, a slug charge; and a tap
// jump's release halves a boost that came after the takeoff (jump.releaseMult 0.5: 18.3 -> ~9.2 m/s).
// With the row on (Loadout.boost43, set by the sim at a run's start: SurvivalSim.N43.Move.cs):
// (a) the combine window, both orders:
//     - a ground THUNDERCLAP hold (charge40's ground rule) still inside its tap split when the player takes off (airborne and rising) fires its
//       volley on that step, in the air: a boost (Loadout.C40Ground, "NIGHT43 boost43");
//     - a ground volley aimed below level, fired up to `window` s before the takeoff, boosts on the takeoff's step (the shot is not re-fired:
//       the push is applied late, Loadout.Boost43Step);
//     - a shot after the takeoff is an air shot (today's rule) and boosts in full (below).
// (b) every boost below level is a launch: the jump's variable-height cut is disarmed (Mover.Boost42Launch) on every path (the volley, the tap
//     on the press, the slug, the late push).
// (c) the push is boost42's shape with this row's numbers: `lift` m/s at `fullDeg` below level or steeper (scaled by sin(pitch) / sin(fullDeg)
//     above it), a fall cut to -fallFloor first, the web's horizontal push. Measured choice in M1.md (n43move boost).
// The row off (or --off43, or a tape of format 25 or older): Params.Neutral; Boost42 / the web's line bit for bit.
namespace Skygrind.Core
{
    public static class Boost43
    {
        public struct Params
        {
            public bool on;
            /// <summary>The combine window in whole steps.</summary>
            public int windowSteps;
            public double window;
            /// <summary>The push (boost42's shape, this row's numbers; on = true).</summary>
            public Boost42.Params push;

            public static Params Neutral => default;

            public static Params From(GameConfig cfg, bool on)
            {
                if (!on) return Neutral;
                const string r = "design.n43.boost43.";
                double full = cfg.Num(r + "fullDeg");
                var p = new Params
                {
                    on = true,
                    window = cfg.Num(r + "window"),
                    push = new Boost42.Params
                    {
                        on = true,
                        lift = cfg.Num(r + "lift"),
                        fullDeg = full,
                        fallFloor = cfg.Num(r + "fallFloor"),
                        sinFull = JsMath.Sin(full * System.Math.PI / 180),
                    },
                };
                p.windowSteps = MovementMath.StepsFor(p.window, 1 / cfg.movement.loop.simHz);
                return p;
            }
        }

        /// <summary>The run's combine state (Loadout.b43): a ground volley waiting for a takeoff (its aim, its age in steps) and the run's counts:
        /// late pushes (a ground volley then the takeoff), holds fired at the takeoff, air boosts (every path), ground volleys that expired.</summary>
        public sealed class State
        {
            public bool pending;
            public Vec3 aim;
            public int age;
            public int latePushes, holdFires, airBoosts, expired;

            public void Reset()
            {
                pending = false;
                age = 0;
                latePushes = holdFires = airBoosts = expired = 0;
            }
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT43 boost43 (the sim sets it at a run's start; Neutral: boost42 / the web's shotgun jump).</summary>
        public Boost43.Params boost43;

        /// <summary>NIGHT43 boost43's combine state (reset by the sim at a run's start).</summary>
        public readonly Boost43.State b43 = new Boost43.State();

        /// <summary>The three THUNDERCLAP shotgun-jump lines (the volley, the sandbox volley, the slug): airborne, the shotgun jump; on the ground with
        /// boost43 on, a volley aimed below level waits `window` s for a takeoff (Boost43Step). Off: exactly `if (!mover.grounded) ShotgunJump42(aim)`.</summary>
        void ShotgunJump43(Vec3 aim)
        {
            if (!mover.grounded) ShotgunJump42(aim);
            else if (boost43.on && aim.y < 0)
            {
                b43.pending = true;
                b43.aim = aim;
                b43.age = 0;
            }
        }

        /// <summary>NIGHT43 boost43 (the sim, every step after the loadout's, only with the row on): a ground volley still inside the window when the
        /// player has taken off (airborne and rising) pushes now; past the window it expires.</summary>
        public void Boost43Step()
        {
            if (!boost43.on || !b43.pending) return;
            if (!mover.grounded && mover.velocity.y > 0)
            {
                b43.pending = false;
                b43.latePushes++;
                ShotgunJump42(b43.aim);
                return;
            }
            if (++b43.age > boost43.windowSteps)
            {
                b43.pending = false;
                b43.expired++;
            }
        }

        /// <summary>NIGHT43 boost43 (a), inside charge40's ground rule (Loadout.C40Ground): a ground hold of gun i not yet a charge, the player now
        /// airborne and rising (a takeoff inside the tap split): the hold ends as its tap, fired this step in the air (a boost). True when it did.</summary>
        bool Boost43TakeoffFire(Charge35 q, int i)
        {
            if (!boost43.on || !q.holding || q.gun != i || !q.c40Ground || q.started || mover.grounded || mover.velocity.y <= 0) return false;
            if (q.steps > boost43.windowSteps) return false;
            int age = q.steps;
            q.Clear();
            charge27.ClearHold();
            q.tapQueued = true;
            q.tapGun = i;
            q.tapAge39 = age;
            q.releaseTaps39++;
            q.taps++;
            b43.holdFires++;
            return true;
        }
    }
}
