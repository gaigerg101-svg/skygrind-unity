// The blocky kit's shapes (skygrind/src/assets/blockyKit.js) and the weapon models built from them
// (skygrind/src/assets/weaponModels.js): the KESTREL, THUNDERCLAP, STITCHER, COMET, LONGWAVE and the PULSE GAUNTLET
// (revolver() / shotgun() / nailgun() / rocket() / rail() / gauntlet()). Shapes: box, tubeZ, coneZ, tubeX, a torus,
// vent, `profile` (a side outline in (z, y) extruded across x, with one hole and a turn about z) and the pistol grip.
// Built in web rig space (the grip at the origin, the barrel along -z, metres at viewmodel scale), converted with the z
// flip. Not ported: the bevels (a profile's outline is drawn at its points, not grown by the chamfer) and the seams; the
// KESTREL's frame window is still drawn solid (its builder predates the holes).
// NIGHT12 U3-25.10 (weaponModels.js :18-22, NIGHT4 F1: `tone`): a finish recolours the four neutral body tones (bone,
// gunmetal, dark, grip) while a model is built, never the family trim, the seams or the core gem. A builder reads the tones
// in force: the FACTORY ones, or a finish's palette inside `using (BlockyKit.Finish(palette)) { ... }` (Finishes.PaletteOf;
// null is FACTORY). Built once into vertex colours, so a finish costs a rebuild, never a per-frame change.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class BlockyKit // NIGHT52 guns52: partial (BlockyKit.Guns52.cs, the machined build)
    {
        // blockyKit.js KIT: the FACTORY body tones, and the seam (never refinished)
        static readonly Color FactoryBone = Hex(0xb9b4a9), FactoryGunmetal = Hex(0x3b3e46), FactoryDark = Hex(0x24262c), FactoryGrip = Hex(0x2e2a26);
        public static readonly Color Seam = Hex(0x15161a);
        static Color bone = FactoryBone, gunmetal = FactoryGunmetal, dark = FactoryDark, grip = FactoryGrip;

        /// <summary>The body tones in force (FACTORY, or the finish of the open Finish scope).</summary>
        public static Color Bone => bone;
        public static Color Gunmetal => gunmetal;
        public static Color Dark => dark;
        public static Color Grip => grip;

        /// <summary>The tones a Finish scope puts back when it closes.</summary>
        public readonly struct FinishScope : IDisposable
        {
            readonly Color b, g, d, r;

            public FinishScope(Color b, Color g, Color d, Color r)
            {
                this.b = b;
                this.g = g;
                this.d = d;
                this.r = r;
            }

            public void Dispose()
            {
                bone = b;
                gunmetal = g;
                dark = d;
                grip = r;
            }
        }

        /// <summary>
        /// weaponModels.js tone(): build with a finish's body tones until the scope is disposed (`using`). A null palette
        /// (FACTORY, an unknown finish) builds in the factory tones.
        /// </summary>
        public static FinishScope Finish(Finishes.Palette palette)
        {
            var scope = new FinishScope(bone, gunmetal, dark, grip);
            if (palette == null)
            {
                bone = FactoryBone;
                gunmetal = FactoryGunmetal;
                dark = FactoryDark;
                grip = FactoryGrip;
            }
            else
            {
                bone = Hex(palette.bone);
                gunmetal = Hex(palette.gunmetal);
                dark = Hex(palette.dark);
                grip = Hex(palette.grip);
            }
            return scope;
        }

        public static Color Hex(uint h) => Look.Hex(h);

        static Vector3 W(float x, float y, float z) => new Vector3(x, y, -z); // web -> Unity
        static Vector3 W(Vector3 p) => new Vector3(p.x, p.y, -p.z);

        /// <summary>box(w, h, d, x, y, z): a box of size (w, h, d) centred at (x, y, z).</summary>
        public static void Box(MeshKit kit, float w, float h, float d, float x, float y, float z, Color c)
        {
            if (Guns52 && Chamfer52Box(kit, w, h, d, x, y, z, c)) return; // NIGHT52 guns52 (BlockyKit.Guns52.cs): the chamfered part
            Vector3 center = W(x, y, z);
            kit.Cube(center, new Vector3(w, h, d), c);
        }

        /// <summary>tubeZ(r, len, x, y, z, sides): a cylinder along z centred at (x, y, z).</summary>
        public static void TubeZ(MeshKit kit, float r, float len, float x, float y, float z, int sides, Color c)
        {
            Vector3 center = W(x, y, z);
            Vector3 front = center + new Vector3(0, 0, len / 2), back = center - new Vector3(0, 0, len / 2);
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2 / sides, a1 = (i + 1) * Mathf.PI * 2 / sides;
                var o0 = new Vector3(Mathf.Cos(a0) * r, Mathf.Sin(a0) * r, 0);
                var o1 = new Vector3(Mathf.Cos(a1) * r, Mathf.Sin(a1) * r, 0);
                Vector3 n = ((o0 + o1) * 0.5f).normalized;
                kit.Quad(back + o0, back + o1, front + o1, front + o0, n, c);
                kit.Triangle(front, front + o0, front + o1, Vector3.forward, c);
                kit.Triangle(back, back + o1, back + o0, Vector3.back, c);
            }
        }

        /// <summary>coneZ(r0, r1, len, x, y, z): a tapered tube along z (r0 at the front, -z, end; r1 at the back).</summary>
        public static void ConeZ(MeshKit kit, float r0, float r1, float len, float x, float y, float z, int sides, Color c)
        {
            Vector3 center = W(x, y, z);
            Vector3 front = center + new Vector3(0, 0, len / 2), back = center - new Vector3(0, 0, len / 2);
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2 / sides, a1 = (i + 1) * Mathf.PI * 2 / sides;
                Vector3 d0 = new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0), d1 = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0);
                kit.Quad(back + d0 * r1, back + d1 * r1, front + d1 * r0, front + d0 * r0, ((d0 + d1) * 0.5f).normalized, c);
                kit.Triangle(front, front + d0 * r0, front + d1 * r0, Vector3.forward, c);
                kit.Triangle(back, back + d1 * r1, back + d0 * r1, Vector3.back, c);
            }
        }

        /// <summary>
        /// tubeX(r, len, x, y, z, sides) (blockyKit.js :269-273): a cylinder along x centred at (x, y, z), its sides where
        /// three's CylinderGeometry turned by rotateZ(90 deg) puts them (a vertex at y = r sin t, z = r cos t, t = 2 pi k / sides).
        /// </summary>
        public static void TubeX(MeshKit kit, float r, float len, float x, float y, float z, int sides, Color c)
        {
            var left = new Vector3(x - len / 2, y, z);
            var right = new Vector3(x + len / 2, y, z);
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2 / sides, a1 = (i + 1) * Mathf.PI * 2 / sides;
                var o0 = new Vector3(0, Mathf.Sin(a0) * r, Mathf.Cos(a0) * r);
                var o1 = new Vector3(0, Mathf.Sin(a1) * r, Mathf.Cos(a1) * r);
                kit.Quad(W(left + o0), W(left + o1), W(right + o1), W(right + o0), W(((o0 + o1) * 0.5f).normalized), c);
                kit.Triangle(W(right), W(right + o0), W(right + o1), Vector3.right, c);
                kit.Triangle(W(left), W(left + o1), W(left + o0), Vector3.left, c);
            }
        }

        /// <summary>
        /// three's TorusGeometry(R, t, radial, tubular) around the web z axis at (x, y, z), flat-shaded; axis 'y' / 'x' turns it
        /// to face that axis (blockyKit.js socketGem :326-327: rotateX / rotateY by 90 deg).
        /// </summary>
        public static void Torus(MeshKit kit, float R, float t, int radial, int tubular, float x, float y, float z, Color c, char axis = 'z')
        {
            var o = new Vector3(x, y, z);
            for (int i = 0; i < tubular; i++)
            {
                for (int j = 0; j < radial; j++)
                {
                    Vector3 a = TorusPoint(R, t, i, j, radial, tubular, axis), b = TorusPoint(R, t, i + 1, j, radial, tubular, axis);
                    Vector3 cc = TorusPoint(R, t, i + 1, j + 1, radial, tubular, axis), d = TorusPoint(R, t, i, j + 1, radial, tubular, axis);
                    float um = (i + 0.5f) / tubular * Mathf.PI * 2;
                    Vector3 tube = Turn(R * Mathf.Cos(um), R * Mathf.Sin(um), 0, axis); // the tube's centre line
                    Vector3 n = Vector3.Cross(b - a, d - a).normalized;
                    if (Vector3.Dot(n, (a + b + cc + d) * 0.25f - tube) < 0) n = -n;
                    kit.Quad(W(a + o), W(b + o), W(cc + o), W(d + o), W(n), c);
                }
            }
        }

        static Vector3 TorusPoint(float R, float t, int i, int j, int radial, int tubular, char axis)
        {
            float u = (float)i / tubular * Mathf.PI * 2, v = (float)j / radial * Mathf.PI * 2;
            float ring = R + t * Mathf.Cos(v);
            return Turn(ring * Mathf.Cos(u), ring * Mathf.Sin(u), t * Mathf.Sin(v), axis);
        }

        /// <summary>A web point turned so a z-facing ring faces `axis`: 'y' rotateX(90 deg), 'x' rotateY(90 deg).</summary>
        static Vector3 Turn(float px, float py, float pz, char axis) =>
            axis == 'y' ? new Vector3(px, -pz, py) : axis == 'x' ? new Vector3(pz, py, -px) : new Vector3(px, py, pz);

        /// <summary>vent(w, h, n, t, x, y, z) on the x axis (blockyKit.js :110-118): `n` slats across a w x h panel.</summary>
        public static void Vent(MeshKit kit, float w, float h, int n, float t, float x, float y, float z, Color c)
        {
            float gap = h / (n * 2 - 1);
            for (int i = 0; i < n; i++) Box(kit, t, gap, w, x, y - h / 2 + gap * (i * 2 + 0.5f), z, c);
        }

        /// <summary>
        /// weaponModels.js grip(z0, y0, {h, rake, w, guard}) :64-73: a raked pistol grip below (z0, y0) (its top front
        /// corner) and the trigger guard (with its hole) and the trigger in front of it.
        /// </summary>
        public static void PistolGrip(MeshKit kit, float z0, float y0, bool guard = true, float h = 0.088f, float rake = 0.024f, float w = 0.034f)
        {
            Profile(kit, new float[,] { { z0, y0 }, { z0 + rake, y0 - h }, { z0 + rake + 0.038f, y0 - h + 0.004f }, { z0 + 0.042f, y0 } }, w, Grip);
            if (!guard) return;
            Extrude(kit, new float[,] { { z0 + 0.004f, y0 }, { z0 - 0.036f, y0 }, { z0 - 0.036f, y0 - 0.024f }, { z0 - 0.012f, y0 - 0.034f }, { z0 + 0.008f, y0 - 0.03f } },
                new float[,] { { z0 - 0.002f, y0 - 0.005f }, { z0 - 0.03f, y0 - 0.005f }, { z0 - 0.03f, y0 - 0.021f }, { z0 - 0.012f, y0 - 0.028f }, { z0 + 0.002f, y0 - 0.025f } },
                0.012f, Dark, 0, 0, 0, 0, 0);
            Box(kit, 0.005f, 0.018f, 0.006f, 0, y0 - 0.012f, z0 - 0.014f, Dark);
        }

        // weaponModels.js shotgun() :125-128: the pump's place (part(group, 'pump', ..., [0, -0.026, -0.285])), web rig space.
        static readonly Vector3 PumpAt = new Vector3(0, -0.026f, -0.285f);
        public static Vector3 ThunderclapPumpAt => W(PumpAt);

        /// <summary>
        /// weaponModels.js shotgun() :108-132 (THUNDERCLAP): the coach gun's body; the pump (it slides back after a shot) on
        /// `pump` built around 0 (the viewmodel puts it at ThunderclapPumpAt), or on the body in place when null. Returns the
        /// muzzle. NIGHT12: the body's pieces NIGHT7 left out (the hinge pin, the two seams, the family wedge and sight, the
        /// pistol grip) and the pump's grooves are drawn now.
        /// </summary>
        public static Vector3 Thunderclap(MeshKit body, Color family, MeshKit pump = null)
        {
            Profile(body, new float[,] { { 0.04f, 0.072f }, { -0.13f, 0.072f }, { -0.165f, 0.04f }, { -0.165f, -0.008f }, { -0.02f, -0.008f }, { 0.0f, 0.004f }, { 0.04f, 0.004f } }, 0.07f, Gunmetal);
            TubeZ(body, 0.021f, 0.3f, 0, 0.056f, -0.3f, 10, Gunmetal);
            TubeZ(body, 0.021f, 0.3f, 0, 0.008f, -0.3f, 10, Gunmetal);
            ConeZ(body, 0.04f, 0.023f, 0.07f, 0, 0.058f, -0.465f, 10, Gunmetal);
            ConeZ(body, 0.04f, 0.023f, 0.07f, 0, 0.006f, -0.465f, 10, Gunmetal);
            Profile(body, new float[,] { { 0.04f, 0.066f }, { 0.2f, 0.03f }, { 0.214f, 0.012f }, { 0.214f, -0.078f }, { 0.196f, -0.084f }, { 0.11f, -0.03f }, { 0.04f, -0.004f } }, 0.05f, Bone);
            Box(body, 0.012f, 0.008f, 0.31f, 0, 0.081f, -0.305f, Bone);
            TubeX(body, 0.01f, 0.074f, 0, 0.032f, -0.155f, 8, Bone); // :117 the hinge pin
            Box(body, 0.058f, 0.005f, 0.15f, 0, 0.0745f, -0.05f, Bone);
            Box(body, 0.054f, 0.1f, 0.012f, 0, -0.034f, 0.216f, Dark);
            Box(body, 0.0712f, 0.003f, 0.11f, 0, 0.034f, -0.07f, Seam); // :119 the seams
            Box(body, 0.0712f, 0.036f, 0.003f, 0, 0.05f, -0.12f, Seam);
            TubeZ(body, 0.041f, 0.008f, 0, 0.058f, -0.496f, 10, family);
            TubeZ(body, 0.041f, 0.008f, 0, 0.006f, -0.496f, 10, family);
            Box(body, 0.072f, 0.008f, 0.1f, 0, 0.012f, -0.08f, family);
            Profile(body, new float[,] { { 0.06f, 0.063f }, { 0.15f, 0.044f }, { 0.15f, 0.04f }, { 0.06f, 0.059f } }, 0.052f, family); // :121
            Box(body, 0.006f, 0.008f, 0.006f, 0, 0.089f, -0.45f, family); // :122 the sight
            PistolGrip(body, -0.016f, 0.004f); // :123
            // :125-128 the pump: the grooved fore-end (built around 0 on its own part, else in place)
            MeshKit pk = pump ?? body;
            Vector3 o = pump == null ? PumpAt : Vector3.zero;
            Profile(pk, new float[,] { { 0.07f, 0.016f }, { -0.07f, 0.016f }, { -0.076f, 0.004f }, { -0.07f, -0.018f }, { 0.07f, -0.018f }, { 0.076f, 0.0f } }, 0.06f, Bone, o.x, o.y, o.z);
            foreach (float z in new[] { -0.045f, -0.015f, 0.015f, 0.045f }) Box(pk, 0.0612f, 0.004f, 0.006f, o.x, o.y - 0.004f, o.z + z, Dark);
            return W(0, 0.032f, -0.5f);
        }

        /// <summary>profile(points, w, ..., x): the (z, y) outline extruded across x in [x - w/2, x + w/2] (ear clipping).</summary>
        public static void Profile(MeshKit kit, float[,] pts, float w, Color c, float x = 0, float dy = 0, float dz = 0) => Extrude(kit, pts, null, w, c, x, 0, 0, dy, dz);

        /// <summary>
        /// profile(points, w, bevel, x, holes) (blockyKit.js :250-258) in full: the (z, y) outline extruded across x in
        /// [x - w/2, x + w/2], with one `hole` cut through it (null: none; bridged into the outline for the caps), then turned
        /// about the web z axis by `turnZ` rad (weaponModels.js turnedZ :58) and moved by (tx, ty, tz).
        /// </summary>
        public static void Extrude(MeshKit kit, float[,] pts, float[,] hole, float w, Color c, float x, float turnZ, float tx, float ty, float tz)
        {
            if (Guns52 && hole == null && Chamfer52Extrude(kit, pts, w, c, x, turnZ, tx, ty, tz)) return; // NIGHT52 guns52: chamfered caps, a two-tone panel
            List<Vector2> outer = Loop(pts, true);
            List<Vector2> inner = hole != null ? Loop(hole, false) : null;
            float cs = Mathf.Cos(turnZ), sn = Mathf.Sin(turnZ);
            float x0 = x - w / 2, x1 = x + w / 2;
            Vector3 P(Vector2 p, float px) => W(px * cs - p.y * sn + tx, px * sn + p.y * cs + ty, p.x + tz);
            Vector3 N(float nx, float ny, float nz) => W(nx * cs - ny * sn, nx * sn + ny * cs, nz); // a web normal, turned, to Unity
            // Sides: outward of a CCW outline is its right; the hole runs CW, so its right faces into the hole.
            void Walls(List<Vector2> loop)
            {
                for (int i = 0; i < loop.Count; i++)
                {
                    Vector2 a = loop[i], b = loop[(i + 1) % loop.Count];
                    Vector2 e = (b - a).normalized;
                    kit.Quad(P(a, x0), P(b, x0), P(b, x1), P(a, x1), N(0, -e.x, e.y), c);
                }
            }
            Walls(outer);
            if (inner != null) Walls(inner);
            // Caps (ear clipping on the CCW outline, the hole bridged in)
            List<Vector2> poly = inner != null ? Bridge(outer, inner) : outer;
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
                    kit.Triangle(P(a, x0), P(b, x0), P(d, x0), N(-1, 0, 0), c);
                    kit.Triangle(P(a, x1), P(b, x1), P(d, x1), N(1, 0, 0), c);
                    idx.RemoveAt(k);
                    clipped = true;
                    break;
                }
                if (!clipped) break;
            }
        }

        /// <summary>An outline as (z, y) points, wound CCW (or CW for a hole).</summary>
        static List<Vector2> Loop(float[,] pts, bool ccw)
        {
            int n = pts.GetLength(0);
            var poly = new List<Vector2>(n);
            for (int i = 0; i < n; i++) poly.Add(new Vector2(pts[i, 0], pts[i, 1]));
            float area = 0;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = poly[i], b = poly[(i + 1) % n];
                area += a.x * b.y - b.x * a.y;
            }
            if ((area < 0) == ccw) poly.Reverse();
            return poly;
        }

        /// <summary>The outline with a hole joined in along the shortest bridge that crosses no edge (one polygon to clip).</summary>
        static List<Vector2> Bridge(List<Vector2> outer, List<Vector2> inner)
        {
            int bi = 0, bj = 0;
            float best = float.MaxValue;
            for (int i = 0; i < outer.Count; i++)
            {
                for (int j = 0; j < inner.Count; j++)
                {
                    float d = (outer[i] - inner[j]).sqrMagnitude;
                    if (d >= best || Crosses(outer[i], inner[j], outer) || Crosses(outer[i], inner[j], inner)) continue;
                    best = d;
                    bi = i;
                    bj = j;
                }
            }
            var poly = new List<Vector2>(outer.Count + inner.Count + 2);
            for (int i = 0; i <= bi; i++) poly.Add(outer[i]);
            for (int k = 0; k <= inner.Count; k++) poly.Add(inner[(bj + k) % inner.Count]);
            for (int i = bi; i < outer.Count; i++) poly.Add(outer[i]);
            return poly;
        }

        /// <summary>Whether the segment p-q crosses an edge of `loop` (edges touching p or q do not count).</summary>
        static bool Crosses(Vector2 p, Vector2 q, List<Vector2> loop)
        {
            for (int k = 0; k < loop.Count; k++)
            {
                Vector2 a = loop[k], b = loop[(k + 1) % loop.Count];
                if (a == p || a == q || b == p || b == q) continue;
                if (Cross(q - p, a - p) * Cross(q - p, b - p) < 0 && Cross(b - a, p - a) * Cross(b - a, q - a) < 0) return true;
            }
            return false;
        }

        static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        static bool InTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross(b - a, p - a), d2 = Cross(c - b, p - b), d3 = Cross(a - c, p - c);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }

        /// <summary>
        /// weaponModels.js revolver(): the body (one painted mesh); the cylinder and the gem are separate. The cylinder is built
        /// around 0 (the viewmodel places it at (0, 0.032, -0.062)), or in place when `cylinderInPlace` (one-mesh models).
        /// </summary>
        public static Vector3 Kestrel(MeshKit body, MeshKit cylinder, Color family, bool cylinderInPlace = false)
        {
            Profile(body, new float[,] { { 0.042f, 0.05f }, { 0.03f, 0.068f }, { -0.104f, 0.068f }, { -0.104f, 0.0f }, { -0.02f, -0.002f }, { 0.012f, 0.008f }, { 0.034f, 0.022f } }, 0.03f, Gunmetal);
            TubeZ(body, 0.011f, 0.15f, 0, 0.047f, -0.175f, 10, Gunmetal);
            Profile(body, new float[,] { { -0.1f, 0.038f }, { -0.25f, 0.038f }, { -0.25f, 0.028f }, { -0.236f, 0.019f }, { -0.1f, 0.019f } }, 0.02f, Gunmetal);
            Box(body, 0.01f, 0.009f, 0.15f, 0, 0.061f, -0.175f, Bone);
            Box(body, 0.022f, 0.004f, 0.12f, 0, 0.0695f, -0.035f, Bone);
            if (!Hammer36) Profile(body, HammerPts, 0.012f, Dark); // NIGHT36 crunch36: the hammer is its own moving part (KestrelHammer36)
            Box(body, 0.004f, 0.012f, 0.01f, 0, 0.07f, -0.243f, Dark);
            Profile(body, new float[,] { { 0.012f, 0.01f }, { 0.036f, 0.022f }, { 0.05f, -0.02f }, { 0.058f, -0.058f }, { 0.05f, -0.08f }, { 0.022f, -0.083f }, { 0.014f, -0.05f }, { 0.004f, -0.01f } }, 0.032f, Grip);
            Box(body, 0.0325f, 0.004f, 0.02f, 0, -0.07f, 0.036f, Bone);
            Profile(body, new float[,] { { 0.0f, 0.002f }, { -0.04f, 0.002f }, { -0.04f, -0.018f }, { -0.016f, -0.03f }, { 0.004f, -0.012f } }, 0.01f, Dark);
            Box(body, 0.032f, 0.005f, 0.07f, 0, 0.012f, -0.062f, family);
            TubeZ(body, 0.0125f, 0.012f, 0, 0.047f, -0.237f, 10, family);
            // The cylinder (its own mesh, built around 0, placed at (0, 0.032, -0.062)).
            float cy = cylinderInPlace ? 0.032f : 0, cz = cylinderInPlace ? -0.062f : 0;
            TubeZ(cylinder, 0.026f, 0.064f, 0, cy, cz, 6, Bone);
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2 + Mathf.PI / 2;
                TubeZ(cylinder, 0.0065f, 0.068f, Mathf.Cos(a) * 0.015f, Mathf.Sin(a) * 0.015f + cy, cz, 6, Dark);
            }
            return W(0, 0.047f, -0.25f); // the muzzle
        }

        /// <summary>NIGHT36 F fist36 (the Viewmodel sets it at load with the row on): the gauntlet and the quick punch's fist are built as hands
        /// (Fist36Hand) instead of blocks.</summary>
        public static bool Fist36;

        /// <summary>
        /// NIGHT36 fist36: a clenched low-poly hand in a w x h x d box centred at (cx, cy, cz) (web rig space, -z forward): the back of the hand
        /// (`plate`), the palm under it (`inner`), four curled fingers in three segments each (the middle two a touch further forward), a knuckle
        /// cap on each (`cap`), and the thumb on side `thumbSide` (+1 web +x) folded across the fingers' front.
        /// </summary>
        public static void Fist36Hand(MeshKit kit, float cx, float cy, float cz, float w, float h, float d, int thumbSide, Color plate, Color inner, Color cap)
        {
            Box(kit, w * 0.94f, h * 0.62f, d * 0.55f, cx, cy + h * 0.12f, cz + d * 0.22f, plate); // the back of the hand
            Box(kit, w * 0.9f, h * 0.36f, d * 0.5f, cx, cy - h * 0.26f, cz + d * 0.24f, inner); // the palm
            for (int k = 0; k < 4; k++)
            {
                float fx = cx + (k - 1.5f) * w / 4, fw = w / 4 * 0.88f, reach = k == 1 || k == 2 ? d * 0.03f : 0;
                Box(kit, fw, h * 0.34f, d * 0.38f, fx, cy + h * 0.2f, cz - d * 0.2f - reach, plate); // the first segment, forward
                Box(kit, fw, h * 0.5f, d * 0.2f, fx, cy - h * 0.06f, cz - d * 0.4f - reach, inner); // the second, down the front
                Box(kit, fw * 0.92f, h * 0.22f, d * 0.3f, fx, cy - h * 0.36f, cz - d * 0.22f - reach, inner); // the tip, curled back under
                Box(kit, fw * 1.02f, h * 0.1f, d * 0.16f, fx, cy + h * 0.4f, cz - d * 0.34f - reach, cap); // the knuckle
            }
            Box(kit, w * 0.26f, h * 0.3f, d * 0.42f, cx + thumbSide * w * 0.52f, cy - h * 0.08f, cz + d * 0.08f, inner); // the thumb's base
            Box(kit, w * 0.42f, h * 0.24f, d * 0.2f, cx + thumbSide * w * 0.28f, cy - h * 0.22f, cz - d * 0.56f, plate); // the thumb across the fingers
        }

        /// <summary>The KESTREL's hammer outline (web (z, y)).</summary>
        static readonly float[,] HammerPts = { { 0.028f, 0.066f }, { 0.05f, 0.086f }, { 0.062f, 0.084f }, { 0.06f, 0.074f }, { 0.044f, 0.054f } };

        /// <summary>NIGHT36 F crunch36 (the Viewmodel sets it at load with the row on): the KESTREL's body leaves its hammer out (built by
        /// KestrelHammer36 as a part that falls on the shot and re-cocks).</summary>
        public static bool Hammer36;

        /// <summary>NIGHT36: the hammer's pivot (web rig space: the foot of its outline, where it meets the frame).</summary>
        public static Vector3 KestrelHammerPivot36 => W(0, 0.054f, 0.044f);

        /// <summary>NIGHT36: the KESTREL's hammer built around its pivot (the part turns about its x axis).</summary>
        public static void KestrelHammer36(MeshKit kit)
        {
            var pts = new float[HammerPts.GetLength(0), 2];
            for (int i = 0; i < pts.GetLength(0); i++)
            {
                pts[i, 0] = HammerPts[i, 0] - 0.044f;
                pts[i, 1] = HammerPts[i, 1] - 0.054f;
            }
            Profile(kit, pts, 0.012f, Dark);
        }

        // The moving parts' places (weaponModels.js part(group, name, parts, at)), web rig space: the part meshes are built
        // around 0 and the viewmodel puts them here (StitcherDrumAt, ...), or a one-mesh model draws them in place.
        static readonly Vector3 DrumAt = new Vector3(0, 0.11f, -0.09f); // nailgun() :152
        static readonly Vector3 CometRing = new Vector3(0, 0.05f, -0.52f); // rocket() :177
        static readonly Vector3 LongwaveRing = new Vector3(0, 0.024f, -0.3f); // rail() :207
        public static Vector3 StitcherDrumAt => W(DrumAt);
        public static Vector3 CometRingAt => W(CometRing);
        public static Vector3 LongwaveRingAt => W(LongwaveRing);

        /// <summary>
        /// weaponModels.js nailgun() :134-157 (STITCHER): a round nail drum standing on top, a wide flat barrel block with fins
        /// and three muzzle holes, the heat slit under it. The body (with the gem's socket); the drum (it turns a notch per
        /// nail) on `drum` built around 0, the heat strip on `heat` in place, white (the viewmodel's glow material carries its
        /// colour). Either one null: drawn on the body in place (the heat strip in the family colour, its look when cool).
        /// Returns the muzzle.
        /// </summary>
        public static Vector3 Stitcher(MeshKit body, Color family, MeshKit drum = null, MeshKit heat = null)
        {
            Profile(body, new float[,] { { 0.05f, 0.05f }, { 0.03f, 0.058f }, { -0.2f, 0.058f }, { -0.21f, 0.04f }, { -0.21f, -0.012f }, { -0.03f, -0.012f }, { 0.0f, 0.004f }, { 0.05f, 0.004f } }, 0.058f, Dark); // :139
            Profile(body, new float[,] { { -0.2f, 0.062f }, { -0.4f, 0.062f }, { -0.43f, 0.052f }, { -0.43f, 0.03f }, { -0.2f, 0.026f } }, 0.07f, Gunmetal); // :140
            Box(body, 0.03f, 0.022f, 0.04f, 0, 0.066f, -0.09f, Gunmetal);
            foreach (float z in new[] { -0.25f, -0.28f, -0.31f, -0.34f, -0.37f }) Box(body, 0.074f, 0.016f, 0.005f, 0, 0.07f, z, Bone); // :141 the fins
            Vent(body, 0.1f, 0.035f, 3, 0.004f, 0.03f, 0.024f, -0.11f, Bone); // :142
            foreach (float x in new[] { -0.02f, 0f, 0.02f }) Box(body, 0.008f, 0.008f, 0.006f, x, 0.041f, -0.43f, Seam); // :143 the three muzzle holes
            Box(body, 0.072f, 0.006f, 0.17f, 0, 0.034f, -0.31f, family); // :144
            Box(body, 0.05f, 0.005f, 0.07f, 0, 0.0605f, 0.005f, family);
            PistolGrip(body, -0.016f, 0.004f); // :145
            // :148-152 the drum (G2: it turns a notch per nail; its spokes show it)
            MeshKit dk = drum ?? body;
            Vector3 o = drum == null ? DrumAt : Vector3.zero;
            TubeX(dk, 0.046f, 0.05f, o.x, o.y, o.z, 12, Bone);
            TubeX(dk, 0.016f, 0.056f, o.x, o.y, o.z, 8, Seam);
            Box(dk, 0.052f, 0.07f, 0.008f, o.x, o.y, o.z, Seam);
            Box(dk, 0.052f, 0.008f, 0.07f, o.x, o.y, o.z, Seam);
            TubeX(dk, 0.0475f, 0.012f, o.x, o.y, o.z, 12, family);
            // :153 the heat strip at (0, 0.02, -0.31) (vcGlow(1.4) on white vertex colours)
            Box(heat ?? body, 0.074f, 0.012f, 0.15f, 0, 0.02f, -0.31f, heat != null ? Color.white : family);
            SocketFor(body, "nailgun"); // :154
            return W(0, 0.045f, -0.43f); // :156
        }

        /// <summary>
        /// weaponModels.js rocket() :159-181 (COMET): a fat launch tube with a flared mouth and a rear venturi, four stabiliser
        /// fins at the back, a sight block, two handles, a turning ring. The body (with the gem's socket); the ring (it turns)
        /// on `ring` built around 0, or on the body in place when null. Returns the muzzle.
        /// </summary>
        public static Vector3 Comet(MeshKit body, Color family, MeshKit ring = null)
        {
            // :163-166 the fins: the outline turned about z by k x 90 deg, out 0.052 m from the tube's axis at y 0.05
            for (int k = 0; k < 4; k++)
            {
                float a = k * Mathf.PI / 2;
                Extrude(body, new float[,] { { 0.04f, 0.0f }, { 0.13f, 0.0f }, { 0.13f, 0.05f }, { 0.1f, 0.05f } }, null, 0.006f, family, 0, a, -Mathf.Sin(a) * 0.052f, 0.05f + Mathf.Cos(a) * 0.052f, 0);
            }
            TubeZ(body, 0.056f, 0.62f, 0, 0.05f, -0.27f, 12, Gunmetal); // :168
            ConeZ(body, 0.084f, 0.058f, 0.06f, 0, 0.05f, -0.6f, 12, Bone); // :169 the mouth
            ConeZ(body, 0.05f, 0.076f, 0.1f, 0, 0.05f, 0.09f, 12, Bone); // the venturi
            TubeZ(body, 0.059f, 0.02f, 0, 0.05f, -0.44f, 12, Bone); // :170
            TubeZ(body, 0.059f, 0.02f, 0, 0.05f, -0.1f, 12, Bone);
            Profile(body, new float[,] { { -0.14f, 0.104f }, { -0.3f, 0.104f }, { -0.3f, 0.12f }, { -0.27f, 0.124f }, { -0.27f, 0.16f }, { -0.17f, 0.16f }, { -0.15f, 0.145f }, { -0.14f, 0.12f } }, 0.03f, Dark); // :171 the sight block
            TubeZ(body, 0.05f, 0.004f, 0, 0.05f, 0.138f, 12, Dark); // :172
            Profile(body, new float[,] { { -0.29f, 0.0f }, { -0.284f, -0.07f }, { -0.314f, -0.07f }, { -0.322f, 0.0f } }, 0.03f, Grip); // :173 the front handle
            TubeZ(body, 0.0595f, 0.012f, 0, 0.05f, -0.36f, 12, family); // :174
            Box(body, 0.026f, 0.024f, 0.004f, 0, 0.142f, -0.272f, family);
            PistolGrip(body, -0.016f, -0.002f, guard: false); // :175
            // :177 the ring: TorusGeometry(0.078, 0.01, 4, 16) in the family colour, on the body material
            Vector3 o = ring == null ? CometRing : Vector3.zero;
            Torus(ring ?? body, 0.078f, 0.01f, 4, 16, o.x, o.y, o.z, family);
            SocketFor(body, "rocket"); // :178
            return W(0, 0.05f, -0.63f); // :180
        }

        /// <summary>
        /// weaponModels.js rail() :183-211 (LONGWAVE): two long blade rails with the charge glowing in the gap between them, a
        /// finned capacitor block, a skeleton stock. The body (with the gem's socket); the charge bar and the capacitor seams
        /// on `glow` in place, the ring (it turns) on `ring` built around 0, both white (they share the gun's one glow
        /// material, vcGlow(1.2), which the viewmodel colours). Either one null: drawn on the body in place in the family
        /// colour. Returns the muzzle.
        /// </summary>
        public static Vector3 Longwave(MeshKit body, Color family, MeshKit glow = null, MeshKit ring = null)
        {
            Profile(body, new float[,] { { 0.07f, 0.06f }, { 0.05f, 0.072f }, { -0.2f, 0.072f }, { -0.24f, 0.05f }, { -0.24f, -0.03f }, { -0.03f, -0.03f }, { 0.0f, 0.004f }, { 0.07f, 0.004f } }, 0.064f, Dark); // :188
            Profile(body, new float[,] { { -0.16f, 0.084f }, { -0.74f, 0.084f }, { -0.78f, 0.068f }, { -0.78f, 0.05f }, { -0.16f, 0.047f } }, 0.026f, Gunmetal); // :190 the upper rail
            Profile(body, new float[,] { { -0.16f, -0.003f }, { -0.78f, -0.004f }, { -0.78f, -0.022f }, { -0.74f, -0.04f }, { -0.16f, -0.04f } }, 0.026f, Gunmetal); // :191 the lower rail
            foreach (float z in new[] { -0.03f, -0.06f, -0.09f, -0.12f, -0.15f, -0.18f }) Box(body, 0.07f, 0.022f, 0.006f, 0, 0.082f, z, Gunmetal); // :192 the capacitor fins
            foreach (float z in new[] { -0.38f, -0.6f }) // :193 the rail posts
            {
                Box(body, 0.006f, 0.13f, 0.016f, 0.016f, 0.022f, z, Bone);
                Box(body, 0.006f, 0.13f, 0.016f, -0.016f, 0.022f, z, Bone);
            }
            Box(body, 0.06f, 0.005f, 0.22f, 0, 0.0745f, -0.07f, Bone); // :194
            Extrude(body, new float[,] { { 0.07f, 0.05f }, { 0.24f, 0.036f }, { 0.25f, 0.02f }, { 0.25f, -0.05f }, { 0.235f, -0.056f }, { 0.07f, -0.01f } },
                new float[,] { { 0.1f, 0.03f }, { 0.215f, 0.02f }, { 0.215f, -0.03f }, { 0.1f, -0.004f } }, 0.044f, Gunmetal, 0, 0, 0, 0, 0); // :195-196 the skeleton stock
            Box(body, 0.028f, 0.006f, 0.06f, 0, 0.087f, -0.71f, family); // :197
            Box(body, 0.066f, 0.006f, 0.07f, 0, 0.075f, -0.22f, family);
            PistolGrip(body, -0.016f, 0.004f); // :198
            // :203-206 the charge at (0, 0.024, -0.47): the bar in the gap and the three capacitor seams
            MeshKit gk = glow ?? body;
            Color gc = glow != null ? Color.white : family;
            Box(gk, 0.012f, 0.03f, 0.6f, 0, 0.024f, -0.47f, gc);
            foreach (float dz in new[] { 0.27f, 0.37f, 0.47f }) Box(gk, 0.0655f, 0.004f, 0.004f, 0, 0.024f - 0.004f, -0.47f + dz, gc);
            // :207 the ring: TorusGeometry(0.06, 0.007, 4, 16)
            Vector3 o = ring == null ? LongwaveRing : Vector3.zero;
            Torus(ring ?? body, 0.06f, 0.007f, 4, 16, o.x, o.y, o.z, ring != null ? Color.white : family);
            SocketFor(body, "rail"); // :208
            return W(0, 0.022f, -0.78f); // :210
        }

        /// <summary>
        /// weaponModels.js socketGem(r, hex, x, y, z, {axis}) per gun (web rig space): revolver :102, shotgun :129, nailgun :154,
        /// rocket :178, rail :208. False for an id without one here.
        /// </summary>
        static bool SocketSpec(string id, out float r, out float x, out float y, out float z, out char axis)
        {
            axis = 'y';
            x = 0;
            switch (id)
            {
                case "revolver": r = 0.011f; y = 0.075f; z = -0.06f; return true;
                case "shotgun": r = 0.02f; y = 0.077f; z = -0.06f; return true;
                case "nailgun": r = 0.016f; x = 0.031f; y = 0.022f; z = -0.035f; axis = 'x'; return true;
                case "rocket": r = 0.022f; y = 0.114f; z = -0.04f; return true;
                case "rail": r = 0.02f; y = 0.08f; z = 0.03f; return true;
                default: r = y = z = 0; return false;
            }
        }

        /// <summary>A gun's core gem: its centre (Unity rig space) and socketGem's r (the gem is r x 0.82). False for none.</summary>
        public static bool GemOf(string id, out Vector3 at, out float r)
        {
            bool ok = SocketSpec(id, out r, out float x, out float y, out float z, out _);
            at = W(x, y, z);
            return ok;
        }

        /// <summary>socketGem's socket (blockyKit.js :324-328): a torus (r x 1.05, tube r x 0.28, 4 x 10) in the dark tone.</summary>
        static void SocketFor(MeshKit kit, string id)
        {
            if (SocketSpec(id, out float r, out float x, out float y, out float z, out char axis)) Torus(kit, r * 1.05f, r * 0.28f, 4, 10, x, y, z, Dark, axis);
        }

        /// <summary>socketGem's gem (blockyKit.js :329-331): OctahedronGeometry(r x 0.82) scaled (1, 0.8, 1), centred at `at` (Unity).</summary>
        static void Octahedron(MeshKit kit, float radius, Vector3 at, Color c)
        {
            for (int s = 0; s < 8; s++)
            {
                var px = new Vector3((s & 1) == 0 ? radius : -radius, 0, 0);
                var py = new Vector3(0, (s & 2) == 0 ? radius * 0.8f : -radius * 0.8f, 0);
                var pz = new Vector3(0, 0, (s & 4) == 0 ? radius : -radius);
                Vector3 n = Vector3.Cross(py - px, pz - px).normalized;
                if (Vector3.Dot(n, px + py + pz) < 0) n = -n;
                kit.Triangle(at + px, at + py, at + pz, n, c);
            }
        }

        /// <summary>
        /// A gun's whole kit model on one mesh (the wall buys' holograms): the body, the moving parts and glows in place (a glow
        /// in the family colour, its look at rest), the socket and the gem (weaponModels.js socketGem). False for an id it has
        /// no model for (supported: revolver, shotgun, nailgun, rocket, rail). NIGHT12: in a finish's body tones (null: FACTORY).
        /// </summary>
        public static bool ForWeapon(string id, MeshKit kit, Color tint, Finishes.Palette palette)
        {
            using (Finish(palette)) return ForWeapon(id, kit, tint);
        }

        /// <summary>ForWeapon in the tones in force (FACTORY outside a Finish scope: the Mystery Box keeps them).</summary>
        public static bool ForWeapon(string id, MeshKit kit, Color tint)
        {
            switch (id)
            {
                case "revolver":
                    Kestrel(kit, kit, tint, cylinderInPlace: true);
                    SocketFor(kit, id); // the KESTREL / THUNDERCLAP builders leave the socket to the viewmodel's gem
                    break;
                case "shotgun":
                    Thunderclap(kit, tint);
                    SocketFor(kit, id);
                    break;
                case "nailgun": Stitcher(kit, tint); break;
                case "rocket": Comet(kit, tint); break;
                case "rail": Longwave(kit, tint); break;
                default: return HandsPortKitWhole.ForWeapon(id, kit, tint); // NIGHT19 (builder V): the ZAPPER's and the BOUNCER's holograms
            }
            if (GemOf(id, out Vector3 at, out float r)) Octahedron(kit, r * 0.82f, at, tint);
            return true;
        }

        /// <summary>weaponModels.js gauntlet(): the body; the pulse strip is the glow (separate, emissive).</summary>
        public static Vector3 Gauntlet(MeshKit body, MeshKit glow, Color family)
        {
            if (Fist36)
            {
                // NIGHT36 F fist36 (REVIEW.md root cause 3: "the gauntlet is a 10 cm block"): a clenched armoured hand on a forearm, the gauntlet's colours
                // (gunmetal plates, bone inner, the family cuff), the pulse strip across the knuckles
                // n36-r3: the hand in its own colours (gunmetal plates, bone fingers, dark knuckle caps and cuff); cyan only on the pulse strip
                Fist36Hand(body, 0, 0, -0.065f, 0.094f, 0.084f, 0.1f, -1, Gunmetal, Bone, Dark);
                Box(body, 0.086f, 0.078f, 0.15f, 0, -0.004f, 0.065f, Gunmetal); // the forearm
                Box(body, 0.06f, 0.01f, 0.12f, 0, 0.04f, 0.07f, Dark); // its top plate
                Box(body, 0.098f, 0.09f, 0.016f, 0, -0.004f, -0.006f, Dark); // the cuff at the wrist
                TubeZ(body, 0.007f, 0.11f, 0.05f, 0.02f, 0.07f, 6, Bone); // the side pistons
                TubeZ(body, 0.007f, 0.11f, 0.05f, -0.02f, 0.07f, 6, Bone);
                Box(body, 0.098f, 0.09f, 0.012f, 0, -0.004f, 0.145f, Dark); // the back cap
                Box(glow, 0.08f, 0.008f, 0.006f, 0, 0.034f, -0.112f, Color.white); // the pulse strip on the knuckles
                return W(0, 0, -0.12f);
            }
            Profile(body, new float[,] { { -0.015f, 0.038f }, { -0.09f, 0.038f }, { -0.11f, 0.02f }, { -0.11f, -0.025f }, { -0.09f, -0.038f }, { -0.015f, -0.038f } }, 0.08f, Bone);
            TubeZ(body, 0.007f, 0.09f, 0.05f, 0.02f, 0.01f, 6, Bone);
            TubeZ(body, 0.007f, 0.09f, 0.05f, -0.02f, 0.01f, 6, Bone);
            foreach (float bx in new[] { -0.03f, -0.01f, 0.01f, 0.03f }) Box(body, 0.017f, 0.022f, 0.03f, bx, 0.03f, -0.096f, Gunmetal);
            Profile(body, new float[,] { { 0.08f, 0.044f }, { -0.015f, 0.046f }, { -0.022f, 0.03f }, { -0.022f, -0.035f }, { -0.015f, -0.046f }, { 0.08f, -0.044f } }, 0.094f, Gunmetal);
            Profile(body, new float[,] { { -0.02f, 0.044f }, { -0.08f, 0.044f }, { -0.075f, 0.052f }, { -0.025f, 0.054f } }, 0.06f, Dark);
            Box(body, 0.098f, 0.096f, 0.012f, 0, 0, 0.08f, Dark);
            foreach (float by in new[] { -0.02f, 0f, 0.02f }) Box(body, 0.0805f, 0.003f, 0.004f, 0, by, -0.11f, Dark);
            Box(body, 0.096f, 0.012f, 0.012f, 0, 0, -0.01f, family);
            Box(glow, 0.07f, 0.008f, 0.006f, 0, 0.01f, -0.112f, Color.white);
            return W(0, 0, -0.11f);
        }

        // --- NIGHT16 U3-29.6 (builder V): the left arm's parts ----------------------------------------------------------

        /// <summary>One row of config.viewmodel.arm (viewmodel.js buildParts: [role, shape, sx, sy, sz, x, y, z]).</summary>
        public struct ArmPart
        {
            public string role, shape;
            public float sx, sy, sz, x, y, z;
        }

        /// <summary>viewmodel.js buildParts(v.arm): the arm's boxes / cylinders in its role colours (body, trim, accent).</summary>
        public static void Arm(MeshKit kit, ArmPart[] parts, Color body, Color trim, Color accent, int segments)
        {
            // viewmodel.js :86-95 buildParts: rows.map(([role, shape, sx, sy, sz, x, y, z]) => cyl ? CylinderGeometry(sx / 2,
            // sx / 2, sz, segments).rotateX(PI / 2) : BoxGeometry(sx, sy, sz), at (x, y, z), in materials[role]
            // (:1011-1013 body = colors.vmBody, trim = colors.vmTrim, accent = colors.usable); vertex colours here.
            foreach (ArmPart p in parts)
            {
                Color c = p.role == "trim" ? trim : p.role == "accent" ? accent : body;
                if (p.shape == "cyl") TubeZ(kit, p.sx / 2, p.sz, p.x, p.y, p.z, segments, c);
                else Box(kit, p.sx, p.sy, p.sz, p.x, p.y, p.z, c);
            }
        }

        // NIGHT17 U3-30.6 round 1 ("a tan plank, not a fist"): the port's fist from config.viewmodel.arm's rows [fist, thumb,
        // cuff, forearm] (the web's order): the fist block scaled up (it grows back, its front face 0.5 x the growth ahead),
        // the thumb kept on its side, the cuff and forearm moved back with it and thinned, knuckle ridges along the fist's
        // top-front edge and two finger folds across its front (UnityTuning.HandsFist* / HandsKnuckleRidge* / HandsFingerFold*).

        /// <summary>The port's fist (config.viewmodel.arm row 0 x HandsFistScale).</summary>
        public static ArmPart FistOf(ArmPart[] parts)
        {
            ArmPart f = parts[0];
            if (parts.Length != 4) return f;
            f.sx *= UnityTuning.HandsFistScaleX;
            f.sy *= UnityTuning.HandsFistScaleY;
            f.sz *= UnityTuning.HandsFistScaleZ;
            return f;
        }

        /// <summary>How far back the fist's growth pushes the cuff and the forearm (m, web +z).</summary>
        static float FistGrowth(ArmPart[] parts) => (FistOf(parts).sz - parts[0].sz) / 2;

        /// <summary>The port's forearm (the last row, thinned and moved back with the fist).</summary>
        public static ArmPart ForearmOf(ArmPart[] parts)
        {
            ArmPart f = parts[parts.Length - 1];
            if (parts.Length != 4) return f;
            f.sx *= UnityTuning.HandsForearmThin;
            f.sy *= UnityTuning.HandsForearmThin;
            f.z += FistGrowth(parts);
            return f;
        }

        /// <summary>The port's left arm: Arm's rows reshaped into a fist that reads (a web-shaped config of other than 4 rows
        /// builds as the web's).</summary>
        public static void ArmFist(MeshKit kit, ArmPart[] parts, Color body, Color trim, Color accent, int segments)
        {
            if (parts.Length != 4)
            {
                Arm(kit, parts, body, trim, accent, segments);
                return;
            }
            ArmPart fist = FistOf(parts), thumb = parts[1], cuff = parts[2], fore = ForearmOf(parts);
            float grow = FistGrowth(parts);
            thumb.x *= fist.sx / parts[0].sx; // kept on the fist's side
            thumb.y *= fist.sy / parts[0].sy;
            cuff.z += grow;
            cuff.sx = fore.sx * UnityTuning.HandsCuffOver;
            cuff.sy = fore.sy * UnityTuning.HandsCuffOver;
            var rows = new[] { fist, thumb, cuff, fore };
            for (int i = 0; i < rows.Length; i++) rows[i].role = parts[i].role;
            if (Fist36)
            {
                // NIGHT36 fist36: the cuff and the sleeve as before; the fist a hand in the fist's box (the thumb on the config thumb's side)
                Arm(kit, new[] { rows[2], rows[3] }, body, trim, accent, segments);
                Fist36Hand(kit, fist.x, fist.y, fist.z, fist.sx, fist.sy, fist.sz, thumb.x >= fist.x ? 1 : -1, trim, Color.Lerp(trim, body, 0.45f), accent);
                Box(kit, UnityTuning.HandsSleeveEdge, UnityTuning.HandsSleeveEdge, fore.sz * 0.94f, fore.x + fore.sx / 2, fore.y + fore.sy / 2, fore.z, accent);
                return;
            }
            Arm(kit, rows, body, trim, accent, segments);
            // the knuckles: four ridges along the top-front edge, standing off the front face
            float front = fist.z - fist.sz / 2, kw = fist.sx / 4 * 0.86f;
            for (int k = 0; k < 4; k++)
            {
                float kx = fist.x + (k - 1.5f) * fist.sx / 4;
                Box(kit, kw, UnityTuning.HandsKnuckleRidgeH, UnityTuning.HandsKnuckleRidgeD, kx, fist.y + fist.sy / 2 - UnityTuning.HandsKnuckleRidgeH / 2,
                    front + UnityTuning.HandsKnuckleRidgeD / 2 - UnityTuning.HandsKnuckleRidgeProud, trim);
            }
            // round 5: a cyan edge along the sleeve's inner top corner (standing half proud of it), the hands' own accent
            float edge = UnityTuning.HandsSleeveEdge;
            Box(kit, edge, edge, fore.sz * 0.94f, fore.x + fore.sx / 2, fore.y + fore.sy / 2, fore.z, accent); // the inner top edge: the one the camera sees
            // the finger folds across the front face (the forearm's darker body colour)
            foreach (float fy in new[] { UnityTuning.HandsFingerFoldY1, UnityTuning.HandsFingerFoldY2 })
                Box(kit, fist.sx * 0.92f, UnityTuning.HandsFingerFold, UnityTuning.HandsFingerFold, fist.x, fist.y + fy, front - UnityTuning.HandsFingerFold / 2 + 0.001f, body);
        }

        /// <summary>Port-only: the knuckle plate on the fist's front face (the parry flash; drawn only while it flashes).</summary>
        public static void ArmKnuckles(MeshKit kit, ArmPart fist, Color c)
        {
            // the fist's front face is at z - sz / 2 (web -z is forward); the plate sits just proud of it
            float inset = (float)UnityTuning.HandsKnuckleInset, depth = UnityTuning.HandsKnuckleDepth;
            Box(kit, fist.sx - 2 * inset, fist.sy - 2 * inset, depth, fist.x, fist.y, fist.z - fist.sz / 2 - depth / 2, c);
        }
    }
}
