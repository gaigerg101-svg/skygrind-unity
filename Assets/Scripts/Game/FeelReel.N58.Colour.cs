// NIGHT58 builder V: the a358 phase (FeelReel.N58.cs routes it here), the colour reel for V1 rim56 (the accent's value, Rim58.N58V.cs), V2 yellow56
// and the slate V3 exit lamps (Exit58.N58V.cs; its line only: the lamps live on n57-kiln: L's a157 frames).
// Not run by its author (builders never run a Player). One level a reel (L-062), the rows on in the copy:
//   bash Tools/night/n47-reels.sh <tag> v1on,n52-foundry,a358                          rim56 + yellow56 on (NIGHT58)
//   bash Tools/night/n47-reels.sh <tag> v1off,n52-foundry,a358,-skygrindNoRim56        rim56 alone off (colour56, tokens56, yellow56 as built)
//   bash Tools/night/n47-reels.sh <tag> v2off,n52-foundry,a358,-skygrindNoYellow56     yellow56 alone off
// It is a357's run (a356's shots + the -nocap twins: FeelReel.N57.Colour.cs), then one line: the input (rim56 / yellow56 ON or OFF, the accent
// pieces rim56 scaled, the lit gain, the dim yellow's line) and where the numbers are. GREEN when a357's run is GREEN and
// rim56's accent scale fired (pieces > 0) with rim56 on; with -skygrindNoRim56 / -skygrindOff58 it says so and stays RED (the comparison side).
// The numbers (per frame, the meter the rows are judged by):
//   python -I Tools/night/n56-colour.py Logs/feel/<tag>/v1on/a358 --kinds entry,mid      V2 rim (body over env p90) and V3 yellow per shot
//   python -I Tools/night/n58-v-rimcause.py Logs/feel/<tag>/v1on/a358 Logs/feel/<tag>/v1off/a358   what sets the env p90, the bodies on vs off
//   python -I Tools/night/n57-v-cvd.py grey Logs/feel/<tag>/v1on/a358                       the greyscale guard
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N58Colour(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = C58Scenario(g, dir, res, offs, pre);

        static IEnumerator C58Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            yield return C57Scenario(g, dir, res, offs, pre);
            string a357 = res[0] ?? "NOT WRITTEN";
            bool green357 = a357.StartsWith("done: GREEN", StringComparison.Ordinal);
            bool rim = Rim56.On(g.cfg), yel = Yellow56.On(g.cfg), off58 = Rim58.Off58();
            bool fired = rim && !off58 && Rim58.AccentPieces > 0;
            bool green = green357 && fired;
            string tail = a357.StartsWith("done: ", StringComparison.Ordinal) ? a357.Substring(6) : a357;
            res[0] = $"done: {(green ? "GREEN" : "RED")} a358"
                + $" | V1 rim56 {(rim ? "ON" : "OFF")}{(Rim58.No("-skygrindNoRim56") ? " (-skygrindNoRim56)" : "")}{(off58 ? " (-skygrindOff58: NIGHT57's rim56)" : "")}:"
                + $" the accent's largest channel x {(fired ? UnityTuning.Rim58AccentScale : 1f):0.00} on {Rim58.AccentPieces} pieces, the lit side's gain written {Rim56.LitNow:0.00}"
                + $" | V2 yellow56 {(yel ? "ON" : "OFF")}{(Rim58.No("-skygrindNoYellow56") ? " (-skygrindNoYellow56)" : "")}: {Cap57.YellowLine}"
                + $" | exit lamps (slate V3): {Exit58.Line}"
                + " | the numbers: python -I Tools/night/n56-colour.py <this side's a358 folder> --kinds entry,mid (V2 rim, V3 yellow per shot)"
                + " | a357's run: " + tail;
        }
    }
}
