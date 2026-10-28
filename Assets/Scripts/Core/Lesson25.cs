// NIGHT25 (builder L): the TEACHING CAMPAIGN's lesson rule, pure (no engine). A tutorial stage (a room or link of a level plan) holds an ordered list
// of lessons ("lessons": [{kind, n, rect, via, minY, prompt, promptPad}], Design/levels/LEVEL-FORMAT.md); the stage's door opens when every
// lesson is cleared (and, in a room with waves, when the waves are dead). A lesson is a small condition on what the player DID since it began:
//   jumpedPit / dashedGap / padHop / flickCrossed  the verb happened (a jump pad, a dash, a fist flick) AND the player stands on the far
//                                                  ground in `rect` (so the lesson is the crossing, not the key press)
//   reached          grounded in `rect` at or above `minY` (the wall-jump flue's deck, the pit's far side)
//   slidTunnel       a slide was seen inside `via` (the tunnel), then the player stands grounded in `rect` (the tunnel's far side)
//   wallJumped       `n` wall jumps
//   wallRiddenAndFired  `n` shots fired while wall riding (NIGHT29: the tutorial's wall-ride room: the slow-mo aim; counts from the lesson's start, needs no enemy)
//   shots / targetsHit / husksDead / punchWhileFiring   `n` shots fired, shots that hit, kills, punches landed inside `punchFireSteps` fixed steps of a shot
//                                                  (count lessons count from the stage's start: no soft-lock when the enemies die early)
// The world feeds a LessonSample each fixed step (counters only ever grow; they are the sim's, the rule takes differences). The thresholds
// (punchFireSteps) come from design.n25.lesson. Ported from nothing: the web has no tutorial (Unity-only).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum LessonKind { None, Reached, JumpedPit, SlidTunnel, WallJumped, DashedGap, PadHop, Shots, TargetsHit, PunchWhileFiring, FlickCrossed, HusksDead, WallRiddenAndFired }

    public sealed class LessonDef
    {
        public LessonKind kind;
        public int n = 1;
        /// <summary>Region [x0, z0, x1, z1] (the far side of a gap, a tunnel's exit, a deck); empty (all zero) when the kind has none.</summary>
        public double rx0, rz0, rx1, rz1;
        public bool hasRect;
        /// <summary>slidTunnel: the tunnel's own region, where the slide must be seen.</summary>
        public double vx0, vz0, vx1, vz1;
        public bool hasVia;
        public double minY = -1e9;
        /// <summary>The prompt text (keyboard / pad variant), at most six words (CURRICULUM.md rule 5).</summary>
        public string prompt = "", promptPad = "";

        public static LessonKind ParseKind(string s)
        {
            switch (s)
            {
                case "reached": return LessonKind.Reached;
                case "jumpedPit": return LessonKind.JumpedPit;
                case "slidTunnel": return LessonKind.SlidTunnel;
                case "wallJumped": return LessonKind.WallJumped;
                case "dashedGap": return LessonKind.DashedGap;
                case "padHop": return LessonKind.PadHop;
                case "shots": return LessonKind.Shots;
                case "targetsHit": return LessonKind.TargetsHit;
                case "punchWhileFiring": return LessonKind.PunchWhileFiring;
                case "flickCrossed": return LessonKind.FlickCrossed;
                case "husksDead": return LessonKind.HusksDead;
                case "wallRiddenAndFired": return LessonKind.WallRiddenAndFired;
                default: return LessonKind.None;
            }
        }

        public static string KindName(LessonKind k)
        {
            switch (k)
            {
                case LessonKind.Reached: return "reached";
                case LessonKind.JumpedPit: return "jumpedPit";
                case LessonKind.SlidTunnel: return "slidTunnel";
                case LessonKind.WallJumped: return "wallJumped";
                case LessonKind.DashedGap: return "dashedGap";
                case LessonKind.PadHop: return "padHop";
                case LessonKind.Shots: return "shots";
                case LessonKind.TargetsHit: return "targetsHit";
                case LessonKind.PunchWhileFiring: return "punchWhileFiring";
                case LessonKind.FlickCrossed: return "flickCrossed";
                case LessonKind.HusksDead: return "husksDead";
                case LessonKind.WallRiddenAndFired: return "wallRiddenAndFired";
                default: return "none";
            }
        }

        /// <summary>A lesson that counts from the stage's start (not from the previous lesson's clear).</summary>
        public bool IsCount => kind == LessonKind.Shots || kind == LessonKind.TargetsHit || kind == LessonKind.PunchWhileFiring || kind == LessonKind.HusksDead;

        /// <summary>A lesson only an enemy can finish (a room with this lesson tops its last wave up when the enemies are dead and the lesson is not).</summary>
        public bool NeedsEnemy => kind == LessonKind.TargetsHit || kind == LessonKind.PunchWhileFiring || kind == LessonKind.HusksDead || kind == LessonKind.FlickCrossed;

        public static LessonDef From(Dictionary<string, object> d)
        {
            var l = new LessonDef { kind = ParseKind(d.TryGetValue("kind", out object k) ? k as string : null) };
            if (d.TryGetValue("n", out object n)) l.n = (int)PortData.Num(n);
            if (d.TryGetValue("rect", out object rc))
            {
                List<object> a = PortData.Arr(rc);
                l.rx0 = PortData.Num(a[0]); l.rz0 = PortData.Num(a[1]); l.rx1 = PortData.Num(a[2]); l.rz1 = PortData.Num(a[3]);
                l.hasRect = true;
            }
            if (d.TryGetValue("via", out object vi))
            {
                List<object> a = PortData.Arr(vi);
                l.vx0 = PortData.Num(a[0]); l.vz0 = PortData.Num(a[1]); l.vx1 = PortData.Num(a[2]); l.vz1 = PortData.Num(a[3]);
                l.hasVia = true;
            }
            if (d.TryGetValue("minY", out object my)) l.minY = PortData.Num(my);
            if (d.TryGetValue("prompt", out object p)) l.prompt = p as string ?? "";
            if (d.TryGetValue("promptPad", out object pp)) l.promptPad = pp as string ?? "";
            return l;
        }
    }

    /// <summary>What the rule reads of the world each fixed step: where the player is and the sim's running counters.</summary>
    public struct LessonSample
    {
        public int step;
        public Vec3 feet;
        public bool grounded, sliding;
        public int wallJumps, dashes, pads, fires, hits, punches, flicks, kills;
        /// <summary>NIGHT29 lockout: 0 with the fire / melee lockout off; else the longest attack cycle in steps (a punch and a shot cannot overlap, so the pair is a shot and a punch taking turns: the window grows by it).</summary>
        public int lockoutGap;
        /// <summary>NIGHT29: the player is wall riding this step (the wallRiddenAndFired lesson).</summary>
        public bool wallRiding;
    }

    public sealed class LessonEvent
    {
        public int room, index, count;
        public string id, kind, prompt, promptPad;
        public double time;
    }

    public sealed class Lesson25
    {
        public readonly List<LessonDef> defs;
        readonly int punchFireSteps;
        /// <summary>NIGHT29: the lockout is on (the punchWhileFiring lesson teaches the new order: one, wait, the other).</summary>
        public bool lockoutOrder;
        public int index; // the lesson in progress (defs.Count: all cleared)
        LessonSample stageBase, lessonBase;
        bool viaSeen;
        int lastFire = -1000000, lastPunch = -1000000, punchPairs;

        public Lesson25(List<LessonDef> defs, int punchFireSteps)
        {
            this.defs = defs;
            this.punchFireSteps = Math.Max(1, punchFireSteps);
        }

        public bool Done => index >= defs.Count;
        public LessonDef Current => Done ? null : defs[index];

        /// <summary>The stage begins (or restarts): the counters' baselines are now. `skip` lessons whose verb is not available in this run
        /// (a block off) are passed over (never a soft-lock).</summary>
        public void Begin(LessonSample s, Func<LessonKind, bool> available = null)
        {
            index = 0;
            lockoutOrder = s.lockoutGap > 0;
            stageBase = lessonBase = s;
            viaSeen = false;
            lastFire = lastPunch = -1000000;
            punchPairs = 0;
            rideShots = 0;
            lastFiresSeen = s.fires;
            lastPunchesSeen = s.punches;
            pairCounted = -1;
            Skip(s, available);
        }

        void Skip(LessonSample s, Func<LessonKind, bool> available)
        {
            while (!Done && available != null && !available(defs[index].kind)) Advance(s);
        }

        void Advance(LessonSample s)
        {
            index++;
            lessonBase = s;
            viaSeen = false;
            rideShots = 0;
        }

        static bool In(LessonDef d, Vec3 f) => d.hasRect && f.x >= d.rx0 && f.x <= d.rx1 && f.z >= d.rz0 && f.z <= d.rz1 && f.y >= d.minY;
        static bool InVia(LessonDef d, Vec3 f) => d.hasVia && f.x >= d.vx0 && f.x <= d.vx1 && f.z >= d.vz0 && f.z <= d.vz1;

        /// <summary>One fixed step. Returns true when a lesson was cleared on this step (the cleared lesson is `Cleared`).</summary>
        public bool Step(LessonSample s, Func<LessonKind, bool> available = null)
        {
            Cleared = null;
            if (Done) return false;
            LessonDef d = defs[index];
            if (s.fires != lastFiresSeen)
            {
                lastFire = s.step;
                if (s.wallRiding) rideShots += s.fires - lastFiresSeen; // NIGHT29: shots fired from the wall (counts from the lesson's start)
            }
            lastFiresSeen = s.fires;
            if (s.punches != lastPunchesSeen) lastPunch = s.step;
            lastPunchesSeen = s.punches;
            lockoutOrder = s.lockoutGap > 0;
            bool pair = lastFire > -1000000 && lastPunch > -1000000 && Math.Abs(lastFire - lastPunch) <= punchFireSteps + s.lockoutGap;
            if (pair && lastPunch != pairCounted) { punchPairs++; pairCounted = lastPunch; }
            bool ok;
            switch (d.kind)
            {
                case LessonKind.Reached: ok = s.grounded && In(d, s.feet); break;
                case LessonKind.JumpedPit: ok = s.grounded && In(d, s.feet); break;
                case LessonKind.SlidTunnel:
                    if (s.sliding && InVia(d, s.feet)) viaSeen = true;
                    ok = viaSeen && s.grounded && In(d, s.feet);
                    break;
                case LessonKind.WallJumped: ok = s.wallJumps - lessonBase.wallJumps >= d.n; break;
                case LessonKind.DashedGap: ok = s.dashes > lessonBase.dashes && s.grounded && In(d, s.feet); break;
                case LessonKind.PadHop: ok = s.pads > lessonBase.pads && s.grounded && In(d, s.feet); break;
                case LessonKind.FlickCrossed: ok = s.flicks > lessonBase.flicks && s.grounded && In(d, s.feet); break;
                case LessonKind.Shots: ok = s.fires - stageBase.fires >= d.n; break;
                case LessonKind.TargetsHit: ok = s.hits - stageBase.hits >= d.n; break;
                case LessonKind.HusksDead: ok = s.kills - stageBase.kills >= d.n; break;
                case LessonKind.PunchWhileFiring: ok = punchPairs >= d.n; break;
                case LessonKind.WallRiddenAndFired: ok = rideShots >= d.n; break;
                default: ok = true; break;
            }
            if (!ok) return false;
            Cleared = d;
            Advance(s);
            Skip(s, available);
            return true;
        }

        int lastFiresSeen, lastPunchesSeen, pairCounted = -1, rideShots;

        /// <summary>The lesson cleared by the last Step that returned true.</summary>
        public LessonDef Cleared { get; private set; }

        /// <summary>Pass over every lesson still to clear that needs an enemy (the top-up cap's way out of a room that cannot finish it).</summary>
        public void SkipPendingEnemyLessons()
        {
            while (!Done && defs[index].NeedsEnemy) { index++; viaSeen = false; }
        }

        /// <summary>Does any lesson still to clear need an enemy to finish?</summary>
        public bool PendingNeedsEnemy
        {
            get
            {
                for (int i = index; i < defs.Count; i++) if (defs[i].NeedsEnemy) return true;
                return false;
            }
        }
    }
}

