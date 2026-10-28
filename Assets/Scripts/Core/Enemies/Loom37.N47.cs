// NIGHT47 builder L (NIGHT47.md G38 / G40; Research/2026-10-18-wave47/L-cards.md section 1, the card gate's PASS): THE LOOM rebuilt, behind the
// sim row loom47 (the sim hands the flag in through Loom47 at each fight's begin; off: nothing here runs, the brain's list and numbers are
// NIGHT46's, bit for bit). Numbers: design.n47.loom47.numbers (each with its _why there).
//   THE WINDOW + CHIP  the brain's chipScale 0.02 -> numbers.chipScale (GG: "you can barely damage it even while firing consistently"; NIGHT47
//                      section 3: chip >= 30 % of full) and windowSeconds 2.2 -> numbers.windowS (B-2: longer than a combo plus the travel);
//                      in the window the body sinks coreDropM toward the player (the spinneret comes to you).
//   THE SEAM           (the BEND is the star) a knee hooked (Latch40: latched) while the player is INSIDE a BEND (the sim hands bendInside47 in)
//                      takes gun damage into its seam; seamDamage tears it (event loom47Seam). The pull-away tear (VERBS 46) is unchanged.
//                      The sim refills the BEND meter on a tear / a parry / a phase start (SurvivalSim.N47.Loom.cs), so the seam shot is made
//                      in a FULL bend.
//   SHUTTLE DIVE       (RED, legs 2-3) the tell marks where you stand; the body dives nose-first at it over `active` (a hit within diveRadius of the
//                      nose: diveDamage + a knock) and STICKS there for its recover (diveStick): its low knees in reach (the punish).
//   WARP CAGE          (RED, legs 2-3) the tell marks a ring of cageRadius round you; on its snap you are hit unless you left the ring, or your
//                      rope is attached, or you are more than cageHalfH over / under its middle (a hook or a jump-dash out).
//   BOBBIN RAIN        (legs 3) three spools drop in a line across your path from bobHeight over it: the middle one YELLOW (punch it back: a
//                      parried spool, Punch), the outer two RED (bobDamage; not parryable). They fall straight (no steer).
// Pure: no UnityEngine, whole steps, JsMath, no randomness of its own (the brain picks).
using System;

namespace Skygrind.Core
{
    /// <summary>design.n47.loom47.numbers (each has its _why there).</summary>
    public sealed class Loom47Params
    {
        public double chipScale = 0.3, windowS = 4, coreDropM = 4, coreDropSpeed = 8, seamDamage = 60, bendOnTear = 0.5, bendOnParry = 0.5, bendAtPhase = 1, fallHp = 5, latchR = 2.2, ruleLineDelayS = 3, finaleS = 0.35, netY = -1e9, netUp = 24, netSide = 6, netHp = 2, netApexAbove = 2;
        public int netChain = 2;
        public double diveWindup = 1.3, diveActive = 1.0, diveStick = 1.5, diveDamage = 35, diveRadius = 3.5, diveWeight = 1, diveKnock = 9, diveNoseY = 3;
        public double cageWindup = 1.5, cageActive = 0.3, cageRecover = 0.8, cageDamage = 25, cageRadius = 5, cageHalfH = 3, cageWeight = 0.9;
        public double bobWindup = 1.2, bobActive = 1.0, bobRecover = 0.6, bobDamage = 20, bobSpacing = 4, bobHeight = 9, bobSpeed = 14, bobWeight = 1.1, bobLead = 0.5;
        public int diveCost = 2, cageCost = 2, bobCost = 2, diveLegs = 6, cageLegs = 6, bobLegs = 4;

