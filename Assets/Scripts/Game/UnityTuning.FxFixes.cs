// NIGHT16 polish #41 / #44 (builder V): numbers the web keeps as literals in its call sites (CLAUDE.md rule 5).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // fx/impactFx.js :63 particles.burst(..., fam.kinetic, Math.round(18 * k), 8, 0.35, 1.1): the ring's burst cube scale
        public const float ImpactRingBurstScale = 1.1f;
        // polish #58: the OVERLOAD's stand-in picture (the web draws its lightning arcs, gunFx.js :96-102, which give no burst
        // numbers): the nearest web burst, the STITCHER detonate's volt burst (projectiles.js :306 particles.burst(p2,
        // fam.volt, 6, 6, 0.25, 0.8)), at the struck target and at each enemy the burst reaches
        public const int OverloadBurstCount = 6;
        public const float OverloadBurstSpeed = 6, OverloadBurstLife = 0.25f, OverloadBurstScale = 0.8f;

        public const int ImpactRingBurstCount = 18; // impactFx.js :63 Math.round(18 * k) (x the preset's impact detail; polish #51)
        public const float ImpactRingBurstSpeed = 8, ImpactRingBurstLife = 0.35f; // impactFx.js :63 the burst's speed and life

        // polish #41: the particles' draw bounds pad by the largest cube scale any burst passes (Projectiles' / Traps' 1.6)
        // so a big cube at the edge of the cloud is never culled; render-only

        // polish #44: a THUNDERCLAP shotResult whose end lies this close to the shotgun's range (m) is the "nothing hit" end
        // (Combat.cs fireShotgun: firstEnd = origin + aim x sgRange); a numeric tolerance, not a gameplay number
        public const double ShotgunMissTolerance = 1e-6;
    }
}
