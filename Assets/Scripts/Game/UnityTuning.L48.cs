// NIGHT48 builder L: loomHull48's render numbers (Loom37View.Hull48.cs; G36 / G37). Render only: the sim never reads these.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The long axis: the shuttle lies across the well (world x, the legs' axis: a loom's shuttle flies across the warp) and turns
        /// toward the player by this share of the angle, capped (deg), on a spring (Hz, critically damped): broadside from the door and every post.</summary>
        public const float Hull48YawFollow = 0.35f, Hull48YawMaxDeg = 20f, Hull48YawHz = 0.6f;

        /// <summary>The spindle: its length x Loom37BodyL (10.8 m: the NIGHT47 hull was 10.4), the straight middle's share of the half
        /// length, the taper's power (over 1: full, then a long point), its rings (the half length's shares, 0 = the middle, 1 = the point),
        /// and where the brass tip caps begin (a real fly shuttle's metal-capped points: Research/2026-10-19-wave48/L.md).</summary>
        /// Round 4 (the G36 blind pair, n48 won 3-0 but two judges wrote "a flat saucer, not a long shuttle"; the lead: the long axis at least
        /// 2.5 x its width, the points at both ends): 1.35 -> 1.7 x the length (13.6 m), the width Hull48WidthMul x Loom37BodyW (2.69 m; was
        /// Look47BodyNarrow 0.6 = 3.84 m), the height Hull48HeightMul x Loom37BodyH (2.4 m; was 3.0): length / width 2.8 -> 5.1, length /
        /// height 3.6 -> 5.7; the straight middle 0.35 -> 0.5 of the half length and the taper 1.6 -> 1.0 (straight cones: sharp points, no
        /// bulb); the rings moved to the new taper's start.
        public const float Hull48LenMul = 1.7f, Hull48WidthMul = 0.42f, Hull48HeightMul = 0.8f, Hull48FlatShare = 0.5f, Hull48TaperPow = 1.0f, Hull48CapShare = 0.85f;
        public static readonly float[] Hull48Rings = { 0f, 0.5f, 0.62f, 0.74f, 0.86f, 0.94f, 1f };

        /// <summary>Colours (vertex colours: SKYGRIND/Stone reads no _BaseColor): the bronze body (lighter than NIGHT47's 0x5a4126, which read
        /// near black from below), its phase-2 hot tone, the brass of the caps / hub / claws, the dark bronze upper legs, the blackened-iron
        /// lower legs and brows, the dark bronze spinneret (it drew white: a primitive sphere has no vertex colours).</summary>
        public const uint Hull48BronzeHex = 0x9c6a38, Hull48BronzeHotHex = 0xb0602c, // round 3: 0x7a5230 / 0x96542a drew dark grey from below (n48-r2)
 Hull48BrassHex = 0xc8963a, Hull48LegUpHex = 0x4a3424, Hull48LegLowHex = 0x2e2729, Hull48SpinHex = 0x3e2c20;

        /// <summary>The under-light (3 m under the body: the belly the player looks at) in the lamps' amber at this intensity (x LightScale;
        /// NIGHT37's pale one was 2.2).</summary>
        public const float Hull48UnderIntensity = 2.6f;

        /// <summary>The brass bands: at +-this share of the half length (and the middle), this much over the spindle's profile there.</summary>
        public const float Hull48BandAt = 0.6f, Hull48BandGrow = 1.06f;

        /// <summary>THE HUB: a brass drum through the middle, its face on the player's flank: its radius x the body's height (over 0.5: it shows
        /// above and under the hull), its length x the body's width (over 1: it stands out of both flanks); the amber thread band round it
        /// (radius x, length share).</summary>
        public const float Hull48HubR = 0.38f, Hull48HubL = 1.08f, Hull48ThreadGrow = 1.05f, Hull48ThreadShare = 0.5f; // round 3: 0.58 / 1.18 read as "a brass sphere" (n48-r2): a 2.3 m boss, not a 3.5 m disc

        /// <summary>Round 3, THE OUTLINE (the lead's n48-r2 look: "a brass sphere with radiating legs"; the bronze was dark on the dark roof from
        /// below): amber glow lines along the spindle's ridge, keel and near edges point to point (gain, this x the profile out, thickness m)
        /// and a lamp on each point (m, gain x the line's).</summary>
        public const float Hull48EdgeGain = 2.2f, Hull48EdgeOut = 1.03f, Hull48EdgeThick = 0.14f, Hull48TipLampM = 0.6f, Hull48TipGain = 1.5f;

        /// <summary>THE EYES: the two amber lamps at +-this share of the half length (clear of the hub's face), this far out of the flank (m),
        /// this share of the height up, this big (m; NIGHT47's 0.9); the angular iron brow over each (its half length m, the outer end's and the
        /// inner end's height over the eye m: the inner end low, the visor language of G13, its thickness m).</summary>
        public const float Hull48EyeAt = 0.45f, Hull48EyeOut = 0.15f, Hull48EyeUp = 0.1f, Hull48EyeM = 0.7f; // round 3: 1.0 bloomed into blobs beside the hub (n48-r2)
        public const float Hull48BrowHalf = 0.75f, Hull48BrowOuterUp = 0.85f, Hull48BrowInnerUp = 0.45f, Hull48BrowThick = 0.28f;

        /// <summary>The drawn hips: inside the flank by this share of the width, this share of the height under the middle; the brass claw on
        /// each foot: this long (m), this x the leg's thickness.</summary>
        public const float Hull48HipIn = 0.4f, Hull48HipDown = 0.15f, Hull48ClawM = 1.4f, Hull48ClawThick = 1.35f;

        /// <summary>The anticipation REAR: the top tips away from the player by this (deg) over this share of the tell (L-cards.md: the first
        /// 40 %), the strike's forward tip (deg) and its kick (deg/s: past rest, then the settle), the tilt spring (Hz, damping under 1).</summary>
        public const float Hull48RearDeg = 12f, Hull48RearShare = 0.4f, Hull48StrikeDeg = 6f, Hull48StrikeKickDegS = 220f, Hull48TiltHz = 2.4f, Hull48TiltZeta = 0.45f;

        /// <summary>The hit FLINCH: a kick (deg/s) at a hit of RefDamage, scaled by the hit's damage between Min and MaxScale (a THUNDERCLAP's
        /// pellets are one kick: the velocity is set to at least it, never summed), a fast spring (6 Hz, damping 0.4: ~33 ms to a ~6.8 deg peak
        /// at 25 damage, the out-and-back half cycle ~91 ms: NIGHT47 section 3's 60-100 ms), capped (deg).</summary>
        public const float Hull48FlinchKickDegS = 420f, Hull48FlinchRefDamage = 25f, Hull48FlinchMin = 0.4f, Hull48FlinchMaxScale = 1.5f, Hull48FlinchHz = 6f, Hull48FlinchZeta = 0.4f, Hull48FlinchMaxDeg = 9f;

        /// <summary>The hit FLASH: an additive shell over the hull for this long (s; NIGHT47 section 3: flash 80 ms), this much bigger, this warm
        /// white at this gain.</summary>
        public const float Hull48FlashS = 0.08f, Hull48FlashGrow = 1.03f, Hull48FlashGain = 1.4f;
        public const uint Hull48FlashHex = 0xffe6c0;

        /// <summary>Weight: each foot that plants dips the body (m/s down, into Look47's body spring).</summary>
        public const float Hull48PlantKick = 0.5f;

        /// <summary>The gait's lead: a stepping foot aims at its goal plus the goal's velocity x the step's time x this (the procedural-gait lead:
        /// Research/2026-10-19-wave48/L.md), the velocity's smoothing per frame (0..1); the reel's forced step moves a goal this far (m).</summary>
        public const float Hull48Lead = 0.6f, Hull48LeadSmooth = 0.2f, Hull48ForceStepM = 1.8f;

        /// <summary>The gait keeps up (n48loom gait, a model of this step rule on the sim's foot goals, 10 seeds: the goals reach 37 m/s,
        /// the NIGHT47 gait's pace is 1.5 m / 0.35 s = 4.3 m/s, so a planted foot trailed by up to 30.8 m): a step's time is StepS x PaceMs /
        /// the goal's speed (m/s), never under StepMinS (s); a planted foot more than LeashM (m) from its goal steps even while the other
        /// diagonal group lifts (the leash).</summary>
        public const float Hull48PaceMs = 4f, Hull48StepMinS = 0.16f, Hull48LeashM = 2.5f;

        /// <summary>The idle step: while a foot's goal moves slower than IdleSpeed (m/s) it steps past IdleStepM (m), under the idle shuffle's
        /// 1.2 m sway, so a resting machine keeps re-planting (NIGHT47's 1.5 m could stall for good: Loom37View.Hull48.cs StepDist48).</summary>
        public const float Hull48IdleSpeed = 0.5f, Hull48IdleStepM = 0.9f;

        /// <summary>Reel a548 (G43's boss flame frames): the side camera this far (m) off the cone's middle line (CROWN FIRE's reach is 9 m, its
        /// arc 90 deg: 16 m to the side is out of the arc and shows the stream whole in profile) and this far back toward the stand (m).</summary>
        public const double L48FireSideM = 16, L48FireBackM = 4;
    }
}
