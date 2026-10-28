// NIGHT12 U3-25.7 (NIGHT12 rule 6): the meta's one JSON profile, Application.persistentDataPath/skygrind-profile.json.
// Versioned: a missing, corrupt or other-version file starts empty. It holds what carries across runs:
// - the records (records.js; migrated once from PlayerPrefs' skygrind.records.v1)
// - the unlock flags (unlocks.js)
// - contract progress (challenges.js's store {v, done} per id, and the port-only contracts)
// - MASTERY counters per gun, the finish chosen per gun
// - the best grade per HEAT rung, the highest HEAT rung open
// - the tips' counts (tips.js), the first-run flag
// - the mutators' best round and picks (mutators.js)
// Settings stay in PlayerPrefs (N16). A LAB run and the smoke's runs (noRecords) never change it (the Game decides).
// Pure (System.IO only): the Game passes the path. Keys are written sorted (ordinal), so the same profile is the same
// bytes; display order always comes from config, never from here.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Skygrind.Core
{
    public sealed class Profile
    {
        public const string FileName = "skygrind-profile.json";
        public const int Version = 1;

        /// <summary>challenges.js store[id]: the value (it only rises) and whether the target was reached.</summary>
        public sealed class Counter
        {
            public double v;
            public bool done;
        }

        public Records records = new Records();
        public bool recordsMigrated;
        public readonly Dictionary<string, bool> flags = new Dictionary<string, bool>();
        public readonly Dictionary<string, Counter> contracts = new Dictionary<string, Counter>();
        public readonly Dictionary<string, Dictionary<string, double>> mastery = new Dictionary<string, Dictionary<string, double>>();
        public readonly Dictionary<string, string> finishes = new Dictionary<string, string>();
        public readonly Dictionary<string, string> bestGrade = new Dictionary<string, string>(); // HEAT rung ("0" = none) -> best grade
        public int heat; // the highest HEAT rung open to pick (0: none open yet)
        public readonly Dictionary<string, double> tips = new Dictionary<string, double>();
        public bool onboarded; // the first-run script has run (or was skipped)
        public double mutatorBest; // mutators.js state.best
        public readonly List<string> mutatorPicks = new List<string>();
        public int runs; // finished runs stored (not LAB, not noRecords)
        /// <summary>NIGHT26 (builder L): the best per CAMPAIGN level id (the level select's card). Written only when a level was cleared: a profile that never played one is the same bytes as before, and an older profile loads with none.</summary>
        public readonly Dictionary<string, LevelBest26> levelBest = new Dictionary<string, LevelBest26>();

        /// <summary>NIGHT33 P (design.n33.shop33): the campaign's WALLET, points carried across levels for the SHOP (Core/Shop33.cs), and the shop's
        /// owned / equipped upgrades (Shop33Catalogue's encoding). Written only when not 0: a profile that never banked is the same bytes as before.</summary>
        public int bank33, shop33;

        /// <summary>NIGHT33 P: the most points each campaign level has banked (its best Economy.earned); a clear banks only what beats it, so a replayed level
        /// never earns twice into the wallet (Shop33.Bank). Written only when not empty.</summary>
        public readonly Dictionary<string, int> banked33 = new Dictionary<string, int>();

        /// <summary>NIGHT35 T (design.n35.teach35, D-7): the teaching lines this profile has been shown (each once per profile, as ULTRAKILL's HudMessage
        /// playerPref). Written only when not empty: a profile that never saw one is the same bytes as before.</summary>
        public readonly HashSet<string> teach35 = new HashSet<string>();

        public bool Teach35Seen(string id) => id != null && teach35.Contains(id);

        /// <summary>Mark a teaching line shown; true the first time (the caller saves).</summary>
        public bool MarkTeach35(string id) => !string.IsNullOrEmpty(id) && teach35.Add(id);

        public LevelBest26 LevelBest(string levelId) => levelId != null && levelBest.TryGetValue(levelId, out LevelBest26 b) ? b : null;

        /// <summary>Keep a cleared level's rank (the card's best of each column); the caller saves.</summary>
        public bool SubmitLevel(string levelId, LevelRank26 r)
        {
            if (string.IsNullOrEmpty(levelId) || r == null) return false;
            if (!levelBest.TryGetValue(levelId, out LevelBest26 b)) levelBest[levelId] = b = new LevelBest26();
            return b.Submit(r);
        }

        // --- flags (unlocks.js) ---------------------------------------------------------------------------------------

        public bool Flag(string f) => f != null && flags.TryGetValue(f, out bool b) && b;

        /// <summary>unlocks.js set: earn a flag; true the first time (a new unlock to announce).</summary>
        public bool SetFlag(string f)
        {
            if (Flag(f)) return false;
            flags[f] = true;
            return true;
        }

        // --- contracts, mastery, grades ---------------------------------------------------------------------------------

        /// <summary>A contract's record, made on first use.</summary>
        public Counter Contract(string id)
        {
            if (!contracts.TryGetValue(id, out Counter c)) contracts[id] = c = new Counter();
            return c;
        }

        public double Mastery(string weapon, string counter) => mastery.TryGetValue(weapon, out var m) && m.TryGetValue(counter, out double v) ? v : 0;

        public void SetMastery(string weapon, string counter, double v)
        {
            if (!mastery.TryGetValue(weapon, out var m)) mastery[weapon] = m = new Dictionary<string, double>();
            m[counter] = v;
        }

        public string FinishOf(string weapon) => finishes.TryGetValue(weapon, out string f) ? f : null;

        /// <summary>A grade's order for "best": D 0 .. S 4, P 5 (design.grade.p), anything else -1.</summary>
        public static int GradeRank(string g) => g switch { "D" => 0, "C" => 1, "B" => 2, "A" => 3, "S" => 4, "P" => 5, _ => -1 };

        public string BestGrade(int rung) => bestGrade.TryGetValue(rung.ToString(CultureInfo.InvariantCulture), out string g) ? g : null;

        /// <summary>Keep a run's grade as the rung's best when it beats it; true when it did.</summary>
        public bool SubmitGrade(int rung, string grade)
        {
            if (GradeRank(grade) < 0) return false;
            string key = rung.ToString(CultureInfo.InvariantCulture);
            if (bestGrade.TryGetValue(key, out string old) && GradeRank(old) >= GradeRank(grade)) return false;
            bestGrade[key] = grade;
            return true;
        }

        public double TipCount(string id) => tips.TryGetValue(id, out double n) ? n : 0;

        /// <summary>
        /// Once per profile: the PlayerPrefs records (records.js's skygrind.records.v1 blob) become the profile's, unless the
        /// profile already holds a record. True when it migrated (the caller saves).
        /// </summary>
        public bool MigrateRecords(string playerPrefsRaw)
        {
            if (recordsMigrated) return false;
            recordsMigrated = true;
            if (records.modes.Count == 0) records = Records.Read(playerPrefsRaw);
            return true;
        }

        /// <summary>
        /// Become a copy of `other`, in place (the systems holding this profile keep their reference): the smoke records into
        /// the live profile without saving, then restores it from its snapshot.
        /// </summary>
        public void CopyFrom(Profile other)
        {
            Profile o = Read(other.ToJson());
            records = o.records;
            recordsMigrated = o.recordsMigrated;
            flags.Clear();
            foreach (var kv in o.flags) flags[kv.Key] = kv.Value;
            contracts.Clear();
            foreach (var kv in o.contracts) contracts[kv.Key] = kv.Value;
            mastery.Clear();
            foreach (var kv in o.mastery) mastery[kv.Key] = kv.Value;
            finishes.Clear();
            foreach (var kv in o.finishes) finishes[kv.Key] = kv.Value;
            bestGrade.Clear();
            foreach (var kv in o.bestGrade) bestGrade[kv.Key] = kv.Value;
            heat = o.heat;
            tips.Clear();
            foreach (var kv in o.tips) tips[kv.Key] = kv.Value;
            onboarded = o.onboarded;
            mutatorBest = o.mutatorBest;
            mutatorPicks.Clear();
            mutatorPicks.AddRange(o.mutatorPicks);
            runs = o.runs;
            levelBest.Clear();
            foreach (var kv in o.levelBest) levelBest[kv.Key] = kv.Value;
            bank33 = o.bank33;
            shop33 = o.shop33;
            banked33.Clear();
            foreach (var kv in o.banked33) banked33[kv.Key] = kv.Value;
            teach35.Clear();
            teach35.UnionWith(o.teach35);
        }

        // --- reading ----------------------------------------------------------------------------------------------------

        /// <summary>A profile's JSON -> the profile; missing, corrupt or another version -> an empty profile.</summary>
        public static Profile Read(string json)
        {
            var p = new Profile();
            try
            {
                if (string.IsNullOrEmpty(json) || !(JsJson.Parse(json) is Dictionary<string, object> o)) return p;
                if (!(o.TryGetValue("v", out object v) && v is double dv && dv == Version)) return p;
                if (o.TryGetValue("records", out object r) && r is Dictionary<string, object> ro) p.records = Records.FromObj(ro);
                p.recordsMigrated = B(o, "recordsMigrated");
                if (o.TryGetValue("flags", out object fl) && fl is Dictionary<string, object> fo)
                    foreach (var kv in fo) if (kv.Value is bool b && b) p.flags[kv.Key] = true;
                if (o.TryGetValue("contracts", out object co) && co is Dictionary<string, object> cd)
                    foreach (var kv in cd)
                        if (kv.Value is Dictionary<string, object> c) p.contracts[kv.Key] = new Counter { v = N(c, "v"), done = B(c, "done") };
                if (o.TryGetValue("mastery", out object mo) && mo is Dictionary<string, object> md)
                    foreach (var kv in md)
                        if (kv.Value is Dictionary<string, object> m)
                        {
                            var counters = new Dictionary<string, double>();
                            foreach (var c in m) if (c.Value is double d) counters[c.Key] = d;
                            p.mastery[kv.Key] = counters;
                        }
                if (o.TryGetValue("finishes", out object fi) && fi is Dictionary<string, object> fd)
                    foreach (var kv in fd) if (kv.Value is string s) p.finishes[kv.Key] = s;
                if (o.TryGetValue("bestGrade", out object bg) && bg is Dictionary<string, object> bd)
                    foreach (var kv in bd) if (kv.Value is string s) p.bestGrade[kv.Key] = s;
                p.heat = (int)N(o, "heat");
                // NIGHT24: a NIGHT23 profile's "daily" record is ignored (the DAILY is gone) and not written back
                if (o.TryGetValue("tips", out object ti) && ti is Dictionary<string, object> td)
                    foreach (var kv in td) if (kv.Value is double d) p.tips[kv.Key] = d;
                p.onboarded = B(o, "onboarded");
                p.mutatorBest = N(o, "mutatorBest");
                if (o.TryGetValue("mutatorPicks", out object mp) && mp is List<object> ml)
                    foreach (object x in ml) if (x is string s && !p.mutatorPicks.Contains(s)) p.mutatorPicks.Add(s);
                p.runs = (int)N(o, "runs");
                if (o.TryGetValue("levelBest", out object lb) && lb is Dictionary<string, object> ld)
                    foreach (var kv in ld) if (kv.Value is Dictionary<string, object> lo) p.levelBest[kv.Key] = LevelBest26.FromObj(lo);
                p.bank33 = Math.Max(0, (int)N(o, "bank33")); // NIGHT33 P: absent in older profiles (0)
                p.shop33 = (int)N(o, "shop33");
                if (o.TryGetValue("banked33", out object bk) && bk is Dictionary<string, object> bd33)
                    foreach (var kv in bd33) if (kv.Value is double d && d > 0) p.banked33[kv.Key] = (int)d;
                if (o.TryGetValue("teach35", out object t35) && t35 is List<object> tl) // NIGHT35 T: absent in older profiles
                    foreach (object x in tl) if (x is string s && s.Length > 0) p.teach35.Add(s);
            }
            catch (Exception)
            {
                return new Profile(); // corrupt: start empty
            }
            return p;
        }

        static double N(Dictionary<string, object> o, string k) => o.TryGetValue(k, out object v) && v is double d && !double.IsNaN(d) ? d : 0;
        static bool B(Dictionary<string, object> o, string k) => o.TryGetValue(k, out object v) && v is bool b && b;
        static string S(Dictionary<string, object> o, string k) => o.TryGetValue(k, out object v) ? v as string : null;

        /// <summary>The file at `path` -> the profile; a missing or unreadable file starts empty.</summary>
        public static Profile Load(string path)
        {
            try
            {
                return File.Exists(path) ? Read(File.ReadAllText(path)) : new Profile();
            }
            catch (Exception)
            {
                return new Profile();
            }
        }

        /// <summary>Write the profile to `path` through a temporary file (a crash mid-write leaves the old file). False on failure.</summary>
        public bool Save(string path)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, ToJson());
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- writing ----------------------------------------------------------------------------------------------------

        static string Num(double v) => v.ToString("R", CultureInfo.InvariantCulture);

        static void Str(StringBuilder sb, string s)
        {
            if (s == null)
            {
                sb.Append("null");
                return;
            }
            sb.Append('"');
            foreach (char ch in s)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (ch < 0x20) sb.Append("\\u").Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(ch);
                        break;
                }
            }
            sb.Append('"');
        }

        static List<string> Sorted<T>(Dictionary<string, T> d)
        {
            var keys = new List<string>(d.Keys);
            keys.Sort(string.CompareOrdinal);
            return keys;
        }

        public string ToJson()
        {
            var sb = new StringBuilder(1024);
            sb.Append("{\"v\":").Append(Version);
            sb.Append(",\"records\":").Append(records.ToJson());
            sb.Append(",\"recordsMigrated\":").Append(recordsMigrated ? "true" : "false");
            sb.Append(",\"flags\":{");
            bool first = true;
            foreach (string k in Sorted(flags))
            {
                if (!flags[k]) continue;
                if (!first) sb.Append(',');
                first = false;
                Str(sb, k);
                sb.Append(":true");
            }
            sb.Append("},\"contracts\":{");
            first = true;
            foreach (string k in Sorted(contracts))
            {
                if (!first) sb.Append(',');
                first = false;
                Str(sb, k);
                sb.Append(":{\"v\":").Append(Num(contracts[k].v)).Append(",\"done\":").Append(contracts[k].done ? "true" : "false").Append('}');
            }
            sb.Append("},\"mastery\":{");
            first = true;
            foreach (string k in Sorted(mastery))
            {
                if (!first) sb.Append(',');
                first = false;
                Str(sb, k);
                sb.Append(":{");
                bool f2 = true;
                foreach (string c in Sorted(mastery[k]))
                {
                    if (!f2) sb.Append(',');
                    f2 = false;
                    Str(sb, c);
                    sb.Append(':').Append(Num(mastery[k][c]));
                }
                sb.Append('}');
            }
            sb.Append("},\"finishes\":{");
            first = true;
            foreach (string k in Sorted(finishes))
            {
                if (!first) sb.Append(',');
                first = false;
                Str(sb, k);
                sb.Append(':');
                Str(sb, finishes[k]);
            }
            sb.Append("},\"bestGrade\":{");
            first = true;
            foreach (string k in Sorted(bestGrade))
            {
                if (!first) sb.Append(',');
                first = false;
                Str(sb, k);
                sb.Append(':');
                Str(sb, bestGrade[k]);
            }
            sb.Append("},\"heat\":").Append(heat);
            sb.Append(",\"tips\":{");
            first = true;
            foreach (string k in Sorted(tips))
            {
                if (!first) sb.Append(',');
                first = false;
                Str(sb, k);
                sb.Append(':').Append(Num(tips[k]));
            }
            sb.Append("},\"onboarded\":").Append(onboarded ? "true" : "false");
            sb.Append(",\"mutatorBest\":").Append(Num(mutatorBest));
            sb.Append(",\"mutatorPicks\":[");
            for (int i = 0; i < mutatorPicks.Count; i++)
            {
                if (i > 0) sb.Append(',');
                Str(sb, mutatorPicks[i]);
            }
            sb.Append("],\"runs\":").Append(runs);
            if (levelBest.Count > 0) // NIGHT26: only a profile that cleared a campaign level carries the card
            {
                sb.Append(",\"levelBest\":{");
                first = true;
                foreach (string k in Sorted(levelBest))
                {
                    if (!first) sb.Append(',');
                    first = false;
                    Str(sb, k);
                    sb.Append(':').Append(levelBest[k].ToJson());
                }
                sb.Append('}');
            }
            if (bank33 != 0) sb.Append(",\"bank33\":").Append(bank33.ToString(CultureInfo.InvariantCulture)); // NIGHT33 P: only once something was banked
            if (shop33 != 0) sb.Append(",\"shop33\":").Append(shop33.ToString(CultureInfo.InvariantCulture));
            if (banked33.Count > 0)
            {
                sb.Append(",\"banked33\":{");
                first = true;
                foreach (string k in Sorted(banked33))
                {
                    if (!first) sb.Append(',');
                    first = false;
                    Str(sb, k);
                    sb.Append(':').Append(banked33[k].ToString(CultureInfo.InvariantCulture));
                }
                sb.Append('}');
            }
            if (teach35.Count > 0) // NIGHT35 T: only once a teaching line was shown (sorted: the same set is the same bytes)
            {
                var ids = new List<string>(teach35);
                ids.Sort(string.CompareOrdinal);
                sb.Append(",\"teach35\":[");
                for (int i = 0; i < ids.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    Str(sb, ids[i]);
                }
                sb.Append(']');
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