// NIGHT35 builder T (design.n35.teach35, NIGHT35.md T1, D-7, PRINCIPLES 2): THE TEACHING TEXT, a text kind beside the lessons (it never gates a door: a
// lesson is a condition, a teach35 line is a message). ULTRAKILL's HudMessage (Research/2026-10-11-wave35/RESEARCH.md: a trigger volume shows `message` +
// the action's live binding + `message2`, timerTime 5 s, once unless notOneTime, an optional playerPref remembers it): a trigger, the verb in capitals, the
// bound button, at most one rule; shown `showS` (5 s), once per PROFILE (Profile.teach35; a fresh profile sees every line: D-7).
// The words are design.n35.teach35.lines (Design/unity-config.json): {id, level, room | weapon, needs?, text, textPad?, textOff?}:
//   level   the campaign level's base name ("foundry": the id after its "nNN-" prefix, so an n35 copy of a level keeps its lines)
//   room    the fallback trigger: the first entry into that room (roomEnter), used while the level file has no rect for the id
//   weapon  a first-pickup line: the first weaponEarned of that weapon (any level)
//   needs   a NIGHT35 block the words describe (charge35, wallride35, grapple35): with it off, `textOff` (absent: the line is not shown)
// The rects are the LEVEL FILE's (builder A): "campaign": {"teach35": [{"id": "l1.fire", "rect": [x0, z0, x1, z1], "minY": y?}]}: the feet inside the
// rect (and at or above minY) show the line. {action} tokens are filled LIVE by the view (Ui: the pad's button or the bound key, PromptText / Glyphs);
// Teach35Text.Resolve is the pure part. Unity-only (the web has no tutorial).
namespace Skygrind.Core
{
    public sealed class Teach35Line
    {
        public string id = "", level = "", room = "", weapon = "", needs = "", text = "", textPad = "", textOff = "";

