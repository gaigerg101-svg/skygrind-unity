// NIGHT47 builder L (NIGHT47.md G36-G43; Research/2026-10-18-wave47/L-cards.md): the sim side of builder L's rows (design.n47.*, "sim": true; the ids
// SurvivalSim.N47.cs resolves once a run). Fills N47LStartRun / N47LStep / N47LCampaign.
//   loom47   THE LOOM rebuilt (Core/Enemies/Loom37.N47.cs): at each fight's begin (Loom47Begin, one line in Loom40StayBegin) the row's flag and
//            numbers go into THE LOOM (the new attacks in or out of the brain's list, the window / chip, the seam). The BEND is the star: while the
//            row is on, THE LOOM's right moves refill the BEND meter (design.n34.bend34's meter, Core/Bend34.cs): a tear +bendOnTear, a parried
//            spool +bendOnParry, a phase start (boss37Leg "fight" / "start") up to bendAtPhase, so the knee hooks after them are FULL bends
//            (read today: the meter refills only on kills and THE LOOM's fight has none: every bend after the first is a 1 s SHORT one).
//            VERBS 22 (the BEND) changed by loom47: the meter also refills in THE LOOM's fight; VERBS 46 (the tear) unchanged, plus the seam.
// Off (the rows off, --off47 / -skygrindOff47): Loom47Begin hands THE LOOM `false` (nothing in its list, its numbers put back), no handler is
// added, nothing else runs. No allocation per step. No UnityEngine: SimHarness compiles this file.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Loom47Params loom47Params;
        bool l47Hooked;

        /// <summary>The harness's counters (this run): BEND meter refills by THE LOOM's right moves (tear / parry / phase).</summary>
        public int loom47BendTear, loom47BendParry, loom47BendPhase;

        // THE FRAME's falling posts (G39's state change; Tools/night/gen-n47-span.cjs: o3.loom47Posts): a world layer stood at each run's start (and
        // each fight's begin), taken away at P1 -> P2 while loom47's fight runs. A level without the key: nothing (the list stays empty).
        readonly BoxLayer loom47PostLayer = new BoxLayer();
        public readonly System.Collections.Generic.List<(Vec3 min, Vec3 max)> loom47Posts = new System.Collections.Generic.List<(Vec3, Vec3)>();
        /// <summary>The posts stand now (the view draws them); the sim step they fell at (-1: never this run; the view's fall).</summary>
        public bool loom47PostsUp;
        public int loom47PostsFellStep = -1;

        void Loom47PostsLoad()
        {
            loom47Posts.Clear();
            if (map?.json == null || !map.json.TryGetValue("campaign", out object co) || !(co is System.Collections.Generic.Dictionary<string, object> camp)
                || !camp.TryGetValue("rooms", out object ro) || !(ro is System.Collections.Generic.List<object> rooms)) return;
            foreach (object o in rooms)
                if (o is System.Collections.Generic.Dictionary<string, object> r && r.TryGetValue("loom47Posts", out object po) && po is System.Collections.Generic.List<object> posts)
                    foreach (object q in posts)
                        if (q is System.Collections.Generic.Dictionary<string, object> b) loom47Posts.Add((PortData.V3(b["min"]), PortData.V3(b["max"])));
        }

        void Loom47PostsStand(bool up)
        {
            loom47PostsUp = up && loom47Posts.Count > 0;
            if (world == null) return;
            if (loom47PostsUp) { loom47PostLayer.SetBoxes(loom47Posts); world.SetLayer("loom47Posts", loom47PostLayer, true); }
            else world.SetLayer("loom47Posts", null);
        }

        /// <summary>P1 -> P2 (boss37Leg change to 2, loom47's fight): the posts fall, their rings go dark, a player whose last hold was one comes back
        /// on the same side's P2 hold (no fall return onto air).</summary>
        void Loom47PostsFall()
        {
            if (!loom47PostsUp) return;
            Loom47PostsStand(false);
            loom47PostsFellStep = simStep;
            Loom37CutNow("t1w");
            Loom37CutNow("t1e");
            if (loom37LastLedge != null && (loom37LastLedge.id == "t1w" || loom37LastLedge.id == "t1e"))
            {
                string to = loom37LastLedge.id == "t1w" ? "t2w" : "t2e";
                foreach (Loom37Ledge l in loom37Ledges) if (l.id == to) { loom37LastLedge = l; break; }
            }
            events.Emit("loom47Cut", "posts");
        }

        int idKneeHold47 = -1, l47BendStartStep = -1000000;
        double l47KneeHoldS = 0.5;

        partial void N47LStartRun()
        {
            idKneeHold47 = rowsRegistry.IndexOf("kneeHold47");
            l47KneeHoldS = cfg.Has("design.n47.kneeHold47.numbers.holdS") ? cfg.Num("design.n47.kneeHold47.numbers.holdS") : 0.5;
            l47BendStartStep = -1000000;
            loom47BendTear = loom47BendParry = loom47BendPhase = 0;
            loom47PostsFellStep = -1;
            l47WasFalling = false;
            loom47Finales = 0;
            loom47NetCatches = 0;
            l47NetChain = 0;
            loom47NetStep = -1;
            l47RuleLeft = 0;
            Loom47PostsLoad();
            Loom47PostsStand(true);
            N47CrownStartRun();
        }

        partial void N47LStep(double dt)
        {
            N47CrownStep(dt);
            // THE FINALE BEAT (the card's transition beat for the kill; NIGHT47 section 6's R4 list "a boss finale beat"): the step THE LOOM starts
            // to fall, a numbers.finaleS hold (the sim's hitstop: the frame loop skips the frozen time; C-2's top tier and past it, once a fight)
            bool falling = loom37 != null && loom37.Falling;
            if (falling && !l47WasFalling && Loom47Live() && loom47Params.finaleS > 0) { Hitstop(loom47Params.finaleS, "loom47Finale"); events.Emit("loom47Finale", "kill"); loom47Finales++; }
            l47WasFalling = falling;
            // the rule line, after the boss's name (the lead's n47-r0d frame: the door's "THE LOOM" banner printed over "HOOK A LIT KNEE ...")
            if (l47RuleLeft > 0 && --l47RuleLeft == 0 && Loom47Live() && loom40Line != null && loom40Params != null)
                loom40Line.Say("rule", Loom40Text("lineRule", "HOOK A LIT KNEE {grapple}. THEN {grapple} PULL AWAY."), loom40Params.lineS);
        }

        int l47RuleLeft;
        bool l47WasFalling;
        /// <summary>The harness's counter (this run): finale beats held.</summary>
        public int loom47Finales;

        /// <summary>Loom40StayBegin's rule line: with loom47 on it waits numbers.ruleLineDelayS (the boss's name banner first; one message at a time,
        /// NIGHT47 section 3's text rule), counted in whole steps here.</summary>
        bool Loom47RuleLater()
        {
            l47RuleLeft = 0;
            if (!rows.On(idLoom47)) return false;
            if (loom47Params == null) loom47Params = Loom47Params.From(cfg);
            l47RuleLeft = System.Math.Max(1, MovementMath.StepsFor(loom47Params.ruleLineDelayS, step));
            return true;
        }

        partial void N47LCampaign()
        {
        }

        partial void N47CrownStartRun();
        partial void N47CrownStep(double dt);

        /// <summary>At THE LOOM's fight begin (Loom40StayBegin): loom47 in or out for this fight.</summary>
        void Loom47Begin()
        {
            if (loom37 == null) return;
            bool on = rows.On(idLoom47);
            if (on && loom47Params == null) loom47Params = Loom47Params.From(cfg);
            if (on && !l47Hooked)
            {
                l47Hooked = true;
                loom37.bendInside47 = () => Bend34Inside;
                loom37.holdKnees47 = () => rows.On(idKneeHold47) && Bend34Inside && (simStep - l47BendStartStep) * step < l47KneeHoldS;
                events.On<Bend34Event>("bend34Start", e => l47BendStartStep = simStep);
                events.On<Loom37Event>("loom37Tear", e => { if (Loom47Live()) Loom47Bend(loom47Params.bendOnTear, ref loom47BendTear); });
                events.On<Loom37Event>("loom37Spool", e => { if (e.stage == "parry" && Loom47Live()) Loom47Bend(loom47Params.bendOnParry, ref loom47BendParry); });
                events.On<Boss37Event>(Boss37Brain.EvLeg, e =>
                {
                    if (e.boss == "loom37" && e.stage == "change" && e.leg == 2 && Loom47Live()) Loom47PostsFall();
                    if (e.boss != "loom37" || (e.stage != "fight" && e.stage != "start") || !Loom47Live()) return;
                    if (Bend34On && bend34.meter < loom47Params.bendAtPhase) { bend34.meter = loom47Params.bendAtPhase; loom47BendPhase++; }
                });
            }
            loom37.Loom47(on, loom47Params);
            if (loom47Posts.Count > 0) Loom47PostsStand(true); // a fresh fight: the frame stands whole again
        }

        // THE WARP NET (round 4, the lead's ask 1: "falling twenty times is the opposite of the BEND being the star"): THE LOOM's own threads
        // stretched under the frame at numbers.netY. While loom47's fight runs, a player falling through it inside the room is caught and THROWN
        // BACK UP (netUp m/s, netSide m/s toward the nearest lit knee), for netHp, and the catch is a latch to the BEND (a slow-motion beat into a
        // hook: Bend34LatchForCheck); netChain catches in a row without standing on a hold, then the next fall goes through (the lip return).
        public int loom47NetCatches;
        /// <summary>The net's height while loom47's fight runs (NaN: no net: the view draws nothing).</summary>
        public double Loom47NetY => Loom47Live() && loom47Params != null && loom47Params.netY > -1e8 ? loom47Params.netY : double.NaN;
        int l47NetChain;
        public int loom47NetStep = -1; // the step of the last catch (the view's pluck)

        void Loom47Net(Vec3 feet)
        {
            if (!Loom47Live() || !loom37.Alive || loom37.Falling || loom47Params == null || !(loom47Params.netY > -1e8)) return;
            if (mover.grounded) { l47NetChain = 0; return; }
            if (feet.y >= loom47Params.netY || mover.velocity.y >= 0) return;
            if (feet.x < loom37CanyonX0 || feet.x > loom37CanyonX1 || feet.z < loom37CanyonZ0 || feet.z > loom37CanyonZ1) return;
            if (l47NetChain >= loom47Params.netChain) return; // it lets this one through
            l47NetChain++;
            // round 4b (MEASURED: a throw toward the nearest knee landed nowhere: 67.6 catches and 33.6 falls a fight): a ballistic throw that LANDS
            // on the last hold stood on (its centre), its apex netApexAbove over the higher of the hold's top and the net; the BEND starts as it flies
            // the hold it lands on: the phase's own pair (t<leg>w / t<leg>e), the nearer side (round 4c, MEASURED: thrown back to the last hold, seed
            // 16 sat on a later phase's hold 180 s); else the last hold stood on
            Loom37Ledge home = null;
            int leg = System.Math.Max(1, System.Math.Min(3, loom37.brain.leg));
            double hd = 1e9;
            foreach (Loom37Ledge l in loom37Ledges)
            {
                if (l.id != "t" + leg + "w" && l.id != "t" + leg + "e") continue;
                double dd = System.Math.Abs(l.at.x - feet.x) + System.Math.Abs(l.at.z - feet.z);
                if (dd < hd) { hd = dd; home = l; }
            }
            if (home == null) home = loom37LastLedge ?? (loom37Ledges.Count > 0 ? loom37Ledges[0] : null);
            double g = MovementMath.GravityFor(cfg.movement.jump.height, cfg.movement.jump.apexTime);
            double vx = 0, vz = 0, vy = loom47Params.netUp;
            if (home != null && g > 0)
            {
                double apex = System.Math.Max(home.y, feet.y) + loom47Params.netApexAbove;
                vy = System.Math.Sqrt(2 * g * (apex - feet.y));
                double t = vy / g + System.Math.Sqrt(2 * System.Math.Max(0, apex - home.y) / g);
                vx = (home.at.x - feet.x) / t;
                vz = (home.at.z - feet.z) / t;
            }
            mover.Launch(vy, vx, vz);
            if (loom47Params.netHp > 0) health.hp = System.Math.Max(1, health.hp - loom47Params.netHp);
            Bend34LatchForCheck(); // the BEND chance: the catch counts as a latch (SHORT, or FULL with the meter full)
            loom47NetCatches++;
            loom47NetStep = simStep;
            events.Emit("loom47Net", "catch");
        }

        bool Loom47Live() => loom37 != null && loom37.loom47 && loom37Owns && rows.On(idLoom47);

        /// <summary>The knee latch's radius (Loom40LatchStep: the rope's anchor within it of a knee latches that knee): numbers.latchR while
        /// loom47's fight runs (the game's forgiveness: a hook that lands near a lit, bobbing knee bites), else NIGHT46's kneeRadius.</summary>
        double Loom47LatchR(double old) => Loom47Live() && loom47Params != null ? System.Math.Max(old, loom47Params.latchR) : old;

        /// <summary>The canyon fall's health cost (SurvivalSim.Loom37.cs's fall rule): numbers.fallHp while loom47's fight runs, else NIGHT46's.</summary>
        double Loom47FallCost(double old) => Loom47Live() && loom37.Alive && loom47Params != null ? loom47Params.fallHp : old;

        void Loom47Bend(double add, ref int count)
        {
            if (!Bend34On || add <= 0) return;
            bend34.meter = Math.Min(1, bend34.meter + add);
            count++;
        }
    }
}
