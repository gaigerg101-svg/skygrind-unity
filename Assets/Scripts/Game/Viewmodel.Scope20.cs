// NIGHT20 U3-34.3 (builder V): the zoom on most guns, in the hands (GG: "most weapons should ... zoom: simple zoom").
// - ZoomOf: the viewmodel's zoom for the held gun: the web's ads.weapons.<gun>.zoom (KESTREL 1.5, LONGWAVE 3.5), and the
//   STITCHER's design.zoom.nailgunMag (1.25: builder G's block) while the run's design.zoom is on (the web never zoomed the
//   nailgun). The FOV (CombatMath.ZoomedFov, the web's optical formula) and the ADS sensitivity read it.
// - The LONGWAVE's scope rims: the web's scope is a flat HUD vignette (hud.js .hud-scope: clear to 31vh, dark by 45vh;
//   builder H draws it tonight). On the gun, two lens edges on the overlay camera make it a tube, not a decal: the
//   eyepiece's rim on the eye (ScopeEyeDepth) at the vignette's 31vh, and the objective's rim at the tube's far end
//   (ScopeObjectiveDepth), a touch smaller, moving with the gun's sway x ScopeParallax (the eyepiece stays put): the
//   parallax a real scope tube shows when the hand drifts. Additive, the player's cyan-pale (CLAUDE.md rule 8), fading
//   in over the ADS amount's last ScopeShowFrom..1 (the scope arriving at the eye). Static (no motion): REDUCED MOTION
//   keeps it; the sway it follows is already stilled by REDUCED MOTION. With design.zoom off the LONGWAVE never zooms in
//   Unity (Combat.cs StepAds: the KESTREL only), so NIGHT19's look is unchanged.
// The split with H (agreed in the report): H owns the HUD's scope vignette, its cross hairs and the rangefinder text; V
// owns the gun's pose and these rims on the overlay camera.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The scope rims' sizes, pure (ZoomLookTests).</summary>
    public static class ScopeMath
    {
        /// <summary>The radius (m) of a ring at `depth` m in front of a camera of vertical FOV `fovDeg` that shows at
        /// `screenShare` of the screen's height from its middle (hud.js's 31vh: 0.31).</summary>
        public static double RimRadius(double screenShare, double fovDeg, double depth) =>
            2 * screenShare * System.Math.Tan(fovDeg * 0.5 * System.Math.PI / 180) * depth;
    }

    public sealed partial class Viewmodel
    {
        Mesh scopeRing;
        Material scopeMat;
        RenderParams scopeEyeRp, scopeObjRp;
        readonly MaterialPropertyBlock scopeEyeBlock = new MaterialPropertyBlock(), scopeObjBlock = new MaterialPropertyBlock();
        static readonly int ScopeColorId = Shader.PropertyToID("_BaseColor");

        /// <summary>The held gun's zoom: builder G's Loadout.ZoomGun (Core/Guns/Zoom20.cs: the web's ads.weapons.<gun>.zoom,
        /// the STITCHER's design.zoom.nailgunMag, 0 for a unique-LT gun), at least 1 (G's two Viewmodel lines, here once).</summary>
        double ZoomOf(string held, Gun gun) => game.loadout != null ? Math.Max(1, game.loadout.ZoomGun(held)) : gun.zoom;

        void ScopeBuild()
        {
            scopeRing = FxDraw.Ring((float)(1 - UnityTuning.ScopeRimWidth), 1, UnityTuning.ScopeRimSegments, "scope rim");
            scopeMat = FxDraw.Additive(game.mats.neon, "Scope rims");
            scopeMat.SetFloat("_Cull", 0);
            scopeEyeRp = FxDraw.Params(scopeMat, Layer);
            scopeEyeRp.matProps = scopeEyeBlock;
            scopeObjRp = FxDraw.Params(scopeMat, Layer);
            scopeObjRp.matProps = scopeObjBlock;
        }

        /// <summary>Is the held gun the scope (ads.weapons.<gun>.scope)?</summary>
        bool IsScope(string held) => held == "rail" && cfg.Has("ads.weapons.rail.scope") && cfg.Bool("ads.weapons.rail.scope");

        bool? railScope;

        /// <summary>
        /// The LONGWAVE's rims this frame, on the overlay camera: `ak` the eased ADS amount, (swayX, swayY) the gun's offset
        /// from its ADS rest (web camera space, m) this frame.
        /// </summary>
        void ScopeFrame(string held, bool show, double ak, double swayX, double swayY)
        {
            if (!show || held != "rail" || ak <= UnityTuning.ScopeShowFrom) return;
            if (railScope == null) railScope = IsScope(held);
            if (railScope != true) return;
            if (scopeRing == null) ScopeBuild();
            double u = (ak - UnityTuning.ScopeShowFrom) / (1 - UnityTuning.ScopeShowFrom);
            float k = (float)(u * u * (3 - 2 * u));
            Color pale = UiTheme.Ready ? UiTheme.CyanPale : Color.white;
            double fov = vmCam.fieldOfView;
            Matrix4x4 cam = vmCam.transform.localToWorldMatrix;
            Quaternion facing = Quaternion.Euler(90, 0, 0); // the flat ring (y up) turned to face the eye (normal along +z)
            // the eyepiece's rim (on the view axis at the vignette's clear edge) is H's lens rim tonight (UiZoom20.cs: a
            // CYAN-DEEP rim over its INK mask, on the HUD): drawn once, by H; only the objective's parallax rim is the gun's
            if (UnityTuning.ScopeDrawEyepiece)
            {
                float d1 = (float)UnityTuning.ScopeEyeDepth, r1 = (float)ScopeMath.RimRadius(UnityTuning.ScopeRimScreen, fov, d1);
                scopeEyeBlock.SetColor(ScopeColorId, pale * (float)UnityTuning.ScopeRimAlpha * k);
                Graphics.RenderMesh(scopeEyeRp, scopeRing, 0, cam * Matrix4x4.TRS(new Vector3(0, 0, d1), facing, new Vector3(r1, 1, r1)));
            }
            // the objective: the tube's far end, smaller, drifting with the sway (web x right, y up: Unity's too)
            float d2 = (float)UnityTuning.ScopeObjectiveDepth, r2 = (float)(ScopeMath.RimRadius(UnityTuning.ScopeRimScreen, fov, d2) * UnityTuning.ScopeObjectiveShrink);
            var off = new Vector3((float)(swayX * UnityTuning.ScopeParallax), (float)(swayY * UnityTuning.ScopeParallax), d2);
            scopeObjBlock.SetColor(ScopeColorId, pale * (float)UnityTuning.ScopeObjectiveAlpha * k);
            Graphics.RenderMesh(scopeObjRp, scopeRing, 0, cam * Matrix4x4.TRS(off, facing, new Vector3(r2, 1, r2)));
        }
    }
}
