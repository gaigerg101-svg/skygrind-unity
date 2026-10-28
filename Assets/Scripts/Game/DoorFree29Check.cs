// NIGHT29 item 14 (GG's dev tool: hold RB 3 s and every door opens; enemies still spawn; the run is UNRANKED): the Player check
//   SKYGRIND.exe -skygrindDoorFree29 [-skygrindNoHold29] -logFile <f>
// boots the saved campaign level, starts the run, counts the open room zones, holds the toggle for 3.6 s through the same sim input the raw RB
// button feeds (N29Fixes.ForceDoorFreeHold; the pad itself is not driven), then counts again, then drops the player into the first FIGHT room and
// waits for its wave. Prints `[doorfree29] ...` lines and `[doorfree29] PASS|FAIL`, then quits. RED: -skygrindNoHold29 never holds (the doors stay locked).
using System;
using System.Collections;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class DoorFree29Check : MonoBehaviour
    {
        SurvivalGame game;

        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-skygrindDoorFree29") >= 0;

        public static void Attach(SurvivalGame g) => g.gameObject.AddComponent<DoorFree29Check>().game = g;

        IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(1.5f);
            bool noHold = Array.IndexOf(Environment.GetCommandLineArgs(), "-skygrindNoHold29") >= 0;
            game.setup.mode = RunSetup.Campaign;
            game.StartRun();
            yield return new WaitForSecondsRealtime(1.5f);
            SurvivalSim sim = game.sim;
            LevelDef lv = sim.campaignLevel;
            if (lv == null) { Debug.Log("[doorfree29] FAIL: no campaign level on map " + game.MapId); Application.Quit(); yield break; }
            int OpenCount() { int n = 0; foreach (RoomDef r in lv.rooms) if (sim.zones.IsOpen(r.id)) n++; return n; }
            int before = OpenCount();
            bool onBefore = sim.DoorFreeOn;
            if (!noHold) N29Fixes.ForceDoorFreeHold = true;
            yield return new WaitForSecondsRealtime(3.6f);
            N29Fixes.ForceDoorFreeHold = false;
            yield return new WaitForSecondsRealtime(0.5f);
            int after = OpenCount();
            bool unranked = sim.Unranked29;
            Debug.Log($"[doorfree29] map '{game.MapId}', {lv.rooms.Count} rooms: open before {before}, after {after}; DoorFreeOn before {onBefore}, after {sim.DoorFreeOn}; Unranked29 {unranked}; hold {(noHold ? "NOT held (RED run)" : "3.6 s")}");
            int fight = -1;
            for (int i = 1; i < lv.rooms.Count; i++) if (lv.rooms[i].kind == "room" && lv.rooms[i].waves != null && lv.rooms[i].waves.Count > 0) { fight = i; break; }
            int alive = -1;
            if (fight >= 0)
            {
                RoomDef fr = lv.rooms[fight];
                game.Respawn(fr.spawn, fr.spawnYawDeg);
                game.sim.health.hp = game.sim.health.max = 1e6;
                yield return new WaitForSecondsRealtime(3f);
                alive = sim.enemies.AliveCount;
                Debug.Log($"[doorfree29] dropped into room {fight} '{fr.id}': {alive} enemies alive after 3 s");
            }
            bool pass = sim.DoorFreeOn && unranked && after >= lv.rooms.Count && after > before && alive > 0;
            Debug.Log("[doorfree29] " + (pass ? "PASS" : "FAIL") + (noHold ? " (expected FAIL: the RED run never held)" : ""));
            Application.Quit();
        }
    }
}
