// NIGHT20 U3-34.10 (builder H): a ring as one uGUI Graphic (a mesh, not a texture): an annulus from Inner to Outer px about
// the rect's centre, crisp at any size and HUD SCALE. Three uses tonight:
// - a progress ring (the shockwave chip): Fill 0..1 sweeps clockwise from 12 o'clock;
// - a segmented ring (the zoom reticle): Gaps gaps of GapDeg each, centred on GapAt + k x 360 / Gaps;
// - a mask with a hole (the LONGWAVE's scope): Inner = the lens, Outer past the screen's corners; and the lens' shaded edge,
//   with InnerAlpha / OuterAlpha shading the band across its width (alpha at the inner edge, at the outer edge).
// The mesh is rebuilt only when a field changes (SetVerticesDirty); OnPopulateMesh writes into Unity's VertexHelper (no
// allocation of ours). Solid fills: no texture, no 1 px hairline (the theme's hard edges).
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiRing20 : MaskableGraphic
    {
        float inner = 10, outer = 12, fill = 1, gapDeg, gapAt, innerAlpha = 1, outerAlpha = 1;
        int gaps, steps = 48;

        /// <summary>The band's radii in px (Inner may be 0: a disc).</summary>
        public void SetRadii(float rIn, float rOut)
        {
            if (rIn == inner && rOut == outer) return;
            inner = rIn;
            outer = rOut;
            SetVerticesDirty();
        }

        /// <summary>The swept share 0..1, clockwise from 12 o'clock (1: a whole ring).</summary>
        public float Fill
        {
            get => fill;
            set
            {
                float v = value < 0 ? 0 : value > 1 ? 1 : value;
                if (Mathf.Abs(v - fill) < 0.0005f) return;
                fill = v;
                SetVerticesDirty();
            }
        }

        /// <summary>`count` gaps of `deg` each, the first centred at `atDeg` (0 = 12 o'clock, clockwise); 0: none.</summary>
        public void SetGaps(int count, float deg, float atDeg)
        {
            if (count == gaps && deg == gapDeg && atDeg == gapAt) return;
            gaps = count;
            gapDeg = deg;
            gapAt = atDeg;
            SetVerticesDirty();
        }

        /// <summary>The band's alpha at its inner and outer edge (x the colour's alpha): a shaded edge.</summary>
        public void SetShade(float atInner, float atOuter)
        {
            if (atInner == innerAlpha && atOuter == outerAlpha) return;
            innerAlpha = atInner;
            outerAlpha = atOuter;
            SetVerticesDirty();
        }

        /// <summary>Steps round a whole turn (the mesh's smoothness).</summary>
        public int Steps
        {
            get => steps;
            set
            {
                int v = Mathf.Max(8, value);
                if (v == steps) return;
                steps = v;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (outer <= inner || outer <= 0) return;
            Rect r = GetPixelAdjustedRect();
            Vector2 c = r.center;
            Color32 cIn = color, cOut = color;
            cIn.a = (byte)(color.a * Mathf.Clamp01(innerAlpha) * 255f);
            cOut.a = (byte)(color.a * Mathf.Clamp01(outerAlpha) * 255f);
            if (gaps > 0)
            {
                float span = 360f / gaps, arc = span - gapDeg;
                if (arc <= 0) return;
                int n = Mathf.Max(2, Mathf.CeilToInt(steps * arc / 360f));
                for (int g = 0; g < gaps; g++)
                    Arc(vh, c, gapAt + g * span + gapDeg * 0.5f, arc, n, cIn, cOut);
                return;
            }
            if (fill <= 0) return;
            Arc(vh, c, 0f, 360f * fill, Mathf.Max(2, Mathf.CeilToInt(steps * fill)), cIn, cOut);
        }

        /// <summary>A band from `fromDeg` sweeping `sweepDeg` clockwise (0 = 12 o'clock), in `n` steps.</summary>
        void Arc(VertexHelper vh, Vector2 c, float fromDeg, float sweepDeg, int n, Color32 cIn, Color32 cOut)
        {
            int start = vh.currentVertCount;
            for (int k = 0; k <= n; k++)
            {
                float a = (fromDeg + sweepDeg * k / n) * Mathf.Deg2Rad;
                var d = new Vector2(Mathf.Sin(a), Mathf.Cos(a)); // clockwise from straight up
                vh.AddVert(c + d * inner, cIn, Vector2.zero);
                vh.AddVert(c + d * outer, cOut, Vector2.zero);
            }
            for (int k = 0; k < n; k++)
            {
                int i = start + 2 * k;
                vh.AddTriangle(i, i + 1, i + 3);
                vh.AddTriangle(i, i + 3, i + 2);
            }
        }
    }
}
