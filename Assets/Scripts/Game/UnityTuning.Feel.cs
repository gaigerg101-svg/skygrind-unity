// NIGHT12 (agent C): the Unity-only numbers of the feel views (the viewmodel, CameraFeel, MovementFx, WeaponFx), each with
// why. Every gameplay and web-tuned number comes from PortExport/config.json instead (viewmodel.*, fx.*, feel.*).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the FX draws (MovementFx.cs: FxCubes, FxLines) -------------------------------------------------------------
        // URP Unlit has no per-instance colour, so the instanced cubes and lines draw one RenderMeshInstanced per colour:
        // at most this many colours per pool (the FX use a handful: the dust, cyan, a gun family's).
        public const int FxColourBuckets = 16;
        // The additive lines fade (colour x life / maxLife, effects.js) in this many brightness steps per colour (a step is
        // one material); 8 steps over the 0.06 s tracer is about one step per frame at 144 fps.
        public const int FxLineLevels = 8;
        // The lines' width (m) at their start; the web's are 1 px GL lines (three.js LineSegments), which a Unity mesh can't
        // draw, so they are thin quads facing the camera, tapering to FxLineTaper x the width at the end (the tracer reads
        // thicker at the muzzle: Research/2026-10-03-polish.md A5).
        public const float TracerWidth = 0.014f;
        public const float StreakWidth = 0.01f;
        public const float FxLineTaper = 0.5f;
        // The pools' sizes: the cubes of one view (the web's particles share fx.poolSize 400 with the enemy bursts, which
        // Particles draws) and the shells kept on the floor (Research A4: "kept on the floor 8 s (cap 24)").
        public const int FxCubePool = 192;
        public const int ShellPool = 24;

        // --- cosmetic randomness: each view's own Mulberry32, seeded per run with the run seed x its salt (never a sim stream)
        public const uint MovementFxSeedSalt = 0x6d6f7665; // "move"
        public const uint WeaponFxSeedSalt = 0x77666678; // "wffx"

        // --- MovementFx -----------------------------------------------------------------------------------------------
        // Wall-ride contact sparks (port-only: the web has the streaks, not sparks; Research D1 "pale-cyan contact sparks at
        // 25/s, life 0.18 s (fx.spark*)"): sparks per second while riding; each is one fx.spark* cube in colors.usable.
        public const double WallSparkRate = 25;
        // The dash edge tint's canvas (player.js createDashTint is a CSS layer under the HUD): its sort order, under the
        // HUD's canvas (Ui.cs: 10) and over the world.
        public const int DashTintSortingOrder = 5;
        // The web's box-shadow colours' alphas (player.js :66-67: the side fringes 0.55, the top / bottom 0.25), literals
        // of the web's CSS that config.js doesn't hold.
        public const float DashTintSideAlpha = 0.55f;
        public const float DashTintEdgeAlpha = 0.25f;
        // The slam ring's segments' tessellation is fx.slamRingSegments; its material is the acid's alpha blend (the web's
        // MeshBasicMaterial transparent) with no polygon offset (URP has none): fx.slamRingLift alone keeps it off the floor.

        // --- WeaponFx: the THUNDERCLAP's shells (port-only: the web ejects none; Research A4, after Nijman's "shells that
        // stay") ---------------------------------------------------------------------------------------------------------
        // Where the shell leaves the gun: a point on the receiver's right side, web rig space (m; the rig's grip is 0).
        public const float ShellPortX = 0.036f, ShellPortY = 0.04f, ShellPortZ = -0.08f;
        // Its throw (m/s, camera space: right, up, back toward the shooter), on top of the player's velocity.
        public const float ShellSpeedMin = 1.5f, ShellSpeedMax = 2.5f, ShellUp = 1.6f, ShellBack = 0.4f;
        // Its tumble (deg/s), its size (m: a 12-gauge hull, about 2 x 2 x 6 cm at the blocky kit's scale), its colour
        // (a dull brass hull: not cyan, red-orange or magenta, which mean something), the bounce it keeps (x the speed at
        // each floor hit), the slide it keeps, and the speed under which it lies still.
        public const float ShellSpin = 900f;
        public const float ShellSizeXY = 0.018f, ShellSizeZ = 0.05f;
        public const uint ShellHex = 0x9a7a45;
        public const float ShellBounce = 0.35f, ShellFriction = 0.6f, ShellRestSpeed = 0.4f;
        // How long a shell stays (s; Research A4 "kept on the floor 8 s") and the last part of it in which it shrinks away.
        public const float ShellKeep = 8f, ShellFadeTime = 0.4f;
    }
}
