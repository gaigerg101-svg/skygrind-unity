// NIGHT16 U3-29.5 (builder I owns this file from the shared commit on): the impact layer's hooks in SurvivalGame's loop
// (SurvivalGame.cs Update). The loop asks, once a frame, how fast the world runs (ImpactBeginFrame: 0 frozen, under 1 a
// slow-mo or a freeze's last frame, 1 as before), and after each sim step whether a freeze began in it (ImpactStepEnded:
// the frozen time is skipped, not caught up). The render half reads hitstopLeft (the longest of the sim's freeze and this
// layer's) and WorldScale. The director (Game/ImpactFrames.cs, on Core's ImpactRules) is built on the first simulated frame,
// on this game's bus; a scene reload (a map switch) builds a new game and a new director; a new run resets it (OnNewRun).
//
// The sim's own hitstop (SurvivalSim.Hitstop: a player kill's feel.hitstopTime, a melee hit's feel.momentumHitstop, a
// parry's design.ultra.parryHitstop / design.parry.perfectHitstop) is pacing, not a sim result (nothing in the sim reads
// sim.hitstopLeft but Hitstop's own "longer than what is left" test and the snapshot, SurvivalSim.Save.cs:44). This file
// hands it to the impact layer: after every sim step, when the step asked (sim.hitstopRequested), its length goes to
// ImpactRules as the step's sim request (its cause came through the bus's `hitstop` event), and sim.hitstopLeft is set to 0
// at once, so the impact layer's freeze (its tiers, cooldowns and budget; HIT FREEZE OFF: none) is the only one the loop
// runs. A value found at a frame's start (a restored snapshot, a request outside a step) is drained the same way, unused.
// Effect: in a Game run sim.hitstopLeft is 0 between steps (a snapshot saves 0), and each step starts at 0, so every one of
// the sim's requests emits its `hitstop` event (u3-n15 let a request no longer than the running freeze pass silently).
using System;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        ImpactFrames impactFrames;

        /// <summary>The impact director (built on the first simulated frame; null before).</summary>
        public ImpactFrames ImpactDirector => impactFrames;

        /// <summary>The impact layer's own freeze left (real s); the render half freezes on max(this, sim.hitstopLeft).</summary>
        double ImpactFreezeLeft => impactFrames != null ? impactFrames.FreezeLeft : 0;

        /// <summary>The world's time scale this frame (1: real time; a slow-mo under 1). WorldDt = frameDt x this outside a freeze.</summary>
        public double WorldScale { get; private set; } = 1;

        /// <summary>Once a frame while simulating: count the freeze down and say how fast the sim runs this frame (0: frozen).</summary>
        double ImpactBeginFrame(double frameDt)
        {
            if (impactFrames == null) impactFrames = new ImpactFrames(this, Move54.ImpactConfig(cfg), events); // NIGHT54 M move54 (Game/Move54.cs): the row's kill freezes, budget and gap (cfg itself when off)
            if (sim.hitstopLeft > 0) sim.hitstopLeft = 0; // drained: the impact layer owns the freeze (the header)
            double share = impactFrames.BeginFrame(frameDt);
            share = SkygrindTimeFrame(frameDt, share); // NIGHT20 F2 (builder V: SurvivalGame.SkygrindTime20.cs; design.adds.skygrindTime)
            share = DebutSlowFrame(frameDt, share); // NIGHT20 C1 (the lead: SurvivalGame.Debut20.cs): a debut card slows the world <= 1.5 s
            share = BossSlowMoFrame(frameDt, share); // NIGHT20 A1 (builder EN: EnemyViews.Bulwark.cs): the BULWARK finisher's slow-mo (main.js bossDying)
            share = Kill74SlowFrame(frameDt, share); // NIGHT74 B3 (SurvivalGame.Kill74.cs): kill74's 0.35 s hold on the boss kill, fewer steps per frame, none skipped
            share = Arena36SlowFrame(frameDt, share); // NIGHT36 L (the lead: SurvivalGame.Arena36.cs): an arena room's last kill slows the world a moment
            share = SlowAimFrame(frameDt, share); // NIGHT27 slowAim27 (the lead: SurvivalGame.SlowAim27.cs): wall-riding + aiming eases the world to design.n27.slowAim27.scale
            share = Move54SlowFrame(frameDt, share); // NIGHT54 M move54 (SurvivalGame.Move54.cs): the slow-mo eased in (the share itself with the row off)
            share = N55KFrame(frameDt, share); // NIGHT55 K (SurvivalGame.N55K.cs): kill55's chain hold (the share itself with the rows off)
            share = Clear55Frame(frameDt, share); // NIGHT55 R clear55 (SurvivalGame.N55R.cs): a combat room's clear holds the world design.n55.clear55.numbers.holdS (the share itself with the row off)
            WorldScale = share;
            return share;
        }

        /// <summary>After each sim step: a freeze began in it (the sim's request through the impact layer, or the layer's own).</summary>
        bool ImpactStepEnded()
        {
            double simAsk = sim.hitstopRequested ? sim.hitstopLeft : 0;
            sim.hitstopLeft = 0; // drained at once (pacing, not a result: the header)
            return N55KStepEnded(impactFrames != null && impactFrames.StepEnded(simAsk)); // NIGHT55 K (SurvivalGame.N55K.cs): a kill55 hold beginning stops the stepping too (the rules' answer with the rows off)
        }

        /// <summary>
        /// NIGHT16 review (finding 5): the way to TITLE (SurvivalGame.ToTitle's first line): a freeze or a slow-mo running ends
        /// here, and the world's scale is real time again, so the title is never drawn frozen or slowed (the render half reads
        /// hitstopLeft and WorldScale without a simulating check; sim.ToTitle zeroes only sim.hitstopLeft).
        /// </summary>
        void ImpactToTitle()
        {
            impactFrames?.rules.Halt();
            SkygrindTimeHalt(); // NIGHT20 F2 (builder V)
            DebutSlowHalt(); // NIGHT20 C1 (the lead)
            Kill74Halt(); // NIGHT74 B3: the kill74 hold never outlives a return to TITLE
            SlowAimHalt(); // NIGHT27 (the lead)
            Move54SlowHalt(); // NIGHT54 M move54
            N55KHalt(); // NIGHT55 K (SurvivalGame.N55K.cs)
            WorldScale = 1;
        }

        /// <summary>What a freeze shows (after the camera and the viewmodel): the FOV kicks and the flash (ImpactFrames.Render).</summary>
        partial void ImpactRender(double frameDt) => impactFrames?.Render(frameDt);
    }
}
