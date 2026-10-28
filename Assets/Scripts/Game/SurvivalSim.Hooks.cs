// NIGHT14 U3-27 (the shared types): the sim composition's hooks for tonight's systems (NIGHT14.md section 4). Each is
// an observer or a swap; with nothing subscribed and the flags at their defaults the sim steps exactly as at u3-n13 (the
// fingerprint is equal). No UnityEngine use.
//   StepEnded      after every Step (the flight recorder, the tape, the route track), in subscription order
//   RunStarted     after StartRun (the run is live, its seed and setup set)
//   UseInput       swap the IPlayerInput (the AUTOPILOT, a tape player), the look carried over
//   metaSilent     NIGHT14 rule 7: an autopilot / farm / attract run writes no profile, records, contracts, mastery,
//                  DAILY or run log (it is judged, never stored, like noRecords)
//   utcNow         the clock StartRun reads for the DAILY (a tape pins it)
//   Headless       a fresh composition with the Player's inputs pinned (the farm, a tape's replay, tests)
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>Raised at the end of every Step (after the kill plane): observers read the state, never change it.</summary>
        public event Action StepEnded;

        /// <summary>Raised at the end of StartRun.</summary>
        public event Action RunStarted;

        /// <summary>NIGHT14 rule 7: the run records nothing (profile, records, contracts, mastery, DAILY, run log).</summary>
        public bool metaSilent;

        /// <summary>The wall clock StartRun reads (the DAILY's seed, day and twist). A tape's replay pins it.</summary>
        public Func<DateTime> utcNow = () => DateTime.UtcNow;

        /// <summary>Swap the player's input (the look carried over to the new one).</summary>
        public void UseInput(IPlayerInput next)
        {
            next.SetLook(input.Yaw, input.Pitch);
            input = next;
        }

        /// <summary>
        /// NIGHT14 U3-27.2: a clean slate per run (RETRY SAME SEED, the attract, the farm: same seed, same run). StartRun calls
        /// it right after ResetWorld: what a fresh composition starts with and a played run leaves behind. Found by hashing each
        /// system on its own after StartRun (Tools/SimHarness `slate`): the enemy pools and ids (reused dead enemies reordered
        /// the registry) and the loadout's shot ids (weapons.js shotSeq, never reset on the web either). On a fresh
        /// composition each is already in this state, so a first run (the fingerprint's) is unchanged.
        /// </summary>
        void CleanSlate()
        {
            husks.ClearPools();
            loadout.shotSeq = 0;
            assist?.NewRun(); // NIGHT16 (the NIGHT14 residual): the aim assist's scan clock (a scan every scanEvery steps) from 0
        }

        void RaiseStepEnded() => StepEnded?.Invoke();

        void RaiseRunStarted() => RunStarted?.Invoke();

        /// <summary>
        /// A fresh headless composition (no scene, no views, no camera), the Player's inputs pinned the way the fingerprint
        /// pins them: OnScreen false, the settings at their defaults, a fresh profile, the keys' labels as their codes; taken
        /// to TITLE with the perk machines' buy points on the flat core (PerkMachines.Render's TITLE frames). FingerprintTests
        /// keeps its own copy of these lines (its baseline was recorded with them).
        /// </summary>
        public static SurvivalSim Headless(GameConfig cfg, SurvivalMap map, IPlayerInput input = null)
        {
            var store = new SettingsStore(cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
            store.Load("");
            var sim = new SurvivalSim(cfg, map, new EventBus(), new Lab(cfg), input ?? new ScriptedInput(), new Profile(), store.Num, store.Bool, store.Str, _ => false, a => a);
            sim.ToTitle();
            foreach (Machine m in sim.machines) sim.RideCore(m);
            return sim;
        }
    }
}
