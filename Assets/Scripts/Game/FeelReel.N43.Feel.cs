// NIGHT43 builder F1 (G19 view, G20; design.n43.feel43): the Player scenario `feel43`, the night's feel before / after in ONE Player (L-001):
// every moment is played twice, pass "off" (feel43 off: NIGHT42's picture; W1's fusion43 sim still on when it lands) then pass "on" (feel43
// forced on by the reel whatever the design row says), so the lead looks at the pair from one build. Not run by its author (no Player in the
// worktree). Boot: any level with an open floor (reel.sh + -skygrindMap <level> + -skygrindReelOnly feel43, L-062; level 1 or 2 is fine).
// The player stands on SafeSpot's open floor, God; the guns are added when the level lacks them; the trigger is a virtual pad's RT (Pad27).
// fusion43 is forced on for this run (sim.n43 and n43Override with fusion43 set: W1's row); the FUSION is a real one when W1's sim answers
// (the player carried at 16 m/s into a husk, the gauntlet swung within reach), else the reel emits a synthetic Fusion43Event at the husk so
// F1's picture is still framed (the line says "synthetic": W1's freeze is then absent).
// Moments, each framed at +0 / +0.05 / +0.15 s of the frame clock after its event (FeelClock.RealNow: inside a freeze when one is granted):
//   F43-<pass>-fusion-000 / -050 / -150   a FUSION on a 1e9-hp husk at ~2 m (real or synthetic)
//   F43-<pass>-slug-000 / -050 / -150     a THUNDERCLAP charged release (the slug when design.n31's slug is on) killing a 1-hp husk at 6 m
//   F43-<pass>-torch-300 / -1000          the TORCH held on a husk at 4 m, 0.3 s and 1.0 s into the hold (skipped when the TORCH never ticks)
// Measured per frame: the screen change against the frame before the moment's press (the mean |delta luma| on a 160 x 90 grid and the share of
// cells that moved > 0.1), so the pair's numbers say how much more the screen changes with feel43 on; the rumble words asked
// (Feel43Haptics.LogLine) and the sprites drawn (Feel43Probe.Line) per pass. GREEN when every moment happened, every frame was saved and pass on
// changed the screen more than pass off on the FUSION's and the slug's +0.05 frame.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N43Feel(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Look43View.ReelFlag ? Look43Scenario(g, dir, res, offs, pre) : Feel43Scenario(g, dir, res, offs, pre);

        // ======================================================= look43 (feel43 + -skygrindLook43) =======================================================
        // NIGHT43 F1 round 2 (design.n43.look43): boot a campaign level (reel.sh + -skygrindMap <level> + -skygrindReelOnly feel43 -skygrindLook43; level 1
        // for a3 / b1, level 3 for kb1). Per room (a3, b1, kb1 when the level has them; else its first two arenas): the player at the room's spawn facing
        // its way, God, the enemies frozen; frames L43-<room>-off (the kit hidden: NIGHT42's look), L43-<room>-on (the room fighting: its exit dim) and
        // L43-<room>-clear (the room drawn cleared: the exit bright, the next door lit), each with the frame's mean luma, mean saturation and the share
        // of cyan / warm pixels (a 160 x 90 grid), plus Look43Probe.Line (what was built, the old strips' pieces touching a ride panel). GREEN when every
        // room's three frames were saved and the kit built something.
        static readonly string[] Look43Rooms = { "a3", "b1", "kb1" };

        static IEnumerator Stats43(string dir, string name, double[] st)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            st[0] = -1;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N43-" + name + ".png"), t.EncodeToPNG());
                Color32[] px = t.GetPixels32();
                int w = t.width, h = t.height, n = 0, cyanN = 0, warmN = 0;
                double luma = 0, sat = 0;
                for (int gy = 0; gy < Grid43H; gy++)
                    for (int gx = 0; gx < Grid43W; gx++)
                    {
                        Color c = px[Mathf.Min(h - 1, (gy * h + h / 2) / Grid43H) * w + Mathf.Min(w - 1, (gx * w + w / 2) / Grid43W)];
                        Color.RGBToHSV(c, out float hu, out float s, out float v);
                        luma += 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                        sat += s;
                        n++;
                        if (s > 0.4f && v > 0.35f)
                        {
                            float deg = hu * 360;
                            if (deg >= 165 && deg <= 205) cyanN++;
                            else if (deg <= 45 || deg >= 345) warmN++;
                        }
                    }
                st[0] = luma / n;
                st[1] = sat / n;
                st[2] = cyanN / (double)n;
                st[3] = warmN / (double)n;
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        static IEnumerator Look43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            Look43View kit = Look43View.Instance;
            string pc = $"map {g.MapId}";
            if (kit == null) { res[0] = "RED look43: no Look43View on this map (not a campaign level, or -skygrindOff43) | " + pc; yield break; }
            bool wasOn = kit.On;
            offs.Add(() =>
            {
                Look43View.ForceCleared = null;
                kit.SetOn(wasOn);
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
            });
            LevelDef def = LevelDef.From(g.map.json);
            var rooms = new List<RoomDef>();
            foreach (string id in Look43Rooms)
                foreach (RoomDef r in def.rooms)
                    if (r.id == id) rooms.Add(r);
            if (rooms.Count == 0)
                foreach (RoomDef r in def.rooms)
                    if (r.kind == "room" && Math.Min(r.x1 - r.x0, r.z1 - r.z0) >= UnityTuning.Look43MarkMinRoom && rooms.Count < 2) rooms.Add(r);
            DoorFree41(g);
            g.forcedCombat = IdleCombat;
            Start(g, "look43", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            var st = new double[4];
            int saved = 0, wanted = 0;
            string Fmt(double[] s) => $"luma {s[0].ToString("0.000", ci)} sat {s[1].ToString("0.000", ci)} cyan {(s[2] * 100).ToString("0.0", ci)}% warm {(s[3] * 100).ToString("0.0", ci)}%";
            foreach (RoomDef r in rooms)
            {
                g.Respawn(r.spawn, r.spawnYawDeg);
                g.input.SetLook(r.spawnYawDeg * Math.PI / 180, 0);
                yield return Frames(20);
                g.husks.frozen = true;
                God(g);
                var sb = new System.Text.StringBuilder();
                sb.Append($"L43-{r.id}:");
                foreach (string pass in new[] { "off", "on", "clear" })
                {
                    wanted++;
                    kit.SetOn(pass != "off");
                    Look43View.ForceCleared = pass == "clear" ? true : pass == "on" ? false : (bool?)null;
                    God(g);
                    yield return Frames(4);
                    yield return Stats43(dir, $"L43-{r.id}-{pass}", st);
                    if (st[0] >= 0) saved++;
                    sb.Append(' ').Append(pass).Append(' ').Append(Fmt(st)).Append(';');
                }
                sb.Append(" | ").Append(Look43Probe.Line());
                lines.Add(sb.ToString());
            }
            Look43View.ForceCleared = null;
            yield return Stop();
            bool built = Look43Probe.RidePanels + Look43Probe.Doors + Look43Probe.Landmarks + Look43Probe.LavaPools > 0;
            bool ok = rooms.Count > 0 && saved == wanted && built;
            res[0] = $"{(ok ? "GREEN" : "RED")} look43 | rooms {rooms.Count}, frames {saved}/{wanted}, built {built} ||{string.Join(" || ", lines)} | {pc}";
        }

        const int Grid43W = 160, Grid43H = 90;

        /// <summary>The screen now, as a 160 x 90 luma grid (the frame's end).</summary>
        static IEnumerator Grab43(float[] into)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t != null) Luma43(t, into);
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        static void Luma43(Texture2D t, float[] into)
        {
            Color32[] px = t.GetPixels32();
            int w = t.width, h = t.height;
            for (int gy = 0; gy < Grid43H; gy++)
                for (int gx = 0; gx < Grid43W; gx++)
                {
                    Color32 c = px[Mathf.Min(h - 1, (gy * h + h / 2) / Grid43H) * w + Mathf.Min(w - 1, (gx * w + w / 2) / Grid43W)];
                    into[gy * Grid43W + gx] = (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b) / 255f;
                }
        }

        /// <summary>Saves dir/shot-N43-name.png and the screen change against `baseline`: (mean |delta luma|, share of cells moved > 0.1) into diff.</summary>
        static IEnumerator Shot43(string dir, string name, float[] baseline, double[] diff, float[] keep = null)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            diff[0] = diff[1] = -1;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N43-" + name + ".png"), t.EncodeToPNG());
                var now = keep ?? new float[Grid43W * Grid43H];
                Luma43(t, now);
                double sum = 0;
                int moved = 0;
                for (int i = 0; i < now.Length; i++)
                {
                    double d = Math.Abs(now[i] - baseline[i]);
                    sum += d;
                    if (d > 0.1) moved++;
                }
                diff[0] = sum / now.Length;
                diff[1] = moved / (double)now.Length;
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        /// <summary>The honest pair: the mean |on - off| luma of the same moment's frames, inside the screen's centre (the middle half each way: where
        /// the hit, the slug's impact and the jet are; the HUD's corners and the run clock are left out).</summary>
        static double Pair43(float[] on, float[] off)
        {
            double sum = 0;
            int n = 0;
            for (int gy = Grid43H / 4; gy < Grid43H * 3 / 4; gy++)
                for (int gx = Grid43W / 4; gx < Grid43W * 3 / 4; gx++)
                {
                    int i = gy * Grid43W + gx;
                    sum += Math.Abs(on[i] - off[i]);
                    n++;
                }
            return n > 0 ? sum / n : 0;
        }

        static IEnumerable<string> PairLine(Dictionary<string, double> pair, CultureInfo ci)
        {
            foreach (KeyValuePair<string, double> kv in pair) yield return kv.Key.Replace("F43-on-", "") + " " + kv.Value.ToString("0.0000", ci);
        }

        static IEnumerator Feel43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            Feel43 feel = g.ImpactDirector != null ? g.ImpactDirector.feel43 : null;
            if (feel == null) { res[0] = "RED feel43: no Feel43 (ImpactFrames not built)"; yield break; }
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId}";
            N43Blocks saved = sim.n43;
            N43Blocks? savedOverride = sim.n43Override;
            offs.Add(() =>
            {
                g.forcedCombat = IdleCombat;
                g.husks.frozen = false;
                sim.n43 = saved;
                sim.n43Override = savedOverride;
                feel.Force(N43View.RowOn(g.cfg, "feel43"));
            });
            N43Blocks fused = saved;
            fused.fusion43 = true; // W1's row for this run (its step reads sim.n43; a new run would read the override)
            sim.n43 = fused;
            sim.n43Override = fused;
            g.forcedCombat = null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "feel43", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 300;

            int fusions = 0, slugFires = 0, flames = 0, gHits = 0;
            offs.Add(g.events.On<Fusion43Event>(N43Events.Fusion, e => { if (e.owner == "player") fusions++; }));
            offs.Add(g.events.On<ChargeShot27Event>("chargeShot27Fire", e => { if (e.gun == "shotgun") slugFires++; }));
            offs.Add(g.events.On<FlameEvent>(G20Events.Flame, e => { if (e.owner == "player") flames++; }));
            offs.Add(g.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") gHits++; }));
            string lastFreeze = "none";
            double lastFreezeMs = 0;
            offs.Add(g.events.On<ImpactFreezeEvent>(FeelKeys.ImpactFreeze, e => { if (!e.skipped) { lastFreeze = e.cause; lastFreezeMs = e.seconds * 1000; } }));

            var lines = new List<string>();
            var change = new Dictionary<string, double>();
            var pair = new Dictionary<string, double>(); // on pass: the honest pair at the moment's key frame
            var offGrids = new Dictionary<string, float[]>();
            int framesSaved = 0, framesWanted = 0, moments = 0, momentsWanted = 0;
            var baseline = new float[Grid43W * Grid43H];
            var diff = new double[2];
            Husk bag = null;
            Action aim = () => { God(g); if (bag != null && bag.alive) Look(g, bag.AimPoint); };

            // frames at `offsets` (s) after t0 on the frame clock, each with its screen change; the +0.05 one is the pair's number
            IEnumerator Frames43(string label, double t0, double[] offsets, System.Text.StringBuilder sb)
            {
                for (int k = 0; k < offsets.Length; k++)
                {
                    framesWanted++;
                    while (FeelClock.RealNow - t0 < offsets[k] - 1e-6 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                    string tag = ((int)Math.Round(offsets[k] * 1000)).ToString("000", ci);
                    double left = g.ImpactDirector != null ? g.ImpactDirector.FreezeLeft : 0;
                    var grid = new float[Grid43W * Grid43H];
                    yield return Shot43(dir, label + "-" + tag, baseline, diff, grid);
                    string key = label.Replace("-on-", "-*-").Replace("-off-", "-*-") + "-" + tag;
                    double pairNow = -1;
                    if (label.Contains("-off-")) offGrids[key] = grid;
                    else if (offGrids.TryGetValue(key, out float[] og)) pairNow = Pair43(grid, og);
                    if (diff[0] >= 0) framesSaved++;
                    sb.Append(' ').Append(tag).Append(" change ").Append(diff[0].ToString("0.0000", ci)).Append(" moved ").Append((diff[1] * 100).ToString("0.0", ci))
                        .Append("% freezeLeft ").Append((left * 1000).ToString("0", ci)).Append(" ms kick ").Append(Feel43Probe.KickDeg.ToString("0.00", ci))
                        .Append(" word ").Append(Feel43Probe.WordScale.ToString("0.00", ci)).Append('/').Append(Feel43Probe.WordAlpha.ToString("0.00", ci))
                        .Append(" core ").Append(Feel43Probe.CoreK.ToString("0.00", ci)).Append(" ring ").Append(Feel43Probe.RingRadius.ToString("0.00", ci)).Append(" m;");
                    if (pairNow >= 0) sb.Append(" pair ").Append(pairNow.ToString("0.0000", ci));
                    if (k == 1 || offsets.Length == 1) { change[label] = diff[0]; if (pairNow >= 0) pair[label] = pairNow; }
                }
            }

            foreach (bool on in new[] { false, true })
            {
                string pass = on ? "on" : "off";
                feel.Force(on);
                feel.Haptics.ClearLog();
                Feel43Probe.Clear();

                // ---- a FUSION: carried at 16 m/s into a husk, the gauntlet swung within reach ----
                momentsWanted++;
                yield return Equip27(g, "gauntlet");
                g.forcedCombat = null;
                g.Respawn(spot, spotYaw * 180 / Math.PI);
                g.input.SetLook(spotYaw, 0);
                g.husks.KillAll();
                bag = PlaceAt(g, 6, 0, out double _);
                if (bag != null) bag.hp = bag.maxHp = 1e9;
                yield return Wait27(g, 2.2, wallEnd, aim); // the swing's cooldown (gauntlet42 1.8 s; gauntlet43 1.25 s)
                yield return Grab43(baseline);
                if (bag != null)
                {
                    int f0 = fusions, h0 = gHits;
                    lastFreeze = "none";
                    lastFreezeMs = 0;
                    bool pressed = false, synthetic = false;
                    double w0 = Time.realtimeSinceStartup;
                    while (fusions == f0 && Time.realtimeSinceStartup - w0 < 1.5 && Time.realtimeSinceStartup < wallEnd)
                    {
                        aim();
                        Vec3 to = bag.position - g.Feet;
                        to.y = 0;
                        double d = to.Length;
                        if (d > 0.3 && gHits == h0) g.mover.velocity = new Vec3(to.x / d * 16, g.mover.velocity.y, to.z / d * 16); // carried in at 16 m/s (W1: >= ~14)
                        if (!pressed && d < 2.3) { pad.Send(0f, 1f); pressed = true; }
                        yield return null;
                    }
                    pad.Send(0f, 0f);
                    if (fusions == f0)
                    {
                        // W1's fusion43 did not answer (not landed, or the hit missed): F1's picture framed from a synthetic event at the husk
                        synthetic = true;
                        var fake = new Fusion43Event { owner = "player", position = bag.AimPoint, dir = new Vec3(0, 0, -1), speed = 16, weapon = "gauntlet", enemy = bag.id, kind = "husk", killed = false, damage = 0 };
                        g.events.Emit(N43Events.Fusion, fake);
                    }
                    moments++;
                    double t0 = FeelClock.RealNow;
                    var sb = new System.Text.StringBuilder();
                    sb.Append($"F43-{pass}-fusion ({(synthetic ? "synthetic" : "real")}, gauntlet hits {gHits - h0}):");
                    yield return Frames43($"F43-{pass}-fusion", t0, new[] { 0, 0.05, 0.15, 0.30 }, sb);
                    sb.Append(" | freeze ").Append(lastFreeze).Append(' ').Append(lastFreezeMs.ToString("0", ci)).Append(" ms");
                    lines.Add(sb.ToString());
                    g.mover.velocity = new Vec3(0, g.mover.velocity.y, 0);
                }
                else lines.Add($"F43-{pass}-fusion: NO HUSK PLACED");
                yield return Wait27(g, 1.0, wallEnd, aim);

                // ---- a THUNDERCLAP charged release (the slug) killing a 1-hp husk at 6 m ----
                momentsWanted++;
                yield return Equip27(g, "shotgun");
                g.forcedCombat = null;
                g.Respawn(spot, spotYaw * 180 / Math.PI);
                g.input.SetLook(spotYaw, 0); // the same view in both passes
                g.husks.KillAll();
                bag = PlaceAt(g, 6, 0, out double _);
                if (bag != null) bag.hp = 1;
                yield return Wait27(g, 1.2, wallEnd, aim);
                {
                    // the last pass's charged shot cools first (charge35 / design.n27 cooldownSeconds), so this hold is a charge, not a plain volley
                    double c0 = Time.realtimeSinceStartup;
                    while (g.loadout.charge27 != null && g.loadout.charge27.cooldown[1] > 0 && Time.realtimeSinceStartup - c0 < 16 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                }
                yield return Grab43(baseline);
                {
                    int s0 = slugFires;
                    lastFreeze = "none";
                    lastFreezeMs = 0;
                    pad.Send(0f, 1f); // held: a charge (auto-released past full, or released below)
                    double w0 = Time.realtimeSinceStartup;
                    while (slugFires == s0 && Time.realtimeSinceStartup - w0 < 6 && Time.realtimeSinceStartup < wallEnd)
                    {
                        aim();
                        ChargeShots27 cs = g.loadout.charge27;
                        if ((cs != null && cs.charging && cs.Level(1) >= 1 && Time.realtimeSinceStartup - w0 > 0.3) || Time.realtimeSinceStartup - w0 > 3.5) pad.Send(0f, 0f); // full (or 3.5 s held: charge35 keeps its own hold state): let go
                        yield return null;
                    }
                    pad.Send(0f, 0f);
                    if (slugFires == s0) lines.Add($"F43-{pass}-slug: NO CHARGED RELEASE in 6 s (charge27 {(g.loadout.charge27 != null ? "present" : "absent")})");
                    else
                    {
                        moments++;
                        double t0 = FeelClock.RealNow;
                        bool slugOn = g.loadout.charge27 != null && g.loadout.charge27.p.slug;
                        var sb = new System.Text.StringBuilder();
                        sb.Append($"F43-{pass}-slug ({(slugOn ? "slug" : "charged volley")}):");
                        yield return Frames43($"F43-{pass}-slug", t0, new[] { 0, 0.05, 0.15 }, sb);
                        sb.Append(" | freeze ").Append(lastFreeze).Append(' ').Append(lastFreezeMs.ToString("0", ci)).Append(" ms");
                        lines.Add(sb.ToString());
                    }
                }
                yield return Wait27(g, 1.0, wallEnd, aim);

                // ---- the TORCH held on a husk at 4 m ----
                yield return Equip27(g, "torch");
                g.forcedCombat = null;
                if (g.loadout.Weapon != "torch") lines.Add($"F43-{pass}-torch: SKIPPED (the TORCH is not in hand: {g.loadout.Weapon})");
                else
                {
                    momentsWanted++;
                    g.Respawn(spot, spotYaw * 180 / Math.PI);
                    g.input.SetLook(spotYaw, 0); // the same view in both passes
                    g.husks.KillAll();
                    bag = PlaceAt(g, 4, 0, out double _);
                    if (bag != null) bag.hp = bag.maxHp = 1e9;
                    yield return Wait27(g, 0.8, wallEnd, aim);
                    yield return Grab43(baseline);
                    int fl0 = flames;
                    pad.Send(0f, 1f);
                    double w0 = Time.realtimeSinceStartup;
                    while (flames == fl0 && Time.realtimeSinceStartup - w0 < 2 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                    if (flames == fl0)
                    {
                        momentsWanted--; // the TORCH is NIGHT20 B5's (Combat.cs b5On): not live in this run, so not a moment of it
                        lines.Add($"F43-{pass}-torch: SKIPPED (no flame tick in 2 s: the TORCH is not live in this run)");
                    }
                    else
                    {
                        moments++;
                        double t0 = FeelClock.RealNow;
                        var sb = new System.Text.StringBuilder();
                        sb.Append($"F43-{pass}-torch:");
                        yield return Frames43($"F43-{pass}-torch", t0, new[] { 0.3, 1.0 }, sb);
                        sb.Append($" | ticks {flames - fl0}");
                        lines.Add(sb.ToString());
                    }
                    pad.Send(0f, 0f);
                }
                lines.Add($"F43-{pass} probe: {Feel43Probe.Line()} | {feel.Haptics.LogLine()}");
                yield return Wait27(g, 0.6, wallEnd, aim);
            }
            g.husks.KillAll();
            yield return Wait27(g, 0.3, wallEnd, null);
            yield return Stop();

            bool more = true; // the old test, printed for reference
            bool paired = true;
            foreach (string m in new[] { "fusion", "slug", "torch" })
                if (!pair.ContainsKey("F43-on-" + m) && m != "torch") paired = false;
                else if (pair.TryGetValue("F43-on-" + m, out double pv) && pv < UnityTuning.Feel43PairMin) paired = false;
            foreach (string m in new[] { "fusion", "slug" })
                more &= change.TryGetValue("F43-on-" + m, out double a) && change.TryGetValue("F43-off-" + m, out double o) && a > o;
            bool ok = moments == momentsWanted && framesSaved == framesWanted && paired;
            res[0] = $"{(ok ? "GREEN" : "RED")} feel43 | moments {moments}/{momentsWanted}, frames {framesSaved}/{framesWanted}, on vs off differ in the centre (pair >= {UnityTuning.Feel43PairMin.ToString("0.000", ci)} on fusion +0.05, slug +0.05, torch +1.0) {paired} [{string.Join(", ", PairLine(pair, ci))}], on changed the screen more than off vs its baseline {more}"
                + $" ||{string.Join(" || ", lines)} | {pc}";
        }
    }
}
