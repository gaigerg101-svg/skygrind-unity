// NIGHT22 leftover 4 (builder M): TRICK CHAIN (design.n22.trickChain), the pure rule. Distinct movement tricks inside a chaining window
// pay a rising style bonus: the first trick of a chain pays only its own award (the web's), each further DISTINCT trick pays
// basePoints x (1 + rise x (links - 2)) with links capped at maxLinks; a repeat of a trick already in the chain pays nothing and does not
// extend the window (spamming one trick is not a chain); the window (steps) restarts at each new distinct trick and the chain ends when it runs out.
// Names are the existing events': the "trick" names (slideJump, dashJump, boostJump, wallJump, padLaunch, slingshot, ...), "slam" (slamUse)
// and "swing" (ropeAttach). Numbers are design.n22.trickChain.* (window, basePoints, rise, maxLinks).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Trick22Chain
    {
        readonly int windowSteps, maxLinks;
        readonly double basePoints, rise;
        readonly HashSet<string> used = new HashSet<string>();
        int left;

        /// <summary>The chain's length now: distinct tricks inside the window (0: no chain).</summary>
        public int Links { get; private set; }

        /// <summary>Total bonus points paid this run.</summary>
        public double Paid { get; private set; }

        public Trick22Chain(GameConfig cfg, double dt)
        {
            windowSteps = MovementMath.StepsFor(cfg.Num("design.n22.trickChain.window"), dt);
            basePoints = cfg.Num("design.n22.trickChain.basePoints");
            rise = cfg.Num("design.n22.trickChain.rise");
            maxLinks = cfg.Int("design.n22.trickChain.maxLinks");
        }

        public void Reset()
        {
            used.Clear();
            Links = 0;
            left = 0;
            Paid = 0;
        }

        /// <summary>One fixed step: the window runs; out of it, the chain ends.</summary>
        public void Step()
        {
            if (left > 0 && --left == 0)
            {
                used.Clear();
                Links = 0;
            }
        }

        /// <summary>A trick happened: the bonus points it pays (0 for the chain's first, a repeat, or when the chain was over).</summary>
        public double Trick(string name)
        {
            if (used.Contains(name)) return 0;
            used.Add(name);
            Links++;
            left = windowSteps;
            if (Links < 2) return 0;
            double pts = basePoints * (1 + rise * (Math.Min(Links, maxLinks) - 2));
            Paid += pts;
            return pts;
        }
    }
}
