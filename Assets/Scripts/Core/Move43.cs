// NIGHT43 builder M1 (design.n43.chimney43 / design.n43.ride43, N43Blocks bits 0 / 1; GG 2026-10-16, G2 / G3 / G13 in
// Research/2026-10-16-wave43/GG-REQUEST.md; DECIDE43 D-1 / D-2; CODE-FACTS-A A1-A3; research/movement-web.md; notes in
// Research/2026-10-16-wave43/M1.md). Core/Mover.cs is named for M1 tonight (NIGHT43.md method 5): every branch that reads these rules is
// marked "NIGHT43 chimney43" / "NIGHT43 ride43" and is skipped with Params.Neutral (both rows off, --off43, a tape of format 25 or older):
// NIGHT42's Mover bit for bit.
// chimney43 (G13 / G2): today a pressed wall jump spends one of 3 per airtime and only a landing refills them (A2), so a chimney or THE STACK's
// six-panel sections run out and the press silently becomes the double jump. With the row on:
// - a wall jump off a DIFFERENT wall than the last wall jump / jump-off (normal dot < newWallDot, or more than newWallDist m along the ground
//   from where that jump left) refills the count first; the same wall still spends (no endless climb on one face);
// - a ride start on a new wall refills it too (rideRefills);
// - flow34's wall buffer holds a press for a wall ahead even with the count empty (the wall ahead is usually a new one; a miss is still the
//   double jump on the buffer's last step, as before).
// ride43 (G3): the ride smooth and forgiving, each sub-rule its own number (design.n43.ride43):
// (a) the attach: the near-miss draw also on a tap ride (tapDraw), and swept: rays ahead-diagonal of the motion reach attachReach m past the
//     capsule plus this step's move (the stay probe stays wallRide.probeDistance 0.12 m);
// (b) the snap: wallgrip36's rise clamp (1.5 m/s, a hard stop) eased: the attach keeps keepRise x the rise (never less than the clamp), then
//     the rising entry brakes by gravity as before;
// (c) lostGrace s (was wallride35's 0.05) before a probe that finds no wall ends the ride;
// (d) B (slide) in the air within slamGrace s of leaving a wall is not a slam (a slam sets (0, -48, 0) and kills every m/s): it is a drop that
//     keeps the horizontal speed (a rise is cut to 0); a slam only after slamGrace s, or looking down more than slamLookDeg; B on a ride stays
//     wallride35's drop;
// (e) ride40's intent with jump held needs no stick into the wall: moving along it (not away from it) is enough;
// (f) ride40's lockout (0.2 s) only for the same wall: a parallel wall more than lockPlane m off the ended ride's plane is not locked;
// (g) a dash that meets a ridable wall in the air (heading into it at least dashIntoDot) ends and attaches (DOOM Eternal's climb walls), along
//     the wall at the dash's exit speed;
// (h) a press on a ride is always a push off the wall: with the wall-jump count empty it is the jump-off (wallride35's, no spend), never the
//     double jump straight up with no push (the lead's note: GG's pad shows the bottom face button as B = JUMP);
// (i) flow34's wall buffer (a press held up to the jump buffer for a wall ahead) also for a shallow approach, from bufferDeg off the wall line
//     (flow34 asked for steeper than 30 deg: a press 0.05 s before a 30 deg approach was the double jump: n43move jumpseq).
// Whole steps (MovementMath.StepsFor); no allocation; no randomness.
using System;

