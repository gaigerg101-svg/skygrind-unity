// NIGHT45 builder S1 (rows as data; NIGHT45.md card S1, GG's J5: "lets cut fluff and old stuff"): the generic replacement for the per-night
// N4xBlocks struct, SurvivalSim.N4x file, tape format bump and six rewritten shared tests. UNWIRED tonight: nothing in the running sim reads
// this file (no tape bump; the play build unchanged). NIGHT46's G0 switches it on once (docs/NIGHT-ROWS.md, the one-time steps); after that a
// night's shared commit is its config rows plus one hooks file.
//
// The rules (each one a unit test in Assets/Tests/EditMode/NightRowsTests.cs):
// - A SIM ROW is a design row `design.nNN.<name>` with `"sim": true`, from night FirstNight (46) on. Its name ends with its night's number
//   (wall46 in design.n46), so a name is unique and says its night. Its `enabled` (a bool) is the design default; `_why` says why it exists.
//   Rows without `sim` (view rows, level rows) are not sim rows: they change what is drawn or which file is loaded, never a tape bit.
// - THE REGISTRY (NightRows.From) reads every night's sim rows ONCE, in order: nights ascending, then names by ordinal (deterministic: never
//   the JSON parser's key order). A row's index is its id for the run (IndexOf once at Init; On(id) per step: an array read, no allocation).
// - THE OFF SWITCH: `-skygrindOffNN` (Player) / `--offNN` (harness) turns OFF every row of night NN and later (NightRows.ForceOffFrom = the
//   smallest NN given): `-skygrindOff47` is NIGHT46's game, and every older switch (-skygrindOff19 ... -skygrindOff45) turns them all off,
//   as each older night's switch did for the N4xBlocks.
// - THE TAPE RECORD (NightRowsTape): self-describing, name -> bit. Per night: its number, its row names in registry order, then the bits.
//   A replay sets each named row the build knows to its recorded bit; a row the tape does not name is OFF (a tape recorded before a row
//   existed replays it off: old formats and old records replay with the new rows off); a row the tape names ON that this build does not
//   have is refused (the replay could not be the run). A tape of an older format has no record: every row off.
// - Missing config = off: a row id of -1 (the row is not in this config) reads false, so code asking for a row that a config lacks is the
//   older game.
using System;
using System.Collections.Generic;
using System.Text;

namespace Skygrind.Core
{
    /// <summary>One sim row: its night, its name (ending with the night's number) and its design default.</summary>
    public readonly struct NightRow
    {
        public readonly int night;
        public readonly string name;
        public readonly bool design;

        public NightRow(int night, string name, bool design)
        {
            this.night = night;
            this.name = name;
            this.design = design;
        }

        public override string ToString() => $"n{night}.{name}{(design ? " (on)" : "")}";
    }

    /// <summary>The registry: every night's sim rows (design.nNN.*.sim: true, NN >= FirstNight), read once, in order.</summary>
    public sealed class NightRows
    {
        /// <summary>The first night whose rows are data (NIGHT19-NIGHT43 keep their N4xBlocks; NIGHT44 and NIGHT45 have no sim rows).</summary>
        public const int FirstNight = 46;

        /// <summary>No off switch given.</summary>
        public const int NoOff = int.MaxValue;

        /// <summary>The harnesses' and the Player checks' switch: every row of this night and later is off, whatever the config says
        /// (`-skygrindOffNN` / `--offNN`: the smallest NN given; NoOff when none). A process-wide switch, set before a sim starts its run.</summary>
        public static int ForceOffFrom = NoOff;

        /// <summary>The rows, nights ascending then names by ordinal; a row's index is its id.</summary>
        public readonly NightRow[] rows;

        readonly Dictionary<string, int> ids;

        /// <summary>No rows (a config without design, or before NIGHT46).</summary>
        public static readonly NightRows Empty = new NightRows(new List<NightRow>());

        NightRows(List<NightRow> list)
        {
            list.Sort((a, b) => a.night != b.night ? a.night.CompareTo(b.night) : string.CompareOrdinal(a.name, b.name));
            rows = list.ToArray();
            ids = new Dictionary<string, int>(rows.Length, StringComparer.Ordinal);
            for (int i = 0; i < rows.Length; i++) ids[rows[i].name] = i;
        }

