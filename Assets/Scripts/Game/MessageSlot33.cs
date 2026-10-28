// NIGHT33 builder U (design.n33.hud33): THE MESSAGE SLOT's rule, pure C# (no UnityEngine; EditMode tests drive it). DESIGN33 U / U1 section 6: one line at the
// top centre, one message at a time, each held at least MinHold (1.2 s), by priority danger > boss line > room title > door hint > lesson. A3 found the
// overlap it replaces: the zone banner queue (two banners at a room clear, 2.4 s each) and the DOOR OPEN cue (CueWait25 gave up after 2.0 s) drawn on top of
// each other. Two kinds of message:
//   - a POST (Post): an event's line with its own display time (a room title, "ROUND 4", a banner): queued; it shows for `seconds` once it is up; a post that
//     waited longer than DropAfter (3 s, U1: "lower is dropped if older than 3 s") is dropped; the same text posted while it shows extends it;
//   - a WANT (Want): a state's line that holds while the state does (the door hint while the door stands open, the lesson while its verb is unused, a boss's
//     teaching line, EXPOSED): set every frame, null clears it.
// Step(now) picks the line: the shown one stays for MinHold whatever happens; after that it stays while it is still wanted / within its time unless a
// higher priority one waits (then a cut-short post goes back to the queue with its time left); else the best waiting line shows (a post before a want of
// the same priority, the oldest post first).
using System;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public sealed class MessageSlot33
    {
        public const int Danger = 0, Boss = 1, Title = 2, Door = 3, Lesson = 4, Count = 5;

        struct PostItem
        {
            public string text;
            public int prio;
            public double born, seconds;
        }

        readonly string[] want = new string[Count];
        readonly List<PostItem> queue = new List<PostItem>(8);
        double shownAt, shownNeed;
        bool shownPost;

        public double MinHold = 1.2, DropAfter = 3.0;

        /// <summary>The line shown now (null: none) and its priority (-1: none).</summary>
        public string Text { get; private set; }
        public int Prio { get; private set; } = -1;
        /// <summary>Lines shown so far, and posts dropped unseen (the reel prints them).</summary>
        public int Shows, Dropped;
        /// <summary>How long the line now shown has been up (s).</summary>
        public double Age(double now) => Text == null ? 0 : now - shownAt;

        public void Clear()
        {
            for (int i = 0; i < Count; i++) want[i] = null;
            queue.Clear();
            Text = null;
            Prio = -1;
            shownPost = false;
        }

        /// <summary>A state's line on channel `prio` (null or empty: none). Call every frame.</summary>
        public void Want(int prio, string text)
        {
            want[prio] = string.IsNullOrEmpty(text) ? null : text;
            live[prio] = false;
        }

        /// <summary>A LIVE state's line (a countdown: "ROUND 2 IN 7" -> "ROUND 2 IN 6"): while it shows, a new text replaces it in place (no new hold, no
        /// flicker); it is one message, held while wanted. Call every frame.</summary>
        public void WantLive(int prio, string text)
        {
            want[prio] = string.IsNullOrEmpty(text) ? null : text;
            live[prio] = true;
        }

        readonly bool[] live = new bool[Count];

        /// <summary>An event's line on channel `prio`, up for `seconds` once it shows.</summary>
        public void Post(int prio, string text, double now, double seconds)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (shownPost && Text == text)
            {
                shownNeed = Math.Max(shownNeed, now - shownAt + seconds); // the same line again: it stays up longer, no second copy
                return;
            }
            for (int i = 0; i < queue.Count; i++) if (queue[i].text == text) return;
            queue.Add(new PostItem { text = text, prio = prio, born = now, seconds = seconds });
        }

        /// <summary>NIGHT44 V2 (vis44, T-69): drop channel `prio` now (its wanted line, its queued posts and, if it is on screen, the line itself,
        /// past MinHold): a room's lesson must not hold over into the next room.</summary>
        public void Drop(int prio)
        {
            if (prio < 0 || prio >= Count) return;
            want[prio] = null;
            live[prio] = false;
            for (int i = queue.Count - 1; i >= 0; i--) if (queue[i].prio == prio) queue.RemoveAt(i);
            if (Prio == prio)
            {
                Text = null;
                Prio = -1;
                shownPost = false;
                shownNeed = 0;
            }
        }

        /// <summary>The line for this frame.</summary>
        public string Step(double now)
        {
            for (int i = queue.Count - 1; i >= 0; i--)
                if (now - queue[i].born > DropAfter) { queue.RemoveAt(i); Dropped++; }
            double age = now - shownAt;
            if (Text != null && !shownPost && live[Prio] && want[Prio] != null && want[Prio] != Text) Text = want[Prio]; // a live line ticks in place
            bool valid = Text != null && (shownPost ? age < shownNeed : want[Prio] == Text);
            if (Text != null && age < MinHold) return Text; // held: nothing replaces a line in its first MinHold
            int bestPrio = int.MaxValue, bestQ = -1;
            string best = null;
            for (int i = 0; i < queue.Count; i++)
                if (queue[i].prio < bestPrio) { bestPrio = queue[i].prio; bestQ = i; best = queue[i].text; }
            for (int c = 0; c < Count; c++)
                if (want[c] != null && c < bestPrio) { bestPrio = c; bestQ = -1; best = want[c]; }
            if (valid && !(bestPrio < Prio)) return Text;
            if (valid && shownPost && shownNeed - age > 0.05) // cut short by a higher line: back to the front of the queue with its time left
                queue.Insert(0, new PostItem { text = Text, prio = Prio, born = now, seconds = shownNeed - age });
            if (best == null)
            {
                Text = null;
                Prio = -1;
                shownPost = false;
                return null;
            }
            if (bestQ >= 0)
            {
                // a post taken from the queue (a re-queued line's index moved by the insert above: find it again by its text)
                int qi = -1;
                for (int i = 0; i < queue.Count; i++) if (queue[i].text == best && queue[i].prio == bestPrio) { qi = i; break; }
                shownNeed = queue[qi].seconds;
                queue.RemoveAt(qi);
                shownPost = true;
            }
            else
            {
                shownPost = false;
                shownNeed = 0;
            }
            Text = best;
            Prio = bestPrio;
            shownAt = now;
            Shows++;
            return Text;
        }
    }
}
