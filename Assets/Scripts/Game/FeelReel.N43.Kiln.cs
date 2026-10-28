// NIGHT43 builder K1 (kiln43; NIGHT43.md card K1, hand-offs): the Player scenario `kiln43`: level 3 as n43-kiln, framed. Boot one Player on
// level 3 (reel.sh + -skygrindMap n43-kiln + -skygrindReelOnly kiln43; L-062). The reel forces kiln43 on for its run (and boost43, M1's combine,
// when its code is in the build: a bit with no code changes nothing) through sim.n43Override, handed back after.
// The run: a CAMPAIGN run, DOORFREE, the player kept alive.
//   1. 3.1 THE STEPS from the door: K43-steps-door (the spawn, the look pitched up a little: the three steps and their cyan lips), the room's
//      frames36: K43-steps-mid (from step 1 toward step 3), K43-steps-top (from the top step down to the door).
//   2. A BOOST on step 1: the player at the step's take-off (tops43.take[0]), the jump pressed and held 0.6 s, the look straight down, the
//      THUNDERCLAP tapped on the next frame: K43-boost-0 (the press), K43-boost-1 (+0.35 s: rising past the lip), K43-boost-2 (landed on step 1,
//      or +2.5 s). The line says landed Y / N, the feet's top and the chain line standing.
//   3. 3.2c THE ASH BED: the player at its spawn, one BURROWER (kind "burrower": A1's burrow43; a plain husk when A1's code is absent) placed
//      on the open ash 10 m ahead: K43-ash-0 (the entry: the five cyan plates), K43-ash-1 (+1.2 s), K43-ash-2 (+2.6 s: its tell or emergence),
//      K43-ash-3 (on the middle plate, looking at it). The line counts burrowTell43 / burrowEmerge43.
//   4. 3.5 THE RING: the room's frames36: K43-ring-0 (from the entry), K43-ring-1 (the far side).
// Line: GREEN when every frame was saved and the boost on step 1 landed. Not run by its author (builders never run a Player).
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
        static partial void N43Kiln(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Kiln43Scenario(g, dir, res, offs, pre);

        /// <summary>A room row of the booted map's campaign block (null when absent).</summary>
        static Dictionary<string, object> K43Row(SurvivalGame g, string id)
        {
            if (g.map == null || g.map.json == null || !g.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return null;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rows)) return null;
            foreach (object o in rows)
                if (o is Dictionary<string, object> r && r.TryGetValue("id", out object v) && v as string == id) return r;
            return null;
        }

        /// <summary>Stand at a frames36 view ([x, y, z, yawDeg, pitchRad]) and save the frame.</summary>
        static IEnumerator K43View(SurvivalGame g, string dir, Dictionary<string, object> row, string which, string name, int[] frames)
        {
            if (row == null || !row.TryGetValue("frames36", out object fo) || !(fo is Dictionary<string, object> f36) || !f36.TryGetValue(which, out object vo)) yield break;
            List<object> v = PortData.Arr(vo);
            double yaw = PortData.Num(v[3]) * Math.PI / 180, pitch = PortData.Num(v[4]);
            g.Respawn(new Vec3(PortData.Num(v[0]), PortData.Num(v[1]) + 0.05, PortData.Num(v[2])), PortData.Num(v[3]));
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            yield return Frames(20);
            God(g);
            yield return Shot30(dir, name);
            frames[0]++;
        }

        static IEnumerator Kiln43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            N43Blocks? overWas = sim.n43Override;
            N43Blocks want = N43Blocks.ForceOff ? N43Blocks.Off : N43Blocks.From(g.cfg);
            want.kiln43 = true;
            want.boost43 = true;
            sim.n43Override = want;
            offs.Add(() => sim.n43Override = overWas);
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED kiln43: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            if (sim.campaignLevel == null || N45Levels.BaseId(sim.cfg, sim.campaignLevel.id) != "n43-kiln") { // NIGHT45: n45-kiln too
                res[0] = $"RED kiln43: the run is on {sim.campaignLevel?.id ?? "?"}, not n43-kiln (boot -skygrindMap n43-kiln, L-062)"; yield break; }
            g.forcedCombat = IdleCombat;
            God(g);
            int tells = 0, emerges = 0, counters = 0;
            offs.Add(g.events.On<Burrow43Event>(N43Events.BurrowTell, _ => tells++));
            offs.Add(g.events.On<Burrow43Event>(N43Events.BurrowEmerge, _ => emerges++));
            offs.Add(g.events.On<Burrow43Event>(N43Events.BurrowCounter, _ => counters++));
            Start(g, "kiln43", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var frames = new int[1];
            var notes = new List<string>();

            // ---- 1. THE STEPS ----
            Dictionary<string, object> kb1 = K43Row(g, "kb1");
            RoomDef r1 = sim.campaignLevel.rooms.Find(q => q.id == "kb1");
            if (r1 != null)
            {
                g.husks.KillAll();
                g.Respawn(r1.spawn + new Vec3(0, 0.05, 0), r1.spawnYawDeg);
                double yaw = r1.spawnYawDeg * Math.PI / 180;
                g.input.SetLook(yaw, 0.12);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.12 };
                yield return Frames(30);
                yield return Shot30(dir, "K43-steps-door");
                frames[0]++;
            }
            yield return K43View(g, dir, kb1, "mid", "K43-steps-mid", frames);
            yield return K43View(g, dir, kb1, "exit", "K43-steps-top", frames);

            // ---- 2. A BOOST ON STEP 1 ----
            bool landed = false;
            double apex = 0;
            if (kb1 != null && kb1.TryGetValue("tops43", out object to) && to is Dictionary<string, object> tops)
            {
                Vec3 take = PortData.V3(PortData.Arr(tops["take"])[0]);
                double top1 = PortData.Num(tops["s1"]);
                g.Respawn(take + new Vec3(0, 0.05, 0), 0);
                g.input.SetLook(0, 0);
                g.forcedMove = new InputFrame { yaw = 0, pitch = 0 };
                int slot = sim.loadout.owned.IndexOf("shotgun");
                double tw = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - tw < 1.5)
                {
                    God(g);
                    g.forcedCombat = new CombatInput { weapon = sim.loadout.Weapon != "shotgun" ? slot : -1 };
                    yield return null;
                }
                g.forcedCombat = IdleCombat;
                double down = -1.5;
                g.input.SetLook(0, down);
                g.forcedMove = new InputFrame { yaw = 0, pitch = down, moveZ = 1, jump = new Button { held = true, pressed = true } };
                yield return null;
                yield return Shot30(dir, "K43-boost-0");
                frames[0]++;
                g.forcedMove = new InputFrame { yaw = 0, pitch = down, moveZ = 1, jump = new Button { held = true } };
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } };
                yield return null;
                g.forcedCombat = IdleCombat;
                double y0 = take.y, t0 = Time.realtimeSinceStartup;
                bool shot1 = false;
                while (Time.realtimeSinceStartup - t0 < 2.5)
                {
                    God(g);
                    double el = Time.realtimeSinceStartup - t0;
                    g.forcedMove = new InputFrame { yaw = 0, pitch = down, moveZ = 1, jump = new Button { held = el < 0.6 } };
                    apex = Math.Max(apex, sim.mover.position.y - y0);
                    if (!shot1 && el >= 0.35)
                    {
                        yield return Shot30(dir, "K43-boost-1");
                        frames[0]++;
                        shot1 = true;
                    }
                    if (el > 0.3 && sim.mover.grounded) { landed = sim.mover.position.y >= top1 - 0.3; break; }
                    yield return null;
                }
                g.forcedMove = new InputFrame { yaw = 0, pitch = 0 };
                yield return Frames(4);
                yield return Shot30(dir, "K43-boost-2");
                frames[0]++;
                notes.Add($"boost on step 1 ({top1.ToString("0.0", ci)} m): {(landed ? "landed" : "short")}, the feet's top {apex.ToString("0.00", ci)} m; the chain {string.Join(", ", sim.kiln43Shown)}");
            }
            else notes.Add("boost: kb1 has no tops43 (an old n43-kiln)");

            // ---- 3. THE ASH BED ----
            Dictionary<string, object> kb2c = K43Row(g, "kb2c");
            RoomDef rc = sim.campaignLevel.rooms.Find(q => q.id == "kb2c");
            if (rc != null)
            {
                g.husks.KillAll();
                g.Respawn(rc.spawn + new Vec3(0, 0.05, 0), rc.spawnYawDeg);
                double yaw = rc.spawnYawDeg * Math.PI / 180;
                g.input.SetLook(yaw, -0.08);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = -0.08 };
                yield return Frames(20);
                Vec3 at = rc.spawn + new Vec3(-Math.Sin(yaw) * 10, 0, -Math.Cos(yaw) * 10);
                Husk b = g.husks.Spawn("burrower", at, rc.spawn);
                yield return Frames(6);
                yield return Shot30(dir, "K43-ash-0");
                frames[0]++;
                double t0 = Time.realtimeSinceStartup;
                bool s1 = false, cShot = false;
                // NIGHT44 R2 (T-68; SPIKE.md 4): the reel never fired, so "counters 0". The rule counts a hit while it is in Emerge with its window
                // open (Burrower43.Damage: windowSteps > 0, windowSeconds 0.5): from its BurrowEmerge, every frame of the open window aims at its
                // body (AimPoint) and presses fire (a real shot through the game's combat, not a Damage call), until the counter lands
                var bw = b as Burrower43;
                int counters0 = counters, fired = 0;
                while (Time.realtimeSinceStartup - t0 < 8)
                {
                    God(g);
                    double el = Time.realtimeSinceStartup - t0;
                    if (!s1 && el >= 1.2) { yield return Shot30(dir, "K43-ash-1"); frames[0]++; s1 = true; continue; }
                    if (bw != null && bw.alive && bw.state == Burrower43.Emerge && bw.windowSteps > 0 && counters == counters0)
                    {
                        var (ay, ap) = LookAt31(g, bw.AimPoint);
                        g.input.SetLook(ay, ap);
                        g.forcedMove = new InputFrame { yaw = ay, pitch = ap };
                        g.forcedCombat = Fire;
                        fired++;
                    }
                    else g.forcedCombat = IdleCombat;
                    if (!cShot && counters > counters0) { yield return Frames(3); yield return Shot30(dir, "K43-ash-counter"); frames[0]++; cShot = true; continue; }
                    bool windowGone = bw == null || !bw.alive || (emerges > 0 && !(bw.state == Burrower43.Emerge && bw.windowSteps > 0));
                    if (el >= 2.6 && s1 && (cShot || windowGone)) break;
                    yield return null;
                }
                g.forcedCombat = IdleCombat;
                notes.Add($"ash counter: {(counters > counters0 ? "GREEN" : "RED")} (fire frames in the window {fired}, counters {counters - counters0}, the burrower {(bw == null ? "not a Burrower43" : bw.state + (bw.alive ? "" : " dead"))})");
                yield return Shot30(dir, "K43-ash-2");
                frames[0]++;
                // the middle plate (its box in the campaign block with burrowSafe43), looking back at the room's floor
                Vec3? plate = null;
                if (g.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("boxes", out object bo) && bo is List<object> boxes)
                    foreach (object o in boxes)
                        if (o is Dictionary<string, object> bx && bx.TryGetValue("burrowSafe43", out object bs) && bs is bool bb && bb && bx.TryGetValue("zone", out object z) && z as string == "kb2c")
                        {
                            List<object> mn = PortData.Arr(bx["min"]), mx = PortData.Arr(bx["max"]);
                            var c = new Vec3((PortData.Num(mn[0]) + PortData.Num(mx[0])) / 2, PortData.Num(mx[1]), (PortData.Num(mn[2]) + PortData.Num(mx[2])) / 2);
                            if (plate == null || Math.Abs(c.x - rc.spawn.x) < Math.Abs(plate.Value.x - rc.spawn.x)) plate = c;
                        }
                if (plate.HasValue)
                {
                    g.Respawn(plate.Value + new Vec3(0, 0.05, 0), rc.spawnYawDeg + 180);
                    double y2 = yaw + Math.PI;
                    g.input.SetLook(y2, -0.15);
                    g.forcedMove = new InputFrame { yaw = y2, pitch = -0.15 };
                    yield return Frames(30);
                    God(g);
                    yield return Shot30(dir, "K43-ash-3");
                    frames[0]++;
                }
                notes.Add($"ash bed: the burrower {(b != null ? "placed (" + b.kind + ")" : "NOT placed")}, tells {tells}, emerges {emerges}, counters {counters}");
            }
            else notes.Add("ash bed: no kb2c room");

            // ---- 4. THE RING ----
            Dictionary<string, object> k5 = K43Row(g, "k5");
            g.husks.KillAll();
            yield return K43View(g, dir, k5, "mid", "K43-ring-0", frames);
            yield return K43View(g, dir, k5, "exit", "K43-ring-1", frames);

            g.forcedMove = null;
            yield return Stop();
            bool green = landed && frames[0] >= 13;
            res[0] = $"{(green ? "GREEN" : "RED")} kiln43 | map {g.MapId}, kiln43 {sim.n43.kiln43}, boost43 {sim.n43.boost43} | frames {frames[0]} | {string.Join(" | ", notes)}";
        }
    }
}
