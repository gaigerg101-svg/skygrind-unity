// NIGHT33 builder E (design.n33.ai33): the Player scenario of card E (the lead's FeelReel.N27.cs dispatches PhaseN33E to N33EnemiesBody).
//   ai33  Boot `-skygrindMap n32-spire` (a lava level): a CAMPAIGN run, DOORFREE, the player (god mode, no input: he stands still, so every walker comes at him) put at the
//         spawn of v6 (the harness's worst room: 49 AI lava deaths in 60 s with the row off), then of f10 on `-skygrindMap n32-foundry`. 40 s per room. Every lava enemy death
//         is told by its cause at the touch that began its burn: PLAYER (knocked: knockSteps > 0, launched, or knocked within the last 1 s) or AI. Frames at 6 s and 20 s.
//         PASS: the row on, at least one enemy spawned, 0 AI lava deaths. With -skygrindOff33 the same run is the RED control (expect AI deaths > 0).
//         What to look at in the frames: walkers bunched on the bank at the lava's edge (not in it, no burn glow on any body), none standing in the lava.
// Shots go to <dir>/shot-N29-N33-*.png (Shot29's sampled numbers). Every result line starts with PASS or RED. Not run by its author (no Player in the worktree).
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
        static partial void N33EnemiesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "ai33") body = Ai33Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Ai33Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            var lines = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            offs.Add(() => Quiet(g));
            offs.Add(() => { g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (boot -skygrindMap n32-spire or n32-foundry) | " + pc; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            bool on = sim.n33.ai33;
            if (sim.lava30 == null || sim.lava30.zones.Count == 0) { res[0] = $"RED: no lava on level {sim.campaignLevel.id} (n30.lava {sim.n30.lava}) | " + pc; yield break; }
            string roomId = sim.campaignLevel.rooms.Exists(r => r.id == "v6") ? "v6" : sim.campaignLevel.rooms.Exists(r => r.id == "f10") ? "f10" : null;
            RoomDef room = roomId != null ? sim.campaignLevel.rooms.Find(r => r.id == roomId) : null;
            if (room == null) { res[0] = $"RED: neither v6 nor f10 on level {sim.campaignLevel.id} | " + pc; yield break; }
            int aiDead = 0, plDead = 0, burns = 0, spawned = 0;
            var cause = new Dictionary<int, bool>();
            var lastKnock = new Dictionary<int, double>();
            Action<Lava30Event> onBurn = e =>
            {
                burns++;
                Enemy en = sim.enemies.all.Find(x => x.id == e.id);
                cause[e.id] = en != null && (en.knockSteps > 0 || en is Husk h && h.launched || lastKnock.TryGetValue(e.id, out double tk) && sim.SimTime - tk <= 1.0);
            };
            Action<Lava30Event> onDeath = e => { if (cause.TryGetValue(e.id, out bool p) && p) plDead++; else aiDead++; };
            Action<SpawnEvent> onSpawn = _ => spawned++;
            Action u1 = sim.events.On("lavaEnemyBurn", onBurn), u2 = sim.events.On("lavaEnemyDeath", onDeath), u3 = sim.events.On("enemySpawn", onSpawn);
            offs.Add(() => { u1(); u2(); u3(); });
            Start(g, "ai33", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            g.Respawn(new Vec3(room.spawn.x, room.spawn.y + 0.05, room.spawn.z), room.spawnYawDeg);
            int step0 = sim.simStep;
            bool shotA = false, shotB = false;
            double wallEnd = Time.realtimeSinceStartup + 240;
            while ((sim.simStep - step0) * sim.step < 40.0 && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                if (sim.flow == Flow.Down) { sim.downSteps = 0; sim.flow = Flow.Playing; }
                g.forcedMove = default;
                g.forcedCombat = new CombatInput { weapon = -1 };
                foreach (Enemy en in sim.enemies.all) if (en.alive && (en.knockSteps > 0 || en is Husk h && h.launched)) lastKnock[en.id] = sim.SimTime;
                double t = (sim.simStep - step0) * sim.step;
                if (!shotA && t >= 6.0) { shotA = true; yield return Shot29(dir, "N33-ai33-" + roomId + "-6s", st); shots.Add(ShotLine29("ai33 " + roomId + " 6 s", st, ci)); }
                if (!shotB && t >= 20.0) { shotB = true; yield return Shot29(dir, "N33-ai33-" + roomId + "-20s", st); shots.Add(ShotLine29("ai33 " + roomId + " 20 s", st, ci)); }
                yield return null;
            }
            bool ok = on && spawned > 0 && aiDead == 0;
            lines.Add($"{(ok ? "PASS" : "RED")} ai33 {roomId} (row {(on ? "ON" : "OFF")}): spawned {spawned}, burns {burns}, lava deaths AI {aiDead} / PLAYER {plDead}, guard backs {sim.ai33CrowdBacks}, spared {sim.lava30.spared33}, spawns moved {sim.ai33SpawnMoves}, masked nodes {sim.ai33Masked} (harness ai33: off 49 AI deaths in v6 / 60 s, on 0)");
            res[0] = $"{(ok ? "PASS" : "RED")}: ai33 ({sim.campaignLevel.id}) | {string.Join(" ; ", lines)} | frames {string.Join(", ", shots)} | " + pc;
        }
    }
}
