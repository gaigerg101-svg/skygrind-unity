// NIGHT63 builder D (slate D1, same-run SPOT pairs; the red team's finding 2, Research/2026-10-22-forge63/redteam.md): the pure parts of the reel's
// same-run toggle (EditMode: N63SameRunGameTests). The reel (FeelReel.N63Same.cs) takes `-skygrindSameRun63 <night.row>`; at each shot it writes
// the frame with the row on (the shot as always), holds the sim and every clock for one more frame with the row OFF (NightRows.ReelOverride),
// writes that frame to <side>-off/ under the same name, then restores. Why: NIGHT62 P1's pair came from two runs that drift (nearest enemy 15.3 vs
// 15.2 m; the kb3 ENTRY pair, before any lamp, changes 139,791 px), so the spotter named a hint line, not the row (L-129).
using System;
using System.Globalization;
using System.IO;

namespace Skygrind.Game
{
    public static class SameRun63Math
    {
        /// <summary>The reel's option: `-skygrindSameRun63 &lt;night.row&gt;` (n61.exitFrame61; case of the switch ignored).</summary>
        public const string Switch = "-skygrindSameRun63";

        /// <summary>The planted rows (no config row: the reel forces them on for its run). noop63: nothing draws it, so its pair must be 0 changed
        /// px (the hold itself is clean). plant63: a UnityTuning.Same63PlantPx square (SameRun63View) drawn while it reads on: exactly 100 px.</summary>
        public const string NoopRow = "noop63", PlantRow = "plant63";

        /// <summary>How a row's OFF is shown in the held frame: Frame (its view re-reads the row every frame: NightRows.Live / ViewOn), UiRoot
        /// (decided at an event: OFF is the same frame with its UI root hidden, the red team's rule; bossRule61), Planted, or Unclaimed (any other
        /// row: its view may read the row once, so its pair proves nothing and the proof line says so).</summary>
        public enum Kind { Unclaimed, Frame, UiRoot, Planted }

        /// <summary>The rows whose views re-read the row every frame (exitFrame61: ExitFrame61.N61V.cs; the n63 rows by their builders' contract;
        /// NIGHT64's A1 lists its three rows here so no two builders edit this line, Research/2026-10-23-forge64/redteam.md 11).</summary>
        public static readonly string[] FrameRows = { "exitFrame61", "bossMotion63", "bossTell63", "bossColours63", "colours63", "goal64", "order64", "fightLine64", "voice71", "words71", "wyrm71", "cyan71", "kicks71", "pace71", "wash71", "marks71", "glow71", "calm72", "lit72", "fix72", "palette72", "decor72", "body72", "spring72", "bossfx72", "btext72", "goal72", "cam72", "kiln73", "core73", "reveal74", "weak74", "target74", "kill74", "crown74", "flick74", "spring74", "ice74", "pads74" };

        /// <summary>The rows judged OFF by their UI root hidden (bossRule61: hud47's fight line holding the rule line).</summary>
        public static readonly string[] UiRootRows = { "bossRule61" };

