// NIGHT71 builder B (slate B2 "the orders name the hand you hold", design.n71.words71, a VIEW row: no tape bit). T-155 (gg-proxy rc2,
// Logs/n64/gg-proxy.md:157-158: "One LEFT CLICK swing drops a tank" while the KESTREL is held, vi/f0400; "PUNCH THE HEAD TO STAGGER IT" while
// the THUNDERCLAP is held, bk/wyrm-windup); DECIDE63 D-4 (the wyrm's rule line names the gauntlet while the reel holds the THUNDERCLAP).
// THE RULE (with the row on): a line NAMES A WEAPON when it holds one of that weapon's phrases (design.n71.words71.numbers.names, case
// ignored; plain words that survive the {action} resolve: "GAUNTLET", "SWING", "SLUG" ...). A line names a weapon NOT HELD when it names a
// weapon, does not name the one in hand (Loadout.Weapon) and does not tell the player to change hands (a swapWords word: "SWAP"). Such a line:
//   - a boss order (the boss bar's hint and its fight-line copy; bossRule61's rule line at the rise): is shown as its variant
//     (numbers.lines: the raw order -> a line that names the hand to take or holds for any hand), in place (the queue, the holds and the fades
//     untouched); an order with no variant waits (not drawn) until its weapon is in hand;
//   - a lesson or a teaching line (the teaching slot: text36 / teach43's card, the lesson channel): waits until its weapon is in hand.
// Why not "{punch}" on the wyrm (the slate's wording): the fist punch staggers THE KILN WYRM only with design.n48.wyrmTell48 on (SurvivalSim.N48.Brawl.cs:
// 124-146, Tell48OnPunch's t48On), and that row is OFF in the repo; with it off only a gauntlet hit (HitEvent weapon "gauntlet") staggers it
// (SurvivalSim.Boss30.cs:282-310), so a {punch} line would tell the player a press that does nothing. The variants name the gauntlet as the hand to
// take instead (Research/2026-10-26-wave71/B/words71/card.md).
// THE SWITCH is read every frame (NightRows.ViewOn: D1's same-run toggle flips it; the row is in SameRun63Math.FrameRows). Off (the row off,
// -skygrindOff71 or older, -skygrindNoWords71): every line as the game without the row draws it.
// THE COUNTER (the slate's proof; L-089: it prints that its input fired): on every HUD frame, the row on OR off, the lines DRAWN that name a weapon
// not held (the bar's hint, the fight line, the teach card, the lesson widget), and the lines the game without the row would draw that do (the
// red, in the same run); the held weapon each time; the same-run toggle's held frames apart. Hooks: UiWords71.cs. EditMode: N71WordsTests.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Words71
    {
        public const string Row = "words71";
        public const string NoSwitch = "-skygrindNoWords71";
        public const int Night = 71;
        public const string Root = "design.n71.words71.numbers.";
        const string RowOff = "design.n71." + Row + " off";

        static string[] args;

        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>Why the row is off for this config and command line (null: on): the row (NightRows.ViewOn, the reel's live override first),
        /// then the off switches (Order64.OffReason's order).</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row) && !NightRows.ViewOn(cfg, Btext72.Row)) return RowOff; // NIGHT72 B btext72 builds on words71: its row on counts as this one on (-skygrindNoWords71 still turns this one off)
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff71 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            return null;
        }

        /// <summary>Is the row on now? Read every frame: never cache it (D1's same-run toggle flips ViewOn).</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>The row's numbers (design.n71.words71.numbers), read once per config.</summary>
        public sealed class Table
        {
            /// <summary>Weapon ids (Loadout ids: gauntlet, shotgun, revolver ...) and, per weapon, the phrases that name it.</summary>
            public readonly string[] Weapons;
            public readonly string[][] Phrases;
            /// <summary>Words that tell the player to change hands (a line holding one names the hand to take: never counted).</summary>
            public readonly string[] SwapWords;
            /// <summary>A boss order's raw text -> the line shown instead while it names a weapon not held.</summary>
            public readonly Dictionary<string, string> Lines;

            public Table(string[] weapons, string[][] phrases, string[] swapWords, Dictionary<string, string> lines)
            {
                Weapons = weapons ?? new string[0];
                Phrases = phrases ?? new string[0][];
                SwapWords = swapWords ?? new string[0];
                Lines = lines ?? new Dictionary<string, string>(StringComparer.Ordinal);
            }

            /// <summary>An empty table (no numbers block: nothing names a weapon).</summary>
            public static readonly Table Empty = new Table(null, null, null, null);
        }

        static GameConfig tableFor;
        static Table table = Table.Empty;

        /// <summary>The table for `cfg` (read once per config instance; Empty when the block is missing or malformed).</summary>
        public static Table Of(GameConfig cfg)
        {
            if (cfg == null) return Table.Empty;
            if (ReferenceEquals(cfg, tableFor)) return table;
            tableFor = cfg;
            table = Read(cfg);
            return table;
        }

        static Table Read(GameConfig cfg)
        {
            try
            {
                if (!cfg.Has(Root + "names")) return Table.Empty;
                var weapons = new List<string>();
                var phrases = new List<string[]>();
                if (cfg.Get(Root + "names") is Dictionary<string, object> names)
                    foreach (KeyValuePair<string, object> kv in names)
                    {
                        if (kv.Key.EndsWith("_why", StringComparison.Ordinal) || !(kv.Value is List<object> list)) continue;
                        var ps = new List<string>();
                        foreach (object o in list) if (o is string s && s.Length > 0) ps.Add(s);
                        weapons.Add(kv.Key);
                        phrases.Add(ps.ToArray());
                    }
                var swap = new List<string>();
                if (cfg.Has(Root + "swapWords") && cfg.Get(Root + "swapWords") is List<object> sw)
                    foreach (object o in sw) if (o is string s && s.Length > 0) swap.Add(s);
                var lines = new Dictionary<string, string>(StringComparer.Ordinal);
                if (cfg.Has(Root + "lines") && cfg.Get(Root + "lines") is Dictionary<string, object> ls)
                    foreach (KeyValuePair<string, object> kv in ls)
                        if (!kv.Key.EndsWith("_why", StringComparison.Ordinal) && kv.Value is string v) lines[kv.Key] = v;
                return new Table(weapons.ToArray(), phrases.ToArray(), swap.ToArray(), lines);
            }
            catch (Exception) { return Table.Empty; }
        }

        static bool Has(string text, string phrase) => text.IndexOf(phrase, StringComparison.OrdinalIgnoreCase) >= 0;

        /// <summary>Does `text` name weapon k of the table?</summary>
        static bool Names(Table t, string text, int k)
        {
            string[] ps = t.Phrases[k];
            for (int i = 0; i < ps.Length; i++) if (Has(text, ps[i])) return true;
            return false;
        }

        /// <summary>The first weapon `text` names (null: none). Allocation-free.</summary>
        public static string Named(Table t, string text)
        {
            if (t == null || string.IsNullOrEmpty(text)) return null;
            for (int k = 0; k < t.Weapons.Length; k++) if (Names(t, text, k)) return t.Weapons[k];
            return null;
        }

        /// <summary>Does `text` tell the player to change hands (a swap word)?</summary>
        public static bool Swaps(Table t, string text)
        {
            if (t == null || string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < t.SwapWords.Length; i++) if (Has(text, t.SwapWords[i])) return true;
            return false;
        }

        /// <summary>THE RULE's test: `text` names a weapon, not the one `held`, and tells no change of hands. Allocation-free.</summary>
        public static bool NotHeld(Table t, string text, string held)
        {
            if (t == null || string.IsNullOrEmpty(text) || Swaps(t, text)) return false;
            bool any = false;
            for (int k = 0; k < t.Weapons.Length; k++)
            {
                if (!Names(t, text, k)) continue;
                if (held != null && string.Equals(t.Weapons[k], held, StringComparison.Ordinal)) return false; // it names the hand you hold
                any = true;
            }
            return any;
        }

        /// <summary>The order to show for raw boss order `raw` (resolved `shown`) with `held` in hand: null when it is shown as it is; the
        /// variant's raw when it names a weapon not held and the table has one; "" when it waits (no variant).</summary>
        public static string OrderFor(Table t, string raw, string shown, string held)
        {
            if (t == null || string.IsNullOrEmpty(raw)) return null;
            if (!NotHeld(t, raw, held) && !NotHeld(t, shown, held)) return null;
            return t.Lines.TryGetValue(raw, out string v) && !string.IsNullOrEmpty(v) ? v : "";
        }

        /// <summary>A lesson or teaching line to show with `held` in hand: the line itself, or null while it waits.</summary>
        public static string LessonFor(Table t, string line, string held) => NotHeld(t, line, held) ? null : line;

        /// <summary>The surfaces the counter reads.</summary>
        public enum Surface { Bar, Fight, Teach, Lesson }

        /// <summary>The proof counter (the HUD's frames, the row on or off).</summary>
        public sealed class Count
        {
            public int Frames, OnFrames, Shown, Would, Held, HeldShown, HeldWould, Variants, Waits;
            public readonly int[] BySurface = new int[4], WouldBySurface = new int[4];
            /// <summary>The distinct lines seen naming a weapon not held (drawn or would-be), with the weapon in hand (at most MaxLines).</summary>
            public readonly List<string> Lines = new List<string>();
            public const int MaxLines = 8;
            public bool Dirty;
            readonly HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            readonly string[] lastLine = new string[4], lastHeld = new string[4];
            readonly bool[] lastDrawn = new bool[4];

            public void Reset()
            {
                Frames = OnFrames = Shown = Would = Held = HeldShown = HeldWould = Variants = Waits = 0;
                Array.Clear(BySurface, 0, 4);
                Array.Clear(WouldBySurface, 0, 4);
                Lines.Clear();
                seen.Clear();
                Array.Clear(lastLine, 0, 4);
                Array.Clear(lastHeld, 0, 4);
                Array.Clear(lastDrawn, 0, 4);
                Dirty = false;
            }

            /// <summary>One HUD frame: `shown` / `would` = any surface drew / would draw (without the row) a line naming a weapon not held.</summary>
            public void Frame(bool held, bool rowOn, bool shown, bool would)
            {
                Dirty = true;
                if (held)
                {
                    Held++;
                    if (shown) HeldShown++;
                    if (would) HeldWould++;
                    return;
                }
                Frames++;
                if (rowOn) OnFrames++;
                if (shown) Shown++;
                if (would) Would++;
            }

            /// <summary>A surface's frame (not held): drawn now / would be drawn without the row, naming a weapon not held.</summary>
            public void SurfaceFrame(Surface s, bool shownNow, bool wouldNow)
            {
                if (shownNow) BySurface[(int)s]++;
                if (wouldNow) WouldBySurface[(int)s]++;
            }

            /// <summary>Remember a line naming a weapon not held (allocates only on a new line).</summary>
            public void Note(Surface s, string line, string held, bool drawn)
            {
                if (Lines.Count >= MaxLines || string.IsNullOrEmpty(line)) return;
                int k = (int)s;
                if (ReferenceEquals(line, lastLine[k]) && ReferenceEquals(held, lastHeld[k]) && drawn == lastDrawn[k]) return; // the same as last frame: no garbage
                lastLine[k] = line;
                lastHeld[k] = held;
                lastDrawn[k] = drawn;
                string key = s + "|" + line + "|" + (held ?? "") + "|" + (drawn ? "1" : "0");
                if (!seen.Add(key)) return;
                Lines.Add(s + " \"" + line + "\" holding " + (string.IsNullOrEmpty(held) ? "nothing" : held) + (drawn ? " (DRAWN)" : " (the game without the row)"));
            }

            /// <summary>The totals line.</summary>
            public string Line(string why, bool rowOnNow, double t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                return "[words71] lines naming a weapon not held " + Shown.ToString(ci) + " frames (drawn now: the bar " + BySurface[0].ToString(ci) + ", the fight line " + BySurface[1].ToString(ci)
                    + ", the teach card " + BySurface[2].ToString(ci) + ", the lesson " + BySurface[3].ToString(ci) + ")"
                    + " | would " + Would.ToString(ci) + " frames (the game without the row: the bar " + WouldBySurface[0].ToString(ci) + ", the fight line " + WouldBySurface[1].ToString(ci)
                    + ", the teach card " + WouldBySurface[2].ToString(ci) + ", the lesson " + WouldBySurface[3].ToString(ci) + ")"
                    + " | an order put in as its variant " + Variants.ToString(ci) + " times, a line held back on " + Waits.ToString(ci) + " surface-frames"
                    + " | held " + Held.ToString(ci) + " frames: drawn " + HeldShown.ToString(ci) + ", would " + HeldWould.ToString(ci) + " (the same-run pairs' on and off halves)"
                    + " | row words71 on " + OnFrames.ToString(ci) + " of " + Frames.ToString(ci) + " frames" + (rowOnNow ? "" : " (off now)")
                    + " | lines: " + (Lines.Count == 0 ? "none" : string.Join("; ", Lines))
                    + " | " + why + ", t " + t.ToString("0.00", ci) + " s (FeelClock)";
            }
        }
    }
}
