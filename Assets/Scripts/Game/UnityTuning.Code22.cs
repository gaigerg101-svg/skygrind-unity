// NIGHT22 U3-36.8 CHALLENGE CODE (builder W2): Unity-only UI numbers for the TITLE code field and RESULTS' code line (Code22Field.cs).
// UNCOMPILED when written (the builder has no Editor). The code's own numbers (alphabet, length, checksum) are design.n21.challenge.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float Code22ResultsY = -470f; // px from the screen's middle (1080p reference): the small CODE line under RESULTS' column; the lead tunes it on the Player
        public const double Code22FirstRepeat = 0.35; // s: a held direction on the code sheet steps once, then waits this long (a pad's d-pad is a button, not a ramp)
        public const double Code22RepeatTime = 0.12; // s: then steps this often while held (a 12-slot code with up to 32 characters each stays fillable in a minute)
    }
}
