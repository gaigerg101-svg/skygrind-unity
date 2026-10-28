// NIGHT9 U3-07: the checkers of the vector sets recorded for the pure web modules that had none (port-export-p18
// scripts/vectors.mjs): regenMath, pointsMath, modMath, powerUpMath (this file), deathRecap, hitDirection, kicks, springs
// (GoldenNewSetsFeel.cs), tokens, slots, aim, crowd, watchdog, flowField (GoldenNewSetsAi.cs). Each walks every record of
// its file, drives the existing C# port and compares bit for bit (Golden.cs). Plain calls carry {fn, args, result,
// argsAfter?}: the expected state after the call is argsAfter[i] when the web mutated argument i, else args[i] (the port
// must not change what the web left alone). Sessions carry {session, args, ops: [{op, args, result?, after?}]}.
// The helpers live in their own class (GoldenNewSets), so they can't collide with the other partial Golden files.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddNewSets(Dictionary<string, GoldenCheck> d) => GoldenNewSets.Register(d);
    }

    static partial class GoldenNewSets
    {
        public static void Register(Dictionary<string, GoldenCheck> d)
        {
            d["regenMath"] = CheckRegen;
            d["pointsMath"] = CheckPoints;
            d["modMath"] = CheckMods;
            d["powerUpMath"] = CheckPowerUps;
            d["deathRecap"] = CheckDeathRecap;
            d["hitDirection"] = CheckHitDirection;
            d["kicks"] = CheckKicks;
            d["springs"] = CheckSprings;
            d["tokens"] = CheckTokens;
            d["slots"] = CheckSlots;
            d["aim"] = CheckAim;
            d["crowd"] = CheckCrowd;
            d["watchdog"] = CheckWatchdog;
            d["flowField"] = CheckFlowField;
        }

        // --- decoding helpers ---------------------------------------------------------------------------------------

        static List<object> Args(Dictionary<string, object> c) => PortData.Arr(c["args"]);

        static string Fn(Dictionary<string, object> c) => c.TryGetValue("fn", out object f) ? f as string : null;

        /// <summary>A record's label for mismatch lines: its fn, sequence, session or constant name.</summary>
        static string Label(Dictionary<string, object> c)
        {
            foreach (string k in new[] { "fn", "sequence", "session", "constant" })
                if (c.TryGetValue(k, out object v) && v is string s) return k == "fn" ? s : $"{k} {s}";
            return "?";
        }

        /// <summary>Argument i as the web left it: argsAfter[i] when the call mutated an argument, else args[i].</summary>
        static object After(Dictionary<string, object> c, int i) => c.TryGetValue("argsAfter", out object a) ? PortData.Arr(a)[i] : Args(c)[i];

        static double Num(object o) => PortData.Num(o);

        static int Int(object o) => (int)PortData.Num(o);

        static bool IsUndefined(object o) => o is Dictionary<string, object> d && d.ContainsKey("$undefined");

        /// <summary>JS truthiness of a recorded value (missing = undefined = false).</summary>
        static bool Truthy(object o)
        {
            switch (o)
            {
                case null: return false;
                case bool b: return b;
                case double x: return x != 0 && !double.IsNaN(x);
                case string s: return s.Length > 0;
                case Dictionary<string, object> d when d.ContainsKey("$undefined"): return false;
                case Dictionary<string, object> d when d.ContainsKey("$num"): return Truthy(PortData.Num(d));
            }
            return true;
        }

        /// <summary>A recorded value with a {"$config": path} reference resolved to the config value.</summary>
        static object Deref(GoldenData data, object o) => o is Dictionary<string, object> d && d.ContainsKey("$config") ? data.ConfigRef(o) : o;

        static Dictionary<string, object> Obj(GoldenData data, object o) => PortData.Obj(Deref(data, o));

        /// <summary>A field of a recorded object (null when missing), references resolved.</summary>
        static object Field(GoldenData data, Dictionary<string, object> o, string key) => o.TryGetValue(key, out object v) ? Deref(data, v) : null;

        static List<double> Nums(GoldenData data, object o) => PortData.Arr(Deref(data, o)).ConvertAll(PortData.Num);

        static List<string> Strs(GoldenData data, object o) => PortData.Arr(Deref(data, o)).ConvertAll(x => (string)x);

        static List<object> Boxed(IEnumerable<double> xs)
        {
            var l = new List<object>();
            foreach (double x in xs) l.Add(x);
            return l;
        }

        static List<object> Boxed(IEnumerable<int> xs)
        {
            var l = new List<object>();
            foreach (int x in xs) l.Add((double)x);
            return l;
        }

        static List<object> Boxed(IEnumerable<string> xs)
        {
            var l = new List<object>();
            foreach (string x in xs) l.Add(x);
            return l;
        }

        /// <summary>Walk every record of a set: label it, run `one`, count it.</summary>
        static void EachRecord(GoldenData data, GoldenResult r, string set, Action<int, Dictionary<string, object>> one)
        {
            var cases = data.Cases(set);
            for (int i = 0; i < cases.Count; i++)
            {
                r.where = $"#{i} {Label(cases[i])}";
                one(i, cases[i]);
                r.cases++;
            }
            r.where = "";
        }

        /// <summary>Run a session's ops in order; stop at the first op that mismatched (the rest would only cascade).</summary>
        static void EachOp(GoldenResult r, Dictionary<string, object> session, string label, Action<string, List<object>, Dictionary<string, object>> one)
        {
            var ops = PortData.Arr(session["ops"]);
            string where = r.where;
            for (int k = 0; k < ops.Count; k++)
            {
                var op = PortData.Obj(ops[k]);
                string name = (string)op["op"];
                r.where = $"{where} {label} op {k} {name}";
                int before = r.mismatches.Count;
                one(name, PortData.Arr(op["args"]), op);
                if (r.mismatches.Count > before) break;
            }
            r.where = where;
        }

        // --- regenMath (Health.cs RegenMath) -----------------------------------------------------------------------

        static RegenMath.Params RegenParams(Dictionary<string, object> o) => new RegenMath.Params
        {
            delay = Num(o["delay"]), rate = Num(o["rate"]), fastSpeed = Num(o["fastSpeed"]), fastMult = Num(o["fastMult"]), dealtWindow = Num(o["dealtWindow"]),
        };

        static RegenMath.Perk RegenPerk(Dictionary<string, object> o) => new RegenMath.Perk { delay = Num(o["delay"]), rate = Num(o["rate"]) };

        static Dictionary<string, object> PerkJson(RegenMath.Perk p) => new Dictionary<string, object> { ["delay"] = p.delay, ["rate"] = p.rate };

        static RegenState RegenStateOf(object o)
        {
            var d = PortData.Obj(o);
            return new RegenState { delay = Num(d["delay"]), sinceDealt = Num(d["sinceDealt"]), regenerating = (bool)d["regenerating"] };
        }

        static Dictionary<string, object> RegenJson(RegenState s) => new Dictionary<string, object> { ["delay"] = s.delay, ["sinceDealt"] = s.sinceDealt, ["regenerating"] = s.regenerating };

        static void CheckRegen(GoldenData data, GoldenResult r)
        {
            double dt = Num(data.Vectors("regenMath")["dt"]);
            EachRecord(data, r, "regenMath", (i, c) =>
            {
                if (c.ContainsKey("constant"))
                {
                    r.SameJson(c["value"], PerkJson(RegenMath.Perk.None), "NO_REGEN_PERK");
                    return;
                }
                if (c.ContainsKey("sequence"))
                {
                    RegenFight(data, c, dt, r);
                    return;
                }
                var a = Args(c);
                RegenMath.Perk PerkArg(int k) => a.Count > k ? RegenPerk(Obj(data, a[k])) : RegenMath.Perk.None;
                switch (Fn(c))
                {
                    case "rankRegenPerk":
                        r.SameJson(c["result"], PerkJson(RegenMath.RankRegenPerk(RegenPerk(Obj(data, a[0])), Int(a[1]), Num(a[2]))), "result");
                        break;
                    case "createRegenState":
                        r.SameJson(c["result"], RegenJson(new RegenState()), "result");
                        break;
                    case "regenOnDamage":
                    {
                        RegenState s = RegenStateOf(a[0]);
                        RegenMath.OnDamage(s, RegenParams(Obj(data, a[1])), PerkArg(2));
                        r.SameJson(c["result"], RegenJson(s), "result");
                        r.SameJson(After(c, 0), RegenJson(s), "state");
                        break;
                    }
                    case "regenOnDealt":
                    {
                        RegenState s = RegenStateOf(a[0]);
                        RegenMath.OnDealt(s);
                        r.SameJson(c["result"], RegenJson(s), "result");
                        r.SameJson(After(c, 0), RegenJson(s), "state");
                        break;
                    }
                    case "regenCountdownRate":
                        r.Same(c["result"], RegenMath.CountdownRate(Num(a[0]), Num(a[1]), RegenParams(Obj(data, a[2]))), "result");
                        break;
                    case "regenStep":
                    {
                        RegenState s = RegenStateOf(a[0]);
                        var input = PortData.Obj(a[1]);
                        double got = RegenMath.Step(s, Num(input["dt"]), Num(input["speed"]), (bool)input["camping"], Num(input["hp"]), Num(input["max"]), RegenParams(Obj(data, a[2])), PerkArg(3));
                        r.Same(c["result"], got, "result");
                        r.SameJson(After(c, 0), RegenJson(s), "state");
                        break;
                    }
                    default:
                        r.Fail("no check for this record");
                        break;
                }
            });
        }

        /// <summary>The regenFight sequence: events and steps on one state, every column compared.</summary>
        static void RegenFight(GoldenData data, Dictionary<string, object> c, double dt, GoldenResult r)
        {
            var g = RegenMath.From(data.Config);
            var perks = PortData.Arr(c["perks"]).ConvertAll(p => RegenPerk(PortData.Obj(p)));
            var s = new RegenState();
            string where = r.where;
            foreach (object rowObj in PortData.Arr(c["rows"]))
            {
                var row = PortData.Arr(rowObj);
                r.where = $"{where} step {Int(row[0])}";
                int before = r.mismatches.Count;
                int ev = Int(row[1]);
                RegenMath.Perk perk = perks[Int(row[2])];
                if (ev == 1) RegenMath.OnDamage(s, g, perk);
                else if (ev == 2) RegenMath.OnDealt(s);
                double got = RegenMath.Step(s, dt, Num(row[3]), (bool)row[4], Num(row[5]), 100, g, perk);
                r.Same(row[6], got, "returned");
                r.Same(row[7], s.delay, "delay");
                r.Same(row[8], s.sinceDealt, "sinceDealt");
                r.SameBool(row[9], s.regenerating, "regenerating");
                if (r.mismatches.Count > before) break;
            }
            r.where = where;
        }

        // --- pointsMath (Economy.cs PointsMath) --------------------------------------------------------------------

        static void CheckPoints(GoldenData data, GoldenResult r)
        {
            PointsMath.Params p = PointsMath.From(data.Config);
            EachRecord(data, r, "pointsMath", (i, c) =>
            {
                var a = Args(c);
                switch (Fn(c))
                {
                    case "basePoints":
                    {
                        var e = PortData.Obj(a[0]);
                        bool parry = Field(data, e, "type") as string == "parry";
                        double got = PointsMath.BasePoints(parry, Truthy(Field(data, e, "killed")), Truthy(Field(data, e, "headshot")), Field(data, e, "weapon") as string, Truthy(Field(data, e, "melee")), p);
                        r.Same(c["result"], got, "result");
                        break;
                    }
                    case "rankMultiplier":
                        r.Same(c["result"], PointsMath.RankMultiplier(Int(a[0]), p), "result");
                        break;
                    case "applyPointMultipliers":
                        r.Same(c["result"], a.Count > 2 ? PointsMath.ApplyMultipliers(Num(a[0]), Num(a[1]), Num(a[2])) : PointsMath.ApplyMultipliers(Num(a[0]), Num(a[1])), "result");
                        break;
                    case "roundClearPoints":
                        r.Same(c["result"], PointsMath.RoundClearPoints(Int(a[0]), p), "result");
                        break;
                    case "canAfford":
                        r.SameBool(c["result"], PointsMath.CanAfford(Num(a[0]), Num(a[1])), "result");
                        break;
                    default:
                        r.Fail("no check for this record");
                        break;
                }
            });
        }

        // --- modMath (Mods.cs ModMath) -----------------------------------------------------------------------------

        static ModMath.M ModsM(GoldenData data, Dictionary<string, object> o)
        {
            var m = new ModMath.M();
            if (o.TryGetValue("offers", out object offers)) m.offers = Int(offers);
            if (o.TryGetValue("listUntil", out object lu)) m.listUntil = Int(lu);
            if (o.TryGetValue("everyAfter", out object ea)) m.everyAfter = Int(ea);
            if (o.TryGetValue("draftRounds", out object dr)) foreach (double x in Nums(data, dr)) m.draftRounds.Add((int)x);
            if (o.TryGetValue("tierWeight", out object tw)) foreach (var kv in Obj(data, tw)) m.tierWeight[kv.Key] = Num(kv.Value);
            return m;
        }

        static Dictionary<string, ModDef> ModDefs(Dictionary<string, object> o)
        {
            var defs = new Dictionary<string, ModDef>();
            foreach (var kv in o)
            {
                var d = PortData.Obj(kv.Value);
                defs[kv.Key] = new ModDef { id = kv.Key, tag = (string)d["tag"], tier = (string)d["tier"], max = d.TryGetValue("max", out object mx) ? Int(mx) : 0 };
            }
            return defs;
        }

        static Dictionary<string, (int offered, int picked)> PickStats(object o)
        {
            var stats = new Dictionary<string, (int, int)>();
            foreach (var kv in PortData.Obj(o))
            {
                var s = PortData.Obj(kv.Value);
                stats[kv.Key] = (Int(s["offered"]), Int(s["picked"]));
            }
            return stats;
        }

        /// <summary>A recorded seeded rng argument ({$fn: {$rng: mulberry32, seed, draws: 0}}) as the port's stream.</summary>
        static Func<double> RngOf(object o)
        {
            var desc = PortData.Obj(PortData.Obj(o)["$fn"]);
            if ((string)desc["$rng"] != "mulberry32" || Num(desc["draws"]) != 0) throw new FormatException("an rng recorded mid-stream");
            return new Mulberry32(Num(desc["seed"])).Next;
        }

        static void CheckMods(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "modMath", (i, c) =>
            {
                var a = Args(c);
                switch (Fn(c))
                {
                    case "isDraftRound":
                        r.SameBool(c["result"], ModMath.IsDraftRound(Int(a[0]), ModsM(data, Obj(data, a[1]))), "result");
                        break;
                    case "rollDraft":
                    {
                        var stacks = new Dictionary<string, int>();
                        foreach (var kv in PortData.Obj(a[3])) stacks[kv.Key] = Int(kv.Value);
                        List<string> exclude = a.Count > 5 ? Strs(data, a[5]) : null;
                        var got = ModMath.RollDraft(RngOf(a[0]), Strs(data, a[1]), ModDefs(Obj(data, a[2])), stacks, ModsM(data, Obj(data, a[4])), exclude);
                        r.SameJson(c["result"], Boxed(got), "result");
                        break;
                    }
                    case "recordPick":
                    {
                        var got = ModMath.RecordPick(PickStats(a[0]), Strs(data, a[1]), a[2] as string);
                        var json = new Dictionary<string, object>();
                        foreach (var kv in got) json[kv.Key] = new Dictionary<string, object> { ["offered"] = (double)kv.Value.offered, ["picked"] = (double)kv.Value.picked };
                        r.SameJson(c["result"], json, "result");
                        break;
                    }
                    case "pickRate":
                    {
                        double? got = ModMath.PickRate(PickStats(a[0]), (string)a[1]);
                        r.SameJson(c["result"], got.HasValue ? (object)got.Value : null, "result");
                        break;
                    }
                    default:
                        r.Fail("no check for this record");
                        break;
                }
            });
        }

        // --- powerUpMath (PowerUps.cs PowerUpMath) -----------------------------------------------------------------

        static PowerUpMath.StyleDropState DropStateOf(object o)
        {
            var d = PortData.Obj(o);
            var s = new PowerUpMath.StyleDropState { readyAt = Num(d["readyAt"]) };
            s.kills.AddRange(PortData.Arr(d["kills"]).ConvertAll(PortData.Num));
            s.air.AddRange(PortData.Arr(d["air"]).ConvertAll(PortData.Num));
            return s;
        }

        static Dictionary<string, object> DropJson(PowerUpMath.StyleDropState s) => new Dictionary<string, object> { ["kills"] = Boxed(s.kills), ["air"] = Boxed(s.air), ["readyAt"] = s.readyAt };

        static void CheckPowerUps(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "powerUpMath", (i, c) =>
            {
                var a = Args(c);
                switch (Fn(c))
                {
                    case "createStyleDropState":
                        r.SameJson(c["result"], DropJson(new PowerUpMath.StyleDropState()), "result");
                        break;
                    case "styleDropOnKill":
                    {
                        var s = DropStateOf(a[0]);
                        var o = Obj(data, a[3]);
                        var cfg = new PowerUpMath.StyleDrop { chainKills = Num(o["chainKills"]), chainWindow = Num(o["chainWindow"]), airKills = Num(o["airKills"]), airWindow = Num(o["airWindow"]), cooldown = Num(o["cooldown"]) };
                        string got = PowerUpMath.StyleDropOnKill(s, Num(a[1]), (bool)a[2], cfg);
                        r.SameJson(c["result"], got, "result");
                        r.SameJson(After(c, 0), DropJson(s), "state");
                        break;
                    }
                    case "killDropChance":
                    {
                        var o = Obj(data, a[1]);
                        var k = new PowerUpMath.KillDrop
                        {
                            enabled = (bool)o["enabled"], chance = Num(o["chance"]), eliteMult = Num(o["eliteMult"]), rise = Num(o["rise"]), rise2 = Num(o["rise2"]),
                            minGap = Num(o["minGap"]), maxChance = Num(o["maxChance"]),
                        };
                        var opts = a.Count > 2 ? PortData.Obj(a[2]) : new Dictionary<string, object>();
                        bool elite = opts.TryGetValue("elite", out object el) && Truthy(el);
                        double mult = opts.TryGetValue("mult", out object mu) && !IsUndefined(mu) ? Num(mu) : 1;
                        r.Same(c["result"], PowerUpMath.KillDropChance(Num(a[0]), k, elite, mult), "result");
                        break;
                    }
                    default:
                        r.Fail("no check for this record");
                        break;
                }
            });
        }
    }
}
