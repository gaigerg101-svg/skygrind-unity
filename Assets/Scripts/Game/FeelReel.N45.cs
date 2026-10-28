// NIGHT45 (the shared commit): the arena builders' proof reels, dispatched from FeelReel.N27.cs (two lines, the lead's). Each builder
// fills its own partial in its own file: a145 (A1: FeelReel.N45.Yard.cs), a245 (A2: FeelReel.N45.Mix.cs), a345 (A3: FeelReel.N45.Ring.cs).
// A partial with no body leaves body null (res[0] stays NOT WRITTEN, RED).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN45 = { "a145", "a245", "a345" };

        static void N45ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a145") N45Yard(g, dir, res, offs, pre, ref body);
            else if (s == "a245") N45Mix(g, dir, res, offs, pre, ref body);
            else if (s == "a345") N45Ring(g, dir, res, offs, pre, ref body);
        }

        static partial void N45Yard(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A1
        static partial void N45Mix(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A2
        static partial void N45Ring(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A3
    }
}
