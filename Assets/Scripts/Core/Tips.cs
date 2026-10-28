// NIGHT12 U3-25.11: tips and the first run. Tips is a port of skygrind/src/ui/tips.js (config.tips: each tip shows the
// first time its moment comes in a run, for its first `times` runs; the counts in the JSON profile). Onboarding is
// port-only (design.onboarding; Research/retention P7, Hodent): the first run only, skippable in SETTINGS; one system per
// round (R1 move and shoot, R2 dash / slide / style, R3 a wall buy and points, R5 perks, R6 the power and the PROTOCOL)
// with a guide pulse on the next thing to learn. No rule changes: only text, lights and the order of tips.
// The port:
// - tips.js's localStorage counts are profile.tips; `recording` false (automation, a LAB run) still shows the tips but
//   leaves the counts alone. moment() returns the text to show (the Game shows it as the hint toast) instead of calling
//   hint(); the {action} placeholders are filled by keyLabel as tips.js does (a null label prints "null", as JS's replace).
// - Where main.js calls moment(id) (the Game wires the same places): roundBreak while Survival is live -> 'break', and
//   'ready' from config.survival.readyFrom; powerUpSpawn in Survival -> 'powerUp'; every frame in a Survival run, the buy
//   candidate's kind through tipFor {weapon: 'wallBuy', forge: 'forge', relay: 'relay', perk: 'perk'}; newRun() in
//   startSurvival before survival.start.
// - Onboarding's lessons are data (design.onboarding.rounds). A lesson may hold back the tips it teaches (`tips`) until its
//   round on the first run (TipAllowed): the order of tips, never a rule.
using System;
using System.Collections.Generic;
using System.Text;

