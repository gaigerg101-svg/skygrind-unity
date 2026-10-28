// NIGHT55 K kill55 (design.n55.kill55, a VIEW row: NightRows.ViewOn(cfg, "kill55"); -skygrindOff55 or under turns it off: NIGHT54's kills
// exactly). GG: "kills not crunchy" (NIGHT16); the veteran critic's #2 (Research/2026-10-20-wave55/critic-veteran.md): "a kill is a 3-frame pause
// on an undamaged pill, and NIGHT54 made it shorter". Three parts, pictures and pacing only (no Core state, no sim step's result):
//   1. THE LADDER (Kill55Hold, this file): kills each within numbers.chainGapS of PLAY time (sim seconds: freezes excluded) of the one before
//      are a chain; the chain's 1st kill keeps the impact rules' freeze exactly (move54's 0.05 s), the 2nd holds numbers.secondS, the 3rd and
//      later numbers.chainS, never over the impact director's kill cap (move54's killCapS 0.12). Through the existing hitstop path only: the
//      impact rules (Core/ImpactRules.cs) decide every freeze as before; this layer reads their outcome after each step and either lengthens the
//      granted freeze (one hold: the largest of the two, contiguous) or, when the rules skipped a chain kill (the shared 0.5 s gap), holds the
//      world itself for that kill, inside its own rolling-second budget (numbers.perSecondS, counted with the rules' own frozen time). HIT FREEZE
//      OFF: nothing; LIGHT: x design.impact.settings.lightScale. A kill off a chain (a354's 0.483-0.492 s spacing) is NIGHT54's.
//   2. THE DEBRIS (Kill55Debris.cs): every kill throws shell shards that land and stay (a resting ring, the oldest recycled first).
//   3. THE DAMAGE (Kill55Damage.cs): a hit husk shows a crack on its shell, a hole once it is below half health (marks on the existing model).
// The loop's lines are in SurvivalGame.N55K.cs. Every pacing number is the row's (design.n55.kill55.numbers); the pictures' in UnityTuning.N55K.cs.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Kill55
    {
        public const string Numbers = "design.n55.kill55.numbers.";

        /// <summary>The row (a view row: NIGHT54's kills when off).</summary>
        public static bool On(GameConfig cfg) => NightRows.ViewOn(cfg, "kill55") && !NoKill55Arg;

        /// <summary>NIGHT58 G (G1): `-skygrindNoKill55` turns this row alone off (the a258 / a355 reels' off side on the same build; -skygrindOff55
        /// would also turn wyrm55 / brief55 / wick55 / grade55 off).</summary>
        static bool NoKill55Arg => noKill55Arg ??= NightRows.HasSwitch(Environment.GetCommandLineArgs(), "-skygrindNoKill55"); // NIGHT63 R1 N5: case ignored, the one rule
        static bool? noKill55Arg;

        /// <summary>The reels' override of the row (null: the config and the command line).</summary>
        public static bool? Force;

        /// <summary>The row as the game reads it (the reel's Force first).</summary>
        public static bool Live(GameConfig cfg) => Force ?? On(cfg);

        /// <summary>The hold a kill of chain index `k` (1 = the chain's first) asks: 0 for the first (the impact rules' own freeze stands), else
        /// secondS for the 2nd, chainS for the 3rd and later, never over `cap` (the impact director's kill cap).</summary>
        public static double Ladder(int k, double secondS, double chainS, double cap)
        {
            if (k <= 1) return 0;
            double s = k == 2 ? secondS : chainS;
            return cap > 0 && s > cap ? cap : s;
        }

        /// <summary>The next chain index: a kill within `gap` s of play time of the last kill continues the chain, else starts one.</summary>
        public static int NextChain(int chain, double lastKillT, double nowT, double gap) =>
            chain > 0 && nowT - lastKillT <= gap + 1e-9 ? chain + 1 : 1;
    }

    /// <summary>
    /// NIGHT55 K kill55 (1): the chain ladder's hold, pure (Tests/EditMode/Game/N55KillTests.cs). The game calls Kill(simT) for each kill of the
    /// player's inside a step, StepEnded(...) after the step with the impact rules' outcome, and Frame(...) once a frame for the world's share.
    /// Real seconds (the impact rules' clock) for the hold and the budget; play seconds (sim time) for the chain.
    /// </summary>
    public sealed class Kill55Hold
    {
        public readonly double chainGapS, secondS, chainS, perSecondS, minHoldS, capS;

        // the chain
        int chain;
        double lastKillT = double.NegativeInfinity;
        int stepChain, stepKills;
        // the hold
        double extPending, holdLeft, prevRulesLeft;
        // this layer's own holds (real clock): a ring for the rolling second
        const int Ring = 64;
        readonly double[] ringAt = new double[Ring], ringS = new double[Ring];
        int ringHead, ringCount;

        /// <summary>Per kill (the reel's probe): the last Probe kills, oldest first through ProbeAt.</summary>
        public const int Probe = 32;
        public struct KillRec
        {
            public double simT, realT, rulesS, extraS, askS;
            public int chain;
            public string cause, note;
        }
        readonly KillRec[] recs = new KillRec[Probe];
        int recHead, recCount;
        public int KillsSeen, Extended, Rescued, Skipped, RunSkipped;
        public double ExtraSeconds;

        /// <summary>NIGHT57 G (G3): numbers.runSpeed (m/s): at or over this horizontal speed a chain kill asks no extra hold (infinity: never).</summary>
        public readonly double runSpeed;

        public Kill55Hold(double chainGapS, double secondS, double chainS, double perSecondS, double minHoldS, double capS, double runSpeed = double.PositiveInfinity)
        {
            this.runSpeed = runSpeed;
            this.chainGapS = chainGapS;
            this.secondS = secondS;
            this.chainS = chainS;
            this.perSecondS = perSecondS;
            this.minHoldS = minHoldS;
            this.capS = capS;
        }

        /// <summary>From the row's numbers; `capS` the impact director's kill cap (its config: move54's view when that row is on).</summary>
        public static Kill55Hold From(GameConfig cfg, double capS) => new Kill55Hold(cfg.Num(Kill55.Numbers + "chainGapS"), cfg.Num(Kill55.Numbers + "secondS"),
            cfg.Num(Kill55.Numbers + "chainS"), cfg.Num(Kill55.Numbers + "perSecondS"), cfg.Num(Kill55.Numbers + "minHoldS"), capS,
            cfg.Has(Kill55.Numbers + "runSpeed") ? cfg.Num(Kill55.Numbers + "runSpeed") : double.PositiveInfinity); // NIGHT57 G (G3)

        /// <summary>The chain's index now (0: none yet).</summary>
        public int Chain => chain;

        /// <summary>The hold this layer runs now (real s left, the extension waiting included).</summary>
        public double HoldLeft => holdLeft + extPending;

        public int RecCount => recCount;

        /// <summary>The i-th kill recorded, oldest first.</summary>
        public KillRec ProbeAt(int i) => recs[(recHead - recCount + i + Probe * 2) % Probe];

        /// <summary>A new run / the way to TITLE: no chain, no hold, the budget and the probe cleared.</summary>
        public void Reset()
        {
            chain = 0;
            lastKillT = double.NegativeInfinity;
            stepChain = stepKills = 0;
            extPending = holdLeft = prevRulesLeft = 0;
            ringHead = ringCount = 0;
            recHead = recCount = 0;
            KillsSeen = Extended = Rescued = Skipped = RunSkipped = 0;
            ExtraSeconds = 0;
        }

        /// <summary>A kill of the player's at play time `simT` (inside a step, or between frames: Frame forgets those).</summary>
        public void Kill(double simT)
        {
            chain = Kill55.NextChain(chain, lastKillT, simT, chainGapS);
            lastKillT = simT;
            if (chain > stepChain) stepChain = chain;
            stepKills++;
            KillsSeen++;
        }

        /// <summary>The frozen real seconds of this layer's own holds inside [from, to].</summary>
        public double OwnIn(double from, double to)
        {
            double sum = 0;
            for (int k = 0; k < ringCount; k++)
            {
                int i = (ringHead - 1 - k + Ring) % Ring;
                double a = ringAt[i], b = a + ringS[i];
                if (b <= from) break;
                double lo = a > from ? a : from, hi = b < to ? b : to;
                if (hi > lo) sum += hi - lo;
            }
            return sum;
        }

        void Own(double at, double s)
        {
            ringAt[ringHead] = at;
            ringS[ringHead] = s;
            ringHead = (ringHead + 1) % Ring;
            if (ringCount < Ring) ringCount++;
        }

        void Rec(int k, double simT, double clock, double rulesS, double extraS, double askS, string cause, string note)
        {
            recs[recHead] = new KillRec { simT = simT, realT = clock, rulesS = rulesS, extraS = extraS, askS = askS, chain = k, cause = cause, note = note };
            recHead = (recHead + 1) % Probe;
            if (recCount < Probe) recCount++;
        }

        /// <summary>
        /// After each sim step. `froze`: the impact rules began a freeze in it (`granted` s, cause `cause`); `skippedKill`: they skipped a kill
        /// tier's request; `rulesFrozenLastSecond`: their budgeted frozen time in [clock - 1, clock]; `scale`: 0 for HIT FREEZE OFF, the light
        /// scale for LIGHT, else 1. Returns true when this layer's own hold begins now (the loop stops stepping, as for a freeze).
        /// </summary>
        public bool StepEnded(double clock, double simT, bool froze, double granted, string cause, double rulesFrozenLastSecond, double scale, bool running = false)
        {
            int k = stepChain;
            stepChain = 0;
            stepKills = 0;
            if (froze) prevRulesLeft = granted; // the freeze beginning now: the frame it ends in knows how much of that frame it took
            if (k <= 0) return false;
            // NIGHT57 G (G3): a kill while the player RUNS (horizontal speed >= numbers.runSpeed, the game passes it) keeps the impact rules' own
            // freeze (move54's): the chain still counts, only its extra hold is not asked, so a running chain keeps move54's smooth camera (a354's
            // jitter 0.191 off vs 0.230 with the ladder at NIGHT56); a chain while standing or strafing slowly holds as before (a355)
            if (running && k > 1) { RunSkipped++; Rec(k, simT, clock, froze ? granted : 0, 0, 0, froze ? cause : "skipped", "running: NIGHT54's"); return false; }
            double ask = Kill55.Ladder(k, secondS, chainS, capS) * scale;
            if (ask <= 0)
            {
                Rec(k, simT, clock, froze ? granted : 0, 0, 0, froze ? cause : "skipped", k <= 1 ? "first" : "hit freeze off");
                return false;
            }
            // the rolling second: the rules' frozen time, this layer's, and the rules' freeze beginning now
            double used = rulesFrozenLastSecond + OwnIn(clock - 1, clock) + (froze ? granted : 0);
            double left = perSecondS - used;
            if (froze)
            {
                double ext = ask - granted;
                if (ext <= 1e-9) { Rec(k, simT, clock, granted, 0, ask, cause, "rules' freeze >= the ladder"); return false; }
                if (ext > left) ext = Math.Max(0, left);
                if (ext <= 1e-9) { Skipped++; Rec(k, simT, clock, granted, 0, ask, cause, "over budget"); return false; }
                extPending = ext;
                Own(clock + granted, ext);
                Extended++;
                ExtraSeconds += ext;
                Rec(k, simT, clock, granted, ext, ask, cause, "extended");
                return false;
            }
            double hold = Math.Min(ask, left);
            if (hold < minHoldS)
            {
                Skipped++;
                Rec(k, simT, clock, 0, 0, ask, "skipped", "over budget");
                return false;
            }
            holdLeft = hold;
            Own(clock, hold);
            Rescued++;
            ExtraSeconds += hold;
            Rec(k, simT, clock, 0, hold, ask, "kill55", "held (the rules skipped it)");
            return true;
        }

        /// <summary>
        /// Once a frame while the sim runs, after the impact rules' BeginFrame (`rulesLeft`: their freeze left now; `share`: the world's share so
        /// far). An extension starts when the rules' freeze ends (inside this frame: on the part of the frame they left running). Returns the share.
        /// </summary>
        public double Frame(double frameDt, double rulesLeft, double share)
        {
            stepChain = 0; // a kill between frames (outside a step) earns no hold
            stepKills = 0;
            double avail = frameDt;
            if (extPending > 0 && rulesLeft <= 0)
            {
                // the rules' freeze ended this frame after prevRulesLeft s of it (or before it: 0)
                avail = Math.Max(0, frameDt - Math.Min(prevRulesLeft, frameDt));
                holdLeft += extPending;
                extPending = 0;
            }
            prevRulesLeft = rulesLeft;
            if (holdLeft <= 0 || frameDt <= 0) return share;
            if (share <= 0) return share; // the rules (or another source) hold the world this frame anyway
            if (holdLeft >= avail)
            {
                holdLeft -= avail;
                return 0;
            }
            double run = avail > 1e-12 ? (avail - holdLeft) / avail : 1;
            holdLeft = 0;
            return share * run;
        }
    }
}
