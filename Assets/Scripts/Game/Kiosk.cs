// NIGHT14 U3-27.7 (agent K): the demo kiosk, a port of skygrind/src/modes/kiosk.js (OVERNIGHT2 N6 item 8, Demo Day 2), the
// web's `?demo=1`: here the -skygrindKiosk command-line flag (Kiosk.Flag). "Behind the title an attract loop runs (...)
// with a flythrough camera; big control hints sit on the title; 60 s without input anywhere puts the game back on the
// title; the pause menu has no way out (no QUIT). The idle clock on frame time."
// This file is the pure logic (no UnityEngine; KioskTests): the flag, the idle clock (`idle`, kiosk.js's `idle`; any
// input pokes it to 0), the reset (config.kiosk.idleReset s without input anywhere but TITLE: the caller emits
// 'kioskReset', the web's event, and goes to TITLE; `resets` is kiosk.js's stats.resets), onTitle (idle 0, the attract
// starts) and debugIdle. The Unity side: SurvivalGame.Kiosk.cs (the devices read, the attract started and stopped, the
// flythrough), UiKiosk.cs (the hints, no QUIT).
// The attract loop, the port's adaptations (NIGHT14 U3-27.7, D-54; the web runs it only in the kiosk and never stops it
// on input, its title menu stays clickable over it):
// - It starts on TITLE: in the kiosk at once when TITLE comes up (kiosk.js onTitle), and on a normal TITLE after
//   design.attract.idleSeconds without input (0 = never there).
// - Any input stops it (Step.StopAttract) and TITLE stays. In the kiosk it comes back after idleSeconds without input, or
//   after config.kiosk.idleReset when idleSeconds is 0 (the kiosk always runs it: its own idle clock then).
// - It never starts on the frame of an input (a click that brought TITLE up means somebody is there), nor while it can't
//   (`canAttract`: a screen open over TITLE, no autopilot); a kiosk start waits until it can.
using System;

namespace Skygrind.Game
{
    public sealed class Kiosk
    {
        /// <summary>What the frame should do (Update's answer).</summary>
        public enum Step { None, Reset, StartAttract, StopAttract }

        /// <summary>The command-line flag (the web's ?demo=1), matched exactly in any case (DevKeys' rule).</summary>
        public const string Flag = "-skygrindKiosk";

        /// <summary>The kiosk is on (the flag).</summary>
        public readonly bool enabled;

        /// <summary>config.kiosk.idleReset: s without input anywhere (outside TITLE) before the game goes back to TITLE.</summary>
        public readonly double idleReset;

        /// <summary>design.attract.idleSeconds: s without input on TITLE before the attract starts (0: never on a normal TITLE).</summary>
        public readonly double attractIdle;

        /// <summary>s since the last input anywhere (kiosk.js idle), on frame time.</summary>
        public double idle;

        /// <summary>kiosk.js stats.resets: the idle resets so far.</summary>
        public int resets;

        bool wasTitle; // TITLE showed last frame (its coming up is onTitle)
        bool startNow; // the kiosk's onTitle asked for the attract

        static int processFlag = -1; // -1: not read yet

        public Kiosk(bool enabled, double idleReset, double attractIdle)
        {
            this.enabled = enabled;
            this.idleReset = idleReset;
            this.attractIdle = attractIdle;
        }

        /// <summary>True when `args` (a command line) holds Kiosk.Flag exactly, in any case. Null args or null entries: skipped.</summary>
        public static bool FlagIn(string[] args)
        {
            if (args == null) return false;
            foreach (string a in args)
                if (a != null && string.Equals(a, Flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>This process's command line holds the flag (read once and cached: the command line doesn't change).</summary>
        public static bool Enabled
        {
            get
            {
                if (processFlag < 0) processFlag = FlagIn(Environment.GetCommandLineArgs()) ? 1 : 0;
                return processFlag == 1;
            }
        }

        /// <summary>s of idle on TITLE before the attract (re)starts: idleSeconds; in the kiosk idleReset when that is 0; else never.</summary>
        public double AttractAfter => attractIdle > 0 ? attractIdle : enabled ? idleReset : double.PositiveInfinity;

        /// <summary>kiosk.js onTitle: TITLE came up. In the kiosk the clock restarts and the attract starts (as soon as it can).</summary>
        public void OnTitle()
        {
            if (!enabled) return;
            idle = 0;
            startNow = true;
        }

        /// <summary>kiosk.js debugIdle: pretend nobody touched anything for `s` seconds.</summary>
        public void DebugIdle(double s) => idle = s;

        /// <summary>
        /// kiosk.js update, once a frame: `frameDt` the frame's time, `input` any input this frame (a key, a mouse button or
        /// move, the wheel, a pad's button or stick), `isTitle` TITLE shows (with the attract behind it or not), `attractOn`
        /// the attract runs, `canAttract` it may start now. Returns what the frame does.
        /// </summary>
        public Step Update(double frameDt, bool input, bool isTitle, bool attractOn, bool canAttract)
        {
            if (isTitle && !wasTitle && !attractOn) OnTitle();
            wasTitle = isTitle;
            if (input)
            {
                idle = 0; // kiosk.js poke
                startNow = false; // somebody is there
                return attractOn ? Step.StopAttract : Step.None;
            }
            idle += frameDt;
            if (attractOn) return Step.None;
            if (enabled && !isTitle && idle >= idleReset)
            {
                idle = 0;
                resets++;
                return Step.Reset; // the caller: events.emit('kioskReset', {}); toTitle()
            }
            if (isTitle && canAttract && (startNow || idle >= AttractAfter))
            {
                startNow = false;
                return Step.StartAttract;
            }
            return Step.None;
        }
    }
}
