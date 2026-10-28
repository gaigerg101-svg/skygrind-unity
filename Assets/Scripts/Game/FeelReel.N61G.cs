// NIGHT61 builder G (S3 bossRule61's proof; Logs/n61/slate.json S3): the Player scenario `boss61` (registered in FeelReel.N41.cs's PhaseN41T
// beside entrance41, whose walk-in it copies). One Player per boss (L-046): the booted campaign level picks it (n52-foundry THE WICK, n52-iceline
// THE ICEBREAKER, n57-kiln THE KILN WYRM, n52-span THE LOOM, n52-spire THE CROWN). Not run by its author (no Player in the worktree). Input
// through forced frames only; the player God. The run: 1920x1080 windowed (a148's way), a CAMPAIGN run with DOORFREE, the husks killed, 3 s of
// quiet, then the player put at the boss room's spawn facing in: the campaign begins the fight itself (else a 0-4 s walk forward, else
// SurvivalSim.BossRoomBegin: the line says FORCED). From the frame `bossRise` is seen (REAL time):
//   B61-<boss>-1000   1.0 s after the rise (UnityTuning.BossRule61ShotS), 1920x1080: the name card and, with the row on, the rule line under it
//   B61-<boss>-720    the screen switched to 1280x720 while the line still stands (about 2 s after the rise): the same line, measured again
// Measured each frame (Ui.W48FightLine, a148's G5 measure: em in screen px = the label's font size x the canvas scale): the line shown, its
// alpha, its em, the text objects drawing it and any raw tag left; from BossRule61Probe: how many lines showed in the 8 s after the rise (want
// exactly 1: once per fight start) and the REAL time the line was visible (want <= BossRule61MaxS). The Player.log also carries the line's own
// proof, "[boss61] line: <boss> ...".
// Line: GREEN when the row is on (BossRule61.On) and: the rise came from the room's own entry or the walk (not forced), the boss is one of the
// five, at 1.0 s the line stands (alpha >= 0.5) with its {button} glyph drawn (no raw tag, one text object), at most 8 words, em >= 36 px at
// 1080p and >= 24 px at 720p, shown once, visible <= 5 s. With the row off (-skygrindNoBossRule61, -skygrindOff61, or the row off in the
// build's config) the line reads OFF and its frames are the SPOT pair's other half (no rule line: want shows 0).
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
        static IEnumerator Boss61Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            int w0 = Screen.width, h0 = Screen.height;
            FullScreenMode mode0 = Screen.fullScreenMode;
            offs.Add(() => { if (Screen.width != w0 || Screen.height != h0) Screen.SetResolution(w0, h0, mode0); });
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; });
            yield return Wick48Res(1920, 1080);
            string r1080 = $"{Screen.width}x{Screen.height}";
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED boss61: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "); boot reel.sh + -skygrindMap <level> + -skygrindReelOnly boss61"; yield break; }
            SurvivalSim sim = g.sim;
            LevelDef lv = sim.campaignLevel;
            RoomDef room = lv != null ? lv.rooms.Find(r => r.kind == "boss") : null;
            if (room == null) { res[0] = $"RED boss61: level {lv?.id ?? "?"} has no boss room"; yield break; }
            string boss = room.boss ?? "?";
            bool rowOn = BossRule61.On(g.cfg);
            string raw = BossRule61.Line(g.cfg, boss);
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            God(g);
            double wallEnd = Time.realtimeSinceStartup + 120;
            Action hold = () => { God(g); };
            yield return Wait27(g, 3.0, wallEnd, hold); // any banner left runs out
            g.UiView?.ClearBanners41();
            int rises = 0, shown0 = BossRule61Probe.Shown;
            double riseAt = -1;
            string risen = null;
            // NIGHT61 review S1: the rise and every rule-line time below are on the reel's fixed clock (FeelClock.Now), the clock the HUD ages the
            // line on (SurvivalGame hands it FeelClock.FrameDelta), not the wall clock: a fast or slow Player box no longer moves the verdict
            offs.Add(g.events.On<BossEvent>("bossRise", e => { rises++; if (riseAt < 0) { riseAt = FeelClock.Now; risen = e != null ? e.boss : null; } }));
            double yaw = room.spawnYawDeg * Math.PI / 180;
            Start(g, "boss61-" + boss, "camera", dir);
            yield return Frames(pre);
            g.Respawn(room.spawn + new Vec3(0, 0.05, 0), room.spawnYawDeg);
            g.input.SetLook(yaw, 0.05);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05 };
            FeelProbe.MarkInput();
            // the campaign begins the fight on its own when it enters the room; else walk in; else force it (said)
            double t0 = Time.realtimeSinceStartup;
            while (riseAt < 0 && Time.realtimeSinceStartup - t0 < 3.0) { God(g); yield return null; }
            string how = riseAt >= 0 ? "the room's own entry" : null;
            if (riseAt < 0)
            {
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05, moveZ = 1 };
                t0 = Time.realtimeSinceStartup;
                while (riseAt < 0 && Time.realtimeSinceStartup - t0 < 4.0) { God(g); yield return null; }
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05 };
                if (riseAt >= 0) how = "a 0-4 s walk forward";
            }
            bool forced = false;
            if (riseAt < 0)
            {
                forced = sim.BossRoomBegin(boss, out Vec3 _, out double _);
                for (int k = 0; k < 30 && riseAt < 0; k++) yield return null;
                how = forced ? "FORCED BEGIN (BossRoomBegin)" : "NO BEGIN";
            }
            if (riseAt < 0) { yield return Stop(); res[0] = $"RED boss61 {boss}: no bossRise ({how}) | map {g.MapId}, row {(rowOn ? "on" : "off")}"; yield break; }
            // the line's visible time, sampled every frame outside the two measured frames (it comes in ~0.45 s after the rise and stands ~4.5 s)
            double visFrom = -1, visTo = -1;
            void Track() { double t = FeelClock.Now - riseAt; if (BossRule61Probe.Alpha > 0f) { if (visFrom < 0) visFrom = t; visTo = t; } }
            while (FeelClock.Now - riseAt < UnityTuning.BossRule61ShotS && Time.realtimeSinceStartup < wallEnd) { God(g); Track(); yield return null; }
            Ui ui = g.UiView;
            string s1080 = "", s720 = "";
            float em1080 = 0, em720 = 0, ink = 0, k1080 = 0, k720 = 0, a1080 = 0, a720 = 0;
            bool built1080 = ui != null && ui.W48FightLine(out s1080, out em1080, out ink, out k1080, out a1080);
            int objects = Hud47Probe.FightObjects;
            bool rawTag = Hud47Probe.FightRawTag, card = Ui.Card38Shown;
            string at1080 = (FeelClock.Now - riseAt).ToString("0.000", ci);
            yield return Shot30(dir, "B61-" + boss + "-1000");
            Track();
            yield return Wick48Res(1280, 720);
            string r720 = $"{Screen.width}x{Screen.height}";
            yield return Frames(2);
            Track();
            bool built720 = ui != null && ui.W48FightLine(out s720, out em720, out ink, out k720, out a720);
            string at720 = (FeelClock.Now - riseAt).ToString("0.000", ci);
            yield return Shot30(dir, "B61-" + boss + "-720");
            yield return Wick48Res(1920, 1080);
            while (FeelClock.Now - riseAt < 8.0 && Time.realtimeSinceStartup < wallEnd) { God(g); Track(); yield return null; }
            int shows = BossRule61Probe.Shown - shown0;
            double visible = visFrom >= 0 ? visTo - visFrom : 0;
            yield return Frames(10);
            yield return Stop();
            int words = BossRule61.Words(raw);
            var bad = new List<string>();
            if (forced || how == null || how.StartsWith("FORCED") || how == "NO BEGIN") bad.Add("the rise was not the room's own (" + how + ")");
            if (!BossRule61.IsBoss(boss)) bad.Add("not one of the five campaign bosses");
            if (risen != boss) bad.Add($"the rise said '{risen}', the room '{boss}'");
            if (rowOn)
            {
                if (!built1080 || a1080 < 0.5f || string.IsNullOrEmpty(s1080)) bad.Add($"no line at 1.0 s (alpha {a1080.ToString("0.00", ci)})");
                if (em1080 < UnityTuning.BossRule61MinEm1080 - 0.01f) bad.Add($"em {em1080.ToString("0.0", ci)} px at 1080p (< {UnityTuning.BossRule61MinEm1080})");
                if (!built720 || a720 < 0.5f || em720 < UnityTuning.BossRule61MinEm720 - 0.01f) bad.Add($"em {em720.ToString("0.0", ci)} px at 720p (< {UnityTuning.BossRule61MinEm720}), alpha {a720.ToString("0.00", ci)} (< 0.5)"); // NIGHT61 review S2: the alpha too, as the 1080p check
                if (words < 1 || words > UnityTuning.BossRule61MaxWords) bad.Add(words + " words");
                if (!BossRule61.HasGlyph(raw)) bad.Add("no {button} glyph in the line");
                if (objects != 1 || rawTag) bad.Add($"text objects {objects}, raw tag {(rawTag ? "YES" : "no")}");
                if (shows != 1) bad.Add($"shown {shows} times in 8 s (want once)");
                if (visible > UnityTuning.BossRule61MaxS + 0.05) bad.Add($"visible {visible.ToString("0.00", ci)} s (> {UnityTuning.BossRule61MaxS})");
                if (r1080 != "1920x1080" || r720 != "1280x720") bad.Add($"screens {r1080} / {r720}");
            }
            else if (shows != 0) bad.Add($"row off but {shows} rule lines showed");
            string verdict = !rowOn ? (bad.Count == 0 ? "OFF" : "RED") : bad.Count == 0 ? "GREEN" : "RED";
            res[0] = $"{verdict} boss61 {boss} | row bossRule61 {(rowOn ? "on" : "OFF (the SPOT pair's other half)")}, rule \"{raw}\" ({words} words) | begun by {how}, bossRise x{rises} ('{risen}')"
                + $" | at {at1080} s, {r1080}: shown \"{s1080}\", alpha {a1080.ToString("0.00", ci)}, em {em1080.ToString("0.0", ci)} px (canvas x{k1080.ToString("0.000", ci)}), card up {card}, text objects {objects}, raw tag {(rawTag ? "YES" : "no")}"
                + $" | at {at720} s, {r720}: shown \"{s720}\", alpha {a720.ToString("0.00", ci)}, em {em720.ToString("0.0", ci)} px (canvas x{k720.ToString("0.000", ci)})"
                + $" | shows in 8 s {shows}, visible {visible.ToString("0.00", ci)} s (from {visFrom.ToString("0.00", ci)} s), planned {BossRule61Probe.OnScreenS.ToString("0.00", ci)} s; probe armed {BossRule61Probe.Armed}, dropped {BossRule61Probe.Dropped}, refused {BossRule61Probe.Refused}, cut {BossRule61Probe.Cut}"
                + $" | map {g.MapId}, room {room.id} '{room.name}'{(bad.Count > 0 ? " | BAD: " + string.Join("; ", bad) : "")}";
        }
    }
}
