// NIGHT33 builder K (design.n33.kit33): the Player scenario of the arena kit (the lead's FeelReel.N27.cs dispatches PhaseN33K to N33KitBody).
//   kit33   every area of the BOOTED campaign level, from its entry at eye height: frame -a facing in (level), frame -b facing in and looking up
//           0.35 rad (the decor above 3 m: trusses, lamps, pipes, banners, the hero landmark). Boot -skygrindMap n33-<level> for GREEN (the dressed
//           file, kit33 row on) and -skygrindMap n32-<level> -skygrindOff33 for RED (NIGHT32's bare rooms; heatVent / checkpoint / plinth / slot as
//           crates). The line prints the level, the kit33 view row, the props the kit drew (PropsView29.Last.kit33Drawn) and one entry per area.
// Shots go to <dir>/shot-N33-kit33-<area>-a/b.png (Shot29's sampled numbers). Every result line starts with PASS or RED. Not run by its author
// (no Player in the worktree).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N33KitBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Kit33Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Kit33Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why + " (boot -skygrindMap n33-foundry | n33-kiln | n33-iceline | n33-span | n33-spire; RED: n32-<level> -skygrindOff33)"; yield break; }
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (boot -skygrindMap n33-<level>)"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            LevelDef lv = sim.campaignLevel;
            bool rowOn = N33Fixes.ViewRowOn(g.cfg, "kit33");
            int drawn = PropsView29.Last != null ? PropsView29.Last.kit33Drawn : -1;
            PropsView29 pv = PropsView29.Last;
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, kit33 view row {rowOn}, kit rows drawn {drawn}"
                + (pv != null ? $", kit mesh objects {pv.kit33Objects} (one stone mesh per room + one per room and glow colour: the draws before culling), {pv.kit33Boxes} boxes, {pv.kit33Vertices} vertices" : "");
            Start(g, "kit33", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            int entered = 0;
            string ended = null;
            foreach (RoomDef r in lv.rooms)
            {
                if (ended != null) break;
                double yaw = r.spawnYawDeg * Math.PI / 180;
                // -a: the entry, facing in, level
                g.Respawn(r.spawn, r.spawnYawDeg);
                g.input.SetLook(yaw, 0);
                yield return Settle31(g, 0.6, () => (ended = Ended30(g)) != null);
                Vec3 ft = g.Feet;
                bool inRoom = ft.x >= r.x0 && ft.x <= r.x1 && ft.z >= r.z0 && ft.z <= r.z1;
                if (inRoom) entered++;
                if (ended == null) yield return Shot33Kit(dir, $"kit33-{r.id}-a", st, shots, ci);
                // -b: the same spot looking up (the decor above 3 m and the landmark)
                g.input.SetLook(yaw, UnityTuning.Kit33ReelLookUp);
                yield return Settle31(g, 0.4, () => (ended = Ended30(g)) != null);
                if (ended == null) yield return Shot33Kit(dir, $"kit33-{r.id}-b", st, shots, ci);
                lines.Add($"{r.id} '{r.name}' entered {inRoom}: {(r.x1 - r.x0).ToString("0", ci)} x {(r.z1 - r.z0).ToString("0", ci)} m{(ended != null ? " | then " + ended : "")}");
            }
            yield return Stop();
            bool ok = entered == lv.rooms.Count && shots.Count >= 2 * lv.rooms.Count && ended == null;
            res[0] = $"{(ok ? "PASS" : "RED")}: {entered} of {lv.rooms.Count} areas entered by respawn, {shots.Count} frames (shot-N33-kit33-<area>-a/b.png){(ended != null ? " | ended: " + ended : "")} | {pc} | "
                + "LOOK (GREEN): skirt + cornice on every wall, a rib / recess / pilaster every 6-8 m, every doorway framed with a warm lintel, trusses / pipes / lamps / banners above head height, ONE landmark per area, the floor clean, no cyan decor; (RED) bare walls || "
                + string.Join(" || ", lines) + " || shots: " + string.Join("; ", shots);
        }

        static IEnumerator Shot33Kit(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N33-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }
    }
}
