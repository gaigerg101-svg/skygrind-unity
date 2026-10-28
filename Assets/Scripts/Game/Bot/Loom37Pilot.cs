// NIGHT37 builder O (NIGHT37.md O1): THE LOOM's grappling pilot on the full sim, through inputs only (shared by the harness `loom37` and the Player
// reel loom37). It crosses THE CANYON the way BOSSES.md L4 means it: look at the next thread anchor, pull (design.n35.grapple35: the hook pulls you
// straight to what it hits), land on the anchor ledge under it, walk to the ledge's north end, pull again; a fall (the sim's ledge rule) puts it
// back on the last ledge and it re-plans from there. With THE LOOM up it also fights (Loom37Pilot.Fight: nails a knee while standing on a ledge
// or hanging, then pulls AWAY to the next anchor: the tear; whips onto the body in the CYAN window).
// Buttons are frame-robust (the Player reel merges two sim steps a frame and keeps the LAST step's held): presses on 4-step boundaries.
// Numbers are bot-side: they shape one scripted player and change no rule.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Loom37Pilot
    {
        public InputFrame Move;
        public CombatInput Combat;
        /// <summary>The route: canyon anchor ids, pulled in order (the ledge under each is where it lands).</summary>
        public readonly List<string> route = new List<string> { "t1w", "t2w", "t3w", "end" };
        public int at; // index in route of the next pull
        public string phase = "aim";
        public int pulls, waits, replans, misses;
        public double airborneSteps, groundStillSteps, totalSteps;
        int phaseSteps, lastFalls;
        bool sawRope;

        static void Toward(ref InputFrame f, Vec3 me, Vec3 to, double yaw)
        {
            double cx = to.x - me.x, cz = to.z - me.z, cl = JsMath.Hypot(cx, cz);
            if (cl < 1e-6) return;
            Vec3 fwd = MovementMath.WishDirection(0, 1, yaw), right = MovementMath.WishDirection(1, 0, yaw);
            f.moveZ = (cx * fwd.x + cz * fwd.z) / cl;
            f.moveX = (cx * right.x + cz * right.z) / cl;
        }

        public static (double yaw, double pitch) LookAt(Vec3 eye, Vec3 to)
        {
            Vec3 a = to - eye;
            return (JsMath.Atan2(-a.x, -a.z), JsMath.Atan2(a.y, JsMath.Hypot(a.x, a.z)));
        }

        SurvivalSim.Loom37Anchor Anchor(SurvivalSim sim, string id)
        {
            foreach (var a in sim.loom37Anchors) if (a.id == id) return a;
            return null;
        }

        /// <summary>The route index to continue from after a fall onto `ledge` (the first pull whose anchor is past it).</summary>
        int Replan(SurvivalSim sim, SurvivalSim.Loom37Ledge ledge)
        {
            for (int i = 0; i < route.Count; i++)
            {
                var a = Anchor(sim, route[i]);
                if (a != null && a.at.z < ledge.z0 - 0.5) return i;
            }
            return route.Count - 1;
        }

        public void Step(SurvivalSim sim, int step)
        {
            Move = default;
            Combat = new CombatInput { weapon = -1 };
            totalSteps++;
            if (!sim.Loom37Level || at >= route.Count + 1) return;
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            if (sim.loom37Falls != lastFalls)
            {
                lastFalls = sim.loom37Falls;
                if (sim.loom37LastLedge != null) at = Replan(sim, sim.loom37LastLedge);
                phase = "aim";
                phaseSteps = 0;
                replans++;
            }
            if (FightStep(sim, step, me, eye)) return;
            if (wasFighting) { wasFighting = false; if (sim.loom37LastLedge != null) at = Replan(sim, sim.loom37LastLedge); phase = "aim"; phaseSteps = 0; }
            phaseSteps++;
            if (!sim.mover.grounded) airborneSteps++;
            if (at >= route.Count)
            {
                Move.yaw = 0;
                return; // across: on the end ledge
            }
            var target = Anchor(sim, route[at]);
            if (target == null) return;
            Vec3 node = target.node < sim.map.grappleNodes.Count ? sim.map.grappleNodes[target.node] : target.at;
            (double yaw, double pitch) = LookAt(eye, node);
            Move.yaw = yaw;
            Move.pitch = pitch;
            switch (phase)
            {
                case "aim":
                {
                    // walk to the north end of the ledge first (a shorter pull), then fire when a charge is in hand
                    SurvivalSim.Loom37Ledge l = sim.loom37LastLedge;
                    if (l != null && sim.mover.grounded && l.Under(me))
                    {
                        Vec3 spot = new Vec3((l.x0 + l.x1) * 0.5, l.y, l.z0 + 1.2);
                        if (JsMath.Hypot(spot.x - me.x, spot.z - me.z) > 0.8 && phaseSteps < 240) { Toward(ref Move, me, spot, yaw); break; }
                    }
                    bool ready = sim.mover.rope.phase == RopePhase.None && (!sim.Grapple32On || sim.grappleCharges32 > 0);
                    if (!ready) { waits++; if (sim.mover.grounded) groundStillSteps++; break; }
                    if (step % 4 != 0) break;
                    Move.grapple = new Button { held = true, pressed = true };
                    pulls++;
                    sawRope = false;
                    phase = "pull";
                    phaseSteps = 0;
                    break;
                }
                case "pull":
                    Move.grapple = new Button { held = true, pressed = false };
                    if (sim.mover.rope.phase != RopePhase.None) sawRope = true;
                    if (phaseSteps > 6 && sim.mover.rope.phase == RopePhase.None && !sawRope)
                    {
                        phase = "aim"; // the hook never went out (no target in the cone, not owned yet): aim again
                        phaseSteps = 0;
                        pulls--;
                        misses++;
                    }
                    else if (phaseSteps > 6 && sim.mover.rope.phase == RopePhase.None)
                    {
                        phase = "land";
                        phaseSteps = 0;
                    }
                    else if (phaseSteps > 240) { phase = "aim"; phaseSteps = 0; }
                    break;
                case "land":
                {
                    // drift on toward the anchor's ledge (the arrival stops 2.6 m short), land, then the next pull
                    Toward(ref Move, me, new Vec3(target.at.x, me.y, target.at.z - 1.5), yaw);
                    // the hook took a thread short of the ledge (the cone picked a mid-gap anchor): falling past the ledge's height, pull again
                    double under = target.ledge != null && LedgeOf(sim, target.ledge) != null ? LedgeOf(sim, target.ledge).y : target.at.y - 5;
                    if (!sim.mover.grounded && sim.mover.rope.phase == RopePhase.None && me.y < under - 1.5) { phase = "aim"; phaseSteps = 0; break; }
                    if (sim.mover.grounded && phaseSteps > 4)
                    {
                        at++;
                        phase = "aim";
                        phaseSteps = 0;
                    }
                    else if (phaseSteps > 360) { phase = "aim"; phaseSteps = 0; }
                    break;
                }
            }
        }

    }
}
