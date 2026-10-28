// NIGHT52 builder U, look52 round 5 (Look52.cs): the health pickup as a chunky PLUS that reads as health at 15 m. Bone body (the kit's stone
// material: the vertex colours lit), a cyan plus inlaid on both faces (the usable glow, pulsing as the vial's core did), turned to face the
// camera with a slow rock (Look52RockDeg at Look52RockHz: it never shows its thin edge). Never a red cross (a protected emblem; red-orange is the
// danger hue). Its "MEND +N" label is not drawn: the shape says it. The floor ring and the small light stay. Off: the NIGHT35 vial (MakeVial).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Pickup35View
    {
        bool cross52;

        /// <summary>From Make with look52 on, for a health pickup (in place of MakeVial).</summary>
        void MakeCross52(SurvivalMaterials mats, GameConfig cfg)
        {
            WeaponId = null;
            cross52 = true;
            float arm = UnityTuning.Look52CrossArm, bar = UnityTuning.Look52CrossBar, dep = UnityTuning.Look52CrossDepth;
            var body = new MeshKit();
            body.Cube(Vector3.zero, new Vector3(bar, arm, dep), BlockyKit.Bone);
            body.Cube(Vector3.zero, new Vector3(arm, bar, dep), BlockyKit.Bone);
            var inlay = new MeshKit();
            float ia = UnityTuning.Look52InlayArm, ib = UnityTuning.Look52InlayBar, z = dep * 0.5f + UnityTuning.Look52InlayProud * 0.5f, t = UnityTuning.Look52InlayProud;
            foreach (float side in new[] { 1f, -1f })
            {
                inlay.Cube(new Vector3(0, 0, side * z), new Vector3(ib, ia, t), Color.white);
                inlay.Cube(new Vector3(0, 0, side * z), new Vector3(ia, ib, t), Color.white);
            }
            var bodyMat52 = new Material(mats.stone) { name = "look52 plus" };
            bodyMat52.SetFloat("_GridMinor", 0);
            bodyMat52.SetFloat("_GridMajor", 0);
            coreMat = new Material(mats.neon) { name = "look52 plus inlay" };
            coreColor = cyan * UnityTuning.Pickup35VialGain;
            coreMat.SetColor("_BaseColor", coreColor);
            Part(model, "plus", body.Build("look52 plus"), bodyMat52);
            Part(model, "plus inlay", inlay.Build("look52 plus inlay"), coreMat);
            nameText = ""; // no label: the plus says it
        }

        /// <summary>The plus's turn this frame: facing the camera (yaw only) with a slow rock.</summary>
        Quaternion Cross52Turn(float t)
        {
            Camera cam = game.cam;
            float yaw = 0f;
            if (cam != null)
            {
                Vector3 to = cam.transform.position - model.position;
                to.y = 0;
                if (to.sqrMagnitude > 1e-4f) yaw = Quaternion.LookRotation(to.normalized, Vector3.up).eulerAngles.y;
            }
            return Quaternion.Euler(0, yaw + UnityTuning.Look52RockDeg * Mathf.Sin(t * UnityTuning.Look52RockHz * 2f * Mathf.PI), 0);
        }

        /// <summary>For the a252 reel: the pickup model's screen box (px, origin bottom-left) from its renderers' bounds.</summary>
        public Rect ScreenBox52(Camera cam)
        {
            if (cam == null || model == null) return Rect.zero;
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
            {
                Bounds b = r.bounds;
                for (int c = 0; c < 8; c++)
                {
                    var p = new Vector3((c & 1) == 0 ? b.min.x : b.max.x, (c & 2) == 0 ? b.min.y : b.max.y, (c & 4) == 0 ? b.min.z : b.max.z);
                    Vector3 s = cam.WorldToScreenPoint(p);
                    if (s.z <= 0) continue;
                    x0 = Mathf.Min(x0, s.x); y0 = Mathf.Min(y0, s.y); x1 = Mathf.Max(x1, s.x); y1 = Mathf.Max(y1, s.y);
                }
            }
            return x1 > x0 && y1 > y0 ? Rect.MinMaxRect(x0, y0, x1, y1) : Rect.zero;
        }

        /// <summary>For the a252 reel: the plus shape is drawn (look52) on this pickup.</summary>
        public bool Cross52 => cross52;
    }
}
