// NIGHT55 S (P0-t125; OPEN O-53, THREADS T-125): a152's THUNDERCLAP taps that never fired in the Player reel.
// THE CAUSE (code read): a152 (FeelReel.N52.Hud.cs) starts its run with N24StartCampaign, which hands the sim an AutopilotInput
// (g.sim.UseInput(ap), FeelReel.N24.Room.cs:44: the campaign bot walks the player into the first room). From then on the sim samples
// the bot, not the player's ReaderInput, and g.forcedCombat is ReaderInput's override (ReaderInput.SampleCombat): every combat input
// a152 sets is ignored. So its Equip27 left the THUNDERCLAP in hand (n52-freeze, EXT53's print) and its six taps fired nothing
// (n52-r20). a252 (FeelReel.N52.Skin.cs:132) hands the sim the player's input back after the same start ("no bot walking him"),
// which is why the KESTREL's presses fire there. EXT53's headless tap53 drove the sim's input directly, so it fired.
// THE FIX: N55T125InputBack, the same hand-back, called in a152 right after its campaign start (one line in FeelReel.N52.Hud.cs);
// N55T125Taps (one line in a152 after its THUNDERCLAP frames) taps the THUNDERCLAP six times, each held a fixed count of sim steps
// (not frames: a fast clock cannot drop a tap) and spaced past the gun's own cycle, and counts the "fire" events.
// a555 (this file): the proof alone, RED then GREEN in one Player: the six taps under the campaign bot's input (a152 before NIGHT55),
// then the hand-back and the six taps again. Run on any campaign level:
//   bash Tools/night/n47-reels.sh <tag> s1,n48-foundry,a555
// GREEN when the second pass hears >= UnityTuning.T125WantFires THUNDERCLAP fire events (the first pass's count is printed: the RED).
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
        static string t125Note = "";

        /// <summary>T-125's fix: the sim reads the player's ReaderInput (the reel's g.forcedCombat) again, as a252 does; the note names what it read.</summary>
        static string N55T125InputBack(SurvivalGame g, SurvivalSim sim)
        {
            string was = sim.input == null ? "null" : sim.input.GetType().Name;
            bool bot = sim.input is AutopilotInput;
            if (bot) sim.UseInput(g.playerInput);
            t125Note = $"T-125 input: the sim read {was}{(bot ? " (N24StartCampaign's campaign bot: g.forcedCombat, the player's ReaderInput override, was never sampled)" : "")} -> {(sim.input == null ? "null" : sim.input.GetType().Name)}"
                + $"{(ReferenceEquals(sim.input, g.playerInput) ? " (the reel's forcedCombat reader, handed back as a252 does: FeelReel.N52.Skin.cs:132)" : " (NOT the reel's)")}";
            return t125Note;
        }

        /// <summary>
        /// Six THUNDERCLAP taps (UnityTuning.T125Taps), each held T125TapFrames reel frames counted in sim steps, the next one past the gun's
        /// cycle (fireInterval or the loadout's last cycle, the larger) + T125TapMarginSteps; the look up (T125TapPitchRad) so no pellet hits a
        /// frozen husk. fires[0] = the THUNDERCLAP's "fire" events heard. One line to `lines`.
        /// </summary>
        static IEnumerator N55T125Taps(SurvivalGame g, SurvivalSim sim, List<string> lines, string label, Action hold, int[] fires)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            int all = 0, tc = 0;
            Action<object> onFire = o =>
            {
                all++;
                if (o is string gun && gun == "shotgun") tc++;
            };
            g.events.On("fire", onFire);
            string inHand = sim.loadout.Weapon;
            double yaw0 = g.input.yaw, pitch0 = g.input.pitch;
            int tapSteps = Math.Max(1, (int)Math.Round(UnityTuning.T125TapFrames / (double)UnityTuning.ReelFps / sim.step));
            int cycleSteps = MovementMath.StepsFor(g.cfg.Num("weapons.shotgun.fireInterval"), sim.step);
            Charge35 q = sim.charge35;
            int qTaps0 = q != null ? q.taps : 0, qShots0 = q != null ? q.tapShots : 0;
            int step0 = sim.simStep, taps = 0, gapMax = 0;
            float wallEnd = Time.realtimeSinceStartup + UnityTuning.T125WallCapS;
            for (int t = 0; t < UnityTuning.T125Taps && Time.realtimeSinceStartup < wallEnd; t++)
            {
                int gap = Math.Max(cycleSteps, sim.loadout.LockoutWindowSteps29) + UnityTuning.T125TapMarginSteps;
                gapMax = Math.Max(gapMax, gap);
                int s0 = sim.simStep;
                taps++;
                while (sim.simStep - s0 < gap && Time.realtimeSinceStartup < wallEnd)
                {
                    int held = sim.simStep - s0;
                    g.forcedCombat = held < tapSteps ? new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = held == 0 } } : IdleCombat;
                    g.input.SetLook(yaw0, UnityTuning.T125TapPitchRad);
                    hold?.Invoke();
                    yield return null;
                }
            }
            g.forcedCombat = IdleCombat;
            g.input.SetLook(yaw0, pitch0);
            g.events.Off("fire", onFire);
            if (fires != null && fires.Length > 0) fires[0] = tc;
            string input = sim.input == null ? "null" : sim.input.GetType().Name + (ReferenceEquals(sim.input, g.playerInput) ? " (the reel's)" : " (NOT the reel's)");
            string note = t125Note.Length > 0 ? t125Note + "; " : ""; // the hand-back's note (the fix named), once
            t125Note = "";
            lines.Add($"{note}T-125 taps, {label}: the sim's input {input}; in hand {inHand} -> {sim.loadout.Weapon}; {taps} taps of {tapSteps} sim steps, up to {gapMax} steps apart (the THUNDERCLAP's fireInterval {cycleSteps} steps), "
                + $"{sim.simStep - step0} sim steps in all; fire events {all} (THUNDERCLAP {tc}; want >= {UnityTuning.T125WantFires}: {(tc >= UnityTuning.T125WantFires ? "met" : "NOT met")}); "
                + $"charge35 {(q == null ? "null" : $"taps +{q.taps - qTaps0}, tap shots +{q.tapShots - qShots0}")}; active {sim.Active}, flow {sim.flow}");
        }

        static partial void N55Smoke(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = T125Scenario(g, dir, res, offs, pre);

        static IEnumerator T125Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            offs.Add(() =>
            {
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
            });
            if (!sim.campaignOn) { res[0] = "RED a555: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 300;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 280) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            g.husks.frozen = true;
            string pc = Preconditions(g, g.Feet);
            Start(g, "a555-t125", "camera", dir);
            yield return Frames(pre);
            void Hold() => g.health.hp = g.health.max;
            var lines = new List<string>();
            t125Note = "";
            if (g.loadout.owned.IndexOf("shotgun") < 0) g.loadout.AddWeapon("shotgun"); // a just-added gun goes in hand (Loadout.AddWeapon's pendingSlot), whoever's input the sim reads
            for (int i = 0; i < 90; i++) { Hold(); yield return null; } // the raise
            // 1. RED: a152's path before NIGHT55 (the sim reads the campaign bot)
            int[] before = { -1 }, after = { -1 };
            yield return N55T125Taps(g, sim, lines, "before (a152's input path until NIGHT55)", Hold, before);
            // 2. the fix, then the THUNDERCLAP in hand by the reel's own input, then GREEN
            N55T125InputBack(g, sim); // its note leads the next taps line
            yield return Equip27(g, "shotgun");
            g.forcedCombat = IdleCombat;
            yield return N55T125Taps(g, sim, lines, "after (the reel's input handed back)", Hold, after);
            yield return Frames(5);
            yield return Stop();
            bool ok = after[0] >= UnityTuning.T125WantFires;
            res[0] = $"{(ok ? "GREEN" : "RED")} a555 t125 | THUNDERCLAP fire events of {UnityTuning.T125Taps} taps: before {before[0]}, after {after[0]} (want >= {UnityTuning.T125WantFires}) || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
