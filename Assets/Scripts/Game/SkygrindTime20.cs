// NIGHT20 U3-34.9 F2 (builder V): SKYGRIND TIME, after Killing Floor 2's Zed Time (the world at 20 % for 3 s, rare and
// earned: wiki.killingfloor2.com Mechanics; Research/2026-10-06-additions-backlog.md row 13). The rule, pure: real time in,
// the world's time scale out (no UnityEngine, no sim state; SkygrindTime20Tests).
// - When: a blast that kills `blastKills` (3) or more with one shot (HitEvent.shot: one trigger pull or blast; a wave's
//   share counts, as ImpactRules' multi-kill), or a headshot kill at style rank `headshotRank` (2: BOLD) or above; the
//   player's only. It starts on the next frame (the loop's ImpactBeginFrame), never while DOWN or in a menu (`blocked`: the
//   moment is dropped, not saved), never with SLOW-MO OFF or REDUCED MOTION (`on`, ImpactRules.slowMoOn) or with the row off
//   (`enabled`: design.adds.skygrindTime AND design.adds).
// - How rare: at most `perWindow` (1) start in any `window` (60) s and none within `minGap` (20) s of another, on the real
//   clock while the sim runs (KF2's lesson: a rare effect made common stops being special): 0.3-1.0 a combat minute.
// - How much: the world at `scale` (0.25) for `hold` (1.5) s of real time, then back to 1 over `easeOut` (0.15) s
//   (smoothstep); the hold waits out an impact freeze (it counts only while the world isn't frozen). The look's share
//   (Look: the screen leaning toward INK, the AIR streaks frozen) leans in over `lookIn` (0.06) s and out with the ease.
// The loop takes min(the impact layer's share, this scale) (SurvivalGame.SkygrindTime20.cs): never stacked with the
// impact slow-mos, the slower wins; the sim steps at 120 Hz as always (fewer steps a real second), so the fingerprint and
// the tapes hold.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>SKYGRIND TIME's rule: when it starts and how slow the world runs (pure: real time in, a scale out).</summary>
    public sealed class SkygrindTime20
    {
        public struct Params
        {
            public double scale, hold, easeOut, minGap, window, lookIn;
            public int blastKills, headshotRank, perWindow;
        }

        /// <summary>design.adds.skygrindTime's numbers when a key is missing (the same as the config's).</summary>
        public static Params Defaults => new Params
        {
            scale = 0.25, hold = 1.5, easeOut = 0.15, minGap = 20, window = 60, perWindow = 1, blastKills = 3, headshotRank = 2, lookIn = 0.06,
        };

        /// <summary>design.adds.skygrindTime's numbers (read once).</summary>
        public static Params From(GameConfig cfg)
        {
            Params d = Defaults;
            const string b = "design.adds.skygrindTime.";
            double N(string k, double dflt) => cfg.Has(b + k) ? cfg.Num(b + k) : dflt;
            return new Params
            {
                scale = N("scale", d.scale), hold = N("hold", d.hold), easeOut = N("easeOut", d.easeOut), minGap = N("minGap", d.minGap),
                window = N("window", d.window), perWindow = (int)N("perWindow", d.perWindow), blastKills = (int)N("blastKills", d.blastKills),
                headshotRank = (int)N("headshotRank", d.headshotRank), lookIn = N("lookIn", d.lookIn),
            };
        }

        readonly Params p;
        readonly double[] startTimes; // the last perWindow starts (a ring)
        int startHead;

        /// <summary>The row is on for this run (design.adds.skygrindTime AND design.adds).</summary>
        public bool enabled;
        /// <summary>SLOW-MO on and REDUCED MOTION off (ImpactRules.slowMoOn).</summary>
        public bool on;

        double now, lastStart = double.NegativeInfinity, age, lookT;
        bool active, pending, started;
        string pendingCause, cause;
        int multiShot = -1, multiKills;
        bool multiDone;

        public SkygrindTime20(Params p)
        {
            this.p = p;
            startTimes = new double[Math.Max(1, p.perWindow)];
            for (int i = 0; i < startTimes.Length; i++) startTimes[i] = double.NegativeInfinity;
        }

        /// <summary>Starts so far (this game).</summary>
        public int Starts { get; private set; }

        /// <summary>Running (held or easing back).</summary>
        public bool Active => active;

        /// <summary>It started on this BeginFrame (the `skygrindTime` event's frame).</summary>
        public bool StartedThisFrame => started;

        /// <summary>The last start's cause: "blast" or "headshot".</summary>
        public string Cause => cause;

        /// <summary>A `hit` on the bus (the sim's step) with the style's rank then: counts a shot's kills; a qualifying moment
        /// waits for the next frame.</summary>
        public void OnHit(HitEvent e, int rank)
        {
            if (e == null || e.owner != "player" || !e.killed) return;
            if (e.shot > 0)
            {
                if (e.shot == multiShot) multiKills++;
                else
                {
                    multiShot = e.shot;
                    multiKills = 1;
                    multiDone = false;
                }
                if (!multiDone && multiKills >= p.blastKills)
                {
                    multiDone = true;
                    pending = true;
                    pendingCause = "blast";
                    return;
                }
            }
            if (e.headshot && rank >= p.headshotRank && !pending)
            {
                pending = true;
                pendingCause = "headshot";
            }
        }

        /// <summary>
        /// Once a frame while the sim runs (real `realDt`): `blocked` DOWN or a menu, `frozen` an impact freeze this frame.
        /// Starts a waiting moment when it may, ages the slow-mo, and returns the world's scale (1: real time).
        /// </summary>
        public double BeginFrame(double realDt, bool blocked, bool frozen)
        {
            started = false;
            if (realDt > 0) now += realDt;
            bool want = pending;
            pending = false; // a moment waits one frame, never longer
            if (active && !frozen && realDt > 0)
            {
                age += realDt;
                lookT += realDt;
                if (age >= p.hold + p.easeOut) active = false;
            }
            if (want && !active && enabled && on && !blocked && Allowed())
            {
                active = true;
                started = true;
                age = 0;
                lookT = realDt;
                lastStart = now;
                startTimes[startHead] = now;
                startHead = (startHead + 1) % startTimes.Length;
                cause = pendingCause;
                Starts++;
            }
            if (blocked && active) active = false; // DOWN or a menu ends it at once
            return Scale;
        }

        bool Allowed()
        {
            if (now - lastStart < p.minGap) return false;
            int inWindow = 0;
            for (int i = 0; i < startTimes.Length; i++)
                if (now - startTimes[i] < p.window) inWindow++;
            return inWindow < Math.Max(1, p.perWindow);
        }

        static double Smooth(double u)
        {
            u = u < 0 ? 0 : u > 1 ? 1 : u;
            return u * u * (3 - 2 * u);
        }

        /// <summary>The world's time scale now: `scale` while held, back to 1 over the ease, 1 otherwise.</summary>
        public double Scale
        {
            get
            {
                if (!active) return 1;
                if (age < p.hold) return p.scale;
                return p.scale + (1 - p.scale) * Smooth((age - p.hold) / Math.Max(1e-6, p.easeOut));
            }
        }

        /// <summary>The look's share (0..1): in over lookIn, whole while held, out with the ease.</summary>
        public double Look
        {
            get
            {
                if (!active) return 0;
                double inShare = p.lookIn > 0 ? Math.Min(1, lookT / p.lookIn) : 1;
                double outShare = age < p.hold ? 1 : 1 - Smooth((age - p.hold) / Math.Max(1e-6, p.easeOut));
                return inShare * outShare;
            }
        }

        /// <summary>The way to TITLE (or a new run): ended, nothing waiting.</summary>
        public void Halt()
        {
            active = false;
            pending = false;
            started = false;
            multiShot = -1;
            multiKills = 0;
        }
    }
}
