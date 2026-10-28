// The AI oracles (NIGHT9 U3-04): every record of PortExport/vectors/directorMath.json (replayed through DirectorMath)
// and assistMath.json (through AssistMath), numbers bit for bit (results, and the mutated arguments where recorded).
// Moved here from DirectorGoldenTests and AssistGoldenTests (one public method per test there) so the EditMode tests,
// the Player's -skygrindGolden and the dotnet harness run one evaluation. The whole-set checkers also walk the records
// those tests never read: createDirectorState, the stepDirector and stepCamp sequences and the PHASES constant.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddAi(Dictionary<string, GoldenCheck> d)
        {
            d["directorMath"] = GoldenDirectorMath.CheckSet;
            d["assistMath"] = GoldenAssistMath.CheckSet;
        }
    }

    /// <summary>directorMath.json, in the groups DirectorGoldenTests checks (each a public method), and the whole set.</summary>
    public static class GoldenDirectorMath
    {
        public const string Set = "directorMath";

        static readonly string[] Handled =
        {
            "roundBudget", "roundHpMult", "isBossRound", "isSwarmRound", "roundKinds",
            "phaseSpawnMult", "outOfView", "behind",
            "planSquads",
            "createDirectorState", "sequence:stepDirector", "sequence:stepCamp",
            "constant:PHASES",
        };

        /// <summary>Every record: each group, then a mismatch for any record no group handles.</summary>
        public static void CheckSet(GoldenData data, GoldenResult r)
        {
            Rounds(data, r);
            PhaseMultOutOfViewBehind(data, r);
            PlanSquads(data, r);
            StateAndSteps(data, r);
            Constants(data, r);
            GoldenWalk.Unhandled(data, r, Set, Handled);
        }

        /// <summary>config.json's director section (the {"$config": "director" / "director.rounds" / "director.camp"} arguments).</summary>
        public static DirectorMath.D Params(GoldenData data) => DirectorMath.From(data.Config);

        public static void Rounds(GoldenData data, GoldenResult r)
        {
            DirectorMath.D d = Params(data);
            GoldenWalk.Each(data, r, Set, "roundBudget", c => r.Same(c["result"], DirectorMath.RoundBudget(Round(c), d), "result"));
            GoldenWalk.Each(data, r, Set, "roundHpMult", c => r.Same(c["result"], DirectorMath.RoundHpMult(Round(c), d), "result"));
            GoldenWalk.Each(data, r, Set, "isBossRound", c => r.SameBool(c["result"], DirectorMath.IsBossRound(Round(c), d), "result"));
            GoldenWalk.Each(data, r, Set, "isSwarmRound", c => r.SameBool(c["result"], DirectorMath.IsSwarmRound(Round(c), d), "result"));
            GoldenWalk.Each(data, r, Set, "roundKinds", c =>
                r.SameJson(c["result"], DirectorMath.RoundKinds(Round(c), d).ToDictionary(kv => kv.Key, kv => (object)kv.Value), "result"));
        }

        public static void PhaseMultOutOfViewBehind(GoldenData data, GoldenResult r)
        {
            DirectorMath.D d = Params(data);
            GoldenWalk.Each(data, r, Set, "phaseSpawnMult", c => r.Same(c["result"], DirectorMath.PhaseSpawnMult((string)GoldenWalk.Args(c)[0], d), "result"));
            GoldenWalk.Each(data, r, Set, "outOfView", c =>
            {
                var a = GoldenWalk.Args(c);
                r.SameBool(c["result"], DirectorMath.OutOfView(N(a[0]), N(a[1]), N(a[2]), N(a[3]), N(a[4]), N(a[5]), (bool)a[6]), "result");
            });
            GoldenWalk.Each(data, r, Set, "behind", c =>
            {
                var a = GoldenWalk.Args(c);
                r.SameBool(c["result"], DirectorMath.Behind(N(a[0]), N(a[1]), N(a[2]), N(a[3]), N(a[4])), "result");
            });
        }

        /// <summary>
        /// planSquads with the recorded mulberry32 stream (and the number of draws the web took, where recorded) and the
        /// recorded availability: every kind unless the 4th argument is a `kind === "x"` / `kind !== "x"` callback.
        /// </summary>
        public static void PlanSquads(GoldenData data, GoldenResult r)
        {
            DirectorMath.D d = Params(data);
            GoldenWalk.Each(data, r, Set, "planSquads", c =>
            {
                var a = GoldenWalk.Args(c);
                int round = (int)N(a[0]);
                var rng = new Mulberry32(N(PortData.Obj(PortData.Obj(a[1])["$fn"])["seed"]));
                int draws = 0;
                Func<double> next = () =>
                {
                    draws++;
                    return rng.Next();
                };
                Func<string, bool> available = k => true;
                if (a.Count > 3 && a[3] is Dictionary<string, object> av && av.TryGetValue("$fn", out object spec))
                {
                    string text = (string)PortData.Obj(spec)["$callback"];
                    var m = Regex.Match(text, "kind (===|!==) \"(\\w+)\"");
                    if (!m.Success)
                    {
                        r.Fail($"unknown recorded availability: {text}");
                        return;
                    }
                    string kindName = m.Groups[2].Value;
                    available = m.Groups[1].Value == "===" ? (Func<string, bool>)(k => k == kindName) : k => k != kindName;
                }
                r.SameJson(c["result"], DirectorMath.PlanSquads(round, next, d, available), $"planSquads({round})");
                if (c.TryGetValue("argsAfter", out object after))
                {
                    var rngAfter = PortData.Obj(PortData.Obj(PortData.Arr(after)[1])["$fn"]);
                    if (rngAfter.TryGetValue("draws", out object want)) r.Same(want, draws, "argsAfter.rng.draws");
                }
            });
        }

        /// <summary>createDirectorState, and the stepDirector and stepCamp sequences (each from a fresh state, one step of the file's dt per row).</summary>
        public static void StateAndSteps(GoldenData data, GoldenResult r)
        {
            DirectorMath.D d = Params(data);
            double dt = N(data.Vectors(Set)["dt"]);
            GoldenWalk.Each(data, r, Set, "createDirectorState", c => r.SameJson(c["result"], StateJson(new DirectorState()), "result"));
            // columns [step, damageTaken, nearbyKills, aggression, intensity, phase, phaseTime, relaxTime, returned]; one
            // mulberry32 stream across the steps, its seed in the record's note
            GoldenWalk.Each(data, r, Set, "sequence:stepDirector", c =>
            {
                if (!DirectorRef(r, c)) return;
                var seed = Regex.Match((string)c["note"], "mulberry32\\((\\d+)\\)");
                if (!seed.Success)
                {
                    r.Fail("no mulberry32(seed) in the note");
                    return;
                }
                var rng = new Mulberry32(double.Parse(seed.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture));
                Func<double> next = rng.Next;
                var s = new DirectorState();
                var rows = PortData.Arr(c["rows"]).ConvertAll(PortData.Arr);
                for (int i = 0; i < rows.Count; i++)
                {
                    int before = r.mismatches.Count;
                    var row = rows[i];
                    string returned = DirectorMath.StepDirector(s, dt, N(row[1]), N(row[2]), N(row[3]), next, d);
                    string at = $"row {N(row[0])}";
                    r.Same(row[4], s.intensity, at + " intensity");
                    r.SameStr(row[5], s.phase, at + " phase");
                    r.Same(row[6], s.phaseTime, at + " phaseTime");
                    r.Same(row[7], s.relaxTime, at + " relaxTime");
                    r.SameStr(row[8], returned, at + " returned");
                    if (r.mismatches.Count > before) break;
                }
            });
            // columns [step, x, z, enemiesAlive, dealtRecent, killChain, camp, lingerTime, exposed, hunted, startedEdge,
            // endedEdge, huntedEdge]; config.director.camp
            GoldenWalk.Each(data, r, Set, "sequence:stepCamp", c =>
            {
                var s = new DirectorState();
                var rows = PortData.Arr(c["rows"]).ConvertAll(PortData.Arr);
                for (int i = 0; i < rows.Count; i++)
                {
                    int before = r.mismatches.Count;
                    var row = rows[i];
                    var (started, ended, hunted) = DirectorMath.StepCamp(s, dt, N(row[1]), N(row[2]), (int)N(row[3]), N(row[4]), (bool)row[5], d);
                    string at = $"row {N(row[0])}";
                    r.Same(row[6], s.camp, at + " camp");
                    r.Same(row[7], s.lingerTime, at + " lingerTime");
                    r.SameBool(row[8], s.exposed, at + " exposed");
                    r.SameBool(row[9], s.hunted, at + " hunted");
                    r.SameBool(row[10], started, at + " startedEdge");
                    r.SameBool(row[11], ended, at + " endedEdge");
                    r.SameBool(row[12], hunted, at + " huntedEdge");
                    if (r.mismatches.Count > before) break;
                }
            });
        }

        /// <summary>The recorded constants. PHASES has no C# port (DirectorMath has no phase list), so it is a mismatch until one exists.</summary>
        public static void Constants(GoldenData data, GoldenResult r) =>
            GoldenWalk.Each(data, r, Set, "constant:PHASES", c => r.SameJson(c["value"], DirectorMath.Phases, "value", true));

        static int Round(Dictionary<string, object> c) => (int)N(GoldenWalk.Args(c)[0]);

        static double N(object o) => PortData.Num(o);

        static bool DirectorRef(GoldenResult r, Dictionary<string, object> c)
        {
            if (!c.TryGetValue("d", out object o) || !(o is Dictionary<string, object> d) || !d.TryGetValue("$config", out object p) || (string)p != "director")
            {
                r.Fail($"unsupported director config {PortData.Describe(o)}");
                return false;
            }
            return true;
        }

        /// <summary>A DirectorState as the web's createDirectorState object (anchor / home are {x, z} or null).</summary>
        static Dictionary<string, object> StateJson(DirectorState s) => new Dictionary<string, object>
        {
            ["intensity"] = s.intensity, ["phase"] = s.phase, ["phaseTime"] = s.phaseTime, ["relaxTime"] = s.relaxTime, ["camp"] = s.camp,
            ["anchor"] = XZ(s.anchor), ["home"] = XZ(s.home), ["lingerTime"] = s.lingerTime, ["hunted"] = s.hunted, ["exposed"] = s.exposed,
        };

        static object XZ(Vec3? v) => v.HasValue ? new Dictionary<string, object> { ["x"] = v.Value.x, ["z"] = v.Value.z } : null;
    }

    /// <summary>assistMath.json, in the groups AssistGoldenTests checks (each a public method), and the whole set.</summary>
    public static class GoldenAssistMath
    {
        public const string Set = "assistMath";

        static readonly string[] Handled =
        {
            "lerp", "angleTo", "wrapAngle", "yawPitchTo", "lookDir", "lookError", "leadPoint",
            "lockParams", "lockPayout", "assistPayout",
            "lockScore", "pickBest", "challenge", "nextLock",
            "springStep", "sequence:springStep", "fireReady", "breaksAway", "homeVelocity",
        };

        /// <summary>Every record: each group, then a mismatch for any record no group handles.</summary>
        public static void CheckSet(GoldenData data, GoldenResult r)
        {
            ScalarsAndAngles(data, r);
            LockParamsAndPayouts(data, r);
            ScoresPicksAndHysteresis(data, r);
            SpringGateBreakawayHoming(data, r);
            GoldenWalk.Unhandled(data, r, Set, Handled);
        }

        /// <summary>config.json's aim.lock (the {"$config": "aim.lock" / "aim.lock.weights" / "aim.lock.fire"} arguments).</summary>
        public static AssistMath.L Params(GoldenData data) => AssistMath.From(data.Config);

        public static void ScalarsAndAngles(GoldenData data, GoldenResult r)
        {
            GoldenWalk.Each(data, r, Set, "lerp", c =>
            {
                var a = GoldenWalk.Args(c);
                r.Same(c["result"], AssistMath.Lerp(N(a[0]), N(a[1]), N(a[2])), "result");
            });
            GoldenWalk.Each(data, r, Set, "angleTo", c =>
            {
                var a = GoldenWalk.Args(c);
                r.Same(c["result"], AssistMath.AngleTo(V(a[0]), V(a[1]), V(a[2])), "result");
            });
            GoldenWalk.Each(data, r, Set, "wrapAngle", c => r.Same(c["result"], AssistMath.WrapAngle(N(GoldenWalk.Args(c)[0])), "result"));
            GoldenWalk.Each(data, r, Set, "yawPitchTo", c =>
            {
                var a = GoldenWalk.Args(c);
                var res = PortData.Obj(c["result"]);
                var (yaw, pitch) = AssistMath.YawPitchTo(V(a[0]), V(a[1]));
                r.Same(res["yaw"], yaw, "result.yaw");
                r.Same(res["pitch"], pitch, "result.pitch");
            });
            GoldenWalk.Each(data, r, Set, "lookDir", c =>
            {
                var a = GoldenWalk.Args(c);
                r.SameVec(c["result"], AssistMath.LookDir(N(a[0]), N(a[1])), "result");
            });
            GoldenWalk.Each(data, r, Set, "lookError", c =>
            {
                var a = GoldenWalk.Args(c);
                r.Same(c["result"], AssistMath.LookError(N(a[0]), N(a[1]), N(a[2]), N(a[3])), "result");
            });
            GoldenWalk.Each(data, r, Set, "leadPoint", c =>
            {
                var a = GoldenWalk.Args(c);
                r.SameVec(c["result"], AssistMath.LeadPoint(V(a[0]), V(a[1]), V(a[2]), N(a[3])), "result");
            });
        }

        public static void LockParamsAndPayouts(GoldenData data, GoldenResult r)
        {
            AssistMath.L lk = Params(data);
            GoldenWalk.Each(data, r, Set, "lockParams", c =>
            {
                var a = GoldenWalk.Args(c);
                AssistMath.Params p = AssistMath.LockParams((string)a[0], N(a[1]), (bool)a[2], lk);
                if (c["result"] == null)
                {
                    if (p != null) r.Fail("result: expected null got params");
                    return;
                }
                if (p == null)
                {
                    r.Fail("result: expected params got null");
                    return;
                }
                var w = PortData.Obj(c["result"]);
                r.Same(w["cone"], p.cone, "result.cone");
                r.Same(w["acquireTime"], p.acquireTime, "result.acquireTime");
                r.Same(w["maxPull"], p.maxPull, "result.maxPull");
                r.Same(w["omega"], p.omega, "result.omega");
                r.Same(w["follow"], p.follow, "result.follow");
                r.Same(w["friction"], p.friction, "result.friction");
                r.Same(w["breakaway"], p.breakaway, "result.breakaway");
                r.Same(w["pulse"], p.pulse, "result.pulse");
                r.Same(w["k"], p.k, "result.k");
                r.SameBool(w["track"], p.track, "result.track");
            });
            GoldenWalk.Each(data, r, Set, "lockPayout", c => r.Same(c["result"], AssistMath.LockPayout(N(GoldenWalk.Args(c)[0]), lk), "result"));
            GoldenWalk.Each(data, r, Set, "assistPayout", c =>
            {
                var a = GoldenWalk.Args(c);
                r.Same(c["result"], AssistMath.AssistPayout(N(a[0]), (bool)a[1], N(a[2])), "result");
            });
        }

        /// <summary>lockScore, pickBest (the one recorded skip callback, `c.id === 1`), the challenge hysteresis, nextLock.</summary>
        public static void ScoresPicksAndHysteresis(GoldenData data, GoldenResult r)
        {
            AssistMath.Weights weights = Params(data).weights;
            GoldenWalk.Each(data, r, Set, "lockScore", c =>
            {
                var a = GoldenWalk.Args(c);
                r.Same(c["result"], AssistMath.LockScore(Candidate(a[0]), N(a[1]), N(a[2]), weights), "result");
            });
            GoldenWalk.Each(data, r, Set, "pickBest", c =>
            {
                var a = GoldenWalk.Args(c);
                var list = PortData.Arr(a[0]).ConvertAll(Candidate);
                Func<AssistMath.Candidate, bool> skip = null;
                if (a.Count > 4)
                {
                    string cb = (string)PortData.Obj(PortData.Obj(a[4])["$fn"])["$callback"];
                    if (cb != "skip(c) = c.id === 1")
                    {
                        r.Fail($"unknown recorded callback: {cb}");
                        return;
                    }
                    skip = x => Equals(x.id, 1.0);
                }
                AssistMath.Candidate best = AssistMath.PickBest(list, N(a[1]), N(a[2]), weights, skip);
                SameId(r, c["result"], best);
            });
            GoldenWalk.Each(data, r, Set, "challenge", c =>
            {
                var a = GoldenWalk.Args(c);
                var s0 = PortData.Obj(a[0]);
                var state = new AssistMath.Rival { id = Id(s0["id"]), t = N(s0["t"]) };
                r.SameBool(c["result"], AssistMath.Challenge(state, N(a[1]), Id(a[2]), N(a[3]), N(a[4]), N(a[5]), N(a[6])), "result");
                var after = PortData.Obj(PortData.Arr(c["argsAfter"])[0]);
                r.SameJson(Id(after["id"]), state.id, "argsAfter.state.id");
                r.Same(after["t"], state.t, "argsAfter.state.t");
            });
            GoldenWalk.Each(data, r, Set, "nextLock", c =>
            {
                var a = GoldenWalk.Args(c);
                SameId(r, c["result"], AssistMath.NextLock(PortData.Arr(a[0]).ConvertAll(Candidate), Id(a[1])));
            });
        }

        /// <summary>springStep (the calls, and the recorded sequence: SNAP's omega from x = 10, v = 0), fireReady, breaksAway, homeVelocity.</summary>
        public static void SpringGateBreakawayHoming(GoldenData data, GoldenResult r)
        {
            AssistMath.L lk = Params(data);
            GoldenWalk.Each(data, r, Set, "springStep", c =>
            {
                var a = GoldenWalk.Args(c);
                var res = PortData.Obj(c["result"]);
                var (x, v) = AssistMath.SpringStep(N(a[0]), N(a[1]), N(a[2]), N(a[3]));
                r.Same(res["x"], x, "result.x");
                r.Same(res["v"], v, "result.v");
            });
            GoldenWalk.Each(data, r, Set, "sequence:springStep", c =>
            {
                double dt = N(data.Vectors(Set)["dt"]), omega = lk.modes["snap"].omega, sx = 10, sv = 0;
                foreach (object o in PortData.Arr(c["rows"]))
                {
                    int before = r.mismatches.Count;
                    var row = PortData.Arr(o);
                    (sx, sv) = AssistMath.SpringStep(sx, sv, omega, dt);
                    r.Same(row[1], sx, $"row {N(row[0])} x");
                    r.Same(row[2], sv, $"row {N(row[0])} v");
                    if (r.mismatches.Count > before) break;
                }
            });
            GoldenWalk.Each(data, r, Set, "fireReady", c =>
            {
                var a = GoldenWalk.Args(c);
                r.SameBool(c["result"], AssistMath.FireReady((bool)a[0], N(a[1]), N(a[2]), N(a[3]), lk.fire), "result");
            });
            GoldenWalk.Each(data, r, Set, "breaksAway", c =>
            {
                var a = GoldenWalk.Args(c);
                bool got = a.Count >= 5
                    ? AssistMath.BreaksAway(N(a[0]), N(a[1]), N(a[2]), N(a[3]), N(a[4]))
                    : a.Count == 4 ? AssistMath.BreaksAway(N(a[0]), N(a[1]), N(a[2]), N(a[3])) : AssistMath.BreaksAway(N(a[0]), N(a[1]), N(a[2]));
                r.SameBool(c["result"], got, "result");
            });
            GoldenWalk.Each(data, r, Set, "homeVelocity", c =>
            {
                var a = GoldenWalk.Args(c);
                Vec3 vel = V(a[0]);
                r.Same(c["result"], AssistMath.HomeVelocity(ref vel, V(a[1]), V(a[2]), N(a[3])), "result");
                if (c.TryGetValue("argsAfter", out object after)) r.SameVec(PortData.Arr(after)[0], vel, "argsAfter.vel");
            });
        }

        // --- decoding -----------------------------------------------------------------------------------------------

        static double N(object o) => PortData.Num(o);

        static Vec3 V(object o) => PortData.V3(o);

        /// <summary>A recorded candidate / rival id: a string, a number, or null.</summary>
        public static object Id(object o) => o is string s ? (object)s : o == null ? null : (object)N(o);

        public static AssistMath.Candidate Candidate(object o)
        {
            var d = PortData.Obj(o);
            return new AssistMath.Candidate
            {
                id = d.TryGetValue("id", out object id) ? Id(id) : null,
                angle = N(d["angle"]),
                distance = d.TryGetValue("distance", out object di) ? N(di) : 0,
                threat = d.TryGetValue("threat", out object th) ? N(th) : 0,
                head = d.TryGetValue("head", out object h) && h is bool hb && hb,
                weak = d.TryGetValue("weak", out object w) && w is bool wb && wb,
            };
        }

        /// <summary>A picked candidate against the recorded one (null, or the candidate object: compared by id).</summary>
        static void SameId(GoldenResult r, object want, AssistMath.Candidate got)
        {
            if (want == null)
            {
                if (got != null) r.Fail($"result: expected null got id {PortData.Describe(got.id)}");
                return;
            }
            if (got == null)
            {
                r.Fail($"result: expected id {PortData.Describe(PortData.Obj(want)["id"])} got null");
                return;
            }
            r.SameJson(Id(PortData.Obj(want)["id"]), got.id, "result.id");
        }
    }
}
