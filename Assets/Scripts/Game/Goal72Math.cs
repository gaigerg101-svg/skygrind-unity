// NIGHT72 builder N, slice N3 (view row design.n72.goal72; GG 2026-10-26: "there needs to be more text on the screen that explains like OK now you have to go up or whatever, obviously
// make it gamified and easy for the user to understand"; his limit 2026-10-14: "way too much instruction at the start of the level"; scout-guidance.md 1-4: the line ON the thing it
// names, a hint that fires on STUCK once and retires on success, one prompt at a time, the verb in capitals, 4-8 words, the input's own glyph). The PURE half (no Unity types: the
// harness `n72goal` and the EditMode tests drive it): the state machine of one room's goal and its texts. The view that draws it is Goal72View.cs.
//   stage 0  nothing: not a campaign room, the room not cleared, the player within the exit's reach, the level's first seconds, a scripted player
//   stage 1  THE LINE: the room is cleared and the exit is open: one line at the exit, "GO UP to the exit" (the verb in capitals), a marker on it
//   stage 2  after Goal72Hint1S (10 s) with no progress: the line says WHERE ("LOOK UP: the exit is there"), the marker pulses
//   stage 3  after Goal72Hint2S (20 s): the line says HOW (the verb: "DOUBLE JUMP: press {jump} twice"), the marker's pointer joins it
// PROGRESS is the feet coming Goal72ProgressM closer to the exit than they have been in this room (a new record); it resets the clock, and a hint on screen when it happens is RETIRED
// (counted). A verb the player has already used this run is not taught again (the hint falls through to the plain one). Each stage is counted once a room. A scripted player (a reel's
// forced input) never goes past stage 1, so a verbs37 run shows no hint; `-skygrindStuck72` (the planted stuck run) lifts that.
// Numbers: UnityTuning.N72N.cs (Goal72*). Texts: [brackets] mark the capitals the view paints in the player's colour; {jump} / {slide} are glyph placeholders (Lesson25Text.Resolve).
using System;

namespace Skygrind.Game
{
    public static class Goal72Math
    {
        public const string Row = "goal72";
        /// <summary>The one-row off switch (Rows72.cs reads -skygrindNo&lt;Row&gt;72: this row alone off).</summary>
        public const string NoSwitch = "-skygrindNoGoal72";
        /// <summary>REEL ONLY (the planted stuck run): a scripted player counts as a real one for the stuck clock.</summary>
        public const string StuckSwitch = "-skygrindStuck72";

        /// <summary>The verbs a hint might teach; Learn() sets the bit when the player does it (the mover's events).</summary>
        [Flags] public enum Verb { None = 0, DoubleJump = 1, Ride = 2, Slide = 4, Pad = 8 }

        public sealed class State
        {
            public int Stage;           // 0..3 (see the file header)
            public double StuckS;       // game seconds since the last progress (0 while scripted)
            public double Best = double.PositiveInfinity; // the closest the feet have been to the exit in this room (m)
            public int Room = -1;
            public Verb Learned;        // verbs used this run
            // counters since the run started (the reel's / the Player log's line)
            public int Lines, Hint10, Hint20, Retired;
            double since;               // game time of the last progress (or the room's first active step)
            bool shown1, shown2, shown3, retiredHere;
            public string LastRaw = "", LastRawPad = "";

            public void NewRun()
            {
                Stage = 0; StuckS = 0; Best = double.PositiveInfinity; Room = -1; Learned = Verb.None;
                Lines = Hint10 = Hint20 = Retired = 0;
                shown1 = shown2 = shown3 = retiredHere = false;
                since = 0;
                LastRaw = LastRawPad = "";
            }

            public void Learn(Verb v) => Learned |= v;