        public static Teach35Line From(Dictionary<string, object> d)
        {
            string S(string k) => d.TryGetValue(k, out object v) && v is string s ? s : "";
            return new Teach35Line
            {
                id = S("id"), level = S("level"), room = S("room"), weapon = S("weapon"), needs = S("needs"),
                text = S("text"), textPad = S("textPad"), textOff = S("textOff"),
            };
        }
    }

    /// <summary>A level file's trigger rect for one line (campaign.teach35[]).</summary>
    public struct Teach35Rect
    {
        public double x0, z0, x1, z1, minY;

        public bool Holds(Vec3 f) => f.x >= x0 && f.x <= x1 && f.z >= z0 && f.z <= z1 && f.y >= minY;
    }

    /// <summary>What the sim emits ("teach35") when a line's trigger fires: the view fills its {action} tokens and shows it `seconds`.</summary>
    public sealed class Teach35Event
    {
        public string id, text, textPad, trigger;
        public double seconds;
    }

    /// <summary>design.n35.teach35: the display time, the slot's character budget and the lines.</summary>
    public sealed class Teach35Table
    {
        public double showS = 5;
        public int maxChars = 52;
        public readonly List<Teach35Line> lines = new List<Teach35Line>();

        public static Teach35Table From(GameConfig cfg)
        {
            var t = new Teach35Table();
            if (cfg == null) return t;
            if (cfg.Has("design.n35.teach35.showS")) t.showS = cfg.Num("design.n35.teach35.showS");
            if (cfg.Has("design.n35.teach35.maxChars")) t.maxChars = (int)cfg.Num("design.n35.teach35.maxChars");
            if (cfg.Has("design.n35.teach35.lines") && cfg.Get("design.n35.teach35.lines") is List<object> ls)
                foreach (object o in ls)
                    if (o is Dictionary<string, object> d) t.lines.Add(Teach35Line.From(d));
            return t;
        }

