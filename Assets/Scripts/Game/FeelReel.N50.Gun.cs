// NIGHT50 builder H: the a150 phase, the proof of G1 (design.n50.charge50: arc40 + meter42 back under hud47's reticle, re-placed, in the weapon's
// token; UiHud38.Arc40.cs) and G2 (design.n50.gunglow50: the charge on the gun in four steps; Viewmodel.Charge35.cs). Run the same Player twice,
// the build copy's config with both rows on (node Tools/night/n39-build-rows.cjs <tag> on n50.charge50 n50.gunglow50):
//   reel.sh + -skygrindMap <a campaign level> + -skygrindReelOnly a150 + -screen-width 1920 -screen-height 1080 -screen-fullscreen 0
//   and again at -screen-width 1280 -screen-height 720. With the rows off the reel is RED ("not built"): the RED half of L-014.
// The CAMPAIGN on the booted map, enemies frozen, God, the player's pose held (forcedMove zero). Parts:
//   1. charge50, the same pose (the KESTREL in hand, a forced 50 % charge, Charge50Probe.Force): OFF (Charge50Probe.Hide: NIGHT49's frame, the
//      arc hidden), ON (re-placed, the token), OLD (Charge50Probe.Old: NIGHT46's placement and colours) -> frames H50-<off|on|old46>-50 and the
//      blind pair's centre crops H50-pair-<on|old46>-50 (charge49's crop: 240 px at 1080p, upscaled 2x). Then ON at a cooldown (50 %) and FULL.
//      Per ON frame: the arc's deepest possible drawn point to the centre and the band's height (1080p px and this frame's px). The pixels: OFF / ON
//      / OFF on the same pose; the pixels that change only with the arc give its nearest PIXEL to the centre and its height on the centre column.
//   2. charge50 live: the KESTREL held on the reel's pad (NIGHT33's recipe; `fires` printed, no action = NO READING): the arc's fill against the
//      telemetry's charge while charging; the gun's screen box at the hold's start and at full (the gun never moves: gunglow50 on); the BEND
//      meter set to 0.5 (the arc shows BEND).
//   3. gunglow50: KESTREL, THUNDERCLAP, PULSE: Gunglow50Probe.ForceLevel 0 / 0.25 / 0.5 / 0.75 / 1 -> frames H50-glow-<gun>-<pct>; per step the
//      lit segments, the mean pixel luma in the gun's screen box and its delta from the step before; the flash's drawn frames at 100 % (want
//      G50FlashFrames = 2); the gun's box at each step against step 0 (moved px).
// GREEN when: charge50 off hidden and on shown on every forced frame, every ON frame clears 18 px / 4 px at 1080p, the pixel read clears 18 px, the
// live hold acted with its fill on the truth; and each gun's luma rises at every step with its lit count right and its flash 2 frames. A gun
// that fails is named in the line (the fallback for the PULSE: KESTREL and THUNDERCLAP only, DECIDE50 D-8).
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
        static partial void N50Gun(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Gun50Scenario(g, dir, res, offs, pre);

        /// <summary>The charge50 probe now as one note.</summary>
        static string Geo50(CultureInfo ci)
        {
            if (!Charge50Probe.Shown) return "HIDDEN";
            return $"{Charge50Probe.Source} {Charge50Probe.State} fill {Charge50Probe.Fill.ToString("0.00", ci)} radius {Charge50Probe.Radius.ToString("0", ci)}: nearest {Charge50Probe.NearestUnits.ToString("0.0", ci)} px at 1080p ({Charge50Probe.NearestPx.ToString("0.0", ci)} px here), height {Charge50Probe.HeightUnits.ToString("0.0", ci)} px at 1080p ({Charge50Probe.HeightPx.ToString("0.0", ci)} here)";
        }

        /// <summary>Mean Rec. 709 luma (0..255) of `px` (bottom-up rows, w x h) inside `box` (screen px, origin bottom-left); -1 when empty.</summary>
        static double Luma50(Color32[] px, int w, int h, Rect box)
        {
            if (px == null || px.Length != w * h || box.width < 1 || box.height < 1) return -1;
            int x0 = Mathf.Clamp((int)box.xMin, 0, w - 1), x1 = Mathf.Clamp((int)box.xMax, 0, w - 1), y0 = Mathf.Clamp((int)box.yMin, 0, h - 1), y1 = Mathf.Clamp((int)box.yMax, 0, h - 1);
            double sum = 0;
            long n = 0;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    Color32 c = px[y * w + x];
                    sum += 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                    n++;
                }
            return n > 0 ? sum / n : -1;
        }

        static float BoxMoved50(Rect a, Rect b) => Mathf.Max(Mathf.Max(Mathf.Abs(a.xMin - b.xMin), Mathf.Abs(a.xMax - b.xMax)), Mathf.Max(Mathf.Abs(a.yMin - b.yMin), Mathf.Abs(a.yMax - b.yMax)));

        static string Box50(Rect r, CultureInfo ci) => $"({r.xMin.ToString("0", ci)},{r.yMin.ToString("0", ci)})-({r.xMax.ToString("0", ci)},{r.yMax.ToString("0", ci)})";

        static IEnumerator Gun50Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            offs.Add(() =>
            {
                Charge50Probe.Hide = false;
                Charge50Probe.Old = false;
                Charge50Probe.Force = null;
                Gunglow50Probe.ForceLevel = null;
                Gunglow50Probe.WantBox = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a150: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 480;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 460) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput); // the player's input (NIGHT47's a747 lesson: the bot's never reads the pad)
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            bool c50 = Charge50Probe.Built, g50 = Gunglow50Probe.Built;
            if (!c50 && !g50)
            {
                res[0] = "RED a150: neither charge50 nor gunglow50 is built in this Player (design.n50.charge50 / gunglow50 off in the build copy's config, an off switch, or hud38 / hud33 off): node Tools/night/n39-build-rows.cjs <tag> on n50.charge50 n50.gunglow50";
                yield break;
            }
            float minC = UnityTuning.H50MinCentrePx, minH = UnityTuning.H50MinHeightPx;
            // NIGHT33's recipe (FeelReel.N33.Shop.cs, NIGHT49's a149): IgnoreFocus for the reel's length, the pad enabled before every press
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
            Start(g, "a150-charge50-gunglow50", "camera", dir);
            yield return Frames(pre);
            string res0 = $"{Screen.width}x{Screen.height}";
            float to1080 = 1080f / Mathf.Max(1, Screen.height);
            var lines = new List<string> { $"frame {res0}; charge50 built {c50}, gunglow50 built {g50} (rigs: {Gunglow50Probe.Guns}); bars: nearest >= {minC.ToString("0", ci)} px, height >= {minH.ToString("0", ci)} px at 1080p; arc radius {UnityTuning.H50ArcRadius.ToString("0", ci)} (NIGHT46 {UnityTuning.Hud40Radius.ToString("0", ci)}); sim input {(sim.input != null ? sim.input.GetType().Name : "null")}, pad current {Gamepad.current == pad.pad}" };
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            IEnumerator Settle() { for (int i = 0; i < UnityTuning.H50ReelSettleFrames; i++) { God(g); yield return null; } }

            yield return Equip27(g, "revolver");
            g.forcedCombat = IdleCombat;
            for (int i = 0; i < 30; i++) { God(g); yield return null; }

            // ---- 1. charge50: OFF / ON / OLD at the same pose, then ON at a cooldown and FULL; the pixels ----
            bool c50ok = !c50, offHidden = false, onShown = false, oldShown = false;
            int under = 0;
            float pixNear = -1f;
            int pixRows = 0, pixChanged = 0;
            bool liveActed = false;
            int liveFrames = 0, liveMis = 0;
            string liveNote = "not run";
            if (c50 && Time.realtimeSinceStartup < wallEnd)
            {
                var sb = new System.Text.StringBuilder("charge50 forced:");
                void CheckOn(string tag)
                {
                    bool low = !Charge50Probe.Shown || Charge50Probe.NearestUnits + 1e-3f < minC || Charge50Probe.HeightUnits + 1e-3f < minH;
                    if (low) under++;
                    sb.Append($" [{tag}: {Geo50(ci)}{(low ? " UNDER" : "")}]");
                }
                Arc40 half = new Arc40 { show = true, segs = 1, line = -1f, fill = 0.5f, mode = Hud40Math.Charging, ready = false };
                Charge50Probe.Force = half;
                var off1 = new Color32[1][];
                var on1 = new Color32[1][];
                var off2 = new Color32[1][];
                Charge50Probe.Hide = true;
                yield return Settle();
                offHidden = !Charge50Probe.Shown;
                yield return Grab48(dir, off1, size, $"H50-off-50-{res0}");
                sb.Append($" [off: {(offHidden ? "HIDDEN" : "SHOWN")}]");
                Charge50Probe.Hide = false;
                yield return Settle();
                onShown = Charge50Probe.Shown;
                yield return Grab48(dir, on1, size, $"H50-on-50-{res0}");
                CheckOn("on 50");
                if (on1[0] != null) Pair49(dir, $"H50-pair-on-50-{res0}", on1[0], size[0], size[1], Mathf.RoundToInt(UnityTuning.H50ReelCropPx / to1080));
                Charge50Probe.Hide = true;
                yield return Settle();
                yield return Grab48(dir, off2, size, null);
                Diff49(off1[0], on1[0], off2[0], size[0], size[1], out pixNear, out pixRows, out pixChanged);
                Charge50Probe.Hide = false;
                Charge50Probe.Old = true;
                yield return Settle();
                oldShown = Charge50Probe.Shown;
                yield return Grab48(dir, frame, size, $"H50-old46-50-{res0}");
                if (frame[0] != null) Pair49(dir, $"H50-pair-old46-50-{res0}", frame[0], size[0], size[1], Mathf.RoundToInt(UnityTuning.H50ReelCropPx / to1080));
                sb.Append($" [old46: {(oldShown ? "SHOWN" : "HIDDEN")} radius {Charge50Probe.Radius.ToString("0", ci)}]");
                Charge50Probe.Old = false;
                Charge50Probe.Force = new Arc40 { show = true, segs = 1, line = -1f, fill = 0.5f, mode = Hud40Math.Draining, ready = false };
                yield return Settle();
                yield return Grab48(dir, frame, size, $"H50-on-cool-{res0}");
                CheckOn("on cool 50");
                Charge50Probe.Force = new Arc40 { show = true, segs = 1, line = -1f, fill = 1f, mode = Hud40Math.Charging, ready = true };
                yield return Settle();
                yield return Grab48(dir, frame, size, $"H50-on-100-{res0}");
                CheckOn("on 100");
                Charge50Probe.Force = null;
                lines.Add(sb.ToString());
                lines.Add($"charge50 pixels (off / on 50 / off, the same pose): {pixChanged} px changed in the centre box; nearest changed pixel {(pixNear < 0 ? "none" : pixNear.ToString("0.0", ci) + " px here = " + (pixNear * to1080).ToString("0.0", ci) + " px at 1080p")}; centre column {pixRows} px here = {(pixRows * to1080).ToString("0.0", ci)} px at 1080p (the band with its under-stroke)");

                // ---- 2. charge50 live: the KESTREL held; the gun's box (never moves); the BEND ----
                Gunglow50Probe.WantBox = true;
                g.forcedCombat = null;
                yield return Wait27(g, 0.6, wallEnd, () => God(g));
                Rect box0 = Gunglow50Probe.Box, boxFull = Rect.zero;
                int fires0 = fires;
                Press(0f, 1f);
                bool at50 = false, atFull = false;
                string g50s = "", gFull = "";
                int s0 = sim.simStep;
                while ((sim.simStep - s0) * sim.step < UnityTuning.H49ReelHoldMaxS && Time.realtimeSinceStartup < wallEnd)
                {
                    God(g);
                    yield return null;
                    double truth = Truth49(g);
                    if (Charge50Probe.Shown && Charge50Probe.State == "CHARGING")
                    {
                        liveFrames++;
                        if (Mathf.Abs(Charge50Probe.Fill - (float)truth) > UnityTuning.Meter42MatchTol + UnityTuning.Hud38FillStep) liveMis++;
                    }
                    if (!at50 && truth >= 0.5)
                    {
                        at50 = true;
                        g50s = Geo50(ci) + $", gun steps {Gunglow50Probe.Step}";
                        yield return Grab48(dir, frame, size, $"H50-live-kestrel-50-{res0}");
                    }
                    if (!atFull && truth >= 1)
                    {
                        atFull = true;
                        for (int i = 0; i < 8; i++) { God(g); yield return null; }
                        gFull = Geo50(ci) + $", gun steps {Gunglow50Probe.Step}";
                        boxFull = Gunglow50Probe.Box;
                        yield return Grab48(dir, frame, size, $"H50-live-kestrel-100-{res0}");
                        break;
                    }
                }
                Press(0f, 0f);
                yield return Wait27(g, 1.0, wallEnd, () => God(g));
                liveActed = fires - fires0 > 0;
                string moved = g50 && box0.width > 0 && boxFull.width > 0 ? $"gun box {Box50(box0, ci)} at rest -> {Box50(boxFull, ci)} at full: moved {BoxMoved50(box0, boxFull).ToString("0.0", ci)} px" : "gun box not read";
                liveNote = $"fires {fires - fires0}, acted {(liveActed ? "yes" : "NO (no reading)")}; at 50 %: {(at50 ? g50s : "not reached")}; at FULL: {(atFull ? gFull : "not reached")}; {liveFrames} charging frames, {liveMis} off the truth; {moved}";
                lines.Add("charge50 live KESTREL hold: " + liveNote);
                g.forcedCombat = IdleCombat;
                if (sim.Bend34On && Time.realtimeSinceStartup < wallEnd)
                {
                    yield return Wait27(g, 4.0, wallEnd, () => God(g)); // the charge shot's cooldown out (the gun in hand speaks first)
                    double meterWas = sim.bend34.meter;
                    sim.bend34.meter = 0.5;
                    for (int i = 0; i < 6; i++) { God(g); yield return null; }
                    string bend = Geo50(ci);
                    yield return Grab48(dir, frame, size, $"H50-live-bend-50-{res0}");
                    lines.Add($"charge50 BEND meter set 0.5: {bend}");
                    sim.bend34.meter = meterWas;
                }
                else lines.Add("charge50 BEND: design.n34.bend34 not live in this run: not checked");
                c50ok = offHidden && onShown && oldShown && under == 0 && pixNear >= 0 && pixNear * to1080 + 0.5f >= minC && liveActed && liveMis == 0;
            }

            // ---- 3. gunglow50: the four steps on each gun, the luma in the gun's box, the flash, the gun still ----
            Charge50Probe.Hide = true; // the arc out of the gun frames (it is not the gun's read)
            Gunglow50Probe.WantBox = true;
            var bad = new List<string>();
            var good = new List<string>();
            if (g50)
            {
                string[] guns = { "revolver", "shotgun", "gauntlet" };
                string[] names = { "KESTREL", "THUNDERCLAP", "PULSE" };
                double[] levels = { 0, 0.25, 0.5, 0.75, 1 };
                for (int gi = 0; gi < guns.Length && Time.realtimeSinceStartup < wallEnd; gi++)
                {
                    yield return Equip27(g, guns[gi]);
                    g.forcedCombat = IdleCombat;
                    for (int i = 0; i < 20; i++) { God(g); yield return null; }
                    var sb = new System.Text.StringBuilder($"gunglow50 {names[gi]}:");
                    bool ok = Gunglow50Probe.HasSteps;
                    if (!ok) sb.Append(" NO STEPS on this rig");
                    double prevL = -1;
                    Rect b0 = Rect.zero;
                    float maxMove = 0f, rest = 0f;
                    int flashSeen = 0;
                    // the baseline: the gun at 0 % over as many frames as the steps take (the web's idle breathing moves it a little; the
                    // check is that the charge adds no movement of its own)
                    Gunglow50Probe.ForceLevel = 0;
                    yield return null;
                    Rect r0 = Gunglow50Probe.Box;
                    for (int i = 0; i < UnityTuning.H50ReelSettleFrames * 4 + 8; i++)
                    {
                        God(g);
                        yield return null;
                        rest = Mathf.Max(rest, BoxMoved50(r0, Gunglow50Probe.Box));
                    }
                    for (int li = 0; li < levels.Length && ok; li++)
                    {
                        Gunglow50Probe.ForceLevel = levels[li];
                        int f0 = Gunglow50Probe.Flashes;
                        if (li == levels.Length - 1)
                        {
                            // 75 % -> 100 %: count the flash's drawn frames, then grab the held FULL
                            for (int i = 0; i < 8; i++) { God(g); yield return null; if (Gunglow50Probe.FlashNow) flashSeen++; }
                        }
                        else yield return Settle();
                        int pct = Mathf.RoundToInt((float)levels[li] * 100f);
                        yield return Grab48(dir, frame, size, $"H50-glow-{guns[gi]}-{pct}-{res0}");
                        Rect box = Gunglow50Probe.Box;
                        if (li == 0) b0 = box;
                        else maxMove = Mathf.Max(maxMove, BoxMoved50(b0, box));
                        double L = Luma50(frame[0], size[0], size[1], box);
                        int want = Viewmodel.Glow50Step(levels[li]);
                        bool stepOk = Gunglow50Probe.Step == want;
                        bool rise = li == 0 || (L >= 0 && prevL >= 0 && L > prevL);
                        if (!stepOk || !rise) ok = false;
                        sb.Append($" [{pct} %: lit {Gunglow50Probe.Step}/{UnityTuning.G50Steps}{(stepOk ? "" : " WANT " + want)}, luma {(L < 0 ? "n/a" : L.ToString("0.00", ci))}{(li > 0 && L >= 0 && prevL >= 0 ? " delta " + (L - prevL).ToString("+0.00;-0.00", ci) : "")}{(rise ? "" : " NO RISE")}]");
                        if (li == 0) sb.Append($" box {Box50(box, ci)}");
                        prevL = L;
                    }
                    Gunglow50Probe.ForceLevel = null;
                    bool flashOk = flashSeen == UnityTuning.G50FlashFrames;
                    bool still = maxMove <= rest + 1f;
                    if (!flashOk || !still) ok = false;
                    sb.Append($" flash {flashSeen} drawn frame(s) (want {UnityTuning.G50FlashFrames}); the box moved {maxMove.ToString("0.0", ci)} px across the steps (at rest, no charge, over as many frames: {rest.ToString("0.0", ci)} px; the charge's own: {(still ? "none above 1 px" : "MOVES")})");
                    lines.Add(sb.ToString());
                    (ok ? good : bad).Add(names[gi]);
                }
            }
            Charge50Probe.Hide = false;
            Gunglow50Probe.WantBox = false;
            yield return Stop();

            lines.Add($"input: focused at the start {focused0}, now {Application.isFocused}; backgroundBehavior {bgWas} -> IgnoreFocus for the reel; the reel pad {(padWasOff ? "added DISABLED" : "added enabled")}, re-enabled {reEnabled} times, presses while unfocused {unfocusedPresses}");
            bool g50ok = !g50 || bad.Count == 0;
            bool noReading = c50 && !liveActed;
            string verdict = noReading ? "NO READING" : c50ok && g50ok ? "GREEN" : "RED";
            string c50Line = c50 ? $"charge50 off {(offHidden ? "hidden" : "SHOWN")}, on {(onShown ? "shown" : "HIDDEN")}, old46 {(oldShown ? "shown" : "HIDDEN")}, {under} ON frames under the bars, nearest changed pixel {(pixNear < 0 ? "none" : (pixNear * to1080).ToString("0.0", ci) + " px at 1080p")}, centre column {(pixRows * to1080).ToString("0.0", ci)} px at 1080p, live {liveFrames} charging frames / {liveMis} off the truth" : "charge50 NOT BUILT";
            string g50Line = g50 ? $"gunglow50 ok: {(good.Count > 0 ? string.Join(" ", good) : "none")}; not ok: {(bad.Count > 0 ? string.Join(" ", bad) : "none")}" : "gunglow50 NOT BUILT";
            res[0] = $"{verdict} a150 charge50 + gunglow50 | {res0}: {c50Line}; {g50Line} || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
