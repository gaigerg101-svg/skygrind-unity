// NIGHT56 (the shared commit; Tools/night/n56-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff56 (NIGHT55's game), GREEN on. A partial with no
// body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs
// <tag> on n56.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a156 L rooms (the room meter: enemies in the frustum, nearest / second-nearest, tells up, room id, per a452 entry frame)
//   a256 G gameplay / enemies (the rank leaving DULL on a combat frame, tokens, kills)    a356 V colour (keys, accents, rims, yellow)
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN56 = { "a156", "a256", "a356" };

        static void N56ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a156") N56Room(g, dir, res, offs, pre, ref body);
            else if (s == "a256") N56Play(g, dir, res, offs, pre, ref body);
            else if (s == "a356") N56Colour(g, dir, res, offs, pre, ref body);
        }

        static partial void N56Room(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N56Play(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // G
        static partial void N56Colour(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V
    }
}
