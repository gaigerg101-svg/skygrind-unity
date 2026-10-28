// NIGHT46 (the shared commit; Tools/night/n46-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (two lines, the
// lead's). Each builder fills its own partial in its own file: a146 (A1: FeelReel.N46.Span.cs), a246 (A2: FeelReel.N46.Spire.cs), a346 (A3:
// FeelReel.N46.Smooth.cs), a446 (B1: FeelReel.N46.Wyrm.cs), a546 (V1: FeelReel.N46.Feel.cs). A partial with no body leaves body null
// (res[0] stays NOT WRITTEN, RED). A reel forces a sim row before the run starts with
// sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wyrm46", true); (docs/NIGHT-ROWS.md).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN46 = { "a146", "a246", "a346", "a446", "a546", "a646" }; // a646: F1's flow rows (FeelReel.N46.Flow.cs; the lead's round-3 ruling)

        static void N46ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a146") N46Span(g, dir, res, offs, pre, ref body);
            else if (s == "a246") N46Spire(g, dir, res, offs, pre, ref body);
            else if (s == "a346") N46Smooth(g, dir, res, offs, pre, ref body);
            else if (s == "a446") N46Wyrm(g, dir, res, offs, pre, ref body);
            else if (s == "a546") N46Feel(g, dir, res, offs, pre, ref body);
            else if (s == "a646") N46Flow(g, dir, res, offs, pre, ref body); // F1
        }

        static partial void N46Flow(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F1

        static partial void N46Span(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A1
        static partial void N46Spire(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A2
        static partial void N46Smooth(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A3
        static partial void N46Wyrm(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B1
        static partial void N46Feel(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V1
    }
}
