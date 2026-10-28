// NIGHT58 (the shared commit; Tools/night/n58-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff58 (NIGHT57's game) where the row is n58's, GREEN on.
// A partial with no body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node
// Tools/night/n46-build-rows.cjs <tag> on <night>.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a158 L levels (n57-kiln's exits / in view at the door; n58-span's new room)
//   a258 G hits and stillness (kill55's marks after the flash; press57)    a358 V colour and the exits lit (rim56, yellow56, the exit lamp)
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN58 = { "a158", "a258", "a358" };

        static void N58ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a158") N58Level(g, dir, res, offs, pre, ref body);
            else if (s == "a258") N58Play(g, dir, res, offs, pre, ref body);
            else if (s == "a358") N58Colour(g, dir, res, offs, pre, ref body);
        }

        static partial void N58Level(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N58Play(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // G
        static partial void N58Colour(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V
    }
}
