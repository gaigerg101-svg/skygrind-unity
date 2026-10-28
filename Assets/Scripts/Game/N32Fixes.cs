// NIGHT32: view / flow-only switches (no sim effect, so no tape or fingerprint change). `-skygrindOff32` (or -skygrindOff31 / -skygrindOff30
// / -skygrindOff29) turns them all off: the RED half of each view proof is tonight's build run with -skygrindOff32.
using System;

namespace Skygrind.Game
{
    public static class N32Fixes
    {
        static readonly string[] args = Environment.GetCommandLineArgs();
        static readonly bool off = Array.IndexOf(args, "-skygrindOff32") >= 0 || Array.IndexOf(args, "-skygrindOff31") >= 0 || Array.IndexOf(args, "-skygrindOff30") >= 0 || Array.IndexOf(args, "-skygrindOff29") >= 0;

        /// <summary>Every NIGHT32 view / flow fix is on (not -skygrindOff32).</summary>
        public static bool On => !off;

        /// <summary>A view row's switch: not -skygrindOff32 and the row's own `design.n32.<row>.enabled` (grapplePips, bossMarks, hud32).</summary>
        public static bool ViewRowOn(Skygrind.Core.GameConfig cfg, string row) => !off && cfg != null && cfg.Design("n32." + row + ".enabled");
    }
}