        /// <summary>The number of rows.</summary>
        public int Count => rows.Length;

        /// <summary>The registry from a run's config (its `design` prefix).</summary>
        public static NightRows From(GameConfig cfg) => cfg != null && cfg.Has("design") ? FromDesign(cfg.Obj("design")) : Empty;

        /// <summary>The registry from a design object (`design` of Design/unity-config.json). A malformed row is skipped here; Problems
        /// names it (the checker's job, not the boot's: a bad row never crashes a run, it stays off).</summary>
        public static NightRows FromDesign(Dictionary<string, object> design)
        {
            var list = new List<NightRow>();
            if (design == null) return new NightRows(list);
            foreach (KeyValuePair<string, object> night in design)
            {
                int nn = NightKey(night.Key);
                if (nn < FirstNight || !(night.Value is Dictionary<string, object> block)) continue;
                foreach (KeyValuePair<string, object> row in block)
                {
                    if (!(row.Value is Dictionary<string, object> r) || !(r.TryGetValue("sim", out object sim) && sim is bool s && s)) continue;
                    if (NightOfName(row.Key) != nn) continue;
                    bool on = r.TryGetValue("enabled", out object e) && e is bool eb && eb;
                    list.Add(new NightRow(nn, row.Key, on));
                }
            }
            return new NightRows(list);
        }

        /// <summary>Every rule a design breaks (empty = clean): a sim row in a night before FirstNight, a `sim` that is not a bool, a sim
        /// row whose name does not end with its night, without a bool `enabled`, or without a `_why`.</summary>
        public static List<string> Problems(Dictionary<string, object> design)
        {
            var bad = new List<string>();
            if (design == null) return bad;
            foreach (KeyValuePair<string, object> night in design)
            {
                int nn = NightKey(night.Key);
                if (nn < 0 || !(night.Value is Dictionary<string, object> block)) continue;
                foreach (KeyValuePair<string, object> row in block)
                {
                    if (!(row.Value is Dictionary<string, object> r) || !r.TryGetValue("sim", out object sim)) continue;
                    string at = "design." + night.Key + "." + row.Key;
                    if (!(sim is bool s)) { bad.Add(at + ".sim is not a bool"); continue; }
                    if (!s) continue;
                    if (nn < FirstNight) { bad.Add(at + ": sim rows are data from NIGHT" + FirstNight + " on (older nights keep their N" + nn + "Blocks)"); continue; }
                    if (NightOfName(row.Key) != nn) bad.Add(at + ": a sim row's name ends with its night's number (" + nn + ")");
                    if (!(r.TryGetValue("enabled", out object e) && e is bool)) bad.Add(at + ".enabled is missing or not a bool");
                    if (!(r.TryGetValue("_why", out object w) && w is string ws && ws.Length > 0)) bad.Add(at + "._why is missing");
                }
            }
            return bad;
        }

        /// <summary>The night of a design key "nNN" (-1 for any other key).</summary>
        public static int NightKey(string key)
        {
            if (key == null || key.Length < 2 || key[0] != 'n') return -1;
            int v = 0;
            for (int i = 1; i < key.Length; i++)
            {
                char c = key[i];
                if (c < '0' || c > '9' || i > 4) return -1;
                v = v * 10 + (c - '0');
            }
            return v;
        }

        /// <summary>The night a row's name ends with (wall46 -> 46; -1 when it ends with no number).</summary>
        public static int NightOfName(string name)
        {
            if (string.IsNullOrEmpty(name)) return -1;
            int i = name.Length;
            while (i > 0 && name[i - 1] >= '0' && name[i - 1] <= '9') i--;
            if (i == name.Length || i == 0 || name.Length - i > 4) return -1;
            int v = 0;
            for (int k = i; k < name.Length; k++) v = v * 10 + (name[k] - '0');
            return v;
        }

        /// <summary>A row's id (its index), -1 when this config has no such row (On(-1) is false: the older game).</summary>
        public int IndexOf(string name) => name != null && ids.TryGetValue(name, out int i) ? i : -1;

        /// <summary>The design defaults with the off switch applied (ForceOffFrom): what a run reads when no tape hands its own.</summary>
        public NightRowState Defaults()
        {
            var s = new NightRowState(this);
            for (int i = 0; i < rows.Length; i++) s.on[i] = rows[i].design && rows[i].night < ForceOffFrom;
            return s;
        }

