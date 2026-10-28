// NIGHT43 (the shared commit, the lead): the NIGHT43 view rows' switch. A view row (hud43, counter43, teach43, screens43, bossbar43,
// feel43, models43, look43) changes only what is drawn (no sim bit): it is on when its design.n43 row is enabled and the session is not
// `-skygrindOff43` (or any older off switch). The command line is read here directly, once, because some views are built before
// PlayerChecks sets N43Blocks.ForceOff (NIGHT40's pump40 lesson, 57a56d52).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class N43View
    {
        static int cliOff = -1; // -1 unread, 0 no, 1 an off switch on the command line

        /// <summary>Is NIGHT43 off in this session (-skygrindOff43 or any older -skygrindOffNN, or N43Blocks.ForceOff)?</summary>
        public static bool Off
        {
            get
            {
                if (N43Blocks.ForceOff) return true;
                if (cliOff < 0)
                {
                    cliOff = 0;
                    try
                    {
                        foreach (string a in Environment.GetCommandLineArgs())
                            for (int n = 19; n <= 43; n++)
                                if (a == "-skygrindOff" + n) cliOff = 1;
                    }
                    catch (Exception) { cliOff = 0; }
                }
                return cliOff == 1;
            }
        }

        /// <summary>A design.n43 view row on in this session.</summary>
        public static bool RowOn(GameConfig cfg, string row) => cfg != null && !Off && cfg.Design("n43." + row + ".enabled");
    }
}
