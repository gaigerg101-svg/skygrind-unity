// NIGHT13 U3-26.12: the version stamp at run time: "SKYGRIND <Application.version> (<commit>)", where <commit> is the short
// hash Editor/BuildInfo.cs wrote into the built Player's data folder (UnityTuning.BuildStampFile; "-dirty" when tracked
// files differed from the commit), "editor" in the Editor and "unknown" when the file is missing (a build made without
// BuildInfo, a Web build: its data folder is a URL). Written into the Player log by the first script line (before the
// first scene loads; Unity's own engine lines come before it), shown on TITLE (UiReport), and at the top of each run log
// and of COPY REPORT. The web game had no build to stamp (Vite served the source).
using System;
using System.IO;
using UnityEngine;

namespace Skygrind.Game
{
    public static class BuildStamp
    {
        static string hash, line;

        /// <summary>The commit's short hash ("abc1234", "abc1234-dirty"), "editor" or "unknown".</summary>
        public static string Hash
        {
            get
            {
                Read();
                return hash;
            }
        }

        /// <summary>"SKYGRIND 0.1.0 (abc1234)": the product, Application.version and the hash.</summary>
        public static string Line
        {
            get
            {
                Read();
                return line;
            }
        }

        /// <summary>The stamp line for a version and a hash.</summary>
        public static string Format(string version, string hash) => $"{UnityTuning.StampProduct} {version} ({hash})";

        /// <summary>The stamp file's hash: its first non-blank line, trimmed; "unknown" when there is none.</summary>
        public static string ParseHash(string fileText)
        {
            if (string.IsNullOrEmpty(fileText)) return "unknown";
            foreach (string raw in fileText.Split('\n'))
            {
                string s = raw.Trim();
                if (s.Length > 0) return s;
            }
            return "unknown";
        }

        static void Read()
        {
            if (line != null) return;
            if (Application.isEditor) hash = "editor";
            else
            {
                try
                {
                    string path = Path.Combine(Application.dataPath, UnityTuning.BuildStampFile);
                    hash = File.Exists(path) ? ParseHash(File.ReadAllText(path)) : "unknown";
                }
                catch (Exception)
                {
                    hash = "unknown"; // a data folder we can't read (a Web build's URL): never fail the boot over a stamp
                }
            }
            line = Format(Application.version, hash);
        }

        /// <summary>The Player log's first script line (and each Play entry's in the Editor).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LogStamp() => Debug.Log(Line);
    }
}
