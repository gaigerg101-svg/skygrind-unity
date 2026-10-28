// NIGHT28 (builder A2): the reel scenario `rink28`: THE GLACIER's rink SEEN as a rink, with design.n28.rink forced ON for the run (SurvivalSim.rink28Override). Three frame sets, 4 sampled frames each:
//   set 1  the lane start (s 4) looking along the bottom straight: the ice, the bank on one side, the island and its door on the other, the roof / skylight;
//   set 2  the east cap's bank looking round the curve;
//   set 3  the island top looking across a door.
// Per frame it reads the screen: the luminance standard deviation over the FLOOR region (centre columns, the band above the HUD: the ice must not be a flat colour, >= 0.03) and the number of occupied
// luminance bands (8 bins of 0.05, 2 % of the samples or more). Then the player skates the lane at 12 m/s and the scenario reads the ice shader's own drivers twice, 6 frames apart (the globals
// _Rink28Scroll / _Rink28Dir / _Rink28Strength that SkateFx27 writes and SKYGRIND/RinkIce28 reads): the streak phase at the world point under the left, centre and right thirds of the floor region (each
// must be inside the rink and change by about the same amount: the streaks scroll over the WHOLE floor, not a patch). And a collision ray from the camera through each of those three points: the first
// thing hit must be floor-grade (nothing opaque between the camera and the floor). The player is parked by Respawn for each set; god mode; bounded by game time and a wall guard.
// Register: PhaseJ in FeelReel.N27.cs gets "rink28" and one partial N27Rink28Body. Not run by its author (no Editor in the worktree): the lead runs it as scenario id `rink28`.
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
        static partial void N27Rink28Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Rink28Scenario(g, dir, res, offs, pre);

        /// <summary>The floor region's luminance standard deviation (0..1) and the occupied bands, over the central columns and the band above the HUD, from the screen; call after WaitForEndOfFrame.</summary>
        static bool Rink28FloorStats(out double std, out int bands)
        {
            std = -1; bands = 0;
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) return false;
                int w = t.width, h = t.height, x0 = (int)(w * 0.3), x1 = (int)(w * 0.7), y0 = (int)(h * 0.16), y1 = (int)(h * 0.45); // GetPixel's y runs from the bottom
                double sum = 0, sum2 = 0;
                int n = 0;
                var bins = new int[8];
                for (int y = y0; y < y1; y += 6)
                    for (int x = x0; x < x1; x += 6)
                    {
                        Color c = t.GetPixel(x, y);
                        double l = 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                        sum += l; sum2 += l * l; n++;
                        bins[Math.Min(7, (int)(l / 0.05))]++;
                    }
                if (n == 0) return false;
                double mean = sum / n;
                std = Math.Sqrt(Math.Max(0, sum2 / n - mean * mean));
                foreach (int b in bins) if (b >= n * 0.02) bands++;
                return true;
            }
            catch (Exception) { return false; }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        /// <summary>The world (sim) point where the camera ray through viewport (vx, vy) meets the first collision surface, and whether that surface is floor-grade; false when nothing is hit.</summary>
        static bool Rink28FloorPoint(SurvivalGame g, float vx, float vy, out Vec3 point, out bool floorGrade, out double dist)
        {
            point = default; floorGrade = false; dist = 0;
            Ray r = g.cam.ViewportPointToRay(new Vector3(vx, vy, 0));
            var o = new Vec3(r.origin.x, r.origin.y, -r.origin.z);
            var d = new Vec3(r.direction.x, r.direction.y, -r.direction.z);
            if (!g.sim.world.RayIntersect(o, d, out RayHit hit)) return false;
            dist = hit.distance;
            point = new Vec3(o.x + d.x * hit.distance, o.y + d.y * hit.distance, o.z + d.z * hit.distance);
            floorGrade = hit.normal.y >= 0.7;
            return true;
        }

        public static IEnumerator Rink28Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            float wall0 = Time.realtimeSinceStartup;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Glacier27On) { res[0] = "RED: design.n27.boss2 is off (the plan's boss 'glacier' is NIGHT26's glacier) | " + pc; yield break; }
            bool prevOverride = sim.rink28Override ?? false;
            bool hadOverride = sim.rink28Override.HasValue;
            offs.Add(() => { sim.rink28Override = hadOverride ? (bool?)prevOverride : null; });
            offs.Add(() => Quiet(g));
            sim.rink28Override = true;
            g.husks.frozen = true;
            if (!sim.BossRoomBegin("glacier", out Vec3 spawn, out double yawDeg) || sim.rink28 == null) { res[0] = "RED: BossRoomBegin(glacier) did not build the rink (rink28Override, the course file or no BossKit) | " + pc; yield break; }
            Rink28Arena a = sim.rink28;
            RinkView28 view = g.rinkView28;
            Quiet(g);
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            yield return Frames(20);
            Start(g, "rink28", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // the three parking spots: (position, yaw, pitch, label)
            double L = a.p.straight, cap = a.math.CapLength;
            a.math.PointAt(L + cap / 2, a.LaneOutLat + 2.0, out double bx, out double bz, out double btx, out double btz);
            var spots = new List<(Vec3 at, double yaw, double pitch, string name)>
            {
                (spawn, yawDeg * Math.PI / 180, -0.12, "lane start, looking along the straight"),
                (new Vec3(bx, a.HeightAtLateral(a.LaneOutLat + 2.0), bz), Math.Atan2(-btx, -btz), -0.25, "cap bank, looking round the curve"),
                (new Vec3(a.p.cx + 8, a.islandTopY, a.p.cz), Math.PI / 2, -0.15, "island top, looking across a door"),
            };
            var detail = new List<string>();
            double minStd = 1e9;
            int minBands = 99, setIdx = 0, frames = 0;
            bool readFail = false;
            foreach (var sp in spots)
            {
                setIdx++;
                g.Respawn(sp.at, sp.yaw * 180 / Math.PI);
                g.input.SetLook(sp.yaw, sp.pitch);
                for (int f = 0; f < 20 && Time.realtimeSinceStartup - wall0 < UnityTuning.ReelViewWallGuard27 * 3; f++) { God(g); yield return null; }
                FeelProbe.Event("set" + setIdx);
                var stds = new List<string>();
                for (int k = 0; k < 4 && Time.realtimeSinceStartup - wall0 < UnityTuning.ReelViewWallGuard27 * 3; k++)
                {
                    for (int f = 0; f < 8; f++) { God(g); g.input.SetLook(sp.yaw, sp.pitch); yield return null; }
                    yield return new WaitForEndOfFrame();
                    if (Rink28FloorStats(out double std, out int bands))
                    {
                        minStd = Math.Min(minStd, std); minBands = Math.Min(minBands, bands);
                        stds.Add(std.ToString("0.000", ci) + "/" + bands);
                    }
                    else readFail = true;
                    frames++;
                }
                detail.Add($"set {setIdx} ({sp.name}): floor luminance std/bands per frame {string.Join(" ", stds)}");
            }

            // the streaks over the whole floor: skate the lane at 12 m/s and read the shader's drivers twice, 6 frames apart
            Vec3 startAt = spawn;
            a.math.PointAt(a.p.playerStartS, 0, out _, out _, out double ltx, out double ltz);
            double laneYaw = Math.Atan2(-ltx, -ltz);
            g.Respawn(startAt, laneYaw * 180 / Math.PI);
            g.input.SetLook(laneYaw, -0.3);
            Mover m = sim.mover;
            SkateFx27 fx = SkateFx27.Active;
            double strength = 0;
            bool onIce = false;
            for (int f = 0; f < 30; f++) { God(g); m.velocity = new Vec3(ltx * 12, m.velocity.y, ltz * 12); strength = fx != null ? fx.strength : 0; onIce = fx != null && fx.onIce; yield return null; }
            float scrollA = Shader.GetGlobalFloat("_Rink28Scroll");
            Vector4 dirV = Shader.GetGlobalVector("_Rink28Dir");
            float lines = (float)g.cfg.Num("design.n28.rink.streakLinesPerM");
            var ptsA = new List<Vec3>();
            var thirds = new[] { 0.35f, 0.5f, 0.65f };
            var clear = new List<string>();
            bool allFloor = true, allInside = true;
            foreach (float vx in thirds)
            {
                if (!Rink28FloorPoint(g, vx, 0.3f, out Vec3 p, out bool fg, out double dd)) { allFloor = false; clear.Add("miss"); ptsA.Add(default); continue; }
                ptsA.Add(p);
                if (!fg) allFloor = false;
                if (a.RegionAt(p.x, p.z) == Rink28Region.Outside) allInside = false;
                clear.Add($"{dd.ToString("0.0", ci)} m {(fg ? "floor" : "NOT FLOOR")}");
            }
            for (int f = 0; f < 6; f++) { God(g); m.velocity = new Vec3(ltx * 12, m.velocity.y, ltz * 12); yield return null; }
            float scrollB = Shader.GetGlobalFloat("_Rink28Scroll");
            double speedNow = m.Speed;
            var phase = new List<double>();
            double dscroll = scrollB - scrollA;
            if (dscroll < -500) dscroll += 1000; // the global wraps at 1000 m
            var ph = new List<string>();
            for (int i = 0; i < ptsA.Count; i++)
            {
                // phase of the dashes at the world point (the shader's `along`): the change between the frames at the SAME point is the scroll's share, whatever the point
                double along0 = (ptsA[i].x * dirV.x + ptsA[i].z * dirV.z) * lines * 0.5 - scrollA * lines * 0.5;
                double along1 = (ptsA[i].x * dirV.x + ptsA[i].z * dirV.z) * lines * 0.5 - scrollB * lines * 0.5;
                double change = Math.Abs(along1 - along0) % 1.0;
                change = Math.Min(change, 1 - change);
                phase.Add(change);
                ph.Add(change.ToString("0.00", ci));
            }
            yield return Stop();
            double phMin = Math.Min(phase[0], Math.Min(phase[1], phase[2])), phMax = Math.Max(phase[0], Math.Max(phase[1], phase[2]));
            bool flatFail = minStd < 0.03;
            bool streakOk = onIce && strength > 0.3 && phMin > 0.02 && phMax - phMin < 0.05 && allInside;
            bool ok = !readFail && !flatFail && minBands >= 3 && streakOk && allFloor && view != null && view.Shown;
            string why = readFail ? "RED: a screen read failed" : flatFail ? "RED: the floor is a FLAT colour (luminance std " + minStd.ToString("0.000", ci) + " < 0.030)" : minBands < 3 ? "RED: fewer than 3 luminance bands across the floor" :
                !streakOk ? "RED: the streaks do not cover the whole floor (onIce " + onIce + ", strength " + strength.ToString("0.00", ci) + ", phase change per third " + string.Join("/", ph) + ", points inside the rink " + allInside + ")" :
                !allFloor ? "RED: something that is not floor sits between the camera and the floor" : "RED: no RinkView28 shown";
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: rink28 {a.tris.Count} triangles, view {(view != null && view.Shown ? "shown" : "NOT shown")} ({(view != null ? view.iceTris + " ice / " + view.wallTris + " wall triangles, " + view.stripQuads + " strip quads, " + view.postCount + " door posts, " + view.roofQuads + " roof quads" : "")}); "
                + string.Join("; ", detail) + $"; min floor luminance std {minStd.ToString("0.000", ci)} (>= 0.030), min bands {minBands}; skating {speedNow.ToString("0.0", ci)} m/s on ice {onIce}, streak strength {strength.ToString("0.00", ci)}, streak phase change over 6 frames at the floor's left / centre / right thirds {string.Join(" / ", ph)} (scroll {dscroll.ToString("0.00", ci)} m); camera-to-floor rays: {string.Join(", ", clear)}"
                + ": " + (ok ? "PASS" : why);
        }
    }
}
