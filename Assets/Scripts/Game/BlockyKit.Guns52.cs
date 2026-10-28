// NIGHT52 builder U, guns52 (design.n52.guns52, a view row: no sim bit; NIGHT52.md section 4: "the THUNDERCLAP / KESTREL viewmodels are flat
// blocks"; GG keeps the guns' shapes: no new silhouette). While BlockyKit.Guns52 is set (only by the Viewmodel, round its KESTREL and THUNDERCLAP
// builds, with the row on: Viewmodel.Guns52.cs), the kit's boxes and outline extrusions are built MACHINED instead of as sharp blocks:
//   - CHAMFERED PARTS: a box's 12 edges are cut by a 45 deg chamfer (c = Guns52Chamfer, at most Guns52ChamferShare of its smallest side) with
//     its 8 corners capped; an extrusion's two cap edges are cut the same way (the cap's outline inset by c, a bevel ring between). The parts'
//     outlines and sizes stay (a chamfer only takes the corner off): the silhouette is the gun's;
//   - A LIT EDGE: every chamfer face is the part's colour lifted toward white (Guns52EdgeLight) and tilted 45 deg, so it catches the key and
//     reads as a machined highlight round each part;
//   - TWO-TONE PANELS: an extrusion's cap big enough for it (its outline inset by c + Guns52PanelBorder still a clean polygon of at least
//     Guns52PanelMinArea) is split into a frame in the part's colour and a recessed panel in its darker tone (Guns52PanelDark).
// Tubes, cones, the torus, the vents and an extrusion with a hole (the trigger guard) stay as they were; a part too small for the chamfer, a seam
// line (BlockyKit.Seam) and an outline whose inset would fold stay plain (counted in Guns52Probe). Off (the flag false): nothing here runs and every
// mesh is built exactly as NIGHT51's. Numbers: UnityTuning.U52.cs.
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class BlockyKit
    {
        /// <summary>NIGHT52 guns52: build machined (Viewmodel.Guns52.cs sets it round the KESTREL / THUNDERCLAP builds; false everywhere else).</summary>
        public static bool Guns52;

        /// <summary>Round 3 (n52-r5: the THUNDERCLAP's lit edges barely read: a 0.72 m gun seen from above-behind, its 2.5 mm chamfers ~4 px): the
        /// big gun's build (set with Guns52 round the THUNDERCLAP only): wider, brighter chamfers and the two-tone stripe on its wide walls.</summary>
        public static bool Guns52Big;

        /// <summary>The chamfer for a part whose smallest side is `minSide`.</summary>
        static float Cc52(float minSide) => Guns52Big
            ? Mathf.Min(UnityTuning.Guns52ChamferBig, UnityTuning.Guns52ChamferShareBig * minSide)
            : Mathf.Min(UnityTuning.Guns52Chamfer, UnityTuning.Guns52ChamferShare * minSide);

        static Color EdgeTone52(Color c)
        {
            Color e = Color.Lerp(c, Color.white, Guns52Big ? UnityTuning.Guns52EdgeLightBig : UnityTuning.Guns52EdgeLight);
            e.a = c.a;
            return e;
        }

        static Color PanelTone52(Color c)
        {
            var p = new Color(c.r * UnityTuning.Guns52PanelDark, c.g * UnityTuning.Guns52PanelDark, c.b * UnityTuning.Guns52PanelDark, c.a);
            return p;
        }

        /// <summary>A box (w, h, d) centred at web (x, y, z) with its 12 edges chamfered and its 8 corners capped. False (nothing built) when the
        /// part is too small for a chamfer or is a seam line: the caller builds the plain box.</summary>
        static bool Chamfer52Box(MeshKit kit, float w, float h, float d, float x, float y, float z, Color c)
        {
            float cc = Cc52(Mathf.Min(w, Mathf.Min(h, d)));
            if (cc < UnityTuning.Guns52ChamferMin || c == Seam) { Guns52Probe.PlainParts++; return false; }
            Vector3 o = W(x, y, z);
            var hs = new Vector3(w * 0.5f, h * 0.5f, d * 0.5f);
            Color ec = EdgeTone52(c);
            // the point at signs (sx, sy, sz) with each axis either at its face (the half size) or inset by the chamfer
            Vector3 Pt(float sx, float sy, float sz, bool ix, bool iy, bool iz) =>
                o + new Vector3(sx * (hs.x - (ix ? cc : 0)), sy * (hs.y - (iy ? cc : 0)), sz * (hs.z - (iz ? cc : 0)));
            // 6 faces, each inset by the chamfer on its four edges
            for (int a = 0; a < 3; a++)
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 n = Vector3.zero;
                    n[a] = s;
                    int b = (a + 1) % 3, k = (a + 2) % 3;
                    Vector3 V(float sb, float sk)
                    {
                        Vector3 p = o;
                        p[a] += s * hs[a];
                        p[b] += sb * (hs[b] - cc);
                        p[k] += sk * (hs[k] - cc);
                        return p;
                    }
                    kit.Quad(V(-1, -1), V(1, -1), V(1, 1), V(-1, 1), n, c);
                }
            // 12 edge bevels: along axis k, between the faces of axes a and b
            for (int k = 0; k < 3; k++)
            {
                int a = (k + 1) % 3, b = (k + 2) % 3;
                for (int sa = -1; sa <= 1; sa += 2)
                    for (int sb = -1; sb <= 1; sb += 2)
                    {
                        Vector3 E(float sk, bool onA)
                        {
                            Vector3 p = o;
                            p[a] += sa * (hs[a] - (onA ? 0 : cc));
                            p[b] += sb * (hs[b] - (onA ? cc : 0));
                            p[k] += sk * (hs[k] - cc);
                            return p;
                        }
                        Vector3 n = Vector3.zero;
                        n[a] = sa;
                        n[b] = sb;
                        kit.Quad(E(-1, true), E(1, true), E(1, false), E(-1, false), n.normalized, ec);
                    }
            }
            // 8 corner caps
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                        kit.Triangle(Pt(sx, sy, sz, false, true, true), Pt(sx, sy, sz, true, false, true), Pt(sx, sy, sz, true, true, false), new Vector3(sx, sy, sz).normalized, ec);
            Guns52Probe.ChamferedParts++;
            return true;
        }

        /// <summary>The outline (CCW, (z, y)) moved `d` inward along its edges (each vertex on the two offset edges' meeting point); null when it
        /// would fold (an edge reversed or collapsed, a mitre past Guns52MiterMax, the winding lost).</summary>
        static List<Vector2> Inset52(List<Vector2> loop, float d)
        {
            int n = loop.Count;
            if (n < 3) return null;
            var o = new List<Vector2>(n);
            for (int i = 0; i < n; i++)
            {
                Vector2 p0 = loop[(i + n - 1) % n], p = loop[i], p1 = loop[(i + 1) % n];
                Vector2 e0 = (p - p0).normalized, e1 = (p1 - p).normalized;
                Vector2 n0 = new Vector2(-e0.y, e0.x), n1 = new Vector2(-e1.y, e1.x); // a CCW outline's inside is its left
                float den = 1f + Vector2.Dot(n0, n1);
                if (den < 1e-3f) return null;
                Vector2 m = (n0 + n1) / den; // the mitre (unit normals): |m| = 1 / cos(half the turn)
                if (m.magnitude > UnityTuning.Guns52MiterMax) return null;
                o.Add(p + m * d);
            }
            float area = 0;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = o[i], b = o[(i + 1) % n];
                area += a.x * b.y - b.x * a.y;
                if (Vector2.Dot(b - a, loop[(i + 1) % n] - loop[i]) <= 0) return null; // an edge reversed or collapsed
            }
            if (area <= 0) return null;
            for (int i = 0; i < n; i++) // no two edges of the inset cross (a fold the checks above missed)
                for (int j = i + 2; j < n; j++)
                {
                    if (i == 0 && j == n - 1) continue;
                    Vector2 a = o[i], b = o[(i + 1) % n], q = o[j], r = o[(j + 1) % n];
                    if (Cross(b - a, q - a) * Cross(b - a, r - a) < 0 && Cross(r - q, a - q) * Cross(r - q, b - q) < 0) return null;
                }
            return o;
        }

        static float Area52(List<Vector2> loop)
        {
            float area = 0;
            for (int i = 0; i < loop.Count; i++)
            {
                Vector2 a = loop[i], b = loop[(i + 1) % loop.Count];
                area += a.x * b.y - b.x * a.y;
            }
            return area * 0.5f;
        }

        /// <summary>An extrusion (no hole) with its cap edges chamfered and, room permitting, a two-tone recessed panel on each cap. False (nothing
        /// built) when the outline's inset would fold: the caller builds the plain extrusion.</summary>
        static bool Chamfer52Extrude(MeshKit kit, float[,] pts, float w, Color c, float x, float turnZ, float tx, float ty, float tz)
        {
            float cc = Cc52(w);
            if (cc < UnityTuning.Guns52ChamferMin || c == Seam) { Guns52Probe.PlainParts++; return false; }
            List<Vector2> outer = Loop(pts, true);
            List<Vector2> cap = Inset52(outer, cc);
            if (cap == null) { Guns52Probe.Folds++; return false; }
            List<Vector2> panel = Inset52(outer, cc + UnityTuning.Guns52PanelBorder);
            if (panel != null && Area52(panel) < UnityTuning.Guns52PanelMinArea) panel = null;
            float cs = Mathf.Cos(turnZ), sn = Mathf.Sin(turnZ);
            float x0 = x - w / 2, x1 = x + w / 2;
            Vector3 P(Vector2 p, float px) => W(px * cs - p.y * sn + tx, px * sn + p.y * cs + ty, p.x + tz);
            Vector3 N(float nx, float ny, float nz) => W(nx * cs - ny * sn, nx * sn + ny * cs, nz).normalized;
            Color ec = EdgeTone52(c), pc = PanelTone52(c);
            // round 3, the big gun: a wide wall (its width between the chamfers >= Guns52StripeMinW) carries a centred stripe in the darker tone
            // (Guns52StripeShare of it) along the whole outline: the stock's and the receiver's tops read two-tone from the player's view
            float inner = (x1 - cc) - (x0 + cc);
            bool stripe = Guns52Big && inner >= UnityTuning.Guns52StripeMinW;
            float s0 = x0 + cc + inner * (1f - UnityTuning.Guns52StripeShare) * 0.5f, s1 = x1 - cc - inner * (1f - UnityTuning.Guns52StripeShare) * 0.5f;
            if (stripe) Guns52Probe.Stripes++;
            // the walls between the two chamfers (the outline itself: the side view's silhouette is the gun's)
            for (int i = 0; i < outer.Count; i++)
            {
                Vector2 a = outer[i], b = outer[(i + 1) % outer.Count];
                Vector2 e = (b - a).normalized;
                Vector3 wn = N(0, -e.x, e.y);
                if (stripe)
                {
                    kit.Quad(P(a, x0 + cc), P(b, x0 + cc), P(b, s0), P(a, s0), wn, c);
                    kit.Quad(P(a, s0), P(b, s0), P(b, s1), P(a, s1), wn, pc);
                    kit.Quad(P(a, s1), P(b, s1), P(b, x1 - cc), P(a, x1 - cc), wn, c);
                }
                else kit.Quad(P(a, x0 + cc), P(b, x0 + cc), P(b, x1 - cc), P(a, x1 - cc), wn, c);
                // the bevel rings: the outline at the wall's end to the inset cap outline at the cap's plane, tilted 45 deg (lit edge)
                Vector2 ia = cap[i], ib = cap[(i + 1) % cap.Count];
                kit.Quad(P(a, x0 + cc), P(b, x0 + cc), P(ib, x0), P(ia, x0), N(-1, -e.x, e.y), ec);
                kit.Quad(P(a, x1 - cc), P(b, x1 - cc), P(ib, x1), P(ia, x1), N(1, -e.x, e.y), ec);
            }
            // the caps: the panel's frame (the inset cap to the panel outline) and the recessed panel in the darker tone, or the plain inset cap
            if (panel != null)
            {
                for (int i = 0; i < cap.Count; i++)
                {
                    Vector2 a = cap[i], b = cap[(i + 1) % cap.Count], pa = panel[i], pb = panel[(i + 1) % panel.Count];
                    kit.Quad(P(a, x0), P(b, x0), P(pb, x0), P(pa, x0), N(-1, 0, 0), c);
                    kit.Quad(P(a, x1), P(b, x1), P(pb, x1), P(pa, x1), N(1, 0, 0), c);
                }
                Cap52(kit, panel, P, N(-1, 0, 0), N(1, 0, 0), x0, x1, pc);
                Guns52Probe.Panels++;
            }
            else Cap52(kit, cap, P, N(-1, 0, 0), N(1, 0, 0), x0, x1, c);
            Guns52Probe.ChamferedProfiles++;
            return true;
        }

        /// <summary>A polygon's two caps (ear clipping, as Extrude's) at x0 and x1.</summary>
        static void Cap52(MeshKit kit, List<Vector2> poly, System.Func<Vector2, float, Vector3> P, Vector3 n0, Vector3 n1, float x0, float x1, Color c)
        {
            var idx = new List<int>();
            for (int i = 0; i < poly.Count; i++) idx.Add(i);
            int guard = 0;
            while (idx.Count > 2 && guard++ < 512)
            {
                bool clipped = false;
                for (int k = 0; k < idx.Count; k++)
                {
                    Vector2 a = poly[idx[(k + idx.Count - 1) % idx.Count]], b = poly[idx[k]], d = poly[idx[(k + 1) % idx.Count]];
                    if (Cross(b - a, d - b) <= 0) continue; // reflex
                    bool inside = false;
                    for (int m = 0; m < idx.Count && !inside; m++)
                    {
                        Vector2 p = poly[idx[m]];
                        if (p == a || p == b || p == d) continue;
                        inside = InTri(p, a, b, d);
                    }
                    if (inside) continue;
                    kit.Triangle(P(a, x0), P(b, x0), P(d, x0), n0, c);
                    kit.Triangle(P(a, x1), P(b, x1), P(d, x1), n1, c);
                    idx.RemoveAt(k);
                    clipped = true;
                    break;
                }
                if (!clipped) break;
            }
        }
    }

    /// <summary>guns52's build counts (Viewmodel.Guns52.cs resets them before a build; the a352 reel prints them).</summary>
    public static class Guns52Probe
    {
        /// <summary>The row was read on this session (the KESTREL / THUNDERCLAP built machined).</summary>
        public static bool Built;
        /// <summary>The reel's "before" frame: the two guns rebuilt plain (NIGHT51's) with the row still built.</summary>
        public static bool ForceOff;
        /// <summary>The reel only: forces the row (null: the config's design.n52.guns52). Read when the viewmodel is made.</summary>
        public static bool? Force;
        public static int ChamferedParts, ChamferedProfiles, Panels, PlainParts, Folds, Stripes, Rebuilds;
        /// <summary>Round 3: the THUNDERCLAP is built machined (the config's optional design.n52.guns52.numbers.thunderclap: 0 = the fallback,
        /// the KESTREL only; absent or 1 = both).</summary>
        public static bool Thunderclap = true;
        /// <summary>Round 4: guns52 draws now (the muzzle light by real time: LightPools.Guns52.cs).</summary>
        public static bool LiveNow => Built && !ForceOff;
        /// <summary>Round 4: the last muzzle flash's light: ms from the shot to the first dark frame, the frames it was lit, flashes ended.</summary>
        public static double LightMs = -1;
        public static int LightFrames, LightEnds;

        public static void ResetCounts() => ChamferedParts = ChamferedProfiles = Panels = PlainParts = Folds = Stripes = 0;
    }
}
