// NIGHT21 U3-35.9 beyond row 5, CHALLENGE CODES (builder W2). Unity-only: the web had a bare seed code (seeds.js seedCode) and
// nothing that held the rest of a run's setup. A code packs the run's seed, HEAT rung, mutator set, mode and target round into
// bits, adds a checksum, and writes the lot in a 32-character alphabet (5 bits a character, no look-alikes). Pasting it plays
// the same run: a decoded Spec is applied to the RunSetup exactly as the TITLE's own picks are, so the run (and its tape,
// format 6) is the one the raw seed + mutators would start. Every number (alphabet, length, field and checksum widths) is in
// Design/unity-config.json design.n21.challenge, each with its _why; the switch is design.n21.challenge.enabled (read straight
// from the config, no N21Blocks bit: the tape format is untouched) and --off21 (N21Blocks.ForceOff) also turns it off.
// Layout, most significant bit first: [spare zeros][seed][heat][mutator mask][mode][target][checksum]. The checksum is a
// 64-bit mix of the payload folded to checkBits.
using System;
using System.Collections.Generic;
using System.Text;

namespace Skygrind.Core
{
    public static class Challenge21
    {
        /// <summary>What a code holds: the run setup (RunSetup.seed / heat / mutators / mode) and the target round (0: none).</summary>
        public struct Spec
        {
            public uint seed;
            public int heat;
            public string mode; // RunSetup.Survival / Grind
            public int target;
            public List<string> mutators; // MutatorMath ids; the code keeps them in MutatorMath.All order
        }

        const int ModeBits = 2; // survival, (1: the removed DAILY, null, a code with it is rejected), grind (+1 spare value that decodes as a rejected code)
        static readonly string[] Modes = { RunSetup.Survival, null, RunSetup.Grind };

        /// <summary>Is the row on? design.n21.challenge.enabled, and --off21 (N21Blocks.ForceOff) forces it off.</summary>
        public static bool Enabled(GameConfig cfg) => !N21Blocks.ForceOff && cfg.Design("n21.challenge.enabled");

        sealed class Layout
        {
            public string alphabet;
            public int length, checkBits, seedBits, heatBits, targetBits, groupSize;
            public int mutBits = MutatorMath.All.Length;
            public int PayloadBits => seedBits + heatBits + mutBits + ModeBits + targetBits;
        }

        static Layout Read(GameConfig cfg)
        {
            var l = new Layout
            {
                alphabet = cfg.Str("design.n21.challenge.alphabet"),
                length = cfg.Int("design.n21.challenge.length"),
                checkBits = cfg.Int("design.n21.challenge.checkBits"),
                seedBits = cfg.Int("design.n21.challenge.seedBits"),
                heatBits = cfg.Int("design.n21.challenge.heatBits"),
                targetBits = cfg.Int("design.n21.challenge.targetBits"),
                groupSize = cfg.Int("design.n21.challenge.groupSize"),
            };
            if (l.alphabet.Length != 32) throw new InvalidOperationException("design.n21.challenge.alphabet must have 32 characters (5 bits each)");
            if (l.PayloadBits + l.checkBits > l.length * 5 || l.PayloadBits + l.checkBits > 63)
                throw new InvalidOperationException("design.n21.challenge: the fields and checksum do not fit the code length");
            return l;
        }

        static ulong Check(ulong payload, int bits)
        {
            // splitmix64 finalizer over the payload: every payload bit reaches every checksum bit.
            ulong z = payload + 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            z ^= z >> 31;
            return z & ((1UL << bits) - 1);
        }

