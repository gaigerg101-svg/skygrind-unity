// NIGHT63 builder G (leftover 2, the pinned hint for text61's two-run pair; PinHint63.cs, UiPinHint63.cs): Unity-only reel values (CLAUDE.md
// rule 5), one comment each. Reel only: nothing here acts without -skygrindPinHint63 and -skygrindReelOnly on the command line.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The line -skygrindPinHint63 pins when it is given no text: the boost chain's second step as the config writes it
        /// (design.n43.kiln43.chain "look"), which brief55's teachMap words "Midair, aim down, then {fire}." (NIGHT62's s1on Player log shows
        /// it on n57-kiln as "Midair, aim down, then LEFT CLICK."): the instruction that stood under THE CLINKER BRIDGES' title on NIGHT60's
        /// red frame (n60-b8 b8on kb2c entry, Research/2026-10-22-wave61/G.md).</summary>
        public const string PinHint63Default = "STEP 1 · IN THE AIR, LOOK STRAIGHT DOWN";

        /// <summary>HUD frames with no lesson line after each room entry: the card closes and the slot empties, as at a room change (vis44
        /// drops the last room's lesson at roomEnter), then the pinned line comes again: a new room's lesson, at the same moment in both runs.</summary>
        public const int PinHint63GapFrames = 1;
    }
}
