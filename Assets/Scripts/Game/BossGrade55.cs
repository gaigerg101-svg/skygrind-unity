// NIGHT55 builder B: design.n55.grade55 (a view row; NightRows.ViewOn(cfg, "grade55"); -skygrindOff55 or under turns it off). THE WICK's and THE
// CROWN's palettes and JoJo turns without hiding the danger red (OPEN O-01 / O-02; DECIDE54 D-16's next try; Research/2026-10-20-wave55 web.md
// item 1, github.md item 3). NIGHT54's grade54 tinted the WHOLE frame (a warm colour filter): every try put the frame's mean hue 1-14 deg from the
// danger red-orange and both bosses shipped ungraded. This grade never touches the bright end of the frame:
// - THE SHADOWS: a second code-built URP Volume (priority numbers.common.priority, over grade54's) with SplitToning only in the shadows: a strongly
//   cool shadow colour (indigo / violet / steel per boss and phase), the highlights at neutral 0.5 grey and a positive balance, so the tint's weight
//   (1 - luma - balance) falls to ~0 on emissive tells, the core, the embers and the player's cyan (URP LutBuilderLdr: SoftLight by luma). No
//   colour filter, no saturation change, no hue shift: the red-orange tells keep their hue and saturation.
// - THE VALUE: post-exposure and contrast as DELTAS on PostFx's live values (main + weight x delta, as grade54 does): the arena darker, so the
//   emissives separate by value.
// - THE LIGHT: the fog lerps to the phase's fog, the ambient and the key light are multiplied by the phase's tints (a layer over the other writers,
//   grade54's method), and THE WICK's stack lights lerp to a cool light (Wick37View.N55.cs reads LightsTint / LightsMix): the walls lit cool, the slag
//   and the core keep their own hot light (V-5: the hazard lights the room).
// - THE TURN (the JoJo beat): at the fight's start, at each phase change and (x numbers.common.openScale) when a window opens, one smooth envelope of
//   turnS: at its peak the shadow colour swings to the phase's `turn` colour (a 40-60 deg swing inside the cool half of the wheel), the frame darkens
//   (EV < 0, never brighter) and contrast rises; the next phase's palette settles across it. At most one turn a turnGapS (>= 0.34 s): under the 3
//   flashes a second limit.
// Row off, or a boss without numbers: nothing is built or touched (NIGHT54's look: both bosses ungraded).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    /// <summary>One phase of a grade55 palette (design.n55.grade55.numbers.&lt;boss&gt;.pN).</summary>
    public sealed class Grade55Phase
    {
        public Color split = Color.grey, turn = Color.grey, fog = Color.black, ambient = Color.white, key = Color.white, lights = Color.white;
        public float balance, ev, contrast, fogMix, lightsMix;
        /// <summary>NIGHT56 V (V4 / r1 critic, THE CROWN): a saturation delta on PostFx's live value (never above 0: a grade never adds chroma; 0 = none)
        /// and the split's balance at a turn's peak (NaN = the phase's balance: the turn's cool reaches only the shadows, round 2's).</summary>
        public float sat, turnBalance = float.NaN;
    }

    /// <summary>A boss's grade55 palette: three phases.</summary>
    public sealed class Grade55Boss
    {
        public string id;
        public readonly Grade55Phase[] p = new Grade55Phase[3];
    }

    [DefaultExecutionOrder(10001)] // after grade54 (10000): this layer reads what PostFx, the lights' owners and grade54 wrote this frame
    public sealed class BossGrade55 : MonoBehaviour
    {
        /// <summary>GG 2026-10-26: this grade's volume weight as last written (0 out of a fight); Bright71 lifts its floors by it.</summary>
        public static float WeightNow;
        public const string Row = "design.n55.grade55.numbers.";
        public static readonly string[] Bosses = { "wick", "crown37", "wyrm" }; // NIGHT56 V (V4): THE WYRM (a palette only when numbers.wyrm.p1 exists)

        /// <summary>The reels' override of the row (null: the config and the command line).</summary>
        public static bool? Force;
        /// <summary>a155: the grade held at weight 0 this frame (the ungraded half of the A/B pair; the fight and the light layer's state untouched).</summary>
        public static bool Suppress;

        // --- the probe (a155 prints these) ---
        public static bool Live { get; private set; }
        public static string Boss { get; private set; }
        public static int Phase { get; private set; }
        public static float Weight { get; private set; }
        public static int Turns { get; private set; }
        public static string LastTurn { get; private set; } = "";
        public static float TurnEnvelope { get; private set; }
        public static BossGrade55 Current { get; private set; }
        /// <summary>THE WICK's stack lights: the cool colour they lerp to and by how much this frame (0: untouched). Read by Wick37View.N55.cs.</summary>
        public static Color LightsTint { get; private set; } = Color.white;
        public static float LightsMix { get; private set; }
        /// <summary>The live shadow colour and balance (a155 prints them).</summary>
        public static Color SplitNow { get; private set; } = Color.grey;
        public static float BalanceNow { get; private set; }

        SurvivalGame game;
        GameConfig readFor;
        bool on;
        Volume volume;
        VolumeProfile profile;
        ColorAdjustments color;
        SplitToning split;
        Grade55Boss pal;
        readonly Grade55Phase blend = new Grade55Phase();
        Grade55Phase from, to;
        float w, turnT = -1, turnScale, lastTurnAt = -999, settleT = 1;
        bool wasOpen, startPending;
        float priority, fadeIn, fadeOut, turnS, turnPeak, turnGap, turnEv, turnContrast, turnFlip, openScale;
        readonly System.Collections.Generic.Dictionary<string, Grade55Boss> bosses = new System.Collections.Generic.Dictionary<string, Grade55Boss>();

        struct Layer
        {
            public Color baseC, written;
            public bool has;

            public Color Over(Color now, Color mul, Color to, float t)
            {
                if (!has || !Near(now, written)) baseC = now;
                has = true;
                written = Color.Lerp(baseC * mul, to, t);
                written.a = baseC.a;
                return written;
            }

            public Color Restore(Color now)
            {
                Color c = has && Near(now, written) ? baseC : now;
                has = false;
                return c;
            }

            static bool Near(Color a, Color b) => Mathf.Abs(a.r - b.r) < 2e-3f && Mathf.Abs(a.g - b.g) < 2e-3f && Mathf.Abs(a.b - b.b) < 2e-3f;
        }

        Layer fogL, skyL, eqL, groundL, keyL;
        bool lightsHeld;

        /// <summary>Added once at boot (SurvivalGame.StartGame, beside grade54's): it builds its volume the first time a graded fight starts.</summary>
        public static void Attach(SurvivalGame g)
        {
            if (g == null || g.GetComponent<BossGrade55>() != null) return;
            BossGrade55 b = g.gameObject.AddComponent<BossGrade55>();
            b.game = g;
            Current = b;
        }

        /// <summary>grade55 draws: its row on and no -skygrindOffNN at or under 55 (cached per config).</summary>
        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (Current == null) return NightRows.ViewOn(cfg, "grade55");
            return Current.Read(cfg);
        }

        bool Read(GameConfig cfg)
        {
            if (ReferenceEquals(cfg, readFor)) return on;
            readFor = cfg;
            on = NightRows.ViewOn(cfg, "grade55");
            bosses.Clear();
            priority = N(cfg, "common.priority", 51);
            fadeIn = Mathf.Max(0.05f, N(cfg, "common.fadeInS", 0.8f));
            fadeOut = Mathf.Max(0.05f, N(cfg, "common.fadeOutS", 1.6f));
            turnS = Mathf.Clamp(N(cfg, "common.turnS", 0.55f), 0.1f, 2f);
            turnPeak = Mathf.Clamp(N(cfg, "common.turnPeak", 0.3f), 0.05f, 0.95f);
            turnGap = Mathf.Max(0.34f, N(cfg, "common.turnGapS", 1f)); // never under 0.34 s: at most 3 turns a second whatever the config says
            turnEv = Mathf.Min(0, N(cfg, "common.turnEv", -0.4f)); // a turn never brightens
            turnContrast = N(cfg, "common.turnContrast", 30);
            turnFlip = Mathf.Clamp01(N(cfg, "common.turnFlip", 1));
            openScale = Mathf.Clamp01(N(cfg, "common.openScale", 0.5f));
            if (on)
                foreach (string id in Bosses)
                {
                    Grade55Boss b = ReadBoss(cfg, id);
                    if (b != null) bosses[id] = b;
                }
            return on;
        }

        public static float N(GameConfig cfg, string k, float missing)
        {
            string p = Row + k;
            if (cfg == null || !cfg.Has(p)) return missing;
            try { return (float)cfg.Num(p); }
            catch (Exception) { return missing; }
        }

        public static bool C(GameConfig cfg, string k, ref Color c)
        {
            string p = Row + k;
            if (cfg == null || !cfg.Has(p)) return false;
            try
            {
                double[] v = cfg.Nums(p);
                if (v.Length < 3) return false;
                c = new Color((float)v[0], (float)v[1], (float)v[2], 1);
                return true;
            }
            catch (Exception) { return false; }
        }

        /// <summary>A boss's palette from the numbers (null: no p1, or its `on` 0).</summary>
        public static Grade55Boss ReadBoss(GameConfig cfg, string id)
        {
            if (cfg == null || !cfg.Has(Row + id + ".p1.split")) return null;
            if (N(cfg, id + ".on", 1) == 0) return null;
            var b = new Grade55Boss { id = id };
            for (int i = 0; i < 3; i++)
            {
                string k = id + ".p" + (i + 1) + ".";
                if (!cfg.Has(Row + k + "split")) { b.p[i] = b.p[Math.Max(0, i - 1)]; continue; }
                var ph = new Grade55Phase();
                C(cfg, k + "split", ref ph.split);
                ph.turn = ph.split;
                C(cfg, k + "turn", ref ph.turn);
                C(cfg, k + "fog", ref ph.fog);
                C(cfg, k + "ambient", ref ph.ambient);
                C(cfg, k + "key", ref ph.key);
                C(cfg, k + "lights", ref ph.lights);
                ph.balance = Mathf.Clamp(N(cfg, k + "balance", 0), -100, 100);
                ph.ev = Mathf.Min(0, N(cfg, k + "ev", 0)); // never brighter
                ph.contrast = N(cfg, k + "contrast", 0);
                ph.fogMix = Mathf.Clamp01(N(cfg, k + "fogMix", 0));
                ph.lightsMix = Mathf.Clamp01(N(cfg, k + "lightsMix", 0));
                ph.sat = Mathf.Clamp(N(cfg, k + "sat", 0), -100, 0); // NIGHT56 V: never more chroma (the tells keep the frame's top saturation)
                ph.turnBalance = cfg.Has(Row + k + "turnBalance") ? Mathf.Clamp(N(cfg, k + "turnBalance", 0), -100, 100) : float.NaN;
                b.p[i] = ph;
            }
            return b;
        }

        /// <summary>The palette a boss id reads (null: none in the numbers, or the row off).</summary>
        public static Grade55Boss PaletteOf(string id) => Current != null && id != null && Current.bosses.TryGetValue(id, out Grade55Boss b) ? b : null;

        // --- the read of the fight (THE WICK and THE CROWN only) ---

        void ReadFight(out string id, out int phase, out bool open)
        {
            id = null;
            phase = 1;
            open = false;
            SurvivalSim s = game.sim;
            if (s == null) return;
            if (s.wick37 != null && s.wick37.Alive && Fighting(s.wick37.brain)) { id = "wick"; phase = s.wick37.brain.leg; open = s.wick37.brain.Open; }
            else if (s.crown37 != null && s.crown37.Alive && Fighting(s.crown37.brain)) { id = "crown37"; phase = s.crown37.brain.leg; open = s.crown37.brain.Open; }
            else if (s.wyrm24 != null && s.wyrm24.Alive && s.wyrm24.phase != "dead" && s.wyrm24.phase != "dying" && s.wyrm24.phase != "intro" && PaletteOf("wyrm") != null)
            {
                // NIGHT56 V (V4): THE WYRM (grade54's read: the beat counts as its next phase); no palette in the numbers = not graded here (grade54's)
                id = "wyrm";
                string rp = s.wyrm24.RetryPhase;
                phase = rp == "p2" ? 2 : rp == "p3" || rp == "p4" ? 3 : 1;
            }
            phase = Math.Max(1, Math.Min(3, phase));
        }

        static bool Fighting(Boss37Brain b) => b != null && b.state != Boss37State.Off && b.state != Boss37State.Dead;

        void Build()
        {
            if (volume != null) return;
            var go = new GameObject("Boss grade (n55 grade55)");
            go.transform.SetParent(transform, false);
            volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = priority;
            volume.weight = 0; WeightNow = 0;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            color = profile.Add<ColorAdjustments>(false); // exposure, contrast and (NIGHT56 V) saturation as deltas on PostFx's live values; the filter stays Look's / PostFx's
            color.postExposure.Override(0f);
            color.contrast.Override(0f);
            color.saturation.Override(0f);
            split = profile.Add<SplitToning>(true);
            split.shadows.Override(Color.grey);
            split.highlights.Override(Color.grey); // neutral: the highlights (tells, the core, the cyan) are never toned
            split.balance.Override(0f);
            volume.sharedProfile = profile;
        }

        void StartTurn(string kind, float scale, float now)
        {
            if (now - lastTurnAt < turnGap) return;
            lastTurnAt = now;
            turnT = 0;
            turnScale = game.cfg.movement.settings.reducedMotion ? 0f : scale; // REDUCED MOTION: no turn envelope (the palette still settles)
            Turns++;
            LastTurn = kind;
        }

        void LateUpdate()
        {
            if (game == null || game.cfg == null) return;
            if (!On(game.cfg)) { if (volume != null || lightsHeld) Leave(); return; }
            float dt = Mathf.Min(FeelClock.UnscaledDelta, 0.1f), now = FeelClock.UnscaledTime; // NIGHT63 D1: held in a same-run held frame
            ReadFight(out string id, out int phase, out bool open);
            Grade55Boss b = id != null && bosses.TryGetValue(id, out Grade55Boss bb) ? bb : null;
            bool live = b != null;
            if (live && (pal == null || pal.id != b.id))
            {
                Build();
                pal = b;
                from = to = b.p[phase - 1];
                settleT = 1;
                Phase = phase;
                wasOpen = open;
                lastTurnAt = -999;
                startPending = true;
            }
            if (live && startPending && w >= 0.5f) { startPending = false; StartTurn("start", 1f, now); }
            if (live && phase != Phase)
            {
                var snap = new Grade55Phase();
                Copy(snap, blend);
                from = snap;
                to = b.p[phase - 1];
                settleT = 0;
                Phase = phase;
                StartTurn("phase " + phase, 1f, now);
            }
            if (live && open && !wasOpen) StartTurn("open", openScale, now);
            wasOpen = live && open;
            Live = live;
            Boss = pal != null ? pal.id : null;
            w = Mathf.MoveTowards(w, live ? 1f : 0f, dt / (live ? fadeIn : fadeOut));
            Weight = w;
            if (pal == null) { LightsMix = 0; return; }
            if (!live && w <= 0) { Leave(); return; }

            settleT = Mathf.Min(1, settleT + dt / turnS);
            float e = 0;
            if (turnT >= 0)
            {
                turnT += dt;
                float u = turnT / turnS;
                e = u >= 1 ? 0 : u < turnPeak ? Smooth(u / turnPeak) : 1 - Smooth((u - turnPeak) / (1 - turnPeak));
                if (u >= 1) turnT = -1;
            }
            e *= turnScale;
            TurnEnvelope = e;
            Lerp(blend, from ?? to, to, Smooth(settleT));
            float ew = Suppress ? 0 : w;

            PostStack main = Look.Post;
            float con0 = main != null && main.color.contrast.overrideState ? main.color.contrast.value : 0f;
            float ev0 = main != null && main.color.postExposure.overrideState ? main.color.postExposure.value : 0f;
            float sat0 = main != null && main.color.saturation.overrideState ? main.color.saturation.value : 0f;
            Color sh = Color.Lerp(blend.split, to.turn, e * turnFlip);
            split.shadows.value = sh;
            split.highlights.value = Color.grey;
            // NIGHT56 V: a phase with turnBalance swings the balance there at the turn's peak (THE CROWN: the cool reaches the fire-lit mid-tones)
            float bal = float.IsNaN(to.turnBalance) ? blend.balance : Mathf.Lerp(blend.balance, to.turnBalance, e);
            split.balance.value = bal;
            color.saturation.value = Mathf.Clamp(sat0 + blend.sat, -100, 100);
            color.contrast.value = Mathf.Clamp(con0 + blend.contrast + e * turnContrast, -100, 100);
            color.postExposure.value = ev0 + blend.ev * (Bright71.On ? UnityTuning.Bright71BossEvScale : 1f) + e * turnEv; // GG 2026-10-26: the boss rooms too dark: no exposure cut with Bright71 on
            volume.priority = priority;
            volume.weight = ew; WeightNow = ew;
            SplitNow = sh;
            BalanceNow = bal;
            RefreshStacks();

            // the light layer (grade54's method): fog, ambient, the key; THE WICK's stack lights through LightsTint / LightsMix
            Color ambT = Color.Lerp(Color.white, blend.ambient, ew), keyT = Color.Lerp(Color.white, blend.key, ew);
            RenderSettings.fogColor = fogL.Over(RenderSettings.fogColor, Color.white, blend.fog, blend.fogMix * ew);
            RenderSettings.ambientSkyColor = skyL.Over(RenderSettings.ambientSkyColor, ambT, Color.black, 0);
            RenderSettings.ambientEquatorColor = eqL.Over(RenderSettings.ambientEquatorColor, ambT, Color.black, 0);
            RenderSettings.ambientGroundColor = groundL.Over(RenderSettings.ambientGroundColor, ambT, Color.black, 0);
            Light sun = RenderSettings.sun;
            if (sun != null) sun.color = keyL.Over(sun.color, keyT, Color.black, 0);
            LightsTint = blend.lights;
            LightsMix = blend.lightsMix * ew;
            lightsHeld = true;
        }

        void Leave()
        {
            if (volume != null) { volume.weight = 0; WeightNow = 0; RefreshStacks(); }
            if (lightsHeld)
            {
                RenderSettings.fogColor = fogL.Restore(RenderSettings.fogColor);
                RenderSettings.ambientSkyColor = skyL.Restore(RenderSettings.ambientSkyColor);
                RenderSettings.ambientEquatorColor = eqL.Restore(RenderSettings.ambientEquatorColor);
                RenderSettings.ambientGroundColor = groundL.Restore(RenderSettings.ambientGroundColor);
                Light sun = RenderSettings.sun;
                if (sun != null) sun.color = keyL.Restore(sun.color);
                lightsHeld = false;
            }
            pal = null;
            from = to = null;
            turnT = -1;
            startPending = false;
            w = 0;
            Weight = 0;
            TurnEnvelope = 0;
            Live = false;
            Boss = null;
            Phase = 0;
            LightsMix = 0;
        }

        Camera stackCam, stackOverlay;
        UniversalAdditionalCameraData stackCamData, stackOverlayData;

        void RefreshStacks()
        {
            if (!VolumeManager.instance.isInitialized) return;
            Camera c = game.cam, o = game.viewmodel != null ? game.viewmodel.Camera : null;
            if (c != stackCam) { stackCam = c; stackCamData = c != null ? c.GetUniversalAdditionalCameraData() : null; }
            if (o != stackOverlay) { stackOverlay = o; stackOverlayData = o != null ? o.GetUniversalAdditionalCameraData() : null; }
            Refresh(stackCam, stackCamData);
            Refresh(stackOverlay, stackOverlayData);
        }

        static void Refresh(Camera c, UniversalAdditionalCameraData d)
        {
            if (c == null || d == null) return;
            if (d.volumeStack != null && !d.volumeStack.isValid) c.DestroyVolumeStack(d);
            c.UpdateVolumeStack(d);
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
            if (profile != null) Destroy(profile);
        }

        static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3 - 2 * x);
        }

        static void Copy(Grade55Phase d, Grade55Phase s)
        {
            if (d == null || s == null) return;
            d.split = s.split; d.turn = s.turn; d.fog = s.fog; d.ambient = s.ambient; d.key = s.key; d.lights = s.lights;
            d.balance = s.balance; d.ev = s.ev; d.contrast = s.contrast; d.fogMix = s.fogMix; d.lightsMix = s.lightsMix;
            d.sat = s.sat; d.turnBalance = s.turnBalance;
        }

        static void Lerp(Grade55Phase d, Grade55Phase a, Grade55Phase b, float t)
        {
            d.split = Color.Lerp(a.split, b.split, t); d.turn = b.turn; d.fog = Color.Lerp(a.fog, b.fog, t); d.ambient = Color.Lerp(a.ambient, b.ambient, t);
            d.key = Color.Lerp(a.key, b.key, t); d.lights = Color.Lerp(a.lights, b.lights, t);
            d.balance = Mathf.Lerp(a.balance, b.balance, t); d.ev = Mathf.Lerp(a.ev, b.ev, t); d.contrast = Mathf.Lerp(a.contrast, b.contrast, t);
            d.fogMix = Mathf.Lerp(a.fogMix, b.fogMix, t); d.lightsMix = Mathf.Lerp(a.lightsMix, b.lightsMix, t);
            d.sat = Mathf.Lerp(a.sat, b.sat, t); d.turnBalance = b.turnBalance;
        }
    }
}
