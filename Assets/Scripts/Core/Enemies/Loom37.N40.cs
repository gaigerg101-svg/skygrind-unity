// NIGHT40 builder B4 (NIGHT40.md B4 THE LOOM; boss_rules 4 "the boss stays in its arena"): THE LOOM stays in THE CANYON, behind loom40
// (the sim hands the flag in through Stay40; off: Loom37.cs runs as NIGHT39, bit for bit).
// The root cause (MEASURED, `n40loom stay`, loom40 off): the anti-hide wrote stations[leg].z = player.z - 6 with no clamp and never put it
// back (a camper by o2's far end dragged the body 43.7 m out of o3; 19247 steps out over 10 seeds), the glide ran at 36-40 m/s (a 36 m
// station step in one second: a jump cut, not a move), and the death fall dropped 72 m, 27.6 m through the canyon floor.
// With loom40:
//   CLAMP     every station and the body's centre inside o3's floor plan minus marginM (the feet reach ~10 m from the centre).
//   HOME      an anti-hide's station goes back to the leg's own station homeS after the last anti-hide, and at once on a leg restart.
//   GLIDE     a readable glide: speed eases up at glideAccel to glideMax and brakes onto the goal (v <= sqrt(2 a d)); a leg change is held
//             open (Boss37Brain.HoldLegChange) until the body arrives, at most legHoldMaxS more (the phase transition, boss_rules 6).
//   LAND      the death fall stops with the feet on the canyon floor (floorY).
//   TEAR      (round 2) no nails: a knee the grapple hooks LATCHES (Latch40, the sim tells which knee the rope is on); the old pull-away tears it.
// Pure: no UnityEngine, whole steps, no randomness.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n40.loom40's stay numbers (each has its _why there).</summary>
    public sealed class Loom40Params
    {
        public double marginM = 10, glideMax = 18, glideAccel = 24, homeS = 6, legHoldMaxS = 3, floorY = -24, lineS = 5, noRideWarnS = 1, practiceBackS = 3, spoolTurnDeg = 90, tearHeal = 20;

        public static Loom40Params From(GameConfig c)
        {
            const string R = "design.n40.loom40.";
            var p = new Loom40Params();
            double N(string k, double d) => c.Has(R + k) ? c.Num(R + k) : d;
            p.marginM = N("marginM", p.marginM);
            p.glideMax = N("glideMax", p.glideMax);
            p.glideAccel = N("glideAccel", p.glideAccel);
            p.homeS = N("homeS", p.homeS);
            p.legHoldMaxS = N("legHoldMaxS", p.legHoldMaxS);
            p.floorY = N("floorY", p.floorY);
            p.lineS = N("lineS", p.lineS);
            p.noRideWarnS = N("noRideWarnS", p.noRideWarnS);
            p.practiceBackS = N("practiceBackS", p.practiceBackS);
            p.spoolTurnDeg = N("spoolTurnDeg", p.spoolTurnDeg);
            p.tearHeal = N("tearHeal", p.tearHeal);
            return p;
        }
    }

    public sealed partial class Loom37
    {
        readonly Vec3[] home40 = new Vec3[3];
        bool stay40;
        Loom40Params p40;
        double r40x0, r40z0, r40x1, r40z1, glideV;
        Vec3 glideDir;
        int homeLeft = -1, homeLeg, restarts40, holdSteps;

        /// <summary>The harness's counters: anti-hides clamped, stations sent home, leg-change steps held for the glide.</summary>
        public int stay40Clamps, stay40Homes, stay40HeldSteps, restores40;

        /// <summary>loom40's stay rule on (set by the sim before each fight's start).</summary>
        public bool Stay40On => stay40;

        /// <summary>The glide's top speed (m/s; the view's lean scale).</summary>
        public double Glide40Max => p40 != null ? p40.glideMax : 18;

        /// <summary>The glide's speed now (m/s; the view's lean).</summary>
        public double GlideSpeed => glideV;

        /// <summary>The sim hands in loom40's bit, o3's floor plan (world) and the numbers. Off: nothing of this file runs.</summary>
        public void Stay40(bool on, double x0, double z0, double x1, double z1, Loom40Params prm)
        {
            stay40 = on && prm != null;
            p40 = prm ?? new Loom40Params();
            double m = p40.marginM;
            r40x0 = x0 + m; r40x1 = x1 - m;
            if (r40x0 > r40x1) r40x0 = r40x1 = (x0 + x1) * 0.5;
            r40z0 = z0 + m; r40z1 = z1 - m;
            if (r40z0 > r40z1) r40z0 = r40z1 = (z0 + z1) * 0.5;
        }

        /// <summary>A point clamped into the canyon minus the margin (x / z; y kept).</summary>
        public Vec3 Clamp40(Vec3 v) => new Vec3(Math.Max(r40x0, Math.Min(r40x1, v.x)), v.y, Math.Max(r40z0, Math.Min(r40z1, v.z)));

        void Stay40Start()
        {
            for (int i = 0; i < 3; i++) stations[i] = Clamp40(home40[i]);
            centre = stations[0];
            glideV = 0;
            glideDir = Vec3.Zero;
            homeLeft = -1;
            restarts40 = 0; // the brain's Begin (after this) resets its counters
            holdSteps = 0;
            stay40Clamps = stay40Homes = stay40HeldSteps = latches40 = restores40 = forces40 = 0;
        }

        /// <summary>Before the brain's step: a restarted leg's station home at once; an anti-hide's station home homeS after it; a leg change
        /// held open while the body still glides to its new station (at most legHoldMaxS past the change's own time).</summary>
        void Stay40Before()
        {
            int li = Math.Min(2, Math.Max(1, brain.leg) - 1);
            if (brain.restarts != restarts40)
            {
                restarts40 = brain.restarts;
                stations[li] = Clamp40(home40[li]);
                homeLeft = -1;
            }
            if (homeLeft > 0 && --homeLeft == 0)
            {
                stations[homeLeg] = Clamp40(home40[homeLeg]);
                homeLeft = -1;
                stay40Homes++;
            }
            // the window a leg restart swallowed (a fall during the CYAN window: RestartLeg goes to REST with the meter kept at 0, and no drain
            // can open it again until a parry: MEASURED round 3, seed 1 fell at 16 s and its window came at 43 s): an empty meter is a window
            if (brain.Fighting && !brain.Open && brain.stagger <= 0 && brain.Stagger(1, "restore")) { windowsOpened++; restores40++; Say("loom37Open", "open", -1, Spinneret); }
            // the leg's pair torn and the meter not empty (legs 2-3 ask a parry on top): the next attack is the YELLOW spool, until one is parried
            // (MEASURED round 3: both knees torn at 44 s, then 19 s of lashes and no spool to parry until the pilot died)
            if (brain.Fighting && !brain.Open && brain.stagger > 0 && brain.leg >= 1 && knees[2 * (li)].torn && knees[2 * li + 1].torn && brain.Force(Spool)) forces40++;
            if (brain.state == Boss37State.LegChange)
            {
                if (Vec3.DistanceTo(centre, stations[li]) > 0.05 && holdSteps < S(p40.legHoldMaxS))
                {
                    if (brain.stateLeft < 2) { holdSteps++; stay40HeldSteps++; }
                    brain.HoldLegChange();
                }
            }
            else holdSteps = 0;
        }

        /// <summary>The anti-hide: the leg's station swings over the player along the canyon, clamped inside it; home again homeS later.</summary>
        void Stay40Reposition(int li, Vec3 me)
        {
            Vec3 want = new Vec3(stations[li].x, stations[li].y, me.z - 6);
            Vec3 at = Clamp40(want);
            if (at.x != want.x || at.z != want.z) stay40Clamps++;
            stations[li] = at;
            homeLeg = li;
            homeLeft = Math.Max(1, S(p40.homeS));
        }

        /// <summary>The glide to `goal`: eases up at glideAccel to glideMax, brakes onto the goal (v <= sqrt(2 a d)); a reversal starts from rest.</summary>
        void Stay40Glide(Vec3 goal)
        {
            Vec3 dv = goal - centre;
            double dl = dv.Length;
            if (dl < 1e-9) { glideV = 0; centre = Clamp40(goal); return; }
            Vec3 dir = dv / dl;
            if (Vec3.Dot(dir, glideDir) < 0) glideV = 0;
            glideDir = dir;
            double want = Math.Min(p40.glideMax, Math.Sqrt(2 * p40.glideAccel * dl));
            glideV = Math.Min(want, glideV + p40.glideAccel * dt);
            double mv = glideV * dt;
            if (dl <= mv) { centre = goal; glideV = 0; }
            else centre = centre + dir * mv;
            centre = Clamp40(centre);
        }

        /// <summary>The harness's counter: knees hooked (latched) by the grapple.</summary>
        public int latches40;

        /// <summary>NIGHT40 B4 THE TEAR BY THE GRAPPLE (loom40; GG "The STITCHER weapon should be removed from the fourth level entirely"; Doom
        /// Eternal's Khan Maykr: hook, then punch): the rope attached to knee `i` this step (the sim tells) LATCHES it: the knee counts as carrying
        /// nailsToTear (the hold, nailHoldS, starts again), and the existing pull-away tear (Step: a rope out and the player tearDistM farther than
        /// the nearest since the latch) tears it. Only a live knee of the leg's pair outside the CYAN window; no nails anywhere. True: it latched (the
        /// sim then lets the rope go: the hook BITES, it does not pull you off your ledge into the void: MEASURED round 2, 7 falls a fight).</summary>
        public bool Latch40(int i, Vec3 me)
        {
            if (!stay40 || i < 0 || i >= 6 || !body.alive || falling || !brain.Fighting || brain.Open || !ActiveKnee(i)) return false;
            Loom37Knee k = knees[i];
            double d = Vec3.DistanceTo(me, k.at);
            if (k.nails < p.nailsToTear)
            {
                k.nails = p.nailsToTear;
                k.nearest = d;
                latches40++;
                Say("loom40Latch", "hook", i, k.at);
            }
            else if (d < k.nearest) k.nearest = d;
            k.nailAge = 0;
            return true;
        }

        /// <summary>Is knee i latched now (loom40: hooked, waiting for the pull-away)?</summary>
        public bool Latched40(int i) => stay40 && i >= 0 && i < 6 && !knees[i].torn && knees[i].nails >= p.nailsToTear;

        /// <summary>The steps a spool was forced (the pair torn, a parry still owed).</summary>
        public int forces40;

        /// <summary>A live spool turns toward the chest at most spoolTurnDeg a second, its speed kept: a sidestep no longer dodges it, a punch
        /// (the parry) answers it (boss_rules 2: every attack has a movement or a parry answer; a dash past it still works).</summary>
        void Stay40Steer(Loom37Spool s, Vec3 chest)
        {
            Vec3 to = chest - s.pos;
            double tl = to.Length, sp = s.vel.Length;
            if (tl < 1e-6 || sp < 1e-6) return;
            Vec3 want = to / tl, now = s.vel / sp;
            double cos = Vec3.Dot(want, now);
            double cx = now.y * want.z - now.z * want.y, cy = now.z * want.x - now.x * want.z, cz = now.x * want.y - now.y * want.x;
            double maxTurn = p40.spoolTurnDeg * Math.PI / 180 * dt;
            double ang = JsMath.Atan2(JsMath.Hypot(cx, JsMath.Hypot(cy, cz)), cos);
            Vec3 dir = ang <= maxTurn ? want : now + (want - now) * (maxTurn / ang);
            double dl = dir.Length;
            if (dl < 1e-6) return;
            s.vel = dir * (sp / dl);
        }

        /// <summary>The death fall stops with the feet on the canyon floor (floorY).</summary>
        void Stay40Land(ref double v)
        {
            double y = p40.floorY + p.footDropY;
            if (centre.y >= y) return;
            centre = new Vec3(centre.x, y, centre.z);
            v = 0;
        }
    }
}
