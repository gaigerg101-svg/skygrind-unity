// NIGHT57 (the shared commit; Tools/night/n57-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff57 (NIGHT56's game), GREEN on. A partial with no
// body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs
// <tag> on n57.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a157 L level 3 as n57-kiln (per room: entry frame, enemies in view, the room id)
//   a257 G enemy pressure (press57's tell and its hit on a still player, tokens56) and kills (kill55's marks)    a357 V colour (colour56 / grade55 / brief55)
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN57 = { "a157", "a257", "a357", "same63", "tour65", "taps71", "still72", "stuck72", "bosskill72", "spring73", "pan74", "reveal74" }; // NIGHT74: pan74, reveal74 (FeelReel.N74.cs, L fills them); NIGHT73 B: spring73 (FeelReel.N73.B.cs); NIGHT63 D: same63, the same-run toggle's planted scenario (FeelReel.N63Same.cs); NIGHT65 T2: tour65 (FeelReel.N65.Tour.cs); NIGHT71 F4: taps71 (FeelReel.N71.Taps.cs); NIGHT72: still72, stuck72, bosskill72 (FeelReel.N72.cs)

        static void N57ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a157") N57Level(g, dir, res, offs, pre, ref body);
            else if (s == "a257") N57Play(g, dir, res, offs, pre, ref body);
            else if (s == "a357") N57Colour(g, dir, res, offs, pre, ref body);
            else if (s == "same63") body = Same63Scenario(g, dir, res, offs, pre); // NIGHT63 D (D1)
            else if (s == "tour65") body = Tour65Scenario(g, dir, res, offs, pre); // NIGHT65 T2: every room's card poses and verb line (FeelReel.N65.Tour.cs)
            else if (s == "taps71") body = Taps71Scenario(g, dir, res, offs, pre); // NIGHT71 F4: a KESTREL tap pressed and released inside a freeze (FeelReel.N71.Taps.cs)
            else if (s == "still72") N72StillBody(g, dir, res, offs, pre, ref body); // NIGHT72 L: the flicker meter's still camera (FeelReel.N72.cs)
            else if (s == "stuck72") N72StuckBody(g, dir, res, offs, pre, ref body); // NIGHT72 N: goal72's stuck hints (FeelReel.N72.cs)
            else if (s == "bosskill72") N72BossKillBody(g, dir, res, offs, pre, ref body); // NIGHT72 B: the boss kill feel (FeelReel.N72.cs)
            else if (s == "spring73") body = Spring73Scenario(g, dir, res, offs, pre); // NIGHT73 B: spring72's flinch, wind-up and death on one husk at 5 m (FeelReel.N73.B.cs)
            else if (s == "pan74") N74PanBody(g, dir, res, offs, pre, ref body); // NIGHT74 L1: the moving-camera flicker meter's yaw sweep (FeelReel.N74.cs)
            else if (s == "reveal74") N74RevealBody(g, dir, res, offs, pre, ref body); // NIGHT74 L2: reveal74's facing-away case (FeelReel.N74.cs)
        }

        static partial void N57Level(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N57Play(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // G
        static partial void N57Colour(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V
    }
}
