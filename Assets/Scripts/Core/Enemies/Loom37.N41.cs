// NIGHT41 builder B8 (NIGHT41.md B8; INVENTORY A4; NIGHT40.md boss_rules 1): THE LOOM's FLOOR SWEEP, behind sweep41 (the sim hands the flag in
// through Sweep41 at each fight's begin; off: the attack is not in the brain's list and Loom37.cs runs as NIGHT40, bit for bit).
// The root cause (MEASURED, `n41loom sweep` with sweep41 off): THE LOOM had three attacks (lash, spool, web) and none of them asks for height:
// standing on the start ledge or a tower top answered everything, and the no-ride phases (loom40, D-12) had no attack that made a hook the answer.
// With sweep41:
//   SWEEP (RED, a floor decal tell of windup >= 0.9 s, boss_rules 1): a thread sweeps across THE CANYON (x, from the wall farther from you to
//          the nearer one) over a band of the canyon along z (halfZ either side of where you stood at the wind-up's start), up to topY[leg] over
//          the ledges (y 0): leg 1 covers the start / end ledges (the tower tops at y 8 are the answer), legs 2-3 cover the tower tops too, the
//          phase where two thirds of the ride walls refuse a ride (loom40's no-ride phases): the answer is a hook.
//   ANSWER on the line's pass: riding a wall (a wall that allows it), a rope attached (the hook), or feet at or over topY: a dodge (counted by
//          answer). Else sweepDamage and a lift (the thread scoops you).
// Pure: no UnityEngine, whole steps, JsMath, no randomness of its own (the brain picks).
using System;

namespace Skygrind.Core
{
    /// <summary>design.n41.sweep41's numbers (each has its _why there).</summary>
    public sealed class Sweep41Params
    {
        public double windup = 1.2, active = 0.9, recover = 0.7, weight = 1, damage = 25, widthM = 2.5, halfZ = 18, liftUp = 7;
        public int cost = 2, legMask = 7;
        public double[] topY = { 3, 11, 11 };
        /// <summary>A sweep over refillOverY (the tower tops) gives one grapple charge back at its tell when you have none (the sim's rule).</summary>
        public bool tellRefill = true;
        public double refillOverY = 8;
        /// <summary>The quiet before a sweep: seconds since another attack's tell / strike (or a spool in flight) before the sweep may be picked.</summary>
        public double gapS = 1.5;

        public static Sweep41Params From(GameConfig c)
        {
            const string R = "design.n41.sweep41.";
            var p = new Sweep41Params();
            double N(string k, double d) => c.Has(R + k) ? c.Num(R + k) : d;
            p.windup = N("windup", p.windup);
            p.active = N("active", p.active);
            p.recover = N("recover", p.recover);
            p.weight = N("weight", p.weight);
            p.cost = (int)N("cost", p.cost);
            p.damage = N("damage", p.damage);
            p.widthM = N("widthM", p.widthM);
            p.halfZ = N("halfZ", p.halfZ);
            p.liftUp = N("liftUp", p.liftUp);
            if (c.Has(R + "topY")) p.topY = c.Nums(R + "topY");
            if (c.Has(R + "tellRefill")) p.tellRefill = c.Bool(R + "tellRefill");
            p.refillOverY = N("refillOverY", p.refillOverY);
            p.gapS = N("gapS", p.gapS);
            if (c.Has(R + "legs"))
            {
                p.legMask = 0;
                foreach (double l in c.Nums(R + "legs")) p.legMask |= 1 << ((int)l - 1);
            }
            return p;
        }
    }

    public sealed partial class Loom37
    {
        public const string Sweep = "sweep";

        /// <summary>sweep41 on this fight (the sim sets it at the fight's begin).</summary>
        public bool sweep41;
        public Sweep41Params sp41;

        /// <summary>The player's rope attached (the sim hands it in; the hook answers the sweep).</summary>
        public Func<bool> ropeAttached41;

        /// <summary>The sweep's band (world): the line runs x sweepFromX -> sweepToX over z sweepZ0..sweepZ1, up to sweepTopY (the view's decal).</summary>
        public double sweepFromX, sweepToX, sweepZ0, sweepZ1, sweepTopY, sweepLineX;
        double s41X0, s41X1;
        int s41Step;
        bool s41Passed;

        /// <summary>The harness's counters (this fight): sweeps told, hits, dodges by answer (ride / rope / height), passes outside the band.</summary>
        public int sweeps, sweepHits, sweepRide, sweepRope, sweepHigh;

