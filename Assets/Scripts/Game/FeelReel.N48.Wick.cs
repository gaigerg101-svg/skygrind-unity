// NIGHT48 builder W (reel phase a148; FeelReel.N48.cs's dispatch): THE WICK's NIGHT48 rows on frames. Not run by its author (no Player in the
// worktree). Boot level 1 with reel.sh + -skygrindReelOnly (L-062: a CAMPAIGN run). Forced before the run unless -skygrindOff48 / older is given:
// the climbTop48 lips (sim.climbTop48Force; with the build copy's climbTop48 on, n48-foundry carries them anyway) and the n47 sim rows wick47 /
// climb47 / wickTop47 / wickRideParry47 / wickIntro47; the view row wickFinale47 is switched on in this run's config in memory (restored after).
// With -skygrindOff48 the same reel takes the RED half: NIGHT47's stack (no lips). God; input through forcedMove (L-046: no button is pressed);
// every state is forced so each frame is certain. TWO RESOLUTIONS (G5): the reel sets 1920x1080 windowed itself, takes the 720p pass at 1280x720
// (Screen.SetResolution, FeelReel.N33.Hud.cs's pattern) and restores the start size. Frames (shot-N29-<name>.png), what each should show:
//   W48-g9-intro            0.6 s into the fight (1080p): the name card "The Wick. The furnace wakes." (wickIntro47), one line, upper third.
//   W48-g5-<key>-1080 / -720 each WICK fight line forced in turn (reveal, crack, phase2, ride, net, phase3, intro), 1.2 s after it is posted: ONE
//                           line, the button glyph drawn (no raw tag), >= 36 px em at 1080p and >= 24 px at 720p (the line prints the measures).
//   W48-g1-top-p<i>         (G1) each top-third panel from its start: the feet at its catch line on the face, looking along it: the panel's end,
//                           the LIP after it (a cyan-edged shelf against the wall, climbTop48) and the next panel in view.
//   W48-g1-lip-<i>          (G1) standing on lip i, looking at the next panel's face (the short hop).
//   W48-g9-rideparry-hit    (round 3) 3 frames after a ride-parried ember hits THE WICK, from the floor looking at it: the hit's read LOCAL (the
//                           white-hot star, the flare, the two rings at the hit point); no whole-screen colour split (chroma <= 0.25, measured).
//   W48-g9-rideparry        0.6 s on: the ride-parried ember's two cracked plates (wickRideParry47), from the floor.
//   W48-g9-finale / -finale-2 0.25 / 0.9 s after the forced finishing core punch: the slowed world, the glass's yellow-white cracks, the slag flaring.
// Line: GREEN when the rows were forced, the lips are in the stack (Climb48On), one per design.n48.climbTop48.numbers.lipAfter entry, every fight line showed at both sizes with 1 object, no
// raw tag, em >= 36 px at 1080 and >= 24 px at 720, the screen reached 1920x1080 and 1280x720, the intro card showed, the ride parry cracked
// plates and a finale drew; RED otherwise (and with -skygrindOff48, where the line reports NIGHT47's stack).
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
        static partial void N48Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Wick48Scenario(g, dir, res, offs, pre);
        }

        /// <summary>G5: THE WICK's fight lines (design.n47.wick47.numbers.lines, non-empty) and the intro card, as (key, text).</summary>
        static List<(string key, string text)> Wick48Lines(SurvivalGame g)
        {
            var list = new List<(string, string)>();
            foreach (string k in new[] { "reveal", "crack", "phase2", "ride", "net", "phase3" })
            {
                string p = "design.n47.wick47.numbers.lines." + k;
                string t = g.cfg.Has(p) ? g.cfg.Str(p) : null;
                if (!string.IsNullOrEmpty(t)) list.Add((k, t));
            }
            if (g.cfg.Has("design.n47.wickIntro47.numbers.line")) list.Add(("intro", g.cfg.Str("design.n47.wickIntro47.numbers.line")));
            return list;
        }

        /// <summary>G5: one pass of the fight lines at the current screen size: each posted with a fresh key, measured 1.2 s later, shot.</summary>
        static IEnumerator Wick48LinePass(SurvivalGame g, string dir, string tag, double minEm, List<string> lines, int[] bad, double[] st)
        {
            SurvivalSim sim = g.sim;
            CultureInfo ci = CultureInfo.InvariantCulture;
            foreach (var (key, text) in Wick48Lines(g))
            {
                if (sim.wick47Line == null) { lines.Add($"{key}@{tag}: no line widget"); bad[0]++; continue; }
                sim.wick47Line.Say("w48-" + key + "-" + tag, text, 3.0);
                double t0 = sim.SimTime;
                while (sim.SimTime - t0 < 1.2) { God(g); yield return null; }
                Ui ui = g.UiView;
                string shown = ""; float em = 0, ink = 0, k = 0, a = 0;
                bool built = ui != null && ui.W48FightLine(out shown, out em, out ink, out k, out a);
                yield return Shot29(dir, "W48-g5-" + key + "-" + tag, st);
                bool ok = built && !string.IsNullOrEmpty(shown) && a > 0.5f && Hud47Probe.FightObjects == 1 && !Hud47Probe.FightRawTag && em >= minEm - 0.01;
                if (!ok) bad[0]++;
                lines.Add($"{key}@{tag} {(ok ? "ok" : "BAD")}: \"{shown}\" em {em.ToString("0.0", ci)} px, ink {ink.ToString("0.0", ci)} px (canvas x{k.ToString("0.000", ci)}), alpha {a.ToString("0.00", ci)}, objects {Hud47Probe.FightObjects}, raw tag {(Hud47Probe.FightRawTag ? "YES" : "no")}");
            }
        }

        /// <summary>G9 round 3: the chromatic aberration's intensity now (the camera's Volume stack, else the global one); -1 when none is found.</summary>
        static float Wick48Chroma(SurvivalGame g)
        {
            UnityEngine.Rendering.VolumeStack vs = null;
            if (g.cam != null) vs = UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(g.cam).volumeStack;
            vs ??= UnityEngine.Rendering.VolumeManager.instance.stack;
            var ca = vs != null ? vs.GetComponent<UnityEngine.Rendering.Universal.ChromaticAberration>() : null;
            return ca != null ? ca.intensity.value : -1f;
        }

        static IEnumerator Wick48Res(int w, int h)
        {
            Screen.SetResolution(w, h, FullScreenMode.Windowed);
            for (int i = 0; i < 60 && (Screen.width != w || Screen.height != h); i++) yield return null;
            yield return Frames(10);
        }

        static IEnumerator Wick48Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            bool force = NightRows.ForceOffFrom > 48;
            bool n47 = NightRows.ForceOffFrom > 47;
            int w0 = Screen.width, h0 = Screen.height;
            FullScreenMode mode0 = Screen.fullScreenMode;
            offs.Add(() => { if (Screen.width != w0 || Screen.height != h0) Screen.SetResolution(w0, h0, mode0); });
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; Quiet(g); g.setup.mode = RunSetup.Survival; });
            NightRowState overWas = sim.rowsOverride;
            if (n47) sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wick47", true).Set("climb47", true).Set("wickTop47", true).Set("wickRideParry47", true).Set("wickIntro47", true);
            offs.Add(() => { g.sim.rowsOverride = overWas; });
            sim.climbTop48Force = force;
            offs.Add(() => { g.sim.climbTop48Force = false; });
            // the view row wickFinale47 for this run (in memory; the config file is untouched)
            Dictionary<string, object> finRow = null;
            object finWas = null;
            try { finRow = PortData.Obj(PortData.Obj(PortData.Obj(g.cfg.root["design"])["n47"])["wickFinale47"]); finWas = finRow["enabled"]; if (n47) finRow["enabled"] = true; } catch (Exception) { finRow = null; }
            offs.Add(() => { if (finRow != null) finRow["enabled"] = finWas; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            yield return Wick48Res(1920, 1080);
            string r1080 = $"{Screen.width}x{Screen.height}";
            g.StartRun(Seed);
            yield return Frames(240);
            if (g.sim != sim) { sim = g.sim; }
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
            bool lips = sim.Climb48On;
            int lipN = s.lipTops.Count;
            Vec3 mouth = c + new Vec3(0, 0.05, s.p.radius + 1.0);
            Start(g, "wick48", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            int ph2 = 0;
            Action offPh = sim.events.On<Wick47Event>("wick47Phase", e => { if (e.phase == 2) ph2++; });
            offs.Add(() => offPh());
            g.Respawn(mouth, 0);
            g.input.SetLook(0, 0);
            g.forcedMove = new InputFrame();
            yield return Frames(10);
            if (!sim.Wick37Begin(out _, out _)) { res[0] = "RED: Wick37Begin returned false"; yield break; }
            Wick37 w = sim.wick37;
            double t0 = sim.SimTime;
            while (sim.SimTime - t0 < 0.6) { God(g); yield return null; }
            string introNow = sim.wick47Line != null ? sim.wick47Line.Now : null;
            yield return Shot29(dir, "W48-g9-intro", st);
            while (sim.SimTime - t0 < 4.0) { God(g); yield return null; } // the intro card and the reveal hint run out
            // G5 at 1080
            var lines = new List<string>();
            int[] bad = { 0 };
            yield return Wick48LinePass(g, dir, "1080", 36, lines, bad, st);
            // P2: the bar to the gate (the panels and lips out)
            if (w.w47 && w.p47 != null) w.Apply(w.body.hp - w.body.maxHp * w.p47.gate[0] + 1);
            for (int k = 0; k < 240 && ph2 == 0; k++) { God(g); yield return null; }
            double t2 = sim.SimTime;
            while (sim.SimTime - t2 < 0.6) { God(g); yield return null; }
            // G1: each top-third panel from its start (the next hold in view), then each lip looking at the next panel
            int topSec = s.panels.Count > 0 ? s.panels[s.panels.Count - 1].section : 0, topFrames = 0;
            foreach (Wick37Panel pn in s.panels)
            {
                if (pn.section != topSec) continue;
                Vec3 tu = (pn.end - pn.start) * (1 / Math.Max(1e-6, (pn.end - pn.start).Length));
                Vec3 at = c + pn.start + tu * 0.6 + pn.normal * 0.6 + new Vec3(0, 1.0, 0);
                yield return Wick47Look(g, at, at + tu * 9 + new Vec3(0, 1.2, 0), dir, "W48-g1-top-p" + pn.index, st);
                topFrames++;
                God(g);
            }
            for (int i = 0; i < lipN; i++)
            {
                double am = (s.lipFrom[i] + s.lipTo[i]) * 0.5, rl = (s.LipInner + s.p.radius) * 0.5;
                Vec3 at = c + new Vec3(Math.Cos(am) * rl, s.lipTops[i] + 0.05, Math.Sin(am) * rl);
                int ni = s.lipPanel[i] + 1;
                Vec3 look = ni < s.panels.Count ? c + s.panels[ni].start + (s.panels[ni].end - s.panels[ni].start) * 0.35 + new Vec3(0, 1.5, 0) : at + new Vec3(0, 2, 0);
                yield return Wick47Look(g, at, look, dir, "W48-g1-lip-" + i, st);
                God(g);
            }
            // G9 idea 1: a ride-parried ember (forced) cracks two plates. Round 3 (the lead's n48-r2 look: a whole-screen rainbow split): the
            // look is set on the floor FIRST, then the ember; W48-g9-rideparry-hit 3 frames after the hit (the local star, flare and rings at the
            // hit point; the chromatic aberration's intensity measured: the open hit's kick is a hurt's now, UnityTuning.W48.cs), then
            // W48-g9-rideparry 0.6 s on (the two cracked plates).
            {
                Vec3 feet = c + new Vec3(0, 0.05, 4), eye = new Vec3(feet.x, feet.y + 1.6, feet.z), dv = w.Centre - eye;
                double yaw = Math.Atan2(-dv.x, -dv.z), pitch = Math.Atan2(dv.y, Math.Sqrt(dv.x * dv.x + dv.z * dv.z));
                g.Respawn(feet, yaw * 180 / Math.PI);
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                for (int k = 0; k < 150; k++) { God(g); yield return null; } // 2.5 s: any earlier kick decays (post.chromaDecay)
            }
            int rp0 = w.rideCracksMade47;
            float chroma0 = Wick48Chroma(g);
            { Vec3 ec = w.Centre + new Vec3(0, 0, 3.5); w.embers.Add(new Wick37Ember { pos = ec, prev = ec, vel = new Vec3(0, 0, -33), life = 600, alive = true, back = true, ride47 = w.rideParry47 }); }
            for (int k = 0; k < 120 && w.rideCracksMade47 == rp0; k++) { God(g); yield return null; }
            yield return Frames(3);
            float chromaHit = Wick48Chroma(g);
            yield return Shot29(dir, "W48-g9-rideparry-hit", st);
            double tr = sim.SimTime;
            while (sim.SimTime - tr < 0.6) { God(g); yield return null; }
            float chromaAfter = Wick48Chroma(g);
            yield return Shot29(dir, "W48-g9-rideparry", st);
            int rideCracks = w.rideCracksMade47 - rp0;
            bool washFree = chromaHit >= 0 && chromaHit <= UnityTuning.Wick48ChromaMax + 1e-4f && chromaAfter <= UnityTuning.Wick48ChromaMax + 1e-4f;
            yield return Stop();
            // G5 at 720 (stills; the fight still on, the bar up)
            g.Respawn(mouth, 0);
            g.forcedMove = new InputFrame();
            yield return Wick48Res(1280, 720);
            string r720 = $"{Screen.width}x{Screen.height}";
            yield return Wick48LinePass(g, dir, "720", 24, lines, bad, st);
            yield return Wick48Res(1920, 1080);
            // G9 idea 3: the finisher (forced: the last core punch from 3 m on the cap)
            int fin0 = Wick37View.Current != null ? Wick37View.Current.Finales47 : -1;
            if (w.w47 && w.p47 != null && w.Alive)
            {
                double rc = (s.p.radius + s.p.capRadius) / 2;
                g.Respawn(c + new Vec3(0, s.p.capY + 0.05, rc), 0);
                for (int k = 0; k < 600 && w.Phase47 < 3; k++) { God(g); yield return null; } // a147's way: the feet on the cap start P3
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
                yield return Shot29(dir, "W48-g9-finale", st);
                while (sim.SimTime - tfin < 0.9) yield return null;
                yield return Shot29(dir, "W48-g9-finale-2", st);
            }
            int finales = Wick37View.Current != null ? Wick37View.Current.Finales47 - fin0 : -1;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            Screen.SetResolution(w0, h0, mode0);
            yield return Frames(10);
            bool resOk = r1080 == "1920x1080" && r720 == "1280x720";
            bool ok = force && lips && lipN == g.cfg.Nums("design.n48.climbTop48.numbers.lipAfter").Length && topFrames > 0 && bad[0] == 0 && lines.Count > 0 && resOk && !string.IsNullOrEmpty(introNow) && rideCracks > 0 && washFree && finales > 0;
            res[0] = $"{(ok ? "GREEN" : "RED")}: rows {(force ? "forced on (climbTop48 lips + the n47 WICK rows)" : "off from 48 (the RED half: NIGHT47's stack)")}, map {g.MapId}; " +
                     $"G1 lips in the stack {lips} ({lipN} lips, tops {string.Join(" / ", s.lipTops.ConvertAll(y => y.ToString("0.0", ci)))} m), top-panel frames {topFrames}; " +
                     $"G5 screens {r1080} and {r720}, lines bad {bad[0]} of {lines.Count} (wants 0: 1 object, no raw tag, em >= 36 px at 1080 / >= 24 at 720) | {string.Join(" | ", lines)} | " +
                     $"G9: intro line \"{introNow}\"; ride-parry cracks {rideCracks}, chromatic aberration before / 3 frames after the hit / 0.6 s on {chroma0.ToString("0.00", ci)} / {chromaHit.ToString("0.00", ci)} / {chromaAfter.ToString("0.00", ci)} (wants <= {UnityTuning.Wick48ChromaMax.ToString("0.00", ci)}: no full-screen wash; was 1.00 for ~19 s on n48-r2); finales drawn {finales} (wickFinale47 {NightRows.ViewOn(g.cfg, "wickFinale47")}) | phase 2 x{ph2}";
        }
    }
}