namespace Skygrind.Core
{
    public sealed class Tips : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: recording, and the tips shown this run (once a run each; the counts across
        // runs are the profile's).
        public void Save(StateWriter w)
        {
            w.B("recording", recording);
            w.Strings("shown", new List<string>(shown));
        }

        public void Load(StateReader r)
        {
            List<string> s = r.Strings("shown");
            recording = r.B("recording");
            shown.Clear();
            foreach (string id in s) shown.Add(id);
        }

        /// <summary>False for an automation run: tips still show, the counts don't change. The Game sets it at each run's start.</summary>
        public bool recording;

        readonly Profile profile;
        readonly Func<string, string> keyLabel;
        readonly EventBus events;
        readonly bool enabled;
        readonly double times;
        readonly Dictionary<string, string> list = new Dictionary<string, string>();
        readonly List<string> ids = new List<string>(); // config.tips.list's order
        readonly HashSet<string> shown = new HashSet<string>(); // this run

        /// <summary>`keyLabel`: an action's key label ("interact" -> "E"), for the {action} placeholders.</summary>
        public Tips(GameConfig cfg, Profile profile, Func<string, string> keyLabel) : this(cfg, profile, keyLabel, null)
        {
        }

        /// <summary>With an event bus: a tip shown emits tip {id, text} (port-only).</summary>
        public Tips(GameConfig cfg, Profile profile, Func<string, string> keyLabel, EventBus events)
        {
            this.profile = profile ?? new Profile();
            this.keyLabel = keyLabel ?? (_ => null);
            this.events = events;
            enabled = cfg.Has("tips.enabled") && cfg.Get("tips.enabled") is bool b && b;
            times = cfg.Has("tips.times") ? cfg.Num("tips.times") : 0;
            if (cfg.Has("tips.list"))
                foreach (var kv in cfg.Obj("tips.list"))
                    if (kv.Value is string s)
                    {
                        list[kv.Key] = s;
                        ids.Add(kv.Key);
                    }
        }

        /// <summary>The tip ids in config order (tests, SETTINGS).</summary>
        public IReadOnlyList<string> Ids => ids;

        /// <summary>A tip's raw text (placeholders unfilled), or null.</summary>
        public string Text(string id) => id != null && list.TryGetValue(id, out string s) ? s : null;

        /// <summary>tips.js moment: the tip's text with its keys filled in when it shows now (once a run, its first `times` runs), else null.</summary>
        public string Moment(string id)
        {
            string text = enabled ? Text(id) : null;
            if (text == null || shown.Contains(id) || profile.TipCount(id) >= times) return null;
            shown.Add(id);
            if (recording) profile.tips[id] = profile.TipCount(id) + 1;
            string filled = Fill(text, keyLabel);
            events?.Emit(MetaEvents.Tip, new TipEvent { id = id, text = filled });
            return filled;
        }

        /// <summary>tips.js newRun: every tip may show again (if it has runs left).</summary>
        public void NewRun()
        {
            shown.Clear();
        }

        /// <summary>text.replace(/\{(\w+)\}/g, (_, action) => keyLabel(action)): \w is [A-Za-z0-9_]; a null label prints "null".</summary>
        public static string Fill(string text, Func<string, string> keyLabel)
        {
            if (text == null || text.IndexOf('{') < 0) return text;
            var sb = new StringBuilder(text.Length + 8);
            int i = 0;
            while (i < text.Length)
            {
                char c = text[i];
                if (c == '{')
                {
                    int j = i + 1;
                    while (j < text.Length && Word(text[j])) j++;
                    if (j > i + 1 && j < text.Length && text[j] == '}')
                    {
                        sb.Append(keyLabel?.Invoke(text.Substring(i + 1, j - i - 1)) ?? "null");
                        i = j + 1;
                        continue;
                    }
                }
                sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        static bool Word(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_';
    }

    public sealed class Onboarding : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the first-run script running this run, and the rounds whose lesson started.
        public void Save(StateWriter w)
        {
            w.B("runActive", runActive);
            w.Ints("stepped", new List<int>(stepped));
        }

        public void Load(StateReader r)
        {
            List<int> s = r.Ints("stepped");
            runActive = r.B("runActive");
            stepped.Clear();
            foreach (int round in s) stepped.Add(round);
        }

        /// <summary>One round's lesson (design.onboarding.rounds).</summary>
        public sealed class Lesson
        {
            public int round;
            public string id, title, text; // text may hold {action} key placeholders
            public string guide; // what the guide pulse lights: "wallBuy", "perk", "power", "relay", "gate", or null
            public string guideThen; // what it lights once `guide` is done (R6: the power switch, then the next relay), or null
            public readonly List<string> tips = new List<string>(); // the tips.js ids it teaches: held back before its round
        }

        readonly Profile profile;
        readonly EventBus events;
        readonly bool enabled;
        readonly List<Lesson> lessons = new List<Lesson>();
        readonly HashSet<int> stepped = new HashSet<int>(); // rounds whose lesson started this run
        bool runActive;

        public Onboarding(GameConfig cfg, Profile profile) : this(cfg, profile, null)
        {
        }

        /// <summary>With an event bus: RoundStart emits onboardingStep {round, id} (port-only).</summary>
        public Onboarding(GameConfig cfg, Profile profile, EventBus events)
        {
            this.profile = profile ?? new Profile();
            this.events = events;
            enabled = cfg.Design("onboarding.enabled");
            if (cfg.Has("design.onboarding.rounds"))
                foreach (object o in cfg.Arr("design.onboarding.rounds"))
                {
                    Dictionary<string, object> d = PortData.Obj(o);
                    var l = new Lesson
                    {
                        round = (int)PortData.Num(d["round"]), id = (string)d["id"], title = (string)d["title"], text = (string)d["text"],
                        guide = d.TryGetValue("guide", out object g) ? g as string : null,
                        guideThen = d.TryGetValue("guideThen", out object gt) ? gt as string : null,
                    };
                    if (d.TryGetValue("tips", out object t) && t is List<object> tl)
                        foreach (object x in tl)
                            if (x is string s) l.tips.Add(s);
                    lessons.Add(l);
                }
            lessons.Sort((a, b) => a.round != b.round ? a.round.CompareTo(b.round) : string.CompareOrdinal(a.id, b.id));
        }

        /// <summary>The script runs this run (design.onboarding.enabled, the setting on, the profile not onboarded).</summary>
        public bool Active => runActive;

        /// <summary>A run starts: `settingOn` is SETTINGS' first-run guide toggle.</summary>
        public void StartRun(bool settingOn)
        {
            runActive = enabled && settingOn && !profile.onboarded;
            stepped.Clear();
        }

        /// <summary>The lesson for a round (null: none), whether or not the script is active.</summary>
        public Lesson For(int round)
        {
            foreach (Lesson l in lessons)
                if (l.round == round) return l;
            return null;
        }

        /// <summary>The whole table, in round order (tests, the SETTINGS description).</summary>
        public List<Lesson> Lessons() => new List<Lesson>(lessons);

        /// <summary>
        /// A round started: its lesson while the script is active (once a run per round; emits onboardingStep), else null.
        /// The Game shows its title and text and points the guide pulse at `guide`.
        /// </summary>
        public Lesson RoundStart(int round)
        {
            if (!runActive || stepped.Contains(round)) return null;
            Lesson l = For(round);
            if (l == null) return null;
            stepped.Add(round);
            events?.Emit(MetaEvents.OnboardingStep, new OnboardingEvent { round = round, id = l.id });
            return l;
        }

        /// <summary>
        /// The order of tips: while the script is active, a tip a lesson teaches waits for that lesson's round (the buy prompt
        /// itself still shows; nothing is refused). True outside the script.
        /// </summary>
        public bool TipAllowed(string tipId, int round)
        {
            if (!runActive) return true;
            foreach (Lesson l in lessons)
                if (l.tips.Contains(tipId)) return round >= l.round;
            return true;
        }

        /// <summary>The first run ended (or the guide was skipped): the profile is onboarded. Not recording: nothing.</summary>
        public void Finish(bool recording)
        {
            runActive = false;
            if (recording) profile.onboarded = true;
        }
    }
}
