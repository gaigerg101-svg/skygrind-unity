// NIGHT32 builder C: the reel scenario `boss5` (THE CROWN, design.n32.boss5), declared in the lead's FeelReel.N27.cs (PhaseN32C / N32CrownBody). Not run by its author (no Editor,
// no Player for builders): the lead runs it in the Player.
//   boss5   THE CROWN fought by Crown32Pilot (the harness crown32's pilot, mortal: a death is Crown32Begin's retry, the bar back to the beat's start) until the kill or 180 s, started
//           through SurvivalSim.Crown32Begin (the routing patch Tools/night/n32-crown-wire.cjs is not needed for the reel). Frames: beat 1 (the beam's tell, the beam), beat 2 (a sector
//           glowing / down, the low orbiting core), beat 3 (the charge line, the slam stun with the cyan plates), the mortar mark if one is seen. The line counts what the view drew
//           against what the sim was doing (Crown32View: beam frames, cyan plates, glowing sectors, the mark, the charge line).
// PASS: killed in the band's reach (<= 100 s, the harness p90) and every tell the sim ran was drawn on every frame it ran. RED (row off): "design.n32.boss5 is off".
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
        static partial void N32CrownBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "boss5") body = Boss5Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Shot32C(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N32-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }

        /// <summary>A death in the ring: the room's retry (Crown32Begin keeps the fight, the bar back to the beat's start).</summary>
        static bool Down32C(SurvivalGame g)
        {
            SurvivalSim sim = g.sim;
            if (!(g.health.Dead || sim.flow == Flow.Down)) return false;
            g.health.Reset();
            sim.downSteps = 0;
            sim.flow = Flow.Playing;
            sim.Crown32Begin(out Vec3 sp, out double yw);
            g.Respawn(sp, yw);
            return true;
        }

        static IEnumerator Boss5Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.n32.boss5) { res[0] = "RED: design.n32.boss5 is off in this run (no level-5 boss) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (sim.crown32 != null && sim.crown32.Alive) sim.crown32.Clear();
            g.health.Reset();
            sim.downSteps = 0;
            sim.flow = Flow.Playing;
            if (!sim.Crown32Begin(out Vec3 spawn, out double yawDeg) || sim.crown32 == null || !sim.crown32.Alive) { res[0] = "RED: Crown32Begin did not start THE CROWN | " + pc; yield break; }
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            foreach (string gun in new[] { "gauntlet", "shotgun", "nailgun" }) if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
            yield return Frames(20);
            Start(g, "boss5", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 420;
            var shots = new List<string>();
            var st = new double[4];
            Crown32 cr = sim.crown32;
            var pilot = new Crown32Pilot((uint)sim.runSeed);
            var drive = new PilotDrive27 { tick = sim.simStep };
            int step0 = sim.simStep, deaths = 0, frames = 0;
            int beamF = 0, beamDrawn = 0, plateF = 0, plateDrawn = 0, dropF = 0, dropDrawn = 0, markF = 0, markDrawn = 0, lineF = 0, lineDrawn = 0, orbF = 0, orbDrawn = 0;
            bool shotBeam = false, shotDrop = false, shotLine = false, shotStun = false, shotMark = false, shotB2 = false;
            while (cr.Alive && (sim.simStep - step0) * sim.step < 180.0 && Time.realtimeSinceStartup < wallEnd)
            {
                frames++;
                if (Down32C(g)) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t => { pilot.Step(sim, t); return (pilot.Move, pilot.Combat); });
                g.input.SetLook(drive.mv.yaw, drive.mv.pitch);
                g.forcedMove = drive.mv;
                g.forcedCombat = drive.cb;
                Crown32View v = Crown32View.Current;
                if (cr.Sweeping || cr.SweepTelling) { beamF++; if (v != null && v.BeamShown) beamDrawn++; }
                if (cr.PlatesOpen) { plateF++; if (v != null && v.PlatesLit == cr.p.plateCount) plateDrawn++; }
                if ((cr.downMask | cr.dropTellMask) != 0) { dropF++; if (v != null && v.SectorsGlowing > 0) dropDrawn++; }
                if (cr.mortarPending) { markF++; if (v != null && v.MarkShown) markDrawn++; }
                if (cr.ChargeTelling) { lineF++; if (v != null && v.LineShown) lineDrawn++; }
                int liveOrbs = 0;
                foreach (Crown32Orb ob in cr.orbs) if (ob.alive) liveOrbs++;
                if (liveOrbs > 0) { orbF++; if (v != null && v.OrbsShown == liveOrbs) orbDrawn++; }
                if (!shotBeam && cr.Sweeping && cr.ActProgress > 0.3) { shotBeam = true; yield return Shot32C(dir, "crown-beam", st, shots, ci); }
                if (!shotB2 && cr.beat == 2 && cr.Hurtable && cr.phaseSteps > 0 && cr.dropState == 0) { shotB2 = true; yield return Shot32C(dir, "crown-beat2", st, shots, ci); }
                if (!shotDrop && cr.downMask != 0) { shotDrop = true; yield return Shot32C(dir, "crown-drop", st, shots, ci); }
                if (!shotLine && cr.ChargeTelling && cr.ActProgress > 0.5) { shotLine = true; yield return Shot32C(dir, "crown-charge-line", st, shots, ci); }
                if (!shotStun && cr.Stunned && cr.actSteps > 30) { shotStun = true; yield return Shot32C(dir, "crown-stun", st, shots, ci); }
                if (!shotMark && cr.mortarPending) { shotMark = true; yield return Shot32C(dir, "crown-mortar", st, shots, ci); }
                yield return null;
            }
            bool killed = !cr.Alive || cr.phase == "dying";
            double killS = (sim.simStep - step0) * sim.step;
            bool drawn = beamF > 0 && beamDrawn == beamF && plateDrawn == plateF && dropDrawn == dropF && markDrawn == markF && lineDrawn == lineF && orbDrawn == orbF;
            bool pass = killed && killS <= 100 && drawn;
            res[0] = $"{(pass ? "PASS" : "RED")}: boss5 THE CROWN {(killed ? "KILLED at " + killS.ToString("0.0", ci) + " s" : "NOT killed in " + killS.ToString("0", ci) + " s (bar " + cr.HpFrac.ToString("0.00", ci) + ", beat " + cr.beat + ")")} (harness band p50 50-75 s, p90 <= 100 s), deaths {deaths}; sweeps {cr.sweeps} ({cr.sweepHits} hit), orbs {cr.orbsFired} ({cr.orbHits} hit, parried {cr.parryHits}), mortars {cr.mortars} ({cr.mortarHits} hit), drops {cr.drops} ({cr.lavaHits} burns), charges {cr.charges} ({cr.chargeHits} hit, slams {cr.slams}), hits core {cr.coreHits} / plates {cr.plateHits} / blocked {cr.blockedHits}; frames {frames}: beam {beamF} (drawn {beamDrawn}), plates open {plateF} (all cyan {plateDrawn}), drop ring {dropF} (glowing {dropDrawn}), mortar mark {markF} (drawn {markDrawn}), charge line {lineF} (drawn {lineDrawn}), orbs {orbF} (all drawn {orbDrawn}); pilot jumps {pilot.jumps}, parries {pilot.parries}, dodges {pilot.dodges} | frames {string.Join(", ", shots)} | " + pc;
        }
    }
}
