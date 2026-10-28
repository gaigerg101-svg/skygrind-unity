// NIGHT43 builder U1 (design.n43.teach43, a view row: no sim bit; NIGHT43.md U1, GG's G7: "Study how games such as ULTRAKILL present important
// tutorial text prominently in the center of the screen without permanently obstructing gameplay"; the lead's n42-r4 frame: "HOLD (B): CHAIN THE
// WALLS." mid-screen on a dark box with a thin cyan edge, at the viewmodel's height). THE CENTRE CARD (ULTRAKILL's HudMessage, ui-art-web A1-A2):
//   - centred ABOVE the crosshair (its centre Teach43Y px up; its bottom always clear of the crosshair's 160 x 160 box), never on it;
//   - line 1, the verb: the HERO face at 40 px in capitals, the bound button in CYAN (teach35's lines: the live binding, re-read on a device change;
//     a lesson line keeps its pad glyph sprite); line 2, the rule, in the body face at 24 px (the first sentence is the verb, the rest the rule);
//   - an INK cut plate (the 45 deg cut, a cyan blade accent on its left, scanlines): the one plate the HUD allows, because it is transient and centred;
//   - it opens with the CRT pop (X then Y, ~0.16 s, unscaled), stands as long as text36's slot keeps the line (teach35's 5 s, or until the lesson is
//     done: the sim's rule, unchanged), and closes Y then X.
// It takes the lines text36's lower slot would show (a boss's teaching line, teach35, the lesson), so text36's slot draws nothing while it is on; the
// queue, the priorities and the hold times are text36's (UiHud36Text.Slot36), so the lessons' text and timing are unchanged. teach43 routes the lines
// through text36's slot even if text36 is off. Off: nothing here is built. Numbers: UnityTuning.Ui43.cs.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 teach43's statics for the reels.</summary>
    public static class Teach43Probe
    {
        public static bool Shown;
        public static string Head = "", Sub = "";
        public static int Opens, Closes;
        /// <summary>The card's last open: s from its first frame to full size; its rect's bottom and top in px above the canvas centre.</summary>
        public static double OpenSeconds = -1;
        public static float BottomAboveCentre, TopAboveCentre, Width, HeadPx, SubPx;
    }

    public sealed partial class Ui
    {
        RectTransform t43Root;
        UiStyle33.Text33 t43Head, t43Sub;
        Image t43Plate, t43Accent;
        RawImage t43Scan;
        string t43Text;
        double t43OpenAge = -1, t43CloseAge = -1, t43OpenClock;
        bool t43Up;

        /// <summary>From BuildText36 with teach43 on (under hud33's layer).</summary>
        void BuildTeach43(RectTransform hud33Layer)
        {
            var mid = new Vector2(0.5f, 0.5f);
            t43Root = UiStyle33.Rect(hud33Layer, Teach43Name, mid, mid, new Vector2(0, Teach51Y), new Vector2(UnityTuning.Teach43MaxW, UnityTuning.Teach43MinH));
            Color ink = UiTheme.Ready ? UiTheme.InkSolid : new Color(0.047f, 0.043f, 0.086f, 1f);
            t43Plate = UiKit43.Plate(t43Root, "Plate", UiTheme.A(ink, Night51.On ? UnityTuning.N51TeachPlateAlpha : UnityTuning.Teach43PlateAlpha), mid, mid, Vector2.zero, Vector2.zero);
            UiKit43.Stretch(t43Plate.rectTransform);
            t43Scan = UiKit43.Scan(t43Plate.rectTransform, "Scan", UnityTuning.Ui43ScanAlpha);
            t43Accent = UiKit43.Blade(t43Root, "Accent", UiStyle33.Cyan, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(UnityTuning.Teach43AccentInset, 0), new Vector2(UnityTuning.Teach43AccentW, UnityTuning.Teach43MinH - 2 * UnityTuning.Teach43AccentInset));
            var top = new Vector2(0.5f, 1);
            t43Head = UiStyle33.Label(t43Root, "Verb", UnityTuning.Teach43HeadPx, UiStyle33.Bone, UiStyle33.FaceNumber, TextAlignmentOptions.Center, top, top, new Vector2(0, -UnityTuning.Teach43PadY), new Vector2(UnityTuning.Teach43MaxW, UnityTuning.Teach43HeadPx + 6));
            t43Sub = UiStyle33.Label(t43Root, "Rule", UnityTuning.Teach43SubPx, UiStyle33.Bone, UiTheme.Body, TextAlignmentOptions.Center, top, top, new Vector2(0, -(UnityTuning.Teach43PadY + UnityTuning.Teach43HeadPx + 6 + UnityTuning.Teach43LineGap)), new Vector2(UnityTuning.Teach43MaxW, UnityTuning.Teach43SubPx + 6), false);
            foreach (UiStyle33.Text33 t in new[] { t43Head, t43Sub })
            {
                t.main.richText = true; // the button in cyan (a colour tag) or a lesson's pad glyph (a sprite tag)
                t.main.overflowMode = TextOverflowModes.Overflow; // round 2 (n43-r1 "MOVE. LOOK...."): never an ellipsis: it wraps, then shrinks (Teach43Fit)
                t.main.textWrappingMode = TextWrappingModes.Normal;
                t.back.gameObject.SetActive(false); // the plate carries the contrast; a shadow twin would double a glyph sprite
            }
            t43Head.main.characterSpacing = UnityTuning.Teach43HeadTracking;
            t43Root.gameObject.SetActive(false);
            game.OnNewRun += () =>
            {
                t43Text = null;
                t43OpenAge = t43CloseAge = -1;
                t43Up = false;
                if (t43Root != null) t43Root.gameObject.SetActive(false);
                Teach43Probe.Shown = false;
                Teach43Probe.Head = Teach43Probe.Sub = "";
            };
            Text61Wire(); // NIGHT61 G text61 (UiText61.cs): a new run resets the gate; a room entered ends the previous room's ROOM CLEAR card (nothing with the row off)
        }

        public const string Teach43Name = "Teach card 43";

        /// <summary>NIGHT51 declutter51: the card's centre over the crosshair: under the room title (N51TeachY), out of the aim zone; off: Teach43Y.</summary>
        static float Teach51Y => Night51.On ? UnityTuning.N51TeachY : UnityTuning.Teach43Y;

        /// <summary>The line as the card shows it: teach35's lines resolved again with the button in cyan; any other line as the slot has it.</summary>
        string Teach43Rich(string s)
        {
            if (t35Now != null && s == t35Text)
            {
                bool pad = PlayerDevices.Last == InputDevice.Pad;
                string fmt = Copy47.Map(cfg, pad && !string.IsNullOrEmpty(t35Now.textPad) ? t35Now.textPad : t35Now.text); // NIGHT47 F copy47 (Copy47.cs)
                string hex = UiTheme.Ready ? UiTheme.CyanHex : "19D3E6";
                return Teach35Text.Resolve(fmt, a => "<color=#" + hex + ">" + Teach35Glyph(a) + "</color>"); // on a new line or a device change only
            }
            return s;
        }

        /// <summary>A line's size wrapped at `width`: at `px`, else smaller in 4 px steps (never under `minPx`, >= 16) until it takes two lines at most.
        /// Sets the text's size; returns the wrapped (width, height).</summary>
        static Vector2 Teach43Fit(UiStyle33.Text33 t, string s, float width, float px, float minPx)
        {
            minPx = Mathf.Max(UnityTuning.Hud33TextMin, minPx);
            Vector2 pref = Vector2.zero;
            for (float p = px; p >= minPx - 0.01f; p -= 4f)
            {
                t.main.fontSize = p;
                float line = t.main.GetPreferredValues("X", width, 0).y;
                pref = t.main.GetPreferredValues(s, width, 0);
                if (pref.y <= line * 2.2f) break;
            }
            return new Vector2(Mathf.Min(pref.x, width), pref.y);
        }

        /// <summary>One frame of the card (from Slot36): `s` is the line text36's slot wants now (null: none).</summary>
        void Teach43Draw(string s, double frameDt)
        {
            s = Text61Filter(s, frameDt); // NIGHT61 G text61 (UiText61.cs): outside level 1 and boss fights, a new line waits for the room's title and shows once a run (s itself with the row off)
            s = Teach55Filter(s, frameDt); // NIGHT55 K brief55 (UiBrief55.cs): a line stands at most teachMaxS; no basic jump after level 1 (s itself with the row off)
            if (!string.IsNullOrEmpty(s) && s != t43Text)
            {
                t43Text = s;
                Ui43Math.TeachSplit(Teach43Rich(s), out string head, out string sub);
                t43Head.Set(head);
                t43Sub.Set(sub);
                Case47(t43Head, head); // NIGHT47 F copy47 (docs/COPY47.md): a head of a sentence reads in sentence case; a 1-2 word label keeps its caps
                // the plate fits the lines (<= Teach43MaxW): each line wraps inside the widest card, and shrinks (never under its floor) past two lines
                float maxInner = UnityTuning.Teach43MaxW - 2 * UnityTuning.Teach43PadX;
                Vector2 hs = Teach43Fit(t43Head, head, maxInner, Teach47HeadPx, Teach47HeadMinPx); // NIGHT47 F hud47: 44 / 36 px (UiHud47.cs); off: hud43's 40 / 28
                Vector2 ss = sub.Length > 0 ? Teach43Fit(t43Sub, sub, maxInner, Teach47SubPx, Teach47SubMinPx) : Vector2.zero; // hud47: 30 / 26; off: 24 / 18
                float inner = Mathf.Min(maxInner, Mathf.Max(hs.x, ss.x) + 2f); // +2: TMP's preferred width can sit a hair under the drawn one
                float w = Mathf.Max(inner + 2 * UnityTuning.Teach43PadX, UnityTuning.Teach43MinW);
                inner = w - 2 * UnityTuning.Teach43PadX;
                float h = UnityTuning.Teach43PadY * 2 + hs.y + (sub.Length > 0 ? UnityTuning.Teach43LineGap + ss.y : 0);
                h = Mathf.Max(h, UnityTuning.Teach43MinH);
                t43Root.sizeDelta = new Vector2(w, h);
                t43Head.rt.sizeDelta = new Vector2(inner, hs.y);
                t43Sub.rt.sizeDelta = new Vector2(inner, Mathf.Max(1f, ss.y));
                t43Sub.rt.anchoredPosition = new Vector2(0, -(UnityTuning.Teach43PadY + hs.y + UnityTuning.Teach43LineGap));
                Teach43Probe.HeadPx = t43Head.main.fontSize;
                Teach43Probe.SubPx = sub.Length > 0 ? t43Sub.main.fontSize : 0f;
                t43Accent.rectTransform.sizeDelta = new Vector2(UnityTuning.Teach43AccentW, h - 2 * UnityTuning.Teach43AccentInset);
                UiKit43.FitScan(t43Scan);
                if (!t43Root.gameObject.activeSelf) t43Root.gameObject.SetActive(true);
                t43OpenAge = 0;
                t43CloseAge = -1;
                t43OpenClock = 0;
                t43Up = false;
                Teach52Open(); // NIGHT52 U hud52 (UiTeach52.cs): the chip's clock from this line (nothing with the row off)
                Teach43Probe.Opens++;
                Teach43Probe.Head = head;
                Teach43Probe.Sub = sub;
                Teach43Probe.Width = w;
                Teach43Probe.BottomAboveCentre = Teach51Y - h * 0.5f;
                Teach43Probe.TopAboveCentre = Teach51Y + h * 0.5f;
            }
            else if (string.IsNullOrEmpty(s) && t43Text != null && t43CloseAge < 0)
            {
                t43CloseAge = 0;
                Teach43Probe.Closes++;
            }
            if (!t43Root.gameObject.activeSelf) { Teach43Probe.Shown = false; return; }

            float sx = 1f, sy = 1f;
            if (t43CloseAge >= 0)
            {
                Ui43Math.Close(t43CloseAge, out sx, out sy, out bool done);
                t43CloseAge += frameDt;
                if (done)
                {
                    t43Root.gameObject.SetActive(false);
                    t43Text = null;
                    t43CloseAge = t43OpenAge = -1;
                    Teach43Probe.Shown = false;
                    Teach43Probe.Head = Teach43Probe.Sub = "";
                    return;
                }
            }
            else if (t43OpenAge >= 0)
            {
                Ui43Math.Pop(t43OpenAge, out sx, out sy);
                if (!t43Up && sx >= 1f && sy >= 1f) { t43Up = true; Teach43Probe.OpenSeconds = t43OpenClock; }
                t43OpenClock += frameDt;
                t43OpenAge = t43OpenAge > Ui43Math.PopTime ? -1 : t43OpenAge + frameDt;
            }
            Teach52Chip(ref sx, ref sy, frameDt); // NIGHT52 U hud52 (UiTeach52.cs): the card becomes a chip after 6 s or the first kill (nothing with the row off)
            UiKit43.Scale(t43Root, sx, sy);
            Teach43Probe.Shown = true;
        }
    }
}
