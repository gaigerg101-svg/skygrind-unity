// NIGHT22 U3-36.8 (builder M): HAPTICS (design.n22.haptics), the pure half. Distinct pad patterns for the shockwave, the ARCLINE's
// attach and yank, a kill and DOWN, played one step after another on the existing Rumble (Game/Input/Rumble.cs, through the sink
// below), with a log of every step that was played (event, low / high motor value, ms) for the reel. Render side only: the
// sim never reads this, nothing here touches the Mover or the fingerprint, and there is no audio.
// - The events keep the web's names: "shockwave" (N20Events.Shockwave), "ropeAttach", "arclinePull", "kill", "death" (owner "player").
// - A pattern is the config's flat [strong, weak, ms] triples (design.n22.haptics.patterns.<name>): strong is the low-frequency motor,
//   weak the high-frequency one; a step of 0, 0 is a pause.
// - Silence: when the sink cannot play (VIBRATION off, RUMBLE 0, the pad not in use, no pad), Fire plays nothing, logs nothing and
//   cancels what was running; a pattern also stops at its next step if the sink goes quiet in the middle of it.
// - A running pattern is not cut by a weaker one (priority: down 3, shockwave 2, attach / yank 2, kill 1); an equal or stronger one replaces it.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Skygrind.Core
{
    /// <summary>What a pattern plays on: Rumble in the game, a recorder in the tests and the harness.</summary>
    public interface IHapticsSink
    {
        /// <summary>VIBRATION on, RUMBLE above 0, the pad in use and present.</summary>
        bool CanPlay { get; }

        /// <summary>RUMBLE's strength 0..1 (the motors' speeds are the pattern's x this, clamped).</summary>
        double Strength { get; }

        /// <summary>One step: the motors for `ms` (the sink turns them off by itself when it is up, or at the next step).</summary>
        void Play(double strong, double weak, double ms);
    }

    /// <summary>One step that was played, as the reel's log prints it: the speeds are what the motors got (x RUMBLE).</summary>
    public struct HapticLogEntry
    {
        public string name;
        public double low, high, ms, at;
    }

    public sealed class Haptics22
    {
        /// <summary>The pattern names, in the log's and the tests' order.</summary>
        public static readonly string[] Names = { "shockwave", "attach", "yank", "kill", "down" };

        readonly Dictionary<string, double[]> patterns = new Dictionary<string, double[]>();
        readonly IHapticsSink sink;
        readonly List<HapticLogEntry> log = new List<HapticLogEntry>();
        string running;
        double[] steps;
        int stepIndex;
        double stepLeft; // s
        double clock; // s since the first Tick / Fire: the log's time stamps

        public Haptics22(GameConfig cfg, IHapticsSink sink)
        {
            this.sink = sink;
            foreach (string n in Names) patterns[n] = cfg.Nums("design.n22.haptics.patterns." + n);
        }

        /// <summary>Every step played so far (cleared by ClearLog).</summary>
        public IReadOnlyList<HapticLogEntry> Log => log;

        public void ClearLog() => log.Clear();

        /// <summary>A pattern is playing now.</summary>
        public bool Playing => running != null;

        public string RunningName => running;

        public double[] Pattern(string name) => patterns.TryGetValue(name, out double[] p) ? p : null;

        static int Priority(string name) => name == "down" ? 3 : name == "shockwave" || name == "attach" || name == "yank" ? 2 : 1;

        /// <summary>The pattern that answers a bus event (the web's names): null when the event has none. `owner`: the event's owner ("player" only).</summary>
        public static string ForEvent(string eventName, string owner)
        {
            if (owner != null && owner != "player") return null;
            switch (eventName)
            {
                case "shockwave": return "shockwave";
                case "ropeAttach": return "attach";
                case "arclinePull": return "yank";
                case "kill": return "kill";
                case "death": return "down";
                default: return null;
            }
        }

        /// <summary>Start a pattern. False (and nothing logged) when the sink cannot play, or a stronger pattern is still running.</summary>
        public bool Fire(string name)
        {
            if (!patterns.TryGetValue(name, out double[] p) || p == null || p.Length < 3) return false;
            if (!sink.CanPlay)
            {
                Cancel();
                return false;
            }
            if (running != null && Priority(running) > Priority(name)) return false;
            running = name;
            steps = p;
            stepIndex = 0;
            PlayStep();
            return true;
        }

        /// <summary>Once a rendered frame with the real frame time: the next step starts when the last one's time is up.</summary>
        public void Tick(double frameDt)
        {
            clock += frameDt;
            if (running == null) return;
            if (!sink.CanPlay)
            {
                Cancel();
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
                stepLeft += carry; // keep the overshoot, so a long frame does not stretch the pattern
            }
        }

        /// <summary>Stop the pattern (the motors are the sink's to turn off: Rumble.Stop on pause and focus loss).</summary>
        public void Cancel()
        {
            running = null;
            steps = null;
        }

        void PlayStep()
        {
            double strong = steps[stepIndex * 3], weak = steps[stepIndex * 3 + 1], ms = steps[stepIndex * 3 + 2];
            stepLeft = ms / 1000;
            sink.Play(strong, weak, ms);
            if (strong == 0 && weak == 0) return; // a pause is not logged
            log.Add(new HapticLogEntry { name = running, low = Scale(strong), high = Scale(weak), ms = ms, at = clock });
        }

        double Scale(double motor) => Math.Max(0, Math.Min(1, motor * sink.Strength));

        /// <summary>The log as the reel prints it: one line, `event low/high ms` per step, in order ("none" when nothing played).</summary>
        public string LogLine()
        {
            if (log.Count == 0) return "haptics: none";
            var sb = new StringBuilder("haptics:");
            CultureInfo ci = CultureInfo.InvariantCulture;
            foreach (HapticLogEntry e in log)
                sb.Append(' ').Append(e.name).Append(' ').Append(e.low.ToString("0.00", ci)).Append('/').Append(e.high.ToString("0.00", ci)).Append(' ').Append(e.ms.ToString("0", ci)).Append("ms;");
            return sb.ToString();
        }
    }
}
