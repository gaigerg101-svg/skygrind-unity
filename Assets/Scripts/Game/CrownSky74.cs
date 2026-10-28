// NIGHT74 builder B (leftover 2, round 2 of design.n74.crownroom74; CrownRoom74.cs has the walls and the reasons): THE CROWN's SKY. The lead's n74-t4 meter on the idle pair read the wall fix at violet + magenta 18.8 -> 11.0 %,
// short of the bar: the violet and magenta that stay (the top-right plane, 85 % of its cell, hue 265; the band under it, hue 305-330) are not stone. They are the dusk SKY seen through the spire's open top (Sky.shader:
// zenith navy 0x100A2C, mid violet 0x4A2168, the horizon's orange 0xFF7A48 mixed up through magenta), so no wall multiplier reaches them. While THE CROWN's fight is live (its BossAttackClock63 read: any phase from the
// reveal to the death) and the row crownroom74 reads on (every frame: NightRows.ViewOn), the sky is drawn in THE CROWN's colours, UnityTuning.N74CrownSky*: a slate zenith and mid and an ivory horizon, "ivory and gold at height over slate".
// HOW (render only; nothing is left changed): Look.Apply, Bright71, DuskClock22, HuntSky20, PostFx and the grades all write the sky material (_Zenith / _Mid / _Horizon) and the stone's fog globals (_SkyFogZenith /
// _SkyFogMid / _SkyFogHorizon, which Stone.shader fades distant stone into) from their own places; a seventh writer would race them. So this one writes at RENDER time instead: RenderPipelineManager.beginCameraRendering
// for the game's main camera sets the nine colours (the sky's own, and the globals, the target x the ratio PostFx's dim already put on them) to the blend between what is there and the Crown's, and endCameraRendering puts back
// exactly what it found. The blend eases in over N74CrownSkyInS and out over N74CrownSkyOutS of frame time (grade54's fade numbers) and is kept running with the row off (D1's contract: the next frame on is the frame it
// would have drawn); the row off draws the dusk exactly. Not the sun disc, the stars or the clouds.
// THE PROOF (L-014 / L-115; Player.log): `[crownsky74] fight live, row crownroom74 on|off, blend <k>: zenith #.. -> #.., mid #.. -> #.., horizon #.. -> #.. (stone fog x<dim>)` once per fight when the blend first reads 0.99
// with the row on, and once on each flip of the row while the fight is live; the boss63 shot lines carry `crownroom74 row on|off, sky blend <k>`. NOT checked in a Player: that the colours set in beginCameraRendering are the
// ones the skybox and the stone read (URP records the sky and the stone and executes at the camera's submit, before endCameraRendering restores): if they are not, the sky stays the dusk and the line says applied while
// the frame shows none: the retake's frame is the check.
using System;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>The pure maths of the sky's blend (no Unity object). EditMode: N74BCrownRoomTests.</summary>
    public static class CrownSky74Math
    {
        /// <summary>The blend after `dt` seconds of frame time: toward 1 while the fight is live (at 1 / inS a second), toward 0 after it (at 1 / outS), never past either; `dt` 0 or less holds it.</summary>
        public static float Step(float blend, bool fight, float dt, float inS, float outS)
        {
            if (dt <= 0f) return blend;
            if (fight) return Math.Min(1f, blend + dt / Math.Max(1e-3f, inS));
            return Math.Max(0f, blend - dt / Math.Max(1e-3f, outS));
        }

        /// <summary>The smoothstep of the blend: no pop at either end.</summary>
        public static float Smooth(float k) { k = k < 0f ? 0f : k > 1f ? 1f : k; return k * k * (3f - 2f * k); }

        /// <summary>The luminance (Rec. 709) of an sRGB-valued colour: what the dim ratio compares.</summary>
        public static float Lum(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

        /// <summary>The ratio PostFx's stone-fog dimming has put on the stone's fog colours against the sky material's own (global's luminance over the material's), clamped to 0.2..1.5; 1 when either is dark.</summary>
        public static float DimRatio(float globalLum, float skyLum) => skyLum < 1e-4f || globalLum < 1e-4f ? 1f : Math.Max(0.2f, Math.Min(1.5f, globalLum / skyLum));
    }

    public sealed class CrownSky74 : MonoBehaviour
    {
        static readonly int ZenithId = Shader.PropertyToID("_Zenith"), MidId = Shader.PropertyToID("_Mid"), HorizonId = Shader.PropertyToID("_Horizon");
        static readonly int FogZ = Shader.PropertyToID("_SkyFogZenith"), FogM = Shader.PropertyToID("_SkyFogMid"), FogH = Shader.PropertyToID("_SkyFogHorizon");

        /// <summary>For the reel and the tests: the blend now (0..1), whether the Crown's fight is live, the frames the sky was drawn blended, the last proof line.</summary>
        public static float Blend;
        public static bool Fight;
        public static int AppliedFrames;
        public static string Line = "crownsky74: not attached";

        SurvivalGame game;
        bool applied, said, rowWas;
        Material skyHeld;
        Color szHeld, smHeld, shHeld, gzHeld, gmHeld, ghHeld;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("CrownSky74") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<CrownSky74>();
        }

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBegin;
            RenderPipelineManager.endCameraRendering += OnEnd;
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBegin;
            RenderPipelineManager.endCameraRendering -= OnEnd;
        }

        void Update()
        {
            game = SurvivalGame.Instance;
            if (game == null || game.sim == null) { Fight = false; Blend = CrownSky74Math.Step(Blend, false, (float)FeelClock.UnscaledDelta, UnityTuning.N74CrownSkyInS, UnityTuning.N74CrownSkyOutS); return; }
            Fight = BossAttackClock63.Read(game.sim, BossAttackClock63.Crown).Live;
            float before = Blend;
            Blend = CrownSky74Math.Step(Blend, Fight, (float)FeelClock.UnscaledDelta, UnityTuning.N74CrownSkyInS, UnityTuning.N74CrownSkyOutS); // frame time: 0 in a same-run held frame
            if (!Fight && before <= 0f) said = false;
        }

        void OnBegin(ScriptableRenderContext ctx, Camera cam)
        {
            if (applied) Put(); // a camera that never reached its end event: what was held goes back first
            SurvivalGame g = game;
            if (g == null || cam == null || cam != g.cam || Blend <= 0f) return;
            bool row = CrownRoom74.Decide(g.cfg);
            if (row != rowWas)
            {
                rowWas = row;
                if (Fight) said = false; // a flip while the fight is live is said again
            }
            if (!row) return;
            Material sky = RenderSettings.skybox;
            float k = CrownSky74Math.Smooth(Blend);
            Color tz = Look.Hex(UnityTuning.N74CrownSkyZenithHex), tm = Look.Hex(UnityTuning.N74CrownSkyMidHex), th = Look.Hex(UnityTuning.N74CrownSkyHorizonHex);
            skyHeld = sky;
            bool hasSky = sky != null && sky.HasProperty(ZenithId) && sky.HasProperty(MidId) && sky.HasProperty(HorizonId);
            gzHeld = Shader.GetGlobalColor(FogZ);
            gmHeld = Shader.GetGlobalColor(FogM);
            ghHeld = Shader.GetGlobalColor(FogH);
            float dim = hasSky ? CrownSky74Math.DimRatio(CrownSky74Math.Lum(gmHeld), CrownSky74Math.Lum(sky.GetColor(MidId))) : 1f;
            if (hasSky)
            {
                szHeld = sky.GetColor(ZenithId); smHeld = sky.GetColor(MidId); shHeld = sky.GetColor(HorizonId);
                sky.SetColor(ZenithId, Color.Lerp(szHeld, tz, k));
                sky.SetColor(MidId, Color.Lerp(smHeld, tm, k));
                sky.SetColor(HorizonId, Color.Lerp(shHeld, th, k));
            }
            Shader.SetGlobalColor(FogZ, Color.Lerp(gzHeld, tz * dim, k));
            Shader.SetGlobalColor(FogM, Color.Lerp(gmHeld, tm * dim, k));
            Shader.SetGlobalColor(FogH, Color.Lerp(ghHeld, th * dim, k));
            applied = true;
            if (!FeelClock.Holding) AppliedFrames++;
            if (!said && Blend >= 0.99f && Fight)
            {
                said = true;
                CultureInfo ci = CultureInfo.InvariantCulture;
                Line = "[crownsky74] fight live, row crownroom74 on, blend " + Blend.ToString("0.00", ci) + ": zenith " + Hex(szHeld) + " -> " + Hex(tz) + ", mid " + Hex(smHeld) + " -> " + Hex(tm) + ", horizon " + Hex(shHeld) + " -> " + Hex(th)
                    + " (stone fog x" + dim.ToString("0.00", ci) + (hasSky ? "" : "; no sky material: the fog only") + ")";
                Debug.Log(Line);
            }
        }

        void OnEnd(ScriptableRenderContext ctx, Camera cam)
        {
            if (applied) Put();
        }

        /// <summary>Puts back exactly what OnBegin found.</summary>
        void Put()
        {
            applied = false;
            if (skyHeld != null && skyHeld.HasProperty(ZenithId))
            {
                skyHeld.SetColor(ZenithId, szHeld);
                skyHeld.SetColor(MidId, smHeld);
                skyHeld.SetColor(HorizonId, shHeld);
            }
            Shader.SetGlobalColor(FogZ, gzHeld);
            Shader.SetGlobalColor(FogM, gmHeld);
            Shader.SetGlobalColor(FogH, ghHeld);
        }

        static string Hex(Color c)
        {
            Color s = new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), 1f);
            return "#" + ColorUtility.ToHtmlStringRGB(s);
        }
    }
}
