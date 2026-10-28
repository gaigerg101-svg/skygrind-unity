// NIGHT63 builder V (exit63, the doorway itself is the way out; view row design.n63.exit63, OFF in the repo; Tools/night/n63-v-exit-rows.cjs):
// the pure parts, in web space (three.js axes, metres), with no Unity object, so EditMode tests pin them (N63ExitTests). Exit63.N63V.cs draws
// them. A door is ExitFrame61Math.Door (its rect is its whole passage, 4 m deep on n57-kiln, its floor at y 0, room A = between[0], the room
// the clear opens it from); `along` is metres from room A's face into room A (negative: into the passage).
// The parts, all INSIDE the passage, clear of its walls, floor and ceiling by numbers.inset (never coplanar), so a157's door box (the passage
// grown 0.37 m, FeelReel.L60ScreenRect) holds every one and the GUARD's frame (outside the box) gets nothing:
// - THE VEIL: a thin sheet over the opening just inside room A's face (the opening itself reads cyan);
// - THE LINERS: both jambs and the lintel lined for the passage's first numbers.linerDepthM (the doorway's own surfaces turn cyan);
// - THE THRESHOLD: a strip across the floor just inside the face;
// - THE DASHES: numbers.dashCount dashes on the passage floor along the axis toward room B (the way on).
using System;
using Skygrind.Core;
using F = Skygrind.Game.ExitFrame61Math;

namespace Skygrind.Game
{
    public static class Exit63Math
    {
        /// <summary>The view row's name (design.n63.exit63).</summary>
        public const string Row = "exit63";
        /// <summary>The one-row off switch: this row alone off.</summary>
        public const string NoSwitch = "-skygrindNoExit63";
        /// <summary>The SPOT pose's reel option (a157 with -skygrindClearShots60): `-skygrindExitShots63 [room]`.</summary>
        public const string ShotSwitch = "-skygrindExitShots63";
        /// <summary>The row's night (its -skygrindOffNN switch covers it from this night on).</summary>
        public const int Night = 63;
        /// <summary>The row's numbers' prefix in the config.</summary>
        public const string Numbers = "design.n63.exit63.numbers.";

        /// <summary>design.n63.exit63.numbers (see the row's _why).</summary>
        public sealed class Params
        {
            /// <summary>s: the one rise from dark to full when the door opens.</summary>
            public float riseS;
            /// <summary>Each part's additive share of colors.usable at full (1 or under: no bloom past the door's box).</summary>
            public float veilGain, linerGain, stripGain, dashGain;
            /// <summary>m: every part's clearance from the passage's walls, floor, ceiling and faces.</summary>
            public float inset;
            /// <summary>m: the veil's and the liners' thickness.</summary>
            public float veilT, linerT;
            /// <summary>m: how far into the passage the liners run (capped by the passage).</summary>
            public float linerDepthM;
            /// <summary>m: the threshold strip's depth along the passage, and its and the dashes' thickness.</summary>
            public float stripDepthM, stripT;
            /// <summary>m: a dash's length, its width and the gap before each.</summary>
            public float dashLenM, dashW, dashGapM;
            /// <summary>The dashes (those that fit the passage).</summary>
            public int dashCount;
            /// <summary>m: the veil is gone with the eye this near its sheet, whole from veilFarM out.</summary>
            public float veilNearM, veilFarM;

            static bool Gain(float g) => g >= 0f && g <= 1f;

            /// <summary>Every size positive, every gain in [0, 1], the rise not negative, the veil's fade far past near: else nothing is drawn.</summary>
            public bool Valid => riseS >= 0f && Gain(veilGain) && Gain(linerGain) && Gain(stripGain) && Gain(dashGain) && inset > 0f && veilT > 0f
                && linerT > 0f && linerDepthM > 0f && stripDepthM > 0f && stripT > 0f && dashLenM > 0f && dashW > 0f && dashGapM >= 0f && dashCount >= 0
                && veilNearM >= 0f && veilFarM > veilNearM;

            public static Params From(GameConfig cfg)
            {
                var p = new Params();
                if (cfg == null) return p;
                float N(string k) => cfg.Has(Numbers + k) ? (float)cfg.Num(Numbers + k) : -1f;
                p.riseS = N("riseS");
                p.veilGain = N("veilGain");
                p.linerGain = N("linerGain");
                p.stripGain = N("stripGain");
                p.dashGain = N("dashGain");
                p.inset = N("inset");
                p.veilT = N("veilT");
                p.linerT = N("linerT");
                p.linerDepthM = N("linerDepthM");
                p.stripDepthM = N("stripDepthM");
                p.stripT = N("stripT");
                p.dashLenM = N("dashLenM");
                p.dashW = N("dashW");
                p.dashGapM = N("dashGapM");
                p.dashCount = (int)N("dashCount");
                p.veilNearM = N("veilNearM");
                p.veilFarM = N("veilFarM");
                return p;
            }
        }

        /// <summary>THE VEIL: a sheet veilT thick just inside room A's face, over the opening less its insets.</summary>
        public static F.Part Veil(F.Door d, Params p) =>
            d.Box(-p.inset - p.veilT / 2, 0, d.height / 2, d.width - 2 * p.inset, d.height - 2 * p.inset, p.veilT);

        /// <summary>The liners' run along the passage: linerDepthM, at most the passage less an inset at each face.</summary>
        static double LinerRun(F.Door d, Params p) => Math.Max(0, Math.Min(p.linerDepthM, d.depth - 2 * p.inset));

