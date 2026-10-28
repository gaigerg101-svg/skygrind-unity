// NIGHT42 builder B1 (brawl42; NIGHT42.md card B1; GG 2026-10-14: "level 2 boss might needs more attacks and needs to be more difficult";
// DECIDE42 D-5): THE ICEBREAKER's two new attacks and a harder phase 2, behind N42Blocks.brawl42 (the sim hands the flag and the numbers
// in with Arm42 when a fight begins; off: on42 false, nothing here runs and Brawl40 is NIGHT41's bit for bit). Numbers: design.n42.brawl42.
// Never more HP (GG w31-32): the pool, the stagger meter and the damage rule are NIGHT40's.
//   SWEEP  (RED tell36 at the right fist, >= 0.9 s; up close, instead of a haymaker at sweep.chance, never twice running): it cocks the arm low
//          and back, then spins once with the arm at shin height, starting on the far side from you (it reaches you half way round). Answer:
//          JUMP as the arm comes (feet above sweep.height), or dash out of its reach. Then it is dizzy (sweep.recover, a slow turn): the punish.
//          The read up close (Sekiro's grammar, research/boss-web.md): a high cocked fist = dash out of the cone; a low arm = jump.
//   LEAP   (RED tell36 at the head, >= 0.9 s; past leap.startDist at leap.chance, never twice running): a landing circle tracks you for
//          leap.trackShare of the wind-up, then LOCKS; it jumps (leap.air, leap.apex up) and lands on it: a hit inside leap.radius (a hop does
//          not clear leap.hitHeight), then its fists are STUCK in the ice (leap.stuckSeconds; every hit x stagger.stuckMult). Answer: dash or run
//          out of the locked circle. Phase 2: the landing also sends the slam's ring (jump it).
//   PHASE 2 rests rest2 (NIGHT40 0.8 s); a haymaker up close that did not chain into the slam chains into a sweep at sweep.chain2.
// Events (names for GG's sounds, no audio): brawl42Sweep ("tell" | "swing" | "hit" | "recover"), brawl42Leap ("tell" | "lock" | "jump" |
// "land" | "hit" | "ring" | "stuck" | "free"); the payload is Brawl40Event (attack "brawlSweep" / "brawlLeap").
using System;

namespace Skygrind.Core
{
    public sealed class Brawl42Params
    {
        public double rest2;
        public double sweepWindup, sweepWindup2, sweepActive, sweepRecover, sweepReach, sweepStart, sweepHeight, sweepArmDeg, sweepDamage, sweepKnock, sweepKnockUp, sweepChance, sweepChain2, sweepSpins2;
        public double leapWindup, leapTrack, leapAir, leapApex, leapStart, leapChance, leapRadius, leapHitHeight, leapDamage, leapKnock, leapKnockUp, leapStuck, leapRecover, leapRing2;

        public const string Row = "design.n42.brawl42.";

        public static Brawl42Params From(GameConfig c)
        {
            double N(string k) => c.Num(Row + k);
            return new Brawl42Params
            {
                rest2 = N("rest2"),
                sweepWindup = N("sweep.windup"), sweepWindup2 = N("sweep.windup2"), sweepActive = N("sweep.active"), sweepRecover = N("sweep.recover"),
                sweepReach = N("sweep.reach"), sweepStart = N("sweep.startDist"), sweepHeight = N("sweep.height"), sweepArmDeg = N("sweep.armDeg"),
                sweepDamage = N("sweep.damage"), sweepKnock = N("sweep.knock"), sweepKnockUp = N("sweep.knockUp"), sweepChance = N("sweep.chance"),
                sweepChain2 = N("sweep.chain2"), sweepSpins2 = N("sweep.spins2"),
                leapWindup = N("leap.windup"), leapTrack = N("leap.trackShare"), leapAir = N("leap.air"), leapApex = N("leap.apex"), leapStart = N("leap.startDist"),
                leapChance = N("leap.chance"), leapRadius = N("leap.radius"), leapHitHeight = N("leap.hitHeight"), leapDamage = N("leap.damage"),
                leapKnock = N("leap.knock"), leapKnockUp = N("leap.knockUp"), leapStuck = N("leap.stuckSeconds"), leapRecover = N("leap.recover"),
                leapRing2 = N("leap.ring2"),
            };
        }
    }

    public sealed partial class Brawl40
    {
        public const string Sweep = "brawlSweep", Leap = "brawlLeap";

        /// <summary>brawl42 on for this fight (the sim's Arm42); false: NIGHT41's fight.</summary>
        public bool on42;
        public Brawl42Params p42;
        /// <summary>The leap's height above the ice now (0 on the ground; the body's hit spheres and the view ride it).</summary>
        public double air42;
        /// <summary>The sweep's arm angle now (yaw radians, the Face convention: atan2(dx, dz)).</summary>
        public double sweepArm;
        /// <summary>The leap's landing spot (world, on the floor) and whether it has locked.</summary>
        public Vec3 leapTo, leapFrom;
        public bool leapLocked;
        double sweepArm0;

