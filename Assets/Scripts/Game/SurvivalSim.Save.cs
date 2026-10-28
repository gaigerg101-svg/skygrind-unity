// NIGHT13 U3-26.10: the run snapshot's sections over this composition (agent E's list, RunRestoreTests proves it: a run
// restored at a break equals the uninterrupted run, three seeds): every stateful run system in its order (the "husks"
// section before every enemy reference), then the composition's own values. RunSnapshot.Save / Load take the list.
// A restore runs into a composition that has started a run on the saved seed (StartRun builds the run's structure:
// relays, traps, hold spots, buy items; Load checks them by id).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The snapshot's (name, IRunState) sections, in their order (design.box off: no box section).</summary>
        public List<(string name, IRunState state)> SnapshotSections()
        {
            var list = new List<(string, IRunState)>
            {
                ("run", run), ("economy", economy), ("health", health), ("regen", regen), ("mover", new MoverState(mover)),
                ("zones", zones), ("nav", nav), ("enemies", enemies), ("husks", husks), ("brain", brain), ("director", director),
                ("beacon20", Beacon20Ensure()), ("buyables", buyables), ("loadout", loadout), // NIGHT20 A3 (builder P2): the BEACON before the buy list (its entry)
                ("assist", new AimAssistState(assist)), ("projectiles", projectiles),
                ("style", style), ("perks", perks), ("mods", mods), ("powerUps", powerUps), ("forge", forge),
            };
            if (box != null) list.Add(("box", box));
            N24Sections(list); // NIGHT24 audit (f): the fist's charge, the BIG PICK's stream, the worm's stream (SurvivalSim.N24Save.cs)
            list.AddRange(new List<(string, IRunState)>
            {
                ("objectives", objectives), ("protocol", protocol), ("traps", traps), ("power", power), ("swarmDrop", swarmDrop),
                ("exfil", exfil), ("moments", moments), ("ultraHealth", ultraHealth), ("lab", lab), ("recap", recap),
                ("contracts", contracts), ("mastery", mastery), ("heat", heat), ("tips", tips), ("onboarding", onboarding),
                ("sim", SimSection()),
            });
            return list;
        }

        /// <summary>
        /// The composition's own run values: the step count (the recap's clock), the seed, DOWN's timer, the flow, hitstop, the
        /// feet for interpolation, the look (the input's), READY's hold, whether the run records and runs the first-run script.
        /// </summary>
        IRunState SimSection() => new CustomState(w =>
        {
            w.I("simStep", simStep);
            w.I("runSeed", runSeed);
            w.I("downSteps", downSteps);
            w.I("flow", (int)flow);
            w.D("hitstopLeft", hitstopLeft);
            w.V("previous", previous);
            w.V("current", current);
            w.D("yaw", input.Yaw);
            w.D("pitch", input.Pitch);
            w.I("readySteps", readySteps);
            w.B("runRecording", runRecording);
            w.B("runOnboarding", runOnboarding);
        }, r =>
        {
            simStep = r.Int("simStep");
            runSeed = r.U("runSeed");
            downSteps = r.Int("downSteps");
            flow = (Flow)r.Int("flow");
            hitstopLeft = r.D("hitstopLeft");
            previous = r.V("previous");
            current = r.V("current");
            input.SetLook(r.D("yaw"), r.D("pitch"));
            readySteps = r.Int("readySteps");
            runRecording = r.B("runRecording");
            runOnboarding = r.B("runOnboarding");
        });

        /// <summary>
        /// design.save's rule (NIGHT14-RESEARCH section 4, BO6's): a save is allowed at a round break only, once at least
        /// `minRounds` rounds are cleared, with no enemy alive, never downed and never during an exfil (solo: the port has one
        /// player). Never in a DAILY (NIGHT13 review): a DAILY is one try on its UTC day, and a resume on a later day would
        /// record that day's run late.
        /// </summary>
        public bool CanSaveAtBreak(int minRounds) =>
            run.Live && run.state == SurvivalRun.State.Break && run.cleared >= minRounds && downSteps == 0 && enemies.AliveCount == 0
            && exfil.phase == ExfilPhase.Off && exfil.Outcome == null;
    }
}
