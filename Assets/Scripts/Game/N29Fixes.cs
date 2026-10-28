// NIGHT29: view / flow-only switches (no sim effect, so no tape or fingerprint change). `-skygrindOff29` turns them all off.
using System;

namespace Skygrind.Game
{
    public static class N29Fixes
    {
        static readonly bool off = Array.IndexOf(Environment.GetCommandLineArgs(), "-skygrindOff29") >= 0;

        /// <summary>A `-skygrind...` argument that marks automation (the boot then ignores a pending level and a saved campaign); the NIGHT29 switches and
        /// the level-select check boot like a player's game.</summary>
        public static bool IsAutomationArg(string a) => a.StartsWith("-skygrind", StringComparison.Ordinal) && a != "-skygrindOff29" && a != "-skygrindOff30" && a != "-skygrindOff31" && a != "-skygrindOff32" && a != "-skygrindOff33" && a != "-skygrindOff34" && a != "-skygrindOff35" && a != "-skygrindOff36" && a != "-skygrindOff37" && a != "-skygrindOff38" && a != "-skygrindOff39" && a != "-skygrindOff40" && a != "-skygrindOff41" && a != "-skygrindOff42" && a != "-skygrindOff43" && a != "-skygrindOff44" && a != "-skygrindOff45" && Skygrind.Core.NightRows.OffSwitchNight(a, "-skygrindOff") < Skygrind.Core.NightRows.FirstNight /* NIGHT46 rows: -skygrindOff46 and later boot like a player's game */ && a != "-skygrindLevelSelect29" && a != "-skygrindDoorFree29" && a != "-skygrindNoHold29" && !Room71.PlayerSwitch71(a) /* NIGHT71 B: the n71 rows' -skygrindNo<Row>71 (GG's A / B) boot like a player's game */ && !Rows72.PlayerSwitch72(a) /* NIGHT72 D-3: -skygrindRows72 and the n72 rows' -skygrindNo<Row>72 boot like a player's game */;

        /// <summary>Item 1: the level select's scene reload starts its run after StartGame's ToTitle (not in Awake, where ToTitle undid it).</summary>
        public static bool LevelSelectDeferStart => !off;

        /// <summary>Item 14's Player check holds the DOORFREE toggle through this (the raw RB / F8 read in SurvivalGame.Update ORs it in).</summary>
        public static bool ForceDoorFreeHold;

        /// <summary>Item 1: a level press while a reload is already queued is dropped (a double click cannot restart the load).</summary>
        public static bool LevelSelectIgnoreRepress => !off;

        /// <summary>Builder B: the reworked bosses' view (lane lines, frost fields, the Lantern's lava glow, the dusk rink): not -skygrindOff29 (the sim blocks gate the rest).</summary>
        public static bool BossViewOn => !off;

        /// <summary>Item 4 (builder V): the CRT overlay runs: not -skygrindOff29 and design.n29.crt.enabled (the lead flips it when the contract passes).</summary>
        public static bool CrtOn(Skygrind.Core.GameConfig cfg) => !off && cfg != null && cfg.Design("n29.crt.enabled");

        /// <summary>Items 2 and 5 (builder V): the light sockets, door frames and closed roofs: not -skygrindOff29 and the row's own enabled flag (lights / props / doors).</summary>
        public static bool ViewRowOn(Skygrind.Core.GameConfig cfg, string row) => !off && cfg != null && cfg.Design("n29." + row + ".enabled");
    }
}
