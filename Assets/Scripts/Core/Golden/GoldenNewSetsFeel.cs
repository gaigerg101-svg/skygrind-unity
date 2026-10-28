// NIGHT9 U3-07 checkers (see GoldenNewSets.cs): deathRecap and hitDirection (DeathRecap.cs), kicks (KickStack.cs),
// springs (CombatMath.cs SpringShape / SpringBank).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    static partial class GoldenNewSets
    {
        // --- deathRecap (DeathRecap.cs) ----------------------------------------------------------------------------

        static Dictionary<string, string> StrDict(GoldenData data, object o)
        {
            var d = new Dictionary<string, string>();
            foreach (var kv in Obj(data, o)) d[kv.Key] = (string)kv.Value;
            return d;
        }

        static void CheckDeathRecap(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "deathRecap", (i, c) =>
            {
                if (c.ContainsKey("session"))
                {
                    RecapSession(c, r);
                    return;
                }
                var a = Args(c);
                if (Fn(c) != "sourceName")
                {
                    r.Fail("no check for this record");
                    return;
                }
                DeathRecap.HurtSource source = null;
                if (a[0] is Dictionary<string, object> so && !IsUndefined(so))
                {
                    source = new DeathRecap.HurtSource
                    {
                        type = Field(data, so, "type") as string, boss = Field(data, so, "boss") as string, attack = Field(data, so, "attack") as string,
                    };
                }
                var labels = PortData.Obj(a[1]);
                r.SameStr(c["result"], DeathRecap.SourceName(source, StrDict(data, labels["names"]), StrDict(data, labels["bossNames"])), "result");
            });
        }

        static void RecapSession(Dictionary<string, object> c, GoldenResult r)
        {
            var recap = new DeathRecap(Num(Args(c)[0]));
            EachOp(r, c, "", (op, a, rec) =>
            {
                switch (op)
                {
                    case "hurt":
                        recap.Hurt(Num(a[0]), Num(a[1]), (string)a[2]);
                        break;
                    case "clear":
                        recap.Clear();
                        break;
                    case "summary":
                    {
                        DeathRecap.Summary s = recap.At(Num(a[0]));
                        var parts = new List<object>();
                        foreach (DeathRecap.Part p in s.parts) parts.Add(new Dictionary<string, object> { ["name"] = p.name, ["amount"] = p.amount });
                        r.SameJson(rec["result"], new Dictionary<string, object> { ["killer"] = s.killer, ["total"] = s.total, ["parts"] = parts }, "summary");
                        break;
                    }
                    default:
                        r.Fail("unknown op");
                        break;
                }
            });
        }

        // --- hitDirection (DeathRecap.cs HitArcs) ------------------------------------------------------------------

        static void CheckHitDirection(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "hitDirection", (i, c) =>
            {
                var a = Args(c);
                if (c.ContainsKey("session"))
                {
                    var pool = new HitArcs(Int(a[0]));
                    EachOp(r, c, "", (op, oa, rec) =>
                    {
                        if (op == "add")
                        {
                            if (oa.Count > 2) pool.Add(Num(oa[0]), Num(oa[1]), Num(oa[2]));
                            else pool.Add(Num(oa[0]), Num(oa[1]));
                        }
                        else if (op == "step") pool.Step(Num(oa[0]), Num(oa[1]));
                        else if (op == "clear") pool.Clear();
                        else
                        {
                            r.Fail("unknown op");
                            return;
                        }
                        var arcs = new List<object>();
                        foreach (HitArcs.Arc arc in pool.arcs) arcs.Add(new Dictionary<string, object> { ["x"] = arc.x, ["z"] = arc.z, ["age"] = arc.age });
                        r.SameJson(rec["after"], arcs, "arcs");
                    });
                    return;
                }
                if (Fn(c) == "hitAngle") r.Same(c["result"], HitArcs.HitAngle(Num(a[0]), Num(a[1]), Num(a[2]), Num(a[3]), Num(a[4])), "result");
                else r.Fail("no check for this record");
            });
        }

        // --- kicks (KickStack.cs) ----------------------------------------------------------------------------------

        static void CheckKicks(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "kicks", (i, c) =>
            {
                var a = Args(c);
                if (c.ContainsKey("session"))
                {
                    var stack = new KickStack(Int(a[0]));
                    EachOp(r, c, "", (op, oa, rec) =>
                    {
                        if (op == "add") stack.Add(Num(oa[0]), Num(oa[1]), Num(oa[2]));
                        else if (op == "advance") stack.Advance(Num(oa[0]));
                        else if (op == "clear") stack.Clear();
                        else
                        {
                            r.Fail("unknown op");
                            return;
                        }
                        r.SameJson(rec["after"], new Dictionary<string, object> { ["active"] = (double)stack.Active, ["value"] = stack.Value() }, "after");
                    });
                    return;
                }
                if (Fn(c) == "kickCurve") r.Same(c["result"], KickStack.KickCurve(Num(a[0]), Num(a[1]), Num(a[2])), "result");
                else r.Fail("no check for this record");
            });
        }

        // --- springs (CombatMath.cs SpringShape, SpringBank) -------------------------------------------------------

        static SpringShape ShapeOf(object o)
        {
            var d = PortData.Obj(o);
            double F(string k) => d.TryGetValue(k, out object v) ? Num(v) : 0;
            return new SpringShape { freqHz = F("freqHz"), zeta = F("zeta"), w = F("w"), a = F("a"), wd = F("wd"), tPeak = F("tPeak"), peakPerA = F("peakPerA"), overshoot = F("overshoot") };
        }

        static Dictionary<string, object> ShapeJson(SpringShape s) => new Dictionary<string, object>
        {
            ["freqHz"] = s.freqHz, ["zeta"] = s.zeta, ["w"] = s.w, ["a"] = s.a, ["wd"] = s.wd, ["tPeak"] = s.tPeak, ["peakPerA"] = s.peakPerA, ["overshoot"] = s.overshoot,
        };

        static void CheckSprings(GoldenData data, GoldenResult r)
        {
            EachRecord(data, r, "springs", (i, c) =>
            {
                var a = Args(c);
                if (c.ContainsKey("session"))
                {
                    double[] snaps = Nums(data, a[1]).ToArray();
                    var bank = new SpringBank(Int(a[0]), snaps);
                    EachOp(r, c, "", (op, oa, rec) =>
                    {
                        if (op == "add") bank.Add(ShapeOf(oa[0]), Nums(data, oa[1]).ToArray());
                        else if (op == "advance") bank.Update(Num(oa[0]));
                        else if (op == "clear") bank.Clear();
                        else
                        {
                            r.Fail("unknown op");
                            return;
                        }
                        var values = new List<object>();
                        var velocities = new List<object>();
                        for (int ch = 0; ch < snaps.Length; ch++)
                        {
                            values.Add(bank.Sample(ch));
                            velocities.Add(bank.Velocity(ch));
                        }
                        r.SameJson(rec["after"], new Dictionary<string, object> { ["active"] = (double)bank.Active, ["value"] = values, ["velocity"] = velocities }, "after");
                    });
                    return;
                }
                switch (Fn(c))
                {
                    case "springShape":
                        r.SameJson(c["result"], ShapeJson(SpringShape.Of(Num(a[0]), Num(a[1]))), "result");
                        break;
                    case "springValue":
                        r.Same(c["result"], ShapeOf(a[1]).Value(Num(a[0]), Num(a[2])), "result");
                        break;
                    case "springVelocity":
                        r.Same(c["result"], ShapeOf(a[1]).Velocity(Num(a[0]), Num(a[2])), "result");
                        break;
                    case "settleTime":
                        r.Same(c["result"], SpringShape.SettleTime(Num(a[0]), ShapeOf(a[1]), Num(a[2])), "result");
                        break;
                    case "softCap":
                        r.Same(c["result"], a.Count > 2 ? SpringShape.SoftCap(Num(a[0]), Num(a[1]), Num(a[2])) : SpringShape.SoftCap(Num(a[0]), Num(a[1])), "result");
                        break;
                    default:
                        r.Fail("no check for this record");
                        break;
                }
            });
        }
    }
}
