// NIGHT60 builder L (B9, T-138): the Player focus-pause probe's numbers (FocusProbe60.cs, -skygrindFocusProbe60). Unity-only: a test
// harness's clock and log budget, no gameplay (the web has no focus probe, so no config number exists for them).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // seconds the probe watches after its ready line before it prints DONE and quits: Tools/night/n60-focus-probe.sh plants its steal
        // 2 s after the ready line and holds it 3 s (n56-foreground.ps1 -HoldS 3), so the pause and the focus coming back both land inside
        public const float FocusProbe60Seconds = 20f;

        // the most seconds the probe waits, from its start, for the run to be Playing with the cursor locked and the window focused (the
        // script brings the Player's window to the front first); past it the probe prints "not ready", DONE False, and quits
        public const float FocusProbe60ReadySeconds = 60f;

        // flow / cursor / focus change lines the probe prints (the rest are counted in its summary): a window that keeps losing and taking
        // the focus must not flood the Player log
        public const int FocusProbe60MaxLines = 60;

        // the probe's run seed: SliceSmoke.Start's (its first StartRun), so the probe's run is the smoke's first run
        public const uint FocusProbe60Seed = 424242;

        // the probe's god mode: SliceSmoke.Start's health (max and hp) until its death step; an idle player in round 1 must stay Playing,
        // since only a Playing run pauses on the focus loss
        public const double FocusProbe60GodHealth = 1e6;
    }
}
