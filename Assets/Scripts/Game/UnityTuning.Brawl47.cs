// NIGHT47 builder B: render numbers for B's view rows (chargerLook47, brawlLook47, wyrmLook47). Visual only: the sim never reads these.
// Palette (CLAUDE.md rule 8, NIGHT47.md section 3): red-orange = danger (the charge line, the worm's tells), yellow = punch it (a stunned
// charger's open back, THE ICEBREAKER's window), white only as the hot edge of a locked line.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>chargerLook47, the charge line (the CHARGER card's wind-up): while it still follows you the line is drawn at
        /// Charger47LineFollow x its NIGHT19 brightness and pulses slowly (Charger47PulseRate rad/s); once it locks (Charger.locked47, the last
        /// 0.25 s) it steps to Charger47LineLocked x, stops pulsing, is Charger47LockWhite toward white (the "now" edge) and Charger47LockWiden x
        /// wider, so the lock reads as a change in one frame (E-4, L-001).</summary>
        public const float Charger47LineFollow = 0.75f, Charger47PulseRate = 8f, Charger47LineLocked = 2.2f, Charger47LockWhite = 0.45f, Charger47LockWiden = 1.25f;

        /// <summary>chargerLook47, the CHARGER's body (the card's shape logic; Unity space: +z is its front, metres; Y as a share of its height):
        /// the PROW, a wedge plate over the head and shoulders (size, its centre's height share and forward offset, its pitch back over the head in
        /// degrees); the two HORN blades swept back off the prow (half spacing, height share, forward offset, size, pitch, yaw out); the three back
        /// VENT slats (first height share, the step between them, their back offset, size; how much taller an open vent is).</summary>
        public static readonly UnityEngine.Vector3 Charger47ProwSize = new UnityEngine.Vector3(1.7f, 0.95f, 0.32f), Charger47HornSize = new UnityEngine.Vector3(0.14f, 0.14f, 1.15f), Charger47VentSize = new UnityEngine.Vector3(0.72f, 0.1f, 0.08f);
        public const float Charger47ProwY = 0.62f, Charger47ProwZ = 0.78f, Charger47ProwPitch = 32f;
        public const float Charger47HornX = 0.72f, Charger47HornY = 0.8f, Charger47HornZ = 0.5f, Charger47HornPitch = -18f, Charger47HornYaw = 28f;
        public const float Charger47VentY0 = 0.42f, Charger47VentStep = 0.11f, Charger47VentZ = -0.7f, Charger47VentOpen = 2.2f;
        /// <summary>The colours: the prow dark steel; the horns red-orange (danger) x gain; the vents dark idle, red-orange x hot gain at the
        /// wind-up's end, yellow x open gain while stunned.</summary>
        public const uint Charger47ProwHex = 0x2b2b36, Charger47VentIdleHex = 0x2a1612;
        public const float Charger47HornGain = 1.1f, Charger47VentHotGain = 2.2f, Charger47VentOpenGain = 2.4f;

        /// <summary>brawlLook47, THE ICEBREAKER: the springs on the lean / head / knee drop (rad/s, damping ratio: ~15 % overshoot, settle <= 0.3 s);
        /// the flinch kick on a hit (deg/s back); the idle breath (rad/s, deg); STUCK / WINDOW's yellow gain; the armour's frost colour and gain;
        /// the ice bursts (the armour breaking, a plate shed: bits, speed m/s, life s, size).</summary>
        public const float Brawl47SpringW = 16f, Brawl47SpringZeta = 0.5f, Brawl47FlinchKick = 120f, Brawl47BreathRate = 1.6f, Brawl47BreathDeg = 2.5f;
        public const float Brawl47YellowGain = 2.2f, Brawl47FrostGain = 1.8f;
        /// <summary>The frost plates' glow gain (they use the view's glow material: the lit stone read dark at dusk, r2 frames).</summary>
        public const float Brawl47PlateGain = 1.4f;
        public const uint Brawl47FrostHex = 0xe8eef8;
        /// <summary>THE ICE ARMOUR's four opaque frost plates on the torso (the lean pivot's space, metres: chest left / right, shoulders left /
        /// right): their centres, sizes and tilts (deg). Shown while Brawl40.Armored47, thrown off when it breaks.</summary>
        public static readonly UnityEngine.Vector3[] Brawl47PlatePos = { new UnityEngine.Vector3(-0.55f, 1.1f, 0.85f), new UnityEngine.Vector3(0.55f, 1.1f, 0.85f), new UnityEngine.Vector3(-1.15f, 1.75f, 0.2f), new UnityEngine.Vector3(1.15f, 1.75f, 0.2f) };
        public static readonly UnityEngine.Vector3[] Brawl47PlateSize = { new UnityEngine.Vector3(1.0f, 1.2f, 0.22f), new UnityEngine.Vector3(1.0f, 1.2f, 0.22f), new UnityEngine.Vector3(0.9f, 0.3f, 1.1f), new UnityEngine.Vector3(0.9f, 0.3f, 1.1f) };
        public static readonly UnityEngine.Vector3[] Brawl47PlateTilt = { new UnityEngine.Vector3(-8f, 6f, 0f), new UnityEngine.Vector3(-8f, -6f, 0f), new UnityEngine.Vector3(0f, 0f, 14f), new UnityEngine.Vector3(0f, 0f, -14f) };
        public const int Brawl47BurstBits = 36, Brawl47ShedBits = 18;
        public const float Brawl47BurstSpeed = 9f, Brawl47BurstLife = 0.7f, Brawl47BurstSize = 1.1f, Brawl47ShedSize = 0.8f;

        /// <summary>wyrmLook47, THE KILN WYRM in molten colours (DECIDE47 D-9: GG's 10-09 blocky shape, NIGHT26's palette: UnityTuning.Wyrm26Fx.cs's
        /// glow / amber / hot / ash, the Tripo shader's): the ash body, the red-orange glow it heats toward, the amber seams (the back plates), the
        /// white-hot jaw; yellow (punch it) on the head while the punch window is open (Wyrm30Punchable).</summary>
        public const uint Wyrm47AshHex = 0x1a1210, Wyrm47GlowHex = 0xff4a10, Wyrm47AmberHex = 0xff9a28, Wyrm47HotHex = 0xfff2d6, Wyrm47PunchHex = 0xffd23a;
        /// <summary>The molten wave: rad/s, rad per piece down the body; how far the body heats toward the glow (0..1) at the wave's top; the seams'
        /// and the punch-head's emissive gains; how far the jaw heats toward white.</summary>
        public const float Wyrm47WaveRate = 3f, Wyrm47WaveStep = 0.55f, Wyrm47BodyHeat = 0.35f, Wyrm47SeamGain = 2.4f, Wyrm47PunchGain = 2.2f, Wyrm47JawHeat = 0.25f;
        /// <summary>The burrow's trail (G34): a crack plate on the floor over each buried piece: its lift off the floor (m), thickness (m), its
        /// size x the piece's footprint, its gain, its flicker rate (rad/s).</summary>
        public const float Wyrm47TrailLift = 0.04f, Wyrm47TrailThick = 0.05f, Wyrm47TrailWiden = 1.3f, Wyrm47TrailGain = 2.6f, Wyrm47TrailPulse = 9f;
    }
}
