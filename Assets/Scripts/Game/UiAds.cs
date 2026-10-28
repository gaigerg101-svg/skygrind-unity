// NIGHT16 polish #36 (builder V): the ADS HUD (POLISH V2, a web port the port never had): hud.js :74-82 (the CSS) and setAds
// :752-770, main.js updateAdsHud :2144-2167.
// - The fine ring (.hud-adsring): a 30 px circle, a 1 px border in the crosshair's colour and a faint dark outline, opacity k
//   for a 'ring' gun (0 under a scope), scale 1.6 - 0.6 x that opacity.
// - The LONGWAVE's scope (.hud-scope): full screen, a radial gradient (clear to 31vh, rgba(4,4,10,0.55) at 33vh,
//   ads.scopeVignette at 45vh and beyond: a 90 vh square texture and four solid panels around it) and two 1 px lines
//   62 vh long crossing at the centre; opacity k for a 'scope' gun.
// - The crosshair's arms fade to 1 - 0.6 k (--tick; Ui.cs multiplies the arms' alpha by adsTickAlpha).
// - The rangefinder (.hud-range, "%.1f M", 24 px under the centre) while scoped, k > 0.5 and a hit: the camera's ray along
//   its look to the first wall (GameWorld.RayIntersect, the web's arena.octree.rayIntersect) or body (EnemyManager.Raycast
//   within weapons.rail.range, the web's enemies.raycast): both read-only, no allocation (a nullable struct, a List
//   enumerator), as WeaponFx and the viewmodel's wall pullback already call them from the view.
// k is the viewmodel's eased zoom (Viewmodel.AdsAmount: frame time, frameDt / the gun's class ease), the amount the FOV is
// drawn with, so the ring and the zoom move together. Frame time. Hidden unless playing (UiFlow: not the attract).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>What setAds draws this frame (hud.js :756-770).</summary>
    public struct UiAdsLook
    {
        public double ringAlpha, ringScale, scopeAlpha, tickAlpha;
        public bool showRange;
    }

    /// <summary>The ADS HUD's pure maths (hud.js setAds; main.js updateAdsHud's gate).</summary>
    public static class UiAdsMath
    {
        /// <summary>
        /// main.js :2148-2166 + hud.js :756-770: `amount` the zoom's eased amount, `zoomable` the gun has an ads.weapons
        /// entry, `scope` its reticle is 'scope', `shown` playing (no override), `hasRange` the rangefinder hit something.
        /// </summary>
        public static UiAdsLook Look(double amount, bool zoomable, bool scope, bool shown, bool hasRange)
        {
            // main.js :2150: if (!spec || ads.amount <= 0 || flow.state !== 'playing' || ...) hud.setAds(null)
            double k = zoomable && shown && amount > 0 ? Math.Min(1, amount) : 0;
            if (k <= 0) scope = false;
            double ring = Fixed2(scope ? 0 : k); // write('adsRing', (scope ? 0 : k).toFixed(2), ...)
            return new UiAdsLook
            {
                ringAlpha = ring,
                ringScale = UnityTuning.AdsRingScaleFrom - UnityTuning.AdsRingScaleBy * ring, // scale(1.6 - 0.6 v)
                scopeAlpha = Fixed2(scope ? k : 0), // write('adsScope', (scope ? k : 0).toFixed(2), ...)
                tickAlpha = Fixed2(1 - UnityTuning.AdsTickFade * k), // write('adsTicks', (1 - 0.6 * k).toFixed(2), ...)
                showRange = scope && k > UnityTuning.AdsRangeFromAmount && hasRange, // scope && k > 0.5 && state.range !== null
            };
        }

        /// <summary>The scope's radial gradient alpha at `rVh` vh from the centre (hud.js :77).</summary>
        public static double VignetteAlpha(double rVh, double scopeVignette)
        {
            if (rVh <= UnityTuning.AdsScopeClearVh) return 0;
            if (rVh <= UnityTuning.AdsScopeEdgeVh)
                return UnityTuning.AdsScopeEdgeAlpha * (rVh - UnityTuning.AdsScopeClearVh) / (UnityTuning.AdsScopeEdgeVh - UnityTuning.AdsScopeClearVh);
            if (rVh <= UnityTuning.AdsScopeFullVh)
                return UnityTuning.AdsScopeEdgeAlpha + (scopeVignette - UnityTuning.AdsScopeEdgeAlpha) * (rVh - UnityTuning.AdsScopeEdgeVh) / (UnityTuning.AdsScopeFullVh - UnityTuning.AdsScopeEdgeVh);
            return scopeVignette;
        }

        /// <summary>The range in tenths of a metre as the label shows it (toFixed(1)); the label changes only when it does.</summary>
        public static int RangeTenths(double metres) => (int)Math.Floor(metres * 10 + 0.5);

        /// <summary>Coverage (0..1) of the annulus rIn..rOut over the texel at (x, y) from the centre (4 x 4 samples).</summary>
        public static double RingCoverage(double x, double y, double rIn, double rOut)
        {
            int inside = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    double sx = x + (i + 0.5) / 4 - 0.5, sy = y + (j + 0.5) / 4 - 0.5;
                    double r = Math.Sqrt(sx * sx + sy * sy);
                    if (r >= rIn && r < rOut) inside++;
                }
            return inside / 16.0;
        }

        static double Fixed2(double v) => Math.Round(v * 100, MidpointRounding.AwayFromZero) / 100; // toFixed(2)
    }

    public sealed partial class Ui
    {
        RectTransform adsScopeRoot, adsRingRoot;
        CanvasGroup adsScopeGroup, adsRingGroup;
        RawImage adsRingLine;
        TextMeshProUGUI adsRange;
        double adsVignette, adsRailRange;
        int adsRangeShown = -1;
        double adsRingShown = -1, adsScopeShown = -1;
        Color adsRingColor;
        /// <summary>hud.js --tick: the crosshair arms' alpha (Ui.cs multiplies the arms' colour by it; 1 unzoomed).</summary>
        float adsTickAlpha = 1;
        readonly Dictionary<string, int> adsSpecs = new Dictionary<string, int>(); // weapon -> 0 none, 1 ring, 2 scope (read once per id)

        void BuildAds()
        {
            adsVignette = cfg.Num("ads.scopeVignette");
            adsRailRange = cfg.Num("weapons.rail.range");
            float vh = (float)cfg.Num("ui.theme.scaleBaseHeight") / 100;

            // .hud-scope: a 90 vh square carrying the gradient, four panels at the full vignette around it, the two lines
            adsScopeRoot = new GameObject("ADS scope", typeof(RectTransform), typeof(CanvasGroup)).GetComponent<RectTransform>();
            adsScopeRoot.SetParent(sight, false);
            adsScopeRoot.anchorMin = Vector2.zero;
            adsScopeRoot.anchorMax = Vector2.one;
            adsScopeRoot.offsetMin = adsScopeRoot.offsetMax = Vector2.zero;
            adsScopeRoot.SetAsFirstSibling(); // under the crosshair
            adsScopeGroup = adsScopeRoot.GetComponent<CanvasGroup>();
            adsScopeGroup.alpha = 0;
            adsScopeGroup.blocksRaycasts = false;
            adsScopeGroup.interactable = false;
            float half = (float)UnityTuning.AdsScopeFullVh * vh;
            int n = UnityTuning.AdsScopeTexture;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "ADS scope" };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    double dx = (x + 0.5) / n * 2 - 1, dy = (y + 0.5) / n * 2 - 1;
                    double rVh = Math.Sqrt(dx * dx + dy * dy) * UnityTuning.AdsScopeFullVh;
                    px[y * n + x] = new Color32(UnityTuning.AdsScopeR, UnityTuning.AdsScopeG, UnityTuning.AdsScopeB, (byte)Math.Round(255 * UiAdsMath.VignetteAlpha(rVh, adsVignette)));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            AdsRaw(adsScopeRoot, "Scope gradient", tex, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2 * half, 2 * half));
            var edge = new Color32(UnityTuning.AdsScopeR, UnityTuning.AdsScopeG, UnityTuning.AdsScopeB, (byte)Math.Round(255 * adsVignette));
            AdsPanel("Scope left", edge, new Vector2(0, 0), new Vector2(0.5f, 1), Vector2.zero, new Vector2(-half, 0));
            AdsPanel("Scope right", edge, new Vector2(0.5f, 0), new Vector2(1, 1), new Vector2(half, 0), Vector2.zero);
            AdsPanel("Scope top", edge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1), new Vector2(-half, half), new Vector2(half, 0));
            AdsPanel("Scope bottom", edge, new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(-half, 0), new Vector2(half, -half));
            var line = new Color(1, 1, 1, UnityTuning.AdsScopeLineAlpha);
            float len = (float)UnityTuning.AdsScopeLineVh * vh;
            Img(adsScopeRoot, "Scope line h", line, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(len, UnityTuning.AdsScopeLineWidth));
            Img(adsScopeRoot, "Scope line v", line, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.AdsScopeLineWidth, len));

            // .hud-adsring: the dark outline and the border, each a 1 px antialiased ring texture (one texel a canvas px)
            adsRingRoot = new GameObject("ADS ring", typeof(RectTransform), typeof(CanvasGroup)).GetComponent<RectTransform>();
            adsRingRoot.SetParent(sight, false);
            adsRingRoot.anchorMin = adsRingRoot.anchorMax = new Vector2(0.5f, 0.5f);
            adsRingRoot.sizeDelta = Vector2.zero;
            adsRingGroup = adsRingRoot.GetComponent<CanvasGroup>();
            adsRingGroup.alpha = 0;
            adsRingGroup.blocksRaycasts = false;
            adsRingGroup.interactable = false;
            float rIn = UnityTuning.AdsRingDiameter / 2, rOut = rIn + UnityTuning.AdsRingBorder, rShadow = rOut + UnityTuning.AdsRingOutline;
            int side = (int)Math.Ceiling(2 * rShadow) + 2;
            AdsRaw(adsRingRoot, "Ring outline", AdsRingTexture(side, rOut, rShadow), new Color(0, 0, 0, UnityTuning.AdsRingOutlineAlpha), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(side, side));
            adsRingLine = AdsRaw(adsRingRoot, "Ring", AdsRingTexture(side, rIn, rOut), crossColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(side, side));
            adsRingColor = crossColor;

            // .hud-range
            adsRange = Text(sight, "ADS range", "", tBody, Look.Hex(UnityTuning.AdsRangeHex), TextAlignmentOptions.Top, new Vector2(0, -UnityTuning.AdsRangeTop), new Vector2(UnityTuning.AdsRangeBoxWidth, tBody * 1.5f), new Vector2(0.5f, 0.5f));
            adsRange.rectTransform.pivot = new Vector2(0.5f, 1); // its top edge 24 px under the centre
            adsRange.characterSpacing = UnityTuning.AdsRangeSpacingEm * 100; // TMP: em / 100
            adsRange.gameObject.SetActive(false);
        }

        static Texture2D AdsRingTexture(int side, float rIn, float rOut)
        {
            var tex = new Texture2D(side, side, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "ADS ring" };
            var px = new Color32[side * side];
            double c = side / 2.0;
            for (int y = 0; y < side; y++)
                for (int x = 0; x < side; x++)
                    px[y * side + x] = new Color32(255, 255, 255, (byte)Math.Round(255 * UiAdsMath.RingCoverage(x + 0.5 - c, y + 0.5 - c, rIn, rOut)));
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        static RawImage AdsRaw(Transform parent, string name, Texture tex, Color c, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<RawImage>();
            img.texture = tex;
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        void AdsPanel(string name, Color c, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
        {
            Image img = Img(adsScopeRoot, name, c, aMin, aMax, Vector2.zero, Vector2.zero);
            img.rectTransform.offsetMin = offMin;
            img.rectTransform.offsetMax = offMax;
        }

        /// <summary>The gun's reticle (ads.weapons.<id>: 0 none, 1 'ring', 2 'scope'), read once per id.</summary>
        int AdsSpec(string weapon)
        {
            if (string.IsNullOrEmpty(weapon)) return 0;
            if (adsSpecs.TryGetValue(weapon, out int s)) return s;
            string p = "ads.weapons." + weapon;
            // main.js: adsHud.reticle = spec.reticle (the scope's look); the rangefinder runs `if (spec.scope)`: the LONGWAVE has both
            s = !cfg.Has(p) ? 0 : cfg.Has(p + ".reticle") && cfg.Str(p + ".reticle") == "scope" ? 2 : 1;
            adsSpecs[weapon] = s;
            return s;
        }

        /// <summary>main.js updateAdsHud + hud.js setAds (frame time).</summary>
        void RenderAds(double frameDt)
        {
            if (adsRingRoot == null) return;
            Viewmodel vm = game.viewmodel;
            int spec = AdsSpec(game.loadout != null ? game.loadout.Weapon : null);
            double amount = vm != null ? vm.AdsAmount : 0;
            bool shown = game.UiFlow == Flow.Playing && game.Active;
            bool scope = spec == 2;
            bool hasRange = false;
            double range = 0;
            if (shown && scope && amount > 0)
            {
                // main.js :2158-2163: the camera's ray to the first wall or body (read-only; no allocation)
                Transform c = game.cam.transform;
                Vec3 o = WebSpace.FromUnity(c.position), d = WebSpace.FromUnity(c.forward);
                double best = double.PositiveInfinity;
                if (game.world.RayIntersect(o, d, out RayHit wall)) best = wall.distance;
                EnemyRayHit? body = game.enemies.Raycast(o, d, adsRailRange);
                if (body.HasValue && body.Value.distance < best) best = body.Value.distance;
                hasRange = !double.IsInfinity(best);
                range = best;
            }
            UiAdsLook l = UiAdsMath.Look(amount, spec != 0, scope, shown, hasRange);
            if (l.ringAlpha != adsRingShown)
            {
                adsRingShown = l.ringAlpha;
                adsRingGroup.alpha = (float)l.ringAlpha;
                adsRingRoot.localScale = Vector3.one * (float)l.ringScale;
            }
            if (adsRingColor != crossColor)
            {
                adsRingColor = crossColor; // var(--cross): the crosshair colour setting
                adsRingLine.color = crossColor;
            }
            if (l.scopeAlpha != adsScopeShown)
            {
                adsScopeShown = l.scopeAlpha;
                adsScopeGroup.alpha = (float)l.scopeAlpha;
            }
            adsTickAlpha = (float)l.tickAlpha;
            int tenths = l.showRange ? UiAdsMath.RangeTenths(range) : -1;
            if (tenths != adsRangeShown)
            {
                adsRangeShown = tenths;
                bool on = tenths >= 0;
                if (adsRange.gameObject.activeSelf != on) adsRange.gameObject.SetActive(on);
                if (on) adsRange.SetText("{0:1} M", tenths / 10f); // "%.1f M", only when the tenths change
            }
        }
    }
}
