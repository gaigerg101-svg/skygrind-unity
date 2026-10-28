// NIGHT50 shared commit (Tools/night/n50-shared.cjs): the proof reel of tonight's builder H, dispatched from FeelReel.N27.cs (the lead's line).
// a150 = H (the charge bar under the reticle and the charge on the gun: FeelReel.N50.Gun.cs). A partial with no body leaves body null (res[0]
// stays NOT WRITTEN, RED). A reel forces the view rows with the build copy's config (node Tools/night/n39-build-rows.cjs <tag> on n50.<row>).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN50 = { "a150" };

        static void N50ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a150") N50Gun(g, dir, res, offs, pre, ref body);
        }

        static partial void N50Gun(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // H
    }
}
