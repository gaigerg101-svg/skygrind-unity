// NIGHT55 builder C (camera and look): the a255 phase (FeelReel.N55.cs routes it here): the Player proof of the view rows step55 (and, in later
// slices, ssao55, decor55, fov55). Not run by its author (builders never run a Player). A campaign level boots only via reel.sh (L-062), e.g.:
//   bash Tools/night/n47-reels.sh <tag> c1k,n52-kiln,a255                      (the build copy's config with design.n55.step55 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> c1koff,n52-kiln,a255,-skygrindOff55    (NIGHT54's camera: move54's Move54Step; RED, its numbers printed)
// (n52-foundry / n52-iceline the same: `n55look step` found no step line on them, so the step part prints "no step on this level".)
// STEP: the CAMPAIGN on the booted map, DOORFREE, God, husks cleared and frozen, the player's own input back (nothing pressed but the reel's).
// Move55Lines' scan over every non-boss room (the harness's scan); the best line (a step down first, else up); the player put at its start and
// run along it (the stick forward), then back. Each rendered frame: Camera.main's height, the drawn feet (g.Move54DrawnFeet), grounded. It
// prints the input first (L-089: the line, the step's height, metres run, the step crossed: the feet's largest one-frame change near it and
// the step55 smoother's held count), then the camera's largest one-frame height change within Step55StepWindowS of the step against the
// feet's (x share), and the flat-ground noise (the camera's largest one-frame height change on grounded frames Step55FlatAwayS from any step).
// GREEN: the row on, the step crossed, camera / feet <= Step55ReelOnFeetShare. One line "GREEN a255 LOOK | ..." (or RED); "[a255] ..." in the log.
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
        static partial void N55Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = N55LookScenario(g, dir, res, offs, pre);

        static IEnumerator N55LookScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            offs.Add(() =>
            {
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a255 LOOK: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 240;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 220) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput);
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            sim.husks.KillAll();
            string map = PostFx.LevelIdOf(g.map.json);
            Start(g, "a255-look", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            var parts = new List<string>();
            var verdicts = new List<bool>();
            var stepOut = new string[2];
            yield return N55StepPart(g, stepOut);
            parts.Add(stepOut[0]);
            verdicts.Add(stepOut[1] == "GREEN");
            var ssaoOut = new string[2];
            yield return N55SsaoPart(g, dir, ssaoOut);
            parts.Add(ssaoOut[0]);
            verdicts.Add(ssaoOut[1] == "GREEN");
            // DECOR (decor55): the map's own [decor55] line (the accent, the band built); its numbers on screen are the a452 entry frames read by
            // Tools/night/n55-decor-share.py (the slate's doneWhen), so this part only checks the band was built with the row on
            bool d55 = Decor55.On(g.cfg);
            parts.Add($"DECOR decor55 on {d55}: {(Decor55.Line.Length > 0 ? Decor55.Line : "no [decor55] line (the map's pass has not run)")}");
            verdicts.Add(d55 && Decor55.Segments > 0);

            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            yield return Frames(5);
            yield return Stop();
            bool ok = verdicts.Count > 0 && verdicts.TrueForAll(v => v);
            string line = string.Join(" || ", parts);
            Debug.Log($"[a255] map {map}; {line}");
            res[0] = $"{(ok ? "GREEN" : "RED")} a255 LOOK | map {map} | {line}";
        }

        /// <summary>The STEP part (see the file's header). out[0] the part's text, out[1] "GREEN" / "RED".</summary>
        static IEnumerator N55StepPart(SurvivalGame g, string[] output)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string F2(double v) => v.ToString("0.00", ci);
            string F3(double v) => v.ToString("0.000", ci);
            SurvivalSim sim = g.sim;
            bool on = Move55.On(g.cfg), on54 = Move54.On(g.cfg);
            LevelDef lv = sim.campaignLevel;
            var lines = new List<Move55Lines.Line>();
            int rooms = 0;
            if (lv != null)
                foreach (RoomDef r in lv.rooms)
                {
                    if (r.kind == "boss") continue;
                    rooms++;
                    Move55Lines.Room(g.world, r.id, r.x0, r.z0, r.x1, r.z1, r.spawn.y, lines);
                }
            int scanned = lines.Count;
            int falls = Move55Lines.DropFalls(lines, g.cfg.movement.collision.groundSnap); // a drop over the ground snap is a fall (kiln's 0.40 m boxes), never a step
            if (lines.Count == 0)
            {
                // a level with no grounded step on it (n55look step: foundry, iceline, span, spire): nothing to measure, said so; not a failure of the row
                output[0] = $"STEP step55 on {on} (move54 {on54}): no step line on this level ({rooms} rooms scanned, {scanned} lines with a {F2(UnityTuning.Step55ScanStepMin)}-{F2(UnityTuning.Step55ScanStepMax)} m floor change, {falls} of them drops over the ground snap: falls): step not checked here (n52-kiln kb2c has one)";
                output[1] = on ? "GREEN" : "RED"; // nothing on this level can pop: the part does not fail the reel with the row on
                yield break;
            }
            lines.Sort(Move55Lines.Rank);
            lines = Move55Lines.Distinct(lines);
            int held0 = g.Step55Steps != null ? g.Step55Steps.StepsHeld : 0;
            // per-frame samples of the attempt that crossed: camera y, drawn feet y, grounded, horizontal speed, time
            var cy = new List<double>();
            var fy = new List<double>();
            var gr = new List<bool>();
            var sp = new List<double>();
            var ts = new List<double>();
            double t = 0, ran = 0, feetMax = 0;
            bool crossed = false;
            int attempts = 0;
            string lineWord = "";
            var tried = new List<string>();
            foreach (Move55Lines.Line ln in lines)
            {
                if (attempts >= UnityTuning.Step55ReelAttempts) break;
                attempts++;
                cy.Clear(); fy.Clear(); gr.Clear(); sp.Clear(); ts.Clear();
                t = ran = feetMax = 0;
                lineWord = $"line room {ln.room} from ({ln.start.x.ToString("0.0", ci)}, {F2(ln.start.y)}, {ln.start.z.ToString("0.0", ci)}) along ({F2(ln.along.x)}, {F2(ln.along.z)}), {ln.length.ToString("0.0", ci)} m, step {(ln.rise < 0 ? "down" : "up")} {F2(Math.Abs(ln.rise))} m at {ln.stepAt.ToString("0.0", ci)} m";
                double yawU = Math.Atan2(-ln.along.x, -ln.along.z);
                g.Respawn(new Vec3(ln.start.x, ln.start.y + 0.05, ln.start.z), yawU * 180 / Math.PI);
                for (int i = 0; i < 40; i++) { God(g); g.forcedMove = new InputFrame { yaw = yawU }; g.input.SetLook(yawU, 0); yield return null; }
                Vec3 lastFeet = g.Feet;
                for (int i = 0; i < UnityTuning.Step55RunSteps / 2; i++)
                {
                    Vec3 fp = g.Feet;
                    double along = (fp.x - ln.start.x) * ln.along.x + (fp.z - ln.start.z) * ln.along.z;
                    double speed = Math.Sqrt(sim.mover.velocity.x * sim.mover.velocity.x + sim.mover.velocity.z * sim.mover.velocity.z);
                    if (along >= ln.length - 1) break;
                    if (i > 30 && speed < 1) break; // pinned on a corner (the Mover vibrates there): the line ends
                    God(g);
                    var mv = new InputFrame { yaw = yawU, moveZ = 1 };
                    RideKill53Stick(yawU, ln.along, out mv.moveX, out mv.moveZ);
                    g.input.SetLook(yawU, 0);
                    g.forcedMove = mv;
                    yield return null;
                    t += g.FrameDt;
                    cy.Add(Camera.main != null ? Camera.main.transform.position.y : 0);
                    fy.Add(g.Move54DrawnFeet.y);
                    gr.Add(sim.mover.grounded && (sim.mover.state == MoveState.Ground || sim.mover.state == MoveState.Slide));
                    sp.Add(speed);
                    ts.Add(t);
                    Vec3 f = g.Feet;
                    ran += Math.Sqrt((f.x - lastFeet.x) * (f.x - lastFeet.x) + (f.z - lastFeet.z) * (f.z - lastFeet.z));
                    lastFeet = f;
                }
                for (int i = 1; i < fy.Count; i++)
                    if (gr[i] && gr[i - 1]) feetMax = Math.Max(feetMax, Math.Abs(fy[i] - fy[i - 1]));
                crossed = feetMax >= 0.8 * UnityTuning.Step55StepMin;
                tried.Add($"{ln.room} {(ln.rise < 0 ? "down" : "up")} {F2(Math.Abs(ln.rise))} m: {(crossed ? "CROSSED" : "not crossed")} (feet {F3(feetMax)} m in a grounded frame)");
                if (crossed) break;
            }
            g.forcedMove = new InputFrame();
            // the step frames: within Step55StepWindowS of a grounded frame whose feet moved >= half Step55StepMin; the flat frames: grounded,
            // moving (>= 3 m/s) and Step55FlatAwayS from any step frame and any airborne frame (a fall's landing dip is not flat-ground noise)
            var stepTimes = new List<double>();
            var airTimes = new List<double>();
            for (int i = 1; i < fy.Count; i++)
            {
                if (gr[i] && gr[i - 1] && Math.Abs(fy[i] - fy[i - 1]) >= 0.5 * UnityTuning.Step55StepMin) stepTimes.Add(ts[i]);
                if (!gr[i]) airTimes.Add(ts[i]);
            }
            double camStep = 0, camFlat = 0;
            int nStep = 0, nFlat = 0;
            for (int i = 1; i < cy.Count; i++)
            {
                if (!(gr[i] && gr[i - 1])) continue;
                double nearStep = double.PositiveInfinity, nearAir = double.PositiveInfinity;
                foreach (double st in stepTimes) nearStep = Math.Min(nearStep, Math.Abs(st - ts[i]));
                foreach (double at in airTimes) nearAir = Math.Min(nearAir, Math.Abs(at - ts[i]));
                double d = Math.Abs(cy[i] - cy[i - 1]);
                if (nearStep <= UnityTuning.Step55StepWindowS) { nStep++; camStep = Math.Max(camStep, d); }
                else if (nearStep >= UnityTuning.Step55FlatAwayS && nearAir >= UnityTuning.Step55FlatAwayS && sp[i] >= 3) { nFlat++; camFlat = Math.Max(camFlat, d); }
            }
            int held = g.Step55Steps != null ? g.Step55Steps.StepsHeld - held0 : -1;
            double share = feetMax > 1e-6 ? camStep / feetMax : double.NaN;
            bool defect = crossed && camStep > UnityTuning.Step55DefectOverNoise * camFlat;
            bool ok = on && crossed && share <= UnityTuning.Step55ReelOnFeetShare;
            output[0] = $"STEP step55 on {on} (move54 {on54}) | input: {lineWord} (attempt {attempts} of {lines.Count} step lines in {rooms} rooms, {falls} drops over the ground snap left out: falls; tried: {string.Join("; ", tried)}); ran {ran.ToString("0.0", ci)} m in {cy.Count} frames, the step {(crossed ? $"CROSSED (the feet {F3(feetMax)} m in one grounded frame; step55 held {(held < 0 ? "n/a (row off)" : held.ToString(ci))})" : "NOT CROSSED")} | "
                + $"camera's largest one-frame height change at the step {F3(camStep)} m vs the feet's {F3(feetMax)} (x{(double.IsNaN(share) ? "n/a" : F2(share))}, want <= x{F2(UnityTuning.Step55ReelOnFeetShare)} on; {nStep} step frames), flat-ground noise {F3(camFlat)} m ({nFlat} grounded moving frames away from steps and landings): the step over {UnityTuning.Step55DefectOverNoise}x the noise {(defect ? "yes" : "NO")}";
            output[1] = ok ? "GREEN" : "RED";
        }
    }
}
