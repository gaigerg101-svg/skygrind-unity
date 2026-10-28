// NIGHT20 U3-34.9 A2 (builder V): THE HUNT's dusk shift (Research/2026-10-06-additions-backlog.md: the swarm round as CoD's
// dog-round ritual: "the dusk shifts toward DANGER"; CoD Zombies' hellhound rounds turn the sky and fog before the dogs
// come). On E's `specialRound` (SpecialRoundEvent, name "hunt") "announce" or "start", the sky dome, the stone's sky fog and
// the URP fog lean toward DANGER (colors.enemy: red-orange is enemies and danger, CLAUDE.md rule 8) over HuntSkyIn s
// (within the row's 3 s), the horizon most and the zenith least; on its "end", back to the dusk over HuntSkyOut s; a new
// run or TITLE: the dusk at once. Visual only (the sim fires the round); frame time (a slow-mo doesn't slow the sky).
// The colours are written only when the share moves a step (no per-frame churn on a still sky).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The shift's pure rules (HuntSky20Tests).</summary>
    public static class HuntSkyMath
    {
        /// <summary>The ramp (0..1, linear) one frame toward `target` (0 or 1): up over HuntSkyIn s, down over HuntSkyOut s.</summary>
        public static double Step(double u, double target, double dt)
        {
            if (!(dt > 0)) return u;
            if (target > u) return System.Math.Min(target, u + dt / UnityTuning.HuntSkyIn);
            if (target < u) return System.Math.Max(target, u - dt / UnityTuning.HuntSkyOut);
            return u;
        }

        /// <summary>The shift's share for ramp u (a smoothstep: no snap at either end).</summary>
        public static double Share(double u)
        {
            u = u < 0 ? 0 : u > 1 ? 1 : u;
            return u * u * (3 - 2 * u);
        }

        /// <summary>A colour leaned toward `danger` by share x amount (alpha kept).</summary>
        public static Color Lean(Color dusk, Color danger, double share, float amount)
        {
            Color c = Color.Lerp(dusk, danger, (float)share * amount);
            c.a = dusk.a;
            return c;
        }

        /// <summary>The target a SpecialRoundEvent sets: 1 for the hunt's announce / start / last, 0 for its end; -1 not the hunt's.</summary>
        public static int Target(SpecialRoundEvent e)
        {
            if (e == null || e.name != UnityTuning.HuntName) return -1;
            return e.phase == "end" ? 0 : 1;
        }
    }

    /// <summary>The view: the shift on the sky material, the stone's fog globals and RenderSettings.fogColor.</summary>
    public sealed class HuntSky20
    {
        readonly Material sky;
        readonly SurvivalGame game; // NIGHT22: the dusk clock's bases (DuskClock22) when the row is on
        readonly Color zenith, mid, horizon, fog, danger;
        double u, target;
        float shown = -1;
        int shownVersion = -1;
        static readonly int ZenithId = Shader.PropertyToID("_Zenith"), MidId = Shader.PropertyToID("_Mid"), HorizonId = Shader.PropertyToID("_Horizon");
        static readonly int FogZenithId = Shader.PropertyToID("_SkyFogZenith"), FogMidId = Shader.PropertyToID("_SkyFogMid"), FogHorizonId = Shader.PropertyToID("_SkyFogHorizon");

        /// <summary>The shift's share now (the reel's and the HUD's read).</summary>
        public double Share => HuntSkyMath.Share(u);

        public HuntSky20(SurvivalGame game)
        {
            GameConfig cfg = game.cfg;
            this.game = game;
            sky = game.mats != null ? game.mats.sky : null;
            zenith = Look.C(cfg, "colors.skyZenith");
            mid = Look.C(cfg, "colors.skyMid");
            horizon = Look.C(cfg, "colors.skyHorizon");
            fog = Look.C(cfg, "colors.fog");
            danger = Look.C(cfg, "colors.enemy");
            game.events.On(N20Events.SpecialRound, o =>
            {
                int t = HuntSkyMath.Target(o as SpecialRoundEvent);
                if (t >= 0) target = t;
            });
            game.OnNewRun += Reset;
        }

        /// <summary>The dusk at once (a new run, TITLE).</summary>
        public void Reset()
        {
            u = target = 0;
            Apply(0);
        }

        /// <summary>Frame (frame time).</summary>
        public void Render(double frameDt)
        {
            u = HuntSkyMath.Step(u, target, frameDt);
            Apply(HuntSkyMath.Share(u));
        }

        void Apply(double share)
        {
            float step = Mathf.Round((float)share * 255) / 255;
            // NIGHT22: with the DUSK CLOCK on, the shift leans from the clock's current bases, not the shipped dusk; a base that moved
            // (DuskClock22.Version) re-applies even when the hunt's share stood still.
            bool clock = DuskClock22.Instance != null && game.sim != null && game.sim.n22.duskClock;
            int ver = clock ? DuskClock22.Version : 0;
            if (step == shown && ver == shownVersion) return;
            shown = step;
            shownVersion = ver;
            Color zb = clock ? DuskClock22.Instance.Zenith : zenith, mb = clock ? DuskClock22.Instance.Mid : mid;
            Color hb = clock ? DuskClock22.Instance.Horizon : horizon, fb = clock ? DuskClock22.Instance.Fog : fog;
            Color z = HuntSkyMath.Lean(zb, danger, step, UnityTuning.HuntZenith);
            Color m = HuntSkyMath.Lean(mb, danger, step, UnityTuning.HuntMid);
            Color h = HuntSkyMath.Lean(hb, danger, step, UnityTuning.HuntHorizon);
            if (sky != null)
            {
                sky.SetColor(ZenithId, z);
                sky.SetColor(MidId, m);
                sky.SetColor(HorizonId, h);
            }
            Shader.SetGlobalColor(FogZenithId, z);
            Shader.SetGlobalColor(FogMidId, m);
            Shader.SetGlobalColor(FogHorizonId, h);
            RenderSettings.fogColor = HuntSkyMath.Lean(fb, danger, step, UnityTuning.HuntFog);
        }
    }
}