        // stats (the harness, the reels)
        public int sweeps, sweepHits, sweepChains, leaps, leapHits, leapRings;
        /// <summary>Health it took from the player this fight (every attack; a stat only).</summary>
        public double hurt42;

        /// <summary>The sim's switch at a fight's begin: brawl42 on with its numbers, or off (NIGHT41's fight).</summary>
        public void Arm42(bool on, Brawl42Params prm)
        {
            p42 = prm;
            on42 = on && prm != null;
        }

        /// <summary>Its body's base: the feet, lifted by the leap (exactly body.position on the ground).</summary>
        Vec3 Base => air42 == 0 ? body.position : body.position + new Vec3(0, air42, 0);

        double RestNow => phase == 1 ? p.restSeconds : on42 ? p42.rest2 : p.restSeconds2;

        void Reset42()
        {
            sweeps = sweepHits = sweepChains = leaps = leapHits = leapRings = 0;
            hurt42 = 0;
            air42 = 0;
            leapLocked = false;
        }

        /// <summary>The harness's line: the player's health lost, and the new attacks' counts with the row on.</summary>
        public string Stats42() => $"; hurt {hurt42:0}" + (on42 ? $"; sweeps {sweeps} (hit {sweepHits}, chained {sweepChains}), leaps {leaps} (hit {leapHits}, rings {leapRings})" : "");

        /// <summary>brawl42's picks (Pick asks first, after the camp answer): up close the sweep instead of the haymaker at sweep.chance (never
        /// twice running); past leap.startDist the leap at leap.chance (never twice running). False: NIGHT40's pick goes on.</summary>
        bool Pick42(double dist)
        {
            if (dist <= p.hayStart)
            {
                if (previous != Sweep && dist <= p42.sweepStart && rng.Next() < p42.sweepChance) { StartAttack(Sweep, false); return true; }
                return false;
            }
            if (dist > p42.leapStart && previous != Leap && rng.Next() < p42.leapChance) { StartAttack(Leap, false); return true; }
            return false;
        }

        /// <summary>The fists in the new attacks (f: the fist's shoulder-side base).</summary>
        Vec3 Fist42(Vec3 f, bool right)
        {
            if (attack == Sweep)
            {
                if (state == Brawl40State.Tell) return f + Side * 1.1 - facing * 1.0 + new Vec3(0, 0.4, 0); // cocked out to the side and back, low by the ice (n42-r1: the arm must read low)
                if (state == Brawl40State.Active)
                    return Base + new Vec3(JsMath.Sin(sweepArm), 0, JsMath.Cos(sweepArm)) * (p.radius + p42.sweepReach * 0.55) + new Vec3(0, 0.45, 0);
                if (state == Brawl40State.Recover) return f + facing * 0.6 + new Vec3(0, 0.6, 0);
                return f + facing * 0.4 + new Vec3(0, p.height * 0.35, 0);
            }
            // the leap
            if (state == Brawl40State.Tell) return f - facing * 0.3 + new Vec3(0, p.height * 0.22, 0);
            if (state == Brawl40State.Active) return f + facing * 0.2 + new Vec3(0, p.height * 1.0, 0);
            if (state == Brawl40State.Stuck) return f + facing * (p.radius * 0.7) + new Vec3(0, p.fistRadius * 0.6, 0);
            return f + facing * 0.4 + new Vec3(0, p.height * 0.35, 0);
        }

        // ---- THE SWEEP -------------------------------------------------------------------------------------------------------------------

        void ActiveSweep(Vec3 pw)
        {
            double b = JsMath.Atan2(pw.x - body.position.x, pw.z - body.position.z);
            sweepArm0 = sweepArm = b + Math.PI; // it starts on the far side: the arm reaches you half way round
            sweepSpins = phase == 2 ? Math.Max(1, (int)p42.sweepSpins2) : 1;
            sweepPassHit = -1;
            Go(Brawl40State.Active, Math.Max(1, S(p42.sweepActive)) * sweepSpins);
            events.Emit("brawl42Sweep", Say("swing", Fist));
        }

        /// <summary>Spins this sweep (phase 2: sweep.spins2) and the pass already landed (-1: none): each pass can hit once.</summary>
        public int sweepSpins = 1;
        int sweepPassHit = -1;

        /// <summary>The sweep's arm speed (radians a second: one turn per sweep.active).</summary>
        public double SweepRate => 2 * Math.PI / Math.Max(1e-6, p42.sweepActive);