        public static Loom47Params From(GameConfig c)
        {
            const string R = "design.n47.loom47.numbers.";
            var p = new Loom47Params();
            double N(string k, double d) => c.Has(R + k) ? c.Num(R + k) : d;
            int L(string k, int d)
            {
                if (!c.Has(R + k)) return d;
                int m = 0;
                foreach (double l in c.Nums(R + k)) m |= 1 << ((int)l - 1);
                return m;
            }
            p.chipScale = N("chipScale", p.chipScale);
            p.windowS = N("windowS", p.windowS);
            p.coreDropM = N("coreDropM", p.coreDropM);
            p.coreDropSpeed = N("coreDropSpeed", p.coreDropSpeed);
            p.seamDamage = N("seamDamage", p.seamDamage);
            p.bendOnTear = N("bendOnTear", p.bendOnTear);
            p.bendOnParry = N("bendOnParry", p.bendOnParry);
            p.bendAtPhase = N("bendAtPhase", p.bendAtPhase);
            p.fallHp = N("fallHp", p.fallHp);
            p.latchR = N("latchR", p.latchR);
            p.ruleLineDelayS = N("ruleLineDelayS", p.ruleLineDelayS);
            p.finaleS = N("finaleS", p.finaleS);
            p.netY = N("netY", p.netY);
            p.netUp = N("netUp", p.netUp);
            p.netSide = N("netSide", p.netSide);
            p.netHp = N("netHp", p.netHp);
            p.netApexAbove = N("netApexAbove", p.netApexAbove);
            p.netChain = (int)N("netChain", p.netChain);
            p.diveWindup = N("diveWindup", p.diveWindup);
            p.diveActive = N("diveActive", p.diveActive);
            p.diveStick = N("diveStick", p.diveStick);
            p.diveDamage = N("diveDamage", p.diveDamage);
            p.diveRadius = N("diveRadius", p.diveRadius);
            p.diveWeight = N("diveWeight", p.diveWeight);
            p.diveKnock = N("diveKnock", p.diveKnock);
            p.diveNoseY = N("diveNoseY", p.diveNoseY);
            p.diveCost = (int)N("diveCost", p.diveCost);
            p.diveLegs = L("diveLegs", p.diveLegs);
            p.cageWindup = N("cageWindup", p.cageWindup);
            p.cageActive = N("cageActive", p.cageActive);
            p.cageRecover = N("cageRecover", p.cageRecover);
            p.cageDamage = N("cageDamage", p.cageDamage);
            p.cageRadius = N("cageRadius", p.cageRadius);
            p.cageHalfH = N("cageHalfH", p.cageHalfH);
            p.cageWeight = N("cageWeight", p.cageWeight);
            p.cageCost = (int)N("cageCost", p.cageCost);
            p.cageLegs = L("cageLegs", p.cageLegs);
            p.bobWindup = N("bobWindup", p.bobWindup);
            p.bobActive = N("bobActive", p.bobActive);
            p.bobRecover = N("bobRecover", p.bobRecover);
            p.bobDamage = N("bobDamage", p.bobDamage);
            p.bobSpacing = N("bobSpacing", p.bobSpacing);
            p.bobHeight = N("bobHeight", p.bobHeight);
            p.bobSpeed = N("bobSpeed", p.bobSpeed);
            p.bobWeight = N("bobWeight", p.bobWeight);
            p.bobLead = N("bobLead", p.bobLead);
            p.bobCost = (int)N("bobCost", p.bobCost);
            p.bobLegs = L("bobLegs", p.bobLegs);
            return p;
        }
    }

    public sealed partial class Loom37
    {
        public const string Dive = "dive", Cage = "cage", Bobbin = "bobbin";

        /// <summary>loom47 on this fight (the sim sets it at the fight's begin).</summary>
        public bool loom47;
        public Loom47Params p47;

        /// <summary>kneeHold47 (the sim hands it in; null: never): true while the body and knees hold still (a BEND's first holdS).</summary>
        public Func<bool> holdKnees47;
        /// <summary>The harness's counter: steps held (kneeHold47).</summary>
        public int holds47;

        /// <summary>The player is inside a BEND now (the sim hands it in): the seam rule.</summary>
        public Func<bool> bendInside47;

        double l47ChipWas = double.NaN, l47WindowWas = double.NaN;
        readonly double[] seam47 = new double[6];

        /// <summary>The dive's line (world) and the cage's ring (the view's tells).</summary>
        public Vec3 diveFrom, diveTo, cageAt;
        Vec3 bobAim, bobSide;

        /// <summary>The harness's counters (this fight).</summary>
        public int seamHits47, seamTears47, dives47, diveHits47, cages47, cageHits47, bobbins47, bobbinHits47;

        public bool DiveLive => loom47 && brain.Current?.id == Dive && (brain.state == Boss37State.Active || brain.state == Boss37State.Recover);
        public bool CageTelling => loom47 && brain.Current?.id == Cage && brain.state == Boss37State.Tell;

