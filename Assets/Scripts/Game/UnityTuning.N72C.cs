// NIGHT72 builder C (COLOUR & DECOR): the Unity-only numbers of palette72 (Game/Palette72.cs) and decor72 (Game/Decor72.cs), each with its reason
// (CLAUDE.md rule 5). View only: nothing here touches the sim. The web has no such numbers (its stone is two colours and its levels have no palette).
// The gameplay-facing choice (which levels, which role) is the slate's C2 / C3; these are render numbers.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---------------- palette72 (Game/Palette72.cs): floor / wall / accent per level ----------------

        /// <summary>palette72: the levels with a palette, in the order of every Palette72* table below (Decor54.KeyOf's keys: the level id after its "nNN-"
        /// prefix, so n52-spire, n67-spire and the play build's rebuilt rooms share a row). A level not listed keeps its stone.</summary>
        public static readonly string[] Palette72Levels = { "foundry", "iceline", "kiln", "span", "spire" };

        /// <summary>palette72: per level, the FLOOR multiplier (r, g, b) on the stone's vertex colours (linear albedo: Stone.shader multiplies the light
        /// by it), so the lit, graded floor moves to a warm-neutral sand at its own brightness (hue 36-42, S 0.25-0.33: the walkable surface is one
        /// language on all five levels, and out of every role band: yellow56's S cap and the parry arc 45-70 stay clear). Tools/night/n72-c-calibrate.py
        /// on the n71-forge72 baseline frames (m = (target / current) ^ 2.2 per channel, clamped 0.25-4; Research/2026-10-27-wave72/C/calibration.txt).
        /// The stone's vertex colours are Float32 (Mesh.SetColors(Color[]); the n72-h0 Player logs show no decor54 re-tone), so a multiplier above 1 is kept. A hypothesis until a Player frame shows it (L-094): the lead's pair reads the result with Tools/night/n72-c-values.py.</summary>
        public static readonly float[] Palette72FloorMul =
        {
            1.02f, 0.96f, 1.50f, // foundry: tan (hue 50, S 0.36: inside the parry arc) to sand (hue 40, S 0.27)
            1.88f, 0.96f, 0.29f, // iceline: blue-grey (hue 222) floor to sand (hue 38): warm floor, cold walls
            0.75f, 1.10f, 1.06f, // kiln: salmon (hue 17, S 0.36: inside the danger band) to sand (hue 36)
            1.02f, 1.12f, 0.33f, // span: lavender (hue 266) floor to sand (hue 40)
            1.27f, 0.65f, 3.98f, // spire: olive (hue 80) floor to sand (hue 42, S 0.27) at luma 95 (from 105): its light is blue-starved (the floor's blue is 40
                                 // on screen), so the blue takes the multiplier's cap (4.0) and the target's luma comes down a little; the walls stay at 35
        };

        /// <summary>palette72: per level, the WALL multiplier on every side face's vertex colour (after decor54's wall ramp), so each level's walls carry
        /// its own hue at their own brightness: foundry oxide green (hue 150), iceline navy (225), kiln plum (272), span indigo (245), spire bottle green
        /// (135). Never a role hue (cyan 160-205, danger 345-35, parry 45-70, magenta 285-335). Same calibration as the floors.</summary>
        public static readonly float[] Palette72WallMul =
        {
            0.71f, 1.05f, 1.19f, // foundry
            1.22f, 0.93f, 1.22f, // iceline
            1.36f, 0.86f, 1.30f, // kiln
            0.92f, 0.95f, 1.53f, // span
            1.23f, 0.93f, 2.92f, // spire
        };

        /// <summary>palette72: per level, the ONE accent when it differs from colour56's (0: colour56's stays). The spire's was a rose (0xe67793, hue 345:
        /// inside the danger band 345-35: GG 2026-10-26 "colors ... I just haven't seen it", gg-proxy #2 "dark-red stripes ... red is supposed to mean
        /// danger"): a bone ivory (0xe8dcc0, hue 42, S 0.17: the spire is a ship's bones; pale, never yellow, never a role hue) on the bottle-green walls.</summary>
        public static readonly uint[] Palette72Accent = { 0, 0, 0, 0, 0xE8DCC0 };

        /// <summary>palette72: a ramp (a wedge's slope, normal.y under this) is lifted by Palette72SlopeLift so it reads against the flat floor beside it
        /// (the spire's green ramp and green floor were one colour); a flat top above Palette72DeckMinY (a ledge / deck you stand on) by Palette72DeckLift;
        /// a top under Palette72PitMaxY (a pit's floor) is darkened by Palette72PitDark. Linear albedo multipliers on the floor's.</summary>
        public const float Palette72FlatMinNy = 0.5f, Palette72SlopeMaxNy = 0.97f, Palette72SlopeLift = 1.18f;
        public const float Palette72DeckMinY = 0.35f, Palette72DeckLift = 1.10f;
        public const float Palette72PitMaxY = -1.0f, Palette72PitDark = 0.55f;

        /// <summary>palette72: the accent scan finds the level's accent pieces (decor54's moved trims, rims, door frames, roof strips, prop collars, the
        /// string course) by their colour: hue within Palette72AccentHueDeg of decor54's accent and saturation at least Palette72AccentSatShare of it.</summary>
        public const float Palette72AccentHueDeg = 8f, Palette72AccentSatShare = 0.6f;

        /// <summary>palette72: the accent scan runs this many unscaled seconds after decor54's second pass (3 s after the map): every piece of a level is
        /// built by then (the frieze at 6 frames, the string course at 5, the fixtures at the map's build); once more at the second time, for a late view.</summary>
        public const float Palette72AccentScanS = 0.5f, Palette72AccentRescanS = 4f;

        // ---------------- colours63 made right (Game/Colours63*.N63D.cs, slate C1): a level accent that wears danger ----------------

        /// <summary>colours72: a level whose accent's hue lies within this many degrees of the danger band's edges (345 - m .. 35 + m) wears danger on a dark wall: 15
        /// takes the spire's rose (hue 345.0) and leaves every other level's accent (foundry violet 265, iceline periwinkle 229, kiln blue 222, span indigo 252).</summary>
        public const double Colours72DangerMarginDeg = 15;

        /// <summary>colours72: the hue such an accent goes to: 40 (amber-bone, S at the world tier's 0.25: off every role band, not the boundary's pale violet-white
        /// (hue 230-300), not the parry yellow (45-70)), the same hue family as palette72's spire accent (0xe8dcc0, hue 42).</summary>
        public const double Colours72ExitHueDeg = 40;

        /// <summary>colours72: a material is the accent when its hue is within this many degrees of it, its saturation at least this share of it, its value at least this
        /// (the dark-red stripes were V 0.36).</summary>
        public const double Colours72AccentHueDeg = 8, Colours72AccentSatShare = 0.6, Colours72AccentMinValue = 0.2;

        // ---------------- decor72 (Game/Decor72.cs): the band segments that keep a job ----------------

        /// <summary>decor72: a band segment (a 4 m piece of a wall box's face at the probe beside it) is a ROUTE mark within this many metres (xz) of a door's
        /// rect (campaign.doors): the door and its surround, not the whole room (7 m kept 64 % of the foundry's segments, 4 m 43 %: Tools/night/n72-c-marks.cjs).</summary>
        public const double Decor72RouteReachM = 4;

        /// <summary>decor72: a HAZARD mark within this many metres of a pit's rect (campaign.pits) or the lava's (the json's lava[]): the edge, not the
        /// room (3 m kept 331 of the foundry's 1428 segments as hazard marks, 2 m 212).</summary>
        public const double Decor72HazardReachM = 2;

        /// <summary>decor72: a map wall box is a ride panel when it lies inside a ride-tagged campaign box grown by this many metres (the map's boxes are the
        /// generator's boxes cut by the room walls: RideTrim36 reads the campaign list, the band reads the map's).</summary>
        public const double Decor72RideEps = 0.6;

        // ---------------- decor72, C5 blank walls: a bare face's value structure, and the route chevrons on ramps ----------------

        /// <summary>decor72 (C5): a wall face at least this tall (m) gets a value structure: its foot a little darker (the plinth) and its upper half lighter, so a big
        /// face is no longer one flat colour (gg-proxy #9: iceline g3's right half one blank navy wall 34 % of the frame, foundry a8's bare slab 24 %: Tools/night/
        /// n72-c-values.py's blank share, T 14 / step 8). decor54's own ramp runs over a fixed 9 m band, so a 3-5 m face barely moves (t^1.6 of the band).</summary>
        public const float Decor72FaceMinH = 2.5f;

        /// <summary>decor72 (C5): over the face's foot (relative height 0 .. Decor72PlinthT) the vertex colour x Decor72PlinthMul rising to 1; from Decor72UpperFromT to the
        /// top it rises (smoothstep) to x Decor72UpperMul. A wall at luma 45 is then ~36 at the foot and ~65 at the top: two or three bands of a 16 px block mean more than
        /// the meter's 14 apart (the flat region splits), without a new object. The upper lift matters more than the plinth (GG: "very dark" rooms): the foot loses 20 %,
        /// the top gains 45 %.</summary>
        public const float Decor72PlinthT = 0.15f, Decor72PlinthMul = 0.8f, Decor72UpperFromT = 0.4f, Decor72UpperMul = 1.45f;

        /// <summary>decor72 (C5): a ramp (a wedge) is a route, the one way up for the player and for an enemy (GG 2026-10-26 "OK now you have to go up"; NIGHT20 "a route
        /// enemies can climb"): chevrons pointing up its slope in the level's accent. One chevron per Decor72ChevronEveryM metres of slope (at least 1, at most
        /// Decor72ChevronMax), each Decor72ChevronWidthM wide across at most 60 % of the ramp's width, the arms Decor72ChevronArmM thick and leaning back
        /// Decor72ChevronLeanM; drawn Decor72ChevronLiftM above the slope (a thin quad: 0.025 m at 100 m is well over the depth buffer's step, 0.5 mm at 30 m).</summary>
        public const double Decor72ChevronEveryM = 3.5;
        public const int Decor72ChevronMax = 3;
        public const float Decor72ChevronWidthM = 1.5f, Decor72ChevronArmM = 0.16f, Decor72ChevronLeanM = 0.45f, Decor72ChevronLiftM = 0.025f;

        /// <summary>decor72 (C5): the chevrons' largest colour channel (their accent's hue and saturation kept). The first cut scaled the accent to 0.55 of the usable cyan's
        /// luminance through Decor54.Recolour and came out near black on the n72-t4 pair (Colour56.Clamp holds an accent piece at 0.6 x 0.45 = 0.27). 0.8 reads as the accent
        /// on a dark ramp and stays far under the usable cyan's neon (HDR, luminance ~2.0 on that frame): cyan is still the brightest line.</summary>
        public const float Decor72ChevronMaxChannel = 0.8f;

        /// <summary>decor72 (C5): the chevrons are built this many frames after a map is seen (after the map, its kits and decor54's first pass).</summary>
        public const int Decor72ChevronFrames = 12;
    }
}
