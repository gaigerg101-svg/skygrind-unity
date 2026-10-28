// NIGHT16 U3-29.7 (builder U): the feel night's small UI, in the partial Ui (Ui.cs calls BuildFeelHud() once and
// RenderFeelHud(frameDt) every frame, every flow, before the HUD's early return).
// - The movement HUD at the crosshair: one small tick per dash pip (game.mover.pips over MaxPips: dash.maxPips + VIGOR's
//   bonusPips; a partial pip fills) and, after a wider gap, the slide charge's longer tick (Mover.slideCharge, 0..1, 1 =
//   ready; builder M drives it, until then it stays 1). Cyan (colors.usable) on a faint track, under the crosshair. They
//   show while something is spent, hold 1 s after everything is full, then fade (FeelHudMath; FeelHudTests). Hidden off
//   the PLAYING flow. Images made once; a fill or the alpha is set only when its hundredths change.
//   The web's pip flash (hud.js :479-486): on the player's dash / dashJump (the mover's events) the pip it consumed
//   (floor(e.pips) after the dash) flashes white for hud.pipFlashTime; on dashFail every tick flashes red-orange
//   (colors.danger) for hud.failFlashTime. Port-only: a pip that refills (floor(pips) rises) and the slide charge's
//   return (FeelKeys.SlideCharged, owner "player") pulse their tick (scale 1 -> FeelHudPulseScale -> 1 over
//   FeelHudPulseTime, cyan). The ticks show while anything is spent, flashing or pulsing, and fade 1 s after all is full
//   and quiet. The mover's handler is one cached delegate (re-attached if the sim's mover changes).
// - The screens' transitions (0.15-0.25 s, UnityTuning.Transition*): RESULTS (DOWN -> RESULTS) and the MODS draft fade in
//   and rise; DOWN fades in; TITLE -> the run lifts a dusk curtain over the world (under the HUD). Looks only: a
//   CanvasGroup's alpha and a RectTransform offset; the screen under a transition is active, interactable and blocks
//   raycasts from its first frame, so a press during a transition acts at once. Unscaled frame time (frameDt is the
//   real frame's: menus run while the sim is paused).
// - The style rank plate's zero state (the audit's n15 tour: a run started with a big "D DULL" over an empty bar): the
//   letter, the name and the bar stay hidden until the run's first style award (or a non-empty meter, or a rank above
//   the lowest), then fade in over RankPlateFadeIn; once earned they stay for the run. Ui.cs's rank change applies
//   rankPlateAlpha to the colour it sets.
// PORT-RULES 6: nothing here allocates per frame (no strings; colours and fills set on change).
using System;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        // --- the movement HUD ---
        RectTransform feelHudRoot;
        CanvasGroup feelHudGroup;
        UnityEngine.UI.Image[] feelHudPipBack, feelHudPipFill;
        UnityEngine.UI.Image feelHudSlideBack, feelHudSlideFill;
        readonly int[] feelHudShownPip = new int[UnityTuning.FeelHudMaxPips];
        int feelHudPipCount = -1, feelHudShownSlide = -1, feelHudShownAlpha = -1;
        double feelHudFullFor = FeelHudMath.HiddenFor(UnityTuning.FeelHudHold, UnityTuning.FeelHudFade);
        readonly double[] feelHudPipFlash = new double[UnityTuning.FeelHudMaxPips]; // s of white left per pip
        readonly double[] feelHudPipPulse = new double[UnityTuning.FeelHudMaxPips]; // s of the refill pulse left per pip
        readonly int[] feelHudShownTint = new int[UnityTuning.FeelHudMaxPips]; // 0 cyan, 1 white, 2 danger
        readonly int[] feelHudShownScale = new int[UnityTuning.FeelHudMaxPips]; // hundredths
        double feelHudFailFlash, feelHudSlidePulse, feelHudPipFlashTime, feelHudFailFlashTime;
        int feelHudShownSlideTint = -1, feelHudShownSlideScale = -1, feelHudWhole = -1;
        Color feelHudTrack;
        Skygrind.Core.Mover feelHudMover; // the mover the handler is attached to
        Action<Skygrind.Core.MoverEvent> feelHudOnMove; // cached: attached once per mover

        // --- the transitions ---
        sealed class TransitionScreen
        {
            public GameObject go;
            public CanvasGroup group;
            public RectTransform rt;
            public Vector2 home;
            public double time, age;
            public float rise;
            public bool on;
            public int shownAlpha = -1;
        }

        TransitionScreen transitionResults, transitionDown, transitionMods;
        UnityEngine.UI.Image transitionCurtain;
        double transitionCurtainAge = double.MaxValue;
        Flow transitionLastFlow = (Flow)(-1);
        int transitionCurtainShown = -1;
        bool transitionNewRun; // NIGHT16 polish #54: OnNewRun fired since the last frame
        CanvasGroup transitionTitleGroup; // NIGHT16 polish #54: TITLE's fade in (alpha only: it stays live)
        double transitionTitleAge = double.MaxValue;
        int transitionTitleShown = -1;

        // --- the rank plate ---
        bool rankPlateEarned;
        double rankPlateAge;
        float rankPlateAlpha; // 0 until the run's first style (Ui.cs's rank change reads it)
        UnityEngine.UI.Image rankPlateBar; // the bar's track (the frame layer's "Rank bar")
        CanvasGroup rankBoxGroup; // NIGHT16 (the lead): UiHudMotion's skewed "Rank plate" box, faded with the rest
        bool rankBoxLooked;
        float rankPlateBarAlpha;
        // NIGHT16 polish #49: RESULTS' RETRY (R): the label, the device and the key it was made for
        TMPro.TextMeshProUGUI retryLabelText;
        bool retryLabelPad;
        string retryLabelKey;
        bool retryLabelSet;

        // NIGHT16 polish #47: the rank bar's pulse
        int rankPulseRank = -1, rankPulseKey = -1;
        Color rankPulseBase = Color.white, rankPulseShown;
        double rankPulseRate, rankPulseClock;

        partial void BuildFeelHud()
        {
            // The ticks: their own small rect, one CanvasGroup for the fade. NIGHT19 U3-33.8 (S14: the centre is clear): the
            // slide charge's cell leaves the crosshair for the vitals cluster, at the stamina pips' row's right end (the HP
            // plate's values twin; Hud19Layout.SlideX), a slanted cell as tall as the pips on an INK-SOFT track.
            feelHudRoot = new GameObject("Movement HUD", typeof(RectTransform), typeof(CanvasGroup)).GetComponent<RectTransform>();
            var vitals19 = values.Find("HP panel (values)") as RectTransform;
            feelHudRoot.SetParent(vitals19 != null ? vitals19 : sight, false);
            if (vitals19 != null)
            {
                feelHudRoot.anchorMin = feelHudRoot.anchorMax = new Vector2(0, 1);
                feelHudRoot.pivot = new Vector2(0.5f, 0.5f);
                feelHudRoot.anchoredPosition = new Vector2(UnityTuning.Hud17VitalsPad + Hud19Layout.SlideX + UnityTuning.Hud19SlideW * 0.5f, -(UnityTuning.Hud19PipsY + UnityTuning.Hud19PipH * 0.5f));
            }
            else
            {
                feelHudRoot.anchorMin = feelHudRoot.anchorMax = feelHudRoot.pivot = new Vector2(0.5f, 0.5f);
                feelHudRoot.anchoredPosition = new Vector2(0, UnityTuning.FeelHudY);
            }
            feelHudRoot.sizeDelta = Vector2.zero;
            feelHudGroup = feelHudRoot.GetComponent<CanvasGroup>();
            feelHudGroup.alpha = 0;
            feelHudGroup.interactable = feelHudGroup.blocksRaycasts = false;
            var track = vitals19 != null ? UiTheme.InkSoft : new Color(1, 1, 1, UnityTuning.FeelHudBackAlpha); // NIGHT19: no translucent white on the INK plate
            int n = UnityTuning.FeelHudMaxPips;
            feelHudPipBack = new UnityEngine.UI.Image[n];
            feelHudPipFill = new UnityEngine.UI.Image[n];
            var pip = new Vector2(UnityTuning.FeelHudPipWidth, UnityTuning.FeelHudPipHeight);
            for (int i = 0; i < n; i++)
            {
                feelHudPipBack[i] = Img(feelHudRoot, $"Pip {i}", track, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, pip);
                feelHudPipBack[i].raycastTarget = false;
                feelHudPipFill[i] = Img(feelHudPipBack[i].rectTransform, "Fill", usable, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                feelHudPipFill[i].rectTransform.pivot = new Vector2(0, 0.5f);
                feelHudPipFill[i].raycastTarget = false;
                feelHudPipBack[i].gameObject.SetActive(false);
                feelHudShownPip[i] = -1;
            }
            feelHudSlideBack = Img(feelHudRoot, "Slide charge", track, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.FeelHudSlideWidth, UnityTuning.FeelHudPipHeight));
            feelHudSlideBack.raycastTarget = false;
            feelHudSlideFill = Img(feelHudSlideBack.rectTransform, "Fill", usable, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            feelHudSlideFill.rectTransform.pivot = new Vector2(0, 0.5f);
            feelHudSlideFill.raycastTarget = false;
            if (vitals19 != null)
            {
                // NIGHT19: the pips' size and slant (hard edges, no cut)
                feelHudSlideBack.rectTransform.sizeDelta = new Vector2(UnityTuning.Hud19SlideW, UnityTuning.Hud19PipH);
                float k19 = Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad);
                UiSkew.Apply(feelHudSlideBack, k19);
                UiSkew.Apply(feelHudSlideFill, k19);
            }
            feelHudTrack = track;
            for (int i = 0; i < n; i++) feelHudShownTint[i] = feelHudShownScale[i] = -1;
            feelHudPipFlashTime = cfg.Num("hud.pipFlashTime");
            feelHudFailFlashTime = cfg.Num("hud.failFlashTime");
            feelHudOnMove = e =>
            {
                switch (e.type)
                {
                    case Skygrind.Core.MoverEventType.Dash:
                    case Skygrind.Core.MoverEventType.DashJump:
                        int k = (int)Math.Floor(e.pips); // hud.js: the pip the dash consumed
                        if (k >= 0 && k < feelHudPipFlash.Length) feelHudPipFlash[k] = feelHudPipFlashTime;
                        break;
                    case Skygrind.Core.MoverEventType.DashFail:
                        feelHudFailFlash = feelHudFailFlashTime;
                        break;
                }
            };
            game.events.On(Skygrind.Core.FeelKeys.SlideCharged, o =>
            {
                if (o == null || o as string == "player") feelHudSlidePulse = UnityTuning.FeelHudPulseTime;
            });

            // The transitions' screens (the MODS draft is attached on its first showing: BuildDraft may run after this).
            transitionResults = TransitionWatch(results, UnityTuning.TransitionResults, UnityTuning.TransitionRise);
            transitionDown = TransitionWatch(down, UnityTuning.TransitionDown, 0);
            // The curtain: the HUD frame layer's first child (over the world, under every HUD and menu layer).
            Color curtain = panel;
            curtain.a = 0;
            transitionCurtain = Img(frame, "Run curtain", curtain, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            transitionCurtain.raycastTarget = false;
            transitionCurtain.transform.SetAsFirstSibling();
            transitionCurtain.enabled = false;

            // The rank plate: hidden until the run's first style.
            Transform bar = frame.Find("Rank bar");
            rankPlateBar = bar != null ? bar.GetComponent<UnityEngine.UI.Image>() : null;
            rankPlateBarAlpha = rankPlateBar != null ? rankPlateBar.color.a : 0;
            game.events.On("styleAward", _ => rankPlateEarned = true);
            // NIGHT16 polish #54: TITLE fades in on every return to it (alpha only; interactable from its first frame).
            if (title != null)
            {
                if (!title.TryGetComponent(out transitionTitleGroup)) transitionTitleGroup = title.AddComponent<CanvasGroup>(); // not ?? (the Editor's fake null)
                // interactable / blocksRaycasts left as they are: UiKiosk's lock shares this group (UiKiosk.cs:50-51, :183)
            }
            game.OnNewRun += () =>
            {
                transitionNewRun = true; // NIGHT16 polish #54: every run start lifts the curtain (RenderFeelHud)
                rankPlateEarned = false;
                rankPlateAge = 0;
                feelHudFullFor = FeelHudMath.HiddenFor(UnityTuning.FeelHudHold, UnityTuning.FeelHudFade);
            };
        }

        TransitionScreen TransitionWatch(GameObject go, double time, float rise)
        {
            if (go == null) return null;
            var t = new TransitionScreen { go = go, time = time, rise = rise, rt = go.GetComponent<RectTransform>(), on = go.activeSelf, age = double.MaxValue };
            if (!go.TryGetComponent(out t.group)) t.group = go.AddComponent<CanvasGroup>(); // NIGHT16 review 1: not ?? (Unity's fake null in the Editor)
            t.group.interactable = t.group.blocksRaycasts = true; // never gates input: the screen is live at once
            if (t.rt != null) t.home = t.rt.anchoredPosition;
            return t;
        }

        static void TransitionStep(TransitionScreen t, double frameDt, bool still)
        {
            if (t == null) return;
            bool on = t.go.activeSelf;
            if (on && !t.on) t.age = 0; // it just opened: the fade starts on this frame
            t.on = on;
            if (!on) return;
            if (t.age < t.time) t.age += frameDt;
            float a = (float)FeelHudMath.TransitionAlpha(t.age, t.time);
            int key = (int)Math.Round(a * 100);
            if (key == t.shownAlpha) return;
            t.shownAlpha = key;
            t.group.alpha = a;
            if (t.rt != null && t.rise > 0) t.rt.anchoredPosition = t.home + new Vector2(0, still ? 0 : -(1 - a) * t.rise); // NIGHT16 review 11: no rise with REDUCED MOTION
        }

        partial void RenderFeelHud(double frameDt)
        {
            Flow f = game.flow;
            // --- the transitions ---
            if (transitionMods == null && draftScreen != null) transitionMods = TransitionWatch(draftScreen, UnityTuning.TransitionMods, UnityTuning.TransitionRise);
            bool transitionStill = game.settings.View.reducedMotion;
            TransitionStep(transitionResults, frameDt, transitionStill);
            TransitionStep(transitionDown, frameDt, transitionStill);
            TransitionStep(transitionMods, frameDt, transitionStill);
            // NIGHT16 polish #54: every new run (PLAY, R, RESTART, RETRY, RETRY SAME SEED) lifts the curtain, every return to
            // TITLE fades TITLE in (ScreenFade; UiFlow: the attract's runs are TITLE). Looks only: the sim and the input never wait.
            Flow uf = game.UiFlow;
            ScreenFade.Kind fade = ScreenFade.For(transitionLastFlow, uf, transitionNewRun);
            transitionNewRun = false;
            if (fade == ScreenFade.Kind.Curtain) transitionCurtainAge = 0;
            else if (fade == ScreenFade.Kind.Title) transitionTitleAge = 0;
            transitionLastFlow = uf;
            if (transitionTitleGroup != null)
            {
                bool fading = transitionTitleAge < UnityTuning.TransitionTitle && uf == Flow.Title;
                if (fading) transitionTitleAge += frameDt;
                float ta = fading ? (float)FeelHudMath.TransitionAlpha(transitionTitleAge, UnityTuning.TransitionTitle) : 1;
                int tk = (int)Math.Round(ta * 100);
                if (tk != transitionTitleShown)
                {
                    transitionTitleShown = tk;
                    transitionTitleGroup.alpha = ta;
                }
            }
            if (transitionCurtain != null)
            {
                bool curtainOn = transitionCurtainAge < UnityTuning.TransitionRun && uf == Flow.Playing;
                if (curtainOn) transitionCurtainAge += frameDt;
                float ca = curtainOn ? UnityTuning.TransitionCurtainAlpha * (1 - (float)FeelHudMath.TransitionAlpha(transitionCurtainAge, UnityTuning.TransitionRun)) : 0;
                int ck = (int)Math.Round(ca * 100);
                if (ck != transitionCurtainShown)
                {
                    transitionCurtainShown = ck;
                    transitionCurtain.enabled = ck > 0;
                    Color c = transitionCurtain.color;
                    c.a = ca;
                    transitionCurtain.color = c;
                }
            }

            // --- RESULTS' retry button (polish #49): "RETRY (R)" with the live restart key; "RETRY" on a pad (no restart button
            // there: A presses it). Compared every frame (a bool and a cached string's reference), set on change only.
            if (retryLabelText != null)
            {
                bool pad = PlayerDevices.Last == InputDevice.Pad;
                string key = PromptText.For("restart", false, KeyLabel(cfg, "restart")); // NIGHT19: PromptText (builder P)
                if (!retryLabelSet || pad != retryLabelPad || !ReferenceEquals(key, retryLabelKey))
                {
                    retryLabelSet = true;
                    retryLabelPad = pad;
                    retryLabelKey = key;
                    retryLabelText.text = RetryLabel.For(pad, key);
                }
            }

            // --- the rank plate ---
            Skygrind.Core.StyleMeter st = game.style;
            if (st != null)
            {
                bool shown = FeelHudMath.RankPlateShown(rankPlateEarned, st.rank, st.Fill);
                if (shown) rankPlateAge += frameDt;
                else rankPlateAge = 0;
                rankPlateAlpha = shown ? (float)FeelHudMath.TransitionAlpha(rankPlateAge, UnityTuning.RankPlateFadeIn) : 0;
                if (rankLetter.alpha != rankPlateAlpha) rankLetter.alpha = rankPlateAlpha;
                if (rankName.alpha != rankPlateAlpha) rankName.alpha = rankPlateAlpha;
                // NIGHT16 polish #47 (hud.js :179-182, :602): the fill breathes, brightness 0.75 -> 1.45 at the rank's pulse rate
                // (StyleMath.RankPulseRate), steady with REDUCED MOTION and while the plate is hidden. The colour only, set when
                // its hundredths change (the rank's base colour parsed once per rank).
                if (st.rank != rankPulseRank)
                {
                    rankPulseRank = st.rank;
                    var sp = st.Params;
                    int r = Math.Max(0, Math.Min(sp.colors.Count - 1, st.rank));
                    rankPulseBase = Look.Hex(sp.colors[r]);
                    rankPulseRate = Skygrind.Core.StyleMath.RankPulseRate(st.rank, sp);
                    rankPulseKey = -1;
                }
                bool pulsing = rankPlateAlpha > 0 && !game.settings.View.reducedMotion;
                if (pulsing) rankPulseClock = RankPulse.Wrap(rankPulseClock + frameDt, rankPulseRate);
                else rankPulseClock = 0;
                float b = pulsing ? (float)RankPulse.Brightness(rankPulseClock, rankPulseRate) : 1;
                int pulseKey = (int)Math.Round(b * 100) * 1000 + (int)Math.Round(rankPlateAlpha * 100);
                if (pulseKey != rankPulseKey || rankFill.color != rankPulseShown) // (Ui.cs's rank change sets the plain colour after this runs)
                {
                    rankPulseKey = pulseKey;
                    rankPulseShown = new Color(Mathf.Min(1, rankPulseBase.r * b), Mathf.Min(1, rankPulseBase.g * b), Mathf.Min(1, rankPulseBase.b * b), rankPlateAlpha);
                    rankFill.color = rankPulseShown;
                }
                if (rankPlateBar != null && rankPlateBar.color.a != rankPlateBarAlpha * rankPlateAlpha)
                {
                    Color c = rankPlateBar.color;
                    c.a = rankPlateBarAlpha * rankPlateAlpha;
                    rankPlateBar.color = c;
                }
                // NIGHT16 (the lead, build 1's tour: Logs/shots/n16-b1/n15-drydock-hub.png): the skewed rank box behind them
                // (UiHudMotion's "Rank plate", built after BuildFeelHud, so found here once) fades with them: hidden, it
                // was an empty dark box mid-right.
                if (!rankBoxLooked)
                {
                    rankBoxLooked = true;
                    Transform box = frame.Find("Rank plate");
                    if (box != null) rankBoxGroup = box.gameObject.AddComponent<CanvasGroup>();
                }
                if (rankBoxGroup != null && rankBoxGroup.alpha != rankPlateAlpha) rankBoxGroup.alpha = rankPlateAlpha;
            }

            // --- the movement HUD ---
            if (feelHudRoot == null) return;
            Skygrind.Core.Mover m = game.mover;
            bool playing = f == Flow.Playing && m != null && sight.gameObject.activeSelf;
            if (!playing)
            {
                FeelHudSetAlpha(0);
                feelHudWasOn18 = false;
                return;
            }
            if (m != feelHudMover)
            {
                if (feelHudMover != null) feelHudMover.Events -= feelHudOnMove;
                feelHudMover = m;
                m.Events += feelHudOnMove;
                feelHudWhole = -1;
            }
            double max = m.MaxPips;
            // NIGHT17 U3-30.7: the pip ticks are retired (the stamina bar under the crosshair: UiStaminaBar.cs reads
            // StaminaHud); the slide charge's tick stays here.
            int n = 0;
            if (n != feelHudPipCount) FeelHudLayout(n);
            // A pip refilled (the whole count rose): its tick pulses (port-only).
            int whole = (int)Math.Floor(Math.Max(0, m.pips));
            if (feelHudWhole >= 0 && whole > feelHudWhole && whole - 1 < n) feelHudPipPulse[whole - 1] = UnityTuning.FeelHudPulseTime;
            feelHudWhole = whole;
            feelHudFailFlash = Math.Max(0, feelHudFailFlash - frameDt);
            feelHudSlidePulse = Math.Max(0, feelHudSlidePulse - frameDt);
            bool busy = feelHudFailFlash > 0 || feelHudSlidePulse > 0;
            for (int i = 0; i < n; i++)
            {
                feelHudPipFlash[i] = Math.Max(0, feelHudPipFlash[i] - frameDt);
                feelHudPipPulse[i] = Math.Max(0, feelHudPipPulse[i] - frameDt);
                busy |= feelHudPipFlash[i] > 0 || feelHudPipPulse[i] > 0;
                int k = (int)Math.Round(FeelHudMath.PipFill(m.pips, i) * 100);
                if (k != feelHudShownPip[i])
                {
                    feelHudShownPip[i] = k;
                    feelHudPipFill[i].rectTransform.anchorMax = new Vector2(k / 100f, 1);
                }
                int tint = feelHudFailFlash > 0 ? 2 : feelHudPipFlash[i] > 0 ? 1 : 0;
                if (tint != feelHudShownTint[i])
                {
                    feelHudShownTint[i] = tint;
                    FeelHudTint(feelHudPipBack[i], feelHudPipFill[i], tint);
                }
                int sc = (int)Math.Round(FeelHudMath.Pulse(UnityTuning.FeelHudPulseTime - feelHudPipPulse[i], UnityTuning.FeelHudPulseTime, UnityTuning.FeelHudPulseScale) * 100);
                if (sc != feelHudShownScale[i])
                {
                    feelHudShownScale[i] = sc;
                    feelHudPipBack[i].rectTransform.localScale = new Vector3(sc / 100f, sc / 100f, 1);
                }
            }
            int slideTint = feelHudFailFlash > 0 ? 2 : 0;
            if (slideTint != feelHudShownSlideTint)
            {
                feelHudShownSlideTint = slideTint;
                FeelHudTint(feelHudSlideBack, feelHudSlideFill, slideTint);
            }
            int ss = (int)Math.Round(FeelHudMath.Pulse(UnityTuning.FeelHudPulseTime - feelHudSlidePulse, UnityTuning.FeelHudPulseTime, UnityTuning.FeelHudPulseScale) * 100);
            if (ss != feelHudShownSlideScale)
            {
                feelHudShownSlideScale = ss;
                feelHudSlideBack.rectTransform.localScale = new Vector3(ss / 100f, ss / 100f, 1);
            }
            double charge = Math.Max(0, Math.Min(1, m.slideCharge));
            int sk = (int)Math.Round(charge * 100);
            if (sk != feelHudShownSlide)
            {
                feelHudShownSlide = sk;
                feelHudSlideFill.rectTransform.anchorMax = new Vector2(sk / 100f, 1);
            }
            feelHudFullFor = FeelHudMath.FullFor(feelHudFullFor, FeelHudMath.AllFull(max, max, m.slideCharge) && !busy, frameDt); // NIGHT17: the slide tick alone
            FeelHudSetAlpha(FeelHudEnter18(frameDt, (float)FeelHudMath.Alpha(feelHudFullFor, UnityTuning.FeelHudHold, UnityTuning.FeelHudFade) * UnityTuning.FeelHudMaxAlpha));
        }

        // NIGHT18 U3-31.6 (FeelAB on): the slide tick enters when it shows (grows from 0.6x and fades in: HudEntrance18) instead
        // of appearing whole the moment the slide spends it; off: `a` as it was.
        double feelHudEnterAge18 = double.MaxValue;
        bool feelHudWasOn18;
        float feelHudShownScale18 = 1;

        float FeelHudEnter18(double frameDt, float a)
        {
            bool on = a > 0;
            if (on && !feelHudWasOn18) feelHudEnterAge18 = 0;
            feelHudWasOn18 = on;
            float s = 1;
            if (FeelAB.On && on && !HudEntrance18.Done(feelHudEnterAge18))
            {
                feelHudEnterAge18 = HudEntrance18.Step(feelHudEnterAge18, frameDt);
                s = HudEntrance18.Scale(feelHudEnterAge18);
                a *= HudEntrance18.Alpha(feelHudEnterAge18);
            }
            if (s != feelHudShownScale18)
            {
                feelHudShownScale18 = s;
                feelHudRoot.localScale = new Vector3(s, s, 1);
            }
            return a;
        }

        /// <summary>The headshot marker's hollow diamond (|x| + |y| = radius, `width` px thick, anti-aliased), white (tinted).</summary>
        static Sprite HeadDiamondSprite(float radius, float width)
        {
            int n = (int)Mathf.Ceil((radius + width + 2) * 2);
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Head diamond" };
            var px = new Color32[n * n];
            float c = n * 0.5f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - c), dy = Mathf.Abs(y + 0.5f - c);
                    float off = Mathf.Abs((dx + dy - radius) * 0.70710678f) - width * 0.5f; // the distance to the edge line
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(0.5f - off) * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        /// <summary>A tick's colours: 0 cyan on its track, 1 the pip flash (white), 2 the dash-fail flash (red-orange).</summary>
        void FeelHudTint(UnityEngine.UI.Image back, UnityEngine.UI.Image fill, int tint)
        {
            fill.color = tint == 2 ? danger : tint == 1 ? Color.white : usable;
            back.color = tint == 2 ? new Color(danger.r, danger.g, danger.b, 0.6f) : tint == 1 ? Color.white : feelHudTrack;
        }

        void FeelHudSetAlpha(float a)
        {
            int k = (int)Math.Round(a * 100);
            if (k == feelHudShownAlpha) return;
            feelHudShownAlpha = k;
            feelHudGroup.alpha = a;
        }

        /// <summary>The pip count changed (a run, VIGOR): the ticks centred under the crosshair, then the slide's.</summary>
        void FeelHudLayout(int n)
        {
            feelHudPipCount = n;
            float pw = UnityTuning.FeelHudPipWidth, gap = UnityTuning.FeelHudPipGap;
            float pipsW = n > 0 ? n * pw + (n - 1) * gap : 0;
            float total = pipsW + (n > 0 ? UnityTuning.FeelHudSlideGap : 0) + UnityTuning.FeelHudSlideWidth;
            float x = -total / 2;
            for (int i = 0; i < feelHudPipBack.Length; i++)
            {
                bool show = i < n;
                if (feelHudPipBack[i].gameObject.activeSelf != show) feelHudPipBack[i].gameObject.SetActive(show);
                if (!show) continue;
                feelHudPipBack[i].rectTransform.anchoredPosition = new Vector2(x + pw / 2, 0);
                x += pw + gap;
            }
            if (n > 0) x += UnityTuning.FeelHudSlideGap - gap;
            feelHudSlideBack.rectTransform.anchoredPosition = new Vector2(x + UnityTuning.FeelHudSlideWidth / 2, 0);
        }
    }

    /// <summary>The movement HUD's, the transitions' and the rank plate's pure rules (FeelHudTests).</summary>
    /// <summary>NIGHT16 polish #54: which fade a flow change gets (SmallUi2Tests).</summary>
    public static class ScreenFade
    {
        public enum Kind { None, Curtain, Title }

        /// <summary>
        /// A new run while the UI shows the run (PLAY, R, RESTART, RETRY, RETRY SAME SEED; UiFlow, so not the attract's runs):
        /// the curtain over the world lifts; any return to TITLE (QUIT TO TITLE, RESULTS' TITLE, the boot): TITLE fades in.
        /// menus.js fades 0.15 s out, switches, then 0.15 s in; the port keeps only the "in" half so nothing waits.
        /// </summary>
        public static Kind For(Flow last, Flow now, bool newRun)
        {
            if (newRun && now == Flow.Playing) return Kind.Curtain;
            if (now == Flow.Title && last != Flow.Title) return Kind.Title;
            return Kind.None;
        }
    }

    /// <summary>NIGHT16 polish #55 / #57: the focused label's tint, a disabled button's look, REROLL's state (SmallUi2Tests).</summary>
    public static class NavLook
    {
        /// <summary>menus.js :201 / :205: a focused menu button's label turns cyan (the 420 px buttons; chips and settings rows keep their own colours).</summary>
        public static bool TintsLabel(bool menuButton, bool interactable) => menuButton && interactable;

        /// <summary>menus.js :203: a disabled button at 0.35.</summary>
        public static float Alpha(bool interactable) => interactable ? 1f : 0.35f;

        /// <summary>menus.js :417: REROLL is disabled with no reroll left or when it can't be paid.</summary>
        public static bool RerollEnabled(int rerollsLeft, bool affordable) => rerollsLeft > 0 && affordable;
    }

    /// <summary>NIGHT16 polish #56: PAUSED's YOUR KEYS lists the pad's buttons after a pad input (SmallUi2Tests).</summary>
    public static class PauseKeysMode
    {
        /// <summary>menus.js :1001-1002: on the pad (the last device), its buttons.</summary>
        public static bool Pad(InputDevice last) => last == InputDevice.Pad;
    }

    /// <summary>NIGHT16 polish #46: the award feed's lines (SmallUiTests).</summary>
    public static class FeedLine
    {
        /// <summary>hud.js :487: "+ NAME points" for a scoring award, the name alone for one that scores nothing.</summary>
        public static string Body(string name, double points) =>
            points > 0 ? string.Concat("+ ", name, " ", points.ToString("0", System.Globalization.CultureInfo.InvariantCulture)) : name;

        /// <summary>hud.js :487 (zero = points &lt;= 0) and .hud-feed div.zero (:216): dimmed.</summary>
        public static bool Dim(double points) => points <= 0;

        /// <summary>A line's alpha: 1 - age / feedFadeTime (hud.js's feed fade), x 0.5 for a dimmed line.</summary>
        public static double Alpha(double age, double fadeTime, bool dim)
        {
            double a = fadeTime > 0 ? Math.Max(0, 1 - age / fadeTime) : 0;
            return dim ? a * 0.5 : a;
        }
    }

    /// <summary>NIGHT16 polish #47: the rank bar's brightness pulse (SmallUiTests).</summary>
    public static class RankPulse
    {
        public const double From = 0.75, To = 1.45; // hud.js :180 @keyframes hud-pulse: brightness(0.75) -> brightness(1.45)

        /// <summary>
        /// The fill's brightness `clock` s in: each way lasts 0.5 / rate s (hud.js :602 --pulse), CSS ease-in-out
        /// (cubic-bezier(0.42, 0, 0.58, 1)), alternate (:179). A rate of 0 or less: steady (1).
        /// </summary>
        public static double Brightness(double clock, double rate)
        {
            if (!(rate > 0)) return 1;
            double half = 0.5 / rate;
            double c = Wrap(clock, rate);
            double u = c < half ? c / half : 2 - c / half;
            return From + (To - From) * EaseInOut(u);
        }

        /// <summary>The clock within one cycle (there and back: 1 / rate s), so it never grows.</summary>
        public static double Wrap(double clock, double rate)
        {
            if (!(rate > 0)) return 0;
            double cycle = 1 / rate;
            double c = clock % cycle;
            return c < 0 ? c + cycle : c;
        }

        /// <summary>CSS ease-in-out: cubic-bezier(0.42, 0, 0.58, 1) at x (bisection on the curve's x; no allocation).</summary>
        static double EaseInOut(double x)
        {
            if (x <= 0) return 0;
            if (x >= 1) return 1;
            double lo = 0, hi = 1, s = x;
            for (int i = 0; i < 40; i++)
            {
                s = (lo + hi) * 0.5;
                double bx = Bezier(s, 0.42, 0.58);
                if (bx < x) lo = s;
                else hi = s;
            }
            return Bezier(s, 0, 1);
        }

        static double Bezier(double s, double p1, double p2)
        {
            double r = 1 - s;
            return 3 * r * r * s * p1 + 3 * r * s * s * p2 + s * s * s;
        }
    }

    /// <summary>NIGHT16 polish #49: RESULTS' retry button's label (SmallUiTests).</summary>
    public static class RetryLabel
    {
        /// <summary>menus.js :1088: "RETRY (" + the restart key + ")"; a pad has no restart button, so no key there (A presses it).</summary>
        public static string For(bool pad, string key) => pad || string.IsNullOrEmpty(key) ? "RETRY" : "RETRY (" + key + ")";
    }

    public static class FeelHudMath
    {
        /// <summary>Pip i's fill (0..1) from the stamina `pips` (pip i holds pips i..i+1).</summary>
        public static double PipFill(double pips, int i) => Math.Max(0, Math.Min(1, pips - i));

        /// <summary>Every pip and the slide charge full: nothing spent.</summary>
        public static bool AllFull(double pips, double maxPips, double slideCharge) => pips >= maxPips && slideCharge >= 1;

        /// <summary>The "full for" clock's resting value: the hold and the fade both past (hidden).</summary>
        public static double HiddenFor(double hold, double fade) => hold + Math.Max(0, fade);

        /// <summary>The time everything has been full: 0 while something is spent, growing by dt while full (it rests at hidden).</summary>
        public static double FullFor(double fullFor, bool allFull, double dt)
        {
            if (!allFull) return 0;
            double hidden = HiddenFor(UnityTuning.FeelHudHold, UnityTuning.FeelHudFade);
            return fullFor >= hidden ? fullFor : Math.Min(hidden, fullFor + dt);
        }

        /// <summary>1 while spent and through the hold, then a linear fade over `fade` s, then 0.</summary>
        public static double Alpha(double fullFor, double hold, double fade)
        {
            if (fullFor <= hold) return 1;
            if (fade <= 0) return 0;
            return Math.Max(0, Math.Min(1, 1 - (fullFor - hold) / fade));
        }

        /// <summary>A transition's alpha `age` s into `time` s: an ease-out (a quick start, a soft landing), 1 at the end.</summary>
        public static double TransitionAlpha(double age, double time)
        {
            if (time <= 0 || age >= time) return 1;
            if (age <= 0) return 0;
            double k = age / time;
            return 1 - (1 - k) * (1 - k);
        }

        /// <summary>A pulse's scale `age` s into `time` s: 1 -> peak -> 1 (a sine hump); 1 outside it.</summary>
        public static double Pulse(double age, double time, double peak)
        {
            if (time <= 0 || age <= 0 || age >= time) return 1;
            return 1 + (peak - 1) * Math.Sin(Math.PI * age / time);
        }

        /// <summary>The rank plate shows once the run has style: an award this run, a non-empty meter, or a rank above the lowest.</summary>
        public static bool RankPlateShown(bool earned, int rank, double fill) => earned || rank > 0 || fill > 0;
    }
}
