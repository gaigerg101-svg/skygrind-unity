// NIGHT49 builder H: the a149 phase, G1's proof (design.n49.charge49, the one charge element under the reticle; UiHud38.Arc40.cs). Run the same
// Player twice, the build copy's config with the row on (node Tools/night/n39-build-rows.cjs <tag> on n49.charge49):
//   reel.sh + -skygrindMap <a campaign level> + -skygrindReelOnly a149 + -screen-width 1920 -screen-height 1080 -screen-fullscreen 0
//   and again at -screen-width 1280 -screen-height 720
// The CAMPAIGN on the booted map, enemies frozen, God. Four parts:
//   1. REST: the KESTREL in hand, nothing held: the element must be HIDDEN (frame H49-rest).
//   2. THE SHEET (forced content, Charge49Probe.Force; both candidates): each source (KESTREL, SLUG, BLAST, HEAT, BEND) at 0 / 50 / 100 % and
//      its cooldown at 50 %, the 100 % flash frame and the held FULL (frames H49-c<1|2>-<source>-<0|50|100f|100|cool>). Per frame the element's
//      nearest drawn point to the screen centre (with its dark under-stroke) and its height, in 1080p px and in this frame's px; the flash's
//      drawn frames counted (want H49FlashFrames = 2).
//   3. THE PIXELS (per candidate): the same frame hidden / shown / hidden; the pixels that change with the element (and not between the two
//      hidden frames) give its nearest PIXEL to the screen centre and its height on the centre column.
//   4. LIVE (Force null): the KESTREL held on the reel's pad (NIGHT33's recipe: IgnoreFocus for the reel, the pad enabled before every press;
//      `fires` / `acted` printed: no action = NO READING) for arc40 (NIGHT46's "before", Charge49Probe.OldArc) and both candidates, its 50 %
//      frame cropped for the blind pair (H49-pair-<arc40|c1|c2>-50); the THUNDERCLAP's slug held; the PULSE's pips set (2, then full); the
//      STITCHER held (its heat); the BEND meter set to 0.5 (its show, then hidden after H49BendShowS).
// Round 2 (DECIDE49 D-16): candidates 1 and 2 are retired (each lost its pair to arc40 0-3 twice); parts 2-4 frame candidate 3 only, and the
// pair frames are a 240 px centre crop (at 1080p) upscaled 2x to 480 px (Pair49): H49-pair-arc40-50 vs H49-pair-c3-50.
// The bars: design.n49.charge49.numbers.minCentrePx (18) and minHeightPx (4), at 1080p. GREEN when every shown frame clears both, the rest is
// hidden, both flashes last 2 frames, both pixel reads clear the centre bar and every live press acted with the element matching the truth.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N49Charge(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Charge49Scenario(g, dir, res, offs, pre);

        /// <summary>The charge level the element must show now (the telemetry's read, as meter42's TrueCharge42).</summary>
        static double Truth49(SurvivalGame g)
        {
            Loadout lo = g.loadout;
            if (lo == null) return 0;
            double level = lo.Charge35Level;
            ChargeShots27 cs = lo.charge27 != null && lo.charge27.active ? lo.charge27 : null;
            if (level <= 0 && cs != null && cs.charging)
            {
                int gi = ChargeShots27.Index(lo.Weapon);
                if (gi >= 0) level = cs.Level(gi);
            }
            return Math.Max(0, Math.Min(1, level));
        }

        /// <summary>The probe's geometry now as one note: "near N px (1080p) / n px here, band b, height h".</summary>
        static string Geo49(CultureInfo ci)
        {
            if (!Charge49Probe.Shown) return "HIDDEN";
            return $"{Charge49Probe.SourceName} {Charge49Probe.State} fill {Charge49Probe.Fill.ToString("0.00", ci)} segs {Charge49Probe.Segs}: nearest {Charge49Probe.NearestUnits.ToString("0.0", ci)} px at 1080p ({Charge49Probe.NearestPx.ToString("0.0", ci)} px here; band {Charge49Probe.NearestFillUnits.ToString("0.0", ci)}), height {Charge49Probe.HeightUnits.ToString("0.0", ci)} px at 1080p ({Charge49Probe.HeightPx.ToString("0.0", ci)} here){(Charge49Probe.FlashNow ? " FLASH" : "")}";
        }

        /// <summary>The pixels that changed between `hidden` and `shown` and not between `hidden` and `hidden2`, in the centre box: the nearest one's
        /// distance to the screen centre (px of this frame) and the count of changed rows on the centre column under the centre (-1 none).</summary>
        static void Diff49(Color32[] hidden, Color32[] shown, Color32[] hidden2, int w, int h, out float nearest, out int columnRows, out int changed)
        {
            nearest = -1f;
            columnRows = 0;
            changed = 0;
            if (hidden == null || shown == null || hidden2 == null || hidden.Length != w * h || shown.Length != w * h || hidden2.Length != w * h) return;
            int half = Mathf.Max(8, Mathf.RoundToInt(UnityTuning.H49ReelDiffBox * h / 1080f * 0.5f));
            float cx = w * 0.5f, cy = h * 0.5f, best = float.MaxValue;
            int x0 = Mathf.Max(0, (int)cx - half), x1 = Mathf.Min(w - 1, (int)cx + half), y0 = Mathf.Max(0, (int)cy - half), y1 = Mathf.Min(h - 1, (int)cy + half);
            int col = (int)cx;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    int i = y * w + x;
                    if (Delta49(hidden[i], hidden2[i]) > UnityTuning.H49ReelDiffMin) continue; // the world moved here: not the element's
                    if (Delta49(hidden[i], shown[i]) <= UnityTuning.H49ReelDiffMin) continue;
                    changed++;
                    float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                    if (d < best) best = d;
                    if (x == col && y < cy) columnRows++; // GetPixels32 rows run bottom-up: under the centre is y < cy
                }
            if (changed > 0) nearest = best;
        }

        /// <summary>The blind pair's frame (DECIDE49 D-16): a `crop` px square round the screen centre (240 px at 1080p), upscaled nearest-neighbour
        /// to H49ReelPairOutPx (480: 2x at 1080p) so the judges see the element, not a window of the whole screen.</summary>
        static void Pair49(string dir, string name, Color32[] src, int sw, int sh, int crop)
        {
            int outPx = UnityTuning.H49ReelPairOutPx;
            crop = Mathf.Max(8, crop);
            var o = new Color32[outPx * outPx];
            int x0 = sw / 2 - crop / 2, y0 = sh / 2 - crop / 2;
            for (int v = 0; v < outPx; v++)
                for (int u = 0; u < outPx; u++)
                {
                    int sx = x0 + u * crop / outPx, sy = y0 + v * crop / outPx;
                    o[v * outPx + u] = sx < 0 || sy < 0 || sx >= sw || sy >= sh ? new Color32(0, 0, 0, 255) : src[sy * sw + sx];
                }
            var t = new Texture2D(outPx, outPx, TextureFormat.RGB24, false);
            try
            {
                t.SetPixels32(o);
                t.Apply(false);
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + name + ".png"), t.EncodeToPNG());
            }
            finally { UnityEngine.Object.Destroy(t); }
        }

        static int Delta49(Color32 a, Color32 b) => Math.Max(Math.Abs(a.r - b.r), Math.Max(Math.Abs(a.g - b.g), Math.Abs(a.b - b.b)));

        static IEnumerator Charge49Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            int? candWas = Charge49Probe.Candidate;
            Charge49? forceWas = Charge49Probe.Force;
            bool oldWas = Charge49Probe.OldArc;
            offs.Add(() =>
            {
                Charge49Probe.Candidate = candWas;
                Charge49Probe.Force = forceWas;
                Charge49Probe.OldArc = oldWas;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a149: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 480;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 460) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput); // the player's input (NIGHT47's a747 lesson: the bot's never reads the pad)
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            if (!Charge49Probe.Built)
            {
                res[0] = "RED a149: the charge49 element is not built in this Player (design.n49.charge49 off in the build copy's config, an off switch, or hud38 / hud33 off): node Tools/night/n39-build-rows.cjs <tag> on n49.charge49";
                yield break;
            }
            GameConfig cfg = g.cfg;
            const string np = "design.n49.charge49.numbers.";
            float minC = cfg != null && cfg.Has(np + "minCentrePx") ? (float)cfg.Num(np + "minCentrePx") : 18f;
            float minH = cfg != null && cfg.Has(np + "minHeightPx") ? (float)cfg.Num(np + "minHeightPx") : 4f;
            // NIGHT33's recipe (FeelReel.N33.Shop.cs, NIGHT48's a748 round 5b): IgnoreFocus for the reel's length, the pad enabled before every press
            bool focused0 = Application.isFocused;
            InputSettings.BackgroundBehavior bgWas = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            offs.Add(() => InputSystem.settings.backgroundBehavior = bgWas);
            Pad27 pad = NewPad27(offs);
            bool padWasOff = !pad.pad.enabled;
            int reEnabled = 0, unfocusedPresses = 0;
            void Press(float lt, float rt)
            {
                if (!pad.pad.enabled) { InputSystem.EnableDevice(pad.pad); reEnabled++; }
                if (!Application.isFocused) unfocusedPresses++;
                pad.pad.MakeCurrent();
                pad.Send(lt, rt);
            }
            if (padWasOff) { InputSystem.EnableDevice(pad.pad); reEnabled++; }
            pad.pad.MakeCurrent();
            int fires = 0;
            offs.Add(g.events.On("fire", o => fires++));
            string pc = Preconditions(g, g.Feet);
            Start(g, "a149-charge49", "camera", dir);
            yield return Frames(pre);
            string res0 = $"{Screen.width}x{Screen.height}";
            float to1080 = 1080f / Mathf.Max(1, Screen.height);
            var lines = new List<string> { $"frame {res0}, the default candidate {UnityTuning.H49Candidate} (1 arc, 2 pips), bars: nearest >= {minC.ToString("0", ci)} px, height >= {minH.ToString("0", ci)} px at 1080p, sim input {(sim.input != null ? sim.input.GetType().Name : "null")}, pad current {Gamepad.current == pad.pad}" };
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            int under = 0, shots = 0;
            bool shownBad = false;
            void Check(string tag, System.Text.StringBuilder sb)
            {
                shots++;
                bool bad = Charge49Probe.Shown && (Charge49Probe.NearestUnits + 1e-3f < minC || Charge49Probe.HeightUnits + 1e-3f < minH);
                if (bad) under++;
                if (!Charge49Probe.Shown) shownBad = true;
                sb.Append($" [{tag}: {Geo49(ci)}{(bad ? " UNDER" : "")}]");
            }

            // ---- 1. REST: hidden with no charge ----
            yield return Equip27(g, "revolver");
            g.forcedCombat = IdleCombat;
            Charge49Probe.Force = null;
            Charge49Probe.OldArc = false;
            for (int i = 0; i < 30; i++) { God(g); yield return null; }
            bool restHidden = !Charge49Probe.Shown;
            yield return Grab48(dir, frame, size, $"H49-rest-{res0}");
            lines.Add($"rest (the KESTREL in hand, nothing held, Force off): the element {(restHidden ? "HIDDEN" : "SHOWN: " + Geo49(ci))}");

            // ---- 2. THE SHEET: both candidates x five sources x 0 / 50 / 100 (flash, held) / cooldown ----
            int[] srcs = { Charge49Math.Kestrel, Charge49Math.Slug, Charge49Math.Blast, Charge49Math.Heat, Charge49Math.Bend };
            string[] guns = { "revolver", "shotgun", "gauntlet", "nailgun", "revolver" };
            int[] flashFrames = new int[4];
            int pipsMax = sim.pulse27 != null && sim.pulse27.p != null ? sim.pulse27.p.pips : 5;
            int c3 = UnityTuning.H49CandidateBig; // round 2 (DECIDE49 D-16): candidates 1 and 2 are retired; only candidate 3 is framed
            for (int cand = c3; cand <= c3 && Time.realtimeSinceStartup < wallEnd; cand++)
            {
                Charge49Probe.Candidate = cand;
                for (int si = 0; si < srcs.Length && Time.realtimeSinceStartup < wallEnd; si++)
                {
                    int src = srcs[si];
                    string name = Charge49Math.SourceWord(src).ToLowerInvariant();
                    Charge49Probe.Force = null;
                    if (g.loadout.Weapon != guns[si]) { yield return Equip27(g, guns[si]); g.forcedCombat = IdleCombat; }
                    int segs = src == Charge49Math.Blast ? pipsMax : 1;
                    int up = src == Charge49Math.Heat ? Charge49Math.HeatUp : Charge49Math.Charging;
                    var sb = new System.Text.StringBuilder($"c{cand} {name}:");
                    foreach (float f in new[] { 0f, 0.5f })
                    {
                        Charge49Probe.Force = new Charge49 { show = true, source = src, mode = up, fill = f, segs = segs };
                        for (int i = 0; i < UnityTuning.H49ReelSettleFrames; i++) { God(g); yield return null; }
                        yield return Grab48(dir, frame, size, $"H49-c{cand}-{name}-{Mathf.RoundToInt(f * 100)}-{res0}");
                        Check($"{Mathf.RoundToInt(f * 100)}", sb);
                    }
                    if (src == Charge49Math.Heat)
                    {
                        // the heat at 100 % is the overheat lock's start (no flash); its cooldown is the lock; the lock's end flashes (the ready edge)
                        Charge49Probe.Force = new Charge49 { show = true, source = src, mode = up, fill = 1f, segs = 1 };
                        for (int i = 0; i < UnityTuning.H49ReelSettleFrames; i++) { God(g); yield return null; }
                        yield return Grab48(dir, frame, size, $"H49-c{cand}-{name}-100-{res0}");
                        Check("100", sb);
                        Charge49Probe.Force = new Charge49 { show = true, source = src, mode = Charge49Math.Cooling, fill = 0.5f, segs = 1 };
                        for (int i = 0; i < UnityTuning.H49ReelSettleFrames; i++) { God(g); yield return null; }
                        yield return Grab48(dir, frame, size, $"H49-c{cand}-{name}-cool-{res0}");
                        Check("cool", sb);
                        Charge49Probe.Force = new Charge49 { show = true, source = src, mode = Charge49Math.Cooling, fill = 0.98f, segs = 1 };
                    }
                    else
                    {
                        Charge49Probe.Force = new Charge49 { show = true, source = src, mode = Charge49Math.Cooling, fill = 0.5f, segs = 1 };
                        for (int i = 0; i < UnityTuning.H49ReelSettleFrames; i++) { God(g); yield return null; }
                        yield return Grab48(dir, frame, size, $"H49-c{cand}-{name}-cool-{res0}");
                        Check("cool", sb);
                        Charge49Probe.Force = new Charge49 { show = true, source = src, mode = up, fill = 0.96f, segs = segs };
                    }
                    for (int i = 0; i < UnityTuning.H49ReelSettleFrames; i++) { God(g); yield return null; }
                    // FULL: the flash's frames, then the held FULL
                    int flashes0 = Charge49Probe.Flashes;
                    Charge49Probe.Force = new Charge49 { show = true, source = src, mode = Charge49Math.Full, fill = 1f, segs = src == Charge49Math.Heat ? 1 : segs };
                    int seen = 0, waited = 0;
                    while (!Charge49Probe.FlashNow && waited < 4) { God(g); yield return null; waited++; }
                    if (Charge49Probe.FlashNow) seen = 1;
                    yield return Grab48(dir, frame, size, $"H49-c{cand}-{name}-100f-{res0}");
                    Check(src == Charge49Math.Heat ? "ready-flash" : "100-flash", sb);
                    for (int i = 0; i < 6; i++)
                    {
                        God(g);
                        yield return null;
                        if (Charge49Probe.FlashNow) seen++;
                    }
                    if (src == Charge49Math.Kestrel) flashFrames[cand] = seen;
                    sb.Append($" flash: {seen} drawn frame(s), flashes {Charge49Probe.Flashes - flashes0}");
                    yield return Grab48(dir, frame, size, $"H49-c{cand}-{name}-100-held-{res0}");
                    Check(src == Charge49Math.Heat ? "ready" : "100", sb);
                    lines.Add(sb.ToString());
                }
            }

            // ---- 3. THE PIXELS: hidden / shown / hidden on the same frame, per candidate ----
            float[] pixNear = { -1f, -1f, -1f, -1f };
            int[] pixRows = new int[4];
            Charge49Probe.Force = null;
            if (g.loadout.Weapon != "revolver") { yield return Equip27(g, "revolver"); g.forcedCombat = IdleCombat; }
            for (int cand = c3; cand <= c3; cand++)
            {
                Charge49Probe.Candidate = cand;
                var hidden = new Color32[1][];
                var shown = new Color32[1][];
                var hidden2 = new Color32[1][];
                Charge49Probe.Force = new Charge49 { show = false };
                for (int i = 0; i < UnityTuning.H49ReelSettleFrames; i++) { God(g); yield return null; }
                yield return Grab48(dir, hidden, size, null);
                Charge49Probe.Force = new Charge49 { show = true, source = Charge49Math.Kestrel, mode = Charge49Math.Full, fill = 1f, segs = 1 }; // from hidden: a switch, no flash
                for (int i = 0; i < 2; i++) { God(g); yield return null; }
                bool flashed = Charge49Probe.FlashNow;
                yield return Grab48(dir, shown, size, $"H49-c{cand}-pixels-shown-{res0}");
                Charge49Probe.Force = new Charge49 { show = false };
                for (int i = 0; i < 2; i++) { God(g); yield return null; }
                yield return Grab48(dir, hidden2, size, null);
                Diff49(hidden[0], shown[0], hidden2[0], size[0], size[1], out float near, out int rows, out int changed);
                pixNear[cand] = near;
                pixRows[cand] = rows;
                lines.Add($"c{cand} pixels (hidden / shown FULL / hidden): {changed} px changed in the centre box; nearest changed pixel {(near < 0 ? "none" : near.ToString("0.0", ci) + " px here = " + (near * to1080).ToString("0.0", ci) + " px at 1080p")}; centre column {rows} px tall here = {(rows * to1080).ToString("0.0", ci)} px at 1080p (the band with its under-stroke){(flashed ? " (a flash was on: re-check)" : "")}");
            }
            Charge49Probe.Force = null;

            // ---- 4. LIVE: the real sources, the pad pressed under NIGHT33's recipe ----
            int liveMismatch = 0, liveFrames = 0, noAct = 0;
            var pairs = new System.Text.StringBuilder("blind pair crops (the KESTREL's live hold at 50 %, the centre):");
            for (int pass = 0; pass < 2 && Time.realtimeSinceStartup < wallEnd; pass++)
            {
                // pass 0: NIGHT46's arc40 + meter42 (the "before"); 1: candidate 3 (round 2: 1 and 2 retired)
                Charge49Probe.OldArc = pass == 0;
                Charge49Probe.Candidate = pass == 0 ? (int?)null : c3;
                string tag = pass == 0 ? "arc40" : "c" + c3;
                if (g.loadout.Weapon != "revolver") { yield return Equip27(g, "revolver"); }
                g.forcedCombat = null;
                yield return Wait27(g, 0.6, wallEnd, () => God(g)); // the last shot's cooldown out
                int fires0 = fires;
                Press(0f, 1f);
                bool at50 = false, atFull = false;
                int s0 = sim.simStep, mis = 0, fr = 0, flashes0 = Charge49Probe.Flashes;
                string g50 = "", gFull = "";
                while ((sim.simStep - s0) * sim.step < UnityTuning.H49ReelHoldMaxS && Time.realtimeSinceStartup < wallEnd)
                {
                    God(g);
                    yield return null;
                    double truth = Truth49(g);
                    if (pass > 0 && Charge49Probe.Shown && (Charge49Probe.State == "CHARGING" || Charge49Probe.State == "FULL"))
                    {
                        fr++;
                        if (Mathf.Abs(Charge49Probe.Fill - (float)truth) > UnityTuning.H49FillStep + 0.02f) mis++;
                    }
                    if (!at50 && truth >= 0.5)
                    {
                        at50 = true;
                        g50 = pass == 0 ? $"arc40 shown {Hud40.ArcShown}, state {Hud40.ArcState}, fill {Hud40.ArcFill.ToString("0.00", ci)}" : Geo49(ci);
                        yield return Grab48(dir, frame, size, $"H49-live-{tag}-kestrel-50-{res0}");
                        if (frame[0] != null) Pair49(dir, $"H49-pair-{tag}-50-{res0}", frame[0], size[0], size[1], Mathf.RoundToInt(UnityTuning.H49ReelCropPx / to1080));
                    }
                    if (!atFull && truth >= 1)
                    {
                        atFull = true;
                        for (int i = 0; i < 8; i++) { God(g); yield return null; }
                        gFull = pass == 0 ? $"arc40 state {Hud40.ArcState}" : Geo49(ci);
                        yield return Grab48(dir, frame, size, $"H49-live-{tag}-kestrel-100-{res0}");
                        if (frame[0] != null) Pair49(dir, $"H49-pair-{tag}-100-{res0}", frame[0], size[0], size[1], Mathf.RoundToInt(UnityTuning.H49ReelCropPx / to1080));
                        break;
                    }
                }
                Press(0f, 0f); // the release fires the charged shot
                string gCool = "", gEnd = "";
                for (int i = 0; i < 40; i++)
                {
                    God(g);
                    yield return null;
                    if (pass > 0 && Charge49Probe.State == "COOLDOWN") { gCool = Geo49(ci); yield return Grab48(dir, frame, size, $"H49-live-{tag}-kestrel-cool-{res0}"); break; }
                }
                yield return Wait27(g, 4.0, wallEnd, () => God(g));
                gEnd = pass == 0 ? $"arc40 shown {Hud40.ArcShown}" : Geo49(ci);
                int shot = fires - fires0;
                bool acted = shot > 0;
                if (!acted) noAct++;
                liveMismatch += mis;
                liveFrames += fr;
                lines.Add($"live KESTREL hold [{tag}]: fires {shot}, acted {(acted ? "yes" : "NO (no reading)")}; at 50 %: {(at50 ? g50 : "not reached")}; at FULL: {(atFull ? gFull : "not reached")}; flashes {Charge49Probe.Flashes - flashes0}; after the release: {(gCool.Length > 0 ? gCool : "no COOLDOWN seen")}; 4 s later: {gEnd}{(pass > 0 ? $"; {fr} charging frames, {mis} off the truth by > {(UnityTuning.H49FillStep + 0.02f).ToString("0.00", ci)}" : "")}");
                pairs.Append($" H49-pair-{tag}-50-{res0}");
            }
            Charge49Probe.OldArc = false;
            Charge49Probe.Candidate = null;
            g.forcedCombat = IdleCombat;

            // the THUNDERCLAP's slug
            if (Time.realtimeSinceStartup < wallEnd)
            {
                yield return Equip27(g, "shotgun");
                g.forcedCombat = null;
                yield return Wait27(g, 0.8, wallEnd, () => God(g));
                int fires0 = fires;
                Press(0f, 1f);
                string mid = "";
                int s0 = sim.simStep;
                while ((sim.simStep - s0) * sim.step < UnityTuning.H49ReelHoldMaxS && Time.realtimeSinceStartup < wallEnd)
                {
                    God(g);
                    yield return null;
                    if (mid.Length == 0 && Truth49(g) >= 0.5) { mid = Geo49(ci); yield return Grab48(dir, frame, size, $"H49-live-slug-50-{res0}"); }
                    if (Truth49(g) >= 1) break;
                }
                Press(0f, 0f);
                yield return Wait27(g, 0.5, wallEnd, () => God(g));
                int shot = fires - fires0;
                if (shot <= 0) noAct++;
                lines.Add($"live THUNDERCLAP slug hold: fires {shot}, acted {(shot > 0 ? "yes" : "NO (no reading)")}; at 50 %: {(mid.Length > 0 ? mid : "not reached")}; after: {Geo49(ci)}");
                g.forcedCombat = IdleCombat;
            }

            // the PULSE's pips (set, as a748 sets them) and the BLAST ready
            PulseCharge27 p = sim.pulse27;
            if (p != null && p.active && Time.realtimeSinceStartup < wallEnd)
            {
                yield return Equip27(g, "gauntlet");
                g.forcedCombat = IdleCombat;
                int pipsWas = p.pips, cdWas = p.cooldownSteps;
                p.pips = Math.Min(2, p.p.pips);
                p.cooldownSteps = 0;
                for (int i = 0; i < 10; i++) { God(g); yield return null; }
                string two = Geo49(ci);
                yield return Grab48(dir, frame, size, $"H49-live-blast-2pips-{res0}");
                int flashes0 = Charge49Probe.Flashes;
                p.pips = p.p.pips;
                for (int i = 0; i < 10; i++) { God(g); yield return null; }
                string full = Geo49(ci);
                yield return Grab48(dir, frame, size, $"H49-live-blast-full-{res0}");
                lines.Add($"live PULSE pips (set): 2 of {p.p.pips}: {two}; full: {full}; flashes {Charge49Probe.Flashes - flashes0}");
                p.pips = pipsWas;
                p.cooldownSteps = cdWas;
            }
            else lines.Add("live PULSE: design.n27.pulse27 not live in this run: not checked");

            // the STITCHER's heat (held)
            if (Time.realtimeSinceStartup < wallEnd)
            {
                yield return Equip27(g, "nailgun");
                g.forcedCombat = null;
                int fires0 = fires;
                Press(0f, 1f);
                yield return Wait27(g, 0.5, wallEnd, () => God(g));
                string hot = Geo49(ci);
                yield return Grab48(dir, frame, size, $"H49-live-heat-{res0}");
                Press(0f, 0f);
                yield return Wait27(g, 3.0, wallEnd, () => God(g));
                int shot = fires - fires0;
                if (shot <= 0) noAct++;
                lines.Add($"live STITCHER held 0.5 s: fires {shot}, acted {(shot > 0 ? "yes" : "NO (no reading)")}; heat: {hot}; 3 s after the release: {Geo49(ci)}");
                g.forcedCombat = IdleCombat;
            }

            // the BEND meter (set to 0.5: its change shows it, then it hides after H49BendShowS)
            bool bendOk = true;
            if (sim.Bend34On && Time.realtimeSinceStartup < wallEnd)
            {
                yield return Equip27(g, "revolver");
                g.forcedCombat = IdleCombat;
                yield return Wait27(g, 1.5, wallEnd, () => God(g));
                double meterWas = sim.bend34.meter;
                sim.bend34.meter = 0.5;
                for (int i = 0; i < 6; i++) { God(g); yield return null; }
                string half = Geo49(ci);
                bool bendShown = Charge49Probe.Shown && Charge49Probe.Source == Charge49Math.Bend;
                yield return Grab48(dir, frame, size, $"H49-live-bend-50-{res0}");
                yield return Wait27(g, UnityTuning.H49BendShowS + 0.5, wallEnd, () => God(g));
                bool bendHidden = !Charge49Probe.Shown;
                bendOk = bendShown && bendHidden;
                lines.Add($"live BEND meter set 0.5: {half}; {(UnityTuning.H49BendShowS + 0.5).ToString("0.0", ci)} s later: {(bendHidden ? "HIDDEN" : Geo49(ci))}");
                sim.bend34.meter = meterWas;
            }
            else lines.Add("live BEND: design.n34.bend34 not live in this run: not checked");
            yield return Stop();

            lines.Add($"input: focused at the start {focused0}, now {Application.isFocused}; backgroundBehavior {bgWas} -> IgnoreFocus for the reel; the reel pad {(padWasOff ? "added DISABLED" : "added enabled")}, re-enabled {reEnabled} times, presses while unfocused {unfocusedPresses}");
            bool flashOk = flashFrames[c3] == UnityTuning.H49FlashFrames;
            bool pixOk = pixNear[c3] >= 0 && pixNear[c3] * to1080 + 0.5f >= minC;
            bool liveOk = noAct == 0 && liveMismatch == 0;
            bool green = restHidden && under == 0 && !shownBad && flashOk && pixOk && liveOk && bendOk;
            string verdict = noAct > 0 ? "NO READING" : green ? "GREEN" : "RED";
            res[0] = $"{verdict} a149 charge49 | {res0}: rest {(restHidden ? "hidden" : "SHOWN")}; the sheet {shots} frames, {under} under the bars (nearest >= {minC.ToString("0", ci)} px, height >= {minH.ToString("0", ci)} px at 1080p){(shownBad ? ", a forced frame NOT drawn" : "")}; the KESTREL flash c{c3} {flashFrames[c3]} drawn frames (want {UnityTuning.H49FlashFrames}); nearest changed pixel c{c3} {(pixNear[c3] * to1080).ToString("0.0", ci)} px at 1080p, centre column c{c3} {(pixRows[c3] * to1080).ToString("0.0", ci)} px; live: {liveFrames} charging frames, {liveMismatch} off the truth, presses without action {noAct}; BEND {(bendOk ? "ok" : "NOT ok")} || {string.Join(" || ", lines)} || {pairs} | {pc}";
        }
    }
}
