// NIGHT50 builder V (G3, design.n49.look49 per level; DECIDE50 D-4): ONE new attempt at the colour script's row for THE ICE LINE and THE SPAN, the
// two levels whose NIGHT49 row failed its meter or its pair. foundry, kiln and spire keep UnityTuning.V49.cs's rows exactly (PostFx reads V49 for
// them), and so do every V49 table this file does not name (fog, stone fog, trilight, rim: the walls keep their hue). The recipe (NIGHT50.md G3):
// a lower saturation lift, darker floors, and a level hue at least Look49AccentMinHueGapDeg (20) from every role token.
// What the NIGHT49 r3 combat frames showed (Logs/feel/n49-r3/v-n47-{iceline,span}/a349/shot-N49-V-*-combat-{off,on}.png; the builder's hand-picked
// rectangles, sRGB luma 0.2126 / 0.7152 / 0.0722, not the reel's masks): iceline ON floor 0.365, right wall 0.114, enemy body 0.160 (OFF 0.272);
// span ON floor 0.379, walls 0.075, enemy body 0.185 (OFF 0.249). The enemies (SKYGRIND/Enemy is UNLIT) sat between dark walls and a bright floor,
// and look49 made them darker (the saturation lift and the contrast push a saturated red's luma down). So this attempt darkens what is LIT (the
// near floor's light is the dusk sun: UnityTuning.Look41.cs round 2b) and lifts less of what is unlit (the enemies, the cyan): the sun x
// V50SunScale on these two levels, a lower saturation lift, a weaker contrast, a higher EV and a weaker grade tint than NIGHT49's row.
// Not measured in a Player by its author (no Player in a worktree): the a349 reel's grayscale audit and meter are the measure.
// THE HUES do not move: iceline 216 (30.4 deg from the usable cyan 185.6, 34.2 from the boundary 250.2) and span 272 (21.8 from the boundary,
// 39.6 from RIVAL 311.6) already sit >= 20 deg from every role token (UnityTuning.Look49.cs; V49Tests and V50Tests pin it). NIGHT49's span 280
// turned the floor pink (n49-r1); under 270.2 it would sit within 20 of the boundary.
// Order: V50Themes names the LightPlan33 theme of each slot; every V50 table has one entry per slot. View only; plain constants (L-035).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The LightPlan33 theme index of each V50 slot: 1 iceline, 3 span (LightPlan33.Themes order: foundry, iceline, kiln, span, spire).
        /// A theme not named here keeps its whole V49 row.</summary>
        public static readonly int[] V50Themes = { 1, 3 };

        /// <summary>The grade's tint (luminance-neutral; V49ShadowTint / V49MidTint shares as V49), made by the builder's scratch script
        /// (colorsys.hsv_to_rgb, rounded): hsv(216, 0.30, 1) for iceline (V49: 0.45, 0x8cbaff), hsv(272, 0.22, 1) for span (V49: 0.30, 0xdbb2ff).
        /// The tint reaches floors and walls alike; weaker, the warm floor takes less of the violet (span's pink) and the blue.</summary>
        public static readonly uint[] V50GradeTintHex = { 0xb2d1ff, 0xe5c7ff };

        /// <summary>ColorAdjustments contrast, URP units: 25 on both (V49: 35 / 35). The enemies' unlit body sits under URP's 0.18 contrast pivot,
        /// so NIGHT49's lift darkened it (iceline enemy body luma 0.272 off -> 0.160 on); 25 keeps part of the wall / floor split.</summary>
        public static readonly double[] V50Contrast = { 25, 25 };
        /// <summary>Post exposure, EV (absolute, as V49ExposureEv): iceline -0.05 (V49 -0.20), span 0 (V49 -0.10). The floor is darkened by the sun
        /// below instead, so the unlit enemies and the usable cyan lose less.</summary>
        public static readonly double[] V50ExposureEv = { -0.05, 0 };
        /// <summary>THE LOWER SATURATION LIFT: URP units added to the config grade's saturation (as V49Saturation): iceline 18 -> 5, span 0 -> -8
        /// (NIGHT49's iceline room frame: 25 % saturated with look49 on, 4 % off). Caveat for the meter: NIGHT49 r3's COMBAT frames were under the
        /// band's floor (saturated 8 % iceline, 5 % span; Logs/player/n49-reel-n49-r3-v-n47-*.log), and the counted pixels there are mostly the
        /// enemies and the cyan, so this lift does not lift that number; it is the first knob to raise if the combat frame stays under 10 %.</summary>
        public static readonly double[] V50Saturation = { 5, -8 };

        /// <summary>DARKER FLOORS: the dusk sun's intensity (RenderSettings.sun, Look.Apply's key light; the light the near floor takes, Look41.cs
        /// round 2b) x this while look49 is applied on the level, put back when it is not (PostFx.Sun50). 0.55: a sun-lit floor at linear x0.55
        /// (x0.61 on iceline with its EV, x0.55 on span), about sRGB luma 0.37 -> 0.28 if the sun is all of its light (an estimate, not measured);
        /// the unlit enemies do not take the sun. The walls facing the sun dim as well; the rim (V49RimHex, unchanged) keeps their hue.</summary>
        public static readonly float[] V50SunScale = { 0.55f, 0.55f };
    }
}
