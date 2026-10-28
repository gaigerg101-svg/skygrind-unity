// NIGHT24 audit (builder B, rule 5): Unity-only numbers that were typed in their files, moved here with their reasons. Visual / tooling only: the sim never reads them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The BIG PICK screen ignores a confirm for this long after it opens (s): the offer opens mid-break, so the jump's A press that
        /// is still down must not take a card (UiBigPick24).</summary>
        public const float BigPick24AcceptDelay = 0.4f;

        /// <summary>The hitch tour's frame recorder (Hitch24; the lead's tooling for "laggy switching"): the ring size (frames), the median window,
        /// a hitch is a frame over Factor x the median and over Floor s; Over33 / Over167 are the 30 fps and 60 fps frame budgets (s).</summary>
        public const int Hitch24Cap = 8192, Hitch24MedianWindow = 60;
        public const float Hitch24Floor = 0.008f, Hitch24Factor = 3f, Hitch24Over33 = 0.033f, Hitch24Over167 = 0.0167f;
    }
}
