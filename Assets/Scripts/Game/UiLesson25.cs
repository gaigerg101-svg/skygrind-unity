// NIGHT25 (builder F, the lesson prompts): ONE short line for the tutorial stage the player is on ("SLIDE UNDER THE GAP", "{jump} TO CLIMB": at most 6 words
// per Design/levels/CURRICULUM.md), drawn low-centre on the meta layer. It is the widget; WHAT to say and WHEN is Campaign24 / Lesson25's (builder L), which calls
//   Ui.ShowLesson25(string prompt, string promptPad = null)   a line as a FORMAT: {action} placeholders ({move} {look} {jump} {dash} {slide} {fire} {punch}
//                                  {special} {altFire} {grapple} {interact} ...) are resolved HERE, per the last device (PlayerDevices.Last) with the live key bindings on a keyboard
//                                  and the live pad layout on a pad, and re-resolved when the device, the bindings or the layout change while it shows. `promptPad`
//                                  (optional) is a pad-only wording; with none, the one format serves both. Lesson25Text.Resolve is the pure part (tested).
//   Ui.HideLesson25()              the verb was used (or the stage ended): the line fades out and is gone
// A line fades in over UnityTuning.Lesson25FadeIn and out over Lesson25FadeOut (M9: no pop-in, no pop-out); showing a line while another is up fades the old one out
// first. Text is the theme's S24 (18 px at 1080p is the floor). Cyan edge: the player's colour (a lesson is the player's). Not run in the Editor when written;
// dn-check clean. The reel `lesson` (FeelReel.N25.Lesson.cs) reads the static mirrors below (the Ui instance is private to SurvivalGame).
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform lesson25Rt;
        CanvasGroup lesson25Group;
        TextMeshProUGUI lesson25Text;
        string lesson25Pending, lesson25PendingPad; // a line waiting for the old one to fade out
        string lesson25Raw, lesson25RawPad; // the shown line's formats (re-resolved when the device changes)
        int lesson25Stamp = -1; // device + glyph set + bindings version + layout of the last resolve
        bool lesson25Want; // the line should be up
        double lesson25Since; // real seconds since the current line was asked for (or told to go)

        /// <summary>What the lesson line is doing now (the reel reads these).</summary>
        public static bool Lesson25Visible;
        public static string Lesson25TextNow = "";
        public static float Lesson25PxNow;
        public static float Lesson25AlphaNow;
        public static float Lesson25FirstAlpha = -1; // the alpha of the first frame it was drawn (0 when it fades in)
        public static double Lesson25InMs = -1; // ms from the ask to alpha 1
        public static double Lesson25OutMs = -1; // ms from the hide to the line gone
        public static int Lesson25Shown, Lesson25Hidden;

        void BuildLesson25()
        {
            var back = Img(metaLayer, "Lesson25", UiTheme.Ink, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, UnityTuning.Lesson25Bottom), new Vector2(UnityTuning.Lesson25MinWidth, UnityTuning.Lesson25Height));
            back.rectTransform.pivot = new Vector2(0.5f, 0);
            Img(back.transform, "Edge", UiTheme.Cyan, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.ThemeAccentBar, 0)).rectTransform.pivot = new Vector2(0, 0.5f);
            lesson25Rt = back.rectTransform;
            lesson25Group = back.gameObject.AddComponent<CanvasGroup>();
            lesson25Group.interactable = lesson25Group.blocksRaycasts = false;
            lesson25Group.alpha = 0;
            lesson25Text = Text(lesson25Rt, "Text", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.White, TextAlignmentOptions.Center, Vector2.zero, new Vector2(UnityTuning.Lesson25MinWidth, UnityTuning.Lesson25Height), new Vector2(0.5f, 0.5f));
            UiTheme.Style(lesson25Text, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold);
            lesson25Rt.gameObject.SetActive(false);
            // NIGHT31 G (the lead's r2 frame shot-N30-spawn31-f9: f1's "MOVE. LOOK. WALK TO THE LIGHT." still up in f9 right after a respawn): the sim clears the old
            // room's line on the respawn's first step (harness lesson31), but this widget faded it out over Lesson25FadeOut and held the new room's line behind it.
            // A room entered (any path: the door, a respawn, DOORFREE) now drops the old line at once; the new room's line (lessonBegin, emitted after roomEnter)
            // fades in fresh. Off: -skygrindOff31.
            if (N31Fixes.On) game.events.On<Skygrind.Core.RoomEvent>("roomEnter", _ => DropLesson25Now());
            BuildHud32Lesson(); // NIGHT32 G (hud32): a boss line goes when its fight is not the live one (UiHud32.cs)
        }

        /// <summary>NIGHT31 G: the line gone this instant (no fade-out, nothing pending): a room change, never a used verb (that one still fades).</summary>
        public void DropLesson25Now()
        {
            lesson25Pending = null;
            lesson25PendingPad = null;
            if (lesson25Rt == null || !lesson25Rt.gameObject.activeSelf) return;
            lesson25Want = false;
            lesson25Group.alpha = 0;
            lesson25Rt.gameObject.SetActive(false);
            Lesson25Visible = false;
            Lesson25AlphaNow = 0;
            Lesson25TextNow = "";
            Lesson25Hidden++;
        }

        /// <summary>The device state a resolved line depends on (a change re-resolves the line on screen).</summary>
        int Lesson25StampNow() => ((int)PlayerDevices.Last * 31 + (int)PlayerDevices.Glyphs) * 7919 + (LiveBindings != null ? LiveBindings.Version : 0) * 131 + PromptText.Version
            + (game.padReader != null && game.padReader.layout != null ? game.padReader.layout.GetHashCode() : 0);

        /// <summary>A placeholder's glyph for the lesson line: the live prompt glyph, or the button's WORDS when it is a sprite tag the atlas does not hold
        /// (the garbled "Za" in stage 1.4: a tag drawn with no sprite behind it).</summary>
        string LessonGlyph25(string action)
        {
            string s = PromptGlyph(action);
            if (PromptText.IsSprite(s) && !PromptGlyphs.HasTags(s))
                s = game.padReader != null ? Glyphs.Label(action, PlayerDevices.Last, PlayerDevices.Glyphs, LiveBindings, game.padReader.layout) : null;
            return s;
        }

        /// <summary>The line `raw` / `rawPad` as the last device reads it (placeholders filled; a pad uses `rawPad` when it has one).</summary>
        string ResolveLesson25(string raw, string rawPad)
        {
            bool pad = PlayerDevices.Last == InputDevice.Pad;
            return Lesson25Text.Resolve(pad && !string.IsNullOrEmpty(rawPad) ? rawPad : raw, pad, LessonGlyph25);
        }

        /// <summary>Show the stage's one line (6 words at most). `prompt` is a format ({jump}, {move} ...); `promptPad` an optional pad wording.</summary>
        public void ShowLesson25(string prompt, string promptPad = null)
        {
            if (string.IsNullOrEmpty(prompt) || lesson25Rt == null) return;
            prompt = Copy47.Map(cfg, prompt); // NIGHT47 F copy47 (Copy47.cs): lessons and boss lines rewritten; off = as they are
            promptPad = Copy47.Map(cfg, promptPad);
            bool up = lesson25Rt.gameObject.activeSelf;
            if (up && lesson25Want && lesson25Raw == prompt && lesson25RawPad == promptPad) return; // already up
            if (up)
            {
                lesson25Pending = prompt; // the old line fades out first, then this one fades in
                lesson25PendingPad = promptPad;
                lesson25Want = false;
                lesson25Since = 0;
                return;
            }
            PlaceLesson25(prompt, promptPad);
        }

        /// <summary>The verb was used (or the stage ended): the line fades out and goes.</summary>
        public void HideLesson25()
        {
            lesson25Pending = null;
            if (lesson25Rt == null || !lesson25Rt.gameObject.activeSelf || !lesson25Want) return;
            lesson25Want = false;
            lesson25Since = 0;
        }

        void SetLesson25Text(string s)
        {
            lesson25Text.text = s;
            float w = Mathf.Max(UnityTuning.Lesson25MinWidth, lesson25Text.GetPreferredValues(s).x + 2 * UnityTuning.Lesson25PadX);
            lesson25Rt.sizeDelta = new Vector2(w, UnityTuning.Lesson25Height);
            lesson25Text.rectTransform.sizeDelta = new Vector2(w, UnityTuning.Lesson25Height);
            Lesson25TextNow = s;
            Lesson25PxNow = lesson25Text.fontSize;
        }

        void PlaceLesson25(string prompt, string promptPad)
        {
            lesson25Raw = prompt;
            lesson25RawPad = promptPad;
            lesson25Stamp = Lesson25StampNow();
            SetLesson25Text(ResolveLesson25(prompt, promptPad));
            lesson25Group.alpha = 0; // fades in from the next frame
            lesson25Rt.gameObject.SetActive(true);
            lesson25Want = true;
            lesson25Since = 0;
            Lesson25Shown++;
            Lesson25FirstAlpha = -1;
            Lesson25InMs = -1;
            Lesson25OutMs = -1;
        }

        void RenderLesson25(double frameDt)
        {
            if (lesson25Rt == null) return;
            RenderHud32Lesson(frameDt); // NIGHT32 G (hud32, UiHud32.cs)
            if (!lesson25Rt.gameObject.activeSelf)
            {
                Lesson25Visible = false;
                Lesson25AlphaNow = 0;
                return;
            }
            lesson25Since += frameDt;
            int stamp = Lesson25StampNow();
            if (stamp != lesson25Stamp && lesson25Raw != null)
            {
                lesson25Stamp = stamp; // the device, the bindings or the layout changed: the same line, read again
                string again = ResolveLesson25(lesson25Raw, lesson25RawPad);
                if (again != lesson25Text.text) SetLesson25Text(again);
            }
            float a = lesson25Group.alpha;
            if (lesson25Want)
            {
                if (Lesson25FirstAlpha < 0) Lesson25FirstAlpha = a;
                a = Mathf.Min(1f, a + (float)(frameDt / UnityTuning.Lesson25FadeIn));
                if (a >= 1f && Lesson25InMs < 0) Lesson25InMs = lesson25Since * 1000.0;
            }
            else
            {
                a = Mathf.Max(0f, a - (float)(frameDt / UnityTuning.Lesson25FadeOut));
                if (a <= 0f)
                {
                    Lesson25OutMs = lesson25Since * 1000.0;
                    lesson25Rt.gameObject.SetActive(false);
                    Lesson25Hidden++;
                    if (N30Fixes.On) Lesson25TextNow = ""; // NIGHT30 (builder S): a reel reading Lesson25TextNow never sees a line that has gone
                    if (lesson25Pending != null)
                    {
                        string next = lesson25Pending, nextPad = lesson25PendingPad;
                        lesson25Pending = null;
                        PlaceLesson25(next, nextPad);
                        a = 0;
                    }
                }
            }
            lesson25Group.alpha = a;
            Lesson25AlphaNow = a;
            Lesson25Visible = lesson25Rt.gameObject.activeSelf && a > 0;
        }
    }
}

