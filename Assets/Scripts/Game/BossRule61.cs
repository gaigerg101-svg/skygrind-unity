// NIGHT61 builder G (S3 "Every boss says what it wants when the fight starts"; design.n61.bossRule61, a VIEW row: no tape bit). GG 2026-10-12:
// bosses "you just teleport into these random rooms ... hard to understand what to do"; docs/PRINCIPLES.md 2: "A boss is the level's final exam
// of its verb". With the row on, each of the five campaign bosses (THE WICK, THE ICEBREAKER, THE KILN WYRM, THE LOOM, THE CROWN) says ONE line
// when its fight starts in a campaign run: what its code rewards, at most BossRule61MaxWords words, the bound button's glyph for the verb it
// names ({action} tokens, the lesson widget's resolver), on hud47's one fight line (40 px em at 1080p), in under its name card
// BossRule61DelayS after the rise whether or not the boss bar is up yet (NIGHT63 G P2: Clock / Tick below), on screen at most BossRule61MaxS,
// once per fight start (the bossRise that starts the fight arms it; showing it spends it). The words are the row's data
// (design.n61.bossRule61.numbers.lines.<boss id>, each line's _why quoting the rule it states with file:line; Research/2026-10-22-wave61/G.md). This file holds the pure rules (EditMode: N61BossRuleTests, N61BossRuleRowTests);
// UiBossRule61.cs draws. Off (the row off, -skygrindOff61 or older, -skygrindNoBossRule61): nothing is armed and hud47's fight line runs as
// NIGHT60's. The sim never reads any of it.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class BossRule61
    {
        public const string Row = "bossRule61";
        public const string NoSwitch = "-skygrindNoBossRule61";
        const string LinesKey = "design.n61.bossRule61.numbers.lines.";

        /// <summary>The five campaign bosses' bossRise ids, in campaign order: THE WICK (Wick37.cs:219), THE ICEBREAKER (Brawl40.cs:297), THE
        /// KILN WYRM (Wyrm.cs:526), THE LOOM (Loom37.cs:227), THE CROWN (Crown37.cs:202).</summary>
        public static readonly string[] Bosses = { "wick", "brawl40", "wyrm", "loom37", "crown37" };

        /// <summary>One of the five campaign bosses (THE STAG "stag37", the survival bosses and any other id: false).</summary>
        public static bool IsBoss(string id) => id != null && Array.IndexOf(Bosses, id) >= 0;

        /// <summary>A switch turns the row off: -skygrindOffNN with NN &lt;= 61 (this night or an older one) or -skygrindNoBossRule61.</summary>
        public static bool Off(string[] args) => NightRows.OffFrom(args, "-skygrindOff") <= 61 || NightRows.HasSwitch(args, NoSwitch); // NIGHT63 R1 N5: case ignored, the one rule

        /// <summary>The row's rule: on with the row on (NightRows.ViewOn) and no switch that turns it off.</summary>
        public static bool Decide(bool rowOn, string[] args) => rowOn && !Off(args);

        static int argsOff = -1; // the command line's verdict, read once (it is fixed at boot): -1 unread, 0 no switch, 1 a switch turns it off

        /// <summary>The row now: the config's design.n61.bossRule61 (read live: a reel may turn a view row on in memory) and this process's
        /// switches (read once). Called at a boss's rise, never per frame.</summary>
        public static bool On(GameConfig cfg)
        {
            if (argsOff < 0)
            {
                string[] args;
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
                argsOff = Off(args) ? 1 : 0;
            }
            return argsOff == 0 && Decide(NightRows.ViewOn(cfg, Row), null);
        }

        /// <summary>The line `bossId` says as written ({action} tokens unresolved), or null: not one of the five, no config, no line, an empty one.</summary>
        public static string Line(GameConfig cfg, string bossId)
        {
            if (cfg == null || !IsBoss(bossId) || !cfg.Has(LinesKey + bossId)) return null;
            string s = cfg.Str(LinesKey + bossId);
            return string.IsNullOrEmpty(s) ? null : s;
        }

        /// <summary>The words of a line as written: runs of non-space characters ("{fire}" is one word: one glyph; "embers," is one word).</summary>
        public static int Words(string line)
        {
            if (string.IsNullOrEmpty(line)) return 0;
            int n = 0;
            bool inWord = false;
            for (int i = 0; i < line.Length; i++)
            {
                bool space = char.IsWhiteSpace(line[i]);
                if (!space && !inWord) n++;
                inWord = !space;
            }
            return n;
        }

        /// <summary>Does the line name a button: a closed, named {action} token (the lesson widget turns it into the bound key or pad glyph)?</summary>
        public static bool HasGlyph(string line)
        {
            if (string.IsNullOrEmpty(line)) return false;
            for (int o = line.IndexOf('{'); o >= 0; o = line.IndexOf('{', o + 1))
            {
                int c = line.IndexOf('}', o);
                if (c > o + 1) return true;
                if (c < 0) return false;
            }
            return false;
        }

        /// <summary>s at full alpha: the text system's 2.0 s + 0.3 s a word (hud47's Hud47LineHoldS / Hud47LineHoldPerWord), cut so the fade in,
        /// the hold and the fade out stay within BossRule61MaxS.</summary>
        public static double HoldS(int words)
        {
            double want = UnityTuning.Hud47LineHoldS + UnityTuning.Hud47LineHoldPerWord * Math.Max(0, words);
            double room = UnityTuning.BossRule61MaxS - UnityTuning.BossRule61InS - UnityTuning.BossRule61OutS;
            return Math.Max(0, Math.Min(want, room));
        }

        /// <summary>s the line is on screen: the fade in, the hold, the fade out.</summary>
        public static double OnScreenS(int words) => UnityTuning.BossRule61InS + HoldS(words) + UnityTuning.BossRule61OutS;

        /// <summary>The line's alpha `age` s after it began, with hold `hold`: 0 before it, rising over BossRule61InS, 1 through the hold, falling
        /// over BossRule61OutS, 0 after.</summary>
        public static float Alpha(double age, double hold)
        {
            double inS = UnityTuning.BossRule61InS, outS = UnityTuning.BossRule61OutS;
            if (age < 0) return 0f;
            if (age < inS) return (float)(age / inS);
            if (age < inS + hold) return 1f;
            if (age < inS + hold + outS) return (float)(1.0 - (age - inS - hold) / outS);
            return 0f;
        }

        /// <summary>The line's scale `age` s after it began: BossRule61ScaleFrom eased to 1 over the fade-in (hud47's fight line's scale-in).</summary>
        public static float Scale(double age)
        {
            float from = UnityTuning.BossRule61ScaleFrom;
            if (age <= 0) return from;
            if (age >= UnityTuning.BossRule61InS) return 1f;
            return from + (1f - from) * (float)(age / UnityTuning.BossRule61InS);
        }

        // ---- NIGHT63 G (P2, the line on time; Logs/n63/slate.json P2): the line's clock from its boss's rise, pure (the HUD ticks one each frame,
        // UiBossRule61.cs; N63BossRuleTimeTests tick it with a planted boss bar). NIGHT62 measured the line in 2.43 s after THE KILN WYRM's rise
        // (Logs/feel/n62-p0 b61k): the HUD began it only with the boss bar up, and the wyrm has no bar through its 2.0 s emerge (Wyrm.cs phase
        // "intro"; SurvivalSim.N43.Bosses.cs). Now the line comes in BossRule61DelayS after the rise, with the name card, whether or not the bar
        // is up; the bar matters only to end it: once the fight's bar has been up, its going down (the fight over) cuts the line at once.

        /// <summary>The rule line's clock: armed by its boss's rise, in BossRule61DelayS later, then its fade in, hold and fade out.</summary>
        public struct Clock
        {
            /// <summary>s since the rise armed the line (-1: nothing armed).</summary>
            public double wait;
            /// <summary>s since the line came in (-1: not showing).</summary>
            public double age;
            /// <summary>s at full alpha, set as it comes in (HoldS of its words).</summary>
            public double hold;
            /// <summary>The boss bar was up on a frame since the rise armed the line (the fight's bar: its going down after that ends the line).</summary>
            public bool barSeen;

            public static Clock Idle => new Clock { wait = -1, age = -1 };
            public bool Armed => wait >= 0;
            public bool Showing => age >= 0;
        }

        /// <summary>What one frame's Tick did.</summary>
        public enum Beat { Idle, Waiting, Began, Showing, Ended, Cut }

        /// <summary>NIGHT63 G P2b (the P2 critic's problem 5: THE ICEBREAKER's bar hint stood above the rule line): is the boss bar's hint drawn?
        /// Not while the rule line holds the place (one instruction at the fight's start); otherwise when it has text, as Text33.Set shows it
        /// (NIGHT62's bar, unchanged outside the hold).</summary>
        public static bool HintShown(bool lineHolds, string hint) => !lineHolds && !string.IsNullOrEmpty(hint);

        /// <summary>A rise arms the line. A line still showing is the caller's to cut first (its proof line); the next Tick drops its age.</summary>
        public static void Arm(ref Clock c)
        {
            c.wait = 0;
            c.barSeen = false;
        }

        /// <summary>One HUD frame of `frameDt` s, the boss bar up or not, for a line of `words` words: Waiting until BossRule61DelayS after the
        /// rise, Began on the frame it comes in (age 0: the caller draws it from the next frame's age), Showing through its fades and hold, Ended
        /// after them; Cut on the first frame the bar is down after it was up since the rise.</summary>
        public static Beat Tick(ref Clock c, double frameDt, bool barUp, int words)
        {
            if (c.wait < 0 && c.age < 0) return Beat.Idle;
            if (barUp) c.barSeen = true;
            if (c.wait >= 0)
            {
                c.age = -1;
                c.wait += frameDt;
                if (c.wait < UnityTuning.BossRule61DelayS) return Beat.Waiting;
                c.wait = -1;
                c.age = 0;
                c.hold = HoldS(words);
                return Beat.Began;
            }
            if (!barUp && c.barSeen) { c.age = -1; return Beat.Cut; }
            c.age += frameDt;
            if (c.age >= UnityTuning.BossRule61InS + c.hold + UnityTuning.BossRule61OutS) { c.age = -1; return Beat.Ended; }
            return Beat.Showing;
        }
    }
}
