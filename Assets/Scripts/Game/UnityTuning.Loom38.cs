// NIGHT38 builder O2 (loom38, NIGHT38.md L4: "one polish pass ... written as config-driven numbers" until the lead's n37-r7 frames come; GG 2026-10-13:
// THE LOOM "needs to [polish] a little more I like it", and of every boss "it's just hard to really know what's happening"). Visual only (Loom37View):
// the sim never reads these. Every cue answers "what do I do now": the knees to nail breathe, each stuck nail is a warm pip circling its knee
// (so the count to a tear reads: three pips = pull away), a knee ready to tear flashes white, and the body carries two red-orange slits (an enemy at
// dusk reads red-orange: CLAUDE.md rule 8) so it reads against the dark sky. Off with -skygrindOff38 (N38Blocks.ForceOff): NIGHT37's look.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>A live knee (the pair to nail) breathes: its drawn scale x (1 + amp x sin), at this rate (Hz). Ours: slow enough to read, a 18 % swell.</summary>
        public const float Loom38KneeBreathAmp = 0.18f, Loom38KneeBreathHz = 1.6f;
        /// <summary>A stuck nail's pip: its drawn radius (m), its orbit (x the knee's hit radius), its turn rate (turns a second), its colour gain
        /// (the nail colour, Loom37NailHex). Ours: 0.22 m reads at the canyon's 20-30 m; the orbit clears the knee sphere.</summary>
        public const float Loom38PipR = 0.22f, Loom38PipOrbit = 1.35f, Loom38PipTurns = 0.5f, Loom38PipGain = 2.6f;
        /// <summary>A knee with design.n37.loom37.nailsToTear nails in it (ready: pull away now) flashes this colour at this rate (Hz) and gain.</summary>
        public const uint Loom38ReadyHex = 0xf4f0ff;
        public const float Loom38ReadyHz = 7f, Loom38ReadyGain = 3.4f;
        /// <summary>The body's two red-orange slits (enemy colour, Loom37DangerHex): width x height (m) on each long side, and their gain.</summary>
        public const float Loom38SlitW = 4.2f, Loom38SlitH = 0.35f, Loom38SlitGain = 1.6f;
    }
}
