// NIGHT54 builder G: design.n54.grade54 (a view row; NightRows.ViewOn(cfg, "grade54"); -skygrindOff54 or under turns it off). GG 2026-10-20
// (NIGHT54.md): "each boss should have its own distinct color palette and lighting identity so the arena shifts in tone and atmosphere during
// the fight, similar to how JoJo's Bizarre Adventure uses dramatic color changes ... the guns and effects during boss fights should follow
// proper color theory".
// - THE GRADE: a second global URP Volume built in code (as Look.BuildPost builds the main one), priority numbers.common.priority, weight
//   blended in while a boss bar is live (BossRead41: the bar's own read of the fight, plus RIVAL PRIME's body) and out after. Per boss and
//   phase (numbers.<boss>.p1..p3): ColorAdjustments' colour filter, SplitToning's shadows / highlights / balance (the duotone), and saturation /
//   contrast / post-exposure as DELTAS on PostFx's live values (this volume writes main + delta each frame, so URP's lerp by the weight gives
//   main + weight x delta: PostFx's pulses, its low-HP drain and its theme EV keep working under the grade). No hue rotation (rule 8: the
//   player's cyan and the danger red-orange keep their hues).
// - THE LIGHT: the fog colour lerps to the phase's fog, the ambient (sky / equator / ground) and the key light (RenderSettings.sun) are
//   multiplied by the phase's tints, each written as a layer after the other writers (Look, DuskClock22, Boss29View, RoofView27, builder L's
//   lights): a colour another writer changed since this layer's last write is taken as the new base, so nothing compounds and their logic is
//   untouched; the base is put back when the grade leaves.
// - THE TURN (the JoJo shift): at the fight's start, at each phase change and (at numbers.common.openScale) when a stagger window opens, one
//   smooth envelope of numbers.common.turnS: at its peak the duotone's colours swap, the colour filter washes to the next phase's wash and the
//   frame darkens (EV < 0, never brighter); the phase's palette settles across it. At most one turn a numbers.common.turnGapS (1 s): far under
//   the 3 flashes a second photosensitivity limit.
// - THE GUNS: Rim(c) (a hook at each player-effect colour site: Fx49's rim ring and trace body, GunFx.Hits43's slug sheath) returns the boss's
//   rim colour for a player-cyan input during a graded fight (one fixed colour a boss: the effect sheets key their materials by colour), divided
//   by the colour filter so it reads as cyan after the grade; Core() is the tracer cores' gain (WeaponFx's laser and bounce line). A colour
//   that is not cyan (white-hot, the danger red-orange, magenta) passes unchanged. Row off or no fight: every call returns its input.
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    /// <summary>One phase's grade and light (design.n54.grade54.numbers.&lt;boss&gt;.pN).</summary>
    public sealed class Grade54Phase
    {
        public Color filter = Color.white, shadows = Color.grey, highlights = Color.grey, fog = Color.black, ambient = Color.white, key = Color.white, wash = Color.white;
        public float balance, sat, contrast, ev, fogMix;
    }

    /// <summary>A boss's palette: three phases, the player's rim colour against it.</summary>
    public sealed class Grade54Boss
    {
        public string id;
        public readonly Grade54Phase[] p = new Grade54Phase[3];
        public Color rim = Color.cyan, rimHdr = Color.cyan;
        public float rimGain = 1, turnSat;
    }

    [DefaultExecutionOrder(10000)] // after PostFx, the lights' owners and the boss views: this layer reads what they wrote this frame
    public sealed class BossGrade54 : MonoBehaviour
    {
        /// <summary>GG 2026-10-26: this grade's volume weight as last written (0 out of a fight); Bright71 lifts its floors by it.</summary>
        public static float WeightNow;
        const string Row = "design.n54.grade54.numbers.";
        public static readonly string[] Bosses = { "wick", "brawl40", "wyrm", "loom37", "crown37", "rival" };

        /// <summary>The reels' override of the row (null: the config and the command line).</summary>
        public static bool? Force;
        /// <summary>a254: the grade held at weight 0 this frame (the ungraded half of the A/B frame pair; the fight's state is untouched).</summary>
        public static bool Suppress;
        /// <summary>a254: added to the read phase (a forced turn for a boss whose phase cannot be pushed by damage; the same path as a real change).</summary>
        public static int ProofBump;

        // --- the probe (a254 prints these) ---
        public static bool Live { get; private set; }
        public static string Boss { get; private set; }
        public static int Phase { get; private set; }
        public static float Weight { get; private set; }
        public static int Turns { get; private set; }
        public static string LastTurn { get; private set; } = "";
        public static float TurnEnvelope { get; private set; }
        public static BossGrade54 Current { get; private set; }

        SurvivalGame game;
        GameConfig readFor;
        bool on, offSwitch;
        Volume volume;
        VolumeProfile profile;
        ColorAdjustments color;
        SplitToning split;
        Grade54Boss pal;
        readonly Grade54Phase blend = new Grade54Phase();
        Grade54Phase from, to;
        float w, turnT = -1, turnScale, lastTurnAt = -999, settleT = 1;
        bool wasOpen, startPending;
        Bar41 bar;
        // the common numbers
        float priority, fadeIn, fadeOut, turnS, turnPeak, turnGap, turnSat, turnContrast, turnEv, turnFlip, washMix, intensity, evGain, openScale, rimMinFilter, coreGain;
        readonly System.Collections.Generic.Dictionary<string, Grade54Boss> bosses = new System.Collections.Generic.Dictionary<string, Grade54Boss>();

        struct Layer
        {
            public Color baseC, written;
            public bool has;

            /// <summary>The colour to write over `now`: the base (re-taken when another writer changed it since this layer's last write)
            /// x `mul`, then lerped to `to` by `t`.</summary>
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

        /// <summary>Added once at boot (SurvivalGame.StartGame): the layer exists; it builds its volume the first time a graded fight starts.</summary>
        public static void Attach(SurvivalGame g)
        {
            if (g == null || g.GetComponent<BossGrade54>() != null) return;
            BossGrade54 b = g.gameObject.AddComponent<BossGrade54>();
            b.game = g;
            Current = b;
        }

        /// <summary>grade54 draws: its row on and no -skygrindOffNN at or under 54 (cached per config).</summary>
        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (Current == null || cfg == null) return false;
            return Current.Read(cfg);
        }

        bool Read(GameConfig cfg)
        {
            if (ReferenceEquals(cfg, readFor)) return on;
            readFor = cfg;
            offSwitch = NightRows.OffFrom(Environment.GetCommandLineArgs(), "-skygrindOff") <= 54;
            on = NightRows.ViewOn(cfg, "grade54") && !offSwitch;
            bosses.Clear();
            priority = N(cfg, "common.priority", 50);
            fadeIn = Mathf.Max(0.05f, N(cfg, "common.fadeInS", 0.8f));
            fadeOut = Mathf.Max(0.05f, N(cfg, "common.fadeOutS", 1.6f));
            turnS = Mathf.Clamp(N(cfg, "common.turnS", 0.55f), 0.1f, 2f);
            turnPeak = Mathf.Clamp(N(cfg, "common.turnPeak", 0.3f), 0.05f, 0.95f);
            turnGap = Mathf.Max(0.34f, N(cfg, "common.turnGapS", 1f)); // never under 0.34 s: at most 3 turns a second whatever the config says
            turnSat = N(cfg, "common.turnSat", 35);
            turnContrast = N(cfg, "common.turnContrast", 25);
            turnEv = Mathf.Min(0, N(cfg, "common.turnEv", -0.3f)); // a turn never brightens
            turnFlip = Mathf.Clamp01(N(cfg, "common.turnFlip", 1));
            washMix = Mathf.Clamp01(N(cfg, "common.washMix", 0.7f));
            intensity = Mathf.Clamp(N(cfg, "common.intensity", 1f), 0f, 3f);
            evGain = Mathf.Clamp(N(cfg, "common.evGain", 1f), 0f, 3f);
            openScale = Mathf.Clamp01(N(cfg, "common.openScale", 0.5f));
            rimMinFilter = Mathf.Clamp(N(cfg, "common.rimMinFilter", 0.3f), 0.05f, 1f);
            coreGain = Mathf.Max(1, N(cfg, "common.coreGain", 1.25f));
            if (on)
                foreach (string id in Bosses)
                {
                    Grade54Boss b = ReadBoss(cfg, id);
                    if (b != null) bosses[id] = b;
                }
            return on;
        }

        static float N(GameConfig cfg, string k, float missing)
        {
            string p = Row + k;
            if (!cfg.Has(p)) return missing;
            try { return (float)cfg.Num(p); }
            catch (Exception) { return missing; }
        }

        static bool C(GameConfig cfg, string k, ref Color c)
        {
            string p = Row + k;
            if (!cfg.Has(p)) return false;
            try
            {
                double[] v = cfg.Nums(p);
                if (v.Length < 3) return false;
                c = new Color((float)v[0], (float)v[1], (float)v[2], 1);
                return true;
            }
            catch (Exception) { return false; }
        }

        Grade54Boss ReadBoss(GameConfig cfg, string id)
        {
            if (!cfg.Has(Row + id + ".p1.filter")) return null;
            if (N(cfg, id + ".on", 1) == 0) return null; // the lead 15:54: a boss's grade off by one number (default on): no grade, no turns, no rim for it
            var b = new Grade54Boss { id = id };
            b.turnSat = N(cfg, id + ".turnSat", turnSat); // a boss's own turn saturation (default common.turnSat)
            for (int i = 0; i < 3; i++)
            {
                string k = id + ".p" + (i + 1) + ".";
                if (!cfg.Has(Row + k + "filter")) { b.p[i] = b.p[Math.Max(0, i - 1)]; continue; }
                var ph = new Grade54Phase();
                C(cfg, k + "filter", ref ph.filter);
                C(cfg, k + "shadows", ref ph.shadows);
                C(cfg, k + "highlights", ref ph.highlights);
                C(cfg, k + "fog", ref ph.fog);
                C(cfg, k + "ambient", ref ph.ambient);
                C(cfg, k + "key", ref ph.key);
                ph.wash = ph.filter;
                C(cfg, k + "wash", ref ph.wash);
                ph.balance = N(cfg, k + "balance", 0);
                ph.sat = N(cfg, k + "sat", 0);
                ph.contrast = N(cfg, k + "contrast", 0);
                ph.ev = Mathf.Min(0, N(cfg, k + "ev", 0)); // never above 0 (the lead 14:45: no brighter)
                ph.fogMix = Mathf.Clamp01(N(cfg, k + "fogMix", 0));
                b.p[i] = ph;
            }
            C(cfg, id + ".rim", ref b.rim);
            b.rimGain = N(cfg, id + ".rimGain", 2);
            // the rim pre-compensated by P1's colour filter (URP applies the filter in linear: divide by its linear value), so it reads as the rim after the grade
            Color f = Push(b.p[0].filter, Color.white, intensity).linear;
            b.rimHdr = new Color(b.rim.r / Mathf.Max(rimMinFilter, f.r), b.rim.g / Mathf.Max(rimMinFilter, f.g), b.rim.b / Mathf.Max(rimMinFilter, f.b), 1) * b.rimGain;
            return b;
        }

        // --- the hooks the effect sites call ---

        /// <summary>A player effect's colour in a graded fight: the boss's rim for a player-cyan input (hue 160-215, saturated), else `c`.</summary>
        public static Color Rim(Color c)
        {
            BossGrade54 g = Current;
            if (g == null || !g.on || g.pal == null || Weight < 0.5f || Suppress) return c;
            Color.RGBToHSV(c, out float h, out float s, out _);
            if (s < 0.3f || h < 160f / 360f || h > 215f / 360f) return c; // not the player's cyan: white-hot, danger, magenta pass unchanged
            return g.pal.rimHdr;
        }

        /// <summary>NIGHT63 B3c (bossColours63, builder D): Rim at the colour's own brightness: a player-cyan input (as Rim's test) during a graded
        /// fight becomes the boss's rim (its hue, pre-divided by P1's colour filter) scaled so its brightest channel sits where the input's did
        /// (rimHdr / rimGain x the input's brightest channel); anything else, or no graded fight, passes unchanged. A boss's weak point drawn in the
        /// player's cyan reads as the player's cyan after THE WYRM's grade (its filter cuts blue to 0.78), as the player's own effects do.</summary>
        public static Color RimAt(Color c)
        {
            BossGrade54 g = Current;
            if (g == null || !g.on || g.pal == null || Weight < 0.5f || Suppress) return c;
            Color.RGBToHSV(c, out float h, out float s, out _);
            if (s < 0.3f || h < 160f / 360f || h > 215f / 360f) return c; // not the player's cyan: passes unchanged
            float m = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            float k = m / Mathf.Max(1e-4f, g.pal.rimGain);
            return new Color(g.pal.rimHdr.r * k, g.pal.rimHdr.g * k, g.pal.rimHdr.b * k, c.a);
        }

        /// <summary>NIGHT64 B3 (bossColours63's weak aim, builder D): the graded fight's boss id where RimAt recolours now (RimAt's own gate: a
        /// graded fight at half weight or more, not suppressed), else null. Reads the grade; changes nothing (grade54's numbers are GG's).</summary>
        public static string RimBoss()
        {
            BossGrade54 g = Current;
            return g == null || !g.on || g.pal == null || Weight < 0.5f || Suppress ? null : g.pal.id;
        }

        /// <summary>The player's tracer cores' gain in a graded fight (1 otherwise).</summary>
        public static float Core()
        {
            BossGrade54 g = Current;
            if (g == null || !g.on || g.pal == null || Suppress) return 1f;
            return 1f + (g.coreGain - 1f) * Weight;
        }

        // --- the read of the fight ---

        void ReadFight(out string id, out int phase, out bool open)
        {
            id = null;
            phase = 1;
            open = false;
            SurvivalSim s = game.sim;
            if (s == null) return;
            // each boss read from its own state (the lead's n54-r2 a254: THE WICK's campaign fight shows no bar41 threat, so the bar's read
            // alone missed it); the bar's read stays the gate for the kit bosses (THE WYRM) beside their own state
            RivalPrime rv = s.rival25;
            bool rival = rv != null && rv.Alive && rv.phase != "dead" && rv.phase != "intro";
            Brawl40 br = s.brawl40;
            if (s.wick37 != null && s.wick37.Alive && Fighting(s.wick37.brain)) { id = "wick"; phase = s.wick37.brain.leg; open = s.wick37.brain.Open; }
            else if (s.loom37 != null && s.loom37.Alive && Fighting(s.loom37.brain)) { id = "loom37"; phase = s.loom37.brain.leg; open = s.loom37.brain.Open; }
            else if (s.crown37 != null && s.crown37.Alive && Fighting(s.crown37.brain)) { id = "crown37"; phase = s.crown37.brain.leg; open = s.crown37.brain.Open; }
            else if (br != null && br.Alive && br.state != Brawl40State.Off && br.state != Brawl40State.Dead) { id = "brawl40"; phase = br.phase; open = br.Open; }
            else if (s.wyrm24 != null && s.wyrm24.Alive && s.wyrm24.phase != "dead" && Bar(ref bar))
            {
                id = "wyrm";
                string rp = s.wyrm24.RetryPhase; // the beat counts as its next phase (the PHASE banner is the big moment)
                phase = rp == "p2" ? 2 : rp == "p3" || rp == "p4" ? 3 : 1;
            }
            else if (rival)
            {
                id = "rival";
                phase = rv.RetryPhase == "p2" ? 2 : 1;
            }
            if (id == null) return;
            phase = Math.Max(1, Math.Min(3, phase + ProofBump));
        }

        static bool Fighting(Boss37Brain b) => b != null && b.state != Boss37State.Off && b.state != Boss37State.Dead;

        bool Bar(ref Bar41 b)
        {
            BossRead41.Read(game, ref b);
            return b.valid;
        }

        void Build()
        {
            if (volume != null) return;
            var go = new GameObject("Boss grade (n54 grade54)");
            go.transform.SetParent(transform, false);
            volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = priority;
            volume.weight = 0; WeightNow = 0;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            color = profile.Add<ColorAdjustments>(true);
            color.colorFilter.Override(Color.white);
            color.saturation.Override(0f);
            color.contrast.Override(0f);
            color.postExposure.Override(0f);
            split = profile.Add<SplitToning>(true);
            split.shadows.Override(Color.grey);
            split.highlights.Override(Color.grey);
            split.balance.Override(0f);
            volume.sharedProfile = profile;
        }

        void StartTurn(string kind, float scale, float now)
        {
            if (now - lastTurnAt < turnGap) return; // one turn a gap: the event is dropped (the palette still settles)
            lastTurnAt = now;
            turnT = 0;
            turnScale = game.cfg.movement.settings.reducedMotion ? 0f : scale; // REDUCED MOTION: no turn envelope (the palette still settles to the phase)
            Turns++;
            LastTurn = kind;
        }

        void LateUpdate()
        {
            if (game == null || game.cfg == null) return;
            if (!On(game.cfg)) { if (volume != null || lightsHeld) Leave(true); return; }
            float dt = Mathf.Min(FeelClock.UnscaledDelta, 0.1f), now = FeelClock.UnscaledTime; // NIGHT63 D1: held in a same-run held frame
            ReadFight(out string id, out int phase, out bool open);
            Grade54Boss b = id != null && bosses.TryGetValue(id, out Grade54Boss bb) ? bb : null;
            bool live = b != null;
            if (live && (pal == null || pal.id != b.id)) // a new fight
            {
                Build();
                pal = b;
                from = to = b.p[phase - 1];
                settleT = 1;
                Phase = phase;
                wasOpen = open;
                lastTurnAt = -999;
                startPending = true; // the start turn waits for the grade's weight to reach 0.5 (a turn under a weight near 0 would not show)
            }
            if (live && startPending && w >= 0.5f) { startPending = false; StartTurn("start", 1f, now); }
            if (live && phase != Phase) // a phase change: the palette settles from where it is to the next phase across the turn
            {
                var snap = new Grade54Phase(); // where the palette is now (mid-settle too): the next settle starts from it, no jump
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
            if (pal == null) return;
            if (!live && w <= 0) { Leave(false); return; }

            settleT = Mathf.Min(1, settleT + dt / turnS);
            float e = 0;
            if (turnT >= 0)
            {
                turnT += dt;
                float u = turnT / turnS, pk = turnPeak;
                e = u >= 1 ? 0 : u < pk ? Smooth(u / pk) : 1 - Smooth((u - pk) / (1 - pk));
                if (u >= 1) turnT = -1;
            }
            e *= turnScale;
            TurnEnvelope = e;
            Lerp(blend, from ?? to, to, Smooth(settleT));
            float ew = Suppress ? 0 : w;

            // the grade: the duotone (flipped by the turn), the filter (washed by the turn), the deltas on PostFx's live values
            PostStack main = Look.Post;
            float sat0 = main != null && main.color.saturation.overrideState ? main.color.saturation.value : 0f;
            float con0 = main != null && main.color.contrast.overrideState ? main.color.contrast.value : 0f;
            float ev0 = main != null && main.color.postExposure.overrideState ? main.color.postExposure.value : 0f;
            float flip = e * turnFlip;
            // common.intensity scales every colour's distance from neutral (white filter, 0.5 grey split) and the sat / contrast deltas;
            // common.evGain the EV delta (the lead's n54-r2: graded vs raw moved 3-4 deg / 0.01 V: aim >= 25 deg or >= 0.08 V)
            Color sh = Push(blend.shadows, Color.grey, intensity), hi = Push(blend.highlights, Color.grey, intensity);
            split.shadows.value = Color.Lerp(sh, hi, flip);
            split.highlights.value = Color.Lerp(hi, sh, flip);
            split.balance.value = blend.balance;
            color.colorFilter.value = Color.Lerp(Push(blend.filter, Color.white, intensity), Push(to.wash, Color.white, intensity), e * washMix);
            color.saturation.value = Mathf.Clamp(sat0 + blend.sat * intensity + e * pal.turnSat, -100, 100);
            color.contrast.value = Mathf.Clamp(con0 + blend.contrast * intensity + e * turnContrast, -100, 100);
            color.postExposure.value = ev0 + blend.ev * evGain * (Bright71.On ? UnityTuning.Bright71BossEvScale : 1f) + e * turnEv; // GG 2026-10-26: the boss rooms too dark: no exposure cut with Bright71 on
            volume.priority = priority;
            volume.weight = ew; WeightNow = ew;
            RefreshStacks(); // the cameras' Volume stacks are Via Scripting (PostFx.UpdateStacks: updated only on a change): this grade changes each frame

            // the light: fog, ambient, the key, as a layer over whoever wrote them
            Color fogT = blend.fog, ambT = Color.Lerp(Color.white, blend.ambient, ew), keyT = Color.Lerp(Color.white, blend.key, ew);
            float fm = blend.fogMix * ew;
            RenderSettings.fogColor = fogL.Over(RenderSettings.fogColor, Color.white, fogT, fm);
            RenderSettings.ambientSkyColor = skyL.Over(RenderSettings.ambientSkyColor, ambT, Color.black, 0);
            RenderSettings.ambientEquatorColor = eqL.Over(RenderSettings.ambientEquatorColor, ambT, Color.black, 0);
            RenderSettings.ambientGroundColor = groundL.Over(RenderSettings.ambientGroundColor, ambT, Color.black, 0);
            Light sun = RenderSettings.sun;
            if (sun != null) sun.color = keyL.Over(sun.color, keyT, Color.black, 0);
            lightsHeld = true;
        }

        /// <summary>The grade gone: the volume at weight 0, the lights' bases put back (once), the fight forgotten.</summary>
        void Leave(bool rowOff)
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
        }

        Camera stackCam, stackOverlay;
        UniversalAdditionalCameraData stackCamData, stackOverlayData;

        /// <summary>The main camera's and the viewmodel overlay's Volume stacks recomputed now (as PostFx.Refresh / SlowAim27 do).</summary>
        void RefreshStacks()
        {
            if (!VolumeManager.instance.isInitialized) return; // before the first render URP updates every frame itself
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

        /// <summary>`c` pushed away from `neutral` by `k` (1: as given), each channel kept in 0..1.</summary>
        static Color Push(Color c, Color neutral, float k) =>
            new Color(Mathf.Clamp01(neutral.r + (c.r - neutral.r) * k), Mathf.Clamp01(neutral.g + (c.g - neutral.g) * k), Mathf.Clamp01(neutral.b + (c.b - neutral.b) * k), 1);

        static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3 - 2 * x);
        }

        static void Copy(Grade54Phase d, Grade54Phase s)
        {
            if (d == null || s == null) return;
            d.filter = s.filter; d.shadows = s.shadows; d.highlights = s.highlights; d.fog = s.fog; d.ambient = s.ambient; d.key = s.key; d.wash = s.wash;
            d.balance = s.balance; d.sat = s.sat; d.contrast = s.contrast; d.ev = s.ev; d.fogMix = s.fogMix;
        }

        static void Lerp(Grade54Phase d, Grade54Phase a, Grade54Phase b, float t)
        {
            d.filter = Color.Lerp(a.filter, b.filter, t); d.shadows = Color.Lerp(a.shadows, b.shadows, t); d.highlights = Color.Lerp(a.highlights, b.highlights, t);
            d.fog = Color.Lerp(a.fog, b.fog, t); d.ambient = Color.Lerp(a.ambient, b.ambient, t); d.key = Color.Lerp(a.key, b.key, t); d.wash = b.wash;
            d.balance = Mathf.Lerp(a.balance, b.balance, t); d.sat = Mathf.Lerp(a.sat, b.sat, t); d.contrast = Mathf.Lerp(a.contrast, b.contrast, t);
            d.ev = Mathf.Lerp(a.ev, b.ev, t); d.fogMix = Mathf.Lerp(a.fogMix, b.fogMix, t);
        }

        // --- a254's colour measures ---

        /// <summary>Relative luminance (WCAG, from linear RGB).</summary>
        public static double Luminance(Color lin) => 0.2126 * lin.r + 0.7152 * lin.g + 0.0722 * lin.b;

        /// <summary>WCAG contrast ratio of two linear colours.</summary>
        public static double ContrastRatio(Color a, Color b)
        {
            double la = Luminance(a), lb = Luminance(b);
            return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
        }

        /// <summary>The hue distance in degrees (0..180).</summary>
        public static double HueDist(double h1, double h2)
        {
            double d = Math.Abs(h1 - h2) % 360;
            return d > 180 ? 360 - d : d;
        }

        /// <summary>The palette a boss id reads (null: none in the numbers, or the row off).</summary>
        public static Grade54Boss PaletteOf(string id) => Current != null && id != null && Current.bosses.TryGetValue(id, out Grade54Boss b) ? b : null;
    }
}
