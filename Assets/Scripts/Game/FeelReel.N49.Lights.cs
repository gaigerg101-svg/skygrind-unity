// NIGHT49 builder L (design.n49.lights49, a VIEW row; G2-G4, the light side): the reel a449 (FeelReel.N49.cs routes it here). Run it per level
// (reel.sh + -skygrindReelOnly, L-062), once with the build copy's lights49 OFF (the old frames) and once ON (node Tools/night/n39-build-rows.cjs
// <tag> on n49.lights49), at 1920x1080. A campaign run, DOORFREE, god mode; nothing pressed (no fire, no action: the frames are of the room).
//   1. every room: its frames36.mid viewpoint (else its spawn, facing in): shot-L49-<level>-<room>.png (Shot29's numbers: cyan / red-orange shares);
//   2. the boss room: from its spawn, facing the landmark (LightPlan33's Roles49 report: target and light): shot-L49-<level>-landmark.png, after
//      one LightKitRepick + 0.5 s so the landmark light has been re-set against the room's other lights.
// One line: PASS when the row is ON in this run, the plan's counts say cyan on no non-usable row and white on no non-boundary row, the landmark
// light is the strongest at its landmark, and every frame was taken; with the row OFF the line is the OLD control (it prints the same counts as
// the plan WOULD be with the row on, and the frames are the old look). The blind sheet's frame per level: the line's SHEET name.
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
        static partial void N49Lights(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Lights49Scenario(g, dir, res, offs, pre);
        }

        /// <summary>The counts of one report as one string (json colours, drawn before, drawn after, the roles).</summary>
        public static string Counts49(LightPlan33.Roles49Report r)
        {
            string D(SortedDictionary<string, int> d) { var parts = new List<string>(); foreach (var kv in d) parts.Add(kv.Key + " " + kv.Value); return string.Join(", ", parts); }
            return $"{r.rows} campaign.lights rows: json [{D(r.json)}]; drawn before [{D(r.before)}]; drawn after [{D(r.after)}]; roles: usable {r.rowsUsable}, boundary {r.rowsBoundary}, danger {r.rowsDanger}, accent {r.rowsAccent}"
                + $" (undrawn {r.rowsUndrawn}); OFF-ROLE before: cyan on a non-usable row {r.offCyanBefore}, white on a non-boundary row {r.offWhiteBefore}; after: {r.offCyan} / {r.offWhite}"
                + $"; surfaces: pads {r.surf.pads}, grapple nodes {r.surf.nodes}, ride panels {r.surf.ride}, climb steps {r.surf.climb}, fall edges {r.surf.edges.Count}"
                + $"; plan fixtures after: usable lenses {r.fixUsable}, exit lamps {r.exits} (cyan: the way on), boundary {r.fixBoundary}, danger {r.fixDanger}, accent {r.fixAccent}"
                + $"; landmark {r.landmark} in {r.landmarkRoom ?? "none"} (light at {r.lx:0.0},{r.ly:0.0},{r.lz:0.0}); sheet room {r.sheetRoom}";
        }

        static IEnumerator Lights49Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot each campaign level with reel.sh + -skygrindReelOnly, lights49 off (old) and on (new)";
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why + " (" + boot + ")"; yield break; }
            SurvivalSim sim = g.sim;
            bool on = LightPlan33.On49(g.cfg);
            bool look30 = N30Fixes.ViewRowOn(g.cfg, "look30"), lights35 = LightPlan33.Lights35On != null && LightPlan33.Lights35On(g.cfg);
            LightPlan33.Roles49Report rep = LightPlan33.Report49(g.map.json, g.cfg, look30, lights35, g.map.json.TryGetValue("name", out object nm) ? nm as string : "");
            if (rep == null) { res[0] = "RED: LightPlan33.Report49 found no campaign level in this map (" + boot + ")"; yield break; }
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
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, lights49 {(on ? "ON" : "off")}, look33 {N33Fixes.ViewRowOn(g.cfg, "look33")}, lights35 {lights35}, look30 {look30}, screen {Screen.width}x{Screen.height}";
            Start(g, "lights49", "camera", dir);
            yield return Frames(pre);
            var shots = new List<string>();
            var st = new double[4];
            int framed = 0, want = 0;
            string ended = null, sheet = null, landmarkShot = null;
            foreach (RoomDef r in lv.rooms)
            {
                if (ended != null) break;
                want++;
                double[] f = Level36Frame(rows, r.id, "mid");
                if (f != null) { g.Respawn(new Vec3(f[0], f[1], f[2]), f[3]); g.input.SetLook(f[3] * Math.PI / 180, f[4]); }
                else { g.Respawn(r.spawn, r.spawnYawDeg); g.input.SetLook(r.spawnYawDeg * Math.PI / 180, -0.04); }
                yield return Settle31(g, 0.6, () => (ended = Ended30(g)) != null);
                if (ended != null) break;
                string name = $"L49-{lv.id}-{r.id}";
                yield return Shot29(dir, name, st);
                framed++;
                shots.Add(ShotLine29(name, st, ci));
                if (r.id == rep.sheetRoom) sheet = "shot-" + name + ".png";
            }
            // the landmark, from its room's spawn
            RoomDef boss = null;
            foreach (RoomDef r in lv.rooms) if (r.id == rep.landmarkRoom) { boss = r; break; }
            if (boss != null && ended == null)
            {
                want++;
                double dx = rep.tx - boss.spawn.x, dz = rep.tz - boss.spawn.z, dy = rep.ty - (boss.spawn.y + 1.6);
                double yawDeg = Math.Atan2(-dx, -dz) * 180 / Math.PI, pitch = Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz));
                g.Respawn(boss.spawn, yawDeg);
                g.input.SetLook(yawDeg * Math.PI / 180, Math.Max(-0.6, Math.Min(0.6, pitch)));
                yield return Settle31(g, UnityTuning.LightKitRepick + 0.5, () => (ended = Ended30(g)) != null);
                if (ended == null)
                {
                    string name = $"L49-{lv.id}-landmark";
                    yield return Shot29(dir, name, st);
                    framed++;
                    shots.Add(ShotLine29(name, st, ci));
                    landmarkShot = "shot-" + name + ".png";
                }
            }
            string lmNow = LightKit29.Landmark49Now;
            double ratio = LightKit29.Landmark49Ratio;
            yield return Stop();
            bool counts = rep.offCyan == 0 && rep.offWhite == 0;
            bool lit = !on || (lmNow.Length > 0 && ratio >= UnityTuning.L49LandmarkPassRatio - 1e-3) || rep.landmarkRoom == null;
            bool ok = on && counts && lit && framed == want && ended == null;
            res[0] = $"{(ok ? "PASS" : on ? "RED" : "OLD CONTROL (lights49 off)")}: level {lv.id}: usable rows {rep.rowsUsable} of {rep.rows}, OFF-ROLE rows before {rep.offCyanBefore + rep.offWhiteBefore} (cyan {rep.offCyanBefore}, white {rep.offWhiteBefore}) -> after {rep.offCyan + rep.offWhite} (cyan {rep.offCyan}, white {rep.offWhite}); "
                + $"landmark: {(lmNow.Length > 0 ? lmNow : on ? "NO LANDMARK LIGHT SET" : "(row off: none)")}; {framed}/{want} frames{(ended != null ? " | ended: " + ended : "")} | SHEET (the blind sheet's frame for this level): {sheet ?? "none"}, landmark frame {landmarkShot ?? "none"} | "
                + $"counts: {Counts49(rep)} | {pc} | LOOK: cyan only on ride panels / grapple nodes / climb steps and the exit lamp; pale violet-white only at a fall's edge; every other fixture the level's own colour, no halo; the landmark the brightest thing on its side of the boss room || "
                + string.Join(" || ", shots);
        }
    }
}
