// NIGHT31 builder B: the Player scenarios of the NIGHT31 bosses (the lead's FeelReel.N27.cs dispatches PhaseN31B to N31BossesBody). Real inputs only: the pilots are stepped once per
// SIM step and go in through g.forcedMove / g.forcedCombat / g.input.SetLook (PilotDrive27); nothing sets boss or player state the sim does not see (NIGHT30's code-set rope was
// invisible to the boss).
//   camp31    THE STAG's camp test in the Player (design.n31.stag31): five MORTAL campers (centre, bank top, island top, half a lap away, the wall corner), each in a fresh fight, hold
//             their spot and fire their best weapon (CampPilot31) for 30 s of game time after the intro. One table line per camper (alive, health lost, damage rate) and one frame each
//             at 8 s. PASS: every camper dies or loses 50 % in 30 s (the rate's second condition is the harness's: `camp31 stag` prints the pilot's rate).
//   stag31    THE STAG fought by StagPilot31 (mortal; a death restarts the room as Campaign24 does) until the kill or 180 s: frames of the tell (and how many drew the red-orange line),
//             of the open back (and the cyan plate), the crystals drawn, the stomps the view saw, the melt (and the red-orange banks). Frames: the tell, the open back, the melted rink.
//   lantern31 THE LANTERN ridden with the charge held through rides: breaks, sparks, open-core frames, the sink tell (and its red-orange edge drawn), the sunk platforms.
//   wyrm31    THE KILN WYRM fought by WyrmPilot31: feints, staggers, sweeps (jumped), spits, the crumble tell (and its cracks drawn), fallen boxes drawn (want 0).
//   loom31    (n30-span) every joint in sight from a deck edge, then a mortal deck camper 20 s: the flyer drop tell (and its mark drawn), drops, health lost, the bar it moved.
// Boss HP is only READ. Shots go to <dir>/shot-N31-*.png (Shot29's sampled numbers). Every result line starts with PASS or RED. Not run by its author (no Player in the worktree).
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
        static partial void N31BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "camp31") body = Camp31Scenario(g, dir, res, offs, pre);
            else if (s == "stag31") body = Stag31Scenario(g, dir, res, offs, pre);
            else if (s == "lantern31") body = Lantern31Scenario(g, dir, res, offs, pre);
            else if (s == "wyrm31") body = Wyrm31Scenario(g, dir, res, offs, pre);
            else if (s == "loom31") body = Loom31Scenario(g, dir, res, offs, pre);
            else body = NotBuilt31(s, res);
        }

        static IEnumerator NotBuilt31(string s, string[] res)
        {
            res[0] = $"RED: {s} is not written in this round (NIGHT31 builder B: its rules are proved in the harness, camp31 {s})";
            yield break;
        }

        static IEnumerator Shot31(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N31-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }

        /// <summary>A fresh Stag fight (a live one is cleared first): false when the row is off or the room does not begin.</summary>
        static bool Stag31Fresh(SurvivalGame g, out Vec3 spawn, out double yawDeg)
        {
            SurvivalSim sim = g.sim;
            if (sim.stag31 != null && sim.stag31.Alive) sim.stag31.Clear();
            g.health.Reset();
            sim.downSteps = 0;
            sim.flow = Flow.Playing;
            bool ok = sim.BossRoomBegin("glacier", out spawn, out yawDeg) && sim.stag31 != null && sim.stag31.Alive;
            if (ok) g.Respawn(spawn, yawDeg);
            return ok;
        }

        // ======================================================= camp31: THE STAG's campers =======================================================

        static IEnumerator Camp31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.n31.stag31) { res[0] = "RED: design.n31.stag31 is off in this run (no Stag to camp) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!Stag31Fresh(g, out Vec3 spawn, out double yawDeg)) { res[0] = "RED: BossRoomBegin(glacier) did not start THE STAG | " + pc; yield break; }
            pc = Preconditions(g, spawn);
            Rink28Arena a = sim.stag31Rink;
            double s0 = a.p.playerStartS;
            Vec3 F(double s, double lat) { Vec3 v = a.FloorPoint(s, lat); return new Vec3(v.x, v.y + 0.1, v.z); }
            var spots = new List<(string, Vec3)>
            {
                ("centre (lane middle)", F(s0 + 10, (a.latIsland + a.latLaneOut) / 2)),
                ("edge (bank top)", F(s0 + 10, a.latBankTop - 0.6)),
                ("highest (the island top)", new Vec3(a.p.cx + 0.5, a.islandTopY + 0.1, a.p.cz)),
                ("farthest (bank top, half a lap)", F(s0 + 10 + a.Lap / 2, a.latBankTop - 0.6)),
                ("wall corner (lip, straight meets curve)", F(a.p.straight / 2, a.latLip - 0.4)),
            };
            yield return Frames(20);
            Start(g, "camp31", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 600;
            var shots = new List<string>();
            var st = new double[4];
            var rows = new List<string>();
            double hpMax0 = g.health.max;
            int lose = 0, ran = 0;
            for (int i = 0; i < spots.Count && Time.realtimeSinceStartup < wallEnd; i++)
            {
                var (name, at) = spots[i];
                if (i > 0 && !Stag31Fresh(g, out spawn, out yawDeg)) { rows.Add($"{name}: the fight did not begin"); continue; }
                Stag31 stg = sim.stag31;
                // the intro in god mode, standing on the spot
                g.Respawn(at, yawDeg);
                int i0 = sim.simStep;
                while (stg.Alive && !stg.Hurtable && (sim.simStep - i0) * sim.step < 20.0) { God(g); g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; yield return null; }
                g.health.max = hpMax0; // god mode set it to 1e6 for the intro
                g.health.Reset();
                g.Respawn(at, yawDeg);
                var camper = new CampPilot31();
                var drive = new PilotDrive27 { tick = sim.simStep };
                double maxHp = g.health.max, prev = g.health.hp, lost = 0, bar0 = stg.HpFrac;
                int c0 = sim.simStep;
                bool died = false, shot = false;
                var used = new HashSet<string>();
                while (stg.Alive && (sim.simStep - c0) * sim.step < 30.0 && Time.realtimeSinceStartup < wallEnd)
                {
                    drive.Advance(sim.simStep, t =>
                    {
                        camper.Step(sim, at, stg.box.c, stg.Raycast, 2, t, true);
                        return (camper.Move, camper.Combat);
                    });
                    g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                    g.forcedMove = drive.mv;
                    g.forcedCombat = drive.cb;
                    if (camper.see) used.Add(camper.gun);
                    lost += Math.Max(0, prev - g.health.hp);
                    prev = g.health.hp;
                    if (!shot && (sim.simStep - c0) * sim.step >= 8.0) { shot = true; yield return Shot31(dir, "camp-" + i, st, shots, ci); }
                    if (g.health.Dead || sim.flow == Flow.Down) { died = true; break; }
                    yield return null;
                }
                double alive = (sim.simStep - c0) * sim.step, rate = alive > 0 ? (bar0 - stg.HpFrac) / alive : 0;
                bool loses = died || lost >= 0.5 * maxHp;
                if (loses) lose++;
                ran++;
                Vec3 endAt = sim.mover.position;
                rows.Add($"{name}: {(died ? "DIED " : "")}{alive.ToString("0.0", ci)} s, lost {(100 * lost / maxHp).ToString("0", ci)} % (max hp {maxHp.ToString("0", ci)}), {(100 * rate).ToString("0.00", ci)} %/s, weapons {string.Join("+", used)}, stag: charges {stg.charges} (hit {stg.chargeHits}), stomps {stg.stomps} ({stg.stompHits} hit), shards {stg.shardVolleys} ({stg.shardHits} hit), act {stg.act}; camper ended {JsMath.Hypot(endAt.x - at.x, endAt.z - at.z).ToString("0.0", ci)} m from its spot, dy {(endAt.y - at.y).ToString("0.0", ci)} m, {(loses ? "dies / 50 %" : "SAFE")}");
                g.health.Reset();
                sim.downSteps = 0;
                sim.flow = Flow.Playing;
            }
            bool pass = ran == spots.Count && lose == ran;
            res[0] = $"{(pass ? "PASS" : "RED")}: camp31 THE STAG, {lose} of {ran} campers die or lose 50 % in 30 s (the rate's R2 half: harness camp31 stag) | {string.Join(" ; ", rows)} | frames {string.Join(", ", shots)} | " + pc;
        }

        // ======================================================= lantern31: the shield and the sink =======================================================

        /// <summary>lantern31: LanternPilot30 rides THE LANTERN RING (design.n31.lantern31 and design.n31.charge on) and HOLDS the trigger through each ride while the core is shut
        /// (the charge fills and auto-releases: the shield breaks only to a charged shot from the wall); until the kill or 200 s. The line counts the breaks, the sparks, the open-core
        /// frames, the sink tell frames (and how many drew the red-orange platform edge), the platforms drawn by Boss31View and the sunk state. Frames: a spark or break, the sink tell.</summary>
        static IEnumerator Lantern31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Lantern30On || !sim.n31.lantern31) { res[0] = $"RED: the NIGHT31 Lantern is off in this run (n30 ring {sim.Lantern30On}, n31.lantern31 {sim.n31.lantern31}) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yawDeg) || sim.lantern27 == null || !sim.lantern27.Alive) { res[0] = "RED: BossRoomBegin(hulk) did not start the lantern | " + pc; yield break; }
            Lantern27 l = sim.lantern27;
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn) + $" | n31.charge {sim.n31.charge}";
            int breaks = 0, sparks = 0;
            offs.Add(g.events.On<Lantern31Event>("lantern31", e => { if (e.stage == "break") breaks++; else if (e.stage == "spark") sparks++; }));
            yield return Frames(20);
            Start(g, "lantern31", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 360;
            var shots = new List<string>();
            var st = new double[4];
            var pilot = new LanternPilot30 { hand = new Mulberry32(Seed * 7919u + 13u) };
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = yawDeg * Math.PI / 180;
            bool shotBreak = false, shotSink = false;
            var charge31 = new LanternCharge31();
            int chargedFired = 0, chargedHitsEv = 0;
            offs.Add(g.events.On<ChargeShot27Event>("chargeShot27Fire", e => { chargedFired++; chargedHitsEv += e.hits; }));
            int step0 = sim.simStep, deaths = 0, frames = 0, openF = 0, tellF = 0, edgeF = 0, platF = 0;
            Boss31View v = Boss31View.Current;
            while (l.Alive && (sim.simStep - step0) * sim.step < 200.0 && Time.realtimeSinceStartup < wallEnd)
            {
                frames++;
                if (Down27(g, "hulk")) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t =>
                {
                    pilot.Step(sim, yawL, t);
                    yawL = pilot.Move.yaw;
                    CombatInput cb = pilot.Combat;

                    return (pilot.Move, cb);
                });
                // ONCE per frame, after the catch-up (the frame's steps see one sim state: a release decided on the first step would be re-pressed by the second)
                drive.cb = charge31.Fire(sim, drive.mv, drive.cb, sim.simStep);
                g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                g.forcedMove = drive.mv;
                g.forcedCombat = drive.cb;
                Lantern31 r = sim.lantern31;
                if (r != null)
                {
                    if (r.CoreOpen) openF++;
                    if (r.sinkState == 1) { tellF++; if (v != null && v.PlatformSinkTellShown) edgeF++; }
                    if (v != null && v.PlatformsShown) platF++;
                    if (!shotBreak && r.CoreOpen && r.CoreOpenProgress > 0.1) { shotBreak = true; yield return Shot31(dir, "lantern-open", st, shots, ci); }
                    if (!shotSink && r.sinkState == 1 && r.SinkTellProgress > 0.5) { shotSink = true; yield return Shot31(dir, "lantern-sink", st, shots, ci); }
                }
                yield return null;
            }
            bool killed = !l.Alive || l.phase == "dying";
            double killS = (sim.simStep - step0) * sim.step;
            Lantern31 lr = sim.lantern31;
            bool pass = killed && breaks > 0 && tellF > 0 && edgeF == tellF && lr != null && lr.sinkState == 3;
            res[0] = $"{(pass ? "PASS" : "RED")}: lantern31 {(killed ? "KILLED at " + killS.ToString("0.0", ci) + " s" : "NOT killed in " + killS.ToString("0", ci) + " s (bar " + l.HpFrac.ToString("0.00", ci) + ")")} (harness 137.8 s), deaths {deaths}; shield breaks {breaks}, sparks {sparks}, open-core frames {openF}; sink tell frames {tellF} (red-orange edge {edgeF}), platforms drawn {platF}/{frames} frames, sunk {(lr != null && lr.sinkState == 3 ? "yes" : "no")}; hits open {lr?.openHits} / light {lr?.lightHits}; charged shots fired {chargedFired} ({chargedHitsEv} bodies hit), charged hits on the core {lr?.chargedSeen} (rejected: not from the wall {lr?.rejectNotWall}, on a platform {lr?.rejectPlatform}); rider holds {charge31.holds}, releases on aim {charge31.releases} | frames {string.Join(", ", shots)} | " + pc;
        }

        // ======================================================= wyrm31: feints, the stagger window, the crumble =======================================================

        /// <summary>wyrm31: THE KILN WYRM (design.n31.wyrm31) fought by WyrmPilot31 (mortal; a death restarts the room) until the kill or 240 s of game time: feints and real
        /// charges, staggers, sweeps (hit or jumped), spits, the crumble tell frames (and how many drew the red-orange cracks), crumble boxes drawn after the fall (must be 0).</summary>
        static IEnumerator Wyrm31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.n31.wyrm31) { res[0] = "RED: design.n31.wyrm31 is off in this run (NIGHT30's worm) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.BossRoomBegin("wyrm", out Vec3 spawn, out double yawDeg) || sim.wyrm24 == null || !sim.wyrm24.Alive) { res[0] = "RED: BossRoomBegin(wyrm) did not start the worm | " + pc; yield break; }
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            foreach (string gun in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
            int feints = 0, sweeps = 0, spits = 0, crumbles = 0;
            offs.Add(g.events.On<Wyrm31Event>("wyrm31", e => { if (e.stage == "feint") feints++; else if (e.stage == "sweep") sweeps++; else if (e.stage == "spit") spits++; else if (e.stage == "crumble") crumbles++; }));
            yield return Frames(20);
            Start(g, "wyrm31", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 420;
            var shots = new List<string>();
            var st = new double[4];
            Wyrm w = sim.wyrm24;
            var pilot = new WyrmPilot31();
            var drive = new PilotDrive27 { tick = sim.simStep };
            Boss31View v = Boss31View.Current;
            int step0 = sim.simStep, deaths = 0, frames = 0, tellF = 0, crackF = 0, fallenDrawn = 0, staggers0 = sim.wyrm30Staggers;
            bool shotCrack = false, shotFeint = false;
            while (w.Alive && (sim.simStep - step0) * sim.step < 240.0 && Time.realtimeSinceStartup < wallEnd)
            {
                frames++;
                if (Down27(g, "wyrm")) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t => { pilot.Step(sim, t); return (pilot.Move, pilot.Combat); });
                g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                g.forcedMove = drive.mv;
                g.forcedCombat = drive.cb;
                if (sim.wyrm31CrumbleState == 1) { tellF++; if (v != null && v.CracksShown) crackF++; }
                if (sim.wyrm31CrumbleState == 2 && v != null && v.CrumbleBoxesShown > 0) fallenDrawn++;
                if (!shotCrack && sim.wyrm31CrumbleState == 1 && sim.Wyrm31CrumbleProgress > 0.5) { shotCrack = true; yield return Shot31(dir, "wyrm-crumble", st, shots, ci); }
                if (!shotFeint && w.PatternName == "feint31") { shotFeint = true; yield return Shot31(dir, "wyrm-feint", st, shots, ci); }
                yield return null;
            }
            bool killed = !w.Alive || w.phase == "dying";
            double killS = (sim.simStep - step0) * sim.step;
            bool pass = feints > 0 && sim.wyrm30Staggers > staggers0 && (tellF == 0 || crackF == tellF) && fallenDrawn == 0 && (crumbles > 0 || w.HpFrac > 0.5);
            res[0] = $"{(pass ? "PASS" : "RED")}: wyrm31 {(killed ? "KILLED at " + killS.ToString("0.0", ci) + " s" : "NOT killed in " + killS.ToString("0", ci) + " s (bar " + w.HpFrac.ToString("0.00", ci) + ")")}, deaths {deaths}; feints {feints}, staggers {sim.wyrm30Staggers - staggers0}, sweeps {sweeps} (jumps {pilot.hops}), spits {spits}, crumbles {crumbles}; frames {frames}: crumble tell {tellF} (cracks drawn {crackF}), fallen boxes still drawn {fallenDrawn} (want 0; WyrmRoomView must leave them out: lead line); full hits {sim.wyrm31?.fullHits}, chip hits {sim.wyrm31?.gunHits} | frames {string.Join(", ", shots)} | " + pc;
        }

        // ======================================================= loom31: sight, the drop, the deck =======================================================

        /// <summary>loom31 (boot -skygrindMap n30-span; the loom30 path: a CAMPAIGN run, DOORFREE, a respawn into s8): (1) from each deck edge's middle, every joint's line of sight
        /// through the level collider (G's pick31 note: under the deck the hook took a surface); (2) MORTAL, the player stands still on the deck firing the NAILGUN at the nearest
        /// joint for 20 s: the flyer drop's tell frames (and the red-orange mark drawn), drops and hits, the sweep, the bar's drop per second (a deck camper: 12 %). Frames: the
        /// joints from the deck edge, the drop mark.</summary>
        static IEnumerator Loom31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            double hpMax0 = g.health.max;
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (boot -skygrindMap n30-span) | " + pc; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            pc = Preconditions(g, g.Feet) + $" | level {sim.campaignLevel.id}, n30.span {sim.n30.span}, n31.loom31 {sim.n31.loom31}";
            if (!sim.n30.span || !sim.n31.loom31) { res[0] = "RED: design.n30.span or design.n31.loom31 is off in this run | " + pc; yield break; }
            RoomDef bossRoom = sim.campaignLevel.rooms.Find(r => r.kind == "boss" && r.boss == "loom");
            if (bossRoom == null) { res[0] = $"RED: level {sim.campaignLevel.id} has no boss room with boss 'loom' | " + pc; yield break; }
            g.husks.frozen = true;
            int drops = 0, dropTells = 0;
            offs.Add(g.events.On<Loom31Event>("loom31", e => { if (e.stage == "drop") drops++; else if (e.stage == "dropTell") dropTells++; }));
            Start(g, "loom31", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            g.Respawn(bossRoom.spawn, bossRoom.spawnYawDeg);
            double wallEnd = Time.realtimeSinceStartup + 120;
            int rise = 0;
            while ((sim.loom30 == null || !sim.loom30.Alive || !sim.loom30.Hurtable) && rise < 1200 && Time.realtimeSinceStartup < wallEnd) { God(g); g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; rise++; yield return null; }
            if (sim.loom30 == null || !sim.loom30.Hurtable) { res[0] = $"RED: the Loom did not rise into its fight ({rise} frames) | " + pc; yield break; }
            Loom30 l = sim.loom30;
            var shots = new List<string>();
            var st = new double[4];
            // (1) sight from the deck edges
            int seen = 0;
            var vis = new bool[l.joints.Length];
            for (int e = 0; e < 4; e++)
            {
                Vec3 g0 = l.grips[e], g1 = l.grips[(e + 1) % 4];
                Vec3 mid = new Vec3((g0.x + g1.x) / 2, g0.y, (g0.z + g1.z) / 2);
                Vec3 inward = Vec3.Normalize(new Vec3(l.centre.x - mid.x, 0, l.centre.z - mid.z));
                Vec3 eye = new Vec3(mid.x + inward.x, mid.y + 1.6, mid.z + inward.z);
                for (int j = 0; j < l.joints.Length; j++)
                {
                    Vec3 d = l.joints[j] - eye;
                    double len = d.Length;
                    if (!(sim.world.RayIntersect(eye, d / len, out RayHit h) && h.distance < len - 0.3)) vis[j] = true;
                }
                if (e == 0)
                {
                    g.Respawn(new Vec3(mid.x + inward.x, mid.y + 0.05, mid.z + inward.z), bossRoom.spawnYawDeg);
                    Vec3 dj = l.joints[1] - eye;
                    g.input.SetLook(JsMath.Atan2(-dj.x, -dj.z), JsMath.Atan2(dj.y, JsMath.Hypot(dj.x, dj.z)));
                    for (int i = 0; i < Ms(400); i++) { God(g); yield return null; }
                    yield return Shot31(dir, "loom-joints", st, shots, ci);
                }
            }
            foreach (bool b in vis) if (b) seen++;
            // (2) the deck camper, mortal, 20 s
            g.health.max = hpMax0;
            g.health.Reset();
            Vec3 spot = new Vec3(l.centre.x + 1, l.centre.y + 0.05, l.centre.z + 1);
            g.Respawn(spot, bossRoom.spawnYawDeg);
            if (!sim.loadout.owned.Contains("nailgun")) sim.loadout.AddWeapon("nailgun");
            var camper = new CampPilot31();
            var drive = new PilotDrive27 { tick = sim.simStep };
            Boss31View v = Boss31View.Current;
            double bar0 = l.HpFrac, lost = 0, prev = g.health.hp;
            int c0 = sim.simStep, tellF = 0, markF = 0;
            bool died = false, shotDrop = false;
            while (l.Alive && (sim.simStep - c0) * sim.step < 20.0 && Time.realtimeSinceStartup < wallEnd)
            {
                drive.Advance(sim.simStep, t =>
                {
                    Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
                    int nj = 0;
                    double bd = 1e9;
                    for (int j = 0; j < l.joints.Length; j++) { double dd = Vec3.DistanceTo(eye, l.joints[j]); if (dd < bd) { bd = dd; nj = j; } }
                    camper.Step(sim, spot, l.joints[nj], l.Raycast, 4, t, true);
                    return (camper.Move, camper.Combat);
                });
                g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                g.forcedMove = drive.mv;
                g.forcedCombat = drive.cb;
                Loom31 r = sim.loom31;
                if (r != null && r.DropTelling) { tellF++; if (v != null && v.LoomDropShown) markF++; if (!shotDrop && r.DropTellProgress > 0.5) { shotDrop = true; yield return Shot31(dir, "loom-drop", st, shots, ci); } }
                lost += Math.Max(0, prev - g.health.hp);
                prev = g.health.hp;
                if (g.health.Dead || sim.flow == Flow.Down) { died = true; break; }
                yield return null;
            }
            double secs = (sim.simStep - c0) * sim.step, rate = secs > 0 ? (bar0 - l.HpFrac) / secs : 0;
            g.health.Reset();
            sim.downSteps = 0;
            sim.flow = Flow.Playing;
            bool pass = seen == l.joints.Length && dropTells > 0 && markF == tellF && (died || lost >= 0.5 * hpMax0);
            res[0] = $"{(pass ? "PASS" : "RED")}: loom31 joints in sight from a deck edge {seen}/{l.joints.Length} (hips {(l.joints[1].y - l.centre.y).ToString("0.0", ci)} m from the deck); deck camper {(died ? "DIED at " : "alive ")}{secs.ToString("0.0", ci)} s, lost {(100 * lost / hpMax0).ToString("0", ci)} %, dealt {(100 * rate).ToString("0.00", ci)} %/s (harness lag bot 0.95 %/s), drop tells {dropTells} (frames {tellF}, mark drawn {markF}), drops {drops}, hits by the drop {sim.loom31?.dropHits}, sweeps {l.sweeps} | frames {string.Join(", ", shots)} | " + pc;
        }

        // ======================================================= stag31: THE STAG fought =======================================================

        static IEnumerator Stag31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.n31.stag31) { res[0] = "RED: design.n31.stag31 is off in this run (level 2's boss is the Glacier) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!Stag31Fresh(g, out Vec3 spawn, out double yawDeg)) { res[0] = "RED: BossRoomBegin(glacier) did not start THE STAG | " + pc; yield break; }
            pc = Preconditions(g, spawn);
            if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
            yield return Frames(20);
            Start(g, "stag31", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 420;
            var shots = new List<string>();
            var st = new double[4];
            Stag31 stg = sim.stag31;
            var pilot = new StagPilot31();
            var drive = new PilotDrive27 { tick = sim.simStep };
            Boss31View v = Boss31View.Current;
            int step0 = sim.simStep, deaths = 0, frames = 0, tellF = 0, lineF = 0, backF = 0, plateF = 0, crystalF = 0, meltF = 0, waterF = 0, stomp0 = v != null ? v.stomps : 0;
            bool shotTell = false, shotBack = false, shotMelt = false;
            while (stg.Alive && (sim.simStep - step0) * sim.step < 180.0 && Time.realtimeSinceStartup < wallEnd)
            {
                frames++;
                if (Down27(g, "glacier")) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t => { pilot.Step(sim, t); return (pilot.Move, pilot.Combat); });
                g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                g.forcedMove = drive.mv;
                g.forcedCombat = drive.cb;
                if (stg.Telling) { tellF++; if (v != null && v.StagLineShown) lineF++; }
                if (stg.BackOpen) { backF++; if (v != null && v.StagBackShown) plateF++; }
                if (v != null && v.StagCrystalsShown > 0) crystalF++;
                if (stg.melted) { meltF++; if (v != null && v.StagWaterShown) waterF++; }
                if (!shotTell && stg.Telling && stg.TellProgress > 0.5) { shotTell = true; yield return Shot31(dir, "stag-tell", st, shots, ci); }
                if (!shotBack && stg.BackOpen && stg.actSteps > 30) { shotBack = true; yield return Shot31(dir, "stag-back", st, shots, ci); }
                if (!shotMelt && stg.melted && meltF > 60) { shotMelt = true; yield return Shot31(dir, "stag-melt", st, shots, ci); }
                yield return null;
            }
            bool killed = !stg.Alive || stg.phase == "dying";
            double killS = (sim.simStep - step0) * sim.step;
            bool drawn = tellF > 0 && lineF == tellF && backF > 0 && plateF == backF && (meltF == 0 || waterF == meltF);
            bool pass = killed && drawn;
            res[0] = $"{(pass ? "PASS" : "RED")}: stag31 {(killed ? "KILLED at " + killS.ToString("0.0", ci) + " s" : "NOT killed in " + killS.ToString("0", ci) + " s (bar " + stg.HpFrac.ToString("0.00", ci) + ")")} (harness band 80-130 s), deaths {deaths}; charges {stg.charges} (hit {stg.chargeHits}, wall slams {stg.slams}, misses {stg.misses}, pairs {stg.pairs}), back punches {stg.backPunches} (gauntlet hits {stg.gauntletHits}: back shut {stg.punchBackShut}, not behind {stg.punchNotBehind}), stomps {stg.stomps} (view saw {(v != null ? v.stomps - stomp0 : -1)}), shards {stg.shardVolleys} ({stg.shardHits} hits), water hits {stg.lavaHits}; frames {frames}: tell {tellF} (line drawn {lineF}), back open {backF} (cyan plate {plateF}), crystals drawn {crystalF}, melted {meltF} (red-orange banks {waterF}); pilot dodges {pilot.dodges}, punches {pilot.punches} | frames {string.Join(", ", shots)} | " + pc;
        }
    }
}
