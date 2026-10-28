// The enemy scale bench (spike Tasks 4 and 5), set up like the web bench (skygrind port-export/bench/RESULTS.md):
// the Survival map from PortExport (collision = the web's Octree triangles + the core at its 'flat' load preset), the
// player standing still at the map's playerSpawn (0, 0, 40) looking down -z, N enemies from the web's shared spawn plan
// (squads of 5 at the ground spawns 15-40 m away), enemies never die. The web game's loop: the frame delta clamped to
// 0.05 s, whole 1/120 s steps, then render.
// Variants (all enemies are Husks: chase, separation, moveHusk):
//   closer-web   managed C#: line of sight every 0.25 s per enemy + grid flow field + collider.js's aggregated
//                capsuleIntersect push-out, all through the C# web collider (three's Octree + the core layer)
//   closer-unity managed C#: the same through Unity physics (Physics.Raycast; ComputePenetration on the per-triangle
//                MeshColliders)
//   plain        managed C#: chase + separation + gravity on the plane y = 0 (no world queries)
//   burst-plain, burst-full: Jobs + Burst versions of plain and closer (PhysX rays, push-out over the triangles)
// Command line (a built Player): -bench runs every -benchVariants x -benchCounts (comma lists) for -benchWarm s then
// -benchMeasure s each, writes the run records to -benchOut (JSON; -benchRaw adds every frame and step time) and quits.
// -benchTag labels the records (the pass), -portExport points at PortExport/ (default: found above the data folder).
// In the Editor: F1-F5 variant, 1-4 N, or RunSuite through unity command eval.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Skygrind.Core;
using Skygrind.Game;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Skygrind.Bench
{
    public sealed class EnemyBench : MonoBehaviour
    {
        public static readonly string[] Variants = { "closer-web", "closer-unity", "plain", "burst-plain", "burst-full" };
        public static readonly int[] Counts = { 24, 40, 100, 200 };

        public Material baseMaterial;
        public Material enemyMaterial; // GPU instancing on (an asset, so a build keeps the instancing variants)

        MovementConfig config;
        ArenaMap map;
        ThreeOctree octree;
        LayeredWorld webWorld;
        FlowField flow;
        UnityPhysicsWorld unityWorld;
        List<Vec3> spawnNodes;
        Vec3 player;
        double spawnMin, spawnMax;
        EnemyCrowd managed;
        EnemyCrowdBurst burst;
        Mesh capsuleMesh;
        readonly Matrix4x4[] matrices = new Matrix4x4[1023];
        double step;
        double accumulator;
        int stepIndex;
        string portExport;

        // public state (HUD, suite)
        public string variant = "closer-web";
        public int count = 24;
        public double lastFps, lastStepMs;
        readonly Stopwatch watch = new Stopwatch();
        long stepTicks;
        int steps;
        double hudTime;
        int hudFrames;

        // suite
        enum Phase { Idle, Warm, Measure }
        Phase phase = Phase.Idle;
        readonly List<(string variant, int n)> queue = new List<(string, int)>();
        double phaseStart, warmSeconds = 10, measureSeconds = 30;
        readonly List<double> frameMs = new List<double>(200000);
        readonly List<double> stepMs = new List<double>(10000);
        long raysMeasured;
        readonly List<string> records = new List<string>();
        readonly List<string> raw = new List<string>();
        string outPath, rawPath, tag = "";
        bool quitWhenDone;
        bool profile; // -benchProfile: per-phase timers in the managed crowd (a labeled breakdown run, not the main numbers)

        public bool SuiteRunning => phase != Phase.Idle;

        void Awake()
        {
            string[] args = Environment.GetCommandLineArgs();
            string Arg(string name) { int i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
            portExport = Arg("-portExport") ?? FindPortExport();
            var configJson = PortData.Obj(PortData.LoadJson(Path.Combine(portExport, "config.json")));
            config = PortData.LoadConfig(configJson);
            var director = PortData.Obj(configJson["director"]);
            spawnMin = PortData.Num(director["spawnMin"]);
            spawnMax = PortData.Num(director["spawnMax"]);

            step = 1 / config.loop.simHz;
            Time.fixedDeltaTime = (float)step;
            Time.maximumDeltaTime = (float)config.loop.maxFrameDelta;
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            Physics.simulationMode = SimulationMode.Script; // queries only
            if (!Application.isEditor) Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);

            if (baseMaterial == null) baseMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            LoadSurvival();
            ArenaBuilder.Build(map, baseMaterial, octree);
            var probe = new GameObject("Enemy query capsule") { layer = 2 };
            probe.transform.position = new Vector3(0, -1000, 0);
            unityWorld = new UnityPhysicsWorld(probe.AddComponent<CapsuleCollider>());

            var tmp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsuleMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(tmp);
            if (enemyMaterial == null)
            {
                enemyMaterial = ArenaBuilder.Lit(baseMaterial, new Color(0.85f, 0.22f, 0.18f));
                enemyMaterial.enableInstancing = true;
            }

            // The player's view: eye height above the spawn, yaw 0 (looking down -z), the web's FOV and clip planes.
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.SetPositionAndRotation(WebSpace.ToUnity(new Vec3(player.x, player.y + config.body.eyeHeight, player.z)), WebSpace.LookRotation(0, 0, 0));
                cam.fieldOfView = (float)config.settings.fovDeg;
                cam.nearClipPlane = (float)config.camera.near;
                cam.farClipPlane = (float)config.camera.far;
            }

            Debug.Log($"[bench] GPU: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType}, {SystemInfo.graphicsMemorySize} MB); CPU: {SystemInfo.processorType}; PortExport: {portExport}");

            if (Arg("-benchWarm") is string w) warmSeconds = double.Parse(w, CultureInfo.InvariantCulture);
            if (Arg("-benchMeasure") is string m) measureSeconds = double.Parse(m, CultureInfo.InvariantCulture);
            tag = Arg("-benchTag") ?? "";
            profile = Array.IndexOf(args, "-benchProfile") >= 0;
            rawPath = Arg("-benchRaw");
            string[] vs = Arg("-benchVariants")?.Split(',') ?? Variants.Take(3).ToArray();
            int[] ns = Arg("-benchCounts")?.Split(',').Select(int.Parse).ToArray() ?? Counts;
            outPath = Arg("-benchOut") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "..", Application.isEditor ? "Logs" : "", "bench-results.json"));
            if (Array.IndexOf(args, "-bench") >= 0)
            {
                quitWhenDone = true;
                RunSuite(vs, ns, warmSeconds, measureSeconds);
            }
            else Setup(variant, count);
        }

        static string FindPortExport()
        {
            for (DirectoryInfo d = new DirectoryInfo(Application.dataPath); d != null; d = d.Parent)
            {
                string p = Path.Combine(d.FullName, "PortExport");
                if (File.Exists(Path.Combine(p, "config.json"))) return p;
            }
            throw new DirectoryNotFoundException("PortExport/ not found above " + Application.dataPath + " (pass -portExport <path>)");
        }

        /// <summary>The Survival map as arena.js loads it, from PortExport/maps/survival.json.</summary>
        void LoadSurvival()
        {
            var json = PortData.Obj(PortData.LoadJson(Path.Combine(portExport, "maps", "survival.json")));
            var collision = PortData.Obj(json["collision"]);
            map = new ArenaMap { floorSize = PortData.Num(json["floorSize"]), killPlaneY = PortData.Num(json["killPlaneY"]) };
            map.boxes.AddRange(PortData.Arr(collision["boxes"]).ConvertAll(PortData.Box)); // the floor + the map's boxes (barriers: collision only)
            map.wedges.AddRange(PortData.Arr(collision["wedges"]).ConvertAll(PortData.Wedge));
            foreach (object o in PortData.Arr(json["pads"]))
            {
                var pad = PortData.Obj(o);
                bool aimed = pad.TryGetValue("target", out object t) && t != null;
                map.pads.Add(new PadSpec { pos = PortData.V3(pad), aimed = aimed, target = aimed ? PortData.V3(t) : default, apexAbove = aimed ? PortData.Num(pad["apexAbove"]) : 0 });
            }
            foreach (object o in PortData.Arr(json["grappleNodes"])) map.grappleNodes.Add(PortData.V3(o));
            octree = PortData.BuildMapOctree(json);

            var core = PortData.Obj(json["core"]);
            var spec = PortData.Obj(core["spec"]);
            var grid = new CoreGrid(config, new CoreSpec
            {
                x0 = PortData.Num(spec["x0"]), z0 = PortData.Num(spec["z0"]),
                cells = (int)PortData.Num(spec["cells"]), cellSize = PortData.Num(spec["cellSize"]),
            });
            grid.SetHeights(PortData.Arr(PortData.Obj(core["presets"])["flat"]).ConvertAll(PortData.Num).ToArray()); // the web bench's core: 'flat'
            webWorld = new LayeredWorld(octree, grid);

            var spawns = PortData.Obj(json["spawns"]);
            player = PortData.V3(spawns["playerSpawn"]);
            spawnNodes = SpawnPlan.Nodes(PortData.Arr(spawns["ground"]).ConvertAll(PortData.V3), player, spawnMin, spawnMax);
            // Spike choices, not web config (the web's nav field is not ported): 1 m cells; a box topping out below
            // 0.6 m is walkable (the floor), one starting above 1.9 m is overhead (the Husk is 1.8 m tall).
            flow = new FlowField(map.boxes, map.floorSize, 1, 0.6, 1.9);
        }

        void OnDestroy()
        {
            EnemyCrowdBurst old = burst;
            burst = null;
            old?.Dispose();
        }

        void Setup(string v, int n)
        {
            variant = v;
            count = n;
            EnemyCrowdBurst old = burst;
            burst = null;
            old?.Dispose();
            stepIndex = 0;
            accumulator = 0;
            bool full = v != "plain" && v != "burst-plain";
            ICollisionWorld world = v == "closer-unity" ? unityWorld : v == "closer-web" ? webWorld : null;
            var crowd = new EnemyCrowd(config, world, full, full ? flow : null, map.floorSize);
            crowd.Place(SpawnPlan.Plan(n, spawnNodes));
            if (v.StartsWith("burst"))
            {
                burst = new EnemyCrowdBurst(config, full, flow, octree, map.floorSize);
                burst.Spawn(crowd);
                managed = null;
            }
            else managed = crowd;
            crowd.profile = profile;
        }

        /// <summary>Queue every variant x N and start (also the Editor's entry point, through unity command eval).</summary>
        public void RunSuite(string[] vs, int[] ns, double warm, double measure)
        {
            warmSeconds = warm;
            measureSeconds = measure;
            queue.Clear();
            records.Clear();
            raw.Clear();
            foreach (string v in vs)
                foreach (int n in ns) queue.Add((v, n));
            StartNext();
        }

        void StartNext()
        {
            if (queue.Count == 0)
            {
                phase = Phase.Idle;
                if (quitWhenDone) Application.Quit();
                return;
            }
            (string v, int n) = queue[0];
            queue.RemoveAt(0);
            Setup(v, n);
            phase = Phase.Warm;
            phaseStart = Time.realtimeSinceStartupAsDouble;
        }

        void Update()
        {
            double frameDt = Math.Min(Time.unscaledDeltaTime, config.loop.maxFrameDelta);
            if (phase == Phase.Measure) frameMs.Add(Time.unscaledDeltaTime * 1000.0);
            accumulator += frameDt;
            while (accumulator >= step)
            {
                SimStep();
                accumulator -= step;
            }
            Render();
            Hud();
            Suite();
            if (phase == Phase.Idle) Keys();
        }

        void SimStep()
        {
            watch.Restart();
            int rays;
            if (burst != null)
            {
                burst.Step(player, step, stepIndex);
                rays = burst.raysThisStep;
            }
            else
            {
                managed.Step(player, step, stepIndex);
                rays = managed.raysThisStep;
            }
            watch.Stop();
            stepIndex++;
            stepTicks += watch.ElapsedTicks;
            steps++;
            if (phase == Phase.Measure)
            {
                stepMs.Add(watch.ElapsedTicks * 1000.0 / Stopwatch.Frequency);
                raysMeasured += rays;
            }
        }

        Vec3 Position(int i)
        {
            if (burst == null) return managed.enemies[i].position;
            var d = burst.Position(i);
            return new Vec3(d.x, d.y, d.z);
        }

        int Alive => burst != null ? burst.Count : managed.enemies.Count;

        void Render()
        {
            int n = Alive;
            var rp = new RenderParams(enemyMaterial);
            var scale = new Vector3((float)(config.husk.radius * 2), (float)(config.husk.height / 2), (float)(config.husk.radius * 2));
            for (int start = 0; start < n; start += matrices.Length)
            {
                int c = Math.Min(matrices.Length, n - start);
                for (int k = 0; k < c; k++)
                {
                    Vector3 u = WebSpace.ToUnity(Position(start + k));
                    matrices[k] = Matrix4x4.TRS(new Vector3(u.x, u.y + (float)(config.husk.height / 2), u.z), Quaternion.identity, scale);
                }
                Graphics.RenderMeshInstanced(rp, capsuleMesh, 0, matrices, c);
            }
        }

        void Hud()
        {
            hudTime += Time.unscaledDeltaTime;
            hudFrames++;
            if (hudTime < 0.5) return;
            lastFps = hudFrames / hudTime;
            lastStepMs = steps > 0 ? stepTicks * 1000.0 / Stopwatch.Frequency / steps : 0;
            hudTime = 0;
            hudFrames = 0;
            stepTicks = 0;
            steps = 0;
        }

        void Suite()
        {
            if (phase == Phase.Idle) return;
            double t = Time.realtimeSinceStartupAsDouble - phaseStart;
            if (phase == Phase.Warm && t >= warmSeconds)
            {
                phase = Phase.Measure;
                phaseStart = Time.realtimeSinceStartupAsDouble;
                frameMs.Clear();
                stepMs.Clear();
                raysMeasured = 0;
                if (managed != null)
                {
                    Array.Clear(managed.phaseTicks, 0, managed.phaseTicks.Length);
                    managed.neighbourChecks = 0;
                }
            }
            else if (phase == Phase.Measure && t >= measureSeconds)
            {
                Record(t);
                StartNext();
            }
        }

        /// <summary>The web bench's statistics (runBrowser.mjs / simBench.mjs): p = sorted[floor(p * n)], worst 1% = the
        /// mean of sorted[floor(0.99 * n)..].</summary>
        static string Stats(List<double> values)
        {
            var s = new List<double>(values);
            s.Sort();
            if (s.Count == 0) return "null";
            double P(double p) => s[Math.Min(s.Count - 1, (int)Math.Floor(p * s.Count))];
            int w = (int)Math.Floor(s.Count * 0.99);
            double worst = 0;
            for (int i = w; i < s.Count; i++) worst += s[i];
            worst /= s.Count - w;
            return Json(("count", s.Count), ("mean", s.Average()), ("p50", P(0.5)), ("p99", P(0.99)), ("worst1pctMean", worst), ("max", s[s.Count - 1]));
        }

        /// <summary>Mean frame time per 5 s of the measured window (the web bench's first -> last window).</summary>
        List<double> Windows()
        {
            var means = new List<double>();
            double edge = 5000, sum = 0, elapsed = 0;
            int k = 0;
            foreach (double f in frameMs)
            {
                sum += f;
                k++;
                elapsed += f;
                if (elapsed >= edge)
                {
                    means.Add(sum / k);
                    sum = 0;
                    k = 0;
                    edge += 5000;
                }
            }
            if (k > 0) means.Add(sum / k);
            return means;
        }

        void Record(double seconds)
        {
            int alive = 0, near = 0;
            for (int i = 0; i < Alive; i++)
            {
                Vec3 p = Position(i);
                if (p.y > map.killPlaneY) alive++;
                if (JsMath.Hypot(p.x - player.x, p.z - player.z) < 3) near++;
            }
            string record = Json(
                ("tag", tag), ("variant", variant), ("n", count), ("aliveAtEnd", alive), ("within3mAtEnd", near),
                ("fps", frameMs.Count / seconds), ("measuredSeconds", seconds),
                ("frameMs", new RawJson(Stats(frameMs))), ("frameWindowMeansMs", new RawJson("[" + string.Join(",", Windows().Select(F)) + "]")),
                ("stepsPerSecond", stepMs.Count / seconds), ("stepMs", new RawJson(Stats(stepMs))),
                ("losRaysPerSecond", raysMeasured / seconds), ("flowRebuilds", flow.rebuilds),
                ("profile", profile), ("phaseMsPerStep", new RawJson(PhaseJson())), ("neighbourChecksPerStep", profile && managed != null && stepMs.Count > 0 ? managed.neighbourChecks / (double)stepMs.Count : 0),
                ("warmSeconds", warmSeconds),
                ("gpu", SystemInfo.graphicsDeviceName), ("graphicsApi", SystemInfo.graphicsDeviceType.ToString()), ("gpuMemoryMB", SystemInfo.graphicsMemorySize),
                ("cpu", SystemInfo.processorType), ("logicalCores", SystemInfo.processorCount), ("ramMB", SystemInfo.systemMemorySize),
                ("os", SystemInfo.operatingSystem), ("unity", Application.unityVersion), ("editor", Application.isEditor),
                ("backend", Backend()), ("developmentBuild", Debug.isDebugBuild),
                ("screen", $"{Screen.width}x{Screen.height}"), ("fullScreenMode", Screen.fullScreenMode.ToString()),
                ("vSyncCount", QualitySettings.vSyncCount), ("targetFrameRate", Application.targetFrameRate),
                ("quality", QualitySettings.names[QualitySettings.GetQualityLevel()]),
                ("utc", DateTime.UtcNow.ToString("o")));
            records.Add(record);
            if (rawPath != null)
                raw.Add(Json(("variant", variant), ("n", count), ("frameMs", new RawJson("[" + string.Join(",", frameMs.Select(F)) + "]")), ("stepMs", new RawJson("[" + string.Join(",", stepMs.Select(F)) + "]"))));
            Debug.Log($"[bench] {tag} {variant} N={count}: {frameMs.Count / seconds:0.0} fps, step {(stepMs.Count > 0 ? stepMs.Average() : 0):0.0000} ms avg; {record}");
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            File.WriteAllText(outPath, "[\n" + string.Join(",\n", records) + "\n]\n");
            if (rawPath != null) File.WriteAllText(rawPath, "[\n" + string.Join(",\n", raw) + "\n]\n");
        }

        string PhaseJson()
        {
            if (!profile || managed == null || stepMs.Count == 0) return "null";
            double Ms(int i) => managed.phaseTicks[i] * 1000.0 / Stopwatch.Frequency / stepMs.Count;
            return Json(("gridAndFlow", Ms(0)), ("lineOfSight", Ms(1)), ("steerAndSeparation", Ms(2)), ("moveAndPushOut", Ms(3)));
        }

        sealed class RawJson
        {
            public readonly string text;
            public RawJson(string text) => this.text = text;
        }

        static string F(double d) => d.ToString("R", CultureInfo.InvariantCulture);

        static string Json(params (string key, object value)[] fields)
        {
            var sb = new StringBuilder("{");
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('"').Append(fields[i].key).Append("\": ");
                switch (fields[i].value)
                {
                    case RawJson r: sb.Append(r.text); break;
                    case string s: sb.Append('"').Append(s.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append('"'); break;
                    case bool b: sb.Append(b ? "true" : "false"); break;
                    case double d: sb.Append(F(d)); break;
                    case IFormattable f: sb.Append(f.ToString(null, CultureInfo.InvariantCulture)); break;
                    default: sb.Append("null"); break;
                }
            }
            return sb.Append('}').ToString();
        }

        static string Backend()
        {
#if ENABLE_IL2CPP
            return "IL2CPP";
#else
            return "Mono";
#endif
        }

        void Keys()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            Key[] fk = { Key.F1, Key.F2, Key.F3, Key.F4, Key.F5 };
            for (int i = 0; i < fk.Length; i++)
                if (kb[fk[i]].wasPressedThisFrame) Setup(Variants[i], count);
            Key[] nk = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };
            for (int i = 0; i < nk.Length; i++)
                if (kb[nk[i]].wasPressedThisFrame) Setup(variant, Counts[i]);
        }
    }
}
