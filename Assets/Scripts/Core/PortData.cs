// Reading PortExport/ (the web game's own outputs: config, golden vectors, trajectories, maps) and replaying its
// Mover trajectories through the C# Mover. Pure C#: the tests and the Unity collision checks both use it.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Skygrind.Core
{
    public static class PortData
    {
        // --- decoding (PortExport/README.md "Value encoding") -------------------------------------------------------

        public static Dictionary<string, object> Obj(object o) => (Dictionary<string, object>)o;

        public static List<object> Arr(object o) => (List<object>)o;

        public static object LoadJson(string path) => JsJson.Parse(File.ReadAllText(path));

        /// <summary>A number, including the {"$num": "-0" | "NaN" | "Infinity" | "-Infinity"} wrappers.</summary>
        public static double Num(object o)
        {
            if (o is double d) return d;
            if (o is Dictionary<string, object> w && w.TryGetValue("$num", out object s))
            {
                switch ((string)s)
                {
                    case "-0": return -0.0;
                    case "NaN": return double.NaN;
                    case "Infinity": return double.PositiveInfinity;
                    case "-Infinity": return double.NegativeInfinity;
                }
            }
            throw new FormatException($"not a number: {Describe(o)}");
        }

        public static Vec3 V3(object o)
        {
            if (o is List<object> a) return new Vec3(Num(a[0]), Num(a[1]), Num(a[2]));
            var d = Obj(o);
            return new Vec3(Num(d["x"]), Num(d["y"]), Num(d["z"]));
        }

        public static MoveState State(string s)
        {
            switch (s)
            {
                case "GROUND": return MoveState.Ground;
                case "AIR": return MoveState.Air;
                case "SLIDE": return MoveState.Slide;
                case "DASH": return MoveState.Dash;
                case "WALLRIDE": return MoveState.WallRide;
                case "SLAM": return MoveState.Slam;
                case "SWING": return MoveState.Swing;
            }
            throw new FormatException($"state {s}");
        }

        // NIGHT8 U2-02: each name is built once per enum value (the ADS check and the style meter ask every step).
        static string[] stateNames = new string[16], jumpNames = new string[16], eventNames = new string[64];
        static bool[] jumpKnown = new bool[16];

        static T[] Fit<T>(T[] a, int i)
        {
            if (i >= a.Length) Array.Resize(ref a, i + 1);
            return a;
        }

        public static string Name(MoveState s)
        {
            int i = (int)s;
            stateNames = Fit(stateNames, i);
            return stateNames[i] ?? (stateNames[i] = s == MoveState.WallRide ? "WALLRIDE" : s.ToString().ToUpperInvariant());
        }

        /// <summary>resolveJump's string result: null keeps the press buffered.</summary>
        public static string Name(JumpKind k)
        {
            int i = (int)k;
            jumpNames = Fit(jumpNames, i);
            jumpKnown = Fit(jumpKnown, i);
            if (!jumpKnown[i])
            {
                jumpNames[i] = k == JumpKind.None ? null : k.ToString().ToLowerInvariant();
                jumpKnown[i] = true;
            }
            return jumpNames[i];
        }

        /// <summary>The web's event type name (mover.js _emit), e.g. WallRideStart -> wallRideStart.</summary>
        public static string Name(MoverEventType t)
        {
            int i = (int)t;
            eventNames = Fit(eventNames, i);
            if (eventNames[i] == null)
            {
                string s = t.ToString();
                eventNames[i] = char.ToLowerInvariant(s[0]) + s.Substring(1);
            }
            return eventNames[i];
        }

        public static BoxSpec Box(object o)
        {
            var d = Obj(o);
            return new BoxSpec { min = V3(d["min"]), max = V3(d["max"]), kind = d.TryGetValue("kind", out object k) ? (string)k : "" };
        }

        public static WedgeSpec Wedge(object o)
        {
            var d = Obj(o);
            return new WedgeSpec { x = Num(d["x"]), z = Num(d["z"]), yaw = Num(d["yaw"]), length = Num(d["length"]), width = Num(d["width"]), rise = Num(d["rise"]) };
        }

        public static string Describe(object o)
        {
            switch (o)
            {
                case null: return "null";
                case double d: return d.ToString("R", CultureInfo.InvariantCulture);
                case string s: return "\"" + s + "\"";
                case bool b: return b ? "true" : "false";
                case List<object> l: return "[" + string.Join(", ", l.ConvertAll(Describe)) + "]";
                case Dictionary<string, object> m:
                    var sb = new StringBuilder("{");
                    foreach (var kv in m) sb.Append(kv.Key).Append(": ").Append(Describe(kv.Value)).Append(", ");
                    return sb.Append('}').ToString();
            }
            return o.ToString();
        }

        // --- config.json -> MovementConfig ------------------------------------------------------------------------

        /// <summary>
        /// A MovementConfig with every ported field read from config.json (the field names are the JS keys). Returns the
        /// paths that are missing in config.json or whose JSON value differs from the C# default (the hand copy).
        /// </summary>
        public static MovementConfig LoadConfig(Dictionary<string, object> json, List<string> differences = null)
        {
            var config = new MovementConfig();
            foreach (FieldInfo section in typeof(MovementConfig).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object target = section.GetValue(config);
                if (!json.TryGetValue(section.Name, out object sectionJson))
                {
                    differences?.Add($"{section.Name}: missing in config.json");
                    continue;
                }
                var values = Obj(sectionJson);
                foreach (FieldInfo f in section.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    string path = section.Name + "." + f.Name;
                    if (!values.TryGetValue(f.Name, out object v))
                    {
                        differences?.Add($"{path}: missing in config.json");
                        continue;
                    }
                    object old = f.GetValue(target);
                    object value;
                    if (f.FieldType == typeof(double)) value = Num(v);
                    else if (f.FieldType == typeof(int)) value = (int)Num(v);
                    else if (f.FieldType == typeof(bool)) value = (bool)v;
                    else continue;
                    if (!Equals(old, value)) differences?.Add($"{path}: C# {Describe(old is double od ? od : old is int oi ? (object)(double)oi : old)} vs config.json {Describe(v)}");
                    f.SetValue(target, value);
                }
            }
            return config;
        }

        // --- maps ---------------------------------------------------------------------------------------------------

        /// <summary>The collider arena.js builds for a map file: [floor, ...boxes], barriers, wedges, through three's Octree.</summary>
        public static ThreeOctree BuildMapOctree(Dictionary<string, object> map)
        {
            var c = Obj(map["collision"]);
            var boxes = Arr(c["boxes"]).ConvertAll(Box);
            boxes.AddRange(Arr(c["barriers"]).ConvertAll(Box));
            return ThreeGeometry.BuildOctree(boxes, Arr(c["wedges"]).ConvertAll(Wedge));
        }
    }

    /// <summary>One recorded Mover run (PortExport/trajectories/*.json).</summary>
    public sealed class Trajectory
    {
        public string name;
        public double dt;
        public Dictionary<string, object> scene;
        public Vec3 start;
        public bool ropeEnabled;
        public GrappleTarget? anchor;
        public List<Dictionary<string, object>> rows;

        public static Trajectory Load(string path)
        {
            var j = PortData.Obj(PortData.LoadJson(path));
            var t = new Trajectory
            {
                name = (string)j["name"],
                dt = PortData.Num(j["dt"]),
                scene = PortData.Obj(j["scene"]),
                start = PortData.V3(PortData.Obj(j["start"])["position"]),
                ropeEnabled = (bool)j["ropeEnabled"],
                rows = PortData.Arr(j["rows"]).ConvertAll(PortData.Obj),
            };
            if (j["anchor"] is Dictionary<string, object> a) t.anchor = new GrappleTarget { point = PortData.V3(a), node = 0 };
            return t;
        }

        public string SceneKind => (string)scene["kind"];
    }

    public sealed class ReplayReport
    {
        public string name;
        public int steps;
        public int firstDivergentStep = -1; // 1-based, -1 = every step identical
        public string firstDifference = "";
        public double divergenceAtFirst; // the size of the first difference (m, m/s, or a count)
        public double maxPositionDiff;
        public double maxVelocityDiff;
        public int stepsWithStateDiff;
        public int stepsWithEventDiff;
        // The first step where behaviour differs (state, grounded, crouched, touchingWall, rope phase, event types or
        // reasons), as opposed to numbers that differ by rounding only.
        public int firstBehaviourStep = -1;
        public string firstBehaviour = "";
        public int stepsWithEventTypeDiff;

        public bool Exact => firstDivergentStep < 0;
    }

    /// <summary>Feeds a trajectory's recorded inputs to the C# Mover and compares every step with the recording.</summary>
    public static class TrajectoryReplay
    {
        /// <param name="afterStep">run after every Mover step, inside the step's event window (the trial routes' arena.checkPads).</param>
        public static ReplayReport Run(Trajectory t, ICollisionWorld world, MovementConfig config, Action<Mover> afterStep = null)
        {
            var mover = new Mover(config, world) { ropeEnabled = t.ropeEnabled };
            var events = new List<MoverEvent>();
            mover.Events += e => events.Add(e);
            mover.Reset(t.start.x, t.start.y, t.start.z);
            var report = new ReplayReport { name = t.name, steps = t.rows.Count };
            foreach (var row in t.rows)
            {
                int step = (int)PortData.Num(row["step"]);
                InputFrame frame = Frame(PortData.Obj(row["input"]), t.anchor);
                events.Clear();
                mover.Step(frame, t.dt);
                afterStep?.Invoke(mover);
                var diffs = new List<(string what, double size)>();
                Vec3 p = PortData.V3(row["position"]), v = PortData.V3(row["velocity"]);
                double dp = MaxAbs(mover.position - p), dv = MaxAbs(mover.velocity - v);
                report.maxPositionDiff = Math.Max(report.maxPositionDiff, dp);
                report.maxVelocityDiff = Math.Max(report.maxVelocityDiff, dv);
                CompareVec("position", p, mover.position, diffs);
                CompareVec("velocity", v, mover.velocity, diffs);
                bool stateDiff = false;
                if ((string)row["state"] != PortData.Name(mover.state)) { diffs.Add(($"state {row["state"]} vs {PortData.Name(mover.state)}", 1)); stateDiff = true; }
                if ((bool)row["grounded"] != mover.grounded) { diffs.Add(($"grounded {row["grounded"]} vs {mover.grounded}", 1)); stateDiff = true; }
                if ((bool)row["crouched"] != mover.crouched) { diffs.Add(($"crouched {row["crouched"]} vs {mover.crouched}", 1)); stateDiff = true; }
                if ((bool)row["touchingWall"] != mover.touchingWall) { diffs.Add(($"touchingWall {row["touchingWall"]} vs {mover.touchingWall}", 1)); stateDiff = true; }
                CompareNum("pips", PortData.Num(row["pips"]), mover.pips, diffs);
                CompareNum("gravityMultiplier", PortData.Num(row["gravityMultiplier"]), mover.gravityMultiplier, diffs);
                if (row.TryGetValue("rope", out object ropeJson))
                {
                    var rope = PortData.Obj(ropeJson);
                    string phase = mover.rope.phase.ToString().ToLowerInvariant();
                    if ((string)rope["phase"] != phase) { diffs.Add(($"rope.phase {rope["phase"]} vs {phase}", 1)); stateDiff = true; }
                    CompareNum("rope.L", PortData.Num(rope["L"]), mover.rope.L, diffs);
                    if ((bool)rope["taut"] != mover.rope.taut) diffs.Add(($"rope.taut {rope["taut"]} vs {mover.rope.taut}", 1));
                }
                if (stateDiff) report.stepsWithStateDiff++;
                bool eventTypeDiff = false;
                if (CompareEvents(PortData.Arr(row["events"]), events, diffs, ref eventTypeDiff)) report.stepsWithEventDiff++;
                if (eventTypeDiff) report.stepsWithEventTypeDiff++;
                if ((stateDiff || eventTypeDiff) && report.firstBehaviourStep < 0)
                {
                    report.firstBehaviourStep = step;
                    report.firstBehaviour = string.Join("; ", diffs.FindAll(d => d.size == 1 && !d.what.Contains(" (Δ ")).ConvertAll(d => d.what));
                }
                if (diffs.Count > 0 && report.firstDivergentStep < 0)
                {
                    report.firstDivergentStep = step;
                    report.firstDifference = string.Join("; ", diffs.ConvertAll(d => d.what));
                    report.divergenceAtFirst = diffs[0].size;
                }
            }
            return report;
        }

        /// <summary>The InputFrame from a row's input (README "Rebuilding the InputFrame").</summary>
        public static InputFrame Frame(Dictionary<string, object> input, GrappleTarget? anchor)
        {
            var held = new HashSet<string>(PortData.Arr(input["held"]).ConvertAll(o => (string)o));
            var pressed = new HashSet<string>(PortData.Arr(input["pressed"]).ConvertAll(o => (string)o));
            Button B(string n) => new Button { held = held.Contains(n), pressed = pressed.Contains(n) };
            return new InputFrame
            {
                moveX = PortData.Num(input["moveX"]),
                moveZ = PortData.Num(input["moveZ"]),
                yaw = PortData.Num(input["yaw"]),
                pitch = PortData.Num(input["pitch"]),
                jump = B("jump"),
                dash = B("dash"),
                slide = B("slide"),
                grapple = B("grapple"),
                reel = B("reel"),
                grappleTarget = held.Contains("grapple") ? anchor : null,
            };
        }

        static double MaxAbs(Vec3 d) => Math.Max(Math.Abs(d.x), Math.Max(Math.Abs(d.y), Math.Abs(d.z)));

        static bool Same(double expected, double actual) =>
            BitConverter.DoubleToInt64Bits(expected) == BitConverter.DoubleToInt64Bits(actual) || (double.IsNaN(expected) && double.IsNaN(actual));

        static void CompareNum(string what, double expected, double actual, List<(string, double)> diffs)
        {
            if (!Same(expected, actual)) diffs.Add(($"{what} {Fmt(expected)} vs {Fmt(actual)} (Δ {Fmt(actual - expected)})", Math.Abs(actual - expected)));
        }

        static void CompareVec(string what, Vec3 expected, Vec3 actual, List<(string, double)> diffs)
        {
            CompareNum(what + ".x", expected.x, actual.x, diffs);
            CompareNum(what + ".y", expected.y, actual.y, diffs);
            CompareNum(what + ".z", expected.z, actual.z, diffs);
        }

        static string Fmt(double d) => d.ToString("R", CultureInfo.InvariantCulture);

        /// <summary>Compares the recorded events of a step with the C# ones (types, order, every payload field).</summary>
        static bool CompareEvents(List<object> expected, List<MoverEvent> actual, List<(string, double)> diffs, ref bool typeDiff)
        {
            bool any = false;
            if (expected.Count != actual.Count)
            {
                diffs.Add(($"events [{string.Join(",", expected.ConvertAll(e => (string)PortData.Obj(e)["type"]))}] vs [{string.Join(",", actual.ConvertAll(e => PortData.Name(e.type)))}]", 1));
                typeDiff = true;
                return true;
            }
            for (int i = 0; i < expected.Count; i++)
            {
                var e = PortData.Obj(expected[i]);
                MoverEvent a = actual[i];
                if ((string)e["type"] != PortData.Name(a.type))
                {
                    diffs.Add(($"event {i}: {e["type"]} vs {PortData.Name(a.type)}", 1));
                    any = true;
                    typeDiff = true;
                    continue;
                }
                foreach (var kv in e)
                {
                    if (kv.Key == "type" || kv.Key == "owner") continue;
                    string field = $"{e["type"]}.{kv.Key}";
                    switch (kv.Key)
                    {
                        case "kind": if ((string)kv.Value != PortData.Name(a.kind)) { diffs.Add(($"{field} {kv.Value} vs {PortData.Name(a.kind)}", 1)); any = typeDiff = true; } break;
                        case "from": if ((string)kv.Value != PortData.Name(a.from)) { diffs.Add(($"{field} {kv.Value} vs {PortData.Name(a.from)}", 1)); any = typeDiff = true; } break;
                        case "reason": if ((string)kv.Value != a.reason) { diffs.Add(($"{field} {kv.Value} vs {a.reason}", 1)); any = typeDiff = true; } break;
                        case "coyote": any |= BoolDiff(field, kv.Value, a.coyote, diffs); break;
                        case "buffered": any |= BoolDiff(field, kv.Value, a.buffered, diffs); break;
                        case "attached": any |= BoolDiff(field, kv.Value, a.attached, diffs); break;
                        case "node": any |= BoolDiff(field, kv.Value, a.node, diffs); break;
                        case "zip": any |= BoolDiff(field, kv.Value, a.zip, diffs); break;
                        case "position":
                        {
                            int before = diffs.Count;
                            CompareVec(field, PortData.V3(kv.Value), a.position, diffs);
                            any |= diffs.Count > before;
                            break;
                        }
                        default:
                        {
                            double actualValue = Field(a, kv.Key, out bool known);
                            if (!known)
                            {
                                diffs.Add(($"{field}: no C# field", 1));
                                any = true;
                                break;
                            }
                            int before = diffs.Count;
                            CompareNum(field, PortData.Num(kv.Value), actualValue, diffs);
                            any |= diffs.Count > before;
                            break;
                        }
                    }
                }
            }
            return any;
        }

        static bool BoolDiff(string field, object expected, bool actual, List<(string, double)> diffs)
        {
            if ((bool)expected == actual) return false;
            diffs.Add(($"{field} {expected} vs {actual}", 1));
            return true;
        }

        static double Field(MoverEvent e, string key, out bool known)
        {
            known = true;
            switch (key)
            {
                case "speed": return e.speed;
                case "fallSpeed": return e.fallSpeed;
                case "speedBefore": return e.speedBefore;
                case "speedAfter": return e.speedAfter;
                case "entrySpeed": return e.entrySpeed;
                case "pips": return e.pips;
                case "fallDist": return e.fallDist;
                case "wallJumpsLeft": return e.wallJumpsLeft;
                case "distance": return e.distance;
                case "length": return e.length;
                case "x": return e.point.x;
                case "y": return e.point.y;
                case "z": return e.point.z;
            }
            known = false;
            return 0;
        }
    }
}
