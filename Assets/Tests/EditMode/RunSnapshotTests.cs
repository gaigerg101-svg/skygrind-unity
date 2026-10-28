// NIGHT13 U3-26.10 (agent E): the run snapshot (save-and-quit; Core/RunSnapshot.cs and the IRunState members of the
// run's systems). The proofs, on a composition of the Core systems built as SurvivalGame builds them (Init, StartRun,
// SimStep's order; the real map and config; a scripted player with a god-mode health pool and a cheat that kills half
// the enemies alive every 2 s, so rounds clear in seconds while the rest path, strafe, take tokens and attack):
// - Mulberry32.State continues a stream exactly;
// - a snapshot at round 5's break (rounds 1-4 cleared: husks and swarmers in the pools), restored into a fresh
//   composition (StartRun on the same seed, then Load), snapshots to the same bytes, section by section;
// - the restored run and the uninterrupted one, stepped on the same script through round 5 to round 6's break, snapshot
//   to the same bytes again (the streams, the Dictionary slot orders, the nav fields, the pools all continue exactly;
//   seen red with the brain's records rebuilt in iteration order instead of their slot layout: an enemy's path
//   diverged 1124 steps on);
// - a corrupted field, a missing section or another saveVersion fails with a StateException and leaves the systems as
//   they were (rolled back); a snapshot is refused while an enemy is alive;
// - DictLayout rebuilds a Dictionary's slot order (a removal's free slot is refilled as the original's would be);
// - MoverState lists every Mover field (Mover.cs is not touched: rule 3; its private state is read by reflection).
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class RunSnapshotTests
    {
        [Test]
        public void Mulberry32_State_RoundTrip_ContinuesTheSameSequence()
        {
            var a = new Mulberry32(0x5eed);
            for (int i = 0; i < 37; i++) a.Next();
            uint state = a.State;
            Assert.That(state, Is.EqualTo(unchecked((uint)(0x5eed + 37u * 0x6D2B79F5u))), "state = seed + draws x 0x6D2B79F5");
            var b = new Mulberry32(1) { State = state };
            for (int i = 0; i < 100; i++) Assert.That(b.Next(), Is.EqualTo(a.Next()), $"draw {i} after the restore");
            Assert.That(b.State, Is.EqualTo(a.State));
        }

        // --- the composition --------------------------------------------------------------------------------------------

        static GameConfig cfg;
        static SurvivalMap map;

        [OneTimeSetUp]
        public void LoadData()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
        }

        /// <summary>
        /// SurvivalGame's Core composition (Init's wiring, StartRun's resets, SimStep's order), without the views. The
        /// perk machines' effects (PerkMachines.Apply) and the box's buy items are copied; the wall buys and the perk
        /// machines' buy items are left out (both compositions are built by this same code).
        /// </summary>
        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly Players players = new Players();
            public readonly Profile profile = new Profile();
            public readonly GameWorld world;
            public readonly CoreGrid core;
            public readonly Zones zones;
            public readonly Mover mover;
            public readonly Health health;
            public readonly Regen regen;
            public readonly Economy economy;
            public readonly Loadout loadout;
            public readonly AimAssist assist;
            public readonly Projectiles projectiles;
            public readonly Husks husks;
            public readonly NavField nav;
            public readonly HuskBrain brain;
            public readonly Director director;
            public readonly Buyables buyables;
            public readonly StyleMeter style;
            public readonly Player p1;
            public readonly MysteryBox box;
            readonly Buyables.Item boxBuy, boxTake;
            public readonly Objectives objectives;
            public readonly Perks perks;
            public readonly Forge forge;
            public readonly PowerUps powerUps;
            public readonly Mods mods;
            public readonly Protocol protocol;
            public readonly Traps traps;
            public readonly RunMoments moments;
            public readonly Power power;
            public readonly SwarmDrop swarmDrop;
            public readonly Exfil exfil;
            public readonly UltraHealth ultraHealth;
            public readonly Ultra ultra;
            public readonly Lab lab;
            public readonly DeathRecap recap;
            public readonly Contracts contracts;
            public readonly Mastery mastery;
            public readonly Heat heat;
            public readonly Tips tips;
            public readonly Onboarding onboarding;
            readonly Mutators.Targets mutTargets;
            public SurvivalRun run;
            public double yaw, pitch;
            public int simStep, readySteps, scriptStep, maxAlive;
            readonly int readyHoldSteps;
            public uint runSeed;
            readonly double dt, campDrainMult, trickRange;

            public Rig()
            {
                MovementConfig mc = cfg.movement;
                dt = cfg.Dt;
                core = map.CreateCore(mc);
                world = new GameWorld(map.BuildOctree(), core);
                zones = new Zones(cfg, map, world, events);
                mover = new Mover(mc, world) { ropeEnabled = false };
                health = new Health(cfg.Num("health.max"), events, "player", () => mover.invulnerable);
                regen = new Regen(cfg, events, health, () => mover.Speed);
                economy = new Economy(cfg, events);
                loadout = new Loadout(cfg, events, enemies, world, mover, health, Aim);
                assist = new AimAssist(cfg, events, enemies, world, Aim, () => loadout.Weapon, () => loadout.adsActive, () => "snap", () => 1);
                loadout.assist = assist;
                projectiles = new Projectiles(cfg, events, world, enemies);
                loadout.projectiles = projectiles;
                ultra = Ultra.From(cfg);
                loadout.freshDamage = ultra.freshDamage;
                loadout.parryRefillsDash = ultra.parryRefillsDash;
                husks = new Husks(cfg, events, enemies, world, map.killPlaneY) { jumper = new HuskNavJumper(cfg, events) };
                nav = new NavField(cfg, NavGrid.FromExport(map.navGrid));
                nav.SetBlocked(zones.LockedTag);
                events.On("gateOpen", _ => nav.SetBlocked(zones.LockedTag));
                events.On("zoneClear", _ => nav.SetBlocked(zones.LockedTag));
                brain = new HuskBrain(cfg, events, nav, world.Geometry) { onScreen = _ => false };
                director = new Director(cfg, events, husks, brain, zones, core, world.Geometry, nav, () => mover.position, () => yaw, () => mover.Speed);
                director.huskWorld = world;
                brain.playerSpeed = () => mover.Speed;
                husks.acid.targetVelocity = () => mover.velocity;
                husks.acid.hunted = () => director.Hunted;
                husks.acid.onReflectHit = (e, damage, at) => loadout.Damage(e, damage, false, "orb", at);
                loadout.acid = husks.acid;
                double campMult = cfg.Num("points.campMult");
                economy.campMult = () => director.Exposed ? campMult : 1;
                regen.camping = () => director.Exposed;
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
                trickRange = cfg.Num("style.trickEnemyRange");
                style = new StyleMeter(cfg, events, () => PortData.Name(mover.state), EnemyNear);
                p1 = players.Add(new Player("player", mover, health) { regen = regen, loadout = loadout, assist = assist, style = style });
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
                if (MysteryBox.Enabled(cfg))
                {
                    box = new MysteryBox(cfg, events, economy, loadout.arsenal.Catalog);
                    box.spotOpen = i => zones.IsOpen(box.Spot(i).zone);
                    double reach = cfg.Num("buy.reach");
                    Func<string, bool> fires = Loadout.Fires;
                    boxBuy = new Buyables.Item
                    {
                        id = MysteryBox.ItemId, name = "MYSTERY BOX", kind = "box", reach = reach,
                        price = () => box.price, available = () => box.CanBuy && box.PoolCount(loadout.owned, fires) > 0, buy = () => box.Roll(loadout.owned, fires),
                    };
                    boxTake = new Buyables.Item
                    {
                        id = MysteryBox.ItemId + ":take", name = "TAKE", kind = "boxTake", reach = reach, price = () => 0, available = () => box.Offered != null,
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
                objectives = new Objectives(cfg, events, economy, zones, () => mover.position);
                events.On<RoundEvent>("roundPlan", e =>
                {
                    if (run != null && run.Live) objectives.Begin(e.number);
                });
                perks = new Perks(cfg, events);
                forge = new Forge(cfg, events);
                perks.apply = ApplyPerks;
                powerUps = new PowerUps(cfg, events)
                {
                    playerPos = () => mover.position, playerGrounded = () => mover.grounded, isLive = () => run.Live,
                    economy = economy, enemies = enemies, kill = (e, damage, at) => loadout.Damage(e, damage, false, "shockwave", at),
                    setStasis = on => husks.frozen = on, setOverclock = on => loadout.arsenal.SetOverclock(on),
                    floorUnder = p => world.RayIntersect(new Vec3(p.x, p.y + 1, p.z), new Vec3(0, -1, 0), out RayHit hit) ? p.y + 1 - hit.distance : 0,
                };
                mods = new Mods(cfg, events)
                {
                    isUnlocked = flag => contracts != null && contracts.Unlocked(flag),
                    airborne = () => !mover.grounded, moveState = () => PortData.Name(mover.state), eye = () => Aim().Item1, styleRank = () => style.rank,
                    health = health, economy = economy,
                };
                loadout.bonus = (e, w, head) => forge.Mult(w, head, e) * mods.Mult(e);
                loadout.after = forge.After;
                forge.status = loadout.arsenal.Status;
                powerUps.dropMult = mods.DropMult;
                campDrainMult = cfg.Num("director.camp.styleDrainMult");
                protocol = new Protocol(cfg, events, buyables, zones, director, forge, loadout.owned, economy, () => run.state,
                    () => contracts != null && contracts.Earn("protocol"), () => contracts != null && contracts.Earn("secretRelay"));
                traps = new Traps(cfg, events, buyables, zones.ZoneAt, Traps.IsOpenOf(zones, map), enemies, Traps.DamageBy(loadout), null, economy);
                moments = new RunMoments(cfg, events, () => mover.Speed, () => mover.position, () => mover.rope.phase == RopePhase.Attached, () => simStep * dt);
                mover.Events += moments.OnMover;
                power = new Power(cfg, events);
                if (power.enabled)
                {
                    power.zoneOpen = zones.IsOpen;
                    buyables.list.Add(power.Item);
                    Func<bool> powered = () => power.Powered;
                    traps.powered = powered;
                    protocol.powered = powered;
                }
                swarmDrop = new SwarmDrop(cfg, events, powerUps);
                exfil = new Exfil(cfg, events, () => mover.position, () => director.Remaining, director.AddSquads);
                if (exfil.enabled)
                {
                    exfil.zoneOpen = zones.IsOpen;
                    buyables.list.Add(exfil.Radio);
                }
                ultraHealth = new UltraHealth(cfg, events, health, () => mover.position, () => style.rank);
                recap = new DeathRecap(cfg.Num("ui.recap.window"));
                events.On<HurtEvent>("hurt", e =>
                {
                    if (e.owner == "player" && e.amount > 0) recap.Hurt(simStep * dt, e.amount, e.source ?? "unknown");
                });
                contracts = new Contracts(cfg, events, profile, mover, () => run?.stats.mutators);
                mastery = new Mastery(cfg, events, profile, Loadout.Fires) { fresh = style.getFresh, airborne = () => !mover.grounded, styleRank = () => style.rank };
                heat = new Heat(cfg, events, profile);
                tips = new Tips(cfg, profile, k => k.ToUpperInvariant());
                onboarding = new Onboarding(cfg, profile);
                mutTargets = new Mutators.Targets { loadout = loadout, health = health, brain = brain, economy = economy, regen = regen };
                events.On<RoundEvent>("roundBreak", e =>
                {
                    if (run == null || !run.Live) return;
                    tips.Moment("break");
                });
                events.On("powerUpSpawn", _ => tips.Moment("powerUp"));
                lab = new Lab(cfg);
                lab.Wire(events, health);
                readyHoldSteps = MovementMath.StepsFor(cfg.Num("survival.readyHold"), dt);
                run = new SurvivalRun(cfg, events, director, economy, regen)
                {
                    holdRound = () => objectives.Holding || exfil.Holding,
                    stepHook = d =>
                    {
                        objectives.Step();
                        exfil.Step(d);
                    },
                };
            }

            public (Vec3, Vec3) Aim()
            {
                Vec3 eye = new Vec3(mover.position.x, mover.position.y + cfg.movement.body.eyeHeight, mover.position.z);
                double cp = JsMath.Cos(pitch);
                return (eye, new Vec3(-JsMath.Sin(yaw) * cp, JsMath.Sin(pitch), -JsMath.Cos(yaw) * cp));
            }

            bool EnemyNear()
            {
                foreach (Enemy e in enemies.all)
                {
                    if (!e.alive) continue;
                    double dx = e.position.x - mover.position.x, dy = e.position.y - mover.position.y, dz = e.position.z - mover.position.z;
                    if (dx * dx + dy * dy + dz * dz <= trickRange * trickRange) return true;
                }
                return false;
            }

            /// <summary>PerkMachines.Apply (perks.js apply).</summary>
            void ApplyPerks(PerkEffects fx)
            {
                mover.bonusPips = fx.bonusPips;
                mover.bonusRideTime = fx.bonusRideTime;
                mover.bonusWallJumps = fx.bonusWallJumps;
                regen.perk = fx.regen ?? RegenMath.Perk.None;
                loadout.fireRateMult = fx.fireRate;
                loadout.swapMult = fx.swap;
                loadout.chargeMult = fx.charge;
                loadout.arsenal.SetMods(fx.arsenalRecover, fx.arsenalPrimedBonus);
                mover.slideSpeedMult = fx.slideSpeed ?? 1;
                mover.slideFrictionMult = fx.slideFriction ?? 1;
                mover.slideJumpCapMult = fx.slideJumpCap ?? 1;
                loadout.headshotMult = fx.headshotMult ?? 1;
                economy.SetPerks(fx.points, fx.headshotPoints ?? 0);
                style.SetPerkMods(fx.styleDrain, fx.styleGain);
                double max = fx.maxHp ?? cfg.Num("health.max");
                if (health.max != max)
                {
                    double grow = max - health.max;
                    health.max = max;
                    if (grow > 0) health.Regen(grow);
                    else health.hp = Math.Min(health.hp, health.max);
                }
            }

            void PlaceBox()
            {
                BoxSpot s = box.Current;
                double front = 0.6;
                var at = new Vec3(s.pos.x + JsMath.Sin(s.yaw) * front, s.pos.y, s.pos.z + JsMath.Cos(s.yaw) * front);
                boxBuy.position = at;
                boxTake.position = at;
            }

            /// <summary>SurvivalGame.StartRun (ResetWorld, the resets in its order, run.Start), god mode after.</summary>
            public void StartRun(uint seed)
            {
                if (run.Live) run.End();
                assist.Reset();
                director.Clear();
                husks.KillAll();
                brain.Clear();
                zones.Reset();
                core.SetHeights(CoreGrid.PresetHeights("flat", core.spec.cells, cfg.movement.core.maxHeight, cfg.movement.core.heightStep, new Mulberry32(1)));
                health.max = cfg.Num("health.max");
                health.Reset();
                regen.Reset();
                perks.Reset();
                powerUps.Clear();
                runSeed = seed;
                mover.Reset(map.playerSpawn.x, map.playerSpawn.y, map.playerSpawn.z);
                yaw = map.playerSpawnYawDeg * Math.PI / 180;
                pitch = 0;
                loadout.Reset(cfg.Strs("weapons.survivalStart"), runSeed);
                projectiles.Reset(runSeed ^ 0x9e3779b9u);
                brain.NewRun(runSeed);
                husks.NewRun(runSeed);
                powerUps.SeedRun(runSeed);
                mods.Start(runSeed);
                forge.Reset(runSeed);
                box?.Reset(runSeed);
                traps.Load(map.trapSpots);
                protocol.Load(map.relaySpots, map.secretRelay);
                objectives.Load(map.holdSpots);
                power.Reset();
                swarmDrop.Reset();
                exfil.Reset(runSeed);
                moments.Start();
                director.NewRun();
                style.Start();
                lab.Read(null);
                lab.Apply(cfg, regen, style, run, director, ultraHealth);
                readySteps = 0;
                recap.Clear();
                contracts.StartRun(false);
                mastery.StartRun(false);
                tips.recording = false;
                tips.NewRun();
                onboarding.StartRun(true);
                heat.Clear(mutTargets);
                run.Start(runSeed);
                RunStats st = run.stats;
                st.seed = runSeed;
                st.seedCode = Seeds.SeedCode(runSeed);
                heat.ApplyRung(mutTargets, 0, st.mutators);
                health.max = health.hp = 1e6; // the script's player lives the whole run
                scriptStep = 0;
            }

            /// <summary>SurvivalGame.SimStep's order, on the fingerprint's kind of script, plus the kill cheat.</summary>
            public void Step()
            {
                int i = scriptStep++;
                simStep++;
                var frame = new InputFrame
                {
                    moveX = (i / 240) % 2 == 0 ? 1 : -1,
                    moveZ = (i / 600) % 2 == 0 ? 0 : 1,
                    yaw = yaw,
                    pitch = pitch,
                    jump = new Button { pressed = i % 150 == 0, held = i % 150 < 10 },
                    dash = new Button { pressed = i % 420 == 210, held = i % 420 == 210 },
                    slide = new Button { pressed = i % 500 == 300, held = i % 500 >= 300 && i % 500 < 360 },
                };
                core.Step(mover);
                mover.Step(frame, dt);
                health.Tick();
                int pad = map.CheckPads(mover, cfg.movement, zones.padEnabled);
                if (pad >= 0) events.Emit("jumpPad", pad);
                p1.yaw = yaw;
                if (run.Live) brain.Step(dt, players, run.round, director.Hunted);
                var combat = new CombatInput
                {
                    fire = new Button { held = true, pressed = i % 10 == 0 },
                    special = new Button { pressed = i % 700 == 350, held = i % 700 == 350 },
                    punch = new Button { pressed = i % 333 == 100, held = i % 333 == 100 },
                    interact = new Button { held = i % 1200 >= 900, pressed = i % 1200 == 900 },
                    weapon = i > 0 && i % 1500 == 0 ? 0 : i % 1500 == 750 ? 1 : -1,
                };
                double lookYaw = yaw + 0.004, lookPitch = pitch;
                assist.Step(dt, ref combat, ref lookYaw, ref lookPitch);
                yaw = lookYaw;
                pitch = lookPitch;
                loadout.Step(combat, dt);
                // The cheat: every 2 s half the enemies alive (alternate registry slots, alternating sweeps) die to the
                // player's loadout (hit, kill, points, drops, style). The others live on: they path, strafe, take tokens
                // and attack, and the staggered deaths make new records reuse think offsets, so the brain's record order
                // (its Dictionary's slot layout) decides which of two thinks on one step draws first.
                if (i % KillEvery == 0)
                {
                    Vec3 eye = Aim().Item1;
                    int sweep = i / KillEvery;
                    for (int k = 0; k < enemies.all.Count; k++)
                    {
                        Enemy e = enemies.all[k];
                        if (e.alive && (k + sweep) % 2 == 0) loadout.Damage(e, e.hp + 1, k % 3 == 0, "revolver", eye);
                    }
                }
                maxAlive = Math.Max(maxAlive, husks.Count);
                regen.Step(dt);
                ultraHealth.Step(dt);
                if (box != null) PlaceBox();
                buyables.Step(mover.position, yaw, combat.interact.held, dt);
                box?.Step(dt);
                if (run.state == SurvivalRun.State.Break && combat.interact.held && buyables.candidate == null)
                {
                    if (++readySteps >= readyHoldSteps)
                    {
                        run.Ready();
                        readySteps = 0;
                    }
                }
                else readySteps = 0;
                projectiles.Step(dt);
                husks.Step(players, dt);
                brain.Separate(husks);
                style.drainMult = ultra.DrainPaused(run.state == SurvivalRun.State.Break, EnemyNear()) ? 0 : director.Exposed ? campDrainMult : 1;
                style.Step(dt);
                mover.regenMult = StyleMath.StaminaRewardMult(style.rank, style.Params);
                if (run.Live) powerUps.Step();
                traps.Step(dt);
                moments.Step();
                if (run.Live) contracts.Step(dt);
                director.fullPace = exfil.FullPace;
                run.Step(dt);
            }

            /// <summary>Things a run does that the script doesn't reach: a perk, a mod, a forge, an open gate, a bought gun, a drop, a boost removed (a free Dictionary slot).</summary>
            public void Spice()
            {
                perks.Give("vigor");
                perks.Give("quick");
                mods.Give("interest");
                forge.Give("revolver");
                zones.Open("armory", free: true);
                loadout.AddWeapon("shotgun");
                powerUps.Spawn(new Vec3(mover.position.x + 30, mover.position.y, mover.position.z), "double");
                economy.SetBoost("probe", 2);
                economy.SetBoost("hold", 1.5);
                economy.SetBoost("probe", 1);
                economy.Grant(1234, "probe");
                power.Throw();
            }

            /// <summary>Every run system in the order the game passes them (the lead's SurvivalSim list).</summary>
            public List<(string name, IRunState state)> Systems()
            {
                var list = new List<(string, IRunState)>
                {
                    ("run", run), ("economy", economy), ("health", health), ("regen", regen), ("mover", new MoverState(mover)), ("zones", zones),
                    ("nav", nav), ("enemies", enemies), ("husks", husks), ("brain", brain), ("director", director), ("buyables", buyables),
                    ("loadout", loadout), ("assist", new AimAssistState(assist)), ("projectiles", projectiles), ("style", style), ("perks", perks),
                    ("mods", mods), ("powerUps", powerUps), ("forge", forge),
                };
                if (box != null) list.Add(("box", box));
                list.AddRange(new List<(string, IRunState)>
                {
                    ("objectives", objectives), ("protocol", protocol), ("traps", traps), ("power", power), ("swarmDrop", swarmDrop), ("exfil", exfil),
                    ("moments", moments), ("ultraHealth", ultraHealth), ("lab", lab), ("recap", recap), ("contracts", contracts), ("mastery", mastery),
                    ("heat", heat), ("tips", tips), ("onboarding", onboarding),
                    ("game", new CustomState(w =>
                    {
                        w.D("yaw", yaw);
                        w.D("pitch", pitch);
                        w.I("simStep", simStep);
                        w.I("scriptStep", scriptStep);
                        w.I("readySteps", readySteps);
                        w.I("runSeed", runSeed);
                    }, r =>
                    {
                        yaw = r.D("yaw");
                        pitch = r.D("pitch");
                        simStep = r.Int("simStep");
                        scriptStep = r.Int("scriptStep");
                        readySteps = r.Int("readySteps");
                        runSeed = r.U("runSeed");
                    })),
                });
                return list;
            }

            public string Save() => RunSnapshot.Save(Systems());

            public void Load(string json) => RunSnapshot.Load(json, Systems());
        }

        const uint Seed = 7;
        const int KillEvery = 240; // steps between the kill cheat's sweeps (2 s)
        const int LateBreak = 5; // round 5's break: rounds 1-4 cleared (swarmers from 3; 6 is a HOLD round, which a script can't clear)

        /// <summary>A run on the script to round `round`'s break (the rounds before it cleared), spiced, `extra` steps into that break.</summary>
        static Rig RunToBreak(int round = 2, int extra = 60)
        {
            var rig = new Rig();
            rig.StartRun(Seed);
            bool spiced = false;
            for (int i = 0; i < 600 * 120; i++)
            {
                rig.Step();
                if (!spiced && rig.run.state == SurvivalRun.State.Fight && rig.husks.Count > 0)
                {
                    rig.Spice();
                    spiced = true;
                }
                if (rig.run.round == round && rig.run.state == SurvivalRun.State.Break) break;
            }
            Assert.That(rig.run.round, Is.EqualTo(round), $"the script reaches round {round}'s break");
            Assert.That(rig.run.state, Is.EqualTo(SurvivalRun.State.Break));
            for (int i = 0; i < extra; i++) rig.Step();
            Assert.That(rig.run.state, Is.EqualTo(SurvivalRun.State.Break), "still in the break");
            Assert.That(rig.run.stats.kills, Is.GreaterThan(0), "the rounds before had kills");
            return rig;
        }

        /// <summary>A fresh composition holding a snapshot: StartRun on the same seed (the run's structure), then Load.</summary>
        static Rig Restore(string json)
        {
            var rig = new Rig();
            rig.StartRun(Seed);
            rig.Load(json);
            return rig;
        }

        /// <summary>The first section that differs between two snapshots, with both texts around the difference ("" when equal).</summary>
        static string FirstDifference(string a, string b)
        {
            if (a == b) return "";
            int i = 0;
            while (i < a.Length && i < b.Length && a[i] == b[i]) i++;
            int from = Math.Max(0, i - 160);
            string Around(string s) => s.Substring(from, Math.Min(s.Length - from, 320));
            // the section: the last top-level key before the difference
            int depth = 0, keyAt = -1;
            for (int k = 0; k < i; k++)
            {
                char c = a[k];
                if (c == '{') depth++;
                else if (c == '}') depth--;
                else if (c == '"' && depth == 1) keyAt = k;
                if (c == '"')
                {
                    k++;
                    while (k < i && a[k] != '"') k += a[k] == '\\' ? 2 : 1;
                }
            }
            string section = keyAt >= 0 ? a.Substring(keyAt, Math.Min(40, a.Length - keyAt)) : "?";
            return $"first difference at {i} (section near {section}):\n  a: {Around(a)}\n  b: {Around(b)}";
        }

        [Test]
        public void Snapshot_AtABreak_RestoredIntoFreshSystems_SnapshotsTheSameBytes()
        {
            Rig a = RunToBreak(LateBreak);
            string json = a.Save();
            StringAssert.Contains("\"saveVersion\":1", json);
            Rig b = Restore(json);
            string again = b.Save();
            Assert.That(again == json, FirstDifference(json, again));
            // every section is in, and each one restored to the same bytes (the whole text is the sections in order)
            foreach (var (name, _) in a.Systems()) StringAssert.Contains($"\"{name}\":{{", json, $"section {name}");
        }

        [Test]
        public void Restored_Run_Continues_Exactly_ThroughTheNextRound()
        {
            Rig a = RunToBreak(LateBreak);
            string json = a.Save();
            Rig b = Restore(json);
            int killsBefore = a.run.stats.kills, poolBefore = a.enemies.all.Count;
            a.maxAlive = b.maxAlive = 0;
            // On through the next round to its break, both stepped on the same script; then 2 s more.
            for (int i = 0; i < 300 * 120 && !(a.run.round == LateBreak + 1 && a.run.state == SurvivalRun.State.Break); i++)
            {
                a.Step();
                b.Step();
            }
            Assert.That(a.run.round, Is.EqualTo(LateBreak + 1), $"the continuation plays round {LateBreak} through");
            for (int i = 0; i < 30; i++) // a little into the break (the script's held BUY may READY it: no further)
            {
                a.Step();
                b.Step();
            }
            Assert.That(a.run.state, Is.EqualTo(SurvivalRun.State.Break), "compared in the break");
            TestContext.WriteLine($"round {LateBreak}: kills {a.run.stats.kills - killsBefore}, the most alive at once {a.maxAlive}, the pools {poolBefore} -> {a.enemies.all.Count}");
            string sa = a.Save(), sb = b.Save();
            Assert.That(sb == sa, FirstDifference(sa, sb));
            Assert.That(a.run.stats.kills, Is.GreaterThan(killsBefore), "the round had kills");
            Assert.That(a.maxAlive, Is.GreaterThanOrEqualTo(6), "enemies lived side by side (the staggered sweeps)");
        }

        [Test]
        public void Corrupted_Field_Fails_And_RollsBack()
        {
            Rig a = RunToBreak(2, 10);
            string json = a.Save();
            var c = new Rig();
            c.StartRun(Seed);
            string before = c.Save();
            string corrupt = json.Replace("\"cleared\":1,", "\"cleared\":\"one\",");
            Assert.That(corrupt, Is.Not.EqualTo(json), "the snapshot has run.cleared");
            var ex = Assert.Throws<StateException>(() => c.Load(corrupt));
            StringAssert.Contains("cleared", ex.Message);
            Assert.That(c.Save() == before, "rolled back: " + FirstDifference(before, c.Save()));
            // a section missing
            string missing = json.Replace("\"tips\":{", "\"tipz\":{");
            Assert.Throws<StateException>(() => c.Load(missing));
            Assert.That(c.Save() == before, "rolled back after a missing section");
            // not JSON
            Assert.Throws<StateException>(() => c.Load(json.Substring(0, json.Length / 2)));
        }

        [Test]
        public void Other_SaveVersion_Fails()
        {
            Rig a = RunToBreak(2, 10);
            string json = a.Save();
            var c = new Rig();
            c.StartRun(Seed);
            string before = c.Save();
            var ex = Assert.Throws<StateException>(() => c.Load(json.Replace("\"saveVersion\":1", "\"saveVersion\":2")));
            StringAssert.Contains("saveVersion", ex.Message);
            Assert.That(c.Save() == before, "nothing loaded");
        }

        [Test]
        public void Snapshot_IsRefused_WhileAnEnemyIsAlive()
        {
            var rig = new Rig();
            rig.StartRun(Seed);
            for (int i = 0; i < 120 * 60 && rig.husks.Count == 0; i++) rig.Step();
            Assert.That(rig.husks.Count, Is.GreaterThan(0), "an enemy spawned");
            var ex = Assert.Throws<StateException>(() => rig.Save());
            StringAssert.Contains("alive", ex.Message);
        }

        [Test]
        public void DictLayout_Rebuild_KeepsTheSlotOrder()
        {
            var a = new Dictionary<int, string>();
            for (int i = 1; i <= 8; i++) a[i] = "v" + i;
            a.Remove(3);
            a.Remove(7);
            a.Remove(1);
            a[20] = "v20"; // refills the last freed slot (1's)
            a.Remove(5);
            var keys = new List<int>(a.Keys);
            var values = new List<string>(a.Values);
            Assert.That(DictLayout.Capture(a, out int slots, out List<int> chain), Is.True, "this runtime's Dictionary layout is known");
            var b = new Dictionary<int, string> { [99] = "old" };
            DictLayout.Rebuild(b, keys, values, slots, chain, k => int.MinValue + k);
            Assert.That(new List<int>(b.Keys), Is.EqualTo(keys), "the live entries in the same order");
            foreach (int k in new[] { 30, 31, 32, 33 })
            {
                a[k] = "n";
                b[k] = "n";
            }
            Assert.That(new List<int>(b.Keys), Is.EqualTo(new List<int>(a.Keys)), "new keys fill the freed slots in the same order");
            var s = new Dictionary<string, double> { ["heat"] = 1.5, ["double"] = 2, ["hold"] = 1.25 };
            s.Remove("heat");
            Assert.That(DictLayout.Capture(s, out int sSlots, out List<int> sChain), Is.True);
            var t = new Dictionary<string, double>();
            DictLayout.Rebuild(t, new List<string>(s.Keys), new List<double>(s.Values), sSlots, sChain, k => "\u0001" + k);
            s["gold"] = 3;
            t["gold"] = 3;
            Assert.That(new List<string>(t.Keys), Is.EqualTo(new List<string>(s.Keys)));
        }

        [Test]
        public void MoverState_ListsEveryMoverField()
        {
            var have = new List<string>();
            foreach (FieldInfo f in typeof(Mover).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) have.Add(f.Name);
            have.Sort(StringComparer.Ordinal);
            var listed = new List<string>(MoverState.Fields);
            listed.AddRange(MoverState.Skipped);
            listed.Sort(StringComparer.Ordinal);
            Assert.That(listed, Is.EqualTo(have), "MoverState.Fields + Skipped = every Mover instance field (a new Mover field needs a decision)");
        }
    }
}
