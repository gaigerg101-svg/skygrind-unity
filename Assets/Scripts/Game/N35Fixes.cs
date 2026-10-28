// NIGHT35: view / flow-only switches (no sim effect, so no tape or fingerprint change). `-skygrindOff35` (or any older off switch from
// -skygrindOff29) turns them all off: the RED half of each view proof is tonight's build run with -skygrindOff35.
using System;

namespace Skygrind.Game
{
    public static class N35Fixes
    {
        static readonly string[] args = Environment.GetCommandLineArgs();
        static readonly bool off = Off(args);

        static bool Off(string[] a)
        {
            for (int n = 29; n <= 35; n++)
                if (Array.IndexOf(a, "-skygrindOff" + n) >= 0) return true;
            return false;
        }

        /// <summary>Every NIGHT35 view / flow fix is on (not -skygrindOff35).</summary>
        public static bool On => !off;

        /// <summary>A view row's switch: not -skygrindOff35 and the row's own `design.n35.<row>.enabled` (pickup35, look35, lights35, tv35,
        /// ridelook35, campaign6).</summary>
        public static bool ViewRowOn(Skygrind.Core.GameConfig cfg, string row) => !off && cfg != null && cfg.Design("n35." + row + ".enabled");
    }
}
