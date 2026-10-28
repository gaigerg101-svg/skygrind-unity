// One-shot setup for the spike scenes, run from the CLI (unity command eval) or the SKYGRIND menu: the URP Lit
// material the runtime builders clone (so the shader ships in builds), the Arena and Enemies scenes, the build list,
// and Run In Background (a Play session otherwise stalls when the Editor loses focus, which also blocks the CLI).
using System.IO;
using Skygrind.Bench;
using Skygrind.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SpikeSetup
{
    const string MaterialPath = "Assets/Materials/SpikeLit.mat";
    const string ArenaScenePath = "Assets/Scenes/Arena.unity";
    const string EnemyMaterialPath = "Assets/Materials/SpikeEnemy.mat";
    const string EnemiesScenePath = "Assets/Scenes/Enemies.unity";

    [MenuItem("SKYGRIND/Create Arena Scene")]
    public static string CreateArenaScene()
    {
        PlayerSettings.runInBackground = true;
        Material mat = BaseMaterial();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var go = new GameObject("Spike Arena");
        var boot = go.AddComponent<ArenaBootstrap>();
        boot.baseMaterial = mat;
        EditorSceneManager.SaveScene(scene, ArenaScenePath);
        AddToBuild(ArenaScenePath, first: true);
        AssetDatabase.SaveAssets();
        return $"saved {ArenaScenePath}; runInBackground={PlayerSettings.runInBackground}";
    }

    /// <summary>The enemy scale bench scene (Task 4/5): the bench on the web training map, an instanced enemy material.</summary>
    [MenuItem("SKYGRIND/Create Enemies Scene")]
    public static string CreateEnemiesScene()
    {
        PlayerSettings.runInBackground = true;
        Material mat = BaseMaterial();
        var enemy = AssetDatabase.LoadAssetAtPath<Material>(EnemyMaterialPath);
        if (enemy == null)
        {
            enemy = new Material(mat) { color = new Color(0.85f, 0.22f, 0.18f), enableInstancing = true };
            AssetDatabase.CreateAsset(enemy, EnemyMaterialPath);
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var go = new GameObject("Enemy Bench");
        var bench = go.AddComponent<EnemyBench>();
        bench.baseMaterial = mat;
        bench.enemyMaterial = enemy;
        go.AddComponent<EnemyBenchHud>();
        EditorSceneManager.SaveScene(scene, EnemiesScenePath);
        AddToBuild(EnemiesScenePath, first: false);
        AssetDatabase.SaveAssets();
        return $"saved {EnemiesScenePath}";
    }

    static Material BaseMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat != null) return mat;
        Directory.CreateDirectory(Path.GetDirectoryName(MaterialPath));
        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        AssetDatabase.CreateAsset(mat, MaterialPath);
        return mat;
    }

    static void AddToBuild(string path, bool first)
    {
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        list.RemoveAll(s => s.path == path);
        var entry = new EditorBuildSettingsScene(path, true);
        if (first) list.Insert(0, entry);
        else list.Add(entry);
        EditorBuildSettings.scenes = list.ToArray();
    }
}
