// NIGHT72 shared commit (FORGE72 D-3): the night's reel phases, dispatched from FeelReel.N57.cs's N57ProofBody (the lead's line). The partials are
// declared here and filled by their builders in their own files: still72 (L: FeelReel.N72.L.cs, the flicker meter's still-camera phase), stuck72
// (N: FeelReel.N72.N.cs, goal72's stuck hints at 10 s and 20 s), bosskill72 (B: FeelReel.N72.B.cs, the kill feel on a boss). With no body a phase
// leaves res[0] NOT WRITTEN (RED), as the N57 phases do.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N72StillBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N72StuckBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // N
        static partial void N72BossKillBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
    }
}
