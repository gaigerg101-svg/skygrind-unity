// NIGHT47 builder W (reel phase a147; FeelReel.N47.cs's dispatch): THE WICK's three phases, the climb and the glass on frames. Not run by its
// author (no Player in the worktree). Boot level 1 with reel.sh + -skygrindReelOnly (L-062: a CAMPAIGN run; with the build copy's foundry47 on,
// n47-foundry). The sim rows wick47 / climb47 / wickTop47 are forced ON before the run (sim.rowsOverride) unless -skygrindOff47 (or older) is
// given: then the same reel takes the RED frames (NIGHT46's fight). God; input through forcedMove (L-046); every state below is forced so each
// frame is certain. Frames (shot-N29-<name>.png), what each should show (GREEN; the RED run shows NIGHT46's stack and fight instead):
//   W47-p1-entry     from the tunnel's mouth 1.5 s into the fight: the floor (r 9 m) with NO ride panels (folded into the wall), THE WICK low.
//   W47-p1-floor     from the floor 5 m out, looking at THE WICK: the round floor, the hot wall, the bar (P1).
//   W47-p2-panels    the bar pushed to the P1 gate: 0.6 s after the phase change, from the floor looking up the shaft: the panels OUT (cyan trim),
//                    the line "IT'S CLIMBING. GET UP THERE." (one line, nothing else).
//   W47-p2-steps     (round 3) from the floor by the entry: the first two STEPS (plain ledges, cyan lips) the climb starts on.
//   W47-p2-climb     on panel 1's ride face (feet at its catch line), looking along it: the next panel in view (W-cards.md: each segment's start
//                    shows the next hold), THE WICK 4 m above.
//   W47-p3-glass     the feet on the cap: 0.3 s after the seal, looking across the shaft: the GLASS (faint cyan pane, cyan rim) over the shaft.
//   W47-p3-open      THE WICK's core OPEN (forced: coreOpen47), from 6 m on the glass: the yellow core (2 Hz), its ring clock, 3 chevrons.
//   W47-p3-hit       the frame after the core punch (forced Punch from 3 m): the cage flung, the bar down 22 %.
//   W47-p3-launch    0.35 s after the punch: the player in the air, launched back over the glass.
//   W47-p3-land      the landing: on the glass, never in the shaft (the feet over the cap's top).
//   G9 (round 4): W47-g9-intro (0.6 s in: the name card "The Wick. The furnace wakes."), W47-g9-rideparry (a ride-parried ember's two cracked
//   plates, from the floor), W47-g9-finale / -finale-2 (0.25 / 0.9 s after the forced finishing punch: the slowed world, the glass's yellow-white
//   cracks, the slag flaring; wickFinale47 must be on in the build copy's config).
// Line: GREEN when the panels were hidden in P1 and out in P2 (the view's objects), the phase changes came (wick47Phase 2 and 3), the glass was on in
// P3, the open core showed, the punch took 20-25 % of the bar and the launch carried 12-18 m with the landing on the cap (feet >= the cap's top - 0.2);
// RED otherwise (and with the rows off, where the line reports what NIGHT46's fight did).
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
        static partial void N47Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Wick47Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Wick47Look(SurvivalGame g, Vec3 feet, Vec3 lookAt, string dir, string name, double[] st)
        {
            Vec3 eye = new Vec3(feet.x, feet.y + 1.6, feet.z), dv = lookAt - eye;
            double yaw = Math.Atan2(-dv.x, -dv.z), pitch = Math.Atan2(dv.y, Math.Sqrt(dv.x * dv.x + dv.z * dv.z));
            g.Respawn(feet, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            yield return Frames(12);
            yield return Shot29(dir, name, st);
        }

        static IEnumerator Wick47Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            bool force = NightRows.ForceOffFrom > 47;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; Quiet(g); g.setup.mode = RunSetup.Survival; });
            NightRowState overWas = sim.rowsOverride;
            if (force) sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wick47", true).Set("climb47", true).Set("wickTop47", true).Set("wickRideParry47", true).Set("wickIntro47", true); // G9: the two G9 sim rows too (wickFinale47 is a view row: the build copy's config)
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
            Start(g, "wick47", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            int ph2 = 0, ph3 = 0;
            Action offPh = sim.events.On<Wick47Event>("wick47Phase", e => { if (e.phase == 2) ph2++; if (e.phase == 3) ph3++; });
            offs.Add(() => offPh());
            g.Respawn(mouth, 0);
            g.input.SetLook(0, 0);
            g.forcedMove = new InputFrame();
            yield return Frames(10);
            if (!sim.Wick37Begin(out _, out _)) { res[0] = "RED: Wick37Begin returned false"; yield break; }
            Wick37 w = sim.wick37;
            double t0 = sim.SimTime;
            while (sim.SimTime - t0 < 0.6) { God(g); yield return null; }
            yield return Shot29(dir, "W47-g9-intro", st); // G9 idea 2: the name card "The Wick. The furnace wakes."
            string introNow = sim.wick47Line != null ? sim.wick47Line.Now : null;
            while (sim.SimTime - t0 < 1.5) { God(g); yield return null; }
            yield return Shot29(dir, "W47-p1-entry", st);
            bool p1Hidden = !sim.Wick47PanelsOut;
            yield return Wick47Look(g, c + new Vec3(0, 0.05, 5), w.Centre, dir, "W47-p1-floor", st);
            // P2: the bar to the gate
            if (w.w47 && w.p47 != null) w.Apply(w.body.hp - w.body.maxHp * w.p47.gate[0] + 1);
            for (int k = 0; k < 240 && ph2 == 0; k++) { God(g); yield return null; }
            double t2 = sim.SimTime;
            while (sim.SimTime - t2 < 0.6) yield return null;
            yield return Wick47Look(g, c + new Vec3(0, 0.05, 3), c + new Vec3(0, s.p.capY * 0.6, -2), dir, "W47-p2-panels", st);
            bool p2Out = sim.Wick47PanelsOut;
            // round 3: the bottom section's steps (plain ledges, cyan lips), from the floor by the entry, looking at the first two
            if (s.stepTops.Count > 0)
            {
                double sa0 = s.stepAngles[0], sa1 = s.stepAngles[Math.Min(1, s.stepTops.Count - 1)];
                Vec3 st0 = c + new Vec3(Math.Cos(sa0) * 7.5, s.stepTops[0], Math.Sin(sa0) * 7.5), st1 = c + new Vec3(Math.Cos(sa1) * 7.5, s.stepTops[Math.Min(1, s.stepTops.Count - 1)], Math.Sin(sa1) * 7.5);
                yield return Wick47Look(g, c + new Vec3(0, 0.05, 2), (st0 + st1) * 0.5, dir, "W47-p2-steps", st);
            }
            // on panel 1, looking along its ride face (the next panel in view)
            Wick37Panel pn = s.panels[Math.Min(1, s.panels.Count - 1)];
            Vec3 tu = (pn.end - pn.start) * (1 / Math.Max(1e-6, (pn.end - pn.start).Length));
            Vec3 onPanel = c + pn.start + tu * 2.0 + pn.normal * 0.6 + new Vec3(0, 1.0, 0);
            yield return Wick47Look(g, onPanel, onPanel + tu * 8 + new Vec3(0, 3, 0), dir, "W47-p2-climb", st);
            // G9 idea 1: a ride-parried ember (forced: one returning with the ride flag) cracks two plates
            int rp0 = w.rideCracksMade47;
            { Vec3 ec = w.Centre + new Vec3(0, 0, 3.5); w.embers.Add(new Wick37Ember { pos = ec, prev = ec, vel = new Vec3(0, 0, -33), life = 600, alive = true, back = true, ride47 = w.rideParry47 }); }
            for (int k = 0; k < 120 && w.rideCracksMade47 == rp0; k++) { God(g); yield return null; }
            yield return Wick47Look(g, c + new Vec3(0, 0.05, 4), w.Centre, dir, "W47-g9-rideparry", st);
            int cracked = w.CrackedCount;
            // P3: the cap
            double rc = (s.p.radius + s.p.capRadius) / 2;
            Vec3 capAt = c + new Vec3(0, s.p.capY + 0.05, rc);
            g.Respawn(capAt, 0);
            g.forcedMove = new InputFrame();
            for (int k = 0; k < 600 && ph3 == 0; k++) { God(g); yield return null; }
            double t3 = sim.SimTime;
            while (sim.SimTime - t3 < 0.3) yield return null;
            yield return Wick47Look(g, capAt, c + new Vec3(0, s.p.capY, -rc), dir, "W47-p3-glass", st);
            bool glassOn = sim.Wick47GlassOn;
            // the open core, forced, from 6 m on the glass
            Vec3 glassAt = c + new Vec3(0, s.p.capY + 0.05, 6);
            if (w.w47 && w.p47 != null) w.coreOpen47 = w.coreOpenSteps47;
            yield return Wick47Look(g, glassAt, w.Centre, dir, "W47-p3-open", st);
            bool openSeen = w.coreOpen47 > 0;
            // the punch from 3 m: the hit, the launch, the landing
            Vec3 near = w.Centre + new Vec3(0, 0, 3.0);
            near = new Vec3(near.x, c.y + s.p.capY + 0.05, near.z);
            g.Respawn(near, 0);
            if (w.w47 && w.p47 != null) w.coreOpen47 = w.coreOpenSteps47;
            yield return Frames(2);
            double before = w.HpFrac;
            Vec3 eye = sim.Eye, dirv = w.Centre - eye;
            double dl = Math.Max(1e-6, dirv.Length);
            Vec3 from = sim.mover.position;
            bool punched = w.Punch(eye, dirv * (1 / dl), false);
            yield return null;
            yield return Shot29(dir, "W47-p3-hit", st);
            double share = before - w.HpFrac;
            double tl = sim.SimTime;
            while (sim.SimTime - tl < 0.35) yield return null;
            yield return Shot29(dir, "W47-p3-launch", st);
            for (int k = 0; k < 600 && !sim.mover.grounded; k++) yield return null;
            Vec3 land = sim.mover.position;
            double dist = Math.Sqrt((land.x - from.x) * (land.x - from.x) + (land.z - from.z) * (land.z - from.z));
            bool onCap = land.y >= c.y + s.p.capY - 0.2;
            yield return Shot29(dir, "W47-p3-land", st);
            // G9 idea 3: the finisher (forced: the last core punch from 3 m): the slow motion, the glass cracking, the slag flaring (wickFinale47 view row)
            int fin0 = Wick37View.Current != null ? Wick37View.Current.Finales47 : -1;
            if (w.w47 && w.p47 != null && w.Alive)
            {
                w.corePunches47 = Math.Max(w.corePunches47, w.p47.punches - 1);
                w.body.hp = Math.Min(w.body.hp, w.body.maxHp * (w.p47.finishAt + 0.05));
                Vec3 nf = w.Centre + new Vec3(0, 0, 3.0);
                g.Respawn(new Vec3(nf.x, c.y + s.p.capY + 0.05, nf.z), 0);
                w.coreOpen47 = w.coreOpenSteps47;
                yield return Frames(2);
                Vec3 e2 = sim.Eye, d2 = w.Centre - e2;
                w.Punch(e2, d2 * (1 / Math.Max(1e-6, d2.Length)), false);
                double tfin = sim.SimTime;
                while (sim.SimTime - tfin < 0.25) yield return null;
                yield return Shot29(dir, "W47-g9-finale", st);
                while (sim.SimTime - tfin < 0.9) yield return null;
                yield return Shot29(dir, "W47-g9-finale-2", st);
            }
            int finales = Wick37View.Current != null ? Wick37View.Current.Finales47 - fin0 : -1;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            bool ok = force && p1Hidden && p2Out && ph2 > 0 && ph3 > 0 && glassOn && openSeen && punched && share >= 0.20 - 1e-6 && share <= 0.25 + 1e-6 && dist >= 12 && dist <= 18 && onCap;
            string g9 = $" | G9: intro line \"{introNow}\"; ride-parry cracks {w.rideCracksMade47 - rp0} (plates cracked {cracked}); finales drawn {finales} (wickFinale47 view row {NightRows.ViewOn(g.cfg, "wickFinale47")})";
            res[0] = $"{(ok ? "GREEN" : "RED")}{g9}: rows {(force ? "forced on" : "off (the RED run)")}, map {g.MapId}, w47 {w.w47}; P1 panels hidden {p1Hidden}; P2 panels out {p2Out} (wick47Phase 2 x{ph2}); P3 glass {glassOn} (wick47Phase 3 x{ph3}); open {openSeen}; punch {punched} took {(share * 100).ToString("0.0", ci)} % of the bar; launch {dist.ToString("0.0", ci)} m, landed at y {(land.y - c.y).ToString("0.00", ci)} (cap {s.p.capY.ToString("0.0", ci)}): on the cap {onCap}";
        }
    }
}
