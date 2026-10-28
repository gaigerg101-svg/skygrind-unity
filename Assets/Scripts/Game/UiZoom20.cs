// NIGHT20 U3-34.10 (builder H): the zoom's reticle and the LONGWAVE's scope with design.zoom live (GG 2026-10-06 evening:
// "most weapons should ... zoom: simple zoom"; NIGHT20.md T2: "the zoom's reticle drawn"). UiAds.cs (NIGHT16, the web's
// .hud-adsring and .hud-scope: a 1 px ring texture, a gradient square and four panels) stays NIGHT19's with the block off;
// with it on, its ring and scope roots are switched off and these draw instead (its rangefinder and the crosshair's arm fade
// stay: UiAds' adsTickAlpha).
// - A ring gun (1.25 STITCHER, 1.5 KESTREL; the ports' 1.3 / 1.4 when they land): a segmented ring in the crosshair's colour
//   (settings.crosshairColor) on a 1 px INK edge, radius Hud20ReticleR0 / magnification (the tighter zoom, the tighter
//   ring), four gaps where the crosshair's arms are; the KESTREL's precise zoom adds a centre dot. It closes in from
//   Hud20ReticleFrom x as the zoom eases (Viewmodel.AdsAmount; REDUCED MOTION: alpha only).
// - The scope (3.5, the LONGWAVE): a lens, not a flat image: a mask (UiRing20 from the lens to past the corners, INK-SOLID),
//   a CYAN-DEEP rim, the tube's shaded inner edge, a duplex reticle (INK posts outside, fine lines in the crosshair's colour
//   inside), stadia ticks on the horizontal and the lower vertical, a centre dot. It settles from Hud20ScopeFrom x.
// Named "Crosshair ..." (S14's crosshair class: the reticle is the crosshair while zoomed). Meshes are made once; per frame
// only the alpha, the scale and (on a gun change) the ring's radius are written, on change.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform zoomRing20, zoomScope20;
        CanvasGroup zoomRingGroup20, zoomScopeGroup20;
        UiRing20 zoomRingLine20, zoomRingEdge20, zoomDot20, zoomDotEdge20, zoomScopeMask20, zoomScopeDot20;
        UnityEngine.UI.Image[] zoomScopeFine20;
        double zoomMagShown20 = -1;
        float zoomRingAlpha20 = -1, zoomRingScale20 = -1, zoomScopeAlpha20 = -1, zoomScopeScale20 = -1;
        Color zoomColor20;
        int zoomWebHud20 = -1; // UiAds' roots: 1 on (NIGHT19's), 0 off (ours)
        Vector2 zoomScopeSize20;

        void BuildZoom20()
        {
            // the ring gun's reticle
            zoomRing20 = Hud17Rect(sight, "Crosshair zoom 20", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(128, 128));
            zoomRingGroup20 = zoomRing20.gameObject.AddComponent<CanvasGroup>();
            zoomRingGroup20.alpha = 0;
            zoomRingGroup20.blocksRaycasts = zoomRingGroup20.interactable = false;
            zoomRingEdge20 = Ring20(zoomRing20, "Edge", UiTheme.A(UiTheme.InkSolid, UnityTuning.Hud20ReticleEdgeAlpha), 10, 12, UnityTuning.Hud20ShockArcSteps * 2);
            zoomRingLine20 = Ring20(zoomRing20, "Ring", crossColor, 10, 12, UnityTuning.Hud20ShockArcSteps * 2);
            zoomRingEdge20.SetGaps(4, UnityTuning.Hud20ReticleGapDeg - 2 * UnityTuning.Hud20ReticleEdge, 0);
            zoomRingLine20.SetGaps(4, UnityTuning.Hud20ReticleGapDeg, 0);
            float d = UnityTuning.Hud20ReticleDot * 0.5f;
            zoomDotEdge20 = Ring20(zoomRing20, "Dot edge", UiTheme.A(UiTheme.InkSolid, UnityTuning.Hud20ReticleEdgeAlpha), 0, d + UnityTuning.Hud20ReticleEdge, 16);
            zoomDot20 = Ring20(zoomRing20, "Dot", crossColor, 0, d, 16);
            zoomColor20 = crossColor;
            zoomRing20.gameObject.SetActive(false);

            // the scope: a full-screen root (its scale about the centre)
            zoomScope20 = Hud17Rect(sight, "Crosshair scope 20", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            zoomScope20.SetAsFirstSibling(); // under the crosshair's arms and the markers
            zoomScopeGroup20 = zoomScope20.gameObject.AddComponent<CanvasGroup>();
            zoomScopeGroup20.alpha = 0;
            zoomScopeGroup20.blocksRaycasts = zoomScopeGroup20.interactable = false;
            zoomScopeSize20 = new Vector2(-1, -1);
            zoomScope20.gameObject.SetActive(false);
        }

        /// <summary>The scope's parts for the canvas' size (built on the first use and on a resize).</summary>
        void LayoutScope20()
        {
            Vector2 size = sight.rect.size;
            if (size == zoomScopeSize20 || size.y <= 0) return;
            zoomScopeSize20 = size;
            for (int i = zoomScope20.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(zoomScope20.GetChild(i).gameObject); // a resize: rare
            float r = ZoomReticle20.ScopeRadius(size.y), outer = ZoomReticle20.MaskOuter(size.x, size.y);
            int steps = UnityTuning.Hud20ScopeArcSteps;
            zoomScopeMask20 = Ring20(zoomScope20, "Mask", UiTheme.A(UiTheme.InkSolid, UnityTuning.Hud20ScopeMaskAlpha), r, outer, steps);
            UiRing20 shade = Ring20(zoomScope20, "Shade", UiTheme.InkSolid, r - UnityTuning.Hud20ScopeRimW - UnityTuning.Hud20ScopeShadeW, r - UnityTuning.Hud20ScopeRimW, steps);
            shade.SetShade(0, UnityTuning.Hud20ScopeShadeAlpha);
            Ring20(zoomScope20, "Rim", UiTheme.CyanDeep, r - UnityTuning.Hud20ScopeRimW, r, steps);
            // the duplex: INK posts from Hud20ScopePostShare x r to the rim, fine lines from the gap to the posts
            float post = r * UnityTuning.Hud20ScopePostShare, gap = UnityTuning.Hud20ScopeGap;
            float fineLen = post - gap, postLen = r - post;
            Color ink = UiTheme.InkSolid;
            for (int k = 0; k < 4; k++)
            {
                var dir = k == 0 ? Vector2.left : k == 1 ? Vector2.right : k == 2 ? Vector2.up : Vector2.down;
                bool h = k < 2;
                Img(zoomScope20, "Post " + k, ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), dir * (post + postLen * 0.5f), h ? new Vector2(postLen, UnityTuning.Hud20ScopePostW) : new Vector2(UnityTuning.Hud20ScopePostW, postLen));
            }
            zoomScopeFine20 = new UnityEngine.UI.Image[4 + 3 * UnityTuning.Hud20ScopeTicks];
            int n = 0;
            for (int k = 0; k < 4; k++)
            {
                var dir = k == 0 ? Vector2.left : k == 1 ? Vector2.right : k == 2 ? Vector2.up : Vector2.down;
                bool h = k < 2;
                zoomScopeFine20[n++] = Img(zoomScope20, "Fine " + k, crossColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), dir * (gap + fineLen * 0.5f), h ? new Vector2(fineLen, UnityTuning.Hud20ScopeFineW) : new Vector2(UnityTuning.Hud20ScopeFineW, fineLen));
            }
            // the stadia: across the horizontal line both ways, across the lower vertical (holdover)
            for (int t = 1; t <= UnityTuning.Hud20ScopeTicks; t++)
            {
                float at = gap + t * UnityTuning.Hud20ScopeTickStep;
                if (at >= post) break;
                var vTick = new Vector2(UnityTuning.Hud20ScopeFineW, UnityTuning.Hud20ScopeTickLen);
                zoomScopeFine20[n++] = Img(zoomScope20, "Tick l" + t, crossColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-at, 0), vTick);
                zoomScopeFine20[n++] = Img(zoomScope20, "Tick r" + t, crossColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(at, 0), vTick);
                zoomScopeFine20[n++] = Img(zoomScope20, "Tick d" + t, crossColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -at), new Vector2(UnityTuning.Hud20ScopeTickLen, UnityTuning.Hud20ScopeFineW));
            }
            zoomScopeDot20 = Ring20(zoomScope20, "Dot", crossColor, 0, UnityTuning.Hud20ReticleDot * 0.5f, 16);
            zoomColor20 = crossColor;
        }

        void RenderZoom20(double frameDt)
        {
            if (zoomRing20 == null) return;
            bool live = game.sim != null && game.sim.n20.zoom;
            // UiAds' ring and scope give way while the block is live (NIGHT19's with it off)
            int web = live ? 0 : 1;
            if (web != zoomWebHud20)
            {
                zoomWebHud20 = web;
                if (adsRingRoot != null) adsRingRoot.gameObject.SetActive(web == 1);
                if (adsScopeRoot != null) adsScopeRoot.gameObject.SetActive(web == 1);
            }
            string weapon = game.loadout != null ? game.loadout.Weapon : null;
            double mag = live ? HudReads20.ZoomOf(game, weapon) : 1;
            int kind = ZoomReticle20.Kind(mag);
            double amount = game.viewmodel != null ? game.viewmodel.AdsAmount : 0;
            bool shown = live && kind != ZoomReticle20.None && amount > 0 && game.UiFlow == Flow.Playing && game.Active;
            bool still = game.settings.View.reducedMotion;
            if (crossColor != zoomColor20) Zoom20Colour();

            bool ringOn = shown && kind == ZoomReticle20.Ring;
            // fix H2 (build 2's reel, M9: the ring appeared whole at its rest, and left in one frame): it enters with its own
            // fade (x the zoom's amount) and, cut off while showing (a swap, a menu), leaves with an exit fade
            if (ringOn && (!zoomRing20.gameObject.activeSelf || zoomRingExit20 >= 0))
            {
                zoomRing20.gameObject.SetActive(true);
                zoomRingEnter20 = 0;
                zoomRingExit20 = -1;
            }
            else if (!ringOn && zoomRing20.gameObject.activeSelf)
            {
                if (zoomRingExit20 < 0 && zoomRingAlpha20 > 0.02f)
                {
                    zoomRingExit20 = 0;
                    zoomRingExitFrom20 = zoomRingAlpha20;
                }
                if (zoomRingExit20 >= 0 && zoomRingExit20 < UnityTuning.Hud18ExitTime)
                {
                    zoomRingGroup20.alpha = zoomRingAlpha20 = zoomRingExitFrom20 * HudEntrance18.ExitAlpha(zoomRingExit20);
                    zoomRingExit20 += frameDt;
                }
                else
                {
                    zoomRing20.gameObject.SetActive(false);
                    zoomRingExit20 = -1;
                    zoomRingAlpha20 = -1;
                }
            }
            if (ringOn)
            {
                if (mag != zoomMagShown20)
                {
                    zoomMagShown20 = mag;
                    float r = ZoomReticle20.Radius(mag), half = UnityTuning.Hud20ReticleW * 0.5f, e = UnityTuning.Hud20ReticleEdge;
                    zoomRingLine20.SetRadii(r - half, r + half);
                    zoomRingEdge20.SetRadii(r - half - e, r + half + e);
                    bool dot = ZoomReticle20.Dot(mag);
                    zoomDot20.enabled = zoomDotEdge20.enabled = dot;
                    float side = 2 * (r + half + e) + 2;
                    zoomRing20.sizeDelta = new Vector2(side, side);
                    zoomRingLine20.rectTransform.sizeDelta = zoomRingEdge20.rectTransform.sizeDelta = new Vector2(side, side);
                }
                float a = ZoomReticle20.Alpha(amount) * ZoomEnter20(ref zoomRingEnter20, frameDt), s = ZoomReticle20.Scale(amount, still);
                if (a != zoomRingAlpha20) zoomRingGroup20.alpha = zoomRingAlpha20 = a;
                if (s != zoomRingScale20)
                {
                    zoomRingScale20 = s;
                    zoomRing20.localScale = new Vector3(s, s, 1);
                }
            }

            bool scopeOn = shown && kind == ZoomReticle20.Scope;
            if (zoomScope20.gameObject.activeSelf != scopeOn)
            {
                zoomScope20.gameObject.SetActive(scopeOn);
                if (scopeOn) zoomScopeEnter20 = 0; // fix H2: the lens fades in too
            }
            if (scopeOn)
            {
                LayoutScope20();
                float a = ZoomReticle20.Alpha(amount) * ZoomEnter20(ref zoomScopeEnter20, frameDt), s = ZoomReticle20.ScopeScale(amount, still);
                if (a != zoomScopeAlpha20) zoomScopeGroup20.alpha = zoomScopeAlpha20 = a;
                if (s != zoomScopeScale20)
                {
                    zoomScopeScale20 = s;
                    zoomScope20.localScale = new Vector3(s, s, 1);
                }
            }
        }

        double zoomRingEnter20 = -1, zoomRingExit20 = -1, zoomScopeEnter20 = -1;
        float zoomRingExitFrom20;

        /// <summary>Fix H2: an entrance's alpha factor (HudEntrance18.Fade: its first frame one 60 Hz frame in, ~0.23), then 1.</summary>
        static float ZoomEnter20(ref double age, double frameDt)
        {
            if (age < 0) return 1f;
            age = HudEntrance18.Step(age, frameDt);
            if (age >= UnityTuning.Hud18FadeInTime)
            {
                age = -1;
                return 1f;
            }
            return HudEntrance18.Fade(age);
        }

        /// <summary>The crosshair colour setting changed: the reticle's lines follow it.</summary>
        void Zoom20Colour()
        {
            zoomColor20 = crossColor;
            zoomRingLine20.color = crossColor;
            zoomDot20.color = crossColor;
            if (zoomScopeFine20 != null)
                foreach (UnityEngine.UI.Image img in zoomScopeFine20)
                    if (img != null) img.color = crossColor;
            if (zoomScopeDot20 != null) zoomScopeDot20.color = crossColor;
        }
    }
}