        /// <summary>A run's rows: a replay's or a harness's override when given, else the defaults (N43StartRun's rule, generic).</summary>
        public NightRowState Resolve(NightRowState overrideState) => overrideState != null ? overrideState.Copy() : Defaults();

        /// <summary>Every row off: the game before NIGHT46 (an old tape, the fingerprint before the freeze records a row).</summary>
        public NightRowState AllOff() => new NightRowState(this);

        // --- the off switch ---

        /// <summary>The NN of one off switch ("-skygrindOff47" with prefix "-skygrindOff" -> 47); -1 when the argument is not one.</summary>
        public static int OffSwitchNight(string arg, string prefix)
        {
            if (arg == null || prefix == null || arg.Length <= prefix.Length || !arg.StartsWith(prefix, StringComparison.Ordinal)) return -1;
            int v = 0;
            for (int i = prefix.Length; i < arg.Length; i++)
            {
                char c = arg[i];
                if (c < '0' || c > '9' || i - prefix.Length > 3) return -1;
                v = v * 10 + (c - '0');
            }
            return v;
        }

        /// <summary>NIGHT63 R1 N5 (Logs/n61/review-n61.md N5: the one-row off switches disagreed on case): is a row's own off switch
        /// (-skygrindNo&lt;Row&gt;NN) on the command line? The ONE rule every row reads (Rim58.No's): that exact switch, case ignored (typed by hand
        /// into a reel script, "-skygrindnobossrule61" is the same switch). Allocation-free.</summary>
        public static bool HasSwitch(string[] args, string flag)
        {
            if (args == null || flag == null) return false;
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>The smallest NN among the off switches in args (NoOff when none): what ForceOffFrom takes at boot.</summary>
        public static int OffFrom(string[] args, string prefix)
        {
            int from = NoOff;
            if (args == null) return from;
            foreach (string a in args)
            {
                int nn = OffSwitchNight(a, prefix);
                if (nn >= 0 && nn < from) from = nn;
            }
            return from;
        }

        /// <summary>A view row (no `sim`) from NIGHT46 on: its design `enabled`, off when the off switch covers its night (N4xView's rule,
        /// generic: `design.nNN.<row>` with NN from the row's name). NIGHT63 D1: the reel's live override first (ReelOverride: a row named there
        /// reads the value given, over the off switch and the design; empty outside a reel, so the answer is NIGHT62's exactly), and no garbage
        /// after a row's first call (its night and its flag's path cached by name, GameConfig.DesignEnabledAt for the lookup): safe every frame
        /// (N63SameRunTests pins the answer for every design row and the zero allocation).</summary>
        public static bool ViewOn(GameConfig cfg, string row)
        {
            if (row == null) return false;
            if (ReelOverride.Count > 0 && ReelOverride.TryGetValue(row, out bool forced)) return forced;
            RowKey k = KeyOf(row);
            int nn = k.night;
            return nn >= FirstNight && nn < ForceOffFrom && cfg != null
                && (k.path != null ? cfg.DesignEnabledAt(k.path, k.nightKey, row) : cfg.Design("n" + nn + "." + row + ".enabled"));
        }

        // --- NIGHT63 D1: the reel's live override (same-run SPOT pairs) ---

        /// <summary>NIGHT63 D1 (same-run SPOT pairs; the red team's finding 2): the reel's live override of VIEW rows, by row name (exitFrame61,
        /// bossRule61, colours63 ...). A row named here reads the value given (ViewOn, Live), whatever its config, the off switch or its own
        /// command-line switch say; a row not named reads as before. Set only by the reel (FeelReel.N63Same.cs): false for the one held frame it
        /// captures a shot with the row off, then removed; the planted rows (plant63, noop63) true for the whole run. Empty outside a reel. View
        /// rows only: the sim's rows are NightRowState bits fixed at StartRun and no sim system calls ViewOn, so the sim never sees it (and the
        /// reel holds the sim in the off frame anyway). Main thread only.</summary>
        public static readonly Dictionary<string, bool> ReelOverride = new Dictionary<string, bool>(StringComparer.Ordinal);

        /// <summary>NIGHT63 D1: a view row read every frame. `decided` is what the row's own rule said (its config, the off switches, its
        /// command-line switch: read once, at build or boot); the reel's override wins when it names the row. Allocation-free. THE CONTRACT for
        /// every row D1 claims (exitFrame61, bossMotion63, bossTell63, bossColours63, colours63): call this (or ViewOn) EVERY frame, show the
        /// row's off look while it is false, and keep the row's state (springs, fades, timers) as it was, so the next frame on is the frame it
        /// would have drawn (Research/2026-10-22-wave63/D.md).</summary>
        public static bool Live(bool decided, string row) => row != null && ReelOverride.Count > 0 && ReelOverride.TryGetValue(row, out bool forced) ? forced : decided;

        /// <summary>A row's night and its flag's dotted path, made once per row name (path null: below NIGHT46, or a name with a dot: read the
        /// old way).</summary>
        sealed class RowKey
        {
            public int night;
            public string nightKey, path;
        }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<string, RowKey> rowKeys =
            new System.Collections.Concurrent.ConcurrentDictionary<string, RowKey>(StringComparer.Ordinal);

        static RowKey KeyOf(string row)
        {
            if (rowKeys.TryGetValue(row, out RowKey k)) return k;
            int nn = NightOfName(row);
            k = new RowKey { night = nn };
            if (nn >= FirstNight && row.IndexOf('.') < 0)
            {
                k.nightKey = "n" + nn;
                k.path = "design." + k.nightKey + "." + row + ".enabled";
            }
            rowKeys.TryAdd(row, k);
            return k;
        }

        // --- level rows (data, no bit: a tape records the level id it loaded; N45Levels' rule, generic) ---

        /// <summary>Each level row from NIGHT46 on (design.nNN.<row>.levelFrom / levelTo, enabled, not covered by the off switch), in
        /// registry order, swaps its levelFrom for its levelTo in the campaign order.</summary>
        public static void SwapLevels(GameConfig cfg, List<string> order)
        {
            if (cfg == null || order == null || order.Count == 0) return;
            foreach (LevelRow lr in LevelRowsOf(cfg))
            {
                if (!lr.enabled || lr.night >= ForceOffFrom) continue;
                int i = order.IndexOf(lr.from);
                if (i >= 0) order[i] = lr.to;
            }
        }

        /// <summary>The id older systems know a NIGHT46+ level by: its row's levelFrom, unconditionally (the level is only ever loaded when
        /// its row swapped it in); chains (an n47 level made from an n46 one ends at the n46 row's levelFrom); any other id is itself.
        /// Allocation-free after the first call for a config (older systems ask it every step: N43KilnStep through N45Levels.BaseId).</summary>
        public static string BaseId(GameConfig cfg, string levelId)
        {
            if (levelId == null || cfg == null) return levelId;
            LevelRow[] rows = LevelRowsOf(cfg);
            for (int hops = 0; hops < 16; hops++)
            {
                string next = null;
                for (int i = 0; i < rows.Length; i++)
                    if (string.Equals(rows[i].to, levelId, StringComparison.Ordinal)) { next = rows[i].from; break; }
                if (next == null || next == levelId) return levelId;
                levelId = next;
            }
            return levelId;
        }

        // The level rows of the last config asked (one entry: a run has one config; a harness that builds a scratch config gets its own).
        static GameConfig levelCfg;
        static LevelRow[] levelRows = new LevelRow[0];

        static LevelRow[] LevelRowsOf(GameConfig cfg)
        {
            if (ReferenceEquals(cfg, levelCfg)) return levelRows;
            LevelRow[] r = cfg.Has("design") ? LevelRows(cfg.Obj("design")).ToArray() : new LevelRow[0];
            levelRows = r;
            levelCfg = cfg;
            return r;
        }

        struct LevelRow
        {
            public int night;
            public string name, from, to;
            public bool enabled;
        }

        /// <summary>Every level row from NIGHT46 on in registry order, uncached (for checkers: SwapLevels / BaseId read the cached copy).</summary>
        static List<LevelRow> LevelRows(Dictionary<string, object> design)
        {
            var list = new List<LevelRow>();
            if (design == null) return list;
            foreach (KeyValuePair<string, object> night in design)
            {
                int nn = NightKey(night.Key);
                if (nn < FirstNight || !(night.Value is Dictionary<string, object> block)) continue;
                foreach (KeyValuePair<string, object> row in block)
                {
                    if (!(row.Value is Dictionary<string, object> r)) continue;
                    if (!(r.TryGetValue("levelFrom", out object f) && f is string fs && fs.Length > 0)) continue;
                    if (!(r.TryGetValue("levelTo", out object t) && t is string ts && ts.Length > 0)) continue;
                    list.Add(new LevelRow { night = nn, name = row.Key, from = fs, to = ts, enabled = r.TryGetValue("enabled", out object e) && e is bool eb && eb });
                }
            }
            list.Sort((a, b) => a.night != b.night ? a.night.CompareTo(b.night) : string.CompareOrdinal(a.name, b.name));
            return list;
        }

        public override string ToString()
        {
            if (rows.Length == 0) return "no NIGHT" + FirstNight + "+ sim rows";
            var sb = new StringBuilder();
            foreach (NightRow r in rows)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(r.ToString());
            }
            return sb.ToString();
        }
    }

