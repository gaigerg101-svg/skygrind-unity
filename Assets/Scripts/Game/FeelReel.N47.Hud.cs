// NIGHT47 builder F: the a647 phase (FeelReel.N47.cs routes it here): the HUD's proof frames (hud47; G16, G18, G19, G5's sizes).
//   reel.sh + -skygrindMap n46-foundry + -skygrindReelOnly a647     (hud47 ON in the build's copy: GREEN frames)
//   the same Player with -skygrindOff47 (or the n46-r3 build): the RED frames (hud47 is read once, when the HUD is built: one pass per Player)
// The CAMPAIGN on the booted map, DOORFREE, enemies frozen, the health held at each share (max 100). Frames (shot-N30-H47-<name>.png):
//   health-100 / -80 / -60 / -40 / -20 / -5   the bar at each share (the line prints Hud47Probe's share and colour per frame)
//   counter-12 / -3 / -1 / 0                  the room counter forced to the count (Counter43Probe.Force; the word and the line)
//   reticle-charge                            the KESTREL's charge held 0.5 s (the arc and the dot's arcs gone; the gun's glow, the rail's bar)
//   pulse-3 / pulse-5 / pulse-cool            the gauntlet in hand: 3 pips, 5 (BLAST ready), the BLAST cooling (the rail's pips and line)
// GREEN when hud47 drew (Hud47Probe.Live), the bar's colour moved with the share (100 % green, 5 % red), the counter's word read LAST ONE at 1
// and CLEAR at 0, and the rail lit the pips it was given.
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
        static partial void N47Hud(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Hud47Scenario(g, dir, res, offs, pre);

        static IEnumerator Hud47Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            AutopilotInput ap = N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            offs.Add(() =>
            {
                Counter43Probe.Force = int.MinValue;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
            });
            if (!sim.campaignOn) { res[0] = "RED a647: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 300;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 280) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            g.husks.frozen = true;
            string pc = Preconditions(g, g.Feet);
            Start(g, "a647-hud47", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string> { $"hud47 drawn {(Hud47Probe.Live ? "yes" : "NO (the row is off in this build: these are the RED frames)")}" };
            int[] mag = new int[1];
            void Hold(double share) { g.health.max = 100; g.health.hp = Math.Max(0.5, share * 100); }

            // the bar
            var sb = new System.Text.StringBuilder("health:");
            Color c100 = default, c5 = default;
            foreach (int s in new[] { 100, 80, 60, 40, 20, 5 })
            {
                for (int i = 0; i < 40; i++) { Hold(s / 100.0); yield return null; } // the ghost drained, the number eased
                yield return Capture47(dir, $"H47-health-{s}", mag);
                Color bc = Hud47Probe.BarColor;
                if (s == 100) c100 = bc;
                if (s == 5) c5 = bc;
                sb.Append($" {s}%: share {Hud47Probe.Frac.ToString("0.00", ci)} colour #{ColorUtility.ToHtmlStringRGB(bc)};");
            }
            lines.Add(sb.ToString());
            Hold(1);

            // overheal (overheal47's pool: frames only when the sim row is on in this build; the bar's second layer, blue -> purple)
            if (sim.Overheal47On && sim.Overheal47Max > 0)
            {
                sb = new System.Text.StringBuilder("overheal:");
                foreach (int o in new[] { 10, 30, 50 })
                {
                    for (int i = 0; i < 40; i++) { Hold(1); g.health.over = Math.Min(o, sim.Overheal47Max); yield return null; }
                    yield return Capture47(dir, $"H47-over-{100 + o}", mag);
                    sb.Append($" {100 + o}: over {g.health.over.ToString("0", ci)};");
                }
                g.health.over = 0;
                lines.Add(sb.ToString());
            }
            else lines.Add("overheal: overheal47 is off in this build: no frames");

            // the counter
            sb = new System.Text.StringBuilder("counter:");
            bool lastOne = false, clear = false;
            foreach (int n in new[] { 12, 3, 1, 0 })
            {
                Counter43Probe.Force = n;
                yield return Frames(30);
                yield return Capture47(dir, $"H47-counter-{n}", mag);
                sb.Append($" {n}: drawn {Counter43Probe.Drawn} word '{Hud47Probe.CounterWord}';");
                if (n == 1 && Hud47Probe.CounterWord == UnityTuning.Hud47LastOneWord) lastOne = true;
                if (n == 0 && Hud47Probe.CounterWord == UnityTuning.Counter43ClearWord) clear = true;
            }
            Counter43Probe.Force = int.MinValue;
            lines.Add(sb.ToString());

            // the reticle with a charge held (the KESTREL)
            int kslot = sim.loadout.owned.IndexOf("revolver");
            if (kslot >= 0)
            {
                g.forcedCombat = new CombatInput { weapon = kslot };
                yield return Frames(2);
                g.forcedCombat = IdleCombat;
                yield return Wait27(g, 0.8, wallEnd, () => Hold(1));
                for (int i = 0; i < 40; i++) { g.forcedCombat = new CombatInput { weapon = -1, fire = new Button { held = true, pressed = i == 0 } }; Hold(1); yield return null; }
                yield return Capture47(dir, "H47-reticle-charge", mag);
                g.forcedCombat = IdleCombat;
                lines.Add($"reticle: the KESTREL's charge held 40 frames (Hud33ChargeNow {Ui.Hud33ChargeNow.ToString("0.00", ci)}; the arc shown {Hud38.BarShown})");
                yield return Wait27(g, 1.5, wallEnd, () => Hold(1));
            }
            else lines.Add("reticle: the KESTREL is not owned here: not taken");

            // the PULSE's reads on the rail
            if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
            int gslot = sim.loadout.owned.IndexOf("gauntlet");
            g.forcedCombat = new CombatInput { weapon = gslot };
            yield return Frames(2);
            g.forcedCombat = IdleCombat;
            yield return Wait27(g, 1.0, wallEnd, () => Hold(1));
            PulseCharge27 p = sim.pulse27;
            sb = new System.Text.StringBuilder("pulse:");
            int litOk = 0;
            if (p != null)
            {
                foreach ((string name, int pips, int cool) in new[] { ("pulse-3", 3, 0), ("pulse-5", 5, 0), ("pulse-cool", 0, p.BlastCooldownSteps / 2) })
                {
                    for (int i = 0; i < 20; i++) { p.pips = pips; p.cooldownSteps = cool; Hold(1); yield return null; }
                    yield return Capture47(dir, $"H47-{name}", mag);
                    sb.Append($" {name}: pips {pips} lit {Hud47Probe.PipsLit} cooldown share {p.CooldownShare.ToString("0.00", ci)};");
                    if (Hud47Probe.PipsLit == pips) litOk++;
                }
            }
            else sb.Append(" no glove (pulse27 off)");
            lines.Add(sb.ToString());

            // the boss text at once (the lead's G5 check): a forced boss bar with a hint (the fight line), a teach card and a banner together
            bool forceWas = Bar43Probe.ForceOn;
            Boss43State stateWas = Bar43Probe.ForceState;
            offs.Add(() => { Bar43Probe.ForceOn = forceWas; Bar43Probe.ForceState = stateWas; });
            Bar43Probe.ForceOn = true;
            Bar43Probe.ForceState = new Boss43State { valid = true, name = "THE LOOM", hp01 = 0.62, leg = 1, legs = 3, stagger01 = 0.4, hint = "{punch} THE HEAD TO STAGGER IT" }; // a glyph in it (n47-r1e: the raw markup's ghost showed on hints with a button)
            Hud47Probe.ForceBanner = "FALLING STONES";
            Hud47Probe.ForceTeach = "{grapple} a lit knee, then pull away.";
            for (int i = 0; i < 45; i++) { Hold(1); yield return null; }
            yield return Capture47(dir, "H47-bosstext", mag);
            lines.Add($"boss text (one line at a time: the hint outranks the banner and the card): bar hint '{Bar43Probe.Hint}', fight line now '{Hud47Probe.FightLine}' (lines {Hud47Probe.FightLines}; drawn by {Hud47Probe.FightObjects} text object(s), raw tag {(Hud47Probe.FightRawTag ? "YES" : "no")}), the card's text '{Teach43Probe.Head}' / '{Teach43Probe.Sub}' (its own slot hidden while the bar is up)");
            for (int i = 0; i < 260; i++) { Hold(1); yield return null; } // the hint's hold ends: the queued banner / card follow, one at a time
            yield return Capture47(dir, "H47-bosstext-next", mag);
            lines.Add($"boss text after the hint's hold: fight line '{Hud47Probe.FightLine}' (lines {Hud47Probe.FightLines})");
            Hud47Probe.ForceTeachOff = true;
            Bar43Probe.ForceOn = forceWas;
            yield return Frames(5);
            yield return Stop();
            bool colours = Hud47Math.HueDeg(c100) > 120f && Hud47Math.HueDeg(c5) < 45f;
            bool ok = Hud47Probe.Live && colours && lastOne && clear && litOk == 3;
            res[0] = $"{(ok ? "GREEN" : "RED")} a647 hud47 | bar hue 100 % {Hud47Math.HueDeg(c100).ToString("0", ci)} / 5 % {Hud47Math.HueDeg(c5).ToString("0", ci)}, LAST ONE {lastOne}, CLEAR {clear}, pips lit {litOk}/3 || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
