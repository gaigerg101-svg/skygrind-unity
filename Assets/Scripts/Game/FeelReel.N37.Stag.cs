// NIGHT37 builder S: the Player scenario of stag37 (the group FeelReel.N27.cs PhaseN37S; the body is the partial N37StagBody). Not run by its author
// (no Player in the worktree). Input through the game's forced move / combat frames only (L-046), driven by Stag37Pilot each frame; the player
// kept alive (God). THE CANAL is the level's (n37-iceline: boot its map) or, on any other map, placed at (3000, 0, 0) (SurvivalSim.Stag37Place).
// The row's bit is forced on for the run (sim.n37.stag37 = true; printed). The player stands in the entry gap facing north; Stag37Begin runs as
// the campaign's walk-in would (no Respawn: his feet are checked). The reel queues a lane cut and a kick back once each (brain.Force) so every tell
// is seen; the band is the harness's (stag37 band), not this run's.
// Frames (shot-N29-<name>.png), what each should show:
//   S37-canal         before the begin, from the gap looking north: the south turn's faceted bank wall with its two cyan trims, the island's end,
//                     straight A's three lanes with pale dashed lines going away, the pale lights.
//   S37-walkin        0.4 s after the begin: THE STAG standing ~16 m ahead on the ice (dark body, red-orange rims, pale antler crown), the slab red
//                     behind (not in view), the name card soon.
//   S37-bolt          1.0 s after the reveal: the Stag galloping away up straight A, legs mid-stride.
//   S37-slip          0.4 s into the first slipstream push: the cyan streak on the ice from its hind legs to you.
//   S37-tell-<id>     each tell at 70 % of its wind-up: shard kick RED hooves + a RED strip down its lane behind it; ice chunk YELLOW antlers, head
//                     back; lane cut RED antlers + a RED stripe across YOUR lane behind it; kick back RED hooves; antler charge YELLOW antlers, head
//                     down, facing you.
//   S37-legpunch      2 frames after the first hind-leg punch: close behind it, the hind bands CYAN, its stumble.
//   S37-window        0.5 s into the first CYAN window: it lies on its side skidding, K's cyan glow round it, the stagger bar cyan.
//   S37-launch        3 frames after the launch punch: it at the canal's outer wall, the hit flash.
//   S37-floes         the first leg-2 frame with a floe pulsing RED: straight B's floes, one RED (a lane ahead of you, or behind the Stag).
//   S37-floes-down    the first frame after that with a floe gone: the dark water where it was.
// The PULSE GAUNTLET is in hand (level 2's weapon: Stag37Begin gives it, the pilot holds it and swings it: its fire).
//   S37-leg3          when it has turned at the tunnel (or the bank): it facing you down the lane, antlers lowering.
//   S37-parry         3 frames after the charge parry: it thrown back, the crash.
//   S37-kill          0.6 s after the kill: it down; the slab open.
// Line: GREEN when the begin kept his feet (< 0.05 m), the Stag died, a leg punch, a launch and the charge parry landed, the slipstream was seen
// and every tell seen lasted 0.8-1.5 s; RED otherwise, with the measures.
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
        static partial void N37StagBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "stag37") body = Stag37Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Stag37Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            bool rowWas = sim.n37.stag37;
            sim.n37.stag37 = true;
            bool levelCanal = sim.stag37Centre.HasValue;
            if (!levelCanal) sim.Stag37Place(new Vec3(3000, 0, 0));
            Stag37Arena a = sim.stag37Arena;
            Vec3 c = sim.stag37Centre.Value;
            Vec3 stand = a.GapPoint + c + new Vec3(0, 0.05, -1.5);
            g.Respawn(stand, 0);
            g.input.SetLook(0, 0.05);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0.05 };
            yield return Frames(30);
            string pc = $"canal {(levelCanal ? "the level's" : "placed at 3000,0,0")}, n37.stag37 {rowWas} (forced on), boss37 {sim.n37.boss37}, tells36 {sim.n36.tells36}, view {(Stag37View.Current != null ? "on" : "MISSING (the lead's two lines in SurvivalGame.Boss26.cs)")}";
            Start(g, "stag37", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            yield return Shot29(dir, "S37-canal", st);
            Vec3 before = sim.mover.position;
            if (!sim.Stag37Begin(out Vec3 spawn, out double yawDeg))
            {
                yield return Stop();
                res[0] = "RED: Stag37Begin returned false | " + pc;
                yield break;
            }
            yield return null;
            double moved = Vec3.DistanceTo(before, sim.mover.position);
            Stag37 s = sim.stag37;
            Boss37Brain b = s.brain;
            var pilot = new Stag37Pilot { hand = new Mulberry32(37) };
            double t0 = sim.SimTime, slipAt = -1, windowAt = -1, killAt = -1, revealEnd = s.p.revealSeconds;
            int frame = 0, legPunchFrame = -1, launchFrame = -1, parryFrame = -1;
            bool floeDownShot = false, walkShot = false, boltShot = false, slipShot = false, legShot = false, winShot = false, launchShot = false, floeShot = false, leg3Shot = false, parryShot = false, killShot = false;
            bool forcedCut = false, forcedKick = false;
            var told = new HashSet<string>();
            double wallEnd = Time.realtimeSinceStartup + 150;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 80)
            {
                God(g);
                frame++;
                double now = sim.SimTime - t0;
                if (s.Alive)
                {
                    pilot.Step(sim, sim.input.Yaw, frame);
                    g.input.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                    g.forcedMove = pilot.Move;
                    g.forcedCombat = pilot.Combat;
                }
                else { g.forcedMove = new InputFrame { yaw = sim.input.Yaw, pitch = sim.input.Pitch }; g.forcedCombat = IdleCombat; }
                // every tell once: a lane cut and a kick back queued in leg 1 / 2 if the roll has not brought them
                if (s.Alive && b.leg < 3 && b.state == Boss37State.Rest && now > 6)
                {
                    if (!forcedCut && !told.Contains(Stag37.LaneCut)) { forcedCut = true; b.Force(Stag37.LaneCut); }
                    else if (!forcedKick && !told.Contains(Stag37.KickBack) && forcedCut && now > 10) { forcedKick = true; b.Force(Stag37.KickBack); }
                }
                if (!walkShot && now >= 0.4) { walkShot = true; yield return Shot29(dir, "S37-walkin", st); continue; }
                if (!boltShot && now >= revealEnd + 1.0) { boltShot = true; yield return Shot29(dir, "S37-bolt", st); continue; }
                if (sim.Stag37Slipping && slipAt < 0) slipAt = now;
                if (!slipShot && slipAt >= 0 && now - slipAt >= 0.4 && sim.Stag37Slipping) { slipShot = true; yield return Shot29(dir, "S37-slip", st); continue; }
                if (b.state == Boss37State.Tell && b.Current != null && b.Progress >= 0.7 && told.Add(b.Current.id))
                {
                    yield return Shot29(dir, "S37-tell-" + b.Current.id, st);
                    continue;
                }
                if (s.legPunches > 0 && legPunchFrame < 0) legPunchFrame = frame;
                if (!legShot && legPunchFrame > 0 && frame - legPunchFrame >= 2) { legShot = true; yield return Shot29(dir, "S37-legpunch", st); continue; }
                if (b.Open && windowAt < 0) windowAt = now;
                if (!winShot && windowAt >= 0 && now - windowAt >= 0.5 && b.Open) { winShot = true; yield return Shot29(dir, "S37-window", st); continue; }
                if (s.launches > 0 && launchFrame < 0) launchFrame = frame;
                if (!launchShot && launchFrame > 0 && frame - launchFrame >= 3) { launchShot = true; yield return Shot29(dir, "S37-launch", st); continue; }
                if (!floeShot && b.leg == 2 && Stag37View.Current != null && Stag37View.Current.FloesTold > 0) { floeShot = true; yield return Shot29(dir, "S37-floes", st); continue; }
                if (!floeDownShot && floeShot && Stag37View.Current != null && Stag37View.Current.FloesDown > 0) { floeDownShot = true; yield return Shot29(dir, "S37-floes-down", st); continue; }
                if (!leg3Shot && s.leg3 == Stag37Leg3.Turned) { leg3Shot = true; yield return Shot29(dir, "S37-leg3", st); continue; }
                if (s.chargeParries > 0 && parryFrame < 0) parryFrame = frame;
                if (!parryShot && parryFrame > 0 && frame - parryFrame >= 3) { parryShot = true; yield return Shot29(dir, "S37-parry", st); continue; }
                if (!s.Alive && b.state != Boss37State.Dying && killAt < 0) killAt = now;
                if (!s.Alive && killAt < 0 && b.state == Boss37State.Dying) killAt = now;
                if (!killShot && killAt >= 0 && now - killAt >= 0.6) { killShot = true; yield return Shot29(dir, "S37-kill", st); break; }
                yield return null;
            }
            double fightS = (killAt >= 0 ? killAt : sim.SimTime - t0) - revealEnd;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            bool tellsOk = told.Count >= 3;
            var sb = new System.Text.StringBuilder();
            var seen = new HashSet<string>();
            foreach (var (atk, steps) in b.tellLog)
            {
                if (!seen.Add(atk)) continue;
                double len = steps * sim.step;
                bool inBand = len >= 0.79 && len <= 1.51;
                tellsOk &= inBand;
                sb.Append($" {atk} {len.ToString("0.00", ci)} s{(inBand ? "" : " RED")};");
            }
            bool ok = moved < 0.05 && !s.Alive && s.legPunches >= 1 && s.launches >= 1 && s.chargeParries >= 1 && slipAt >= 0 && tellsOk;
            res[0] = $"{(ok ? "GREEN" : "RED")}: begin moved the feet {moved.ToString("0.000", ci)} m; killed {!s.Alive} in {fightS.ToString("0.0", ci)} s of fight (the band is the harness's); tells seen {told.Count}:{sb} leg punches {s.legPunches}, chunk parries {s.chunkParries}, windows {b.windows}, launches {s.launches}, charge parried {s.chargeParries}, slipstream first at {slipAt.ToString("0.0", ci)} s ({s.slipSeconds.ToString("0.0", ci)} s in all), floes dropped {sim.stag37FloeDrops} (RED frame {floeShot}, down frame {floeDownShot}), weapon in hand '{sim.loadout.Weapon}', falls {sim.stag37Falls}, taken: shards {s.shardHits} chunks {s.chunkHits} kicks {s.kickHits} charges {s.chargeHits}; pilot still steps {pilot.stillSteps}; view pose at the end '{Stag37View.Current?.Pose}', bands cyan seen {legShot}, glow {Stag37View.Current?.GlowLevel.ToString("0.00", ci)} | {pc}";
        }
    }
}
