// NIGHT31 builder G (round 3, item 4): the pilot of the grapple CHAIN on n30-span s5 -> s7 (pull, wall ride, pull), shared by the harness (chain31) and the
// Player reel (FeelReel.N31.Grapple.cs ride31), so the route the reel plays is the route the harness proved. It only makes input frames (look, forward, the
// grapple button: a TAP = the press, then up 4 steps after the hook is out); the sim's own latch turns a tap into a pull. No UnityEngine.
//   0 walk from s5 into s6, then a TAP at s6's WEST wall 12 m ahead (pull 1, a surface) -> the wall ride (forward held, the look along the room)
//   1 from the ride: a TAP at s6's next node ahead (pull 2)
//   2 after the arrival: on toward s7's door until one of s7's nodes is in view within 35 m -> TAP (pull 3)
// (round 3's first route pulled to s6's first node first: the kept speed and the 0.4 s swing cooldown carried the player 16 m past the wall lane before
// the next press could fire, so the wall comes first.)
// Each aim holds `dwellSteps` (and, in the Player, until the cyan mark is drawn) before the press, as a player aims first.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Chain31Pilot
    {
        public int phase; // 0..2 as above; 3+ = done (4 once seen)
        public int presses, failedPresses;
        public readonly List<string> log = new List<string>();
        public int dwellSteps = 12;

        readonly RoomDef s6, s7;
        readonly List<Vec3> doors;
        int pressStep = -1, firedStep = -1, dwell, aimKey = -99;
        Vec3 aim;
        Vec3? lastTarget;

        public Chain31Pilot(LevelDef level, List<Vec3> doorCentres)
        {
            s6 = level.rooms.Find(r => r.id == "s6");
            s7 = level.rooms.Find(r => r.id == "s7");
            doors = doorCentres;
        }

        public bool Ready => s6 != null && s7 != null;

        static bool Sees(SurvivalSim sim, Vec3 from, Vec3 to)
        {
            Vec3 t = to - from;
            double len = t.Length;
            return !(sim.world.RayIntersect(from, t / len, out RayHit hit) && hit.distance < len - 0.4);
        }

        static void Look(SurvivalSim sim, Vec3 at, ref InputFrame f)
        {
            Vec3 d = at - sim.Eye;
            f.yaw = Math.Atan2(-d.x, -d.z);
            f.pitch = Math.Asin(d.y / Math.Max(1e-6, d.Length));
        }

        List<Vec3> NodesIn(SurvivalSim sim, RoomDef r)
        {
            var l = new List<Vec3>();
            foreach (Vec3 n in sim.map.grappleNodes)
                if (n.x >= r.x0 - 1 && n.x <= r.x1 + 1 && n.z >= r.z0 - 1 && n.z <= r.z1 + 1) l.Add(n);
            l.Sort((a, b) => b.z.CompareTo(a.z)); // the course order (toward -z)
            return l;
        }

        /// <summary>The next step's (or frame's) input. `markShown`: the Player's cyan mark is drawn this frame (the harness passes the pick's Peek).</summary>
        public InputFrame Next(SurvivalSim sim, bool markShown)
        {
            var f = new InputFrame();
            Rope r = sim.mover.rope;
            int now = sim.simStep;
            // a press in progress: pressed until the hook is out, then held 4 steps, then up (the tap)
            if (pressStep >= 0)
            {
                Look(sim, aim, ref f);
                if (firedStep < 0 && r.phase != RopePhase.None) firedStep = now;
                if (firedStep < 0 && now - pressStep > 30)
                {
                    failedPresses++;
                    log.Add($"phase {phase}: the press at step {pressStep} fired nothing");
                    pressStep = -1;
                    return f;
                }
                f.grapple = new Button { held = firedStep < 0 || now - firedStep < 4, pressed = firedStep < 0 };
                if (firedStep >= 0 && r.phase == RopePhase.None && now - firedStep > 4)
                {
                    pressStep = -1; // the pull is over (arrived, or let go)
                    phase++;
                }
                return f;
            }
            if (phase >= 4) { f.yaw = sim.input.Yaw; return f; }
            bool idle = r.phase == RopePhase.None && r.cooldownSteps == 0;
            Vec3 eye = sim.Eye;
            Vec3? want = null;
            if (phase == 0)
            {
                // inside s6: the WEST wall 12 m ahead at the eye's height + 1.5 m (the ray from the eye finds the face): a pull along the wall lane
                if (sim.mover.position.z < s6.z1 - 1)
                {
                    var guess = new Vec3(s6.x0 - 1, eye.y + 1.5, sim.mover.position.z - 12);
                    Vec3 d = (guess - eye) / Vec3.DistanceTo(guess, eye);
                    if (sim.world.RayIntersect(eye, d, out RayHit h) && h.distance < 30) want = eye + d * h.distance;
                }
            }
            else if (phase == 1)
            {
                // on the ride (or after it): s6's next node AHEAD, in view
                Vec3? best = null;
                foreach (Vec3 n in NodesIn(sim, s6)) if (n.z < sim.mover.position.z - 2 && Sees(sim, eye, n)) { best = n; break; }
                // none ahead (the ride outran them during the 0.4 s swing cooldown: 16 m at 40 m/s): s7's nodes, through the door
                if (best == null) foreach (Vec3 n in NodesIn(sim, s7)) if (Vec3.DistanceTo(n, eye) < 35 && Sees(sim, eye, n)) { best = n; break; }
                want = best;
                if (want == null && !sim.mover.grounded) { f.yaw = 0; f.moveZ = 1; return f; } // ride on (forward along the room; a stick toward the door would end the ride 'away')
            }
            else if (phase == 2)
            {
                foreach (Vec3 n in NodesIn(sim, s7))
                    if ((!lastTarget.HasValue || Vec3.DistanceTo(n, lastTarget.Value) > 1) && Vec3.DistanceTo(n, eye) < 35 && Sees(sim, eye, n)) { want = n; break; }
            }
            if (phase >= 3) { phase = 4; return f; }
            if (want == null)
            {
                // walk on: the nearest door ahead (or straight on)
                Vec3? wp = null;
                foreach (Vec3 dc in doors) if (dc.z < sim.mover.position.z - 0.3 && (!wp.HasValue || dc.z > wp.Value.z)) wp = dc;
                if (wp.HasValue) Look(sim, new Vec3(wp.Value.x, eye.y, wp.Value.z - 2), ref f);
                f.pitch = 0;
                f.moveZ = 1;
                dwell = 0;
                return f;
            }
            aim = want.Value;
            Look(sim, aim, ref f);
            // keep the run going while aiming; on a wall ride the stick is let go (aiming at a node off the wall with the stick forward ends the ride 'away')
            if ((phase == 2 || phase == 1) && sim.mover.state != MoveState.WallRide) f.moveZ = 1;
            int key = phase * 1000 + (int)Math.Round(aim.z);
            if (key != aimKey) { aimKey = key; dwell = 0; }
            dwell++;
            if (idle && dwell >= dwellSteps && markShown)
            {
                pressStep = now;
                firedStep = -1;
                presses++;
                lastTarget = aim;
                f.grapple = new Button { held = true, pressed = true };
                log.Add($"phase {phase}: TAP at ({aim.x:0.0}, {aim.y:0.0}, {aim.z:0.0}) step {now}, speed in {sim.mover.velocity.Length:0.0} m/s, state {sim.mover.state}");
            }
            return f;
        }
    }
}
