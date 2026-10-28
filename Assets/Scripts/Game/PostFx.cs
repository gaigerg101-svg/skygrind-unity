// post.js's per-frame half on the URP Volume (NIGHT12 U3-25.2; Look.Apply builds the Volume): the chromatic aberration
// pulses (post.chromaHurt on the player's "hurt", chromaDash on the mover's dash, chromaRank on a style rank up: the
// largest kick wins, decaying at post.chromaDecay per second) plus the speed share (post.chromaSpeed between
// chromaSpeedMin and chromaSpeedMax m/s), 0 with reduced motion and x effects intensity; low HP under post.lowHpAt
// darkens the vignette (post.lowHpDarkness) and drains the grade's saturation (grade.js: x (1 - 0.6 hurt)) with the LOW
// HEALTH VIGNETTE setting on; the style energy lifts the bloom (post.bloomEnergy: setEnergy, eased at style.energyEase as
// main.js updateRankWorld does); the quality preset's bloom, grain and SMAA (post.quality.*, quality.presets.*.grain,
// settings.filmGrain) and the tone map setting (settings.toneMapping: ACES, NEUTRAL; URP has no AgX, so AGX draws ACES).
// Frame time (post.js render(frameDt)); no allocation per frame (parameter values only).
// NIGHT16 polish #9 (port-only): the round-clear beat: while the round's last kill slows time (the impact layer's
// `slowMo`, cause lastKill) the vignette deepens by UnityTuning.BeatVignetteBoost and lets go after it.
// NIGHT13 U3-26.4 (PerfSwitches volume, on by default): an override is written only when its value changed, and the
// Volume framework runs "Via Scripting" on both cameras (the main one and the viewmodel's overlay) instead of URP's every
// frame update (PC_RPAsset's Volume Update Mode: Every Frame, which recomputes each camera's whole stack every frame):
// the main camera's stack is updated on a frame where a value changed; the overlay's (it renders no post: only the
// final blit's HDR-output tone map could read its stack) when the preset or the tone map changes. The switch is made
// once VolumeManager exists (URP builds it with the pipeline, at the first render); until then URP updates every frame.
// NIGHT17 U3-30.5 (builder X, UnityTuning.Crunch.cs): the impact frame, on a PERFECT PARRY and on every third kill of a
// multikill (kills within style.multikillWindow of sim time) only: one high-key frame (white over the world at
// CrunchFrameAlpha, the second frame at CrunchFrameSecond of it; never an inverted palette, which maps cyan toward red:
// CLAUDE.md rule 8), x FLASH INTENSITY, through the game's one flash gate (refused: no frame); its own canvas over the
// impact flash, under the HUD; the port-only event `impactFrame` (payload: its cause) on the frame it shows, for GG's
// sounds. And the sprint's tint: a steady chromatic share while StaminaHud says sprinting (the dash keeps its kick).
// NIGHT17 round 3 (the reviewer: "never lighten the whole screen"): the white impact frame is gone; a big moment (a
// headshot kill, a punch kill, every third kill of a multikill, a PERFECT PARRY) CRUSHES the frame instead: the world's
// post exposure drops UnityTuning.CrushEv (about -2 EV: ~25 % value toward the ink) for CrushHold s, eased back over
// CrushFade s (2-4 frames, real time: it plays through the freeze), x FLASH INTENSITY (0: none), through the flash gate;
// the victim's blocks rise by as much (Chunks: their white x 2^-EV), so they stay white-hot with the bloom. No hue moves
// (rule 8). Never on a round-clear's last kill. The port-only event impactFrame (payload: the cause) on its first frame.
// NIGHT18 U3-31.9 backlog 4 (builder X; FeelAB on): under post.lowHpAt the vignette beats (a beat and a smaller echo,
// quickening from LowHpPulseRestHz to LowHpPulseLowHz as HP falls; x FLASH INTENSITY; steady with REDUCED MOTION or the
// LOW HEALTH VIGNETTE setting off) and its colour leans toward DANGER (colors.enemy) with the hurt share. No sound code: the
// web's `hurt` stays GG's hook.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed class PostFx
    {
        readonly SurvivalGame game;
        readonly PostStack post;
        readonly UniversalAdditionalCameraData camData;
        readonly double chromaHurt, chromaDash, chromaRank, chromaDecay, chromaSpeed, chromaSpeedMin, chromaSpeedMax;
        readonly double lowHpAt, bloomEnergy, energyEase;
        double saturation, bloomBase; // NIGHT33 L: look33 sets them per theme (no longer readonly)
        // NIGHT33 L (design.n33.look33; DESIGN33 L round 2): the post profile (L1 section 5): Neutral tonemapping, bloom threshold 1.0 / scatter 0.6 / clamp 10, the
        // per-theme bloom, post-exposure and saturation, and the low-HP vignette kept black (the red stays in the HUD's edge frame: the outer 7 % with n31.lowHealthEdge)
        readonly bool look33;
        readonly Bend34Grade bend34; // NIGHT34 H (design.n34.bendLook): null with the row off
        double exposure33, saturationRest;
        object theme33For;
        /// <summary>For the reel line: the theme look33 applied (-1 none), and its bloom / EV / saturation.</summary>
        public int Theme33 { get; private set; } = -1;
        // NIGHT49 V (design.n49.look49, a view row; UnityTuning.V49.cs): the one grade and the colour script, on look33's per-theme path (look49 needs
        // look33 on: the theme, the held ambient / fog and the stone globals are look33's machinery). Off: look33's values exactly, as before.
        readonly bool look49Row;
        /// <summary>NIGHT49 V: the reel's switch (a349 takes the same frames with look49 forced off, then on); null = the row and the level list.
        /// NIGHT50 V: a force wins over the level list too (true: the grade on whatever level is booted, listed or not; false: off), so the reel
        /// can take its off / on frames on a level before the freeze lists it.</summary>
        public static bool? Look49Force;
        bool? look49ForceShown;
        /// <summary>For the reel line: true while look49's grade is applied (a theme matched and the reel's force, or else the row and the level list, on).</summary>
        public bool Look49Applied { get; private set; }
        // NIGHT50 V (G3, DECIDE50 D-4): the grade per level. design.n49.look49.numbers.levels lists the level ids the grade is ON for (with the row
        // on); an empty list = off on every level. A level's id is its map's (LevelIdOf: LevelDef.From's rule, the maps.list id, e.g. n48-foundry),
        // matched exactly: NightRows.BaseId is not used, because it walks every swapped level back to one base (n46-foundry and n48-foundry would
        // both match an n48-foundry entry), which would make the list per theme instead of per level.
        readonly string[] look49Levels;
        /// <summary>For the reel line (NIGHT50 V): this map's level id, and the config's verdict for it with no force (the row on and the id listed).</summary>
        public string Look49Level { get; private set; } = "";
        public bool Look49Gate { get; private set; }
        // NIGHT50 V (UnityTuning.V50SunScale, darker floors on a V50 level): the sun this scaled, its own intensity, the value this wrote
        Light sun50;
        float sunBase50, sunWritten50;
        bool sunScaled50;
        readonly Vector4 gradeS0, gradeM0, gradeH0; // Look.BuildPost's grade, put back with look49 off
        readonly bool gradeMOn0, contrastOn0;
        readonly float contrast0;
        readonly float vigRestI, vigRestS, vigHurtI, vigHurtS;
        double pulse, energy, hurtShown = -1;
        int lastRank;
        bool bloomOn = true, hurtVignette = true;
        bool hqBloomShown72; // NIGHT72 fix72: the bloom's High Quality Filtering as last written

        // NIGHT13 U3-26.4: writes on change and the Volume framework Via Scripting (PerfSwitches volume).
        readonly bool scripted;
        readonly Camera overlay; // the viewmodel's camera (null without one)
        readonly UniversalAdditionalCameraData overlayData;
        float chromaShown = float.NaN, bloomShown = float.NaN;
        bool scriptedSet, mainDirty = true, overlayDirty = true;

        public PostFx(SurvivalGame game, PostStack post)
        {
            this.game = game;
            this.post = post;
            GameConfig cfg = game.cfg;
            camData = game.cam != null ? game.cam.GetUniversalAdditionalCameraData() : null;
            scripted = PerfSwitches.On(PerfFix.Volume) && camData != null;
            overlay = game.viewmodel?.Camera;
            overlayData = overlay != null ? overlay.GetUniversalAdditionalCameraData() : null;
            chromaHurt = cfg.Num("post.chromaHurt");
            chromaDash = cfg.Num("post.chromaDash");
            chromaRank = cfg.Num("post.chromaRank");
            chromaDecay = cfg.Num("post.chromaDecay");
            chromaSpeed = cfg.Num("post.chromaSpeed");
            chromaSpeedMin = cfg.Num("post.chromaSpeedMin");
            chromaSpeedMax = cfg.Num("post.chromaSpeedMax");
            lowHpAt = cfg.Num("post.lowHpAt");
            saturation = saturationRest = cfg.Num("post.grade.saturation");
            bloomBase = cfg.Num("post.bloomIntensity") * UnityTuning.BloomIntensityScale;
            look33 = N33Fixes.ViewRowOn(cfg, "look33");
            if (look33)
            {
                bloomBase = UnityTuning.Look33BloomDefault;
                post.bloom.threshold.Override(UnityTuning.Look33BloomThreshold);
                post.bloom.scatter.Override(UnityTuning.Look33BloomScatter);
                post.bloom.clamp.Override(UnityTuning.Look33BloomClamp);
                post.tone.mode.Override(TonemappingMode.Neutral);
            }
            if (N35Fixes.ViewRowOn(cfg, "lights35")) post.bloom.threshold.Override(UnityTuning.Look35BloomThreshold); // NIGHT35 V lights35: bloom on the fixtures only
            look49Row = NightRows.ViewOn(cfg, "look49"); // NIGHT49 V: read once; the grade is written on the theme's apply (ApplyTheme33)
            look49Levels = Look49LevelsOf(cfg); // NIGHT50 V: the level list, read once; matched against the map's id at the theme's apply
            gradeS0 = post.grade.shadows.value;
            gradeM0 = post.grade.midtones.value;
            gradeMOn0 = post.grade.midtones.overrideState;
            gradeH0 = post.grade.highlights.value;
            contrast0 = post.color.contrast.value;
            contrastOn0 = post.color.contrast.overrideState;
            bloomEnergy = cfg.Num("post.bloomEnergy");
            energyEase = cfg.Num("style.energyEase");
            // The vignette at rest and at the lowest HP (darkness + lowHpDarkness), each fitted to URP's curve once.
            double offset = cfg.Num("post.vignetteOffset"), dark = cfg.Num("post.vignetteDarkness");
            Look.FitVignette(offset, dark, out vigRestI, out vigRestS);
            Look.FitVignette(offset, dark + cfg.Num("post.lowHpDarkness"), out vigHurtI, out vigHurtS);
            dangerColor = Look.C(cfg, "colors.enemy"); // NIGHT18 backlog 4: the low-HP vignette's lean (DANGER)
            if (N34Fixes.ViewRowOn(cfg, "bendLook")) bend34 = new Bend34Grade(game, post); // NIGHT34 H: off, no curve is added to the Volume

            // post.js: kick(amount) = the largest of the pulse and the amount; the player's own hurt and dash.
            game.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner == "player") Kick(chromaHurt);
            });
            game.mover.Events += e =>
            {
                if (e.type == MoverEventType.Dash) Kick(chromaDash);
            };
            // NIGHT16 polish #9 (port-only): the round's last kill slows time (the impact layer's `slowMo`, cause lastKill);
            // the vignette deepens with it and lets go after it (the round-clear beat: the slow-mo, the ceremony's banner, this).
            game.events.On<SlowMoEvent>(FeelKeys.SlowMo, e =>
            {
                if (e.cause != "lastKill") return;
                beatAge = 0;
                beatHold = e.seconds;
                roundClearFrame = Time.frameCount; // NIGHT17 round 2: the round's last kill: no impact frame on it
            });
            // NIGHT17 round 2: the round clears (the run's event, inside the step): no impact frame on this frame
            game.events.On("roundClear", _ => roundClearFrame = Time.frameCount);

            // NIGHT17 U3-30.5: the impact frame's moments (handlers only queue it; Render shows it)
            multikillWindow = cfg.Num("style.multikillWindow");
            game.events.On<KillEvent>("kill", e =>
            {
                if (e.owner != "player") return;
                double now = game.SimTime;
                chain = now - lastKillAt <= multikillWindow ? chain + 1 : 1;
                lastKillAt = now;
                if (CrunchMath.ImpactFrameDue(chain, UnityTuning.CrunchMultiStep)) frameCause = FrameMultikill;
                else if (e.melee) frameCause = FramePunchKill; // NIGHT17 round 3: the crush's big kills
                else if (e.headshot) frameCause = FrameHeadshotKill;
            });
            // NIGHT17 round 2 (the first kill's hitch): the overlay's canvas, image and ring sprite made at load, hidden
            EnsureFrameCanvas();
            frameCanvas.enabled = false;
            game.events.On<HitEvent>("parry", e =>
            {
                if (e.owner != "player" || !e.perfect) return;
                frameCause = FramePerfectParry;
                ringAsk = true; // round 1: the cyan ring from the contact point (the payload is reused: keep its point)
                ringAt = e.position;
            });
        }

        // NIGHT17 round 1 (the reviewer): a PERFECT PARRY's cyan ring burst from the contact point on screen: 0 -> CrunchRingPx
        // px (at 1080p) over CrunchRingTime s, ease-out, fading; frame time (it plays through the parry's freeze), on the
        // impact frame's canvas (under the HUD). Not a flash (a thin ring): no gate; FLASH INTENSITY 0 still shows it.
        bool ringAsk;
        Vec3 ringAt;
        double ringAge = double.MaxValue;
        float ringPx; // NIGHT17 round 4: the ring's final size (px) for this parry
        Image ringImage;
        Sprite ringSprite;

        /// <summary>A thin ring sprite (made once): alpha 1 in a band near the edge, soft on both sides.</summary>
        static Sprite RingSprite()
        {
            int n = UnityTuning.CrunchRingTexture;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "Parry ring" };
            var px = new Color32[n * n];
            float band = UnityTuning.CrunchRingWidth;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n * 2 - 1, dy = (y + 0.5f) / n * 2 - 1;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float d = Mathf.Abs(r - (1 - band)) / band; // 0 on the band's middle, 1 at its edges
                byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(1 - d) * 255);
                px[y * n + x] = new Color32(255, 255, 255, a);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100);
        }

        void EnsureFrameCanvas()
        {
            if (frameCanvas != null) return;
            var go = new GameObject("Impact frame", typeof(Canvas));
            frameCanvas = go.GetComponent<Canvas>();
            frameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            frameCanvas.sortingOrder = UnityTuning.CrunchFrameSortingOrder;
            var img = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            img.transform.SetParent(go.transform, false);
            var rt = (RectTransform)img.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            frameImage = img.GetComponent<Image>();
            frameImage.raycastTarget = false;
            frameImage.enabled = false;
            var ring = new GameObject("Parry ring", typeof(RectTransform), typeof(Image));
            ring.transform.SetParent(go.transform, false);
            var rr = (RectTransform)ring.transform;
            rr.anchorMin = rr.anchorMax = Vector2.zero; // positioned in screen pixels from the bottom left
            rr.pivot = new Vector2(0.5f, 0.5f);
            ringImage = ring.GetComponent<Image>();
            ringSprite ??= RingSprite();
            ringImage.sprite = ringSprite;
            ringImage.raycastTarget = false;
            ringImage.enabled = false;
        }

        /// <summary>The ring this frame (frame time).</summary>
        void RenderParryRing(double frameDt)
        {
            if (ringAsk)
            {
                ringAsk = false;
                if (game.flow == Flow.Playing && game.cam != null)
                {
                    Vector3 sp = game.cam.WorldToScreenPoint(WebSpace.ToUnity(ringAt));
                    if (sp.z > 0)
                    {
                        EnsureFrameCanvas();
                        ((RectTransform)ringImage.transform).anchoredPosition = new Vector2(sp.x, sp.y);
                        ringAge = 0;
                        // NIGHT17 round 4 (the reviewer: about twice the body's size): the body's height on screen at its
                        // distance x CrunchRingBodies, never under CrunchRingPx (at 1080p)
                        float tanV = Mathf.Tan(game.cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                        float bodyPx = UnityTuning.CrunchRingBodyM / (2 * Mathf.Max(0.3f, sp.z) * tanV) * Screen.height;
                        ringPx = Mathf.Max(UnityTuning.CrunchRingPx * Screen.height / 1080f, bodyPx * UnityTuning.CrunchRingBodies);
                    }
                }
            }
            if (ringImage == null) return;
            if (ringAge >= UnityTuning.CrunchRingTime)
            {
                if (ringImage.enabled) ringImage.enabled = false;
                return;
            }
            double u = ringAge / UnityTuning.CrunchRingTime;
            ringAge += frameDt;
            double w = 1 - u;
            float grow = (float)(1 - w * w * w); // ease-out
            float px = ringPx * grow;
            var rt = (RectTransform)ringImage.transform;
            rt.sizeDelta = new Vector2(px, px);
            ringImage.color = UiTheme.A(UiTheme.Ready ? UiTheme.Cyan : Color.cyan, (float)(w * w));
            ringImage.enabled = true;
            frameCanvas.enabled = true;
        }

        // NIGHT17 U3-30.5: the impact frame (the queued cause, the multikill chain, the frame it started, the overlay).
        /// <summary>The port-only event the impact frame emits on the frame it shows (payload: the cause string).</summary>
        public const string ImpactFrameEvent = "impactFrame";
        const string FrameMultikill = "multikill", FramePerfectParry = "perfectParry", FramePunchKill = "punchKill", FrameHeadshotKill = "headshotKill";
        // NIGHT17 round 3: the crush (its start in real s, the frame it was last computed, its EV now, the EV last applied)
        double crushStart = double.NegativeInfinity;
        int crushFrame = -1;
        double crushEv;
        float crushShown;
        bool exposureOverridden;

        /// <summary>
        /// NIGHT17 round 3: the crush frame's exposure this frame (EV, 0 or negative). The first caller of a frame (Chunks, which
        /// draws earlier, or Render) starts a queued crush (the flash gate and FLASH INTENSITY allowing), so both see the same
        /// value on the crush's first frame.
        /// </summary>
        public double CrushEvNow()
        {
            int now = Time.frameCount;
            if (crushFrame == now) return crushEv;
            crushFrame = now;
            double fi = game.settings.View.flashIntensity;
            if (frameCause != null && !FeelClock.Holding) // NIGHT63 D1: a same-run held frame starts no crush (the cause waits for the next live frame, as without the hold)
            {
                string cause = frameCause;
                frameCause = null;
                // never on a round-clear's last kill (the round end has its own beat)
                // round 4: not the run's break (every strip holds the first break): the round-clear frame, or while its slow-mo holds
                bool roundEnd = roundClearFrame == now || beatAge < beatHold;
                if (!roundEnd && fi > 0 && game.flow == Flow.Playing && game.flashGate.TryFlash(SurvivalGame.RealNow))
                {
                    crushStart = SurvivalGame.RealNow;
                    ImpactFrameCount++;
                    game.events.Emit(ImpactFrameEvent, cause);
                }
            }
            crushEv = CrunchMath.CrushEv(SurvivalGame.RealNow - crushStart, UnityTuning.CrushHold, UnityTuning.CrushFade, UnityTuning.CrushEv, fi);
            return crushEv;
        }
        readonly double multikillWindow;
        double lastKillAt = double.NegativeInfinity;
        int chain;
        string frameCause;
        int roundClearFrame = -10; // NIGHT17 round 2
        Canvas frameCanvas;
        Image frameImage;
        double sprintChroma;
        StaminaHud.Info stamina;

        /// <summary>Impact frames shown since boot (a Play check reads it).</summary>
        public int ImpactFrameCount { get; private set; }

        /// <summary>The impact frame this frame: a queued cause starts it (the gate and FLASH INTENSITY allowing); 2 frames.</summary>
        void RenderImpactFrame()
        {
            // NIGHT17 round 3: the crush on the post exposure (no white overlay any more: frameImage stays off)
            float ev = (float)CrushEvNow();
            if (!ev.Equals(crushShown))
            {
                crushShown = ev;
                if (!exposureOverridden)
                {
                    exposureOverridden = true;
                    post.color.postExposure.overrideState = true;
                }
                post.color.postExposure.value = UnityTuning.Exposure + (float)exposure33 + ev + (Bright71.On ? UnityTuning.Bright71ExposureLift : 0f); // NIGHT33 L: + the theme's EV (0 with look33 off); GG 2026-10-26: + Bright71's lift
                mainDirty = true;
                if (scripted) UpdateStacks();
            }
            RenderParryRing(game.FrameDt);
            if (frameCanvas != null) frameCanvas.enabled = frameImage.enabled || ringImage.enabled;
        }

        void Kick(double amount) => pulse = System.Math.Max(pulse, amount);

        /// <summary>NIGHT43 F1 (feel43, Game/Feel43.cs): a chroma kick from outside (the FUSION, an OPEN boss hit): the largest pulse wins, as post.js kick.</summary>
        public void Kick43(double amount) => Kick(amount);

        /// <summary>NIGHT43 F1 (feel43): queue the crush frame for `cause` (the FUSION's "fusion43"); shown by the next render through the flash gate,
        /// FLASH INTENSITY and the round-end rule, like the big kills' (the impactFrame event carries the cause).</summary>
        public void Crush43(string cause) => frameCause = cause;

        // NIGHT16 polish #9: the beat's age and hold (real s); its share of UnityTuning.BeatVignetteBoost, last applied.
        double beatAge = double.MaxValue, beatHold, beatShown;

        // NIGHT18 U3-31.9 backlog 4: the low-HP pulse's phase (0-1 of a beat), the intensity it last added, the DANGER tint last
        // applied, and the DANGER colour (colors.enemy)
        double lowHpPhase;
        float pulseShown = -1, tintShown;
        bool edgeShown31; // NIGHT31 G: design.n31.lowHealthEdge as last written
        readonly Color dangerColor;

        /// <summary>
        /// NIGHT18 backlog 4: the low-HP pulse now (0-1: the beat's share at this HP, 0 off / steady). The HUD's own DANGER
        /// breath (Ui.cs, NIGHT17 U3-30.7) may follow it so the edges keep one rhythm (the lead's call).
        /// </summary>
        public static float LowHpPulseNow { get; private set; }

        /// <summary>The beat's share: up over UnityTuning.BeatVignetteIn, held through the slow-mo's hold, eased out over BeatVignetteOut.</summary>
        double BeatShare()
        {
            if (beatAge >= beatHold + UnityTuning.BeatVignetteOut) return 0;
            if (beatAge < UnityTuning.BeatVignetteIn) return beatAge / UnityTuning.BeatVignetteIn;
            if (beatAge < beatHold) return 1;
            double t = (beatAge - beatHold) / UnityTuning.BeatVignetteOut;
            return (1 - t) * (1 - t);
        }

        /// <summary>A new run: no pulse left, the rank from DULL (a rank up is a rise from the last frame's rank).</summary>
        public void NewRun()
        {
            pulse = 0;
            lastRank = 0;
            beatAge = double.MaxValue; // NIGHT16: no beat carried into a new run
            chain = 0; // NIGHT17: no multikill chain or queued impact frame carried over
            lastKillAt = double.NegativeInfinity;
            frameCause = null;
            sprintChroma = 0;
            ringAsk = false; // NIGHT17 round 1: no parry ring carried over
            ringAge = double.MaxValue;
            lowHpPhase = 0; // NIGHT18 backlog 4
        }

        /// <summary>
        /// post.js build(level) and applyToneMapping: the preset's bloom (post.quality.*.bloom; bloomScale under 1 starts
        /// URP's chain at a quarter instead of a half), SMAA (High only), the grain (quality.presets.*.grain while
        /// settings.filmGrain is on), and the tone map.
        /// </summary>
        public void ApplyPreset(bool bloom, double bloomScale, bool smaa, double grain, string toneMapping, bool hurtVignetteOn)
        {
            bloomOn = bloom;
            post.bloom.active = bloom;
            post.bloom.downscale.Override(bloomScale < 1 ? BloomDownscaleMode.Quarter : BloomDownscaleMode.Half);
            if (camData != null) camData.antialiasing = smaa ? AntialiasingMode.SubpixelMorphologicalAntiAliasing : AntialiasingMode.None;
            post.grain.intensity.value = Mathf.Clamp01((float)grain);
            post.grain.active = grain > 0;
            post.tone.mode.value = look33 || toneMapping == "neutral" ? TonemappingMode.Neutral : TonemappingMode.ACES; // NIGHT33 L: look33 is always Neutral (ACES pushes cyan to white)
            hurtVignette = hurtVignetteOn;
            hurtShown = -1; // re-apply the vignette and the saturation
            mainDirty = overlayDirty = true; // NIGHT13: both stacks take the preset (Via Scripting)
        }

        /// <summary>NIGHT33 L: the map's theme row (LightPlan33.Theme; once per map): bloom, post-exposure, saturation. The ambient and fog are RoofView27's.</summary>
        void ApplyTheme33()
        {
            theme33For = game.map.json;
            int t = LightPlan33.Theme(game.map.json);
            Theme33 = t;
            globals33 = false; // the probe, rim and stone fog are taken again for this map
            bloomBase = t >= 0 ? UnityTuning.Look33ThemeBloom[t] : UnityTuning.Look33BloomDefault;
            exposure33 = UnityTuning.Look33ExposureBase - UnityTuning.Exposure + (t >= 0 ? UnityTuning.Look33ThemeExposure[t] : 0); // round 3: an absolute base (0 EV), not NIGHT32's +0.35
            saturation = saturationRest + (t >= 0 ? UnityTuning.Look33ThemeSaturation[t] / 100.0 : 0);
            // NIGHT49 V (look49): the colour script's row for this theme instead (UnityTuning.V49.cs), and the one grade; off: Look.BuildPost's grade back
            // NIGHT50 V (G3): only on a listed level (the row on and the map's id in look49.numbers.levels), unless the reel forces it; iceline and span
            // take UnityTuning.V50.cs's values where it has them (V50Slot)
            Look49Level = LevelIdOf(game.map.json);
            Look49Gate = look49Row && (Look49Listed(look49Levels, Look49Level) || Look52Lift.GradeListed(game.cfg, Look49Level)); // NIGHT52 U look52: its own grade list (on only)
            Look49Applied = (Look49Force ?? Look49Gate) && t >= 0;
            if (Look49Applied)
            {
                int s = V50Slot(t);
                bloomBase = UnityTuning.V49Bloom;
                exposure33 = (s >= 0 ? UnityTuning.V50ExposureEv[s] : UnityTuning.V49ExposureEv[t]) - UnityTuning.Exposure; // absolute EV, as look33's base 0 + its theme EV
                saturation = saturationRest + (s >= 0 ? UnityTuning.V50Saturation[s] : UnityTuning.V49Saturation[t]) / 100.0;
                Grade49(t);
            }
            else
            {
                post.grade.shadows.value = gradeS0;
                post.grade.midtones.value = gradeM0;
                post.grade.midtones.overrideState = gradeMOn0;
                post.grade.highlights.value = gradeH0;
                post.color.contrast.value = contrast0;
                post.color.contrast.overrideState = contrastOn0;
            }
            crushShown = float.NaN; // rewrite the exposure with the theme's EV
            hurtShown = -1; // and the saturation
            bloomShown = float.NaN;
            mainDirty = true;
        }

        /// <summary>NIGHT49 V (look49): the one grade on this theme's hue. ShadowsMidtonesHighlights: the shadows and midtones lean to the level's tint
        /// (UnityTuning.V49GradeTintHex, linear, divided by its luminance, mixed in by V49ShadowTint / V49MidTint, clamped), the shadows' lightness at
        /// V49ShadowOffset, the highlights neutral (the role emissives keep their hue); ColorAdjustments contrast V49Contrast. URP's value is the
        /// multiplier gamma-encoded (Look.BuildPost's way), w the lightness offset.</summary>
        void Grade49(int t)
        {
            int s = V50Slot(t); // NIGHT50 V: iceline / span take V50's tint and contrast
            Color c = Look.Hex(s >= 0 ? UnityTuning.V50GradeTintHex[s] : UnityTuning.V49GradeTintHex[t]).linear;
            float lum = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
            if (lum <= 1e-4f) lum = 1;
            post.grade.shadows.Override(Smh49(c, lum, UnityTuning.V49ShadowTint, UnityTuning.V49ShadowOffset));
            post.grade.midtones.Override(Smh49(c, lum, UnityTuning.V49MidTint, 0f));
            post.grade.highlights.Override(Smh49(c, lum, UnityTuning.V49HighTint, 0f));
            post.color.contrast.Override((float)(s >= 0 ? UnityTuning.V50Contrast[s] : UnityTuning.V49Contrast[t]));
        }

        /// <summary>NIGHT50 V: the V50 slot of a theme (UnityTuning.V50Themes: iceline, span), -1 when the theme keeps its whole V49 row.</summary>
        public static int V50Slot(int t)
        {
            for (int i = 0; i < UnityTuning.V50Themes.Length; i++) if (UnityTuning.V50Themes[i] == t) return i;
            return -1;
        }

        /// <summary>NIGHT50 V: design.n49.look49.numbers.levels, lower case (empty when absent or empty: the grade is off on every level).</summary>
        public static string[] Look49LevelsOf(GameConfig cfg)
        {
            const string key = "design.n49.look49.numbers.levels";
            if (cfg == null || !cfg.Has(key) || !(cfg.Get(key) is List<object> list)) return new string[0];
            var ids = new List<string>();
            foreach (object o in list) if (o is string id && id.Length > 0) ids.Add(id.ToLowerInvariant());
            return ids.ToArray();
        }

        /// <summary>NIGHT50 V: a map's level id, LevelDef.From's rule (the json's "id", else its "name" in lower case: the maps.list id, e.g.
        /// n48-foundry); "" when it has neither (the survival arena's json is not a campaign level).</summary>
        public static string LevelIdOf(Dictionary<string, object> json)
        {
            if (json == null) return "";
            if (json.TryGetValue("id", out object id) && id is string s && s.Length > 0) return s.ToLowerInvariant();
            return json.TryGetValue("name", out object nm) && nm is string n ? n.ToLowerInvariant() : "";
        }

        /// <summary>NIGHT50 V: the id is in the list (exact, ordinal; both lower case).</summary>
        public static bool Look49Listed(string[] levels, string levelId)
        {
            if (levels == null || string.IsNullOrEmpty(levelId)) return false;
            foreach (string l in levels) if (string.Equals(l, levelId, System.StringComparison.Ordinal)) return true;
            return false;
        }

        /// <summary>NIGHT50 V: look49's switch for a map, as PostFx decides it (RoofView27 asks it at its build): the reel's force, else the row on
        /// AND the map's level listed.</summary>
        public static bool Look49OnFor(GameConfig cfg, Dictionary<string, object> json) =>
            Look49Force ?? (NightRows.ViewOn(cfg, "look49") && (Look49Listed(Look49LevelsOf(cfg), LevelIdOf(json)) || Look52Lift.GradeListed(cfg, LevelIdOf(json)))); // NIGHT52 U look52: its own grade list (on only)

        /// <summary>NIGHT50 V (UnityTuning.V50SunScale, darker floors): while the grade is applied on a V50 level (slot >= 0) the sun is held at its own
        /// intensity x the scale; off (slot -1) the intensity this scaled is put back (unless another writer changed it since). Another writer's value
        /// (DuskClock22 dimming the key) becomes the base. A new sun (each scene's Look.Apply makes one) starts unscaled.</summary>
        void Sun50(int slot)
        {
            Light s = RenderSettings.sun;
            if (s != sun50) { sun50 = s; sunScaled50 = false; }
            if (s == null) return;
            if (slot < 0)
            {
                if (sunScaled50 && s.intensity == sunWritten50) s.intensity = sunBase50;
                sunScaled50 = false;
                return;
            }
            if (!sunScaled50 || s.intensity != sunWritten50) sunBase50 = s.intensity;
            float want = sunBase50 * UnityTuning.V50SunScale[slot];
            if (s.intensity != want) s.intensity = want;
            sunWritten50 = want;
            sunScaled50 = true;
        }

        /// <summary>One trackball: per channel mix(1, channel / luminance, share), clamped to V49TintMin..Max, gamma-encoded; w = the lightness offset.</summary>
        static Vector4 Smh49(Color linearTint, float lum, float share, float offset)
        {
            float r = Mathf.Clamp(Mathf.Lerp(1f, linearTint.r / lum, share), UnityTuning.V49TintMin, UnityTuning.V49TintMax);
            float g = Mathf.Clamp(Mathf.Lerp(1f, linearTint.g / lum, share), UnityTuning.V49TintMin, UnityTuning.V49TintMax);
            float b = Mathf.Clamp(Mathf.Lerp(1f, linearTint.b / lum, share), UnityTuning.V49TintMin, UnityTuning.V49TintMax);
            return new Vector4(Mathf.LinearToGammaSpace(r), Mathf.LinearToGammaSpace(g), Mathf.LinearToGammaSpace(b), offset);
        }

        /// <summary>NIGHT49 V (look49): the level's stone terms (look33's Globals33 with the colour script's values): the rim in the level hue at
        /// V49RimScale, the stone's sky fog in the level's dark hue, and the SH probe from the level's trilight; held every frame (written when it differs).</summary>
        void Globals49(int t)
        {
            if (!globals33)
            {
                globals33 = true;
                GameConfig cfg = game.cfg;
                rim33 = Look.Hex(UnityTuning.V49RimHex[t], (float)cfg.Num("lights.rimIntensity") * UnityTuning.LightScale * 3 * UnityTuning.V49RimScale);
                fogZ33 = Look.Hex(UnityTuning.V49StoneFogZenithHex[t]);
                fogM33 = Look.Hex(UnityTuning.V49StoneFogMidHex[t]);
                fogH33 = Look.Hex(UnityTuning.V49StoneFogHorizonHex[t]);
                ProbeOf(UnityTuning.V49AmbientSkyHex[t], UnityTuning.V49AmbientEquatorHex[t], UnityTuning.V49AmbientGroundHex[t]);
                Look52Lift.Walls(cfg, V50Slot(t), ref rim33); // NIGHT52 U look52 (Look52.cs): iceline / span's walls lifted in their hue (the rim, the probe's equator); off: nothing
            }
            if (Shader.GetGlobalColor(RimColorId) != rim33) Shader.SetGlobalColor(RimColorId, rim33);
            if (Shader.GetGlobalColor(SkyFogZenithId) != fogZ33) Shader.SetGlobalColor(SkyFogZenithId, fogZ33);
            if (Shader.GetGlobalColor(SkyFogMidId) != fogM33) Shader.SetGlobalColor(SkyFogMidId, fogM33);
            if (Shader.GetGlobalColor(SkyFogHorizonId) != fogH33) Shader.SetGlobalColor(SkyFogHorizonId, fogH33);
        }

        /// <summary>NIGHT49 V: the SH probe from a trilight (Probe33's construction: the middle everywhere, the top / bottom as one up / down lobe).</summary>
        static void ProbeOf(uint skyHex, uint eqHex, uint groundHex)
        {
            Color sky = Look.Hex(skyHex).linear, eq = Look.Hex(eqHex).linear, gr = Look.Hex(groundHex).linear;
            var sh = new SphericalHarmonicsL2();
            sh.AddAmbientLight(eq);
            sh.AddDirectionalLight(Vector3.up, (sky - gr) * 0.5f, 1f);
            RenderSettings.ambientProbe = sh;
        }

        /// <summary>NIGHT49 V (look49): the level's trilight ambient and exponential fog (Ambient33's with the colour script's row), held every frame.
        /// NIGHT50 V: internal so RoofView27's build writes the same values (it asks Look49OnFor first).</summary>
        internal static void Ambient49(int t)
        {
            Color sky = Look.Hex(UnityTuning.V49AmbientSkyHex[t]), eq = Look.Hex(UnityTuning.V49AmbientEquatorHex[t]), gr = Look.Hex(UnityTuning.V49AmbientGroundHex[t]), fog = Look.Hex(UnityTuning.V49FogHex[t]);
            float dens = (float)UnityTuning.V49FogDensity[t];
            if (RenderSettings.ambientMode != AmbientMode.Trilight) RenderSettings.ambientMode = AmbientMode.Trilight;
            if (RenderSettings.ambientSkyColor != sky) RenderSettings.ambientSkyColor = sky;
            if (RenderSettings.ambientEquatorColor != eq) RenderSettings.ambientEquatorColor = eq;
            if (RenderSettings.ambientGroundColor != gr) RenderSettings.ambientGroundColor = gr;
            if (!RenderSettings.fog) RenderSettings.fog = true;
            if (RenderSettings.fogMode != FogMode.Exponential) RenderSettings.fogMode = FogMode.Exponential;
            if (RenderSettings.fogDensity != dens) RenderSettings.fogDensity = dens;
            if (RenderSettings.fogColor != fog) RenderSettings.fogColor = fog;
        }

        /// <summary>NIGHT33 L round 3: the theme's ambient and fog, held every frame (the lead's n33-r3 line printed Look's hemisphere ambient 0.22-0.33 and Linear fog:
        /// something after RoofView27's build, or its OnDestroy restore on a reload, put NIGHT32's back). Written only when a value differs.</summary>
        static readonly int RimColorId = Shader.PropertyToID("_RimColor"), SkyFogZenithId = Shader.PropertyToID("_SkyFogZenith"), SkyFogMidId = Shader.PropertyToID("_SkyFogMid"), SkyFogHorizonId = Shader.PropertyToID("_SkyFogHorizon");
        Color rim33, fogZ33, fogM33, fogH33;
        bool globals33;

        /// <summary>NIGHT33 L round 3: the stone's own lighting terms that RenderSettings does not reach. Stone.shader lights with SampleSH (the AMBIENT PROBE: nothing
        /// in the project ever recomputes it at runtime, so the trilight colours alone change nothing in a Player), an UNSHADOWED rim (_RimColor: it lifts every
        /// wall inside a closed room) and its sky-coloured fog (_SkyFog*: the far wall goes lavender). look33 writes the probe from the theme's trilight, scales the
        /// rim by Look33RimScale and dims the stone fog's colours by Look33StoneFogDim; each held every frame (written when it differs).</summary>
        void Globals33(int t)
        {
            if (!globals33)
            {
                globals33 = true;
                // from the config, as Look.Apply writes them (never from the globals: they already carry this scale after the first frame)
                GameConfig cfg = game.cfg;
                rim33 = Look.C(cfg, "colors.lightRim", (float)cfg.Num("lights.rimIntensity") * UnityTuning.LightScale * 3) * UnityTuning.Look33RimScale;
                fogZ33 = Look.C(cfg, "colors.skyZenith") * UnityTuning.Look33StoneFogDim;
                fogM33 = Look.C(cfg, "colors.skyMid") * UnityTuning.Look33StoneFogDim;
                fogH33 = Look.C(cfg, "colors.skyHorizon") * UnityTuning.Look33StoneFogDim;
                Probe33(t);
            }
            if (Shader.GetGlobalColor(RimColorId) != rim33) Shader.SetGlobalColor(RimColorId, rim33);
            if (Shader.GetGlobalColor(SkyFogZenithId) != fogZ33) Shader.SetGlobalColor(SkyFogZenithId, fogZ33);
            if (Shader.GetGlobalColor(SkyFogMidId) != fogM33) Shader.SetGlobalColor(SkyFogMidId, fogM33);
            if (Shader.GetGlobalColor(SkyFogHorizonId) != fogH33) Shader.SetGlobalColor(SkyFogHorizonId, fogH33);
        }

        /// <summary>The ambient probe from the theme's trilight (linear): the middle colour everywhere, the sky above and the ground below as one up/down lobe.</summary>
        static void Probe33(int t)
        {
            Color sky = Look.Hex(UnityTuning.Look33ThemeAmbientSky[t]).linear, eq = Look.Hex(UnityTuning.Look33ThemeAmbientEquator[t]).linear, gr = Look.Hex(UnityTuning.Look33ThemeAmbientGround[t]).linear;
            var sh = new SphericalHarmonicsL2();
            sh.AddAmbientLight(eq);
            sh.AddDirectionalLight(Vector3.up, (sky - gr) * 0.5f, 1f);
            RenderSettings.ambientProbe = sh;
        }

        static void Ambient33(int t)
        {
            Color sky = Look.Hex(UnityTuning.Look33ThemeAmbientSky[t]), eq = Look.Hex(UnityTuning.Look33ThemeAmbientEquator[t]), gr = Look.Hex(UnityTuning.Look33ThemeAmbientGround[t]), fog = Look.Hex(UnityTuning.Look33ThemeFogHex[t]);
            float dens = (float)UnityTuning.Look33ThemeFogDensity[t];
            if (RenderSettings.ambientMode != AmbientMode.Trilight) RenderSettings.ambientMode = AmbientMode.Trilight;
            if (RenderSettings.ambientSkyColor != sky) RenderSettings.ambientSkyColor = sky;
            if (RenderSettings.ambientEquatorColor != eq) RenderSettings.ambientEquatorColor = eq;
            if (RenderSettings.ambientGroundColor != gr) RenderSettings.ambientGroundColor = gr;
            if (!RenderSettings.fog) RenderSettings.fog = true;
            if (RenderSettings.fogMode != FogMode.Exponential) RenderSettings.fogMode = FogMode.Exponential;
            if (RenderSettings.fogDensity != dens) RenderSettings.fogDensity = dens;
            if (RenderSettings.fogColor != fog) RenderSettings.fogColor = fog;
        }

        /// <summary>post.js setHealth / setSpeed / setEnergy and render(frameDt)'s parameter half.</summary>
        public void Render(double frameDt)
        {
            if (look33)
            {
                // NIGHT49 V: a change of the reel's force re-applies the theme (NIGHT50 V: the row and the level list are fixed per map: read at the apply)
                bool? force49 = Look49Force;
                if (force49 != look49ForceShown) { look49ForceShown = force49; theme33For = null; }
            }
            if (look33 && game.map != null && !ReferenceEquals(game.map.json, theme33For)) ApplyTheme33();
            if (look33 && Theme33 >= 0)
            {
                if (Look49Applied) { Ambient49(Theme33); Globals49(Theme33); } // NIGHT49 V: the colour script's row
                else { Ambient33(Theme33); Globals33(Theme33); }
            }
            Sun50(look33 && Look49Applied ? V50Slot(Theme33) : -1); // NIGHT50 V: darker floors on iceline / span (UnityTuning.V50SunScale)
            Flow flow = game.flow;
            bool playing = flow == Flow.Playing || flow == Flow.Down; // the web's 'playing' state holds DOWN
            // A style rank up (post.js rankChange with up): a rise in the meter's rank since the last frame.
            int rank = game.style.rank;
            if (rank > lastRank && flow == Flow.Playing) Kick(chromaRank);
            lastRank = rank;

            // Chromatic aberration: the pulse decays on frame time; the speed share grows from chromaSpeedMin to Max.
            pulse = System.Math.Max(0, pulse - frameDt * chromaDecay);
            double speed = playing ? game.mover.Speed : 0;
            double fast = System.Math.Max(0, System.Math.Min(1, (speed - chromaSpeedMin) / (chromaSpeedMax - chromaSpeedMin)));
            SettingsConfig st = game.cfg.movement.settings;
            // NIGHT17 U3-30.5: the sprint's steady share (StaminaHud), eased on frame time; the boost keeps the dash's kick
            StaminaHud.Read(game, ref stamina);
            bool sprinting = playing && stamina.valid && stamina.sprinting && !stamina.boosting;
            sprintChroma += ((sprinting ? UnityTuning.SprintChroma : 0) - sprintChroma) * (1 - System.Math.Exp(-UnityTuning.SprintChromaEase * frameDt));
            double amount = st.reducedMotion ? 0 : (pulse + fast * chromaSpeed + sprintChroma) * st.effectsIntensity;
            float chroma = Mathf.Clamp01((float)(amount * UnityTuning.ChromaScale));
            if (!scripted) post.chroma.intensity.value = chroma;
            else if (!chroma.Equals(chromaShown))
            {
                chromaShown = chroma;
                post.chroma.intensity.value = chroma;
                mainDirty = true;
            }

            // Low HP (setHealth): under lowHpAt the vignette darkens and the grade desaturates.
            double share = playing && game.health.max > 0 ? game.health.hp / game.health.max : 1;
            // NIGHT72 calm72 (design.n72.calm72, read every frame): the low-HP vignette starts under UnityTuning.L72CalmEdgeBelow (30 %), not post.lowHpAt (45 %)
            double lowAt = NightRows.ViewOn(game.cfg, "calm72") ? System.Math.Min(lowHpAt, UnityTuning.L72CalmEdgeBelow) : lowHpAt;
            double hurt = System.Math.Max(0, System.Math.Min(1, (lowAt - share) / lowAt));
            double h = hurtVignette ? hurt : 0;
            if (beatAge < double.MaxValue) beatAge += frameDt; // NIGHT16 polish #9: the round-clear beat (frame time)
            double beat = playing ? BeatShare() : 0;
            // NIGHT18 U3-31.9 backlog 4 (FeelAB on): the low-HP vignette beats (CrunchMath.HeartPulse, quickening as HP falls),
            // x FLASH INTENSITY, steady with REDUCED MOTION; its colour leans toward DANGER with the hurt share. Off: NIGHT17's
            // steady black vignette (both numbers 0).
            bool feelOn = FeelAB.On;
            float pulseAmp = UnityTuning.Pick(feelOn, UnityTuning.LowHpPulseAmp, UnityTuning.LowHpPulseAmpN17);
            float pulseAdd = 0;
            if (h > 0 && pulseAmp > 0 && !st.reducedMotion)
            {
                lowHpPhase = (lowHpPhase + frameDt * CrunchMath.HeartRate(h, UnityTuning.LowHpPulseRestHz, UnityTuning.LowHpPulseLowHz)) % 1;
                double fi = game.settings.View.flashIntensity;
                fi = fi < 0 ? 0 : fi > 1 ? 1 : fi;
                pulseAdd = pulseAmp * (float)(h * CrunchMath.HeartPulse(lowHpPhase) * fi);
                pulseAdd = Mathf.Round(pulseAdd / UnityTuning.LowHpPulseStep) * UnityTuning.LowHpPulseStep; // written in steps (the volume refreshes as often as it shows)
            }
            else lowHpPhase = 0;
            LowHpPulseNow = pulseAmp > 0 ? pulseAdd / pulseAmp : 0;
            float tint = UnityTuning.Pick(feelOn, UnityTuning.LowHpPulseTint, UnityTuning.LowHpPulseTintN17) * (float)h;
            bool edge31 = LowHealthEdge31.Active(game.cfg); // NIGHT31 G (P0 d, design.n31.lowHealthEdge): the low-HP vignette stays at the edges
            if (h != hurtShown || beat != beatShown || pulseAdd != pulseShown || tint != tintShown || edge31 != edgeShown31)
            {
                hurtShown = h;
                beatShown = beat;
                pulseShown = pulseAdd;
                edgeShown31 = edge31;
                float k = (float)h;
                float vi = Mathf.Min(1, Mathf.Lerp(vigRestI, vigHurtI, k) + (float)(UnityTuning.BeatVignetteBoost * beat) + pulseAdd);
                post.vignette.intensity.value = edge31 ? Mathf.Min(vi, Mathf.Max(vigRestI, UnityTuning.Edge31PostMaxIntensity)) : vi; // NIGHT31: capped (never under the rest vignette)
                post.vignette.smoothness.value = edge31 ? Mathf.Min(Mathf.Lerp(vigRestS, vigHurtS, k), UnityTuning.Edge31PostSmoothness) : Mathf.Lerp(vigRestS, vigHurtS, k); // NIGHT31: a tighter falloff
                post.color.saturation.value = (float)((saturation * (1 - (edge31 ? 0 : UnityTuning.GradeHurtDesaturate) * h) - 1) * 100); // NIGHT31: no whole-screen desaturation with the edge row
                if (tint != tintShown)
                {
                    tintShown = tint;
                    if (!post.vignette.color.overrideState) post.vignette.color.overrideState = true;
                    post.vignette.color.value = Color.Lerp(Color.black, dangerColor, look33 ? 0f : tint); // DANGER: danger only (rule 8); NIGHT33 L: look33 keeps the full-frame vignette black, the red is the HUD edge frame's
                }
                mainDirty = true;
            }

            // NIGHT72 fix72 (design.n72.fix72, read every frame; Fix72.cs has the reasons) and the flicker meter's diagnostic (-skygrindFlickerOff72): the film grain
            // draws a new random offset every frame and the CRT pass then quantises it to 32 levels, so it flips whole colour steps: it is off while the quantise is
            // live; the bloom takes High Quality Filtering. The preset's own values stay as ApplyPreset wrote them (intensity, bloom intensity): only the switches move.
            bool fix72 = NightRows.ViewOn(game.cfg, Fix72.Row);
            bool grainWant = post.grain.intensity.value > 0f && !Fix72.Off("grain") && !(fix72 && Crt29.QuantStrength > 0f);
            if (post.grain.active != grainWant) { post.grain.active = grainWant; mainDirty = true; }
            bool bloomWant = bloomOn && !Fix72.Off("bloom");
            if (post.bloom.active != bloomWant) { post.bloom.active = bloomWant; mainDirty = true; }
            if (fix72 != hqBloomShown72)
            {
                hqBloomShown72 = fix72;
                if (fix72) post.bloom.highQualityFiltering.Override(true);
                else { post.bloom.highQualityFiltering.overrideState = false; post.bloom.highQualityFiltering.value = false; }
                mainDirty = true;
            }

            // setEnergy (main.js updateRankWorld): the rank's energy, eased on frame time, lifts the bloom.
            StyleMeter style = game.style;
            bool on = style.enabled && flow != Flow.Title;
            double target = on ? StyleMath.RankEnergy(style.rank, style.Fill, style.Params) : 0;
            energy += (target - energy) * (1 - System.Math.Exp(-energyEase * frameDt));
            if (bloomOn)
            {
                float bloom = (float)(bloomBase * (1 + bloomEnergy * energy)) * (Wick37View.Current != null ? Wick37View.Bloom54 : 1f); // NIGHT54 light54: THE STACK's bloom trim (1 with the row off)
                if (!scripted) post.bloom.intensity.value = bloom;
                else if (!bloom.Equals(bloomShown))
                {
                    bloomShown = bloom;
                    post.bloom.intensity.value = bloom;
                    mainDirty = true;
                }
            }
            if (bend34 != null && bend34.Render(frameDt)) mainDirty = true; // NIGHT34 H (design.n34.bendLook): the bend's grade and sky dim (Bend34View.cs)
            if (scripted) UpdateStacks();
            RenderImpactFrame(); // NIGHT17 U3-30.5
        }

        /// <summary>
        /// NIGHT13 U3-26.4: the Volume framework Via Scripting. Once VolumeManager exists, both cameras switch (URP updates
        /// each stack as it switches); after that a camera's stack is updated only on a frame where its values changed.
        /// </summary>
        void UpdateStacks()
        {
            if (!VolumeManager.instance.isInitialized) return; // before the first render: URP still updates every frame
            Camera main = game.cam;
            if (!scriptedSet)
            {
                scriptedSet = true;
                main.SetVolumeFrameworkUpdateMode(VolumeFrameworkUpdateMode.ViaScripting);
                if (overlay != null) overlay.SetVolumeFrameworkUpdateMode(VolumeFrameworkUpdateMode.ViaScripting);
                mainDirty = overlayDirty = true; // the first update below (the switch may not have updated a stack)
            }
            if (mainDirty)
            {
                mainDirty = false;
                Refresh(main, camData);
            }
            if (overlayDirty)
            {
                overlayDirty = false;
                if (overlay != null) Refresh(overlay, overlayData);
            }
        }

        /// <summary>A camera's own stack recomputed now (a stack left invalid by a pipeline rebuild is made anew first).</summary>
        static void Refresh(Camera c, UniversalAdditionalCameraData data)
        {
            if (data.volumeStack != null && !data.volumeStack.isValid) c.DestroyVolumeStack(data);
            c.UpdateVolumeStack(data);
        }
    }
}
