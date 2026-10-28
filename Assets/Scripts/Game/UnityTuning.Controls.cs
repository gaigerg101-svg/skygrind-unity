// NIGHT13 U3-26.8 (agent S: settings, CONTROLS, menu navigation): the Unity-only UI layout and settings numbers
// (display rows, the selected state, the settings file), each with a comment saying why the web has no value for it.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the settings file (GameSettings) ---------------------------------------------------------------------------
        // The settings JSON beside the profile (Application.persistentDataPath). The web keeps it in localStorage under
        // skygrind.settings.v1 (SettingsStore.StorageKey, the port's PlayerPrefs key until NIGHT13, migrated once).
        public const string SettingsFileName = "skygrind-settings.json";

        // --- the Unity-only display rows (VIDEO tab) -----------------------------------------------------------------------
        // WINDOW MODE replaces the web's FULLSCREEN toggle (a browser page can only ask for fullscreen; a Player picks the
        // mode). Options: a window, a fullscreen window (borderless), exclusive fullscreen. The default is the web's
        // fullscreen: false (a window), which is what the Player did before NIGHT13; a saved FULLSCREEN on migrates to
        // WindowFullscreen once (SettingsMath.MigrateWindowMode).
        public const string WindowWindowed = "windowed";
        public const string WindowFullscreen = "borderless";
        public const string WindowExclusive = "exclusive";
        public const string WindowModeDefault = WindowWindowed;
        public const string WindowModeLabel = "WINDOW MODE";
        public const string WindowModeDesc = "FULLSCREEN WINDOW fills the display without a border; EXCLUSIVE gives the display to the game alone";
        public static readonly (string key, string label)[] WindowModes =
        {
            (WindowWindowed, "WINDOWED"), (WindowFullscreen, "FULLSCREEN WINDOW"), (WindowExclusive, "EXCLUSIVE"),
        };
        // RESOLUTION: AUTO leaves the size alone (the display's in fullscreen, the Player's window otherwise: the Player's
        // behaviour before NIGHT13); the other options are the display's modes (Screen.resolutions, one per size). The web
        // has no such row: the browser sizes the canvas.
        public const string ResolutionAuto = "auto";
        public const string ResolutionDefault = ResolutionAuto;
        public const string ResolutionLabel = "RESOLUTION";
        public const string ResolutionAutoLabel = "AUTO";
        public const string ResolutionDesc = "AUTO keeps the display's own size in fullscreen";
        // The tab both rows sit on (config.ui.settingsTabs' VIDEO).
        public const string VideoTabId = "video";

        // --- descriptions Unity changes (config.ui.settings' desc, which the port's rows don't all match) ------------------
        // REDUCED MOTION: the web's row says camera roll, shake, colour fringing and speed lines; the port's covers the
        // chroma pulses too (PostFx.cs reads it) and the gun's bob (NIGHT13 U3-26.8 asks its owner, Viewmodel, to read it;
        // the camera has no bob, as the web).
        public const string ReducedMotionDesc = "No camera roll, shake, gun bob, colour fringing, chroma pulses or speed lines";
        // UI SCALE: the HUD follows it; the menus stay at 100% (they are laid out edge to edge at 1080p, so a larger scale
        // would push their buttons off the screen). The web scales its menus too, and scrolls them.
        public const string UiScaleDesc = "Every HUD cluster, in % (the menus stay at 100%)";
        // The UI SCALE value that leaves the canvas as it was (config.ui.settings uiScale is in %).
        public const double UiScalePercent = 100;

        // --- SETTINGS' description line (menus.js: the desc under a row; px at 1080p) ---------------------------------------
        // Under the tallest tab's eight rows (the last row's bottom at -185) and over RESET TAB / BACK (top at -373).

        // --- the selected control (UiNav; menus.js .menus.pad button:focus: cyan text, a cyan edge, rgba(cyan, 0.08)) --------
        // A frame round the selected button or row: its edge width and how far it stands out of the control (px at 1080p),
        // and the cyan fill's alpha (a pick a little over the web's 0.08; not checked against a capture).
        public const float NavFocusOutset = 3f;
        // Controls whose centres are this close in height (px at 1080p) are one row of the reading order (the title's run
        // setup chips, the draft's cards, SETTINGS' tabs): left / right walk them, up / down leave the row.
        public const float NavRowTolerance = 20f;
        // Up / down score a control by its distance that way plus this x the horizontal gap between the two (0 when they
        // overlap), so the same column wins over a nearer control in the next column (uGUI's automatic navigation weighs
        // the side offset the same way; the web walks one list).
        public const float NavColumnWeight = 3f;

        // --- CONTROLS (UiControls; menus.js lays it out in a CSS grid: these are the port's px at 1080p) --------------------
        // From the top: the heading, the hint line, the page tabs (only while a pad is connected), the grid, the rebind
        // note, RESET + BACK (the heading's and the buttons' boxes stay inside the 1080 px height).
        public const float ControlsHeadY = 470f;
        public const float ControlsHintY = 422f;
        public const float ControlsTabsY = 375f;
        public const float ControlsTabWidth = 300f;
        public const float ControlsTabHeight = 46f;
        public const float ControlsGridTop = 335f;
        public const float ControlsGridBottom = -385f;
        public const float ControlsNoteY = -415f;
        public const float ControlsButtonsY = -478f;
        public const float ControlsButtonSpacing = 460f; // RESET's and BACK's centres apart (SETTINGS' RESET TAB / BACK)
        // The KEYBOARD + MOUSE page: four columns of this width and gap (1678 px: inside a 16:10 screen's 1728 at 1080p, so
        // the Steam Deck's 1280 x 800 shows every column); controlRows' eight groups fill them (MOVE + FIGHT + SURVIVAL,
        // then ARCLINE + RUN + MOUSE, TECHNIQUES, READING THE HUD).
        public const int ControlsColumns = 4;
        public const float ControlsColumnWidth = 400f;
        public const float ControlsColumnGap = 26f;
        // A group's header line, a key row (its key button inside), the gap between a label and its key, under a wrapped
        // row, and between groups.
        public const float ControlsHeaderHeight = 34f;
        public const float ControlsKeyRowHeight = 38f;
        public const float ControlsBindWidth = 180f;
        public const float ControlsBindHeight = 34f;
        public const float ControlsCellGap = 10f;
        public const float ControlsRowGap = 4f;
        public const float ControlsGroupGap = 14f;
        // The GAMEPAD page: one column of the pad's 17 actions (17 x 38 + the header fits the grid's 720 px), its button
        // cells wide enough for PRESS A PAD BUTTON at 16 px.
        public const float ControlsPadColumnWidth = 520f;
        public const float ControlsPadBindWidth = 220f;
    }
}
