// PG3 V1 (WEAPON FEEL, row 1; view row design.n74.shot74, read EVERY frame with NightRows.ViewOn; off: -skygrindRows72 shot74:0 and the game draws exactly as before).
// GG 2026-10-28: guns "very satisfying", visuals "much higher quality". The KESTREL's and the THUNDERCLAP's shot, in layers added on top of what WeaponFx, Fx47 and
// Fx49 draw (nothing of theirs is replaced or touched; the row off draws none of this):
//   - the MUZZLE FLASH: a 2-4 frame layered flash at the post-kick muzzle (a hot core, a soft cyan halo, two crossed lens-flare stars, a brief point light);
//     the THUNDERCLAP's is bigger and throws a "wall": a ring and a soft disc across its cone a few metres out, with a star at the muzzle;
//   - the TRACER: a hot comet (a white core in a cyan sleeve) that flies from the muzzle to the shot's end in a few frames and runs out, so the round is SEEN to
//     go; every THUNDERCLAP pellet flies one too (WeaponFx hands each cosmetic pellet's end to Shot74.Pellet), so the spread reads as a wall of streaks;
//   - the IMPACT on the world: a shock ring and a small hot decal that cools from white-hot to nothing on the surface, a star, and sparks that fly off the
//     surface along its normal and fall;
//   - the BURST on an enemy: bigger and different from the world's: a hot flash, a big star, an expanding ring and a spray of sparks leaning at the camera,
//     bigger for a headshot or a kill; one burst per enemy per frame however many pellets landed (they grow it instead).
// Colours: cyan (colors.usable) and white-hot only: the player's things (CLAUDE.md rule 8); never the enemy's red-orange. FLASH INTENSITY scales every flash;
// REDUCED MOTION and EFFECTS INTENSITY 0 drop the sparks. Sprites / sparks age on the world's time (frozen in a hitstop, so a kill's burst holds on the freeze);
// the muzzle flash ages on frame time. Pooled: nothing is allocated per frame once built (Fx74.cs). The cosmetic dice are this view's own Mulberry32, seeded per
// run (CLAUDE.md rule 6); the sim never sees them. Self-installing (Reveal74View's way): a hidden object made at scene load finds the live SurvivalGame and hooks
// its bus, OnRender and OnNewRun. The proof: Shot74Probe.Line(), the `[shot74]` line in the Player log at quit (what was drawn). Numbers: UnityTuning.PG3V1.cs.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>PG3 V1: what shot74 drew (the Player log's `[shot74]` line reads it; zero with the row off).</summary>
    public static class Shot74Probe
    {
        public static bool On;
        public static int Flashes, Tracers, Pellets, WorldImpacts, Decals, EnemyBursts, Sparks, Lights, Walls;
        public static int PeakQuads;

        public static void Reset() { Flashes = Tracers = Pellets = WorldImpacts = Decals = EnemyBursts = Sparks = Lights = Walls = 0; PeakQuads = 0; }

        public static string Line() =>
            $"[shot74] on {On}: muzzle flashes {Flashes} (walls {Walls}, lights {Lights}), tracers {Tracers} (pellet tracers {Pellets}), world impacts {WorldImpacts} (hot decals {Decals}), enemy bursts {EnemyBursts}, sparks {Sparks}, peak quads in a frame {PeakQuads}";
    }

    public sealed class Shot74View : MonoBehaviour
    {
        public const string Row = "shot74";

        // ---- the pellet hook: WeaponFx hands each cosmetic THUNDERCLAP pellet's end here (Unity space), recorded only while the row is live ----
        struct PelletRec { public Vector3 to, normal; public bool world; }
        static readonly PelletRec[] pellets = new PelletRec[32];
        static int pelletN;

        /// <summary>True while the row reads on this frame (WeaponFx checks it before it hands a pellet over).</summary>
        public static bool RowLive;

        /// <summary>WeaponFx's cosmetic pellet ended at `to` (Unity space); `world` when it ended on a wall, with that wall's normal (facing the shooter).</summary>
        public static void Pellet(Vector3 to, Vector3 normal, bool world)
        {
            if (!RowLive || pelletN >= pellets.Length) return;
            pellets[pelletN++] = new PelletRec { to = to, normal = normal, world = world };
        }

        // ---- what the bus told us since the last frame (drained at the frame's render, after the viewmodel's kick moved the muzzle) ----
        struct ShotRec { public string weapon, surface; public Vec3 origin, end, normal; }
        struct HitRec { public int enemy; public Vec3 position; public bool headshot, killed; public string weapon; }
        readonly ShotRec[] shots = new ShotRec[8];
        readonly HitRec[] hits = new HitRec[32];
        int shotN, hitN;
        string fireGun;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Shot74View>() != null) return;
            var go = new GameObject("Shot74");
            DontDestroyOnLoad(go);
            go.AddComponent<Shot74View>();
        }

        SurvivalGame game;
        Mulberry32 rng = new Mulberry32(1);
        Fx74Sprites sprites;
        Fx74Sparks sparks;
        Fx74Comets comets;
        float starForward, gravity, sgRange, sgHalfDeg;
        Color cyan, hot;
        // the muzzle flash's state: the gun, its age (s of frame time), the rendered frames it has drawn, its roll
        string flashGun;
        float flashAge, flashRoll;
        int flashFrames;
        bool flashLive;
        // enemies burst this frame (one burst per enemy per frame)
        readonly int[] burstEnemy = new int[16];

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == game || g == null) return;
            try { Hook(g); }
            catch (System.Exception ex)
            {
                game = g; // no retry every frame
                faulted = true;
                Debug.LogError("[shot74] could not hook the game, stopped for this session: " + ex);
            }
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            sprites ??= new Fx74Sprites(UnityTuning.PG3V1SpritePool);
            sparks ??= new Fx74Sparks(UnityTuning.PG3V1SparkPool);
            comets ??= new Fx74Comets(UnityTuning.PG3V1CometPool);
            sprites.Clear(); sparks.Clear(); comets.Clear();
            shotN = hitN = pelletN = 0;
            flashLive = false;
            rng = new Mulberry32(g.runSeed ^ UnityTuning.PG3V1SeedSalt);
            GameConfig cfg = g.cfg;
            starForward = (float)cfg.Num("fx.flashForward");
            gravity = (float)cfg.Num("fx.gravity");
            sgRange = (float)cfg.Num("weapons.shotgun.range");
            sgHalfDeg = (float)cfg.Num("weapons.shotgun.coneHalfAngleDeg");
            cyan = Look.C(cfg, "colors.usable");
            hot = Color.Lerp(cyan, Color.white, UnityTuning.PG3V1HotWhite);
            EventBus ev = g.events;
            ev.On("fire", o => { if (o is string w && (w == "revolver" || w == "shotgun")) fireGun = w; });
            ev.On<ShotEvent>("shotResult", e =>
            {
                if (e == null || e.owner != "player" || e.rayCount == 0 || (e.weapon != "revolver" && e.weapon != "shotgun") || shotN >= shots.Length) return;
                shots[shotN++] = new ShotRec { weapon = e.weapon, surface = e.surface, origin = e.origin, end = e.end, normal = e.normal };
            });
            ev.On<HitEvent>("hit", e =>
            {
                if (e == null || e.owner != "player" || e.melee || e.shock || (e.weapon != "revolver" && e.weapon != "shotgun") || hitN >= hits.Length) return;
                hits[hitN++] = new HitRec { enemy = e.enemy, position = e.position, headshot = e.headshot, killed = e.killed, weapon = e.weapon };
            });
            g.OnRender += Render;
            g.OnNewRun += () =>
            {
                rng = new Mulberry32(g.runSeed ^ UnityTuning.PG3V1SeedSalt);
                sprites.Clear(); sparks.Clear(); comets.Clear();
                shotN = hitN = pelletN = 0;
                flashLive = false;
                if (Shot74Probe.Flashes + Shot74Probe.Tracers + Shot74Probe.EnemyBursts > 0) Debug.Log(Shot74Probe.Line());
            };
        }

        // the proof: what was drawn, printed to the Player log at quit
        void OnApplicationQuit() => Debug.Log(Shot74Probe.Line());

        float R() => (float)rng.Next();

        bool faulted;

        /// <summary>The frame hook: any exception in this additive, view-only layer logs once and stops the layer for the session (it must never break the game's frame).</summary>
        void Render(double frameDt, double alpha)
        {
            if (faulted) return;
            try { RenderCore(frameDt); }
            catch (System.Exception ex)
            {
                faulted = true;
                RowLive = false;
                Debug.LogError("[shot74] stopped for this session after an exception: " + ex);
            }
        }

        void RenderCore(double frameDt)
        {
            SurvivalGame g = game;
            if (g == null || g.cam == null || g.sim == null || g.cfg == null || g.mats == null) return;
            bool on = NightRows.ViewOn(g.cfg, Row); // read every frame (D1's contract)
            RowLive = on;
            Shot74Probe.On = on;
            if (!on || g.flow != Flow.Playing || g.attractOn)
            {
                shotN = hitN = pelletN = 0; // nothing is kept for a row that is off: the next frame on draws from the next event
                fireGun = null;
                if (!on) { sprites.Clear(); sparks.Clear(); comets.Clear(); flashLive = false; }
                return;
            }
            Camera cam = g.cam;
            Fx74Draw.Ensure(g.mats.neon);
            Fx74Draw.Begin();
            float dt = (float)frameDt, wdt = (float)g.WorldDt;
            bool still = g.cfg.movement.settings.reducedMotion || g.cfg.movement.settings.effectsIntensity <= 0;
            float fi = Mathf.Clamp01((float)g.settings.View.flashIntensity);

            // the muzzle (post-kick: the viewmodel rendered before OnRender), the same point the muzzle star uses
            Vector3 muzzle = default;
            bool hasMuzzle = g.viewmodel != null && g.viewmodel.MuzzleWorld(out muzzle);
            if (!hasMuzzle) muzzle = cam.transform.position + cam.transform.forward * UnityTuning.FxMuzzleDepth;
            muzzle += cam.transform.forward * starForward;

            // ---- a shot this frame: start the flash ----
            if (fireGun != null)
            {
                flashGun = fireGun;
                flashAge = 0f;
                flashFrames = 0;
                flashRoll = R() * 360f;
                flashLive = true;
                fireGun = null;
                Shot74Probe.Flashes++;
                bool sg = flashGun == "shotgun";
                if (fi > 0f && hasMuzzle)
                {
                    LightPools lights = g.lightAndLens?.lights;
                    if (lights != null)
                    {
                        lights.Flash(muzzle, cyan, sg ? UnityTuning.PG3V1LightThunder : UnityTuning.PG3V1LightKestrel, UnityTuning.PG3V1LightRangeM, UnityTuning.PG3V1LightS);
                        Shot74Probe.Lights++;
                    }
                    if (sg) SpawnWall(cam, muzzle, fi);
                }
            }

            // ---- the shots: a tracer each, an impact on the world ----
            for (int i = 0; i < shotN; i++) DrawShot(shots[i], cam, muzzle, still);
            shotN = 0;
            for (int i = 0; i < pelletN; i++) DrawPellet(pellets[i], cam, muzzle, still);
            pelletN = 0;
            DrawHits(cam, muzzle, still);

            // ---- the muzzle flash ----
            if (flashLive)
            {
                bool sg = flashGun == "shotgun";
                float life = sg ? UnityTuning.PG3V1FlashThunderS : UnityTuning.PG3V1FlashS;
                if (!Fx74Math.FlashAlive(flashAge, flashFrames, life, UnityTuning.PG3V1FlashMinFrames)) flashLive = false;
                else
                {
                    if (hasMuzzle && fi > 0f) DrawFlash(cam, muzzle, sg, life, fi);
                    if (dt > 0f) { flashAge += dt; flashFrames++; } // drawn at its age, then older (a held same-run frame has dt 0: it stands still)
                }
            }

            // ---- the pools draw: impacts and sparks wait for their tracer on real time, then age on the world's (a hit freeze holds the burst); the tracers fly on real time ----
            sprites.StepAndDraw(wdt, dt, cam);
            sparks.StepAndDraw(wdt, dt, gravity, UnityTuning.PG3V1SparkDrag, cam);
            comets.StepAndDraw(dt, cam);
            Fx74Draw.End();
            if (Fx74Draw.PeakQuads > Shot74Probe.PeakQuads) Shot74Probe.PeakQuads = Fx74Draw.PeakQuads;
        }

        // ================================================= the flash =================================================

        void DrawFlash(Camera cam, Vector3 at, bool sg, float life, float fi)
        {
            // the first frame is whole, then it snaps down (Fx74Math.Env: hold a quarter of the life, then squared); the minimum-frames rule keeps a short frame time lit
            float env = Mathf.Max(Fx74Math.Env(flashAge, life, UnityTuning.PG3V1FlashHold), flashFrames < UnityTuning.PG3V1FlashMinFrames ? UnityTuning.PG3V1FlashMinEnv : 0f) * fi;
            if (env <= 0.003f) return;
            float k = sg ? UnityTuning.PG3V1ThunderScale : 1f;
            Color white = Color.white;
            // a soft cyan halo, a hot core, two crossed stars (one rolled off the other): the layers read at any distance
            float dm = (at - cam.transform.position).magnitude; // every size is capped in degrees of view from the eye (GG's NIGHT43 "filled most of the screen")
            Fx74Draw.Billboard(Fx74Tex.Soft, cam, at, Fx74Math.CapDeg(UnityTuning.PG3V1FlashHaloM * k, dm, UnityTuning.PG3V1CapSoftDeg), 0f, cyan * (UnityTuning.PG3V1FlashHaloGain * env));
            Fx74Draw.Billboard(Fx74Tex.Star, cam, at, Fx74Math.CapDeg(UnityTuning.PG3V1FlashStarM * k * (0.85f + 0.15f * env), dm, UnityTuning.PG3V1CapStarDeg), flashRoll, Color.Lerp(cyan, white, sg ? 0.55f : 0.3f) * (UnityTuning.PG3V1FlashStarGain * env));
            Fx74Draw.Billboard(Fx74Tex.Star, cam, at, Fx74Math.CapDeg(UnityTuning.PG3V1FlashStar2M * k, dm, UnityTuning.PG3V1CapStarDeg), flashRoll + UnityTuning.PG3V1FlashStar2RollDeg, white * (UnityTuning.PG3V1FlashStar2Gain * env));
            Fx74Draw.Billboard(Fx74Tex.Soft, cam, at, Fx74Math.CapDeg(UnityTuning.PG3V1FlashCoreM * k * (0.7f + 0.3f * env), dm, UnityTuning.PG3V1CapSoftDeg), 0f, white * (UnityTuning.PG3V1FlashCoreGain * env));
            // streak spikes: thin hot rays out of the muzzle in the screen's plane (hottest at the muzzle), each at a fixed angle for the whole flash
            int spikes = sg ? UnityTuning.PG3V1FlashSpikesThunder : UnityTuning.PG3V1FlashSpikes;
            Transform ct = cam.transform;
            for (int i = 0; i < spikes; i++)
            {
                float a = (flashRoll + i * (360f / spikes) + (Hash01(flashRoll, i) - 0.5f) * UnityTuning.PG3V1FlashSpikeJitterDeg) * Mathf.Deg2Rad;
                float len = UnityTuning.PG3V1FlashSpikeLenM * k * (0.55f + 0.7f * Hash01(flashRoll + 7.7f, i)) * (0.6f + 0.4f * env);
                Vector3 dir = ct.right * Mathf.Cos(a) + ct.up * Mathf.Sin(a);
                Fx74Draw.Ribbon(cam, at + dir * len, at, UnityTuning.PG3V1FlashSpikeWidthM * k, Color.Lerp(cyan, white, 0.5f) * (UnityTuning.PG3V1FlashSpikeGain * env));
            }
            if (sg)
            {
                // the cone's wall: a ring blown out from the muzzle along the aim (the disc across the cone is spawned once, in SpawnWall)
                float u = Mathf.Clamp01(flashAge / life);
                Vector3 ringAt = at + cam.transform.forward * (UnityTuning.PG3V1ThunderRingAheadM + UnityTuning.PG3V1ThunderRingTravelM * u);
                Fx74Draw.Facing(Fx74Tex.Ring, ringAt, cam.transform.forward, Fx74Math.CapDeg(Fx74Math.SizeAt(UnityTuning.PG3V1ThunderRingFromM, UnityTuning.PG3V1ThunderRingToM, u), (ringAt - cam.transform.position).magnitude, UnityTuning.PG3V1CapRingDeg), 0f, white * (UnityTuning.PG3V1ThunderRingGain * env));
            }
        }

        /// <summary>A repeatable 0..1 from a seed and an index (no dice: the same flash draws the same spikes every frame).</summary>
        static float Hash01(float seed, int i)
        {
            float x = Mathf.Sin(seed * 12.9898f + i * 78.233f) * 43758.5453f;
            return x - Mathf.Floor(x);
        }

        /// <summary>The THUNDERCLAP's wall: a soft disc and a ring across the spread's cone a few metres out, growing and fading over a few frames, at the cone's real width.</summary>
        void SpawnWall(Camera cam, Vector3 muzzle, float fi)
        {
            Vector3 aim = cam.transform.forward;
            float d = UnityTuning.PG3V1WallDistM;
            float w = Fx74Math.Subtend(d, sgHalfDeg * 2f * UnityTuning.PG3V1WallWidth);
            Vector3 c = muzzle + aim * d;
            sprites.Facing(Fx74Tex.Soft, c, aim, w * 0.5f, w, UnityTuning.PG3V1WallS, 0.1f, cyan * (UnityTuning.PG3V1WallDiscGain * fi), cyan * (0.1f * fi));
            sprites.Facing(Fx74Tex.Ring, c + aim * 0.02f, aim, w * 0.6f, w * 1.15f, UnityTuning.PG3V1WallS * 1.1f, 0f, hot * (UnityTuning.PG3V1WallRingGain * fi), cyan * (0.3f * fi));
            Shot74Probe.Walls++;
        }

        // ================================================= tracers and impacts =================================================

        void DrawShot(ShotRec s, Camera cam, Vector3 muzzle, bool still)
        {
            bool sg = s.weapon == "shotgun";
            Vector3 to = WebSpace.ToUnity(s.end);
            Tracer(cam, muzzle, to, sg ? UnityTuning.PG3V1PelletCoreM * UnityTuning.PG3V1PrimaryPelletK : UnityTuning.PG3V1TracerCoreM, sg ? UnityTuning.PG3V1PelletSleeveM : UnityTuning.PG3V1TracerSleeveM, sg ? UnityTuning.PG3V1PelletTailM : UnityTuning.PG3V1TracerTailM, sg);
            if (s.surface != "world" || (sg && Vec3.DistanceTo(s.origin, s.end) >= sgRange - UnityTuning.ShotgunMissTolerance)) return; // an enemy's hit has its own burst; a THUNDERCLAP in the air no impact
            Vector3 n = Vector3.zero;
            Vec3 nrm = s.normal;
            if (nrm.x != 0 || nrm.y != 0 || nrm.z != 0) n = WebSpace.ToUnity(nrm);
            else if (!SurfaceNormal(s.origin, s.end, out n)) return;
            if (Vector3.Dot(n, cam.transform.position - to) < 0f) n = -n; // face the shooter
            WorldImpact(to, n, sg ? UnityTuning.PG3V1PelletImpactK : 1f, sg ? UnityTuning.PG3V1PelletSparks : UnityTuning.PG3V1WorldSparks, still, ArriveS(muzzle, to));
        }

        void DrawPellet(PelletRec p, Camera cam, Vector3 muzzle, bool still)
        {
            Tracer(cam, muzzle, p.to, UnityTuning.PG3V1PelletCoreM, UnityTuning.PG3V1PelletSleeveM, UnityTuning.PG3V1PelletTailM, true);
            Shot74Probe.Pellets++;
            if (p.world && p.normal.sqrMagnitude > 0f) WorldImpact(p.to, p.normal, UnityTuning.PG3V1PelletImpactK, UnityTuning.PG3V1PelletSparks, still, ArriveS(muzzle, p.to));
        }

        /// <summary>The real seconds until the tracer's head reaches `to` (x the lead): the impact and the burst wait this long so the round is seen to arrive.</summary>
        static float ArriveS(Vector3 from, Vector3 to) =>
            Fx74Math.TravelS((to - from).magnitude, UnityTuning.PG3V1TracerSpeed, UnityTuning.PG3V1TracerMaxTravelS) * UnityTuning.PG3V1ImpactLead;

        /// <summary>One hot comet muzzle to end: a white core in a cyan sleeve, the head flying out in a few frames, the tail running on after it.</summary>
        void Tracer(Camera cam, Vector3 from, Vector3 to, float core, float sleeve, float tail, bool pellet)
        {
            float len = (to - from).magnitude;
            if (len < 0.05f) return;
            float travel = Fx74Math.TravelS(len, UnityTuning.PG3V1TracerSpeed, UnityTuning.PG3V1TracerMaxTravelS);
            comets.Add(from, to, travel, UnityTuning.PG3V1TracerSettleS, tail, core, sleeve, Color.Lerp(cyan, Color.white, 0.8f) * UnityTuning.PG3V1TracerCoreGain, cyan * (pellet ? UnityTuning.PG3V1PelletSleeveGain : UnityTuning.PG3V1TracerSleeveGain));
            Shot74Probe.Tracers++;
        }

        /// <summary>A world hit: a shock ring and a small hot decal that cools, a star, and sparks that leave the surface along its normal and fall.</summary>
        void WorldImpact(Vector3 at, Vector3 n, float k, int sparkN, bool still, float delay)
        {
            Vector3 p = at + n * UnityTuning.PG3V1DecalLiftM;
            float s = k * UnityTuning.PG3V1ImpactM;
            sprites.Facing(Fx74Tex.Ring, p, n, UnityTuning.PG3V1ImpactRingFromK * s, UnityTuning.PG3V1ImpactRingToK * s, UnityTuning.PG3V1ImpactRingS, 0f, Color.white * UnityTuning.PG3V1ImpactRingGain, cyan * UnityTuning.PG3V1ImpactRingEdge, R() * 360f, delay);
            sprites.Facing(Fx74Tex.Soft, p + n * UnityTuning.PG3V1DecalOverRingM, n, UnityTuning.PG3V1DecalFromK * s, UnityTuning.PG3V1DecalToK * s, UnityTuning.PG3V1DecalS, 0.05f, hot * UnityTuning.PG3V1DecalGain, cyan * UnityTuning.PG3V1DecalCool, 0f, delay);
            // the star stands off the wall toward the shooter, capped in the view's degrees (a wall hit at arm's length is not a screen-filling flash)
            Vector3 starAt = p + n * UnityTuning.PG3V1ImpactStarLiftM;
            float ds = (starAt - game.cam.transform.position).magnitude;
            sprites.Billboard(Fx74Tex.Star, starAt, Fx74Math.CapDeg(UnityTuning.PG3V1ImpactStarFromK * s, ds, UnityTuning.PG3V1CapStarDeg), Fx74Math.CapDeg(UnityTuning.PG3V1ImpactStarToK * s, ds, UnityTuning.PG3V1CapStarDeg), UnityTuning.PG3V1ImpactStarS, 0.2f, Color.white * UnityTuning.PG3V1ImpactStarGain, cyan * UnityTuning.PG3V1ImpactStarEdge, R() * 360f, delay);
            Shot74Probe.WorldImpacts++;
            Shot74Probe.Decals++;
            if (still) return;
            Vector3 t1 = Vector3.Cross(n, Mathf.Abs(n.y) > 0.9f ? Vector3.right : Vector3.up).normalized, t2 = Vector3.Cross(n, t1);
            for (int i = 0; i < sparkN; i++)
            {
                float a = R() * Mathf.PI * 2f, spread = Mathf.Lerp(0.25f, 1.1f, R());
                Vector3 dir = (n * Mathf.Lerp(0.5f, 1.3f, R()) + (t1 * Mathf.Cos(a) + t2 * Mathf.Sin(a)) * spread).normalized;
                float speed = Mathf.Lerp(UnityTuning.PG3V1SparkSpeedMin, UnityTuning.PG3V1SparkSpeedMax, R());
                sparks.Add(p + n * 0.01f, dir * speed, Mathf.Lerp(UnityTuning.PG3V1SparkLifeMin, UnityTuning.PG3V1SparkLifeMax, R()), UnityTuning.PG3V1SparkLenM, UnityTuning.PG3V1SparkWidthM, Color.Lerp(cyan, Color.white, R() * 0.9f) * UnityTuning.PG3V1SparkGain, delay);
                Shot74Probe.Sparks++;
            }
        }

        bool SurfaceNormal(Vec3 origin, Vec3 end, out Vector3 n)
        {
            n = default;
            Vec3 d = end - origin;
            double len = d.Length;
            if (!(len > 0) || !game.world.RayIntersect(origin, d / len, out RayHit hit) || System.Math.Abs(hit.distance - len) > UnityTuning.DecalOnSurface) return false;
            Vec3 nn = Vec3.Dot(hit.normal, d) > 0 ? -hit.normal : hit.normal;
            n = WebSpace.ToUnity(nn);
            return true;
        }

        // ================================================= the enemy burst =================================================

        void DrawHits(Camera cam, Vector3 muzzle, bool still)
        {
            int done = 0;
            for (int i = 0; i < hitN; i++)
            {
                int id = hits[i].enemy;
                bool seen = false;
                for (int k = 0; k < done; k++) if (burstEnemy[k] == id) { seen = true; break; }
                if (seen) continue; // one burst per enemy a frame: its other pellets grow it
                if (done < burstEnemy.Length) burstEnemy[done++] = id;
                int count = 0;
                bool head = false, kill = false;
                for (int k = i; k < hitN; k++)
                    if (hits[k].enemy == id) { count++; head |= hits[k].headshot; kill |= hits[k].killed; }
                float scale = Mathf.Min(UnityTuning.PG3V1BurstMaxK, 1f + UnityTuning.PG3V1BurstPerPellet * (count - 1)) * (head ? UnityTuning.PG3V1BurstHeadK : 1f) * (kill ? UnityTuning.PG3V1BurstKillK : 1f);
                Vector3 hitAt = WebSpace.ToUnity(hits[i].position);
                EnemyBurst(cam, hitAt, scale, head || kill, still, ArriveS(muzzle, hitAt));
            }
            hitN = 0;
        }

        /// <summary>A burst at an enemy hit: bigger and different from the world's: a hot flash, a big star, a ring blown out, sparks leaning at the camera.
        /// It waits `delay` (the tracer's flight) on real time, then holds through a hit freeze (it ages on the world's time).</summary>
        void EnemyBurst(Camera cam, Vector3 at, float k, bool big, bool still, float delay)
        {
            Vector3 toCam = cam.transform.position - at;
            float d = toCam.magnitude;
            if (d > 1e-3f) at += toCam / d * Mathf.Min(UnityTuning.PG3V1BurstLiftMaxM, d * 0.3f); // on the camera's side of the skin, a short way
            float s = k * UnityTuning.PG3V1BurstM * Mathf.Clamp(d / UnityTuning.PG3V1BurstFarRefM, 1f, UnityTuning.PG3V1BurstFarMaxK); // a far enemy's burst grows so it reads
            float dc = (at - cam.transform.position).magnitude; // the burst's own distance from the eye: a close one is capped in the view's degrees, never a screen-filling flash
            sprites.Billboard(Fx74Tex.Soft, at, Fx74Math.CapDeg(UnityTuning.PG3V1BurstFlashFromK * s, dc, UnityTuning.PG3V1CapSoftDeg), Fx74Math.CapDeg(UnityTuning.PG3V1BurstFlashToK * s, dc, UnityTuning.PG3V1CapSoftDeg), UnityTuning.PG3V1BurstFlashS, 0.2f, Color.white * UnityTuning.PG3V1BurstFlashGain, cyan * UnityTuning.PG3V1BurstFlashEdge, 0f, delay);
            sprites.Billboard(Fx74Tex.Star, at, Fx74Math.CapDeg(UnityTuning.PG3V1BurstStarFromK * s, dc, UnityTuning.PG3V1CapStarDeg), Fx74Math.CapDeg(UnityTuning.PG3V1BurstStarToK * s, dc, UnityTuning.PG3V1CapStarDeg), UnityTuning.PG3V1BurstStarS, 0.25f, Color.Lerp(cyan, Color.white, 0.5f) * UnityTuning.PG3V1BurstStarGain, cyan * UnityTuning.PG3V1BurstStarEdge, R() * 360f, delay);
            sprites.Billboard(Fx74Tex.Ring, at, Fx74Math.CapDeg(UnityTuning.PG3V1BurstRingFromK * s, dc, UnityTuning.PG3V1CapRingDeg), Fx74Math.CapDeg(UnityTuning.PG3V1BurstRingToK * s, dc, UnityTuning.PG3V1CapRingDeg), UnityTuning.PG3V1BurstRingS, 0f, Color.white * UnityTuning.PG3V1BurstRingGain, cyan * UnityTuning.PG3V1BurstRingEdge, 0f, delay);
            Shot74Probe.EnemyBursts++;
            Vector3 lean = d > 1e-3f ? toCam / d : Vector3.up;
            int n = Mathf.RoundToInt((big ? UnityTuning.PG3V1BurstSparksBig : UnityTuning.PG3V1BurstSparks) * Mathf.Min(1.5f, k));
            if (!still)
                for (int i = 0; i < n; i++)
                {
                    Vector3 v = (SprayMath.UnitSphere(rng.Next(), rng.Next()) + lean * UnityTuning.PG3V1BurstLean).normalized;
                    sparks.Add(at, v * Mathf.Lerp(UnityTuning.PG3V1BurstSpeedMin, UnityTuning.PG3V1BurstSpeedMax, R()), Mathf.Lerp(UnityTuning.PG3V1SparkLifeMin, UnityTuning.PG3V1SparkLifeMax * 1.3f, R()), UnityTuning.PG3V1SparkLenM * 1.25f, UnityTuning.PG3V1SparkWidthM * 1.2f, Color.Lerp(cyan, Color.white, R() * 0.9f) * UnityTuning.PG3V1SparkGain, delay);
                    Shot74Probe.Sparks++;
                }
            LightPools lights = game.lightAndLens?.lights;
            if (lights != null && d > UnityTuning.PG3V1BurstLightNearM)
            {
                lights.Flash(at, cyan, UnityTuning.PG3V1BurstLight, UnityTuning.PG3V1LightRangeM, UnityTuning.PG3V1LightS);
                Shot74Probe.Lights++;
            }
        }
    }
}
