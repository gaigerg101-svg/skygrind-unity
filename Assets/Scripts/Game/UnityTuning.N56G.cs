// NIGHT56 builder G: Unity-only numbers of G's reels (FeelReel.N56.Play.cs, a256). Probe layout only: no gameplay number.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the a256 reel (G1 rank56's Player proof) ---
        /// <summary>a256: husks (1 hp, held) on the arc, its radius (m: under heal36's 4 m, so every kill is a close kill that heals) and the
        /// angle between two husks (deg: 7 x 18 deg = a 108 deg arc, every husk inside a 1080p frame's width when looked at in turn).</summary>
        public const int Rank56ReelHusks = 7;
        public const double Rank56ReelRadius = 3.0, Rank56ReelStepDeg = 18;
        /// <summary>a256: the health held at most this (of 100) before each kill, so heal36's heal (10 HP x the rank's multiplier, up to 20) is never
        /// capped by the max.</summary>
        public const double Rank56ReelHp = 60;

        // --- brief55's top-left column under the room counter (UiBrief55.cs Brief55LeftTop; P1-teachText) ---
        /// <summary>The room counter's plate height at rest (px at the 1920 x 1080 reference): the column clears this from the plate's top, so a card
        /// laid while the counter's open animation still draws it short (4.2 px at its first frame) is not covered when it grows. n56-r1's verbs37
        /// telemetry (kiln, spire): the plate at rest 83.6 px (Plate 52; its shadow 57.4 + 83.6 from the top); 88 covers it and the shadow's 5 px
        /// offset.</summary>
        public const float Brief55CounterRestPx = 88f;
    }
}
