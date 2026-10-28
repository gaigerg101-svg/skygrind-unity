// NIGHT22 U3-36.8 THE DUSK CLOCK (the lead): the reel scenario `dusk`. The player stands at SafeSpot looking a little up at the sky
// (husks frozen, god mode), and the run is skipped to round 1, 8 and 15 in turn, 5 s each: the LUM meter reads the three equal
// time thirds of the scenario, and the mean luminance must fall by at least LUM_FALL (a flat sky, the row off, is RED). The result
// line carries the preconditions, the row's switch, and the clock's share at the end of each segment.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N22DuskBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = DuskScenario(g, dir, res, pre);
        }

        static IEnumerator DuskScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 1.5); // straight up at the zenith (0.3 rad looked at a wall, 1.1 at the wall's top: LUM read flat on both sides)
            g.husks.frozen = true;
            yield return Frames(20);
            Start(g, "dusk", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var shares = new double[3];
            int[] rounds = { 1, 7, 15 }; // not 8 / 13: THE HUNT's swarm rounds lean the sky to DANGER (the reel's second try read 8 and 15 both red: skipping past 8 never fires the hunt's end)
            for (int k = 0; k < rounds.Length; k++)
            {
                g.run.DebugSkipTo(rounds[k]);
                g.run.phaseSteps = 1;
                FeelProbe.Event("dusk:round" + rounds[k]);
                for (int i = 0; i < Ms(5000); i++)
                {
                    God(g);
                    g.input.SetLook(spotYaw, 1.5);
                    yield return null;
                }
                shares[k] = g.DuskShare;
            }
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {Preconditions(g, spot)} (safe gap ok {gapOk}); design.n22.duskClock {(g.sim.n22.duskClock ? "ON" : "off")}; dusk share at rounds 1 / 7 / 15 = {shares[0]:0.00} / {shares[1]:0.00} / {shares[2]:0.00}";
        }
    }
}
