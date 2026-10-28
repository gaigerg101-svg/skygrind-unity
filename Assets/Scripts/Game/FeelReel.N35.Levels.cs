// NIGHT35 builder A (NIGHT35.md A1; row design.n35.campaign6): the Player scenario `levels35` (the lead's FeelReel.N27.cs dispatches PhaseN35A to
// N35LevelsBody). It is the BOOT CHECK of each changed level (L-043: the Editor never loads a campaign level) and its frames.
//   levels35  boot -skygrindMap n35-foundry | n35-iceline | n35-kiln | n35-finale (GREEN; one Player per map, L-046) or the n34 file of the same level
//             with -skygrindOff35 (RED: NIGHT34's rooms, no ride hall). A campaign run, DOORFREE, god mode. Frames: every RIDE ROOM (a room row with
//             `ride35`, the Lucio semi-platforming rooms) and, when there is none (RED side), the level's first two fight rooms: -a the room's spawn at
//             eye height facing in (the panels / pillars and the lava ahead), -b the entry floor's west end facing north up the ride line, -c the room's
//             north-west corner (the deck) facing back over the lava. Then one `[reel] levels35:` line: the level booted, the order's place, the ride
//             rooms framed, the teach35 trigger ids in the file, the grapple nodes per framed room and whether the grapple is owned on this level
//             (D-3: owned from design.n35.grapple35.fromLevel), the weapons the run started with.
// Shots go to <dir>/shot-N35-levels35-<room>-a/-b/-c.png (Shot29's sampled numbers). The result starts with PASS or RED. Not run by its author (no Player
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
        static partial void N35LevelsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Levels35Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Shot35A(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N35-levels35-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }

        /// <summary>The map JSON's campaign room rows by id (the generators' extra keys: ride35, teach35, d3).</summary>
        static Dictionary<string, Dictionary<string, object>> Levels35Rows(SurvivalGame g)
        {
            var res = new Dictionary<string, Dictionary<string, object>>();
            if (g.map != null && g.map.json != null && g.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("rooms", out object ro) && ro is List<object> rows)
                foreach (object o in rows)
                    if (o is Dictionary<string, object> r && r.TryGetValue("id", out object id) && id is string sid) res[sid] = r;
            return res;
        }

        static IEnumerator Levels35Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot -skygrindMap n35-foundry | n35-iceline | n35-kiln | n35-finale (GREEN) or n34-foundry | n34-iceline | n34-kiln -skygrindOff35 (RED)";
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
            var rows = Levels35Rows(g);
            List<string> order = Curriculum25.Order(g.cfg, new N25Blocks { tutorial = true, curriculum = true });
            int place = order.IndexOf(lv.id) + 1;
            var teach = new List<string>(); // builder T's format: the level's campaign.teach35 [{id, room, rect, minY}]
            if (g.map != null && g.map.json != null && g.map.json.TryGetValue("campaign", out object cj) && cj is Dictionary<string, object> cmp && cmp.TryGetValue("teach35", out object t) && t is List<object> tl)
                foreach (object tt in tl) if (tt is Dictionary<string, object> td && td.TryGetValue("id", out object tid)) teach.Add($"{tid}@{(td.TryGetValue("room", out object rm) ? rm : "?")}");
            bool owned = sim.arcline21 != null && sim.arcline21.Owned;
            string weapons = sim.loadout != null ? string.Join(",", sim.loadout.owned) : "?";
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id} (campaign place {place} of {order.Count}: {string.Join(",", order)}), campaign6 {g.cfg.Design("n35.campaign6.enabled")}, off35 {N35Blocks.ForceOff}, grapple35Here {sim.grapple35Here}, grapple owned {owned}, weapons [{weapons}]";
            Start(g, "levels35", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            var want = new List<RoomDef>();
            foreach (RoomDef r in lv.rooms) if (rows.TryGetValue(r.id, out var row) && row.ContainsKey("ride35")) want.Add(r);
            bool rideRooms = want.Count > 0;
            if (!rideRooms) foreach (RoomDef r in lv.rooms) if (r.kind != "boss" && r.waves.Count > 0 && want.Count < 2) want.Add(r);
            int entered = 0, framed = 0;
            string ended = null;
            foreach (RoomDef r in want)
            {
                if (ended != null) break;
                int nodes = 0;
                foreach (Vec3 n in sim.map.grappleNodes) if (n.x >= r.x0 && n.x <= r.x1 && n.z >= r.z0 && n.z <= r.z1) nodes++;
                var centre = new Vec3((r.x0 + r.x1) / 2, 0, (r.z0 + r.z1) / 2);
                // -a: the spawn, facing in
                g.Respawn(r.spawn, r.spawnYawDeg);
                g.input.SetLook(r.spawnYawDeg * Math.PI / 180, -0.05);
                yield return Settle31(g, 0.6, () => (ended = Ended30(g)) != null);
                Vec3 ft = g.Feet;
                bool inRoom = ft.x >= r.x0 && ft.x <= r.x1 && ft.z >= r.z0 && ft.z <= r.z1;
                if (inRoom) entered++;
                if (ended == null) { yield return Shot35A(dir, $"{r.id}-a", st, shots, ci); framed++; }
                // -b: the entry floor's west end (web: +z south; the entry floor is the room's south 5 m), facing north up the ride line
                var sw = new Vec3(r.x0 + 1.5, r.spawn.y, r.z1 - 2.0);
                var up = new Vec3(r.x0 + 4.0, 0, r.z0 + 4.0);
                double yb = YawTo33(sw, up);
                g.Respawn(sw, yb);
                g.input.SetLook(yb * Math.PI / 180, 0.08);
                yield return Settle31(g, 0.5, () => (ended = Ended30(g)) != null);
                if (ended == null) { yield return Shot35A(dir, $"{r.id}-b", st, shots, ci); framed++; }
                // -c: the north-west corner (the ride rooms' deck), facing back over the room's centre
                var nw = new Vec3(r.x0 + 1.5, 0, r.z0 + 1.5);
                double yc = YawTo33(nw, centre);
                g.Respawn(nw, yc);
                g.input.SetLook(yc * Math.PI / 180, -0.12);
                yield return Settle31(g, 0.5, () => (ended = Ended30(g)) != null);
                if (ended == null) { yield return Shot35A(dir, $"{r.id}-c", st, shots, ci); framed++; }
                string line = rows.TryGetValue(r.id, out var rr) && rr.TryGetValue("ride35", out object ro) && ro is Dictionary<string, object> rd && rd.TryGetValue("line", out object lo) ? lo as string : "no ride35 row";
                string d3 = rr != null && rr.TryGetValue("d3", out object dd) ? " | " + dd : "";
                lines.Add($"{r.id} '{r.name}' entered {inRoom}: {(r.x1 - r.x0).ToString("0", ci)} x {(r.z1 - r.z0).ToString("0", ci)} m, grapple nodes {nodes}, enemies alive {Alive30(sim)}, line: {line}{d3}");
            }
            yield return Stop();
            bool ok = rideRooms && entered == want.Count && framed == 3 * want.Count && ended == null;
            res[0] = $"{(ok ? "PASS" : "RED")}: level {lv.id} booted (place {place} of {order.Count}); {want.Count} {(rideRooms ? "ride rooms" : "fight rooms (no ride room: the RED side)")}, {entered} entered, {framed} of {3 * want.Count} frames (shot-N35-levels35-<room>-a spawn / -b up the ride line / -c from the deck); "
                + $"teach35 triggers [{string.Join(", ", teach)}]{(ended != null ? " | ended: " + ended : "")} | {pc} | "
                + "LOOK: -a: two cyan-striped ride panels (or four pillars) standing in red-orange lava, the dry lane to the east; -b: the panels staggered up the room, faces alternating; -c: the deck over the lava, the pillar || "
                + string.Join(" || ", lines) + " || shots: " + string.Join("; ", shots);
        }
    }
}
