// NIGHT24 (builder F, contract reticle-arc; design.n24.reticleArc): the cooldown arc under the crosshair. UNCOMPILED and UNRUN when written (no
// Editor in the builder's worktree). GG: "a little bar or like a curved bar ... should not be in the way".
// A thin curved bar (a smile: the ends higher than the middle), about 140 x 5 px at 1080p, 28-42 px under the crosshair's centre, three groups:
//   ARCLINE   one cell that FILLS left to right as the yank's 15 s cooldown runs out (1 - Arcline21.YankShare); drawn only once the gadget is owned.
//             Its NO TARGET look (the crosshair has nothing to hook) is the cell in the muted pale (never red). The ARCLINE chip of NIGHT22 is
//             hidden while this is on (UiArcline22.cs reads Arc24Moved): the chip's READY / COOLDOWN / NO TARGET live here.
//   DASH      the stamina: one cell per pip (the web's pips) or one cell for the bar (design.stamina), filled by StaminaHud.Read.
//   FIST      three cells (the notches at the fist's tiers 33 / 66 / 100 %), each filled by the charge (Fist24.charge); drawn only with chargeFist on.
// Never text, never red (a group is cyan; a spent one is the dark track), never magenta. At rest the bar is at Arc24IdleAlpha (0.2); a group that
// is recharging, or a fist holding a charge, is at Arc24ChargeAlpha; the moment a group becomes READY it flashes (white to cyan, 120 ms in and 130 ms
// out = 250 ms) and settles back. It fades in on its first show (no pop-in). The one persistent element allowed in the central 40 % (D-14).
// Wiring (the lead, Ui.N24.cs): BuildN24Hud calls BuildReticleArc24(); RenderN24Hud calls RenderReticleArc24(frameDt).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>The bar as one uGUI Graphic: cells (track + fill) along a circle's arc, placed by x in px from the bar's centre.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiArc24 : MaskableGraphic
    {
        public struct Cell
        {
            public float x0, x1; // px from the centre, left to right
            public float fill; // 0..1
            public Color track, bar; // the track under the fill, the fill (both with their alpha)
        }

        public readonly List<Cell> cells = new List<Cell>(10);
        public float radius = UnityTuning.Arc24Radius, thick = UnityTuning.Arc24Thick;

        /// <summary>The cells changed: rebuild the mesh.</summary>
        public void Changed() => SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            // the circle's centre sits `radius` above the lowest point of the outer edge, which is the rect's bottom
            Vector2 c = new Vector2(r.center.x, r.yMin + radius);
            foreach (Cell cell in cells)
            {
                Band(vh, c, cell.x0, cell.x1, cell.track);
                if (cell.fill > 0.001f) Band(vh, c, cell.x0, cell.x0 + (cell.x1 - cell.x0) * Mathf.Clamp01(cell.fill), cell.bar);
            }
        }

        void Band(VertexHelper vh, Vector2 c, float xa, float xb, Color col)
        {
            if (xb <= xa || col.a <= 0.001f) return;
            int n = Mathf.Max(1, Mathf.CeilToInt((xb - xa) / 6f)); // a vertex pair every ~6 px
            int start = vh.currentVertCount;
            Color32 c32 = col;
            for (int k = 0; k <= n; k++)
            {
                float x = Mathf.Lerp(xa, xb, (float)k / n);
                float phi = Mathf.Asin(Mathf.Clamp(x / radius, -0.99f, 0.99f));
                float s = Mathf.Sin(phi), co = Mathf.Cos(phi);
                vh.AddVert(c + new Vector2(radius * s, -radius * co), c32, Vector2.zero); // outer (the lower edge)
                vh.AddVert(c + new Vector2((radius - thick) * s, -(radius - thick) * co), c32, Vector2.zero); // inner
            }
            for (int k = 0; k < n; k++)
            {
                int i = start + 2 * k;
                vh.AddTriangle(i, i + 1, i + 3);
                vh.AddTriangle(i, i + 3, i + 2);
            }
        }
    }

    public sealed partial class Ui
    {
        RectTransform arc24Root;
        UiArc24 arc24;
        CanvasGroup arc24Group;
        double arc24Enter = -1;
        readonly double[] arc24FlashAge = { -1, -1, -1 };
        readonly bool[] arc24WasReady = new bool[3], arc24Ready = new bool[3], arc24Present = new bool[3];
        readonly float[] arc24Shown = new float[24];
        int arc24ShownCount = -1;
        StaminaHud.Info arc24Stamina;

        /// <summary>What the arc draws now, for the reel's scenario `reticle` (the Ui instance is private to SurvivalGame).</summary>
        public static bool Arc24Built, Arc24Moved;
        public static readonly float[] Arc24FillNow = new float[3]; // ARCLINE, DASH, FIST: 0..1
        public static readonly float[] Arc24AlphaNow = new float[3];
        public static float Arc24FlashNow; // the largest flash 0..1 across the groups
        public static int Arc24FlashCount; // ready flashes since the last run's start

        void BuildReticleArc24()
        {
            Arc24Built = true;
            // the rect: the bar's width, the sag and the thickness; its top edge sits Arc24EndsBelow under the crosshair's centre
            float sag = UnityTuning.Arc24MidBelow - UnityTuning.Arc24EndsBelow;
            float h = sag + UnityTuning.Arc24Thick + 2f;
            arc24Root = Hud17Rect(sight, "Reticle arc 24", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0, -UnityTuning.Arc24EndsBelow), new Vector2(UnityTuning.Arc24Width + 8f, h));
            arc24Group = arc24Root.gameObject.AddComponent<CanvasGroup>();
            arc24Group.blocksRaycasts = arc24Group.interactable = false;
            arc24Group.alpha = 0;
            var go = new GameObject("Arc", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiArc24));
            var rt = (RectTransform)go.transform;
            rt.SetParent(arc24Root, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = arc24Root.sizeDelta;
            arc24 = go.GetComponent<UiArc24>();
            arc24.raycastTarget = false;
            arc24Root.gameObject.SetActive(false);
            game.OnNewRun += () =>
            {
                arc24Enter = -1;
                Arc24FlashCount = 0;
                for (int i = 0; i < 3; i++)
                {
                    arc24FlashAge[i] = -1;
                    arc24WasReady[i] = false;
                }
            };
        }

        /// <summary>The flash's share 0..1 `age` s in: up over Arc24FlashIn, down over Arc24FlashOut, then 0.</summary>
        static float Arc24Flash(double age)
        {
            if (age < 0) return 0f;
            if (age < UnityTuning.Arc24FlashIn) return (float)(age / UnityTuning.Arc24FlashIn);
            double out_ = age - UnityTuning.Arc24FlashIn;
            return out_ < UnityTuning.Arc24FlashOut ? 1f - (float)(out_ / UnityTuning.Arc24FlashOut) : 0f;
        }

        void RenderReticleArc24(double frameDt)
        {
            if (arc24Root == null) return;
            SurvivalSim sim = game.sim;
            bool on = sim != null && sim.n24.reticleArc && game.flow == Flow.Playing;
            Arc24Moved = on; // UiArcline22.cs hides its chip while this draws the same three looks
            if (arc24Root.gameObject.activeSelf != on)
            {
                arc24Root.gameObject.SetActive(on);
                if (on) arc24Enter = 0; // M9: it fades in, never whole on its first frame
            }
            if (!on) return;

            // --- the three groups' values ----------------------------------------------------------------------------------------------
            Arcline21 a = sim.arcline21;
            bool arcOwned = a != null && sim.n21.arcline && a.Owned;
            float arcFill = arcOwned ? (float)(1.0 - a.YankShare) : 0f;
            bool noTarget = false;
            if (arcOwned && a.YankSteps == 0)
            {
                var look = new InputFrame { yaw = sim.input.Yaw, pitch = sim.input.Pitch };
                Arcline21Kind k = sim.mover.rope.phase == RopePhase.None ? a.Reticle(look) : Arcline21Kind.Surface;
                noTarget = Arcline22Chip.Resolve(true, 0, k) == Arcline22Look.NoTarget;
            }
            StaminaHud.Read(game, ref arc24Stamina);
            float dashFill = arc24Stamina.valid && arc24Stamina.max > 0 ? Mathf.Clamp01((float)(arc24Stamina.fill / arc24Stamina.max)) : 0f;
            Fist24 fist = sim.fist24;
            bool fistOn = fist != null && fist.Active && !sim.n27.pulse27; // NIGHT27 pulse27: the glove's five pips (UiWeapons.N27.cs) replace the fist's three cells
            float fistFill = fistOn ? (float)fist.charge : 0f;
            Arc24FillNow[0] = arcFill;
            Arc24FillNow[1] = dashFill;
            Arc24FillNow[2] = fistFill;

            // --- ready: a group that just became usable flashes once ------------------------------------------------------------------------
            bool[] ready = arc24Ready, present = arc24Present; // fields: no allocation per frame
            ready[0] = arcOwned && a.YankSteps == 0;
            ready[1] = dashFill >= 0.999f;
            ready[2] = fistOn && fist.Tier > 0;
            present[0] = arcOwned;
            present[1] = false; // the dash refills every few seconds: it does not flash (the ARCLINE and the fist do)
            present[2] = fistOn;
            float flashMax = 0f;
            for (int i = 0; i < 3; i++)
            {
                if (present[i] && ready[i] && !arc24WasReady[i] && Time.frameCount > 2 && arc24Enter < 0)
                {
                    arc24FlashAge[i] = 0;
                    Arc24FlashCount++;
                    FeelProbe.Event(UnityTuning.Arc24ProbeReady);
                }
                arc24WasReady[i] = present[i] && ready[i];
                if (arc24FlashAge[i] >= 0)
                {
                    arc24FlashAge[i] += frameDt;
                    if (arc24FlashAge[i] > UnityTuning.Arc24FlashIn + UnityTuning.Arc24FlashOut) arc24FlashAge[i] = -1;
                }
                flashMax = Mathf.Max(flashMax, Arc24Flash(arc24FlashAge[i]));
            }
            Arc24FlashNow = flashMax;

            // --- the cells ------------------------------------------------------------------------------------------------------------------
            float idle = UnityTuning.Arc24IdleAlpha, busy = UnityTuning.Arc24ChargeAlpha;
            Color cy = UiTheme.Cyan, ink = UiTheme.InkSolid, pale = UiTheme.Muted;
            var cells = arc24.cells;
            cells.Clear();
            float left = -UnityTuning.Arc24Width * 0.5f;
            float x = left;
            float gap = UnityTuning.Arc24GroupGap, cg = UnityTuning.Arc24CellGap;
            // ARCLINE
            if (arcOwned)
            {
                float fl = Arc24Flash(arc24FlashAge[0]);
                float alpha = a.YankSteps > 0 ? busy : idle;
                Color bar = noTarget ? UiTheme.A(pale, busy) : Color.Lerp(UiTheme.A(cy, alpha), UiTheme.A(Color.white, UnityTuning.Arc24FlashAlpha), fl);
                if (fl > 0) bar.a = Mathf.Lerp(alpha, UnityTuning.Arc24FlashAlpha, fl);
                Arc24AlphaNow[0] = bar.a;
                cells.Add(new UiArc24.Cell { x0 = x, x1 = x + UnityTuning.Arc24WidthArcline, fill = arcFill, track = UiTheme.A(ink, idle), bar = bar });
            }
            else Arc24AlphaNow[0] = 0f;
            x += UnityTuning.Arc24WidthArcline + gap;
            // DASH: one cell per pip (the web's pips) or one cell for the bar
            if (arc24Stamina.valid)
            {
                int n = arc24Stamina.max > 1.5 ? Mathf.Max(1, Mathf.RoundToInt((float)arc24Stamina.max)) : 1;
                float cw = (UnityTuning.Arc24WidthDash - cg * (n - 1)) / n;
                float fl = Arc24Flash(arc24FlashAge[1]);
                float alpha = dashFill >= 0.999f ? idle : busy;
                for (int i = 0; i < n; i++)
                {
                    float cellFill = n == 1 ? dashFill : Mathf.Clamp01(dashFill * n - i);
                    Color bar = Color.Lerp(UiTheme.A(cy, alpha), UiTheme.A(Color.white, UnityTuning.Arc24FlashAlpha), fl);
                    if (fl > 0) bar.a = Mathf.Lerp(alpha, UnityTuning.Arc24FlashAlpha, fl);
                    cells.Add(new UiArc24.Cell { x0 = x + i * (cw + cg), x1 = x + i * (cw + cg) + cw, fill = cellFill, track = UiTheme.A(ink, idle), bar = bar });
                }
                Arc24AlphaNow[1] = Mathf.Lerp(alpha, UnityTuning.Arc24FlashAlpha, fl);
            }
            else Arc24AlphaNow[1] = 0f;
            x += UnityTuning.Arc24WidthDash + gap;
            // FIST: three cells, the notches at the tiers
            if (fistOn)
            {
                float cw = (UnityTuning.Arc24WidthFist - cg * 2) / 3f;
                float fl = Arc24Flash(arc24FlashAge[2]);
                float alpha = fistFill > 0f ? busy : idle;
                for (int i = 0; i < 3; i++)
                {
                    double t0 = i == 0 ? 0 : fist.rules.tiers[i - 1], t1 = fist.rules.tiers[i];
                    float cellFill = t1 > t0 ? Mathf.Clamp01((float)((fist.charge - t0) / (t1 - t0))) : 0f;
                    Color bar = Color.Lerp(UiTheme.A(cy, alpha), UiTheme.A(Color.white, UnityTuning.Arc24FlashAlpha), fl);
                    if (fl > 0) bar.a = Mathf.Lerp(alpha, UnityTuning.Arc24FlashAlpha, fl);
                    cells.Add(new UiArc24.Cell { x0 = x + i * (cw + cg), x1 = x + i * (cw + cg) + cw, fill = cellFill, track = UiTheme.A(ink, idle), bar = bar });
                }
                Arc24AlphaNow[2] = Mathf.Lerp(alpha, UnityTuning.Arc24FlashAlpha, fl);
            }
            else Arc24AlphaNow[2] = 0f;

            // the mesh is rebuilt only when a drawn number changed (quantised: 1/64 of a fill, 1/32 of an alpha)
            int count = cells.Count;
            bool dirty = count != arc24ShownCount;
            for (int i = 0; i < count && i * 3 + 2 < arc24Shown.Length; i++)
            {
                UiArc24.Cell cell = cells[i];
                float f = Mathf.Round(cell.fill * 64f), al = Mathf.Round(cell.bar.a * 32f), w = cell.bar.g + cell.bar.r; // the flash tints the fill
                if (f != arc24Shown[i * 3] || al != arc24Shown[i * 3 + 1] || w != arc24Shown[i * 3 + 2]) dirty = true;
                arc24Shown[i * 3] = f;
                arc24Shown[i * 3 + 1] = al;
                arc24Shown[i * 3 + 2] = w;
            }
            arc24ShownCount = count;
            if (dirty) arc24.Changed();

            // the first show fades in
            float group = 1f;
            if (arc24Enter >= 0)
            {
                arc24Enter = HudEntrance18.Step(arc24Enter, frameDt);
                group = Mathf.Clamp01((float)(arc24Enter / UnityTuning.Arc24FadeIn));
                if (group >= 1f) arc24Enter = -1;
            }
            if (arc24Group.alpha != group) arc24Group.alpha = group;
        }
    }
}
