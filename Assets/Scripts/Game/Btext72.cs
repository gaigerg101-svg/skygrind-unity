// NIGHT72 builder B (slate B3, design.n72.btext72, a VIEW row: no tape bit; gg-proxy #5; GG 2026-10-12 bosses "hard to understand what to do", 2026-10-26 "more text on the
// screen that explains like OK now you have to go up"). THE BOSS TEXT, right: the rules and the counter (pure, no engine call; the HUD's half is UiBtext72.cs).
// What the n71-polish8 boss frames showed (Research/2026-10-27-forge72/gg-proxy.md #5): the wyrm's order names the gauntlet while the THUNDERCLAP is held; a grey STAGGER stands under
// the Crown's bar in idle, tell and windup; the orange beam cuts through a line ("throu|gh"); the bar's order is about 16 px.
// THE ROW ON (read EVERY frame, D1's contract), on top of words71 / voice71 / bossText55 (btext72 on counts as each of the first two on: Words71.OffReason, Voice71.OffReason):
//   1. the meter's name STAGGER is drawn only while the boss is staggered (the bar's OPEN tag): never beside the unfilled meter (LabelWord);
//   2. one order at a time: while the fight line draws a line the bar's own hint is not drawn, whatever it says (HintHidden; order64 hid it only when the two read the same);
//   3. a plate, a dark rounded rectangle behind the fight line and the bar's order, so a beam, a burst or a bright wall never cuts the text; the bar's order is at least
//      UnityTuning.Btext72HintPx at 1080p and in the bone colour (it was grey at 16-18 px).
// THE COUNTER (the slate's proof; L-089: it prints that its input fired): on every HUD frame with a boss bar up, the row on or off: the frames a drawn boss line names a weapon not
// held (Words71's rule), the frames STAGGER is drawn while the boss is not staggered, the frames the fight line and the bar's order are both drawn (would: before the row's own hide),
// the smallest boss text drawn (px at 1080p), the plates drawn. EditMode: Btext72Tests.
using System;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Btext72
    {
        public const string Row = "btext72";

        /// <summary>Is the row on now? Read every frame (D1's same-run toggle flips ViewOn; -skygrindNoBtext72 is Rows72's: it sets the reel override false).</summary>
        public static bool On(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, Row);

        /// <summary>The word under the bar for the unfilled meter: "" with the row on (STAGGER names a state, not a meter), `word` as the game without the row draws it. `meterOn`: the meter is built.</summary>
        public static string LabelWord(bool rowOn, bool meterOn, string word) => meterOn && !rowOn ? word : "";

        /// <summary>THE LINE for a boss order (its raw text, as the game and words71 carry it) with `held` (the Loadout weapon id) in hand, with btext72's rule (UnityTuning.Btext72Lines): at most
        /// Btext72MaxWords words, the verb in capitals, a glyph token. The held line when the weapon the order names is in hand (or it names none), else the line that names the hand to take
        /// (SWAP). Null: the order is not in the table (words71's own variant, or the order as it is, stands). Pure.</summary>
        public static string Order(string raw, string held)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            var table = UnityTuning.Btext72Lines;
            for (int i = 0; i < table.Length; i++)
            {
                if (!string.Equals(table[i].raw, raw, StringComparison.Ordinal)) continue;
                string w = table[i].weapon;
                return w == null || string.Equals(w, held, StringComparison.Ordinal) ? table[i].held : table[i].swap;
            }
            return null;
        }

        /// <summary>The words in a raw line: split on spaces, an {action} token counts as one word (BossRule61.Words' rule; DisplayWords is the as-displayed count). 0 for null or empty.</summary>
        public static int Words(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 0;
            return raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>The words of a table line AS DISPLAYED on the widest device (the t3 verdict: "SWAP to gauntlet, then SWING {fire}" is six tokens but reads seven with the keyboard's "LEFT CLICK"): an
        /// {action} token that UnityTuning.Lesson25Aliases spells in words ({fire} LEFT CLICK, {altFire} RIGHT CLICK) counts those words, any other token (a bound key, a pad glyph) one. 0 for null or empty.</summary>
        public static int DisplayWords(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 0;
            int n = 0;
            foreach (string w in raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string t = w.Trim(',', '.', ':', ';', '!', '?');
                if (t.Length > 2 && t[0] == '{' && t[t.Length - 1] == '}'
                    && UnityTuning.Lesson25Aliases.TryGetValue(t.Substring(1, t.Length - 2), out var alias) && !string.IsNullOrEmpty(alias.keys))
                    n += Words(alias.keys);
                else n++;
            }
            return n;
        }

        /// <summary>Does the line have a verb in capitals: a word of two or more letters, all capitals, that is not a glyph token (an {action} token is lower case in the raw).</summary>
        public static bool HasCapsVerb(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return false;
            foreach (string w in raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string t = w.Trim(',', '.', ':', ';', '!', '?');
                if (t.Length < 2 || t[0] == '{') continue;
                bool caps = true;
                foreach (char ch in t) if (char.IsLetter(ch) && !char.IsUpper(ch)) { caps = false; break; }
                if (caps) return true;
            }
            return false;
        }

        /// <summary>Is the bar's own order hidden this frame: the row on and the fight line draws a line (one order at a time), and the bar has something to say.</summary>
        public static bool HintHidden(bool rowOn, bool lineDrawn, bool hintHasText) => rowOn && lineDrawn && hintHasText;

        /// <summary>The proof counter for one boss (the HUD's frames with its bar up).</summary>
        public sealed class Count
        {
            public string Boss { get; private set; } = "";
            /// <summary>The run's frames (held frames apart), those with the row on; frames a drawn line names a weapon not held (the bar's order, the fight line); frames STAGGER is drawn
            /// while not staggered; frames two orders (the fight line and the bar's order) are drawn at once, and would be without the row's hide; the plates drawn.</summary>
            public int Frames, OnFrames, WrongHand, WrongBar, WrongLine, StaggerLabel, Two, WouldTwo, Plates, Held, LongFrames;
            /// <summary>The smallest boss text drawn (px at 1080p; 0: none drawn yet).</summary>
            public float MinPx;
            /// <summary>The distinct boss lines drawn over Btext72MaxWords words (at most MaxLongNotes), each with its word count: what the rule has not reached.</summary>
            public readonly System.Collections.Generic.List<string> LongNotes = new System.Collections.Generic.List<string>();
            public const int MaxLongNotes = 5;
            public bool Dirty;

            public void Reset(string boss)
            {
                Boss = boss ?? "";
                Frames = OnFrames = WrongHand = WrongBar = WrongLine = StaggerLabel = Two = WouldTwo = Plates = Held = LongFrames = 0;
                MinPx = 0f;
                LongNotes.Clear();
                Dirty = false;
            }

            /// <summary>A drawn boss line over the word limit: remembered once (its text and its words).</summary>
            public void NoteLong(string text, int words)
            {
                if (LongNotes.Count >= MaxLongNotes || string.IsNullOrEmpty(text)) return;
                string n = "\"" + text + "\" (" + words + " words)";
                if (!LongNotes.Contains(n)) LongNotes.Add(n);
            }

            /// <summary>One HUD frame with the bar up. `held`: a same-run toggle's held frame (counted apart). `longLine`: a drawn boss line is over Btext72MaxWords words.</summary>
            public void Frame(bool held, bool rowOn, bool wrongBar, bool wrongLine, bool staggerLabel, bool two, bool wouldTwo, int plates, float minPx, bool longLine = false)
            {
                Dirty = true;
                if (held) { Held++; return; }
                Frames++;
                if (longLine) LongFrames++;
                if (rowOn) OnFrames++;
                if (wrongBar) WrongBar++;
                if (wrongLine) WrongLine++;
                if (wrongBar || wrongLine) WrongHand++;
                if (staggerLabel) StaggerLabel++;
                if (two) Two++;
                if (wouldTwo) WouldTwo++;
                Plates += plates;
                if (minPx > 0f && (MinPx <= 0f || minPx < MinPx)) MinPx = minPx;
            }

            /// <summary>The totals line.</summary>
            public string Line(string why, bool rowOnNow, double t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                return "[btext72] " + (Boss.Length == 0 ? "?" : Boss) + " | boss lines naming a weapon not held " + WrongHand.ToString(ci) + " frames (the bar's order " + WrongBar.ToString(ci) + ", the fight line "
                    + WrongLine.ToString(ci) + ") | STAGGER drawn while not staggered " + StaggerLabel.ToString(ci) + " frames | two orders at once " + Two.ToString(ci) + " frames (would be "
                    + WouldTwo.ToString(ci) + " without the row's hide) | boss lines over " + UnityTuning.Btext72MaxWords.ToString(ci) + " words " + LongFrames.ToString(ci) + " frames" + (LongNotes.Count == 0 ? "" : " (" + string.Join("; ", LongNotes) + ")")
                    + " | smallest boss text " + MinPx.ToString("0.#", ci) + " px | plates drawn " + Plates.ToString(ci)
                    + " | held " + Held.ToString(ci) + " frames | row btext72 on " + OnFrames.ToString(ci) + " of " + Frames.ToString(ci) + " frames" + (rowOnNow ? "" : " (off now)")
                    + " | " + why + ", t " + t.ToString("0.00", ci) + " s (FeelClock)";
            }
        }
    }
}