        /// <summary>A level id's base name: "n34-foundry" -> "foundry", "n35-finale" -> "finale", "tutorial" -> "tutorial".</summary>
        public static string LevelKey(string levelId)
        {
            if (string.IsNullOrEmpty(levelId)) return "";
            int dash = levelId.IndexOf('-');
            if (dash > 1 && levelId[0] == 'n' && char.IsDigit(levelId[1])) return levelId.Substring(dash + 1);
            return levelId;
        }

        /// <summary>A level file's rects (its "campaign" block's "teach35" rows), by line id.</summary>
        public static Dictionary<string, Teach35Rect> Rects(Dictionary<string, object> campaign)
        {
            var r = new Dictionary<string, Teach35Rect>();
            if (campaign == null || !campaign.TryGetValue("teach35", out object to) || !(to is List<object> rows)) return r;
            foreach (object ro in rows)
            {
                if (!(ro is Dictionary<string, object> d) || !(d.TryGetValue("id", out object io) && io is string id)) continue;
                if (!(d.TryGetValue("rect", out object rc) && rc is List<object> a && a.Count >= 4)) continue;
                r[id] = new Teach35Rect
                {
                    x0 = Math.Min(PortData.Num(a[0]), PortData.Num(a[2])), z0 = Math.Min(PortData.Num(a[1]), PortData.Num(a[3])),
                    x1 = Math.Max(PortData.Num(a[0]), PortData.Num(a[2])), z1 = Math.Max(PortData.Num(a[1]), PortData.Num(a[3])),
                    minY = d.TryGetValue("minY", out object my) ? PortData.Num(my) : -1e9,
                };
            }
            return r;
        }
    }

