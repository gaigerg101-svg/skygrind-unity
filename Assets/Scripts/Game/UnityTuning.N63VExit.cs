// NIGHT63 builder V (exit63's SPOT pose; FeelReel.N63V.Exit.cs, a157 with -skygrindClearShots60 -skygrindExitShots63 [room]): a reel-only Unity
// number (CLAUDE.md rule 5, its comment). The sim never reads it.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>m from room A's face, on the door's axis, where the exit63 shot stands looking at the door. a157's clear shots stand at the
        /// room's entry, 18-27 m out on n57-kiln, where the door's box is 15,004-37,810 px on b3 (under rule 8's 41,472 px even if every pixel
        /// changed). At camera.fovDeg 74 (vertical) and 1080p a 4 x 4.5 m opening covers about 18 x (716.6 / d)^2 px: 92,000 px at 10 m (41,472
        /// needs 14.3 m or nearer). The spot 10 m out stands clear of every box and ramp on kiln's kb2, kb3, kb3c and kb3d door axes (the level
        /// read, not walked); kb3's 4.5 m ramp 3-6 m in front of d6 still hides much of that door from it, and kb2c's axis meets its 4.5 m ramp
        /// 9-12 m out, so kb2c's shot stands on or in it (the line prints the spot). The SPOT pose is kb3d's.</summary>
        public const float Exit63ShotM = 10f;
    }
}
