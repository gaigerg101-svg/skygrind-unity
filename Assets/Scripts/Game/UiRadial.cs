// NIGHT13 U3-26.9 (agent H): the hold-Tab radial picker, drawn (hud.js .hud-radial :88-95, markup :307, render :646-667;
// main.js hudInfo.radial :2385). The input half (open on the radial key, the pointer clamped to weapons.radial.maxOffset,
// ArsenalMath.RadialSector, the pick on release) is U3-26.7's: CombatReader.radial is read here, never written.
// - Open while game.combatReader.radial.open (and only while playing: the web closes it when the pointer is released).
// - At most HudRadialMax (24) options for the guns you own: option i at the angle i / n of a turn, 0 straight up, clockwise,
//   at weapons.radial.radius px from the centre; each "n · NAME" for the first ten (the hotbar's digits), the name past
//   them, on the plate with a 2 px bottom border in the gun's family colour. The highlighted one (radial.highlighted,
//   -1 inside the dead zone) in cyan with dark text, x1.15.
// - The ring behind them (.ring: a dark band between 30 % and 70 % of the farthest-corner radius).
// - The middle reads the highlighted gun's name, else "POINT · RELEASE " + the radial key (input.bindings.radial; the web
//   writes TAB).
// - Port-only (the web draws none; NIGHT13.md asks for it): a dot at the pointer's offset. radial.x / y are the web's
//   pointer px (x right, y DOWN: what RadialSector reads); the canvas's y is up, so the dot sits at (x, -y).
// PORT-RULES section 6: 24 options made once; the labels and sizes are set when the owned list changes, the highlight when
// it changes, the dot when the pointer moves; nothing per frame allocates. The radial has its own nested canvas under the
// sight layer (the dot moves every mouse frame while it is open).
// NIGHT15 U3-28.8 (builder K): the pad's wheel draws the same radial (radial.pad: the dot is the right stick's tilt);
// its middle reads UnityTuning.PadWheelPrompt instead of the mouse's "POINT · RELEASE TAB".
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class RadialOption
        {
            public RectTransform rt;
            public UnityEngine.UI.Image back, border;
            public TextMeshProUGUI label;
            public string id;
            public bool hi;
        }

        RectTransform radialRoot;
        RadialOption[] radialOpts;
        TextMeshProUGUI radialMid;
        UnityEngine.UI.Image radialDot;
        string radialPrompt; // "POINT · RELEASE TAB"
        string radialPadPrompt19; // NIGHT19: the pad's middle in glyphs (built on a PromptText change)
        int radialPadPromptVersion19 = -1;

        /// <summary>
        /// The pad wheel's middle: NIGHT18's "RIGHT STICK · RELEASE"; NIGHT19 U3-33.4 (design.pad.n19), the right stick's glyph
        /// POINT · RELEASE and the button that holds the wheel, drawn (PromptText; rebuilt only when the device or layout changes).
        /// </summary>
        string RadialPadPrompt19()
        {
            if (!PlayerDevices.N19) return UnityTuning.PadWheelPrompt;
            int v = PromptText.Version;
            if (radialPadPrompt19 == null || v != radialPadPromptVersion19)
            {
                radialPadPromptVersion19 = v;
                radialPadPrompt19 = PromptText.For("look", false, "RIGHT STICK") + UnityTuning.PadWheelPoint19 + PromptText.For("weaponWheel", true, "");
            }
            return radialPadPrompt19;
        }
        float radialRadius;
        bool radialShown;
        bool radialPadOpener; // NIGHT15: the pad's wheel opened the radial shown (its middle reads the pad's prompt)
        int radialCount = -1, radialHi = -2;
        double radialDotX = double.NaN, radialDotY = double.NaN;
        Color radialPlate, radialHiText;

        /// <summary>Automation (U3-26.9's smoke step): the radial is drawn, and the option it highlights (-1: none).</summary>
        public bool RadialShown => radialShown;
        public int RadialHighlight => radialShown ? radialHi : -1;

        partial void BuildRadial()
        {
            radialRadius = (float)cfg.Num("weapons.radial.radius");
            radialPrompt = "POINT · RELEASE " + (KeyLabel(cfg, "radial") ?? "TAB");
            radialPlate = panel;
            radialPlate.a = (float)cfg.Num("hud.panelOpacity"); // hud.js --plate: rgba(12,11,22, hud.panelOpacity)
            radialHiText = Look.Hex(UnityTuning.HudRadialHiText);

            radialRoot = SubCanvas(sight, "Radial");
            // .ring: a (radius x 2 + pad x 2) px disc; the band between the gradient's stops of its farthest-corner radius.
            float half = radialRadius + UnityTuning.HudRadialRingPad;
            float far = half * Mathf.Sqrt(2);
            float inner = UnityTuning.HudRadialRingInnerStop * far, outer = Mathf.Min(half, UnityTuning.HudRadialRingOuterStop * far);
            // NIGHT16 U3-29.7: a dim disc behind the ring and the options (the world's labels, the zone signs and prices, read
            // through the open band): the panel colour at WheelBackdropAlpha, out to the ring's edge plus a margin.
            float backR = half + UnityTuning.WheelBackdropPad;
            Sprite disc = RingSprite(backR * 0.5f, backR);
            Color backColor = panel;
            backColor.a = UnityTuning.WheelBackdropAlpha;
            var backdrop = Img(radialRoot, "Backdrop", backColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, disc.rect.size);
            backdrop.sprite = disc;
            backdrop.raycastTarget = false;
            Sprite band = RingSprite((inner + outer) * 0.5f, outer - inner);
            Color ringColor = Look.Hex(UnityTuning.HudRadialRingColor);
            ringColor.a = UnityTuning.HudRadialRingAlpha;
            var ring = Img(radialRoot, "Ring", ringColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, band.rect.size);
            ring.sprite = band;

            radialOpts = new RadialOption[UnityTuning.HudRadialMax];
            float h = UnityTuning.HudRadialOptLine + 2 * UnityTuning.HudRadialOptPadY;
            for (int i = 0; i < radialOpts.Length; i++)
            {
                var o = new RadialOption();
                // The option's box: the plate (padding 3 8 round the text) over its 2 px family border, centred on its point.
                o.rt = new GameObject($"Radial option {i}", typeof(RectTransform)).GetComponent<RectTransform>();
                o.rt.SetParent(radialRoot, false);
                o.rt.anchorMin = o.rt.anchorMax = o.rt.pivot = new Vector2(0.5f, 0.5f);
                o.rt.sizeDelta = new Vector2(0, h + UnityTuning.HudRadialOptBorder);
                o.back = Img(o.rt, "Plate", radialPlate, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, h));
                o.back.rectTransform.pivot = new Vector2(0.5f, 1);
                o.border = Img(o.rt, "Border", usable, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, UnityTuning.HudRadialOptBorder));
                o.border.rectTransform.pivot = new Vector2(0.5f, 0);
                o.label = Text(o.back.transform, "Label", "", tBody, text, TextAlignmentOptions.Center, Vector2.zero, new Vector2(0, UnityTuning.HudRadialOptLine), new Vector2(0.5f, 0.5f));
                o.label.characterSpacing = UnityTuning.HudRadialOptSpacing;
                o.rt.gameObject.SetActive(false);
                radialOpts[i] = o;
            }
            radialMid = Text(radialRoot, "Radial middle", radialPrompt, tH3, text, TextAlignmentOptions.Center, Vector2.zero, new Vector2(UnityTuning.HudRadialMidWidth, UnityTuning.HudRadialMidLine), new Vector2(0.5f, 0.5f));
            radialMid.characterSpacing = UnityTuning.HudRadialMidSpacing;
            Sprite dot = RingSprite(UnityTuning.HudRadialDotSize * 0.25f, UnityTuning.HudRadialDotSize * 0.5f); // a disc
            radialDot = Img(radialRoot, "Radial pointer", text, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, dot.rect.size);
            radialDot.sprite = dot;
            radialRoot.gameObject.SetActive(false);
        }

        partial void RenderRadial(double frameDt)
        {
            if (radialRoot == null) return;
            RadialState rs = game.combatReader.radial;
            bool open = rs.open && game.flow == Flow.Playing && sight.gameObject.activeSelf;
            if (open != radialShown)
            {
                radialShown = open;
                radialRoot.gameObject.SetActive(open);
            }
            if (!open) return;
            if (rs.pad != radialPadOpener)
            {
                radialPadOpener = rs.pad;
                radialHi = -2; // the middle's prompt follows the opener (refreshed with the highlight below)
            }

            // The options (hud.js 'radialList': owned.join()), compared element by element.
            List<string> owned = game.loadout.owned;
            int n = Math.Min(owned.Count, radialOpts.Length);
            bool changed = n != radialCount;
            for (int i = 0; i < n && !changed; i++) changed = owned[i] != radialOpts[i].id;
            if (changed) LayoutRadial(owned, n);

            // The highlight and the middle (hud.js 'radialHi').
            int hi = rs.highlighted;
            if (hi != radialHi)
            {
                radialHi = hi;
                for (int i = 0; i < n; i++)
                {
                    RadialOption o = radialOpts[i];
                    bool on = i == hi;
                    if (on == o.hi) continue;
                    o.hi = on;
                    o.back.color = on ? usable : radialPlate;
                    o.label.color = on ? radialHiText : text;
                    float s = on ? UnityTuning.HudRadialHiScale : 1;
                    o.rt.localScale = new Vector3(s, s, 1);
                }
                radialMid.text = hi >= 0 && hi < owned.Count ? WeaponName(owned[hi]) : radialPadOpener ? RadialPadPrompt19() : radialPrompt;
            }

            // The pointer (web px, y down).
            if (rs.x != radialDotX || rs.y != radialDotY)
            {
                radialDotX = rs.x;
                radialDotY = rs.y;
                radialDot.rectTransform.anchoredPosition = new Vector2((float)rs.x, -(float)rs.y);
            }
        }

        /// <summary>The owned list changed (or the radial first opened): each option's gun, label, size, border and place.</summary>
        void LayoutRadial(List<string> owned, int n)
        {
            radialCount = n;
            float h = UnityTuning.HudRadialOptLine + 2 * UnityTuning.HudRadialOptPadY;
            for (int i = 0; i < radialOpts.Length; i++)
            {
                RadialOption o = radialOpts[i];
                bool show = i < n;
                if (o.rt.gameObject.activeSelf != show) o.rt.gameObject.SetActive(show);
                if (!show)
                {
                    o.id = null;
                    continue;
                }
                string id = owned[i];
                o.id = id;
                o.label.text = i < 10 ? Digit[i] + " · " + WeaponName(id) : WeaponName(id); // on change only
                float w = o.label.GetPreferredValues().x + 2 * UnityTuning.HudRadialOptPadX;
                o.rt.sizeDelta = new Vector2(w, h + UnityTuning.HudRadialOptBorder);
                o.label.rectTransform.sizeDelta = new Vector2(w, UnityTuning.HudRadialOptLine);
                o.border.color = WeaponLook.FamilyColor(cfg, id, usable); // border-bottom: var(--fam)
                double a = (double)i / n * Math.PI * 2; // 0 = up, clockwise
                o.rt.anchoredPosition = new Vector2((float)Math.Round(Math.Sin(a) * radialRadius, MidpointRounding.AwayFromZero), (float)Math.Round(Math.Cos(a) * radialRadius, MidpointRounding.AwayFromZero));
                // Back to the plain look; the highlight is applied again below.
                o.hi = false;
                o.back.color = radialPlate;
                o.label.color = text;
                o.rt.localScale = Vector3.one;
            }
            radialHi = -2;
        }
    }
}
