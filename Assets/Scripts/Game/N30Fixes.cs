// NIGHT30: view / flow-only switches (no sim effect, so no tape or fingerprint change). `-skygrindOff30` (or -skygrindOff29, which
// gives NIGHT28's game) turns them all off: the RED half of each P0 proof is tonight's build run with -skygrindOff30.
using System;

namespace Skygrind.Game
{
    public static class N30Fixes
    {
        static readonly string[] args = Environment.GetCommandLineArgs();
        static readonly bool off = Array.IndexOf(args, "-skygrindOff30") >= 0 || Array.IndexOf(args, "-skygrindOff29") >= 0;

        /// <summary>Every NIGHT30 view / flow fix is on (not -skygrindOff30).</summary>
        public static bool On => !off;

        /// <summary>A view row's switch: not -skygrindOff30 and the row's own `design.n30.<row>.enabled` (hud30, look30, lavaView, ...).</summary>
        public static bool ViewRowOn(Skygrind.Core.GameConfig cfg, string row) => !off && cfg != null && cfg.Design("n30." + row + ".enabled");
    }
}
