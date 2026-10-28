// NIGHT13 U3-26.4 (agent P): the perf gate's runtime switches, so one Player build A/Bs every fix without a rebuild.
// Every fix is ON by default; `-skygrindPerfOff <comma list>` turns the named ones OFF (`all`: every one), and OFF is
// exactly the u3-n12 behaviour. A Web build reads `?skygrindPerfOff=a,b` from its URL. Parsed once, on first use (no
// dependency on SurvivalGame); Describe() is the one line the boot logs. The fixes (each in its own file):
//   lights          LightPools: each slot's (and the flash light's) final state applied once a frame, no Light disabled
//                   and re-enabled within a frame
//   volume          PostFx: the Volume overrides written only when a value changed, and both cameras' Volume framework
//                   "Via Scripting" (the stack updated only when something changed)
//   mpb             EnemyLook.Tint: a part's MaterialPropertyBlock set only when its colour, flash or rim changed
//   xray            EnemyLook: the x-ray second material attached only while ENEMY X-RAY is on
//   overlayShadows  Viewmodel: the overlay camera renders no shadows (its gun casts none and receives none today: the
//                   overlay's own shadow map is the empty one)
//   opaqueTexture   Look.Apply: the main camera's Opaque Texture off (no project shader samples _CameraOpaqueTexture)
//   gcSlice         QualityRuntime.Boot: the incremental GC's time slice UnityTuning.GcSliceNs (Unity's default 3 ms)
//   warmup          Warmup.Run: the recorded GraphicsStateCollection warmed up at boot (no file: nothing)
//   quality         QualityRuntime: quality.js's remainder (AUTO's benchmark, the render scale, the per-preset shadows)
using System;
using System.Text;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The perf fixes, by switch (the order of PerfSwitches.Names).</summary>
    public enum PerfFix
    {
        Lights,
        Volume,
        Mpb,
        Xray,
        OverlayShadows,
        OpaqueTexture,
        GcSlice,
        Warmup,
        Quality,
    }

    public static class PerfSwitches
    {
        /// <summary>The command-line names, in PerfFix order.</summary>
        public static readonly string[] Names = { "lights", "volume", "mpb", "xray", "overlayShadows", "opaqueTexture", "gcSlice", "warmup", "quality" };

        const string Flag = "-skygrindPerfOff";
        static bool[] off; // null until parsed
        static string described;
        static string unknown; // names given that match no switch (reported by Describe)

        /// <summary>Is fix `f` on (the default)? Parses the command line on first use.</summary>
        public static bool On(PerfFix f)
        {
            if (off == null) Parse(Environment.GetCommandLineArgs(), Application.absoluteURL);
            return !off[(int)f];
        }

        /// <summary>
        /// Reads `-skygrindPerfOff a,b,c` from `args` (or `skygrindPerfOff=a,b` from a Web build's `url`). Names are
        /// matched ignoring case; `all` turns every fix off. Called once by On; public for a check that sets its own.
        /// </summary>
        public static void Parse(string[] args, string url)
        {
            off = new bool[Names.Length];
            described = null;
            unknown = null;
            string list = null;
            if (args != null)
            {
                int i = Array.IndexOf(args, Flag);
                if (i >= 0 && i + 1 < args.Length) list = args[i + 1];
            }
            if (list == null && !string.IsNullOrEmpty(url))
            {
                int u = url.IndexOf(Flag.Substring(1) + "=", StringComparison.Ordinal);
                if (u >= 0)
                {
                    int start = u + Flag.Length; // past "skygrindPerfOff="
                    int end = url.IndexOf('&', start);
                    list = Uri.UnescapeDataString(end < 0 ? url.Substring(start) : url.Substring(start, end - start));
                }
            }
            if (string.IsNullOrEmpty(list)) return;
            foreach (string raw in list.Split(','))
            {
                string name = raw.Trim();
                if (name.Length == 0) continue;
                if (string.Equals(name, "all", StringComparison.OrdinalIgnoreCase))
                {
                    for (int k = 0; k < off.Length; k++) off[k] = true;
                    continue;
                }
                int idx = -1;
                for (int k = 0; k < Names.Length; k++)
                {
                    if (string.Equals(name, Names[k], StringComparison.OrdinalIgnoreCase)) idx = k;
                }
                if (idx >= 0) off[idx] = true;
                else unknown = unknown == null ? name : unknown + "," + name;
            }
        }

        /// <summary>One line for the boot log: "perf: lights on, volume off, ..." (and any name that matched nothing).</summary>
        public static string Describe()
        {
            if (off == null) Parse(Environment.GetCommandLineArgs(), Application.absoluteURL);
            if (described != null) return described;
            var sb = new StringBuilder("perf: ");
            for (int k = 0; k < Names.Length; k++)
            {
                if (k > 0) sb.Append(", ");
                sb.Append(Names[k]).Append(off[k] ? " off" : " on");
            }
            if (unknown != null) sb.Append("; unknown in ").Append(Flag).Append(": ").Append(unknown);
            return described = sb.ToString();
        }
    }
}