        /// <summary>THE LINERS: the -lateral jamb, the +lateral jamb and the lintel (the passage's ceiling between them), linerT thick, inset
        /// clear of the walls, floor and ceiling, from just inside room A's face for LinerRun m.</summary>
        public static F.Part[] Liners(F.Door d, Params p)
        {
            double run = LinerRun(d, p), along = -p.inset - run / 2;
            double side = d.width / 2 - p.inset - p.linerT / 2, up = d.height - 2 * p.inset;
            return new[]
            {
                d.Box(along, -side, d.height / 2, p.linerT, up, run),
                d.Box(along, side, d.height / 2, p.linerT, up, run),
                d.Box(along, 0, d.height - p.inset - p.linerT / 2, d.width - 2 * p.inset - 2 * p.linerT, p.linerT, run),
            };
        }

        /// <summary>THE THRESHOLD: a strip across the floor, stripDepthM deep, just inside room A's face, inset over the floor.</summary>
        public static F.Part Strip(F.Door d, Params p) =>
            d.Box(-p.inset - p.stripDepthM / 2, 0, p.inset + p.stripT / 2, d.width - 2 * p.inset, p.stripT, p.stripDepthM);

        /// <summary>THE DASHES: after the strip and a gap, dashCount dashes dashLenM long with dashGapM between, on the axis toward room B; a dash
        /// that would reach past room B's face (less the inset) is left out.</summary>
        public static F.Part[] Dashes(F.Door d, Params p)
        {
            var list = new System.Collections.Generic.List<F.Part>();
            double s = p.inset + p.stripDepthM + p.dashGapM;
            for (int i = 0; i < p.dashCount; i++, s += p.dashLenM + p.dashGapM)
            {
                double e = s + p.dashLenM;
                if (e > d.depth - p.inset) break;
                list.Add(d.Box(-(s + e) / 2, 0, p.inset + p.stripT / 2, p.dashW, p.stripT, p.dashLenM));
            }
            return list.ToArray();
        }

        /// <summary>Every part: the veil first, then the liners, the strip and the dashes (Exit63's order: index 0 is the veil).</summary>
        public static F.Part[] All(F.Door d, Params p)
        {
            F.Part[] l = Liners(d, p), s = Dashes(d, p);
            var all = new F.Part[2 + l.Length + s.Length];
            all[0] = Veil(d, p);
            Array.Copy(l, 0, all, 1, l.Length);
            all[1 + l.Length] = Strip(d, p);
            Array.Copy(s, 0, all, 2 + l.Length, s.Length);
            return all;
        }

        /// <summary>ONE RISE: 0 at the open, smoothstep to 1 at riseS, held at 1 after (never above: no flash); riseS 0 or less: 1 at once.</summary>
        public static float Rise(float t, float riseS)
        {
            if (riseS <= 0f) return 1f;
            float u = t <= 0f ? 0f : t >= riseS ? 1f : t / riseS;
            return u * u * (3f - 2f * u);
        }

        /// <summary>The veil's share as the eye nears its sheet (`distM`: the eye to the sheet's box): 1 from veilFarM out, 0 at veilNearM or
        /// nearer, smoothstep between (no screen-wide cyan wash as you run through the door; Doors38's own shimmer marks the crossing).</summary>
        public static float VeilK(float distM, Params p)
        {
            if (p == null || !(p.veilFarM > p.veilNearM)) return 1f;
            if (distM >= p.veilFarM) return 1f;
            if (distM <= p.veilNearM) return 0f;
            float u = (distM - p.veilNearM) / (p.veilFarM - p.veilNearM);
            return u * u * (3f - 2f * u);
        }

        /// <summary>Why the row is off for this config and command line (null: on). The row first (its design `enabled`, the process's off switch,
        /// D1's reel override: NightRows.ViewOn), then -skygrindOff63 (or older), then -skygrindNoExit63.</summary>
        public static string OffReason(GameConfig cfg, string[] args)
        {
            if (!NightRows.ViewOn(cfg, Row)) return RowOff;
            if (NightRows.OffFrom(args, "-skygrindOff") <= Night) return "-skygrindOff63 (or older)";
            if (NightRows.HasSwitch(args, NoSwitch)) return NoSwitch;
            return null;
        }

        /// <summary>OffReason's answer when the row itself is off (the view rebuilds if the row turns on later: D1's override).</summary>
        public const string RowOff = "design.n63.exit63 off";

        public static bool OffFor(GameConfig cfg, string[] args) => OffReason(cfg, args) != null;

        /// <summary>The Player's proof line when a door lights (L-089: grep "[exit63] door lit: room").</summary>
        public static string LitLine(string room, string door, int frame) => "[exit63] door lit: room " + room + " (door " + door + ", frame " + frame + ")";

        /// <summary>THE SPOT POSE: the feet on the door's axis `distM` m into room A from its face, on the floor (y 0).</summary>
        public static Vec3 ShotSpot(F.Door d, double distM)
        {
            Vec3 c = d.At(distM, 0, 0);
            return new Vec3(c.x, 0, c.z);
        }

        /// <summary>The web yaw (deg; 0 faces -z, + turns toward -x: L60ClearShot's) from `at` toward the door's centre.</summary>
        public static double ShotYawDeg(F.Door d, Vec3 at) => Math.Atan2(-(d.cx - at.x), -(d.cz - at.z)) * 180 / Math.PI;

        /// <summary>The SPOT pose's room: null without -skygrindExitShots63 (a157 unchanged), "" for every room, or the room id after it (an
        /// argument that is not another switch).</summary>
        public static string ShotRoom(string[] args)
        {
            if (args == null) return null;
            for (int i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], ShotSwitch, StringComparison.OrdinalIgnoreCase)) continue;
                return i + 1 < args.Length && args[i + 1] != null && args[i + 1].Length > 0 && args[i + 1][0] != '-' ? args[i + 1] : "";
            }
            return null;
        }
    }
}
