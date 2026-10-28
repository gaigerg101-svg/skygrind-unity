// NIGHT20 U3-34.10 (builder H): the shockwave's chip (GG 2026-10-06 evening: "there needs to be a long, long cooldown";
// NIGHT20.md U3-34.3's H line): a ring round the alt-fire glyph (LT / L2 / ZL as the pad's sprite, RMB on the mouse:
// PromptText) on a tab standing on the weapon card over the THUNDERCLAP's slot, so it reads while the gun is holstered.
// - The ring (UiRing20): an INK-SOFT track, the fill CYAN-DEEP while the next charge comes back (clockwise from 12
//   o'clock), CYAN once a charge is ready; the INK-SOLID disc inside it.
// - Inside: the seconds left (rounded up) while no charge is ready and more than 3 s remain; else the glyph (get ready).
//   The FORGE's second charge: two pips under the glyph, CYAN when ready.
// - A charge's return: the disc flashes CYAN-HOT -> INK over 0.2 s (game.flashGate, x FLASH INTENSITY: a gated flash keeps
//   only the punch) and the chip punches 1.3x -> 1 (REDUCED MOTION: none); FeelProbe event "shockChipReady".
// - Holstered: the glyph and the seconds at Hud20ShockHolsterAlpha (in hand: whole).
// Shown only while design.shockwave is live and the player owns the THUNDERCLAP (HudReads20.Shockwave); -skygrindOff20:
// none. The tab sits on the card's top edge (Hud20Layout.ChipTab: out of S14's box), inside the weapon block's canvas, so
// it rides the card's HUD STYLE ATTACHED sway. Written on change only; no allocation per frame.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform shockChip20, shockChipBody20;
        UiRing20 shockDisc20, shockTrack20, shockFill20;
        TextMeshProUGUI shockGlyph20, shockSeconds20;
        UiShape[] shockPips20;
        HudReads20.Shock shock20;
        int shockPrevCharges20 = -1, shockSecondsShown20 = -2, shockTint20 = -1, shockPipsShown20 = -1, shockPipMask20 = -1;
        int shockGlyphVersion20 = -1, shockGlyphMode20 = -1;
        double shockFlashAge20 = double.MaxValue, shockPunchAge20 = double.MaxValue, shockEnterAge20 = -1;
        CanvasGroup shockChipGroup20;
        float shockFlashShown20 = -1, shockPunchShown20 = -1, shockAlphaShown20 = -1;
        Vector2 shockChipPosShown20 = new Vector2(float.NaN, 0);

        void BuildCooldown20()
        {
            if (weaponBlock == null) return;
            float side = UnityTuning.Hud20ShockChipD + 2 * UnityTuning.Hud20ShockTabPad;
            float tabW = Math.Max(UnityTuning.Hud17StripCellW, side);
            float rOut = UnityTuning.Hud20ShockChipD * 0.5f, rIn = rOut - UnityTuning.Hud20ShockRingW;
            shockChip20 = Hud17Rect(weaponBlock, "Shockwave chip", new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(tabW, side));
            shockChipGroup20 = shockChip20.gameObject.AddComponent<CanvasGroup>();
            shockChipGroup20.blocksRaycasts = shockChipGroup20.interactable = false;
            // the tab: INK with the theme's cut, as the card's own plate (it reads as part of the card)
            UnityEngine.UI.Image tab = Img(shockChip20, "Tab", UiTheme.Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(tabW, side));
            tab.sprite = UiTheme.Cut;
            tab.type = UnityEngine.UI.Image.Type.Sliced;
            shockChipBody20 = Hud17Rect(shockChip20, "Body", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud20ShockChipD, UnityTuning.Hud20ShockChipD));
            shockDisc20 = Ring20(shockChipBody20, "Disc", UiTheme.InkSolid, 0, rIn, UnityTuning.Hud20ShockArcSteps);
            shockTrack20 = Ring20(shockChipBody20, "Track", UiTheme.InkSoft, rIn, rOut, UnityTuning.Hud20ShockArcSteps);
            shockFill20 = Ring20(shockChipBody20, "Fill", UiTheme.CyanDeep, rIn, rOut, UnityTuning.Hud20ShockArcSteps);
            float box = 2 * rIn;
            shockGlyph20 = Hud17Text(shockChipBody20, "Glyph", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, UnityTuning.Hud20ShockTextY), new Vector2(box, box * 0.6f), false);
            shockGlyph20.richText = true;
            shockSeconds20 = Hud17Text(shockChipBody20, "Seconds", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, UnityTuning.Hud20ShockTextY), new Vector2(box, box * 0.6f), false);
            shockSeconds20.gameObject.SetActive(false);
            shockPips20 = new UiShape[3];
            for (int i = 0; i < shockPips20.Length; i++)
            {
                shockPips20[i] = Hud17Shape(shockChipBody20, $"Charge {i}", UiTheme.InkSoft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud20ShockPipW, UnityTuning.Hud20ShockPipH));
                shockPips20[i].SetCuts(0, 0);
                shockPips20[i].gameObject.SetActive(false);
            }
            shockChip20.gameObject.SetActive(false);
            game.OnNewRun += () =>
            {
                shockPrevCharges20 = -1;
                shockFlashAge20 = shockPunchAge20 = double.MaxValue;
            };
        }

        /// <summary>A UiRing20 under `parent`, centred, its rect the outer diameter.</summary>
        static UiRing20 Ring20(Transform parent, string name, Color c, float rIn, float rOut, int steps)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiRing20));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(2 * rOut, 2 * rOut);
            var ring = go.GetComponent<UiRing20>();
            ring.color = c;
            ring.raycastTarget = false;
            ring.SetRadii(rIn, rOut);
            ring.Steps = steps;
            return ring;
        }

        void RenderCooldown20(double frameDt)
        {
            if (shockChip20 == null) return;
            HudReads20.Shockwave(game, ref shock20);
            bool on = shock20.valid && game.flow == Flow.Playing;
            if (shockChip20.gameObject.activeSelf != on)
            {
                shockChip20.gameObject.SetActive(on);
                if (on) shockEnterAge20 = 0; // M9: it enters (grows and fades in), never whole on its first frame
            }
            if (!on)
            {
                shockPrevCharges20 = shock20.valid ? shock20.charges : -1;
                return;
            }
            PlaceCooldown20();
            if (shockEnterAge20 >= 0)
            {
                shockEnterAge20 = HudEntrance18.Step(shockEnterAge20, frameDt);
                bool done = HudEntrance18.Done(shockEnterAge20);
                float es = done || game.settings.View.reducedMotion ? 1f : HudEntrance18.Scale(shockEnterAge20);
                shockChip20.localScale = new Vector3(es, es, 1);
                shockChipGroup20.alpha = done ? 1f : HudEntrance18.Alpha(shockEnterAge20);
                if (done) shockEnterAge20 = -1;
            }

            // a charge back: the flash (gated) and the punch
            if (ShockRing20.ChargeBack(shockPrevCharges20, shock20.charges))
            {
                shockPunchAge20 = 0;
                if (game.flashGate.TryFlash(SurvivalGame.RealNow)) shockFlashAge20 = 0;
                FeelProbe.Event(UnityTuning.Hud20ShockReadyEvent);
            }
            shockPrevCharges20 = shock20.charges;

            bool ready = ShockRing20.Ready(shock20.charges);
            shockFill20.Fill = ShockRing20.Fill(shock20.charges, shock20.max, shock20.left, shock20.cooldown);
            int tint = ready ? 1 : 0;
            if (tint != shockTint20)
            {
                shockTint20 = tint;
                shockFill20.color = ready ? UiTheme.Cyan : UiTheme.CyanDeep;
            }

            // inside: the seconds above 3 s with no charge ready, else the glyph
            int secs = ShockRing20.Seconds(shock20.charges, shock20.left);
            if (secs != shockSecondsShown20)
            {
                bool showSecs = secs >= 0, wasSecs = shockSecondsShown20 >= 0;
                shockSecondsShown20 = secs;
                if (showSecs) shockSeconds20.SetText("{0:0}", secs);
                if (showSecs != wasSecs || shockSeconds20.gameObject.activeSelf != showSecs)
                {
                    shockSeconds20.gameObject.SetActive(showSecs);
                    shockGlyph20.gameObject.SetActive(!showSecs);
                    shockInnerAge20 = 0; // NIGHT20 fix H1 (M9: the seconds popped in whole): the new inside slams in and fades up
                }
            }
            Cooldown20Glyph();

            // the charges (the FORGE's second): pips, CYAN when ready
            int pips = shock20.max > 1 ? Math.Min(shockPips20.Length, shock20.max) : 0;
            if (pips != shockPipsShown20)
            {
                if (pips > 0 && pips > shockPipsShown20) shockPipEnter20 = 0; // M9: the FORGE's second charge: the pips fade in
                shockPipsShown20 = pips;
                shockPipMask20 = -1;
                float w = UnityTuning.Hud20ShockPipW, gap = UnityTuning.Hud20ShockPipGap, x0 = -(pips * w + (pips - 1) * gap) * 0.5f + w * 0.5f;
                for (int i = 0; i < shockPips20.Length; i++)
                {
                    bool show = i < pips;
                    if (shockPips20[i].gameObject.activeSelf != show) shockPips20[i].gameObject.SetActive(show);
                    if (show) shockPips20[i].rectTransform.anchoredPosition = new Vector2(x0 + i * (w + gap), UnityTuning.Hud20ShockPipY);
                }
            }
            int mask = 0;
            for (int i = 0; i < pips; i++) if (i < shock20.charges) mask |= 1 << i;
            float pipFade = 1f;
            if (shockPipEnter20 >= 0)
            {
                shockPipEnter20 = HudEntrance18.Step(shockPipEnter20, frameDt);
                if (shockPipEnter20 >= UnityTuning.Hud18FadeInTime) shockPipEnter20 = -1;
                else pipFade = HudEntrance18.Fade(shockPipEnter20);
                shockPipMask20 = -1; // rewritten while it fades in
            }
            if (mask != shockPipMask20)
            {
                shockPipMask20 = mask;
                for (int i = 0; i < pips; i++) shockPips20[i].color = UiTheme.A((mask & (1 << i)) != 0 ? UiTheme.Cyan : UiTheme.InkSoft, pipFade);
            }

            // the flash and the punch
            SettingsView sv = game.settings.View;
            float flash = ShockRing20.Flash(shockFlashAge20, sv.flashIntensity);
            if (flash != shockFlashShown20)
            {
                shockFlashShown20 = flash;
                shockDisc20.color = Color.Lerp(UiTheme.InkSolid, UiTheme.CyanHot, flash);
            }
            float punch = ShockRing20.Punch(shockPunchAge20, sv.reducedMotion);
            if (punch != shockPunchShown20)
            {
                shockPunchShown20 = punch;
                shockChipBody20.localScale = new Vector3(punch, punch, 1);
            }
            if (shockFlashAge20 < UnityTuning.Hud20ShockFlash) shockFlashAge20 += frameDt;
            if (shockPunchAge20 < UnityTuning.Hud20ShockPunchTime) shockPunchAge20 += frameDt;

            // holstered: the inside dims; a new inside (the seconds, the glyph) enters: 1.3x -> 1 and a fade up (fix H1)
            float a = game.loadout.Weapon == UnityTuning.Hud20ShockGun ? 1f : UnityTuning.Hud20ShockHolsterAlpha;
            float innerScale = 1f;
            if (shockInnerAge20 >= 0)
            {
                shockInnerAge20 = HudEntrance18.Step(shockInnerAge20, frameDt);
                bool done = shockInnerAge20 >= UnityTuning.Hud18FadeInTime;
                if (!done)
                {
                    a *= HudEntrance18.Fade(shockInnerAge20);
                    if (!sv.reducedMotion) innerScale = 1f + (UnityTuning.Hud20ShockInnerFrom - 1f) * (1f - UiTween.OutCubic(shockInnerAge20 / UnityTuning.Hud18FadeInTime));
                }
                else shockInnerAge20 = -1;
            }
            if (a != shockAlphaShown20)
            {
                shockAlphaShown20 = a;
                shockGlyph20.alpha = a;
                shockSeconds20.alpha = a;
            }
            if (innerScale != shockInnerScaleShown20)
            {
                shockInnerScaleShown20 = innerScale;
                var sc = new Vector3(innerScale, innerScale, 1);
                shockGlyph20.rectTransform.localScale = sc;
                shockSeconds20.rectTransform.localScale = sc;
            }
        }

        double shockInnerAge20 = -1, shockPipEnter20 = -1;
        float shockInnerScaleShown20 = -1;

        /// <summary>The glyph's text: the alt-fire's live binding (PromptText: the pad's sprite, the mouse's RMB), on change only.</summary>
        void Cooldown20Glyph()
        {
            bool pad = PlayerDevices.Last == InputDevice.Pad;
            int mode = pad ? (PlayerDevices.Glyphs == GlyphSet.PlayStation ? 2 : PlayerDevices.Glyphs == GlyphSet.Nintendo ? 3 : 1) : 0;
            int version = PromptText.Version;
            if (mode == shockGlyphMode20 && version == shockGlyphVersion20) return;
            shockGlyphMode20 = mode;
            shockGlyphVersion20 = version;
            string today = mode == 0 ? UnityTuning.HudSpecialRmb : mode == 2 ? UnityTuning.HudPadAltPlayStation : mode == 3 ? UnityTuning.HudPadAltNintendo : UnityTuning.HudPadAltNeutral;
            shockGlyph20.text = PromptText.For("altFire", false, today);
        }

        /// <summary>The tab over the THUNDERCLAP's slot (the window's edge when it is scrolled out), on the card's top edge.</summary>
        void PlaceCooldown20()
        {
            int idx = -1;
            for (int i = 0; i < hotVisible; i++)
            {
                if (hotCells[i].id == UnityTuning.Hud20ShockGun) { idx = i; break; }
            }
            if (idx < 0)
            {
                int owned = game.loadout.owned.IndexOf(UnityTuning.Hud20ShockGun);
                idx = owned < hotFirst ? 0 : Math.Max(0, hotVisible - 1);
            }
            float cellRight = idx >= 0 && idx < hotCells.Length ? hotCells[idx].rt.anchoredPosition.x : 0;
            var pos = new Vector2(weaponRight + cellRight - UnityTuning.Hud17StripCellW * 0.5f, weaponBase + weaponPlate.rectTransform.sizeDelta.y);
            if (pos == shockChipPosShown20) return;
            shockChipPosShown20 = pos;
            shockChip20.anchoredPosition = pos;
        }
    }
}
