// NIGHT20 U3-34.9 F3 (builder H): the reward reveal (design.adds.reveal; Research/2026-10-06-additions-backlog.md F3:
// Vampire Survivors' chest as a slot machine, Hades' 1 of 3, CoD's box reveal: "a 0.8 s three-slot reveal, pick 1 of 3 (a
// power-up, a MODS pick, points)").
// - A hunt's, an elite's or a boss's end (`specialRound` phase "end": E's special rounds; Reveal20.Triggers) earns a reveal;
//   it opens in the next break once the round-clear beat has played and any MODS draft is taken, pausing the game as the
//   draft does.
// - Three slots turn over in turn (Reveal20.SlotIn: 0.2 s apart, whole by 0.8 s): a power-up (its name), a MODS pick, the
//   round's points (GOLD: points); 1 / 2 / 3, a click or the pad's A on a slot takes one once the reveal has ended (a jump's
//   A never takes one mid-reveal); B / ESC skips (the game resumes with nothing).
// - The offers: Reveal20.Offers from the run's seed and the round (their own Mulberry32: the run's streams are untouched),
//   design.adds.reveal's points and salt. The pick: RevealApply20 (a power-up dropped at the player's feet, a MODS draft
//   queued for this break, the points granted) and the bus events `rewardReveal` / `rewardPick` for GG's sounds.
// THE TAPE: a pick changes the run while paused, as a MODS pick does; the lead's tape records it (RevealApply20.Apply is the
// one call a replay makes with the slot's offer). Built once; per frame only the slots' turn-over is written while it runs.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>F3's bus payload (`rewardReveal` when it opens, `rewardPick` on a pick: slot -1 is a skip).</summary>
    public sealed class RewardRevealEvent
    {
        public int round;
        public string source;
        public int slot = -1, kind = -1, points;
        public string type;
    }

    /// <summary>F3: what a taken offer does to the run (the UI's pick and a tape's replay make the same call).</summary>
    public static class RevealApply20
    {
        public static void Apply(SurvivalSim sim, Vec3 feet, RevealOffer o, int round)
        {
            if (sim == null) return;
            if (o.kind == Reveal20.PowerUp && o.type != null) sim.powerUps.Spawn(feet, o.type);
            else if (o.kind == Reveal20.Mod) sim.mods.Queue(round);
            else if (o.kind == Reveal20.Points && o.points > 0) sim.economy.Grant(o.points, "reveal");
        }
    }

    public sealed partial class Ui
    {
        GameObject reveal20;
        UiShape[] revealCards20;
        UiShape[] revealBars20;
        TextMeshProUGUI[] revealTags20, revealHeroes20, revealKeys20;
        UiShape[] revealKeyChips20; // row 4 (builder HV), banned 10: the slots' keys on drawn chips
        TextMeshProUGUI revealSkip20, revealSkipKey20;
        UiShape revealSkipChip20;
        readonly RevealOffer[] revealOffers20 = new RevealOffer[3];
        readonly RewardRevealEvent revealEvent20 = new RewardRevealEvent();
        bool revealCfg20, revealModsOn20, revealOpen20;
        double revealBase20, revealPerRound20, revealSalt20, revealAge20;
        int revealPendingRound20 = -1, revealRound20;
        string revealSource20;
        string[] revealTypes20;

        /// <summary>Ui.cs: the reveal is up (the PAUSED screen stays hidden under it).</summary>
        bool Reveal20Open => revealOpen20;

        void BuildReveal20()
        {
            revealCfg20 = cfg.Design("adds.reveal.enabled");
            revealBase20 = cfg.Has("design.adds.reveal.pointsBase") ? cfg.Num("design.adds.reveal.pointsBase") : 0;
            revealPerRound20 = cfg.Has("design.adds.reveal.pointsPerRound") ? cfg.Num("design.adds.reveal.pointsPerRound") : 0;
            revealSalt20 = cfg.Has("design.adds.reveal.seedSalt") ? cfg.Num("design.adds.reveal.seedSalt") : 0;
            revealModsOn20 = cfg.Has("mods.enabled") && cfg.Bool("mods.enabled");
            revealTypes20 = PowerUps.Types;

            reveal20 = Screen("Reward reveal", UiTheme.A(UiTheme.InkSolid, UnityTuning.MenuSheetAlpha));
            float h1 = UiTheme.Px(UiTheme.Size.S56);
            TextMeshProUGUI head = Text(reveal20.transform, "Head", "PICK ONE", h1, UiTheme.White, TextAlignmentOptions.Center, new Vector2(0, 260), new Vector2(1200, 80), new Vector2(0.5f, 0.5f));
            UiTheme.Style(head, h1, UiTheme.White, UiTheme.Display, true);
            float w = UnityTuning.Hud20RevealCardW, h = UnityTuning.Hud20RevealCardH;
            revealCards20 = new UiShape[3];
            revealBars20 = new UiShape[3];
            revealTags20 = new TextMeshProUGUI[3];
            revealHeroes20 = new TextMeshProUGUI[3];
            revealKeys20 = new TextMeshProUGUI[3];
            revealKeyChips20 = new UiShape[3];
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                UiShape card = Hud17Shape(reveal20.transform, $"Slot {i}", UiTheme.A(UiTheme.InkSoft, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * (w + UnityTuning.Hud20RevealCardGap), 0), new Vector2(w, h));
                card.Slant = UnityTuning.ThemeSlantDeg * 0.5f; // a tall card leans less (the MODS cards' rule)
                card.raycastTarget = true;
                var b = card.gameObject.AddComponent<UnityEngine.UI.Button>();
                b.transition = UnityEngine.UI.Selectable.Transition.None; // UiNav's focus frame shows it
                b.onClick.AddListener(() => Reveal20Take(idx));
                RectTransform rt = card.rectTransform;
                revealBars20[i] = Hud17Shape(rt, "Bar", UiTheme.Cyan, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -4), new Vector2(w - 8, 12));
                revealBars20[i].SetCuts(0, 0);
                // row 4 (builder HV), banned 10: the slot's key on a drawn key chip (the menus' KeyChip), INK-SOLID on the INK-SOFT card
                UiShape chip = KeyChip(rt, Vector2.zero, out revealKeys20[i]);
                chip.rectTransform.anchorMin = chip.rectTransform.anchorMax = new Vector2(0, 1);
                chip.rectTransform.anchoredPosition = new Vector2(UnityTuning.Hud20RevealKeyX, -UnityTuning.Hud20RevealKeyY);
                chip.color = UiTheme.InkSolid;
                KeyChipSet(chip, revealKeys20[i], RevealPrompt20.SlotKey(i));
                revealKeyChips20[i] = chip;
                revealTags20[i] = Hud17Text(rt, "Tag", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.CyanHot, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(w - 30, 28), false);
                revealTags20[i].characterSpacing = UnityTuning.ThemeLabelTracking;
                revealHeroes20[i] = Hud17Text(rt, "Hero", "", UiTheme.Px(UiTheme.Size.S42), UiTheme.White, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(w - 30, 60), true);
                revealHeroes20[i].enableAutoSizing = true;
                revealHeroes20[i].fontSizeMin = UiTheme.Px(UiTheme.Size.S24);
                revealHeroes20[i].fontSizeMax = UiTheme.Px(UiTheme.Size.S42);
                revealCards20[i] = card;
            }
            // row 4 (builder HV), banned 10: the skip as DOWN's retry is: a drawn key chip beside its verb (the pad: its glyph in the verb)
            revealSkip20 = Hud17Text(reveal20.transform, "Skip", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), new Vector2(0, -h * 0.5f - 50), new Vector2(800, 40), true);
            revealSkipChip20 = KeyChip(reveal20.transform, new Vector2(0, -h * 0.5f - 50), out revealSkipKey20);
            revealSkip20.richText = true;
            revealSkip20.characterSpacing = UnityTuning.ThemeLabelTracking;

            game.events.On<SpecialRoundEvent>(N20Events.SpecialRound, e =>
            {
                if (e == null || !revealCfg20 || game.sim == null || !game.sim.n20.adds || !Reveal20.Triggers(e.name, e.phase)) return;
                revealPendingRound20 = e.round > 0 ? e.round : game.run.round;
                revealSource20 = e.name;
            });
            game.OnNewRun += () =>
            {
                revealPendingRound20 = -1;
                if (revealOpen20) Reveal20Close();
            };
        }

        void RenderReveal20(double frameDt)
        {
            if (reveal20 == null) return;
            if (!revealOpen20)
            {
                if (revealPendingRound20 < 0 || game.flow != Flow.Playing || game.run.state != SurvivalRun.State.Break || game.DraftOpen || (game.mods != null && game.mods.Pending)) return;
                if (game.sim.hitstopLeft > 0 || (game.ImpactDirector != null && game.ImpactDirector.rules.SlowMoActive)) return; // after the clear's beat
                Reveal20Begin();
                return;
            }
            if (game.flow != Flow.Paused)
            {
                Reveal20Skip(); // ESC / B resumed the game: a skip
                return;
            }
            revealAge20 += frameDt;
            bool still = game.settings.View.reducedMotion;
            for (int i = 0; i < 3; i++)
            {
                float k = Reveal20.SlotIn(revealAge20, i);
                RectTransform rt = revealCards20[i].rectTransform;
                float sx = still ? 1f : UnityTuning.Hud20RevealFlip + (1f - UnityTuning.Hud20RevealFlip) * k;
                if (rt.localScale.x != sx) rt.localScale = new Vector3(sx, 1, 1);
                SetRevealAlpha20(i, k);
            }
            if (!Reveal20.Ready(revealAge20)) return;
            UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;
            if (kb.digit1Key.wasPressedThisFrame) Reveal20Take(0);
            else if (kb.digit2Key.wasPressedThisFrame) Reveal20Take(1);
            else if (kb.digit3Key.wasPressedThisFrame) Reveal20Take(2);
        }

        void SetRevealAlpha20(int i, float a)
        {
            revealCards20[i].color = UiTheme.A(UiTheme.InkSoft, a);
            revealBars20[i].color = UiTheme.A(revealOffers20[i].kind == Reveal20.Points ? UiTheme.Gold : UiTheme.Cyan, a);
            revealTags20[i].alpha = a;
            revealHeroes20[i].alpha = a;
            revealKeys20[i].alpha = a;
            revealKeyChips20[i].color = UiTheme.A(UiTheme.InkSolid, a);
        }

        /// <summary>The skip row: the chip (the mouse and keyboard) and the verb, centred as one (DOWN's retry's layout).</summary>
        void Reveal20SkipRow(bool pad)
        {
            string key = RevealPrompt20.SkipChip(pad), verb = RevealPrompt20.SkipVerb(pad, PlayerDevices.Glyphs);
            KeyChipSet(revealSkipChip20, revealSkipKey20, key);
            revealSkip20.SetText(verb);
            float chipW = key != null ? revealSkipChip20.rectTransform.sizeDelta.x + UnityTuning.KeyChipGap : 0f;
            float verbW = revealSkip20.GetPreferredValues(verb).x;
            float left = -(chipW + verbW) * 0.5f, y = revealSkip20.rectTransform.anchoredPosition.y;
            revealSkipChip20.rectTransform.anchoredPosition = new Vector2(left, y);
            revealSkip20.rectTransform.anchoredPosition = new Vector2(left + chipW, y);
        }

        /// <summary>The reveal opens: the offers, the slots' words, the pause.</summary>
        void Reveal20Begin()
        {
            revealRound20 = revealPendingRound20;
            revealPendingRound20 = -1;
            Reveal20.Offers(game.sim.runSeed, revealRound20, revealSalt20, revealBase20, revealPerRound20, revealTypes20, revealModsOn20, revealOffers20);
            bool pad = PlayerDevices.Last == InputDevice.Pad;
            for (int i = 0; i < 3; i++)
            {
                RevealOffer o = revealOffers20[i];
                revealTags20[i].SetText(o.kind == Reveal20.PowerUp ? "POWER-UP" : o.kind == Reveal20.Mod ? "MOD" : "POINTS");
                if (o.kind == Reveal20.PowerUp) revealHeroes20[i].SetText(PowerUpName20(o.type));
                else if (o.kind == Reveal20.Mod) revealHeroes20[i].SetText("A MODS PICK");
                else revealHeroes20[i].SetText("+{0:0}", o.points);
                revealHeroes20[i].color = o.kind == Reveal20.Points ? UiTheme.Gold : UiTheme.White;
                revealKeyChips20[i].gameObject.SetActive(!pad); // the pad picks with its focus (no digits on a pad: S12)
                SetRevealAlpha20(i, 0f);
            }
            Reveal20SkipRow(pad);
            revealAge20 = 0;
            revealOpen20 = true;
            reveal20.SetActive(true);
            revealEvent20.round = revealRound20;
            revealEvent20.source = revealSource20;
            revealEvent20.slot = revealEvent20.kind = -1;
            revealEvent20.type = null;
            revealEvent20.points = 0;
            game.events.Emit(UnityTuning.Hud20RevealEvent, revealEvent20);
            FeelProbe.Event(UnityTuning.Hud20RevealEvent);
            game.Pause();
        }

        /// <summary>Slot `i` taken (once the reveal has ended): its reward, the event, the game resumes.</summary>
        void Reveal20Take(int i)
        {
            if (!revealOpen20 || !Reveal20.Ready(revealAge20) || i < 0 || i > 2) return;
            RevealOffer o = revealOffers20[i];
            RevealApply20.Apply(game.sim, game.Feet, o, revealRound20);
            game.TapeLive?.RecordReveal(o.kind, o.type, o.points, revealRound20, game.Feet); // NIGHT20 (the lead): THE TAPE's RevealPick
            revealEvent20.slot = i;
            revealEvent20.kind = o.kind;
            revealEvent20.type = o.type;
            revealEvent20.points = o.points;
            game.events.Emit(UnityTuning.Hud20RevealPickEvent, revealEvent20);
            Reveal20Close();
            game.Resume();
        }

        void Reveal20Skip()
        {
            revealEvent20.slot = revealEvent20.kind = -1;
            revealEvent20.type = null;
            revealEvent20.points = 0;
            game.events.Emit(UnityTuning.Hud20RevealPickEvent, revealEvent20);
            Reveal20Close();
        }

        void Reveal20Close()
        {
            revealOpen20 = false;
            reveal20.SetActive(false);
        }

        static readonly System.Collections.Generic.Dictionary<string, string> RevealNames20 = new System.Collections.Generic.Dictionary<string, string>();

        /// <summary>A power-up's name (powerUps.types.<id>.name: DOUBLE POINTS, OVERCLOCK ...), cached.</summary>
        string PowerUpName20(string type)
        {
            if (type == null) return string.Empty;
            if (RevealNames20.TryGetValue(type, out string n)) return n;
            string p = "powerUps.types." + type + ".name";
            n = cfg.Has(p) ? cfg.Str(p) : type.ToUpperInvariant();
            RevealNames20[type] = n;
            return n;
        }
    }
}
