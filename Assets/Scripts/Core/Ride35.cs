// NIGHT35 builder M (design.n35.wallride35; NIGHT35.md M1, DECIDE35 D-6): the Lucio wall ride. Research/2026-10-11-wave35/research/lucio.md is
// the spec (each number's source is in its row's _why in Design/unity-config.json). The Mover reads it through Mover.flow34.ride35 (inside the
// NIGHT34 flow state, so the run snapshot saves it whole), neutral by default: with Params.Neutral (on = false: the golden vectors', the replays'
// and every row-off run's config) every Mover.cs branch that reads it is skipped, so the web's mover bit for bit (CLAUDE.md rule 3).
// The rules (Mover.cs branches marked "NIGHT35 wallride35"):
// - The HOLD ride: a ride entered with jump held has no 1.2 s timer (only a long safety cap, `safetyCap` s, logged as a "safety" end); it keeps
//   its height (rising entries brake at `riseGravityMult`, then a slow `sinkSpeed`); the entry gate is lowered to `entrySpeed` (along the wall)
//   and a head-on entry rides the way the camera faces. A ride entered WITHOUT jump held is the web's timed ride (press = the web's wall jump).
// - Release jump (OW Oct 2020): letting go of jump on a hold ride is the jump-off: the web's wall jump shape (along kept, `outSpeed` out) with
//   an up speed that follows the look pitch (`upLow` looking down at `pitchLowDeg`, `upHigh` looking up at `pitchHighDeg`; the wiki's 4.12 /
//   9.12 m/s) and an exit boost (`exitBoost` m/s added to the horizontal speed, never past `chainCap`), so chains build speed. It does not
//   spend the airtime's wall jumps (the re-ride rule is the limit).
// - Crouch on a ride drops off with no boost and no slam (OW Oct 2020); a release within the late window still jumps (the 0.2 s grace).
// - A speed buff while riding: the ride's floor is wallRide.minSpeed x (1 + `speedBuff`) (OW 2019: 30 %).
// - Corners and curved walls (OW May 2018; HOLD rides only: Lucio needs jump held to ride corners): after `cornerMinHold` s on the wall, an inside corner (a contact ahead) or an outside corner (the
//   wall ends; the next face is behind the ride's direction) up to `followMaxDeg` turns the ride onto the new face at the same speed; on any
//   wall the normal is re-sampled every step and a small `stick` speed into the wall keeps a pillar's curve. An outside corner is found by a
//   ray back along the ride from just past the old wall's plane, and taken once the capsule has cleared the edge; a probe that finds no wall
//   for up to `lostGrace` s (a facet's vertical edge: the collider's edge contacts are not walls) does not end the ride.
// - Attach: in the air with jump held, a wall within `attachReach` m beside the motion draws the player onto it (`attachPull` m/s into it):
//   "jump at a nearby wall and hold" rides on a near miss.
// - Re-ride (OW May 2018): a wall facing within `reRideDeg` of the last ridden one starts no ride until a landing or until the player is
//   `reRideDistance` m (horizontal) from where that ride ended.
// - Late jump: `lateJump` s after a ride ends on its own (away, lost, corner, blocked, timeout, safety, drop), a jump press, or the release of a
//   hold ride's jump, is still the jump-off from that wall.
// Whole steps (MovementMath.StepsFor at the config's simHz); no allocation; no randomness.
using System;

namespace Skygrind.Core
{
    public static class Ride35
    {
        /// <summary>design.n35.wallride35, read once (on = false when the row is off or absent).</summary>
        public struct Params
        {
            public bool on;
            /// <summary>NIGHT39 ride39 (With39 only; false in wallride35's own rules): the jump-off's whole horizontal speed (the along speed plus the
            /// outSpeed push, which the next wall's ride start turns into along speed) never passes chainCap unless the ride itself was faster
            /// (Boost39). Without it a chain grows past chainCap by the out push alone (n39ride chain: 10.4 -> 15.2 -> 19.1 -> 20.7 -> 24.9 m/s).</summary>
            public bool capJumpOff39;
            public double entrySpeed, speedBuff, exitBoost, chainCap, outSpeed, upLow, upHigh, pitchLowDeg, pitchHighDeg;
            public double riseGravityMult, sinkSpeed, safetyCap, cornerMinHold, followMaxDeg, reRideDeg, reRideDistance, lateJump, stick, lostGrace, attachReach, attachPull;
            /// <summary>The clocks in whole steps at the config's simHz.</summary>
            public int safetySteps, cornerMinSteps, lateSteps, lostGraceSteps;
            /// <summary>cos(followMaxDeg), cos(reRideDeg).</summary>
            public double cosFollowMax, cosReRide;

            /// <summary>Every rule off: the web's ride.</summary>
            public static Params Neutral => default;