namespace Skygrind.Core
{
    public static class Move43
    {
        public struct Params
        {
            /// <summary>The rows (chimney43, ride43).</summary>
            public bool chimney, ride;
            // chimney43
            public double newWallDot, newWallDist;
            public bool rideRefills;
            /// <summary>Round 2, the chain: a pressed wall jump off a new wall within chainWindow s (chainSteps) of leaving the last wall: chainUp up, chainOut out (0: off).</summary>
            public double chainWindow, chainUp, chainOut;
            public int chainSteps;
            // ride43
            public bool tapDraw, holdNoStick, dashAttach, pressPushes;
            public double attachReach, keepRise, lostGrace, slamGrace, slamLookDeg, lockPlane, dashIntoDot, bufferDeg;
            /// <summary>Round 2 (a)': the sideways near-miss draw's reach (m past the capsule) while jump is held (0: ride40's attachReach).</summary>
            public double holdReach;
            /// <summary>sin(bufferDeg): (i) flow34's wall-ahead test accepts an approach this steep or steeper (dot of the motion with -normal).</summary>
            public double sinBuffer;
            public int lostGraceSteps, slamGraceSteps;
            /// <summary>sin(slamLookDeg): a look pitch below -this (sin) is "looking down" (a slam after all).</summary>
            public double sinSlamLook;

            /// <summary>Round 2, ONE "same wall" (design.n43.ride43.sameWall*; read when chimney43 or ride43 is on): facing within sameDot, the
            /// plane within samePlane m, along the ground within sameAlong m, and neither sameRise m higher nor sameDrop m lower than where the
            /// player left it. chimney43's new wall = not the same wall; ride43 uses it for the re-ride rule, ride40's lockout, the ride lock and
            /// the wall-jump lock.</summary>
            public double sameDot, samePlane, sameAlong, sameRise, sameDrop;

            /// <summary>Round 2, boost43's Mover rule (design.n43.boost43.doubleKeeps): a double jump never lowers a rise (vy = max(vy, the double's)).</summary>
            public bool boost;

            public bool Any => chimney || ride;

            /// <summary>Both rows off: NIGHT42's Mover.</summary>
            public static Params Neutral => default;

            /// <summary>The rows' numbers (design.n43.chimney43 / design.n43.ride43 / design.n43.boost43), each only when its row is on this run.</summary>
            public static Params From(GameConfig cfg, bool chimneyOn, bool rideOn, bool boostOn = false)
            {
                var p = new Params();
                double dt = 1 / cfg.movement.loop.simHz;
                p.boost = boostOn && cfg.Design("n43.boost43.doubleKeeps");
                if (chimneyOn || rideOn)
                {
                    const string w = "design.n43.ride43.";
                    p.sameDot = JsMath.Cos(cfg.Num(w + "sameWallDeg") * Math.PI / 180);
                    p.samePlane = cfg.Num(w + "sameWallPlane");
                    p.sameAlong = cfg.Num(w + "sameWallAlong");
                    p.sameRise = cfg.Num(w + "sameWallRise");
                    p.sameDrop = cfg.Num(w + "sameWallDrop");
                }
                if (chimneyOn)
                {
                    const string c = "design.n43.chimney43.";
                    p.chimney = true;
                    p.newWallDot = cfg.Num(c + "newWallDot");
                    p.newWallDist = cfg.Num(c + "newWallDist");
                    p.rideRefills = cfg.Design("n43.chimney43.rideRefills");
                    p.chainWindow = cfg.Has(c + "chainWindow") ? cfg.Num(c + "chainWindow") : 0;
                    p.chainUp = cfg.Has(c + "chainUp") ? cfg.Num(c + "chainUp") : 0;
                    p.chainOut = cfg.Has(c + "chainOut") ? cfg.Num(c + "chainOut") : 0;
                    p.chainSteps = MovementMath.StepsFor(p.chainWindow, dt);
                }
                if (rideOn)
                {
                    const string r = "design.n43.ride43.";
                    p.ride = true;
                    p.tapDraw = cfg.Design("n43.ride43.tapDraw");
                    p.holdNoStick = cfg.Design("n43.ride43.holdNoStick");
                    p.dashAttach = cfg.Design("n43.ride43.dashAttach");
                    p.pressPushes = cfg.Design("n43.ride43.pressPushes");
                    p.attachReach = cfg.Num(r + "attachReach");
                    p.keepRise = cfg.Num(r + "keepRise");
                    p.lostGrace = cfg.Num(r + "lostGrace");
                    p.slamGrace = cfg.Num(r + "slamGrace");
                    p.slamLookDeg = cfg.Num(r + "slamLookDeg");
                    p.lockPlane = cfg.Num(r + "lockPlane");
                    p.dashIntoDot = cfg.Num(r + "dashIntoDot");
                    p.bufferDeg = cfg.Num(r + "bufferDeg");
                    p.holdReach = cfg.Has(r + "holdReach") ? cfg.Num(r + "holdReach") : 0;
                    p.sinBuffer = JsMath.Sin(p.bufferDeg * Math.PI / 180);
                    p.lostGraceSteps = MovementMath.StepsFor(p.lostGrace, dt);
                    p.slamGraceSteps = MovementMath.StepsFor(p.slamGrace, dt);
                    p.sinSlamLook = JsMath.Sin(p.slamLookDeg * Math.PI / 180);
                }
                return p;
            }
        }

