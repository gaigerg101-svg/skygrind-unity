// NIGHT8 U2-05, the first 15 minutes (docs/FRICTION.md #8, #9, #11), three pure web modules:
// - modes/deathRecap.js: what killed you and the damage of your last ui.recap.window s by source (the DOWN screen).
// - ui/hitDirection.js: the screen angle of a hit's source from your look, and a small pool of arcs that age.
// - modes/records.js: personal bests (the results screen's line), one versioned record; the Survival kind only (the
//   other modes aren't ported), its judge() text as the web's. NIGHT11 U3-24.7: a run also keeps an exfil flag (design.exfil,
//   port-only), written only when true and read as false when absent; judging ignores it.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Skygrind.Core
{
    /// <summary>deathRecap.js createDeathRecap: a ring of the player's recent hurts (fixed-step time) and its summary.</summary>
    public sealed class DeathRecap : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the hurts in the window (their times are the Game's sim time, simStep x step:
        // the Game's section restores simStep).
        public void Save(StateWriter w)
        {
            var t = new List<double>(hurts.Count);
            var a = new List<double>(hurts.Count);
            var n = new List<string>(hurts.Count);
            foreach (var h in hurts)
            {
                t.Add(h.t);
                a.Add(h.amount);
                n.Add(h.name);
            }
            w.Doubles("t", t);
            w.Doubles("amount", a);
            w.Strings("name", n);
        }

        public void Load(StateReader r)
        {
            List<double> t = r.Doubles("t"), a = r.Doubles("amount");
            List<string> n = r.Strings("name");
            if (a.Count != t.Count || n.Count != t.Count) throw new StateException("snapshot: recap.hurts");
            hurts.Clear();
            for (int i = 0; i < t.Count; i++) hurts.Add((t[i], a[i], n[i]));
        }

        public struct Part
        {
            public string name;
            public double amount;
        }

        public sealed class Summary
        {
            public string killer; // the last source that hurt you, or null
            public double total;
            public readonly List<Part> parts = new List<Part>(); // per source in the window, largest first
        }

        readonly double windowS;
        readonly List<(double t, double amount, string name)> hurts = new List<(double, double, string)>();

        public DeathRecap(double windowS) => this.windowS = windowS;

        /// <summary>A hurt event's source as deathRecap.js reads it ({type, boss?, attack?}); null fields are the web's undefined.</summary>
        public sealed class HurtSource
        {
            public string type, boss, attack;
        }

        static readonly Dictionary<string, string> NoBossNames = new Dictionary<string, string>();

        /// <summary>
        /// sourceName(source, labels): a readable name for a hurt event's source (labels = main.js recapLabels: names =
        /// config.ui.recap.names, bossNames = config.bossKit.names). A null source is unknown; a field the source lacks reads
        /// as the key "undefined", as a JS property lookup does.
        /// </summary>
        public static string SourceName(HurtSource source, Dictionary<string, string> names, Dictionary<string, string> bossNames)
        {
            if (source == null) return names.TryGetValue("unknown", out string u) ? u : "UNKNOWN";
            if (source.type == "boss") return bossNames.TryGetValue(source.boss ?? "undefined", out string b) ? b : names.TryGetValue("boss", out string nb) ? nb : "BOSS";
            if (source.type == "affix") return names.TryGetValue("affix:" + (source.attack ?? "undefined"), out string a) ? a : names.TryGetValue("affix", out string na) ? na : "AFFIX";
            string key = source.type ?? "undefined";
            return names.TryGetValue(key, out string n) ? n : key.ToUpperInvariant();
        }

        /// <summary>sourceName for a payload that carries only its type (null: unknown).</summary>
        public static string SourceName(string type, Dictionary<string, string> names) =>
            SourceName(type == null ? null : new HurtSource { type = type }, names, NoBossNames);

        /// <summary>A hurt at fixed-step time `t` (s).</summary>
        public void Hurt(double t, double amount, string name)
        {
            hurts.Add((t, amount, name));
            while (hurts.Count > 0 && t - hurts[0].t > windowS) hurts.RemoveAt(0);
        }

        public void Clear() => hurts.Clear();

        /// <summary>The recap at time `t`: the killer, and the damage per source in the window (rounded, largest first).</summary>
        public Summary At(double t)
        {
            var s = new Summary();
            var order = new List<string>();
            var by = new Dictionary<string, double>();
            string last = null;
            foreach (var h in hurts)
            {
                if (t - h.t > windowS + 1e-9) continue;
                if (!by.ContainsKey(h.name)) order.Add(h.name);
                by[h.name] = (by.TryGetValue(h.name, out double v) ? v : 0) + h.amount;
                last = h.name;
            }
            foreach (string name in order) s.parts.Add(new Part { name = name, amount = JsMath.Round(by[name]) });
            // JS's sort is stable: equal amounts keep their first-hurt order.
            for (int i = 1; i < s.parts.Count; i++)
            {
                Part x = s.parts[i];
                int j = i - 1;
                while (j >= 0 && s.parts[j].amount < x.amount)
                {
                    s.parts[j + 1] = s.parts[j];
                    j--;
                }
                s.parts[j + 1] = x;
            }
            foreach (Part p in s.parts) s.total += p.amount;
            s.killer = last;
            return s;
        }
    }

    /// <summary>hitDirection.js: the damage-direction arcs.</summary>
    public sealed class HitArcs
    {
        public sealed class Arc
        {
            public double x, z;
            public double age = double.PositiveInfinity; // s; Infinity = free
        }

        public readonly Arc[] arcs;

        public HitArcs(int max)
        {
            arcs = new Arc[max];
            for (int i = 0; i < max; i++) arcs[i] = new Arc();
        }

        /// <summary>
        /// hitAngle: the screen angle (degrees clockwise from straight up) of a world point seen from (px, pz) looking along
        /// yaw (yaw 0 looks toward -z). 0 = ahead, 90 = right, 180 = behind, -90 = left.
        /// </summary>
        public static double HitAngle(double yaw, double px, double pz, double sx, double sz)
        {
            double dx = sx - px;
            double dz = sz - pz;
            double ahead = -JsMath.Sin(yaw) * dx - JsMath.Cos(yaw) * dz;
            double right = JsMath.Cos(yaw) * dx - JsMath.Sin(yaw) * dz;
            return JsMath.Atan2(right, ahead) * 180 / Math.PI;
        }

        /// <summary>A hit from (x, z): the same side again refreshes its arc, else the oldest one is reused.</summary>
        public void Add(double x, double z, double sameWithin = 1.5)
        {
            Arc slot = null;
            foreach (Arc a in arcs)
            {
                if (a.age != double.PositiveInfinity && JsMath.Hypot(a.x - x, a.z - z) < sameWithin)
                {
                    slot = a;
                    break;
                }
            }
            if (slot == null)
            {
                slot = arcs[0];
                foreach (Arc a in arcs) if (a.age > slot.age) slot = a;
            }
            slot.x = x;
            slot.z = z;
            slot.age = 0;
        }

        /// <summary>Age every arc by `dt` s; past `life` an arc is free.</summary>
        public void Step(double dt, double life)
        {
            foreach (Arc a in arcs) if (a.age != double.PositiveInfinity) a.age = a.age + dt > life ? double.PositiveInfinity : a.age + dt;
        }

        public void Clear()
        {
            foreach (Arc a in arcs) a.age = double.PositiveInfinity;
        }

        /// <summary>main.js updateHitArcs: full until fadeFrom, then fading to 0 at life.</summary>
        public static double Alpha(double age, double fadeFrom, double life) => age < fadeFrom ? 1 : Math.Max(0, 1 - (age - fadeFrom) / (life - fadeFrom));
    }

    /// <summary>records.js for Survival: the best run (round first, then points), its results line, a versioned store.</summary>
    public sealed class Records
    {
        public const string StorageKey = "skygrind.records.v1"; // the web's localStorage key
        public const int Version = 1;

        public sealed class Run
        {
            public double round, points, date;
            /// <summary>NIGHT11 U3-24.7 (design.exfil, port-only): the run ended in a successful exfil. Not judged; a record without it reads false.</summary>
            public bool exfil;
        }

        public struct Judgement
        {
            public bool newBest, first;
            public string text;
        }

        public readonly Dictionary<string, Run> modes = new Dictionary<string, Run>();

        static bool Better(Run a, Run b) => a.round > b.round || (a.round == b.round && a.points > b.points);

        static string N(double v) => v.ToString("R", CultureInfo.InvariantCulture);

        /// <summary>judge('survival', run, best): newBest, first and the results screen's line.</summary>
        public static Judgement Judge(Run run, Run best)
        {
            bool first = best == null;
            bool newBest = first || Better(run, best);
            string text;
            if (first) text = "FIRST RUN ON RECORD";
            else if (newBest) text = run.round > best.round ? $"NEW BEST · +{N(run.round - best.round)} ROUND{(run.round - best.round == 1 ? "" : "S")}" : $"NEW BEST · ROUND {N(run.round)}, +{N(run.points - best.points)} POINTS";
            else text = run.round == best.round ? $"BEST ROUND {N(best.round)} · {N(best.points - run.points)} POINTS SHORT" : $"BEST ROUND {N(best.round)} · {N(best.round - run.round)} SHORT";
            return new Judgement { newBest = newBest, first = first, text = text };
        }

        /// <summary>readRecords: a stored blob -> its records, or none when it is missing, corrupt or another version.</summary>
        public static Records Read(string raw)
        {
            try
            {
                if (string.IsNullOrEmpty(raw) || !(JsJson.Parse(raw) is Dictionary<string, object> data)) return new Records();
                return FromObj(data);
            }
            catch (Exception)
            {
                return new Records(); // corrupt: nothing kept
            }
        }

        /// <summary>NIGHT12: a parsed records blob ({v, modes}) -> its records (the JSON profile embeds one); none when another version.</summary>
        public static Records FromObj(Dictionary<string, object> data)
        {
            var r = new Records();
            try
            {
                if (data == null) return r;
                if (!(data.TryGetValue("v", out object v) && v is double dv && dv == Version)) return r;
                if (!(data.TryGetValue("modes", out object m) && m is Dictionary<string, object> modes)) return r;
                foreach (var kv in modes)
                {
                    if (!(kv.Value is Dictionary<string, object> o)) continue;
                    var run = new Run();
                    if (o.TryGetValue("round", out object ro) && ro is double rd) run.round = rd;
                    if (o.TryGetValue("points", out object po) && po is double pd) run.points = pd;
                    if (o.TryGetValue("date", out object da) && da is double dd) run.date = dd;
                    run.exfil = o.TryGetValue("exfil", out object ex) && ex is bool eb && eb; // NIGHT11: absent (older records) reads false
                    r.modes[kv.Key] = run;
                }
            }
            catch (Exception)
            {
                r.modes.Clear(); // corrupt: nothing kept
            }
            return r;
        }

        public Run Best(string key) => modes.TryGetValue(key, out Run r) ? r : null;

        /// <summary>Judge a finished run against the best and keep it when it is better (the caller saves).</summary>
        public Judgement Submit(string key, Run run)
        {
            Judgement j = Judge(run, Best(key));
            if (j.newBest) modes[key] = run;
            return j;
        }

        public string ToJson()
        {
            var sb = new System.Text.StringBuilder("{\"v\":").Append(Version).Append(",\"modes\":{");
            bool first = true;
            foreach (var kv in modes)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append('"').Append(kv.Key).Append("\":{\"round\":").Append(N(kv.Value.round)).Append(",\"points\":").Append(N(kv.Value.points)).Append(",\"date\":").Append(N(kv.Value.date));
                if (kv.Value.exfil) sb.Append(",\"exfil\":true"); // NIGHT11: only when set, so a record without an exfil is the web's bytes
                sb.Append('}');
            }
            return sb.Append("}}").ToString();
        }
    }
}
