// NIGHT12 U3-25.6 (D-13: the theme fonts): SKYGRIND/Make Theme Fonts builds the TMP SDF font assets of theme.js's two OFL
// families from the TTFs in Assets/Fonts (Barlow Semi Condensed 600 / 700, Chakra Petch 600 / 700 italic, fetched once
// from Google Fonts' GitHub with their OFL.txt) into Assets/Resources/Fonts (Ui loads them with Resources.Load, so a
// Player has them). Each atlas is STATIC: it holds ASCII 32-126, Latin-1 160-255 and a few marks the UI may use, and
// nothing is ever added at run time (the tracked LiberationSans SDF - Fallback asset rewrites itself when a dynamic
// fallback gets a glyph). The atlas sizes in UnityTuning.FontAtlasSizes are tried in order until every glyph the font
// has fits. Then the weight tables: Chakra Petch 600's italic and bold italic are Chakra Petch 700 italic; Barlow 600's
// bold is Barlow 700 (TMP picks the real face for a bold / italic style or a <b> / <i> tag instead of faking it).
// One log line per asset made, with the characters the font lacks. Run it once in the Editor; rerunning replaces the
// assets (new GUIDs; Ui finds them by path).
using System.Collections.Generic;
using System.IO;
using System.Text;
using Skygrind.Game;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Skygrind.EditorTools
{
    public static class ThemeFonts
    {
        const string OutDir = "Assets/Resources/Fonts";

        // The static charset beyond ASCII and Latin-1: the middle dot is Latin-1; these are the bullet, the white square,
        // the arrows, the en and em dashes, the ellipsis and the check mark (the multiplication sign is Latin-1).
        const string Marks = "•□←↑→↓–—…✓";

        static readonly (string ttf, string asset)[] Faces =
        {
            ("Assets/Fonts/ChakraPetch/ChakraPetch-SemiBold.ttf", UnityTuning.FontDisplayAsset),
            ("Assets/Fonts/ChakraPetch/ChakraPetch-BoldItalic.ttf", UnityTuning.FontDisplayItalicAsset),
            ("Assets/Fonts/BarlowSemiCondensed/BarlowSemiCondensed-SemiBold.ttf", UnityTuning.FontBodyAsset),
            ("Assets/Fonts/BarlowSemiCondensed/BarlowSemiCondensed-Bold.ttf", UnityTuning.FontBodyBoldAsset),
        };

        // NIGHT17 U3-30.2 (the identity): the candidate faces, static instances cut from Google Fonts' variable TTFs with
        // fontTools (Archivo wdth 62 wght 900 italic, wdth 100 wght 600 / 800; Tourney wdth 100 wght 900 italic; Big
        // Shoulders Display 900; Saira wdth 87.5 wght 600), each folder with its OFL.txt. Baked by MakeN17 only (the older
        // faces are kept until the pick lands).
        static readonly (string ttf, string asset)[] FacesN17 =
        {
            ("Assets/Fonts/Archivo/Archivo-ExtraCondensedBlackItalic.ttf", UnityTuning.FontArchivoDisplay),
            ("Assets/Fonts/Archivo/Archivo-SemiBold.ttf", UnityTuning.FontArchivoBody),
            ("Assets/Fonts/Archivo/Archivo-ExtraBold.ttf", UnityTuning.FontArchivoBodyBold),
            ("Assets/Fonts/Tourney/Tourney-BlackItalic.ttf", UnityTuning.FontTourneyRank),
            ("Assets/Fonts/BigShouldersDisplay/BigShouldersDisplay-Black.ttf", UnityTuning.FontBigShouldersDisplay),
            ("Assets/Fonts/Saira/Saira-SemiCondensedSemiBold.ttf", UnityTuning.FontSairaBody),
        };

        [MenuItem("SKYGRIND/Make Theme Fonts (NIGHT17)")]
        public static void MakeN17()
        {
            uint[] charset = Charset();
            int n = 0;
            foreach (var (ttf, asset) in FacesN17)
                if (MakeOne(ttf, asset, charset) != null) n++;
            AssetDatabase.SaveAssets();
            Debug.Log($"[ThemeFonts] NIGHT17 done: {n} / {FacesN17.Length} font assets in {OutDir}");
        }

        [MenuItem("SKYGRIND/Make Theme Fonts")]
        public static void Make()
        {
            uint[] charset = Charset();
            Directory.CreateDirectory(OutDir);
            var made = new Dictionary<string, TMP_FontAsset>();
            foreach (var (ttf, asset) in Faces)
            {
                TMP_FontAsset fa = MakeOne(ttf, asset, charset);
                if (fa != null) made[asset] = fa;
            }
            // The weight tables (TMP_FontWeightPair is a struct: set the array's elements in place).
            if (made.TryGetValue(UnityTuning.FontDisplayAsset, out TMP_FontAsset display) && made.TryGetValue(UnityTuning.FontDisplayItalicAsset, out TMP_FontAsset displayItalic))
            {
                display.fontWeightTable[4].italicTypeface = displayItalic; // italic
                display.fontWeightTable[7].italicTypeface = displayItalic; // bold italic
                EditorUtility.SetDirty(display);
                Debug.Log($"[ThemeFonts] {display.name}: italic and bold italic -> {displayItalic.name}");
            }
            if (made.TryGetValue(UnityTuning.FontBodyAsset, out TMP_FontAsset body) && made.TryGetValue(UnityTuning.FontBodyBoldAsset, out TMP_FontAsset bodyBold))
            {
                body.fontWeightTable[7].regularTypeface = bodyBold; // bold
                EditorUtility.SetDirty(body);
                Debug.Log($"[ThemeFonts] {body.name}: bold -> {bodyBold.name}");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ThemeFonts] done: {made.Count} / {Faces.Length} font assets in {OutDir}");
        }

        /// <summary>ASCII 32-126, Latin-1 160-255 and the marks.</summary>
        static uint[] Charset()
        {
            var list = new List<uint>();
            for (uint c = 32; c <= 126; c++) list.Add(c);
            for (uint c = 160; c <= 255; c++) list.Add(c);
            foreach (char c in Marks) list.Add(c);
            return list.ToArray();
        }

        static TMP_FontAsset MakeOne(string ttfPath, string resourcePath, uint[] charset)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null)
            {
                AssetDatabase.ImportAsset(ttfPath, ImportAssetOptions.ForceSynchronousImport);
                font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            }
            if (font == null)
            {
                Debug.LogError($"[ThemeFonts] {ttfPath}: no Font asset (is the TTF imported?)");
                return null;
            }
            string name = Path.GetFileName(resourcePath);
            string path = $"Assets/Resources/{resourcePath}.asset";
            foreach (Vector2Int size in UnityTuning.FontAtlasSizes)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(path) != null) AssetDatabase.DeleteAsset(path);
                TMP_FontAsset fa = TMP_FontAsset.CreateFontAsset(font, UnityTuning.FontSamplingPointSize, UnityTuning.FontAtlasPadding, GlyphRenderMode.SDFAA, size.x, size.y, AtlasPopulationMode.Dynamic, false);
                if (fa == null)
                {
                    Debug.LogError($"[ThemeFonts] {ttfPath}: TMP could not load the font face");
                    return null;
                }
                fa.name = name;
                // Persist first (as TMP's own Create menu does), with the atlas and the material as sub-assets.
                AssetDatabase.CreateAsset(fa, path);
                Texture2D atlas = fa.atlasTextures[0];
                atlas.name = name + " Atlas";
                AssetDatabase.AddObjectToAsset(atlas, fa);
                fa.material.name = name + " Material";
                AssetDatabase.AddObjectToAsset(fa.material, fa);

                fa.TryAddCharacters(charset, out uint[] missing, true);
                // NIGHT12 (the lead, after Play checks 1-2): no imported kerning. With these static atlases its pair records drew
                // letter-spaced labels with uneven gaps ("R O UND", "F IRST RUN", the tagline), and they were most of each
                // asset's 8 MB of YAML (10,957 records in Chakra 600).
                fa.fontFeatureTable.glyphPairAdjustmentRecords.Clear();
                // A character the font has but the atlas couldn't take means the atlas is full: try the next size.
                var lacking = new List<uint>();
                bool full = false;
                if (missing != null && missing.Length > 0 && FontEngine.LoadFontFace(font, UnityTuning.FontSamplingPointSize) == FontEngineError.Success)
                {
                    foreach (uint u in missing)
                    {
                        // NIGHT12 (the lead): a lacking character comes back as glyph 0 (.notdef) with true: only a real glyph means "full".
                        if (FontEngine.TryGetGlyphWithUnicodeValue(u, GlyphLoadFlags.LOAD_NO_BITMAP, out UnityEngine.TextCore.Glyph g) && g != null && g.index != 0) full = true;
                        else lacking.Add(u);
                    }
                }
                else if (missing != null) lacking.AddRange(missing);
                if (full && size != UnityTuning.FontAtlasSizes[UnityTuning.FontAtlasSizes.Length - 1]) continue;

                fa.atlasPopulationMode = AtlasPopulationMode.Static; // nothing is added at run time (drops the TTF reference)
                EditorUtility.SetDirty(atlas);
                EditorUtility.SetDirty(fa);
                AssetDatabase.SaveAssets();
                var sb = new StringBuilder();
                foreach (uint u in lacking) sb.Append(" U+").Append(u.ToString("X4"));
                Debug.Log($"[ThemeFonts] made {path}: atlas {size.x} x {size.y}, {fa.characterTable.Count} characters, {fa.glyphTable.Count} glyphs" +
                          (lacking.Count > 0 ? $"; missing (the font has no glyph):{sb}" : "; missing: none") + (full ? "; ATLAS FULL even at the largest size" : ""));
                return fa;
            }
            return null;
        }
    }
}
