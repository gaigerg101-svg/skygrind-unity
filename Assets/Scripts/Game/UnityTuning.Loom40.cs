// NIGHT40 builder B4 (NIGHT40.md B4 THE LOOM): Unity-only numbers for loom40's look (CLAUDE.md rule 5: each with a comment). The rules are the
// sim's (design.n40.loom40); these only draw them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float Loom40NoRideBlinkHz = 4f; // the canyon's next no-ride walls blink 4 times a second through the 1 s warning (design.n40.loom40.noRideWarnS): four clear on/off beats before they hold (the walls are dull slate, so the blink is a pattern, not a bright flash)
        // Loom37View.N40.cs: the poses (offsets and tilts on the body only; the legs stay the sim's, so each stays under ~1.5 m)
        public const float Loom40RearM = 1.2f; // m the body draws back from you through a wind-up (the anticipation frames: "even a frame of anticipation" helps, research/boss-web.md 1)
        public const float Loom40RearUpM = 0.8f; // m it rises through the wind-up (reads at 20-30 m: ~2 deg of screen)
        public const float Loom40RearDeg = 12f; // deg it tips back through the wind-up
        public const float Loom40LungeM = 1.5f; // m it lunges at you on the strike (the release of the wind-up)
        public const float Loom40LungeDeg = 14f; // deg it pitches forward on the strike
        public const float Loom40SagM = 1.0f; // m it sags in RECOVER and in the CYAN window (the punish beat: it hangs low and open)
        public const float Loom40WindowRollDeg = 18f; // deg it rolls in the CYAN window (the spinneret turned to you)
        public const float Loom40StaggerS = 0.6f; // s a tear's shudder lasts (a stagger you can see)
        public const float Loom40ShudderM = 0.45f; // m the shudder's amplitude at its start (fades to 0)
        public const float Loom40GlideLeanDeg = 10f; // deg it leans into its glide at the glide's top speed
        public const float Loom40TellR = 1.6f; // m the tell glow's radius at the end of a wind-up (about the spinneret's size: a part, not the whole body)
        public const float Loom40PullR = 0.9f; // m each lit pull-away anchor's cue ball (the anchors' own rings are ~0.6 m)
        public const float Loom40PracticeR = 1.0f; // m o2's practice knee (THE LOOM's knees are kneeRadius 1.3 x Loom37KneeScale 0.85 = 1.1 m)
        public const float Loom40PracticePulse = 0.08f; // its breath (loom38's knee breath amplitude)
    }
}
