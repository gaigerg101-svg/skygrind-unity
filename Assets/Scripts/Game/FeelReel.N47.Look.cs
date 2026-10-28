// NIGHT47 builder F: the a847 phase (FeelReel.N47.cs routes it here): the render rows' proof, RED (row off) then GREEN (row on) in ONE Player.
//   reel.sh + -skygrindMap n46-foundry + -skygrindReelOnly a847     (the door passes and the lava walk; any level: run once per level 1-5)
// The reel starts the CAMPAIGN on the booted map (N24StartCampaign, L-062), DOORFREE (every door open), God, enemies frozen. Then:
//   DOOR (G10, door47; F-cards 6a): the nearest open curtain; Passes47 passes through it at the eye's height, each pass: one side, 2 frames,
//     the other side (the camera crosses the plane within 2 m: Doors38View's shimmer), a capture 2 frames into the shimmer: magenta pixels counted
//     on a 64 x 36 grid (R > 0.8, G < 0.3, B > 0.8) and Doors38View.StaleVeilFrames read. Three sets: "fresh" (door47 off, no re-attach: the
//     first level of a session: expected clean), "off" (door47 off after Doors38View.ReelReattach(): what a level change does: expected RED),
//     "on" (door47 on after another re-attach: expected GREEN). Frames: shot-N30-D47-<set>-<n>.png (the first 2 passes of each set).
//   LAVA (G11, lava47; F-cards 6b): for each lava field of the level (the map's `lava` rects): the player walked across it at 1.8 m over the
//     surface, 8 m before its centre to 8 m past, 2 m/s of reel frames (one 0.25 m step a frame), twice: lava47 off then on. Per frame: the
//     enabled lights and the light at a fixed probe (the field's centre, 0.3 m over the surface: sum over enabled point lights of
//     intensity x (1 - (d / range)^2)^2), its largest frame-to-frame jump (share of the larger) and the frames with a jump over 15 %.
//     Frames: shot-N30-L47-<field>-<off|on>-<a|b>.png (the walk's middle and its end).
// GREEN when: door "on" has 0 magenta passes and 0 stale frames, door "off" shows the bug (> 0: the RED is reproduced) and lava "on" has 0
// frames with a jump over 15 % on every field (span / spire have no lava: "no lava fields" is reported, not a RED).
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
        const int Passes47 = 10; // passes in the RED sets (fresh, off after a re-attach)
        const int DoorPassesOn47 = 40; // passes in the GREEN set (door47 on): 40 a level, 200 across levels 1-5 (G10's done-when)

        static partial void N47Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Look47Scenario(g, dir, res, offs, pre);

        /// <summary>The end of this frame's picture: saved (when `name` is set) and its magenta pixels on a 64 x 36 grid counted.</summary>
        static IEnumerator Capture47(string dir, string name, int[] magenta, float[] lum = null) => Capture47(dir, name, magenta, lum, false);

        /// <summary>As Capture47, and with `gray` a greyscale copy beside it (shot-N30-&lt;name&gt;-gray.png: Rec. 709 luminance; the silhouette sheet's read,
        /// DESIGN-RULES E-3: a shape must read without its colour).</summary>
        static IEnumerator Capture47(string dir, string name, int[] magenta, float[] lum, bool gray)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                int m = 0;
                double sum = 0;
                for (int y = 0; y < 36; y++)
                    for (int x = 0; x < 64; x++)
                    {
                        Color c = t.GetPixel((x * t.width) / 64 + 1, (y * t.height) / 36 + 1);
                        if (c.r > 0.8f && c.g < 0.3f && c.b > 0.8f) m++;
                        sum += 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                    }
                magenta[0] = m;
                if (lum != null) lum[0] = (float)(sum / (64 * 36)); // the frame's mean luminance (Rec. 709 weights, on the gamma values)
                if (lum != null && lum.Length >= 3)
                {
                    // n47-r1b (NIGHT47.md: "never covers the reticle's centre box (10 %), never fills the screen"): lum[1] the centre box's mean
                    // (the middle 10 % of the width and height, 16 x 16 samples), lum[2] the share of the 64 x 36 samples brighter than 0.85
                    double cs = 0;
                    for (int y = 0; y < 16; y++)
                        for (int x = 0; x < 16; x++)
                        {
                            Color c = t.GetPixel((int)(t.width * (0.45f + 0.1f * (x + 0.5f) / 16f)), (int)(t.height * (0.45f + 0.1f * (y + 0.5f) / 16f)));
                            cs += 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                        }
                    int bright = 0;
                    for (int y = 0; y < 36; y++)
                        for (int x = 0; x < 64; x++)
                        {
                            Color c = t.GetPixel((x * t.width) / 64 + 1, (y * t.height) / 36 + 1);
                            if (0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b > 0.85) bright++;
                        }
                    lum[1] = (float)(cs / 256);
                    lum[2] = bright / (64f * 36f);
                }
                if (!string.IsNullOrEmpty(name))
                {
                    System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + name + ".png"), t.EncodeToPNG());
                    if (gray)
                    {
                        Color32[] px = t.GetPixels32();
                        for (int i = 0; i < px.Length; i++)
                        {
                            byte l = (byte)Mathf.Clamp(Mathf.RoundToInt(0.2126f * px[i].r + 0.7152f * px[i].g + 0.0722f * px[i].b), 0, 255);
                            px[i] = new Color32(l, l, l, 255);
                        }
                        t.SetPixels32(px);
                        System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + name + "-gray.png"), t.EncodeToPNG());
                    }
                }
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        static IEnumerator Look47Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            AutopilotInput ap = N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            bool? doorWas = Doors38View.Door47Force, lavaWas = Lava47.Force;
            offs.Add(() =>
            {
                Doors38View.Door47Force = doorWas;
                Lava47.Force = lavaWas;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
            });
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED a847: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 400;
            while ((sim.campaign == null || sim.campaign.room < 0 || sim.campaign.phase == Campaign24.Phase.None) && Time.realtimeSinceStartup < wallEnd - 380)
            {
                AutopilotInput.TakeDraft(sim);
                yield return null;
            }
            yield return Frames(20);
            DoorFree41(g);
            g.husks.frozen = true;
            God(g);
            string pc = Preconditions(g, g.Feet);
            Start(g, "a847-look47", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            int[] mag = new int[1];

            // ---- DOOR ----
            int[] magPasses = new int[3], stale = new int[3], passes = new int[3];
            string[] sets = { "fresh", "off", "on" };
            float doorWait = Time.realtimeSinceStartup + 5;
            while (!Doors38View.ReelOpenFound && Time.realtimeSinceStartup < doorWait) { God(g); yield return null; }
            if (!Doors38View.ReelOpenFound) lines.Add("DOOR: no open curtain drawn on this level (doors38 / doors41On off, or no campaign doors): not measured");
            else
            {
                Vector3 c0 = Doors38View.ReelOpenCenter;
                bool spansX = Doors38View.ReelOpenSpansX;
                Vector3 along = spansX ? Vector3.forward : Vector3.right; // the passage's axis (Doors38View: spansX = the curtain runs along x)
                for (int s = 0; s < 3; s++)
                {
                    Doors38View.Door47Force = s == 2;
                    if (s >= 1) { Doors38View.ReelReattach(); yield return Frames(3); }
                    int stale0 = Doors38View.StaleVeilFrames;
                    int count = s == 2 ? DoorPassesOn47 : Passes47; // the GREEN set: 40 a level (G10: 200 across levels 1-5)
                    for (int p = 0; p < count; p++)
                    {
                        // the speed of this pass (the camera carried across the plane each frame at it): sprint / slide / dash, in turn
                        int kind = p % 3;
                        float speed = kind == 0 ? UnityTuning.Door47SprintMps : kind == 1 ? UnityTuning.Door47SlideMps : UnityTuning.Door47DashMps;
                        float eyeDrop = kind == 1 ? UnityTuning.Door47SlideDrop : 0f; // a slide passes low
                        Vector3 a = c0 - along * 2.5f, b = c0 + along * 2.5f;
                        if ((p / 3) % 2 == 1) { Vector3 tmp = a; a = b; b = tmp; }
                        float feetY = WebSpace.ToUnity(g.Feet).y - eyeDrop;
                        double yaw = Math.Atan2(-(b.x - a.x), (b.z - a.z)) * 180 / Math.PI; // web yaw of the Unity direction a -> b (z flips)
                        float dist = Vector3.Distance(a, b), walked = 0f;
                        bool crossedShot = false;
                        while (walked < dist)
                        {
                            Vector3 at = Vector3.Lerp(a, b, walked / dist);
                            g.Respawn(new Vec3(at.x, feetY, -at.z), yaw); // Unity -> web (z flips: WebSpace.ToUnity's inverse)
                            God(g);
                            yield return null;
                            walked += speed * Mathf.Max(1f / 240f, Time.unscaledDeltaTime);
                            if (!crossedShot && walked > dist * 0.5f + speed * 0.034f) // 2 frames past the plane: inside the shimmer
                            {
                                crossedShot = true;
                                yield return Capture47(dir, p < 3 ? $"D47-{sets[s]}-{(kind == 0 ? "sprint" : kind == 1 ? "slide" : "dash")}" : null, mag);
                                if (mag[0] > 0) magPasses[s]++;
                            }
                        }
                        passes[s]++;
                        yield return Frames(20); // the shimmer (0.28 s) ends
                    }
                    stale[s] = Doors38View.StaleVeilFrames - stale0;
                }
                lines.Add($"DOOR at ({c0.x.ToString("0.0", ci)}, {c0.y.ToString("0.0", ci)}, {c0.z.ToString("0.0", ci)}): " +
                          $"fresh (door47 off, no re-attach) magenta passes {magPasses[0]}/{passes[0]}, stale veil frames {stale[0]}; " +
                          $"off (after a re-attach) {magPasses[1]}/{passes[1]}, stale {stale[1]}; on (door47, after a re-attach) {magPasses[2]}/{passes[2]}, stale {stale[2]}; shimmers {Doors38View.Shimmers}");
            }
            bool doorOk = !Doors38View.ReelOpenFound || (magPasses[2] == 0 && stale[2] == 0);
            bool doorRed = Doors38View.ReelOpenFound && (magPasses[1] > 0 || stale[1] > 0);

            // ---- LAVA ----
            var fields = new List<(double x0, double z0, double x1, double z1, double y)>();
            if (g.map != null && g.map.json != null && g.map.json.TryGetValue("lava", out object lo) && lo is List<object> ll)
                foreach (object row in ll)
                    if (LavaView30Math.Rect(row, out double x0, out double z0, out double x1, out double z1, out double ly)) fields.Add((x0, z0, x1, z1, ly));
            int lavaBadOn = 0;
            if (fields.Count == 0) lines.Add("LAVA: no lava fields on this level");
            // The meter follows the PLAYER (the lead's n47-r0 read: a fixed probe at the field's centre missed the pop): per frame the modelled light
            // on 9 floor points (the feet + a ring of 8 at Lava47RingProbeM: what the camera sees lit), the screen's mean luminance (a 64 x 36 grid of
            // the frame), and the enabled light count; at three speeds (a walk, a sprint, a slide past the lava). A jump: > 15 % between two frames.
            int lavaRedOff = 0;
            float[] lum = new float[1];
            Vector3[] ring = new Vector3[9];
            for (int fi = 0; fi < fields.Count && Time.realtimeSinceStartup < wallEnd; fi++)
            {
                var f = fields[fi];
                double cx = (f.x0 + f.x1) / 2, cz = (f.z0 + f.z1) / 2;
                bool alongX = f.x1 - f.x0 >= f.z1 - f.z0;
                double half = Math.Max(8, (alongX ? f.x1 - f.x0 : f.z1 - f.z0) / 2 + 4);
                var sb = new System.Text.StringBuilder($"LAVA field {fi} ({(f.x1 - f.x0).ToString("0", ci)} x {(f.z1 - f.z0).ToString("0", ci)} m):");
                for (int pass = 0; pass < 2; pass++)
                {
                    Lava47.Force = pass == 1;
                    yield return Frames(30); // the lights eased to the new rule
                    int jumpsAll = 0;
                    foreach (float mps in UnityTuning.Lava47WalkMps)
                    {
                        double prevL = -1, prevS = -1, maxJ = 0, maxS = 0;
                        var curC = new Dictionary<Light, double>();
                        var prevC = new Dictionary<Light, double>();
                        var culprits = new List<string>();
                        int jL = 0, jS = 0, minLights = int.MaxValue, maxLights = 0, prevLit = -1, maxStep = 0, n = 0;
                        bool shotA = false;
                        for (double s = -half; s <= half; )
                        {
                            Vec3 at = alongX ? new Vec3(cx + s, f.y + 1.8, cz) : new Vec3(cx, f.y + 1.8, cz + s);
                            g.Respawn(at, alongX ? -90 : 0);
                            God(g);
                            yield return null;
                            s += mps * Mathf.Max(1f / 240f, Time.unscaledDeltaTime);
                            Vector3 feet = WebSpace.ToUnity(new Vec3(at.x, f.y + 0.1, at.z));
                            ring[0] = feet;
                            for (int k = 0; k < 8; k++) ring[k + 1] = feet + Quaternion.Euler(0, k * 45f, 0) * Vector3.forward * UnityTuning.Lava47RingProbeM;
                            double L = 0;
                            int lit = 0;
                            curC.Clear();
                            foreach (Light l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                            {
                                if (!l.isActiveAndEnabled || l.intensity <= 0f) continue;
                                lit++;
                                if (l.type != LightType.Point && l.type != LightType.Spot) continue;
                                double cl = 0;
                                foreach (Vector3 probe in ring)
                                {
                                    float d = Vector3.Distance(l.transform.position, probe);
                                    if (d >= l.range) continue;
                                    float q = 1f - (d / l.range) * (d / l.range);
                                    cl += l.intensity * q * q;
                                }
                                L += cl;
                                if (cl > 0) curC[l] = cl;
                            }
                            // the culprit of a jump: the light whose share changed most since the last frame (the lead's ask)
                            string culprit = "";
                            double worst = 0;
                            foreach (var kv in curC) { prevC.TryGetValue(kv.Key, out double was); double dd = Math.Abs(kv.Value - was); if (dd > worst) { worst = dd; culprit = kv.Key.name; } }
                            foreach (var kv in prevC) if (!curC.ContainsKey(kv.Key) && kv.Value > worst) { worst = kv.Value; culprit = kv.Key != null ? kv.Key.name + " (gone)" : "(destroyed)"; }
                            prevC.Clear();
                            foreach (var kv in curC) prevC[kv.Key] = kv.Value;
                            minLights = Math.Min(minLights, lit);
                            maxLights = Math.Max(maxLights, lit);
                            if (prevLit >= 0) maxStep = Math.Max(maxStep, Math.Abs(lit - prevLit));
                            prevLit = lit;
                            if (prevL >= 0)
                            {
                                double big = Math.Max(prevL, L);
                                double j = big > 1e-4 ? Math.Abs(L - prevL) / big : 0;
                                maxJ = Math.Max(maxJ, j);
                                if (j > 0.15) { jL++; if (culprits.Count < 6 && !culprits.Contains(culprit)) culprits.Add(culprit); }
                            }
                            prevL = L;
                            if (mps >= 8f) // the screen read (a frame capture each frame: the fast passes only, as they are short)
                            {
                                string nm = !shotA && s >= 0 ? $"L47-{fi}-{(pass == 1 ? "on" : "off")}-{mps.ToString("0", ci)}" : null;
                                if (nm != null) shotA = true;
                                yield return Capture47(dir, nm, mag, lum);
                                if (prevS >= 0)
                                {
                                    double big = Math.Max(prevS, lum[0]);
                                    double j = big > 1e-4 ? Math.Abs(lum[0] - prevS) / big : 0;
                                    maxS = Math.Max(maxS, j);
                                    if (j > 0.15) jS++;
                                }
                                prevS = lum[0];
                            }
                            n++;
                        }
                        jumpsAll += jL + jS;
                        sb.Append($" {(pass == 1 ? "ON" : "OFF")} {mps.ToString("0", ci)} m/s: {n} frames, lights {minLights}-{maxLights} (largest step {maxStep}), floor-light jumps > 15 % {jL} (largest {(maxJ * 100).ToString("0", ci)} %{(culprits.Count > 0 ? ", from: " + string.Join(" / ", culprits) : "")})" +
                                  (mps >= 8f ? $", screen jumps > 15 % {jS} (largest {(maxS * 100).ToString("0", ci)} %);" : ";"));
                    }
                    if (pass == 0 && jumpsAll > 0) lavaRedOff++;
                    if (pass == 1 && jumpsAll > 0) lavaBadOn++;
                }
                lines.Add(sb.ToString());
            }
            lines.Add($"lava RED reproduced (off: a jump on {lavaRedOff} of {fields.Count} fields)");
            lines.Add($"lava47 lights built {Lava47.Built}, lit at the end {Lava47.LitNow}, built late {Lava47.CreatedLate}");

            // ---- FOES (G12-G14: visor47, foes47): a lineup of every small and visored kind, stood across the view, frozen; the camera at 10 / 20 /
            // 30 m; visor47 + foes47 forced off then on. Frames shot-N30-F47-<off|on>-<d>m.png (the lead's grayscale sheet is a post step) ----
            if (Time.realtimeSinceStartup < wallEnd)
            {
                bool? vWas = Visor47.Force, fWas = Foes47.Force;
                offs.Add(() => { Visor47.Force = vWas; Foes47.Force = fWas; });
                Vec3 at0 = g.Feet;
                double yaw0 = g.input != null ? g.input.yaw : 0;
                double fx = -Math.Sin(yaw0), fz = -Math.Cos(yaw0), sx = Math.Cos(yaw0), sz = -Math.Sin(yaw0);
                string[] kinds = { "husk", "swarmer", "spitter", "charger", "warden", "drone" };
                var spawned = new List<string>();
                var lineup = new List<Husk>();
                for (int i = 0; i < kinds.Length; i++)
                {
                    double off = (i - (kinds.Length - 1) / 2.0) * 1.8;
                    Husk h = null;
                    try { h = sim.husks.Spawn(kinds[i], new Vec3(at0.x + fx * 10 + sx * off, at0.y, at0.z + fz * 10 + sz * off), at0); } catch (Exception) { }
                    if (h != null) { h.hp = h.maxHp = 1e6; lineup.Add(h); spawned.Add(kinds[i]); }
                }
                g.husks.frozen = true;
                var fs = new System.Text.StringBuilder($"FOES lineup ({string.Join(", ", spawned)}):");
                var visorPx = new float[2, lineup.Count];
                Visor47Probe.Recording = true;
                offs.Add(() => Visor47Probe.Recording = false);
                for (int pass = 0; pass < 2; pass++)
                {
                    Visor47.Force = Foes47.Force = pass == 1;
                    foreach (double d in new[] { 10.0, 20.0, 30.0 })
                    {
                        g.Respawn(new Vec3(at0.x - fx * (d - 10), at0.y, at0.z - fz * (d - 10)), yaw0 * 180 / Math.PI);
                        God(g);
                        yield return Frames(12);
                        yield return Capture47(dir, $"F47-{(pass == 1 ? "on" : "off")}-{d.ToString("0", ci)}m", mag, null, true); // + its greyscale copy (round 4)
                        // G13's meter (F-cards 5b): each kind's visor height on screen (its bounds projected, px at 1080p) at this distance
                        fs.Append($" {(pass == 1 ? "ON" : "off")} {d.ToString("0", ci)} m visor px:");
                        for (int i = 0; i < lineup.Count; i++)
                        {
                            Husk h = lineup[i];
                            if (h == null) continue;
                            Vector3 near = WebSpace.ToUnity(new Vec3(h.position.x, h.position.y + h.Height * 0.8, h.position.z));
                            float px = Visor47Probe.PxNear(g.cam, spawned[i], near);
                            fs.Append($" {spawned[i]} {(px < 0 ? "-" : px.ToString("0.0", ci))}");
                            if (d == 30.0) visorPx[pass, i] = px;
                        }
                        fs.Append(";");
                    }
                    fs.Append($" {(pass == 1 ? "ON" : "off")}: 3 frames, the shield's down share {Foes47.ShieldDownK.ToString("0.00", ci)}, overlaps the body {Foes47.ShieldOverlapsBody};");
                }
                Visor47Probe.Recording = false;
                // G13: at 30 m the ON visor stands at least as tall as the off one (F-cards 5b: the husk ~2.3 px today -> ~3.4 after)
                var g13 = new System.Text.StringBuilder(" || G13 visor at 30 m (off -> ON px):");
                int taller = 0, seen = 0;
                for (int i = 0; i < lineup.Count; i++)
                {
                    if (visorPx[0, i] < 0 || visorPx[1, i] < 0) { g13.Append($" {spawned[i]} not seen;"); continue; }
                    seen++;
                    if (visorPx[1, i] >= visorPx[0, i] - 0.05f) taller++;
                    g13.Append($" {spawned[i]} {visorPx[0, i].ToString("0.0", ci)} -> {visorPx[1, i].ToString("0.0", ci)};");
                }
                g13.Append($" {taller} of {seen} as tall or taller");
                fs.Append(g13);
                lines.Add(fs.ToString());
                foreach (Husk h in lineup) if (h != null) h.alive = false; // cleared (left at 0.0001 hp they died to the next thing that touched them)
            }
            yield return Stop();
            bool ok = doorOk && lavaBadOn == 0;
            res[0] = $"{(ok ? "GREEN" : "RED")} a847 look47 | level {sim.campaignLevel.id} | door RED reproduced {(doorRed ? "yes" : "no")} || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
