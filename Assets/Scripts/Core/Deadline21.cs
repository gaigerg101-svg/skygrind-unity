// NIGHT21 U3-35.6 DEADLINE (builder S owns this file): the kill-or-die twist round (Mullet MadJack's hourglass of kills; its
// fatigue warning keeps this one twist among five, never a mode). Port-only: the web has no such twist. A fuse runs while the
// round runs; kills refill it, a SLIDE kill and a SLAM kill the most; when it burns out the player takes one heavy hit (never
// the run: the hit leaves at least 1 HP) and the fuse restarts shorter; the round's points double. Numbers: design.n21.deadline
// (Unity-only, each with its why in Design/unity-config.json). The twist's id and its draw are SpecialRounds' (Core/Enemies/
// SpecialRounds.cs: "deadline", added to the twist ids only with the row on); the fuse starts at the twist's "start" and stops at
// its "end" (both from the specialRound event, name "twist:deadline"). Timers are whole steps (rule 4). The fuse is DANGER
// (red-orange) on the HUD.
// Events: "deadlineTick" (DeadlineEvent) every tickEvery s of fuse and at each refill; "deadlineBurn" (DeadlineEvent, hit set)
// when it burns out.
using System;

namespace Skygrind.Core
{
    /// <summary>"deadlineTick" / "deadlineBurn": the fuse now.</summary>
    public sealed class DeadlineEvent
    {
        /// <summary>Seconds of fuse left, its full length now (a restart is shorter) and the share left 0..1.</summary>
        public double left, full, share;
        /// <summary>NIGHT22: the seconds the kill that refilled the fuse paid (2.5 a kill, 5 a slide kill, 6 a slam kill; 0 on a clock tick or a burn). The fuse may be capped at its full length.</summary>
        public double paid;
        /// <summary>The round, the burns this round and (a burn) the HP the hit took.</summary>
        public int round, burns;
        public double hit;
        /// <summary>A tick said because a kill refilled the fuse (not the clock).</summary>
        public bool refill;
    }

    public sealed class Deadline21
    {
        public const string TwistId = "deadline", Tick = "deadlineTick", Burn = "deadlineBurn";

        /// <summary>design.n21.deadline's numbers, read once.</summary>
        public struct P
        {
            public double fuse, killRefill, slideRefill, slamRefill, hitFrac, burnFuse, pointsMult, weight, tickEvery;

            public static P From(GameConfig c) => new P
            {
                fuse = c.Num("design.n21.deadline.fuse"), killRefill = c.Num("design.n21.deadline.killRefill"),
                slideRefill = c.Num("design.n21.deadline.slideRefill"), slamRefill = c.Num("design.n21.deadline.slamRefill"),
                hitFrac = c.Num("design.n21.deadline.hitFrac"), burnFuse = c.Num("design.n21.deadline.burnFuse"),
                pointsMult = c.Num("design.n21.deadline.pointsMult"), weight = c.Num("design.n21.deadline.weight"),
                tickEvery = c.Num("design.n21.deadline.tickEvery"),
            };
        }

        readonly EventBus events;
        readonly Health health;
        readonly Func<bool> sliding;
        readonly double dt;
        readonly P p;
        readonly DeadlineEvent payload = new DeadlineEvent();
        readonly int fuseSteps, killSteps, slideSteps, slamSteps, burnSteps, tickSteps;

        /// <summary>The row is on for this run (its start sets it).</summary>
        public bool on;
        /// <summary>The fuse is running (a deadline round is on).</summary>
        public bool Running { get; private set; }
        /// <summary>Steps of fuse left, the length it counts down from now (a restart is shorter), the round and its burns.</summary>
        public int Left { get; private set; }
        public int Full { get; private set; }
        public int Round { get; private set; }
        public int Burns { get; private set; }
        /// <summary>The hit the last burn took (HP), and the HP of every burn this run.</summary>
        public double LastHit;
        int sinceTick;

        public double LeftSeconds => Left * dt;
        public double Share => Full > 0 ? (double)Left / Full : 0;
        public P Numbers => p;

        public Deadline21(GameConfig cfg, EventBus events, Health health, Func<bool> sliding)
        {
            this.events = events;
            this.health = health;
            this.sliding = sliding;
            dt = cfg.Dt;
            p = P.From(cfg);
            fuseSteps = MovementMath.StepsFor(p.fuse, dt);
            killSteps = MovementMath.StepsFor(p.killRefill, dt);
            slideSteps = MovementMath.StepsFor(p.slideRefill, dt);
            slamSteps = MovementMath.StepsFor(p.slamRefill, dt);
            burnSteps = MovementMath.StepsFor(p.burnFuse, dt);
            tickSteps = MovementMath.StepsFor(p.tickEvery, dt);
            events.On<SpecialRoundEvent>(N20Events.SpecialRound, OnSpecial);
            events.On<KillEvent>("kill", OnKill);
        }

        /// <summary>A new run: the row on or off; no fuse.</summary>
        public void NewRun(bool row)
        {
            on = row;
            Running = false;
            Left = Full = Round = Burns = sinceTick = 0;
            LastHit = 0;
        }

        void OnSpecial(SpecialRoundEvent e)
        {
            if (!on || e.name != "twist:" + TwistId) return;
            if (e.phase == "start")
            {
                Running = true;
                Left = Full = fuseSteps;
                Round = e.round;
                Burns = 0;
                sinceTick = 0;
                Say(Tick, false, 0);
            }
            else if (e.phase == "end") Running = false;
        }

        void OnKill(KillEvent k)
        {
            if (!on || !Running || !(k.owner == null || k.owner == "player")) return;
            // a SLAM kill pays the most, then a SLIDE kill (the verbs the twist asks for), then any kill
            int add = k.weapon == "slam" ? slamSteps : sliding != null && sliding() ? slideSteps : killSteps;
            Left = Math.Min(Left + add, Full);
            sinceTick = 0;
            Say(Tick, true, 0, add * dt);
        }

        void Say(string name, bool refill, double hit, double paid = 0)
        {
            payload.paid = paid; // NIGHT22: what the kill paid (the HUD's separate "+N" pop); a picture only, no sim state
            payload.left = LeftSeconds;
            payload.full = Full * dt;
            payload.share = Share;
            payload.round = Round;
            payload.burns = Burns;
            payload.hit = hit;
            payload.refill = refill;
            events.Emit(name, payload);
        }

        /// <summary>One fixed step: the fuse burns down; at zero the heavy hit lands and a shorter fuse starts.</summary>
        public void Step()
        {
            if (!on || !Running) return;
            if (health.Dead) return;
            if (--Left <= 0)
            {
                // the heavy hit: hitFrac of max HP, never the run (it leaves 1 HP at the least; through takenMult)
                double want = health.max * p.hitFrac;
                double room = (health.hp - 1) / (health.takenMult > 0 ? health.takenMult : 1);
                double hit = Math.Max(0, Math.Min(want, room));
                double before = health.hp;
                if (hit > 0) health.Damage(hit, "deadline");
                LastHit = before - health.hp;
                Burns++;
                Left = Full = burnSteps;
                sinceTick = 0;
                Say(Burn, false, LastHit);
                return;
            }
            if (++sinceTick >= tickSteps)
            {
                sinceTick = 0;
                Say(Tick, false, 0);
            }
        }
    }
}
