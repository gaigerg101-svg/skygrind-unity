// NIGHT17 U3-30.2 (the lead's): the style tile, `SKYGRIND.exe -skygrindTile <dir>` (PlayerChecks): over a frozen run, two
// directions side by side, each with the same sample components drawn with UiTheme's tokens: the vitals block (HP, the
// stamina bar), the points with a merged pop, the style rank (letter, word, decay bar, callouts), the round's hero number,
// a menu column (focused + unfocused) and a settings row with a slider. A: Archivo Extra Condensed Black Italic + Archivo
// SemiBold / ExtraBold + Tourney for the rank and the round. B: Big Shoulders Display Black + Saira SemiCondensed SemiBold.
// It is a capture tool (it allocates when it builds); nothing in the game uses it.
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public static class StyleTile
    {
        struct Dir
        {
            public string name;
            public TMP_FontAsset display, body, bold, rank;
        }

        static TMP_FontAsset F(string path) => Resources.Load<TMP_FontAsset>(path);

        public static IEnumerator Run(SurvivalGame g, string dir, List<string> log)
        {
            Directory.CreateDirectory(dir);
            g.noRecords = true;
            g.StartRun(151515);
            g.health.max = g.health.hp = 1e6;
            if (g.run.state == SurvivalRun.State.Break) g.run.phaseSteps = UnityTuning.ReelHoldBreakSteps; // NIGHT17: a believable countdown on screen (int.MaxValue / 2 showed "ROUND 1 IN 8947848")
            for (int i = 0; i < 90; i++) yield return null;
            var a = new Dir { name = "A  ARCHIVO + TOURNEY", display = F(UnityTuning.FontArchivoDisplay), body = F(UnityTuning.FontArchivoBody), bold = F(UnityTuning.FontArchivoBodyBold), rank = F(UnityTuning.FontTourneyRank) };
            var b = new Dir { name = "B  BIG SHOULDERS + SAIRA", display = F(UnityTuning.FontBigShouldersDisplay), body = F(UnityTuning.FontSairaBody), bold = F(UnityTuning.FontSairaBody), rank = F(UnityTuning.FontBigShouldersDisplay) };
            GameObject root = Build(a, b);
            for (int i = 0; i < 3; i++) yield return null;
            string path = Path.Combine(dir, "n17-tile.png");
            ScreenCapture.CaptureScreenshot(path);
            yield return null;
            yield return null;
            Debug.Log("[tile] " + path + (a.display == null || b.display == null ? " (a face is missing: TMP's default drawn)" : ""));
            log.Add(path);
            Object.Destroy(root);
            g.ToTitle();
        }

        static GameObject Build(Dir a, Dir b)
        {
            var go = new GameObject("Style tile", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            var sc = go.GetComponent<UnityEngine.UI.CanvasScaler>();
            sc.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
            sc.matchWidthOrHeight = 1;
            Half(go.transform, a, -480);
            Half(go.transform, b, 480);
            // the divider: a 4 px accent bar along the slant
            Shape(go.transform, UiTheme.Cyan, new Vector2(0, 0), new Vector2(UnityTuning.ThemeAccentBar, 1080), UnityTuning.ThemeSlantDeg, 0);
            return go;
        }

        static void Half(Transform p, Dir d, float cx)
        {
            // the name of the direction
            T(p, d.name, d.bold, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, new Vector2(cx, 500), TextAlignmentOptions.Center, false);

            // --- the HUD over the world (top half): vitals, points + pop, rank, round ---
            // the round's hero number with the wordmark's cyan offset copy
            T(p, "07", d.rank, UiTheme.Px(UiTheme.Size.S133), UiTheme.CyanDeep, new Vector2(cx - 300 + UnityTuning.ThemeHeroOffset, 360 - UnityTuning.ThemeHeroOffset), TextAlignmentOptions.Center, false);
            T(p, "07", d.rank, UiTheme.Px(UiTheme.Size.S133), UiTheme.White, new Vector2(cx - 300, 360), TextAlignmentOptions.Center, true);
            T(p, "ROUND", d.bold, UiTheme.Px(UiTheme.Size.S18), UiTheme.CyanPale, new Vector2(cx - 300, 268), TextAlignmentOptions.Center, true);
            // the style rank: letter, word, decay bar, callouts (the cyan ladder: pale, cyan, hot)
            T(p, "S", d.rank, UiTheme.Px(UiTheme.Size.S100), UiTheme.CyanHot, new Vector2(cx + 40, 370), TextAlignmentOptions.Center, true);
            T(p, "SAVAGE", d.display, UiTheme.Px(UiTheme.Size.S32), UiTheme.Cyan, new Vector2(cx + 190, 392), TextAlignmentOptions.Left, true, 150);
            Shape(p, UiTheme.InkSolid, new Vector2(cx + 230, 350), new Vector2(240, 12), UnityTuning.ThemeSlantDeg, 1f);
            Shape(p, UiTheme.Cyan, new Vector2(cx + 230, 350), new Vector2(240, 12), UnityTuning.ThemeSlantDeg, 0.62f);
            T(p, "+ HEADSHOT", d.bold, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, new Vector2(cx + 190, 312), TextAlignmentOptions.Left, true, 300);
            T(p, "+ MULTIKILL", d.bold, UiTheme.Px(UiTheme.Size.S18), UiTheme.CyanPale, new Vector2(cx + 190, 282), TextAlignmentOptions.Left, true, 300);
            // the vitals block: HP in big numerals, its bar, the stamina bar (dim below the boost floor, solid above)
            T(p, "100", d.display, UiTheme.Px(UiTheme.Size.S75), UiTheme.White, new Vector2(cx - 300, 160), TextAlignmentOptions.Left, true, 300);
            Shape(p, UiTheme.InkSolid, new Vector2(cx - 220, 100), new Vector2(300, 18), UnityTuning.ThemeSlantDeg, 1f);
            Shape(p, UiTheme.White, new Vector2(cx - 220, 100), new Vector2(300, 18), UnityTuning.ThemeSlantDeg, 0.8f);
            Shape(p, UiTheme.A(UiTheme.Cyan, 0.35f), new Vector2(cx - 220, 72), new Vector2(300, 10), UnityTuning.ThemeSlantDeg, 1f);
            Shape(p, UiTheme.Cyan, new Vector2(cx - 220, 72), new Vector2(300, 10), UnityTuning.ThemeSlantDeg, 0.55f);
            // points and a merged pop
            T(p, "12450", d.display, UiTheme.Px(UiTheme.Size.S56), UiTheme.Gold, new Vector2(cx + 300, 150), TextAlignmentOptions.Right, true, 340);
            T(p, "+150 ×3", d.display, UiTheme.Px(UiTheme.Size.S42), UiTheme.Gold, new Vector2(cx + 120, 210), TextAlignmentOptions.Center, true, 260);

            // --- the menus on INK (bottom half) ---
            Shape(p, UiTheme.Ink, new Vector2(cx, -260), new Vector2(860, 500), 0, 0); // a solid INK panel, no slant
            T(p, "PAUSED", d.display, UiTheme.Px(UiTheme.Size.S75), UiTheme.White, new Vector2(cx - 200, -60), TextAlignmentOptions.Left, true, 500);
            T(p, "ROUND 7  ·  12450  ·  6:42", d.body, UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, new Vector2(cx - 200, -110), TextAlignmentOptions.Left, false, 500);
            Row(p, d, "RESUME", true, new Vector2(cx - 160, -170));
            Row(p, d, "RESTART", false, new Vector2(cx - 145, -232));
            Row(p, d, "SETTINGS", false, new Vector2(cx - 130, -294));
            Row(p, d, "QUIT", false, new Vector2(cx - 115, -356));
            // a settings row: the label, a slider, the value; the focus footer
            T(p, "SENSITIVITY", d.bold, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, new Vector2(cx + 60, -180), TextAlignmentOptions.Left, false, 220);
            Shape(p, UiTheme.InkSoft, new Vector2(cx + 300, -240), new Vector2(260, 16), UnityTuning.ThemeSlantDeg, 1f);
            Shape(p, UiTheme.Cyan, new Vector2(cx + 300, -240), new Vector2(260, 16), UnityTuning.ThemeSlantDeg, 0.45f);
            T(p, "2.40", d.display, UiTheme.Px(UiTheme.Size.S32), UiTheme.Cyan, new Vector2(cx + 300, -290), TextAlignmentOptions.Center, true, 120);
            T(p, "HOW FAR THE VIEW TURNS", d.body, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, new Vector2(cx + 300, -330), TextAlignmentOptions.Center, false, 340);
            T(p, "[SHIFT] BOOST", d.bold, UiTheme.Px(UiTheme.Size.S24), UiTheme.CyanPale, new Vector2(cx + 300, -420), TextAlignmentOptions.Center, true, 340);
        }

        static void Row(Transform p, Dir d, string label, bool focus, Vector2 pos)
        {
            Shape(p, focus ? UiTheme.Cyan : UiTheme.InkSoft, pos, new Vector2(320, 52), UnityTuning.ThemeSlantDeg, 1f);
            if (focus) Shape(p, UiTheme.White, pos + new Vector2(-170, 0), new Vector2(UnityTuning.ThemeAccentBar * 2, 52), UnityTuning.ThemeSlantDeg, 1f);
            T(p, label, d.display, UiTheme.Px(UiTheme.Size.S32), focus ? UiTheme.InkSolid : UiTheme.White, pos + new Vector2(-130, 0), TextAlignmentOptions.Left, !focus, 280);
        }

        static UiShape Shape(Transform p, Color c, Vector2 pos, Vector2 size, float slant, float fill)
        {
            var go = new GameObject("Shape", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(p, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var s = go.GetComponent<UiShape>();
            s.color = c;
            s.raycastTarget = false;
            s.Slant = slant;
            if (slant == 0) s.SetCuts(UnityTuning.ThemeCut, UnityTuning.ThemeCut);
            if (fill > 0) s.Fill = fill;
            return s;
        }

        static TextMeshProUGUI T(Transform p, string s, TMP_FontAsset face, float px, Color c, Vector2 pos, TextAlignmentOptions align, bool shadow, float width = 600)
        {
            var go = new GameObject(s, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(p, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = align == TextAlignmentOptions.Left ? new Vector2(0, 0.5f) : align == TextAlignmentOptions.Right ? new Vector2(1, 0.5f) : new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, px * 1.3f);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = s;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.alignment = align;
            t.raycastTarget = false;
            if (face != null)
            {
                t.font = face;
                if (shadow)
                {
                    var m = new Material(face.material);
                    m.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                    m.SetColor(ShaderUtilities.ID_UnderlayColor, UiTheme.InkSolid);
                    m.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.6f);
                    m.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.6f);
                    m.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0f);
                    t.fontSharedMaterial = m;
                }
            }
            t.fontSize = Mathf.Max(UnityTuning.ThemeTextFloor, px);
            t.color = c;
            return t;
        }
    }
}
