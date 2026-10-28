// NIGHT8 U2-11: the HUD's perk row (perks.js: "The HUD reads `owned` for its icon row"): each owned perk's 8x8 pixel
// icon in its colour, in purchase order, in a row above the HP panel. Rebuilt only when the owned count changes.
// NIGHT16 polish #60 (hud.js :139, :848-855): after the perk icons, each owned mod as a chip: its name's first three
// letters and its stack ("VAM×2"; NIGHT17 round 1: the whole word at S18, ModChip.Word), bold 16 px, in its tag's colour (Mods.TagColor: the weapon family's, else the style
// tag's, else colors.usable) on a plate with a 2 px line under it in the same colour. Rule 8: a red-orange or magenta tag
// colour (ModChip.Rule8Ok; tonight only "fire", 0xFF8C1A) falls back to the text colour. Config order, as the web's
// hudKey. Rebuilt only when the owned perks or the mods' version change; nothing per frame.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT16 polish #60: a mod chip's label and colour rule (ModChipTests).</summary>
    public static class ModChip
    {
        /// <summary>hud.js :855: d.name.slice(0, 3) + (k > 1 ? "×k" : "").</summary>
        public static string Label(string name, int stacks)
        {
            string n = name == null ? "" : name.Length > 3 ? name.Substring(0, 3) : name;
            return stacks > 1 ? n + "\u00D7" + stacks.ToString(System.Globalization.CultureInfo.InvariantCulture) : n;
        }

        /// <summary>
        /// NIGHT17 round 1's fix list ("never cut a word"): the mod's whole name in caps, "x k" after it from two stacks. The
        /// web's three-letter cut (Label, hud.js :855) stays for its test; the HUD shows this.
        /// </summary>
        public static string Word(string name, int stacks)
        {
            string n = name == null ? "" : name.ToUpperInvariant();
            return stacks > 1 ? n + "×" + stacks.ToString(System.Globalization.CultureInfo.InvariantCulture) : n;
        }

        /// <summary>
        /// Rule 8: red-orange is enemies and danger only, magenta RIVAL only. A saturated, bright colour with a hue up to 30.5
        /// degrees or from 285 on is refused (greyish ones pass: they read as neither).
        /// </summary>
        public static bool Rule8Ok(uint rgb)
        {
            double r = ((rgb >> 16) & 0xFF) / 255.0, g = ((rgb >> 8) & 0xFF) / 255.0, b = (rgb & 0xFF) / 255.0;
            double max = System.Math.Max(r, System.Math.Max(g, b)), min = System.Math.Min(r, System.Math.Min(g, b));
            double sat = max > 0 ? (max - min) / max : 0;
            if (sat < 0.35 || max < 0.35) return true;
            double d = max - min, h;
            if (max == r) h = 60 * (((g - b) / d) % 6);
            else if (max == g) h = 60 * ((b - r) / d + 2);
            else h = 60 * ((r - g) / d + 4);
            if (h < 0) h += 360;
            return h > 30.5 && h < 285;
        }
    }

    public sealed partial class Ui
    {
        /// <summary>NIGHT17 U3-30.7: the perks' row centre, just over the rebuilt vitals plate (its height + UiMoveRow's growth).</summary>
        float PerkRowY17 => margin + UnityTuning.Hud17VitalsHeight + UnityTuning.MoveRowPlateGrow + UnityTuning.Hud17PerkGap + UnityTuning.PerkIconSize * 0.5f;

        readonly List<UnityEngine.UI.Image> perkIcons = new List<UnityEngine.UI.Image>();
        readonly Dictionary<string, Sprite> perkSprites = new Dictionary<string, Sprite>();
        int shownPerks = -1;
        int shownModChips = -1; // NIGHT16 polish #60: the mods' version the chips show
        readonly List<(UnityEngine.UI.Image plate, UnityEngine.UI.Image line, TMPro.TextMeshProUGUI label)> modChips = new List<(UnityEngine.UI.Image, UnityEngine.UI.Image, TMPro.TextMeshProUGUI)>();

        Sprite PerkSprite(string id)
        {
            if (perkSprites.TryGetValue(id, out Sprite s)) return s;
            List<string> rows = cfg.Strs($"perks.{id}.icon");
            int n = rows.Count;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    bool on = x < rows[n - 1 - y].Length && rows[n - 1 - y][x] == '#';
                    tex.SetPixel(x, y, on ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            perkSprites[id] = s = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            return s;
        }

        // NIGHT18 U3-31.6 (FeelAB on): a newly owned perk's icon enters (grows from 0.6x about its middle and fades in:
        // HudEntrance18) instead of appearing whole; off: NIGHT17's row. Per icon: its entrance age, its colour at rest, the
        // scale shown (written only while it moves).
        readonly List<double> perkEnterAge18 = new List<double>();
        readonly List<Color> perkColor18 = new List<Color>();
        readonly List<float> perkShownScale18 = new List<float>();

        void RenderPerks(double frameDt)
        {
            RenderPerksLayout();
            for (int i = 0; i < perkEnterAge18.Count && i < perkIcons.Count; i++)
            {
                if (!perkIcons[i].enabled) continue;
                float s = 1, a = 1;
                if (FeelAB.On && !HudEntrance18.Done(perkEnterAge18[i]))
                {
                    perkEnterAge18[i] = HudEntrance18.Step(perkEnterAge18[i], frameDt);
                    s = HudEntrance18.Scale(perkEnterAge18[i]);
                    a = HudEntrance18.Alpha(perkEnterAge18[i]);
                }
                if (s == perkShownScale18[i]) continue;
                perkShownScale18[i] = s;
                perkIcons[i].rectTransform.localScale = new Vector3(s, s, 1);
                perkIcons[i].color = UiTheme.A(perkColor18[i], a);
            }
        }

        void RenderPerksLayout()
        {
            List<string> owned = game.perks.owned;
            Mods mods = game.mods;
            int modsVersion = mods != null ? mods.version : 0;
            if (owned.Count == shownPerks && modsVersion == shownModChips) return;
            shownModChips = modsVersion;
            if (owned.Count == shownPerks)
            {
                LayoutModChips(mods, owned.Count);
                return;
            }
            int before = System.Math.Max(0, shownPerks);
            shownPerks = owned.Count;
            LayoutModChips(mods, owned.Count);
            while (perkIcons.Count < owned.Count)
            {
                int i = perkIcons.Count;
                var img = Img(values, $"Perk {i}", Color.white, Vector2.zero, Vector2.zero, new Vector2(margin + i * (UnityTuning.PerkIconSize + 6), PerkRowY17), new Vector2(UnityTuning.PerkIconSize, UnityTuning.PerkIconSize));
                img.rectTransform.pivot = Vector2.zero;
                perkIcons.Add(img);
                perkEnterAge18.Add(double.MaxValue);
                perkColor18.Add(Color.white);
                perkShownScale18.Add(1);
            }
            for (int i = 0; i < perkIcons.Count; i++)
            {
                bool on = i < owned.Count;
                perkIcons[i].enabled = on;
                if (!on) continue;
                perkIcons[i].sprite = PerkSprite(owned[i]);
                perkIcons[i].color = perkColor18[i] = Look.C(cfg, $"perks.{owned[i]}.color");
                if (i >= before) PerkEnter18(i);
            }
        }

        /// <summary>NIGHT18: icon `i` starts its entrance (its pivot to the middle, the place kept, so it grows about it).</summary>
        void PerkEnter18(int i)
        {
            perkEnterAge18[i] = 0;
            RectTransform rt = perkIcons[i].rectTransform;
            if (rt.pivot == Vector2.zero)
            {
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition += new Vector2(UnityTuning.PerkIconSize * 0.5f, UnityTuning.PerkIconSize * 0.5f);
            }
        }

        /// <summary>NIGHT16 polish #60: the owned mods' chips after the `perks` icons (on a change only).</summary>
        void LayoutModChips(Mods mods, int perks)
        {
            // NIGHT17 round 1's fix list: the chip's whole word (ModChip.Word, never the web's 3-letter cut) at S18 BODY-BOLD on
            // an INK plate (no translucent grey), its 2 px line in the tag's colour.
            float x = margin + perks * (UnityTuning.PerkIconSize + 6), h = UnityTuning.Hud17ModChipHeight;
            int used = 0;
            if (mods != null && UnityTuning.Hud17ModChipsOnHud) // round 5: the mods live on PAUSED, not the HUD at rest
            {
                foreach (string id in mods.ids)
                {
                    if (!mods.stacks.TryGetValue(id, out int k) || k <= 0 || !mods.defs.TryGetValue(id, out ModDef d)) continue;
                    if (used == modChips.Count)
                    {
                        var p = Img(values, $"Mod chip {used}", UiTheme.Ink, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, h));
                        p.rectTransform.pivot = Vector2.zero;
                        var ln = Img(p.transform, "Line", UiTheme.White, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, UnityTuning.Hud17ModChipBar));
                        ln.rectTransform.pivot = new Vector2(0.5f, 0);
                        var t = Hud17Text(p.transform, "Label", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.White, UiTheme.BodyBold, TMPro.TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, h), false);
                        t.characterSpacing = UnityTuning.ThemeLabelTracking;
                        t.rectTransform.anchorMin = Vector2.zero;
                        t.rectTransform.anchorMax = Vector2.one;
                        t.rectTransform.sizeDelta = Vector2.zero;
                        modChips.Add((p, ln, t));
                    }
                    var c = modChips[used++];
                    uint rgb = mods.TagColor(d.tag);
                    Color col = ModChip.Rule8Ok(rgb) ? Look.Hex(rgb) : UiTheme.White;
                    c.label.text = ModChip.Word(d.name, k);
                    c.label.color = UiTheme.White; // round 2's reviewer: white words on INK; the tag's colour only as the bar (gold is points only)
                    c.line.color = col;
                    float w = Mathf.Ceil(c.label.GetPreferredValues(c.label.text).x) + 2 * UnityTuning.Hud17ModChipPad;
                    c.plate.rectTransform.sizeDelta = new Vector2(w, h);
                    c.plate.rectTransform.anchoredPosition = new Vector2(x, PerkRowY17);
                    if (!c.plate.gameObject.activeSelf) c.plate.gameObject.SetActive(true);
                    x += w + 4; // .hud-perks gap 4px
                }
            }
            for (int i = used; i < modChips.Count; i++) if (modChips[i].plate.gameObject.activeSelf) modChips[i].plate.gameObject.SetActive(false);
        }
    }
}
