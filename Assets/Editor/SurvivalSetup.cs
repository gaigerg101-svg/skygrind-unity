// NIGHT7 setup for the Survival scene, run from the CLI (unity command eval) or the SKYGRIND menu: the material assets
// the runtime copies (so their shaders ship in builds), the Survival scene (a camera + the SurvivalGame bootstrap; the
// map is built at load from PortExport), the build list (Survival first), and Sync Port Data (copies the PortExport
// files a Player reads into StreamingAssets).
using System.IO;
using Skygrind.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SurvivalSetup
{
    const string ScenePath = "Assets/Scenes/Survival.unity";
    const string MatDir = "Assets/Materials/Survival";

    static Material Mat(string name, string shader)
    {
        string path = $"{MatDir}/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader s = Shader.Find(shader);
        if (s == null) throw new System.Exception($"shader {shader} not found");
        if (m == null)
        {
            Directory.CreateDirectory(MatDir);
            m = new Material(s);
            AssetDatabase.CreateAsset(m, path);
        }
        else if (m.shader != s) m.shader = s;
        return m;
    }

    [MenuItem("SKYGRIND/Create Survival Scene")]
    public static string CreateSurvivalScene()
    {
        PlayerSettings.runInBackground = true;
        var stone = Mat("Stone", "SKYGRIND/Stone");
        var neon = Mat("Neon", "Universal Render Pipeline/Unlit");
        var field = Mat("Field", "SKYGRIND/Field");
        var boundary = Mat("Boundary", "SKYGRIND/Field");
        var lit = Mat("Lit", "Universal Render Pipeline/Lit");
        var sky = Mat("Sky", "SKYGRIND/Sky");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) Object.DestroyImmediate(l.gameObject); // Look makes the key light
        var go = new GameObject("SKYGRIND Survival");
        var game = go.AddComponent<SurvivalGame>();
        game.stoneMaterial = stone;
        game.neonMaterial = neon;
        game.fieldMaterial = field;
        game.boundaryMaterial = boundary;
        game.litMaterial = lit;
        game.skyMaterial = sky;
        game.acidMaterial = Mat("Acid", "SKYGRIND/Acid");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuild(ScenePath, 0);
        AssetDatabase.SaveAssets();
        return $"saved {ScenePath}; build list: {string.Join(", ", System.Array.ConvertAll(EditorBuildSettings.scenes, s => s.path))}";
    }

    /// <summary>
    /// NIGHT8: material assets added after the scene was made (Acid), assigned to the Survival scene's SurvivalGame and
    /// saved, without rebuilding the scene.
    /// </summary>
    [MenuItem("SKYGRIND/Add New Survival Materials")]
    public static string AddMaterials()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var game = Object.FindFirstObjectByType<SurvivalGame>();
        if (game == null) return $"no SurvivalGame in {ScenePath}";
        game.acidMaterial = Mat("Acid", "SKYGRIND/Acid");
        EditorUtility.SetDirty(game);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return $"acid {AssetDatabase.GetAssetPath(game.acidMaterial)} on {ScenePath}";
    }

    /// <summary>
    /// Copies what a Player reads (config.json, maps/survival.json) into StreamingAssets/PortExport. NIGHT9 U3-04: with
    /// the oracle, also every golden vector set, every trajectory and the maps they run on, so a Windows Player's
    /// -skygrindGolden checks what the Editor's tests check. The oracle copies are gitignored (PortExport/ is the
    /// tracked source; the copies would duplicate about 16 MB); PortDataBuildSync runs this before every build.
    /// </summary>
    [MenuItem("SKYGRIND/Sync Port Data")]
    public static string SyncPortData() => SyncPortData(true);

    public static string SyncPortData(bool withOracle)
    {
        string src = Path.GetFullPath("PortExport");
        string dst = Path.GetFullPath("Assets/StreamingAssets/PortExport");
        Directory.CreateDirectory(Path.Combine(dst, "maps"));
        File.Copy(Path.Combine(src, "config.json"), Path.Combine(dst, "config.json"), true);
        File.Copy(Path.Combine(src, "maps", "survival.json"), Path.Combine(dst, "maps", "survival.json"), true);
        int oracle = 0;
        foreach (string folder in new[] { "vectors", "trajectories" })
        {
            string to = Path.Combine(dst, folder);
            if (Directory.Exists(to)) Directory.Delete(to, true);
            if (File.Exists(to + ".meta")) File.Delete(to + ".meta");
            if (!withOracle) continue;
            Directory.CreateDirectory(to);
            foreach (string f in Directory.GetFiles(Path.Combine(src, folder), "*.json"))
            {
                File.Copy(f, Path.Combine(to, Path.GetFileName(f)), true);
                oracle++;
            }
        }
        foreach (string map in new[] { "spire.json", "training.json", "spire-duel-navgrid.json" }) // NIGHT14: the duel graph's towers grid (the navGraph set)
        {
            string to = Path.Combine(dst, "maps", map);
            if (File.Exists(to)) File.Delete(to);
            if (File.Exists(to + ".meta")) File.Delete(to + ".meta");
            if (!withOracle) continue;
            File.Copy(Path.Combine(src, "maps", map), to, true);
            oracle++;
        }
        // NIGHT8 U2-07: the Unity design config, beside PortExport in StreamingAssets (GameConfig.DesignPath).
        string design = Path.GetFullPath(Path.Combine("Design", "unity-config.json"));
        string designDst = Path.GetFullPath("Assets/StreamingAssets/Design");
        Directory.CreateDirectory(designDst);
        File.Copy(design, Path.Combine(designDst, "unity-config.json"), true);
        // NIGHT15 U3-28.5: the Unity-authored maps (Design/maps/<id>.json, byte-identical copies; the *.plan.json design
        // sources and the README stay out of the Player).
        int maps = 0;
        string mapsSrc = Path.GetFullPath(Path.Combine("Design", "maps"));
        if (Directory.Exists(mapsSrc))
        {
            string mapsDst = Path.Combine(designDst, "maps");
            Directory.CreateDirectory(mapsDst);
            foreach (string f in Directory.GetFiles(mapsSrc, "*.json"))
            {
                if (f.EndsWith(".plan.json", System.StringComparison.OrdinalIgnoreCase)) continue;
                File.Copy(f, Path.Combine(mapsDst, Path.GetFileName(f)), true);
                maps++;
            }
        }
        // NIGHT24: the campaign levels (Design/levels/<id>.json) and the boss tables (Design/bosses/*.json), the same way.
        foreach (string sub in new[] { "levels", "bosses" })
        {
            string subSrc = Path.GetFullPath(Path.Combine("Design", sub));
            if (!Directory.Exists(subSrc)) continue;
            string subDst = Path.Combine(designDst, sub);
            Directory.CreateDirectory(subDst);
            foreach (string f in Directory.GetFiles(subSrc, "*.json"))
            {
                if (f.EndsWith(".plan.json", System.StringComparison.OrdinalIgnoreCase)) continue;
                File.Copy(f, Path.Combine(subDst, Path.GetFileName(f)), true);
                maps++;
            }
        }
        AssetDatabase.Refresh();
        return $"synced {src} -> {dst} (oracle: {oracle} files); {design} -> {designDst} (+ {maps} map / level / boss file(s))";
    }

    static void AddToBuild(string path, int index)
    {
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        list.RemoveAll(s => s.path == path);
        list.Insert(System.Math.Min(index, list.Count), new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
