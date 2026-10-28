// NIGHT20 U3-34.4 beyond (builder P; NIGHT20.md: "the first-pad hint card (L3 sprint, R3 melee, RB scroll, LT zoom)"): the
// four verbs a pad player can't guess from the web's keys, shown once per profile the first time a pad plays a run with the
// v3 layout live (design.pad.n20): each line is its live button's glyph (PromptText: the drawn sprite, remaps followed;
// the words without the sprites) and a word (UnityTuning.PadHintRows). Ui's onboarding draws the card for
// UnityTuning.PadHintSeconds in the top-left column. The rule and the lines only (built when the card shows: strings then).
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class PadHintCard
    {
        /// <summary>The profile flag: the card was shown.</summary>
        public const string Flag = "padHint20";

        /// <summary>The card's lines: each row's live glyph and its word ("[L3] SPRINT").</summary>
        public static string[] Lines()
        {
            var rows = UnityTuning.PadHintRows;
            var lines = new string[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                string glyph = PromptText.For(rows[i].action, false, rows[i].action.ToUpperInvariant());
                lines[i] = string.IsNullOrEmpty(glyph) ? rows[i].word : glyph + " " + rows[i].word;
            }
            return lines;
        }

        /// <summary>Show it now: the pad in hand, the v3 layout live, in a run, not the attract or a bot, never shown before.</summary>
        public static bool Due(Profile p, bool padLast, bool v3, bool playing, bool botOrAttract) =>
            padLast && v3 && playing && !botOrAttract && p != null && !p.Flag(Flag);

        /// <summary>Mark it shown (true when newly marked: the caller saves the profile).</summary>
        public static bool Seen(Profile p) => p != null && p.SetFlag(Flag);
    }
}
