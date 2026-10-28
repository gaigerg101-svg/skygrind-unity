// NIGHT32 builder C: THE CROWN's view numbers (Game/Crown32View.cs). Unity-only (render and layout); every gameplay number is in design.n32.boss5.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The ring's stone colours (dark dusk stone so only the lava, the beam and the core read hot): wall, inner disc, outer sectors, roof, ledges.</summary>
        public const uint Crown32WallHex = 0x463f5c, Crown32InnerHex = 0x3a3550, Crown32SectorHex = 0x2c283a, Crown32RoofHex = 0x17151f, Crown32LedgeHex = 0x8a86a0;
        /// <summary>Red-orange danger (rule 8): the beam, the tells, the lava, the orbs, the mortar mark, the charge line, the crown's prongs.</summary>
        public const uint Crown32DangerHex = 0xff5a1f;
        /// <summary>The core's body (near-black violet, lit by its red-orange seams) and a closed plate (dim grey).</summary>
        public const uint Crown32BodyHex = 0x1c1826, Crown32PlateShutHex = 0x4a4a55;
        /// <summary>The red-orange glow of a sector in its drop tell (pulsing at Crown32TellHz between the two gains) and of a down sector's lava (additive gains).</summary>
        public const float Crown32TellGainLo = 0.25f, Crown32TellGainHi = 0.9f, Crown32TellHz = 3f, Crown32LavaGain = 0.85f, Crown32LavaLift = 0.08f;
        /// <summary>The beam's drawn thickness (m) and its tell line on the wall (m wide, gain).</summary>
        public const float Crown32BeamThick = 0.3f, Crown32TellLineWidth = 0.35f, Crown32TellLineGain = 0.8f;
        /// <summary>An orb's drawn size (m), the mortar mark's ring thickness (m) and lift over the floor (m), the charge line's width (m) and lift.</summary>
        public const float Crown32OrbSize = 0.7f, Crown32MarkThick = 0.25f, Crown32MarkLift = 0.05f, Crown32LineWidth = 1.2f, Crown32LineLift = 0.04f;
        /// <summary>The crown: prongs per side, prong size (m), and the open plates' cyan gain (pulsing at Crown32PlateHz in beat 3's stun).</summary>
        public const int Crown32Prongs = 3;
        public const float Crown32ProngSize = 0.45f, Crown32PlateGain = 1.1f, Crown32PlateHz = 4f;
        /// <summary>The ring's point light: range (m), intensity (x LightScale), colour (dusk violet), height under the roof (m).</summary>
        public const float Crown32LightRange = 60f, Crown32LightIntensity = 9f, Crown32LightBelowRoof = 3f;
        public const uint Crown32LightHex = 0xb59cff;
    }
}
