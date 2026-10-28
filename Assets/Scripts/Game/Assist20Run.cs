// NIGHT20 E2 (builder P): ASSIST on a run (design.adds.assist; the rule is Core/Assist20.cs; SETTINGS > ACCESS > ASSIST,
// UnityTuning.AssistSetting, off by default). At each run's start (sim.RunStarted, after HEAT has set its GLASS CANNON):
// with the block on (design.adds AND design.adds.assist), the setting on and a human's run (never a bot's: metaSilent),
// the run takes the damage x (1 - resistance) (Health.takenMult *= that: no hook in Health), is stamped (sim.noRecords:
// no personal best, no contracts, as the LAB; RESULTS shows Stamp) and each of the player's downs adds one to the saved
// count (the next run resists 2 % more, up to the cap; Hades counts every death in God Mode). Off: nothing changes, and
// noRecords is back as it was before ASSIST set it. Not in the tape: a stamped run replays without it (as GG's choice, not
// the run's rules). Nothing per step; handlers made once. No UnityEngine use.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Assist20Run
    {
        /// <summary>Tests: the block counts as on (the setting and a bot's run still decide).</summary>
        public bool Force;

        readonly SurvivalSim sim;
        readonly Func<bool> settingOn;
        readonly Func<int> loadDowns;
        readonly Action<int> saveDowns;
        readonly Assist20.P p;
        bool stampedRecords, savedNoRecords; // noRecords as it was before ASSIST set it

        /// <summary>This run has ASSIST on.</summary>
        public bool Active { get; private set; }

        /// <summary>This run's resistance (0 when off).</summary>
        public double Resistance { get; private set; }

        /// <summary>The downs counted so far (the saved count).</summary>
        public int Downs { get; private set; }

        /// <summary>RESULTS' stamp for this run ("ASSIST 24%"; null when off).</summary>
        public string Stamp { get; private set; }

        public Assist20Run(SurvivalSim sim, Func<bool> settingOn, Func<int> loadDowns, Action<int> saveDowns)
        {
            this.sim = sim;
            this.settingOn = settingOn;
            this.loadDowns = loadDowns;
            this.saveDowns = saveDowns;
            p = Assist20.P.From(sim.cfg);
            sim.RunStarted += OnRunStarted;
            sim.events.On<DeathEvent>("death", e =>
            {
                if (!Active || e.owner != "player") return;
                Downs++;
                saveDowns?.Invoke(Downs);
            });
        }

        void OnRunStarted()
        {
            Assist20.P rule = p;
            if (Force) rule.enabled = true; // the tests' switch (the row's own enabled stands in for the lead's adds bit)
            bool block = Force || sim.n20.Add(sim.cfg, "assist");
            Active = block && rule.enabled && !sim.metaSilent && settingOn != null && settingOn();
            if (!Active)
            {
                Resistance = 0;
                Stamp = null;
                if (stampedRecords)
                {
                    sim.noRecords = savedNoRecords;
                    stampedRecords = false;
                }
                return;
            }
            Downs = loadDowns != null ? Math.Max(0, loadDowns()) : 0;
            Resistance = Assist20.Resistance(rule, Downs);
            Stamp = Assist20.Stamp(Resistance);
            sim.health.takenMult *= 1 - Resistance;
            if (!stampedRecords)
            {
                savedNoRecords = sim.noRecords;
                stampedRecords = true;
            }
            sim.noRecords = true;
        }
    }
}
