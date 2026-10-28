// NIGHT34: view / flow-only switches (no sim effect, so no tape or fingerprint change). `-skygrindOff34` (or any older off switch from
// -skygrindOff29) turns them all off: the RED half of each view proof is tonight's build run with -skygrindOff34.
using System;

namespace Skygrind.Game
{
    public static class N34Fixes
    {
        static readonly string[] args = Environment.GetCommandLineArgs();
        static readonly bool off = Array.IndexOf(args, "-skygrindOff34") >= 0 || Array.IndexOf(args, "-skygrindOff33") >= 0 || Array.IndexOf(args, "-skygrindOff32") >= 0 || Array.IndexOf(args, "-skygrindOff31") >= 0 || Array.IndexOf(args, "-skygrindOff30") >= 0 || Array.IndexOf(args, "-skygrindOff29") >= 0;

        /// <summary>Every NIGHT34 view / flow fix is on (not -skygrindOff34).</summary>
        public static bool On => !off;

        /// <summary>A view row's switch: not -skygrindOff34 and the row's own `design.n34.<row>.enabled` (bendLook, rail34, nomark34,
        /// calm34, lines34).</summary>
        public static bool ViewRowOn(Skygrind.Core.GameConfig cfg, string row) => !off && cfg != null && cfg.Design("n34." + row + ".enabled");
    }
}
