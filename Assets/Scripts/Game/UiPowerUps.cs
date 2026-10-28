// NIGHT8 U2-12: the running power-ups on the HUD (ui/powerUpHud.js): each active one's name in its colour with its whole
// seconds left (REFUND: until the next buy).
// NIGHT17 U3-30.7 (builder H; round 3's reviewer: collecting DOUBLE POINTS showed only an ~11 px gold line "DOUBLE POINTS
// 30" under the round): a pickup is a moment, then a chip.
// - On `powerUpGrab`: a 100 ms edge flash in the power-up's colour (through game.flashGate, x FLASH INTENSITY; none at 0);
//   its NAME as a banner (S75 DISPLAY, the power-up's colour, the INK stroke) 250 px over the centre: in at 1.4x (ease-out-
//   back, 150 ms), shown 1 s, then it shrinks into its chip beside the score over 250 ms;
// - each running power-up a CHIP left of the score's plate: INK with the theme's cut, a 2-character glyph (2x, OC, ST, 1/2:
//   UnityTuning.Hud17PowerGlyphs; no word) and a RING timer in its colour (the time left as a clockwise fill: no "30");
//   REFUND's ring stays full until the next buy. A one-shot (SHOCKWAVE) is the banner only.
// The colours are config.json's power-up colours (DOUBLE POINTS is gold: points). The grab's handler only queues; no
// allocation per frame (rings and glyphs set on change, the chips laid out when the set changes).
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class PowerChip
        {
            public UnityEngine.UI.Image plate, ring, ringTrack;
            public TextMeshProUGUI glyph;
            public int shownFill = -1;
            public bool on;
            public CanvasGroup group18; // NIGHT18 U3-31.6: the chip's fade-in (its ring and glyph with it)
            public double enterAge18 = double.MaxValue;
            public float shownScale18 = 1, shownAlpha18 = 1;
        }

        // NIGHT18 U3-31.6 (FeelAB on; NIGHT17's reviewer: DOUBLE POINTS "popped in at full size"; M9: a 2x slam DOWN is a
        // pop-in by the reel's rule): the banner ENTERS (HudEntrance18: grows from 0.6x with its overshoot, fades in, slides
        // ThemeEnterDistance px along the slant), holds, then shrinks into its chip as before; the chip it lands as enters too
        // (grows and fades in); the edge flash swells in over Hud18FlashAttack. Off: NIGHT17's path.
        double powerEdgeAge18 = double.MaxValue;
        bool powerBannerHot18; // round 3: the banner shows white-hot (its first frames)

        PowerChip[] powerChips;
        Color[] powerColor;
        string[] powerName;
        double[] powerTime;
        UnityEngine.UI.Image powerEdge;
        TextMeshProUGUI powerBanner;
        double powerEdgeLeft, powerBannerAge = double.MaxValue;
        int powerBannerType = -1, powerPending = -1, powerShownSet = -1;
        float powerShownPlateW = -1, powerShownEdge = -1;
        bool powerBuilt;

        void BuildPowerUps()
        {
            powerBuilt = true;
            int n = PowerUps.Types.Length;
            powerChips = new PowerChip[n];
            powerColor = new Color[n];
            powerName = new string[n];
            powerTime = new double[n];
            Sprite ring = RingSprite(UnityTuning.Hud17PowerRing, UnityTuning.Hud17PowerRingWidth);
            var chipSize = new Vector2(UnityTuning.Hud17PowerChip, UnityTuning.Hud17PowerChip);
            for (int i = 0; i < n; i++)
            {
                string type = PowerUps.Types[i];
                powerColor[i] = Look.C(cfg, $"powerUps.types.{type}.color");
                powerColor[i].a = 1;
                powerName[i] = cfg.Str($"powerUps.types.{type}.name");
                powerTime[i] = cfg.Has($"powerUps.types.{type}.time") ? cfg.Num($"powerUps.types.{type}.time") : 0;
                var c = new PowerChip();
                c.plate = Img(values, $"Power chip {type}", UiTheme.InkSolid, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, chipSize);
                c.plate.rectTransform.pivot = new Vector2(1, 0.5f);
                c.plate.sprite = UiTheme.Cut;
                c.plate.type = UnityEngine.UI.Image.Type.Sliced;
                c.ringTrack = Img(c.plate.transform, "Ring track", UiTheme.InkSoft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, ring.rect.size);
                c.ringTrack.sprite = ring;
                c.ring = Img(c.plate.transform, "Ring", powerColor[i], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, ring.rect.size);
                c.ring.sprite = ring;
                c.ring.type = UnityEngine.UI.Image.Type.Filled;
                c.ring.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
                c.ring.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
                c.ring.fillClockwise = false; // it runs down anticlockwise from the top
                string glyph = UnityTuning.Hud17PowerGlyphs.TryGetValue(type, out string g) ? g : "";
                c.glyph = Hud17Text(c.plate.transform, "Glyph", glyph, UiTheme.Px(UiTheme.Size.S18), UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, chipSize, false);
                c.group18 = c.plate.gameObject.AddComponent<CanvasGroup>();
                c.group18.interactable = c.group18.blocksRaycasts = false;
                c.plate.gameObject.SetActive(false);
                powerChips[i] = c;
            }
            // The banner (the sight layer: over everything, not scaled by HUD SCALE) and the edge (the frame layer, under the plates).
            powerBanner = Hud17Text(sight, "Power-up banner", "", UiTheme.Px(UiTheme.Size.S75), UiTheme.Gold, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, UnityTuning.Hud17PowerBannerY), new Vector2(1400, 100), true);
            powerBanner.outlineWidth = UnityTuning.Hud17PopOutline;
            powerBanner.outlineColor = UiTheme.InkSolid;
            powerBanner.gameObject.SetActive(false);
            powerEdge = Img(frame, "Power-up edge", UiTheme.A(UiTheme.Gold, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            powerEdge.sprite = InsetShadowSprite(UnityTuning.ChargeEdgeBlur, UnityTuning.ChargeEdgeSpread, 1);
            powerEdge.type = UnityEngine.UI.Image.Type.Sliced;
            powerEdge.raycastTarget = false;
            powerEdge.transform.SetAsFirstSibling(); // under the HUD's plates
            powerEdge.enabled = false;
            game.events.On<PowerUpEvent>("powerUpGrab", e => powerPending = Array.IndexOf(PowerUps.Types, e.type)); // only queues
            game.OnNewRun += () =>
            {
                powerPending = -1;
                powerBannerAge = double.MaxValue;
                powerEdgeLeft = 0;
            };
        }

        void RenderPowerUps(double frameDt)
        {
            if (!powerBuilt) BuildPowerUps();
            PowerUps pu = game.powerUps;

            // A grab: the edge flash (the flash gate) and the banner.
            if (powerPending >= 0)
            {
                int i = powerPending;
                powerPending = -1;
                powerBannerType = i;
                powerBannerAge = 0;
                powerBanner.text = powerName[i];
                powerBanner.color = powerColor[i];
                powerBannerHot18 = false; // its colour now (the slam turns it white on its first frame)
                if (game.settings.Num("flashIntensity") > 0 && game.flashGate.TryFlash(SurvivalGame.RealNow))
                {
                    powerEdgeLeft = UnityTuning.Hud17PowerEdgeTime;
                    powerEdge.color = UiTheme.A(powerColor[i], 0);
                    powerEdgeAge18 = FeelAB.On ? 0 : double.MaxValue;
                }
            }
            if (FeelAB.On && powerEdgeAge18 < UnityTuning.Hud18PowerEdgeRise)
            {
                powerEdgeAge18 += frameDt; // NIGHT18 U3-31.6: the rise runs first (round 1: 120 ms, it read as a pop at 40), then NIGHT17's 100 ms fade
                powerEdgeLeft = UnityTuning.Hud17PowerEdgeTime + frameDt;
            }
            powerEdgeLeft = Math.Max(0, powerEdgeLeft - frameDt);
            float ea = (float)(powerEdgeLeft / UnityTuning.Hud17PowerEdgeTime) * UnityTuning.Hud17PowerEdgeAlpha * (float)Math.Max(0, Math.Min(1, game.settings.Num("flashIntensity")));
            if (FeelAB.On && powerEdgeAge18 < UnityTuning.Hud18PowerEdgeRise + frameDt) ea *= UiTween.InOutCubic(powerEdgeAge18 / UnityTuning.Hud18PowerEdgeRise);
            if (ea != powerShownEdge)
            {
                powerShownEdge = ea;
                powerEdge.enabled = ea > 0;
                Color c = powerEdge.color;
                c.a = ea;
                powerEdge.color = c;
            }

            // The chips: one per running power-up, right to left from the score's plate.
            int set = 0;
            for (int i = 0; i < powerChips.Length; i++)
            {
                double left = pu.Left(PowerUps.Types[i]);
                bool on = left != 0 && (powerTime[i] > 0 || left < 0) && !(powerBannerType == i && powerBannerAge < UnityTuning.Hud17PowerBannerIn + UnityTuning.Hud17PowerBannerHold + UnityTuning.Hud17PowerBannerFly);
                if (on && Hud30HidesPower(i)) on = false; // NIGHT30 P0 f (design.n30.hud30): the REFUND "½" chip means nothing in a campaign run (Hud30.cs)
                if (on) set |= 1 << i;
                PowerChip c = powerChips[i];
                if (on != c.on)
                {
                    c.on = on;
                    c.plate.gameObject.SetActive(on);
                    if (on) c.enterAge18 = 0;
                }
                if (!on) continue;
                // NIGHT18 U3-31.6: the chip enters (grows from its right edge and fades in; FeelAB on)
                float cs = 1, ca = 1;
                if (FeelAB.On && !HudEntrance18.Done(c.enterAge18))
                {
                    c.enterAge18 = HudEntrance18.Step(c.enterAge18, frameDt);
                    cs = HudEntrance18.Scale(c.enterAge18);
                    ca = HudEntrance18.Alpha(c.enterAge18);
                }
                if (cs != c.shownScale18)
                {
                    c.shownScale18 = cs;
                    c.plate.rectTransform.localScale = new Vector3(cs, cs, 1);
                }
                if (ca != c.shownAlpha18)
                {
                    c.shownAlpha18 = ca;
                    c.group18.alpha = ca;
                }
                double share = left < 0 ? 1 : powerTime[i] > 0 ? Math.Max(0, Math.Min(1, left / powerTime[i])) : 1;
                int k = (int)Math.Round(share * 200);
                if (k != c.shownFill)
                {
                    c.shownFill = k;
                    c.ring.fillAmount = k / 200f;
                }
            }
            float plateW = pointsPlate17 != null ? pointsPlate17.rectTransform.sizeDelta.x : 0;
            if (set != powerShownSet || plateW != powerShownPlateW)
            {
                powerShownSet = set;
                powerShownPlateW = plateW;
                float x = -margin + UnityTuning.Hud17PointsPlatePad - plateW - UnityTuning.Hud17PowerChipGap;
                float y = margin + (UnityTuning.Hud17PointsBox + UnityTuning.Hud17PointsBarHeight + UnityTuning.Hud17PointsBarGap) * 0.5f;
                for (int i = 0; i < powerChips.Length; i++)
                {
                    if (!powerChips[i].on) continue;
                    powerChips[i].plate.rectTransform.anchoredPosition = new Vector2(x, y);
                    x -= UnityTuning.Hud17PowerChip + UnityTuning.Hud17PowerChipGap;
                }
            }

            // The banner: in, held, then it shrinks toward the chips' place beside the score.
            double total = UnityTuning.Hud17PowerBannerIn + UnityTuning.Hud17PowerBannerHold + UnityTuning.Hud17PowerBannerFly;
            bool shown = powerBannerAge < total && game.flow == Flow.Playing;
            if (powerBanner.gameObject.activeSelf != shown) powerBanner.gameObject.SetActive(shown);
            if (!shown) return;
            if (FeelAB.On)
            {
                // NIGHT18 round 2's picks ("DOUBLE POINTS slams down from about 1.4x [NIGHT17]; [tonight] grows from a faded
                // 0.8x"): the banner SLAMS (HudSlam18: about 1.4x down to 1 over 150 ms, fading in from about 0.6), holds, then
                // shrinks into its chip as before; drawn at its age, then stepped
                double age = powerBannerAge;
                double flyAt18 = UnityTuning.Hud17PowerBannerIn + UnityTuning.Hud17PowerBannerHold;
                float fly18 = age <= flyAt18 ? 0f : UiTween.InCubic((age - flyAt18) / UnityTuning.Hud17PowerBannerFly);
                powerBanner.rectTransform.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, UnityTuning.Hud17PowerBannerY), PowerChipPoint(), fly18);
                float sc18 = HudSlam18.Scale(age) * Mathf.Lerp(1f, 0.2f, fly18);
                powerBanner.rectTransform.localScale = new Vector3(sc18, sc18, 1);
                // round 3's picks: white-hot on the slam's first frames, then its own colour (FLASH INTENSITY 0: its colour)
                bool hot18 = HudSlam18.White(age) && game.settings.Num("flashIntensity") > 0;
                if (hot18 != powerBannerHot18)
                {
                    powerBannerHot18 = hot18;
                    powerBanner.color = hot18 ? UiTheme.White : powerColor[powerBannerType];
                }
                powerBanner.alpha = HudSlam18.Alpha(age) * (1f - fly18 * fly18);
                powerBannerAge += frameDt;
                return;
            }
            // round 5: the slam entrance (2x -> 1x, ease-out cubic), its first frame drawn at age 0; the clock moves after
            float s = powerBannerAge < UnityTuning.Hud17PowerBannerIn ? Mathf.LerpUnclamped(UnityTuning.Hud17PowerBannerFrom, 1f, UiTween.OutCubic(powerBannerAge / UnityTuning.Hud17PowerBannerIn)) : 1f;
            double flyAt = UnityTuning.Hud17PowerBannerIn + UnityTuning.Hud17PowerBannerHold;
            float fly = powerBannerAge <= flyAt ? 0f : UiTween.InCubic((powerBannerAge - flyAt) / UnityTuning.Hud17PowerBannerFly);
            var from = new Vector2(0, UnityTuning.Hud17PowerBannerY);
            Vector2 to = PowerChipPoint();
            powerBanner.rectTransform.anchoredPosition = Vector2.LerpUnclamped(from, to, fly);
            float sc = s * Mathf.Lerp(1f, 0.2f, fly);
            powerBanner.rectTransform.localScale = new Vector3(sc, sc, 1);
            powerBanner.alpha = 1f - fly * fly;
            powerBannerAge += frameDt;
        }

        /// <summary>Where the newest chip sits (left of the score's plate), in the sight layer's space (the banner flies there).</summary>
        Vector2 PowerChipPoint()
        {
            if (pointsPlate17 == null) return new Vector2(0, -UnityTuning.Hud17PowerBannerY);
            RectTransform plate = pointsPlate17.rectTransform;
            Vector3 w = plate.TransformPoint(new Vector3(-plate.sizeDelta.x - UnityTuning.Hud17PowerChipGap - UnityTuning.Hud17PowerChip * 0.5f, plate.sizeDelta.y * 0.5f, 0));
            Vector3 l = sight.InverseTransformPoint(w);
            return new Vector2(l.x, l.y);
        }
    }
}
