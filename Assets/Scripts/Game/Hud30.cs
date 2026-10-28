// NIGHT30 builder V (design.n30.hud30; P0 c / e / f and the card's layout for item 2). View only: no sim read changes, no tape, no fingerprint.
// Every change here is behind N30Fixes.ViewRowOn(cfg, "hud30"), so -skygrindOff30 (or the row off) draws NIGHT29's HUD (the RED frame).
//  - P0 c: the run's rows hide on TITLE, PAUSE and RESULTS (and under SETTINGS opened from them). The HUD's own layers already hid there
//    (Ui.Render: frame / values / sight), but two run rows live elsewhere: Hud29's canvas (the boss GAP bar, the DOORS OPEN tag: shot 190310)
//    and the lesson line on the meta layer (drawn over every screen). Both hide while a menu shows; the lesson keeps its own fade state
//    (it is scaled to 0, not faded), so it comes back as it was when the menu closes.
//  - P0 e: the style meter's multiplier chip ("x1.2 AIR") stands clear of the rank plate (Hud30ChipClearX: it lay over the plate's slanted edge in
//    shot 170227); "PULSE READY" and the freshness word ("TRIGGER STALE") go: the weapon-state widget (HudWeapons30.cs) says each weapon's state
//    in its own slot. The check that finds any collision is Hud30Layout (the reel `layout30`).
//  - P0 f: the green "1/2" badge by the score is the REFUND power-up's chip (UnityTuning.Hud17PowerGlyphs: refund = "½"; it counts nothing: it
//    means "your next buy costs half"). The campaign has nothing to buy, so with hud30 on the chip does not show in a campaign run; in a survival
//    run it stays (its banner names it on the grab).
// Wiring (Ui.cs, this file's owner's two lines): BuildHud30() at the end of BuildHud; RenderHud30(f, frameDt) before the HUD's early return.
using System;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT30 view statics (the reels read them; the Ui instance is private to SurvivalGame).</summary>
    public static class Hud30
    {
        /// <summary>design.n30.hud30 is live this session (not -skygrindOff30).</summary>
        public static bool On;
        /// <summary>A menu (TITLE / PAUSE / RESULTS / SETTINGS) shows: the run rows are hidden.</summary>
        public static bool MenuHidden;
        /// <summary>Run rows drawn this frame (the lesson line, Hud29's gap bar or DOORS OPEN tag): the reel `menus30` wants 0 under a menu.</summary>
        public static int RunRowsDrawn;
        /// <summary>The lesson line is drawn this frame (on screen, not hidden by a menu, alpha over 0).</summary>
        public static bool LessonDrawn;
        /// <summary>The REFUND chip was kept off the HUD this frame (a campaign run, P0 f).</summary>
        public static bool RefundHidden;

        /// <summary>A slot turned READY (its id: a gun id, "punch" or "blast"): GG's sound hook (no audio here: CLAUDE.md rule 7).</summary>
        public static event Action<string> WeaponReady;
        internal static void RaiseReady(string id) => WeaponReady?.Invoke(id);
    }

    public sealed partial class Ui
    {
        bool hud30On;

        void BuildHud30()
        {
            BuildLava30(); // item 3: the lava hurt flash (HudLava30.cs; its own gate: the run's lava block)
            BuildFx30(); // item 8: the HUD's scanlines / flicker / glow / buzz (HudFx30.cs; its own gate: design.n30.look30)
            hud30On = N30Fixes.ViewRowOn(cfg, "hud30");
            Hud30.On = hud30On;
            if (!hud30On) return;
            // P0 e: the multiplier chip clear of the rank plate; round 4 (the lead's r1 Lantern frame: left of the block it floated in the open by the top
            // line): DOCKED as a tab under the plate's right end, following the plate as it grows (DockChip30, every frame)
            if (styleChipRt != null) styleChipRt.pivot = new Vector2(1, 1);
            // item 2: the hotbar's silhouette row gives its place to the weapon-state widget (the card keeps its meter and its role header)
            if (hotRow != null) hotRow.gameObject.SetActive(false);
            // the lead's round-1 frame (shot-N30-weaponhud30-pips): the role word ("TRIGGER", the old "TRIGGER STALE" header's first half) sat over the
            // slots: it goes with the row on; the header keeps the gun's NAME on a swap only
            if (weaponHeadTag != null) weaponHeadTag.gameObject.SetActive(false);
            BuildWeapons30();
            weaponBlockDirty = true;
        }

        void RenderHud30(Flow f, double frameDt)
        {
            RenderLava30(frameDt);
            RenderFx30(frameDt);
            if (!hud30On) return;
            bool menu = f == Flow.Title || f == Flow.Paused || f == Flow.Results || settingsOpen;
            Hud30.MenuHidden = menu;
            Hud29.MenuHidden = menu;
            int rows = 0;
            if (lesson25Rt != null)
            {
                Vector3 s = menu ? Vector3.zero : Vector3.one;
                if (lesson25Rt.localScale != s) lesson25Rt.localScale = s;
                Hud30.LessonDrawn = !menu && lesson25Rt.gameObject.activeInHierarchy && lesson25Group != null && lesson25Group.alpha > 0f;
                if (Hud30.LessonDrawn) rows++;
            }
            if (Hud29View.DrawnNow) rows++; // its canvas hides on the view's next Update (one frame later): the reel waits frames before it reads
            Hud30.RunRowsDrawn = rows;
            DockChip30();
            RenderWeapons30(frameDt);
        }

        Vector2 chipDockShown30 = new Vector2(float.NaN, 0);

        /// <summary>Round 4: the "x1.2 AIR" chip as a tab under the rank plate's right end (the plate grows with the callouts: followed each frame,
        /// written only on change). The plate and the chip share the style block's root and its left-middle anchor.</summary>
        void DockChip30()
        {
            if (styleChipRt == null || styleColPlate == null) return;
            RectTransform p = styleColPlate.rectTransform;
            var at = new Vector2(p.anchoredPosition.x + p.sizeDelta.x, p.anchoredPosition.y - p.sizeDelta.y - UnityTuning.Hud30ChipDockGap);
            if (at == chipDockShown30) return;
            chipDockShown30 = at;
            styleChipRt.anchoredPosition = at;
        }

        /// <summary>P0 f: the REFUND chip is kept off a campaign run's HUD (nothing to buy there). False with hud30 off.</summary>
        bool Hud30HidesPower(int type)
        {
            bool hide = hud30On && PowerUps30Refund(type) && game.setup != null && game.setup.mode == Skygrind.Core.RunSetup.Campaign;
            if (PowerUps30Refund(type)) Hud30.RefundHidden = hide;
            return hide;
        }

        static bool PowerUps30Refund(int type) => type >= 0 && type < Skygrind.Core.PowerUps.Types.Length && Skygrind.Core.PowerUps.Types[type] == "refund";

        /// <summary>
        /// Item 2: the weapon card with the widget, bottom up: padding, the meter (as wide as the row), the widget's slots, the role header (no
        /// freshness word), padding; the plate as wide as its widest row. Runs in place of NIGHT19's FitWeaponBlock body while hud30 is on.
        /// </summary>
        void FitWeaponBlock30()
        {
            float x = weaponRight, y = UnityTuning.Hud17StripPad;
            float rowW = Math.Max(w30RowWidth, UnityTuning.Hud30SlotPitch);
            float headW = weaponNameW; // the header holds the gun's name on a swap only (the role word is hidden with the row on)
            float innerW = Math.Max(rowW, headW);
            meterBack.rectTransform.anchoredPosition = new Vector2(x, weaponBase + y);
            meterBack.rectTransform.sizeDelta = new Vector2(rowW, UnityTuning.Hud17StripMeterH);
            if (altMeterRoot != null)
            {
                altMeterRoot.anchoredPosition = new Vector2(x, weaponBase + y);
                altMeterRoot.sizeDelta = new Vector2(rowW, UnityTuning.Hud17StripMeterH);
            }
            y += UnityTuning.Hud17StripMeterH + UnityTuning.Hud17StripGap;
            if (w30Row != null) w30Row.anchoredPosition = new Vector2(x, weaponBase + y);
            y += Weapons30RowHeight + UnityTuning.Hud19CardHeadGap;
            if (weaponHeadTag != null)
            {
                float mid = weaponBase + y + UnityTuning.Hud19CardHeadH * 0.5f;
                weaponSwapName.rectTransform.anchoredPosition = new Vector2(x, mid);
                weaponHeadTag.rectTransform.anchoredPosition = new Vector2(x, mid);
                weaponHeadTier.rectTransform.anchoredPosition = new Vector2(x, mid);
            }
            y += UnityTuning.Hud19CardHeadH + UnityTuning.Hud17StripPad;
            weaponPlate.rectTransform.sizeDelta = new Vector2(innerW + 2 * UnityTuning.Hud17StripPad + weaponEdge, y);
        }
    }
}