            /// <summary>design.n35.wallride35's numbers, ON only when `rowOn` (the run's n35 wallride35 bit) and the row's numbers exist.</summary>
            public static Params From(GameConfig cfg, bool rowOn)
            {
                const string p = "design.n35.wallride35.";
                if (!rowOn || !cfg.Has(p + "entrySpeed")) return Neutral;
                double dt = 1 / cfg.movement.loop.simHz;
                var r = new Params
                {
                    on = true,
                    entrySpeed = cfg.Num(p + "entrySpeed"),
                    speedBuff = cfg.Num(p + "speedBuff"),
                    exitBoost = cfg.Num(p + "exitBoost"),
                    chainCap = cfg.Num(p + "chainCap"),
                    outSpeed = cfg.Num(p + "outSpeed"),
                    upLow = cfg.Num(p + "upLow"),
                    upHigh = cfg.Num(p + "upHigh"),
                    pitchLowDeg = cfg.Num(p + "pitchLowDeg"),
                    pitchHighDeg = cfg.Num(p + "pitchHighDeg"),
                    riseGravityMult = cfg.Num(p + "riseGravityMult"),
                    sinkSpeed = cfg.Num(p + "sinkSpeed"),
                    safetyCap = cfg.Num(p + "safetyCap"),
                    cornerMinHold = cfg.Num(p + "cornerMinHold"),
                    followMaxDeg = cfg.Num(p + "followMaxDeg"),
                    reRideDeg = cfg.Num(p + "reRideDeg"),
                    reRideDistance = cfg.Num(p + "reRideDistance"),
                    lateJump = cfg.Num(p + "lateJump"),
                    stick = cfg.Num(p + "stick"),
                    lostGrace = cfg.Num(p + "lostGrace"),
                    attachReach = cfg.Has(p + "attachReach") ? cfg.Num(p + "attachReach") : 0,
                    attachPull = cfg.Has(p + "attachPull") ? cfg.Num(p + "attachPull") : 0,
                };
                r.safetySteps = MovementMath.StepsFor(r.safetyCap, dt);
                r.cornerMinSteps = MovementMath.StepsFor(r.cornerMinHold, dt);
                r.lateSteps = MovementMath.StepsFor(r.lateJump, dt);
                r.lostGraceSteps = MovementMath.StepsFor(r.lostGrace, dt);
                r.cosFollowMax = JsMath.Cos(r.followMaxDeg * Math.PI / 180);
                r.cosReRide = JsMath.Cos(r.reRideDeg * Math.PI / 180);
                return r;
            }

            /// <summary>NIGHT39 ride39 (builder R1; design.n39.ride39; GG 2026-10-13: "you gain speed too quickly ... the jump height should be
            /// increased slightly"): these rules with the row's speedBuff / exitBoost / chainCap / upLow / upHigh in place of wallride35's (each only
            /// where the row has it). The Mover's wallride35 branches read the result as they read wallride35's own numbers, so no Mover line
            /// changes; design.n35.wallride35 itself is untouched (the row off, or --off39: NIGHT38's ride bit for bit). Neutral stays neutral.</summary>
            public Params With39(GameConfig cfg)
            {
                const string q = "design.n39.ride39.";
                Params r = this;
                if (!r.on) return r;
                if (cfg.Has(q + "speedBuff")) r.speedBuff = cfg.Num(q + "speedBuff");
                if (cfg.Has(q + "exitBoost")) r.exitBoost = cfg.Num(q + "exitBoost");
                if (cfg.Has(q + "chainCap")) r.chainCap = cfg.Num(q + "chainCap");
                if (cfg.Has(q + "upLow")) r.upLow = cfg.Num(q + "upLow");
                if (cfg.Has(q + "upHigh")) r.upHigh = cfg.Num(q + "upHigh");
                r.capJumpOff39 = cfg.Design("n39.ride39.capJumpOff");
                return r;
            }
        }

