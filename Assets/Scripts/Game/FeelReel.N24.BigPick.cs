// NIGHT24 (builder F): the reel scenario `bigpick`, the contract big-pick's Player half. UNCOMPILED and UNRUN when written (no Editor in the
// builder's worktree); the rule ran headless as `SimHarness fist --bigpick` (PASS; `--off` is the RED run).
// The story, in SURVIVAL:
//   early    the clears of rounds 1-4 (roundClear events, labelled: the reel does not play four rounds) open no draft
//   round 5  the clear of round 5 opens the BIG PICK: the screen is the draft's, the head reads BIG PICK, three cards, no REROLL, the text sizes
//            (the smallest font in px, 18 or more), the cards' names and heroes printed
//   press    one press (PickMod, the keys' and A's call) takes card 2 and the screen is gone the next frame
//   round 10 the clear of round 10 opens a second pick; nobody presses: the default card (card 1) is taken after the default seconds of real time
//            (printed with the measured seconds)
// If a box card is taken, the free spin runs on the Player: boxRoll within the reveal is counted and printed.
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g). RED: the row off (the old draft), the screen not drawn as a BIG
// PICK (Ui.BigPick24Open), or any check failing.
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
        static partial void N24BigPickBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = BigPickScenario(g, dir, res, offs, pre);
        }

        static IEnumerator BigPickScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            var fails = new List<string>();
            if (!g.sim.n24.bigPick || g.mods.big == null || !g.mods.big.Active)
            {
                res[0] = pc + " | RED: design.n24.bigPick is off (the old percentage draft)";
                yield break;
            }
            BigPick24 big = g.mods.big;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(20);
            g.husks.frozen = true;
            var picked = new List<string>();
            int boxRolls = 0;
            offs.Add(g.events.On<ModEvent>("modPick", e => { picked.Add(e.id); FeelProbe.Event("bigPickTake"); }));
            offs.Add(g.events.On("boxRoll", _ => boxRolls++));

            // --- early: no draft before round 5 -------------------------------------------------------------------------------------------
            for (int r = 1; r <= 4; r++) g.events.Emit("roundClear", new RoundEvent { number = r });
            bool earlyPending = g.mods.Pending;
            if (earlyPending) fails.Add("a draft was waiting after the clears of rounds 1-4");

            Start(g, "bigpick", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // --- round 5 ------------------------------------------------------------------------------------------------------------------
            HoldBreak(g);
            g.events.Emit("roundClear", new RoundEvent { number = 5 });
            int n = 0;
            while (!g.DraftOpen && n++ < 300)
            {
                HoldBreak(g);
                yield return null;
            }
            if (!g.DraftOpen)
            {
                yield return Stop();
                res[0] = pc + " | " + (fails.Count == 0 ? "" : string.Join("; ", fails) + "; ") + "FAIL (RED): the clear of round 5 opened no BIG PICK in 5 s";
                yield break;
            }
            yield return Frames(Ms(800));
            if (!Ui.BigPick24Open) fails.Add("the open draft is not drawn as a BIG PICK");
            // NIGHT25 (builder F): the head is read HERE, when the round-5 pick is open (N24 printed it after round 10 had opened: "ROUND 10")
            string head5 = Ui.BigPick24HeadNow;
            if (head5 != BigPick24.Head(5)) fails.Add($"the round-5 pick's head reads '{head5}', not '{BigPick24.Head(5)}'");
            if (Ui.BigPick24CardsNow != 3) fails.Add($"{Ui.BigPick24CardsNow} cards, not 3");
            if (Ui.BigPick24RerollShown) fails.Add("a REROLL is on the BIG PICK");
            if (Ui.BigPick24MinFontPx < 18f) fails.Add($"the smallest text is {Ui.BigPick24MinFontPx.ToString("0", ci)} px, under 18");
            string cards5 = string.Join(" | ", Ui.BigPick24NameNow[0] + " " + Ui.BigPick24HeroNow[0], Ui.BigPick24NameNow[1] + " " + Ui.BigPick24HeroNow[1], Ui.BigPick24NameNow[2] + " " + Ui.BigPick24HeroNow[2]);

            // --- one press -------------------------------------------------------------------------------------------------------------
            FeelProbe.Event("bigPickPress");
            bool took = g.PickMod(1);
            yield return null;
            yield return null;
            if (!took || g.DraftOpen) fails.Add("one press (PickMod 1) did not take the card and close the screen");
            string pressPick = picked.Count > 0 ? picked[picked.Count - 1] : "none";
            if (picked.Count != 1) fails.Add($"{picked.Count} picks after one press");
            if (BigPick24.KindOf(pressPick) == BigPick24.Box)
            {
                int w = 0;
                while (boxRolls == 0 && w++ < Ms(12000)) yield return null;
                if (boxRolls != 1) fails.Add($"the box card gave {boxRolls} boxRoll events");
            }
            yield return Frames(Ms(500));

            // --- round 10: nobody presses; the default card after the default seconds ----------------------------------------------------------
            HoldBreak(g);
            g.events.Emit("roundClear", new RoundEvent { number = 10 });
            n = 0;
            while (!g.DraftOpen && n++ < 300)
            {
                HoldBreak(g);
                yield return null;
            }
            float t0 = Time.realtimeSinceStartup;
            yield return Frames(Ms(300));
            string head10 = Ui.BigPick24HeadNow; // NIGHT25: its own head, read when its own pick is open
            if (head10 != BigPick24.Head(10)) fails.Add($"the round-10 pick's head reads '{head10}', not '{BigPick24.Head(10)}'");
            int before = picked.Count;
            string offer10 = g.mods.offer != null && g.mods.offer.Count > 0 ? g.mods.offer[big.rules.defaultCard] : "?";
            int guard = 0;
            while (g.DraftOpen && guard++ < Ms(12000)) yield return null;
            float secs = Time.realtimeSinceStartup - t0;
            string defPick = picked.Count > before ? picked[picked.Count - 1] : "none";
            if (g.DraftOpen) fails.Add("the BIG PICK did not close on its own");
            else if (Math.Abs(secs - big.rules.defaultSeconds) > 1.0) fails.Add($"the default pick came after {secs.ToString("0.0", ci)} s, not {big.rules.defaultSeconds.ToString("0", ci)}");
            if (defPick != offer10) fails.Add($"the default pick took '{defPick}', not card {big.rules.defaultCard + 1} '{offer10}'");
            yield return Frames(Ms(400));
            yield return Stop();

            res[0] = pc + $" | {FeelProbe.FrameCount} frames; rounds 1-4 (roundClear events, labelled) drafted {(earlyPending ? "YES" : "nothing")}; round 5: head '{head5}', {Ui.BigPick24CardsNow} cards [{cards5}], reroll shown {Ui.BigPick24RerollShown}, smallest text {Ui.BigPick24MinFontPx.ToString("0", ci)} px; one press took '{pressPick}' (boxRoll {boxRolls}); round 10: default '{defPick}' after {secs.ToString("0.0", ci)} s of {big.rules.defaultSeconds.ToString("0", ci)} | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
