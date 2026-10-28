// NIGHT33 builder R (DESIGN33 card R; rows design.n33.arenas33 / devices33): the Player scenario of the overhauled rooms and the devices (the lead's
// FeelReel.N27.cs dispatches PhaseN33R to N33ArenasBody).
//   arenas33  boot -skygrindMap n33-foundry | n33-iceline | n33-kiln | n33-span | n33-spire (a CAMPAIGN level; one set per map). A campaign run, DOORFREE,
//             god mode. For each overhauled room of the booted level: a frame from its ENTRY at eye height facing in (-a). Then for each device in those rooms,
//             from its refuge facing the device: a FAN (-fan: the cyan rim and the rising motes; then the player stood on the grate, a frame 0.5 s into the
//             lift: -lift), a VENT (-tell: the red-orange tell glowing; -burst: the burst column), a PLATFORM (-p0, then -p1 1.5 s later: it moved).
//             The devices run when the run has devices33 on; when the design row is off (and the run is not forced off: -skygrindOff33) the reel turns
//             them on for this run (SurvivalSim.Devices33Force) and says so. With -skygrindOff33 (the RED reel) nothing is forced: no device frames.
// Shots go to <dir>/shot-N33-arenas33-*.png (Shot29's sampled numbers). Every result line starts with PASS or RED. Not run by its author (no Player
// in the worktree).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N33ArenasBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Arenas33Scenario(g, dir, res, offs, pre);
        }

        /// <summary>The overhauled rooms per n33 level id (DESIGN33 R, the lead's approved list), in the order they are framed.</summary>
        static readonly Dictionary<string, string[]> Arenas33Rooms = new Dictionary<string, string[]>
        {
            { "n33-foundry", new[] { "f9", "f8" } },
            { "n33-iceline", new[] { "i2", "i5" } },
            { "n33-kiln", new[] { "k1", "k4" } },
            { "n33-span", new[] { "s6", "s1" } },
            { "n33-spire", new[] { "v4", "v2" } },
        };

        static IEnumerator Shot33R(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N33-arenas33-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }

        /// <summary>Web yaw (degrees) that looks from `a` toward `b` (yaw 0 looks toward -z, + turns left / west).</summary>
        static double YawTo33(Vec3 a, Vec3 b) => Math.Atan2(-(b.x - a.x), -(b.z - a.z)) * 180 / Math.PI;

        /// <summary>A standing spot `dist` m from the device centre `at` (the direction of `toward` first, then seven more round it): the eye ray to the
        /// device is clear of the world and a downward ray finds floor within 0.4 m of the device's floor. None found: `toward` itself.</summary>
        static Vec3 Spot33(SurvivalSim sim, Vec3 at, Vec3 toward, double dist) // the eye 1.6 m up; the vent burst (2.5 m) is seen whole from 4 m
        {
            double a0 = Math.Atan2(toward.z - at.z, toward.x - at.x);
            for (int k = 0; k < 8; k++)
            {
                double a = a0 + (k % 2 == 0 ? 1 : -1) * ((k + 1) / 2) * Math.PI / 4;
                var c = new Vec3(at.x + Math.Cos(a) * dist, at.y, at.z + Math.Sin(a) * dist);
                var eye = new Vec3(c.x, at.y + 1.6, c.z);
                var tgt = new Vec3(at.x, at.y + 0.5, at.z);
                double dx = tgt.x - eye.x, dy = tgt.y - eye.y, dz = tgt.z - eye.z, len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                if (sim.world.RayIntersect(eye, new Vec3(dx / len, dy / len, dz / len), out RayHit h) && h.distance < len - 0.3) continue;
                if (!sim.world.RayIntersect(new Vec3(c.x, at.y + 1.6, c.z), new Vec3(0, -1, 0), out RayHit f) || Math.Abs(at.y + 1.6 - f.distance - at.y) > 0.4) continue;
                return c;
            }
            return toward;
        }

        static IEnumerator Arenas33Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot -skygrindMap n33-foundry | n33-iceline | n33-kiln | n33-span | n33-spire";
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why + " (" + boot + ")"; yield break; }
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (" + boot + ")"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            LevelDef lv = sim.campaignLevel;
            if (!Arenas33Rooms.TryGetValue(lv.id, out string[] ids)) { res[0] = $"RED: level '{lv.id}' is not an n33 level (" + boot + ")"; yield break; }
            string forced = "";
            if (!sim.n33.devices33 && !N33Blocks.ForceOff) { sim.Devices33Force(true); forced = " (design.n33.devices33 is off: the reel turned the devices on for this run)"; }
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, n33.devices33 {sim.n33.devices33}{forced}, off33 {N33Blocks.ForceOff}";
            Start(g, "arenas33", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            int entered = 0, devShots = 0, devWanted = 0;
            string ended = null;
            var want = new List<RoomDef>();
            foreach (string id in ids) { RoomDef r = lv.rooms.Find(q => q.id == id); if (r != null) want.Add(r); }
            foreach (RoomDef r in want)
            {
                if (ended != null) break;
                g.Respawn(r.spawn, r.spawnYawDeg);
                yield return Settle31(g, 0.6, () => (ended = Ended30(g)) != null);
                Vec3 ft = g.Feet;
                bool inRoom = ft.x >= r.x0 && ft.x <= r.x1 && ft.z >= r.z0 && ft.z <= r.z1;
                if (inRoom) entered++;
                if (ended == null) yield return Shot33R(dir, $"{r.id}-a", st, shots, ci);
                lines.Add($"{r.id} '{r.name}' entered {inRoom}: {(r.x1 - r.x0).ToString("0", ci)} x {(r.z1 - r.z0).ToString("0", ci)} m, enemies alive {Alive30(sim)}");
                Devices33 dev = sim.devices33;
                if (dev == null) continue;
                for (int i = 0; i < dev.Count && ended == null; i++)
                {
                    Device33 d = dev.devices[i];
                    if (d.room != r.id) continue;
                    devWanted++;
                    Vec3 at = new Vec3(d.CenterX, d.kind == Device33Kind.Platform ? d.pos.y : d.y, d.CenterZ);
                    Vec3 from = d.refuge ?? new Vec3(at.x, at.y, at.z + 5);
                    double yaw = YawTo33(from, at);
                    g.Respawn(from, yaw);
                    g.input.SetLook(yaw * Math.PI / 180, -0.15);
                    string tag = $"{r.id}-{i}";
                    if (d.kind == Device33Kind.Fan)
                    {
                        yield return Settle31(g, 0.5, () => (ended = Ended30(g)) != null);
                        if (ended != null) break;
                        yield return Shot33R(dir, tag + "-fan", st, shots, ci);
                        g.Respawn(new Vec3(d.CenterX, d.y, d.CenterZ), yaw);
                        g.input.SetLook(yaw * Math.PI / 180, -0.35);
                        yield return Settle31(g, 0.5, () => (ended = Ended30(g)) != null);
                        double rise = g.Feet.y - d.y;
                        if (ended == null) { yield return Shot33R(dir, tag + "-lift", st, shots, ci); devShots++; }
                        lines.Add($"fan #{i} in {r.id}: the feet {rise.ToString("0.0", ci)} m over the grate 0.5 s after standing on it, motes shown {Devices33View.FansShown}");
                    }
                    else if (d.kind == Device33Kind.Vent)
                    {
                        // round 3 (the lead's Player n33-r6: the camera stood IN the vent, the frame one opaque column; "burst drawn False"): the vent is framed
                        // from 4 m away (the refuge's side first, then the other seven directions: the first spot with a clear ray to the vent and floor under it),
                        // and each Shown flag is read a frame after the phase starts (the view's LateUpdate sets it after the coroutine's resume)
                        Vec3 view = Spot33(sim, at, from, 4.0);
                        double vyaw = YawTo33(view, at);
                        g.Respawn(view, vyaw);
                        g.input.SetLook(vyaw * Math.PI / 180, -0.2);
                        int k = 0;
                        for (; k < Ms(6000) && d.vent != Vent33Phase.Warn; k++) { God(g); yield return null; }
                        for (int w = 0; w < Ms(300) && d.vent == Vent33Phase.Warn; w++) { God(g); yield return null; } // into the tell: the glow has risen
                        bool tell = d.vent == Vent33Phase.Warn && Devices33View.TellShown;
                        if (d.vent == Vent33Phase.Warn) yield return Shot33R(dir, tag + "-tell", st, shots, ci);
                        for (k = 0; k < Ms(2000) && d.vent != Vent33Phase.Burst; k++) { God(g); yield return null; }
                        for (int w = 0; w < 3 && d.vent == Vent33Phase.Burst; w++) { God(g); yield return null; } // the view's LateUpdate has drawn the burst
                        bool burst = d.vent == Vent33Phase.Burst && Devices33View.BurstShown;
                        if (d.vent == Vent33Phase.Burst) { yield return Shot33R(dir, tag + "-burst", st, shots, ci); }
                        if (tell && burst) devShots++;
                        double away = Math.Sqrt((g.Feet.x - at.x) * (g.Feet.x - at.x) + (g.Feet.z - at.z) * (g.Feet.z - at.z));
                        lines.Add($"vent #{i} in {r.id}: framed from {away.ToString("0.0", ci)} m, tell drawn {tell}, burst drawn {burst}");
                    }
                    else
                    {
                        yield return Settle31(g, 0.3, () => (ended = Ended30(g)) != null);
                        if (ended != null) break;
                        Vec3 p0 = d.pos;
                        yield return Shot33R(dir, tag + "-p0", st, shots, ci);
                        yield return Settle31(g, 1.5, () => (ended = Ended30(g)) != null);
                        Vec3 p1 = d.pos;
                        double moved = Math.Sqrt((p1.x - p0.x) * (p1.x - p0.x) + (p1.y - p0.y) * (p1.y - p0.y) + (p1.z - p0.z) * (p1.z - p0.z));
                        if (ended == null) yield return Shot33R(dir, tag + "-p1", st, shots, ci);
                        if (moved > 0.5 || Devices33View.PlatformsShown > 0) devShots++;
                        lines.Add($"platform #{i} in {r.id}: moved {moved.ToString("0.0", ci)} m between -p0 and -p1, platforms drawn {Devices33View.PlatformsShown}");
                    }
                }
            }
            yield return Stop();
            bool ok = want.Count == ids.Length && entered == want.Count && ended == null && devShots == devWanted && (devWanted > 0 || N33Blocks.ForceOff);
            res[0] = $"{(ok ? "PASS" : "RED")}: {entered} of {ids.Length} overhauled rooms entered, {devShots} of {devWanted} devices framed working, {shots.Count} frames (shot-N33-arenas33-<room>-a, -<room>-<i>-fan/-lift/-tell/-burst/-p0/-p1){(ended != null ? " | ended: " + ended : "")} | {pc} | "
                + "LOOK: -a: two or more heights, cover pieces, the landmark, a ramp up; -tell: a red-orange grate; -burst: a column over it; -lift: the floor dropping away; -p0/-p1: the slab moved, cyan edges || "
                + string.Join(" || ", lines) + " || shots: " + string.Join("; ", shots);
        }
    }
}
