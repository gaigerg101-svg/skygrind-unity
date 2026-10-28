// NIGHT63 builder G (text63: text61's critic points, DECIDE63 D-7; Text63.cs, UiText61.cs): Unity-only UI timing (CLAUDE.md rule 5), one comment.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>s a new instruction waits at most for the room's title to leave the message slot with text63 on (text61: Text61WaitS, 3 s).
        /// b3's verbs37 runs showed 4 lines after text61's 3 s with the title still up ("LATE": iceline, kiln, span, spire): the titles there stand
        /// longer than 3 s, so the line waits up to 10 s for the title to clear; past it the line shows anyway (logged LATE) so a lesson is never
        /// lost.</summary>
        public const double Text63WaitS = 10.0;

        /// <summary>s a line released from text63's queue after the slot moved on (it waited for the title while the run went on) stands at least,
        /// before the next queued line takes the card (NIGHT63 G T63b: the waiting line is never dropped, Logs/n63/verdict-T63.md). Under brief55's
        /// teachMaxS (5.5 s), which still closes any line that stands longer; long enough to read a 5-8 word line.</summary>
        public const double Text63HoldS = 3.0;
    }
}
