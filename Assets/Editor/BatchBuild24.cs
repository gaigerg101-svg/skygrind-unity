// NIGHT24 (the lead): a Player build from a batch-mode Editor, for a project COPY (a git worktree) when the live Editor cannot be
// reached (its test run held the main thread for over an hour). Never run against the project the live Editor has open.
//   Unity.exe -batchmode -nographics -projectPath <copy> -executeMethod Skygrind.EditorTools.BatchBuild24.Build -buildOut <exe> [-dev] -quit -logFile <f>
// Scenes: Survival, then Arena (CLAUDE.md "Player build"). The data copy (SKYGRIND/Sync Port Data) runs first, so StreamingAssets is current.
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Skygrind.EditorTools
{
    public static class BatchBuild24
    {
        public static void Build()
        {
            string[] args = Environment.GetCommandLineArgs();
            string outPath = "Builds/batch/SKYGRIND.exe";
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-buildOut") outPath = args[i + 1];
            bool dev = Array.IndexOf(args, "-dev") >= 0;
            try
            {
                Debug.Log("[batch24] sync port data: " + SurvivalSetup.SyncPortData());
            }
            catch (Exception e)
            {
                Debug.LogError("[batch24] sync failed: " + e);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath)));
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Survival.unity", "Assets/Scenes/Arena.unity" },
                locationPathName = outPath,
                target = BuildTarget.StandaloneWindows64,
                options = dev ? BuildOptions.Development : BuildOptions.None,
            };
            BuildReport r = BuildPipeline.BuildPlayer(opts);
            Debug.Log($"[batch24] build {r.summary.result}: {r.summary.totalErrors} errors, {r.summary.totalWarnings} warnings, {r.summary.totalSize / (1024 * 1024)} MB -> {outPath} in {r.summary.totalTime}");
            EditorApplication.Exit(r.summary.result == BuildResult.Succeeded ? 0 : 2);
        }
    }
}
