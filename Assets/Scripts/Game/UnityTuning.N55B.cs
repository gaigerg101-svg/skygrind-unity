// NIGHT55 builder B (bosses): the render and reel numbers of design.n55.grade55 / wick55 / bossText55 (BossGrade55.cs, Wick37View.N55.cs,
// FeelReel.N55.Boss.cs). Unity-only: the sim never reads them (golden vectors, replays and fingerprints untouched). The palette, the heat and the
// proof thresholds are the row's numbers in Design/unity-config.json (design.n55.grade55.numbers).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>THE WICK's heat patch: the most quads laid over the seams in one frame (9 facets x 7 rows + 9 iron quads at the default span; a
        /// bigger patch is cut at this count, never allocated past it).</summary>
        public const int Grade55HeatPool = 96;

        /// <summary>a155's sample grid over the captured frame (16:9; one ray into the C# world and one pixel per cell).</summary>
        public const int Grade55GridW = 96, Grade55GridH = 54;

        /// <summary>a155's class-mask image: each grid cell drawn as this many pixels square (96 x 54 -> 960 x 540), saved beside the frame.</summary>
        public const int Grade55MaskCell = 10;

        /// <summary>a155: a UI graphic covering more than this share of the screen is an overlay (a vignette, a flash, the letterbox's full image),
        /// not a HUD element: it does not mask the samples under it (it is counted and printed).</summary>
        public const float Grade55UiOverlayShare = 0.35f;

        /// <summary>a155's burn / cold frames: only the middle of the screen this wide and tall (as a share) counts in the classes: a burn's hurt
        /// vignette is a red border (the lead's n55-r1: it raised the other walls' S at the edges); the border's cells are drawn black in the mask.</summary>
        public const float Grade55CentralShare = 0.7f;

        /// <summary>a155: a UI graphic fainter than this (its colour alpha x its inherited alpha) does not mask the samples under it.</summary>
        public const float Grade55UiMinAlpha = 0.05f;
    }
}
