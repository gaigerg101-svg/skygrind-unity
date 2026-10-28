// NIGHT21 (the lead, phase E): the reel scenarios for NIGHT21's features, on the Player. Written at 13:30 after the Editor
// returned (NIGHT21-LOG.md): only the two that a still camera can show are here, slidebite and arcline. DEADLINE (its fuse
// bar is not built), THE GRIND (the MODE row, the smoke) and INSTANT REPLAY (no view) have no scenario yet.
//   slidebite  a sprint into a slide through three frozen husks in a line: the slideBite events and the kills
//   arcline    the ARCLINE owned (Own(true), the buy itself is Arcline21Tests'), a press at a husk 12 m ahead (the yank:
//              arclinePull, 15 s cooldown), then a press at a second husk inside the cooldown (the hook goes to a surface or a
//              node: ropeAttach)
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseE = { "slidebite", "arcline" };

        static bool IsPhaseE(string s) => Array.IndexOf(PhaseE, s) >= 0;

        static IEnumerator N21Scenario(SurvivalGame g, string s, string dir, string[] res, List<Action> offs, double yaw, int pre)
        {
            var (open, openYaw, _) = Sandbox.OpenSpot(g.sim);
            g.Respawn(open, openYaw * 180 / Math.PI);
            g.input.SetLook(openYaw, 0);
            yield return Frames(10);
            switch (s)
            {
                case "slidebite":
                {
                    if (!g.sim.n21.slideBites)
                    {
                        res[0] = "SKIPPED: design.n21.slideBites is off";
                        yield break;
                    }
                    // the open spot is beside the jump pad at (0, -26.5): running forward steps on it and bounces (build 3-5's 0 bites).
                    // Start from the arena's middle instead, 16 m from that pad, where the floor is flat.
                    double floorY0 = g.Feet.y;
                    if (FloorAt(g, 0, -8, floorY0))
                    {
                        g.Respawn(new Vec3(0, floorY0, -8), openYaw * 180 / Math.PI);
                        g.input.SetLook(openYaw, 0);
                        yield return Frames(10);
                    }
                    int bites = 0, kills = 0;
                    offs.Add(g.events.On("slideBite", e => { bites++; if (((SlideBite21Event)e).killed) kills++; }));
                    Husk a = PlaceAt(g, 3.5, 0, out double hy);
                    Husk b = PlaceAt(g, 5, 0, out _);
                    Husk c = PlaceAt(g, 6.5, 0, out _);
                    if (a == null)
                    {
                        res[0] = "NO SPOT for the husks";
                        yield break;
                    }
                    g.husks.frozen = true;
                    g.input.SetLook(hy, 0);
                    yield return Frames(20);
                    Start(g, s, "camera", dir);
                    yield return Frames(pre);
                    FeelProbe.MarkInput();
                    // as SimHarness slidebite: run for 0.2 s, then the slide key held and pressed every frame while pushing on
                    for (int i = 0; i < Ms(200); i++)
                    {
                        God(g);
                        g.input.SetLook(hy, 0);
                        g.forcedMove = new InputFrame { moveZ = 1 };
                        yield return null;
                    }
                    for (int i = 0; i < Ms(900); i++)
                    {
                        God(g);
                        g.input.SetLook(hy, 0);
                        g.forcedMove = new InputFrame { moveZ = 1, slide = new Skygrind.Core.Button { held = true, pressed = true } };
                        yield return null;
                    }
                    g.forcedMove = new InputFrame();
                    yield return Frames(Ms(500));
                    yield return Stop();
                    res[0] = $"{FeelProbe.FrameCount} frames (slide bites {bites}, kills {kills}; husks placed {(a != null ? 1 : 0) + (b != null ? 1 : 0) + (c != null ? 1 : 0)})";
                    yield break;
                }
                case "arcline":
                {
                    if (!g.sim.n21.arcline || g.sim.arcline21 == null)
                    {
                        res[0] = "SKIPPED: design.n21.arcline is off";
                        yield break;
                    }
                    g.sim.arcline21.Own(true);
                    int pulls = 0, attaches = 0;
                    offs.Add(g.events.On("arclinePull", _ => pulls++));
                    Action<MoverEvent> onMove = e => { if (e.type == MoverEventType.RopeAttach) attaches++; };
                    g.mover.Events += onMove;
                    offs.Add(() => g.mover.Events -= onMove);
                    Husk h = PlaceAt(g, 12, 0, out double hy);
                    if (h == null)
                    {
                        res[0] = "NO SPOT for the husk at 12 m";
                        yield break;
                    }
                    g.husks.frozen = false; // the yank needs the husk to move
                    var at = new Vec3(h.position.x, h.position.y + 1.0, h.position.z);
                    Look(g, at);
                    yield return Frames(20);
                    Start(g, s, "camera", dir);
                    yield return Frames(pre);
                    FeelProbe.MarkInput();
                    g.forcedMove = new InputFrame { grapple = new Skygrind.Core.Button { held = true, pressed = true } };
                    yield return null;
                    g.forcedMove = new InputFrame();
                    yield return Frames(Ms(1500));
                    FeelProbe.Event("arclineSecond");
                    // inside the 15 s cooldown: a second husk, so the press goes to a surface or a node instead
                    Husk h2 = PlaceAt(g, 12, 0, out double hy2);
                    if (h2 != null)
                    {
                        g.input.SetLook(hy2, -0.2); // down at the floor ahead: any surface swings
                        yield return Frames(10);
                    }
                    for (int i = 0; i < Ms(1200); i++)
                    {
                        God(g);
                        g.forcedMove = new InputFrame { grapple = new Skygrind.Core.Button { held = true, pressed = i == 0 } };
                        yield return null;
                    }
                    g.forcedMove = new InputFrame();
                    yield return Frames(Ms(800));
                    yield return Stop();
                    res[0] = $"{FeelProbe.FrameCount} frames (arclinePull {pulls}, ropeAttach {attaches})";
                    yield break;
                }
            }
        }
    }
}
