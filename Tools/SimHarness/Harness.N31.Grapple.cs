// NIGHT31 (builder G): the pull-in grapple's headless judge. ONE entry method, N31Grapple(cmd, args, ref code) (declared and called in Program.cs).
//   grapple31 [--off] [--level path]   item 1 (DESIGN31 B + R3 + R9) on Design/levels/n30-span.json (the campaign's grapple level):
//     A. MEASURE: the speed toward a node 0.3 s after the press, for a HOLD (NIGHT30's swing) and a TAP (press, up after 50 ms).
//     B. MEASURE: the time from the press to 30 m of path covered by a tap pull at 34 / 44 / 60 m/s (the pull speed swapped per run).
//     C. CHAIN: three node pulls in a row from s2's spawn toward s3 (each tap at the next node ahead as soon as the rope's cooldown ends):
//        the speed in at each press and out at each arrival; the speed kept into the second hook.
//     D. CANCEL: a second press during a pull lets go with the speed kept; a jump in the air the same (the Mover's own release).
//     E. TAPE: a taped campaign run with taps and holds at the nodes replays EQUAL from the tape alone.
//     F. TOGGLE: HoldToggle's toggle mode with the tap window: a tap passes the button-up through, a hold latches as before.
//   `--off`: design.n31.grapplePull off (NIGHT30's grapple): RED when no tap pulls you in.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N31Grapple(string cmd, string[] args, ref int code)
    {
        if (cmd == "grapple31") Grapple31Command(args, ref code);
        else if (cmd == "padreel31") PadReel31Command(ref code);
        else if (cmd == "pick31") Pick31Command(args, ref code);
        else if (cmd == "lesson31") Lesson31Command(args, ref code);
        else if (cmd == "ride31") Ride31Command(args, ref code);
        else if (cmd == "chain31") Chain31Command(args, ref code);
        else if (cmd == "freeze31") Freeze31Command(args, ref code);
        else if (cmd == "loompick31") LoomPick31Command(args, ref code);
    }

    /// <summary>`loompick31` (round 4, after B's Loom commit: hips 1.5 m over the deck, 6 m out): THE LOOM begun in s8 (B's CampRig31 "loom31": loom31 + grapplePull);
    /// for each joint, from the deck edge (1 m in) that sees it, aimed at it: what the pick takes (Arcline21.Peek: a NODE at that joint?), then a TAP: does the pull
    /// take that joint, and does it HANG there (DESIGN31 R4: the zip turns into the minLen swing, the rope stays) for 1 s. GREEN: every joint in sight is taken and hung on.</summary>
    static void LoomPick31Command(string[] args, ref int code)
    {
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        CampRig31 rig0 = CampRig31For("loom31", 1);
        SurvivalSim.Wyrm24Dir = null;
        if (rig0 == null || rig0.sim.loom30 == null) { Console.WriteLine("loompick31: no Loom"); code = 2; return; }
        int joints = rig0.sim.loom30.joints.Length;
        int taken = 0, hung = 0, inSight = 0;
        for (int j = 0; j < joints; j++)
        {
            SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
            CampRig31 rig = CampRig31For("loom31", 1);
            SurvivalSim.Wyrm24Dir = null;
            SurvivalSim sim = rig.sim;
            var si = sim.input as ScriptedInput ?? new ScriptedInput();
            sim.UseInput(si);
            for (int i = 0; i < 3; i++) G31Tick(sim, si, null, false, false);
            Loom30 l = sim.loom30;
            // the edge that sees this joint (nearest clear)
            Vec3? feet = null;
            double bestD = double.MaxValue;
            for (int e = 0; e < 4; e++)
            {
                Vec3 g0 = l.grips[e], g1 = l.grips[(e + 1) % 4];
                Vec3 mid = new Vec3((g0.x + g1.x) / 2, g0.y, (g0.z + g1.z) / 2);
                Vec3 inward = Vec3.Normalize(new Vec3(l.centre.x - mid.x, 0, l.centre.z - mid.z));
                Vec3 f0 = new Vec3(mid.x + inward.x, mid.y, mid.z + inward.z);
                Vec3 eye = new Vec3(f0.x, f0.y + 1.6, f0.z);
                double d = Vec3.DistanceTo(eye, l.joints[j]);
                if (G31Sees(sim, eye, l.joints[j]) && d < bestD) { bestD = d; feet = f0; }
            }
            if (!feet.HasValue) { Console.WriteLine($"loompick31 joint {j} ({(j % 2 == 0 ? "knee" : "hip")}{j / 2}): not in sight from any deck edge"); continue; }
            inSight++;
            sim.Respawn(feet.Value, 0);
            for (int i = 0; i < 12; i++) G31Tick(sim, si, sim.loom30.joints[j], false, false);
            Vec3 jp = sim.loom30.joints[j];
            (double yaw, double pitch) = ArclineLook(sim.Eye, jp);
            Arcline21Kind k = sim.arcline21.Peek(new InputFrame { yaw = yaw, pitch = pitch }, out Vec3 pt);
            bool peekJoint = k == Arcline21Kind.Node && Vec3.DistanceTo(pt, jp) < 0.5;
            int hangs0 = sim.grappleHangs31;
            bool tookJoint = false;
            string relWhy = "-";
            sim.events.On<Arcline21RopeEvent>("ropeRelease", e => { if (sim.grappleHangs31 > hangs0 && relWhy == "-") relWhy = $"{e.reason} at y {G31f(sim.mover.position.y)}"; });
            double hangSecs = 0, hangDist = 0;
            for (int s = 0; s < 360; s++)
            {
                Vec3 aim = sim.loom30.joints[j];
                G31Tick(sim, si, s < 20 ? aim : (Vec3?)null, s < 6, s == 0);
                Rope r = sim.mover.rope;
                if (r.phase != RopePhase.None && r.node - 1 == sim.loom30NodeStartPublic + j) tookJoint = true;
                if (sim.grappleHangs31 > hangs0 && r.phase == RopePhase.Attached) { hangSecs += sim.step; hangDist = Vec3.DistanceTo(sim.mover.RopePoint, r.anchor); }
                if (hangSecs >= 1.0) break;
            }
            if (peekJoint && tookJoint) taken++;
            if (hangSecs >= 1.0) hung++;
            Console.WriteLine($"loompick31 joint {j} ({(j % 2 == 0 ? "knee" : "hip")}{j / 2}) at ({G31f(jp.x, "0.0")}, {G31f(jp.y, "0.0")}, {G31f(jp.z, "0.0")}) from {G31f(bestD, "0.0")} m: the mark takes {k}{(peekJoint ? " (the joint)" : $" at ({G31f(pt.x, "0.0")}, {G31f(pt.y, "0.0")}, {G31f(pt.z, "0.0")})")}; the tap took the joint {tookJoint}; hang {(hangSecs >= 1.0 ? $"held 1 s, {G31f(hangDist)} m from the joint" : $"{G31f(hangSecs)} s, let go '{relWhy}'")}");
        }
        bool ok = inSight > 0 && taken == inSight && hung == inSight;
        Console.WriteLine($"loompick31 {(ok ? "GREEN" : "RED")}: {inSight} of {joints} joints in sight from a deck edge; taken {taken}, hung on {hung}");
        code = ok ? 0 : 1;
    }

    /// <summary>An input wrapper that keeps the last frame the sim sampled (the freeze trace reads what the Mover was given).</summary>
    sealed class G31Rec : IPlayerInput
    {
        public readonly IPlayerInput inner;
        public InputFrame last;
        public G31Rec(IPlayerInput inner) { this.inner = inner; }
        public double Yaw => inner.Yaw;
        public double Pitch => inner.Pitch;
        public void SetLook(double y, double p) => inner.SetLook(y, p);
        public bool Captured => inner.Captured;
        public InputFrame SampleMove(bool active) { last = inner.SampleMove(active); return last; }
        public CombatInput SampleCombat(bool active) => inner.SampleCombat(active);
    }

    /// <summary>`freeze31 [--level span] [--seed 6] [--cap 900]` (round 4, the lead: walk30 span seed 6 FROZEN IN THE AIR under s6's deck node): walk30's run for one
    /// seed; from the first 3 s with the player in the air and not moving (under 0.2 m), a trace every 0.5 s for 3 s: the Mover's state, velocity, rope (phase,
    /// zip, anchor, L, attachedSteps, cooldown), the grapple latch (Pulling, OwnZip) and the frame the Mover was given. RED: frozen; GREEN: no freeze in the run.</summary>
    static void Freeze31Command(string[] args, ref int code)
    {
        string name = Opt30(args, "--level", "span");
        uint seed = uint.Parse(Opt30(args, "--seed", "6"), G31ci);
        double cap = double.Parse(Opt30(args, "--cap", "900"), G31ci);
        GameConfig cfg = Config();
        string path = Path.Combine("Design", "levels", $"n30-{name}.json");
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var ap = new AutopilotInput(sim, "normal", seed);
        ap.GoalProvider = () =>
        {
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) return lv.rooms[0].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        var pilot = new Walk29Pilot(sim, ap);
        var rec = new G31Rec(pilot);
        sim.UseInput(rec);
        var ropeLog = new List<string>();
        int step = 0;
        sim.mover.Events += e =>
        {
            if (e.type == MoverEventType.RopeFire || e.type == MoverEventType.RopeAttach || e.type == MoverEventType.RopeRelease)
            {
                ropeLog.Add($"s{step} {e.type} zip {e.zip} node {e.node} reason {e.reason}");
                if (ropeLog.Count > 12) ropeLog.RemoveAt(0);
            }
        };
        sim.StartRun(seed, null, null);
        int max = MovementMath.StepsFor(cap, sim.step), stillSteps = 0, frozenAt = -1, traced = 0;
        Vec3 refP = sim.mover.position;
        double hz = cfg.movement.loop.simHz;
        for (; step < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); step++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
            Vec3 p = sim.mover.position;
            if (!sim.mover.grounded && Vec3.DistanceTo(p, refP) < 1.5) stillSteps++; // round 4: a hover that jitters counts (walk30's stop line: 'state Air grounded False' for minutes)
            else { stillSteps = 0; refP = p; }
            if (frozenAt < 0 && stillSteps >= (int)(3 * hz))
            {
                frozenAt = step;
                Console.WriteLine($"freeze31 FROZEN at {G31f(step / hz, "0.0")} s, room {(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")}, at ({G31f(p.x, "0.0")}, {G31f(p.y, "0.00")}, {G31f(p.z, "0.0")}); last rope events: {string.Join("; ", ropeLog)}");
            }
            if (frozenAt >= 0 && (step - frozenAt) % (int)(hz / 2) == 0 && traced < 6)
            {
                traced++;
                Rope r = sim.mover.rope;
                InputFrame f = rec.last;
                Console.WriteLine($"freeze31   +{G31f((step - frozenAt) / hz, "0.0")} s: {sim.mover.state} grounded {sim.mover.grounded} v ({G31f(sim.mover.velocity.x)}, {G31f(sim.mover.velocity.y)}, {G31f(sim.mover.velocity.z)}) touchingWall {sim.mover.touchingWall}; rope {r.phase} zip {r.zip} node {r.node} anchor ({G31f(r.anchor.x, "0.0")}, {G31f(r.anchor.y, "0.0")}, {G31f(r.anchor.z, "0.0")}) L {G31f(r.L)} attached {r.attachedSteps} cooldown {r.cooldownSteps} pull {r.pullSteps} taut {r.taut}; latch pulling {sim.grappleTap31.Pulling} ownZip {sim.grappleTap31.OwnZip}; frame grapple {f.grapple.held}/{f.grapple.pressed} jump {f.jump.held}/{f.jump.pressed} dash {f.dash.pressed} move ({G31f(f.moveX, "0.0")}, {G31f(f.moveZ, "0.0")}); unsticks {pilot.unsticks}");
            }
            if (traced >= 6) break;
            if (frozenAt < 0 && step >= max - (int)(3 * hz) && (max - step) % (int)(hz / 2) == 0)
            {
                Rope r = sim.mover.rope;
                InputFrame f = rec.last;
                Console.WriteLine($"freeze31   end -{G31f((max - step) / hz, "0.0")} s: ({G31f(p.x, "0.00")}, {G31f(p.y, "0.00")}, {G31f(p.z, "0.00")}) {sim.mover.state} grounded {sim.mover.grounded} v ({G31f(sim.mover.velocity.x)}, {G31f(sim.mover.velocity.y)}, {G31f(sim.mover.velocity.z)}) wall {sim.mover.touchingWall}; rope {r.phase} zip {r.zip} cd {r.cooldownSteps}; latch {sim.grappleTap31.Pulling}; frame grapple {f.grapple.held}/{f.grapple.pressed} jump {f.jump.held}/{f.jump.pressed} move ({G31f(f.moveX, "0.0")}, {G31f(f.moveZ, "0.0")}) yaw {G31f(f.yaw)}; unsticks {pilot.unsticks}; alive {sim.enemies.AliveCount}; ropes [{string.Join("; ", ropeLog)}]");
            }
        }
        ap.Detach();
        Console.WriteLine(frozenAt >= 0 ? "freeze31 RED: the player froze in the air" : $"freeze31 GREEN: no freeze in the air in {G31f(step / hz, "0")} s (rooms cleared {sim.campaign?.clearSteps.Count} of {playable})");
        code = frozenAt >= 0 ? 1 : 0;
    }

    /// <summary>The doors' centres of a level JSON's campaign block (the walk between rooms).</summary>
    static List<Vec3> G31Doors(Dictionary<string, object> json)
    {
        var doors = new List<Vec3>();
        if (json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("doors", out object dO) && dO is List<object> dl)
            foreach (object o in dl)
                if (o is Dictionary<string, object> dd && dd.TryGetValue("rect", out object rO) && rO is List<object> rc && rc.Count == 4)
                    doors.Add(new Vec3((PortData.Num(rc[0]) + PortData.Num(rc[2])) / 2, 0, (PortData.Num(rc[1]) + PortData.Num(rc[3])) / 2));
        return doors;
    }

    /// <summary>`chain31 [--off] [--dbg]` (round 3, item 4's route): Chain31Pilot from s5's spawn on n30-span: pull to s6's node, pull to the west wall, ride, pull to
    /// s6's second node, walk on, pull to an s7 node. Prints each tap, what the hook took, each arrival's speed, each wall ride (start speed, end reason).
    /// GREEN: 3 pulls arrive (wall, s6's node, an s7 node), at least one ride starts after a pull.</summary>
    static void Chain31Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        bool pull = !Flag(args, "--off");
        SurvivalMap map = CampaignMapFrom(Opt30(args, "--level", "Design/levels/n30-span.json"), cfg, out var json);
        LevelDef level = LevelDef.From(json);
        RoomDef s5 = level.rooms.Find(r => r.id == "s5");
        var si = new ScriptedInput();
        SurvivalSim sim = G31Sim(cfg, map, pull, si);
        G31Warm(sim, si, 3140);
        sim.campaign?.SetDoorFree29(true);
        sim.Respawn(s5.spawn, s5.spawnYawDeg);
        for (int s = 0; s < 30; s++) G31Tick(sim, si, null, false, false);
        var pilot = new Chain31Pilot(level, G31Doors(json));
        var lines = new List<string>();
        int step = 0, arrivals = 0, rides = 0;
        sim.events.On<GrapplePull31Event>("grapplePull", e => lines.Add($"s{step} pull ({(e.node ? "Node" : "Surface")})"));
        sim.events.On<GrapplePull31Event>("grappleArrive", e => { arrivals++; lines.Add($"s{step} ARRIVE {G31f(e.speed, "0.0")} m/s at ({G31f(e.x, "0.0")}, {G31f(e.y, "0.0")}, {G31f(e.z, "0.0")})"); });
        sim.mover.Events += e =>
        {
            if (e.type == MoverEventType.WallRideStart) { rides++; lines.Add($"s{step} RIDE start {G31f(e.speed, "0.0")} m/s"); }
            else if (e.type == MoverEventType.WallRideEnd) lines.Add($"s{step} ride end '{e.reason}' {G31f(e.speed, "0.0")} m/s");
        };
        for (step = 0; step < 2400 && pilot.phase < 4; step++)
        {
            InputFrame look = new InputFrame { yaw = si.Yaw, pitch = si.Pitch };
            bool mark = sim.arcline21 != null && sim.arcline21.Peek(look, out _) != Arcline21Kind.None;
            InputFrame f = pilot.Next(sim, mark);
            si.SetLook(f.yaw, f.pitch);
            si.Set(f, new CombatInput { weapon = -1 });
            sim.health.hp = sim.health.max;
            sim.mover.invulnerable = true;
            sim.hitstopLeft = 0;
            sim.Step();
            if (Flag(args, "--dbg") && step % 30 == 0) Console.WriteLine($"  dbg s{step} phase {pilot.phase} {sim.mover.state} pos ({G31f(sim.mover.position.x, "0.0")}, {G31f(sim.mover.position.y, "0.0")}, {G31f(sim.mover.position.z, "0.0")}) v {G31f(sim.mover.velocity.Length, "0.0")}");
        }
        Console.WriteLine($"chain31 {(pull ? "ON" : "OFF")}: taps {pilot.presses} ({pilot.failedPresses} fired nothing), arrivals {arrivals}, wall rides {rides}, phase reached {pilot.phase} in {G31f(step / cfg.movement.loop.simHz, "0.0")} s");
        foreach (string l in pilot.log) Console.WriteLine("chain31   " + l);
        foreach (string l in lines) Console.WriteLine("chain31   " + l);
        bool ok = pull && arrivals >= 3 && rides >= 1;
        Console.WriteLine(ok ? "chain31 GREEN: pull to the wall, wall ride, pull, pull from s5 into s7" : "chain31 RED");
        code = ok ? 0 : 1;
    }

    /// <summary>`ride31 [--off] [--room s6] [--side 1|-1] [--ahead m] [--up m] [--dbg]` (the lead's round 3, item 1e: "a pull into a wall ride keeps the speed"):
    /// on n30-span, from the room's spawn a TAP at a point on a side wall ahead (the hook takes the SURFACE), the pull, then: does a wall ride start within
    /// 0.5 s of the arrival, with what along-wall speed against the speed at the arrival; then from the ride a TAP at the room's next node (hook-ride-hook).
    /// GREEN when the ride starts with at least 80 % of the arrival's horizontal speed and the second pull arrives.</summary>
    static void Ride31Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        bool pull = !Flag(args, "--off");
        string roomId = Opt30(args, "--room", "s6");
        double side = double.Parse(Opt30(args, "--side", "1"), G31ci), ahead = double.Parse(Opt30(args, "--ahead", "17"), G31ci), up = double.Parse(Opt30(args, "--up", "3"), G31ci);
        bool dbg = Flag(args, "--dbg");
        SurvivalMap map = CampaignMapFrom(Opt30(args, "--level", "Design/levels/n30-span.json"), cfg, out var json);
        LevelDef level = LevelDef.From(json);
        RoomDef room = level.rooms.Find(r => r.id == roomId);
        if (room == null) { Console.WriteLine($"ride31: no room {roomId}"); code = 2; return; }
        var si = new ScriptedInput();
        SurvivalSim sim = G31Sim(cfg, map, pull, si);
        G31Warm(sim, si, 3130);
        sim.campaign?.SetDoorFree29(true);
        sim.Respawn(room.spawn, room.spawnYawDeg);
        for (int s = 0; s < 30; s++) G31Tick(sim, si, null, false, false);
        // the wall point: along the room's side wall (x = x1 or x0), `ahead` m past the spawn, `up` m high; the ray from the eye finds the wall's real face
        Vec3 eye = sim.Eye;
        Vec3 aimGuess = new Vec3(side > 0 ? room.x1 + 1 : room.x0 - 1, room.spawn.y + up, room.spawn.z - ahead);
        Vec3 dirG = (aimGuess - eye) / Vec3.DistanceTo(aimGuess, eye);
        if (!sim.world.RayIntersect(eye, dirG, out RayHit wh)) { Console.WriteLine("ride31: no wall in that direction"); code = 2; return; }
        Vec3 wallPt = eye + dirG * wh.distance;
        int step = 0, arriveStep = -1, rideStep = -1, rideEndStep = -1, pull2Step = -1, arrive2Step = -1;
        double arriveSpeed = 0, arriveHoriz = 0, rideSpeed = 0, rideEndSpeed = 0, arrive2Speed = 0;
        string took1 = "-", took2 = "-", rideEnd = "-";
        sim.events.On<GrapplePull31Event>("grapplePull", e => { if (took1 == "-") took1 = e.node ? "Node" : "Surface"; else if (took2 == "-") { took2 = e.node ? "Node" : "Surface"; pull2Step = step; } });
        sim.events.On<GrapplePull31Event>("grappleArrive", e =>
        {
            if (arriveStep < 0) { arriveStep = step; arriveSpeed = sim.mover.velocity.Length; arriveHoriz = Math.Sqrt(sim.mover.velocity.x * sim.mover.velocity.x + sim.mover.velocity.z * sim.mover.velocity.z); }
            else if (arrive2Step < 0 && pull2Step >= 0) { arrive2Step = step; arrive2Speed = sim.mover.velocity.Length; }
        });
        sim.mover.Events += e =>
        {
            if (e.type == MoverEventType.WallRideStart && rideStep < 0 && arriveStep >= 0) { rideStep = step; rideSpeed = e.speed; }
            if (e.type == MoverEventType.WallRideEnd && rideStep >= 0 && rideEndStep < 0) { rideEndStep = step; rideEnd = e.reason; rideEndSpeed = sim.mover.velocity.Length; }
        };
        int pressAt = 0;
        Vec3? node2 = null;
        foreach (Vec3 n in map.grappleNodes) if (n.z < room.spawn.z - 10 && n.z > room.z0 && (!node2.HasValue || n.z > node2.Value.z)) node2 = n;
        int press2At = -1;
        for (step = 0; step < 600; step++)
        {
            Vec3? aim = null;
            bool held = false, pressed = false;
            if (step < 60 && arriveStep < 0) { aim = wallPt; held = step - pressAt < 6; pressed = step == pressAt; }
            else if (rideStep >= 0 && press2At < 0 && node2.HasValue && step >= rideStep + 12 && sim.mover.rope.cooldownSteps == 0 && G31Sees(sim, sim.Eye, node2.Value)) press2At = step;
            if (press2At >= 0) { aim = node2; held = step - press2At < 6; pressed = step == press2At; }
            double moveZ = rideStep >= 0 && press2At < 0 ? 1 : 0; // hold forward on the ride
            if (dbg && step % 6 == 0 && step < 120) Console.WriteLine($"  dbg s{step} {sim.mover.state} pos ({G31f(sim.mover.position.x, "0.0")}, {G31f(sim.mover.position.y, "0.0")}, {G31f(sim.mover.position.z, "0.0")}) v ({G31f(sim.mover.velocity.x, "0.0")}, {G31f(sim.mover.velocity.y, "0.0")}, {G31f(sim.mover.velocity.z, "0.0")}) rope {sim.mover.rope.phase} wall {sim.mover.touchingWall}");
            G31Tick(sim, si, aim, held, pressed, false, moveZ);
            if (arrive2Step >= 0) break;
        }
        double hz = cfg.movement.loop.simHz;
        Console.WriteLine($"ride31 {(pull ? "ON" : "OFF")} room {roomId}: wall point ({G31f(wallPt.x, "0.0")}, {G31f(wallPt.y, "0.0")}, {G31f(wallPt.z, "0.0")}), the hook took {took1}; arrival at step {arriveStep}: speed {G31f(arriveSpeed, "0.0")} m/s (horizontal {G31f(arriveHoriz, "0.0")}); wall ride {(rideStep >= 0 ? $"started {G31f((rideStep - arriveStep) / hz, "0.000")} s after the arrival at {G31f(rideSpeed, "0.0")} m/s along the wall, ended '{rideEnd}' after {G31f((rideEndStep - rideStep) / hz, "0.00")} s at {G31f(rideEndSpeed, "0.0")} m/s" : "NOT started")}; second hook at node {(node2.HasValue ? $"({G31f(node2.Value.x, "0.0")}, {G31f(node2.Value.y, "0.0")}, {G31f(node2.Value.z, "0.0")})" : "-")}: took {took2}, arrived {(arrive2Step >= 0 ? $"at {G31f(arrive2Speed, "0.0")} m/s" : "no")}");
        bool ok = pull && rideStep >= 0 && rideStep - arriveStep <= MovementMath.StepsFor(0.5, 1 / hz) && rideSpeed >= 0.8 * arriveHoriz && arrive2Step >= 0;
        Console.WriteLine(ok ? "ride31 GREEN: a pull into the wall starts a ride with the speed kept, and a second pull from the ride arrives" : "ride31 RED");
        code = ok ? 0 : 1;
    }

    /// <summary>`lesson31 [--level path] [--room id] [--nodoorfree]` (the lead's r2 frame: n30-foundry f1's "MOVE. LOOK. WALK TO THE LIGHT." still on screen in f9 after a
    /// respawn): a CAMPAIGN run with the config's rows; once f1's line is up, a respawn into a later room (DOORFREE, as spawn31 does); the line the HUD would show
    /// (the last lessonBegin's prompt, hidden by lessonClear: SurvivalGame.N25's wiring) is read every step for 2 s. GREEN when, from the first step after the
    /// respawn, no line of an earlier room is up.</summary>
    static void Lesson31Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        string path = Opt30(args, "--level", "Design/levels/n30-foundry.json");
        string roomId = Opt30(args, "--room", "f9");
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        RoomDef to = level.rooms.Find(r => r.id == roomId);
        if (to == null) { Console.WriteLine($"lesson31: no room {roomId}"); code = 2; return; }
        int toIdx = level.rooms.IndexOf(to);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        if (Flag(args, "--off")) sim.n31Override = N31Blocks.Off;
        var si = new ScriptedInput();
        sim.UseInput(si);
        string lineId = null, linePrompt = null;
        var log = new List<string>();
        int step = 0;
        sim.events.On<LessonEvent>("lessonBegin", e => { if (!string.IsNullOrEmpty(e.prompt)) { lineId = e.id; linePrompt = e.prompt; } log.Add($"s{step} begin {e.id} '{e.prompt}'"); });
        sim.events.On<LessonEvent>("lessonClear", e => { lineId = null; linePrompt = null; log.Add($"s{step} clear {e.id}"); });
        sim.events.On<RoomEvent>("roomEnter", e => log.Add($"s{step} roomEnter {e.id}"));
        sim.StartRun(3120, null, null);
        for (step = 0; step < 120 && lineId == null; step++) G31Tick(sim, si, null, false, false);
        string before = lineId;
        if (!Flag(args, "--nodoorfree")) sim.campaign?.SetDoorFree29(true);
        sim.Respawn(to.spawn, to.spawnYawDeg);
        int stale = 0, firstStale = -1;
        string staleText = null;
        int respawnAt = step;
        for (int s = 0; s < 240; s++, step++)
        {
            G31Tick(sim, si, null, false, false);
            int li = lineId == null ? -1 : level.rooms.FindIndex(r => r.id == lineId);
            if (li >= 0 && li < toIdx) { stale++; if (firstStale < 0) { firstStale = s; staleText = linePrompt; } }
        }
        Console.WriteLine($"lesson31 {level.id}: n30.teach {sim.n30.teach}, line up before the respawn: {before ?? "none"}; respawn into {roomId} at step {respawnAt}; campaign room now {(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "-")}; line now {lineId ?? "none"} '{linePrompt}'");
        Console.WriteLine("lesson31 events: " + string.Join("; ", log));
        bool ok = before != null && stale == 0;
        Console.WriteLine(ok ? $"lesson31 GREEN: no earlier room's line up in any of the 240 steps after the respawn" : $"lesson31 RED: an earlier room's line up in {stale} of 240 steps after the respawn (first at step +{firstStale}: '{staleText}')");
        code = ok ? 0 : 1;
    }

    /// <summary>`pick31 [--off]` (the lead's r2 frame: "the mark said [Surface, Surface, none]"): on n30-span, from each room's spawn and from 4.5 m before each node,
    /// aimed straight at each node in range: what the pick takes (Arcline21.Peek), the node's line-of-sight ray (hit distance vs the node's), and the node's
    /// clearance from the nearest surface. GREEN when every node in view of its own room's spawn is taken as a NODE.</summary>
    static void Pick31Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalMap map = CampaignMapFrom(Opt30(args, "--level", "Design/levels/n30-span.json"), cfg, out var json);
        LevelDef level = LevelDef.From(json);
        var si = new ScriptedInput();
        SurvivalSim sim = G31Sim(cfg, map, !Flag(args, "--off"), si);
        G31Warm(sim, si, 3110);
        sim.campaign?.SetDoorFree29(true);
        int asked = 0, nodeTaken = 0;
        int planNodes = json.TryGetValue("grappleNodes", out object gnO) && gnO is List<object> gnl ? gnl.Count : map.grappleNodes.Count; // the plan's nodes (a boss fight appends its joints after them)
        int upTo = Flag(args, "--all") ? map.grappleNodes.Count : Math.Min(planNodes, map.grappleNodes.Count); // --all: the boss's joints too (THE LOOM appends its joints after the plan's nodes)
        for (int ni = 0; ni < upTo; ni++)
        {
            Vec3 n = map.grappleNodes[ni];
            foreach (bool before in new[] { false, true })
            {
                Vec3 feet;
                string from;
                if (before) { feet = new Vec3(n.x, 0, n.z + 4.5); from = "4.5 m before"; }
                else
                {
                    RoomDef own = null;
                    foreach (RoomDef r in level.rooms) if (n.z >= r.z0 - 1 && n.z <= r.z1 + 1) { own = r; break; }
                    if (own == null) continue;
                    feet = own.spawn;
                    from = own.id + " spawn";
                }
                sim.Respawn(feet, 0);
                for (int s = 0; s < 20; s++) G31Tick(sim, si, n, false, false);
                Vec3 eye = sim.Eye;
                double d = Vec3.DistanceTo(eye, n);
                Vec3 dir = (n - eye) / d;
                bool hit = sim.world.RayIntersect(eye, dir, out RayHit h);
                (double yaw, double pitch) = ArclineLook(eye, n);
                Arcline21Kind k = sim.arcline21.Peek(new InputFrame { yaw = yaw, pitch = pitch }, out Vec3 p);
                double clear = double.MaxValue;
                foreach (Vec3 dd in new[] { new Vec3(1, 0, 0), new Vec3(-1, 0, 0), new Vec3(0, 0, 1), new Vec3(0, 0, -1), new Vec3(0, 1, 0), new Vec3(0, -1, 0) })
                    if (sim.world.RayIntersect(n, dd, out RayHit h2)) clear = Math.Min(clear, h2.distance);
                bool inRange = d <= cfg.movement.arcline.range;
                if (inRange && !before) { asked++; if (k == Arcline21Kind.Node) nodeTaken++; }
                Console.WriteLine($"pick31 node {ni} ({G31f(n.x, "0.0")}, {G31f(n.y, "0.0")}, {G31f(n.z, "0.0")}) from {from}: d {G31f(d, "0.0")} m, LOS ray {(hit ? $"hits at {G31f(h.distance, "0.00")}" : "clear")}, takes {k} at ({G31f(p.x, "0.0")}, {G31f(p.y, "0.0")}, {G31f(p.z, "0.0")}), nearest surface {G31f(clear, "0.00")} m");
            }
        }
        bool ok = nodeTaken == asked;
        Console.WriteLine($"pick31 {(ok ? "GREEN" : "RED")}: {nodeTaken} of {asked} nodes taken as NODE from their room's spawn when aimed at");
        code = ok ? 0 : 1;
    }

    /// <summary>`padreel31` (P0 e): the pad button that carries REEL in every default layout (v1-v5), and what else it carries.</summary>
    static void PadReel31Command(ref int code)
    {
        bool all = true;
        for (int v = 1; v <= 5; v++)
        {
            PadLayout l = GamepadMath.DefaultForVersion(v);
            var on = new List<string>();
            foreach (var e in l.Entries())
                if (Array.IndexOf(e.actions, PadAction.Reel) >= 0) on.Add($"{e.button} ({string.Join(" + ", Array.ConvertAll(e.actions, a => a.ToString()))})");
            if (on.Count == 0) all = false;
            Console.WriteLine($"padreel31 layout v{v}: REEL on {(on.Count > 0 ? string.Join(", ", on) : "NOTHING")}");
        }
        Console.WriteLine(all ? "padreel31 GREEN: REEL is bound on the pad in every default layout" : "padreel31 RED: a default layout leaves REEL unbound");
        code = all ? 0 : 1;
    }

    static readonly CultureInfo G31ci = CultureInfo.InvariantCulture;
    static string G31f(double v, string f = "0.00") => v.ToString(f, G31ci);

    /// <summary>A CAMPAIGN composition on the span with the ARCLINE owned (design.n30.span) and the pull row as asked.</summary>
    static SurvivalSim G31Sim(GameConfig cfg, SurvivalMap map, bool pull, ScriptedInput si, GrapplePull31Rules rules = null)
    {
        SurvivalSim sim = CampaignSim30(cfg, map, new N30Blocks { span = true });
        sim.n31Override = new N31Blocks { grapplePull = pull };
        if (rules != null) sim.grapplePull31 = rules;
        sim.UseInput(si);
        return sim;
    }

    /// <summary>One step: look at `aim` (or keep the look), the grapple button, a jump.</summary>
    static void G31Tick(SurvivalSim sim, ScriptedInput si, Vec3? aim, bool held, bool pressed, bool jump = false, double moveZ = 0)
    {
        if (aim.HasValue)
        {
            (double yaw, double pitch) = ArclineLook(sim.Eye, aim.Value);
            si.SetLook(yaw, pitch);
        }
        si.Set(new InputFrame { moveZ = moveZ, grapple = new Button { held = held, pressed = pressed }, jump = new Button { held = jump, pressed = jump } }, new CombatInput { weapon = -1 });
        sim.health.hp = sim.health.max;
        sim.mover.invulnerable = true;
        sim.hitstopLeft = 0;
        sim.Step();
    }

    static void G31Warm(SurvivalSim sim, ScriptedInput si, uint seed)
    {
        sim.StartRun(seed, null, null);
        for (int s = 0; s < 240; s++) G31Tick(sim, si, null, false, false);
    }

    static double G31Toward(SurvivalSim sim, Vec3 node)
    {
        Vec3 d = node - sim.mover.RopePoint;
        double len = d.Length;
        return len > 0 ? Vec3.Dot(sim.mover.velocity, d) / len : 0;
    }

    static bool G31Sees(SurvivalSim sim, Vec3 from, Vec3 to)
    {
        Vec3 t = to - from;
        double len = t.Length;
        return !(sim.world.RayIntersect(from, t / len, out RayHit hit) && hit.distance < len - 0.4);
    }

    static void Grapple31Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        string path = Opt30(args, "--level", "Design/levels/n30-span.json");
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        double hz = cfg.movement.loop.simHz;
        bool pull = !off;
        GrapplePull31Rules design = GrapplePull31Rules.From(cfg);
        Console.WriteLine($"grapple31 {(off ? "OFF (design.n31.grapplePull off: NIGHT30's grapple)" : "ON (grapplePull)")}: level {level.id}, {map.grappleNodes.Count} nodes; design speed {G31f(design.speed, "0")} m/s, tap window {G31f(design.tapWindow)} s, arrival pop {G31f(design.arrivePop, "0.0")} m/s; web zip {G31f(cfg.Num("arcline.zip.reelSpeed"), "0")} m/s, arrive {G31f(cfg.Num("arcline.zip.arrive"), "0.0")} m, maxTime {G31f(cfg.Num("arcline.zip.maxTime"), "0.0")} s");

        // ---- A. the speed toward a node 0.3 s after the press (hold and tap), from a room spawn to a node 15-25 m away in view ---------------
        RoomDef roomA = null;
        Vec3 nodeA = default;
        {
            var probe = new ScriptedInput();
            SurvivalSim ps = G31Sim(cfg, map, false, probe);
            G31Warm(ps, probe, 3100);
            foreach (RoomDef rr in level.rooms)
            {
                Vec3 chest = new Vec3(rr.spawn.x, rr.spawn.y + cfg.movement.arcline.ropeHeight, rr.spawn.z);
                foreach (Vec3 n in map.grappleNodes)
                {
                    double d = Vec3.DistanceTo(chest, n);
                    if (d < 15 || d > 25 || !G31Sees(ps, new Vec3(rr.spawn.x, rr.spawn.y + 1.6, rr.spawn.z), n)) continue;
                    roomA = rr;
                    nodeA = n;
                    break;
                }
                if (roomA != null) break;
            }
        }
        if (roomA == null) { Console.WriteLine("grapple31: no room spawn with a node 15-25 m away in view"); code = 2; return; }
        int at03 = MovementMath.StepsFor(0.3, 1 / hz);
        double[] towardAt = new double[2];
        string[] whyA = new string[2];
        int[] pullsA = new int[2];
        for (int kind = 0; kind < 2; kind++) // 0 hold (1 s), 1 tap (up after 6 steps = 50 ms)
        {
            var si = new ScriptedInput();
            SurvivalSim sim = G31Sim(cfg, map, pull, si);
            G31Warm(sim, si, 3101);
            sim.Respawn(roomA.spawn, roomA.spawnYawDeg);
            for (int s = 0; s < 30; s++) G31Tick(sim, si, nodeA, false, false); // settle on the floor
            string why = null;
            sim.events.On<Arcline21RopeEvent>("ropeRelease", e => why ??= e.reason);
            int heldSteps = kind == 0 ? 120 : 6;
            for (int s = 0; s <= at03; s++)
            {
                G31Tick(sim, si, nodeA, s < heldSteps, s == 0);
                if (s == at03) towardAt[kind] = G31Toward(sim, nodeA);
            }
            whyA[kind] = why ?? "-";
            pullsA[kind] = sim.grappleTap31.pulls;
        }
        double distA = Vec3.DistanceTo(new Vec3(roomA.spawn.x, roomA.spawn.y + cfg.movement.arcline.ropeHeight, roomA.spawn.z), nodeA);
        Console.WriteLine($"grapple31 A (0.3 s after the press, room {roomA.id} spawn -> node ({G31f(nodeA.x, "0.0")}, {G31f(nodeA.y, "0.0")}, {G31f(nodeA.z, "0.0")}), {G31f(distA, "0.0")} m): HOLD speed toward the node {G31f(towardAt[0])} m/s (first release '{whyA[0]}'); TAP {G31f(towardAt[1])} m/s (first release '{whyA[1]}', pulls {pullsA[1]})");

        // ---- B. time from the press to 30 m of path at 34 / 44 / 60 m/s (a node added in memory 32.6 m from a spawn, in view, in the open) ----
        double[] speeds = { cfg.Num("arcline.zip.reelSpeed"), 44, 60 };
        var lineB = new List<string>();
        SurvivalMap mapB = CampaignMapFrom(path, cfg, out _);
        Vec3? nodeB = null;
        RoomDef roomB = null;
        {
            var probe = new ScriptedInput();
            SurvivalSim ps = G31Sim(cfg, mapB, false, probe);
            G31Warm(ps, probe, 3102);
            double want = 30 + cfg.Num("arcline.zip.arrive") + 0.6;
            foreach (RoomDef rr in level.rooms)
            {
                Vec3 eye = new Vec3(rr.spawn.x, rr.spawn.y + 1.6, rr.spawn.z);
                Vec3 chest = new Vec3(rr.spawn.x, rr.spawn.y + cfg.movement.arcline.ropeHeight, rr.spawn.z);
                for (int k = 0; k < 16 && nodeB == null; k++)
                {
                    double a = rr.spawnYawDeg * Math.PI / 180 + (k % 2 == 0 ? 1 : -1) * (k / 2) * Math.PI / 24;
                    foreach (double up in new[] { 10.0, 8.0, 6.0 })
                    {
                        double h = Math.Sqrt(Math.Max(0, want * want - (up - cfg.movement.arcline.ropeHeight) * (up - cfg.movement.arcline.ropeHeight)));
                        var p = new Vec3(chest.x - Math.Sin(a) * h, rr.spawn.y + up, chest.z - Math.Cos(a) * h);
                        if (!G31Sees(ps, eye, p) || !G31Sees(ps, chest, p)) continue;
                        bool air = true;
                        foreach (Vec3 d in new[] { new Vec3(1, 0, 0), new Vec3(-1, 0, 0), new Vec3(0, 0, 1), new Vec3(0, 0, -1), new Vec3(0, 1, 0) })
                            if (ps.world.RayIntersect(p, d, out RayHit h2) && h2.distance < 0.8) air = false;
                        if (!air) continue;
                        nodeB = p;
                        roomB = rr;
                        break;
                    }
                }
                if (nodeB != null) break;
            }
        }
        if (nodeB == null) lineB.Add("no open point 32.6 m from a spawn in view: not measured");
        else
        {
            mapB.grappleNodes.Clear(); // only the measuring node (the pick prefers nodes; another in the cone would steal the press)
            mapB.grappleNodes.Add(nodeB.Value);
            foreach (double sp in speeds)
            {
                var si = new ScriptedInput();
                var rules = new GrapplePull31Rules { speed = sp, tapWindow = design.tapWindow, arrivePop = design.arrivePop };
                SurvivalSim sim = G31Sim(cfg, mapB, true, si, rules);
                G31Warm(sim, si, 3103);
                sim.Respawn(roomB.spawn, roomB.spawnYawDeg);
                for (int s = 0; s < 30; s++) G31Tick(sim, si, nodeB, false, false);
                Vec3 last = sim.mover.position;
                double pathLen = 0;
                int reached = -1;
                for (int s = 0; s < 240 && reached < 0; s++)
                {
                    G31Tick(sim, si, nodeB, s < 6, s == 0);
                    pathLen += Vec3.DistanceTo(sim.mover.position, last);
                    last = sim.mover.position;
                    if (pathLen >= 30) reached = s + 1;
                }
                lineB.Add(reached < 0 ? $"{G31f(sp, "0")} m/s: 30 m not reached in 2 s (path {G31f(pathLen, "0.0")} m)" : $"{G31f(sp, "0")} m/s: {G31f(reached / hz, "0.000")} s ({reached} steps)");
            }
        }
        cfg.movement.arcline.zip.reelSpeed = cfg.Num("arcline.zip.reelSpeed"); // the measuring runs swapped it; the web's back (the next sims restore their own)
        Console.WriteLine($"grapple31 B (press -> 30 m of path, a tap pull{(nodeB.HasValue ? $", room {roomB.id} -> a node added in memory at ({G31f(nodeB.Value.x, "0.0")}, {G31f(nodeB.Value.y, "0.0")}, {G31f(nodeB.Value.z, "0.0")})" : "")}): {string.Join("; ", lineB)}");

        // ---- C. a chain of three node pulls from s2 toward s3 ---------------------------------------------------------------------------
        RoomDef s2 = level.rooms.Count > 1 ? level.rooms[1] : level.rooms[0];
        var chainIn = new List<double>();
        var chainOut = new List<double>();
        var chainWhy = new List<string>();
        var chainTook = new List<string>(); // what each pull's hook bit (grapplePull's node flag)
        int chainArrived = 0;
        double secondHookIn = -1;
        {
            var si = new ScriptedInput();
            SurvivalSim sim = G31Sim(cfg, map, pull, si);
            G31Warm(sim, si, 3104);
            sim.campaign?.SetDoorFree29(true); // DOORFREE (NIGHT29): every door open, so the chain is the grapple's alone (the rooms' waves are not the point here)
            sim.Respawn(s2.spawn, s2.spawnYawDeg);
            for (int s = 0; s < 30; s++) G31Tick(sim, si, null, false, false);
            int arrivedNow = 0;
            sim.events.On<GrapplePull31Event>("grappleArrive", e => { arrivedNow++; chainOut.Add(e.speed); });
            sim.events.On<GrapplePull31Event>("grapplePull", e => chainTook.Add(e.node ? "Node" : "Surface"));
            sim.events.On<Arcline21RopeEvent>("ropeRelease", e => { chainWhy.Add(e.reason); if (!pull) chainOut.Add(e.speed); });
            // the doors' centres (the walk between rooms when no node is in view)
            var doorC = new List<Vec3>();
            if (json.TryGetValue("campaign", out object campO) && campO is Dictionary<string, object> camp && camp.TryGetValue("doors", out object dO) && dO is List<object> dl)
                foreach (object o in dl)
                    if (o is Dictionary<string, object> dd && dd.TryGetValue("rect", out object rO) && rO is List<object> rc && rc.Count == 4)
                        doorC.Add(new Vec3((PortData.Num(rc[0]) + PortData.Num(rc[2])) / 2, 0, (PortData.Num(rc[1]) + PortData.Num(rc[3])) / 2));
            var used = new HashSet<int>();
            double lastZ = double.MaxValue;
            int pressAt = -1, target = -1, presses = 0;
            for (int s = 0; s < 2400 && presses < 3 + 3; s++)
            {
                Rope r = sim.mover.rope;
                bool idle = r.phase == RopePhase.None && r.cooldownSteps == 0;
                bool press = false;
                Vec3 eye = sim.Eye;
                Vec3? aim = null;
                double moveZ = 0;
                if (r.phase != RopePhase.None && target >= 0) aim = map.grappleNodes[target];
                else
                {
                    // the next node along the course (past the last one used), in view, within the hook's range
                    int next = -1;
                    double bestZ = double.MinValue;
                    for (int i = 0; i < map.grappleNodes.Count; i++)
                    {
                        Vec3 n = map.grappleNodes[i];
                        if (used.Contains(i) || n.z > lastZ - 1 || Vec3.DistanceTo(n, eye) > cfg.movement.arcline.range * 0.9 || !G31Sees(sim, eye, n)) continue;
                        if (n.z > bestZ) { bestZ = n.z; next = i; }
                    }
                    if (next >= 0)
                    {
                        aim = map.grappleNodes[next];
                        if (idle && (pressAt < 0 || s - pressAt > 12))
                        {
                            target = next;
                            press = true;
                            pressAt = s;
                            presses++;
                            used.Add(target);
                            lastZ = map.grappleNodes[target].z;
                            double vin = sim.mover.velocity.Length;
                            chainIn.Add(vin);
                            if (chainIn.Count >= 2) secondHookIn = Math.Max(secondHookIn, vin); // the best speed carried from one pull into the next hook
                        }
                    }
                    else
                    {
                        // walk on: the nearest door ahead of the player
                        Vec3? wp = null;
                        foreach (Vec3 dc in doorC) if (dc.z < sim.mover.position.z - 0.3 && (!wp.HasValue || dc.z > wp.Value.z)) wp = dc;
                        if (wp.HasValue) aim = new Vec3(wp.Value.x, eye.y, wp.Value.z - 2);
                        moveZ = 1;
                        if (Flag(args, "--dbg") && s % 60 == 0) Console.WriteLine($"  dbg s{s} walk pos ({G31f(sim.mover.position.x, "0.0")}, {G31f(sim.mover.position.y, "0.0")}, {G31f(sim.mover.position.z, "0.0")}) v {G31f(sim.mover.velocity.Length, "0.0")} lastZ {G31f(lastZ, "0.0")}");
                    }
                }
                bool held = pressAt >= 0 && s - pressAt < 6;
                G31Tick(sim, si, aim, held, press, false, moveZ);
                if (arrivedNow > 0) { chainArrived += arrivedNow; arrivedNow = 0; }
                if (chainArrived >= 3) break;
            }
        }
        string fmtList(List<double> l) => string.Join(" / ", l.ConvertAll(v => G31f(v, "0.0")));
        Console.WriteLine($"grapple31 C (chain from room {s2.id}): presses {chainIn.Count}, arrivals {chainArrived}, the hook took [{string.Join(", ", chainTook)}]; speed in at each press {fmtList(chainIn)} m/s; out at each arrival {fmtList(chainOut)} m/s; releases [{string.Join(", ", chainWhy)}]; the best speed carried from a pull into the next hook {G31f(secondHookIn, "0.0")} m/s (after the swing's 0.4 s cooldown)");

        // ---- D. cancels: a second press, a jump --------------------------------------------------------------------------------------------
        double cancelBefore = -1, cancelAfter = -1, jumpBefore = -1, jumpAfter = -1;
        string cancelWhy = "-", jumpWhy = "-";
        for (int kind = 0; kind < 2 && pull; kind++)
        {
            var si = new ScriptedInput();
            SurvivalSim sim = G31Sim(cfg, map, pull, si);
            G31Warm(sim, si, 3105);
            sim.Respawn(roomA.spawn, roomA.spawnYawDeg);
            for (int s = 0; s < 30; s++) G31Tick(sim, si, nodeA, false, false);
            string why = null;
            sim.events.On<Arcline21RopeEvent>("ropeRelease", e => why ??= e.reason);
            int cancelStep = 24; // 0.2 s into the press: mid pull
            double before = 0;
            for (int s = 0; s <= cancelStep; s++)
            {
                if (s == cancelStep) before = sim.mover.velocity.Length;
                bool second = kind == 0 && s == cancelStep;
                G31Tick(sim, si, nodeA, s < 6 || second, s == 0 || second, kind == 1 && s == cancelStep);
            }
            double after = sim.mover.velocity.Length;
            if (kind == 0) { cancelBefore = before; cancelAfter = after; cancelWhy = why ?? "-"; }
            else { jumpBefore = before; jumpAfter = after; jumpWhy = why ?? "-"; }
        }
        if (pull) Console.WriteLine($"grapple31 D (cancels 0.2 s into a pull): second press '{cancelWhy}' speed {G31f(cancelBefore, "0.0")} -> {G31f(cancelAfter, "0.0")} m/s; jump '{jumpWhy}' {G31f(jumpBefore, "0.0")} -> {G31f(jumpAfter, "0.0")} m/s");

        // ---- E. the tape ----------------------------------------------------------------------------------------------------------------
        TapeResult tr;
        int tapePulls = 0, tapeFires = 0;
        RunTape tape;
        {
            var si = new ScriptedInput();
            SurvivalSim simT = G31Sim(cfg, map, pull, si);
            simT.events.On<GrapplePull31Event>("grapplePull", e => tapePulls++);
            simT.events.On<Arcline21RopeEvent>("ropeFire", e => tapeFires++);
            tape = TapeScript.Record(simT, 3106, 2400, "grapple31", (sm, i) =>
            {
                Vec3 aimAt = map.grappleNodes[0];
                double bd = double.MaxValue;
                foreach (Vec3 gn in map.grappleNodes) { double d = Vec3.DistanceTo(gn, sm.mover.position); if (d > 6 && d < bd) { bd = d; aimAt = gn; } }
                (double yaw, double pitch) = ArclineLook(sm.Eye, aimAt);
                int k = i % 180;
                bool tapCycle = (i / 180) % 3 != 2; // two taps, then a hold
                bool held = i >= 240 && (tapCycle ? k < 5 : k < 100);
                si.SetLook(yaw, pitch);
                si.Set(new InputFrame { moveZ = k > 120 ? 1 : 0, grapple = new Button { held = held, pressed = i >= 240 && k == 0 }, jump = new Button { pressed = k == 150, held = k >= 150 && k < 156 } },
                    new CombatInput { weapon = -1, fire = new Button { held = k % 60 < 10, pressed = k % 60 == 0 } });
            }, TapeScript.God, false);
            tr = TapeReplay.Replay(Config(), map, tape);
        }
        Console.WriteLine($"grapple31 E (tape): {tape.steps} campaign steps, hooks fired {tapeFires}, pulls {tapePulls}, tape bits n31 {tape.n31}; replay: {tr.Line}");

        // ---- F. the toggle mode's tap / hold rule (HoldToggle with the tap window) ------------------------------------------------------
        int tapW = pull ? MovementMath.StepsFor(design.tapWindow, 1 / hz) : 0;
        bool tapUpSeen = false, holdLatched = false;
        {
            var st = new ToggleState { tapSteps = tapW }; // what ReaderInput copies from HoldToggle.grappleTapSteps (the run's) before each sample
            for (int s = 0; s < 10; s++) // a tap: down 5 steps, the rope out from step 1
            {
                var f = new InputFrame { grapple = new Button { held = s < 5, pressed = s == 0 } };
                HoldToggle.ApplyToggles(ref f, st, false, true, s == 0 ? RopePhase.None : RopePhase.Flying);
                if (s >= 5 && !f.grapple.held) tapUpSeen = true;
            }
            st.Reset();
            st.tapSteps = tapW;
            bool allHeld = true;
            for (int s = 0; s < 60; s++) // a hold: down 40 steps (past the window), then up: the toggle keeps the swing
            {
                var f = new InputFrame { grapple = new Button { held = s < 40, pressed = s == 0 } };
                HoldToggle.ApplyToggles(ref f, st, false, true, s == 0 ? RopePhase.None : RopePhase.Attached);
                if (s > 0 && !f.grapple.held) allHeld = false;
            }
            holdLatched = allHeld;
        }
        Console.WriteLine($"grapple31 F (toggle mode, tap window {tapW} steps): a tap passes the button-up through {tapUpSeen}; a hold past the window stays latched {holdLatched}");

        if (off)
        {
            bool red = towardAt[1] < 5 && pullsA[1] == 0 && chainArrived == 0;
            Console.WriteLine(red ? $"grapple31 RED (as expected: no pull-in; a tap lets go at once, a hold swings: {G31f(towardAt[0])} m/s toward the node at 0.3 s)" : "grapple31 NOT RED: something pulled with the row off");
            code = red ? 1 : 2;
            return;
        }
        bool ok = towardAt[1] >= 0.8 * design.speed && pullsA[1] == 1 && chainArrived >= 3 && !chainTook.Contains("Surface") && secondHookIn > 10 && cancelWhy == "letGo" && jumpWhy == "jump" && cancelAfter > 0.8 * cancelBefore && tapePulls > 0 && tr.Equal && (tape.n31 & 1) != 0 && tapUpSeen && holdLatched;
        Console.WriteLine(ok ? "grapple31 GREEN: a tap pulls you in at the pull speed, three node pulls chain, cancels keep the speed, the taped run replays EQUAL, the toggle's tap / hold rule"
            : $"grapple31 FAIL (tap speed {towardAt[1] >= 0.8 * design.speed}, one pull {pullsA[1] == 1}, chain {chainArrived >= 3}, carried {secondHookIn > 10}, cancels {cancelWhy == "letGo" && jumpWhy == "jump" && cancelAfter > 0.8 * cancelBefore}, tape {tapePulls > 0 && tr.Equal && (tape.n31 & 1) != 0}, toggle {tapUpSeen && holdLatched})");
        code = ok ? 0 : 1;
    }
}
