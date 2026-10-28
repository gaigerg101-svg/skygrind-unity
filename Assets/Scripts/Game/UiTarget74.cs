// NIGHT74 builder B (slate B2, design.n74.target74; Target74.cs has the rule and the reasons): the Ui half. UiBossBar43's Bar43Draw asks Target74Tag for the OPEN tag every frame the bar reads OPEN:
// with the row on and the open weak point off screen it returns the tag with the way ("OPEN 2.9 - ABOVE") and draws the cyan edge ARROW (round 2: a shaft and two head arms, dark outlined, about 130 px long: the
// B2 spot read round 1's two thin strokes as "a small cyan corner bracket"; its tip on the HUD layer's edge, inset
// UnityTuning.N74TargetInsetPx, pointing along the camera-local bearing of the point); otherwise it hides the chevron and returns the tag unchanged. Bar43Draw hides the chevron in every other
// mode, and Bar43Frame when the bar goes away. No allocation per frame after the first composed tag of a (tag, direction) pair.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform t74Root;
        Image[] t74Bars; // the shaft and the two head arms (cyan)
        string t74Dir, t74BaseTag, t74Composed;
        float t74Clock;

        /// <summary>The OPEN tag for this frame: `baseTag` as the bar made it, or with " - <way>" while the row is on and the weak point is off screen (and the chevron drawn).</summary>
        string Target74Tag(string baseTag)
        {
            if (!Target74.On(cfg)) { Target74Off(); return baseTag; }
            SurvivalSim sim = game.sim;
            Camera cam = game.cam;
            if (sim == null || cam == null || hud33 == null || !Target74.WeakPoint(sim, out Vec3 weak, out string boss)) { Target74Off(); return baseTag; }
            string dir = Target74.Where(cam, weak, out Vector3 local);
            if (dir == null) { Target74Off(); return baseTag; } // on screen: nothing drawn, the tag as the bar made it
            if (t74Root == null) Target74Build(hud33);
            double hw = hud33.rect.width * 0.5, hh = hud33.rect.height * 0.5;
            Target74Math.Edge(local.x, local.y, hw, hh, UnityTuning.N74TargetInsetPx, out double ex, out double ey, out double deg);
            t74Root.anchoredPosition = new Vector2((float)ex, (float)ey);
            t74Root.localRotation = Quaternion.Euler(0f, 0f, (float)deg);
            t74Clock += Time.unscaledDeltaTime;
            float swing = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * UnityTuning.N74TargetPulseHz * t74Clock);
            Color c = UiStyle33.Cyan;
            c.a = Mathf.Lerp(UnityTuning.N74TargetAlphaLo, UnityTuning.N74TargetAlphaHi, swing);
            for (int i = 0; i < t74Bars.Length; i++) t74Bars[i].color = c;
            if (!t74Root.gameObject.activeSelf) t74Root.gameObject.SetActive(true);
            if (!ReferenceEquals(baseTag, t74BaseTag) || !ReferenceEquals(dir, t74Dir))
            {
                bool first = !ReferenceEquals(dir, t74Dir);
                t74BaseTag = baseTag;
                t74Dir = dir;
                t74Composed = Target74Math.Tag(baseTag, dir, UnityTuning.N74TargetSeparator);
                Target74.Dir = dir;
                Target74.Boss = boss;
                Target74.Tag = t74Composed;
                if (first)
                {
                    Target74.TagPx = b43Tag != null ? b43Tag.FontPx : 0f;
                    Debug.Log("[target74] dir=" + dir + " boss " + boss + " | tag \"" + t74Composed + "\" at " + Target74.TagPx.ToString("0") + " px (min " + UnityTuning.N74TargetTextMinPx.ToString("0") + "), "
                        + Target74Math.Words(t74Composed) + " words | chevron at " + ex.ToString("0") + "," + ey.ToString("0") + " pointing " + deg.ToString("0") + " deg | row on");
                }
            }
            Target74.ChevronX = (float)ex;
            Target74.ChevronY = (float)ey;
            Target74.ChevronDeg = (float)deg;
            Target74.Drawn++;
            return t74Composed;
        }

        /// <summary>Nothing of target74 is drawn: the chevron hidden, the direction forgotten (the next off-screen frame logs again).</summary>
        void Target74Off()
        {
            if (t74Root != null && t74Root.gameObject.activeSelf) t74Root.gameObject.SetActive(false);
            t74Dir = null;
            t74BaseTag = null;
            Target74.Dir = null;
        }

        void Target74Build(RectTransform layer)
        {
            var mid = new Vector2(0.5f, 0.5f);
            t74Root = UiStyle33.Rect(layer, "Target 74 arrow", mid, mid, Vector2.zero, Vector2.zero);
            // THE ARROW (round 2, after the B2 spot read "a small cyan corner bracket"): a shaft along the pointing axis and two head arms at +- the half angle, all three meeting at the tip (the root's origin),
            // each bar pivoting on its outer end so a rotation about it folds it back from the tip; behind them the same three in the dark outline (longer and thicker by N74TargetOutlinePx a side)
            var back = UiStyle33.Rect(t74Root, "outline", mid, mid, Vector2.zero, Vector2.zero);
            var front = UiStyle33.Rect(t74Root, "cyan", mid, mid, Vector2.zero, Vector2.zero);
            Color ink = UiStyle33.Shadow;
            ink.a = UnityTuning.N74TargetOutlineAlpha;
            float o = UnityTuning.N74TargetOutlinePx, th = UnityTuning.N74TargetThickPx;
            float[] len = { UnityTuning.N74TargetShaftPx, UnityTuning.N74TargetArmPx, UnityTuning.N74TargetArmPx };
            float[] deg = { 0f, UnityTuning.N74TargetHalfAngleDeg, -UnityTuning.N74TargetHalfAngleDeg };
            var tip = new Vector2(1f, 0.5f);
            t74Bars = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                RectTransform hold = UiStyle33.Rect(back, "bar " + i, mid, mid, Vector2.zero, Vector2.zero);
                hold.localRotation = Quaternion.Euler(0f, 0f, deg[i]);
                UiStyle33.Line(hold, "outline", ink, mid, tip, new Vector2(o, 0f), new Vector2(len[i] + 2f * o, th + 2f * o)); // +o along the bar: the outline passes the tip and the tail by o
                RectTransform cy = UiStyle33.Rect(front, "bar " + i, mid, mid, Vector2.zero, Vector2.zero);
                cy.localRotation = Quaternion.Euler(0f, 0f, deg[i]);
                t74Bars[i] = UiStyle33.Line(cy, "cyan", UiStyle33.Cyan, mid, tip, Vector2.zero, new Vector2(len[i], th));
            }
            t74Root.gameObject.SetActive(false);
        }
    }
}
