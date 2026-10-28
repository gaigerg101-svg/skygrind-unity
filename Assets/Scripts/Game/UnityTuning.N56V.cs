// NIGHT56 builder V: Unity-only numbers of colour56 / rim56 / yellow56 and the a356 reel (render, geometry, capture); each with its reason.
// The levels' hexes and every gameplay-facing number are config (design.n56.<row>.numbers, written by Tools/night/n56-v-rows.cjs).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---------------- colour56 (Game/Colour56.cs): the level key's frieze ----------------

        /// <summary>colour56: frames after a map is seen before the frieze is built (the map, its world and its boxes are up; decor55's is 5).</summary>
        public const int Colour56SettleFrames = 6;
        /// <summary>colour56: the frieze's bottom, m over the floor beside the wall: above decor55's string course (2.7-3.02 m) with a 0.4 m gap of
        /// stone between (two lines, two colours, never one stripe), above the eye (1.6-1.7 m) and the waist-high cyan ride strip.</summary>
        public const double Colour56FriezeY = 3.4;
        /// <summary>colour56: the frieze's height (m): at 10 m it spans ~10-19 deg above the eye (~130 px at 1080p), so one wall in an entry view carries
        /// the key on several % of the frame (V1's doneWhen: key or accent >= 3 %); walls are 8-12 m, so most of each wall stays stone.</summary>
        public const double Colour56FriezeH = 1.4;
        /// <summary>colour56: how far the frieze stands proud of the face (m): a slab whose edges catch the light at a grazing angle.</summary>
        public const double Colour56FriezeOut = 0.05;
        /// <summary>colour56: a face is cut into segments of at most this (m), each with its own floor sample (decor55's).</summary>
        public const double Colour56FriezeSegment = 4;
        /// <summary>colour56: a moved decor piece's largest colour channel at most this (Colour56.Clamp, Decor54.Recolour's hook): 1.0 keeps the accent
        /// in the tonemapper's near-linear range, so the hue on the frame is the number's (decor54 scaled the accent to the cyan piece's HDR luminance:
        /// a violet at cyan's luminance needs its blue channel at ~2.5).</summary>
        public const float Colour56AccentMaxChannel = 1.0f;
        /// <summary>colour56: faces shorter than this (m) get no frieze (posts, the ends of thin walls).</summary>
        public const double Colour56FriezeMinLen = 1.0;
        /// <summary>colour56: the floor probe stands this far out from the face (m), clear of its own collider (decor55's).</summary>
        public const double Colour56FriezeProbe = 0.35;
        /// <summary>colour56: a floor up to this far (m) under the box's bottom still counts (decor55's).</summary>
        public const double Colour56FriezeFloorBelow = 3;
        /// <summary>colour56: the frieze stays at least this far (m) under the box's top (the wall-top rim keeps its own line).</summary>
        public const double Colour56FriezeTopClear = 0.6;

        // ---------------- rim56 (Game/Rim56.cs): skin52's globals while the row is on (render numbers; skin52's in UnityTuning.U52.cs) ----------------

        /// <summary>rim56: the enemy body's shadow side multiplier (linear; skin52's 0.20 / 0.15 / 0.30, a cool near-black): warm and brighter, so the
        /// danger token's shadow is ~#C4260A (hue ~9, S ~0.95, V ~0.77 in sRGB) instead of ~#7C1A0C (V ~0.48): the body one warm mass, its hue kept.
        /// Round 2 (n56-r3: 0.55 / 0.30 / 0.26 moved the bodies' mean V by +0.01): 0.70 / 0.38 / 0.30.</summary>
        public const float Rim56ShadeR = 0.70f, Rim56ShadeG = 0.38f, Rim56ShadeB = 0.30f;
        /// <summary>rim56: the lit side x this (skin52's 1.0; beyond skin52's 20 m the whole body is the lit side): the red channel past the clip, the
        /// green and blue lifted with it, so a darker body kind gains value and the danger token goes a little orange (still 5-35). Round 2 (n56-r3:
        /// 1.2 moved the bodies' mean V +0.01: R already at the clip): 1.5.</summary>
        public const float Rim56LitGain = 1.5f;
        /// <summary>rim56 round 2: the enemy body material's own far rim (_RimColor, fx.enemyRimColor x strength x the crunch boost; EnemyLook.body)
        /// x this while the row is on (skin52's near cut and rim56's near share apply under 14 m; this is the rim every enemy past it wears).</summary>
        public const float Rim56FarRimGain = 1.5f;
        /// <summary>rim56 round 2: the far rim's power (fx.enemyRimPower 2.2): 1.7, a wider warm band on the silhouette at fighting range.</summary>
        public const float Rim56FarRimPower = 1.7f;
        /// <summary>rim56: the warm rim's share at skin52's near distance (3 m; skin52's 0.3): a visible warm band on the silhouette up close, under
        /// the far rim's full strength (skin52 cut it from 1.0 for the "jelly" glow at arm's length: half way back, at a thinner power below).</summary>
        public const float Rim56RimNearShare = 0.55f;
        /// <summary>rim56: the rim's power at the near distance (skin52's 5; fx.enemyRimPower 2.2 far): 3.5, a band a little wider than skin52's edge.</summary>
        public const float Rim56RimNearPower = 3.5f;

        // ---------------- yellow56 (Game/Yellow56.cs): the floors' pass ----------------

        /// <summary>yellow56: frames after a map is seen before the floors' pass (after decor54's first pass at its 3 and colour56's frieze at 6).</summary>
        public const int Yellow56FloorFrames = 8;
        /// <summary>yellow56: every this many frames the probe vertex is checked (a rewrite of the stone puts the floors back: re-applied).</summary>
        public const int Yellow56FloorRecheck = 30;
        /// <summary>yellow56: a vertex is a floor's when its world normal's y is over this (decor54's side faces are |y| < its Decor54SideMaxNy).</summary>
        public const float Yellow56FloorMinNy = 0.7f;

        // ---------------- the a356 reel (FeelReel.N56.Colour.cs) ----------------

        /// <summary>a356: the sky grid's columns and rows (one ray per cell into the C# world: a miss within Colour56SkyRayM = open sky). 96 x 54 cells
        /// are 20 x 20 px at 1080p: the meter's sky mask (n56-colour.py) reads upper walls and ceilings as world, not sky.</summary>
        public const int Colour56SkyCols = 96, Colour56SkyRows = 54;
        /// <summary>a356: a sky ray that hits nothing within this (m) is open sky (the campaign levels are under 300 m across).</summary>
        public const double Colour56SkyRayM = 400;
    }
}
