// NIGHT52 builder U, look52 round 8: the TIER audit a349 prints (FeelReel.N49.Look.cs calls it on its room / floor / arena ON frames): every 6th
// pixel of the frame (the HUD bands and the viewmodel's screen boxes cut) cast into the world collider, the hit classed by the RENDER box it lands
// on (the level JSON's data.boxes kinds: floor / wall / ledge) and its face (|normal.y| >= 0.7 a top, <= 0.35 a face): FLOOR (a floor box's top),
// WALL (a wall box's face), LEDGE FACE (a ledge box's face: v6's island side), DECK TOP (a ledge box's top); the mean Rec. 709 luma of each and the
// three gaps wall - floor, ledge face - wall, deck top - floor. Off (look52 off, -skygrindOff52): the same numbers, today's one-olive frames.
// Round 9 (n52-r13: the floor x0.65 left it 0.08-0.16 OVER the walls: the measured floor is the sunlit top, a wall its side; under the walls by
// 0.04 needs ~x0.24 albedo on spire's room frame and ~x0.07 on its fogged arena frame (extrapolated from r13's x1 / x0.65 pair), which would take
// the 2 m tiles' seams (x0.55 albedo, ~x0.76 luma) from ~0.07 to ~0.02 of luma (estimated, not measured)): the floor is NIGHT51's again and the verdict judges the LEDGE FACE against walls FACING THE SAME WAY (the faces bucketed by their
// normal's axis, +x / -x / +z / -z), pooled over the three frames: a ledge face and a wall in the same light differ by their albedo only (r13's
// spire floor frame set a sunlit wall patch against ledge faces in shade). GREEN when the tiers are applied and the pooled same-facing gap is >=
// Look52ReelTierGap with >= 30 samples of each in at least one bucket. Wall - floor and deck top - floor are printed, not judged.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>[0] floor mean, [1] n, [2] wall, [3], [4] ledge face, [5], [6] deck top, [7]; round 9: [8 + 2b] / [9 + 2b] a wall face's luma SUM
        /// and n in normal bucket b (0 +x, 1 -x, 2 +z, 3 -z), [16 + 2b] / [17 + 2b] a ledge face's.</summary>
        static double[] Tier52Audit(SurvivalGame g, Color32[] px, int w, int h)
        {
            if (g == null || g.cam == null || px == null || g.sim == null || g.sim.map == null) return null;
            const int step = 6;
            var sum = new double[24];
            var vm = new List<Rect>();
            Camera vc = g.viewmodel != null ? g.viewmodel.Camera : null;
            if (vc != null)
                foreach (Renderer r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                {
                    if (!r.enabled || !r.gameObject.activeInHierarchy || r.gameObject.layer != Viewmodel.Layer) continue;
                    Bounds b = r.bounds;
                    float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
                    for (int c = 0; c < 8; c++)
                    {
                        var corner = new Vector3((c & 1) == 0 ? b.min.x : b.max.x, (c & 2) == 0 ? b.min.y : b.max.y, (c & 4) == 0 ? b.min.z : b.max.z);
                        Vector3 sp = vc.WorldToScreenPoint(corner);
                        if (sp.z <= 0) continue;
                        x0 = Math.Min(x0, sp.x); y0 = Math.Min(y0, sp.y); x1 = Math.Max(x1, sp.x); y1 = Math.Max(y1, sp.y);
                    }
                    if (x1 > x0 && y1 > y0) vm.Add(Rect.MinMaxRect(x0, y0, x1, y1));
                }
            List<MapBox> boxes = g.sim.map.boxes;
            MapBox floor = g.sim.map.floor;
            int hudLo = (int)(h * 0.18), hudHi = (int)(h * 0.88);
            const double eps = 0.03;
            bool In(MapBox b, Vec3 p) => p.x >= b.min.x - eps && p.x <= b.max.x + eps && p.y >= b.min.y - eps && p.y <= b.max.y + eps && p.z >= b.min.z - eps && p.z <= b.max.z + eps;
            for (int y = hudLo + step / 2; y < hudHi; y += step)
                for (int x = step / 2; x < w; x += step)
                {
                    bool cut = false;
                    foreach (Rect rc in vm) if (rc.Contains(new Vector2(x + 0.5f, y + 0.5f))) { cut = true; break; }
                    if (cut) continue;
                    Ray ray = g.cam.ScreenPointToRay(new Vector3(x + 0.5f, y + 0.5f, 0));
                    Vec3 o = WebSpace.FromUnity(ray.origin), d = WebSpace.FromUnity(ray.direction);
                    if (!g.world.RayIntersect(o, d, out RayHit hit)) continue;
                    var p = new Vec3(o.x + d.x * hit.distance, o.y + d.y * hit.distance, o.z + d.z * hit.distance);
                    string kind = null;
                    // a ledge wins over a wall / floor it stands on (the shared face belongs to the ledge's own box first)
                    foreach (MapBox b in boxes)
                    {
                        if (!In(b, p)) continue;
                        if (b.kind == "ledge") { kind = "ledge"; break; }
                        kind ??= b.kind;
                    }
                    if (kind == null && In(floor, p)) kind = "floor";
                    if (kind == null) continue;
                    double ny = Math.Abs(hit.normal.y);
                    bool top = ny >= 0.7 && hit.normal.y > 0, face = ny <= 0.35;
                    int k = -1;
                    if (kind == "floor" && top) k = 0;
                    else if (kind == "wall" && face) k = 2;
                    else if (kind == "ledge" && face) k = 4;
                    else if (kind == "ledge" && top) k = 6;
                    if (k < 0) continue;
                    Color32 c = px[y * w + x];
                    double l = (0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b) / 255.0;
                    sum[k] += l;
                    sum[k + 1]++;
                    if (k == 2 || k == 4)
                    {
                        int bucket = Math.Abs(hit.normal.x) >= Math.Abs(hit.normal.z) ? (hit.normal.x >= 0 ? 0 : 1) : (hit.normal.z >= 0 ? 2 : 3);
                        int at = (k == 2 ? 8 : 16) + 2 * bucket;
                        sum[at] += l;
                        sum[at + 1]++;
                    }
                }
            for (int k = 0; k < 8; k += 2) if (sum[k + 1] > 0) sum[k] /= sum[k + 1];
            return sum;
        }

        /// <summary>Round 9: the ledge face against walls facing the same way: per normal bucket with >= 30 samples of each, the gap in mean luma,
        /// weighted by the smaller count (NaN: no such bucket). `n` the weight.</summary>
        static double SameFacingGap52(double[] a, out double n)
        {
            n = 0;
            double s = 0;
            if (a == null || a.Length < 24) return double.NaN;
            for (int b = 0; b < 4; b++)
            {
                double nw = a[9 + 2 * b], nl = a[17 + 2 * b];
                if (nw < 30 || nl < 30) continue;
                double wgt = Math.Min(nw, nl);
                s += wgt * (a[16 + 2 * b] / nl - a[8 + 2 * b] / nw);
                n += wgt;
            }
            return n > 0 ? s / n : double.NaN;
        }

        /// <summary>One frame's tier line (round 9: every gap printed; the judged one is the pooled same-facing gap, Tier52Pooled).</summary>
        static string Tier52Text(double[] a, CultureInfo ci)
        {
            if (a == null) return "no audit";
            string M(int i) => a[i + 1] >= 30 ? a[i].ToString("0.000", ci) : "n/a";
            string Gap(int i, int j) => a[i + 1] < 30 || a[j + 1] < 30 ? "n/a" : (a[i] - a[j]).ToString("+0.000;-0.000;0.000", ci);
            double sf = SameFacingGap52(a, out double sn);
            string sfs = double.IsNaN(sf) ? "n/a" : $"{sf.ToString("+0.000;-0.000;0.000", ci)} (weight {sn:0})";
            return $"floor {M(0)} (n {a[1]:0}) wall {M(2)} (n {a[3]:0}) ledge face {M(4)} (n {a[5]:0}) deck top {M(6)} (n {a[7]:0}); gaps wall-floor {Gap(2, 0)}, face-wall {Gap(4, 2)} (facing the same way {sfs}), deck-floor {Gap(6, 0)}";
        }

        /// <summary>Round 9: adds a frame's buckets into the pooled sums (null: a new pool).</summary>
        static double[] Tier52Pool(double[] pool, double[] a)
        {
            pool ??= new double[24];
            if (a == null || a.Length < 24) return pool;
            for (int i = 8; i < 24; i++) pool[i] += a[i];
            return pool;
        }

        /// <summary>Round 9: the verdict's gap, the ledge face against walls facing the same way pooled over the frames.</summary>
        static string Tier52Pooled(double[] pool, CultureInfo ci, out bool ok)
        {
            double g = SameFacingGap52(pool, out double n);
            ok = !double.IsNaN(g) && g >= UnityTuning.Look52ReelTierGap;
            return double.IsNaN(g) ? "ledge face - same-facing wall, pooled: n/a (no bucket with 30 samples of each)" : $"ledge face - same-facing wall, pooled: {g.ToString("+0.000;-0.000;0.000", ci)} (weight {n:0})";
        }
    }
}
