// NIGHT71 builder F (LEFTOVER L4, T-105 "the kill's wash"; design.n71.wash71, a VIEW row: no sim read, no tape bit). The wash meter (a153's
// T-111 item: the centre 20 % box's luma rise over f-2, bar +0.25, GG w30 "no full-screen effect") read +0.425 shipped and +0.315 with
// polish53's killWash cuts (EXT53; killWash ships OFF, DECIDE53 D-15), never GREEN. The f3-f4 source, read frame by frame from EXT53's saved
// crops (Research/2026-10-26-wave71/F/L4/wash-classes.txt): with killWash on, the foundry box's rise from f1 to f3 is the room lit red-orange
// (warm-lit pixels 0.011 -> 0.214 of the box's luma; white 0.242 -> 0.209), and the reel's own curve keeps that light through the kill's
// freeze and fades it after (f5 +0.236 .. f8 +0.126): every kill pops a light in the enemy's colour (effects.js pop: fx.popIntensity 60 over
// fx.popDistance 9 m for fx.popTime 0.12 s, k^2; LightPools.Light54's pop light), and it fades on WORLD time, so the freeze (world time 0)
// holds it at its peak for the whole hold (a kill's 10 steps; longer under kill55 / clear55 holds).
// With the row on (and polish53 on): (1) polish53's killWash item is live with its shipped numbers (whiteFrames 2, silhouetteFrames 2,
// burstK 0.5, ringInWash 0: Polish53.Item); (2) a KILL's pop light fades on frame time from the kill (popFrameClock 1: the freeze no longer
// holds it; a blast's pop and the impact glow unchanged) at popK x its peak. The kill still pops on its first frames; only the held part goes.
// Not found from code within the slice: the near-white ring round the reticle on f2-f6 of the same crops (fx49's kill ring was skipped there,
// 9 frames; KillConfirm draws nothing while fx49 is on), about 0.2 of the f3 box: left as it is, named in the card.
// THE SWITCH is read every frame (NightRows.ViewOn first; the row is in SameRun63Math.FrameRows): a same-run held off frame shows the pop at its
// off peak; its clock is state (held frames run no clock). Off: the row off, -skygrindOff71 or older, -skygrindNoWash71 (any case),
// polish53 off (the a153 reel's OFF side: Polish53.Force false); a number missing keeps it off. Numbers: design.n71.wash71.numbers (both
// config copies). Proof: a153's T-111 line (ON peak <= +0.25 with the row on; the line names wash71 and the kill pops it clocked).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Wash71
    {
        public const string Row = "wash71";
        public const string NoSwitch = "-skygrindNoWash71";
        public const int Night = 71;
        const string RowOff = "design.n71." + Row + " off";
        const string Incomplete = "design.n71." + Row + ".numbers incomplete";
        const string P = "design.n71." + Row + ".numbers.";

        /// <summary>The row's numbers: popFrameClock (1: a kill's pop light fades on frame time from the kill, 0: world time as before) and
        /// popK (x the kill pop's peak, fx.popIntensity).</summary>
        public sealed class Numbers
        {
            public bool popFrameClock;
            public double popK;
        }

        /// <summary>The reel's probe: kill pops this row clocked on frame time, since the Player started.</summary>
        public static int KillPopsClocked;

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

        /// <summary>The numbers of this config (read once per config; null when one is missing, not a number or out of range).</summary>
        public static Numbers Of(GameConfig cfg)
        {
            if (cfg == null) return null;
            if (ReferenceEquals(cfg, numbersCfg)) return numbers;
            numbersCfg = cfg;
            numbers = null;
            if (cfg.Has(P + "popFrameClock") && cfg.Has(P + "popK") && cfg.Get(P + "popFrameClock") is double clock && cfg.Get(P + "popK") is double k
                && k >= 0 && k <= 1)
                numbers = new Numbers { popFrameClock = clock != 0, popK = PopKOverride(Args(), k) };
            return numbers;
        }

        /// <summary>NIGHT72 L (the wash71 leftover: a153's T-111 read +0.255 against the bar +0.25 with popK 1): `-skygrindWash71PopK=<k>` (0..1; or the number as the next argument)
        /// replaces popK for this run, so one build tries 0.8 and 0.6 without a config edit; the bar never moves. No switch, or a bad number: the config's popK.</summary>
        public static double PopKOverride(string[] argv, double fromConfig)
        {
            const string flag = "-skygrindWash71PopK";
            if (argv == null) return fromConfig;
            for (int i = 0; i < argv.Length; i++)
            {
                string v = null;
                if (argv[i] != null && argv[i].StartsWith(flag + "=", StringComparison.OrdinalIgnoreCase)) v = argv[i].Substring(flag.Length + 1);
                else if (string.Equals(argv[i], flag, StringComparison.OrdinalIgnoreCase) && i + 1 < argv.Length) v = argv[i + 1];
                if (v != null && double.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double k) && k >= 0 && k <= 1) return k;
            }
            return fromConfig;
        }

        /// <summary>Why the row is off (null: on): the row (NightRows.ViewOn, the reel's live override first), the off switches, its numbers.</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return RowOff;
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff71 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            if (Of(cfg) == null) return Incomplete;
            return null;
        }

        /// <summary>Is the row on now (its own rule)? Read every frame: never cache it.</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>Does it draw: the row on and polish53 on (the a153 reel's OFF side forces polish53 off, NIGHT52's kill).</summary>
        public static bool Live(GameConfig cfg) => Polish53.On(cfg) && On(cfg);

        /// <summary>A kill pop's clock step and peak this frame: the frame's dt and popK x the peak with the row live and popFrameClock; the
        /// world's dt and the peak as before otherwise.</summary>
        public static double PopStep(GameConfig cfg, bool kill, double worldDt, double frameDt, double peak, out double peakOut)
        {
            peakOut = peak;
            if (!kill || !Live(cfg)) return worldDt;
            Numbers n = Of(cfg);
            if (n == null) return worldDt;
            peakOut = peak * n.popK;
            return n.popFrameClock ? frameDt : worldDt;
        }
    }
}