        /// <summary>The seam's fill 0..1 on knee i (the view).</summary>
        public double Seam01(int i) => loom47 && p47 != null && i >= 0 && i < 6 ? Math.Min(1, seam47[i] / Math.Max(1e-6, p47.seamDamage)) : 0;

        /// <summary>At a fight's begin: loom47's attacks in the brain's list and its window / chip numbers (on), or NIGHT46's (off).</summary>
        public void Loom47(bool on, Loom47Params lp)
        {
            for (int i = brain.attacks.Count - 1; i >= 0; i--)
            {
                string id = brain.attacks[i].id;
                if (id == Dive || id == Cage || id == Bobbin) brain.attacks.RemoveAt(i);
            }
            if (!double.IsNaN(l47ChipWas)) { brain.p.chipScale = l47ChipWas; l47ChipWas = double.NaN; }
            if (!double.IsNaN(l47WindowWas)) { brain.p.windowSeconds = l47WindowWas; l47WindowWas = double.NaN; }
            loom47 = on && lp != null;
            p47 = lp;
            for (int i = 0; i < 6; i++) seam47[i] = 0;
            holds47 = 0;
            seamHits47 = seamTears47 = dives47 = diveHits47 = cages47 = cageHits47 = bobbins47 = bobbinHits47 = 0;
            if (!loom47) return;
            l47ChipWas = brain.p.chipScale;
            l47WindowWas = brain.p.windowSeconds;
            brain.p.chipScale = lp.chipScale;
            brain.p.windowSeconds = lp.windowS;
            brain.attacks.Add(Boss37Attack.Of(Dive, Tell36Kind.Red, lp.diveWindup, lp.diveActive, lp.diveStick, dt, lp.diveCost, lp.diveWeight, 0, 1e9, lp.diveLegs, Boss37Habit.Any));
            brain.attacks.Add(Boss37Attack.Of(Cage, Tell36Kind.Red, lp.cageWindup, lp.cageActive, lp.cageRecover, dt, lp.cageCost, lp.cageWeight, 0, 1e9, lp.cageLegs, Boss37Habit.Any));
            brain.attacks.Add(Boss37Attack.Of(Bobbin, Tell36Kind.Yellow, lp.bobWindup, lp.bobActive, lp.bobRecover, dt, lp.bobCost, lp.bobWeight, 0, 1e9, lp.bobLegs, Boss37Habit.Any));
        }

        /// <summary>Filter's hook (a hit on knee k, before the scale): the seam of a latched knee fills while the player is inside a BEND.</summary>
        void Seam47(int k, double amount)
        {
            if (k < 0 || k >= 6 || brain.Open || !Latched40(k) || bendInside47 == null || !bendInside47()) return;
            seam47[k] += amount;
            seamHits47++;
            Say("loom47Seam", "hit", k, knees[k].at);
            if (seam47[k] < p47.seamDamage) return;
            seam47[k] = 0;
            seamTears47++;
            Tear(k, "seam");
        }

        /// <summary>Before Pose: the dive moves the body along its line and holds it at the perch; the window sinks it toward the player.</summary>
        void Loom47Body(Vec3 me)
        {
            Boss37Attack a = brain.Current;
            if (a != null && a.id == Dive && brain.state == Boss37State.Active)
            {
                double t = Math.Min(1, brain.stateSteps / (double)Math.Max(1, brain.stateTotal));
                double e = t * t * (3 - 2 * t); // eased (smoothstep): a lunge, not a slide
                centre = diveFrom + (diveTo - diveFrom) * e;
                if (stay40) centre = Clamp40(centre);
                return;
            }
            if (a != null && a.id == Dive && brain.state == Boss37State.Recover)
            {
                centre = stay40 ? Clamp40(diveTo) : diveTo;
                return;
            }
            if (brain.Open)
            {
                Vec3 goal = stations[Math.Min(2, Math.Max(0, brain.leg - 1))];
                double floor = goal.y - p47.coreDropM, y = Math.Max(floor, centre.y - p47.coreDropSpeed * dt);
                centre = new Vec3(centre.x, y, centre.z);
            }
        }

