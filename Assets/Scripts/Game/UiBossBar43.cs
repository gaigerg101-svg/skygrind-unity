// NIGHT43 builder U1 (design.n43.bossbar43, a view row: no sim bit; NIGHT43.md U1 with B1, GG's G14: "the player needs clear feedback explaining
// whether the boss is taking damage, whether it is vulnerable, what phase it is in, and what the player's attacks are accomplishing"). With the row on
// and SurvivalSim.N43BossState valid (B1 fills it for THE WICK: valid for the whole fight), top centre, in the dusk instrument's language:
//   - the name (HERO face, 32 px) over the bar's left end; "LEG 1 / 3" (label face) over its right end, punched on a leg change;
//   - THE BAR: one 640 x 14 blade in a bracket: the track, a HOT after-image of what the last hits took (waits 0.15 s, drains at gap x 5 + 0.5 per s:
//     a pistol makes a sliver, a big hit a chunk: bosses-web A2), the fill; the lock (lockAt) a bone notch through the bar;
//   - the state, one at a time (bosses-web A3):
//       NORMAL  the fill red-orange (the boss is danger);
//       LOCKED  the fill STEEL (it cannot be hurt now) and the tag LOCKED in steel; the hint ("CRACK A PLATE: RETURN AN EMBER") in bone;
//       OPEN    the fill bright (red-orange toward white, breathing), the bracket cyan, the tag "OPEN 1.4" in cyan counting down, the line under the bar
//               a cyan timer draining (the player's window);
//   - the stagger meter (stagger01 >= 0) as a 4 px bone line under the bar, named STAGGER, when not OPEN;
//   - the hint in the body face under the bar's right end, its {action} tokens resolved to the bound buttons.
// While it draws: N41View.BossBar41Live (hud33's bar hides) and bars41 does not draw (one bar, never two); the message slot drops under it. Without a
// valid state (another boss, B1's state not landed) bars41 draws as before. Off: nothing here is built. Numbers: UnityTuning.Ui43.cs.
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 bossbar43's statics for the reels.</summary>
    public static class Bar43Probe
    {
        public static bool Shown;
        public static Ui43Math.BossMode Mode;
        public static float Fill, After, Stagger = -1f;
        public static int Leg, Legs, LegChanges, Hits;
        public static string Name = "", Tag = "", Hint = "";
        /// <summary>The ui43 reel only: a state to draw in place of the sim's (the view's own frames of every state, apart from B1's WICK).</summary>
        public static bool ForceOn;
        public static Boss43State ForceState;
    }

    public sealed partial class Ui
    {
        RectTransform b43Root, b43Bar;
        UiStyle33.Text33 b43Name, b43Leg, b43Tag, b43Hint;
        Image b43Track, b43After, b43Fill, b43Notch, b43Line, b43LineTrack;
        Image[] b43Bracket;
        Boss43State b43S;
        float b43Last = -1f, b43AfterLv = -1f, b43OpenMax;
        double b43Wait, b43Age = -1, b43LegAge = double.MaxValue, b43Clock;
        int b43ShownLeg = -1, b43ShownLegs = -1;
        string b43ShownName, b43ShownHintRaw, b43ShownTag, b43TagTenths, b43TagOpen;
        Ui43Math.BossMode b43ShownMode = Ui43Math.BossMode.None;

        /// <summary>From RenderBar41 (every frame hud33 draws): true while bossbar43 draws a valid state (then bars41 does not).</summary>
        bool Bar43Frame(double frameDt)
        {
            if (!Bossbar43On || game.sim == null) return false;
            b43S = Bar43Probe.ForceOn ? Bar43Probe.ForceState : game.sim.N43BossState;
            bool on = b43S.valid && game.flow == Flow.Playing;
            if (!on)
            {
                if (b43Root != null && b43Root.gameObject.activeSelf) b43Root.gameObject.SetActive(false);
                Target74Off(); // NIGHT74 B target74 (UiTarget74.cs): no bar, no chevron
                Bar43Probe.Shown = false;
                Bar43Probe.Mode = Ui43Math.BossMode.None;
                return false;
            }
            if (b43Root == null) { BuildBar43(); if (hud47Built) Bar47Build(); } // NIGHT47 F hud47 (UiHud47.Boss.cs): the bar's text sized, its hint a fight line
            if (!b43Root.gameObject.activeSelf)
            {
                b43Root.gameObject.SetActive(true);
                b43Age = 0;
                b43Last = b43AfterLv = -1f;
                b43ShownName = b43ShownHintRaw = b43ShownTag = null;
                b43ShownLeg = b43ShownLegs = -1;
                b43ShownMode = Ui43Math.BossMode.None;
            }
            if (h33BossRt != null && h33BossRt.gameObject.activeSelf) h33BossRt.gameObject.SetActive(false); // one bar, never two (as bars41)
            Bar43Draw(frameDt);
            return true;
        }

        void BuildBar43()
        {
            var top = new Vector2(0.5f, 1);
            var tl = new Vector2(0, 1);
            var tr = new Vector2(1, 1);
            float w = UnityTuning.Bar43W, edge = UnityTuning.Hud33EdgeMargin;
            Color bone = UiStyle33.Bone, muted = UiKit43.Muted;
            b43Root = UiStyle33.Rect(hud33, Bar43Name, top, top, new Vector2(0, -edge), new Vector2(w, UnityTuning.Bar43H));
            b43Name = UiStyle33.Label(b43Root, "Name", UnityTuning.Bar43NamePx, bone, UiStyle33.FaceNumber, TextAlignmentOptions.BottomLeft, tl, tl, Vector2.zero, new Vector2(w * 0.7f, UnityTuning.Bar43NamePx + 4));
            b43Name.main.characterSpacing = b43Name.back.characterSpacing = UnityTuning.Teach43HeadTracking;
            b43Leg = UiStyle33.Label(b43Root, "Leg", UnityTuning.Bar43LabelPx, muted, UiStyle33.FaceLabel, TextAlignmentOptions.BottomRight, tr, tr, new Vector2(0, -UnityTuning.Bar43LegDrop), new Vector2(w * 0.3f, UnityTuning.Bar43LabelPx + 4));
            float barY = -(UnityTuning.Bar43NamePx + 4 + UnityTuning.Bar43Gap);
            b43Bar = UiStyle33.Rect(b43Root, "Bar", tl, tl, new Vector2(0, barY), new Vector2(w, UnityTuning.Bar43BarH));
            b43Track = UiKit43.Blade(b43Bar, "Track", UiTheme.A(muted, UnityTuning.Hud43TrackAlpha), tl, tl, Vector2.zero, new Vector2(w, UnityTuning.Bar43BarH));
            UiKit43.Stretch(b43Track.rectTransform);
            b43After = UiKit43.BladeFill(b43Bar, "After", Color.white, tl, tl, Vector2.zero, Vector2.zero);
            UiKit43.Stretch(b43After.rectTransform);
            b43Fill = UiKit43.BladeFill(b43Bar, "Fill", UiStyle33.Danger, tl, tl, Vector2.zero, Vector2.zero);
            UiKit43.Stretch(b43Fill.rectTransform);
            UiSkew.Apply(UiKit43.Scan(b43Bar, "Scan", UnityTuning.Ui43ScanAlpha), UiKit43.SlantTan);
            b43Notch = UiStyle33.Line(b43Bar, "Lock", bone, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.Bar43NotchW, UnityTuning.Bar43BarH + 2 * UnityTuning.Bar43NotchOver));
            b43Notch.gameObject.SetActive(false);
            b43Bracket = UiKit43.Bracket(b43Bar, "Bracket", muted, UnityTuning.Hud43BracketPad);
            float lineY = barY - UnityTuning.Bar43BarH - UnityTuning.Bar43Gap;
            b43LineTrack = UiKit43.Blade(b43Root, "Meter track", UiTheme.A(muted, UnityTuning.Hud43TrackAlpha), tl, tl, new Vector2(0, lineY), new Vector2(w, UnityTuning.Bar43LineH));
            b43Line = UiKit43.BladeFill(b43Root, "Meter", bone, tl, tl, new Vector2(0, lineY), new Vector2(w, UnityTuning.Bar43LineH));
            float rowY = lineY - UnityTuning.Bar43LineH - UnityTuning.Bar43Gap;
            b43Tag = UiStyle33.Label(b43Root, "State", UnityTuning.Bar43LabelPx, muted, UiStyle33.FaceLabel, TextAlignmentOptions.TopLeft, tl, tl, new Vector2(0, rowY), new Vector2(UnityTuning.Bar43TagW, UnityTuning.Bar43LabelPx + 4));
            b43Hint = UiStyle33.Label(b43Root, "Hint", UnityTuning.Bar43HintPx, bone, UiTheme.Body, TextAlignmentOptions.TopRight, tr, tr, new Vector2(0, rowY), new Vector2(w - UnityTuning.Bar43TagW, UnityTuning.Bar43HintPx + 6), false);
            b43Hint.main.richText = true;
            b43Hint.back.richText = false; // a glyph sprite would double in the shadow twin
            b43Hint.main.overflowMode = TextOverflowModes.Ellipsis;
            b43Root.gameObject.SetActive(false);
        }

        public const string Bar43Name = "Boss bar 43";

        /// <summary>For the message slot (UiHud33.Slot33): the bar draws this frame.</summary>
        bool Bar43Live => b43Root != null && b43Root.gameObject.activeSelf;

        void Bar43Draw(double frameDt)
        {
            b43Clock += frameDt;
            Boss43State s = b43S;
            Color bone = UiStyle33.Bone, cyan = UiStyle33.Cyan, danger = UiStyle33.Danger, muted = UiKit43.Muted, steel = UiKit43.Steel;
            Ui43Math.BossMode mode = Ui43Math.Mode(s.valid, s.open, s.locked);

            if (!ReferenceEquals(s.name, b43ShownName)) { b43ShownName = s.name; b43Name.Set(s.name ?? ""); }
            if (s.leg != b43ShownLeg || s.legs != b43ShownLegs)
            {
                if (b43ShownLeg > 0 && s.leg != b43ShownLeg) { b43LegAge = 0; Bar43Probe.LegChanges++; }
                b43ShownLeg = s.leg;
                b43ShownLegs = s.legs;
                b43Leg.Set(Ui43Math.Leg(s.leg, s.legs));
            }
            float legSc = Ui43Math.Tick(b43LegAge);
            var lsv = new Vector3(legSc, legSc, 1f);
            if (b43Leg.rt.localScale != lsv) b43Leg.rt.localScale = lsv;
            b43Leg.Color = b43LegAge < UnityTuning.Ui43TickTime ? Color.white : muted;
            if (b43LegAge < double.MaxValue) b43LegAge = b43LegAge > UnityTuning.Ui43TickTime ? double.MaxValue : b43LegAge + frameDt;

            // HP and its after-image
            float fill = Mathf.Clamp01((float)s.hp01);
            if (b43Last < 0f) { b43Last = fill; b43AfterLv = fill; }
            if (fill < b43Last - 1e-4f) Bar43Probe.Hits++;
            b43AfterLv = Ui43Math.After(b43AfterLv, b43Last, fill, ref b43Wait, frameDt);
            b43Last = fill;
            float fq = Mathf.Round(fill * 500f) / 500f, aq = Mathf.Round(b43AfterLv * 500f) / 500f;
            if (b43Fill.fillAmount != fq) b43Fill.fillAmount = fq;
            if (b43After.fillAmount != aq) b43After.fillAmount = aq;

            // the lock notch
            bool notch = s.lockAt > 0 && s.lockAt < 1;
            if (b43Notch.gameObject.activeSelf != notch) b43Notch.gameObject.SetActive(notch);
            if (notch)
            {
                float x = Mathf.Round((float)s.lockAt * UnityTuning.Bar43W); // at the bar's mid-height, where the skewed fill's edge crosses that share
                if (b43Notch.rectTransform.anchoredPosition.x != x) b43Notch.rectTransform.anchoredPosition = new Vector2(x, 0);
                b43Notch.color = mode == Ui43Math.BossMode.Locked ? steel : bone;
            }

            // the state: the fill's colour, the bracket, the tag, the line under the bar
            float breathe = 0.5f + 0.5f * Mathf.Cos((float)(b43Clock * UnityTuning.Ui43BreatheHz * 2 * System.Math.PI));
            Color fc = mode == Ui43Math.BossMode.Locked ? steel : mode == Ui43Math.BossMode.Open ? Color.Lerp(danger, Color.white, UnityTuning.Bar43OpenHot + UnityTuning.Bar43OpenSwing * breathe) : danger;
            if (b43Fill.color != fc) b43Fill.color = fc;
            Color bc = mode == Ui43Math.BossMode.Open ? cyan : mode == Ui43Math.BossMode.Locked ? steel : muted;
            for (int i = 0; i < b43Bracket.Length; i++) if (b43Bracket[i].color != bc) b43Bracket[i].color = bc;
            if (mode == Ui43Math.BossMode.Open && b43ShownMode != Ui43Math.BossMode.Open) b43OpenMax = Mathf.Max(0.01f, (float)s.openLeft);
            string tag;
            Color tagC;
            float lineFill;
            Color lineC;
            bool lineOn;
            if (mode == Ui43Math.BossMode.Open)
            {
                string t = s.openLeft > 0 ? Ui43Math.Tenths(s.openLeft) : "";
                if (!ReferenceEquals(t, b43TagTenths) || b43ShownMode != Ui43Math.BossMode.Open)
                {
                    b43TagTenths = t;
                    b43TagOpen = t.Length > 0 ? UnityTuning.Bar43OpenWord + " " + t : UnityTuning.Bar43OpenWord; // once per tenth, not per frame
                }
                tag = Target74Tag(b43TagOpen); // NIGHT74 B target74 (UiTarget74.cs): "OPEN 2.9 - ABOVE" and an edge chevron while the weak point is off screen (the tag as before with the row off or on screen)
                tagC = cyan;
                lineOn = true;
                lineFill = s.openLeft > 0 ? Mathf.Clamp01((float)s.openLeft / b43OpenMax) : 1f;
                lineC = cyan;
            }
            else if (mode == Ui43Math.BossMode.Locked)
            {
                tag = UnityTuning.Bar43LockedWord;
                tagC = steel;
                lineOn = s.stagger01 >= 0;
                lineFill = lineOn ? Mathf.Clamp01((float)s.stagger01) : 0f;
                lineC = bone;
            }
            else
            {
                lineOn = s.stagger01 >= 0;
                tag = Btext72.LabelWord(Btext72.On(cfg), lineOn, UnityTuning.Bar43StaggerWord); // NIGHT72 B btext72: STAGGER names a state, never the unfilled meter (the row off: the word as NIGHT71 draws it)
                tagC = muted;
                lineFill = lineOn ? Mathf.Clamp01((float)s.stagger01) : 0f;
                lineC = bone;
            }
            b43ShownMode = mode;
            if (mode != Ui43Math.BossMode.Open) Target74Off(); // NIGHT74 B target74: the chevron only while the bar reads OPEN
            if (tag != b43ShownTag) { b43ShownTag = tag; b43Tag.Set(tag); }
            b43Tag.Color = tagC;
            if (b43Line.gameObject.activeSelf != lineOn) { b43Line.gameObject.SetActive(lineOn); b43LineTrack.gameObject.SetActive(lineOn); }
            lineFill = Mathf.Round(lineFill * 200f) / 200f;
            if (lineOn && b43Line.fillAmount != lineFill) b43Line.fillAmount = lineFill;
            if (lineOn && b43Line.color != lineC) b43Line.color = lineC;

            // the hint (what hurts it now): resolved on a change only
            if (!ReferenceEquals(s.hint, b43ShownHintRaw))
            {
                b43ShownHintRaw = s.hint;
                string shown = string.IsNullOrEmpty(s.hint) ? "" : ResolveLesson25(s.hint, null);
                b43Hint.Set(shown);
                Bar43Probe.Hint = shown;
                if (hud47Built) Fight47Post(shown); // NIGHT47 F hud47: the hint as a fight line (UiHud47.Boss.cs)
            }
            b43Hint.Color = mode == Ui43Math.BossMode.Normal ? UiTheme.A(bone, UnityTuning.Bar43HintRestAlpha) : bone;

            // the open (the CRT pop, when the fight's bar first shows)
            float sx = 1f, sy = 1f;
            if (b43Age >= 0)
            {
                Ui43Math.Pop(b43Age, out sx, out sy);
                b43Age = b43Age > Ui43Math.PopTime ? -1 : b43Age + frameDt;
            }
            UiKit43.Scale(b43Root, sx, sy);

            Bar43Probe.Shown = true;
            Bar43Probe.Mode = mode;
            Bar43Probe.Fill = fq;
            Bar43Probe.After = aq;
            Bar43Probe.Stagger = lineOn && mode != Ui43Math.BossMode.Open ? lineFill : -1f;
            Bar43Probe.Leg = s.leg;
            Bar43Probe.Legs = s.legs;
            Bar43Probe.Name = s.name ?? "";
            Bar43Probe.Tag = tag;
        }
    }
}
