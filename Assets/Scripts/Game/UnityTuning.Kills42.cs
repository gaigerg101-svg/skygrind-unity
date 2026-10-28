// NIGHT42 builder F1 (design.n42.kills42, a view row: no sim bit; G9 / G15): the render side of the hit / kill feel (Game/KillFeel42.cs): the
// gauntlet's shockwave ring, its camera kick, the kill's bigger burst and harder break. CLAUDE.md rule 5: Unity-only numbers, each with a comment.
// The freezes are gameplay pacing and live in design.n42.impact42 (Core/ImpactRules.cs). Sizes are indexed by GauntletHit42Event class
// (0 a husk-size body, 1 a big kind, 2 a boss) or by KillTier42Event tier (0 unused, 1 plain, 2 heavy, 3 melee, 4 big).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the gauntlet's shockwave ring (camera-facing, at the struck body's aim point, additive, the gauntlet's kinetic cyan: player-usable) ---
        public const int Kills42RingPool = 8; // rings alive at once (one per swing; a swing every 1.8 s lives 0.32 s, so 8 is never reached)
        public static readonly float[] Kills42RingRadius = { 1.8f, 2.6f, 3.8f }; // m the ring grows to, by class: about 2x a husk's height, a tank's body and a half, THE ICEBREAKER's shoulders and more
        public const float Kills42RingStart = 0.25f; // share of the end radius the ring starts at (it reads as leaving the fist, not appearing big)
        public const double Kills42RingTime = 0.32; // s of frame time the ring lives (it grows during the freeze: the world stops, the shock does not)
        public const float Kills42RingWidth = 0.075f; // the ring's band as a share of its radius (n42-r2: 0.22 filled the screen at punch range and hid the enemy; a third of it)
        public const float Kills42RingHdr = 1.6f; // gain on its first frame, falling with (1 - t)^2 (n42-r2: 3.2 read opaque; half, so the body shows through the additive band)
        public const float Kills42RingMaxDeg = 9; // deg: the ring's radius never subtends more than this from the eye (n42-r2: at 1.8 m the boss-size ring covered ~70 % of the screen; 9 deg keeps it ~25 % of a 70 deg view, around the target)
        public const int Kills42RingSegments = 48; // the ring mesh's segments (smooth at 4 m across the screen)
        public const float Kills42SecondRing = 0.6f; // a second, thinner ring at this share of the first one's size, a frame behind (a double shock reads as force)

        // --- the gauntlet's camera kick (after CameraFeel: a pitch-up and a roll that spring back on frame time) -----------------------------------
        public static readonly float[] Kills42KickDeg = { 1.6f, 2.4f, 3.4f }; // deg the view pitches up on the hit, by class (Nuclear Throne's camera kick, hitfeel-web.md number 6, as an angle: 6 px of 240 at a 90 deg view ~ 2.3 deg)
        public const float Kills42KickRoll = 0.45f; // roll as a share of the pitch kick (alternating sides per hit)
        public const double Kills42KickTime = 0.22; // s the kick takes to spring back (ease-out cubic); shorter than the boss freeze, so the frozen frame shows the kick moving
        public static readonly float[] Kills42Trauma = { 0.35f, 0.5f, 0.7f }; // trauma added to the web shake (Core/Trauma.cs, capped by feel.trauma's cap), by class: hitfeel-web.md section 1 shake impulse strong 0.3-0.5, critical 0.6-1.0

        // --- the gauntlet's spark burst (particles: the gauntlet's family colour) ----------------------------------------------------------------
        public static readonly int[] Kills42SparkCount = { 18, 24, 32 }; // sparks by class (hitfeel-web.md: 20-30 particles, 0.5-1 s life)
        public const float Kills42SparkSpeed = 9; // m/s
        public const float Kills42SparkLife = 0.45f; // s
        public const float Kills42SparkScale = 1.6f; // x the particle cube (bigger than a gun's sparks)

        // --- the kill (by KillTier42Event tier): a bigger burst in the enemy colour, a trauma kick, a harder break ---------------------------------
        public static readonly int[] Kills42KillCount = { 0, 8, 14, 20, 30 }; // extra debris particles beside the web's death burst, by tier
        public const float Kills42KillSpeed = 7; // m/s
        public const float Kills42KillLife = 0.6f; // s
        public static readonly float[] Kills42KillTrauma = { 0, 0, 0.12f, 0.2f, 0.35f }; // trauma added beyond feel.trauma.kill, by tier (a plain kill keeps the web's)
        public static readonly float[] Kills42ThrowMult = { 1, 1, 1.25f, 1.45f, 1.6f }; // x the chunks' overkill throw (Chunks.Break throwK), by tier: a crunchier break
        public const float Kills42ThrowMax = 2.6f; // the throw's cap after the multiplier (CrunchOverkillMax is the old cap; blocks must stay in the room)

        // --- the hit flinch by weapon (EnemyViews.Hit.cs: x the hit's tilt / push / squash share and its knock-back; a weapon not listed: x1) --------
        public const float Kills42FlinchRevolver = 1.35f; // the KESTREL: a slow, heavy pistol shot, a visible snap back (hitfeel-web.md: Nuclear Throne knocks a hit enemy 3 px a frame)
        public const float Kills42FlinchShotgun = 1.7f; // the THUNDERCLAP: a volley in one body throws its top back hardest of the guns
        public const float Kills42FlinchNail = 0.8f; // the STITCHER: ~10 nails a second, a smaller flinch each so the stream holds the body back instead of shaking it
        public const float Kills42FlinchGauntlet = 2.2f; // the PULSE gauntlet: the strongest flinch of all (G15), on top of its melee tilt
        public const float Kills42TiltScaleMax = 3.2f; // the tilt share's cap after the multiplier (CrunchTiltMelee x gauntlet stays a lean, not a fold)
        public const float Kills42KnockMax = 1.2f; // m: the knock-back's cap after the multiplier (a body never leaves its spot by more)
    }
}
