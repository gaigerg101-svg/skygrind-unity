// NIGHT43 builder U1 (design.n43.counter43, a view row: no sim bit; NIGHT43.md U1, GG's G5: "add a small, unobtrusive UI element showing how many
// enemies remain in the current locked encounter. It should be tucked into a corner and visually integrated with the rest of the HUD"). With the row on
// (and hud33's layer drawing), top-left (the survival round line's corner; a campaign run has no round line):
//   - a red-orange blade accent (they are enemies), the count in the HERO face, "LEFT" in the label face, and a red-orange blade pip per enemy (<= 12);
//   - it opens with the CRT pop when a locked room starts fighting (SurvivalSim.N43RoomLeft >= 0, E1's count of the room's OWN enemies);
//   - a kill ticks it: the number punches and flashes white, the pip goes;
//   - at 0 the label reads CLEAR in cyan (the door is the player's now); when the count goes back to -1 (the door opened, or no locked fight) it closes.
// It reads only sim.N43RoomLeft (-1 = hidden): with E1's rooms43 off that is always -1, so nothing shows. Numbers: UnityTuning.Ui43.cs.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 counter43's statics for the reels.</summary>
    public static class Counter43Probe
    {
        public static bool Shown;
        /// <summary>The count drawn now (-1 none), the sim's count this frame, frames where they differed while shown, kills ticked, opens.</summary>
        public static int Drawn = -1, Read = -1, Mismatch, Ticks, Opens;
        /// <summary>The ui43 reel only: a count to draw in place of the sim's (int.MinValue: the sim's). The view's own frames, apart from E1's rooms.</summary>
        public static int Force = int.MinValue;

        public static void ResetCounters()
        {
            Shown = false;
            Drawn = Read = -1;
            Mismatch = Ticks = Opens = 0;
        }
    }

    public sealed partial class Ui
    {
        RectTransform c43Root;
        UiStyle33.Text33 c43Num, c43Label;
        Image c43Accent;
        Image[] c43Pips;
        int c43Shown = -1, c43PipsShown = -1;
        double c43OpenAge = -1, c43CloseAge = -1, c43TickAge = double.MaxValue;
        bool c43Clear;

        /// <summary>From BuildHud33Now with counter43 on.</summary>
        void BuildCounter43()
        {
            var tl = new Vector2(0, 1);
            float side = UnityTuning.Hud33SideMargin, edge = UnityTuning.Hud33EdgeMargin;
            c43Root = UiStyle33.Rect(hud33, Counter43Name, tl, tl, new Vector2(side, -edge), new Vector2(UnityTuning.Counter43W, UnityTuning.Counter43H));
            c43Accent = UiKit43.Blade(c43Root, "Accent", UiStyle33.Danger, tl, tl, Vector2.zero, new Vector2(UnityTuning.Counter43AccentW, UnityTuning.Counter43NumPx));
            float nx = UnityTuning.Counter43AccentW + UnityTuning.Counter43Gap;
            c43Num = UiStyle33.Label(c43Root, "Count", UnityTuning.Counter43NumPx, UiStyle33.Bone, UiStyle33.FaceNumber, TextAlignmentOptions.BottomRight, tl, tl, new Vector2(nx, 0), new Vector2(UnityTuning.Counter43NumW, UnityTuning.Counter43NumPx), false);
            c43Label = UiStyle33.Label(c43Root, "Left", UnityTuning.Counter43LabelPx, UiKit43.Muted, UiStyle33.FaceLabel, TextAlignmentOptions.BottomLeft, tl, tl, new Vector2(nx + UnityTuning.Counter43NumW + UnityTuning.Counter43Gap, -UnityTuning.Counter43LabelDrop), new Vector2(UnityTuning.Counter43LabelW, UnityTuning.Counter43LabelPx + 4));
            c43Pips = new Image[UnityTuning.Counter43MaxPips];
            for (int i = 0; i < c43Pips.Length; i++)
            {
                c43Pips[i] = UiKit43.Blade(c43Root, "Pip " + i, UiStyle33.Danger, tl, tl, new Vector2(nx + i * UnityTuning.Counter43PipPitch, -(UnityTuning.Counter43NumPx + UnityTuning.Counter43PipGap)), new Vector2(UnityTuning.Counter43PipW, UnityTuning.Counter43PipH));
                c43Pips[i].gameObject.SetActive(false);
            }
            c43Root.gameObject.SetActive(false);
            game.OnNewRun += () =>
            {
                c43Shown = c43PipsShown = -1;
                c43OpenAge = c43CloseAge = -1;
                c43TickAge = double.MaxValue;
                if (c43Root != null) c43Root.gameObject.SetActive(false);
                Counter43Probe.ResetCounters();
            };
        }

        public const string Counter43Name = "Counter 43";

        /// <summary>Every frame the HUD draws (from RenderHud33).</summary>
        void Counter43(double frameDt)
        {
            SurvivalSim sim = game.sim;
            int n = sim != null && game.flow == Flow.Playing ? (Counter43Probe.Force != int.MinValue ? Counter43Probe.Force : sim.N43RoomLeft) : -1;
            Counter43Probe.Read = n;
            bool vis = c43Root.gameObject.activeSelf;
            if (n >= 0)
            {
                if (!vis || c43CloseAge >= 0)
                {
                    c43Root.gameObject.SetActive(true);
                    c43OpenAge = 0;
                    c43CloseAge = -1;
                    c43Shown = -1;
                    Counter43Probe.Opens++;
                }
                if (n != c43Shown)
                {
                    if (c43Shown > n) { c43TickAge = 0; Counter43Probe.Ticks++; }
                    c43Shown = n;
                    c43Num.Set(Ui43Math.Num(n));
                    bool clear = n == 0;
                    if (clear != c43Clear || c43Label.Shown.Length == 0)
                    {
                        c43Clear = clear;
                        c43Label.Set(clear ? UnityTuning.Counter43ClearWord : UnityTuning.Counter43LeftWord);
                        c43Label.Color = clear ? UiStyle33.Cyan : UiKit43.Muted;
                        c43Accent.color = clear ? UiStyle33.Cyan : UiStyle33.Danger;
                    }
                }
            }
            else if (vis && c43CloseAge < 0) c43CloseAge = 0;

            if (!c43Root.gameObject.activeSelf) { Counter43Probe.Shown = false; Counter43Probe.Drawn = -1; return; }

            // the pips: one per enemy left (capped)
            int pips = Ui43Math.Pips(c43Shown);
            if (pips != c43PipsShown)
            {
                c43PipsShown = pips;
                for (int i = 0; i < c43Pips.Length; i++)
                {
                    bool on = i < pips && !c47Built; // NIGHT47 F hud47: the tiny pips go (UiHud47.cs draws a line instead)
                    if (c43Pips[i].gameObject.activeSelf != on) c43Pips[i].gameObject.SetActive(on);
                }
            }
            // the tick: the number punches and flashes white
            float sc = Ui43Math.Tick(c43TickAge);
            var scv = new Vector3(sc, sc, 1f);
            if (c43Num.rt.localScale != scv) c43Num.rt.localScale = scv;
            Color nc = c43TickAge < UnityTuning.Counter43FlashTime ? Color.white : c43Clear ? UiStyle33.Cyan : UiStyle33.Bone;
            c43Num.Color = nc;
            if (c47Built) Counter47(c43Shown, c43OpenAge == 0, frameDt); // NIGHT47 F hud47 (UiHud47.cs): LAST ONE, the draining line
            if (c43TickAge < double.MaxValue) c43TickAge = c43TickAge > UnityTuning.Ui43TickTime ? double.MaxValue : c43TickAge + frameDt;

            // the open / close (the CRT pop)
            float sx = 1f, sy = 1f;
            if (c43CloseAge >= 0)
            {
                Ui43Math.Close(c43CloseAge, out sx, out sy, out bool done);
                c43CloseAge += frameDt;
                if (done)
                {
                    c43Root.gameObject.SetActive(false);
                    c43CloseAge = c43OpenAge = -1;
                    c43Shown = -1;
                    Counter43Probe.Shown = false;
                    Counter43Probe.Drawn = -1;
                    return;
                }
            }
            else if (c43OpenAge >= 0)
            {
                Ui43Math.Pop(c43OpenAge, out sx, out sy);
                c43OpenAge = c43OpenAge > Ui43Math.PopTime ? -1 : c43OpenAge + frameDt;
            }
            UiKit43.Scale(c43Root, sx, sy);
            Counter43Probe.Shown = true;
            Counter43Probe.Drawn = c43CloseAge >= 0 ? -1 : c43Shown;
            if (c43CloseAge < 0 && n >= 0 && c43Shown != n) Counter43Probe.Mismatch++;
        }
    }
}
