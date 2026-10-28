// PG3 V1 (WEAPON FEEL; the shared kit of shot74 and charge74, GG 2026-10-28: visuals "much higher quality", guns "very satisfying"). View only: pure math
// (Fx74Math, pinned by Fx74Tests), one quad mesh and five procedural additive textures (soft glow, a lens-flare star, a hot streak, a ring, an even beam), and three
// pooled draw lists (timed sprites, sparks, comets). Nothing here reads or writes the sim; nothing is allocated per frame once the kit is built (the
// quad, the textures and the materials are made once; a draw is one Graphics.RenderMesh with a reused property block, the way WeaponFx draws its star).
// The materials are copies of the neon (URP Unlit) material set to Blend One One, no depth write, the transparent queue (FxDraw.Additive's recipe),
// both faces; colours are HDR (the post stack blooms anything over 1). Numbers: UnityTuning.PG3V1.cs.
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>PG3 V1: the four procedural textures' names (a quad's picture).</summary>
    public enum Fx74Tex { Soft = 0, Star = 1, Streak = 2, Ring = 3, Beam = 4 }

    /// <summary>PG3 V1: the pure shapes of the weapon-feel effects (envelopes, sizes, the comet's head and tail, the charge's glow, the READY edge).</summary>
    public static class Fx74Math
    {
        public static float Clamp01(float x) => x < 0f ? 0f : x > 1f ? 1f : x;

        /// <summary>Ease-out 0..1: fast off the start, slow into the end (1 - (1 - u)^2).</summary>
        public static float EaseOut(float u) { u = Clamp01(u); return 1f - (1f - u) * (1f - u); }

        /// <summary>A hot envelope at `age` of `life`: whole for the first `hold` share, then (1 - v)^2 down to 0 at the end (a crisp snap: bright at once, gone fast).</summary>
        public static float Env(float age, float life, float hold = 0f)
        {
            if (!(life > 0f) || age < 0f) return 0f;
            float u = age / life;
            if (u >= 1f) return 0f;
            if (u <= hold) return 1f;
            float v = (u - hold) / (1f - hold);
            return (1f - v) * (1f - v);
        }

        /// <summary>A size growing from s0 to s1 over the sprite's life (ease-out).</summary>
        public static float SizeAt(float s0, float s1, float u) => s0 + (s1 - s0) * EaseOut(u);

        /// <summary>The width in metres that subtends `deg` degrees at `distM`.</summary>
        public static float Subtend(float distM, float deg) => 2f * distM * Mathf.Tan(deg * 0.5f * Mathf.Deg2Rad);

        /// <summary>A sprite's size kept under `maxDeg` of the view at `distM` (GG's NIGHT43 complaint: a charged release's flash "filled most of the screen"): a flash close to
        /// the eye cannot cover the picture however big its world size. A distance under 1 mm leaves the size alone.</summary>
        public static float CapDeg(float size, float distM, float maxDeg) => distM > 1e-3f ? Mathf.Min(size, Subtend(distM, maxDeg)) : size;

        /// <summary>A comet's head and tail distances (m from the start) on a segment of `len` at `age`: the head runs out over `travelS`; the tail trails
        /// it by `tailM`; after the head arrives the tail runs on to the end over `settleS` (the streak is gone then).</summary>
        public static void Comet(float len, float age, float travelS, float tailM, float settleS, out float tail, out float head)
        {
            if (!(len > 0f)) { tail = head = 0f; return; }
            head = travelS > 0f ? len * Clamp01(age / travelS) : len;
            tail = Mathf.Max(0f, head - tailM);
            if (age > travelS) tail = settleS > 0f ? Mathf.Lerp(Mathf.Max(0f, len - tailM), len, Clamp01((age - travelS) / settleS)) : len;
        }

        /// <summary>The seconds a comet takes to travel `len` m at `speed` m/s, never more than `maxS` (a far wall is crossed in a few frames).</summary>
        public static float TravelS(float len, float speed, float maxS) => speed > 0f ? Mathf.Min(maxS, len / speed) : maxS;

        /// <summary>The seconds the whole comet lives: its travel plus the tail's run-out.</summary>
        public static float CometLife(float travelS, float settleS) => travelS + settleS;

        /// <summary>The charge glow's size (m) at level 0..1: from `from` at 0 to `to` at 1, smoothstep (it swells slowly then fills).</summary>
        public static float ChargeSize(float level, float from, float to)
        {
            float l = Clamp01(level);
            float e = l * l * (3f - 2f * l);
            return from + (to - from) * e;
        }

        /// <summary>The charge glow's brightness (HDR multiplier) at level: a floor so a tap of charge shows, then level squared up to `peak`.</summary>
        public static float ChargeGain(float level, float floor, float peak)
        {
            float l = Clamp01(level);
            return l <= 0f ? 0f : floor + (peak - floor) * l * l;
        }

        /// <summary>True on the frame a charge level crosses into FULL (prev under 1, now at or over): the READY flash's edge. A drop to 0 and back fires again.</summary>
        public static bool ReadyEdge(float prev, float now) => prev < 1f && now >= 1f;

        /// <summary>A mote's radius (0..1 of the shell) as it falls into the muzzle: phase 0..1 repeating, so it starts at the shell and ends at 0.</summary>
        public static float MoteRadius(float time, float hz, float index, float count)
        {
            float p = time * hz + index / Mathf.Max(1f, count);
            p -= Mathf.Floor(p);
            return 1f - p;
        }

        /// <summary>The muzzle flash is shown for its first `minFrames` rendered frames however short the time is, and for `lifeS` seconds.</summary>
        public static bool FlashAlive(float ageS, int frames, float lifeS, int minFrames) => ageS < lifeS || frames < minFrames;

        /// <summary>The kick frame's weight: 1 on the first rendered frame, `second` on the next, 0 after (a 1-2 frame screen kick).</summary>
        public static float KickWeight(int frame, float second) => frame == 0 ? 1f : frame == 1 ? second : 0f;
    }

    /// <summary>PG3 V1: the one quad mesh, four textures and their additive materials; the draw calls. Static: one kit for the process, made on first use.</summary>
    public static class Fx74Draw
    {
        static Mesh quad;
        static Material[] mats;
        static Texture2D[] texs;
        static Material neonSrc;
        static readonly MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>Quads submitted since the last Begin (this frame), and in all (the probes read both).</summary>
        public static int Quads, TotalQuads, PeakQuads;

        public static bool Ready => quad != null;

        /// <summary>Once a frame, before the first draw: the count restarts.</summary>
        public static void Begin() { Quads = 0; }

        public static void End() { if (Quads > PeakQuads) PeakQuads = Quads; }

        /// <summary>Make the kit from the game's neon material (a new neon, a new scene's game, rebuilds the materials only).</summary>
        public static void Ensure(Material neon)
        {
            if (neon == null) return;
            if (quad == null) { quad = MakeQuad(); texs = MakeTextures(); }
            if (mats != null && ReferenceEquals(neonSrc, neon)) return;
            if (mats != null) foreach (Material m in mats) if (m != null) Object.Destroy(m);
            neonSrc = neon;
            mats = new Material[texs.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = FxDraw.Additive(neon, "PG3 V1 fx " + (Fx74Tex)i);
                mats[i].SetTexture("_BaseMap", texs[i]);
                mats[i].SetColor(BaseColor, Color.black);
            }
        }

        /// <summary>One quad: its centre, the half-extents' full vectors (ax is the quad's full width vector, ay its full height vector), a colour (HDR).</summary>
        public static void Submit(Fx74Tex t, Vector3 center, Vector3 ax, Vector3 ay, Color col)
        {
            if (mats == null || Quads >= UnityTuning.PG3V1MaxQuads) return;
            Vector3 az = Vector3.Cross(ax.normalized, ay.normalized);
            var m = new Matrix4x4(new Vector4(ax.x, ax.y, ax.z, 0f), new Vector4(ay.x, ay.y, ay.z, 0f), new Vector4(az.x, az.y, az.z, 0f), new Vector4(center.x, center.y, center.z, 1f));
            mpb.SetColor(BaseColor, col);
            float reach = ax.magnitude + ay.magnitude + 0.1f;
            var rp = new RenderParams(mats[(int)t]) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false, matProps = mpb, worldBounds = new Bounds(center, Vector3.one * reach) };
            Graphics.RenderMesh(rp, quad, 0, m);
            Quads++;
            TotalQuads++;
        }

        /// <summary>A camera-facing quad `size` m wide, rolled `rollDeg` about the view axis.</summary>
        public static void Billboard(Fx74Tex t, Camera cam, Vector3 at, float size, float rollDeg, Color col)
        {
            Transform ct = cam.transform;
            Quaternion q = ct.rotation * Quaternion.Euler(0f, 0f, rollDeg);
            Submit(t, at, q * Vector3.right * size, q * Vector3.up * size, col);
        }

        /// <summary>A quad lying on a surface (or standing across an axis): `normal` is the way it faces, `size` m wide, rolled about it.</summary>
        public static void Facing(Fx74Tex t, Vector3 at, Vector3 normal, float size, float rollDeg, Color col)
        {
            if (normal.sqrMagnitude < 1e-8f) return;
            Vector3 n = normal.normalized;
            Vector3 up = Mathf.Abs(n.y) > 0.95f ? Vector3.forward : Vector3.up;
            Quaternion q = Quaternion.LookRotation(-n, up) * Quaternion.Euler(0f, 0f, rollDeg);
            Submit(t, at, q * Vector3.right * size, q * Vector3.up * size, col);
        }

        /// <summary>A camera-facing ribbon from `tail` to `head` (the streak texture is hottest at the head; the beam texture is even along its length), `width` m across.</summary>
        public static void Ribbon(Camera cam, Vector3 tail, Vector3 head, float width, Color col, Fx74Tex tex = Fx74Tex.Streak)
        {
            Vector3 d = head - tail;
            if (d.sqrMagnitude < 1e-8f) return;
            Vector3 mid = (head + tail) * 0.5f;
            Vector3 view = cam.transform.position - mid;
            Vector3 side = Vector3.Cross(d, view);
            if (side.sqrMagnitude < 1e-10f) side = Vector3.Cross(d, cam.transform.up);
            if (side.sqrMagnitude < 1e-10f) return;
            Submit(tex, mid, d, side.normalized * width, col);
        }

        static Mesh MakeQuad()
        {
            var m = new Mesh { name = "PG3 V1 quad" };
            m.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f) };
            m.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) };
            m.triangles = new[] { 0, 2, 1, 0, 3, 2, 0, 1, 2, 0, 2, 3 }; // both faces
            m.RecalculateBounds();
            return m;
        }

        static float Smooth(float a, float b, float x)
        {
            float t = Fx74Math.Clamp01((x - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        static Texture2D Bake(string name, int n, System.Func<float, float, float> f)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true) { name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float v = Fx74Math.Clamp01(f((x + 0.5f) / n, (y + 0.5f) / n)); // u, v in 0..1
                    byte b = (byte)Mathf.RoundToInt(v * 255f);
                    px[y * n + x] = new Color32(b, b, b, 255);
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        static Texture2D[] MakeTextures()
        {
            int n = UnityTuning.PG3V1TexSize;
            var t = new Texture2D[5];
            // soft: a round glow with a hot core
            t[(int)Fx74Tex.Soft] = Bake("PG3 V1 soft", n, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f, r = Mathf.Sqrt(x * x + y * y);
                if (r >= 1f) return 0f;
                float halo = Mathf.Pow(1f - r, 2.4f) * 0.78f;
                float core = Mathf.Pow(Fx74Math.Clamp01(1f - r / 0.3f), 2f) * 0.6f;
                return (halo + core) * Smooth(1f, 0.82f, r);
            });
            // star: a lens-flare cross (the long horizontal spike, a shorter vertical one, faint diagonals) round a hot core
            t[(int)Fx74Tex.Star] = Bake("PG3 V1 star", n, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f, r = Mathf.Sqrt(x * x + y * y);
                if (r >= 1f) return 0f;
                float core = Mathf.Pow(Fx74Math.Clamp01(1f - r / 0.26f), 2f);
                float glow = Mathf.Pow(1f - r, 3f) * 0.32f;
                float ang = Mathf.Atan2(y, x);
                float rs = Mathf.Sqrt(x * x + (y * 1.8f) * (y * 1.8f)); // the vertical spike is shorter
                float cross = Mathf.Pow(Mathf.Abs(Mathf.Cos(2f * ang)), 70f) * Mathf.Pow(Fx74Math.Clamp01(1f - rs), 1.5f);
                float diag = Mathf.Pow(Mathf.Abs(Mathf.Sin(2f * ang)), 110f) * Mathf.Pow(1f - r, 3.4f) * 0.42f;
                return (core + glow + cross + diag) * Smooth(1f, 0.86f, r);
            });
            // streak: u runs tail (0) to head (1), v across; a hot thin core, a soft shoulder, a rounded head
            t[(int)Fx74Tex.Streak] = Bake("PG3 V1 streak", n, (u, v) =>
            {
                float y = Mathf.Abs(v * 2f - 1f);
                float across = Mathf.Pow(1f - y, 2.2f) * 0.55f + Mathf.Pow(1f - y, 9f) * 0.75f;
                float along = Mathf.Pow(Smooth(0f, 0.9f, u), 1.4f);
                float cap = u > 0.93f ? Mathf.Sqrt(Fx74Math.Clamp01(1f - Mathf.Pow((u - 0.93f) / 0.07f, 2f))) : 1f;
                return across * along * cap;
            });
            // beam: even along its length with short soft ends, the same hot thin core and soft shoulder across (a charged shot's line)
            t[(int)Fx74Tex.Beam] = Bake("PG3 V1 beam", n, (u, v) =>
            {
                float y = Mathf.Abs(v * 2f - 1f);
                float across = Mathf.Pow(1f - y, 2.0f) * 0.6f + Mathf.Pow(1f - y, 8f) * 0.7f;
                return across * Smooth(0f, 0.03f, u) * Smooth(1f, 0.97f, u);
            });
            // ring: a thin bright circle at 0.8 of the radius with soft edges and a faint fill
            t[(int)Fx74Tex.Ring] = Bake("PG3 V1 ring", n, (u, v) =>
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

    /// <summary>PG3 V1: timed sprites (a flash, a ring, a decal) in a fixed pool; the oldest is reused. A sprite grows c0/size0 to c1/size1 over its life.</summary>
    public sealed class Fx74Sprites
    {
        public struct Sprite
        {
            public bool alive, facing;
            public Fx74Tex tex;
            public Vector3 at, normal;
            public float age, life, size0, size1, roll, hold, wait;
            public Color c0, c1;
        }

        readonly Sprite[] pool;
        int next;

        /// <summary>Sprites alive after the last Step.</summary>
        public int Live;

        public Fx74Sprites(int capacity) { pool = new Sprite[capacity]; }

        public void Clear() { for (int i = 0; i < pool.Length; i++) pool[i].alive = false; Live = 0; }

        /// <summary>A camera-facing sprite (a flash, a star, a ring across the view).</summary>
        public void Billboard(Fx74Tex tex, Vector3 at, float size0, float size1, float life, float hold, Color c0, Color c1, float roll = 0f, float delay = 0f)
            => Put(tex, false, at, Vector3.zero, size0, size1, life, hold, c0, c1, roll, delay);

        /// <summary>A sprite facing along `normal` (a decal on a wall, a ring across the beam).</summary>
        public void Facing(Fx74Tex tex, Vector3 at, Vector3 normal, float size0, float size1, float life, float hold, Color c0, Color c1, float roll = 0f, float delay = 0f)
            => Put(tex, true, at, normal, size0, size1, life, hold, c0, c1, roll, delay);

        void Put(Fx74Tex tex, bool facing, Vector3 at, Vector3 normal, float size0, float size1, float life, float hold, Color c0, Color c1, float roll, float delay)
        {
            ref Sprite s = ref pool[next];
            next = (next + 1) % pool.Length;
            s.alive = true; s.facing = facing; s.tex = tex; s.at = at; s.normal = normal; s.age = 0f; s.life = life; s.size0 = size0; s.size1 = size1;
            s.hold = hold; s.c0 = c0; s.c1 = c1; s.roll = roll; s.wait = delay;
        }

        /// <summary>Wait (in REAL frame time: `delay` of a put, so an impact lands when its tracer arrives even through a hit freeze), then age every sprite by `dt`
        /// (the world's time: frozen in a hit freeze, so a burst holds on it) and draw it (the camera is the world camera).</summary>
        public void StepAndDraw(float dt, float realDt, Camera cam)
        {
            int live = 0;
            for (int i = 0; i < pool.Length; i++)
            {
                ref Sprite s = ref pool[i];
                if (!s.alive) continue;
                if (s.wait > 0f)
                {
                    s.wait -= realDt; // still on its way (a tracer in flight, a ring staggered in)
                    if (s.wait > 0f) { live++; continue; }
                }
                s.age += dt;
                if (s.age >= s.life) { s.alive = false; continue; }
                live++;
                float u = s.age / s.life;
                float env = Fx74Math.Env(s.age, s.life, s.hold);
                if (env <= 0.002f) continue;
                Color c = Color.Lerp(s.c0, s.c1, Mathf.Sqrt(u)) * env;
                float size = Fx74Math.SizeAt(s.size0, s.size1, u);
                if (s.facing) Fx74Draw.Facing(s.tex, s.at, s.normal, size, s.roll, c);
                else Fx74Draw.Billboard(s.tex, cam, s.at, size, s.roll, c);
            }
            Live = live;
        }

        /// <summary>One clock for both (a pool that needs no world / real split).</summary>
        public void StepAndDraw(float dt, Camera cam) => StepAndDraw(dt, dt, cam);
    }

    /// <summary>PG3 V1: sparks in a fixed pool: a point with a velocity (gravity and drag), drawn as a short hot streak along its motion.</summary>
    public sealed class Fx74Sparks
    {
        struct Spark
        {
            public bool alive;
            public Vector3 pos, vel;
            public float age, life, len, width, wait;
            public Color c;
        }

        readonly Spark[] pool;
        int next;

        /// <summary>Sparks alive after the last Step, and added since the pool was made.</summary>
        public int Live, Added;

        public Fx74Sparks(int capacity) { pool = new Spark[capacity]; }

        public void Clear() { for (int i = 0; i < pool.Length; i++) pool[i].alive = false; Live = 0; }

        /// <summary>A spark from `pos` at `vel` that waits `delay` s of real time before it flies (an impact whose tracer has not arrived yet).</summary>
        public void Add(Vector3 pos, Vector3 vel, float life, float len, float width, Color c, float delay = 0f)
        {
            ref Spark s = ref pool[next];
            next = (next + 1) % pool.Length;
            s.alive = true; s.pos = pos; s.vel = vel; s.age = 0f; s.life = life; s.len = len; s.width = width; s.c = c; s.wait = delay;
            Added++;
        }

        public void StepAndDraw(float dt, float gravity, float drag, Camera cam) => StepAndDraw(dt, dt, gravity, drag, cam);

        /// <summary>Wait in real frame time, then fly and age on the world's `dt` (frozen in a hit freeze).</summary>
        public void StepAndDraw(float dt, float realDt, float gravity, float drag, Camera cam)
        {
            int live = 0;
            float k = Mathf.Exp(-drag * dt);
            for (int i = 0; i < pool.Length; i++)
            {
                ref Spark s = ref pool[i];
                if (!s.alive) continue;
                if (s.wait > 0f)
                {
                    s.wait -= realDt;
                    if (s.wait > 0f) { live++; continue; }
                }
                s.age += dt;
                if (s.age >= s.life) { s.alive = false; continue; }
                live++;
                s.vel.y -= gravity * dt;
                s.vel *= k;
                s.pos += s.vel * dt;
                float speed = s.vel.magnitude;
                if (speed < 1e-3f) continue;
                float u = s.age / s.life;
                float env = Mathf.Pow(1f - u, 1.5f);
                float len = Mathf.Min(s.len, speed * UnityTuning.PG3V1SparkStretchS) * (0.4f + 0.6f * env);
                Fx74Draw.Ribbon(cam, s.pos - s.vel / speed * len, s.pos, s.width * (0.5f + 0.5f * env), s.c * env);
            }
            Live = live;
        }
    }

    /// <summary>PG3 V1: comets in a fixed pool: a bright streak that flies from one point to another and runs out (a tracer round, a beam's flash).
    /// Each draws a wide soft sleeve and a hot thin core; the head leads and the tail follows (Fx74Math.Comet).</summary>
    public sealed class Fx74Comets
    {
        struct Comet
        {
            public bool alive;
            public Vector3 from, to;
            public float age, travelS, settleS, tailM, width, sleeve;
            public Color core, glow;
        }

        readonly Comet[] pool;
        int next;

        public int Live, Added;

        public Fx74Comets(int capacity) { pool = new Comet[capacity]; }

        public void Clear() { for (int i = 0; i < pool.Length; i++) pool[i].alive = false; Live = 0; }

        public void Add(Vector3 from, Vector3 to, float travelS, float settleS, float tailM, float width, float sleeveWidth, Color core, Color glow)
        {
            ref Comet c = ref pool[next];
            next = (next + 1) % pool.Length;
            c.alive = true; c.from = from; c.to = to; c.age = 0f; c.travelS = travelS; c.settleS = settleS; c.tailM = tailM; c.width = width; c.sleeve = sleeveWidth; c.core = core; c.glow = glow;
            Added++;
        }

        public void StepAndDraw(float dt, Camera cam)
        {
            int live = 0;
            for (int i = 0; i < pool.Length; i++)
            {
                ref Comet c = ref pool[i];
                if (!c.alive) continue;
                c.age += dt;
                if (c.age >= Fx74Math.CometLife(c.travelS, c.settleS)) { c.alive = false; continue; }
                live++;
                Vector3 d = c.to - c.from;
                float len = d.magnitude;
                if (len < 1e-3f) continue;
                Vector3 dir = d / len;
                Fx74Math.Comet(len, c.age, c.travelS, c.tailM, c.settleS, out float tail, out float head);
                if (head - tail < 1e-3f) continue;
                float fade = c.age > c.travelS ? 1f - Fx74Math.Clamp01((c.age - c.travelS) / Mathf.Max(1e-4f, c.settleS)) * 0.5f : 1f;
                Vector3 a = c.from + dir * tail, b = c.from + dir * head;
                if (c.sleeve > 0f) Fx74Draw.Ribbon(cam, a, b, c.sleeve, c.glow * fade);
                Fx74Draw.Ribbon(cam, a, b, c.width, c.core * fade);
            }
            Live = live;
        }
    }
}
