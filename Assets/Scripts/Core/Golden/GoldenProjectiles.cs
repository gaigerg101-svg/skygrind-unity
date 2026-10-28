// NIGHT10 U3-13.3 checkers (port-export-p19/scripts/projectiles.mjs):
// - projectiles: sessions on projectiles.js (Projectiles.cs) in a box arena with fake enemies (vertical capsules),
//   two fake loadouts and one owner; every op's calls (damage, status, impulse, events), each fired projectile's state,
//   the stuck counts, the live counts and the enemies, bit for bit. V8's Math.random is a mulberry32 with the session's
//   seed; Projectiles draws from a Mulberry32 with the same seed.
// - weapons: the rail sessions on the real weapons.js loadout (Loadout.cs: charge, cancel, cooldown, pierce, the
//   overcharge special and the walls variant), the arsenal off and momentum on, as the recorder builds it.
// The fakes mirror the recorder's facades exactly: EnemyRegistry is the enemies facade (raycast: the nearest alive
// capsule hit within the length, the earlier one on a tie; within: alive, aim point within the radius, in order).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddProjectiles(Dictionary<string, GoldenCheck> d)
        {
            d["projectiles"] = GoldenNewSets.CheckProjectiles;
            d["weapons"] = GoldenNewSets.CheckWeapons;
        }
    }

    static partial class GoldenNewSets
    {
        /// <summary>The recorder's fake enemy: a vertical capsule standing on its position, its aim point aimHeight up.</summary>
        sealed class FakeEnemy : Enemy
        {
            public double height, radius, aimHeight;
            public override double Height => height;
            public override double Radius => radius;
            public override double AimHeight => aimHeight;

            public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
            {
                headshot = false;
                return CombatMath.RayVerticalCapsule(o, d, position, height, radius);
            }
        }

        static EnemyRegistry FakeEnemies(Dictionary<string, object> session, Dictionary<int, FakeEnemy> byId)
        {
            var reg = new EnemyRegistry();
            foreach (object o in PortData.Arr(session["enemies"]))
            {
                var e = PortData.Obj(o);
                var f = new FakeEnemy
                {
                    id = (int)Num(e["id"]), kind = e.TryGetValue("kind", out object k) ? (string)k : "husk", alive = true, hp = Num(e["hp"]),
                    position = PortData.V3(e["position"]), height = Num(e["height"]), radius = Num(e["radius"]), aimHeight = Num(e["aimHeight"]),
                };
                f.maxHp = f.hp;
                reg.all.Add(f);
                byId[f.id] = f;
            }
            return reg;
        }

        static ICollisionWorld ArenaOf(Dictionary<string, object> session) =>
            ThreeGeometry.BuildOctree(PortData.Arr(PortData.Obj(session["arena"])["boxes"]).ConvertAll(PortData.Box), null);

        static List<object> V(Vec3 v) => new List<object> { v.x, v.y, v.z };

        sealed class FakeLoadout : IProjectileLoadout
        {
            public int idx, seq, shot;
            public bool arsenal;
            public List<object> calls;

            public int Shot => shot;
            public int NewShot() => shot = ++seq;
            public bool ArsenalEnabled => arsenal;

            public void Status(Enemy enemy, string name) =>
                calls.Add(new Dictionary<string, object> { ["c"] = "status", ["loadout"] = (double)idx, ["enemy"] = (double)enemy.id, ["kind"] = name });

            public bool Damage(Enemy enemy, double amount, bool headshot, string weapon, Vec3 from, bool wasAssisted, double pointScale, int shotId, bool blast)
            {
                object opts = blast ? new Dictionary<string, object> { ["shot"] = (double)shotId, ["blast"] = true } : (object)(double)shotId;
                calls.Add(new Dictionary<string, object>
                {
                    ["c"] = "damage", ["loadout"] = (double)idx, ["enemy"] = (double)enemy.id, ["amount"] = amount, ["headshot"] = headshot, ["weapon"] = weapon,
                    ["from"] = V(from), ["assisted"] = wasAssisted, ["pointScale"] = pointScale, ["opts"] = opts,
                });
                if (!enemy.alive) return false;
                enemy.hp -= amount;
                if (enemy.hp <= 0)
                {
                    enemy.alive = false;
                    return true;
                }
                return false;
            }
        }

        sealed class FakeOwner : IProjectileOwner
        {
            public string id = "p1";
            public Vec3 position;
            public List<object> calls;
            public string Id => id;
            public Vec3 Position => position;

            public void Impulse(double x, double y, double z) =>
                calls.Add(new Dictionary<string, object> { ["c"] = "impulse", ["x"] = x, ["y"] = y, ["z"] = z });
        }

        static Dictionary<string, object> Ev(string name, Dictionary<string, object> payload) =>
            new Dictionary<string, object> { ["c"] = "event", ["name"] = name, ["payload"] = payload };

        /// <summary>A recorded call vs the C# one: the kind, then its fields (a status' `time` is undefined on the web for these).</summary>
        static void SameCall(GoldenResult r, Dictionary<string, object> want, Dictionary<string, object> got, string what)
        {
            string c = (string)want["c"];
            if (!r.SameStr(c, got["c"] as string, what + ".c")) return;
            switch (c)
            {
                case "damage":
                case "impulse":
                    foreach (var kv in want) r.SameJson(kv.Value, got.TryGetValue(kv.Key, out object g) ? g : null, $"{what}.{kv.Key}");
                    break;
                case "status":
                    r.SameJson(want["loadout"], got["loadout"], what + ".loadout");
                    r.SameJson(want["enemy"], got["enemy"], what + ".enemy");
                    r.SameJson(want["kind"], got["kind"], what + ".kind");
                    break;
                case "event":
                {
                    string name = (string)want["name"];
                    if (!r.SameStr(name, got["name"] as string, what + ".name")) return;
                    var wp = PortData.Obj(want["payload"]);
                    var gp = (Dictionary<string, object>)got["payload"];
                    if (name == "shotResult")
                    {
                        r.SameJson(wp["weapon"], gp["weapon"], what + ".weapon");
                        r.Same((double)PortData.Arr(wp["hits"]).Count, Num(gp["hits"]), what + ".hits.length");
                        r.SameJson(wp["rayCount"], gp["rayCount"], what + ".rayCount");
                    }
                    else
                    {
                        foreach (var kv in wp)
                        {
                            if (kv.Key == "owner") continue; // 'p1' on both sides; checked by the shape
                            r.SameJson(kv.Value, gp.TryGetValue(kv.Key, out object g) ? g : null, $"{what}.{kv.Key}");
                        }
                    }
                    break;
                }
                default:
                    r.Fail($"{what}: no check for call '{c}'");
                    break;
            }
        }

        static void SameCalls(GoldenResult r, List<object> want, List<object> got)
        {
            if (want.Count != got.Count)
            {
                var names = new List<string>();
                foreach (object g in got) names.Add(Describe((Dictionary<string, object>)g));
                var wnames = new List<string>();
                foreach (object w in want) wnames.Add(Describe(PortData.Obj(w)));
                r.Fail($"calls: expected {want.Count} [{string.Join(", ", wnames)}] got {got.Count} [{string.Join(", ", names)}]");
                return;
            }
            for (int i = 0; i < want.Count; i++) SameCall(r, PortData.Obj(want[i]), (Dictionary<string, object>)got[i], $"calls[{i}]");
        }

        static string Describe(Dictionary<string, object> c) => (string)c["c"] == "event" ? "event " + c["name"] : (string)c["c"];

        // --- projectiles ---------------------------------------------------------------------------------------------

        internal static void CheckProjectiles(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "projectiles", (i, c) =>
            {
                if (!c.ContainsKey("session"))
                {
                    r.Fail("no check for this record");
                    return;
                }
                ProjectileSession(data, c, r);
            });
        }

        static void ProjectileSession(GoldenData data, Dictionary<string, object> c, GoldenResult r)
        {
            GameConfig cfg = data.Config;
            var calls = new List<object>();
            var byId = new Dictionary<int, FakeEnemy>();
            EnemyRegistry enemies = FakeEnemies(c, byId);
            var events = new EventBus();
            var proj = new Projectiles(cfg, events, ArenaOf(c), enemies, (uint)Num(c["seed"]));
            var loadouts = new List<FakeLoadout>();
            var lds = PortData.Arr(c["loadouts"]);
            for (int k = 0; k < lds.Count; k++)
            {
                var ld = PortData.Obj(lds[k]);
                loadouts.Add(new FakeLoadout { idx = k, arsenal = ld.TryGetValue("arsenal", out object a) && a is bool b && b, calls = calls });
            }
            var ownerRec = PortData.Obj(c["owner"]);
            var owner = new FakeOwner { id = (string)ownerRec["id"], position = PortData.V3(ownerRec["position"]), calls = calls };
            events.On<BlastEvent>("rocketBlast", e => calls.Add(Ev("rocketBlast", new Dictionary<string, object> { ["position"] = e.position, ["radius"] = e.radius, ["hits"] = (double)e.hits })));
            events.On<BlastJumpEvent>("rocketJump", e => calls.Add(Ev("rocketJump", new Dictionary<string, object> { ["speed"] = e.speed, ["weapon"] = e.weapon })));
            events.On<DetonateEvent>("nailDetonate", e => calls.Add(Ev("nailDetonate", new Dictionary<string, object> { ["nails"] = (double)e.nails })));
            events.On<FragmentEvent>("rocketFragment", e => calls.Add(Ev("rocketFragment", new Dictionary<string, object> { ["position"] = e.position, ["hits"] = (double)e.hits })));
            events.On<ShotEvent>("shotResult", e => calls.Add(Ev("shotResult", new Dictionary<string, object> { ["weapon"] = e.weapon, ["hits"] = (double)e.hitCount, ["rayCount"] = (double)e.rayCount })));
            var fired = new List<Projectiles.Projectile>();
            EachOp(r, c, (string)c["session"], (op, a, rec) =>
            {
                calls.Clear();
                switch (op)
                {
                    case "newShot":
                        r.Same(rec["result"], loadouts[(int)Num(a[0])].NewShot(), "result");
                        break;
                    case "fire":
                    {
                        HomingTarget? homing = null;
                        if (a[4] is Dictionary<string, object> h)
                        {
                            FakeEnemy target = byId[(int)Num(h["enemy"])];
                            homing = new HomingTarget { enemy = target, id = h.TryGetValue("id", out object hid) ? (int)Num(hid) : target.id, turn = Num(h["turn"]) };
                        }
                        Projectiles.Projectile p = proj.Fire((string)a[0], loadouts[(int)Num(a[1])], owner, PortData.V3(a[2]), PortData.V3(a[3]), homing, a[5] is bool asd && asd);
                        if (p != null) fired.Add(p);
                        r.SameJson(rec["result"], p != null ? (object)(double)(fired.Count - 1) : null, "result");
                        break;
                    }
                    case "set":
                    {
                        Projectiles.Projectile p = fired[(int)Num(a[0])];
                        string field = (string)a[1];
                        if (field == "pierce") p.pierce = (int)Num(a[2]);
                        else if (field == "radiusMult") p.radiusMult = Num(a[2]);
                        else if (field == "fragments") p.fragments = (int)Num(a[2]);
                        else r.Fail($"set: unknown field {field}");
                        break;
                    }
                    case "step":
                        proj.Step(Num(a[0]));
                        break;
                    case "detonate":
                        r.Same(rec["result"], proj.Detonate(loadouts[(int)Num(a[0])], owner), "result");
                        break;
                    case "steerRockets":
                        r.Same(rec["result"], proj.SteerRockets(loadouts[(int)Num(a[0])], PortData.V3(a[1])), "result");
                        break;
                    case "killEnemy":
                        byId[(int)Num(a[0])].alive = false;
                        break;
                    case "moveEnemy":
                        byId[(int)Num(a[0])].position = PortData.V3(a[1]);
                        break;
                    case "clear":
                        proj.Clear();
                        break;
                    default:
                        r.Fail($"unknown op {op}");
                        return;
                }
                if (!rec.TryGetValue("after", out object afterO)) return;
                var after = PortData.Obj(afterO);
                SameCalls(r, PortData.Arr(after["calls"]), calls);
                var wf = PortData.Arr(after["fired"]);
                if (wf.Count != fired.Count) r.Fail($"fired: expected {wf.Count} got {fired.Count}");
                for (int k = 0; k < Math.Min(wf.Count, fired.Count); k++)
                {
                    var w = PortData.Obj(wf[k]);
                    Projectiles.Projectile p = fired[k];
                    string f = $"fired[{k}]";
                    r.SameBool(w["alive"], p.alive, f + ".alive");
                    r.SameJson(w["position"], V(p.position), f + ".position");
                    r.SameJson(w["previous"], V(p.previous), f + ".previous");
                    r.SameJson(w["velocity"], V(p.velocity), f + ".velocity");
                    r.SameJson(w["dir"], V(p.dir), f + ".dir");
                    r.Same(w["ageSteps"], p.ageSteps, f + ".ageSteps");
                    r.Same(w["pierce"], p.pierce, f + ".pierce");
                    r.Same(w["turned"], p.turned, f + ".turned");
                    r.SameBool(w["assisted"], p.assisted, f + ".assisted");
                    r.Same(w["shot"], p.shot, f + ".shot");
                    r.Same(w["radiusMult"], p.radiusMult, f + ".radiusMult");
                    r.Same(w["fragments"], p.fragments, f + ".fragments");
                }
                var ws = PortData.Arr(after["stuck"]);
                for (int k = 0; k < ws.Count; k++) r.Same(ws[k], proj.StuckCount(loadouts[k]), $"stuck[{k}]");
                var wc = PortData.Obj(after["count"]);
                r.Same(wc["nail"], proj.Count("nail"), "count.nail");
                r.Same(wc["rocket"], proj.Count("rocket"), "count.rocket");
                SameEnemies(r, PortData.Arr(after["enemies"]), byId);
            });
        }

        static void SameEnemies(GoldenResult r, List<object> want, Dictionary<int, FakeEnemy> byId)
        {
            foreach (object o in want)
            {
                var w = PortData.Obj(o);
                FakeEnemy e = byId[(int)Num(w["id"])];
                r.Same(w["hp"], e.hp, $"enemy {e.id}.hp");
                r.SameBool(w["alive"], e.alive, $"enemy {e.id}.alive");
            }
        }

        // --- weapons (the rail on the real loadout) -------------------------------------------------------------------

        internal static void CheckWeapons(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "weapons", (i, c) =>
            {
                if (!c.ContainsKey("session"))
                {
                    r.Fail("no check for this record");
                    return;
                }
                RailSession(data, c, r);
            });
        }

        static Button Btn(Dictionary<string, object> frame, string name)
        {
            if (!frame.TryGetValue(name, out object o) || !(o is Dictionary<string, object> b)) return default;
            return new Button { held = b.TryGetValue("held", out object h) && h is bool hb && hb, pressed = b.TryGetValue("pressed", out object p) && p is bool pb && pb };
        }

        static readonly HashSet<string> RailEvents = new HashSet<string> { "fire", "railCharge", "railCancel", "railFire", "shotResult", "hit", "kill", "overheat" };

        static void RailSession(GoldenData data, Dictionary<string, object> c, GoldenResult r)
        {
            GameConfig cfg = data.Config;
            double dt = cfg.Dt;
            var byId = new Dictionary<int, FakeEnemy>();
            EnemyRegistry enemies = FakeEnemies(c, byId);
            ICollisionWorld world = ArenaOf(c);
            var events = new EventBus();
            var mover = new Mover(cfg.movement, world);
            var ownerRec = PortData.Obj(c["owner"]);
            Vec3 start = PortData.V3(ownerRec["position"]);
            mover.Reset(start.x, start.y, start.z);
            Vec3 aimO = default, aimD = new Vec3(0, 0, -1);
            var loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "p1"), () => (aimO, aimD), (string)ownerRec["id"]);
            loadout.Reset(new List<string> { "rail" }, 1);
            loadout.arsenal.enabled = false; // createLoadout(..., {arsenal: false, momentum: true})
            var calls = new List<object>();
            events.On("fire", o => calls.Add(Ev("fire", new Dictionary<string, object> { ["weapon"] = (string)o })));
            events.On<RailChargeEvent>("railCharge", e => calls.Add(Ev("railCharge", new Dictionary<string, object> { ["over"] = e.over })));
            events.On("railCancel", _ => calls.Add(Ev("railCancel", new Dictionary<string, object>())));
            events.On<RailFireEvent>("railFire", e => calls.Add(Ev("railFire", new Dictionary<string, object> { ["from"] = e.from, ["to"] = e.to, ["hits"] = (double)e.hits })));
            events.On<ShotEvent>("shotResult", e => calls.Add(Ev("shotResult", new Dictionary<string, object> { ["weapon"] = e.weapon, ["hits"] = (double)e.hitCount, ["rayCount"] = (double)e.rayCount })));
            events.On<HitEvent>("hit", e => calls.Add(Ev("hit", new Dictionary<string, object>
            {
                ["target"] = (double)e.enemy, ["damage"] = e.damage, ["headshot"] = e.headshot, ["distance"] = e.distance, ["killed"] = e.killed, ["weapon"] = e.weapon, ["momentum"] = e.momentum,
            })));
            events.On<KillEvent>("kill", e => calls.Add(Ev("kill", new Dictionary<string, object> { ["target"] = (double)e.enemy, ["weapon"] = e.weapon })));
            events.On("overheat", _ => calls.Add(Ev("overheat", new Dictionary<string, object>())));
            EachOp(r, c, (string)c["session"], (op, a, rec) =>
            {
                calls.Clear();
                switch (op)
                {
                    case "aim":
                        aimO = PortData.V3(a[0]);
                        aimD = PortData.V3(a[1]);
                        break;
                    case "velocity":
                        mover.velocity = PortData.V3(a[0]);
                        break;
                    case "step":
                    {
                        var frame = PortData.Obj(a[0]);
                        var input = new CombatInput
                        {
                            fire = Btn(frame, "fire"), altFire = Btn(frame, "altFire"), special = Btn(frame, "special"), punch = Btn(frame, "punch"),
                            weapon = frame.TryGetValue("weapon", out object wv) ? (int)Num(wv) : -1,
                        };
                        loadout.Step(input, dt);
                        break;
                    }
                    default:
                        r.Fail($"unknown op {op}");
                        return;
                }
                if (!rec.TryGetValue("after", out object afterO)) return;
                var after = PortData.Obj(afterO);
                // The events this port emits under the web's names (the loadout's other events are compared elsewhere).
                var want = new List<object>();
                foreach (object o in PortData.Arr(after["calls"]))
                {
                    var w = PortData.Obj(o);
                    if (w["c"] as string == "event" && RailEvents.Contains((string)w["name"])) want.Add(RailCall(w));
                }
                SameRailCalls(r, want, calls);
                r.SameJson(after["velocity"], V(mover.velocity), "velocity");
                r.Same(after["railCharge"], loadout.RailCharge, "railCharge");
                r.Same(after["cooldown"], loadout.CooldownFractionOf("rail"), "cooldown");
                if (after.TryGetValue("variant", out object variant)) r.Same(variant, loadout.AimMomentum(), "variant");
                SameEnemies(r, PortData.Arr(after["enemies"]), byId);
            });
        }

        /// <summary>The recorded web event, reduced to the fields the port's payload carries.</summary>
        static Dictionary<string, object> RailCall(Dictionary<string, object> w)
        {
            string name = (string)w["name"];
            var p = PortData.Obj(w["payload"]);
            var o = new Dictionary<string, object>();
            switch (name)
            {
                case "fire":
                    o["weapon"] = p["weapon"];
                    break;
                case "railCharge":
                    o["over"] = p.TryGetValue("over", out object over) && over is bool ob && ob;
                    break;
                case "railFire":
                    o["from"] = p["from"];
                    o["to"] = p["to"];
                    o["hits"] = p["hits"];
                    break;
                case "shotResult":
                    o["weapon"] = p["weapon"];
                    o["hits"] = (double)PortData.Arr(p["hits"]).Count;
                    o["rayCount"] = p["rayCount"];
                    break;
                case "hit":
                    foreach (string k in new[] { "target", "damage", "headshot", "distance", "killed", "weapon", "momentum" }) o[k] = p[k];
                    break;
                case "kill":
                    o["target"] = p["target"];
                    o["weapon"] = p["weapon"];
                    break;
            }
            return Ev(name, o);
        }

        static void SameRailCalls(GoldenResult r, List<object> want, List<object> got)
        {
            if (want.Count != got.Count)
            {
                var g = new List<string>();
                foreach (object x in got) g.Add(Describe((Dictionary<string, object>)x));
                var w = new List<string>();
                foreach (object x in want) w.Add(Describe((Dictionary<string, object>)x));
                r.Fail($"events: expected {want.Count} [{string.Join(", ", w)}] got {got.Count} [{string.Join(", ", g)}]");
                return;
            }
            for (int i = 0; i < want.Count; i++)
            {
                var w = (Dictionary<string, object>)want[i];
                var gg = (Dictionary<string, object>)got[i];
                if (!r.SameStr(w["name"] as string, gg["name"] as string, $"events[{i}].name")) return;
                r.SameJson(w["payload"], gg["payload"], $"events[{i}] {w["name"]}");
            }
        }
    }
}
