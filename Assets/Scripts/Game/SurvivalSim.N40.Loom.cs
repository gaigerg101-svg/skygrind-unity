// NIGHT40 builder B4 (NIGHT40.md B4 THE LOOM): loom40 in the sim. Fills the lead's partials N40LoomStartRun / N40LoomStep (SurvivalSim.N40.cs).
// - The stay rule (boss_rules 4; Core/Enemies/Loom37.N40.cs): at each fight's begin THE LOOM gets loom40's bit, THE CANYON's floor plan (o3)
//   and design.n40.loom40's numbers: stations and body clamped inside it, a station home after an anti-hide, a readable glide, the fall lands.
// - The tear by the grapple (round 2): the rope attached to a knee node latches that knee (Loom37.Latch40); the old pull-away tears it. o2's
//   PRACTICE KNEE (the room row's kneeTarget37, data since NIGHT37, never live before) is a grapple node on n40-span: hook it, then pull away
//   to another thread: it tears (event loom40Practice "latch" / "tear"), and is back practiceBackS later.
// - The boss line (boss_rules 5; B5's Core/BossLine40): the damage rule on entry ("HOOK A LIT KNEE ..."), a nudge on the first latches, the
//   CYAN window's "OPEN" line.
// - THE NO-RIDE PHASES (D-12): o3's row `loom40NoRide` (gen-n40-span.cjs) lists the canyon's ride walls that refuse a ride in phase 1 (leg 1:
//   about a third) and phase 2 (legs 2-3: about two thirds). A runtime switch of the Mover's noride set (Mover.SetNoRide38: the level's own
//   boxes + the phase's), each change behind a noRideWarnS warning (event loom40NoRide "warn" then "on"; NoRide39View blinks them, then draws
//   them dead). Back to the level's own set when the fight ends.
// Off (loom40 off, --off40): Stay40(false, ...), no latch, no practice node, no line, no phase set: NIGHT39 bit for bit.
// No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Loom40Params loom40Params;

        /// <summary>The practice knee's grapple node (-1: none), its place, latched / torn now, counters (the harness).</summary>
        public int loom40PracticeNode = -1;
        public Vec3 loom40PracticeAt;
        public bool loom40PracticeLatched, loom40PracticeTorn;
        public int loom40PracticeTears, loom40PracticeLatches, loom40Heals;
        double l40PracticeNearest;
        int l40PracticeAge, l40PracticeBack;

        /// <summary>The boss line (null until the first fight with loom40).</summary>
        public BossLine40 loom40Line;

        /// <summary>The no-ride phases: the boxes of phase 1 / 2 (o3's row), the phase on the Mover now (0: none), the one coming and its warning left.</summary>
        public Ride38.Box[] loom40NoRide1, loom40NoRide2;
        public int loom40NoRidePhase, loom40NoRideNext, loom40NoRideWarnLeft, loom40NoRideWarnTotal, loom40NoRideVersion;
        Ride38.Box[] l40Static, l40Mover1, l40Mover2;
        readonly List<Ride38.Box> l40Scratch = new List<Ride38.Box>();

        /// <summary>The phase's boxes now (the view), null when none.</summary>
        public Ride38.Box[] Loom40NoRideBoxes(int phase) => phase == 1 ? loom40NoRide1 : phase == 2 ? loom40NoRide2 : null;

        partial void N40LoomStartRun()
        {
            if (loom40Params == null) loom40Params = Loom40Params.From(cfg);
            // the practice knee: off the map's node list first (the run before may have added it; the fight's own nodes went in Loom37BossStartRun)
            if (loom40PracticeNode >= 0 && loom40PracticeNode == map.grappleNodes.Count - 1) map.grappleNodes.RemoveAt(loom40PracticeNode);
            loom40PracticeNode = -1;
            loom40PracticeLatched = loom40PracticeTorn = false;
            loom40PracticeTears = loom40PracticeLatches = 0;
            l40PracticeAge = l40PracticeBack = 0;
            loom40NoRide1 = loom40NoRide2 = l40Mover1 = l40Mover2 = l40Static = null;
            loom40NoRidePhase = loom40NoRideNext = loom40NoRideWarnLeft = loom40NoRideWarnTotal = 0;
            loom40NoRideVersion++;
            loom40Line?.Clear();
            if (!n40.loom40 || !Loom37Level || map?.json == null || !map.json.ContainsKey("campaign")) return;
            foreach (object ro in PortData.Arr(PortData.Obj(map.json["campaign"])["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (r.ContainsKey("kneeTarget37") && loom40PracticeNode < 0)
                {
                    loom40PracticeAt = PortData.V3(PortData.Obj(r["kneeTarget37"])["at"]);
                    loom40PracticeNode = map.grappleNodes.Count;
                    map.grappleNodes.Add(loom40PracticeAt);
                }
                if (r.ContainsKey("loom40NoRide"))
                {
                    var nr = PortData.Obj(r["loom40NoRide"]);
                    loom40NoRide1 = Loom40Boxes(nr, "phase1");
                    loom40NoRide2 = Loom40Boxes(nr, "phase2");
                }
            }
            // the Mover's sets, built once: the level's own noride boxes (N38RideStartRun's rule) + each phase's
            l40Static = n38.Any ? Ride38.NoRideFrom(map.json) : null;
            l40Mover1 = Loom40Join(l40Static, loom40NoRide1);
            l40Mover2 = Loom40Join(l40Static, loom40NoRide2);
        }

        static Ride38.Box[] Loom40Boxes(Dictionary<string, object> nr, string key)
        {
            if (!nr.ContainsKey(key)) return null;
            var list = new List<Ride38.Box>();
            foreach (object bo in PortData.Arr(nr[key]))
            {
                var b = PortData.Obj(bo);
                list.Add(new Ride38.Box { min = PortData.V3(b["min"]), max = PortData.V3(b["max"]) });
            }
            return list.ToArray();
        }

        Ride38.Box[] Loom40Join(Ride38.Box[] a, Ride38.Box[] b)
        {
            l40Scratch.Clear();
            if (a != null) l40Scratch.AddRange(a);
            if (b != null) l40Scratch.AddRange(b);
            return l40Scratch.Count > 0 ? l40Scratch.ToArray() : null;
        }

        partial void N40LoomStep(double dt)
        {
            if (!n40.loom40 || !Loom37Level) return;
            Loom40PracticeStep();
            Loom40NoRideStep();
            if (loom40Line != null)
            {
                loom40Line.Step();
                if (loom37 == null || !loom37Owns || !loom37.Alive) loom40Line.Clear();
            }
        }

        /// <summary>Before THE LOOM's Place / StartFight: loom40's stay rule on or off for this fight; with it on, the entry line.</summary>
        void Loom40StayBegin()
        {
            if (loom37 == null) return;
            if (loom40Params == null) loom40Params = Loom40Params.From(cfg);
            loom37.Stay40(n40.loom40, loom37CanyonX0, loom37CanyonZ0, loom37CanyonX1, loom37CanyonZ1, loom40Params);
            Loom41SweepBegin(); // NIGHT41 B8 (sweep41; SurvivalSim.N41.Loom.cs): the floor sweep in or out of the brain's list for this fight
            Loom47Begin(); // NIGHT47 L (loom47; SurvivalSim.N47.Loom.cs): the new attacks, the window / chip, the seam, in or out for this fight
            if (!n40.loom40) return;
            if (loom40Line == null)
            {
                loom40Line = new BossLine40(events, step);
                events.On<Loom37Event>("loom40Latch", e => { if (n40.loom40 && loom40Line != null) loom40Line.Say("latch", Loom40Text("lineLatch", "HOOKED. NOW {grapple} PULL AWAY TO A THREAD."), loom40Params.lineS, null, 2); });
                // a tear heals (Doom Eternal's blood punch / glory kill: the aggressive move refills you): MEASURED round 3, the pilot died in leg 3
                // after 3 falls (-20 each) and a lash (-30); the tear is the fight's right move, so it pays the health back (tearHeal)
                events.On<Loom37Event>("loom37Tear", e => { if (n40.loom40 && loom37Owns && health.hp > 0) { health.hp = System.Math.Min(health.max, health.hp + loom40Params.tearHeal); loom40Heals++; } });
                events.On<Loom37Event>("loom37Open", e => { if (n40.loom40 && loom40Line != null) loom40Line.Say("open", Loom40Text("lineOpen", "OPEN: SHOOT THE CORE. EVERY GUN COUNTS."), loom40Params.lineS, null, 1); });
            }
            loom40Line.Begin("loom37", campaign?.Room?.id ?? "o3", campaign != null ? campaign.room : -1);
            if (!Loom47RuleLater()) loom40Line.Say("rule", Loom40Text("lineRule", "HOOK A LIT KNEE {grapple}. THEN {grapple} PULL AWAY."), loom40Params.lineS); // NIGHT47 L: loom47 says it after the boss's name
            Loom40NoRideWant(1);
        }

        string Loom40Text(string key, string fallback) => cfg.Has("design.n40.loom40." + key) ? cfg.Str("design.n40.loom40." + key) : fallback;

        /// <summary>Before THE LOOM's step (Loom37BossStep): the rope on a knee node latches that knee.</summary>
        void Loom40LatchStep()
        {
            if (!n40.loom40 || loom37 == null || loom37KneeNode < 0 || mover.rope.phase != RopePhase.Attached) return;
            // the anchor within the knee's radius (a knee bobs while the hook flies: the rope keeps its anchor on the node only once attached)
            for (int i = 0; i < 6; i++)
                if (!loom37.knees[i].torn && Vec3.DistanceTo(mover.rope.anchor, loom37.knees[i].at) <= Loom47LatchR(loom37.p.kneeRadius)) // NIGHT47 L: loom47's wider lock-on
                {
                    if (loom37.Latch40(i, mover.position)) Loom40LetGo(); // the hook bites and lets go: you stay where you are, the knee latched
                    break;
                }
        }

        /// <summary>The rope lets go where you are (the Mover's public release: Launch keeps your own velocity, no pull). No Mover edit.</summary>
        void Loom40LetGo()
        {
            Vec3 v = mover.velocity;
            mover.Launch(System.Math.Min(v.y, 0), v.x, v.z);
        }

        /// <summary>o2's practice knee: hooked = latched; a rope out and tearDistM farther than the nearest since = torn (back practiceBackS later).</summary>
        void Loom40PracticeStep()
        {
            if (loom40PracticeNode < 0 || loom40PracticeNode >= map.grappleNodes.Count) return;
            Vec3 me = mover.position;
            Loom37Params lp = loom37?.p;
            double tearDist = lp != null ? lp.tearDistM : cfg.Num("design.n37.loom37.tearDistM");
            int hold = MovementMath.StepsFor(lp != null ? lp.nailHoldS : cfg.Num("design.n37.loom37.nailHoldS"), step);
            if (loom40PracticeTorn)
            {
                if (--l40PracticeBack <= 0)
                {
                    loom40PracticeTorn = false;
                    Loom37MoveNode(loom40PracticeNode, loom40PracticeAt);
                    events.Emit("loom40Practice", "back");
                }
                return;
            }
            bool onIt = mover.rope.phase == RopePhase.Attached && Vec3.DistanceTo(mover.rope.anchor, loom40PracticeAt) < 1e-6;
            double d = Vec3.DistanceTo(me, loom40PracticeAt);
            if (onIt)
            {
                if (!loom40PracticeLatched) { loom40PracticeLatched = true; loom40PracticeLatches++; l40PracticeNearest = d; events.Emit("loom40Practice", "latch"); }
                l40PracticeAge = 0;
                Loom40LetGo(); // as THE LOOM's knees: the hook bites and lets go
            }
            if (!loom40PracticeLatched) return;
            if (d < l40PracticeNearest) l40PracticeNearest = d;
            if (++l40PracticeAge > hold) { loom40PracticeLatched = false; return; }
            if (mover.rope.phase != RopePhase.None && !onIt && d - l40PracticeNearest >= tearDist)
            {
                loom40PracticeLatched = false;
                loom40PracticeTorn = true;
                loom40PracticeTears++;
                l40PracticeBack = MovementMath.StepsFor(loom40Params.practiceBackS, step);
                Loom37MoveNode(loom40PracticeNode, Loom37Parked);
                events.Emit("loom40Practice", "tear");
            }
        }

        /// <summary>A phase wanted: its warning starts (no-op if it is already on or coming).</summary>
        void Loom40NoRideWant(int phase)
        {
            if (Loom40NoRideBoxes(phase) == null || phase == loom40NoRideNext || (loom40NoRideWarnLeft == 0 && phase == loom40NoRidePhase)) return;
            loom40NoRideNext = phase;
            loom40NoRideWarnTotal = loom40NoRideWarnLeft = System.Math.Max(1, MovementMath.StepsFor(loom40Params.noRideWarnS, step));
            loom40NoRideVersion++;
            events.Emit("loom40NoRide", "warn");
        }

        void Loom40NoRideStep()
        {
            bool fight = loom37 != null && loom37Owns && loom37.Alive && !loom37.Falling;
            if (!fight)
            {
                if (loom40NoRidePhase != 0 || loom40NoRideWarnLeft > 0)
                {
                    loom40NoRidePhase = loom40NoRideNext = loom40NoRideWarnLeft = 0;
                    mover.SetNoRide38(l40Static);
                    loom40NoRideVersion++;
                    events.Emit("loom40NoRide", "off");
                }
                return;
            }
            Loom40NoRideWant(loom37.brain.leg >= 2 ? 2 : 1);
            if (loom40NoRideWarnLeft > 0 && --loom40NoRideWarnLeft == 0)
            {
                loom40NoRidePhase = loom40NoRideNext;
                loom40NoRideNext = 0;
                mover.SetNoRide38(loom40NoRidePhase == 2 ? l40Mover2 : l40Mover1);
                loom40NoRideVersion++;
                events.Emit("loom40NoRide", "on");
            }
        }

        /// <summary>Level 4 as n40-span (design.n40.loom40.levelTo, a copy of n38-span without the STITCHER: Tools/night/gen-n40-span.cjs) keeps THE
        /// LOOM's rooms o1-o3: the level side runs there while loom40 is on. N37StartRun runs before N40StartRun, so the run's bit is taken the way
        /// N40StartRun takes it (a tape's override, else off when forced off, else the design's).</summary>
        bool Loom37OnSpan40(string levelId)
        {
            bool on = n40Override.HasValue ? n40Override.Value.loom40 : !N40Blocks.ForceOff && n40Design.loom40;
            if (!on) return false;
            string to = cfg.Has("design.n40.loom40.levelTo") ? cfg.Str("design.n40.loom40.levelTo") : "";
            return to.Length > 0 && levelId == to;
        }
    }
}
