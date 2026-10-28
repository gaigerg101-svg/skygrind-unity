// NIGHT26 builder V: the Player reel scenarios of THE KILN WYRM's glow, aura and animation (Game/WyrmVisual26.cs; row design.n26.wormVisual):
//   wormcoil    the idle pulse (min / max), the coil's pulse rate at ~3 s, ~1.5 s and ~0.5 s before the release, the peak glow at the release, the aura
//               (halo + embers) on in the coil and off at idle, then the FRAME COST of the effects (600 frames on vs 600 off, twice, 24 husks alive)
//   wormattack  the telegraph glow by kind (dive, lunge, and spit / roar from phase 2), the trail front running head to tail, the impact flash, the damage
//               flash on a chosen piece (white-hot, then gone), the glow dimming with the HP, the death: flare, then burn out head to tail
// Each prints readings, not adjectives (CLAUDE.md rule 9; feel is done when measured). RED: the row off / no parts asset (WyrmView.visual is null), no WyrmView,
// the fight not started, a beat not reached. Every line starts with the scenario's preconditions. The sim is the real one; the player is in god mode. Not run
// by its author: the lead runs it in a Player (the Editor cannot show bloom as the Player does). The frame cost is the whole frame and the Render CPU time:
// the GPU cost is the lead's -skygrindBench 60 with the row on vs off (--off26 turns the row off).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N26WormcoilBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = WormCoilScenario(g, dir, res, pre);

        static partial void N26WormattackBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = WormAttackScenario(g, dir, res, pre);

        static string F(double v, string f = "0.00") => v.ToString(f, CultureInfo.InvariantCulture);

        /// <summary>The checks both scenarios share: the row on, the view there, the fight started. Returns the worm, or null with res[0] set.</summary>
        static Wyrm WormStart(SurvivalGame g, string[] res, out WyrmVisual26 vis, out string pc)
        {
            vis = null;
            pc = Preconditions(g, g.sim.wyrm24Arena == null ? default : g.sim.Wyrm24Spawn);
            if (!g.sim.n24.wyrm)
            {
                res[0] = pc + " | RED: design.n24.wyrm is off (no worm)";
                return null;
            }
            if (g.wyrmView == null)
            {
                res[0] = pc + " | RED: no WyrmView (SurvivalGame.N24InitViewsHook does not call Wyrm24InitView)";
                return null;
            }
            vis = g.wyrmView.visual;
            if (vis == null)
            {
                res[0] = pc + " | RED: no WyrmVisual26: design.n26.wormVisual is off, or Resources/WormBossParts is missing / not ready (run SKYGRIND > Worm Boss > Build Prefab)";
                return null;
            }
            if (!g.sim.Wyrm24Begin())
            {
                res[0] = pc + " | RED: Wyrm24Begin failed (the row is on: the design files were not found, or no BossKit yet)";
                return null;
            }
            pc = Preconditions(g, g.sim.Wyrm24Spawn);
            g.husks.frozen = true;
            g.Respawn(g.sim.Wyrm24Spawn, 0);
            g.input.SetLook(0, 0.28);
            God(g);
            return g.sim.wyrm24;
        }

                /// <summary>God mode, and the camera on the worm (the lead's frames must show it): at the head, or at the middle of the visible pieces when `middle` (the coil, the finisher).
        /// yaw 0 looks toward web -z (the NIGHT25 AimAtWorm convention). The eye is the feet + 1.6 m.</summary>
        static void WormAim(SurvivalGame g, Wyrm w, bool middle)
        {
            Vec3 f = g.Feet;
            double tx = 0, ty = 0, tz = 0;
            int cnt = 0;
            for (int i = 0; i < w.PieceCount; i++)
            {
                if (w.buried[i]) continue;
                if (!middle)
                {
                    tx = w.boxes[i].c.x; ty = w.boxes[i].c.y; tz = w.boxes[i].c.z;
                    cnt = 1;
                    break;
                }
                tx += w.boxes[i].c.x; ty += w.boxes[i].c.y; tz += w.boxes[i].c.z;
                cnt++;
            }
            if (cnt == 0) return; // nothing above the floor: keep the last look
            double dx = tx / cnt - f.x, dy = ty / cnt - (f.y + 1.6), dz = tz / cnt - f.z;
            g.input.SetLook(Math.Atan2(-dx, -dz), Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)));
        }

        static bool wormMiddle;

        static void WormGod(SurvivalGame g, Wyrm w)
        {
            God(g);
            WormAim(g, w, wormMiddle);
        }

        // Wall-clock guards: every wait in these scenarios is bounded by game frames AND by real time (the n26-9 Player hung 19 minutes in wormattack: a frame bound is not a
        // time bound when frames are slow or the game is paused). WormPhase() restarts the per-phase clock (WormPhaseSec); WormWall() is false past it or past the whole
        // scenario's (WormTotalSec). A phase that runs out prints its failure and the scenario carries on with the next readings.
        const float WormPhaseSec = 90f, WormTotalSec = 420f;
        static float wormPhase0, wormTotal0;

        static void WormBegin() { wormTotal0 = wormPhase0 = Time.realtimeSinceStartup; }

        static void WormPhase() { wormPhase0 = Time.realtimeSinceStartup; }

        static bool WormWall() => Time.realtimeSinceStartup - wormPhase0 < WormPhaseSec && Time.realtimeSinceStartup - wormTotal0 < WormTotalSec;

        static float MaxGlow(WyrmVisual26 v, int from, int to)
        {
            float m = 0f;
            for (int i = from; i <= to && i < v.Fx.Length; i++) m = Mathf.Max(m, v.Fx[i].glow);
            return m;
        }

        static float MinGlow(WyrmVisual26 v, int from, int to)
        {
            float m = 99f;
            for (int i = from; i <= to && i < v.Fx.Length; i++) m = Mathf.Min(m, v.Fx[i].glow);
            return m;
        }


        // --- the pixel proof: the effects' glow as a number read from the screen --------------------------------------------------------------------------------------

        /// <summary>Mean red-orange energy and hot-pixel share inside the worm's screen rect, for frames drawn with the effects on and off (interleaved frame by frame: the
        /// same pose, the same sim, only WyrmVisual26.FxOn differs).</summary>
        sealed class PxAcc
        {
            public double eOn, eOff, hotOn, hotOff;
            public int nOn, nOff, rectOn, rectOff, f0 = -1, f1 = -1;

            public double MeanOn => nOn > 0 ? eOn / nOn : 0;
            public double MeanOff => nOff > 0 ? eOff / nOff : 0;

            public string Line(string name)
            {
                double on = MeanOn, off = MeanOff;
                return $"{name} (frames f{f0:0000}..f{f1:0000}, {nOn} on / {nOff} off, rect {(nOn > 0 ? rectOn / nOn : 0)} px): red-orange energy ON {F(on, "0.0000")} vs OFF {F(off, "0.0000")} (x{F(off > 1e-6 ? on / off : 0)}), hot-pixel share ON {F(nOn > 0 ? hotOn / nOn : 0, "0.0000")} vs OFF {F(nOff > 0 ? hotOff / nOff : 0, "0.0000")}";
            }
        }

        /// <summary>The worm's screen rect (pixels of the camera): the head's box (a sphere of 2.5 m) or all visible pieces; false when nothing is in front of the camera.</summary>
        static bool WormRect(SurvivalGame g, Wyrm w, bool whole, out int x0, out int y0, out int x1, out int y1)
        {
            Camera cam = g.cam;
            x0 = y0 = int.MaxValue;
            x1 = y1 = int.MinValue;
            if (cam == null) return false;
            for (int i = 0; i < w.PieceCount; i++)
            {
                if (w.buried[i] || (!whole && i != 0)) continue;
                Vector3 c = WebSpace.ToUnity(w.boxes[i].c);
                Vector3 sp = cam.WorldToScreenPoint(c);
                if (sp.z <= 0.5f) continue;
                float r = Mathf.Abs(cam.WorldToScreenPoint(c + cam.transform.up * 2.5f).y - sp.y);
                x0 = Mathf.Min(x0, (int)(sp.x - r));
                x1 = Mathf.Max(x1, (int)(sp.x + r));
                y0 = Mathf.Min(y0, (int)(sp.y - r));
                y1 = Mathf.Max(y1, (int)(sp.y + r));
            }
            if (x1 < x0) return false;
            x0 = Mathf.Max(0, x0);
            y0 = Mathf.Max(0, y0);
            x1 = Mathf.Min(cam.pixelWidth - 1, x1);
            y1 = Mathf.Min(cam.pixelHeight - 1, y1);
            return x1 > x0 + 4 && y1 > y0 + 4;
        }

        /// <summary>Call after a frame's WaitForEndOfFrame: read the screen and add the worm's rect to the arm (on / off) that frame was drawn with.</summary>
        static void WormMeasure(SurvivalGame g, Wyrm w, bool whole, bool on, PxAcc acc)
        {
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
            Color32[] px = tex.GetPixels32();
            int tw = tex.width, th = tex.height;
            UnityEngine.Object.Destroy(tex);
            if (!WormRect(g, w, whole, out int x0, out int y0, out int x1, out int y1)) return;
            float sx = tw / (float)g.cam.pixelWidth, sy = th / (float)g.cam.pixelHeight;
            int a = (int)(x0 * sx), b = (int)(x1 * sx), c = (int)(y0 * sy), d = (int)(y1 * sy);
            double e = 0, hot = 0;
            int n = 0;
            for (int y = c; y <= d && y < th; y += 2)
                for (int x = a; x <= b && x < tw; x += 2)
                {
                    Color32 p = px[y * tw + x];
                    e += Math.Max(0, p.r - p.g * 0.8 - p.b * 0.4) / 255.0; // red-orange energy: r over the greens and blues
                    if (p.r >= 230 && p.g <= 170 && p.b <= 110) hot++;
                    n++;
                }
            if (n == 0) return;
            int frame = FeelProbe.FrameCount;
            if (acc.f0 < 0) acc.f0 = frame;
            acc.f1 = frame;
            if (on) { acc.eOn += e / n; acc.hotOn += hot / n; acc.nOn++; acc.rectOn += (x1 - x0) * (y1 - y0); }
            else { acc.eOff += e / n; acc.hotOff += hot / n; acc.nOff++; acc.rectOff += (x1 - x0) * (y1 - y0); }
        }

        /// <summary>A spot `dist` m from `target` (horizontally, on the arena floor) with a clear line of sight to it, not inside a wall and with floor under it; false if
        /// none of 16 directions x 5 distances qualifies.</summary>
        static bool WormSpot(SurvivalGame g, Vec3 target, double dist, out Vec3 spot)
        {
            double floorY = g.sim.Wyrm24Spawn.y;
            spot = default;
            foreach (double mul in new[] { 1.0, 0.8, 1.25, 0.6, 1.5 })
                for (int k = 0; k < 16; k++)
                {
                    double ang = k * Math.PI / 8, r = dist * mul;
                    var pos = new Vec3(target.x + Math.Cos(ang) * r, floorY, target.z + Math.Sin(ang) * r);
                    var eye = new Vec3(pos.x, floorY + 1.6, pos.z);
                    Vec3 to = new Vec3(target.x, Math.Max(target.y, floorY + 1.6), target.z) - eye;
                    double len = to.Length;
                    if (len < 1) continue;
                    if (g.world.RayIntersect(eye, to / len, out RayHit h) && h.distance < len - 1.0) continue;
                    bool blocked = false;
                    foreach (Vec3 d in new[] { new Vec3(1, 0, 0), new Vec3(-1, 0, 0), new Vec3(0, 0, 1), new Vec3(0, 0, -1) })
                        if (g.world.RayIntersect(eye, d, out RayHit wh) && wh.distance < 1.2) blocked = true;
                    if (blocked) continue;
                    if (!g.world.RayIntersect(eye, new Vec3(0, -1, 0), out RayHit fh) || fh.distance > 2.2) continue;
                    spot = pos;
                    return true;
                }
            return false;
        }

        static Vec3 WormMiddle(Wyrm w)
        {
            double x = 0, y = 0, z = 0;
            int n = 0;
            for (int i = 0; i < w.PieceCount; i++)
            {
                if (w.buried[i]) continue;
                x += w.boxes[i].c.x; y += w.boxes[i].c.y; z += w.boxes[i].c.z;
                n++;
            }
            return n == 0 ? w.Center : new Vec3(x / n, y / n, z / n);
        }

        // --- wormcoil -------------------------------------------------------------------------------------------------------------------

        static IEnumerator WormCoilScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            Wyrm w = WormStart(g, res, out WyrmVisual26 vis, out string pc);
            if (w == null) yield break;
            WormBegin();
            wormMiddle = true; // the coil: the camera on the middle of the visible pieces
            yield return Frames(20);
            Start(g, "wormcoil", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();
            int n = w.PieceCount;

            // --- idle: the intro, the worm rising: the glow's floor and ceiling over the chain, the halo off ----------------------------------------------
            FeelProbe.Event("wormcoil:idle");
            float idleMin = 99f, idleMax = 0f;
            bool haloAtIdle = false;
            int frames = 0, guard = 0;
            WormPhase();
            while (w.phase == "intro" && guard++ < Ms(9000) && WormWall())
            {
                WormGod(g, w);
                yield return null;
                if (vis.Drawn && w.phase == "intro")
                {
                    idleMin = Mathf.Min(idleMin, MinGlow(vis, 1, n - 2));
                    idleMax = Mathf.Max(idleMax, MaxGlow(vis, 1, n - 2));
                    haloAtIdle |= vis.HaloOn;
                    frames++;
                }
            }
            if (frames < 30) fails.Add("only " + frames + " idle frames drawn (the worm was not drawn in the intro)");
            if (idleMax - idleMin < 0.05f) fails.Add("the idle pulse does not move (min " + F(idleMin) + ", max " + F(idleMax) + ")");
            if (haloAtIdle) fails.Add("the halo was on at idle");

            // --- the coil: the pulse rate at ~3 / ~1.5 / ~0.5 s before the release, the peak glow at it -------------------------------------------------
            FeelProbe.Event("wormcoil:coil");
            guard = 0;
            WormPhase();
            while (w.PatternName != "coil" && guard++ < Ms(9000) && WormWall()) { WormGod(g, w); yield return null; }
            if (w.PatternName != "coil") fails.Add("no coil pattern");
            // sample every coil frame (time, predicted seconds to the release, pulse rate, glow); after the pattern ends the readings are picked by the ACTUAL time to
            // the end (3 / 1.5 / 0.5 s before it, or at 75 / 40 / 10 % of the coil if it was shorter), and the prediction's error is printed
            var tS = new List<float>();
            var leftS = new List<float>();
            var rateS = new List<float>();
            var glowS = new List<float>();
            bool haloInCoil = false, embersInCoil = false, shellSeen = false;
            float coilGlowMax = 0f;
            guard = 0;
            WormPhase();
            int coilFrames = 0;
            // CLOSE-UP PROOF: stand 12 m from the coil's middle (a clear line of sight) and read the screen: 30 frames early in the coil and 30 frames in its last ~2 s, effects
            // ON and OFF on alternate frames (same pose, same sim): the red-orange energy inside the worm's screen rect
            var pxEarly = new PxAcc();
            var pxLate = new PxAcc();
            bool spotOk = WormSpot(g, WormMiddle(w), 12, out Vec3 coilSpot);
            if (spotOk) { g.Respawn(coilSpot, 0); WormAim(g, w, true); }
            else fails.Add("no clear spot 12 m from the coil for the close-up");
            int earlyDone = 0, lateDone = 0;
            while (w.PatternName == "coil" && guard++ < Ms(40000) && WormWall())
            {
                WormGod(g, w);
                bool windowEarly = spotOk && coilFrames >= 20 && earlyDone < 30;
                bool windowLate = spotOk && !windowEarly && vis.SecondsToRelease > 0f && vis.SecondsToRelease < 2.2f && lateDone < 30;
                bool armOn = true;
                if (windowEarly || windowLate)
                {
                    armOn = ((windowEarly ? earlyDone : lateDone) & 1) == 0;
                    vis.FxOn = armOn;
                }
                else vis.FxOn = true;
                yield return null;
                if (windowEarly || windowLate)
                {
                    yield return new WaitForEndOfFrame();
                    WormMeasure(g, w, true, armOn, windowEarly ? pxEarly : pxLate);
                    if (windowEarly) earlyDone++; else lateDone++;
                    vis.FxOn = true;
                    if (!armOn) continue; // an OFF frame is not a glow sample
                }
                if (!vis.Drawn || w.PatternName != "coil") continue;
                coilFrames++;
                tS.Add(coilFrames / (float)UnityTuning.ReelFps);
                leftS.Add(vis.SecondsToRelease);
                rateS.Add(vis.CoilRateHz);
                glowS.Add(MaxGlow(vis, 0, n - 1));
                haloInCoil |= vis.HaloOn;
                embersInCoil |= vis.EmbersDrawn > 0;
                shellSeen |= vis.DrawCallsLast > 3;
                coilGlowMax = Mathf.Max(coilGlowMax, glowS[glowS.Count - 1]);
            }
            vis.FxOn = true;
            float coilLen = coilFrames / (float)UnityTuning.ReelFps;
            float[] want = { 3f, 1.5f, 0.5f };
            if (coilLen < 3.5f) want = new[] { coilLen * 0.75f, coilLen * 0.4f, coilLen * 0.1f };
            float[] rate = { -1, -1, -1 }, glowAt = { -1, -1, -1 }, leftAt = { -1, -1, -1 }, predErr = { 0, 0, 0 };
            for (int k = 0; k < 3 && tS.Count > 0; k++)
            {
                int best = 0;
                float bd = float.MaxValue;
                for (int i = 0; i < tS.Count; i++)
                {
                    float d = Mathf.Abs((coilLen - tS[i]) - want[k]);
                    if (d < bd) { bd = d; best = i; }
                }
                rate[k] = rateS[best];
                glowAt[k] = glowS[best];
                leftAt[k] = coilLen - tS[best];
                predErr[k] = leftS[best] - leftAt[k]; // predicted minus actual seconds to the release
            }
            // --- the release: the first frames after the pattern changed ------------------------------------------------------------------------------
            FeelProbe.Event("wormcoil:release");
            float peak = 0f;
            int peakFrame = -1, emberBurst = 0;
            for (int i = 0; i < Ms(700); i++)
            {
                WormGod(g, w);
                yield return null;
                if (!vis.Drawn) continue;
                float m = MaxGlow(vis, 0, n - 1);
                if (m > peak)
                {
                    peak = m;
                    peakFrame = i;
                }
                emberBurst = Mathf.Max(emberBurst, vis.EmbersDrawn);
            }
            for (int k = 0; k < 3; k++) if (rate[k] < 0) fails.Add("no reading at " + F(want[k]) + " s before the release");
            if (rate[0] >= 0 && rate[2] >= 0 && !(rate[2] > rate[0] * 1.5f)) fails.Add("the pulse does not speed up (" + F(rate[0]) + " Hz -> " + F(rate[2]) + " Hz)");
            if (!haloInCoil) fails.Add("no halo during the coil");
            if (!embersInCoil) fails.Add("no embers during the coil");
            if (peak < coilGlowMax * 0.99f) fails.Add("the glow does not peak at the release (release window peak " + F(peak) + " under the coil's " + F(coilGlowMax) + ")");
            string pixLine = "CLOSE-UP from 12 m: " + pxEarly.Line("early coil") + "; " + pxLate.Line("late coil");
            if (spotOk && !(pxLate.nOn >= 5 && pxLate.nOff >= 5 && pxLate.MeanOn > pxLate.MeanOff * 1.15)) fails.Add("RED: the late-coil glow is not visible in the pixels (ON " + F(pxLate.MeanOn, "0.0000") + " vs OFF " + F(pxLate.MeanOff, "0.0000") + ")");
            string coilLine = pixLine + " | " + $"coil held {F(coilLen)} s ({coilFrames} frames); prediction error of the view's seconds-to-release {F(predErr[0])} / {F(predErr[1])} / {F(predErr[2])} s; idle glow min {F(idleMin)} max {F(idleMax)} over {frames} frames (band {F(UnityTuning.WyrmFxIdleMin)}..{F(UnityTuning.WyrmFxIdleMax)}), halo at idle {(haloAtIdle ? "ON" : "off")}; "
                + $"coil pulse {F(rate[0])} Hz at {F(leftAt[0])} s, {F(rate[1])} Hz at {F(leftAt[1])} s, {F(rate[2])} Hz at {F(leftAt[2])} s before the release (actual) "
                + $"(glow {F(glowAt[0])} / {F(glowAt[1])} / {F(glowAt[2])}); aura in the coil: halo {(haloInCoil ? "ON" : "off")}, embers {(embersInCoil ? "ON" : "off")}, shell {(shellSeen ? "ON" : "off")}; "
                + $"peak glow {F(peak)} {F(peakFrame * 1000.0 / UnityTuning.ReelFps, "0")} ms after the release (coil max {F(coilGlowMax)}), ember burst {emberBurst} embers";
            yield return Stop();

            // --- the frame cost: the effects on vs off, 24 husks alive ------------------------------------------------------------------------------------
            string cost = "";
            IEnumerator c = WormCost(g, vis, w, s => cost = s);
            while (c.MoveNext()) yield return c.Current;
            res[0] = $"{FeelProbe.FrameCount} frames; {pc}; design.n26.wormVisual ON | {coilLine} | {cost} | " + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }

        /// <summary>The cost helper: 24 husks near the worm (frozen), the worm fighting; 4 windows of `frames` frames (effects on, off, on, off); the mean frame time and
        /// the mean CPU time of WyrmVisual26.Render for each. The recording is over (the probe's capture would swamp the number).</summary>
        static IEnumerator WormCost(SurvivalGame g, WyrmVisual26 vis, Wyrm w, Action<string> done, int frames = 600)
        {
            Vec3 at = g.sim.Wyrm24Spawn;
            int husks = 0;
            for (int k = 0; k < 60 && husks < 24; k++)
            {
                double a = k * 2.399963, r = 5 + (k % 7) * 1.5;
                Husk h = g.husks.Spawn(new Vec3(at.x + Math.Cos(a) * r, at.y, at.z + Math.Sin(a) * r), g.mover.position);
                if (h != null)
                {
                    g.brain.Attach(h);
                    husks++;
                }
            }
            g.husks.frozen = true;
            var ms = new double[2];
            var cpu = new double[2];
            var passes = 0;
            int maxCalls = 0;
            for (int pass = 0; pass < 4; pass++)
            {
                bool on = pass % 2 == 0;
                vis.FxOn = on;
                vis.Measure = true;
                vis.RenderTicks = 0;
                vis.RenderCalls = 0;
                float t0 = Time.realtimeSinceStartup;
                WormPhase();
                for (int f = 0; f < frames && WormWall(); f++)
                {
                    WormGod(g, w);
                    yield return null;
                    maxCalls = Math.Max(maxCalls, vis.DrawCallsLast);
                }
                float dt = Time.realtimeSinceStartup - t0;
                int idx = on ? 0 : 1;
                ms[idx] += dt * 1000.0 / frames;
                cpu[idx] += vis.RenderCalls > 0 ? vis.RenderTicks * 1e6 / Stopwatch.Frequency / vis.RenderCalls : 0;
                passes++;
            }
            vis.Measure = false;
            vis.FxOn = true;
            double on0 = ms[0] / 2, off0 = ms[1] / 2;
            done($"FRAME COST with {husks} husks alive, {frames} frames x2 per arm: effects ON {F(on0)} ms/frame, OFF {F(off0)} ms/frame, difference {F(on0 - off0)} ms ({F(off0 > 0 ? (on0 - off0) / off0 * 100 : 0, "0.0")} %); "
                + $"WyrmVisual26.Render CPU ON {F(cpu[0] / 2, "0")} us, OFF {F(cpu[1] / 2, "0")} us; most draw calls in a frame {maxCalls}; GPU not measured here (use -skygrindBench 60 with the row on and off)");
        }

        // --- wormattack -------------------------------------------------------------------------------------------------------------------

        static IEnumerator WormAttackScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            Wyrm w = WormStart(g, res, out WyrmVisual26 vis, out string pc);
            if (w == null) yield break;
            WormBegin();
            wormMiddle = false; // the head
            yield return Frames(20);
            Start(g, "wormattack", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();
            var lines = new List<string>();
            int n = w.PieceCount;
            int guard = 0;
            WormPhase();
            while (w.phase == "intro" && guard++ < Ms(9000) && WormWall()) { WormGod(g, w); yield return null; }

            // --- telegraphs by kind: peak head glow, peak jaw glow, hue, the strongest body glow, from the tell's start to the impact -----------------------------------
            var peakHead = new Dictionary<WyrmKind26, float>();
            var peakJaw = new Dictionary<WyrmKind26, float>();
            var peakBody = new Dictionary<WyrmKind26, float>();
            var hueOf = new Dictionary<WyrmKind26, float>();
            var seenKinds = new List<WyrmKind26>();
            float impactPeak = 0f;
            var frontAt = new List<string>();
            int impacts = 0;
            WyrmKind26 lastKind = WyrmKind26.None;
            bool closeDone = false;
            var pxHead = new PxAcc();
            guard = 0;
            WormPhase();
            FeelProbe.Event("wormattack:telegraph");
            // phase 1 until a dive and a lunge have both told and landed (the cycle: coil, dive, lunge, loop)
            while (guard++ < Ms(80000) && WormWall() && !(peakHead.ContainsKey(WyrmKind26.Dive) && peakHead.ContainsKey(WyrmKind26.Lunge) && impacts >= 2 && w.phase == "p1" && lastKind == WyrmKind26.None))
            {
                WormGod(g, w);
                yield return null;
                if (!vis.Drawn) continue;
                WyrmKind26 k = vis.TellKindLast;
                if (k == WyrmKind26.Lunge && !closeDone)
                {
                    // CLOSE-UP PROOF: 8 m from the lunge's start spot, the camera on the head, 30 frames of the tell with the effects ON and OFF on alternate frames
                    closeDone = true;
                    WyrmMarker lm = null;
                    foreach (WyrmMarker m in w.markers) if (m.kind == "lunge") lm = m;
                    if (lm != null && WormSpot(g, lm.pos, 8, out Vec3 headSpot))
                    {
                        g.Respawn(headSpot, 0);
                        for (int f = 0; f < 30 && vis.TellKindLast == WyrmKind26.Lunge && WormWall(); f++)
                        {
                            WormGod(g, w);
                            bool armOn = (f & 1) == 0;
                            vis.FxOn = armOn;
                            yield return null;
                            yield return new WaitForEndOfFrame();
                            WormMeasure(g, w, false, armOn, pxHead);
                            if (armOn && vis.Drawn)
                            {
                                peakHead[k] = Mathf.Max(peakHead.TryGetValue(k, out float ph0) ? ph0 : 0f, vis.Fx[0].glow);
                                peakJaw[k] = Mathf.Max(peakJaw.TryGetValue(k, out float pj0) ? pj0 : 0f, vis.Fx[0].jaw);
                            }
                        }
                        vis.FxOn = true;
                    }
                    else fails.Add("no clear spot 8 m from the lunge's start for the close-up");
                    k = vis.TellKindLast;
                }
                if (k != WyrmKind26.None)
                {
                    // the close-up branch above may have added only the head and jaw peaks for this kind: seed each map on its own
                    if (!peakHead.ContainsKey(k)) peakHead[k] = 0f;
                    if (!peakJaw.ContainsKey(k)) peakJaw[k] = 0f;
                    if (!peakBody.ContainsKey(k)) peakBody[k] = 0f;
                    if (!hueOf.ContainsKey(k)) hueOf[k] = 0f;
                    if (!seenKinds.Contains(k)) seenKinds.Add(k);
                    peakHead[k] = Mathf.Max(peakHead[k], vis.Fx[0].glow);
                    peakJaw[k] = Mathf.Max(peakJaw[k], vis.Fx[0].jaw);
                    peakBody[k] = Mathf.Max(peakBody[k], MaxGlow(vis, 1, n - 2));
                    hueOf[k] = Mathf.Max(hueOf[k], vis.Fx[0].hue);
                    lastKind = k;
                }
                else if (lastKind != WyrmKind26.None)
                {
                    // the tell resolved: the impact. Read the head's flash and the trail's front (the body piece with the most glow) 0.1 / 0.3 / 0.6 s later
                    impacts++;
                    FeelProbe.Event("wormattack:impact");
                    impactPeak = Mathf.Max(impactPeak, vis.Fx[0].glow);
                    var fronts = new List<int>();
                    int prevMs = 0;
                    foreach (int at in new[] { 100, 300, 600 })
                    {
                        for (int i = 0; i < Ms(at - prevMs); i++) { WormGod(g, w); yield return null; }
                        prevMs = at;
                        int best = 1;
                        float bv = -1f;
                        for (int i = 1; i < n - 1; i++)
                            if (vis.Fx[i].glow > bv) { bv = vis.Fx[i].glow; best = i; }
                        fronts.Add(best);
                        impactPeak = Mathf.Max(impactPeak, vis.Fx[0].glow);
                    }
                    frontAt.Add(lastKind + " front at piece " + string.Join(" -> ", fronts) + " (0.1 / 0.3 / 0.6 s after)");
                    if (!(fronts[0] <= fronts[1] && fronts[1] <= fronts[2]) && lastKind == WyrmKind26.Dive) fails.Add("the dive's trail front does not run head to tail (" + string.Join(" -> ", fronts) + ")");
                    lastKind = WyrmKind26.None;
                }
            }
            lines.Add("CLOSE-UP from 8 m: " + pxHead.Line("lunge telegraph, head"));
            if (closeDone && !(pxHead.nOn >= 5 && pxHead.nOff >= 5 && pxHead.MeanOn > pxHead.MeanOff * 1.15)) fails.Add("RED: the lunge telegraph's glow is not visible in the pixels on the head (ON " + F(pxHead.MeanOn, "0.0000") + " vs OFF " + F(pxHead.MeanOff, "0.0000") + ")");
            foreach (WyrmKind26 k in seenKinds)
                lines.Add($"{k}: head {F(peakHead[k])}, jaw {F(peakJaw[k])}, body max {F(peakBody[k])}, hue {F(hueOf[k])}, strobe {F(UnityTuning.WyrmFxTellRate[(int)k], "0.0")} Hz");
            foreach (WyrmKind26 need in new[] { WyrmKind26.Dive, WyrmKind26.Lunge })
                if (!peakHead.ContainsKey(need)) fails.Add("no " + need + " telegraph glow seen");
            if (impacts == 0) fails.Add("no impact seen");
            if (impactPeak < 1f) fails.Add("the impact flash is weak (head glow peak " + F(impactPeak) + ")");

            // --- the damage flash: a hit on piece 6 (a scripted ray), white-hot, then gone; a neighbour less, a far piece none --------------------------------------------------
            FeelProbe.Event("wormattack:hit");
            int target = 6;
            while (w.buried[target] && target < n - 3) target++;
            WyrmBox tb = w.boxes[target];
            Vec3 dirIn = tb.u * -1.0;
            WyrmVisual26.HitRayProbe = (tb.c + tb.u * 6.0, dirIn);
            double hp0 = w.body.hp;
            w.Apply(1);
            yield return null;
            WyrmVisual26.HitRayProbe = null;
            float hot0 = vis.Fx[target].hot, hotNb = vis.Fx[target + 1].hot, hotFar = vis.Fx[Math.Min(n - 2, target + 8)].hot, glowHit = vis.Fx[target].glow;
            int pieceSeen = vis.HitPieceLast;
            for (int i = 0; i < Ms(400); i++) { WormGod(g, w); yield return null; }
            float hotLater = vis.Fx[target].hot;
            lines.Add($"damage: hit piece {target} (view found {pieceSeen}): hot {F(hot0)} on it, {F(hotNb)} next to it, {F(hotFar)} 8 pieces away, glow {F(glowHit)}; 0.4 s later hot {F(hotLater)}");
            if (pieceSeen != target) fails.Add("the hit piece was " + pieceSeen + ", not " + target);
            if (hot0 < 0.5f || hotNb <= 0f || hotNb >= hot0 || hotFar > 0.001f || hotLater > 0.001f) fails.Add("the white-hot flash is wrong (on " + F(hot0) + ", next " + F(hotNb) + ", far " + F(hotFar) + ", later " + F(hotLater) + ")");

            // --- the HP dimming: the idle-level glow (the same phase: read the HpDim factor and the mean body glow over 1 s) -------------------------------------------------------
            FeelProbe.Event("wormattack:dim");
            var dim = new List<string>();
            float meanFull = -1f, meanLow = -1f;
            foreach (double frac in new[] { 1.0, 0.7 })
            {
                w.body.hp = w.body.maxHp * frac;
                yield return Frames(4);
                double sum = 0;
                int cnt = 0;
                for (int i = 0; i < Ms(1000); i++)
                {
                    WormGod(g, w);
                    yield return null;
                    if (vis.TellKindLast != WyrmKind26.None) continue;
                    for (int k = 1; k < n - 1; k++) { sum += vis.Fx[k].glow; cnt++; }
                }
                float mean = cnt > 0 ? (float)(sum / cnt) : -1f;
                if (frac == 1.0) meanFull = mean; else meanLow = mean;
                dim.Add($"{F(frac, "0.0")} hp: HpDim {F(WyrmGlow26.HpDim((float)frac))}, mean untelegraphed body glow {F(mean)}");
            }
            lines.Add("dimming: " + string.Join("; ", dim));
            if (!(WyrmGlow26.HpDim(0.7f) < WyrmGlow26.HpDim(1f))) fails.Add("HpDim does not fall with the HP");

            // --- phase 2: the spit's and the roar's tells (kinds only seen from phase 2), the p2 HP dim --------------------------------------------------------------------------
            FeelProbe.Event("wormattack:p2");
            w.body.hp = w.body.maxHp * 0.6;
            w.Apply(0);
            guard = 0;
            WormPhase();
            while (w.phase != "p2" && guard++ < Ms(5000) && WormWall()) { WormGod(g, w); yield return null; }
            if (w.phase != "p2") fails.Add("phase 2 not reached");
            else
            {
                var p2Kinds = new HashSet<WyrmKind26>();
                var p2Head = new Dictionary<WyrmKind26, float>();
                var p2Body = new Dictionary<WyrmKind26, float>();
                guard = 0;
                WormPhase();
            WormPhase();
                while (guard++ < Ms(40000) && WormWall() && !(p2Kinds.Contains(WyrmKind26.Spit) && p2Kinds.Contains(WyrmKind26.Roar)))
                {
                    WormGod(g, w);
                    yield return null;
                    WyrmKind26 k = vis.TellKindLast;
                    if (k != WyrmKind26.Spit && k != WyrmKind26.Roar) continue;
                    p2Kinds.Add(k);
                    p2Head[k] = Mathf.Max(p2Head.TryGetValue(k, out float h0) ? h0 : 0f, vis.Fx[0].glow);
                    p2Body[k] = Mathf.Max(p2Body.TryGetValue(k, out float b0) ? b0 : 0f, MaxGlow(vis, 1, n - 2));
                }
                foreach (WyrmKind26 k in p2Kinds) lines.Add($"{k} (phase 2): head {F(p2Head[k])}, body max {F(p2Body[k])}, hue {F(UnityTuning.WyrmFxTellHue[(int)k])}, strobe {F(UnityTuning.WyrmFxTellRate[(int)k], "0.0")} Hz");
                if (!p2Kinds.Contains(WyrmKind26.Spit)) lines.Add("spit tell: not seen in 40 s");
                if (!p2Kinds.Contains(WyrmKind26.Roar)) lines.Add("roar tell: not seen in 40 s");
            }

            // --- death: the flare, then the burn-out order ---------------------------------------------------------------------------------------------------------------------------
            FeelProbe.Event("wormattack:death");
            wormMiddle = true; // the whole worm in frame for the burn-out
            w.body.hp = w.body.maxHp * 0.3;
            w.Apply(0);
            guard = 0;
            WormPhase();
            while (w.phase != "p3" && guard++ < Ms(5000) && WormWall()) { WormGod(g, w); yield return null; }
            w.body.hp = 1;
            w.Apply(5);
            var burnedAt = new float[n];
            for (int i = 0; i < n; i++) burnedAt[i] = -1f;
            float flareGlow = 0f, flareHot = 0f;
            int deathFrames = 0;
            guard = 0;
            WormPhase();
            int stillDrawnAfterSim = 0;
            while (guard++ < Ms(14000) && WormWall())
            {
                yield return null;
                WormGod(g, w);
                deathFrames++;
                if (vis.Drawn)
                {
                    if (!w.Alive) stillDrawnAfterSim++;
                    float t = vis.State.deathAge;
                    if (t >= 0f && t < UnityTuning.WyrmFxFlareSec + 0.1f)
                    {
                        flareGlow = Mathf.Max(flareGlow, MaxGlow(vis, 0, n - 1));
                        for (int i = 0; i < n; i++) flareHot = Mathf.Max(flareHot, vis.Fx[i].hot);
                    }
                    for (int i = 0; i < n; i++)
                        if (burnedAt[i] < 0f && vis.Fx[i].burn >= 0.99f) burnedAt[i] = t;
                }
                else if (!w.Alive && deathFrames > 10) break;
            }
            bool order = true;
            int unburnt = 0;
            for (int i = 0; i < n; i++)
            {
                if (burnedAt[i] < 0f) { unburnt++; continue; }
                if (i > 0 && burnedAt[i - 1] >= 0f && burnedAt[i] < burnedAt[i - 1]) order = false;
            }
            lines.Add($"death: flare glow {F(flareGlow)}, white-hot {F(flareHot)}; burn-out head {F(burnedAt[0])} s, piece {n / 2} {F(burnedAt[n / 2])} s, tail {F(burnedAt[n - 1])} s after the finisher began (planned {F(WyrmGlow26.BurnOutTime(0))} .. {F(WyrmGlow26.BurnOutTime(n - 1))} s); "
                + $"order head to tail {(order ? "yes" : "NO")}; {unburnt} pieces never burnt out; drawn {stillDrawnAfterSim} frames after the sim's death");
            if (!order) fails.Add("the burn-out is not head to tail");
            if (unburnt > 0) fails.Add(unburnt + " pieces never burnt out");
            if (flareGlow < 1.5f) fails.Add("the death flare is weak (" + F(flareGlow) + ")");
            yield return Frames(Ms(300));
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {pc}; design.n26.wormVisual ON | " + string.Join(" | ", lines) + " | " + string.Join(" | ", frontAt) + " | " + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }

    }
}
