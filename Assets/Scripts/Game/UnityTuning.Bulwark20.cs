// NIGHT20 U3-34.9 A1 block 2 (builder EN owns this file): the BULWARK's look, bulwark.js's render numbers as it draws them
// (GG's design, rebuilt as it is; CLAUDE.md rule 7). Visual only: the sim never reads them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>bulwark.js: the hull 0x3a3f4a (Lambert) with its red-orange rim 0xff7a30 (the enemy's rim: EnemyLook's), the
        /// plates 0x9aa3b0 (white x3 for 0.08 s when hit), the vent core 0xff7a20 x1.2 (open: white x (2.6 + 0.6 sin(14 age)));
        /// the eye strip 1.1 x 0.18 x 0.05 at (0, 6.9, -1.02) in TELEGRAPH.danger x2.5, white while it winds an attack up.</summary>
        public const uint BulwarkHullHex = 0x3a3f4a, BulwarkPlateHex = 0x9aa3b0, BulwarkCoreHex = 0xff7a20;
        public const float BulwarkPlateFlash = 3f, BulwarkCoreShut = 1.2f, BulwarkCoreOpen = 2.6f, BulwarkCorePulse = 0.6f, BulwarkCorePulseRate = 14f;
        public const float BulwarkEyeW = 1.1f, BulwarkEyeH = 0.18f, BulwarkEyeD = 0.05f, BulwarkEyeY = 6.9f, BulwarkEyeZ = -1.02f, BulwarkEyeGain = 2.5f;
        /// <summary>bulwark.js render: the legs swing 0.35 m by sin(walk x 1.2); the ram's lean drops the upper body 0.7 m and
        /// pushes it 0.5 m ahead; the stun drops it 0.9 m and back 0.4 m; the back plate swings up 1.4 m while the vent is open.</summary>
        public const float BulwarkLeanDrop = 0.7f, BulwarkLeanAhead = 0.5f, BulwarkStunDrop = 0.9f, BulwarkStunBack = 0.4f, BulwarkVentLift = 1.4f;
        /// <summary>The stomp's shock ring: an open cylinder of 56 sides, 0xff7a30 x2, additive.</summary>
        /// <summary>The sweep beam: a 0.3 x 0.3 bar from it out to beam.range, 0xff3a10 x1.2 and 0.4 thick while it charges, x4
        /// while it sweeps.</summary>
        // NIGHT20 (the lead): BulwarkRingHex / Gain / Sides, BulwarkBeamHex and BulwarkBeamThick live in UnityTuning.Fx20.cs (builder HV draws the ring and the beam; the same values: a CS0102 clash at integration)
        public const float BulwarkBeamCharge = 0.4f, BulwarkBeamChargeGain = 1.2f, BulwarkBeamGain = 4f;
        /// <summary>particles.burst: a broken plate's bits (0x9aa3b0, 26, 9 m/s, 0.7 s, 1.6); the wall's and the stomp's slam dust
        /// (colors.slamDust, 30 / 36, 10 / 12 m/s, 0.6 s, 2); its death at the core (white, 70, 16 m/s, 1 s, 2.4).</summary>
        public const int BulwarkPlateBits = 26, BulwarkWallDust = 30, BulwarkStompDust = 36, BulwarkDeathBits = 70;
        public const float BulwarkPlateBitSpeed = 9f, BulwarkPlateBitLife = 0.7f, BulwarkPlateBitSize = 1.6f;
        public const float BulwarkWallDustSpeed = 10f, BulwarkStompDustSpeed = 12f, BulwarkDustLife = 0.6f, BulwarkDustSize = 2f;
        public const float BulwarkDeathSpeed = 16f, BulwarkDeathLife = 1f, BulwarkDeathSize = 2.4f;
    }
}