    /// <summary>The rows a run reads (made at StartRun, never per step): one flag per registry row.</summary>
    public sealed class NightRowState
    {
        public readonly NightRows registry;
        internal readonly bool[] on;

        public NightRowState(NightRows registry)
        {
            this.registry = registry ?? NightRows.Empty;
            on = new bool[this.registry.Count];
        }

        /// <summary>Row id on? (-1, a row this config lacks: false.) An array read: safe per sim step.</summary>
        public bool On(int id) => id >= 0 && id < on.Length && on[id];

        /// <summary>A row by name (logs, tests, harnesses; a dictionary read: resolve ids once for per-step code).</summary>
        public bool On(string name) => On(registry.IndexOf(name));

        /// <summary>Set a row (harness overrides, tests). An unknown name is refused (a typo never silently tests nothing).</summary>
        public NightRowState Set(string name, bool value)
        {
            int i = registry.IndexOf(name);
            if (i < 0) throw new ArgumentException("no sim row '" + name + "' in this registry (" + registry + ")");
            on[i] = value;
            return this;
        }

        /// <summary>Is any row on?</summary>
        public bool Any
        {
            get
            {
                for (int i = 0; i < on.Length; i++)
                    if (on[i]) return true;
                return false;
            }
        }

        public NightRowState Copy()
        {
            var c = new NightRowState(registry);
            Array.Copy(on, c.on, Math.Min(on.Length, c.on.Length));
            return c;
        }

