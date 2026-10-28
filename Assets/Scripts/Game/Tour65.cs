// NIGHT65 builder T (T2 tour65): the reel's pure parts, kept free of UnityEngine so SimHarness compiles them (`n65tour`, Tools/SimHarness/
// Harness.N65.Tour.cs, the headless twin of Assets/Tests/EditMode/Game/N65TourTests.cs). The scenario is FeelReel.N65.Tour.cs.
// NIGHT66 builder T (T1a, code-scout.md section 3's S3): the GREEN rule moved here (Green: no unframed room, no WARN; the always-true
// `framed == poses` dropped), Solids reads the collision wedges too, and Dry tours a level file by the reel's own rules (the planted cases).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>NIGHT65 T2: tour65's pure parts (no Unity call): the poses of a room row, its line65 frames, the planted pose, the solids test.</summary>
    public static class Tour65
    {
        public sealed class Pose
        {
            public string name, source;
            public double x, y, z, yawDeg, pitch;
            /// <summary>As K52Place takes a spot: [x, y, z, yawDeg, pitchRad].</summary>
            public double[] Spot => new[] { x, y, z, yawDeg, pitch };
        }

        static Pose FromArr(string name, string source, object o)
        {
            if (!(o is List<object> a) || a.Count < 3) return null;
            return new Pose { name = name, source = source, x = PortData.Num(a[0]), y = PortData.Num(a[1]), z = PortData.Num(a[2]), yawDeg = a.Count > 3 ? PortData.Num(a[3]) : 0, pitch = a.Count > 4 ? PortData.Num(a[4]) : 0 };
        }

        /// <summary>The row's frames65 poses in the file's order (keys "_..." skipped; a malformed pose skipped), else its frames36.mid as one pose
        /// "mid36", else none. `source`: "frames65", "frames36.mid" or "none".</summary>
        public static List<Pose> Poses(Dictionary<string, object> row, out string source)
        {
            var res = new List<Pose>();
            source = "none";
            if (row == null) return res;
            if (row.TryGetValue("frames65", out object fo) && fo is Dictionary<string, object> f)
            {
                foreach (var kv in f)
                {
                    if (kv.Key.StartsWith("_", StringComparison.Ordinal)) continue;
                    Pose p = FromArr(kv.Key, "frames65", kv.Value);
                    if (p != null) res.Add(p);
                }
                if (res.Count > 0) { source = "frames65"; return res; }
            }
            if (row.TryGetValue("frames36", out object f36) && f36 is Dictionary<string, object> d36 && d36.TryGetValue("mid", out object mid))
            {
                Pose p = FromArr("mid36", "frames36.mid", mid);
                if (p != null) { res.Add(p); source = "frames36.mid"; }
            }
            return res;
        }

        /// <summary>The row's line65 waypoints ([[x, y, z], ...] or {points: [...]}), malformed points skipped.</summary>
        public static List<double[]> Line(Dictionary<string, object> row)
        {
            var res = new List<double[]>();
            if (row == null || !row.TryGetValue("line65", out object lo)) return res;
            if (lo is Dictionary<string, object> d) lo = d.TryGetValue("points", out object pts) ? pts : d.TryGetValue("waypoints", out object wp) ? wp : null;
            if (!(lo is List<object> list)) return res;
            foreach (object o in list)
                if (o is List<object> a && a.Count >= 3) res.Add(new[] { PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]) });
            return res;
        }

        /// <summary>n frames along the polyline by arc length (the first at its start, the last at its end), each looking along the segment it lies
        /// on (yaw as the sim's: 0 = north, -z; pitch from the slope, clamped, with the reel's bias). Fewer than 2 points: none.</summary>
        public static List<Pose> LineFrames(List<double[]> pts, int n)
        {
            var res = new List<Pose>();
            if (pts == null || pts.Count < 2 || n < 2) return res;
            var cum = new double[pts.Count];
            for (int i = 1; i < pts.Count; i++) cum[i] = cum[i - 1] + Dist(pts[i - 1], pts[i]);
            double total = cum[pts.Count - 1];
            if (total < 1e-9) return res;
            int seg = 0;
            for (int k = 0; k < n; k++)
            {
                double s = total * k / (n - 1);
                while (seg < pts.Count - 2 && cum[seg + 1] < s) seg++;
                double len = cum[seg + 1] - cum[seg], u = len < 1e-9 ? 0 : Math.Min(1, Math.Max(0, (s - cum[seg]) / len));
                double[] a = pts[seg], b = pts[seg + 1];
                double dx = b[0] - a[0], dy = b[1] - a[1], dz = b[2] - a[2], h = Math.Sqrt(dx * dx + dz * dz);
                double pitch = Math.Max(-UnityTuning.T65LinePitchMax, Math.Min(UnityTuning.T65LinePitchMax, Math.Atan2(dy, Math.Max(h, 1e-6)))) + UnityTuning.T65LinePitchBias;
                res.Add(new Pose
                {
                    name = "line" + (k + 1), source = "line65",
                    x = a[0] + dx * u, y = a[1] + dy * u, z = a[2] + dz * u,
                    yawDeg = h < 1e-9 ? 0 : Math.Atan2(-dx, -dz) * 180 / Math.PI, pitch = pitch,
                });
            }
            return res;
        }

        static double Dist(double[] a, double[] b) { double dx = b[0] - a[0], dy = b[1] - a[1], dz = b[2] - a[2]; return Math.Sqrt(dx * dx + dy * dy + dz * dz); }

        /// <summary>The planted pose from the command line: `-skygrindTourPlant65 <room>:x,y,z,yawDeg,pitchRad`, or one token
        /// `-skygrindTourPlant65=<room>:x/y/z/yawDeg/pitchRad` (n65-reels.sh splits a side's extra args on commas): (room, the pose "planted"), or null.</summary>
        public static (string room, Pose pose)? Plant(string[] args)
        {
            const string flag = "-skygrindTourPlant65";
            args = args ?? new string[0];
            string v = null;
            for (int k = 0; k < args.Length && v == null; k++)
            {
                if (args[k] == flag && k + 1 < args.Length) v = args[k + 1];
                else if (args[k] != null && args[k].StartsWith(flag + "=", StringComparison.Ordinal)) v = args[k].Substring(flag.Length + 1);
            }
            if (v == null) return null;
            int c = v.IndexOf(':');
            if (c <= 0) return null;
            string[] p = v.Substring(c + 1).Split(',', '/');
            if (p.Length < 3) return null;
            var n = new double[5];
            for (int k = 0; k < p.Length && k < 5; k++)
                if (!double.TryParse(p[k], NumberStyles.Float, CultureInfo.InvariantCulture, out n[k])) return null;
            return (v.Substring(0, c), new Pose { name = "planted", source = "planted", x = n[0], y = n[1], z = n[2], yawDeg = n[3], pitch = n[4] });
        }

        /// <summary>The level file's solids: its collision boxes ([minx, miny, minz, maxx, maxy, maxz], 6 numbers) and, NIGHT66 T1a (code-scout.md
        /// section 3's S3), its collision wedges, the 20-degree slide ramps ([x, z, yaw, length, width, rise, 1], 7 numbers: an entry of 7 is a
        /// wedge, as PortData.Wedge reads it; its base at y 0, rising `rise` over `length` along `yaw`, ArenaMap.WedgeCorners).</summary>
        public static List<double[]> Solids(Dictionary<string, object> json)
        {
            var res = new List<double[]>();
            if (json == null || !json.TryGetValue("collision", out object co) || !(co is Dictionary<string, object> col)) return res;
            if (col.TryGetValue("boxes", out object bo) && bo is List<object> boxes)
                foreach (object o in boxes)
                    if (o is Dictionary<string, object> b && b.TryGetValue("min", out object mi) && b.TryGetValue("max", out object ma) && mi is List<object> lo && ma is List<object> hi && lo.Count >= 3 && hi.Count >= 3)
                        res.Add(new[] { PortData.Num(lo[0]), PortData.Num(lo[1]), PortData.Num(lo[2]), PortData.Num(hi[0]), PortData.Num(hi[1]), PortData.Num(hi[2]) });
            if (col.TryGetValue("wedges", out object wo) && wo is List<object> wedges)
                foreach (object o in wedges)
                    if (o is Dictionary<string, object> w && w.ContainsKey("x") && w.ContainsKey("z") && w.ContainsKey("yaw") && w.ContainsKey("length") && w.ContainsKey("width") && w.ContainsKey("rise"))
                    {
                        WedgeSpec s = PortData.Wedge(w);
                        res.Add(new[] { s.x, s.z, s.yaw, s.length, s.width, s.rise, 1 });
                    }
            return res;
        }

        /// <summary>NIGHT66 T1a (S3): whether the tour frames this room: every fight room (kind room), and any other room whose row gave it
        /// frames65 poses, or the planted pose. The reel and the harness's dry tour (`n65tour`) share it.</summary>
        public static bool Toured(string kind, string source, bool planted) => kind == "room" || source == "frames65" || planted;

        /// <summary>NIGHT66 T1a (code-scout.md section 3's S3; review N3): tour65's GREEN. Every toured room framed (no room with no pose), no
        /// pose inside a solid (box or wedge), each frame in its own room with a live wave, a 1920x1080 screen, the run not ended. The old rule's
        /// `framed == poses` term is dropped: it was always true (both counted the same poses), and it let an unframed room or a WARN read GREEN.</summary>
        public static bool Green(int rooms, int unframed, int warns, int wrongRoom, int noWave, bool screen, bool ended)
            => rooms > 0 && unframed == 0 && warns == 0 && wrongRoom == 0 && noWave == 0 && screen && !ended;

        /// <summary>NIGHT66 T1a: the tour's level-file half (no frame, no live wave): the rooms it would tour, the poses it would frame (the
        /// row's poses, the planted one, the line65 frames), the rooms with no pose and the WARNs, read by the reel's own rules.</summary>
        public sealed class DryTour
        {
            public int rooms, poses, lineFrames, warns;
            public readonly List<string> unframed = new List<string>(), warned = new List<string>(), fallback = new List<string>();
            /// <summary>tour65's GREEN rule on the level file's half (the live terms, wrong room / no wave / screen / ended, as a clean run's).</summary>
            public bool Green => Tour65.Green(rooms, unframed.Count, warns, 0, 0, true, false);
        }

        /// <summary>NIGHT66 T1a: the dry tour of a level file's campaign rooms (`campaign.rooms`, each row's id and kind), as the reel tours them.</summary>
        public static DryTour Dry(Dictionary<string, object> json, (string room, Pose pose)? plant)
        {
            var d = new DryTour();
            if (json == null || !json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp) || !camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rows)) return d;
            List<double[]> solids = Solids(json);
            foreach (object o in rows)
            {
                if (!(o is Dictionary<string, object> row) || !row.TryGetValue("id", out object io) || !(io is string rid)) continue;
                string kind = row.TryGetValue("kind", out object k) && k is string ks ? ks : "room";
                List<Pose> list = Poses(row, out string source);
                bool planted = plant.HasValue && plant.Value.room == rid;
                if (planted) list.Add(plant.Value.pose);
                if (!Toured(kind, source, planted)) continue;
                if (list.Count == 0) { d.unframed.Add(rid); continue; }
                if (source == "frames36.mid") d.fallback.Add(rid);
                d.rooms++;
                d.poses += list.Count;
                List<Pose> lf = LineFrames(Line(row), UnityTuning.T65LineFrames);
                d.lineFrames += lf.Count;
                var shots = new List<Pose>(list);
                shots.AddRange(lf);
                foreach (Pose p in shots)
                {
                    string inside = InsideSolid(solids, p);
                    if (inside != null) { d.warns++; d.warned.Add($"{rid} {p.name} ({inside})"); }
                }
            }
            return d;
        }

        /// <summary>Which point of the pose (its low point over the feet, or its eye) is inside a solid: "eye", "feet", "feet + eye", or null.</summary>
        public static string InsideSolid(List<double[]> solids, Pose p)
        {
            bool feet = Inside(solids, p.x, p.y + UnityTuning.T65InsideFeetH, p.z), eye = Inside(solids, p.x, p.y + UnityTuning.T65InsideEyeH, p.z);
            return feet && eye ? "feet + eye" : feet ? "feet" : eye ? "eye" : null;
        }

        static bool Inside(List<double[]> solids, double x, double y, double z)
        {
            double e = UnityTuning.T65InsideEps;
            foreach (double[] b in solids)
            {
                if (b.Length == 7)
                {
                    // a wedge (NIGHT66 T1a): the point in the ramp's frame (along its rise, across it), inside under its sloped top
                    double ax = -Math.Sin(b[2]), az = -Math.Cos(b[2]), rx = Math.Cos(b[2]), rz = -Math.Sin(b[2]);
                    double dx = x - b[0], dz = z - b[1], along = dx * ax + dz * az, across = dx * rx + dz * rz;
                    if (b[3] > 1e-9 && along > e && along < b[3] - e && Math.Abs(across) < b[4] / 2 - e && y > e && y < b[5] * along / b[3] - e) return true;
                    continue;
                }
                if (x > b[0] + e && x < b[3] - e && y > b[1] + e && y < b[4] - e && z > b[2] + e && z < b[5] - e) return true;
            }
            return false;
        }
    }
}
