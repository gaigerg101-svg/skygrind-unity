// NIGHT20 U3-34.9 A1 / A4 (builder H): the boss bar and the elite's name plate, one threat bar at the bottom centre (the
// backlog's A1 BULWARK "a boss bar", A4 THE ELITE "a name plate and a small bar"; Dark Souls' boss bar: the name over a long
// bar at the bottom, out of the play space). E's reads (HudReads20.ThreatNow: the boss while it fights, else the round's
// named elite) give the name, the affixes ("CHARGER · VOLATILE · SWIFT"), the HP and the boss's phase mark.
// - An INK slanted plate with a DANGER accent bar on its left edge (an enemy's), the name (S24 DISPLAY for the boss, S18
//   BODY-BOLD for the elite, WHITE, the hard shadow), the bar: an INK-SOFT track, the WHITE chunk a hit took (held 0.35 s,
//   then draining: ThreatBar20), the DANGER fill, an INK phase tick.
// - It enters with its threat: fading in while the bar fills from empty (0.5 s, ease-out); it leaves when the threat dies.
// - The boss's bar is wide (640), the elite's small (440), both narrowed so they keep clear of the vitals plate at any HUD
//   SCALE (Hud20Layout.ThreatBar; Hud20Tests); out of S14's box.
// - The boss's plates (builder HV, A1: the BULWARK's BossBar pips / pipsOn): pips at the name line's right end, DANGER while
//   on, the one just lost WHITE and punched, then dark (BossPips20).
// Written on change only (the widths at 0.5 px, the line on a new threat); no allocation per frame.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>
    /// NIGHT20 A1 (builder HV): the boss's plates as pips on its bar's name line (BossBar.pips / pipsOn: the BULWARK's five;
    /// a wall ram drops the weakest, and a broken plate opens its socket): the plates still on lit in DANGER (its armour),
    /// the one just lost WHITE and punched for Hud20PipFlash s, then dark (INK-SOFT). The read of DOOM Eternal's armour
    /// chunks on a boss: what's left to strip.
    /// </summary>
    public static class BossPips20
    {
        /// <summary>Pip i's state: 0 on, 1 just lost (`lostAt` the pip lost last, -1 none; `lostAge` s since), 2 dark.</summary>
        public static int State(int i, int pipsOn, int lostAt, double lostAge) =>
            i < pipsOn ? 0 : i == lostAt && lostAge >= 0 && lostAge < UnityTuning.Hud20PipFlash ? 1 : 2;

        /// <summary>The lost pip's punch: Hud20PipPunch -> 1 over Hud20PipFlash (ease-out); REDUCED MOTION (`still`): 1.</summary>
        public static float Scale(double lostAge, bool still = false)
        {
            if (still || !(lostAge >= 0) || lostAge >= UnityTuning.Hud20PipFlash) return 1f;
            return 1f + (UnityTuning.Hud20PipPunch - 1f) * (1f - UiTween.OutCubic(lostAge / UnityTuning.Hud20PipFlash));
        }
    }

    public sealed partial class Ui
    {
        RectTransform threat20;
        CanvasGroup threatGroup20;
        UiShape threatPlate20, threatAccent20, threatTrack20, threatChunk20, threatFill20, threatTick20;
        TextMeshProUGUI threatName20;
        HudReads20.Threat threatNow20;
        readonly ThreatBar20 threatBar20 = new ThreatBar20();
        int threatEnemy20 = -2;
        bool threatBossShown20;
        double threatAge20;
        float threatBarW20 = -1, threatScaleKey20 = -1, threatCanvasW20 = -1, threatFillShown20 = -1, threatChunkShown20 = -1, threatAlpha20 = -1;
        int threatChunkWhite20 = -1;
        double threatChunkAge25 = double.MaxValue; // NIGHT25: seconds since the chunk opened
        float threatChunkAlpha25 = 1f;
        // A1 (builder HV): the boss's plates as pips (BossPips20)
        UiShape[] threatPips20;
        readonly int[] threatPipState20 = new int[UnityTuning.Hud20PipMax];
        int threatPipsShown20 = -1, threatPipsOn20 = -1, threatPipLost20 = -1;
        double threatPipAge20 = double.MaxValue;
        float threatPipScale20 = -1;

        void BuildThreat20()
        {
            threat20 = Hud17Rect(values, "Threat bar 20", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, UnityTuning.Hud20ThreatBottom), new Vector2(640, 50));
            threatGroup20 = threat20.gameObject.AddComponent<CanvasGroup>();
            threatGroup20.blocksRaycasts = threatGroup20.interactable = false;
            threatPlate20 = Hud17Shape(threat20, "Plate", UiTheme.Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660, 70));
            threatAccent20 = Hud17Shape(threatPlate20.rectTransform, "Accent", UiTheme.Danger, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(UnityTuning.ThemeAccentBar, 70));
            threatAccent20.SetCuts(0, 0);
            threatName20 = Hud17Text(threat20, "Name", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.Display, TextAlignmentOptions.BottomLeft, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(900, UnityTuning.Hud20ThreatNameH), true);
            threatTrack20 = Hud17Shape(threat20, "Track", UiTheme.InkSoft, new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, new Vector2(640, UnityTuning.Hud20ThreatBossH));
            threatChunk20 = Hud17Shape(threat20, "Chunk", UiTheme.White, new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, new Vector2(0, UnityTuning.Hud20ThreatBossH));
            threatFill20 = Hud17Shape(threat20, "Fill", UiTheme.Danger, new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, new Vector2(0, UnityTuning.Hud20ThreatBossH));
            threatTick20 = Hud17Shape(threat20, "Phase tick", UiTheme.InkSolid, new Vector2(0, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(UnityTuning.Hud17HpSlit, UnityTuning.Hud20ThreatBossH));
            foreach (UiShape s in new[] { threatTrack20, threatChunk20, threatFill20, threatTick20 }) s.SetCuts(0, 0);
            threatPips20 = new UiShape[UnityTuning.Hud20PipMax];
            for (int i = 0; i < threatPips20.Length; i++)
            {
                threatPips20[i] = Hud17Shape(threat20, $"Plate {i}", UiTheme.Danger, new Vector2(1, 1), new Vector2(1, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud20PipW, UnityTuning.Hud20PipH));
                threatPips20[i].SetCuts(0, 0);
                threatPips20[i].gameObject.SetActive(false);
            }
            threat20.gameObject.SetActive(false);
            game.OnNewRun += () => threatEnemy20 = -2;
        }

        void RenderThreat20(double frameDt)
        {
            if (threat20 == null) return;
            HudReads20.ThreatNow(game, ref threatNow20);
            bool on = threatNow20.valid && game.flow == Flow.Playing;
            if (threat20.gameObject.activeSelf != on) threat20.gameObject.SetActive(on);
            if (!on)
            {
                if (!threatNow20.valid) threatEnemy20 = -2;
                return;
            }
            bool boss = threatNow20.boss;
            if (threatNow20.enemy != threatEnemy20 || boss != threatBossShown20)
            {
                // a new threat: its line, its sizes, its entrance
                threatEnemy20 = threatNow20.enemy;
                threatBossShown20 = boss;
                threatAge20 = 0;
                threatBar20.Reset();
                threatBarW20 = threatFillShown20 = threatChunkShown20 = threatAlpha20 = -1;
                threatChunkWhite20 = -1;
                string line = ThreatBar20.Line(threatNow20.name, threatNow20.affixA, threatNow20.affixB) ?? string.Empty;
                UiTheme.Style(threatName20, UiTheme.Px(boss ? UiTheme.Size.S24 : UiTheme.Size.S18), UiTheme.White, boss ? UiTheme.Display : UiTheme.BodyBold, true);
                threatName20.characterSpacing = boss ? UnityTuning.ThemeDisplayTracking : UnityTuning.ThemeLabelTracking;
                threatName20.SetText(line);
            }
            Threat20Layout(boss);

            float frac = threatNow20.maxHp > 0 ? Mathf.Clamp01((float)(threatNow20.hp / threatNow20.maxHp)) : 0f;
            threatBar20.Step(frac, frameDt);
            threatAge20 = HudEntrance18.Step(threatAge20, frameDt);
            float sweep = ThreatBar20.Sweep(threatAge20);
            float shown = Mathf.Min(frac, sweep), trail = Mathf.Min(threatBar20.Trail, sweep);
            float barH = boss ? UnityTuning.Hud20ThreatBossH : UnityTuning.Hud20ThreatEliteH;
            float fw = Mathf.Round(threatBarW20 * shown * 2f) / 2f, cw = Mathf.Round(threatBarW20 * trail * 2f) / 2f;
            if (fw != threatFillShown20)
            {
                threatFillShown20 = fw;
                threatFill20.rectTransform.sizeDelta = new Vector2(fw, barH);
                threatFill20.enabled = fw > 0.5f;
            }
            if (cw != threatChunkShown20)
            {
                threatChunkShown20 = cw;
                threatChunk20.rectTransform.sizeDelta = new Vector2(cw, barH);
                bool chunkOn = cw > fw + 0.5f;
                if (chunkOn && !threatChunk20.enabled) threatChunkAge25 = 0; // NIGHT25 (T-12, M9): the chunk fades in when a hit first opens it
                threatChunk20.enabled = chunkOn;
            }
            int white = threatBar20.Chunk ? 1 : 0;
            float chunkA = threatChunkAge25 < UnityTuning.Fade25In ? UiFade25.In(threatChunkAge25 += frameDt, frameDt) : 1f;
            if (white != threatChunkWhite20 || chunkA != threatChunkAlpha25)
            {
                threatChunkWhite20 = white;
                threatChunkAlpha25 = chunkA;
                threatChunk20.color = UiTheme.A(white == 1 ? UiTheme.White : UiTheme.Muted, chunkA); // held WHITE, then MUTED as it drains
            }
            float a = HudEntrance18.Fade(threatAge20);
            if (a != threatAlpha20) threatGroup20.alpha = threatAlpha20 = a;
            Threat20Pips(boss ? threatNow20.pips : 0, threatNow20.pipsOn, frameDt);
        }

        /// <summary>A1 (builder HV): the plates' pips on the name line's right end, right to left as they're lost; written on change.</summary>
        void Threat20Pips(int pips, int pipsOn, double frameDt)
        {
            int n = Mathf.Min(pips, threatPips20.Length);
            if (n != threatPipsShown20)
            {
                // a new boss (or none): placed once, no flash for plates already gone
                threatPipsShown20 = n;
                threatPipsOn20 = pipsOn;
                threatPipLost20 = -1;
                threatPipAge20 = double.MaxValue;
                threatPipScale20 = -1;
                float y = -UnityTuning.Hud20ThreatNameH * 0.5f, step = UnityTuning.Hud20PipW + UnityTuning.Hud20PipGap;
                for (int i = 0; i < threatPips20.Length; i++)
                {
                    bool show = i < n;
                    threatPips20[i].gameObject.SetActive(show);
                    threatPipState20[i] = -1;
                    if (show) threatPips20[i].rectTransform.anchoredPosition = new Vector2(-(n - 1 - i) * step, y);
                }
            }
            if (n == 0) return;
            if (pipsOn < threatPipsOn20)
            {
                threatPipLost20 = pipsOn; // the right-most lit pip goes out
                threatPipAge20 = 0;
            }
            threatPipsOn20 = pipsOn;
            if (threatPipAge20 < UnityTuning.Hud20PipFlash) threatPipAge20 += frameDt;
            for (int i = 0; i < n; i++)
            {
                int st = BossPips20.State(i, pipsOn, threatPipLost20, threatPipAge20);
                if (st == threatPipState20[i]) continue;
                threatPipState20[i] = st;
                threatPips20[i].color = st == 0 ? UiTheme.Danger : st == 1 ? UiTheme.White : UiTheme.InkSoft;
            }
            if (threatPipLost20 < 0 || threatPipLost20 >= n) return;
            float s = BossPips20.Scale(threatPipAge20, game.settings.View.reducedMotion);
            if (s == threatPipScale20) return;
            threatPipScale20 = s;
            threatPips20[threatPipLost20].rectTransform.localScale = new Vector3(s, s, 1);
        }

        /// <summary>The block's width for the canvas, the HUD SCALE and the threat's kind (on change only).</summary>
        void Threat20Layout(bool boss)
        {
            float canvasW = root.rect.width, scale = hudScaleShown17 > 0 ? hudScaleShown17 : 1f;
            float key = scale * 10 + (boss ? 1 : 0);
            if (canvasW == threatCanvasW20 && key == threatScaleKey20 && threatBarW20 >= 0) return;
            threatCanvasW20 = canvasW;
            threatScaleKey20 = key;
            HudRect r = Hud20Layout.ThreatBar(canvasW, boss, scale);
            float barH = boss ? UnityTuning.Hud20ThreatBossH : UnityTuning.Hud20ThreatEliteH;
            threatBarW20 = r.w;
            threat20.sizeDelta = new Vector2(r.w, r.h);
            threatPlate20.rectTransform.sizeDelta = new Vector2(r.w + 2 * UnityTuning.Hud17StripPad + UnityTuning.ThemeAccentBar, r.h + 2 * UnityTuning.Hud17StripPad);
            threatAccent20.rectTransform.sizeDelta = new Vector2(UnityTuning.ThemeAccentBar, r.h + 2 * UnityTuning.Hud17StripPad - 2 * UnityTuning.ThemeCut);
            threatTrack20.rectTransform.sizeDelta = new Vector2(r.w, barH);
            double phase = threatNow20.phaseAt;
            bool tick = boss && phase > 0 && phase < 1;
            threatTick20.enabled = tick;
            if (tick)
            {
                threatTick20.rectTransform.anchoredPosition = new Vector2((float)(r.w * phase), 0);
                threatTick20.rectTransform.sizeDelta = new Vector2(UnityTuning.Hud17HpSlit, barH);
            }
            threatFillShown20 = threatChunkShown20 = -1;
        }
    }
}
