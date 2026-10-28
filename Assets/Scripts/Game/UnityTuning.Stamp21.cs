// NIGHT21 U3-35.5 THE STAMP (builder S): Unity-only layout numbers for the round strip on RESULTS' PAYOFF panel (UiPayoff.cs).
// UNCOMPILED when written (no Editor that night). The grade's own numbers are design.n21.stamp (Design/unity-config.json).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The round strip: this many rounds' grades a row ("1 B   2 D   3 A   4 A   5 S" is about 30 characters at the strip's
        // 16 px small size, inside PayoffWidth minus its padding), this many rows at most (the last rounds when a run is long).
        public const int PayoffRoundsPerRow = 5;
        public const int PayoffRoundsRows = 3;
    }
}
