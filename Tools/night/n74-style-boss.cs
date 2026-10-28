// NIGHT74 builder L, leftover 5 (gg-proxy-close #10: "D DULL" sits in the middle of every boss fight): the style meter's own arithmetic, run headless on the shipped StyleCore with PortExport/config.json,
// on four inputs a boss fight can give: hits only (the gun on a boss), hits with movement tricks, hits with tricks and parries, and a normal round (kills plus hits). No game, no Unity: Program.Main of a scratch
// console that compiles Assets/Scripts/Core (a scratch csproj lists Core/**/*.cs and this file); the fixed step is the sim's 120 Hz; the state is GROUND (x1) and, in the last rows, AIR (x1.2).
// Prints the meter, the shown rank and the points paid per second at 5 s marks, and the steady state. This file is a tool, not game code (it lives under Tools/night and is compiled only by that scratch project).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

static class StyleBoss74
{
    sealed class Case
    {
        public string name;
        public double hitsPerSecond, killEvery, trickEvery, parryEvery, headshotShare, mult = 1;
        public string state = "GROUND";
        public string weapon = "revolver";
    }

    static void Run(StyleMath.S s, Case c, double seconds)
    {
        var core = new StyleCore(s);
        core.Start();
        core.air34 = true; // design.n34.air34 is ON in the shipped config: the movement multiplier replaces the flat state multiplier (grounded x1, in the air or sliding up to x3)
        core.mult34 = c.mult;
        const double dt = 1.0 / 120;
        double nextHit = 0, nextKill = c.killEvery > 0 ? c.killEvery : double.MaxValue, nextTrick = c.trickEvery > 0 ? c.trickEvery : double.MaxValue, nextParry = c.parryEvery > 0 ? c.parryEvery : double.MaxValue;
        double shot = 0, t = 0, last = 0, paid = 0, lastPaid = 0;
        int hit = 0;
        string[] tricks = { "wallJump", "dashJump", "slideJump", "padLaunch" };
        int trickIx = 0;
        Console.WriteLine("CASE {0}: {1:0.#} hits/s{2}{3}{4}, weapon {5}, state {6}", c.name, c.hitsPerSecond,
            c.killEvery > 0 ? ", a kill every " + c.killEvery + " s" : "", c.trickEvery > 0 ? ", a trick every " + c.trickEvery + " s" : "", c.parryEvery > 0 ? ", a parry every " + c.parryEvery + " s" : "", c.weapon, c.state);
        double sumMeter = 0;
        int n = 0;
        while (t < seconds)
        {
            core.Step(dt);
            t += dt;
            if (t >= nextHit && c.hitsPerSecond > 0)
            {
                nextHit += 1.0 / c.hitsPerSecond;
                shot += 1;
                bool head = c.headshotShare > 0 && (hit++ % (int)Math.Round(1 / c.headshotShare)) == 0;
                paid += core.Hit(c.weapon, c.state, head, 1, false, shot, 1, null);
            }
            if (t >= nextKill) { nextKill += c.killEvery; paid += core.Kill("husk", c.weapon, c.state, 8, false, false, 1, null); }
            if (t >= nextTrick) { nextTrick += c.trickEvery; paid += core.Trick(tricks[trickIx % tricks.Length], tricks[trickIx % tricks.Length], true, c.state); trickIx++; }
            if (t >= nextParry) { nextParry += c.parryEvery; paid += core.Parry(c.state, null, false); }
            if (t - last >= 5 - 1e-9)
            {
                last = t;
                Console.WriteLine("  t {0,3:0} s | meter {1,6:0.0} | shown rank {2} ({3}) | points paid in the last 5 s: {4,5:0}  = {5:0.0} / s | drain at this rank {6:0.0} / s", t, core.meter, core.Rank, s.ranks[core.Rank], paid - lastPaid, (paid - lastPaid) / 5, StyleMath.DrainRate(core.Rank, s));
                lastPaid = paid;
            }
            if (t > seconds / 2) { sumMeter += core.meter; n++; }
        }
        Console.WriteLine("  second half: mean meter {0:0.0}, thresholds {1}", sumMeter / Math.Max(1, n), string.Join(" / ", s.thresholds));
        Console.WriteLine();
    }

    static int Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : "PortExport";
        var cfg = new GameConfig(PortData.Obj(PortData.LoadJson(Path.Combine(root, "config.json"))));
        StyleMath.S s = StyleMath.From(cfg);
        Console.WriteLine("style numbers: drain {0:0.#} + {1:0.#} per rank (per second), hit cap {2:0} / s (burst {3:0}), diminishing x{4} per award of the same name + weapon in {5:0} s, floor {6}, thresholds {7}",
            s.drainBase, s.drainPerRank, s.hitCapPerSecond, s.hitCapBurst, s.dimRepeat, s.dimWindow, s.dimFloor, string.Join(" / ", s.thresholds));
        Console.WriteLine();
        Run(s, new Case { name = "A the gun on a boss, no tricks, no kills", hitsPerSecond = 4 }, 60);
        Run(s, new Case { name = "A2 the same at 8 hits/s with a headshot in four", hitsPerSecond = 8, headshotShare = 0.25 }, 60);
        Run(s, new Case { name = "B hits and a movement trick every 3 s (an enemy near)", hitsPerSecond = 4, trickEvery = 3 }, 60);
        Run(s, new Case { name = "C hits, a trick every 3 s and a parry every 5 s", hitsPerSecond = 4, trickEvery = 3, parryEvery = 5 }, 60);
        Run(s, new Case { name = "D a normal round: a kill every 2 s and 4 hits/s", hitsPerSecond = 4, killEvery = 2 }, 60);
        Run(s, new Case { name = "E hits and a trick every 3 s with the air multiplier at its x3 peak all fight (air34)", hitsPerSecond = 4, trickEvery = 3, state = "AIR", mult = 3 }, 60);
        Run(s, new Case { name = "F E plus a parry every 5 s (a parry pays 40)", hitsPerSecond = 4, trickEvery = 3, parryEvery = 5, state = "AIR", mult = 3 }, 60);
        return 0;
    }
}
