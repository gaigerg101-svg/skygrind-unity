// NIGHT24 removals (builder R): `remove --grep`, the contracts' grep check as a harness line. It reads every file under Assets/ and Tools/ (not
// *.md, not *.meta, not bin / obj / Temp / Library, and not this file, which holds the patterns) and counts the lines matching each pattern of
// the removed features: INSTANT REPLAY (the ring, the view, the clip, its config row), the DAILY / WEEKLY (their files, the mode constant) and the
// DAILY ghost (its class and config row). The input tape (RunTape / TapeReplay) is not among the patterns. Want 0 for each; exit 0 iff all are.
// Dispatch (Harness.N24.cs, the lead's): else if (cmd == "remove") RemoveCommand(args, ref code);
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public static partial class Harness
{
    static readonly string[] RemovedPatterns =
    {
        "Replay22", "Replay21", "replayClip", "ReplayEnsureClip", "design\\.n21\\.replay", "Daily\\.cs", "Weekly20", "GhostDaily22",
        "design\\.n22\\.ghostDaily", "RunSetup\\.Daily",
    };

    static void RemoveCommand(string[] args, ref int code)
    {
        var regs = new List<Regex>();
        foreach (string p in RemovedPatterns) regs.Add(new Regex(p));
        var counts = new int[regs.Count];
        var first = new List<string>();
        int files = 0;
        string self = "Harness.N24.Remove.cs";
        foreach (string root in new[] { "Assets", "Tools" })
        {
            if (!Directory.Exists(root)) continue;
            foreach (string f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                string n = f.Replace('\\', '/');
                if (n.EndsWith(".md") || n.EndsWith(".meta") || n.EndsWith(self)) continue;
                if (n.Contains("/bin/") || n.Contains("/obj/") || n.Contains("/Temp/") || n.Contains("/Library/") || n.Contains("/__pycache__/")) continue;
                string ext = Path.GetExtension(n).ToLowerInvariant();
                if (ext == ".png" || ext == ".jpg" || ext == ".dll" || ext == ".exe" || ext == ".sktape" || ext == ".fbx" || ext == ".asset" || ext == ".unity" || ext == ".prefab" || ext == ".ttf" || ext == ".otf" || ext == ".woff2") continue;
                string[] lines;
                try { lines = File.ReadAllLines(f); }
                catch (Exception) { continue; }
                files++;
                for (int i = 0; i < lines.Length; i++)
                    for (int r = 0; r < regs.Count; r++)
                        if (regs[r].IsMatch(lines[i]))
                        {
                            counts[r]++;
                            if (first.Count < 12) first.Add($"  {n}:{i + 1}: [{RemovedPatterns[r]}] {lines[i].Trim()}");
                        }
            }
        }
        int total = 0;
        for (int r = 0; r < regs.Count; r++)
        {
            Console.WriteLine($"remove --grep: {RemovedPatterns[r]} {counts[r]}");
            total += counts[r];
        }
        foreach (string s in first) Console.WriteLine(s.Length > 220 ? s.Substring(0, 220) : s);
        code = total == 0 ? 0 : 1;
        Console.WriteLine($"remove --grep {(total == 0 ? "PASS" : "RED")}: {total} matching lines in {files} files (want 0)");
    }
}
