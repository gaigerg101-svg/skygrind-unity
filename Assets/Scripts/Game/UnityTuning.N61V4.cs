// NIGHT61 builder V (slate S4, lights hang from real fixtures: Game/Fixture61.N61V4.cs, view row design.n61.fixture61): Unity-only numbers (render
// geometry and the meter's threshold), each with its reason. No gameplay number here. Unity-free plain numbers (Fixture61Math runs in a harness too).
// Every mount stays within Rods40WallClear (0.3 m) under its ceiling: rods40's play volume (GG: "white rods that you clip through") never meets one.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>fixture61: the housing's rim (m) outside the ceiling panel's own frame (Look33FrameWidth 0.08): a dark border of 0.30 m in all,
        /// ~9 px at 25 m at 1080p (camera.fovDeg 74: 0.035 m a px); the frame alone is ~2 px there (the N55 critic: "a dark frame >= 4 px").
        /// NIGHT72 L6: 0.22 (NIGHT61: 0.16, 0.24 m in all, ~7 px): NIGHT62's lead looked at the pair and saw "a small change against a dark ceiling at 1080p"
        /// (Logs/n62/slate.json P4); the housing is a third wider so the pair reads at the size the lamps are seen.</summary>
        public const double Fixture61RimW = 0.22;

        /// <summary>fixture61: the housing's walls hang this far (m) under the ceiling, deeper than the frame's 0.07 m lip, so the lens reads recessed in
        /// a fitting; at or under Rods40WallClear 0.3 (flush under a ceiling: never in the play volume). NIGHT72 L6: 0.26 (NIGHT61: 0.22).</summary>
        public const double Fixture61Drop = 0.26;

        /// <summary>fixture61: a hanger's width (m): the rail from the housing to the beam (lights35's joist) or the wall, ~5 px at 15 m; NIGHT72 L6: 0.16
        /// (NIGHT61: 0.12), ~7 px at 15 m.</summary>
        public const double Fixture61HangerW = 0.16;

        /// <summary>fixture61: a hanger's height (m); its band is HangerTop .. HangerTop + this under the ceiling (0.10-0.22 m: inside Rods40WallClear 0.3).
        /// NIGHT72 L6: 0.12 (NIGHT61: 0.10).</summary>
        public const double Fixture61HangerH = 0.12;

        /// <summary>fixture61: a hanger's top this far (m) under the ceiling: its band (0.10-0.20 m under) meets a joist's side (0.45 m deep) and stays
        /// within Rods40WallClear.</summary>
        public const double Fixture61HangerTop = 0.10;

        /// <summary>fixture61: a joist farther than this (m) from the housing does not hold it (lights35 puts the key panel in a 4 m bay: the two nearest
        /// joists are ~1.2 m from its housing).</summary>
        public const double Fixture61BeamMaxM = 4.0;

        /// <summary>fixture61: with no joist on either side, a hanger runs to the nearest wall face at most this far (m) (a short room: under 6 m long,
        /// lights35 places no joist).</summary>
        public const double Fixture61WallMaxM = 6.0;

        /// <summary>fixture61: the mounts' colour, colors.pillarSide x this, on the stone material: lighter than the frames and joists (Look33FrameShade
        /// 0.55), so the metal reads against the dark ceiling round the panel (its key light is a spot down: the ceiling stays dark).</summary>
        public const float Fixture61Shade = 0.8f;

        /// <summary>fixture61's meter: a ceiling light reads as mounted when its dark border is at least this wide (m), ~4 px at 25 m at 1080p, and a
        /// hanger holds it to a beam or a wall.</summary>
        public const double Fixture61MinRimM = 0.14;
    }
}
