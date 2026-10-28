// NIGHT38 builder M (design.n38.ride38; NIGHT38.md M1, Research/2026-10-13-wave38/PLAN.md section 1, DECIDE38 D-3): WALLS YOU STAY ON.
// GG 2026-10-13 ~00:10 (GG-REQUEST.md): wall rides "you don't actually fall down you just stay on forever ... you can choose ... where you're
// aiming up and down". The Mover reads it through Mover.flow34.ride35 (inside the NIGHT35 ride state, so the run snapshot saves it whole);
// neutral by default: with Params.Neutral (on = false: the golden vectors', the replays' and every row-off run's config) every Mover.cs branch
// that reads it is skipped, so NIGHT37's ride bit for bit (CLAUDE.md rule 3; the branches sit inside the NIGHT35 wallride35 ones, named by
// NIGHT38.md M1).
// The rules (Mover.cs branches marked "NIGHT38 ride38"; only a HOLD ride: a ride entered with jump held, wallride35's Lucio ride):
// - The height is kept: once a rising entry has braked (wallride35 / wallgrip36's brake), no sink (`holdSink`, 0: wallride35's 0.1 m/s
//   sinkSpeed is not used) and no 8 s safety cap: only a long `guard` (60 s) against a stuck state, still logged as a "safety" end.
// - The look pitch drifts the ride: within +-`deadzoneDeg` it holds; beyond it the ride rises (pitch up) or sinks (pitch down), linear up to
//   `driftMax` m/s at +-`fullDeg`, clamped past it.
// - Never past the wall's top: a rise is cut to 0 when a ray toward the wall from the capsule's upper sample (the feet + body height - radius:
//   the top of the Mover's valid contact band) moved up by this step's rise finds no wall within the radius + `topProbe` m. The whole contact
//   band stays on the wall, so the ride is never lost by drifting.
// - A TIMED ride (jump not held) keeps wallride35 / wallgrip36's rules (the web's 1.2 s). Every end stays (the release's jump-off, crouch,
//   "away", a corner past wallride35's followMaxDeg, the wall ending, a landing).
// NOT RIDEABLE (`noride`): a campaign box tagged `noride: true` (the level format, Research/2026-10-13-wave38 E's notes) is a wall no ride
// starts on and no ride turns onto (wallride35's entry, attach and corner turn). Its boxes are handed to the Mover per run (Mover.SetNoRide38)
// while any NIGHT38 block is on; none: every check is skipped.
// Whole steps (MovementMath.StepsFor at the config's simHz); no allocation in a step; no randomness.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class Ride38
    {
        /// <summary>design.n38.ride38, read once (on = false when the row is off or its numbers are absent).</summary>
        public struct Params
        {
            public bool on;
            public double holdSink, guard, deadzoneDeg, fullDeg, driftMax, topProbe;
            /// <summary>The guard in whole steps at the config's simHz.</summary>
            public int guardSteps;

            /// <summary>Every rule off: NIGHT37's ride.</summary>
            public static Params Neutral => default;

            /// <summary>design.n38.ride38's numbers, ON only when `rowOn` (the run's ride38 bit) and the row's numbers exist.</summary>
            public static Params From(GameConfig cfg, bool rowOn)
            {
                const string p = "design.n38.ride38.";
                if (!rowOn || !cfg.Has(p + "driftMax")) return Neutral;
                var r = new Params
                {
                    on = true,
                    holdSink = cfg.Num(p + "holdSink"),
                    guard = cfg.Num(p + "guard"),
                    deadzoneDeg = cfg.Num(p + "deadzoneDeg"),
                    fullDeg = cfg.Num(p + "fullDeg"),
                    driftMax = cfg.Num(p + "driftMax"),
                    topProbe = cfg.Num(p + "topProbe"),
                };
                r.guardSteps = MovementMath.StepsFor(r.guard, 1 / cfg.movement.loop.simHz);
                return r;
            }
        }

        /// <summary>The vertical speed of a hold ride (m/s, + up) for a look pitch (rad, + up): -holdSink inside the dead zone; beyond it linear
        /// to +-driftMax at +-fullDeg, clamped.</summary>
        public static double Drift(double pitch, in Params p)
        {
            double deg = pitch * 180 / System.Math.PI;
            double a = deg < 0 ? -deg : deg;
            if (a <= p.deadzoneDeg) return -p.holdSink;
            double span = p.fullDeg - p.deadzoneDeg;
            double t = span > 1e-9 ? (a - p.deadzoneDeg) / span : 1;
            if (t > 1) t = 1;
            double v = p.driftMax * t;
            return deg < 0 ? -v : v;
        }

        /// <summary>A `noride` box (axis-aligned, the campaign box's min / max).</summary>
        public struct Box
        {
            public Vec3 min, max;
        }

        /// <summary>Is the point on (or within `skin` m of) a noride box? `boxes` null or empty: never.</summary>
        public static bool NoRideAt(Box[] boxes, Vec3 point, double skin)
        {
            if (boxes == null) return false;
            for (int i = 0; i < boxes.Length; i++)
            {
                Box b = boxes[i];
                if (point.x >= b.min.x - skin && point.x <= b.max.x + skin && point.y >= b.min.y - skin && point.y <= b.max.y + skin &&
                    point.z >= b.min.z - skin && point.z <= b.max.z + skin) return true;
            }
            return false;
        }

        /// <summary>Does a noride box (grown by `skin` m) hold any point of the vertical segment at (x, z) from y0 to y1 (the capsule's contact band
        /// pushed onto the wall)? `boxes` null: never.</summary>
        public static bool NoRideBand(Box[] boxes, double x, double z, double y0, double y1, double skin)
        {
            if (boxes == null) return false;
            for (int i = 0; i < boxes.Length; i++)
            {
                Box b = boxes[i];
                if (x >= b.min.x - skin && x <= b.max.x + skin && z >= b.min.z - skin && z <= b.max.z + skin && y1 >= b.min.y - skin && y0 <= b.max.y + skin)
                    return true;
            }
            return false;
        }

        /// <summary>The level's `noride` boxes: every box in json "campaign"."boxes" with `noride: true` and a min / max (null when none). The
        /// sim hands them to the Mover; E's gate checker (Harness.N38.Gates.cs) may read them the same way.</summary>
        public static Box[] NoRideFrom(Dictionary<string, object> mapJson)
        {
            if (mapJson == null || !mapJson.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return null;
            if (!camp.TryGetValue("boxes", out object bo) || !(bo is List<object> boxes)) return null;
            List<Box> list = null;
            foreach (object o in boxes)
            {
                if (!(o is Dictionary<string, object> b) || !b.TryGetValue("noride", out object nr) || !(nr is bool on) || !on) continue;
                if (!b.TryGetValue("min", out object mn) || !b.TryGetValue("max", out object mx)) continue;
                list ??= new List<Box>();
                list.Add(new Box { min = PortData.V3(mn), max = PortData.V3(mx) });
            }
            return list?.ToArray();
        }
    }
}
