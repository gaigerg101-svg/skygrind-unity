// NIGHT22 leftover 1 (builder M): DIRECTED STEPS for THE BEACON and THE HUNT (design.n22.directed). The pure half: a small state
// machine that turns the run's bus events (the web-style names the two rows already emit) into a step list with a current step and,
// for the beacon, a target to walk to. RENDER SIDE ONLY: it reads events and the player's position and never writes the sim, so the
// row changes no fingerprint and no tape (N22Blocks.SimChanging stays false for it; N22OnPath stays false).
//   THE BEACON  `beaconStand` {round, position}      step 0 GO TO THE BEACON (target = its spot)
//               the player within perks.reach of it   step 1 HOLD BUY TO LIGHT IT
//               `beaconTake`                          step 2 SURVIVE THE ELITE ROUND (no target)
//               `roundClear`, a new run               the cue ends; `roundBegin` ends a beacon nobody took
//   THE HUNT    `specialRound` hunt "announce"        step 0 THE HUNT COMES (the break's countdown)
//               "start"                               step 1 SQUADS CLOSE IN (from the ring: design.adds.hunt ringMin..ringMax)
//               "last"                                step 2 FINISH THE LAST ONE;  "end" / `roundClear`: the cue ends
// The hunt has no place to walk to (its ring is round the player: Director.huntRingOn), so its cue is a step list with no marker: the
// cue never invents a location. Directed, not alarming: the view draws it in the player-usable cyan, never red (rule 8).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum Directed22Kind
    {
        None,
        Beacon,
        Hunt,
    }

    /// <summary>Where a target lies relative to the way the player faces.</summary>
    public enum Directed22Side
    {
        Ahead,
        Right,
        Left,
        Behind,
    }

    public sealed class Directed22
    {
        public const int StepCount = 3;

        readonly double reach;
        Directed22Kind kind;
        int round, step;
        bool hasTarget;
        Vec3 target;

        /// <summary>`reach`: perks.reach (how close the BUY hold works, m).</summary>
        public Directed22(GameConfig cfg)
        {
            reach = cfg.Num("perks.reach");
        }

        public Directed22Kind Kind => kind;

        public int Round => round;

        /// <summary>The current step, 0 .. StepCount - 1 (0 while there is no cue).</summary>
        public int Step => step;

        public bool HasTarget => hasTarget;

        public Vec3 Target => target;

        public bool Active => kind != Directed22Kind.None;

        public void Reset()
        {
            kind = Directed22Kind.None;
            round = step = 0;
            hasTarget = false;
        }

        /// <summary>`beaconStand`: the beacon for `forRound` stands at `at`.</summary>
        public void BeaconStood(int forRound, Vec3 at)
        {
            kind = Directed22Kind.Beacon;
            round = forRound;
            step = 0;
            hasTarget = true;
            target = at;
        }

        /// <summary>`beaconTake`: it is lit; the next round brings the elite.</summary>
        public void BeaconTaken()
        {
            if (kind != Directed22Kind.Beacon) return;
            step = 2;
            hasTarget = false;
        }

        /// <summary>`roundBegin`: a beacon nobody took leaves the list; a lit one stays until its round clears.</summary>
        public void RoundBegan()
        {
            if (kind == Directed22Kind.Beacon && step < 2) Reset();
        }

        public void RoundCleared() => Reset();

        /// <summary>`specialRound` {name, round, phase}: the hunt's beats. Other names (boss, elite, twists) are not directed.</summary>
        public void Special(string name, int forRound, string phase)
        {
            if (name != "hunt") return;
            switch (phase)
            {
                case "announce":
                    kind = Directed22Kind.Hunt;
                    round = forRound;
                    step = 0;
                    hasTarget = false;
                    break;
                case "start":
                    if (kind == Directed22Kind.Hunt) step = 1;
                    break;
                case "last":
                    if (kind == Directed22Kind.Hunt) step = 2;
                    break;
                case "end":
                    Reset();
                    break;
            }
        }

        /// <summary>Once a frame: a beacon cue moves from "go" to "hold BUY" when the player is within reach of it (flat distance and height).</summary>
        public void Update(Vec3 player)
        {
            if (kind != Directed22Kind.Beacon || !hasTarget) return;
            bool near = Flat(player, target) <= reach && Math.Abs(player.y - target.y) < 3;
            if (step == 0 && near) step = 1;
            else if (step == 1 && !near) step = 0;
        }

        /// <summary>Flat distance (m) from the player to the target, 0 without one.</summary>
        public double Distance(Vec3 player) => hasTarget ? Flat(player, target) : 0;

        static double Flat(Vec3 a, Vec3 b)
        {
            double dx = a.x - b.x, dz = a.z - b.z;
            return Math.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>The signed angle (degrees, + to the right) from the way the player faces to `to` (yaw 0 faces -z).</summary>
        public static double BearingDeg(Vec3 player, double yaw, Vec3 to)
        {
            double dx = to.x - player.x, dz = to.z - player.z;
            double fx = -JsMath.Sin(yaw), fz = -JsMath.Cos(yaw), rx = JsMath.Cos(yaw), rz = -JsMath.Sin(yaw);
            return JsMath.Atan2(dx * rx + dz * rz, dx * fx + dz * fz) * 180 / Math.PI;
        }

        public static Directed22Side SideOf(double bearingDeg)
        {
            double a = Math.Abs(bearingDeg);
            if (a <= 30) return Directed22Side.Ahead;
            if (a >= 135) return Directed22Side.Behind;
            return bearingDeg > 0 ? Directed22Side.Right : Directed22Side.Left;
        }
    }
}
