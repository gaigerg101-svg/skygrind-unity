// NIGHT9 U3-09: the momentum ring (ui/momentumArc.js; OVERNIGHT2 N2, POLISH V5): a thin full ring around the crosshair
// (config.hud.momentumArc radius / width) that fills clockwise from the top with the momentum M along the aim (0..1) and
// takes the style rank's colour above momentum.arcAt (where a melee hit becomes an IMPACT), else hud.momentumArc's
// idleColor; a faint full ring behind it (rgba(255,255,255,0.12), as the web's SVG). Written only when its value
// changes (the web's toFixed(2) step); shown while a run is being played.
using System;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UnityEngine.UI.Image momentumBack, momentumFill;
        double arcAt;
        Color arcIdle;
        float shownArc = -1;
        int shownArcColor = -2; // -1 idle, else the rank
        bool shownArcVisible = true;

        /// <summary>Automation (Play checks): the ring's shown fill (0..1, the web's 2-decimal step) and colour key (-1 idle, else the rank).</summary>
        public float MomentumArcFill => shownArc;
        public int MomentumArcColorKey => shownArcColor;

        void BuildMomentumArc()
        {
            float radius = (float)cfg.Num("hud.momentumArc.radius"), width = (float)cfg.Num("hud.momentumArc.width");
            arcAt = cfg.Num("momentum.arcAt");
            arcIdle = CssColor(cfg.Str("hud.momentumArc.idleColor"));
            Sprite ring = RingSprite(radius, width);
            momentumBack = Img(sight, "Momentum ring (back)", new Color(1, 1, 1, 0.12f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, ring.rect.size);
            momentumBack.sprite = ring;
            momentumFill = Img(sight, "Momentum ring", arcIdle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, ring.rect.size);
            momentumFill.sprite = ring;
            momentumFill.type = UnityEngine.UI.Image.Type.Filled;
            momentumFill.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
            momentumFill.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
            momentumFill.fillClockwise = true;
            momentumFill.fillAmount = 0;
        }

        void RenderMomentumArc()
        {
            if (momentumFill == null) BuildMomentumArc();
            bool visible = game.UiFlow == Flow.Playing && game.loadout.momentum.enabled; // NIGHT14: not over TITLE's attract
            if (visible != shownArcVisible)
            {
                momentumBack.enabled = momentumFill.enabled = visible;
                shownArcVisible = visible;
                if (visible) momentumEnterAge18 = 0;
            }
            if (!visible) return;
            MomentumEnter18();
            double M = game.loadout.AimMomentum();
            float v = (float)(JsMath.Round(Math.Max(0, Math.Min(1, M)) * 100) / 100); // the web's toFixed(2)
            if (v != shownArc)
            {
                momentumFill.fillAmount = v;
                shownArc = v;
            }
            int colorKey = M > arcAt && game.style != null ? game.style.rank : -1;
            if (colorKey != shownArcColor)
            {
                // NIGHT17 U3-30.7: the style meter's cyan value ladder, not the web's rank hues (red-orange / magenta are taken: rule 8)
                momentumFill.color = colorKey >= 0 ? StyleLadderColor(colorKey, game.style.Params.letters.Count) : arcIdle;
                shownArcColor = colorKey;
            }
        }

        // NIGHT18 U3-31.6 (FeelAB on): the ring enters when it shows (grows from 0.6x and fades in: HudEntrance18; both
        // images, their colours' own alphas kept); off: it appears whole, as NIGHT17's.
        double momentumEnterAge18 = double.MaxValue;
        float momentumShownScale18 = 1, momentumBackAlpha18 = -1;

        void MomentumEnter18()
        {
            if (momentumBackAlpha18 < 0) momentumBackAlpha18 = momentumBack.color.a;
            float s = 1, a = 1;
            if (FeelAB.On && !HudEntrance18.Done(momentumEnterAge18))
            {
                momentumEnterAge18 = HudEntrance18.Step(momentumEnterAge18, game.FrameDt);
                s = HudEntrance18.Scale(momentumEnterAge18);
                a = HudEntrance18.Alpha(momentumEnterAge18);
            }
            if (s == momentumShownScale18) return;
            momentumShownScale18 = s;
            var sc = new Vector3(s, s, 1);
            momentumBack.rectTransform.localScale = sc;
            momentumFill.rectTransform.localScale = sc;
            momentumBack.color = UiTheme.A(momentumBack.color, momentumBackAlpha18 * a);
            Color fc = momentumFill.color;
            momentumFill.color = UiTheme.A(fc, a); // the fill's colours are opaque (the ladder, the idle colour)
            if (a >= 1) shownArcColor = -2; // the colour key rewrites the fill's own colour after the entrance
        }

        /// <summary>A full ring of `radius` px, `width` px thick (anti-aliased), white (tinted by the Image).</summary>
        static Sprite RingSprite(float radius, float width)
        {
            int n = (int)Mathf.Ceil((radius + width + 4) * 2);
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Momentum ring" };
            var px = new Color32[n * n];
            float c = n * 0.5f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = x + 0.5f - c, dy = y + 0.5f - c;
                    float off = Mathf.Abs(Mathf.Sqrt(dx * dx + dy * dy) - radius) - width * 0.5f;
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(0.5f - off) * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        /// <summary>A CSS colour as config writes it: "rgba(r, g, b, a)", "rgb(r, g, b)" or "#rrggbb".</summary>
        static Color CssColor(string css)
        {
            css = css.Trim();
            if (css.StartsWith("#") && ColorUtility.TryParseHtmlString(css, out Color hex)) return hex;
            int open = css.IndexOf('('), close = css.LastIndexOf(')');
            if (open < 0 || close < open) return Color.white;
            string[] parts = css.Substring(open + 1, close - open - 1).Split(',');
            float P(int i) => float.Parse(parts[i].Trim(), CultureInfo.InvariantCulture);
            return new Color(P(0) / 255f, P(1) / 255f, P(2) / 255f, parts.Length > 3 ? P(3) : 1);
        }
    }
}
