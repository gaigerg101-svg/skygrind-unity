// PG3 V2's draw path: EXACTLY V1's Fx74Draw recipe (the lead, after pg3-c / pg3-d: V1's quads render in the Player, V2's FxSprites batches showed nothing in the pair frames). One static unit quad (both faces),
// procedural RGBA32 linear textures baked here (no Resources load), copies of the game's neon (URP Unlit) material made by FxDraw.Additive (Blend One One, no depth write, the transparent queue) with the
// base colour black, the colour of each quad given by a MaterialPropertyBlock (HDR, alpha forced to 1), one Graphics.RenderMesh per quad with RenderParams on layer 0 and the quad's own world bounds. No
// 8-step brightness buckets any more: a colour is exact, so a fade is smooth. Textures: Glow (rim74's (1 - r^2)^2 radial glow, no hot core, no streak), Star (a soft lens cross), Streak (tail 0 to head 1,
// hottest at the head), Ring (a thin bright circle at 0.8 of the radius), Beam (even along its length). Nothing here reads or writes the sim.
// The probe reads Describe(): the shader name, blend, queue, the neon's _BaseMap / _BaseColor properties, the texture formats and the last colour submitted (alpha included).
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public enum V2Tex { Glow = 0, Star = 1, Streak = 2, Ring = 3, Beam = 4 }

    public static class V2Draw
    {
        static Mesh quad;
        static Material[] mats;
        static Texture2D[] texs;
        static Material neonSrc;
        static MaterialPropertyBlock mpb; // made in Ensure (an engine call: a static initialiser would throw in a test with no engine, where Describe() is still read)
        static int baseColor;

        /// <summary>Quads submitted since the last Begin (this frame), the most in one frame, and in all.</summary>
        public static int Quads, PeakQuads;
        public static long Total;
        /// <summary>The last colour submitted (what the material got, alpha included).</summary>
        public static Color LastColour = Color.black;

        public static bool Ready => mats != null;

        /// <summary>Once a frame before the first draw: the count restarts.</summary>
        public static void Begin() { Quads = 0; }

        public static void End() { if (Quads > PeakQuads) PeakQuads = Quads; }

        /// <summary>Make the kit from the game's neon material (a new neon rebuilds the materials only).</summary>
        public static void Ensure(Material neon)
        {
            if (neon == null) return;
            if (mpb == null) { mpb = new MaterialPropertyBlock(); baseColor = Shader.PropertyToID("_BaseColor"); }
            if (quad == null) { quad = MakeQuad(); texs = MakeTextures(); }
            if (mats != null && ReferenceEquals(neonSrc, neon)) return;
            if (mats != null) foreach (Material m in mats) if (m != null) Object.Destroy(m);
            neonSrc = neon;
            mats = new Material[texs.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = FxDraw.Additive(neon, "PG3 V2 fx " + (V2Tex)i);
                mats[i].SetTexture("_BaseMap", texs[i]);
                mats[i].SetColor(baseColor, Color.black);
            }
        }

        /// <summary>The probe's words: what the Player actually drew with.</summary>
        public static string Describe()
        {
            if (mats == null || mats[0] == null) return "kit not built";
            Material m = mats[0];
            string shader = m.shader != null ? m.shader.name : "null shader";
            string sup = m.shader != null && m.shader.isSupported ? "supported" : "NOT SUPPORTED";
            string blend = m.HasProperty("_SrcBlend") ? m.GetFloat("_SrcBlend").ToString("0") + "/" + m.GetFloat("_DstBlend").ToString("0") : "no blend props";
            string tex = texs[0] != null ? texs[0].width + "x" + texs[0].height + " " + texs[0].format : "texture destroyed";
            return $"shader {shader} ({sup}), blend {blend}, queue {m.renderQueue}, _BaseMap {m.HasProperty("_BaseMap")}, glow texture {tex}, last colour ({LastColour.r:0.00}, {LastColour.g:0.00}, {LastColour.b:0.00}, a {LastColour.a:0.00}), quads {Total} (most in a frame {PeakQuads})";
        }

        /// <summary>One quad: its centre, the FULL width vector (ax) and full height vector (ay), a colour (HDR; the alpha is forced to 1).</summary>
        public static void Submit(V2Tex t, Vector3 center, Vector3 ax, Vector3 ay, Color col)
        {
            if (mats == null || Quads >= UnityTuning.N74DrawMaxQuads) return;
            if (ax.sqrMagnitude < 1e-10f || ay.sqrMagnitude < 1e-10f) return;
            Vector3 az = Vector3.Cross(ax.normalized, ay.normalized);
            var m = new Matrix4x4(new Vector4(ax.x, ax.y, ax.z, 0f), new Vector4(ay.x, ay.y, ay.z, 0f), new Vector4(az.x, az.y, az.z, 0f), new Vector4(center.x, center.y, center.z, 1f));
            col.a = 1f;
            LastColour = col;
            mpb.SetColor(baseColor, col);
            float reach = ax.magnitude + ay.magnitude + 0.1f;
            var rp = new RenderParams(mats[(int)t]) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false, matProps = mpb, worldBounds = new Bounds(center, Vector3.one * reach) };
            Graphics.RenderMesh(rp, quad, 0, m);
            Quads++;
            Total++;
        }

        /// <summary>A quad from its HALF extents (the way FxSprites.Quad took them).</summary>
        public static void Quad(V2Tex t, Vector3 center, Vector3 halfRight, Vector3 halfUp, Color col) => Submit(t, center, halfRight * 2f, halfUp * 2f, col);

        /// <summary>A camera-facing quad `size` m wide, rolled `rollRad` about the view axis.</summary>
        public static void Billboard(V2Tex t, Transform cam, Vector3 at, float size, float rollRad, Color col)
        {
            Quaternion q = cam.rotation * Quaternion.Euler(0f, 0f, rollRad * Mathf.Rad2Deg);
            Submit(t, at, q * Vector3.right * size, q * Vector3.up * size, col);
        }

        /// <summary>A quad lying on a surface (or standing across an axis): `normal` is the way it faces, `size` m wide, rolled about it.</summary>
        public static void Facing(V2Tex t, Vector3 at, Vector3 normal, float size, float rollRad, Color col)
        {
            if (normal.sqrMagnitude < 1e-8f) return;
            Vector3 n = normal.normalized;
            Vector3 up = Mathf.Abs(n.y) > 0.95f ? Vector3.forward : Vector3.up;
            Quaternion q = Quaternion.LookRotation(-n, up) * Quaternion.Euler(0f, 0f, rollRad * Mathf.Rad2Deg);
            Submit(t, at, q * Vector3.right * size, q * Vector3.up * size, col);
        }

        /// <summary>A camera-facing ribbon from `tail` to `head`, `width` m across (the streak texture is hottest at the head; the beam texture is even along its length).</summary>
        public static void Ribbon(Vector3 eye, Vector3 tail, Vector3 head, float width, Color col, V2Tex tex)
        {
            Vector3 d = head - tail;
            if (d.sqrMagnitude < 1e-8f) return;
            Vector3 mid = (head + tail) * 0.5f;
            Vector3 side = Vector3.Cross(d, eye - mid);
            if (side.sqrMagnitude < 1e-10f) return;
            Submit(tex, mid, d, side.normalized * width, col);
        }

        /// <summary>A strip lying FLAT on the floor from `a` to `b`, `width` m across (the slide's rails; a camera-facing ribbon would stand up).</summary>
        public static void Flat(Vector3 a, Vector3 b, float width, Color col, V2Tex tex)
        {
            Vector3 d = b - a;
            if (d.sqrMagnitude < 1e-8f) return;
            Vector3 side = Vector3.Cross(Vector3.up, d);
            if (side.sqrMagnitude < 1e-10f) return;
            Submit(tex, (a + b) * 0.5f, d, side.normalized * width, col);
        }

        static Mesh MakeQuad()
        {
            var m = new Mesh { name = "PG3 V2 quad" };
            m.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f) };
            m.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) };
            m.triangles = new[] { 0, 2, 1, 0, 3, 2, 0, 1, 2, 0, 2, 3 }; // both faces
            m.RecalculateBounds();
            return m;
        }

        static float Smooth(float a, float b, float x)
        {
            float t = Mathf.Clamp01((x - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        static Texture2D Bake(string name, int n, System.Func<float, float, float> f)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true) { name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float v = Mathf.Clamp01(f((x + 0.5f) / n, (y + 0.5f) / n)); // u, v in 0..1
                    byte b = (byte)Mathf.RoundToInt(v * 255f);
                    px[y * n + x] = new Color32(b, b, b, 255);
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        static Texture2D[] MakeTextures()
        {
            int n = UnityTuning.N74DrawTexSize;
            var t = new Texture2D[5];
            // glow: rim74's radial glow, (1 - r^2)^2 (Rim74Math.GlowProfile): whole at the centre, nothing and no slope at the edge
            t[(int)V2Tex.Glow] = Bake("PG3 V2 glow", n, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f;
                return (float)Rim74Math.GlowProfile(Mathf.Sqrt(x * x + y * y));
            });
            // star: a soft lens cross round a hot core (V1's recipe)
            t[(int)V2Tex.Star] = Bake("PG3 V2 star", n, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f, r = Mathf.Sqrt(x * x + y * y);
                if (r >= 1f) return 0f;
                float core = Mathf.Pow(Mathf.Clamp01(1f - r / 0.26f), 2f);
                float glow = Mathf.Pow(1f - r, 3f) * 0.32f;
                float ang = Mathf.Atan2(y, x);
                float rs = Mathf.Sqrt(x * x + (y * 1.8f) * (y * 1.8f));
                float cross = Mathf.Pow(Mathf.Abs(Mathf.Cos(2f * ang)), 70f) * Mathf.Pow(Mathf.Clamp01(1f - rs), 1.5f);
                float diag = Mathf.Pow(Mathf.Abs(Mathf.Sin(2f * ang)), 110f) * Mathf.Pow(1f - r, 3.4f) * 0.42f;
                return (core + glow + cross + diag) * Smooth(1f, 0.86f, r);
            });
            // streak: u runs tail (0) to head (1), v across; a hot thin core, a soft shoulder, a rounded head (V1's recipe)
            t[(int)V2Tex.Streak] = Bake("PG3 V2 streak", n, (u, v) =>
            {
                float y = Mathf.Abs(v * 2f - 1f);
                float across = Mathf.Pow(1f - y, 2.2f) * 0.55f + Mathf.Pow(1f - y, 9f) * 0.75f;
                float along = Mathf.Pow(Smooth(0f, 0.9f, u), 1.4f);
                float cap = u > 0.93f ? Mathf.Sqrt(Mathf.Clamp01(1f - Mathf.Pow((u - 0.93f) / 0.07f, 2f))) : 1f;
                return across * along * cap;
            });
            // beam: even along its length with short soft ends, the same hot thin core and soft shoulder across (V1's recipe)
            t[(int)V2Tex.Beam] = Bake("PG3 V2 beam", n, (u, v) =>
            {
                float y = Mathf.Abs(v * 2f - 1f);
                float across = Mathf.Pow(1f - y, 2.0f) * 0.6f + Mathf.Pow(1f - y, 8f) * 0.7f;
                return across * Smooth(0f, 0.03f, u) * Smooth(1f, 0.97f, u);
            });
            // ring: a thin bright circle at 0.8 of the radius with soft edges and a faint fill (V1's recipe)
            t[(int)V2Tex.Ring] = Bake("PG3 V2 ring", n, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f, r = Mathf.Sqrt(x * x + y * y);
                if (r >= 1f) return 0f;
                float band = Mathf.Exp(-Mathf.Pow((r - 0.8f) / 0.075f, 2f));
                float fill = 0.1f * Mathf.Pow(1f - r, 2f);
                return (band + fill) * Smooth(1f, 0.93f, r);
            });
            return t;
        }
    }
}