        /// <summary>The ride's state (inside Mover.flow34, saved with it): the rules, this ride's hold, the late window, the re-ride memory, counts.</summary>
        public struct State
        {
            public Params rules;
            /// <summary>This ride was entered with jump held (the hold ride: no timer, the release jumps).</summary>
            public bool held;
            /// <summary>Steps on this ride (the corner rule's minimum hold).</summary>
            public int rideAge;
            /// <summary>Steps in a row the wall probe found nothing (a facet's edge, an outside corner being cleared): lostGrace steps before "lost".</summary>
            public int lostSteps;
            /// <summary>The late window: steps left, the ended ride's wall normal, and whether a release still jumps (a hold ride's jump still held).</summary>
            public int lateLeft;
            public Vec3 lateNormal;
            public bool lateHeld;
            /// <summary>The re-ride memory: the last ride's wall normal and where it ended (cleared by a landing).</summary>
            public bool hasLast;
            public Vec3 lastNormal, lastEnd;
            /// <summary>The run's counts (the harness and the reel read them).</summary>
            public int rides, holdRides, jumpOffs, lateJumps, drops, corners, safetyEnds, reRideBlocks, attaches;
            /// <summary>The last jump-off's up speed and horizontal speed (m/s).</summary>
            public double lastUp, lastSpeed;
            /// <summary>The ride's own speed along the wall this step (the corner turn keeps it).</summary>
            public double rideSpeed;
            /// <summary>NIGHT36 wallgrip36 (Core/Grip36.cs): the grip rules (Mover.SetGrip36; kept by SetRide35), the steps since this ride's attach, and the
            /// run's count of attaches whose rise was clamped.</summary>
            public Grip36.Params grip;
            public int gripAge, gripClamps;
            /// <summary>NIGHT38 ride38 (Core/Ride38.cs): the hold ride's rules (Mover.SetRide38; kept by SetRide35), whether this hold ride is still
            /// braking a rising entry, the run's steps a rise was cut at a wall's top, the run's entries / turns refused by a noride wall.</summary>
            public Ride38.Params r38;
            public bool r38Rising;
            public int r38TopHolds, r38NoRideBlocks;
            /// <summary>NIGHT38 noride (Core/Ride38.cs): the level's noride boxes (Mover.SetNoRide38; kept by SetRide35; null: none).</summary>
            public Ride38.Box[] noRide;
            /// <summary>NIGHT40 ride40 (Core/Ride40.cs): every ride holds (Mover.SetRide40; kept by SetRide35). With it on, `r38` holds
            /// design.n40.ride40's hold rule and a tap ride takes it as a hold ride does.</summary>
            public Ride40.Params r40;
            /// <summary>NIGHT40 ride40's attach: the same-wall lockout (steps left, the ended ride's wall normal) and the run's refused attach
            /// checks by rule (a step each: a wall touched for 10 steps under 1 m of clearance counts 10).</summary>
            public int r40LockLeft;
            public Vec3 r40LockNormal;
            public int r40Low, r40Slow, r40NoIntent, r40Short, r40Locked;
        }

        /// <summary>The jump-off's up speed for a look pitch (rad, + is up): upLow at pitchLowDeg, upHigh at pitchHighDeg, linear and clamped between.</summary>
        public static double JumpUp(double pitch, in Params p)
        {
            double deg = pitch * 180 / Math.PI;
            double span = p.pitchHighDeg - p.pitchLowDeg;
            double t = span > 1e-9 ? (deg - p.pitchLowDeg) / span : 1;
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return p.upLow + (p.upHigh - p.upLow) * t;
        }

        /// <summary>The horizontal velocity (vx, vz) with the exit boost added along its direction, never past chainCap (a faster entry keeps its own speed).</summary>
        public static Vec3 Boost(Vec3 v, in Params p)
        {
            double h = JsMath.Hypot(v.x, v.z);
            if (h < 1e-9) return v;
            double target = h + p.exitBoost;
            if (target > p.chainCap) target = JsMath.Max(h, p.chainCap);
            double k = target / h;
            return new Vec3(v.x * k, v.y, v.z * k);
        }

        /// <summary>NIGHT39 ride39 (rules.capJumpOff39): Boost, then the whole horizontal speed held to the larger of chainCap and the ride's own
        /// speed along the wall (`along`: a ride already faster than the cap keeps its speed, never gains from the push). A chain then climbs to
        /// chainCap and stays there.</summary>
        public static Vec3 Boost39(Vec3 v, in Params p, double along)
        {
            Vec3 b = Boost(v, p);
            double h = JsMath.Hypot(b.x, b.z);
            double top = JsMath.Max(p.chainCap, along);
            if (h <= top || h < 1e-9) return b;
            double k = top / h;
            return new Vec3(b.x * k, b.y, b.z * k);
        }

        /// <summary>The unit direction along a wall (horizontal normal n) a ride continues in after turning onto it: the wall's tangent whose sign
        /// follows `toward` (the old direction, plus the old normal's pull: + for an inside corner, - for an outside one). Zero when n is not horizontal.</summary>
        public static Vec3 Tangent(Vec3 n, Vec3 toward)
        {
            double tx = -n.z, tz = n.x;
            double l = JsMath.Hypot(tx, tz);
            if (l < 1e-9) return Vec3.Zero;
            tx /= l;
            tz /= l;
            if (tx * toward.x + tz * toward.z < 0)
            {
                tx = -tx;
                tz = -tz;
            }
            return new Vec3(tx, 0, tz);
        }
    }
}
