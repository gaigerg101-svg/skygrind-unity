// NIGHT54 builder D (design.n54.decor54, a VIEW row; Decor54.cs): the Unity-only numbers of the decor pass: when it runs after a map is built and
// how its proof line tells a cyan piece from the rest. Every colour is the level's script in the config (design.n54.decor54.numbers.levels) or a
// Look49 role token (UnityTuning.Look49.cs); none is typed here. View only: nothing here touches the sim.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>Frames after a new map is seen before the first pass: SurvivalGame.Init builds the map, CampaignLighting24 and its kits (Look43's
        /// stone drain included) in one frame; 3 frames leaves the kits' Start / first Update their turn first.</summary>
        public const int Decor54SettleFrames = 3;

        /// <summary>The longest the first pass waits (frames) for look43's stone drain (Look43Probe.StoneMeshes > 0) when look43 is on: it drains on
        /// its first Update with a sim, a frame or two after the map; 600 frames (5 s at 120 fps) only bounds a map whose stone it never drains.</summary>
        public const int Decor54MaxWaitFrames = 600;

        /// <summary>Seconds (unscaled) after a new map is seen of the second, last pass: catches a view built on its first room's frame (both passes are
        /// idempotent: a material or a mesh is changed once).</summary>
        public const float Decor54SecondPassS = 3f;

        /// <summary>After the two passes, the wall tone's guard reads one probe vertex of the stone every this many frames (no allocation: GetColors into
        /// a kept list); a changed probe (look43's SetOn re-applying its drained copy, taken before the tone) re-tones the walls, at most
        /// Decor54MaxRetones times per map (a writer that rewrites the stone every frame cannot loop it).</summary>
        public const int Decor54RecheckFrames = 60;
        public const int Decor54MaxRetones = 8;

        /// <summary>The proof line's "cyan" test: a _BaseColor whose hue (of its normalised RGB) is within Decor54CyanBandDeg of the usable token's
        /// hue (Look49UsableHex 0x19d3e6: 186 deg) and whose saturation is at least Decor54CyanMinSat (the pale rope / pale cyan of look52 stays out).</summary>
        public const float Decor54CyanBandDeg = 20f;
        public const float Decor54CyanMinSat = 0.35f;

        /// <summary>The proof line's "boundary white" test: hue within Decor54CyanBandDeg of the boundary token (Look49BoundaryHex 0xd8d0ff: 250 deg),
        /// saturation under Decor54BoundaryMaxSat and value over Decor54BoundaryMinV.</summary>
        public const float Decor54BoundaryMaxSat = 0.3f;
        public const float Decor54BoundaryMinV = 0.5f;

        /// <summary>An accent under this saturation is "pale": it is told from the role tokens by saturation, not by hue (the proof line's gap check).</summary>
        public const float Decor54PaleSat = 0.45f;

        /// <summary>Wall faces: a stone vertex is on a side face when |normal.y| is under this (the tops of floors, ledges and pillars keep their colour).</summary>
        public const float Decor54SideMaxNy = 0.5f;
    }
}
