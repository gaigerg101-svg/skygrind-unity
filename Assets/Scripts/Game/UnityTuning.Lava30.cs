// NIGHT30 builder V (item 3, design.n30.lava; the Lantern's floor and the boss glows): LavaView30's and HudLava30's Unity-only view numbers (CLAUDE.md
// rule 5: each with a comment). The lava's gameplay numbers (damage share, knock, grace, the 0.6 s hurt flash's length) are builder S's design.n30.lava
// rows and are read from config, never typed here; the lava rects come from the level JSON's `lava` array (builder L).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the lava surface (LavaView30 + Shaders/Resources/Lava30.shader) -------------------------------------------------------------------------
        public const uint Lava30HotHex = 0xff5a1f; // the danger red-orange of the molten surface (rule 8: red-orange is danger; lava is danger)
        public const uint Lava30GoldHex = 0xffb84a; // the gold of the seams and the rim (DESIGN30 B: "a gold rim on its edge")
        public const uint Lava30CrustHex = 0x52100a; // the dark crust between the molten veins
        public const float Lava30Gain = 1.6f; // the surface's emission (over 1: bloom carries the glow; it lights nothing else)
        public const float Lava30PulseHz = 0.8f; // the surface's breath (DESIGN30 E: a 0.8 Hz pulse); REDUCED MOTION holds it at 1
        public const float Lava30PulseDepth = 0.15f; // the breath's depth (+-15 %): a slow swell, far under the 3-flashes-a-second limit
        public const float Lava30Lift = 0.01f; // m over the plan's surface y (no fight with the trench floor's own face)
        public const float Lava30RimWidth = 0.14f; // m: the gold rim strip along each edge of a lava rect
        public const float Lava30RimLift = 0.02f; // m over the surface (the rim sits on top of the lava)
        public const float Lava30RimGain = 1.3f; // the rim's additive brightness (x the gold, x the pulse)
        public const string Lava30Shader = "SKYGRIND/Lava30"; // Shaders/Resources/Lava30.shader (a plain opaque red-orange when the shader is missing)

        // --- a burning enemy (event lavaEnemyBurn, builder S) ------------------------------------------------------------------------------------------
        public const int Lava30BurnPool = 8; // flares at most at once
        public const float Lava30BurnSeconds = 0.8f; // s: a flare's life (rises and fades)
        public const float Lava30BurnRise = 1.6f; // m: how far a flare climbs over its life
        public const float Lava30BurnWidth = 0.9f, Lava30BurnHeight = 1.8f; // m: the flare column's size at its start (it narrows as it rises)

        // --- the player's lava hurt flash (event lavaBurn; HudLava30.cs) -------------------------------------------------------------------------------
        public const float Lava30FlashAlpha = 0.55f; // the edge flash's peak alpha (x FLASH INTENSITY), red-orange inset edges only: the centre stays clear (R2: under 25 % of the screen)

        // --- THE LANTERN's floor with design.n30.look30 or lantern30 (the full-frame wash removed: shot 170227, mid-frame luminance 0.347) ---------------
        public const float Lava30LanternLift = 0.04f; // m over the arena floor (NIGHT29's Lava29Lift)
        public const float Lava30TellFrom = 0.12f, Lava30TellTo = 0.45f; // the floor's gain share through the tell (it heats up visibly before it hurts)
        public const float Lava30VentSize = 1.4f; // m: a heat vent's glowing disc (B's Lantern30Arena.Vent positions) in the tell
        public const float Lava30VentColumn = 2.4f; // m: the faint heat column over a vent in the tell
        public const float Lava30VentGain = 1.4f; // the vents' additive gold brightness
        public const float Lava30BeamWidth = 0.5f; // m: the open core's beam (core to roof) while the floor is live
        public const float Lava30BeamGain = 0.7f; // the beam's additive white-gold brightness
        public const float Lava30SweepInset = 0.06f; // m: the sweep's panel mark stands this far inside the panel (no fight with the wall)
        public const float Lava30SweepTellGain = 0.55f, Lava30SweepBurnGain = 1.3f, Lava30SweepTellHz = 2.5f; // the marked panel: a 2.5 Hz swell in its tell (under 3 a second), bright while it burns

        // --- the KILN WYRM's head while it can be punched (B's Wyrm30Punchable) ---------------------------------------------------------------------
        public const float Lava30WyrmGlowScale = 1.35f; // x the head's largest size: the cyan shell round it (cyan: player-usable, "punch now")
        public const float Lava30WyrmGlowGain = 0.8f; // the shell's additive brightness
    }
}
