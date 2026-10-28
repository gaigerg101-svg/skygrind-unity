// NIGHT74 shared commit (NIGHT74.md section 4 step 3): the night's reel phases, dispatched from FeelReel.N57.cs's N57ProofBody (the lead's line).
// The partials are declared here and filled by builder L in FeelReel.N74.L.cs: pan74 (L1, the moving-camera flicker meter's fixed slow yaw sweep)
// and reveal74 (L2, a room's "N LEFT" with no enemy on screen, facing away). With no body a phase leaves res[0] NOT WRITTEN (RED), as the N57
// phases do.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N74PanBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N74RevealBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
    }
}
