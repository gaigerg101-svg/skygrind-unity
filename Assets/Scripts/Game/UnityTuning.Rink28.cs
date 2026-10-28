// NIGHT28 builder A2: THE GLACIER rink's look (Game/RinkView28.cs, Assets/Shaders/Resources/RinkIce28.shader). Visual only: the sim never reads these (its numbers are design.n28.rink).
// Dusk palette (rule 8): cyan (colors.usable) marks the wall-ride lane (player-usable), the pale violet-white boundary tone (barrier.boundaryColor) draws the boundary lines, the ice itself is a
// cool violet-blue, the roof is the roof27 dusk-dark violet; red-orange never appears here (danger and enemies only), magenta never (RIVAL).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The ice's colour on its shadow side and its lit side (the shader's fake normal shade; 0xRRGGBB).</summary>
        public const uint RinkIceDeepHex = 0x1a2040, RinkIceLitHex = 0x5a68a0;
        /// <summary>The rim wall's and the island's stone tint (vertex colour on the stone material), and the wall's cyan strip height above the lip, its thickness and gain (HDR so bloom picks it up).</summary>
        public const uint RinkWallHex = 0x4a4258;
        public const float RinkStripHeight = 1.4f, RinkStripThick = 0.3f, RinkStripGain = 0.9f;
        /// <summary>The dim wide glow under the cyan strip (height m, gain): the first Player frame showed a 2 m slab of pure cyan, so the strip is a thin line and the glow is faint.</summary>
        public const float RinkGlowHeight = 1.6f, RinkGlowGain = 0.16f;
        /// <summary>Vertex-colour tint of the ice by zone (the shader brightens by 1.25): the lane and the door floor 0.8, the bank a little lighter 1.0, the island top darker 0.5.</summary>
        public const float RinkLaneTint = 0.8f, RinkBankTint = 1.0f, RinkIslandTint = 0.5f;
        /// <summary>The door posts (pale violet-white, mark each door gap): the post's side in m, its height above the island top, the gain.</summary>
        public const float RinkPostSize = 0.18f, RinkPostHeight = 0.9f, RinkPostGain = 1.3f;
        /// <summary>The boundary lines (at the bank's foot and round the island's top edge): width in m, lift above the surface in m, gain on the additive colour.</summary>
        public const float RinkLineWidth = 0.14f, RinkLineLift = 0.02f, RinkLineGain = 1.1f;
        /// <summary>The rink's one light, over its centre, as a share of the rim height (the room light's range / intensity / colour come from design.n27.boss2.room).</summary>
        public const float RinkLightHeightShare = 0.85f;
        /// <summary>The wall ride lane's strip stands this far off the wall (m), so it does not z-fight the rim.</summary>
        public const float RinkStripOff = 0.03f;
    }
}
