// NIGHT15 U3-28.5: choosing a map. design.maps.list (Design/unity-config.json) names the maps TITLE's MAP row offers
// (id, label, file: PortExport/maps/ for the web's arena, Design/maps/ for a Unity-authored map). The composition is built
// for one map in Awake (the sim, the views, the nav), so a different pick reloads the scene with the map's id held in
// PendingMapId; the boot reads it, else the saved pick (UnityTuning.MapPref), else "survival". Automation (any
// -skygrind* argument: the smoke, the fingerprint, the golden check, the farm, the bench, a replay) always boots the old
// arena ("survival"), unless -skygrindMap <id> says otherwise. The old map keeps its game: its file, its fingerprint and
// its records' key ("survival").
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>The map the next boot builds (set by TITLE's MAP row before it reloads the scene); null: the saved pick.</summary>
        public static string PendingMapId;

        /// <summary>One entry of design.maps.list.</summary>
        public struct MapEntry
        {
            public string id, label, file;
            /// <summary>NIGHT24: a CAMPAIGN level (design.maps.list "campaignOnly": true): never offered on TITLE's MAP row, played through the MODE row's CAMPAIGN (and -skygrindMap).</summary>
            public bool campaignOnly;
        }

        /// <summary>The map this composition plays (SurvivalMap.name; "survival" for the web's arena).</summary>
        public string MapId => map != null ? map.name : RunSetup.DefaultMap;

        /// <summary>design.maps.list's maps whose file exists under the data root's parent (PortExport's folder's parent).</summary>
        public static List<MapEntry> MapList(GameConfig cfg, string portExportDir)
        {
            var list = new List<MapEntry>();
            string baseDir = Path.GetDirectoryName(portExportDir) ?? ".";
            if (cfg.Has("design.maps.list"))
            {
                foreach (object o in cfg.Arr("design.maps.list"))
                {
                    var d = PortData.Obj(o);
                    var e = new MapEntry { id = (string)d["id"], label = (string)d["label"], file = Path.Combine(baseDir, (string)d["file"]), campaignOnly = d.TryGetValue("campaignOnly", out object co) && co is bool cb && cb };
                    if (File.Exists(e.file)) list.Add(e);
                }
            }
            if (list.Count == 0 || list.FindIndex(m => m.id == RunSetup.DefaultMap) < 0)
                list.Insert(0, new MapEntry { id = RunSetup.DefaultMap, label = UnityTuning.MapArenaLabel, file = Path.Combine(portExportDir, "maps", "survival.json") });
            return list;
        }

        /// <summary>The boot's map: automation's (the old arena, or -skygrindMap), else PendingMapId, else the saved pick.</summary>
        static MapEntry BootMap(List<MapEntry> maps)
        {
            string id = null;
            string[] args = Environment.GetCommandLineArgs();
            bool automation = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (!N29Fixes.IsAutomationArg(args[i])) continue; // NIGHT29: -skygrindOff29 and -skygrindLevelSelect29 are not automation (they boot the saved campaign level)
                automation = true;
                if (args[i] == "-skygrindMap" && i + 1 < args.Length) id = args[i + 1];
            }
            if (!automation) id = PendingMapId ?? PlayerPrefs.GetString(UnityTuning.MapPref, RunSetup.DefaultMap);
            PendingMapId = null;
            int at = maps.FindIndex(m => m.id == id);
            if (at < 0)
            {
                if (id != null && id != RunSetup.DefaultMap) Debug.LogWarning($"[skygrind] map '{id}' is not in design.maps.list or its file is missing: the arena instead");
                at = maps.FindIndex(m => m.id == RunSetup.DefaultMap);
            }
            return maps[at];
        }

        /// <summary>TITLE's MAP row: pick a map. The same map: nothing; another: saved, then the scene reloads on it.</summary>
        public void PickMap(string id)
        {
            if (id == MapId || flow != Flow.Title) return;
            if (mapList.FindIndex(m => m.id == id) < 0) return;
            PlayerPrefs.SetString(UnityTuning.MapPref, id);
            PlayerPrefs.Save();
            PendingMapId = id;
            Debug.Log($"[skygrind] map: {MapId} -> {id} (the scene reloads)");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        List<MapEntry> mapList = new List<MapEntry>();

        /// <summary>NIGHT24: the mode the next boot starts in (set with PendingMapId when the MODE row crosses between a CAMPAIGN level and the survival maps).</summary>
        public static string PendingMode;

        /// <summary>The maps TITLE's MAP row offers: design.maps.list without the CAMPAIGN levels.</summary>
        public List<MapEntry> PickableMaps
        {
            get
            {
                var list = new List<MapEntry>();
                foreach (MapEntry m in mapList) if (!m.campaignOnly) list.Add(m);
                return list;
            }
        }

        /// <summary>This composition was built on a CAMPAIGN level.</summary>
        public bool OnCampaignMap
        {
            get
            {
                foreach (MapEntry m in mapList) if (m.id == MapId) return m.campaignOnly;
                return false;
            }
        }

        /// <summary>The design lists a CAMPAIGN level (and its file exists).</summary>
        public bool HasCampaignMap => mapList.FindIndex(m => m.campaignOnly) >= 0;

        /// <summary>MODE: CAMPAIGN. On a CAMPAIGN level already: the mode is set. Else the scene reloads on the level (the saved MAP pick is left as it was) and boots in CAMPAIGN.</summary>
        public void GoCampaign()
        {
            string startLevel = CampaignStartLevel(); // NIGHT25: the curriculum's level (the tutorial first, then where the last clear left off)
            int at = mapList.FindIndex(m => m.campaignOnly && m.id == startLevel);
            if (at < 0) at = mapList.FindIndex(m => m.campaignOnly);
            if (at < 0 || flow != Flow.Title) return;
            if (mapList[at].id == MapId)
            {
                setup.mode = RunSetup.Campaign;
                return;
            }
            PendingMapId = mapList[at].id;
            PendingMode = RunSetup.Campaign;
            Debug.Log($"[skygrind] campaign: {MapId} -> {PendingMapId} (the scene reloads)");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>MODE: SURVIVAL or THE GRIND picked while on a CAMPAIGN level: back to the saved survival map (the arena when the saved pick is a level), the scene reloads in that mode.</summary>
        public void LeaveCampaign(string mode)
        {
            if (!OnCampaignMap || flow != Flow.Title)
            {
                setup.mode = mode;
                return;
            }
            string id = PlayerPrefs.GetString(UnityTuning.MapPref, RunSetup.DefaultMap);
            int at = mapList.FindIndex(m => m.id == id && !m.campaignOnly);
            PendingMapId = at >= 0 ? id : RunSetup.DefaultMap;
            PendingMode = mode;
            Debug.Log($"[skygrind] campaign left: {MapId} -> {PendingMapId} (the scene reloads)");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>The maps TITLE offers (design.maps.list with a file).</summary>
        public IReadOnlyList<MapEntry> Maps => mapList;

        /// <summary>The records' mode key: "survival" on the web's arena (its records stay where they were), "survival@id" else.</summary>
        public static string RecordsKeyFor(string mapId) => SurvivalSim.RecordsKeyFor(mapId);
    }
}
