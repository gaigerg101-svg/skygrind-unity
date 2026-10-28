// NIGHT27 (builder S): the Player scenarios of the two bosses (the lead's FeelReel.N27.cs calls the hooks; the bodies live here).
//   lantern     THE LANTERN: the fight starts through the campaign's own call (BossRoomBegin "hulk" routes to the lantern with design.n27.boss1 on). The scripted pilot
//               (Game/Bot/LanternPilot27.cs, the harness's) runs the ring, jumps into the wall (the Mover starts the ride by its own rule) and shoots the lit face: its Move /
//               Combat go in through g.forcedMove / g.forcedCombat / g.input.SetLook, the entries every reel scenario uses (the pilot is stepped once per SIM step and the
//               frame's presses are merged: it was written for the fixed step). The shield is measured twice on the way: RED frames (not riding: the ray lands on the box,
//               the shield is up) with two deliberate pistol probe taps that must be BLOCKED, and GREEN frames (riding the wall that faces the box: shield down, hits land).
//   glacier27   THE GLACIER: BossRoomBegin "glacier" (design.n27.boss2). The harness's GlacierPilot27 runs the ice and dodges the lane attacks (Move); its Combat is replaced by
//               a pulse27 overlay that uses the SAME buttons a player has (LT press with a full glove inside the blast's reach, else the 2.5 s swing): the pilot's own launch was
//               written for NIGHT26's Fist24 flick. The glove starts FULL (5 pips: the ice has no kills; the harness banks the charge the same way) and a hit gives 5 pips back.
// NO HP CUT anywhere: the scenarios only READ boss HP. A PASS needs the boss dead by the pilot's own hits (the boss's alive flag and a bossDown event). Every loop is
// bounded by GAME time (the sim's steps) and by a wall-clock guard; no dictionary is read except the boss's own stats (all created at its first step; read with TryGetValue).
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
        static partial void N27LanternBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Lantern27Scenario(g, dir, res, offs, pre);

        static partial void N27Glacier27Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Glacier27Scenario(g, dir, res, offs, pre);

        static int Stat27(Dictionary<string, int> d, string k) => d != null && d.TryGetValue(k, out int v) ? v : 0;

        /// <summary>Runs a per-sim-step pilot from the render loop: one call per sim step that passed, the frame's presses merged, the held states the last call's.</summary>
        sealed class PilotDrive27
        {
            public InputFrame mv;
            public CombatInput cb = new CombatInput { weapon = -1 };
            public int tick;

            public void Advance(int now, Func<int, (InputFrame, CombatInput)> stepper)
            {
                bool first = true;
                InputFrame am = default;
                CombatInput ac = default;
                while (tick < now)
                {
                    tick++;
                    var (m, c) = stepper(tick);
                    if (first) { am = m; ac = c; first = false; continue; }
                    m.jump.pressed |= am.jump.pressed;
                    m.slide.pressed |= am.slide.pressed;
                    m.dash.pressed |= am.dash.pressed; // NIGHT28 B2: the lap fight's pilot dashes
                    m.sprint.pressed |= am.sprint.pressed;
                    c.fire.pressed |= ac.fire.pressed;
                    c.altFire.pressed |= ac.altFire.pressed;
                    if (c.weapon < 0) c.weapon = ac.weapon;
                    am = m;
                    ac = c;
                }
                if (!first) { mv = am; cb = ac; return; }
                mv.jump.pressed = mv.slide.pressed = mv.sprint.pressed = false; // no step passed (a hitstop frame): held stays, presses do not repeat
                cb.fire.pressed = cb.altFire.pressed = false;
                cb.weapon = -1;
            }
        }

        static Vec3 AimDir27(double yaw, double pitch)
        {
            Vec3 fw = MovementMath.WishDirection(0, 1, yaw);
            double cp = JsMath.Cos(pitch);
            return new Vec3(fw.x * cp, JsMath.Sin(pitch), fw.z * cp);
        }

        /// <summary>A death in the boss room: what Campaign24.Retry does (the room's begin again: back to the phase's start; the boss keeps its hits).</summary>
        static bool Down27(SurvivalGame g, string bossId)
        {
            SurvivalSim sim = g.sim;
            if (!(g.health.Dead || sim.flow == Flow.Down)) return false;
            g.health.Reset();
            sim.downSteps = 0;
            sim.flow = Flow.Playing;
            sim.BossRoomBegin(bossId, out Vec3 sp, out double yw);
            g.Respawn(sp, yw);
            return true;
        }

        static IEnumerator Lantern27Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Lantern27On) { res[0] = "RED: design.n27.boss1 is off (the plan's boss 'hulk' is NIGHT26's Hulk, no lantern) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yawDeg) || sim.lantern27 == null || !sim.lantern27.Alive) { res[0] = "RED: BossRoomBegin(hulk) did not start the lantern (arena file missing, no BossKit, or the row's route) | " + pc; yield break; }
            Lantern27 l = sim.lantern27;
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            yield return Frames(20);
            Start(g, "lantern", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 150;
            int downs = 0;
            offs.Add(g.events.On<BossEvent>("bossDown", e => downs++));
            var pilot = new LanternPilot27 { hand = new Mulberry32(Seed * 7919u + 13u) };
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = yawDeg * Math.PI / 180;
            int step0 = sim.simStep, frame = 0, deaths = 0, retries0 = sim.lantern27Retries;
            int redSamples = 0, redWrong = 0, greenSamples = 0, greenDown = 0, rideOther = 0, graceFrames = 0, downInGrace = 0, lastRideStep = -1000000;
            double graceS = l.p.shieldGrace; // design.n27.boss1.shield.grace
            int probes = 0, probeAt = -1, probeBlocks0 = 0, probeLit0 = 0, probeBlocked = 0, probeLanded = 0;
            bool markedRed = false, markedGreen = false;
            string lastPhase = "";
            var phases = new List<string>();
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
                if (cb.fire.held) { bool tap = frame % 2 == 0; cb.fire = new Btn { held = tap, pressed = tap }; } // taps, not a held trigger (the RT charge would build on a held one)
                // the shield, measured on the look the pilot has now: a ray at the box from the eye
                Vec3 eye = sim.Eye;
                Vec3 look = AimDir27(mv.yaw, mv.pitch);
                double t0 = l.Raycast(eye, look);
                bool seen = t0 >= 0 && !(sim.world.RayIntersect(eye, look, out RayHit wh) && wh.distance < t0);
                bool riding = sim.mover.state == MoveState.WallRide;
                if (riding) lastRideStep = sim.simStep;
                bool inGrace = !riding && (sim.simStep - lastRideStep) * sim.step <= graceS + 3.0 / UnityTuning.ReelFps; // by design the shield stays down shieldGrace s after a ride
                if (seen && l.Hurtable)
                {
                    bool down = l.ShieldDownOn(l.hitFace);
                    if (inGrace)
                    {
                        graceFrames++;
                        if (down) downInGrace++;
                    }
                    else if (!riding)
                    {
                        redSamples++;
                        if (down) redWrong++;
                        if (!markedRed) { markedRed = true; FeelProbe.Event("lantern:shieldUp"); }
                        // two deliberate probe shots of the pistol, one tap each, while the shield is up: both must be blocked and none may land
                        if (probes < 2 && probeAt < 0 && !cb.fire.held && sim.loadout.Weapon == "revolver" && frame > 120)
                        {
                            probes++;
                            probeAt = frame;
                            probeBlocks0 = Stat27(l.stats, "shieldBlocks");
                            probeLit0 = Stat27(l.stats, "litHits");
                            cb.fire = new Btn { held = true, pressed = true };
                        }
                    }
                    else if (down)
                    {
                        greenSamples++;
                        greenDown++;
                        if (!markedGreen) { markedGreen = true; FeelProbe.Event("lantern:rideLit"); }
                    }
                    else rideOther++;
                }
                if (probeAt >= 0 && frame >= probeAt + 12)
                {
                    probeBlocked += Stat27(l.stats, "shieldBlocks") - probeBlocks0;
                    probeLanded += Stat27(l.stats, "litHits") - probeLit0;
                    probeAt = -1;
                }
                g.input.SetLook(mv.yaw, mv.pitch);
                g.forcedMove = mv;
                g.forcedCombat = cb;
                if (l.phase != lastPhase) { lastPhase = l.phase; if (l.phase.StartsWith("p", StringComparison.Ordinal)) phases.Add($"{l.phase}@{(sim.simStep - step0) * sim.step:0}s"); }
                yield return null;
            }
            bool killed = !l.Alive;
            double killSeconds = (sim.simStep - step0) * sim.step;
            Quiet(g);
            yield return Frames(Ms(800));
            yield return Stop();
            var fails = new List<string>();
            if (!killed) fails.Add($"the lantern was not killed in {killSeconds.ToString("0", ci)} s of game time (HP {l.HpFrac.ToString("0.00", ci)}, phase {l.phase}{(Time.realtimeSinceStartup >= wallEnd ? ", wall guard hit" : "")})");
            if (killed && downs < 1) fails.Add("the lantern died but no bossDown event came");
            if (redSamples == 0) fails.Add("no RED frame: the pilot never had the box in the ray while not riding");
            if (redWrong > 0) fails.Add($"the shield was DOWN in {redWrong} of {redSamples} frames with the ray on the box and no wall ride");
            if (greenSamples == 0) fails.Add("no GREEN frame: never riding the wall of the lit face with the ray on the box");
            if (probes == 0) fails.Add("no probe shot was fired (pistol not in hand or no RED frame)");
            if (probeLanded > 0) fails.Add($"{probeLanded} probe shot(s) LANDED with the shield up");
            if (probes > 0 && probeBlocked < 1) fails.Add("no probe shot was counted as blocked");
            int rides = pilot.rides, litHits = Stat27(l.stats, "litHits"), blocks = Stat27(l.stats, "shieldBlocks");
            if (litHits < 1) fails.Add("no hit landed on the lit face");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, pilot through forcedMove / forcedCombat, boss HP only READ (never cut): "
                + $"RED frames (ray on the box, not riding for more than the {graceS.ToString("0.00", ci)} s shield grace + 3 frames): {redSamples}, shield down in {redWrong}; inside the grace (by design): {graceFrames} frames, shield down in {downInGrace};{probes} probe pistol shot(s): {probeBlocked} blocked, {probeLanded} landed (shieldBlocks {blocks}); "
                + $"GREEN frames (riding the face's wall): {greenSamples}, shield down in {greenDown} (riding a different wall: {rideOther}); wall rides {rides} (jumps {pilot.jumps}), lit hits {litHits}; "
                + $"{(killed ? "KILLED" : "NOT killed")} at {killSeconds.ToString("0.0", ci)} s of game time, deaths {deaths}, phase retries {sim.lantern27Retries - retries0}, bossDown x{downs}; {string.Join(" ", phases)}";
        }

        static IEnumerator Glacier27Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Glacier27On) { res[0] = "RED: design.n27.boss2 is off (the plan's boss 'glacier' is NIGHT26's glacier) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.BossRoomBegin("glacier", out Vec3 spawn, out double yawDeg) || sim.glacier27 == null || !sim.glacier27.Alive) { res[0] = "RED: BossRoomBegin(glacier) did not start the chase (course file missing or no BossKit) | " + pc; yield break; }
            Glacier27 gl = sim.glacier27;
            if (sim.loadout.owned.IndexOf("gauntlet") < 0) sim.loadout.AddWeapon("gauntlet");
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            PulseCharge27 p27 = sim.pulse27;
            bool live = sim.n27.pulse27 && p27 != null && p27.active;
            int banked = 0;
            if (live) while (p27.pips < p27.p.pips) { p27.AddKill(); banked++; } // the glove arrives full (the ice has no kills; the harness banks the charge the same way): stated in the line
            yield return Frames(20);
            Start(g, "glacier27", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 170;
            int downs = 0;
            offs.Add(g.events.On<BossEvent>("bossDown", e => downs++));
            var pilot = new GlacierPilot27 { hand = new Mulberry32(Seed * 7919u + 13u), noLaunch = true }; // the pilot's own launch is NIGHT26's Fist24 flick: the overlay below is the player's LT
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = yawDeg * Math.PI / 180;
            int step0 = sim.simStep, deaths = 0, retries0 = sim.glacier27Retries, launches = 0, swings = 0, ltFrames = 0, swingFrame = -1000, frame = 0;
            double gapMin = 1e9, gapMax = -1e9;
            bool markedWin = false, markedHit = false;
            int hits0 = 0;
            string lastPhase = "";
            var phases = new List<string>();
            while (gl.Alive && (sim.simStep - step0) * sim.step < 170.0 && Time.realtimeSinceStartup < wallEnd)
            {
                frame++;
                if (Down27(g, "glacier")) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t =>
                {
                    pilot.Step(sim, yawL, t);
                    yawL = pilot.Move.yaw;
                    return (pilot.Move, pilot.Combat);
                });
                InputFrame mv = drive.mv;
                CombatInput cb = drive.cb;
                cb.altFire = default; // the pilot's own LT is replaced by the overlay
                Vec3 eye = sim.Eye;
                Vec3 look = AimDir27(mv.yaw, mv.pitch);
                bool inHand = sim.loadout.Weapon == "gauntlet";
                bool win = gl.WindowOpen;
                if (win && !markedWin) { markedWin = true; FeelProbe.Event("glacier27:window"); }
                double rayT = gl.Raycast(eye, look);
                if (live && inHand && win && rayT >= 0)
                {
                    Vec3 rel = gl.body.AimPoint - eye;
                    double along = Vec3.Dot(rel, look);
                    double off = (rel - look * along).Length;
                    bool inBlast = along >= 0 && along <= p27.p.blastRange && off <= p27.p.blastRadius + gl.body.Radius;
                    if (ltFrames > 0) { ltFrames--; cb.altFire = new Btn { held = ltFrames > 0 }; } // LT held a few frames, then let go (the blast itself is the press)
                    else if (p27.Ready && inBlast)
                    {
                        ltFrames = 4;
                        launches++;
                        cb.altFire = new Btn { held = true, pressed = true };
                    }
                    else if (rayT <= 2.6 && frame - swingFrame > 10) { swings++; swingFrame = frame; cb.fire = new Btn { held = true, pressed = true }; } // the heavy swing on a body in reach
                }
                else if (ltFrames > 0) { ltFrames--; cb.altFire = new Btn { held = ltFrames > 0 }; }
                if (gl.phase.StartsWith("p", StringComparison.Ordinal)) { gapMin = Math.Min(gapMin, gl.gap); gapMax = Math.Max(gapMax, gl.gap); }
                if (!markedHit && gl.hits > hits0) { markedHit = true; FeelProbe.Event("glacier27:pulseHit"); }
                g.input.SetLook(mv.yaw, mv.pitch);
                g.forcedMove = mv;
                g.forcedCombat = cb;
                if (gl.phase != lastPhase) { lastPhase = gl.phase; if (gl.phase.StartsWith("p", StringComparison.Ordinal)) phases.Add($"{gl.phase}@{(sim.simStep - step0) * sim.step:0}s"); }
                yield return null;
            }
            bool killed = !gl.Alive;
            double killSeconds = (sim.simStep - step0) * sim.step;
            Quiet(g);
            yield return Frames(Ms(800));
            yield return Stop();
            int windows = Stat27(gl.stats, "windows"), misses = Stat27(gl.stats, "misses"), pulseHits = Stat27(gl.stats, "pulseHits"), blocked = Stat27(gl.stats, "blocked"), softFails = Stat27(gl.stats, "softFails");
            var fails = new List<string>();
            if (!live) fails.Add("design.n27.pulse27 is off: no glove to launch with (the overlay needs it)");
            if (!killed) fails.Add($"the glacier was not killed in {killSeconds.ToString("0", ci)} s of game time ({gl.hits} of 3 pulse hits, phase {gl.phase}{(Time.realtimeSinceStartup >= wallEnd ? ", wall guard hit" : "")})");
            if (killed && downs < 1) fails.Add("the glacier died but no bossDown event came");
            if (killed && pulseHits < 3) fails.Add($"killed by {pulseHits} pulse hits, not 3");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, pilot Move through forcedMove (LT / swing through forcedCombat), boss HP only READ (never cut), glove banked {banked} pips at the start: "
                + $"gap {gapMin.ToString("0", ci)}..{gapMax.ToString("0", ci)} m; windows {windows}, PULSE hits {pulseHits} ({launches} LT blasts, {swings} swings), misses {misses}, gun/out-of-window hits blocked {blocked}, soft fails {softFails}; "
                + $"{(killed ? "KILLED" : "NOT killed")} at {killSeconds.ToString("0.0", ci)} s of game time, deaths {deaths}, retries {sim.glacier27Retries - retries0}, bossDown x{downs}; {string.Join(" ", phases)}";
        }
    }
}
