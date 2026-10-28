// NIGHT29 builder B: the Player scenarios of the REWORKED bosses (design.n29.boss1 / boss2). The lead owns the phase list; the two names are PhaseN29 below and the body is the one partial method
// N29BossesBody (the lead adds its declaration and a branch in the scenario switch; see the report). Structure copied from FeelReel.N27.Bosses.cs (PilotDrive27: the pilots are stepped once per SIM step,
// their Move / Combat go in through g.forcedMove / g.forcedCombat / g.input.SetLook) and FeelReel.N28.Glacier.cs.
//   lantern29   Phase A: LanternPilot29 (a wall rider; it shoots the box on ANY face whenever its ray lands, the light-damage rule) must kill THE LANTERN inside 45-70 s of game time. Phase B: a second fight,
//               the pilot STANDS on the floor firing the pistol at the box for 40 s of game time: it must move the bar (light damage) and never land a full hit (Lantern29.fullHits 0). On the way the
//               scenario reads sim.Lantern29SlowScale during the live floor (expect about the design's slowScale 0.55) and counts the frames the view's floor-live glow shows in the tell and the live stage.
//   glacier29   GlacierPilot29 (chases the boss's LANE, dashes to switch lanes, the pistol on sight) must catch up and kill THE GLACIER inside 80-120 s of game time; the line prints the gap, the exposed windows,
//               the other-gun hits (light and inside the window) and the lane-lock share.
// NO HP CUT anywhere: the scenarios only READ boss HP. Three screenshots per scenario go to <dir>/shot-N29-*.png with their sampled numbers (roof / wall band brightness, cyan lane-line pixels, red-orange pixels).
// Every result line starts with PASS or RED and its reason, then the preconditions. Loops are bounded by game time (the sim's steps) and a wall-clock guard. Not run by its author (no Player in the worktree).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN29 = { "lantern29", "glacier29" };

        static bool IsPhaseN29(string s) => Array.IndexOf(PhaseN29, s) >= 0;

        static partial void N29BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        static partial void N29BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "lantern29") body = Lantern29Scenario(g, dir, res, offs, pre);
            else if (s == "glacier29") body = Glacier29Scenario(g, dir, res, offs, pre);
        }

        /// <summary>Saves the screen to dir/shot-N29-name.png and returns what it measured: the mean brightness of the top band (roof / upper wall), the share of cyan pixels (lane lines, wall-ride strip) and of
        /// red-orange pixels (the lava glow) in the lower two thirds. Call from a coroutine; yields to the end of the frame.</summary>
        static IEnumerator Shot29(string dir, string name, double[] stats)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N29-" + name + ".png"), t.EncodeToPNG());
                int w = t.width, h = t.height, n = 0, cyan = 0, red = 0, nt = 0, nm = 0;
                double top = 0, mid = 0;
                for (int y = 0; y < h; y += 8)
                    for (int x = 0; x < w; x += 8)
                    {
                        Color c = t.GetPixel(x, y); // y runs from the bottom
                        // the mean luminance of the central 60 % x 56 % of the frame, without the gun's corner (x > 62 %, y under 45 % from the top-down) and without cyan pixels (the boss, the lane lines, the strips)
                        if (x > w * 0.2 && x < w * 0.8 && y > h * 0.22 && y < h * 0.78 && !(x > w * 0.62 && y < h * 0.45) && !(c.b > 0.55 && c.g > 0.55 && c.r < 0.45))
                        {
                            mid += 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                            nm++;
                        }
                        if (y > h * 0.80) { top += 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b; nt++; }
                        else if (y < h * 0.66)
                        {
                            n++;
                            if (c.b > 0.55 && c.g > 0.55 && c.r < 0.45) cyan++;
                            if (c.r > 0.55 && c.g < 0.45 && c.b < 0.30) red++;
                        }
                    }
                stats[0] = nt > 0 ? top / nt : -1;
                stats[1] = n > 0 ? (double)cyan / n : -1;
                stats[2] = n > 0 ? (double)red / n : -1;
                stats[3] = nm > 0 ? mid / nm : -1;
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
            yield return SameRun63(dir, "shot-N29-" + name + ".png"); // NIGHT63 D1: with -skygrindSameRun63, the row-off half into <side>-off/ (FeelReel.N63Same.cs); nothing without it
        }

        static string ShotLine29(string name, double[] s, CultureInfo ci) => $"{name} (top band {s[0].ToString("0.00", ci)}, cyan {(100 * s[1]).ToString("0.0", ci)} %, red-orange {(100 * s[2]).ToString("0.0", ci)} %, mid-frame luminance {s[3].ToString("0.000", ci)})";

        // ======================================================= THE LANTERN =======================================================

        static IEnumerator Lantern29Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Lantern27On) { res[0] = "RED: design.n27.boss1 is off (the plan's boss 'hulk' is NIGHT26's Hulk, no lantern) | " + pc; yield break; }
            if (!sim.n29.boss1) { res[0] = "RED: design.n29.boss1 is off in this run (the old shield rule applies, not the light-damage rule) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yawDeg) || sim.lantern27 == null || !sim.lantern27.Alive) { res[0] = "RED: BossRoomBegin(hulk) did not start the lantern | " + pc; yield break; }
            Lantern27 l = sim.lantern27;
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            yield return Frames(20);
            Start(g, "lantern29", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 300;
            int downs = 0;
            offs.Add(g.events.On<BossEvent>("bossDown", e => downs++));
            var shotStats = new List<string>();
            var st = new double[4];
            // ---------------- phase A: the wall rider ----------------
            var pilot = new LanternPilot29 { hand = new Mulberry32(Seed * 7919u + 13u) };
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = yawDeg * Math.PI / 180;
            int step0 = sim.simStep, frame = 0, deaths = 0;
            int tellFrames = 0, tellGlowFrames = 0, liveFrames = 0, liveGlowFrames = 0;
            double slowMinLive = 1, slowMaxLive = 0;
            bool shotTell = false, shotLive = false;
            while (l.Alive && (sim.simStep - step0) * sim.step < 160.0 && Time.realtimeSinceStartup < wallEnd)
            {
                frame++;
                if (Down27(g, "hulk")) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t =>
                {
                    pilot.Step(sim, yawL, t);
                    yawL = pilot.Move.yaw;
                    return (pilot.Move, pilot.Combat);
                });
                InputFrame mv = drive.mv;
                CombatInput cb = drive.cb;
                // any face, any time the ray lands: the light-damage rule
                Vec3 eye = sim.Eye;
                Vec3 look = AimDir27(mv.yaw, mv.pitch);
                double t0 = l.Raycast(eye, look);
                bool seen = t0 >= 0 && !(sim.world.RayIntersect(eye, look, out RayHit wh) && wh.distance < t0);
                if (seen) { bool tap = frame % 2 == 0; cb.fire = new Btn { held = tap, pressed = tap }; }
                else cb.fire = default;
                g.input.SetLook(mv.yaw, mv.pitch);
                g.forcedMove = mv;
                g.forcedCombat = cb;
                Lantern29 l29 = sim.lantern29;
                if (l29 != null)
                {
                    bool glow = g.boss29View != null && g.boss29View.LavaShown;
                    if (l29.Stage == Lantern29Stage.Tell) { tellFrames++; if (glow) tellGlowFrames++; }
                    if (l29.Stage == Lantern29Stage.Live)
                    {
                        liveFrames++;
                        if (glow) liveGlowFrames++;
                        slowMinLive = Math.Min(slowMinLive, sim.Lantern29SlowScale);
                        slowMaxLive = Math.Max(slowMaxLive, sim.Lantern29SlowScale);
                    }
                    if (!shotTell && l29.Stage == Lantern29Stage.Tell && l29.TellProgress > 0.6) { shotTell = true; yield return Shot29(dir, "lantern-tell", st); shotStats.Add(ShotLine29("tell", st, ci)); }
                    if (!shotLive && l29.Stage == Lantern29Stage.Live && l29.LiveProgress > 0.3) { shotLive = true; yield return Shot29(dir, "lantern-live", st); shotStats.Add(ShotLine29("live", st, ci)); }
                }
                yield return null;
            }
            bool killed = !l.Alive;
            double killSeconds = (sim.simStep - step0) * sim.step;
            Lantern29 a29 = sim.lantern29;
            int fullA = a29 != null ? a29.fullHits : 0, lightA = a29 != null ? a29.lightHits : 0;
            double floorDmgA = a29 != null ? a29.floorDamage : 0;
            if (killed) { yield return Frames(Ms(600)); yield return Shot29(dir, "lantern-after", st); shotStats.Add(ShotLine29("after", st, ci)); }
            // ---------------- phase B: standing on the floor ----------------
            int bFull = -1, bLight = -1, bTicks = 0;
            double bHp0 = 0, bHp1 = 0, bSlow = 1, bFloor = 0;
            bool bStarted = false;
            if (killed && Time.realtimeSinceStartup < wallEnd)
            {
                g.health.Reset();
                sim.downSteps = 0;
                sim.flow = Flow.Playing;
                if (sim.BossRoomBegin("hulk", out spawn, out yawDeg) && sim.lantern27 != null && sim.lantern27.Alive)
                {
                    bStarted = true;
                    l = sim.lantern27;
                    g.Respawn(spawn, yawDeg);
                    int stepB = sim.simStep, fb = 0;
                    // let the intro run (the lantern is unhurtable) before the HP is read
                    while (l.Alive && !l.Hurtable && (sim.simStep - stepB) * sim.step < 30.0) { g.forcedMove = default; g.forcedCombat = new CombatInput { weapon = -1 }; yield return null; }
                    bHp0 = l.HpFrac;
                    int stepFight = sim.simStep;
                    while (l.Alive && (sim.simStep - stepFight) * sim.step < 40.0 && Time.realtimeSinceStartup < wallEnd)
                    {
                        fb++;
                        if (Down27(g, "hulk")) { deaths++; }
                        Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = l.Centre - eye;
                        double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                        int gun = sim.loadout.owned.IndexOf("revolver");
                        bool tap = fb % 2 == 0;
                        g.input.SetLook(wy, wp);
                        g.forcedMove = new InputFrame { yaw = wy, pitch = wp };
                        g.forcedCombat = new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1, fire = new Btn { held = tap, pressed = tap } };
                        if (sim.lantern29 != null && sim.lantern29.Stage == Lantern29Stage.Live) bSlow = Math.Min(bSlow, sim.Lantern29SlowScale);
                        yield return null;
                    }
                    bHp1 = l.HpFrac;
                    if (sim.lantern29 != null) { bFull = sim.lantern29.fullHits; bLight = sim.lantern29.lightHits; bFloor = sim.lantern29.floorDamage; bTicks = sim.lantern29.floorTicks; }
                }
            }
            Quiet(g);
            yield return Frames(Ms(800));
            yield return Stop();
            double slowDesign = g.cfg.Num("design.n29.boss1.slowScale");
            var fails = new List<string>();
            if (!killed) fails.Add($"the wall rider did not kill the lantern in {killSeconds.ToString("0", ci)} s of game time (HP {l.HpFrac.ToString("0.00", ci)}{(Time.realtimeSinceStartup >= wallEnd ? ", wall guard hit" : "")})");
            else if (killSeconds < 45 || killSeconds > 70) fails.Add($"the kill took {killSeconds.ToString("0.0", ci)} s of game time, outside 45-70 s");
            if (killed && downs < 1) fails.Add("the lantern died but no bossDown event came");
            if (fullA < 1) fails.Add("the rider landed no full hit (wall riding with the floor live)");
            if (liveFrames == 0) fails.Add("the floor was never live");
            else if (slowMinLive > slowDesign + 0.05) fails.Add($"the world's slow value was {slowMinLive.ToString("0.00", ci)} at its lowest in the live floor, not about {slowDesign.ToString("0.00", ci)}");
            if (tellFrames == 0) fails.Add("no tell frame");
            else if (tellGlowFrames * 2 < tellFrames) fails.Add($"the floor glow showed in {tellGlowFrames} of {tellFrames} tell frames");
            if (liveFrames > 0 && liveGlowFrames * 2 < liveFrames) fails.Add($"the floor glow showed in {liveGlowFrames} of {liveFrames} live frames");
            if (!bStarted) fails.Add("phase B (the floor-standing fight) did not start");
            else
            {
                if (bFull != 0) fails.Add($"the floor-standing pilot landed {bFull} full hit(s)");
                if (bLight < 1) fails.Add("the floor-standing pilot landed no light hit");
                if (!(bHp1 < bHp0)) fails.Add($"the floor-standing pilot did not move the bar ({bHp0.ToString("0.000", ci)} -> {bHp1.ToString("0.000", ci)})");
                else if (killed && bHp0 > 0)
                {
                    double projected = 40.0 * bHp0 / (bHp0 - bHp1); // game seconds a floor camper would need at this rate (the bar's drop per 40 s, straight-lined)
                    if (projected < 3 * killSeconds) fails.Add($"a floor camper would kill in about {projected.ToString("0", ci)} s, under 3x the rider's {killSeconds.ToString("0.0", ci)} s");
                }
            }
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, LanternPilot29 through forcedMove / forcedCombat, boss HP only READ (never cut): "
                + $"A wall rider: {(killed ? "KILLED" : "NOT killed")} at {killSeconds.ToString("0.0", ci)} s of game time (band 45-70), full hits {fullA}, light hits {lightA}, floor damage {floorDmgA.ToString("0", ci)}, deaths {deaths}; "
                + $"world slow value in the live floor {slowMinLive.ToString("0.00", ci)}..{slowMaxLive.ToString("0.00", ci)} (design {slowDesign.ToString("0.00", ci)}); glow frames: tell {tellGlowFrames} of {tellFrames}, live {liveGlowFrames} of {liveFrames}; "
                + $"B floor-standing 40 s (projected kill {(bStarted && bHp0 > bHp1 ? 40.0 * bHp0 / (bHp0 - bHp1) : 0).ToString("0", ci)} s): bar {bHp0.ToString("0.000", ci)} -> {bHp1.ToString("0.000", ci)}, full hits {bFull}, light hits {bLight}, floor damage {bFloor.ToString("0", ci)} ({bTicks} ticks), lowest slow {bSlow.ToString("0.00", ci)}; shots: {string.Join("; ", shotStats)}";
        }

        // ======================================================= THE GLACIER =======================================================

        static IEnumerator Glacier29Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Glacier28On) { res[0] = "RED: design.n28.glacier.enabled is off (no rink fight) | " + pc; yield break; }
            if (!sim.n29.boss2) { res[0] = "RED: design.n29.boss2 is off in this run (the pulse-only rule applies) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.Glacier28Begin() || sim.glacier28 == null || !sim.glacier28.Alive) { res[0] = "RED: Glacier28Begin did not start the lap fight (no BossKit) | " + pc; yield break; }
            Glacier28 gl = sim.glacier28;
            if (sim.loadout.owned.IndexOf("gauntlet") < 0) sim.loadout.AddWeapon("gauntlet");
            Vec3 spawn = sim.Glacier28Spawn;
            double yawDeg = sim.Glacier28SpawnYawDeg;
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            yield return Frames(20);
            Start(g, "glacier29", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 260;
            int downs = 0;
            offs.Add(g.events.On<BossEvent>("bossDown", e => downs++));
            var pilot = new GlacierPilot29 { hand = new Mulberry32(Seed * 7919u + 13u) };
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = yawDeg * Math.PI / 180;
            int step0 = sim.simStep, deaths = 0, play = 0, locked = 0, frostFrames = 0, enragedFrames = 0, nearFrames = 0, laneLines = 0;
            double gapMin = 1e9, gapMax = -1e9;
            bool shotLane = false, shotFrost = false, shotWin = false;
            double lumMax = -1;
            int lumN = 0;
            var shotStats = new List<string>();
            var st = new double[4];
            while (gl.Alive && (sim.simStep - step0) * sim.step < 240.0 && Time.realtimeSinceStartup < wallEnd)
            {
                if (Down28(g)) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t =>
                {
                    pilot.Step(sim, yawL, t);
                    yawL = pilot.Move.yaw;
                    return (pilot.Move, pilot.Combat);
                });
                InputFrame mv = drive.mv;
                g.input.SetLook(mv.yaw, mv.pitch);
                g.forcedMove = mv;
                g.forcedCombat = drive.cb;
                Glacier29 g29 = sim.glacier29;
                if (gl.phase.StartsWith("p", StringComparison.Ordinal))
                {
                    gapMin = Math.Min(gapMin, gl.gap);
                    gapMax = Math.Max(gapMax, gl.gap);
                    play++;
                    if (g29 != null)
                    {
                        if (g29.lane.Locked) locked++;
                        if (g29.Enraged) enragedFrames++;
                    }
                    if (gl.gap <= g.cfg.Num("design.n29.boss2.catchUpMetres")) nearFrames++;
                }
                if (g.boss29View != null)
                {
                    laneLines = Math.Max(laneLines, g.boss29View.LaneLinesBuilt);
                    if (g.boss29View.FrostShown > 0) frostFrames++;
                }
                if (!shotLane && (sim.simStep - step0) * sim.step > 12) { shotLane = true; yield return Shot29(dir, "glacier-lanes", st); shotStats.Add(ShotLine29("lanes", st, ci)); lumMax = Math.Max(lumMax, st[3]); lumN++; }
                if (!shotFrost && g.boss29View != null && g.boss29View.FrostShown > 0) { shotFrost = true; yield return Shot29(dir, "glacier-frost", st); shotStats.Add(ShotLine29("frost", st, ci)); lumMax = Math.Max(lumMax, st[3]); lumN++; }
                if (!shotWin && gl.WindowOpen) { shotWin = true; yield return Shot29(dir, "glacier-window", st); shotStats.Add(ShotLine29("window", st, ci)); lumMax = Math.Max(lumMax, st[3]); lumN++; }
                yield return null;
            }
            bool killed = !gl.Alive;
            double killSeconds = (sim.simStep - step0) * sim.step;
            Glacier29 f29 = sim.glacier29;
            if (!shotFrost && killed) { yield return Shot29(dir, "glacier-end", st); shotStats.Add(ShotLine29("end", st, ci)); lumMax = Math.Max(lumMax, st[3]); lumN++; }
            Quiet(g);
            yield return Frames(Ms(800));
            yield return Stop();
            int windows = Stat27(gl.stats, "windows");
            var fails = new List<string>();
            if (!killed) fails.Add($"the glacier was not killed in {killSeconds.ToString("0", ci)} s of game time (bar {gl.HpFrac.ToString("0.00", ci)}, phase {gl.phase}{(Time.realtimeSinceStartup >= wallEnd ? ", wall guard hit" : "")})");
            else if (killSeconds < 80 || killSeconds > 120) fails.Add($"the kill took {killSeconds.ToString("0.0", ci)} s of game time, outside 80-120 s");
            if (killed && downs < 1) fails.Add("the glacier died but no bossDown event came");
            if (f29 == null) fails.Add("no Glacier29 rule attached");
            else
            {
                if (f29.frostTells < 1) fails.Add("no frost trail ever fired");
                if (f29.charges < 1) fails.Add("no lane charge ever fired");
                if (f29.exposedWindows < 1) fails.Add("no exposed window opened (never caught up in its lane)");
                if (f29.lightHits + f29.exposedHits < 1) fails.Add("no gun hit landed");
                if (play > 0 && locked * 3 < play) fails.Add($"the lane lock held in only {(100.0 * locked / play).ToString("0", ci)} % of the fight");
            }
            if (laneLines < 3) fails.Add($"the view built {laneLines} lane lines, not 3");
            if (lumN == 0) fails.Add("no screenshot was taken, so the arena's darkness was not judged");
            else if (lumMax >= 0.25) fails.Add($"the arena reads too bright: mid-frame luminance {lumMax.ToString("0.000", ci)} (needs under 0.25; the n29-5 frame was about 0.45)");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, GlacierPilot29 through forcedMove / forcedCombat, boss HP only READ (never cut): "
                + $"{(killed ? "KILLED" : "NOT killed")} at {killSeconds.ToString("0.0", ci)} s of game time (band 80-120), deaths {deaths}; gap {gapMin.ToString("0", ci)}..{gapMax.ToString("0", ci)} m (within {g.cfg.Num("design.n29.boss2.catchUpMetres").ToString("0", ci)} m for {(nearFrames * (double)sim.step).ToString("0.0", ci)} s of frames); "
                + $"exposed windows {(f29 != null ? f29.exposedWindows : 0)}, other-gun hits {(f29 != null ? f29.lightHits : 0)} light + {(f29 != null ? f29.exposedHits : 0)} inside windows, lane lock {(play > 0 ? 100.0 * locked / play : 0).ToString("0", ci)} % of the fight, "
                + $"arena mid-frame luminance, brightest shot {lumMax.ToString("0.000", ci)} (limit 0.25), lane lines built {laneLines}, frost frames {frostFrames} (volleys {(f29 != null ? f29.volleys : 0)}, frost {(f29 != null ? f29.frostTells : 0)}, charges {(f29 != null ? f29.charges : 0)}), enraged {(play > 0 ? 100.0 * enragedFrames / play : 0).ToString("0", ci)} % of the fight; shots: {string.Join("; ", shotStats)}";
        }
    }
}
