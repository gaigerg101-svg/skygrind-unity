// NIGHT43 builder A1 (models43, a view row: design.n43.models43, N43View.RowOn; NIGHT43.md card A1; GG's G21 / G23; DECIDE43 D-4: the enemy models
// rebuilt in the pill language, CLAUDE.md rule 7's "the web's blocky models as they are" retired for the models): Unity-only render numbers of the
// rebuilt enemies (Game/Enemy43View.cs, Game/Mesh43.cs). No gameplay number lives here (no part has a collider; the hit shapes are the sim's).
// The rules (Research/2026-10-16-wave43/research/ui-art-web.md section C; TF2 / Darktide): colour first (the enemy palette: a DARK body, a BRIGHT
// visor, yellow only where parryable), then silhouette (every kind keeps its pill; its class is in the TOP THIRD: a visor shape + one back part:
// slit = the husk, three lenses = the swarmer, a round lens + a tank = the spitter, a chevron brow + shoulder plates = the charger (the
// mini-ICEBREAKER's heavy language), a ring of lenses = the drone, a plate visor + a framed shield = the warden, a double slit + pauldrons = the
// tank), then detail (the chest plate, the gorget, the bezels and straps). Smooth meshes only (Mesh43: superellipsoids, lathes, visor patches).
// A base swap: each view's own renderers keep their transforms and draw a Mesh43 mesh in the same unit space instead of the primitive (the order of
// a kind's *Base43 array is the view's renderer order). An extra part: { mesh (Mesh43 id), anchor (0 the body's root: y x the kind's height;
// 1..n the view's renderer n-1's transform, in its own unit space; 100 + k the k-th extra part), x, y, z, scale x, y, z, euler x, y, z (deg), role,
// group, tell pitch (deg on x in the wind-up), tell grow (x 1 + this in it) }.
// Roles (Enemy40Parts.Pose): 0 DARK (armour: x Models43Dark), 1 BRIGHT, 2 DANGER (red-orange: an ember at rest, lit in the wind-up), 3 PARRY
// (yellow: only where the player can parry), 4 WEAK, 5 VISOR (Models43VisorHex: always lit; hotter in the wind-up; it gutters while stunned),
// 6 TRIM (the plates' edges: the enemy colour, a step brighter than the darkened body). Group: 0 always, 1 a husk only, 2 a tank only.
// Renderer budget (Models43Tests): husk 6, tank 6, swarmer 5, spitter 6, charger 5, drone 6, warden 7.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int Models43Stride = 15; // floats per extra part (the layout above)
        // round 2 (n43-r2's line-up at 30 m, measured with the silhouette sheet's mask, CIE L*: the bodies 51-62 against the dusk horizon 50-57
        // behind them, OFF and ON alike: 1-6 L* apart, no greyscale read (DESIGN-RULES E-3)): the body x 0.35 (about L* 35-40, a dark mass
        // against the bright horizon band; the rim and the visor carry its edge and face against the dark floor)
        public const float Models43BodyValue = 0.35f; // the body's colour x this while models43 draws (dark body, bright visor: the value hierarchy)
        public const float Models43Dark = 0.42f; // DARK armour: x the (already darkened) body colour; near black, still warm, against the sky
        public const float Models43Trim = 1.8f; // TRIM: x the darkened body colour (about 0.63 of the web's enemy orange: the plates' edges read)
        public const uint Models43VisorHex = 0xFF9A3C; // the visor: a hot amber orange (not the parry yellow 0xFFD23C, not the danger red)
        public const float Models43VisorRest = 1.7f; // the visor at rest: x its colour (HDR: the bloom picks it up at range)
        public const float Models43VisorTell = 3.2f; // ... in the wind-up (and toward white: Models43VisorTellWhite)
        public const float Models43VisorTellWhite = 0.35f; // the wind-up visor's lerp toward white
        public const float Models43VisorStun = 0.35f; // a stunned enemy's visor: x this, flickering (Models43VisorStunHz): it is open, not looking
        public const float Models43VisorStunHz = 9f;
        public const float Models43Form = 1f; // SKYGRIND/Enemy's _Form while models43 draws: the sky-from-above shade on the smooth meshes (0 = the web's flat colour)

        // The base swaps (Mesh43 ids in each view's renderer order).
        public static readonly int[] Models43HuskBase = { Mesh43.Pill, Mesh43.Helmet }; // the body capsule, the head box
        public static readonly int[] Models43SwarmerBase = { Mesh43.SoftBox, Mesh43.Helmet, Mesh43.Pauldron }; // the body, the head, the legs (a carapace skirt)
        public static readonly int[] Models43SpitterBase = { Mesh43.Pill, Mesh43.Ball, Mesh43.Helmet }; // the body, the acid sac, the front box (its head)
        public static readonly int[] Models43ChargerBase = { Mesh43.BodyBox, Mesh43.Brow, Mesh43.ShieldSlab }; // the ram body, the head plate (a heavy brow), the back plate (a curved weak plate)
        public static readonly int[] Models43WardenBase = { Mesh43.Pill, Mesh43.ShieldSlab, Mesh43.SoftBox }; // the body, the shield, the arm
        // the drone's two renderers are in metres (the view scales Mesh43.Saucer to drone.radius; the core is Mesh43.Ball at Models43DroneCore x the radius)
        public const float Models43DroneCore = 1.0f; // the core ball's diameter x drone.radius (its glow reads below the saucer)
        public const float Models43DroneCoreY = -0.62f; // the core's centre x drone.radius (it hangs under the belly: the charge-up is seen)

        // husk (height 1.8; the head box 0.62 x 0.45 x 0.56 at 1.5 m, tilted 20 deg): a SLIT visor across the helmet, a gorget at the shoulders, a
        // chest plate (TF2's brightest band at chest height). The tank (group 2: the same body x 1.4): a double slit, pauldrons, an exhaust stack.
        public static readonly float[] Models43Husk =
        {
            Mesh43.VisorSlit, 2, 0, 0, 0, 1, 1, 1, 0, 0, 0, 5, 1, 0, 0,
            Mesh43.VisorSlit2, 2, 0, 0, 0, 1, 1, 1, 0, 0, 0, 5, 2, 0, 0,
            // the gorget (round 2: n43-r2's line-up read the 1.25-wide flat ring as a hat brim): tighter to the body and deeper, a collar
            Mesh43.Ring, 0, 0, 0.73f, -0.02f, 1.02f, 1.1f, 1.0f, 12, 0, 0, 0, 0, 0, 0,
            Mesh43.Pauldron, 0, 0, 0.58f, 0.32f, 0.56f, 0.25f, 0.34f, 90, 0, 0, 6, 1, 0, 0,
            // its back part: two swept spines off the shoulder blades (DANGER: an ember at rest, lit and raised in the melee wind-up: a tell in the top third)
            Mesh43.Fins2, 0, 0, 0.8f, -0.3f, 0.6f, 0.42f, 0.38f, -15, 0, 0, 2, 1, -25, 0.15f,
            Mesh43.Pauldrons2, 0, 0, 0.75f, 0, 1.3f, 0.5f, 0.75f, 0, 0, 0, 6, 2, 0, 0,
            Mesh43.Pill, 0, 0, 0.8f, -0.4f, 0.24f, 0.22f, 0.24f, -12, 0, 0, 0, 2, 0, 0,
        };

        // swarmer (height 0.75; a low crawler of three blocks): three lenses on its head (VISOR), two swept fins on its back (DANGER: lit in the crouch)
        public static readonly float[] Models43Swarmer =
        {
            Mesh43.Eyes3, 2, 0, 0.05f, 0.42f, 0.7f, 0.55f, 0.5f, 0, 0, 0, 5, 0, 0, 0,
            Mesh43.Fins2, 0, 0, 0.85f, -0.12f, 0.5f, 0.45f, 0.35f, 0, 0, 0, 2, 0, -20, 0.2f,
        };

        // spitter (height 2): a ROUND LENS on its head (VISOR) in a bezel, two straps round its acid tank (the sac: its globs are parryable, the view
        // keeps the sac's own acid / yellow-hot colours)
        public static readonly float[] Models43Spitter =
        {
            Mesh43.Ball, 3, 0, 0.02f, 0.45f, 0.62f, 0.72f, 0.35f, 0, 0, 0, 5, 0, 0, 0,
            Mesh43.Ring, 3, 0, 0.02f, 0.46f, 0.85f, 0.95f, 0.85f, 90, 0, 0, 6, 0, 0, 0,
            Mesh43.Rings2, 2, 0, 0, 0, 1.12f, 1.0f, 1.12f, 0, 0, 0, 6, 0, 0, 0,
        };

        // charger (height 2.2; the low long ram of models40): a CHEVRON visor under its brow (VISOR), shoulder plates (TRIM)
        public static readonly float[] Models43Charger =
        {
            Mesh43.VisorChevron, 2, 0, 0, 0, 1, 1, 1, 0, 0, 0, 5, 0, 0, 0,
            Mesh43.Pauldrons2, 0, 0, 0.6f, 0.5f, 1.95f, 0.55f, 0.85f, 0, 0, 0, 6, 0, 0, 0.08f,
        };

        // drone (its root is its centre; y in m): a band round the saucer's rim (TRIM), three lenses on the rim (VISOR), the emitter under it
        // (PARRY yellow, full in its charge-up), a short antenna on top (DARK)
        public static readonly float[] Models43Drone =
        {
            Mesh43.Ring, 0, 0, 0, 0, 1.32f, 0.55f, 1.32f, 0, 0, 0, 6, 0, 0, 0,
            Mesh43.Lenses3, 0, 0, 0.05f, 0, 1.26f, 1.26f, 1.26f, 0, 0, 0, 5, 0, 0, 0,
            Mesh43.Spike, 0, 0, -0.8f, 0, 0.2f, 0.42f, 0.2f, 180, 0, 0, 3, 0, 0, 0.5f,
            Mesh43.Spike, 0, 0, 0.5f, 0, 0.07f, 0.36f, 0.07f, 0, 0, 0, 0, 0, 0, 0,
        };

        // warden (height 2.3): its shield in a FRAME (TRIM, on the shield), a helmet (DARK) with a PLATE visor (VISOR), pauldrons (TRIM)
        public static readonly float[] Models43Warden =
        {
            // the frame DARK (round 2: n43-r2's line-up showed a glowing TRIM frame drowning the shield; dark iron round the lit panel reads as a frame)
            Mesh43.ShieldFrame, 2, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
            // the helmet sits on the capsule's top (its cap is 1.5 m wide: the helmet at 1.02 h, half above it); the pauldrons at 0.82 h, outside the cap
            Mesh43.Helmet, 0, 0, 1.02f, 0.08f, 0.7f, 0.55f, 0.66f, 0, 0, 0, 0, 0, 0, 0,
            Mesh43.VisorPlate, 101, 0, 0, 0, 1, 1, 1, 0, 0, 0, 5, 0, 0, 0,
            Mesh43.Pauldrons2, 0, 0, 0.82f, 0, 2.1f, 0.6f, 1.0f, 0, 0, 0, 6, 0, 0, 0,
        };
    }
}
