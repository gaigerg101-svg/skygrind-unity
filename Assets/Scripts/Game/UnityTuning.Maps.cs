// NIGHT15 U3-28.5: choosing a map (SurvivalGame.Maps.cs, TITLE's MAP row in UiMeta.cs). Prefs keys, folder names and the
// row's layout and texts: the web has one map, so it has none of these. The maps themselves are design.maps.list.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The saved map pick (PlayerPrefs; a per-machine convenience like the window size: not the run's data).
        public const string MapPref = "skygrind.map";

        // A map other than the arena keeps its best runs (the ghosts) under <profile folder>/maps/<id>/.
        public const string MapProfileFolder = "maps";

        // The arena's label when design.maps.list doesn't name it (the old map, the web's).
        public const string MapArenaLabel = "ARENA";

        // TITLE's MAP group: on the run setup's line (SetupAbovePlay), after it (SetupGroupGap), the same button height; a label,
        // one button per map (wide enough for "DRYDOCK" at the label size).
    }
}
