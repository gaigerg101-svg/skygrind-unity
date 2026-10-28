// NIGHT43 builder U1 (design.n43.screens43; GG's G24: "Blur or otherwise visually separate the gameplay background more effectively"): THE BACKDROP.
// One capture of the game frame (ScreenCapture.CaptureScreenshotAsTexture at the end of the frame the screen opens, the screen itself hidden that
// frame), blurred once on the GPU by SKYGRIND/UiBlur43 (a dual-filter Kawase: Blur43Levels halvings down, the same up), into one persistent half-size
// RenderTexture shown by a RawImage tinted toward DUSK and darkened (U1.md 1.2: the blurred game multiplied by DUSK), with the scanline overlay over
// it. Nothing runs per frame after the open (ui-art-web B8's fallback: "freeze one frame to a RenderTexture at menu open and blur it once"); no
// package, no renderer feature. The CPU readback costs one frame's copy at the open only. Without the shader (a build that stripped it) the backdrop
// stays the tint alone (a flat DUSK sheet), never a blank screen. Numbers: UnityTuning.Ui43.cs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 screens43's capture and blur (the reels print Blur43Probe).</summary>
    public static class UiBlur43
    {
        static Material mat;
        static bool matRead;
        static RenderTexture target;

        /// <summary>For the reels: the last capture's size, the blur's passes, its ms (CPU, readback + blits), and whether the shader was found.</summary>
        public static int LastW, LastH, Passes, Captures;
        public static double LastMs = -1;
        public static bool ShaderFound;

        static Material Mat
        {
            get
            {
                if (!matRead)
                {
                    matRead = true;
                    Shader s = Shader.Find("SKYGRIND/UiBlur43");
                    ShaderFound = s != null && s.isSupported;
                    if (ShaderFound) mat = new Material(s) { name = "UiBlur43", hideFlags = HideFlags.DontSave };
                }
                return mat;
            }
        }

        /// <summary>Capture this frame at its end (call it the frame the screen opens, with the screen hidden), blur it, and hand the texture to
        /// `done` (null when it could not: then the tint alone shows). Run as a coroutine on any live MonoBehaviour.</summary>
        public static IEnumerator Capture(Action<Texture> done)
        {
            yield return new WaitForEndOfFrame();
            Texture result = null;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Texture2D shot = null;
            try
            {
                shot = ScreenCapture.CaptureScreenshotAsTexture();
                if (shot != null) result = Blur(shot);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[skygrind] screens43: the backdrop capture failed (" + e.Message + "); the tint alone shows");
                result = null;
            }
            finally
            {
                if (shot != null) UnityEngine.Object.Destroy(shot);
            }
            LastMs = sw.Elapsed.TotalMilliseconds;
            Captures++;
            done?.Invoke(result);
        }

        static Texture Blur(Texture2D src)
        {
            Material m = Mat;
            int w = Math.Max(2, src.width / 2), h = Math.Max(2, src.height / 2);
            LastW = src.width;
            LastH = src.height;
            if (target == null || target.width != w || target.height != h)
            {
                if (target != null) { target.Release(); UnityEngine.Object.Destroy(target); }
                target = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32) { name = "UiBlur43 backdrop", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                target.Create();
            }
            if (m == null) { Graphics.Blit(src, target); Passes = 0; return target; } // no shader: the capture as it is (the tint still darkens it)
            int levels = Math.Max(1, UnityTuning.Blur43Levels);
            var chain = new RenderTexture[levels];
            Passes = 0;
            m.SetFloat("_Offset", UnityTuning.Blur43Offset);
            Texture cur = src;
            for (int i = 0; i < levels; i++)
            {
                int cw = Math.Max(2, w >> i), ch = Math.Max(2, h >> i);
                chain[i] = RenderTexture.GetTemporary(cw, ch, 0, RenderTextureFormat.ARGB32);
                chain[i].filterMode = FilterMode.Bilinear;
                Graphics.Blit(cur, chain[i], m, 0);
                Passes++;
                cur = chain[i];
            }
            for (int i = levels - 2; i >= 0; i--)
            {
                Graphics.Blit(cur, chain[i], m, 1);
                Passes++;
                cur = chain[i];
            }
            Graphics.Blit(cur, target, m, 1);
            Passes++;
            for (int i = 0; i < levels; i++) RenderTexture.ReleaseTemporary(chain[i]);
            return target;
        }

        /// <summary>The backdrop: a full-screen RawImage (the blurred capture once it lands; the DUSK tint from the first frame) and the scanlines over
        /// it, as the first children of `parent` (behind everything on that screen). Raycasts on: it takes the clicks meant for what is under it.</summary>
        public static RawImage Backdrop(RectTransform parent, out RawImage scan)
        {
            RectTransform rt = UiStyle33.Rect(parent, "Backdrop 43", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiKit43.Stretch(rt);
            rt.SetAsFirstSibling();
            var ri = rt.gameObject.AddComponent<RawImage>();
            ri.raycastTarget = true;
            ri.texture = null;
            ri.color = Tint(false);
            scan = UiKit43.Scan(rt, "Scan", UnityTuning.Blur43ScanAlpha);
            return ri;
        }

        /// <summary>The backdrop's colour: with the capture, a multiply toward DUSK (dark, violet); without it, DUSK itself, nearly opaque.</summary>
        public static Color Tint(bool hasCapture)
        {
            Color dusk = Look.Hex(UnityTuning.Blur43DuskHex);
            if (!hasCapture)
            {
                Color flat = Look.Hex(UnityTuning.Blur43FlatHex);
                return new Color(flat.r, flat.g, flat.b, UnityTuning.Blur43FlatAlpha);
            }
            Color t = Color.Lerp(Color.white, dusk, UnityTuning.Blur43DuskMix) * UnityTuning.Blur43Dim;
            t.a = 1f;
            return t;
        }
    }
}
