// NIGHT9 U3-07: the checkers of the AI vector sets (port-export-p18 vectors.mjs): tokens, slots, aim, crowd, watchdog
// (plain calls: the state is rebuilt from args, the call runs on the C# port, then the result and the state the web left
// in argsAfter are compared) and flowField (two sessions: synthetic grids and the Survival map's nav grid). See
// GoldenNewSets.cs for the record shapes and the helpers. Map-order data (token holders, slot owners) compare in order.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    static partial class GoldenNewSets
    {
        // --- tokens.js (TokenPools, HuskBrain.cs) ----------------------------------------------------------------------

        static Dictionary<string, int> SizesOf(Dictionary<string, object> o)
        {
            var s = new Dictionary<string, int>();
            foreach (var kv in o) s[kv.Key] = Int(kv.Value);
            return s;
        }

        static Dictionary<string, object> SizesJson(Dictionary<string, int> s)
        {
            var o = new Dictionary<string, object>();
            foreach (var kv in s) o[kv.Key] = (double)kv.Value;
            return o;
        }

        /// <summary>{sizes, holders: {type: Map id -> {score, committed}}} -> TokenPools (the holders' key order is the pools' order).</summary>
        static TokenPools PoolsOf(Dictionary<string, object> o)
        {
            var sizes = SizesOf(PortData.Obj(o["sizes"]));
            var holders = PortData.Obj(o["holders"]);
            var order = new Dictionary<string, int>();
            foreach (var kv in holders) order[kv.Key] = sizes.TryGetValue(kv.Key, out int n) ? n : 0;
            var pools = new TokenPools(order);
            pools.sizes.Clear();
            foreach (var kv in sizes) pools.sizes[kv.Key] = kv.Value;
            foreach (var kv in holders)
            {
                foreach (object e in PortData.Arr(PortData.Obj(kv.Value)["$map"]))
                {
                    var pair = PortData.Arr(e);
                    var rec = PortData.Obj(pair[1]);
                    pools.holders[kv.Key].Add(Int(pair[0]), Num(rec["score"]), (bool)rec["committed"]);
                }
            }
            return pools;
        }

        static Dictionary<string, object> PoolsJson(TokenPools p)
        {
            var holders = new Dictionary<string, object>();
            foreach (string type in p.types)
            {
                TokenHolders h = p.holders[type];
                var map = new List<object>();
                for (int i = 0; i < h.Count; i++)
                    map.Add(new List<object> { (double)h.ids[i], new Dictionary<string, object> { ["score"] = h.scores[i], ["committed"] = h.committed[i] } });
                holders[type] = new Dictionary<string, object> { ["$map"] = map };
            }
            return new Dictionary<string, object> { ["sizes"] = SizesJson(p.sizes), ["holders"] = holders };
        }

        static void CheckTokens(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "tokens", (i, c) =>
            {
                var a = Args(c);
                string fn = Fn(c);
                if (fn == "tokenPoolSizes")
                {
                    var t = Obj(data, a[2]);
                    var baseSizes = new List<(string, int)>();
                    foreach (var kv in PortData.Obj(t["base"])) baseSizes.Add((kv.Key, Int(kv.Value)));
                    var got = TokenPools.PoolSizes(Int(a[0]), Truthy(a[1]), baseSizes, Nums(data, t["roundSteps"]).ToArray(), Int(t["campBonus"]));
                    r.SameJson(c["result"], SizesJson(got), "result", true);
                    return;
                }
                if (fn == "createTokenPools")
                {
                    var pools = new TokenPools(SizesOf(PortData.Obj(a[0])));
                    r.SameJson(c["result"], PoolsJson(pools), "result", true);
                    return;
                }
                TokenPools p = PoolsOf(PortData.Obj(a[0]));
                object result;
                switch (fn)
                {
                    case "requestToken":
                    {
                        var (granted, stolen) = a.Count > 4 ? p.Request((string)a[1], Int(a[2]), Num(a[3]), Num(a[4])) : p.Request((string)a[1], Int(a[2]), Num(a[3]));
                        result = new Dictionary<string, object> { ["granted"] = granted, ["stolenFrom"] = stolen.HasValue ? (object)(double)stolen.Value : null };
                        break;
                    }
                    case "commitToken": result = p.Commit((string)a[1], Int(a[2])); break;
                    case "releaseToken": result = p.Release((string)a[1], Int(a[2])); break;
                    case "releaseAll":
                        p.ReleaseAll(Int(a[1]));
                        result = Golden.Undefined;
                        break;
                    case "resizeTokenPools":
                    {
                        List<int> lost = a.Count > 2 ? PortData.Arr(a[2]).ConvertAll(Int) : null;
                        List<int> got = p.Resize(SizesOf(Obj(data, a[1])), lost);
                        result = Boxed(got);
                        if (a.Count > 2) r.SameJson(After(c, 2), Boxed(got), "argsAfter[2]");
                        break;
                    }
                    case "holds": result = p.Holds((string)a[1], Int(a[2])); break;
                    case "holderCount": result = (double)p.HolderCount((string)a[1]); break;
                    default:
                        r.Fail($"no C# port mapped for {fn}");
                        return;
                }
                r.SameJson(c["result"], result, "result");
                r.SameJson(After(c, 0), PoolsJson(p), "argsAfter[0]", true);
            });
        }

        // --- slots.js (Slots.cs) ---------------------------------------------------------------------------------------

        static Slots SlotsOf(Dictionary<string, object> o)
        {
            var ringsJson = PortData.Arr(o["rings"]);
            var spec = new List<(double, int)>();
            foreach (object ro in ringsJson) spec.Add((Num(PortData.Obj(ro)["radius"]), Int(PortData.Obj(ro)["count"])));
            var s = new Slots(spec);
            for (int k = 0; k < ringsJson.Count; k++)
            {
                var owners = PortData.Arr(PortData.Obj(ringsJson[k])["owners"]);
                for (int i = 0; i < owners.Count; i++) s.rings[k].owners[i] = owners[i] == null ? (int?)null : Int(owners[i]);
            }
            foreach (object e in PortData.Arr(PortData.Obj(o["byOwner"])["$map"]))
            {
                var pair = PortData.Arr(e);
                var held = PortData.Obj(pair[1]);
                s.byOwner.Add((Int(pair[0]), Int(held["ring"]), Int(held["index"])));
            }
            return s;
        }

        static Dictionary<string, object> SlotsJson(Slots s)
        {
            var rings = new List<object>();
            foreach (Slots.Ring ring in s.rings)
            {
                var owners = new List<object>();
                foreach (int? o in ring.owners) owners.Add(o.HasValue ? (object)(double)o.Value : null);
                rings.Add(new Dictionary<string, object> { ["radius"] = ring.radius, ["count"] = (double)ring.count, ["owners"] = owners });
            }
            var map = new List<object>();
            foreach (var (owner, ring, index) in s.byOwner)
                map.Add(new List<object> { (double)owner, new Dictionary<string, object> { ["ring"] = (double)ring, ["index"] = (double)index } });
            return new Dictionary<string, object> { ["rings"] = rings, ["byOwner"] = new Dictionary<string, object> { ["$map"] = map } };
        }

        static Dictionary<string, object> XZ(double x, double z) => new Dictionary<string, object> { ["x"] = x, ["z"] = z };

        static void CheckSlots(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "slots", (i, c) =>
            {
                var a = Args(c);
                switch (Fn(c))
                {
                    case "createSlots":
                    {
                        var spec = new List<(double, int)>();
                        foreach (object ro in PortData.Arr(Deref(data, a[0]))) spec.Add((Num(PortData.Obj(ro)["radius"]), Int(PortData.Obj(ro)["count"])));
                        r.SameJson(c["result"], SlotsJson(new Slots(spec)), "result", true);
                        break;
                    }
                    case "slotPoint":
                    {
                        Slots s = SlotsOf(PortData.Obj(a[0]));
                        var center = PortData.Obj(a[3]);
                        Slots.SlotPoint(s, Int(a[1]), Int(a[2]), Num(center["x"]), Num(center["z"]), out double x, out double z);
                        r.SameJson(c["result"], XZ(x, z), "result");
                        r.SameJson(After(c, 4), XZ(x, z), "argsAfter[4]");
                        break;
                    }
                    case "claimSlot":
                    {
                        Slots s = SlotsOf(PortData.Obj(a[0]));
                        var from = PortData.Obj(a[3]);
                        var center = PortData.Obj(a[4]);
                        Func<double, double, bool> accept = null;
                        if (a.Count > 5 && a[5] != null)
                        {
                            string rule = (string)PortData.Obj(PortData.Obj(a[5])["$fn"])["$callback"];
                            if (rule != "accept(x, z) = x + z < 4")
                            {
                                r.Fail($"unknown recorded rule {rule}");
                                return;
                            }
                            accept = (x, z) => x + z < 4;
                        }
                        int got = Slots.ClaimSlot(s, Int(a[1]), Int(a[2]), Num(from["x"]), Num(from["z"]), Num(center["x"]), Num(center["z"]), accept);
                        r.Same(c["result"], got, "result");
                        r.SameJson(After(c, 0), SlotsJson(s), "argsAfter[0]", true);
                        break;
                    }
                    case "releaseSlot":
                    {
                        Slots s = SlotsOf(PortData.Obj(a[0]));
                        r.SameBool(c["result"], Slots.ReleaseSlot(s, Int(a[1])), "result");
                        r.SameJson(After(c, 0), SlotsJson(s), "argsAfter[0]", true);
                        break;
                    }
                    case "slotOf":
                    {
                        Slots s = SlotsOf(PortData.Obj(a[0]));
                        object got = Slots.SlotOf(s, Int(a[1]), out int ring, out int index)
                            ? new Dictionary<string, object> { ["ring"] = (double)ring, ["index"] = (double)index }
                            : null;
                        r.SameJson(c["result"], got, "result");
                        break;
                    }
                    case "slotsUnique":
                        r.SameBool(c["result"], Slots.SlotsUnique(SlotsOf(PortData.Obj(a[0]))), "result");
                        break;
                    case "flankPoint":
                    {
                        var center = PortData.Obj(a[0]);
                        Slots.FlankPoint(Num(center["x"]), Num(center["z"]), Num(a[1]), Num(a[2]), Num(a[3]), out double x, out double z);
                        r.SameJson(c["result"], XZ(x, z), "result");
                        r.SameJson(After(c, 4), XZ(x, z), "argsAfter[4]");
                        break;
                    }
                    default:
                        r.Fail($"no C# port mapped for {Fn(c)}");
                        break;
                }
            });
        }

        // --- aim.js (AimMath, Spitter.cs) ------------------------------------------------------------------------------

        static void CheckAim(GoldenData data, GoldenResult r)
        {
            AimMath.A cfgA = AimMath.A.From(data.Config);
            EachRecord(data, r, "aim", (i, c) =>
            {
                var a = Args(c);
                switch (Fn(c))
                {
                    case "hitChance":
                        if (!IsAimRef(a[1]))
                        {
                            r.Fail($"unsupported config {Golden.Describe(a[1])}");
                            return;
                        }
                        r.Same(c["result"], AimMath.HitChance(Num(a[0]), cfgA), "result");
                        break;
                    case "leadPoint":
                    {
                        double t = AimMath.LeadPoint(PortData.V3(a[0]), PortData.V3(a[1]), PortData.V3(a[2]), Num(a[3]), Num(a[4]), out Vec3 output);
                        r.Same(c["result"], t, "result");
                        r.SameVec(After(c, 5), output, "argsAfter[5]");
                        break;
                    }
                    case "missPoint":
                    {
                        Vec3 got = AimMath.MissPoint(PortData.V3(a[0]), PortData.V3(a[1]), Num(a[2]), Num(a[3]));
                        r.SameVec(c["result"], got, "result");
                        r.SameVec(After(c, 1), got, "argsAfter[1]");
                        break;
                    }
                    default:
                        r.Fail($"no C# port mapped for {Fn(c)}");
                        break;
                }
            });
        }

        static bool IsAimRef(object o) => o is Dictionary<string, object> d && d.TryGetValue("$config", out object p) && (string)p == "ai.aim";

        // --- crowd.js (Crowd, HuskBrain.cs) ----------------------------------------------------------------------------

        static List<object> BodiesJson(CrowdBody[] b, int n)
        {
            var l = new List<object>();
            for (int i = 0; i < n; i++)
                l.Add(new Dictionary<string, object> { ["x"] = b[i].x, ["y"] = b[i].y, ["z"] = b[i].z, ["vx"] = b[i].vx, ["vz"] = b[i].vz, ["r"] = b[i].r, ["m"] = b[i].m });
            return l;
        }

        static void CheckCrowd(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "crowd", (i, c) =>
            {
                if (Fn(c) != "separateBodies")
                {
                    r.Fail($"no C# port mapped for {Fn(c)}");
                    return;
                }
                var a = Args(c);
                var list = PortData.Arr(a[0]);
                var bodies = new CrowdBody[list.Count];
                for (int k = 0; k < list.Count; k++)
                {
                    var o = PortData.Obj(list[k]);
                    bodies[k] = new CrowdBody { x = Num(o["x"]), y = Num(o["y"]), z = Num(o["z"]), vx = Num(o["vx"]), vz = Num(o["vz"]), r = Num(o["r"]), m = Num(o["m"]) };
                }
                var cfg = Obj(data, a[1]);
                int count = a.Count > 2 ? Int(a[2]) : list.Count;
                int overlaps = Crowd.SeparateBodies(bodies, Num(cfg["gap"]), Num(cfg["stiffness"]), Num(cfg["maxPush"]), Num(cfg["height"]), Math.Min(count, list.Count));
                r.Same(c["result"], overlaps, "result");
                r.SameJson(After(c, 0), BodiesJson(bodies, list.Count), "argsAfter[0]", true);
            });
        }

        // --- watchdog.js (Watch, HuskBrain.cs) -------------------------------------------------------------------------

        static Watch WatchOf(Dictionary<string, object> o) => new Watch
        {
            stuck = Num(o["stuck"]), next = Int(o["next"]), cycleStart = Num(o["cycleStart"]), ax = Num(o["ax"]), az = Num(o["az"]), best = Num(o["best"]), events = Int(o["events"]),
        };

        static Dictionary<string, object> WatchJson(Watch w) => new Dictionary<string, object>
        {
            ["stuck"] = w.stuck, ["next"] = (double)w.next, ["cycleStart"] = w.cycleStart, ["ax"] = w.ax, ["az"] = w.az, ["best"] = w.best, ["events"] = (double)w.events,
        };

        static void CheckWatchdog(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "watchdog", (i, c) =>
            {
                var a = Args(c);
                switch (Fn(c))
                {
                    case "createWatch":
                        r.SameJson(c["result"], WatchJson(a.Count > 0 ? new Watch(Num(a[0]), a.Count > 1 ? Num(a[1]) : 0) : new Watch()), "result", true);
                        break;
                    case "resetWatch":
                    {
                        Watch w = WatchOf(PortData.Obj(a[0]));
                        if (a.Count > 3) w.Reset(Num(a[1]), Num(a[2]), Num(a[3]));
                        else w.Reset(Num(a[1]), Num(a[2]));
                        r.SameJson(c["result"], Golden.Undefined, "result");
                        r.SameJson(After(c, 0), WatchJson(w), "argsAfter[0]", true);
                        break;
                    }
                    case "stepWatch":
                    {
                        Watch w = WatchOf(PortData.Obj(a[0]));
                        var cfg = Obj(data, a[6]);
                        string action = w.Step(Num(a[1]), Num(a[2]), Num(a[3]), (bool)a[4], Num(a[5]), Num(cfg["minMove"]), Num(cfg["minGain"]), Nums(data, cfg["ladder"]).ToArray(), Strs(data, cfg["actions"]));
                        r.SameJson(c["result"], action, "result");
                        r.SameJson(After(c, 0), WatchJson(w), "argsAfter[0]", true);
                        break;
                    }
                    case "canRelocate":
                        r.SameBool(c["result"], WatchOf(PortData.Obj(a[0])).CanRelocate(Num(a[1])), "result");
                        break;
                    default:
                        r.Fail($"no C# port mapped for {Fn(c)}");
                        break;
                }
            });
        }

        // --- flowField.js (Flow, NavField.cs) --------------------------------------------------------------------------

        static List<object> Floats(float[] a, int n)
        {
            var l = new List<object>(n);
            for (int i = 0; i < n; i++) l.Add((double)a[i]);
            return l;
        }

        static List<object> Ints(int[] a, int n)
        {
            var l = new List<object>(n);
            for (int i = 0; i < n; i++) l.Add((double)a[i]);
            return l;
        }

        static Dictionary<string, object> FlowJson(Flow f, int n) => new Dictionary<string, object>
        {
            ["len"] = (double)f.len, ["pops"] = (double)f.pops, ["running"] = f.running,
            ["dist"] = Floats(f.dist, n), ["work"] = Floats(f.work, n), ["heapNode"] = Ints(f.heapNode, f.len), ["heapKey"] = Floats(f.heapKey, f.len),
        };

        static bool JumpsOpt(List<object> args, int at) => !(args.Count > at && args[at] is Dictionary<string, object> o && o.TryGetValue("jumps", out object j) && j is bool b && !b);

        static object PathJson(List<int> nodes, Flow.PathScratch sc)
        {
            if (nodes == null) return null;
            return new Dictionary<string, object> { ["nodes"] = Boxed(nodes), ["links"] = Boxed(sc.links), ["cost"] = (double)sc.cost };
        }

        static void CheckFlowField(GoldenData data, GoldenResult r)
        {
            var cases = data.Cases("flowField");
            for (int ci = 0; ci < cases.Count; ci++)
            {
                var c = cases[ci];
                string session = c.TryGetValue("session", out object so) ? so as string : null;
                if (session == "synthetic") FlowSynthetic(data, r, c);
                else if (session == "survival") FlowSurvival(data, r, c);
                else r.Fail($"#{ci} unknown record {Label(c)}");
            }
            r.where = "";
        }

        static void FlowSynthetic(GoldenData data, GoldenResult r, Dictionary<string, object> c)
        {
            var grids = PortData.Arr(c["grids"]).ConvertAll(g => NavGrid.FromExport(PortData.Obj(g)));
            var f = new Flow();
            var sc = new Flow.PathScratch();
            var ops = PortData.Arr(c["ops"]);
            for (int i = 0; i < ops.Count; i++)
            {
                var o = PortData.Obj(ops[i]);
                string op = (string)o["op"];
                NavGrid s = grids[Int(o["grid"])];
                var a = PortData.Arr(o["args"]);
                r.where = $"synthetic op {i} {op}";
                switch (op)
                {
                    case "computeFlow":
                        f.Start(s, Nums(data, a[0]), JumpsOpt(a, 1));
                        f.Continue(s);
                        r.SameJson(o["after"], FlowJson(f, s.count), "after", true);
                        break;
                    case "startFlow":
                        f.Start(s, Nums(data, a[0]), JumpsOpt(a, 1));
                        r.SameJson(o["after"], FlowJson(f, s.count), "after", true);
                        break;
                    case "continueFlow":
                    {
                        bool done = f.Continue(s, Int(a[0]));
                        r.SameBool(o["result"], done, "result");
                        r.SameJson(o["after"], FlowJson(f, s.count), "after", true);
                        break;
                    }
                    case "bestLink":
                    {
                        bool jumps = (bool)a[0];
                        var got = new List<object>(s.count);
                        for (int n = 0; n < s.count; n++) got.Add((double)Flow.BestLink(s, f.dist, n, jumps));
                        r.SameJson(o["result"], got, "result");
                        break;
                    }
                    case "gridPath":
                    {
                        List<int> nodes = a.Count > 3 ? Flow.GridPath(s, Int(a[0]), Int(a[1]), (bool)a[2], Int(a[3]), sc) : Flow.GridPath(s, Int(a[0]), Int(a[1]), (bool)a[2], 20000, sc);
                        r.SameJson(o["result"], PathJson(nodes, sc), "result", true);
                        r.Same(PortData.Obj(o["after"])["expanded"], sc.expanded, "after.expanded");
                        break;
                    }
                    case "block":
                    {
                        var tags = new HashSet<int>(PortData.Arr(a[0]).ConvertAll(Int));
                        for (int n = 0; n < s.count; n++) s.blocked[n] = (byte)(tags.Contains(s.tag[n]) ? 1 : 0);
                        break;
                    }
                    default:
                        r.Fail($"no C# op for {op}");
                        continue;
                }
                r.cases++;
            }
        }

        static void FlowSurvival(GoldenData data, GoldenResult r, Dictionary<string, object> c)
        {
            string map = c.TryGetValue("map", out object mo) ? (string)mo : "survival";
            NavGrid s = NavGrid.FromExport(PortData.Obj(PortData.Obj(data.Json($"maps/{map}.json"))["navGrid"]));
            int stride = Int(c["stride"]);
            var flows = new Dictionary<string, Flow> { ["jump"] = new Flow(), ["walk"] = new Flow() };
            var sc = new Flow.PathScratch();
            Dictionary<string, object> Summary(Flow fl)
            {
                int finite = 0;
                double sum = 0;
                for (int i = 0; i < s.count; i++)
                {
                    if (fl.dist[i] < float.PositiveInfinity)
                    {
                        finite++;
                        sum += fl.dist[i];
                    }
                }
                var sample = new List<object>();
                for (int k = 0; k * stride < s.count; k++) sample.Add((double)fl.dist[k * stride]);
                return new Dictionary<string, object> { ["len"] = (double)fl.len, ["pops"] = (double)fl.pops, ["running"] = fl.running, ["finite"] = (double)finite, ["sum"] = sum, ["sample"] = sample };
            }
            var ops = PortData.Arr(c["ops"]);
            for (int i = 0; i < ops.Count; i++)
            {
                var o = PortData.Obj(ops[i]);
                string op = (string)o["op"];
                var a = PortData.Arr(o["args"]);
                r.where = $"survival op {i} {op}";
                Flow f = o.TryGetValue("flow", out object fo) ? flows[(string)fo] : null;
                switch (op)
                {
                    case "computeFlow":
                        f.Start(s, Nums(data, a[0]), JumpsOpt(a, 1));
                        f.Continue(s);
                        r.SameJson(o["after"], Summary(f), "after", true);
                        break;
                    case "startFlow":
                        f.Start(s, Nums(data, a[0]), JumpsOpt(a, 1));
                        r.SameJson(o["after"], Summary(f), "after", true);
                        break;
                    case "continueFlow":
                        r.SameBool(o["result"], f.Continue(s, Int(a[0])), "result");
                        r.SameJson(o["after"], Summary(f), "after", true);
                        break;
                    case "bestLink":
                    {
                        bool jumps = (bool)a[0];
                        int every = Int(a[1]);
                        var got = new List<object>();
                        for (int n = 0; n < s.count; n += every) got.Add((double)Flow.BestLink(s, f.dist, n, jumps));
                        r.SameJson(o["result"], got, "result");
                        break;
                    }
                    case "gridPath":
                    {
                        List<int> nodes = Flow.GridPath(s, Int(a[0]), Int(a[1]), (bool)a[2], 20000, sc);
                        r.SameJson(o["result"], PathJson(nodes, sc), "result", true);
                        r.Same(PortData.Obj(o["after"])["expanded"], sc.expanded, "after.expanded");
                        break;
                    }
                    default:
                        r.Fail($"no C# op for {op}");
                        continue;
                }
                r.cases++;
            }
        }
    }
}
