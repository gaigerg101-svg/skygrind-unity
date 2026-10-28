// NIGHT54 (the shared commit; Tools/night/n54-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff54 (NIGHT53's game), GREEN on. A partial with no
// body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs
// <tag> on n54.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a154 L light54 (the flicker count, a still camera)   a254 G grade54 (each boss's grade and its turns)
//   a354 M move54 (the camera's jitter)                    a454 W wick54 (THE WICK's fight in the Player)
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN54 = { "a154", "a254", "a354", "a454" };

        static void N54ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a154") N54Light(g, dir, res, offs, pre, ref body);
            else if (s == "a254") N54Grade(g, dir, res, offs, pre, ref body);
            else if (s == "a354") N54Move(g, dir, res, offs, pre, ref body);
            else if (s == "a454") N54Wick(g, dir, res, offs, pre, ref body);
        }

        static partial void N54Light(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N54Grade(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // G
        static partial void N54Move(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // M
        static partial void N54Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // W
    }
}
