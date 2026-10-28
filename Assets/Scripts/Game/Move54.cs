// NIGHT54 M move54 (design.n54.move54, a VIEW row: NightRows.ViewOn(cfg, "move54"); off = NIGHT53's camera exactly). GG 2026-10-20 (NIGHT54.md):
// "the movement needs to feel cleaner, more fluid, and less choppy" with the core mechanics unchanged (DECIDE54 D-3: the view only). Nothing
// here writes the sim (no Mover, no step result): the impact layer's pacing (how long and how often the loop holds the world on a kill),
// the camera's drawn height after a grounded step (Mover's ground snap stays as it is), and the views' presentation. Every number is the
// row's (design.n54.move54.numbers); none is typed here.
//   1. ImpactConfig: the impact director's own config view (Game/ImpactFrames reads its freeze tiers, budget and gap once, at build). A copy
//      on write of only the paths the row's "impact" block names; everything else is the game's own objects (the player's settings object
//      shared, so HIT FREEZE / REDUCED MOTION / EFFECTS still apply live). A horde's kill freezes hold the camera shorter and less often;
//      parries, the gauntlet, punch kills and boss hits keep theirs (the crunch GG asked for on earlier nights).
//   2. Move54Step: a grounded step's height change (the sim's ground snap down up to collision.groundSnap in one step, a step up) becomes a
//      view offset that a critically damped spring returns to 0, so the camera reaches the true eye height in about 0.1 s; never on a jump,
//      a fall, a landing or a wall ride (both ends of the step grounded, on the ground or sliding).
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Move54
    {
        public const string Numbers = "design.n54.move54.numbers.";

        /// <summary>The row (a view row: NIGHT53's camera when off).</summary>
        public static bool On(GameConfig cfg) => NightRows.ViewOn(cfg, "move54");

        /// <summary>The row's impact numbers (numbers.impact.&lt;key&gt;) and the game's config path each one is read as (paths, not numbers).</summary>
        public static readonly string[,] ImpactPaths =
        {
            { "killS", "design.n42.impact42.killS" },
            { "headshotKillS", "design.n42.impact42.headshotKillS" },
            { "killCapS", "design.n42.impact42.killCapS" },
            { "tableKill", "design.impact.freeze.kill" },
            { "tableHeadshotKill", "design.impact.freeze.headshotKill" },
            { "tableKillCap", "design.impact.freeze.killCap" },
        };

        /// <summary>(7) The row's dynamic resolution numbers (numbers.quality.&lt;key&gt;) and the config path each is read as (Game/Quality's
        /// QualityParams): smaller render scale steps, held longer before a change (wider hysteresis), a longer lockout after a drop that did
        /// not help.</summary>
        public static readonly string[,] QualityPaths =
        {
            { "step", "quality.dynamic.step" },
            { "holdTime", "quality.dynamic.holdTime" },
            { "holdUpTime", "quality.dynamic.holdUpTime" },
            { "lockoutTime", "quality.dynamic.lockoutTime" },
        };

        /// <summary>The impact director's config (ImpactPaths from numbers.impact); `cfg` itself with the row off.</summary>
        public static GameConfig ImpactConfig(GameConfig cfg)
        {
            GameConfig v = View(cfg, "impact", ImpactPaths, out int set);
            ImpactPathsSet = set;
            return v;
        }

        /// <summary>The quality director's config (QualityPaths from numbers.quality); `cfg` itself with the row off.</summary>
        public static GameConfig QualityConfig(GameConfig cfg)
        {
            GameConfig v = View(cfg, "quality", QualityPaths, out int set);
            QualityPathsSet = set;
            return v;
        }

        /// <summary>
        /// `cfg` itself with the row off (or no such block); else a view of it whose `paths` read the row's numbers.&lt;block&gt; (a key the
        /// row lacks, or a path the game lacks, is skipped: the game's own value). Only the copied path's dictionaries are new.
        /// </summary>
        static GameConfig View(GameConfig cfg, string block, string[,] paths, out int set)
        {
            set = 0;
            if (cfg == null || !On(cfg) || !cfg.Has(Numbers + block)) return cfg;
            var root = new Dictionary<string, object>(cfg.root);
            for (int i = 0; i < paths.GetLength(0); i++)
            {
                string key = Numbers + block + "." + paths[i, 0], path = paths[i, 1];
                if (!cfg.Has(key) || !cfg.Has(path)) continue;
                if (SetPath(root, path, cfg.Num(key))) set++;
            }
            if (set == 0) return cfg;
            var view = new GameConfig(root);
            view.movement.settings = cfg.movement.settings; // GameSettings.Apply writes the player's settings into this one object
            return view;
        }

        /// <summary>The paths the last ImpactConfig / QualityConfig re-read from the row (the reel prints them: 0 with the row off).</summary>
        public static int ImpactPathsSet { get; private set; }
        public static int QualityPathsSet { get; private set; }

        /// <summary>
        /// (2) The slow-mo's ease in: the world's slow share (everything ImpactBeginFrame multiplied in after the freeze's run share) goes
        /// down toward its target over easeInS (1 - e^-dt/easeInS a frame) instead of in one frame; up (a slow-mo ending, already eased by
        /// its source) at once. A freeze (run 0) passes through untouched. The sim steps at 120 Hz as always, a few more steps in the ease.
        /// </summary>
        public sealed class SlowEase
        {
            readonly double easeInS;
            double eased = 1;

            public SlowEase(GameConfig cfg) { easeInS = cfg.Num(Numbers + "slowMo.easeInS"); }

            public void Halt() => eased = 1;

            /// <summary>`run`: the impact rules' freeze share this frame (0 frozen); `slow`: the product of the slow sources; returns the share.</summary>
            public double Frame(double frameDt, double run, double slow)
            {
                if (run <= 0) return 0;
                if (slow >= eased || easeInS <= 0) eased = slow;
                else eased += (slow - eased) * (1 - Math.Exp(-Math.Max(0, frameDt) / easeInS));
                return run * eased;
            }
        }

        // Copy on write down the path (the game's dictionaries are never written), then the leaf.
        static bool SetPath(Dictionary<string, object> root, string path, double value)
        {
            string[] keys = path.Split('.');
            Dictionary<string, object> node = root;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (!node.TryGetValue(keys[i], out object child) || !(child is Dictionary<string, object> d)) return false;
                var copy = new Dictionary<string, object>(d);
                node[keys[i]] = copy;
                node = copy;
            }
            node[keys[keys.Length - 1]] = value;
            return true;
        }
    }

    /// <summary>
    /// NIGHT54 M move54 (2): the grounded step's camera height, smoothed in the view. The loop calls StepEnded after each sim step (the
    /// feet before and after it); the render calls Frame once a frame with the drawn feet's height and adds the offset it returns to the
    /// camera only (the feet, the sim and every ray are untouched). Over a frame inside a step's window, the drawn height's change beyond
    /// keepRate x frame time is absorbed into the offset; a critically damped spring (omega) returns it to 0 on frame time.
    /// </summary>
    public sealed class Move54Step
    {
        readonly double omega, stepMin, keepRate, maxOffset, resetJump;
        readonly int gateFrames;
        double offset, vel, lastDrawnY;
        bool haveLast, groundedBefore;
        int gate;

        /// <summary>The offset now (m, added to the camera's height).</summary>
        public double Offset => offset;
        /// <summary>Telemetry (the reel): grounded steps that opened the window, and the largest step seen (m).</summary>
        public int StepsSeen;
        public double MaxStepSeen;

        public Move54Step(GameConfig cfg)
        {
            string n = Move54.Numbers + "step.";
            omega = cfg.Num(n + "omega");
            stepMin = cfg.Num(n + "stepMin");
            keepRate = cfg.Num(n + "keepRate");
            maxOffset = cfg.Num(n + "maxOffset");
            resetJump = cfg.Num(n + "resetJump");
            gateFrames = (int)cfg.Num(n + "gateFrames");
        }

        /// <summary>After each sim step: both ends grounded (on the ground or sliding) and the height changed by stepMin or more opens the window.</summary>
        public void StepEnded(Mover m, Vec3 before, Vec3 after) =>
            StepEnded(m != null && m.grounded && (m.state == MoveState.Ground || m.state == MoveState.Slide), before.y, after.y);

        /// <summary>The same with the step's end state given (grounded: on the ground or sliding) and the feet's heights (N54MoveTests).</summary>
        public void StepEnded(bool groundedNow, double beforeY, double afterY)
        {
            double dy = Math.Abs(afterY - beforeY);
            if (groundedBefore && groundedNow && dy >= stepMin && dy <= resetJump)
            {
                gate = gateFrames;
                StepsSeen++;
                if (dy > MaxStepSeen) MaxStepSeen = dy;
            }
            else if (!groundedNow) gate = 0; // a jump (or a fall) right after a step: its rise is never held
            groundedBefore = groundedNow;
        }

        /// <summary>A new run, a respawn, a screen: no offset.</summary>
        public void Reset()
        {
            offset = vel = 0;
            gate = 0;
            haveLast = false;
            groundedBefore = false;
        }

        /// <summary>Once a rendered frame: the offset (m) to add to the drawn feet's height for the camera. `live`: the sim runs.</summary>
        public double Frame(double drawnY, double frameDt, bool live)
        {
            if (!live) { Reset(); return 0; }
            if (!haveLast) { lastDrawnY = drawnY; haveLast = true; }
            double dy = drawnY - lastDrawnY;
            lastDrawnY = drawnY;
            if (Math.Abs(dy) > resetJump) { offset = vel = 0; gate = 0; return 0; } // a teleport, a respawn, a level's start
            if (gate > 0)
            {
                gate--;
                double keep = keepRate * Math.Max(0, frameDt);
                if (Math.Abs(dy) > keep) offset -= dy - Math.Sign(dy) * keep;
            }
            if (frameDt > 0)
            {
                // the critically damped spring's exact step: x(t) = (x0 + (v0 + w x0) t) e^-wt
                double e = Math.Exp(-omega * frameDt);
                double x0 = offset, v0 = vel, c = v0 + omega * x0;
                offset = (x0 + c * frameDt) * e;
                vel = (v0 - omega * c * frameDt) * e;
            }
            if (offset > maxOffset) { offset = maxOffset; vel = 0; }
            else if (offset < -maxOffset) { offset = -maxOffset; vel = 0; }
            return offset;
        }
    }
}
