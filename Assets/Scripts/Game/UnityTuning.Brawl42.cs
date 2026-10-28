// NIGHT42 builder B1 (brawl42): THE ICEBREAKER's new attacks drawn (Brawl40View.N42.cs). Render numbers only; colours are Brawl40's (rule 8:
// red-orange = danger, cyan = the punish).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The sweep's wind-up: the crouch (m, at the tell's end), the lean forward (deg) and the head's turn down (deg).</summary>
        public const float Brawl42SweepCrouch = 0.7f, Brawl42SweepLean = 32f, Brawl42SweepHead = 12f;
        /// <summary>The sweep's reach ring on the ice through the wind-up: height above the ice (m), width (m), thickness (m), gain from
        /// Brawl40TellMin to this at the tell's end (red-orange: where the arm will pass).</summary>
        public const float Brawl42ReachY = 0.03f, Brawl42ReachWidth = 0.3f, Brawl42ReachThick = 0.04f, Brawl42ReachGain = 1.6f;
        /// <summary>The spinning arm's blade: a red-orange bar from the body's surface to the reach at shin height (m up), its thickness (m) and
        /// gain (a bright additive bar, so the arm's sweep reads even when the fist is behind the body).</summary>
        public const float Brawl42BladeY = 0.35f, Brawl42BladeThick = 0.35f, Brawl42BladeGain = 2.2f;
        /// <summary>Dizzy after the sweep: the head's roll (deg) and how fast it wobbles (radians a second).</summary>
        public const float Brawl42DizzyRoll = 18f, Brawl42DizzyRate = 9f;
        /// <summary>The leap's wind-up crouch (m) and lean (deg).</summary>
        public const float Brawl42LeapCrouch = 0.8f, Brawl42LeapLean = 20f;
        /// <summary>The landing circle on the ice: height (m), width (m), thickness (m); gain while it tracks (from Brawl40TellMin up to the first
        /// value) and once LOCKED (the second, brighter: the cue to leave it). A second ring inside at this share of the radius fills the read.</summary>
        public const float Brawl42CircleY = 0.04f, Brawl42CircleWidth = 0.4f, Brawl42CircleThick = 0.05f, Brawl42CircleGain = 1.1f, Brawl42CircleLockGain = 2.4f, Brawl42CircleInner = 0.55f;
        /// <summary>n42-r1 iteration (the lead's frames: the circle read AMBER over the warm ice, thin; the low arm did not read): the danger red
        /// for the new attacks' glows (less green than Brawl40DangerHex so additive over the lit ice stays red, not amber), the ring's width (m).</summary>
        public const uint Brawl42RedHex = 0xff1e0a;
        public const float Brawl42CircleWidthR1 = 0.7f;
        /// <summary>The landing disc: an OPAQUE unlit red-orange disc filling the circle (unlit, so the warm room light cannot turn it amber), at
        /// this height (m); its gain while tracking (dark red, growing from the centre with the wind-up) and once LOCKED (full radius, bright,
        /// pulsing by the second value at the rate, radians a second).</summary>
        public const float Brawl42DiscY = 0.02f, Brawl42DiscTrackGain = 0.45f, Brawl42DiscLockGain = 1.0f, Brawl42DiscPulse = 0.35f, Brawl42DiscRate = 14f;
        /// <summary>The sweep's shin band: a red-orange band of ring segments at the arm's height round its reach through the wind-up: centre
        /// height (m), band height (m), thickness (m), gain at the tell's end.</summary>
        public const float Brawl42ShinY = 0.35f, Brawl42ShinHeight = 0.28f, Brawl42ShinThick = 0.08f, Brawl42ShinGain = 1.5f;
        /// <summary>F1's gauntletHit42 on THE ICEBREAKER (Brawl40View.N42.Hit.cs): ice chips (count, size m, speed away from the player m/s, the
        /// upward kick m/s, the spread m/s, gravity m/s2, life s, gain; pale-blue frost, not cyan), the chest crack's flash (seconds, gain), the
        /// recoil (metres back along the hit at the hit, degrees the torso rocks back), easing out over the crack's seconds.</summary>
        public const int Brawl42ChipCount = 10;
        public const float Brawl42ChipSize = 0.32f, Brawl42ChipSpeed = 7f, Brawl42ChipUp = 5f, Brawl42ChipSpread = 3.5f, Brawl42ChipGravity = 22f, Brawl42ChipLife = 0.6f, Brawl42ChipGain = 1.6f;
        public const float Brawl42CrackSeconds = 0.28f, Brawl42CrackGain = 2.6f, Brawl42RecoilMetres = 0.45f, Brawl42RecoilDeg = 16f;
        /// <summary>The leap in the air: the body tucks (lean deg, knee drop m).</summary>
        public const float Brawl42AirLean = -10f, Brawl42AirTuck = 0.4f;
    }
}
