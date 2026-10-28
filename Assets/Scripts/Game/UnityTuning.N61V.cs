// NIGHT61 builder V (slate S1, the exit frame: Game/ExitFrame61.N61V.cs, view row design.n61.exitFrame61): Unity-only numbers (render), each with
// its reason. No gameplay number here. lamp60's numbers (UnityTuning.N58V.cs, UnityTuning.N60V8.cs) are untouched.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---------------- the frame at room A's face of the passage ----------------

        /// <summary>exit frame: the posts' and the lintel's width (m), from the opening's edge outward. It fills the band a157's clear shot counts as
        /// the door (FeelReel.L60ScreenRect grows the opening by Exit58FrameOut + Exit58FrameW = 0.37 m), so the whole frame is inside the door's box;
        /// at 1080p and camera.fovDeg 74 it is ~10 px at 25 m (0.035 m a px), lamp60's 0.22 m bar ~6 px.</summary>
        public const double ExitFrame61BarW = 0.37;

        /// <summary>exit frame: the bars' depth along the passage (m): a slab standing in front of room A's face, not a block reaching into the room.</summary>
        public const double ExitFrame61BarD = 0.12;

        /// <summary>exit frame: the gap (m) between room A's face and the frame's back: in front of the wall, never in it (Exit60FaceGap's reason:
        /// n57-kiln's doors are 4 m deep, a frame at the rect's centre sat inside the walls).</summary>
        public const double ExitFrame61FaceGap = 0.02;

        /// <summary>exit frame: HDR multiplier on colors.usable for the frame's and the gate's bars (additive): over the bloom threshold, so they glow
        /// and read at 25 m (Doors38Glow is 2.4, Exit58Glow 3.2).</summary>
        public const float ExitFrame61Glow = 4f;

        // ---------------- the passage light and its gate (the fixture it hangs from) ----------------

        /// <summary>exit frame: the gate's bar width (m): two posts and a lintel just inside the passage's far mouth, the passage light's fixture
        /// (PRINCIPLES 7: lights hang from real fixtures). Seen from room A it frames the passage's end inside the opening.</summary>
        public const double ExitFrame61GateW = 0.25;

        /// <summary>exit frame: the gate's inset (m) from the passage's walls, its ceiling and its far face: never coplanar with them (no z-fight).</summary>
        public const double ExitFrame61GateInset = 0.01;

        /// <summary>exit frame: the passage light hangs this far (m) on room A's side of the gate, so the gate's posts do not hide it from room A.</summary>
        public const double ExitFrame61LightBack = 0.3;

        /// <summary>exit frame: and this far (m) under the gate's lintel, so the lintel and the passage's ceiling round it take the most light.</summary>
        public const double ExitFrame61LightDrop = 0.35;

        /// <summary>exit frame: the passage light's intensity (URP units). The stone is dark: Doors38View's doorway light (intensity 3, the passage's
        /// middle) left the opening dark on n60-b8's kb3 clear frames, so the passage's walls, floor and ceiling need several times that to take
        /// the cyan. A steady light: one rise with the frame, never a flash.</summary>
        public const float ExitFrame61LightIntensity = 12f;

        /// <summary>exit frame: the passage light's range (m): it reaches the passage and room B's floor beyond the mouth, never room A's floor
        /// outside a157's door box (0.37 m in front of the face is ~5.5 m from the light on a 4 m deep, 4.5 m high door): the GUARD (the room's
        /// p95 luma outside the door unchanged). URP point lights here cast no shadow, so the range is the only bound.</summary>
        public const float ExitFrame61LightRange = 4.8f;

        // ---------------- the floor line ----------------

        /// <summary>exit frame: the floor line's width (m): a stripe a player reads from across the room, narrower than a 1 m floor tile.</summary>
        public const double ExitFrame61LineW = 0.2;

        /// <summary>exit frame: one dash's length (m) along the line: dashes read as a path, not a crack in the floor.</summary>
        public const double ExitFrame61LineDashM = 0.8;

        /// <summary>exit frame: the gap (m) between two dashes.</summary>
        public const double ExitFrame61LineGapM = 0.4;

        /// <summary>exit frame: the dashes' lift over the floor (m): no z-fight with the floor's top at 25 m.</summary>
        public const double ExitFrame61LineLift = 0.03;

        /// <summary>exit frame: the dashes' thickness (m): a flat stripe.</summary>
        public const double ExitFrame61LineThick = 0.02;

        /// <summary>exit frame: the most the line runs back into room A from the door's face (m): half a 20 m room.</summary>
        public const double ExitFrame61LineMaxM = 10;

        /// <summary>exit frame: the line stops this far (m) short of the first thing on its strip (a low solid, a ramp, a floor at another height,
        /// lava): never into a step or across the lava.</summary>
        public const double ExitFrame61LineClearM = 0.75;

        /// <summary>exit frame: the half width (m) of the strip searched for that first thing: the line's half width and a margin.</summary>
        public const double ExitFrame61LineHalfM = 0.5;

        /// <summary>exit frame: a solid whose bottom is higher than this over the floor (m) is overhead (a ledge you walk under): the line runs on.</summary>
        public const double ExitFrame61LineHeadM = 2.0;

        /// <summary>exit frame: a solid whose top is within this of the floor's height (m) is the floor itself.</summary>
        public const double ExitFrame61LineStepM = 0.05;

        /// <summary>exit frame: HDR multiplier on colors.usable for the dashes at the door (dimmer than the frame: the door reads first).</summary>
        public const float ExitFrame61LineGlow = 2f;

        /// <summary>exit frame: the dashes' share of that glow at the line's room end: it brightens toward the door (the direction without motion).</summary>
        public const float ExitFrame61LineDim = 0.3f;

        // ---------------- the rise ----------------

        /// <summary>exit frame: seconds of the one rise from dark to lit when the door opens (no flash: CLAUDE.md rule 8); at or under Exit58FadeS,
        /// so a157's clear shot (it settles Exit58FadeS + Lamp60SettleS after the clear) frames it at full.</summary>
        public const float ExitFrame61RiseS = 0.35f;
    }
}
