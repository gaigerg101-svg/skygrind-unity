// NIGHT48 builder F: the a748 phase, the WEAPONS reel (G20 blast48, G24 guns48, G43 the TORCH). Run:
//   reel.sh + -skygrindMap <a campaign level, e.g. n47-iceline> + -skygrindReelOnly a748   (pulse47 ON in the build's copy: the sim's throw)
// The CAMPAIGN on the booted map, DOORFREE, God, the player's own input + the reel's virtual pad as Gamepad.current (the line prints `sim input
// ReaderInput, pad current True`: NIGHT47's lesson). Then:
//   BLAST (G20): three passes, husks frozen at 3 / 5 / 7 m on the look until the press's frame: OFF (fx47 + blast48 off: NIGHT46's), A (fx47 +
//     blast48 on, variant 0 "ring"), B (variant 1 "lance"). Frames -2 .. +30 (shot-N30-X48-blast-<off|A|B>-f<k>.png); per frame the reticle
//     box's mean (Rec. 709 on the gamma values, the middle 10 % of width and height) and its rise over the pass's f-2, the bright (> 0.85) share,
//     the halos (red / yellow tells) and burst shells drawn, the ring's radius, the blast's kick, the camera's pitch over f-2, the bodies thrown.
//     The WASH meter: a frame of an ON pass whose box rose > 0.25 (or whose bright share rose > 25 points) over f-2; GREEN at 0 in A and in B.
//     The blind pair (3 judges, "which LT blast hits harder?"): A / B as shot-N30-X48-pair-blast-<P|Q>-f<k>.png by a seeded coin, the key on
//     the line; the judged moments f4 (the shockwave widest round the reticle) and f12 (the throw).
//   GUNS (G24) + the TORCH (G43): every action, three passes: OFF (fx47 + guns48 off: NIGHT46's), N47 (fx47 on, guns48 off: today's), N48
//     (fx47 + guns48 on). The aim 15 deg up into empty air, no bodies (a clean sheet). Actions: kestrel-tap (RT 2 frames), kestrel-charge (RT
//     held past the charge, released at f0), thunder-shot, thunder-slug (held past the THUNDERCLAP's charge), thunder-boost (in the air 3 m up,
//     looking down 75 deg, RT at f0), pulse-punch (the gauntlet's RT), fist (the punch button), torch (RT held through the sheet; the lick
//     frame after its release; a 1 s hold strip), swap (the THUNDERCLAP in hand, the KESTREL's slot asked at f0). Per frame -2 .. +30: the
//     camera's pitch against f-2 (the kick the player sees), the field of view's change, fx47's own kick share, the frame's mean luminance and
//     its rise (the flash). A contact sheet per action (shot-N30-X48-sheet-<action>.png: rows OFF / N47 / N48, the 11 frames at 1/5 size), full
//     frames for the blind pair (OFF vs N48 at f2 and f6, P / Q by a seeded coin per action; "which hits harder?").
//     The G24 LADDER line: the N48 peak in its tier's band (F-cards 1: light 0.5-1.0, medium 1.5-2.5, heavy 3-5; the KESTREL tap kept at
//     today's 1.5-3.5: DECIDE47 D-14), settled under 10 % of its peak by the tier's frame, and NO action lighter than N47 (D-14).
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
        static partial void N48Fx(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Fx48Scenario(g, dir, res, offs, pre);

        /// <summary>A grid of whole frames shrunk by `div` (rows x cols).</summary>
        sealed class Strip48
        {
            public readonly int cw, ch, cols, rows, w, h;
            public readonly Color32[] px;
            const int Gut = 4;

            public Strip48(int cols, int rows, int sw, int sh, int div)
            {
                this.cols = cols;
                this.rows = rows;
                cw = Mathf.Max(8, sw / div);
                ch = Mathf.Max(8, sh / div);
                w = cols * cw + (cols - 1) * Gut;
                h = rows * ch + (rows - 1) * Gut;
                px = new Color32[w * h];
                var bg = new Color32(12, 12, 16, 255);
                for (int i = 0; i < px.Length; i++) px[i] = bg;
            }

            public void Put(int col, int row, Color32[] src, int sw, int sh)
            {
                int x0 = col * (cw + Gut), y0 = row * (ch + Gut);
                float fx = (float)sw / cw, fy = (float)sh / ch;
                for (int v = 0; v < ch; v++)
                    for (int u = 0; u < cw; u++)
                    {
                        int r = 0, gg = 0, b = 0, n = 0;
                        for (int j = 0; j < 2; j++)
                            for (int i = 0; i < 2; i++)
                            {
                                int sx = Mathf.Min(sw - 1, (int)((u + 0.25f + 0.5f * i) * fx)), sy = Mathf.Min(sh - 1, (int)((ch - 1 - v + 0.25f + 0.5f * j) * fy));
                                Color32 c = src[sy * sw + sx];
                                r += c.r; gg += c.g; b += c.b; n++;
                            }
                        px[(h - 1 - (y0 + v)) * w + x0 + u] = new Color32((byte)(r / n), (byte)(gg / n), (byte)(b / n), 255);
                    }
            }

            public void Write(string dir, string name)
            {
                var t = new Texture2D(w, h, TextureFormat.RGB24, false);
                try
                {
                    t.SetPixels32(px);
                    t.Apply(false);
                    System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + name + ".png"), t.EncodeToPNG());
                }
                finally { UnityEngine.Object.Destroy(t); }
            }
        }

        /// <summary>The reticle box's mean, the bright share and the frame's mean (Rec. 709 on the gamma values) of a grabbed frame.</summary>
        static void Lum48(Color32[] px, int w, int h, out float box, out float bright, out float mean)
        {
            double cs = 0, all = 0;
            int nb = 0;
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    int sx = (int)(w * (0.45f + 0.1f * (x + 0.5f) / 16f)), sy = (int)(h * (0.45f + 0.1f * (y + 0.5f) / 16f));
                    Color32 c = px[sy * w + sx];
                    cs += (0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b) / 255.0;
                }
            for (int y = 0; y < 36; y++)
                for (int x = 0; x < 64; x++)
                {
                    Color32 c = px[((y * h) / 36) * w + (x * w) / 64];
                    double l = (0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b) / 255.0;
                    all += l;
                    if (l > 0.85) nb++;
                }
            box = (float)(cs / 256);
            bright = nb / (64f * 36f);
            mean = (float)(all / (64 * 36));
        }

        static IEnumerator Fx48Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            bool? fxWas = Fx47Probe.Force, b48Was = Blast48.Force, g48Was = Guns48.Force;
            int variantWas = Fx47Probe.Variant;
            offs.Add(() =>
            {
                Fx47Probe.Force = fxWas;
                Blast48.Force = b48Was;
                Guns48.Force = g48Was;
                Blast48.ReelVariant = false;
                Fx47Probe.Variant = variantWas;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a748: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 540;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 520) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput); // the player's input (NIGHT47's a747 lesson: the bot's never reads the pad)
            g.forcedMove = null;
            // round 5b (n48-r3: the G24 sheets fired 0 shots, the blasts still went; n48-r4: nothing fired at all, "blasted NO" x3, so the ladder
            // read 0.00 deg and the wash meter read a blast that never came): FeelReel.N33.Shop.cs's lesson (n33-r6). The Input System's default
            // backgroundBehavior (ResetAndDisableNonBackgroundDevices) DISABLES a virtual Gamepad while the Player window is not focused, and the reel
            // Player runs on unfocused (runInBackground): a focus change mid-reel silences the pad (r3 lost it after the blast passes, r4 before them).
            // The reel takes IgnoreFocus for its length, enables its pad before every press, and counts what it had to re-enable.
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
            yield return Equip27(g, "gauntlet");
            g.forcedCombat = null;
            string pc = Preconditions(g, g.Feet);
            Start(g, "a748-fx48", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string> { $"pulse47 (sim) {(sim.Pulse47On ? "ON" : "off")}, sim input {(sim.input != null ? sim.input.GetType().Name : "null")}, pad current {Gamepad.current == pad.pad}, frame {Screen.width}x{Screen.height}" };
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            Vec3 home = g.Feet;
            double homeYaw = g.input != null ? g.input.yaw : 0;

            // ---- BLAST (G20) ----
            Blast48.ReelVariant = true;
            string[] bTag = { "off", "A", "B" };
            int[] washed = new int[3];
            bool[] blasted = new bool[3];
            var washWhere = new System.Text.StringBuilder();
            bool aIsP = new Mulberry32(Seed ^ 0x48B1u).Next() < 0.5;
            for (int pass = 0; pass < 3 && Time.realtimeSinceStartup < wallEnd; pass++)
            {
                Fx47Probe.Force = pass >= 1;
                Blast48.Force = pass >= 1;
                Fx47Probe.Variant = pass == 2 ? 1 : 0;
                g.Respawn(home, homeYaw * 180 / Math.PI);
                yield return Equip27(g, "gauntlet");
                g.forcedCombat = null;
                double yaw = g.input != null ? g.input.yaw : 0;
                double fx = -Math.Sin(yaw), fz = -Math.Cos(yaw);
                var husks = new List<Husk>();
                var from = new List<Vec3>();
                foreach (double d in new[] { 3.0, 5.0, 7.0 })
                {
                    var at = new Vec3(g.Feet.x + fx * d, g.Feet.y, g.Feet.z + fz * d);
                    Husk h = null;
                    try { h = sim.husks.Spawn("husk", at, g.Feet); } catch (Exception) { }
                    if (h != null) { h.hp = h.maxHp = 1e6; husks.Add(h); from.Add(at); }
                }
                g.husks.frozen = true;
                PulseCharge27 p = sim.pulse27;
                yield return Wait27(g, 0.6, wallEnd, () => { God(g); if (p != null) { p.pips = p.p.pips; p.cooldownSteps = 0; } });
                if (sim.loadout.Weapon != "gauntlet") { yield return Equip27(g, "gauntlet"); g.forcedCombat = null; }
                int blasts0 = p != null ? p.blasts : 0;
                var sb = new System.Text.StringBuilder($"blast {bTag[pass]}:");
                float box0 = 0, bright0 = 0, pitch0 = 0;
                int frameN = -2;
                foreach (int k in Sheet47)
                {
                    while (frameN < k)
                    {
                        if (frameN == 0) { g.husks.frozen = false; Press(1f, 0f); }
                        if (frameN == 8) Press(0f, 0f);
                        God(g);
                        yield return null;
                        frameN++;
                    }
                    yield return Grab48(dir, frame, size, $"X48-blast-{bTag[pass]}-f{k}");
                    if (frame[0] == null) continue;
                    Lum48(frame[0], size[0], size[1], out float box, out float bright, out float mean);
                    float pitch = Mathf.DeltaAngle(0, g.cam.transform.eulerAngles.x);
                    if (k == -2) { box0 = box; bright0 = bright; pitch0 = pitch; }
                    double thrown = 0;
                    for (int i = 0; i < husks.Count; i++) if (husks[i] != null) thrown = Math.Max(thrown, Math.Sqrt((husks[i].position.x - from[i].x) * (husks[i].position.x - from[i].x) + (husks[i].position.z - from[i].z) * (husks[i].position.z - from[i].z)));
                    bool wash = pass >= 1 && k > -2 && (box > box0 + UnityTuning.Fx47WashCentreRise || bright > bright0 + UnityTuning.Fx47WashBrightRise);
                    if (wash) { washed[pass]++; washWhere.Append($" {bTag[pass]} f{k} (+{(box - box0).ToString("0.00", ci)}, halos r{Tell36View.RedHalos}/y{Tell36View.YellowHalos}, shells {Tell36View.ShellsShown});"); }
                    sb.Append($" f{k}: box {box.ToString("0.00", ci)} ({(box - box0 >= 0 ? "+" : "")}{(box - box0).ToString("0.00", ci)}) bright {(bright * 100).ToString("0", ci)} % halos r{Tell36View.RedHalos}/y{Tell36View.YellowHalos} shells {Tell36View.ShellsShown} ring {Fx47Probe.RingRadius.ToString("0.0", ci)} m kick {Fx47Probe.KickDeg.ToString("0.0", ci)} pitch {Mathf.Abs(Mathf.DeltaAngle(pitch0, pitch)).ToString("0.0", ci)} sprites {Fx47Probe.Sprites} thrown {thrown.ToString("0.0", ci)} m{(wash ? " WASH" : "")};");
                    if (pass >= 1)
                    {
                        string src = System.IO.Path.Combine(dir, $"shot-N30-X48-blast-{bTag[pass]}-f{k}.png");
                        string to = (pass == 1) == aIsP ? "P" : "Q";
                        try { if (System.IO.File.Exists(src)) System.IO.File.Copy(src, System.IO.Path.Combine(dir, $"shot-N30-X48-pair-blast-{to}-f{k}.png"), true); } catch (Exception) { }
                    }
                }
                Press(0f, 0f);
                blasted[pass] = p != null && p.blasts > blasts0;
                sb.Append($" blasted {(blasted[pass] ? "yes" : "NO")}, gun {sim.loadout.Weapon}, wash frames {washed[pass]}");
                lines.Add(sb.ToString());
                foreach (Husk h in husks) if (h != null) h.alive = false;
                yield return Wait27(g, 0.8, wallEnd, () => God(g));
            }
            Blast48.ReelVariant = false;
            bool washOk = washed[1] == 0 && washed[2] == 0 && blasted[1] && blasted[2]; // round 5b: a wash meter over a blast that never fired is no reading
            lines.Add($"G20 wash meter {(washOk ? "GREEN" : blasted[1] && blasted[2] ? "RED" : "NO READING (the ON passes did not blast)")}: A {washed[1]}, B {washed[2]} frames over +{UnityTuning.Fx47WashCentreRise.ToString("0.00", ci)} (box) / +{(UnityTuning.Fx47WashBrightRise * 100).ToString("0", ci)} pts (bright) on f-2{(washWhere.Length > 0 ? ":" + washWhere : "")}");
            lines.Add($"G20 blind pair (\"which LT blast hits harder?\", the moments f4 and f12): X48-pair-blast-P / -Q; key (the lead's, after the votes): P = {(aIsP ? "A ring" : "B lance")}, Q = {(aIsP ? "B lance" : "A ring")}");

            // ---- GUNS (G24) + the TORCH (G43) ----
            string[] acts = { "kestrel-tap", "kestrel-charge", "thunder-shot", "thunder-slug", "thunder-boost", "pulse-punch", "fist", "torch", "swap" };
            string[] pTag = { "OFF", "N47", "N48" };
            var ladder = new System.Text.StringBuilder();
            int ladderOut = 0, lighter = 0, unquiet = 0;
            var pairKey = new System.Text.StringBuilder("G24 blind pairs (\"which hits harder?\", OFF vs N48 at f2 and f6; the lead's key, after the votes):");
            offs.Add(g.events.On("fire", o => sheetFires++));
            int acts48 = 0; // round 5b: the action happened (a shot, a swing, a punch, a raise): a pass where it did not is no reading
            offs.Add(g.events.On("fire", o => acts48++));
            offs.Add(g.events.On<MeleeEvent>("meleeSwing", e => { if (e.owner == "player") acts48++; }));
            offs.Add(g.events.On<MeleeEvent>("punch", e => { if (e.owner == "player") acts48++; }));
            offs.Add(g.events.On("weaponRaise", o => acts48++));
            int noAct = 0;
            foreach (string act in acts)
            {
                if (Time.realtimeSinceStartup > wallEnd) { lines.Add($"{act}: the wall clock ran out"); break; }
                string gun = act.StartsWith("kestrel") ? "revolver" : act.StartsWith("thunder") ? "shotgun" : act == "pulse-punch" ? "gauntlet" : act == "torch" ? "torch" : act == "swap" ? "shotgun" : "revolver";
                Strip48 strip = null;
                float[] peak = new float[3], fovPeak = new float[3];
                int[] settle = new int[3];
                bool[] acted = new bool[3];
                var sb = new System.Text.StringBuilder($"{act}:");
                uint ah = 0x48C0u;
                foreach (char ch in act) ah = ah * 31u + ch;
                bool offIsP = new Mulberry32(Seed ^ ah).Next() < 0.5;
                for (int pass = 0; pass < 3; pass++)
                {
                    Fx47Probe.Force = pass >= 1;
                    Guns48.Force = pass == 2;
                    g.Respawn(home, homeYaw * 180 / Math.PI);
                    yield return Equip27(g, gun);
                    g.forcedCombat = null;
                    bool air = act == "thunder-boost";
                    if (g.input != null) g.input.SetLook(g.input.yaw, (air ? -75 : 15) * Math.PI / 180);
                    yield return Wait27(g, UnityTuning.Fx48PrePassS, wallEnd, () => God(g)); // round 2: the last pass's shake / FOV settle first (n48-r1's torch N47 started 0.98 deg off)
                    bool hold = act == "kestrel-charge" || act == "thunder-slug";
                    if (hold)
                    {
                        // (Press makes the pad current)
                        Press(0f, 1f);
                        yield return Wait27(g, act == "kestrel-charge" ? UnityTuning.Fx48ChargeHoldKestrelS : UnityTuning.Fx48ChargeHoldThunderS, wallEnd, () => God(g));
                    }
                    if (air) { g.Respawn(new Vec3(home.x, home.y + UnityTuning.Fx48BoostUpM, home.z), homeYaw * 180 / Math.PI); if (g.input != null) g.input.SetLook(g.input.yaw, -75 * Math.PI / 180); }
                    // round 3 (n48-r2: some N48 passes started with the view still moving, the field of view falling 0.4 -> 2.7 deg and the pitch
                    // drifting to 1.0 deg over the sheet with nothing pressed, so pulse-punch / swap read "lighter"): a QUIET baseline first, the
                    // pitch and the field of view steady (< Fx48QuietDeg a frame) for Fx48QuietFrames frames in a row, at most Fx48QuietMaxS
                    int quiet = 0, quietFrames = 0;
                    float qp = Mathf.DeltaAngle(0, g.cam.transform.eulerAngles.x), qf = g.cam.fieldOfView;
                    float quietEnd = Time.realtimeSinceStartup + UnityTuning.Fx48QuietMaxS;
                    while (quiet < UnityTuning.Fx48QuietFrames && Time.realtimeSinceStartup < quietEnd)
                    {
                        God(g);
                        yield return null;
                        quietFrames++;
                        float np = Mathf.DeltaAngle(0, g.cam.transform.eulerAngles.x), nf = g.cam.fieldOfView;
                        quiet = Mathf.Abs(Mathf.DeltaAngle(qp, np)) < UnityTuning.Fx48QuietDeg && Mathf.Abs(nf - qf) < UnityTuning.Fx48QuietDeg ? quiet + 1 : 0;
                        qp = np;
                        qf = nf;
                    }
                    bool wasQuiet = quiet >= UnityTuning.Fx48QuietFrames;
                    int slot = g.loadout.owned.IndexOf("revolver");
                    float pitch0 = 0, fov0 = 0, mean0 = 0, pk = 0;
                    int pkF = -99, stF = -99, frameN = -2, fires0 = sheetFires, acts0 = acts48;
                    var per = new System.Text.StringBuilder();
                    // round 2: the peak and the settle from EVERY frame -1 .. +30 (n48-r1 read them on the sheet's frames only, which skip 5 and 7:
                    // the same kick read 1.71 or 2.02 deg by where the sample fell; "lighter" was that phase, not the gun)
                    var kseq = new List<(int f, float kick, float fov)>();
                    bool haveBase = false;
                    float fovPk = 0;
                    foreach (int k in Sheet47)
                    {
                        while (frameN < k)
                        {
                            if (frameN == 0)
                            {
                                // (Press makes the pad current)
                                if (act == "swap") g.forcedCombat = new CombatInput { weapon = slot };
                                else if (act == "fist") g.forcedCombat = new CombatInput { weapon = -1, punch = new Skygrind.Core.Button { held = true, pressed = true } };
                                else if (hold) Press(0f, 0f); // the release fires the charged shot
                                else Press(0f, 1f);
                            }
                            if (frameN == 1 && (act == "swap" || act == "fist")) g.forcedCombat = act == "fist" ? (CombatInput?)new CombatInput { weapon = -1 } : null;
                            if (frameN == 2 && !hold && act != "torch" && act != "swap" && act != "fist") Press(0f, 0f);
                            if (frameN == 3 && act == "fist") g.forcedCombat = null;
                            God(g);
                            yield return null;
                            frameN++;
                            if (haveBase)
                            {
                                float pNow = Mathf.DeltaAngle(0, g.cam.transform.eulerAngles.x);
                                kseq.Add((frameN, Mathf.Abs(Mathf.DeltaAngle(pitch0, pNow)), g.cam.fieldOfView - fov0));
                            }
                        }
                        bool pairShot = pass != 1 && (k == 2 || k == 6);
                        string full = pairShot ? $"X48-pair-{act}-{((pass == 0) == offIsP ? "P" : "Q")}-f{k}" : null;
                        yield return Grab48(dir, frame, size, full);
                        if (frame[0] == null) continue;
                        if (strip == null) strip = new Strip48(Sheet47.Length, 3, size[0], size[1], 5);
                        int col = Array.IndexOf(Sheet47, k);
                        strip.Put(col, pass, frame[0], size[0], size[1]);
                        Lum48(frame[0], size[0], size[1], out float box, out float bright, out float mean);
                        float pitch = Mathf.DeltaAngle(0, g.cam.transform.eulerAngles.x), fov = g.cam.fieldOfView;
                        if (k == -2) { pitch0 = pitch; fov0 = fov; mean0 = mean; haveBase = true; }
                        float kick = Mathf.Abs(Mathf.DeltaAngle(pitch0, pitch));
                        per.Append($" f{k} {kick.ToString("0.00", ci)}/{(fov - fov0).ToString("0.0", ci)}/{Fx47Probe.WeaponKickDeg.ToString("0.0", ci)}/{(mean - mean0 >= 0 ? "+" : "")}{(mean - mean0).ToString("0.00", ci)}");
                    }
                    Press(0f, 0f);
                    g.forcedCombat = null;
                    if (act == "torch") { for (int i = 0; i < 4; i++) yield return null; yield return Grab48(dir, frame, size, $"X48-torch-{pTag[pass]}-lick"); }
                    foreach (var (f, kk, fv) in kseq) { if (kk > pk) { pk = kk; pkF = f; } if (fv > fovPk) fovPk = fv; }
                    foreach (var (f, kk, fv) in kseq) if (pkF > -99 && f > pkF && kk < 0.1f * pk) { stF = f; break; }
                    var every = new System.Text.StringBuilder();
                    foreach (var (f, kk, fv) in kseq) every.Append(' ').Append(kk.ToString("0.0", ci));
                    peak[pass] = pk;
                    settle[pass] = stF;
                    fovPeak[pass] = fovPk;
                    sb.Append($" {pTag[pass]} (sheet frames: pitch / fov / fx47 kick / mean-L rise):{per} | every frame f-1..f30 pitch:{every} | peak {pk.ToString("0.00", ci)} deg at f{pkF}, settled f{(stF == -99 ? "none" : stF.ToString(ci))}, fov punch {fovPk.ToString("0.0", ci)} deg, fires {sheetFires - fires0}, acted {acts48 - acts0}, in hand {g.loadout.Weapon}, baseline {(wasQuiet ? "quiet" : "NOT quiet")} after {quietFrames} frames, focused {Application.isFocused}, pad enabled {pad.pad.enabled};");
                    acted[pass] = acts48 - acts0 > 0;
                    if (!acted[pass]) noAct++;
                    if (!wasQuiet) unquiet++;
                    yield return Wait27(g, 0.6, wallEnd, () => God(g));
                }
                if (strip != null) strip.Write(dir, $"X48-sheet-{act}");
                lines.Add(sb.ToString());
                pairKey.Append($" {act}: P = {(offIsP ? "OFF" : "N48")};");
                // the ladder: the N48 peak in its tier's band, settled in the tier's frames, and never lighter than N47 (D-14)
                float lo, hi;
                int settleMax;
                switch (act)
                {
                    case "kestrel-tap": lo = 1.5f; hi = 3.5f; settleMax = 18; break; // today's band (D-14: kept)
                    // round 2: the charged shots and the boost measured on n48-r1 for the first time (a747 never fired them) sit OVER the cards'
                    // bands (KESTREL charge 7.0-7.5, slug 5.1-5.6, boost 6.6): D-14 keeps them; their bands are today's (none turned down)
                    // round 3: n48-r1 -> r2 the same charged shots read 7.0 -> 5.9 (KESTREL) and 5.1 -> 4.0 (slug) OFF: the bands span both runs
                    case "kestrel-charge": lo = 4.5f; hi = 8.5f; settleMax = 30; break; // heavy-plus: the LONGWAVE (OFF 5.86-6.99)
                    case "thunder-shot": lo = 3f; hi = 5f; settleMax = 30; break; // heavy
                    case "thunder-slug": lo = 3f; hi = 7.5f; settleMax = 30; break; // heavy (OFF 4.00-5.06)
                    case "thunder-boost": lo = 4f; hi = 7.5f; settleMax = 30; break; // the shot's kick + fx47's 2.5 deg up kick (N47 6.60)
                    case "pulse-punch": lo = 1.5f; hi = 2.5f; settleMax = 30; break; // the PULSE swing's medium band (NIGHT48.md G24)
                    case "fist": lo = 0.8f; hi = 2.0f; settleMax = 18; break; // light-medium (F-cards 1a: 1.2 deg)
                    case "torch": lo = 0.5f; hi = 1.0f; settleMax = 18; break; // light
                    default: lo = 0.3f; hi = 1.0f; settleMax = 30; break; // the swap's dip
                }
                // the TORCH is a held stream (its low recoil runs while RT is held: it never "settles" inside the sheet): its band only
                bool inBand = peak[2] >= lo && peak[2] <= hi && (act == "torch" || (settle[2] != -99 && settle[2] <= settleMax));
                if (!acted[0] || !acted[1] || !acted[2]) ladder.Append($" [{act}: NOT ACTED in {(acted[0] ? "" : "OFF ")}{(acted[1] ? "" : "N47 ")}{(acted[2] ? "" : "N48")}: no reading]");
                bool light = peak[2] < peak[1] - UnityTuning.Fx48LighterTol && fovPeak[2] < fovPeak[1] + 0.3f; // lighter: a smaller kick and no larger FOV punch
                if (!inBand) ladderOut++;
                if (light) lighter++;
                ladder.Append($" {act} {peak[0].ToString("0.00", ci)} -> {peak[1].ToString("0.00", ci)} -> {peak[2].ToString("0.00", ci)} deg (fov {fovPeak[0].ToString("0.0", ci)} -> {fovPeak[1].ToString("0.0", ci)} -> {fovPeak[2].ToString("0.0", ci)}) [{lo.ToString("0.0", ci)}-{hi.ToString("0.0", ci)}] settled f{(settle[2] == -99 ? "none" : settle[2].ToString(ci))} {(inBand ? "IN" : "OUT")}{(light ? " LIGHTER" : "")};");
            }
            Fx47Probe.Force = fxWas;
            Guns48.Force = g48Was;
            lines.Add($"G24 ladder {(noAct > 0 ? "NO READING" : ladderOut == 0 && lighter == 0 ? "GREEN" : "RED")} (OFF -> N47 -> N48 peak; {ladderOut} out of band, {lighter} lighter than N47; passes without a quiet baseline {unquiet}; passes where the action never happened {noAct}):{ladder}");
            lines.Add($"input (round 5b): focused at the start {focused0}, now {Application.isFocused}; backgroundBehavior {bgWas} -> IgnoreFocus for the reel; the reel pad {(padWasOff ? "added DISABLED" : "added enabled")}, re-enabled {reEnabled} times, presses while unfocused {unfocusedPresses}");
            lines.Add(pairKey.ToString());
            yield return Stop();
            bool ok = blasted[0] && blasted[1] && blasted[2];
            res[0] = $"{(ok ? "GREEN" : "RED")} a748 fx48 | {string.Join(" || ", lines)} | {pc}";
        }
    }
}
