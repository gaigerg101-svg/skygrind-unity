// NIGHT16 U3-29.5 (builder I): no eaten input (NIGHT16.md section 0 rule 2). A jump pressed and released inside an impact
// freeze still jumps: the frame loop polls the devices every frame, frozen or not (SurvivalGame.Update: Poll before the
// impact scale), the press waits in the reader's latch (the pad's frame.latched here; the keyboard's jumpLatch the same
// way), and the first sim step after the freeze consumes it. The loop is modelled as SurvivalGame.Update runs it: the
// impact layer's BeginFrame gives the frame's share (0 frozen), the accumulator steps the sim at 120 Hz. A headless sim
// (SurvivalSim.Headless) on the arena, driven by the Game's ReaderInput with a pad filled by hand (no Input System device).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class ImpactLatchTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        [Test]
        public void AJumpPressedAndReleasedInsideAFreeze_StillJumps_OnTheFirstStepAfterIt()
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));
            var pad = new PadReader(cfg) { Connected = true };
            var combat = new CombatReader();
            combat.Init(cfg, null);
            var input = new ReaderInput(new InputReader(cfg.movement), combat, pad, null);
            SurvivalSim sim = SurvivalSim.Headless(cfg, map, input);
            sim.noLab = sim.noRecords = true; // automation: the design defaults (the headless store has no LAB rows), nothing stored
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(4242, null, null);
            var rules = new ImpactRules(cfg);
            double dt = 1.0 / 144, acc = 0;
            int steps = 0;

            // One rendered frame: the devices polled (always), then the impact scale, then the steps it allows.
            void Frame(bool jumpHeld)
            {
                pad.state.Clear();
                if (jumpHeld) pad.state.Set(PadButtonName.A, true, 1);
                pad.Step(dt, true);
                input.ApplyPad(dt, 1);
                double share = rules.BeginFrame(dt, false);
                if (share <= 0) return;
                acc += dt * share;
                while (acc >= sim.step)
                {
                    sim.hitstopRequested = false;
                    sim.Step();
                    steps++;
                    acc -= sim.step;
                    if (rules.EndStep(false, sim.hitstopRequested ? sim.hitstopLeft : 0))
                    {
                        acc %= sim.step;
                        break;
                    }
                }
            }

            for (int i = 0; i < 288 && !sim.mover.grounded; i++) Frame(false);
            for (int i = 0; i < 30; i++) Frame(false); // settled on the floor
            Assert.That(sim.mover.grounded, Is.True, "the run starts on the floor");

            // A headshot kill's freeze (design.impact.freeze.headshotKill) begins.
            rules.OnHit(new HitEvent { owner = "player", weapon = "revolver", kind = "husk", headshot = true, killed = true, shot = 1 });
            rules.OnKill(new KillEvent { owner = "player", weapon = "revolver", kind = "husk", headshot = true });
            Assert.That(rules.EndStep(false), Is.True);
            double freeze = rules.FreezeLeft;
            Assert.That(freeze, Is.GreaterThan(4 * dt), "the freeze outlasts the press and the release");

            int before = steps;
            Frame(false);
            Frame(true); // pressed inside the freeze
            Frame(true);
            Frame(false); // released inside the freeze
            Assert.That(steps, Is.EqualTo(before), "no sim step ran: the world is frozen");
            Assert.That(rules.FreezeLeft, Is.GreaterThan(0), "still frozen at the release");
            Assert.That(sim.mover.grounded, Is.True, "nothing happened yet");

            int guard = 0;
            while (steps == before && guard++ < 100) Frame(false);
            Assert.That(steps, Is.GreaterThan(before), "the freeze ended");
            Assert.That(sim.mover.velocity.y, Is.GreaterThan(0), "the first step after the freeze jumped: the press waited in the latch");
            Assert.That(sim.mover.grounded, Is.False);
        }
    }
}
