// NIGHT31: view / flow-only switches (no sim effect, so no tape or fingerprint change). `-skygrindOff31` (or -skygrindOff30 / -skygrindOff29)
// turns them all off: the RED half of each P0 proof is tonight's build run with -skygrindOff31.
using System;

namespace Skygrind.Game
{
    public static class N31Fixes
    {
        static readonly string[] args = Environment.GetCommandLineArgs();
        static readonly bool off = Array.IndexOf(args, "-skygrindOff31") >= 0 || Array.IndexOf(args, "-skygrindOff30") >= 0 || Array.IndexOf(args, "-skygrindOff29") >= 0;

        /// <summary>Every NIGHT31 view / flow fix is on (not -skygrindOff31).</summary>
        public static bool On => !off;

        /// <summary>A view row's switch: not -skygrindOff31 and the row's own `design.n31.<row>.enabled` (grappleMark, lowHealthEdge, ...).</summary>
        public static bool ViewRowOn(Skygrind.Core.GameConfig cfg, string row) => !off && cfg != null && cfg.Design("n31." + row + ".enabled");
    }
}
