// NIGHT42 builder M1 (design.n42.boost42, N42Blocks bit 1; GG 2026-10-14 ~18:35, G5: "shoutgun jumping is too difficult and needs to be eaiser
// to boost"; CODE-FACTS G5): the THUNDERCLAP's air boost made easy.
// Today (Combat.cs FireShotgun, Sandbox19.cs FireShotgun19, ChargeShots27.cs's slug): an air shot adds -aim x weapons.shotgun.jumpSpeed (10) to
// the velocity: straight down +10 m/s up, 45 deg down only +7.07, and a falling player keeps his fall (at -8 m/s a 45 deg shot nets ~ -1).
// With the row on (Loadout.boost42, set by the sim at a run's start: SurvivalSim.N42.Move.cs), for a shot aimed below level:
// - the push along the ground is the web's (-aim x jumpSpeed, its horizontal part);
// - the lift is `lift` m/s at fullDeg below level or steeper, scaled by sin(pitch) / sin(fullDeg) above it (a shallow shot still lifts some);
// - a fall is cut first: the vertical speed before the lift is at least -fallFloor (a shot late in a fall still climbs);
// - the boost is a launch: letting go of jump after it no longer halves the climb (Mover.Boost42Launch disarms the jump's variable-height cut,
//   jump.releaseMult 0.5, as a wall jump or a pad does). Measured before (n42move boost --trace): a jump held until a straight-down shot and
//   released just after it: the up speed 16.8 m/s halved to 8.4 (the shot's +10 netted +1.6).
// A shot at or above level is the web's push. The row off (or --off42 / -skygrindOff42, or a tape of format 24 or older): Params.Neutral, the
// web's line bit for bit.
namespace Skygrind.Core
{
    public static class Boost42
    {
        public struct Params
        {
            public bool on;
            public double lift, fullDeg, fallFloor;
            /// <summary>sin(fullDeg), computed once (JsMath).</summary>
            public double sinFull;

            /// <summary>The web's shotgun jump.</summary>
            public static Params Neutral => default;

            /// <summary>The row's numbers (design.n42.boost42) when `on`; Neutral when off.</summary>
            public static Params From(GameConfig cfg, bool on)
            {
                if (!on) return Neutral;
                const string r = "design.n42.boost42.";
                double full = cfg.Num(r + "fullDeg");
                return new Params
                {
                    on = true,
                    lift = cfg.Num(r + "lift"),
                    fullDeg = full,
                    fallFloor = cfg.Num(r + "fallFloor"),
                    sinFull = JsMath.Sin(full * System.Math.PI / 180),
                };
            }
        }

        /// <summary>The velocity after an air shot aimed along `aim` (unit) with the gun's jump speed `jump`, under the row's rules.</summary>
        public static Vec3 Push(Vec3 v, Vec3 aim, double jump, in Params p)
        {
            double down = -aim.y; // sin of the pitch below level
            if (!p.on || down <= 0 || p.sinFull <= 0) return v - aim * jump;
            double k = down >= p.sinFull ? 1 : down / p.sinFull;
            double vy = v.y < -p.fallFloor ? -p.fallFloor : v.y;
            return new Vec3(v.x - aim.x * jump, vy + p.lift * k, v.z - aim.z * jump);
        }

        /// <summary>The lift a shot at `pitchDeg` below level gives a player at rest (the harness's table).</summary>
        public static double LiftAt(double pitchDeg, double jump, in Params p)
        {
            double rad = pitchDeg * System.Math.PI / 180;
            Vec3 aim = new Vec3(0, -JsMath.Sin(rad), -JsMath.Cos(rad));
            return Push(Vec3.Zero, aim, jump, p).y;
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT42 boost42 (the sim sets it at a run's start; Neutral: the web's shotgun jump).</summary>
        public Boost42.Params boost42;

        /// <summary>The shotgun jump of an air shot along `aim` (the three THUNDERCLAP lines call it, airborne only).</summary>
        void ShotgunJump42(Vec3 aim)
        {
            if (boost43.on) // NIGHT43 boost43 (Core/Boost43.cs): boost42's shape with boost43's numbers; every boost below level is a launch
            {
                mover.velocity = Boost42.Push(mover.velocity, aim, sgJump, boost43.push);
                if (aim.y < 0)
                {
                    mover.Boost42Launch();
                    b43.airBoosts++;
                }
            }
            else if (boost42.on)
            {
                mover.velocity = Boost42.Push(mover.velocity, aim, sgJump, boost42);
                if (aim.y < 0) mover.Boost42Launch(); // a boost below level is a launch: releasing jump after it does not halve it
            }
            else mover.velocity = mover.velocity - aim * sgJump;
        }
    }
}