    /// <summary>One run's teaching triggers on one level. The caller marks the profile; `seen` answers "shown on this profile already".</summary>
    public sealed class Teach35
    {
        readonly List<Teach35Line> level = new List<Teach35Line>(), weapons = new List<Teach35Line>();
        readonly Dictionary<string, Teach35Rect> rects;
        readonly HashSet<string> done = new HashSet<string>();
        readonly Func<string, bool> seen, blockOn;

        /// <summary>Lines shown in this run, in order (the harness / reel read it).</summary>
        public readonly List<string> shown = new List<string>();

        /// <param name="blockOn">Is the named NIGHT35 block on in this run (a line's `needs`)?</param>
        public Teach35(Teach35Table table, string levelId, Dictionary<string, Teach35Rect> rects, Func<string, bool> seen, Func<string, bool> blockOn)
        {
            this.rects = rects ?? new Dictionary<string, Teach35Rect>();
            this.seen = seen;
            this.blockOn = blockOn;
            string key = Teach35Table.LevelKey(levelId);
            foreach (Teach35Line l in table.lines)
            {
                if (string.IsNullOrEmpty(l.id)) continue;
                if (!string.IsNullOrEmpty(l.weapon)) weapons.Add(l);
                else if (l.level == key) level.Add(l);
            }
        }

        /// <summary>The words this run shows for a line (null: not shown: its block is off and it has no textOff).</summary>
        public string Words(Teach35Line l, out string pad)
        {
            bool on = string.IsNullOrEmpty(l.needs) || (blockOn != null && blockOn(l.needs));
            pad = on ? l.textPad : "";
            string t = on ? l.text : l.textOff;
            return string.IsNullOrEmpty(t) ? null : t;
        }

        bool Ready(Teach35Line l) => !done.Contains(l.id) && (seen == null || !seen(l.id)) && Words(l, out _) != null;

        void Fire(Teach35Line l, List<Teach35Line> into)
        {
            done.Add(l.id);
            shown.Add(l.id);
            into.Add(l);
        }

        /// <summary>A room entered: its fallback lines (a line with a rect in the level file waits for its rect).</summary>
        public void RoomEntered(string roomId, List<Teach35Line> into)
        {
            foreach (Teach35Line l in level)
                if (l.room == roomId && !rects.ContainsKey(l.id) && Ready(l)) Fire(l, into);
        }

