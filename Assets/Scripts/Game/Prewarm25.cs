// NIGHT25 (builder F, design.n25.hitchFix): the HUD's first frame, at boot. The Player's marks (Logs/player/n25-hitch-3.log): the first PLAY frame is 247.8 ms and
// R.Ui is 173.4 ms of it (8.3 MB allocated): the HUD layers (frame, values, sight) are inactive on TITLE, so the first PLAY builds every HUD TextMeshProUGUI's mesh and
// layout and rasterises its glyphs at once. While TITLE shows, this pass (1) adds the UI's character set to the theme fonts, then (2) makes the three HUD layers, the
// lesson line and the menu screens active once at alpha 0 for a couple of frames (Prewarm24.WakeScreens: layout and meshes built, nothing drawn) and puts them back.
// View only: it never touches the sim (EditMode: Prewarm24Tests proves WakeScreens leaves the state hash alone). It stops at once when TITLE is left, and does
// nothing under -skygrindOff24 or with the row off. No shader warm-up (last night's shaderWarmup stalled the boot 15 s without a gain).
using System.Collections;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>The in-game HUD's layers and the lesson line, then the big menu screens (a screen not built yet is skipped).</summary>
        internal GameObject[] Prewarm25Screens()
        {
            GameObject[] menus = Prewarm24Screens();
            var all = new GameObject[menus.Length + 4];
            all[0] = frame.gameObject;
            all[1] = values.gameObject;
            all[2] = sight.gameObject;
            all[3] = lesson25Rt != null ? lesson25Rt.gameObject : null;
            System.Array.Copy(menus, 0, all, 4, menus.Length);
            return all;
        }
    }

    public static class Prewarm25
    {
        public static void Start(SurvivalGame g)
        {
            if (g == null || N25Blocks.ForceOff || !g.cfg.Design("n25.hitchFix.enabled") || !g.cfg.Design("n25.hitchFix.hud")) return;
            g.StartCoroutine(Run(g));
        }

        static IEnumerator Run(SurvivalGame g)
        {
            float t0 = Time.realtimeSinceStartup;
            yield return null; // the views finish their own first frame
            if (g.flow != Flow.Title || g.UiView == null) yield break;
            string chars = g.cfg.Str("design.n24.prewarm.glyphs");
            int fonts = 0;
            foreach (TMP_FontAsset f in new[] { UiTheme.Display, UiTheme.Body, UiTheme.BodyBold, UiTheme.Rank })
            {
                if (f == null) continue;
                f.TryAddCharacters(chars, out _);
                fonts++;
            }
            yield return null;
            if (g.flow == Flow.Title && g.sim != null) g.sim.WarmParse25(); // the worm's files parsed once (the first run start used to pay for it)
            yield return null;
            int woken = 0;
            yield return Prewarm24.WakeScreens(() => g.flow == Flow.Title, g.UiView.Prewarm25Screens(), g.cfg.Int("design.n25.hitchFix.frames"), n => woken = n);
            Debug.Log($"[prewarm25] {fonts} fonts, {woken} layers woken, {((Time.realtimeSinceStartup - t0) * 1000f).ToString("0", System.Globalization.CultureInfo.InvariantCulture)} ms");
        }
    }
}
