// NIGHT20 U3-34.9 F4, the player-facing half (builder HV; design.adds.downtell; Research/2026-10-06-additions-backlog.md F4:
// a death should say what to do next time, as DOOM Eternal's codex tips and Hades' death screen do). The killer's kind and
// its tell in <= 4 words (DownTell20, Hud20Look.cs: each kind's debut card counter, e.g. CHARGER · SIDESTEP INTO WALLS):
// - WHAT GOT YOU's replay: under the heading's INK block (UiRecap.cs, read, not edited), the tell on an INK slanted plate
//   with a CYAN foot (the player's lesson: cyan is the player's), entering Hud20TellRecapDelay s into the replay with a fade
//   and a rise (REDUCED MOTION: the fade only).
// - DOWN: NIGHT19's "KILLED BY HUSK" line becomes KILLED BY + the kind on the recap's DANGER plate (INK words, as the
//   replay names it), the same tell plate under it; the recap's next-target line and the retry chip move down under them.
//   The row enters with DOWN's other lines (NIGHT18's parts, UiMenus.MenuPart18Add).
// Off (design.adds.downtell or the adds block off, or -skygrindOff20): NIGHT19's DOWN and replay. Texts and sizes set on a
// change of the killer only; no allocation per frame.
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>The tell's plate: an INK slanted plate sized to its CYAN-PALE words, a CYAN foot along its lower edge.</summary>
        sealed class TellPlate20
        {
            public RectTransform root;
            public UiShape plate, foot;
            public TextMeshProUGUI words;
        }

        DownTell20 downTell20;
        // DOWN: KILLED BY [KIND] over the tell plate
        RectTransform downTellRoot20;
        TextMeshProUGUI downTellKilled20, downTellKind20;
        UiShape downTellKindPlate20;
        TellPlate20 downTellPlate20;
        float downTellKilledW20, downTellRowH20, downTellLineH20;
        float downTellShift20, downTellRetryShift20; // px DOWN's recap line and its retry chip moved down under the tell
        // WHAT GOT YOU: the tell plate under the heading's block
        TellPlate20 recapTell20;
        CanvasGroup recapTellGroup20;
        string recapTellShown20;
        double recapTellAge20 = double.NaN; // NaN: not playing
        float recapTellY20, recapTellAlphaShown20 = -1, recapTellLiftShown20 = -1;

        void BuildDownTell20()
        {
            if (!cfg.Design("adds.downtell.enabled") || down == null) return;
            var names = new Dictionary<string, string>();
            foreach (var kv in cfg.Obj("ui.recap.names")) names[kv.Key] = (string)kv.Value;
            Dictionary<string, string> bosses = null;
            if (cfg.Has("bossKit.names"))
            {
                bosses = new Dictionary<string, string>();
                foreach (var kv in cfg.Obj("bossKit.names")) bosses[kv.Key] = (string)kv.Value;
            }
            downTell20 = new DownTell20(names, bosses);

            // --- DOWN's row: KILLED BY + the kind's DANGER plate (centred as one), the tell plate under it -----------------------
            float px24 = UiTheme.Px(UiTheme.Size.S24), px32 = UiTheme.Px(UiTheme.Size.S32);
            downTellRowH20 = px32 + 2 * UnityTuning.RecapTagPadY;
            float rootH = downTellRowH20 + UnityTuning.Hud20TellGap + UnityTuning.Hud20TellPlateH;
            downTellRoot20 = Hud17Rect(down.transform, "Down tell 20", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1), new Vector2(0, UnityTuning.DownRecapY), new Vector2(1200, rootH));
            downTellKilled20 = Hud17Text(downTellRoot20, "Killed by", "KILLED BY", px24, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Left,
                new Vector2(0.5f, 1), new Vector2(0, 0.5f), new Vector2(0, -downTellRowH20 * 0.5f), new Vector2(400, downTellRowH20), true);
            downTellKilledW20 = Mathf.Ceil(downTellKilled20.GetPreferredValues("KILLED BY").x);
            downTellKindPlate20 = Hud17Shape(downTellRoot20, "Kind plate", UiTheme.Danger, new Vector2(0.5f, 1), new Vector2(0, 0.5f), new Vector2(0, -downTellRowH20 * 0.5f), new Vector2(200, downTellRowH20));
            downTellKind20 = Hud17Text(downTellKindPlate20.rectTransform, "Kind", "", px32, UiTheme.InkSolid, UiTheme.Display, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, downTellRowH20), false);
            downTellPlate20 = TellPlate20Make(downTellRoot20, -(downTellRowH20 + UnityTuning.Hud20TellGap));
            // One line of DOWN's recap text (its line advance): the KILLED BY line the row replaces.
            downTellLineH20 = downRecap.GetPreferredValues("A\nA").y - downRecap.GetPreferredValues("A").y;
            MenuPart18Add(down, downTellRoot20); // enters with DOWN's lines (after the retry: the lesson last)
            downTellRoot20.gameObject.SetActive(false);

            // --- WHAT GOT YOU: under the heading's block (UiRecap.BuildRecap's own sizes) ----------------------------------------
            if (recapRoot == null) return; // design.recap off: no replay
            float pad = UnityTuning.RecapBlockPad;
            float blockH = UnityTuning.RecapHeadHeight + 30 + UnityTuning.RecapTagGap + px32 + 2 * UnityTuning.RecapTagPadY + 2 * pad;
            recapTellY20 = -(UnityTuning.RecapHeadTop - pad + blockH + UnityTuning.Hud20TellGap);
            recapTell20 = TellPlate20Make(recapRoot, recapTellY20);
            recapTellGroup20 = recapTell20.root.gameObject.AddComponent<CanvasGroup>();
            recapTellGroup20.alpha = 0;
            recapTell20.root.gameObject.SetActive(false);
        }

        TellPlate20 TellPlate20Make(Transform parent, float y)
        {
            var t = new TellPlate20();
            float h = UnityTuning.Hud20TellPlateH, px = UiTheme.Px(UiTheme.Size.S24);
            t.root = Hud17Rect(parent, "Tell 20", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(400, h));
            t.plate = Hud17Shape(t.root, "Plate", UiTheme.Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, h));
            t.foot = Hud17Shape(t.root, "Foot", UiTheme.Cyan, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(400, UnityTuning.ThemeAccentBar));
            t.foot.SetCuts(0, 0);
            t.words = Hud17Text(t.root, "Words", "", px, UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, UnityTuning.ThemeAccentBar * 0.5f), new Vector2(1000, h), true);
            t.words.characterSpacing = UnityTuning.ThemeLabelTracking;
            return t;
        }

        /// <summary>The plate's words and its width (on a change of the tell only).</summary>
        static void TellPlate20Set(TellPlate20 t, string words)
        {
            t.words.SetText(words);
            float w = Mathf.Ceil(t.words.GetPreferredValues(words).x) + 2 * UnityTuning.Hud20TellPadX;
            t.root.sizeDelta = new Vector2(w, UnityTuning.Hud20TellPlateH);
            t.plate.rectTransform.sizeDelta = new Vector2(w, UnityTuning.Hud20TellPlateH);
            t.foot.rectTransform.sizeDelta = new Vector2(w, UnityTuning.ThemeAccentBar);
        }

        bool DownTell20Live => downTell20 != null && game.sim != null && game.sim.n20.adds;

        /// <summary>
        /// From RenderMenus when DOWN's death changes: lay out KILLED BY [KIND] and the tell for `killer` (DeathRecap's name).
        /// True: the row shows (DOWN's recap drops its KILLED BY line); false: NIGHT19's line. Moves DOWN's recap line and its
        /// retry chip down under the row (MenuDownRetry reads downTellRetryShift20).
        /// </summary>
        bool DownTell20Set(string killer)
        {
            if (downTellRoot20 == null) return false;
            string kind = DownTell20Live ? downTell20.Kind(killer) : null, tell = kind != null ? downTell20.Tell(killer) : null;
            bool on = tell != null;
            if (downTellRoot20.gameObject.activeSelf != on) downTellRoot20.gameObject.SetActive(on);
            float shift = on ? downTellRowH20 + UnityTuning.Hud20TellGap + UnityTuning.Hud20TellPlateH + UnityTuning.Hud20TellGap : 0;
            if (shift != downTellShift20)
            {
                downTellShift20 = shift;
                downTellRetryShift20 = on ? Mathf.Max(0, shift - downTellLineH20) : 0;
                RectTransform r = downRecap.rectTransform;
                r.anchoredPosition = new Vector2(r.anchoredPosition.x, UnityTuning.DownRecapY - shift);
                menuDownDevice = (InputDevice)(-1); // the retry chip lays out again under it
            }
            if (!on) return false;
            downTellKind20.SetText(kind);
            float plateW = Mathf.Ceil(downTellKind20.GetPreferredValues(kind).x) + 2 * UnityTuning.RecapTagPadX;
            float total = downTellKilledW20 + UnityTuning.Hud20TellKillGap + plateW, left = -total * 0.5f;
            downTellKilled20.rectTransform.anchoredPosition = new Vector2(left, -downTellRowH20 * 0.5f);
            downTellKindPlate20.rectTransform.anchoredPosition = new Vector2(left + downTellKilledW20 + UnityTuning.Hud20TellKillGap, -downTellRowH20 * 0.5f);
            downTellKindPlate20.rectTransform.sizeDelta = new Vector2(plateW, downTellRowH20);
            TellPlate20Set(downTellPlate20, tell);
            return true;
        }

        /// <summary>Every frame after RenderRecap: WHAT GOT YOU's tell under its block while the replay plays.</summary>
        void RenderDownTell20(double frameDt)
        {
            if (recapTell20 == null) return;
            string tell = DownTell20Live && game.RecapPlaying ? downTell20.Tell(game.RecapKillerName) : null;
            bool on = tell != null;
            if (recapTell20.root.gameObject.activeSelf != on) recapTell20.root.gameObject.SetActive(on);
            if (!on)
            {
                recapTellAge20 = double.NaN;
                return;
            }
            if (!ReferenceEquals(tell, recapTellShown20))
            {
                recapTellShown20 = tell;
                TellPlate20Set(recapTell20, tell);
            }
            if (double.IsNaN(recapTellAge20)) recapTellAge20 = -UnityTuning.Hud20TellRecapDelay; // the replay's first frame
            else if (recapTellAge20 < UnityTuning.Hud18FadeInTime) recapTellAge20 += frameDt;
            float k = recapTellAge20 <= 0 ? 0f : HudEntrance18.Fade(recapTellAge20);
            float a = Mathf.Round(k * 100f) / 100f;
            float lift = game.settings.View.reducedMotion ? 0f : Mathf.Round(UnityTuning.Hud20TellLift * (1f - k));
            if (a != recapTellAlphaShown20)
            {
                recapTellAlphaShown20 = a;
                recapTellGroup20.alpha = a;
            }
            if (lift != recapTellLiftShown20)
            {
                recapTellLiftShown20 = lift;
                recapTell20.root.anchoredPosition = new Vector2(0, recapTellY20 - lift);
            }
        }
    }
}
