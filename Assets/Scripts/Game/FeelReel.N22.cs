// NIGHT22 (the lead, phase F): the reel scenarios for NIGHT22's rows, on the Player, proof-first (NIGHT22.md): a scenario is
// written and seen RED (the row's switch off or its feature absent) before the feature is built. Each scenario is in its owner's
// partial file (FeelReel.N22.<Topic>.cs) as a `partial IEnumerator N22<Topic>(...)`-style hook: the lead's switch below calls the
// hook, and a hook with no body answers "NOT WRITTEN" (which counts as RED). Every result line starts with the scenario's
// PRECONDITIONS (spawn spot, distance to the nearest jump pad and buy station, what was owned): SafeSpot's line.
//   arcline2    M   FeelReel.N22.Arcline.cs      the terminal bought, 3+ husks in a line, a virtual pad press of the GRAPPLE button
//   stamp       S   FeelReel.N22.Stamp.cs        a round clear with a grade: the stamp's entrance and the grade card off POINTS
//   deadline    S   FeelReel.N22.Deadline.cs     a DEADLINE round: the fuse bar, a kill's "+N" pop, the burn flash
//   grind       W   FeelReel.N22.Grind.cs        THE GRIND from the MODE row: a wave, the banner
//   code        W2  FeelReel.N22.Code.cs         a challenge code typed on TITLE, the RESULTS line
//   dusk        W2  FeelReel.N22.Dusk.cs         rounds 1, 8, 15: the sky
//   haptics     M   FeelReel.N22.Haptics.cs      the haptics log for a shockwave, an attach, a kill, DOWN
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseF = { "arcline2", "stamp", "deadline", "grind", "code", "dusk", "haptics", "directed" };

        static bool IsPhaseF(string s) => Array.IndexOf(PhaseF, s) >= 0;

        /// <summary>The owners' scenario bodies. A hook that no builder has written leaves `res[0]` as it is (NOT WRITTEN).</summary>
        static partial void N22ArclineBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N22StampBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N22DeadlineBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N22GrindBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N22CodeBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N22DuskBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N22HapticsBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N22DirectedBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        /// <summary>The preconditions every NIGHT22 scenario prints: where the player stands, and the distance to the nearest jump pad
        /// and buy station (NIGHT21's slidebite read 0 bites because its spawn spot sat beside a jump pad).</summary>
        public static string Preconditions(SurvivalGame g, Vec3 at)
        {
            double pad = double.PositiveInfinity, buy = double.PositiveInfinity;
            foreach (PadSpec p in g.sim.map.pads) pad = Math.Min(pad, Dist2(at, p.pos));
            foreach (Buyables.Item it in g.sim.buyables.list) buy = Math.Min(buy, Dist2(at, it.position));
            return $"at ({at.x:0.0}, {at.z:0.0}); nearest jump pad {pad:0.0} m, nearest buy station {buy:0.0} m; owned {g.loadout.owned.Count} guns";
        }

        static double Dist2(Vec3 a, Vec3 b) => Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));

        /// <summary>An open spot at least `minGap` m (default 12) from every jump pad and every buy station: the search is a 4 m grid
        /// over the floor, scored by the sandbox's clear distance. Falls back to the sandbox's open spot with the gap printed (never
        /// silently: the result line carries Preconditions).</summary>
        public static (Vec3 pos, double yaw, bool gapOk) SafeSpot(SurvivalGame g, double minGap = 12)
        {
            var (open, openYaw, _) = Sandbox.OpenSpot(g.sim);
            MapBox fl = g.sim.map.floor;
            double y = fl.max.y;
            double below = g.sim.cfg.Num("ai.grid.lookBelow"), above = g.sim.cfg.Num("ai.grid.lookAbove");
            Vec3 best = open;
            double bestGap = -1;
            for (double x = fl.min.x + 4; x <= fl.max.x - 4; x += 4)
            {
                for (double z = fl.min.z + 4; z <= fl.max.z - 4; z += 4)
                {
                    if (g.sim.nav.grid.NodeAt(x, y, z, below, above) < 0) continue;
                    var p = new Vec3(x, y, z);
                    double gap = double.PositiveInfinity;
                    foreach (PadSpec pad in g.sim.map.pads) gap = Math.Min(gap, Dist2(p, pad.pos));
                    foreach (Buyables.Item it in g.sim.buyables.list) gap = Math.Min(gap, Dist2(p, it.position));
                    if (gap > bestGap) { bestGap = gap; best = p; }
                }
            }
            if (bestGap >= minGap) return (best, openYaw, true);
            return (open, openYaw, false);
        }

        static IEnumerator N22Scenario(SurvivalGame g, string s, string dir, string[] res, List<Action> offs, double yaw, int pre)
        {
            res[0] = "NOT WRITTEN (RED): no scenario body for '" + s + "'";
            IEnumerator body = null;
            switch (s)
            {
                case "arcline2": N22ArclineBody(g, dir, res, offs, pre, ref body); break;
                case "stamp": N22StampBody(g, dir, res, offs, pre, ref body); break;
                case "deadline": N22DeadlineBody(g, dir, res, offs, pre, ref body); break;
                case "grind": N22GrindBody(g, dir, res, offs, pre, ref body); break;
                case "code": N22CodeBody(g, dir, res, offs, pre, ref body); break;
                case "dusk": N22DuskBody(g, dir, res, offs, pre, ref body); break;
                case "haptics": N22HapticsBody(g, dir, res, offs, pre, ref body); break;
                case "directed": N22DirectedBody(g, dir, res, offs, pre, ref body); break;
            }
            if (body == null) yield break;
            yield return body;
        }
    }
}
