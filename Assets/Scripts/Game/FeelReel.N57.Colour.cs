// NIGHT57 builder V: the a357 phase (FeelReel.N57.cs routes it here), the colour reel for V1 colour56 with THE WORLD SATURATION CAP (Render/Cap57Pass.cs).
// Not run by its author (builders never run a Player). One level a reel (L-062), colour56 on in the build copy:
//   bash Tools/night/n47-reels.sh <tag> v1on,n52-foundry,a357          the cap on (colour56 on, NIGHT57)
//   bash Tools/night/n47-reels.sh <tag> v1off,n52-foundry,a357,-skygrindOff57   NIGHT56's colour56 (no cap): the A0 side of the guard
//   bash Tools/night/n47-reels.sh <tag> v1none,n52-foundry,a357,-skygrindOff56  no colour56 at all (NIGHT55's look): the no-colour56 baseline
// (likewise n52-iceline, n52-kiln, n52-span, n52-spire). It is a356's run (FeelReel.N56.Colour.cs C56Scenario: the campaign, God, each framed room's
// ENTRY and MID shot with its sidecar and the frieze's A/B twin) plus, while the cap is live, each shot's "-nocap" twin one frame later with the cap
// hidden (Cap57.Hide): the meter reads the cap's exact pixels by the difference and measures the same view with and without it.
// One line per shot from a356 (`[reel] a356: <room> <kind> <file>`), then `done: ...`: GREEN when a356's run is GREEN, the cap is ON for the level
// (the input: Cap57.Line, passes recorded on the main camera > 0) and every shot has its -nocap twin; with -skygrindOff57 / -skygrindOff56 the line
// says the cap is OFF and stays RED (the comparison sides). The numbers (n56-colour.py; the guard counts "sat (whole world)" FAILs):
//   python -I Tools/night/n56-colour.py Logs/feel/<tag>/v1on/a357 --kinds entry,mid      (and each off side's folder)
//   python -I Tools/night/n57-v-cvd.py grey Logs/feel/<tag>/v1on/a357 ; ... cvd <4 frames>   (the greyscale and deuteranopia guards)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>a357: the -nocap twins written (C56Shot) and the shots that wanted one, this run.</summary>
        static int c57Twins, c57Wanted;

        static partial void N57Colour(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = C57Scenario(g, dir, res, offs, pre);

        static IEnumerator C57Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            c57Twins = c57Wanted = 0;
            int rec0 = Cap57.Recorded, vm0 = Cap57.RecordedVm;
            offs.Add(() => { Cap57.Hide = false; });
            yield return C56Scenario(g, dir, res, offs, pre);
            string a356 = res[0] ?? "NOT WRITTEN";
            bool green356 = a356.StartsWith("done: GREEN", StringComparison.Ordinal);
            bool capOn = Cap57.Value < 1f;
            int recorded = Cap57.Recorded - rec0, recordedVm = Cap57.RecordedVm - vm0;
            bool twins = c57Wanted > 0 && c57Twins == c57Wanted;
            bool green = green356 && capOn && recorded > 0 && twins;
            string tail = a356.StartsWith("done: ", StringComparison.Ordinal) ? a356.Substring(6) : a356;
            res[0] = $"done: {(green ? "GREEN" : "RED")} a357 | THE WORLD SATURATION CAP {(capOn ? "ON" : "OFF")} for {Cap57.Level} (cap {Cap57.Value:0.00}): {Cap57.Line}"
                + $" | passes recorded on the main camera this run {recorded} | -nocap twins {c57Twins}/{c57Wanted}"
                + $" | V4: {Cap57.YellowLine} (the twin hides it too; passes on the viewmodel's camera this run {recordedVm}) | rim56 {(Rim56.On(g.cfg) ? "ON" : "OFF")}, the lit side's gain written {Rim56.LitNow:0.00} (NIGHT56 1.50)"
                + " | the numbers: python -I Tools/night/n56-colour.py <this side's a357 folder> --kinds entry,mid (its CAP A/B line per shot)"
                + " | a356's run: " + tail;
        }

        /// <summary>C56Shot's hook (FeelReel.N56.Colour.cs): while the cap is live, the same view one frame later with the cap hidden
        /// (shot-&lt;name&gt;-nocap.png).</summary>
        static IEnumerator C57NoCapTwin(string dir, string name)
        {
            if (Cap57.Value >= 1f) yield break;
            c57Wanted++;
            Cap57.Hide = true;
            Texture2D t = null;
            try
            {
                for (int i = 0; i < UnityTuning.Cap57TwinFrames; i++) yield return null;
                yield return new WaitForEndOfFrame();
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t != null)
                {
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-" + name + "-nocap.png"), t.EncodeToPNG());
                    c57Twins++;
                }
            }
            finally
            {
                if (t != null) UnityEngine.Object.Destroy(t);
                Cap57.Hide = false;
            }
        }
    }
}
