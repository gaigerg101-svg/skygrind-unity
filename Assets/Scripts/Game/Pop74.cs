// PG3 V3 (KILL POP, row 1 of the kill pass; view row design.n74.pop74, read EVERY frame with NightRows.ViewOn; off: -skygrindRows72 pop74:0 and the game draws exactly as before).
// GG, for weeks: kills "not crunchy". A regular enemy's death (the bus's "kill", KillEvent.kind != "boss") gets a pop in layers on top of what EnemyLook.Die, Chunks, the
// kill ring and the scorch51 mark already draw (nothing of theirs is replaced or touched; the row off draws none of this):
//   - a CORE FLASH at the body, held 4-6 frames (real time: a hit freeze does not hold it), the core about 1.5x the body's height: a white-hot centre into a red-orange halo, and a lens-flare star;
//   - a RING that expands to about 2 m (a husk), bigger for a head / melee kill (tier 1) and the biggest, with a second ring a beat later, for a multikill (tier 2);
//   - 10-24 chunky SHARDS in the enemy's own red-orange: angular, spinning, flung out and up, falling with gravity and bouncing on the floor under the body;
//   - an EMBER spray (small hot streaks) that drifts out and falls with gravity;
//   - a short-lived SCORCH on the floor under the body: a hot mark that cools to nothing in 1.5 s.
// Sizes by the kind's body height (Pop74Math.BodyM); the tier comes from the kill confirm (fx:killConfirm, KillConfirmEvent.tier: 0 plain, 1 head / melee, 2 multikill), or from the
// kill itself when none came. Colours: the enemy's red-orange (colors.enemy) and white-hot only (CLAUDE.md rule 8: the player's cyan never; no gold, it is points only). No
// full-screen flash: every flash size is capped in degrees of the view from the eye; FLASH INTENSITY scales every flash and the light; REDUCED MOTION and EFFECTS INTENSITY 0 drop
// the shards and the embers. A crowded frame (a blast's horde) thins each pop's shards and embers (Pop74Math.LodK) and never its flash. Sprites, rings, shards and embers age on the
// world's time (frozen in a hitstop, so a kill's burst holds on the freeze); the core flash ages on frame time. Pooled: nothing is allocated per kill once the kit is built (the
// quad, the shard texture and the material are made once, the way Fx74Draw's are; a draw is one Graphics.RenderMesh with a reused property block). The cosmetic dice are this
// view's own Mulberry32, seeded per run (CLAUDE.md rule 6); the sim never sees them. Self-installing (Shot74View's way): a hidden object made at scene load finds the live
// SurvivalGame and hooks its bus, OnRender and OnNewRun. The proof: Pop74Probe.Line(), the `[pop74]` line in the Player log at quit (what was drawn). Numbers: UnityTuning.PG3V3.cs.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>PG3 V3: what pop74 drew (the Player log's `[pop74]` line reads it; zero with the row off).</summary>
    public static class Pop74Probe
    {
        public static bool On;
        public static int Pops, Tier0, Tier1, Tier2, Confirmed, Thinned, Flashes, Rings, Scorches, NoFloor, Shards, Embers, Lights, Skipped;
        public static int PeakQuads;

        public static void Reset() { Pops = Tier0 = Tier1 = Tier2 = Confirmed = Thinned = Flashes = Rings = Scorches = NoFloor = Shards = Embers = Lights = Skipped = 0; PeakQuads = 0; }

        public static string Line() =>
            $"[pop74] on {On}: pops {Pops} (tier 0 {Tier0}, tier 1 {Tier1}, tier 2 {Tier2}; confirmed {Confirmed}; thinned {Thinned}), core flashes {Flashes}, rings {Rings}, scorches {Scorches} (no floor {NoFloor}), shards {Shards}, embers {Embers}, lights {Lights}, skipped {Skipped}, peak quads in a frame {PeakQuads}";
    }

    /// <summary>PG3 V3: the shard's one procedural texture and its additive material (Fx74Draw's recipe: the neon material copied with Blend One One, no depth write, the
    /// transparent queue, both faces), and the draw: one Graphics.RenderMesh with a reused property block. Static: one kit for the process, made on first use.</summary>
    public static class Pop74Draw
    {
        static Mesh quad;
        static Texture2D tex;
        static Material mat, neonSrc;
        static readonly MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>Shard quads submitted since the last Begin (this frame), and in all.</summary>
        public static int Quads, TotalQuads;

        public static void Begin() { Quads = 0; }

        public static void Ensure(Material neon)
        {
            if (neon == null) return;
            if (quad == null) { quad = MakeQuad(); tex = MakeShardTexture(UnityTuning.PG3V3ShardTexSize); }
            if (mat != null && ReferenceEquals(neonSrc, neon)) return;
            if (mat != null) Object.Destroy(mat);
            neonSrc = neon;
            mat = FxDraw.Additive(neon, "PG3 V3 shard");
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor(BaseColor, Color.black);
        }

        /// <summary>A camera-facing quad `w` wide and `h` tall at `at`, rolled `rollDeg` about the view axis (HDR colour). Dropped past the pool's size in one frame.</summary>
        public static void Shard(Camera cam, Vector3 at, float w, float h, float rollDeg, Color col)
        {
            if (mat == null || Quads >= UnityTuning.PG3V3ShardPool) return;
            Quaternion q = cam.transform.rotation * Quaternion.Euler(0f, 0f, rollDeg);
            Vector3 ax = q * Vector3.right * w, ay = q * Vector3.up * h;
            Vector3 az = Vector3.Cross(ax.normalized, ay.normalized);
            var m = new Matrix4x4(new Vector4(ax.x, ax.y, ax.z, 0f), new Vector4(ay.x, ay.y, ay.z, 0f), new Vector4(az.x, az.y, az.z, 0f), new Vector4(at.x, at.y, at.z, 1f));
            mpb.SetColor(BaseColor, col);
            float reach = w + h + 0.1f;
            var rp = new RenderParams(mat) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false, matProps = mpb, worldBounds = new Bounds(at, Vector3.one * reach) };
            Graphics.RenderMesh(rp, quad, 0, m);
            Quads++;
            TotalQuads++;
        }

        static Mesh MakeQuad()
        {
            var m = new Mesh { name = "PG3 V3 quad" };
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

        /// <summary>The shard: a hard-edged irregular quadrilateral (a splinter, not a blob) with a bright rim and a dimmer body, so a spinning one has an edge that reads at speed.
        /// Brightness is white-grey (the colour is the quad's HDR tint); the shape is a convex polygon in -1..1, its distance from the edge decides the rim.</summary>
        public static Texture2D MakeShardTexture(int n, bool keepReadable = false)
        {
            // CCW vertices; the outward normal of edge (a to b) is (dy, -dx) / len; a point is inside when every edge's signed distance is under 0
            var vx = new[] { 0.05f, -0.60f, -0.20f, 0.65f };
            var vy = new[] { 1.00f, 0.10f, -1.00f, -0.30f };
            var nx = new float[4];
            var ny = new float[4];
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                float dx = vx[j] - vx[i], dy = vy[j] - vy[i], len = Mathf.Sqrt(dx * dx + dy * dy);
                nx[i] = dy / len;
                ny[i] = -dx / len;
            }
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false, true) { name = "PG3 V3 shard", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = ((x + 0.5f) / n) * 2f - 1f, v = ((y + 0.5f) / n) * 2f - 1f;
                    float d = float.NegativeInfinity;
                    for (int i = 0; i < 4; i++) d = Mathf.Max(d, nx[i] * (u - vx[i]) + ny[i] * (v - vy[i]));
                    float depth = -d; // 0 on the edge, more inside
                    float val = 0f;
                    if (depth > 0f)
                    {
                        float aa = Smooth(0f, 0.05f, depth); // a soft hard edge (about one texel)
                        float rim = 1f - Smooth(0f, 0.24f, depth); // bright just inside the edge
                        float spot = Mathf.Exp(-((u + 0.05f) * (u + 0.05f) + (v - 0.1f) * (v - 0.1f)) / 0.16f); // a warm spot in the body
                        val = aa * Fx74Math.Clamp01(0.42f + 0.58f * rim + 0.16f * spot);
                    }
                    byte b = (byte)Mathf.RoundToInt(Fx74Math.Clamp01(val) * 255f);
                    px[y * n + x] = new Color32(b, b, b, 255);
                }
            t.SetPixels32(px);
            t.Apply(false, !keepReadable); // the game's copy is uploaded and dropped from memory (Fx74Draw's way); the test keeps its copy readable
            return t;
        }
    }

    /// <summary>PG3 V3: shards in a fixed pool: a point with a velocity, a roll and a spin, flown by Pop74Math.ShardStep (gravity, drag, a bounce on its floor), drawn as a rolled
    /// camera-facing quad (Pop74Draw); the oldest is reused. It fades over the last part of its life.</summary>
    public sealed class Pop74Shards
    {
        struct Shard
        {
            public bool alive, rest;
            public Vector3 pos, vel;
            public float age, life, w, h, roll, spin, floorY;
            public Color c;
        }

        readonly Shard[] pool;
        int next;

        /// <summary>Shards alive after the last Step, and added since the pool was made.</summary>
        public int Live, Added;

        public Pop74Shards(int capacity) { pool = new Shard[capacity]; }

        public void Clear() { for (int i = 0; i < pool.Length; i++) pool[i].alive = false; Live = 0; }

        public void Add(Vector3 pos, Vector3 vel, float life, float w, float h, float roll, float spinDegPerS, float floorY, Color c)
        {
            ref Shard s = ref pool[next];
            next = (next + 1) % pool.Length;
            s.alive = true; s.rest = false; s.pos = pos; s.vel = vel; s.age = 0f; s.life = life; s.w = w; s.h = h; s.roll = roll; s.spin = spinDegPerS; s.floorY = floorY; s.c = c;
            Added++;
        }

        /// <summary>Fly and age every shard on the world's `dt` (frozen in a hit freeze, so the burst holds) and draw it.</summary>
        public void StepAndDraw(float dt, float gravity, float drag, Camera cam)
        {
            int live = 0;
            for (int i = 0; i < pool.Length; i++)
            {
                ref Shard s = ref pool[i];
                if (!s.alive) continue;
                s.age += dt;
                if (s.age >= s.life) { s.alive = false; continue; }
                live++;
                Pop74Math.ShardStep(ref s.pos, ref s.vel, ref s.rest, s.floorY, gravity, drag, dt);
                if (!s.rest) s.roll += s.spin * dt;
                float env = Fx74Math.Env(s.age, s.life, UnityTuning.PG3V3ShardHold);
                if (env <= 0.01f) continue;
                Pop74Draw.Shard(cam, s.pos, s.w, s.h, s.roll, s.c * env);
            }
            Live = live;
        }
    }

    public sealed class Pop74View : MonoBehaviour
    {
        public const string Row = "pop74";

        /// <summary>True while the row reads on this frame (the bus handlers check it: with the row off a kill is not even recorded).</summary>
        public static bool RowLive;

        // ---- what the bus told us since the last frame (drained at the frame's render; the kill confirm's events came earlier in the same frame) ----
        struct KillRec
        {
            public string kind;
            public Vec3 at;
            public bool head, melee, player, confirmed;
            public int tier; // the confirm's tier, -1 until one comes
            public float bodyM;
        }

        readonly KillRec[] kills = new KillRec[UnityTuning.PG3V3MaxKills];
        int killN;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Pop74View>() != null) return;
            var go = new GameObject("Pop74");
            DontDestroyOnLoad(go);
            go.AddComponent<Pop74View>();
        }

        SurvivalGame game;
        Mulberry32 rng = new Mulberry32(1);
        Fx74Sprites flashes, sprites; // flashes age on frame time, the rings and the scorch on the world's
        Fx74Sparks embers;
        Pop74Shards shards;
        float gravity;
        Color enemy;
        bool faulted;

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == game || g == null) return;
            try { Hook(g); }
            catch (System.Exception ex)
            {
                game = g; // no retry every frame
                faulted = true;
                Debug.LogError("[pop74] could not hook the game, stopped for this session: " + ex);
            }
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            flashes ??= new Fx74Sprites(UnityTuning.PG3V3FlashPool);
            sprites ??= new Fx74Sprites(UnityTuning.PG3V3SpritePool);
            embers ??= new Fx74Sparks(UnityTuning.PG3V3EmberPool);
            shards ??= new Pop74Shards(UnityTuning.PG3V3ShardPool);
            ClearAll();
            rng = new Mulberry32(g.runSeed ^ UnityTuning.PG3V3SeedSalt);
            GameConfig cfg = g.cfg;
            gravity = (float)cfg.Num("fx.gravity");
            enemy = Look.C(cfg, "colors.enemy");
            EventBus ev = g.events;
            ev.On<KillEvent>("kill", OnKill);
            ev.On<KillConfirmEvent>(MetaEvents.KillConfirm, OnConfirm);
            g.OnRender += Render;
            g.OnNewRun += () =>
            {
                rng = new Mulberry32(g.runSeed ^ UnityTuning.PG3V3SeedSalt);
                ClearAll();
                if (Pop74Probe.Pops > 0) Debug.Log(Pop74Probe.Line());
            };
        }

        void ClearAll()
        {
            flashes.Clear(); sprites.Clear(); embers.Clear(); shards.Clear();
            killN = 0;
        }

        // the proof: what was drawn, printed to the Player log at quit
        void OnApplicationQuit() => Debug.Log(Pop74Probe.Line());

        float R() => (float)rng.Next();

        // ================================================= the bus =================================================

        /// <summary>A regular enemy died (inside the sim step): note it. Bosses (kind "boss" and their bodies' kinds) and the RIVAL are not popped (Pop74Math.BodyM: the table's kinds only).</summary>
        void OnKill(KillEvent e)
        {
            if (!RowLive || e == null || e.kind == null || killN >= kills.Length) return;
            float body = Pop74Math.BodyM(e.kind);
            if (body <= 0f) { Pop74Probe.Skipped++; return; }
            kills[killN++] = new KillRec { kind = e.kind, at = e.position, head = e.headshot, melee = e.melee, player = e.owner == "player", tier = -1, bodyM = body };
        }

        /// <summary>The kill confirm (KillConfirm.Render, earlier in the frame than this view's render): the tier of the kill at this spot (x and z are the kill's; y is lifted).</summary>
        void OnConfirm(KillConfirmEvent e)
        {
            if (!RowLive || e == null || e.owner != "player") return;
            for (int i = 0; i < killN; i++)
            {
                if (kills[i].confirmed || !kills[i].player || !Pop74Math.SameSpot(kills[i].at.x, kills[i].at.z, e.position.x, e.position.z)) continue;
                kills[i].confirmed = true;
                kills[i].tier = e.tier;
                return;
            }
        }

        // ================================================= the frame =================================================

        /// <summary>The frame hook: any exception in this additive, view-only layer logs once and stops the layer for the session (it must never break the game's frame).</summary>
        void Render(double frameDt, double alpha)
        {
            if (faulted) return;
            try { RenderCore(frameDt); }
            catch (System.Exception ex)
            {
                faulted = true;
                RowLive = false;
                Debug.LogError("[pop74] stopped for this session after an exception: " + ex);
            }
        }

        void RenderCore(double frameDt)
        {
            SurvivalGame g = game;
            if (g == null || g.cam == null || g.sim == null || g.cfg == null || g.mats == null) return;
            bool on = NightRows.ViewOn(g.cfg, Row); // read every frame (D1's contract)
            RowLive = on;
            Pop74Probe.On = on;
            if (!on || g.flow != Flow.Playing || g.attractOn)
            {
                killN = 0; // nothing is kept for a row that is off: the next frame on draws from the next kill
                if (!on) { flashes.Clear(); sprites.Clear(); embers.Clear(); shards.Clear(); }
                return;
            }
            Camera cam = g.cam;
            Fx74Draw.Ensure(g.mats.neon);
            Pop74Draw.Ensure(g.mats.neon);
            Fx74Draw.Begin();
            Pop74Draw.Begin();
            float dt = (float)frameDt, wdt = (float)g.WorldDt;
            bool still = g.cfg.movement.settings.reducedMotion || g.cfg.movement.settings.effectsIntensity <= 0;
            float fi = Mathf.Clamp01((float)g.settings.View.flashIntensity);

            for (int i = 0; i < killN; i++) Pop(kills[i], i, cam, dt, fi, still);
            killN = 0;

            // ---- the pools draw: the core flash on real time; the rings, shards and embers on the world's (a hit freeze holds the burst) ----
            flashes.StepAndDraw(dt, cam);
            sprites.StepAndDraw(wdt, dt, cam);
            shards.StepAndDraw(wdt, gravity, UnityTuning.PG3V3ShardDrag, cam);
            embers.StepAndDraw(wdt, dt, gravity * UnityTuning.PG3V3EmberGravityK, UnityTuning.PG3V3EmberDrag, cam);
            int q = Fx74Draw.Quads + Pop74Draw.Quads;
            if (q > Pop74Probe.PeakQuads) Pop74Probe.PeakQuads = q;
        }

        // ================================================= one pop =================================================

        /// <summary>One kill's pop: the flash, the ring(s), the scorch, the shards, the embers, a light. `index` is its place among the frame's kills (a crowded frame thins the later ones).</summary>
        void Pop(KillRec k, int index, Camera cam, float dt, float fi, bool still)
        {
            int tier = Pop74Math.Tier(k.tier, k.head, k.melee);
            Vector3 body = WebSpace.ToUnity(k.at); // the body's aim point (KillEvent.position): the torso, a drone's centre
            Vector3 toCam = cam.transform.position - body;
            float d = toCam.magnitude;
            float unit = Pop74Math.Unit(k.bodyM, tier, d);
            Vector3 lean = d > 1e-3f ? toCam / d : Vector3.up;
            // the flash stands on the camera's side of the body, a short way (never buried in the chunks), capped in the view's degrees from the eye (GG: no full-screen flash)
            Vector3 at = d > 1e-3f ? body + lean * Mathf.Min(UnityTuning.PG3V3LiftMaxM, d * 0.3f) : body;
            float dc = (at - cam.transform.position).magnitude;
            Color white = Pop74Math.Hot(enemy, UnityTuning.PG3V3WhiteMix);

            Pop74Probe.Pops++;
            if (tier >= 2) Pop74Probe.Tier2++; else if (tier == 1) Pop74Probe.Tier1++; else Pop74Probe.Tier0++;
            if (k.confirmed) Pop74Probe.Confirmed++;
            if (Pop74Math.LodK(index) < 1f) Pop74Probe.Thinned++;

            float fl = fi * Pop74Math.FlashK(index); // FLASH INTENSITY x the crowd's dimming: scales every flash, the rings and the light
            if (fl > 0f)
            {
                fi = fl;
                // ---- the core flash: held 4-6 frames, real time ----
                float life = Pop74Math.FlashLife(UnityTuning.PG3V3FlashS, UnityTuning.PG3V3FlashMinFrames, dt, UnityTuning.PG3V3FlashMaxS);
                float hold = UnityTuning.PG3V3FlashHold;
                flashes.Billboard(Fx74Tex.Soft, at, Fx74Math.CapDeg(UnityTuning.PG3V3HaloFromK * unit, dc, UnityTuning.PG3V3CapSoftDeg), Fx74Math.CapDeg(UnityTuning.PG3V3HaloToK * unit, dc, UnityTuning.PG3V3CapSoftDeg),
                    life, hold, enemy * (UnityTuning.PG3V3HaloGain * fi), enemy * (UnityTuning.PG3V3HaloGain * 0.3f * fi));
                flashes.Billboard(Fx74Tex.Soft, at, Fx74Math.CapDeg(UnityTuning.PG3V3CoreFromK * unit, dc, UnityTuning.PG3V3CapSoftDeg), Fx74Math.CapDeg(UnityTuning.PG3V3CoreToK * unit, dc, UnityTuning.PG3V3CapSoftDeg),
                    life, hold, white * (UnityTuning.PG3V3CoreGain * fi), enemy * (UnityTuning.PG3V3CoreGain * 0.4f * fi));
                flashes.Billboard(Fx74Tex.Star, at, Fx74Math.CapDeg(UnityTuning.PG3V3StarFromK * unit, dc, UnityTuning.PG3V3CapStarDeg), Fx74Math.CapDeg(UnityTuning.PG3V3StarToK * unit, dc, UnityTuning.PG3V3CapStarDeg),
                    Mathf.Max(life, UnityTuning.PG3V3StarS), hold, Pop74Math.Hot(enemy, 0.5f) * (UnityTuning.PG3V3StarGain * fi), enemy * (UnityTuning.PG3V3StarGain * 0.4f * fi), R() * 360f);
                Pop74Probe.Flashes++;

                // ---- the ring(s), on the world's clock; the multikill's second one comes a beat later and wider ----
                float rr = Pop74Math.RingRadiusM(unit);
                float quad1 = Pop74Math.RingQuadM(rr);
                sprites.Billboard(Fx74Tex.Ring, at, Fx74Math.CapDeg(quad1 * UnityTuning.PG3V3RingFromK, dc, UnityTuning.PG3V3CapRingDeg), Fx74Math.CapDeg(quad1, dc, UnityTuning.PG3V3CapRingDeg),
                    UnityTuning.PG3V3RingS, 0f, Pop74Math.Hot(enemy, 0.6f) * (UnityTuning.PG3V3RingGain * fi), enemy * (UnityTuning.PG3V3RingEdge * fi));
                // the companion ring a little inside it: the V1 ring texture's band is thin, two of them side by side read as one thick ring
                sprites.Billboard(Fx74Tex.Ring, at, Fx74Math.CapDeg(quad1 * UnityTuning.PG3V3RingFromK * UnityTuning.PG3V3RingThickK, dc, UnityTuning.PG3V3CapRingDeg), Fx74Math.CapDeg(quad1 * UnityTuning.PG3V3RingThickK, dc, UnityTuning.PG3V3CapRingDeg),
                    UnityTuning.PG3V3RingS, 0f, Pop74Math.Hot(enemy, 0.6f) * (UnityTuning.PG3V3RingGain * UnityTuning.PG3V3RingThickGain * fi), enemy * (UnityTuning.PG3V3RingEdge * fi));
                Pop74Probe.Rings++;
                if (tier >= 2)
                {
                    float quad2 = Pop74Math.RingQuadM(rr * UnityTuning.PG3V3Ring2K);
                    sprites.Billboard(Fx74Tex.Ring, at, Fx74Math.CapDeg(quad2 * UnityTuning.PG3V3RingFromK, dc, UnityTuning.PG3V3CapRingDeg), Fx74Math.CapDeg(quad2, dc, UnityTuning.PG3V3CapRingDeg),
                        UnityTuning.PG3V3Ring2S, 0f, Pop74Math.Hot(enemy, 0.5f) * (UnityTuning.PG3V3Ring2Gain * fi), enemy * (UnityTuning.PG3V3RingEdge * fi), 0f, UnityTuning.PG3V3Ring2DelayS);
                    sprites.Billboard(Fx74Tex.Ring, at, Fx74Math.CapDeg(quad2 * UnityTuning.PG3V3RingFromK * UnityTuning.PG3V3RingThickK, dc, UnityTuning.PG3V3CapRingDeg), Fx74Math.CapDeg(quad2 * UnityTuning.PG3V3RingThickK, dc, UnityTuning.PG3V3CapRingDeg),
                        UnityTuning.PG3V3Ring2S, 0f, Pop74Math.Hot(enemy, 0.5f) * (UnityTuning.PG3V3Ring2Gain * UnityTuning.PG3V3RingThickGain * fi), enemy * (UnityTuning.PG3V3RingEdge * fi), 0f, UnityTuning.PG3V3Ring2DelayS);
                    Pop74Probe.Rings++;
                }

                // ---- a brief red-orange light on what is near the body ----
                LightPools lights = game.lightAndLens?.lights;
                if (lights != null && d > UnityTuning.PG3V3LightNearM)
                {
                    lights.Flash(at, enemy, Pop74Math.LightFor(tier) * fi, UnityTuning.PG3V3LightRangeM, UnityTuning.PG3V3LightS);
                    Pop74Probe.Lights++;
                }
            }

            // ---- the scorch: a hot mark on the floor under the body that cools (no floor within reach: none) ----
            bool hasFloor = FloorY(k.at, out float floorY);
            if (hasFloor)
            {
                sprites.Facing(Fx74Tex.Soft, new Vector3(body.x, floorY + UnityTuning.PG3V3ScorchLiftM, body.z), Vector3.up, UnityTuning.PG3V3ScorchFromK * unit, UnityTuning.PG3V3ScorchToK * unit,
                    UnityTuning.PG3V3ScorchS, UnityTuning.PG3V3ScorchHold, enemy * UnityTuning.PG3V3ScorchGain, enemy * UnityTuning.PG3V3ScorchCool, R() * 360f);
                Pop74Probe.Scorches++;
            }
            else Pop74Probe.NoFloor++;

            if (still) return;

            // ---- the shards: chunky, angular, flung out and up, falling and bouncing on the floor under the body ----
            float floor = hasFloor ? floorY : float.NegativeInfinity;
            float speedK = Pop74Math.ShardSpeedK(k.bodyM);
            int ns = Pop74Math.ShardCount(tier, index);
            for (int i = 0; i < ns; i++)
            {
                Vector3 dir = (SprayMath.UnitSphere(rng.Next(), rng.Next()) + Vector3.up * UnityTuning.PG3V3ShardUp + lean * UnityTuning.PG3V3ShardLean).normalized;
                Vector3 v = dir * (Mathf.Lerp(UnityTuning.PG3V3ShardSpeedMin, UnityTuning.PG3V3ShardSpeedMax, R()) * speedK);
                Vector3 p = body + Vector3.up * ((R() - 0.5f) * k.bodyM * 0.3f) + dir * 0.05f + v * UnityTuning.PG3V3PreRollS; // a shard has flown a moment already: a frozen first frame is a burst
                float w = Pop74Math.ShardWidthM(unit, R());
                float spin = Mathf.Lerp(UnityTuning.PG3V3ShardSpinMin, UnityTuning.PG3V3ShardSpinMax, R()) * (R() < 0.5f ? -1f : 1f);
                Color c = Color.Lerp(enemy, white, R() * 0.25f) * UnityTuning.PG3V3ShardGain;
                shards.Add(p, v, Mathf.Lerp(UnityTuning.PG3V3ShardLifeMin, UnityTuning.PG3V3ShardLifeMax, R()), w, w * UnityTuning.PG3V3ShardAspect, R() * 360f, spin, floor, c);
                Pop74Probe.Shards++;
            }

            // ---- the embers: small hot streaks that drift out and fall ----
            int ne = Pop74Math.EmberCount(tier, index);
            for (int i = 0; i < ne; i++)
            {
                Vector3 dir = (SprayMath.UnitSphere(rng.Next(), rng.Next()) + lean * UnityTuning.PG3V3ShardLean).normalized;
                Vector3 v = dir * Mathf.Lerp(UnityTuning.PG3V3EmberSpeedMin, UnityTuning.PG3V3EmberSpeedMax, R());
                embers.Add(body + v * UnityTuning.PG3V3PreRollS, v, Mathf.Lerp(UnityTuning.PG3V3EmberLifeMin, UnityTuning.PG3V3EmberLifeMax, R()), UnityTuning.PG3V3EmberLenM, UnityTuning.PG3V3EmberWidthM,
                    Color.Lerp(enemy, Color.white, R() * UnityTuning.PG3V3EmberHotMix) * UnityTuning.PG3V3EmberGain);
                Pop74Probe.Embers++;
            }
        }

        /// <summary>The floor under a point (web space): the first surface straight down within PG3V3FloorMaxM that faces up. False when there is none (a flier over a pit).</summary>
        bool FloorY(Vec3 origin, out float y)
        {
            y = 0f;
            Vec3 down = new Vec3(0, -1, 0);
            if (!game.world.RayIntersect(origin, down, out RayHit hit) || hit.distance > UnityTuning.PG3V3FloorMaxM) return false;
            double ny = hit.normal.y;
            if (ny < 0) ny = -ny; // the hit triangle's normal may face either way: a floor faces up from above
            if (ny < UnityTuning.PG3V3FloorMinNormalY) return false;
            y = (float)(origin.y - hit.distance);
            return true;
        }
    }
}
