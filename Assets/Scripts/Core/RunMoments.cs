// Port of skygrind/src/modes/runMoments.js (OVERNIGHT2 N6 item 10; NIGHT11 U3-24.8).
// Results polish (OVERNIGHT2 N6 item 10): a run's best moments (the biggest style award, the longest ARCLINE swing,
// the top speed, the biggest multi-kill, bosses down) and its grade (D, C, B, A, S from the round reached, the best
// style rank and the accuracy), plus the one-line score text the end screen copies. Pure grade + line; the tracker
// listens to the bus (fixed step for the speed).
//
// Port notes (logged differences):
// - The multi-kill window: the web reads performance.now() (a wall clock, so a kill's hitstop freeze and a pause count
//   toward results.multiWindow); Core reads no clock (PORT-RULES 5), so the tracker takes the sim's time (`simTime`,
//   SurvivalGame.SimTime): frozen time doesn't count, and a multi-kill across a hitstop is a little easier to make.
// - The rope: the web's mover emits ropeAttach / ropeRelease on the bus ({owner: 'player', ...}); the port's Mover
//   raises them on Mover.Events (MoverEventType.RopeAttach / RopeRelease), so the Game hands them to OnMover (or calls
//   RopeAttach / RopeRelease). In Survival the ARCLINE is a 5500 gadget nobody owns at the start (ropeEnabled false),
//   so the swing path never fires there until the ARCLINE is bought; the code path is kept.
// - Bosses: the web pushes e.boss on a kill of kind 'boss'; no boss is ported and the port's KillEvent has no `boss`
//   field, so `bosses` stays empty until the bosses land (the path is kept as a comment where it goes).
// - scoreLine's template literals: numbers print as JS String(number) does (JsNumber below: V8's shortest round-trip
//   digits, the ECMAScript Number::toString layout); `${x}` of a string is the string, of null "null"; `seed` and
//   `build` print only when truthy (null, "", 0 and NaN are falsy). ScoreLine(object ...) takes recorded JSON values as
//   they come (string, double, bool or null) for a vector checker.
// Step allocates nothing.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Skygrind.Core
{
    public sealed class RunMoments : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the best award, the swing and speed records, the best multikill and its kill
        // times, the swing in progress, the switch.
        public void Save(StateWriter w)
        {
            w.B("hasBestAward", hasBestAward);
            w.S("bestAwardName", bestAwardName);
            w.D("bestAwardPoints", bestAwardPoints);
            w.D("longestSwing", longestSwing);
            w.D("topSpeed", topSpeed);
            w.I("bestMulti", bestMulti);
            w.Strings("bosses", bosses);
            w.B("swingFrom", swingFrom);
            w.D("swingDist", swingDist);
            w.D("lastX", lastX);
            w.D("lastZ", lastZ);
            w.B("live", live);
            w.Doubles("killTimes", killTimes);
        }

        public void Load(StateReader r)
        {
            List<string> b = r.Strings("bosses");
            List<double> k = r.Doubles("killTimes");
            hasBestAward = r.B("hasBestAward");
            bestAwardName = r.S("bestAwardName");
            bestAwardPoints = r.D("bestAwardPoints");
            longestSwing = r.D("longestSwing");
            topSpeed = r.D("topSpeed");
            bestMulti = r.Int("bestMulti");
            bosses.Clear();
            bosses.AddRange(b);
            swingFrom = r.B("swingFrom");
            swingDist = r.D("swingDist");
            lastX = r.D("lastX");
            lastZ = r.D("lastZ");
            live = r.B("live");
            killTimes.Clear();
            killTimes.AddRange(k);
        }

        /// <summary>config.results.grade, read once: the blend's weights, the round that counts in full, the bands in order.</summary>
        public sealed class Grade
        {
            public double round, rank, accuracy, roundFull;
            public readonly List<string> letters = new List<string>();
            public readonly List<double> at = new List<double>();

            public static Grade From(GameConfig cfg)
            {
                var g = new Grade
                {
                    round = cfg.Num("results.grade.round"), rank = cfg.Num("results.grade.rank"), accuracy = cfg.Num("results.grade.accuracy"),
                    roundFull = cfg.Num("results.grade.roundFull"),
                };
                foreach (object band in cfg.Arr("results.grade.bands"))
                {
                    List<object> pair = PortData.Arr(band);
                    g.letters.Add((string)pair[0]);
                    g.at.Add(PortData.Num(pair[1]));
                }
                return g;
            }
        }

        /// <summary>
        /// Pure: the grade of a run: round (x roundWeight, capped at roundFull), best rank (0..5) and accuracy (0..1) blended,
        /// then the first band it reaches.
        /// </summary>
        /// <returns>"S" | "A" | "B" | "C" | "D"</returns>
        public static string GradeOf(double round, double bestRank, double accuracy, Grade g)
        {
            double score = g.round * JsMath.Min(1, round / g.roundFull) + g.rank * JsMath.Min(1, bestRank / 5) + g.accuracy * JsMath.Max(0, JsMath.Min(1, accuracy));
            for (int i = 0; i < g.letters.Count; i++) if (score >= g.at[i]) return g.letters[i];
            return "D";
        }

        /// <summary>
        /// Pure: the text line the end screen copies. `mutators` null is the web's default []; `seed` null its default
        /// null; `build` "" its default ''.
        /// </summary>
        public static string ScoreLine(double round, double points, string grade, string bestRankName, double time, IList<string> mutators = null, string seed = null, string build = "")
        {
            List<object> muts = null;
            if (mutators != null)
            {
                muts = new List<object>(mutators.Count);
                foreach (string s in mutators) muts.Add(s);
            }
            return ScoreLine((object)round, points, grade, bestRankName, time, muts, seed, build);
        }

        /// <summary>
        /// scoreLine with JSON-shaped values (string, double, bool or null), as a recorded call holds them: each `${x}` is JS
        /// String(x), `seed` / `build` print when truthy, a null `mutators` is the default [] (an absent key: pass null for
        /// seed and build too, the defaults null and '' print the same).
        /// </summary>
        public static string ScoreLine(object round, object points, object grade, object bestRankName, double time, IList<object> mutators, object seed, object build)
        {
            double m = Math.Floor(time / 60);
            string s = JsNumber.ToString(Math.Floor(time % 60)).PadLeft(2, '0'); // String(...).padStart(2, '0')
            string muts = "";
            if (mutators != null && mutators.Count > 0)
            {
                var join = new StringBuilder(" [");
                for (int i = 0; i < mutators.Count; i++)
                {
                    if (i > 0) join.Append(" + ");
                    if (mutators[i] != null) join.Append(JsNumber.Str(mutators[i])); // Array.join: null / undefined join as ''
                }
                muts = join.Append(']').ToString();
            }
            // OVERNIGHT3 P7: the seed; NIGHT6 B5: the build
            return "SKYGRIND survival: round " + JsNumber.Str(round) + ", " + JsNumber.Str(points) + " pts, grade " + JsNumber.Str(grade) + ", best " +
                   JsNumber.Str(bestRankName) + ", " + JsNumber.ToString(m) + ":" + s + muts + (JsNumber.Truthy(seed) ? ", seed " + JsNumber.Str(seed) : "") +
                   (JsNumber.Truthy(build) ? ", " + JsNumber.Str(build) : "");
        }

        /// <summary>The end screen's moments (summary()): rounded as the web rounds them.</summary>
        public sealed class Summary
        {
            public string bestAwardName; // null: no award this run (m.bestAward null)
            public double bestAwardPoints; // Math.round(e.points) at the award
            public double longestSwing; // m, Math.round(x * 10) / 10
            public double topSpeed; // m/s, Math.round(x * 10) / 10
            public int bestMulti;
            public readonly List<string> bosses = new List<string>();
        }

        readonly double multiWindow;
        readonly Func<double> speed;
        readonly Func<Vec3> position;
        readonly Func<bool> ropeAttached;
        readonly Func<double> simTime;
        // m: { bestAward: null, longestSwing: 0, topSpeed: 0, bestMulti: 0, bosses: [] }
        string bestAwardName;
        double bestAwardPoints;
        bool hasBestAward;
        double longestSwing, topSpeed;
        int bestMulti;
        readonly List<string> bosses = new List<string>();
        bool swingFrom; // where the current swing hooked on (the distance travelled while attached)
        double swingDist;
        double lastX, lastZ;
        bool live;
        readonly List<double> killTimes = new List<double>();

        /// <param name="speed">mover.speed (the player's horizontal speed, m/s)</param>
        /// <param name="position">mover.position (feet)</param>
        /// <param name="ropeAttached">mover.rope?.phase === 'attached'</param>
        /// <param name="simTime">the sim's time in s (the multi-kill window; the web's performance.now() / 1000)</param>
        public RunMoments(GameConfig cfg, EventBus events, Func<double> speed, Func<Vec3> position, Func<bool> ropeAttached, Func<double> simTime)
        {
            multiWindow = cfg.Num("results.multiWindow");
            this.speed = speed;
            this.position = position;
            this.ropeAttached = ropeAttached;
            this.simTime = simTime;
            events.On<StyleAwardEvent>("styleAward", e =>
            {
                if (!live || e.quiet) return;
                if (!hasBestAward || e.points > bestAwardPoints)
                {
                    hasBestAward = true;
                    bestAwardName = e.name;
                    bestAwardPoints = JsMath.Round(e.points);
                }
            });
            events.On<KillEvent>("kill", e =>
            {
                if (!live || e.owner != "player") return;
                // if (e.kind === 'boss' && e.boss) m.bosses.push(e.boss): no boss is ported (KillEvent has no `boss`).
                double now = simTime();
                killTimes.Add(now);
                while (killTimes.Count > 0 && now - killTimes[0] > multiWindow) killTimes.RemoveAt(0);
                bestMulti = Math.Max(bestMulti, killTimes.Count);
            });
        }

        /// <summary>ropeAttach (the web's bus event; owner null is the web's undefined, which counts as the player).</summary>
        public void RopeAttach(string owner = null)
        {
            if (owner != null && owner != "player") return;
            swingFrom = true;
            swingDist = 0;
            Vec3 p = position();
            lastX = p.x;
            lastZ = p.z;
        }

        /// <summary>ropeRelease (the web's bus event).</summary>
        public void RopeRelease(string owner = null)
        {
            if (owner != null && owner != "player") return;
            if (swingFrom && live) longestSwing = JsMath.Max(longestSwing, swingDist);
            swingFrom = false;
        }

        /// <summary>The player's Mover.Events (the port's ropeAttach / ropeRelease; the player's mover: owner 'player').</summary>
        public void OnMover(MoverEvent e)
        {
            if (e.type == MoverEventType.RopeAttach) RopeAttach("player");
            else if (e.type == MoverEventType.RopeRelease) RopeRelease("player");
        }

        /// <summary>A new run.</summary>
        public void Start()
        {
            hasBestAward = false;
            bestAwardName = null;
            bestAwardPoints = 0;
            longestSwing = 0;
            topSpeed = 0;
            bestMulti = 0;
            bosses.Clear();
            swingFrom = false;
            killTimes.Clear();
            live = true;
        }

        public void Stop() => live = false;

        public bool Live => live;

        /// <summary>Fixed step: the top speed, the distance of the swing in progress. Allocates nothing.</summary>
        public void Step()
        {
            if (!live) return;
            topSpeed = JsMath.Max(topSpeed, speed());
            if (swingFrom && ropeAttached())
            {
                Vec3 p = position();
                swingDist += JsMath.Hypot(p.x - lastX, p.z - lastZ);
                lastX = p.x;
                lastZ = p.z;
                longestSwing = JsMath.Max(longestSwing, swingDist);
            }
        }

        /// <summary>The end screen's moments.</summary>
        public Summary Get()
        {
            var s = new Summary
            {
                bestAwardName = hasBestAward ? bestAwardName : null,
                bestAwardPoints = hasBestAward ? bestAwardPoints : 0,
                bestMulti = bestMulti,
                longestSwing = JsMath.Round(longestSwing * 10) / 10,
                topSpeed = JsMath.Round(topSpeed * 10) / 10,
            };
            s.bosses.AddRange(bosses);
            return s;
        }

        /// <summary>main.js endRun: stats.moments = moments.summary() (the RunStats fields the results read).</summary>
        public void Fill(RunStats stats)
        {
            Summary s = Get();
            stats.bestAwardName = s.bestAwardName;
            stats.bestAwardPoints = s.bestAwardPoints;
            stats.longestSwing = s.longestSwing;
            stats.topSpeed = s.topSpeed;
            stats.bestMulti = s.bestMulti;
        }
    }

    /// <summary>
    /// JS String(x) for a number (ECMAScript Number::toString(10), as V8 prints it): the shortest digits that read back to
    /// the same double (the closest of them; a tie to the even one), laid out as plain digits for 1e-7 &lt; |x| &lt; 1e21 and
    /// as d.ddde±n outside. Exact (BigInteger; JsJson.ParseNumber checks the read-back), no System.Math transcendental.
    /// Off the step path: it allocates.
    /// </summary>
    public static class JsNumber
    {
        public static string ToString(double x)
        {
            if (double.IsNaN(x)) return "NaN";
            if (x == 0) return "0"; // +0 and -0
            if (double.IsInfinity(x)) return x > 0 ? "Infinity" : "-Infinity";
            if (x < 0) return "-" + ToString(-x);
            if (x < 9007199254740992.0 && x == Math.Floor(x)) return ((long)x).ToString(CultureInfo.InvariantCulture); // an integer below 2^53 is its own shortest form
            Shortest(x, out string digits, out int n);
            int k = digits.Length;
            if (k <= n && n <= 21) return digits + new string('0', n - k);
            if (0 < n && n <= 21) return digits.Substring(0, n) + "." + digits.Substring(n);
            if (-6 < n && n <= 0) return "0." + new string('0', -n) + digits;
            int e = n - 1;
            string exp = (e < 0 ? "-" : "+") + Math.Abs(e).ToString(CultureInfo.InvariantCulture);
            return k == 1 ? digits + "e" + exp : digits.Substring(0, 1) + "." + digits.Substring(1) + "e" + exp;
        }

        /// <summary>JS String(v) of a JSON-shaped value: null "null", a string itself, a number String(n), a bool "true" / "false".</summary>
        public static string Str(object v)
        {
            switch (v)
            {
                case null: return "null";
                case string s: return s;
                case double d: return ToString(d);
                case float f: return ToString(f);
                case int i: return ToString(i);
                case long l: return ToString(l);
                case bool b: return b ? "true" : "false";
                default: return v.ToString();
            }
        }

        /// <summary>JS truthiness of a JSON-shaped value: null, false, 0, NaN and "" are falsy.</summary>
        public static bool Truthy(object v)
        {
            switch (v)
            {
                case null: return false;
                case string s: return s.Length > 0;
                case double d: return d != 0 && !double.IsNaN(d);
                case float f: return f != 0 && !float.IsNaN(f);
                case int i: return i != 0;
                case long l: return l != 0;
                case bool b: return b;
                default: return true;
            }
        }

        /// <summary>
        /// The shortest decimal digits s (no trailing zeros) and the point position n with x = 0.s x 10^n: for p = 1, 2, ...
        /// digits, the p-digit decimals just below and above x; the first p where one reads back to x wins (both: the closer,
        /// a tie to the even one). Any p-digit decimal that reads back lies between those two and x, so none is missed.
        /// </summary>
        static void Shortest(double x, out string digits, out int point)
        {
            long bits = BitConverter.DoubleToInt64Bits(x);
            int be = (int)((bits >> 52) & 0x7FF);
            long frac = bits & 0xFFFFFFFFFFFFFL;
            BigInteger f = be == 0 ? new BigInteger(frac) : new BigInteger(frac | (1L << 52));
            int e2 = be == 0 ? -1074 : be - 1075; // x = f * 2^e2
            BigInteger num = e2 >= 0 ? f << e2 : f, den = e2 >= 0 ? BigInteger.One : BigInteger.One << -e2;
            // n with 10^(n-1) <= x < 10^n: an estimate from the binary exponent (log10(2) = 0.30103...), then exact.
            int n = (int)Math.Floor((e2 + 52) * 0.30102999566398120) + 1;
            while (CompareToPow10(num, den, n - 1) < 0) n--;
            while (CompareToPow10(num, den, n) >= 0) n++;
            for (int p = 1; p <= 17; p++)
            {
                BigInteger a = num, b = den;
                int sh = p - n;
                if (sh >= 0) a *= BigInteger.Pow(10, sh);
                else b *= BigInteger.Pow(10, -sh);
                BigInteger lo = BigInteger.DivRem(a, b, out BigInteger rem);
                if (rem.IsZero)
                {
                    Emit(lo, n - p, out digits, out point);
                    return;
                }
                BigInteger hi = lo + 1;
                bool loOk = ReadsBack(lo, n - p, x), hiOk = ReadsBack(hi, n - p, x);
                if (!loOk && !hiOk) continue;
                BigInteger pick;
                if (loOk && hiOk)
                {
                    int c = (rem * 2).CompareTo(b); // x - lo vs hi - x, in units of 10^(n-p)
                    pick = c < 0 ? lo : c > 0 ? hi : lo.IsEven ? lo : hi;
                }
                else pick = loOk ? lo : hi;
                Emit(pick, n - p, out digits, out point);
                return;
            }
            throw new InvalidOperationException("JsNumber: no 17-digit form reads back (impossible for a finite double)");
        }

        /// <summary>The sign of num / den - 10^k.</summary>
        static int CompareToPow10(BigInteger num, BigInteger den, int k) =>
            k >= 0 ? num.CompareTo(den * BigInteger.Pow(10, k)) : (num * BigInteger.Pow(10, -k)).CompareTo(den);

        static bool ReadsBack(BigInteger s, int exp10, double x)
        {
            if (s.IsZero) return false;
            try
            {
                return JsJson.ParseNumber(s.ToString(CultureInfo.InvariantCulture) + "e" + exp10.ToString(CultureInfo.InvariantCulture)) == x;
            }
            catch (OverflowException)
            {
                return false; // past the largest double: reads back as Infinity
            }
        }

        static void Emit(BigInteger s, int exp10, out string digits, out int point)
        {
            string d = s.ToString(CultureInfo.InvariantCulture);
            point = d.Length + exp10;
            digits = d.TrimEnd('0');
        }
    }
}
