// NIGHT34 builder A (NIGHT34.md A1; rows design.n34.lines34 / calm34): the Player scenario `lines34` (the lead's FeelReel.N27.cs dispatches PhaseN34A to
// N34LinesBody). It is the BOOT CHECK of each changed level (L-043: the Editor never loads a campaign level) and its frames.
//   lines34  boot -skygrindMap n34-foundry | n34-kiln (GREEN; one set per map) or n33-foundry | n33-kiln with -skygrindOff34 (RED: NIGHT33's rooms, the
//            dressing uncut). A campaign run, DOORFREE, god mode. For the level's first two fight rooms (a room with waves, not a boss arena), three
//            fixed spots each: -a the room's spawn at eye height facing in; -b the room's south-east corner (1.5 m in) facing the room's centre;
//            -c under the room's first grapple node (4 m south of it, its floor), looking up at the node (the cyan ring); with no node in the room, -c
//            is the north-west corner facing the centre. Then one `[reel] lines34:` line: the level id booted, the dressing rows in its file, how many
//            calm34 keeps (Calm34.Keep with the row's keep set) and how many the kit drew (PropsView29.Last.kit33Drawn), the grapple nodes per framed room.
// Shots go to <dir>/shot-N34-lines34-<room>-a/-b/-c.png (Shot29's sampled numbers). The result starts with PASS or RED. Not run by its author (no Player
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
        static partial void N34LinesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Lines34Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Shot34A(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N34-lines34-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }

        static IEnumerator Lines34Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot -skygrindMap n34-foundry | n34-kiln (GREEN) or n33-foundry | n33-kiln -skygrindOff34 (RED)";
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
            bool calmOn = N34Fixes.ViewRowOn(g.cfg, "calm34");
            HashSet<string> keep = calmOn ? Calm34.KeepKinds(g.cfg) : null;
            int dressing = 0, kept = 0;
            if (g.map != null && g.map.json != null && g.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("props", out object po) && po is List<object> rows)
                foreach (object ro in rows)
                {
                    if (!(ro is Dictionary<string, object> r)) continue;
                    string id = r.TryGetValue("id", out object io) ? io as string : null;
                    if (!Calm34.IsDressing(id)) continue;
                    dressing++;
                    if (Calm34.Keep(keep, id, r.TryGetValue("kind", out object ko) ? ko as string : null)) kept++;
                }
            int drawn = PropsView29.Last != null ? PropsView29.Last.kit33Drawn : -1;
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, lines34 {N34Fixes.ViewRowOn(g.cfg, "lines34")}, calm34 {calmOn}, off34 {N34Blocks.ForceOff}";
            Start(g, "lines34", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            var want = new List<RoomDef>();
            foreach (RoomDef r in lv.rooms) if (r.kind != "boss" && r.waves.Count > 0 && want.Count < 2) want.Add(r);
            int entered = 0, framed = 0;
            string ended = null;
            foreach (RoomDef r in want)
            {
                if (ended != null) break;
                var nodes = new List<Vec3>();
                foreach (Vec3 n in sim.map.grappleNodes) if (n.x >= r.x0 && n.x <= r.x1 && n.z >= r.z0 && n.z <= r.z1) nodes.Add(n);
                var centre = new Vec3((r.x0 + r.x1) / 2, 0, (r.z0 + r.z1) / 2);
                // -a: the spawn, facing in
                g.Respawn(r.spawn, r.spawnYawDeg);
                g.input.SetLook(r.spawnYawDeg * Math.PI / 180, -0.05);
                yield return Settle31(g, 0.6, () => (ended = Ended30(g)) != null);
                Vec3 ft = g.Feet;
                bool inRoom = ft.x >= r.x0 && ft.x <= r.x1 && ft.z >= r.z0 && ft.z <= r.z1;
                if (inRoom) entered++;
                if (ended == null) { yield return Shot34A(dir, $"{r.id}-a", st, shots, ci); framed++; }
                // -b: the south-east corner (web: +x east, +z south), 1.5 m in, facing the centre
                var se = new Vec3(r.x1 - 1.5, r.spawn.y, r.z1 - 1.5);
                double yb = YawTo33(se, centre);
                g.Respawn(se, yb);
                g.input.SetLook(yb * Math.PI / 180, 0.05);
                yield return Settle31(g, 0.5, () => (ended = Ended30(g)) != null);
                if (ended == null) { yield return Shot34A(dir, $"{r.id}-b", st, shots, ci); framed++; }
                // -c: under the first node (4 m south of it) looking up at it; no node: the north-west corner facing the centre
                Vec3 from; double yc, pitch;
                if (nodes.Count > 0)
                {
                    Vec3 n0 = nodes[0];
                    double fz = Math.Min(r.z1 - 1.0, n0.z + 4);
                    from = new Vec3(n0.x, 0, fz);
                    yc = YawTo33(from, n0);
                    double dz = Math.Abs(fz - n0.z);
                    pitch = Math.Atan2(n0.y - 1.6, Math.Max(0.5, dz));
                }
                else { from = new Vec3(r.x0 + 1.5, 0, r.z0 + 1.5); yc = YawTo33(from, centre); pitch = 0.05; }
                g.Respawn(from, yc);
                g.input.SetLook(yc * Math.PI / 180, pitch);
                yield return Settle31(g, 0.5, () => (ended = Ended30(g)) != null);
                if (ended == null) { yield return Shot34A(dir, $"{r.id}-c", st, shots, ci); framed++; }
                lines.Add($"{r.id} '{r.name}' entered {inRoom}: {(r.x1 - r.x0).ToString("0", ci)} x {(r.z1 - r.z0).ToString("0", ci)} m, grapple nodes {nodes.Count}, enemies alive {Alive30(sim)}");
            }
            yield return Stop();
            bool ok = want.Count == 2 && entered == want.Count && framed == 3 * want.Count && ended == null;
            res[0] = $"{(ok ? "PASS" : "RED")}: level {lv.id} booted; {entered} of {want.Count} fight rooms entered, {framed} of {3 * want.Count} frames (shot-N34-lines34-<room>-a spawn / -b south-east corner / -c under the first node); "
                + $"dressing rows {dressing}, calm34 keeps {kept} ({(dressing > 0 ? (100.0 * (dressing - kept) / dressing).ToString("0.0", ci) : "0")} % cut), the kit drew {drawn}{(ended != null ? " | ended: " + ended : "")} | {pc} | "
                + "LOOK: -a / -b: two or more floors, a ramp up, plain walls (no ribs, banners, pipes with calm34 on); -c: the cyan node ring against the roof, the floor and wall it chains to || "
                + string.Join(" || ", lines) + " || shots: " + string.Join("; ", shots);
        }
    }
}
