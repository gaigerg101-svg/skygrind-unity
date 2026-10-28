// NIGHT17 U3-30.3 (the shared commit, the lead's): the identity's shape as one uGUI Graphic: a solid parallelogram along
// the theme's slant (UnityTuning.ThemeSlantDeg) with 45 deg cuts on its top-right and bottom-left corners (ThemeCut), for
// panels, plates, bars and buttons. Solid fills only: no 1 px outline, no translucent grey (NIGHT17.md section 1).
// A fill share (0..1, from the left) makes it a bar: the fill's right edge follows the slant. The mesh is rebuilt only
// when a field changes (SetVerticesDirty); OnPopulateMesh writes at most 7 vertices into Unity's VertexHelper (no
// allocation of ours).
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiShape : MaskableGraphic
    {
        float slantDeg = UnityTuning.ThemeSlantDeg;
        float cutTopRight = UnityTuning.ThemeCut, cutBottomLeft = UnityTuning.ThemeCut;
        float fill = 1f;

        /// <summary>The slant in degrees (0: upright). Positive leans the top to the right, like the italic.</summary>
        public float Slant
        {
            get => slantDeg;
            set { if (value != slantDeg) { slantDeg = value; SetVerticesDirty(); } }
        }

        /// <summary>The corner cuts in px (0: square).</summary>
        public void SetCuts(float topRight, float bottomLeft)
        {
            if (topRight == cutTopRight && bottomLeft == cutBottomLeft) return;
            cutTopRight = topRight;
            cutBottomLeft = bottomLeft;
            SetVerticesDirty();
        }

        /// <summary>The share filled from the left, 0..1 (a bar); 1 is the whole shape.</summary>
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

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            float w = r.width, h = r.height;
            if (w <= 0 || h <= 0 || fill <= 0) return;
            // The skew: x shifts by tan(slant) * (y - centre), so the shape stays centred in its rect.
            float k = Mathf.Tan(slantDeg * Mathf.Deg2Rad);
            float cy = r.y + h * 0.5f;
            float left = r.xMin, right = r.xMin + w * fill;
            float ct = Mathf.Min(cutTopRight, Mathf.Min(w, h) * 0.5f), cb = Mathf.Min(cutBottomLeft, Mathf.Min(w, h) * 0.5f);
            // The outline (counter-clockwise from the bottom-left cut), before the skew.
            // bottom-left cut: (left, ymin + cb) .. (left + cb, ymin); top-right cut: (right - ct, ymax) .. (right, ymax - ct)
            float ymin = r.yMin, ymax = r.yMax;
            bool cutRight = fill >= 0.999f; // a partial fill ends square on the slant
            Color32 c = color;
            vh.AddVert(Skew(new Vector2((left + right) * 0.5f, cy), k, cy), c, Vector2.zero); // 0: the centre
            vh.AddVert(Skew(new Vector2(left, ymin + cb), k, cy), c, Vector2.zero);           // 1
            vh.AddVert(Skew(new Vector2(Mathf.Min(left + cb, right), ymin), k, cy), c, Vector2.zero); // 2
            vh.AddVert(Skew(new Vector2(right, ymin), k, cy), c, Vector2.zero);               // 3
            vh.AddVert(Skew(new Vector2(right, cutRight ? ymax - ct : ymax), k, cy), c, Vector2.zero); // 4
            vh.AddVert(Skew(new Vector2(cutRight ? right - ct : right, ymax), k, cy), c, Vector2.zero); // 5
            vh.AddVert(Skew(new Vector2(left, ymax), k, cy), c, Vector2.zero);                // 6
            for (int i = 1; i <= 6; i++) vh.AddTriangle(0, i, i == 6 ? 1 : i + 1);
        }

        static Vector2 Skew(Vector2 p, float k, float cy) => new Vector2(p.x + k * (p.y - cy), p.y);
    }
}
