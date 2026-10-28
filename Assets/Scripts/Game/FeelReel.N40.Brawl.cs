// NIGHT40 builder B2: the Player scenario `brawl40` (THE ICEBREAKER). The lead's dispatch (FeelReel.N27.cs): add `|| Array.IndexOf(PhaseN40B, s)
// >= 0` to IsPhaseN37 and, in the body switch, `else if (Array.IndexOf(PhaseN40B, s) >= 0) N40BrawlBody(s, g, dir, res, offs, pre, ref body);`.
// Not run by its author (no Player in the worktree). Input through the game's forced move / combat frames only (L-046), driven by Brawl40Pilot
// (Mixed: the skilled pilot; `brawl40casual`: the casual pilot) each frame; the player kept alive (God). The arena is the level's (n40-iceline's
// x9: boot that map with -skygrindReelOnly) or, on any other map, placed at (3000, 0, 0) (SurvivalSim.Brawl40Place). The row's bit is forced on
// for the run (n40Override; printed). On a campaign level the reel starts a CAMPAIGN run (DOORFREE; no SURVIVAL countdown), puts the player at
// x9's spawn with the gauntlet in hand (the KESTREL owned) and the campaign begins the fight (his feet are checked). The reel queues a slam, a block and a rush once each (Brawl40.Force) so every tell is seen;
// the band is the harness's (`n40brawl band`), not this run's.
// Frames (shot-N29-<name>.png), what each should show:
//   B40-arena        the walk-in, from x9's spawn looking in: the 36 m ice floor, the rim walls with cyan trims at the foot and the top,
//                    the four pillars with cyan trims.
//   B40-line         1.0 s after the begin: the boss line "GET IN CLOSE: <fire> THE GAUNTLET HITS FULL" (not cut off), the brute
//                    standing ~22 m ahead (dark, red-orange rims and eyes, big fists, small head, hunched), the name card soon.
//   B40-tell-<id>    each attack at 70 % of its wind-up: brawlHaymaker the right fist cocked high and back, glowing red-orange; brawlSlam both
//                    fists overhead glowing and the red-orange ring decal on the ice round it; brawlBlock a YELLOW block raised in its left
//                    fist; brawlRush head down and a red-orange line on the ice along its path.
//   B40-swing        the haymaker's swing (the arm out, the lunge).
//   B40-stuck        0.3 s into the stuck fist: bent over, the fist in the ice, CYAN.
//   B40-ring         0.4 s into a slam's shockwave: the red-orange ring running out along the floor.
//   B40-block        the block in flight (yellow), and B40-parry 3 frames after a parry (white, on its way back).
//   B40-window       0.5 s into the first window: down on one knee, CYAN rims.
//   B40-phase        0.5 s into the phase change: arms spread, rims flared, the PHASE 2 banner.
//   B40-floes        phase 2: the first floe cracking (a 6 m square of the ice RED); B40-floes-open: the first open water (a dark square).
//   B40-kill         0.6 s after the kill: it down, the slab open.
// Line: GREEN when the begin kept his feet (< 0.05 m), it died, every one of the four tells was seen with its length in boss_rules 1's band
// (haymaker >= 0.9 s, slam 0.65-0.9, block 0.65-0.9, rush 0.45-0.65), a window opened, and its body never left the arena; RED otherwise.
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
        public static readonly string[] PhaseN40B = { "brawl40", "brawl40casual" };

        static void N40BrawlBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "brawl40" || s == "brawl40casual") body = Brawl40Scenario(g, dir, res, offs, pre, s == "brawl40casual");
        }

        static IEnumerator Brawl40Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, bool casual)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            // the run's NIGHT40 bits: the config's, with brawl40 and iceline40 forced on (a run reads them at its start: the override, put back after)
            N40Blocks? overWas = sim.n40Override;
            bool rowWas = sim.n40.brawl40;
            N40Blocks want = N40Blocks.ForceOff ? N40Blocks.Off : N40Blocks.From(g.cfg);
            want.brawl40 = true;
            want.iceline40 = true;
            sim.n40Override = want;
            offs.Add(() => { sim.n40Override = overWas; });
            // a CAMPAIGN run when the map is a campaign level (as B4's loom37 and B5's crown37 reels; DOORFREE): no SURVIVAL round countdown, the
            // level's own start set (iceline40: the gauntlet), and entering x9 makes the campaign begin the fight itself (the walk-in)
            bool campaign = N24CampaignGate(g) == null;
            if (campaign)
            {
                offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(240); // the run-start title has run out before any frame
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                DoorFree41(g); // NIGHT41 lead (A9, T2's helper): DOORFREE + its queued room banners cleared (FeelReel.N41.cs)
                campaign = sim.campaignOn && sim.campaign != null;
            }
            else
            {
                sim.n40.brawl40 = true; // a survival run already started: the bit for the begin (the walk-in flag is the placement's)
            }
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            bool levelArena = sim.brawl40Centre.HasValue;
            if (!levelArena && !sim.Brawl40Place(new Vec3(3000, 0, 0))) { res[0] = "RED: the arena file was not found (design.n40.brawl40.arena)"; yield break; }
            Vec3 c = sim.brawl40Centre.Value;
            // the player at the boss room's spawn (x9's: the arena's centre + 16 m, yaw 0), looking in; on a map without the row, the arena's spawn
            RoomDef x9 = campaign && sim.campaignLevel != null ? sim.campaignLevel.rooms.Find(r => r.boss == "brawl40") : null;
            Vec3 stand = x9 != null ? x9.spawn + new Vec3(0, 0.05, 0) : sim.brawl40Arena.spawn + c + new Vec3(0, 0.05, 0);
            double standYaw = x9 != null ? x9.spawnYawDeg : sim.brawl40Arena.spawnYawDeg;
            // level 2's weapons at its boss: the gauntlet (iceline40's start set) in hand, the KESTREL (the armoury's award) owned
            foreach (string w in new[] { "gauntlet", "revolver" }) if (!sim.loadout.owned.Contains(w)) sim.loadout.AddWeapon(w);
            int gSlot = sim.loadout.owned.IndexOf("gauntlet");
            g.Respawn(stand, standYaw);
            g.input.SetLook(standYaw * Math.PI / 180, 0.05);
            g.forcedMove = new InputFrame { yaw = standYaw * Math.PI / 180, pitch = 0.05 };
            g.forcedCombat = new CombatInput { weapon = gSlot };
            Vec3 before = sim.mover.position;
            // the campaign begins the fight on its own when it enters x9 (DOORFREE); a survival run begins it here
            for (int k = 0; k < 60 && (sim.brawl40 == null || !sim.brawl40.Alive) && campaign; k++) yield return null;
            g.forcedCombat = IdleCombat;
            string roomNow = sim.campaign?.Room?.id ?? "none";
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, map {g.MapId}, room {roomNow}, arena {(levelArena ? "the level's" : "placed at 3000,0,0")}, n40.brawl40 {rowWas} (forced on: {sim.n40.brawl40}), tells36 {sim.n36.tells36}, pilot {(casual ? "casual" : "skilled")}, view {(Brawl40View.Current != null ? "on" : "MISSING (the lead's two lines in SurvivalGame.Boss26.cs)")}";
            if ((sim.brawl40 == null || !sim.brawl40.Alive) && !sim.BossRoomBegin("brawl40", out Vec3 spawn, out double yawDeg))
            {
                res[0] = "RED: the fight did not begin (the campaign's x9 entry or BossRoomBegin(\"brawl40\"): the lead's Boss25 hook, the row, the arena) | " + pc;
                yield break;
            }
            double moved = Vec3.DistanceTo(before, sim.mover.position);
            Start(g, casual ? "brawl40casual" : "brawl40", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            yield return Shot29(dir, "B40-arena", st);
            Brawl40 b = sim.brawl40;
            var pilot = new Brawl40Pilot { hand = new Mulberry32(40), style = casual ? Brawl40Pilot.Style.Casual : Brawl40Pilot.Style.Mixed };
            double t0 = sim.SimTime, windowAt = -1, killAt = -1, ringAt = -1, stuckAt = -1, phaseAt = -1;
            int frame = 0, parryFrame = -1, lastSimStep = sim.simStep;
            bool lineShot = false, swingShot = false, stuckShot = false, ringShot = false, blockShot = false, parryShot = false, winShot = false, phaseShot = false, killShot = false, floeShot = false, floeOpenShot = false;
            var told = new HashSet<string>();
            var forcedList = new List<string> { Brawl40.Slam, Brawl40.Block, Brawl40.Rush };
            int maxOutside = 0;
            double wallEnd = Time.realtimeSinceStartup + 180;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 120)
            {
                God(g);
                frame++;
                double now = sim.SimTime - t0;
                if (b.Alive)
                {
                    // the harness's pilot on the harness's clock: one pilot step per sim step that ran since the last frame, the frame's input =
                    // the last of them with every press decided on any of its steps kept (a Player press spans every sim step of its frame: L-053)
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
                    // a tell's frame: the camera on the boss (the tell is what the frame is for)
                    if (b.state == Brawl40State.Tell && b.attack != null && b.Progress >= 0.55 && !told.Contains(b.attack))
                    {
                        Vec3 dv = b.Centre - sim.Eye;
                        mv.yaw = JsMath.Atan2(-dv.x, -dv.z);
                        mv.pitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
                    }
                    g.input.SetLook(mv.yaw, mv.pitch);
                    g.forcedMove = mv;
                    g.forcedCombat = cb;
                }
                else { g.forcedMove = new InputFrame { yaw = sim.input.Yaw, pitch = sim.input.Pitch }; g.forcedCombat = IdleCombat; }
                maxOutside = Math.Max(maxOutside, b.outside);
                // every attack once: queue the next unseen one while it walks (after the first haymaker)
                if (b.Alive && !b.ForcePending && b.state == Brawl40State.Idle && told.Contains(Brawl40.Haymaker) && forcedList.Count > 0 && now > 4)
                {
                    string f = forcedList[0];
                    forcedList.RemoveAt(0);
                    b.Force(f);
                }
                if (!lineShot && now >= 1.0) { lineShot = true; yield return Shot29(dir, "B40-line", st); continue; }
                if (b.state == Brawl40State.Tell && b.attack != null && b.Progress >= 0.7 && told.Add(b.attack)) { yield return Shot29(dir, "B40-tell-" + b.attack, st); continue; }
                if (!swingShot && b.Acting(Brawl40.Haymaker)) { swingShot = true; yield return Shot29(dir, "B40-swing", st); continue; }
                if (b.StuckNow && b.attack == Brawl40.Haymaker && stuckAt < 0) stuckAt = now;
                if (!stuckShot && stuckAt >= 0 && now - stuckAt >= 0.3 && b.StuckNow) { stuckShot = true; yield return Shot29(dir, "B40-stuck", st); continue; }
                if (b.ringOn && ringAt < 0) ringAt = now;
                if (!ringShot && ringAt >= 0 && now - ringAt >= 0.4 && b.ringOn) { ringShot = true; yield return Shot29(dir, "B40-ring", st); continue; }
                if (!blockShot && b.block.alive && !b.block.back) { blockShot = true; yield return Shot29(dir, "B40-block", st); continue; }
                if (b.blockParries > 0 && parryFrame < 0) parryFrame = frame;
                if (!parryShot && parryFrame > 0 && frame - parryFrame >= 3) { parryShot = true; yield return Shot29(dir, "B40-parry", st); continue; }
                if (b.Open && windowAt < 0) windowAt = now;
                if (!winShot && windowAt >= 0 && now - windowAt >= 0.5 && b.Open) { winShot = true; yield return Shot29(dir, "B40-window", st); continue; }
                if (!floeShot && Brawl40View.Current != null && Brawl40View.Current.FloesTold > 0) { floeShot = true; yield return Shot29(dir, "B40-floes", st); continue; }
                if (!floeOpenShot && floeShot && Brawl40View.Current != null && Brawl40View.Current.FloesOpen > 0) { floeOpenShot = true; yield return Shot29(dir, "B40-floes-open", st); continue; }
                if (b.state == Brawl40State.PhaseChange && phaseAt < 0) phaseAt = now;
                if (!phaseShot && phaseAt >= 0 && now - phaseAt >= 0.5 && b.state == Brawl40State.PhaseChange) { phaseShot = true; yield return Shot29(dir, "B40-phase", st); continue; }
                if (!b.Alive && killAt < 0) killAt = now;
                if (b.state == Brawl40State.Dying && killAt < 0) killAt = now;
                if (!killShot && killAt >= 0 && now - killAt >= 0.6) { killShot = true; yield return Shot29(dir, "B40-kill", st); break; }
                yield return null;
            }
            double fightS = (killAt >= 0 ? killAt : sim.SimTime - t0) - b.p.revealSeconds;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            var sb = new System.Text.StringBuilder();
            var seen = new HashSet<string>();
            bool tellsOk = told.Count >= 4;
            foreach (var (atk, steps) in b.tellLog)
            {
                if (!seen.Add(atk)) continue;
                double len = steps * sim.step;
                bool inBand = atk == Brawl40.Haymaker ? len >= 0.89 : atk == Brawl40.Rush ? len >= 0.44 && len <= 0.66 : len >= 0.64 && len <= 0.91;
                tellsOk &= inBand;
                sb.Append($" {atk} {len.ToString("0.00", ci)} s{(inBand ? "" : " RED")};");
            }
            // NIGHT40 lead (n40-r4 r4brawl): the reel stops 0.6 s after Dying begins (1.2 s long), so a beaten boss can still read Alive: killAt counts
            bool ok = moved < 0.05 && (!b.Alive || killAt >= 0) && tellsOk && b.windows >= 1 && maxOutside == 0;
            res[0] = $"{(ok ? "GREEN" : "RED")}: begin moved the feet {moved.ToString("0.000", ci)} m; killed {!b.Alive || killAt >= 0} in {fightS.ToString("0.0", ci)} s of fight (the band is the harness's); tells seen {told.Count}:{sb} haymakers {b.haymakers} (hit {b.hayHits}), slams {b.slams} (hit {b.slamHits}), blocks {b.blocks} (hit {b.blockHits}, parried {b.blockParries}), rushes {b.rushes} (hit {b.rushHits}, walls {b.rushWalls}), windows {b.windows}, stuck hits {b.stuckHits}, phase 2 seen {phaseShot}, floes cracked {b.floesCracked} (bites {b.floeTicks}; RED frame {floeShot}, water frame {floeOpenShot}); frames outside {maxOutside}; weapon in hand '{sim.loadout.Weapon}'; boss lines shown {sim.brawl40Line?.shown}; pilot swings {pilot.swings} (hit {pilot.swingHits}), still {pilot.stillSteps} steps; view pose at the end '{Brawl40View.Current?.Pose}', cyan seen {stuckShot || winShot} | {pc}";
        }
    }
}
