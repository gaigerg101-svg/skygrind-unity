// EXT53 builder N (row N; design.n53.rideKill53, a sim row; GG w35 2026-10-12: "The rooms still don't incentivize wall-riding or creative use
// of the weapons"): RIDE KILL's pure rule. A player's kill is a ride kill when the player is on a wall ride at the kill, or rode one within
// graceSteps whole steps before it (RideKill53Clock: the last step the player rode; the step a ride ended counts as a riding step). It pays
// the rider back `charges` boosts: charges x the boost's cost of the bar (design.stamina's bar on) or charges x dash.pipCost pips (the web's
// pips), capped at the max (Refund). The sim (SurvivalSim.N53.RideKill.cs) keeps the clock, pays and emits `rideKill53`. No allocation.
// The web has no such rule or numbers (design.n53.rideKill53.numbers: graceS, charges).
namespace Skygrind.Core
{
    /// <summary>design.n53.rideKill53.numbers, read once a run (the grace in whole steps).</summary>
    public readonly struct RideKill53Params
    {
        public readonly int graceSteps;
        public readonly int charges;

        public RideKill53Params(int graceSteps, int charges)
        {
            this.graceSteps = graceSteps;
            this.charges = charges;
        }

        public const string Path = "design.n53.rideKill53.numbers.";

        /// <summary>The numbers at this sim step (graceS as whole steps: MovementMath.StepsFor).</summary>
        public static RideKill53Params From(GameConfig cfg, double step) =>
            new RideKill53Params(MovementMath.StepsFor(cfg.Num(Path + "graceS"), step), (int)cfg.Num(Path + "charges"));
    }

    /// <summary>The last step the player rode a wall this run (Reset at the run's start: rule 6).</summary>
    public struct RideKill53Clock
    {
        public bool rode;
        public int rideStep;

        public void Reset()
        {
            rode = false;
            rideStep = 0;
        }

        /// <summary>The player rode a wall during `step` (on the wall at the step's end, or the ride ended in it).</summary>
        public void Rode(int step)
        {
            if (rode && step < rideStep) return; // never back in time
            rode = true;
            rideStep = step;
        }

        /// <summary>A kill at `step` is a ride kill: on the wall now, or rode at most graceSteps whole steps before.</summary>
        public bool Counts(bool onWallNow, int step, int graceSteps) => onWallNow || (rode && step >= rideStep && step - rideStep <= graceSteps);

        /// <summary>Whole steps since the last riding step (0 on the wall; -1 when the player has not ridden this run).</summary>
        public int StepsSince(bool onWallNow, int step) => onWallNow ? 0 : rode ? step - rideStep : -1;
    }

    public static class RideKill53
    {
        /// <summary>The gameplay event (a new name: the web has no ride kill), payload RideKill53Event.</summary>
        public const string Event = "rideKill53";

        /// <summary>The resource after the refund: level + charges x unit, never over max (the bar's or the pips' units).</summary>
        public static double Refund(double level, double unit, int charges, double max) => JsMath.Min(max, level + unit * charges);
    }

    /// <summary>rideKill53's payload (reused by the sim: read it inside the handler).</summary>
    public sealed class RideKill53Event
    {
        public int enemy;
        public string kind, weapon;
        public bool onWall; // on the wall at the kill (false: inside the grace after it)
        public int stepsSinceRide; // 0 on the wall
        public bool bar; // the units: design.stamina's bar (true) or the web's pips
        public double before, after; // the resource before the refund (after the kill's own refill) and after it
        public int count; // ride kills this run, this one included
    }
}