        /// <summary>After the knees: the tells mark their places, the strikes land (once), the dive hits along its line.</summary>
        void Loom47Attacks(Boss37Attack a, Vec3 me)
        {
            if (a.id != Dive && a.id != Cage && a.id != Bobbin) return;
            if (brain.state == Boss37State.Tell && brain.entered)
            {
                if (a.id == Dive)
                {
                    dives47++;
                    diveFrom = centre;
                    diveTo = new Vec3(me.x, me.y + p.bodyHalfH + p47.diveNoseY, me.z);
                    s47DiveHit = false;
                    Say("loom47Dive", "tell", -1, me);
                }
                else if (a.id == Cage)
                {
                    cages47++;
                    cageAt = me;
                    Say("loom47Cage", "tell", -1, me);
                }
                else
                {
                    bobbins47++;
                    Vec3 v = playerVel != null ? playerVel() : new Vec3(0, 0, 0);
                    double vl = Math.Sqrt(v.x * v.x + v.z * v.z);
                    Vec3 f = vl > 0.5 ? new Vec3(v.x / vl, 0, v.z / vl) : Flat(centre - me);
                    bobAim = new Vec3(me.x + v.x * p47.bobLead, me.y, me.z + v.z * p47.bobLead);
                    bobSide = new Vec3(-f.z, 0, f.x); // the line runs across your path
                    Say("loom47Bobbin", "tell", -1, bobAim);
                }
            }
            if (brain.state == Boss37State.Active)
            {
                if (a.id == Dive)
                {
                    if (brain.entered) Say("loom47Dive", "strike", -1, diveTo);
                    Vec3 nose = new Vec3(centre.x, centre.y - p.bodyHalfH, centre.z);
                    if (!s47DiveHit && Vec3.DistanceTo(nose, new Vec3(me.x, me.y + 0.9, me.z)) <= p47.diveRadius)
                    {
                        s47DiveHit = true;
                        diveHits47++;
                        target.health.Damage(p47.diveDamage, "loom37");
                        Vec3 k = Flat(me - centre);
                        target.impulse?.Invoke(k.x * p47.diveKnock, 4, k.z * p47.diveKnock);
                        Say("loom47Dive", "hit", -1, me);
                    }
                }
                else if (brain.entered && a.id == Cage)
                {
                    double dx = me.x - cageAt.x, dz = me.z - cageAt.z;
                    bool inside = Math.Sqrt(dx * dx + dz * dz) <= p47.cageRadius && Math.Abs(me.y - cageAt.y) <= p47.cageHalfH;
                    bool rope = ropeOut != null && ropeOut();
                    if (inside && !rope)
                    {
                        cageHits47++;
                        target.health.Damage(p47.cageDamage, "loom37");
                        Say("loom47Cage", "hit", -1, me);
                    }
                    else Say("loom47Cage", "free", -1, me);
                }
                else if (brain.entered && a.id == Bobbin)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        Vec3 at = bobAim + bobSide * (i * p47.bobSpacing);
                        Vec3 from = new Vec3(at.x, at.y + p47.bobHeight, at.z);
                        spools.Add(new Loom37Spool
                        {
                            pos = from, prev = from, vel = new Vec3(0, -p47.bobSpeed, 0), alive = true, bob47 = true, red47 = i != 0, dmg47 = p47.bobDamage,
                            life = S(p47.bobHeight / p47.bobSpeed + 1.0),
                        });
                    }
                    Say("loom47Bobbin", "drop", -1, bobAim);
                }
            }
            if (a.id == Dive && brain.state == Boss37State.Recover && brain.entered) Say("loom47Dive", "stick", -1, diveTo);
        }

        bool s47DiveHit;

        /// <summary>The bar tells the phase (B-1): in phases 1-2 the bar stops at the phase's third (maxHp x (3 - leg) / 3), so a 4 s window
        /// never kills THE LOOM before its last phase (MEASURED round 1, before this: 3 of 10 skilled fights ended in 2 windows, 29-36 s); the
        /// last phase's window takes the rest (the fall).</summary>
        double Loom47Floor(double dmg)
        {
            int leg = Math.Max(1, Math.Min(3, brain.leg));
            if (leg >= 3) return dmg;
            double floor = body.maxHp * (3 - leg) / 3.0;
            return Math.Max(0, Math.Min(dmg, body.hp - floor));
        }

        static Vec3 Flat(Vec3 v)
        {
            double l = Math.Sqrt(v.x * v.x + v.z * v.z);
            return l > 1e-6 ? new Vec3(v.x / l, 0, v.z / l) : new Vec3(0, 0, 1);
        }
    }
}
