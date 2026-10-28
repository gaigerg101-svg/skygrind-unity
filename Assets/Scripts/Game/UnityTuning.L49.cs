// NIGHT49 builder L (design.n49.lights49, a VIEW row; G2-G4, the light side): the level lights in their roles. Sizes, distances and tiers only;
// every colour is a Look49 role token (UnityTuning.Look49.cs) or the level's own theme key (Look41.KeyHex), never typed here. View only: nothing
// here touches the sim. The harness keeps a COPY of these (Tools/SimHarness/Harness.N49.Lights.cs: the harness compiles no render file) and its
// `n49lights` command checks that the copy still matches this file. Unity-free plain numbers.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>A fixture is USABLE (cyan) when its drawn box (or, with no box, its plan socket point) is within this many metres (3D box gap) of a
        /// jump pad (json `pads`), a grapple node (json `grappleNodes` and a campaign room's own `grappleNodes`), a ride panel (a campaign.boxes `wall`
        /// with `ride: true`, RideTrim36's rule) or a climb step (a campaign.boxes row with `job: "climb"`). The brief's test (NIGHT49-BRIEF section 4 L).</summary>
        public const double L49UsableReachM = 1.5;

        /// <summary>A floor or wall fixture is on THE BOUNDARY (pale violet-white) when its box is within this many metres of a fall edge: a non-lava
        /// campaign pit (a dry pit or the void), as the box from the pit's floor up to the room floor (y 0). Ceiling panels never count (they light;
        /// they mark no edge). Same reach as the usable test, so both roles are read at the same scale.</summary>
        public const double L49BoundaryReachM = 1.5;

        /// <summary>The bloom threshold the emissive tiers below are set against: lights35's (PostFx: Look35BloomThreshold 1.5, gamma; URP compares
        /// the max channel). HAND-OFF: when V's look49 sets its own threshold per level, this follows it (one number for both).</summary>
        public const double L49BloomThreshold = Look35BloomThreshold;

        /// <summary>The emissive HDR of a USABLE fixture's lens, x the bloom threshold: it blooms (cyan's max channel 0.90 x 2.0 = 1.8 x the
        /// threshold). With the danger tier the only fixtures that glow past the threshold by this much (ULTRAKILL / DOOM: tell loud, tell fast).</summary>
        public const double L49TierUsable = 2.0;

        /// <summary>The boundary's lens HDR x the threshold: just over it (a thin pale halo: "the edge is here"), well under the usable tier.</summary>
        public const double L49TierBoundary = 1.15;

        /// <summary>The danger (orange: braziers, lava pools) lens HDR x the threshold: 2.0 x 1.5 = 3.0, look33's Look33BrazierHdr kept.</summary>
        public const double L49TierDanger = 2.0;

        /// <summary>Every other fixture (the level accent: room panels, sconces, strips, floods, poles, trims): its lens HDR x the threshold stays
        /// UNDER it (no halo): a light for seeing by, not a signal. The theme keys' max channel is 1.0, so 0.9 x 1.5 = 1.35 < 1.5.</summary>
        public const double L49TierDim = 0.9;

        /// <summary>A level accent paler than this HSV saturation reads no hue (Look41PaleSat's value): it passes the role guard whatever its hue.
        /// A theme key that is more saturated AND within Look49AccentMinHueGapDeg of a role token is desaturated to this (hue and value kept).</summary>
        public const double L49PaleSat = 0.15;

        /// <summary>The landmark light stands this many metres over its target when the target is a boss room's centre / axis or the room's middle.</summary>
        public const double L49LandmarkLiftM = 6;

        /// <summary>The landmark light hangs this many metres under a landmark PROP's centre (a truss or a beam high on the far wall lit from below).</summary>
        public const double L49LandmarkPropDropM = 2;

        /// <summary>The landmark light's range (m): a boss room is 28-68 m across; the light reaches the landmark's whole silhouette, not the room.</summary>
        public const double L49LandmarkRangeM = 30;

        /// <summary>The landmark is LIT AS THE ROOM'S STRONGEST LIGHT: at the target, its own light's illuminance (I / d^2) is this many times the
        /// largest any other light in the room puts there (re-measured every LightKitRepick).</summary>
        public const double L49LandmarkOverRoom = 1.5;

        /// <summary>The landmark light's intensity floor and cap (before UnityTuning.LightScale): the floor keeps it lit in a room whose other lights
        /// do not reach the target; the cap keeps a 50 m-tall room's key from driving it to a white-out.</summary>
        public const double L49LandmarkMinIntensity = 8, L49LandmarkMaxIntensity = 60;

        /// <summary>Round 2 (the lead's n49-r1 a449: the landmark capped at 19.10 = 60 / pi, 6 m out: 0.531 at the landmark vs the room's 0.550 / 9.026):
        /// when the cap cannot reach L49LandmarkOverRoom from L49LandmarkLiftM, the light moves in toward its target (the same line), down to this
        /// distance (m), so the landmark is its own light's strongest without a white-out over the room.</summary>
        public const double L49LandmarkMinLiftM = 1.0;

        /// <summary>The reel a449's pass mark: the landmark's own illuminance at the landmark at least this many times the room's next strongest
        /// (the lead's round-2 rule, x1.2; the light aims at L49LandmarkOverRoom).</summary>
        public const double L49LandmarkPassRatio = 1.2;

        /// <summary>A non-lava pit is a FALL EDGE (the boundary test) only when its floor is at least this many metres under the room floor: k5's whole
        /// floor is a 0.5 m "pit" under the ring's own floor box (NIGHT46 B1), not an edge.</summary>
        public const double L49FallMinDepthM = 1.0;
    }
}
