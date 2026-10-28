// NIGHT20 E1 (builder P; Research/2026-10-06-additions-backlog.md E1, QUEUE-first-fifteen U3-28.6): the first fifteen
// minutes v2, design.adds.firstFifteen (Unity-only: the web has no first-run teaching). Hodent's onboarding (The Gamer's
// Brain): teach a verb at its moment of need, gone on use, one thing at a time. NIGHT19's first run taught moving, the
// wall buy, perks and the power by round cards; nothing taught the parry, the punch, the box, HOLD, MODS or exfil.
// The rule only (Game/FirstFifteenWatch.cs finds the moments, Game/UiOnboarding.cs draws the line):
// - Need(id): a moment of need. Its line shows unless the profile learned it; while another shows it waits (one at a time).
// - Used(id): its first use learns it (the profile's flag "learn.<id>": never again; the caller saves the profile); the
//   next waiting moment shows.
// - Gone(id): its moment passed without a use (the break ended, the draft closed): it goes unlearned, as does one shown
//   showSeconds without a use (Tick, whose clock runs only while the view can show it); the next need shows it again.
// - Reset: a new run drops what shows and what waits; what was learned stays.
// Nothing allocates after construction (the waiting moments are flags; Version counts changes for the view).
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class FirstFifteen
    {
        /// <summary>One verb: its id (the watch's trigger) and its line ({action} placeholders: the live glyph).</summary>
        public sealed class Moment
        {
            public string id, line;
        }

        /// <summary>The profile flag that marks a verb learned ("learn.parry").</summary>
        public const string FlagPrefix = "learn.";

        public readonly List<Moment> moments = new List<Moment>();

        /// <summary>design.adds.firstFifteen.showSeconds: a line shown this long without a use goes (unlearned).</summary>
        public readonly double showSeconds;

        readonly Profile profile;
        readonly bool enabled;
        readonly string[] flags; // FlagPrefix + id, built once
        readonly bool[] waiting;
        readonly bool[] urgent; // NIGHT20 E1 (builder P2): design.adds.firstFifteen.urgent
        readonly double[] held; // NIGHT20 E1: the show time a line had when it gave way (it comes back with it)
        int active = -1;
        double shownFor;

        /// <summary>Moves on every change of Active (the view rebuilds its line once).</summary>
        public int Version { get; private set; }

        /// <summary>The rule over design.adds.firstFifteen's moments; `enabled` false: nothing ever shows.</summary>
        public FirstFifteen(GameConfig cfg, Profile profile, bool enabled)
        {
            this.profile = profile;
            this.enabled = enabled;
            if (cfg.Has("design.adds.firstFifteen.moments"))
            {
                showSeconds = cfg.Num("design.adds.firstFifteen.showSeconds");
                foreach (object o in cfg.Arr("design.adds.firstFifteen.moments"))
                {
                    Dictionary<string, object> m = PortData.Obj(o);
                    moments.Add(new Moment { id = (string)m["id"], line = (string)m["line"] });
                }
            }
            flags = new string[moments.Count];
            for (int i = 0; i < moments.Count; i++) flags[i] = FlagPrefix + moments[i].id;
            waiting = new bool[moments.Count];
            urgent = new bool[moments.Count];
            held = new double[moments.Count];
            if (cfg.Has("design.adds.firstFifteen.lessonYield")) lessonYield = cfg.Num("design.adds.firstFifteen.lessonYield");
            readSeconds = cfg.Has("design.adds.firstFifteen.readSeconds") ? cfg.Num("design.adds.firstFifteen.readSeconds") : double.PositiveInfinity;
            if (cfg.Has("design.adds.firstFifteen.urgent"))
                foreach (string id in cfg.Strs("design.adds.firstFifteen.urgent"))
                {
                    int i = Index(id);
                    if (i >= 0) urgent[i] = true;
                }
        }

        int Index(string id)
        {
            for (int i = 0; i < moments.Count; i++)
                if (moments[i].id == id) return i;
            return -1;
        }

        public bool Has(string id) => Index(id) >= 0;

        /// <summary>The verb's flag is set in the profile.</summary>
        public bool Learned(string id)
        {
            int i = Index(id);
            return i >= 0 && profile != null && profile.Flag(flags[i]);
        }

        /// <summary>The line showing (null: none).</summary>
        public Moment Active => active >= 0 ? moments[active] : null;

        void Show(int i)
        {
            active = i;
            shownFor = held[i]; // NIGHT20 E1: a line given up mid-show comes back with the time it had (showSeconds in all)
            held[i] = 0;
            Version++;
        }

        /// <summary>NIGHT20 E1: the line showing gives way to `i` and waits, keeping its time.</summary>
        void GiveWay(int i)
        {
            held[active] = shownFor;
            waiting[active] = true;
            waiting[i] = false;
            Show(i);
        }

        /// <summary>NIGHT20 E1: the first urgent moment waiting, in the table's order (-1: none).</summary>
        int UrgentWaiting()
        {
            for (int k = 0; k < waiting.Length; k++)
                if (waiting[k] && urgent[k] && k != active && (profile == null || !profile.Flag(flags[k]))) return k;
            return -1;
        }

        /// <summary>The next waiting moment, in the table's order (none: nothing shows).</summary>
        void Next()
        {
            active = -1;
            Version++;
            for (int i = 0; i < waiting.Length; i++)
            {
                if (!waiting[i]) continue;
                waiting[i] = false;
                if (profile != null && profile.Flag(flags[i])) continue;
                Show(i);
                return;
            }
        }

        /// <summary>A moment of need: true when it shows now or waits (false: off, unknown, learned or already showing).</summary>
        public bool Need(string id)
        {
            if (!enabled) return false;
            int i = Index(id);
            if (i < 0 || (profile != null && profile.Flag(flags[i])) || active == i) return false;
            if (active < 0)
            {
                Show(i);
                return true;
            }
            // NIGHT20 E1: a short window takes the line from a standing moment, or from an urgent one read readSeconds already;
            // the one it took from waits (and comes back if its moment is still on)
            if (urgent[i] && (!urgent[active] || shownFor >= readSeconds))
            {
                GiveWay(i);
                return true;
            }
            waiting[i] = true;
            return true;
        }

        /// <summary>The verb was used: learned for the profile. True when it was newly learned (save the profile).</summary>
        public bool Used(string id)
        {
            int i = Index(id);
            if (i < 0 || profile == null || !enabled) return false;
            waiting[i] = false;
            held[i] = 0;
            bool fresh = profile.SetFlag(flags[i]);
            if (active == i) Next();
            return fresh;
        }

        /// <summary>Its moment passed without a use: it goes (or stops waiting), unlearned.</summary>
        public void Gone(string id)
        {
            int i = Index(id);
            if (i < 0) return;
            waiting[i] = false;
            held[i] = 0;
            if (active == i) Next();
        }

        /// <summary>`dt` s of a frame; the show clock runs only while the view can show the line (`canShow`).</summary>
        public void Tick(double dt, bool canShow)
        {
            if (active < 0 || !canShow) return;
            shownFor += dt;
            if (shownFor >= showSeconds) Next(); // unlearned: the next need shows it again
            else if (urgent[active] && shownFor >= readSeconds) // NIGHT20 E1: read; an urgent moment waiting takes the line
            {
                int w = UrgentWaiting();
                if (w >= 0) GiveWay(w);
            }
        }

        /// <summary>NIGHT20 E1 (builder P2): design.adds.firstFifteen.lessonYield: a first-run lesson card up this long (s)
        /// yields the card to a moment's line (the lesson's line comes back after it).</summary>
        public readonly double lessonYield;

        /// <summary>NIGHT20 E1: design.adds.firstFifteen.readSeconds: an urgent line shown this long (s, while it can show)
        /// gives the line to another urgent moment (no line hogs the card past a reading).</summary>
        public readonly double readSeconds;

        /// <summary>NIGHT20 E1: a moment's line may take the card: no lesson on it, or one up `lessonFor` >= lessonYield s.</summary>
        public bool MayShow(bool lessonOn, double lessonFor) => !lessonOn || lessonFor >= lessonYield;

        /// <summary>NIGHT20 E1: the moment is urgent (design.adds.firstFifteen.urgent: its window is short).</summary>
        public bool Urgent(string id)
        {
            int i = Index(id);
            return i >= 0 && urgent[i];
        }

        /// <summary>A new run: nothing shows, nothing waits (learned stays: the profile's).</summary>
        public void Reset()
        {
            for (int i = 0; i < waiting.Length; i++)
            {
                waiting[i] = false;
                held[i] = 0;
            }
            if (active >= 0)
            {
                active = -1;
                Version++;
            }
        }
    }
}
