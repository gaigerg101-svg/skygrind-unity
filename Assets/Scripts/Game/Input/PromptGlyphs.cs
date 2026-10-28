// NIGHT19 U3-33.4 (builder P): the pad's drawn buttons as a TMP sprite asset, built at run time (no Editor step) from the
// vendored Kenney Input Prompts atlas (Assets/ThirdParty/KenneyInputPrompts: CC0, License.txt beside it; packed by its
// Source~/pack_atlas.py into Resources/KenneyInputPromptsAtlas.png). The atlas holds 3 sets (Xbox, PlayStation, Switch
// drawn by position) x 18 glyphs (PadButtonName 0-15, the left and right sticks), each plain then with the HOLD ring
// (UnityTuning.PromptAtlas*: the grid the script packs). Every glyph is a sprite character named PromptText.SpriteName
// (set, glyph, hold); TMP finds the asset by name through TMP_Text.OnSpriteAssetRequest (`<sprite="padglyphs" name=..>`
// in any text, no text component touched). The material is a copy of TMP's default sprite asset's (its TMP_Sprite shader
// ships with the Player through TMP Settings), else Shader.Find. Ready stays false when the atlas or the shader is
// missing: PromptText then writes the word sets (the fallback, and what a screen reader reads).
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

namespace Skygrind.Game
{
    public static class PromptGlyphs
    {
        static TMP_SpriteAsset asset;
        static bool tried, ready;

        /// <summary>NIGHT25: every sprite named in `text`'s tags is in the asset (a lesson line draws the pad's words instead of a tag the atlas cannot show).</summary>
        public static bool HasTags(string text)
        {
            if (!Ready) return false;
            int at = 0;
            while ((at = text.IndexOf("name=\"", at, System.StringComparison.Ordinal)) >= 0)
            {
                at += 6;
                int end = text.IndexOf('"', at);
                if (end < 0) return false;
                if (asset.GetSpriteIndexFromName(text.Substring(at, end - at)) < 0) return false;
                at = end;
            }
            return true;
        }

        /// <summary>The sprite asset is built and registered: PromptText may write sprite tags.</summary>
        public static bool Ready => ready && asset != null;

        /// <summary>Build the asset (ReaderInput calls it every frame with the block on: once it's built, or failed, a compare).
        /// A build that failed stays failed (the words); an asset destroyed under it (the Editor leaving Play) is built again.</summary>
        public static void Boot()
        {
            if (ready ? asset != null : tried) return;
            tried = true;
            ready = false;
            try
            {
                Build();
            }
            catch (System.Exception e)
            {
                ready = false;
                Debug.LogWarning("[prompts] the glyph sprite asset failed (the pad's prompts stay words): " + e);
            }
        }

        static void Build()
        {
            var tex = Resources.Load<Texture2D>(UnityTuning.PromptAtlasResource);
            if (tex == null)
            {
                Debug.LogWarning("[prompts] no glyph atlas (Resources/" + UnityTuning.PromptAtlasResource + "): the pad's prompts stay words");
                return;
            }
            Material mat = null;
            TMP_SpriteAsset def = TMP_Settings.defaultSpriteAsset;
            if (def != null && def.material != null) mat = new Material(def.material);
            else
            {
                Shader sh = Shader.Find("TextMeshPro/Sprite");
                if (sh != null) mat = new Material(sh);
            }
            if (mat == null)
            {
                Debug.LogWarning("[prompts] no TMP sprite shader: the pad's prompts stay words");
                return;
            }
            mat.SetTexture(ShaderUtilities.ID_MainTex, tex);
            mat.name = UnityTuning.PromptSpriteAsset;

            asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            asset.name = UnityTuning.PromptSpriteAsset;
            asset.hideFlags = HideFlags.DontSave;
            asset.spriteSheet = tex;
            // The glyph's size: pointSize glyph px make one em (TMP scales a sprite by fontSize / pointSize when it's set, and
            // takes the line's ascender / descender from the asset's face: the glyph's top and bottom around the baseline).
            int size = UnityTuning.PromptAtlasGlyph, n = 0;
            float bearingY = size * UnityTuning.PromptGlyphRise;
            asset.faceInfo = new FaceInfo
            {
                pointSize = size / UnityTuning.PromptGlyphEm, scale = 1, lineHeight = size, ascentLine = bearingY, descentLine = bearingY - size, baseline = 0,
            };
            JsonUtility.FromJsonOverwrite(UnityTuning.PromptSpriteAssetVersionJson, asset); // a current asset: TMP never "upgrades" it
            for (int hold = 0; hold < 2; hold++)
            {
                for (int set = 0; set < PromptText.SetCount; set++)
                {
                    for (int g = 0; g < PromptText.GlyphCount; g++, n++)
                    {
                        int col = n % UnityTuning.PromptAtlasColumns, row = n / UnityTuning.PromptAtlasColumns;
                        int pad = (UnityTuning.PromptAtlasCell - size) / 2;
                        int x = col * UnityTuning.PromptAtlasCell + pad;
                        int y = tex.height - (row * UnityTuning.PromptAtlasCell + pad) - size; // TMP's rects start at the bottom left
                        var glyph = new TMP_SpriteGlyph((uint)n, new GlyphMetrics(size, size, 0, bearingY, size), new GlyphRect(x, y, size, size), 1, 0);
                        asset.spriteGlyphTable.Add(glyph);
                        asset.spriteCharacterTable.Add(new TMP_SpriteCharacter(0xFFFE, glyph) { name = PromptText.SpriteName(set, g, hold == 1) });
                    }
                }
            }
            // The tables first, the material after: TMP's UpdateLookupTables would "upgrade" an asset with a material and no
            // version (it rebuilds the tables from the legacy list); with no material yet it only fills the lookups.
            asset.UpdateLookupTables();
            asset.material = mat;
            TMP_Text.OnSpriteAssetRequest -= Request; // a rebuild never subscribes twice
            TMP_Text.OnSpriteAssetRequest += Request;
            ready = true;
        }

        static TMP_SpriteAsset Request(int hashCode, string name) => name == UnityTuning.PromptSpriteAsset ? asset : null;
    }
}
