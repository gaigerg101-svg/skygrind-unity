// NIGHT43 builder A1: the Player scenario `burrow43` (models43's and burrow43's proof: NIGHT43.md card A1). Dispatched by the lead's FeelReel.N43.cs.
// Not run by its author (no Player in a builder worktree). Boot any map in SURVIVAL (the survival map is fine: open floor). Both rows are forced
// for the reel only (Models43.Force; the BURROWER's rules handed to the pools directly), whatever the build's config says; the line-up's OFF frames
// are NIGHT42's picture (models40 as the config has it). The player is put on open floor (lineup40's grid search), God, input by forced frames
// only (L-046). Frames (shot-N29-<name>.png):
//   A1-lineup-off-12 / A1-lineup-off-30   husk, tank, swarmer, spitter, charger, warden, drone in a row at 12 m / 30 m, frozen, models43 OFF (before)
//   A1-lineup-on-12  / A1-lineup-on-30    the same row, models43 ON (after): a visor per kind (slit, double slit, three lenses, round lens,
//                                         chevron, plate, a ring of lenses), smooth bodies, armour; the visor the brightest thing on each body
//   A1-silhouette-off-30 / -on-30         the row's changed pixels black, the rest white (the TF2 test: name each kind from its outline); round 2:
//                                         the line prints the GREYSCALE read of each (the row's pixels' L* against what is behind them: the mean
//                                         |dL*| and the share >= 15; DESIGN-RULES E-3). n43-r2 (by a script on its frames): 1-6 L* apart OFF and ON
//   A1-lineup-on-tells                    the ON row at 12 m in every kind's wind-up (the visors hotter, the drone's yellow emitter lit)
//   A1-burning-12                         the ON row at 12 m, the husk / swarmer / charger burning fresh, the spitter half burnt (flame tongues)
//   A1-burrow-trail                      a BURROWER hunting under the floor toward the player (the mound trail, the dust), the camera down 25 deg
//   A1-burrow-tell-0 / A1-burrow-tell-6   its tell at +0.05 s and +0.6 s (the player moved 4 m back to watch: the red-orange rings, the inner closing)
//   A1-burrow-emerge                      +0.06 s after the emergence (the body out, the mouth lit, the cyan window ring)
//   A1-burrow-counter                     a hit in the window (+0.15 s): it reels back, the mouth gutters, the fangs pulse
//   A1-burrow-dive                        it sinking back
// Line: GREEN when the row was placed (7/7) and the burrower told, emerged and was countered (its event counts printed).
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
        static partial void N43Burrow(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            // round 3: `-skygrindA1Ring` on the Player's command line runs THE RING's frames instead (boot level 3, n43-kiln, through reel.sh +
            // -skygrindReelOnly so the worm's room is the level's own arena: L-062)
            bool ring = false;
            foreach (string a in Environment.GetCommandLineArgs()) if (a == "-skygrindA1Ring") ring = true;
            body = ring ? Ring43Scenario(g, dir, res, offs, pre) : Burrow43Scenario(g, dir, res, offs, pre);
        }

        /// <summary>
        /// Round 3: THE RING (WyrmRing43View, look43 forced by the reel): the worm's fight started as Campaign24 enters its boss room (Wyrm24Begin),
        /// the player (God) at the room's spawn looking at its middle. Frames: A1-ring-off-entrance / A1-ring-on-entrance (the room before / after
        /// from the entrance: the crucible landmark, the core, the hot field), A1-ring-on-up (the crucible on its chains, the pour), then after
        /// ~6 s of the fight A1-ring-off-mid / A1-ring-on-mid (the worm in its dressed room). The line: the arena file, the pieces built.
        /// </summary>
        static IEnumerator Ring43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            offs.Add(() => { WyrmRing43View.Force = null; g.forcedMove = null; g.forcedCombat = IdleCombat; });
            g.forcedCombat = IdleCombat;
            God(g);
            if (!sim.Wyrm24Begin())
            {
                res[0] = "RED: burrow43 ring | Wyrm24Begin failed (the worm's files were not found, or no BossKit)";
                yield break;
            }
            Vec3 spawn = sim.Wyrm24Spawn;
            double yawDeg = sim.wyrm24Arena.spawnYawDeg, yaw = yawDeg * Math.PI / 180;
            g.Respawn(spawn, yawDeg);
            void Look(double pitch)
            {
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            }
            Look(0.05);
            WyrmRing43View.Force = false;
            for (float t = 0; t < 1.0f; t += Time.unscaledDeltaTime) { God(g); yield return null; }
            Start(g, "burrow43", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            yield return Shot29(dir, "A1-ring-off-entrance", st);
            WyrmRing43View.Force = true;
            for (int k = 0; k < 6; k++) { God(g); yield return null; }
            yield return Shot29(dir, "A1-ring-on-entrance", st);
            Look(0.55); // the crucible at 21 m, 27.5 m away: ~35 deg up
            for (int k = 0; k < 6; k++) { God(g); yield return null; }
            yield return Shot29(dir, "A1-ring-on-up", st);
            Look(-0.05);
            for (float t = 0; t < 6f; t += Time.unscaledDeltaTime) { God(g); yield return null; }
            yield return Shot29(dir, "A1-ring-on-mid", st);
            WyrmRing43View.Force = false;
            for (int k = 0; k < 4; k++) { God(g); yield return null; }
            yield return Shot29(dir, "A1-ring-off-mid", st);
            WyrmRing43View.Force = true;
            for (int k = 0; k < 2; k++) { God(g); yield return null; }
            yield return Stop();
            WyrmRing43View v = g.Ring43View;
            bool ok = v != null && v.Plates > 0 && sim.wyrm24 != null;
            res[0] = (ok ? "GREEN" : "RED") + $": burrow43 ring | arena {sim.wyrm39ArenaBuilt ?? "(the config's)"}, worm alive {(sim.wyrm24 != null && sim.wyrm24.Alive)}; built: plates {v?.Plates}, hot plates {v?.HotPlates}, hot zones {v?.HotZones}, perch lips {v?.Lips}, ride strips {v?.RideStrips}; K1's wyrm43 extras: the kiln mouth landmark {v?.Landmark}, slag plates {v?.Slabs}";
        }

        /// <summary>
        /// Round 2 (n43-r2's frames: lineup40's search, casting down from 60 m, stood the row and the burrower on a surface high over the map
        /// (the row floating against the sky, the trail floating, the 4 m step back off its edge into a fall): a spot on the map's own floor at
        /// the player spawn's floor height (+/- 0.25 m), with `ahead` m clear in front, `behind` m behind and `side` m either side, all on that
        /// floor; the nearest to the spawn first. False: none (the caller falls back to lineup40's).
        /// </summary>
        static bool Burrow43Spot(SurvivalSim sim, double ahead, double behind, double side, out Vec3 at, out double ux, out double uz)
        {
            at = sim.map.playerSpawn;
            ux = 0;
            uz = -1;
            Vec3 sp = sim.map.playerSpawn;
            if (!sim.world.RayIntersect(new Vec3(sp.x, sp.y + 1.5, sp.z), new Vec3(0, -1, 0), out RayHit h0)) return false;
            double floorY = sp.y + 1.5 - h0.distance;
            bool Floor(double x, double z)
            {
                var c = new Vec3(x, floorY, z);
                var q = new Vec3(c.x, c.y + 1.0, c.z);
                if (WorldCollider.CapsuleIntersect(sim.world, new Capsule { start = q, end = new Vec3(q.x, q.y + 1.5, q.z), radius = 0.6 }, out _, out _)) return false;
                if (!sim.world.RayIntersect(new Vec3(x, floorY + 0.5, z), new Vec3(0, -1, 0), out RayHit hit)) return false;
                return hit.distance > 0.25 && hit.distance < 0.75;
            }
            for (int ring = 0; ring <= 12; ring++)
                for (int gx = -ring; gx <= ring; gx++)
                    for (int gz = -ring; gz <= ring; gz++)
                    {
                        if (Math.Max(Math.Abs(gx), Math.Abs(gz)) != ring) continue;
                        double px = sp.x + gx * 4, pz = sp.z + gz * 4;
                        if (!Floor(px, pz)) continue;
                        for (int k = 0; k < 8; k++)
                        {
                            double a = k * Math.PI / 4, dx = Math.Sin(a), dz = -Math.Cos(a);
                            bool ok = true;
                            for (double d = -behind; d <= ahead && ok; d += 2)
                                for (double w = -side; w <= side && ok; w += 3)
                                    ok = Floor(px + dx * d - dz * w, pz + dz * d + dx * w);
                            if (!ok) continue;
                            at = new Vec3(px, floorY + 0.05, pz);
                            ux = dx;
                            uz = dz;
                            return true;
                        }
                    }
            return false;
        }

        /// <summary>CIE L* (0..100) of an sRGB pixel (Rec.709 luminance).</summary>
        static double Lstar43(Color32 c)
        {
            double Lin(byte v)
            {
                double s = v / 255.0;
                return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
            }
            double y = 0.2126 * Lin(c.r) + 0.7152 * Lin(c.g) + 0.0722 * Lin(c.b);
            return y <= 0.008856 ? y * 903.3 : 116 * Math.Pow(y, 1.0 / 3) - 16;
        }

        static IEnumerator Burrow43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            Burrow43Params rulesWas = g.husks.burrow43;
            HuskParams bodyWas = g.husks.burrowBody43;
            offs.Add(() =>
            {
                g.forcedMove = null;
                g.forcedCombat = IdleCombat;
                g.husks.frozen = false;
                Models43.Force = null;
                g.husks.burrow43 = rulesWas;
                g.husks.burrowBody43 = bodyWas;
            });
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            // round 2: the map's own floor (Burrow43Spot), lineup40's search only when it finds none
            string spotWas = "floor";
            bool open = Burrow43Spot(sim, 32, 0, 11, out Vec3 at, out double ux, out double uz);
            if (!open)
            {
                spotWas = "lineup40";
                open = Lineup40Spot(sim, 40, 12, out at, out ux, out uz);
            }
            double yaw = Math.Atan2(-ux, -uz);
            g.Respawn(at, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, 0.02);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.02 };
            Models43.Force = false;
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
                    h.yaw = JsMath.Atan2(-(at.x - p.x), -(at.z - p.z));
                }
            }
            foreach (string kind in Lineup40Kinds)
            {
                Husk h = g.husks.Spawn(kind == "tank" ? "husk" : kind, new Vec3(at.x + ux * 30, at.y + 0.05, at.z + uz * 30), at);
                if (h == null) continue;
                if (kind == "tank") h.tank29 = true;
                row.Add(h);
            }
            Start(g, "burrow43", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            int black = 0;
            string grey = "";
            foreach (bool on in new[] { false, true })
            {
                Models43.Force = on;
                string tag = on ? "on" : "off";
                Place(12);
                for (int k = 0; k < 10; k++) { God(g); Place(12); yield return null; }
                yield return Shot29(dir, "A1-lineup-" + tag + "-12", st);
                Place(30);
                for (int k = 0; k < 10; k++) { God(g); Place(30); yield return null; }
                yield return Shot29(dir, "A1-lineup-" + tag + "-30", st);
                // the silhouette sheet (lineup40's: the row, then the same view with the row moved away) and, round 2, the GREYSCALE read
                // (DESIGN-RULES E-3): each changed pixel's CIE L* with the row against the same pixel without it (what is behind it)
                yield return new WaitForEndOfFrame();
                Texture2D withRow = ScreenCapture.CaptureScreenshotAsTexture();
                foreach (Husk h in row) h.position = h.previous = new Vec3(at.x, at.y - 400, at.z);
                yield return Frames(3);
                yield return new WaitForEndOfFrame();
                Texture2D without = ScreenCapture.CaptureScreenshotAsTexture();
                if (withRow != null && without != null && withRow.width == without.width && withRow.height == without.height)
                {
                    Color32[] a = withRow.GetPixels32(), bpx = without.GetPixels32();
                    var o = new Color32[a.Length];
                    int n = 0, strong = 0;
                    double sum = 0;
                    for (int i = 0; i < a.Length; i++)
                    {
                        int d = Math.Abs(a[i].r - bpx[i].r) + Math.Abs(a[i].g - bpx[i].g) + Math.Abs(a[i].b - bpx[i].b);
                        bool px = d > 24;
                        o[i] = px ? new Color32(0, 0, 0, 255) : new Color32(255, 255, 255, 255);
                        if (!px) continue;
                        double dl = Math.Abs(Lstar43(a[i]) - Lstar43(bpx[i]));
                        n++;
                        sum += dl;
                        if (dl >= 15) strong++;
                    }
                    if (on) black = n;
                    grey += $"{tag}: |dL*| mean {(n > 0 ? sum / n : 0):0.0}, >= 15 on {(n > 0 ? 100.0 * strong / n : 0):0}% of {n} px; ";
                    var sheet = new Texture2D(withRow.width, withRow.height, TextureFormat.RGBA32, false);
                    sheet.SetPixels32(o);
                    sheet.Apply();
                    Directory.CreateDirectory(dir);
                    File.WriteAllBytes(Path.Combine(dir, "shot-N29-A1-silhouette-" + tag + "-30.png"), sheet.EncodeToPNG());
                    UnityEngine.Object.Destroy(sheet);
                }
                if (withRow != null) UnityEngine.Object.Destroy(withRow);
                if (without != null) UnityEngine.Object.Destroy(without);
            }
            Place(12);
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
            yield return Shot29(dir, "A1-lineup-on-tells", st);
            // round 2: the burning look (W1's torch43 burn; forced here through EnemyLook.Burning43): the husk, the swarmer and the charger fresh
            // (1.0), the spitter half burnt (0.5), the rest not burning; the tells stay on
            offs.Add(() => EnemyLook.Burning43 = null);
            EnemyLook.Burning43 = e => e.kind == "husk" || e.kind == "swarmer" || e.kind == "charger" ? 1f : e.kind == "spitter" ? 0.5f : 0f;
            for (int k = 0; k < 15; k++) { God(g); yield return null; }
            yield return Shot29(dir, "A1-burning-12", st);
            EnemyLook.Burning43 = null;
            g.husks.KillAll();

            // the BURROWER: its rules handed to the pools for the reel, one spawned 7 m ahead; the player stands (God) and looks down at it
            g.husks.burrow43 = Burrow43Params.From(g.cfg);
            g.husks.burrowBody43 = g.husks.burrow43.Body(g.cfg, g.husks.p.killPlaneY);
            g.husks.frozen = false;
            int tells = 0, emerges = 0, counters = 0;
            Action u1 = g.events.On<Burrow43Event>(N43Events.BurrowTell, e => tells++);
            Action u2 = g.events.On<Burrow43Event>(N43Events.BurrowEmerge, e => emerges++);
            Action u3 = g.events.On<Burrow43Event>(N43Events.BurrowCounter, e => counters++);
            offs.Add(u1);
            offs.Add(u2);
            offs.Add(u3);
            // round 2: its own spot on the floor: 10 m clear ahead (it hunts in from 7 m), 6 m behind (the 4 m step back to watch the tell)
            if (Burrow43Spot(sim, 10, 6, 3, out Vec3 bat, out double bux, out double buz))
            {
                at = bat;
                ux = bux;
                uz = buz;
                yaw = Math.Atan2(-ux, -uz);
            }
            g.Respawn(at, yaw * 180 / Math.PI);
            double down = -0.44; // 25 deg down: the floor ahead in view
            g.input.SetLook(yaw, down);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = down };
            yield return Frames(10);
            var bw = g.husks.Spawn("burrower", new Vec3(at.x + ux * 7, at.y - 0.05, at.z + uz * 7), at) as Burrower43;
            if (bw == null)
            {
                yield return Stop();
                res[0] = $"RED: burrow43 | no burrower spawned (open floor {open}, placed {row.Count}/7)";
                yield break;
            }
            // the trail: 0.35 s into the hunt
            for (int k = 0; k < 400 && bw.state != Burrower43.Hunt; k++) { God(g); yield return null; }
            for (float t = 0; t < 0.35f; t += Time.unscaledDeltaTime) { God(g); yield return null; }
            yield return Shot29(dir, "A1-burrow-trail", st);
            for (int k = 0; k < 600 && tells == 0; k++) { God(g); yield return null; }
            // watch the tell from 4 m back (the spot is locked: it stays where the player was)
            Vector3 back = new Vector3((float)-ux, 0, (float)-uz) * 4;
            sim.mover.Reset(sim.mover.position.x + back.x, sim.mover.position.y, sim.mover.position.z + back.z);
            for (float t = 0; t < 0.05f; t += Time.unscaledDeltaTime) { God(g); yield return null; }
            yield return Shot29(dir, "A1-burrow-tell-0", st);
            for (float t = 0; t < 0.5f && emerges == 0; t += Time.unscaledDeltaTime) { God(g); yield return null; }
            yield return Shot29(dir, "A1-burrow-tell-6", st);
            for (int k = 0; k < 400 && emerges == 0; k++) { God(g); yield return null; }
            for (float t = 0; t < 0.06f; t += Time.unscaledDeltaTime) { God(g); yield return null; }
            g.input.SetLook(yaw, -0.05);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = -0.05 };
            yield return Shot29(dir, "A1-burrow-emerge", st);
            if (bw.alive && bw.state == Burrower43.Emerge && bw.windowSteps > 0) bw.Damage(64, 0.08); // a THUNDERCLAP volley's worth, in the window
            for (float t = 0; t < 0.15f; t += Time.unscaledDeltaTime) { God(g); yield return null; }
            yield return Shot29(dir, "A1-burrow-counter", st);
            for (int k = 0; k < 600 && bw.alive && bw.state != Burrower43.Dive; k++) { God(g); yield return null; }
            for (float t = 0; t < 0.15f; t += Time.unscaledDeltaTime) { God(g); yield return null; }
            yield return Shot29(dir, "A1-burrow-dive", st);
            yield return Stop();
            g.husks.KillAll();
            bool ok = row.Count == 7 && tells >= 1 && emerges >= 1 && counters >= 1;
            res[0] = (ok ? "GREEN" : "RED") + $": burrow43 | spot {spotWas} (open {open}), placed {row.Count}/7, silhouette pixels {black}; greyscale at 30 m (the row vs what is behind it): {grey}burrower tells {tells}, emergences {emerges}, counters {counters}, hits on the player {bw.hitsOnPlayer} (want 0: watched from 4 m), hp left {bw.hp:0}/{bw.maxHp:0}";
        }
    }
}
