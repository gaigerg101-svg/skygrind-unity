// NIGHT43 builder A1 (models43, a view row: design.n43.models43; NIGHT43.md card A1; GG's G21 / G23: "keep the pill silhouette", "better-looking
// visors ... different visor shapes, shield designs, armor details, attachments", "should not simply be perfect pill shapes or collections of
// geometric primitives"): the smooth procedural meshes the enemies are rebuilt from. Every mesh is built ONCE on first use and cached (a static
// table; a mesh Unity destroyed, e.g. across a domain reload, is rebuilt), so nothing is allocated per frame. The shapes:
// - superellipsoids (Barr's: |x|^(2/e2) ... ; e = 1 an ellipsoid, e < 1 a rounded box): the helmets, the soft bodies, the lenses;
// - lathes (a profile turned about y): the waisted pill (the capsule primitive's replacement, same unit space: r 0.5, y -1..1), the rings, the
//   spikes, the pauldron domes, the drone's saucer, the burrower's segments and its ringed mouth;
// - visor patches: a band ON a helmet's own superellipsoid (the same exponents, so it sits on the surface it is anchored to), raised as a
//   pillow (thickest in the middle, flush at the edges: no walls, no gap): a SLIT (thin, wide), a CHEVRON (a V: the charger's brow), a PLATE
//   (tall: the warden), a double slit (the tank);
// - the warden's shield: a bent rounded slab and a tube frame swept along its rounded-rectangle edge.
// Normals: analytic on the superellipsoids, by the profile on the lathes, by finite differences on the patches (oriented by the surface's).
// Winding: every triangle is turned so Cross(b - a, c - a) points along its vertices' normals (Unity's front face), so no builder can get it
// backwards. Unit spaces: a "cube" mesh fits -0.5..0.5 (it replaces a cube primitive under the same transform: the views keep their sizes).
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class Mesh43 // NIGHT47 F: partial (visor47: Mesh43.Visor47.cs)
    {
        // The meshes by id (a part spec names one: UnityTuning.Models43.cs).
        public const int Pill = 0; // the capsule primitive's replacement (r 0.5, y -1..1): a waist, a chest, rounded shoulders
        public const int Helmet = 1; // a rounded box head (cube space)
        public const int SoftBox = 2; // a softer rounded box (cube space): bodies, skirts
        public const int VisorSlit = 3; // on Helmet: a thin wide band
        public const int VisorChevron = 4; // on Brow: a V band, low in the middle
        public const int Ball = 5; // a smooth sphere (cube space: r 0.5): lenses, cores, sacs
        public const int VisorPlate = 6; // on Helmet: a tall face plate
        public const int Pauldron = 7; // a dome with a rolled lip (cube space)
        public const int Ring = 8; // a torus (R 0.4, tube 0.1; cube space)
        public const int Spike = 9; // a curved cone, tip up (cube space)
        public const int ShieldSlab = 10; // a bent rounded slab (cube space)
        public const int ShieldFrame = 11; // a tube along the slab's edge (cube space)
        public const int Brow = 12; // a heavy brow helmet (cube space): flatter top, the charger's
        public const int BodyBox = 13; // a bulky rounded body (cube space): the charger's ram
        public const int VisorSlit2 = 14; // on Helmet: two slits (the tank)
        public const int Eyes3 = 15; // three small lenses in a cluster (cube space): the swarmer
        public const int Fins2 = 16; // two swept fins, mirrored (cube space): the swarmer's back
        public const int Pauldrons2 = 17; // a pair of pauldrons at x -/+ 0.32 (cube space)
        public const int Rings2 = 18; // two rings stacked at y -/+ 0.3 (cube space): the spitter's tank straps
        public const int Saucer = 19; // a lens body (r 1 at the rim; scaled by the view to the drone's radius)
        public const int Lenses3 = 20; // three lenses around a rim (r 0.5, cube space): the drone's eyes
        public const int Segment = 21; // a burrower body segment: a fat ring with a crest (cube space)
        public const int Mouth = 22; // the burrower's ringed mouth: three nested lip rings (cube space)
        public const int Tooth = 23; // a hooked fang (cube space)
        public const int Teeth = 24; // six fangs round the mouth's rim, leaning in (cube space)
        public const int Crucible = 25; // a thick-walled pouring crucible: a bowl with a rolled lip and its cavity (cube space): THE RING's landmark
        public const int Count = 26;

        static readonly Mesh[] cache = new Mesh[Count];

        /// <summary>The mesh `id` (built once, then cached).</summary>
        public static Mesh Get(int id)
        {
            Mesh m = cache[id];
            if (m == null)
            {
                m = Build(id);
                m.name = "mesh43 " + id;
                cache[id] = m;
            }
            return m;
        }

        /// <summary>A copy of mesh `id` scaled by `s` (a view's own size, made once by the view).</summary>
        public static Mesh Scaled(int id, float s) => Combine("mesh43 " + id + " x" + s, (Get(id), Matrix4x4.Scale(Vector3.one * s)));

        // Superellipsoid exponents (lat e1, lon e2) of the shapes the visors sit on.
        public const float HelmetE1 = 0.62f, HelmetE2 = 0.7f, BrowE1 = 0.42f, BrowE2 = 0.62f;

        static Mesh Build(int id)
        {
            switch (id)
            {
                case Pill: return PillMesh();
                case Helmet: return Super(HelmetE1, HelmetE2, 18, 24);
                case SoftBox: return Super(0.5f, 0.55f, 16, 24);
                case VisorSlit: return Patch(HelmetE1, HelmetE2, 0.04f, 0.05f, 1.15f, 0f, 0.07f); // ~0.14 of the head's height (e1 < 1 stretches latitude near the equator)
                case VisorChevron: return Patch(BrowE1, BrowE2, -0.16f, 0.13f, 1.0f, 0.42f, 0.09f);
                case Ball: return Super(1, 1, 12, 20); // low enough for the composites (three lenses in one mesh)
                case VisorPlate: return Patch(HelmetE1, HelmetE2, -0.02f, 0.36f, 0.82f, 0f, 0.06f);
                case Pauldron: return Lathe(DomeProfile(), 24);
                case Ring: return Lathe(Circle(0.4f, 0.1f, 12), 28);
                case Spike: return Lathe(SpikeProfile(), 16);
                case ShieldSlab: return Slab();
                case ShieldFrame: return Frame();
                case Brow: return Super(BrowE1, BrowE2, 16, 24);
                case BodyBox: return Super(0.55f, 0.8f, 18, 28);
                case VisorSlit2:
                    return Combine("", (Patch(HelmetE1, HelmetE2, 0.2f, 0.07f, 1.05f, 0f, 0.06f), Matrix4x4.identity),
                        (Patch(HelmetE1, HelmetE2, -0.08f, 0.07f, 1.05f, 0f, 0.06f), Matrix4x4.identity));
                case Eyes3:
                    return Combine("", (Get(Ball), Matrix4x4.TRS(new Vector3(-0.3f, 0.12f, 0), Quaternion.identity, Vector3.one * 0.4f)),
                        (Get(Ball), Matrix4x4.TRS(new Vector3(0.3f, 0.12f, 0), Quaternion.identity, Vector3.one * 0.4f)),
                        (Get(Ball), Matrix4x4.TRS(new Vector3(0, -0.2f, 0.05f), Quaternion.identity, Vector3.one * 0.52f)));
                case Fins2:
                    return Combine("", (Get(Spike), Matrix4x4.TRS(new Vector3(-0.3f, 0, 0), Quaternion.Euler(-55, 0, 28), new Vector3(0.35f, 1, 0.12f))),
                        (Get(Spike), Matrix4x4.TRS(new Vector3(0.3f, 0, 0), Quaternion.Euler(-55, 0, -28), new Vector3(0.35f, 1, 0.12f))));
                case Pauldrons2:
                    return Combine("", (Get(Pauldron), Matrix4x4.TRS(new Vector3(-0.32f, 0, 0), Quaternion.Euler(0, 0, 18), new Vector3(0.36f, 1, 0.8f))),
                        (Get(Pauldron), Matrix4x4.TRS(new Vector3(0.32f, 0, 0), Quaternion.Euler(0, 0, -18), new Vector3(0.36f, 1, 0.8f))));
                case Rings2:
                    return Combine("", (Get(Ring), Matrix4x4.TRS(new Vector3(0, 0.3f, 0), Quaternion.identity, new Vector3(1, 0.5f, 1))),
                        (Get(Ring), Matrix4x4.TRS(new Vector3(0, -0.3f, 0), Quaternion.identity, new Vector3(1, 0.5f, 1))));
                case Saucer: return Lathe(SaucerProfile(), 32);
                case Lenses3:
                    {
                        var parts = new (Mesh, Matrix4x4)[3];
                        for (int k = 0; k < 3; k++)
                        {
                            float a = k * 120 * Mathf.Deg2Rad;
                            parts[k] = (Get(Ball), Matrix4x4.TRS(new Vector3(Mathf.Sin(a) * 0.5f, 0, Mathf.Cos(a) * 0.5f), Quaternion.Euler(0, k * 120, 0), new Vector3(0.2f, 0.2f, 0.12f)));
                        }
                        return Combine("", parts);
                    }
                case Segment: return Lathe(SegmentProfile(), 24);
                case Mouth:
                    return Combine("", (Lathe(Circle(0.42f, 0.08f, 10), 28), Matrix4x4.TRS(new Vector3(0, 0.38f, 0), Quaternion.identity, Vector3.one)),
                        (Lathe(Circle(0.32f, 0.07f, 10), 24), Matrix4x4.TRS(new Vector3(0, 0.3f, 0), Quaternion.identity, Vector3.one)),
                        (Lathe(Circle(0.22f, 0.06f, 10), 20), Matrix4x4.TRS(new Vector3(0, 0.22f, 0), Quaternion.identity, Vector3.one)));
                case Tooth: return Lathe(ToothProfile(), 10);
                case Crucible:
                    // from the bottom's centre out and up the outer wall, over the rolled lip, down the inner wall to the cavity's floor
                    return Lathe(new[]
                    {
                        new Vector2(0, -0.5f), new Vector2(0.22f, -0.5f), new Vector2(0.36f, -0.42f), new Vector2(0.45f, -0.2f), new Vector2(0.49f, 0.1f),
                        new Vector2(0.5f, 0.32f), new Vector2(0.54f, 0.42f), new Vector2(0.52f, 0.49f), new Vector2(0.46f, 0.47f), new Vector2(0.43f, 0.3f),
                        new Vector2(0.4f, 0.0f), new Vector2(0.3f, -0.26f), new Vector2(0.14f, -0.36f), new Vector2(0, -0.38f),
                    }, 32);
                case Teeth:
                    {
                        var parts = new (Mesh, Matrix4x4)[6];
                        for (int k = 0; k < 6; k++)
                        {
                            float a = k * 60;
                            Quaternion q = Quaternion.Euler(0, a, 0);
                            parts[k] = (Get(Tooth), Matrix4x4.TRS(q * new Vector3(0, 0.25f, 0.36f), q * Quaternion.Euler(-28, 0, 0), new Vector3(0.16f, 0.4f, 0.16f)));
                        }
                        return Combine("", parts);
                    }
            }
            return Super(1, 1, 8, 12);
        }

        // ---------------------------------------------------------------- the builder

        sealed class B
        {
            public readonly List<Vector3> v = new List<Vector3>();
            public readonly List<Vector3> n = new List<Vector3>();
            public readonly List<int> t = new List<int>();

            public int Add(Vector3 p, Vector3 nn)
            {
                v.Add(p);
                n.Add(nn.sqrMagnitude > 1e-12f ? nn.normalized : Vector3.up);
                return v.Count - 1;
            }

            /// <summary>A triangle turned to face along its vertices' normals; a zero-area one is dropped (the poles).</summary>
            public void Tri(int a, int b, int c)
            {
                Vector3 cr = Vector3.Cross(v[b] - v[a], v[c] - v[a]);
                if (cr.sqrMagnitude < 1e-14f) return;
                if (Vector3.Dot(cr, n[a] + n[b] + n[c]) < 0) (b, c) = (c, b);
                t.Add(a);
                t.Add(b);
                t.Add(c);
            }

            public void Quad(int a, int b, int c, int d)
            {
                Tri(a, b, c);
                Tri(a, c, d);
            }

            public Mesh ToMesh()
            {
                var m = new Mesh();
                if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                m.SetVertices(v);
                m.SetNormals(n);
                m.SetTriangles(t, 0);
                m.RecalculateBounds();
                return m;
            }
        }

        static float Sp(float w, float e)
        {
            float a = Mathf.Abs(w);
            return a < 1e-7f ? 0 : Mathf.Sign(w) * Mathf.Pow(a, e);
        }

        /// <summary>A point of the unit-cube superellipsoid (half sizes 0.5) at latitude `lat`, longitude `lon` (lon pi/2 = +z, the front).</summary>
        public static Vector3 SuperPoint(float e1, float e2, float lat, float lon)
        {
            float cv = Sp(Mathf.Cos(lat), e1), sv = Sp(Mathf.Sin(lat), e1);
            return new Vector3(0.5f * cv * Sp(Mathf.Cos(lon), e2), 0.5f * sv, 0.5f * cv * Sp(Mathf.Sin(lon), e2));
        }

        /// <summary>Its outward normal (the analytic gradient).</summary>
        public static Vector3 SuperNormal(float e1, float e2, float lat, float lon)
        {
            float cv = Sp(Mathf.Cos(lat), 2 - e1), sv = Sp(Mathf.Sin(lat), 2 - e1);
            var nn = new Vector3(cv * Sp(Mathf.Cos(lon), 2 - e2), sv, cv * Sp(Mathf.Sin(lon), 2 - e2));
            return nn.sqrMagnitude > 1e-12f ? nn.normalized : (lat > 0 ? Vector3.up : Vector3.down);
        }

        static Mesh Super(float e1, float e2, int lats, int lons)
        {
            var b = new B();
            for (int i = 0; i <= lats; i++)
            {
                float lat = -Mathf.PI / 2 + Mathf.PI * i / lats;
                for (int j = 0; j <= lons; j++)
                {
                    float lon = -Mathf.PI + 2 * Mathf.PI * j / lons;
                    b.Add(SuperPoint(e1, e2, lat, lon), SuperNormal(e1, e2, lat, lon));
                }
            }
            int row = lons + 1;
            for (int i = 0; i < lats; i++)
                for (int j = 0; j < lons; j++)
                    b.Quad(i * row + j, (i + 1) * row + j, (i + 1) * row + j + 1, i * row + j + 1);
            return b.ToMesh();
        }

        /// <summary>
        /// A visor band on the (e1, e2) superellipsoid: centred on the front (lon pi/2), `halfLon` rad each side, latitude `lat0` +/- `halfLat`
        /// rising by `vee` x |lon - pi/2| (a chevron), raised `thick` (cube units) along the surface's normal in the middle and 0.012 at the
        /// edges (a pillow: no walls, no gap). Normals by finite differences, oriented by the surface's.
        /// </summary>
        static Mesh Patch(float e1, float e2, float lat0, float halfLat, float halfLon, float vee, float thick)
        {
            const int nu = 28, nv = 8;
            var pos = new Vector3[nu + 1, nv + 1];
            var refN = new Vector3[nu + 1, nv + 1];
            for (int i = 0; i <= nu; i++)
            {
                float su = -1 + 2f * i / nu; // -1..1 across
                float lon = Mathf.PI / 2 + su * halfLon;
                for (int j = 0; j <= nv; j++)
                {
                    float sv = -1 + 2f * j / nv; // -1..1 up
                    float lat = lat0 + vee * Mathf.Abs(su * halfLon) + sv * halfLat;
                    Vector3 p = SuperPoint(e1, e2, lat, lon);
                    Vector3 nn = SuperNormal(e1, e2, lat, lon);
                    float bump = Mathf.Pow(Mathf.Max(0, 1 - sv * sv), 0.35f) * Mathf.Pow(Mathf.Max(0, 1 - Mathf.Pow(Mathf.Abs(su), 6)), 0.5f);
                    pos[i, j] = p + nn * (0.012f + thick * bump);
                    refN[i, j] = nn;
                }
            }
            return Grid(pos, refN, nu, nv);
        }

        /// <summary>A grid surface with normals by central differences (each oriented by `refN`; a degenerate one takes `refN`).</summary>
        static Mesh Grid(Vector3[,] pos, Vector3[,] refN, int nu, int nv)
        {
            var b = new B();
            for (int i = 0; i <= nu; i++)
                for (int j = 0; j <= nv; j++)
                {
                    Vector3 du = pos[Mathf.Min(nu, i + 1), j] - pos[Mathf.Max(0, i - 1), j];
                    Vector3 dv = pos[i, Mathf.Min(nv, j + 1)] - pos[i, Mathf.Max(0, j - 1)];
                    Vector3 nn = Vector3.Cross(du, dv);
                    if (nn.sqrMagnitude < 1e-12f) nn = refN[i, j];
                    else if (Vector3.Dot(nn, refN[i, j]) < 0) nn = -nn;
                    b.Add(pos[i, j], nn);
                }
            int row = nv + 1;
            for (int i = 0; i < nu; i++)
                for (int j = 0; j < nv; j++)
                    b.Quad(i * row + j, (i + 1) * row + j, (i + 1) * row + j + 1, i * row + j + 1);
            return b.ToMesh();
        }

        /// <summary>A lathe: the profile (x = radius, y = height; closed if its first and last points meet) turned `segs` times about y.
        /// Normals from the profile's tangent (outward for a profile running bottom to top on its outer side).</summary>
        static Mesh Lathe(Vector2[] prof, int segs)
        {
            var b = new B();
            int np = prof.Length;
            bool closed = (prof[0] - prof[np - 1]).sqrMagnitude < 1e-10f;
            var norm2 = new Vector2[np];
            for (int k = 0; k < np; k++)
            {
                Vector2 a = k > 0 ? prof[k - 1] : closed ? prof[np - 2] : prof[k];
                Vector2 c = k < np - 1 ? prof[k + 1] : closed ? prof[1] : prof[k];
                Vector2 tan = c - a;
                Vector2 nn = new Vector2(tan.y, -tan.x);
                norm2[k] = nn.sqrMagnitude > 1e-12f ? nn.normalized : new Vector2(0, prof[k].y >= 0 ? 1 : -1);
            }
            for (int s = 0; s <= segs; s++)
            {
                float a = 2 * Mathf.PI * s / segs;
                float ca = Mathf.Cos(a), sa = Mathf.Sin(a);
                for (int k = 0; k < np; k++)
                {
                    Vector2 p = prof[k];
                    b.Add(new Vector3(p.x * ca, p.y, p.x * sa), new Vector3(norm2[k].x * ca, norm2[k].y, norm2[k].x * sa));
                }
            }
            for (int s = 0; s < segs; s++)
                for (int k = 0; k < np - 1; k++)
                    b.Quad(s * np + k, (s + 1) * np + k, (s + 1) * np + k + 1, s * np + k + 1);
            return b.ToMesh();
        }

        /// <summary>The waisted pill in the capsule primitive's space (r 0.5, y -1..1): the capsule's own outline x a shape (hips 0.94, a waist
        /// 0.86 at y -0.05, the chest full at 0.4, the shoulders rounding into the top cap). Still a pill from far away (GG: "keep the pill").</summary>
        static Vector2[] PillMesh_Profile()
        {
            const int n = 40;
            var p = new Vector2[n + 1];
            for (int k = 0; k <= n; k++)
            {
                float y = -Mathf.Cos(Mathf.PI * k / n); // dense at the poles
                float a = Mathf.Abs(y);
                float cap = a > 0.5f ? Mathf.Sqrt(Mathf.Max(0, 0.25f - (a - 0.5f) * (a - 0.5f))) : 0.5f;
                float waist = 1 - 0.14f * Mathf.Exp(-((y + 0.05f) / 0.26f) * ((y + 0.05f) / 0.26f));
                float hips = y < -0.2f ? 0.95f + 0.05f * Mathf.Clamp01((-0.2f - y) / 0.4f) : 1;
                p[k] = new Vector2(cap * waist * hips, y);
            }
            p[0] = new Vector2(0, -1);
            p[n] = new Vector2(0, 1);
            return p;
        }

        static Mesh PillMesh() => Lathe(PillMesh_Profile(), 28);

        static Vector2[] Circle(float rr, float tube, int n)
        {
            var p = new Vector2[n + 1];
            for (int k = 0; k <= n; k++)
            {
                float a = -Mathf.PI / 2 + 2 * Mathf.PI * k / n; // from the bottom, counter-clockwise (outer side first)
                p[k] = new Vector2(rr + tube * Mathf.Cos(a), tube * Mathf.Sin(a));
            }
            p[n] = p[0];
            return p;
        }

        static Vector2[] DomeProfile()
        {
            // the underside (a flat disc at y -0.2), the rolled lip, then the dome up to the top pole (cube space: r 0.5, y -0.2..0.5)
            var p = new List<Vector2> { new Vector2(0, -0.2f), new Vector2(0.3f, -0.2f), new Vector2(0.42f, -0.19f), new Vector2(0.49f, -0.13f), new Vector2(0.5f, -0.05f) };
            for (int k = 1; k <= 12; k++)
            {
                float a = Mathf.PI / 2 * k / 12;
                p.Add(new Vector2(0.5f * Mathf.Cos(a), -0.05f + 0.55f * Mathf.Sin(a)));
            }
            p[p.Count - 1] = new Vector2(0, 0.5f);
            return p.ToArray();
        }

        static Vector2[] SpikeProfile()
        {
            // a horn: a base disc at y -0.5, the flank curving in (concave) to the tip at y 0.5
            var p = new List<Vector2> { new Vector2(0, -0.5f), new Vector2(0.5f, -0.5f) };
            for (int k = 1; k <= 10; k++)
            {
                float s = k / 10f;
                p.Add(new Vector2(0.5f * Mathf.Pow(1 - s, 1.6f), -0.5f + s));
            }
            p[p.Count - 1] = new Vector2(0, 0.5f);
            return p.ToArray();
        }

        static Vector2[] SaucerProfile()
        {
            // a lens: the belly (shallow) up to the rim lip at r 1, then the canopy (a higher dome); y -0.32..0.42
            var p = new List<Vector2> { new Vector2(0, -0.32f) };
            for (int k = 1; k <= 8; k++)
            {
                float s = k / 8f;
                p.Add(new Vector2(0.92f * Mathf.Sin(s * Mathf.PI / 2), -0.32f * Mathf.Cos(s * Mathf.PI / 2) - 0.02f));
            }
            p.Add(new Vector2(1.0f, -0.02f));
            p.Add(new Vector2(1.0f, 0.04f));
            p.Add(new Vector2(0.9f, 0.08f));
            for (int k = 1; k <= 8; k++)
            {
                float s = k / 8f;
                p.Add(new Vector2(0.86f * Mathf.Cos(s * Mathf.PI / 2), 0.08f + 0.34f * Mathf.Sin(s * Mathf.PI / 2)));
            }
            p[p.Count - 1] = new Vector2(0, 0.42f);
            return p.ToArray();
        }

        static Vector2[] SegmentProfile()
        {
            // a burrower segment (cube space, y -0.5..0.5): a fat rounded ring body with a raised crest band around its middle
            var p = new List<Vector2> { new Vector2(0, -0.5f) };
            for (int k = 1; k <= 14; k++)
            {
                float s = k / 14f;
                float y = -0.5f + s;
                float r = 0.42f * Mathf.Sqrt(Mathf.Max(0, 1 - Mathf.Pow(2 * y, 4))) + 0.06f * Mathf.Exp(-(y / 0.08f) * (y / 0.08f));
                p.Add(new Vector2(r, y));
            }
            p[p.Count - 1] = new Vector2(0, 0.5f);
            return p.ToArray();
        }

        static Vector2[] ToothProfile()
        {
            var p = new List<Vector2> { new Vector2(0, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0.42f, -0.2f), new Vector2(0.28f, 0.1f), new Vector2(0.12f, 0.35f), new Vector2(0, 0.5f) };
            return p.ToArray();
        }

        /// <summary>The shield's bow in cube space (forward in the middle, back at the sides). Strong, because a shield's transform scales z thin
        /// (the warden's 0.12 m: a 0.15 m bow over its 1.9 m).</summary>
        public static float Bend(float x) => 0.625f - 5f * x * x;

        /// <summary>The shield: a rounded slab (superellipsoid 0.25 / 0.3, cube space; its thin axis z) bowed by Bend. Normals by finite differences.</summary>
        static Mesh Slab()
        {
            const int lats = 12, lons = 32;
            const float e1 = 0.25f, e2 = 0.3f;
            var pos = new Vector3[lons + 1, lats + 1];
            var refN = new Vector3[lons + 1, lats + 1];
            for (int j = 0; j <= lons; j++)
            {
                float lon = -Mathf.PI + 2 * Mathf.PI * j / lons;
                for (int i = 0; i <= lats; i++)
                {
                    float lat = -Mathf.PI / 2 + Mathf.PI * i / lats;
                    // the slab's thin axis is z: the superellipsoid's y and z swapped (a face plate, not a disc)
                    Vector3 s = SuperPoint(e1, e2, lat, lon), sn = SuperNormal(e1, e2, lat, lon);
                    var p = new Vector3(s.x, s.z, s.y);
                    p.z += Bend(p.x);
                    pos[j, i] = p;
                    refN[j, i] = new Vector3(sn.x + 10f * p.x * sn.y, sn.z, sn.y); // the unbent normal tipped by the bow's slope (-Bend'(x) = 10 x on z)
                }
            }
            return Grid(pos, refN, lons, lats);
        }

        /// <summary>The shield's frame: a tube (r 0.07) along the slab's rounded-rectangle edge (exponent 0.3, half sizes 0.5), bent with it.</summary>
        static Mesh Frame()
        {
            const int nu = 64, nv = 8;
            const float e = 0.3f, tube = 0.07f;
            var pos = new Vector3[nu + 1, nv + 1];
            var refN = new Vector3[nu + 1, nv + 1];
            for (int i = 0; i <= nu; i++)
            {
                float a = 2 * Mathf.PI * i / nu;
                var c = new Vector2(0.5f * Sp(Mathf.Cos(a), e), 0.5f * Sp(Mathf.Sin(a), e));
                var nIn = new Vector2(Sp(Mathf.Cos(a), 2 - e), Sp(Mathf.Sin(a), 2 - e)).normalized; // the loop's outward normal in its plane
                for (int j = 0; j <= nv; j++)
                {
                    float w = 2 * Mathf.PI * j / nv;
                    Vector3 off = new Vector3(nIn.x, nIn.y, 0) * Mathf.Cos(w) + new Vector3(0, 0, 1) * Mathf.Sin(w);
                    // an oval tube: thin in the plane (tube), deeper than the slab on z (0.62 vs its 0.5) so the frame stands proud of its face
                    var p = new Vector3(c.x + nIn.x * tube * Mathf.Cos(w), c.y + nIn.y * tube * Mathf.Cos(w), 0.62f * Mathf.Sin(w));
                    p.z += Bend(c.x); // bowed as the slab is (it is anchored on the shield)
                    pos[i, j] = p;
                    refN[i, j] = off;
                }
            }
            return Grid(pos, refN, nu, nv);
        }

        /// <summary>Meshes placed by matrices merged into one (a mirroring matrix flips its triangles; normals by the inverse transpose).</summary>
        public static Mesh Combine(string name, params (Mesh mesh, Matrix4x4 m)[] parts)
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var t = new List<int>();
            foreach ((Mesh mesh, Matrix4x4 m) in parts)
            {
                Matrix4x4 nm = m.inverse.transpose;
                bool flip = m.determinant < 0;
                int o = v.Count;
                Vector3[] mv = mesh.vertices, mn = mesh.normals;
                int[] mt = mesh.triangles;
                for (int i = 0; i < mv.Length; i++)
                {
                    v.Add(m.MultiplyPoint3x4(mv[i]));
                    n.Add(nm.MultiplyVector(mn[i]).normalized);
                }
                for (int i = 0; i < mt.Length; i += 3)
                {
                    t.Add(o + mt[i]);
                    t.Add(o + (flip ? mt[i + 2] : mt[i + 1]));
                    t.Add(o + (flip ? mt[i + 1] : mt[i + 2]));
                }
            }
            var r = new Mesh { name = name };
            r.SetVertices(v);
            r.SetNormals(n);
            r.SetTriangles(t, 0);
            r.RecalculateBounds();
            return r;
        }
    }
}
