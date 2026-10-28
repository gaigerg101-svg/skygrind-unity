// NIGHT13 U3-26.10 (agent E): the run snapshot, save-and-quit's Core half (NIGHT14-RESEARCH.md section 4; new design:
// the web has no save). RunSnapshot writes `{ "saveVersion": 1, "sections": [...], "<system>": {...}, ... }` from an
// ordered list of (name, IRunState) the caller gives, and reads it back into a set of systems in the same order.
//
// The contract (what "restore" means):
// - A snapshot is taken at a round break, where no enemy is alive (the director is Done; nothing spawns in a break).
//   Husks.Save refuses one while an enemy is alive (StateException), so a snapshot can't silently drop a fight.
// - Load restores into systems built by the same code on the same map and config, after their run-start setup
//   (SurvivalGame.StartRun on the saved seed: it builds the run's structure, the relays, traps, hold spots and their
//   Buyables items, which Load checks by id and refuses when they differ). Load then overwrites every field that changes
//   during a run, bit for bit: doubles exactly (StateWriter), lists in order, the Mulberry32 streams by State, a
//   Dictionary whose removals decide its iteration order by its slot layout (DictLayout), the dead enemies' pool slots
//   and registry order (Husks), references to enemies by their registry index.
// - Load emits no event and calls no owner hook (no perk `apply`, no `buy`): every effect a hook or a handler had is a
//   field of the system it changed, saved there. Views refresh from the systems (Zones / Protocol / Traps bump Version).
// - No partial load: the whole text is parsed, the version and every section checked, and a backup snapshot of the
//   target systems taken before anything changes; if a section's Load fails, the backup is loaded back (rollback) and a
//   StateException says which section failed. If the rollback fails too (it shouldn't: the backup came from these
//   systems a moment before), the exception says so and the caller must rebuild the run (ToTitle).
// - Another saveVersion is refused with a StateException naming both versions (the caller discards the file).
// No per-step work: everything here runs on save and load only; the systems' Save / Load members add nothing to Step.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Skygrind.Core
{
    public static class RunSnapshot
    {
        /// <summary>The snapshot format. A save of another version is refused (and discarded by the caller).</summary>
        public const int SaveVersion = 1;

        static void Check(IReadOnlyList<(string name, IRunState state)> systems)
        {
            if (systems == null) throw new ArgumentNullException(nameof(systems));
            var names = new HashSet<string>();
            foreach (var (name, state) in systems)
            {
                if (string.IsNullOrEmpty(name) || name == "saveVersion" || name == "sections") throw new ArgumentException($"run snapshot: bad section name '{name}'");
                if (state == null) throw new ArgumentException($"run snapshot: section '{name}' has no system (leave out a system that is off)");
                if (!names.Add(name)) throw new ArgumentException($"run snapshot: section '{name}' is listed twice");
            }
        }

        /// <summary>The snapshot of `systems`, in their order. Throws StateException when one can't be saved now (an enemy alive).</summary>
        public static string Save(IReadOnlyList<(string name, IRunState state)> systems)
        {
            Check(systems);
            var w = new StateWriter();
            w.I("saveVersion", SaveVersion);
            var names = new List<string>(systems.Count);
            foreach (var (name, _) in systems) names.Add(name);
            w.Strings("sections", names);
            foreach (var (name, state) in systems)
            {
                w.Begin(name);
                try
                {
                    state.Save(w);
                }
                catch (StateException e)
                {
                    throw new StateException($"snapshot: saving '{name}': {e.Message}");
                }
                w.End();
            }
            return w.ToJson();
        }

        /// <summary>
        /// Load a snapshot into `systems` (the same names, in the same order, as it was saved). All or nothing: a failure
        /// rolls every system back to what it was and throws StateException.
        /// </summary>
        public static void Load(string json, IReadOnlyList<(string name, IRunState state)> systems)
        {
            Check(systems);
            StateReader[] sections = Open(json, systems);
            string backup = Save(systems); // before anything changes (throws, changing nothing, if the target can't be saved)
            int at = 0;
            try
            {
                for (; at < systems.Count; at++) systems[at].state.Load(sections[at]);
            }
            catch (Exception e)
            {
                string why = $"snapshot: loading '{systems[at].name}' failed: {e.Message}";
                try
                {
                    StateReader[] back = Open(backup, systems);
                    for (int i = 0; i < systems.Count; i++) systems[i].state.Load(back[i]);
                }
                catch (Exception e2)
                {
                    throw new StateException(why + $"; the rollback failed too ({e2.Message}): rebuild the run");
                }
                throw new StateException(why + " (rolled back: nothing changed)");
            }
        }

        /// <summary>Parse, check the version and the section list, and open every section (nothing changes yet).</summary>
        static StateReader[] Open(string json, IReadOnlyList<(string name, IRunState state)> systems)
        {
            if (json == null) throw new StateException("snapshot: no text");
            var r = new StateReader(json);
            if (!r.Has("saveVersion")) throw new StateException("snapshot: no saveVersion (not a run snapshot)");
            long version = r.I("saveVersion");
            if (version != SaveVersion) throw new StateException($"snapshot: saveVersion {version.ToString(CultureInfo.InvariantCulture)}, but this build reads saveVersion {SaveVersion} only");
            List<string> names = r.Strings("sections");
            if (names.Count != systems.Count) throw new StateException($"snapshot: {names.Count} sections saved, {systems.Count} systems to load ({string.Join(", ", names)})");
            var sections = new StateReader[systems.Count];
            for (int i = 0; i < systems.Count; i++)
            {
                if (names[i] != systems[i].name) throw new StateException($"snapshot: section {i} is '{names[i]}', the game's is '{systems[i].name}'");
                sections[i] = r.Section(systems[i].name);
            }
            return sections;
        }
    }

    /// <summary>The systems' shared Save / Load helpers (vectors, streams, ordered dictionaries, packed arrays, enemy references).</summary>
    public static class StateIO
    {
        public static void V(this StateWriter w, string key, Vec3 v) => w.Doubles(key, new[] { v.x, v.y, v.z });

        public static Vec3 V(this StateReader r, string key)
        {
            List<double> d = r.Doubles(key);
            if (d.Count != 3) throw new StateException($"snapshot: {key} is not a vector");
            return new Vec3(d[0], d[1], d[2]);
        }

        /// <summary>A Vec3? as [] (null) or [x, y, z].</summary>
        public static void VN(this StateWriter w, string key, Vec3? v) => w.Doubles(key, v.HasValue ? new[] { v.Value.x, v.Value.y, v.Value.z } : new double[0]);

        public static Vec3? VN(this StateReader r, string key)
        {
            List<double> d = r.Doubles(key);
            if (d.Count == 0) return null;
            if (d.Count != 3) throw new StateException($"snapshot: {key} is not a vector or []");
            return new Vec3(d[0], d[1], d[2]);
        }

        /// <summary>A list of Vec3 as one flat number list.</summary>
        public static void Vecs(this StateWriter w, string key, IList<Vec3> list)
        {
            var d = new double[list.Count * 3];
            for (int i = 0; i < list.Count; i++)
            {
                d[i * 3] = list[i].x;
                d[i * 3 + 1] = list[i].y;
                d[i * 3 + 2] = list[i].z;
            }
            w.Doubles(key, d);
        }

        public static List<Vec3> Vecs(this StateReader r, string key)
        {
            List<double> d = r.Doubles(key);
            if (d.Count % 3 != 0) throw new StateException($"snapshot: {key} is not a list of vectors");
            var list = new List<Vec3>(d.Count / 3);
            for (int i = 0; i < d.Count; i += 3) list.Add(new Vec3(d[i], d[i + 1], d[i + 2]));
            return list;
        }

        /// <summary>A Mulberry32 stream: its State.</summary>
        public static void Rng(this StateWriter w, string key, Mulberry32 rng) => w.I(key, rng.State);

        /// <summary>A new stream at the saved State (the caller rebinds any delegate made from the old one).</summary>
        public static Mulberry32 Rng(this StateReader r, string key) => new Mulberry32(0) { State = r.U(key) };

        /// <summary>A section written by another system's (or a part's) Save.</summary>
        public static void Sub(this StateWriter w, string key, IRunState part)
        {
            w.Begin(key);
            part.Save(w);
            w.End();
        }

        public static void Sub(this StateReader r, string key, IRunState part) => part.Load(r.Section(key));

        /// <summary>A string -> double dictionary in its iteration order (keys, values).</summary>
        public static void SD(this StateWriter w, string key, Dictionary<string, double> d)
        {
            w.Begin(key);
            w.Strings("k", new List<string>(d.Keys));
            w.Doubles("v", new List<double>(d.Values));
            w.End();
        }

        /// <summary>Read into `into` (cleared, then filled in the saved order: its iteration order is the saved one).</summary>
        public static void SD(this StateReader r, string key, Dictionary<string, double> into)
        {
            ReadPairs(r, key, out List<string> k, out List<double> v);
            into.Clear();
            for (int i = 0; i < k.Count; i++) into.Add(k[i], v[i]);
        }

        public static void ReadPairs(StateReader r, string key, out List<string> k, out List<double> v)
        {
            StateReader s = r.Section(key);
            k = s.Strings("k");
            v = s.Doubles("v");
            if (k.Count != v.Count) throw new StateException($"snapshot: {key}: {k.Count} keys, {v.Count} values");
            if (new HashSet<string>(k).Count != k.Count || k.Contains(null)) throw new StateException($"snapshot: {key} has a null or repeated key");
        }

        /// <summary>A string -> int dictionary in its iteration order.</summary>
        public static void SI(this StateWriter w, string key, Dictionary<string, int> d)
        {
            w.Begin(key);
            w.Strings("k", new List<string>(d.Keys));
            w.Ints("v", new List<int>(d.Values));
            w.End();
        }

        public static void SI(this StateReader r, string key, Dictionary<string, int> into)
        {
            StateReader s = r.Section(key);
            List<string> k = s.Strings("k");
            List<int> v = s.Ints("v");
            if (k.Count != v.Count || new HashSet<string>(k).Count != k.Count || k.Contains(null)) throw new StateException($"snapshot: {key} is not a dictionary");
            into.Clear();
            for (int i = 0; i < k.Count; i++) into.Add(k[i], v[i]);
        }

        /// <summary>The layout of a Dictionary that has had removals (DictLayout.Capture): its slot count and free chain.</summary>
        public static void Layout<TKey, TValue>(this StateWriter w, string key, Dictionary<TKey, TValue> d)
        {
            if (!DictLayout.Capture(d, out int slots, out List<int> chain))
            {
                slots = d.Count; // a runtime whose layout this can't read: rebuilt in iteration order (logged by DictLayout.Supported)
                chain = new List<int>();
            }
            w.Begin(key);
            w.I("slots", slots);
            w.Ints("chain", chain);
            w.End();
        }

        public static void Layout(this StateReader r, string key, out int slots, out List<int> chain)
        {
            StateReader s = r.Section(key);
            slots = s.Int("slots");
            chain = s.Ints("chain");
        }

        /// <summary>float[0..count) packed as base64 of its little-endian bits (exact; the nav fields' Float32Arrays).</summary>
        public static void Floats(this StateWriter w, string key, float[] a, int count)
        {
            var b = new byte[count * 4];
            if (count > 0) Buffer.BlockCopy(a, 0, b, 0, b.Length);
            w.S(key, Convert.ToBase64String(b));
        }

        public static float[] Floats(this StateReader r, string key)
        {
            byte[] b = Bytes(r, key);
            if (b.Length % 4 != 0) throw new StateException($"snapshot: {key} is not packed floats");
            var a = new float[b.Length / 4];
            Buffer.BlockCopy(b, 0, a, 0, b.Length);
            return a;
        }

        public static void IntsPacked(this StateWriter w, string key, int[] a, int count)
        {
            var b = new byte[count * 4];
            if (count > 0) Buffer.BlockCopy(a, 0, b, 0, b.Length);
            w.S(key, Convert.ToBase64String(b));
        }

        public static int[] IntsPacked(this StateReader r, string key)
        {
            byte[] b = Bytes(r, key);
            if (b.Length % 4 != 0) throw new StateException($"snapshot: {key} is not packed ints");
            var a = new int[b.Length / 4];
            Buffer.BlockCopy(b, 0, a, 0, b.Length);
            return a;
        }

        public static void Bytes(this StateWriter w, string key, byte[] a) => w.S(key, Convert.ToBase64String(a));

        public static byte[] Bytes(this StateReader r, string key)
        {
            string s = r.S(key) ?? throw new StateException($"snapshot: {key} is null");
            try
            {
                return Convert.FromBase64String(s);
            }
            catch (FormatException)
            {
                throw new StateException($"snapshot: {key} is not base64");
            }
        }

        /// <summary>An enemy reference as its index in the registry (-1: none). An enemy outside the registry can't be saved.</summary>
        public static int EnemyIndex(EnemyRegistry registry, Enemy e)
        {
            if (e == null) return -1;
            int i = registry.all.IndexOf(e);
            if (i < 0) throw new StateException($"snapshot: a reference to an enemy ({e.kind} {e.id}) outside the registry");
            return i;
        }

        /// <summary>The enemy at a saved registry index (-1: null). The registry must be restored first (the "husks" section).</summary>
        public static Enemy EnemyAt(EnemyRegistry registry, int i)
        {
            if (i == -1) return null;
            if (i < 0 || i >= registry.all.Count) throw new StateException($"snapshot: enemy index {i} past the registry's {registry.all.Count} (load the 'husks' section first)");
            return registry.all[i];
        }

        public static List<int> EnemyIndices(EnemyRegistry registry, IEnumerable<Enemy> list)
        {
            var r = new List<int>();
            foreach (Enemy e in list) r.Add(EnemyIndex(registry, e));
            return r;
        }
    }

    /// <summary>
    /// The slot layout of a Dictionary&lt;K, V&gt;. Its iteration order is its slot order, and a removal frees a slot the
    /// next Add reuses (the last freed first), so after removals the order is not the insertion order (PORT-RULES section
    /// 4). Where that order is behaviour (HuskBrain's records: the think order and its draws; Economy's boosts: the
    /// product's order), a restore has to put each live entry back in its slot and rebuild the free chain. Capture reads
    /// the runtime's own fields by reflection (Mono's reference source: entries / count / freeList, a free entry's `next`
    /// the next free slot; CoreCLR 3+: _entries / _count / _freeList, `next` encoded as -3 - next); Rebuild replays Adds
    /// and Removes to reach the same layout. A runtime this can't read (Capture false) is restored in iteration order.
    /// </summary>
    public static class DictLayout
    {
        const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// The slot count (live + free, from 0) and the free chain (slot indices, the next Add's first) of `d`. False when
        /// this runtime's Dictionary isn't one this reads (slots = Count, chain empty: iteration order).
        /// </summary>
        public static bool Capture<TKey, TValue>(Dictionary<TKey, TValue> d, out int slots, out List<int> chain)
        {
            chain = new List<int>();
            slots = d.Count;
            Type t = d.GetType();
            FieldInfo entriesF = t.GetField("_entries", Any) ?? t.GetField("entries", Any);
            FieldInfo countF = t.GetField("_count", Any) ?? t.GetField("count", Any);
            FieldInfo freeF = t.GetField("_freeList", Any) ?? t.GetField("freeList", Any);
            if (entriesF == null || countF == null || freeF == null) return false;
            var entries = entriesF.GetValue(d) as Array;
            int count = (int)countF.GetValue(d);
            int free = (int)freeF.GetValue(d);
            if (entries == null) return d.Count == 0 && count == 0;
            FieldInfo nextF = entries.GetType().GetElementType()?.GetField("next", Any);
            if (nextF == null) return false;
            var seen = new HashSet<int>();
            while (free >= 0)
            {
                if (free >= count || !seen.Add(free)) return false;
                chain.Add(free);
                int next = (int)nextF.GetValue(entries.GetValue(free));
                free = next <= -2 ? -3 - next : next; // CoreCLR 3+ stores -3 - next (always <= -2); Mono the next itself (>= -1)
            }
            if (count - chain.Count != d.Count)
            {
                chain.Clear();
                return false;
            }
            slots = count;
            return true;
        }

        /// <summary>Did Capture read this runtime's layout (a probe dictionary with a removal)?</summary>
        public static bool Supported
        {
            get
            {
                var probe = new Dictionary<int, int> { [1] = 1, [2] = 2 };
                probe.Remove(1);
                return Capture(probe, out int slots, out List<int> chain) && slots == 2 && chain.Count == 1 && chain[0] == 0;
            }
        }

        /// <summary>
        /// Rebuild `d` as `slots` slots: the live entries (keys / values, in iteration order) in the slots not in `chain`,
        /// the others freed so `chain` is the free list (its first slot the next Add's). `dummy(slot)` gives a placeholder
        /// key that is none of `keys` (it is added, then removed).
        /// </summary>
        public static void Rebuild<TKey, TValue>(Dictionary<TKey, TValue> d, IList<TKey> keys, IList<TValue> values, int slots, IList<int> chain, Func<int, TKey> dummy)
        {
            if (keys.Count != values.Count || slots - chain.Count != keys.Count || slots < 0) throw new StateException($"snapshot: a dictionary layout of {slots} slots, {chain.Count} free, {keys.Count} entries");
            var free = new bool[slots];
            foreach (int c in chain)
            {
                if (c < 0 || c >= slots || free[c]) throw new StateException($"snapshot: a dictionary free chain with slot {c} of {slots}");
                free[c] = true;
            }
            d.Clear();
            var placeholders = new TKey[slots];
            int k = 0;
            try
            {
                for (int i = 0; i < slots; i++)
                {
                    if (free[i])
                    {
                        placeholders[i] = dummy(i);
                        d.Add(placeholders[i], default);
                    }
                    else
                    {
                        d.Add(keys[k], values[k]);
                        k++;
                    }
                }
            }
            catch (ArgumentException e)
            {
                throw new StateException($"snapshot: a repeated dictionary key ({e.Message})");
            }
            for (int j = chain.Count - 1; j >= 0; j--) d.Remove(placeholders[chain[j]]); // the last removed heads the free list
        }
    }

    /// <summary>
    /// Field-by-field snapshots by reflection, for the classes whose files this item doesn't own (Mover: CLAUDE.md rule 3;
    /// AimAssist: agent I's). Supported field types: double, float, int, uint, long, bool, string, enums, Vec3, nullable
    /// and plain structs of these (recursively), one-dimensional arrays of them, Enemy references (a registry index), an
    /// `object` holding null / int / double / string, Dictionary&lt;string, int&gt;, and classes read and written in place.
    /// Anything else throws StateException when saved, naming the field (a new field of a new kind is loud, never lost).
    /// </summary>
    public static class FieldSnapshot
    {
        const BindingFlags Inst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static FieldInfo Field(Type t, string name) => t.GetField(name, Inst) ?? throw new InvalidOperationException($"run snapshot: {t.Name} has no field '{name}'");

        public static FieldInfo[] Fields(Type t) => t.GetFields(Inst);

        public static void Write(StateWriter w, string key, Type type, object value, EnemyRegistry registry)
        {
            if (type == typeof(double)) w.D(key, (double)value);
            else if (type == typeof(float)) w.D(key, (float)value);
            else if (type == typeof(int)) w.I(key, (int)value);
            else if (type == typeof(uint)) w.I(key, (uint)value);
            else if (type == typeof(long)) w.I(key, (long)value);
            else if (type == typeof(bool)) w.B(key, (bool)value);
            else if (type == typeof(string)) w.S(key, (string)value);
            else if (type.IsEnum) w.I(key, Convert.ToInt64(value, CultureInfo.InvariantCulture));
            else if (type == typeof(Vec3)) w.V(key, (Vec3)value);
            else if (Nullable.GetUnderlyingType(type) is Type inner)
            {
                w.Begin(key);
                w.B("has", value != null);
                if (value != null) Write(w, "v", inner, value, registry);
                w.End();
            }
            else if (typeof(Enemy).IsAssignableFrom(type))
            {
                if (registry == null && value != null) throw new StateException($"snapshot: {key}: an enemy reference with no registry");
                w.I(key, value == null ? -1 : StateIO.EnemyIndex(registry, (Enemy)value));
            }
            else if (type == typeof(object))
            {
                w.Begin(key);
                switch (value)
                {
                    case null: w.S("t", "null"); break;
                    case int i: w.S("t", "int"); w.I("v", i); break;
                    case double d: w.S("t", "double"); w.D("v", d); break;
                    case string s: w.S("t", "string"); w.S("v", s); break;
                    default: throw new StateException($"snapshot: {key} holds a {value.GetType().Name}");
                }
                w.End();
            }
            else if (type == typeof(Dictionary<string, int>))
            {
                if (value == null) throw new StateException($"snapshot: {key} is a null dictionary");
                w.SI(key, (Dictionary<string, int>)value);
            }
            else if (type.IsArray && type.GetArrayRank() == 1)
            {
                var a = (Array)value;
                w.Begin(key);
                w.I("n", a?.Length ?? -1);
                Type e = type.GetElementType();
                if (a != null) for (int i = 0; i < a.Length; i++) Write(w, i.ToString(CultureInfo.InvariantCulture), e, a.GetValue(i), registry);
                w.End();
            }
            else if (type.IsValueType && !type.IsPrimitive)
            {
                w.Begin(key);
                foreach (FieldInfo f in Fields(type)) Write(w, f.Name, f.FieldType, f.GetValue(value), registry);
                w.End();
            }
            else throw new StateException($"snapshot: can't save {key} (a {type.Name})");
        }

        /// <summary>The value of `key` as a `type` (structs and arrays reuse `current` when they can).</summary>
        public static object Read(StateReader r, string key, Type type, object current, EnemyRegistry registry)
        {
            if (type == typeof(double)) return r.D(key);
            if (type == typeof(float)) return (float)r.D(key);
            if (type == typeof(int)) return r.Int(key);
            if (type == typeof(uint)) return r.U(key);
            if (type == typeof(long)) return r.I(key);
            if (type == typeof(bool)) return r.B(key);
            if (type == typeof(string)) return r.S(key);
            if (type.IsEnum) return Enum.ToObject(type, r.I(key));
            if (type == typeof(Vec3)) return r.V(key);
            if (Nullable.GetUnderlyingType(type) is Type inner)
            {
                StateReader s = r.Section(key);
                return s.B("has") ? Read(s, "v", inner, null, registry) : null;
            }
            if (typeof(Enemy).IsAssignableFrom(type))
            {
                int i = r.Int(key);
                if (i == -1) return null;
                if (registry == null) throw new StateException($"snapshot: {key}: an enemy reference with no registry");
                Enemy e = StateIO.EnemyAt(registry, i);
                if (!type.IsInstanceOfType(e)) throw new StateException($"snapshot: {key}: enemy {i} is a {e.GetType().Name}, not a {type.Name}");
                return e;
            }
            if (type == typeof(object))
            {
                StateReader s = r.Section(key);
                switch (s.S("t"))
                {
                    case "null": return null;
                    case "int": return s.Int("v");
                    case "double": return s.D("v");
                    case "string": return s.S("v");
                    default: throw new StateException($"snapshot: {key} has an unknown boxed type");
                }
            }
            if (type == typeof(Dictionary<string, int>))
            {
                var d = current as Dictionary<string, int> ?? new Dictionary<string, int>();
                r.SI(key, d);
                return d;
            }
            if (type.IsArray && type.GetArrayRank() == 1)
            {
                StateReader s = r.Section(key);
                int n = s.Int("n");
                if (n < 0) return null;
                Type e = type.GetElementType();
                var a = current is Array c && c.Length == n ? c : Array.CreateInstance(e, n);
                for (int i = 0; i < n; i++) a.SetValue(Read(s, i.ToString(CultureInfo.InvariantCulture), e, a.GetValue(i), registry), i);
                return a;
            }
            if (type.IsValueType && !type.IsPrimitive)
            {
                StateReader s = r.Section(key);
                object boxed = current ?? Activator.CreateInstance(type);
                foreach (FieldInfo f in Fields(type)) f.SetValue(boxed, Read(s, f.Name, f.FieldType, f.GetValue(boxed), registry));
                return boxed;
            }
            throw new StateException($"snapshot: can't load {key} (a {type.Name})");
        }

        /// <summary>Every instance field of a class object, written as a section (an object read back in place).</summary>
        public static void WriteObject(StateWriter w, string key, object o, EnemyRegistry registry)
        {
            w.Begin(key);
            foreach (FieldInfo f in Fields(o.GetType())) Write(w, f.Name, f.FieldType, f.GetValue(o), registry);
            w.End();
        }

        public static void ReadObject(StateReader r, string key, object o, EnemyRegistry registry)
        {
            StateReader s = r.Section(key);
            foreach (FieldInfo f in Fields(o.GetType()))
            {
                object v = Read(s, f.Name, f.FieldType, f.GetValue(o), registry);
                if (!f.IsInitOnly) f.SetValue(o, v);
            }
        }

        /// <summary>One named field of `owner`: a value written as is; a readonly reference (an array, an object) read in place.</summary>
        public static void WriteField(StateWriter w, object owner, FieldInfo f, EnemyRegistry registry)
        {
            object v = f.GetValue(owner);
            if (f.IsInitOnly && !f.FieldType.IsValueType && !f.FieldType.IsArray && f.FieldType != typeof(Dictionary<string, int>))
            {
                if (v == null) throw new StateException($"snapshot: {f.Name} is null");
                WriteObject(w, f.Name, v, registry);
            }
            else Write(w, f.Name, f.FieldType, v, registry);
        }

        public static void ReadField(StateReader r, object owner, FieldInfo f, EnemyRegistry registry)
        {
            object current = f.GetValue(owner);
            if (f.IsInitOnly && !f.FieldType.IsValueType && !f.FieldType.IsArray && f.FieldType != typeof(Dictionary<string, int>))
            {
                ReadObject(r, f.Name, current, registry);
                return;
            }
            object v = Read(r, f.Name, f.FieldType, current, registry);
            if (f.IsInitOnly)
            {
                if (!ReferenceEquals(v, current)) throw new StateException($"snapshot: {f.Name} (readonly) changed size");
                return; // an array or a dictionary filled in place
            }
            f.SetValue(owner, v);
        }
    }

    /// <summary>
    /// The Mover's run state, from outside (Mover.cs is not touched: CLAUDE.md rule 3). Every instance field is either in
    /// `Fields` (saved; the private ones read and written by reflection) or in `Skipped` (wiring), and RunSnapshotTests
    /// checks that the two lists cover the class, so a new Mover field fails a test until it is placed in one.
    /// </summary>
    public sealed class MoverState : IRunState
    {
        /// <summary>The saved fields: the public state, the collision and step scratch (some of it carries across steps:
        /// lastWallNormal / hasLastWall, the stored input), every timer, the dash / slide / slam / wall-ride state, the
        /// swing; `rope` (a readonly Rope) and `wallHits` (a readonly Vec3[8]) are read and written in place.</summary>
        public static readonly string[] Fields =
        {
            "position", "velocity", "state", "grounded", "crouched", "pips", "regenMult", "walkMult", "bonusPips", "bonusRideTime", "bonusWallJumps",
            "slideSpeedMult", "slideFrictionMult", "groundFrictionMult", "slideJumpCapMult", "slideCharge", "ropeEnabled", "rope", "doubleJumpsLeft", "wallJumpsLeft", "lastApexHeight",
            "lastDashDistance", "invulnerable", "touchingWall", "wallNormal", "groundNormal", "gravityMultiplier",
            "capsule", "probe", "move", "preMoveSpeed", "impactSpeed", "wallHits", "wallHitCount", "probeNormal", "lastWallNormal", "hasLastWall", "nearWallNormal",
            "input", "wish", "wishSpeed", "forward",
            "bufferSteps", "coyoteSteps", "groundedSteps", "windSteps", "boostSteps", "slideBufferSteps", "jumpCutArmed", "jumpAirtime", "airborne", "takeoffY", "airPeakY",
            "dashSteps", "dashDir", "dashSpeedNow", "dashSpeedBefore", "dashStart", "dashFromSlide", "slideDir", "slamStartY",
            "rideSteps", "rideNormal", "rideDir", "rideLockActive", "rideLockNormal", "wallJumpLockSteps", "wallJumpLockNormal",
            "swingState",
            "moveBalance", // NIGHT16 U3-29.4 (builder M): the run's design.move rules and their clocks (the charge, the decay's age, the hop chain)
            "stamina", // NIGHT17 U3-30.4 (builder M): the run's design.stamina rules, the bar and its clocks (the refill's delay, the sprint)
            "flow34", // NIGHT34 M1 (builder M): the run's design.n34.flow34 rules and clocks (the wall coyote, the zip carry; Core/Flow34.cs)
            "wall42", "wall42NoIntent", "wall42Landing", "wall42NoBounce", "wall42Capped", // NIGHT42 M1 wallfling42: the run's rules (Core/Wall42.cs) and its counts
            "m43", // NIGHT43 M1 chimney43 / ride43: the run's rules, the last wall jump's wall, the clock since the last ride, the counts (Core/Move43.cs)
            "climb47", // NIGHT47 W climb47: the run's climb rules, the catch's ease, the release clock, the buffered jump-off, the counts (Core/Climb47.cs)
        };

        /// <summary>Not saved: the config, the collision world and the event (wiring the owner set up).</summary>
        public static readonly string[] Skipped = { "config", "world", "Events", "aimHook", "zipAnchor" /* NIGHT21: the ARCLINE's wiring (SurvivalSim.Arcline21.cs) */, "climbWall", "climbNext" /* NIGHT47 climb47: the sim's climb wall and next-hold tests */ };

        static FieldInfo[] infos;
        readonly Mover mover;

        public MoverState(Mover mover)
        {
            this.mover = mover ?? throw new ArgumentNullException(nameof(mover));
            if (infos == null)
            {
                var list = new FieldInfo[Fields.Length];
                for (int i = 0; i < Fields.Length; i++) list[i] = FieldSnapshot.Field(typeof(Mover), Fields[i]);
                infos = list;
            }
        }

        public void Save(StateWriter w)
        {
            foreach (FieldInfo f in infos) FieldSnapshot.WriteField(w, mover, f, null);
        }

        /// <summary>NIGHT16 (the review): fields a NIGHT15 save (SaveVersion 1) never wrote. Absent, they keep what StartRun and the
        /// composition set (the charge full, the run's design.move flags); every other field is as strict as before.</summary>
        public static readonly string[] OptionalSinceNight16 = { "slideCharge", "moveBalance" };

        /// <summary>NIGHT17 U3-30.4: a field a NIGHT16 save never wrote. Absent, it keeps what StartRun set (the run's design.stamina
        /// flags, the bar full), so an older save resumes.</summary>
        public static readonly string[] OptionalSinceNight17 = { "stamina" };

        /// <summary>NIGHT34 M1: a field a NIGHT33 save never wrote. Absent, it keeps what StartRun set (the run's design.n34.flow34 rules,
        /// the clocks off), so an older save resumes.</summary>
        public static readonly string[] OptionalSinceNight34 = { "flow34" };

        /// <summary>NIGHT47 W climb47: a field a NIGHT46 save never wrote. Absent, it keeps what StartRun set (the run's climb rules, the clocks
        /// off), so an older save resumes.</summary>
        public static readonly string[] OptionalSinceNight47 = { "climb47" };

        public void Load(StateReader r)
        {
            foreach (FieldInfo f in infos)
            {
                if (!r.Has(f.Name) && (Array.IndexOf(OptionalSinceNight16, f.Name) >= 0 || Array.IndexOf(OptionalSinceNight17, f.Name) >= 0 || Array.IndexOf(OptionalSinceNight34, f.Name) >= 0 || Array.IndexOf(OptionalSinceNight47, f.Name) >= 0)) continue;
                FieldSnapshot.ReadField(r, mover, f, null);
            }
        }
    }

    /// <summary>
    /// The aim assist's run state, from outside (AimAssist.cs is agent I's file tonight). Every instance field that isn't
    /// readonly is saved (enumerated by reflection, so a field agent I adds is covered, or refused loudly when its type is
    /// unknown); of the readonly ones, the hysteresis `rival` (in place) and the `releases` counts. Not saved: the config
    /// numbers, the delegates, the scratch lists and the HUD's LockInfo (recomputed). The lock's and the pending
    /// candidate's enemies are registry indices (at a break they are null: the lock ends with its target's death).
    /// </summary>
    public sealed class AimAssistState : IRunState
    {
        static readonly string[] InPlace = { "rival", "releases" };
        static List<FieldInfo> infos;
        static FieldInfo enemiesField;
        readonly AimAssist assist;

        public AimAssistState(AimAssist assist)
        {
            this.assist = assist ?? throw new ArgumentNullException(nameof(assist));
            if (infos == null)
            {
                var list = new List<FieldInfo>();
                foreach (FieldInfo f in FieldSnapshot.Fields(typeof(AimAssist)))
                {
                    if (typeof(Delegate).IsAssignableFrom(f.FieldType)) continue;
                    if (f.IsInitOnly && Array.IndexOf(InPlace, f.Name) < 0) continue;
                    list.Add(f);
                }
                list.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name)); // a stable section order whatever the runtime's field order
                infos = list;
                enemiesField = FieldSnapshot.Field(typeof(AimAssist), "enemies");
            }
        }

        EnemyRegistry Registry => (EnemyRegistry)enemiesField.GetValue(assist);

        public void Save(StateWriter w)
        {
            EnemyRegistry reg = Registry;
            foreach (FieldInfo f in infos) FieldSnapshot.WriteField(w, assist, f, reg);
        }

        public void Load(StateReader r)
        {
            EnemyRegistry reg = Registry;
            foreach (FieldInfo f in infos) FieldSnapshot.ReadField(r, assist, f, reg);
        }
    }

    /// <summary>A section from two delegates (the Game's own run values: the sim step, the run seed, the look, ...).</summary>
    public sealed class CustomState : IRunState
    {
        readonly Action<StateWriter> save;
        readonly Action<StateReader> load;

        public CustomState(Action<StateWriter> save, Action<StateReader> load)
        {
            this.save = save ?? throw new ArgumentNullException(nameof(save));
            this.load = load ?? throw new ArgumentNullException(nameof(load));
        }

        public void Save(StateWriter w) => save(w);

        public void Load(StateReader r) => load(r);
    }
}
