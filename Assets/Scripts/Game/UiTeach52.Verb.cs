// NIGHT52 builder U, hud52 round 14 (NIGHT52.md leftover 5: "Teach lines that wait for the verb, not a timer"; UiTeach35's lines only time out,
// 5 s after the slot shows them). With hud52 on, a teach35 line whose verb the view can hear (UnityTuning.Teach52Verbs: the Mover's events for the
// moves, the sim's "fire" / "charge35Start" / "whip33Latch" / "punch" events for the guns) stays up until the player does that verb after it showed
// (at least Teach52VerbMinS), then ends as any line ends (the next queued line follows; the teach43 card closes). It does not block the queue: when
// another line is waiting it ends at NIGHT51's time; and Teach52VerbSafetyS ends it anyway (a player who never does it is not stuck with it). While it
// waits, hud52's card shrinks to its chip after 6 s as before (the reminder stays small). A line with no verb to hear keeps NIGHT51's timer. View
// only: it listens to events the sim already sends and changes only when the view lets a line go. Off (the row off, -skygrindOff52, the a152 reel's
// ForceOff): NIGHT51's timer exactly. Numbers: UnityTuning.U52.cs.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>hud52 round 14's statics for the a152 reel.</summary>
    public static class Verb52Probe
    {
        /// <summary>Lines that waited for a verb, ended by it, ended by the safety timeout; the verb the last one waited for.</summary>
        public static int Waited, Cleared, Safety;
        /// <summary>Round 15: "fire" events heard since the listener was wired (the reel tells a shot that never fired from a line that did not hear it).</summary>
        public static int Fires;
        public static string Verb = "";
    }

    public sealed partial class Ui
    {
        readonly Dictionary<string, int> v52Counts = new Dictionary<string, int>(); // pure .NET: safe in an initialiser
        bool v52Wired;
        string v52Verb;
        int v52Base;

        int Verb52Count(string k) => v52Counts.TryGetValue(k, out int n) ? n : 0;
        void Verb52Add(string k) => v52Counts[k] = Verb52Count(k) + 1;

        /// <summary>Listens once (hud52 built): the Mover's move events and the sim's gun events, counted by verb.</summary>
        void WireVerb52()
        {
            if (v52Wired || game == null || game.events == null || game.mover == null) return;
            v52Wired = true;
            game.mover.Events += e =>
            {
                switch (e.type)
                {
                    case MoverEventType.Dash: Verb52Add("dash"); break;
                    case MoverEventType.SlideStart: Verb52Add("slide"); break;
                    case MoverEventType.WallRideStart: Verb52Add("wallride"); break;
                    case MoverEventType.WallJump: Verb52Add("walljump"); break;
                    case MoverEventType.RopeAttach: Verb52Add("grapple"); break;
                    case MoverEventType.DashJump: Verb52Add("jumpdash"); break;
                }
            };
            game.events.On("fire", o => { Verb52Add("fire"); Verb52Probe.Fires++; if (o is string gun) Verb52Add("fire:" + gun); });
            game.events.On("charge35Start", o => Verb52Add("charge"));
            game.events.On("whip33Latch", o => Verb52Add("whip"));
            game.events.On("punch", o => { if (!(o is MeleeEvent m) || m.owner == null || m.owner == "player") Verb52Add("punch"); });
        }

        /// <summary>From Teach35Want when a line first shows: the verb it waits for (none: NIGHT51's timer) and the count to beat.</summary>
        void Teach52VerbMark(Teach35Event e)
        {
            v52Verb = null;
            if (!H52Live || e == null || e.id == null || !UnityTuning.Teach52Verbs.TryGetValue(e.id, out string verb)) return;
            WireVerb52();
            if (!v52Wired) return;
            v52Verb = verb;
            v52Base = Verb52Count(verb);
            Verb52Probe.Waited++;
            Verb52Probe.Verb = verb;
        }

        /// <summary>Teach35Want's end test for the line showing `shown` s: NIGHT51's (shown >= its seconds) unless hud52 waits for its verb.</summary>
        bool Teach35Over(Teach35Event e, double shown)
        {
            if (v52Verb == null || !H52Live) return shown >= e.seconds;
            if (shown >= UnityTuning.Teach52VerbSafetyS) { Verb52Probe.Safety++; return true; }
            if (t35Queue.Count > 0 && shown >= e.seconds) return true; // another line is waiting: it ends at NIGHT51's time
            if (Verb52Count(v52Verb) > v52Base && shown >= UnityTuning.Teach52VerbMinS) { Verb52Probe.Cleared++; return true; }
            return false;
        }
    }
}
