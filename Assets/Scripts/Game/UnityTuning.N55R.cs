// NIGHT55 builder R (P2-clear; design.n55.clear55, a view row): the room-clear card's layout and motion (UiClear55.cs) and the a455 reel's
// placement (FeelReel.N55.Room.cs). Unity-only numbers: the sim never reads them (golden vectors, replays and fingerprints untouched). Px at
// 1080p (the HUD canvas scales with the height). The card's type is HUD33's own (UnityTuning.Hud33.cs: Hud33Name 24 px labels, Hud33Mid
// 32 px numbers, the 2 px shadow, bone / cyan); the hold's length is gameplay pacing and lives in the config (design.n55.clear55.numbers.holdS).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- the card (UiClear55.cs) ----
        public const float Clear55Y = -190f; // px from the screen's middle: under the reticle and its rows (the grapple row ends at -82), clear of RIDE KILL (+150) and the top-centre message slot
        public const float Clear55W = 360f; // px: the card's box (Hud33MaxWidth: no HUD33 element wider)
        public const float Clear55HeadH = 28f; // px: the head line's box (a 24 px label)
        public const float Clear55LineH = 38f; // px: the number line's box (a 32 px number)
        public const float Clear55Gap = 4f; // px between the two lines (the card is 70 px tall: under Hud33MaxHeight 80)
        public const double Clear55LifeS = 2.6; // s of HUD frame time the card stays (the 0.5 s hold, then ~2 s to read it while the door opens)
        public const double Clear55FadeS = 0.35; // s of the life's end it fades over

        // ---- the a455 reel (FeelReel.N55.Room.cs) ----
        public const int Clear55ReelRooms = 2; // combat rooms per level the reel clears (each a hold measured and two frames: mid-hold and the card)
        public const double Clear55ReelKillEvery = 0.25; // s of sim time between the reel's KillAll sweeps (the waves and their spawn tells come in)
        public const double Clear55ReelRoomMaxS = 45; // s of sim time a room may take to clear before the reel gives up on it (a lesson's top-ups)
        public const int Clear55ReelShotAtFrame = 6; // held frames into the hold the mid-hold frame is taken (~0.1 s at 60 fps; the hold is measured on the loop's own clock)
        public const double Clear55ReelCardShotS = 0.3; // s of real time after the hold the card's frame is taken (the card still up)
        public const double Clear55ReelHoldMin = 0.4, Clear55ReelHoldMax = 0.6; // s: the slate's band for the hold served (P2-clear doneWhen)
        public const float Clear55ReelMinAlpha = 0.5f; // the card's on-screen alpha (through every parent CanvasGroup) at its frame for GREEN
    }
}
