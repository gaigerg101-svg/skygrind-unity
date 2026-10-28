// NIGHT20 U3-34.2 (the lead owns this file): the reel's NIGHT20 scenarios (phase D), run after phases A-C in a full reel.
// - shockwave: the THUNDERCLAP's LT from a standstill, level, then aimed down, then pressed again inside its cooldown
//   (nothing fires; the HUD shows the cooldown). With design.shockwave off it is the THUNDERCLAP's NIGHT19 LT.
// - zoom: the KESTREL, the STITCHER and the LONGWAVE each hold LT on a husk at 20 m for 1.2 s.
// - climb: the player on a 2.5-5 m face's top, one husk at its foot 4-8 m away, live: its tell and its climb (or, with
//   design.routes off, the husk standing below: NIGHT20.md banned pattern 17).
// - special-round: the run skipped to round 8 (THE HUNT's first round with design.adds.hunt): the announce and the first
//   spawns, 5 s.
// - regen: round 1 cleared by the aim bot with the player at 30 HP for its last enemy: the round-end heal on the HUD.
// - rbcycle (the pad pass only): a virtual pad's RB pressed 3 times, 0.5 s apart (the hotbar answers each).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseD = { "shockwave", "zoom", "climb", "special-round", "regen", "rbcycle", "slam" };

        static bool IsPhaseD(string s) => Array.IndexOf(PhaseD, s) >= 0;

        static int SlotOf(SurvivalGame g, string id)
        {
            if (!g.loadout.owned.Contains(id) && g.cfg.Has($"weapons.catalog.{id}")) g.loadout.AddWeapon(id);
            return g.loadout.owned.IndexOf(id);
        }

        static CombatInput Lt(bool pressed) => new CombatInput { weapon = -1, altFire = new Skygrind.Core.Button { held = true, pressed = pressed } };

        static IEnumerator N20Scenario(SurvivalGame g, string s, string dir, string[] res, List<Action> offs, double yaw, int pre)
        {
            switch (s)
            {
                case "shockwave":
                {
                    int slot = SlotOf(g, "shotgun");
                    if (slot < 0)
                    {
                        res[0] = "SKIPPED: no THUNDERCLAP";
                        yield break;
                    }
                    g.forcedCombat = new CombatInput { weapon = slot };
                    yield return Frames(45);
                    g.forcedCombat = IdleCombat;
                    int fired = 0;
                    offs.Add(g.events.On(N20Events.Shockwave, _ => fired++));
                    Start(g, s, "camera", dir);
                    yield return Frames(pre);
                    FeelProbe.MarkInput();
                    g.input.SetLook(yaw, 0);
                    g.forcedCombat = Lt(true);
                    yield return null;
                    g.forcedCombat = IdleCombat;
                    yield return Frames(Ms(1200));
                    FeelProbe.Event("shockwaveDown");
                    g.input.SetLook(yaw, -UnityTuning.ReelGustDownDeg * Math.PI / 180);
                    g.forcedCombat = Lt(true);
                    yield return null;
                    g.forcedCombat = IdleCombat;
                    yield return Frames(Ms(1200));
                    FeelProbe.Event("shockwaveAgain");
                    g.input.SetLook(yaw, 0);
                    g.forcedCombat = Lt(true);
                    yield return null;
                    g.forcedCombat = IdleCombat;
                    yield return Frames(Ms(900));
                    // NIGHT25 shockwaveOff: hold LT half a second and read the zoom (the verdict below)
                    double zoomSeen = 1;
                    for (int f = 0; f < Ms(500); f++)
                    {
                        g.forcedCombat = Lt(f == 0);
                        yield return null;
                        if (f == Ms(500) - 2) zoomSeen = g.loadout.ZoomMag;
                    }
                    g.forcedCombat = IdleCombat;
                    yield return Stop();
                    string verdict = g.sim.n25.shockwaveOff
                        ? (fired == 0 && zoomSeen > 1 ? $" | PASS: no shockwave events on 3 shotgun LT presses; zoom on (x{zoomSeen:0.##})" : $" | FAIL (RED): {fired} shockwaves on 3 presses, zoom x{zoomSeen:0.##}")
                        : $" | RED: design.n25.shockwaveOff is off (NIGHT24's shockwave: {fired} of 3 presses fired)";
                    res[0] = $"{FeelProbe.FrameCount} frames (shockwaves fired {fired} of 3 presses)" + verdict;
                    yield break;
                }
                case "zoom":
                {
                    // NIGHT20 round 2 (logged): the spawn is enclosed (no floor 20 m out on build 2's reel): start from THE
                    // RANGE's open spot (Sandbox.OpenSpot), then 20 m, else 16 / 12 m (the distance is in the header)
                    var (open, openYaw, _) = Sandbox.OpenSpot(g.sim);
                    g.Respawn(open, openYaw * 180 / Math.PI);
                    g.input.SetLook(openYaw, 0);
                    yield return Frames(10);
                    Husk h = PlaceAt(g, 20, 0, out double hy);
                    if (h == null) h = PlaceAt(g, 16, 0, out hy);
                    if (h == null) h = PlaceAt(g, 12, 0, out hy);
                    if (h == null)
                    {
                        res[0] = "NO SPOT for the husk at 20 m";
                        yield break;
                    }
                    g.husks.frozen = true;
                    string[] guns = { "revolver", "nailgun", "rail" };
                    var slots = new int[guns.Length];
                    for (int i = 0; i < guns.Length; i++) slots[i] = SlotOf(g, guns[i]);
                    var at = new Vec3(h.position.x, h.position.y + 1.2, h.position.z);
                    Look(g, at);
                    yield return Frames(30);
                    Start(g, s, "camera", dir, 20);
                    yield return Frames(pre);
                    FeelProbe.MarkInput();
                    for (int i = 0; i < guns.Length; i++)
                    {
                        if (slots[i] < 0) continue;
                        g.forcedCombat = new CombatInput { weapon = slots[i] };
                        Look(g, at);
                        yield return null;
                        g.forcedCombat = IdleCombat;
                        for (int k = 0; k < Ms(500); k++)
                        {
                            God(g);
                            Look(g, at);
                            yield return null;
                        }
                        FeelProbe.Event("zoom:" + guns[i]);
                        for (int k = 0; k < Ms(1200); k++)
                        {
                            God(g);
                            Look(g, at);
                            g.forcedCombat = Lt(k == 0);
                            yield return null;
                        }
                        g.forcedCombat = IdleCombat;
                        yield return Frames(Ms(300));
                    }
                    yield return Stop();
                    res[0] = $"{FeelProbe.FrameCount} frames (3 guns at 20 m)";
                    yield break;
                }
                case "climb":
                {
                    if (!ClimbSpot(g, out Vec3 top, out Vec3 foot))
                    {
                        res[0] = "NO SPOT: no 2.5-5 m face with a floor 4-8 m from its top";
                        yield break;
                    }
                    g.Respawn(top, 0);
                    yield return Frames(10);
                    Husk h = g.husks.Spawn(foot, top);
                    if (h == null)
                    {
                        res[0] = "NO SPAWN at the face's foot";
                        yield break;
                    }
                    g.sim.brain.Attach(h);
                    g.husks.frozen = false;
                    int climbs = 0;
                    offs.Add(g.events.On<ClimbEvent>(N20Events.Climb, e =>
                    {
                        climbs++;
                        FeelProbe.Event("climb:" + e.phase);
                    }));
                    Look(g, new Vec3(foot.x, foot.y + 1, foot.z));
                    Start(g, s, "enemy", dir, Math.Sqrt((top.x - foot.x) * (top.x - foot.x) + (top.z - foot.z) * (top.z - foot.z)));
                    yield return Frames(pre);
                    FeelProbe.MarkInput();
                    for (int i = 0; i < Ms(4500); i++)
                    {
                        God(g);
                        if (h.alive) Look(g, new Vec3(h.position.x, h.position.y + 1, h.position.z), 0);
                        yield return null;
                    }
                    yield return Stop();
                    res[0] = $"{FeelProbe.FrameCount} frames (face {top.y - foot.y:0.0} m, climb events {climbs}, husk dy {h.position.y - foot.y:0.0} m)";
                    yield break;
                }
                case "special-round":
                {
                    g.husks.frozen = false;
                    g.run.DebugSkipTo(8);
                    g.run.phaseSteps = 1; // the break ends at once
                    int announced = 0;
                    offs.Add(g.events.On<SpecialRoundEvent>(N20Events.SpecialRound, e =>
                    {
                        announced++;
                        FeelProbe.Event("special:" + e.name + ":" + e.phase);
                    }));
                    Start(g, s, "ui", dir);
                    FeelProbe.MarkInput();
                    for (int i = 0; i < Ms(5000); i++)
                    {
                        God(g);
                        yield return null;
                    }
                    yield return Stop();
                    res[0] = $"{FeelProbe.FrameCount} frames (round {g.run.round}, special events {announced})";
                    yield break;
                }
                case "regen":
                {
                    g.run.phaseSteps = 1;
                    g.husks.frozen = false;
                    bool cleared = false;
                    offs.Add(g.events.On("roundClear", _ => cleared = true));
                    int guard = 0;
                    while (guard++ < 7200)
                    {
                        God(g);
                        FeelStrip.AimBot(g);
                        int alive = 0;
                        foreach (Enemy e in g.enemies.all) if (e.alive) alive++;
                        if (alive == 1 && g.run.state != SurvivalRun.State.Break && g.run.stats.kills >= UnityTuning.ReelRoundEndKillsBeforeRecord) break;
                        yield return null;
                    }
                    g.health.max = 100;
                    g.health.hp = 30;
                    g.husks.frozen = true; // the last enemy can't hit: the 30 HP stays 30 until the clear
                    Start(g, s, "ui", dir);
                    int n = 0;
                    while (!cleared && n++ < 1200)
                    {
                        FeelStrip.AimBot(g);
                        yield return null;
                    }
                    g.forcedCombat = IdleCombat;
                    FeelProbe.MarkInput(); // the clear
                    yield return Frames(Ms(2600));
                    yield return Stop();
                    res[0] = cleared ? $"{FeelProbe.FrameCount} frames (HP {g.health.hp:0} of {g.health.max:0} 2.6 s after the clear)" : "NO ROUND CLEAR";
                    yield break;
                }
                case "slam":
                {
                    // NIGHT20 round 2 (M8 v2 L4's vertical case): from 8 m over open floor, the slide key pressed in the air
                    // (the web's slam), recorded to the landing and 0.5 s after it
                    if (!DropSpot(g, yaw, UnityTuning.ReelSlamHeight, out Vec3 top))
                    {
                        res[0] = "SKIPPED: no open floor with clearance";
                        yield break;
                    }
                    bool landed = false;
                    Action<MoverEvent> onMove = e =>
                    {
                        if (e.type == MoverEventType.Land) landed = true;
                    };
                    g.Respawn(top, yaw * 180 / Math.PI);
                    g.input.SetLook(yaw, 0);
                    g.mover.Events += onMove;
                    offs.Add(() => g.mover.Events -= onMove);
                    Start(g, s, "camera", dir);
                    yield return Frames(6);
                    FeelProbe.MarkInput();
                    g.forcedMove = new InputFrame { slide = new Skygrind.Core.Button { held = true, pressed = true } };
                    yield return null;
                    int n = 0;
                    while (!landed && n++ < 180)
                    {
                        God(g);
                        g.input.SetLook(yaw, 0);
                        g.forcedMove = new InputFrame { slide = new Skygrind.Core.Button { held = true } };
                        yield return null;
                    }
                    g.forcedMove = new InputFrame();
                    yield return Frames(Ms(500));
                    yield return Stop();
                    res[0] = landed ? $"{FeelProbe.FrameCount} frames (a slam from {UnityTuning.ReelSlamHeight} m)" : "NO LAND in 3 s";
                    yield break;
                }
                case "rbcycle":
                {
                    if (!PadPass)
                    {
                        res[0] = "SKIPPED: the pad pass only";
                        yield break;
                    }
                    foreach (string id in UnityTuning.StripSwapGuns) SlotOf(g, id);
                    Gamepad pad = InputSystem.AddDevice<Gamepad>("ReelPad");
                    offs.Add(() => InputSystem.RemoveDevice(pad));
                    var state = new GamepadState();
                    yield return Frames(30);
                    Start(g, s, "ui", dir);
                    yield return Frames(pre);
                    FeelProbe.MarkInput();
                    int slot0 = g.loadout.targetSlot;
                    GamepadButton cycleBtn = g.padReader.NextWeaponInputButton;
                    g.forcedCombat = null; // NIGHT20 round 3: the reel's IdleCombat masked every pad press (build 4: slot 5 -> 5); the pad drives combat for the taps
                    offs.Add(() => g.forcedCombat = IdleCombat);
                    for (int k = 0; k < 3; k++)
                    {
                        FeelProbe.Event("rb" + (k + 1));
                        state = state.WithButton(cycleBtn, true); // NIGHT20 round 2: the layout's NEXT WEAPON (v3 default: RB), as the smoke presses it
                        InputSystem.QueueStateEvent(pad, state);
                        yield return Frames(4); // a tap (~0.07 s), well under the wheel's hold
                        state = state.WithButton(cycleBtn, false);
                        InputSystem.QueueStateEvent(pad, state);
                        yield return Frames(Ms(500));
                    }
                    yield return Stop();
                    res[0] = $"{FeelProbe.FrameCount} frames (slot {slot0} -> {g.loadout.targetSlot} after 3 taps of {cycleBtn})";
                    yield break;
                }
            }
            res[0] = "SKIPPED: unknown scenario";
        }

        /// <summary>A face for the climb scenario: a grid node 2.5-5 m above a floor node 4-8 m away across a wall (the ray
        /// between them blocked low), the nearest such pair to the player.</summary>
        static bool ClimbSpot(SurvivalGame g, out Vec3 top, out Vec3 foot)
        {
            NavGrid grid = g.sim.nav.grid;
            Vec3 me = g.Feet;
            double best = double.PositiveInfinity;
            top = foot = default;
            float floorY = (float)me.y;
            for (int n = 0; n < grid.count; n++)
            {
                if (grid.blocked[n] != 0) continue;
                double rise = grid.y[n] - floorY;
                if (rise < 2.5 || rise > 5) continue;
                double nx = grid.NodeX(n), nz = grid.NodeZ(n);
                double dme = (nx - me.x) * (nx - me.x) + (nz - me.z) * (nz - me.z);
                if (dme >= best || dme > 60 * 60) continue;
                // a floor node 4-8 m out in one of 8 headings
                for (int h = 0; h < 8; h++)
                {
                    double a = h * Math.PI / 4;
                    for (double d = 4; d <= 8; d += 2)
                    {
                        double fx = nx + Math.Cos(a) * d, fz = nz + Math.Sin(a) * d;
                        int fn = grid.NodeAt(fx, floorY, fz, 0.5, 0.5);
                        if (fn < 0 || grid.blocked[fn] != 0) continue;
                        top = new Vec3(nx, grid.y[n], nz);
                        foot = new Vec3(grid.NodeX(fn), grid.y[fn], grid.NodeZ(fn));
                        best = dme;
                        h = 8;
                        break;
                    }
                }
            }
            return best < double.PositiveInfinity;
        }
    }
}
