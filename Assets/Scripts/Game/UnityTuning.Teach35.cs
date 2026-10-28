// NIGHT35 builder T (design.n35.teach35): Unity-only UI numbers of the teaching text (UiTeach35.cs). The display time and the text are the row's
// (design.n35.teach35.showS / lines); nothing here is a gameplay number.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const double Teach35MaxWaitS = 8.0; // s: the longest a fired teaching line waits for the message slot (behind danger or a boss line) before it is dropped: a line about a room long left teaches nothing (the slot drops a waiting post after 3 s; a teaching line is worth more patience)
    }
}
