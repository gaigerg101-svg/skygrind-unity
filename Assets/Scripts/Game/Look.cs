// The dusk look, after skygrind/src/world/sky.js, fx/post.js + fx/grade.js and main.js's lights: the sky material
// (config.colors sky*), fog (config.fog near / far, colors.fog: URP's linear fog for the Unlit / Lit materials, and for
// the stone the web's own: worldMaterial.js's height fog in the sky's colour along the view ray, through the _SkyFog*
// globals Stone.shader reads), sky/ground ambient (colors.lightSky / lightGround x lights.hemiIntensity), a warm key light
// with shadows (colors.lightSun, lights.dirIntensity, dirX/Y/Z), the violet rim (colors.lightRim, lights.rimIntensity,
// rimElevationDeg) as a shader global, and URP post (NIGHT12 U3-25.2: post.js on the Volume): bloom (post.bloomThreshold /
// bloomIntensity / bloomRadius), ACES tone mapping, the grade (post.grade on ShadowsMidtonesHighlights + saturation),
// the vignette (post.vignetteOffset / vignetteDarkness, fitted to URP's curve), chromatic aberration and film grain (0
// until PostFx drives them). The per-frame driver is PostFx (LightAndLens): pulses, low HP, the quality preset, settings.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    /// <summary>The post Volume Look.Apply built and its overrides (PostFx drives them per frame).</summary>
    public sealed class PostStack
    {
        public Volume volume;
        public VolumeProfile profile;
        public Bloom bloom;
        public Tonemapping tone;
        public ColorAdjustments color;
        public ShadowsMidtonesHighlights grade;
        public Vignette vignette;
        public ChromaticAberration chroma;
        public FilmGrain grain;
    }

    public static class Look
    {
        /// <summary>The post Volume the last Apply built (null before): LightAndLens's PostFx drives it.</summary>
        public static PostStack Post { get; private set; }

        static readonly int SkyFogParams = Shader.PropertyToID("_SkyFogParams"), SkyFogZenith = Shader.PropertyToID("_SkyFogZenith"),
            SkyFogMid = Shader.PropertyToID("_SkyFogMid"), SkyFogHorizon = Shader.PropertyToID("_SkyFogHorizon"),
            SkyFogSun = Shader.PropertyToID("_SkyFogSun"), SkyFogOn = Shader.PropertyToID("_SkyFogOn");

        public static Color C(GameConfig cfg, string path, float scale = 1)
        {
            var (r, g, b) = cfg.Rgb(path);
            return new Color(r * scale, g * scale, b * scale, 1);
        }

        public static Color Hex(uint hex, float scale = 1)
        {
            var (r, g, b) = GameConfig.Hex(hex);
            return new Color(r * scale, g * scale, b * scale, 1);
        }

        /// <summary>Web direction (three.js, +z toward the viewer) to a Unity vector (z flipped).</summary>
        static Vector3 W(double x, double y, double z) => new Vector3((float)x, (float)y, (float)-z);

        public static Light Apply(GameConfig cfg, Material sky, Camera cam)
        {
            // Sky
            double el = cfg.Num("sky.sunElevationDeg") * Mathf.Deg2Rad, az = cfg.Num("sky.sunAzimuthDeg") * Mathf.Deg2Rad;
            Vector3 sunDir = W(System.Math.Cos(el) * System.Math.Sin(az), System.Math.Sin(el), -System.Math.Cos(el) * System.Math.Cos(az));
            if (sky != null)
            {
                sky.SetColor("_Zenith", C(cfg, "colors.skyZenith"));
                sky.SetColor("_Mid", C(cfg, "colors.skyMid"));
                sky.SetColor("_Horizon", C(cfg, "colors.skyHorizon"));
                sky.SetColor("_Sun", C(cfg, "colors.sun"));
                sky.SetVector("_SunDir", sunDir);
                sky.SetFloat("_SunSize", (float)cfg.Num("sky.sunSize"));
                sky.SetFloat("_Glow", (float)cfg.Num("sky.warmGlow"));
                RenderSettings.skybox = sky;
            }

            // Fog (linear, near -> far, sky-tinted): URP's, for the Unlit / Lit materials (enemies, props).
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = C(cfg, "colors.fog");
            RenderSettings.fogStartDistance = (float)cfg.Num("fog.near");
            RenderSettings.fogEndDistance = (float)cfg.Num("fog.far");
            // NIGHT12 U3-25.2: the stone's fog is the web's (worldMaterial.js FRAGMENT_FOG): smoothstep(near, far, view depth)
            // x mix(1, fog.heightMin, smoothstep(0, fog.heightFade, y)) (it thins with height: high ledges stay clear), in
            // the sky's colour along the view ray, so the far map fades into the dome behind it. Stone.shader's SkyAt is
            // Sky.shader's gradient without the disc, from these globals (the same values as the sky material's).
            Shader.SetGlobalVector(SkyFogParams, new Vector4((float)cfg.Num("fog.near"), (float)cfg.Num("fog.far"), (float)cfg.Num("fog.heightFade"), (float)cfg.Num("fog.heightMin")));
            Shader.SetGlobalColor(SkyFogZenith, C(cfg, "colors.skyZenith"));
            Shader.SetGlobalColor(SkyFogMid, C(cfg, "colors.skyMid"));
            Shader.SetGlobalColor(SkyFogHorizon, C(cfg, "colors.skyHorizon"));
            Shader.SetGlobalVector(SkyFogSun, new Vector4(sunDir.x, sunDir.y, sunDir.z, (float)cfg.Num("sky.warmGlow")));
            Shader.SetGlobalFloat(SkyFogOn, 1);
            Night51.Apply(); // NIGHT51: the stone's panels, seams, bevels and grime (UnityTuning.Stone51.cs)

            // Ambient: the hemisphere light (sky above, ground below)
            float hemi = (float)cfg.Num("lights.hemiIntensity") * UnityTuning.LightScale;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = C(cfg, "colors.lightSky", hemi);
            RenderSettings.ambientEquatorColor = Color.Lerp(C(cfg, "colors.lightSky", hemi), C(cfg, "colors.lightGround", hemi), 0.5f);
            RenderSettings.ambientGroundColor = C(cfg, "colors.lightGround", hemi);

            // Key light: from (dirX, dirY, dirZ) toward the origin
            var keyGo = new GameObject("Key light (lights.dir*)");
            Light key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = C(cfg, "colors.lightSun");
            key.intensity = (float)cfg.Num("lights.dirIntensity") * UnityTuning.LightScale;
            key.shadows = LightShadows.Soft;
            key.shadowBias = 0.02f;
            key.shadowNormalBias = 0.4f;
            if (Light54.On(cfg))
            {
                // NIGHT54 light54 (fix 7): URP reads a light's own bias only with usePipelineSettings false; until now the key light used the
                // pipeline asset's (PC_RPAsset m_ShadowDepthBias 0.1, m_ShadowNormalBias 0.5), low for a dusk sun at a grazing angle (acne that
                // crawls as the cascades move). The key light's own bias with the row on: numbers.keyDepthBias / keyNormalBias.
                var ald = keyGo.GetComponent<UniversalAdditionalLightData>();
                if (ald == null) ald = keyGo.AddComponent<UniversalAdditionalLightData>(); // Unity's null (a missing component is a fake null in the Editor)
                ald.usePipelineSettings = false;
                key.shadowBias = Light54.F(cfg, "keyDepthBias", 1.0);
                key.shadowNormalBias = Light54.F(cfg, "keyNormalBias", 0.8);
            }
            Vector3 from = W(cfg.Num("lights.dirX"), cfg.Num("lights.dirY"), cfg.Num("lights.dirZ")).normalized;
            keyGo.transform.rotation = Quaternion.LookRotation(-from);
            RenderSettings.sun = key;

            // The violet rim, as a global the stone reads (a second directional light from low behind the key)
            double rimEl = cfg.Num("lights.rimElevationDeg") * Mathf.Deg2Rad;
            Vector3 rimDir = new Vector3(-from.x, 0, -from.z).normalized * (float)System.Math.Cos(rimEl) + Vector3.up * (float)System.Math.Sin(rimEl);
            Shader.SetGlobalVector("_RimDir", rimDir);
            Shader.SetGlobalColor("_RimColor", C(cfg, "colors.lightRim", (float)cfg.Num("lights.rimIntensity") * UnityTuning.LightScale * 3));

            // The shadow distance lives on the URP asset (PC_RPAsset: UnityTuning.ShadowDistance, set once); writing it here at
            // runtime would rewrite the project asset on every Editor Play.

            // Camera + post
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.allowHDR = true;
                var data = cam.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                // NIGHT13 U3-26.4 (PerfSwitches opaqueTexture): no Opaque Texture copy for this camera. PC_RPAsset asks for one
                // (Opaque Texture on), but no project shader samples _CameraOpaqueTexture (SampleSceneColor) and no renderer
                // feature reads it (PC_Renderer: SSAO only, from depth normals), so the copy pass did nothing visible. URP's
                // per-camera override, not the asset: the tracked asset is never written (the overlay camera never copies).
                if (PerfSwitches.On(PerfFix.OpaqueTexture)) data.requiresColorOption = CameraOverrideOption.Off;
            }
            Post = BuildPost(cfg);
            return key;
        }

        /// <summary>
        /// post.js's merged effect pass on a global Volume (NIGHT12 U3-25.2): bloom, the tone map, the grade, the vignette,
        /// chromatic aberration and grain. Values that move (the pulses, low HP, the preset's bloom and grain, the tone-map
        /// setting) start at rest here; PostFx owns them after.
        /// </summary>
        static PostStack BuildPost(GameConfig cfg)
        {
            var volGo = new GameObject("Post (fx/post.js)");
            var s = new PostStack { volume = volGo.AddComponent<Volume>() };
            s.volume.isGlobal = true;
            s.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            // Bloom (post.js BloomEffect: luminanceThreshold, intensity, radius; mipmap blur ~ URP's scatter).
            s.bloom = s.profile.Add<Bloom>(true);
            s.bloom.threshold.Override((float)cfg.Num("post.bloomThreshold"));
            s.bloom.intensity.Override((float)cfg.Num("post.bloomIntensity") * UnityTuning.BloomIntensityScale);
            s.bloom.scatter.Override((float)cfg.Num("post.bloomRadius"));

            s.tone = s.profile.Add<Tonemapping>(true);
            s.tone.mode.Override(TonemappingMode.ACES);

            // The grade (grade.js, after the tone map: URP's LDR grading does the same order): shadows lean to
            // post.grade.shadowTint by split (c x mix(1, tint, shadows x split)), highlights warm by highlightTint x split x 0.25
            // (the web adds it; URP multiplies: x (1 + that)), then saturation (x post.grade.saturation). URP's
            // ShadowsMidtonesHighlights takes a linear multiplier through its sRGB encoding (ColorUtils.PrepareShadowsMidtonesHighlights).
            double[] shadowTint = cfg.Nums("post.grade.shadowTint"), highTint = cfg.Nums("post.grade.highlightTint");
            float split = (float)cfg.Num("post.grade.split");
            s.grade = s.profile.Add<ShadowsMidtonesHighlights>(true);
            s.grade.shadows.Override(SmhValue(1 + ((float)shadowTint[0] - 1) * split, 1 + ((float)shadowTint[1] - 1) * split, 1 + ((float)shadowTint[2] - 1) * split));
            float share = split * UnityTuning.GradeHighlightShare;
            s.grade.highlights.Override(SmhValue(1 + (float)highTint[0] * share, 1 + (float)highTint[1] * share, 1 + (float)highTint[2] * share));
            s.grade.shadowsStart.Override(0f);
            s.grade.shadowsEnd.Override(UnityTuning.GradeShadowsEnd);
            s.grade.highlightsStart.Override(UnityTuning.GradeHighlightsStart);
            s.grade.highlightsEnd.Override(UnityTuning.GradeHighlightsEnd);

            s.color = s.profile.Add<ColorAdjustments>(true);
            s.color.postExposure.Override(UnityTuning.Exposure);
            s.color.saturation.Override(((float)cfg.Num("post.grade.saturation") - 1) * 100);

            // The vignette (post.js VignetteEffect offset / darkness), fitted to URP's curve (the 0.22 literal is gone).
            FitVignette(cfg.Num("post.vignetteOffset"), cfg.Num("post.vignetteDarkness"), out float vi, out float vs);
            s.vignette = s.profile.Add<Vignette>(true);
            s.vignette.intensity.Override(vi);
            s.vignette.smoothness.Override(vs);
            s.vignette.rounded.Override(false);

            // Chromatic aberration (0 at rest: inactive, no cost) and film grain (the preset's; PostFx sets it).
            s.chroma = s.profile.Add<ChromaticAberration>(true);
            s.chroma.intensity.Override(0f);
            s.grain = s.profile.Add<FilmGrain>(true);
            s.grain.intensity.Override(0f);
            s.volume.sharedProfile = s.profile;
            return s;
        }

        /// <summary>A linear multiplier per channel as URP's ShadowsMidtonesHighlights value (sRGB-encoded, no offset).</summary>
        static Vector4 SmhValue(float r, float g, float b) => new Vector4(Mathf.LinearToGammaSpace(r), Mathf.LinearToGammaSpace(g), Mathf.LinearToGammaSpace(b), 0);

        /// <summary>
        /// The web's vignette factor at uv distance d from the centre (pmndrs VignetteEffect, technique DEFAULT):
        /// smoothstep(0.8, offset x 0.799, d x (darkness + offset)) (the pmndrs shader's literals: UnityTuning.WebVignette*).
        /// </summary>
        public static double WebVignette(double d, double offset, double darkness)
        {
            double e0 = UnityTuning.WebVignetteEdge, e1 = offset * UnityTuning.WebVignetteOffsetScale;
            double t = System.Math.Max(0, System.Math.Min(1, (d * (darkness + offset) - e0) / (e1 - e0)));
            return t * t * (3 - 2 * t);
        }

        /// <summary>
        /// URP's vignette (intensity, smoothness) closest to the web's (offset, darkness): least squares over
        /// UnityTuning.VignetteFitSamples radii up to the screen corner, on a grid searched twice. URP's factor is
        /// pow(saturate(1 - (3 intensity d)^2), 5 smoothness) (Common.hlsl ApplyVignette, PostProcessPass: x 3, x 5).
        /// </summary>
        public static void FitVignette(double offset, double darkness, out float intensity, out float smoothness)
        {
            int n = UnityTuning.VignetteFitSamples, grid = UnityTuning.VignetteFitGrid;
            double corner = UnityTuning.VignetteCorner;
            var d = new double[n];
            var g = new double[n];
            for (int i = 0; i < n; i++)
            {
                d[i] = corner * (i + 1) / n;
                g[i] = WebVignette(d[i], offset, darkness);
            }
            // a = (3 intensity)^2 below 1 / corner^2 (the corner never clips to black), s = 5 smoothness in URP's range.
            double aLo = 0, aHi = 1 / (corner * corner), sLo = 0.05, sHi = 5;
            double bestA = aHi * 0.5, bestS = 1;
            for (int pass = 0; pass < 2; pass++)
            {
                double best = double.MaxValue, da = (aHi - aLo) / grid, ds = (sHi - sLo) / grid;
                for (int ia = 0; ia <= grid; ia++)
                {
                    double a = aLo + da * ia;
                    for (int js = 0; js <= grid; js++)
                    {
                        double sm = sLo + ds * js, err = 0;
                        for (int i = 0; i < n; i++)
                        {
                            double f = System.Math.Pow(System.Math.Max(0, 1 - a * d[i] * d[i]), sm) - g[i];
                            err += f * f;
                        }
                        if (err < best)
                        {
                            best = err;
                            bestA = a;
                            bestS = sm;
                        }
                    }
                }
                aLo = System.Math.Max(0, bestA - da);
                aHi = System.Math.Min(1 / (corner * corner), bestA + da);
                sLo = System.Math.Max(0.05, bestS - ds);
                sHi = System.Math.Min(5, bestS + ds);
            }
            intensity = Mathf.Clamp01((float)(System.Math.Sqrt(bestA) / 3));
            smoothness = Mathf.Clamp((float)(bestS / 5), 0.01f, 1f);
        }
    }
}
