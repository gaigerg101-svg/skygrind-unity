// NIGHT34 builder H: the Player scenario of the rail, the bend's look and nomark (FeelReel.N27.cs PhaseN34H "hud34"; the body is the partial N34HudBody).
// Not run by its author (no Player in a worktree): the lead runs it. Input is the reel's own (g.forcedCombat / g.forcedMove / g.input.SetLook), never
// a device, so a focus change cannot disable it (the hardened pattern of b8655b3c, L-046: no virtual pad press anywhere here).
//   hud34   any SURVIVAL map (a fresh run), 1920 x 1080 recorded, then 1280 x 720 stills (rest, one swap):
//     rail5      five guns owned (revolver + shotgun / gauntlet / nailgun / rocket as the catalog has them): all five slots, the hand highlighted
//                                                                                                     shot hud34-1080-rail5
//     swap-1..4  four swaps, each to the next slot; the highlighted slot printed each time           shots hud34-1080-swap-1 .. -4
//     cooling    a gun fired (the first whose cooldown shows: >= Hud30MinCoolSeconds), then swapped off: its line drains (CyanDeep, never red)
//                                                                                                     shot hud34-1080-cooling
//     ready      the holstered gun turns ready: its one pulse (Ui.Rail34Pulses)                     shot hud34-1080-ready
//     bend       a bend forced THROUGH THE SIM: sim.Bend34LatchForCheck() (builder B: a latch on the next step; needs design.n34.bend34 on):
//                the grade, the sky dim, the entry thump, the rail brightened                          shot hud34-1080-bend
//                a swap inside it (the hands' snap)                                                  shot hud34-1080-bend-swap
//                a kill inside it (the grade a step deeper)                                          shot hud34-1080-bend-kill
//                after it eased out (the grade and the sky back to 0)                                shot hud34-1080-bend-exit
//     nomark     aimed at a grapple node and at a wall for 0.5 s each: Grapple31View.MarkShown (the target exists) vs MarkDrawn (0 with nomark34),
//                the node's ring visible (GrappleNodes30.RingsShown)                                  shots hud34-1080-nomark-node, -nomark-wall
//     boss       THE STAG begun (BossRoomBegin glacier): the rail with the boss bar                 shot hud34-1080-boss
//   At each shot the hud33 rule checker (Hud33Layout.Check on the live text rects, HUD33's bars and the rail's rect) runs; the problems are summed.
//   PASS: rail34 on, every shot clean, 5 slots shown, 4 of 4 swaps moved the highlight to the wanted slot, the bend seen (depth >= 0.9, grade > 0,
//   sky > 0, the rail's outlines brighter than at rest, the grade back to 0 after), nomark 0 drawn marks in both aims (with the row on) and the ring
//   seen at the node. With -skygrindOff34 the same run is the RED half: no rail (hud33's pips and switch list), the diamond drawn, no grade.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N34HudBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "hud34") body = Hud34Scenario(g, dir, res, offs, pre);
        }

        /// <summary>One shot and its rule check: HUD33's live items plus the rail's rect ("Rail 34") while it is drawn.</summary>
        static IEnumerator Hud34Shot(string dir, string name, List<string> lines, int[] bad)
        {
            yield return Shot30(dir, name);
            List<Hud33Layout.Item> items = Hud33LiveItems(out double cw);
            if (Ui.Rail34Live && Ui.Rail34Root != null && Ui.Rail34Root.gameObject.activeInHierarchy)
            {
                var c = new Vector3[4];
                Ui.Rail34Root.GetWorldCorners(c);
                float k = Screen.height > 0 ? 1080f / Screen.height : 1f;
                items.Add(new Hud33Layout.Item { name = Hud33Layout.RailName, x = c[0].x * k, y = (Screen.height - c[1].y) * k, w = (c[2].x - c[0].x) * k, h = (c[1].y - c[0].y) * k });
            }
            List<string> found = Hud33Layout.Check(items, cw, 1080);
            bad[0] += found.Count;
            lines.Add(found.Count == 0 ? $"{name}: clean (f{FeelProbe.FrameCount:0000}, rail '{Ui.Rail34Describe()}')" : $"{name}: {found.Count} problem(s) (f{FeelProbe.FrameCount:0000}): {string.Join(" ;; ", found.GetRange(0, Math.Min(6, found.Count)))}");
        }

        /// <summary>A swap by the reel's own combat input to owned slot `to`; waits (sim time) for the gun in hand to change and the rail's slide to end.</summary>
        static IEnumerator Swap34(SurvivalGame g, int to)
        {
            g.forcedCombat = new CombatInput { weapon = to };
            yield return Frames(2);
            g.forcedCombat = IdleCombat;
            string want = to >= 0 && to < g.loadout.owned.Count ? g.loadout.owned[to] : null;
            yield return Sim33(g, 1.0, null, () => g.loadout.Weapon == want && Ui.Rail34SlideNow >= 1f);
            yield return Sim33(g, 0.1);
        }

        static IEnumerator Hud34Pass(SurvivalGame g, string dir, string tag, bool full, List<string> lines, int[] bad, int[] seen, List<Action> offs)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            Loadout lo = g.loadout;
            g.forcedCombat = IdleCombat;
            // five guns owned
            foreach (string w in new[] { "shotgun", "gauntlet", "nailgun", "rocket" })
                if (lo.owned.Count < 5 && lo.owned.IndexOf(w) < 0 && g.cfg.Has($"weapons.catalog.{w}")) lo.AddWeapon(w);
            yield return Sim33(g, 0.5);
            yield return Hud34Shot(dir, $"hud34-{tag}-rail5", lines, bad);
            lines.Add($"rail5 {tag}: owned {lo.owned.Count} [{string.Join(",", lo.owned)}], slots drawn {Ui.Rail34Slots}, hand slot {Ui.Rail34HandSlot} ({lo.Weapon}), rail live {Ui.Rail34Live}, hud33 pips row hidden {(Ui.Hud33Root != null && Ui.Hud33Root.Find("Weapon pips") is Transform pr ? !pr.gameObject.activeInHierarchy : true)}");
            if (Ui.Rail34Live && Ui.Rail34Slots == Math.Min(lo.owned.Count, UnityTuning.Rail34MaxSlots) && lo.owned.Count >= 5 && Ui.Rail34HandSlot == lo.owned.IndexOf(lo.Weapon)) seen[0]++;
            // four swaps, each to the next slot
            int swaps = full ? 4 : 1;
            for (int k = 1; k <= swaps; k++)
            {
                int from = Ui.Rail34HandSlot, to = (lo.owned.IndexOf(lo.Weapon) + 1) % lo.owned.Count;
                int sw0 = Ui.Rail34Swaps;
                yield return Swap34(g, to);
                yield return Hud34Shot(dir, $"hud34-{tag}-swap-{k}", lines, bad);
                bool moved = Ui.Rail34HandSlot == to && Ui.Rail34Swaps > sw0;
                lines.Add($"swap {k} {tag}: highlighted slot {from} -> {Ui.Rail34HandSlot} (wanted {to}, {lo.Weapon}), slide {Ui.Rail34SlideNow.ToString("0.00", ci)}, swaps seen {Ui.Rail34Swaps - sw0}{(moved ? "" : " NOT MOVED")}");
                if (moved) seen[1]++;
            }
            if (!full) yield break;
            // a cooldown that shows, then the holstered READY pulse
            string cooled = null;
            for (int i = 0; i < lo.owned.Count && cooled == null; i++)
            {
                if (lo.owned.IndexOf(lo.Weapon) != i) yield return Swap34(g, i);
                yield return Sim33(g, 0.3); // the raise's fire step
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } };
                yield return Frames(2);
                g.forcedCombat = IdleCombat;
                yield return Frames(2);
                int left = Hud30Reads.CooldownSteps(lo, lo.Weapon);
                if (left * sim.step >= UnityTuning.Hud30MinCoolSeconds + 0.3) cooled = lo.Weapon;
            }
            if (cooled != null)
            {
                int ci0 = lo.owned.IndexOf(cooled);
                int p0 = Ui.Rail34Pulses;
                yield return Swap34(g, (ci0 + 1) % lo.owned.Count);
                yield return Hud34Shot(dir, $"hud34-{tag}-cooling", lines, bad);
                lines.Add($"cooling {tag}: {cooled} fired then holstered, {Hud30Reads.CooldownSteps(lo, cooled) * sim.step:0.0} s left, rail '{Ui.Rail34Describe()}'");
                bool pulsed = false;
                yield return Sim33(g, 8.0, null, () => Ui.Rail34Pulses > p0);
                if (Ui.Rail34Pulses > p0) { pulsed = true; yield return Frames(3); } // into the pulse (280 ms)
                yield return Hud34Shot(dir, $"hud34-{tag}-ready", lines, bad);
                lines.Add($"ready {tag}: {cooled}'s READY pulse {(pulsed ? "seen" : "NOT seen within 8 s")} (pulses {Ui.Rail34Pulses - p0})");
                if (pulsed) seen[2]++;
            }
            else lines.Add($"cooling {tag}: no owned gun's cooldown shows after one shot (>= {UnityTuning.Hud30MinCoolSeconds} s): not shot");

            // the bend, forced through the sim (needs design.n34.bend34)
            float dimRest = Ui.Rail34DimNow;
            float meterBefore = Ui.Bend34MeterNow;
            bool meterShown = Ui.Bend34MeterShown;
            int e0 = Bend34View.Entries, snaps0 = Viewmodel.BendSnaps34;
            float fovMin = 0, depthMax = 0, gradeMax = 0, skyMax = 0, dimBend = 0;
            sim.Bend34LatchForCheck(); // B's check hook: a latch on the next step, exactly as a RopeAttach / arclinePull would (no haptic yank)
            yield return Sim33(g, 0.6, () =>
            {
                fovMin = Math.Min(fovMin, Bend34View.FovNow);
                depthMax = Math.Max(depthMax, (float)sim.Bend34Depth01);
                gradeMax = Math.Max(gradeMax, Bend34Grade.GradeNow);
                skyMax = Math.Max(skyMax, Bend34Grade.SkyNow);
                dimBend = Math.Max(dimBend, Ui.Rail34DimNow);
            }, () => sim.Bend34Depth01 >= 0.99 && Bend34View.FovNow == 0 && Bend34View.Entries > e0);
            yield return Hud34Shot(dir, $"hud34-{tag}-bend", lines, bad);
            float grade1 = Bend34Grade.GradeNow;
            lines.Add($"bend meter {tag}: the slash beside the grapple ticks shown {meterShown} at {meterBefore.ToString("0.00", ci)} before the latch, {Ui.Bend34MeterNow.ToString("0.00", ci)} in the bend (tier {sim.Bend34TierNow})");
            // the combo's first half: a kill with the gun in hand (two guns, two kills inside one bend: B's bend34Combo)
            int combo0 = Ui.Combo34Posts;
            g.events.Emit("kill", new KillEvent { owner = "player", weapon = lo.Weapon, kind = "husk" });
            yield return Frames(2);
            // a swap inside the bend: the hands' snap
            string w0 = lo.Weapon;
            int to2 = (lo.owned.IndexOf(lo.Weapon) + 1) % lo.owned.Count;
            g.forcedCombat = new CombatInput { weapon = to2 };
            yield return Frames(1);
            g.forcedCombat = IdleCombat;
            int framesToSwap = 1;
            while (lo.Weapon == w0 && framesToSwap < 60) { yield return null; framesToSwap++; }
            yield return Frames(3);
            yield return Hud34Shot(dir, $"hud34-{tag}-bend-swap", lines, bad);
            // a kill inside the bend
            g.events.Emit("kill", new KillEvent { owner = "player", weapon = lo.Weapon, kind = "husk" });
            yield return Frames(4);
            float grade2 = Bend34Grade.GradeNow;
            yield return Hud34Shot(dir, $"hud34-{tag}-bend-kill", lines, bad);
            // the combo's name in the message slot (cyan, 24 px), if B's bend34Combo fired
            yield return Sim33(g, 0.3, null, () => Ui.Combo34Posts > combo0 && Ui.Hud33SlotNow == Ui.Combo34Last);
            yield return Frames(4); // past the slot's flicker-on
            yield return Hud34Shot(dir, $"hud34-{tag}-combo", lines, bad);
            lines.Add($"combo {tag}: bend34Combo posts {Ui.Combo34Posts - combo0} (last '{Ui.Combo34Last}'), the slot shows '{Ui.Hud33SlotNow}'{(Ui.Combo34Posts > combo0 ? "" : " (no bend34Combo event: B's event not in this build, or the rule wants more)")}");
            lines.Add($"bend {tag}: design.n34.bend34 {sim.n34.bend34}, bendLook {N34Fixes.ViewRowOn(g.cfg, "bendLook")}, entries {Bend34View.Entries - e0}, depth max {depthMax.ToString("0.00", ci)}, FOV thump min {fovMin.ToString("0.0", ci)} deg, grade {grade1.ToString("0.00", ci)} -> {grade2.ToString("0.00", ci)} after a kill (kills counted {Bend34Grade.KillsNow}), sky dim max {skyMax.ToString("0.00", ci)}, rail outline alpha {dimRest.ToString("0.00", ci)} at rest -> {dimBend.ToString("0.00", ci)} in the bend; swap inside: {w0} -> {lo.Weapon} in {framesToSwap} frame(s), hands' snaps {Viewmodel.BendSnaps34 - snaps0}");
            if (depthMax >= 0.9f && grade1 > 0 && skyMax > 0 && dimBend > dimRest) seen[3]++;
            yield return Sim33(g, 8.0, null, () => sim.Bend34Depth01 <= 0 && Bend34Grade.GradeNow <= 0 && Bend34Grade.SkyNow <= 0);
            yield return Sim33(g, 0.2);
            yield return Hud34Shot(dir, $"hud34-{tag}-bend-exit", lines, bad);
            lines.Add($"bend exit {tag}: depth {sim.Bend34Depth01.ToString("0.00", ci)}, grade {Bend34Grade.GradeNow.ToString("0.00", ci)}, sky {Bend34Grade.SkyNow.ToString("0.00", ci)}");
            if (grade2 > grade1 && Bend34Grade.GradeNow <= 0 && Bend34Grade.SkyNow <= 0) seen[4]++;

            // nomark: aimed at a node, then at a wall (the target is read, never drawn with nomark34)
            bool nomark = N34Fixes.ViewRowOn(g.cfg, "nomark34");
            if (!sim.arcline21.Owned)
            {
                sim.mover.ropeEnabled = true; // the ARCLINE granted for the check (said in the line), taken back after
                offs.Add(() => { if (g.sim != null) g.sim.mover.ropeEnabled = false; });
                lines.Add($"nomark {tag}: the ARCLINE was not owned: granted for the check");
            }
            int nodeIdx = -1;
            Vec3 stand = g.Feet;
            for (int i = 0; i < sim.map.grappleNodes.Count && nodeIdx < 0; i++)
            {
                Vec3 n = sim.map.grappleNodes[i];
                if ((n - sim.Eye).Length < 20 && Sees31(g, sim.Eye, n)) { nodeIdx = i; break; }
                foreach (var o in new[] { new Vec3(0, 0, 4.5), new Vec3(0, 0, -4.5), new Vec3(4.5, 0, 0), new Vec3(-4.5, 0, 0) })
                {
                    var at = new Vec3(n.x + o.x, g.Feet.y, n.z + o.z);
                    var eye = new Vec3(at.x, at.y + g.cfg.movement.body.eyeHeight, at.z);
                    if (Sees31(g, eye, n) && Math.Abs(n.y - eye.y) < 8) { nodeIdx = i; stand = at; break; }
                }
            }
            int[] node = { 0, 0, 0, 0 }; // frames, would-show frames, drawn frames, rings max
            if (nodeIdx >= 0)
            {
                if ((stand - g.Feet).Length > 0.1) { g.Respawn(stand, 0); yield return Sim33(g, 0.3); }
                Vec3 nv = sim.map.grappleNodes[nodeIdx];
                yield return Aim34(g, nv, node);
                yield return Hud34Shot(dir, $"hud34-{tag}-nomark-node", lines, bad);
                lines.Add($"nomark node {tag}: node {nodeIdx} at ({nv.x.ToString("0.0", ci)}, {nv.y.ToString("0.0", ci)}, {nv.z.ToString("0.0", ci)}): the mark would show in {node[1]} of {node[0]} frames, drawn in {node[2]}, rings drawn max {node[3]} (nomark34 {nomark})");
            }
            else lines.Add($"nomark node {tag}: no grapple node in sight on '{g.MapId}' ({sim.map.grappleNodes.Count} nodes): not shot (boot -skygrindMap n30-span or an n33 level)");
            // the nearest wall in 16 directions, 2 .. 18 m
            Vec3 eye0 = sim.Eye;
            double best = double.MaxValue;
            Vec3 wall = default;
            for (int i = 0; i < 16; i++)
            {
                double a = i * Math.PI / 8;
                var d = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                if (g.world.RayIntersect(eye0, d, out RayHit hit) && hit.distance > 2 && hit.distance < 18 && hit.distance < best) { best = hit.distance; wall = eye0 + d * (hit.distance - 0.05); }
            }
            int[] wallF = { 0, 0, 0, 0 };
            if (best < double.MaxValue)
            {
                yield return Aim34(g, wall, wallF);
                yield return Hud34Shot(dir, $"hud34-{tag}-nomark-wall", lines, bad);
                lines.Add($"nomark wall {tag}: a wall {best.ToString("0.0", ci)} m off: the mark would show in {wallF[1]} of {wallF[0]} frames ({Grapple31View.MarkKind}), drawn in {wallF[2]} (nomark34 {nomark})");
            }
            else lines.Add($"nomark wall {tag}: no wall within 18 m: not shot");
            g.forcedMove = new InputFrame();
            if (nomark && node[0] > 0 && wallF[0] > 0 && node[2] == 0 && wallF[2] == 0 && node[1] > 0 && node[3] > 0) seen[5]++;

            // a boss fight: the rail beside the boss bar
            if (sim.stag31 != null && sim.stag31.Alive) sim.stag31.Clear();
            sim.flow = Flow.Playing;
            if (sim.BossRoomBegin("glacier", out Vec3 sp, out double syaw) && sim.stag31 != null && sim.stag31.Alive)
            {
                g.Respawn(sp, syaw);
                yield return Sim33(g, 1.5);
                yield return Hud34Shot(dir, $"hud34-{tag}-boss", lines, bad);
                lines.Add($"boss {tag}: bar {Ui.Hud33BossBarShown} '{Ui.Hud33BossNameNow}', rail '{Ui.Rail34Describe()}'");
                if (Ui.Hud33BossBarShown && Ui.Rail34Live) seen[6]++;
                if (sim.stag31 != null && sim.stag31.Alive) sim.stag31.Clear();
            }
            else lines.Add($"hud34-{tag}-boss: not shot (BossRoomBegin(glacier) did not start THE STAG)");
        }

        /// <summary>Aims at `at` for 0.5 s of sim time; counts [frames, MarkShown frames, MarkDrawn frames, the most node rings drawn].</summary>
        static IEnumerator Aim34(SurvivalGame g, Vec3 at, int[] c)
        {
            double t0 = g.sim.SimTime;
            float wall = Time.realtimeSinceStartup;
            while (g.sim.SimTime - t0 < 0.5 && Time.realtimeSinceStartup - wall < 10f)
            {
                God(g);
                (double yaw, double pitch) = LookAt31(g, at);
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                yield return null;
                c[0]++;
                if (Grapple31View.MarkShown) c[1]++;
                if (Grapple31View.MarkDrawn) c[2]++;
                c[3] = Math.Max(c[3], GrappleNodes30.RingsShown);
            }
        }

        static IEnumerator Hud34Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            bool rail = N34Fixes.ViewRowOn(g.cfg, "rail34"), look = N34Fixes.ViewRowOn(g.cfg, "bendLook"), nomark = N34Fixes.ViewRowOn(g.cfg, "nomark34"), hud33 = N33Fixes.ViewRowOn(g.cfg, "hud33");
            int w0 = Screen.width, h0 = Screen.height;
            FullScreenMode mode0 = Screen.fullScreenMode;
            offs.Add(() => Quiet(g));
            offs.Add(() => { if (Screen.width != w0 || Screen.height != h0) Screen.SetResolution(w0, h0, mode0); });
            string setup0 = g.setup.mode;
            offs.Add(() => g.setup.mode = setup0);
            g.setup.mode = RunSetup.Survival;
            Fresh(g);
            g.husks.frozen = true;
            string pc = Preconditions(g, g.Feet) + $" | rows rail34 {rail} bendLook {look} nomark34 {nomark} hud33 {hud33} bend34 {g.sim.n34.bend34} split {g.sim.n34.bendSplit34}, screen {w0}x{h0} {mode0}, map {g.MapId}";
            var lines = new List<string>();
            int[] bad = { 0 }, seen = { 0, 0, 0, 0, 0, 0, 0 }, seen720 = { 0, 0, 0, 0, 0, 0, 0 };
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            for (int i = 0; i < 60 && Screen.height != 1080; i++) yield return null;
            string r1 = $"{Screen.width}x{Screen.height}";
            Start(g, "hud34", "ui", dir);
            yield return Frames(pre);
            yield return Hud34Pass(g, dir, "1080", true, lines, bad, seen, offs);
            yield return Stop();
            // 720p stills: the rail at rest and one swap (the layout rules at the other size)
            g.setup.mode = RunSetup.Survival;
            Fresh(g);
            g.husks.frozen = true;
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            for (int i = 0; i < 60 && Screen.height != 720; i++) yield return null;
            string r2 = $"{Screen.width}x{Screen.height}";
            yield return Frames(10);
            yield return Hud34Pass(g, dir, "720", false, lines, bad, seen720, offs);
            Screen.SetResolution(w0, h0, mode0);
            yield return Frames(10);
            g.setup.mode = setup0;
            Quiet(g);
            bool ok = rail && hud33 && bad[0] == 0 && seen[0] == 1 && seen[1] == 4 && seen[3] == 1 && seen[4] == 1 && seen[5] == 1 && seen720[0] == 1 && seen720[1] == 1 && r1 == "1920x1080" && r2 == "1280x720";
            res[0] = $"{(ok ? "PASS" : "RED")}:{(rail ? "rail34 ON" : "rail34 OFF (-skygrindOff34 or the row off: hud33's pips, the RED half)")}; hud33 rule problems {bad[0]} (passes {r1} and {r2}); " +
                     $"5 slots + hand {seen[0]}/1, swaps moved the highlight {seen[1]}/4, holstered READY pulse {seen[2]}/1, bend look (depth / grade / sky / rail brighter) {seen[3]}/1, grade deeper per kill and restored {seen[4]}/1, " +
                     $"nomark 0 drawn at node and wall with the ring {seen[5]}/1, boss bar beside the rail {seen[6]}/1, 720 slots {seen720[0]}/1 swap {seen720[1]}/1 | " +
                     string.Join(" | ", lines) + " | shots shot-N30-hud34-1080-<rail5|swap-1..4|cooling|ready|bend|bend-swap|bend-kill|combo|bend-exit|nomark-node|nomark-wall|boss>.png, shot-N30-hud34-720-<rail5|swap-1>.png | " + pc;
        }
    }
}
