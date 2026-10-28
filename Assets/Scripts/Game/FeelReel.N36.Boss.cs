// NIGHT36 builder B: the Player scenario of boss36 / walkin36 / bossmodel36 (the name is FeelReel.N27.cs PhaseN36B; the body is the partial N36BossBody).
// Not run by its author (no Player in the worktree). Input through the game's forced move / combat frames only (L-046); the player kept alive (God).
//   boss36  THE LANTERN's hall: the level's own (an n36 level with the lantern36 row: boot -skygrindMap <L1>) or, on any other map, the hall put at
//     (2400, 0, 0) (SurvivalSim.Lantern36Place). The player stands 2.5 m inside the gap facing the well; Lantern36Begin runs as the campaign's
//     walk-in would (no Respawn: his feet are checked). Then he stands and watches; when a fan's orb comes within 2.4 m he punches it (the parry).
//     Frames (shot-N29-<name>.png):
//       N36-boss-reveal-a  0.3 s after the begin: the slab closed red behind, the beam out of the well, nothing risen yet
//       N36-boss-reveal-b  1.6 s: the lantern half out of the well, arms folded, the beam on it
//       N36-boss-reveal-c  3.2 s: risen to 4.5 m, arms open, the name card
//       N36-boss-tell-floor / -tell-fan / -tell-sweep  each attack's tell at 70 % of its wind-up: its pose and colour (floor RED: arms down, the
//                          lava ring growing from the well; fan YELLOW: arms up, plates open, the core yellow; sweep RED: one arm out at the wall
//                          and the red bar on the wall)
//       N36-boss-parry     3 frames after the parry: the orb white, flying back to the core
//       N36-boss-stun      1.0 s into the 50 % stun (the bar is moved there by hand after the sweep): dropped on the well, tilted, plates open,
//                          the core cyan
//     Line: GREEN when the begin kept his feet (< 0.05 m), the three tells were seen with lengths 1.0-1.5 s, a parry landed (parries >= 1) and the
//     stun ran; RED otherwise, with the measures. -skygrindOff36: the scenario still runs the fight (it calls the boss directly) but the
//     model is the plain block (bossmodel36 off): the RED side of the body's frames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N36BossBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "boss36") body = Boss36Scenario(g, dir, res, offs, pre);
        }

        static (double yaw, double pitch) Aim36(Vec3 eye, Vec3 at)
        {
            Vec3 dv = at - eye;
            return (Math.Atan2(-dv.x, -dv.z), Math.Atan2(dv.y, Math.Sqrt(dv.x * dv.x + dv.z * dv.z)));
        }

        static IEnumerator Boss36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            bool levelHall = sim.lantern36Centre.HasValue;
            if (!levelHall) sim.Lantern36Place(new Vec3(2400, 0, 0));
            Vec3 c = sim.lantern36Centre.Value;
            Vec3 stand = new Vec3(c.x, c.y + 0.05, c.z + sim.lantern36Arena.p.apothem - 2.5);
            g.Respawn(stand, 0);
            g.input.SetLook(0, 0.12);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0.12 };
            yield return Frames(30);
            string pc = $"hall {(levelHall ? "the level's" : "placed at 2400,0,0")}, n36.boss36 {sim.n36.boss36}, walkin36 {sim.n36.walkin36}, tells36 {sim.n36.tells36}, bossmodel36 {N36Fixes.ViewRowOn(g.cfg, "bossmodel36")}, view {(Boss36View.Current != null ? "on" : "MISSING (the lead's two lines in SurvivalGame.Boss26.cs)")}";
            Start(g, "boss36", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            Vec3 before = sim.mover.position;
            if (!sim.Lantern36Begin(out Vec3 spawn, out double yawDeg))
            {
                yield return Stop();
                res[0] = "RED: Lantern36Begin returned false | " + pc;
                yield break;
            }
            yield return null;
            double moved = Vec3.DistanceTo(before, sim.mover.position);
            Lantern36 l = sim.lantern36;
            double t0 = sim.SimTime;
            bool ra = false, rb = false, rc = false, parryShot = false, stunShot = false, stunForced = false;
            var told = new HashSet<string>();
            var tellLen = new Dictionary<string, double>();
            int parryFrame = -1, frame = 0;
            double wallEnd = Time.realtimeSinceStartup + 120;
            while (Time.realtimeSinceStartup < wallEnd && l.Alive && sim.SimTime - t0 < 60)
            {
                God(g);
                frame++;
                double now = sim.SimTime - t0;
                Vec3 eye = sim.Eye;
                // look at the nearest incoming orb when one is close (to punch it), else at the core
                Lantern36Orb near = null;
                double nd = 7;
                foreach (Lantern36Orb ob in l.orbs) { if (!ob.alive || ob.back) continue; double d = Vec3.DistanceTo(ob.pos, eye); if (d < nd) { nd = d; near = ob; } }
                var (yaw, pitch) = Aim36(eye, near != null ? near.pos : l.Centre);
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                g.forcedCombat = near != null && nd < 2.4 ? new CombatInput { weapon = -1, punch = new Btn { pressed = true, held = true } } : IdleCombat;
                if (!ra && now >= 0.3) { ra = true; yield return Shot29(dir, "N36-boss-reveal-a", st); continue; }
                if (!rb && now >= 1.6) { rb = true; yield return Shot29(dir, "N36-boss-reveal-b", st); continue; }
                if (!rc && now >= 3.2) { rc = true; yield return Shot29(dir, "N36-boss-reveal-c", st); continue; }
                Boss36Brain b = l.brain;
                if (b.state == Boss36State.Tell && b.Current != null && !b.Current.camp && b.Progress >= 0.7 && told.Add(b.Current.id))
                {
                    string nm = b.attack == Lantern36.Floor ? "floor" : b.attack == Lantern36.Fan ? "fan" : "sweep";
                    yield return Shot29(dir, "N36-boss-tell-" + nm, st);
                    continue;
                }
                if (l.parries > 0 && parryFrame < 0) parryFrame = frame;
                if (!parryShot && parryFrame > 0 && frame - parryFrame >= 3) { parryShot = true; yield return Shot29(dir, "N36-boss-parry", st); continue; }
                // after the sweep's tell (all three seen) and one parry, move the bar to the stun by hand
                if (!stunForced && told.Count >= 3 && (parryShot || now > 40) && b.state == Boss36State.Rest)
                {
                    stunForced = true;
                    l.body.hp = Math.Min(l.body.hp, l.body.maxHp * b.p.stunAt - 1);
                }
                if (!stunShot && b.state == Boss36State.Stun && b.stateSteps * sim.step >= 1.0) { stunShot = true; yield return Shot29(dir, "N36-boss-stun", st); continue; }
                if (stunShot && parryShot && told.Count >= 3) break;
                yield return null;
            }
            foreach (var (a, steps) in l.brain.tellLog) if (!tellLen.ContainsKey(a)) tellLen[a] = steps * sim.step;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            bool tellsOk = told.Count >= 3;
            var sb = new System.Text.StringBuilder();
            foreach (var kv in tellLen)
            {
                bool inBand = kv.Value >= 0.99 && kv.Value <= 1.51;
                tellsOk &= inBand;
                sb.Append($" {kv.Key} {kv.Value.ToString("0.00", ci)} s{(inBand ? "" : " RED")};");
            }
            bool ok = moved < 0.05 && tellsOk && l.parries >= 1 && l.brain.stuns >= 1;
            res[0] = $"{(ok ? "GREEN" : "RED")}: begin moved the feet {moved.ToString("0.000", ci)} m (spawn for a retry {spawn.x.ToString("0.0", ci)},{spawn.z.ToString("0.0", ci)}), tells seen {told.Count}/3:{sb} parries {l.parries} cracks {l.cracksTotal} breaks {l.breaks}, stuns {l.brain.stuns}, punishes {l.brain.punishes}, orbs that hit {l.orbHits}, view pose at the end '{Boss36View.Current?.Pose}', stun bursts drawn {Boss36View.Current?.stunBursts}, core colour at the end {Boss36View.Current?.CoreColour}, slab drawn {Boss36View.Current?.SlabShown} | {pc}";
        }
    }
}
