// NIGHT13 U3-26.9 (agent H): the weapon block, bottom right (hud.js .hud-mount's .hud-plate.w, :331-336; render :615-645
// and :839-843; main.js specialHint :2226-2235, freshOf :2237, hudInfo.owned / slot / special / fresh :2382-2385).
// - The plate: the web's .hud-plate.w (rgba(ui.theme.panel, hud.panelOpacity), a cyan right edge, padding, skewed with
//   design.ceremony.hudMotion like the port's other plates; without the flag the vitals panel's colour, straight). It
//   starts where Ui.cs put the weapon line and fits its rows' width whenever a row's text changes.
// - The rows, top to bottom as the web's: the hotbar; the slot label over the weapon's name (UiFeel's line, moved into
//   the plate); the meter; the special hint (gone when the gun has none: the plate shrinks, as the web's display: none).
// - The hotbar (POWER P3): every gun you own in purchase order, up to weapons.hotbarShown cells, a window that scrolls to
//   keep loadout.targetSlot in view (first = clamp(slot - shown / 2)); each cell its key label (n + 1) % 10 for the first
//   ten, its glyph (the web's name fallback: the name's first word, five letters; no SVG art in the port), a FRESH bar in
//   the family colour (arsenal.Fresh, 1 without the arsenal), the glyph dimmed under weapons.arsenal.staleAt; "+N" past
//   the window ("N+" before it). The cell in hand is cyan with its inset line.
// - The slot label: "KEY n" (n = (slot + 1) % 10), the radial's key past the tenth (hud.js 'TAB'). On a pad (port-only):
//   no digits in the cells, the slot label says how the pad cycles (UnityTuning.HudPadCycle*).
// - The meter (weapons.js meter(): Loadout.Meter): heat, charge or reload, 160 x 4; red-orange while hot, the lock's
//   colour with hud-deny's 4 px jitter while locked; hidden (its space kept, the web's visibility) for a gun without one.
// - The special hint (POLISH V5): RMB ZOOM xN on a zoom gun, plus the SPECIAL key with ui.specialHints[gun]; RMB and the
//   label on the rest; none when the gun has neither. On a pad (port-only) LT / L2 and D-PAD UP.
// NIGHT19 U3-33.8 (builder H): the card gains a header row over the slots: the gun in hand's ROLE (WeaponRole19: TRIGGER,
// POINT BLANK, BATTERY ...) and its freshness tier word (W's read: HudReads19), and the swap's NAME slams in there instead of
// under the crosshair (S14); the meter row shows the alt's meter (the KESTREL's gust charges, W's read) for a gun without
// a heat / charge / reload meter.
// PORT-RULES section 6: the cells and labels are made once; per frame only compares (the owned list by element, the
// freshness as hundredths, the meter as hundredths); strings are cached per gun / slot / device, texts and colours set
// only on change. The block has its own nested canvas (the fresh bars and the meter change often).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class HotCell
        {
            public RectTransform rt;
            public UnityEngine.UI.Image back, onLine, freshFill, sil, pip; // NIGHT17 round 2: the slot's silhouette and freshness pip
            public TextMeshProUGUI key, glyph;
            public int tier = -1; // the pip's tier shown (0 FRESH, 1 USED, 2 STALE, 3 none)
            public string id; // the gun shown (null: the cell is hidden)
            public int fresh = -1; // the bar shown, in hundredths (hud.js f.toFixed(2))
            public bool on, stale;
            public CanvasGroup grp; // NIGHT25 (T-12, M9): the cell fades in when it first shows
            public double fadeAge = double.MaxValue;
        }

        HotCell[] hotCells;
        RectTransform weaponBlock, hotRow;
        TextMeshProUGUI hotMore, weaponSlot, specialText;
        UnityEngine.UI.Image weaponPlate, meterBack, meterFill;
        int hotShown, hotFirst = -1, hotCount = -1, hotVisible;
        bool hotPad, hotMoreOn;
        float hotRowWidth, weaponBase, weaponRight, weaponEdge;
        bool weaponBlockDirty = true;
        Color hotCellColor, hotOnColor, hotGlyphColor, hotGlyphStale, meterHot, meterLocked;
        string radialKey; // the radial's key (input.bindings.radial: Tab -> TAB)
        string[] slotLabels; // "KEY 1" .. "KEY 0", then the radial's key
        static readonly string[] Digit = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        float hotCellWidth = UnityTuning.HudHotbarCellWidth; // NIGHT16 U3-29.7: widened to the widest short word (never a cut word)
        TMPro.TextMeshProUGUI wheelHintText; // NIGHT16 U3-29.7: the hold-Y wheel hint over the hotbar (pad players, 3+ guns)
        int shownSlotLabel = -1; // index into slotLabels, or -2 / -3 for the pad's two glyph sets
        int shownSlotPrompt20 = -1; // NIGHT20: PromptText.Version when the line was written
        int shownMeter = -1; // hundredths; -1 hidden
        int shownMeterState = -1; // 0 cool, 1 hot, 2 locked
        float shownMeterShift = -1;
        double meterClock;
        string textHex, faintHex;

        /// <summary>Automation (U3-26.9's smoke step): the hotbar's first shown slot and how many cells show.</summary>
        public int HotbarFirst => hotFirst;
        public int HotbarVisible => hotVisible;

        partial void BuildHotbar()
        {
            hotShown = Math.Max(0, Math.Min(UnityTuning.Hud17StripCells, cfg.Int("weapons.hotbarShown"))); // NIGHT17 round 2: the strip's slots
            radialKey = KeyLabel(cfg, "radial") ?? "TAB";
            slotLabels = new string[11];
            for (int i = 0; i < 10; i++) slotLabels[i] = "KEY " + Digit[i];
            slotLabels[10] = radialKey;
            textHex = ColorUtility.ToHtmlStringRGB(text);
            Color faint = text;
            faint.a = UnityTuning.HudFaintAlpha; // theme.js --faint: rgba(text, 0.16)
            faintHex = ColorUtility.ToHtmlStringRGBA(faint);
            // NIGHT17 U3-30.7 / round 2: the slots in the theme's tokens: INK-SOFT, the one in hand CYAN-filled; the silhouette
            // WHITE (INK on the cyan slot), MUTED when stale (no translucent white cells).
            hotCellColor = UiTheme.InkSoft;
            hotOnColor = UiTheme.Cyan;
            hotGlyphColor = UiTheme.White;
            hotGlyphStale = UiTheme.Muted;
            meterHot = Look.Hex(UnityTuning.HudMeterHotColor);
            meterLocked = Look.Hex(UnityTuning.HudMeterLockedColor);

            // Where Ui.cs put the weapon line is where the plate starts (its bottom); the rows sit inside its padding.
            float edge = weaponEdge = (float)cfg.Num("ui.theme.edgeWidth");
            RectTransform nameRt = weapon.rectTransform;
            weaponBase = nameRt.anchoredPosition.y;
            weaponRight = nameRt.anchoredPosition.x - UnityTuning.HudWeaponPadRight - edge;

            // The plate (the static frame layer: it changes only when a row's width does). NIGHT17 U3-30.7: the weapon CARD
            // (Neon White): INK with the theme's cut, a 4 px cyan accent bar on its right edge; no translucent grey plate.
            weaponEdge = edge = UnityTuning.ThemeAccentBar;
            weaponRight = nameRt.anchoredPosition.x - UnityTuning.Hud17StripPad - edge;
            weapon.gameObject.SetActive(false); // NIGHT17 round 2: no name at rest (it shows on a swap, under the crosshair)
            weaponPlate = Img(frame, "Weapon plate", UiTheme.Ink, new Vector2(1, 0), new Vector2(1, 0), new Vector2(nameRt.anchoredPosition.x, weaponBase), new Vector2(0, 0));
            weaponPlate.rectTransform.pivot = new Vector2(1, 0);
            weaponPlate.sprite = UiTheme.Cut;
            weaponPlate.type = UnityEngine.UI.Image.Type.Sliced;
            var plateEdge = Img(weaponPlate.transform, "Edge", UiTheme.Cyan, new Vector2(1, 0), new Vector2(1, 1), Vector2.zero, new Vector2(edge, -2 * UnityTuning.ThemeCut));
            plateEdge.rectTransform.pivot = new Vector2(1, 0.5f);

            // The rows (their own canvas under the values layer, so they show with the HUD).
            weaponBlock = SubCanvas(values, "Weapon block");
            weaponSlot = Text(weaponBlock, "Weapon slot", "", tSmall, usable, TextAlignmentOptions.Right, Vector2.zero, new Vector2(nameRt.sizeDelta.x, UnityTuning.HudWeaponSlotLine), new Vector2(1, 0));
            weaponSlot.characterSpacing = UnityTuning.HudWeaponSlotSpacing;
            weaponSlot.gameObject.SetActive(false); // NIGHT17 U3-30.7: "KEY 1" is a key hint: the teaching tier, gone at rest
            weaponSil = Img(weaponBlock, "Weapon silhouette", UiTheme.White, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, new Vector2(UnityTuning.Hud17CardSilW, UnityTuning.Hud17CardSilH));
            weaponSil.rectTransform.pivot = new Vector2(1, 0.5f);
            weaponSil.preserveAspect = true;
            weaponSil.enabled = false;
            weaponSil.gameObject.SetActive(false); // NIGHT17 round 2: the slots carry the silhouettes
            // NIGHT17 round 2: the gun's name only on a swap, for Hud17SwapNameTime. NIGHT19 U3-33.8 (S14: the centre is clear):
            // no longer under the crosshair: it slams into the card's header row (the card is its plate), where the gun's ROLE
            // sits at rest (S18 BODY-BOLD, CYAN-PALE, tracked caps) with its freshness tier word (W's read, design.freshness).
            var headAnchor = new Vector2(1, 0);
            var headPivot = new Vector2(1, 0.5f);
            weaponSwapName = Hud17Text(weaponBlock, "Weapon name (swap)", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Right, headAnchor, headPivot, Vector2.zero, new Vector2(600, UnityTuning.Hud19CardHeadH), false);
            weaponSwapName.characterSpacing = UnityTuning.ThemeLabelTracking;
            weaponSwapName.gameObject.SetActive(false);
            weaponHeadTag = Hud17Text(weaponBlock, "Weapon role", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Right, headAnchor, headPivot, Vector2.zero, new Vector2(600, UnityTuning.Hud19CardHeadH), false);
            weaponHeadTag.characterSpacing = UnityTuning.ThemeLabelTracking;
            weaponHeadTier = Hud17Text(weaponBlock, "Weapon tier", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.Cyan, UiTheme.BodyBold, TextAlignmentOptions.Right, headAnchor, headPivot, Vector2.zero, new Vector2(200, UnityTuning.Hud19CardHeadH), false);
            weaponHeadTier.characterSpacing = UnityTuning.ThemeLabelTracking;
            weaponHeadTier.gameObject.SetActive(false);

            meterBack = Img(weaponBlock, "Weapon meter", UiTheme.InkSoft, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, new Vector2(UnityTuning.HudMeterWidth, UnityTuning.Hud17StripMeterH));
            meterBack.rectTransform.pivot = new Vector2(1, 0);
            meterBack.gameObject.AddComponent<UnityEngine.UI.RectMask2D>(); // .hud-wbar overflow: hidden (the lock's jitter)
            meterFill = Img(meterBack.transform, "Fill", usable, Vector2.zero, new Vector2(0, 1), Vector2.zero, Vector2.zero);
            meterFill.rectTransform.pivot = new Vector2(0, 0.5f);
            meterBack.enabled = meterFill.enabled = false;
            meterGroup20 = meterBack.gameObject.AddComponent<CanvasGroup>(); // NIGHT20 fix H3: its entrance (M9)
            meterGroup20.blocksRaycasts = meterGroup20.interactable = false;
            // NIGHT19 U3-33.8: the alt's meter in the same row, for a gun without a heat / charge / reload meter: the KESTREL's
            // gust charges (HudReads19.AltMeter, builder W's read), one slanted cell per charge, CYAN when ready, the next one
            // filling; hidden when there is no read.
            altMeterRoot = Hud17Rect(weaponBlock, "Alt meter", new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, new Vector2(UnityTuning.HudMeterWidth, UnityTuning.Hud17StripMeterH));
            altSegBack = new UiShape[UnityTuning.Hud19AltMaxSegs];
            altSegFill = new UiShape[UnityTuning.Hud19AltMaxSegs];
            for (int i = 0; i < altSegBack.Length; i++)
            {
                altSegBack[i] = Hud17Shape(altMeterRoot, $"Charge {i}", UiTheme.InkSoft, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(10, UnityTuning.Hud17StripMeterH));
                altSegFill[i] = Hud17Shape(altSegBack[i].rectTransform, "Fill", UiTheme.Cyan, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10, UnityTuning.Hud17StripMeterH));
                altSegBack[i].SetCuts(0, 0);
                altSegFill[i].SetCuts(0, 0);
                altSegBack[i].gameObject.SetActive(false);
            }
            altMeterRoot.gameObject.SetActive(false);
            altGroup20 = altMeterRoot.gameObject.AddComponent<CanvasGroup>(); // NIGHT20 fix H3: its entrance (M9)
            altGroup20.blocksRaycasts = altGroup20.interactable = false;

            specialText = Text(weaponBlock, "Special hint", "", tSmall, muted, TextAlignmentOptions.Right, Vector2.zero, new Vector2(nameRt.sizeDelta.x, UnityTuning.HudSpecialLine), new Vector2(1, 0));
            specialText.characterSpacing = UnityTuning.HudSpecialSpacing;
            specialText.richText = true;
            specialText.gameObject.SetActive(false);

            hotRow = new GameObject("Hotbar", typeof(RectTransform)).GetComponent<RectTransform>();
            hotRow.SetParent(weaponBlock, false);
            hotRow.anchorMin = hotRow.anchorMax = hotRow.pivot = new Vector2(1, 0);
            hotRow.sizeDelta = new Vector2(0, UnityTuning.HudHotbarCellHeight);
            hotCells = new HotCell[hotShown];
            var cellSize = new Vector2(UnityTuning.Hud17StripCellW, UnityTuning.Hud17StripCellH);
            hotCellWidth = UnityTuning.Hud17StripCellW;
            hotRow.sizeDelta = new Vector2(0, UnityTuning.Hud17StripCellH);
            for (int i = 0; i < hotShown; i++)
            {
                var c = new HotCell();
                c.back = Img(hotRow, $"Hotbar cell {i}", hotCellColor, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, cellSize);
                c.rt = c.back.rectTransform;
                c.grp = c.back.gameObject.AddComponent<CanvasGroup>();
                c.rt.pivot = new Vector2(1, 0);
                c.onLine = Img(c.rt, "On", usable, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, UnityTuning.HudHotbarOnLine));
                c.onLine.rectTransform.pivot = new Vector2(0.5f, 0);
                c.onLine.enabled = false;
                var fr = Img(c.rt, "Fresh", new Color(1, 1, 1, UnityTuning.HudHotbarFreshBackAlpha), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, UnityTuning.HudHotbarFreshBottom), new Vector2(-2 * UnityTuning.HudHotbarFreshInset, UnityTuning.HudHotbarFreshHeight));
                fr.rectTransform.pivot = new Vector2(0.5f, 0);
                c.freshFill = Img(fr.transform, "Fill", usable, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                c.freshFill.rectTransform.pivot = new Vector2(0, 0.5f);
                c.glyph = Text(c.rt, "Glyph", "", tSmall, hotGlyphColor, TextAlignmentOptions.Bottom, new Vector2(0, UnityTuning.HudHotbarGlyphBottom), new Vector2(UnityTuning.HudHotbarCellWidth, UnityTuning.HudHotbarGlyphLine), new Vector2(0.5f, 0));
                c.glyph.characterSpacing = UnityTuning.HudHotbarGlyphSpacing;
                c.glyph.overflowMode = TextOverflowModes.Overflow;
                c.key = Text(c.rt, "Key", "", tSmall, muted, TextAlignmentOptions.TopLeft, new Vector2(UnityTuning.HudHotbarKeyX, 0), new Vector2(UnityTuning.HudHotbarCellWidth - UnityTuning.HudHotbarKeyX, UnityTuning.HudHotbarKeyLine), new Vector2(0, 1));
                // NIGHT17 round 2: a slot is its gun's silhouette (no name, no key digit, no family bar) and a freshness pip.
                c.glyph.gameObject.SetActive(false);
                c.key.gameObject.SetActive(false);
                fr.gameObject.SetActive(false);
                c.onLine.gameObject.SetActive(false);
                c.sil = Img(c.rt, "Silhouette", hotGlyphColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud17StripSilW, UnityTuning.Hud17StripSilH));
                c.sil.preserveAspect = true;
                c.pip = Img(c.rt, "Fresh pip", UiTheme.CyanHot, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-UnityTuning.Hud17StripPipInset, -UnityTuning.Hud17StripPipInset), new Vector2(UnityTuning.Hud17StripPip, UnityTuning.Hud17StripPip));
                c.pip.rectTransform.pivot = new Vector2(1, 1);
                c.pip.enabled = false;
                c.back.sprite = UiTheme.Cut;
                c.back.type = UnityEngine.UI.Image.Type.Sliced;
                c.back.gameObject.SetActive(false);
                hotCells[i] = c;
            }
            // NIGHT16 U3-29.7: the hold-Y wheel hint, right-aligned just over the hotbar row.
            wheelHintText = Text(hotRow, "Wheel hint", "", tSmall, usable, TextAlignmentOptions.BottomRight, new Vector2(0, UnityTuning.WheelHintGap), new Vector2(400, UnityTuning.HudHotbarGlyphLine), new Vector2(1, 1));
            wheelHintText.rectTransform.anchorMin = wheelHintText.rectTransform.anchorMax = new Vector2(1, 1);
            wheelHintText.rectTransform.pivot = new Vector2(1, 0);
            wheelHintText.rectTransform.anchoredPosition = new Vector2(0, UnityTuning.WheelHintGap);
            wheelHintText.gameObject.SetActive(false);
            hotMore = Text(hotRow, "More", "", tSmall, muted, TextAlignmentOptions.Right, Vector2.zero, new Vector2(80, UnityTuning.HudHotbarCellHeight), new Vector2(1, 0));
            hotMore.gameObject.SetActive(false);
            weaponBlockDirty = true;
        }

        /// <summary>A full-size nested canvas under `parent` (a layer of its own: its changes rebuild only its batch).</summary>
        static RectTransform SubCanvas(Transform parent, string name)
        {
            var rt = new GameObject(name, typeof(RectTransform), typeof(Canvas)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        partial void RenderHotbar(double frameDt)
        {
            if (hotCells == null || !values.gameObject.activeSelf) return;
            Loadout lo = game.loadout;
            if (lo == null) return;
            WeaponAttach18();
            for (int i = 0; i < hotShown; i++)
            {
                HotCell fc = hotCells[i];
                if (fc.fadeAge >= UnityTuning.Fade25In) continue;
                fc.fadeAge += frameDt;
                fc.grp.alpha = UiFade25.In(fc.fadeAge, frameDt);
            }
            List<string> owned = lo.owned;
            int n = owned.Count;
            int slot = lo.targetSlot;
            bool pad = PlayerDevices.Last == InputDevice.Pad;
            int first = n <= hotShown ? 0 : Math.Max(0, Math.Min(n - hotShown, slot - hotShown / 2));

            // The window's guns (hud.js 'hotList': owned.join() | first), compared element by element.
            bool listChanged = first != hotFirst || n != hotCount || pad != hotPad;
            for (int i = 0; i < hotShown && !listChanged; i++)
            {
                string id = first + i < n ? owned[first + i] : null;
                if (id != hotCells[i].id) listChanged = true;
            }
            if (listChanged) LayoutHotbar(owned, first, pad);

            // Per cell: in hand, the FRESH bar, stale.
            Arsenal ars = lo.arsenal;
            bool fresh = ars != null && ars.enabled; // main.js: hudInfo.fresh = loadout.arsenal.enabled ? freshOf : null
            double staleAt = ars != null ? ars.Params.staleAt : 0;
            for (int i = 0; i < hotVisible; i++)
            {
                HotCell c = hotCells[i];
                bool on = first + i == slot;
                double f = fresh ? ars.Fresh(c.id) : 1;
                bool stale = f < staleAt;
                if (on != c.on || stale != c.stale)
                {
                    // NIGHT17 round 2: the gun in hand CYAN-filled with an INK silhouette; the rest INK-SOFT, WHITE or MUTED (stale)
                    c.on = on;
                    c.stale = stale;
                    c.back.color = on ? hotOnColor : hotCellColor;
                    c.sil.color = on ? UiTheme.InkSolid : stale ? hotGlyphStale : hotGlyphColor;
                }
                // NIGHT17 round 2 (FRESH / USED / STALE was jargon): a pip in the ladder's value, no word; none without the arsenal.
                int tier = !fresh || game.style == null ? 3 : FreshPipTier(game.style.TierName(f * game.style.Params.max));
                int pipKey = tier * 2 + (on ? 1 : 0); // the pip's look depends on the slot it sits on
                if (pipKey != c.tier)
                {
                    c.tier = pipKey;
                    c.pip.enabled = tier < 3;
                    // on the cyan slot the ladder reads in INK (CYAN-HOT / CYAN would vanish), MUTED stays MUTED
                    if (tier < 3) c.pip.color = on && tier < 2 ? UiTheme.InkSolid : tier == 0 ? UiTheme.CyanHot : tier == 1 ? UiTheme.Cyan : UiTheme.Muted;
                }
            }

            // The slot label over the name: KEY n / the radial's key; on a pad, how the pad cycles.
            // NIGHT15 U3-28.8: a Nintendo pad prints X at the north button (UnityTuning.PadWheel.cs).
            int label = pad ? (PlayerDevices.Glyphs == GlyphSet.PlayStation ? -3 : PlayerDevices.Glyphs == GlyphSet.Nintendo ? -4 : -2) : Math.Max(0, Math.Min(10, slot));
            int promptVersion = PromptText.Version; // NIGHT20 U3-34.10: a rebind or a new layout (v3: RB) refreshes the line
            if (label != shownSlotLabel || promptVersion != shownSlotPrompt20)
            {
                shownSlotLabel = label;
                shownSlotPrompt20 = promptVersion;
                // NIGHT20 U3-34.10: the cycle line follows the live layout (SpecialHint20.CycleLine: v3's RB NEXT), not v2's fixed
                // "Y NEXT"; with design.pad.n19 on PromptText draws it from the layout itself (it knows the fixed lines as its cue)
                string today = label == -3 ? UnityTuning.HudPadCyclePlayStation : label == -4 ? UnityTuning.HudPadCycleNintendo : UnityTuning.HudPadCycleNeutral;
                if (!PlayerDevices.N19) today = SpecialHint20.CycleLine(PlayerDevices.Layout, PlayerDevices.Glyphs);
                weaponSlot.text = label == -2 || label == -3 || label == -4 ? PromptText.For("nextWeapon", false, today) : slotLabels[label]; // NIGHT19: PromptText (builder P)
                weaponBlockDirty = true;
            }

            RenderWeaponMeter(lo, frameDt, slot >= 0 && slot < n ? owned[slot] : lo.Weapon);
            // NIGHT17 U3-30.7: the special hint (RMB ZOOM x1.5, the SPECIAL key) and the pad's HOLD Y wheel hint are the
            // teaching tier: gone at rest (round 1's fix list; onboarding teaches the wheel: builder N's UiOnboarding). Its text
            // stays built, never shown (its rule deleted at NIGHT71 D2).
            RenderWeaponCard(frameDt);
        }

        // --- NIGHT17 U3-30.7 / round 2: the guns as silhouettes (Neon White: a picture, not a name plus key hints): one per
        // slot in the strip; the gun's name only on a swap -------------------------------------------------------------------
        UnityEngine.UI.Image weaponSil; // the first pass's card silhouette (built hidden)
        string weaponCardId;
        static readonly Dictionary<string, Sprite> WeaponSilhouettes = new Dictionary<string, Sprite>();

        TextMeshProUGUI weaponSwapName; // NIGHT17 round 2: the gun's name on a swap (NIGHT19: in the card's header row)
        TextMeshProUGUI weaponHeadTag, weaponHeadTier; // NIGHT19 U3-33.8: the role at rest and the freshness tier word
        double weaponSwapAge = double.MaxValue;
        string weaponCardName;
        float weaponTagW, weaponTierW, weaponNameW, weaponTagAlphaShown = -1, weaponNameScaleShown = -1;
        int weaponTierShown = -2;
        // NIGHT19 U3-33.8: the alt's meter (the gust's charges), in the meter's row
        RectTransform altMeterRoot;
        UiShape[] altSegBack, altSegFill;
        HudReads19.Alt weaponAlt;
        int altSegsShown = -1;
        float altWidthShown = -1;
        readonly int[] altSegTint = { -1, -1, -1, -1, -1, -1, -1, -1 };

        /// <summary>The gun in hand (or its name: a forge's "+") changed (RenderFeel): the header's role and name; a swap slams the
        /// name into the header for Hud17SwapNameTime (not at a run's first gun). Measured on change only.</summary>
        void WeaponCardSet(string id, string name)
        {
            if (id == weaponCardId && ReferenceEquals(name, weaponCardName)) return;
            bool first = weaponCardId == null, swapped = id != weaponCardId;
            weaponCardId = id;
            weaponCardName = name;
            if (weaponSwapName == null) return;
            string tag = WeaponRole19.Tag(id) ?? string.Empty; // cached strings (UnityTuning.Hud19Roles)
            weaponHeadTag.text = tag;
            weaponTagW = tag.Length > 0 ? weaponHeadTag.GetPreferredValues(tag).x : 0;
            if (!string.IsNullOrEmpty(name))
            {
                weaponSwapName.text = name; // a cached name string (WeaponName / ForgedName)
                weaponNameW = weaponSwapName.GetPreferredValues(name).x;
            }
            weaponBlockDirty = true; // the card fits its header (FitWeaponBlock, later this frame)
            if (first || !swapped || string.IsNullOrEmpty(name)) return;
            weaponSwapAge = 0;
        }

        // NIGHT18 U3-31.9 backlog 5 (FeelAB on; the web's default HUD STYLE, settings.hudStyle "attached"): the weapon block (its
        // plate in the frame layer, its rows' canvas in the values layer) rides the gun's sway + bob (HudAttach18, written by the
        // viewmodel) and rolls with it about the plate's bottom-right corner. CLASSIC, REDUCED MOTION or FeelAB off: still.
        // Written only when the 0.1 px / 0.01 deg values change.
        double weaponAttachK18 = -1, weaponAttachHud18;
        Vector2 weaponPlateHome18, weaponAttachShown18;
        float weaponAttachRollShown18;

        void WeaponAttach18()
        {
            if (weaponAttachK18 < 0)
            {
                weaponAttachHud18 = cfg.Num("viewmodel.hudAttach");
                weaponAttachK18 = cfg.Num("hud.attachPxPerMeter");
                weaponPlateHome18 = weaponPlate.rectTransform.anchoredPosition;
            }
            SettingsView sv = game.settings.View;
            bool on = FeelAB.On && HudAttach18.Attached(sv.hudStyle) && !sv.reducedMotion;
            Vector2 off = on ? HudAttach18.Offset(HudAttach18.X, HudAttach18.Y, weaponAttachHud18, weaponAttachK18) : Vector2.zero;
            float roll = on ? HudAttach18.Roll(HudAttach18.RollDeg, weaponAttachHud18) : 0f;
            if (off == weaponAttachShown18 && roll == weaponAttachRollShown18) return;
            weaponAttachShown18 = off;
            weaponAttachRollShown18 = roll;
            Quaternion turn = Quaternion.Euler(0, 0, roll);
            weaponPlate.rectTransform.anchoredPosition = weaponPlateHome18 + off; // its pivot is that corner: it turns about it
            weaponPlate.rectTransform.localRotation = turn;
            // the rows' canvas is stretched over the layer (its pivot the layer's middle): the same turn about the plate's corner
            Rect r = values.rect;
            var corner = new Vector2(r.width * 0.5f + weaponPlateHome18.x, -r.height * 0.5f + weaponPlateHome18.y);
            Vector2 turned = turn * corner;
            weaponBlock.localRotation = turn;
            weaponBlock.anchoredPosition = off + corner - turned;
        }

        /// <summary>
        /// NIGHT19 U3-33.8: the card's header (CardHead19): on a swap the name slams in (from 1.6x at full alpha, ease-out, its
        /// last 100 ms fading) for Hud17SwapNameTime, the role hidden; then the role and its tier enter again (alpha, 150 ms).
        /// The tier word is W's read (HudReads19.FreshTier, design.freshness): FRESH CYAN-HOT, USED CYAN, STALE / DULL MUTED.
        /// Written only on change.
        /// </summary>
        void RenderWeaponCard(double frameDt)
        {
            if (weaponSwapName == null) return;
            int tier = HudReads19.FreshTier(game, weaponCardId);
            if (tier != weaponTierShown)
            {
                weaponTierShown = tier;
                bool tierOn = tier >= 0 && !hud30On; // NIGHT30 P0 e (design.n30.hud30): the freshness word goes ("TRIGGER STALE"); the widget says the state
                weaponHeadTier.gameObject.SetActive(tierOn);
                weaponTierW = 0;
                if (tierOn)
                {
                    string word = HudReads19.TierWord(tier);
                    weaponHeadTier.text = word;
                    weaponHeadTier.color = tier == 0 ? UiTheme.CyanHot : tier == 1 ? UiTheme.Cyan : UiTheme.Muted;
                    weaponTierW = weaponHeadTier.GetPreferredValues(word).x;
                }
                weaponBlockDirty = true;
            }
            double age = weaponSwapAge;
            bool nameOn = game.flow == Flow.Playing && CardHead19.NameShown(age);
            if (weaponSwapName.gameObject.activeSelf != nameOn) weaponSwapName.gameObject.SetActive(nameOn);
            if (nameOn)
            {
                float s = CardHead19.NameScale(age);
                if (s != weaponNameScaleShown)
                {
                    weaponNameScaleShown = s;
                    weaponSwapName.rectTransform.localScale = new Vector3(s, s, 1);
                }
                float a = CardHead19.NameAlpha(age) * UiFade25.In(age, frameDt); // NIGHT25 (T-12, M9): no full-alpha first frame under the slam
                if (weaponSwapName.alpha != a) weaponSwapName.alpha = a;
            }
            float ta = CardHead19.TagAlpha(age);
            if (ta != weaponTagAlphaShown)
            {
                weaponTagAlphaShown = ta;
                weaponHeadTag.alpha = ta;
                weaponHeadTier.alpha = ta;
            }
            weaponSwapAge = age < UnityTuning.Hud17SwapNameTime + UnityTuning.Hud19CardTagIn ? age + frameDt : double.MaxValue;
        }

        /// <summary>NIGHT19 U3-33.8: the alt's meter (W's read): one slanted cell per charge, CYAN when ready, CYAN-DEEP filling.</summary>
        void RenderAltMeter(double frameDt)
        {
            if (altMeterRoot == null) return;
            bool on = weaponAlt.valid;
            if (altMeterRoot.gameObject.activeSelf != on)
            {
                altMeterRoot.gameObject.SetActive(on);
                if (on) altEnterAge20 = 0; // NIGHT20 fix H3 (M9: it popped in whole on a swap): it fades in
            }
            if (!on) return;
            MeterFade20(altGroup20, ref altEnterAge20, frameDt);
            int n = Math.Min(altSegBack.Length, weaponAlt.max);
            float width = altMeterRoot.sizeDelta.x;
            if (n != altSegsShown || width != altWidthShown)
            {
                altSegsShown = n;
                altWidthShown = width;
                float gap = UnityTuning.Hud19AltSegGap, w = n > 0 ? (width - (n - 1) * gap) / n : 0;
                var size = new Vector2(w, UnityTuning.Hud17StripMeterH);
                for (int i = 0; i < altSegBack.Length; i++)
                {
                    bool show = i < n;
                    if (altSegBack[i].gameObject.activeSelf != show) altSegBack[i].gameObject.SetActive(show);
                    if (!show) continue;
                    altSegBack[i].rectTransform.anchoredPosition = new Vector2(i * (w + gap), 0);
                    altSegBack[i].rectTransform.sizeDelta = size;
                    altSegFill[i].rectTransform.sizeDelta = size;
                }
            }
            for (int i = 0; i < n; i++)
            {
                bool ready = i < weaponAlt.charges;
                altSegFill[i].Fill = ready ? 1f : i == weaponAlt.charges ? (float)Math.Max(0, Math.Min(1, weaponAlt.next)) : 0f;
                int t = ready ? 1 : 0;
                if (t == altSegTint[i]) continue;
                altSegTint[i] = t;
                altSegFill[i].color = ready ? UiTheme.Cyan : UiTheme.CyanDeep;
            }
        }

        // NIGHT20 fix H3 (build 2's reel, swapall f74 / f109 / f138): the meter row's two meters fade in when they show
        CanvasGroup meterGroup20, altGroup20;
        double meterEnterAge20 = -1, altEnterAge20 = -1;

        /// <summary>A meter's entrance: drawn at its age (its first frame one 60 Hz frame in: HudEntrance18.Fade, ~0.23 alpha),
        /// then stepped; whole after Hud18FadeInTime. Written only while it runs.</summary>
        static void MeterFade20(CanvasGroup g, ref double age, double frameDt)
        {
            if (g == null || age < 0) return;
            age = HudEntrance18.Step(age, frameDt);
            bool done = age >= UnityTuning.Hud18FadeInTime;
            g.alpha = done ? 1f : HudEntrance18.Fade(age);
            if (done) age = -1;
        }

        /// <summary>The freshness tier's pip: 0 FRESH, 1 USED, 2 STALE (and DULL with the LAB's teeth), by the tier names.</summary>
        static int FreshPipTier(string tier) => tier == "FRESH" ? 0 : tier == "USED" ? 1 : 2;

        /// <summary>A gun's silhouette (UnityTuning.Hud17Silhouettes' rects on a 32 x 16 grid, along the theme's slant), made once per gun; the revolver's for a gun without one.</summary>
        static Sprite WeaponSilhouette(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (WeaponSilhouettes.TryGetValue(id, out Sprite cached)) return cached;
            if (!UnityTuning.Hud17Silhouettes.TryGetValue(id, out float[] rects)) rects = UnityTuning.Hud17Silhouettes["revolver"];
            int w = UnityTuning.Hud17CardSilTex, h = w / 2;
            float unit = w / 32f, k = Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad);
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Silhouette " + id };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 4 samples per pixel (a soft edge); the slant: un-skew x about the middle row before the rect test
                    int hits = 0;
                    for (int sy = 0; sy < 2; sy++)
                        for (int sx = 0; sx < 2; sx++)
                        {
                            float v = (y + 0.25f + sy * 0.5f) / unit, u = (x + 0.25f + sx * 0.5f) / unit;
                            u -= (v - 8f) * k * 0.6f; // a little less than the full slant: the card keeps the gun readable
                            for (int r = 0; r + 3 < rects.Length; r += 4)
                                if (u >= rects[r] && u < rects[r] + rects[r + 2] && v >= rects[r + 1] && v < rects[r + 1] + rects[r + 3]) { hits++; break; }
                        }
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(hits * 255 / 4));
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            Sprite s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            WeaponSilhouettes[id] = s;
            return s;
        }

        /// <summary>The window changed (a buy, a scroll, the device): each cell's gun, key, glyph, colour; the row's layout.</summary>
        void LayoutHotbar(List<string> owned, int first, bool pad)
        {
            int n = owned.Count;
            hotFirst = first;
            hotCount = n;
            hotPad = pad;
            int visible = 0;
            for (int i = 0; i < hotShown; i++)
            {
                HotCell c = hotCells[i];
                string id = first + i < n ? owned[first + i] : null;
                c.id = id;
                bool show = id != null;
                if (c.back.gameObject.activeSelf != show)
                {
                    c.back.gameObject.SetActive(show);
                    if (show)
                    {
                        c.fadeAge = 0; // the cell, its silhouette and pip enter together
                        c.grp.alpha = 0;
                    }
                }
                if (!show) continue;
                visible++;
                c.sil.sprite = WeaponSilhouette(id); // NIGHT17 round 2: the slot's glyph (made once per gun)
                c.tier = -1;
                // Back to the plain look; RenderHotbar sets in hand, the bar and stale again this frame.
                c.fresh = -1;
                c.on = c.stale = false;
                c.back.color = hotCellColor;
                c.onLine.enabled = false;
                c.sil.color = hotGlyphColor;
            }
            hotVisible = visible;
            // "+N" past the window, "N+" before it (hud.js .more), at the row's right end.
            int rest = n - first - hotShown;
            bool more = rest > 0 || first > 0;
            if (more != hotMoreOn)
            {
                hotMoreOn = more;
                hotMore.gameObject.SetActive(more);
            }
            float moreW = 0;
            if (more)
            {
                if (rest > 0) hotMore.SetText("+{0:0}", rest);
                else hotMore.SetText("{0:0}+", first);
                moreW = UnityTuning.Hud17StripMoreW + UnityTuning.Hud17StripGap;
            }
            // NIGHT17 round 2: the slots right-aligned; "+N" / "N+" at the row's left end.
            float step = hotCellWidth + UnityTuning.Hud17StripGap;
            for (int i = 0; i < visible; i++) hotCells[i].rt.anchoredPosition = new Vector2(-((visible - 1 - i) * step), 0);
            float slotsW = visible > 0 ? visible * step - UnityTuning.Hud17StripGap : 0;
            if (more) hotMore.rectTransform.anchoredPosition = new Vector2(-(slotsW + UnityTuning.Hud17StripGap), 0);
            hotRowWidth = slotsW + moreW;
            weaponBlockDirty = true;
        }

        /// <summary>weapons.js meter() under the name: heat (nailgun), charge (rail) or reload (rocket); hidden otherwise.</summary>
        void RenderWeaponMeter(Loadout lo, double frameDt, string inHand)
        {
            bool has = lo.Meter(out double value, out bool hot, out bool locked);
            // NIGHT19 U3-33.8: a gun without its own meter may show its alt's (the gust's charges: W's read)
            if (has) weaponAlt.valid = false;
            else HudReads19.AltMeter(game, inHand, ref weaponAlt);
            RenderAltMeter(frameDt);
            int q = has ? (int)Math.Round(Math.Max(0, Math.Min(1, value)) * 100, MidpointRounding.AwayFromZero) : -1;
            if ((q >= 0) != (shownMeter >= 0))
            {
                meterBack.enabled = meterFill.enabled = q >= 0; // visibility: its space stays
                if (q >= 0) meterEnterAge20 = 0; // NIGHT20 fix H3 (M9: it re-entered whole on a swap): it fades in
            }
            if (q >= 0) MeterFade20(meterGroup20, ref meterEnterAge20, frameDt);
            if (q != shownMeter)
            {
                shownMeter = q;
                if (q >= 0) meterFill.rectTransform.anchorMax = new Vector2(q / 100f, 1);
            }
            if (q < 0) return;
            int state = locked ? 2 : hot ? 1 : 0;
            if (state != shownMeterState)
            {
                shownMeterState = state;
                meterFill.color = state == 2 ? meterLocked : state == 1 ? meterHot : usable;
                meterClock = 0;
            }
            float shift = 0;
            if (locked)
            {
                meterClock += frameDt;
                shift = (int)(meterClock / UnityTuning.HudMeterDenyStep) % 2 == 1 ? UnityTuning.HudMeterDenyShift : 0;
            }
            if (shift != shownMeterShift)
            {
                shownMeterShift = shift;
                RectTransform rt = meterFill.rectTransform;
                rt.offsetMin = new Vector2(shift, 0);
                rt.offsetMax = new Vector2(shift, 0);
            }
        }

        /// <summary>
        /// The plate and its rows, bottom up (hud.js's column reversed): padding, the special hint (when shown), the meter, the
        /// name with the slot label over it, the hotbar, padding; the plate as wide as its widest row. Run when a row changed
        /// (after the name's text was set this frame: RenderFeel calls it).
        /// </summary>
        /// <summary>
        /// NIGHT17 round 2's reviewer: the strip, bottom up: padding, the meter (as wide as the slots), the slots, padding; the
        /// plate as wide as the slots (at most 6 + "+N": about 360 x 64 px). Run when the slots changed.
        /// </summary>
        void FitWeaponBlock()
        {
            if (hotCells == null) return;
            weaponBlockDirty = false;
            if (hud30On) { FitWeaponBlock30(); return; } // NIGHT30 item 2 (design.n30.hud30): the weapon-state widget's row in the slots' place (Hud30.cs)
            float x = weaponRight, y = UnityTuning.Hud17StripPad;
            float slotsW = Math.Max(hotRowWidth, UnityTuning.Hud17StripCellW);
            // NIGHT19 U3-33.8: the header row over the slots (the role + its tier at rest, the name on a swap), as wide as its
            // widest text for the gun in hand, so the plate keeps one width through a swap (Hud19Layout.CardInnerWidth)
            float tierPart = weaponTierW > 0 ? weaponTierW + UnityTuning.Hud19CardTierGap : 0;
            float headW = Math.Max(weaponTagW + tierPart, weaponNameW);
            float rowW = Math.Max(slotsW, headW);
            meterBack.rectTransform.anchoredPosition = new Vector2(x, weaponBase + y);
            meterBack.rectTransform.sizeDelta = new Vector2(slotsW, UnityTuning.Hud17StripMeterH);
            if (altMeterRoot != null)
            {
                altMeterRoot.anchoredPosition = new Vector2(x, weaponBase + y);
                altMeterRoot.sizeDelta = new Vector2(slotsW, UnityTuning.Hud17StripMeterH);
            }
            y += UnityTuning.Hud17StripMeterH + UnityTuning.Hud17StripGap;
            hotRow.anchoredPosition = new Vector2(x, weaponBase + y);
            y += UnityTuning.Hud17StripCellH + UnityTuning.Hud19CardHeadGap;
            if (weaponHeadTag != null)
            {
                float mid = weaponBase + y + UnityTuning.Hud19CardHeadH * 0.5f;
                weaponSwapName.rectTransform.anchoredPosition = new Vector2(x, mid);
                weaponHeadTier.rectTransform.anchoredPosition = new Vector2(x, mid);
                weaponHeadTag.rectTransform.anchoredPosition = new Vector2(x - tierPart, mid);
            }
            y += UnityTuning.Hud19CardHeadH + UnityTuning.Hud17StripPad;
            weaponPlate.rectTransform.sizeDelta = new Vector2(rowW + 2 * UnityTuning.Hud17StripPad + weaponEdge, y);
        }
    }
}
