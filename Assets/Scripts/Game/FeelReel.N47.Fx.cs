// NIGHT47 builder F: the a747 phase (FeelReel.N47.cs routes it here): the weapons' proof frames. Round 1: the LT blast's contact sheet.
//   reel.sh + -skygrindMap n46-foundry + -skygrindReelOnly a747   (pulse47 ON in the build's copy for the sim's launch; fx47 is flipped here)
// The CAMPAIGN on the booted map, DOORFREE, God, enemies frozen; the gauntlet given and in hand; three husks stood 3 / 5 / 7 m ahead on the
// spawn's look (a body to throw); the glove filled (5 pips) and its cooldown cleared before each press; LT pressed. Two passes in this Player:
// fx47 OFF (today's picture: no blast look) then ON (variant A "ring"). The contact sheet per pass (NIGHT47.md section 4: fire -2, 0, +1, +2,
// +3, +4, +6, +8, +12, +18, +30 render frames): shot-N30-X47-blast-<off|on>-f<k>.png. The line: per frame the camera's pitch kick (Fx47Probe),
// the ring's radius, the core flash, the sprites drawn, and the husks' distance from where they stood (the sim's throw: pulse47 on or off).
// GREEN when both passes blasted, the ON pass drew the look (sprites > 0 at +1, the ring > 0 by +6, a kick > 0) and the OFF pass drew none.
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
        static readonly int[] Sheet47 = { -2, 0, 1, 2, 3, 4, 6, 8, 12, 18, 30 };
        static int sheetFires, sheetSwings; // the sheets' counters (round 3)

        /// <summary>G21's frames (round 4: "each punch's start / hit / fade: the fist blue, the gauntlet pink, an enemy's red-orange and a parryable
        /// yellow"), fx47 forced on. The player's side: a husk stood 2.2 m ahead (frozen, near-endless health); the FIST (the punch button, any gun in
        /// hand: the KESTREL) and the GAUNTLET's swing (RT), frames at +1 (the start: the glove lit), +5 (the hit: the aura at the body), +18 (the
        /// fade). The enemies' side: a SWARMER (its pounce: an unparryable red-orange tell) and a HUSK (its melee: a parryable yellow tell) stood 3 m
        /// ahead, live (God on), waited for until their tell lights (Tell36View's Red / Yellow halos), frames at its first lit frame, +0.25 s, +0.6 s.
        /// shot-N30-X47-aura-&lt;fist|gauntlet|red|yellow&gt;-&lt;start|hit|fade&gt;.png; the line: the auras / halos counted per frame.</summary>
        static IEnumerator Aura47Frames(SurvivalGame g, string dir, List<string> lines, Pad27 pad, List<Action> offs, double wallEnd, int[] mag)
        {
            bool? fxWas = Fx47Probe.Force;
            offs.Add(() => Fx47Probe.Force = fxWas);
            Fx47Probe.Force = true;
            var sb = new System.Text.StringBuilder("auras (G21, fx47 on):");
            SurvivalSim sim = g.sim;
            foreach (string who in new[] { "fist", "gauntlet" })
            {
                if (Time.realtimeSinceStartup > wallEnd) break;
                yield return Equip27(g, who == "fist" ? "revolver" : "gauntlet");
                g.forcedCombat = null;
                Husk h = PlaceAt(g, 2.2, 0, out _);
                if (h == null) { sb.Append($" {who}: no husk placed;"); continue; }
                h.hp = h.maxHp = 1e9;
                g.husks.frozen = true;
                for (int i = 0; i < 20; i++) { God(g); Look(g, h.AimPoint); yield return null; }
                int auras0 = Fx47Probe.Auras;
                if (who == "fist") g.forcedCombat = new CombatInput { weapon = -1, punch = new Skygrind.Core.Button { held = true, pressed = true } };
                else { pad.pad.MakeCurrent(); pad.Send(0f, 1f); }
                int frame = 0;
                sb.Append($" {who}:");
                foreach ((int k, string tag) in new[] { (1, "start"), (5, "hit"), (18, "fade") })
                {
                    while (frame < k)
                    {
                        God(g);
                        Look(g, h.AimPoint);
                        yield return null;
                        frame++;
                        if (frame == 1) g.forcedCombat = who == "fist" ? (CombatInput?)new CombatInput { weapon = -1 } : null; // the punch released
                        if (frame == 2) { pad.Send(0f, 0f); g.forcedCombat = null; }
                    }
                    yield return Capture47(dir, $"X47-aura-{who}-{tag}", mag);
                    sb.Append($" {tag} auras +{Fx47Probe.Auras - auras0} sprites {Fx47Probe.Sprites};");
                }
                g.forcedCombat = null;
                h.alive = false;
                g.husks.frozen = false;
                yield return Wait27(g, 0.6, wallEnd, () => God(g));
            }
            foreach (string who in new[] { "red", "yellow" })
            {
                if (Time.realtimeSinceStartup > wallEnd) break;
                Husk e = PlaceAt(g, 3.0, 0, out _);
                if (e != null) e.alive = false; // PlaceAt stands a husk: the yellow pass uses it, the red one a swarmer in its place
                Husk body = null;
                try
                {
                    Vec3 at = e != null ? e.position : g.Feet;
                    body = sim.husks.Spawn(who == "red" ? "swarmer" : "husk", at, g.Feet);
                }
                catch (Exception) { }
                if (body == null) { sb.Append($" {who}: no body spawned;"); continue; }
                body.hp = body.maxHp = 1e9;
                g.husks.frozen = false;
                float until = Time.realtimeSinceStartup + 8f;
                bool lit = false;
                while (Time.realtimeSinceStartup < until)
                {
                    God(g);
                    Look(g, body.AimPoint);
                    yield return null;
                    if (who == "red" ? Tell36View.RedHalos > 0 : Tell36View.YellowHalos > 0) { lit = true; break; }
                }
                sb.Append($" {who} ({(who == "red" ? "swarmer pounce" : "husk melee")}): tell {(lit ? "lit" : "NOT seen in 8 s")};");
                if (lit)
                {
                    foreach ((float s, string tag) in new[] { (0f, "start"), (0.25f, "hit"), (0.6f, "fade") })
                    {
                        float t0 = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - t0 < s) { God(g); yield return null; }
                        yield return Capture47(dir, $"X47-aura-{who}-{tag}", mag);
                        sb.Append($" {tag} halos red {Tell36View.RedHalos} yellow {Tell36View.YellowHalos};");
                    }
                }
                body.alive = false;
                yield return Wait27(g, 0.5, wallEnd, () => God(g));
            }
            lines.Add(sb.ToString());
        }

        /// <summary>G24 round 3 (the lead's polish list: "the THUNDERCLAP slug and the swap"): more contact sheets, fx47 on, the aim 15 deg up into the
        /// air. The KESTREL charged and the THUNDERCLAP slug (RT held past the charge's hold + FULL, released at frame 0: the charged shot; NIGHT27
        /// chargeShots27), and the swap (the THUNDERCLAP in hand, the KESTREL's slot asked at frame 0). Per frame -2 .. +30 the camera's pitch against
        /// frame -2's, its peak and settle; frames shot-N30-X47-sheet-&lt;charged|slug|swap&gt;-f&lt;k&gt;.png; the ladder line against F-cards 1a: KESTREL
        /// charged medium-heavy 1.5-3.5 deg, slug heavy 3-5 (4.5 the card's), swap a 0.3-1.0 deg dip.</summary>
        static IEnumerator Sheets47More(SurvivalGame g, string dir, List<string> lines, Pad27 pad, List<Action> offs, double wallEnd, int[] mag)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            bool? fxWas = Fx47Probe.Force;
            offs.Add(() => Fx47Probe.Force = fxWas);
            Fx47Probe.Force = true;
            ChargeShots27 c = g.sim.charge27;
            var sb = new System.Text.StringBuilder("sheets more (fx47 on):");
            var ladder = new System.Text.StringBuilder();
            int outN = 0;
            foreach (string kind in new[] { "charged", "slug", "swap" })
            {
                if (Time.realtimeSinceStartup > wallEnd) break;
                string gun = kind == "charged" ? "revolver" : "shotgun";
                if (kind != "swap" && (c == null || !c.active)) { sb.Append($" {kind}: no charge shots (chargeShots27 off);"); continue; }
                yield return Equip27(g, gun);
                g.forcedCombat = null;
                if (g.input != null) g.input.SetLook(g.input.yaw, 15 * Math.PI / 180);
                yield return Wait27(g, 0.8, wallEnd, () => God(g));
                int fires0 = sheetFires;
                if (kind != "swap")
                {
                    // the charge: RT held past its hold + FULL (game time), then the sheet starts on the release
                    int gi = ChargeShots27.Index(gun);
                    double fullHold = c.p.holdSeconds + c.FullSteps(gi) * g.cfg.Dt + 0.15;
                    pad.pad.MakeCurrent();
                    pad.Send(0f, 1f);
                    yield return Wait27(g, fullHold, wallEnd, () => God(g));
                }
                float pitch0 = 0, peak = 0;
                int peakF = -99, settleF = -99, frame = -2;
                int slot = g.loadout.owned.IndexOf("revolver");
                sb.Append($" {kind}:");
                foreach (int k in Sheet47)
                {
                    while (frame < k)
                    {
                        if (frame == 0)
                        {
                            if (kind == "swap") g.forcedCombat = new CombatInput { weapon = slot };
                            else pad.Send(0f, 0f); // the release fires the charged shot
                        }
                        if (frame == 1 && kind == "swap") g.forcedCombat = null;
                        God(g);
                        yield return null;
                        frame++;
                    }
                    yield return Capture47(dir, $"X47-sheet-{kind}-f{k}", mag);
                    float p = Mathf.DeltaAngle(0, g.cam.transform.eulerAngles.x);
                    if (k == -2) pitch0 = p;
                    float kick = Mathf.Abs(Mathf.DeltaAngle(pitch0, p));
                    if (kick > peak) { peak = kick; peakF = k; }
                    if (peakF > -99 && k > peakF && settleF == -99 && kick < 0.1f * peak) settleF = k;
                    sb.Append($" f{k} {kick.ToString("0.00", ci)}");
                }
                g.forcedCombat = null;
                pad.Send(0f, 0f);
                sb.Append($" | peak {peak.ToString("0.00", ci)} deg at f{peakF}, settled by f{(settleF == -99 ? "(not by +30)" : settleF.ToString(ci))}, fires {sheetFires - fires0}, in hand {g.loadout.Weapon};");
                float lo = kind == "slug" ? 3f : kind == "charged" ? 1.5f : 0.3f, hi = kind == "slug" ? 5f : kind == "charged" ? 3.5f : 1.0f;
                bool inBand = peak >= lo && peak <= hi && settleF != -99;
                ladder.Append($" {kind} {peak.ToString("0.00", ci)} deg [{lo.ToString("0.0", ci)}-{hi.ToString("0.0", ci)}] {(inBand ? "IN" : "OUT")};");
                if (!inBand) outN++;
                yield return Wait27(g, 0.8, wallEnd, () => God(g));
            }
            lines.Add(sb.ToString());
            lines.Add($"G24 ladder more {(outN == 0 ? "GREEN" : "RED")}:{ladder}");
        }

        /// <summary>R4 finale47's frames: the views' beat (Finale47.TestEvent: the views hear it as bossDown; the sim does not) for a boss "THE
        /// ICEBREAKER" 10 m ahead, finale47 forced off then on; frames +2, +12, +60 render frames: shot-N30-X47-finale-&lt;off|on&gt;-f&lt;k&gt;.png; the
        /// line: the title, its opacity, the camera's added kick per frame (Fx47Probe.WeaponKickDeg) and the frame's centre box / bright share.</summary>
        static IEnumerator Finale47Frames(SurvivalGame g, string dir, List<string> lines, List<Action> offs, int[] mag)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            bool? was = Finale47.Force, fxWas = Fx47Probe.Force;
            offs.Add(() => { Finale47.Force = was; Fx47Probe.Force = fxWas; });
            Fx47Probe.Force = true; // the arena's part rides fx47's sprites and kicks
            float[] wash = new float[3];
            var sb = new System.Text.StringBuilder("finale47:");
            double yaw = g.input != null ? g.input.yaw : 0;
            for (int pass = 0; pass < 2; pass++)
            {
                Finale47.Force = pass == 1;
                int beats0 = Finale47.Beats;
                var at = new Vec3(g.Feet.x - Math.Sin(yaw) * 10, g.Feet.y, g.Feet.z - Math.Cos(yaw) * 10);
                var e = new BossEvent { boss = "icebreaker", name = "THE ICEBREAKER", position = at };
                g.events.Emit(Finale47.TestEvent, e);
                int frame = 0;
                sb.Append(pass == 1 ? " ON" : " off");
                foreach (int k in new[] { 2, 12, 60 })
                {
                    while (frame < k) { God(g); yield return null; frame++; }
                    yield return Capture47(dir, $"X47-finale-{(pass == 1 ? "on" : "off")}-f{k}", mag, wash);
                    sb.Append($" f{k}: title '{(pass == 1 ? Finale47.Title : "")}' alpha {Finale47.TitleAlpha.ToString("0.00", ci)} kick {Fx47Probe.WeaponKickDeg.ToString("0.0", ci)} deg centre L {wash[1].ToString("0.00", ci)} bright {(wash[2] * 100).ToString("0", ci)} %;");
                }
                sb.Append($" beats +{Finale47.Beats - beats0};");
                for (int i = 0; i < 90; i++) { God(g); yield return null; }
            }
            lines.Add(sb.ToString());
        }

        /// <summary>R4 hitzone47's frames: a husk stood 6 m ahead (frozen in place, near-endless health); the KESTREL tapped at its HEAD, its LEGS and
        /// its SIDE, hitzone47 forced off then on; frames at +4 render frames after each shot: shot-N30-X47-zone-&lt;off|on&gt;-&lt;head|legs|side&gt;.png;
        /// the line: the zone each ON shot read (EnemyLook.Zone47Last) and the reactions started.</summary>
        static IEnumerator Zone47Frames(SurvivalGame g, string dir, List<string> lines, Pad27 pad, List<Action> offs, double wallEnd, int[] mag)
        {
            bool? was = EnemyLook.Zone47Force;
            offs.Add(() => EnemyLook.Zone47Force = was);
            yield return Equip27(g, "revolver");
            g.forcedCombat = null;
            Husk h = PlaceAt(g, 6, 0, out _);
            if (h == null) { lines.Add("hitzone47: no husk placed"); yield break; }
            h.hp = h.maxHp = 1e9;
            g.husks.frozen = true;
            var sb = new System.Text.StringBuilder("hitzone47:");
            string[] zones = { "head", "legs", "side" };
            for (int pass = 0; pass < 2 && Time.realtimeSinceStartup < wallEnd; pass++)
            {
                EnemyLook.Zone47Force = pass == 1;
                sb.Append(pass == 1 ? " ON" : " off");
                foreach (string z in zones)
                {
                    double ht = h.Height;
                    var aim = new Vec3(h.position.x, h.position.y + (z == "head" ? ht * 0.9 : z == "legs" ? ht * 0.18 : ht * 0.55), h.position.z);
                    if (z == "side")
                    {
                        // the shooter's right edge of the body
                        double dx = h.position.x - g.Eye.x, dz = h.position.z - g.Eye.z, dl = Math.Sqrt(dx * dx + dz * dz);
                        if (dl > 1e-6) aim = new Vec3(aim.x - dz / dl * h.Radius * 0.75, aim.y, aim.z + dx / dl * h.Radius * 0.75);
                    }
                    int n0 = EnemyLook.Zone47Count;
                    for (int i = 0; i < 20; i++) { God(g); Look(g, aim); yield return null; }
                    pad.pad.MakeCurrent();
                    pad.Send(0f, 1f);
                    for (int i = 0; i < 2; i++) { God(g); Look(g, aim); yield return null; }
                    pad.Send(0f, 0f);
                    for (int i = 0; i < 2; i++) { God(g); yield return null; }
                    yield return Capture47(dir, $"X47-zone-{(pass == 1 ? "on" : "off")}-{z}", mag);
                    sb.Append($" {z}: read {(EnemyLook.Zone47Count > n0 ? EnemyLook.Zone47Last.ToString(CultureInfo.InvariantCulture) : "-")};");
                    yield return Wait27(g, 0.8, wallEnd, () => God(g));
                }
            }
            h.alive = false;
            g.husks.frozen = false;
            lines.Add(sb + " (0 head, 1 legs, 2 side; off reads none)");
        }

        /// <summary>G20's blind pair (NIGHT47.md: 3 judges, "which blast feels heavier?"; a tie keeps the new ON): the A ("ring") and B ("lance")
        /// passes' frames copied as shot-N30-X47-pair-P-f&lt;k&gt;.png / -Q- in an order a seeded coin picks (the run's Seed: the same each run of a
        /// build), so the judges see P / Q, never A / B; the key is the returned line (the lead reads it after the votes).</summary>
        static string BlindPair47(string dir)
        {
            bool aIsP = new Mulberry32(Seed ^ 0x47ABu).Next() < 0.5;
            int copied = 0;
            foreach (int k in Sheet47)
            {
                foreach (var (from, to) in new[] { ("on", aIsP ? "P" : "Q"), ("onB", aIsP ? "Q" : "P") })
                {
                    string src = System.IO.Path.Combine(dir, $"shot-N30-X47-blast-{from}-f{k}.png");
                    if (!System.IO.File.Exists(src)) continue;
                    try { System.IO.File.Copy(src, System.IO.Path.Combine(dir, $"shot-N30-X47-pair-{to}-f{k}.png"), true); copied++; }
                    catch (Exception) { /* a locked file: the line says how many */ }
                }
            }
            return $"blind pair: {copied} frames as X47-pair-P / -Q; key (for the lead, after the votes): P = {(aIsP ? "A ring" : "B lance")}, Q = {(aIsP ? "B lance" : "A ring")}";
        }

        /// <summary>n47-r1d ("large flat angular slabs 1-3 m in front of the camera": log what is near the eye): a kill's blocks this frame (drawn /
        /// culled within Fx47NearEye under fx47, the nearest's distance), the nearest standing body's distance (its centre) and whether it is
        /// tumbling (launched), the live burst shells.</summary>
        static string Near47(SurvivalGame g, List<Husk> husks, CultureInfo ci)
        {
            Vector3 eye = g.cam != null ? g.cam.transform.position : Vector3.zero;
            float body = float.PositiveInfinity;
            bool tumbling = false;
            foreach (Husk h in husks)
            {
                if (h == null || !h.alive) continue;
                float d = Vector3.Distance(eye, WebSpace.ToUnity(new Vec3(h.position.x, h.position.y + h.Height * 0.5, h.position.z)));
                if (d < body) { body = d; tumbling = h.Airborne && h.knockSteps > 0; }
            }
            string near = float.IsPositiveInfinity(Fx47Probe.ChunkNearest) ? "-" : Fx47Probe.ChunkNearest.ToString("0.0", ci);
            string b = float.IsPositiveInfinity(body) ? "-" : body.ToString("0.0", ci);
            return $" | kill blocks {Fx47Probe.ChunksLive} (culled {Fx47Probe.ChunksCulled}, nearest {near} m), nearest body {b} m{(tumbling ? " tumbling" : "")}, shells {Tell36View.ShellsShown}";
        }

        static partial void N47Fx(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Fx47Scenario(g, dir, res, offs, pre);

        static IEnumerator Fx47Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            AutopilotInput ap = N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            bool? fxWas = Fx47Probe.Force;
            offs.Add(() =>
            {
                Fx47Probe.Force = fxWas;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
            });
            if (!sim.campaignOn) { res[0] = "RED a747: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 480; // round 3: three blast passes, 11 sheets, the zoom, hitzone47 and finale47 (r2 ran 124 s with less)
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 460) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            // the n47-r0 run: "blasted NO" with g.forcedCombat's altFire: the press goes through a VIRTUAL PAD's LT instead (FeelReel.N27.Weapons.cs's
            // Pad27: the path a real pad takes through PadReader), the gauntlet put in hand by Equip27 (asked every frame until it is in hand)
            // n47-r1 (blasted NO with the gauntlet in hand, the zoom's ads False, the grapple's RB unfired, the swaps unasked): N24StartCampaign hands
            // the sim an AutopilotInput, whose SampleCombat reads neither the pad nor forcedCombat (only the bot's fire / slot / parry), and whose
            // look is the bot's. The player's own input back for the whole scenario (FeelReel.N24.Cue.cs does the same; the campaign's off restores
            // the input it found): the virtual pad, Equip27's slot and Look now reach the sim.
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput);
            g.forcedMove = null;
            Pad27 pad = NewPad27(offs);
            pad.pad.MakeCurrent(); // PadReader reads Gamepad.current: the reel's pad, not a real one plugged in
            yield return Equip27(g, "gauntlet");
            g.forcedCombat = null;
            string pc = Preconditions(g, g.Feet);
            Start(g, "a747-fx47", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string> { $"pulse47 (sim) {(sim.Pulse47On ? "ON" : "off")}, gun in hand {sim.loadout.Weapon}" };
            yield return Grapple47Frames(g, dir, lines, offs, wallEnd); // G23: the grapple's frames when this level owns it (n46-span); else a line
            pad.pad.MakeCurrent(); // the grapple's pad was the current one
            yield return Equip27(g, "gauntlet");
            g.forcedCombat = null;
            bool[] blasted = new bool[3], drew = new bool[3];
            int variantWas = Fx47Probe.Variant;
            offs.Add(() => Fx47Probe.Variant = variantWas);
            string[] passTag = { "off", "on", "onB" }; // round 3: the blind pair's B (variant 1 "lance") as a third pass
            int[] mag = new int[1];
            float[] wash = new float[3];
            float centre0 = 0, bright0 = 0;
            int washed = 0;
            for (int pass = 0; pass < 3; pass++)
            {
                Fx47Probe.Force = pass >= 1;
                Fx47Probe.Variant = pass == 2 ? 1 : 0;
                // three bodies ahead on the look, standing
                double yaw = g.input != null ? g.input.yaw : 0;
                double fx = -Math.Sin(yaw), fz = -Math.Cos(yaw);
                var husks = new List<Husk>();
                var from = new List<Vec3>();
                foreach (double d in new[] { 3.0, 5.0, 7.0 })
                {
                    var at = new Vec3(g.Feet.x + fx * d, g.Feet.y, g.Feet.z + fz * d);
                    Husk h = sim.husks.Spawn("husk", at, g.Feet);
                    if (h != null) { h.hp = h.maxHp = 1e6; husks.Add(h); from.Add(at); }
                }
                g.husks.frozen = true;
                yield return Frames(10);
                // n47-r2: unfrozen here they walked in over the waits (the nearest at 1.8 m at the press: its own tell halo washed the frame):
                // they stay frozen at 3 / 5 / 7 m until the press's frame, then thaw so the blast can throw them (frozen bodies do not move)
                PulseCharge27 p = sim.pulse27;
                if (p != null) { p.pips = p.p.pips; p.cooldownSteps = 0; }
                yield return Wait27(g, 0.5, wallEnd, () => { God(g); if (p != null) { p.pips = p.p.pips; p.cooldownSteps = 0; } }); // NIGHT30 rules: any action / swap ends first
                // n47-r0c: "gun revolver" at the press: something put the KESTREL back after Equip27 (the campaign's loadout or a swap): equip again
                // right before the press, every pass, and say what was in hand
                string before = sim.loadout.Weapon;
                if (sim.loadout.Weapon != "gauntlet")
                {
                    yield return Equip27(g, "gauntlet");
                    g.forcedCombat = null;
                    yield return Wait27(g, 0.4, wallEnd, () => { God(g); if (p != null) { p.pips = p.p.pips; p.cooldownSteps = 0; } });
                }
                lines.Add($"pass {pass}: in hand before the press '{before}', now '{sim.loadout.Weapon}', owned [{string.Join(",", sim.loadout.owned)}], sim input {(sim.input != null ? sim.input.GetType().Name : "null")}, pad current {Gamepad.current == pad.pad}");
                int blasts0 = p != null ? p.blasts : 0;
                var sb = new System.Text.StringBuilder($"pass {(pass == 2 ? "fx47 ON variant B lance" : pass == 1 ? "fx47 ON variant A ring" : "fx47 off")}:");
                int frame = -2;
                foreach (int k in Sheet47)
                {
                    while (frame < k)
                    {
                        if (frame == 0) { g.husks.frozen = false; pad.pad.MakeCurrent(); pad.Send(1f, 0f); } // LT down (held 8 frames: every sim step of those frames reads it held, L-053)
                        if (frame == 8) pad.Send(0f, 0f);
                        God(g);
                        yield return null;
                        frame++;
                    }
                    yield return Capture47(dir, $"X47-blast-{passTag[pass]}-f{k}", mag, wash);
                    double thrown = 0;
                    for (int i = 0; i < husks.Count; i++) if (husks[i] != null) thrown = Math.Max(thrown, Math.Sqrt((husks[i].position.x - from[i].x) * (husks[i].position.x - from[i].x) + (husks[i].position.z - from[i].z) * (husks[i].position.z - from[i].z)));
                    // n47-r1b: the centre box's mean and the bright share against this pass's f-2 (before the press): the ON pass may not wash
                    if (k == -2) { centre0 = wash[1]; bright0 = wash[2]; }
                    else if (pass >= 1 && (wash[1] > centre0 + UnityTuning.Fx47WashCentreRise || wash[2] > bright0 + UnityTuning.Fx47WashBrightRise)) washed++;
                    sb.Append($" f{k}: kick {Fx47Probe.KickDeg.ToString("0.0", ci)} deg ring {Fx47Probe.RingRadius.ToString("0.0", ci)} m core {Fx47Probe.CoreK.ToString("0.00", ci)} sprites {Fx47Probe.Sprites} thrown {thrown.ToString("0.0", ci)} m centre L {wash[1].ToString("0.00", ci)} bright {(wash[2] * 100).ToString("0", ci)} %{Near47(g, husks, ci)};");
                    if (pass >= 1 && k >= 1 && Fx47Probe.Sprites > 0) drew[pass] = true;
                    if (pass == 0 && Fx47Probe.Sprites > 0) drew[0] = true;
                }
                pad.Send(0f, 0f);
                blasted[pass] = p != null && p.blasts > blasts0;
                sb.Append($" blasted {(blasted[pass] ? "yes" : "NO")} (gun {sim.loadout.Weapon}, pips {(p != null ? p.pips : -1)}, denied {(p != null ? p.denied : -1)} '{(p != null ? p.deniedWhy : "")}', active {(p != null && p.active)})");
                lines.Add(sb.ToString());
                // spent: the next pass stands new ones (n47-r1d: they were left at 0.0001 hp, so the next pass's blast KILLED them at 3-7 m and their
                // blocks rode its frames: the bodies are cleared now; Chunks.cs's near-eye cap is the play fix)
                foreach (Husk h in husks) if (h != null) h.alive = false;
                yield return Wait27(g, 1.0, wallEnd, () => God(g));
            }
            // G24's baseline contact sheets (the meter first, L-001): per gun, RT tapped on the virtual pad, frames -2 .. +30, the camera's pitch per
            // frame against frame -2's (the kick the player sees: CameraFeel + the punch springs + Feel43's kicks), its peak and the frame it settles
            // under 10 % of the peak. fx47 as the build's copy has it (the per-weapon changes are round 3: these are the before numbers).
            if (Time.realtimeSinceStartup < wallEnd)
            {
                var ws = new System.Text.StringBuilder("sheets (pitch kick deg per frame):");
                var ladder = new System.Text.StringBuilder();
                int ladderOut = 0;
                offs.Add(g.events.On("fire", o => sheetFires++));
                offs.Add(g.events.On<MeleeEvent>("meleeSwing", e => { if (e.owner == "player") sheetSwings++; }));
                for (int sheetPass = 0; sheetPass < 2; sheetPass++)
                foreach (string gun in new[] { "revolver", "shotgun", "gauntlet", "torch" }) // the TORCH (G43): RT held through the sheet, released after +30 (the lick)
                {
                    Fx47Probe.Force = sheetPass == 1; // fx47 off then on (G24: before / after)
                    string tag = sheetPass == 1 ? "on" : "off";
                    if (Time.realtimeSinceStartup > wallEnd) break;
                    yield return Equip27(g, gun);
                    g.forcedCombat = null;
                    // round 3 (the r1b sheets: the KESTREL 2.91 deg off vs 1.67 on, the gauntlet's on pass flat 0): a clean sheet, no body to hit or
                    // kill (a kill's shake rode the off pass: the blast passes' bodies are cleared now), the aim 15 deg up into the empty air,
                    // the shots / swings counted (the room's own enemies are left: clearing them would end the room)
                    if (g.input != null) g.input.SetLook(g.input.yaw, 15 * Math.PI / 180);
                    yield return Wait27(g, 0.8, wallEnd, () => God(g));
                    int fires0 = sheetFires, swings0 = sheetSwings;
                    float pitch0 = 0, peak = 0;
                    int peakF = -99, settleF = -99, frame = -2;
                    ws.Append($" {gun} fx47 {tag}:");
                    foreach (int k in Sheet47)
                    {
                        while (frame < k)
                        {
                            if (frame == 0) { pad.pad.MakeCurrent(); pad.Send(0f, 1f); } // RT down 2 frames: a tap
                            if (frame == 2 && gun != "torch") pad.Send(0f, 0f);
                            God(g);
                            yield return null;
                            frame++;
                        }
                        yield return Capture47(dir, $"X47-sheet-{gun}-{tag}-f{k}", mag);
                        float p = Mathf.DeltaAngle(0, g.cam.transform.eulerAngles.x);
                        if (k == -2) pitch0 = p;
                        float kick = Mathf.Abs(Mathf.DeltaAngle(pitch0, p));
                        if (kick > peak) { peak = kick; peakF = k; }
                        if (peakF > -99 && k > peakF && settleF == -99 && kick < 0.1f * peak) settleF = k;
                        ws.Append($" f{k} {kick.ToString("0.00", ci)}");
                    }
                    pad.Send(0f, 0f);
                    if (gun == "torch") { for (int i = 0; i < 4; i++) yield return null; yield return Capture47(dir, $"X47-sheet-torch-{tag}-lick", mag); } // the release's lick
                    ws.Append($" | peak {peak.ToString("0.00", ci)} deg at f{peakF}, settled by f{(settleF == -99 ? "(not by +30)" : settleF.ToString(ci))}, fires {sheetFires - fires0}, swings {sheetSwings - swings0};");
                    if (sheetPass == 1)
                    {
                        // G24's ladder (F-cards 1): the ON peak in its tier's band, settled in the tier's time (+30 frames = 0.5 s at 60 fps)
                        // DECIDE47 D-14 ("no gun gets lighter tonight"): the KESTREL keeps today's kick (its band: today's 1.5-3.5, the medium class's cap)
                        float lo = gun == "shotgun" ? 3f : gun == "gauntlet" || gun == "revolver" ? 1.5f : 0.5f, hi = gun == "shotgun" ? 5f : gun == "gauntlet" ? 2.5f : gun == "revolver" ? 3.5f : 1.0f;
                        int settleMax = gun == "shotgun" ? 30 : gun == "gauntlet" ? 30 : 18; // 0.7 / 0.5 / 0.35 s, the sheet's frames that fit
                        bool inBand = peak >= lo && peak <= hi && settleF != -99 && settleF <= settleMax;
                        ladder.Append($" {gun} {peak.ToString("0.00", ci)} deg [{lo.ToString("0.0", ci)}-{hi.ToString("0.0", ci)}] settled f{(settleF == -99 ? "none" : settleF.ToString(ci))} {(inBand ? "IN" : "OUT")};");
                        if (!inBand) ladderOut++;
                    }
                }
                lines.Add(ws.ToString());
                lines.Add($"G24 ladder {(ladderOut == 0 ? "GREEN" : "RED")} (fx47 on):{ladder}");
            }
            if (Time.realtimeSinceStartup < wallEnd) yield return Sheets47More(g, dir, lines, pad, offs, wallEnd, mag); // G24: the charged shots, the slug, the swap

            // the THUNDERCLAP's zoom (zoom47, G22): LT held on the pad, frames at hip / mid / full, off then on
            bool zoomOk = false;
            if (Time.realtimeSinceStartup < wallEnd)
            {
                bool? zWas = Zoom47.Force;
                offs.Add(() => Zoom47.Force = zWas);
                yield return Equip27(g, "shotgun");
                g.forcedCombat = null;
                var zs = new System.Text.StringBuilder("zoom:");
                for (int pass = 0; pass < 2; pass++)
                {
                    Zoom47.Force = pass == 1;
                    yield return Wait27(g, 0.6, wallEnd, () => God(g));
                    yield return Capture47(dir, $"X47-zoom-{(pass == 1 ? "on" : "off")}-hip", mag);
                    pad.pad.MakeCurrent();
                    pad.Send(1f, 0f);
                    float t0 = Time.unscaledTime;
                    bool midShot = false;
                    while (Time.unscaledTime - t0 < 0.3f)
                    {
                        God(g);
                        yield return null;
                        if (!midShot && Time.unscaledTime - t0 >= 0.075f) { midShot = true; yield return Capture47(dir, $"X47-zoom-{(pass == 1 ? "on" : "off")}-mid", mag); zs.Append($" {(pass == 1 ? "ON" : "off")} mid: fov {g.cam.fieldOfView.ToString("0.0", ci)};"); }
                    }
                    yield return Capture47(dir, $"X47-zoom-{(pass == 1 ? "on" : "off")}-full", mag);
                    zs.Append($" {(pass == 1 ? "ON" : "off")} full: fov {g.cam.fieldOfView.ToString("0.0", ci)}, ads {sim.loadout.adsActive}, zoom47 ease {Zoom47.Ease.ToString("0.00", ci)} yaw {Zoom47.YawDeg.ToString("0.00", ci)} lift {Zoom47.Lift.ToString("0.000", ci)};");
                    if (pass == 1 && Zoom47.Ease > 0.95f) zoomOk = true;
                    pad.Send(0f, 0f);
                }
                lines.Add(zs.ToString());
            }
            if (Time.realtimeSinceStartup < wallEnd) yield return Zone47Frames(g, dir, lines, pad, offs, wallEnd, mag); // R4 hitzone47
            if (Time.realtimeSinceStartup < wallEnd) yield return Finale47Frames(g, dir, lines, offs, mag); // R4 finale47
            if (Time.realtimeSinceStartup < wallEnd) yield return Aura47Frames(g, dir, lines, pad, offs, wallEnd, mag); // G21: the four punch auras
            yield return Stop();
            bool ok = blasted[0] && blasted[1] && blasted[2] && drew[1] && drew[2] && !drew[0] && zoomOk && washed == 0;
            lines.Add(BlindPair47(dir));
            lines.Add($"wash (ON frames whose centre box rose > {UnityTuning.Fx47WashCentreRise.ToString("0.00", ci)} or bright share > {(UnityTuning.Fx47WashBrightRise * 100).ToString("0", ci)} points over f-2): {washed}");
            res[0] = $"{(ok ? "GREEN" : "RED")} a747 fx47 blast | {string.Join(" || ", lines)} | {pc}";
        }
    }
}
