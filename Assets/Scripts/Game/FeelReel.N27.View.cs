// NIGHT27 builder V: the reel scenarios `roof`, `skate` and `slowaim` (the three view rows). Written first, compiled by the dn-check, NOT RUN on a Player by the
// builder (no Editor in the builder's worktree). Each is bounded by game time (frames from Ms) AND a real-time wall guard, reads the same state a player has
// (the mover, the ice rect, the cue's Volume), starts with FeelReel.Preconditions(g, pos) from SafeSpot(g), and prints one result line: PASS, or RED with the reason.
// The lead adds, in FeelReel.N27.cs (declarations) and the phase list, the three hooks:
//   static partial void N27RoofBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = RoofScenario27(g, dir, res, offs, pre);
//   static partial void N27SkateBody(...same...) => body = SkateScenario27(g, dir, res, offs, pre);
//   static partial void N27SlowaimBody(...same...) => body = SlowAimScenario27(g, dir, res, offs, pre);
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        // --- roof ---------------------------------------------------------------------------------------------------------------------

        /// <summary>Stand in the level with the roof ON for a few seconds of frames, then average the frame time over N frames with the roof's meshes and lights ON and OFF
        /// (the same view, the player still). PASS: the view exists with a ceiling, 6-10 strips a room, AO strips and at most 4 lights in all. RED: row off, or not a campaign level.</summary>
        public static IEnumerator RoofScenario27(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            float wall0 = Time.realtimeSinceStartup;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            bool rowOn = !N27Blocks.ForceOff && g.cfg.Design("n27.roof27.enabled");
            CampaignLighting24 kit = UnityEngine.Object.FindObjectOfType<CampaignLighting24>();
            RoofView27 roof = kit != null ? kit.roof : null;
            offs.Add(() => { if (roof != null) roof.SetVisible(true); });
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            Start(g, "roof", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            FeelProbe.Event("roofOn");
            for (int f = 0; f < Ms(2500) && Time.realtimeSinceStartup - wall0 < UnityTuning.ReelViewWallGuard27; f++) { God(g); yield return null; }
            // the PIXEL GUARD: a frame that is flat (the first n27-4 / n27-5 regressions filled the view with one colour yet passed) is RED
            double lumaStd = -1;
            {
                yield return new WaitForEndOfFrame();
                lumaStd = CenterLumaStd27();
            }
            double msOn = 0, msOff = 0;
            if (roof != null)
            {
                int n = UnityTuning.ReelRoofMsFrames27;
                for (int f = 0; f < n && Time.realtimeSinceStartup - wall0 < UnityTuning.ReelViewWallGuard27; f++) { God(g); yield return null; msOn += Time.unscaledDeltaTime * 1000.0; }
                roof.SetVisible(false);
                FeelProbe.Event("roofOff");
                yield return Frames(10);
                for (int f = 0; f < n && Time.realtimeSinceStartup - wall0 < UnityTuning.ReelViewWallGuard27; f++) { God(g); yield return null; msOff += Time.unscaledDeltaTime * 1000.0; }
                roof.SetVisible(true);
                msOn /= n; msOff /= n;
            }
            yield return Stop();
            bool flat = lumaStd >= 0 && lumaStd < UnityTuning.ReelMinLumaStd27;
            bool ok = !flat && roof != null && roof.ceilingQuads > 0 && roof.strips >= 6 && roof.extraLights + 2 <= 4 && roof.aoQuads > 0;
            string why = !rowOn ? "RED: design.n27.roof27 is off" : kit == null ? "RED: not a campaign level (no CampaignLighting24)" : roof == null ? "RED: the roof view was not built" : "RED: a count is out of range";
            if (flat) why = "RED: the frame is FLAT (centre luminance std " + lumaStd.ToString("0.000", ci) + " < " + UnityTuning.ReelMinLumaStd27.ToString("0.000", ci) + "): something opaque or a fog fills the view";
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: roof27 {(rowOn ? "on" : "OFF")}: " + (roof == null ? "no view" :
                $"ceiling y {roof.ceilingY.ToString("0.0", ci)} m, {roof.ceilingQuads} ceiling quads, {roof.strips} strips, {roof.aoQuads} AO quads, {roof.extraLights} extra lights (+2 room kit); centre-40% luminance std {lumaStd.ToString("0.000", ci)}; frame ms ON {msOn.ToString("0.00", ci)} vs OFF {msOff.ToString("0.00", ci)} (player still, {UnityTuning.ReelRoofMsFrames27} frames a side, shared box: indicative)")
                + ": " + (ok ? "PASS" : why);
        }

        /// <summary>The standard deviation (0..1) of the luminance of the centre 40 % of the screen, sampled every 8 px; call after WaitForEndOfFrame. -1 when the read failed.</summary>
        static double CenterLumaStd27()
        {
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) return -1;
                int w = t.width, h = t.height, x0 = (int)(w * 0.3), x1 = (int)(w * 0.7), y0 = (int)(h * 0.3), y1 = (int)(h * 0.7);
                double sum = 0, sum2 = 0;
                int n = 0;
                for (int y = y0; y < y1; y += 8)
                    for (int x = x0; x < x1; x += 8)
                    {
                        Color c = t.GetPixel(x, y);
                        double l = 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                        sum += l; sum2 += l * l; n++;
                    }
                if (n == 0) return -1;
                double mean = sum / n;
                return Math.Sqrt(Math.Max(0, sum2 / n - mean * mean));
            }
            catch (Exception) { return -1; }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        // --- skate --------------------------------------------------------------------------------------------------------------------

        /// <summary>Lay an ice rect around the safe spot, sprint 0.7 s, release and coast 0.9 s on it; read SkateFx27's own numbers every frame: the peak FOV add, the peak |roll|, the
        /// peak streak strength. PASS: FOV add above 0.5 deg and strength above 0.3 (the skate view reacted to speed on ice). RED: the row is off.</summary>
        public static IEnumerator SkateScenario27(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            float wall0 = Time.realtimeSinceStartup;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            bool rowOn = !N27Blocks.ForceOff && g.cfg.Design("n27.skate27.enabled");
            Surfaces26 saved = g.sim.surfaces26;
            SkateFx27 fx = SkateFx27.Active;
            bool made = false;
            if (rowOn && fx == null) { fx = SkateFx27.Create(g, g.transform); made = true; }
            offs.Add(() => { g.forcedMove = null; g.sim.surfaces26 = saved; g.sim.mover.groundFrictionMult = 1; if (made && fx != null) UnityEngine.Object.Destroy(fx.gameObject); });
            var z = new Surfaces26(SurfaceRules.From(g.cfg));
            z.zones.Add(new SurfaceZone { kind = SurfaceKind.Ice, x0 = spot.x - 80, x1 = spot.x + 80, z0 = spot.z - 80, z1 = spot.z + 80 });
            g.sim.surfaces26 = z;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            Start(g, "skate", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            FeelProbe.Event("skateStart");
            Mover m = g.sim.mover;
            double topSpeed = 0, maxFov = 0, maxRoll = 0, maxStr = 0;
            double fullSpeed = g.cfg.movement.ground.walkSpeed * g.cfg.Num("design.n27.skate27.fovFullMult");
            // phases in game time (a straight run of 0.8 s, then a steady left turn at 1.4 rad/s so the run stays inside a ~15 m patch and carves): sprint 0.8 s,
            // sprint + turn with a slide held for the first 0.5 s 1.4 s, sprint + turn + strafe right 0.8 s, coast 0.6 s
            int p1 = Ms(800), p2 = p1 + Ms(1400), p3 = p2 + Ms(800), p4 = p3 + Ms(600);
            int slideEnd = p1 + Ms(500);
            double yawNow = spotYaw;
            for (int f = 0; f < p4 && Time.realtimeSinceStartup - wall0 < UnityTuning.ReelViewWallGuard27; f++)
            {
                God(g);
                if (f >= p1 && f < p3) yawNow += 1.4 / UnityTuning.ReelFps;
                g.input.SetLook(yawNow, 0);
                var sprint = new Skygrind.Core.Button { held = true, pressed = f == 0 };
                if (f < p1) g.forcedMove = new InputFrame { moveZ = 1, sprint = sprint };
                else if (f < p2) g.forcedMove = new InputFrame { moveZ = 1, sprint = sprint, slide = f < slideEnd ? new Skygrind.Core.Button { held = true, pressed = f == p1 } : default };
                else if (f < p3) g.forcedMove = new InputFrame { moveZ = 1, moveX = 1, sprint = sprint };
                else g.forcedMove = new InputFrame();
                yield return null;
                topSpeed = Math.Max(topSpeed, m.Speed);
                if (fx != null) { maxFov = Math.Max(maxFov, fx.fovAdd); maxRoll = Math.Max(maxRoll, Math.Abs(fx.rollDeg)); maxStr = Math.Max(maxStr, fx.strength); }
            }
            g.forcedMove = null;
            yield return Stop();
            bool ok = rowOn && fx != null && maxFov > 0.5 && maxStr > 0.3 && maxRoll > 0.3;
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: ice rect, sprint 0.8 s, then a 1.4 rad/s turn (slide 0.5 s, then strafe), coast 0.6 s: top {topSpeed.ToString("0.0", ci)} m/s, peak FOV add {maxFov.ToString("0.00", ci)} deg, peak |roll| {maxRoll.ToString("0.00", ci)} deg, peak streak strength {maxStr.ToString("0.00", ci)} (strength 1 at {fullSpeed.ToString("0.0", ci)} m/s; design.n27.skate27 {(rowOn ? "on" : "OFF")}; the floor-pixel motion is read from the frames): "
                + (ok ? "PASS" : !rowOn ? "RED: design.n27.skate27 is off" : "RED: the view did not react (FOV add <= 0.5 deg, strength <= 0.3 or |roll| <= 0.3 deg)");
        }

        // --- slowaim ------------------------------------------------------------------------------------------------------------------

        /// <summary>Drive the cue by its own ease (130 ms in, held 0.8 s, 100 ms out) with the loop's driver paused, and read the camera's Volume stack: vignette, chromatic aberration, saturation
        /// and the FOV add at slow01 0 and 1. PASS: vignette at 1 at least 0.2 above 0, saturation at 1 below 0, FOV add 4 deg. The loop's own scale (wall ride + aim) is the lead's hook; this reads the cue.</summary>
        public static IEnumerator SlowAimScenario27(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            float wall0 = Time.realtimeSinceStartup;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            bool rowOn = !N27Blocks.ForceOff && g.cfg.Design("n27.slowAim27.enabled");
            SlowAim27Driver driver = UnityEngine.Object.FindObjectOfType<SlowAim27Driver>();
            SlowAim27 cue = null;
            bool made = false;
            if (rowOn)
            {
                if (driver == null) { cue = SlowAim27.Create(g, g.transform); driver = UnityEngine.Object.FindObjectOfType<SlowAim27Driver>(); made = true; }
                else cue = driver.Cue;
            }
            if (driver != null) driver.enabled = false; // the scenario drives slow01 itself
            offs.Add(() => { if (driver != null) driver.enabled = true; if (made && driver != null) UnityEngine.Object.Destroy(driver.gameObject); if (cue != null) cue.Set(0); });
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            var stack = g.cam.GetUniversalAdditionalCameraData().volumeStack;
            float vig0 = 0, vig1 = 0, ch1 = 0, sat1 = 0, fov1 = 0;
            if (cue != null)
            {
                cue.Set(0); yield return Frames(3);
                vig0 = stack != null && stack.GetComponent<Vignette>() != null ? stack.GetComponent<Vignette>().intensity.value : 0;
                cue.Set(1); yield return Frames(3);
                var vg = stack != null ? stack.GetComponent<Vignette>() : null;
                var ca = stack != null ? stack.GetComponent<ChromaticAberration>() : null;
                var co = stack != null ? stack.GetComponent<ColorAdjustments>() : null;
                vig1 = vg != null ? vg.intensity.value : 0; ch1 = ca != null ? ca.intensity.value : 0; sat1 = co != null ? co.saturation.value : 0; fov1 = cue.FovAdd;
                cue.Set(0); yield return Frames(3);
            }
            Start(g, "slowaim", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            FeelProbe.Event("slowIn");
            float s01 = 0;
            float dt = 1f / (float)UnityTuning.ReelFps;
            float inS = (float)g.cfg.Num("design.n27.slowAim27.easeInS"), outS = (float)g.cfg.Num("design.n27.slowAim27.easeOutS");
            int hold = Ms(800), tail = Ms(500);
            for (int f = 0; f < Ms(300) + hold + tail && Time.realtimeSinceStartup - wall0 < UnityTuning.ReelViewWallGuard27; f++)
            {
                God(g);
                bool target = f < Ms(300) + hold;
                s01 = SlowAim27.Ease(s01, target, dt, inS, outS);
                if (cue != null) cue.Set(s01);
                if (f == Ms(300)) FeelProbe.Event("slowOut");
                yield return null;
            }
            if (cue != null) cue.Set(0);
            yield return Stop();
            bool ok = rowOn && cue != null && vig1 - vig0 >= 0.2f && sat1 < 0 && fov1 > 3.5f;
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: cue at slow01 0 -> 1: vignette {vig0.ToString("0.00", ci)} -> {vig1.ToString("0.00", ci)}, chromatic aberration {ch1.ToString("0.00", ci)}, saturation {sat1.ToString("0.0", ci)}, FOV add {fov1.ToString("0.0", ci)} deg (design.n27.slowAim27 {(rowOn ? "on" : "OFF")}; the world scale is the loop hook's, see the Player line): "
                + (ok ? "PASS" : !rowOn ? "RED: design.n27.slowAim27 is off" : "RED: the Volume did not take the cue (vignette < +0.2, saturation >= 0 or FOV add < 3.5)");
        }
    }
}
