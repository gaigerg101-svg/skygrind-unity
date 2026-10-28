// NIGHT45 (the shared commit): the NIGHT45 view row's switch (look45). On when design.n45.look45 is enabled and the session is not
// -skygrindOff45 (or any older off switch, or N45Levels.ForceOff). The command line is read here directly, once (NIGHT40's pump40 lesson).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class N45View
    {
        static int cliOff = -1;

        public static bool Off
        {
            get
            {
                if (N45Levels.ForceOff) return true;
                if (cliOff < 0)
                {
                    cliOff = 0;
                    try
                    {
                        foreach (string a in Environment.GetCommandLineArgs())
                            for (int n = 19; n <= 45; n++)
                                if (a == "-skygrindOff" + n) cliOff = 1;
                    }
                    catch (Exception) { cliOff = 0; }
                }
                return cliOff == 1;
            }
        }

        public static bool RowOn(GameConfig cfg, string row) => cfg != null && !Off && cfg.Design("n45." + row + ".enabled");
    }
}
