// NIGHT49 builder X: the a249 phase, the IMPACT KIT reel (G5, design.n49.fx49; Fx49.cs). Run:
//   reel.sh + -skygrindMap <a campaign level, e.g. n47-iceline> + -skygrindReelOnly a249
// The CAMPAIGN on the booted map, DOORFREE, God, the player's own input + the reel's virtual pad (the n33 recipe: IgnoreFocus for the reel's
// length, the pad enabled before every press; the line prints `fires` / `acted` per pass: no action = NO READING). Every effect, two passes:
// OFF (Fx49.Force false: each effect's own look, the rows as the build has them) and ON (Fx49.Force true: the kit). Effects (in sheet order):
//   kestrel-tap, kestrel-charge, thunder-shot, thunder-slug (a husk at 5 m on the aim), thunder-boost (3 m up, looking down 75 deg), pulse-punch
//   (a husk at 1.6 m), pulse-blast (LT at f0, husks at 3 / 5 m), fist (a husk at 1.4 m), torch (RT held, a husk at 4 m), kill (a 1 hp husk at
//   5 m), parry (the reel emits "parry36" at 3 m ahead, off the aim: a real parry needs a glob in flight), hurt (the reel emits "hurt" from a
//   husk at 4 m, 30 deg right), boss-weak (no OPEN boss on a campaign room: the ON cell is the kit's critical spawned by the reel at 8 m, the
//   OFF cell feel43's look is NOT staged; the line says so).
// Frames -2, 0, 1, 2, 3, 4, 6, 8, 12 per pass. The PEAK frame of each pass = the frame with the most luma change against f-2 (a 64 x 36 grid);
// the contact sheets X49-sheet-off / X49-sheet-on put each effect's peak frame in a 5 x 3 grid at 1/4 size (the order above, row by row); the
// blind pair copies them as X49-pair-P / -Q by a seeded coin ("which set of effects looks like one game?"; the key on the line).
// The WASH meter: a frame whose WASH box (the middle 20 %: 384 x 216 px at 1080p, FeelReel.N31.Grapple.cs:301) mean luma (Rec. 709 on the
// gamma values) rose more than +0.25 over the pass's f-2. GREEN = 0 ON frames over it, with every ON pass acted.
// The line per effect: OFF / ON peak frame and its change, the wash box's peak rise, fires / acted, and from the kit's probe its tier, owner
// colour and the kit's peak frame (Fx49.Note: the first frame past the tier's rise).
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
        static partial void N49Fx(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Fx49Scenario(g, dir, res, offs, pre);

        static readonly int[] Sheet49 = { -2, 0, 1, 2, 3, 4, 6, 8, 12 };
        const float Wash49Rise = 0.25f; // the wash meter's bar (NIGHT49.md G5: no frame's WASH box > +0.25 over f-2)

        /// <summary>The WASH box's mean (the middle 20 % of width and height) and a 64 x 36 luma grid of a grabbed frame (Rec. 709 on the gamma values).</summary>
        static float Wash49(Color32[] px, int w, int h, float[] grid)
        {
            double s = 0;
            for (int y = 0; y < 24; y++)
                for (int x = 0; x < 24; x++)
                {
                    int sx = (int)(w * (0.4f + 0.2f * (x + 0.5f) / 24f)), sy = (int)(h * (0.4f + 0.2f * (y + 0.5f) / 24f));
                    Color32 c = px[sy * w + sx];
                    s += (0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b) / 255.0;
                }
            for (int y = 0; y < 36; y++)
                for (int x = 0; x < 64; x++)
                {
                    Color32 c = px[((y * h) / 36) * w + (x * w) / 64];
                    grid[y * 64 + x] = (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b) / 255f;
                }
            return (float)(s / (24 * 24));
        }

        static IEnumerator Fx49Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            bool? fxWas = Fx49.Force;
            offs.Add(() =>
            {
                Fx49.Force = fxWas;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a249: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 600;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 580) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput); // the player's input (NIGHT47's a747 lesson: the bot's never reads the pad)
            g.forcedMove = null;
            // the n33 recipe (FeelReel.N33.Shop.cs; NIGHT48 a748 round 5b): IgnoreFocus for the reel, the pad enabled before every press
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
            yield return Equip27(g, "revolver");
            g.forcedCombat = null;
            string pc = Preconditions(g, g.Feet);
            Start(g, "a249-fx49", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string> { $"fx49 row {(NightRows.ViewOn(g.cfg, "fx49") ? "ON" : "off")} (the reel forces OFF / ON), fx47 {(Fx47View.RowOn(g.cfg) ? "ON" : "off")}, sim input {(sim.input != null ? sim.input.GetType().Name : "null")}, pad current {Gamepad.current == pad.pad}, frame {Screen.width}x{Screen.height}" };
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            Vec3 home = g.Feet;
            double homeYaw = g.input != null ? g.input.yaw : 0;
            int acts = 0, fires = 0;
            offs.Add(g.events.On("fire", o => { fires++; acts++; }));
            offs.Add(g.events.On<MeleeEvent>("meleeSwing", e => { if (e.owner == "player") acts++; }));
            offs.Add(g.events.On<MeleeEvent>("punch", e => { if (e.owner == "player") acts++; }));
            offs.Add(g.events.On<Pulse27Event>("pulse27Blast", e => acts++));
            string[] fx = { "kestrel-tap", "kestrel-charge", "thunder-shot", "thunder-slug", "thunder-boost", "pulse-punch", "pulse-blast", "fist", "torch", "kill", "parry", "hurt", "boss-weak" };
            Strip48[] sheets = new Strip48[2];
            int[] washed = new int[2];
            int noAct = 0;
            // round 2 (D-17): the wash box's peak rise per kit tier (light / heavy / critical), OFF and ON, by the tier the kit drew for the effect
            float[] tierOff = new float[3], tierOn = new float[3];
            float offPk = 0;
            var washWhere = new System.Text.StringBuilder();
            float[] grid0 = new float[64 * 36], grid = new float[64 * 36];
            for (int ei = 0; ei < fx.Length; ei++)
            {
                string act = fx[ei];
                if (Time.realtimeSinceStartup > wallEnd) { lines.Add($"{act}: the wall clock ran out"); break; }
                string gun = act.StartsWith("thunder") ? "shotgun" : act.StartsWith("pulse") ? "gauntlet" : act == "torch" ? "torch" : "revolver";
                var sb = new System.Text.StringBuilder($"{act}:");
                for (int pass = 0; pass < 2; pass++)
                {
                    Fx49.Force = pass == 1;
                    Fx49.Probe.Remove(act);
                    g.Respawn(home, homeYaw * 180 / Math.PI);
                    yield return Equip27(g, gun);
                    g.forcedCombat = null;
                    bool air = act == "thunder-boost";
                    if (g.input != null) g.input.SetLook(g.input.yaw, (air ? -75 : -6) * Math.PI / 180);
                    double yaw = g.input != null ? g.input.yaw : 0;
                    double fxx = -Math.Sin(yaw), fzz = -Math.Cos(yaw);
                    // the targets
                    var husks = new List<Husk>();
                    double[] at = act == "pulse-punch" ? new[] { 1.6 } : act == "fist" ? new[] { 1.4 } : act == "torch" ? new[] { 4.0 } : act == "pulse-blast" ? new[] { 3.0, 5.0 }
                        : act == "thunder-boost" || act == "parry" || act == "hurt" || act == "boss-weak" ? new double[0] : new[] { 5.0 };
                    foreach (double d in at)
                    {
                        Husk h = null;
                        try { h = sim.husks.Spawn("husk", new Vec3(g.Feet.x + fxx * d, g.Feet.y, g.Feet.z + fzz * d), g.Feet); } catch (Exception) { }
                        if (h != null) { h.hp = h.maxHp = act == "kill" ? 1 : 1e6; husks.Add(h); }
                    }
                    g.husks.frozen = true;
                    PulseCharge27 p = sim.pulse27;
                    yield return Wait27(g, 0.5, wallEnd, () => { God(g); if (p != null) { p.pips = p.p.pips; p.cooldownSteps = 0; } });
                    if (sim.loadout.Weapon != gun) { yield return Equip27(g, gun); g.forcedCombat = null; }
                    bool hold = act == "kestrel-charge" || act == "thunder-slug";
                    if (hold)
                    {
                        Press(0f, 1f);
                        yield return Wait27(g, act == "kestrel-charge" ? UnityTuning.Fx48ChargeHoldKestrelS : UnityTuning.Fx48ChargeHoldThunderS, wallEnd, () => God(g));
                    }
                    if (air) { g.Respawn(new Vec3(home.x, home.y + UnityTuning.Fx48BoostUpM, home.z), homeYaw * 180 / Math.PI); if (g.input != null) g.input.SetLook(g.input.yaw, -75 * Math.PI / 180); }
                    for (int q = 0; q < 8; q++) { God(g); yield return null; } // a short settle (the respawn's camera)
                    int frameN = -2, acts0 = acts, fires0 = fires, f0Frame = -1;
                    bool emitted = false;
                    float wash0 = 0, washPk = 0, bestE = -1;
                    int bestK = -99;
                    Color32[] best = null;
                    foreach (int k in Sheet49)
                    {
                        while (frameN < k)
                        {
                            if (frameN == 0)
                            {
                                f0Frame = Time.frameCount;
                                Transform ct = g.cam != null ? g.cam.transform : null;
                                if (act == "fist") g.forcedCombat = new CombatInput { weapon = -1, punch = new Skygrind.Core.Button { held = true, pressed = true } };
                                else if (act == "pulse-blast") Press(1f, 0f);
                                else if (hold) Press(0f, 0f); // the release fires the charged shot
                                else if (act == "parry" && ct != null)
                                {
                                    Vector3 pAt = ct.position + ct.forward * 3f + ct.right * 0.5f - ct.up * 0.3f;
                                    g.events.Emit(Parry36.Event, new HitEvent { owner = "player", weapon = "gauntlet", kind = "spitterGlob", enemy = -1, position = WebSpace.FromUnity(pAt) });
                                    emitted = true;
                                }
                                else if (act == "hurt" && ct != null)
                                {
                                    Vector3 from = ct.position + Quaternion.AngleAxis(30f, Vector3.up) * Vector3.ProjectOnPlane(ct.forward, Vector3.up).normalized * 4f;
                                    g.events.Emit("hurt", new HurtEvent { owner = "player", amount = 20, hp = g.health.hp, source = "husk", from = WebSpace.FromUnity(from), hasFrom = true });
                                    emitted = true;
                                }
                                else if (act == "boss-weak" && ct != null)
                                {
                                    if (pass == 1) { Fx49Kit.Of(g)?.Spawn("boss-weak", ct.position + ct.forward * 8f + ct.right * 0.8f + ct.up * 0.4f, Fx49Owner.Player, Fx49Tier.Critical, -1, 0, default, UnityTuning.Fx49BossWeakScale); emitted = true; }
                                }
                                else Press(0f, 1f);
                            }
                            if (frameN == 1 && act == "fist") g.forcedCombat = new CombatInput { weapon = -1 };
                            if (frameN == 2 && !hold && act != "torch" && act != "pulse-blast" && act != "fist") Press(0f, 0f);
                            if (frameN == 3 && act == "fist") g.forcedCombat = null;
                            if (frameN == 8 && act == "pulse-blast") Press(0f, 0f);
                            if (frameN == 0) g.husks.frozen = act != "pulse-blast"; // the blast throws its bodies
                            God(g);
                            yield return null;
                            frameN++;
                        }
                        yield return Grab48(dir, frame, size, null);
                        if (frame[0] == null) continue;
                        if (k == -2) { wash0 = Wash49(frame[0], size[0], size[1], grid0); continue; }
                        float wash = Wash49(frame[0], size[0], size[1], grid);
                        float e = 0;
                        for (int i = 0; i < grid.Length; i++) { float dl = grid[i] - grid0[i]; if (dl > 0) e += dl; }
                        if (e > bestE) { bestE = e; bestK = k; best = frame[0]; }
                        if (wash - wash0 > washPk) washPk = wash - wash0;
                        if (wash - wash0 > Wash49Rise) { washed[pass]++; washWhere.Append($" {(pass == 1 ? "ON" : "OFF")} {act} f{k} (+{(wash - wash0).ToString("0.00", ci)});"); }
                    }
                    Press(0f, 0f);
                    g.forcedCombat = null;
                    if (best != null)
                    {
                        if (sheets[pass] == null) sheets[pass] = new Strip48(5, 3, size[0], size[1], 4);
                        sheets[pass].Put(ei % 5, ei / 5, best, size[0], size[1]);
                    }
                    bool acted = acts - acts0 > 0 || emitted;
                    if (!acted && act != "boss-weak") noAct++;
                    if (pass == 0) offPk = washPk;
                    string kit = "";
                    if (pass == 1)
                    {
                        if (Fx49.Probe.TryGetValue(act, out Fx49.Seen s))
                        {
                            kit = $", kit: tier {s.tier}, owner {Fx49.OwnerName(s.owner)}, kit peak f{(f0Frame >= 0 ? s.peakFrame - f0Frame : -99)} (x{s.count})";
                            int ti = (int)s.tier;
                            tierOn[ti] = Mathf.Max(tierOn[ti], washPk);
                            tierOff[ti] = Mathf.Max(tierOff[ti], offPk);
                        }
                        else kit = ", kit: NOT SEEN (no Fx49.Note for it)";
                    }
                    sb.Append($" {(pass == 1 ? "ON" : "OFF")}: peak f{bestK} (luma change {bestE.ToString("0.0", ci)}), wash box peak +{washPk.ToString("0.00", ci)}, fires {fires - fires0}, acted {(acted ? acts - acts0 + (emitted ? 1 : 0) : 0)}{(emitted ? " (emitted)" : "")}, in hand {sim.loadout.Weapon}{kit};");
                    if (act == "boss-weak" && pass == 0) sb.Append(" (OFF not staged: no OPEN boss on a campaign room; feel43's look not on the sheet);");
                    foreach (Husk h in husks) if (h != null) h.alive = false;
                    yield return Wait27(g, 0.5, wallEnd, () => God(g));
                }
                lines.Add(sb.ToString());
            }
            Fx49.Force = fxWas;
            bool pIsOn = new Mulberry32(Seed ^ 0x49F0u).Next() < 0.5;
            for (int pass = 0; pass < 2; pass++)
            {
                if (sheets[pass] == null) continue;
                sheets[pass].Write(dir, pass == 1 ? "X49-sheet-on" : "X49-sheet-off");
                sheets[pass].Write(dir, (pass == 1) == pIsOn ? "X49-pair-P" : "X49-pair-Q");
            }
            bool washOk = washed[1] == 0 && noAct == 0;
            lines.Add($"G5 wash peak per kit tier (the middle 20 % box over f-2, OFF -> ON): light +{tierOff[0].ToString("0.00", ci)} -> +{tierOn[0].ToString("0.00", ci)}, heavy +{tierOff[1].ToString("0.00", ci)} -> +{tierOn[1].ToString("0.00", ci)}, critical +{tierOff[2].ToString("0.00", ci)} -> +{tierOn[2].ToString("0.00", ci)} (bar +{Wash49Rise.ToString("0.00", ci)})");
            lines.Add($"G5 wash meter {(noAct > 0 ? "NO READING (a pass did not act)" : washOk ? "GREEN" : "RED")}: ON {washed[1]} frames, OFF {washed[0]} frames over +{Wash49Rise.ToString("0.00", ci)} on f-2 (the middle 20 % box){(washWhere.Length > 0 ? ":" + washWhere : "")}");
            lines.Add($"G5 sheets: X49-sheet-off / X49-sheet-on (5 x 3, row by row: {string.Join(", ", fx)}); blind pair \"which set of effects looks like one game?\": X49-pair-P / -Q; key (the lead's, after the votes): P = {(pIsOn ? "ON (fx49)" : "OFF")}, Q = {(pIsOn ? "OFF" : "ON (fx49)")}");
            lines.Add($"input: focused at the start {focused0}, now {Application.isFocused}; backgroundBehavior {bgWas} -> IgnoreFocus; the pad {(padWasOff ? "added DISABLED" : "added enabled")}, re-enabled {reEnabled} times, presses while unfocused {unfocusedPresses}; passes that did not act {noAct}; kit bursts {Fx49.Bursts}");
            yield return Stop();
            res[0] = $"{(washOk ? "GREEN" : "RED")} a249 fx49 | {string.Join(" || ", lines)} | {pc}";
        }
    }
}
