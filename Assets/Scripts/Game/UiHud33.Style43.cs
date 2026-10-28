// NIGHT43 builder U1 round 3 (with hud43; the lead's round-3 card: "the style meter / rank (style42's) in the same language so the right side of the HUD
// matches the new health bar"). With hud43 and style42 on, top-right:
//   - style42's 160 x 3 meter line becomes EIGHT BLADE CELLS (the health bar's 12 deg cells, smaller) in a bracket: each cell fills with the band's share
//     (style42's eased line), coloured as style42 colours its line (cyan; white on a gain; red-orange on a hit; fading out with it), the empty cells a
//     MUTED track. The letter, its punch / drop / shake, the combo, the name and the award stay style42's.
//   - the points in GOLD (U1.md 1.2: points only), the HERO face as before.
// The line itself stays (hidden) and style42 keeps writing it, so its reel numbers (Style42.LineShown) are unchanged. Off (hud43 or style42 off):
// nothing here is built. Numbers: UnityTuning.Ui43.cs (Style43*).
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform st43Root;
        Image[] st43Track, st43Fill, st43Bracket;
        float st43Shown = -2f;
        Color st43Col;

        /// <summary>From BuildHud43 when style42 is built.</summary>
        void BuildStyle43()
        {
            Color gold = UiTheme.Ready ? UiTheme.Gold : new Color(1f, 0.76f, 0.24f, 1f), muted = UiKit43.Muted;
            h33Points.Color = gold;
            Hud33Layout.Box rl = Hud33Layout.RankLine;
            var tr = new Vector2(1, 1);
            var bl = Vector2.zero;
            int n = UnityTuning.Style43Cells;
            float w = n * UnityTuning.Style43CellPitch - (UnityTuning.Style43CellPitch - UnityTuning.Style43CellW);
            st43Root = UiStyle33.Rect(hud33, "Style meter 43", tr, tr, new Vector2((float)rl.x, (float)rl.y), new Vector2(w, UnityTuning.Style43CellH));
            st43Track = new Image[n];
            st43Fill = new Image[n];
            for (int i = 0; i < n; i++)
            {
                var at = new Vector2(i * UnityTuning.Style43CellPitch, 0);
                var size = new Vector2(UnityTuning.Style43CellW, UnityTuning.Style43CellH);
                st43Track[i] = UiKit43.Blade(st43Root, "Track " + i, UiTheme.A(muted, UnityTuning.Hud43TrackAlpha), bl, bl, at, size);
                st43Fill[i] = UiKit43.BladeFill(st43Root, "Fill " + i, UiStyle33.Cyan, bl, bl, at, size);
            }
            st43Bracket = UiKit43.Bracket(st43Root, "Bracket", muted, UnityTuning.Style43BracketPad);
            st43Root.gameObject.SetActive(false);
            if (h33RankLine != null) h33RankLine.enabled = false; // style42 still writes it (its reel numbers); the cells draw it
            Transform track = h33RankLineRt != null ? h33RankLineRt.Find("Track") : null;
            if (track != null && track.TryGetComponent(out Image ti)) ti.enabled = false;
        }

        /// <summary>From Style42Draw (every frame the HUD draws): the cells from style42's eased line and its colour.</summary>
        void Style43Draw(bool drawn, float line, Color c)
        {
            if (st43Root.gameObject.activeSelf != drawn) st43Root.gameObject.SetActive(drawn);
            if (!drawn) { st43Shown = -2f; return; }
            float share = Mathf.Round(Mathf.Clamp01(line) * 200f) / 200f;
            if (share == st43Shown && c == st43Col) return;
            st43Shown = share;
            st43Col = c;
            int n = st43Fill.Length;
            for (int i = 0; i < n; i++)
            {
                float f = Hud43Math.Cell(share, i, n);
                if (st43Fill[i].fillAmount != f) st43Fill[i].fillAmount = f;
                st43Fill[i].color = c;
            }
            Color bc = Color.Lerp(UiKit43.Muted, c, UnityTuning.Style43BracketTint);
            bc.a = c.a;
            for (int i = 0; i < st43Bracket.Length; i++) st43Bracket[i].color = bc;
        }
    }
}
