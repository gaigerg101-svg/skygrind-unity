// NIGHT71 builder F (LEFTOVER L10 press71, after F4's RED: Research/2026-10-26-wave71/F/F4/card.md). A play-build switch, Game-side only, no
// config row (NIGHT71.md section 7 L10): `-skygrindPress71` turns it on; -skygrindOff71 (or older) keeps it off.
// What it fixes (F4, measured headless: taps inside freezes 6, fired 3): both readers latch a fire press until the next sim step, and a freeze
// runs no step, so a tap pressed and released inside a freeze reaches the first step after it as `pressed`. When the step that STARTED the
// freeze was itself pressed (a press-step shot: the KESTREL while its charge cools, the THUNDERCLAP's air tap, a one-step click), the sim sees
// two pressed steps in a row, and Charge35's n35-r3 rule (pressed && !pressedLast, there for scripts that repeat `pressed` every step) reads
// them as ONE press: the tap is lost. With press71 on, the reader never delivers fire's `pressed` on two steps in a row: a press latched for
// the step right after a pressed step waits one step (8.3 ms at 120 Hz) and is delivered then, so the sim sees two presses; nothing is
// dropped, nothing is doubled, the order is kept. The sim's own rules (Core) are untouched; a tape records what the sim saw (TapeRecorder
// wraps the reader), so a replay is exact; a forced input (a reel's, a bench's) is left as it is. Golden and fingerprints cannot move (they
// run no reader).
// NOT built here: L10's other half, "a fresh press shortens the hit freeze" (the freeze is Core's ImpactRules, which has no way to end a freeze
// and keep a slow-mo: Halt() ends both; hitstop is part of the web's loop, CLAUDE.md rule 4; GG's "kills not crunchy"). A sim decision first.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>The fire press's delivery rule (pure state; one per reader).</summary>
    public sealed class Press71
    {
        public const string Switch = "-skygrindPress71";

        static int on = -1;

        /// <summary>The switch, read once: -skygrindPress71 given and no -skygrindOffNN at or under 71.</summary>
        public static bool On
        {
            get
            {
                if (on < 0)
                {
                    string[] a;
                    try { a = Environment.GetCommandLineArgs() ?? new string[0]; }
                    catch (Exception) { a = new string[0]; }
                    on = NightRows.HasSwitch(a, Switch) && NightRows.OffFrom(a, "-skygrindOff") > 71 ? 1 : 0;
                }
                return on == 1;
            }
        }

        /// <summary>For tests and probes: set the switch (null: read the command line again).</summary>
        public static void ForceOn(bool? v) => on = v.HasValue ? (v.Value ? 1 : 0) : -1;

        bool lastDelivered;
        int pending;

        /// <summary>The probe: presses delivered one step late (each one a press the sim would have merged with the step before).</summary>
        public static int Deferred;

        /// <summary>A new run, a menu, the cursor let go: nothing pending, nothing delivered last.</summary>
        public void Reset()
        {
            lastDelivered = false;
            pending = 0;
        }

        /// <summary>One sim step's fire press: `latched` what the readers' latches give this step; returns what the sim gets. Never two
        /// delivered presses on consecutive steps; a held-back press is delivered on the next step without one (never lost, at most two kept).</summary>
        public bool Deliver(bool latched)
        {
            if (latched && pending < 2) pending++;
            bool d = pending > 0 && !lastDelivered;
            if (d) pending--;
            else if (pending > 0 && latched) Deferred++;
            lastDelivered = d;
            return d;
        }
    }
}