        /// <summary>The code for a run setup, hyphen-grouped (XXXX-XXXX-XXXX). Throws ArgumentException on a field out of its range.</summary>
        public static string Encode(Spec s, GameConfig cfg)
        {
            Layout l = Read(cfg);
            if (s.seed >= 1UL << Math.Min(l.seedBits, 32)) throw new ArgumentException("seed out of range: " + s.seed);
            if (s.heat < 0 || s.heat >= 1 << l.heatBits) throw new ArgumentException("heat out of range: " + s.heat);
            if (s.target < 0 || s.target >= 1 << l.targetBits) throw new ArgumentException("target out of range: " + s.target);
            int mode = Array.IndexOf(Modes, s.mode);
            if (mode < 0) throw new ArgumentException("unknown mode: " + s.mode);
            int mask = 0;
            if (s.mutators != null)
                foreach (string id in s.mutators)
                {
                    int i = Array.IndexOf(MutatorMath.All, id);
                    if (i < 0) throw new ArgumentException("unknown mutator: " + id);
                    mask |= 1 << i;
                }
            ulong p = s.seed;
            p = (p << l.heatBits) | (uint)s.heat;
            p = (p << l.mutBits) | (uint)mask;
            p = (p << ModeBits) | (uint)mode;
            p = (p << l.targetBits) | (uint)s.target;
            ulong v = (p << l.checkBits) | Check(p, l.checkBits);
            var sb = new StringBuilder(l.length + l.length / Math.Max(1, l.groupSize));
            for (int i = 0; i < l.length; i++)
            {
                if (i > 0 && l.groupSize > 0 && i % l.groupSize == 0) sb.Append('-');
                sb.Append(l.alphabet[(int)((v >> (5 * (l.length - 1 - i))) & 31)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// A typed code back to its run setup, or false: wrong length, a character outside the alphabet (O reads as 0, I and L as 1;
        /// hyphens, spaces and case are ignored), a spare bit set, a checksum that does not match, or an unknown mode.
        /// </summary>
        public static bool TryDecode(string text, GameConfig cfg, out Spec s)
        {
            s = default;
            if (text == null) return false;
            Layout l = Read(cfg);
            var sb = new StringBuilder(text.Length);
            foreach (char raw in text)
            {
                if (raw == '-' || char.IsWhiteSpace(raw)) continue;
                char c = char.ToUpperInvariant(raw);
                sb.Append(c == 'O' ? '0' : c == 'I' || c == 'L' ? '1' : c);
            }
            if (sb.Length != l.length) return false;
            ulong v = 0;
            foreach (char c in sb.ToString())
            {
                int d = l.alphabet.IndexOf(c);
                if (d < 0) return false;
                v = (v << 5) | (uint)d;
            }
            int total = l.PayloadBits + l.checkBits;
            if ((v >> total) != 0) return false; // the spare bits read 0
            ulong p = v >> l.checkBits;
            if ((v & ((1UL << l.checkBits) - 1)) != Check(p, l.checkBits)) return false;
            int target = (int)(p & ((1UL << l.targetBits) - 1)); p >>= l.targetBits;
            int mode = (int)(p & ((1UL << ModeBits) - 1)); p >>= ModeBits;
            int mask = (int)(p & ((1UL << l.mutBits) - 1)); p >>= l.mutBits;
            int heat = (int)(p & ((1UL << l.heatBits) - 1)); p >>= l.heatBits;
            if (mode >= Modes.Length || Modes[mode] == null) return false; // NIGHT24: the DAILY (1) is gone; its old codes are refused
            var muts = new List<string>();
            for (int i = 0; i < MutatorMath.All.Length; i++)
                if ((mask & (1 << i)) != 0) muts.Add(MutatorMath.All[i]);
            s = new Spec { seed = (uint)p, heat = heat, mode = Modes[mode], target = target, mutators = muts };
            return true;
        }

        /// <summary>The spec a RunSetup describes (the RESULTS screen's code for the run just played).</summary>
        public static Spec FromSetup(RunSetup setup, uint seed, int target) => new Spec
        {
            seed = seed, heat = setup.heat, mode = setup.mode, target = target, mutators = new List<string>(setup.mutators),
        };

        /// <summary>Apply a decoded code to a RunSetup (the TITLE's CODE field): the next StartRun plays that run.</summary>
        public static void Apply(Spec s, RunSetup setup)
        {
            setup.seed = s.seed;
            setup.heat = s.heat;
            setup.mode = s.mode;
            setup.mutators.Clear();
            setup.mutators.AddRange(s.mutators);
        }

        // --- NIGHT22 U3-36.8 (builder W2): the reader, the RESULTS line, the TITLE field's model --------------------------------

        /// <summary>
        /// A3: the reader for RunSetup.mutators. Adds the setup's picks (the TITLE's fun toggles, or a typed code's mutator set) to
        /// the run's list `into` (RunStats.mutators) in MutatorMath.All order, skipping repeats and ones the port can't play
        /// (Mutators.Available). Nothing is added when the row is off (design.n21.challenge.enabled, --off21) or the setup holds none,
        /// so a run with no code is the run NIGHT21 played. Called in StartRun right after the rung's mutators are listed, before
        /// heat.ApplyRung (which applies the list). Returns how many it added.
        /// </summary>
        public static int ReadSetupMutators(GameConfig cfg, RunSetup setup, List<string> into)
        {
            if (!Enabled(cfg) || setup == null || into == null || setup.mutators.Count == 0) return 0;
            int added = 0;
            foreach (string id in MutatorMath.All)
                if (setup.mutators.Contains(id) && Mutators.Available(id) && !into.Contains(id)) { into.Add(id); added++; }
            return added;
        }

        /// <summary>The RESULTS line: "CODE XXXX-XXXX-XXXX", the code of the run just played (seed, rung, mode, picks); null when the row is off or a field is out of range.</summary>
        public static string ResultsLine(GameConfig cfg, RunSetup setup, uint seed, int target)
        {
            if (!Enabled(cfg) || setup == null) return null;
            try { return "CODE " + Encode(FromSetup(setup, seed, target), cfg); }
            catch (ArgumentException) { return null; }
        }

        /// <summary>
        /// The TITLE CODE field's model, drivable from a pad: the slots of one code, a cursor. UP / DOWN (Cycle) steps the slot's
        /// character through the code alphabet (blank first), LEFT / RIGHT (Move) moves the cursor, a typed or pasted character fills
        /// the slot and moves on (Type, Paste; the hand-typing rules of TryDecode: case, hyphens and spaces ignored, O is 0, I and L
        /// are 1), Back clears the slot behind the cursor. Valid when all slots are full and TryDecode accepts them.
        /// </summary>
        public sealed class Entry
        {
            readonly string alphabet;
            readonly int groupSize;
            readonly char[] slots;
            public int Cursor { get; private set; }
            public int Length => slots.Length;

            public Entry(GameConfig cfg)
            {
                Layout l = Read(cfg);
                alphabet = l.alphabet;
                groupSize = l.groupSize;
                slots = new char[l.length];
            }

            public void Clear() { Array.Clear(slots, 0, slots.Length); Cursor = 0; }

            public void Move(int d) { Cursor = Math.Max(0, Math.Min(slots.Length - 1, Cursor + d)); }

            /// <summary>Step the slot under the cursor: blank, then the alphabet in order; d = +1 / -1 wraps.</summary>
            public void Cycle(int d)
            {
                int cur = slots[Cursor] == 0 ? -1 : alphabet.IndexOf(slots[Cursor]);
                int n = alphabet.Length + 1; // the blank is the extra value
                int next = ((cur + 1 + d) % n + n) % n - 1;
                slots[Cursor] = next < 0 ? (char)0 : alphabet[next];
            }

            /// <summary>One typed character: false when it is not in the alphabet (hyphens and spaces are skipped quietly, returning true).</summary>
            public bool Type(char raw)
            {
                if (raw == '-' || char.IsWhiteSpace(raw)) return true;
                char c = char.ToUpperInvariant(raw);
                c = c == 'O' ? '0' : c == 'I' || c == 'L' ? '1' : c;
                if (alphabet.IndexOf(c) < 0) return false;
                slots[Cursor] = c;
                if (Cursor < slots.Length - 1) Cursor++;
                return true;
            }

            /// <summary>A pasted text from the cursor (Clear first for a fresh code): the characters it can read, in order; stops at the first it can't.</summary>
            public int Paste(string text)
            {
                if (text == null) return 0;
                int n = 0;
                foreach (char ch in text)
                {
                    if (ch == '-' || char.IsWhiteSpace(ch)) continue;
                    if (!Type(ch)) break;
                    n++;
                }
                return n;
            }

            /// <summary>Clear the slot under the cursor if it holds a character, else step back and clear that one.</summary>
            public void Back()
            {
                if (slots[Cursor] != 0) { slots[Cursor] = (char)0; return; }
                if (Cursor > 0) { Cursor--; slots[Cursor] = (char)0; }
            }

            public int Filled { get { int n = 0; foreach (char c in slots) if (c != 0) n++; return n; } }

            /// <summary>The code as shown, hyphen-grouped, '_' for a blank slot.</summary>
            public string Text()
            {
                var sb = new StringBuilder(slots.Length + slots.Length / Math.Max(1, groupSize));
                for (int i = 0; i < slots.Length; i++)
                {
                    if (i > 0 && groupSize > 0 && i % groupSize == 0) sb.Append('-');
                    sb.Append(slots[i] == 0 ? '_' : slots[i]);
                }
                return sb.ToString();
            }

            /// <summary>All slots full and the checksum right: the run it holds.</summary>
            public bool TryResult(GameConfig cfg, out Spec s)
            {
                s = default;
                return Filled == slots.Length && TryDecode(Text(), cfg, out s);
            }
        }
    }
}
