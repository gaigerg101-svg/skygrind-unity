// NIGHT20 U3-34.9 A8 (builder H): the NEXT line in the break (design.adds.next; Research/2026-10-06-additions-backlog.md A8:
// Hades shows what's behind each door, L4D's build-up, CoD's round number as the clock: "a heartbeat players can predict and
// plan for"). In the break, under the top line (ROUND 4 IN 12): the nearest special round in <= 3 words (NEXT: HOLD, SWARM
// IN 3, EXFIL OPEN while the radio can be called) and five slanted pips for rounds +1 .. +5: INK-SOFT ordinary, CYAN an
// objective or the way out, DANGER a threat, a taller DANGER pip for a boss (NextLine20.Tint).
// The kinds come from HudReads20.RoundKind (E's director.n20 read once it's filled: THE HUNT, THE ELITE; else the director's
// own schedule), read when the break's round changes; EXFIL OPEN from the exfil's radio (Exfil.CanCall) each frame.
// It enters with the break (alpha and a slide along the slant: HudEntrance18) and is gone in a fight. Top centre, outside
// S14's box. Words set on change only (cached strings), pips tinted on change: no allocation per frame.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform next20;
        TextMeshProUGUI nextWords20;
        UiShape[] nextPips20;
        CanvasGroup nextGroup20;
        readonly string[] nextKinds20 = new string[NextLine20.Pips];
        readonly int[] nextTint20 = { -1, -1, -1, -1, -1 };
        bool nextCfg20;
        int nextRound20 = -1;
        string nextLineShown20 = "\u0000";
        double nextAge20 = -1;
        float nextAlphaShown20 = -1;
        Vector2 nextOffShown20 = new Vector2(float.NaN, 0);

        void BuildNext20()
        {
            nextCfg20 = cfg.Design("adds.next.enabled");
            next20 = Hud17Rect(values, "Next line 20", new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -UnityTuning.Hud20NextY), new Vector2(600, 30));
            nextGroup20 = next20.gameObject.AddComponent<CanvasGroup>();
            nextGroup20.blocksRaycasts = nextGroup20.interactable = false;
            nextWords20 = Hud17Text(next20, "Words", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Right, new Vector2(0.5f, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(400, 28), true);
            nextWords20.characterSpacing = UnityTuning.ThemeLabelTracking;
            nextPips20 = new UiShape[NextLine20.Pips];
            for (int i = 0; i < nextPips20.Length; i++)
            {
                nextPips20[i] = Hud17Shape(next20, $"Pip {i}", UiTheme.InkSoft, new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud20NextPipW, UnityTuning.Hud20NextPipH));
                nextPips20[i].SetCuts(0, 0);
            }
            next20.gameObject.SetActive(false);
            game.OnNewRun += () => nextRound20 = -1;
        }

        void RenderNext20(double frameDt)
        {
            if (next20 == null) return;
            SurvivalRun run = game.run;
            bool on = nextCfg20 && game.sim != null && game.sim.n20.adds && run.state == SurvivalRun.State.Break && game.flow == Flow.Playing && run.round > 0;
            if (next20.gameObject.activeSelf != on)
            {
                next20.gameObject.SetActive(on);
                nextAge20 = 0; // each break enters
            }
            if (!on) return;

            // the kinds of rounds +1 .. +5 (in the break run.round is the coming round), on change of the round only
            if (run.round != nextRound20)
            {
                nextRound20 = run.round;
                for (int i = 0; i < NextLine20.Pips; i++)
                {
                    nextKinds20[i] = HudReads20.RoundKind(game, run.round + i);
                    int t = NextLine20.Tint(nextKinds20[i]);
                    if (t == nextTint20[i]) continue;
                    nextTint20[i] = t;
                    UiShape p = nextPips20[i];
                    p.color = t == 0 ? UiTheme.InkSoft : t == 1 ? UiTheme.Cyan : UiTheme.Danger;
                    p.rectTransform.sizeDelta = new Vector2(UnityTuning.Hud20NextPipW, t == 3 ? UnityTuning.Hud20NextPipBossH : UnityTuning.Hud20NextPipH);
                }
                nextLineShown20 = "\u0000"; // re-fit
            }
            bool exfil = game.sim.exfil != null && game.sim.exfil.CanCall();
            string line = NextLine20.Line(nextKinds20, exfil);
            if (!ReferenceEquals(line, nextLineShown20))
            {
                nextLineShown20 = line;
                nextWords20.SetText(line ?? string.Empty);
                // the words right of nothing when there's no line; else words, a gap, the pips, the whole centred
                float wordsW = line != null ? Mathf.Ceil(nextWords20.GetPreferredValues(line).x) : 0f;
                float pipsW = NextLine20.Pips * UnityTuning.Hud20NextPipW + (NextLine20.Pips - 1) * UnityTuning.Hud20NextPipGap;
                float gap = line != null ? UnityTuning.Hud20NextGap : 0f;
                float total = wordsW + gap + pipsW, left = -total * 0.5f;
                nextWords20.rectTransform.anchoredPosition = new Vector2(left + wordsW, 0);
                for (int i = 0; i < nextPips20.Length; i++)
                    nextPips20[i].rectTransform.anchoredPosition = new Vector2(left + wordsW + gap + i * (UnityTuning.Hud20NextPipW + UnityTuning.Hud20NextPipGap), 0);
            }

            // the entrance (alpha, a slide along the slant), then still
            if (nextAge20 >= 0)
            {
                nextAge20 = HudEntrance18.Step(nextAge20, frameDt);
                bool still = game.settings.View.reducedMotion;
                float a = UiTween.OutCubic(nextAge20 / UnityTuning.Hud20NextIn);
                Vector2 off = still ? Vector2.zero : -HudEntrance18.Along(UnityTuning.ThemeEnterDistance * (1f - UiTween.OutCubic(nextAge20 / UnityTuning.Hud20NextIn)));
                if (a != nextAlphaShown20) nextGroup20.alpha = nextAlphaShown20 = a;
                if (off != nextOffShown20)
                {
                    nextOffShown20 = off;
                    next20.anchoredPosition = new Vector2(0, -UnityTuning.Hud20NextY) + off;
                }
                if (nextAge20 >= UnityTuning.Hud20NextIn) nextAge20 = -1;
            }
        }
    }
}
