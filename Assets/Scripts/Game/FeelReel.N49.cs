// NIGHT49 shared commit (Tools/night/n49-shared.cjs): the proof reels of tonight's builders, dispatched from FeelReel.N27.cs (the lead's two
// lines). a149 = H (the charge element: FeelReel.N49.Charge.cs), a249 = X (the impact kit: FeelReel.N49.Fx.cs), a349 = V (the grade and the
// colour script: FeelReel.N49.Look.cs), a449 = L (the level lights: FeelReel.N49.Lights.cs). A partial with no body leaves body null (res[0]
// stays NOT WRITTEN, RED). A reel forces a view row with the build copy's config (node Tools/night/n39-build-rows.cjs <tag> on n49.<row>).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN49 = { "a149", "a249", "a349", "a449" };

        static void N49ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a149") N49Charge(g, dir, res, offs, pre, ref body);
            else if (s == "a249") N49Fx(g, dir, res, offs, pre, ref body);
            else if (s == "a349") N49Look(g, dir, res, offs, pre, ref body);
            else if (s == "a449") N49Lights(g, dir, res, offs, pre, ref body);
        }

        static partial void N49Charge(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // H
        static partial void N49Fx(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // X
        static partial void N49Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V
        static partial void N49Lights(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
    }
}
