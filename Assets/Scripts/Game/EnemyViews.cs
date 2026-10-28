// Enemy visuals (GG owns enemy design: these are the web's existing looks rebuilt as they are, nothing new) and the
// cube particle bursts, after skygrind/src/enemies/husk.js render() and fx/particles.js:
// - the HUSK: a capsule body (husk.radius, husk.height) and a hunched head box (husk.headBox, tilted headTiltDeg, at
//   headHeight / headForward) in config.colors.enemy, the head x headShade; the wind-up tell pulses the size
//   (windupPulseScale at windupPulseRate) and the colour toward husk.windupColor; a hit flashes white (hitFlashTime);
//   a stagger pales it (staggerPale) and leans it back (staggerLeanDeg, eased at staggerLeanEaseRate). Interpolated.
// - the SWARMER (swarmer.js parts): three boxes in colors.enemy x swarmer.shade (a 0.46 x 0.32 x 0.8 body at 0.45 of its
//   height, a 0.3 x 0.26 x 0.3 head 0.42 m forward at 0.62 x 0.8, 0.62 x 0.3 x 0.62 legs at 0.18 x 0.62); the crouch tell
//   squashes each box (1.15, 0.65, 1.1) and pulses the colour toward husk.windupColor; a hit flashes it white.
// - particles: fx.size cubes from a pool of fx.poolSize, fx.gravity, a burst per hit (fx.hitCount / hitSpeed / hitLife)
//   and per death (deathCount / deathSpeed / deathLife) in the enemy colour; directions are a hash (no RNG in the sim).
// NIGHT12 U3-25.3 / U3-25.4 (EnemyLook below): every part draws with SKYGRIND/Enemy (the unlit colour plus rimGlow.js's
// rim with a distance boost, the hit flash, xray.js's pass on each kind's body, dissolve.js on a dead enemy's frozen
// pose) and flinches / vibrates on a hit (render only); the particles are one instanced draw (RenderMeshInstanced).
// NIGHT17 U3-30.5 (builder X, the crunch; UnityTuning.Crunch.cs): every damaging hit flashes the enemy full white at once
// (held CrunchHitFlashHold, then eased back over CrunchHitFlashFade, frame time) and tips / pops it away from the shot (60-100 ms);
// the vibration runs across the screen (the camera's right and up: a head-on punch reads too); below CrunchShedAt of its
// HP an enemy sheds one block (thrown off, an INK notch where it was; view only; port-only event `crunchShed`); a kill
// (CHUNKS ON) breaks the enemy into its own blocks (Game/Chunks.cs) on the freeze's first frame, white and vibrating while
// the freeze holds, then thrown along the shot. Inside a freeze the particles drift at CrunchFreezeDrift on frame time.
// NIGHT18 (the shared commit, a pure move): EnemyLook is partial; its hit members (the Hit record, the flinch / tilt /
// knock, the parry's knock, the shed and its notch, React) are in EnemyViews.Hit.cs (builder R), its death members (Die,
// the Corpse dissolve) in EnemyViews.Death.cs (builder X); this file keeps the rest and the per-type views.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>
    /// NIGHT12 U3-25.3: what every enemy view shares, made once per game (For): the SKYGRIND/Enemy body material (the rim:
    /// fx.enemyRimColor / Power / Strength; the death dissolve: dissolve.js's cells and band, aura.edgeGlow x the rim
    /// colour on its edge, over aura.materializeTime; a noise texture made here), the x-ray materials (aim.xray; elites x
    /// eliteBoost), drawn while settings.xray is on (input.bindings.xray flips and saves it in play, as main.js
    /// input:xray does); the victim's flinch and vibration per hit (UnityTuning, render only: Research polish B3); and the
    /// dying enemies, each a snapshot of its view's drawn parts dissolving on world time (pooled, Graphics.RenderMesh).
    /// Its per-frame work runs once, from whichever view renders first (Frame).
    /// NIGHT13 U3-26.4 (PerfSwitches, both on by default): `mpb`: Tint sets a part's property block only when its colour,
    /// flash or rim differs from what that renderer last got (the renderer keeps its block until the next set); `xray`:
    /// the x-ray material is on a body's renderer only while ENEMY X-RAY is on (off, the body draws its one material; the
    /// x-ray material with every pass disabled drew nothing, so the picture is the same).
    /// </summary>
    public sealed partial class EnemyLook
    {
        /// <summary>The most parts a view draws (the husk 2, the swarmer and the spitter 3).</summary>
        public const int MaxParts = 3;
        const int NoEnemy = int.MinValue;
        const float Idle = float.MaxValue / 4; // a timer that never started (past every window, still the oldest)

        static EnemyLook shared;

        readonly SurvivalGame game;
        /// <summary>NIGHT47 F (visor47's switch, read by the models' parts): the game's config.</summary>
        public GameConfig Cfg => game != null ? game.cfg : null;
        /// <summary>SKYGRIND/Enemy is there (a Player built without it falls back to the neon copy: no rim, x-ray or dissolve).</summary>
        public readonly bool ok;
        public readonly Material body;
        readonly Material xray, xrayElite;
        readonly float dissolveTime;
        Key xrayKey;
        int xrayKeyVersion = int.MinValue; // NIGHT13 review: X follows a rebind (game.bindings.Version)
        bool xrayOn;
        int settingsVersion = -1;
        int frame = -1;

        // NIGHT17 U3-30.5: the chunks, the sheds (one block each, below CrunchShedAt of the HP), their notches, the flash time.
        public readonly Chunks chunks;
        /// <summary>NIGHT17 U3-30.10: the arrivals' ground ring and core (Chunks.cs SpawnFlashes).</summary>
        public readonly SpawnFlashes spawnFlashes;
        /// <summary>NIGHT17: this frame's particle step inside a freeze (frame time x CrunchFreezeDrift; 0 outside one).</summary>
        public static float FreezeDriftDt { get; private set; }

        // NIGHT13 U3-26.4: property blocks on change (PerfSwitches mpb) and the x-ray material while X-RAY is on (xray).
        struct Tinted
        {
            public Color color;
            public float flash; // NIGHT17: a share
            public float rim;
            public float form; // NIGHT43 A1 models43: the form shade it was given
            public float near63; // NIGHT63 V C2 nearFade63: the near-eye dissolve it was given (1 whole)
        }

        // NIGHT63 V C2 nearFade63 (NearFade63.N63V.cs): the row, its numbers and the eye, read once a frame in Frame(); every live part's
        // dissolve set from them in Tint
        bool near63On;
        NearFade63.Params near63P;
        GameConfig near63For;
        Vector3 near63Eye;

        /// <summary>NIGHT43 A1 models43: SKYGRIND/Enemy's _Form for this frame's tints (Models43's on: UnityTuning.Models43Form; off: 0, the web's flat colour).</summary>
        public static float Form { get; private set; }
        static readonly int FormId = Shader.PropertyToID("_Form");

        sealed class SameRenderer : IEqualityComparer<Renderer>
        {
            public bool Equals(Renderer a, Renderer b) => ReferenceEquals(a, b);
            public int GetHashCode(Renderer r) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(r);
        }

        struct XrayPart
        {
            public Renderer r;
            public bool elite;
        }

        readonly bool tintOnChange, xrayWhileOn;
        readonly Dictionary<Renderer, Tinted> tinted = new Dictionary<Renderer, Tinted>(new SameRenderer());
        readonly List<XrayPart> xrayParts = new List<XrayPart>();
        Material[] bodyOnly, bodyXray, bodyXrayElite; // made once: assigning sharedMaterials copies the array

        public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int Flash = Shader.PropertyToID("_Flash"), RimOn = Shader.PropertyToID("_RimOn"), Dissolve = Shader.PropertyToID("_Dissolve"),
            RimColor = Shader.PropertyToID("_RimColor"), RimPower = Shader.PropertyToID("_RimPower"), RimBoost = Shader.PropertyToID("_RimBoost"),
            EdgeColor = Shader.PropertyToID("_EdgeColor"), DissolveCells = Shader.PropertyToID("_DissolveCells"), DissolveBand = Shader.PropertyToID("_DissolveBand"),
            NoiseTex = Shader.PropertyToID("_NoiseTex"), NoiseSize = Shader.PropertyToID("_NoiseSize"), XrayColor = Shader.PropertyToID("_XrayColor"),
            XrayAlpha = Shader.PropertyToID("_XrayAlpha"), XrayEdge = Shader.PropertyToID("_XrayEdge"), XrayFade = Shader.PropertyToID("_XrayFade"),
            XrayBoost = Shader.PropertyToID("_XrayBoost");

        // The passes, by their LightMode tags (Enemy.shader names each pass the same).
        const string ForwardPass = "UniversalForwardOnly", DepthPass = "DepthOnly", NormalsPass = "DepthNormalsOnly", XrayPass = "SRPDefaultUnlit";
        const string InkPass = "UniversalForward"; // NIGHT51 ink51: the outline (Enemy.shader; its width is Night51's global)

        /// <summary>The game's shared look (made on first use; a new game makes a new one).</summary>
        public static EnemyLook For(SurvivalGame game)
        {
            if (shared == null || shared.game != game) shared = new EnemyLook(game);
            return shared;
        }

        EnemyLook(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            ClearHits(); // NIGHT18: the hit timers and the sheds (EnemyViews.Hit.cs)
            for (int i = 0; i < sheds.Length; i++) sheds[i].id = NoEnemy;
            shedColor = Look.C(cfg, "colors.enemy");
            chunks = new Chunks(game);
            spawnFlashes = new SpawnFlashes(game); // NIGHT17 U3-30.10: an enemy's arrival
            game.events.On<HitEvent>("hit", OnHit);
            game.events.On<HitEvent>("parry", OnParry); // NIGHT17 round 1: the attacker's knock-back
            // U3-30.10 the COMET: its blast's point (the bus's rocketBlast, after its kills in the same step) aims their blocks
            game.events.On<BlastEvent>("rocketBlast", e =>
            {
                if (e.owner == "player") chunks.ApplyBlast(WebSpace.ToUnity(e.position));
            });
            game.OnNewRun += Clear;
            Fusion43Init(); // NIGHT43 A1: the FUSION victim's shake (EnemyViews.Fusion43.cs)
            Nav43Init(); // NIGHT43 A1 round 2: the stuck tell and the relocation's hop (EnemyViews.Nav43.cs)
            Burn43Init(); // NIGHT43 A1 round 2: W1's torch43 burn drawn (EnemyViews.Burn43.cs)
            Zone47Init(); // NIGHT47 F R4 hitzone47: the hit read by where it lands (EnemyViews.Zone47.cs)
            Skin52.Init(cfg); // NIGHT52 U skin52 (Skin52.cs): the row read, the shader's globals (off: _Skin52.x 0, NIGHT51's surface)
            tintOnChange = PerfSwitches.On(PerfFix.Mpb);
            xrayWhileOn = PerfSwitches.On(PerfFix.Xray);
            Shader shader = Shader.Find("SKYGRIND/Enemy");
            ok = shader != null;
            if (!ok)
            {
                Debug.LogWarning("[enemies] SKYGRIND/Enemy is not in the build: enemies draw with the neon copy (no rim, x-ray or dissolve)");
                body = new Material(game.mats.neon);
                return;
            }
            Color rim = Look.C(cfg, "fx.enemyRimColor").linear; // three.js new Color(hex): linear
            body = new Material(shader) { name = "Enemy" };
            body.SetVector(RimColor, rim * (float)cfg.Num("fx.enemyRimStrength") * UnityTuning.CrunchRimBoost); // NIGHT17 round 1: x the rim boost
            body.SetFloat(RimPower, (float)cfg.Num("fx.enemyRimPower"));
            body.SetVector(RimBoost, new Vector4(UnityTuning.EnemyRimBoostNear, UnityTuning.EnemyRimBoostFar, UnityTuning.EnemyRimBoostMax, 0));
            body.SetVector(EdgeColor, rim * (float)cfg.Num("aura.edgeGlow"));
            body.SetFloat(DissolveCells, UnityTuning.EnemyDissolveCells);
            body.SetFloat(DissolveBand, UnityTuning.EnemyDissolveBand);
            body.SetTexture(NoiseTex, MakeNoise());
            body.SetFloat(NoiseSize, UnityTuning.EnemyNoiseSize);
            body.SetShaderPassEnabled(XrayPass, false); // the x-ray is the x-ray materials' pass
            xray = MakeXray(cfg, 1);
            xrayElite = MakeXray(cfg, (float)cfg.Num("aim.xray.eliteBoost"));
            bodyOnly = new[] { body };
            bodyXray = new[] { body, xray };
            bodyXrayElite = new[] { body, xrayElite };
            dissolveTime = (float)cfg.Num("aura.materializeTime");
            xrayKey = SurvivalGame.KeyOf(cfg.Str("input.bindings.xray"));
            corpses = new Corpse[UnityTuning.EnemyCorpseMax];
            for (int k = 0; k < corpses.Length; k++)
            {
                var c = new Corpse();
                for (int i = 0; i < MaxParts; i++) c.blocks[i] = new MaterialPropertyBlock();
                corpses[k] = c;
            }
            corpseParams = new RenderParams(body) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            notchParams = new RenderParams(body) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false, matProps = notchBlock };
            chunks.UseEnemyMaterial(body); // NIGHT17 round 1: the blocks draw as the corpse did (RenderMesh on SKYGRIND/Enemy)
        }

        /// <summary>xray.js material(): aim.xray's colour, alpha, edge and fade, x `boost`; only the x-ray pass (off until Frame turns it on).</summary>
        Material MakeXray(GameConfig cfg, float boost)
        {
            var m = new Material(body) { name = "EnemyXray", renderQueue = (int)RenderQueue.Transparent + UnityTuning.EnemyXrayQueueOffset };
            m.SetVector(XrayColor, Look.C(cfg, "aim.xray.color").linear);
            m.SetFloat(XrayAlpha, (float)cfg.Num("aim.xray.alpha"));
            m.SetFloat(XrayEdge, (float)cfg.Num("aim.xray.edge"));
            m.SetVector(XrayFade, new Vector4((float)cfg.Num("aim.xray.fadeStart"), (float)cfg.Num("aim.xray.fadeEnd"), 0, 0));
            m.SetFloat(XrayBoost, boost);
            m.SetShaderPassEnabled(ForwardPass, false);
            m.SetShaderPassEnabled(DepthPass, false);
            m.SetShaderPassEnabled(NormalsPass, false);
            m.SetShaderPassEnabled(XrayPass, false);
            m.SetShaderPassEnabled(InkPass, false); // NIGHT51 ink51: the body draws the outline, never the x-ray
            return m;
        }

        /// <summary>The dissolve's cells: one Mulberry32 value per texel (point filtered, repeating), made once at load.</summary>
        static Texture2D MakeNoise()
        {
            int n = UnityTuning.EnemyNoiseSize;
            var tex = new Texture2D(n, n, TextureFormat.R8, false, true) { name = "EnemyDissolveNoise", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            var rng = new Mulberry32(UnityTuning.EnemyNoiseSeed);
            var bytes = new byte[n * n];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = (byte)(rng.Next() * 256);
            tex.SetPixelData(bytes, 0);
            tex.Apply(false, true);
            return tex;
        }

        /// <summary>A view part's materials: the body, plus the x-ray (elites brighter) on the part xray.js draws (each kind's first).</summary>
        public void Attach(Renderer r, bool xrayPart, bool elite)
        {
            if (ok && xrayPart)
            {
                if (!xrayWhileOn)
                {
                    r.sharedMaterials = new[] { body, elite ? xrayElite : xray };
                    return;
                }
                // NIGHT13: the x-ray material only while X-RAY is on (Frame swaps it as the setting changes).
                xrayParts.Add(new XrayPart { r = r, elite = elite });
                r.sharedMaterials = XrayMaterials(elite);
            }
            else r.sharedMaterial = body;
        }

        /// <summary>A body's materials now: with its x-ray material while X-RAY is on, else the body alone.</summary>
        Material[] XrayMaterials(bool elite) => !xrayOn ? bodyOnly : elite ? bodyXrayElite : bodyXray;

        /// <summary>
        /// Tint a drawn part: its colour, the hit flash (white while the enemy's flash time runs) and the rim (0 = none).
        /// The neon fallback has no _Flash: it flashes by colour, as the views did before.
        /// </summary>
        public void Tint(MaterialPropertyBlock block, Renderer r, Color c, float flash, float rim)
        {
            if (TripoModels.FlashActive) TripoModels.EnemyTint(r, flash); // PG3 tripo74: the hit flash on the Tripo model (TripoModels.cs); one bool when none is attached
            if (Skin52.Built) Skin52.Bake(r); // NIGHT52 U skin52: a part's mesh swapped once for its smoothed-normal copy (the ink's gaps; Skin52.cs)
            c.a = 1;
            float near63 = near63On ? NearFade63.Dissolve(near63P, r.bounds, near63Eye) : 1f; // NIGHT63 V C2 nearFade63: the part dissolves as the eye nears it (1 whole: off, or far)
            if (tintOnChange)
            {
                // NIGHT13: the renderer keeps the block it was given last; set it again only when a value differs (exactly).
                float f = Form;
                if (tinted.TryGetValue(r, out Tinted was) && was.color.Equals(c) && was.flash.Equals(flash) && was.rim.Equals(rim) && was.form.Equals(f) && was.near63.Equals(near63)) return;
                tinted[r] = new Tinted { color = c, flash = flash, rim = rim, form = f, near63 = near63 };
            }
            Color v = c * UnityTuning.CrunchEnemyValue; // NIGHT17 round 1: the brightest warm value in the frame (no new hue)
            v.a = 1;
            block.SetColor(BaseColor, ok ? v : Color.Lerp(v, Color.white, flash));
            block.SetFloat(Flash, flash); // NIGHT17: a share (the eased hit flash), _Flash lerps toward white
            block.SetFloat(RimOn, rim);
            block.SetFloat(FormId, Form); // NIGHT43 A1 models43: the form shade (0 with the row off: the shader's colour is unchanged)
            // NIGHT63 V C2 nearFade63: set every time (the block is shared across a view's parts): 1 = whole, exactly the material's default (the
            // shader returns before the clip at 1). C2b: the screen-door's own _NearFade (Enemy.shader's NearClip), never the death's _Dissolve or
            // its edge band (the kill's; the critic read C2's dissolve as a shatter)
            block.SetFloat(NearFade63.NearFadeId, near63);
            r.SetPropertyBlock(block);
        }

        /// <summary>
        /// Once a frame, from the first view that renders: the X key and the x-ray setting, the hit timers (the flinch on
        /// world time, the vibration on frame time while the run simulates), then the dissolving enemies' draws.
        /// </summary>
        public void Frame()
        {
            if (frame == Time.frameCount) return;
            frame = Time.frameCount;
            Form = Models43.On(game.cfg) ? UnityTuning.Models43Form : 0; // NIGHT43 A1 models43: this frame's form shade (every view tints after this)
            Spring72Frame(); // NIGHT72 B4 spring72 (EnemyViews.Spring72.cs): the live bodies by id, the row read (the springs' layer is added in React)
            // NIGHT63 V C2 nearFade63: the row and the eye, once a frame (D1's contract: read every frame, never cached across frames)
            near63On = game.cam != null && NearFade63.On(game.cfg);
            if (near63On)
            {
                if (!ReferenceEquals(near63For, game.cfg)) { near63P = NearFade63.Params.From(game.cfg); near63For = game.cfg; }
                near63Eye = game.cam.transform.position;
                Shader.SetGlobalVector(NearFade63.GlobalId, NearFade63.Globals(near63P)); // C2b: the screen-door's cell, rim and ink (Enemy.shader)
            }
            bool simulating = game.flow == Flow.Playing || game.flow == Flow.Down;
            float vibDt = simulating ? (float)game.FrameDt : 0, worldDt = (float)game.WorldDt;
            FrameHits(vibDt, worldDt); // NIGHT18: the hit timers (EnemyViews.Hit.cs)
            // NIGHT17 U3-30.5: the screen's axes (the vibration), the particles' drift inside a freeze, the sheds, the chunks.
            if (game.cam != null)
            {
                Transform ct = game.cam.transform;
                screenRight = ct.right;
                screenUp = ct.up;
            }
            // NIGHT17 round 2: the kill path's pipeline states made on the first frame with a camera (TITLE), not on the first kill
            if (!warmed && game.cam != null)
            {
                warmed = true;
                chunks.Warm(game.cam);
                spawnFlashes.Warm(game.cam);
                WarmNotch(game.cam);
            }
            ImpactFrames director = game.ImpactDirector;
            bool frozen = simulating && director != null && director.FreezeLeft > 0;
            FreezeDriftDt = frozen ? vibDt * UnityTuning.CrunchFreezeDrift : 0;
            if (game.flow == Flow.Title)
            {
                chunks.Clear();
                spawnFlashes.Clear();
                shedAskCount = 0;
            }
            for (int i = 0; i < shedAskCount; i++)
            {
                ShedAsk a = shedAsks[i];
                chunks.Shed(a.at, a.dir, shedColor, a.feetY);
                shedPayload.enemy = a.id;
                game.events.Emit(ShedEvent, shedPayload); // port-only: the block flies on this frame (GG's sound hook)
            }
            shedAskCount = 0;
            chunks.Render(simulating);
            spawnFlashes.Render(simulating); // NIGHT17 U3-30.10
            if (game.flow == Flow.Title) Scorch51Clear(); // NIGHT51 scorch51: no mark on TITLE
            Scorch51Frame(simulating ? Time.deltaTime : 0f); // NIGHT51 scorch51: the kills' floor marks
            Burn43Frame(); // NIGHT43 A1 round 2: burning enemies' flame tongues (EnemyViews.Burn43.cs; nothing until Burning43 is wired)
            if (!ok) return;
            if (notchMesh == null) notchMesh = chunks.Cube;
            // main.js input:xray: the bound key flips settings.xray and saves it (the web's hint text is not ported).
            if (game.bindings != null && game.bindings.Version != xrayKeyVersion)
            {
                xrayKeyVersion = game.bindings.Version;
                xrayKey = KeyCodes.Bound(game.bindings, "xray");
            }
            if (xrayKey != Key.None && game.flow == Flow.Playing && game.input.Locked)
            {
                Keyboard kb = Keyboard.current;
                if (kb != null && kb[xrayKey].wasPressedThisFrame) game.settings.Set("xray", !game.settings.Bool("xray"));
            }
            if (settingsVersion != game.settings.version)
            {
                settingsVersion = game.settings.version;
                bool on = game.settings.Bool("xray");
                if (on != xrayOn)
                {
                    xrayOn = on;
                    xray.SetShaderPassEnabled(XrayPass, on);
                    xrayElite.SetShaderPassEnabled(XrayPass, on);
                    for (int i = 0; i < xrayParts.Count; i++) xrayParts[i].r.sharedMaterials = XrayMaterials(xrayParts[i].elite); // NIGHT13 (xray)
                }
            }
            if (game.flow == Flow.Title)
            {
                ClearCorpses(); // resetWorld: no enemy is left on TITLE (its world time is 0)
                return;
            }
            for (int k = 0; k < corpses.Length; k++)
            {
                Corpse c = corpses[k];
                if (!c.live) continue;
                c.age += worldDt;
                if (c.age >= dissolveTime)
                {
                    c.live = false;
                    continue;
                }
                c.flash -= worldDt;
                c.vibAge += vibDt;
                Vector3 off = c.dir * CrunchMath.Vib(c.vibAge, UnityTuning.EnemyVibTime, UnityTuning.EnemyVibAmp, UnityTuning.EnemyVibHz);
                float whole = 1 - c.age / dissolveTime;
                for (int i = 0; i < c.parts; i++)
                {
                    MaterialPropertyBlock b = c.blocks[i];
                    b.SetColor(BaseColor, c.colors[i]);
                    b.SetFloat(Flash, c.flash > 0 ? 1 : 0);
                    b.SetFloat(RimOn, c.rims[i]);
                    b.SetFloat(Dissolve, whole);
                    Matrix4x4 m = c.matrices[i];
                    m.m03 += off.x;
                    m.m13 += off.y;
                    m.m23 += off.z;
                    corpseParams.matProps = b;
                    corpseParams.worldBounds = new Bounds(new Vector3(m.m03, m.m13, m.m23), Vector3.one * UnityTuning.EnemyCorpseBounds);
                    Graphics.RenderMesh(corpseParams, c.meshes[i], 0, m);
                }
            }
        }

        /// <summary>A new run (anything per run resets at run start): no hit timers, no dissolves.</summary>
        void Clear()
        {
            for (int i = 0; i < hits.Length; i++) hits[i] = new Hit { id = NoEnemy, flinchAge = Idle, vibAge = Idle, flashAge = Idle };
            ClearCorpses();
            chunks.Clear(); // NIGHT17: no block, shed or notch carried into a new run
            spawnFlashes.Clear();
            Scorch51Clear(); // NIGHT51
        }

    }

    public sealed class HuskView
    {
        /// <summary>NIGHT47 F fx47 (G20; F-cards 1b: "flung enemies tumble"): while a launch's knockback runs (launch36 / pulse47's flight), the body
        /// turns about its own middle round its sideways axis at UnityTuning.Fx47TumbleDegS (view only: the sim's body never turns).</summary>
        void Tumble47(Transform root, Husk husk)
        {
            float flightS = (float)(cfg.Has("design.n36.launch36.flightS") ? cfg.Num("design.n36.launch36.flightS") : 0.8);
            int flightSteps = Mathf.Max(1, Mathf.RoundToInt(flightS * 120f));
            float t = Mathf.Clamp(flightSteps - husk.knockSteps, 0, flightSteps) / 120f;
            float h = (float)husk.Height * 0.5f;
            Vector3 mid = root.position + Vector3.up * h;
            Quaternion spin = Quaternion.AngleAxis(t * UnityTuning.Fx47TumbleDegS, root.right);
            root.rotation = spin * root.rotation;
            root.position = mid - spin * (Vector3.up * h);
        }

        sealed class Body
        {
            public Transform root;
            public readonly Renderer[] rends = new Renderer[2]; // the body, the head
            public readonly Color[] colors = new Color[2]; // as last drawn (the dissolve keeps them)
            public Enemy40Parts m40; // NIGHT40 E3 models40: the silhouette parts (null: the row off)
            public Look43 l43; // NIGHT43 A1 models43: the rebuilt model (Enemy43View.cs)
            public Enemy drawn; // the husk drawn here last frame
        }

        static readonly float[] Rims = { 1, 1 }; // rimGlow.js on the bodies and the heads (husk.js)
        readonly GameConfig cfg;
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly bool models40; // NIGHT40 E3: design.n40.models40.enabled when the view was made
        readonly Color danger40, parry40; // NIGHT40 E3: the parts' red-orange and yellow
        readonly List<Body> pool = new List<Body>();
        int drawnCount;
        Mesh[] meshes; // the capsule and the cube, for the dissolve
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Transform parent;
        readonly Color enemy, tell;
        readonly Color tankColor; // NIGHT29 design.n29.tank: the level-2 tank is this body in its own colour and size (Enemy.tank29)
        readonly float tankScale = 1;
        readonly float headShade, pulseScale, pulseRate, staggerPale, leanDeg, leanEase, headTilt;
        readonly Vector3 headBox;
        readonly float height, radius, headHeight, headForward;

        public HuskView(SurvivalGame game)
        {
            this.game = game;
            cfg = game.cfg;
            look = EnemyLook.For(game); // SKYGRIND/Enemy: unlit, as the web's MeshBasicMaterial husks (their colour reads at any light)
            models40 = Enemy40Parts.On(game.cfg);
            danger40 = Look.Hex(UnityTuning.Tell36RedHex);
            parry40 = Look.Hex(UnityTuning.Tell36YellowHex);
            parent = new GameObject("Husks").transform;
            enemy = Look.C(cfg, "colors.enemy");
            tell = Look.C(cfg, "husk.windupColor");
            tankColor = cfg.Has("design.n29.tank.color") ? Look.C(cfg, "design.n29.tank.color") : enemy;
            if (cfg.Has("design.n29.tank.scale")) tankScale = (float)cfg.Num("design.n29.tank.scale");
            headShade = (float)cfg.Num("husk.headShade");
            pulseScale = (float)cfg.Num("husk.windupPulseScale");
            pulseRate = (float)cfg.Num("husk.windupPulseRate");
            staggerPale = (float)cfg.Num("husk.staggerPale");
            leanDeg = (float)cfg.Num("husk.staggerLeanDeg");
            leanEase = (float)cfg.Num("husk.staggerLeanEaseRate");
            headTilt = (float)cfg.Num("husk.headTiltDeg");
            double[] hb = cfg.Nums("husk.headBox");
            headBox = new Vector3((float)hb[0], (float)hb[1], (float)hb[2]);
            height = (float)cfg.Num("husk.height");
            radius = (float)cfg.Num("husk.radius");
            headHeight = (float)cfg.Num("husk.headHeight");
            headForward = (float)cfg.Num("husk.headForward");
            game.events.On<KillEvent>("kill", OnKill);
        }

        /// <summary>A husk drawn last frame died: its pose dissolves (EnemyLook.Die).</summary>
        void OnKill(KillEvent e)
        {
            for (int i = 0; i < drawnCount; i++)
            {
                Body b = pool[i];
                if (b.drawn == null || b.drawn.id != e.enemy) continue;
                look.Die(b.drawn, e, b.rends, meshes, b.colors, Rims, 2, 1, b.l43 != null ? b.l43.DyingParts(b.m40) : b.m40); // NIGHT41 E6 models41: + the parts break (NIGHT43 A1: models43's while it draws). NIGHT17: the head box pops on a headshot
                return;
            }
        }

        Body Make()
        {
            var b = new Body();
            b.root = new GameObject("husk").transform;
            b.root.SetParent(parent, false);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(b.root, false);
            body.transform.localScale = new Vector3(radius * 2, height / 2, radius * 2); // the primitive is 2 m tall, 1 m wide
            body.transform.localPosition = new Vector3(0, height / 2, 0);
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(head.GetComponent<Collider>());
            head.transform.SetParent(b.root, false);
            head.transform.localScale = headBox;
            head.transform.localPosition = new Vector3(0, headHeight, headForward); // web -z forward = Unity +z
            head.transform.localRotation = Quaternion.Euler(headTilt, 0, 0);
            b.rends[0] = body.GetComponent<Renderer>();
            b.rends[1] = head.GetComponent<Renderer>();
            look.Attach(b.rends[0], true, false); // husk.js xrayMesh: the bodies
            look.Attach(b.rends[1], false, false);
            meshes ??= new[] { body.GetComponent<MeshFilter>().sharedMesh, head.GetComponent<MeshFilter>().sharedMesh };
            if (models40) b.m40 = Enemy40Parts.Make("husk", b.root, look, height, N41View.RowOn(cfg, "models41")); // NIGHT40 E3 models40; NIGHT41 E6 models41: the husk's own feature
            b.l43 = new Look43("husk", b.root, b.rends, UnityTuning.Models43HuskBase, null, look, height); // NIGHT43 A1 models43 (swaps on its first ON frame)
            return b;
        }

        public void Render(double alpha, double frameDt)
        {
            look.Frame();
            int n = 0;
            foreach (Enemy e in game.enemies.all)
            {
                if (!(e is Husk husk) || husk.kind != "husk" || !husk.alive) continue;
                if (n >= pool.Count) pool.Add(Make());
                Body b = pool[n++];
                b.drawn = husk;
                b.root.gameObject.SetActive(true);
                husk.flash = System.Math.Max(0, husk.flash - frameDt);
                bool windingUp = husk.windupSteps > 0;
                if (windingUp) husk.windupAge += frameDt;
                float wave = windingUp ? 0.5f + 0.5f * Mathf.Sin((float)husk.windupAge * pulseRate) : 0;
                float pulse = 1 + (pulseScale - 1) * wave;
                Color c = husk.tank29 ? tankColor : enemy;
                if (windingUp) c = Color.Lerp(c, tell, 0.5f + 0.5f * wave);
                bool staggered = husk.staggerSteps > 0;
                if (staggered) c = Color.Lerp(c, Color.white, staggerPale);
                b.l43.tripoAttack = windingUp; // PG3: the Tripo model rears back and lunges (Enemy43View.Tripo.cs)
                b.l43.Apply(Models43.On(cfg), b.m40); // NIGHT43 A1 models43
                if (b.l43.on) c = Models43.Body(c);
                Color hc = c * headShade; // a flash whitens both (the shader's _Flash: husk.js WHITE)
                hc.a = 1;
                float leanTarget = staggered ? leanDeg * Mathf.Deg2Rad : 0;
                husk.lean += (leanTarget - husk.lean) * (1 - System.Math.Exp(-leanEase * frameDt));
                Vec3 p = husk.previous + (husk.position - husk.previous) * alpha;
                // Web yaw (0 faces -z, + turns toward -x) -> Unity (0 faces +z): the same turn, negated; the lean tips it back.
                // NIGHT54 M move54: the yaw at alpha (husk.yaw with the row off)
                Quaternion rot = Quaternion.Euler(0, -(float)(game.Move54EnemyYaw(husk, alpha) * Mathf.Rad2Deg), 0) * Quaternion.Euler(-(float)(husk.lean * Mathf.Rad2Deg), 0, 0);
                float flash = look.React(husk.id, rot, husk.flash > 0, out Vector3 off, out Vector3 squash, out Quaternion tilt); // NIGHT17
                b.root.position = WebSpace.ToUnity(p) + off;
                b.root.rotation = tilt * rot;
                if (husk.Airborne && husk.knockSteps > 0 && Fx47View.RowOn(cfg)) Tumble47(b.root, husk); // NIGHT47 F fx47 (G20): a launched body tumbles
                b.root.localScale = squash * (husk.tank29 ? pulse * tankScale : pulse);
                look.Notch(husk.id, b.root);
                b.colors[0] = c;
                b.colors[1] = hc;
                look.Tint(block, b.rends[0], c, flash, Rims[0]);
                look.Tint(block, b.rends[1], hc, flash, Rims[1]);
                FeelProbe.Enemy(husk.id, b.rends[0].bounds, b.rends[1].bounds, flash); // NIGHT18: the reel's victim rect (M5)
                if (b.m40 != null) { b.m40.ShowGroup(husk.tank29 ? 2 : 1); b.m40.Pose(look, block, c, flash, windingUp ? 0.5f + 0.5f * wave : 0, staggered, danger40, parry40, (float)husk.windupAge); } // NIGHT40 E3 models40
                if (b.l43.on) { b.l43.extras.ShowGroup(husk.tank29 ? 2 : 1); b.l43.extras.Pose(look, block, c, flash, windingUp ? 0.5f + 0.5f * wave : 0, staggered, danger40, parry40, (float)husk.windupAge); } // NIGHT43 A1 models43
            }
            drawnCount = n;
            for (int i = n; i < pool.Count; i++)
            {
                pool[i].root.gameObject.SetActive(false);
                pool[i].drawn = null;
            }
        }
    }

    public sealed partial class SwarmerView // NIGHT48 F: partial (foes48's gait: EnemyViews.Gait48.cs)
    {
        sealed class Body
        {
            public Transform root;
            public readonly Transform[] parts = new Transform[3];
            public readonly Renderer[] rends = new Renderer[3];
            public readonly Color[] colors = new Color[3];
            public Enemy40Parts m40; // NIGHT40 E3 models40: the silhouette parts (null: the row off)
            public Look43 l43; // NIGHT43 A1 models43: the rebuilt model (Enemy43View.cs)
            public Enemy drawn;
            public MeshFilter legsF47; // NIGHT47 F foes47: the legs' filter (read once)
            public Mesh legsWas47; // the mesh models43 gave the legs (put back with foes47 off)
            public float crouch47, landAge47 = -1f; // the crouch's eased share, s since the pounce landed (-1: none)
            public string state47;
        }

        /// <summary>NIGHT47 F foes47 (round 4; F-cards 5c "the pounce crouch springs (anticipation 0.1 s down, overshoot on landing)"): the squash
        /// share this frame (1 = NIGHT46's crouch squash; negative = a stretch): the crouch eases in over Swarmer47CrouchInS (the anticipation; it
        /// snapped before), the leap stretches the body Swarmer47LeapStretch (squash and stretch), the landing squashes it on a damped spring
        /// (Swarmer47LandSquash, its period Swarmer47LandPeriodS, gone in Swarmer47LandS). Frame time, render only.</summary>
        static float Spring47(Body b, string state, float dt)
        {
            if (b.state47 == "pounce" && state != "pounce") b.landAge47 = 0f;
            b.state47 = state;
            float target = state == "crouch" ? 1f : 0f;
            b.crouch47 = Mathf.MoveTowards(b.crouch47, target, dt / UnityTuning.Swarmer47CrouchInS);
            float k = b.crouch47 * b.crouch47 * (3f - 2f * b.crouch47); // smoothstep: a gather, not a snap
            if (state == "pounce") k -= UnityTuning.Swarmer47LeapStretch;
            if (b.landAge47 >= 0f)
            {
                float t = b.landAge47;
                b.landAge47 += dt;
                if (t > UnityTuning.Swarmer47LandS) b.landAge47 = -1f;
                else k += UnityTuning.Swarmer47LandSquash * Mathf.Exp(-t / (UnityTuning.Swarmer47LandS * 0.35f)) * Mathf.Cos(t * 2f * Mathf.PI / UnityTuning.Swarmer47LandPeriodS);
            }
            return k;
        }

        /// <summary>NIGHT47 F foes47 (round 4; F-cards 5c "four short pointed legs"): with models43 drawing, the legs' mesh is Mesh43.Legs47 with the
        /// row on and models43's own (the carapace skirt) with it off. Look43 sets its meshes only when models43 toggles, so this holds per frame.</summary>
        static void Legs47(Body b, bool on)
        {
            if (b.legsF47 == null) b.legsF47 = b.parts[2] != null ? b.parts[2].GetComponent<MeshFilter>() : null;
            MeshFilter f = b.legsF47;
            if (f == null) return;
            Mesh legs = Mesh43.Legs47();
            if (on)
            {
                if (f.sharedMesh != legs) { if (!Mesh43.IsLegs48(f.sharedMesh)) b.legsWas47 = f.sharedMesh; f.sharedMesh = legs; } // NIGHT48 F: never keep a gait pose as the put-back mesh
            }
            else if ((f.sharedMesh == legs || Mesh43.IsLegs48(f.sharedMesh)) && b.legsWas47 != null) f.sharedMesh = b.legsWas47;
        }

        static readonly float[] Rims = { 1, 1, 1 }; // enemyKind.js: every part rims (none says rim: false)
        const int HeadPart = 1; // NIGHT17: the head box (popped on a headshot kill)
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly bool models40; // NIGHT40 E3: design.n40.models40.enabled when the view was made
        readonly Color danger40, parry40; // NIGHT40 E3: the parts' red-orange and yellow
        readonly List<Body> pool = new List<Body>();
        int drawnCount;
        Mesh[] meshes;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Transform parent;
        readonly Color baseColor, tell;
        readonly float height;
        static readonly Vector3[] Sizes = { new Vector3(0.46f, 0.32f, 0.8f), new Vector3(0.3f, 0.26f, 0.3f), new Vector3(0.62f, 0.3f, 0.62f) };
        static readonly float[] Shades = { 1, 0.8f, 0.62f };
        static readonly Vector3 Squash = new Vector3(1.15f, 0.65f, 1.1f);

        public SwarmerView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            look = EnemyLook.For(game); // SKYGRIND/Enemy: unlit, as the web's MeshBasicMaterial parts
            models40 = Enemy40Parts.On(game.cfg);
            danger40 = Look.Hex(UnityTuning.Tell36RedHex);
            parry40 = Look.Hex(UnityTuning.Tell36YellowHex);
            parent = new GameObject("Swarmers").transform;
            baseColor = Look.C(cfg, "colors.enemy") * (float)cfg.Num("swarmer.shade");
            baseColor.a = 1;
            tell = Look.C(cfg, "husk.windupColor");
            height = (float)cfg.Num("swarmer.height");
            game.events.On<KillEvent>("kill", OnKill);
        }

        void OnKill(KillEvent e)
        {
            for (int i = 0; i < drawnCount; i++)
            {
                Body b = pool[i];
                if (b.drawn == null || b.drawn.id != e.enemy) continue;
                look.Die(b.drawn, e, b.rends, meshes, b.colors, Rims, 3, HeadPart, b.l43 != null ? b.l43.DyingParts(b.m40) : b.m40); // NIGHT41 E6 models41: + the parts break (NIGHT43 A1: models43's while it draws)
                return;
            }
        }

        /// <summary>The part's NIGHT46 offset (Make's): the head 0.42 m in front.</summary>
        Vector3 SwarmerOffset(int i) => i == 0 ? new Vector3(0, height * 0.45f, 0) : i == 1 ? new Vector3(0, height * 0.62f, 0.42f) : new Vector3(0, height * 0.18f, 0);

        Body Make()
        {
            var b = new Body();
            b.root = new GameObject("swarmer").transform;
            b.root.SetParent(parent, false);
            // Web offsets (forward -z) -> Unity (+z): the head sits 0.42 m in front.
            var offsets = new[] { new Vector3(0, height * 0.45f, 0), new Vector3(0, height * 0.62f, 0.42f), new Vector3(0, height * 0.18f, 0) };
            for (int i = 0; i < 3; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(b.root, false);
                go.transform.localPosition = offsets[i];
                go.transform.localScale = Sizes[i];
                b.parts[i] = go.transform;
                b.rends[i] = go.GetComponent<Renderer>();
                look.Attach(b.rends[i], i == 0, false); // enemyKind.js xrayMesh: the first part (the body)
            }
            if (meshes == null)
            {
                Mesh cube = b.parts[0].GetComponent<MeshFilter>().sharedMesh;
                meshes = new[] { cube, cube, cube };
            }
            if (models40) b.m40 = Enemy40Parts.Make("swarmer", b.root, look, height); // NIGHT40 E3 models40
            b.l43 = new Look43("swarmer", b.root, b.rends, UnityTuning.Models43SwarmerBase, null, look, height); // NIGHT43 A1 models43
            return b;
        }

        public void Render(double alpha, double frameDt)
        {
            look.Frame();
            int n = 0;
            foreach (Enemy e in game.enemies.all)
            {
                if (!(e is Swarmer s) || !s.alive) continue;
                if (n >= pool.Count) pool.Add(Make());
                Body b = pool[n++];
                b.drawn = s;
                b.root.gameObject.SetActive(true);
                s.flash = System.Math.Max(0, s.flash - frameDt);
                s.drawAge += frameDt;
                bool crouch = s.state == "crouch";
                Color c = baseColor;
                if (crouch) c = Color.Lerp(c, tell, 0.5f + 0.5f * Mathf.Sin((float)s.drawAge * 40));
                b.l43.tripoAttack = s.windupSteps > 0; // PG3: the Tripo model rears back and lunges (Enemy43View.Tripo.cs)
                b.l43.Apply(Models43.On(game.cfg), b.m40); // NIGHT43 A1 models43
                if (b.l43.on) c = Models43.Body(c);
                Vec3 p = s.previous + (s.position - s.previous) * alpha;
                Quaternion rot = Quaternion.Euler(0, -(float)(game.Move54EnemyYaw(s, alpha) * Mathf.Rad2Deg), 0); // NIGHT54 M move54: the yaw at alpha (s.yaw with the row off)
                float flash = look.React(s.id, rot, s.flash > 0, out Vector3 off, out Vector3 squash, out Quaternion tilt); // NIGHT17
                b.root.position = WebSpace.ToUnity(p) + off;
                b.root.rotation = tilt * rot;
                b.root.localScale = squash;
                look.Notch(s.id, b.root);
                bool f47 = Foes47.On(game.cfg); // NIGHT47 F foes47 (G14; F-cards 5c): the small robot as a small pill on short legs, leaning into its pounce
                if (b.l43.on) Legs47(b, f47); // round 4: the legs as four pointed spikes (one mesh: Mesh43.Legs47)
                bool g48 = f47 && b.l43.on && Foes48.On(game.cfg);
                if (g48) Gait48(b, (float)frameDt); // NIGHT48 F foes48 (G14; EnemyViews.Gait48.cs): the legs step with the drawn motion
                float sq47 = f47 ? Spring47(b, s.state, (float)frameDt) : 0f; // round 4: the pounce's squash and stretch (F-cards 5c)
                for (int i = 0; i < 3; i++)
                {
                    Vector3 size = f47 ? UnityTuning.Swarmer47Sizes[i] : Sizes[i];
                    if (f47 && !g48 && i == 2) size.x *=1f + UnityTuning.Swarmer47ScuttleDepth * Mathf.Sin((float)s.drawAge * UnityTuning.Swarmer47ScuttleHz * 2f * Mathf.PI); // the legs' scuttle
                    b.parts[i].localScale = f47 ? Vector3.Scale(size, Vector3.LerpUnclamped(Vector3.one, Squash, sq47)) : crouch ? Vector3.Scale(size, Squash) : size;
                    b.parts[i].localPosition = f47 ? Vector3.Scale(UnityTuning.Swarmer47Offsets[i], new Vector3(1, height, 1)) : SwarmerOffset(i);
                    b.parts[i].localRotation = f47 && i == 0 ? Quaternion.Euler(UnityTuning.Swarmer47LeanDeg, 0, 0) : Quaternion.identity;
                    Color pc = c * (f47 ? 1f : Shades[i]); // foes47: the head and legs no darker than the body (they read as one black blob with three dots)
                    pc.a = 1;
                    b.colors[i] = pc;
                    look.Tint(block, b.rends[i], pc, flash, Rims[i]);
                }
                if (FeelProbe.Recording)
                {
                    // NIGHT60 V (B3 a): the reel's enemy rect, as the husk view reports it (its three parts' bounds; recording-only)
                    Bounds sb = b.rends[0].bounds;
                    sb.Encapsulate(b.rends[2].bounds);
                    FeelProbe.Enemy(s.id, sb, b.rends[1].bounds, flash);
                }
                b.m40?.Pose(look, block, c, flash, crouch ? 1 : 0, false, danger40, parry40, (float)s.drawAge); // NIGHT40 E3 models40
                if (b.l43.on) b.l43.extras.Pose(look, block, c, flash, crouch ? 1 : 0, false, danger40, parry40, (float)s.drawAge); // NIGHT43 A1 models43
            }
            drawnCount = n;
            for (int i = n; i < pool.Count; i++)
            {
                pool[i].root.gameObject.SetActive(false);
                pool[i].drawn = null;
            }
        }
    }

    /// <summary>
    /// NIGHT8 U2-10: the SPITTER as spitter.js draws it (GG's design, rebuilt as it is): a capsule body (spitter.radius,
    /// height) in colors.enemy x spitter.shade, leaned 0.25 rad about the world x axis after its yaw (composeAt's Euler XYZ);
    /// a 0.42 m acid sac at (0, 0.72 h, 0.32 back) that swells x1.6 over the tell and pulses toward 0xfff0a0 (x2.2); a
    /// 0.4 x 0.3 x 0.4 box at (0, 0.9 h, 0.3 forward) in colors.enemy x 0.7. A hit flashes it white. Its globs (0.28 m
    /// spheres, acid x 2) and puddles (discs of puddleRadius, growing over 0.2 s, shrinking over the last 0.5 s, acid x 1.3
    /// at 0.55 alpha) are pooled objects.
    /// </summary>
    public sealed class SpitterView
    {
        sealed class Body
        {
            public Transform root;
            public readonly Transform[] parts = new Transform[3];
            public readonly Renderer[] rends = new Renderer[3];
            public readonly Color[] colors = new Color[3];
            public Enemy40Parts m40; // NIGHT40 E3 models40: the silhouette parts (null: the row off)
            public Look43 l43; // NIGHT43 A1 models43: the rebuilt model (Enemy43View.cs)
            public Enemy drawn;
        }

        static readonly float[] Rims = { 1, 0, 1 }; // spitter.js: the sac has rim: false
        const int HeadPart = 2; // NIGHT17: the front box (popped on a headshot kill)
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly bool models40; // NIGHT40 E3: design.n40.models40.enabled when the view was made
        readonly Color danger40, parry40; // NIGHT40 E3: the parts' red-orange and yellow
        readonly Material acidMat;
        readonly List<Body> pool = new List<Body>();
        int drawnCount;
        Mesh[] meshes;
        readonly Transform[] globs, puddles;
        // NIGHT17 round 4 (builder X; the reviewer: the deflected glob cyan with a trail): a reflected glob (SpitterAcid's
        // Glob.reflected, the player's parry) draws in colors.usable x2 with a cyan trail (a TrailRenderer, emitting only then)
        readonly Renderer[] globRends;
        readonly TrailRenderer[] globTrails;
        readonly bool[] globReflected;
        Material globMat, globCyanMat;
        int trailWarm; // NIGHT17 round 5: 0 not yet, 1 drawn this frame (undo on the next), 2 done
        Vector3 globScale;
        readonly Renderer[] puddleRends;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Transform parent;
        readonly Color body, box, acid, hot;
        readonly float height, radius, puddleRadius, puddleTime;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public SpitterView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            look = EnemyLook.For(game);
            models40 = Enemy40Parts.On(game.cfg);
            danger40 = Look.Hex(UnityTuning.Tell36RedHex);
            parry40 = Look.Hex(UnityTuning.Tell36YellowHex);
            acidMat = game.mats.acid;
            parent = new GameObject("Spitters").transform;
            Color enemy = Look.C(cfg, "colors.enemy");
            body = enemy * (float)cfg.Num("spitter.shade");
            body.a = 1;
            box = enemy * 0.7f;
            box.a = 1;
            acid = Look.C(cfg, "spitter.acidColor");
            hot = Look.Hex(0xfff0a0);
            height = (float)cfg.Num("spitter.height");
            radius = (float)cfg.Num("spitter.radius");
            puddleRadius = (float)cfg.Num("spitter.puddleRadius");
            puddleTime = (float)cfg.Num("spitter.puddleTime");
            SpitterAcid a = game.husks.acid;
            globs = new Transform[a.globs.Length];
            globRends = new Renderer[a.globs.Length];
            globTrails = new TrailRenderer[a.globs.Length];
            globReflected = new bool[a.globs.Length];
            float gr = (float)cfg.Num("spitter.globRadius");
            globMat = new Material(game.mats.neon);
            globMat.SetColor(BaseColor, acid * 2);
            Color usable = Look.C(cfg, "colors.usable");
            globCyanMat = new Material(game.mats.neon) { name = "Glob reflected" };
            globCyanMat.SetColor(BaseColor, usable * 2);
            var trailMat = new Material(game.mats.neon) { name = "Glob trail", renderQueue = (int)RenderQueue.Transparent };
            trailMat.SetFloat("_SrcBlend", (float)BlendMode.One); // additive (WeaponFx.Additive's recipe)
            trailMat.SetFloat("_DstBlend", (float)BlendMode.One);
            trailMat.SetFloat("_ZWrite", 0);
            trailMat.SetFloat("_Cull", 0);
            trailMat.SetColor(BaseColor, usable * UnityTuning.GlobTrailHdr);
            for (int i = 0; i < globs.Length; i++)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(g.GetComponent<Collider>());
                g.transform.SetParent(parent, false);
                g.transform.localScale = Vector3.one * gr * 2;
                var r = g.GetComponent<Renderer>();
                r.sharedMaterial = globMat;
                r.shadowCastingMode = ShadowCastingMode.Off;
                var trail = g.AddComponent<TrailRenderer>();
                trail.sharedMaterial = trailMat;
                trail.time = UnityTuning.GlobTrailTime;
                trail.startWidth = gr * 2 * UnityTuning.GlobTrailWidth;
                trail.endWidth = 0;
                trail.shadowCastingMode = ShadowCastingMode.Off;
                trail.receiveShadows = false;
                trail.emitting = false;
                g.SetActive(false);
                globs[i] = g.transform;
                globRends[i] = r;
                globTrails[i] = trail;
            }
            puddles = new Transform[a.puddles.Length];
            puddleRends = new Renderer[a.puddles.Length];
            Color pc = acid * 1.3f;
            pc.a = 0.55f;
            for (int i = 0; i < puddles.Length; i++)
            {
                var d = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // 2 m tall, radius 0.5: scaled to a disc below
                Object.Destroy(d.GetComponent<Collider>());
                d.transform.SetParent(parent, false);
                var r = d.GetComponent<Renderer>();
                r.sharedMaterial = acidMat;
                r.shadowCastingMode = ShadowCastingMode.Off;
                block.SetColor(BaseColor, pc);
                r.SetPropertyBlock(block);
                d.SetActive(false);
                puddles[i] = d.transform;
                puddleRends[i] = r;
            }
            block.Clear(); // the puddles' colour stays theirs; the bodies' block starts empty
            game.events.On<KillEvent>("kill", OnKill);
        }

        void OnKill(KillEvent e)
        {
            for (int i = 0; i < drawnCount; i++)
            {
                Body b = pool[i];
                if (b.drawn == null || b.drawn.id != e.enemy) continue;
                look.Die(b.drawn, e, b.rends, meshes, b.colors, Rims, 3, HeadPart, b.l43 != null ? b.l43.DyingParts(b.m40) : b.m40); // NIGHT41 E6 models41: + the parts break (NIGHT43 A1: models43's while it draws)
                return;
            }
        }

        Body Make()
        {
            var b = new Body();
            b.root = new GameObject("spitter").transform;
            b.root.SetParent(parent, false);
            var shapes = new[] { PrimitiveType.Capsule, PrimitiveType.Sphere, PrimitiveType.Cube };
            var offsets = new[] { new Vector3(0, height * 0.5f, 0), new Vector3(0, height * 0.72f, -0.32f), new Vector3(0, height * 0.9f, 0.3f) }; // web z -> Unity -z
            var scales = new[] { new Vector3(radius * 2, height / 2, radius * 2), Vector3.one * 0.84f, new Vector3(0.4f, 0.3f, 0.4f) };
            for (int i = 0; i < 3; i++)
            {
                var go = GameObject.CreatePrimitive(shapes[i]);
                Object.Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(b.root, false);
                go.transform.localPosition = offsets[i];
                go.transform.localScale = scales[i];
                b.parts[i] = go.transform;
                b.rends[i] = go.GetComponent<Renderer>();
                look.Attach(b.rends[i], i == 0, true); // enemyKind.js xrayMesh: the body; main.js: the spitter is an elite
            }
            if (meshes == null)
            {
                meshes = new Mesh[3];
                for (int i = 0; i < 3; i++) meshes[i] = b.parts[i].GetComponent<MeshFilter>().sharedMesh;
            }
            if (models40) b.m40 = Enemy40Parts.Make("spitter", b.root, look, height); // NIGHT40 E3 models40
            b.l43 = new Look43("spitter", b.root, b.rends, UnityTuning.Models43SpitterBase, null, look, height); // NIGHT43 A1 models43
            return b;
        }

        public void Render(double alpha, double frameDt)
        {
            look.Frame();
            int n = 0;
            foreach (Enemy e in game.enemies.all)
            {
                if (!(e is Spitter s) || !s.alive) continue;
                if (n >= pool.Count) pool.Add(Make());
                Body b = pool[n++];
                b.drawn = s;
                b.root.gameObject.SetActive(true);
                s.flash = System.Math.Max(0, s.flash - frameDt);
                s.drawAge += frameDt;
                bool tell = s.state == "tell";
                Vec3 p = s.previous + (s.position - s.previous) * alpha;
                // The body's lean is about the world x axis after the yaw (the web's Euler XYZ), mirrored into Unity's z.
                Quaternion yawQ = Quaternion.Euler(0, -(float)(game.Move54EnemyYaw(s, alpha) * Mathf.Rad2Deg), 0); // NIGHT54 M move54: the yaw at alpha (s.yaw with the row off)
                float flash = look.React(s.id, yawQ, s.flash > 0, out Vector3 off, out Vector3 squash, out Quaternion tilt); // NIGHT17
                b.root.position = WebSpace.ToUnity(p) + off;
                b.root.rotation = tilt * yawQ;
                b.root.localScale = squash;
                Quaternion lean = Quaternion.Euler(-0.25f * Mathf.Rad2Deg, 0, 0);
                b.parts[0].rotation = tilt * lean * yawQ;
                b.parts[0].position = b.root.position + tilt * lean * new Vector3(0, height * 0.5f, 0); // the offset leans too (composeAt); NIGHT17: and tips with the hit
                float swell = tell ? 1 + 0.6f * Mathf.Min(1, 1 - s.stateSteps / (float)System.Math.Max(1, s.tellSteps)) : 1;
                b.parts[1].localScale = Vector3.one * 0.84f * swell;
                Color sac = acid;
                if (tell) sac = Color.Lerp(sac, hot, 0.5f + 0.5f * Mathf.Sin((float)s.drawAge * 30)) * 2.2f;
                b.l43.tripoAttack = s.windupSteps > 0 || s.tellSteps > 0; // PG3: the Tripo model rears back and lunges (Enemy43View.Tripo.cs)
                b.l43.Apply(Models43.On(game.cfg), b.m40); // NIGHT43 A1 models43: the body and the head darkened, the sac keeps its acid
                Color body43 = b.l43.on ? Models43.Body(body) : body, box43 = b.l43.on ? Models43.Body(box) : box;
                b.colors[0] = body43;
                b.colors[1] = sac;
                b.colors[2] = box43;
                for (int i = 0; i < 3; i++) look.Tint(block, b.rends[i], b.colors[i], flash, Rims[i]);
                if (FeelProbe.Recording)
                {
                    // NIGHT60 V (B3 a): the reel's enemy rect, as the husk view reports it (the body + the sac, which swells and glows in the tell, with the box; recording-only)
                    Bounds pb = b.rends[0].bounds;
                    pb.Encapsulate(b.rends[1].bounds);
                    FeelProbe.Enemy(s.id, pb, b.rends[2].bounds, flash);
                }
                b.m40?.Pose(look, block, body, flash, tell ? (swell - 1) / 0.6f : 0, false, danger40, parry40, (float)s.drawAge); // NIGHT40 E3 models40
                if (b.l43.on) b.l43.extras.Pose(look, block, body43, flash, tell ? (swell - 1) / 0.6f : 0, false, danger40, parry40, (float)s.drawAge); // NIGHT43 A1 models43
                look.Notch(s.id, b.root);
            }
            drawnCount = n;
            for (int i = n; i < pool.Count; i++)
            {
                pool[i].root.gameObject.SetActive(false);
                pool[i].drawn = null;
            }

            SpitterAcid a = game.husks.acid;
            if (trailWarm == 1 && globs.Length > 0)
            {
                // NIGHT17 round 5: the warm-up frame is over: glob 0 back as it was (hidden, acid, no trail)
                trailWarm = 2;
                globTrails[0].emitting = false;
                globTrails[0].widthMultiplier = 1;
                globTrails[0].Clear();
                globRends[0].sharedMaterial = globMat;
                globs[0].localScale = globScale;
                globs[0].gameObject.SetActive(false);
                globReflected[0] = false;
            }
            for (int i = 0; i < globs.Length; i++)
            {
                SpitterAcid.Glob g = a.globs[i];
                if (g.alive != globs[i].gameObject.activeSelf)
                {
                    globs[i].gameObject.SetActive(g.alive);
                    if (g.alive) globTrails[i].Clear();
                }
                bool reflected = g.alive && g.reflected;
                if (reflected != globReflected[i])
                {
                    // NIGHT17 round 4: the parried glob turns cyan and trails
                    globReflected[i] = reflected;
                    globRends[i].sharedMaterial = reflected ? globCyanMat : globMat;
                    globTrails[i].emitting = reflected;
                    if (!reflected) globTrails[i].Clear();
                }
                if (g.alive) globs[i].position = WebSpace.ToUnity(g.previous + (g.position - g.previous) * alpha);
            }
            if (trailWarm == 0 && game.cam != null && globs.Length > 0 && !a.globs[0].alive)
            {
                // NIGHT17 round 5 (no first-parry hitch): glob 0, tiny and cyan, 1 m ahead with a two-point trail for one frame,
                // so the trail renderer's pipeline state exists before the first reflected glob
                trailWarm = 1;
                globScale = globs[0].localScale;
                Vector3 at = CrunchWarm.Point(game.cam);
                globs[0].gameObject.SetActive(true);
                globs[0].position = at;
                globs[0].localScale = Vector3.one * UnityTuning.CrunchWarmScale;
                globRends[0].sharedMaterial = globCyanMat;
                globTrails[0].Clear();
                globTrails[0].widthMultiplier = UnityTuning.CrunchWarmScale; // sub-pixel
                globTrails[0].AddPosition(at);
                globTrails[0].AddPosition(at + game.cam.transform.up * UnityTuning.CrunchWarmScale);
            }
            for (int i = 0; i < puddles.Length; i++)
            {
                SpitterAcid.Puddle pd = a.puddles[i];
                if (pd.alive != puddles[i].gameObject.activeSelf) puddles[i].gameObject.SetActive(pd.alive);
                if (!pd.alive) continue;
                float grow = Mathf.Min(1, (float)pd.age / 0.2f) * (1 - Mathf.Max(0, ((float)pd.age - puddleTime + 0.5f) / 0.5f));
                puddles[i].localScale = new Vector3(puddleRadius * 2 * grow, 0.02f, puddleRadius * 2 * grow); // 0.04 m thick
                puddles[i].position = WebSpace.ToUnity(new Vec3(pd.position.x, pd.position.y + 0.03, pd.position.z));
            }
        }
    }

    /// <summary>
    /// fx/particles.js burst(): pooled cubes flying out and falling, shrinking as they die (the port's own hashed directions
    /// and jitter, unchanged). NIGHT12 U3-25.4 drew them as one instanced mesh (Graphics.RenderMeshInstanced, SKYGRIND/Particle).
    /// NIGHT17 round 1 (builder X): build 2's Player drew nothing through RenderMeshInstanced (no death-burst cube at a punch
    /// kill 1.4 m ahead, no kill ring, no block), so the cubes now draw as the web's colours in buckets (a burst's colour x 2,
    /// as the neon material's SetColor did): per colour one CubeBatch (the cubes' corners written into one dynamic mesh each
    /// frame) drawn with Graphics.RenderMesh on a copy of the neon material (URP Unlit, both faces), the path the muzzle star
    /// and the corpse proved in the Player. No GameObject per cube, no allocation per frame.
    /// </summary>
    public sealed class Particles
    {
        struct P
        {
            public Vector3 pos, vel;
            public float life, age;
            public int bucket; // NIGHT17: the colour's batch
            public bool alive;
            public float scale; // NIGHT16 polish #41: particles.js p.scale, x the pool's cube size (fx.size)
        }

        readonly P[] parts;
        readonly Material neon;
        readonly float gravity, size;
        int next;
        // NIGHT16 polish #50: particles.js setCap (quality.presets.<level>.particleCap, read once): the ring of take() runs over
        // [0, cap), the oldest recycled first; a lower cap kills the cubes past it. The level is LightAndLens' preset in use.
        readonly double capLow, capMedium, capHigh;
        int cap;
        string capLevel = "";
        /// <summary>The quality preset in use (LightAndLens.Level; null: none yet, HIGH's cap).</summary>
        public System.Func<string> QualityLevel;
        uint hash = 2166136261;
        // NIGHT17: the colour buckets (the colours the bursts use: the enemy, the gun families, the traps; the last bucket
        // takes any colour past UnityTuning.ParticleBuckets)
        readonly Color[] bucketKey = new Color[UnityTuning.ParticleBuckets];
        readonly CubeBatch[] batches = new CubeBatch[UnityTuning.ParticleBuckets];
        readonly RenderParams[] bucketRp = new RenderParams[UnityTuning.ParticleBuckets];
        int buckets;
        bool drawLogged; // NIGHT17 round 1: the first draw's one log line

        public Particles(GameConfig cfg, Material neon)
        {
            int n = Mathf.Min(cfg.Int("fx.poolSize"), UnityTuning.InstancedMax);
            this.neon = neon;
            gravity = (float)cfg.Num("fx.gravity");
            size = (float)cfg.Num("fx.size");
            parts = new P[n];
            cap = n; // particles.js: let cap = poolSize
            capLow = cfg.Num("quality.presets.low.particleCap");
            capMedium = cfg.Num("quality.presets.medium.particleCap");
            capHigh = cfg.Num("quality.presets.high.particleCap");
            // NIGHT17 round 2 (the first kill's hitch): the colours the bursts use made at load (a material and a batch each):
            // the enemy's (death and hit bursts), the sparks', every gun family's
            BucketOf(Look.C(cfg, "colors.enemy"));
            BucketOf(Look.C(cfg, "colors.spark"));
            foreach (KeyValuePair<string, object> f in cfg.Obj("weapons.families")) BucketOf(Look.Hex((uint)PortData.Num(f.Value)));
        }

        bool warmed; // NIGHT17 round 2: the one tiny draw on the first frame with a camera

        float H()
        {
            hash ^= hash << 13;
            hash ^= hash >> 17;
            hash ^= hash << 5;
            return (hash & 0xffffff) / (float)0x1000000;
        }

        /// <summary>NIGHT17: the batch for a burst's colour (made on its first use: a material and a mesh per colour, once).</summary>
        int BucketOf(Color color)
        {
            for (int i = 0; i < buckets; i++) if (bucketKey[i].Equals(color)) return i;
            if (buckets == bucketKey.Length) return buckets - 1;
            bucketKey[buckets] = color;
            var m = new Material(neon) { name = "Particles " + buckets };
            m.SetColor("_BaseColor", color * 2); // the neon material's SetColor(colour x 2) (particles.js)
            m.SetFloat("_Cull", 0); // both faces
            batches[buckets] = new CubeBatch(parts.Length, "Particles " + buckets);
            bucketRp[buckets] = new RenderParams(m) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            return buckets++;
        }

        /// <summary>particles.js burst(position, colorHex, count, speed, life, scale = 1): `scale` x the pool's cube size.</summary>
        public void Burst(Vec3 at, Color color, int count, float speed, float life, float scale = 1)
        {
            Vector3 p = WebSpace.ToUnity(at);
            int b = BucketOf(color);
            for (int k = 0; k < count; k++)
            {
                int i = FxGates.TakeSlot(ref next, cap); // particles.js take(): the oldest recycled first, within the cap (#50)
                Vector3 dir = new Vector3(H() * 2 - 1, H() * 1.4f - 0.2f, H() * 2 - 1).normalized;
                parts[i] = new P { pos = p, vel = dir * speed * (0.6f + 0.6f * H()), life = life * (0.7f + 0.6f * H()), age = 0, bucket = b, alive = true, scale = scale };
            }
        }

        /// <summary>particles.js setCap(n): use at most `n` particles; the rest die at once.</summary>
        void SetCap(double n)
        {
            cap = FxGates.ParticleCap(parts.Length, n);
            for (int i = cap; i < parts.Length; i++) parts[i].alive = false;
        }

        public void Render(double frameDt)
        {
            string level = QualityLevel != null ? QualityLevel() : null;
            if (!ReferenceEquals(level, capLevel))
            {
                capLevel = level; // the preset changed (main.js applyPreset: particles.setCap(q.presets[next].particleCap))
                SetCap(FxGates.PresetValue(level, capLow, capMedium, capHigh));
            }
            float dt = (float)frameDt;
            // NIGHT17 U3-30.5: inside a freeze (given 0) the cubes drift at CrunchFreezeDrift on frame time and don't age: the
            // kill's burst opens a little while the world holds (a freeze with nothing moving reads as lag)
            float drift = dt == 0 ? EnemyLook.FreezeDriftDt : 0;
            for (int b = 0; b < buckets; b++) batches[b].Begin();
            if (!warmed && buckets > 0)
            {
                // NIGHT17 round 2: the batch path's pipeline state (neon copy, both faces, a positions-only mesh) made now
                Camera main = Camera.main;
                if (main != null)
                {
                    warmed = true;
                    batches[0].Add(CrunchWarm.Point(main), UnityTuning.CrunchWarmScale);
                    batches[0].Draw(bucketRp[0]);
                    batches[0].Begin();
                }
            }
            int n = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                if (!parts[i].alive) continue;
                P q = parts[i];
                if (drift > 0) q.pos += q.vel * drift;
                q.age += dt;
                if (q.age >= q.life)
                {
                    parts[i].alive = false;
                    continue;
                }
                q.vel.y -= gravity * dt;
                q.pos += q.vel * dt;
                parts[i] = q;
                float s = FxGates.ParticleScale(size, q.scale, q.age, q.life); // particles.js :135 (life / maxLife) x p.scale
                batches[q.bucket].Add(q.pos, s);
                n++;
            }
            if (n == 0) return;
            if (!drawLogged)
            {
                // NIGHT17 round 1 (builder X): one line proving the first draw (build 2's Player showed no cube)
                drawLogged = true;
                Debug.Log($"[particles] first draw: {n} cubes in {buckets} colour batch(es), RenderMesh on {bucketRp[0].material.shader.name}");
            }
            for (int b = 0; b < buckets; b++) batches[b].Draw(bucketRp[b]);
        }
    }
}
