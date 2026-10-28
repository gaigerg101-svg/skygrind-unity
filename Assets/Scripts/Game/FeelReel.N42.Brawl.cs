// NIGHT42 builder B1 (brawl42): the Player scenario `brawl42` (THE ICEBREAKER's new attacks: each tell and its answer). The dispatch line is
// the lead's (FeelReel.N42.cs). Not run by its author (no Player in the worktree). The shape is NIGHT40's brawl40 reel (FeelReel.N40.Brawl.cs):
// a CAMPAIGN run on n40-iceline (boot it with reel.sh + -skygrindReelOnly, L-062) or the arena placed at (3000, 0, 0) on any other map; the
// player kept alive (God); input through forcedMove / forcedCombat only (L-046), driven by Brawl40Pilot (the skilled pilot, which answers the
// sweep with a jump and the leap with a dash out of the circle). The run's NIGHT40 bits forced on (brawl40, iceline40) and NIGHT42's brawl42
// on (printed). The reel queues a sweep, a leap, a second sweep and a second leap (Brawl40.Force) after the first haymaker, so every new tell
// and its answer is framed even though the pilot fights up close.
// Frames (shot-N29-<name>.png), what each should show:
//   B42-sweep-tell     the sweep at 70 % of its wind-up: crouched low, the right arm cocked back near the ice, its fist (and shell) red-orange,
//                      a red-orange ring on the ice round it at the arm's reach (4 m past its body).
//   B42-sweep          the spin as the arm passes the player's side: a bright red-orange blade at shin height from its body to the ring.
//   B42-sweep-answer   the same moment seen for the answer: the player's feet off the ice (the camera higher than the ring; pilot jumped).
//   B42-dizzy          0.3 s into the dizzy recover: the head wobbling (the punish window after the sweep).
//   B42-leap-tell      the leap's wind-up at 40 %: crouched, a red-orange circle on the ice under the player (tracking).
//   B42-leap-lock      just after the lock: the circle brighter with an inner ring (leave it now); the player already moving out of it.
//   B42-leap-air       mid-air (its height > 2 m): the body above the ice, the locked circle under it, the player outside it.
//   B42-leap-stuck     0.3 s after the landing: bent over, both fists in the ice, CYAN (hit it now).
// Line: GREEN when the fight began, the sweep and the leap were each seen at least once with their tells >= 0.9 s (DECIDE42 D-5), the pilot
// jumped a sweep or a leap missed it at least once (an answer seen), the view drew the reach ring, the blade and the locked circle, and its
// body never left the arena; RED otherwise.
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
        static partial void N42Brawl(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Brawl42Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Brawl42Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            N40Blocks? overWas = sim.n40Override;
            N42Blocks? over42Was = sim.n42Override;
            N40Blocks want = N40Blocks.ForceOff ? N40Blocks.Off : N40Blocks.From(g.cfg);
            want.brawl40 = true;
            want.iceline40 = true;
            sim.n40Override = want;
            N42Blocks want42 = N42Blocks.ForceOff ? N42Blocks.Off : N42Blocks.From(g.cfg);
            want42.brawl42 = true;
            sim.n42Override = want42;
            offs.Add(() => { sim.n40Override = overWas; sim.n42Override = over42Was; });
            bool campaign = N24CampaignGate(g) == null;
            if (campaign)
            {
                offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(240);
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                DoorFree41(g);
                campaign = sim.campaignOn && sim.campaign != null;
            }
            else
            {
                sim.n40.brawl40 = true;
                sim.n42.brawl42 = true; // a survival run already started: the bit for the begin (Brawl42Arm reads it there)
            }
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            bool levelArena = sim.brawl40Centre.HasValue;
            if (!levelArena && !sim.Brawl40Place(new Vec3(3000, 0, 0))) { res[0] = "RED: the arena file was not found (design.n40.brawl40.arena)"; yield break; }
            Vec3 c = sim.brawl40Centre.Value;
            RoomDef x9 = campaign && sim.campaignLevel != null ? sim.campaignLevel.rooms.Find(r => r.boss == "brawl40") : null;
            Vec3 stand = x9 != null ? x9.spawn + new Vec3(0, 0.05, 0) : sim.brawl40Arena.spawn + c + new Vec3(0, 0.05, 0);
            double standYaw = x9 != null ? x9.spawnYawDeg : sim.brawl40Arena.spawnYawDeg;
            foreach (string w in new[] { "gauntlet", "revolver" }) if (!sim.loadout.owned.Contains(w)) sim.loadout.AddWeapon(w);
            int gSlot = sim.loadout.owned.IndexOf("gauntlet");
            g.Respawn(stand, standYaw);
            g.input.SetLook(standYaw * Math.PI / 180, 0.05);
            g.forcedMove = new InputFrame { yaw = standYaw * Math.PI / 180, pitch = 0.05 };
            g.forcedCombat = new CombatInput { weapon = gSlot };
            for (int k = 0; k < 60 && (sim.brawl40 == null || !sim.brawl40.Alive) && campaign; k++) yield return null;
            g.forcedCombat = IdleCombat;
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, map {g.MapId}, room {sim.campaign?.Room?.id ?? "none"}, arena {(levelArena ? "the level's" : "placed at 3000,0,0")}, n42.brawl42 {sim.n42.brawl42}, view {(Brawl40View.Current != null ? "on" : "MISSING")}";
            if ((sim.brawl40 == null || !sim.brawl40.Alive) && !sim.BossRoomBegin("brawl40", out Vec3 spawn, out double yawDeg))
            {
                res[0] = "RED: the fight did not begin | " + pc;
                yield break;
            }
            Brawl40 b = sim.brawl40;
            if (!b.on42) { res[0] = "RED: brawl42 is not on for the fight (Brawl42Arm) | " + pc; yield break; }
            Start(g, "brawl42", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            var pilot = new Brawl40Pilot { hand = new Mulberry32(42), style = Brawl40Pilot.Style.Mixed };
            double t0 = sim.SimTime, dizzyAt = -1, stuckAt = -1;
            int lastSimStep = sim.simStep, maxOutside = 0;
            bool sweepTell = false, sweepShot = false, sweepAnswer = false, dizzyShot = false, leapTell = false, leapLock = false, leapAir = false, leapStuck = false;
            bool ringSeen = false, bladeSeen = false, circleSeen = false, sawHaymaker = false;
            var forcedList = new List<string> { Brawl40.Sweep, Brawl40.Leap, Brawl40.Sweep, Brawl40.Leap };
            double wallEnd = Time.realtimeSinceStartup + 150;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 90 && b.Alive)
            {
                God(g);
                double now = sim.SimTime - t0;
                int ran = Math.Max(1, sim.simStep - lastSimStep);
                lastSimStep = sim.simStep;
                InputFrame mv = default;
                CombatInput cb = new CombatInput { weapon = -1 };
                for (int k = 0; k < ran; k++)
                {
                    pilot.Step(sim, sim.input.Yaw, sim.simStep - ran + 1 + k);
                    InputFrame m = pilot.Move;
                    m.jump.pressed |= mv.jump.pressed; m.jump.held |= mv.jump.pressed;
                    m.dash.pressed |= mv.dash.pressed; m.dash.held |= mv.dash.pressed;
                    CombatInput cc = pilot.Combat;
                    cc.fire.pressed |= cb.fire.pressed; cc.fire.held |= cb.fire.pressed;
                    cc.punch.pressed |= cb.punch.pressed; cc.punch.held |= cb.punch.pressed;
                    if (cc.weapon < 0) cc.weapon = cb.weapon;
                    mv = m;
                    cb = cc;
                }
                // the new attacks' frames: the camera on the boss (or, for the leap's circle, down at it)
                bool newAtk = b.attack == Brawl40.Sweep || b.attack == Brawl40.Leap;
                if (newAtk && (b.state == Brawl40State.Tell || b.state == Brawl40State.Active || b.state == Brawl40State.Stuck))
                {
                    Vec3 look = b.attack == Brawl40.Leap && b.state == Brawl40State.Tell ? b.leapTo + (b.Centre - b.leapTo) * 0.5 : b.Centre;
                    Vec3 dv = look - sim.Eye;
                    mv.yaw = JsMath.Atan2(-dv.x, -dv.z);
                    mv.pitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
                }
                g.input.SetLook(mv.yaw, mv.pitch);
                g.forcedMove = mv;
                g.forcedCombat = cb;
                maxOutside = Math.Max(maxOutside, b.outside);
                if (b.attack == Brawl40.Haymaker) sawHaymaker = true;
                if (!b.ForcePending && b.state == Brawl40State.Idle && sawHaymaker && forcedList.Count > 0 && now > 3)
                {
                    b.Force(forcedList[0]);
                    forcedList.RemoveAt(0);
                }
                Brawl40View v = Brawl40View.Current;
                if (v != null) { ringSeen |= v.SweepRingShown; bladeSeen |= v.SweepBladeShown; circleSeen |= v.LeapLockedShown; }
                // the sweep
                if (!sweepTell && b.Telling(Brawl40.Sweep) && b.Progress >= 0.7) { sweepTell = true; yield return Shot29(dir, "B42-sweep-tell", st); continue; }
                if (b.Acting(Brawl40.Sweep))
                {
                    Vec3 me = sim.mover.position;
                    double bear = JsMath.Atan2(me.x - b.Feet.x, me.z - b.Feet.z), d = b.sweepArm - bear;
                    while (d > Math.PI) d -= 2 * Math.PI;
                    while (d < -Math.PI) d += 2 * Math.PI;
                    if (!sweepShot && Math.Abs(d) < 0.6) { sweepShot = true; yield return Shot29(dir, "B42-sweep", st); continue; }
                    if (!sweepAnswer && sweepShot && Math.Abs(d) < 0.3 && me.y - b.origin.y > b.p42.sweepHeight) { sweepAnswer = true; yield return Shot29(dir, "B42-sweep-answer", st); continue; }
                }
                if (b.state == Brawl40State.Recover && b.attack == Brawl40.Sweep && dizzyAt < 0) dizzyAt = now;
                if (!dizzyShot && dizzyAt >= 0 && now - dizzyAt >= 0.3 && b.state == Brawl40State.Recover && b.attack == Brawl40.Sweep) { dizzyShot = true; yield return Shot29(dir, "B42-dizzy", st); continue; }
                // the leap
                if (!leapTell && b.Telling(Brawl40.Leap) && b.Progress >= 0.4 && !b.leapLocked) { leapTell = true; yield return Shot29(dir, "B42-leap-tell", st); continue; }
                if (!leapLock && b.Telling(Brawl40.Leap) && b.leapLocked && b.Progress >= b.p42.leapTrack + 0.12) { leapLock = true; yield return Shot29(dir, "B42-leap-lock", st); continue; }
                if (!leapAir && b.Acting(Brawl40.Leap) && b.air42 > 2) { leapAir = true; yield return Shot29(dir, "B42-leap-air", st); continue; }
                if (b.StuckNow && b.attack == Brawl40.Leap && stuckAt < 0) stuckAt = now;
                if (!leapStuck && stuckAt >= 0 && now - stuckAt >= 0.3 && b.StuckNow && b.attack == Brawl40.Leap) { leapStuck = true; yield return Shot29(dir, "B42-leap-stuck", st); continue; }
                if (forcedList.Count == 0 && sweepTell && leapStuck && !b.ForcePending && b.state == Brawl40State.Idle && now > 30) break; // every frame taken
                yield return null;
            }
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            // the new attacks' tells (DECIDE42 D-5: >= 0.9 s)
            var sb = new System.Text.StringBuilder();
            bool tellsOk = true;
            int sweepsSeen = 0, leapsSeen = 0;
            foreach (var (atk, steps) in b.tellLog)
            {
                if (atk != Brawl40.Sweep && atk != Brawl40.Leap) continue;
                double len = steps * sim.step;
                if (atk == Brawl40.Sweep) sweepsSeen++; else leapsSeen++;
                bool okLen = len >= 0.89;
                tellsOk &= okLen;
                sb.Append($" {atk} {len.ToString("0.00", ci)} s{(okLen ? "" : " RED")};");
            }
            bool answered = pilot.sweepJumps > 0 && b.sweeps > b.sweepHits || b.leaps > b.leapHits;
            bool ok = sweepsSeen > 0 && leapsSeen > 0 && tellsOk && answered && ringSeen && bladeSeen && circleSeen && maxOutside == 0;
            res[0] = $"{(ok ? "GREEN" : "RED")}: tells{sb} sweeps {b.sweeps} (hit {b.sweepHits}, chained {b.sweepChains}, pilot jumps {pilot.sweepJumps}), leaps {b.leaps} (hit {b.leapHits}, rings {b.leapRings}, pilot escapes {pilot.leapEscapes}); view: reach ring {ringSeen}, blade {bladeSeen}, locked circle {circleSeen}; frames sweep-tell {sweepTell} sweep {sweepShot} answer {sweepAnswer} dizzy {dizzyShot} leap-tell {leapTell} lock {leapLock} air {leapAir} stuck {leapStuck}; health lost {b.hurt42.ToString("0", ci)}; frames outside {maxOutside}; fight {(sim.SimTime - t0).ToString("0.0", ci)} s, boss hp {(b.HpFrac * 100).ToString("0", ci)} % | {pc}";
        }
    }
}
