// NIGHT24 shotgun-rate (builder R; design.n24.shotgunTune): the THUNDERCLAP fires a little slower, not weaker. GG 2026-10-08: "the shotgun is not
// balanced it just fires a little too fast". MEASURED first (SimHarness shotgun, Harness.N24.Shotgun.cs): the shot-to-shot time today is 0.408 s
// (49 steps), not the web's 0.9 s: NIGHT20's retune (design.arsenal.n20.tune.shotgun: interval x0.45, damage x2.5) shortened it. The row
// multiplies that by design.n24.shotgunTune.intervalMult (1.30, DECIDE D-8) and scales the pellets' damage by damageMult (1.30) so the damage per
// second stays the same (slower, not weaker). A sim change when on (the fingerprint path); off (--off24, the row off, a tape without the bit):
// both multipliers are exactly 1 and the loadout is NIGHT22's bit for bit. Called at every run's start from SurvivalSim.N24StartRun.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        partial void N24ShotgunStartRun()
        {
            bool on = n24.shotgunTune;
            loadout.sgIntervalMult24 = on ? cfg.Num("design.n24.shotgunTune.intervalMult") : 1;
            loadout.sgDamageMult24 = on ? cfg.Num("design.n24.shotgunTune.damageMult") : 1;
        }
    }
}
