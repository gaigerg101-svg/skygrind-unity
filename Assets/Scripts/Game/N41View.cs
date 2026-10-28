// NIGHT41 (the shared commit, the lead): the NIGHT41 view rows' switch. A view row (juice41, juice41On, doors41On, pump41On, bars41,
// entrance41, select41, look41, brawlLook41, models41) changes only what is drawn (no sim bit): it is on when its design.n41 row is
// enabled and the session is not `-skygrindOff41` (or any older off switch). The command line is read here directly, once, because some
// views are built before PlayerChecks sets N41Blocks.ForceOff (NIGHT40's pump40 lesson, 57a56d52).
// The old rows NIGHT41 turns on are turned on through these rows, never in place (NIGHT41.md G0; lean_rules 2): juice38 reads
// `design.n38.juice38 || juice41On`, doors38 `design.n38.doors38 || doors41On`, pump40 `design.n40.pump40 || pump41On`, so `--off41` is
// NIGHT40's game.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class N41View
    {
        static int cliOff = -1; // -1 unread, 0 no, 1 an off switch on the command line

        /// <summary>Set by J2's UiBossBar41 while it draws the boss's bar (bars41 on): the HUD33 boss bar (UiHud33.Boss33) and the Boss37
        /// stagger bar (Boss37StaggerBar.Show) hide while it is true (the lead's hooks), so one bar shows, never two.</summary>
        public static bool BossBar41Live;

        /// <summary>Is NIGHT41 off in this session (-skygrindOff41 or any older -skygrindOffNN, or N41Blocks.ForceOff)?</summary>
        public static bool Off
        {
            get
            {
                if (N41Blocks.ForceOff) return true;
                if (cliOff < 0)
                {
                    cliOff = 0;
                    try
                    {
                        foreach (string a in Environment.GetCommandLineArgs())
                            for (int n = 19; n <= 41; n++)
                                if (a == "-skygrindOff" + n) cliOff = 1;
                    }
                    catch (Exception) { cliOff = 0; }
                }
                return cliOff == 1;
            }
        }

        /// <summary>A design.n41 view row on in this session.</summary>
        public static bool RowOn(GameConfig cfg, string row) => cfg != null && !Off && cfg.Design("n41." + row + ".enabled");
    }
}