        /// <summary>The option's row: the name after `night.` (null when absent: why null; null when malformed: why says what is wrong).</summary>
        public static string Row(string[] args, out string why)
        {
            why = null;
            if (args == null) return null;
            int at = -1;
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], Switch, StringComparison.OrdinalIgnoreCase)) { at = i; break; }
            if (at < 0) return null;
            string v = at + 1 < args.Length ? args[at + 1] : null;
            if (string.IsNullOrEmpty(v) || v[0] == '-') { why = Switch + " wants <night.row> (e.g. n61.exitFrame61), got " + (v ?? "nothing"); return null; }
            int dot = v.IndexOf('.');
            if (dot < 0) { why = Switch + " " + v + ": no night (write n" + Math.Max(0, NightRowsName(v)) + "." + v + ")"; return null; }
            string night = v.Substring(0, dot), row = v.Substring(dot + 1);
            if (row.Length == 0 || row.IndexOf('.') >= 0) { why = Switch + " " + v + ": the row is one name after the night"; return null; }
            int nn = Skygrind.Core.NightRows.NightKey(night), rn = NightRowsName(row);
            if (nn < 0 || rn < 0 || nn != rn) { why = Switch + " " + v + ": the night (" + night + ") is not the row's own (its name ends with " + rn + ")"; return null; }
            return row;
        }

        static int NightRowsName(string row) => Skygrind.Core.NightRows.NightOfName(row);

        /// <summary>The kind of `row` (Unclaimed for null or any row D1 does not claim).</summary>
        public static Kind KindOf(string row)
        {
            if (row == null) return Kind.Unclaimed;
            if (row == NoopRow || row == PlantRow) return Kind.Planted;
            if (Array.IndexOf(UiRootRows, row) >= 0) return Kind.UiRoot;
            if (Array.IndexOf(FrameRows, row) >= 0) return Kind.Frame;
            return Kind.Unclaimed;
        }

        /// <summary>The value after the switch `name` (null when absent, last, or followed by another switch).</summary>
        public static string ArgAfter(string[] args, string name)
        {
            if (args == null) return null;
            for (int i = 0; i + 1 < args.Length; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1].Length > 0 && args[i + 1][0] != '-' ? args[i + 1] : null;
            return null;
        }

        /// <summary>Where a shot's off capture goes: under the reel's root (reel.sh's Logs/feel/&lt;tag&gt;/&lt;side&gt;), the same path under
        /// &lt;side&gt;-off; elsewhere (an Editor eval, no root) the shot's own folder + "-off". Full paths with this platform's separators.</summary>
        public static string OffDir(string reelRoot, string shotDir) => TwinDir(reelRoot, shotDir, "-off");

        /// <summary>NIGHT63 review S4 (one name, one frame): where a shot's held ON frame goes, beside the off half: the same path under
        /// &lt;side&gt;-on (an Editor eval: the shot's own folder + "-on"). The shot's own PNG in &lt;side&gt;/ stays the reel's frame, the one its
        /// stats (Shot29) and sidecars (lamp60's door box, colour56's rects) describe.</summary>
        public static string OnDir(string reelRoot, string shotDir) => TwinDir(reelRoot, shotDir, "-on");

        static string TwinDir(string reelRoot, string shotDir, string suffix)
        {
            string shot = Path.GetFullPath(shotDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!string.IsNullOrEmpty(reelRoot))
            {
                string root = Path.GetFullPath(reelRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (string.Equals(shot, root, StringComparison.OrdinalIgnoreCase)) return root + suffix;
                if (shot.Length > root.Length && shot.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                    && (shot[root.Length] == Path.DirectorySeparatorChar || shot[root.Length] == Path.AltDirectorySeparatorChar))
                    return root + suffix + shot.Substring(root.Length);
            }
            return shot + suffix;
        }

        /// <summary>The planted square in screen px from the top-left: {x, y, w, h}, `at` from the corner, pushed inside a small screen.</summary>
        public static int[] PlantRect(int screenW, int screenH, int at, int px)
        {
            int x = Math.Max(0, Math.Min(at, screenW - px)), y = Math.Max(0, Math.Min(at, screenH - px));
            return new[] { x, y, px, px };
        }

        /// <summary>NIGHT63 review S4: the proof line naming both halves of the pair: "... | on &lt;side&gt;-on/... | off &lt;side&gt;-off/...".</summary>
        public static string ToggleLine(string file, string nightRow, Kind kind, bool held, string state, string onPath, string offPath, int shot)
        {
            string l = ToggleLine(file, nightRow, kind, held, state, offPath, shot);
            int at = l.LastIndexOf(" | off ", StringComparison.Ordinal);
            return l.Substring(0, at) + " | on " + onPath + l.Substring(at);
        }

        /// <summary>The proof line (L-089): one per shot, printed after the off capture is written.</summary>
        public static string ToggleLine(string file, string nightRow, Kind kind, bool held, string state, string offPath, int shot)
        {
            string k = kind == Kind.Frame ? "frame (its view re-reads the row every frame)" : kind == Kind.UiRoot ? "ui root (the row's UI root hidden in the off frame)"
                : kind == Kind.Planted ? "planted" : "unclaimed (its view may read the row once: a 0 px pair proves nothing about it)";
            return "[same63] toggle fired: shot " + shot.ToString(CultureInfo.InvariantCulture) + " " + file + " | row " + nightRow + " on -> off -> on, " + k
                + " | sim held " + (held ? "yes" : "NO") + " (" + state + ") | off " + offPath;
        }

        /// <summary>The held clock's bookkeeping (FeelClock reads it): from Begin (the end of the frame whose on capture was written) to End (the
        /// end of the held frame, its off capture written) the frame and the unscaled time read as the hold's start and no time passes; after
        /// End they run on with the held span skipped, so nothing that ages on them sees the extra frame.</summary>
        public sealed class HoldClock
        {
            bool holding;
            int startFrame, heldFrames;
            double startT, heldT;

            public bool Holding => holding;
            public int HeldFrames => heldFrames;
            public double HeldTime => heldT;

            public void Begin(int frame, double t)
            {
                if (holding) return;
                holding = true;
                startFrame = frame;
                startT = t;
            }

            public void End(int frame, double t)
            {
                if (!holding) return;
                holding = false;
                heldFrames += Math.Max(0, frame - startFrame);
                heldT += Math.Max(0, t - startT);
            }

            /// <summary>The frame as the views see it.</summary>
            public int Frame(int frame) => (holding ? startFrame : frame) - heldFrames;

            /// <summary>The unscaled time as the views see it.</summary>
            public double Time(double t) => (holding ? startT : t) - heldT;

            /// <summary>The frame's delta as the views see it (0 while holding).</summary>
            public double Delta(double dt) => holding ? 0 : dt;
        }
    }
}
