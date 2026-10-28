// NIGHT63 builder V (slate B1; view row design.n63.bossMotion63): Unity-only numbers (CLAUDE.md rule 5: the boss63 reel's timing and how the
// view reads the wyrm's strike, one comment each). The sim never reads them (golden vectors, replays and fingerprints untouched). The motion
// layer's own numbers (springs, limits, flinch, slump, breathing) are gameplay-facing view numbers and live in design.n63.bossMotion63.numbers.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- BossAttackClock63 (BossAttackClock63.cs) ---

        /// <summary>s the clock reads THE KILN WYRM's attack as a Strike after its tell's hit step (the wyrm's log has a tell's step and seconds,
        /// not an active state: its lunge, dive and spit resolve in one step). A view reading only: no sim number moves.</summary>
        public const double Boss63WyrmStrikeS = 0.25;

        /// <summary>s the wyrm's strike stays the clock's read when another tell is already pending (two reel frames at 60 fps, so the boss63 reel
        /// still reads the hit); then the pending tell's wind-up shows (NIGHT63 fix round: n63-b2's spit posed 192 ms before its hit).</summary>
        public const double Boss63WyrmStrikeOverS = 0.034;

        // --- the boss63 reel (FeelReel.N63V.cs) ---

        /// <summary>s the reel holds the fight after the boss's rise (the slate's B1: "20-30 s after the rise"), on the reel's fixed clock.</summary>
        public const double Boss63HoldS = 25.0;

        /// <summary>s after the rise the reel forces the boss's stagger when none came by itself (the reel's guns rarely land a boss's "right move"):
        /// the stagger slump's SPOT frame on every boss. The line says FORCED.</summary>
        public const double Boss63ForceStaggerAtS = 14.0;

        /// <summary>s into a stagger the reel frames the slump (B63-&lt;boss&gt;-stagger): the slump spring has settled most of the way by then.</summary>
        public const double Boss63StaggerShotS = 0.6;

        /// <summary>Frames after a damage event's frame the reel frames the flinch (B63-&lt;boss&gt;-flinch): the squash peaks about 40 ms after the hit
        /// (2.4 frames at 60 fps).</summary>
        public const int Boss63FlinchShotFrames = 2;

        /// <summary>s after the rise before a damage event may be the flinch frame (the reveal and the name card are over).</summary>
        public const double Boss63FlinchAfterS = 3.0;

        /// <summary>s after the rise the reel frames the boss at rest (B63-&lt;boss&gt;-idle): the breathing's frame.</summary>
        public const double Boss63IdleShotS = 2.5;

        /// <summary>Reel frames between trigger taps while it fires at the weak point (a held trigger would build the RT charge instead).</summary>
        public const int Boss63TapEvery = 2;

        /// <summary>m added to a near63 placement so the husk's body surface (not its feet) sits the named distance from the eye: the husk's body
        /// radius as drawn (about 0.4 m; the reel prints the measured gap from the eye to the capsule's bounds beside it).</summary>
        public const double Near63HuskRadiusM = 0.4;

        /// <summary>The share of an attack's peak wind-up pose (BossMotion63Probe.Windup) at which the reel calls the pose visible: its "pose" ms.</summary>
        public const double Boss63PoseShare = 0.25;
    }
}
