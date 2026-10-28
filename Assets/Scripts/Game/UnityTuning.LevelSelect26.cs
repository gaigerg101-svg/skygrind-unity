// NIGHT26 (builder L): Unity-only layout numbers for the LEVEL SELECT (UiLevelSelect26.cs) and the ranks on the CAMPAIGN RESULTS. 1080p reference, y up from the screen
// centre. Nothing here is a gameplay number (the ranks' numbers are design.n26.rank). The web has no campaign.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float LevelSel26RowWidth = 1340f; // px: a slot's plate (the column's left edge at the first row is -555, so the right end stays inside +808)
        public const float LevelSel26FirstRowY = 322f; // px: the first slot's centre (the head sits at MetaHeadY 420, the column headers at +372)
        public const float LevelSel26HeadersY = 378f; // px: the column labels
        public const float LevelSel26RowPitch = 66f; // px between slots (the plate is MenuItemHeight 60; ten rows end at -272)
        public const float LevelSel26NameW = 540f; // px: the "1  THE FOUNDRY" label (S32)
        public const float LevelSel26RankX = 560f, LevelSel26RankW = 70f; // px from the plate's left edge: the aggregate letter (S42)
        public const float LevelSel26TimeX = 650f, LevelSel26TimeW = 110f; // the best time (S24)
        public const float LevelSel26KillsX = 770f, LevelSel26KillsW = 100f; // the best kills %
        public const float LevelSel26StyleX = 880f, LevelSel26StyleW = 90f; // the best style letter
        public const float LevelSel26UnlockX = 990f, LevelSel26UnlockW = 340f; // the weapon the level unlocks
        public const float LevelSel26LockedDim = 0.6f; // alpha of the INK veil laid over a LOCKED slot
        public const float LevelSel26BackX = -230f; // px: BACK (the meta screens' BACK)
        public const float LevelSel26HintY = -352f; // px: the one-line hint under the column
        public const double LevelSel26FocusHold = 1.0; // s: the cursor row keeps the focus after the screen opens (the menu nav re-picks the first row in the frames after a screen shows)
        public const double ReelTutorialGameCap26 = 420; // s of GAME time the `tutorial` reel may run (level 1 is about 250 s with the hulk: 14 stages and the fight); never a frame count
        public const float ReelTutorialWallGuard26 = 600f; // s of wall clock: a stuck reel still ends
        public const string LevelSel26Head = "CAMPAIGN";
        public const string LevelSel26Locked = "LOCKED";
        public const string LevelSel26None = "-";

        // the RESULTS' rank line (UiCampaignResults25): the three letters under the head
        public const float CampRes26RanksY = 258f; // px: the TIME / KILLS / STYLE / OVERALL line with the unlocked weapon (S24), between the summary (290, S32) and the first room row (225, S32)
        public const float CampRes26Width = 1300f; // px: that line's box (it holds the unlock text too)
    }
}
