// NIGHT17 U3-30.3 (the shared commit, the lead's): the identity read once, for every screen. The tokens live in
// UnityTuning.Theme.cs (fonts, colour mixes, the type scale, slant, cut, motion); the hues are config.json's colour roles.
// Ui.Init calls UiTheme.Init(cfg) before it builds anything; every builder styles through these fields and helpers, never
// with a `new Color(` literal or a hand-typed size (NIGHT17.md rule 6).
// Allocation: Init allocates (fonts, the shadow materials, the cut sprite); the helpers don't.
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public static class UiTheme
    {
        /// <summary>The type scale's steps (UnityTuning.ThemeTypeScale): px at 1080p 18 / 24 / 32 / 42 / 56 / 75 / 100 / 133.</summary>
        public enum Size { S18 = 0, S24 = 1, S32 = 2, S42 = 3, S56 = 4, S75 = 5, S100 = 6, S133 = 7 }

        public static bool Ready { get; private set; }

        // --- the faces ---
        public static TMP_FontAsset Display, Body, BodyBold, Rank;
        /// <summary>Each face's material with a hard INK shadow (TMP underlay; ThemeShadowOffset px down-right).</summary>
        public static Material DisplayShadow, BodyShadow, BodyBoldShadow, RankShadow;

        // --- the colours ---
        /// <summary>INK: panels (opaque near-black violet at ThemeInkAlpha); INK-SOLID the same at alpha 1.</summary>
        public static Color Ink, InkSolid, InkSoft;
        /// <summary>WHITE: text. MUTED: secondary text (a token, never text at a low alpha).</summary>
        public static Color White, Muted;
        /// <summary>The player's cyan and its value ladder (deep, base, pale, hot).</summary>
        public static Color Cyan, CyanDeep, CyanPale, CyanHot;
        /// <summary>GOLD: points only. DANGER: enemies and danger only (CLAUDE.md rule 8).</summary>
        public static Color Gold, Danger;
        /// <summary>The same as 6 hex digits (rich text), made once.</summary>
        public static string WhiteHex, MutedHex, CyanHex, CyanPaleHex, GoldHex, DangerHex;

        /// <summary>A white square sprite with 45 deg corner cuts (top-right and bottom-left, ThemeCut px), 9-sliced: Img(...).sprite = Cut.</summary>
        public static Sprite Cut;

        public static void Init(GameConfig cfg)
        {
            if (Ready) return;
            Display = Load(UnityTuning.ThemeFontDisplay);
            Body = Load(UnityTuning.ThemeFontBody);
            BodyBold = Load(UnityTuning.ThemeFontBodyBold) ?? Body;
            Rank = Load(UnityTuning.ThemeFontRank) ?? Display;
            if (Display == null) Display = Body;
            Color text = Look.C(cfg, "ui.theme.text");
            Color panel = Look.C(cfg, "ui.theme.panel");
            Color cyan = Look.C(cfg, "colors.usable");
            InkSolid = new Color(panel.r, panel.g, panel.b, 1f);
            Ink = new Color(panel.r, panel.g, panel.b, UnityTuning.ThemeInkAlpha);
            InkSoft = Color.Lerp(InkSolid, text, UnityTuning.ThemeInkSoftMix);
            InkSoft.a = UnityTuning.ThemeInkAlpha;
            White = new Color(text.r, text.g, text.b, 1f);
            Muted = Color.Lerp(White, InkSolid, UnityTuning.ThemeMutedMix);
            Muted.a = 1f;
            Cyan = new Color(cyan.r, cyan.g, cyan.b, 1f);
            CyanDeep = Color.Lerp(Cyan, InkSolid, UnityTuning.ThemeCyanDeepMix);
            CyanDeep.a = 1f;
            CyanPale = Color.Lerp(Cyan, Color.white, UnityTuning.ThemeCyanPaleMix);
            CyanPale.a = 1f;
            CyanHot = Color.Lerp(Cyan, Color.white, UnityTuning.ThemeCyanHotMix);
            CyanHot.a = 1f;
            Gold = Look.C(cfg, "ui.theme.gold");
            Gold.a = 1f;
            Danger = Look.C(cfg, "colors.enemy");
            Danger.a = 1f;
            WhiteHex = ColorUtility.ToHtmlStringRGB(White);
            MutedHex = ColorUtility.ToHtmlStringRGB(Muted);
            CyanHex = ColorUtility.ToHtmlStringRGB(Cyan);
            CyanPaleHex = ColorUtility.ToHtmlStringRGB(CyanPale);
            GoldHex = ColorUtility.ToHtmlStringRGB(Gold);
            DangerHex = ColorUtility.ToHtmlStringRGB(Danger);
            DisplayShadow = ShadowOf(Display);
            BodyShadow = ShadowOf(Body);
            BodyBoldShadow = ShadowOf(BodyBold);
            RankShadow = ShadowOf(Rank);
            Cut = MakeCutSprite();
            Ready = true;
        }

        /// <summary>px at 1080p of a type-scale step.</summary>
        public static float Px(Size s) => UnityTuning.ThemeTypeScale[(int)s];

        /// <summary>The face for a size: DISPLAY from ThemeDisplayFrom, else BODY (BODY-BOLD when bold).</summary>
        public static TMP_FontAsset FaceFor(float px, bool bold = false) =>
            px >= UnityTuning.ThemeDisplayFrom && Display != null ? Display : bold ? BodyBold : Body;

        /// <summary>Style a text: face, size (never under the floor), colour; with a hard INK shadow when `shadow`.</summary>
        public static void Style(TMP_Text t, float px, Color c, TMP_FontAsset face = null, bool shadow = false)
        {
            if (t == null) return;
            if (face == null) face = FaceFor(px);
            if (face != null)
            {
                t.font = face;
                if (shadow)
                {
                    Material m = ShadowFor(face);
                    if (m != null) t.fontSharedMaterial = m;
                }
            }
            t.fontSize = Mathf.Max(UnityTuning.ThemeTextFloor, px);
            t.color = c;
            t.characterSpacing = face == Display ? UnityTuning.ThemeDisplayTracking : t.characterSpacing;
        }

        /// <summary>The face's hard-shadow material (null when the face has none).</summary>
        public static Material ShadowFor(TMP_FontAsset face) =>
            face == null ? null : face == Display ? DisplayShadow : face == BodyBold ? BodyBoldShadow : face == Rank ? RankShadow : face == Body ? BodyShadow : null;

        /// <summary>Colour with an alpha (no allocation).</summary>
        public static Color A(Color c, float a) => new Color(c.r, c.g, c.b, a);

        static TMP_FontAsset Load(string path) => string.IsNullOrEmpty(path) ? null : Resources.Load<TMP_FontAsset>(path);

        static Material ShadowOf(TMP_FontAsset face)
        {
            if (face == null || face.material == null) return null;
            var m = new Material(face.material) { name = face.name + " (ink shadow)" };
            m.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            m.SetColor(ShaderUtilities.ID_UnderlayColor, InkSolid);
            // Underlay offsets are in the SDF's units (-1..1 of the padding); a 2 px hard shadow at ~32 px text.
            m.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.6f);
            m.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.6f);
            m.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0f);
            m.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.1f);
            return m;
        }

        /// <summary>A 64 px white square with its top-right and bottom-left corners cut at 45 deg (ThemeCut), 9-sliced.</summary>
        static Sprite MakeCutSprite()
        {
            const int n = 64;
            int cut = Mathf.Clamp(Mathf.RoundToInt(UnityTuning.ThemeCut * 1.6f), 2, n / 2 - 2);
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "UiTheme cut", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    // distance inside each cut edge (top-right: x + y > 2n - 2 - cut; bottom-left: x + y < cut), 1 px of AA
                    float tr = (2 * n - 2 - cut) - (x + y) + 0.5f;
                    float bl = (x + y) - cut + 0.5f;
                    float a = Mathf.Clamp01(Mathf.Min(tr, bl));
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            float b = cut + 2;
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }
    }
}
