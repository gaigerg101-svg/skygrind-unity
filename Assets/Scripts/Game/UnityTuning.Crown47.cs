// NIGHT47 builder L: THE CROWN's crown47 attacks on screen (Crown37View.N47.cs): CROWN FIRE (the boss flame, L-cards.md section 5) and BELL TOLL.
// Render numbers only; the rules are design.n47.crown47's.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The flame's colour ramp, nozzle to tips (NIGHT47.md section 3 the flame spec: white-hot core, then the enemy family; never cyan
        /// (player-usable) or magenta (RIVAL)): white-hot, yellow, orange; the tips are Crown37DangerHex (red-orange).</summary>
        public const uint Crown47WhiteHex = 0xFFF4DC, Crown47YellowHex = 0xFFC94A, Crown47OrangeHex = 0xFF7A1E;
        /// <summary>Round 2 (the lead's n47-r0d frame: the box cone read as "a flat yellow box beam"): the soft cone. Puffs (KenneyFlare soft
        /// billboards, additive) flowing outward along the cone, each with a hotter heart and a ring of FireRing puffs on the cone's skin; the flow's
        /// rate (puffs a second through one slot) and a puff's size at the nozzle / the tips (m, plus 1.2 x the cone's radius there).</summary>
        public const int Crown47FirePuffs = 40, Crown47FireRing = 5;
        public const float Crown47FlowHz = 2.2f, Crown47PuffFrom = 0.8f, Crown47PuffTo = 2.4f;
        /// <summary>Round 2b (the lead's n47-r1 frame: "a thin cloud of embers ... not a FLAME with weight"): the axis runs from the clapper down to the
        /// floor at FloorAt of the reach; the continuous core: CoreStreaks long KenneyTrace streaks, each CoreLen of the axis; the lick on the floor:
        /// PoolPuffs puffs of PoolPuffM m spread over PoolR m where it lands.</summary>
        public const float Crown47FloorAt = 0.75f, Crown47CoreLen = 0.35f, Crown47PoolR = 1.8f, Crown47PoolPuffM = 1.8f;
        public const int Crown47CoreStreaks = 8, Crown47PoolPuffs = 8;
        /// <summary>The heat haze band: faint warm KenneySmoke sprites along the cone's edge (how many, colour, brightness 0..1): a shimmer at the
        /// cone's edge, never over the screen or the reticle.</summary>
        public const int Crown47HazePuffs = 10;
        public const uint Crown47HazeHex = 0xFFE2B8;
        public const float Crown47HazeK = 0.18f;
        /// <summary>Smoke off the cone's far third (Fx43Shade dark puffs, the TORCH's ink): how many, how dark at most.</summary>
        public const int Crown47SmokePuffs = 12;
        public const float Crown47SmokeDark = 0.45f;
        /// <summary>A spark's streak: its tail is its velocity x this (s): sparks stretch with their speed (Company of Heroes, L.md).</summary>
        public const float Crown47SparkStretch = 0.04f;
        /// <summary>Weight: the ring's pressure kick back along the cone on the sweep's first frame (m, sprung back over 0.15 s) and its shudder
        /// while it pours (m, a cosmetic hash).</summary>
        public const float Crown47FireKickM = 0.4f, Crown47ShudderM = 0.06f;
        /// <summary>The nozzle's glow (m across at full, gain): the clapper heating white through the tell.</summary>
        public const float Crown47NozzleM = 1.2f, Crown47NozzleGain = 3f;
        /// <summary>The floor line of the tell: drawn at this share of the reach, its segments, thickness (m) and gain at the tell's start / end.</summary>
        public const int Crown47LineSegs = 18;
        public const float Crown47LineAt = 0.7f, Crown47LineThick = 0.5f, Crown47LineGain0 = 0.8f, Crown47LineGain1 = 2.6f;
        /// <summary>Embers thrown along the cone: how many, their size (m), how often each restarts (per s), how far they fall over a life (m), gain.</summary>
        public const int Crown47FireEmbers = 40;
        public const float Crown47EmberM = 0.12f, Crown47EmberRate = 1.6f, Crown47EmberFall = 2.5f, Crown47EmberGain = 2.4f;
        /// <summary>The flickering light on the floor and walls (range m, intensity x LightScale, its slow curve's rate Hz: G11's rule, low amplitude).</summary>
        public const float Crown47LightRange = 12f, Crown47LightIntensity = 3f, Crown47LightHz = 1.3f;
        /// <summary>Scorch marks left where the cone passed: how many at once, one every ScorchEvery s while it pours, their size (m) and how long
        /// they fade (s), their colour (a dark burn, not a hue).</summary>
        public const int Crown47ScorchMax = 16;
        public const float Crown47ScorchEvery = 0.12f, Crown47ScorchM = 2.2f, Crown47ScorchSeconds = 4f;
        public const uint Crown47ScorchHex = 0x1A1210;
        // ---- crownLook47 (Crown37View.Look47.cs): THE CROWN's new look and motion (L-cards.md section 2) ----
        /// <summary>Level 5's theme (stone, bronze bells, amber lamps): the points' dark bronze, their gold edge, the phase-2 red-gold.</summary>
        public const uint CLook47BronzeHex = 0x5a3e22, CLook47GoldHex = 0xffc65a, CLook47RedGoldHex = 0xff8a3a;
        /// <summary>The crown's points: how many (eight, the card's), their length along the radius and width (m), the gold edge's width (m) and gain.</summary>
        public const int CLook47Points = 8;
        public const float CLook47PointLen = 2.6f, CLook47PointW = 0.7f, CLook47EdgeW = 0.12f, CLook47EdgeGain = 1.6f;
        /// <summary>The ring's drawn tilt toward you (deg): 0, so the opening drawn is the plane the pass tests (a tilted drawing would aim a jump-dash
        /// wrong); the card's 20 deg lean is round 3's, on the points only. The clapper's chain (m) and size (m).</summary>
        public const float CLook47TiltDeg = 0f, CLook47ChainLen = 1.4f, CLook47ClapperM = 0.9f;
        /// <summary>Motion: a pass's rock kick (deg/s), a hit's flinch (deg/s), the rock spring (Hz, damping: a struck bell's ~1.2 s settle), the
        /// tell's rear-back (deg over its first 40 %) and its spring (Hz), the strike's snap (deg/s), the clapper's lag spring (Hz).</summary>
        public const float CLook47RockKick = 60f, CLook47FlinchKick = 18f, CLook47RockHz = 1.1f, CLook47RockZeta = 0.18f;
        public const float CLook47RearDeg = 10f, CLook47RearHz = 2.2f, CLook47StrikeKick = 140f, CLook47ClapperHz = 0.8f;
        /// <summary>Route47View (crownLook47): THE CROWN's light through level 5's way-on windows: the shaft's z (web: the v11 core's axis, read
        /// from Design/levels/n46/spire.plan.json's spire37 core z -293..-285), how far in front of the window it is drawn (m), the shaft's width (m),
        /// its gain far / near the shaft, the wash's share of it, the slow pulse (Hz: the CROWN's toll).</summary>
        public const double Route47ShaftZ = -289;
        public const float Route47Lift = 0.08f, Route47ShaftW = 0.8f, Route47GainFar = 0.9f, Route47GainNear = 2.4f, Route47WashShare = 0.3f, Route47PulseHz = 0.4f;
        /// <summary>THE COLUMN (round 4, SKYGRIND/Column47): its axis x (web = Unity; z is Route47ShaftZ), bottom and top y (m: n47spire view's
        /// 10..300, the column its checker measures), width (m: the shaft core is 8 m across), gain, and the share drawn behind a roof or wall (the
        /// ZTest Greater pass: a ghost, not the full light, so a roof still reads as a roof).</summary>
        public const float Route47ColumnX = -10f, Route47ColumnBottom = 10f, Route47ColumnTop = 300f, Route47ColumnW = 3f, Route47ColumnGain = 2.2f, Route47ColumnThrough = 0.35f;
        /// <summary>BELL TOLL's wave on the floor: its segments round the ring, height (m: the hop it asks for), thickness (m), gain.</summary>
        public const int Crown47TollSegs = 32;
        public const float Crown47TollH = 0.9f, Crown47TollThick = 0.25f, Crown47TollGain = 2.2f;
    }
}
