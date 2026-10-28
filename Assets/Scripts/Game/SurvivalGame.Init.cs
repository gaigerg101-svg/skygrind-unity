// NIGHT13 U3-26.5: SurvivalGame split by concern (audit-perf section 5 move 2): boot and the composition root (Awake, BootWeb, Init, Start, StartGame, MakeMaterials).
// NIGHT13 U3-26.6: Init builds the sim composition first (SurvivalSim: every Core system and every sim-affecting EventBus
// registration, in Init's old order), then the devices' and the views' parts in their old relative order. The views'
// handlers now follow all of the sim's (they read their payloads only; none changes the sim).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        void Awake()
        {
            Instance = this;
            devMode = DevModeFor(Environment.GetCommandLineArgs());
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(BootWeb());
#else
            string root = dataRoot = GameConfig.PortExportDir(Path.GetDirectoryName(Application.dataPath), Application.streamingAssetsPath, Application.isEditor);
            GameConfig config = GameConfig.Load(Path.Combine(root, "config.json"));
            foreach (string line in Rows72.Apply(config, Environment.GetCommandLineArgs())) Debug.Log(line); // NIGHT72 D-3: the night's rows from the command line, before anything reads the config
            MovementTuning71.Apply(config); // GG 2026-10-26: the jump is 50% higher in the Unity game (design.movementTuning; the web's config stays exact)
            Bright71.Create(config, gameObject); // GG 2026-10-26: "the boss rooms are very dark": the ambient / fog floor, last writer each frame (view only)
            // NIGHT15 U3-28.5: the map TITLE picked (SurvivalGame.Maps.cs); automation always boots the old arena.
            mapList = MapList(config, root);
            N26DefaultCampaign(config); // NIGHT26 (the lead): CAMPAIGN is the default mode (design.n26.levelSelect)
            MapEntry boot = BootMap(mapList);
            Init(config, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(boot.file))));
            sim.setup.mapId = map.name;
            if (PendingMode != null) // NIGHT24: the MODE row's reload (CAMPAIGN <-> the survival maps) boots in the mode that was picked
            {
                sim.setup.mode = PendingMode;
                PendingMode = null;
            }
            if (AutoStartCampaign25) // NIGHT25: CONTINUE on the campaign RESULTS reloaded the scene on the next level: its run starts at once
            {
                AutoStartCampaign25 = false;
                // NIGHT29 item 1 (GG: the level select "brings me back to the home menu"): Start's StartGame ends with ToTitle(), which
                // undid a StartRun made here in Awake. With the fix on, the start waits until StartGame has built the views (below).
                if (N29Fixes.LevelSelectDeferStart) autoStart29Pending = true;
                else StartRun();
            }