        public bool SweepTelling => sweep41 && brain.Current?.id == Sweep && brain.state == Boss37State.Tell;
        public bool SweepLive => sweep41 && brain.Current?.id == Sweep && brain.state == Boss37State.Active;

        /// <summary>The sweep's tell progress 0..1 (the view's decal fill), 0 when not telling.</summary>
        public double SweepTell01 => SweepTelling && sp41 != null ? Math.Min(1, brain.stateSteps / (double)Math.Max(1, MovementMath.StepsFor(sp41.windup, dt))) : 0;

        /// <summary>At a fight's begin: the sweep in the brain's list (on) or out of it (off); the canyon's x span (the sweep runs wall to wall).</summary>
        public void Sweep41(bool on, Sweep41Params sp, double canyonX0, double canyonX1)
        {
            for (int i = brain.attacks.Count - 1; i >= 0; i--) if (brain.attacks[i].id == Sweep) brain.attacks.RemoveAt(i);
            sweep41 = on;
            sp41 = sp;
            s41X0 = canyonX0;
            s41X1 = canyonX1;
            sweeps = sweepHits = sweepRide = sweepRope = sweepHigh = 0;
            s41LastOther = -1000000;
            if (!on || sp == null) return;
            brain.attacks.Add(Boss37Attack.Of(Sweep, Tell36Kind.Red, sp.windup, sp.active, sp.recover, dt, sp.cost, sp.weight, 0, 1e9, sp.legMask, Boss37Habit.Any));
        }

        /// <summary>Before the brain's step: the sweep is out of the pick (its leg mask 0) until gapS after the last step another attack was
        /// telling or striking (or a spool was in the air), so a sweep never stacks on a lash / spool read; its own recover keeps the next tell
        /// gapS after its pass. MEASURED round 3: the casual pilot died 2 / 10 to a sweep's pull landing it into the next lash / spool.</summary>
        void Sweep41Gate()
        {
            Boss37Attack cur = brain.Current;
            if ((cur != null && cur.id != Sweep && (brain.state == Boss37State.Tell || brain.state == Boss37State.Active)) || spools.Count > 0) s41LastOther = stepNo;
            Boss37Attack sw = null;
            foreach (Boss37Attack a in brain.attacks) if (a.id == Sweep) { sw = a; break; }
            if (sw == null) return;
            sw.legMask = stepNo - s41LastOther >= MovementMath.StepsFor(sp41.gapS, dt) ? sp41.legMask : 0;
        }

        int s41LastOther = -1000000;

        /// <summary>The wind-up's first step: the band where you stand, the line from the far wall.</summary>
        void Sweep41Tell(Vec3 me)
        {
            sweeps++;
            s41Step = 0;
            s41Passed = false;
            bool fromWest = me.x - s41X0 > s41X1 - me.x; // you are nearer the east wall: it starts at the west wall
            sweepFromX = fromWest ? s41X0 : s41X1;
            sweepToX = fromWest ? s41X1 : s41X0;
            sweepLineX = sweepFromX;
            sweepZ0 = me.z - sp41.halfZ;
            sweepZ1 = me.z + sp41.halfZ;
            double[] t = sp41.topY;
            sweepTopY = t.Length > 0 ? t[Math.Min(t.Length - 1, Math.Max(0, brain.leg - 1))] : 3;
            Say("loom41Sweep", "tell", -1, new Vec3((s41X0 + s41X1) * 0.5, 0, me.z));
        }

        /// <summary>Each active step: the line moves; on its pass over you (inside the band) the answer is read once.</summary>
        void Sweep41Active(Vec3 me)
        {
            int n = Math.Max(1, MovementMath.StepsFor(sp41.active, dt));
            double prev = sweepLineX;
            s41Step++;
            sweepLineX = sweepFromX + (sweepToX - sweepFromX) * Math.Min(1, s41Step / (double)n);
            if (s41Passed || me.z < sweepZ0 || me.z > sweepZ1) return;
            double lo = Math.Min(prev, sweepLineX) - sp41.widthM, hi = Math.Max(prev, sweepLineX) + sp41.widthM;
            if (me.x < lo || me.x > hi) return;
            s41Passed = true;
            string how;
            if (playerRiding != null && playerRiding()) { sweepRide++; how = "ride"; }
            else if (ropeAttached41 != null && ropeAttached41()) { sweepRope++; how = "rope"; }
            else if (me.y >= sweepTopY) { sweepHigh++; how = "high"; }
            else
            {
                sweepHits++;
                how = "hit";
                target.health.Damage(sp41.damage, "loom37");
                target.impulse?.Invoke(0, sp41.liftUp, 0);
            }
            Say("loom41Sweep", how, -1, me);
        }
    }
}
