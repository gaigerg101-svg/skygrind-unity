// NIGHT36 builder B (boss36 / bossmodel36): THE LANTERN's hall and its jointed body as Boss36View draws them. Visual only: the sim never reads
// these (its numbers are design.n36.boss36). Dusk palette (rule 8): the body dark violet-charcoal so its rim and core read as a silhouette with no
// light; red-orange is danger only (the shield's glow, the lava, the sweep, the camp decal, the locked slab); cyan is player-usable (the ride
// band's trim, the OPEN core); the tells use builder P's yellow / red (UnityTuning.Tell36).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The hall: the wall, floor and roof stone (opaque _BaseColor), the cyan trim strips (height m each, gain), the light.</summary>
        public const uint Hall36WallHex = 0x2b2536, Hall36FloorHex = 0x1e1a26, Hall36RoofHex = 0x16131d, Hall36TrimHex = 0x36e0ff, Hall36LightHex = 0xd8d0ff;
        public const float Hall36TrimHeight = 0.18f, Hall36TrimGain = 1.6f, Hall36TrimStandOff = 0.04f, Hall36LightY = 9f, Hall36LightRange = 40f, Hall36LightIntensity = 7f;
        /// <summary>The locked slab (red-orange, gain) and the well's grate (radius m, gain at rest / in the reveal).</summary>
        public const uint Hall36DangerHex = 0xff5a1f, Hall36WellHex = 0xffa040;
        public const float Hall36SlabGain = 1.4f, Hall36WellRadius = 2.5f, Hall36WellGainRest = 0.25f, Hall36WellGainReveal = 1.6f;
        /// <summary>The reveal's beam out of the well (width m, gain at its peak, the pale boundary colour).</summary>
        public const uint Hall36BeamHex = 0xd8d0ff;
        public const float Hall36BeamWidth = 2.2f, Hall36BeamGain = 1.2f;
        /// <summary>The lava floor: its gain through the tell (min..max, growing from the well out) and while live; lift off the floor (m); disc segments.</summary>
        public const float Lava36TellMin = 0.25f, Lava36TellMax = 1.1f, Lava36LiveGain = 1.5f, Lava36Lift = 0.03f;
        public const int Lava36Segments = 64;
        /// <summary>The sweep: the wall bar's width (m) and gain in the tell / burn, the beam's thickness (m).</summary>
        public const float Sweep36BarWidth = 1.6f, Sweep36TellGain = 1.4f, Sweep36BurnGain = 3.0f, Sweep36BeamThick = 0.35f;
        /// <summary>The camp decal: lift (m), gain through the tell (min..max), the slam's flash (s).</summary>
        public const float Camp36Lift = 0.05f, Camp36TellMin = 0.4f, Camp36TellMax = 2.2f, Camp36SlamFlash = 0.25f;
        /// <summary>The orbs: yellow gain (parryable), the returned orb's white gain.</summary>
        public const float Orb36Gain = 2.4f, Orb36BackGain = 3.2f;
        public const int Orb36Pool = 16;

        /// <summary>The body (bossmodel36): the dark iron (opaque), the rim strips (red-orange, gain), the core shielded (red-orange) / open (cyan, gain,
        /// pulse Hz), the plates' open angle (deg) in a fan tell / an open core, the cage's half width (m), the rib and arm sizes (m).</summary>
        public const uint Body36Hex = 0x2a2438, Body36DarkHex = 0x1a1622, Body36RimHex = 0xff5a1f, Body36CoreHex = 0xff5a1f, Body36OpenHex = 0x36e0ff;
        public const float Body36RimGain = 0.9f, Body36CoreGain = 0.9f, Body36OpenGain = 2.6f, Body36OpenPulseHz = 3f;
        public const float Body36PlateFanDeg = 55f, Body36PlateOpenDeg = 80f, Body36Half = 1.15f, Body36Height = 3.0f;
        public const float Body36UpperArm = 2.0f, Body36Forearm = 1.8f, Body36ArmThick = 0.26f;
        /// <summary>Spring bones: the joints' natural frequency (rad/s, critically damped) and the chain's sway (m of lag per m/s of body motion).</summary>
        public const float Body36SpringW = 9f, Body36ChainLag = 0.12f;
        /// <summary>The breathing idle: Hz and the scale swing; the idle arm sway (deg) and its Hz.</summary>
        public const float Body36BreatheHz = 0.35f, Body36BreatheAmp = 0.035f, Body36SwayDeg = 6f, Body36SwayHz = 0.5f;
        /// <summary>The lead's n36-r3 frames: the body read small in the 30 m ring: drawn at this scale (the sim's hit sphere, design.n36.boss36.lantern.bodyR,
        /// follows it).</summary>
        public const float Body36Scale = 1.4f;
        /// <summary>The core as THE weak point in one frame (the lead's n36-r3 note): its size (m across, before Body36Scale), warm white at rest (gain), the tell's
        /// colour boosted while telling, the plates ajar at rest (deg) so it glows through, its own light (range m, intensity).</summary>
        public const uint Body36IdleCoreHex = 0xfff1d6;
        public const float Body36CoreSize = 1.5f, Body36IdleCoreGain = 2.4f, Body36CoreTellBoost = 1.4f, Body36PlateIdleDeg = 24f, Body36CoreLightRange = 9f, Body36CoreLightIntensity = 3f;
        /// <summary>The stun as a hit (the lead's n36-r3 note: it read only as a slump): a cyan flash on the core (s, gain), the plates kicked open (deg/s), sparks
        /// (count, m/s, s, size, gain).</summary>
        public const float Body36StunFlashS = 0.35f, Body36StunFlashGain = 6f, Body36StunPlateKick = 900f, Body36StunSparkSpeed = 9f, Body36StunSparkLife = 0.6f, Body36StunSparkSize = 0.18f, Body36StunSparkGain = 3f;
        public const int Body36StunSparks = 40;
        /// <summary>The stun's tilt (deg) and the dying's shake (m).</summary>
        public const float Body36StunTiltDeg = 24f, Body36DieShake = 0.12f;
    }
}
