// NIGHT9 U3-07: the styleCore session (PortExport/vectors/styleCore.json, port-export vectors.mjs styleCore()):
// createStyleCore(config.style, hooks), then the recorded operations in order through StyleCore, comparing each op's
// result, the hooks it called (onAward(name, points, quiet) / onRank(rank, up)) and the core's state after it (meter,
// rank, total, time, fill, stats) bit for bit. An award's `quiet` is recorded as undefined when the award list leaves
// it out; styleCore only tests it for truth (`if (!quiet)`), so undefined compares as false.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddStyleCore(Dictionary<string, GoldenCheck> d)
        {
            d["styleCore"] = CheckStyleCore;
            d["styleSim"] = CheckStyleSim;
        }

        /// <summary>A StyleSimResult as styleSim.js returns it (the recorded value's shape).</summary>
        static Dictionary<string, object> SimJson(StyleSimResult r)
        {
            var timeline = new List<object>();
            foreach (var (second, rank) in r.timeline) timeline.Add(new List<object> { second, (double)rank });
            var breakdown = new Dictionary<string, object>();
            foreach (var kv in r.breakdown) breakdown[kv.Key] = kv.Value;
            var reached = new List<object>();
            foreach (double v in r.reached) reached.Add(v);
            return new Dictionary<string, object>
            {
                ["profile"] = r.profile, ["weapon"] = r.weapon, ["maxRank"] = (double)r.maxRank, ["finalRank"] = (double)r.finalRank,
                ["reached"] = reached, ["timeline"] = timeline, ["total"] = r.total, ["awards"] = (double)r.awards, ["capped"] = (double)r.capped,
                ["dimmed"] = (double)r.dimmed, ["breakdown"] = breakdown,
            };
        }

        static StyleSimOptions SimOptions(Dictionary<string, object> o) => new StyleSimOptions
        {
            profile = Field(o, "profile") as string, weapon = Field(o, "weapon") as string, target = Field(o, "target") as string, parry = Truthy(Field(o, "parry")),
            seconds = Field(o, "seconds") != null ? PortData.Num(o["seconds"]) : (double?)null, seed = Field(o, "seed") != null ? PortData.Num(o["seed"]) : (double?)null,
            startRank = Field(o, "startRank") != null ? PortData.Num(o["startRank"]) : (double?)null, snipeEvery = Field(o, "snipeEvery") != null ? PortData.Num(o["snipeEvery"]) : (double?)null,
        };

        /// <summary>styleSim.json: paceReport(config) and simulateStyle(config, opts) results, whole.</summary>
        static void CheckStyleSim(GoldenData data, GoldenResult r)
        {
            var cases = data.Cases("styleSim");
            for (int i = 0; i < cases.Count; i++)
            {
                var c = cases[i];
                string fn = Field(c, "fn") as string;
                var a = PortData.Arr(c["args"]);
                r.where = $"{fn} #{i}";
                if (!(a[0] is Dictionary<string, object> cfgRef) || !cfgRef.TryGetValue("$config", out object p) || (string)p != "")
                {
                    r.Fail($"unsupported config argument {Describe(a[0])}");
                    continue;
                }
                if (fn == "paceReport")
                {
                    var rows = new List<object>();
                    foreach (var (name, result) in StyleSim.PaceReport(data.Config))
                    {
                        var row = new Dictionary<string, object> { ["name"] = name };
                        foreach (var kv in SimJson(result)) row[kv.Key] = kv.Value;
                        rows.Add(row);
                    }
                    r.SameJson(c["result"], rows, "result");
                }
                else if (fn == "simulateStyle") r.SameJson(c["result"], SimJson(StyleSim.SimulateStyle(data.Config, SimOptions(PortData.Obj(a[1])))), "result");
                else
                {
                    r.Fail($"no C# port mapped for {fn}");
                    continue;
                }
                r.cases++;
            }
            r.where = "";
        }

        static object Field(Dictionary<string, object> o, string key) => o.TryGetValue(key, out object v) ? v : null;

        static double? FreshArg(Dictionary<string, object> o) => Field(o, "fresh") is object f && !(f is Dictionary<string, object> u && u.ContainsKey("$undefined")) ? PortData.Num(f) : (double?)null;

        static bool Truthy(object o) => o is bool b ? b : o is double n ? n != 0 && !double.IsNaN(n) : o is string s ? s.Length > 0 : o != null && !(o is Dictionary<string, object> u && u.ContainsKey("$undefined"));

        static double NumOr(Dictionary<string, object> o, string key, double fallback) => Field(o, key) is object v && !(v is Dictionary<string, object> u && u.ContainsKey("$undefined")) ? PortData.Num(v) : fallback;

        static void CheckStyleCore(GoldenData data, GoldenResult r)
        {
            var events = new List<object>();
            var core = new StyleCore(StyleMath.From(data.Config),
                (name, points, quiet) => events.Add(new List<object> { "award", name, points, quiet }),
                (rank, up) => events.Add(new List<object> { "rank", (double)rank, up }));
            double dt = PortData.Num(data.Vectors("styleCore")["dt"]);
            foreach (var rec in data.Cases("styleCore"))
            {
                if (!rec.TryGetValue("session", out object sessionObj))
                {
                    r.Fail($"unknown record {Describe(rec)}");
                    continue;
                }
                var session = PortData.Arr(sessionObj);
                for (int i = 0; i < session.Count; i++)
                {
                    var o = PortData.Obj(session[i]);
                    string op = (string)o["op"];
                    var a = PortData.Arr(o["args"]);
                    r.where = $"session op {i} {op}";
                    events.Clear();
                    object result = null;
                    switch (op)
                    {
                        case "start": core.Start(); break;
                        case "hit":
                        {
                            var h = PortData.Obj(a[0]);
                            result = core.Hit((string)h["weapon"], (string)h["state"], Truthy(Field(h, "headshot")), NumOr(h, "pointScale", 1), Truthy(Field(h, "melee")), NumOr(h, "shot", 0), NumOr(h, "assist", 1), FreshArg(h));
                            break;
                        }
                        case "kill":
                        {
                            var k = PortData.Obj(a[0]);
                            result = core.Kill((string)k["kind"], (string)k["weapon"], (string)k["state"], PortData.Num(k["distance"]), Truthy(Field(k, "melee")), Truthy(Field(k, "bounced")), NumOr(k, "assist", 1), FreshArg(k));
                            break;
                        }
                        case "parry": result = core.Parry((string)PortData.Obj(a[0])["state"], FreshArg(PortData.Obj(a[0]))); break;
                        case "trick": result = core.Trick((string)a[0], (string)a[1], (bool)a[2], (string)a[3]); break;
                        case "cross":
                        {
                            var c = PortData.Obj(a[0]);
                            result = core.Cross(Field(c, "combo") as string, Field(c, "weapon") as string, (string)c["state"]);
                            break;
                        }
                        case "swapKill":
                        {
                            var c = PortData.Obj(a[0]);
                            result = core.SwapKill(Field(c, "weapon") as string, (string)c["state"]);
                            break;
                        }
                        case "bonus":
                        {
                            var c = a.Count > 2 ? PortData.Obj(a[2]) : new Dictionary<string, object>();
                            result = core.Bonus((string)a[0], PortData.Num(a[1]), Field(c, "weapon") as string, Field(c, "state") as string ?? "GROUND", NumOr(c, "assist", 1));
                            break;
                        }
                        case "hurt": core.Hurt(); break;
                        case "setDrainMult": core.SetDrainMult(PortData.Num(a[0])); break;
                        case "setPerkMods":
                        {
                            var v = a[0] as Dictionary<string, object>;
                            core.SetPerkMods(v != null && Field(v, "drain") != null ? PortData.Num(v["drain"]) : (double?)null, v != null && Field(v, "gain") != null ? PortData.Num(v["gain"]) : (double?)null);
                            break;
                        }
                        case "use": core.Use((string)a[0], a.Count > 1 ? PortData.Num(a[1]) : 1); break;
                        case "useKey": core.UseKey((string)a[0]); break;
                        case "freshTiers":
                        {
                            var tiers = core.FreshTiers(new Dictionary<string, string>());
                            var asObj = new Dictionary<string, object>();
                            foreach (var kv in tiers) asObj[kv.Key] = kv.Value;
                            result = asObj;
                            break;
                        }
                        case "debugSetRank": core.DebugSetRank((int)PortData.Num(a[0])); break;
                        case "debugSetMeter": core.DebugSetMeter(PortData.Num(a[0])); break;
                        case "summary":
                        {
                            var breakdown = new Dictionary<string, object>();
                            foreach (var kv in core.breakdown) breakdown[kv.Key] = kv.Value;
                            result = new Dictionary<string, object>
                            {
                                ["total"] = core.total, ["bestRank"] = (double)core.bestRank,
                                ["best"] = new Dictionary<string, object> { ["name"] = core.bestName, ["points"] = core.bestPoints },
                                ["breakdown"] = breakdown,
                            };
                            break;
                        }
                        case "step":
                        {
                            int n = o.TryGetValue("repeat", out object rep) ? (int)PortData.Num(rep) : 1;
                            double stepDt = PortData.Num(a[0]);
                            if (!GoldenResult.Bits(stepDt, dt)) r.Fail($"step dt {GoldenResult.Fmt(stepDt)} vs the file's dt {GoldenResult.Fmt(dt)}");
                            for (int k = 0; k < n; k++) core.Step(stepDt);
                            break;
                        }
                        default:
                            r.Fail($"no C# op for {op}");
                            continue;
                    }
                    if (op != "step") r.SameJson(o["result"], result, "result");
                    // The hooks it called: an award's undefined `quiet` is false (see the header).
                    var want = PortData.Arr(o["events"]);
                    if (want.Count != events.Count) r.Fail($"events: expected {want.Count} [{Describe(want)}] got {events.Count} [{Describe(events)}]");
                    for (int e = 0; e < Math.Min(want.Count, events.Count); e++)
                    {
                        var we = new List<object>(PortData.Arr(want[e]));
                        if (we.Count == 4 && we[3] is Dictionary<string, object> u && u.ContainsKey("$undefined")) we[3] = false;
                        r.SameJson(we, events[e], $"events[{e}]");
                    }
                    var after = PortData.Obj(o["after"]);
                    r.Same(after["meter"], core.meter, "after.meter");
                    r.Same(after["rank"], core.Rank, "after.rank");
                    r.Same(after["total"], core.total, "after.total");
                    r.Same(after["time"], core.time, "after.time");
                    r.Same(after["fill"], core.Fill(), "after.fill");
                    var st = PortData.Obj(after["stats"]);
                    r.Same(st["awards"], core.statAwards, "after.stats.awards");
                    r.Same(st["capped"], core.statCapped, "after.stats.capped");
                    r.Same(st["dimmed"], core.statDimmed, "after.stats.dimmed");
                    r.Same(st["sameShot"], core.statSameShot, "after.stats.sameShot");
                    r.cases++;
                }
            }
            r.where = "";
        }
    }
}