        /// <summary>One step: the lines whose rect holds the feet.</summary>
        public void Step(Vec3 feet, List<Teach35Line> into)
        {
            if (rects.Count == 0) return;
            foreach (Teach35Line l in level)
                if (rects.TryGetValue(l.id, out Teach35Rect r) && r.Holds(feet) && Ready(l)) Fire(l, into);
        }

        /// <summary>A weapon earned: its first-pickup line.</summary>
        public void WeaponEarned(string weapon, List<Teach35Line> into)
        {
            foreach (Teach35Line l in weapons)
                if (l.weapon == weapon && Ready(l)) Fire(l, into);
        }

        /// <summary>This level's lines (the harness's table).</summary>
        public IEnumerable<Teach35Line> LevelLines => level;

        /// <summary>Does the level file give this line a rect?</summary>
        public bool HasRect(string id) => rects.ContainsKey(id);
    }

    /// <summary>The line's format: the {action} tokens filled, and the ULTRAKILL rule checked. Pure.</summary>
    public static class Teach35Text
    {
        /// <summary>Fill each {name} with `glyph(name)` (null / empty: the name in capitals). Text outside braces is kept; an unclosed brace is kept.</summary>
        public static string Resolve(string format, Func<string, string> glyph)
        {
            if (string.IsNullOrEmpty(format) || format.IndexOf('{') < 0) return format;
            var b = new System.Text.StringBuilder(format.Length + 16);
            int i = 0;
            while (i < format.Length)
            {
                int open = format.IndexOf('{', i);
                int close = open >= 0 ? format.IndexOf('}', open) : -1;
                if (open < 0 || close < 0)
                {
                    b.Append(format, i, format.Length - i);
                    break;
                }
                b.Append(format, i, open - i);
                string name = format.Substring(open + 1, close - open - 1);
                string text = glyph?.Invoke(name);
                b.Append(string.IsNullOrEmpty(text) ? name.ToUpperInvariant() : text);
                i = close + 1;
            }
            return b.ToString();
        }

        /// <summary>The {names} in a format, in order.</summary>
        public static List<string> Tokens(string format)
        {
            var r = new List<string>();
            if (string.IsNullOrEmpty(format)) return r;
            int i = 0;
            while (true)
            {
                int open = format.IndexOf('{', i);
                int close = open >= 0 ? format.IndexOf('}', open) : -1;
                if (open < 0 || close < 0) break;
                r.Add(format.Substring(open + 1, close - open - 1));
                i = close + 1;
            }
            return r;
        }

        /// <summary>ULTRAKILL's format (teach.md section 1: "Hold 'Left Control' to SLIDE."): one {action} token at least, a verb in capitals (a word of 3+
        /// letters, all capitals, outside the tokens), at most two sentences (the instruction and one rule), at most `maxChars` once resolved. Null: fine;
        /// else what is wrong.</summary>
        public static string Problem(string format, Func<string, string> glyph, int maxChars)
        {
            if (string.IsNullOrEmpty(format)) return "empty";
            List<string> tokens = Tokens(format);
            if (tokens.Count == 0) return "no {action} token";
            string bare = format;
            foreach (string t in tokens) bare = bare.Replace("{" + t + "}", " ");
            bool caps = false;
            foreach (string w in bare.Split(' ', '.', ',', ':', '!', '?', ';'))
            {
                if (w.Length < 3) continue;
                bool all = true;
                foreach (char c in w) if (!(char.IsUpper(c) || c == '\'' || c == '-')) { all = false; break; }
                if (all) { caps = true; break; }
            }
            if (!caps) return "no verb in capitals";
            int sentences = 0;
            foreach (string s in bare.Split('.', '!', '?')) if (s.Trim().Length > 0) sentences++;
            if (sentences > 2) return sentences + " sentences (the verb and at most one rule)";
            string r = Resolve(format, glyph);
            if (r.Length > maxChars) return r.Length + " chars resolved (max " + maxChars + ")";
            return null;
        }
    }
}
