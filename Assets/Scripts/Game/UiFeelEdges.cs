// NIGHT16 U3-29.8 polish #32-#34 (web ports of hud.js cues the port never had):
// - #32 EXPOSED's pulsing edge (hud.js .hud-exposed-edge :219 / :290 / :694-695; SURVIVAL S5): while the director calls
//   the player EXPOSED (camping), an inset red-orange edge (box-shadow: inset 0 0 90px 30px rgba(255, 40, 10, 0.85)) at
//   opacity 0.35 + 0.35 sin(7 t), t the frame time exposed (0 when not). The shadow is a 9-sliced sprite: an inset box
//   shadow is a Gaussian (sigma = blur / 2) over the inner rect inset by the spread, so its alpha at depths (dx, dy) from
//   the edges is 0.85 (1 - F(dx) F(dy)), F(d) = Phi((d - spread) / sigma).
// - #33 the buy prompt's poor and denied states (hud.js .hud-buy.poor / .denied :237-240 / :688-689): poor (not owned,
//   not affordable) tints the prompt #ff9a82 and its hold bar's back danger (the web's border-bottom); denied (the
//   buyables' deniedSteps, buy.deniedFlashTime) shakes it right by hud-deny (0.1 s steps(2), 4 times: 0, 2, 4, 2 px each
//   25 ms), from the frame it turns on.
// - #34 regen (hud.js .hud-hp-fill.regen / .hud-regen-tag :121-124 / :134 / :728-731): while the HP regenerates, a pale
//   sheen sweeps the HP fill (the web's gradient, text -> #bff8ff at 42 % -> text at 72 %, 220 % wide, slid from 100 %
//   to -120 % every 0.8 s) and a REGEN tag shows by the HP (the web's sits in the speed row the port doesn't have). The
//   web's low-HP variant (#ffb49c over a danger fill) is left: the port's low-HP fill blinks white / cyan, not danger.
// The CSS literals are UnityTuning.FeelPolish.cs's. No allocation per frame.
using System;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UnityEngine.UI.Image exposedEdge;
        Color exposedEdgeColor;
        double exposedT;
        float shownExposedEdge = -1;

        UnityEngine.UI.Image promptBarBack;
        Color promptBarBackColor, poorColor;
        Vector2 promptHome;
        bool shownPoor, wasDenied;
        double denyAge = double.MaxValue;
        float shownDenyX;

        UnityEngine.UI.RawImage regenSheen;
        TextMeshProUGUI regenTag;
        bool shownRegen;
        double regenT;
        double regenEnterAge18 = double.MaxValue; // NIGHT18 U3-31.6: the REGEN tag's and sheen's entrance
        float regenShownScale18 = 1;

        void BuildFeelEdges(UnityEngine.UI.Image promptBack, RectTransform hpValues)
        {
            // #32
            exposedEdgeColor = Look.Hex(UnityTuning.ExposedEdgeHex);
            exposedEdge = Img(values, "Exposed edge", exposedEdgeColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            exposedEdge.sprite = InsetShadowSprite(UnityTuning.ExposedEdgeBlur, UnityTuning.ExposedEdgeSpread, UnityTuning.ExposedEdgeAlpha);
            exposedEdge.type = UnityEngine.UI.Image.Type.Sliced;
            exposedEdge.color = new Color(exposedEdgeColor.r, exposedEdgeColor.g, exposedEdgeColor.b, 0);
            exposedEdge.transform.SetSiblingIndex(vignette.transform.GetSiblingIndex() + 1); // the web's DOM: after the vignettes
            // #33
            promptBarBack = promptBack;
            promptBarBackColor = promptBack.color;
            poorColor = Look.Hex(UnityTuning.BuyPoorTextHex);
            promptHome = prompt.rectTransform.anchoredPosition;
            // #34: the sheen over the HP fill (stretched with it), and the tag at the HP plate's top right
            regenSheen = new GameObject("Regen sheen", typeof(RectTransform), typeof(UnityEngine.UI.RawImage)).GetComponent<UnityEngine.UI.RawImage>();
            regenSheen.rectTransform.SetParent(hpFill.transform, false);
            regenSheen.rectTransform.anchorMin = Vector2.zero;
            regenSheen.rectTransform.anchorMax = Vector2.one;
            regenSheen.rectTransform.offsetMin = regenSheen.rectTransform.offsetMax = Vector2.zero;
            regenSheen.texture = SheenTexture();
            regenSheen.raycastTarget = false;
            regenSheen.color = Look.Hex(UnityTuning.RegenSheenHex);
            regenSheen.uvRect = new Rect(0, 0, 1 / UnityTuning.RegenSheenSize, 1);
            regenSheen.enabled = false;
            regenTag = Text(hpValues, "Regen tag", "REGEN", tSmall, Look.Hex(UnityTuning.RegenTagHex), TextAlignmentOptions.Right, new Vector2(-16, -14), new Vector2(160, 22), new Vector2(1, 1));
            regenTag.characterSpacing = UnityTuning.RegenTagSpacing;
            regenTag.gameObject.SetActive(false);
        }

        /// <summary>The three cues' frame (frame time), from Render after the HP and the buy prompt.</summary>
        void RenderFeelEdges(double frameDt, bool exposedNow, bool promptOn, bool poor)
        {
            // #32
            exposedT = exposedNow ? exposedT + frameDt : 0;
            float ea = exposedNow ? (float)Math.Round(UnityTuning.ExposedEdgeBase + UnityTuning.ExposedEdgeSwing * Math.Sin(exposedT * UnityTuning.ExposedEdgeRate), 2) : 0;
            // NIGHT18 U3-31.6 (FeelAB on): the edge swells in over Hud18FlashAttack instead of starting whole at 0.35 (an M9
            // pop-in); off: x 1
            if (exposedNow && FeelAB.On) ea = (float)Math.Round(ea * FlashAttack18(exposedT), 2);
            if (ea != shownExposedEdge)
            {
                shownExposedEdge = ea;
                exposedEdge.color = new Color(exposedEdgeColor.r, exposedEdgeColor.g, exposedEdgeColor.b, ea);
            }
            // #33
            bool poorNow = promptOn && poor;
            if (poorNow != shownPoor)
            {
                shownPoor = poorNow;
                prompt.color = poorNow ? poorColor : text;
                promptBarBack.color = poorNow ? new Color(danger.r, danger.g, danger.b, UnityTuning.BuyPoorBarAlpha) : promptBarBackColor;
            }
            bool denied = promptOn && game.buyables.Denied;
            if (denied && !wasDenied) denyAge = 0; // the class goes on: the animation starts
            wasDenied = denied;
            float dx = 0;
            if (denyAge < UnityTuning.BuyDenyPeriod * UnityTuning.BuyDenyRepeats)
            {
                denyAge += frameDt;
                int quarter = (int)(denyAge / (UnityTuning.BuyDenyPeriod / 4)) % 4; // steps(2) on each half of 0 -> 4 px -> 0
                dx = quarter == 1 || quarter == 3 ? UnityTuning.BuyDenyShift / 2 : quarter == 2 ? UnityTuning.BuyDenyShift : 0;
            }
            if (dx != shownDenyX)
            {
                shownDenyX = dx;
                prompt.rectTransform.anchoredPosition = promptHome + new Vector2(dx, 0);
            }
            // #34
            bool regenNow = game.regen != null && game.regen.Regenerating;
            if (regenNow != shownRegen)
            {
                shownRegen = regenNow;
                regenSheen.enabled = regenNow;
                regenTag.gameObject.SetActive(regenNow);
                regenT = 0;
                regenEnterAge18 = 0;
            }
            if (regenNow)
            {
                // NIGHT18 U3-31.6 (FeelAB on): the tag and the sheen enter (the tag grows from 0.6x; both fade in)
                float rs = 1, ra = 1;
                if (FeelAB.On && !HudEntrance18.Done(regenEnterAge18))
                {
                    regenEnterAge18 = HudEntrance18.Step(regenEnterAge18, frameDt);
                    rs = HudEntrance18.Scale(regenEnterAge18);
                    ra = HudEntrance18.Alpha(regenEnterAge18);
                }
                if (rs != regenShownScale18)
                {
                    regenShownScale18 = rs;
                    regenTag.rectTransform.localScale = new Vector3(rs, rs, 1);
                    regenTag.alpha = ra;
                    Color sc = regenSheen.color;
                    sc.a = ra;
                    regenSheen.color = sc;
                }
                regenT = (regenT + frameDt) % UnityTuning.RegenSheenPeriod;
                double p = regenT / UnityTuning.RegenSheenPeriod; // background-position 100 % -> -120 %
                double offset = -(UnityTuning.RegenSheenSize - 1) * (1 - UnityTuning.RegenSheenTravel * p); // the image's left edge, in fill widths
                regenSheen.uvRect = new Rect((float)(-offset / UnityTuning.RegenSheenSize), 0, 1 / UnityTuning.RegenSheenSize, 1);
            }
        }

        /// <summary>A 9-sliced inset box shadow (white; the Image's colour tints it): border = spread + blur px.</summary>
        static Sprite InsetShadowSprite(float blur, float spread, float alpha)
        {
            int b = Mathf.CeilToInt(spread + blur), n = 2 * b + 4;
            double sigma = blur / 2.0;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Inset shadow" };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            {
                double dy = Math.Min(y + 0.5, n - y - 0.5);
                double fy = Phi((dy - spread) / sigma);
                for (int x = 0; x < n; x++)
                {
                    double dx = Math.Min(x + 0.5, n - x - 0.5);
                    double a = alpha * (1 - Phi((dx - spread) / sigma) * fy);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)Math.Round(Math.Max(0, Math.Min(1, a)) * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }

        /// <summary>The standard normal CDF (Abramowitz and Stegun 7.1.26 for erf: |error| under 1.5e-7).</summary>
        static double Phi(double z)
        {
            double x = Math.Abs(z) / Math.Sqrt(2), t = 1 / (1 + 0.3275911 * x);
            double erf = 1 - ((((1.061405429 * t - 1.453152027) * t + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t * Math.Exp(-x * x);
            return z >= 0 ? 0.5 * (1 + erf) : 0.5 * (1 - erf);
        }

        /// <summary>One tile of the regen gradient as an alpha band (the tint is the RawImage's colour): 0 at 0 %, 1 at 42 %,
        /// 0 from 72 % (the web's stops; the fill's own colour shows where the band is clear).</summary>
        static Texture2D SheenTexture()
        {
            const int n = 256;
            var tex = new Texture2D(n, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, name = "Regen sheen" };
            var px = new Color32[n];
            for (int i = 0; i < n; i++)
            {
                float u = (i + 0.5f) / n;
                float a = u < UnityTuning.RegenSheenPeak ? u / UnityTuning.RegenSheenPeak : u < UnityTuning.RegenSheenEnd ? 1 - (u - UnityTuning.RegenSheenPeak) / (UnityTuning.RegenSheenEnd - UnityTuning.RegenSheenPeak) : 0;
                px[i] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255));
            }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }
    }
}
