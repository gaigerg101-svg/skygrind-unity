// NIGHT30 (builder S, round 3): the GRAPPLE part of the level pilot (design.n30.span; LEVEL 4 THE SPAN). Walk29Pilot asks it first in every room: when the room
// holds grapple nodes (the level's `grappleNodes` inside the room's rect) and THE ARCLINE is owned, it crosses them in order along the room's travel (toward -z,
// as every plan runs): walk to a run-up point approachBack m before the first node, run at it, press GRAPPLE aimed at the node (the hook bites the node: GrappleAim
// prefers a node in the cone), hold it through the swing and LET GO once the feet are releaseAhead m past the node (R3 / DESIGN30 s3's lesson: "SWING. LET GO AT
// THE TOP."); in the air the next node is hooked the moment the swing's 0.4 s cooldown is over. Past the last node and grounded, the autopilot takes the room.
// Pure input through the sim's public state (headless walk30 and the Player). The numbers are the pilot's own (a bot, not the game): no config.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Span30Pilot
    {
        readonly SurvivalSim sim;
        readonly List<Vec3> nodes = new List<Vec3>();
        string roomId;
        int ni, phase, steps, pressStep, tries;
        bool done;

        const double ApproachBack = 5.0, ReleaseAhead = 2.0, RoomMargin = 1.0, TopBelow = 3.5;
        double floorY;
        bool reeling, dashed, pastExit;
        int releaseStep, assistLeft;
        Vec3 lastPos;
        const int AssistSteps = 120 * 8;
        const int RunUpSteps = 30, AttachWait = 40, MaxSteps = 120 * 25, MaxTries = 4;

        /// <summary>Counters (walk30's line): node attaches seen, rooms crossed by the grapple, rooms given up.</summary>
        public int attaches, crossed, gaveUp;
        public readonly List<string> crossedRooms = new List<string>();

        /// <summary>Is `rd` a CROSSING room for the pilot (grapple nodes in a line along the travel, not a ring)? The harness's list of rooms to prove.</summary>
        public static bool IsCrossing(SurvivalMap map, RoomDef rd)
        {
            var ns = new List<Vec3>();
            foreach (Vec3 n in map.grappleNodes)
                if (n.x >= rd.x0 - RoomMargin && n.x <= rd.x1 + RoomMargin && n.z >= rd.z0 - RoomMargin && n.z <= rd.z1 + RoomMargin) ns.Add(n);
            if (ns.Count == 0) return false;
            ns.Sort((a, b) => b.z.CompareTo(a.z));
            for (int i = 1; i < ns.Count; i++) if (Math.Abs(ns[i].z - ns[i - 1].z) < 0.5) return false;
            return true;
        }
        int lastAttachEvents;

        public Span30Pilot(SurvivalSim sim)
        {
            this.sim = sim;
            sim.events.On<Arcline21RopeEvent>("ropeAttach", e => { if (e.node) attaches++; });
        }

        static double Yaw(Vec3 from, Vec3 to) => Math.Atan2(-(to.x - from.x), -(to.z - from.z));

        void Enter(RoomDef rd)
        {
            roomId = rd.id;
            nodes.Clear();
            foreach (Vec3 n in sim.map.grappleNodes)
                if (n.x >= rd.x0 - RoomMargin && n.x <= rd.x1 + RoomMargin && n.z >= rd.z0 - RoomMargin && n.z <= rd.z1 + RoomMargin) nodes.Add(n);
            nodes.Sort((a, b) => b.z.CompareTo(a.z)); // the travel runs toward -z: the largest z is met first
            ni = phase = steps = tries = assistLeft = 0;
            pastExit = false;
            pressStep = -1;
            done = nodes.Count == 0;
            // a RING of nodes (two at the same z: s7's pylon ring, the Loom's legs) is not a crossing: the room is walkable round it, so the autopilot keeps it
            for (int i = 1; i < nodes.Count && !done; i++)
                if (Math.Abs(nodes[i].z - nodes[i - 1].z) < 0.5) done = true;
        }

        /// <summary>The LAVA GUARD (round 6): the autopilot's frame with its walk cancelled when the step 1 m along its wish lands over a lava rect that lies below the
        /// feet (a fight in s6 / s7 dragged the bot into the lava floor and the pylon's moat: 121 / 60 room retries over 10 seeds). The look and the trigger stay: it
        /// keeps fighting from the edge. Counts the cancelled steps.</summary>
        InputFrame GuardLava(InputFrame auto)
        {
            Lava30 lava = sim.lava30;
            if (lava == null || lava.zones.Count == 0 || (auto.moveX == 0 && auto.moveZ == 0)) return auto;
            Vec3 p = sim.mover.position;
            double sy = JsMath.Sin(auto.yaw), cy = JsMath.Cos(auto.yaw);
            double wx = -sy * auto.moveZ + cy * auto.moveX, wz = -cy * auto.moveZ - sy * auto.moveX, wl = JsMath.Hypot(wx, wz);
            if (wl < 1e-6) return auto;
            Vec3 v = sim.mover.velocity;
            foreach (double reach in new[] { 0.6, 1.2, 2.0, 2.8, -1 })
            {
                // the wish 0.6-2.8 m ahead (a walk stops in about 1.2 m), and where the momentum carries the feet in 0.35 s (reach -1)
                var q = reach > 0 ? new Vec3(p.x + wx / wl * reach, p.y, p.z + wz / wl * reach) : new Vec3(p.x + v.x * 0.35, p.y, p.z + v.z * 0.35);
                foreach (Lava30Zone z in lava.zones)
                    if (q.x >= z.x0 && q.x <= z.x1 && q.z >= z.z0 && q.z <= z.z1 && p.y > z.y + 0.3 && !FloorAbove(q, p.y, z.y))
                    {
                        auto.moveX = auto.moveZ = 0;
                        auto.jump = default;
                        auto.dash = default;
                        lavaHolds++;
                        return auto;
                    }
            }
            return auto;
        }

        /// <summary>Is there a floor (a step, a deck, a bridge) at q above the lava surface, within a step up / a short drop of the feet? Then walking there is safe.</summary>
        bool FloorAbove(Vec3 q, double feetY, double lavaY)
        {
            var from = new Vec3(q.x, feetY + 1.2, q.z);
            if (!sim.world.RayIntersect(from, new Vec3(0, -1, 0), out RayHit h)) return false;
            double floorY = from.y - h.distance;
            return floorY > lavaY + 0.2 && floorY > feetY - 1.6;
        }

        /// <summary>Is a live enemy on the near side of the room's first node (z above it: the travel runs toward -z), or has the room's wave not spawned yet?</summary>
        bool EnemiesNearSide(double firstNodeZ)
        {
            int alive = 0;
            foreach (Enemy e in sim.enemies.all)
            {
                if (!e.alive || e.standIn29) continue;
                alive++;
                if (e.position.z > firstNodeZ) return true;
            }
            return alive == 0; // nothing alive yet: the wave is still coming (wait for it on this side)
        }

        /// <summary>Steps the lava guard cancelled the autopilot's walk.</summary>
        public int lavaHolds;

        /// <summary>This step's move frame when the grapple script drives (true); false: the caller's own script / the autopilot. `auto` is the autopilot's frame
        /// (returned guarded against walking into lava in a node room while the autopilot has it).</summary>
        public bool Drive(RoomDef rd, InputFrame auto, out InputFrame f)
        {
            if (DriveCore(rd, out f)) return true;
            if (rd == null || nodes.Count == 0 || !sim.n30.span) return false; // not a span room: the caller's own scripts, untouched
            f = GuardLava(auto);
            return true;
        }

        bool DriveCore(RoomDef rd, out InputFrame f)
        {
            f = default;
            if (rd == null) return false;
            Mover m = sim.mover;
            Vec3 p = m.position;
            // a new room, or a respawn (a room retry puts the player back at the room's spawn: a jump of metres in one step): the script starts over
            if (rd.id != roomId || JsMath.Hypot(p.x - lastPos.x, p.z - lastPos.z) > 6) Enter(rd);
            lastPos = p;
            bool grounded = m.grounded;
            // after a crossing, once the room is cleared: straight to its exit, then on to the next room's spawn (a swing can land the player where the autopilot's
            // nav graph has no node: s6's far floor behind the deck)
            if (assistLeft > 0 && sim.campaign != null && sim.campaign.phase == Campaign24.Phase.Cleared)
            {
                assistLeft--;
                int ri = sim.campaign.room;
                LevelDef lv = sim.campaignLevel;
                Vec3 goal = JsMath.Hypot(p.x - rd.exit.x, p.z - rd.exit.z) > 1.2 && !pastExit ? rd.exit : ri + 1 < lv.rooms.Count ? lv.rooms[ri + 1].spawn : rd.exit;
                if (!pastExit && JsMath.Hypot(p.x - rd.exit.x, p.z - rd.exit.z) <= 1.2) pastExit = true;
                f = new InputFrame { moveZ = 1, yaw = Yaw(p, goal) };
                return true;
            }
            if (done || !sim.n30.span || sim.arcline21 == null || !sim.arcline21.Owned) return false;
            // a FIGHT room (round 6, L's walk30 stalls in s6 / s7): the autopilot fights until the room is cleared; only then the grapple crossing (a crossing begun
            // mid-fight hung the pilot under a node or on s6's catch steps under the spitters). A link room has no fight: it crosses at once.
            // ... but only while an enemy is on THIS side (before the first node): with every live enemy across the gap (s6's spitters on the far deck) the fight
            // is over there, so the pilot crosses first (holding the entry floor under their fire cost 75 room retries over 10 seeds)
            if (rd.kind == "room" && (sim.campaign == null || sim.campaign.phase != Campaign24.Phase.Cleared) && phase == 0 && EnemiesNearSide(nodes[0].z)) return false;
            Vec3 last = nodes[nodes.Count - 1];
            if (grounded && p.z < last.z - ReleaseAhead - 0.5 && m.rope.phase == RopePhase.None)
            {
                done = true;
                crossed++;
                crossedRooms.Add(rd.id);
                assistLeft = AssistSteps;
                pastExit = false;
                return false;
            }
            if (++steps > MaxSteps || tries > MaxTries)
            {
                done = true;
                gaveUp++;
                return false;
            }
            Vec3 node = nodes[Math.Min(ni, nodes.Count - 1)];
            Vec3 eye = sim.Eye;
            f = new InputFrame { moveZ = 1 };
            if (phase == 0) // to the run-up point, on the ground
            {
                var a = new Vec3(node.x, p.y, node.z + ApproachBack);
                double d = JsMath.Hypot(p.x - a.x, p.z - a.z);
                if (d < 0.8 || p.z < a.z) { phase = 1; steps = 0; floorY = p.y; }
                else
                {
                    f.yaw = Yaw(p, a);
                    return true;
                }
            }
            double dx = node.x - eye.x, dz = node.z - eye.z, dy = node.y - eye.y;
            // the travel look (straight on past the node, level) moves the player; the press step looks AT the node (the hook's aim is the frame's look)
            f.yaw = Yaw(p, new Vec3(node.x, p.y, node.z - 10));
            f.pitch = 0;
            if (phase == 1) // the run-up, then the press
            {
                if (steps < RunUpSteps && ni == 0) return true;
                bool canFire = m.rope.phase == RopePhase.None && m.rope.cooldownSteps == 0;
                if (canFire && (pressStep < 0 || steps - pressStep > AttachWait))
                {
                    f.yaw = Math.Atan2(-dx, -dz);
                    f.pitch = Math.Atan2(dy, Math.Max(0.01, JsMath.Hypot(dx, dz)));
                    f.grapple = new Button { held = true, pressed = true };
                    pressStep = steps;
                    tries++;
                    lastAttachEvents = attaches;
                    phase = 2;
                }
                return true;
            }
            if (phase == 4) // the let-go: a jump off the rope, then one air dash along the travel, then the next node (or the far side)
            {
                if (!dashed && !grounded && steps - releaseStep >= 4 && m.velocity.y <= 0.5) // at the jump's apex: the highest point to clear a wall from
                {
                    f.dash = new Button { held = true, pressed = true };
                    dashed = true;
                }
                else if (!grounded) f.jump = new Button { held = true, pressed = false };
                if (dashed && m.state != MoveState.Dash)
                {
                    if (ni < nodes.Count - 1) { ni++; phase = 1; pressStep = -1; tries = 0; }
                    else phase = 3;
                }
                return true;
            }
            if (phase == 3) return true; // the last let-go: fly on past the node, land, then the autopilot
            // phase 2: hold through the swing; let go past the node
            if (m.rope.phase == RopePhase.None && attaches == lastAttachEvents && steps - pressStep > AttachWait)
            {
                phase = 1; // the hook missed (or the rope broke): press again
                return true;
            }
            // NIGHT35 M (grapple35, the straight pull): the pull let go by itself at the node (its arrival): the let-go as below, the jump being the air's
            // double jump (over the wall / across to the next node), then the dash
            if (sim.Grapple35Pull && m.rope.phase == RopePhase.None && attaches != lastAttachEvents)
            {
                f.jump = new Button { held = true, pressed = true };
                releaseStep = steps;
                dashed = false;
                phase = 4;
                return true;
            }
            // the rope reels the player UP to the node (a node hangs over a wall or a gap: DESIGN30 I-1, s2's 4 m wall), then lets go near the top with a jump off
            // the rope and a dash along the travel (over the wall / across to the next node)
            bool high = p.y >= node.y - TopBelow || (m.rope.phase == RopePhase.Attached && m.rope.attachedSteps > 150);
            if (high && m.rope.phase == RopePhase.Attached)
            {
                f.grapple = new Button { held = true, pressed = false }; // still held: the JUMP lets go (the Mover's jump release keeps the rising boost and adds jumpUp)
                f.jump = new Button { held = true, pressed = true };
                releaseStep = steps;
                dashed = false;
                phase = 4;
                return true;
            }
            f.grapple = new Button { held = true, pressed = false };
            // reel in all the way (the arc from a high node dips into the trench otherwise: a lava touch launches the player off the rope)
            if (m.rope.phase == RopePhase.Attached) f.reel = new Button { held = true, pressed = !reeling };
            reeling = f.reel.held;
            return true;
        }
    }
}
