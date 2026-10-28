// NIGHT26 builder B (contract glacier): the GAP METER, a bottom-centre bar over the boss bar (the lead's Ui.N26.cs hooks BuildN26HudHook / RenderN26HudHook): the gap to THE GLACIER
// in metres along the course, the 25 m hurt line marked, and the one-way rubber-band line ("IT NEVER SLOWS DOWN" / "IT SPEEDS UP WHEN YOU FALL BEHIND"). Colours: cyan while you
// can hurt it (gap <= the hurt line: player-usable, rule 8), the pale boundary tone between, red-orange in the soft-fail zone (the last stretch before the failAt gap: danger).
// Text 18 px. Shown only while THE GLACIER fights. Numbers: UnityTuning.Gap26 (Unity-only layout). Not compiled in the Editor by its author (dn-check only).
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform gap26;
        UiShape gap26Plate, gap26Track, gap26Fill, gap26Hurt;
        TextMeshProUGUI gap26Text, gap26Rubber;
        string gap26Line, gap26Sub;
        float gap26FillW = -1;

        partial void BuildN26HudHook()
        {
            gap26 = Hud17Rect(values, "Gap meter 26", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, UnityTuning.Gap26Bottom), new Vector2(UnityTuning.Gap26W, 56));
            gap26Plate = Hud17Shape(gap26, "Plate", UiTheme.Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 16), new Vector2(UnityTuning.Gap26W + 20, 116));
            gap26Track = Hud17Shape(gap26, "Track", UiTheme.InkSoft, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 14), new Vector2(UnityTuning.Gap26W, UnityTuning.Gap26BarH));
            gap26Fill = Hud17Shape(gap26, "Fill", UiTheme.Cyan, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 14), new Vector2(0, UnityTuning.Gap26BarH));
            gap26Hurt = Hud17Shape(gap26, "Hurt line", UiTheme.White, new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(4, UnityTuning.Gap26BarH + 8));
            foreach (UiShape s in new[] { gap26Plate, gap26Track, gap26Fill, gap26Hurt }) s.SetCuts(0, 0);
            gap26Text = Hud17Text(gap26, "Gap text", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.BottomLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 2), new Vector2(UnityTuning.Gap26W, 28), true);
            gap26Rubber = Hud17Text(gap26, "Rubber text", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.BottomLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 32), new Vector2(UnityTuning.Gap26W, 28), true); // its own row above the gap line (it overlapped it at 18 px)
            gap26.gameObject.SetActive(false);
        }

        partial void RenderN26HudHook(double frameDt)
        {
            if (gap26 == null) return;
            Glacier g = game.sim?.glacier26;
            bool on = g != null && g.Alive && game.flow == Flow.Playing;
            if (gap26.gameObject.activeSelf != on) gap26.gameObject.SetActive(on);
            if (!on) return;
            GlacierParams p = g.p;
            double gap = System.Math.Max(0, System.Math.Min(p.gapFail, g.gap));
            float w = UnityTuning.Gap26W;
            float fw = Mathf.Round((float)(gap / p.gapFail) * w * 2) / 2f;
            if (fw != gap26FillW)
            {
                gap26FillW = fw;
                gap26Fill.rectTransform.sizeDelta = new Vector2(fw, UnityTuning.Gap26BarH);
                gap26Fill.enabled = fw > 0.5f;
                gap26Hurt.rectTransform.anchoredPosition = new Vector2((float)(p.gapHurt / p.gapFail) * w, 10);
            }
            gap26Fill.color = g.Hurtable ? UiTheme.Cyan : gap >= p.gapFail - UnityTuning.Gap26SoftZone ? UiTheme.Danger : UiTheme.White;
            string line = "GAP " + Mathf.RoundToInt((float)g.gap) + " M   HURT AT " + Mathf.RoundToInt((float)p.gapHurt) + " M" + (gap >= p.gapFail - UnityTuning.Gap26SoftZone ? "   FALLING BEHIND" : "");
            if (line != gap26Line) { gap26Line = line; gap26Text.SetText(line); }
            string sub = g.RubberText;
            if (sub != gap26Sub) { gap26Sub = sub; gap26Rubber.SetText(sub); }
        }
    }
}
