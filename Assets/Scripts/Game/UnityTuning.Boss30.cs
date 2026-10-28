// NIGHT30 builder B: THE LANTERN RING's and the KILN WYRM head-punch's look (Game/Boss30View.cs). Visual only: the sim never reads these (its numbers are design.n30.lantern30 / wyrm30).
// Dusk palette (rule 8): cyan is player-usable (the wall-ride lane strips, the open core's beam, the punchable head), red-orange is danger only (the swept panel, the vents before the floor
// goes live), the stone stays dark so the danger reads on the dangerous surface, never as a wash.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The ring's stone, as the _BaseColor of a stone-material copy per surface (it multiplies the stone's texture): panels dark dusk violet, floor darker, roof near black,
        /// the platform tops a lighter grey-violet (a safe surface reads as one). Round 1's frame: terracotta panels (the vertex colours were ignored).</summary>
        public const uint Ring30PanelHex = 0x4a4560, Ring30FloorHex = 0x302c3c, Ring30RoofHex = 0x1a1822, Ring30PlatformHex = 0x8a86a0;
        /// <summary>The cyan lane strip on each panel (y laneLow..laneHigh of the config): its gain, and how far it stands off the panel (m) so it never z-fights.</summary>
        public const uint Ring30LaneHex = 0x36e0ff;
        public const float Ring30LaneGain = 0.55f, Ring30LaneStandOff = 0.03f;
        /// <summary>The sweep: the marked panel's red-orange (tell: pulsing from TellMin to TellMax at TellRate Hz; burn: BurnGain), standing off the panel (m).</summary>
        public const uint Ring30SweepHex = 0xff5a1f;
        public const float Ring30SweepTellMin = 0.4f, Ring30SweepTellMax = 1.6f, Ring30SweepTellRate = 6f, Ring30SweepBurnGain = 2.6f, Ring30SweepStandOff = 0.06f;
        /// <summary>The heat vents: a square plate on the floor (size m, lift m), dim at rest, bright red-orange through the floor's tell (VentTellGain), held while live.</summary>
        public const float Ring30VentSize = 1.6f, Ring30VentLift = 0.04f, Ring30VentRestGain = 0.15f, Ring30VentTellGain = 2.2f, Ring30VentLiveGain = 1.2f;
        /// <summary>The lob's landing mark on a platform (m across; tinted with the sweep's red-orange, brightening through the tell).</summary>
        public const float Ring30LobSize = 2.2f;
        /// <summary>The open core's beam: a thin cyan column from the lantern to the roof while the core is exposed (width m, gain).</summary>
        public const float Ring30BeamWidth = 0.35f, Ring30BeamGain = 1.8f;
        /// <summary>The ring's point light: height under the roof (m), range (m), intensity (x LightScale), colour (dusk violet).</summary>
        public const float Ring30LightBelowRoof = 2f, Ring30LightRange = 40f, Ring30LightIntensity = 3f;
        public const uint Ring30LightHex = 0xb59cff;

        /// <summary>THE GLACIER REBUILT: the homing shards (pool, width, length m, gain: red-orange danger), the sweep band (segments along the lap, lift over the floor m),
        /// the volley's tell (a red-orange shell over the boss: scale over its box, gain).</summary>
        public const int Glacier30ShardPool = 24, Glacier30BandSegments = 6;
        public const float Glacier30ShardWidth = 0.25f, Glacier30ShardLength = 1.1f, Glacier30ShardGain = 2.2f, Glacier30BandLift = 0.05f, Glacier30VolleyScale = 1.15f, Glacier30VolleyGain = 1.8f;

        /// <summary>THE LOOM (blocky, as THE LANTERN): the body and the legs dark steel, the joints a dim boundary tone (cyan while they take full damage: player-usable), the joints' size
        /// over their hit sphere, the CUT's red line thickness (m).</summary>
        public const uint Loom30BodyHex = 0x2b2f3a, Loom30LegHex = 0x3a3f4a, Loom30JointHex = 0xcfc6ff;
        public const float Loom30JointGain = 0.5f, Loom30JointScale = 0.8f, Loom30CutThick = 0.08f;

        /// <summary>The punchable head: a cyan shell around the head's box (scale over the box, gain, pulse rate Hz).</summary>
        public const uint WyrmPunch30Hex = 0x36e0ff;
        public const float WyrmPunch30Scale = 1.12f, WyrmPunch30Gain = 2.4f, WyrmPunch30PulseRate = 14f;
        /// <summary>The stagger: a white burst at the head (bits, speed, life, size) and a cyan-white ring that grows from RingStart to RingEnd m over RingLife s.</summary>
        public const int WyrmStagger30Bits = 40;
        public const float WyrmStagger30Speed = 16f, WyrmStagger30Life = 0.5f, WyrmStagger30Size = 1.4f, WyrmStagger30RingStart = 1f, WyrmStagger30RingEnd = 9f, WyrmStagger30RingLife = 0.35f, WyrmStagger30RingThick = 0.15f, WyrmStagger30RingGain = 2.6f;
    }
}
