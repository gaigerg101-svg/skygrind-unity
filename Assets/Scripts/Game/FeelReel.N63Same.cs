// NIGHT63 builder D (slate D1, SAME-RUN SPOT PAIRS; NIGHT63.md section 3; the red team's finding 2). Why: NIGHT62 P1's SPOT pair came from two
// reel runs that drift (n62-p0 s1on vs s1spot: nearest enemy at kb3 15.3 vs 15.2 m; their kb3 ENTRY frames, taken before any lamp lights, differ
// by 139,791 px), so the blind spotter named a hint line, not the row (L-129). With the option, ONE run gives both halves of every pair.
//   SKYGRIND.exe -skygrindReel <dir> ... -skygrindSameRun63 <night.row>        e.g. -skygrindSameRun63 n61.exitFrame61
// NIGHT63 review S4 (one name, one frame): the shot's own PNG in <side>/ is the reel's frame, as without the option (Shot29's stats and every
// sidecar describe it); the pair is two held frames, <side>-on/<file> (the row as it is) and <side>-off/<file> (the row off).
// At each shot (Shot29, Shot30; any shot helper that ends with `yield return SameRun63(dir, file)`): the frame with the row ON is written as
// always; then FeelClock holds (BeginHold: the sim steps 0, its interpolation alpha kept, timeScale 0, every view's wall clock frozen through
// FeelClock's shims, UnityEngine.Random and the SSAO noise replayed: SameRun63View, Ssao55), NightRows.ReelOverride turns the row OFF for one
// frame (the views that re-read it every frame show their off look; bossRule61's UI root is hidden: Ui.N63Same.cs), that frame is written under
// the SAME file name in <side>-off/ (SameRun63Math.OffDir: Logs/feel/<tag>/<side>-off/<scenario>/...), then the row comes back and the hold ends
// (EndHold: the clocks run on with the held frame skipped; FeelProbe never records it, so telemetry.jsonl and f####.jpg are as without the
// option). Proof (L-089): one line per shot, "[same63] toggle fired: shot <n> <file> | row <night.row> on -> off -> on, <kind> | sim held yes
// (step, feet, alive, alpha, FeelClock.Now, Time.time before = after) | off <path>". Without the option nothing here runs (no frame added).
// The planted cases (D1's doneWhen) are the rows noop63 (nothing draws it: 0 changed px) and plant63 (a 10 x 10 px square: exactly 100), forced
// on by the reel for its run; and the planted scenario `same63` below (frame-counted waits only, so its telemetry is comparable on vs off).
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
        static string same63Row, same63NightRow, same63Root;
        static SameRun63Math.Kind same63Kind;
        static int same63Shots, same63Held, same63Written;

        /// <summary>The option, read once at boot (a planted row must be on before the first shot's ON frame): the row, the reel's root, the
        /// view helper installed. No option, or a malformed one: nothing installed (a malformed one prints why).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SameRun63Boot()
        {
            if (!Application.isPlaying) return;
            string[] args;
            try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
            catch (Exception) { args = new string[0]; }
            same63Row = SameRun63Math.Row(args, out string why);
            if (same63Row == null)
            {
                if (why != null) Debug.Log("[same63] option refused: " + why + " (no same-run pairs this run)");
                return;
            }
            same63NightRow = "n" + NightRows.NightOfName(same63Row) + "." + same63Row;
            same63Kind = SameRun63Math.KindOf(same63Row);
            same63Root = SameRun63Math.ArgAfter(args, "-skygrindReel");
            if (same63Kind == SameRun63Math.Kind.Planted) NightRows.ReelOverride[same63Row] = true; // the planted rows: on for the whole run
            SameRun63View.Install(same63Row);
            Debug.Log("[same63] option: row " + same63NightRow + " (" + same63Kind + "); the pair's held frames under "
                + (same63Root != null ? SameRun63Math.OnDir(same63Root, same63Root) + " (on) and " + SameRun63Math.OffDir(same63Root, same63Root) + " (off)" : "<each shot folder>-on and -off")
                + " with the same file names; each shot's own PNG stays where it is");
        }

        /// <summary>The sim's state for the proof line (exact: "R" doubles): the step, the feet, the live enemies, the interpolation alpha.</summary>
        static string Sim63(SurvivalGame g)
        {
            if (g == null || g.sim == null) return "no game";
            CultureInfo ci = CultureInfo.InvariantCulture;
            Vec3 f = g.Feet;
            return "step " + g.simStep.ToString(ci) + " feet (" + f.x.ToString("R", ci) + ", " + f.y.ToString("R", ci) + ", " + f.z.ToString("R", ci) + ") alive "
                + g.sim.enemies.AliveCount.ToString(ci) + " alpha " + g.Alpha.ToString("R", ci);
        }

        /// <summary>After a shot's PNG `file` was written into `dir` at the end of this frame: with -skygrindSameRun63, the same instant again with
        /// the row OFF into the off folder under the same name (the sim and every clock held), then the row back on; without it, nothing.</summary>
        static IEnumerator SameRun63(string dir, string file)
        {
            if (same63Row == null || string.IsNullOrEmpty(file) || string.IsNullOrEmpty(dir)) yield break;
            SurvivalGame g = SurvivalGame.Instance;
            CultureInfo ci = CultureInfo.InvariantCulture;
            string before = Sim63(g);
            double now0 = FeelClock.Now;
            float time0 = Time.time;
            int frame0 = Time.frameCount;
            Color32[] shotPx = Grab63(null, null); // the shot's own frame again (still the end of its frame): the diagnostics' reference
            Ssao55.Snapshot63(); // the shot frame's SSAO noise, put back in the held frames
            FeelClock.BeginHold();
            // NIGHT63 review S4 (one name, one frame): the held ON frame goes to <side>-on/ under the same name, never over the shot's own PNG
            string onDir = SameRun63Math.OnDir(same63Root, dir), onPath = Path.Combine(onDir, file);
            string offDir = SameRun63Math.OffDir(same63Root, dir), offPath = Path.Combine(offDir, file);
            bool wrote = false, wroteOn = false;
            int onFrame = -1, heldFrame = -1;
            Color32[] onPx = null, offPx = null;
            try
            {
                // n63-b2 fix round (the lead: the no-op pairs changed 7-253,849 px; by eye the viewmodel, the crosshair arc, the hit flash and the
                // whole view moved): the shot's own frame and a held frame are not the same draw, because some views read another's output before it
                // updates in a frame (CameraFeel reads the viewmodel's punch, updated after it in RenderFrame), so the first held frame draws one
                // update further on. BOTH halves are therefore held frames: the first (the row as it is) is the ON half, the second (the row off)
                // the off half. A dt-0 frame repeats itself, so the two draw the same instant but for the row. NIGHT63 review S4: the ON half goes
                // to <side>-on/ (it used to replace the shot's PNG, so Shot29's stats and the sidecars described another frame under the same name)
                yield return null; // held frame 1: Update (0 sim steps, frameDt 0), the row as it is
                yield return new WaitForEndOfFrame();
                onFrame = Time.frameCount;
                onPx = Grab63(onDir, onPath);
                wroteOn = onPx != null;
                NightRows.ReelOverride[same63Row] = false;
                yield return null; // held frame 2: every view re-reads the row (off); LateUpdate (SameRun63View: the UI root, the plant)
                yield return new WaitForEndOfFrame();
                heldFrame = Time.frameCount;
                offPx = Grab63(offDir, offPath);
                wrote = offPx != null;
            }
            finally
            {
                string after = Sim63(g);
                double now1 = FeelClock.Now;
                float time1 = Time.time;
                bool held = after == before && now1 == now0 && time1 == time0;
                bool uiHid = SameRun63View.HidThisHold;
                if (same63Kind == SameRun63Math.Kind.Planted) NightRows.ReelOverride[same63Row] = true;
                else NightRows.ReelOverride.Remove(same63Row);
                SameRun63View.AfterHold(g);
                FeelClock.EndHold();
                same63Shots++;
                if (held) same63Held++;
                if (wrote) same63Written++;
                string state = before + (after == before ? "" : " -> AFTER " + after) + "; FeelClock.Now " + now0.ToString("0.#####", ci) + (now1 == now0 ? "" : " -> " + now1.ToString("0.#####", ci))
                    + ", Time.time " + time0.ToString("0.#####", ci) + (time1 == time0 ? "" : " -> " + time1.ToString("0.#####", ci))
                    + "; frames " + frame0.ToString(ci) + " shot, " + onFrame.ToString(ci) + " on (held, " + (wroteOn ? "in <side>-on/; the shot's own PNG kept" : "NOT WRITTEN") + "), " + heldFrame.ToString(ci) + " off (held)"
                    + (same63Kind == SameRun63Math.Kind.UiRoot ? "; ui root hidden " + (uiHid ? "yes" : "NO (no rule line up at this shot)") : "");
                // the in-Player diagnostics (n63-b2 fix round): the pair's own changed px (n62-gun-yellow.py's rule, the planted cases' number:
                // noop63 0, plant63 100), the held ON frame against the shot's own frame, and each frame's mean luma (a held frame gone dark shows here)
                int pair = Changed63(onPx, offPx), drift = Changed63(shotPx, onPx);
                double lShot = Luma63(shotPx), lOn = Luma63(onPx), lOff = Luma63(offPx);
                bool dark = (lShot > 0.02 && lOn < 0.5 * lShot) || (lOn > 0.02 && lOff < 0.5 * lOn);
                state += "; pair changed px " + pair.ToString(ci) + " (held on vs held off), held on vs the shot's frame " + drift.ToString(ci) + " px; mean luma shot "
                    + lShot.ToString("0.000", ci) + ", on " + lOn.ToString("0.000", ci) + ", off " + lOff.ToString("0.000", ci) + (dark ? " DARK HELD FRAME" : "")
                    + "; ssao materials " + Ssao55.MaterialsFound.ToString(ci) + ", noise replays " + Ssao55.Replayed63.ToString(ci); // the D1 residual's check (Ssao55.Rescan63)
                Debug.Log(SameRun63Math.ToggleLine(file, same63NightRow, same63Kind, held, state, wroteOn ? onPath : "NOT WRITTEN", wrote ? offPath : "NOT WRITTEN", same63Shots));
                // NIGHT63 B3c (B3's GATE): with bossColours63 toggled, per part the colour the held ON frame used (its last call that frame)
                if (same63Row == BossColours63Math.Row && onFrame >= 0) Debug.Log(BossColours63.FrameLine(file, onFrame));
            }
        }

        /// <summary>The screen now (call at the end of a frame): its pixels, and with a path its PNG written there; null when nothing was captured.</summary>
        static Color32[] Grab63(string folder, string path)
        {
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) return null;
                if (path != null)
                {
                    Directory.CreateDirectory(folder);
                    File.WriteAllBytes(path, t.EncodeToPNG());
                }
                return t.GetPixels32();
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        /// <summary>Pixels whose largest channel difference is over 24 (n62-gun-yellow.py --pair's rule); -1 when either is missing or the sizes differ.</summary>
        static int Changed63(Color32[] a, Color32[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return -1;
            int n = 0;
            for (int i = 0; i < a.Length; i++)
            {
                Color32 p = a[i], q = b[i];
                int dr = p.r - q.r, dg = p.g - q.g, db = p.b - q.b;
                if (dr > 24 || dr < -24 || dg > 24 || dg < -24 || db > 24 || db < -24) n++;
            }
            return n;
        }

        /// <summary>The frame's mean Rec. 709 luma (0-1) over every 4th pixel; -1 when missing.</summary>
        static double Luma63(Color32[] a)
        {
            if (a == null || a.Length == 0) return -1;
            double s = 0;
            int n = 0;
            for (int i = 0; i < a.Length; i += 4)
            {
                s += 0.2126 * a[i].r + 0.7152 * a[i].g + 0.0722 * a[i].b;
                n++;
            }
            return s / (255.0 * n);
        }

        /// <summary>D1's planted scenario `same63` (registered in FeelReel.N57.cs): the same seed's run (One's preamble), three husks walking in
        /// (UnityTuning.Same63HuskAt, unfrozen), the KESTREL firing at the nearest every Same63FireEvery frames (the aim bot), and a shot
        /// (shot-N30-S63-<nn>.png) every Same63ShotEvery frames from Same63FirstShot: muzzle, tracer, hits, chunks, HUD and grain all live around
        /// each shot. Every wait is frame-counted on the fixed clock (no wall time), so with the option the sim sees the same input per frame and
        /// telemetry.jsonl is comparable line for line with a run without it. Its pairs: <side>/same63 vs <side>-off/same63.</summary>
        static IEnumerator Same63Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            offs.Add(() => { g.forcedCombat = IdleCombat; });
            int placed = 0;
            foreach (double d in UnityTuning.Same63HuskAt)
            {
                Husk h = PlaceAt(g, d, 0, out _);
                if (h == null) continue;
                h.hp = Math.Max(h.hp, UnityTuning.ReelHitHp);
                placed++;
            }
            g.husks.frozen = false;
            yield return Frames(2);
            int toggles0 = same63Shots, held0 = same63Held, wrote0 = same63Written, shots = 0, step0 = g.simStep;
            Start(g, "same63", "camera", dir);
            FeelProbe.MarkInput();
            for (int i = 0; i < UnityTuning.Same63Frames; i++)
            {
                God(g);
                if (i % UnityTuning.Same63FireEvery == 0) FeelStrip.AimBot(g);
                else g.forcedCombat = IdleCombat;
                if (i >= UnityTuning.Same63FirstShot && (i - UnityTuning.Same63FirstShot) % UnityTuning.Same63ShotEvery == 0)
                {
                    yield return Shot30(dir, "S63-" + shots.ToString("00", ci));
                    shots++;
                }
                else yield return null;
            }
            g.forcedCombat = IdleCombat;
            yield return Stop();
            int toggles = same63Shots - toggles0, held = same63Held - held0, wrote = same63Written - wrote0;
            bool option = same63Row != null;
            bool ok = placed > 0 && shots > 0 && (!option || (toggles == shots && held == shots && wrote == shots));
            res[0] = (ok ? "GREEN" : "RED") + $" same63 | husks {placed} of {UnityTuning.Same63HuskAt.Length} walking in, the KESTREL every {UnityTuning.Same63FireEvery} frames"
                + $" | {FeelProbe.FrameCount} frames recorded, sim steps {g.simStep - step0}, shots {shots} (shot-N30-S63-00 ..)"
                + (option ? $" | option row {same63NightRow} ({same63Kind}): toggles fired {toggles}, sim held {held}, off frames written {wrote} (in {SameRun63Math.OffDir(same63Root, dir)})"
                          : " | no -skygrindSameRun63: no off frames (this run is the telemetry's other half)")
                + " | compare: n62-gun-yellow.py --pair <side>-on/same63/<shot> <side>-off/same63/<shot> --rect 0,0,1,1; telemetry.jsonl vs a run without the option";
        }
    }
}
