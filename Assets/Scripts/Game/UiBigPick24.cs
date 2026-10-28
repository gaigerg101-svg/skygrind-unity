// NIGHT24 (builder F, contract big-pick; design.n24.bigPick): the BIG PICK on the MODS draft screen (UiMods.cs). UNCOMPILED and UNRUN when written
// (no Editor in the builder's worktree). The screen is the draft's own (same three cards, same entrance, same focus frame); with a BIG PICK open
// (Mods.BigOffer) UiMods.RenderDraft hands the cards to RenderBigPick24 instead of drawing mods:
//   head   "BIG PICK  ·  ROUND 5"; three cards: a glyph, the name (S32), the hero (S56: "+1000", "SPIN", ...), one finished line (S24); no REROLL, no
//          STACK line, no RARE tag; the line under the cards counts down the default pick (S24: "PICKS CARD 1 IN 6 s")
//   input  keys 1-3 and a click take a card (SurvivalGame.Debug.cs / the card buttons, unchanged); pad A takes the card the focus is on, and with
//          nothing selected yet (the draft opens mid-break, so a jump's A must not take one) card 1 once 0.4 s have passed (BigPick24AcceptDelay)
//   time   the default card is taken after design.n24.bigPick.defaultSeconds of the screen's REAL time (the game is paused under it)
// Text is the theme's S24 / S32 / S56 (18 px at 1080p is the floor). Never red; points are the one gold thing. Static reads for the reel `bigpick`.
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool bigStyled;
        float bigOpenedAt;
        int bigTimerShown = -1;

        /// <summary>What the BIG PICK screen shows, for the reel's scenario `bigpick` (the Ui instance is private to SurvivalGame).</summary>
        public static bool BigPick24Open;
        public static string BigPick24HeadNow = "";
        public static readonly string[] BigPick24NameNow = new string[3], BigPick24HeroNow = new string[3], BigPick24LineNow = new string[3];
        public static int BigPick24CardsNow;
        public static float BigPick24MinFontPx;
        public static int BigPick24SecondsLeftNow;
        public static bool BigPick24RerollShown;
        public static string BigPick24HintNow = ""; // the first-showing line ("" once learned)

        /// <summary>The pad's confirm button as the live glyph set names it (the south button).</summary>
        static string PadConfirmWord24() => PlayerDevices.Last == InputDevice.Pad && PlayerDevices.Glyphs == GlyphSet.PlayStation ? "CROSS" : PlayerDevices.Last == InputDevice.Pad && PlayerDevices.Glyphs == GlyphSet.Nintendo ? "B" : "A";

        /// <summary>The pad A guard: a press this soon after the screen opened is a jump's, not a pick (s).</summary>

        void RenderBigPick24(Mods mods)
        {
            BigPick24 big = mods.big;
            if (mods.version != shownDraft)
            {
                shownDraft = mods.version;
                bigStyled = true;
                bigOpenedAt = Time.unscaledTime;
                bigTimerShown = -1;
                draftHead.SetText(BigPick24.Head(mods.offerRound)); // NIGHT25: the head names the round of THIS pick (Core/BigPick24.Head)
                rerollLabel.transform.parent.gameObject.SetActive(false); // no reroll
                bigTimer.gameObject.SetActive(true);
                // the first-showing prompt (the first-fifteen rule's "bigpick" moment, shown once per profile, gone on the first pick); the game is
                // paused under the draft, so the line is drawn here, not on the lesson card
                FirstFifteen.Moment hint = firstFifteen != null ? firstFifteen.rule.Active : null;
                bool showHint = hint != null && hint.id == FirstFifteenWatch.BigPick;
                bigHint.gameObject.SetActive(showHint);
                BigPick24HintNow = "";
                if (showHint)
                {
                    bigHint.text = FillGlyphs("PRESS 1 2 3 OR " + PadConfirmWord24() + ": " + hint.line); // the moment is the short line (at most 3 words); the keys are the screen's
                    BigPick24HintNow = bigHint.text;
                }
                float minPx = float.MaxValue;
                for (int i = 0; i < cards.Count; i++)
                {
                    bool on = i < mods.offer.Count;
                    cards[i].go.SetActive(on);
                    BigPick24NameNow[i] = BigPick24HeroNow[i] = BigPick24LineNow[i] = "";
                    if (!on) continue;
                    string id = mods.offer[i];
                    bool points = BigPick24.KindOf(id) == BigPick24.Points;
                    cards[i].name.text = big.Name(id, game.cfg);
                    cards[i].tag.text = "";
                    cards[i].bar.color = points ? UiTheme.Gold : UiTheme.CyanHot;
                    cards[i].glyph.Set(BigPick24.Glyph(id), points ? UiTheme.Gold : UiTheme.CyanHot);
                    cards[i].hero.text = big.Hero(id, mods.offerRound);
                    cards[i].hero.color = points ? UiTheme.Gold : UiTheme.White; // gold: points only
                    cards[i].desc.text = big.Line(id, game.cfg);
                    cards[i].stack.text = "";
                    BigPick24NameNow[i] = cards[i].name.text;
                    BigPick24HeroNow[i] = cards[i].hero.text;
                    BigPick24LineNow[i] = cards[i].desc.text;
                    minPx = Mathf.Min(minPx, cards[i].name.fontSize, cards[i].hero.fontSize, cards[i].desc.fontSize);
                }
                minPx = Mathf.Min(minPx, bigTimer.fontSize, draftHead.fontSize);
                BigPick24MinFontPx = minPx;
                BigPick24CardsNow = mods.offer.Count;
                BigPick24HeadNow = draftHead.text;
            }
            BigPick24Open = true;
            BigPick24RerollShown = rerollLabel.transform.parent.gameObject.activeSelf;

            float elapsed = Time.unscaledTime - bigOpenedAt;
            int left = Mathf.Max(0, Mathf.CeilToInt((float)big.rules.defaultSeconds - elapsed));
            BigPick24SecondsLeftNow = left;
            if (left != bigTimerShown)
            {
                bigTimerShown = left;
                bigTimer.SetText(UnityTuning.BigPick24Timer, (float)left);
            }
            // pad A with nothing selected yet takes card 1 (once the open's own press is past); with a card selected, UiNav submits that card
            if (navDraftWaits && PlayerDevices.Menu.confirm && elapsed > UnityTuning.BigPick24AcceptDelay) game.PickMod(big.rules.defaultCard);
            else if (elapsed >= (float)big.rules.defaultSeconds) game.PickMod(big.rules.defaultCard); // the default pick
        }

        /// <summary>The old draft after a BIG PICK: REROLL and the timer back to their places.</summary>
        void EndBigPick24Style()
        {
            bigStyled = false;
            BigPick24Open = false;
            rerollLabel.transform.parent.gameObject.SetActive(true);
            bigTimer.gameObject.SetActive(false);
            bigHint.gameObject.SetActive(false);
            for (int i = 0; i < cards.Count; i++) cards[i].hero.color = UiTheme.White;
        }
    }
}
