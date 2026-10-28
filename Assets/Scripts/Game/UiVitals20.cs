// NIGHT20 U3-34.10 (builder H): the regen's segments on the vitals plate and the round-end refill (GG 2026-10-06 evening:
// "the regen happens way too quickly and should only really be at the end of the round"; design.regen.n20, builder E: in a
// round the passive regen climbs only to the top of the current 25 HP segment; at the round's clear the bar refills).
// - The ticks: NIGHT19's quarter slits give way to an INK tick every segment (HpSegments20: 3 at 100 HP, 5 with the PLATE's
//   150), so a segment reads at a glance whatever the bar's max.
// - The reach: the HP the in-round regen will still give back (from the fill to the segment's top) shows CYAN-DEEP behind
//   the WHITE fill, while a round is on (Bloodborne's rally, Halo's segmented health: what you can win back reads at once).
// - The refill (the `roundHeal` event, kind "round"): a CYAN-PALE promise from the fill to the refill's target enters with
//   the round's banner, holds while the bar climbs, lingers and fades (Refill20). Any tick the fill reaches going up (the
//   refill, an earned heal, the regen's top) lights CYAN-HOT for Hud20TickFlash s (FeelProbe events "refillStart",
//   "refillTick").
// - The earned heals (`earnedHeal`: a close kill, a punch kill) pop "+N" right of the HP numeral, CYAN-HOT, summing heals
//   within 0.4 s, punched in, drifting up as it fades (HealPop20; FeelProbe event "earnedHealPop").
// Shown only while design.regen.n20 is live (HudReads20.Regen); -skygrindOff20: NIGHT19's quarters, nothing else. In the HP
// bar's own values rect (Ui.cs), skewed with the theme like the fill. Written on change only; no allocation per frame.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform hpBarValues20; // Ui.cs: the HP bar's values rect (the fill, the hard cap, the slits)
        UnityEngine.UI.Image[] hpSlits20; // Ui.cs: NIGHT19's quarter slits
        UnityEngine.UI.Image vitalsReach20, vitalsPromise20;
        UnityEngine.UI.Image[] vitalsTicks20;
        readonly double[] vitalsTickAge20 = new double[UnityTuning.Hud20MaxTicks];
        readonly int[] vitalsTickTint20 = new int[UnityTuning.Hud20MaxTicks];
        readonly Refill20 vitalsRefill20 = new Refill20();
        HudReads20.Seg vitalsSeg20;
        int vitalsLive20 = -1, vitalsTicksShown20 = -1;
        double vitalsMaxShown20 = -1, vitalsSegShown20 = -1;
        float vitalsReachFrom20 = -1, vitalsReachTo20 = -1, vitalsPromiseFrom20 = -1, vitalsPromiseTo20 = -1, vitalsPromiseAlpha20 = -1;
        float vitalsPrevFrac20 = -1, vitalsReachAlpha20 = -1;
        double vitalsReachAge20 = -1, vitalsClock20;
        TMPro.TextMeshProUGUI vitalsHealText20;
        Vector2 vitalsHealHome20;
        readonly HealPop20 vitalsHeal20 = new HealPop20();
        int vitalsHealShown20 = -1;

        void BuildVitals20()
        {
            if (hpBarValues20 == null) return;
            float slantTan = Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad);
            // behind the fill (the fill is the rect's first child): the reach and the promise draw first
            vitalsPromise20 = Img(hpBarValues20, "Refill promise", UiTheme.CyanPale, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            vitalsReach20 = Img(hpBarValues20, "Regen reach", UiTheme.CyanDeep, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            vitalsPromise20.transform.SetAsFirstSibling();
            vitalsReach20.transform.SetAsFirstSibling();
            UiSkew.Apply(vitalsPromise20, slantTan);
            UiSkew.Apply(vitalsReach20, slantTan);
            vitalsPromise20.enabled = vitalsReach20.enabled = false;
            vitalsTicks20 = new UnityEngine.UI.Image[UnityTuning.Hud20MaxTicks];
            for (int i = 0; i < vitalsTicks20.Length; i++)
            {
                var t = Img(hpBarValues20, $"Segment tick {i}", UiTheme.InkSolid, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.Hud17HpSlit, 0));
                UiSkew.Apply(t, slantTan);
                t.enabled = false;
                vitalsTicks20[i] = t;
                vitalsTickAge20[i] = double.MaxValue;
            }
            game.events.On<RoundHealEvent>(N20Events.RoundHeal, e =>
            {
                if (e == null || !vitalsSeg20.valid || e.kind != "round") return;
                Health h = game.health;
                double to = h.max > 0 ? Math.Min(h.Cap, h.hp + e.amount) / h.max : 1;
                vitalsRefill20.Start(to, e.seconds);
                FeelProbe.Event(UnityTuning.Hud20RefillEvent);
            });
            // the earned heal's "+N" right of the HP numeral (CYAN-HOT: the player's; S24 BODY-BOLD with the hard shadow)
            RectTransform hpValues = hpBarValues20.parent as RectTransform;
            if (hpValues != null)
            {
                vitalsHealText20 = Hud17Text(hpValues, "Earned heal", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.CyanHot, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(0, 1), new Vector2(0, 1), new Vector2(UnityTuning.Hud17VitalsPad + UnityTuning.Hud20HealX, -8), new Vector2(120, 30), true);
                vitalsHealHome20 = vitalsHealText20.rectTransform.anchoredPosition;
                vitalsHealText20.gameObject.SetActive(false);
            }
            game.events.On<RoundHealEvent>(N20Events.EarnedHeal, e =>
            {
                if (e == null || !vitalsSeg20.valid) return;
                vitalsHeal20.Add(e.amount, vitalsClock20);
                FeelProbe.Event(UnityTuning.Hud20HealEvent);
            });
            game.OnNewRun += () =>
            {
                vitalsRefill20.Reset();
                vitalsHeal20.Reset();
                vitalsPrevFrac20 = -1;
            };
        }

        /// <summary>The earned heal's "+N": punched in, held, drifting up and fading (HealPop20); written on change only.</summary>
        void RenderHealPop20(double frameDt)
        {
            if (vitalsHealText20 == null) return;
            vitalsClock20 += frameDt;
            bool on = vitalsSeg20.valid && vitalsHeal20.Shown;
            if (vitalsHealText20.gameObject.activeSelf != on) vitalsHealText20.gameObject.SetActive(on);
            if (!on) return;
            int n = vitalsHeal20.Amount;
            if (n != vitalsHealShown20)
            {
                vitalsHealShown20 = n;
                vitalsHealText20.SetText("+{0:0}", n);
            }
            double age = vitalsHeal20.Age;
            float s = game.settings.View.reducedMotion ? 1f : HealPop20.Scale(age);
            vitalsHealText20.rectTransform.localScale = new Vector3(s, s, 1);
            vitalsHealText20.rectTransform.anchoredPosition = vitalsHealHome20 + new Vector2(0, game.settings.View.reducedMotion ? 0f : HealPop20.Rise(age));
            vitalsHealText20.alpha = HealPop20.Alpha(age) * UiFade25.In(age, frameDt); // NIGHT25 (T-12, M9)
            vitalsHeal20.Step(frameDt);
        }

        void RenderVitals20(double frameDt, float frac)
        {
            if (vitalsTicks20 == null) return;
            HudReads20.Regen(game, ref vitalsSeg20);
            bool live = vitalsSeg20.valid;
            if ((live ? 1 : 0) != vitalsLive20)
            {
                vitalsLive20 = live ? 1 : 0;
                for (int i = 0; i < hpSlits20.Length; i++) hpSlits20[i].enabled = !live; // NIGHT19's quarters with the block off
                vitalsTicksShown20 = -1;
                if (!live)
                {
                    for (int i = 0; i < vitalsTicks20.Length; i++) vitalsTicks20[i].enabled = false;
                    vitalsReach20.enabled = vitalsPromise20.enabled = false;
                    vitalsRefill20.Reset();
                    vitalsHeal20.Reset();
                }
            }
            RenderHealPop20(frameDt);
            if (!live) return;
            Health h = game.health;
            frac = Mathf.Clamp01(frac);

            // the ticks: one every segment (placed when the max or the segment changes)
            int n = HpSegments20.Ticks(h.max, vitalsSeg20.segment);
            if (n != vitalsTicksShown20 || h.max != vitalsMaxShown20 || vitalsSeg20.segment != vitalsSegShown20)
            {
                vitalsTicksShown20 = n;
                vitalsMaxShown20 = h.max;
                vitalsSegShown20 = vitalsSeg20.segment;
                for (int i = 0; i < vitalsTicks20.Length; i++)
                {
                    bool show = i < n;
                    vitalsTicks20[i].enabled = show;
                    if (!show) continue;
                    float at = HpSegments20.TickAt(i, h.max, vitalsSeg20.segment);
                    RectTransform rt = vitalsTicks20[i].rectTransform;
                    rt.anchorMin = new Vector2(at, 0);
                    rt.anchorMax = new Vector2(at, 1);
                }
            }

            // a tick the fill reached going up (the refill, an earned heal, the regen's top) lights CYAN-HOT
            if (vitalsPrevFrac20 >= 0)
            {
                int crossed = HpSegments20.Crossed(vitalsPrevFrac20, frac, n, h.max, vitalsSeg20.segment);
                if (crossed >= 0)
                {
                    vitalsTickAge20[crossed] = 0;
                    FeelProbe.Event(UnityTuning.Hud20TickEvent);
                }
            }
            vitalsPrevFrac20 = frac;
            for (int i = 0; i < n; i++)
            {
                int tint = vitalsTickAge20[i] < UnityTuning.Hud20TickFlash ? 1 : 0;
                if (tint != vitalsTickTint20[i])
                {
                    vitalsTickTint20[i] = tint;
                    vitalsTicks20[i].color = tint == 1 ? UiTheme.CyanHot : UiTheme.InkSolid;
                }
                if (vitalsTickAge20[i] < UnityTuning.Hud20TickFlash) vitalsTickAge20[i] += frameDt;
            }

            // the reach: the fill to the segment's top, while a round is on and the regen has HP to give back
            bool fight = game.run != null && game.run.state == SurvivalRun.State.Fight;
            float to = h.max > 0 ? Mathf.Clamp01((float)(vitalsSeg20.top / h.max)) : 0f;
            bool reach = fight && to > frac + 0.001f && !vitalsRefill20.Active;
            if (vitalsReach20.enabled != reach)
            {
                vitalsReach20.enabled = reach;
                vitalsReachAge20 = 0; // M9: it fades in, never whole on its first frame
                vitalsReachAlpha20 = -1;
            }
            if (reach && vitalsReachAge20 >= 0)
            {
                vitalsReachAge20 = HudEntrance18.Step(vitalsReachAge20, frameDt);
                float ra = HudEntrance18.Fade(vitalsReachAge20);
                if (ra != vitalsReachAlpha20)
                {
                    vitalsReachAlpha20 = ra;
                    vitalsReach20.color = UiTheme.A(UiTheme.CyanDeep, ra);
                }
                if (ra >= 1f) vitalsReachAge20 = -1;
            }
            if (reach && (frac != vitalsReachFrom20 || to != vitalsReachTo20))
            {
                vitalsReachFrom20 = frac;
                vitalsReachTo20 = to;
                vitalsReach20.rectTransform.anchorMin = new Vector2(frac, 0);
                vitalsReach20.rectTransform.anchorMax = new Vector2(to, 1);
            }

            // the refill's promise: the fill to the target, entering, held, lingering, fading
            vitalsRefill20.Step(frameDt, frac);
            float pa = vitalsRefill20.Alpha;
            bool promise = vitalsRefill20.Active && pa > 0.001f && vitalsRefill20.Target > frac + 0.001f;
            if (vitalsPromise20.enabled != promise) vitalsPromise20.enabled = promise;
            if (!promise) return;
            float pt = vitalsRefill20.Target;
            if (frac != vitalsPromiseFrom20 || pt != vitalsPromiseTo20)
            {
                vitalsPromiseFrom20 = frac;
                vitalsPromiseTo20 = pt;
                vitalsPromise20.rectTransform.anchorMin = new Vector2(frac, 0);
                vitalsPromise20.rectTransform.anchorMax = new Vector2(pt, 1);
            }
            pa = Mathf.Round(pa * 100f) / 100f;
            if (pa != vitalsPromiseAlpha20)
            {
                vitalsPromiseAlpha20 = pa;
                vitalsPromise20.color = UiTheme.A(UiTheme.CyanPale, pa);
            }
        }
    }
}
