// NIGHT17 U3-30.10 (beyond the ask; the lead's): the world answers the style rank (the web's main.js updateRankWorld:
// the neon brightens and a pulse runs along the trims as the rank climbs). The map's neon (SurvivalBuilder's "Neon": the
// ride strips, the rims, the nodes) climbs the cyan VALUE ladder with the rank (IDENTITY.md: never a hue ladder; the web's
// S+ horizon warming is left out, D-70): brighter (HDR, so the bloom catches it) and pulsing faster. REDUCED MOTION keeps
// the brightness and drops the pulse. A picture only: it reads the style meter, writes one MaterialPropertyBlock on one
// renderer, and only when the colour moved (no allocation per frame).
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class RankWorld
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        readonly SurvivalGame game;
        readonly Renderer neon;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color baseColor;
        double phase, shownLevel;
        Color shown = new Color(-1, -1, -1, -1);

        public RankWorld(SurvivalGame game, Transform mapRoot)
        {
            this.game = game;
            Transform t = mapRoot != null ? mapRoot.Find("Neon") : null;
            neon = t != null ? t.GetComponent<Renderer>() : null;
            baseColor = neon != null && neon.sharedMaterial != null && neon.sharedMaterial.HasProperty(BaseColorId) ? neon.sharedMaterial.GetColor(BaseColorId) : Color.white;
        }

        /// <summary>The rank's share of the ladder: 0 at the bottom rank, 1 at the top.</summary>
        public static double LevelOf(int rank, int ranks) => ranks <= 1 ? 0 : Math.Max(0, Math.Min(1, rank / (double)(ranks - 1)));

        public void Render(double frameDt)
        {
            if (neon == null) return;
            StyleMeter st = game.style;
            double target = 0;
            if (st != null && game.flow == Flow.Playing) target = LevelOf(st.rank, st.Params.colors.Count);
            // ease toward the rank's level (a rank up lands fast, a fall drains slower)
            double rate = target > shownLevel ? UnityTuning.RankWorldRiseRate : UnityTuning.RankWorldFallRate;
            shownLevel += (target - shownLevel) * Math.Min(1, rate * frameDt);
            bool calm = game.cfg.movement.settings.reducedMotion;
            double hz = UnityTuning.RankWorldPulseHzLow + (UnityTuning.RankWorldPulseHzHigh - UnityTuning.RankWorldPulseHzLow) * shownLevel;
            phase = (phase + frameDt * hz) % 1.0;
            double pulse = calm ? 0 : Math.Sin(phase * 2 * Math.PI) * UnityTuning.RankWorldPulseAmp * shownLevel;
            float gain = (float)(1 + UnityTuning.RankWorldGain * shownLevel + pulse);
            // the value ladder: the base neon toward CYAN-HOT with the level, then x gain (HDR over 1)
            Color c = Color.Lerp(baseColor, UiTheme.Ready ? UiTheme.CyanHot : baseColor, (float)(shownLevel * UnityTuning.RankWorldHotMix)) * gain;
            c.a = baseColor.a;
            if (Mathf.Abs(c.r - shown.r) + Mathf.Abs(c.g - shown.g) + Mathf.Abs(c.b - shown.b) < UnityTuning.RankWorldEpsilon) return;
            shown = c;
            block.SetColor(BaseColorId, c);
            neon.SetPropertyBlock(block);
        }
    }
}
