// NIGHT20 E2 (builder P): ASSIST's Unity-only settings row and store (Core/Assist20.cs is the rule; its numbers are
// design.adds.assist). The web has no assist mode, so it has no such row, key or text.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // SETTINGS > ACCESS > ASSIST: the settings key, off by default (Hades keeps God Mode a choice), its label and its line.
        public const string AssistSetting = "assist20";
        public const bool AssistDefault = false;
        public const string AssistLabel = "ASSIST";
        public const string AssistDesc = "TAKE LESS DAMAGE, MORE AFTER EACH DOWN · RUNS ARE STAMPED, NO RECORDS";

        // The downs counted while ASSIST is on (PlayerPrefs, an int): the next run's resistance grows with them.
        public const string AssistDownsKey = "skygrind.assist20.downs";

        // RESULTS' line under the tally for an ASSIST run: the stamp, then this.
        public const string AssistResultsTail = " · NOT A PERSONAL BEST";
    }
}
