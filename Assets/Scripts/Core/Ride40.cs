// NIGHT40 builder R2 (design.n40.ride40; NIGHT40.md R2, DECIDE40 D-9; GG 2026-10-13 ~22:00, Research/2026-10-14-wave40/GG-REQUEST.md asks 11, 12,
// 17: "Infinite wall-riding should also be implemented so players don't slowly slide down after a few seconds; the wall-ride must feel crisp,
// smooth, and reliable" and "Wall-riding currently triggers too easily when climbing random platforms or stairs"). The Mover reads it through
// Mover.flow34.ride35.r40 (inside the NIGHT35 ride state, so the run snapshot saves it whole); neutral by default: with Params.Neutral (on = false:
// the golden vectors', the replays' and every row-off run's config) every Mover.cs branch that reads it is skipped, so NIGHT39's ride bit for bit
// (CLAUDE.md rule 3; the branches sit inside the NIGHT35 wallride35 ones, NIGHT40.md R2).
// THE RIDE HOLDS (round 1): ride38's hold rule (Core/Ride38.cs: no sink, the look pitch past +-deadzoneDeg drifts the ride up / down up to driftMax,
// never past the wall's top, only a long guard against a stuck state) for EVERY ride, a tap ride (jump not held at the entry) as well as a hold
// ride. A ride then ends only by the player (a jump: a tap ride's press is the web's wall jump, a hold ride's release the jump-off; a crouch
// drop; the stick away from the wall), by the wall (it ends: "lost" after wallride35's lostGrace, a corner past followMaxDeg, a noride face) or
// by a landing. A tap ride also takes the hold ride's checks (wallride35's corners and curved walls, its lost grace), so the two rides end the
// same way. The hold rule's numbers are design.n40.ride40's (Hold), each falling back to design.n38.ride38's where the row lacks it; the n38 row
// is never edited (`--off40`: NIGHT39's game).
// THE ATTACH ASKS FOR INTENT (round 2; Mover.Ride40Intent, called from wallride35's Ride35Entry and Ride35Attach): a ride starts only with
// the ground at least minClearance below the feet, minSpeed of horizontal speed, the stick toward the wall (inputDot), the wall's face
// still there minWallHeight above the feet, and not on the wall a ride just left (lockout); the near-miss draw reaches attachReach. A jump
// held at a ride wall still rides (once the jump has lifted the feet minClearance); risers, platform faces and crates do not.
// Whole steps (MovementMath.StepsFor at the config's simHz); no allocation in a step; no randomness.
namespace Skygrind.Core
{
    public static class Ride40
    {
        /// <summary>design.n40.ride40, read once (on = false when the run's ride40 bit is off).</summary>
        public struct Params
        {
            public bool on;
            /// <summary>THE ATTACH ASKS FOR INTENT (round 2; each rule off at its neutral value: 0, or inputDot <= -1): no ride starts (by a contact
            /// or by wallride35's near-miss draw) unless the ground below the feet is at least `minClearance` m down, the horizontal speed (before the
            /// wall clipped it) is at least `minSpeed` m/s, the stick points toward the wall (dot(wish, -normal) >= `inputDot`), and the wall's face
            /// is still there `minWallHeight` m above the feet; the near-miss draw reaches `attachReach` m (wallride35: 0.5); a ride's wall (or one
            /// facing within wallride35's reRideDeg of it) starts no ride for `lockout` s after that ride ends in the air.</summary>
            public double minClearance, minSpeed, inputDot, minWallHeight, attachReach, lockout;
            public int lockoutSteps;

            /// <summary>Every rule off: NIGHT39's ride.</summary>
            public static Params Neutral => default;

            /// <summary>design.n40.ride40's rules, ON only when `rowOn` (the run's ride40 bit) and the hold rule's numbers exist. An absent attach
            /// number leaves its rule off (attachReach absent: wallride35's).</summary>
            public static Params From(GameConfig cfg, bool rowOn)
            {
                if (!rowOn || !Hold(cfg, true).on) return Neutral;
                double Opt(string k, double off) => cfg.Has(Row + k) ? cfg.Num(Row + k) : off;
                var p = new Params
                {
                    on = true,
                    minClearance = Opt("minClearance", 0),
                    minSpeed = Opt("minSpeed", 0),
                    inputDot = Opt("inputDot", -2),
                    minWallHeight = Opt("minWallHeight", 0),
                    attachReach = Opt("attachReach", -1),
                    lockout = Opt("lockout", 0),
                };
                p.lockoutSteps = p.lockout > 0 ? MovementMath.StepsFor(p.lockout, 1 / cfg.movement.loop.simHz) : 0;
                return p;
            }
        }

        const string Row = "design.n40.ride40.", Old = "design.n38.ride38.";

        static double Num(GameConfig cfg, string key) => cfg.Has(Row + key) ? cfg.Num(Row + key) : cfg.Num(Old + key);

        /// <summary>The hold rule every ride takes while ride40 is on: ride38's numbers with design.n40.ride40's in their place (each where the row
        /// has it). ON only when `rowOn` and a driftMax exists in either row; otherwise Ride38.Params.Neutral.</summary>
        public static Ride38.Params Hold(GameConfig cfg, bool rowOn)
        {
            if (!rowOn || !(cfg.Has(Row + "driftMax") || cfg.Has(Old + "driftMax"))) return Ride38.Params.Neutral;
            var r = new Ride38.Params
            {
                on = true,
                holdSink = Num(cfg, "holdSink"),
                guard = Num(cfg, "guard"),
                deadzoneDeg = Num(cfg, "deadzoneDeg"),
                fullDeg = Num(cfg, "fullDeg"),
                driftMax = Num(cfg, "driftMax"),
                topProbe = Num(cfg, "topProbe"),
            };
            r.guardSteps = MovementMath.StepsFor(r.guard, 1 / cfg.movement.loop.simHz);
            return r;
        }
    }
}
