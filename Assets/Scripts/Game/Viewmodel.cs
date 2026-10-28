// The first-person viewmodel and the shot feedback, after skygrind/src/player/viewmodel.js + player.js (the parts
// ported tonight): an overlay camera in the URP stack at config.viewmodel.fovDeg drawing only the viewmodel layer; the
// kit KESTREL and PULSE GAUNTLET (BlockyKit) at the hold pose (viewmodel.hold*), easing to the ADS pose (viewmodel.ads);
// the gun's recoil on a closed-form spring bank (viewmodel.recoil.<weapon>, recoilSprings by weight class); the swap
// drop (swapDropY / swapPitchDeg x the loadout's lowered amount); the gauntlet's jab (viewmodel.swing.gauntlet); the gem
// flashing in a parry window (NIGHT12: the tracer moved to WeaponFx; NIGHT17: the plain 2-frame muzzle quad went too, the
// muzzle flash is WeaponFx's per-gun star at MuzzleWorld, the muzzle light LightPools'). The camera punch (feel.punch:
// per-weapon pattern, spring by class, x adsMult zoomed) and the ADS zoom (ads.weapons.<w>.zoom, eased by
// ads.classEase) are returned to the camera.
// NIGHT10: the kit STITCHER, COMET and LONGWAVE (BlockyKit) at viewmodel.kitOffsets.<w>, with their moving parts and
// glows (viewmodel.js animateParts :424-448: the nailgun's heat strip and drum, the rail's charge and ring, the
// rocket's ring).
// NIGHT12 U3-25.5, the viewmodel's life (viewmodel.js update() :730-933, in its order): idle breathing; the movement
// bob (grounded speed, x effects intensity); the look sway (the look delta feeds a decaying velocity, the offset eases to
// it, clamped, with its roll; heavy guns lag more: recoilSprings.<class>.swayGain / swayReturn); the strafe roll, the
// slide dip and roll, the wall tilt (away from the wall), the air lift, the dash pull (a kick on `dash`), the land dip
// (the camera's landing dip x landDipScale); the swap and the respawn raise on one smoothed lowered curve, with the
// settle when the gun lands in the hands; the wall pullback (a wall right ahead: the eye ray, viewmodel.wallPull); the
// KESTREL's cylinder stepping a chamber per shot (cylinderStepDeg in cylinderTurnTime); the THUNDERCLAP's pump
// (pumpTravel over pumpDuration with pumpTiltDeg, weapons.shotgun.pumpDelay after the shot); the gem's FRESH dimming and
// fire flash (gemFreshMin, gemFlash, gemFlashTime); trauma's share (feel.trauma.vmShare of the drawn shake); the ADS
// pose steadying every layer (ads.vmSwayMult); the melee afterimages (viewmodel.afterimages: fading additive copies of
// the gauntlet along its swing; none with reduced motion). The web's split of time is kept: the look sway and the
// landing dip run on frame time, every other layer on the world's (SurvivalGame.WorldDt: frozen in a hitstop).
// The camera punch is drawn as player.js draws it: soft-capped per class, x effects intensity, none with reduced motion,
// x feel.punch.adsMult by the zoom amount, with its lattice jitter.
// NIGHT16 U3-29.6: a draw for every gun inside the sim's swap timer (Viewmodel.Draw.cs: the holster tips to the hip, the
// raise is up on the step fire opens, each gun's signature lives in the settle and a shot cancels it; the run start and
// the respawn raise are draws; weaponDraw); the left arm's punch (viewmodel.arm, Viewmodel.Arm.cs, with the port-only
// knuckle flash on a parry); the cooldown-ready glint and its weaponReady event (viewmodel.js :845-862).
// NIGHT16 U3-29.8: the momentum hit's hold spring (viewmodel.js :654-670 / :740 / :875-878 / :956, HandsImpactHold: frame
// time, so the drive plays through the hitstop) and the forge flourish (:658-663, HandsForgeFlourish: a kick on the gun in
// hand's spring, a gem flash through the game's FlashGate x flashIntensity), both in Viewmodel.Arm.cs.
// NIGHT16 polish #42: the STEADY perk's recoil multiplier (perks.fx.recoil x the kick and the camera punch; STEADY itself is not
// buyable in the port yet: SurvivalSim.cs AddMachines, Perks.Ported). Not ported: the HUD offset (hudAttach), the GLB rigs.
// NIGHT19 (builder V): the ZAPPER and the BOUNCER from weaponModels.js (Viewmodel.Ports.cs), the KESTREL's gust in the hands
// (Viewmodel.Gust.cs), every gun's alt-fire and the R3 jab (Viewmodel.Alt19.cs), on Spring-It-On's half-life springs
// (Viewmodel.Springs.cs, MIT).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    public sealed partial class Viewmodel
    {
        public const int Layer = 30;
        const double Deg = Math.PI / 180;
        readonly SurvivalGame game;
        readonly GameConfig cfg;
        readonly Camera vmCam;

        /// <summary>The overlay camera that draws the viewmodel layer (PostFx sets its Volume update mode, NIGHT13).</summary>
        public Camera Camera => vmCam;
        readonly Transform root;
        readonly Dictionary<string, Rig> rigs = new Dictionary<string, Rig>();
        readonly List<Rig> rigList = new List<Rig>();
        readonly Dictionary<string, Gun> guns = new Dictionary<string, Gun>();
        readonly SpringBank recoil;
        readonly HandsRecoilClock recoilClock = new HandsRecoilClock(); // NIGHT16 (the reviewer): the kick's attack lands inside its own shot's freeze
        readonly SpringBank punch;
        readonly Material kit;
        double adsAmount;

        /// <summary>The zoom's eased amount (0..1, frame time): what the FOV is drawn with (UiAds reads it; read-only).</summary>
        public double AdsAmount => adsAmount;
        double clock; // frame time (the punch pattern's reset: player.js feelClock)
        double lastShotAt = -99;
        int patternShot, shotCount;
        double punchCap;
        public double PunchPitchRad, PunchYawRad;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        // viewmodel.js animateParts' numbers (config.viewmodel / config.forge), read once
        readonly Color heatColor;
        readonly double heatGlow, chargeGlow, drumStepRad, drumTurnRate, rocketKickDeg, forgeGlow, forgeGem;
        readonly P v;
        readonly Trauma.Params traumaP; // the shake the camera draws (feel.trauma), sampled again for the gun's share

        // --- the life's state (viewmodel.js :537-553) ---------------------------------------------------------------
        readonly KickStack dashPull;
        double time; // the breathing clock (world time)
        double bobPhase, bobAmp;
        double swayX, swayY, swayVx, swayVy;
        double lastYaw = double.NaN, lastPitch = double.NaN;
        double strafeRoll, slideW, wallSide, airW; // rad / 0..1 / -1..1 / 0..1
        double wallW; // 0..1 the wall pullback
        double dipAmount, dipTime = double.PositiveInfinity; // the camera's landing dip, kept here too (player.js landingDip)
        double pumpIn; // s until the THUNDERCLAP's pump (weapons.shotgun.pumpDelay after its shot)

        // --- the melee afterimages (viewmodel.js :582-633) ------------------------------------------------------------
        readonly Material[] ghostMats;
        readonly RenderParams[] ghostParams;
        readonly Matrix4x4[] ghostHistory; // camera-space rig matrices, newest first (a ring)
        int ghostHead, ghostCount;
        double ghostAge = double.PositiveInfinity, ghostSample;
        Rig ghostRig;
        Matrix4x4[] ghostLocals = new Matrix4x4[0];

        /// <summary>config.viewmodel (and the few feel / movement numbers the layers read), read once (PORT-RULES 6).</summary>
        sealed class P
        {
            public double breathAmp, breathPeriod, bobAmpX, bobAmpY, bobCycleDistance, bobMaxScale, bobEaseRate;
            public double swayGain, swayVelDamp, swayReturnRate, swayMaxX, swayMaxY, swayRollDegPerM, strafeRollDeg, poseEaseRate;
            public double slideDipY, slideRollDeg, wallTiltDeg, airLiftY, dashPullZ, dashPullAttack, dashPullRecover, landDipScale;
            public double cylinderStepDeg, cylinderTurnTime, pumpTravel, pumpDuration, pumpTiltDeg, pumpDelay;
            public double swapDropY, swapPitchDeg, respawnRaiseTime, gemFreshMin, gemFlash, gemFlashTime;
            public double wpStart, wpFull, wpDropY, wpBackZ, wpPitchDeg, wpRate;
            public double[] afOpacity;
            public int afSpacing;
            public double afEvery, afLife, afGlow;
            public double vmShare, adsSwayMult, punchJitter, punchAdsMult, patternReset;
            public double swingOut, swingHold, swingTime, swingZ, swingPitchDeg;
        }

        /// <summary>A weapon's numbers, read from config once (NIGHT8 U2-04; NIGHT12: the recoil and punch too).</summary>
        sealed class Gun
        {
            public double ease, zoom;
            public double holdX, holdY, holdZ, holdYaw, holdPitch; // web camera space (m, deg)
            public double adsX, adsY, adsZ, adsYaw, adsPitch, adsRoll;
            public Vector3 kitPos; // Unity rig offset
            public Quaternion kitRot = Quaternion.identity;
            public float kitScale = 1;
            // the weight class (viewmodel.js classOf / springFor): the recoil spring, the look lag, the raise settle
            public SpringShape spring;
            public string cls = "medium"; // NIGHT33 view33: the weight class's recoil rate (GunFeel33.RecoilRate)
            public double swayGain = 1, swayReturn = 1, settleDeg, settleY;
            public bool hasRecoil;
            public double kick, back, drop, roll; // viewmodel.recoil.<w>
            public bool hasPunch;
            public SpringShape punchShape;
            public double punchPitch, punchCapDeg;
            public double[][] pattern; // feel.punch.weapons.<w>.pattern
            public HandsDrawSpec draw; // NIGHT16 U3-29.6: its draw (HandsDraw.SpecFor)
            public HandsKickSpec kickSpec; // NIGHT17 U3-30.6: its own kick (HandsKick.SpecFor; null: the web's class recoil)
            public double drawLastFired = double.NegativeInfinity; // frame clock of its last shot (the quick draw)
        }

        sealed class Rig
        {
            public string name;
            public Transform t;
            public Transform cylinder;
            public Transform part; // the THUNDERCLAP's pump
            public Vector3 partRest;
            public Vector3 muzzle;
            public Material gem;
            public Color gemColor;
            public int shots; // the KESTREL's, for the cylinder
            public double cylinderAngle; // rad drawn
            public double pumpAge = double.PositiveInfinity;
            public double flash; // s left of the gem's fire flash
            public double flashScale = 1; // NIGHT16: x the flash's strength (1 a shot / swing; the forge flourish's: x flashIntensity)
            public Transform drum, ring; // the STITCHER's drum; the COMET's / LONGWAVE's ring
            public Material glow; // the gun's one glow material (the STITCHER's heat strip; the LONGWAVE's charge + ring)
            public double drumAngle, drumTarget, spin; // rad (viewmodel.js rig.drumAngle / drumTarget / spin)
            public MeshFilter[] filters; // every mesh of the rig (the afterimages copy them)
            public readonly List<Mesh> meshes = new List<Mesh>(); // the kit meshes in the builder's order (body, then its parts)
            public string finishShown = Factory; // the finish its meshes are built in
            public HandsGlint glint; // NIGHT16 U3-29.6: viewmodel.js rig.glint / rig.lastCooldown
            public double glintFull; // viewmodel.js fullCooldown(name)
            public Vector3 glintFrom, glintTo; // NIGHT17 U3-30.6: QUICKDRAW's strip, from the back of the body's top to its front
            public float glintWide; // the strip's width (m, rig space; 0: none)
            public GameObject aura; // NIGHT17 round 3: the style aura's shell (an inverted hull of the body mesh)
            public double[] sheenPlace; // NIGHT18 U3-31.9 #2: each filter's place along the gun (0 back .. 1 muzzle), made on first use
            public Renderer[] renderers; // NIGHT18 U3-31.9 #6: the rig's renderers for the reel's gun rect (made on the first recorded frame)
        }

        // NIGHT12 U3-25.10: the finish each gun wears (weapon id -> finish id; the lead passes mastery.FinishFor), checked for
        // the gun in hand each frame; its meshes are rebuilt in place only when that finish changes.
        const string Factory = "factory"; // config.weapons.finishes.order[0], Mastery.FinishFor's default
        Func<string, string> finishFor;

        /// <summary>The finish source (weapon id -> finish id); null draws FACTORY.</summary>
        public void SetFinishSource(Func<string, string> finishFor) => this.finishFor = finishFor;

        /// <summary>
        /// viewmodel.js setFinish (NIGHT4 F1): rebuild a rig's kit meshes in a finish's body tones (Finishes.PaletteOf; null:
        /// FACTORY), into the same Mesh objects (the renderers, the afterimages and the moving parts keep them). Only on a change.
        /// </summary>
        void Refinish(Rig rig, string finish)
        {
            rig.finishShown = finish;
            Finishes.Palette palette = Finishes.PaletteOf(cfg, finish);
            var kits = new[] { new MeshKit(), new MeshKit(), new MeshKit() };
            using (BlockyKit.Finish(palette))
            {
                Guns52Flag(rig.name, true); // NIGHT52 U guns52: the KESTREL / THUNDERCLAP rebuilt machined while the row draws (Viewmodel.Guns52.cs)
                switch (rig.name)
                {
                    case "revolver": BlockyKit.Kestrel(kits[0], kits[1], HandsLook.Accent(rig.gemColor)); break;
                    case "shotgun": BlockyKit.Thunderclap(kits[0], HandsLook.Accent(rig.gemColor), kits[1]); break;
                    case "gauntlet": BlockyKit.Gauntlet(kits[0], kits[1], HandsLook.Accent(rig.gemColor)); break;
                    case "nailgun": BlockyKit.Stitcher(kits[0], HandsLook.Accent(rig.gemColor), kits[1], kits[2]); break;
                    case "rocket": BlockyKit.Comet(kits[0], HandsLook.Accent(rig.gemColor), kits[1]); break;
                    case "rail": BlockyKit.Longwave(kits[0], HandsLook.Accent(rig.gemColor), kits[1], kits[2]); break;
                    default:
                        if (PortRefinish(rig, kits)) break; // NIGHT19: the ZAPPER, the BOUNCER
                        return;
                }
                Guns52Flag(rig.name, false);
            }
            for (int i = 0; i < rig.meshes.Count && i < kits.Length; i++) kits[i].Build(rig.meshes[i].name, rig.meshes[i]);
        }

        public Viewmodel(SurvivalGame game)
        {
            this.game = game;
            cfg = game.cfg;
            Camera main = game.cam;
            main.cullingMask &= ~(1 << Layer);
            var go = new GameObject("Viewmodel camera");
            go.transform.SetParent(main.transform, false);
            vmCam = go.AddComponent<Camera>();
            vmCam.fieldOfView = (float)cfg.Num("viewmodel.fovDeg");
            vmCam.nearClipPlane = (float)cfg.Num("viewmodel.near");
            vmCam.farClipPlane = (float)cfg.Num("viewmodel.far");
            vmCam.cullingMask = 1 << Layer;
            var data = vmCam.GetUniversalAdditionalCameraData();
            data.renderType = CameraRenderType.Overlay;
            // NIGHT13 U3-26.4 (PerfSwitches overlayShadows): no shadow work for the overlay. It draws only the viewmodel layer,
            // whose parts cast no shadow (shadows: false), so its own shadow map was the empty one URP binds when a camera
            // has no casters: the gun received no main-light shadow before either. Off, URP skips its shadow culling and
            // binds the same empty map (renderShadows false takes the same empty-shadowmap path).
            if (PerfSwitches.On(PerfFix.OverlayShadows)) data.renderShadows = false;
            main.GetUniversalAdditionalCameraData().cameraStack.Add(vmCam);
            root = new GameObject("Viewmodel").transform;
            root.SetParent(go.transform, false);
            v = ReadParams(cfg);
            Pump40Init(cfg); // NIGHT40 W1 pump40 (Viewmodel.Pump40.cs): the faster pump while design.n40.pump40 is on
            traumaP = Trauma.Params.From(cfg);

            kit = new Material(game.mats.stone);
            kit.SetFloat("_GridMinor", 0);
            kit.SetFloat("_GridMajor", 0);
            Color family = GunYellow61.Family(cfg, "revolver", Look.Hex((uint)cfg.Num($"weapons.families.{cfg.Str("weapons.catalog.revolver.family")}"))); // NIGHT63 V (R1's N11): through gunYellow61's rule as the other guns (kinetic cyan today: as it came)
            Hammer36Prepare(); // NIGHT36 F crunch36: the KESTREL's hammer as its own part (Viewmodel.Guns36.cs)
            Guns52Prepare(); // NIGHT52 U guns52: the KESTREL and THUNDERCLAP built machined with the row on (Viewmodel.Guns52.cs, BlockyKit.Guns52.cs)
            Add(Guns52Build("revolver", () => BuildKestrel(kit, family)));
            Add(BuildGauntlet(kit, GunYellow61.Family(cfg, "gauntlet", Look.Hex((uint)cfg.Num($"weapons.families.{cfg.Str("weapons.catalog.gauntlet.family")}"))))); // NIGHT63 V (R1's N11): through gunYellow61's rule (kinetic cyan today: as it came)
            Add(Guns52Build("shotgun", () => BuildThunderclap(kit, Yellow56.Family(cfg, "shotgun", GunYellow61.Family(cfg, "shotgun", Look.Hex((uint)cfg.Num($"weapons.families.{cfg.Str("weapons.catalog.shotgun.family")}"))))))); // NIGHT56 V yellow56: the plate off the yellow arc while on; NIGHT61 V gunYellow61: the plate, gem and glow in colors.usable while on (first)
            Add(BuildStitcher(kit, FamilyOf("nailgun")));
            Add(BuildComet(kit, FamilyOf("rocket")));
            Add(BuildLongwave(kit, FamilyOf("rail")));
            PortsBuild(kit); // NIGHT19 U3-33.6: the ZAPPER and the BOUNCER from weaponModels.js (Viewmodel.Ports.cs)
            foreach (Rig r in rigList)
            {
                r.filters = r.t.GetComponentsInChildren<MeshFilter>(true);
                GunOf(r.name); // read its numbers now, not on its first shot
            }
            heatColor = Look.Hex((uint)cfg.Num("viewmodel.heatColor"));
            heatGlow = cfg.Num("viewmodel.heatGlow");
            chargeGlow = cfg.Num("viewmodel.chargeGlow");
            drumStepRad = cfg.Num("viewmodel.states.drumStepDeg") * Deg;
            drumTurnRate = cfg.Num("viewmodel.states.drumTurnRate");
            rocketKickDeg = cfg.Num("viewmodel.recoil.rocket.kickDeg");
            forgeGlow = cfg.Num("forge.glow");
            forgeGem = cfg.Num("forge.gem");

            recoil = new SpringBank(cfg.Int("viewmodel.recoilSlots"), cfg.Nums("viewmodel.recoilSnap"));
            punch = new SpringBank(cfg.Int("feel.punch.slots"), new[] { cfg.Num("feel.punch.snapDeg"), cfg.Num("feel.punch.snapDeg") });
            punchCap = cfg.Num("feel.punch.classes.medium.capDeg");
            dashPull = new KickStack(cfg.Int("viewmodel.recoilSlots")); // viewmodel.js :537 createKickStack(v.recoilSlots)

            // NIGHT17 U3-30.5 / U3-30.6: no plain muzzle quad here any more; the muzzle flash is WeaponFx's per-gun star
            // (builder X), placed at MuzzleWorld (Viewmodel.Draw.cs).

            // The afterimages: one additive material per copy (viewmodel.js :585 ghostMats), drawn by the viewmodel camera.
            ghostMats = new Material[v.afOpacity.Length];
            ghostParams = new RenderParams[v.afOpacity.Length];
            for (int i = 0; i < ghostMats.Length; i++)
            {
                ghostMats[i] = FxDraw.Additive(game.mats.neon, $"Afterimage {i}");
                ghostParams[i] = FxDraw.Params(ghostMats[i], Layer);
            }
            ghostHistory = new Matrix4x4[v.afOpacity.Length * v.afSpacing + 1];

            DrawBuild(); // NIGHT16 U3-29.6: the draws, the glint (Viewmodel.Draw.cs)
            FeelBuild(); // NIGHT17 U3-30.6: the per-gun kicks, the poses, the QUICKDRAW strip (Viewmodel.Feel.cs)
            DrawOutBuild(); // NIGHT17 round 2: the swap's overlap (the outgoing gun's root)
            ArmBuild(); // the left arm (Viewmodel.Arm.cs); it takes the `punch` event (viewmodel.js :678), which drew the
                        // gauntlet's jab here before the arm was ported (the web jabs the gauntlet on meleeSwing only)
            GustBuild(); // NIGHT19 U3-33.5: the KESTREL's gust in the hands (Viewmodel.Gust.cs)
            AltBuild(); // NIGHT19 U3-33.8: every gun's alt-fire (Viewmodel.Alt19.cs)
            View33Build(); // NIGHT33 W: the view33 zoom pose, the piercer's kick (Viewmodel.View33.cs)
            Charge35Build(); // NIGHT35 W: charge35's tip, barrel glow and wind-up (Viewmodel.Charge35.cs)
            game.events.On("fire", o => OnFire((string)o));
            game.events.On<MeleeEvent>("meleeSwing", OnSwing);
            game.events.On("shotgunPump", _ => StartPump()); // weapons.js :211 (the port's loadout times it here instead: pumpIn)
            game.mover.Events += OnMover;
            game.OnNewRun += Respawn;
        }

        double swingAge = 99;

        static P ReadParams(GameConfig cfg)
        {
            string vm = "viewmodel.";
            var p = new P
            {
                breathAmp = cfg.Num(vm + "breathAmp"), breathPeriod = cfg.Num(vm + "breathPeriod"),
                bobAmpX = cfg.Num(vm + "bobAmpX"), bobAmpY = cfg.Num(vm + "bobAmpY"), bobCycleDistance = cfg.Num(vm + "bobCycleDistance"),
                bobMaxScale = cfg.Num(vm + "bobMaxScale"), bobEaseRate = cfg.Num(vm + "bobEaseRate"),
                swayGain = cfg.Num(vm + "swayGain"), swayVelDamp = cfg.Num(vm + "swayVelDamp"), swayReturnRate = cfg.Num(vm + "swayReturnRate"),
                swayMaxX = cfg.Num(vm + "swayMaxX"), swayMaxY = cfg.Num(vm + "swayMaxY"), swayRollDegPerM = cfg.Num(vm + "swayRollDegPerM"),
                strafeRollDeg = cfg.Num(vm + "strafeRollDeg"), poseEaseRate = cfg.Num(vm + "poseEaseRate"),
                slideDipY = cfg.Num(vm + "slideDipY"), slideRollDeg = cfg.Num(vm + "slideRollDeg"), wallTiltDeg = cfg.Num(vm + "wallTiltDeg"),
                airLiftY = cfg.Num(vm + "airLiftY"), dashPullZ = cfg.Num(vm + "dashPullZ"), dashPullAttack = cfg.Num(vm + "dashPullAttack"),
                dashPullRecover = cfg.Num(vm + "dashPullRecover"), landDipScale = cfg.Num(vm + "landDipScale"),
                cylinderStepDeg = cfg.Num(vm + "cylinderStepDeg"), cylinderTurnTime = cfg.Num(vm + "cylinderTurnTime"),
                pumpTravel = cfg.Num(vm + "pumpTravel"), pumpDuration = cfg.Num(vm + "pumpDuration"), pumpTiltDeg = cfg.Num(vm + "pumpTiltDeg"),
                pumpDelay = cfg.Num("weapons.shotgun.pumpDelay"),
                swapDropY = cfg.Num(vm + "swapDropY"), swapPitchDeg = cfg.Num(vm + "swapPitchDeg"), respawnRaiseTime = cfg.Num(vm + "respawnRaiseTime"),
                gemFreshMin = cfg.Num(vm + "gemFreshMin"), gemFlash = cfg.Num(vm + "gemFlash"), gemFlashTime = cfg.Num(vm + "gemFlashTime"),
                wpStart = cfg.Num(vm + "wallPull.start"), wpFull = cfg.Num(vm + "wallPull.full"), wpDropY = cfg.Num(vm + "wallPull.dropY"),
                wpBackZ = cfg.Num(vm + "wallPull.backZ"), wpPitchDeg = cfg.Num(vm + "wallPull.pitchDeg"), wpRate = cfg.Num(vm + "wallPull.rate"),
                afOpacity = cfg.Nums(vm + "afterimages.opacity"), afSpacing = cfg.Int(vm + "afterimages.spacing"),
                afEvery = cfg.Num(vm + "afterimages.every"), afLife = cfg.Num(vm + "afterimages.life"), afGlow = cfg.Num(vm + "afterimages.glow"),
                vmShare = cfg.Num("feel.trauma.vmShare"), adsSwayMult = cfg.Num("ads.vmSwayMult"),
                punchJitter = cfg.Has("feel.punch.jitter") ? cfg.Num("feel.punch.jitter") : 0, punchAdsMult = cfg.Num("feel.punch.adsMult"),
                patternReset = cfg.Num("feel.punch.patternReset"),
                swingOut = cfg.Num(vm + "swing.gauntlet.out"), swingHold = cfg.Num(vm + "swing.gauntlet.hold"), swingTime = cfg.Num(vm + "swing.gauntlet.time"),
                swingZ = cfg.Num(vm + "swing.gauntlet.z"), swingPitchDeg = cfg.Num(vm + "swing.gauntlet.pitchDeg"),
            };
            return p;
        }

        void Add(Rig r)
        {
            rigs[r.name] = r;
            rigList.Add(r);
        }

        Rig BuildKestrel(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var cyl = new MeshKit();
            Vector3 muzzle = BlockyKit.Kestrel(body, cyl, HandsLook.Accent(family));
            var rig = new Rig { name = "revolver", t = new GameObject("KESTREL").transform, muzzle = muzzle };
            rig.t.SetParent(root, false);
            SetLayer(MeshKit.Object("body", rig.t, Keep(rig, body.Build("kestrel")), kitMat, shadows: false));
            var c = MeshKit.Object("cylinder", rig.t, Keep(rig, cyl.Build("kestrel cylinder")), kitMat, shadows: false);
            c.transform.localPosition = new Vector3(0, 0.032f, 0.062f);
            SetLayer(c);
            rig.cylinder = c.transform;
            Hammer36Build(rig, kitMat); // NIGHT36 F crunch36
            rig.gem = Gem(rig.t, new Vector3(0, 0.075f, 0.06f), 0.011f, family);
            rig.gemColor = family;
            return rig;
        }

        Rig BuildThunderclap(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var pump = new MeshKit();
            Vector3 muzzle = BlockyKit.Thunderclap(body, HandsLook.Accent(family), pump);
            var rig = new Rig { name = "shotgun", t = new GameObject("THUNDERCLAP").transform, muzzle = muzzle };
            rig.t.SetParent(root, false);
            SetLayer(MeshKit.Object("body", rig.t, Keep(rig, body.Build("thunderclap")), kitMat, shadows: false));
            rig.part = KitPart(rig, "pump", pump, BlockyKit.ThunderclapPumpAt, kitMat); // weaponModels.js parts.pump: it slides
            rig.partRest = rig.part.localPosition;
            rig.gem = Gem(rig.t, new Vector3(0, 0.077f, 0.06f), 0.02f, family);
            rig.gemColor = family;
            return rig;
        }

        Rig BuildGauntlet(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var glow = new MeshKit();
            Vector3 muzzle = BlockyKit.Gauntlet(body, glow, HandsLook.Accent(family));
            var rig = new Rig { name = "gauntlet", t = new GameObject("PULSE GAUNTLET").transform, muzzle = muzzle };
            rig.t.SetParent(root, false);
            SetLayer(MeshKit.Object("body", rig.t, Keep(rig, body.Build("gauntlet")), kitMat, shadows: false));
            var glowMat = new Material(game.mats.neon);
            glowMat.SetColor("_BaseColor", family * 2);
            SetLayer(MeshKit.Object("pulse", rig.t, Keep(rig, glow.Build("gauntlet pulse")), glowMat, shadows: false));
            // NIGHT36 F fist36 (n36-r3): no cyan ball on the hand: the gem sits small inside the knuckles' pulse strip (its flash lights the strip)
            rig.gem = fist36 ? Gem(rig.t, UnityTuning.Fist36GemAt, UnityTuning.Fist36GemSize, family) : Gem(rig.t, new Vector3(0, 0.06f, 0.05f), 0.016f, family);
            rig.gemColor = family;
            return rig;
        }

        /// <summary>viewmodel.js createRig: a weapon's family colour (config.weapons.families[catalog[name].family]).</summary>
        Color FamilyOf(string weapon) => Yellow56.Family(cfg, weapon, GunYellow61.Family(cfg, weapon, WeaponLook.FamilyColor(cfg, weapon, Color.white))); // NIGHT13 U3-26.5: the one family colour lookup (every kit gun has a family); NIGHT56 V yellow56: the plate off the yellow arc while on; NIGHT61 V gunYellow61: in colors.usable while on (first)

        /// <summary>A kit gun's rig: its transform under the root, the body mesh, the core gem (BlockyKit.GemOf).</summary>
        Rig KitRig(string weapon, string title, MeshKit body, Vector3 muzzle, Material kitMat, Color family)
        {
            var rig = new Rig { name = weapon, t = new GameObject(title).transform, muzzle = muzzle };
            rig.t.SetParent(root, false);
            SetLayer(MeshKit.Object("body", rig.t, Keep(rig, body.Build(title.ToLowerInvariant())), kitMat, shadows: false));
            BlockyKit.GemOf(weapon, out Vector3 at, out float r);
            rig.gem = Gem(rig.t, at, r, family);
            rig.gemColor = family;
            return rig;
        }

        /// <summary>A moving part's mesh (built around 0) at `at` (Unity rig space).</summary>
        Transform KitPart(Rig rig, string name, MeshKit part, Vector3 at, Material mat)
        {
            var go = MeshKit.Object(name, rig.t, Keep(rig, part.Build($"{rig.t.name.ToLowerInvariant()} {name}")), mat, shadows: false);
            go.transform.localPosition = at;
            SetLayer(go);
            return go.transform;
        }

        /// <summary>A kit mesh of the rig, kept in build order (a finish rebuilds it in place).</summary>
        static Mesh Keep(Rig rig, Mesh mesh)
        {
            rig.meshes.Add(mesh);
            return mesh;
        }

        /// <summary>A glow material on the neon (unlit, HDR) base: viewmodel.js vcGlow(intensity), its colour set per frame.</summary>
        Material GlowMaterial(Color start)
        {
            var m = new Material(game.mats.neon);
            m.SetColor(BaseColor, start);
            return m;
        }

        /// <summary>weaponModels.js nailgun() (STITCHER): the body, the drum (a notch per nail), the heat strip (vcGlow(1.4)).</summary>
        Rig BuildStitcher(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var drum = new MeshKit();
            var heat = new MeshKit();
            Vector3 muzzle = BlockyKit.Stitcher(body, HandsLook.Accent(family), drum, heat);
            Rig rig = KitRig("nailgun", "STITCHER", body, muzzle, kitMat, family);
            rig.drum = KitPart(rig, "drum", drum, BlockyKit.StitcherDrumAt, kitMat);
            rig.glow = GlowMaterial(family * 1.4f);
            KitPart(rig, "heat", heat, Vector3.zero, rig.glow);
            return rig;
        }

        /// <summary>weaponModels.js rocket() (COMET): the body and the turning ring (on the body material).</summary>
        Rig BuildComet(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var ring = new MeshKit();
            Vector3 muzzle = BlockyKit.Comet(body, HandsLook.Accent(family), ring);
            Rig rig = KitRig("rocket", "COMET", body, muzzle, kitMat, family);
            rig.ring = KitPart(rig, "ring", ring, BlockyKit.CometRingAt, kitMat);
            return rig;
        }

        /// <summary>weaponModels.js rail() (LONGWAVE): the body; the charge bar + seams and the ring share one glow (vcGlow(1.2)).</summary>
        Rig BuildLongwave(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var glow = new MeshKit();
            var ring = new MeshKit();
            Vector3 muzzle = BlockyKit.Longwave(body, HandsLook.Accent(family), glow, ring);
            Rig rig = KitRig("rail", "LONGWAVE", body, muzzle, kitMat, family);
            rig.glow = GlowMaterial(family * 1.2f);
            KitPart(rig, "charge", glow, Vector3.zero, rig.glow);
            rig.ring = KitPart(rig, "ring", ring, BlockyKit.LongwaveRingAt, rig.glow);
            return rig;
        }

        Material Gem(Transform parent, Vector3 at, float r, Color family)
        {
            GameObject g;
            if (Night51.On)
            {
                // NIGHT51 gem51: the web's faceted gem (blockyKit.js socketGem: an octahedron of r x 0.82, scaled (1, 0.8, 1)), not a smooth dome
                var kit = new MeshKit();
                float R = r * 0.82f * UnityTuning.N51GemScale;
                Vector3 up = new Vector3(0, R * 0.8f, 0), dn = -up;
                Vector3[] ring = { new Vector3(R, 0, 0), new Vector3(0, 0, R), new Vector3(-R, 0, 0), new Vector3(0, 0, -R) };
                for (int k = 0; k < 4; k++)
                {
                    Vector3 a = ring[k], b = ring[(k + 1) % 4];
                    kit.Triangle(a, b, up, Outward51(a, b, up), Color.white);
                    kit.Triangle(b, a, dn, Outward51(b, a, dn), Color.white);
                }
                g = MeshKit.Object("gem51", parent, kit.Build("gem51"), game.mats.neon, shadows: false);
                g.layer = Layer;
                g.transform.localPosition = at;
            }
            else
            {
                g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                UnityEngine.Object.Destroy(g.GetComponent<Collider>());
                g.layer = Layer;
                g.transform.SetParent(parent, false);
                g.transform.localPosition = at;
                g.transform.localScale = Vector3.one * r * 2;
            }
            var m = new Material(game.mats.neon);
            m.SetColor("_BaseColor", family * (float)(2.4 * UnityTuning.HandsGemGain)); // round 3: brighter (HandsGemGain)
            var rend = g.GetComponent<Renderer>();
            rend.sharedMaterial = m;
            rend.shadowCastingMode = ShadowCastingMode.Off;
            return m;
        }

        /// <summary>NIGHT51 gem51: a facet's normal, turned away from the gem's centre (the origin).</summary>
        static Vector3 Outward51(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 n = Vector3.Cross(b - a, c - a).normalized;
            return Vector3.Dot(n, a + b + c) < 0 ? -n : n;
        }

        static void SetLayer(GameObject go) => go.layer = Layer;

        /// <summary>A weapon's numbers (Gun), read from config on first use and kept.</summary>
        Gun GunOf(string held)
        {
            if (guns.TryGetValue(held, out Gun p)) return p;
            p = new Gun();
            // viewmodel.js classOf: feel.punch.weapons[name].cls ?? viewmodel.meleeClass[name] ?? 'medium'
            string cls = cfg.Has($"feel.punch.weapons.{held}.cls") ? cfg.Str($"feel.punch.weapons.{held}.cls")
                : cfg.Has($"viewmodel.meleeClass.{held}") ? cfg.Str($"viewmodel.meleeClass.{held}") : "medium";
            string ec = cls != "" && cfg.Has($"ads.classEase.{cls}") ? $"ads.classEase.{cls}" : null;
            p.ease = ec != null ? cfg.Num(ec) : cfg.Num("ads.easeTime");
            p.zoom = cfg.Has($"ads.weapons.{held}.zoom") ? cfg.Num($"ads.weapons.{held}.zoom") : 1;
            string ap = cfg.Has($"viewmodel.ads.{held}") ? $"viewmodel.ads.{held}" : "viewmodel.ads.default";
            p.holdX = cfg.Num("viewmodel.holdX");
            p.holdY = cfg.Num("viewmodel.holdY");
            p.holdZ = cfg.Num("viewmodel.holdZ");
            p.holdYaw = cfg.Num("viewmodel.holdYawDeg");
            p.holdPitch = cfg.Num("viewmodel.holdPitchDeg");
            p.adsX = cfg.Num(ap + ".x");
            p.adsY = cfg.Num(ap + ".y");
            p.adsZ = cfg.Num(ap + ".z");
            p.adsYaw = cfg.Num(ap + ".yawDeg");
            p.adsPitch = cfg.Num(ap + ".pitchDeg");
            p.adsRoll = cfg.Num(ap + ".rollDeg");
            if (held == "nailgun" && !cfg.Has("viewmodel.ads.nailgun"))
            {
                // NIGHT20 U3-34.3 (builder V): the STITCHER zooms (design.zoom; the web never zoomed it): its own ADS pose
                // (UnityTuning.Fx20.cs). Shown only while the sim zooms it (adsActive): with design.zoom off it never does.
                p.adsX = UnityTuning.NailgunAdsX;
                p.adsY = UnityTuning.NailgunAdsY;
                p.adsZ = UnityTuning.NailgunAdsZ;
                p.adsYaw = UnityTuning.NailgunAdsYawDeg;
                p.adsPitch = UnityTuning.NailgunAdsPitchDeg;
                p.adsRoll = UnityTuning.NailgunAdsRollDeg;
            }
            string ko = $"viewmodel.kitOffsets.{held}";
            if (cfg.Has(ko))
            {
                p.kitPos = new Vector3((float)cfg.Num(ko + ".x"), (float)cfg.Num(ko + ".y"), (float)-cfg.Num(ko + ".z"));
                float kp = cfg.Has(ko + ".pitchDeg") ? (float)cfg.Num(ko + ".pitchDeg") : 0;
                float ky = cfg.Has(ko + ".yawDeg") ? (float)cfg.Num(ko + ".yawDeg") : 0;
                HandsDraw.HoldTurn(held, out double turnP, out double turnY); // round 6: the THUNDERCLAP's barrel turned out of the centre
                kp += (float)turnP;
                ky += (float)turnY;
                float kr = cfg.Has(ko + ".rollDeg") ? (float)cfg.Num(ko + ".rollDeg") : 0;
                // three's rig.group.rotation (order XYZ: M = Rx Ry Rz) under the z flip: Rx(-p) Ry(-y) Rz(r)
                p.kitRot = Quaternion.AngleAxis(-kp, Vector3.right) * Quaternion.AngleAxis(-ky, Vector3.up) * Quaternion.AngleAxis(kr, Vector3.forward);
                if (cfg.Has(ko + ".scale")) p.kitScale = (float)cfg.Num(ko + ".scale");
            }
            HandsDraw.HoldNudge(held, out double nudgeX, out double nudgeY); // NIGHT17 D-69: the GAUNTLET raised off the weapon card
            p.kitPos += new Vector3((float)nudgeX, (float)nudgeY, 0);
            p.kitScale *= (float)HandsDraw.Presence(held); // round 3: a little more presence on screen
            string sp = cfg.Has($"viewmodel.recoilSprings.{cls}") ? $"viewmodel.recoilSprings.{cls}" : "viewmodel.recoilSprings.medium";
            p.cls = cfg.Has($"viewmodel.recoilSprings.{cls}") ? cls : "medium";
            p.spring = SpringShape.Of(cfg.Num(sp + ".freqHz"), cfg.Num(sp + ".zeta"));
            p.swayGain = cfg.Num(sp + ".swayGain");
            p.swayReturn = cfg.Num(sp + ".swayReturn");
            p.settleDeg = cfg.Num(sp + ".settleDeg");
            p.settleY = cfg.Num(sp + ".settleY");
            string rc = $"viewmodel.recoil.{held}";
            if (p.hasRecoil = cfg.Has(rc))
            {
                p.kick = cfg.Num(rc + ".kickDeg");
                p.back = cfg.Num(rc + ".backZ");
                p.drop = cfg.Num(rc + ".dropY");
                p.roll = cfg.Num(rc + ".rollDeg");
            }
            string pw = $"feel.punch.weapons.{held}";
            if (p.hasPunch = cfg.Has(pw))
            {
                string pc = cfg.Str(pw + ".cls");
                string pcls = cfg.Has($"feel.punch.classes.{pc}") ? $"feel.punch.classes.{pc}" : "feel.punch.classes.medium";
                p.punchShape = SpringShape.Of(cfg.Num(pcls + ".freqHz"), cfg.Num(pcls + ".zeta"));
                p.punchCapDeg = cfg.Num(pcls + ".capDeg");
                p.punchPitch = cfg.Num(pw + ".pitchDeg");
                if (cfg.Has(pw + ".pattern"))
                {
                    var rows = cfg.Arr(pw + ".pattern");
                    p.pattern = new double[rows.Count][];
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var row = PortData.Arr(rows[i]);
                        p.pattern[i] = new[] { PortData.Num(row[0]), row.Count > 1 ? PortData.Num(row[1]) : 0 };
                    }
                }
            }
            p.draw = HandsDraw.SpecFor(cfg, held); // NIGHT16 U3-29.6
            p.kickSpec = HandsKick.SpecFor(held); // NIGHT17 U3-30.6
            guns[held] = p;
            return p;
        }

        readonly double[] peaks4 = new double[4];
        readonly double[] peaks2 = new double[2];

        void OnFire(string weapon)
        {
            DrawCancel(); // NIGHT16 U3-29.6: a shot cancels the draw's flourish (and the idle) on this frame; the recoil takes over
            Gun g = GunOf(weapon);
            g.drawLastFired = clock;
            // NIGHT17 U3-30.6: the gun's own kick (two spring layers, Viewmodel.Feel.cs) in place of the web's class recoil;
            // every shot yields the sprint / boost poses
            if (!FeelShot(weapon, g) && g.hasRecoil)
            {
                // viewmodel.js :567-581: the gun's kick on its class's spring (pitch deg, back m, drop m, roll deg)
                double steady = HandsSteady.Mult(game.sim != null && game.sim.perks != null ? game.sim.perks.fx : null); // STEADY: recoilMult (viewmodel.js :151, :571-574; polish #42)
                peaks4[0] = g.kick * steady;
                peaks4[1] = g.back * steady;
                peaks4[2] = g.drop * steady;
                peaks4[3] = g.roll * steady;
                recoil.Add(g.spring, peaks4);
                recoilClock.Kick(g.spring.tPeak); // the attack to the peak on frame time while the world is frozen
            }
            if (g.hasPunch)
            {
                // player.js :151-165: the camera punch, a pattern row per shot (reset after patternReset), jittered by the
                // shot count's lattice hash (no random stream)
                if (clock - lastShotAt > v.patternReset) patternShot = 0;
                lastShotAt = clock;
                double row0 = 1, row1 = 0;
                if (g.pattern != null && g.pattern.Length > 0)
                {
                    double[] row = g.pattern[patternShot++ % g.pattern.Length];
                    row0 = row[0];
                    row1 = row[1];
                }
                shotCount++;
                double steadyPunch = HandsSteady.Mult(game.sim != null && game.sim.perks != null ? game.sim.perks.fx : null); // player.js :160-161 x recoilMult (polish #42)
                peaks2[0] = g.punchPitch * row0 * (1 + v.punchJitter * Cosmetic.Lattice(shotCount, 5)) * steadyPunch;
                peaks2[1] = row1 * (1 + v.punchJitter * Cosmetic.Lattice(shotCount, 9)) * steadyPunch;
                if (Fx47View.RowOn(game.cfg)) { double pm = Fx47View.PunchMul(weapon); peaks2[0] *= pm; peaks2[1] *= pm; } // NIGHT47 F fx47 (G24): the weight ladder's camera punch per gun
                punch.Add(g.punchShape, peaks2);
                punchCap = g.punchCapDeg;
            }
            if (rigs.TryGetValue(weapon, out Rig r))
            {
                r.flash = v.gemFlashTime; // the gem's white-hot flash
                r.flashScale = 1;
                if (r.cylinder != null) r.shots++; // the cylinder steps a chamber
                Hammer36Fire(weapon); // NIGHT36 F crunch36: the hammer falls
                if (r.drum != null) r.drumTarget += weapon == "bouncer" ? Math.PI / 3 : drumStepRad; // viewmodel.js :579 (NIGHT4 G2): the drum turns a notch per nail; :578 the BOUNCER's a sixth
            }
            if (weapon == "shotgun") pumpIn = Pump40Delay(v.pumpDelay); // weapons.js :601 pumpSteps = stepsFor(sg.pumpDelay); NIGHT40 W1 pump40 (Viewmodel.Pump40.cs)
        }

        void StartPump()
        {
            pumpIn = 0;
            if (rigs.TryGetValue("shotgun", out Rig r)) r.pumpAge = 0;
        }

        /// <summary>viewmodel.js :636-652 meleeSwing: the jab, the gem's flash, and the afterimages start along the swing.</summary>
        void OnSwing(MeleeEvent e)
        {
            swingAge = 0;
            DrawCancel(); // NIGHT16 U3-29.6: the gauntlet's swing is its shot
            feelMove.Shot(); // NIGHT17 U3-30.6: and the poses yield to it
            if (!rigs.TryGetValue(e.weapon, out Rig rig)) return;
            rig.flash = v.gemFlashTime;
            rig.flashScale = 1;
            // buildGhosts: each mesh's place in the rig now (the moving parts as they stand)
            if (ghostLocals.Length < rig.filters.Length) ghostLocals = new Matrix4x4[rig.filters.Length];
            Matrix4x4 inv = rig.t.worldToLocalMatrix;
            for (int i = 0; i < rig.filters.Length; i++) ghostLocals[i] = inv * rig.filters[i].transform.localToWorldMatrix;
            ghostRig = rig;
            Color fam = rig.gemColor * (float)v.afGlow;
            for (int i = 0; i < ghostMats.Length; i++) ghostMats[i].SetColor(BaseColor, fam * (float)v.afOpacity[i]);
            ghostCount = 0; // ghosts.pool.push(...history.splice(0))
            ghostAge = 0;
            ghostSample = v.afEvery;
            if (fist36 && e.weapon == "gauntlet") ghostRig = null; // NIGHT36 F fist36 (n36-r3: the lunge frame was a cyan smear): no afterimages on the fist
        }

        void OnMover(MoverEvent e)
        {
            DrawOnMover(e); // NIGHT16 U3-29.6: the take-off lift, the weighted landing, the slam's impact (Viewmodel.Draw.cs)
            if (e.type == MoverEventType.WallJump) armPushAge = 0; // the free hand pushes off the wall (Viewmodel.Arm.cs)
            if (e.type == MoverEventType.Jump && e.kind == JumpKind.Slide) feelMove.SlideJump(); // NIGHT17 round 1: the slide pose snaps back up
            switch (e.type)
            {
                case MoverEventType.Dash: // viewmodel.js :677 the dash pull
                    dashPull.Add(1, v.dashPullAttack, v.dashPullRecover);
                    FeelOnDash(); // NIGHT17 U3-30.6: the boost's swing (lags the move, catches up past it)
                    break;
                case MoverEventType.Land: // player.js :128-132 (the camera's dip, which the gun shares)
                    FeelConfig f = cfg.movement.feel;
                    dipAmount = Math.Min(f.landingDipMax, f.landingDipPerSpeed * e.fallSpeed);
                    dipTime = 0;
                    break;
            }
        }

        /// <summary>viewmodel.js respawn(): a cut: the poses reset, the sway resyncs, the gun rises in (a new run).</summary>
        public void Respawn()
        {
            lastYaw = lastPitch = double.NaN;
            swayX = swayY = swayVx = swayVy = 0;
            bobAmp = 0;
            strafeRoll = slideW = wallSide = airW = 0;
            dashPull.Clear();
            recoil.Clear();
            recoilClock.Clear();
            armImpact.Clear(); // :956 impact.clear()
            foreach (Rig r in rigList) r.pumpAge = double.PositiveInfinity;
            pumpIn = 0;
            drawState.Respawn(); // NIGHT16 U3-29.6: the raise is the held gun's draw (weaponDraw on the next frame)
            drawQuick = false;
            drawSlamW = 0;
            foreach (Gun g in guns.Values) g.drawLastFired = double.NegativeInfinity;
            ArmRespawn();
            FeelRespawn(); // NIGHT17 U3-30.6
            wallW = 0;
            dipTime = double.PositiveInfinity;
            ghostAge = double.PositiveInfinity;
            gustHands.Clear(); // NIGHT19
            altHands.Clear();
            altOver = false;
            altSteered = -1;
            altSteerHold = 0;
        }

        /// <summary>
        /// Frame time: the pose and every layer of its life, the springs, the flash; returns the drawn FOV for the
        /// main camera. The world's layers advance on SurvivalGame.WorldDt (viewmodel.js animDt: frozen in a hitstop).
        /// </summary>
        public float Render(double frameDt, float baseFov)
        {
            double animDt = game.WorldDt;
            clock += frameDt;
            punch.Update(frameDt);
            Loadout lo = game.loadout;
            Mover mover = game.mover;
            MovementConfig mc = cfg.movement;
            bool show = game.Active;
            if (root.gameObject.activeSelf != show) root.gameObject.SetActive(show);
            double intensity = Clamp(mc.settings.effectsIntensity, 0, mc.feel.effectsIntensityMax);
            // 6 (first: it decides the gun drawn). The swap and the respawn raise: the gun's draw (Viewmodel.Draw.cs). NIGHT17
            // round 1: the view wants the loadout's TARGET from the press; the gun on screen flicks out (off-screen at the flick's
            // depth), the wanted one rises on the sim's step clock to hold on the fire step; every layer below draws the gun
            // SHOWN (drawState.held). x effects intensity, damped by REDUCED MOTION; world time (frozen in a hitstop).
            double drawMotion = intensity * (mc.settings.reducedMotion ? UnityTuning.HandsReducedMotionScale : 1);
            string want = WantGun(lo);
            DrawFrame(lo, want, GunOf(want), animDt, frameDt, drawMotion);
            string held = drawState.held ?? lo.Weapon;
            Gun gun = GunOf(held);
            rigs.TryGetValue(held, out Rig rig);
            if (rig != null && finishFor != null)
            {
                string finish = finishFor(held) ?? Factory;
                if (finish != rig.finishShown) Refinish(rig, finish); // NIGHT12 U3-25.10: only when it changes
            }
            double walk = mc.ground.walkSpeed;
            MoveState state = mover.state;
            double speed = mover.Speed;
            double px = 0, py = 0, pz = 0, rx = 0, ry = 0, rz = 0;

            armImpact.Advance(frameDt); // :740 NIGHT5 F4: frame time, so the momentum hit's drive plays through the hitstop
            // 1. Idle breathing
            time += animDt;
            py += v.breathAmp * Math.Sin(2 * Math.PI * time / v.breathPeriod);

            // 2. Movement bob: grounded speed only, zero while sliding or airborne (x EFFECTS INTENSITY)
            bool bobbing = mover.grounded && state != MoveState.Slide;
            double bobTarget = bobbing ? GunFeel33.BobScale(view33, speed, walk, v.bobMaxScale) * intensity : 0; // NIGHT33 view33: scales on to 16 m/s
            bobAmp += (bobTarget - bobAmp) * EaseFactor(v.bobEaseRate, animDt);
            if (bobbing) bobPhase += speed * animDt / v.bobCycleDistance * 2 * Math.PI;
            double sb = Math.Sin(bobPhase);
            armBobX = sb * v.bobAmpX * bobAmp; // viewmodel.js :751-752 bobX / bobY (the arm shares them)
            armBobY = -(sb * sb) * v.bobAmpY * bobAmp;
            px += armBobX;
            py += armBobY;
            if (view33)
            {
                // NIGHT33 view33: the idle figure-eight (a still gun only: faded by the bob)
                var idle = GunFeel33.IdleSway(time, 1 - bobAmp);
                px += idle.x * intensity;
                py += idle.y * intensity;
                rz += idle.rollDeg * Deg * intensity;
                IdleSwayNow33 = idle.x * intensity;
            }

            // 3. Look sway (frame time): the look delta feeds a velocity that decays; the offset eases toward it, clamped.
            double yaw = game.input.yaw, pitch = game.input.pitch;
            if (double.IsNaN(lastYaw))
            {
                lastYaw = yaw;
                lastPitch = pitch;
            }
            swayVx += (yaw - lastYaw) * v.swayGain * gun.swayGain;
            swayVy -= (pitch - lastPitch) * v.swayGain * gun.swayGain;
            lastYaw = yaw;
            lastPitch = pitch;
            double velDecay = 1 - EaseFactor(v.swayVelDamp, frameDt);
            swayVx *= velDecay;
            swayVy *= velDecay;
            double back = EaseFactor(v.swayReturnRate * gun.swayReturn, frameDt);
            swayX += (Clamp(swayVx, -v.swayMaxX, v.swayMaxX) - swayX) * back;
            swayY += (Clamp(swayVy, -v.swayMaxY, v.swayMaxY) - swayY) * back;
            px += swayX;
            py += swayY;
            rz += -swayX * v.swayRollDegPerM * Deg;
            HudAttach18.Set(swayX + armBobX, swayY + armBobY, -swayX * v.swayRollDegPerM); // NIGHT18 backlog 5: viewmodel.js :930-932 hudOffset

            // 4. Strafe roll + 5. state poses, all eased (world time)
            double pose = EaseFactor(v.poseEaseRate, animDt);
            double rightX = Math.Cos(yaw), rightZ = -Math.Sin(yaw);
            double lateral = (mover.velocity.x * rightX + mover.velocity.z * rightZ) / walk;
            strafeRoll += (-Clamp(lateral, -1, 1) * v.strafeRollDeg * Deg - strafeRoll) * pose;
            rz += strafeRoll;
            slideW += ((state == MoveState.Slide ? 1 : 0) - slideW) * pose;
            py += v.slideDipY * slideW;
            rz += v.slideRollDeg * Deg * slideW; // inward
            double side = state == MoveState.WallRide ? mover.wallNormal.x * rightX + mover.wallNormal.z * rightZ : 0;
            armWallSide = side; // NIGHT16 U3-29.6: the free hand reads it (Viewmodel.Arm.cs)
            wallSide += (side - wallSide) * pose;
            rz -= v.wallTiltDeg * Deg * wallSide; // wall on the left: tilt right, away from it
            airW += ((!mover.grounded && state != MoveState.WallRide ? 1 : 0) - airW) * pose;
            py += v.airLiftY * airW;
            dashPull.Advance(animDt);
            pz += v.dashPullZ * dashPull.Value();
            var dip33 = GunFeel33.LandDip(view33, LandingDip(frameDt) * intensity * v.landDipScale); // player.landingDip x landDipScale
            py += dip33.y; // (NIGHT33 view33: also back toward the eye and tipped down)
            pz += dip33.z;
            rx += dip33.pitchDeg * Deg;
            LandDipNow33 = -dip33.y;
            DrawSlamWind(state == MoveState.Slam, animDt, ref py, ref pz, ref rx); // NIGHT16 U3-29.6: the slam's wind-up
            FeelPose(animDt, ref px, ref py, ref pz, ref rx, ref ry, ref rz); // NIGHT17 U3-30.6: the sprint carry, the boost lean and swing, the breath

            // 6. The draw's pose (computed first, above): the web's drop-and-raise (swapDropY / swapPitchDeg) as the flick and
            // the rise; NIGHT5 F2's settle (the gun landing in the hands on its class's spring) is the draw's settle, closed-form,
            // so a shot can cancel it; each gun's signature rides on it.
            px += drawPose.x;
            py += drawPose.y;
            pz += drawPose.z;
            rx += drawPose.pitchDeg * Deg;
            ry += drawPose.yawDeg * Deg;
            rz += drawPose.rollDeg * Deg;
            // NIGHT4 G2: a wall right ahead: the gun lowers, tips down and comes back (the eye ray; visual only).
            double near = show ? Clamp((v.wpStart - WallAhead()) / (v.wpStart - v.wpFull), 0, 1) : 0;
            wallW += (Smooth(near) - wallW) * EaseFactor(v.wpRate, animDt);
            py -= v.wpDropY * wallW;
            pz += v.wpBackZ * wallW;
            rx -= v.wpPitchDeg * Deg * wallW;

            // 7. The gun in hand: its kit offset, the jab, the moving parts, the gem, the recoil, the cylinder, the pump.
            string outGun = DrawOutGun(held); // round 2: the gun still flicking out (the swap's overlap) stays shown too
            for (int i = 0; i < rigList.Count; i++)
            {
                Rig r = rigList[i];
                bool visible = r == rig || (outGun != null && r.name == outGun);
                if (r.t.gameObject.activeSelf != visible) r.t.gameObject.SetActive(visible);
                r.flash = Math.Max(0, r.flash - animDt);
            }
            TripoFrame(); // PG3 tripo74: the KESTREL's / THUNDERCLAP's Tripo model over the kit (Viewmodel.Tripo.cs; nothing when its file is missing or the row is off)
            // NIGHT16 (the reviewer): a headshot / rail / rocket / heavy hit freezes on its own step; the newest kick's ATTACK
            // (to its spring's first peak) runs on max(worldDt, frameDt) so the kick lands in the freeze and holds there; the
            // return on world time. The world running: animDt exactly (the web's).
            recoil.Update(recoilClock.Step(animDt, frameDt) * RecoilRate33(held)); // NIGHT33 view33: the class's faster settle (x1 off)
            FeelKickFrame(animDt, frameDt, ref px, ref py, ref pz, ref rx, ref ry, ref rz); // NIGHT17 U3-30.6: the gun's own kick (its attack in the freeze too)
            GustFrame(animDt, frameDt, ref px, ref py, ref pz, ref rx, ref ry, ref rz); // NIGHT19 U3-33.5: the gust's blowback
            AltFrame(lo, held, animDt, ref px, ref py, ref pz, ref rx, ref ry, ref rz); // NIGHT19 U3-33.8: the alt-fires
            Charge35Frame(lo, held, frameDt, ref px, ref py, ref pz, ref rx, ref ry, ref rz); // NIGHT35 W: charge35's wind-up and shake (Viewmodel.Charge35.cs)
            Fist36Frame(frameDt); // NIGHT36 F fist36: the parry pose's weight this frame
            if (pumpIn > 0)
            {
                pumpIn -= animDt;
                if (pumpIn <= 0 && held == "shotgun") StartPump(); // weapons.js :210: the pump only with the shotgun still in hand
            }
            if (rig != null)
            {
                rig.t.localPosition = gun.kitPos;
                rig.t.localRotation = gun.kitRot;
                // NIGHT16 U3-29.6: the PULSE GAUNTLET's draw clenches (a squeeze across x / y)
                float squeeze = (float)(1 - UnityTuning.HandsGauntletClenchSquash * drawPose.clench);
                rig.t.localScale = new Vector3(gun.kitScale * squeeze, gun.kitScale * squeeze, gun.kitScale);
                Fist36Kit(rig, held); // NIGHT36 F fist36 (n36-r3): the fist bigger, higher, nose down so the knuckles show
                if (held == "gauntlet")
                {
                    // viewmodel.js poseSwing gauntlet: out, held, back on the envelope
                    // NIGHT16 build 1: the jab's OUT travel on frame time while the world is frozen or slowed (a gauntlet hit
                    // freezes on its own step), so it is out in the freeze; the hold and the return on world time (the web's)
                    swingAge = HandsArm.AdvanceOut(swingAge, animDt, frameDt, v.swingOut);
                    double e = Envelope(swingAge, v.swingOut, v.swingHold, v.swingTime);
                    pz += v.swingZ * e;
                    rx += v.swingPitchDeg * Deg * e;
                    Fist36Swing(e, swingAge, v.swingOut, ref py, ref pz, ref rz); // NIGHT36 F fist36: the lunge, the corkscrew, the follow-through
                    Fist36GuardGun(ref py, ref pz, ref rx, ref rz); // NIGHT36 F fist36: the parry36 guard
                }
                bool forgedGun = game.forge != null && game.forge.IsForged(held);
                // NIGHT16 U3-29.6: the LONGWAVE's draw sweeps its charge glow (x 1 + sweepGlow at the sweep's peak)
                double sweepBoost = held == "rail" ? 1 + UnityTuning.HandsLongwaveSweepGlow * drawGlow : 1; // x flashIntensity, gated
                AnimateParts(rig, lo, animDt, (forgedGun ? forgeGlow : 1) * sweepBoost);
                double glint = DrawGlint(rig, lo, animDt); // viewmodel.js :845-855
                double flare = held == "gauntlet" ? drawGlow : 0; // the GAUNTLET's draw: the gem flares as it clenches
                double quickdraw = FeelQuickdraw(); // NIGHT17 U3-30.6: QUICKDRAW's window brightens the gem with the strip
                flare = Math.Max(flare, quickdraw * UnityTuning.HandsQuickdrawGem / UnityTuning.HandsGauntletGemFlare);
                // The core gem: brightness follows FRESH (dim when stale), a white-hot flash when it fires; the gauntlet's
                // flickers in a parry window; a forged gun's burns brighter and whiter with a slow pulse (NIGHT6 B1).
                double fresh = lo.arsenal != null && lo.arsenal.enabled ? Clamp(lo.arsenal.Fresh(held), 0, 1) : 1;
                double fl = rig.flash / v.gemFlashTime * rig.flashScale;
                bool parry = held == "gauntlet" && lo.parryReady;
                double forgedGlow = forgedGun ? forgeGem * (0.85 + 0.15 * Math.Sin(time * 4)) : 1;
                double k = 2.4 * UnityTuning.HandsGemGain * (v.gemFreshMin + (1 - v.gemFreshMin) * fresh) * (1 + v.gemFlash * fl) * (1 + drawGlintGain * glint) * forgedGlow
                    * (1 + UnityTuning.HandsGauntletGemFlare * flare); // coreGem intensity 2.4; :861 x (1 + glintGain x glint)
                double toWhite = parry ? 0.5 + 0.5 * Math.Sin(time * 40) : Math.Max(Math.Max(fl * 0.6, glint * 0.7), Math.Max(forgedGlow > 1 ? 0.25 : 0, flare * 0.6));
                rig.gem.SetColor(BaseColor, Color.Lerp(rig.gemColor, Color.white, (float)toWhite) * (float)k);
                // the recoil (pitch deg, back m, drop m, roll deg)
                rx += recoil.Sample(0) * Deg;
                pz += recoil.Sample(1);
                py -= recoil.Sample(2);
                rz += recoil.Sample(3) * Deg;
                if (armImpact.Active)
                {
                    // :875-878 the momentum hit's drive (after the recoil, as the web's)
                    rx += armImpact.Value(0) * Deg;
                    pz += armImpact.Value(1);
                    py -= armImpact.Value(2);
                    rz += armImpact.Value(3) * Deg;
                }
                if (rig.cylinder != null)
                {
                    // :880-888 the cylinder turns toward the chamber of the shot count, forward only
                    int chambers = (int)Math.Round(360 / v.cylinderStepDeg);
                    double target = rig.shots % chambers * v.cylinderStepDeg * Deg;
                    double stepRad = v.cylinderStepDeg * Deg / v.cylinderTurnTime * animDt;
                    double delta = target - rig.cylinderAngle;
                    if (delta < -Math.PI) delta += 2 * Math.PI; // 300 deg -> 0 deg keeps turning forward
                    rig.cylinderAngle = (rig.cylinderAngle + Math.Sign(delta) * Math.Min(Math.Abs(delta), stepRad)) % (2 * Math.PI);
                    // three rotation.z: the same in Unity; + the KESTREL's draw spin (a whole turn: it ends on the same chamber)
                    rig.cylinder.localRotation = Quaternion.Euler(0, 0, (float)((rig.cylinderAngle + drawPose.spinRad) / Deg));
                }
            }
            Hammer36Frame(animDt, frameDt); // NIGHT36 F crunch36: the hammer's fall and re-cock
            if (rigs.TryGetValue("shotgun", out Rig sg) && sg.part != null)
            {
                // :900-905 the pump slides back (web +z) and comes home; the gun tilts with it
                sg.pumpAge += animDt;
                double pumpDur = Pump40Duration(v.pumpDuration); // NIGHT40 W1 pump40 (Viewmodel.Pump40.cs)
                double w = sg.pumpAge < pumpDur ? Math.Sin(Math.PI * (sg.pumpAge / pumpDur)) : 0;
                double rack = sg == rig ? drawPose.pump : 0; // NIGHT16 U3-29.6: the THUNDERCLAP's draw racks the pump (its tilt is in the draw's pose)
                sg.part.localPosition = sg.partRest - new Vector3(0, 0, (float)(v.pumpTravel * Math.Max(w, rack)));
                if (sg == rig) rz += v.pumpTiltDeg * Deg * w;
            }
            Charge35Glow(rig, lo, held, frameDt); // NIGHT35 W: charge35's tip, barrel glow and gem (Viewmodel.Charge35.cs)

            // POLISH V2: zoomed, the gun eases to its ADS pose (centred, a slight tilt) and every layer steadies.
            // NIGHT33 view33 (Viewmodel.View33.cs): the zoom eases over 120 ms, the gun stays at its hold pose (lowered, no roll)
            double zoomEase = ZoomEase(gun.ease);
            bool z47 = Zoom47For(held); // NIGHT47 F zoom47 (Viewmodel.Zoom47.cs): the THUNDERCLAP's shouldered zoom (off: NIGHT46's)
            if (z47) zoomEase = lo.adsActive ? UnityTuning.Zoom47InS : UnityTuning.Zoom47OutS;
            adsAmount = lo.adsActive ? Math.Min(1, adsAmount + frameDt / zoomEase) : Math.Max(0, adsAmount - frameDt / zoomEase);
            double zoom = 1 + (ZoomOf(held, gun) - 1) * adsAmount; // NIGHT20: the STITCHER's from design.zoom
            double ak = Smooth(adsAmount);
            if (z47)
            {
                ak = Zoom47.EaseOut(adsAmount);
                zoom = 1 + (Zoom47.FactorFor(baseFov, UnityTuning.Zoom47FovShare) - 1) * ak;
                Zoom47.Ease = (float)ak;
                Zoom47.Fov = (float)CombatMath.ZoomedFov(baseFov, zoom);
            }
            double damp = 1 - (1 - v.adsSwayMult) * ak;
            // NIGHT5 F2: the gun takes a share of the camera's shake, so it reads as held by the shaking view.
            double calm = mc.settings.reducedMotion ? 0 : 1;
            var shake = game.trauma.Sample(traumaP, intensity * calm);
            double share = v.vmShare;
            ZoomPose33.Pose zp = ZoomPose33.Of(view33, ak, UnityTuning.ZoomLower33, gun.holdX, gun.holdY, gun.holdZ, gun.holdPitch, gun.holdYaw,
                gun.adsX, gun.adsY, gun.adsZ, gun.adsPitch, gun.adsYaw, gun.adsRoll);
            if (z47 && view33) zp = Zoom47Pose(zp, ak, adsAmount, gun.holdX, gun.holdYaw); // NIGHT47 F zoom47: to the shoulder, the barrel on the reticle's line
            double x = zp.x + px * damp;
            double y = zp.y + py * damp;
            double z = zp.z + pz * damp;
            double pitchDeg = zp.pitchDeg + rx * damp / Deg + shake.pitch * share;
            double yawDeg = zp.yawDeg + ry * damp / Deg + shake.yaw * share;
            double rollDeg = rz * damp / Deg + zp.rollDeg + shake.roll * share;
            root.localPosition = new Vector3((float)x, (float)y, (float)-z);
            // three's root rotation (order YXZ, as Quaternion.Euler) under the z flip: yaw and pitch negate, roll stays
            root.localRotation = Quaternion.Euler((float)-pitchDeg, (float)-yawDeg, (float)rollDeg);
            DrawOutFrame(held, show, x, y, z, pitchDeg, yawDeg, rollDeg, damp); // round 2: the outgoing gun on its own root
            StepGhosts(animDt, show, held);
            DrawBand(rig, show); // NIGHT16 U3-29.6: the LONGWAVE's sweep band
            ScopeFrame(held, show, ak, px * damp, py * damp); // NIGHT20 U3-34.3: the LONGWAVE's scope rims (Viewmodel.Scope20.cs)
            FeelGlintBand(rig, show, FeelQuickdraw()); // NIGHT17 U3-30.6: QUICKDRAW's strip along the gun
            FeelAuraFrame(); // NIGHT17 round 3: the style aura on the gun in hand (the cyan ladder by rank)
            // 8. The left arm: the punch (it shares the sway + bob so it feels attached). viewmodel.js :924-928
            ArmRender(animDt, frameDt, show);

            // player.js :414-421: the punch drawn soft-capped, x intensity, none with reduced motion, x adsMult by the zoom
            double punchScale = intensity * calm * (1 - (1 - v.punchAdsMult) * adsAmount);
            PunchPitchRad = SpringShape.SoftCap(punch.Sample(0), punchCap) * punchScale * Deg;
            PunchYawRad = SpringShape.SoftCap(punch.Sample(1), punchCap) * punchScale * Deg;
            return (float)CombatMath.ZoomedFov(baseFov, zoom);
        }

        /// <summary>main.js wallAhead (NIGHT4 G2): m from the drawn eye to the wall straight ahead (infinity when none).</summary>
        double WallAhead()
        {
            Transform c = game.cam.transform;
            Vec3 o = WebSpace.FromUnity(c.position), d = WebSpace.FromUnity(c.forward);
            return game.world.RayIntersect(o, d, out RayHit hit) ? hit.distance : double.PositiveInfinity;
        }

        /// <summary>player.js landingDip (frame time): eases in over landingDipAttack, springs back over landingDipReturnTime.</summary>
        double LandingDip(double frameDt)
        {
            FeelConfig f = cfg.movement.feel;
            double attack = f.landingDipAttack, back = f.landingDipReturnTime;
            if (dipTime >= attack + back) return 0;
            dipTime += frameDt;
            if (dipTime < attack) return dipAmount * Math.Sin(Math.PI / 2 * (dipTime / attack));
            double k = 1 - Clamp((dipTime - attack) / back, 0, 1);
            return dipAmount * k * k * k;
        }

        /// <summary>
        /// viewmodel.js stepGhosts (:610-633): while a swing's afterimages live, the rig's place is sampled every
        /// afterimages.every s; copy i stands where the gun was (i + 1) x spacing samples ago, fading over the life.
        /// </summary>
        void StepGhosts(double dt, bool show, string held)
        {
            Rig rig = ghostRig;
            bool on = rig != null && ghostAge < v.afLife && held == rig.name && !cfg.movement.settings.reducedMotion && show;
            if (!on) return;
            ghostAge += dt;
            ghostSample += dt;
            Transform cam = vmCam.transform;
            if (ghostSample >= v.afEvery)
            {
                ghostSample = 0;
                ghostHead = (ghostHead + ghostHistory.Length - 1) % ghostHistory.Length; // unshift (the oldest falls off)
                ghostHistory[ghostHead] = cam.worldToLocalMatrix * rig.t.localToWorldMatrix;
                ghostCount = Math.Min(ghostHistory.Length, ghostCount + 1);
            }
            float fade = (float)(1 - ghostAge / v.afLife);
            if (fade <= 0) return;
            Matrix4x4 toWorld = cam.localToWorldMatrix;
            Color fam = rig.gemColor * (float)v.afGlow;
            for (int i = 0; i < ghostMats.Length; i++)
            {
                int age = (i + 1) * v.afSpacing;
                if (age >= ghostCount) break;
                ghostMats[i].SetColor(BaseColor, fam * (float)v.afOpacity[i] * fade); // additive: the opacity in the colour
                Matrix4x4 at = toWorld * ghostHistory[(ghostHead + age) % ghostHistory.Length];
                for (int m = 0; m < rig.filters.Length; m++)
                {
                    Mesh mesh = rig.filters[m].sharedMesh;
                    if (mesh != null) Graphics.RenderMesh(ghostParams[i], mesh, 0, at * ghostLocals[m]);
                }
            }
        }

        /// <summary>
        /// viewmodel.js animateParts() :419-448 for the kit guns ported here (world time, the gun in hand). `boost`: NIGHT6 B1
        /// applyForgeGlow (a forged gun's glows x forge.glow). The literals (0.7 + 0.3 sin 40t, 0.8 + 14 kick, 1 + 12 charge,
        /// 0.25 not ready, vcGlow 1.4 / 1.2) are viewmodel.js's / weaponModels.js's own.
        /// </summary>
        void AnimateParts(Rig rig, Loadout lo, double dt, double boost)
        {
            switch (rig.name)
            {
                case "nailgun":
                {
                    // :425-434 heat: the family colour when cool, white-hot amber (viewmodel.heatColor) at full heat,
                    // brighter by heatGlow, flickering while locked; the drum eases to its target (states.drumTurnRate).
                    double heat = lo.Heat;
                    double flicker = lo.Overheated ? 0.7 + 0.3 * Math.Sin(time * 40) : 1;
                    Color c = Color.Lerp(rig.gemColor, heatColor, (float)heat);
                    rig.glow.SetColor(BaseColor, c * (float)(1.4 * (1 + heat * heatGlow) * flicker * boost));
                    rig.drumAngle += (rig.drumTarget - rig.drumAngle) * (1 - Math.Exp(-drumTurnRate * dt));
                    // web drum.rotation.x = drumAngle; the z flip turns a rotation about x the other way
                    rig.drum.localRotation = Quaternion.Euler(-(float)(rig.drumAngle % (2 * Math.PI) * Mathf.Rad2Deg), 0, 0);
                    break;
                }
                case "rail":
                {
                    // :437-443 the charge (and the ring, on the same glow) brightens with loadout.railCharge; dim until the
                    // cooldown is done; the ring turns faster with the charge.
                    double charge = lo.RailCharge;
                    double ready = lo.CooldownFraction(cfg.Dt) <= 0 ? 1 : 0.25;
                    // U3-30.10: the kill flourish's hum lights the coil in the player's cyan (x 1 + HumGlow), ready or not
                    Color coil = feelFlourishGlow > 0 ? Color.Lerp(rig.gemColor, feelUsable, (float)feelFlourishGlow) : rig.gemColor;
                    double hum = 1 + UnityTuning.HandsFlourishHumGlow * feelFlourishGlow;
                    rig.glow.SetColor(BaseColor, coil * (float)(1.2 * Math.Max(ready, feelFlourishGlow) * (0.8 + chargeGlow * charge * charge) * boost * hum));
                    rig.spin += dt * (1 + 12 * charge);
                    rig.ring.localRotation = Quaternion.Euler(0, 0, (float)(rig.spin % (2 * Math.PI) * Mathf.Rad2Deg));
                    break;
                }
                case "rocket":
                {
                    // :445-447 the ring spins up with the kick (the recoil's pitch as a share of viewmodel.recoil.rocket.kickDeg)
                    double kick = Math.Max(0, Math.Min(1, (recoil.Sample(0) + feelKick.Sample(0)) / rocketKickDeg)); // + the COMET's own kick (NIGHT17)
                    rig.spin += dt * (0.8 + 14 * kick);
                    rig.ring.localRotation = Quaternion.Euler(0, 0, (float)(rig.spin % (2 * Math.PI) * Mathf.Rad2Deg));
                    break;
                }
                default:
                    PortParts(rig, lo, dt, boost); // NIGHT19: the ZAPPER's coils and arc, the BOUNCER's drum (Viewmodel.Ports.cs)
                    break;
            }
        }

        /// <summary>
        /// x the look sensitivity while zoomed: the zoom's tangent ratio (ads.js), x settings.adsSensitivity eased in with the
        /// zoom (ads.js: 1 + (adsSensitivity - 1) x amount).
        /// </summary>
        public double AdsSensitivity(double baseFov, double adsSensitivity = 1)
        {
            if (adsAmount <= 0) return 1;
            string w = game.loadout.Weapon;
            double zoomSens = CombatMath.ZoomSensitivity(baseFov, CombatMath.ZoomedFov(baseFov, 1 + (ZoomOf(w, GunOf(w)) - 1) * adsAmount));
            return zoomSens * (1 + (adsSensitivity - 1) * adsAmount);
        }

        /// <summary>viewmodel.js envelope: a sine rise over `rise`, a hold, then a smoothstep fall until `total`.</summary>
        static double Envelope(double t, double rise, double hold, double total)
        {
            if (t < 0 || t >= total) return 0;
            if (t < rise) return Math.Sin(Math.PI / 2 * (t / rise));
            if (t < rise + hold) return 1;
            return 1 - Smooth((t - rise - hold) / (total - rise - hold));
        }

        static double Smooth(double u) => u * u * (3 - 2 * u);

        static double Mix(double a, double b, double k) => a + (b - a) * k;

        static double EaseFactor(double rate, double dt) => 1 - Math.Exp(-rate * dt);

        static double Clamp(double x, double lo, double hi) => Math.Max(lo, Math.Min(hi, x));
    }
}
