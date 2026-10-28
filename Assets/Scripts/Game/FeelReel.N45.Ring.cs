// NIGHT45 builder A3 (NIGHT45.md card A3, T-65): the Player scenario `a345`, THE RING's home (THE KILN'S HEART, docs/ROOMS.md's identity card) on
// its frames. Dispatched by FeelReel.N45.cs (the lead's shared hook). Not run by its author (no Player in a builder worktree).
// Boot level 3 as n45-kiln through reel.sh: -skygrindMap n45-kiln -skygrindReelOnly a345 (L-062: -skygrindMap alone boots SURVIVAL), so the
// worm's room is the level's own arena (the k5 row's wyrmArena: Design/maps/wyrm45-arena.json). look43 (THE RING's dressing, WyrmRing43View)
// as the build's config has it (ON since NIGHT43). The player is God; input by forced frames only (L-046).
// Frames (dir/shot-N29-<name>.png):
//   A3-ring45-entrance       the arena's spawn, looking north as the fight starts: THE KILN MOUTH ahead (the landmark), the two DIVE plates
//                            either side of it, the crucible over the core (the card's entry view)
//   A3-ring45-rim-west       stood on the west COIL plate (box 32), looking east across the ring: the plate under foot, the coil's floor
//   A3-ring45-mid            back at the spawn ~6 s into the fight: the worm on its loop round the core (the 30 s problem in play)
//   A3-ring45-crumble-tell   the bar set just under half (the reel's only cheat, printed): the two 13 m perches' red-orange crack tell
//   A3-ring45-after-crumble  0.5 s after they fall: the high camp gone (the state change)
// Line: GREEN when the arena built is wyrm45-arena.json with its four plates drawn (look43's Slabs == 4: K1's step + three), the landmark drawn, and the crumble ran
// (tell then fallen); the counts printed.
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
        static partial void N45Ring(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Ring45Scenario(g, dir, res, offs, pre);

        static IEnumerator Ring45Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED a345: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            if (sim.campaignLevel == null || N45Levels.BaseId(sim.cfg, sim.campaignLevel.id) != "n43-kiln")
            {
                res[0] = $"RED a345: the run is on {sim.campaignLevel?.id ?? "?"}, not level 3 (boot -skygrindMap n45-kiln through reel.sh, L-062)";
                yield break;
            }
            g.forcedCombat = IdleCombat;
            God(g);
            if (!sim.Wyrm24Begin()) { res[0] = "RED a345: Wyrm24Begin failed (the worm's files were not found, or no BossKit)"; yield break; }
            const string want = "Design/maps/wyrm45-arena.json";
            string built = sim.wyrm39ArenaBuilt ?? "(the config's)";
            WyrmArena arena = sim.wyrm24Arena;
            Vec3 o = sim.Wyrm24Spawn - arena.spawn; // the room's origin in the world
            double Yaw(double dx, double dz) => Math.Atan2(-dx, -dz);
            double yaw = arena.spawnYawDeg * Math.PI / 180, pitch = 0.05;
            void Look(double y, double p)
            {
                yaw = y;
                pitch = p;
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            }
            IEnumerator Hold(float seconds) { for (float t = 0; t < seconds; t += Time.unscaledDeltaTime) { God(g); yield return null; } }
            g.Respawn(sim.Wyrm24Spawn, arena.spawnYawDeg);
            Look(yaw, 0.05);
            yield return Hold(1.0f);
            Start(g, "a345", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            var shots = new List<string>();
            yield return Shot29(dir, "A3-ring45-entrance", st);
            shots.Add("A3-ring45-entrance");

            // the west COIL plate (box 32 in wyrm45): stood on its top, looking east across the ring
            if (arena.boxes.Count > 32)
            {
                var (min, max, _) = arena.boxes[32];
                g.Respawn(o + new Vec3((min.x + max.x) * 0.5, max.y + 0.05, (min.z + max.z) * 0.5), 0);
                Look(Yaw(1, 0), -0.08);
                yield return Hold(0.5f);
                yield return Shot29(dir, "A3-ring45-rim-west", st);
                shots.Add("A3-ring45-rim-west");
            }

            // mid-fight: back at the spawn, the worm on its loop
            g.Respawn(sim.Wyrm24Spawn, arena.spawnYawDeg);
            Look(arena.spawnYawDeg * Math.PI / 180, -0.05);
            yield return Hold(6f);
            yield return Shot29(dir, "A3-ring45-mid", st);
            shots.Add("A3-ring45-mid");

            // the crumble (the state change): the bar put just under the crumble's mark, the perches' tell, then fallen
            int tellFrames = 0, fallen = 0;
            string armorNote = "-";
            Wyrm w = sim.wyrm24;
            if (w != null && w.Alive && sim.wyrm31 != null)
            {
                Vec3 perch = default;
                if (sim.wyrm31CrumbleBoxes.Count > 0 && sim.wyrm31CrumbleBoxes[0] < arena.boxes.Count)
                {
                    var (pmin, pmax, _) = arena.boxes[sim.wyrm31CrumbleBoxes[0]];
                    perch = o + new Vec3((pmin.x + pmax.x) * 0.5, pmax.y, (pmin.z + pmax.z) * 0.5);
                }
                // round 2 (n45-r1: the bar came back to 0.75, the perches restored): the idle player is STILL (design.n33.bosses33.stillArmor:
                // within stillRadius 4 m for stillSeconds 1.5 s) and the still armor gives back half of every drop of the bar each step, the
                // cheat's write included. A 6 m step first resets the stillness, so the write lands whole (n45ring crumble: fallen at 1.00 s,
                // the bar 0.49; --move no: restored, 0.75); the bar is then held under the mark every frame (only ever lowered) until the frames end
                Vec3 at = sim.mover.position;
                g.Respawn(new Vec3(at.x, at.y, at.z - 6), arena.spawnYawDeg);
                for (int k = 0; k < 2; k++) { God(g); yield return null; }
                Vec3 eye = sim.mover.position + new Vec3(0, 1.6, 0), d = perch - eye;
                Look(Yaw(d.x, d.z), Math.Atan2(d.y, Math.Sqrt(d.x * d.x + d.z * d.z)));
                double mark = Math.Min(0.49, sim.wyrm31.p.crumbleAt - 0.01);
                armorNote = sim.bosses33Armor ? "ON" : "off";
                w.body.hp = w.body.maxHp * mark;
                void HoldBar() { if (w.Alive && w.HpFrac > mark) w.body.hp = w.body.maxHp * mark; }
                bool shotTell = false;
                for (float t = 0; t < 6f && sim.wyrm31CrumbleState != 2; t += Time.unscaledDeltaTime)
                {
                    God(g);
                    HoldBar();
                    if (sim.wyrm31CrumbleState == 1) tellFrames++;
                    if (!shotTell && sim.wyrm31CrumbleState == 1 && sim.Wyrm31CrumbleProgress > 0.5)
                    {
                        shotTell = true;
                        yield return Shot29(dir, "A3-ring45-crumble-tell", st);
                        shots.Add("A3-ring45-crumble-tell");
                    }
                    yield return null;
                }
                if (sim.wyrm31CrumbleState == 2)
                {
                    for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime) { God(g); HoldBar(); yield return null; }
                    for (int i = 0; i < arena.boxes.Count; i++) if (sim.Wyrm31Fallen(i)) fallen++;
                    yield return Shot29(dir, "A3-ring45-after-crumble", st);
                    shots.Add("A3-ring45-after-crumble");
                }
            }
            yield return Stop();
            WyrmRing43View v = g.Ring43View;
            bool green = built == want && v != null && v.Slabs == 4 && v.Landmark == 1 && tellFrames > 0 && fallen == 2;
            res[0] = $"{(green ? "GREEN" : "RED")} a345 | map {g.MapId}, level {sim.campaignLevel?.id}, arena {built} (want {want}), boxes {arena.boxes.Count} | look43 shown {(v != null && v.Shown)}: slag plates {v?.Slabs} (want 4), kiln mouth {v?.Landmark}, perch lips {v?.Lips}, ride strips {v?.RideStrips}, hot zones {v?.HotZones} | the crumble: the bar set to {(w != null ? w.HpFrac.ToString("0.00", ci) : "-")} (the reel's cheat; the still armor at the write {armorNote}), tell frames {tellFrames}, fallen {fallen} (want 2) | frames {string.Join(", ", shots)}";
        }
    }
}
