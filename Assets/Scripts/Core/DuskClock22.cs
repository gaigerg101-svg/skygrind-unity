// NIGHT22 U3-36.8 THE DUSK CLOCK (the lead; the pure rules, tested in EditMode): the sky sinks per round. The share is 0 at round 1
// (the dusk as shipped) and 1 at design.n22.duskClock.lastRound (15); the shown share eases toward it over easeSeconds of frame
// time. The views (Game/DuskClock22.cs) lean the sky's three colours, the stone's sky fog and the URP fog toward the config's
// round-15 colours (violet and blue-violet: never magenta, never red-orange).
namespace Skygrind.Core
{
    public static class DuskClockMath
    {
        /// <summary>The target share for a round: 0 at round 1 and below, 1 at lastRound and above, linear between.</summary>
        public static double Target(int round, int lastRound)
        {
            if (lastRound <= 1 || round <= 1) return 0;
            if (round >= lastRound) return 1;
            return (round - 1) / (double)(lastRound - 1);
        }

        /// <summary>One frame of the ease (linear, easeSeconds for the whole 0..1 span) toward `target`.</summary>
        public static double Step(double shown, double target, double dt, double easeSeconds)
        {
            if (!(dt > 0)) return shown;
            double d = easeSeconds > 0 ? dt / easeSeconds : 1;
            if (target > shown) return System.Math.Min(target, shown + d);
            if (target < shown) return System.Math.Max(target, shown - d);
            return shown;
        }
    }
}
