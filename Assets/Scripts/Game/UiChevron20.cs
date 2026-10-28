// NIGHT20 U3-34.10 (builder H): a chevron as one uGUI Graphic (a mesh): pointing up within its rect, a notch cut into its
// base (Hud20ClimbArrowNotch of the height) so it reads as a direction, not a triangle. The climb's off-screen arrow rotates
// it toward the climber. Four vertices, two triangles, rebuilt only when the rect or the notch changes; no allocation.
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiChevron20 : MaskableGraphic
    {
        float notch = UnityTuning.Hud20ClimbArrowNotch;

        /// <summary>The notch's depth as a share of the height (0: a triangle).</summary>
        public float Notch
        {
            get => notch;
            set
            {
                float v = Mathf.Clamp(value, 0f, 0.9f);
                if (v == notch) return;
                notch = v;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            if (r.width <= 0 || r.height <= 0) return;
            Color32 c = color;
            float cx = r.center.x;
            vh.AddVert(new Vector3(cx, r.yMax), c, Vector2.zero); // 0: the tip
            vh.AddVert(new Vector3(r.xMax, r.yMin), c, Vector2.zero); // 1: the right wing
            vh.AddVert(new Vector3(cx, r.yMin + r.height * notch), c, Vector2.zero); // 2: the notch
            vh.AddVert(new Vector3(r.xMin, r.yMin), c, Vector2.zero); // 3: the left wing
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
        }
    }
}
