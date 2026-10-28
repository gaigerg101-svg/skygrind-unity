// NIGHT32 builder G: the Player scenarios of the grapple / balance / HUD rows (the names are FeelReel.N27.cs PhaseN32G; the body is the partial N32GrappleBody).
// Each writes PASS or RED lines into res with the numbers, on the player's own input path (g.forcedMove / g.forcedCombat go through the reader the pad and the
// keyboard feed, then the sim's tap latch and the grapple32 charges: no code-set rope, L-031); buttons held for whole Player frames (L-039). Not run by its
// author (no Player in the worktree).
//   grapple32  boot -skygrindMap n30-span. CAMPAIGN, DOORFREE, the ARCLINE owned. From the first room with a node: 20 s of TAP SPAM (whenever the rope is idle,
//              aim at the nearest node in sight and press until the hook is out or 0.25 s passed, then up 4 sim steps after it fired; walk on toward the next door
//              when no node is in sight). Lines: presses, hooks fired, pulls, presses cleared (no charge / the gap), charges over time, the pips (GrapplePips32:
//              frames shown, full count seen 2 / 1 / 0, a partial fill seen), and the frames to look at (pips full, pips empty, pips refilling) plus three shots.
//              PASS (rows on): pulls at most 2 + floor(20 / recharge) + kills and at least 3, the pips seen full, empty and refilling. With -skygrindOff32 the
//              same spam runs on NIGHT31's grapple: RED (no charges, no pips), with its pull count (the before).
//   hud32      any map, SURVIVAL (the boss reels' own path): THE LANTERN begun (BossRoomBegin hulk) until its first teaching line shows (or 10 s), then THE STAG
//              begun at once (BossRoomBegin glacier, no room change: the reels' jump); for 2 s: frames with the Lantern's line still up, frames with a
//              "ROUND n IN" top line, then RT held 1.2 s on the pistol: the slot's word (CHARGING). Shots: the Stag fight's HUD, the charging slot.
//              PASS (row on): 0 frames with the Lantern line after 0.1 s, 0 frames of "ROUND n IN" during the Stag, the word read CHARGING. -skygrindOff32: RED.
//   balance32  a line only: balance32 has no sim change yet (builder G's slice 5 adds the table first; the reel body comes with any change it makes).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N32GrappleBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "grapple32") body = Grapple32Scenario(g, dir, res, offs, pre);
            else if (s == "hud32") body = Hud32Scenario(g, dir, res, offs, pre);
            else if (s == "balance32") body = Balance32Scenario(res);
        }

        static IEnumerator Balance32Scenario(string[] res)
        {
            res[0] = "SHORT: balance32 has no sim change yet (no Player check to run); the harness table comes first (builder G slice 5)";
            yield break;
        }

        // ======================================================= grapple32 =======================================================

        static IEnumerator Grapple32Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            if (sim.map.grappleNodes.Count < 3) { res[0] = $"RED: the booted map '{g.MapId}' has {sim.map.grappleNodes.Count} grapple nodes (boot -skygrindMap n30-span)"; yield break; }
            yield return StartCampaign30(g, offs);
            double t0 = sim.SimTime;
            while (sim.SimTime - t0 < 1.0) { God(g); yield return null; }
            bool owned = sim.arcline21 != null && sim.arcline21.Owned;
            bool on = sim.Grapple32On;
            string pc = Preconditions(g, g.Feet) + $" | n32.grapple32 {sim.n32.grapple32}, grapplePips row {N32Fixes.ViewRowOn(g.cfg, "grapplePips")}, range {g.cfg.movement.arcline.range.ToString("0", ci)} m, charges {(on ? sim.grapple32Rules.charges.ToString() : "-")}, recharge {(on ? sim.grapple32Rules.rechargeSeconds.ToString("0.0", ci) : "-")} s";
            if (!owned) { res[0] = "RED: the ARCLINE is not owned 1 s into the level | " + pc; yield break; }
            RoomDef start = null;
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                foreach (Vec3 n in sim.map.grappleNodes)
                    if (n.x >= r.x0 - 1 && n.x <= r.x1 + 1 && n.z >= r.z0 - 1 && n.z <= r.z1 + 1) { start = r; break; }
                if (start != null) break;
            }
            if (start == null) { res[0] = "RED: no room holds a node | " + pc; yield break; }
            g.Respawn(start.spawn, start.spawnYawDeg);
            double t1 = sim.SimTime;
            while (sim.SimTime - t1 < 0.5) { God(g); yield return null; }
            var doors = new List<Vec3>();
            if (sim.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("doors", out object dO) && dO is List<object> dl)
                foreach (object o in dl)
                    if (o is Dictionary<string, object> dd && dd.TryGetValue("rect", out object rO) && rO is List<object> rc && rc.Count == 4)
                        doors.Add(new Vec3((PortData.Num(rc[0]) + PortData.Num(rc[2])) / 2, 0, (PortData.Num(rc[1]) + PortData.Num(rc[3])) / 2));
            int fires = 0, kills = 0;
            var fireTimes = new List<string>();
            double tSpam = 0;
            Action<MoverEvent> onMover = e => { if (e.type == MoverEventType.RopeFire) { fires++; if (fireTimes.Count < 24) fireTimes.Add((sim.SimTime - tSpam).ToString("0.0", ci)); } };
            sim.mover.Events += onMover;
            offs.Add(() => sim.mover.Events -= onMover);
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.kind != "boss") kills++; }));
            int pulls0 = sim.grappleTap31.pulls, empty0 = sim.g32Empty;
            Start(g, "grapple32", "camera", dir);
            yield return Frames(pre);
            tSpam = sim.SimTime;
            double tEnd = tSpam + 20;
            int presses = 0, pressStep = -1, firedStep = -1, lastNode = -1;
            int pipFrames = 0, frameFull = -1, frameEmpty = -1, frameRefill = -1, frameOne = -1;
            bool seen2 = false, seen1 = false, seen0 = false, seenPartial = false;
            var chargeLog = new List<string>();
            int lastCharges = -1;
            bool shotFull = false, shotEmpty = false, shotRefill = false;
            while (sim.SimTime < tEnd)
            {
                God(g);
                Rope r = sim.mover.rope;
                Vec3 eye = sim.Eye;
                var f = new InputFrame();
                if (pressStep >= 0 && firedStep < 0 && r.phase != RopePhase.None) firedStep = sim.simStep;
                if (pressStep >= 0 && (firedStep >= 0 || sim.simStep - pressStep > 30))
                {
                    // the press is spent: up 4 sim steps after the hook went out (a TAP), or at once when nothing fired (no charge, the gap)
                    f.grapple = new Btn { held = firedStep >= 0 && sim.simStep - firedStep < 4, pressed = false };
                    if (lastNode >= 0) (f.yaw, f.pitch) = LookAt31(g, sim.map.grappleNodes[lastNode]);
                    if (r.phase == RopePhase.None && (firedStep < 0 || sim.simStep - firedStep > 4)) pressStep = -1;
                }
                else if (pressStep >= 0)
                {
                    (f.yaw, f.pitch) = LookAt31(g, sim.map.grappleNodes[lastNode]);
                    f.grapple = new Btn { held = true, pressed = true }; // pressed for whole frames until the hook is out (L-039)
                }
                else
                {
                    int next = -1;
                    double bd = double.MaxValue;
                    for (int i = 0; i < sim.map.grappleNodes.Count; i++)
                    {
                        Vec3 n = sim.map.grappleNodes[i];
                        double d = Vec3.DistanceTo(n, eye);
                        if (d < 4 || d > g.cfg.movement.arcline.range * 0.9 || i + 1 == r.lastNode || !Sees31(g, eye, n)) continue;
                        if (d < bd) { bd = d; next = i; }
                    }
                    if (next >= 0 && r.phase == RopePhase.None && r.cooldownSteps == 0)
                    {
                        lastNode = next;
                        (f.yaw, f.pitch) = LookAt31(g, sim.map.grappleNodes[next]);
                        f.grapple = new Btn { held = true, pressed = true };
                        presses++;
                        pressStep = sim.simStep;
                        firedStep = -1;
                    }
                    else if (next < 0)
                    {
                        Vec3? wp = null;
                        foreach (Vec3 dc in doors) if (dc.z < sim.mover.position.z - 0.3 && (!wp.HasValue || dc.z > wp.Value.z)) wp = dc;
                        if (wp.HasValue) (f.yaw, f.pitch) = LookAt31(g, new Vec3(wp.Value.x, eye.y, wp.Value.z - 2));
                        else (f.yaw, f.pitch) = (g.input.yaw, 0);
                        f.moveZ = 1;
                    }
                    else (f.yaw, f.pitch) = LookAt31(g, sim.map.grappleNodes[next]);
                }
                g.input.SetLook(f.yaw, f.pitch);
                g.forcedMove = f;
                if (on && sim.grappleCharges32 != lastCharges)
                {
                    lastCharges = sim.grappleCharges32;
                    if (chargeLog.Count < 30) chargeLog.Add($"{(sim.SimTime - tSpam).ToString("0.0", ci)}:{lastCharges}");
                }
                if (GrapplePips32.Shown)
                {
                    pipFrames++;
                    int full = GrapplePips32.FullShown;
                    if (full >= 2) { seen2 = true; if (frameFull < 0) frameFull = FeelProbe.FrameCount; }
                    if (full == 1) { seen1 = true; if (frameOne < 0) frameOne = FeelProbe.FrameCount; }
                    if (full == 0) { seen0 = true; if (frameEmpty < 0) frameEmpty = FeelProbe.FrameCount; }
                    if (GrapplePips32.NextFill > 0.3f && GrapplePips32.NextFill < 0.7f) { seenPartial = true; if (frameRefill < 0) frameRefill = FeelProbe.FrameCount; }
                }
                yield return null;
                if (GrapplePips32.Shown)
                {
                    // three still shots (the end of a frame), each once: both full, both spent, one refilling
                    if (!shotFull && GrapplePips32.FullShown >= 2 && sim.SimTime - tSpam < 1) { shotFull = true; yield return Shot30(dir, "grapple32-pips-full"); }
                    else if (!shotEmpty && GrapplePips32.FullShown == 0) { shotEmpty = true; yield return Shot30(dir, "grapple32-pips-empty"); }
                    else if (!shotRefill && GrapplePips32.FullShown < 2 && GrapplePips32.NextFill > 0.45f && GrapplePips32.NextFill < 0.6f) { shotRefill = true; yield return Shot30(dir, "grapple32-pips-refill"); }
                }
            }
            g.forcedMove = new InputFrame();
            yield return Frames(Ms(300));
            yield return Stop();
            int pulls = sim.grappleTap31.pulls - pulls0;
            int cleared = sim.g32Empty - empty0;
            double recharge = on ? sim.grapple32Rules.rechargeSeconds : 4;
            int cap = 2 + (int)Math.Floor(20 / recharge) + kills;
            bool ok = on && pulls <= cap && pulls >= 3 && seen2 && seen0 && seenPartial;
            res[0] = $"{(ok ? "PASS" : "RED")}: {(on ? "rows ON" : "rows OFF (-skygrindOff32 or the row off: NIGHT31's grapple)")}; 20 s of tap spam: presses {presses}, hooks fired {fires}, pulls {pulls} (cap 2 + floor(20 / {recharge.ToString("0.0", ci)}) + kills {kills} = {cap}), presses cleared {cleared}; hook times [{string.Join(" ", fireTimes)}] s; charges [{string.Join(" ", chargeLog)}]; pips shown {pipFrames} frames, full 2 {seen2} / 1 {seen1} / 0 {seen0}, a partial fill {seenPartial} | frames: pips full f{frameFull:0000}, one left f{frameOne:0000}, empty f{frameEmpty:0000}, refilling f{frameRefill:0000}; shots shot-N30-grapple32-pips-full / -empty / -refill.png | {pc}";
        }

        // ======================================================= hud32 =======================================================

        static IEnumerator Hud32Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            bool row = N32Fixes.ViewRowOn(g.cfg, "hud32");
            string pc = Preconditions(g, g.Feet) + $" | hud32 row {row}, mode {g.setup.mode}, run state {g.run.state}";
            offs.Add(() => Quiet(g));
            Quiet(g);
            g.husks.frozen = true;
            God(g);
            // THE LANTERN until its first teaching line (bossLine30) shows
            string lanternLine = null;
            offs.Add(g.events.On<BossLine30>("bossLine30", e => { if (e.boss == "lantern" && lanternLine == null) lanternLine = e.text; }));
            if (sim.lantern27 != null && sim.lantern27.Alive) sim.lantern27.Clear();
            sim.flow = Flow.Playing;
            if (!sim.BossRoomBegin("hulk", out Vec3 lsp, out double lyaw) || sim.lantern27 == null || !sim.lantern27.Alive) { res[0] = "RED: BossRoomBegin(hulk) did not start THE LANTERN | " + pc; yield break; }
            g.Respawn(lsp, lyaw);
            Start(g, "hud32", "camera", dir);
            yield return Frames(pre);
            double t0 = sim.SimTime;
            while (sim.SimTime - t0 < 10 && (lanternLine == null || !Ui.Lesson25Visible)) { God(g); yield return null; }
            int frameLine = FeelProbe.FrameCount;
            bool lineSeen = lanternLine != null && Ui.Lesson25Visible;
            string lineText = Ui.Lesson25TextNow;
            // THE STAG at once (the reels' jump: no room change)
            if (sim.stag31 != null && sim.stag31.Alive) sim.stag31.Clear();
            g.health.Reset();
            sim.downSteps = 0;
            sim.flow = Flow.Playing;
            if (!sim.BossRoomBegin("glacier", out Vec3 ssp, out double syaw) || sim.stag31 == null || !sim.stag31.Alive) { yield return Stop(); res[0] = "RED: BossRoomBegin(glacier) did not start THE STAG | " + pc; yield break; }
            g.Respawn(ssp, syaw);
            God(g);
            double t1 = sim.SimTime;
            int staleFrames = 0, staleAfter = 0, roundFrames = 0, frames = 0;
            string roundSample = null;
            int frameStag = -1;
            while (sim.SimTime - t1 < 2.0)
            {
                God(g);
                yield return null;
                frames++;
                bool stale = Ui.Lesson25Visible && lanternLine != null && Ui.Lesson25TextNow == lineText;
                if (stale) { staleFrames++; if (sim.SimTime - t1 > 0.1) staleAfter++; }
                string top = Ui.Hud32TopLineNow ?? "";
                if (top.StartsWith("ROUND ", StringComparison.Ordinal) && top.Contains(" IN ")) { roundFrames++; roundSample ??= top; }
                if (frameStag < 0 && sim.SimTime - t1 > 1.0) frameStag = FeelProbe.FrameCount;
            }
            yield return Shot30(dir, "hud32-stag-hud");
            // RT held on the pistol: the slot's word
            string word = "not seen";
            ChargeShots27 cs = sim.charge27;
            if (cs != null && cs.active && sim.n27.chargeShots27)
            {
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { held = true, pressed = true } };
                yield return null;
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { held = true, pressed = false } };
                double t2 = sim.SimTime;
                while (sim.SimTime - t2 < 1.2)
                {
                    God(g);
                    yield return null;
                    if (Slot30State("revolver") == "CHARGING") word = Slot30Word("revolver");
                }
                yield return Shot30(dir, "hud32-charging");
                g.forcedCombat = IdleCombat;
            }
            else word = "not run (chargeShots27 off or no charge shots)";
            yield return Frames(Ms(300));
            yield return Stop();
            if (sim.stag31 != null && sim.stag31.Alive) sim.stag31.Clear();
            bool ok = row && staleAfter == 0 && roundFrames == 0 && (word == UnityTuning.Hud30WordCharging || word.StartsWith("not run", StringComparison.Ordinal));
            res[0] = $"{(ok ? "PASS" : "RED")}: {(row ? "hud32 ON" : "hud32 OFF")}; the Lantern's line '{lanternLine ?? "none"}' seen {lineSeen} (f{frameLine:0000}); after the jump to THE STAG (2 s, {frames} frames): the line still up {staleFrames} frames ({staleAfter} after 0.1 s), boss lines dropped by hud32 {Ui.Hud32BossLineDrops}; 'ROUND n IN' top line {roundFrames} frames{(roundSample != null ? $" ('{roundSample}')" : "")}, break line hidden by hud32 {Ui.Hud32BreakHidden}; RT on the pistol: the slot's word '{word}' | frames: line up f{frameLine:0000}, Stag HUD f{frameStag:0000}; shots shot-N30-hud32-stag-hud.png, shot-N30-hud32-charging.png | {pc}";
        }
    }
}
