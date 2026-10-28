// EXT53 builder N (SurvivalSim.N53.cs's N hooks): the sim row rideKill53 (RIDE KILL; Core/RideKill53.cs). A player's kill on a wall ride, or
// within design.n53.rideKill53.numbers.graceS (whole steps) after one, pays back numbers.charges boosts (the bar: x design.stamina.boost.cost;
// the web's pips: x dash.pipCost; capped at the max), after the kill's own refill (SurvivalSim.Stamina.cs's kill handler runs first: it was
// subscribed at Init), and emits `rideKill53` (RideKill53Event) for the HUD's RIDE KILL callout (UiRideKill53.cs) and GG's sounds.
// The ride clock: the Mover's WallRideEnd (not a Respawn's "reset") marks the step the ride ended; this step hook marks every step that ends
// on the wall; a kill reads the Mover's state at the kill itself. The Mover is not changed (rule 3): the refund writes the bar / the pips as
// spire38's meter does (SurvivalSim.N38.Spire.cs). Off (the row off, -skygrindOff53, or a config without the row): no handler runs a line
// past its first check, no field the run reads is written: NIGHT53's game bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The run's rideKill53 state (the harness, the reel, the HUD): the row was on at this run's start.</summary>
        public bool RideKill53On { get; private set; }

        /// <summary>The numbers this run read (graceSteps, charges); default with the row off.</summary>
        public RideKill53Params rideKill53Params;

        /// <summary>The last step the player rode a wall this run (public for the tests and the reel; reset at StartRun).</summary>
        public RideKill53Clock rideKill53Clock;

        /// <summary>Counters for the harness and the reel (this run): ride kills paid, the resource paid back, player kills that did not count.</summary>
        public int RideKill53Count, RideKill53Missed;
        public double RideKill53Refunded;

        readonly RideKill53Event rk53Event = new RideKill53Event();
        bool rk53Hooked;

        partial void N53RideKillStartRun()
        {
            RideKill53On = idRideKill53 >= 0 && rows.On(idRideKill53);
            rideKill53Clock.Reset();
            RideKill53Count = RideKill53Missed = 0;
            RideKill53Refunded = 0;
            if (!RideKill53On) return;
            rideKill53Params = RideKill53Params.From(cfg, step);
            if (rk53Hooked) return;
            rk53Hooked = true; // once a composition (the bus and the Mover live as long as the sim); the handlers read the run's flag
            events.On("kill", RideKill53OnKill);
            mover.Events += RideKill53OnMover;
        }

        partial void N53RideKillStep(double dt)
        {
            if (RideKill53On && mover.state == MoveState.WallRide) rideKill53Clock.Rode(simStep);
        }

        void RideKill53OnMover(MoverEvent e)
        {
            if (RideKill53On && e.type == MoverEventType.WallRideEnd && e.reason != "reset") rideKill53Clock.Rode(simStep);
        }

        void RideKill53OnKill(object payload)
        {
            if (!RideKill53On || !(payload is KillEvent k) || k.owner != "player" || health.Dead) return;
            bool onWall = mover.state == MoveState.WallRide;
            if (!rideKill53Clock.Counts(onWall, simStep, rideKill53Params.graceSteps))
            {
                RideKill53Missed++;
                return;
            }
            bool bar = mover.stamina.rules.Any;
            double before = bar ? mover.stamina.bar : mover.pips;
            double unit = bar ? mover.stamina.rules.cost : cfg.movement.dash.pipCost;
            double max = bar ? mover.StaminaMax : mover.MaxPips;
            double after = RideKill53.Refund(before, unit * Rank56RefundScale(), rideKill53Params.charges, max); // NIGHT56 G1 rank56 (x1 off)
            if (bar) mover.stamina.bar = after;
            else mover.pips = after;
            RideKill53Count++;
            RideKill53Refunded += after - before;
            RideKill53Event ev = rk53Event;
            ev.enemy = k.enemy;
            ev.kind = k.kind;
            ev.weapon = k.weapon;
            ev.onWall = onWall;
            ev.stepsSinceRide = rideKill53Clock.StepsSince(onWall, simStep);
            ev.bar = bar;
            ev.before = before;
            ev.after = after;
            ev.count = RideKill53Count;
            events.Emit(RideKill53.Event, ev);
        }
    }
}
