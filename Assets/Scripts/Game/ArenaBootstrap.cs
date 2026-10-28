// The movement test arena: builds the map, runs the player mover at a fixed 120 Hz with whole-step timers, draws the
// camera feel, and measures frame time and the sim cost per step.
// Loop (default): the web game's loop (main.js frame()): frame delta clamped to loop.maxFrameDelta, whole steps of
// 1/simHz from an accumulator in Update, render interpolated by alpha = accumulator / step. Update runs after the
// Input System has processed the frame's events; MonoBehaviour.FixedUpdate runs before that, so it would see each
// press one frame late. F2 switches to FixedUpdate (Time.fixedDeltaTime = 1/120 either way) to compare the two.
// F1 switches the collision world between Unity physics and the web's own collider (the C# port of three's Octree +
// the core grid, bit-exact against the web game's recorded runs), live. P reshapes the Grinder; O goes to it.
// Untested against the web game (not in the exported runs): jump pads, the kill plane, the Grinder's motion, perks.
using System.Diagnostics;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Skygrind.Game
{
    public sealed class ArenaBootstrap : MonoBehaviour
    {
        [Tooltip("URP Lit material the arena materials are cloned from (keeps the shader in builds).")]
        public Material baseMaterial;

        [Tooltip("Step the mover in MonoBehaviour.FixedUpdate instead of the Update accumulator (F2).")]
        public bool useFixedUpdate;

        [Tooltip("Collide against the three.js reference triangles instead of Unity physics (F1).")]
        public bool referenceWorld;

        public bool showHud = true;

        public readonly MovementConfig config = new MovementConfig();
        ArenaMap map;
        Mover mover;
        UnityPhysicsWorld unityWorld;
        LayeredWorld webWorld;
        CoreGrid core;
        CoreView coreView;
        Mulberry32 coreRng;
        int corePreset;
        InputReader input;
        GrappleAim aim;
        CameraFeel feel;
        Camera cam;
        LineRenderer ropeLine;
        double step;
        double accumulator;
        Vec3 previous;
        Vec3 current;

        // Measurement (1 s windows)
        readonly Stopwatch stepWatch = new Stopwatch();
        double windowTime;
        int windowFrames;
        int windowSteps;
        long windowStepTicks;
        long windowStepMax;
        double windowFrameMax;
        int windowContacts;
        int windowQueries;
        int windowFallbacks;
        int stepsThisFrame;
        string hudText = "";
        string hookText = "";
        int hookCheck;

        static readonly (string name, double x, double y, double z, double yawDeg)[] Spots =
        {
            ("lane", 0, 0, 28, 0), // config.arena.testSpots
            ("ledges", -11, 0, 26, 0),
            ("gap", 9, 1, 21, -90),
            ("tunnel", -14, 0, 14, 0),
            ("wall-ride corridor", -27.5, 0, 2, 15),
            ("12 m wall", 27.3, 0, -9, 0),
            ("slam deck", 24, 4, -27, 180),
            ("jump pad", 2.2, 0, 18.5, 0),
            ("ramps (spike)", 0, 0, 6, 0),
            ("mesh test piece (spike)", 19, 0, 0, -90),
        };

        public Mover Mover => mover;

        void Awake()
        {
            step = 1 / config.loop.simHz;
            Time.fixedDeltaTime = (float)step;
            Time.maximumDeltaTime = (float)config.loop.maxFrameDelta;
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            Physics.simulationMode = SimulationMode.Script; // queries only: nothing here is simulated

            if (baseMaterial == null) baseMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            map = TrainingMap.Build();
            ThreeOctree octree = map.BuildOctree();
            GameObject arena = ArenaBuilder.Build(map, baseMaterial, octree);
            core = map.CreateCore(config);
            webWorld = new LayeredWorld(octree, core);
            coreRng = new Mulberry32(1);
            if (core != null) coreView = new CoreView(core, config, arena.transform, ArenaBuilder.Lit(baseMaterial, new Color(0.55f, 0.50f, 0.62f)));

            var probe = new GameObject("Mover query capsule") { layer = 2 }; // Ignore Raycast: never a candidate
            probe.transform.position = new Vector3(0, -1000, 0);
            unityWorld = new UnityPhysicsWorld(probe.AddComponent<CapsuleCollider>());

            mover = new Mover(config, referenceWorld ? (ICollisionWorld)webWorld : unityWorld) { ropeEnabled = true };
            input = new InputReader(config);
            aim = new GrappleAim(config, map.grappleNodes);
            feel = new CameraFeel(config, mover);
            mover.Events += e =>
            {
                if (e.type == MoverEventType.WallRideEnd || e.type == MoverEventType.RopeRelease) Debug.Log($"[mover] {e.type} {e.reason} {e.speed:0.0}");
            };

            cam = Camera.main;
            if (cam == null) cam = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            cam.nearClipPlane = (float)config.camera.near;
            cam.farClipPlane = (float)config.camera.far;

            ropeLine = new GameObject("Rope").AddComponent<LineRenderer>();
            ropeLine.positionCount = 2;
            ropeLine.widthMultiplier = (float)config.arcline.ropeWidth * 2;
            ropeLine.material = new Material(baseMaterial) { color = new Color(0.10f, 0.83f, 0.90f) };
            ropeLine.enabled = false;

            Respawn(map.spawn, map.spawnYawDeg);
        }

        void Respawn(Vec3 at, double yawDeg)
        {
            mover.Reset(at.x, at.y, at.z);
            input.SetLook(yawDeg * System.Math.PI / 180, 0);
            previous = current = mover.position;
            accumulator = 0;
            feel.Cut();
        }

        void Update()
        {
            input.Poll();
            HotKeys();
            double frameDt = System.Math.Min(Time.unscaledDeltaTime, config.loop.maxFrameDelta);
            if (!useFixedUpdate)
            {
                accumulator += frameDt;
                while (accumulator >= step)
                {
                    SimStep();
                    accumulator -= step;
                }
            }
            double alpha = useFixedUpdate
                ? Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime)
                : accumulator / step;
            Vec3 drawn = previous + (current - previous) * alpha;
            feel.Apply(cam, drawn, frameDt, input.yaw, input.pitch);
            coreView?.Render(alpha);
            DrawRope(drawn);
            Measure(Time.unscaledDeltaTime);
            stepsThisFrame = 0;
        }

        void FixedUpdate()
        {
            if (useFixedUpdate) SimStep();
        }

        /// <summary>One fixed simulation step (main.js simulate): sample input, pick the hook, step, pads, fall.</summary>
        void SimStep()
        {
            InputFrame frame = input.Sample();
            ICollisionWorld world = mover.World;
            Vec3 eye = new Vec3(current.x, current.y + config.body.eyeHeight, current.z);
            unityWorld.gathers = unityWorld.penetrationTests = unityWorld.contacts = unityWorld.pointFallbacks = 0;
            stepWatch.Restart();
            aim.Step(ref frame, mover, eye, world);
            previous = current;
            if (core != null)
            {
                core.Step(mover); // main.js: arena.stepCore(bodies) before player.step
                coreView.Sync();
            }
            mover.Step(frame, step);
            map.CheckPads(mover, config);
            stepWatch.Stop();
            if (mover.position.y < map.killPlaneY) Respawn(map.spawn, map.spawnYawDeg);
            current = mover.position;
            long ticks = stepWatch.ElapsedTicks;
            windowSteps++;
            windowStepTicks += ticks;
            if (ticks > windowStepMax) windowStepMax = ticks;
            windowQueries += unityWorld.gathers + unityWorld.penetrationTests;
            windowContacts += unityWorld.contacts;
            windowFallbacks += unityWorld.pointFallbacks;
            stepsThisFrame++;
        }

        void HotKeys()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (kb.rKey.wasPressedThisFrame) Respawn(map.spawn, map.spawnYawDeg);
            if (kb.f1Key.wasPressedThisFrame)
            {
                referenceWorld = !referenceWorld;
                mover.World = referenceWorld ? (ICollisionWorld)webWorld : unityWorld;
            }
            if (kb.f2Key.wasPressedThisFrame)
            {
                useFixedUpdate = !useFixedUpdate;
                accumulator = 0;
            }
            if (kb.f3Key.wasPressedThisFrame) showHud = !showHud;
            if (kb.vKey.wasPressedThisFrame) QualitySettings.vSyncCount = QualitySettings.vSyncCount == 0 ? 1 : 0;
            if (kb.pKey.wasPressedThisFrame && core != null)
            {
                // arena.reshape: tween to the next preset over endless.reshapeTime on the fixed step.
                corePreset = (corePreset + 1) % CoreGrid.Presets.Length;
                core.Reshape(CoreGrid.PresetHeights(CoreGrid.Presets[corePreset], core.spec.cells, config.core.maxHeight, config.core.heightStep, coreRng),
                    config.endless.reshapeTime, step);
            }
            if (kb.oKey.wasPressedThisFrame)
            {
                Spot g = TrainingMap.TestSpots["grinder"];
                Respawn(new Vec3(g.x, g.y, g.z), g.yawDeg);
            }
            Key[] digits = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0 };
            for (int i = 0; i < digits.Length; i++)
            {
                if (!kb[digits[i]].wasPressedThisFrame) continue;
                var s = Spots[i];
                Respawn(new Vec3(s.x, s.y, s.z), s.yawDeg);
            }
        }

        void DrawRope(Vec3 drawnFeet)
        {
            Rope r = mover.rope;
            ropeLine.enabled = r.phase != RopePhase.None;
            if (!ropeLine.enabled) return;
            Vec3 chest = new Vec3(drawnFeet.x, drawnFeet.y + config.arcline.ropeHeight, drawnFeet.z);
            ropeLine.SetPosition(0, WebSpace.ToUnity(chest));
            ropeLine.SetPosition(1, WebSpace.ToUnity(r.phase == RopePhase.Flying ? r.hook : r.anchor));
        }

        void Measure(double rawDt)
        {
            windowTime += rawDt;
            windowFrames++;
            if (rawDt > windowFrameMax) windowFrameMax = rawDt;
            if (--hookCheck <= 0)
            {
                // arcline.js update(): the hook reticle, checked a few times a second.
                hookCheck = 4;
                if (mover.rope.phase != RopePhase.None) hookText = mover.rope.phase.ToString().ToLowerInvariant();
                else
                {
                    Vec3 eye = new Vec3(current.x, current.y + config.body.eyeHeight, current.z);
                    GrappleTarget? t = aim.Choose(eye, GrappleAim.AimDirection(input.yaw, input.pitch), mover.World);
                    hookText = t.HasValue ? (t.Value.node != 0 ? "node" : "surface") : "none";
                }
            }
            if (windowTime < 0.5) return;
            double usPerTick = 1e6 / Stopwatch.Frequency;
            double fps = windowFrames / windowTime;
            double avgStep = windowSteps > 0 ? windowStepTicks * usPerTick / windowSteps : 0;
            double perStep(int n) => windowSteps > 0 ? (double)n / windowSteps : 0;
            hudText =
                $"{fps:0} fps   frame {1000 * windowTime / windowFrames:0.00} ms (max {1000 * windowFrameMax:0.0})   steps/s {windowSteps / windowTime:0}\n" +
                $"sim step {avgStep:0.0} us avg, {windowStepMax * usPerTick:0.0} us max   world: {(referenceWorld ? "WEB COLLIDER (C# three.js Octree)" : "UNITY PHYSICS (per-triangle MeshColliders)")}   loop: {(useFixedUpdate ? "FixedUpdate" : "Update accumulator")}   grinder: {(core == null ? "-" : CoreGrid.Presets[corePreset])}\n" +
                $"unity queries/step {perStep(windowQueries):0.0}, contacts/step {perStep(windowContacts):0.00}, mesh point fallbacks/step {perStep(windowFallbacks):0.00}   vsync {QualitySettings.vSyncCount}";
            windowTime = 0;
            windowFrames = 0;
            windowSteps = 0;
            windowStepTicks = 0;
            windowStepMax = 0;
            windowFrameMax = 0;
            windowContacts = windowQueries = windowFallbacks = 0;
        }

        GUIStyle style;

        void OnGUI()
        {
            if (style == null) style = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = false };
            float cx = Screen.width / 2f, cy = Screen.height / 2f;
            GUI.Label(new Rect(cx - 5, cy - 11, 20, 22), "+", style);
            if (!showHud) return;
            string body =
                $"{mover.state}{(mover.grounded ? " grounded" : "")}   speed {mover.Speed:0.0} m/s   vy {mover.velocity.y:0.0}   pips {mover.pips:0.00}/{mover.MaxPips}   " +
                $"wall jumps {mover.wallJumpsLeft}   double {mover.doubleJumpsLeft}   apex {mover.lastApexHeight:0.00} m   hook: {hookText}\n" + hudText +
                (input.Locked ? "" : "\nCLICK TO PLAY") +
                "\nWASD move  SPACE jump  SHIFT dash  C slide/slam  G grapple (hold)  Z reel  R respawn  1-0 test spots  O grinder  P reshape grinder  F1 world  F2 loop  F3 HUD  V vsync  ESC free cursor";
            GUI.color = Color.black;
            GUI.Label(new Rect(11, 11, Screen.width, 200), body, style);
            GUI.color = Color.white;
            GUI.Label(new Rect(10, 10, Screen.width, 200), body, style);
        }
    }
}
