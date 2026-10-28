// NIGHT32 builder S: the Player scenario of LEVEL 4 rebuilt (row design.n32.span32; the name is FeelReel.N27.cs PhaseN32S, the body is the partial
// N32SpanBody). Not run by its author (no Player in the worktree): the lead runs it.
// Boot with -skygrindMap n32-span (a CAMPAIGN level); DOORFREE is set, God on.
//   span32  (1) every room of the level: the player respawned at the room's entry (its spawn and facing), 0.6 s later one eye-height frame
//           (shot-N30-span32-<room>.png) and one line: enemies alive, the lesson on screen, the feet. (2) the ring route "ring, ride, ring" of s3
//           (the room row's ringRoute32, Tools/night/gen-n32-span.cjs) flown with the REAL grapple press: respawn at the route's standing point, a TAP
//           (the button held whole frames, released inside the tap window) aimed at ring 1, the stick toward ring 2 after the arrival, the second tap
//           0.8 s or more after the first (DESIGN32 G's gap), then the stick toward the goal; frames at the first press (-press1), the first arrival
//           (-arrive1), the second press (-press2) and the landing (-land). PASS when every room was framed and the route landed in its goal with two
//           arrivals (the line prints the wall-ride seconds between the pulls and where it came down).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N32SpanBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "span32") body = Span32Scenario(g, dir, res, offs, pre);
        }

        /// <summary>The reel's own timing (a bot's, not the game's): the settle before a room frame, the tap length in frames, the gap between taps (DESIGN32 G's
        /// 0.8 s with a margin), the limit for one pull to arrive and for the landing.</summary>
        const double Span32Settle = 0.6, Span32Gap = 0.85, Span32ArriveMax = 1.5, Span32LandMax = 4.0;
        const int Span32TapFrames = 3;

        static Vec3 Span32V(object o)
        {
            var a = (List<object>)o;
            return new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]));
        }

        static IEnumerator Span32Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            yield return StartCampaign30(g, offs);
            LevelDef lv = sim.campaignLevel;
            if (!sim.campaignOn || lv == null) { res[0] = "RED: the run did not start as a CAMPAIGN (boot -skygrindMap n32-span)"; yield break; }
            if (lv.id != "n32-span") { res[0] = $"RED: the booted level is '{lv.id}', not n32-span (boot -skygrindMap n32-span)"; yield break; }
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, span32 row {g.cfg.Design("n32.span32.enabled")}, grapple32 row {g.cfg.Design("n32.grapple32.enabled")}, ARCLINE owned {sim.arcline21 != null && sim.arcline21.Owned}";
            Start(g, "span32", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            int framed = 0;
            string ended = null;
            // (1) a frame of every room at eye height from its entry
            foreach (RoomDef r in lv.rooms)
            {
                if (ended != null) break;
                g.Respawn(r.spawn, r.spawnYawDeg);
                double t0 = sim.SimTime;
                while (sim.SimTime - t0 < Span32Settle && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (ended != null) break;
                yield return Shot30(dir, $"span32-{r.id}");
                framed++;
                string lesson = Ui.Lesson25Visible && !string.IsNullOrEmpty(Ui.Lesson25TextNow) ? Ui.Lesson25TextNow : "(none)";
                lines.Add($"{r.id} [{r.kind}] framed: alive {Alive30(sim)}, lesson '{lesson}', feet ({g.Feet.x.ToString("0.0", ci)}, {g.Feet.y.ToString("0.0", ci)}, {g.Feet.z.ToString("0.0", ci)}), map {g.MapId}");
            }
            // (2) s3's "ring, ride, ring" with the real grapple press
            string route = "not flown";
            bool routeOk = false;
            Dictionary<string, object> rr = null;
            List<Vec3> rings = new List<Vec3>();
            if (ended == null && sim.map.json != null && sim.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("rooms", out object ro) && ro is List<object> rl)
                foreach (object o in rl)
                    if (o is Dictionary<string, object> rj && rj.TryGetValue("id", out object id) && (string)id == "s3" && rj.TryGetValue("ringRoute32", out object rto) && rto is List<object> rts && rts.Count > 0)
                    {
                        rr = rts[0] as Dictionary<string, object>;
                        if (rj.TryGetValue("grappleNodes", out object gn) && gn is List<object> gl) foreach (object n in gl) rings.Add(Span32V(n));
                    }
            if (ended == null && rr != null && rings.Count >= 2)
            {
                var pulls = new List<int>();
                foreach (object p in (List<object>)rr["pulls"]) pulls.Add((int)PortData.Num(p));
                var goal = (List<object>)rr["goal"];
                double gx0 = PortData.Num(goal[0]), gz0 = PortData.Num(goal[1]), gx1 = PortData.Num(goal[2]), gz1 = PortData.Num(goal[3]), gMinY = PortData.Num(rr["goalMinY"]);
                Vec3 from = Span32V(rr["from"]);
                g.Respawn(from, PortData.Num(rr["yawDeg"]));
                double t0 = sim.SimTime;
                while (sim.SimTime - t0 < 0.5) { God(g); yield return null; }
                int arrivals = 0, rideFrames = 0;
                offs.Add(g.events.On<GrapplePull31Event>("grappleArrive", e => arrivals++));
                var log = new List<string>();
                double lastPress = -1e9;
                for (int k = 0; k < pulls.Count && ended == null; k++)
                {
                    Vec3 ring = rings[pulls[k]];
                    // wait for the gap and the previous arrival, the stick toward this ring
                    double tw = sim.SimTime;
                    while ((sim.SimTime - lastPress < Span32Gap || arrivals < k) && sim.SimTime - tw < Span32ArriveMax + Span32Gap && (ended = Ended30(g)) == null)
                    {
                        God(g);
                        if (sim.mover.state == MoveState.WallRide) rideFrames++;
                        var f = new InputFrame { moveZ = k > 0 ? 1 : 0 };
                        (f.yaw, f.pitch) = LookAt31(g, new Vec3(ring.x, sim.Eye.y, ring.z));
                        g.input.SetLook(f.yaw, f.pitch);
                        g.forcedMove = f;
                        yield return null;
                    }
                    if (ended != null) break;
                    bool sees = Sees31(g, sim.Eye, ring);
                    // the tap: aimed at the ring, held Span32TapFrames whole frames, then released (inside design.n31.grapplePull.tapWindow)
                    for (int fr = 0; fr < Span32TapFrames + 1; fr++)
                    {
                        God(g);
                        var f = new InputFrame();
                        (f.yaw, f.pitch) = LookAt31(g, ring);
                        f.grapple = new Btn { held = fr < Span32TapFrames, pressed = fr == 0 };
                        g.input.SetLook(f.yaw, f.pitch);
                        g.forcedMove = f;
                        if (fr == 1) yield return Shot30(dir, $"span32-press{k + 1}");
                        else yield return null;
                    }
                    lastPress = sim.SimTime;
                    log.Add($"tap {k + 1} at ring {pulls[k]} ({(sees ? "in sight" : "NOT in sight")}, {(ring - sim.Eye).Length.ToString("0.0", ci)} m)");
                    // the pull in flight: keep looking at the ring until it arrives
                    double tp = sim.SimTime;
                    while (arrivals <= k && sim.SimTime - tp < Span32ArriveMax && (ended = Ended30(g)) == null)
                    {
                        God(g);
                        var f = new InputFrame();
                        (f.yaw, f.pitch) = LookAt31(g, ring);
                        g.input.SetLook(f.yaw, f.pitch);
                        g.forcedMove = f;
                        yield return null;
                    }
                    if (k == 0 && arrivals >= 1) yield return Shot30(dir, "span32-arrive1");
                }
                // after the last pull: the stick toward the goal until grounded (or the limit)
                double tl = sim.SimTime;
                Vec3 gm = new Vec3((gx0 + gx1) / 2, 0, (gz0 + gz1) / 2);
                int grounded = 0;
                while (sim.SimTime - tl < Span32LandMax && grounded < Ms(300) && (ended = Ended30(g)) == null)
                {
                    God(g);
                    if (sim.mover.state == MoveState.WallRide) rideFrames++;
                    var f = new InputFrame { moveZ = 1 };
                    (f.yaw, f.pitch) = LookAt31(g, new Vec3(gm.x, sim.Eye.y, gm.z));
                    g.input.SetLook(f.yaw, f.pitch);
                    g.forcedMove = f;
                    grounded = sim.mover.grounded ? grounded + 1 : 0;
                    yield return null;
                }
                g.forcedMove = new InputFrame();
                yield return Shot30(dir, "span32-land");
                Vec3 q = g.Feet;
                bool inGoal = q.x >= gx0 - 0.5 && q.x <= gx1 + 0.5 && q.z >= gz0 - 0.5 && q.z <= gz1 + 0.5 && q.y >= gMinY;
                routeOk = inGoal && arrivals >= pulls.Count;
                route = $"s3 '{(string)rr["name"]}': {(routeOk ? "GREEN" : "RED")}, arrivals {arrivals} of {pulls.Count}, wall ride {(rideFrames / (double)UnityTuning.ReelFps).ToString("0.00", ci)} s (frames), came down at ({q.x.ToString("0.0", ci)}, {q.y.ToString("0.0", ci)}, {q.z.ToString("0.0", ci)}) {(inGoal ? "in" : "OUTSIDE")} the goal; {string.Join("; ", log)}";
            }
            else if (ended == null) route = "RED: s3 has no ringRoute32 row with two rings in the booted map";
            yield return Stop();
            bool ok = framed == lv.rooms.Count && routeOk && ended == null;
            res[0] = $"{(ok ? "PASS" : "RED")}: {framed} of {lv.rooms.Count} rooms framed (shot-N30-span32-<room>.png); ring route {route}{(ended != null ? " | ended: " + ended : "")} | {pc} || " + string.Join(" || ", lines);
        }
    }
}
