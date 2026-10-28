// NIGHT40 builder E3 (models40, a view row: design.n40.models40.enabled; NIGHT40.md card E3): Unity-only render numbers of the silhouette parts every enemy
// kind gains (Game/Enemy40View.cs). No gameplay number lives here (no part has a collider; the hit shapes are the sim's). The rules (research/enemies-web.md
// section 4): a different black outline per kind; the defining part on the head, shoulders or back (Darktide: head and shoulders read most, back attachments
// for the long-range ones); squares for heavies, wedges for fast or dangerous ones; the brightest value at chest height (TF2); red-orange only for danger
// (eyes, the wind-up glow), YELLOW for the DRONE's parryable emitter; the rim (EnemyLook's rim term) on every part, against the dusk sky.
// A part: { prim (0 cube, 1 sphere, 2 cylinder), x, y (x the kind's height; the DRONE's root is its centre: y in m), z (Unity +z = the enemy's front),
//           scale x, y, z (m), euler x, y, z (deg), role, group, tell pitch (deg added on x during the wind-up), tell grow (x 1 + this during it) }.
// Roles: 0 DARK (the body x Models40Dark: the silhouette's mass), 1 BRIGHT (the body toward white: the chest), 2 DANGER (red-orange: dim at rest, full in
// the wind-up), 3 PARRY (yellow: dim at rest, full in the charge-up), 4 WEAK (bone; pulses while stunned). Group: 0 always, 1 a husk only, 2 a tank only
// (the husk view draws both on one body). Renderer budget: each model at most 2x today's renderers (Models40Tests): husk / tank 2 -> 4 (the two groups
// share the body), swarmer 3 -> 5, spitter 3 -> 6, charger 3 -> 6, warden 3 -> 6, drone 2 -> 4.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int Models40Stride = 14; // floats per part (the layout above)
        public const float Models40Dark = 0.45f; // DARK parts: x the body colour (the outline's mass reads black against the sky first)
        public const float Models40Bright = 0.45f; // BRIGHT parts: the body lerped this far to white (the chest, TF2's brightest band)
        public const float Models40GlowRest = 0.35f; // DANGER / PARRY parts at rest: x their colour (an ember, not a tell)
        public const float Models40GlowTell = 2.2f; // ... in the wind-up: x their colour (HDR; the bloom picks it up)
        public const uint Models40WeakHex = 0xE8DCC4; // WEAK parts: bone (not cyan: cyan is the player's; not yellow: yellow is "punch it")
        public const float Models40WeakPulseHz = 6f; // WEAK parts while stunned: the pulse (an open weak point)

        // NIGHT40 round 4 (the lead's lineup40 frames on n40-r4a: a fresh reader named the silhouette sheet 7/7 but paired TANK / CHARGER "both heavy
        // squares" and HUSK / SPITTER "both small capsule-like blobs"): the CHARGER's own body box redrawn low and long with models40 on (EnemyViews.Charger.cs;
        // the view's box was 1.35 x 0.8 h x 1.2, upright), its head plate down at the nose and its back plate (the weak point) on the rump. View only: the
        // sim's hit shape (charger.radius / height) is unchanged.
        public static readonly float[] Models40ChargerBody = { 1.5f, 0.55f, 2.2f }; // m wide, x the height tall, m long (a ram: longer than tall)
        public const float Models40ChargerBodyY = 0.36f; // the body's centre x the height (its belly ~0.18 m off the floor, the back ~1.4 m)
        public static readonly float[] Models40ChargerHead = { 0.42f, 1.25f }; // the head plate: y x the height, z m (at the nose: head down)
        public static readonly float[] Models40ChargerBack = { 0.4f, -1.16f }; // the back plate: y x the height, z m (on the rump, facing back)

        // husk (height 1.8): a hunched yoke over the shoulders (a wider top than the capsule); the tank: a square slab across them (squares for heavies)
        public static readonly float[] Models40Husk =
        {
            0, 0, 0.80f, -0.05f, 1.0f, 0.22f, 0.42f, 10, 0, 0, 0, 1, 0, 0,
            0, 0, 0.82f, -0.08f, 1.45f, 0.5f, 0.75f, 0, 0, 0, 0, 2, 0, 0,
        };

        // swarmer (height 0.75): a spine wedge on the back and a swept tail blade (wedges: fast and dangerous)
        public static readonly float[] Models40Swarmer =
        {
            0, 0, 0.80f, -0.05f, 0.1f, 0.32f, 0.32f, 45, 0, 0, 0, 0, 0, 0,
            0, 0, 0.55f, -0.5f, 0.08f, 0.22f, 0.5f, -25, 0, 0, 0, 0, 0, 0,
        };

        // spitter (height 2; round 4: not a capsule from any side): a big acid dome riding high on its back, over its head (PARRY yellow, full and swelling in
        // the volley's tell: its globs are parryable), and two acid pods on its flanks (a lumpy, wide-at-the-middle outline; the husk's is a plain capsule)
        public static readonly float[] Models40Spitter =
        {
            1, 0, 1.0f, -0.2f, 1.1f, 0.95f, 1.1f, 0, 0, 0, 3, 0, 0, 0.25f,
            1, -0.55f, 0.55f, -0.1f, 0.5f, 0.6f, 0.5f, 0, 0, 0, 0, 0, 0, 0,
            1, 0.55f, 0.55f, -0.1f, 0.5f, 0.6f, 0.5f, 0, 0, 0, 0, 0, 0, 0,
        };

        // charger (height 2.2; round 4: low and long, its body redrawn by the view: Models40ChargerBody): a wide plough wedge at its feet, and two horns
        // forward from its brow, splayed out and up (DANGER red-orange: lit in the tell); its back plate (the view's) stays the weak point on its rump
        public static readonly float[] Models40Charger =
        {
            0, 0, 0.1f, 1.3f, 1.8f, 0.4f, 0.4f, 45, 0, 0, 0, 0, 0, 0,
            0, -0.55f, 0.5f, 1.45f, 0.16f, 0.16f, 1.1f, -25, -35, 0, 2, 0, 0, 0.3f,
            0, 0.55f, 0.5f, 1.45f, 0.16f, 0.16f, 1.1f, -25, 35, 0, 2, 0, 0, 0.3f,
        };

        // warden (height 2.3): square pauldrons, a tall spike up its back (Darktide's back attachment: reads over the shield from the front), a bright chest plate
        public static readonly float[] Models40Warden =
        {
            0, 0, 0.82f, 0, 1.7f, 0.3f, 0.7f, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1.0f, -0.4f, 0.14f, 1.3f, 0.14f, -8, 0, 0, 0, 0, 0, 0,
            0, 0, 0.6f, 0.42f, 0.75f, 0.5f, 0.1f, 0, 0, 0, 1, 0, 0, 0,
        };

        // drone (its root is its centre; y in m): a flat saucer rim (an outline no walker has), a downward emitter spike (PARRY yellow, full in its charge-up)
        public static readonly float[] Models40Drone =
        {
            2, 0, 0, 0, 2.2f, 0.03f, 2.2f, 0, 0, 0, 0, 0, 0, 0,
            0, 0, -0.8f, 0, 0.22f, 0.6f, 0.22f, 0, 45, 0, 3, 0, 0, 0.5f,
        };
    }
}
