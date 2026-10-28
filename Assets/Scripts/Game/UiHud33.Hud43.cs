// NIGHT43 builder U1 (design.n43.hud43, a view row: no sim bit; NIGHT43.md U1, GG's G6: "The health bar specifically should look much cooler and more
// intentional"; the lead's n42-r4 frame: "120" in the display italic over 10 slash ticks with two cyan underline segments). With the row on (and hud33's
// layer drawing), bottom-left, in the dusk instrument's language (Research/2026-10-16-wave43/U1.md section 1):
//   - the number in the HERO face at 56 px, left; beside it THE BAR: ten blade cells (12 deg) in a bracket, a notch over each 25 %, a scanline overlay;
//   - a HIT: the cells keep a HOT after-image of what was lost that waits 0.15 s then drains at (gap x 5 + 0.5) per s (bosses-web A2); the bracket (never
//     the fill) pulses red-orange; the number splits into a cyan and a red-orange copy 3 px apart (a CRT's misconvergence) for 0.18 s and counts down;
//   - a HEAL (+2 % of the max or more in a frame): the cells it filled glow cyan and fade; the bracket goes cyan; the number pops;
//   - LOW (hud33's 25 %): the fill and the number turn red-orange and the bracket breathes at 1.2 Hz (no screen-wide tint);
//   - the stamina bars move under the bar, as wide as it.
// hud33's ticks hide; health42 (if on) does not draw while hud43 does. Also (T-64): a 0-point style award is not drawn (UiHud33.Style42.cs).
// Off (the row off, -skygrindOff43 or any older switch): nothing here is built and hud33 / health42 draw as before. Numbers: UnityTuning.Ui43.cs.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 hud43's statics for the reels (the ui43 scenario prints them).</summary>
    public static class Hud43
    {
        public static bool Live;
        public static int Hits, Heals, LitCells, AfterCells, GlowCells, NumberShown;
        public static float Frac = -1f, After = -1f, SplitNow;

        public static void ResetCounters()
        {
            Hits = Heals = LitCells = AfterCells = GlowCells = NumberShown = 0;
            Frac = After = -1f;
            SplitNow = 0f;
        }
    }

    /// <summary>hud43's pure rules (EditMode: Ui43Tests).</summary>
    public static class Hud43Math
    {
        /// <summary>Cell i's fill (0..1) of n cells for a share: the cells fill left to right, the last one partly.</summary>
        public static float Cell(float share, int i, int n) => Mathf.Clamp01(share * n - i);

        /// <summary>The cells a share touches (a partly filled cell counts).</summary>
        public static int Cells(float share, int n) => Mathf.Clamp(Mathf.CeilToInt(share * n - 1e-4f), 0, n);

        /// <summary>The number shown: the health rounded up, never over the max, never more than 4 digits (the box's width; a reel's god mode is 1e6).</summary>
        public static int Shown(double hp, double max)
        {
            double v = Math.Ceiling(double.IsNaN(hp) ? 0 : hp);
            if (max > 0) v = Math.Min(v, Math.Ceiling(max));
            return (int)Math.Max(0, Math.Min(UnityTuning.Hud43NumCap, v));
        }
    }

    public sealed partial class Ui
    {
        // --- the five NIGHT43 view rows (read once per session; hud33 must draw for any of them to show) ---
        bool hud43On, counter43On, teach43On, bossbar43On, screens43On, n43RowsRead;

        void ReadRows43()
        {
            if (n43RowsRead || cfg == null) return;
            n43RowsRead = true;
            hud43On = N43View.RowOn(cfg, "hud43");
            counter43On = N43View.RowOn(cfg, "counter43");
            teach43On = N43View.RowOn(cfg, "teach43");
            bossbar43On = N43View.RowOn(cfg, "bossbar43");
            screens43On = N43View.RowOn(cfg, "screens43");
        }

        bool Hud43On { get { ReadRows43(); return hud43On; } }
        bool Counter43On { get { ReadRows43(); return counter43On; } }
        bool Teach43On { get { ReadRows43(); return teach43On; } }
        bool Bossbar43On { get { ReadRows43(); return bossbar43On; } }
        bool Screens43On { get { ReadRows43(); return screens43On; } }

        // --- hud43: the health bar ---
        bool hp43Built;
        RectTransform hp43Bar;
        Image[] hp43Track, hp43After, hp43Fill, hp43Glow, hp43Bracket;
        Image[] hp43Notch;
        TextMeshProUGUI hp43SplitA, hp43SplitB;
        float hp43Last = -1f, hp43AfterLv = -1f, hp43HealLo, hp43HealHi;
        double hp43Wait, hp43Num = double.NaN, hp43HitAge = double.MaxValue, hp43HealAge = double.MaxValue, hp43Clock;
        int hp43ShownNum = int.MinValue;
        Vector2 hp43NumHome;

        /// <summary>From BuildHud33Now with hud43 on (after hud33's health and stamina exist).</summary>
        void BuildHud43()
        {
            hp43Built = true;
            Color bone = UiStyle33.Bone, muted = UiKit43.Muted;
            Vector2 bl = Vector2.zero;
            int n = UnityTuning.Hud43Cells;
            float side = UnityTuning.Hud33SideMargin, edge = UnityTuning.Hud33EdgeMargin;
            float barX = side + UnityTuning.Hud43NumW + UnityTuning.Hud43NumGap;
            float barW = n * UnityTuning.Hud43CellPitch - (UnityTuning.Hud43CellPitch - UnityTuning.Hud43CellW);
            float barY = edge + UnityTuning.Hud33StaminaH + UnityTuning.Hud43StaminaGap;

            // hud33's ticks hide (the bar replaces them); the number moves left of the bar and grows to the HERO size
            RectTransform ticks = h33HpTicks[0].rectTransform.parent as RectTransform;
            ticks.gameObject.SetActive(false);
            // round 2 (the lead's n43-r1 h-hit / t-hold: "88" and god mode's "1000000" ran into the cells): the number hangs off its box's RIGHT edge (the
            // punch grows it leftwards, away from the bar), the box is 4 digits wide, the shown value is clamped (Hud43Math.Shown)
            hp43NumHome = new Vector2(side + UnityTuning.Hud43NumW, edge); // its bottom on the stamina's bottom (the 54 px margin)
            h33Health.rt.pivot = new Vector2(1, 0);
            h33Health.rt.anchoredPosition = hp43NumHome;
            h33Health.rt.sizeDelta = new Vector2(UnityTuning.Hud43NumW, UnityTuning.Hud43NumPx + 8);
            h33Health.main.fontSize = h33Health.back.fontSize = UnityTuning.Hud43NumPx;
            h33Health.main.alignment = h33Health.back.alignment = TextAlignmentOptions.BottomRight;
            // the chroma split's two copies, behind the number (between its shadow and its face)
            hp43SplitA = SplitCopy43(h33Health, "Split cyan");
            hp43SplitB = SplitCopy43(h33Health, "Split danger");

            // the stamina (the boost charges: kept, PRINCIPLES 1): under the cells, inside the bracket, leaning with them (round 2: hud33's 20 deg slash
            // bars read as "two cyan underline segments" under the bar)
            RectTransform stam = h33StamFill[0].rectTransform.parent as RectTransform;
            stam.anchoredPosition = new Vector2(barX, edge);
            for (int i = 0; i < h33StamFill.Length; i++)
            {
                UiSkew.Apply(h33StamFill[i], UiKit43.SlantTan);
                UiSkew.Apply(h33StamTrack[i], UiKit43.SlantTan);
            }

            hp43Bar = UiStyle33.Rect(hud33, "Health 43", bl, bl, new Vector2(barX, barY), new Vector2(barW, UnityTuning.Hud43CellH));
            hp43Track = new Image[n];
            hp43After = new Image[n];
            hp43Fill = new Image[n];
            hp43Glow = new Image[n];
            for (int i = 0; i < n; i++)
            {
                var at = new Vector2(i * UnityTuning.Hud43CellPitch, 0);
                var size = new Vector2(UnityTuning.Hud43CellW, UnityTuning.Hud43CellH);
                hp43Track[i] = UiKit43.Blade(hp43Bar, "Track " + i, UiTheme.A(muted, UnityTuning.Hud43TrackAlpha), bl, bl, at, size);
                hp43After[i] = UiKit43.BladeFill(hp43Bar, "After " + i, UiStyle33.Danger, bl, bl, at, size); // round 2: red-orange (a white after-image vanished against the bone fill)
                hp43Fill[i] = UiKit43.BladeFill(hp43Bar, "Fill " + i, bone, bl, bl, at, size);
                hp43Glow[i] = UiKit43.BladeFill(hp43Bar, "Glow " + i, UiTheme.A(UiStyle33.Cyan, 0f), bl, bl, at, size);
            }
            // the scanlines ride each cell (never the gaps between: no striped box on the world)
            for (int i = 0; i < n; i++) UiSkew.Apply(UiKit43.Scan(hp43Fill[i].rectTransform, "Scan", UnityTuning.Ui43ScanAlpha), UiKit43.SlantTan);
            hp43Notch = new Image[3];
            for (int q = 0; q < 3; q++)
            {
                float x = Mathf.Round((q + 1) * 0.25f * n * UnityTuning.Hud43CellPitch - (UnityTuning.Hud43CellPitch - UnityTuning.Hud43CellW) * 0.5f);
                hp43Notch[q] = UiStyle33.Line(hp43Bar, "Notch " + q, muted, new Vector2(0, 1), new Vector2(0.5f, 0), new Vector2(x + UnityTuning.Hud43CellH * UiKit43.SlantTan * 0.5f, UnityTuning.Hud43NotchGap), new Vector2(UnityTuning.Ui43BracketW, UnityTuning.Hud43NotchH));
            }
            hp43Bracket = UiKit43.Bracket(hp43Bar, "Bracket", muted, UnityTuning.Hud43BracketPad);
            // the bracket holds the stamina too (it hangs under the cells)
            var bh = hp43Bracket[0].rectTransform.parent as RectTransform;
            bh.offsetMin = new Vector2(-UnityTuning.Hud43BracketPad, -(UnityTuning.Hud43BracketPad + UnityTuning.Hud43StaminaGap + UnityTuning.Hud33StaminaH));
            BuildFall43(); // NIGHT43 U1 (round 2): E1's fallReturn43 blink (UiHud33.Fall43.cs)
            if (s42Rank != null) BuildStyle43(); // NIGHT43 U1 (round 3): style42's line as blade cells, the points gold (UiHud33.Style43.cs)
            game.OnNewRun += () =>
            {
                hp43Last = hp43AfterLv = -1f;
                hp43Wait = 0;
                hp43Num = double.NaN;
                hp43HitAge = hp43HealAge = double.MaxValue;
                hp43ShownNum = int.MinValue;
                Hud43.ResetCounters();
            };
        }

        static TextMeshProUGUI SplitCopy43(UiStyle33.Text33 t, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(t.rt, false);
            UiKit43.Stretch(rt);
            rt.SetSiblingIndex(t.main.rectTransform.GetSiblingIndex()); // under the face, over the shadow
            var c = go.GetComponent<TextMeshProUGUI>();
            c.font = t.main.font;
            c.fontSize = t.main.fontSize;
            c.alignment = t.main.alignment;
            c.textWrappingMode = TextWrappingModes.NoWrap;
            c.overflowMode = TextOverflowModes.Overflow;
            c.raycastTarget = false;
            c.richText = false;
            c.color = new Color(1, 1, 1, 0);
            return c;
        }

        /// <summary>Health33's hud43 branch (every frame the HUD draws).</summary>
        void Health43Draw(Health h, double frameDt)
        {
            hp43Clock += frameDt;
            Fall43Draw(frameDt); // round 2: the fallReturn43 blink
            int n = hp43Fill.Length;
            float frac = Mathf.Clamp01((float)(h.hp / Math.Max(1, h.max)));
            if (hp43Last < 0f) { hp43Last = frac; hp43AfterLv = frac; }
            if (frac < hp43Last - 1e-4f) { hp43HitAge = 0; Hud43.Hits++; }
            if (frac >= hp43Last + UnityTuning.Health42HealMin)
            {
                hp43HealLo = hp43HealAge < UnityTuning.Hud43HealTime ? Math.Min(hp43HealLo, hp43Last) : hp43Last;
                hp43HealHi = frac;
                hp43HealAge = 0;
                Hud43.Heals++;
            }
            hp43AfterLv = Ui43Math.After(hp43AfterLv, hp43Last, frac, ref hp43Wait, frameDt);
            hp43Last = frac;

            bool low = H52Live ? Hud52Math.Low(frac) : frac <= UnityTuning.Hud33LowFrac; // NIGHT52 hud52: red-orange below 30 % (UiHud52.cs); off: hud33's 25 %
            float hitK = hp43HitAge < UnityTuning.Hud43HitTime ? (float)(1 - hp43HitAge / UnityTuning.Hud43HitTime) : 0f;
            float healK = hp43HealAge < UnityTuning.Hud43HealTime ? (float)(1 - hp43HealAge / UnityTuning.Hud43HealTime) : 0f;
            float breathe = low ? 0.5f + 0.5f * Mathf.Cos((float)(hp43Clock * UnityTuning.Ui43BreatheHz * 2 * Math.PI)) : 0f;
            Color bone = UiStyle33.Bone, cyan = UiStyle33.Cyan, danger = UiStyle33.Danger, muted = UiKit43.Muted;

            // the number: counts toward the health; red-orange low; a tick punch on a hit / heal; the split on a hit
            hp43Num = Health42Math.EaseNumber(hp43Num, h.hp + h.over, frameDt); // NIGHT47 F overheal47: + the overheal (0 with the row off)
            int num = Hud43Math.Shown(hp43Num, h.max + (game.sim != null ? game.sim.Overheal47Max : 0));
            if (num != hp43ShownNum)
            {
                hp43ShownNum = num;
                string s = Ui43Math.Num(num);
                h33Health.Set(s);
                hp43SplitA.SetText(s);
                hp43SplitB.SetText(s);
            }
            Color nc = low ? danger : healK > 0f ? Color.Lerp(bone, cyan, healK) : bone;
            h33Health.Color = nc;
            h33Health.Alpha = 1f;
            double punchAge = Math.Min(hp43HitAge, hp43HealAge);
            float sc = Ui43Math.Tick(punchAge);
            var scv = new Vector3(sc, sc, 1f);
            if (h33Health.rt.localScale != scv) h33Health.rt.localScale = scv;
            if (h33Health.rt.anchoredPosition != hp43NumHome) h33Health.rt.anchoredPosition = hp43NumHome;
            float split = hp43HitAge < UnityTuning.Ui43SplitTime ? (float)(1 - hp43HitAge / UnityTuning.Ui43SplitTime) : 0f;
            split = Mathf.Round(split * 20f) / 20f;
            Hud43.SplitNow = split;
            hp43SplitA.color = UiTheme.A(cyan, split);
            hp43SplitB.color = UiTheme.A(danger, split);
            float off = UnityTuning.Ui43SplitPx * (split > 0 ? 1f : 0f);
            hp43SplitA.rectTransform.anchoredPosition = new Vector2(-off, 0);
            hp43SplitB.rectTransform.anchoredPosition = new Vector2(off, 0);

            // the cells: track, the HOT after-image, the fill (bone; red-orange low), the heal's cyan glow
            Color fill = low ? Color.Lerp(danger, UiTheme.A(danger, UnityTuning.Hud43LowDim), breathe) : bone;
            int glowLo = Hud43Math.Cells(hp43HealLo, n), glowHi = Hud43Math.Cells(hp43HealHi, n);
            int afterN = 0, glowN = 0;
            if (hp47Built) Health47Bar(frac, Over47Share(h), frameDt); // NIGHT47 F hud47 (UiHud47.cs): one continuous bar in place of the cells
            else
            for (int i = 0; i < n; i++)
            {
                float f = Hud43Math.Cell(frac, i, n), a = Hud43Math.Cell(hp43AfterLv, i, n);
                f = Mathf.Round(f * 50f) / 50f;
                a = Mathf.Round(a * 50f) / 50f;
                if (hp43Fill[i].fillAmount != f) hp43Fill[i].fillAmount = f;
                if (hp43After[i].fillAmount != a) hp43After[i].fillAmount = a;
                if (a > f) afterN++;
                if (hp43Fill[i].color != fill) hp43Fill[i].color = fill;
                bool glow = healK > 0f && i >= glowLo - 1 && i < glowHi;
                if (glow) glowN++;
                Color gc = UiTheme.A(cyan, glow ? Mathf.Round(healK * 50f) / 50f : 0f);
                if (hp43Glow[i].color != gc) hp43Glow[i].color = gc;
                hp43Glow[i].fillAmount = f;
            }
            // the bracket: MUTED at rest; red-orange on a hit (decaying), cyan on a heal, breathing red-orange when low
            Color bc = muted;
            if (low) bc = Color.Lerp(UiTheme.A(danger, UnityTuning.Hud43LowDim), danger, breathe);
            if (healK > 0f) bc = Color.Lerp(bc, cyan, healK);
            if (hitK > 0f) bc = Color.Lerp(bc, danger, hitK);
            for (int i = 0; i < hp43Bracket.Length; i++) if (hp43Bracket[i].color != bc) hp43Bracket[i].color = bc;

            if (hp43HitAge < double.MaxValue) hp43HitAge = hp43HitAge > UnityTuning.Hud43HitTime + UnityTuning.Ui43TickTime ? double.MaxValue : hp43HitAge + frameDt;
            if (hp43HealAge < double.MaxValue) hp43HealAge = hp43HealAge > UnityTuning.Hud43HealTime + UnityTuning.Ui43TickTime ? double.MaxValue : hp43HealAge + frameDt;
            h33ShownLit = -1; // hud33's tracker: redraw from scratch if the row is ever off mid-session
            Hud43.Live = true;
            Hud43.Frac = frac;
            Hud43.After = hp43AfterLv;
            Hud43.LitCells = Hud43Math.Cells(frac, n);
            Hud43.AfterCells = afterN;
            Hud43.GlowCells = glowN;
            Hud43.NumberShown = num;
            if (h52Built) Health52Draw(h, frac, frameDt); // NIGHT52 U hud52 (UiHud52.cs): the panel, the hit shake, the heal pulse, the palette's colours (last: over this frame's)
        }
    }
}
