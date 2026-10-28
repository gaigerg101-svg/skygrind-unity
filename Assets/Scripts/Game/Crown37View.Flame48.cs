// NIGHT48 builder L (flame48, a VIEW row: NightRows.ViewOn(cfg, "flame48"); NIGHT48.md G43, the boss part; DECIDE48 D-30): THE CROWN's CROWN
// FIRE drawn as the PARTICLE CONE (Crown37View.FlameVariant47 1: half the puffs at 1.4x their size, no core streaks, no floor pool), the look
// that won the a548 blind pair 3 to 0 over NIGHT47's mesh stream (FlameVariant47 0). NIGHT47's files are not edited: Crown37View.N47.cs
// still reads FlameVariant47; this file only chooses it, once a frame before Crown47Draw (one hook line in Crown37View.Crown.cs).
// Row off (or -skygrindOff48): the variant is never touched here, NIGHT47's flame (0) stays. A reel may still set FlameVariant47 itself
// with the row off; Flame48Force is the reel's switch for the row (-1 the config, 0 off, 1 on).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        /// <summary>Reel-only (FeelReel.N48.Crown.cs): -1 the config's flame48, 0 off, 1 on.</summary>
        public static int Flame48Force = -1;

        /// <summary>For the reels: flame48 chose the particle cone this frame.</summary>
        public static bool Flame48Shown;

        bool flame48Set;

        /// <summary>Every frame of a live Crown, just before Crown47Draw: the row on sets the particle cone; turning off gives back NIGHT47's.</summary>
        void Flame48Select()
        {
            bool on = Flame48Force >= 0 ? Flame48Force == 1 : NightRows.ViewOn(game.cfg, "flame48");
            Flame48Shown = on;
            if (on) { FlameVariant47 = 1; flame48Set = true; }
            else if (flame48Set) { FlameVariant47 = 0; flame48Set = false; } // only undo what this row set
        }
    }
}
