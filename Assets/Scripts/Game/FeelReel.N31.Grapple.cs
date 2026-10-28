// NIGHT31 builder G: the Player scenarios of the grapple rows (the names are FeelReel.N27.cs PhaseN31G; the body is the partial N31GrappleBody). Each writes
// PASS or RED lines into res with the numbers, on the player's own input path (g.forcedMove goes through the same reader the pad and the keyboard feed, then
// the sim's tap latch: no code-set rope, L-031). Not run by its author (no Player in the worktree).
//   grapple31  boot -skygrindMap n30-span. CAMPAIGN, DOORFREE, the ARCLINE owned. From s2's spawn: three node pulls in a row (s2's node, then s3's two over the
//              lava gap), each a TAP (the press, then up 4 sim steps after the hook is out), aimed at the next node along the course in view (walking toward
//              the next door when none is). Lines: the speed toward the node 0.3 s after each press, the speed out at each arrival, the best speed carried
//              into the next hook, the mark shown before each press (Grapple31View: kind Node), the FOV kick and beads seen during the pulls, and the frames
//              to look at (mark before press 1, mid pull 1, arrival 1). RED with -skygrindOff31 (no pull: a tap lets go at once).
//   ring31     the same level: a frame aimed at the first node from 4.5 m (P0 b): GrappleNodes30's rings drawn, the aimed one, and Arcline21View's cubes hidden
//              (design.n31.arcRing); RED with the row off / -skygrindOff31 (the cubes drawn over the ring: T-34).
//   edge31     any map, SURVIVAL: health at 20 % for 1.5 s; the frame's centre and edge luminance and redness (r - (g + b) / 2), with the lowHealthEdge row
//              as configured and forced off (LowHealthEdge31.ReelOverride), so one run prints both (P0 d).
//   spawn31    builder L's (not here: NOT WRITTEN stays its line until L writes it).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N31GrappleBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "grapple31") body = Grapple31Scenario(g, dir, res, offs, pre);
            else if (s == "ring31") body = Ring31Scenario(g, dir, res, offs, pre);
            else if (s == "edge31") body = Edge31Scenario(g, dir, res, offs, pre);
            else if (s == "ride31") body = Ride31Scenario(g, dir, res, offs, pre); // round 3: needs "ride31" in PhaseN31G (FeelReel.N27.cs, the lead's line)
        }

        // ======================================================= ride31 =======================================================

        /// <summary>Round 3 (item 4): n30-span s5 -> s7 on the real input path, Chain31Pilot's route (the harness chain31's): a TAP at s6's west wall, the wall ride,
        /// then TAPs at the next nodes into s7. Prints each tap, what the hook took, each arrival's speed and frame, each ride's start speed and end reason.</summary>
        static IEnumerator Ride31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            yield return StartCampaign30(g, offs);
            double t0 = sim.SimTime;
            while (sim.SimTime - t0 < 1.0) { God(g); yield return null; }
            LevelDef lv = sim.campaignLevel;
            RoomDef s5 = lv.rooms.Find(r => r.id == "s5");
            var doors = new List<Vec3>();
            if (sim.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("doors", out object dO) && dO is List<object> dl)
                foreach (object o in dl)
                    if (o is Dictionary<string, object> dd && dd.TryGetValue("rect", out object rO) && rO is List<object> rc && rc.Count == 4)
                        doors.Add(new Vec3((PortData.Num(rc[0]) + PortData.Num(rc[2])) / 2, 0, (PortData.Num(rc[1]) + PortData.Num(rc[3])) / 2));
            var pilot = new Chain31Pilot(lv, doors);
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, n31.grapplePull {sim.n31.grapplePull}, owned {sim.arcline21 != null && sim.arcline21.Owned}";
            if (s5 == null || !pilot.Ready) { res[0] = "RED: the booted level has no s5 / s6 / s7 (boot -skygrindMap n30-span) | " + pc; yield break; }
            g.Respawn(s5.spawn, s5.spawnYawDeg);
            double t1 = sim.SimTime;
            while (sim.SimTime - t1 < 0.4) { God(g); yield return null; }
            var lines = new List<string>();
            int arrivals = 0, rides = 0;
            offs.Add(g.events.On<GrapplePull31Event>("grapplePull", e => lines.Add($"pull ({(e.node ? "Node" : "Surface")})")));
            offs.Add(g.events.On<GrapplePull31Event>("grappleArrive", e => { arrivals++; lines.Add($"ARRIVE {e.speed.ToString("0.0", ci)} m/s f{FeelProbe.FrameCount:0000}"); }));
            Action<MoverEvent> onMover = e =>
            {
                if (e.type == MoverEventType.WallRideStart) { rides++; lines.Add($"RIDE {e.speed.ToString("0.0", ci)} m/s f{FeelProbe.FrameCount:0000}"); }
                else if (e.type == MoverEventType.WallRideEnd) lines.Add($"ride end '{e.reason}' {e.speed.ToString("0.0", ci)} m/s");
            };
            sim.mover.Events += onMover;
            offs.Add(() => sim.mover.Events -= onMover);
            Start(g, "ride31", "camera", dir);
            yield return Frames(pre);
            double tEnd = sim.SimTime + 15;
            while (sim.SimTime < tEnd && pilot.phase < 4)
            {
                God(g);
                InputFrame f = pilot.Next(sim, Grapple31View.MarkShown);
                g.input.SetLook(f.yaw, f.pitch);
                g.forcedMove = f;
                yield return null;
            }
            g.forcedMove = new InputFrame();
            yield return Frames(Ms(300));
            yield return Stop();
            bool ok = arrivals >= 3 && rides >= 1;
            res[0] = $"{(ok ? "PASS" : "RED")}: taps {pilot.presses} ({pilot.failedPresses} fired nothing), arrivals {arrivals}, wall rides {rides} | {string.Join("; ", lines)} | taps: {string.Join("; ", pilot.log)} | {pc}";
        }

        static (double yaw, double pitch) LookAt31(SurvivalGame g, Vec3 at)
        {
            Vec3 d = at - g.sim.Eye;
            return (Math.Atan2(-d.x, -d.z), Math.Asin(d.y / Math.Max(1e-6, d.Length)));
        }

        static bool Sees31(SurvivalGame g, Vec3 from, Vec3 to)
        {
            Vec3 t = to - from;
            double len = t.Length;
            return !(g.world.RayIntersect(from, t / len, out RayHit hit) && hit.distance < len - 0.4);
        }

        // ======================================================= grapple31 =======================================================

        static IEnumerator Grapple31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            if (sim.map.grappleNodes.Count < 3) { res[0] = $"RED: the booted map '{g.MapId}' has {sim.map.grappleNodes.Count} grapple nodes (boot -skygrindMap n30-span)"; yield break; }
            yield return StartCampaign30(g, offs);
            double t0 = sim.SimTime;
            while (sim.SimTime - t0 < 1.0) { God(g); yield return null; }
            bool owned = sim.arcline21 != null && sim.arcline21.Owned;
            string pc = Preconditions(g, g.Feet) + $" | n31.grapplePull {sim.n31.grapplePull}, grappleMark row {(!N31Blocks.ForceOff && g.cfg.Design("n31.grappleMark.enabled"))}, zip speed {g.cfg.movement.arcline.zip.reelSpeed.ToString("0", ci)} m/s, tap steps {sim.grappleTap31.tapSteps}";
            if (!owned) { res[0] = "RED: the ARCLINE is not owned 1 s into the level | " + pc; yield break; }
            // s2: the room whose rect holds the first node in the level's order
            RoomDef start = null;
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                foreach (Vec3 n in sim.map.grappleNodes)
                    if (n.x >= r.x0 - 1 && n.x <= r.x1 + 1 && n.z >= r.z0 - 1 && n.z <= r.z1 + 1) { start = r; break; }
                if (start != null) break;
            }
            if (start == null) { res[0] = "RED: no room holds a node | " + pc; yield break; }
            g.Respawn(start.spawn, start.spawnYawDeg);
            double t1 = sim.SimTime;
            while (sim.SimTime - t1 < 0.5) { God(g); yield return null; }
            var arriveOut = new List<double>();
            int arrivals = 0;
            offs.Add(g.events.On<GrapplePull31Event>("grappleArrive", e => { arrivals++; arriveOut.Add(e.speed); }));
            var tookKind = new List<string>(); // what each pull's hook bit (grapplePull's node flag): the pick itself, not the mark
            offs.Add(g.events.On<GrapplePull31Event>("grapplePull", e => tookKind.Add(e.node ? "Node" : "Surface")));
            int dwellOn = -1, dwell = 0;
            // the doors' centres (the walk between rooms when no node is in view)
            var doors = new List<Vec3>();
            if (sim.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("doors", out object dO) && dO is List<object> dl)
                foreach (object o in dl)
                    if (o is Dictionary<string, object> dd && dd.TryGetValue("rect", out object rO) && rO is List<object> rc && rc.Count == 4)
                        doors.Add(new Vec3((PortData.Num(rc[0]) + PortData.Num(rc[2])) / 2, 0, (PortData.Num(rc[1]) + PortData.Num(rc[3])) / 2));
            Start(g, "grapple31", "camera", dir);
            yield return Frames(pre);
            var used = new HashSet<int>();
            double lastZ = double.MaxValue;
            var toward03 = new List<double>(); // NaN: the pull had already arrived at 0.3 s
            var towardMax = new List<double>(); // the best speed toward the node while roped, per press
            var speedIn = new List<double>();
            var markBefore = new List<string>();
            int presses = 0, pressStep = -1, firedStep = -1, target = -1, sampled = -1;
            int frameMark1 = -1, frameMid1 = -1, frameArrive1 = -1;
            float fovMax = 0;
            int beadsMax = 0;
            int at03 = MovementMath.StepsFor(0.3, sim.step);
            double tEnd = sim.SimTime + 20;
            while (sim.SimTime < tEnd && arrivals < 3)
            {
                God(g);
                Rope r = sim.mover.rope;
                Vec3 eye = sim.Eye;
                InputFrame f = new InputFrame();
                bool idle = r.phase == RopePhase.None && r.cooldownSteps == 0;
                if (pressStep >= 0 && firedStep < 0 && r.phase != RopePhase.None) firedStep = sim.simStep;
                if (pressStep >= 0 && (firedStep >= 0 || sim.simStep - pressStep > 30)) // the press is spent (the hook is out, or nothing fired in 0.25 s)
                {
                    bool held = firedStep >= 0 && sim.simStep - firedStep < 4;
                    Vec3 tn = sim.map.grappleNodes[target];
                    (f.yaw, f.pitch) = LookAt31(g, tn);
                    f.grapple = new Btn { held = held, pressed = false };
                    Vec3 dn = tn - sim.mover.RopePoint;
                    double towardNow = Vec3.Dot(sim.mover.velocity, dn) / Math.Max(1e-6, dn.Length);
                    if (r.phase == RopePhase.Attached)
                    {
                        while (towardMax.Count < presses) towardMax.Add(0);
                        towardMax[presses - 1] = Math.Max(towardMax[presses - 1], towardNow);
                    }
                    if (sampled < presses && (sim.simStep - pressStep >= at03 || (firedStep >= 0 && r.phase == RopePhase.None)))
                    {
                        // 0.3 s after the press, or the pull's last frame when it arrived sooner (s2's node is 11 m off: 44 m/s arrives in about 0.26 s)
                        toward03.Add(r.phase == RopePhase.None ? double.NaN : towardNow);
                        sampled = presses;
                        if (presses == 1) frameMid1 = FeelProbe.FrameCount;
                    }
                    if (r.phase == RopePhase.None && sim.simStep - pressStep > at03 + 2) pressStep = -1; // this pull is over
                }
                else if (pressStep >= 0)
                {
                    Vec3 tn = sim.map.grappleNodes[target];
                    (f.yaw, f.pitch) = LookAt31(g, tn);
                    f.grapple = new Btn { held = true, pressed = true }; // pressed until the hook is out (a frame may hold no sim step)
                }
                else
                {
                    int next = -1;
                    double bestZ = double.MinValue;
                    for (int i = 0; i < sim.map.grappleNodes.Count; i++)
                    {
                        Vec3 n = sim.map.grappleNodes[i];
                        if (used.Contains(i) || n.z > lastZ - 1 || Vec3.DistanceTo(n, eye) > g.cfg.movement.arcline.range * 0.9 || !Sees31(g, eye, n)) continue;
                        if (n.z > bestZ) { bestZ = n.z; next = i; }
                    }
                    if (next >= 0)
                    {
                        (f.yaw, f.pitch) = LookAt31(g, sim.map.grappleNodes[next]);
                        // round 2 (r2 printed [Surface, Surface, none]: the mark read here is LAST frame's, drawn with last frame's look): aim first, as a player
                        // does, and press once the mark has said Node on an aimed frame (or after 0.3 s of aiming)
                        if (next != dwellOn) { dwellOn = next; dwell = 0; }
                        dwell++;
                        bool markOnNode = dwell > 1 && Grapple31View.MarkShown && Grapple31View.MarkKind == Arcline21Kind.Node;
                        if (idle && (markOnNode || dwell > Ms(300)))
                        {
                            markBefore.Add(Grapple31View.MarkShown ? Grapple31View.MarkKind.ToString() : "none");
                            if (presses == 0) frameMark1 = FeelProbe.FrameCount;
                            target = next;
                            used.Add(next);
                            lastZ = sim.map.grappleNodes[next].z;
                            speedIn.Add(sim.mover.velocity.Length);
                            presses++;
                            pressStep = sim.simStep;
                            firedStep = -1;
                            f.grapple = new Btn { held = true, pressed = true };
                        }
                    }
                    else
                    {
                        Vec3? wp = null;
                        foreach (Vec3 dc in doors) if (dc.z < sim.mover.position.z - 0.3 && (!wp.HasValue || dc.z > wp.Value.z)) wp = dc;
                        if (wp.HasValue) (f.yaw, f.pitch) = LookAt31(g, new Vec3(wp.Value.x, eye.y, wp.Value.z - 2));
                        else (f.yaw, f.pitch) = (g.input.yaw, 0);
                        f.moveZ = 1;
                    }
                }
                g.input.SetLook(f.yaw, f.pitch);
                g.forcedMove = f;
                fovMax = Mathf.Max(fovMax, Grapple31View.FovKickNow);
                beadsMax = Math.Max(beadsMax, Grapple31View.BeadsShown);
                if (arrivals >= 1 && frameArrive1 < 0) frameArrive1 = FeelProbe.FrameCount;
                yield return null;
            }
            g.forcedMove = new InputFrame();
            yield return Frames(Ms(300));
            yield return Stop();
            string L(List<double> l) => string.Join(" / ", l.ConvertAll(v => v.ToString("0.0", ci)));
            double carried = -1;
            for (int i = 1; i < speedIn.Count; i++) carried = Math.Max(carried, speedIn[i]);
            int pulls = sim.grappleTap31.pulls;
            double bestToward = 0;
            foreach (double v in towardMax) bestToward = Math.Max(bestToward, v);
            bool ok = arrivals >= 3 && pulls >= 3 && bestToward > 0.8 * g.cfg.movement.arcline.zip.reelSpeed && tookKind.Count >= 3 && !tookKind.Contains("Surface");
            res[0] = $"{(ok ? "PASS" : "RED")}: presses {presses}, pulls {pulls}, arrivals {arrivals}; speed toward the node 0.3 s after each press {L(toward03)} m/s (NaN: already arrived); the best while roped per press {L(towardMax)} m/s; out at each arrival {L(arriveOut)} m/s; speed in at each press {L(speedIn)} m/s (best carried into a next hook {carried.ToString("0.0", ci)}); mark before each press [{string.Join(", ", markBefore)}]; the hook took [{string.Join(", ", tookKind)}]; FOV kick max {fovMax.ToString("0.0", ci)} deg, beads max {beadsMax} | frames: mark before press 1 f{frameMark1:0000}, 0.3 s into pull 1 f{frameMid1:0000}, arrival 1 f{frameArrive1:0000} | {pc}";
        }

        // ======================================================= ring31 =======================================================

        static IEnumerator Ring31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            if (sim.map.grappleNodes.Count == 0) { res[0] = $"RED: the booted map '{g.MapId}' has no grapple nodes (boot -skygrindMap n30-span)"; yield break; }
            yield return StartCampaign30(g, offs);
            double t0 = sim.SimTime;
            while (sim.SimTime - t0 < 1.0) { God(g); yield return null; }
            Vec3 node = sim.map.grappleNodes[0];
            RoomDef nodeRoom = null;
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                foreach (Vec3 n in sim.map.grappleNodes)
                    if (n.x >= r.x0 - 1 && n.x <= r.x1 + 1 && n.z >= r.z0 - 1 && n.z <= r.z1 + 1) { node = n; nodeRoom = r; break; }
                if (nodeRoom != null) break;
            }
            var stand = new Vec3(node.x, nodeRoom != null ? nodeRoom.spawn.y : g.Feet.y, node.z + 4.5);
            g.Respawn(stand, 0);
            double t1 = sim.SimTime;
            while (sim.SimTime - t1 < 0.4) { God(g); yield return null; }
            Start(g, "ring31", "camera", dir);
            yield return Frames(pre);
            int frameAimed = -1, aimedFrames = 0, hiddenFrames = 0, frames = 0, ringsMax = 0, markNode = 0;
            double t2 = sim.SimTime;
            while (sim.SimTime - t2 < 1.0)
            {
                God(g);
                (double yaw, double pitch) = LookAt31(g, node);
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                yield return null;
                frames++;
                ringsMax = Math.Max(ringsMax, GrappleNodes30.RingsShown);
                if (GrappleNodes30.RingAimed) { aimedFrames++; if (frameAimed < 0 && frames > 10) frameAimed = FeelProbe.FrameCount - 1; }
                if (Arcline21View.CubesHidden31Now) hiddenFrames++;
                if (Grapple31View.MarkShown && Grapple31View.MarkKind == Arcline21Kind.Node) markNode++;
            }
            g.forcedMove = new InputFrame();
            yield return Stop();
            bool ok = ringsMax > 0 && aimedFrames > 0 && hiddenFrames == frames;
            res[0] = $"{(ok ? "PASS" : "RED")}: rings drawn {ringsMax}, aimed in {aimedFrames} of {frames} frames, the old cubes hidden in {hiddenFrames} of {frames} frames (design.n31.arcRing {(!N31Blocks.ForceOff && g.cfg.Design("n31.arcRing.enabled"))}), the cyan mark on the node in {markNode} frames | look at f{frameAimed:0000} (the node from 4.5 m: one ring, no cube over it, the diamond framing it) | node ({node.x.ToString("0.0", ci)}, {node.y.ToString("0.0", ci)}, {node.z.ToString("0.0", ci)}) in {(nodeRoom != null ? nodeRoom.id : "?")}";
        }

        // ======================================================= edge31 =======================================================

        /// <summary>The frame's mean luminance and redness (r - (g + b) / 2) in the centre box (the middle 20 % each way) and the outer band (8 % on each side).</summary>
        static void EdgeRead31(Texture2D tex, out double lumC, out double redC, out double lumE, out double redE)
        {
            int w = tex.width, h = tex.height;
            Color32[] px = tex.GetPixels32();
            double lc = 0, rc = 0, le = 0, re = 0;
            int nc = 0, ne = 0;
            int bw = Math.Max(1, (int)(w * 0.08)), bh = Math.Max(1, (int)(h * 0.08));
            for (int y = 0; y < h; y += 2)
                for (int x = 0; x < w; x += 2)
                {
                    Color32 c = px[y * w + x];
                    double r = c.r / 255.0, gg = c.g / 255.0, b = c.b / 255.0;
                    double lum = 0.2126 * r + 0.7152 * gg + 0.0722 * b, red = r - (gg + b) / 2;
                    bool centre = x > w * 0.4 && x < w * 0.6 && y > h * 0.4 && y < h * 0.6;
                    bool edge = x < bw || x >= w - bw || y < bh || y >= h - bh;
                    if (centre) { lc += lum; rc += red; nc++; }
                    else if (edge) { le += lum; re += red; ne++; }
                }
            lumC = lc / Math.Max(1, nc);
            redC = rc / Math.Max(1, nc);
            lumE = le / Math.Max(1, ne);
            redE = re / Math.Max(1, ne);
        }

        static IEnumerator Edge31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            offs.Add(() => { LowHealthEdge31.ReelOverride = null; Quiet(g); });
            Fresh(g);
            if (g.husks != null) g.husks.frozen = true;
            SurvivalSim sim = g.sim;
            double max = g.cfg.Num("health.max");
            bool configured = !N31Blocks.ForceOff && g.cfg.Design("n31.lowHealthEdge.enabled");
            double yaw = g.input.yaw;
            Start(g, "edge31", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            int frameOn = -1, frameOff = -1;
            double redCOn = 0, redCOff = 0, redEOn = 0;
            foreach (bool side in new[] { true, false })
            {
                LowHealthEdge31.ReelOverride = side;
                double t0 = sim.SimTime;
                double t0r = Time.realtimeSinceStartup;
                while (sim.SimTime - t0 < 1.5 && Time.realtimeSinceStartup - t0r < 20)
                {
                    g.health.max = max;
                    g.health.hp = max * 0.2;
                    g.input.SetLook(yaw, 0);
                    g.forcedMove = new InputFrame { yaw = yaw, pitch = 0 };
                    yield return null;
                }
                yield return new WaitForEndOfFrame();
                Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
                EdgeRead31(tex, out double lumC, out double redC, out double lumE, out double redE);
                UnityEngine.Object.Destroy(tex);
                if (side) { frameOn = FeelProbe.FrameCount - 1; redCOn = redC; redEOn = redE; }
                else { frameOff = FeelProbe.FrameCount - 1; redCOff = redC; }
                lines.Add($"row {(side ? "ON" : "OFF")}: centre luminance {lumC.ToString("0.000", ci)} redness {redC.ToString("0.000", ci)}, edge luminance {lumE.ToString("0.000", ci)} redness {redE.ToString("0.000", ci)}");
            }
            LowHealthEdge31.ReelOverride = null;
            g.forcedMove = new InputFrame();
            yield return Stop();
            // PASS: with the row on the centre is no redder than with it off (within 0.01) and the edge still carries the red
            bool ok = redCOn <= redCOff + 0.01 && redEOn > redCOn + 0.02;
            res[0] = $"{(ok ? "PASS" : "RED")}: health 20 %; {string.Join("; ", lines)}; the row as configured {configured} | frames: on f{frameOn:0000}, off f{frameOff:0000} (look: on = red only at the edges, the middle clean)";
        }
    }
}
