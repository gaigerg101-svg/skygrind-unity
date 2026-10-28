// NIGHT71 builder B (leftover L1, design.n71.room71; the rule: Room71.cs; the select's half: UiRoom71.cs). The run's half: self-installed after
// the first scene load (no hook line), it outlives the select's scene reload. When a campaign run is Playing and Room71.PendingRoom names one of
// its level's rooms: hold DOORFREE through the player's own path (N29Fixes.ForceDoorFreeHold, as DoorFree29Check does) until the sim reads it
// on (Room71HoldCapS at most), then SurvivalGame.Respawn at the room's spawn; the campaign enters it by its zone. `-skygrindStartRoom <id>`
// sets the pending room at boot and starts the campaign run on the booted map (Room71BootWaitS after boot, DoorFree29Check's wait). The row is
// read when a run applies a pick and when the switch is read (off: said once, nothing done).
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Room71Runner : MonoBehaviour
    {
        static Room71Runner instance;
        SurvivalGame wiredFor;
        bool switchRead, bootStart;
        float bootAt;
        // the pick being applied
        int stage; // 0 idle, 1 holding DOORFREE, 2 waiting for the room's entry
        RoomDef target;
        string level;
        float runAt, holdAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying || instance != null) return;
            var go = new GameObject("Room71Runner") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            instance = go.AddComponent<Room71Runner>();
            instance.bootAt = Time.realtimeSinceStartup;
        }

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.sim == null || g.cfg == null) return;
            if (!ReferenceEquals(wiredFor, g))
            {
                wiredFor = g;
                stage = 0;
                g.events.On<RoomEvent>("roomEnter", OnRoomEnter);
            }
            if (!switchRead) ReadSwitch(g);
            if (bootStart && g.flow == Flow.Title && Time.realtimeSinceStartup - bootAt >= UnityTuning.Room71BootWaitS)
            {
                bootStart = false;
                g.setup.mode = RunSetup.Campaign;
                g.StartRun();
                Debug.Log("[room71] " + Room71.StartSwitch + ": the campaign run started on " + g.MapId);
            }
            if (stage == 0 && Room71.PendingRoom != null && g.flow == Flow.Playing && g.sim.campaignOn && g.sim.campaign != null && g.sim.campaignLevel != null) Begin(g);
            if (stage == 1)
            {
                if (g.flow != Flow.Playing) { N29Fixes.ForceDoorFreeHold = false; stage = 0; return; }
                if (g.sim.DoorFreeOn || Time.realtimeSinceStartup - holdAt > UnityTuning.Room71HoldCapS)
                {
                    N29Fixes.ForceDoorFreeHold = false;
                    bool on = g.sim.DoorFreeOn;
                    g.Respawn(target.spawn, target.spawnYawDeg);
                    Debug.Log("[room71] start room " + target.id + " '" + target.name + "' on " + level + ": DOORFREE " + (on ? "on" : "NOT on") + " after "
                        + Room71.Seconds(Time.realtimeSinceStartup - holdAt) + " s, respawned at its spawn");
                    stage = 2;
                }
            }
        }

        void ReadSwitch(SurvivalGame g)
        {
            switchRead = true;
            string[] args = Room71.Args();
            if (!NightRows.HasSwitch(args, Room71.StartSwitch)) return;
            string id = SameRun63Math.ArgAfter(args, Room71.StartSwitch);
            if (id == null) { Debug.Log("[room71] " + Room71.StartSwitch + " wants a room id (e.g. kb3): ignored"); return; }
            string off = Room71.OffReason(g.cfg, args);
            if (off != null) { Debug.Log("[room71] " + Room71.StartSwitch + " " + id + " ignored: " + off); return; }
            Room71.PendingRoom = id;
            bootStart = g.flow == Flow.Title;
            Debug.Log("[room71] " + Room71.StartSwitch + " " + id + ": the run will start there (map " + g.MapId + ")");
        }

        void Begin(SurvivalGame g)
        {
            string id = Room71.PendingRoom;
            Room71.PendingRoom = null;
            LevelDef lv = g.sim.campaignLevel;
            level = lv.id;
            if (!Room71.On(g.cfg)) { Debug.Log("[room71] start room " + id + " not applied: " + Room71.OffReason(g.cfg, Room71.Args())); return; }
            target = lv.rooms.Find(r => r.id == id);
            if (target == null) { Debug.Log("[room71] start room " + id + ": level " + level + " has no such room (its rooms: " + string.Join(" ", lv.rooms.ConvertAll(r => r.id)) + ")"); return; }
            runAt = holdAt = Time.realtimeSinceStartup;
            N29Fixes.ForceDoorFreeHold = true;
            stage = 1;
        }

        void OnRoomEnter(RoomEvent e)
        {
            if (stage != 2 || e == null || target == null || e.id != target.id) return;
            stage = 0;
            Debug.Log("[room71] room entered " + e.id + " '" + e.name + "' " + Room71.Seconds(Time.realtimeSinceStartup - runAt) + " s after the run began (unranked: DOORFREE)");
        }

        void OnDisable()
        {
            if (stage == 1) N29Fixes.ForceDoorFreeHold = false;
        }
    }
}
