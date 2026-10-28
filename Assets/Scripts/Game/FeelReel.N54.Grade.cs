// NIGHT54 builder G: the a254 phase (FeelReel.N54.cs routes it here): design.n54.grade54's proof (BossGrade54.cs). Not run by its author (no
// Player in the worktree). One campaign level a reel (L-062), the row on in the build copy's config (node Tools/night/n46-build-rows.cjs on
// n54.grade54) for GREEN; -skygrindOff54 for RED (NIGHT53's look):
//   bash Tools/night/n47-reels.sh <tag> g1on,n52-foundry,a254      (THE WICK)        and  g1off,n52-foundry,a254,-skygrindOff54
//   bash Tools/night/n47-reels.sh <tag> g2on,n52-iceline,a254      (THE ICEBREAKER)
//   bash Tools/night/n47-reels.sh <tag> g3on,n52-kiln,a254         (THE WYRM)
//   bash Tools/night/n47-reels.sh <tag> g4on,n52-span,a254         (THE LOOM)
//   bash Tools/night/n47-reels.sh <tag> g5on,n52-spire,a254        (THE CROWN)
// The CAMPAIGN on the booted map, DOORFREE, God, the husks killed and frozen, the camera still. The level's boss room (its RoomDef.boss) is begun
// with sim.BossRoomBegin (the campaign's own call) and the player put at its spawn. Frames (shot-N54-<boss>-<name>.jpg, 85 % JPG, in dir):
//   before    the level's start, no fight (NIGHT53's look whatever the row)
//   p1        the fight's phase 1, the grade fully in (weight 1, the start turn over)
//   p1-raw    the same view two frames later with BossGrade54.Suppress (the grade's weight 0): the ungraded half of the A/B pair
//   turn      the phase turn's peak (THE WICK: a real phase change, its bar pushed to P1's gate as a147 does; the others: BossGrade54.ProofBump,
//             the read phase + 1, through the same turn path a real change takes; the line says which)
//   p2        1 s after the turn: phase 2's palette settled
// Each frame's mean hue (circular, saturation-weighted) / saturation / value comes from a 64 x 36 grid of the captured frame. The line prints
// that the fight started (BossRoomBegin true, BossGrade54.Live, the boss id) and that each turn fired (BossGrade54.Turns and LastTurn: its
// input), then the numbers; the player's cyan (colors.usable) and the boss's rim against the graded frame: the hue distance and the WCAG
// luminance ratio. GREEN: the row on, the fight live, the start turn and the phase turn fired, the graded p1 frame away from the ungraded one by
// numbers.common.proofMinHue degrees or proofMinV / proofMinS. RED otherwise (the row off: no grade, no turns). Each boss's mean hue / value
// is printed so the lead can set the five reels side by side (the >= 40 deg or clearly-in-value check is across reels).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N54Grade(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = G54Scenario(g, dir, res, offs, pre);
        }

        /// <summary>A frame's mean: hue (deg, circular, weighted by saturation), saturation, value, and the mean linear colour.</summary>
        struct G54Mean
        {
            public bool ok;
            public double h, s, v;
            public Color lin;

            public string Line(CultureInfo ci) => ok ? $"H {h.ToString("0", ci)} S {s.ToString("0.000", ci)} V {v.ToString("0.000", ci)}" : "not measured";
        }

        /// <summary>Saves the screen to dir/shot-N54-name.jpg and measures its 64 x 36 grid into m[0]. Yields to the frame's end.</summary>
        static IEnumerator G54Shot(string dir, string name, G54Mean[] m)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            m[0] = default;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N54-" + name + ".jpg"), t.EncodeToJPG(85));
                const int GW = 64, GH = 36;
                double sx = 0, sy = 0, ss = 0, sv = 0, lr = 0, lg = 0, lb = 0;
                int n = 0;
                for (int j = 0; j < GH; j++)
                    for (int i = 0; i < GW; i++)
                    {
                        int x = (int)((i + 0.5) * t.width / GW), y = (int)((j + 0.5) * t.height / GH);
                        Color c = t.GetPixel(x, y);
                        Color.RGBToHSV(c, out float h, out float s, out float v);
                        double a = h * 2 * Math.PI;
                        sx += s * Math.Cos(a);
                        sy += s * Math.Sin(a);
                        ss += s;
                        sv += v;
                        Color l = c.linear;
                        lr += l.r; lg += l.g; lb += l.b;
                        n++;
                    }
                if (n == 0) yield break;
                double hue = Math.Atan2(sy, sx) * 180 / Math.PI;
                if (hue < 0) hue += 360;
                m[0] = new G54Mean { ok = true, h = hue, s = ss / n, v = sv / n, lin = new Color((float)(lr / n), (float)(lg / n), (float)(lb / n), 1) };
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        static IEnumerator G54Seconds(SurvivalGame g, double s)
        {
            float t0 = Time.unscaledTime;
            while (Time.unscaledTime - t0 < s) { God(g); yield return null; }
        }

        static IEnumerator G54Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; BossGrade54.Suppress = false; BossGrade54.ProofBump = 0; Quiet(g); g.setup.mode = RunSetup.Survival; });
            string gate = N24CampaignGate(g);
            if (gate != null) { res[0] = "RED: " + gate + " (boot -skygrindMap n52-foundry / n52-iceline / n52-kiln / n52-span / n52-spire)"; yield break; }
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
            bool rowOn = BossGrade54.On(g.cfg);
            string bossId = null;
            if (sim.campaign != null && sim.campaign.Level != null)
                foreach (RoomDef r in sim.campaign.Level.rooms)
                    if (r.kind == "boss") { bossId = r.boss; break; }
            string pc = $"map {g.MapId}, campaign {sim.campaign != null}, boss room {bossId ?? "none"}, row grade54 {(rowOn ? "on" : "off (the RED run)")}";
            if (bossId == null) { res[0] = "RED: no boss room in the level | " + pc; yield break; }

            Start(g, "grade54", "camera", dir);
            yield return Frames(pre);
            var m = new G54Mean[1];
            // before: the level's start, no fight
            g.forcedMove = new InputFrame();
            yield return Frames(20);
            yield return G54Shot(dir, bossId + "-before", m);
            G54Mean before = m[0];

            // the fight: the campaign's own begin, the player at its spawn, the camera held on the spawn's yaw
            int turns0 = BossGrade54.Turns;
            bool begun = sim.BossRoomBegin(bossId, out Vec3 spawn, out double yawDeg);
            if (!begun) { yield return Stop(); res[0] = $"RED: BossRoomBegin({bossId}) returned false | " + pc; yield break; }
            g.Respawn(spawn, yawDeg);
            double yaw = yawDeg * Math.PI / 180, pitch = 0.08;
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            for (int k = 0; k < 600 && !BossGrade54.Live; k++) { God(g); yield return null; }
            bool live = BossGrade54.Live;
            string graded = BossGrade54.Boss ?? "none";
            double startPeak = 0;
            for (int k = 0; k < 600 && (BossGrade54.Weight < 0.99f || BossGrade54.TurnEnvelope > 0 || BossGrade54.Turns == turns0); k++)
            {
                startPeak = Math.Max(startPeak, BossGrade54.TurnEnvelope);
                God(g); g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch }; yield return null;
            }
            int startTurns = BossGrade54.Turns - turns0; // the start turn fires once the grade's weight passes 0.5
            string startKind = BossGrade54.LastTurn + ", envelope peak " + startPeak.ToString("0.00", ci);
            yield return G54Seconds(g, 0.8); // past the turn gap (common.turnGapS 1 s) from the start turn (the loop above ends >= turnS 0.55 s after it)
            int phase1 = BossGrade54.Phase;
            float weight1 = BossGrade54.Weight;
            yield return G54Shot(dir, bossId + "-p1", m);
            G54Mean p1 = m[0];
            BossGrade54.Suppress = true;
            yield return Frames(2);
            yield return G54Shot(dir, bossId + "-p1-raw", m);
            G54Mean raw = m[0];
            BossGrade54.Suppress = false;
            yield return Frames(2);

            // THE TURN: THE WICK's bar to P1's gate (a real phase change, as a147 pushes it); the others through ProofBump (the same turn path)
            int turnsBefore = BossGrade54.Turns;
            string turnInput;
            int wickPh = 0;
            Action offPh = sim.events.On<Wick47Event>("wick47Phase", e => { if (e.phase >= 2) wickPh++; });
            offs.Add(() => offPh());
            Wick37 w = sim.wick37;
            if (bossId.StartsWith("wick", StringComparison.Ordinal) && w != null && w.Alive && w.w47 && w.p47 != null && w.p47.gate.Length > 0)
            {
                w.Apply(w.body.hp - w.body.maxHp * w.p47.gate[0] + 1);
                turnInput = "THE WICK's bar pushed to P1's gate (a real phase change)";
                for (int k = 0; k < 240 && wickPh == 0; k++) { God(g); yield return null; }
                if (wickPh == 0) { BossGrade54.ProofBump = 1; turnInput += "; no wick47Phase in 240 frames: ProofBump 1 instead"; }
            }
            else
            {
                BossGrade54.ProofBump = 1;
                turnInput = "BossGrade54.ProofBump 1 (the read phase + 1: the turn path a real phase change takes)";
            }
            for (int k = 0; k < 240 && BossGrade54.Turns == turnsBefore; k++) { God(g); g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch }; yield return null; }
            bool turned = BossGrade54.Turns > turnsBefore;
            string turnKind = BossGrade54.LastTurn;
            int phase2 = BossGrade54.Phase;
            double envPeak = 0, envPrev = 0;
            for (int k = 0; k < 120 && turned; k++)
            {
                double e = BossGrade54.TurnEnvelope;
                envPeak = Math.Max(envPeak, e);
                if (e >= 0.85 || (envPrev > 0.3 && e < envPrev)) break; // at the peak, or just past it (a slow recorder frame can step over it)
                envPrev = e;
                God(g); yield return null;
            }
            envPeak = Math.Max(envPeak, BossGrade54.TurnEnvelope);
            yield return G54Shot(dir, bossId + "-turn", m);
            G54Mean turnM = m[0];
            yield return G54Seconds(g, 1.0);
            yield return G54Shot(dir, bossId + "-p2", m);
            G54Mean p2 = m[0];
            BossGrade54.ProofBump = 0;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();

            // the numbers
            double dHue = p1.ok && raw.ok ? BossGrade54.HueDist(p1.h, raw.h) : 0, dV = p1.ok && raw.ok ? p1.v - raw.v : 0, dS = p1.ok && raw.ok ? p1.s - raw.s : 0;
            double minHue = G54Num(g, "proofMinHue", 20), minV = G54Num(g, "proofMinV", 0.05), minS = G54Num(g, "proofMinS", 0.06);
            bool differs = dHue >= minHue || Math.Abs(dV) >= minV || Math.Abs(dS) >= minS;
            Color usable = Skygrind.Game.Look.C(g.cfg, "colors.usable");
            Color.RGBToHSV(usable, out float uh, out _, out _);
            Grade54Boss pal = BossGrade54.PaletteOf(graded);
            string rimLine = "no palette";
            if (pal != null && p1.ok)
            {
                Color.RGBToHSV(pal.rim, out float rh, out _, out _);
                rimLine = $"cyan (hue {(uh * 360).ToString("0", ci)}) vs the graded frame: hue distance {BossGrade54.HueDist(uh * 360, p1.h).ToString("0", ci)} deg, luminance ratio {BossGrade54.ContrastRatio(usable.linear, p1.lin).ToString("0.0", ci)}:1; the rim (hue {(rh * 360).ToString("0", ci)}, gain {pal.rimGain.ToString("0.0", ci)}): hue distance {BossGrade54.HueDist(rh * 360, p1.h).ToString("0", ci)} deg, luminance ratio {BossGrade54.ContrastRatio(pal.rim.linear * pal.rimGain, p1.lin).ToString("0.0", ci)}:1";
            }
            // rule 8 (the lead's n54-r3): the graded frames' mean hue stays >= 20 deg from the danger hue (colors.enemy, ~12 deg)
            Color.RGBToHSV(Skygrind.Game.Look.C(g.cfg, "colors.enemy"), out float dh0, out _, out _);
            double dangerHue = dh0 * 360;
            double dz1 = p1.ok ? BossGrade54.HueDist(p1.h, dangerHue) : 0, dzT = turnM.ok ? BossGrade54.HueDist(turnM.h, dangerHue) : 0, dz2 = p2.ok ? BossGrade54.HueDist(p2.h, dangerHue) : 0;
            bool notDanger = dz1 >= 20 && dzT >= 20 && dz2 >= 20;
            rimLine += $" | danger hue {dangerHue.ToString("0", ci)}: graded mean hue distance p1 {dz1.ToString("0", ci)} / turn {dzT.ToString("0", ci)} / p2 {dz2.ToString("0", ci)} deg (>= 20 each): {(notDanger ? "ok" : "TOO CLOSE TO DANGER")}";
            bool ok = rowOn && begun && live && startTurns > 0 && turned && phase2 > phase1 && differs && notDanger;
            res[0] = $"{(ok ? "GREEN" : "RED")}: {pc}; the fight: BossRoomBegin({bossId}) {begun}, graded live {live} (boss {graded}, weight {weight1.ToString("0.00", ci)}), start turn fired {startTurns} ({startKind}); " +
                     $"the turn's input: {turnInput}: fired {turned} ({turnKind}, phase {phase1} -> {phase2}, envelope peak {envPeak.ToString("0.00", ci)}) | " +
                     $"before {before.Line(ci)} | p1 {p1.Line(ci)} | p1-raw {raw.Line(ci)} | turn {turnM.Line(ci)} | p2 {p2.Line(ci)} | " +
                     $"graded vs ungraded: dH {dHue.ToString("0", ci)} deg (>= {minHue.ToString("0", ci)}), dS {dS.ToString("+0.000;-0.000", ci)} (|.| >= {minS.ToString("0.00", ci)}), dV {dV.ToString("+0.000;-0.000", ci)} (|.| >= {minV.ToString("0.00", ci)}): differs {differs} | {rimLine}";
            Debug.Log("[a254] " + res[0]);
        }

        static double G54Num(SurvivalGame g, string key, double missing)
        {
            string p = "design.n54.grade54.numbers.common." + key;
            try { return g.cfg != null && g.cfg.Has(p) ? g.cfg.Num(p) : missing; }
            catch (Exception) { return missing; }
        }
    }
}
