// NIGHT43 builder F1 (design.n43.feel43, a view row): the pure half of the night's feel (EditMode tests: Assets/Tests/EditMode/Game/Feel43Tests.cs).
// - Fusion43Burst: one FUSION's timeline on frame time (the motes' converge share, the core flash, the rings, the word's punch, the camera
//   kick, when the trauma lets go: on the first running frame after the freeze, combat-web.md B7), no engine types;
// - Feel43Math: the per-weapon hit sparks, the TORCH's ramp, the rumble word for an event, the boss-open read's gate;
// - Feel43Haptics: the rumble words played one step after another on the existing Rumble (Haptics22's way, through Core's IHapticsSink), with
//   a priority (a light shot never cuts a FUSION's crack) and a log of every word asked (the reel prints it; a pad is not needed to log).
// View only: nothing here is read by the sim; no allocation per frame once made (the log is a fixed ring).
using System;
using System.Globalization;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>One FUSION's picture over time: t = real seconds since the event (the freeze does not stop it).</summary>
    public struct Fusion43Burst
    {
        public bool live;
        public double age; // s since the event (frame time)
        public bool big; // a boss or a big kind: sizes x Fusion43BossScale
        public bool traumaDue; // the trauma is waiting for the freeze to end
        public double traumaWaited;

        public static Fusion43Burst Start(bool big) => new Fusion43Burst { live = true, age = 0, big = big, traumaDue = true, traumaWaited = 0 };

        /// <summary>A kind drawn at the big size (Fusion43BossScale, the second ring radius).</summary>
        public static bool Big(string kind) => kind == "boss" || kind == "tank" || kind == "warden" || kind == "charger";

        /// <summary>The burst a fusion43 event starts: the player's only (false for another owner or no event).</summary>
        public static bool For(Fusion43Event e, out Fusion43Burst b)
        {
            b = default;
            if (e == null || e.owner != "player") return false;
            b = Start(Big(e.kind));
            return true;
        }

        /// <summary>The motes' share of the way to the hit point (ease-in: pulled, faster as they near it): 0 at the event, 1 at ConvergeTime.</summary>
        public static double Converge(double t)
        {
            if (t <= 0) return 0;
            if (t >= UnityTuning.Fusion43ConvergeTime) return 1;
            double u = t / UnityTuning.Fusion43ConvergeTime;
            return u * u;
        }

        /// <summary>The core flash's brightness 0..1: rising over the last third of the converge, 1 at the meeting, (1 - u)^2 after it.</summary>
        public static double Core(double t)
        {
            double meet = UnityTuning.Fusion43ConvergeTime;
            if (t < meet * 0.66) return 0;
            if (t < meet) return (t - meet * 0.66) / (meet * 0.34);
            double u = (t - meet) / UnityTuning.Fusion43CoreTime;
            if (u >= 1) return 0;
            return (1 - u) * (1 - u);
        }

        /// <summary>The core's size share (a quarter at the meeting, growing to 1 with ease-out).</summary>
        public static double CoreSize(double t)
        {
            double u = Math.Max(0, Math.Min(1, (t - UnityTuning.Fusion43ConvergeTime) / UnityTuning.Fusion43CoreTime));
            double w = 1 - u;
            return 0.25 + 0.75 * (1 - w * w * w);
        }

        /// <summary>A ring's share of its radius (ease-out cubic from 0.1) and its brightness, `lag` s behind the first; (0, 0) before it starts / after it ends.</summary>
        public static (double radius, double k) Ring(double t, double lag)
        {
            double u = (t - UnityTuning.Fusion43ConvergeTime - lag) / UnityTuning.Fusion43RingTime;
            if (u < 0 || u >= 1) return (0, 0);
            double w = 1 - u;
            return (0.1 + 0.9 * (1 - w * w * w), w * w);
        }

        /// <summary>The camera kick's share now (1 at the event, ease-out cubic back to 0 over Fusion43KickTime).</summary>
        public static double Kick(double t)
        {
            if (t < 0 || t >= UnityTuning.Fusion43KickTime) return 0;
            double w = 1 - t / UnityTuning.Fusion43KickTime;
            return w * w * w;
        }

        /// <summary>The word's scale (Fusion43WordFrom -> 1, ease-out back) and alpha (1, held, then a fade); alpha 0 after.</summary>
        public static (double scale, double alpha) Word(double t)
        {
            if (t < 0) return (UnityTuning.Fusion43WordFrom, 0);
            double punch = UnityTuning.Fusion43WordPunch, hold = UnityTuning.Fusion43WordHold, fade = UnityTuning.Fusion43WordFade;
            double scale = 1;
            if (t < punch)
            {
                // ease-out back (overshoot 1.70158): from WordFrom down through 1, a touch under, settling at 1
                double u = t / punch, c1 = 1.70158, c3 = c1 + 1;
                double e = 1 + c3 * Math.Pow(u - 1, 3) + c1 * Math.Pow(u - 1, 2);
                scale = UnityTuning.Fusion43WordFrom + (1 - UnityTuning.Fusion43WordFrom) * e;
            }
            double alpha = t < punch + hold ? 1 : t < punch + hold + fade ? 1 - (t - punch - hold) / fade : 0;
            return (scale, alpha);
        }

        /// <summary>Advance by a frame; true on the frame the trauma lets go: the first frame the freeze is over (freezeLeft <= 0), or after
        /// Fusion43TraumaWaitMax. Ends the burst at Fusion43Rest.</summary>
        public bool Advance(double frameDt, double freezeLeft)
        {
            if (!live) return false;
            age += frameDt;
            bool release = false;
            if (traumaDue)
            {
                traumaWaited += frameDt;
                if (freezeLeft <= 0 || traumaWaited >= UnityTuning.Fusion43TraumaWaitMax)
                {
                    traumaDue = false;
                    release = true;
                }
            }
            if (age >= UnityTuning.Fusion43Rest && !traumaDue) live = false;
            return release;
        }
    }

    public static class Feel43Math
    {
        /// <summary>A weapon's hit read: { sparks, star size m, spark width m } (UnityTuning.Gun43*).</summary>
        public static float[] GunHit(string weapon)
        {
            switch (weapon)
            {
                case "revolver": return UnityTuning.Gun43Revolver;
                case "shotgun": return UnityTuning.Gun43Shotgun;
                case "nailgun": return UnityTuning.Gun43Nailgun;
                case "rail": return UnityTuning.Gun43Rail;
                case "torch": return UnityTuning.Gun43Torch;
                default: return UnityTuning.Gun43Default;
            }
        }

        /// <summary>Is this a gun's hit (not the gauntlet, the punch, a wave's share or another owner's)?</summary>
        public static bool IsGunHit(HitEvent e) =>
            e != null && e.owner == "player" && !e.melee && !e.shock && e.weapon != "gauntlet" && e.weapon != "punch" && e.weapon != "boss";

        /// <summary>The TORCH's ramp: Torch43RampFrom at the first tick, 1 after Torch43RampTime of a hold.</summary>
        public static double TorchRamp(double heldSeconds)
        {
            double u = Math.Max(0, Math.Min(1, heldSeconds / UnityTuning.Torch43RampTime));
            return UnityTuning.Torch43RampFrom + (1 - UnityTuning.Torch43RampFrom) * u;
        }

        /// <summary>The rumble word a `fire` (payload: the weapon) asks for: the THUD for the THUNDERCLAP, the BUZZ for the TORCH, a light tick otherwise.</summary>
        public static string FireWord(string weapon)
        {
            switch (weapon)
            {
                case "shotgun": return Feel43Haptics.Thud;
                case "torch": return Feel43Haptics.Buzz;
                case "rail":
                case "rocket": return Feel43Haptics.Rail;
                case null: return null;
                default: return Feel43Haptics.Light;
            }
        }

        /// <summary>A boss hit reads as OPEN: the boss bar says open (N43BossState), or the WICK's own core hit (BossEvent.core); damage > 0.</summary>
        public static bool BossOpenHit(bool barValid, bool barOpen, bool core, double damage) => damage > 0 && ((barValid && barOpen) || core);
    }

    /// <summary>The rumble words on Rumble (Core's IHapticsSink): one word at a time, step after step, by priority; every word asked is logged.</summary>
    public sealed class Feel43Haptics
    {
        public const string Crack = "crack", Release = "release", Thud = "thud", Rail = "rail", Buzz = "buzz", Dash = "dash", Light = "light";

        /// <summary>The words in the log's and the tests' order.</summary>
        public static readonly string[] Names = { Crack, Release, Thud, Rail, Buzz, Dash, Light };

        public static double[] Pattern(string word)
        {
            switch (word)
            {
                case Crack: return UnityTuning.Rumble43Crack;
                case Release: return UnityTuning.Rumble43Release;
                case Thud: return UnityTuning.Rumble43Thud;
                case Rail: return UnityTuning.Rumble43Rail;
                case Buzz: return UnityTuning.Rumble43Buzz;
                case Dash: return UnityTuning.Rumble43Dash;
                case Light: return UnityTuning.Rumble43Light;
                default: return null;
            }
        }

        /// <summary>A running word is cut only by an equal or stronger one: crack 3, release 2, thud / rail / buzz 1, dash / light 0.</summary>
        public static int Priority(string word) => word == Crack ? 3 : word == Release ? 2 : word == Thud || word == Rail || word == Buzz ? 1 : 0;

        public struct Entry
        {
            public string word;
            public double at, low, high, ms;
            public bool played; // false: asked with no pad in use (or a stronger word running)
        }

        readonly IHapticsSink sink;
        readonly Entry[] log = new Entry[UnityTuning.Rumble43LogMax];
        int logCount, logNext;
        string running;
        double[] steps;
        int stepIndex;
        double stepLeft, clock;

        public Feel43Haptics(IHapticsSink sink) => this.sink = sink;

        public string RunningName => running;
        public int LogCount => logCount;

        /// <summary>The n-th oldest kept entry.</summary>
        public Entry LogAt(int n) => log[(logNext - logCount + n + log.Length * 2) % log.Length];

        public void ClearLog() => logCount = logNext = 0;

        /// <summary>Ask for a word: played when the sink can play and no stronger word runs; logged either way. True when it started.</summary>
        public bool Fire(string word)
        {
            double[] p = Pattern(word);
            if (p == null || p.Length < 3) return false;
            bool can = sink != null && sink.CanPlay;
            bool blocked = running != null && Priority(running) > Priority(word);
            bool play = can && !blocked;
            Log(word, p, play);
            if (!can)
            {
                running = null;
                steps = null;
                return false;
            }
            if (blocked) return false;
            running = word;
            steps = p;
            stepIndex = 0;
            PlayStep();
            return true;
        }

        void Log(string word, double[] p, bool played)
        {
            log[logNext] = new Entry { word = word, at = clock, low = p[0], high = p[1], ms = TotalMs(p), played = played };
            logNext = (logNext + 1) % log.Length;
            if (logCount < log.Length) logCount++;
        }

        /// <summary>A word's whole length (ms).</summary>
        public static double TotalMs(double[] p)
        {
            double ms = 0;
            for (int i = 2; i < p.Length; i += 3) ms += p[i];
            return ms;
        }

        /// <summary>Once a rendered frame with the real frame time.</summary>
        public void Tick(double frameDt)
        {
            clock += frameDt;
            if (running == null) return;
            if (sink == null || !sink.CanPlay)
            {
                running = null;
                steps = null;
                return;
            }
            stepLeft -= frameDt;
            while (running != null && stepLeft <= 0)
            {
                stepIndex++;
                if (stepIndex * 3 >= steps.Length)
                {
                    running = null;
                    steps = null;
                    return;
                }
                double carry = stepLeft;
                PlayStep();
                stepLeft += carry;
            }
        }

        void PlayStep()
        {
            double strong = steps[stepIndex * 3], weak = steps[stepIndex * 3 + 1], ms = steps[stepIndex * 3 + 2];
            stepLeft = ms / 1000;
            sink.Play(strong, weak, ms);
        }

        /// <summary>The log as one line: `word low/high ms[ (no pad)]` per word asked ("rumble43: none" when empty).</summary>
        public string LogLine()
        {
            if (logCount == 0) return "rumble43: none";
            CultureInfo ci = CultureInfo.InvariantCulture;
            var sb = new StringBuilder("rumble43:");
            for (int n = 0; n < logCount; n++)
            {
                Entry e = LogAt(n);
                sb.Append(' ').Append(e.word).Append(' ').Append(e.low.ToString("0.00", ci)).Append('/').Append(e.high.ToString("0.00", ci))
                    .Append(' ').Append(e.ms.ToString("0", ci)).Append("ms").Append(e.played ? "" : " (not played)").Append(';');
            }
            return sb.ToString();
        }
    }
}
