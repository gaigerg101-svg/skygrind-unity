// NIGHT24 (builder F, the lead's switch-lag row; design.n24.prewarm): the boot prewarm pass. UNCOMPILED and UNRUN when written. View only: it never touches
// the sim (no run starts, no input, no event). At boot, while TITLE shows, a coroutine
//   1. adds the UI's character set to every theme TMP font (TMP_FontAsset.TryAddCharacters: the glyphs a screen first needs are rasterised now),
//   2. makes every big screen (pause, down, results, the draft, settings, controls) active once at alpha 0 (a CanvasGroup), runs
//      Canvas.ForceUpdateCanvases and waits a frame so its layout and TMP meshes are built, then puts it back as it was,
//   3. Shader.WarmupAllShaders() (the shaders' first-use compile), when design.n24.prewarm.shaderWarmup is on.
// Wiring (the lead): at game init, after the views (SurvivalGame.N24InitViews): `Prewarm24.Start(this);` Off by default (the row's `enabled`).
using System.Collections;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>The big screens a prewarm pass wakes once (a screen not built yet is skipped; the draft is built on demand).</summary>
        internal GameObject[] Prewarm24Screens()
        {
            if (draftScreen == null) BuildDraft();
            return new[] { pause, down, results, draftScreen, settingsScreen, controlsScreen };
        }
    }

    public static class Prewarm24
    {
        /// <summary>Start the pass when design.n24.prewarm.enabled (and not under --off24 / -skygrindOff24).</summary>
        public static void Start(SurvivalGame g)
        {
            // NIGHT25 (builder F, design.n25.hitchFix): the route layer the first run start used to build (138 ms cold), built now at boot
            if (g != null && g.sim != null && g.cfg.Design("n25.hitchFix.enabled")) g.sim.PrebuildRoutes25();
            Prewarm25.Start(g); // NIGHT25: the HUD's first frame at boot (design.n25.hitchFix.hud)
            if (g == null || N24Blocks.ForceOff || !g.cfg.Design("n24.prewarm.enabled")) return;
            g.StartCoroutine(Run(g));
        }

        /// <summary>Each screen active once at alpha 0 for `frames` frames (a CanvasGroup it did not have is removed again), then put back as it was.
        /// Touches only the UI objects it is handed (EditMode: Prewarm24Tests proves a sim's state hash is unchanged around it). Stops at once when
        /// `stillTitle` turns false.</summary>
        public static IEnumerator WakeScreens(System.Func<bool> stillTitle, GameObject[] screens, int frames, System.Action<int> woken)
        {
            int n = 0;
            foreach (GameObject go in screens)
            {
                if (go == null) continue;
                if (!stillTitle()) break;
                bool was = go.activeSelf;
                var cg = go.GetComponent<CanvasGroup>();
                bool addedGroup = cg == null;
                if (addedGroup) cg = go.AddComponent<CanvasGroup>();
                float a = cg.alpha;
                bool br = cg.blocksRaycasts;
                cg.alpha = 0f;
                cg.blocksRaycasts = false;
                go.SetActive(true);
                Canvas.ForceUpdateCanvases();
                for (int f = 0; f < frames; f++) yield return null;
                go.SetActive(was);
                cg.alpha = a;
                cg.blocksRaycasts = br;
                if (addedGroup)
                {
                    if (Application.isPlaying) Object.Destroy(cg);
                    else Object.DestroyImmediate(cg);
                }
                n++;
            }
            woken(n);
        }

        static IEnumerator Run(SurvivalGame g)
        {
            GameConfig cfg = g.cfg;
            float t0 = Time.realtimeSinceStartup;
            yield return null; // the views finish their own first frame first
            if (g.flow != Flow.Title) yield break; // only at boot: never over a run

            // 1. glyphs
            string chars = cfg.Str("design.n24.prewarm.glyphs");
            int added = 0;
            foreach (TMP_FontAsset f in new[] { UiTheme.Display, UiTheme.Body, UiTheme.BodyBold, UiTheme.Rank })
            {
                if (f == null) continue;
                if (f.TryAddCharacters(chars, out string missing)) added++;
                else if (missing != null && missing.Length < chars.Length) added++;
            }
            yield return null;

            // 2. the big screens, once, invisible
            int woken = 0;
            yield return WakeScreens(() => g.flow == Flow.Title, g.UiView.Prewarm24Screens(), cfg.Int("design.n24.prewarm.screenFrames"), n => woken = n);

            // 3. shaders
            bool shaders = cfg.Design("n24.prewarm.shaderWarmup");
            if (shaders) Shader.WarmupAllShaders();
            yield return null;
            Debug.Log($"[prewarm] {added} fonts, {woken} screens, shaders {shaders}, {((Time.realtimeSinceStartup - t0) * 1000f).ToString("0", System.Globalization.CultureInfo.InvariantCulture)} ms");
        }
    }
}
