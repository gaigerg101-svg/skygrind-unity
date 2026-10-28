// SKYGRIND Survival in Unity: the bootstrap and the frame loop (NIGHT7 section 3). Loads the web game's config and map
// (PortExport), builds the sim composition (NIGHT13 U3-26.6: SurvivalSim owns the run's Core systems, their wiring and
// the step in main.js simulate() order), the map's look, the devices and every view, then runs the web game's loop
// (main.js frame()): the frame delta clamped to loop.maxFrameDelta, whole 1/simHz steps from an accumulator in Update
// (after the Input System's events), hitstop that freezes the sim and skips its time (main.js hitstop: the longest
// request wins, never added up), render interpolated by alpha = accumulator / step.
// The flow (main.js flow + stepMode): TITLE -> PLAYING (Esc: PAUSED) -> DOWN (health.downTime) -> RESULTS -> a new run.
// A restart rebuilds the run's state, not the scene. The fields the views read forward to the sim (no view changed).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public enum Flow { Title, Playing, Paused, Down, Results }

    public sealed partial class SurvivalGame : MonoBehaviour
    {
        [Header("Materials (SurvivalSetup creates these; they keep their shaders in builds)")]
        public Material stoneMaterial;
        public Material neonMaterial;
        public Material fieldMaterial;
        public Material boundaryMaterial;
        public Material litMaterial;
        public Material skyMaterial;
        public Material acidMaterial;

        public GameConfig cfg;
        public SurvivalMap map;
        /// <summary>NIGHT13 U3-26.6: the sim composition (every Core system, the step, the run's flow). The fields below forward to it.</summary>
        public SurvivalSim sim;
        public readonly EventBus events = new EventBus();
        public InputReader input;
        public CombatReader combatReader = new CombatReader();
        /// <summary>NIGHT13: the devices as the sim's input (InputReader + CombatReader, the automation overrides).</summary>
        public ReaderInput playerInput;
        /// <summary>NIGHT13 U3-26.7: the live key bindings (bindings.js; saved on change), the pad (gamepad.js through PadReader), its rumble.</summary>
        public Bindings bindings;
        public PadReader padReader;
        public Rumble rumble;
        int inputSettingsVersion = -1;
        string padOnHint, padOddHint;
        static readonly object PadOn = "connected", PadOff = "disconnected", DevicePad = "pad", DeviceKeyboard = "keyboard"; // payloads made once
        public Camera cam;
        public SurvivalMaterials mats;
        public GameSettings settings; // the player's settings (SETTINGS; PlayerPrefs)
        string assistBack = "snap"; // the mode the AIM ASSIST button turns back on
        // NIGHT8 U2-16: the LAB (design.lab): its toggles are settings, read at each run's start (Lab.Read / Apply).
        public Lab lab;
        // NIGHT12: the JSON profile (records, unlocks, contracts, mastery, HEAT, the DAILY, tips); the sim holds the same object.
        public Profile profile;
        string profilePath;
        /// <summary>Automation (the smoke): the profile may change in memory but is never written (the smoke restores it).</summary>
        public bool profileReadOnly;
        /// <summary>
        /// NIGHT17 U3-30.3 (NIGHT17.md rule 7): a developer build. On with -skygrindDev or any automation flag
        /// (UnityTuning.AutomationFlags); a Play check's eval may set it. It guards what only a developer acts on: COPY REPORT,
        /// COPY SCORE LINE, SAVE CARD, COPY SEED, the LAB tab, the version stamp, the dev overlay. Off for the reel's captures
        /// (-skygrindShots / -skygrindStrip / -skygrindReel aren't automation flags), so they show the player's screens.
        /// </summary>
        public bool devMode;

        /// <summary>NIGHT17: devMode from the command line (-skygrindDev as DevKeys reads it, or an automation flag). The Editor
        /// shows the player's screens (DevKeys' Editor default isn't taken): a Play check sets devMode when it needs it.</summary>
        public static bool DevModeFor(string[] args) => DevKeys.Allowed(args, false) || RunLogMath.IsAutomation(args);

        // --- the sim's state, forwarded (NIGHT13 U3-26.6) ----------------------------------------------------------------
        public GameWorld world => sim.world;
        public CoreGrid core => sim.core;
        public Zones zones => sim.zones;
        public Mover mover => sim.mover;
        public Health health => sim.health;
        public Regen regen => sim.regen;
        public Economy economy => sim.economy;
        public SurvivalRun run => sim.run;
        public IRoundSpawner spawner => sim.spawner;
        public EnemyRegistry enemies => sim.enemies;
        public Loadout loadout => sim.loadout;
        public Projectiles projectiles => sim.projectiles;
        public Ultra ultra => sim.ultra;
        public CombatInput lastCombat => sim.lastCombat;
        public Husks husks => sim.husks;
        public AimAssist assist => sim.assist;
        public NavField nav => sim.nav;
        public HuskBrain brain => sim.brain;
        public Director director => sim.director;
        public Buyables buyables => sim.buyables;
        public StyleMeter style => sim.style;
        public Players players => sim.players;
        public Player p1 => sim.p1;
        public Perks perks => sim.perks;
        public PowerUps powerUps => sim.powerUps;
        public Mods mods => sim.mods;
        public MysteryBox box => sim.box;
        public Objectives objectives => sim.objectives;
        public Protocol protocol => sim.protocol;
        public Traps traps => sim.traps;
        public RunMoments moments => sim.moments;
        public Power power => sim.power;
        public SwarmDrop swarmDrop => sim.swarmDrop;
        public Exfil exfil => sim.exfil;
        public UltraHealth ultraHealth => sim.ultraHealth;
        public Forge forge => sim.forge;
        public Flow flow { get => sim.flow; set => sim.flow = value; }
        public RunStats lastStats => sim.lastStats;
        public DeathRecap recap => sim.recap;
        public HitArcs hitArcs => sim.hitArcs;
        public DeathRecap.Summary lastDeath => sim.lastDeath;
        public string downTarget => sim.downTarget;
        public Records records => sim.records;
        public Records.Judgement lastPb => sim.PbShown; // NIGHT22 (builder W): in a GRIND RESULTS reads the GRIND's own personal-best line
        public bool noRecords { get => sim.noRecords; set => sim.noRecords = value; }
        public bool noLab { get => sim.noLab; set => sim.noLab = value; }
        public RunSetup setup => sim.setup;
        public Contracts contracts => sim.contracts;
        public Mastery mastery => sim.mastery;
        public Heat heat => sim.heat;
        public Tips tips => sim.tips;
        public Onboarding onboarding => sim.onboarding;
        public uint runSeed => sim.runSeed;
        public int simStep => sim.simStep;
        /// <summary>The running run may change the profile (not a LAB run, not automation). Valid once StartRun read the LAB.</summary>
        public bool Recording => sim.Recording;
        public bool Active => sim.Active;
        /// <summary>A MODS draft is open (the game paused under it).</summary>
        public bool DraftOpen => sim.DraftOpen;
        /// <summary>Fixed-step time since boot (the recap's clock, main.js simTime).</summary>
        public double SimTime => sim.SimTime;
        /// <summary>The sim's eye (feet + eye height, lowered while crouched), where shots and buys look from.</summary>
        public Vec3 Eye => sim.Eye;
        /// <summary>The player's aim: the sim eye and the look direction (yaw 0 looks toward -z, + pitch looks up).</summary>
        public (Vec3, Vec3) Aim() => sim.Aim();
        /// <summary>The assist mode in use: settings.aimAssist (off | assist | snap | lock).</summary>
        public string AssistMode => sim.AssistMode;
        /// <summary>Automation (the slice smoke, debug): replaces the player's combat input each step while set.</summary>
        public CombatInput? forcedCombat { get => playerInput.forcedCombat; set => playerInput.forcedCombat = value; }
        /// <summary>NIGHT12 automation (Play checks): replaces the movement input's axes and buttons each step while set (the look stays the player's).</summary>
        public InputFrame? forcedMove { get => playerInput.forcedMove; set => playerInput.forcedMove = value; }
        Vec3 previous => sim.previous;
        Vec3 current => sim.current;
        double hitstopLeft => Math.Max(sim.hitstopLeft, ImpactFreezeLeft); // NIGHT16: the longest freeze, the sim's or the impact layer's
        int downSteps => sim.downSteps;
        double step => sim.step;

        // --- the views and the frame -------------------------------------------------------------------------------------
        PerkMachines perkMachines;
        PowerUpView powerUpView;
        public readonly Trauma trauma = new Trauma(); // U2-06: the camera shake (fx/trauma.js)
        Trauma.Params traumaP;
        TelegraphView telegraphView;
        WallBuys wallBuys;
        public Viewmodel viewmodel;
        ObjectiveRingView ringView;
        RelayViews relayViews;
        TrapViews trapViews;
        PowerSwitchView powerView;
        ExfilRadioView radioView;
        ExfilSiteView siteView;
        Beacon20View beaconView; // NIGHT20 A3 (builder P2)
        SpawnTell33View spawnTell33View; // NIGHT33 E: the spawns33 spawn tell (SpawnTell33View.cs)
        StillArmor33View stillArmor33View; // NIGHT33 E: STILL ARMOR's sign (StillArmor33View.cs)
        Arcline21View arclineView; // NIGHT21 U3-35.3 (builder M; the lead's wiring): THE ARCLINE's rope, node glows and hook reticle (null with the row off)
        MysteryBoxView boxView;
        Ui ui;
        TitleView titleView; // NIGHT12 U3-25.6: the living title (design.ceremony.livingTitle)
        RankWorld rankWorld; // NIGHT17 U3-30.10: the map's neon climbs the cyan value ladder with the style rank
        ZoneSigns signs;
        HuskView huskView;
        SwarmerView swarmerView;
        SpitterView spitterView;
        Particles particles;
        KillConfirm killConfirm; // NIGHT12 U3-25.4: killConfirm.js
        public LightAndLens lightAndLens; // NIGHT12 U3-25.2: post.js on the Volume, light pools, the muzzle / pop lights, motes, blob shadows
        ImpactFx impactFx;
        ProjectileViews projectileViews; // NIGHT10: nails, stuck nails, rockets
        WeaponFx weaponFx;
        MovementFx movementFx; // NIGHT12 U3-25.4: land puffs, slam rings, wall-ride streaks and sparks, the dash edge tint
        Decals decals;
        /// <summary>This frame's clamped delta, and the world's (0 under hitstop, pause and the screens): views animate on WorldDt.</summary>
        public double FrameDt { get; private set; }
        public double WorldDt { get; private set; }
        Key restartKey, lockOnKey; // the live bindings' keys (HotKeys re-reads them when bindings.Version moves)
        int hotKeysVersion = int.MinValue;
        string dataRoot;
        double wallBuyOut; // read once at Init (the guide light)
        CameraFeel feel;
        CoreMesh coreMesh;
        ZonesView zonesView;
        PadMarker[] padRings;
        double accumulator;
        public bool debugOverlay;
        public static SurvivalGame Instance { get; private set; }

        /// <summary>The frame's hooks (views added later; automation).</summary>
        public event Action<double, double> OnRender; // frame time: (frameDt, alpha)
        public event Action OnNewRun;

        public Vec3 Feet => current;

        /// <summary>The HUD and the screens (automation: the smoke, Play checks).</summary>
        public Ui UiView => ui;

        public double Step => step;

        public double Alpha => accumulator / step;

        /// <summary>Is a world point inside the main camera's view (the watchdog relocates only off-screen)?</summary>
        public bool OnScreen(Vec3 p)
        {
            Vector3 v = cam.WorldToViewportPoint(WebSpace.ToUnity(p));
            return v.z > 0 && v.x >= 0 && v.x <= 1 && v.y >= 0 && v.y <= 1;
        }

        bool ready; // the data is loaded and the game built (a WebGL Player loads its JSON by URL first)

        /// <summary>settings.fpsCap (0: no cap, the web's default) and vsync (a Unity-only setting; the browser always syncs).</summary>
        public static void ApplyFrameCap(int fpsCap, bool vsync)
        {
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            Application.targetFrameRate = !vsync && fpsCap > 0 ? fpsCap : -1;
        }

        /// <summary>A web key code (KeyboardEvent.code: "KeyR", "Digit1", "ShiftLeft") as an Input System key (NIGHT13: KeyCodes' table; the old name parse made ShiftLeft Key.None).</summary>
        public static Key KeyOf(string code) => KeyCodes.ToKey(code);

        /// <summary>
        /// NIGHT13 U3-26.7 (keyboardMouse.js pollPad :397-408, main.js :1312-1340): this frame's pad commands (lockOn, xray,
        /// pause; the weapon cycle is applied by the poll) and the device events. The keyboard's lockOn / xray / restart keep
        /// their own reads (HotKeys, EnemyLook), so nothing toggles twice.
        /// </summary>
        void RoutePad()
        {
            PadCommands pc = padReader.commands;
            if (flow == Flow.Playing && !attractOn) // NIGHT14: not the attract's run (a pad press only stops it)
            {
                if (pc.lockOn) assist.ToggleLock();
                if (pc.xray) settings.Set("xray", !settings.Bool("xray"));
                if (pc.pause) Pause();
            }
            if (playerInput.DeviceChanged) events.Emit("input:device", PlayerDevices.Last == InputDevice.Pad ? DevicePad : DeviceKeyboard);
            if (padReader.ConnectedChanged)
            {
                events.Emit("input:padConnected", padReader.Connected ? PadOn : PadOff);
                string hint = padReader.Connected ? (padReader.OddPad ? padOddHint : padOnHint) : null;
                if (hint != null) ui?.ShowHint(hint);
            }
        }

        // --- the loop --------------------------------------------------------------------------------------------

        /// <summary>NIGHT13 U3-26.2 (the bench's per-frame breakdown; measurement only): this frame's sim steps, their ms, and Update's ms.</summary>
        public int StepsThisFrame { get; private set; }
        public double StepMsThisFrame { get; private set; }
        public double UpdateMsThisFrame { get; private set; }
        /// <summary>NIGHT18 (the reel's telemetry): the whole frame was frozen (a freeze counted down; no sim time passed).</summary>
        public bool HeldThisFrame { get; private set; }
        /// <summary>NIGHT18: the steps a freeze swallowed this frame (the frame's whole steps minus the steps run, while frozen or cut by a freeze).</summary>
        public int HeldStepsThisFrame { get; private set; }
        static readonly double TicksToMs = 1000.0 / System.Diagnostics.Stopwatch.Frequency;

        void Update()
        {
            if (!ready) return;
            long updateStart = System.Diagnostics.Stopwatch.GetTimestamp();
            int stepsRun = 0;
            long stepTicks = 0;
            bool wasLocked = input.Locked;
            bool wasFocused60 = FocusFrame60(out bool focused60); // NIGHT60 L B9: last frame's window focus and this frame's (SurvivalGame.N60L.cs)
            double frameDt = Math.Min(FeelClock.FrameDelta, cfg.movement.loop.maxFrameDelta); // NIGHT18: the reel's fixed clock while it records
            bool held63 = FeelClock.Holding; // NIGHT63 D1: a same-run held frame (FeelReel.N63Same.cs): no input read, no draft, no impact frame, no step
            if (!held63) using (rInput.Auto())
            {
                // NIGHT13 U3-26.7 (main.js :2444-2447): the devices polled in every flow state (the pad drives the menus too);
                // the keyboard and mouse readers, the pad's merge, its look and its weapon cycle only in game.
                if (settings.version != inputSettingsVersion)
                {
                    inputSettingsVersion = settings.version;
                    playerInput.ApplySettings(settings.store);
                }
                bool inGame = flow == Flow.Playing || flow == Flow.Down;
                if (inGame) input.sensMult = viewmodel.AdsSensitivity(cfg.movement.settings.fovDeg, settings.View.adsSensitivity) * assist.LookScale; // main.js setLookScale: friction x the zoom
                playerInput.Poll(frameDt, inGame, loadout.targetSlot, loadout.owned.Count);
                // NIGHT29 item 14: DOORFREE's hold is the raw RB button (the v5 layout's action is Punch) or F8, read here for the sim (SurvivalSim.Systems29.cs)
                var pad29 = UnityEngine.InputSystem.Gamepad.current;
                var kb29 = UnityEngine.InputSystem.Keyboard.current;
                sim.doorFreeHeld = inGame && (N29Fixes.ForceDoorFreeHold || (pad29 != null && pad29.rightShoulder.isPressed) || (kb29 != null && kb29.f8Key.isPressed));
                Hud29.DoorsOpenTag = sim.DoorFreeOn;
                // NIGHT29 item 13: THE GLACIER's gap bar (a distance, not text): -1 hides it
                bool gl29 = sim.n29.boss2 && sim.glacier29 != null && sim.glacier28 != null && sim.glacier28.Alive;
                Hud29.BossGapMetres = gl29 ? sim.glacier29.Gap : -1;
                Hud29.BossGapDanger = gl29 && sim.glacier29.Enraged;
                RoutePad();
                if (PausesOnLockDrop(flow, wasLocked, input.Locked, attractOn, FeelReel.Running)) Pause(); // NIGHT56 lead: the rule in SurvivalGame.N56Lead.cs (pinned by N56FocusPauseTests). Esc frees the cursor: the pause screen (not the attract's; NIGHT18: not inside the reel, where another window taking the focus must not pause a scenario)
                if (FocusPause60On && PausesOnFocusLoss63(flow, wasFocused60, focused60, input.Locked, attractOn, FeelReel.Running)) Pause(); // NIGHT60 L B9 (row focus60): in a Player the lock above never drops on a focus loss (SurvivalGame.N60L.cs); NIGHT63 V (T-146, the w60 review's S4): only with the cursor locked, so an unlocked Editor run never pauses
                if (!attractOn) HotKeys(); // NIGHT14: the key that stops the attract does nothing else (X-RAY, LOCK ON, R)
                FrameKiosk(frameDt); // NIGHT14 U3-27.7: the idle clock and the attract (SurvivalGame.Kiosk.cs)
            }
            // MODS: a waiting draft opens during a break only, pausing the game until a card is taken (mods.js / main.js).
            // NIGHT16 (the review's finding 3): after the round-clear beat (its freeze and slow-mo play first; under the draft they
            // waited and played after the pick, at the break's start, reading as lag).
            if (!held63 && flow == Flow.Playing && (run.state == SurvivalRun.State.Break || mods.LayerPick) && mods.Pending && hitstopLeft <= 0 && !(ImpactDirector != null && ImpactDirector.rules.SlowMoActive))
            {
                mods.Open();
                Pause();
            }
            bool simulating = flow == Flow.Playing || flow == Flow.Down;
            RecapGate(ref simulating); // NIGHT14 U3-27.9: WHAT GOT YOU holds DOWN's clock while it plays
            HeldThisFrame = false; // NIGHT18: the reel's telemetry (FeelProbe)
            bool freezeCut = false;
            if (simulating && !held63) // NIGHT63 D1: the held frame keeps the shot frame's freeze, slow-mo share (WorldScale), accumulator and alpha
            {
                // NIGHT16 U3-29.5: the impact layer's time scale (SurvivalGame.Impact.cs): 0 while a freeze (the sim's hitstop
                // or the impact layer's) counts down, under 1 in a slow-mo (the sim steps at 120 Hz as always, fewer per real
                // second), 1 otherwise. A press during a freeze waits in the input latches for the next step.
                double impactScale = ImpactBeginFrame(frameDt);
                HeldThisFrame = impactScale <= 0;
                if (impactScale > 0)
                {
                    accumulator += frameDt * impactScale;
                    long stepStart = System.Diagnostics.Stopwatch.GetTimestamp();
                    while (accumulator >= step)
                    {
                        sim.hitstopRequested = false;
                        Move54StepBegin(); // NIGHT54 M move54 (SurvivalGame.Move54.cs): the enemies' yaw before the step, read only (a no-op with the row off)
                        sim.Step();
                        Move54StepEnded(); // NIGHT54 M move54 (SurvivalGame.Move54.cs): the step's feet, read only (a no-op with the row off)
                        stepsRun++;
                        accumulator -= step;
                        if (ImpactStepEnded())
                        {
                            accumulator %= step; // the frozen time is skipped, not caught up afterwards
                            freezeCut = true;
                            break;
                        }
                    }
                    stepTicks = System.Diagnostics.Stopwatch.GetTimestamp() - stepStart;
                }
            }
            StepsThisFrame = stepsRun;
            HeldStepsThisFrame = HeldThisFrame || freezeCut ? Math.Max(0, (int)Math.Round(frameDt / step) - stepsRun) : 0;
            StepMsThisFrame = stepTicks * TicksToMs;
            double alpha = accumulator / step;
            FrameDt = frameDt;
            WorldDt = simulating && hitstopLeft <= 0 ? frameDt * WorldScale : 0; // NIGHT12: hitstop and pause apply (NIGHT12 rule 4); NIGHT16: x the slow-mo
            RenderFrame(frameDt, alpha, simulating); // the render half (SurvivalGame.Render.cs)
            UpdateMsThisFrame = (System.Diagnostics.Stopwatch.GetTimestamp() - updateStart) * TicksToMs;
        }

        // NIGHT8 U2-02: one profiler marker per sim system (the Profiler and the Development Player's bench read them);
        // NIGHT13: the sim marks them by id (SurvivalSim.P*), mapped here.
        static readonly ProfilerMarker[] simMarkers =
        {
            new ProfilerMarker("SKYGRIND.SimStep"), new ProfilerMarker("SKYGRIND.Mover"), new ProfilerMarker("SKYGRIND.Brain"), new ProfilerMarker("SKYGRIND.Assist"),
            new ProfilerMarker("SKYGRIND.Loadout"), new ProfilerMarker("SKYGRIND.Regen"), new ProfilerMarker("SKYGRIND.Buyables"), new ProfilerMarker("SKYGRIND.Husks"),
            new ProfilerMarker("SKYGRIND.Crowd"), new ProfilerMarker("SKYGRIND.Style"), new ProfilerMarker("SKYGRIND.AfterPlayer"), new ProfilerMarker("SKYGRIND.Mode"),
        };

        static void ProfBegin(int id) => simMarkers[id].Begin();

        static void ProfEnd(int id) => simMarkers[id].End();
    }
}
