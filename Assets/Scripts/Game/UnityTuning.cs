// Unity-only numbers (NIGHT7 rule 5): render, camera stack and UI layout values that have no web counterpart, each with
// why. Every gameplay number comes from PortExport/config.json instead (GameConfig).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>Stage 1's PlayerPrefs key of the aim-assist toggle; read once into the settings (GameSettings), then unused.</summary>
        public const string AimAssistPref = "skygrind.aimAssist";
        // NIGHT13 U3-26.10: the saved run's file beside the profile (design.save; one slot). The web has no save or suspend.
        public const string SaveFileName = "skygrind-run.json";

        // three.js (r155+, physical lights) divides Lambert by pi; URP doesn't. A web light of intensity I lights like a
        // URP light of I / pi (directional key, hemisphere ambient).
        public const float LightScale = 1f / UnityEngine.Mathf.PI;

        // The web's exposure lands after its ACES pass; this keeps the same mid-grey after URP's ACES.
        public const float Exposure = 0.35f;

        // URP bloom: the web's Bloom mipmap threshold is 1 (post.js); intensity and scatter are URP's own knobs.
        public const float BloomThreshold = 1f;
        public const float BloomIntensity = 0.8f;

        // Neon (strips, rims, pad rings) = colors.usable x world.neonBase, in HDR so bloom picks it up.
        public const float NeonHdr = 1f;

        // The gate fields' HDR multiplier (additive sheet).
        public const float FieldHdr = 1.6f;

        // URP main-light shadow distance (m); the web's high preset shadows 42 m around the player.
        public const float ShadowDistance = 60f;

        // Seconds a debug overlay line stays before it is refreshed.

        // theme.js --usable-dim: colors.usable at this alpha (the stamina segments' back). A web UI number that config.js
        // doesn't hold (theme.js computes it inline).

        // theme.js / hud.js: the stamina segments' height (px at 1080p, .hud-seg) and their gap (.hud-stamina gap).

        // The SETTINGS screen's layout (px at 1080p; the web's menus.css panel is fluid): the row width and height, a tab's width.

        // NIGHT8 U2-16: the LAB settings tab (Unity-only; the web has no LAB): its id in the settings items and its label.
        public const string LabTabId = "lab";
        public const string LabTabLabel = "LAB";

        // The HUD perk row's icon size (px at 1080p; the web's hud.css row is fluid).
        public const float PerkIconSize = 30f;

        // The MODS draft's cards (px at 1080p; the web's menus.css cards are fluid).
        public const float ModCardWidth = 380f;
        public const float ModCardHeight = 330f;

        // Vsync's default (SETTINGS can turn it on). The web has no such setting: the browser always presents on vsync.
        // Off by default so the bench measures the sim and the render, as stage 1's bench did (vsync 0, no cap).
        public const bool VsyncDefault = false;

        // NIGHT10 U3-13.8: the Mystery Box's drawing (the web has no box; its gameplay numbers are design.box). The crate
        // (m: a CoD-sized chest), the lid's swing speed (opens per s), the gun's rise over the box (m) and its size (m, the
        // wall buys' hologram fit), and how long before the offer ends the gun starts to sink (s).
        public const float BoxWidth = 1.4f;
        public const float BoxDepth = 0.75f;
        public const float BoxHeight = 0.8f;
        public const float BoxLidSpeed = 3f;
        public const float BoxRise = 0.6f;
        public const double BoxHoloSize = 0.7;
        public const double BoxSinkTime = 1.5;

        // NIGHT10 U3-13.6 (weaponFx.js, effects.js decals): the depth (m) in the world camera's view where a muzzle drawn
        // by the overlay viewmodel camera is placed (Viewmodel.OnShot's literal for the tracer's start); the ricochet
        // bounce line's width (m; the web's Line is 1 px, this is the port's tracer width); how near (m) a re-cast hit must
        // be to the THUNDERCLAP's recorded end to count as that surface (the port's shotResult carries no pellet normals).
        public const float FxMuzzleDepth = 0.6f;
        public const float FxBounceWidth = 0.02f;
        public const double DecalOnSurface = 1e-3;

        // NIGHT11 U3-24.5 (TrapViews): the trap switches' screen text (world-space TMP size, letters about 0.11 m; the web's
        // CRT is a canvas texture) and the pool of the pylon's zap lines (fx/arcs.js is not ported: one line per zap).
        public const float TrapLabelFontSize = 1.1f;
        public const int TrapZapPool = 8;

        // NIGHT11 U3-24.6 / U3-24.7 (RunShapeDesignViews: port-only designs, no web counterpart). The power switch: its
        // cabinet (m), the lever (m; its lean from vertical, degrees: up when off, down when on; its swing per s), the
        // beacon over it while off (m), the lamp's pulse while off (Hz) and the dimming once thrown (x the cyan).
        public const float PowerWidth = 0.9f, PowerHeight = 1.6f, PowerDepth = 0.5f;
        public const float PowerLeverLength = 0.45f, PowerLeverAngle = 35f, PowerLeverSpeed = 4f;
        public const float PowerBeaconHeight = 30f, PowerBeaconRadius = 0.25f;
        public const float PowerPulseRate = 2.5f, PowerDim = 0.25f;
        // The exfil radio's cabinet (m), its antenna over it (m) and its light's blinks while it can be called (Hz).
        public const float RadioWidth = 0.6f, RadioHeight = 1.0f, RadioDepth = 0.4f, RadioAntenna = 0.9f, RadioBlinkRate = 2f;
        // The exfil site: the ring's band (m), the beacon's radius (m), the ring's pulse during REACH (Hz).
        public const float SiteRingBand = 0.3f, SiteBeaconRadius = 0.7f, SitePulseRate = 1.5f;
    }
}
