// The combat oracle (NIGHT9 U3-04): every record of PortExport/vectors/combatMath.json replayed through CombatMath,
// numbers bit for bit. Moved here from CombatGoldenTests (one public method per test there) so the EditMode tests, the
// Player's -skygrindGolden and the dotnet harness run one evaluation. pickRicochetTarget's two cases inject a callback
// (README: "hasLOS = always true" and its text) and run with that rule. gunMath.json's sweepTargets is not this set.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddCombat(Dictionary<string, GoldenCheck> d)
        {
            d["combatMath"] = GoldenCombat.CheckSet;
        }
    }

    /// <summary>combatMath.json, in the groups CombatGoldenTests checks (each a public method), and the whole set.</summary>
    public static class GoldenCombat
    {
        public const string Set = "combatMath";

        static readonly string[] Handled =
        {
            "sampleCone", "raySphere", "rayVerticalCapsule", "leapVelocity", "swapLowered", "swapCanFire", "swapMirror",
            "pickPunchTarget", "punchKind", "pickRicochetTarget",
        };

        /// <summary>Every record: each group, then a mismatch for any record no group handles.</summary>
        public static void CheckSet(GoldenData data, GoldenResult r)
        {
            SampleCone(data, r);
            RaySphereAndCapsule(data, r);
            LeapVelocity(data, r);
            SwapTimeline(data, r);
            PunchPickAndKind(data, r);
            RicochetPick(data, r);
            GoldenWalk.Unhandled(data, r, Set, Handled);
        }

        public static void SampleCone(GoldenData data, GoldenResult r) =>
            GoldenWalk.Each(data, r, Set, "sampleCone", c =>
            {
                var a = GoldenWalk.Args(c);
                r.SameVec(c["result"], CombatMath.SampleCone(V(a[0]), N(a[1]), N(a[2]), N(a[3])), "result");
            });

        public static void RaySphereAndCapsule(GoldenData data, GoldenResult r)
        {
            GoldenWalk.Each(data, r, Set, "raySphere", c =>
            {
                var a = GoldenWalk.Args(c);
                r.Same(c["result"], CombatMath.RaySphere(V(a[0]), V(a[1]), V(a[2]), N(a[3])), "result");
            });
            GoldenWalk.Each(data, r, Set, "rayVerticalCapsule", c =>
            {
                var a = GoldenWalk.Args(c);
                r.Same(c["result"], CombatMath.RayVerticalCapsule(V(a[0]), V(a[1]), V(a[2]), N(a[3]), N(a[4])), "result");
            });
        }

        public static void LeapVelocity(GoldenData data, GoldenResult r) =>
            GoldenWalk.Each(data, r, Set, "leapVelocity", c =>
            {
                var a = GoldenWalk.Args(c);
                Vec3 v = CombatMath.LeapVelocity(V(a[0]), V(a[1]), N(a[2]), N(a[3]), out double time);
                r.SameVec(c["result"], v, "result");
                r.Same(PortData.Obj(c["result"])["time"], time, "result.time");
            });

        public static void SwapTimeline(GoldenData data, GoldenResult r)
        {
            GoldenWalk.Each(data, r, Set, "swapLowered", c =>
            {
                var a = GoldenWalk.Args(c);
                r.Same(c["result"], CombatMath.SwapLowered(N(a[0]), N(a[1]), N(a[2])), "result");
            });
            GoldenWalk.Each(data, r, Set, "swapCanFire", c =>
            {
                var a = GoldenWalk.Args(c);
                r.SameBool(c["result"], CombatMath.SwapCanFire(N(a[0]), N(a[1]), N(a[2]), N(a[3])), "result");
            });
            GoldenWalk.Each(data, r, Set, "swapMirror", c =>
            {
                var a = GoldenWalk.Args(c);
                r.Same(c["result"], CombatMath.SwapMirror(N(a[0]), N(a[1]), N(a[2])), "result");
            });
        }

        public static void PunchPickAndKind(GoldenData data, GoldenResult r)
        {
            GoldenWalk.Each(data, r, Set, "pickPunchTarget", c =>
            {
                var a = GoldenWalk.Args(c);
                TargetPick? pick = CombatMath.PickPunchTarget(V(a[0]), V(a[1]), Targets(a[2]), N(a[3]), N(a[4]));
                if (c["result"] == null)
                {
                    if (pick.HasValue) r.Fail($"result: expected null got target {PortData.Describe(pick.Value.target.enemy)}");
                    return;
                }
                if (!pick.HasValue)
                {
                    r.Fail("result: expected a target got null");
                    return;
                }
                var res = PortData.Obj(c["result"]);
                r.SameJson(PortData.Obj(res["target"])["id"], pick.Value.target.enemy, "result.target.id");
                r.Same(res["distance"], pick.Value.distance, "result.distance");
            });
            GoldenWalk.Each(data, r, Set, "punchKind", c =>
                r.SameStr(c["result"], CombatMath.IsParry((int)N(GoldenWalk.Args(c)[0])) ? "parry" : "hit", "result"));
        }

        /// <summary>pickRicochetTarget with the recorded line-of-sight rule (a record with a rule this checker can't run is a mismatch).</summary>
        public static void RicochetPick(GoldenData data, GoldenResult r) =>
            GoldenWalk.Each(data, r, Set, "pickRicochetTarget", c =>
            {
                var a = GoldenWalk.Args(c);
                var list = PortData.Arr(a[1]).ConvertAll(PortData.Obj);
                string rule = (string)PortData.Obj(PortData.Obj(a[3])["$fn"])["$callback"];
                Func<Vec3, Vec3, bool> los;
                if (rule.Contains("always true")) los = (x, y) => true;
                else if (rule.Contains("to.x > 0")) los = (x, y) => y.x > 0;
                else
                {
                    r.Fail($"unknown recorded rule: {rule}");
                    return;
                }
                int i = CombatMath.PickRicochetTarget(V(a[0]), list.ConvertAll(p => V(p["point"])), N(a[2]), los);
                object want = c["result"] == null ? null : PortData.Obj(c["result"])["id"];
                r.SameJson(want, i < 0 ? null : list[i]["id"], $"result.id ({rule})");
            });

        /// <summary>The recorded body targets ({id, base, height, radius, windupSteps?}).</summary>
        public static List<BodyTarget> Targets(object o) => PortData.Arr(o).Select(t =>
        {
            var d = PortData.Obj(t);
            return new BodyTarget { enemy = d["id"], basePos = V(d["base"]), height = N(d["height"]), radius = N(d["radius"]), windupSteps = d.TryGetValue("windupSteps", out object w) ? (int)N(w) : 0 };
        }).ToList();

        static double N(object o) => PortData.Num(o);

        static Vec3 V(object o) => PortData.V3(o);
    }
}
