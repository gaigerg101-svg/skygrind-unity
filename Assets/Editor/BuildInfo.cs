// NIGHT13 U3-26.12: the version stamp's build side. Before a Player build it reads the commit (`git rev-parse --short
// HEAD`) and whether tracked files differ from it (`git status --porcelain`: "-dirty"); after the build it writes that one
// line into the built Player's data folder (UnityTuning.BuildStampFile beside the Player's own data, where
// Application.dataPath points), which BuildStamp reads at run time. Nothing inside the repo is written, so `git status`
// stays as it was after a build (Builds/ is ignored).
// The commit is read before the build (after PortDataBuildSync's copies), so the build's own rewrites (Mobile_RPAsset)
// can't mark it dirty. Tracked files that Unity itself keeps modified on this machine and that are never committed
// (CLAUDE.md's git rules) don't count as dirty; every tracked change is still listed in the build log.
// A build without git (or outside a repository) is stamped "nogit"; a target with no data folder here (Web) gets no file
// (BuildStamp says "unknown").
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Skygrind.Game;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public sealed class BuildInfo : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    // After PortDataBuildSync (0): the StreamingAssets copies it makes are part of what the stamp describes.
    public int callbackOrder => 10;

    /// <summary>
    /// Tracked files modified on this machine by Unity and never committed (CLAUDE.md: UnityConnectSettings is never staged,
    /// Play mode rewrites the TMP fallback font, every build rewrites the Mobile URP asset until D-09): not "dirty".
    /// </summary>
    public static readonly string[] KnownLocal =
    {
        "ProjectSettings/UnityConnectSettings.asset",
        "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset",
        "Assets/Settings/Mobile_RPAsset.asset",
    };

    static string pending; // the stamp read before this build

    public void OnPreprocessBuild(BuildReport report)
    {
        pending = Capture(ProjectRoot, out string detail);
        Debug.Log($"[skygrind] build stamp: {pending}{detail}");
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        try
        {
            string stamp = pending ?? Capture(ProjectRoot, out _); // a domain reload between the two (not expected) reads it again
            pending = null;
            string dir = DataFolder(report.summary.platform, report.summary.outputPath);
            if (dir == null || !Directory.Exists(dir))
            {
                Debug.LogWarning($"[skygrind] build stamp: no data folder for {report.summary.platform} at '{report.summary.outputPath}': the Player will say 'unknown'");
                return;
            }
            string path = Path.Combine(dir, UnityTuning.BuildStampFile);
            File.WriteAllText(path, stamp + "\n", new UTF8Encoding(false));
            Debug.Log($"[skygrind] build stamp written: {path} ({stamp})");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[skygrind] build stamp not written (the build is fine; the Player will say 'unknown'): " + e.Message); // never fail a build over the stamp
        }
    }

    static string ProjectRoot => Path.GetDirectoryName(UnityEngine.Application.dataPath);

    /// <summary>
    /// The built Player's data folder (its Application.dataPath): "&lt;name&gt;_Data" beside a Windows or Linux executable, the
    /// app bundle's Contents on macOS; null for other targets.
    /// </summary>
    public static string DataFolder(BuildTarget target, string outputPath)
    {
        if (string.IsNullOrEmpty(outputPath)) return null;
        if (!Path.IsPathRooted(outputPath)) outputPath = Path.Combine(ProjectRoot, outputPath);
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneLinux64:
                return Path.Combine(Path.GetDirectoryName(outputPath) ?? "", Path.GetFileNameWithoutExtension(outputPath) + "_Data");
            case BuildTarget.StandaloneOSX:
                return Path.Combine(outputPath, "Contents");
            default:
                return null;
        }
    }

    /// <summary>
    /// The stamp for the repository at `root`: the short hash, "-dirty" when a tracked file (not KnownLocal) differs from the
    /// commit, "nogit" without git. `detail` lists the tracked changes for the build log.
    /// </summary>
    public static string Capture(string root, out string detail)
    {
        string head = Git(root, "rev-parse --short HEAD");
        if (string.IsNullOrWhiteSpace(head))
        {
            detail = " (git not found, or not a repository)";
            return "nogit";
        }
        head = head.Trim();
        string status = Git(root, "status --porcelain -z --untracked-files=no");
        if (status == null)
        {
            detail = " (git status failed: marked dirty)";
            return head + "-dirty";
        }
        List<string> all = TrackedChanges(status);
        var counted = new List<string>();
        foreach (string p in all) if (Array.IndexOf(KnownLocal, p) < 0) counted.Add(p);
        var sb = new StringBuilder();
        if (all.Count > 0)
        {
            sb.Append(" (tracked changes: ");
            for (int i = 0; i < all.Count; i++) sb.Append(i > 0 ? ", " : "").Append(all[i]).Append(Array.IndexOf(KnownLocal, all[i]) >= 0 ? " [known, not counted]" : "");
            sb.Append(')');
        }
        detail = sb.ToString();
        return counted.Count > 0 ? head + "-dirty" : head;
    }

    /// <summary>The paths `git status --porcelain -z` lists as changed (untracked "??" and ignored "!!" entries skipped).</summary>
    public static List<string> TrackedChanges(string porcelainZ)
    {
        var list = new List<string>();
        if (string.IsNullOrEmpty(porcelainZ)) return list;
        string[] parts = porcelainZ.Split('\0');
        for (int i = 0; i < parts.Length; i++)
        {
            string e = parts[i];
            if (e.Length < 4) continue;
            char x = e[0], y = e[1];
            if (x == 'R' || x == 'C' || y == 'R' || y == 'C') i++; // -z: a rename's or copy's source path follows as its own entry
            if (x == '?' || x == '!') continue;
            list.Add(e.Substring(3));
        }
        return list;
    }

    /// <summary>git's standard output, or null when git can't be run, fails, or takes over 15 s.</summary>
    static string Git(string root, string args)
    {
        try
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            };
            using (Process p = Process.Start(psi))
            {
                if (p == null) return null;
                p.ErrorDataReceived += (s, e) => { }; // drained so a chatty stderr can't block the process
                p.BeginErrorReadLine();
                string output = p.StandardOutput.ReadToEnd();
                if (!p.WaitForExit(15000))
                {
                    try
                    {
                        p.Kill();
                    }
                    catch (Exception)
                    {
                        // already gone
                    }
                    return null;
                }
                return p.ExitCode == 0 ? output : null;
            }
        }
        catch (Exception)
        {
            return null; // no git on PATH
        }
    }
}
