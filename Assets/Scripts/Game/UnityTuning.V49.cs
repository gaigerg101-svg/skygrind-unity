// NIGHT49 builder V (design.n49.look49, a view row; G2-G4 environment): THE COLOUR SCRIPT. One grade for the whole game (the same structure and
// strengths on every level: Neutral tonemapping, ShadowsMidtonesHighlights toward the level hue, a contrast lift, one bloom) and one row per level
// (its one dominant environment hue on a dark base: fog, the stone's sky fog, the trilight ambient, the stone's rim, the grade's tint, its EV).
// GG 2026-10-19: "the colors and stuff are way too bland ... it all needs to be unified ... I'm very inspired by Ultra Kill and Doom".
// Rules (CLAUDE.md rule 8; NIGHT49-BRIEF section 2): the role tokens are UnityTuning.Look49.cs's (the only colour table: read, never edited here);
// a level hue sits at least Look49AccentMinHueGapDeg (20) from every role hue (usable 186, danger 12, parry 44, RIVAL 312, boundary 250), so the
// allowed windows are 64-166, 206-230 and 270-292 deg (332-352 is legal but reads as danger / RIVAL: not used); every environment colour here has
// saturation 0.25-0.65 (round 2: the rims 0.65, the tints 0.25-0.5), under the roles' 0.77-0.89 (the boundary is a pale, 0.18, judged by hue only). Level 1's lava keeps the danger hue: its
// walls take the cool jade, 146 deg away. The HUD is outside the grade (every HUD canvas is Screen Space - Overlay, drawn after URP's post).
// Order = LightPlan33.Themes: foundry, iceline, kiln, span, spire. Plain constants (no Unity API in an initializer, L-035). View only.
// The hex tables were made from (hue, saturation, value) by the builder's scratch script (colorsys.hsv_to_rgb, rounded): each comment gives them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The level's one dominant environment hue, degrees (the documented source of every table below; V49Tests checks the gaps).
        /// foundry 158 jade (the lava's orange is its complement: the danger pops); iceline 216 azure (cold, 30 deg from the usable cyan, with
        /// look41's blue-white key); kiln 66 warm yellow-stone (round 2: was 72, which the lead read as "olive mud" on n49-r1: 66 is the warmest
        /// hue the rule allows, 22.4 deg off the parry yellow, and its tint is the weakest so look41's amber sun keeps the stone warm); span 272 dusk
        /// violet (round 2: was 280, which turned the floor pink on n49-r1, near RIVAL's family: 272 is 39.6 deg from RIVAL, 21.8 from the
        /// boundary, with a weak tint); spire 124 emerald (under look41's pale gold key; the farthest from every role).</summary>
        public static readonly double[] V49HueDeg = { 158, 216, 66, 272, 124 };

        /// <summary>URP exponential fog colour per level (enemies and props: the Lit / Unlit fog): hsv(hue, 0.45, 0.11), the hue's dusk dark.
        /// Darkens with distance, never lifts (look33's rule).</summary>
        public static readonly uint[] V49FogHex = { 0x0f1c17, 0x0f141c, 0x1b1c0f, 0x160f1c, 0x0f1c10 };
        /// <summary>URP exponential fog density per level: look33's were 0.002-0.007 (grey far walls); a little more so the far room sinks into the
        /// level's dark hue (at 20 m combat range 0.010 keeps exp(-0.2) = 82 % of a foe's colour: silhouettes hold, L4D's lesson). Span is the
        /// LOOM's wide hall (already 61 % dark): the least.</summary>
        public static readonly double[] V49FogDensity = { 0.010, 0.010, 0.008, 0.006, 0.010 };

        /// <summary>The stone's own fog (Stone.shader SkyAt: the _SkyFogZenith / Mid / Horizon globals; look33 dimmed the config's lavender sky to
        /// 0.4): the level hue at hsv(hue, 0.5, 0.05) above, (0.5, 0.09) at the middle, (0.45, 0.15) at the horizon. Far stone fades to the level's
        /// dark, not to a shared lavender.</summary>
        public static readonly uint[] V49StoneFogZenithHex = { 0x060d0a, 0x06090d, 0x0c0d06, 0x0a060d, 0x060d07 };
        public static readonly uint[] V49StoneFogMidHex = { 0x0b1713, 0x0b1017, 0x16170b, 0x120b17, 0x0b170c };
        public static readonly uint[] V49StoneFogHorizonHex = { 0x152620, 0x151c26, 0x252615, 0x1e1526, 0x152616 };

        /// <summary>The trilight ambient per level (RenderSettings and the stone's SH probe, as look33's Probe33 writes it): the top hsv(hue, 0.45,
        /// 0.07), the middle (0.45, 0.13), the bottom (0.45, 0.10). The top is DIMMER than the bottom (look33's was brighter), so an up-facing floor
        /// takes less ambient than a wall (the brief: floors darker than walls); the middle stays dusk dark (the fixtures make the pools).</summary>
        public static readonly uint[] V49AmbientSkyHex = { 0x0a120f, 0x0a0d12, 0x11120a, 0x0e0a12, 0x0a120a };
        public static readonly uint[] V49AmbientEquatorHex = { 0x12211c, 0x121821, 0x202112, 0x1a1221, 0x122113 };
        public static readonly uint[] V49AmbientGroundHex = { 0x0e1a15, 0x0e131a, 0x181a0e, 0x140e1a, 0x0e1a0f };

        /// <summary>The stone's unshadowed rim light colour per level (_RimColor; low, 18 deg up: it lights walls more than floors). Round 2 (the lead:
        /// "a lower saturation for the floor than for the walls"): the walls' colour moves here, hsv(hue, 0.65, 1) (round 1: 0.55; still under the
        /// roles' 0.77), with the grade tint below weaker; kiln's at hsv(66, 0.40, 1), so its walls stay warm stone, not acid green.
        /// It replaces the config's violet colors.lightRim (hue 263: 13 deg from the boundary) with the level hue, so every wall carries it.</summary>
        public static readonly uint[] V49RimHex = { 0x59ffc2, 0x599cff, 0xf5ff99, 0xb259ff, 0x59ff64 };
        /// <summary>The rim's strength: config lights.rimIntensity x LightScale x 3 (Look.Apply's) x this. look33 used 0.3 (the rooms' even wash);
        /// round 1 0.4; round 2 0.6 (n49-r1: 5-9 % saturated on the combat frames): the walls carry the hue while the floors (lit from above) do not.</summary>
        public const float V49RimScale = 0.6f;

        /// <summary>The grade's tint per level, taken linear and divided by its own luminance (a luminance-neutral multiplier), then mixed in by
        /// V49ShadowTint / V49MidTint (the same shares on every level: one grade). It reaches floors and walls alike, so round 2 made it weaker where
        /// the n49-r1 floors went wrong: hsv(hue, s, 1) with s foundry 0.40 (was 0.5: khaki floor), iceline 0.45, kiln 0.25 ("olive mud"), span 0.30
        /// (the pink floor: its red channel x1.28 -> x1.11 in the shadows), spire 0.50.</summary>
        public static readonly uint[] V49GradeTintHex = { 0x99ffda, 0x8cbaff, 0xf9ffbf, 0xdbb2ff, 0x80ff88 };
        /// <summary>ShadowsMidtonesHighlights: the shadows' share of the tint multiplier (0 = none, 1 = the full luminance-neutral tint).</summary>
        public const float V49ShadowTint = 0.35f;
        /// <summary>The midtones' share of the tint (smaller: the lit walls lean to the hue, the role colours on top keep theirs).</summary>
        public const float V49MidTint = 0.18f;
        /// <summary>The highlights stay neutral (1, 1, 1): the emissive role colours (cyan, danger, parry) and the HUD-like bright pixels keep their hue.</summary>
        public const float V49HighTint = 0f;
        /// <summary>The shadows' lightness offset (URP's w: under 0 it subtracts from the multiplier): -0.12 deepens the dark base (the meter's dark share).</summary>
        public const float V49ShadowOffset = -0.12f;
        /// <summary>A tint multiplier channel is clamped to this range after the mix (a violet tint's blue would be x2.7 at the full share).</summary>
        public const float V49TintMin = 0.55f, V49TintMax = 1.6f;

        /// <summary>ColorAdjustments contrast per level (URP units, -100..100; look33 had none): the lift that pulls the 61-86 % mid band apart (SPIKE.md).
        /// Round 1: 25 / 25 / 15 / 15 / 22. Round 3 (DECIDE49 D-18; the lead's n49-r2 meter: iceline 1/4, kiln 2/4, span 0/4, spire 0/4 on the combat
        /// frame, foundry 4/4): foundry kept; iceline 35, kiln 45, span 35, spire 42 (with the EV below), from the builder's estimate on the n49-r2
        /// look-on room and combat frames (contrast in log space round URP's 0.18 pivot, EV x 2^ev, a saturation lerp: not a Player measure).</summary>
        public static readonly double[] V49Contrast = { 25, 35, 45, 35, 42 };
        /// <summary>Post exposure per level, EV (absolute: look33's base 0 plus this). Round 1: foundry's WICK arena was 29 % bright and 2 % dark,
        /// iceline 7 % bright / 32 % dark, spire 25 % bright: down; kiln and span unchanged. Round 3 (n49-r2: span room dark 25 % mid 73 %, spire
        /// combat dark 7 % mid 82 % under its cyan-lit walls, kiln combat mid 63 %): kiln 0 -> -0.30, span 0 -> -0.10, spire -0.20 -> -0.65;
        /// foundry and iceline kept.</summary>
        public static readonly double[] V49ExposureEv = { -0.30, -0.20, -0.30, -0.10, -0.65 };
        /// <summary>Saturation per level, URP ColorAdjustments units added to the config grade's (look33's were 5 / -10 / 5 / 0 / 10; round 1 8 / 8 / 4
        /// / 8 / 8). Round 2 from the meter on the n49-r1 look-on frames (saturated %, room / floor / arena / boss / combat): foundry 7 / 5 / 12 / 33 / 9
        /// -> 23; iceline 22 / 10 / 57 / 32 / 6 -> 18; kiln 55 / 62 / 49 / 46 / 55 (over the 30 % top) -> -10; span 36 / 43 / 35 / 40 / 5 -> 0;
        /// spire 6 / 1 / 15 / 26 / 25 -> 28. The builder's estimate on those frames (a saturation lerp in linear light, not a Player measure) puts
        /// each level's median frame at 15-25 %. Round 3: kiln -10 -> -30 (n49-r2 room 48 % saturated; with its contrast 45 the estimate gives
        /// room 26 %, combat 11 %, and less of the olive cast); the others kept.</summary>
        public static readonly double[] V49Saturation = { 23, 18, -30, 0, 28 };
        /// <summary>One bloom intensity for every level (UNIFY.md: the bloom is global, never per level, so an emissive tier glows the same everywhere).
        /// The threshold stays lights35's Look35BloomThreshold 1.5 (L's emissive tiers are read against it). x the rank energy lift as before.</summary>
        public const double V49Bloom = 0.5;
    }
}
