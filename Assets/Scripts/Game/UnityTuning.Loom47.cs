// NIGHT47 builder L: THE LOOM's loom47 tells and the seam (Loom37View.N47.cs). Render numbers only; the rules are design.n47.loom47's.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The blink of a loom47 tell's last quarter (Hz), as the sweep's decal (Loom41DecalBlinkHz): "now".</summary>
        public const float Loom47BlinkHz = 6f;
        /// <summary>SHUTTLE DIVE's tell: the line's thickness from the body to the marked perch (m; thicker than the lash's, a bigger read) and the
        /// side of the red-orange disc on the spot it lands (m, about the dive's hit radius x 2).</summary>
        public const float Loom47DiveLineThick = 0.45f, Loom47DiveSpotM = 6f;
        /// <summary>WARP CAGE's tell: how many threads stand on its ring and their thickness (m).</summary>
        public const int Loom47CageThreads = 8;
        public const float Loom47CageThick = 0.18f;
        /// <summary>THE SEAM: a latched knee inside a BEND glows yellow (the parry / punch-it colour, PRINCIPLES 3), at this gain plus
        /// SeamGainFull x its fill, pulsing at SeamHz, growing by SeamGrow x its fill.</summary>
        public const uint Loom47SeamHex = 0xFFD23A;
        public const float Loom47SeamGain = 1.6f, Loom47SeamGainFull = 1.6f, Loom47SeamHz = 4f, Loom47SeamGrow = 0.35f;

        /// <summary>THE FRAME's falling posts (sim.loom47Posts, drawn here: a world layer has no level mesh): the stone's colour (level 4's
        /// grey-violet stone), the amber pool's gain on their tops while they stand, how long their fall is drawn (s) and its gravity (m/s2).</summary>
        public const uint Loom47PostHex = 0x4a4458;
        public const float Loom47PostPoolGain = 1.3f, Loom47PostFallS = 1.6f, Loom47PostFallG = 14f;

        /// <summary>THE WARP NET's look (sim.Loom47NetY): threads across x and along z, their thickness (m) and gain, the pluck's sag on a catch (m),
        /// and the start ledge's depth the net stops short of (m: THE LOOM FRAME's 8 m start ledge, gen-n47-span.cjs).</summary>
        public const int Loom47NetThreadsX = 9, Loom47NetThreadsZ = 10;
        public const float Loom47NetThick = 0.07f, Loom47NetGain = 0.9f, Loom47NetPluckM = 0.8f, Loom47NetLedgeM = 8f;

        // ---- loomLook47 (Loom37View.Look47.cs): THE LOOM's new look and motion (L-cards.md section 1) ----
        /// <summary>Level 4's theme (iron, brass, amber lamps at dusk: docs/ROOMS.md level 4's amber lamps): the drum's brass, its hot phase-2
        /// tone and gain, the lamps' and the warp's rest amber.</summary>
        public const uint Look47BrassHex = 0x8a6a2e, Look47BrassHotHex = 0xffa43a, Look47AmberHex = 0xffb347;
        /// <summary>The body's own warm dark bronze-iron (round 2b: it read grey) and its phase-2 hotter tone.</summary>
        public const uint Look47BodyHex = 0x5a4126, Look47BodyHotHex = 0x7a4a22;
        public const float Look47BrassHotGain = 1.4f;
        /// <summary>The shuttle: the body's width x this (narrower: a shuttle, not a crate), the drum's radius and length (m), the lamps' size (m)
        /// and gain.</summary>
        public const float Look47BodyNarrow = 0.6f, Look47DrumR = 1.3f, Look47DrumL = 2.6f, Look47LampM = 0.9f, Look47LampGain = 3.2f;
        /// <summary>Round 4 (the lead's r2 shuttle frames: "a box"): the hull, a hexagonal prism pointed at both ends: each tip's share of its length, the
        /// hull's length x the body's; its brass bands (how many, spread over this share of the body's length, each this long m); the hips' brass
        /// collars (m across).</summary>
        public const int Look47Bands = 3;
        public const float Look47TipShare = 0.22f, Look47HullLen = 1.3f, Look47BandSpread = 0.6f, Look47BandM = 0.35f, Look47CollarM = 0.9f;
        /// <summary>The warp: threads from the drum to the roof, segments per thread (the pluck's wave needs a few), the fan's width and the
        /// roof's height (m: o3's walls are 30 m), thickness, sag (m) and gains (rest, an attack running, phase 3 burning).</summary>
        public const int Look47WarpThreads = 6, Look47WarpSegs = 5;
        public const float Look47WarpFanM = 16f, Look47WarpTopY = 29.5f, Look47WarpThick = 0.09f, Look47WarpSagM = 0.6f, Look47WarpGain = 0.7f, Look47WarpHotGain = 1.6f, Look47WarpBurnGain = 2.2f;
        /// <summary>The pluck: its amplitude (m) on a tell's start, a tear and a hit, how fast it decays (m/s), its wave's rate (Hz).</summary>
        public const float Look47PluckTell = 0.5f, Look47PluckTear = 1.4f, Look47PluckHit = 0.25f, Look47PluckDecay = 1.2f, Look47PluckHz = 3.5f;
        /// <summary>Motion (NIGHT47 section 3): the anticipation sink and pull-back over a tell's first 40 % (m), the strike's kick (m/s), the hit
        /// flinch's kick (m/s: a 60-100 ms recoil at this spring), the body spring's rate (Hz) and damping (under 1: an overshoot, then a settle).</summary>
        public const float Look47AnticipM = 0.8f, Look47PullBackM = 0.6f, Look47StrikeKick = 6f, Look47FlinchKick = 2.5f, Look47SpringHz = 2.2f, Look47SpringZeta = 0.45f;
        /// <summary>Round 2 (the lead's n47-r0d frame: "its body is grey"): the brass glow strips along the body's top edges and the drum's band
        /// (their thickness m, their gain): level 4's brass / amber read at 40 m.</summary>
        public const float Look47StripM = 0.25f, Look47StripGain = 1.8f;
        /// <summary>The steps (the procedural-spider rule): a planted foot steps when its target is this far (m), over StepS (s), lifting StepLift (m),
        /// reaching StepOvershoot (m) past before it settles.</summary>
        public const float Look47StepDist = 1.5f, Look47StepS = 0.35f, Look47StepLift = 1.5f, Look47StepOvershoot = 0.25f;
        /// <summary>The idle shuffle: each foot's goal sways this far (m) along the wall at this rate (Hz): a step every few seconds at rest.</summary>
        public const float Look47ShuffleM = 1.2f, Look47ShuffleHz = 0.22f;
        /// <summary>The roll lag: degrees of lean per m/s of the glide across the body, its cap (deg) and its spring's rate (Hz).</summary>
        public const float Look47RollPerMs = 1.2f, Look47RollMax = 14f, Look47RollHz = 1.4f;
    }
}
