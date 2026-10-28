// NIGHT33 builder K (design.n33.kit33): the arena kit's Unity-only numbers (CLAUDE.md rule 5). The web has none of these: its maps have no
// set dressing (no skirt, cornice, truss, landmark ...), no theme palettes for decor and no dressing checker. Pure numbers (no UnityEngine),
// so the harness's `kit33` checker and the level validator read the same values as the view (Kit33Props.cs).
// Palettes are dusk tones only (CLAUDE.md rule 8): no cyan (player-usable), no red-orange except the danger tone (hazard stripes, lava
// channels), no magenta (RIVAL), no pale violet-white (the boundary) on decor.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the theme palettes (a decor row's `tone`): dark / mid / light / metal, per level theme ---
        public const uint Kit33FoundryDark = 0x2B2730, Kit33FoundryMid = 0x433C49, Kit33FoundryLight = 0x675C6C, Kit33FoundryMetal = 0x55505C; // soot and iron
        public const uint Kit33KilnDark = 0x3A2A2C, Kit33KilnMid = 0x55403F, Kit33KilnLight = 0x7A5E57, Kit33KilnMetal = 0x5C4A48;             // fired brick
        public const uint Kit33IceDark = 0x323A4C, Kit33IceMid = 0x4C566E, Kit33IceLight = 0x7C86A2, Kit33IceMetal = 0x5A6274;                 // slate and frost (blue-violet, hue ~225: never cyan)
        public const uint Kit33SpanDark = 0x2E2A38, Kit33SpanMid = 0x484256, Kit33SpanLight = 0x6A6280, Kit33SpanMetal = 0x524E5E;             // girder violet
        public const uint Kit33SpireDark = 0x342D42, Kit33SpireMid = 0x4E4462, Kit33SpireLight = 0x76688E, Kit33SpireMetal = 0x5A5068;         // tower violet
        public const uint Kit33IceCrystalHex = 0x8C96C8;   // ice crystals and stalactites: periwinkle (hue ~230), dimmer than the boundary's violet-white
        public const uint Kit33LampHex = 0xFFD7A0;         // a lamp's / lintel's / edge line's emissive core: warm white (no cyan)
        public const uint Kit33AccentHex = 0xE8C48A;       // a banner's / landmark's accent: pale brass
        public const uint Kit33SnowHex = 0xA8ACC4;         // snow drifts: a grey-lilac, under the boundary's brightness
        public const float Kit33TopLift = 0.18f;           // a decor box's top face is lerped this far toward white (reads its silhouette from above)
        public const float Kit33GradBottom = 0.62f;        // vertical gradient: a decor face's colour at its base (x the tone) ...
        public const float Kit33GradTop = 1.12f;           // ... and at its top: dark at the floor, lighter up high (K1 5.2: the walls stop reading flat)
        public const float Kit33EmissiveScale = 0.55f;     // x the roof strip brightness for decor emissives (lamps, lintels, edge lines: dimmer than L's fixtures)
        public const float Kit33DangerScale = 0.35f;       // x for the danger tone on hazard stripes / lava channels at rest (the hazard view owns the live pulse)
        public const float Kit33InlayScale = 0.25f;        // x for floor inlays: a faint trim, never the brightest thing in the frame (look30 floor strip lesson)

        // --- the shapes ---
        public const float Kit33DoorJamb = 0.4f;           // a door frame's jamb / lintel thickness (K1 #5)
        public const float Kit33CorniceStep = 0.5f;        // the cornice's upper step is this share of its height and sticks out by the full depth
        public const float Kit33TrussChord = 0.25f;        // a truss's chord / web bar size (m)
        public const float Kit33TrussBay = 2.5f;           // a truss's web spacing along its length (m)
        public const float Kit33ChainLink = 0.32f;         // a hanging chain's link length (m)
        public const float Kit33GrateBar = 0.12f;          // a grate's bar width (m) ...
        public const float Kit33GrateGap = 0.3f;           // ... and pitch (m)
        public const float Kit33StripePitch = 0.6f;        // a hazard stripe's band length (m): alternating danger and dark
        public const float Kit33FriezeBelow = 2.0f;        // a landmark row shorter than this (m) is drawn as a frieze (a low area's, above 3 m), not a tower
        public const float Kit33CutMin = 0.02f;            // a part of a box cut by a clear band shorter than this (m) is not drawn
        public const float Kit33CutTouch = 0.005f;         // a band within this (m) of a part's edge only touches it: no cut (float rounding; the lead's 5 mm)
        public const double Kit33RideGap = 0.2;           // m above and below a wall-ride strip that no wall piece covers (dress33's `gap`; the checker's band)
        public const float Kit33CapLift = 0.3f;           // a chimney's cap height (m)

        // --- the kit33 checker's rules (K1 section 7 / 8; the harness's `kit33` and LevelValidate29's decor rows) ---
        public const double Kit33DensityMin = 8, Kit33DensityMax = 14;  // decor pieces per 100 m2 of room floor (K1 7) ...
        public const double Kit33DensityFailLow = 6, Kit33DensityFailHigh = 18; // ... RED outside these
        public const double Kit33DensityMinArea = 150;     // the density band is graded for areas this big or bigger (a 12 x 12 m room is mostly wall: skirt,
                                                           // cornice, corner pilasters and frames alone pass 18 per 100 m2)
        public const double Kit33HighShare = 0.40;        // share of decor with min y >= Kit33HighY (K1 7)
        public const double Kit33HighY = 3.0;              // "above head height" (m)
        public const double Kit33WallBand = 1.5;           // decor below Kit33HighY must lie within this of a wall (K1 8.2) or be flush
        public const double Kit33FlushH = 0.05;            // a flush floor piece is at most this tall (K1 8.9)
        public const double Kit33WallPieceEvery = 8.0;     // a rib / recess / pilaster at least every this many metres of wall (K1 7)
        public const double Kit33CoverReach = 8.0;         // every floor cell within this of cover (K1 7) ...
        public const double Kit33CoverCell = 1.0;          // ... sampled on this grid (m)
        public const double Kit33WallFlat = 0.65;          // a decor piece whose far face is within this of its wall is "flat on the wall": no lane clutter (no collider)
        public const double Kit33LaneHalf = 1.5;          // the clear lane's half width (K1 7: 3 m) from each entrance to the exit / ramps
        public const double Kit33CoverMinM2 = 40;          // cover: at least 1 piece per this many m2 of open floor ...
        public const double Kit33CoverOpenMin = 100;       // ... counted only for rooms with this much floor (smaller rooms: the reach rule only)
        public const double Kit33DecorOutside = 6.0;       // a decor row may stand this far outside its room's rect (the 2 m wall ring + a chimney / landmark band)
        public const int Kit33DrawBoxCap = 400;            // decor draw boxes per room (K1 8.13)
        public const double Kit33EyeY = 1.7;               // the landmark's sight ray starts at the entry's feet + this (K1 8.6)
        public const double Kit33ReelLookUp = 0.35;        // the kit33 reel's -b frame pitch (rad, + is up): the decor above 3 m in view
    }
}
