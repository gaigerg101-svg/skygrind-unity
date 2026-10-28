// NIGHT13 U3-26.6: the sim composition (audit-perf section 5 move 6). A plain C# class with no UnityEngine use: it owns
// the run's Core systems, builds and wires them in the order SurvivalGame.Init did (every sim-affecting EventBus
// registration in today's sequence: handler order is behaviour, EventBus.cs:27-31), and steps them in main.js
// simulate() order (SurvivalSim.Run.cs). SurvivalGame keeps the boot, the frame loop (the accumulator, hitstop's
// clock, interpolation), the devices and every view; it forwards its old fields here so no view changes.
// What the sim needs from outside is injected: the player's input (IPlayerInput, the look included), the settings it
// reads (aimAssist, snapStrength, damageDirection, the LAB toggles, FIRST-RUN GUIDE), the camera test the brain's
// watchdog asks (onScreen: a render -> sim dependency kept from the web, pinned in the fingerprint), the tips' key
// labels, the profile (and a callback that writes it), and the callbacks a run's flow fires for the Game (the run
// ended, the views reset at a new run). Headless (the fingerprint test, the run snapshot's proof) passes defaults.
// Also here: the sim half of the ARMORY wall buys (wallBuys.js: the buyables) and of the perk machines (perks.js:
// the buyables, apply, MEND's buy point riding its pillar), which lived in the WallBuys / PerkMachines views.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public readonly GameConfig cfg;
        public readonly SurvivalMap map;
        public readonly EventBus events;
        public readonly double step;
        public IPlayerInput input;
        public readonly Lab lab;
        public CoreGrid core;
        public GameWorld world;
        public Zones zones;
        public Mover mover;
        public Health health;
        public Regen regen;
        public Economy economy;
        public SurvivalRun run;
        public IRoundSpawner spawner;
        public readonly EnemyRegistry enemies = new EnemyRegistry();
        public Loadout loadout;
        public Projectiles projectiles; // NIGHT10 U3-13.3: nails and rockets in flight (projectiles.js)
        public Ultra ultra; // NIGHT10 U3-13.9: design.ultra (the ULTRAKILL alignment)
        public CombatInput lastCombat;
        public Husks husks;
        public AimAssist assist; // the player's lock (aimAssist.js): settings.aimAssist, at settings.snapStrength
        public NavField nav;
        public HuskBrain brain;
        public Director director;
        public Buyables buyables;
        public StyleMeter style;
        public readonly Players players = new Players();
        public Player p1;
        public Perks perks; // U2-11: the run's perks (perks.js)
        public PowerUps powerUps; // U2-12: kill drops (powerUps.js)
        public Mods mods; // U2-13: MODS drafts (mods.js)
        public MysteryBox box; // NIGHT10 U3-13.8: null when design.box.enabled is off (the web's game)
        public Buyables.Item boxBuy, boxTake;
        string boxTakeFor;
        readonly Func<string, bool> fires = Loadout.Fires;
        public Objectives objectives; // NIGHT11 U3-24.3: objective rounds (objectives.js: HOLD, GOLD RUSH)
        public Protocol protocol; // NIGHT11 U3-24.4: THE PROTOCOL (protocol.js)
        public Traps traps; // NIGHT11 U3-24.5: the four traps (traps.js)
        public RunMoments moments; // NIGHT11 U3-24.8: the run's best moments (runMoments.js)
        RunMoments.Grade gradeP; // results.grade, read once
        public Power power; // NIGHT11 U3-24.6: the power switch (design.power; port-only)
        public SwarmDrop swarmDrop; // NIGHT11 U3-24.3: the swarm round's banner and drop (design.zombies.swarmDrop; port-only)
        public Exfil exfil; // NIGHT11 U3-24.7: exfil (design.exfil; port-only)
        public UltraHealth ultraHealth; // NIGHT11 U3-24.9: ULTRA HEALTH (the LAB's lab.ultraHealth; off by default)
        public Forge forge; // NIGHT8 U2-17
        public Flow flow = Flow.Title;
        public RunStats lastStats;
        // U2-05: the death recap and the damage-direction arcs (main.js OVERNIGHT3 P4), personal bests (records.js)
        public DeathRecap recap;
        public HitArcs hitArcs;
        public DeathRecap.Summary lastDeath; // the recap at the death (DOWN, RESULTS)
        public string downTarget; // DOWN's next target line (NIGHT6 B5), or null
        public Records records;
        public Records.Judgement lastPb;
        public bool noRecords; // automation (the smoke): runs are judged, never stored as bests
        public bool noLab; // automation (the smoke): the web's rules whatever the LAB tab says
        public readonly Profile profile;
        public readonly RunSetup setup = new RunSetup();
        public Contracts contracts;
        public Mastery mastery;
        public Heat heat;
        public Tips tips;
        public Onboarding onboarding;
        Payoff.P payoffP; // design.grade.p, read once
        Mutators.Targets mutTargets; // the systems a mutator changes (damage both ways, enemy pace, points, regen)
        bool runRecording; // the running run changes the profile (Recording at its start)
        bool runOnboarding; // the first-run script ran this run
        int readySteps, readyHoldSteps; // NIGHT6 B6 READY: the BUY hold in a break
        Dictionary<string, string> recapNames;
        public uint runSeed;
        public int downSteps;
        public Vec3 previous, current;
        public int simStep; // whole steps since boot
        /// <summary>main.js hitstop: the longest request wins (the frame loop counts it down and skips the frozen time).</summary>
        public double hitstopLeft;
        public bool hitstopRequested;
        double enemyNearRange, campDrainMult; // read once (NIGHT12: they were read every step)
        readonly TipEvent tipEvent = new TipEvent();
        Buyables.Item tipCandidate;

        // Injected (SurvivalGame: GameSettings, the camera, Ui.KeyLabel, the profile file, the cursor and the views).
        readonly Func<string, double> settingNum;
        readonly Func<string, bool> settingBool;
        readonly Func<string, string> settingStr;
        /// <summary>Write the profile (null: never written; the Game checks profileReadOnly).</summary>
        public Action saveProfile;
        /// <summary>A run ended (EndRun: the Game frees the cursor).</summary>
        public Action runEnded;
        /// <summary>NIGHT13 U3-26.7: the assist mode while the pad is the device (main.js :377: SNAP MODE (PAD)); null: the mouse's setting.</summary>
        public Func<string, string> assistModeFor;
        /// <summary>NIGHT13 U3-26.7: the pad's breakaway cap (main.js :378-379: yawRate x padSensX x breakawayShare); null: +infinity (the mouse's).</summary>
        public Func<double> breakawayCap;

        /// <summary>The ARMORY wall's buyable guns (wallBuys.js), in the order the wall shows them (WallBuys draws them).</summary>
        public sealed class WallGun
        {
            public string id, name;
            public int wallIndex; // map.armoryWallZs index (-1: a map spot's gun, NIGHT15)
            public double price;
            public Buyables.Item item;
            /// <summary>NIGHT15: where the plate hangs (its centre on the wall) and the way it faces out (web x / z, unit).</summary>
            public Vec3 at;
            public double fx, fz;
        }

        public readonly List<WallGun> wallGuns = new List<WallGun>();

        /// <summary>A perk machine or THE FORGE's cabinet (perks.js / forge.js load); PerkMachines draws it.</summary>
        public sealed class Machine
        {
            public PerkSpot spot;
            public int zone;
            public bool forge;
            public Buyables.Item item;
        }

        public readonly List<Machine> machines = new List<Machine>();

        public SurvivalSim(GameConfig cfg, SurvivalMap map, EventBus events, Lab lab, IPlayerInput input, Profile profile,
            Func<string, double> settingNum, Func<string, bool> settingBool, Func<string, string> settingStr,
            Func<Vec3, bool> onScreen, Func<string, string> keyLabel)
        {
            this.cfg = cfg;
            this.map = map;
            this.events = events;
            this.lab = lab;
            this.input = input;
            this.profile = profile;
            this.settingNum = settingNum;
            this.settingBool = settingBool;
            this.settingStr = settingStr;
            MovementConfig mc = cfg.movement;
            step = 1 / mc.loop.simHz;

            core = map.CreateCore(mc);
            world = new GameWorld(map.BuildOctree(), core);
            zones = new Zones(cfg, map, world, events);

            mover = new Mover(mc, world) { ropeEnabled = false }; // the ARCLINE is a 5500 gadget in Survival (not owned at start)
            health = new Health(cfg.Num("health.max"), events, "player", () => mover.invulnerable);
            regen = new Regen(cfg, events, health, () => mover.Speed);
            economy = new Economy(cfg, events);
            loadout = new Loadout(cfg, events, enemies, world, mover, health, Aim);
            assist = new AimAssist(cfg, events, enemies, world, Aim, () => loadout.Weapon, () => loadout.adsActive, () => AssistMode, () => settingNum("snapStrength"), BreakawayCap);
            loadout.assist = assist;
            projectiles = new Projectiles(cfg, events, world, enemies);
            loadout.projectiles = projectiles;
            ultra = Ultra.From(cfg); // NIGHT10 U3-13.9: the ULTRAKILL alignment flags (design.ultra; SurvivalGame logs them at boot)
            loadout.freshDamage = ultra.freshDamage;
            loadout.parryRefillsDash = ultra.parryRefillsDash;
            husks = new Husks(cfg, events, enemies, world, map.killPlaneY);
            husks.jumper = new HuskNavJumper(cfg, events);
            nav = new NavField(cfg, NavGrid.FromExport(map.navGrid));
            nav.SetBlocked(zones.LockedTag);
            events.On("gateOpen", _ => nav.SetBlocked(zones.LockedTag)); // a gate only unmasks its zone (no rebuild)
            events.On("zoneClear", _ => nav.SetBlocked(zones.LockedTag));
            brain = new HuskBrain(cfg, events, nav, world.Geometry);
            brain.onScreen = onScreen;
            director = new Director(cfg, events, husks, brain, zones, core, world.Geometry, nav, () => mover.position, () => this.input.Yaw, () => mover.Speed);
            director.huskWorld = world;
            spawner = campaignSpawner = new Campaign24Spawner(director); // NIGHT24 (campaign-core): a wrapper that passes through unless a campaign run is on
            // U2-10: the spitters' aim reads your velocity and the camping flag; a punch reflects their globs; a reflected
            // glob's hit is the player's (weapon "orb": RETURN TO SENDER); the brain's hit roll reads your speed.
            brain.playerSpeed = () => mover.Speed;
            husks.acid.targetVelocity = () => mover.velocity;
            husks.acid.hunted = () => director.Hunted;
            husks.acid.onReflectHit = (e, damage, at) => loadout.Damage(e, damage, false, "orb", at);
            loadout.acid = husks.acid;
            economy.campMult = () => director.Exposed ? cfg.Num("points.campMult") : 1; // EXPOSED halves combat points
            regen.camping = () => director.Exposed; // and pauses regen (regen.js campingSource)
            buyables = new Buyables(cfg, events, economy);
            foreach (BuySpot s in zones.spots)
            {
                BuySpot spot = s;
                buyables.list.Add(new Buyables.Item
                {
                    id = $"gate:{spot.zoneId}", name = spot.name, kind = "gate", position = spot.at, reach = cfg.Num("zones.buyReach"),
                    price = () => spot.price, available = () => zones.SpotBuyable(spot), buy = () => zones.Open(spot.zoneId),
                });
            }
            // The style meter (styleMeter.js): the rank multiplies points, speeds regen and stamina (as on the web).
            double trickRange = cfg.Num("style.trickEnemyRange");
            style = new StyleMeter(cfg, events, () => PortData.Name(mover.state), () =>
            {
                foreach (Enemy e in enemies.all)
                {
                    if (!e.alive) continue;
                    double dx = e.position.x - mover.position.x, dy = e.position.y - mover.position.y, dz = e.position.z - mover.position.z;
                    if (dx * dx + dy * dy + dz * dz <= trickRange * trickRange) return true;
                }
                return false;
            });
            p1 = players.Add(new Player("player", mover, health) { regen = regen, loadout = loadout, assist = assist, style = style });
            // NIGHT9 U3-08 (main.js POWER P3): one FRESH per weapon: the arsenal's (while it runs) sets the style freshness
            // of its weapons too (punch and orb are the Gauntlet's); a source it doesn't track keeps the meter's own.
            Arsenal arsenal = loadout.arsenal;
            style.getFresh = w => arsenal.enabled && arsenal.Catalog.Has(w == "punch" || w == "orb" ? "gauntlet" : w) ? arsenal.Fresh(w) : (double?)null;
            economy.rank = () => style.rank;
            regen.rank = () => style.rank;
            mover.Events += e =>
            {
                switch (e.type)
                {
                    case MoverEventType.WallJump: events.Emit("trick", "wallJump"); break;
                    case MoverEventType.DashJump: events.Emit("trick", "dashJump"); break;
                    case MoverEventType.BoostJump: events.Emit("trick", "boostJump"); break;
                    case MoverEventType.JumpPad: events.Emit("trick", "padLaunch"); break;
                    case MoverEventType.Jump: if (e.kind == JumpKind.Slide) events.Emit("trick", "slideJump"); break;
                    case MoverEventType.Slam: events.Emit("slamUse", "slam"); break;
                }
            };
            MoveBalanceInit(); // NIGHT16 U3-29.4: design.move read once, the mover's events (SurvivalSim.MoveBalance.cs, builder M)
            StaminaInit(); // NIGHT17 U3-30.4: design.stamina read once, the mover's stamina events, the kill / parry rewards (SurvivalSim.Stamina.cs, builder M)
            N19Init(); // NIGHT19: the seven NIGHT19 design blocks, read once (SurvivalSim.N19.cs)
            N20Init(); // NIGHT20: the eight NIGHT20 design blocks, read once (SurvivalSim.N20.cs)
            N21Init(); // NIGHT21: the six NIGHT21 design blocks, read once (SurvivalSim.N21.cs)
            N26Init(); // NIGHT26: the seven NIGHT26 design blocks, read once (SurvivalSim.N26.cs)
            N27Init(); // NIGHT27: the NIGHT27 design blocks, read once (SurvivalSim.N27.cs)
            N29Init(); // NIGHT29: the NIGHT29 design blocks, read once (SurvivalSim.N29.cs)
            N30Init(); // NIGHT30: the NIGHT30 design blocks, read once (SurvivalSim.N30.cs)
            N31Init(); // NIGHT31: the NIGHT31 design blocks, read once (SurvivalSim.N31.cs)
            N32Init(); // NIGHT32: the NIGHT32 design blocks, read once (SurvivalSim.N32.cs)
            N33Init(); // NIGHT33: the NIGHT33 design blocks, read once (SurvivalSim.N33.cs)
            N34Init(); // NIGHT34: the NIGHT34 design blocks, read once (SurvivalSim.N34.cs)
            N35Init(); // NIGHT35: the NIGHT35 design blocks, read once (SurvivalSim.N35.cs)
            N36Init(); // NIGHT36: the NIGHT36 design blocks, read once (SurvivalSim.N36.cs)
            N37Init(); // NIGHT37: the NIGHT37 design blocks, read once (SurvivalSim.N37.cs)
            N38Init(); // NIGHT38: the NIGHT38 design blocks, read once (SurvivalSim.N38.cs)
            N39Init(); // NIGHT39: the NIGHT39 design blocks, read once (SurvivalSim.N39.cs)
            N40Init(); // NIGHT40: the NIGHT40 design blocks and level rows, read once (SurvivalSim.N40.cs)
            N41Init(); // NIGHT41: the NIGHT41 design blocks and level rows, read once (SurvivalSim.N41.cs)
            N42Init(); // NIGHT42: the NIGHT42 design blocks and level rows, read once (SurvivalSim.N42.cs)
            N43Init(); // NIGHT43: the NIGHT43 design blocks and level rows, read once (SurvivalSim.N43.cs)
            RowsInit(); // NIGHT46 rows (docs/NIGHT-ROWS.md): every NIGHT46+ sim row, read once (SurvivalSim.Rows.cs, Core/NightRows.cs)
            N25Init(); // NIGHT25: the six NIGHT25 design blocks, read once (SurvivalSim.N25.cs)
            N24Init(); // NIGHT24: the nine NIGHT24 design blocks, read once (SurvivalSim.N24.cs)
            N22Init(); // NIGHT22: the six NIGHT22 design blocks, read once (SurvivalSim.N22.cs)
            Beacon20Ensure(); // NIGHT20 A3 (builder P2): THE BEACON (SurvivalSim.Beacon20.cs; it stands only with design.adds.beacon)
            AddWallBuys(); // the ARMORY wall's buyables (they were WallBuys' constructor's, here in its place)
            BuildBox();
            // NIGHT11 U3-24.3 (main.js survivalHooks): objectives.begin right after the director plans the round (its
            // roundPlan event, before roundBegin, as hooks.onRoundBegin runs before survival emits roundBegin); the
            // round can't clear while a HOLD is short (hooks.holdRound); its clock runs after the spawner (hooks.step).
            objectives = new Objectives(cfg, events, economy, zones, () => mover.position);
            events.On<RoundEvent>("roundPlan", e =>
            {
                if (run != null && run.Live) objectives.Begin(e.number);
            });
            perks = new Perks(cfg, events);
            forge = new Forge(cfg, events); // NIGHT8 U2-17: before the machines, which build its cabinet
            AddMachines(); // the perk machines' and THE FORGE's buyables and perks.apply (they were PerkMachines' constructor's)
            // U2-12: kill drops and power-ups (powerUps.js), stepped in the mode's part of the step.
            powerUps = new PowerUps(cfg, events)
            {
                playerPos = () => mover.position, playerGrounded = () => mover.grounded, isLive = () => flow == Flow.Playing && run.Live,
                economy = economy, enemies = enemies, kill = (e, damage, at) => loadout.Damage(e, damage, false, "shockwave", at),
                setStasis = on => husks.frozen = on, setOverclock = on => loadout.arsenal.SetOverclock(on), // powerUps.js: the arsenal's OVERCLOCK
                floorUnder = p => world.RayIntersect(new Vec3(p.x, p.y + 1, p.z), new Vec3(0, -1, 0), out RayHit hit) ? p.y + 1 - hit.distance : 0,
            };
            // U2-13: MODS (mods.js): the bonus hook on every hit, the kill effects, SCAVENGER on the drops; a locked mod
            // waits for its flag (only REACH ROUND 10 can be earned here: a best round of 10 or more).
            mods = new Mods(cfg, events)
            {
                isUnlocked = flag => (flag == "round10" && Math.Max(records.Best("survival")?.round ?? 0, records.Best(RecordsKey)?.round ?? 0) >= 10) || (contracts != null && contracts.Unlocked(flag)), // NIGHT12: unlocks.js flags
                airborne = () => !mover.grounded, moveState = () => PortData.Name(mover.state), eye = () => Eye, styleRank = () => style.rank,
                health = health, economy = economy,
            };
            // U2-17: THE FORGE (forge.js; its cabinet is built with the perk machines, above): the run's bonus hook is
            // forge x mods (main.js runBonus), and a forged gun's status roll runs after each hit.
            loadout.bonus = (e, w, head) => forge.Mult(w, head, e) * mods.Mult(e);
            loadout.after = forge.After;
            forge.status = loadout.arsenal.Status; // NIGHT9 U3-08: a forged gun's status lands (forge.js loadout.arsenal.status)
            powerUps.dropMult = mods.DropMult;
            BuildRunShape();
            enemyNearRange = cfg.Num("style.trickEnemyRange");
            campDrainMult = cfg.Num("director.camp.styleDrainMult");
            // main.js: a player kill or parry freezes the sim (the longest request wins).
            double hitstopKill = cfg.Num("feel.hitstopTime");
            events.On<KillEvent>("kill", e =>
            {
                if (e.owner == "player") Hitstop(hitstopKill, "kill");
            });
            // OVERNIGHT2 N8 (NIGHT9 U3-09): a melee hit with real momentum lands with a short freeze (not a wave's share: once a swing).
            double momentumHitstopAt = cfg.Num("feel.momentumHitstopAt"), momentumHitstop = cfg.Num("feel.momentumHitstop");
            events.On<HitEvent>("hit", e =>
            {
                if (e.owner == "player" && e.melee && !e.shock && e.momentum > momentumHitstopAt) Hitstop(momentumHitstop, "momentum");
            });
            events.On<HitEvent>("parry", e =>
            {
                // NIGHT8 U2-07: a PERFECT PARRY freezes for design.parry.perfectHitstop instead.
                // NIGHT10 U3-13.9: design.ultra.parryHitstop (ULTRAKILL's 0.25 s) for a plain parry; the perfect one keeps its own.
                if (e.owner == "player") Hitstop(e.perfect ? cfg.Num("design.parry.perfectHitstop") : ultra.ParryHitstop(false, 0, cfg.Num("punch.parryHitstop")), "parry");
            });
            events.On<DeathEvent>("death", e =>
            {
                if (e.owner != "player" || flow != Flow.Playing) return;
                downSteps = MovementMath.StepsFor(cfg.Num("health.downTime"), step);
                lastDeath = recap.At(SimTime); // main.js: the results say it too
                downTarget = lab.Any ? null : SurvivalTarget(); // a LAB run sets no best, so it chases none
                flow = Flow.Down;
            });
            // main.js OVERNIGHT3 P4: every hurt feeds the recap (fixed-step time) and, with DAMAGE DIRECTION on, an arc
            // toward where it came from.
            recap = new DeathRecap(cfg.Num("ui.recap.window"));
            hitArcs = new HitArcs(cfg.Int("ui.hitDirection.max"));
            recapNames = new Dictionary<string, string>();
            foreach (var kv in cfg.Obj("ui.recap.names")) recapNames[kv.Key] = (string)kv.Value;
            events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner != "player" || !(e.amount > 0)) return;
                recap.Hurt(SimTime, e.amount, DeathRecap.SourceName(e.source, recapNames));
                if (settingBool("damageDirection") && e.hasFrom) hitArcs.Add(e.from.x, e.from.z);
            });
            // NIGHT12: records live in the JSON profile (loaded and migrated by SurvivalGame).
            records = profile.records;
            // NIGHT12: the meta (Core): CONTRACTS + unlocks (challenges.js, unlocks.js), MASTERY + finishes, HEAT (mutators.js),
            // tips (tips.js) and the first run; each reads and writes the profile.
            contracts = new Contracts(cfg, events, profile, mover, () => run?.stats.mutators);
            mastery = new Mastery(cfg, events, profile, Loadout.Fires);
            mastery.fresh = style.getFresh; // the tier-2 track: FRESH (the arsenal's) or air kills
            mastery.airborne = () => !mover.grounded;
            mastery.styleRank = () => style.rank; // the tier-3 track: kills at rank S or higher
            heat = new Heat(cfg, events, profile);
            tips = new Tips(cfg, profile, keyLabel);
            onboarding = new Onboarding(cfg, profile);
            payoffP = Payoff.P.From(cfg);
            mutTargets = new Mutators.Targets { loadout = loadout, health = health, brain = brain, economy = economy, regen = regen }; // mutators.js apply's t
            // tips.js moments (main.js NIGHT6 B6): a break (and READY from survival.readyFrom), a power-up dropped, the buy
            // prompt's kind (a wall gun, THE FORGE, a relay, a perk machine). A tip shown goes out as the port-only `tip` event
            // (the HUD's hint toast shows it).
            double readyFrom = cfg.Num("survival.readyFrom");
            events.On<RoundEvent>("roundBreak", e =>
            {
                if (run == null || !run.Live) return;
                Tip("break");
                if (e.number >= readyFrom) Tip("ready");
            });
            events.On("powerUpSpawn", _ => Tip("powerUp"));
            lab.Wire(events, health); // CLOSE-KILL HEAL (only while a run's toggle is on)
            readyHoldSteps = MovementMath.StepsFor(cfg.Num("survival.readyHold"), step);
            Respawn(map.playerSpawn, map.playerSpawnYawDeg);
            // SurvivalGame.StartGame: the run, once the systems exist.
            run = new SurvivalRun(cfg, events, spawner, economy, regen);
            run.holdRound = HoldRound; // NIGHT11: survival.js hooks.holdRound (a method group made once here, not per step)
            run.stepHook = RunStepHook; // survival.js hooks.step
        }

        /// <summary>The assist mode in use: settings.aimAssist (off | assist | snap | lock).</summary>
        public string AssistMode => assistModeFor != null ? assistModeFor(settingStr("aimAssist")) : settingStr("aimAssist");

        string recordsKey;

        /// <summary>NIGHT15 U3-28.5: the records' mode key of this composition's map (RecordsKeyFor(map.name)).</summary>
        public string RecordsKey => recordsKey ??= RecordsKeyFor(map.name);

        /// <summary>NIGHT15 U3-28.5: records keyed by mode and map: "survival" on the web's arena (where they always were), "survival@id" on another map.</summary>
        public static string RecordsKeyFor(string mapId) => string.IsNullOrEmpty(mapId) || mapId == RunSetup.DefaultMap ? RunSetup.Survival : RunSetup.Survival + "@" + mapId;

        /// <summary>The breakaway cap the lock reads (AimAssist.cs: JsMath.Min(p.breakaway, cap)): the pad's while it plays, else +infinity.</summary>
        double BreakawayCap() => breakawayCap != null ? breakawayCap() : double.PositiveInfinity;

        /// <summary>A boolean setting, or `fallback` when the key isn't a setting (read at a run's start, never per frame).</summary>
        bool SettingOr(string key, bool fallback)
        {
            try
            {
                return settingBool(key);
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        /// <summary>The running run may change the profile (not a LAB run, not automation). Valid once StartRun read the LAB.</summary>
        public bool Recording => !noRecords && !metaSilent && !lab.Any; // NIGHT14 rule 7: metaSilent (a bot's run)

        public bool Active => flow == Flow.Playing && downSteps == 0;

        /// <summary>A MODS draft is open (the game paused under it).</summary>
        public bool DraftOpen => flow == Flow.Paused && mods.offer != null;

        /// <summary>Fixed-step time since boot (the recap's clock, main.js simTime).</summary>
        public double SimTime => simStep * step;

        /// <summary>The sim's eye (feet + eye height, lowered while crouched), where shots and buys look from.</summary>
        public Vec3 Eye => new Vec3(current.x, current.y + cfg.movement.body.eyeHeight - (mover.crouched ? cfg.movement.slide.cameraDrop : 0), current.z);

        /// <summary>The player's aim: the sim eye and the look direction (yaw 0 looks toward -z, + pitch looks up).</summary>
        public (Vec3, Vec3) Aim()
        {
            double y = input.Yaw, p = input.Pitch;
            double cp = JsMath.Cos(p);
            return (Eye, new Vec3(-JsMath.Sin(y) * cp, JsMath.Sin(p), -JsMath.Cos(y) * cp));
        }

        // NIGHT16 (the shared commit): builder M's hooks (SurvivalSim.MoveBalance.cs). Init: in the constructor, after the mover's
        // events are wired. StartRun: after the LAB is read; `fromSettings` is false for automation (noLab: the design defaults).
        partial void MoveBalanceInit();
        partial void MoveBalanceStartRun(bool fromSettings);
        // NIGHT17 U3-30.4: builder M's hooks (SurvivalSim.Stamina.cs), placed as MoveBalance's.
        partial void StaminaInit();
        partial void StaminaStartRun(bool fromSettings);

        /// <summary>main.js hitstop: a request longer than the one left replaces it (the frame loop skips the frozen time).</summary>
        public void Hitstop(double duration, string cause)
        {
            if (duration <= hitstopLeft) return;
            hitstopLeft = duration;
            hitstopRequested = true;
            events.Emit("hitstop", cause);
        }

        /// <summary>The sim's half of a respawn: the mover reset there, the look, no interpolation (the Game resets its views).</summary>
        public void Respawn(Vec3 at, double yawDeg)
        {
            mover.Reset(at.x, at.y, at.z);
            if (n43.rooms43) { spire38Held = -1; spire38Dashed = false; spire38Pad = -1; } // NIGHT43 (M1 traced, the lead): a respawn (fallReturn43's lip) clears the NIGHT38 flight meter's held bar, else n43-foundry's GANTRY (flight38 inherited) zeroes the refilled dash bar and the dash after a lava fall fails
            input.SetLook(yawDeg * Math.PI / 180, 0);
            previous = current = mover.position;
        }

        /// <summary>tips.js moment(id): when the tip shows now, the port-only `tip` event carries its text (the hint toast).</summary>
        public void Tip(string id)
        {
            if (runOnboarding && run != null && !onboarding.TipAllowed(id, run.round)) return; // the first run teaches it in its round
            string text = tips?.Moment(id);
            if (text == null) return;
            tipEvent.id = id;
            tipEvent.text = text;
            events.Emit(MetaEvents.Tip, tipEvent);
        }

        /// <summary>main.js frame (NIGHT6 B6): the buy prompt's kind has a tip (weapon, forge, relay, perk); asked when the prompt changes.</summary>
        public void TipForCandidate()
        {
            Buyables.Item c = Active && run.Live ? buyables.candidate : null;
            if (ReferenceEquals(c, tipCandidate)) return;
            tipCandidate = c;
            if (c == null) return;
            switch (c.kind)
            {
                case "weapon": Tip("wallBuy"); break;
                case "forge": Tip("forge"); break;
                case "relay": Tip("relay"); break;
                case "perk": Tip("perk"); break;
            }
        }

        /// <summary>
        /// wallBuys.js load: the ARMORY guns ported so far, in config.weapons.prices order (melee left out), one per wall slot
        /// (map.armoryWall): hold E at wallBuy.buyOut m from the wall; the gun takes the next slot and goes in hand.
        /// </summary>
        void AddWallBuys()
        {
            var names = new List<string>();
            foreach (var kv in cfg.Obj("weapons.prices")) if (cfg.Str($"weapons.catalog.{kv.Key}.kind") != "melee") names.Add(kv.Key);
            double buyOut = cfg.Num("wallBuy.buyOut");
            // NIGHT15 U3-28.6: a Unity-authored map hangs single guns at its own spots (spots.wallBuys: weapon, x, y = the
            // plate's centre, z, fx / fz = the way it faces, floor = the buy point's height), one per zone; the web's maps
            // keep the ARMORY wall below.
            if (map.spots != null && map.spots.TryGetValue("wallBuys", out object spotList) && spotList != null)
            {
                foreach (object o in PortData.Arr(spotList))
                {
                    var s = PortData.Obj(o);
                    string id = (string)s["weapon"];
                    if (!Loadout.Fires(id) || !cfg.Has($"weapons.prices.{id}")) continue;
                    var at = new Vec3(PortData.Num(s["x"]), PortData.Num(s["y"]), PortData.Num(s["z"]));
                    double fx = PortData.Num(s["fx"]), fz = PortData.Num(s["fz"]), floor = PortData.Num(s["floor"]);
                    AddWallGun(id, -1, at, fx, fz, new Vec3(at.x + fx * buyOut, floor, at.z + fz * buyOut));
                }
                return;
            }
            for (int i = 0; i < names.Count && i < map.armoryWallZs.Count; i++)
            {
                string id = names[i];
                if (!Loadout.Fires(id)) continue; // the guns with a port (NIGHT7: THUNDERCLAP; NIGHT10: STITCHER, COMET, LONGWAVE); the rest of the wall stays bare until they exist
                double z = map.armoryWallZs[i], x = map.armoryWallX, f = map.armoryWallFacing;
                AddWallGun(id, i, new Vec3(x, map.armoryWallY, z), f, 0, new Vec3(x + f * buyOut, 0, z));
            }
        }

        /// <summary>One wall gun and its buyable (hold E at `buyAt`; the gun takes the next slot and goes in hand).</summary>
        void AddWallGun(string id, int wallIndex, Vec3 at, double fx, double fz, Vec3 buyAt)
        {
            string name = cfg.Str($"weapons.catalog.{id}.name");
            double price = cfg.Num($"weapons.prices.{id}");
            var gun = new WallGun { id = id, name = name, wallIndex = wallIndex, price = price, at = at, fx = fx, fz = fz };
            gun.item = new Buyables.Item
            {
                id = $"weapon:{id}", name = name, kind = "weapon", position = buyAt, reach = cfg.Num("buy.reach"),
                price = () => price, available = () => !loadout.owned.Contains(id), buy = () => loadout.AddWeapon(id),
            };
            buyables.list.Add(gun.item);
            wallGuns.Add(gun);
        }

        /// <summary>
        /// perks.js load (Perks.Ported at their map spots, data.perkSpots; once a run, only while the zone is open, the
        /// stack-taxed price) and forge.js load (its cabinet on map data.forgeSpot: FORGE the gun in hand, forge.cost, while
        /// its zone is open, a catalog gun not yet forged); perks.js apply on every change of the owned perks.
        /// </summary>
        void AddMachines()
        {
            perks.apply = ApplyPerks;
            foreach (PerkSpot spot in map.perkSpots)
            {
                if (!Perks.Ported.Contains(spot.perk)) continue; // STEADY, ANVIL, STATIC, PHOENIX: N12
                var m = new Machine { spot = spot };
                m.zone = ZoneMath.ZoneIndexAt(map.zoneParts, spot.pos.x, spot.pos.y + 1, spot.pos.z);
                string id = spot.perk;
                Machine mc = m;
                m.item = new Buyables.Item
                {
                    id = $"perk:{id}", kind = "perk", name = id.ToUpperInvariant(), position = spot.pos, reach = cfg.Num("perks.reach"),
                    price = () => perks.Price(id),
                    available = () => !perks.Owns(id) && (mc.zone < 0 || !zones.LockedTag(mc.zone)),
                    buy = () => perks.Give(id, perks.Price(id)),
                };
                buyables.list.Add(m.item);
                machines.Add(m);
            }
            if (map.forgeSpot.HasValue && forge.p.enabled)
            {
                var f = new Machine { spot = map.forgeSpot.Value, forge = true };
                f.zone = ZoneMath.ZoneIndexAt(map.zoneParts, f.spot.pos.x, f.spot.pos.y + 1, f.spot.pos.z);
                Machine fm = f;
                f.item = new Buyables.Item
                {
                    id = "forge", kind = "forge", name = "FORGE", position = f.spot.pos, reach = cfg.Num("perks.reach"),
                    price = () => forge.p.cost,
                    available = () => (fm.zone < 0 || !zones.LockedTag(fm.zone)) && forge.Known(loadout.Weapon) && !forge.IsForged(loadout.Weapon),
                    buy = () => forge.Give(loadout.Weapon, forge.p.cost),
                };
                buyables.list.Add(f.item);
                machines.Add(f);
            }
        }

        /// <summary>A machine on the Grinder core rides its pillar: its buy point at the pillar's top (PerkMachines.Render calls this each frame, as before).</summary>
        public void RideCore(Machine m)
        {
            if (!m.spot.core || core == null) return;
            double? top = core.TopAt(m.spot.pos.x, m.spot.pos.z);
            if (top.HasValue) m.item.position = new Vec3(m.spot.pos.x, top.Value, m.spot.pos.z);
        }

        /// <summary>perks.js apply: the effects of the owned perks on the player's systems.</summary>
        void ApplyPerks(PerkEffects fx)
        {
            Mover mv = mover;
            mv.bonusPips = fx.bonusPips;
            mv.bonusRideTime = fx.bonusRideTime;
            mv.bonusWallJumps = fx.bonusWallJumps;
            regen.perk = fx.regen ?? RegenMath.Perk.None;
            loadout.fireRateMult = fx.fireRate;
            loadout.swapMult = fx.swap;
            loadout.chargeMult = fx.charge; // NIGHT10: the LONGWAVE's charge (weaponMath.js chargeTime)
            loadout.arsenal.SetMods(fx.arsenalRecover, fx.arsenalPrimedBonus); // NIGHT9 U3-08 FLIP: perks.js loadout.arsenal.setMods(fx.arsenal)
            // NIGHT9 U3-11 (perks.js apply): SLIPSTREAM mv.slideMods, DEADEYE loadout.setPerkMods headshotMult +
            // economy headshot points, BOUNTY economy points, FLOW style.setPerkMods.
            mv.slideSpeedMult = fx.slideSpeed ?? 1;
            mv.slideFrictionMult = fx.slideFriction ?? 1;
            mv.slideJumpCapMult = fx.slideJumpCap ?? 1;
            loadout.headshotMult = fx.headshotMult ?? 1;
            economy.SetPerks(fx.points, fx.headshotPoints ?? 0);
            style.SetPerkMods(fx.styleDrain, fx.styleGain);
            double max = fx.maxHp ?? cfg.Num("health.max");
            Health h = health;
            if (h.max != max)
            {
                double grow = max - h.max;
                h.max = max;
                if (grow > 0) h.Regen(grow); // PLATE fills its new HP at once
                else h.hp = Math.Min(h.hp, h.max);
            }
        }

        /// <summary>
        /// NIGHT10 U3-13.8: the Mystery Box (new design, D-20; design.box). Off by flag: no box at all (the web's game). A
        /// Buyables entry like a wall gun (hold E, the same prompt): MYSTERY BOX for its price while it stands idle with a gun
        /// left to roll; TAKE &lt;gun&gt; while one is offered, which joins the loadout as a wall buy does (wallBuys.js: the next
        /// slot, in hand). A teddy only sends the box to a spot whose zone is open.
        /// </summary>
        void BuildBox()
        {
            if (!MysteryBox.Enabled(cfg)) return;
            box = new MysteryBox(cfg, events, economy, loadout.arsenal.Catalog, map.spots); // NIGHT15: a Unity-authored map brings its own spots
            box.spotOpen = i => zones.IsOpen(box.Spot(i).zone);
            double reach = cfg.Num("buy.reach");
            boxBuy = new Buyables.Item
            {
                id = MysteryBox.ItemId, name = "MYSTERY BOX", kind = "box", reach = reach,
                price = () => box.price, available = () => box.CanBuy && box.PoolCount(loadout.owned, fires) > 0, buy = () => box.Roll(loadout.owned, fires),
            };
            boxTake = new Buyables.Item
            {
                id = MysteryBox.ItemId + ":take", name = "TAKE", kind = "boxTake", reach = reach,
                price = () => 0, available = () => box.Offered != null,
                buy = () =>
                {
                    string w = box.Take();
                    if (w != null) loadout.AddWeapon(w);
                },
            };
            buyables.list.Add(boxBuy);
            buyables.list.Add(boxTake);
            PlaceBox();
        }

        /// <summary>
        /// NIGHT11, the run's shape (main.js: objectives before the PROTOCOL, so their roundClear handlers run in its order;
        /// traps; the moments tracker): the web's systems, then the port-only designs behind their flags (design.power,
        /// design.zombies.swarmDrop, design.exfil; each off = the web's game) and the LAB's ULTRA HEALTH (enabled per run).
        /// The Game sets traps.burst / projectiles.burst to its particles and builds the views.
        /// </summary>
        void BuildRunShape()
        {
            // NIGHT12: the PROTOCOL's finishes (unlocks.js earn: true the first time, so the banner can say NEW FINISH).
            protocol = new Protocol(cfg, events, buyables, zones, director, forge, loadout.owned, economy, () => run.state,
                () => contracts != null && contracts.Earn("protocol"), () => contracts != null && contracts.Earn("secretRelay"));
            traps = new Traps(cfg, events, buyables, zones.ZoneAt, Traps.IsOpenOf(zones, map), enemies, Traps.DamageBy(loadout), null, economy);
            moments = new RunMoments(cfg, events, () => mover.Speed, () => mover.position, () => mover.rope.phase == RopePhase.Attached, () => SimTime);
            mover.Events += moments.OnMover;
            gradeP = RunMoments.Grade.From(cfg);
            power = new Power(cfg, events, map.spots);
            if (power.enabled)
            {
                power.zoneOpen = zones.IsOpen;
                buyables.list.Add(power.Item);
                Func<bool> powered = () => power.Powered; // the traps and the relays wake with the power (the web's rules with the flag off)
                traps.powered = powered;
                protocol.powered = powered;
            }
            swarmDrop = new SwarmDrop(cfg, events, powerUps);
            exfil = new Exfil(cfg, events, () => mover.position, () => director.Remaining, director.AddSquads, map.spots);
            if (exfil.enabled)
            {
                exfil.zoneOpen = zones.IsOpen;
                buyables.list.Add(exfil.Radio);
            }
            ultraHealth = new UltraHealth(cfg, events, health, () => mover.position, () => style.rank);
        }

        /// <summary>The box's buy point: a step in front of the crate at its current spot; the take prompt names the gun.</summary>
        void PlaceBox()
        {
            BoxSpot s = box.Current;
            double front = UnityTuning.BoxDepth * 0.5 + 0.4;
            var at = new Vec3(s.pos.x + JsMath.Sin(s.yaw) * front, s.pos.y, s.pos.z + JsMath.Cos(s.yaw) * front);
            boxBuy.position = at;
            boxTake.position = at;
            string offered = box.Offered;
            if (!ReferenceEquals(offered, boxTakeFor))
            {
                boxTakeFor = offered;
                boxTake.name = offered != null ? "TAKE " + cfg.Str($"weapons.catalog.{offered}.name") : "TAKE"; // once per offer
            }
        }
    }
}
