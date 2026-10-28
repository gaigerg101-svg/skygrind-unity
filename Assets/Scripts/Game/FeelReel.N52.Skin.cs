// NIGHT52 builder U: the a252 phase (FeelReel.N52.cs routes it here): skin52's proof (design.n52.skin52, Skin52.cs, Enemy.shader). Any campaign
// level (the warm floors are the hard case: kiln and foundry):
//   bash Tools/night/n47-reels.sh <tag> u2on,n47-kiln,a252                 (the build copy's config with design.n52.skin52 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> u2off,n47-kiln,a252,-skygrindOff52 (the row off: RED, NIGHT51's enemies)
// The CAMPAIGN on the booted map (L-062), DOORFREE, God, every enemy frozen, the player's input back and still. Round 2: the player steps back
// along the longest clear line through the start (ahead + behind) and turns along it; a HUSK (near 2.5 m: the pill's jelly read) then a CHARGER
// (near 6 m: at 2.5 m its shield plate fills the frame; its cube parts are the ink's gap sites) are held facing the camera near and far (20 m, or
// the line's length - 1.5 m). With the row built, each pose is a TRIPLE in the same Player: Skin52Probe.ForceOff (NIGHT51's surface, "off"),
// Skin52Probe.NoRamp (skin52 without its ramp: the rim cut and the ink, the fallback's look, "flat"), then skin52 ("on").
// Frames: shot-N30-S52-<kind>-<near|far>-<off|flat|on>.png (full) and -crop.png (the enemy's box nearest-neighbour to 640 px); not built: -off52.
// Printed per pose, inside the enemy's projected box (its middle 80 %): mean luma, the glow share (luma > Skin52ReelBright), and on the ON frame
// the SHADOW share (among the body's red-orange pixels, those whose luma is under Skin52ReelShadeRatio x the same pixel FLAT: the ramp alone);
// the bake. The config's design.n52.skin52.numbers.ramp 0 ships the fallback (no ramp: the two tones not judged). GREEN when skin52 is
// built and every check holds (at least 3; 1 with the ramp off): each kind's near pose shows two tones (Skin52ReelShadeMin..Max), the husk's near glow drops
// (the rim cut; the charger's rim is not judged: n52-r2 measured 0.004 -> 0.004, its plate fills the box), each far pose at >= Skin52ReelFarMinM
// keeps its mean luma >= Skin52ReelFarKeep x NIGHT51's (the ramp fades out from Skin52RampFullM to Skin52RampFlatM), the bake closed gap
// vertices (> 0), and no frame shows magenta (a missing shader).
// Round 4 (leftover 1, the scorch's hot core): per side (OFF, ON) a husk at ~5 m with 1 hp on fresh floor (the player turned 14 deg either side),
// one KESTREL shot, the frame 0.3 s after its death (shot-N30-S52-scorch-<side>.png, -crop.png): the core's mean luma over the empty floor's;
// GREEN also needs the ON core to rise >= Skin52ReelScorchMin and over the OFF one (EnemyViews.Scorch52.cs: glow 6, hot 2 s under skin52).
// Round 6: the core box is the core's own (Skin52ReelScorchCoreM, was 0.35 m: the blotch's dark ring and kiln's floor strip filled it); the line
// prints the mark's colour and whether it drew through its own material (n52-r9: the block's values never reached the draw: no core at all).
// Round 9 (n52-r12: the OFF core was drawn, a dark red; the ON own material drew none, EnemyViews.Scorch52.cs): the frame is taken when the new
// mark is Skin52ReelScorchAgeS old (game time) or Skin52ReelScorchWaitS after the kill, both sides; the line prints the core's drawn colour and the
// box's mean sRGB before and after. design.n52.skin52.numbers.scorchGlow 0 (the fallback, NIGHT51's mark) takes the scorch out of the verdict.
// Round 12 (the lead: K's T-111 cap, Chunks.KillWhiteCap, unmeasured): per side (OFF = ForceOff, ON = skin52) a husk at Skin52ReelWashM killed by
// one KESTREL shot, every render frame grabbed from before the kill to Skin52ReelWashFrames after it, NIGHT50's WASH box (Wash49) per frame: the
// rise over f-2, its peak and peak frame, both curves printed. The OFF side drops the cap where Chunks gates it on Skin52.Live (the lead's 50b54c99).
// Round 13 (the lead's option b, n52-r17): a THREAD, not in a252's verdict (T-111 stays open): the cap cuts the f6-f8 tail, the f3-f4 peak is
// another source; the line prints the peak and the f6-f8 tail per side and saves the box's surround for f0-f8 (S52-wash-<side>-f<k>-box).
// Round 5 (design.n52.look52, Look52.cs; one side per Player: the row is read when the map is built): the player stands 15 m from the level's first
// health pickup (a clear line found round it) and the frame S52-pickup15-<on|off52> (+ -crop) prints its on-screen size and label; then 8 m from
// grapple node 0 (S52-ring-<side>, + -crop; a level with nodes: n47-spire / n47-span), the rings drawn, the node cubes skipped, the rope's colour.
// With look52 on GREEN also needs: the plus >= Look52ReelPickupMinPx tall at 15 m, as wide as x Look52ReelPickupAspect, no label (or >= 16 px);
// the rope pale (chroma < 0.75 x the usable's) and >= 60 deg of hue from the danger token, and the node cubes gone where the level has nodes.
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
        static partial void N52Skin(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Skin52Scenario(g, dir, res, offs, pre);

        /// <summary>The enemy's box on screen (px, origin bottom-left) from its body's box projected by its 8 corners, shrunk by `inner` about its
        /// centre (the box's middle: the body's pixels, little background).</summary>
        static Rect Box52(Camera cam, Husk h, float inner)
        {
            double r = Math.Max(0.35, h.Radius), y0 = h.position.y, y1 = h.position.y + h.Height;
            float lx = float.MaxValue, hx = float.MinValue, ly = float.MaxValue, hy = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                var c = new Vec3(h.position.x + ((i & 1) == 0 ? -r : r), (i & 2) == 0 ? y0 : y1, h.position.z + ((i & 4) == 0 ? -r : r));
                Vector3 s = cam.WorldToScreenPoint(WebSpace.ToUnity(c));
                if (s.z <= 0) continue;
                lx = Mathf.Min(lx, s.x); hx = Mathf.Max(hx, s.x); ly = Mathf.Min(ly, s.y); hy = Mathf.Max(hy, s.y);
            }
            if (hx < lx) return new Rect(0, 0, 0, 0);
            float cx = (lx + hx) * 0.5f, cy = (ly + hy) * 0.5f, w = (hx - lx) * inner, hh = (hy - ly) * inner;
            return new Rect(cx - w * 0.5f, cy - hh * 0.5f, w, hh);
        }

        /// <summary>Inside `box` of the grab: mean luma (0..1), the share brighter than UnityTuning.Skin52ReelBright (the glow), and the left third's
        /// mean luma minus the right third's (the ramp's key comes from the camera's left: a lit side and a shadow side; NIGHT51's flat body ~0).</summary>
        static Vector3 Stats52(Color32[] px, int w, int h, Rect box)
        {
            if (px == null || box.width < 3 || box.height < 2) return new Vector3(-1, -1, -1);
            int x0 = Mathf.Clamp((int)box.xMin, 0, w - 1), x1 = Mathf.Clamp((int)box.xMax, 0, w - 1), y0 = Mathf.Clamp((int)box.yMin, 0, h - 1), y1 = Mathf.Clamp((int)box.yMax, 0, h - 1);
            int third = Math.Max(1, (x1 - x0 + 1) / 3), n = 0, bright = 0, nl = 0, nr = 0;
            double sum = 0, sl = 0, sr = 0;
            float cut = UnityTuning.Skin52ReelBright * 255f;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    Color32 c = px[y * w + x];
                    float l = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
                    sum += l;
                    n++;
                    if (l > cut) bright++;
                    if (x < x0 + third) { sl += l; nl++; }
                    else if (x > x1 - third) { sr += l; nr++; }
                }
            if (n == 0) return new Vector3(-1, -1, -1);
            double asym = (nl > 0 ? sl / nl : 0) - (nr > 0 ? sr / nr : 0);
            return new Vector3((float)(sum / n / 255.0), bright / (float)n, (float)(asym / 255.0));
        }

        /// <summary>Round 9: the box's mean colour (sRGB bytes; black for no frame or an empty box): the scorch core's hue is printed.</summary>
        static Color32 MeanRgb52(Color32[] px, int w, int h, Rect box)
        {
            if (px == null || box.width < 1 || box.height < 1) return new Color32(0, 0, 0, 255);
            int x0 = Mathf.Clamp((int)box.xMin, 0, w - 1), x1 = Mathf.Clamp((int)box.xMax, 0, w - 1), y0 = Mathf.Clamp((int)box.yMin, 0, h - 1), y1 = Mathf.Clamp((int)box.yMax, 0, h - 1);
            long r = 0, gg = 0, b = 0, n = 0;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    Color32 c = px[y * w + x];
                    r += c.r; gg += c.g; b += c.b; n++;
                }
            if (n == 0) return new Color32(0, 0, 0, 255);
            return new Color32((byte)(r / n), (byte)(gg / n), (byte)(b / n), 255);
        }

        static IEnumerator Skin52Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            var spawned = new List<Husk>();
            offs.Add(() =>
            {
                Skin52Probe.ForceOff = false;
                Skin52Probe.NoRamp = false;
                Skin52.Apply();
                foreach (Husk h in spawned) if (h != null) h.alive = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a252: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 420;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 400) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput); // the player's own input back (no bot walking him); nothing is pressed
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            sim.husks.KillAll(); // the room's own wave out of the frame (silently: no kill events): the reel's enemy alone
            bool built = Skin52Probe.Built;

            // round 2: the longest clear line through the feet (8 headings: ahead + behind, at 0.5 m and the eye), the player stepped back to its far
            // end (the floor checked under it) and turned along it: the far pose needs >= Skin52ReelFarMinM (kiln's first spot gave 9.2 m ahead)
            Vec3 feet0 = g.Feet;
            double bestLine = -1, bestYaw = 0, bestBack = 0;
            for (int i = 0; i < 8; i++)
            {
                double yaw = i * Math.PI / 4;
                var dv = new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
                double ahead = double.MaxValue, behind = double.MaxValue;
                foreach (double y in new[] { 0.5, 1.6 })
                {
                    var o = new Vec3(feet0.x, feet0.y + y, feet0.z);
                    ahead = Math.Min(ahead, g.world.RayIntersect(o, dv, out RayHit h1) ? h1.distance : 60);
                    behind = Math.Min(behind, g.world.RayIntersect(o, new Vec3(-dv.x, 0, -dv.z), out RayHit h2) ? h2.distance : 60);
                }
                double back = Math.Max(0, behind - 1.2);
                // the floor under the stepped-back spot (a drop or a hole: no step back)
                var bs = new Vec3(feet0.x - dv.x * back, feet0.y + 1, feet0.z - dv.z * back);
                if (back > 0 && !(g.world.RayIntersect(bs, new Vec3(0, -1, 0), out RayHit fl) && Math.Abs(fl.distance - 1) < 0.35)) back = 0;
                double line = ahead + back;
                if (line > bestLine) { bestLine = line; bestYaw = yaw; bestBack = back; }
            }
            var bdv = new Vec3(-Math.Sin(bestYaw), 0, -Math.Cos(bestYaw));
            Vec3 feet = new Vec3(feet0.x - bdv.x * bestBack, feet0.y, feet0.z - bdv.z * bestBack);
            g.Respawn(feet, bestYaw * 180 / Math.PI);
            for (int i = 0; i < 30; i++) { God(g); yield return null; }
            feet = g.Feet;
            string pc = Preconditions(g, feet);
            Start(g, "a252-skin52", "camera", dir);
            yield return Frames(pre);
            Vector3 fwdU = g.cam.transform.forward;
            fwdU.y = 0;
            fwdU = fwdU.sqrMagnitude > 1e-6f ? fwdU.normalized : Vector3.forward;
            Vec3 fwd = WebSpace.FromUnity(fwdU);
            double faceYaw = Math.Atan2(fwd.x, fwd.z); // a body with this yaw faces back along the camera's line (husk.js: forward (-sin, -cos))
            double farD = Math.Min(UnityTuning.Skin52RampFlatM - 4, bestLine - 1.5);
            var lines = new List<string> { $"skin52 built {(built ? "yes (pairs: ForceOff = NIGHT51's surface, then skin52)" : "NO (the row is off in this build or -skygrindOff52: NIGHT51's frames, the RED side)")}; the line {bestLine.ToString("0.0", ci)} m (stepped back {bestBack.ToString("0.0", ci)} m); far pose {farD.ToString("0.0", ci)} m; frame {Screen.width}x{Screen.height}" };
            // round 2: a third frame, "flat" (skin52 with the ramp off: the rim cut and the ink only, the fallback's look), so the ramp is measured
            // alone (ON against FLAT; ON against OFF mixed the rim cut into it)
            string[] sides = built ? new[] { "off", "flat", "on" } : new[] { "off52" };
            bool rampCfg = Skin52.RampConfigured;
            if (built && !rampCfg) lines.Add("the ramp is off in this build's config (design.n52.skin52.numbers.ramp 0: the fallback, the rim's near cut only): two tones not judged");
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2], mag = new int[1];
            int magentaFrames = 0, checksOk = 0, checks = 0;
            float k1080 = Screen.height / 1080f;
            var verdicts = new List<string>();

            // the kinds and their poses: the HUSK (the pill: the jelly read) near at 2.5 m; the CHARGER (its cube parts: the ink's gap sites) near at
            // 6 m (at 2.5 m its shield plate fills the box: n52-r2 measured no rim there either way, so its rim is not judged)
            foreach ((string kind, double nearD, bool rimJudged) in new[] { ("husk", 2.5, true), ("charger", 6.0, false) })
            {
                if (Time.realtimeSinceStartup > wallEnd) { lines.Add($"{kind}: the wall clock ran out"); break; }
                double[] dists = { Math.Min(nearD, bestLine - 1.5), farD };
                var at0 = new Vec3(feet.x + fwd.x * dists[0], feet.y, feet.z + fwd.z * dists[0]);
                Husk h = null;
                sim.husks.KillAll(); // anything a wave put in since
                try { h = sim.husks.Spawn(kind, at0, feet); } catch (Exception) { }
                if (h == null) { lines.Add($"{kind}: did not spawn (its pool off or full)"); continue; }
                spawned.Add(h);
                void HoldAt(Vec3 p)
                {
                    h.position = p;
                    h.previous = p;
                    h.yaw = faceYaw;
                    h.hp = h.maxHp = 1e9;
                    h.alive = true;
                    God(g);
                }
                for (int i = 0; i < 90; i++) { HoldAt(at0); yield return null; } // its arrival's dissolve done
                var sb = new System.Text.StringBuilder($"{kind} (in its box: mean luma / glow share; on: the shadow share against off):");
                for (int di = 0; di < dists.Length; di++)
                {
                    double d = dists[di];
                    var at = new Vec3(feet.x + fwd.x * d, feet.y, feet.z + fwd.z * d);
                    Color32[] offPx = null, flatPx = null;
                    Vector3 offSt = default;
                    foreach (string side in sides)
                    {
                        Skin52Probe.ForceOff = side == "off";
                        Skin52Probe.NoRamp = side == "flat";
                        Skin52.Apply();
                        for (int i = 0; i < 8; i++) { HoldAt(at); yield return null; }
                        string name = $"S52-{kind}-{(di == 0 ? "near" : "far")}-{side}";
                        yield return Grab48(dir, frame, size, name);
                        yield return Capture47(dir, null, mag);
                        if (mag[0] > 0) magentaFrames++;
                        if (frame[0] == null) continue;
                        Rect box = Box52(g.cam, h, 0.8f);
                        Vector3 st = Stats52(frame[0], size[0], size[1], box);
                        Rect full = Box52(g.cam, h, 1.15f);
                        int crop = Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(full.width, full.height)), 32, size[1]);
                        CropUp48(dir, name + "-crop", frame[0], size[0], size[1], full.center.x, full.center.y, crop, 640);
                        sb.Append($" {d.ToString("0.0", ci)} m {side}: {st.x.ToString("0.000", ci)} / {st.y.ToString("0.000", ci)}");
                        if (side == "off") { offPx = frame[0]; offSt = st; sb.Append(";"); continue; }
                        if (side == "flat") { flatPx = frame[0]; sb.Append(";"); continue; }
                        if (side != "on" || offPx == null || flatPx == null) { sb.Append(";"); continue; }
                        float shade = Shade52(offPx, flatPx, frame[0], size[0], size[1], box);
                        sb.Append($" / shadow {shade.ToString("0.000", ci)} (box {Mathf.RoundToInt(box.width / k1080)} x {Mathf.RoundToInt(box.height / k1080)} px at 1080p);");
                        if (di == 0 && rampCfg)
                        {
                            bool tones = shade >= UnityTuning.Skin52ReelShadeMin && shade <= UnityTuning.Skin52ReelShadeMax;
                            checks++; if (tones) checksOk++;
                            verdicts.Add($"{kind} two tones at {d.ToString("0.0", ci)} m {tones} (shadow {shade.ToString("0.000", ci)} of the body against the flat frame, want {UnityTuning.Skin52ReelShadeMin.ToString("0.00", ci)}-{UnityTuning.Skin52ReelShadeMax.ToString("0.00", ci)})");
                        }
                        if (di == 0)
                        {
                            if (rimJudged)
                            {
                                bool cut = st.y < offSt.y;
                                checks++; if (cut) checksOk++;
                                verdicts.Add($"{kind} rim cut at {d.ToString("0.0", ci)} m {cut} (glow {offSt.y.ToString("0.000", ci)} -> {st.y.ToString("0.000", ci)})");
                            }
                        }
                        else if (d >= UnityTuning.Skin52ReelFarMinM)
                        {
                            bool kept = offSt.x > 0 && st.x >= UnityTuning.Skin52ReelFarKeep * offSt.x;
                            checks++; if (kept) checksOk++;
                            verdicts.Add($"{kind} far read kept at {d.ToString("0.0", ci)} m {kept} (mean luma {offSt.x.ToString("0.000", ci)} -> {st.x.ToString("0.000", ci)}, want >= x{UnityTuning.Skin52ReelFarKeep.ToString("0.00", ci)})");
                        }
                        else verdicts.Add($"{kind} far pose {d.ToString("0.0", ci)} m is under {UnityTuning.Skin52ReelFarMinM.ToString("0", ci)} m (this level's line): not judged");
                    }
                }
                lines.Add(sb.ToString());
                h.alive = false;
                for (int i = 0; i < 10; i++) { God(g); yield return null; }
            }
            // ---- round 4 (leftover 1): the scorch mark's hot core, 0.3 s after a kill on open floor in view ----
            // per side (off: NIGHT51's glow and heat; on: skin52's), the player turned Skin52ReelScorchTurnDeg either side of the line (fresh floor
            // each), a reference frame of the empty floor, a husk at ~5 m with 1 hp, one KESTREL shot, the frame 0.3 s after its death
            double dS = Math.Min(5.0, bestLine - 1.5);
            float coreOff = float.NaN, coreOn = float.NaN;
            var scorchNotes = new List<string>();
            int si = 0;
            foreach (string side in sides)
            {
                if (side == "flat" || Time.realtimeSinceStartup > wallEnd) continue;
                Skin52Probe.ForceOff = side == "off";
                Skin52Probe.NoRamp = false;
                Skin52.Apply();
                double ang = (si++ == 0 ? -1 : 1) * UnityTuning.Skin52ReelScorchTurnDeg * Math.PI / 180;
                var hd = new Vec3(fwd.x * Math.Cos(ang) + fwd.z * Math.Sin(ang), 0, -fwd.x * Math.Sin(ang) + fwd.z * Math.Cos(ang));
                g.Respawn(feet, Math.Atan2(-hd.x, -hd.z) * 180 / Math.PI);
                sim.husks.KillAll();
                for (int i = 0; i < 25; i++) { God(g); yield return null; }
                var spot = new Vec3(feet.x + hd.x * dS, feet.y, feet.z + hd.z * dS);
                yield return Grab48(dir, frame, size, null);
                Color32[] refPx = frame[0];
                Husk hs = null;
                try { hs = sim.husks.Spawn("husk", spot, feet); } catch (Exception) { }
                if (hs == null) { scorchNotes.Add($"{side}: the husk did not spawn"); continue; }
                spawned.Add(hs);
                double face = Math.Atan2(hd.x, hd.z);
                for (int i = 0; i < 60; i++) { hs.position = hs.previous = spot; hs.yaw = face; hs.hp = hs.maxHp = 1; hs.alive = true; God(g); yield return null; }
                int made0 = EnemyLook.Scorch51Made, drawn0 = EnemyLook.Scorch52Drawn;
                for (int i = 0; i < 90 && hs.alive; i++)
                {
                    g.forcedCombat = i % 30 == 0 ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                    if (hs.alive) { hs.position = hs.previous = spot; hs.yaw = face; }
                    God(g);
                    yield return null;
                }
                g.forcedCombat = IdleCombat;
                if (hs.alive) { scorchNotes.Add($"{side}: no kill in 1.5 s (the shot missed)"); hs.alive = false; continue; }
                float tKill = Time.unscaledTime;
                // round 9: the frame when the new mark is Skin52ReelScorchAgeS old (game time; both sides past the kill's pop light), or the cap
                while (EnemyLook.Scorch52Age < UnityTuning.Skin52ReelScorchAgeS && Time.unscaledTime - tKill < UnityTuning.Skin52ReelScorchWaitS) { God(g); yield return null; }
                float age = EnemyLook.Scorch52Age, heat = EnemyLook.Scorch52Heat;
                Color drawnCore = EnemyLook.Scorch52Core;
                string sname = $"S52-scorch-{side}";
                yield return Grab48(dir, frame, size, sname);
                Vector3 sp = g.cam.WorldToScreenPoint(WebSpace.ToUnity(spot));
                double coreM = UnityTuning.Skin52ReelScorchCoreM; // round 6: the core's own radius (0.35 m took in the blotch's dark ring and the floor's strip)
                Vector3 sr = g.cam.WorldToScreenPoint(WebSpace.ToUnity(new Vec3(spot.x + hd.z * coreM, spot.y, spot.z - hd.x * coreM)));
                float rpx = Mathf.Max(4f, Mathf.Abs(sr.x - sp.x));
                var core = new Rect(sp.x - rpx, sp.y - rpx * 0.5f, 2 * rpx, rpx);
                Vector3 a0 = Stats52(refPx, size[0], size[1], core), a1 = Stats52(frame[0], size[0], size[1], core);
                float rise = a0.x >= 0 && a1.x >= 0 ? a1.x - a0.x : float.NaN;
                if (frame[0] != null) CropUp48(dir, sname + "-crop", frame[0], size[0], size[1], sp.x, sp.y + rpx, Mathf.Clamp(Mathf.RoundToInt(rpx * 8), 32, size[1]), 640);
                Color mt = EnemyLook.Scorch52Tint;
                Color32 rgb0 = MeanRgb52(refPx, size[0], size[1], core), rgb1 = MeanRgb52(frame[0], size[0], size[1], core);
                scorchNotes.Add($"{side}: marks made {EnemyLook.Scorch51Made - made0}, the youngest {age.ToString("0.00", ci)} s old at heat {heat.ToString("0.00", ci)}, its colour (linear) {mt.r.ToString("0.00", ci)} / {mt.g.ToString("0.00", ci)} / {mt.b.ToString("0.00", ci)}, the core drawn in (linear) {drawnCore.r.ToString("0.00", ci)} / {drawnCore.g.ToString("0.00", ci)} / {drawnCore.b.ToString("0.00", ci)}, skin52's core on its block {EnemyLook.Scorch52Drawn > drawn0}; the core ({Mathf.RoundToInt(core.width / k1080)} x {Mathf.RoundToInt(core.height / k1080)} px at 1080p) mean luma {a0.x.ToString("0.000", ci)} -> {a1.x.ToString("0.000", ci)} (rise {rise.ToString("0.000", ci)}), mean sRGB {rgb0.r} / {rgb0.g} / {rgb0.b} -> {rgb1.r} / {rgb1.g} / {rgb1.b}, glow share {a1.y.ToString("0.000", ci)}");
                if (side == "on") coreOn = rise; else coreOff = rise;
            }
            if (built && Skin52.ScorchOn)
            {
                bool hot = !float.IsNaN(coreOn) && coreOn >= UnityTuning.Skin52ReelScorchMin && (float.IsNaN(coreOff) || coreOn > coreOff);
                checks++; if (hot) checksOk++;
                verdicts.Add($"scorch core reads {hot} (rise {coreOff.ToString("0.000", ci)} -> {coreOn.ToString("0.000", ci)} at {UnityTuning.Skin52ReelScorchAgeS.ToString("0.0", ci)} s, want >= {UnityTuning.Skin52ReelScorchMin.ToString("0.00", ci)} and over NIGHT51's)");
            }
            else if (built)
            {
                scorchNotes.Add("scorchGlow 0: the fallback ships (NIGHT51's mark under skin52), the core not judged");
                verdicts.Add("scorch as NIGHT51's (scorchGlow 0: not judged)");
            }
            lines.Add($"scorch (frames S52-scorch-<side>): {string.Join("; ", scorchNotes)}");

            // ---- round 12 (the lead; T-111, K's Chunks.KillWhiteCap: a gun kill's white hold capped with fx49 AND skin52): the kill's wash ----
            // per side (off: the reel's ForceOff, no cap; on: skin52, the cap) a husk at Skin52ReelWashM (past KillFarWhiteFrom, the far-white
            // case) killed by one KESTREL shot; every render frame grabbed from two before the kill to Skin52ReelWashFrames after it; NIGHT50's WASH
            // box (Wash49: the middle 20 %, 384 x 216 px at 1080p) per frame, its rise over f-2 (the frame two before the kill's first frame)
            var washPeak = new Dictionary<string, float>();
            var washTail = new Dictionary<string, float>();
            var washNotes = new List<string>();
            var washGrid = new float[64 * 36];
            int wi = 0;
            yield return Equip27(g, "revolver"); // the KESTREL
            g.forcedCombat = IdleCombat;
            foreach (string side in sides)
            {
                if (side == "flat" || Time.realtimeSinceStartup > wallEnd) continue;
                Skin52Probe.ForceOff = side == "off";
                Skin52Probe.NoRamp = false;
                Skin52.Apply();
                double ang = (wi++ == 0 ? -1 : 1) * UnityTuning.Skin52ReelWashTurnDeg * Math.PI / 180;
                var hd = new Vec3(fwd.x * Math.Cos(ang) + fwd.z * Math.Sin(ang), 0, -fwd.x * Math.Sin(ang) + fwd.z * Math.Cos(ang));
                g.Respawn(feet, Math.Atan2(-hd.x, -hd.z) * 180 / Math.PI);
                sim.husks.KillAll();
                for (int i = 0; i < 30; i++) { God(g); yield return null; }
                double dW = Math.Min(UnityTuning.Skin52ReelWashM, bestLine - 1.0);
                var spot = new Vec3(feet.x + hd.x * dW, feet.y, feet.z + hd.z * dW);
                Husk hs = null;
                try { hs = sim.husks.Spawn("husk", spot, feet); } catch (Exception) { }
                if (hs == null) { washNotes.Add($"{side}: the husk did not spawn"); continue; }
                spawned.Add(hs);
                double face = Math.Atan2(hd.x, hd.z);
                for (int i = 0; i < 45; i++) { hs.position = hs.previous = spot; hs.yaw = face; hs.hp = hs.maxHp = 1; hs.alive = true; God(g); yield return null; }
                var wash = new List<float>();
                int f0 = -1;
                for (int i = 0; i < 150; i++)
                {
                    int k = f0 >= 0 ? i - f0 : -1;
                    if (f0 >= 0 && k > UnityTuning.Skin52ReelWashFrames) break;
                    // one press two frames into the capture (again every 30 frames if it missed)
                    g.forcedCombat = f0 < 0 && i % 30 == 2 ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                    if (hs.alive) { hs.position = hs.previous = spot; hs.yaw = face; }
                    God(g);
                    string shot = k == UnityTuning.Skin52ReelWashShotK ? $"S52-wash-{side}-f{k}" : null; // this grab is the kill's frame k
                    yield return Grab48(dir, frame, size, shot);
                    wash.Add(frame[0] != null ? Wash49(frame[0], size[0], size[1], washGrid) : float.NaN);
                    // round 13 (T-111's thread): the wash box's surround for each kill frame up to f8 (S52-wash-<side>-f<k>-box, 0.4 of the height square)
                    if (f0 >= 0 && k >= 0 && k <= 8 && frame[0] != null)
                        CropUp48(dir, $"S52-wash-{side}-f{k}-box", frame[0], size[0], size[1], size[0] * 0.5f, size[1] * 0.5f, (int)(size[1] * 0.4f), 432);
                    if (f0 < 0 && !hs.alive) f0 = i; // the first frame drawn with the husk dead (its kill's frame 0)
                }
                g.forcedCombat = IdleCombat;
                if (f0 < 2) { washNotes.Add($"{side}: no kill in the capture (the shot missed)"); hs.alive = false; continue; }
                float w0 = wash[f0 - 2];
                float peak = float.MinValue;
                int peakK = -1;
                var curve = new System.Text.StringBuilder();
                for (int k = 0; f0 + k < wash.Count; k++)
                {
                    float r = wash[f0 + k] - w0;
                    curve.Append($"{(k == 0 ? "" : " ")}f{k} {r.ToString("+0.000;-0.000;0.000", ci)}");
                    if (r > peak) { peak = r; peakK = k; }
                }
                washPeak[side] = peak;
                float tail = float.MinValue; // K's T-111 frames: f6-f8 (the white hold through the freeze and the flight)
                for (int k = 6; k <= 8 && f0 + k < wash.Count; k++) tail = Mathf.Max(tail, wash[f0 + k] - w0);
                washTail[side] = tail;
                washNotes.Add($"{side} (skin52 live {Skin52.Live}): kill at {dW.ToString("0.0", ci)} m, f-2 {w0.ToString("0.000", ci)}, peak {peak.ToString("+0.000;-0.000;0.000", ci)} at f{peakK}, f6-f8 max {tail.ToString("+0.000;-0.000;0.000", ci)} [{curve}]");
            }
            Skin52Probe.ForceOff = false;
            Skin52.Apply();
            if (built)
            {
                // round 13 (the lead's option b): a THREAD, not judged in a252's count. n52-r17: the cap cut the f6-f8 tail (kiln +0.223 / +0.217 /
                // +0.213 -> +0.135 / +0.103 / +0.094; spire +0.288 / +0.289 / +0.232 -> +0.174 / +0.118 / +0.130), but the peak sits on f3-f4 (kiln
                // +0.310 -> +0.314, spire +0.424 -> +0.378; NIGHT51's off52 runs +0.319 / +0.424): a second source during the kill burst's frames
                // (its HDR 2.0 streaks full on frames 1-2, fading 3-4; the white body's first frames; their bloom: which, not checked). T-111 stays open.
                bool washHas = washPeak.TryGetValue("on", out float wOn) & washPeak.TryGetValue("off", out float wOff);
                bool tailHas = washTail.TryGetValue("on", out float tOn) & washTail.TryGetValue("off", out float tOff);
                string F(bool has, float v) => has ? v.ToString("+0.000;-0.000;0.000", ci) : "n/a";
                washNotes.Add($"T-111 (a thread, not judged): peak {F(washHas, wOff)} -> {F(washHas, wOn)} (the wash-box target <= +{Wash49Rise.ToString("0.00", ci)}: {(washHas && wOn <= Wash49Rise ? "met" : "NOT met")}); "
                    + $"the f6-f8 tail {F(tailHas, tOff)} -> {F(tailHas, tOn)} (the cap's own frames: {(tailHas && tOn < tOff ? "lower" : "not lower")})");
            }
            lines.Add($"kill wash (T-111; the KESTREL at {UnityTuning.Skin52ReelWashM.ToString("0.0", ci)} m; the WASH box's rise over f-2 per frame; frames S52-wash-<side>-f{UnityTuning.Skin52ReelWashShotK}; the cap follows the reel's ForceOff only where Chunks reads Skin52.Live): {string.Join("; ", washNotes)}");

            // ---- round 16 (hud52, NIGHT52.md leftover 5; UiTeach52.Verb.cs): a teach35 line waits for its verb, proven here where the KESTREL's
            // 1-frame presses fire (the kill-wash block above; a152's context never fired in n52-r19 / r20). Per side (OFF = Hud52Probe.ForceOff):
            // teach35's l1.fire line sent into the view's queue, held Teach52ReelVerbWaitS of HUD time, then a press every Teach52ReelTapEvery frames
            // until a "fire" is heard; GREEN (with hud52 built) when ON is still up at the wait, a fire is heard and the line is gone within
            // Teach52ReelVerbClearFrames, ended by the verb, and OFF is gone at NIGHT51's timer
            bool vOk = false, vOffOk = true, vJudged = Hud52Probe.Built;
            var vNotes = new List<string>();
            double vShow = g.cfg != null && g.cfg.Has("design.n35.teach35.showS") ? g.cfg.Num("design.n35.teach35.showS") : 5;
            foreach (string side in sides)
            {
                if (side == "flat" || Time.realtimeSinceStartup > wallEnd) continue;
                Hud52Probe.ForceOff = side == "off";
                for (int i = 0; i < 30; i++) { God(g); yield return null; }
                int shows0 = Ui.Teach35Shows;
                g.events.Emit("teach35", new Teach35Event { id = UnityTuning.Teach52ReelVerbId, text = UnityTuning.Teach52ReelVerbLine, trigger = "reel52", seconds = vShow });
                int waited = 0;
                while (Ui.Teach35Shows == shows0 && waited < 900) { God(g); waited++; yield return null; }
                if (Ui.Teach35Shows == shows0) { vNotes.Add($"{side}: the line never showed in 15 s"); continue; }
                int hold = (int)Math.Round(UnityTuning.Teach52ReelVerbWaitS * UnityTuning.ReelFps);
                for (int i = 0; i < hold; i++) { God(g); yield return null; }
                bool upAtWait = Ui.Teach35IdNow == UnityTuning.Teach52ReelVerbId;
                yield return Grab48(dir, frame, size, $"S52-verb-wait-{side}");
                int cleared0 = Verb52Probe.Cleared, fires0 = Verb52Probe.Fires, tries = 0;
                bool heard = false;
                for (int f = 0; f < 6 * UnityTuning.Teach52ReelTapEvery && !heard; f++)
                {
                    int ph = f % UnityTuning.Teach52ReelTapEvery;
                    if (ph == 0) tries++;
                    g.forcedCombat = ph < UnityTuning.Teach52ReelTapFrames ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = ph == 0 } } : IdleCombat;
                    God(g);
                    yield return null;
                    if (Verb52Probe.Fires > fires0 || (side != "on" && ph == UnityTuning.Teach52ReelTapFrames)) heard = true; // the OFF side's listener may be unwired
                }
                g.forcedCombat = IdleCombat;
                bool fired = Verb52Probe.Fires > fires0;
                int frames = 0;
                while (Ui.Teach35IdNow == UnityTuning.Teach52ReelVerbId && frames < 120) { God(g); frames++; yield return null; }
                bool gone = Ui.Teach35IdNow != UnityTuning.Teach52ReelVerbId;
                yield return Grab48(dir, frame, size, $"S52-verb-after-{side}");
                vNotes.Add($"{side}: up after {UnityTuning.Teach52ReelVerbWaitS.ToString("0", ci)} s {upAtWait}; presses {tries} (in hand {g.loadout.Weapon}), a fire heard {fired}; then gone {gone} in {frames} frames (cleared by the verb {Verb52Probe.Cleared > cleared0}); the last finished line held {Ui.Teach35LastHeldS.ToString("0.00", ci)} s");
                if (side == "on") vOk = upAtWait && fired && gone && frames <= UnityTuning.Teach52ReelVerbClearFrames && Verb52Probe.Cleared > cleared0;
                else if (side == "off") vOffOk = !upAtWait;
            }
            Hud52Probe.ForceOff = false;
            g.forcedCombat = IdleCombat;
            if (built && vJudged) { checks++; if (vOk && vOffOk) checksOk++; verdicts.Add($"teach line waits for its verb {vOk && vOffOk}"); }
            lines.Add($"verb wait (hud52 built {vJudged}; line '{UnityTuning.Teach52ReelVerbId}', NIGHT51's timer {vShow.ToString("0.0", ci)} s, the verb after {UnityTuning.Teach52ReelVerbWaitS.ToString("0", ci)} s; frames S52-verb-wait-<side>, S52-verb-after-<side>): {string.Join("; ", vNotes)}");

            // ---- round 5: look52 (Look52.cs): the health pickup at 15 m; the grapple's ring, node cubes and rope colours ----
            bool l52 = Look52.On(g.cfg);
            string l52Side = l52 ? "on" : "off52";
            sim.husks.KillAll();
            Pickup35View hp = null;
            foreach (Pickup35View pv in Pickup35View.All) if (pv != null && pv.Kind == "healthPickup" && pv.gameObject.activeInHierarchy) { hp = pv; break; }
            if (hp == null) lines.Add("look52 pickup: no health pickup in this level (run a252 on a level with one)");
            else if (Time.realtimeSinceStartup < wallEnd)
            {
                // a standing spot ~15 m from it with a clear line to it (16 headings; the floor found under each)
                Vector3 c0 = hp.Centre;
                Vector3 standU = Vector3.zero;
                bool found = false;
                for (int k = 0; k < 16 && !found; k++)
                {
                    float a = k * Mathf.PI / 8f;
                    var dU = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                    Vector3 cand = c0 + dU * UnityTuning.Look52ReelPickupM;
                    Vec3 top = WebSpace.FromUnity(new Vector3(cand.x, c0.y + 2.5f, cand.z));
                    if (!g.world.RayIntersect(top, new Vec3(0, -1, 0), out RayHit fl) || fl.distance > 8) continue;
                    Vector3 feetU = WebSpace.ToUnity(new Vec3(top.x, top.y - fl.distance, top.z));
                    Vector3 eyeU = feetU + Vector3.up * 1.6f;
                    Vector3 toP = c0 - eyeU;
                    float dist = toP.magnitude;
                    if (g.world.RayIntersect(WebSpace.FromUnity(eyeU), WebSpace.FromUnity(eyeU + toP.normalized) - WebSpace.FromUnity(eyeU), out RayHit los) && los.distance < dist - 0.6) continue;
                    standU = feetU;
                    found = true;
                }
                if (!found) lines.Add("look52 pickup: no clear standing spot 15 m from the health pickup in this level");
                else
                {
                    Vector3 face = c0 - standU;
                    face.y = 0;
                    Vec3 fw = WebSpace.FromUnity(face.normalized);
                    g.Respawn(WebSpace.FromUnity(standU), Math.Atan2(-fw.x, -fw.z) * 180 / Math.PI);
                    for (int i = 0; i < 40; i++) { God(g); yield return null; }
                    Rect pb = hp.ScreenBox52(g.cam);
                    float hPx = pb.height / k1080, wPx = pb.width / k1080;
                    float labelPx = hp.LabelPixelHeight(g.cam) / k1080;
                    float dReal = Vector3.Distance(g.cam.transform.position, hp.Centre);
                    yield return Grab48(dir, frame, size, "S52-pickup15-" + l52Side);
                    if (frame[0] != null && pb.width > 1) CropUp48(dir, "S52-pickup15-" + l52Side + "-crop", frame[0], size[0], size[1], pb.center.x, pb.center.y, Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(pb.width, pb.height) * 4f), 32, size[1]), 640);
                    bool reads = hp.Cross52 && hPx >= UnityTuning.Look52ReelPickupMinPx && wPx >= UnityTuning.Look52ReelPickupAspect * hPx && (hp.LabelAlpha <= 0f || labelPx >= 16f);
                    lines.Add($"look52 pickup ({l52Side}): {(hp.Cross52 ? "the plus" : "the vial (NIGHT51)")} at {dReal.ToString("0.0", ci)} m: {wPx.ToString("0", ci)} x {hPx.ToString("0", ci)} px at 1080p (want height >= {UnityTuning.Look52ReelPickupMinPx.ToString("0", ci)}, width >= x{UnityTuning.Look52ReelPickupAspect.ToString("0.0", ci)}); label '{hp.LabelText}' alpha {hp.LabelAlpha.ToString("0.00", ci)} {labelPx.ToString("0.0", ci)} px; reads as health {reads}");
                    if (l52) { checks++; if (reads) checksOk++; verdicts.Add($"health pickup reads at 15 m {reads}"); }
                }
            }
            // the grapple: the ring (GrappleNodes30) at ~8 m from a node, the node cubes skipped, the rope's colour (pale cyan, never red-orange)
            List<Vec3> gnodes = sim.map != null ? sim.map.grappleNodes : null;
            Color rope = Look52.RopeColor, usableC = Skygrind.Game.Look.C(g.cfg, "colors.usable");
            float ropeGap = HueGap52(Hud47Math.HueDeg(rope), Hud47Math.HueDeg(UiStyle33.Danger));
            bool ropeOk = ropeGap >= 60f && Chroma52(rope) < Chroma52(usableC) * 0.75f;
            string ringNote = "no grapple node in this level (run a252 on n47-spire or n47-span for the ring frame)";
            if (gnodes != null && gnodes.Count > 0 && Time.realtimeSinceStartup < wallEnd)
            {
                Vector3 nU = WebSpace.ToUnity(gnodes[0]);
                bool placed = false;
                for (int k = 0; k < 16 && !placed; k++)
                {
                    float a = k * Mathf.PI / 8f;
                    Vector3 cand = nU + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * 8f;
                    Vec3 top = WebSpace.FromUnity(new Vector3(cand.x, nU.y + 1f, cand.z));
                    if (!g.world.RayIntersect(top, new Vec3(0, -1, 0), out RayHit fl) || fl.distance > 14) continue;
                    Vector3 feetU = WebSpace.ToUnity(new Vec3(top.x, top.y - fl.distance, top.z));
                    Vector3 eyeU = feetU + Vector3.up * 1.6f, toN = nU - eyeU;
                    if (g.world.RayIntersect(WebSpace.FromUnity(eyeU), WebSpace.FromUnity(eyeU + toN.normalized) - WebSpace.FromUnity(eyeU), out RayHit los) && los.distance < toN.magnitude - 0.8) continue;
                    Vector3 face = toN;
                    face.y = 0;
                    Vec3 fw = WebSpace.FromUnity(face.normalized);
                    g.Respawn(WebSpace.FromUnity(feetU), Math.Atan2(-fw.x, -fw.z) * 180 / Math.PI);
                    placed = true;
                }
                if (placed)
                {
                    for (int i = 0; i < 40; i++) { God(g); yield return null; }
                    yield return Grab48(dir, frame, size, "S52-ring-" + l52Side);
                    Vector3 sn = g.cam.WorldToScreenPoint(nU);
                    if (frame[0] != null && sn.z > 0) CropUp48(dir, "S52-ring-" + l52Side + "-crop", frame[0], size[0], size[1], sn.x, sn.y, Mathf.RoundToInt(240 * k1080), 640);
                    ringNote = $"a ring frame at 8 m from node 0 (rings drawn {GrappleNodes30.RingsShown}, band {UnityTuning.Grapple31RingBand.ToString("0.000", ci)} m; on screen at ({sn.x.ToString("0", ci)}, {sn.y.ToString("0", ci)}))";
                }
                else ringNote = "no clear spot 8 m from node 0";
            }
            bool cubesOk = gnodes == null || gnodes.Count == 0 || Look52.NodeCubesSkipped > 0;
            lines.Add($"look52 grapple ({l52Side}): {ringNote}; node cubes skipped {Look52.NodeCubesSkipped}; rope {Col52(rope, ci)} (usable {Col52(usableC, ci)}; {ropeGap.ToString("0", ci)} deg from the danger hue: pale and not red-orange {ropeOk}); look52's wall lift on this map x{Look52Lift.Lift.ToString("0.00", ci)} (iceline / span only)");
            if (l52) { checks++; bool gOk = ropeOk && cubesOk; if (gOk) checksOk++; verdicts.Add($"grapple rope pale, node cubes gone {gOk}"); }
            Skin52Probe.ForceOff = false;
            Skin52Probe.NoRamp = false;
            Skin52.Apply();
            lines.Add($"bake: meshes {Skin52Probe.Meshes}, renderers {Skin52Probe.Renderers}, gap vertices closed {Skin52Probe.GapVerts}, skipped {Skin52Probe.Skipped} [{string.Join("; ", Skin52Probe.Notes)}]");
            lines.Add($"magenta frames {magentaFrames}");
            yield return Frames(5);
            yield return Stop();
            bool ok = built && checks >= (rampCfg ? 3 : 1) && checksOk == checks && Skin52Probe.GapVerts > 0 && magentaFrames == 0;
            res[0] = $"{(ok ? "GREEN" : "RED")} a252 skin52 | built {built}, checks {checksOk}/{checks} ({string.Join("; ", verdicts)}), gap verts {Skin52Probe.GapVerts}, magenta {magentaFrames} || {string.Join(" || ", lines)} | {pc}";
        }

        /// <summary>Among `box`'s BODY pixels (red-orange in the OFF frame: the enemy's colour, so the charger's plate, the floor and the background
        /// are left out), the share whose luma ON is under Skin52ReelShadeRatio x the same pixel in the FLAT frame (skin52 without its ramp): the
        /// ramp's shadow side alone. -1 when the box has fewer than 200 body pixels.</summary>
        static float Shade52(Color32[] mask, Color32[] flat, Color32[] on, int w, int h, Rect box)
        {
            if (mask == null || flat == null || on == null || mask.Length != on.Length || flat.Length != on.Length || box.width < 2 || box.height < 2) return -1f;
            int x0 = Mathf.Clamp((int)box.xMin, 0, w - 1), x1 = Mathf.Clamp((int)box.xMax, 0, w - 1), y0 = Mathf.Clamp((int)box.yMin, 0, h - 1), y1 = Mathf.Clamp((int)box.yMax, 0, h - 1);
            int n = 0, dark = 0;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    Color32 m = mask[y * w + x], a = flat[y * w + x], b = on[y * w + x];
                    if (m.r < 50 || m.r < UnityTuning.Skin52ReelBodyRG * m.g || m.r < UnityTuning.Skin52ReelBodyRB * m.b) continue; // not the body's red-orange
                    float la = 0.2126f * a.r + 0.7152f * a.g + 0.0722f * a.b, lb = 0.2126f * b.r + 0.7152f * b.g + 0.0722f * b.b;
                    n++;
                    if (lb < UnityTuning.Skin52ReelShadeRatio * la) dark++;
                }
            return n >= 200 ? dark / (float)n : -1f;
        }
    }
}
