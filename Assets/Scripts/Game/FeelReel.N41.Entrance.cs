// NIGHT41 builder T2 (INVENTORY B1 / B2's proof; NIGHT41.md card T2): the Player scenario `entrance41`: the booted campaign level's boss room
// entered for real, framed through the entrance beat. One Player per boss (L-046): the level picks the boss (L1 n38-foundry THE WICK, L2 n40-iceline
// THE ICEBREAKER, L3 n40-kiln the worm, L4 n40-span THE LOOM, L5 n40-spire THE CROWN; an n41 level id works as its levelFrom).
// Not run by its author (no Player in the worktree). Input through forced frames only; the player God.
// The run: a CAMPAIGN run (Campaign41), DOORFREE on so the boss room is reachable, the banners it queued silenced at once (A9); the husks killed;
// 3 s of quiet (any banner left runs out); then the player is put at the boss room's spawn (kind "boss"), facing its yaw: the campaign enters
// the room and begins the fight itself (as brawl40 / loom37). If no `bossRise` came 3 s after the step in, the player walks forward up to 4 s;
// still none: SurvivalSim.BossRoomBegin(room.boss) (the line says "FORCED BEGIN": the entry did not begin it).
// Frames (shot-N30-<name>.png), t = REAL time after the frame `bossRise` was seen (the beat is a view thing; a hitstop holds the sim, not the clock):
//   E41-<boss>-walkin     the first frame at the room's spawn, if bossRise has not come yet: nothing of the beat (the room, no banner)
//   E41-<boss>-000 / -150 / -300 / -600 / -900 / -1200 / -1600   the beat: the name card, the shake, the CRT flicker, the rule line (~0.45 s),
//                         the fade (1.25-1.5 s), and the bars (bars41's one bar; the HUD33 bar and the Boss37 stagger bar hidden while it draws)
// Each frame's line prints J2's probes by reflection (Entrance41Probe, Juice38Probe, BossBar41Probe; "absent" when not in the build) and
// Ui.Hud33BossBarShown / Boss37StaggerBar.Shown / N41View.BossBar41Live, plus the banner on screen (A9: want none).
// Line: GREEN when bossRise came from the room's own entry (not forced) and all 8 frames were saved.
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
        static IEnumerator Entrance41Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED entrance41: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "); boot reel.sh + -skygrindMap <level> + -skygrindReelOnly entrance41"; yield break; }
            SurvivalSim sim = g.sim;
            LevelDef lv = sim.campaignLevel;
            RoomDef room = lv != null ? lv.rooms.Find(r => r.kind == "boss") : null;
            if (room == null) { res[0] = $"RED entrance41: level {lv?.id ?? "?"} has no boss room"; yield break; }
            string boss = room.boss ?? "?";
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            God(g);
            double wallEnd = Time.realtimeSinceStartup + 90;
            Action hold = () => { God(g); };
            yield return Wait27(g, 3.0, wallEnd, hold); // any banner left runs out
            g.UiView?.ClearBanners41();
            int rises = 0;
            double riseReal = -1;
            int riseStep = -1;
            offs.Add(g.events.On("bossRise", o => { rises++; if (riseReal < 0) { riseReal = Time.realtimeSinceStartup; riseStep = sim.simStep; } }));
            double yaw = room.spawnYawDeg * Math.PI / 180;
            Start(g, "entrance41-" + boss, "camera", dir);
            yield return Frames(pre);
            // the step in: the room's spawn, facing in
            Vec3 stand = room.spawn + new Vec3(0, 0.05, 0);
            g.Respawn(stand, room.spawnYawDeg);
            g.input.SetLook(yaw, 0.05);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05 };
            FeelProbe.MarkInput();
            string walkLine = $"room {room.id} '{room.name}' (boss {boss}), entered: campaign room now {sim.campaign?.Room?.id ?? "none"}";
            int saved = 0;
            if (riseReal < 0) { yield return Shot30(dir, "E41-" + boss + "-walkin"); saved++; }
            // the campaign begins the fight on its own when it enters the room; else walk in; else force it (said)
            double t0 = Time.realtimeSinceStartup;
            while (riseReal < 0 && Time.realtimeSinceStartup - t0 < 3.0) { God(g); yield return null; }
            string how = riseReal >= 0 ? "the room's own entry" : null;
            if (riseReal < 0)
            {
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05, moveZ = 1 };
                t0 = Time.realtimeSinceStartup;
                while (riseReal < 0 && Time.realtimeSinceStartup - t0 < 4.0) { God(g); yield return null; }
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05 };
                if (riseReal >= 0) how = "a 0-4 s walk forward";
            }
            bool forced = false;
            if (riseReal < 0)
            {
                forced = sim.BossRoomBegin(boss, out Vec3 _, out double _);
                for (int k = 0; k < 30 && riseReal < 0; k++) yield return null;
                how = forced ? "FORCED BEGIN (BossRoomBegin)" : "NO BEGIN";
            }
            var lines = new List<string>();
            if (riseReal >= 0)
            {
                double[] at = { 0, 0.15, 0.30, 0.60, 0.90, 1.20, 1.60 };
                foreach (double s in at)
                {
                    while (Time.realtimeSinceStartup - riseReal < s && Time.realtimeSinceStartup < wallEnd) { God(g); yield return null; }
                    string tag = ((int)Math.Round(s * 1000)).ToString("000", ci);
                    double real = Time.realtimeSinceStartup - riseReal, simS = (sim.simStep - riseStep) * sim.step;
                    lines.Add($"{tag}: real {real.ToString("0.000", ci)} s, sim {simS.ToString("0.000", ci)} s, banner '{Banner41(g)}' | {Probes41()}");
                    yield return Shot30(dir, "E41-" + boss + "-" + tag);
                    saved++;
                }
            }
            yield return Frames(10);
            yield return Stop();
            bool green = riseReal >= 0 && !forced && how != null && !how.StartsWith("FORCED") && saved == 8;
            res[0] = $"{(green ? "GREEN" : "RED")} entrance41 {boss} | begun by {how ?? "nothing"}, bossRise x{rises} | frames {saved}/8 | {walkLine}"
                + $" | map {g.MapId}, juice41On {N41View.RowOn(g.cfg, "juice41On")}, entrance41 {N41View.RowOn(g.cfg, "entrance41")}, bars41 {N41View.RowOn(g.cfg, "bars41")}"
                + $" || {string.Join(" || ", lines)}";
        }

        /// <summary>The banner text on screen now ("" none): the reel's A9 check.</summary>
        static string Banner41(SurvivalGame g) => g.UiView != null ? g.UiView.Banner41Now : "";
    }
}
