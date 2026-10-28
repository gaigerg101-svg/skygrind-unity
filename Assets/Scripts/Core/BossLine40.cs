// NIGHT40 builder B5 (NIGHT40.md B5 + <boss_rules> 5: "the damage rule is taught on entry (one line with the bound buttons, generalised from
// bossLine30)"): ONE boss line any boss can show. bossLine30 (Lantern30.cs + SurvivalSim.Systems30 BossLine30) was the Lantern's alone, went
// through Teach30's budget and LessonEvent id "boss", and the hud32 rule drops an id "boss" line at once in a fight BossMask32 does not know
// (every NIGHT37+ boss: the WICK, the STAG, the LOOM, the CROWN, the brawler). This class owns its line end to end:
//   Begin(boss, roomId, room)   a new fight (not a retry): every key's show count back to 0. The line belongs to the boss room `roomId`
//                               (LessonEvent.id = the room's id, kind "boss40": the room-line rule keeps it while the player is in that room;
//                               hud32 ignores it).
//   Say(key, text, seconds, textPad, maxShows)   show `text` (a format: {dash} {jump} {fire} {punch} {grapple} ... resolved to the bound
//                               buttons by the lesson widget, UiLesson25) for `seconds`, at most maxShows times per fight; false = over the
//                               budget (nothing said). Emits "lessonBegin" (the widget) and "bossLine40" (BossLine30's payload: the reels).
//   Step()                      once per sim step: the line's whole-step clock; at 0 it goes ("lessonClear").
//   Clear()                     the fight is over (the kill, the run's end): the line goes now if it is up.
// Pure (no UnityEngine), whole steps, no randomness, no allocation per step (the payloads and the count table are made once). No audio:
// event "bossLine40" for GG's sound (and "lessonBegin" / "lessonClear", the widget's).
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class BossLine40
    {
        readonly EventBus events;
        readonly double dt;
        readonly Dictionary<string, int> shows = new Dictionary<string, int>();
        readonly LessonEvent lesson = new LessonEvent { kind = "boss40" };
        readonly BossLine30 said = new BossLine30();
        int left;

        /// <summary>The boss whose fight this is ("crown37", ...), the line up now (null: none) and its key.</summary>
        public string boss, Now, Key;
        /// <summary>Counters (harness, reel): lines shown, lines refused over the budget, lines cleared by time / by Clear.</summary>
        public int shown, refused, expired, cleared;

        public BossLine40(EventBus events, double dt)
        {
            this.events = events;
            this.dt = dt;
        }

        /// <summary>A new fight: the counts start over; the line (if any) goes.</summary>
        public void Begin(string boss, string roomId, int room)
        {
            Clear();
            this.boss = boss;
            lesson.id = string.IsNullOrEmpty(roomId) ? "boss40" : roomId;
            lesson.room = room;
            shows.Clear();
            shown = refused = expired = cleared = 0;
        }

        /// <summary>Show a line for `seconds` (at most maxShows times per fight for this key). False: over the budget or empty.</summary>
        public bool Say(string key, string text, double seconds, string textPad = null, int maxShows = 1)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(text)) return false;
            shows.TryGetValue(key, out int n);
            if (n >= maxShows) { refused++; return false; }
            shows[key] = n + 1;
            shown++;
            Now = text;
            Key = key;
            left = System.Math.Max(1, MovementMath.StepsFor(seconds, dt));
            lesson.prompt = text;
            lesson.promptPad = textPad ?? text;
            lesson.index = n;
            events.Emit("lessonBegin", lesson);
            said.boss = boss;
            said.key = key;
            said.text = text;
            said.seconds = seconds;
            said.show = n + 1;
            events.Emit("bossLine40", said);
            return true;
        }

        /// <summary>How many times `key` showed this fight.</summary>
        public int Shown(string key) => key != null && shows.TryGetValue(key, out int n) ? n : 0;

        /// <summary>One sim step: the line's clock.</summary>
        public void Step()
        {
            if (Now == null) return;
            if (--left > 0) return;
            expired++;
            Gone();
        }

        /// <summary>The line goes now (the fight is over).</summary>
        public void Clear()
        {
            if (Now == null) return;
            cleared++;
            Gone();
        }

        void Gone()
        {
            Now = Key = null;
            left = 0;
            events.Emit("lessonClear", lesson);
        }
    }
}
