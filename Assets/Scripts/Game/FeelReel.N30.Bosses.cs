// NIGHT30 builder B: the Player scenarios of the NIGHT30 bosses (the lead's FeelReel.N27.cs dispatches PhaseN30B to N30BossesBody). Structure from FeelReel.N29.Bosses.cs (PilotDrive27: the pilots
// are stepped once per SIM step, their Move / Combat go in through g.forcedMove / g.forcedCombat / g.input.SetLook).
//   lantern30   Phase A: LanternPilot30 rides THE LANTERN RING and must kill it inside 45-75 s of game time; the line counts the frames of the floor's tell (and how many showed the vents lit), the
//               live floor (and the core's beam), the sweep (and the swept panel drawn), the ring drawn, and the teaching lines seen (bossLine30, at most two each). Phase B: a second fight, the pilot
//               stands ON A PLATFORM firing the pistol for 20 s: the bar must not move (platform shots land 0). Phase C: on the floor for 40 s: the projected kill must be at least 3x phase A's.
//   wyrm30      THE KILN WYRM, the puncher (god mode): walks up to provoke a charge, faces the head, swings the PULSE gauntlet when the charging head's box is in reach; until two staggers or 90 s.
//               The line counts the punchable frames and how many showed the cyan head, the staggers, the view's bursts, the hitstop requests (cause wyrmStagger) and the pattern after each recoil.
//   glacier30, loom30   RED: not built in this round.
// Boss HP is only READ. Shots go to <dir>/shot-N30-*.png (Shot29's sampled numbers). Every result line starts with PASS or RED. Not run by its author (no Player in the worktree).
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
        static partial void N30BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "lantern30") body = Lantern30Scenario(g, dir, res, offs, pre);
            else if (s == "wyrm30") body = Wyrm30Scenario(g, dir, res, offs, pre);
            else if (s == "glacier30") body = Glacier30Scenario(g, dir, res, offs, pre);
            else if (s == "loom30") body = Loom30Scenario(g, dir, res, offs, pre);
            else body = NotBuilt30(s, res);
        }

        static IEnumerator NotBuilt30(string s, string[] res)
        {
            res[0] = $"RED: {s} is not built in this round (NIGHT30 builder B's queue)";
            yield break;
        }

        static IEnumerator Shot30(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N30-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }

        // ======================================================= THE LANTERN RING =======================================================

        static IEnumerator Lantern30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Lantern27On) { res[0] = "RED: design.n27.boss1 is off (no lantern) | " + pc; yield break; }
            if (!sim.Lantern30On) { res[0] = $"RED: the ring is off in this run (n30.lantern30 {sim.n30.lantern30}, n29.boss1 {sim.n29.boss1}): NIGHT29's courtyard | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yawDeg) || sim.lantern27 == null || !sim.lantern27.Alive) { res[0] = "RED: BossRoomBegin(hulk) did not start the lantern | " + pc; yield break; }
            Lantern27 l = sim.lantern27;
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            var linesSeen = new List<string>();
            int tellLines = 0, coreLines = 0;
            offs.Add(g.events.On<BossLine30>("bossLine30", e => { if (e.key == "tell") tellLines++; else coreLines++; if (linesSeen.Count < 4) linesSeen.Add($"'{e.text}'"); }));
            yield return Frames(20);
            Start(g, "lantern30", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 360;
            var shots = new List<string>();
            var st = new double[4];
            // ---------------- phase A: the rider ----------------
            var pilot = new LanternPilot30 { hand = new Mulberry32(Seed * 7919u + 13u) };
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = yawDeg * Math.PI / 180;
            int step0 = sim.simStep, deaths = 0;
            int frames = 0, ringFrames = 0, tellFrames = 0, ventFrames = 0, liveFrames = 0, sweepFrames = 0, sweepDrawn = 0;
            bool shotTell = false, shotSweep = false, shotLive = false;
            Boss30View v = Boss30View.Current;
            while (l.Alive && (sim.simStep - step0) * sim.step < 160.0 && Time.realtimeSinceStartup < wallEnd)
            {
                frames++;
                if (Down27(g, "hulk")) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t =>
                {
                    pilot.Step(sim, yawL, t);
                    yawL = pilot.Move.yaw;
                    return (pilot.Move, pilot.Combat);
                });
                g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                g.forcedMove = drive.mv;
                g.forcedCombat = drive.cb;
                Lantern30 r = sim.lantern30;
                if (v != null && v.RingShown) ringFrames++;
                if (r != null)
                {
                    Lantern29Stage stg = r.rule.Stage;
                    if (stg == Lantern29Stage.Tell) { tellFrames++; if (v != null && v.VentsLit) ventFrames++; }
                    if (stg == Lantern29Stage.Live) liveFrames++;
                    if (r.SweepPanel >= 0) { sweepFrames++; if (v != null && v.SweepShown) sweepDrawn++; }
                    if (!shotTell && stg == Lantern29Stage.Tell && r.rule.TellProgress > 0.5) { shotTell = true; yield return Shot30(dir, "lantern-tell", st, shots, ci); }
                    if (!shotLive && stg == Lantern29Stage.Live && r.rule.LiveProgress > 0.2) { shotLive = true; yield return Shot30(dir, "lantern-live", st, shots, ci); }
                    if (!shotSweep && r.SweepTelling && r.SweepTellProgress > 0.5) { shotSweep = true; yield return Shot30(dir, "lantern-sweep", st, shots, ci); }
                }
                yield return null;
            }
            bool killed = !l.Alive;
            double killS = (sim.simStep - step0) * sim.step;
            int sweeps = sim.lantern30 != null ? sim.lantern30.sweeps : 0, led = sim.lantern30 != null ? sim.lantern30.ledFans : 0, dodges = pilot.sweepDodges;
            int fullA = sim.lantern29 != null ? sim.lantern29.fullHits : 0;
            // ---------------- phase B: on a platform (20 s) and C: on the floor (40 s) ----------------
            double bHp0 = -1, bHp1 = -1, cHp0 = -1, cHp1 = -1, hpMax0 = g.health.max;
            int bPlatformHits = 0, lobFrames = 0, lobDrawn = 0;
            bool shotLob = false;
            string bDiag = "", cDiag = "";
            int tellA = tellLines, coreA = coreLines; // the line budget is per FIGHT: phase A is one fight (B and C begin two more)
            for (int ph = 0; ph < 2 && killed && Time.realtimeSinceStartup < wallEnd; ph++)
            {
                g.health.Reset();
                sim.downSteps = 0;
                sim.flow = Flow.Playing;
                if (!sim.BossRoomBegin("hulk", out spawn, out yawDeg) || sim.lantern27 == null || !sim.lantern27.Alive) break;
                l = sim.lantern27;
                Vec3 stand = spawn;
                if (ph == 0)
                {
                    Lantern30Arena a = sim.lantern30Arena;
                    int k = a.p.platformPanels[0];
                    Vec3 lp = -a.InwardNormal(k) * (a.p.apothem - a.p.platformDepth / 2);
                    stand = new Vec3(l.OriginWorld.x + lp.x, l.OriginWorld.y + a.p.platformTop + 0.05, l.OriginWorld.z + lp.z);
                }
                g.Respawn(spawn, yawDeg);
                int s0 = sim.simStep;
                // round 1's Player RED (the bar moved while "no shot" landed from the platform): the pilot is put on the platform AFTER the intro (the game's boss intro / retry
                // can put it back at the room's spawn), in god mode the whole time, and the line prints where it stood and which rule took its shots
                while (l.Alive && !l.Hurtable && (sim.simStep - s0) * sim.step < 30.0) { God(g); g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; yield return null; }
                g.Respawn(stand, yawDeg);
                for (int i = 0; i < Ms(500); i++) { God(g); g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; yield return null; }
                double hp0 = l.HpFrac;
                int sF = sim.simStep, fb = 0, p0 = sim.lantern30 != null ? sim.lantern30.platformHits : 0, f0 = sim.lantern30 != null ? sim.lantern30.floorHits : 0, lh0 = sim.lantern29 != null ? sim.lantern29.lightHits : 0;
                int onFrames = 0, allFrames = 0;
                bool ruleIs30 = l.rule29 is Lantern30;
                double len = ph == 0 ? 20.0 : 40.0;
                while (l.Alive && (sim.simStep - sF) * sim.step < len && Time.realtimeSinceStartup < wallEnd)
                {
                    fb++;
                    God(g);
                    allFrames++;
                    if (sim.lantern30 != null && sim.lantern30.PlayerOnPlatform()) onFrames++;
                    Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = l.Centre - eye;
                    double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                    int gun = sim.loadout.owned.IndexOf("revolver");
                    bool tap = fb % 2 == 0;
                    g.input.SetLook(wy, wp);
                    g.forcedMove = new InputFrame { yaw = wy, pitch = wp };
                    g.forcedCombat = new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1, fire = new Btn { held = tap, pressed = tap } };
                    if (ph == 0 && sim.lantern30 != null && sim.lantern30.LobTelling) { lobFrames++; if (v != null && v.LobShown) lobDrawn++; if (!shotLob && sim.lantern30.LobTellProgress > 0.5) { shotLob = true; yield return Shot30(dir, "lantern-lob", st, shots, ci); } }
                    yield return null;
                }
                Vec3 endLocal = sim.mover.position - l.OriginWorld;
                string diag = $"stood on a platform {onFrames}/{allFrames} frames, rule {(ruleIs30 ? "Lantern30" : "NOT Lantern30")}, light hits {(sim.lantern29 != null ? sim.lantern29.lightHits : 0) - lh0}, floor hits {(sim.lantern30 != null ? sim.lantern30.floorHits : 0) - f0}, end at local ({endLocal.x.ToString("0.0", ci)}, {endLocal.y.ToString("0.00", ci)}, {endLocal.z.ToString("0.0", ci)})";
                if (ph == 0) { bHp0 = hp0; bHp1 = l.HpFrac; bPlatformHits = (sim.lantern30 != null ? sim.lantern30.platformHits : 0) - p0; bDiag = diag; }
                else { cHp0 = hp0; cHp1 = l.HpFrac; cDiag = diag; }
                g.health.max = hpMax0; g.health.Reset();
            }
            Quiet(g);
            yield return Frames(Ms(800));
            yield return Stop();
            var fails = new List<string>();
            if (!killed) fails.Add($"the rider did not kill the lantern in {killS.ToString("0", ci)} s");
            else if (killS < 45 || killS > 75) fails.Add($"the kill took {killS.ToString("0.0", ci)} s, outside 45-75 s");
            if (ringFrames * 2 < frames) fails.Add($"the ring was drawn in {ringFrames} of {frames} frames");
            if (tellFrames == 0) fails.Add("no tell frame"); else if (ventFrames * 2 < tellFrames) fails.Add($"the vents glowed in {ventFrames} of {tellFrames} tell frames");
            if (sweepFrames == 0) fails.Add("no sweep"); else if (sweepDrawn * 2 < sweepFrames) fails.Add($"the swept panel was drawn in {sweepDrawn} of {sweepFrames} sweep frames");
            if (tellA == 0 || coreA == 0) fails.Add($"teaching lines in the rider's fight: tell {tellA} / core {coreA}");
            if (tellA > 2 || coreA > 2) fails.Add($"a teaching line showed more than twice in one fight (tell {tellA}, core {coreA})");
            if (bHp0 < 0) fails.Add("phase B (platform) did not run");
            else
            {
                if (bHp1 < bHp0 - 1e-9) fails.Add($"platform shots moved the bar ({bHp0.ToString("0.000", ci)} -> {bHp1.ToString("0.000", ci)})");
                if (bPlatformHits == 0) fails.Add("the platform pilot landed no shot on the lantern (the test shows nothing)");
                if (lobFrames == 0) fails.Add("no lob tell at the platform camper in 20 s"); else if (lobDrawn * 2 < lobFrames) fails.Add($"the lob's mark was drawn in {lobDrawn} of {lobFrames} tell frames");
            }
            double projected = cHp0 > cHp1 ? 40.0 * cHp0 / (cHp0 - cHp1) : double.PositiveInfinity;
            if (cHp0 < 0) fails.Add("phase C (floor) did not run");
            else if (killed && projected < 3 * killS) fails.Add($"a floor camper would kill in about {projected.ToString("0", ci)} s, under 3x the rider's {killS.ToString("0.0", ci)} s");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, LanternPilot30 through forcedMove / forcedCombat, boss HP only READ: "
                + $"A rider {(killed ? "KILLED" : "NOT killed")} at {killS.ToString("0.0", ci)} s (band 45-75), full hits {fullA}, deaths {deaths}, sweeps {sweeps} (dodged {dodges}), led fans {led}; frames: ring {ringFrames}/{frames}, tell {tellFrames} (vents lit {ventFrames}), live {liveFrames}, sweep {sweepFrames} (drawn {sweepDrawn}); "
                + $"lines in fight A tell {tellA} / core {coreA} (all three fights {tellLines} / {coreLines}): {string.Join(", ", linesSeen)}; B platform 20 s: bar {bHp0.ToString("0.000", ci)} -> {bHp1.ToString("0.000", ci)}, platform hits {bPlatformHits}, lob tell frames {lobFrames} (drawn {lobDrawn}), {bDiag}; "
                + $"C floor 40 s: bar {cHp0.ToString("0.000", ci)} -> {cHp1.ToString("0.000", ci)} (projected kill {(double.IsInfinity(projected) ? "never" : projected.ToString("0", ci) + " s")}), {cDiag}; shots: {string.Join("; ", shots)}";
        }

        // ======================================================= THE GLACIER REBUILT =======================================================

        /// <summary>glacier30: phase A, GlacierPilot29 chases on the bigger rink and must kill inside 80-130 s; the line counts the sweep's frames (and its band drawn), the volley tells (and
        /// the boss's red-orange shell drawn), the longest out-of-reach stretch, the rink's size. Phase B: the BANK CAMPER (god mode) stands near the top of the outer bank for 30 s firing the
        /// pistol: it must be hit by the boss (the old safe zone is in reach), its longest quiet stretch printed.</summary>
        static IEnumerator Glacier30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Glacier28On) { res[0] = "RED: design.n28.glacier.enabled is off (no rink fight) | " + pc; yield break; }
            if (!sim.Glacier30On) { res[0] = $"RED: the rebuilt glacier is off in this run (n30.glacier30 {sim.n30.glacier30}, n29.boss2 {sim.n29.boss2}): NIGHT29's rink | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.Glacier28Begin() || sim.glacier28 == null || !sim.glacier28.Alive) { res[0] = "RED: Glacier28Begin did not start the lap fight | " + pc; yield break; }
            Glacier28 gl = sim.glacier28;
            Rink28Arena a = sim.glacier28Rink;
            double w = 2 * (a.p.islandRadius + a.p.laneWidth + a.p.bankRun + a.p.lip), len = a.p.straight + w;
            if (sim.loadout.owned.IndexOf("gauntlet") < 0) sim.loadout.AddWeapon("gauntlet");
            g.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
            pc = Preconditions(g, sim.Glacier28Spawn);
            int bossHits = 0;
            offs.Add(g.events.On<HurtEvent>("hurt", e => { if (e.source == "boss") bossHits++; }));
            yield return Frames(20);
            Start(g, "glacier30", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 300;
            var shots = new List<string>();
            var st = new double[4];
            Boss30View v = Boss30View.Current;
            var pilot = new GlacierPilot29 { hand = new Mulberry32(Seed * 7919u + 13u) };
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = sim.Glacier28SpawnYawDeg * Math.PI / 180;
            int step0 = sim.simStep, deaths = 0, sweepFrames = 0, bandDrawn = 0, volleyFrames = 0, volleyDrawn = 0, outRun = 0, outMax = 0;
            bool shotSweep = false, shotVolley = false;
            while (gl.Alive && (sim.simStep - step0) * sim.step < 200.0 && Time.realtimeSinceStartup < wallEnd)
            {
                if (Down28(g)) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t =>
                {
                    pilot.Step(sim, yawL, t);
                    yawL = pilot.Move.yaw;
                    return (pilot.Move, pilot.Combat);
                });
                g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                g.forcedMove = drive.mv;
                g.forcedCombat = drive.cb;
                Glacier30 g30 = sim.glacier30;
                if (g30 != null)
                {
                    if (g30.SweepTelling || g30.SweepLive) { sweepFrames++; if (v != null && v.BandShown) bandDrawn++; }
                    if (g30.VolleyTelling) { volleyFrames++; if (v != null && v.VolleyGlowShown) volleyDrawn++; }
                    outRun = g30.OutOfReach ? outRun + 1 : 0;
                    outMax = Math.Max(outMax, outRun);
                    if (!shotSweep && g30.SweepTelling && g30.SweepTellProgress > 0.5) { shotSweep = true; yield return Shot30(dir, "glacier-sweep", st, shots, ci); }
                    if (!shotVolley && g30.VolleyTelling) { shotVolley = true; yield return Shot30(dir, "glacier-volley", st, shots, ci); }
                }
                yield return null;
            }
            bool killed = !gl.Alive;
            double killS = (sim.simStep - step0) * sim.step;
            int hitsA = bossHits;
            // ---------------- phase B: the bank camper ----------------
            int hitsB = -1, volleysB = 0, shardHitsB = 0, volleyFramesB = 0, volleyDrawnB = 0, shardFramesB = 0;
            bool shotVolleyB = false;
            double quietB = 0;
            if (killed && Time.realtimeSinceStartup < wallEnd)
            {
                g.health.Reset();
                sim.downSteps = 0;
                sim.flow = Flow.Playing;
                if (sim.Glacier28Begin() && sim.glacier28 != null && sim.glacier28.Alive)
                {
                    gl = sim.glacier28;
                    a = sim.glacier28Rink;
                    Vec3 camp = a.FloorPoint(a.p.playerStartS + 20, a.latBankTop - 0.6);
                    g.Respawn(new Vec3(camp.x, camp.y + 0.1, camp.z), 0);
                    int s0 = sim.simStep;
                    while (gl.Alive && !gl.IsPlaying && (sim.simStep - s0) * sim.step < 20.0) { God(g); g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; yield return null; }
                    int h0 = bossHits, sF = sim.simStep, lastHit = sim.simStep, quiet = 0, fb = 0;
                    int v0 = sim.glacier30 != null ? sim.glacier30.volleys : 0, sh0 = sim.glacier30 != null ? sim.glacier30.shardHits : 0;
                    while (gl.Alive && (sim.simStep - sF) * sim.step < 30.0 && Time.realtimeSinceStartup < wallEnd)
                    {
                        fb++;
                        God(g);
                        if (bossHits != h0 + (hitsB < 0 ? 0 : hitsB)) { hitsB = bossHits - h0; quiet = Math.Max(quiet, sim.simStep - lastHit); lastHit = sim.simStep; }
                        Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = gl.box.c - eye;
                        double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                        var mv = new InputFrame { yaw = wy, pitch = wp };
                        double cx = camp.x - me.x, cz = camp.z - me.z, cl = JsMath.Hypot(cx, cz);
                        if (cl > 1.0)
                        {
                            Vec3 fwd = MovementMath.WishDirection(0, 1, wy), right = MovementMath.WishDirection(1, 0, wy);
                            mv.moveZ = (cx * fwd.x + cz * fwd.z) / cl;
                            mv.moveX = (cx * right.x + cz * right.z) / cl;
                        }
                        int gun = sim.loadout.owned.IndexOf("revolver");
                        bool tap = fb % 4 == 0;
                        g.input.SetLook(wy, wp);
                        g.forcedMove = mv;
                        g.forcedCombat = new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1, fire = new Btn { held = tap, pressed = tap } };
                        if (fb == 240) yield return Shot30(dir, "glacier-bank", st, shots, ci);
                        Glacier30 gb = sim.glacier30;
                        if (gb != null && gb.VolleyTelling) { volleyFramesB++; if (v != null && v.VolleyGlowShown) volleyDrawnB++; if (!shotVolleyB) { shotVolleyB = true; yield return Shot30(dir, "glacier-bank-volley", st, shots, ci); } }
                        if (gb != null && gb.shards.Count > 0) shardFramesB++;
                        yield return null;
                    }
                    if (hitsB < 0) hitsB = bossHits - h0;
                    volleysB = (sim.glacier30 != null ? sim.glacier30.volleys : 0) - v0;
                    shardHitsB = (sim.glacier30 != null ? sim.glacier30.shardHits : 0) - sh0;
                    quietB = Math.Max(quiet, sim.simStep - lastHit) * sim.step;
                    g.health.Reset();
                }
            }
            Quiet(g);
            yield return Frames(Ms(800));
            yield return Stop();
            double limit = g.cfg.Num("design.n30.glacier30.hideSeconds") + g.cfg.Num("design.n30.glacier30.fillSeconds");
            var fails = new List<string>();
            if (!killed) fails.Add($"the chaser did not kill the glacier in {killS.ToString("0", ci)} s");
            else if (killS < 80 || killS > 130) fails.Add($"the kill took {killS.ToString("0.0", ci)} s, outside 80-130 s");
            if (sweepFrames == 0) fails.Add("no frost sweep"); else if (bandDrawn * 2 < sweepFrames) fails.Add($"the sweep band was drawn in {bandDrawn} of {sweepFrames} frames");
            if (volleyFrames > 0 && volleyDrawn * 2 < volleyFrames) fails.Add($"the volley tell was drawn in {volleyDrawn} of {volleyFrames} frames");
            if (outMax * sim.step > limit + 0.5) fails.Add($"the chaser was out of reach {(outMax * sim.step).ToString("0.0", ci)} s (limit {limit.ToString("0", ci)} s)");
            if (hitsB < 0) fails.Add("phase B (the bank camper) did not run"); else if (hitsB == 0) fails.Add("the bank camper was never hit in 30 s (the slope is still a safe zone)");
            if (hitsB >= 0 && volleysB == 0) fails.Add("no homing volley at the bank camper in 30 s (far / hidden must be volleyed)");
            else if (volleyFramesB > 0 && volleyDrawnB * 2 < volleyFramesB) fails.Add($"the bank camper's volley tell was drawn in {volleyDrawnB} of {volleyFramesB} frames");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, GlacierPilot29 through forcedMove / forcedCombat, boss HP only READ; rink {w.ToString("0", ci)} x {len.ToString("0", ci)} m: "
                + $"A chaser {(killed ? "KILLED" : "NOT killed")} at {killS.ToString("0.0", ci)} s (band 80-130), deaths {deaths}, boss hits on it {hitsA}, longest out of reach {(outMax * sim.step).ToString("0.0", ci)} s, sweep frames {sweepFrames} (band drawn {bandDrawn}), volley tell frames {volleyFrames} (drawn {volleyDrawn}); "
                + $"B bank camper 30 s: boss hits {hitsB}, longest quiet {quietB.ToString("0.0", ci)} s, volleys {volleysB} ({shardHitsB} shard hits, tell frames {volleyFramesB}, drawn {volleyDrawnB}, shards flying {shardFramesB} frames); shots: {string.Join("; ", shots)}";
        }

        // ======================================================= THE LOOM =======================================================

        /// <summary>loom30 (needs a campaign run on the span level, n30-span, with the player in room s8 or the room current): the Loom is begun (BossRoomBegin "loom"), the player in god
        /// mode hangs on joint 0 (the rope attached to it directly: no pilot swings yet) and fires the pistol at joint 1 (the same leg: full damage) for 12 s, then lets go; the line
        /// counts the Loom drawn, the cyan joints, the CUT's tells and cuts and the red line drawn, and the bar's drop per hit on the line vs off it.</summary>
        static IEnumerator Loom30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            // round 6 (r5 Player: BossRoomBegin from outside the campaign started nothing): the campaign path L's rooms30 uses (FeelReel.N30.Levels.cs): a CAMPAIGN run on the
            // booted map (-skygrindMap n30-span), DOORFREE, god mode, then a respawn into the boss room: the campaign enters it and begins its boss (Campaign24 -> BossRoomBegin "loom")
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (boot -skygrindMap n30-span) | " + pc; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            pc = Preconditions(g, g.Feet) + $" | level {sim.campaignLevel.id}, n30.span {sim.n30.span}";
            if (!sim.n30.span) { res[0] = "RED: design.n30.span is off in this run (no Loom) | " + pc; yield break; }
            RoomDef bossRoom = null;
            int bossIdx = -1;
            for (int k = 0; k < sim.campaignLevel.rooms.Count; k++) if (sim.campaignLevel.rooms[k].kind == "boss" && sim.campaignLevel.rooms[k].boss == "loom") { bossRoom = sim.campaignLevel.rooms[k]; bossIdx = k; }
            if (bossRoom == null) { res[0] = $"RED: level {sim.campaignLevel.id} has no boss room with boss 'loom' | " + pc; yield break; }
            g.husks.frozen = true;
            int tells = 0, cuts = 0;
            offs.Add(g.events.On<LoomCut30>("loomCut", e => { if (e.stage == "tell") tells++; else cuts++; }));
            var linesSeen = new List<string>();
            offs.Add(g.events.On<BossLine30>("bossLine30", e => { if (e.boss == "loom") linesSeen.Add($"'{e.text}'"); }));
            Start(g, "loom30", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            g.Respawn(bossRoom.spawn, bossRoom.spawnYawDeg);
            double yawDeg = bossRoom.spawnYawDeg;
            double wallRise = Time.realtimeSinceStartup + 30;
            int riseFrames = 0;
            while ((sim.loom30 == null || !sim.loom30.Alive) && riseFrames < 600 && Time.realtimeSinceStartup < wallRise && sim.flow != Flow.Results) { God(g); riseFrames++; yield return null; }
            bool inRoom = sim.campaign != null && sim.campaign.room == bossIdx;
            if (sim.loom30 == null || !sim.loom30.Alive)
            {
                res[0] = $"RED: the Loom did not rise after a respawn into {bossRoom.id} ({riseFrames} frames; in the room {inRoom}; flow {sim.flow}) | " + pc;
                yield break;
            }
            Loom30 l = sim.loom30;
            yield return Shot30(dir, "loom-rise");
            var shots = new List<string> { "shot-N30-loom-rise.png" };
            int s0 = sim.simStep;
            // round 5 (r2: L's span30 hung a Player 14 min waiting): every loop here ends on its own: 90 s of game time in all, a 150 s wall-clock guard, RESULTS / DOWN / a paused flow
            double wallEnd = Time.realtimeSinceStartup + 150;
            string ended = null;
            bool Stop30()
            {
                if (sim.flow == Flow.Results) ended = "the run reached RESULTS";
                else if (sim.flow == Flow.Title) ended = "the run went back to TITLE";
                else if ((sim.simStep - s0) * sim.step >= 90.0) ended = "the 90 s game-time timeout";
                else if (Time.realtimeSinceStartup >= wallEnd) ended = "the 150 s wall-clock guard (the sim did not advance)";
                return ended != null;
            }
            while (l.Alive && l.phase != "p1" && (sim.simStep - s0) * sim.step < 20.0 && !Stop30()) { God(g); g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; yield return null; }
            Boss30View v = Boss30View.Current;
            int frames = 0, drawn = 0, cyanFrames = 0, cutLineFrames = 0, full0 = l.fullHits, light0 = l.lightHits;
            double hp0 = l.HpFrac;
            bool shotCyan = false, shotCut = false;
            // round 7 (r6 Player RED: the CUT saw 0 tells: a rope set by code is cleared by the Mover's own rope step before the boss steps, so the reel re-set it every frame
            // and nobody was ever "hanging"): the REAL grapple, as S's arcline30 attaches: stand on the deck's corner by leg 0's grip, aim at joint 0 (the knee, a grapple node
            // while the Loom fights) and hold the grapple press until the hook is on, then keep holding (the swing) and nail joint 0; after a cut (or a lost hook) back to the corner
            Vec3 j0 = l.joints[0], grip0 = l.grips[0];
            int sF = sim.simStep, attaches = 0, regrabs = 0, lastRespawn = -100000, hangFrames = 0;
            double anchorOff = 1e9;
            bool fired = false;
            offs.Add(g.events.On<Arcline21RopeEvent>("ropeAttach", e => { if (e.node) attaches++; }));
            bool owned = sim.arcline21 != null && sim.arcline21.Owned;
            while (l.Alive && (sim.simStep - sF) * sim.step < 16.0 && !Stop30())
            {
                frames++;
                God(g);
                if (sim.flow == Flow.Down || g.health.Dead) { g.health.Reset(); sim.downSteps = 0; sim.flow = Flow.Playing; } // god mode should prevent it; a death is not a hang
                j0 = l.joints[0];
                bool attached = sim.mover.rope.phase == RopePhase.Attached;
                int gun = sim.loadout.owned.IndexOf("revolver");
                if (!attached && sim.mover.rope.phase == RopePhase.None && (sim.simStep - lastRespawn) * sim.step > 0.6 && (sim.simStep - sF) * sim.step < 14.0)
                {
                    // back to the corner by leg 0's grip (a step inside the deck) and a fresh press
                    double cx = l.centre.x - grip0.x, cz = l.centre.z - grip0.z, cl = Math.Max(1e-6, JsMath.Hypot(cx, cz));
                    Vec3 me0 = sim.mover.position;
                    if (!l.OnDeck(me0) || JsMath.Hypot(me0.x - grip0.x, me0.z - grip0.z) > 3.0)
                    {
                        g.Respawn(new Vec3(grip0.x + cx / cl * 1.5, grip0.y + 0.05, grip0.z + cz / cl * 1.5), yawDeg);
                        regrabs++;
                    }
                    lastRespawn = sim.simStep;
                    fired = false;
                }
                Vec3 eye = sim.Eye, d = j0 - eye;
                double wy = Math.Atan2(-d.x, -d.z), wp = Math.Asin(Math.Max(-1, Math.Min(1, d.y / Math.Max(1e-6, d.Length))));
                g.input.SetLook(wy, wp);
                if (sim.mover.rope.phase != RopePhase.None) fired = true;
                // the grapple: pressed until the hook is out (a frame may hold no sim step), then held: the swing
                g.forcedMove = new InputFrame { yaw = wy, pitch = wp, grapple = new Btn { held = true, pressed = !fired } };
                bool tap = attached && frames % 3 == 0;
                if (attached) { hangFrames++; anchorOff = Math.Min(anchorOff, Vec3.DistanceTo(sim.mover.rope.anchor, j0)); }
                g.forcedCombat = new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1, fire = new Btn { held = tap, pressed = tap } };
                if (v != null && v.LoomShown) drawn++;
                if (v != null && v.LoomCyanJoints > 0) { cyanFrames++; if (!shotCyan) { shotCyan = true; shots.Add("shot-N30-loom-joint.png"); yield return Shot30(dir, "loom-joint"); } }
                if (v != null && v.CutLineShown) { cutLineFrames++; if (!shotCut && l.CutTellProgress > 0.5) { shotCut = true; shots.Add("shot-N30-loom-cut.png"); yield return Shot30(dir, "loom-cut"); } }
                yield return null;
            }
            double hp1 = l.HpFrac;
            // the deck: let go of the grapple, stand in its middle for 14 s (the SWEEP comes for a player standing there)
            g.forcedMove = new InputFrame();
            yield return Frames(6);
            g.Respawn(new Vec3(l.centre.x + 1.5, l.centre.y + 0.05, l.centre.z + 1.5), yawDeg);
            int sw0 = l.sweeps, swHits0 = l.sweepHits, swFrames = 0, swDrawn = 0, sD = sim.simStep;
            bool shotSweep = false;
            while (l.Alive && (sim.simStep - sD) * sim.step < 14.0 && !Stop30())
            {
                God(g);
                if (sim.flow == Flow.Down || g.health.Dead) { g.health.Reset(); sim.downSteps = 0; sim.flow = Flow.Playing; }
                g.forcedMove = new InputFrame { yaw = yawDeg * Math.PI / 180 };
                g.forcedCombat = new CombatInput { weapon = -1 };
                if (l.SweepTelling || l.SweepLive) { swFrames++; if (v != null && v.LoomSweepShown) swDrawn++; if (!shotSweep && l.SweepTelling && l.SweepTellProgress > 0.5) { shotSweep = true; shots.Add("shot-N30-loom-sweep.png"); yield return Shot30(dir, "loom-sweep"); } }
                yield return null;
            }
            int sweepsC = l.sweeps - sw0, sweepHitsC = l.sweepHits - swHits0;
            Quiet(g);
            yield return Frames(Ms(600));
            yield return Stop();
            var fails = new List<string>();
            if (sweepsC == 0) fails.Add("no SWEEP at a player standing on the deck for 14 s");
            else if (swDrawn * 2 < swFrames) fails.Add($"the sweep bar was drawn in {swDrawn} of {swFrames} frames");
            if (drawn * 2 < frames) fails.Add($"the Loom was drawn in {drawn} of {frames} frames");
            if (cyanFrames == 0) fails.Add("no joint glowed cyan while the player hung on its leg");
            if (!owned) fails.Add("the ARCLINE is not owned in this run (no grapple)");
            else if (attaches == 0) fails.Add("the real grapple never hooked a node from leg 0's corner");
            if (tells == 0 || cuts == 0) fails.Add($"the CUT: {tells} tells, {cuts} cuts in 14 s of grappling on leg 0");
            else if (cutLineFrames == 0) fails.Add("the cut's red line was never drawn");
            if (l.fullHits - full0 == 0) fails.Add("no full hit on the hung leg's joint");
            if (ended != null) fails.Insert(0, "ended by " + ended);
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, god mode, the REAL grapple held on joint 0 from leg 0's corner (as arcline30): "
                + $"ARCLINE owned {owned}, real hooks on a node {attaches}, corner respawns {regrabs}, hanging frames {hangFrames}, the anchor's nearest to joint 0 {(anchorOff < 1e8 ? anchorOff.ToString("0.0", ci) + " m" : "never attached")}; drawn {drawn}/{frames}, cyan-joint frames {cyanFrames}, cut tells {tells}, cuts {cuts}, red-line frames {cutLineFrames}, full hits {l.fullHits - full0}, light hits {l.lightHits - light0}, bar {hp0.ToString("0.000", ci)} -> {hp1.ToString("0.000", ci)}; deck 14 s: sweeps {sweepsC} (hit {sweepHitsC}), sweep frames {swFrames} (bar drawn {swDrawn}); shots: {string.Join("; ", shots)}";
        }

        // ======================================================= THE KILN WYRM HEAD-PUNCH =======================================================

        static IEnumerator Wyrm30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.n30.wyrm30) { res[0] = "RED: design.n30.wyrm30 is off in this run (no head punch: NIGHT29's worm) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            if (!sim.Wyrm24Begin()) { res[0] = "RED: Wyrm24Begin failed (the row is off, the files were not found, or no BossKit) | " + pc; yield break; }
            Wyrm w = sim.wyrm24;
            g.husks.frozen = true;
            g.Respawn(sim.Wyrm24Spawn, sim.wyrm24Arena.spawnYawDeg);
            if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
            int gi = sim.loadout.owned.IndexOf("gauntlet");
            pc = Preconditions(g, sim.mover.position);
            int hitstops = 0;
            var after = new List<string>();
            offs.Add(g.events.On<string>("hitstop", c => { if (c == "wyrmStagger") hitstops++; }));
            for (int i = 0; i < Ms(800); i++) { God(g); yield return null; }
            Start(g, "wyrm30", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 240;
            var shots = new List<string>();
            var st = new double[4];
            Boss30View v = Boss30View.Current;
            int bursts0 = v != null ? v.staggerBursts : 0, st0 = sim.wyrm30Staggers;
            int step0 = sim.simStep, frames = 0, punchFrames = 0, glowFrames = 0, charges = 0, swings = 0, recoils = 0, chargeAfter = 0;
            bool wasCharging = false, swung = false, wasRecoil = false, shotGlow = false, shotStagger = false;
            double reach = g.cfg.Num("weapons.gauntlet.range");
            while (w.Alive && sim.wyrm30Staggers - st0 < 2 && (sim.simStep - step0) * sim.step < 90.0 && Time.realtimeSinceStartup < wallEnd)
            {
                frames++;
                God(g);
                Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), head = w.boxes[0].c, d = head - eye;
                double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                bool charging = w.Charging30;
                if (charging && !wasCharging) { charges++; swung = false; }
                wasCharging = charging;
                var mv = new InputFrame { yaw = wy, pitch = wp };
                if (!charging && !w.Recoiling30 && d.Length > 9) mv.moveZ = 1;
                bool swing = charging && !swung && w.boxes[0].Touches(eye, reach);
                if (swing) { swung = true; swings++; }
                g.input.SetLook(wy, wp);
                g.forcedMove = mv;
                g.forcedCombat = new CombatInput { weapon = sim.loadout.Weapon != "gauntlet" ? gi : -1, fire = new Btn { held = swing, pressed = swing } };
                if (sim.Wyrm30Punchable) { punchFrames++; if (v != null && v.HeadGlowShown) glowFrames++; }
                bool rec = w.Recoiling30;
                if (rec && !wasRecoil) recoils++;
                if (!rec && wasRecoil) { after.Add(w.PatternName ?? "?"); if (w.AggroDiving || w.Lunging) chargeAfter++; }
                wasRecoil = rec;
                if (!shotGlow && sim.Wyrm30Punchable && v != null && v.HeadGlowShown) { shotGlow = true; yield return Shot30(dir, "wyrm-punchable", st, shots, ci); }
                if (!shotStagger && sim.wyrm30Staggers > st0) { shotStagger = true; yield return Shot30(dir, "wyrm-stagger", st, shots, ci); }
                yield return null;
            }
            int staggers = sim.wyrm30Staggers - st0, bursts = (v != null ? v.staggerBursts : 0) - bursts0;
            double secs = (sim.simStep - step0) * sim.step;
            Quiet(g);
            yield return Frames(Ms(800));
            yield return Stop();
            var fails = new List<string>();
            if (staggers == 0) fails.Add($"no stagger in {secs.ToString("0", ci)} s ({charges} charges, {swings} swings at the charging head)");
            if (punchFrames == 0) fails.Add("the head was never punchable");
            else if (glowFrames * 2 < punchFrames) fails.Add($"the cyan head showed in {glowFrames} of {punchFrames} punchable frames");
            if (bursts != staggers) fails.Add($"the view burst {bursts} times for {staggers} staggers");
            if (hitstops != staggers) fails.Add($"{hitstops} wyrmStagger hitstops for {staggers} staggers");
            if (chargeAfter > 0) fails.Add($"{chargeAfter} charge(s) right after a recoil");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, the puncher in god mode, {secs.ToString("0.0", ci)} s of game time: "
                + $"charges {charges}, swings {swings}, punchable frames {punchFrames} (cyan head drawn {glowFrames}), staggers {staggers}, view bursts {bursts}, hitstops {hitstops}, recoils {recoils}, then [{string.Join(", ", after)}]; shots: {string.Join("; ", shots)}";
        }
    }
}
