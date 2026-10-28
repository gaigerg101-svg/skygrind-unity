// NIGHT40 builder L2 (the lead's round-4 ask: "the ice canal at dusk" did not read; every room was the same brown): n40-iceline's stone colours, a VIEW
// change only (SurvivalBuilder.Build's vertex colours for the floor, the map boxes and the wedges). Used only while design.n40.iceline40 is on, no
// NIGHT40 off switch is given, and the map is n40-iceline (SurvivalBuilder.Ice40Look); every other level, and -skygrindOff40, keep config.colors.pillarTop
// / pillarSide exactly. Render numbers only (CLAUDE.md rule 5); plain constants (no Unity API in an initializer, L-035).
// CLAUDE.md rule 8: neither is cyan (player-usable, colors.usable 0x19d3e6, hue ~186 deg), red-orange (danger, colors.enemy 0xff4a1c) or the boundary's
// pale violet-white (design.n27.roof27.boundaryHex 0xd8d0ff); the ride trims and the danger colours are other materials and do not change.
// Round 4b (the lead's n40-r4 frame f0300, g1): the first tint (top 0xa9c3d9, side 0x4a5568) WAS drawn but read tan: pixels sampled (floor centre
// 145,107,94 before -> 155,128,111 after; the lit west wall 103,71,71 -> 92,68,56): the room's warm light (look33's ceiling panels, the sconces, the
// iceline grade) keeps about 0.9 x red, 0.7 x green, 0.5 x blue of the albedo, and the first side colour had LESS blue than the old violet stone.
// Both are now bluer, so what survives the warm light is a grey-blue, not tan (not checked in the Player: the lead's next frame).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>Every top face (the floor, decks, the bridge): ice, an azure blue-white (hue ~215 deg, well away from the cyan's ~186) strong enough
        /// in blue that, under the room's warm light (x ~0.9 / 0.7 / 0.5 per channel, measured on f0300), the floor reads a pale grey-blue.</summary>
        public const uint Ice40TopHex = 0x8db4ec;

        /// <summary>Every side face (the walls, the pillars, the panels' sides): a cold dusk stone, a deep blue-grey (hue ~224 deg), more blue than
        /// colors.pillarSide (0x524a7a: 140 vs 122) and less red, so the walls read colder than the old violet under the same light.</summary>
        public const uint Ice40SideHex = 0x46598c;
    }
}
