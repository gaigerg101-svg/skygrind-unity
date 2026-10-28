// NIGHT20 E1 (builder P; Research/2026-10-06-additions-backlog.md E1, QUEUE-first-fifteen U3-28.6): the first fifteen
// minutes' moments of need on the sim (design.adds.firstFifteen; the rule is Core/FirstFifteen.cs; Ui's onboarding draws
// the line). Each verb at the moment it would help, gone on its first use:
// - parry: the first spitter volley (a glob in the air at you: "spitterVolley"); used on a parry ("parry", the player's);
// - melee: the first live enemy within the web's punch reach (config punch.range, horizontal); used on a punch;
// - box: the first time the bank holds the box's price (design.box.price; the backlog's "first 950 points"); used on a roll;
// - hold: a HOLD ring appears ("objectiveStart" hold); used when you stand inside it (the game's own test, Objectives.Inside);
//   gone when its round ends;
// - mods: the first draft ("modDraft"); used on the pick; gone when the round begins;
// - exfil: the radio offered (Exfil.CanCall); used on the call ("exfilCall"); gone when the round begins.
// On for a run when design.adds and design.adds.firstFifteen are on (N20Blocks.Add, read once at the run's start), never
// in a bot's run (sim.metaSilent: the AUTOPILOT, the attract, the judges); Force is the tests' switch. A newly learned verb
// saves the profile (its "learn.<id>" flag) once. Step runs once a rendered frame: a few compares and one pass over the
// live enemies while melee is unlearned; nothing allocates (the handlers are made once). No UnityEngine use.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class FirstFifteenWatch
    {
        public const string Parry = "parry", Melee = "melee", Box = "box", Hold = "hold", Mods = "mods", BigPick = "bigpick", ExfilId = "exfil";

        /// <summary>The rule (Active: the line to draw; Version: changes to it).</summary>
        public readonly FirstFifteen rule;

        /// <summary>Tests: on whatever the blocks say (a bot's run still turns it off).</summary>
        public bool Force;

        readonly SurvivalSim sim;
        readonly Action saveProfile;
        readonly double punchRange;
        bool blockOn; // design.adds + design.adds.firstFifteen, taken at the run's start

        public FirstFifteenWatch(SurvivalSim sim, Profile profile, Action saveProfile)
        {
            this.sim = sim;
            this.saveProfile = saveProfile;
            rule = new FirstFifteen(sim.cfg, profile, true);
            punchRange = sim.cfg.Num("punch.range");
            sim.RunStarted += () =>
            {
                blockOn = sim.n20.Add(sim.cfg, "firstFifteen");
                rule.Reset();
                boxWas = radioWas = reachWas = false;
            };
            EventBus ev = sim.events;
            ev.On("spitterVolley", _ => Need(Parry));
            ev.On<HitEvent>("parry", e =>
            {
                if (e.owner == "player") Used(Parry);
            });
            ev.On<MeleeEvent>("punch", e =>
            {
                if (e.owner == "player") Used(Melee);
            });
            ev.On<BoxOpenEvent>("boxOpen", e =>
            {
                if (e.owner == "player") Used(Box);
            });
            ev.On<ObjectiveStartEvent>("objectiveStart", e =>
            {
                if (e.type == Objectives.Hold) Need(Hold);
            });
            ev.On<ObjectiveDoneEvent>("objectiveDone", e =>
            {
                if (e.type == Objectives.Hold) Used(Hold);
            });
            ev.On<ModEvent>("modDraft", _ => Need(BigPickRun ? BigPick : Mods)); // NIGHT24: a BIG PICK draft has its own line (PRESS 1 2 3 OR A: TAKE ONE)
            ev.On<ModEvent>("modPick", e =>
            {
                if (!e.free) Used(BigPickRun ? BigPick : Mods);
            });
            ev.On<ExfilEvent>("exfilCall", e =>
            {
                if (e.owner == "player") Used(ExfilId);
            });
            ev.On<RoundEvent>("roundBreak", _ => rule.Gone(Hold)); // the HOLD's round is over
            ev.On<RoundEvent>("roundBegin", _ =>
            {
                rule.Gone(Mods); // the draft closed
                rule.Gone(BigPick);
                rule.Gone(ExfilId); // the radio is no longer offered
            });
        }

        /// <summary>This run teaches: the blocks on (or Force), and not a bot's run.</summary>
        public bool On => (blockOn || Force) && !sim.metaSilent;

        void Need(string id)
        {
            if (On) rule.Need(id);
        }

        void Used(string id)
        {
            if (On && rule.Used(id)) saveProfile?.Invoke();
        }

        /// <summary>Once a rendered frame while playing: the moments read from the sim's state (the box's price in the bank,
        /// the radio offered, the ring stood in, an enemy in punching reach).</summary>
        public void Step()
        {
            if (!On) return;
            // Each a rising edge: a moment shown its showSeconds without a use comes back on the next one, never every frame.
            bool box = sim.box != null && sim.economy.total >= sim.box.price;
            if (box && !boxWas) rule.Need(Box);
            boxWas = box;
            bool radio = sim.exfil != null && sim.exfil.enabled && sim.exfil.CanCall();
            if (radio && !radioWas) rule.Need(ExfilId);
            radioWas = radio;
            if (sim.objectives.Holding && sim.objectives.Inside) Used(Hold);
            bool reach = !rule.Learned(Melee) && EnemyInReach();
            if (reach && !reachWas) rule.Need(Melee);
            reachWas = reach;
        }

        bool boxWas, radioWas, reachWas;

        /// <summary>NIGHT24: this run drafts BIG PICKs (Mods.big is active).</summary>
        bool BigPickRun => sim.mods.big != null && sim.mods.big.Active;

        bool EnemyInReach()
        {
            Vec3 p = sim.mover.position;
            List<Enemy> all = sim.enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (!e.alive) continue;
                if (JsMath.Hypot(e.position.x - p.x, e.position.z - p.z) <= punchRange) return true;
            }
            return false;
        }
    }
}
