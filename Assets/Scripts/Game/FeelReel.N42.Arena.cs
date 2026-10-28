// NIGHT42 builder K1 (arena42; G12; NIGHT42.md hand-offs): the Player scenario `arena42`: each arena element in use, framed. Boot one Player on
// level 1 (reel.sh + -skygrindMap n42-foundry + -skygrindReelOnly arena42; L-062). The reel forces arena42 on for its run (SurvivalSim.Force42Arena,
// handed back after). When the level has no `grinders42` / `cover42` yet (L1's n42-foundry before it places them), the reel PLANTS one of each on
// room 0's floor before the run starts (a 4 x 4 m grinder 2.2-6.2 m ahead of the spawn, a 1.0 x 2.4 x 1.6 m cover box 5 m to the right) and takes
// them out of the map JSON after; the line says "planted" or "level".
// The run: a CAMPAIGN run, DOORFREE, the husks cleared, the player kept alive.
//   1. THE GRINDER: a husk (1000 hp: only the grinder kills it) 0.6 m short of the grinder's near edge, the player facing it; frames
//      A42-grinder-0 (before), the PUNCH (F), A42-grinder-1 (+0.15 s: the body over the blades), A42-grinder-2 (the frame after arena42Ground: the
//      flare, the GROUND bonus U1 draws).
//   2. THE COVER: two husks held 1 m behind the box, the player facing it, the KESTREL fired at its face; frames A42-cover-0 (before),
//      A42-cover-1 (after 3 hits: the cyan band shorter), A42-cover-2 (the frame after arena42CoverBreak: shards), A42-cover-3 (+0.25 s).
//   3. THE PAD (when the level has a pad): the player put on pad 0; A42-pad-0 (+0.45 s after jumpPad: mid-arc over the room).
// Line: GREEN when the punched husk was ground (knocked), the cover broke and its shatter hit a body, and every grinder / cover frame was saved.
// Not run by its author (builders never run a Player).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N42Arena(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Arena42Scenario(g, dir, res, offs, pre);

        static List<object> A42List(params double[] v)
        {
            var l = new List<object>();
            foreach (double d in v) l.Add(d);
            return l;
        }

        static IEnumerator Arena42Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim.Force42Arena = true;
            offs.Add(() => SurvivalSim.Force42Arena = null);
            // plant one grinder and one cover box on room 0's floor when the level has none
            Dictionary<string, object> json = g.map != null ? g.map.json : null;
            bool planted = false;
            LevelDef lv0 = json != null && json.ContainsKey("campaign") ? LevelDef.From(json) : null;
            if (json != null && lv0 != null && lv0.rooms.Count > 0 && !json.ContainsKey("grinders42") && !json.ContainsKey("cover42"))
            {
                RoomDef r0 = lv0.rooms[0];
                double yaw0 = r0.spawnYawDeg * Math.PI / 180, fx = -Math.Sin(yaw0), fz = -Math.Cos(yaw0), rx = -fz, rz = fx; // forward; right
                Vec3 s0 = r0.spawn;
                double gx = s0.x + fx * 4.2, gz = s0.z + fz * 4.2;
                json["grinders42"] = new List<object> { new Dictionary<string, object> { { "rect", A42List(gx - 2, gz - 2, gx + 2, gz + 2) }, { "y", s0.y }, { "room", r0.id } } };
                double cx = s0.x + rx * 5, cz = s0.z + rz * 5;
                bool alongX = Math.Abs(rx) >= Math.Abs(rz);
                double hx = alongX ? 0.5 : 1.2, hz = alongX ? 1.2 : 0.5;
                json["cover42"] = new List<object> { new Dictionary<string, object> { { "min", A42List(cx - hx, s0.y, cz - hz) }, { "max", A42List(cx + hx, s0.y + 1.6, cz + hz) }, { "hits", 6.0 }, { "room", r0.id } } };
                planted = true;
                offs.Add(() => { json.Remove("grinders42"); json.Remove("cover42"); });
            }
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            SurvivalSim sim = g.sim;
            Arena42 a = sim != null ? sim.arena42 : null;
            if (!ok[0] || a == null) { res[0] = $"RED arena42: {(ok[0] ? "no arena42 on this run (the row forced on; the level's keys or the plant missing)" : "no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")")}"; yield break; }
            g.forcedCombat = IdleCombat;
            God(g);
            int ground = 0, groundKnocked = 0, coverHits = 0, breaks = 0, burstBodies = 0, pads = 0, frames = 0;
            offs.Add(g.events.On<Arena42GroundEvent>(Arena42.GroundEvent, e => { ground++; if (e.knocked) groundKnocked++; }));
            offs.Add(g.events.On<Arena42CoverEvent>(Arena42.CoverHitEvent, e => coverHits++));
            offs.Add(g.events.On<Arena42CoverEvent>(Arena42.CoverBreakEvent, e => { breaks++; burstBodies += e.bodies; }));
            offs.Add(g.events.On("jumpPad", _ => pads++));
            Start(g, "arena42", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var notes = new List<string>();

            // ---- 1. THE GRINDER ----
            if (a.grinders.Count > 0)
            {
                Arena42Grinder gr = a.grinders[0];
                double gcx = (gr.x0 + gr.x1) * 0.5, gcz = (gr.z0 + gr.z1) * 0.5;
                // stand 3.8 m from the grinder's centre on its longer free side... the plant's spawn when planted; else south of the rect, facing it
                Vec3 at = planted ? lv0.rooms[0].spawn : new Vec3(gcx, gr.y + 4.0, gr.z1 + 3.0); // a pit's rim is at floor + its depth: the level's own row says where (not checked)
                double dx = gcx - at.x, dz = gcz - at.z, dl = Math.Sqrt(dx * dx + dz * dz);
                double ux = dl > 1e-6 ? dx / dl : 0, uz = dl > 1e-6 ? dz / dl : -1;
                double yaw = Math.Atan2(-ux, -uz);
                double edge = Math.Min(Math.Abs((Math.Abs(ux) > 1e-9 ? ((ux > 0 ? gr.x0 : gr.x1) - at.x) / ux : 1e9)), Math.Abs((Math.Abs(uz) > 1e-9 ? ((uz > 0 ? gr.z0 : gr.z1) - at.z) / uz : 1e9)));
                g.husks.KillAll();
                g.Respawn(at + new Vec3(0, 0.05, 0), yaw * 180 / Math.PI);
                g.input.SetLook(yaw, -0.2);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = -0.2 };
                yield return Frames(20);
                double put = Math.Max(1.2, edge - 0.6);
                Husk h = g.husks.Spawn("husk", new Vec3(at.x + ux * put, at.y, at.z + uz * put), at);
                if (h != null)
                {
                    h.hp = h.maxHp = 1000;
                    yield return Frames(6);
                    yield return Shot30(dir, "A42-grinder-0");
                    frames++;
                    g.forcedCombat = new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } };
                    yield return null;
                    g.forcedCombat = IdleCombat;
                    double t0 = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - t0 < 0.15) { God(g); yield return null; }
                    yield return Shot30(dir, "A42-grinder-1");
                    frames++;
                    while (Time.realtimeSinceStartup - t0 < 2.0 && ground == 0) { God(g); yield return null; }
                    yield return Shot30(dir, "A42-grinder-2");
                    frames++;
                    notes.Add($"grinder: punched from {put:0.00} m (edge {edge:0.00} m), ground {ground} (knocked {groundKnocked})");
                }
                else notes.Add("grinder: the husk could not be placed");
            }
            else notes.Add("grinder: none on this level");

            // ---- 2. THE COVER ----
            if (a.cover.Count > 0)
            {
                Arena42Cover cv = a.cover[0];
                Vec3 c = cv.Centre;
                Vec3 at = planted ? lv0.rooms[0].spawn : new Vec3(c.x, cv.min.y, cv.max.z + 5.0);
                double dx = c.x - at.x, dz = c.z - at.z, dl = Math.Sqrt(dx * dx + dz * dz);
                double ux = dl > 1e-6 ? dx / dl : 0, uz = dl > 1e-6 ? dz / dl : -1;
                double yaw = Math.Atan2(-ux, -uz);
                g.husks.KillAll();
                g.Respawn(at + new Vec3(0, 0.05, 0), yaw * 180 / Math.PI);
                yield return Frames(10);
                Vec3 eye = sim.Eye;
                var aim = new Vec3(c.x - ux * 0.6, cv.min.y + 0.9, c.z - uz * 0.6);
                Vec3 d = aim - eye;
                double pitch = Math.Asin(d.y / d.Length);
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                Husk h1 = g.husks.Spawn("husk", new Vec3(c.x + ux * 1.6 - uz * 0.7, cv.min.y, c.z + uz * 1.6 + ux * 0.7), at);
                Husk h2 = g.husks.Spawn("husk", new Vec3(c.x + ux * 1.6 + uz * 0.7, cv.min.y, c.z + uz * 1.6 - ux * 0.7), at);
                void Hold()
                {
                    foreach (Husk hh in new[] { h1, h2 })
                        if (hh != null && hh.alive && hh.knockSteps == 0) { hh.velocity.x = 0; hh.velocity.z = 0; hh.windupSteps = 0; }
                }
                yield return Frames(6);
                yield return Shot30(dir, "A42-cover-0");
                frames++;
                double t0 = Time.realtimeSinceStartup;
                bool shot1 = false;
                int f = 0;
                while (Time.realtimeSinceStartup - t0 < 10 && breaks == 0)
                {
                    God(g);
                    Hold();
                    bool press = (f++ % 2) == 0;
                    g.forcedCombat = new CombatInput { weapon = -1, fire = new Button { held = press, pressed = press } };
                    yield return null;
                    if (!shot1 && coverHits >= 3)
                    {
                        g.forcedCombat = IdleCombat;
                        yield return Shot30(dir, "A42-cover-1");
                        frames++;
                        shot1 = true;
                    }
                }
                g.forcedCombat = IdleCombat;
                yield return Shot30(dir, "A42-cover-2");
                frames++;
                double t1 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t1 < 0.25) { God(g); yield return null; }
                yield return Shot30(dir, "A42-cover-3");
                frames++;
                notes.Add($"cover: hits {coverHits}, broke {breaks}, shatter bodies {burstBodies}, husks alive {(h1 != null && h1.alive ? 1 : 0) + (h2 != null && h2.alive ? 1 : 0)}");
            }
            else notes.Add("cover: none on this level");

            // ---- 3. THE PAD ----
            if (g.map != null && g.map.pads.Count > 0)
            {
                PadSpec pad = g.map.pads[0];
                g.husks.KillAll();
                double yaw = pad.aimed ? Math.Atan2(-(pad.target.x - pad.pos.x), -(pad.target.z - pad.pos.z)) : 0;
                g.input.SetLook(yaw, 0);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0 };
                g.Respawn(pad.pos + new Vec3(0, 0.05, 0), yaw * 180 / Math.PI);
                double t0 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t0 < 3 && pads == 0) { God(g); yield return null; }
                if (pads > 0)
                {
                    double t1 = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - t1 < 0.45) { God(g); yield return null; }
                    yield return Shot30(dir, "A42-pad-0");
                    frames++;
                }
                notes.Add($"pad: pad 0 launched {(pads > 0 ? "Y" : "N")}");
            }
            else notes.Add("pad: none on this level");
            g.forcedMove = null;
            yield return Stop();
            bool green = groundKnocked >= 1 && breaks >= 1 && burstBodies >= 1 && frames >= 7;
            res[0] = $"{(green ? "GREEN" : "RED")} arena42 | map {g.MapId}, elements {(planted ? "planted (the level has none)" : "level")}: grinders {a.grinders.Count}, cover {a.cover.Count} | frames {frames} | {string.Join(" | ", notes)}"
                + $" | view: grinders {Arena42View.GrindersBuilt}, blades {Arena42View.BladesBuilt}, cover {Arena42View.CoverBuilt}, flares {Arena42View.FlaresShown}, shatters {Arena42View.ShattersShown}";
        }
    }
}
