// NIGHT22 U3-36.8 THE DUSK CLOCK (the lead): the view. The sky dome's three colours, the stone's sky-fog globals and the URP fog
// lean from the dusk toward the config's round-15 colours (design.n22.duskClock: violet and blue-violet) by the round
// (Core/DuskClock22.cs). Render only: no sim state, the shipped sky material and fog globals only (no new shader or material),
// frame time. THE HUNT's shift (HuntSky20) leans from THESE bases while it runs (DuskClock22.Version tells it to re-apply), so
// the two compose. Rule 8: the colours are violets; never magenta, never red-orange. Off (the row, --off22): nothing here is built.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class DuskClock22
    {
        readonly SurvivalGame game;
        readonly Material sky;
        readonly Color zenith0, mid0, horizon0, fog0, zenith1, mid1, horizon1, fog1;
        readonly int lastRound;
        readonly float lightScale, ambientScale;
        Light key;
        float keyBase = -1;
        Color ambSky0, ambEq0, ambGround0;
        bool ambCaptured;
        readonly double ease;
        double shown, target;
        float stepShown = -1;

        static readonly int ZenithId = Shader.PropertyToID("_Zenith"), MidId = Shader.PropertyToID("_Mid"), HorizonId = Shader.PropertyToID("_Horizon");
        static readonly int FogZenithId = Shader.PropertyToID("_SkyFogZenith"), FogMidId = Shader.PropertyToID("_SkyFogMid"), FogHorizonId = Shader.PropertyToID("_SkyFogHorizon");

        /// <summary>The current base colours (HuntSky20 leans from these while the clock runs); null before the clock is built.</summary>
        public static DuskClock22 Instance;

        /// <summary>Bumped whenever the bases moved (HuntSky20 re-applies when it changes).</summary>
        public static int Version;

        public Color Zenith { get; private set; }
        public Color Mid { get; private set; }
        public Color Horizon { get; private set; }
        public Color Fog { get; private set; }

        /// <summary>The shown share now (the reel's and the HUD's read).</summary>
        public double Share => shown;

        public DuskClock22(SurvivalGame g)
        {
            game = g;
            GameConfig cfg = g.cfg;
            sky = g.mats != null ? g.mats.sky : null;
            zenith0 = Look.C(cfg, "colors.skyZenith");
            mid0 = Look.C(cfg, "colors.skyMid");
            horizon0 = Look.C(cfg, "colors.skyHorizon");
            fog0 = Look.C(cfg, "colors.fog");
            zenith1 = Look.C(cfg, "design.n22.duskClock.zenith");
            mid1 = Look.C(cfg, "design.n22.duskClock.mid");
            horizon1 = Look.C(cfg, "design.n22.duskClock.horizon");
            fog1 = Look.C(cfg, "design.n22.duskClock.fog");
            lastRound = (int)cfg.Num("design.n22.duskClock.lastRound");
            ease = cfg.Num("design.n22.duskClock.easeSeconds");
            lightScale = (float)cfg.Num("design.n22.duskClock.lightScale");
            ambientScale = (float)cfg.Num("design.n22.duskClock.ambientScale");
            Instance = this;
            Zenith = zenith0; Mid = mid0; Horizon = horizon0; Fog = fog0;
            g.OnNewRun += Reset;
        }

        /// <summary>The dusk as shipped at once (a new run, TITLE).</summary>
        public void Reset()
        {
            shown = target = 0;
            stepShown = -1;
            Apply(0, true);
        }

        /// <summary>Every frame (frame time; a slow-mo doesn't slow the sky).</summary>
        public void Render(double frameDt, bool title, int round)
        {
            if (!game.sim.n22.duskClock) return;
            if (title) { if (shown > 0 || target > 0) Reset(); return; }
            target = DuskClockMath.Target(round, lastRound);
            shown = DuskClockMath.Step(shown, target, frameDt, ease);
            Apply(shown, false);
        }

        void Apply(double share, bool force)
        {
            float step = Mathf.Round((float)share * 255) / 255;
            if (step == stepShown && !force) return;
            stepShown = step;
            Zenith = Color.Lerp(zenith0, zenith1, step);
            Mid = Color.Lerp(mid0, mid1, step);
            Horizon = Color.Lerp(horizon0, horizon1, step);
            Fog = Color.Lerp(fog0, fog1, step);
            Version++;
            ApplyLight(step);
            if (game.HuntSkyShare > 0) return; // the HUNT's shift is leaning from these bases and writes the sky itself
            if (sky != null)
            {
                sky.SetColor(ZenithId, Zenith);
                sky.SetColor(MidId, Mid);
                sky.SetColor(HorizonId, Horizon);
            }
            Shader.SetGlobalColor(FogZenithId, Zenith);
            Shader.SetGlobalColor(FogMidId, Mid);
            Shader.SetGlobalColor(FogHorizonId, Horizon);
            RenderSettings.fogColor = Fog;
        }

        /// <summary>The key light and the hemisphere ambient sink with the round (their shipped values are captured once, at the first
        /// apply: Look.Build has run by then). The share 0 puts back exactly what was captured.</summary>
        void ApplyLight(float share)
        {
            if (key == null) key = RenderSettings.sun;
            if (key != null)
            {
                if (keyBase < 0) keyBase = key.intensity;
                key.intensity = keyBase * Mathf.Lerp(1f, lightScale, share);
            }
            if (!ambCaptured)
            {
                ambSky0 = RenderSettings.ambientSkyColor;
                ambEq0 = RenderSettings.ambientEquatorColor;
                ambGround0 = RenderSettings.ambientGroundColor;
                ambCaptured = true;
            }
            float a = Mathf.Lerp(1f, ambientScale, share);
            RenderSettings.ambientSkyColor = ambSky0 * a;
            RenderSettings.ambientEquatorColor = ambEq0 * a;
            RenderSettings.ambientGroundColor = ambGround0 * a;
        }
    }
}
