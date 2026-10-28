// NIGHT13 U3-26.6: the sim composition's run: the world reset, a run's start and end, TITLE, one fixed step in main.js
// simulate() order, stepMode, and the fingerprint's run. Moved from SurvivalGame unchanged, in the same statement order;
// the Game's parts (the cursor, the views' resets) are the callbacks it passes.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // One profiler mark per sim system (NIGHT8 U2-02): the Game maps each id to a ProfilerMarker (no UnityEngine here).
        public const int PStep = 0, PMover = 1, PBrain = 2, PAssist = 3, PLoadout = 4, PRegen = 5, PBuy = 6, PHusks = 7, PCrowd = 8, PStyle = 9, PAfter = 10, PMode = 11;
        public Action<int> profBegin, profEnd;

        void ResetWorld()
        {
            assist?.Reset(); // main.js: aimAssist.reset() for a new run
            spawner.Clear();
            husks.KillAll();
            brain?.Clear();
            zones.Reset();
            core.SetHeights(CoreGrid.PresetHeights("flat", core.spec.cells, cfg.movement.core.maxHeight, cfg.movement.core.heightStep, new Mulberry32(1)));
            health.max = cfg.Num("health.max"); // a new run's max (PLATE later; the smoke's god mode never carries over)
            health.Reset();
            regen.Reset();
            perks?.Reset(); // perks.js reset: every perk dropped, its effects undone (mover bonuses, regen, loadout, max HP)
            powerUps?.Clear(); // no drops, no effects
            hitstopLeft = 0;
        }

        /// <summary>The sim's half of SurvivalGame.ToTitle: the run left, the run shape unloaded, the world reset, the spawn.</summary>
        public void ToTitle()
        {
            if (run.Live) run.End();
            mods?.Stop();
            moments?.Stop();
            // NIGHT11 (main.js leaving Survival: traps.clear, protocol.load(null), objectives.load(null)).
            traps?.Clear();
            protocol?.Load(null, null);
            objectives?.Load(null);
            if (mutTargets != null) heat.Clear(mutTargets); // mutators.js clear (a mode switch)
            if (contracts != null && (contracts.Dirty || mastery.Dirty)) // progress from a run left early is kept
            {
                saveProfile?.Invoke();
                contracts.Dirty = mastery.Dirty = false;
            }
            flow = Flow.Title;
            downSteps = 0;
            ResetWorld();
            Respawn(map.playerSpawn, map.playerSpawnYawDeg);
        }

        /// <summary>
        /// startSurvival: a fresh world, the start loadout and bank, round 1 after the first break. `resetViews` runs where
        /// the Game's views were reset (decals, the shake, the light pools, the respawned camera and gun), `newRun` where
        /// SurvivalGame.OnNewRun was raised (before run.Start).
        /// </summary>
        public void StartRun(uint? seed, Action resetViews, Action newRun)
        {
            Hitch24.Mark("sr:00 begin");
           
            if (run.Live) run.End();
            Hitch24.Mark("sr:01 if");
            ResetWorld();
            Hitch24.Mark("sr:02 ResetWorld");
            CleanSlate(); // NIGHT14 U3-27.2: what a fresh composition starts with (SurvivalSim.Hooks.cs)
            Hitch24.Mark("sr:03 CleanSlate");
            N19StartRun(); // NIGHT19: the run's NIGHT19 blocks (a tape's, or off with --off19), handed to the Loadout and the Director before their resets
            Hitch24.Mark("sr:04 N19StartRun");
            N20StartRun(); // NIGHT20: the run's NIGHT20 blocks (a tape's, or off with --off20 / --off19), the same way (SurvivalSim.N20.cs)
            Hitch24.Mark("sr:05 N20StartRun");
            N21StartRun(); // NIGHT21: the run's NIGHT21 blocks (a tape's, or off with --off21 / --off20 / --off19), the same way (SurvivalSim.N21.cs)
            Hitch24.Mark("sr:06 N21StartRun");
            N26StartRun(); // NIGHT26: the run's NIGHT26 blocks (a tape's, or off with --off26 and older), the same way (SurvivalSim.N26.cs)
            N27StartRun(); // NIGHT27: the run's NIGHT27 blocks (a tape's, or off with --off27 and older), the same way (SurvivalSim.N27.cs)
            N29StartRun(); // NIGHT29: the run's NIGHT29 blocks (a tape's, or off with --off29 and older), the same way (SurvivalSim.N29.cs)
            N30StartRun(); // NIGHT30: the run's NIGHT30 blocks (a tape's, or off with --off30 and older), the same way (SurvivalSim.N30.cs)
            N31StartRun(); // NIGHT31: the run's NIGHT31 blocks (a tape's, or off with --off31 and older), the same way (SurvivalSim.N31.cs)
            N32StartRun(); // NIGHT32: the run's NIGHT32 blocks (a tape's, or off with --off32 and older), the same way (SurvivalSim.N32.cs)
            N33StartRun(); // NIGHT33: the run's NIGHT33 blocks (a tape's, or off with --off33 and older), the same way (SurvivalSim.N33.cs)
            N34StartRun(); // NIGHT34: the run's NIGHT34 blocks (a tape's, or off with --off34 and older), the same way (SurvivalSim.N34.cs)
            N35StartRun(); // NIGHT35: the run's NIGHT35 blocks (a tape's, or off with --off35 and older), the same way (SurvivalSim.N35.cs)
            N36StartRun(); // NIGHT36: the run's NIGHT36 blocks (a tape's, or off with --off36 and older), the same way (SurvivalSim.N36.cs)
            N37StartRun(); // NIGHT37: the run's NIGHT37 blocks (a tape's, or off with --off37 and older), the same way (SurvivalSim.N37.cs)
            N38StartRun(); // NIGHT38: the run's NIGHT38 blocks (a tape's, or off with --off38 and older), the same way (SurvivalSim.N38.cs)
            N39StartRun(); // NIGHT39: the run's NIGHT39 blocks (a tape's, or off with --off39 and older), the same way (SurvivalSim.N39.cs)
            N40StartRun(); // NIGHT40: the run's NIGHT40 blocks (a tape's, or off with --off40 and older), the same way (SurvivalSim.N40.cs)
            N41StartRun(); // NIGHT41: the run's NIGHT41 blocks (a tape's, or off with --off41 and older), the same way (SurvivalSim.N41.cs)
            N42StartRun(); // NIGHT42: the run's NIGHT42 blocks (a tape's, or off with --off42 and older), the same way (SurvivalSim.N42.cs)
            N43StartRun(); // NIGHT43: the run's NIGHT43 blocks (a tape's, or off with --off43 and older), the same way (SurvivalSim.N43.cs)
            RowsStartRun(); // NIGHT46 rows: the run's NIGHT46+ rows (a tape's, or off with -skygrindOffNN / --offNN from their night), then each night's StartRun hook
            N25StartRun(); // NIGHT25: the run's NIGHT25 blocks (a tape's, or off with --off25 and older), the same way (SurvivalSim.N25.cs)
            Hitch24.Mark("sr:07 N25StartRun");
            N24StartRun(); // NIGHT24: the run's NIGHT24 blocks and campaign state (a tape's, or off with --off24 and older), the same way (SurvivalSim.N24.cs)
            Hitch24.Mark("sr:08 N24StartRun");
            N22StartRun(); // NIGHT22: the run's NIGHT22 blocks (a tape's, or off with --off22 and older), the same way (SurvivalSim.N22.cs)
            Hitch24.Mark("sr:09 N22StartRun");
            // NIGHT12 (the run setup on TITLE): RETRY SAME SEED plays the seed it was given once (NIGHT24: the DAILY and the WEEKLY are
            // gone). An explicit seed (automation) wins.
            if (seed == null) seed = setup.seed;
            Hitch24.Mark("sr:10 if");
            setup.seed = null;
            Hitch24.Mark("sr:11 setup.seed");
            runSeed = seed ?? (uint)(DateTime.Now.Ticks & 0x7fffffff); // the run seed (the web's randomSeed); every roll after it is mulberry32
            Hitch24.Mark("sr:12 runSeed");
            Respawn(map.playerSpawn, map.playerSpawnYawDeg);
            Hitch24.Mark("sr:13 Respawn");
            loadout.Reset(cfg.Strs("weapons.survivalStart"), runSeed);
            Hitch24.Mark("sr:14 loadout.Reset");
            projectiles.Reset(runSeed ^ 0x9e3779b9u); // main.js projectiles.clear(); the port's stream from the run seed (the web uses Math.random)
            Hitch24.Mark("sr:15 projectiles.Reset");
            brain.NewRun(runSeed);
            Hitch24.Mark("sr:16 brain.NewRun");
            husks.NewRun(runSeed); // the spitters' seeded miss sides
            Hitch24.Mark("sr:17 husks.NewRun");
            powerUps.SeedRun(runSeed); // powerUps.js seedRun: the kill-drop stream
            Hitch24.Mark("sr:18 powerUps.SeedRun");
            mods.Start(runSeed); // no mods; the draft and roll streams from the run seed
            Hitch24.Mark("sr:19 mods.Start");
            forge.Reset(runSeed); // forge.js reset: nothing forged, the status stream from the run seed
            Hitch24.Mark("sr:20 forge.Reset");
            box?.Reset(runSeed); // NIGHT10: idle at the first spot (the starting zone), no pulls, the stream from the run seed
            Hitch24.Mark("sr:21 box.Reset");
            // NIGHT11 (main.js startSurvival: traps.load, protocol.load, objectives.load, moments.start) and the port-only
            // designs' resets (each a no-op with its flag off).
            traps.Load(map.trapSpots);
            Hitch24.Mark("sr:22 traps.Load");
            protocol.Load(map.relaySpots, map.secretRelay);
            Hitch24.Mark("sr:23 protocol.Load");
            objectives.Load(map.holdSpots);
            Hitch24.Mark("sr:24 objectives.Load");
            power.Reset();
            Hitch24.Mark("sr:25 power.Reset");
            swarmDrop.Reset();
            Hitch24.Mark("sr:26 swarmDrop.Reset");
            exfil.Reset(runSeed);
            Hitch24.Mark("sr:27 exfil.Reset");
            moments.Start();
            Hitch24.Mark("sr:28 moments.Start");
            resetViews?.Invoke(); // the Game's views (decals, the shake, the light pools; the camera and gun respawned)
            Hitch24.Mark("sr:29 resetViews.Invoke");
            director.NewRun();
            Hitch24.Mark("sr:30 director.NewRun");
            style.Start();
            Hitch24.Mark("sr:31 style.Start");
            lab.Read(noLab ? null : settingBool); // the run keeps the LAB toggles it started with
            Hitch24.Mark("sr:32 lab.Read");
            MoveBalanceStartRun(!noLab); // NIGHT16 U3-29.4: the movement rows (move.*) the run keeps; automation: design.move's defaults
            Hitch24.Mark("sr:33 MoveBalanceStartRun");
            StaminaStartRun(!noLab); // NIGHT17 U3-30.4: design.stamina's flags (a tape's when replaying); the bar full
            Hitch24.Mark("sr:34 StaminaStartRun");
            N19StaminaStartRun(); // NIGHT19: the sprint's own key (design.stamina.n19.ownKey) with the stamina block on
            Hitch24.Mark("sr:35 N19StaminaStartRun");
            lab.Apply(cfg, regen, style, run, director, ultraHealth); // NIGHT11: ULTRA HEALTH on with its toggle only
            Hitch24.Mark("sr:36 lab.Apply");
            readySteps = 0;
            Hitch24.Mark("sr:37 readySteps");
            recap.Clear(); // main.js OVERNIGHT3 P4
            Hitch24.Mark("sr:38 recap.Clear");
            hitArcs.Clear();
            Hitch24.Mark("sr:39 hitArcs.Clear");
            lastDeath = null;
            Hitch24.Mark("sr:40 lastDeath");
            downTarget = null;
            Hitch24.Mark("sr:41 downTarget");
            // NIGHT12: the meta for this run (nothing is recorded by a LAB run or automation).
            runRecording = Recording;
            Hitch24.Mark("sr:42 runRecording");
            contracts.StartRun(runRecording);
            Hitch24.Mark("sr:43 contracts.StartRun");
            mastery.StartRun(runRecording);
            Hitch24.Mark("sr:44 mastery.StartRun");
            tips.recording = runRecording;
            Hitch24.Mark("sr:45 tips.recording");
            tips.NewRun();
            Hitch24.Mark("sr:46 tips.NewRun");
            onboarding.StartRun(SettingOr("onboarding", true));
            Hitch24.Mark("sr:47 onboarding.StartRun");
            runOnboarding = onboarding.Active;
            Hitch24.Mark("sr:48 runOnboarding");
            int rung = heat.enabled ? Math.Max(0, Math.Min(setup.heat, heat.Unlocked)) : 0;
            Hitch24.Mark("sr:49 intrung");
            heat.Clear(mutTargets); // the last run's mutators off (damage both ways, enemy pace, the points boosts)
            Hitch24.Mark("sr:50 heat.Clear");
            newRun?.Invoke();
            Hitch24.Mark("sr:51 newRun.Invoke");
            run.Start(runSeed);
            Hitch24.Mark("sr:52 run.Start");
            // After run.Start (it re-enables regen and restarts the economy): the run's stats and its HEAT rung.
            RunStats st = run.stats;
            Hitch24.Mark("sr:53 RunStatsst");
            st.seed = runSeed;
            Hitch24.Mark("sr:54 st.seed");
            st.seedCode = Seeds.SeedCode(runSeed);
            Hitch24.Mark("sr:55 st.seedCode");
            st.heat = rung;
            Hitch24.Mark("sr:56 st.heat");
            st.scoreMult = heat.ScoreMult(rung);
            Hitch24.Mark("sr:57 st.scoreMult");
            st.mutators.AddRange(heat.MutatorsOf(rung));
            Hitch24.Mark("sr:58 st.mutators.AddRange");
            Challenge21.ReadSetupMutators(cfg, setup, st.mutators); // NIGHT22 U3-36.8 A-3: a typed code's (or the TITLE's) mutators play; none picked: unchanged
            Hitch24.Mark("sr:59 Challenge21.ReadSetupMutator");
            // mutators.apply (main.js startSurvival): the rung's rules and the twist's, and the rung's score multiplier (an
            // Economy boost "heat"). Rung 0 with no twist sets every number back to x1: the web's game.
            heat.ApplyRung(mutTargets, rung, st.mutators);
            Hitch24.Mark("sr:60 heat.ApplyRung");
            N33ShopAfterStart(); // NIGHT33 P: the shop's upgrades and the banked points, once the economy and the health are set (SurvivalSim.Shop33.cs)
            flow = Flow.Playing;
            Hitch24.Mark("sr:61 flow");
            downSteps = 0;
            Hitch24.Mark("sr:62 downSteps");
            RaiseRunStarted(); // NIGHT14: the recorder, the tape, the route track (SurvivalSim.Hooks.cs)
            Hitch24.Mark("sr:63 RaiseRunStarted");
        }

        /// <summary>The run ends: its stats, grade, records and the meta's judgement; RESULTS (the Game frees the cursor: runEnded).</summary>
        public void EndRun()
        {
            lastStats = run.End();
            director.Stop();
            style.Stop();
            mods.Stop();
            if (lastStats != null)
            {
                lastStats.style = style.total;
                lastStats.bestRank = style.Params.ranks[style.bestRank];
                if (lastDeath?.killer != null) lastStats.killedBy = lastDeath.killer; // the recap's name (main.js stats.killedBy)
                lastStats.forged.Clear(); // main.js stats.forged: the forged guns' names with their "+"
                foreach (string id in forge.order) lastStats.forged.Add((cfg.Has($"weapons.catalog.{id}.name") ? cfg.Str($"weapons.catalog.{id}.name") : id.ToUpperInvariant()) + "+");
                // NIGHT11 (main.js endRun): the moments, the grade (round, best rank, accuracy), the PROTOCOL's summary, the
                // score line (no mutators or seed code in the port yet), and the port-only exfil outcome.
                moments.Stop();
                moments.Fill(lastStats);
                lastStats.bestRankIndex = style.bestRank;
                lastStats.grade = RunMoments.GradeOf(lastStats.round, style.bestRank, lastStats.Accuracy, gradeP);
                ProtocolSummary ps = protocol.Summary;
                lastStats.hasProtocol = ps.has;
                lastStats.protocolLit = ps.lit;
                lastStats.protocolTotal = ps.total;
                lastStats.protocolComplete = ps.complete;
                lastStats.exfil = exfil.Outcome;
                lastStats.grade = Payoff.GradeWithP(lastStats, payoffP); // NIGHT12 design.grade.p: an exfil at S with an SS peak and no down is "P"
                var build = new List<string>();
                int modCount = 0;
                foreach (var kv in mods.stacks) if (kv.Value > 0) modCount++; // main.js stats.mods = mods.owned(): the mods held
                if (modCount > 0) build.Add($"{modCount} mods");
                if (lastStats.forged.Count > 0) build.Add($"{lastStats.forged.Count} forged");
                if (ps.complete) build.Add("PROTOCOL");
                // main.js endRun's scoreLine: the active mutators' names and the run's seed code (NIGHT12: both now exist).
                List<string> mutatorNames = null;
                if (lastStats.mutators.Count > 0)
                {
                    mutatorNames = new List<string>();
                    foreach (string id in lastStats.mutators) mutatorNames.Add(cfg.Str($"mutators.kinds.{id}.name"));
                }
                lastStats.line = RunMoments.ScoreLine(lastStats.round, lastStats.earned, lastStats.grade, lastStats.bestRank, lastStats.time, mutatorNames, lastStats.seedCode, string.Join(", ", build));
                // records.js: the run against the best (round, then points earned); the results' personal-best line. A run
                // that can't set a best (the smoke's, a LAB run) is judged but never stored.
                var thisRun = new Records.Run { round = lastStats.round, points = Math.Round(lastStats.earned), date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), exfil = lastStats.exfil == Exfil.Win };
                if (lab.Any) lastPb = new Records.Judgement { text = $"<b>LAB</b> · {lab.Names()} · NOT A PERSONAL BEST" }; // NIGHT8 U2-16
                else if (noRecords || metaSilent) lastPb = Records.Judge(thisRun, records.Best(RecordsKey)); // NIGHT14 rule 7: a bot's run too (NIGHT15: keyed by the map)
                else
                {
                    lastPb = records.Submit(RecordsKey, thisRun); // NIGHT15: keyed by mode and map
                    lastStats.recorded = true;
                    profile.runs++;
                }
                // NIGHT12: the meta judges the run (each a no-op when the run doesn't record): the end-of-run contracts, a HEAT
                // rung opened by an exfil, the best grade per rung, the first run done.
                bool rec = runRecording && lastStats.recorded;
                contracts.EndRun(lastStats);
                lastStats.contractsDone.AddRange(contracts.doneThisRun);
                heat.RunEnd(lastStats.heat, lastStats.exfil == Exfil.Win, rec);
                heat.mutators.RunEnd(lastStats.round, rec); // mutators.js runEnd: the best round, mutatorUnlock
                if (rec) profile.SubmitGrade(lastStats.heat, lastStats.grade);
                if (runOnboarding) onboarding.Finish(rec);
                if (rec || contracts.Dirty || mastery.Dirty) saveProfile?.Invoke(); // NIGHT12: the JSON profile (records.js's store moved there)
                contracts.Dirty = mastery.Dirty = false;
            }
            flow = Flow.Results;
            runEnded?.Invoke();
        }

        /// <summary>main.js survivalTarget (NIGHT6 B5): DOWN's next target, a new best or the round that beats yours.</summary>
        string SurvivalTarget()
        {
            Records.Run best = records.Best(RecordsKey); // NIGHT15: keyed by mode and map
            if (best == null || best.round <= 0) return null;
            return run.round > best.round ? $"ROUND {run.round}: A NEW BEST" : $"ROUND {best.round + 1} BEATS YOUR BEST";
        }

        /// <summary>One fixed step, in main.js simulate() order (the systems ported so far).</summary>
        public void Step()
        {
            profBegin?.Invoke(PStep);
            simStep++;
            N34BendBeginStep(); // NIGHT34 B: sets n34HoldWorld for the split bend (SurvivalSim.N34.cs: the world calls below skip on a held step)
            InputFrame frame = input.SampleMove(Active);
            N24LimitFrame(ref frame); // NIGHT24 (weapons-limits): a LIMIT room strips a verb for its room only
            if (n20.zoom && loadout.ZoomMag > 1) // NIGHT20 U3-34.3 (design.zoom): a zoom ends the sprint (the sprint key and the boost key's hold; a fresh press still boosts). No Mover change.
            {
                frame.sprint = default;
                frame.dash.held = frame.dash.pressed;
            }
            N31GrappleFrame(ref frame); // NIGHT31 G: the grapple's tap / hold latch, before the Mover reads the frame (SurvivalSim.Grapple31.cs)
            N32GrappleFrame(ref frame); // NIGHT32 G: the grapple's charges, after the NIGHT31 latch (SurvivalSim.Grapple32.cs)
            N33WhipFrame(ref frame); // NIGHT33 W: the whip, after the NIGHT32 charges (SurvivalSim.Whip33.cs)
            previous = current;
            profBegin?.Invoke(PMover);
            if (!n34HoldWorld) core?.Step(mover); // arena.stepCore(bodies) before player.step
            mover.Step(frame, step);
            profEnd?.Invoke(PMover);
            health.Tick();
            int pad = map.CheckPads(mover, cfg.movement, zones.padEnabled);
            if (pad >= 0) events.Emit("jumpPad", pad);
            if (!n34HoldWorld) N33DevicesMove(step); // NIGHT33 R: the devices that move the player (fan, vent, platform), right after the Mover and the pads (SurvivalSim.Devices33.cs)
            profBegin?.Invoke(PBrain);
            p1.yaw = input.Yaw;
            if (!n34HoldWorld && run.Live) brain.Step(step, players, run.round, director.Hunted); // nav fields + AI (main.js: before the loadout; +1 tokens while hunted)
            if (!n34HoldWorld) N20RoutesStep(step); // NIGHT20: builder N's hook (SurvivalSim.Routes20.cs; nothing without it)
            profEnd?.Invoke(PBrain);
            lastCombat = input.SampleCombat(Active); // the reader gates the keyboard on the captured cursor itself; a playing pad counts without it
            if (Active)
            {
                profBegin?.Invoke(PAssist);
                // main.js: the lock turns the look before the shots that read it (it may hold this step's press, at most 40 ms).
                double lookYaw = input.Yaw, lookPitch = input.Pitch;
                assist.Step(step, ref lastCombat, ref lookYaw, ref lookPitch);
                input.SetLook(lookYaw, lookPitch);
                profEnd?.Invoke(PAssist);
            }
            profBegin?.Invoke(PLoadout);
            if (Active) loadout.Step(lastCombat, step); // ADS, the punch, then the weapon in hand (main.js: ads -> aim -> loadout -> punch)
            N19ArsenalStep(step); // NIGHT19: builder W's hook (SurvivalSim.Arsenal19.cs; nothing without it)
            N20ArsenalStep(step); // NIGHT20: builder G's hook (SurvivalSim.Arsenal20.cs; nothing without it)
            N21ArclineStep(step); // NIGHT21: builder M's hooks (SurvivalSim.Arcline21.cs, SlideBite21; nothing without them)
            N21SlideBiteStep(step);
            profEnd?.Invoke(PLoadout);
            profBegin?.Invoke(PRegen);
            regen.Step(step);
            N20RegenStep(step); // NIGHT20: builder E's hook (SurvivalSim.Regen20.cs; nothing without it)
            ultraHealth.Step(step); // NIGHT11 U3-24.9: hard damage's timer and drain (nothing while the LAB toggle is off)
            profEnd?.Invoke(PRegen);
            profBegin?.Invoke(PBuy);
            if (box != null) PlaceBox();
            if (Active) buyables.Step(mover.position, input.Yaw, lastCombat.interact.held, step); // hold E: gates (main.js: after regen)
            box?.Step(step); // NIGHT10: the roll's reveal, offer, teddy and move timers
            // NIGHT6 B6 READY (main.js): in a break, hold BUY with nothing for sale in reach to start the round now (from
            // survival.readyFrom; the LAB's READY FROM ROUND 1 lowers it).
            if (Active && run.state == SurvivalRun.State.Break && lastCombat.interact.held && buyables.candidate == null)
            {
                if (++readySteps >= readyHoldSteps)
                {
                    run.Ready();
                    readySteps = 0;
                }
            }
            else readySteps = 0;
            profEnd?.Invoke(PBuy);
            if (!n34HoldWorld) projectiles.Step(step); // main.js simulate: projectiles after the loadout, the buyables and READY, before the enemies
            profBegin?.Invoke(PHusks);
            if (!n34HoldWorld) husks.Step(players, step); // enemies after the loadout (main.js simulate order), each at its nearest player
            if (!n34HoldWorld) N19EnemiesStep(step); // NIGHT19: builder E's hook (SurvivalSim.Enemies19.cs; nothing without it)
            if (!n34HoldWorld) N20DirectorStep(step); // NIGHT20: builder E's hook (SurvivalSim.Director20.cs; nothing without it)
            if (!n34HoldWorld) N21DeadlineStep(step); // NIGHT21: builder S's hooks (the DEADLINE fuse, the round stamp), then W's (the GRIND); nothing without them
            if (!n34HoldWorld) N21StampStep(step);
            if (!n34HoldWorld) N21GrindStep(step);
            if (!n34HoldWorld) N22DirectedStep(step); // NIGHT22: the lead's hooks for the NIGHT22 rows (SurvivalSim.N22.cs; nothing without a body)
            N22TrickStep(step);
            if (!n34HoldWorld) N22DuskStep(step);
            N22HapticsStep(step);
            if (!n34HoldWorld) N24CampaignStep(step); // NIGHT24: the lead's hooks for the NIGHT24 rows (SurvivalSim.N24.cs; nothing without a body)
            N24FistStep(step);
            if (!n34HoldWorld) N24WyrmStep(step);
            N26WeaponsStep(step); // NIGHT26: the lead's hooks for the NIGHT26 rows (SurvivalSim.N26.cs; nothing without a body)
            N26SurfaceStep(step);
            if (!n34HoldWorld) N26BossStep(step);
            if (!n34HoldWorld) N26LevelStep(step);
            N27WeaponsStep(step); // NIGHT27: the lead's hooks for the NIGHT27 rows (SurvivalSim.N27.cs; nothing without a body)
            if (!n34HoldWorld) N27BossStep(step);
            if (!n34HoldWorld) N27LevelStep(step);
            if (!n34HoldWorld) N29SystemsStep(step); // NIGHT29: the lead's hooks for the NIGHT29 rows (SurvivalSim.N29.cs; nothing without a body)
            if (!n34HoldWorld) N29BossStep(step);
            N30SystemsStep(step); // NIGHT30: the lead's hooks for the NIGHT30 rows (SurvivalSim.N30.cs; nothing without a body)
            if (!n34HoldWorld) N30BossStep(step);
            N31GrappleStep(step); // NIGHT31: the lead's hooks for the NIGHT31 rows (SurvivalSim.N31.cs; nothing without a body)
            N31WeaponsStep(step);
            if (!n34HoldWorld) N31BossStep(step);
            N32GrappleStep(step); // NIGHT32: the lead's hooks for the NIGHT32 rows (SurvivalSim.N32.cs; nothing without a body)
            if (!n34HoldWorld) N32BalanceStep(step);
            if (!n34HoldWorld) N32BossStep(step);
            if (!n34HoldWorld) N32ArenaStep(step);
            if (!n34HoldWorld) N32SpanStep(step);
            if (!n34HoldWorld) N32FinaleStep(step);
            if (!n34HoldWorld) N32CrownStep(step);
            if (!n34HoldWorld) N33AiStep(step); // NIGHT33: the lead's hooks for the NIGHT33 rows (SurvivalSim.N33.cs; nothing without a body)
            if (!n34HoldWorld) N33SpawnsStep(step);
            if (!n34HoldWorld) N33BossStep(step);
            N33WhipStep(step);
            N33PierceStep(step);
            if (!n34HoldWorld) N33DevicesStep(step);
            N34BendStep(step); // NIGHT34 B: every step (the player's clock), after this step's latches (SurvivalSim.Bend34.cs)
            N35WeaponsStep(step); // NIGHT35 W: charge35 (SurvivalSim.Weapons35.cs); the player's side
            N35MoveStep(step); // NIGHT35 M: wallride35 / grapple35 (SurvivalSim.Move35.cs)
            N35TeachStep(step); // NIGHT35 T: teach35 (SurvivalSim.Teach35.cs)
            N36GunsStep(step); // NIGHT36 F: tap36, crunch36 (SurvivalSim.Guns36.cs)
            N36CoreStep(step); // NIGHT36 P: tells36, parry36, launch36, heal36 (SurvivalSim.Core36.cs)
            N36MoveStep(step); // NIGHT36 M: wallgrip36 (SurvivalSim.Move36.cs)
            N36LevelStep(step); // NIGHT36 L: the arena flow on n36 levels (SurvivalSim.Level36.cs)
            N36BossStep(step); // NIGHT36 B: walkin36, boss36 (SurvivalSim.Boss36.cs)
            N37Step(step); // NIGHT37 W / S / O / C: THE WICK, THE STAG, THE LOOM, THE CROWN (SurvivalSim.N37.cs)
            N38Step(step); // NIGHT38 M / S2: ride38, stag38 (SurvivalSim.N38.cs; nothing until a builder fills its hook)
            N39Step(step); // NIGHT39 P1 / B1: p39, boss39 (SurvivalSim.N39.cs; nothing until a builder fills its hook)
            N40Step(step); // NIGHT40 S1 / B2 / B4 / B5 / E3 (SurvivalSim.N40.cs; nothing until a builder fills its hook)
            N41Step(step); // NIGHT41 B8 / E6 / P3 (SurvivalSim.N41.cs; nothing until a builder fills its hook)
            N42Step(step); // NIGHT42 M1 / W1 / F1 / B1 / K1 (SurvivalSim.N42.cs; nothing until a builder fills its hook)
            N43Step(step); // NIGHT43 M1 / W1 / E1 / A1 / B1 / K1 (SurvivalSim.N43.cs; nothing until a builder fills its hook)
            RowsStep(step); // NIGHT46 rows: each NIGHT46+ night's step hook in order (SurvivalSim.NNN.cs; nothing until a night fills its hook)
            N25CombatStep(step); // NIGHT25: the lead's hooks for the NIGHT25 rows (SurvivalSim.N25.cs; nothing without a body)
            if (!n34HoldWorld) N25WyrmStep(step);
            if (!n34HoldWorld) N25LevelStep(step);
            profEnd?.Invoke(PHusks);
            profBegin?.Invoke(PCrowd);
            if (!n34HoldWorld) brain.Separate(husks); // ai/crowd.js: once per step, after every enemy moved
            if (!n34HoldWorld) N33AfterCrowd(step); // NIGHT33 E: the lava edge guard, after the crowd's pushes (SurvivalSim.Ai33.cs)
            profEnd?.Invoke(PCrowd);
            profBegin?.Invoke(PStyle);
            // NIGHT10 U3-13.9 design.ultra.styleBreakPause: no drain in a break or with no live enemy within style.trickEnemyRange.
            style.drainMult = ultra.DrainPaused(run.state == SurvivalRun.State.Break, EnemyNear()) ? 0 : director.Exposed ? campDrainMult : 1; // EXPOSED: style drains x2
            style.Step(step);
            mover.regenMult = StyleMath.StaminaRewardMult(style.rank, style.Params); // the rank reward: faster stamina
            profEnd?.Invoke(PStyle);
            profBegin?.Invoke(PAfter);
            if (run.Live) powerUps.Step(); // drops age / are grabbed, timed effects run out
            if (!n34HoldWorld) traps.Step(step); // NIGHT11 U3-24.5 (main.js simulate: powerUps, traps, affixes (not ported), moments)
            moments.Step();
            if (run.Live) contracts.Step(step); // NIGHT12: challenges.js step (top speed, the ARCLINE's air distance); nothing while not recording
            profEnd?.Invoke(PAfter);
            profBegin?.Invoke(PMode);
            if (!n34HoldWorld) StepMode(step);
            profEnd?.Invoke(PMode);
            // main.js: falling past the kill plane in Survival is death (a full HP blow)
            if (mover.position.y < map.killPlaneY && flow == Flow.Playing) health.Damage(health.max, "fall");
            current = mover.position;
            RaiseStepEnded(); // NIGHT14: observers only (SurvivalSim.Hooks.cs)
            profEnd?.Invoke(PStep);
        }

        /// <summary>A live enemy within style.trickEnemyRange of the player (the style meter's own test; U3-13.9's drain pause).</summary>
        bool EnemyNear()
        {
            double r = enemyNearRange;
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive) continue;
                double dx = e.position.x - mover.position.x, dy = e.position.y - mover.position.y, dz = e.position.z - mover.position.z;
                if (dx * dx + dy * dy + dz * dz <= r * r) return true;
            }
            return false;
        }

        /// <summary>stepMode: the DOWN timer, then the run's round loop.</summary>
        void StepMode(double dt)
        {
            if (downSteps > 0 && --downSteps == 0)
            {
                EndRun();
                return;
            }
            director.fullPace = exfil.FullPace; // NIGHT11 design.exfil: the HORDE at full pace (false otherwise: the web's director)
            if (flow == Flow.Playing && downSteps == 0 && !holdRunClock) run.Step(dt); // NIGHT19: THE RANGE holds the round clock while a cell plays
            // NIGHT11 design.exfil: an exfil ends the run either way: a win straight to RESULTS ("EXFIL"); a failure on the
            // clock as a death would (DOWN, then RESULTS, "EXFIL FAILED"; a death during it is already on that path).
            if (exfil.Outcome != null && flow == Flow.Playing && downSteps == 0 && run.Live)
            {
                if (exfil.Outcome == Exfil.Win) EndRun();
                else
                {
                    downSteps = MovementMath.StepsFor(cfg.Num("health.downTime"), step);
                    lastDeath = recap.At(SimTime);
                    downTarget = null;
                    flow = Flow.Down;
                }
            }
        }

        /// <summary>NIGHT11 (survival.js hooks.holdRound): a HOLD not held yet, or a running exfil (port-only), keeps the round from clearing.</summary>
        bool HoldRound() => objectives.Holding || exfil.Holding;

        /// <summary>NIGHT11 (survival.js hooks.step): objectives.step, then the exfil's clock (port-only), each fight step after the spawner.</summary>
        void RunStepHook(double dt)
        {
            objectives.Step();
            exfil.Step(dt);
        }

        /// <summary>
        /// NIGHT13 U3-26.2 / U3-26.6: one fingerprint run on this composition (SimFingerprint): a plain Survival run at HEAT 0
        /// on `seed` with no LAB and no records, the player at 1e6 HP, every step on the fixed input script through a
        /// ScriptedInput (the look its own), a hash every `every` steps. The caller pins the rest (onScreen, the settings'
        /// defaults, a fresh profile) and restores what it changed; the input is restored here.
        /// </summary>
        Func<string, string> fingerprintAssist; // NIGHT15: the cached pin (no delegate built per run)
        string webAssistMode; // config settings.aimAssist, read once (AimAssist asks every step: PORT-RULES section 6)

        /// <summary>NIGHT15 U3-28.7: the fingerprint run's assist mode: the web's default (config settings.aimAssist).</summary>
        string FingerprintAssistMode(string playerMode) => webAssistMode ??= cfg.Str("settings.aimAssist");

        public List<string> RunFingerprint(uint seed, int steps, int every, out string summary)
        {
            var lines = new List<string>();
            IPlayerInput devices = input;
            var script = new ScriptedInput();
            bool nr = noRecords, nl = noLab;
            noRecords = noLab = true;
            // NIGHT14 (the Player smoke's step 18): the mouse's aim assist whatever device played last; the pad's SNAP MODE and
            // breakaway cap (PadReader's hooks) are set aside for the run and handed back after it.
            Func<string, string> modeFor = assistModeFor;
            Func<double> cap = breakawayCap;
            // NIGHT15 U3-28.7: and the web's own default mode (config settings.aimAssist, 'snap'), whatever the player's
            // setting: the Unity defaults put the mouse at OFF (design.aimAssist), the baseline is the web's.
            fingerprintAssist ??= FingerprintAssistMode;
            assistModeFor = fingerprintAssist;
            breakawayCap = null;
            setup.mode = RunSetup.Survival;
            setup.heat = 0;
            setup.seed = null;
            // NIGHT19: until the close re-records the on-baseline (FingerprintBaseline.N19OnPath), the composed fingerprint runs
            // with every NIGHT19 block off, so it must print NIGHT18's baseline whatever the design defaults say (rule 3).
            N19Blocks? n19Saved = n19Override;
            if (!FingerprintBaseline.N19OnPath) n19Override = N19Blocks.Off;
            N20Blocks? n20Saved = n20Override; // NIGHT20: and every NIGHT20 block off (the same rule)
            if (!FingerprintBaseline.N19OnPath) n20Override = N20Blocks.Off;
            N21Blocks? n21Saved = n21Override; // NIGHT21: and every NIGHT21 block off until the close (FingerprintBaseline.N21OnPath)
            if (!FingerprintBaseline.N21OnPath) n21Override = N21Blocks.Off;
            N22Blocks? n22Saved = n22Override; // NIGHT22: and every NIGHT22 block off until the close (FingerprintBaseline.N22OnPath)
            if (!FingerprintBaseline.N22OnPath) n22Override = N22Blocks.Off;
            N27Blocks? n27Saved = n27Override; // NIGHT27: and every NIGHT27 block off until the freeze (FingerprintBaseline.N27OnPath)
            if (!FingerprintBaseline.N27OnPath) n27Override = N27Blocks.Off;
            N29Blocks? n29Saved = n29Override; // NIGHT29: and every NIGHT29 block off until the freeze (FingerprintBaseline.N29OnPath)
            if (!FingerprintBaseline.N29OnPath) n29Override = N29Blocks.Off;
            N30Blocks? n30Saved = n30Override; // NIGHT30: and every NIGHT30 block off until the freeze (FingerprintBaseline.N30OnPath)
            if (!FingerprintBaseline.N30OnPath) n30Override = N30Blocks.Off;
            N31Blocks? n31Saved = n31Override; // NIGHT31: and every NIGHT31 block off until the freeze (FingerprintBaseline.N31OnPath)
            if (!FingerprintBaseline.N31OnPath) n31Override = N31Blocks.Off;
            N32Blocks? n32Saved = n32Override; // NIGHT32: and every NIGHT32 block off until the freeze (FingerprintBaseline.N32OnPath)
            if (!FingerprintBaseline.N32OnPath) n32Override = N32Blocks.Off;
            N33Blocks? n33Saved = n33Override; // NIGHT33: and every NIGHT33 block off until the freeze (FingerprintBaseline.N33OnPath)
            if (!FingerprintBaseline.N33OnPath) n33Override = N33Blocks.Off;
            int shop33Saved = shop33Owned, bank33Saved = bank33; // NIGHT33 P: the fingerprint buys nothing and banks nothing
            shop33Owned = bank33 = 0;
            N34Blocks? n34Saved = n34Override; // NIGHT34: and every NIGHT34 block off until the freeze (FingerprintBaseline.N34OnPath)
            if (!FingerprintBaseline.N34OnPath) n34Override = N34Blocks.Off;
            N35Blocks? n35Saved = n35Override; // NIGHT35: and every NIGHT35 block off until the freeze (FingerprintBaseline.N35OnPath)
            if (!FingerprintBaseline.N35OnPath) n35Override = N35Blocks.Off;
            N36Blocks? n36Saved = n36Override; // NIGHT36: and every NIGHT36 block off until the freeze (FingerprintBaseline.N36OnPath)
            if (!FingerprintBaseline.N36OnPath) n36Override = N36Blocks.Off;
            N37Blocks? n37Saved = n37Override; // NIGHT37: and every NIGHT37 block off until the freeze (FingerprintBaseline.N37OnPath)
            if (!FingerprintBaseline.N37OnPath) n37Override = N37Blocks.Off;
            N38Blocks? n38Saved = n38Override; // NIGHT38: and every NIGHT38 block off until the freeze (FingerprintBaseline.N38OnPath)
            if (!FingerprintBaseline.N38OnPath) n38Override = N38Blocks.Off;
            N39Blocks? n39Saved = n39Override; // NIGHT39: and every NIGHT39 block off until the freeze (FingerprintBaseline.N39OnPath)
            if (!FingerprintBaseline.N39OnPath) n39Override = N39Blocks.Off;
            N40Blocks? n40Saved = n40Override; // NIGHT40: and every NIGHT40 block off until the freeze (FingerprintBaseline.N40OnPath)
            if (!FingerprintBaseline.N40OnPath) n40Override = N40Blocks.Off;
            N41Blocks? n41Saved = n41Override; // NIGHT41: and every NIGHT41 block off until the freeze (FingerprintBaseline.N41OnPath)
            if (!FingerprintBaseline.N41OnPath) n41Override = N41Blocks.Off;
            N42Blocks? n42Saved = n42Override; // NIGHT42: and every NIGHT42 block off until the freeze (FingerprintBaseline.N42OnPath)
            if (!FingerprintBaseline.N42OnPath) n42Override = N42Blocks.Off;
            N43Blocks? n43Saved = n43Override; // NIGHT43: and every NIGHT43 block off until the freeze (FingerprintBaseline.N43OnPath)
            if (!FingerprintBaseline.N43OnPath) n43Override = N43Blocks.Off;
            RowsFingerprintEnter(); // NIGHT46 rows: rows of nights after RowsBaseline.OnPathThrough off until their freeze
            N26Blocks? n26Saved = n26Override; // NIGHT26: and every NIGHT26 block off until the freeze (FingerprintBaseline.N26OnPath)
            if (!FingerprintBaseline.N26OnPath) n26Override = N26Blocks.Off;
            N25Blocks? n25Saved = n25Override; // NIGHT25: and every NIGHT25 block off until the freeze (FingerprintBaseline.N25OnPath)
            if (!FingerprintBaseline.N25OnPath) n25Override = N25Blocks.Off;
            N24Blocks? n24Saved = n24Override; // NIGHT24: and every NIGHT24 block off until the freeze (FingerprintBaseline.N24OnPath)
            if (!FingerprintBaseline.N24OnPath) n24Override = N24Blocks.Off;
            try
            {
                input = script;
                script.SetLook(devices.Yaw, devices.Pitch);
                StartRun(seed, null, null);
                health.max = health.hp = 1e6; // the script's player lives the whole run (the bench's god mode, smaller)
                for (int i = 0; i < steps; i++)
                {
                    double yaw = script.Yaw, pitch = script.Pitch;
                    SimFingerprint.Script(i, Eye, enemies, ref yaw, ref pitch, out InputFrame move, out CombatInput combat);
                    script.SetLook(yaw, pitch);
                    script.Set(move, combat);
                    Step();
                    if ((i + 1) % every == 0) lines.Add($"step {i + 1} hash {SimFingerprint.Hash(this):x16}");
                }
                summary = $"run {run.state} round {run.round}, kills {run.stats.kills}, shots {run.stats.shots}, earned {economy.earned:0}, alive {enemies.AliveCount}, owned {loadout.owned.Count}, flow {flow}";
            }
            finally
            {
                input = devices;
                noRecords = nr;
                noLab = nl;
                assistModeFor = modeFor;
                breakawayCap = cap;
                n19Override = n19Saved;
                n20Override = n20Saved;
                n21Override = n21Saved;
                n22Override = n22Saved;
                n24Override = n24Saved;
                n26Override = n26Saved;
                n27Override = n27Saved;
                n29Override = n29Saved;
                n30Override = n30Saved;
                n31Override = n31Saved;
                n32Override = n32Saved;
                n33Override = n33Saved;
                shop33Owned = shop33Saved;
                bank33 = bank33Saved;
                n34Override = n34Saved;
                n35Override = n35Saved;
                n36Override = n36Saved;
                n37Override = n37Saved;
                n38Override = n38Saved;
                n39Override = n39Saved;
                n40Override = n40Saved;
                n41Override = n41Saved;
                n42Override = n42Saved;
                n43Override = n43Saved;
                RowsFingerprintExit(); // NIGHT46 rows
                n25Override = n25Saved;
            }
            return lines;
        }
    }
}
