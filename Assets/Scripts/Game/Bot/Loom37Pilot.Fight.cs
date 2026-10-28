// NIGHT37 builder O: Loom37Pilot's fight (THE LOOM up and fighting). Per leg L it works the tower pair L (t<L>w, t<L>e), the pair whose knees are live:
//   - off the pair: pull to t<L>w's anchor (the crossing's pull);
//   - on a tower top: STRAFE along the ledge (never standing still: rule 2 and the harness's still-damage share) and nail the knee on this side
//     until it carries nailsToTear; then pull ACROSS to the other tower's anchor (away from that knee: the tear), nailing the other knee in the air;
//   - landed with the other knee nailed: LT (the NAILGUN's detonate) tears it;
//   - the CYAN window: unload on the spinneret while strafing; a spool within 6 m is looked at and punched (the parry).
// Buttons on 4-step boundaries (the Player reel merges two sim steps a frame). Bot-side numbers only.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Loom37Pilot
    {
        public int fightPulls, detonates, punches, strafeFlips, dodges, hooks, sweepDodges;
        int fHookKnee = -1; // NIGHT40 B4 (loom40): the knee a hook under way is meant for (-1: an anchor pull)
        public double fightSteps, stillSteps;
        bool wasFighting, fpull, fSawRope, strafeNorth;
        int fpullSteps, punchCool;
        string fpullTo;

        static SurvivalSim.Loom37Ledge LedgeOf(SurvivalSim sim, string id)
        {
            foreach (var l in sim.loom37Ledges) if (l.id == id) return l;
            return null;
        }

        void Strafe(SurvivalSim sim, SurvivalSim.Loom37Ledge l, Vec3 me, double yaw)
        {
            double north = l.z0 + 1.5, south = l.z1 - 1.5; // world z: north is smaller
            if (strafeNorth && me.z <= north + 0.6) { strafeNorth = false; strafeFlips++; }
            else if (!strafeNorth && me.z >= south - 0.6) { strafeNorth = true; strafeFlips++; }
            Vec3 to = new Vec3((l.x0 + l.x1) * 0.5, me.y, strafeNorth ? north : south);
            Toward(ref Move, me, to, yaw);
            Move.sprint = new Button { held = true, pressed = false };
        }

        void Fire(SurvivalSim sim, int step, bool special = false)
        {
            int gi = sim.loadout.owned.IndexOf("nailgun");
            Combat.weapon = gi >= 0 && sim.loadout.Weapon != "nailgun" && step % 30 == 0 ? gi : -1; // once in a while: a repeated ask cancels the queued switch (Rules30)
            Combat.fire = new Button { held = true, pressed = step % 4 == 0 };
            if (special) Combat.special = new Button { held = true, pressed = step % 4 == 0 };
        }

        /// <summary>NIGHT40 B4 (L-058): the CASUAL pilot (a person who hesitates): casualWaitS on the ledge before each pull, every second spool not
        /// parried, the lash read at casualLashRead of its wind-up (the skilled pilot: 0.35). Bot-side numbers only.</summary>
        public bool casual;
        const double casualWaitS = 1.0, casualLashRead = 0.7;
        /// NIGHT41 B8: the sweep read late in its wind-up so the rope is still attached when the line passes (a pull lasts ~1 s); the casual reads later
        public static double sweepRead = 0.7, sweepReadCasual = 0.85;
        int casualGround, casualSpools;
        Loom37Spool casualLast;

        bool StartPull(SurvivalSim sim, int step, string anchorId)
        {
            if (casual && sim.mover.grounded && casualGround < (int)(casualWaitS * 120)) return false; // hesitates on a ledge, never while falling (a thread to catch)
            bool ready = sim.mover.rope.phase == RopePhase.None && (!sim.Grapple32On || sim.grappleCharges32 > 0);
            if (!ready || step % 4 != 0) return false;
            Move.grapple = new Button { held = true, pressed = true };
            casualGround = 0;
            fpull = true;
            fSawRope = false;
            fpullSteps = 0;
            fpullTo = anchorId;
            fightPulls++;
            return true;
        }

        /// <summary>The fight's step; false when THE LOOM is not up (the crossing runs).</summary>
        bool FightStep(SurvivalSim sim, int step, Vec3 me, Vec3 eye)
        {
            Loom37 L = sim.loom37;
            if (L == null || !sim.loom37Owns || !L.Alive || L.Falling || L.brain.state == Boss37State.Off) return false;
            wasFighting = true;
            fightSteps++;
            if (!sim.mover.grounded) airborneSteps++;
            if (punchCool > 0) punchCool--;
            if (sim.mover.grounded) casualGround++;
            int leg = Math.Max(1, Math.Min(3, L.brain.leg));
            string wId = "t" + leg + "w", eId = "t" + leg + "e";
            int kW = 2 * (leg - 1), kE = kW + 1;
            SurvivalSim.Loom37Ledge ledge = sim.loom37LastLedge;
            bool onPair = sim.mover.grounded && ledge != null && (ledge.id == wId || ledge.id == eId) && ledge.Under(me);
            Vec3 look = L.centre;
            bool hook40 = L.Stay40On; // NIGHT40 B4 (loom40): no nails; a knee tears by hooking it, then pulling away
            // NIGHT40 B4: a knee hook under way: hold until the knee latches, then let go (stay on the ledge for the pull-away)
            if (fpull && fHookKnee >= 0)
            {
                fpullSteps++;
                look = L.knees[fHookKnee].at;
                if (L.Latched40(fHookKnee) || L.knees[fHookKnee].torn || fpullSteps > 90 || (fpullSteps > 6 && sim.mover.rope.phase == RopePhase.None))
                {
                    if (L.Latched40(fHookKnee)) hooks++;
                    fpull = false;
                    fHookKnee = -1;
                }
                else Move.grapple = new Button { held = true, pressed = false };
                (Move.yaw, Move.pitch) = LookAt(eye, look);
                return true;
            }
            // a pull under way: hold it; once the hook is in, nail the far knee from the air
            if (fpull)
            {
                fpullSteps++;
                Move.grapple = new Button { held = true, pressed = false };
                if (sim.mover.rope.phase != RopePhase.None) fSawRope = true;
                var a = Anchor(sim, fpullTo);
                look = a != null ? sim.map.grappleNodes[a.node] : L.centre;
                if (fSawRope && hook40)
                {
                    if (L.Open) { look = L.Spinneret; Fire(sim, step); }
                }
                else if (fSawRope)
                {
                    int far = fpullTo == eId ? kE : fpullTo == wId ? kW : -1;
                    if (far >= 0 && !L.knees[far].torn) { look = L.knees[far].at; Fire(sim, step); }
                    else if (L.Open) { look = L.Spinneret; Fire(sim, step); }
                }
                if ((fpullSteps > 6 && sim.mover.rope.phase == RopePhase.None && !fSawRope) || fpullSteps > 360) fpull = false;
                else if (fSawRope && sim.mover.rope.phase == RopePhase.None) fpull = false;
                (Move.yaw, Move.pitch) = LookAt(eye, look);
                return true;
            }
            // NIGHT41 B8 (sweep41): THE SWEEP (RED, the floor decal): in its band and under its top late in the wind-up (the lash's read): get height,
            // a pull across the pair (or to the pair's anchor): the rope attached or the feet over the top when the line passes
            if (L.SweepTelling && L.brain.Progress > (casual ? sweepReadCasual : sweepRead) && me.z >= L.sweepZ0 && me.z <= L.sweepZ1 && me.y < L.sweepTopY + 3
                && sim.mover.rope.phase == RopePhase.None)
            {
                string to = onPair ? (ledge.id == wId ? eId : wId) : (ledge != null && ledge.id.EndsWith("e") ? eId : wId);
                var a3 = Anchor(sim, to);
                if (a3 != null && !a3.dark)
                {
                    (Move.yaw, Move.pitch) = LookAt(eye, sim.map.grappleNodes[a3.node]);
                    if (StartPull(sim, step, to)) { sweepDodges++; return true; }
                }
            }
            // a spool close: look at it and punch it (the parry)
            Loom37Spool near = null;
            double nd = 6;
            foreach (var s in L.spools) { if (!s.alive || s.back) continue; double d = Vec3.DistanceTo(s.pos, eye); if (d < nd) { nd = d; near = s; } }
            if (near != null)
            {
                look = near.pos;
                if (casual && near != casualLast) { casualLast = near; casualSpools++; }
                if (casual && casualSpools % 2 == 0) { (Move.yaw, Move.pitch) = LookAt(eye, L.centre); if (onPair) Strafe(sim, ledge, me, Move.yaw); return true; } // this one is missed
                if (nd < 3.0 && punchCool == 0) { Combat.punch = new Button { held = true, pressed = true }; punchCool = 12; punches++; }
                (Move.yaw, Move.pitch) = LookAt(eye, look);
                if (onPair) Strafe(sim, ledge, me, Move.yaw);
                return true;
            }
            // THE LASH (RED): on its line late in the wind-up: get off it (pull across the pair, else run sideways)
            if (L.LashLive && L.brain.state == Boss37State.Tell && L.brain.Progress > (casual ? casualLashRead : 0.35))
            {
                Vec3 d = L.lashTo - L.lashFrom;
                double len = Math.Max(1e-6, d.Length);
                Vec3 u = d / len, rel = new Vec3(me.x, me.y + 0.9, me.z) - L.lashFrom;
                double t = Vec3.Dot(rel, u);
                double off = Vec3.DistanceTo(L.lashFrom + u * t, new Vec3(me.x, me.y + 0.9, me.z));
                if (off < L.p.lashWidthM + 1.5)
                {
                    dodges++;
                    if (onPair)
                    {
                        string to = ledge.id == wId ? eId : wId;
                        var a2 = Anchor(sim, to);
                        if (a2 != null && !a2.dark)
                        {
                            (Move.yaw, Move.pitch) = LookAt(eye, sim.map.grappleNodes[a2.node]);
                            if (StartPull(sim, step, to)) return true;
                        }
                    }
                    Vec3 side = new Vec3(-u.z, 0, u.x);
                    if (ledge != null && ledge.Under(me) && me.x + side.x * 2 > ledge.x1) side = side * -1;
                    if (ledge != null && ledge.Under(me) && me.x + side.x * 2 < ledge.x0) side = side * -1;
                    (Move.yaw, Move.pitch) = LookAt(eye, L.centre);
                    Toward(ref Move, me, me + side * 4, Move.yaw);
                    Move.sprint = new Button { held = true };
                    return true;
                }
            }
            if (!onPair)
            {
                // get to the pair: pull to its west anchor (or wait for a charge, drifting on the ledge)
                string go = ledge != null && ledge.id.EndsWith("e") ? eId : wId; // same side: the straight pull clears the mid-gap threads (the cone would take one)
                var a = Anchor(sim, go);
                if (a == null) return false;
                look = sim.map.grappleNodes[a.node];
                (Move.yaw, Move.pitch) = LookAt(eye, look);
                if (sim.mover.grounded && ledge != null && ledge.Under(me)) Strafe(sim, ledge, me, Move.yaw);
                bool falling = !sim.mover.grounded && me.y < 6.5; // below the tower tops: catch the next thread now
                if ((sim.mover.grounded && L.brain.state != Boss37State.LegChange) || falling) StartPull(sim, step, go); // in the air over a ledge: land first (an across pull ends 2.6 m short) // the Loom moves its knees through the gap in a leg change: wait on the ledge
                return true;
            }
            bool west = ledge.id == wId;
            int mine = west ? kW : kE, other = west ? kE : kW;
            if (L.Open)
            {
                look = L.Spinneret;
                Fire(sim, step);
            }
            else if (hook40 && L.brain.Fighting && L.brain.state != Boss37State.LegChange && (!L.knees[mine].torn || !L.knees[other].torn))
            {
                // NIGHT40 B4: hook this side's knee (it latches), then pull ACROSS to the other tower's anchor: away from it, the tear
                if (!L.knees[mine].torn && !L.Latched40(mine))
                {
                    look = L.knees[mine].at;
                    (Move.yaw, Move.pitch) = LookAt(eye, look);
                    Strafe(sim, ledge, me, Move.yaw);
                    if (StartPull(sim, step, null)) { fHookKnee = mine; Move.yaw = LookAt(eye, look).yaw; }
                    return true;
                }
                var a = Anchor(sim, west ? eId : wId);
                if (a != null && !a.dark)
                {
                    look = sim.map.grappleNodes[a.node];
                    (Move.yaw, Move.pitch) = LookAt(eye, look);
                    Strafe(sim, ledge, me, Move.yaw);
                    StartPull(sim, step, west ? eId : wId);
                    return true;
                }
            }
            else if (hook40) { }
            else if (L.brain.Fighting && !L.knees[mine].torn && L.knees[mine].nails < L.p.nailsToTear)
            {
                look = L.knees[mine].at;
                Fire(sim, step);
            }
            else if (L.brain.Fighting && !L.knees[other].torn)
            {
                // pull across, away from this knee (it tears on the way)
                var a = Anchor(sim, west ? eId : wId);
                if (a != null)
                {
                    look = sim.map.grappleNodes[a.node];
                    (Move.yaw, Move.pitch) = LookAt(eye, look);
                    Strafe(sim, ledge, me, Move.yaw);
                    StartPull(sim, step, west ? eId : wId);
                    return true;
                }
            }
            else if (L.brain.Fighting && !L.knees[mine].torn && L.knees[mine].nails >= L.p.nailsToTear)
            {
                look = L.knees[mine].at;
                Fire(sim, step, true); // LT: the detonate tears it
                if (step % 4 == 0) detonates++;
            }
            (Move.yaw, Move.pitch) = LookAt(eye, look);
            Strafe(sim, ledge, me, Move.yaw);
            if (sim.mover.grounded && JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z) < 1) stillSteps++;
            return true;
        }
    }
}
