// NIGHT47 builder F (SurvivalSim.N47.cs's F hooks): the sim rows pulse47 (the LT blast: Core/Guns/Blast47.cs) and overheal47 (round 2).
// Ids are resolved by SurvivalSim.N47.cs (idPulse47, idOverheal47); a row the config lacks reads off (-1), the older game. Off: NIGHT46 bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>overheal47Gain / overheal47Spent (new event names, for GG's sounds): how much, and the overheal after it.</summary>
    public sealed class Overheal47Event
    {
        public double amount, over;
    }

    public sealed partial class SurvivalSim
    {
        Blast47Params blast47Params; // design.n47.pulse47.numbers, read once (the config does not change in a session)

        /// <summary>The run's pulse47 state (the harness): the row was on at this run's start.</summary>
        public bool Pulse47On { get; private set; }

        partial void N47FStartRun()
        {
            Pulse47On = idPulse47 >= 0 && rows.On(idPulse47);
            if (pulse27 != null)
            {
                if (Pulse47On && blast47Params == null) blast47Params = Blast47Params.From(cfg);
                pulse27.blast47 = Pulse47On ? blast47Params : null;
                pulse27.blast47Hits = pulse27.blast47Launched = pulse27.blast47Ring = 0;
            }
            Overheal47StartRun();
        }

        // ---- overheal47 (G17; DECIDE47 D-4): a close kill at full health spills above 100 up to a cap; it holds, then drains back (decay, never
        // regen below the max: the w19 rule holds); a hit takes it first (Health.over). The source is heal36's own (VERBS 42: a kill within
        // design.n36.heal36.radius of the eye spills design.n36.heal36.amount): what heal36 could not fit under the max goes over it. ----

        /// <summary>The run's overheal47 state (the harness, the HUD).</summary>
        public bool Overheal47On { get; private set; }

        /// <summary>The overheal's most (cap - max), 0 with the row off.</summary>
        public double Overheal47Max { get; private set; }

        double oh47Amount, oh47Radius, oh47Decay;
        int oh47HoldSteps, oh47HoldLeft, oh47Kills;
        double oh47Got;
        bool oh47Hooked;
        readonly Overheal47Event oh47Event = new Overheal47Event();

        /// <summary>Counters for the harness: overheal gained and spent this run.</summary>
        public double Overheal47Gained, Overheal47Spent;

        void Overheal47StartRun()
        {
            Overheal47On = idOverheal47 >= 0 && rows.On(idOverheal47);
            health.over = 0;
            oh47HoldLeft = oh47Kills = 0;
            oh47Got = 0;
            Overheal47Gained = Overheal47Spent = 0;
            if (!Overheal47On) { Overheal47Max = 0; return; }
            const string p = "design.n47.overheal47.numbers.";
            Overheal47Max = System.Math.Max(0, cfg.Num(p + "cap") - health.max);
            oh47Amount = cfg.Num("design.n36.heal36.amount");
            oh47Radius = cfg.Num("design.n36.heal36.radius");
            oh47Decay = cfg.Num(p + "decayPerSecond") * step;
            oh47HoldSteps = MovementMath.StepsFor(cfg.Num(p + "holdSeconds"), step);
            if (!oh47Hooked)
            {
                oh47Hooked = true;
                events.On<KillEvent>("kill", e =>
                {
                    if (!Overheal47On || e.owner != "player" || health.Dead || !n36.heal36) return;
                    Vec3 f = mover.position;
                    double d = JsMath.Hypot(e.position.x - f.x, e.position.y - (f.y + cfg.movement.body.eyeHeight), e.position.z - f.z);
                    if (d <= oh47Radius) oh47Kills++;
                });
                events.On<Heal36Event>(Heal36.Event, e => { if (Overheal47On) oh47Got += e.amount; });
            }
            oh47Last = 0;
        }

        double oh47Last; // the overheal at the end of the last step (a drop since then is a hit's: Health.Damage takes it first)

        void Overheal47Step()
        {
            if (!Overheal47On) return;
            if (health.over < oh47Last - 1e-9)
            {
                double spent = oh47Last - health.over;
                Overheal47Spent += spent;
                oh47Event.amount = spent;
                oh47Event.over = health.over;
                events.Emit("overheal47Spent", oh47Event);
            }
            if (oh47Kills > 0)
            {
                double spill = oh47Kills * oh47Amount - oh47Got; // what heal36 could not fit under the max this step
                oh47Kills = 0;
                oh47Got = 0;
                if (spill > 1e-9 && health.hp >= health.Cap - 1e-9)
                {
                    double gain = System.Math.Min(spill, Overheal47Max - health.over);
                    if (gain > 0)
                    {
                        health.over += gain;
                        Overheal47Gained += gain;
                        oh47HoldLeft = oh47HoldSteps;
                        oh47Event.amount = gain;
                        oh47Event.over = health.over;
                        events.Emit("overheal47Gain", oh47Event);
                    }
                }
            }
            else if (health.over > 0)
            {
                if (oh47HoldLeft > 0) oh47HoldLeft--;
                else health.over = System.Math.Max(0, health.over - oh47Decay);
            }
            oh47Last = health.over;
        }

        partial void N47FStep(double dt) => Overheal47Step();

        partial void N47FCampaign() { }
    }
}
