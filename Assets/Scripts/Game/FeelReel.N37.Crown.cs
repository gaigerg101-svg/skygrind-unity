// NIGHT37 builder C: the Player scenario crown37 (FeelReel.N27.cs PhaseN37C; the body is the partial N37CrownBody). Not run by its author (no
// Player in the worktree). Boot the level: -skygrindMap n37-spire with design.n37.crown37 on (the pieces need the row). Input through the game's
// forced move / combat frames and Respawn only (L-046); the player kept alive (God).
//   crown37  the spire: frames of the space, the walk-in, the tells, a pass, the CYAN window, each leg's change and the kill. The passes are
//     PUPPET passes (the player put 2.5 m before the ring on its facing, then 2.5 m past it one step later, airborne: the sim's pass test sees
//     the chest's step cross the opening): the reel shows what a pass looks like, the band is the harness's (crown37 band).
//     Frames (shot-N29-<name>.png):
//       N37C-arena-bridge   from the bridge stub (the walk-in), looking north over the chasm: the summit on the 60 m core, the three orbit rings
//                           turning under it (cyan-edged fragments), the gallery's slabs below; no Crown yet
//       N37C-arena-orbit    standing on orbit fragment orb10 (the ring at -15), looking at the core: the next fragments, the drop to -22
//       N37C-arena-gallery  on the leg-2 ledge (-27, the core's west face): the broken spiral of slabs stepping down, the cyan ride panels
//       N37C-arena-plaza    on the plaza (-60) looking up the core: the inner square, the eight outer sectors, the gallery above
//       N37C-rooms-stones / N37C-rooms-ring  room v9 from its spawn (the two stones over the pit) and v10 (the ring over the gap)
//       N37C-walkin-a / -b  0.3 s and 1.6 s after the begin: the Crown rising out of the summit, its 8 red-orange gems, the name card
//       N37C-shatter        0.5 s into the summit's RED tell: the summit slabs red and shaking
//       N37C-tell-<attack>  each attack's tell at 70 % of its wind-up (bladeRing RED: the cut's band shown at the release; gemShard YELLOW;
//                           beamCut RED: the target piece red and shaking; lowFire RED; plazaDrop RED)
//       N37C-pass           1 frame after a puppet pass: the ring flashing cyan, a gem gone
//       N37C-window         0.5 s into the CYAN window: the ring stopped, every blade and gem cyan
//       N37C-leg2 / -leg3   0.8 s into each leg change: the orbits falling away (leg 2) / the gallery falling (leg 3), the Crown lower
//       N37C-kill           0.5 s after leg 3's finisher (a KESTREL charge through the opening): the ring sinking, dying
//     Line: GREEN when the begin kept the feet (< 0.05 m), the pieces were drawn, the summit shattered, three or more tells were seen at 1.0-1.5 s,
//     a pass flash and the window were drawn, legs 2 and 3 came, and the Crown died; RED otherwise, with the measures.
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
        static partial void N37CrownBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "crown37") body = Crown37Scenario(g, dir, res, offs, pre);
        }

        static void Look37(SurvivalGame g, Vec3 feet, Vec3 at)
        {
            var (yaw, pitch) = Aim36(new Vec3(feet.x, feet.y + 1.6, feet.z), at);
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
        }

        static IEnumerator Stand37(SurvivalGame g, Vec3 feet, Vec3 at, int frames)
        {
            g.Respawn(feet, 0);
            Look37(g, feet, at);
            yield return Frames(frames);
        }

        static IEnumerator Crown37Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            // round 1 (the lead: "the reel runs in SURVIVAL so ROUND 1 IN 73 shows"): a CAMPAIGN run when the map is a campaign level (as level36's reel),
            // DOORFREE: entering THE SPIRE's room makes the campaign begin the fight itself (BeginBoss: the walk-in, no Respawn)
            bool campaign = N24CampaignGate(g) == null;
            if (campaign)
            {
                offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(30);
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                DoorFree41(g); // NIGHT41 T2 (A9): DOORFREE + its queued room banners cleared (FeelReel.N41.cs)
                campaign = sim.campaignOn && sim.campaign != null;
            }
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, crown37 row {sim.n37.crown37}, pieces {sim.orbit37?.Count ?? -1}, spire row {sim.spire37Row != null}";
            if (sim.orbit37 == null || sim.spire37Row == null)
            {
                res[0] = "RED: no spire (boot -skygrindMap n37-spire with design.n37.crown37 on) | " + pc;
                yield break;
            }
            Vec3 ax = sim.spire37Axis;
            Vec3 stub = sim.spire37Checkpoint[0], ledge = sim.spire37Checkpoint[1], plaza = sim.spire37Checkpoint[2];
            var st = new double[4];
            Start(g, "crown37", "camera", dir);
            yield return Frames(pre);
            yield return Stand37(g, new Vec3(-10, 0, -171.5), new Vec3(-10, -1, -197), 30);
            yield return Shot29(dir, "N37C-rooms-stones", st);
            yield return Stand37(g, new Vec3(-10, 0, -218), new Vec3(-10, 1.6, -226), 30);
            yield return Shot29(dir, "N37C-rooms-ring", st);
            // ---- the space, before the fight
            yield return Stand37(g, stub, new Vec3(ax.x, -12, ax.z), 30);
            bool byCampaign = campaign && sim.crown37 != null && sim.crown37.Alive;
            if (byCampaign) sim.crown37.Frozen = true;
            yield return Shot29(dir, "N37C-arena-bridge", st);
            int orb = sim.orbit37.pieces.FindIndex(q => q.id == "orb10");
            if (orb >= 0)
            {
                Piece37 q = sim.orbit37.pieces[orb];
                yield return Stand37(g, new Vec3(q.pos.x, q.pos.y + 0.05, q.pos.z), new Vec3(ax.x, q.pos.y - 3, ax.z), 40);
                yield return Shot29(dir, "N37C-arena-orbit", st);
            }
            yield return Stand37(g, ledge, new Vec3(ax.x, -42, ax.z + 10), 30);
            yield return Shot29(dir, "N37C-arena-gallery", st);
            yield return Stand37(g, plaza, new Vec3(ax.x, -30, ax.z), 30);
            yield return Shot29(dir, "N37C-arena-plaza", st);
            // ---- the walk-in: the player on the bridge's far half (a summit piece), the begin as the campaign's would run it
            Vec3 walk = new Vec3(stub.x, stub.y + 0.02, stub.z - 9);
            yield return Stand37(g, walk, new Vec3(ax.x, 2, ax.z), 20);
            Vec3 before = sim.mover.position;
            if (byCampaign)
            {
                // the campaign's begin (at the stub) restarted at the walk point: the Crown's reveal from here
                sim.crown37.Frozen = false;
                sim.crown37.Clear();
                sim.bossKit.Clear();
            }
            if (!sim.BossRoomBegin("crown37", out Vec3 spawn, out double yawDeg))
            {
                yield return Stop();
                res[0] = "RED: BossRoomBegin(crown37) returned false | " + pc;
                yield break;
            }
            yield return null;
            double moved = Vec3.DistanceTo(before, sim.mover.position);
            Crown37 c = sim.crown37;
            double t0 = sim.SimTime;
            bool wa = false, wb = false, shat = false, passShot = false, winShot = false, leg2Shot = false, leg3Shot = false, killShot = false, finisher = false;
            int passesDone = 0, legSeen = 1, piecesMax = 0;
            double legAt = 0, killAt = -1, nextPass = 6;
            var told = new HashSet<string>();
            double wallEnd = Time.realtimeSinceStartup + 150;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 90)
            {
                God(g);
                double now = sim.SimTime - t0;
                piecesMax = Math.Max(piecesMax, Crown37View.PiecesShown);
                // look at the Crown (the player keeps his footing: a fall is the fall rule's)
                Look37(g, sim.mover.position, c.centre);
                if (!wa && now >= 0.3) { wa = true; yield return Shot29(dir, "N37C-walkin-a", st); continue; }
                if (!wb && now >= 1.6) { wb = true; yield return Shot29(dir, "N37C-walkin-b", st); continue; }
                if (!shat && Crown37View.TellsShown > 0 && c.brain.state != Boss37State.Reveal && now >= c.p.revealSeconds + 0.5) { shat = true; yield return Shot29(dir, "N37C-shatter", st); continue; }
                Boss37Brain b = c.brain;
                if (b.state == Boss37State.Tell && b.Current != null && b.Progress >= 0.7 && told.Add(b.Current.id)) { yield return Shot29(dir, "N37C-tell-" + b.Current.id, st); continue; }
                if (c.Leg > legSeen)
                {
                    legSeen = c.Leg;
                    legAt = now;
                    // the player to the new leg's checkpoint (as a fall would put him)
                    g.Respawn(sim.spire37Checkpoint[legSeen - 1], 0);
                }
                if (legSeen == 2 && !leg2Shot && now - legAt >= 0.8) { leg2Shot = true; yield return Shot29(dir, "N37C-leg2", st); continue; }
                if (legSeen == 3 && !leg3Shot && now - legAt >= 0.8) { leg3Shot = true; yield return Shot29(dir, "N37C-leg3", st); continue; }
                if (!winShot && c.Open && b.stateSteps * sim.step >= 0.5) { winShot = true; yield return Shot29(dir, "N37C-window", st); continue; }
                // a puppet pass every 2.5 s of fight once three tells were seen (or 25 s in): 2.5 m before the ring on its facing, then 2.5 m past
                bool fighting = b.Fighting && !c.Open && b.state != Boss37State.LegChange;
                if (c.Alive && !c.Dying && fighting && now >= nextPass && (told.Count >= 3 || now > 25) && c.Leg < 3)
                {
                    nextPass = now + 2.5;
                    // a jump-dash's flight through the opening: 1.5 m before it on its facing, at the jump-dash's 24 m/s (the pass needs crown.passSpeed)
                    Vec3 n = c.normal, ctr = c.centre;
                    int passes0 = c.passes;
                    g.Respawn(new Vec3(ctr.x + n.x * 1.5, ctr.y + n.y * 1.5 - 1.0, ctr.z + n.z * 1.5), 0);
                    sim.mover.velocity = new Vec3(-n.x * 24, -n.y * 24, -n.z * 24);
                    for (int k = 0; k < 10 && c.passes == passes0; k++) yield return null;
                    passesDone++;
                    if (!passShot && c.passes > passes0) { passShot = true; Look37(g, sim.mover.position, ctr); yield return Shot29(dir, "N37C-pass", st); }
                    g.Respawn(sim.spire37Checkpoint[Math.Max(0, c.Leg - 1)], 0);
                    continue;
                }
                // leg 3: the finisher (a KESTREL charge through the opening) once the plaza frame is in
                if (c.Leg >= 3 && leg3Shot && !finisher && fighting)
                {
                    finisher = true;
                    Vec3 eye = sim.Eye, d = Vec3.Normalize(c.centre - eye);
                    c.Raycast(eye, d, out bool through);
                    sim.loadout.Damage(c.body, Math.Max(c.p.chargeMin, 100), false, "revolver", c.centre);
                }
                if ((c.Dying || !c.Alive) && !killShot)
                {
                    if (killAt < 0) killAt = now;
                    if (now - killAt >= 0.5 || (c.Dying && b.stateSteps * sim.step >= 0.5)) { killShot = true; yield return Shot29(dir, "N37C-kill", st); continue; }
                }
                if (killShot) break;
                yield return null;
            }
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            var sb = new System.Text.StringBuilder();
            bool tellsOk = told.Count >= 3;
            var seen = new Dictionary<string, double>();
            foreach (var (a, steps) in c.brain.tellLog) if (!seen.ContainsKey(a)) seen[a] = steps * sim.step;
            foreach (var kv in seen)
            {
                bool inBand = kv.Value >= 0.99 && kv.Value <= 1.51;
                tellsOk &= inBand;
                sb.Append($" {kv.Key} {kv.Value.ToString("0.00", ci)} s{(inBand ? "" : " RED")};");
            }
            bool ok = moved < 0.05 && piecesMax > 0 && shat && tellsOk && passShot && winShot && leg2Shot && leg3Shot && killShot;
            res[0] = $"{(ok ? "GREEN" : "RED")}: begin moved the feet {moved.ToString("0.000", ci)} m, pieces drawn (max) {piecesMax}, shatter frame {shat}, tells seen {told.Count}:{sb} puppet passes {passesDone} (crown passes {c.passes}), pass flash frame {passShot}, window frame {winShot} (windows {c.brain.windows}), legs 2 / 3 frames {leg2Shot} / {leg3Shot}, finisher {c.finishers}, kill frame {killShot}, gems left {c.GemsLeft}, falls {sim.crown37Falls}, hits taken: blade {c.bladeHits} shard {c.shardHits} low {c.lowHits}, cuts {c.cuts}, habit drops {c.habitDrops} | {pc}";
        }
    }
}
