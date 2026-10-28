// NIGHT63 builder G (NIGHT63.md leftover 2: text61, NIGHT62 P3, made judgeable on a two-run pair with the hint pinned). NIGHT62's P1 pair (two
// a157 runs of one build) differed in the hint line under DOOR OPEN (Logs/n62/spot/P1.txt), and P3's pair could not show text61 at all: two runs
// of one build drift (NIGHT62's s1on / s1spot telemetry: 0 frames to the kb2 clear, 1 from it, 9 by the kb3 entry), so the sim's lesson rotation
// (the boost chain's timed steps) put a different line on screen at the same shot. -skygrindPinHint63 [text] (REEL ONLY: it acts only with
// -skygrindReelOnly on the command line; never in normal play) replaces, for the whole reel, the lesson and teach lines the HUD's teaching slot
// is fed (UiHud33.Slot33's `lesson` / `teach`: the centre card's input) with the pinned line, armed at each room's entry after
// UnityTuning.PinHint63GapFrames empty frames (a new room's lesson, as the game shows one), so what the card is asked to show depends on the
// room entries alone; text61's gate (or its absence) then decides what is drawn. View only: the sim's lessons still run, untouched (the
// fingerprints do not see the HUD). This file holds the pure switch and feed (EditMode: N63PinHintTests); UiPinHint63.cs wires them.
using System;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static class PinHint63
    {
        public const string Switch = "-skygrindPinHint63";
        public const string ReelSwitch = "-skygrindReelOnly";

        /// <summary>The pinned lines (as the config writes a lesson: {action} tokens), or null: no switch, or not a reel. The text is the argument
        /// after the switch, '|' separating lines (one per room, in turn); none (the last argument, or the next is another switch): the default.</summary>
        public static string[] Parse(string[] args)
        {
            if (args == null) return null;
            int i = Array.IndexOf(args, Switch);
            if (i < 0 || Array.IndexOf(args, ReelSwitch) < 0) return null;
            var lines = new List<string>();
            if (i + 1 < args.Length && args[i + 1] != null && !args[i + 1].StartsWith("-", StringComparison.Ordinal))
                foreach (string part in args[i + 1].Split('|'))
                {
                    string t = part.Trim();
                    if (t.Length > 0) lines.Add(t);
                }
            if (lines.Count == 0) lines.Add(UnityTuning.PinHint63Default);
            return lines.ToArray();
        }
    }

    /// <summary>The pinned feed: one per Ui. Nothing before the first room entry; at each entry PinHint63GapFrames frames with nothing, then the
    /// room's line (lines in turn, one per room) every frame. Pure.</summary>
    public sealed class PinHint63Feed
    {
        readonly string[] lines;
        int gap;
        /// <summary>Rooms entered, frames fed a line, frames left empty after an entry.</summary>
        public int Rooms, Fed, Gaps;

        public PinHint63Feed(string[] lines) { this.lines = lines != null && lines.Length > 0 ? lines : new[] { UnityTuning.PinHint63Default }; }

        /// <summary>The line the current room is fed (null before the first room).</summary>
        public string Current => Rooms > 0 ? lines[(Rooms - 1) % lines.Length] : null;

        public void RoomEnter()
        {
            Rooms++;
            gap = Math.Max(1, UnityTuning.PinHint63GapFrames);
        }

        /// <summary>This HUD frame's line: null before the first room and in the gap after each entry, else the room's line.</summary>
        public string Line()
        {
            if (Rooms == 0) return null;
            if (gap > 0) { gap--; Gaps++; return null; }
            Fed++;
            return Current;
        }
    }
}
