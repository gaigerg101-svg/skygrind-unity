// NIGHT72 builder B (slate B3, design.n72.btext72; the rules and the counter: Btext72.cs). The HUD's half, one hook: the last line of UiWords71.cs's Words71Late() (which RenderHud33 calls after
// FightLine64Late: the frame's last write to the boss text; Bar43Draw's Set, Fight47Late's queue, order64, bossRule61, fightLine64 and words71 have drawn) calls Btext72Late(), before Top52().
// (A file of the boss text's own: UiHud33.cs, the HUD's layout file, is not touched.)
//   Row on: while the fight line draws a line the bar's own order is not drawn (one order at a time); the bar's order is at least UnityTuning.Btext72HintPx and bone; a plate (the HUD's
//           shadow ink at Btext72PlateAlpha) sits behind the fight line and behind the bar's order, sized to their text, fading with it. STAGGER's word is Bar43Draw's own edit (the tag).
//   Row off: the plates hidden, the order's size put back, nothing hidden: the HUD as words71's frame draws it. The counter runs on every frame the bar is up, the row on or off.
// Proof lines (Player.log; L-089: they print that the input fired; t on FeelClock, the reel's fixed clock):
//   [btext72] <boss> | boss lines naming a weapon not held <w> frames (...) | STAGGER drawn while not staggered <s> frames | two orders at once <t> frames (would be <u> ...) |
//             smallest boss text <px> px | plates drawn <p> | held <h> frames | row btext72 on <n> of <m> frames | <why>, t <s> s (FeelClock)
//                       every Btext72SayEveryS s of a drawn boss bar, at the bar's going down, a new boss, a new run and the quit
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        readonly Btext72.Count b72 = new Btext72.Count();
        bool b72Wired;
        Image b72LinePlate, b72HintPlate;
        float b72HintPx0 = -1f, b72Clock;
        string b72LineFor, b72HintFor;
        float b72LinePxFor, b72HintPxFor, b72LineW, b72HintW;

        /// <summary>From RenderHud33 after Words71Late: the boss text's rules (row on), the plates, and the counter (always).</summary>
        void Btext72Late()
        {
            Btext72Wire();
            bool barUp = b43Root != null && b43Root.gameObject.activeSelf && b43Hint != null && b43Tag != null;
            bool rowOn = Btext72.On(cfg); // every frame (D1's same-run toggle)
            if (!barUp)
            {
                if (b72.Dirty) Btext72Say("the boss bar went down");
                Plate72(ref b72LinePlate, fight47, false, 0f, ref b72LineFor, ref b72LinePxFor, ref b72LineW, false);
                Plate72(ref b72HintPlate, b43Hint, false, 0f, ref b72HintFor, ref b72HintPxFor, ref b72HintW, true);
                b72Clock = 0f;
                return;
            }
            string boss = string.IsNullOrEmpty(b43ShownName) ? "?" : b43ShownName;
            if (!string.Equals(boss, b72.Boss, System.StringComparison.Ordinal))
            {
                if (b72.Dirty) Btext72Say("a new boss");
                b72.Reset(boss);
            }
            if (b72HintPx0 < 0f) b72HintPx0 = b43Hint.main.fontSize;
            bool lineDrawn = fight47 != null && fight47.rt.gameObject.activeInHierarchy && fight47.Alpha > 0f && fight47.Shown.Length > 0;
            bool hintDrawn = b43Hint.rt.gameObject.activeInHierarchy && b43Hint.Alpha > 0f && b43Hint.Shown.Length > 0;
            bool wouldTwo = lineDrawn && hintDrawn;
            // ONE ORDER AT A TIME: the bar's own order waits while the fight line draws a line
            if (Btext72.HintHidden(rowOn, lineDrawn, hintDrawn))
            {
                if (b43Hint.rt.gameObject.activeSelf) b43Hint.rt.gameObject.SetActive(false);
                hintDrawn = false;
            }
            // THE BAR'S ORDER: at least Btext72HintPx at 1080p (x TEXT SIZE), bone; put back when the row reads false
            float want = rowOn ? Mathf.Max(b72HintPx0, UnityTuning.Btext72HintPx * (hud47Built ? textScale47 : 1f)) : b72HintPx0;
            if (Mathf.Abs(b43Hint.main.fontSize - want) > 0.01f) Apply47(b43Hint, want);
            if (rowOn && hintDrawn) b43Hint.Color = UiStyle33.Bone;
            // THE PLATES
            Plate72(ref b72LinePlate, fight47, rowOn && lineDrawn, lineDrawn ? fight47.Alpha : 0f, ref b72LineFor, ref b72LinePxFor, ref b72LineW, false);
            Plate72(ref b72HintPlate, b43Hint, rowOn && hintDrawn, hintDrawn ? b43Hint.Alpha : 0f, ref b72HintFor, ref b72HintPxFor, ref b72HintW, true);
            // THE COUNTER (the row on or off)
            Words71.Table t = Words71.Of(cfg);
            string held = W71Held();
            bool wrongBar = hintDrawn && Words71.NotHeld(t, b43Hint.Shown, held);
            bool wrongLine = lineDrawn && Words71.NotHeld(t, fight47.Shown, held);
            bool tagWord = b43Tag.rt.gameObject.activeInHierarchy && string.Equals(b43Tag.Shown, UnityTuning.Bar43StaggerWord, System.StringComparison.Ordinal)
                && Bar43Probe.Mode != Ui43Math.BossMode.Open;
            float minPx = 0f;
            if (lineDrawn) minPx = fight47.main.fontSize;
            if (hintDrawn && (minPx <= 0f || b43Hint.main.fontSize < minPx)) minPx = b43Hint.main.fontSize;
            if (b43Tag.rt.gameObject.activeInHierarchy && b43Tag.Shown.Length > 0 && (minPx <= 0f || b43Tag.main.fontSize < minPx)) minPx = b43Tag.main.fontSize;
            int plates = (b72LinePlate != null && b72LinePlate.gameObject.activeInHierarchy ? 1 : 0) + (b72HintPlate != null && b72HintPlate.gameObject.activeInHierarchy ? 1 : 0);
            // the rule's length: a drawn boss line over Btext72MaxWords words (words as displayed: a glyph or a key name is one word, LEFT CLICK two); remembered once, with its text
            int longest = 0;
            string longText = null;
            if (lineDrawn) { int w = Words72(fight47.Shown, ref b72LineWordsFor, ref b72LineWords); if (w > longest) { longest = w; longText = fight47.Shown; } }
            if (hintDrawn) { int w = Words72(b43Hint.Shown, ref b72HintWordsFor, ref b72HintWords); if (w > longest) { longest = w; longText = b43Hint.Shown; } }
            bool longLine = longest > UnityTuning.Btext72MaxWords;
            if (longLine && !FeelClock.Holding) b72.NoteLong(longText, longest);
            b72.Frame(FeelClock.Holding, rowOn, wrongBar, wrongLine, tagWord, lineDrawn && hintDrawn, wouldTwo, plates, minPx, longLine);
            b72Clock += Time.deltaTime;
            if (b72Clock >= UnityTuning.Btext72SayEveryS) { b72Clock = 0f; Btext72Say("every " + UnityTuning.Btext72SayEveryS.ToString("0") + " s of the bar"); }
        }

        string b72LineWordsFor, b72HintWordsFor;
        int b72LineWords, b72HintWords;

        /// <summary>The words of a drawn line AS DISPLAYED (the t3 verdict: the old count took the keyboard's "LEFT CLICK" for one word and let a seven-word line read as six): a pad glyph's sprite tag is
        /// one word, a key name is one, "LEFT CLICK" is two; any other rich-text tag is no word. Counted once per text.</summary>
        int Words72(string shown, ref string cacheFor, ref int cache)
        {
            if (ReferenceEquals(shown, cacheFor)) return cache;
            string s = System.Text.RegularExpressions.Regex.Replace(shown ?? "", "<sprite[^>]*>", " G ");
            s = System.Text.RegularExpressions.Regex.Replace(s, "<[^>]*>", "");
            cacheFor = shown;
            cache = Btext72.Words(s);
            return cache;
        }

        /// <summary>A plate behind a label: made once, sized to the label's text (preferred width + padding), at Btext72PlateAlpha x the label's alpha; hidden when `show` is false.</summary>
        void Plate72(ref Image plate, UiStyle33.Text33 t, bool show, float alpha, ref string textFor, ref float pxFor, ref float width, bool rightAligned)
        {
            if (t == null || t.rt == null) return;
            if (!show)
            {
                if (plate != null && plate.gameObject.activeSelf) plate.gameObject.SetActive(false);
                return;
            }
            if (plate == null)
            {
                Color ink = UiStyle33.Shadow;
                ink.a = UnityTuning.Btext72PlateAlpha;
                var mid = new Vector2(0.5f, 0.5f);
                plate = UiStyle33.Line(t.rt, "Plate 72", ink, rightAligned ? new Vector2(1f, 0.5f) : mid, rightAligned ? new Vector2(1f, 0.5f) : mid, Vector2.zero, new Vector2(100f, 30f));
                plate.rectTransform.SetAsFirstSibling(); // behind the shadow twin and the text
            }
            if (!plate.gameObject.activeSelf) plate.gameObject.SetActive(true);
            string text = t.Shown;
            float px = t.main.fontSize;
            if (!ReferenceEquals(text, textFor) || Mathf.Abs(px - pxFor) > 0.01f)
            {
                textFor = text;
                pxFor = px;
                width = Mathf.Min(t.main.preferredWidth, Mathf.Max(1f, t.rt.sizeDelta.x)); // the text's own width, never past its box
            }
            var size = new Vector2(width + 2f * UnityTuning.Btext72PlatePadX, t.rt.sizeDelta.y + 2f * UnityTuning.Btext72PlatePadY);
            if (plate.rectTransform.sizeDelta != size) plate.rectTransform.sizeDelta = size;
            var pos = new Vector2(rightAligned ? UnityTuning.Btext72PlatePadX : 0f, 0f);
            if (plate.rectTransform.anchoredPosition != pos) plate.rectTransform.anchoredPosition = pos;
            Color c = plate.color;
            float a = UnityTuning.Btext72PlateAlpha * Mathf.Clamp01(alpha);
            if (!Mathf.Approximately(c.a, a)) { c.a = a; plate.color = c; }
        }

        /// <summary>Once a session: a new run prints the totals and starts clean; the quit prints what is left.</summary>
        void Btext72Wire()
        {
            if (b72Wired || game == null) return;
            b72Wired = true;
            game.OnNewRun += () => { if (b72.Dirty) Btext72Say("a new run"); b72.Reset(""); };
            Application.quitting += () => { if (b72.Dirty) Btext72Say("the quit"); };
        }

        void Btext72Say(string why)
        {
            Debug.Log(b72.Line(why, Btext72.On(cfg), FeelClock.Now));
            b72.Dirty = false;
        }
    }
}
