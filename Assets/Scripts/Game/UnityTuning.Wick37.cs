// NIGHT37 builder W (wick37): THE STACK and THE WICK as Wick37View draws them. Visual only: the sim never reads these (its numbers are
// design.n37.wick37). Dusk palette (rule 8): the shaft's hot iron glows red-orange (danger: it throws you off); the ride panels, the ledge lips and
// the cap's ring wall carry cyan trim (player-usable); the slag is red-orange; THE WICK's core is warm at rest, the tell's colour (UnityTuning.Tell36)
// while it winds up, CYAN in the window and in the dive's stall.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The stack's surfaces (opaque _BaseColor): the shaft's iron, the panels, the ledges, the cap and the tunnel.</summary>
        public const uint Stack37ShaftHex = 0x2a1a1c, Stack37PanelHex = 0x2b2536, Stack37LedgeHex = 0x3a3344, Stack37CapHex = 0x302a3c, Stack37TunnelHex = 0x221d2a;
        /// <summary>The shaft's hot seams (red-orange, gain; one every SeamStep m, SeamH m tall, SeamOff m off the wall).</summary>
        public const uint Stack37HotHex = 0xff5a1f;
        public const float Stack37SeamGain = 1.1f, Stack37SeamStep = 1.2f, Stack37SeamH = 0.16f, Stack37SeamOff = 0.03f;
        /// <summary>The hot wall reads hot (the lead, round 2): a dim red-orange wash over every shaft facet (gain), the seams' ripple (Hz, +-amp:
        /// two row sets in counter-phase), and the flash when a burn throws the player off (seconds, extra gain at its start).</summary>
        public const float Stack37WashGain = 0.12f, Stack37ShimmerHz = 0.9f, Stack37ShimmerAmp = 0.45f, Stack37BurnFlashS = 0.35f, Stack37BurnFlashGain = 2.5f;
        /// <summary>The cyan trim on the panels' ride faces, the ledge lips and the cap's ring wall (m thick, gain, stand-off m).</summary>
        public const uint Stack37TrimHex = 0x36e0ff;
        public const float Stack37TrimH = 0.16f, Stack37TrimGain = 1.7f, Stack37LipGain = 0.8f, Stack37TrimOff = 0.035f, Stack37CapTrimY = 1.6f;
        /// <summary>The shaft's pale lights (heights m, range m, intensity before LightScale, colour) and the slag's glow light (range, intensity).</summary>
        public static readonly float[] Stack37LightYs = { 6f, 18f, 30f, 43f };
        public const uint Stack37LightHex = 0xd8d0ff, Stack37SlagHex = 0xff6a22;
        public const float Stack37LightRange = 26f, Stack37LightIntensity = 9f, Stack37SlagLightRange = 16f, Stack37SlagLightIntensity = 24f, Stack37SlagGain = 1.3f, Stack37SlagLift = 0.02f;
        public const int Stack37DiscSegments = 64;
        /// <summary>THE WICK: the plates (iron), the rings, the core (warm idle hex, gains), the cracked plates' glow, the core light.</summary>
        public const uint Wick37IronHex = 0x17131b, Wick37IronDarkHex = 0x0e0b11, Wick37WarmHex = 0xffa040, Wick37OpenHex = 0x36e0ff;
        public const float Wick37PlateW = 0.95f, Wick37PlateT = 0.14f, Wick37CoreGainIdle = 2.4f, Wick37CoreGainTell = 3.4f, Wick37CoreGainOpen = 3.8f, Wick37CrackGain = 1.8f;
        /// <summary>Round 3 (the lead on n37-r6 frames: "a plain beige drum, the core hidden"): the core drawn at this share of the cage radius (it shows through the gaps between the narrower, darker plates), the plates flung open this far in the window / the stall (deg), the plates' ember-lit inner edges (gain).</summary>
        public const float Wick37CoreDrawFrac = 0.78f, Wick37OpenTiltDeg = 75f, Wick37EdgeGain = 0.7f;
        /// <summary>The flare's CURTAIN: a red sheet through the axis on the line between the burning half and the safe one (cross it), m tall, gain in the tell (min..) and live.</summary>
        public const float Wick37CurtainH = 10f, Wick37CurtainTellMin = 0.35f, Wick37CurtainLive = 0.6f, Wick37FlareTellMin = 0.9f;
        public const float Wick37CrackTiltDeg = 28f, Wick37CoreLightRange = 14f, Wick37CoreLightIntensity = 30f, Wick37BreatheHz = 0.7f, Wick37BreatheAmp = 0.06f;
        /// <summary>The embers (m radius drawn), the flare band (degrees span, m tall, gain at the peak), the slam decal (gain).</summary>
        public const float Wick37EmberDraw = 0.45f, Wick37FlareSpanDeg = 180f, Wick37FlareH = 12f, Wick37FlareGain = 1.4f, Wick37SlamGain = 1.4f;
        public const int Wick37EmberPool = 12, Wick37FlareSegments = 24;
    }
}
