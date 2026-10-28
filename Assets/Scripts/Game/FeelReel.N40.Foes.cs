// NIGHT40 builder E3: the Player scenario `lineup40` (models40's proof: NIGHT40.md card E3). The lead's dispatch (FeelReel.N27.cs): add
// `|| Array.IndexOf(PhaseN40E, s) >= 0` to IsPhaseN37 and, in the body switch, `else if (Array.IndexOf(PhaseN40E, s) >= 0) N40FoesBody(s, g, dir, res, offs, pre, ref body);`.
// Not run by its author (no Player in the worktree). Boot any map in SURVIVAL (the survival map is fine); the models40 parts are drawn only when the build's
// config has design.n40.models40.enabled true (the views read it once at boot: the "before" frames are n39-r4's or a build with the row off). The player is
// put on open floor (a grid search: 40 m of clear floor ahead, 14 m to each side), God, input through forced frames only (L-046); the enemies are frozen
// (husks.frozen: nobody moves) and stood in a row across the view, 3.5 m apart, in this order: husk, tank (a husk body with tank29), swarmer, spitter,
// charger, warden, drone; each faces the player.
// Frames (shot-N29-<name>.png), what each should show:
//   E40-lineup-30      the seven at 30 m, lit (dusk), at rest: a different outline per kind (the husk's hunched yoke, the tank's square slab, the
//                      swarmer's spine and tail wedges, the spitter's snout and glowing sac bulb, the charger's plough, hump and red-orange eye slit, the
//                      warden's pauldrons, back spike and bright chest plate, the drone's flat saucer rim and yellow emitter spike).
//   E40-silhouette-30  made from two frames (the row, then the same view with the row moved away): every pixel that changed BLACK, the rest WHITE
//                      (the TF2 test: each kind named from its black outline alone; a fresh reader with no labels should name 6 of 7).
//   E40-lineup-12      the same row at 12 m (the parts' detail).
//   E40-lineup-tells   at 12 m, every kind in its wind-up (husk melee, swarmer crouch, spitter volley tell, charger tell, warden tell, drone charge):
//                      the red-orange eyes / glow and the YELLOW drone spike and spitter bulb lit, the tell poses.
// Line: GREEN when the seven were placed and the four frames saved; the parts' count printed (Enemy40Parts in the scene; 0 = the row off in this build).
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN40E = { "lineup40" };

        static readonly string[] Lineup40Kinds = { "husk", "tank", "swarmer", "spitter", "charger", "warden", "drone" };

        static void N40FoesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "lineup40") body = Lineup40Scenario(g, dir, res, offs, pre);
        }

        /// <summary>Open floor for the row: from a grid of points, one with `ahead` m clear along a direction and `side` m either side of that line.</summary>
        static bool Lineup40Spot(SurvivalSim sim, double ahead, double side, out Vec3 at, out double ux, out double uz)
        {
            bool Clear(Vec3 c)
            {
                var q = new Vec3(c.x, c.y + 1.0, c.z);
                if (WorldCollider.CapsuleIntersect(sim.world, new Capsule { start = q, end = new Vec3(q.x, q.y + 1.5, q.z), radius = 0.6 }, out _, out _)) return false;
                return sim.world.RayIntersect(new Vec3(c.x, c.y + 0.5, c.z), new Vec3(0, -1, 0), out RayHit hit) && hit.distance < 1.0;
            }
            for (int gx = -12; gx <= 12; gx++)
                for (int gz = -12; gz <= 12; gz++)
                {
                    var top = new Vec3(gx * 8.0, 60, gz * 8.0);
                    if (!sim.world.RayIntersect(top, new Vec3(0, -1, 0), out RayHit down)) continue;
                    var p = new Vec3(top.x, top.y - down.distance + 0.05, top.z);
                    for (int k = 0; k < 8; k++)
                    {
                        double a = k * Math.PI / 4, dx = Math.Sin(a), dz = -Math.Cos(a);
                        bool ok = true;
                        for (double d = 0; d <= ahead && ok; d += 2)
                            for (double w = -side; w <= side && ok; w += 3.5)
                                ok = Clear(new Vec3(p.x + dx * d - dz * w, p.y, p.z + dz * d + dx * w));
                        if (!ok) continue;
                        at = p;
                        ux = dx;
                        uz = dz;
                        return true;
                    }
                }
            at = sim.map.playerSpawn;
            ux = 0;
            uz = -1;
            return false;
        }

        static IEnumerator Lineup40Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            bool open = Lineup40Spot(sim, 40, 12, out Vec3 at, out double ux, out double uz);
            double yaw = Math.Atan2(-ux, -uz);
            g.Respawn(at, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, 0.02);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.02 };
            yield return Frames(20);
            var row = new List<Husk>();
            void Place(double dist)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    Husk h = row[i];
                    double w = (i - (row.Count - 1) * 0.5) * 3.5;
                    var p = new Vec3(at.x + ux * dist - uz * w, at.y + (h.kind == "drone" ? 2.0 : 0.05), at.z + uz * dist + ux * w);
                    h.position = h.previous = p;
                    h.velocity = default;
                    h.yaw = JsMath.Atan2(-(at.x - p.x), -(at.z - p.z)); // facing the player
                }
            }
            foreach (string kind in Lineup40Kinds)
            {
                Husk h = g.husks.Spawn(kind == "tank" ? "husk" : kind, new Vec3(at.x + ux * 30, at.y + 0.05, at.z + uz * 30), at);
                if (h == null) continue;
                if (kind == "tank") h.tank29 = true;
                row.Add(h);
            }
            Place(30);
            Start(g, "lineup40", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            for (int k = 0; k < 10; k++) { God(g); Place(30); yield return null; }
            // the parts are made by the views as they first draw the row (round 4: counted before that, the line said 0): counted now, active ones only
            int parts = 0;
            foreach (Transform t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name.Contains(" part40 ")) parts++;
            string pc = $"open floor {open}, placed {row.Count}/7, models40 parts drawn {parts} (0: the row is off in this build; 15 with the row on: 1 husk + 1 tank (each body hides the other group) + 2 swarmer + 3 spitter + 3 charger + 3 warden + 2 drone; inactive objects are not found)";
            yield return Shot29(dir, "E40-lineup-30", st);
            // the silhouette sheet: the row's frame, then the same view with the row moved away; changed pixels black, the rest white
            yield return new WaitForEndOfFrame();
            Texture2D withRow = ScreenCapture.CaptureScreenshotAsTexture();
            foreach (Husk h in row) { h.position = h.previous = new Vec3(at.x, at.y - 400, at.z); }
            yield return Frames(3);
            yield return new WaitForEndOfFrame();
            Texture2D without = ScreenCapture.CaptureScreenshotAsTexture();
            int black = 0;
            if (withRow != null && without != null && withRow.width == without.width && withRow.height == without.height)
            {
                Color32[] a = withRow.GetPixels32(), b = without.GetPixels32();
                var o = new Color32[a.Length];
                for (int i = 0; i < a.Length; i++)
                {
                    int d = Math.Abs(a[i].r - b[i].r) + Math.Abs(a[i].g - b[i].g) + Math.Abs(a[i].b - b[i].b);
                    bool on = d > 24; // a changed pixel (3 x 8 levels: above the frame-to-frame noise of the dusk sky)
                    if (on) black++;
                    o[i] = on ? new Color32(0, 0, 0, 255) : new Color32(255, 255, 255, 255);
                }
                var sheet = new Texture2D(withRow.width, withRow.height, TextureFormat.RGBA32, false);
                sheet.SetPixels32(o);
                sheet.Apply();
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N29-E40-silhouette-30.png"), sheet.EncodeToPNG());
                UnityEngine.Object.Destroy(sheet);
            }
            if (withRow != null) UnityEngine.Object.Destroy(withRow);
            if (without != null) UnityEngine.Object.Destroy(without);
            Place(12);
            for (int k = 0; k < 10; k++) { God(g); yield return null; }
            yield return Shot29(dir, "E40-lineup-12", st);
            // every kind in its wind-up (the views draw the state; the frozen sim never ends it)
            foreach (Husk h in row)
            {
                if (h is Charger c) { c.state = "tell"; c.stateSteps = 100000; c.chargeYaw = h.yaw; }
                else if (h is Warden w) { w.state = "tell"; w.stateSteps = 100000; }
                else if (h is Drone d) d.chargeSteps = 100000;
                else if (h is Spitter sp) { sp.state = "tell"; sp.stateSteps = 60; sp.tellSteps = 120; }
                else if (h is Swarmer sw) { sw.state = "crouch"; sw.stateSteps = 100000; }
                else h.windupSteps = 100000;
            }
            for (int k = 0; k < 20; k++) { God(g); yield return null; }
            yield return Shot29(dir, "E40-lineup-tells", st);
            yield return Stop();
            g.husks.KillAll();
            res[0] = (row.Count == 7 ? "GREEN" : "RED") + $": lineup40 | {pc}, silhouette pixels {black}";
        }
    }
}
