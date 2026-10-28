// NIGHT15 U3-28.4 (builder P): the map pipeline's menu items, thin wrappers over Skygrind.Game.MapAuthoring:
//   SKYGRIND/Map/Export                         the selected (or only) MapRoot -> Design/maps/<id>.json + its Player copy,
//                                               navGrid / zoneParts built by NavGridBuilder.ExportFor
//   SKYGRIND/Map/Export (Keep Imported Nav)     the same, keeping the imported file's nav grid while the geometry is unchanged
//   SKYGRIND/Map/Import...                      a map JSON -> a new scene of markers (save it where you like)
//   SKYGRIND/Map/Validate                       the selected MapRoot's Design/maps/<id>.json (else a file you pick) ->
//                                               Reports/MAP-<id>-validate.md
//   SKYGRIND/Map/Round Trip survival.json       import PortExport/maps/survival.json, export it (nav built) to
//                                               Temp/MapRoundTrip/survival.json, compare the sim keys, validate it
//                                               (Reports/MAP-survival-validate.md); the markers are removed after
// Each logs one "[map] ..." line (and its warnings). RoundTripSurvival() and the others return that line, for an eval.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;
using Skygrind.Game.MapAuthoring;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Skygrind.EditorTools
{
    public static class MapMenu
    {
        static string Repo => Path.GetDirectoryName(Application.dataPath);

        static GameConfig Config() => GameConfig.Load(Path.Combine(GameConfig.PortExportDir(Repo, Application.streamingAssetsPath, true), "config.json"));

        static string Relative(string path)
        {
            string full = Path.GetFullPath(path).Replace('\\', '/'), repo = Path.GetFullPath(Repo).Replace('\\', '/') + "/";
            return full.StartsWith(repo) ? full.Substring(repo.Length) : full;
        }

        /// <summary>The selected MapRoot (or its parent's), else the only one in the open scenes, else null.</summary>
        static MapRoot SelectedRoot()
        {
            if (Selection.activeGameObject != null)
            {
                MapRoot r = Selection.activeGameObject.GetComponentInParent<MapRoot>();
                if (r != null) return r;
            }
            MapRoot[] all = Object.FindObjectsByType<MapRoot>(FindObjectsSortMode.None);
            return all.Length == 1 ? all[0] : null;
        }

        static void LogWarnings(IEnumerable<string> warnings)
        {
            foreach (string w in warnings) Debug.LogWarning("[map] " + w);
        }

        [MenuItem("SKYGRIND/Map/Export")]
        public static string Export() => ExportSelected(MapNavMode.Build);

        [MenuItem("SKYGRIND/Map/Export (Keep Imported Nav)")]
        public static string ExportKeepNav() => ExportSelected(MapNavMode.KeepIfGeometryUnchanged);

        static string ExportSelected(MapNavMode nav)
        {
            MapRoot root = SelectedRoot();
            if (root == null)
            {
                Debug.LogError("[map] export: select a MapRoot (or keep one MapRoot in the open scenes)");
                return "no MapRoot";
            }
            var t = System.Diagnostics.Stopwatch.StartNew();
            MapExportResult r = MapExport.Write(root, Config(), Repo, nav);
            AssetDatabase.Refresh();
            LogWarnings(r.warnings);
            string line = $"[map] export {root.mapId}: {Relative(r.path)} and {Relative(r.playerCopyPath)} ({r.text.Length} chars, nav grid {(r.navKept ? "kept from " + root.importedFrom : "built")}, {r.warnings.Count} warnings, {t.ElapsedMilliseconds} ms)";
            Debug.Log(line);
            return line;
        }

        [MenuItem("SKYGRIND/Map/Import...")]
        public static void Import()
        {
            string path = EditorUtility.OpenFilePanel("Import a map JSON", Path.Combine(Repo, "Design", "maps"), "json");
            if (string.IsNullOrEmpty(path)) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ImportFile(path);
        }

        /// <summary>Builds the markers of a map file in the active scene (no new scene, no prompt) and selects the root.</summary>
        public static string ImportFile(string path)
        {
            var json = PortData.Obj(PortData.LoadJson(path));
            MapRoot root = MapImport.Build(json, Relative(path), out List<string> warnings);
            Selection.activeGameObject = root.gameObject;
            LogWarnings(warnings);
            int markers = root.GetComponentsInChildren<MapMarker>().Length;
            string line = $"[map] import {Relative(path)}: MapRoot '{root.mapId}' with {markers} markers, {warnings.Count} warnings (save the scene to keep it)";
            Debug.Log(line);
            return line;
        }

        [MenuItem("SKYGRIND/Map/Validate")]
        public static string Validate()
        {
            MapRoot root = SelectedRoot();
            string path = root != null ? Path.Combine(Repo, UnityTuning.MapDesignDir, root.mapId + ".json") : null;
            if (path == null || !File.Exists(path)) path = EditorUtility.OpenFilePanel("Validate a map JSON", Path.Combine(Repo, "Design", "maps"), "json");
            if (string.IsNullOrEmpty(path)) return "no map";
            return ValidateFile(path);
        }

        /// <summary>Validates a map file: Reports/MAP-&lt;id&gt;-validate.md.</summary>
        public static string ValidateFile(string path)
        {
            var json = PortData.Obj(PortData.LoadJson(path));
            var t = System.Diagnostics.Stopwatch.StartNew();
            (string report, int findings) = MapValidate.Write(Config(), json, Repo, Relative(path));
            string line = $"[map] validate {Relative(path)}: {findings} findings -> {Relative(report)} ({t.ElapsedMilliseconds} ms)";
            if (findings == 0) Debug.Log(line);
            else Debug.LogWarning(line);
            return line;
        }

        [MenuItem("SKYGRIND/Map/Round Trip survival.json")]
        public static string RoundTripSurvival()
        {
            string source = Path.Combine(Repo, "PortExport", "maps", "survival.json");
            string target = Path.Combine(Repo, UnityTuning.MapRoundTripDir, "survival.json");
            var json = PortData.Obj(PortData.LoadJson(source));
            GameConfig cfg = Config();
            MapRoot root = MapImport.Build(json, Relative(source), out List<string> warnings);
            try
            {
                var t = System.Diagnostics.Stopwatch.StartNew();
                MapExportResult r = MapExport.WriteTo(root, cfg, target, MapNavMode.Build, Repo);
                var back = PortData.Obj(JsJson.Parse(r.text));
                List<MapKeyCompare> keys = MapRoundTrip.Compare(json, back);
                int same = keys.FindAll(k => k.same).Count;
                bool whole = MapRoundTrip.SameWithoutDiagnostics(json, back, out int at);
                (string report, int findings) = MapValidate.Write(cfg, back, Repo, Relative(target));
                LogWarnings(warnings);
                LogWarnings(r.warnings);
                string line = $"[map] round trip survival.json: {same}/{keys.Count} sim keys byte-identical, the rest but the diagnostics {(whole ? "identical" : $"DIFFERENT at char {at}")}, nav grid built, "
                    + $"{warnings.Count + r.warnings.Count} warnings -> {Relative(target)}; validate: {findings} findings -> {Relative(report)} ({t.ElapsedMilliseconds} ms)";
                if (same == keys.Count && whole) Debug.Log(line);
                else Debug.LogError(line + "\n" + MapRoundTrip.Describe(keys.FindAll(k => !k.same)));
                return line;
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }
    }
}
