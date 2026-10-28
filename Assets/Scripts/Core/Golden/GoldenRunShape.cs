// NIGHT11 (the run's shape): the p20 sets (port-export-p20/scripts/runShape.mjs).
// - roundType.json: directorMath.js roundType(round, r) through DirectorMath.RoundType, with the real director.rounds
//   ({"$config"}) and 22 variants written out in full (an absent key is JS undefined: falsy, as the web reads it).
// - runMoments.json: runMoments.js gradeOf and scoreLine through RunMoments.GradeOf / ScoreLine (an absent key stays
//   absent; seed may be null, absent, "", 0 or a code).
// - objectives.json: sessions of objectives.js (createObjectives with a fake economy, fake zones over five boxes, a fake
//   player) replayed through Objectives: after every op the calls it made (setBoost, grant, zoneAt, isOpen, the events
//   on the bus, in order; "at" inside a stepN op), hud(), holding, debugState(), the ring's visibility and the step count.
// - traps.json: a session of traps.js (fake enemies, loadout.damage, buyables, zones, arcs, particles) replayed through
//   Traps: the calls (zoneAt, isOpen, damage, launch, arcs, bursts, the events, in order; the buyables' adds and removes
//   as their own ordered list, since the port adds them to Buyables.list), the items, the stats, the enemies.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddRunShape(Dictionary<string, GoldenCheck> d)
        {
            d["roundType"] = GoldenRunShape.CheckRoundType;
            d["objectives"] = GoldenRunShape.CheckObjectives;
            d["runMoments"] = GoldenRunShape.CheckRunMoments;
            d["traps"] = GoldenRunShape.CheckTraps;
        }
    }

    public static class GoldenRunShape
    {
        static double N(object o) => PortData.Num(o);

        static bool IsConfigRef(object o) => o is Dictionary<string, object> d && d.ContainsKey("$config");

        // --- roundType -------------------------------------------------------------------------------------------------

        /// <summary>A director.rounds object written out in full: the keys roundType reads (an absent key is JS undefined: NaN in a comparison or a %).</summary>
        static DirectorMath.D RoundsOf(GoldenData data, object r)
        {
            if (IsConfigRef(r)) return DirectorMath.From(data.Config);
            var o = PortData.Obj(r);
            double K(string k) => o.TryGetValue(k, out object v) && v != null ? N(v) : double.NaN;
            return new DirectorMath.D
            {
                bossEvery = K("bossEvery"), bossFrom = K("bossFrom"), swarmEvery = K("swarmEvery"), swarmOffset = K("swarmOffset"), swarmFrom = K("swarmFrom"),
                objectiveFrom = K("objectiveFrom"), objectiveEvery = K("objectiveEvery"), holdOffset = K("holdOffset"), goldOffset = K("goldOffset"),
            };
        }

        public static void CheckRoundType(GoldenData data, GoldenResult r)
        {
            GoldenWalk.Each(data, r, "roundType", "roundType", c =>
            {
                var a = GoldenWalk.Args(c);
                r.SameStr(c["result"], DirectorMath.RoundType((int)N(a[0]), RoundsOf(data, a[1])), "result");
            });
            GoldenWalk.Unhandled(data, r, "roundType", new[] { "roundType" });
        }

        // --- runMoments ------------------------------------------------------------------------------------------------

        public static void CheckRunMoments(GoldenData data, GoldenResult r)
        {
            RunMoments.Grade g = RunMoments.Grade.From(data.Config);
            GoldenWalk.Each(data, r, "runMoments", "gradeOf", c =>
            {
                var a = GoldenWalk.Args(c);
                var s = PortData.Obj(a[0]);
                r.SameStr(c["result"], RunMoments.GradeOf(N(s["round"]), N(s["bestRank"]), N(s["accuracy"]), g), "result");
            });
            GoldenWalk.Each(data, r, "runMoments", "scoreLine", c =>
            {
                var s = PortData.Obj(GoldenWalk.Args(c)[0]);
                List<object> mutators = s.TryGetValue("mutators", out object m) && m != null ? PortData.Arr(m) : null; // absent: the web's default []
                object seed = s.TryGetValue("seed", out object sd) ? sd : null; // absent and null are both falsy
                object build = s.TryGetValue("build", out object b) ? b : null;
                string got = RunMoments.ScoreLine(s["round"], s["points"], s["grade"], s["bestRankName"], N(s["time"]), mutators, seed, build);
                r.SameStr(c["result"], got, "result");
            });
            GoldenWalk.Unhandled(data, r, "runMoments", new[] { "gradeOf", "scoreLine" });
        }

        // --- the session helpers ---------------------------------------------------------------------------------------

        /// <summary>The fake zones of a session: the first box (list order) containing p, inclusive, else "".</summary>
        sealed class FakeZones
        {
            readonly List<(string id, double[] min, double[] max)> boxes = new List<(string, double[], double[])>();
            public HashSet<string> open = new HashSet<string>();
            readonly List<object> calls;

            public FakeZones(Dictionary<string, object> spec, List<object> calls)
            {
                this.calls = calls;
                foreach (object o in PortData.Arr(spec["boxes"]))
                {
                    var b = PortData.Obj(o);
                    boxes.Add(((string)b["id"], PortData.Arr(b["min"]).ConvertAll(N).ToArray(), PortData.Arr(b["max"]).ConvertAll(N).ToArray()));
                }
                foreach (object id in PortData.Arr(spec["open"])) open.Add((string)id);
            }

            public string ZoneAt(Vec3 p)
            {
                string id = "";
                foreach (var b in boxes)
                {
                    if (p.x >= b.min[0] && p.x <= b.max[0] && p.y >= b.min[1] && p.y <= b.max[1] && p.z >= b.min[2] && p.z <= b.max[2])
                    {
                        id = b.id;
                        break;
                    }
                }
                Log.Add(calls, new Dictionary<string, object> { ["c"] = "zoneAt", ["p"] = Arr(p), ["result"] = id });
                return id;
            }

            public bool IsOpen(string id)
            {
                bool o = id != null && open.Contains(id);
                Log.Add(calls, new Dictionary<string, object> { ["c"] = "isOpen", ["id"] = id, ["result"] = o });
                return o;
            }
        }

        /// <summary>The call log of one op ("at": the 1-based step inside a stepN op).</summary>
        static class Log
        {
            public static int at = -1;

            public static void Add(List<object> calls, Dictionary<string, object> call)
            {
                if (at >= 0) call["at"] = (double)at;
                calls.Add(call);
            }
        }

        static List<object> Arr(Vec3 p) => new List<object> { p.x, p.y, p.z };

        static Vec3 V(object o)
        {
            var a = PortData.Arr(o);
            return new Vec3(N(a[0]), N(a[1]), N(a[2]));
        }

        /// <summary>An event payload as the web's object: its public fields (null ones left out, as the web never sets them).</summary>
        static Dictionary<string, object> Payload(object o)
        {
            var d = new Dictionary<string, object>();
            if (o == null) return d;
            foreach (FieldInfo f in o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object v = f.GetValue(o);
                if (v == null) continue;
                if (f.Name == "assistPay" && v is double ap && ap == 1) continue; // the port-only aim-assist payout (NIGHT9), 1 = the web's (none)
                d[f.Name] = v is Vec3 vv ? Arr(vv) : v;
            }
            return d;
        }

        static void LogEvents(EventBus events, List<object> calls, params string[] names)
        {
            foreach (string name in names)
            {
                string n = name;
                events.On(n, o => Log.Add(calls, new Dictionary<string, object> { ["c"] = "event", ["name"] = n, ["payload"] = o is string s ? (object)s : Payload(o) }));
            }
        }

        /// <summary>The run's config, or a fresh copy with one section's keys replaced (a session's configOverride).</summary>
        static GameConfig ConfigWith(GoldenData data, object over)
        {
            if (!(over is Dictionary<string, object> o)) return data.Config;
            var cfg = GameConfig.Load(System.IO.Path.Combine(data.root, "config.json"));
            foreach (var section in o)
            {
                var into = PortData.Obj(cfg.Get(section.Key));
                foreach (var kv in PortData.Obj(section.Value)) into[kv.Key] = kv.Value;
            }
            return cfg;
        }

        // --- objectives ------------------------------------------------------------------------------------------------

        public static void CheckObjectives(GoldenData data, GoldenResult r)
        {
            int index = 0;
            foreach (var c in data.Cases("objectives"))
            {
                string name = c.TryGetValue("session", out object sn) ? (string)sn : "?";
                r.where = $"objectives.{name}";
                try
                {
                    ObjectivesSession(data, r, c);
                }
                catch (Exception e)
                {
                    r.Fail($"exception: {e.GetType().Name}: {e.Message}");
                }
                r.cases++;
                index++;
            }
            r.where = "";
            if (index == 0) r.Fail("objectives: no sessions");
        }

        static void ObjectivesSession(GoldenData data, GoldenResult r, Dictionary<string, object> c)
        {
            var a = PortData.Obj(c["args"]);
            GameConfig cfg = ConfigWith(data, a.TryGetValue("configOverride", out object ov) ? ov : null);
            var calls = new List<object>();
            var zones = new FakeZones(PortData.Obj(a["zones"]), calls);
            Vec3 player = V(PortData.Obj(PortData.Obj(a["player"])["mover"])["position"]);
            var events = new EventBus();
            var o = new Objectives(cfg, events, zones.ZoneAt, zones.IsOpen,
                (n, m) => Log.Add(calls, new Dictionary<string, object> { ["c"] = "setBoost", ["name"] = n, ["mult"] = m }),
                (amount, reason) => Log.Add(calls, new Dictionary<string, object> { ["c"] = "grant", ["amount"] = amount, ["reason"] = reason }),
                () => player);
            LogEvents(events, calls, "banner", "objectiveStart", "objectiveDone");
            var ops = PortData.Arr(c["ops"]);
            string where = r.where;
            for (int i = 0; i < ops.Count; i++)
            {
                var op = PortData.Obj(ops[i]);
                string kind = (string)op["op"];
                var args = PortData.Arr(op["args"]);
                r.where = $"{where} op {i} ({kind})";
                calls.Clear();
                Log.at = -1;
                int before = r.mismatches.Count;
                switch (kind)
                {
                    case "load":
                        if (args[0] == null) o.Load(null);
                        else
                        {
                            var map = PortData.Obj(args[0]);
                            var spots = new List<Vec3>();
                            if (map.TryGetValue("holdSpots", out object hs) && hs != null)
                                foreach (object s in PortData.Arr(hs)) spots.Add(new Vec3(N(PortData.Obj(s)["x"]), N(PortData.Obj(s)["y"]), N(PortData.Obj(s)["z"])));
                            o.Load(spots);
                        }
                        break;
                    case "begin":
                        r.SameStr(op["result"], o.Begin((int)N(args[0])), "result");
                        break;
                    case "step":
                        o.Step();
                        break;
                    case "stepN":
                        for (int k = 1; k <= (int)N(args[0]); k++)
                        {
                            Log.at = k;
                            o.Step();
                        }
                        Log.at = -1;
                        break;
                    case "emit":
                    {
                        string ev = (string)args[0];
                        var p = PortData.Obj(args[1]);
                        if (ev == "kill")
                        {
                            events.Emit("kill", new KillEvent
                            {
                                owner = p.TryGetValue("owner", out object ow) ? (string)ow : null,
                                kind = p.TryGetValue("kind", out object kk) ? (string)kk : null,
                                enemy = p.TryGetValue("target", out object t) ? (int)N(t) : 0,
                            });
                        }
                        else if (ev == "roundClear") events.Emit("roundClear", new RoundEvent { number = (int)N(p["number"]), points = N(p["points"]) });
                        else r.Fail($"unknown emitted event {ev}");
                        break;
                    }
                    case "setOpen":
                        zones.open = new HashSet<string>();
                        foreach (object id in PortData.Arr(args[0])) zones.open.Add((string)id);
                        break;
                    case "move":
                        player = V(args[0]);
                        break;
                    default:
                        r.Fail($"unknown op {kind}");
                        break;
                }
                var after = PortData.Obj(op["after"]);
                r.SameJson(after["calls"], new List<object>(calls), "calls", ordered: true);
                r.SameJson(after["hud"], ObjectivesHud(o), "hud");
                r.SameBool(after["holding"], o.Holding, "holding");
                r.SameJson(after["debug"], ObjectivesDebug(o), "debug");
                r.SameBool(PortData.Obj(after["ring"])["visible"], o.RingVisible, "ring.visible");
                r.Same(after["steps"], o.Time, "steps");
                if (r.mismatches.Count - before > 6) break; // the first divergence is enough
            }
            r.where = where;
        }

        static object ObjectivesHud(Objectives o)
        {
            if (!o.HudOn) return null;
            if (o.HudType == "hold") return new Dictionary<string, object> { ["type"] = "hold", ["left"] = (double)o.HudLeft, ["inside"] = o.HudInside };
            return new Dictionary<string, object> { ["type"] = o.HudType };
        }

        static object ObjectivesDebug(Objectives o) => new Dictionary<string, object>
        {
            ["type"] = o.Type, ["round"] = (double)o.Round, ["spot"] = o.Spot.HasValue ? (object)o.Spot.Value : null, ["held"] = (double)o.Held,
            ["need"] = (double)o.Need, ["done"] = o.Done, ["inside"] = o.Inside,
        };

        // --- traps -----------------------------------------------------------------------------------------------------

        /// <summary>A session enemy: the recorded id, kind, hp, feet, aim height and boss flag.</summary>
        sealed class SessionEnemy : Enemy
        {
            public double aimHeight;
            public bool boss;
            public override double Height => 1.8;
            public override double Radius => 0.4;
            public override double AimHeight => aimHeight;

            public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
            {
                headshot = false;
                return -1;
            }
        }

        public static void CheckTraps(GoldenData data, GoldenResult r)
        {
            int n = 0;
            foreach (var c in data.Cases("traps"))
            {
                string name = c.TryGetValue("session", out object sn) ? (string)sn : "?";
                r.where = $"traps.{name}";
                try
                {
                    TrapsSession(data, r, c);
                }
                catch (Exception e)
                {
                    r.Fail($"exception: {e.GetType().Name}: {e.Message} {e.StackTrace?.Split('\n')[0]}");
                }
                r.cases++;
                n++;
            }
            r.where = "";
            if (n == 0) r.Fail("traps: no sessions");
        }

        static void TrapsSession(GoldenData data, GoldenResult r, Dictionary<string, object> c)
        {
            var a = PortData.Obj(c["args"]);
            GameConfig cfg = data.Config;
            var calls = new List<object>();
            var zones = new FakeZones(PortData.Obj(a["zones"]), calls);
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var buyables = new Buyables(cfg, events, new Economy(cfg, events));
            var byId = new Dictionary<int, SessionEnemy>();
            Func<Enemy, double, Vec3, bool> damage = (e, amount, from) =>
            {
                bool killed = false;
                if (e.alive)
                {
                    e.hp -= amount;
                    if (e.hp <= 0)
                    {
                        e.alive = false;
                        killed = true;
                    }
                }
                Log.Add(calls, new Dictionary<string, object>
                {
                    ["c"] = "damage", ["enemy"] = (double)e.id, ["amount"] = amount, ["headshot"] = false, ["weapon"] = "trap", ["from"] = Arr(from),
                    ["assisted"] = false, ["pointScale"] = 1.0, ["opts"] = 0.0, ["killed"] = killed,
                });
                return killed;
            };
            var traps = new Traps(cfg, events, buyables, zones.ZoneAt, zones.IsOpen, enemies, damage,
                (e, speed) => Log.Add(calls, new Dictionary<string, object> { ["c"] = "launch", ["enemy"] = (double)e.id, ["speed"] = speed }));
            traps.isBoss = e => ((SessionEnemy)e).boss;
            traps.arcs = arc => Log.Add(calls, new Dictionary<string, object>
            {
                ["c"] = "arc", ["from"] = Arr(arc.from), ["to"] = Arr(arc.to), ["hex"] = (double)arc.hex, ["intensity"] = arc.intensity, ["life"] = arc.life,
                ["jag"] = arc.jag, ["width"] = arc.width,
            });
            traps.burst = (at, colorHex, count, speed, life, scale) => Log.Add(calls, new Dictionary<string, object>
            {
                ["c"] = "burst", ["position"] = Arr(at), ["colorHex"] = (double)colorHex, ["count"] = (double)count, ["speed"] = speed, ["life"] = life, ["scale"] = scale,
            });
            LogEvents(events, calls, "trap", "trapKill", "styleBonus", "trapSlam");
            var ops = PortData.Arr(c["ops"]);
            string where = r.where;
            var shownItems = new List<Buyables.Item>();
            for (int i = 0; i < ops.Count; i++)
            {
                var op = PortData.Obj(ops[i]);
                string kind = (string)op["op"];
                var args = PortData.Arr(op["args"]);
                r.where = $"{where} op {i} ({kind})";
                calls.Clear();
                Log.at = -1;
                int before = r.mismatches.Count;
                var itemsBefore = new List<Buyables.Item>(buyables.list);
                switch (kind)
                {
                    case "load":
                    {
                        var spots = new List<TrapSpot>();
                        if (args[0] != null)
                        {
                            foreach (object so in PortData.Arr(PortData.Obj(args[0])["trapSpots"]))
                            {
                                var s = PortData.Obj(so);
                                spots.Add(new TrapSpot { trap = (string)s["trap"], pos = new Vec3(N(s["x"]), N(s["y"]), N(s["z"])), sx = N(s["sx"]), sz = N(s["sz"]) });
                            }
                            traps.Load(spots);
                        }
                        else traps.Load(null);
                        break;
                    }
                    case "clear":
                        traps.Clear();
                        break;
                    case "setOpen":
                        zones.open = new HashSet<string>();
                        foreach (object id in PortData.Arr(args[0])) zones.open.Add((string)id);
                        break;
                    case "spawn":
                    {
                        var s = PortData.Obj(args[0]);
                        var e = new SessionEnemy
                        {
                            id = (int)N(s["id"]), kind = (string)s["kind"], boss = s.TryGetValue("isBoss", out object ib) && ib is bool bb && bb, hp = N(s["hp"]), maxHp = N(s["hp"]),
                            alive = true, position = V(s["position"]), aimHeight = N(s["aimHeight"]),
                        };
                        byId[e.id] = e;
                        enemies.all.Add(e);
                        break;
                    }
                    case "move":
                        byId[(int)N(args[0])].position = V(args[1]);
                        break;
                    case "killEnemy":
                        byId[(int)N(args[0])].alive = false;
                        break;
                    case "available":
                        r.SameBool(op["result"], buyables.list.Find(x => x.id == "trap:" + (string)args[0]).available(), "result");
                        break;
                    case "buy":
                        buyables.list.Find(x => x.id == "trap:" + (string)args[0]).buy();
                        break;
                    case "debugFire":
                        r.SameBool(op["result"], traps.DebugFire((string)args[0]), "result");
                        break;
                    case "step":
                        traps.Step(N(args[0]));
                        break;
                    case "stepN":
                        for (int k = 1; k <= (int)N(args[1]); k++)
                        {
                            Log.at = k;
                            traps.Step(N(args[0]));
                        }
                        Log.at = -1;
                        break;
                    default:
                        r.Fail($"unknown op {kind}");
                        break;
                }
                var after = PortData.Obj(op["after"]);
                // The buyables' adds and removes, in order (the port changes Buyables.list itself), against the recorded ones.
                var wantBuy = new List<object>();
                var wantCalls = new List<object>();
                foreach (object w in PortData.Arr(after["calls"]))
                {
                    string cn = (string)PortData.Obj(w)["c"];
                    if (cn == "buyablesAdd" || cn == "buyablesRemove") wantBuy.Add(w);
                    else wantCalls.Add(w);
                }
                var gotBuy = new List<object>();
                foreach (Buyables.Item it in itemsBefore) if (!buyables.list.Contains(it)) gotBuy.Add(new Dictionary<string, object> { ["c"] = "buyablesRemove", ["id"] = it.id });
                foreach (Buyables.Item it in buyables.list)
                {
                    if (itemsBefore.Contains(it)) continue;
                    gotBuy.Add(new Dictionary<string, object>
                    {
                        ["c"] = "buyablesAdd", ["id"] = it.id, ["kind"] = it.kind, ["name"] = it.name, ["position"] = it.position, ["reach"] = it.reach, ["price"] = it.price(),
                    });
                }
                r.SameJson(wantBuy, gotBuy, "buyables", ordered: true);
                r.SameJson(wantCalls, new List<object>(calls), "calls", ordered: true);
                r.SameJson(after["items"], TrapItems(traps), "items", ordered: true);
                r.SameJson(after["stats"], TrapStats(traps), "stats");
                var en = new List<object>();
                foreach (Enemy e in enemies.all) en.Add(new Dictionary<string, object> { ["id"] = (double)e.id, ["hp"] = e.hp, ["alive"] = e.alive, ["position"] = Arr(e.position) });
                r.SameJson(after["enemies"], en, "enemies", ordered: true);
                if (r.mismatches.Count - before > 6) break;
            }
            r.where = where;
        }

        static List<object> TrapItems(Traps traps)
        {
            var list = new List<object>();
            foreach (Traps.Trap it in traps.items)
            {
                var thrown = new List<object>();
                foreach (Enemy e in it.Thrown) thrown.Add((double)e.id);
                var pending = new List<object>();
                foreach (Traps.Pending p in it.PendingKills) pending.Add(new Dictionary<string, object> { ["enemy"] = (double)p.e.id, ["steps"] = (double)p.steps });
                list.Add(new Dictionary<string, object>
                {
                    ["id"] = it.id, ["zone"] = it.zone, ["active"] = (double)it.active, ["cooldown"] = (double)it.cooldown, ["tick"] = (double)it.tick, ["thrown"] = thrown, ["pending"] = pending,
                });
            }
            return list;
        }

        static Dictionary<string, object> TrapStats(Traps traps)
        {
            var fired = new Dictionary<string, object>();
            foreach (var kv in traps.stats.fired) fired[kv.Key] = (double)kv.Value;
            return new Dictionary<string, object>
            {
                ["fired"] = fired, ["kills"] = (double)traps.stats.kills, ["eliteHits"] = (double)traps.stats.eliteHits, ["bossHits"] = (double)traps.stats.bossHits,
                ["launched"] = (double)traps.stats.launched,
            };
        }
    }
}