        /// <summary>A copy with every row of a night after `night` off (the composed fingerprint: rows of nights up to the last freeze run
        /// as configured, later nights' rows off until their freeze records them).</summary>
        public NightRowState Through(int night)
        {
            NightRowState c = Copy();
            for (int i = 0; i < c.on.Length; i++)
                if (registry.rows[i].night > night) c.on[i] = false;
            return c;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < on.Length; i++)
            {
                if (!on[i]) continue;
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(registry.rows[i].name);
            }
            return sb.Length == 0 ? "every NIGHT" + NightRows.FirstNight + "+ sim row off" : sb.ToString();
        }
    }

    /// <summary>A tape's rows record that cannot be read (another record version, truncated, or a row ON that this build lacks).</summary>
    public sealed class NightRowsFormatException : Exception
    {
        public NightRowsFormatException(string message) : base(message) { }
    }

    /// <summary>The planned tape record (NIGHT46's format 27 carries it once; no later night bumps the tape for a row): name -> bit.
    /// Layout (its own little-endian varints, so Core needs no Game type): Byte version (1); VarU nights; per night: VarU night, VarU rows,
    /// each row's name (VarU length, then each char as a VarU), then ceil(rows / 8) bytes of bits (bit i of byte i / 8 = row i).</summary>
    public static class NightRowsTape
    {
        /// <summary>The record's own version.</summary>
        public const byte RecordVersion = 1;

        /// <summary>The tape format that first carries the record (NIGHT46's one bump; formats 1-26: no record, every row off).</summary>
        public const int FirstTapeFormat = 27;

        /// <summary>The record of a run's rows (at StartRun / the recorder's header, never per step).</summary>
        public static byte[] Encode(NightRowState s)
        {
            NightRows reg = s?.registry ?? NightRows.Empty;
            var w = new List<byte>(16 + 12 * reg.Count);
            w.Add(RecordVersion);
            int nights = 0;
            for (int i = 0; i < reg.Count; i++)
                if (i == 0 || reg.rows[i].night != reg.rows[i - 1].night) nights++;
            VarU(w, (uint)nights);
            int at = 0;
            while (at < reg.Count)
            {
                int night = reg.rows[at].night, end = at;
                while (end < reg.Count && reg.rows[end].night == night) end++;
                VarU(w, (uint)night);
                VarU(w, (uint)(end - at));
                for (int i = at; i < end; i++) Str(w, reg.rows[i].name);
                for (int b = 0; b < (end - at + 7) / 8; b++)
                {
                    int v = 0;
                    for (int k = 0; k < 8; k++)
                    {
                        int i = at + b * 8 + k;
                        if (i < end && s.On(i)) v |= 1 << k;
                    }
                    w.Add((byte)v);
                }
                at = end;
            }
            return w.ToArray();
        }

        /// <summary>A tape's rows against this build's registry: null (a tape of format 1-26) = every row off; a row the record does not
        /// name = off; a row it names ON that the registry lacks = NightRowsFormatException (named); one it names OFF is ignored.</summary>
        public static NightRowState Decode(byte[] record, NightRows registry)
        {
            var s = new NightRowState(registry);
            if (record == null || record.Length == 0) return s;
            int p = 0;
            byte version = Byte(record, ref p);
            if (version != RecordVersion) throw new NightRowsFormatException($"rows record version {version}, this build reads version {RecordVersion} only");
            uint nights = ReadVarU(record, ref p);
            List<string> missing = null;
            for (uint n = 0; n < nights; n++)
            {
                uint night = ReadVarU(record, ref p);
                uint count = ReadVarU(record, ref p);
                if (count > (uint)(record.Length - p)) throw new NightRowsFormatException($"rows record: night {night} announces {count} rows past the record's end");
                var names = new string[count];
                for (uint i = 0; i < count; i++) names[i] = ReadStr(record, ref p);
                for (uint b = 0; b < (count + 7) / 8; b++)
                {
                    int v = Byte(record, ref p);
                    for (int k = 0; k < 8; k++)
                    {
                        uint i = b * 8 + (uint)k;
                        if (i >= count || (v & (1 << k)) == 0) continue;
                        int id = registry.IndexOf(names[i]);
                        if (id >= 0) s.on[id] = true;
                        else (missing ??= new List<string>()).Add(names[i]);
                    }
                }
            }
            if (missing != null) throw new NightRowsFormatException("the tape ran with sim rows this build does not have: " + string.Join(", ", missing.ToArray()) + " (the replay could not be the run)");
            return s;
        }

        /// <summary>The rows a tape of a given format replays with: formats before FirstTapeFormat carry no record (every row off).</summary>
        public static NightRowState ForTape(int tapeFormat, byte[] record, NightRows registry) => tapeFormat < FirstTapeFormat ? new NightRowState(registry) : Decode(record, registry);

        static void VarU(List<byte> w, uint v)
        {
            while (v >= 0x80)
            {
                w.Add((byte)(v | 0x80));
                v >>= 7;
            }
            w.Add((byte)v);
        }

        static void Str(List<byte> w, string s)
        {
            VarU(w, (uint)s.Length);
            for (int i = 0; i < s.Length; i++) VarU(w, s[i]);
        }

        static byte Byte(byte[] d, ref int p)
        {
            if (p >= d.Length) throw new NightRowsFormatException($"rows record truncated at byte {p}");
            return d[p++];
        }

        static uint ReadVarU(byte[] d, ref int p)
        {
            uint v = 0;
            for (int shift = 0; shift < 35; shift += 7)
            {
                byte b = Byte(d, ref p);
                v |= (uint)(b & 0x7f) << shift;
                if ((b & 0x80) == 0) return v;
            }
            throw new NightRowsFormatException($"rows record: a varint runs past 5 bytes at byte {p}");
        }

        static string ReadStr(byte[] d, ref int p)
        {
            uint n = ReadVarU(d, ref p);
            if (n > (uint)(d.Length - p)) throw new NightRowsFormatException($"rows record: a name of {n} chars runs past the record's end at byte {p}");
            var sb = new StringBuilder((int)n);
            for (uint i = 0; i < n; i++) sb.Append((char)ReadVarU(d, ref p));
            return sb.ToString();
        }
    }
}
