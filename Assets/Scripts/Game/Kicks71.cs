// NIGHT71 builder F (F3, T-77; design.n71.kicks71, a VIEW row: no sim read, no tape bit). GG NIGHT16: "kills not crunchy"; NIGHT46's
// feel46 kicks (G2 a gun hit's view kick, G3 the kill ladder's kicks and the body kill's FOV punch-in, G4 the THUNDERCLAP shot's shake under
// its kill's) were built and left off (design.n46.feel46.numbers.kicks false) because n46-r2's blind pairs read them "same": a KESTREL body
// kill's 0.9 deg came on the same frame as the tap's own 1.2 + 1.8 deg fire kick, and every hit kicked too. This row turns them ON with new
// numbers, bigger and rarer:
//   bigger: every kill kick above the tap's fire kick (body < head < punch kept), held a little longer, the body kill's FOV punch-in larger;
//   rarer:  a hit kick at most once per hitGapS; a kill within killGapS of the last kill kick adds only what its tier has over that kick's
//           (a THUNDERCLAP multi-kill is one big kick, not three stacked); the THUNDERCLAP's own shot keeps less of its shake (G4).
// With the row on, feel46's G2-G4 run with these numbers whatever numbers.kicks says (feel46 itself must be on: -skygrindOff46 or older
// turns feel46 off and these with it). THE SWITCH is read every frame (NightRows.ViewOn first: the row is in SameRun63Math.FrameRows), so a
// same-run held frame with the row off draws no n71 kick while its stack keeps its state (the D1 contract). G4's cut is applied at the
// shot, so a held off frame keeps it (the shake's state is CameraFeel's, not this row's). Off: the row off, -skygrindOff71 or older,
// -skygrindNoKicks71 (any case). Numbers: design.n71.kicks71.numbers in both config copies (CLAUDE.md rule 5); a number missing keeps the
// row off ("numbers incomplete"), never a default typed here. Proof: the a546 reel's probe line (Feel46Probe: hit kicks, kill kicks,
// kicks71 on, merged counts), RED with the row off (0 / 0), GREEN on (> 0 / > 0).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Kicks71
    {
        public const string Row = "kicks71";
        public const string NoSwitch = "-skygrindNoKicks71";
        public const int Night = 71;
        const string RowOff = "design.n71." + Row + " off"; // constants: OffReason runs every frame, so it never allocates
        const string Incomplete = "design.n71." + Row + ".numbers incomplete";
        const string P = "design.n71." + Row + ".numbers.";

        /// <summary>The row's numbers (deg of view pitch-up / FOV; s of KickStack's attack and recover, frame time).</summary>
        public sealed class Numbers
        {
            public double hitLightDeg, hitHeavyDeg, hitGapS, hitAttackS, hitRecoverS;
            public double bodyKillDeg, headKillDeg, punchKillDeg, killGapS, killAttackS, killRecoverS;
            public double bodyKillFovDeg, fovAttackS, fovRecoverS;
            public double heavyFireKeep;
        }

        static readonly string[] Keys =
        {
            "hitLightDeg", "hitHeavyDeg", "hitGapS", "hitAttackS", "hitRecoverS",
            "bodyKillDeg", "headKillDeg", "punchKillDeg", "killGapS", "killAttackS", "killRecoverS",
            "bodyKillFovDeg", "fovAttackS", "fovRecoverS", "heavyFireKeep",
        };

        static GameConfig numbersCfg;
        static Numbers numbers;
        static string[] args;

        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>The numbers of this config (read once per config; null when one is missing or not a number).</summary>
        public static Numbers Of(GameConfig cfg)
        {
            if (cfg == null) return null;
            if (ReferenceEquals(cfg, numbersCfg)) return numbers;
            numbersCfg = cfg;
            numbers = Read(cfg);
            return numbers;
        }

        static Numbers Read(GameConfig cfg)
        {
            var v = new double[Keys.Length];
            for (int i = 0; i < Keys.Length; i++)
            {
                string path = P + Keys[i];
                if (!cfg.Has(path)) return null;
                object o = cfg.Get(path);
                if (o is double d) v[i] = d;
                else if (o is long l) v[i] = l;
                else if (o is int n) v[i] = n;
                else return null;
            }
            int k = 0;
            return new Numbers
            {
                hitLightDeg = v[k++], hitHeavyDeg = v[k++], hitGapS = v[k++], hitAttackS = v[k++], hitRecoverS = v[k++],
                bodyKillDeg = v[k++], headKillDeg = v[k++], punchKillDeg = v[k++], killGapS = v[k++], killAttackS = v[k++], killRecoverS = v[k++],
                bodyKillFovDeg = v[k++], fovAttackS = v[k++], fovRecoverS = v[k++], heavyFireKeep = v[k++],
            };
        }

        /// <summary>Why the row is off for this config and command line (null: on): the row (NightRows.ViewOn, the reel's live override first),
        /// the off switches, then its numbers.</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return RowOff;
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff71 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            if (Of(cfg) == null) return Incomplete;
            return null;
        }

        /// <summary>Is the row on now? Read every frame: never cache it (D1's same-run toggle flips ViewOn).</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>A time `now` is inside `gap` of `lastAt` (s, the view's frame clock): now &lt; lastAt + gap. Written as a sum, not
        /// now - lastAt &lt; gap: (10 + 0.35) - 10 is 0.34999999999999964 in doubles, so a hit exactly one gap later read as inside it
        /// (N71KicksTests). lastAt -infinity: never inside.</summary>
        public static bool InsideGap(double now, double lastAt, double gap) => now < lastAt + gap;

        /// <summary>A gun's hit kick (G2) with the row on: its deg, or 0 inside hitGapS of the last hit kick (rarer). `now` and `lastAt` are the
        /// view's frame clock (s).</summary>
        public static double HitKick(Numbers n, bool heavy, double now, double lastAt) =>
            n == null || InsideGap(now, lastAt, n.hitGapS) ? 0 : heavy ? n.hitHeavyDeg : n.hitLightDeg;

        /// <summary>A kill's kick (G3) with the row on: its tier's deg, or inside killGapS of the last kill kick only what the tier has over
        /// that kick's deg (0 when it has nothing more: a multi-kill is one kick, the biggest tier's).</summary>
        public static double KillKick(Numbers n, double tierDeg, double now, double lastAt, double lastDeg) =>
            n == null ? 0 : InsideGap(now, lastAt, n.killGapS) ? Math.Max(0, tierDeg - lastDeg) : tierDeg;
    }
}