namespace Skygrind.Game
{
    /// <summary>NIGHT25 (builder F): the lesson line's format. Pure (no Unity types): the tests drive it with a fake glyph source.</summary>
    public static class Lesson25Text
    {
        /// <summary>Fill the {names} of `format`: a composite alias (move, look) from UnityTuning.Lesson25Aliases for the device, else `glyph(name)` (a bound
        /// action's live key / pad button), else the name in capitals. Text outside braces is kept; an unclosed brace is kept as written.</summary>
        public static string Resolve(string format, bool pad, System.Func<string, string> glyph)
        {
            if (string.IsNullOrEmpty(format) || format.IndexOf('{') < 0) return format;
            var b = new System.Text.StringBuilder(format.Length + 16);
            int i = 0;
            while (i < format.Length)
            {
                int open = format.IndexOf('{', i);
                int close = open >= 0 ? format.IndexOf('}', open) : -1;
                if (open < 0 || close < 0)
                {
                    b.Append(format, i, format.Length - i);
                    break;
                }
                b.Append(format, i, open - i);
                string name = format.Substring(open + 1, close - open - 1);
                string text = null;
                if (UnityTuning.Lesson25Aliases.TryGetValue(name, out var pair)) text = pad ? pair.pad : pair.keys; // null: this device reads its binding
                if (text == null && glyph != null) text = glyph(name);
                b.Append(string.IsNullOrEmpty(text) ? name.ToUpperInvariant() : text);
                i = close + 1;
            }
            return b.ToString();
        }
    }
}
