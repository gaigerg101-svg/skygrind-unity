// NIGHT35 builder T (design.n35.teach35, NIGHT35.md T1): the Player scenarios of PhaseN35T (FeelReel.N27.cs dispatches to N35TeachBody):
//   teach35  boot -skygrindMap <L1 id> (n34-foundry or A's n35 swap) -skygrindReelOnly teach35 (GREEN: the row on) / with -skygrindOff35 (RED: no line).
//            A CAMPAIGN run on the booted level, DOORFREE, god mode, the profile READ-ONLY (nothing is written to disk) and its teach35 set emptied for the
//            run (a fresh profile, D-7; put back after). For each of the level's teaching lines in the table's order: the player is put in its trigger
//            (the rect's centre from the level file, else its fallback room's spawn), the line is waited for in the message slot (Ui.Teach35IdNow), a frame
//            is shot 0.4 s after it shows (shot-N35-teach35-<id>.png), and the time it stood is read when it goes (Ui.Teach35LastHeldS). Then the same rooms
//            again in a NEW run on the same (now marked) profile: no line may fire (once per profile).
//            One line `[reel] teach35: PASS|RED ...`: per line its text as drawn, the slot's px (>= 16), the seconds it stood (5 +- 0.6), the trigger kind.
//   verbs35  FeelReel.N35.Verbs.cs.
// Not run by its author (no Player in the worktree).
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
        static partial void N35TeachBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "teach35") body = Teach35Scenario(g, dir, res, offs, pre);
            else if (s == "verbs35") body = Verbs35Scenario(g, dir, res, offs, pre);
        }

        /// <summary>A CAMPAIGN run on the booted level with DOORFREE and god mode; null when it started, else why not.</summary>
        static IEnumerator Campaign35(SurvivalGame g, List<Action> offs, string[] why)
        {
            why[0] = N24CampaignGate(g);
            if (why[0] != null) yield break;
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { why[0] = "RED: the run did not start as a CAMPAIGN"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
        }

        /// <summary>Where a line fires: its rect's centre (the room's floor height under it), else its fallback room's spawn.</summary>
        static bool Teach35Spot(SurvivalSim sim, Teach35Line l, out Vec3 at, out double yawDeg)
        {
            at = default;
            yawDeg = 0;
            LevelDef lv = sim.campaignLevel;
            Dictionary<string, Teach35Rect> rects = sim.map.json != null && sim.map.json.TryGetValue("campaign", out object co) ? Teach35Table.Rects(co as Dictionary<string, object>) : new Dictionary<string, Teach35Rect>();
            if (rects.TryGetValue(l.id, out Teach35Rect r))
            {
                double cx = (r.x0 + r.x1) / 2, cz = (r.z0 + r.z1) / 2;
                RoomDef home = null;
                foreach (RoomDef rd in lv.rooms) if (cx >= rd.x0 && cx <= rd.x1 && cz >= rd.z0 && cz <= rd.z1) { home = rd; break; }
                double y = r.minY > -1e8 ? r.minY + 0.3 : home != null ? home.spawn.y + 0.3 : sim.mover.position.y;
                at = new Vec3(cx, y, cz);
                yawDeg = home != null ? home.spawnYawDeg : 0;
                return true;
            }
            foreach (RoomDef rd in lv.rooms)
                if (rd.id == l.room) { at = rd.spawn; yawDeg = rd.spawnYawDeg; return true; }
            return false;
        }

        static double Teach35Seconds(SurvivalSim sim, string id)
        {
            foreach (Teach35Event e in sim.teach35Emitted) if (e.id == id) return e.seconds;
            return double.NaN;
        }

        static IEnumerator Teach35Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot -skygrindMap <L1 id> -skygrindReelOnly teach35 (GREEN) or + -skygrindOff35 (RED)";
            var why = new string[1];
            // the profile: read-only for the reel, its teach35 set emptied (a fresh profile) and put back after
            bool roWas = g.profileReadOnly;
            var seenWas = new List<string>(g.profile.teach35);
            offs.Add(() => { g.profileReadOnly = roWas; g.profile.teach35.Clear(); g.profile.teach35.UnionWith(seenWas); });
            g.profileReadOnly = true;
            g.profile.teach35.Clear();
            yield return Campaign35(g, offs, why);
            if (why[0] != null) { res[0] = why[0] + " (" + boot + ")"; yield break; }
            SurvivalSim sim = g.sim;
            for (int k = 0; k < 10 && sim.teach35 == null && sim.n35.teach35; k++) yield return null; // built on the run's first step
            string pc = $"level {sim.campaignLevel.id}, n35.teach35 {sim.n35.teach35}, off35 {N35Blocks.ForceOff}, device {PlayerDevices.Last}";
            if (!sim.n35.teach35 || sim.teach35 == null)
            {
                res[0] = $"RED: no teaching line (teach35 {(sim.n35.teach35 ? "on but not built" : "off")}) | {pc}";
                yield break;
            }
            var lines = new List<Teach35Line>(sim.teach35.LevelLines);
            Start(g, "teach35", "ui", dir);
            yield return Frames(pre);
            var rows = new List<string>();
            var st = new double[4];
            int ok = 0;
            double wallEnd = Time.realtimeSinceStartup + 240;
            foreach (Teach35Line l in lines)
            {
                if (Time.realtimeSinceStartup > wallEnd) { rows.Add($"{l.id} RED: wall clock out"); continue; }
                if (!Teach35Spot(sim, l, out Vec3 at, out double yaw)) { rows.Add($"{l.id} RED: no rect and no room '{l.room}' in the level"); continue; }
                int shows0 = Ui.Teach35Shows;
                g.Respawn(at, yaw);
                g.input.SetLook(yaw * Math.PI / 180, 0);
                // wait for this line in the slot (it may queue behind the one before it)
                double t0 = Time.realtimeSinceStartup;
                while (Ui.Teach35IdNow != l.id && Time.realtimeSinceStartup - t0 < 16) { God(g); yield return null; }
                if (Ui.Teach35IdNow != l.id) { rows.Add($"{l.id} RED: not shown in 16 s (emitted: {string.Join(", ", sim.teach35.shown)})"); continue; }
                double up0 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - up0 < 0.4) { God(g); yield return null; }
                string text = Ui.Teach35TextNow;
                float px = Ui.Teach35PxNow;
                yield return Shot29(dir, "N35-teach35-" + l.id, st);
                while (Ui.Teach35IdNow == l.id && Time.realtimeSinceStartup - up0 < 12) { God(g); yield return null; }
                double held = Ui.Teach35LastHeldS;
                string trig = sim.teach35.HasRect(l.id) ? "rect" : "room " + l.room;
                bool good = !string.IsNullOrEmpty(text) && text.IndexOf('{') < 0 && px >= 16 && Math.Abs(held - Teach35Seconds(sim, l.id)) <= 0.6;
                if (good) ok++;
                rows.Add($"{l.id} {(good ? "PASS" : "RED")}: \"{text}\" {px.ToString("0", ci)} px, stood {held.ToString("0.00", ci)} s, by {trig}");
            }
            // once per profile: a new run, the same spots, nothing may fire
            yield return Campaign35(g, offs, why);
            int again = -1;
            if (why[0] == null)
            {
                for (int k = 0; k < 10 && sim.teach35 == null; k++) yield return null;
                foreach (Teach35Line l in lines)
                {
                    if (!Teach35Spot(sim, l, out Vec3 at, out double yaw)) continue;
                    g.Respawn(at, yaw);
                    yield return Settle31(g, 0.5, () => false);
                }
                again = sim.teach35Emitted.Count;
            }
            yield return Stop();
            bool pass = lines.Count > 0 && ok == lines.Count && again == 0;
            res[0] = $"{(pass ? "PASS" : "RED")}: {ok} of {lines.Count} level lines shown in their trigger (text resolved, >= 16 px, ~5 s); second run on the same profile emitted {again} (want 0) | {pc} || "
                + string.Join(" || ", rows) + " || shots: shot-N35-teach35-<id>.png";
        }
    }
}
