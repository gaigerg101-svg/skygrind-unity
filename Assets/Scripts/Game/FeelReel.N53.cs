// NIGHT53 (the shared commit; Tools/night/n53-shared.cjs): builder U's proof reel, dispatched from FeelReel.N27.cs (the lead's line).
// a153 (U: FeelReel.N53.Polish.cs, polish53): each item prints that its input happened (L-089), then its number; RED with -skygrindOff53
// (NIGHT52's look), GREEN on. A partial with no body leaves body null (res[0] stays NOT WRITTEN, RED). The row is forced by the build copy's
// config (node Tools/night/n46-build-rows.cjs <tag> on n53.polish53); a campaign level boots only via reel.sh + -skygrindMap (L-062).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN53 = { "a153", "a253" }; // EXT53 N: a253, RIDE KILL's proof (FeelReel.N53.RideKill.cs, rideKill53)

        static void N53ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a153") N53Polish(g, dir, res, offs, pre, ref body);
            else if (s == "a253") N53RideKill(g, dir, res, offs, pre, ref body);
        }

        static partial void N53Polish(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U
        static partial void N53RideKill(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // EXT53 N
    }
}
