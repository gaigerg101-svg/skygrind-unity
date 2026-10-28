// NIGHT36: view / flow-only switches (no sim effect, so no tape or fingerprint change). `-skygrindOff36` (or any older off switch from
// -skygrindOff29) turns them all off: the RED half of each view proof is tonight's build run with -skygrindOff36.
using System;

namespace Skygrind.Game
{
    public static class N36Fixes
    {
        static readonly string[] args = Environment.GetCommandLineArgs();
        static readonly bool off = Off(args);

        static bool Off(string[] a)
        {
            for (int n = 29; n <= 36; n++)
                if (Array.IndexOf(a, "-skygrindOff" + n) >= 0) return true;
            return false;
        }

        /// <summary>Every NIGHT36 view / flow fix is on (not -skygrindOff36).</summary>
        public static bool On => !off;

        /// <summary>A view row's switch: not -skygrindOff36 and the row's own `design.n36.<row>.enabled` (muzzle36, fist36, text36,
        /// bossmodel36, level36).</summary>
        public static bool ViewRowOn(Skygrind.Core.GameConfig cfg, string row) => !off && cfg != null && cfg.Design("n36." + row + ".enabled");
    }
}
