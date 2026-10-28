// NIGHT42 builder U1 (design.n42.style42, a view row: no sim bit; NIGHT42.md U1, GG's G13: "styule meter ... need to look better and needs to feel
// aweosme to combo and UI needs to be animated ... alot more movement and expression"; DECIDE42 D-6). What hud33 drew (CODE-FACTS G13): the rank letter
// and a 160 x 3 line, both snapping, shown only above 0. The sim's meter is untouched: its six ranks (D C B A S SS, names DULL .. SKYGRIND), thresholds,
// drain and awards stay PortExport/config.json's `style` (StyleCore). With the row on, top-right:
//   - THE LETTER punches in on a rank up (from 1.9 x with an overshoot, a white flash, a shake), deflates and falls in on a rank down, shakes and turns
//     red-orange on a hit taken (the sim's damage penalty), dims as the band drains (the decay you see before the drop), breathes at the top rank, and
//     shrinks out when the meter reaches 0. D and C bone, B and up cyan.
//   - THE LINE eases to the meter (a gain slides up and flashes white; the drain follows the sim), red-orange on a hit.
//   - THE COMBO: "x7" over the line, the awards in a chain (a chain ends on a hit taken or the meter at 0); it pops on every award.
//   - THE NAME under the line (DULL .. SKYGRIND: the top rank's name is SKYGRIND, breathing) and the LAST AWARD under it ("+30 AIRSHOT"), sliding in.
// ULTRAKILL's style meter (hitfeel-web.md section 7: a rank letter and bar, bonus text popups). Off (style42 off, -skygrindOff42 or any older switch):
// nothing is built and hud33's letter and line draw as before. View only. Numbers: UnityTuning.Ui42.cs. Events read: styleAward, rankChange, hurt.
using System;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT42 style42's statics for the reels.</summary>
    public static class Style42
    {
        public static bool Live;
        public static int Rank = -1, Combo, BestCombo, RankUps, RankDowns, Hits, Awards;
        public static string Letter = "", Name = "", Award = "";
        public static float Scale = 1f, Shake, LineShown = -1f;

        public static void ResetCounters()
        {
            Rank = -1;
            Combo = BestCombo = RankUps = RankDowns = Hits = Awards = 0;
            Letter = Name = Award = "";
            Scale = 1f;
            Shake = 0f;
            LineShown = -1f;
        }
    }

    /// <summary>style42's motion rules (pure; EditMode tests: Style42Tests).</summary>
    public static class Style42Math
    {
        /// <summary>The share of the punch at which UiTween.OutBack overshoots most (d/dx of x^2((s+1)x + s) = 0: x = -2s / 3(s+1); 0.58 at s 1.70158).</summary>
        public static readonly double PunchPeakK = 1.0 - 2.0 * UnityTuning.ThemeOvershoot / (3.0 * (UnityTuning.ThemeOvershoot + 1.0));

        /// <summary>The letter's punch runs (its shadow twin hidden: the scaled 2 px shadow read as a ghost letter on the lead's n42-r2 s-up2 frame).</summary>
        public static bool Punching(double age, bool up) => up && age >= 0 && age < UnityTuning.Style42PunchTime * PunchPeakK;

        /// <summary>The letter's scale `age` s after a rank change (up: a punch from PunchScale with an overshoot; down: a deflate from DropScale).</summary>
        public static float LetterScale(double age, bool up)
        {
            double len = up ? UnityTuning.Style42PunchTime : UnityTuning.Style42DropTime;
            if (age < 0 || age >= len) return 1f;
            double k = age / len;
            if (up && k >= PunchPeakK) return 1f; // n42-r2 (the lead's s-up2 frame): past the overshoot it snaps to rest, no slow settle (it read as blur)
            float from = up ? UnityTuning.Style42PunchScale : UnityTuning.Style42DropScale;
            return up ? Mathf.LerpUnclamped(from, 1f, UiTween.OutBack(k)) : Mathf.Lerp(from, 1f, UiTween.OutCubic(k));
        }

        /// <summary>The shake's offset (px) `age` s into a shake of amplitude `amp`: two sines, decaying as (1 - k)^2; zero after ShakeTime.</summary>
        public static Vector2 Shake(double age, float amp)
        {
            if (age < 0 || age >= UnityTuning.Style42ShakeTime || amp <= 0) return Vector2.zero;
            float d = (float)(1 - age / UnityTuning.Style42ShakeTime);
            d *= d;
            double w = age * UnityTuning.Style42ShakeHz * 2 * Math.PI;
            return new Vector2((float)Math.Sin(w) * amp * d, (float)Math.Sin(w * 1.37 + 1.1) * amp * d);
        }

        /// <summary>The letter's alpha from the band's share: full above DecayFrom, down to DecayDim at an empty band.</summary>
        public static float DecayAlpha(float share)
        {
            if (share >= UnityTuning.Style42DecayFrom) return 1f;
            float k = Mathf.Clamp01(share / UnityTuning.Style42DecayFrom);
            return Mathf.Lerp(UnityTuning.Style42DecayDim, 1f, k);
        }

        /// <summary>The combo after an event: an award adds one; a hit or the meter at 0 ends the chain.</summary>
        public static int Combo(int combo, bool award, bool hit, bool empty) => hit || empty ? 0 : award ? combo + 1 : combo;

        /// <summary>The line's eased value this frame (toward `target` at LineEase per second; snaps within 0.002).</summary>
        public static float EaseLine(float shown, float target, double dt)
        {
            if (shown < 0f) return target;
            float k = Mathf.Clamp01((float)(dt * UnityTuning.Style42LineEase));
            float v = shown + (target - shown) * k;
            return Mathf.Abs(target - v) < 0.002f ? target : v;
        }
    }

    public sealed partial class Ui
    {
        bool style42On, style42Read;
        RectTransform s42Rank;
        UiStyle33.Text33 s42Combo, s42Name, s42Award;
        Vector2 s42RankHome, s42AwardHome, s42ComboHome;
        int s42ShownRank = -1, s42ShownCombo = -1, s42Chain;
        bool s42Up, s42WasShown;
        double s42RankAge = double.MaxValue, s42ShakeAge = double.MaxValue, s42HitAge = double.MaxValue, s42GainAge = double.MaxValue, s42ComboAge = double.MaxValue,
            s42AwardAge = double.MaxValue, s42FadeAge = double.MaxValue, s42Clock;
        float s42ShakeAmp, s42Line = -1f;
        string s42AwardText = "", s42LastLetter = "";
        bool s42AwardNew, s42Hurt;

        bool Style42On
        {
            get
            {
                if (!style42Read && cfg != null)
                {
                    style42On = N42View.RowOn(cfg, "style42");
                    style42Read = true;
                }
                return style42On;
            }
        }

        /// <summary>NIGHT51 rank51: the rank letter's resting scale (N51StyleLetterScale; off: Style42LetterScale).</summary>
        static float LetterScale51 => Night51.On ? UnityTuning.N51StyleLetterScale : UnityTuning.Style42LetterScale;

        /// <summary>From BuildHud33Now with style42 on: the combo, the name and the award labels, the event wiring.</summary>
        void BuildStyle42()
        {
            Color bone = UiStyle33.Bone;
            Vector2 tr = Vector2.one;
            Hud33Layout.Box rk = Hud33Layout.RankLetter, rl = Hud33Layout.RankLine;
            s42Rank = h33Rank.rt;
            s42RankHome = P(rk);
            float rowY = (float)(rk.y - rk.h * LetterScale51) - UnityTuning.Style42RowGap; // under the letter at its resting scale (it grows down from its top-right pivot)
            float nameH = UnityTuning.Style42NamePx + 6f, awardH = UnityTuning.Style42AwardPx + 6f;
            s42Name = UiStyle33.Label(hud33, "Style42 name", UnityTuning.Style42NamePx, bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopRight, tr, tr, new Vector2((float)rk.x, rowY), new Vector2(UnityTuning.Hud33RankLineW + (float)rk.w, nameH));
            s42AwardHome = new Vector2((float)rk.x, rowY - nameH - UnityTuning.Style42RowGap);
            s42Award = UiStyle33.Label(hud33, "Style42 award", UnityTuning.Style42AwardPx, bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopRight, tr, tr, s42AwardHome, new Vector2(UnityTuning.Hud33RankLineW + (float)rk.w, awardH));
            float comboH = UnityTuning.Style42ComboPx + 6f;
            s42ComboHome = new Vector2((float)rl.x, (float)rl.y + comboH + UnityTuning.Style42RowGap);
            s42Combo = UiStyle33.Label(hud33, "Style42 combo", UnityTuning.Style42ComboPx, bone, UiStyle33.FaceNumber, TextAlignmentOptions.BottomRight, tr, tr, s42ComboHome, new Vector2(UnityTuning.Hud33RankLineW, comboH), false);
            s42Combo.rt.pivot = new Vector2(1f, 0.5f); // pops round its middle
            s42Combo.rt.anchoredPosition = s42ComboHome - new Vector2(0, comboH * 0.5f);
            s42ComboHome = s42Combo.rt.anchoredPosition;
            s42Name.Set("");
            s42Award.Set("");
            s42Combo.Set("");
            game.events.On<StyleAwardEvent>("styleAward", e =>
            {
                if (e == null || e.quiet) return;
                if (Hud43On && !Ui43Math.ShowAward(e.points)) return; // NIGHT43 U1 hud43 (T-64, the lead's n42-r4 frame "+0 WALL JUMP"): a 0-point award is not drawn nor chained
                s42Chain = Style42Math.Combo(s42Chain, true, false, false);
                s42ComboAge = 0;
                s42GainAge = 0;
                s42AwardText = "+" + Math.Round(e.points).ToString(CultureInfo.InvariantCulture) + " " + e.name;
                s42AwardNew = true;
                Style42.Awards++;
            });
            game.events.On<HurtEvent>("hurt", e =>
            {
                if (e == null || e.owner != "player") return;
                s42Hurt = true;
            });
            game.OnNewRun += () =>
            {
                s42ShownRank = s42ShownCombo = -1;
                s42Chain = 0;
                s42RankAge = s42ShakeAge = s42HitAge = s42GainAge = s42ComboAge = s42AwardAge = s42FadeAge = double.MaxValue;
                s42Line = -1f;
                s42AwardText = s42LastLetter = "";
                s42AwardNew = s42Hurt = s42WasShown = false;
                Style42.ResetCounters();
            };
        }

        /// <summary>Points33's style42 branch (every frame the HUD draws): the letter, the line, the combo, the name, the award.</summary>
        void Style42Draw(StyleMeter st, double frameDt)
        {
            s42Clock += frameDt;
            bool show = st != null && st.meter > 0;
            StyleMath.S sp = st != null ? st.Params : null;
            int rank = show ? st.rank : -1;
            // the events since the last frame
            if (s42Hurt)
            {
                s42Hurt = false;
                if (s42WasShown || show)
                {
                    s42HitAge = 0;
                    s42ShakeAge = 0;
                    s42ShakeAmp = UnityTuning.Style42ShakeHitPx;
                    Style42.Hits++;
                }
                s42Chain = Style42Math.Combo(s42Chain, false, true, false);
            }
            if (!show) s42Chain = Style42Math.Combo(s42Chain, false, false, true);
            Style42.Combo = s42Chain;
            if (s42Chain > Style42.BestCombo) Style42.BestCombo = s42Chain;
            // the letter: a rank change punches or drops
            if (show && rank != s42ShownRank)
            {
                bool up = s42ShownRank < 0 || rank > s42ShownRank;
                s42RankAge = 0;
                s42Up = up;
                if (up) { s42ShakeAge = 0; s42ShakeAmp = UnityTuning.Style42ShakeUpPx; Style42.RankUps++; }
                else Style42.RankDowns++;
                s42ShownRank = rank;
                s42LastLetter = rank >= 0 && rank < sp.letters.Count ? sp.letters[rank] : "";
                h33Rank.Set(s42LastLetter);
                s42Name.Set(rank >= 0 && rank < sp.ranks.Count ? sp.ranks[rank] : "");
                s42FadeAge = double.MaxValue;
            }
            if (!show && s42WasShown) { s42FadeAge = 0; s42ShownRank = -1; }
            s42WasShown = show;
            bool fading = !show && s42FadeAge < UnityTuning.Style42FadeOut;
            bool drawn = show || fading;
            if (!drawn && h33Rank.Shown.Length > 0) { h33Rank.Set(""); s42Name.Set(""); }
            // the band's share and the line
            float share = 0f;
            if (show)
            {
                double lo = sp.thresholds != null && rank < sp.thresholds.Length ? sp.thresholds[rank] : 0;
                double hi = sp.thresholds != null && rank + 1 < sp.thresholds.Length ? sp.thresholds[rank + 1] : sp.max;
                share = hi > lo ? Mathf.Clamp01((float)((st.meter - lo) / (hi - lo))) : 1f;
            }
            s42Line = drawn ? Style42Math.EaseLine(s42Line, share, frameDt) : -1f;
            bool top = sp != null && rank == sp.ranks.Count - 1;
            // colours
            Color bone = UiStyle33.Bone, cyan = UiStyle33.Cyan, danger = UiStyle33.Danger;
            Color baseCol = rank >= UnityTuning.Style42CyanFromRank ? cyan : bone;
            float breathe = top ? 0.5f + 0.5f * Mathf.Cos((float)(s42Clock * UnityTuning.Style42TopHz * 2 * Math.PI)) : 1f;
            Color letter = baseCol;
            if (s42RankAge < UnityTuning.Style42FlashTime && s42Up) letter = Color.Lerp(Color.white, baseCol, (float)(s42RankAge / UnityTuning.Style42FlashTime));
            float hitK = s42HitAge < UnityTuning.Style42HitTime ? (float)(1 - s42HitAge / UnityTuning.Style42HitTime) : 0f;
            if (hitK > 0f) letter = Color.Lerp(letter, danger, hitK);
            float alpha = Style42Math.DecayAlpha(share) * (top ? Mathf.Lerp(0.8f, 1f, breathe) : 1f);
            float scale = LetterScale51 * Style42Math.LetterScale(s42RankAge, s42Up);
            Vector2 off = Style42Math.Shake(s42ShakeAge, s42ShakeAmp);
            if (s42RankAge < UnityTuning.Style42DropTime && !s42Up) off.y += UnityTuning.Style42DropPx * (1f - UiTween.OutCubic(s42RankAge / UnityTuning.Style42DropTime));
            if (fading)
            {
                float k = (float)(s42FadeAge / UnityTuning.Style42FadeOut);
                scale *= Mathf.Lerp(1f, 0.6f, k);
                alpha *= 1f - k;
                if (h33Rank.Shown.Length == 0 && s42LastLetter.Length > 0) h33Rank.Set(s42LastLetter);
            }
            bool ghostOff = Style42Math.Punching(s42RankAge, s42Up);
            if (h33Rank.back.enabled == ghostOff) h33Rank.back.enabled = !ghostOff; // the shadow twin back on the frame after the overshoot
            h33Rank.Color = letter;
            h33Rank.Alpha = Mathf.Round(alpha * 50f) / 50f;
            var sc = new Vector3(scale, scale, 1f);
            if (s42Rank.localScale != sc) s42Rank.localScale = sc;
            Vector2 pos = s42RankHome + new Vector2(Mathf.Round(off.x), Mathf.Round(off.y));
            if (s42Rank.anchoredPosition != pos) s42Rank.anchoredPosition = pos;
            // the line: cyan, white on a gain, red-orange on a hit
            if (h33RankLineRt.gameObject.activeSelf != drawn) h33RankLineRt.gameObject.SetActive(drawn);
            if (drawn)
            {
                float fill = Mathf.Round(Mathf.Clamp01(s42Line) * 200f) / 200f;
                if (h33RankLine.fillAmount != fill) h33RankLine.fillAmount = fill;
                Color lc = cyan;
                if (s42GainAge < UnityTuning.Style42GainFlash) lc = Color.Lerp(Color.white, cyan, (float)(s42GainAge / UnityTuning.Style42GainFlash));
                if (hitK > 0f) lc = Color.Lerp(lc, danger, hitK);
                if (fading) lc.a *= 1f - (float)(s42FadeAge / UnityTuning.Style42FadeOut);
                if (h33RankLine.color != lc) h33RankLine.color = lc;
            }
            if (st43Root != null) Style43Draw(drawn, s42Line, h33RankLine.color); // NIGHT43 U1 hud43 (UiHud33.Style43.cs): the line as blade cells
            // the name: the rank's word; the top's breathes
            s42Name.Color = top ? cyan : bone;
            s42Name.Alpha = drawn ? (fading ? 1f - (float)(s42FadeAge / UnityTuning.Style42FadeOut) : 1f) : 0f;
            float ns = top ? Mathf.Lerp(1f, 1.06f, breathe) : 1f;
            var nsv = new Vector3(ns, ns, 1f);
            if (s42Name.rt.localScale != nsv) s42Name.rt.localScale = nsv;
            // the combo
            int combo = s42Chain >= UnityTuning.Style42ComboMin ? s42Chain : 0;
            if (combo != s42ShownCombo)
            {
                s42ShownCombo = combo;
                s42Combo.Set(combo > 0 ? "x" + combo.ToString(CultureInfo.InvariantCulture) : "");
            }
            if (combo > 0)
            {
                float cs = s42ComboAge < UnityTuning.Style42ComboPop ? Mathf.Lerp(UnityTuning.Style42ComboPopScale, 1f, UiTween.OutCubic(s42ComboAge / UnityTuning.Style42ComboPop)) : 1f;
                var csv = new Vector3(cs, cs, 1f);
                if (s42Combo.rt.localScale != csv) s42Combo.rt.localScale = csv;
                Color cc = combo >= UnityTuning.Style42ComboCyan ? cyan : bone;
                if (hitK > 0f) cc = Color.Lerp(cc, danger, hitK);
                s42Combo.Color = cc;
            }
            // the last award: slides in, holds, fades
            if (s42AwardNew)
            {
                s42AwardNew = false;
                s42Award.Set(s42AwardText);
                s42AwardAge = 0;
                Style42.Award = s42AwardText;
            }
            if (s42AwardAge < UnityTuning.Style42AwardHold + UnityTuning.Style42AwardFade)
            {
                float slide = s42AwardAge < UnityTuning.Style42AwardSlide ? (1f - UiTween.OutCubic(s42AwardAge / UnityTuning.Style42AwardSlide)) * UnityTuning.Style42AwardSlidePx : 0f;
                Vector2 ap = s42AwardHome - new Vector2(Mathf.Round(slide), 0); // n42-r2: it slides in from the LEFT, so it never passes the score / rank column's right edge
                if (s42Award.rt.anchoredPosition != ap) s42Award.rt.anchoredPosition = ap;
                s42Award.Alpha = s42AwardAge < UnityTuning.Style42AwardHold ? 1f : Mathf.Round((1f - (float)((s42AwardAge - UnityTuning.Style42AwardHold) / UnityTuning.Style42AwardFade)) * 50f) / 50f;
                s42Award.Color = rank >= UnityTuning.Style42CyanFromRank ? cyan : bone;
            }
            else if (s42Award.Shown.Length > 0) s42Award.Set("");
            // the clocks
            if (s42RankAge < double.MaxValue) s42RankAge = s42RankAge > 1 ? double.MaxValue : s42RankAge + frameDt;
            if (s42ShakeAge < double.MaxValue) s42ShakeAge = s42ShakeAge > UnityTuning.Style42ShakeTime ? double.MaxValue : s42ShakeAge + frameDt;
            if (s42HitAge < double.MaxValue) s42HitAge = s42HitAge > UnityTuning.Style42HitTime ? double.MaxValue : s42HitAge + frameDt;
            if (s42GainAge < double.MaxValue) s42GainAge = s42GainAge > UnityTuning.Style42GainFlash ? double.MaxValue : s42GainAge + frameDt;
            if (s42ComboAge < double.MaxValue) s42ComboAge = s42ComboAge > UnityTuning.Style42ComboPop ? double.MaxValue : s42ComboAge + frameDt;
            if (s42AwardAge < double.MaxValue) s42AwardAge = s42AwardAge > UnityTuning.Style42AwardHold + UnityTuning.Style42AwardFade ? double.MaxValue : s42AwardAge + frameDt;
            if (s42FadeAge < double.MaxValue) s42FadeAge = s42FadeAge > UnityTuning.Style42FadeOut ? double.MaxValue : s42FadeAge + frameDt;
            Style42.Live = true;
            Style42.Rank = rank;
            Style42.Letter = drawn ? s42LastLetter : "";
            Style42.Name = drawn ? s42Name.Shown : "";
            Style42.Scale = scale;
            Style42.Shake = off.magnitude;
            Style42.LineShown = drawn ? s42Line : -1f;
            if (Style42.Award.Length > 0 && s42Award.Shown.Length == 0) Style42.Award = "";
            Hud33RankShown = drawn;
        }
    }
}
