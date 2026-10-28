// NIGHT36 builder P (tells36 / parry36 / launch36 / heal36): Unity-only render numbers of the tell flash (Game/Tell36View.cs) and of
// the core loop's feedback. Shared with builder B (Boss36View draws its tells in these colours). No gameplay number lives here:
// wind-ups, parry windows, launch and heal numbers are design.n36.<row> in Design/unity-config.json.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The two tell colours (CLAUDE.md rule 8: red-orange is enemies and danger; yellow is new tonight and means "punch it").
        public const uint Tell36YellowHex = 0xFFD23C; // a warm parry yellow: brighter and greener than the enemies' red-orange, so a parryable wind-up reads apart from the body's own colour at a glance
        public const uint Tell36RedHex = 0xFF3A14; // the danger red-orange, pushed hotter than the enemy bodies' (dodge only)
        public const float Tell36FlashPeak = 2.4f; // HDR multiplier of the flash colour at its peak (the bloom picks it up; 1 = plain colour)
        public const float Tell36FlashStart = 0.35f; // share of the peak the flash starts at when the wind-up begins (it rises to the peak as the attack nears)
        public const float Tell36PulseHz = 7f; // the flash's pulse in its last 30 % (the "now" cue, ULTRAKILL's flash pop)
        public const float Tell36HaloScale = 1.25f; // the flash halo's size x the body's radius x 2
        public const float Tell36GlobGlowScale = 2.2f; // a parryable projectile's yellow glow sphere x its radius x 2
        public const float Tell36ParryFlashS = 0.18f; // s: the white-yellow screen flash of a parry36 (its freeze is the sim's hitstop)
        public const float Tell36BurstS = 0.35f; // s: a launch36 burst's expanding sphere
        public const float Tell36HealNumberS = 0.9f; // s: a heal36 "+10" number rising from the kill
        public const float Tell36HealNumberRise = 1.2f; // m the heal number rises over its life
        public const float Tell36HealNumberSize = 7f; // TextMeshPro (world) font size of the heal number: about 0.7 m tall, readable at the 4 m heal radius
        public const uint Tell36HealHex = 0x3CF0FF; // the heal number and its spark: cyan (rule 8: player-usable)
        public const float Tell36HaloAlpha = 0.55f; // the tell halo's additive strength at the flash's peak (x the colour)
        public const int Tell36ParryBits = 26; // spark bits of a parry36 at the parry point
        public const float Tell36ParryBitSpeed = 7f; // m/s of those bits
        public const float Tell36ParryBitLife = 0.35f; // s
        public const float Tell36ParryBitSize = 0.11f; // m
        public const float Tell36ParryRingR = 1.6f; // m: the parry's yellow shell at full size
        public const int Tell36BurstBits = 40; // spark bits of a launch36 / parry36 burst
        public const float Tell36BurstBitSpeed = 11f; // m/s
        public const float Tell36BurstBitLife = 0.45f; // s
        public const float Tell36BurstBitSize = 0.16f; // m
        public const float Tell36BurstAlpha = 0.9f; // the burst shell's additive strength at its start
        // The tell POSE (applied on top of the enemy views' own transforms each frame, pivot at the feet): a wind-up rears back and grows,
        // the last Tell36PoseStrikeAt of it snaps forward (the strike), and the pose eases out after the attack at Tell36PoseEase per s.
        public const float Tell36PoseHuskBack = 0.5f; // rad (~29 deg) a husk rears back at the wind-up's peak: reads as a lean at 10 m (the body is 1.8 m: the head moves ~0.9 m)
        public const float Tell36PoseHuskGrow = 0.16f; // a husk stands this much taller at the peak (scale y x 1.16, x/z x 0.94)
        public const float Tell36PoseSpitterBack = 0.38f; // rad a spitter tilts its head back
        public const float Tell36PoseSpitterSac = 0.6f; // a spitter's back sac swells by this share (x 1.6)
        public const float Tell36PoseSwarmerCrouch = 0.4f; // a swarmer squats this share lower (scale y x 0.6, x/z x 1.25)
        public const float Tell36PoseStrike = 0.45f; // rad forward at the strike (husk; a spitter's lob nods half as much)
        public const float Tell36PoseStrikeAt = 0.88f; // share of the wind-up where the strike starts
        public const float Tell36PoseRiseAt = 0.4f; // share of the wind-up by which the rear-back is full (smoothstep; n36-r3: 0.6 read 0.31 on the half-way frame)
        public const float Tell36PoseEase = 10f; // 1/s: the pose eases out after the attack at this rate
        public const float Tell36PoseEaseIn = 30f; // 1/s: the pose follows its rising target this fast during the wind-up (n36-r3: 10 lagged it to 0.31 by the half-way frame)
        public const uint Tell36BurstHex =0xFFB040; // a launch / return burst: a hot orange-white blast (the yellow parry's payoff, warmer; never the boundary's violet)
    }
}
