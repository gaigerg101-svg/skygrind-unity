// NIGHT32 builder B (design.n32.bosses32): the Player scenarios of card B (the lead's FeelReel.N27.cs dispatches PhaseN32B to N32BossesBody). Real inputs only: the pilots are
// stepped once per SIM step and go in through g.forcedMove / g.forcedCombat / g.input.SetLook (PilotDrive27); boss HP is only READ.
//   bosses32  Default boot: THE LANTERN (LanternPilot30 + LanternCharge31), THE STAG (StagPilot31), THE KILN WYRM (WyrmPilot31), each killed in turn (a death restarts the room
//             as Down27 does), cap 150 s each. Boot `-skygrindMap n30-span`: THE LOOM (a CAMPAIGN run, DOORFREE, the s8 fight) fought by LoomPilot32 from the deck corners (a death
//             puts the pilot back on a deck corner with full health; the Loom keeps its bar). One line per boss: kill time (band 25-40 s), deaths, hits, full-share hits,
//             mean landed share (boss32Stats), hits under 25 %; a frame at 10 s of each fight. PASS per boss: killed in at most 45 s; the scenario PASSes when every boss does.
//   camp32    Default boot: the campers of the harness's camp32 (camp31's spots) at THE LANTERN (4), THE STAG (5) and THE KILN WYRM (3: centre, corner, far corner), each in a fresh
//             fight; boot `-skygrindMap n30-span`: the Loom's 4 spots in one fight (the bar carries over between spots). Each camper has the REAL max health (design health.max:
//             NIGHT31's camp31 reel read the max after the reel's God(), 1e6, so flat shard hits showed 0 %). LOSES = dies or loses half its health in 30 s having taken under half
//             the bar; and its landed hits average at least 25 % of their damage. A frame at 8 s of each camper.
// Shots go to <dir>/shot-N29-N32-*.png (Shot29's sampled numbers). Every result line starts with PASS or RED. Not run by its author (no Player in the worktree).
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
        static partial void N32BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "bosses32") body = Bosses32Scenario(g, dir, res, offs, pre);
            else if (s == "camp32") body = Camp32Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Shot32B(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N32-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }

        static string B32Stats(SurvivalSim sim, int h0, int f0, int p0, double r0, double l0, int low0, CultureInfo ci)
        {
            Boss32Stats s = sim.boss32Stats;
            int h = s.hits - h0, f = s.fullHits - f0, p = s.paired - p0, low = s.lowHits - low0;
            double raw = s.pairedRaw - r0, landed = s.pairedLanded - l0;
            return $"hits {h}, full-share {(h > 0 ? 100.0 * f / h : 0).ToString("0", ci)} %, mean share {(raw > 0 ? 100 * landed / raw : 0).ToString("0", ci)} %, hits under 25 % {(p > 0 ? 100.0 * low / p : 0).ToString("0", ci)} %";
        }

        static void B32RealHealth(SurvivalGame g)
        {
            g.health.max = g.sim.cfg.Num("health.max");
            g.health.Reset();
        }

        // ======================================================= bosses32 =======================================================

        static IEnumerator Bosses32Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.n32.bosses32) { res[0] = "RED: design.n32.bosses32 is off in this run (-skygrindOff32 or the row) | " + pc; yield break; }
            var lines = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            bool all = true;
            offs.Add(() => Quiet(g));
            if (N24CampaignGate(g) == null)
            {
                // THE LOOM (boot -skygrindMap n30-span)
                offs.Add(() => { g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(30);
                God(g);
                if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (boot -skygrindMap n30-span) | " + pc; yield break; }
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                sim.campaign?.SetDoorFree29(true);
                RoomDef bossRoom = sim.campaignLevel.rooms.Find(r => r.kind == "boss" && r.boss == "loom");
                if (bossRoom == null || !sim.n30.span || !sim.n31.loom31) { res[0] = $"RED: no Loom here (level {sim.campaignLevel.id}, n30.span {sim.n30.span}, n31.loom31 {sim.n31.loom31}) | " + pc; yield break; }
                g.husks.frozen = true;
                foreach (string gun in new[] { "gauntlet", "shotgun", "nailgun" }) if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
                Start(g, "bosses32", "camera", dir);
                yield return Frames(pre);
                FeelProbe.MarkInput();
                g.Respawn(bossRoom.spawn, bossRoom.spawnYawDeg);
                double wallEnd = Time.realtimeSinceStartup + 300;
                int rise = 0;
                while ((sim.loom30 == null || !sim.loom30.Alive || !sim.loom30.Hurtable) && rise < 1200 && Time.realtimeSinceStartup < wallEnd) { God(g); g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; rise++; yield return null; }
                if (sim.loom30 == null || !sim.loom30.Hurtable) { res[0] = $"RED: the Loom did not rise into its fight ({rise} frames) | " + pc; yield break; }
                Loom30 l = sim.loom30;
                Vec3 g0 = l.grips[0], inw = Vec3.Normalize(new Vec3(l.centre.x - g0.x, 0, l.centre.z - g0.z));
                Vec3 corner = new Vec3(g0.x + inw.x * 1.0, g0.y + 0.05, g0.z + inw.z * 1.0);
                B32RealHealth(g);
                g.Respawn(corner, bossRoom.spawnYawDeg);
                Boss32Stats s = sim.boss32Stats;
                int h0 = s.hits, f0 = s.fullHits, p0 = s.paired, low0 = s.lowHits;
                double r0 = s.pairedRaw, l0 = s.pairedLanded;
                var pilot = new LoomPilot32();
                var drive = new PilotDrive27 { tick = sim.simStep };
                int step0 = sim.simStep, deaths = 0;
                bool shot = false;
                while (l.Alive && l.phase != "dying" && (sim.simStep - step0) * sim.step < 150.0 && Time.realtimeSinceStartup < wallEnd)
                {
                    if (g.health.Dead || sim.flow == Flow.Down) { deaths++; B32RealHealth(g); sim.downSteps = 0; sim.flow = Flow.Playing; g.Respawn(corner, bossRoom.spawnYawDeg); drive.tick = sim.simStep; }
                    drive.Advance(sim.simStep, t => { pilot.Step(sim, t); return (pilot.Move, pilot.Combat); });
                    g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                    g.forcedMove = drive.mv;
                    g.forcedCombat = drive.cb;
                    if (!shot && (sim.simStep - step0) * sim.step >= 10.0) { shot = true; yield return Shot32B(dir, "loom-fight", st, shots, ci); }
                    yield return null;
                }
                bool killed = !l.Alive || l.phase == "dying";
                double killS = (sim.simStep - step0) * sim.step;
                bool ok = killed && killS <= 45; // the band (the lead 20:10, GG 'bosses still too tanky!'): 25-40 s, at most 45
                all &= ok;
                lines.Add($"{(ok ? "PASS" : "RED")} loom {(killed ? "KILLED at " + killS.ToString("0.0", ci) + " s" : "NOT killed in " + killS.ToString("0", ci) + " s (bar " + l.HpFrac.ToString("0.00", ci) + ")")} (band 25-40 s; harness LoomPilot32 p50 29.9), deaths {deaths} (bar kept), pool {l.body.maxHp}, {B32Stats(sim, h0, f0, p0, r0, l0, low0, ci)}; joints full {l.fullHits} / light {l.lightHits}, sweeps {l.sweeps}, drops {sim.loom31?.drops}, pit {sim.loom31?.breakState}; pilot corners {pilot.switches}, hops {pilot.hops}");
                g.health.Reset();
                sim.downSteps = 0;
                sim.flow = Flow.Playing;
                res[0] = $"{(all ? "PASS" : "RED")}: bosses32 (n30-span) | {string.Join(" ; ", lines)} | frames {string.Join(", ", shots)} | " + pc;
                yield break;
            }
            g.husks.frozen = true;
            yield return Frames(20);
            Start(g, "bosses32", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wall = Time.realtimeSinceStartup + 900;
            // --- THE LANTERN ---
            if (!sim.Lantern30On || !sim.n31.lantern31) { lines.Add($"RED lantern: the NIGHT31 Lantern is off (n30 ring {sim.Lantern30On}, n31.lantern31 {sim.n31.lantern31})"); all = false; }
            else if (!sim.BossRoomBegin("hulk", out Vec3 sp, out double yd) || sim.lantern27 == null || !sim.lantern27.Alive) { lines.Add("RED lantern: BossRoomBegin(hulk) did not start it"); all = false; }
            else
            {
                Lantern27 l = sim.lantern27;
                B32RealHealth(g);
                g.Respawn(sp, yd);
                Boss32Stats s = sim.boss32Stats;
                int h0 = s.hits, f0 = s.fullHits, p0 = s.paired, low0 = s.lowHits;
                double r0 = s.pairedRaw, l0 = s.pairedLanded;
                var pilot = new LanternPilot30 { hand = new Mulberry32(Seed * 7919u + 13u) };
                var charge = new LanternCharge31();
                var drive = new PilotDrive27 { tick = sim.simStep };
                double yawL = yd * Math.PI / 180;
                int step0 = sim.simStep, deaths = 0;
                bool shot = false;
                while (l.Alive && l.phase != "dying" && (sim.simStep - step0) * sim.step < 150.0 && Time.realtimeSinceStartup < wall)
                {
                    if (Down27(g, "hulk")) { deaths++; drive.tick = sim.simStep; l = sim.lantern27; }
                    drive.Advance(sim.simStep, t => { pilot.Step(sim, yawL, t); yawL = pilot.Move.yaw; return (pilot.Move, pilot.Combat); });
                    drive.cb = charge.Fire(sim, drive.mv, drive.cb, sim.simStep);
                    g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                    g.forcedMove = drive.mv;
                    g.forcedCombat = drive.cb;
                    if (!shot && (sim.simStep - step0) * sim.step >= 10.0) { shot = true; yield return Shot32B(dir, "lantern-fight", st, shots, ci); }
                    yield return null;
                }
                bool killed = !l.Alive || l.phase == "dying";
                double killS = (sim.simStep - step0) * sim.step;
                bool ok = killed && killS <= 45; // the band (the lead 20:10, GG 'bosses still too tanky!'): 25-40 s, at most 45
                all &= ok;
                lines.Add($"{(ok ? "PASS" : "RED")} lantern {(killed ? "KILLED at " + killS.ToString("0.0", ci) + " s" : "NOT killed in " + killS.ToString("0", ci) + " s (bar " + l.HpFrac.ToString("0.00", ci) + ")")} (band 25-40 s; harness p50 29.0), deaths {deaths}, pool {l.body.maxHp}, {B32Stats(sim, h0, f0, p0, r0, l0, low0, ci)}; breaks {sim.lantern31?.breaks}");
            }
            // --- THE STAG ---
            if (!sim.n31.stag31) { lines.Add("RED stag: design.n31.stag31 is off"); all = false; }
            else if (!Stag31Fresh(g, out Vec3 sps, out double yds)) { lines.Add("RED stag: BossRoomBegin(glacier) did not start it"); all = false; }
            else
            {
                Stag31 stg = sim.stag31;
                B32RealHealth(g);
                if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
                Boss32Stats s = sim.boss32Stats;
                int h0 = s.hits, f0 = s.fullHits, p0 = s.paired, low0 = s.lowHits;
                double r0 = s.pairedRaw, l0 = s.pairedLanded;
                var pilot = new StagPilot31();
                var drive = new PilotDrive27 { tick = sim.simStep };
                int step0 = sim.simStep, deaths = 0, backF = 0, markF = 0;
                bool shot = false;
                while (stg.Alive && stg.phase != "dying" && (sim.simStep - step0) * sim.step < 150.0 && Time.realtimeSinceStartup < wall)
                {
                    if (Down27(g, "glacier")) { deaths++; drive.tick = sim.simStep; }
                    drive.Advance(sim.simStep, t => { pilot.Step(sim, t); return (pilot.Move, pilot.Combat); });
                    g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                    g.forcedMove = drive.mv;
                    g.forcedCombat = drive.cb;
                    if (stg.BackOpen) { backF++; if (BossMarks32.Current != null && BossMarks32.Current.OpenShown) markF++; }
                    if (!shot && stg.BackOpen && stg.actSteps > 30) { shot = true; yield return Shot32B(dir, "stag-back", st, shots, ci); }
                    yield return null;
                }
                bool killed = !stg.Alive || stg.phase == "dying";
                double killS = (sim.simStep - step0) * sim.step;
                bool ok = killed && killS <= 45; // the band (the lead 20:10, GG 'bosses still too tanky!'): 25-40 s, at most 45
                all &= ok;
                lines.Add($"{(ok ? "PASS" : "RED")} stag {(killed ? "KILLED at " + killS.ToString("0.0", ci) + " s" : "NOT killed in " + killS.ToString("0", ci) + " s (bar " + stg.HpFrac.ToString("0.00", ci) + ")")} (band 25-40 s; harness 27.8), deaths {deaths}, pool {stg.body.maxHp}, {B32Stats(sim, h0, f0, p0, r0, l0, low0, ci)}; back punches {stg.backPunches}, wall slams {stg.slams}, crystals {stg.crystalBreaks}; back-open frames {backF}, bossMarks open plate drawn {markF} (row bossMarks {N32Fixes.ViewRowOn(sim.cfg, "bossMarks")})");
            }
            // --- THE KILN WYRM ---
            if (!sim.n31.wyrm31) { lines.Add("RED wyrm: design.n31.wyrm31 is off"); all = false; }
            else if (!sim.BossRoomBegin("wyrm", out Vec3 spw, out double ydw) || sim.wyrm24 == null || !sim.wyrm24.Alive) { lines.Add("RED wyrm: BossRoomBegin(wyrm) did not start it"); all = false; }
            else
            {
                Wyrm w = sim.wyrm24;
                B32RealHealth(g);
                g.Respawn(spw, ydw);
                foreach (string gun in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
                Boss32Stats s = sim.boss32Stats;
                int h0 = s.hits, f0 = s.fullHits, p0 = s.paired, low0 = s.lowHits;
                double r0 = s.pairedRaw, l0 = s.pairedLanded;
                int slug0 = sim.wyrm31 != null ? sim.wyrm31.slugHits32 : 0, stag0 = sim.wyrm30Staggers;
                var pilot = new WyrmPilot31();
                var drive = new PilotDrive27 { tick = sim.simStep };
                int step0 = sim.simStep, deaths = 0;
                bool shot = false;
                while (w.Alive && w.phase != "dying" && (sim.simStep - step0) * sim.step < 150.0 && Time.realtimeSinceStartup < wall)
                {
                    if (Down27(g, "wyrm")) { deaths++; drive.tick = sim.simStep; w = sim.wyrm24; }
                    drive.Advance(sim.simStep, t => { pilot.Step(sim, t); return (pilot.Move, pilot.Combat); });
                    g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                    g.forcedMove = drive.mv;
                    g.forcedCombat = drive.cb;
                    if (!shot && (sim.simStep - step0) * sim.step >= 10.0) { shot = true; yield return Shot32B(dir, "wyrm-fight", st, shots, ci); }
                    yield return null;
                }
                bool killed = !w.Alive || w.phase == "dying";
                double killS = (sim.simStep - step0) * sim.step;
                bool ok = killed && killS <= 45; // the band (the lead 20:10, GG 'bosses still too tanky!'): 25-40 s, at most 45
                all &= ok;
                lines.Add($"{(ok ? "PASS" : "RED")} wyrm {(killed ? "KILLED at " + killS.ToString("0.0", ci) + " s" : "NOT killed in " + killS.ToString("0", ci) + " s (bar " + w.HpFrac.ToString("0.00", ci) + ")")} (band 25-40 s; harness p50 28.6), deaths {deaths}, pool {w.body.maxHp}, {B32Stats(sim, h0, f0, p0, r0, l0, low0, ci)}; staggers {sim.wyrm30Staggers - stag0}, slug hits in a stagger {(sim.wyrm31 != null ? sim.wyrm31.slugHits32 - slug0 : 0)}");
            }
            g.health.Reset();
            sim.downSteps = 0;
            sim.flow = Flow.Playing;
            res[0] = $"{(all ? "PASS" : "RED")}: bosses32 (the Loom: boot -skygrindMap n30-span) | {string.Join(" ; ", lines)} | frames {string.Join(", ", shots)} | " + pc;
        }

        // ======================================================= camp32 =======================================================

        /// <summary>One camper at `at` for 30 s of game time from the boss being hurtable (the intro in god mode on the spot), with the real max health.</summary>
        static IEnumerator Camp32One(SurvivalGame g, string boss, string name, Vec3 at, double yawDeg, int level, Func<bool> alive, Func<bool> hurtable, Func<double> hpFrac,
            Func<Vec3> centre, Func<Vec3, Vec3, double> ray, string dir, string shotName, double[] st, List<string> shots, List<string> rows, int[] tally, CultureInfo ci)
        {
            SurvivalSim sim = g.sim;
            g.Respawn(at, yawDeg);
            int i0 = sim.simStep;
            while (alive() && !hurtable() && (sim.simStep - i0) * sim.step < 20.0) { God(g); g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; yield return null; }
            B32RealHealth(g);
            g.Respawn(at, yawDeg);
            Boss32Stats s = sim.boss32Stats;
            int p0 = s.paired;
            double r0 = s.pairedRaw, l0 = s.pairedLanded;
            var camper = new CampPilot31();
            var drive = new PilotDrive27 { tick = sim.simStep };
            double maxHp = g.health.max, prev = g.health.hp, lost = 0, bar0 = hpFrac();
            int c0 = sim.simStep;
            bool died = false, shot = false;
            var used = new HashSet<string>();
            while (alive() && (sim.simStep - c0) * sim.step < 30.0)
            {
                drive.Advance(sim.simStep, t => { camper.Step(sim, at, centre(), ray, level, t, true); return (camper.Move, camper.Combat); });
                g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                g.forcedMove = drive.mv;
                g.forcedCombat = drive.cb;
                if (camper.see) used.Add(camper.gun);
                lost += Math.Max(0, prev - g.health.hp);
                prev = g.health.hp;
                if (!shot && (sim.simStep - c0) * sim.step >= 8.0) { shot = true; yield return Shot32B(dir, shotName, st, shots, ci); }
                if (g.health.Dead || sim.flow == Flow.Down) { died = true; break; }
                yield return null;
            }
            double secs = (sim.simStep - c0) * sim.step, took = bar0 - hpFrac();
            int hits = s.paired - p0;
            double share = s.pairedRaw - r0 > 0 ? (s.pairedLanded - l0) / (s.pairedRaw - r0) : 0;
            bool loses = (died || lost >= 0.5 * maxHp) && took < 0.5, shareOk = hits == 0 || share >= 0.245;
            tally[0]++;
            if (loses && shareOk) tally[1]++;
            rows.Add($"{boss} {name}: {(died ? "DIED " : "")}{secs.ToString("0.0", ci)} s, lost {(100 * lost / maxHp).ToString("0", ci)} % (max hp {maxHp.ToString("0", ci)}), took {(100 * took).ToString("0", ci)} % of the bar, hits {hits} at {(100 * share).ToString("0", ci)} %, weapons {string.Join("+", used)}: {(loses ? "LOSES" : "CAMPS")}{(shareOk ? "" : ", LOW SHARE")}");
            g.health.Reset();
            sim.downSteps = 0;
            sim.flow = Flow.Playing;
        }

        static IEnumerator Camp32Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.n32.bosses32) { res[0] = "RED: design.n32.bosses32 is off in this run | " + pc; yield break; }
            var rows = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            var tally = new int[2];
            offs.Add(() => Quiet(g));
            if (N24CampaignGate(g) == null)
            {
                // THE LOOM's four spots in one fight (boot -skygrindMap n30-span)
                offs.Add(() => { g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(30);
                God(g);
                if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (boot -skygrindMap n30-span) | " + pc; yield break; }
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                sim.campaign?.SetDoorFree29(true);
                RoomDef bossRoom = sim.campaignLevel.rooms.Find(r => r.kind == "boss" && r.boss == "loom");
                if (bossRoom == null || !sim.n31.loom31) { res[0] = "RED: no NIGHT31 Loom on this map | " + pc; yield break; }
                g.husks.frozen = true;
                foreach (string gun in new[] { "gauntlet", "shotgun", "nailgun" }) if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
                Start(g, "camp32", "camera", dir);
                yield return Frames(pre);
                FeelProbe.MarkInput();
                g.Respawn(bossRoom.spawn, bossRoom.spawnYawDeg);
                int rise = 0;
                while ((sim.loom30 == null || !sim.loom30.Alive || !sim.loom30.Hurtable) && rise < 1200) { God(g); g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; rise++; yield return null; }
                if (sim.loom30 == null || !sim.loom30.Hurtable) { res[0] = $"RED: the Loom did not rise ({rise} frames) | " + pc; yield break; }
                Loom30 l = sim.loom30;
                Vec3 c = new Vec3((l.grips[0].x + l.grips[2].x) / 2, l.grips[0].y, (l.grips[0].z + l.grips[2].z) / 2);
                Vec3 In(Vec3 gp, double by) { Vec3 d = Vec3.Normalize(new Vec3(c.x - gp.x, 0, c.z - gp.z)); return new Vec3(gp.x + d.x * by, gp.y + 0.05, gp.z + d.z * by); }
                Vec3 NearJoint()
                {
                    Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
                    Vec3 best = l.joints[0];
                    double bd = 1e9;
                    foreach (Vec3 j in l.joints) { double dd = Vec3.DistanceTo(eye, j); if (dd < bd) { bd = dd; best = j; } }
                    return best;
                }
                var spots = new List<(string, Vec3)> { ("centre (deck)", new Vec3(c.x + 1, c.y + 0.05, c.z + 1)), ("corner (deck grip)", In(l.grips[0], 1.0)), ("far deck corner", In(l.grips[2], 1.0)), ("the entry", new Vec3(bossRoom.spawn.x, bossRoom.spawn.y + 0.05, bossRoom.spawn.z)) };
                for (int i = 0; i < spots.Count && l.Alive; i++)
                    yield return Camp32One(g, "loom", spots[i].Item1, spots[i].Item2, bossRoom.spawnYawDeg, 4, () => l.Alive, () => l.Hurtable, () => l.HpFrac, NearJoint, l.Raycast, dir, "camp-loom-" + i, st, shots, rows, tally, ci);
                bool passL = tally[0] == spots.Count && tally[1] == tally[0];
                res[0] = $"{(passL ? "PASS" : "RED")}: camp32 THE LOOM (one fight, the bar carries over), {tally[1]} of {tally[0]} campers lose with a share >= 25 % | {string.Join(" ; ", rows)} | frames {string.Join(", ", shots)} | " + pc;
                yield break;
            }
            g.husks.frozen = true;
            yield return Frames(20);
            Start(g, "camp32", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            int want = 0;
            // THE LANTERN: centre, edge, platform, across
            if (sim.Lantern30On && sim.n31.lantern31)
            {
                for (int i = 0; i < 4; i++)
                {
                    want++;
                    if (!sim.BossRoomBegin("hulk", out Vec3 sp, out double yd) || sim.lantern27 == null) { rows.Add("lantern: the fight did not begin"); continue; }
                    Lantern27 l = sim.lantern27;
                    Lantern30Arena a = sim.lantern30Arena;
                    if (a == null) { rows.Add("lantern: no ring"); continue; }
                    Vec3 o = l.OriginWorld;
                    int k = a.p.platformPanels[0], far = (a.p.panels / 2 + k) % a.p.panels;
                    Vec3 Ring(int panel, double r, double y) { Vec3 v = -a.InwardNormal(panel) * r; return new Vec3(o.x + v.x, o.y + y, o.z + v.z); }
                    var spots = new[] { ("centre (floor)", new Vec3(o.x + 1.5, o.y + 0.05, o.z + 1.5)), ("edge (floor at the wall)", Ring((k + 4) % a.p.panels, a.p.apothem - 1.2, 0.05)), ("highest (platform)", Ring(k, a.p.apothem - a.p.platformDepth / 2, a.p.platformTop + 0.05)), ("farthest (wall, across)", Ring(far, a.p.apothem - 1.2, 0.05)) };
                    yield return Camp32One(g, "lantern", spots[i].Item1, spots[i].Item2, yd, 1, () => l.Alive, () => l.Hurtable, () => l.HpFrac, () => l.Centre, l.Raycast, dir, "camp-lantern-" + i, st, shots, rows, tally, ci);
                }
            }
            else rows.Add("lantern: the NIGHT31 Lantern is off");
            // THE STAG: the five camp31 spots, each a fresh fight
            if (sim.n31.stag31)
            {
                for (int i = 0; i < 5; i++)
                {
                    want++;
                    if (!Stag31Fresh(g, out Vec3 sp, out double yd)) { rows.Add("stag: the fight did not begin"); continue; }
                    if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
                    Stag31 stg = sim.stag31;
                    Rink28Arena a = sim.stag31Rink;
                    double s0 = a.p.playerStartS;
                    Vec3 F(double sv, double lat) { Vec3 v = a.FloorPoint(sv, lat); return new Vec3(v.x, v.y + 0.1, v.z); }
                    var spots = new[] { ("centre (lane middle)", F(s0 + 10, (a.latIsland + a.latLaneOut) / 2)), ("edge (bank top)", F(s0 + 10, a.latBankTop - 0.6)), ("highest (the island top)", new Vec3(a.p.cx + 0.5, a.islandTopY + 0.1, a.p.cz)), ("farthest (bank top, half a lap)", F(s0 + 10 + a.Lap / 2, a.latBankTop - 0.6)), ("wall corner (lip)", F(a.p.straight / 2, a.latLip - 0.4)) };
                    yield return Camp32One(g, "stag", spots[i].Item1, spots[i].Item2, yd, 2, () => stg.Alive, () => stg.Hurtable, () => stg.HpFrac, () => stg.box.c, stg.Raycast, dir, "camp-stag-" + i, st, shots, rows, tally, ci);
                }
            }
            else rows.Add("stag: design.n31.stag31 is off");
            // THE KILN WYRM: centre, spawn-side corner, far corner
            if (sim.n31.wyrm31)
            {
                for (int i = 0; i < 3; i++)
                {
                    want++;
                    if (!sim.BossRoomBegin("wyrm", out Vec3 sp, out double yd) || sim.wyrm24 == null) { rows.Add("wyrm: the fight did not begin"); continue; }
                    foreach (string gun in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
                    Wyrm w = sim.wyrm24;
                    WyrmArena a = sim.wyrm24Arena;
                    Vec3 o = sim.Wyrm24Spawn - a.spawn;
                    double fy = a.spawn.y + 0.05, sx = a.spawn.x >= 0 ? 1 : -1, sz = a.spawn.z >= 0 ? 1 : -1;
                    var spots = new[] { ("centre (floor)", o + new Vec3(1, fy, 1)), ("corner (floor, spawn side)", o + new Vec3(sx * (a.half - 2), fy, sz * (a.half - 2))), ("farthest (floor, far corner)", o + new Vec3(-sx * (a.half - 2), fy, -sz * (a.half - 2))) };
                    yield return Camp32One(g, "wyrm", spots[i].Item1, spots[i].Item2, yd, 3, () => w.Alive, () => w.Alive && w.phase != "intro" && w.phase != "beat" && w.phase != "dying", () => w.HpFrac, () => w.boxes[0].c, w.Raycast, dir, "camp-wyrm-" + i, st, shots, rows, tally, ci);
                }
            }
            else rows.Add("wyrm: design.n31.wyrm31 is off");
            bool pass = want == 12 && tally[0] == want && tally[1] == tally[0];
            res[0] = $"{(pass ? "PASS" : "RED")}: camp32 (the Loom: boot -skygrindMap n30-span), {tally[1]} of {tally[0]} campers lose with a share >= 25 % (want 12 of 12; real max health) | {string.Join(" ; ", rows)} | frames {string.Join(", ", shots)} | " + pc;
        }
    }
}
