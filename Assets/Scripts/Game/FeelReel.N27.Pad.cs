// NIGHT27 builder P: the reel scenario `jumppad` (the jump pad's look, Game/JumpPadView27.cs). Written first, compiled by the dn-check, NOT RUN on a Player by the builder
// (no Editor in the builder's worktree). Bounded by game time and a real-time wall guard. It stands 8 m from the first enabled pad that has a nav-valid standing spot at
// that distance, looks down at it, records 4 frames' worth of samples (pulse every frame for the whole window, a screen read at each of the 4 frame points), then fires the
// view's burst and reads it. One result line: PASS or RED.
//   mesh count / chevron count    the pad's MeshFilters and the chevron renderers (read from the view)
//   pulse min / max               the idle glow multiplier over the window
//   contrast                      mean luminance of the pad's screen ellipse over the mean of the floor around it (lower half and sides of an annulus), min over the 4 frames
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
        static partial void N27JumppadBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = JumpPadScenario27(g, dir, res, offs, pre);

        public static IEnumerator JumpPadScenario27(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            float wall0 = Time.realtimeSinceStartup;
            Zones zones = g.sim.zones;
            JumpPadView27[] views = UnityEngine.Object.FindObjectsByType<JumpPadView27>(FindObjectsSortMode.None);
            // the pad: enabled, with a nav-valid standing spot 8 m away on some of 16 bearings
            double below = g.sim.cfg.Num("ai.grid.lookBelow"), above = g.sim.cfg.Num("ai.grid.lookAbove");
            double R = g.sim.cfg.Num("jumpPad.radius"), dist = UnityTuning.Pad27ReelDistance;
            JumpPadView27 view = null;
            Vec3 spot = default;
            double yaw = 0;
            Vec3 padPos = default;
            foreach (JumpPadView27 v in views)
            {
                if (v.index >= g.sim.map.pads.Count || !zones.padEnabled[v.index]) continue;
                PadSpec p = g.sim.map.pads[v.index];
                for (int a = 0; a < 16 && view == null; a++)
                {
                    double ang = a * Math.PI * 2 / 16;
                    var at = new Vec3(p.pos.x + Math.Cos(ang) * dist, p.pos.y, p.pos.z + Math.Sin(ang) * dist);
                    if (g.sim.nav.grid.NodeAt(at.x, at.y, at.z, below, above) < 0) continue;
                    view = v; spot = at; padPos = p.pos;
                    yaw = Math.Atan2(-(p.pos.x - at.x), -(p.pos.z - at.z)); // yaw 0 looks toward web -z
                }
                if (view != null) break;
            }
            if (view == null)
            {
                res[0] = $"{views.Length} pad views | RED: no enabled pad with a nav-valid standing spot {dist:0} m away";
                yield break;
            }
            string pc = Preconditions(g, spot) + $" (looking at pad {view.index})";
            double eyeH = g.cfg.movement.body.eyeHeight;
            g.Respawn(spot, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, -Math.Atan2(eyeH - 0.15, dist));
            yield return Frames(30);
            Start(g, "jumppad", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            FeelProbe.Event("padLook");
            int gap = Ms(500);
            double pulseMin = double.MaxValue, pulseMax = double.MinValue, minContrast = double.MaxValue, bestPad = 0, bestFloor = 0;
            int reads = 0;
            for (int k = 0; k < UnityTuning.Pad27ReelFrames && Time.realtimeSinceStartup - wall0 < UnityTuning.ReelViewWallGuard27; k++)
            {
                for (int f = 0; f < gap; f++)
                {
                    God(g);
                    yield return null;
                    pulseMin = Math.Min(pulseMin, view.pulse); pulseMax = Math.Max(pulseMax, view.pulse);
                }
                FeelProbe.Event("padFrame" + (k + 1));
                yield return new WaitForEndOfFrame();
                if (PadContrast27(g.cam, view.transform.position, (float)R, out double padL, out double floorL))
                {
                    reads++;
                    double c = padL / Math.Max(floorL, 0.01);
                    if (c < minContrast) { minContrast = c; bestPad = padL; bestFloor = floorL; }
                }
            }
            // the launch burst (the view's own call; the real "jumpPad" event runs the same method)
            view.Burst();
            double burstPeak = 0;
            for (int f = 0; f < Ms(200); f++) { God(g); yield return null; burstPeak = Math.Max(burstPeak, view.burst); }
            yield return Stop();
            double ratio = pulseMax / Math.Max(pulseMin, 1e-6);
            bool ok = view.meshCount >= 6 && view.chevronCount >= 3 && ratio >= UnityTuning.Pad27ReelMinPulseRatio && reads == UnityTuning.Pad27ReelFrames
                && minContrast >= UnityTuning.Pad27ReelMinContrast && burstPeak > 0.5;
            string why = reads < UnityTuning.Pad27ReelFrames ? $"RED: only {reads} of {UnityTuning.Pad27ReelFrames} screen reads worked (or the wall guard ran out)"
                : view.meshCount < 6 || view.chevronCount < 3 ? "RED: the pad has too few meshes or chevrons"
                : ratio < UnityTuning.Pad27ReelMinPulseRatio ? "RED: the glow does not pulse"
                : burstPeak <= 0.5 ? "RED: the burst did not flare"
                : $"RED: pad / floor luminance {minContrast.ToString("0.00", ci)} < {UnityTuning.Pad27ReelMinContrast.ToString("0.0", ci)}";
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: pad {view.index} ({(view.aimed ? "aimed: flat chevrons to the target" : "unaimed: rising chevrons")}): {view.meshCount} meshes, {view.chevronCount} chevrons, "
                + $"glow pulse {pulseMin.ToString("0.00", ci)}..{pulseMax.ToString("0.00", ci)} (x{ratio.ToString("0.00", ci)}), burst peak +{burstPeak.ToString("0.00", ci)}, "
                + $"worst pad/floor luminance {(reads > 0 ? bestPad.ToString("0.000", ci) + " / " + bestFloor.ToString("0.000", ci) + " = " + minContrast.ToString("0.00", ci) : "n/a")} (>= {UnityTuning.Pad27ReelMinContrast.ToString("0.0", ci)}): "
                + (ok ? "PASS" : why);
        }

        /// <summary>The mean luminance of the pad's screen ellipse and of the floor around it, from the frame just rendered (call after WaitForEndOfFrame). The pad ellipse is the pad
        /// disc projected (90 % of it); the floor is the annulus from 2.1x to 3.0x that (outside the halo), lower half and sides only (above the pad is the stream and the wall). False on a failed read or a pad off screen.</summary>
        static bool PadContrast27(Camera cam, Vector3 centre, float radius, out double padL, out double floorL)
        {
            padL = floorL = 0;
            Texture2D t = null;
            try
            {
                Vector3 sc = cam.WorldToScreenPoint(centre);
                if (sc.z <= 0) return false;
                Vector3 right = cam.transform.right; right.y = 0; right.Normalize();
                Vector3 toward = centre - cam.transform.position; toward.y = 0; toward.Normalize();
                Vector3 sx = cam.WorldToScreenPoint(centre + right * radius), sy = cam.WorldToScreenPoint(centre + toward * radius);
                float rx = Mathf.Abs(sx.x - sc.x) * 0.9f, ry = Mathf.Abs(sy.y - sc.y) * 0.9f;
                if (rx < 2f || ry < 1f) return false;
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) return false;
                double ps = 0, fs = 0;
                int pn = 0, fn = 0;
                int x0 = Mathf.Max(0, (int)(sc.x - rx * 3.0f)), x1 = Mathf.Min(t.width - 1, (int)(sc.x + rx * 3.0f));
                int y0 = Mathf.Max(0, (int)(sc.y - ry * 3.0f)), y1 = Mathf.Min(t.height - 1, (int)(sc.y + ry * 3.0f));
                for (int y = y0; y <= y1; y += 2)
                    for (int x = x0; x <= x1; x += 2)
                    {
                        float dx = (x - sc.x) / rx, dy = (y - sc.y) / ry, d2 = dx * dx + dy * dy;
                        bool inPad = d2 <= 1f, inFloor = d2 >= 2.1f * 2.1f && d2 <= 3.0f * 3.0f && dy <= 0.3f;
                        if (!inPad && !inFloor) continue;
                        Color c = t.GetPixel(x, y);
                        double l = 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                        if (inPad) { ps += l; pn++; } else { fs += l; fn++; }
                    }
                if (pn < 20 || fn < 20) return false;
                padL = ps / pn; floorL = fs / fn;
                return true;
            }
            catch (Exception) { return false; }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }
    }
}