        /// <summary>One step of the spin: the arm turns 2 pi x spins / steps; a player at shin height inside its reach whose bearing it passes is
        /// hit once per pass (phase 2 spins twice: the second pass wants a second jump, the double jump).</summary>
        void StepSweep()
        {
            int n = Math.Max(1, stateTotal);
            double w = 2 * Math.PI * sweepSpins / n, a0 = sweepArm0 + w * (stateSteps - 1);
            sweepArm = a0 + w;
            // the pass: one per turn, counted from the far side (the arm meets the player half way round each turn)
            int pass = (int)Math.Floor((a0 - sweepArm0) / (2 * Math.PI));
            hitThisAttack = sweepPassHit == pass;
            if (hitThisAttack) return;
            Capsule cap = PlayerCapsule();
            Vec3 pc = (cap.start + cap.end) * 0.5;
            double dx = pc.x - body.position.x, dz = pc.z - body.position.z, d = JsMath.Hypot(dx, dz);
            if (d - cap.radius > p.radius + p42.sweepReach) return;
            if (cap.start.y - cap.radius - body.position.y > p42.sweepHeight) return; // over the arm: the jump answers it
            double rel = JsMath.Atan2(dx, dz) - a0, tol = p42.sweepArmDeg * Math.PI / 180;
            while (rel < 0) rel += 2 * Math.PI;
            while (rel >= 2 * Math.PI) rel -= 2 * Math.PI;
            if (rel > w + tol && rel < 2 * Math.PI - tol) return;
            hitThisAttack = true;
            sweepPassHit = pass;
            sweepHits++;
            HurtPlayer(p42.sweepDamage);
            double ux = d > 1e-6 ? dx / d : facing.x, uz = d > 1e-6 ? dz / d : facing.z;
            target.impulse?.Invoke(ux * p42.sweepKnock, p42.sweepKnockUp, uz * p42.sweepKnock);
            events.Emit("brawl42Sweep", Say("hit", pc));
        }

        void EndSweep()
        {
            Go(Brawl40State.Recover, Math.Max(1, S(p42.sweepRecover)));
            events.Emit("brawl42Sweep", Say("recover", Fist));
        }

        // ---- THE LEAP --------------------------------------------------------------------------------------------------------------------

        /// <summary>The wind-up: the landing circle follows the player for leap.trackShare, then locks.</summary>
        void TellLeap(Vec3 pw)
        {
            if (leapLocked) return;
            if (Progress < p42.leapTrack) { leapTo = Clamp(pw); return; }
            leapLocked = true;
            events.Emit("brawl42Leap", Say("lock", leapTo));
        }

        void ActiveLeap()
        {
            if (!leapLocked) { leapLocked = true; events.Emit("brawl42Leap", Say("lock", leapTo)); }
            leapFrom = body.position;
            Go(Brawl40State.Active, Math.Max(1, S(p42.leapAir)));
            events.Emit("brawl42Leap", Say("jump", Centre));
        }

        /// <summary>One step of the arc: along the line to the locked spot, leap.apex up at the middle.</summary>
        void StepLeap()
        {
            double k = Math.Min(1, stateSteps / (double)Math.Max(1, stateTotal));
            body.position = leapFrom + (leapTo - leapFrom) * k;
            air42 = 4 * p42.leapApex * k * (1 - k);
            Face(leapTo, 10);
        }

        /// <summary>The landing: a hit inside leap.radius (below leap.hitHeight), phase 2's ring, then STUCK.</summary>
        void Land()
        {
            air42 = 0;
            body.position = leapTo;
            Vec3 f = target.position();
            double dx = f.x - body.position.x, dz = f.z - body.position.z, d = JsMath.Hypot(dx, dz);
            events.Emit("brawl42Leap", Say("land", body.position));
            if (d <= p42.leapRadius + 0.4 && f.y - origin.y <= p42.leapHitHeight)
            {
                leapHits++;
                HurtPlayer(p42.leapDamage);
                double ux = d > 1e-6 ? dx / d : facing.x, uz = d > 1e-6 ? dz / d : facing.z;
                target.impulse?.Invoke(ux * p42.leapKnock, p42.leapKnockUp, uz * p42.leapKnock);
                events.Emit("brawl42Leap", Say("hit", f));
            }
            if (phase == 2 && p42.leapRing2 > 0)
            {
                ringOn = true;
                ringHit = false;
                ringR = p.radius;
                ringAt = body.position;
                leapRings++;
                events.Emit("brawl42Leap", Say("ring", body.position));
            }
            Go(Brawl40State.Stuck, Math.Max(1, S(p42.leapStuck)));
            events.Emit("brawl42Leap", Say("stuck", Fist));
        }
    }
}