#endif
        }

        /// <summary>
        /// NIGHT8 U2-14: a WebGL Player can't read StreamingAssets as files (it's a URL there): fetch config.json, the design
        /// config and the map with UnityWebRequest, then build the game. The screen stays dark meanwhile (a short load).
        /// </summary>
        System.Collections.IEnumerator BootWeb()
        {
            string baseUrl = Application.streamingAssetsPath;
            var texts = new string[3];
            string[] paths = { "PortExport/config.json", "Design/unity-config.json", "PortExport/maps/survival.json" };
            for (int i = 0; i < paths.Length; i++)
            {
                using var req = UnityEngine.Networking.UnityWebRequest.Get($"{baseUrl}/{paths[i]}");
                yield return req.SendWebRequest();
                if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[skygrind] could not load {paths[i]}: {req.error}");
                    if (i != 1) yield break; // the design config is optional; the others aren't
                }
                else texts[i] = req.downloadHandler.text;
            }
            dataRoot = baseUrl;
            Init(GameConfig.FromText(texts[0], texts[1]), SurvivalMap.FromExport(PortData.Obj(JsJson.Parse(texts[2]))));
            StartGame();
        }

        void Init(GameConfig config, SurvivalMap survivalMap)
        {
            cfg = config;
            map = survivalMap;
            MovementConfig mc = cfg.movement;
            Time.fixedDeltaTime = (float)(1 / mc.loop.simHz);
            Time.maximumDeltaTime = (float)mc.loop.maxFrameDelta;
            lab = new Lab(cfg); // before the settings: its toggles are rows of the LAB tab
            bool vsyncSaved54 = Move54VsyncSaved(); // NIGHT54 M move54 (SurvivalGame.Move54.cs): read before the store's first save
            settings = new GameSettings(this);
            Move54Vsync(vsyncSaved54); // NIGHT54 M move54: VSYNC on by default with the row on (a saved choice wins)
            settings.Apply(); // the saved settings into the movement settings, the frame cap, vsync, fullscreen
            FeelAB.Boot(this); // NIGHT18: the A / B switch (on, or -skygrindFeelAB off); the LAB row shows it
            Physics.simulationMode = SimulationMode.Script; // nothing is simulated by PhysX (NIGHT7 rule 4)
            // NIGHT12: records live in the JSON profile, migrated once from PlayerPrefs (the old key is left as it was).
            profilePath = Path.Combine(Application.persistentDataPath, Profile.FileName);
            profile = Profile.Load(profilePath);
            if (profile.MigrateRecords(PlayerPrefs.GetString(Records.StorageKey, ""))) SaveProfile();
            input = new InputReader(mc);
            // NIGHT13 U3-26.7: the live bindings (bindings.js; the web's storage key), the readers on them, the pad and its rumble.
            bindings = InputPrefs.LoadBindings(cfg);
            input.bindings = bindings;
            Ui.LiveBindings = bindings; // NIGHT13 review: the shown keys follow a rebind
            combatReader.Init(cfg, bindings);
            padReader = new PadReader(cfg) { layout = InputPrefs.LoadPadLayout() };
            rumble = new Rumble(cfg);
            playerInput = new ReaderInput(input, combatReader, padReader, rumble);
            playerInput.events = events; // NIGHT24 (pad-y-weapons): the weaponSwitch event
            playerInput.ApplySettings(settings.store);
            inputSettingsVersion = settings.version;
            padOnHint = cfg.Has("ui.notices.padOn") ? cfg.Str("ui.notices.padOn") : null;
            padOddHint = cfg.Has("ui.notices.padOdd") ? cfg.Str("ui.notices.padOdd") : null;
            // NIGHT13 U3-26.6: the sim composition (the Core systems in Init's old order; OnScreen, the settings and the key
            // labels injected; the profile written through SaveProfile; the cursor freed when a run ends).
            sim = new SurvivalSim(cfg, map, events, lab, playerInput, profile, settings.Num, settings.Bool, settings.Str, OnScreen, KeyLabel)
            {
                saveProfile = SaveProfile,
                runEnded = FreeCursor,
                profBegin = ProfBegin,
                profEnd = ProfEnd,
                assistModeFor = padReader.AimMode, // NIGHT13 U3-26.7: SNAP MODE (PAD) while the pad plays
                breakawayCap = padReader.BreakawayCap, // and the pad's breakaway cap (+infinity on the mouse)
            };
            playerInput.mover = mover; // hold / toggle reads the rope's phase
            Debug.Log("[ultra] " + ultra.Describe());

            mats = MakeMaterials();
            GameObject mapRoot = SurvivalBuilder.Build(cfg, map, mats);
            coreMesh = new CoreMesh(cfg, core, mapRoot.transform, mats.stone);
            zonesView = new ZonesView(zones, mapRoot.transform, mats.field);
            if (!N24Blocks.ForceOff && N24Blocks.From(cfg).lighting) CampaignLighting24.Create(this, mapRoot.transform, mats); // NIGHT24 (builder L): the room lighting kit
            SlowAim27.Create(this, mapRoot.transform); // NIGHT27 slowAim27 (the lead; builder V's cue): null when the row is off
            padRings = mapRoot.GetComponentsInChildren<PadMarker>();
            rankWorld = new RankWorld(this, mapRoot.transform); // NIGHT17 U3-30.10

            cam = Camera.main;
            if (cam == null) cam = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            cam.nearClipPlane = (float)mc.camera.near;
            cam.farClipPlane = (float)mc.camera.far;
            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional) Destroy(l.gameObject); // Look makes the key
            Look.Apply(cfg, mats.sky, cam);

            feel = new CameraFeel(mc, mover);
            feel.Wire(events, cfg); // NIGHT12 U3-25.4: the aim-lock pinch, the wall-ride pre-tilt, the speed FOV cap
            viewmodel = new Viewmodel(this);
            telegraphView = new TelegraphView(this);
            wallBuys = new WallBuys(this); // the ARMORY wall's look (its buyables are the sim's)
            if (box != null) boxView = new MysteryBoxView(this, box);
            perkMachines = new PerkMachines(this); // the perk machines' look (their buyables and perks.apply are the sim's)
            powerUpView = new PowerUpView(this);
            huskView = new HuskView(this);
            swarmerView = new SwarmerView(this);
            spitterView = new SpitterView(this);
            N19ViewsInit(); // NIGHT19: the new kinds' views and the ported guns' effects (SurvivalGame.N19.cs)
            N20ViewsInit(); // NIGHT20: the climb view, the new kinds' views, the speed lines and the shockwave's look (SurvivalGame.N20.cs)
            particles = new Particles(cfg, mats.neon);
            particles.QualityLevel = () => lightAndLens != null ? lightAndLens.Level : null; // polish #50: the preset's particle cap (one closure, made once)
            killConfirm = new KillConfirm(this);
            projectiles.burst = (at, hex, count, speed, life, size) => particles.Burst(at, Look.Hex(hex), count, (float)speed, (float)life, (float)size); // projectiles.js particles.burst: `size` is particles.js's cube scale (:61 / :135; polish #41)
            projectileViews = new ProjectileViews(this);
            impactFx = new ImpactFx(this, particles); // NIGHT9 U3-09: the impact wave's ring (simple tonight; the full look is N12)
            weaponFx = new WeaponFx(this); // NIGHT10 U3-13.6: weaponFx.js (the rail's laser, dot and beam flash; the ricochet line)
            movementFx = new MovementFx(this);
            decals = new Decals(this); // NIGHT10 U3-13.6: effects.js decals on hitscan shotResults
            // NIGHT11: the run shape's views (the systems are the sim's).
            traps.burst = (at, hex, n, sp, life, size) => particles.Burst(at, Look.Hex(hex), n, (float)sp, (float)life, (float)size); // `size`: particles.js's cube scale (polish #41)
            ringView = new ObjectiveRingView(this, objectives);
            relayViews = new RelayViews(this, protocol);
            trapViews = new TrapViews(this, traps);
            powerView = new PowerSwitchView(this, power);
            radioView = new ExfilRadioView(this, exfil);
            siteView = new ExfilSiteView(this, exfil);
            beaconView = new Beacon20View(this, sim.beacon20); // NIGHT20 A3 (builder P2)
            spawnTell33View = new SpawnTell33View(this); // NIGHT33 E: draws enemySpawnTell33 (nothing shows with spawns33 off: no event)
            stillArmor33View = new StillArmor33View(this, particles); // NIGHT33 E: the deflect burst and the shell (nothing shows with bosses33 off)
            arclineView = new Arcline21View(this); // NIGHT21 U3-35.3 (builder M)
            N26InitViews(); // NIGHT26 (the shared commit): the NIGHT26 world views (SurvivalGame.N26.cs hooks)
            N25InitViews(); // NIGHT25 (the shared commit): the NIGHT25 world views (SurvivalGame.N25.cs hooks)
            N24InitViews(); // NIGHT24 (the shared commit): the NIGHT24 world views (SurvivalGame.N24.cs hooks)
            N22InitViews(); // NIGHT22 (the shared commit): the NIGHT22 world views (SurvivalGame.N22.cs hooks)
            Color enemyColor = Look.C(cfg, "colors.enemy");
            // NIGHT12 (a review): the burst and trauma numbers read once (PORT-RULES section 6: a cfg lookup on the hit / kill
            // path allocates through GameConfig.Get's path split).
            int hitCount = cfg.Int("fx.hitCount"), deathCount = cfg.Int("fx.deathCount");
            float hitSpeed = (float)cfg.Num("fx.hitSpeed"), hitLife = (float)cfg.Num("fx.hitLife"), deathSpeed = (float)cfg.Num("fx.deathSpeed"), deathLife = (float)cfg.Num("fx.deathLife");
            // NIGHT50 G4 (DECIDE50 D-7): under fx49 the web's enemy death burst is drawn at UnityTuning.Fx49KillBurstScale of its size (the kit's kill
            // burst carries the read); read once here, never per kill. Off = the web's size.
            float killScale = NightRows.ViewOn(cfg, "fx49") ? UnityTuning.Fx49KillBurstScale : 1f;
            double traumaKill = cfg.Num("feel.trauma.kill"), traumaSlam = cfg.Num("feel.trauma.slam");
            wallBuyOut = cfg.Num("wallBuy.buyOut");
            events.On<HitEvent>("hit", e =>
            {
                if (!e.killed) particles.Burst(e.position, enemyColor, hitCount, hitSpeed, hitLife);
            });
            // NIGHT16 polish #58: OVERLOAD's picture, a stand-in for the web's lightning arcs (gunFx.js :96-102; the arc pool is for
            // later): a volt-family burst (weapons.families.volt) at the struck target and at each enemy the burst reaches
            // (ArsenalEvent.to, read-only), with the STITCHER detonate's numbers (projectiles.js :306, UnityTuning). No flash,
            // no light (D-71).
            Color voltColor = Look.Hex((uint)cfg.Num("weapons.families.volt"));
            events.On<ArsenalEvent>("overload", e =>
            {
                if (!FxGates.OverloadBurst(e)) return;
                particles.Burst(e.position, voltColor, UnityTuning.OverloadBurstCount, UnityTuning.OverloadBurstSpeed, UnityTuning.OverloadBurstLife, UnityTuning.OverloadBurstScale);
                for (int i = 0; i < e.to.Count; i++)
                    particles.Burst(e.to[i], voltColor, UnityTuning.OverloadBurstCount, UnityTuning.OverloadBurstSpeed, UnityTuning.OverloadBurstLife, UnityTuning.OverloadBurstScale);
            });
            // NIGHT16 polish #52 (gunFx.js :120-125): where a melee hit of mine lands, config.weaponFx.sparkCount sparks in the
            // weapon's family colour (the GAUNTLET: kinetic cyan; the punch has no catalog entry: colors.spark), at
            // weaponFx.sparkScale (polish #41's cube scale), read once
            int meleeSparkCount = cfg.Int("weaponFx.sparkCount");
            float meleeSparkSpeed = (float)cfg.Num("weaponFx.sparkSpeed"), meleeSparkLife = (float)cfg.Num("weaponFx.sparkLife"), meleeSparkScale = (float)cfg.Num("weaponFx.sparkScale");
            Color meleeSparkFallback = Look.C(cfg, "colors.spark");
            events.On<HitEvent>("hit", e =>
            {
                if (!FxGates.MeleeSparks(e)) return;
                particles.Burst(e.position, WeaponLook.FamilyColor(cfg, e.weapon, meleeSparkFallback), meleeSparkCount, meleeSparkSpeed, meleeSparkLife, meleeSparkScale);
            });
            events.On<KillEvent>("kill", e =>
            {
                // NIGHT16 polish #53: the burst at the web's height: husk.js :107 at aimHeight (the KillEvent's AimPoint, as
                // before), enemyKind.js :67 (the swarmer, the spitter) at the body's middle, height x 0.5 (they burst at their
                // aim height, 7.5 cm / 10 cm too high); read-only, the emit is synchronous so the dead enemy is still listed
                Vec3 at = e.position;
                List<Enemy> all = enemies.all;
                for (int i = 0; i < all.Count; i++)
                {
                    Enemy en = all[i];
                    if (en.id != e.enemy) continue;
                    at = new Vec3(e.position.x, FxGates.DeathBurstY(en.kind, en.position.y, en.Height, e.position.y), e.position.z);
                    break;
                }
                particles.Burst(at, enemyColor, CrunchMath.DeathBurstCount(deathCount, UnityTuning.CrunchChunksOn, FeelAB.On), deathSpeed, deathLife, killScale); // NIGHT19 (builder V): horde5's confetti cut when the chunks carry the kill; NIGHT50 G4: killScale (1 with fx49 off)
            });
            // NIGHT8 U2-06: impacts (effects.js shotResult): a world hit bursts fx.sparkCount sparks in the gun family's
            // colour (weapons.families[catalog.<w>.family], else colors.spark), fx.surfaceOffset off the surface.
            int sparkCount = cfg.Int("fx.sparkCount");
            float sparkSpeed = (float)cfg.Num("fx.sparkSpeed"), sparkLife = (float)cfg.Num("fx.sparkLife");
            float sparkSize = (float)cfg.Num("fx.sparkSize"); // effects.js :186 the sparks' cube scale (polish #41)
            double surfaceOffset = cfg.Num("fx.surfaceOffset");
            double shotgunRange = cfg.Num("weapons.shotgun.range"); // polish #44: a THUNDERCLAP that hit nothing ends here
            Color sparkFallback = Look.C(cfg, "colors.spark");
            events.On<ShotEvent>("shotResult", e =>
            {
                if (e.surface != "world" || e.rayCount == 0) return; // effects.js shotResult: no burst for a projectile's hit
                if (FxGates.ShotgunMissedAll(e, shotgunRange)) return; // polish #44: no sparks in mid-air when no pellet hit anything
                Color c = WeaponLook.FamilyColor(cfg, e.weapon, sparkFallback); // NIGHT13: the one family colour lookup (cached per gun)
                particles.Burst(e.end + e.normal * surfaceOffset, c, sparkCount - 1, sparkSpeed, sparkLife, sparkSize); // effects.js :186: sparkCount - 1 sparks at fx.sparkSize beside the impact flare (WeaponFx, NIGHT12)
            });
            // NIGHT8 U2-06: the trauma shake (player.js): a shot adds feel.trauma.fire[its weight class], a kill and a slam
            // theirs; stepped and sampled on frame time, drawn only (CameraFeel), x effects intensity, none with reduced motion.
            traumaP = Trauma.Params.From(cfg);
            var fireTrauma = new Dictionary<string, double>();
            events.On("fire", o =>
            {
                string w = (string)o;
                if (!fireTrauma.TryGetValue(w, out double t))
                {
                    string cls = cfg.Has($"feel.punch.weapons.{w}.cls") ? cfg.Str($"feel.punch.weapons.{w}.cls") : null;
                    fireTrauma[w] = t = cls != null && cfg.Has($"feel.trauma.fire.{cls}") ? cfg.Num($"feel.trauma.fire.{cls}") : 0;
                }
                trauma.Add(t, traumaP.cap);
            });
            events.On<KillEvent>("kill", e =>
            {
                if (e.owner == "player") trauma.Add(traumaKill, traumaP.cap);
            });
            events.On("slamUse", _ => trauma.Add(traumaSlam, traumaP.cap));
            // NIGHT13 U3-26.7 (keyboardMouse.js rumble; main.js :1331-1340): the pad rumbles on your hurt, kill and parry (VIBRATION, off by default).
            events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner == "player" && e.amount > 0) rumble.Hurt();
            });
            events.On<KillEvent>("kill", e =>
            {
                if (e.owner == "player") rumble.Kill();
            });
            events.On<HitEvent>("parry", e =>
            {
                if (e.owner == "player") rumble.Parry();
            });
            viewmodel.SetFinishSource(mastery.FinishFor); // NIGHT12 U3-25.10: a gun wears its finish in hand and on the wall
            wallBuys.SetFinishSource(mastery.FinishFor);
            lightAndLens = new LightAndLens(this); // after Look.Apply, the machines, the box and the run's shape exist
            QualityRuntime.Boot(this); // NIGHT13 U3-26.4: the perf switches (logged), the GC slice, quality.js's AUTO / render scale / shadows
            InitSave(); // NIGHT13 U3-26.10: design.save and the saved run's file
            ViewRespawn();
        }

        void Start()
        {
            if (cfg != null && !ready) StartGame(); // a WebGL boot calls StartGame once its data has loaded
        }

        void StartGame()
        {
            // The run itself is the sim's (built with it); the HUD, the title and the signs once everything exists.
            ui = new Ui(this);
            FeelHintsInit(); // NIGHT16 polish #17: the slide charge's first-time hint (SurvivalGame.FeelHints.cs)
            titleView = new TitleView(this);
            signs = new ZoneSigns(this);
            RunLog.Attach(this); // NIGHT13 U3-26.12: the local run log (its handlers last; off for automation and with RUN LOG off)
            ToTitle();
            BootNight14(); // NIGHT14: the tape, the recap, the route, the kiosk (SurvivalGame.Night14.cs)
            Warmup.Run(); // NIGHT13 U3-26.4: the recorded shader / PSO warm-up (none without the file)
            FirstShot53.Boot(this); // NIGHT53 U polish53 (T-122): the fx49 kit built and the first shot's path compiled at boot (Polish53.cs)
            BossGrade54.Attach(this); // NIGHT54 G grade54: each boss's grade, light layer and turns (BossGrade54.cs; nothing built or touched with the row off)
            BossGrade55.Attach(this); // NIGHT55 B grade55: THE WICK's / THE CROWN's shadows-only grade, light layer and turns (BossGrade55.cs; nothing with the row off)
            Debug.Log($"[skygrind] Survival loaded: {map.zones.Count} zones, {zones.nodes.Count} spawn nodes, {nav.grid.count} nav nodes, data from {dataRoot}");
            ready = true;
            if (!Application.isEditor) PlayerChecks.Attach(this);
            if (autoStart29Pending) // NIGHT29 item 1: the level select's reload starts its run after the title the boot built
            {
                autoStart29Pending = false;
                StartRun();
            }
        }

        bool autoStart29Pending;

        SurvivalMaterials MakeMaterials()
        {
            Material Copy(Material m, string fallbackShader) => m != null ? new Material(m) : new Material(Shader.Find(fallbackShader));
            var m = new SurvivalMaterials
            {
                stone = Copy(stoneMaterial, "SKYGRIND/Stone"),
                neon = Copy(neonMaterial, "Universal Render Pipeline/Unlit"),
                field = Copy(fieldMaterial, "SKYGRIND/Field"),
                boundary = Copy(boundaryMaterial, "SKYGRIND/Field"),
                lit = Copy(litMaterial, "Universal Render Pipeline/Lit"),
                sky = Copy(skyMaterial, "SKYGRIND/Sky"),
                acid = Copy(acidMaterial, "SKYGRIND/Acid"),
            };
            m.stone.SetFloat("_GridMinor", (float)cfg.Num("world.gridMinor"));
            m.stone.SetFloat("_GridMajor", (float)cfg.Num("world.gridMajor"));
            m.stone.SetFloat("_Plate", (float)cfg.Num("world.trim.plate"));
            m.neon.SetColor("_BaseColor", Look.C(cfg, "colors.usable", (float)cfg.Num("world.neonBase") * UnityTuning.NeonHdr));
            m.field.SetColor("_Color", Look.C(cfg, "barrier.gateColor", UnityTuning.FieldHdr));
            m.field.SetFloat("_FarAlpha", (float)cfg.Num("barrier.gateFarAlpha"));
            m.field.SetFloat("_StrongWithin", (float)cfg.Num("barrier.strongWithin"));
            m.field.SetFloat("_ClearWithin", (float)cfg.Num("barrier.clearWithin"));
            m.field.SetFloat("_FadeTop", (float)cfg.Num("barrier.gateFadeTop"));
            m.field.SetFloat("_FadeLength", (float)cfg.Num("barrier.gateFadeLength"));
            m.field.SetFloat("_Stripe", (float)cfg.Num("barrier.stripeWidth"));
            m.boundary.SetColor("_Color", Look.C(cfg, "barrier.boundaryColor"));
            m.boundary.SetFloat("_FarAlpha", (float)cfg.Num("barrier.farAlpha"));
            m.boundary.SetFloat("_StrongWithin", (float)cfg.Num("barrier.strongWithin"));
            m.boundary.SetFloat("_ClearWithin", (float)cfg.Num("barrier.clearWithin"));
            m.boundary.SetFloat("_FadeTop", (float)cfg.Num("barrier.fadeTop"));
            m.boundary.SetFloat("_FadeLength", (float)cfg.Num("barrier.fadeLength"));
            m.boundary.SetFloat("_Stripe", (float)cfg.Num("barrier.stripeWidth"));
            m.boundary.SetFloat("_Doorway", 0); // NIGHT13 S3: the doorway look (Field.shader) is the gate's; the boundary keeps the old sheet
            return m;
        }
    }
}
