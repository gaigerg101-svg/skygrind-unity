// NIGHT39 builder F1: Unity-only view numbers of slow39's screen cue and the parry's slowed streak (CLAUDE.md rule 5: render numbers live in
// UnityTuning, each with a comment). Read only with design.n39.slow39 on; off, the cue keeps design.n27.slowAim27.cue* and the streak UnityTuning.Parry31*.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The slow cue (SlowAim27.cs's Volume, blended by SlowAim01 = slow39's depth): stronger than NIGHT27's 0.25 / 0.15 / -15 so a still frame
        // shows the 0.3 world share ("a still frame must show it", NIGHT39.md F1). Starting values, tuned on the lead's Player frames.
        public const float Slow39CueVignette = 0.38f; // vignette intensity added at full slow (NIGHT27: 0.25)
        public const float Slow39CueChroma = 0.3f; // chromatic aberration added at full slow (NIGHT27: 0.15); URP's is edge-weighted, the centre stays clean for aim
        public const float Slow39CueSaturation = -35f; // saturation added at full slow (NIGHT27: -15): the world drains while the cyan HUD / trims keep their colour
        public const float Slow39CueFovDeg = 4f; // extra vertical FOV at full slow (NIGHT27's +4 kept: a bigger add fights the zoom GG is holding)

        // The parry's returned projectile (Parry31View) with slow39 on: the streak ages on WORLD time (game.WorldDt), so it holds through the 0.25 s
        // freeze and crawls through the parry's slow window: "a frame where you see it turn" (NIGHT39.md F1).
        public const float Parry39StreakSeconds = 0.3f; // world s: the streak's flight to the boss (NIGHT31: 0.15 real s); at the window's 0.25 share about 1 real s
        public const float Parry39StreakStartU = 0.12f; // share of the path already drawn on the parry's frame, so the frozen frame shows the turn (a 0-length line is invisible)
        public const float Parry39StreakWidth = 0.16f; // m: twice NIGHT31's 0.08 so the returned shot reads as heavy
        public const float Parry39StreakBrightness = 3.5f; // x colors.usable (cyan: player-usable) on the additive neon (NIGHT31: 2.5)
    }
}
