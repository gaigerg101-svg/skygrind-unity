// NIGHT55 (the shared commit; Tools/night/n55-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff55 (NIGHT54's game), GREEN on. A partial with no
// body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs
// <tag> on n55.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a155 B bosses (palettes, the brief, the finish)    a255 C camera / look (step-down, SSAO, decor)
//   a355 K kills (speed kills, chains, ink)            a455 R rooms (the mirrors, rides, pads)    a555 S smoke / harness
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN55 = { "a155", "a255", "a355", "a455", "a555" };

        static void N55ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a155") N55Boss(g, dir, res, offs, pre, ref body);
            else if (s == "a255") N55Look(g, dir, res, offs, pre, ref body);
            else if (s == "a355") N55Kill(g, dir, res, offs, pre, ref body);
            else if (s == "a455") N55Room(g, dir, res, offs, pre, ref body);
            else if (s == "a555") N55Smoke(g, dir, res, offs, pre, ref body);
        }

        static partial void N55Boss(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N55Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // C
        static partial void N55Kill(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // K
        static partial void N55Room(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // R
        static partial void N55Smoke(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // S
    }
}
