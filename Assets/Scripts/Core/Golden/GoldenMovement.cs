// The movement oracle (NIGHT9 U3-04): every record of PortExport/vectors/movementMath.json and swingMath.json (calls,
// the recorded sequences, the MoveState constant) replayed through MovementMath / SwingMath, numbers bit for bit. Moved
// here from GoldenVectorTests so the EditMode tests, the Player's -skygrindGolden and the dotnet harness run one
// evaluation. GoldenWalk (the record walker every checker of NIGHT9 U3-04 shares) lives here too.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddMovement(Dictionary<string, GoldenCheck> d)
        {
            d["movementMath"] = (data, r) => GoldenMovement.CheckSet(data, r, "movementMath");
            d["swingMath"] = (data, r) => GoldenMovement.CheckSet(data, r, "swingMath");
        }
    }

    /// <summary>Walking a vector file's records: their keys, one key's records, the keys no checker handles.</summary>
    public static class GoldenWalk
    {
        /// <summary>The key a record is filed under: its fn, else "sequence:name", "constant:name" or "session:name".</summary>
        public static string Key(Dictionary<string, object> d)
        {
            if (d.TryGetValue("fn", out object f) && f is string fn) return fn;
            if (d.TryGetValue("sequence", out object s) && s is string sequence) return "sequence:" + sequence;
            if (d.TryGetValue("constant", out object c) && c is string constant) return "constant:" + constant;
            if (d.TryGetValue("session", out object x) && x is string session) return "session:" + session;
            return "?";
        }

        /// <summary>The distinct keys of a set, in file order.</summary>
        public static List<string> Keys(GoldenData data, string set)
        {
            var keys = new List<string>();
            foreach (var d in data.Cases(set))
            {
                string k = Key(d);
                if (!keys.Contains(k)) keys.Add(k);
            }
            return keys;
        }

        /// <summary>
        /// Check every record filed under `key`, in file order: r.where = "set.key #i" (i counts that key's records), one
        /// r.cases each, an exception a mismatch of that record only. A key with no record is a mismatch (the checker
        /// expects it).
        /// </summary>
        public static void Each(GoldenData data, GoldenResult r, string set, string key, Action<Dictionary<string, object>> check)
        {
            var cases = data.Cases(set).FindAll(d => Key(d) == key);
            if (cases.Count == 0)
            {
                r.where = "";
                r.Fail($"{set}.{key}: no records");
                return;
            }
            for (int i = 0; i < cases.Count; i++)
            {
                r.where = $"{set}.{key} #{i}";
                try
                {
                    check(cases[i]);
                }
                catch (Exception e)
                {
                    r.Fail($"exception: {e.GetType().Name}: {e.Message}");
                }
                r.cases++;
            }
            r.where = "";
        }

        /// <summary>The records whose key none of the set's checkers handles: each key is a mismatch (every record is checked).</summary>
        public static void Unhandled(GoldenData data, GoldenResult r, string set, ICollection<string> handled)
        {
            r.where = "";
            foreach (string key in Keys(data, set))
                if (!handled.Contains(key)) r.Fail($"{set}: no checker for the {key} records");
        }

        public static List<object> Args(Dictionary<string, object> c) => PortData.Arr(c["args"]);

        /// <summary>A mutated argument as recorded after the call (argsAfter), else as passed.</summary>
        public static List<object> After(Dictionary<string, object> c) => PortData.Arr(c.TryGetValue("argsAfter", out object a) ? a : c["args"]);
    }

    /// <summary>movementMath.json and swingMath.json: one checker per record key (GoldenVectorTests runs one per key).</summary>
    public static class GoldenMovement
    {
        /// <summary>Every record of the set, key by key in file order.</summary>
        public static void CheckSet(GoldenData data, GoldenResult r, string set)
        {
            foreach (string key in GoldenWalk.Keys(data, set)) CheckFn(data, r, set, key);
        }

        /// <summary>The records of one key: a function's calls, "sequence:name" or "constant:name".</summary>
        public static void CheckFn(GoldenData data, GoldenResult r, string set, string key)
        {
            if (key.StartsWith("sequence:", StringComparison.Ordinal)) GoldenWalk.Each(data, r, set, key, c => Sequence(data, r, set, key.Substring(9), c));
            else if (key.StartsWith("constant:", StringComparison.Ordinal)) GoldenWalk.Each(data, r, set, key, c => Constant(r, c));
            else GoldenWalk.Each(data, r, set, key, c => Call(data, r, key, c));
        }

        // --- calls --------------------------------------------------------------------------------------------------

        static void Call(GoldenData data, GoldenResult r, string fn, Dictionary<string, object> c)
        {
            var a = GoldenWalk.Args(c);
            object want = c["result"];
            switch (fn)
            {
                case "jumpVelocity": r.Same(want, MovementMath.JumpVelocity(N(a[0]), N(a[1])), "result"); break;
                case "gravityFor": r.Same(want, MovementMath.GravityFor(N(a[0]), N(a[1])), "result"); break;
                case "stepsFor": r.Same(want, MovementMath.StepsFor(N(a[0]), N(a[1])), "result"); break;
                case "verticalStep":
                {
                    VerticalStepResult o = MovementMath.VerticalStep(N(a[0]), N(a[1]), N(a[2]));
                    r.Same(PortData.Obj(want)["vy"], o.vy, "vy");
                    r.Same(PortData.Obj(want)["dy"], o.dy, "dy");
                    break;
                }
                case "gravityScale":
                {
                    var f = PortData.Obj(a[2]);
                    var flags = new GravityFlags { jumpHeld = (bool)f["jumpHeld"], jumpAirtime = (bool)f["jumpAirtime"], windActive = (bool)f["windActive"] };
                    MovementConfig config = ConfigRef(data, r, a[3]);
                    if (config != null) r.Same(want, MovementMath.GravityScale(PortData.State((string)a[0]), N(a[1]), flags, config), "result");
                    break;
                }
                case "accelerate": r.SameVec(want, MovementMath.Accelerate(V(a[0]), V(a[1]), N(a[2]), N(a[3]), N(a[4])), "result"); break;
                case "applyFriction": r.SameVec(want, MovementMath.ApplyFriction(V(a[0]), N(a[1]), N(a[2]), N(a[3])), "result"); break;
                case "dashSpeed": r.Same(want, MovementMath.DashSpeed(N(a[0]), N(a[1])), "result"); break;
                case "dashExitSpeed": r.Same(want, MovementMath.DashExitSpeed(N(a[0]), N(a[1])), "result"); break;
                case "slideJumpSpeed": r.Same(want, MovementMath.SlideJumpSpeed(N(a[0]), N(a[1]), N(a[2])), "result"); break;
                case "regenPips": r.Same(want, MovementMath.RegenPips(N(a[0]), N(a[1]), N(a[2]), N(a[3]), (bool)a[4]), "result"); break;
                case "releaseJump":
                {
                    JumpRelease o = MovementMath.ReleaseJump(N(a[0]), (bool)a[1], N(a[2]), (bool)a[3]);
                    r.Same(PortData.Obj(want)["vy"], o.vy, "vy");
                    r.SameBool(PortData.Obj(want)["cutArmed"], o.cutArmed, "cutArmed");
                    break;
                }
                case "resolveJump":
                {
                    var x = PortData.Obj(a[0]);
                    var ctx = new JumpContext
                    {
                        state = PortData.State((string)x["state"]),
                        grounded = (bool)x["grounded"],
                        coyoteSteps = (int)N(x["coyoteSteps"]),
                        doubleJumpsLeft = (int)N(x["doubleJumpsLeft"]),
                        landingSoon = (bool)x["landingSoon"],
                        wallNear = (bool)x["wallNear"],
                        wallJumpsLeft = (int)N(x["wallJumpsLeft"]),
                        boostSteps = (int)N(x["boostSteps"]),
                        pips = N(x["pips"]),
                        dashJumpCost = N(x["dashJumpCost"]),
                    };
                    r.SameStr(want, PortData.Name(MovementMath.ResolveJump(ctx)), "result");
                    break;
                }
                case "alongWallSpeed": r.Same(want, MovementMath.AlongWallSpeed(V(a[0]), V(a[1])), "result"); break;
                case "wallJumpVelocity": r.SameVec(want, MovementMath.WallJumpVelocity(V(a[0]), V(a[1]), N(a[2]), N(a[3])), "result"); break;
                case "wallRideDashDir": r.SameVec(want, MovementMath.WallRideDashDir(V(a[0]), (bool)a[1], V(a[2]), V(a[3])), "result"); break;
                case "padLaunchSpeed": r.Same(want, MovementMath.PadLaunchSpeed(N(a[0]), N(a[1])), "result"); break;
                case "padLaunchVelocity": r.SameVec(want, MovementMath.PadLaunchVelocity(V(a[0]), V(a[1]), N(a[2]), N(a[3]), N(a[4])), "result"); break;
                case "fallDistance": r.Same(want, MovementMath.FallDistance(N(a[0]), N(a[1]), N(a[2])), "result"); break;
                case "wishDirection": r.SameVec(want, MovementMath.WishDirection(N(a[0]), N(a[1]), N(a[2])), "result"); break;
                case "horizontalSpeed": r.Same(want, MovementMath.HorizontalSpeed(V(a[0])), "result"); break;
                case "clampHorizontal": r.SameVec(want, MovementMath.ClampHorizontal(V(a[0]), N(a[1])), "result"); break;
                case "slopeSlideAccel": r.Same(want, MovementMath.SlopeSlideAccel(V(a[0]), V(a[1]), N(a[2]), N(a[3])), "result"); break;

                case "swingParams":
                {
                    ArclineConfig arcline = ArclineRef(data, r, a[0]);
                    if (arcline != null) SameParams(r, want, SwingMath.Params(arcline, N(a[1])));
                    break;
                }
                case "ropeDistance": r.Same(want, SwingMath.RopeDistance(V(a[0]), V(a[1])), "result"); break;
                case "swingEnergy": r.Same(want, SwingMath.SwingEnergy(V(a[0]), V(a[1]), N(a[2])), "result"); break;
                case "swingStep":
                {
                    SwingState s = SwingStateOf(a[0]);
                    SwingStepResult o = SwingMath.Step(ref s, V(a[1]), SwingInputOf(a[2]), ParamsOf(a[3]), N(a[4]));
                    r.SameBool(PortData.Obj(want)["taut"], o.taut, "taut");
                    r.Same(PortData.Obj(want)["stretch"], o.stretch, "stretch");
                    var after = PortData.Obj(PortData.Arr(c["argsAfter"])[0]);
                    r.SameVec(after["pos"], s.pos, "argsAfter.pos");
                    r.SameVec(after["vel"], s.vel, "argsAfter.vel");
                    r.Same(after["L"], s.L, "argsAfter.L");
                    break;
                }
                case "releaseVelocity":
                {
                    Vec3 o = SwingMath.ReleaseVelocity(V(a[0]), N(a[1]), N(a[2]));
                    r.SameVec(want, o, "result");
                    if (c.TryGetValue("argsAfter", out object after)) r.SameVec(PortData.Arr(after)[0], o, "argsAfter.vel"); // the JS mutates vel
                    break;
                }
                case "hookTime":
                {
                    ArclineConfig arcline = ArclineRef(data, r, a[1]);
                    if (arcline != null) r.Same(want, SwingMath.HookTime(N(a[0]), arcline), "result");
                    break;
                }
                case "pickAnchor":
                {
                    var cands = PortData.Arr(a[0]).Select(o => new AnchorCandidate { point = V(PortData.Obj(o)["point"]), node = (bool)PortData.Obj(o)["node"] }).ToList();
                    r.Same(want, SwingMath.PickAnchor(cands, V(a[1]), V(a[2]), N(a[3]), N(a[4])), "result");
                    break;
                }
                default: r.Fail($"no C# port mapped for {fn}"); break;
            }
        }

        // --- sequences ----------------------------------------------------------------------------------------------

        /// <summary>A recorded sequence, row by row from its start; the first mismatching row ends it (later rows follow from it).</summary>
        static void Sequence(GoldenData data, GoldenResult r, string set, string name, Dictionary<string, object> c)
        {
            var rows = PortData.Arr(c["rows"]).ConvertAll(PortData.Arr);
            if (name == "heldJumpVerticalArc")
            {
                MovementConfig config = data.Movement;
                double dt = N(data.Vectors(set)["dt"]);
                double g = MovementMath.GravityFor(config.jump.height, config.jump.apexTime);
                double vy = N(c["v0"]), y = 0;
                var flags = new GravityFlags { jumpHeld = true, jumpAirtime = true, windActive = false };
                for (int i = 0; i < rows.Count; i++)
                {
                    int before = r.mismatches.Count;
                    double mult = MovementMath.GravityScale(MoveState.Air, vy, flags, config);
                    VerticalStepResult o = MovementMath.VerticalStep(vy, g * mult, dt);
                    vy = o.vy;
                    y += o.dy;
                    r.Same(rows[i][1], mult, $"row {i + 1} mult");
                    r.Same(rows[i][2], vy, $"row {i + 1} vy");
                    r.Same(rows[i][3], y, $"row {i + 1} y");
                    if (r.mismatches.Count > before) break;
                }
                return;
            }
            if (!c.ContainsKey("start") || !c.ContainsKey("anchor"))
            {
                r.Fail($"unknown sequence {name}");
                return;
            }
            // freePendulum, pumpAndReel: repeated swingStep
            SwingState s = SwingStateOf(c["start"]);
            Vec3 anchor = V(c["anchor"]);
            SwingInput input = SwingInputOf(c["input"]);
            SwingParams p = ParamsOf(c["params"]);
            double h = N(c["h"]);
            for (int i = 0; i < rows.Count; i++)
            {
                int before = r.mismatches.Count;
                SwingStepResult o = SwingMath.Step(ref s, anchor, input, p, h);
                var row = rows[i];
                r.Same(row[1], s.pos.x, $"row {i + 1} pos.x");
                r.Same(row[2], s.pos.y, $"row {i + 1} pos.y");
                r.Same(row[3], s.pos.z, $"row {i + 1} pos.z");
                r.Same(row[4], s.vel.x, $"row {i + 1} vel.x");
                r.Same(row[5], s.vel.y, $"row {i + 1} vel.y");
                r.Same(row[6], s.vel.z, $"row {i + 1} vel.z");
                r.Same(row[7], s.L, $"row {i + 1} L");
                r.SameBool(row[8], o.taut, $"row {i + 1} taut");
                r.Same(row[9], o.stretch, $"row {i + 1} stretch");
                if (r.mismatches.Count > before) break;
            }
        }

        // --- constants ----------------------------------------------------------------------------------------------

        static void Constant(GoldenResult r, Dictionary<string, object> c)
        {
            if ((string)c["constant"] != "MoveState")
            {
                r.Fail($"unknown constant {c["constant"]}");
                return;
            }
            var names = PortData.Obj(c["value"]).Keys.ToList();
            var ours = Enum.GetValues(typeof(MoveState)).Cast<MoveState>().Select(s => PortData.Name(s)).ToList();
            if (!names.SequenceEqual(ours)) r.Fail($"MoveState [{string.Join(",", names)}] vs [{string.Join(",", ours)}]");
        }

        // --- argument decoding --------------------------------------------------------------------------------------

        static double N(object o) => PortData.Num(o);

        static Vec3 V(object o) => PortData.V3(o);

        /// <summary>{"$config": ""}: the whole movement config (the only reference these functions are recorded with).</summary>
        static MovementConfig ConfigRef(GoldenData data, GoldenResult r, object o)
        {
            string path = (string)PortData.Obj(o)["$config"];
            if (path == "") return data.Movement;
            r.Fail($"unsupported $config {path}");
            return null;
        }

        static ArclineConfig ArclineRef(GoldenData data, GoldenResult r, object o)
        {
            string path = (string)PortData.Obj(o)["$config"];
            if (path == "arcline") return data.Movement.arcline;
            r.Fail($"unsupported $config {path}");
            return null;
        }

        static SwingState SwingStateOf(object o)
        {
            var d = PortData.Obj(o);
            return new SwingState { pos = V(d["pos"]), vel = V(d["vel"]), L = N(d["L"]) };
        }

        static SwingInput SwingInputOf(object o)
        {
            var d = PortData.Obj(o);
            return new SwingInput { wish = d["wish"] == null ? (Vec3?)null : V(d["wish"]), reel = (bool)d["reel"], pulling = (bool)d["pulling"] };
        }

        static SwingParams ParamsOf(object o)
        {
            var d = PortData.Obj(o);
            return new SwingParams
            {
                g = N(d["g"]), drag = N(d["drag"]), pump = N(d["pump"]), softCap = N(d["softCap"]), reelSpeed = N(d["reelSpeed"]),
                minLen = N(d["minLen"]), stretch = N(d["stretch"]), omega = N(d["omega"]), pullAccel = N(d["pullAccel"]),
            };
        }

        static void SameParams(GoldenResult r, object want, SwingParams p)
        {
            var w = PortData.Obj(want);
            r.Same(w["g"], p.g, "g");
            r.Same(w["drag"], p.drag, "drag");
            r.Same(w["pump"], p.pump, "pump");
            r.Same(w["softCap"], p.softCap, "softCap");
            r.Same(w["reelSpeed"], p.reelSpeed, "reelSpeed");
            r.Same(w["minLen"], p.minLen, "minLen");
            r.Same(w["stretch"], p.stretch, "stretch");
            r.Same(w["omega"], p.omega, "omega");
            r.Same(w["pullAccel"], p.pullAccel, "pullAccel");
        }
    }
}
