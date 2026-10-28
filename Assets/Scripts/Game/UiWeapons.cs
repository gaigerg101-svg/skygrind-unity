// NIGHT12 U3-25.10 (agent F): the WEAPONS screen on TITLE. Per gun with a MASTERY track (game.mastery.Guns(), catalog
// order): its name, its tier as pips (3 tonight: design.mastery), the current track's bar ("KILLS 12 / 25": the first
// track short of its target; MASTERED when none is), and the finishes it may wear as swatches: four-tone chips of the
// finish's palette (Finishes.PaletteOf: bone, gunmetal, dark, grip; FACTORY = the guns' own tones, BlockyKit), the one in
// use framed in cyan. Clicking a swatch wears it: game.mastery.Choose, then game.SaveProfile() (the profile, NIGHT12 rule
// 6). menus.js's GUN FINISH select is the web's version (a settings row); this screen replaces it per gun. Rows are built
// on first need and filled when the screen opens or a finish is chosen, never per frame.
// NIGHT19 U3-33.8 (builder H): each row is a ROLE CARD too (GG: "each weapon has its own purpose"): the gun's role tag on a
// CYAN slanted plate by its name (TRIGGER, POINT BLANK, BATTERY ...) and a band at the row's foot: GOOD AGAINST and COMBOS
// WITH (UnityTuning.Hud19Roles; MUTED labels, WHITE / CYAN-PALE values). NIGHT18's leftover (the body appeared at once): the
// rows cascade in along the slant on open (RowCascade19), each tag popping after its row (RenderWeapons19, from RenderMenus).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class SwatchView
        {
            public UnityEngine.UI.Image frame;
            public readonly UnityEngine.UI.Image[] tones = new UnityEngine.UI.Image[4];
        }

        sealed class WeaponRowView
        {
            public UnityEngine.UI.Image back, edge, fill;
            public TextMeshProUGUI name, tier, track, finish;
            // NIGHT19 U3-33.8: the role card: the tag on its CYAN plate by the name, the GOOD AGAINST / COMBOS WITH band
            public UiShape rolePlate;
            public TextMeshProUGUI roleTag, goodLabel, goodText, combosLabel, combosText;
            public CanvasGroup group;
            public Vector2 home;
            public float shownAlpha = -1, shownTagScale = -1;
            public Vector2 shownOffset = new Vector2(float.NaN, 0);
            public readonly UnityEngine.UI.Image[] pips = new UnityEngine.UI.Image[UnityTuning.MasteryPips];
            public readonly List<SwatchView> swatches = new List<SwatchView>();
            public string weapon;
            public readonly List<string> finishes = new List<string>();
        }

        GameObject weaponsScreen;
        TextMeshProUGUI weaponsHead, weaponsEmpty;
        UnityEngine.UI.ScrollRect weaponsScroll;
        RectTransform weaponsContent;
        readonly List<WeaponRowView> weaponRows = new List<WeaponRowView>();

        void BuildWeapons()
        {
            weaponsScreen = Screen("Weapons", UiTheme.A(UiTheme.InkSolid, UnityTuning.ThemeInkAlpha)); // NIGHT17 U3-30.10: the menus' INK
            weaponsHead = MenuHead(weaponsScreen.transform, "WEAPONS", UnityTuning.MetaHeadY); // NIGHT17 U3-30.10
            (weaponsScroll, weaponsContent) = ScrollList(weaponsScreen.transform, "Weapon list");
            weaponsEmpty = Text(weaponsScreen.transform, "Empty", "NO MASTERY TRACKS YET", tBody, muted, TextAlignmentOptions.Center, Vector2.zero, new Vector2(UnityTuning.MetaListWidth, UnityTuning.LineBody), new Vector2(0.5f, 0.5f));
            Button(weaponsScreen.transform, "BACK", new Vector2(-UnityTuning.MetaSwitchX, UnityTuning.MetaBackY), CloseMeta);
            Button(weaponsScreen.transform, "CONTRACTS", new Vector2(UnityTuning.MetaSwitchX, UnityTuning.MetaBackY), () => OpenContracts()); // NIGHT17: TITLE's PROGRESS holds both
        }

        partial void OpenWeapons()
        {
            OpenMeta(MetaScreen.Weapons);
            if (metaOpen != MetaScreen.Weapons) return;
            if (codexScroll20 != null) CodexShow20(false); // NIGHT20 E3: WEAPONS opens on its GUNS tab (UiCodex20.cs)
            else RefreshWeapons();
        }

        WeaponRowView WeaponRow(int i)
        {
            while (weaponRows.Count <= i)
            {
                var v = new WeaponRowView();
                v.back = Row(weaponsContent, $"Gun {weaponRows.Count}", UnityTuning.Hud19WeaponRowHeight, out v.edge); // NIGHT19: + the role band
                v.edge.color = usable;
                Transform t = v.back.transform;
                float pad = UnityTuning.MetaRowPad;
                v.name = Text(t, "Name", "", tH3, text, TextAlignmentOptions.Left, new Vector2(pad, -UnityTuning.WeaponNameY), new Vector2(UnityTuning.WeaponNameWidth, UnityTuning.LineBody + UnityTuning.LineGapSmall), new Vector2(0, 1));
                v.name.fontStyle = FontStyles.Bold;
                v.name.characterSpacing = UnityTuning.NameSpacing;
                for (int p = 0; p < v.pips.Length; p++)
                {
                    v.pips[p] = Img(t, $"Tier {p + 1}", faint, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.TierPipSize, UnityTuning.TierPipSize));
                    v.pips[p].rectTransform.pivot = new Vector2(0, 1);
                    v.pips[p].rectTransform.anchoredPosition = new Vector2(pad + p * (UnityTuning.TierPipSize + UnityTuning.TierPipGap), -UnityTuning.WeaponPipY);
                }
                float tierX = pad + v.pips.Length * (UnityTuning.TierPipSize + UnityTuning.TierPipGap) + UnityTuning.TierPipGap;
                v.tier = Text(t, "Tier", "", tSmall, muted, TextAlignmentOptions.Left, new Vector2(tierX, -UnityTuning.WeaponTierY), new Vector2(UnityTuning.WeaponTierWidth, UnityTuning.LineSmall), new Vector2(0, 1));
                v.track = Text(t, "Track", "", tSmall, text, TextAlignmentOptions.Left, new Vector2(pad, -UnityTuning.WeaponTrackY), new Vector2(UnityTuning.WeaponBarWidth, UnityTuning.LineSmall), new Vector2(0, 1));
                v.track.overflowMode = TextOverflowModes.Ellipsis;
                v.fill = Bar(t, new Vector2(0, 1), new Vector2(pad, -UnityTuning.WeaponBarY), UnityTuning.WeaponBarWidth, out _);
                v.finish = Text(t, "Finish", "", tSmall, muted, TextAlignmentOptions.Left, new Vector2(UnityTuning.FinishX, -UnityTuning.FinishLabelY), new Vector2(UnityTuning.MetaListWidth - UnityTuning.FinishX - pad, UnityTuning.LineSmall), new Vector2(0, 1));
                WeaponRoleCard19(v, t, pad);
                weaponRows.Add(v);
            }
            return weaponRows[i];
        }

        SwatchView Swatch(WeaponRowView v, int rowIndex, int s)
        {
            while (v.swatches.Count <= s)
            {
                int index = v.swatches.Count;
                var sw = new SwatchView();
                // A small button without a label (the menus' tints), re-anchored to the row's top left; its image is the frame.
                TextMeshProUGUI label = SmallButton(v.back.transform, "", Vector2.zero, new Vector2(UnityTuning.SwatchWidth, UnityTuning.SwatchHeight), () => ChooseFinish(rowIndex, index));
                label.gameObject.SetActive(false);
                sw.frame = label.transform.parent.GetComponent<UnityEngine.UI.Image>();
                sw.frame.name = $"Finish {index}";
                RectTransform fr = sw.frame.rectTransform;
                fr.anchorMin = fr.anchorMax = fr.pivot = new Vector2(0, 1);
                fr.anchoredPosition = new Vector2(UnityTuning.FinishX + index * (UnityTuning.SwatchWidth + UnityTuning.SwatchGap), -UnityTuning.SwatchY);
                sw.frame.color = faint;
                float inner = UnityTuning.SwatchWidth - 2 * UnityTuning.SwatchFrame, tone = inner / sw.tones.Length;
                for (int k = 0; k < sw.tones.Length; k++)
                {
                    var img = Img(sw.frame.transform, $"Tone {k}", Color.white, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, Vector2.zero);
                    img.rectTransform.pivot = new Vector2(0, 0.5f);
                    img.rectTransform.offsetMin = new Vector2(UnityTuning.SwatchFrame + k * tone, UnityTuning.SwatchFrame);
                    img.rectTransform.offsetMax = new Vector2(UnityTuning.SwatchFrame + (k + 1) * tone, -UnityTuning.SwatchFrame);
                    sw.tones[k] = img;
                }
                v.swatches.Add(sw);
            }
            return v.swatches[s];
        }

        /// <summary>Fill the screen from game.mastery (on open).</summary>
        void RefreshWeapons()
        {
            List<Mastery.Gun> guns = game.mastery != null ? game.mastery.Guns() : null;
            int n = guns != null ? guns.Count : 0;
            weaponsEmpty.gameObject.SetActive(n == 0);
            float step = UnityTuning.Hud19WeaponRowHeight + UnityTuning.WeaponRowGap;
            for (int i = 0; i < n; i++)
            {
                WeaponRowView v = WeaponRow(i);
                v.back.gameObject.SetActive(true);
                v.home = new Vector2(0, -i * step);
                v.back.rectTransform.anchoredPosition = v.home;
                FillWeaponRow(i, guns[i]);
            }
            for (int i = n; i < weaponRows.Count; i++) weaponRows[i].back.gameObject.SetActive(false);
            ResetList(weaponsScroll, weaponsContent, n * step);
            // NIGHT19 U3-33.8: the rows cascade in from here (RenderWeapons19), drawn at their start this frame
            weaponsShownRows19 = n;
            weaponsEnterAge19 = 0;
            RenderWeapons19(0);
        }

        // --- NIGHT19 U3-33.8 (builder H): the role cards ---------------------------------------------------------------------
        double weaponsEnterAge19 = double.MaxValue;
        int weaponsShownRows19;

        /// <summary>A row's role card: the tag's CYAN plate (by the name; placed when filled) and the band at its foot.</summary>
        void WeaponRoleCard19(WeaponRowView v, Transform t, float pad)
        {
            v.group = v.back.gameObject.AddComponent<CanvasGroup>();
            float px = UiTheme.Px(UiTheme.Size.S18);
            v.rolePlate = Hud17Shape(t, "Role plate", UiTheme.Cyan, new Vector2(0, 1), new Vector2(0, 0.5f), Vector2.zero, new Vector2(100, UnityTuning.Hud19RoleTagH));
            v.roleTag = Hud17Text(v.rolePlate.transform, "Role", "", px, UiTheme.InkSolid, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100, UnityTuning.Hud19RoleTagH), false);
            v.roleTag.characterSpacing = UnityTuning.ThemeLabelTracking;
            float y = -(UnityTuning.Hud19RoleBandY + UnityTuning.Hud19RoleLineH * 0.5f);
            var left = new Vector2(0, 1);
            var leftMid = new Vector2(0, 0.5f);
            v.goodLabel = Hud17Text(t, "Good against label", "GOOD AGAINST", px, UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.Left, left, leftMid, new Vector2(pad, y), new Vector2(200, UnityTuning.Hud19RoleLineH), false);
            v.goodLabel.characterSpacing = UnityTuning.ThemeLabelTracking;
            float gx = pad + v.goodLabel.GetPreferredValues("GOOD AGAINST").x + UnityTuning.Hud19RoleLabelGap;
            v.goodText = Hud17Text(t, "Good against", "", px, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Left, left, leftMid, new Vector2(gx, y), new Vector2(UnityTuning.Hud19CombosX - gx - pad, UnityTuning.Hud19RoleLineH), false);
            v.combosLabel = Hud17Text(t, "Combos with label", "COMBOS WITH", px, UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.Left, left, leftMid, new Vector2(UnityTuning.Hud19CombosX, y), new Vector2(200, UnityTuning.Hud19RoleLineH), false);
            v.combosLabel.characterSpacing = UnityTuning.ThemeLabelTracking;
            float cx = UnityTuning.Hud19CombosX + v.combosLabel.GetPreferredValues("COMBOS WITH").x + UnityTuning.Hud19RoleLabelGap;
            v.combosText = Hud17Text(t, "Combos with", "", px, UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Left, left, leftMid, new Vector2(cx, y), new Vector2(UnityTuning.MetaListWidth - cx - pad, UnityTuning.Hud19RoleLineH), false);
        }

        /// <summary>Fill a row's role card (on open): the tag by the name, the two lines; a gun with no role shows none.</summary>
        void FillWeaponRole19(WeaponRowView v, string weapon)
        {
            string tag = WeaponRole19.Tag(weapon);
            bool on = tag != null;
            v.rolePlate.gameObject.SetActive(on);
            v.goodLabel.gameObject.SetActive(on);
            v.goodText.gameObject.SetActive(on);
            v.combosLabel.gameObject.SetActive(on);
            v.combosText.gameObject.SetActive(on);
            if (!on) return;
            v.roleTag.text = tag;
            float tw = v.roleTag.GetPreferredValues(tag).x + 2 * UnityTuning.Hud19RoleTagPadX;
            float nameW = v.name.GetPreferredValues(v.name.text).x;
            RectTransform rt = v.rolePlate.rectTransform;
            rt.sizeDelta = new Vector2(tw, UnityTuning.Hud19RoleTagH);
            v.roleTag.rectTransform.sizeDelta = rt.sizeDelta;
            rt.anchoredPosition = new Vector2(UnityTuning.MetaRowPad + nameW + UnityTuning.Hud19RoleTagGap, -(UnityTuning.WeaponNameY + (UnityTuning.LineBody + UnityTuning.LineGapSmall) * 0.5f));
            v.goodText.text = WeaponRole19.GoodAgainst(weapon);
            v.combosText.text = WeaponRole19.CombosWith(weapon);
        }

        /// <summary>
        /// Frame time while WEAPONS shows (UiMenus.RenderMenus): the rows cascade in along the slant (RowCascade19; REDUCED
        /// MOTION: the fade only), each tag plate popping after its row. Written only while it moves; still once at rest.
        /// </summary>
        void RenderWeapons19(double frameDt)
        {
            RenderCodex20(frameDt); // NIGHT20 E3: the CODEX tab's cascade and its list following the pad (UiCodex20.cs)
            if (weaponsScreen == null || !weaponsScreen.activeSelf || weaponsEnterAge19 == double.MaxValue) return;
            double age = weaponsEnterAge19;
            bool still = game.settings.View.reducedMotion;
            int n = Math.Min(weaponsShownRows19, weaponRows.Count);
            for (int i = 0; i < n; i++)
            {
                WeaponRowView v = weaponRows[i];
                float a = RowCascade19.Alpha(age, i);
                if (a != v.shownAlpha)
                {
                    v.shownAlpha = a;
                    v.group.alpha = a;
                }
                Vector2 off = RowCascade19.Offset(age, i, still);
                if (off != v.shownOffset)
                {
                    v.shownOffset = off;
                    v.back.rectTransform.anchoredPosition = v.home + off;
                }
                float s = RowCascade19.TagScale(age, i);
                if (s != v.shownTagScale)
                {
                    v.shownTagScale = s;
                    v.rolePlate.rectTransform.localScale = new Vector3(s, s, 1);
                }
            }
            weaponsEnterAge19 = RowCascade19.Done(age, n) ? double.MaxValue : age + frameDt;
        }

        void FillWeaponRow(int i, Mastery.Gun g)
        {
            WeaponRowView v = weaponRows[i];
            v.weapon = g.weapon;
            v.name.text = g.name ?? WeaponName(g.weapon);
            FillWeaponRole19(v, g.weapon); // NIGHT19 U3-33.8: its role card
            for (int p = 0; p < v.pips.Length; p++) v.pips[p].color = p < g.tier ? usable : faint;
            v.tier.text = g.tier > 0 ? $"MASTERY {g.tier} / {UnityTuning.MasteryPips}" : "MASTERY -";
            // The current track: the first short of its target (the tier being worked on); none short = mastered.
            Mastery.Track current = null;
            foreach (Mastery.Track tr in g.tracks)
            {
                if (tr.value < tr.target)
                {
                    current = tr;
                    break;
                }
            }
            if (current != null)
            {
                v.track.text = (current.name ?? current.id ?? string.Empty) + " " + Progress(current.value, current.target);
                v.track.color = text;
                SetBar(v.fill, current.target > 0 ? current.value / current.target : 0);
            }
            else
            {
                v.track.text = g.tracks.Count > 0 ? "MASTERED" : string.Empty;
                v.track.color = usable;
                SetBar(v.fill, g.tracks.Count > 0 ? 1 : 0);
            }
            // The finishes it may wear (unlocked; FACTORY first), the one in use framed in cyan.
            v.finishes.Clear();
            v.finishes.AddRange(g.finishes);
            string worn = g.finish ?? (v.finishes.Count > 0 ? v.finishes[0] : null);
            v.finish.text = worn != null ? "FINISH: " + Finishes.Name(cfg, worn) : string.Empty;
            for (int s = 0; s < v.finishes.Count; s++)
            {
                SwatchView sw = Swatch(v, i, s);
                sw.frame.gameObject.SetActive(true);
                sw.frame.color = v.finishes[s] == worn ? usable : faint;
                Finishes.Palette pal = Finishes.PaletteOf(cfg, v.finishes[s]);
                sw.tones[0].color = pal != null ? Look.Hex(pal.bone) : BlockyKit.Bone;
                sw.tones[1].color = pal != null ? Look.Hex(pal.gunmetal) : BlockyKit.Gunmetal;
                sw.tones[2].color = pal != null ? Look.Hex(pal.dark) : BlockyKit.Dark;
                sw.tones[3].color = pal != null ? Look.Hex(pal.grip) : BlockyKit.Grip;
            }
            for (int s = v.finishes.Count; s < v.swatches.Count; s++) v.swatches[s].frame.gameObject.SetActive(false);
        }

        /// <summary>A swatch clicked: wear that finish (the profile changes: saved at once).</summary>
        void ChooseFinish(int row, int s)
        {
            if (game.mastery == null || row >= weaponRows.Count) return;
            WeaponRowView v = weaponRows[row];
            if (v.weapon == null || s >= v.finishes.Count) return;
            if (game.mastery.Choose(v.weapon, v.finishes[s])) game.SaveProfile();
            Mastery.Gun g = game.mastery.Of(v.weapon);
            if (g != null) FillWeaponRow(row, g);
        }
    }
}
