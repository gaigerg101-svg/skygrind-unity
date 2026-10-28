// NIGHT26 (builder M): the reel scenario `ice`, the surfaces contract's Player half. Written first, compiled by the dn-check, NOT RUN on a Player by the
// builder (no Editor in the builder's worktree). On a Player: the same standing-start sprint (forward held 1.5 s) and the same release, once on normal ground
// (0.45 s: the run, 4 m, plus the ice stop, 6 m, must fit SafeSpot's 12 m clear line: a 1.5 s sprint, 15 m, hit the wall and the stop was measured from the wall) and once on an ice rect laid around the safe spot; the result line prints both top speeds and both stop distances. RED when design.n26.surfaces is off
// (the sim does not take the zones: the stop on "ice" is the normal one) and PASS when the ice stop is at least 1.5x the normal one.
// The line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N26IceBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = IceScenario26(g, dir, res, offs, pre);

        /// <summary>A forward sprint for `runSteps` frames, then the input released until the speed is under 0.1 m/s: (top speed, metres to the stop).</summary>
        static IEnumerator IceRun26(SurvivalGame g, double yaw, int runFrames, double[] topStop)
        {
            Mover m = g.sim.mover;
            double top = 0;
            for (int f = 0; f < runFrames; f++)
            {
                God(g);
                g.input.SetLook(yaw, 0);
                g.forcedMove = new InputFrame { moveZ = 1, sprint = new Skygrind.Core.Button { held = true, pressed = f == 0 } };
                yield return null;
                top = Math.Max(top, m.Speed);
            }
            Vec3 from = m.position;
            g.forcedMove = new InputFrame();
            int max = Ms(6000);
            for (int f = 0; f < max && m.Speed >= 0.1; f++) yield return null;
            topStop[0] = top;
            topStop[1] = Math.Sqrt((m.position.x - from.x) * (m.position.x - from.x) + (m.position.z - from.z) * (m.position.z - from.z));
        }

        static IEnumerator IceScenario26(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            Surfaces26 saved = g.sim.surfaces26;
            offs.Add(() => { g.forcedMove = null; g.sim.surfaces26 = saved; g.sim.mover.groundFrictionMult = 1; });
            bool rowOn = g.sim.n26.surfaces;
            // the zones come from the sim's own rule: with the row off the sim does not take them (the line says so), with it on an ice rect is laid
            // around the spot (the whole run and its stop fit inside)
            var normal = new double[2];
            var ice = new double[2];
            g.sim.surfaces26 = rowOn ? new Surfaces26(SurfaceRules.From(g.cfg)) : null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            Start(g, "ice", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            FeelProbe.Event("normalGround");
            yield return IceRun26(g, spotYaw, Ms(450), normal);
            yield return Frames(Ms(500));
            if (rowOn)
            {
                var z = new Surfaces26(SurfaceRules.From(g.cfg));
                z.zones.Add(new SurfaceZone { kind = SurfaceKind.Ice, x0 = spot.x - 80, x1 = spot.x + 80, z0 = spot.z - 80, z1 = spot.z + 80 });
                g.sim.surfaces26 = z;
            }
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            FeelProbe.Event("iceGround");
            yield return IceRun26(g, spotYaw, Ms(450), ice);
            yield return Stop();
            bool ok = rowOn && ice[1] >= 1.5 * normal[1];
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: sprint 0.45 s then release: normal ground top {normal[0].ToString("0.00", ci)} m/s, stop {normal[1].ToString("0.00", ci)} m; ice (x{SurfaceRules.From(g.cfg).iceMult.ToString("0.00", ci)} friction) top {ice[0].ToString("0.00", ci)} m/s, stop {ice[1].ToString("0.00", ci)} m (design.n26.surfaces {(rowOn ? "on" : "OFF")}): "
                + (ok ? "PASS" : rowOn ? "RED: the ice stop is not 1.5x the normal one" : "RED: design.n26.surfaces is off, ice is normal ground");
        }
    }
}