        /// <summary>The Mover's NIGHT43 state (Mover.m43, saved whole by RunSnapshot.MoverState): the rules, the last wall jump's wall, the
        /// clock since the last ride ended, and the run's counts (the harness and the reel read them).</summary>
        public struct State
        {
            public Params p;
            /// <summary>chimney43: the last wall jump / jump-off this airtime: its wall's normal and where it left (cleared by a landing).</summary>
            public bool hasWall;
            public Vec3 wallN, wallAt;
            /// <summary>ride43 (d): steps since the last ride ended (large: none this airtime).</summary>
            public int sinceRide;
            /// <summary>chimney43's counts: refills (the count was short and a new wall filled it), same-wall spends.</summary>
            public int refills, rideRefills, sameWall, chains;
            /// <summary>ride43's counts: tap draws, swept draws, eased snaps, grace saves (a lost probe that came back), B drops (no slam),
            /// lockouts skipped for another plane, dash attaches, presses that pushed off instead of the double jump, holds without a stick.</summary>
            public int tapDraws, sweptDraws, eased, graceSaves, bDrops, planeSkips, dashAttaches, pressPushes, noStickHolds, shallowHolds, topCuts;
        }

        /// <summary>chimney43: is the wall with normal n, at feet position pos, a different wall than the last wall jump's?</summary>
        public static bool NewWall(in State s, Vec3 n, Vec3 pos) => !s.hasWall || !SameWall(s.p, s.wallN, s.wallAt, n, pos);

        /// <summary>Round 2, the one "same wall" test: is the wall with normal nB, met at feet position atB, the wall with normal nA the player left at
        /// feet position atA? Facing within sameDot; the plane offset (along nA) within samePlane; along the ground within sameAlong; not sameRise
        /// or more higher (a wall that far above is a new wall: THE STACK's panel four slots up) and not sameDrop or more lower (falling back down the
        /// wall you left catches you).</summary>
        public static bool SameWall(in Params p, Vec3 nA, Vec3 atA, Vec3 nB, Vec3 atB)
        {
            if (nA.x * nB.x + nA.z * nB.z < p.sameDot) return false;
            double dx = atB.x - atA.x, dz = atB.z - atA.z;
            double off = dx * nA.x + dz * nA.z;
            if (Math.Abs(off) > p.samePlane) return false;
            double along = Math.Sqrt(Math.Max(0, dx * dx + dz * dz - off * off));
            if (along > p.sameAlong) return false;
            double dy = atB.y - atA.y;
            return dy < p.sameRise && -dy < p.sameDrop;
        }

        /// <summary>ride43 (f): is a wall with normal n, at feet position pos, the same plane as the ended ride's (normal lockN, ended at lockAt)?
        /// Its offset along the normal within lockPlane m.</summary>
        public static bool SamePlane(Vec3 lockN, Vec3 lockAt, Vec3 pos, double lockPlane)
        {
            double off = (pos.x - lockAt.x) * lockN.x + (pos.z - lockAt.z) * lockN.z;
            return Math.Abs(off) <= lockPlane;
        }
    }
}
