// NIGHT14 U3-27.5: THE AUTOPILOT. The web's bot brain driving the player's own Mover and loadout in SURVIVAL, as the web's
// attract loop drives it in the duel (main.js :702-721: a fresh neutral frame each step, frame.weapon = loadout.targetSlot,
// attractBrain.step(frame, 1 / simHz); the player's look is the brain's look). Built on top of the brain (IBotBrain),
// never editing it:
// - AutopilotTarget: getPlayer() is the chosen enemy of the horde (the nearest in sight, else by graph cost / distance);
// - AutopilotShopper: a break's shopping (design.autopilot.shopping), READY when nothing is affordable;
// - AutopilotGuns: BotBrain.pickWeapon (the port's loadout has more than the duel's two guns);
// - TakeDraft: a waiting MODS draft's first card (design.autopilot.takeFirstMod).
// The sim calls SampleMove once a step (the brain steps there), then SampleCombat (fire, the slot, interact).
// The bot's rules, set while it drives (Detach restores them):
// - NIGHT14 rule 7: sim.metaSilent (no profile, records, contracts, mastery, DAILY or run log) and sim.noLab (the web's
//   rules whatever the LAB tab says);
// - NIGHT14 rule 6: no aim assist (sim.assistModeFor answers "off"): the assist would turn the bot's aim for it.
//   Rejected: the human's default assist (a bot that cheats). Real HP: nothing here touches health.
// - NIGHT14 rule 8: the brain's rng is a Mulberry32 stream from the run seed XOR design.autopilot.rngSalt, re-seeded at
//   every run start, so a bot run is a pure function of (seed, difficulty, build).
// NIGHT20 P0-2 (builder P, design.autopilot.n20: the bot tells the truth; off: NIGHT19's bot): a HOLD ring and the exfil
// site's REACH are held by the game's own inside test (Ring: the last metres straight when the nav stops short; inside,
// free to move and fight within holdKeepShare of the radius, steered back past it); the break calls the exfil radio from
// design.autopilot.n20.exfilFrom (AutopilotShopper). NIGHT19's farm: 7 of 24 runs timed out 4.5 m from the round-6 ring.
// Zero allocation per step (a reused frame, target and plan; delegates made once). No UnityEngine use (Tools/SimHarness
// compiles this file).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class AutopilotInput : IPlayerInput
    {
        /// <summary>The brain the autopilot builds (injectable): agent N's port of botBrain.js (U3-27.4). StandInBrain is the
        /// placeholder the adapter was first built on (the harness's "standin").</summary>
        public static Func<AutopilotInput, BotSelf, BotDeps, IBotBrain> BrainFactory = PortedBrain;

        /// <summary>The nav graph the brain and the target adapter plan on (injectable): BotNav's Survival graph (navGraph.js
        /// on the map data, built once per process and content hash), seen through AutopilotNavView (no locked zones).</summary>
        public static Func<SurvivalSim, NavGraph> GraphSource = BotNav.Survival;

        public static IBotBrain StandInBrain(AutopilotInput ap, BotSelf self, BotDeps deps) => new AutopilotStandInBrain(ap.sim.cfg, self, deps, ap.Guns);

        public static IBotBrain PortedBrain(AutopilotInput ap, BotSelf self, BotDeps deps) => new BotBrain(ap.sim.cfg, self, deps);

        public static NavGraph NoGraph(SurvivalSim sim) => null;

        static readonly Func<string, string> AssistOff = _ => "off";

        readonly SurvivalSim sim;
        readonly string difficulty;
        readonly IBotBrain brain;
        readonly BotDeps deps;
        readonly Action<Vec3?> setOverride;
        readonly bool wasSilent, wasNoLab;
        readonly double holdRadius, holdHeight, holdFight;
        readonly Func<string, string> wasAssist;
        BotFrame frame;
        double yaw, pitch;
        bool fighting, interact, interactWas, overrideOn, attached = true;
        bool parryWas31; // NIGHT31 W: last step's parry punch (the press edge)
        Vec3 overrideGoal;

        public readonly AutopilotParams Params;
        public readonly AutopilotTarget Target;
        public readonly AutopilotShopper Shopper;
        public readonly AutopilotGuns Guns;
        public readonly AutopilotNavView NavView;

        /// <summary>The autopilot for `sim`'s player at `difficulty` (easy | normal | hard: config.bot's tables); its brain's
        /// rng is a Mulberry32 stream from `seed` (the run seed), so a bot run is a pure function of (seed, difficulty, build).</summary>
        public AutopilotInput(SurvivalSim sim, string difficulty, uint seed)
        {
            this.sim = sim;
            if (!sim.cfg.Has($"bot.reaction.{difficulty}")) throw new ArgumentException($"AUTOPILOT: unknown difficulty '{difficulty}' (config.bot.reaction has easy | normal | hard)");
            this.difficulty = difficulty;
            Params = AutopilotParams.From(sim.cfg);
            Guns = new AutopilotGuns(sim.loadout, Params);
            NavView = new AutopilotNavView(sim, GraphSource); // the graph without the locked zones (rebuilt when a gate opens)
            Target = new AutopilotTarget(sim, Params, NavView.Source);
            Shopper = new AutopilotShopper(sim, Params);
            holdRadius = sim.objectives.o.radius * Params.holdInsideShare;
            holdHeight = Params.holdHeightMeters;
            holdFight = Params.holdFightMeters;
            Loadout loadout = sim.loadout;
            var self = new BotSelf { mover = sim.mover, health = sim.health, weapon = () => loadout.Weapon };
            deps = new BotDeps { getPlayer = Target.Get, ray = sim.world.RayIntersect, getGraph = NavView.Get, rng = new Mulberry32(seed ^ Params.rngSalt) };
            brain = BrainFactory(this, self, deps);
            if (brain is BotBrain ported)
            {
                ported.pickWeapon = Guns.Pick;
                setOverride = ported.SetOverride;
            }
            else if (brain is AutopilotStandInBrain standIn) setOverride = standIn.SetOverride;
            brain.Reset(difficulty);
            yaw = sim.input.Yaw;
            pitch = sim.input.Pitch;
            wasSilent = sim.metaSilent;
            wasNoLab = sim.noLab;
            wasAssist = sim.assistModeFor;
            sim.metaSilent = true;
            sim.noLab = true;
            sim.assistModeFor = AssistOff;
            sim.RunStarted += OnRunStarted;
        }

        /// <summary>The brain (its state and look, for the views and the farm's report).</summary>
        public IBotBrain Brain => brain;

        /// <summary>The brain is the stand-in (every measurement made with it says so).</summary>
        public bool StandIn => brain is AutopilotStandInBrain;

        public string Difficulty => difficulty;

        /// <summary>NIGHT20 P0-2: the honest bot is on (TellTheTruth with design.autopilot.n20.enabled): the game's own ring
        /// test, the last metres straight, the exfil called.</summary>
        public bool Truth => truth;

        /// <summary>NIGHT20 P0-2: the first round whose offered exfil the bot calls (design.autopilot.n20.exfilFrom; 0: never).
        /// Public so a test (or a farm A / B) can move it.</summary>
        public int ExfilFrom;

        /// <summary>NIGHT24 (builder L, the campaign bot): where the bot walks when no enemy is in reach; null (the default) keeps
        /// SURVIVAL's behaviour exactly (the spawn, and the break shops).</summary>
        public Func<Vec3> GoalProvider;

        bool truth;
        double holdKeepShare; // NIGHT20: inside a ring, the brain moves freely within this share of the radius
        double ringReturn; // NIGHT20: m from a ring's centre within which the bot walks back while it shoots
        double approach; // NIGHT20: m from a buy point or a ring within which the bot walks straight, not by the graph
        int exfilWalkRemaining; // NIGHT20: the last enemies of the round before an exfil offer: walk to the radio

        /// <summary>
        /// NIGHT20 P0-2: turn the honest bot on, as design.autopilot.n20 says (enabled false or absent: NIGHT19's bot stays).
        /// Opt-in, called by THE FARM (Farm.RunOne, so THE MARATHON too): the judges that build their own autopilot (THE CHASE,
        /// THE CLIMB's STAND) keep NIGHT19's bot, so their frozen rows don't move (THE CHASE's FIGHT ran into an exfil break
        /// after round 10 and 15 and an exfil win ended its clock early).
        /// </summary>
        public void TellTheTruth()
        {
            GameConfig cfg = sim.cfg;
            truth = cfg.Design("autopilot.n20.enabled");
            if (!truth) return;
            holdKeepShare = cfg.Num("design.autopilot.n20.holdKeepShare");
            ringReturn = cfg.Num("design.autopilot.n20.ringReturnMeters");
            approach = cfg.Num("design.autopilot.n20.approachMeters");
            exfilWalkRemaining = cfg.Int("design.autopilot.n20.exfilWalkRemaining");
            exfilWaitMax = MovementMath.StepsFor(cfg.Num("design.autopilot.n20.exfilWaitSeconds"), sim.step);
            ExfilFrom = cfg.Int("design.autopilot.n20.exfilFrom");
        }

        /// <summary>NIGHT20 P0-2: in the fight of the round before a break that will offer the exfil radio (from ExfilFrom on, a
        /// site open, nothing called yet), with at most design.autopilot.n20.exfilWalkRemaining enemies left.</summary>
        bool ExfilComing()
        {
            Exfil x = sim.exfil;
            if (ExfilFrom <= 0 || x == null || x.Radio == null || x.phase != ExfilPhase.Off || x.Outcome != null) return false;
            int next = sim.run.round + 1;
            return sim.run.state == SurvivalRun.State.Fight && next >= ExfilFrom && x.IsExfilRound(next) && x.SiteFor(next) >= 0
                && sim.director.Remaining <= exfilWalkRemaining;
        }

        int exfilWaitMax, exfilWaitRound, exfilWaitSteps; // NIGHT20: design.autopilot.n20.exfilWaitSeconds in steps; this round's wait

        /// <summary>NIGHT20 P0-2: the radio wait has time left this round (design.autopilot.n20.exfilWaitSeconds; then the bot
        /// hunts the last enemies: NIGHT20's farm timed out once waiting on enemies that never came).</summary>
        bool ExfilWait()
        {
            if (exfilWaitRound != sim.run.round)
            {
                exfilWaitRound = sim.run.round;
                exfilWaitSteps = 0;
            }
            return ++exfilWaitSteps <= exfilWaitMax;
        }

        /// <summary>NIGHT20: the bot stands within `m` (horizontal) of `goal`.</summary>
        bool Near(Vec3 goal, double m)
        {
            Vec3 p = sim.mover.position;
            return JsMath.Hypot(goal.x - p.x, goal.z - p.z) <= m;
        }

        /// <summary>The target adapter has an enemy (the attract's flythrough orbits the autopilot and it).</summary>
        public bool HasTarget => fighting && Target.Has;

        /// <summary>The chosen enemy's feet (valid while HasTarget).</summary>
        public Vec3 TargetPosition => Target.Current != null ? Target.Current.position : sim.mover.position;

        /// <summary>design.autopilot.enabled.</summary>
        public static bool Enabled(GameConfig cfg) => cfg.Design("autopilot.enabled");

        static GameConfig takeCfg;
        static bool takeFirst;

        /// <summary>A waiting MODS draft (sim.mods.Pending in a break): the autopilot's pick (design.autopilot.takeFirstMod)
        /// taken on `sim`, headless (the farm) or live (the attract, before SurvivalGame.Update would open it and pause).</summary>
        public static void TakeDraft(SurvivalSim sim)
        {
            if (sim.flow != Flow.Playing || !(sim.run.state == SurvivalRun.State.Break || sim.mods.LayerPick) || !sim.mods.Pending) return; // NIGHT24: the campaign's layer-end BIG PICK opens outside a break
            if (!ReferenceEquals(takeCfg, sim.cfg))
            {
                takeFirst = sim.cfg.Design("autopilot.takeFirstMod"); // read once per config (a draft is rare; never per step)
                takeCfg = sim.cfg;
            }
            if (!takeFirst) return;
            if (sim.mods.Open() != null) sim.mods.Pick(0); // mods.js: the open draft's card 0 (SurvivalGame.PickMod without the pause)
        }

        /// <summary>Stop driving: the run rules go back to what they were (the human's run records again).</summary>
        public void Detach()
        {
            if (!attached) return;
            attached = false;
            sim.RunStarted -= OnRunStarted;
            sim.metaSilent = wasSilent;
            sim.noLab = wasNoLab;
            sim.assistModeFor = wasAssist;
        }

        /// <summary>A new run on the sim while this autopilot drives it: the brain, the target, the shopping, the rng stream.</summary>
        void OnRunStarted()
        {
            if (!ReferenceEquals(sim.input, this)) return;
            deps.rng.State = sim.runSeed ^ Params.rngSalt; // = new Mulberry32(runSeed ^ salt)
            brain.Reset(difficulty);
            Target.Reset();
            Shopper.Reset();
            Guns.Reset();
            parryWas31 = false; // NIGHT31 W: per run
            if (overrideOn)
            {
                setOverride?.Invoke(null);
                overrideOn = false;
            }
            fighting = interact = interactWas = false;
            exfilWaitRound = exfilWaitSteps = 0; // NIGHT20
        }

        public double Yaw => yaw;
        public double Pitch => pitch;
        public bool Captured => true;

        public void SetLook(double newYaw, double newPitch)
        {
            yaw = newYaw;
            pitch = newPitch;
            BotLook l = brain.Look;
            l.yaw = newYaw;
            l.pitch = newPitch;
        }

        void Override(Vec3 goal)
        {
            if (setOverride == null) return;
            if (overrideOn && overrideGoal.x == goal.x && overrideGoal.y == goal.y && overrideGoal.z == goal.z) return;
            overrideGoal = goal;
            overrideOn = true;
            setOverride(goal);
        }

        void EndOverride()
        {
            if (!overrideOn) return;
            overrideOn = false;
            setOverride(null);
        }

        public InputFrame SampleMove(bool active)
        {
            frame = default;
            frame.weapon = sim.loadout.targetSlot;
            interact = false;
            if (!active)
            {
                fighting = false;
                InputFrame idle = default;
                idle.yaw = yaw;
                idle.pitch = pitch;
                return idle;
            }
            double dt = sim.step;
            bool shopping = sim.run.state == SurvivalRun.State.Break && GoalProvider == null; // NIGHT24: the campaign's bot never shops
            if (!shopping && sim.objectives.Holding && sim.objectives.Spot.HasValue)
            {
                if (truth) Ring(sim.objectives.Spot.Value, sim.objectives.Radius, holdHeight, dt); // NIGHT20 P0-2
                else Hold(sim.objectives.Spot.Value, dt);
                return LookAndFrame();
            }
            if (truth && !shopping && sim.exfil != null && sim.exfil.phase == ExfilPhase.Reach)
            {
                Ring(sim.exfil.SitePos, sim.exfil.siteRadius, sim.exfil.siteHeight, dt); // NIGHT20 P0-2: the exfil's REACH
                return LookAndFrame();
            }
            if (truth && !shopping && ExfilComing() && ExfilWait()) // (ExfilFrom 0 without the truth: never)
            {
                // NIGHT20 P0-2: the round before an exfil offer, its last enemies left: wait at the radio (a 10 s break can't
                // cross the map: NIGHT20's farm missed the call from 88 m away), fighting what comes.
                Ring(sim.exfil.Radio.position, approach, holdHeight, dt); // within approachMeters (the radio sits on its box)
                return LookAndFrame();
            }
            Shopper.exfilFrom = ExfilFrom; // NIGHT20 P0-2: the break calls the radio (AutopilotShopper.Plan)
            fighting = !shopping && Target.Update();
            if (fighting)
            {
                EndOverride();
                if (Target.switched) brain.ClearPath(); // a new enemy: the brain replans toward it
                brain.Step(ref frame, dt);
            }
            else
            {
                // No enemy (a break, or a fight's gap): the break shops; a gap walks back to the spawn (the start zone is
                // always open; rejected: the map's centre, the GRINDER, locked at the start).
                if (shopping) Shopper.Plan();
                interact = shopping && Shopper.interact;
                if (shopping && Shopper.arrived)
                {
                    // In reach: stand still facing the item (Buyables' cone), or stand where it is to READY. The brain is not
                    // stepped: its follower would count the stillness as stuck and replan every step.
                    BotLook l = brain.Look;
                    l.yaw = Shopper.faceYaw;
                    l.pitch = 0;
                    frame.move.yaw = l.yaw;
                    frame.move.pitch = 0;
                }
                else
                {
                    bool buying = shopping && Shopper.mode == AutopilotShopper.Mode.Buy;
                    if (!shopping) Override(GoalProvider?.Invoke() ?? sim.map.playerSpawn); // NIGHT24: the campaign walks to its room's exit (null provider: the spawn, as before)
                    else if (Shopper.mode != AutopilotShopper.Mode.None) Override(Shopper.walkTo);
                    brain.Step(ref frame, dt);
                    if (buying && (frame.move.moveX == 0 && frame.move.moveZ == 0 || truth && Near(Shopper.walkTo, approach))) Approach(Shopper.walkTo); // NIGHT20: the last metres straight
                }
            }
            return LookAndFrame();
        }

        /// <summary>
        /// The last metres to a buy point the brain stands short of: the graph's nearest node to a gate's spot (on the force
        /// field) or a wall gun can be metres off, where navOnly "arrives" and stands. Walk straight at it, hopping at a wall
        /// (botBrain.js's own approach when the graph got it as close as it can).
        /// </summary>
        void Approach(Vec3 goal)
        {
            Mover m = sim.mover;
            double ay = JsMath.Atan2(-(goal.x - m.position.x), -(goal.z - m.position.z));
            BotLook l = brain.Look;
            l.yaw = ay;
            l.pitch = 0;
            frame.move.yaw = ay;
            frame.move.pitch = 0;
            frame.move.moveX = 0;
            frame.move.moveZ = 1;
            if (m.grounded && m.touchingWall) frame.move.jump.pressed = frame.move.jump.held = true;
        }

        InputFrame LookAndFrame()
        {
            BotLook look = brain.Look;
            yaw = look.yaw;
            pitch = look.pitch;
            return frame.move;
        }

        /// <summary>
        /// A HOLD round (objectives.js: the round can't clear until the ring is held for objectives.holdTime): the web's bot
        /// knows no objectives, so the adapter walks to the ring (the brain's nav override) and, once within
        /// design.autopilot.holdInsideShare of objectives.radius, holds it: the brain aims and fires at its target, its
        /// movement dropped (standing the ground; the jump kept).
        /// </summary>
        void Hold(Vec3 ring, double dt)
        {
            Vec3 p = sim.mover.position;
            double dx = ring.x - p.x, dz = ring.z - p.z;
            bool inRing = JsMath.Hypot(dx, dz) <= holdRadius && Math.Abs(p.y - ring.y) < holdHeight;
            bool target = Target.Update();
            bool near = target && Target.visible && Vec3.DistanceTo(Target.Current.position, p) <= holdFight;
            if (!inRing && !near)
            {
                fighting = false;
                Override(ring);
                brain.Step(ref frame, dt);
                return;
            }
            EndOverride();
            fighting = target;
            if (!fighting) return; // nothing to shoot: stand in the ring
            if (Target.switched) brain.ClearPath();
            brain.Step(ref frame, dt);
            if (!inRing) return; // on the way: an enemy this near is fought first
            frame.move.moveX = frame.move.moveZ = 0;
            frame.move.dash = frame.move.slide = default;
        }

        /// <summary>
        /// NIGHT20 P0-2 (design.autopilot.n20): a ring the bot must stand in (a HOLD's, or the exfil site's REACH): inside is
        /// the game's own test (the full `radius`, |dy| under `height`). Away from it (past ringReturnMeters, or with no enemy
        /// in sight within holdFightMeters): the nav override, the last metres straight when the nav stops short
        /// (Approach). Inside, or near it with an enemy that close: the brain aims and fires; it moves freely within
        /// holdKeepShare of the radius, and past that the bot walks back to the centre, keeping its aim. Never a jump, a dash or
        /// a slide there (NIGHT20's trace: one jump's air carried the bot out of a 4 m ring at 9 m/s).
        /// </summary>
        void Ring(Vec3 c, double radius, double height, double dt)
        {
            Vec3 p = sim.mover.position;
            double dx = c.x - p.x, dz = c.z - p.z, d = JsMath.Hypot(dx, dz);
            bool inside = d <= radius && Math.Abs(p.y - c.y) < height;
            bool target = Target.Update();
            bool near = target && Target.visible && Vec3.DistanceTo(Target.Current.position, p) <= holdFight;
            if (!inside && !near)
            {
                fighting = false;
                Override(c);
                brain.Step(ref frame, dt);
                if (frame.move.moveX == 0 && frame.move.moveZ == 0 || d <= approach) Approach(c); // the nav stood short of the ring
                return;
            }
            EndOverride();
            fighting = target;
            if (fighting)
            {
                if (Target.switched) brain.ClearPath();
                brain.Step(ref frame, dt);
            }
            if (!inside && d > ringReturn) return; // far from the ring with an enemy this near: fight it first (NIGHT19's rule)
            if (inside && d <= radius * holdKeepShare)
            {
                // Free inside the keep circle (standing when there's nothing to shoot), but no dash or slide, and a jump only
                // while moving toward the centre (its air then lands inside).
                frame.move.dash = frame.move.slide = default;
                Vec3 v = sim.mover.velocity;
                if (v.x * dx + v.z * dz <= 0) frame.move.jump = default;
                return;
            }
            frame.move.jump = frame.move.dash = frame.move.slide = default;
            if (!fighting)
            {
                Approach(c);
                return;
            }
            // Steer back to the centre in the look's frame (MovementMath.WishDirection inverted: its matrix is its own inverse).
            double yawNow = frame.move.yaw, sin = JsMath.Sin(yawNow), cos = JsMath.Cos(yawNow), wx = dx / d, wz = dz / d;
            frame.move.moveX = cos * wx - sin * wz;
            frame.move.moveZ = -sin * wx - cos * wz;
        }

        public CombatInput SampleCombat(bool active)
        {
            var c = new CombatInput { weapon = -1 };
            if (!active)
            {
                interactWas = false;
                return c;
            }
            if (fighting)
            {
                c.fire = frame.fire;
                c.weapon = frame.weapon;
                // NIGHT31 W (design.n31.ltAlways only; off: NIGHT30's autopilot, which never punched): the punch parry when a body in parry range winds
                // up, through AltUse.ParryWanted(Loadout, ...), which skips it with the gauntlet in hand (R8: its LT is the only parry then)
                Loadout lo = sim.loadout;
                bool parry = lo != null && lo.lt31 != null && lo.lt31.active && AltUse.ParryWanted(lo, sim.mover.position, lo.AltParams);
                c.punch.held = parry;
                c.punch.pressed = parry && !parryWas31;
                parryWas31 = parry;
            }
            else parryWas31 = false;
            c.interact.held = interact;
            c.interact.pressed = interact && !interactWas;
            interactWas = interact;
            return c;
        }
    }
}
