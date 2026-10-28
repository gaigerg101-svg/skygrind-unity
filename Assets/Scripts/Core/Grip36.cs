// NIGHT36 builder M (design.n36.wallgrip36; NIGHT36.md M1, REVIEW.md root cause 4, PLAN.md F5): WALLS GRIP, THEY NEVER LIFT YOU. GG 2026-10-12 on the
// wall ride: "you kind of hook onto a wall and you kind of just start sliding up it". The cause (REVIEW.md 4): a ride keeps a jump's rise on attach
// (Mover.cs StartRide, the web's `if (velocity.y < 0) velocity.y = 0`: rising entries keep rising) and a TAP ride (jump not held) climbs at the web's
// wallRide.gravityMult 0.15 (flow34 ridePhase keeps that 0.15 for a rising entry: Flow34.RideGravity), about 7 m from a 7 m/s jump.
// ULTRAKILL (Research/2026-10-12-wave36/research/rea-ultrakill-combat.md section 5, NewMovement.cs Cling :820-858, rule shape only): "There is no upward
// motion from touching a wall"; the cling is a slide whose fall speed grows over the airtime (2 x clingFade, clingFade +4 / s).
// The rules (Mover.cs branches marked "NIGHT36 wallgrip36", inside the NIGHT35 wallride35 branches only: CLAUDE.md rule 3, named by NIGHT36.md M1):
// - Attach: the upward speed is clamped to attachUpMax (a small kick); the arrival speed along the wall is kept (the web's ride already redirects
//   the full horizontal speed along the wall).
// - A rising remainder brakes at riseGravityMult x gravity (1: the kick of 1.5 m/s rises 0.05 m), so the ride never climbs.
// - Then the GRIP: gripTime s level (no gravity, no sink), counted from the attach.
// - Then a slow SINK: a ride without jump held (the web's 1.2 s timed ride) sinks at sinkAccel m/s per s up to sinkMax m/s; a HOLD ride (jump held,
//   wallride35's Lucio ride) keeps wallride35's own constant sinkSpeed (it rides as long as jump is held).
// Off (the row off, --off36): Params.Neutral (on = false), every branch is skipped: wallride35's ride bit for bit. Whole steps; no allocation; no
// randomness.

namespace Skygrind.Core
{
    public static class Grip36
    {
        /// <summary>design.n36.wallgrip36, read once (on = false when the row is off or its numbers are absent).</summary>
        public struct Params
        {
            public bool on;
            public double attachUpMax, riseGravityMult, gripTime, sinkAccel, sinkMax;
            /// <summary>The grip in whole steps at the config's simHz.</summary>
            public int gripSteps;

            /// <summary>Every rule off: wallride35's ride.</summary>
            public static Params Neutral => default;

            /// <summary>design.n36.wallgrip36's numbers, ON only when `rowOn` (the run's n36 wallgrip36 bit) and the row's numbers exist.</summary>
            public static Params From(GameConfig cfg, bool rowOn)
            {
                const string p = "design.n36.wallgrip36.";
                if (!rowOn || !cfg.Has(p + "attachUpMax")) return Neutral;
                var r = new Params
                {
                    on = true,
                    attachUpMax = cfg.Num(p + "attachUpMax"),
                    riseGravityMult = cfg.Num(p + "riseGravityMult"),
                    gripTime = cfg.Num(p + "gripTime"),
                    sinkAccel = cfg.Num(p + "sinkAccel"),
                    sinkMax = cfg.Num(p + "sinkMax"),
                };
                r.gripSteps = MovementMath.StepsFor(r.gripTime, 1 / cfg.movement.loop.simHz);
                return r;
            }
        }

        /// <summary>The ride's gravity multiplier this step (`age` steps since the attach, 1 on the first ride step), and the vertical speed set when the
        /// rule holds it: rising brakes at riseGravityMult; the grip holds vy at 0; then the sink (a hold ride: `holdSink`; a timed ride: the ramp).</summary>
        public static double Gravity(in Params p, int age, bool held, double holdSink, double dt, ref double vy)
        {
            if (vy > 0) return p.riseGravityMult;
            if (age <= p.gripSteps)
            {
                vy = 0;
                return 0;
            }
            double sink = held ? holdSink : p.sinkAccel * (age - p.gripSteps) * dt;
            if (sink > p.sinkMax && !held) sink = p.sinkMax;
            vy = -sink;
            return 0;
        }
    }
}
