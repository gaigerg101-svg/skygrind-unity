// NIGHT31 builder G (P0 d, D-6; design.n31.lowHealthEdge, a view row: no sim bit; T-33 "the low-health full-screen red"; GG asked that effects not touch the
// whole screen). With the row on, the low-health red stays at the screen's EDGES: the HUD's two red vignettes (the hurt / low-HP one and the damage one)
// swap to a frame sprite whose alpha is 0 inside a box (UnityTuning.Edge31*Clear of the half-size on each axis) and ramps up only in the outer band, and
// PostFx's vignette is capped and tightened with no whole-screen desaturation (PostFx.cs, the low-HP block). Off: the row off, -skygrindOff31, or the reel's
// ReelOverride false: NIGHT30's vignettes as they were.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The low-health edge row as the views read it (once from config; the reel can force it either way to print both sides in one run).</summary>
    public static class LowHealthEdge31
    {
        static bool? row;

        /// <summary>The reel's switch (edge31): true / false forces the row; null = the config's.</summary>
        public static bool? ReelOverride;

        public static bool Active(GameConfig cfg)
        {
            row ??= !N31Blocks.ForceOff && cfg != null && cfg.Design("n31.lowHealthEdge.enabled");
            return ReelOverride ?? row.Value;
        }
    }

    public sealed partial class Ui
    {
        Sprite edge31Hurt, edge31Damage, orig31Hurt, orig31Damage;
        bool edge31Shown;

        /// <summary>Once a frame before the vignettes are written: the sprites follow the row (swapped only when it changes).</summary>
        void LowHealthEdge31Swap()
        {
            bool on = LowHealthEdge31.Active(game.cfg);
            if (on == edge31Shown) return;
            if (orig31Hurt == null)
            {
                orig31Hurt = vignette.sprite;
                orig31Damage = damageVignette.sprite;
            }
            if (on && edge31Hurt == null)
            {
                edge31Hurt = EdgeSprite31(UnityTuning.Edge31HurtClear, UnityTuning.Edge31HurtEdgeAlpha, "Hurt vignette edge 31");
                edge31Damage = EdgeSprite31(UnityTuning.Edge31DamageClear, UnityTuning.Edge31DamageEdgeAlpha, "Damage vignette edge 31");
            }
            vignette.sprite = on ? edge31Hurt : orig31Hurt;
            damageVignette.sprite = on ? edge31Damage : orig31Damage;
            edge31Shown = on;
        }

        /// <summary>A frame: white with alpha 0 inside the box |x|, |y| below `clear` (of the half-size), a smooth ramp to `edgeAlpha` at the screen's edge. Stretched over
        /// the screen like the old radial sprites, so the band is the same share of the width and of the height.</summary>
        static Sprite EdgeSprite31(float clear, float edgeAlpha, string name)
        {
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = name };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = Mathf.Abs((x + 0.5f) / n * 2 - 1), dy = Mathf.Abs((y + 0.5f) / n * 2 - 1);
                    float d = Mathf.Max(dx, dy);
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - clear) / (1 - clear))) * edgeAlpha;
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }
    }
}
