// NIGHT54 builder W (reel phase a454; FeelReel.N54.cs's dispatch): THE WICK's fight in the Player with wick54 (no FLARE, the easy encounter).
// Not run by its author (no Player in the worktree). Boot level 1 with reel.sh + -skygrindReelOnly (L-062: a CAMPAIGN run, n52-foundry):
//   bash Tools/night/n47-reels.sh <tag> w1on,n52-foundry,a454 w1off,n52-foundry,a454,-skygrindOff54
// The sim row wick54 is forced ON before the run (sim.rowsOverride: the config's rows as they ship + wick54) unless -skygrindOff54 (or older)
// is given: then the same reel fights NIGHT53's WICK (the RED run). God; the input is Wick37Pilot as the sim's input, one pilot step per SIM
// step (Wick44ProofInput, as the wick37 reel and the harness run it; L-046 / L-053). The fight runs to the kill or 120 s of sim time.
// Frames (shot-N29-<name>.png): W54-p1 (2 s in: THE WICK low over the floor), W54-p2 (1 s after P2 began: the panels out), W54-p3 (1 s after
// the cap), W54-open (0.3 s into the first crack-open: the yellow core), W54-kill (0.5 s after the kill).
// Line (L-089: it prints that its input happened): the pilot's steps / rides / leaps, the shots and punches; the FLARE count (tells by the
// wick37Flare event and the WICK's own counter; 0 with the row on), every attack's tells by id, the phases reached, the crack-opens and core
// punches, the kill time. GREEN when the row is on (w54), the input happened (pilot steps > 0 and shots + punches > 0), the flares are 0, the
// fight progressed (P2 reached, or killed) and at least two attack ids told; RED otherwise (and with -skygrindOff54: the line reports what
// NIGHT53's fight did, its FLARE count among it).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N54Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Wick54Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Wick54Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            res[0] = "RED: a454 began, the fight not reached";
            SurvivalSim sim = g.sim;
            bool force = NightRows.ForceOffFrom > 54;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; Quiet(g); g.setup.mode = RunSetup.Survival; });
            NightRowState overWas = sim.rowsOverride;
            if (force) sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wick54", true);
            offs.Add(() => { g.sim.rowsOverride = overWas; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(240);
            sim = g.sim;
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            DoorFree41(g);
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            if (!sim.wick37Centre.HasValue) sim.Wick37Place(new Vec3(2600, 0, 0));
            if (!sim.wick37Centre.HasValue || sim.wick37Stack == null) { res[0] = "RED: no stack (the wick37 row off?)"; yield break; }
            Vec3 c = sim.wick37Centre.Value;
            Wick37Stack s = sim.wick37Stack;
            Vec3 mouth = c + new Vec3(0, 0.05, s.p.radius + 1.0);
            Start(g, "wick54", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            // the counts (the events, so the RED run counts NIGHT53's FLARE the same way)
            int flareEv = 0, fires = 0, punches = 0, opens = 0;
            var told = new Dictionary<string, int>();
            Action offFl = sim.events.On<Wick37Fx>("wick37Flare", e => { if (e.stage == "tell") flareEv++; });
            Action offTell = sim.events.On<Boss37Event>(Boss37Brain.EvTell, e => { if (e.boss != "wick" || e.attack == null) return; told.TryGetValue(e.attack, out int n); told[e.attack] = n + 1; });
            Action offFire = sim.events.On("fire", _ => fires++);
            Action offPunch = sim.events.On("punch", _ => punches++);
            Action offOpen = sim.events.On<Wick47Event>("wick47CrackOpen", _ => opens++);
            offs.Add(() => { offFl(); offTell(); offFire(); offPunch(); offOpen(); });
            g.Respawn(mouth, 0);
            g.input.SetLook(0, 0);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0 };
            yield return Frames(20);
            sim.Wick37Reset(); // a campaign run may have begun it: a fresh fight
            if (!sim.Wick37Begin(out _, out _)) { yield return Stop(); res[0] = "RED: Wick37Begin returned false (the wick37 row off?)"; yield break; }
            Wick37 w = sim.wick37;
            var pilot = new Wick37Pilot { hand = new Mulberry32(1u * 7919u + 13u) }; // the harness's seed-1 hand (n47human wick seed 1)
            IPlayerInput prevInput = sim.input;
            var pilotIn = new Wick44ProofInput(sim, pilot, prevInput);
            g.forcedMove = null;
            g.forcedCombat = null;
            sim.UseInput(pilotIn);
            offs.Add(() => { if (sim.input == pilotIn) sim.UseInput(prevInput); });
            double t0 = sim.SimTime, p2At = -1, p3At = -1, openAt = -1, killAt = -1;
            bool p1Shot = false, p2Shot = false, p3Shot = false, openShot = false, killShot = false;
            double wallEnd = Time.realtimeSinceStartup + 200;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 120)
            {
                God(g);
                double now = sim.SimTime - t0;
                Boss37Brain b = w.brain;
                if (b != null && w.Alive)
                {
                    if (b.leg >= 2 && p2At < 0) p2At = now;
                    if (b.leg >= 3 && p3At < 0) p3At = now;
                    if (opens > 0 && openAt < 0) openAt = now;
                }
                if (!w.Alive && killAt < 0) killAt = now;
                if (!p1Shot && now >= 2.0) { p1Shot = true; yield return Shot29(dir, "W54-p1", st); continue; }
                if (!p2Shot && p2At >= 0 && now - p2At >= 1.0) { p2Shot = true; yield return Shot29(dir, "W54-p2", st); continue; }
                if (!p3Shot && p3At >= 0 && now - p3At >= 1.0) { p3Shot = true; yield return Shot29(dir, "W54-p3", st); continue; }
                if (!openShot && openAt >= 0 && now - openAt >= 0.3) { openShot = true; yield return Shot29(dir, "W54-open", st); continue; }
                if (!killShot && killAt >= 0 && now - killAt >= 0.5) { killShot = true; yield return Shot29(dir, "W54-kill", st); break; }
                yield return null;
            }
            if (sim.input == pilotIn) sim.UseInput(prevInput);
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            int flares = Math.Max(flareEv, w.flareTells54);
            bool input = pilotIn.piloted > 0 && fires + punches > 0;
            bool progressed = p2At >= 0 || killAt >= 0;
            bool ok = force && w.w54 && input && flares == 0 && progressed && told.Count >= 2;
            string attacks = told.Count == 0 ? "none" : string.Join(", ", told.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Key + " x" + kv.Value));
            string T(double t) => t < 0 ? "never" : t.ToString("0.0", ci) + " s";
            res[0] = $"{(ok ? "GREEN" : "RED")}: rows {(force ? "wick54 forced on" : "off (the RED run)")}, map {g.MapId}, w47 {w.w47}, w54 {w.w54}; input: pilot steps {pilotIn.piloted}, rides {pilot.rides}, leaps {pilot.leaps}, shots {fires}, punches {punches}; " +
                $"FLARES {flares} (wick37Flare tells {flareEv}, the WICK's count {w.flareTells54}, burns {w.flareBurns54}, hits {w.flareHits}); attacks told [{attacks}]; P2 at {T(p2At)}, P3 at {T(p3At)}; " +
                $"crack-opens {opens}, core punches {w.corePunches47}; {(w.Alive ? "alive at " + T(sim.SimTime - t0) + " (bar " + (w.HpFrac * 100).ToString("0", ci) + " %)" : "killed at " + T(killAt))}; pool {w.body.maxHp.ToString("0", ci)}";
        }
    }
}
