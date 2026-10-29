// NIGHT12 U3-25.11 (agent F): the onboarding's view. The hint toast (menus.js hint / nextHint: top left under the edge
// margin, never over the reticle; a panel with the cyan edge, 18 px, up to 520 px wide; queued, each for ui.hintTime,
// fading over 300 ms): tips.js shows its tips there (MetaEvents.Tip, or ShowHint). The first-run lesson card
// (design.onboarding; Research/retention section 5 #5) while game.onboarding.Active. NIGHT13 U3-26.11 S9 (the card was
// 115-192 characters for 9 s, arriving at roundBegin with the first enemies): the lesson is one line now
// (UnityTuning.LessonLines, {action} placeholders filled with the live keys as bindings.js fillKeys does; the lesson's
// title for an id it lacks), shown at the break before its round (roundBreak carries the coming round) with
// MetaEvents.OnboardingStep, and kept until the player does the thing it teaches (UnityTuning.LessonDoneOn: a kill, a
// dash or slide, a gun bought, a perk, the power on) or its round ends (the next break). GuideTarget names what the guide
// pulse should light from that break to its round's end, or until the thing is done (a lesson with a guideThen keeps it:
// the guide moves on by itself); the lead turns it into a light's position. Display only: no rule changes. SETTINGS'
// FIRST-RUN GUIDE toggle (GameSettings, Unity-only) is shown through the static constructor below.
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        // SETTINGS shows the Unity-only FIRST-RUN GUIDE row (GameSettings.Extras) beside the web's rows. A static constructor
        // runs after every part's static field initialisers, so Shown (UiSettings.cs) exists here.
        static Ui()
        {
            Shown.Add(UnityTuning.OnboardingSetting);
        }

        double hintTime; // config ui.hintTime (s), read in BuildMeta
        RectTransform hintRt;
        CanvasGroup hintGroup;
        TextMeshProUGUI hintText;
        float hintH, hintAlpha = -1;
        readonly List<string> hintQueue = new List<string>(8);
        double hintLeft;
        bool hintOn;
        string hintShown; // the line showing (its words may sit beside a chip: the dedupe compares the whole line)
        UiShape hintChip18, lessonChip18;
        TextMeshProUGUI hintChipText18, lessonChipText18;

        /// <summary>NIGHT18: a key chip on a top-left card (hidden until a line with a key shows).</summary>
        UiShape CardChip18(RectTransform card, out TextMeshProUGUI label)
        {
            UiShape chip = KeyChip(card, Vector2.zero, out label);
            RectTransform rt = chip.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            chip.gameObject.SetActive(false);
            return chip;
        }

        /// <summary>
        /// NIGHT18: lay a card's line out: with FeelAB on and a leading "[KEY]", the key on its chip and the words beside it;
        /// else the line as NIGHT17 showed it. Returns the extra width the chip takes and the line's height.
        /// </summary>
        float CardLine18(string line, TextMeshProUGUI text, UiShape chip, TextMeshProUGUI chipText, out string words)
        {
            string key = null;
            words = line;
            bool split = FeelAB.On && MenuRules.KeyChipSplit(line, out key, out words);
            if (!split) words = line;
            KeyChipSet(chip, chipText, split ? key : null);
            ChipPlate19(chip, split ? key : null);
            return split ? chip.rectTransform.sizeDelta.x + UnityTuning.HintChipGap18 : 0f;
        }

        /// <summary>NIGHT18: the card's line placed beside its chip, both centred on one line of `lineH` px.</summary>
        static void CardPlace18(TextMeshProUGUI text, UiShape chip, float chipW, float textH, float lineH)
        {
            text.rectTransform.anchoredPosition = new Vector2(UnityTuning.ToastPadX + chipW, -UnityTuning.ToastPadY - (lineH - textH) * 0.5f);
            if (chip.gameObject.activeSelf) chip.rectTransform.anchoredPosition = new Vector2(UnityTuning.ToastPadX, -UnityTuning.ToastPadY - lineH * 0.5f);
        }

        RectTransform lessonRt;
        CanvasGroup lessonGroup;
        TextMeshProUGUI lessonText;
        float lessonH, lessonAlpha = -1;
        Onboarding.Lesson lessonNow; // the lesson on the card (null: none, or it was done)
        string lessonDoneOn; // what ends it early (UnityTuning.LessonDoneOn), or null: its round's end only
        bool lessonFading;
        string guideTarget;
        readonly OnboardingEvent onboardingPayload = new OnboardingEvent(); // reused (read inside the handler)

        /// <summary>
        /// What the first-run guide pulse should light now: the current lesson's guide kind ("wallBuy", "perk", "power",
        /// "relay", "gate"), or null (no lesson, a lesson without a guide, its round is over, or its thing is done).
        /// </summary>
        public string GuideTarget => guideTarget;

        void BuildOnboarding()
        {
            float w = UnityTuning.ToastWidth - 2 * UnityTuning.ToastPadX;
            // The hint (menus.css .hint).
            var hb = Card("Hint", out hintGroup);
            hintRt = hb.rectTransform;
            // NIGHT16 (the audit's defect 3): the lesson card's type (tBody, no letter-spacing), not menus.css .hint's 18 px at
            // 0.08em: stacked under the lesson card, two treatments read as unfinished (Logs/shots/n16-before/n15-drydock-hub.png).
            hintText = Text(hintRt, "Text", "", tBody, text, TextAlignmentOptions.TopLeft, new Vector2(UnityTuning.ToastPadX, -UnityTuning.ToastPadY), new Vector2(w, UnityTuning.ToastHeadLine), new Vector2(0, 1));
            hintText.textWrappingMode = TextWrappingModes.Normal;
            UiTheme.Style(hintText, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold); // NIGHT17: BODY-BOLD on INK
            hintRt.gameObject.SetActive(false);
            // The lesson card: one line (a cyan FIRST RUN tag, then the lesson), as wide as it needs up to LessonLineMaxWidth.
            var lb = Card("Lesson", out lessonGroup);
            lessonRt = lb.rectTransform;
            lessonText = Text(lessonRt, "Text", "", tBody, text, TextAlignmentOptions.Left, new Vector2(UnityTuning.ToastPadX, -UnityTuning.ToastPadY), new Vector2(UnityTuning.LessonLineMaxWidth - 2 * UnityTuning.ToastPadX, UnityTuning.ToastHeadLine), new Vector2(0, 1));
            lessonText.overflowMode = TextOverflowModes.Ellipsis;
            UiTheme.Style(lessonText, UiTheme.Px(UiTheme.Size.S24), UiTheme.CyanPale, UiTheme.BodyBold); // NIGHT17: a teaching line, the prompts' look
            lessonRt.gameObject.SetActive(false);
            // NIGHT18 U3-31.7: a line led by an ASCII key ("[E] BUY GUN") shows the key on a drawn chip (FeelAB on)
            hintChip18 = CardChip18(hintRt, out hintChipText18);
            lessonChip18 = CardChip18(lessonRt, out lessonChipText18);

            game.events.On<TipEvent>(MetaEvents.Tip, e =>
            {
                if (game.attractOn) return; // NIGHT14 review B-4: no tips over TITLE from the attract's run
                string s = MenuRules.HudHint(e.id, e.text); // NIGHT17 round 1: at most 3 words on the HUD, or nothing
                if (s != null) ShowHint(FillGlyphs(s));
            });
            // NIGHT13 S9: the break before a round shows its lesson (roundBreak's number is the coming round); the break also
            // ends the last round's lesson and guide.
            game.events.On<RoundEvent>("roundBreak", e =>
            {
                HideLesson();
                guideTarget = null; // a round without a lesson guides nothing
                if (game.attractOn) return; // NIGHT14 review B-4: the attract's run teaches nobody (TITLE shows over it)
                if (game.onboarding == null || !game.onboarding.Active) return;
                Onboarding.Lesson lesson = game.onboarding.For(e.number);
                if (lesson == null) return;
                ShowLesson(lesson);
                guideTarget = lesson.guide;
                onboardingPayload.round = e.number;
                onboardingPayload.id = lesson.id;
                game.events.Emit(MetaEvents.OnboardingStep, onboardingPayload);
            });
            // The things the lessons teach (UnityTuning.LessonDoneOn; a dash or slide is read from the mover each frame).
            game.events.On<KillEvent>("kill", e =>
            {
                if (e.owner == "player") LessonDone(UnityTuning.DoneOnKill);
            });
            game.events.On("weaponBought", _ => LessonDone(UnityTuning.DoneOnGun));
            game.events.On("perk", _ => LessonDone(UnityTuning.DoneOnPerk));
            game.events.On("powerOn", _ => LessonDone(UnityTuning.DoneOnPower));
            BuildPrompts();
            BuildFirstFifteen(); // NIGHT20 E1
            BuildPadHint(); // NIGHT20: the first-pad hint card
            BuildControlsCard(); // PG3: the controls card at a run's start and its reminders (UiControlsCard.cs)
        }

        // --- NIGHT17 U3-30.8: the verb prompts (glyph + verb, "[SHIFT] BOOST") ------------------------------------------------
        // Hodent's onboarding: one verb a prompt, at the moment it's useful (UnityTuning.Prompts; PromptUseful's cases), gone on
        // the first use, back after PromptReshowAfter s unused, never pausing; at most PromptMaxShown at once; the glyph of
        // the device in hand (Glyphs.Label: a pad shows its button). BOLD CYAN-PALE on an INK chip at the bottom centre (the
        // tile's look). Off with the TIPS row, in the attract and in a bot's run. Per frame: a few compares per prompt; a
        // chip's text is built when it shows or the device changes (a string then, never per frame).

        sealed class VerbPrompt
        {
            public string id, action, verb;
            public bool used, shown;
            public int shows; // round 2: how many times it has shown this session
            public double lastUse = double.NegativeInfinity, usefulFor;
            public RectTransform rt;
            public UiShape plate;
            public TextMeshProUGUI text, chipText;
            public UiShape chip; // round 3: the glyph on a drawn key chip
            public CanvasGroup group;
            public UiEnter enter;
            public float width;
            public Vector2 home18; // NIGHT18: its place in the strip (the layout's), and its slide in along the slant
            public double age18;
            public bool entering18;
        }

        readonly List<VerbPrompt> prompts = new List<VerbPrompt>(8);
        bool[] promptDue;
        readonly int[] promptPick = new int[UnityTuning.PromptMaxShown];
        RectTransform promptRoot;
        InputDevice promptDevice = (InputDevice)(-1);
        int promptVersion19 = -1; // NIGHT19: PromptText.Version the chips were built at
        bool promptLayoutDirty;
        double promptLastAction = double.NegativeInfinity; // round 2: the last shot, swap or hit (real time)
        StaminaHud.Info promptStamina;

        /// <summary>Round 2: no prompt while sprinting (StaminaHud: the one place the views read the bar).</summary>
        bool PromptSprinting()
        {
            StaminaHud.Read(game, ref promptStamina);
            return promptStamina.valid && promptStamina.sprinting;
        }

        void BuildPrompts()
        {
            promptRoot = new GameObject("Prompts", typeof(RectTransform)).GetComponent<RectTransform>();
            promptRoot.SetParent(metaLayer, false);
            promptRoot.anchorMin = promptRoot.anchorMax = new Vector2(0.5f, 0);
            promptRoot.anchoredPosition = new Vector2(0, UnityTuning.PromptStripY);
            promptRoot.sizeDelta = Vector2.zero;
            float px = UiTheme.Px(UiTheme.Size.S24);
            foreach (var (id, action, verb) in UnityTuning.Prompts)
            {
                var p = new VerbPrompt { id = id, action = action, verb = verb };
                var go = new GameObject("Prompt " + id, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape), typeof(CanvasGroup));
                p.rt = (RectTransform)go.transform;
                p.rt.SetParent(promptRoot, false);
                p.rt.anchorMin = p.rt.anchorMax = new Vector2(0.5f, 0.5f);
                p.rt.sizeDelta = new Vector2(200, UnityTuning.PromptChipHeight);
                p.plate = go.GetComponent<UiShape>();
                p.plate.color = UiTheme.Ink;
                p.plate.raycastTarget = false;
                p.group = go.GetComponent<CanvasGroup>();
                p.group.interactable = p.group.blocksRaycasts = false;
                p.text = Text(p.rt, "Text", "", px, UiTheme.CyanPale, TextAlignmentOptions.Center, Vector2.zero, new Vector2(600, UnityTuning.PromptChipHeight), new Vector2(0.5f, 0.5f));
                UiTheme.Style(p.text, px, UiTheme.CyanPale, UiTheme.BodyBold);
                p.text.alignment = TextAlignmentOptions.Left; // placed by its left edge beside the chip
                p.chip = KeyChip(p.rt, Vector2.zero, out p.chipText);
                go.SetActive(false);
                prompts.Add(p);
            }
            promptDue = new bool[prompts.Count];
            game.events.On("punch", o =>
            {
                if (o is MeleeEvent m && m.owner == "player") PromptUsed("punch");
            });
            game.events.On("weaponSwap", _ =>
            {
                PromptUsed("swap");
                promptLastAction = SurvivalGame.RealNow; // round 2: no prompt within PromptActionQuiet s of a swap
            });
            game.events.On("fire", _ => promptLastAction = SurvivalGame.RealNow); // ... of a shot
            game.events.On<HitEvent>("hit", e =>
            {
                if (e.owner == "player") promptLastAction = SurvivalGame.RealNow; // ... of a hit given
            });
            game.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner == "player") promptLastAction = SurvivalGame.RealNow; // ... or taken
            });
        }

        void PromptUsed(string id)
        {
            for (int i = 0; i < prompts.Count; i++)
            {
                if (prompts[i].id != id) continue;
                prompts[i].used = true;
                prompts[i].lastUse = SurvivalGame.RealNow;
            }
        }

        /// <summary>Is prompt `id` useful now (the moment its verb would help)?</summary>
        bool PromptUseful(string id, out bool usingIt)
        {
            usingIt = false;
            Mover mv = game.mover;
            MoveState ms = mv.state;
            double speed = System.Math.Sqrt(mv.velocity.x * mv.velocity.x + mv.velocity.z * mv.velocity.z);
            if (System.Array.IndexOf(UnityTuning.PromptsBreakOnly, id) >= 0 && game.run.state != SurvivalRun.State.Break) return false; // NIGHT17 round 1: the WHEEL and LAST GUN only in a break
            switch (id)
            {
                case "boost":
                    usingIt = ms == MoveState.Dash;
                    return mv.grounded && speed > UnityTuning.PromptMoveSpeed;
                case "slide":
                    usingIt = ms == MoveState.Slide || ms == MoveState.Slam;
                    return mv.grounded && speed > UnityTuning.PromptMoveSpeed;
                case "swap":
                    return game.loadout != null && game.loadout.owned.Count >= 2;
                case "wheel":
                    usingIt = game.combatReader != null && game.combatReader.radial.open;
                    return game.loadout != null && game.loadout.owned.Count >= 3;
                default:
                    return false; // JUMP, ZOOM, BUY: the HUD's own prompts and the first lesson teach them
            }
        }

        /// <summary>The nearest live enemy's distance from the eye (m; infinity: none). One pass over the live list.</summary>
        double PromptNearestEnemy()
        {
            double best = double.PositiveInfinity;
            Vec3 eye = game.Eye;
            List<Enemy> all = game.enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (!e.alive) continue;
                double d = Vec3.DistanceTo(e.AimPoint, eye);
                if (d < best) best = d;
            }
            return best;
        }

        /// <summary>The device's glyph for an input action (a pad's button, else the bound key). NIGHT19 U3-33.4: through
        /// PromptText (design.pad.n19: the drawn button; "radial" on a pad is the button that holds the wheel open, not TAB).</summary>
        string PromptGlyph(string action) => PromptText.For(action, false,
            (game.padReader != null ? Glyphs.Label(action, PlayerDevices.Last, PlayerDevices.Glyphs, LiveBindings, game.padReader.layout) : null)
            ?? KeyLabel(cfg, action) ?? action.ToUpperInvariant());

        /// <summary>NIGHT19: a chip holding a drawn glyph shows the glyph alone (the button is its own plate).</summary>
        static void ChipPlate19(UiShape chip, string key)
        {
            if (chip != null) chip.enabled = !PromptText.IsSprite(key);
        }

        /// <summary>A short line's {action} placeholders as the device's glyphs (a string: when a hint or lesson shows, never per frame).</summary>
        string FillGlyphs(string s)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf('{') < 0) return s;
            var b = new StringBuilder(s.Length + 8);
            int i = 0;
            while (i < s.Length)
            {
                int open = s.IndexOf('{', i);
                int close = open >= 0 ? s.IndexOf('}', open) : -1;
                if (open < 0 || close < 0)
                {
                    b.Append(s, i, s.Length - i);
                    break;
                }
                b.Append(s, i, open - i).Append(PromptGlyph(s.Substring(open + 1, close - open - 1)));
                i = close + 1;
            }
            return b.ToString();
        }


        void RenderPrompts(double frameDt)
        {
            if (promptRoot == null) return;
            // NIGHT17 round 1: only while playing, never with an enemy within PromptQuietRange m (DOWN is another flow)
            bool on = game.flow == Flow.Playing && !game.attractOn && !game.sim.metaSilent && game.settings.Bool(UnityTuning.OnboardingSetting)
                && MenuRules.PromptQuiet(PromptNearestEnemy(), SurvivalGame.RealNow - promptLastAction, PromptSprinting(), game.mover.state == MoveState.Slide || game.mover.state == MoveState.Slam);
            double now = SurvivalGame.RealNow;
            for (int i = 0; i < prompts.Count; i++)
            {
                VerbPrompt p = prompts[i];
                bool useful = false;
                if (on)
                {
                    useful = PromptUseful(p.id, out bool usingIt);
                    if (usingIt)
                    {
                        p.used = true;
                        p.lastUse = now;
                    }
                }
                p.usefulFor = useful ? p.usefulFor + frameDt : 0;
                promptDue[i] = on && MenuRules.PromptDue(useful, p.usefulFor, p.used, now - p.lastUse, System.Array.IndexOf(UnityTuning.PromptsOnce, p.id) >= 0, p.shows);
            }
            int n = MenuRules.PickPrompts(promptDue, UnityTuning.PromptMaxShown, promptPick);
            int promptTextVersion = PromptText.Version; // NIGHT19: a glyph set, a remap or the sprites changed too
            bool deviceChanged = PlayerDevices.Last != promptDevice || promptTextVersion != promptVersion19;
            promptDevice = PlayerDevices.Last;
            promptVersion19 = promptTextVersion;
            for (int i = 0; i < prompts.Count; i++)
            {
                VerbPrompt p = prompts[i];
                bool want = false;
                for (int k = 0; k < n; k++) want |= promptPick[k] == i;
                if (want && (!p.shown || deviceChanged))
                {
                    // round 3: the device's glyph on a drawn key chip, the verb beside it (no ASCII brackets)
                    string glyph = PromptGlyph(p.action);
                    KeyChipSet(p.chip, p.chipText, glyph);
                    ChipPlate19(p.chip, glyph);
                    p.text.text = p.verb;
                    float chipW = p.chip.rectTransform.sizeDelta.x, verbW = p.text.GetPreferredValues(p.verb).x, pad = UnityTuning.PromptChipGap;
                    p.width = pad + chipW + UnityTuning.KeyChipGap + verbW + pad + UnityTuning.ThemeCut;
                    p.rt.sizeDelta = new Vector2(p.width, UnityTuning.PromptChipHeight);
                    p.chip.rectTransform.anchoredPosition = new Vector2(-p.width * 0.5f + pad, 0);
                    MenuPinLeft(p.text, -p.width * 0.5f + pad + chipW + UnityTuning.KeyChipGap, 0);
                    if (!p.shown) p.shows++; // round 2: each prompt at most PromptMaxShows times
                    if (!p.shown)
                    {
                        p.rt.gameObject.SetActive(true);
                        p.enter.In();
                        p.age18 = 0; // NIGHT18: it also slides in along the slant (round 1: whole in 33 ms, under M9's 90)
                        p.entering18 = FeelAB.On;
                    }
                    p.shown = true;
                    promptLayoutDirty = true;
                }
                else if (!want && p.shown)
                {
                    p.shown = false;
                    p.enter.Out();
                    p.entering18 = false;
                    promptLayoutDirty = true;
                }
                if (p.rt.gameObject.activeSelf)
                {
                    float e = p.enter.Step(frameDt);
                    p.group.alpha = Mathf.Clamp01(e);
                    float s = 0.9f + 0.1f * e;
                    p.rt.localScale = new Vector3(s, s, 1);
                    if (!p.shown && !p.enter.Running) p.rt.gameObject.SetActive(false);
                }
            }
            if (promptLayoutDirty)
            {
                promptLayoutDirty = false;
                float total = 0;
                for (int i = 0; i < prompts.Count; i++) if (prompts[i].shown) total += prompts[i].width + UnityTuning.PromptChipGap;
                float x = -total * 0.5f;
                for (int i = 0; i < prompts.Count; i++)
                {
                    if (!prompts[i].shown) continue;
                    prompts[i].home18 = new Vector2(x + prompts[i].width * 0.5f, 0);
                    prompts[i].rt.anchoredPosition = prompts[i].home18;
                    x += prompts[i].width + UnityTuning.PromptChipGap;
                }
            }
            PromptSlide18(frameDt);
        }

        /// <summary>
        /// NIGHT18 (FeelAB on): a prompt that just showed slides in along the slant from ThemeEnterDistance px down-left
        /// (MenuMotion18, ease-out-back over ThemeInTime; REDUCED MOTION: the fade alone), on top of its place in the strip.
        /// Round 1's reel: "Prompt slide" was whole in 33 ms (its fade and a 10 % scale only); M9 wants 90-300 ms.
        /// </summary>
        void PromptSlide18(double frameDt)
        {
            bool still = game.settings.View.reducedMotion;
            for (int i = 0; i < prompts.Count; i++)
            {
                VerbPrompt p = prompts[i];
                if (!p.entering18) continue;
                p.age18 += frameDt;
                if (!FeelAB.On || still || p.age18 >= UnityTuning.ThemeInTime)
                {
                    p.entering18 = false;
                    p.rt.anchoredPosition = p.home18;
                    continue;
                }
                p.rt.anchoredPosition = p.home18 + MenuMotion18.Offset(p.age18);
            }
        }

        /// <summary>A top-left card (the toasts' look): the panel, the cyan edge, a CanvasGroup for its fade.</summary>
        UnityEngine.UI.Image Card(string name, out CanvasGroup group)
        {
            var back = Img(metaLayer, name, UiTheme.Ink, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.ToastWidth, UnityTuning.ToastHeadLine));
            back.rectTransform.pivot = new Vector2(0, 1);
            Img(back.transform, "Edge", UiTheme.Cyan, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.ThemeAccentBar, 0)).rectTransform.pivot = new Vector2(0, 0.5f); // NIGHT17: solid INK, a 4 px accent bar
            group = back.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 0;
            return back;
        }

        /// <summary>tips.js hint: queue a contextual hint (top left); the same text already showing or waiting is not repeated.</summary>
        public void ShowHint(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            // NIGHT17 round 1: a known long notice becomes its short line; in play nothing over 3 words, elsewhere over 8
            string s = MenuRules.HudHint(null, text);
            if (s == null && (game.flow == Flow.Playing || MenuRules.Words(text) > 8)) return;
            text = s ?? text;
            if (hintOn && hintShown == text) return;
            if (hintQueue.Contains(text)) return;
            hintQueue.Add(text);
            if (!hintOn) NextHint();
        }

        /// <summary>menus.js nextHint: the next queued hint for ui.hintTime, or fade out when none waits.</summary>
        void NextHint()
        {
            if (hintQueue.Count == 0)
            {
                hintOn = false;
                return;
            }
            string s = hintQueue[0];
            hintQueue.RemoveAt(0);
            // Round 5 (the reviewer: "POWER-UP · GRAB IT" on a half-empty 520 px plate): the plate sized to its words (the
            // text's one-line width plus the theme's padding), wrapping only at the card's widest (ToastWidth).
            hintShown = s;
            float chipW = CardLine18(s, hintText, hintChip18, hintChipText18, out string words); // NIGHT18: "[E]" on a chip
            float maxW = UnityTuning.ToastWidth - 2 * UnityTuning.ToastPadX - chipW;
            hintText.text = words;
            float w = Mathf.Min(maxW, Mathf.Ceil(hintText.GetPreferredValues(words).x) + 2f);
            float th = hintText.GetPreferredValues(words, w, 0).y;
            float lineH = chipW > 0 ? Mathf.Max(th, UnityTuning.KeyChipSize) : th;
            hintText.rectTransform.sizeDelta = new Vector2(w, th);
            CardPlace18(hintText, hintChip18, chipW, th, lineH);
            hintH = lineH + 2 * UnityTuning.ToastPadY;
            hintRt.sizeDelta = new Vector2(w + chipW + 2 * UnityTuning.ToastPadX, hintH);
            hintLeft = hintTime;
            hintOn = true;
            if (!hintRt.gameObject.activeSelf) hintRt.gameObject.SetActive(true);
            leftDirty = true;
        }

        void ClearHints()
        {
            hintQueue.Clear();
            hintOn = false;
            hintAlpha = 0;
            hintGroup.alpha = 0;
            if (hintRt.gameObject.activeSelf)
            {
                hintRt.gameObject.SetActive(false);
                leftDirty = true;
            }
        }

        /// <summary>NIGHT13 S9: the lesson's one line on the card (built here, on the break: a string then, never per frame).</summary>
        void ShowLesson(Onboarding.Lesson lesson)
        {
            lessonNow = lesson;
            momentOnCard = null; // NIGHT20 E1: a round card takes the card (the moment waits, its clock held)
            UnityTuning.LessonDoneOn.TryGetValue(lesson.id ?? string.Empty, out lessonDoneOn);
            string line = UnityTuning.LessonLines17.TryGetValue(lesson.id ?? string.Empty, out string l) ? l : lesson.title ?? string.Empty; // NIGHT17: glyph + verb, 3 words
            lessonLine = line; // NIGHT20 E1: back on the card after a moment's line
            lessonShownFor = 0;
            ShowCardLine(line);
        }

        // --- NIGHT20 E1 (builder P): the first fifteen minutes v2 (design.adds.firstFifteen; FirstFifteenWatch finds the
        // moments, Core/FirstFifteen.cs is the rule). The moment's line shows on the lesson card while no round card holds it
        // (one thing at a time), with the TIPS row on, while playing, never in the attract or a bot's run; it fades when used,
        // when its moment passes or after showSeconds. Its event "firstFifteenPrompt" (the id; port-only) is GG's sound hook.

        // NIGHT20 E1 (builder P2; FirstFifteenVerbsTests): a round card up FirstFifteen.lessonYield s yields the card to a
        // moment's line (FirstFifteen.MayShow); the lesson's line comes back when the moment's goes (its guide stays lit), and a
        // lesson done or ended under a moment leaves the moment's line on the card.

        FirstFifteenWatch firstFifteen;
        int momentVersion = -1;
        string momentOnCard; // the moment's id the card shows (null: a lesson, or nothing)
        string lessonLine; // the lesson's line (ShowLesson's), to bring back after a moment
        double lessonShownFor; // the lesson's seconds on the card while playing (FirstFifteen.MayShow)

        /// <summary>The moments' rule and watch (null before BuildOnboarding).</summary>
        public FirstFifteenWatch FirstFifteen => firstFifteen;

        // --- NIGHT20 (builder P): the first-pad hint card (PadHintCard: L3 SPRINT, R3 MELEE, RB NEXT GUN, LT ZOOM as the live
        // glyphs; once per profile, the first time a pad plays a run with the v3 layout), in the top-left column under the
        // lesson card for UnityTuning.PadHintSeconds, then it fades. Its lines are built once, when it shows.

        RectTransform padHintRt;
        CanvasGroup padHintGroup;
        TextMeshProUGUI padHintText;
        float padHintH, padHintAlpha;
        double padHintLeft;

        void BuildPadHint()
        {
            var card = Card("Pad hint", out padHintGroup);
            padHintRt = card.rectTransform;
            float w = UnityTuning.ToastWidth - 2 * UnityTuning.ToastPadX;
            padHintText = Text(padHintRt, "Text", "", tBody, text, TextAlignmentOptions.TopLeft, new Vector2(UnityTuning.ToastPadX, -UnityTuning.ToastPadY), new Vector2(w, UnityTuning.ToastHeadLine), new Vector2(0, 1));
            UiTheme.Style(padHintText, UiTheme.Px(UiTheme.Size.S24), UiTheme.CyanPale, UiTheme.BodyBold); // a teaching card: the prompts' look
            padHintText.textWrappingMode = TextWrappingModes.NoWrap;
            padHintRt.gameObject.SetActive(false);
        }

        void RenderPadHint(double frameDt)
        {
            if (padHintRt == null) return;
            if (!padHintRt.gameObject.activeSelf)
            {
                bool due = PadHintCard.Due(game.profile, PlayerDevices.Last == InputDevice.Pad, game.padReader != null && game.padReader.V3,
                    game.flow == Flow.Playing, game.attractOn || (game.sim != null && game.sim.metaSilent));
                if (!due) return;
                if (PadHintCard.Seen(game.profile)) game.SaveProfile();
                padHintText.text = string.Join("\n", PadHintCard.Lines());
                Vector2 size = padHintText.GetPreferredValues(padHintText.text);
                padHintText.rectTransform.sizeDelta = new Vector2(Mathf.Ceil(size.x) + 2f, size.y);
                padHintH = size.y + 2 * UnityTuning.ToastPadY;
                padHintRt.sizeDelta = new Vector2(Mathf.Ceil(size.x) + 2f + 2 * UnityTuning.ToastPadX, padHintH);
                padHintLeft = UnityTuning.PadHintSeconds;
                padHintAlpha = 0;
                padHintGroup.alpha = 0; // fades in once the layout placed it
                padHintRt.gameObject.SetActive(true);
                leftDirty = true;
                game.events.Emit("padHint", null); // NIGHT20: port-only (GG's sound hook)
                return;
            }
            padHintLeft -= frameDt;
            float fade = (float)(frameDt / UnityTuning.CardFade);
            float a = Mathf.Clamp01(padHintAlpha + (padHintLeft > 0 ? fade : -fade));
            if (a != padHintAlpha)
            {
                padHintAlpha = a;
                padHintGroup.alpha = a;
            }
            if (padHintLeft <= 0 && a <= 0)
            {
                padHintRt.gameObject.SetActive(false);
                leftDirty = true;
            }
        }

        void BuildFirstFifteen()
        {
            if (game.sim == null || game.profile == null) return;
            firstFifteen = new FirstFifteenWatch(game.sim, game.profile, game.SaveProfile);
        }

        void RenderFirstFifteen(double frameDt)
        {
            if (firstFifteen == null) return;
            bool playing = game.flow == Flow.Playing && !game.attractOn;
            if (playing) firstFifteen.Step();
            FirstFifteen rule = firstFifteen.rule;
            if (lessonNow != null && playing) lessonShownFor += frameDt; // NIGHT20 E1: the round card's time up
            bool free = lessonNow != null ? rule.MayShow(true, lessonShownFor) : momentOnCard != null || !lessonRt.gameObject.activeSelf;
            bool can = playing && free && game.settings.Bool(UnityTuning.OnboardingSetting);
            rule.Tick(frameDt, can);
            FirstFifteen.Moment m = can ? rule.Active : null;
            if (m != null && (momentOnCard != m.id || momentVersion != rule.Version || lessonFading || !lessonRt.gameObject.activeSelf))
            {
                bool fresh = momentOnCard != m.id || momentVersion != rule.Version; // (a break's fade or a pause: the same line back)
                momentOnCard = m.id;
                momentVersion = rule.Version;
                ShowCardLine(m.line);
                if (fresh) game.events.Emit("firstFifteenPrompt", m.id);
            }
            else if (m == null && momentOnCard != null)
            {
                momentOnCard = null;
                if (lessonNow != null && lessonLine != null) ShowCardLine(lessonLine); // NIGHT20 E1: the round card's line back
                else if (lessonRt.gameObject.activeSelf) lessonFading = true; // used, passed or timed out: the card fades
            }
        }

        /// <summary>A line on the lesson card: its {action}s as the device's glyphs, a leading key on its chip.</summary>
        void ShowCardLine(string line)
        {
            metaSb.Clear();
            metaSb.Append(FillGlyphs(line)); // NIGHT17 round 1: the line alone (3 words on the HUD), the device's glyph
            string s = metaSb.ToString();
            float chipW = CardLine18(s, lessonText, lessonChip18, lessonChipText18, out string words); // NIGHT18: "[SHIFT]" on a chip
            lessonText.text = words;
            float pad = 2 * UnityTuning.ToastPadX;
            float w = Mathf.Min(UnityTuning.LessonLineMaxWidth - chipW, lessonText.GetPreferredValues(words).x + pad);
            float lineH = chipW > 0 ? Mathf.Max(UnityTuning.ToastHeadLine, UnityTuning.KeyChipSize) : UnityTuning.ToastHeadLine;
            lessonText.rectTransform.sizeDelta = new Vector2(w - pad, UnityTuning.ToastHeadLine);
            CardPlace18(lessonText, lessonChip18, chipW, UnityTuning.ToastHeadLine, lineH);
            lessonH = lineH + 2 * UnityTuning.ToastPadY;
            lessonRt.sizeDelta = new Vector2(w + chipW, lessonH);
            lessonFading = false;
            lessonAlpha = 0;
            lessonGroup.alpha = 0; // fades in from the next frame, once the layout placed it
            if (!lessonRt.gameObject.activeSelf) lessonRt.gameObject.SetActive(true);
            leftDirty = true;
        }

        /// <summary>The lesson's round is over (the next break): the card fades out.</summary>
        void HideLesson()
        {
            lessonNow = null;
            lessonDoneOn = null;
            lessonLine = null;
            if (lessonRt.gameObject.activeSelf && momentOnCard == null) lessonFading = true; // NIGHT20 E1: a moment's line stays
        }

        /// <summary>
        /// The player did `kind` (UnityTuning.DoneOn*): if the lesson on the card teaches it, the card fades out and its guide
        /// goes dark (a lesson with a guideThen keeps it: SurvivalGame.UpdateGuide moves from the power to the next relay).
        /// </summary>
        void LessonDone(string kind)
        {
            if (lessonNow == null || lessonFading || lessonDoneOn != kind) return;
            if (lessonNow.guideThen == null) guideTarget = null;
            lessonNow = null;
            lessonLine = null;
            if (momentOnCard == null) lessonFading = true; // NIGHT20 E1: a moment's line on the card stays
        }

        /// <summary>The run left play (DOWN, RESULTS, TITLE): the card goes, the guide lights nothing.</summary>
        void EndLesson()
        {
            guideTarget = null;
            lessonNow = null;
            lessonDoneOn = null;
            lessonFading = false;
            if (lessonRt.gameObject.activeSelf)
            {
                lessonRt.gameObject.SetActive(false);
                leftDirty = true;
            }
        }

        // NIGHT13 U3-26.11 S6: FRESH / STALE (the weapon line's freshness tier, UiFeel) were never explained: a one-time tip
        // the first time the gun in hand reads STALE, through the hint toast's path (the `tip` event, whose handler above
        // shows it). Counted in the profile's tips like tips.js's (once per profile; an automation run, tips.recording
        // false, shows it without counting); the profile is saved with the run as tips.js's counts are. The tier is
        // computed as UiFeel computes it (read here, not from UiFeel's fields, so the weapon line can change freely).
        bool staleTipDone;
        readonly TipEvent staleTipEvent = new TipEvent(); // reused (read inside the handler)

        void CheckStaleTip()
        {
            if (staleTipDone || game.flow != Flow.Playing || game.style == null || game.loadout == null) return;
            Profile p = game.profile;
            if (p != null && p.TipCount(UnityTuning.StaleTipId) >= UnityTuning.StaleTipTimes)
            {
                staleTipDone = true; // this profile has seen it
                return;
            }
            string w = game.loadout.Weapon;
            Arsenal ars = game.loadout.arsenal;
            double v = ars != null && ars.enabled && ars.Catalog.Has(ArsenalMath.WeaponOf(w)) ? ars.Fresh(w) * game.style.Params.max : game.style.Freshness(w);
            if (v < 0 || game.style.TierName(v) != "STALE") return;
            staleTipDone = true;
            if (p != null && (game.tips == null || game.tips.recording)) p.tips[UnityTuning.StaleTipId] = p.TipCount(UnityTuning.StaleTipId) + 1;
            staleTipEvent.id = UnityTuning.StaleTipId;
            staleTipEvent.text = UnityTuning.StaleTip17; // NIGHT17: 6 words (was 113 characters)
            game.events.Emit(MetaEvents.Tip, staleTipEvent);
        }

        void RenderOnboarding(double frameDt)
        {
            CheckStaleTip();
            RenderPrompts(frameDt); // NIGHT17 U3-30.8: the verb prompts
            RenderFirstFifteen(frameDt); // NIGHT20 E1: the moments of need
            RenderPadHint(frameDt); // NIGHT20: the first-pad hint card
            RenderControlsCard(frameDt); // PG3: the controls card (UiControlsCard.cs)
            // The hint: shown for ui.hintTime, then the next; the panel fades in and out over CardFade.
            float fade = (float)(frameDt / UnityTuning.CardFade);
            if (hintOn)
            {
                hintLeft -= frameDt;
                if (hintLeft <= 0) NextHint();
            }
            if (hintRt.gameObject.activeSelf)
            {
                float a = Mathf.Clamp01((hintAlpha < 0 ? 0 : hintAlpha) + (hintOn ? fade : -fade));
                if (a != hintAlpha)
                {
                    hintAlpha = a;
                    hintGroup.alpha = a;
                }
                if (!hintOn && a <= 0)
                {
                    hintRt.gameObject.SetActive(false);
                    leftDirty = true;
                }
            }
            // The lesson card (NIGHT13 S9): on until its thing is done or its round ends, fading in and out over CardFade. A
            // dash or a slide (or the slide's slam in the air) is read from the mover.
            if (lessonRt.gameObject.activeSelf)
            {
                if (lessonDoneOn == UnityTuning.DoneOnDashSlide && lessonNow != null && game.flow == Flow.Playing)
                {
                    MoveState ms = game.mover.state;
                    if (ms == MoveState.Dash || ms == MoveState.Slide || ms == MoveState.Slam) LessonDone(UnityTuning.DoneOnDashSlide);
                }
                float a = Mathf.Clamp01((lessonAlpha < 0 ? 0 : lessonAlpha) + (lessonFading ? -fade : fade));
                if (a != lessonAlpha)
                {
                    lessonAlpha = a;
                    lessonGroup.alpha = a;
                }
                if (lessonFading && a <= 0)
                {
                    lessonFading = false;
                    lessonRt.gameObject.SetActive(false);
                    leftDirty = true;
                }
            }
        }

        // --- keys (bindings.js keyName / fillKeys) ------------------------------------------------------------------------

        static readonly Dictionary<string, string> KeyLabels = new Dictionary<string, string>();
        static int keyLabelsVersion = int.MinValue;

        /// <summary>The live key bindings (SurvivalGame.Init sets them; null: the config's defaults).</summary>
        public static Bindings LiveBindings;

        /// <summary>
        /// bindings.js keyName of an action's key (config.input.bindings: KeyE -> E, Digit1 -> 1, ShiftLeft -> L-SHIFT), or
        /// null when the action has no binding. Read from LiveBindings once SurvivalGame sets them (NIGHT13 review: CONTROLS
        /// rebinds at run time), else the config's defaults; cached, and the cache cleared when the bindings' Version moves.
        /// Usable as Tips' keyLabel: <c>a => Ui.KeyLabel(cfg, a)</c>.
        /// </summary>
        public static string KeyLabel(GameConfig cfg, string action)
        {
            if (string.IsNullOrEmpty(action)) return null;
            if (LiveBindings != null && LiveBindings.Version != keyLabelsVersion)
            {
                keyLabelsVersion = LiveBindings.Version;
                KeyLabels.Clear();
            }
            if (KeyLabels.TryGetValue(action, out string label)) return label;
            string path = "input.bindings." + action;
            string code = LiveBindings != null ? LiveBindings.Code(action) : cfg.Has(path) ? cfg.Get(path) as string : null;
            label = code == null ? null : KeyName(code);
            KeyLabels[action] = label;
            return label;
        }

        static string KeyName(string code)
        {
            if (code.StartsWith("Key")) return code.Substring(3);
            if (code.StartsWith("Digit")) return code.Substring(5);
            switch (code)
            {
                case "Space": return "SPACE";
                case "ShiftLeft": return "L-SHIFT";
                case "ShiftRight": return "R-SHIFT";
                case "AltLeft": return "L-ALT";
                case "AltRight": return "R-ALT";
                case "Tab": return "TAB";
                case "CapsLock": return "CAPS";
                case "Enter": return "ENTER";
                case "Backquote": return "`";
                default: return code.Replace("Arrow", "").ToUpperInvariant();
            }
        }

        /// <summary>bindings.js fillKeys: each {action} becomes that action's key name; unknown names stay as they are.</summary>
        public static string FillKeys(GameConfig cfg, string s)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf('{') < 0) return s;
            var sb = new StringBuilder(s.Length + 8);
            int i = 0;
            while (i < s.Length)
            {
                char ch = s[i];
                if (ch == '{')
                {
                    int j = i + 1;
                    while (j < s.Length && (char.IsLetterOrDigit(s[j]) || s[j] == '_')) j++;
                    if (j < s.Length && s[j] == '}' && j > i + 1)
                    {
                        string label = KeyLabel(cfg, s.Substring(i + 1, j - i - 1));
                        if (label != null)
                        {
                            sb.Append(label);
                            i = j + 1;
                            continue;
                        }
                    }
                }
                sb.Append(ch);
                i++;
            }
            return sb.ToString();
        }
    }
}
