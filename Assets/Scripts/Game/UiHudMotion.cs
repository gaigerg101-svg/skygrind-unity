// NIGHT12 U3-25.6 (design.ceremony.hudMotion; Research/2026-10-03-polish.md F4 and B6): the HUD's motion, display only.
// - The HP bar's damage ghost: a white segment behind the fill that holds UnityTuning.HpGhostHold s after a drop, then
//   drains to the fill over HpGhostDrain s (a new hit holds it again; a heal pulls it up at once).
// - The points roll: the counter counts toward the bank over PointsRollTime s (eased out); a gain of PointsPunchAt or
//   more punches it to PointsPunchScale. A new run snaps it.
// - The web's plates (hud.js .hud-plate, .hud-points, .hud-top, .hud-rankbox): rgba(ui.theme.panel, hud.panelOpacity),
//   skewed by hud.panelSkewDeg (UiSkew shifts each vertex along x by its height), the points' gold right edge and the
//   rank box's cyan one; the top line's plate follows its text's width. The bars inside keep straight (a 12 px bar
//   skews by under 2 px on the web).
// Everything here is set only when it changes; the ghost and the roll write a fill and a number while they move.
using System;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UnityEngine.UI.Image hpGhost;
        float ghostFrac = 1, shownGhostFrac = -1, lastHpFrac = 1;
        double ghostHold, ghostSpeed;
        bool ghostDraining;

        UnityEngine.UI.Image topPlate;
        bool topLineDirty;
        float shownTopWidth = -1;

        void BuildHudMotion()
        {
            // The ghost: under the fill, in its bar (slanted with it: NIGHT17 U3-30.7).
            hpGhost = Img(hpFill.transform.parent, "Damage ghost", UiTheme.A(UiTheme.White, UnityTuning.HpGhostAlpha), Vector2.zero, new Vector2(1, 1), Vector2.zero, Vector2.zero);
            hpGhost.rectTransform.pivot = new Vector2(0, 0.5f);
            hpGhost.transform.SetSiblingIndex(hpFill.transform.GetSiblingIndex());
            UiSkew.Apply(hpGhost, Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad));
            hpGhost.enabled = false;

            // NIGHT17 U3-30.7: the vitals plate is INK with the theme's cut (Ui.cs), the points have no plate (a bare S75 gold
            // numeral with a hard shadow over its gold accent bar: BuildPoints17), and the rank box is gone (UiStyleMeter.cs):
            // no translucent grey plates (the research's cheap-UI list).

            // The top line's plate (hud.js .hud-top), sized to the line when it changes: INK at the theme's alpha, slanted.
            RectTransform rt = roundText.rectTransform;
            topPlate = Img(frame, "Top plate", UiTheme.Ink, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, rt.anchoredPosition.y + UnityTuning.TopPlatePadY), new Vector2(0, rt.sizeDelta.y + 2 * UnityTuning.TopPlatePadY));
            topPlate.rectTransform.pivot = new Vector2(0.5f, 1);
            UiSkew.Apply(topPlate, Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad));
            topPlate.enabled = false;

            game.OnNewRun += SnapHudMotion;
        }

        void SnapHudMotion()
        {
            ghostFrac = 1;
            ghostDraining = false;
            ghostHold = 0;
        }

        /// <summary>Frame time: the ghost behind the HP fill (`frac` = the fill now), the points roll, the plates' widths.</summary>
        void RenderHudMotion(double frameDt, float frac)
        {
            // The damage ghost
            if (frac >= ghostFrac)
            {
                ghostFrac = frac;
                ghostDraining = false;
                ghostHold = 0;
            }
            else
            {
                if (frac < lastHpFrac)
                {
                    ghostHold = UnityTuning.HpGhostHold; // a new hit: the ghost waits again where it is
                    ghostDraining = false;
                }
                if (ghostHold > 0) ghostHold -= frameDt;
                else
                {
                    if (!ghostDraining)
                    {
                        ghostDraining = true;
                        ghostSpeed = (ghostFrac - frac) / UnityTuning.HpGhostDrain;
                    }
                    ghostFrac = Math.Max(frac, ghostFrac - (float)(ghostSpeed * frameDt));
                }
            }
            lastHpFrac = frac;
            if (ghostFrac != shownGhostFrac)
            {
                shownGhostFrac = ghostFrac;
                bool on = ghostFrac > frac;
                if (hpGhost.enabled != on) hpGhost.enabled = on;
                hpGhost.rectTransform.anchorMax = new Vector2(ghostFrac, 1);
            }

            // NIGHT17 U3-30.7: the points roll moved to RenderPoints17 (UiCount, UiPunch, the white frame).

            // The top line's plate follows its text (set when the line changed, a frame later)
            if (topLineDirty)
            {
                topLineDirty = false;
                float w = roundText.preferredWidth; // 0 for an empty line (no string is built: TMP measures its buffer)
                if (w != shownTopWidth)
                {
                    shownTopWidth = w;
                    bool on = w > 1;
                    if (topPlate.enabled != on) topPlate.enabled = on;
                    topPlate.rectTransform.sizeDelta = new Vector2(w + UnityTuning.TopPlatePadLeft + UnityTuning.TopPlatePadRight, topPlate.rectTransform.sizeDelta.y);
                }
            }
        }

        // --- NIGHT17 U3-30.7: the points odometer (Research/2026-10-05-overhaul.md section 2's energetic score) --------------
        // The bank as the glance tier's ONE hero number (S75 display, GOLD, tabular digits, a hard INK shadow, no label, no plate)
        // over a 4 px gold accent bar as wide as the number. A gain rolls it up (UiCount: 250-600 ms by the log of the change,
        // ease-out cubic), punches it 1 -> 1.15 -> 1 over 150 ms (UiPunch, at most once per 100 ms) and turns it white for one
        // frame (Hud17PointsWhite; FLASH INTENSITY 0: none); a spend rolls it down quietly. A new run snaps it.
        UiCount pointsCount17;
        UiPunch pointsPunch17;
        double pointsWhite17;
        int pointsShown17 = int.MinValue, pointsDigits17 = -1;
        bool pointsSnap17 = true, pointsWhiteShown17;
        UiShape pointsBar17;
        float hudScaleShown17 = -1;
        Vector2 hudScaleRoot17;
        int hudScaleVersion17 = -1;

        UnityEngine.UI.Image pointsPlate17; // round 3's reviewer: the score's opaque INK plate (cut), sized to the number

        void BuildPoints17()
        {
            // Round 3's reviewer (gold-on-white vanished in the punch's freeze and under orange chunks): an opaque INK plate
            // with the theme's cut behind the number and its gold bar, as wide as the number (set when its digits change).
            float pad = UnityTuning.Hud17PointsPlatePad;
            pointsPlate17 = Img(values, "Points plate", UiTheme.InkSolid, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-margin + pad, margin - pad), new Vector2(0, UnityTuning.Hud17PointsBox + UnityTuning.Hud17PointsBarHeight + UnityTuning.Hud17PointsBarGap + 2 * pad));
            pointsPlate17.rectTransform.pivot = new Vector2(1, 0);
            pointsPlate17.sprite = UiTheme.Cut;
            pointsPlate17.type = UnityEngine.UI.Image.Type.Sliced;
            pointsPlate17.transform.SetSiblingIndex(points.transform.GetSiblingIndex()); // under the number
            pointsBar17 = Hud17Shape(values, "Points bar", UiTheme.Gold, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-margin, margin), new Vector2(0, UnityTuning.Hud17PointsBarHeight));
            pointsBar17.SetCuts(0, 0);
            points.rectTransform.pivot = new Vector2(1, 0.15f); // the punch grows from the number's baseline, right end
            points.rectTransform.anchoredPosition += new Vector2(0, UnityTuning.Hud17PointsBox * 0.15f);
            game.OnNewRun += () => pointsSnap17 = true;
        }

        void RenderPoints17(double frameDt)
        {
            // Round 2's reviewer: the bank ticks on the gain's frame (it read 500 through every kill strip while the points
            // waited for the pop to land): it rolls at once with the 1.15 punch and a white frame; a pop's flight into the
            // counter is decoration, its landing a second, smaller punch (Hud17PopArrivePunch).
            // Round 3's reviewer: the round end is a ceremony (the exception to "ticks on the frame"): the round's bonus is
            // held back until its pop lands in the score, which rolls then with a big punch.
            double target = Math.Max(0, Math.Round(game.economy.total - PointsHeldBack()));
            bool arrived = popArrived, arrivedRound = popArrivedRound;
            popArrived = popArrivedRound = false;
            if (pointsSnap17)
            {
                pointsSnap17 = false;
                pointsCount17.Snap(target);
            }
            else if (target != pointsCount17.Target)
            {
                if (target > pointsCount17.Target)
                {
                    if (arrivedRound) pointsPunch17.Kick(UnityTuning.Hud17PopRoundPunch, UnityTuning.Hud17PopArriveTime, force: true); // the round's bonus lands
                    else pointsPunch17.Kick(UnityTuning.Hud17PopArrivePunch, UnityTuning.Hud17PopArriveTime, force: true); // round 5: 1.0 -> 1.2 -> 1.0 over 120 ms on the gain's frame
                    if (game.settings.Num("flashIntensity") > 0) pointsWhite17 = UnityTuning.Hud17PointsWhite;
                }
                pointsCount17.Set(target);
            }
            else if (arrived) pointsPunch17.Kick(UnityTuning.Hud17PopArrivePunch, UnityTuning.Hud17PopArriveTime, force: true); // round 5: the pop lands: 1.2x again
            int shown = (int)Math.Round(pointsCount17.Step(frameDt));
            if (shown != pointsShown17)
            {
                pointsShown17 = shown;
                points.SetText(UnityTuning.Hud17Tabular + "{0:0}", shown);
                int digits = shown <= 0 ? 1 : (int)Math.Floor(Math.Log10(shown)) + 1;
                if (digits != pointsDigits17)
                {
                    pointsDigits17 = digits; // the bar follows the number's width (measured when its digit count changes)
                    pointsBar17.rectTransform.sizeDelta = new Vector2(points.preferredWidth, UnityTuning.Hud17PointsBarHeight);
                    pointsPlate17.rectTransform.sizeDelta = new Vector2(points.preferredWidth + 2 * UnityTuning.Hud17PointsPlatePad, pointsPlate17.rectTransform.sizeDelta.y);
                }
            }
            float p = pointsPunch17.Step(frameDt);
            if (p != points.rectTransform.localScale.x) points.rectTransform.localScale = new Vector3(p, p, 1); // written only while it moves
            pointsWhite17 = Math.Max(0, pointsWhite17 - frameDt);
            bool white = pointsWhite17 > 0;
            if (white != pointsWhiteShown17)
            {
                pointsWhiteShown17 = white;
                points.color = white ? UiTheme.White : UiTheme.Gold;
            }
        }

        /// <summary>
        /// NIGHT17 U3-30.7: HUD SCALE (settings "hudScale", 0.8-1.25; 1 until builder N adds the row): the frame and values
        /// layers scale about the screen's centre and grow by HudScaleMath.Grow on each side, so their corners stay at the
        /// screen's and every corner block scales toward its own corner. The crosshair's layer (sight) keeps its size: its
        /// markers and brackets are placed in screen space.
        /// </summary>
        void RenderHudScale17()
        {
            Vector2 size = root.rect.size;
            if (game.settings.version == hudScaleVersion17 && size == hudScaleRoot17) return;
            hudScaleVersion17 = game.settings.version;
            hudScaleRoot17 = size;
            bool has = game.settings.store.values.ContainsKey("hudScale");
            float s = HudScaleMath.From(has, has ? game.settings.Num("hudScale") : 1);
            hudScaleShown17 = s;
            float gx = HudScaleMath.Grow(size.x, s), gy = HudScaleMath.Grow(size.y, s);
            HudScaleLayer17(frame, s, gx, gy);
            HudScaleLayer17(values, s, gx, gy);
        }

        static void HudScaleLayer17(RectTransform layer, float s, float gx, float gy)
        {
            layer.localScale = new Vector3(s, s, 1);
            layer.offsetMin = new Vector2(-gx, -gy);
            layer.offsetMax = new Vector2(gx, gy);
        }

        /// <summary>Automation: the HUD SCALE applied now.</summary>
        public float HudScale17 => hudScaleShown17;
    }

    /// <summary>
    /// NIGHT12 U3-25.6: CSS skewX for a uGUI graphic (hud.js .hud-plate, menus.js .grade): each vertex moves along x by
    /// `tan` times its height over the rect's centre. Applied when the graphic rebuilds its mesh, never per frame.
    /// </summary>
    public sealed class UiSkew : UnityEngine.UI.BaseMeshEffect
    {
        public float tan;

        /// <summary>Skew `g` (adds the effect once) and rebuild its mesh.</summary>
        public static void Apply(UnityEngine.UI.Graphic g, float tan)
        {
            if (g == null) return;
            UiSkew s = g.GetComponent<UiSkew>();
            if (s == null) s = g.gameObject.AddComponent<UiSkew>();
            s.tan = tan;
            g.SetVerticesDirty();
        }

        public override void ModifyMesh(UnityEngine.UI.VertexHelper vh)
        {
            if (!IsActive() || tan == 0) return;
            float cy = graphic.rectTransform.rect.center.y;
            UIVertex v = default;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                v.position.x += (v.position.y - cy) * tan;
                vh.SetUIVertex(v, i);
            }
        }
    }

    /// <summary>NIGHT17 U3-30.7: HUD SCALE (NIGHT17.md rule 8; Hud17Tests).</summary>
    public static class HudScaleMath
    {
        /// <summary>The settings' hudScale clamped to the theme's range; 1 without the row (builder N adds it) or for NaN.</summary>
        public static float From(bool has, double value)
        {
            if (!has || double.IsNaN(value)) return 1f;
            return (float)Math.Max(UnityTuning.ThemeHudScaleMin, Math.Min(UnityTuning.ThemeHudScaleMax, value));
        }

        /// <summary>px a full-screen layer grows on each side so that, x `scale`, it covers the screen again (its corners stay at the screen's).</summary>
        public static float Grow(float size, float scale) => scale > 0 ? size * (1f / scale - 1f) * 0.5f : 0f;
    }
}