            /// <summary>One frame. `t` game seconds; `active`: a campaign room that is cleared, the exit open, the player outside the hide radius and past the level's first seconds;
            /// `room` the room index; `dist` the 3-D distance feet to exit (m); `scripted`: a reel's forced input (it never passes stage 1 unless `plantedStuck`);
            /// `dy` the exit's height over the feet (m); `bearingDeg` the exit's bearing off the look (0 ahead, +-180 behind).</summary>
            public void Step(double t, bool active, int room, double dist, bool scripted, bool plantedStuck, double dy, double bearingDeg)
            {
                if (room != Room)
                {
                    Room = room;
                    Best = double.PositiveInfinity;
                    since = t;
                    shown1 = shown2 = shown3 = retiredHere = false;
                    Stage = 0;
                }
                if (!active)
                {
                    Stage = 0;
                    StuckS = 0;
                    since = t; // the clock starts when the goal does
                    Best = double.PositiveInfinity;
                    return;
                }
                if (double.IsPositiveInfinity(Best)) { Best = dist; since = t; }
                bool progress = dist <= Best - UnityTuning.Goal72ProgressM;
                if (progress)
                {
                    if (Stage >= 2 && !retiredHere) { Retired++; retiredHere = true; } // the hint on screen worked: it is spent for this room
                    Best = dist;
                    since = t;
                }
                // (Best is the distance at the last progress, not a running minimum: a steady approach beats it by the margin every few seconds)
                if (scripted && !plantedStuck) since = t; // a scripted player's clock is held: a reel never sees a hint unless it plants the stuck run
                StuckS = Math.Max(0, t - since);
                int stage = StuckS >= UnityTuning.Goal72Hint2S ? 3 : StuckS >= UnityTuning.Goal72Hint1S ? 2 : 1;
                if (retiredHere && stage > 1) stage = 1; // a retired hint does not come back in this room
                Stage = stage;
                if (stage >= 1 && !shown1) { shown1 = true; Lines++; }
                if (stage >= 2 && !shown2) { shown2 = true; Hint10++; }
                if (stage >= 3 && !shown3) { shown3 = true; Hint20++; }
                Format(stage, dy, bearingDeg, Learned, out LastRaw, out LastRawPad);
            }
        }

        /// <summary>The line of a stage as a format: [CAPITALS] the view paints, {action} glyph placeholders, `pad` the pad wording (the same unless it differs).</summary>
        public static void Format(int stage, double dy, double bearingDeg, Verb learned, out string raw, out string pad)
        {
            bool up = dy >= UnityTuning.Goal72UpM, down = dy <= -UnityTuning.Goal72UpM, behind = Math.Abs(bearingDeg) > UnityTuning.Goal72BehindDeg;
            pad = null;
            if (stage <= 1)
            {
                raw = up ? "[GO UP] to the exit" : down ? "[DROP DOWN] to the exit" : "[GO] to the exit";
                return;
            }
            if (stage == 2)
            {
                // NIGHT73: the verb first (NIGHT72's N3 verdict: "The exit is AHEAD" had no capitalised verb)
                raw = up ? "[LOOK UP]: the exit is there" : down ? "[LOOK DOWN]: the exit is below" : behind ? "[TURN AROUND]: the exit is behind" : "[GO AHEAD] to the exit";
                return;
            }
            // stage 3: how
            if (up)
            {
                bool far = dy >= UnityTuning.Goal72RideM;
                if (!far && (learned & Verb.DoubleJump) == 0) { raw = "[DOUBLE JUMP]: press {jump} twice"; return; }
                if ((learned & Verb.Ride) == 0) { raw = "[RIDE] a wall: hold {jump}"; return; }
                raw = "[KEEP GOING] up to the exit";
                return;
            }
            if (down) { raw = "[WALK OFF] the ledge and drop"; return; }
            raw = behind ? "[TURN AROUND] and follow the arrow" : "[FOLLOW] the arrow to the exit";
        }

        /// <summary>Words in a line, counted the way the slate counts them (a placeholder is one word, brackets are not words); the bar: at most 6.</summary>
        public static int Words(string raw)
        {
            int n = 0;
            foreach (string w in raw.Replace("[", " ").Replace("]", " ").Replace(":", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) n++;
            return n;
        }

        /// <summary>The line with [CAPITALS] as TMP rich text (`hex` is "RRGGBB" of the player's colour).</summary>
        public static string Rich(string resolved, string hex) => resolved.Replace("[", "<color=#" + hex + ">").Replace("]", "</color>");
    }
}
