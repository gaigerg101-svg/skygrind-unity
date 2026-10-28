// NIGHT20 U3-34.7 step 3 (builder V): the travel blur (Research/2026-10-06-speed-lines.md 3.3; GG's SPEED LINES switch:
// "a light travel blur on boosts (linear velocity only: never on mouse look)"). Ported from two GitHub projects:
// - Bauxitedev/godot-motion-blur (MIT, Copyright (c) 2018 Bauxitedev), motion_blur/moblur_shader.shader: the pixel's
//   position from the depth texture, offset by the camera's velocity, projected back; the image averaged over taps along
//   `pixel_diff_ndc * (i / n - 0.5) * intensity`. Here the angular term is dropped (only the eye's linear travel: a turn
//   never smears the aim), the velocity is the Mover's over a 0.010 s shutter, the middle is kept sharp
//   (smoothstep(0.25, 0.60, rho) about the crosshair), the smear clamped to 1.8 % of the width, six taps.
// - daniel-ilett/blur-pro-urp (MIT, Copyright (c) 2026 Daniel Ilett), Assets/Blur Shaders Pro/URP/Scripts/RadialBlur.cs:
//   the Unity 6 render-graph recipe: a copy pass (the active colour -> a temporary) and a main pass (the temporary ->
//   the active colour through the material, Blitter.BlitTexture), requiresIntermediateTexture.
// The pass runs on the main (base) camera only, after its post-processing injection point and before the viewmodel's
// overlay camera, so the gun and the HUD stay sharp (DOOM Eternal excludes its gun the same way). It is enqueued only
// while the game set a strength above 0 this frame (a boost): no cost otherwise. Its MOTION BLUR row (ACCESS, on by
// default) and REDUCED MOTION turn it off. The lead adds the feature to PC_Renderer (the report says how).
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    /// <summary>The travel blur's pure rules (TravelBlurTests; the shader mirrors Offset and Clamp).</summary>
    public static class TravelBlurMath
    {
        static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;

        /// <summary>0 under TravelBlurFrom m/s, a smoothstep to 1 at TravelBlurFull, x EFFECTS INTENSITY (0..1); 0 when off.</summary>
        public static double Strength(double vEff, double intensity, bool on)
        {
            if (!on || !(vEff > UnityTuning.TravelBlurFrom)) return 0;
            double t = Clamp01((vEff - UnityTuning.TravelBlurFrom) / (UnityTuning.TravelBlurFull - UnityTuning.TravelBlurFrom));
            return t * t * (3 - 2 * t) * Clamp01(intensity);
        }

        /// <summary>The middle stays sharp: smoothstep(TravelBlurMaskIn, TravelBlurMaskFull, rho).</summary>
        public static double Mask(double rho)
        {
            double t = Clamp01((rho - UnityTuning.TravelBlurMaskIn) / (UnityTuning.TravelBlurMaskFull - UnityTuning.TravelBlurMaskIn));
            return t * t * (3 - 2 * t);
        }

        /// <summary>The eye's travel over `shutter` s in view space (x right, y up, z ahead) from its world velocity `v`
        /// (Unity space) and the view's basis: the linear velocity only (the look's turn never enters).</summary>
        public static Vector3 ViewDisp(Vector3 v, Vector3 right, Vector3 up, Vector3 fwd, double shutter)
        {
            float s = (float)shutter;
            return new Vector3(Vector3.Dot(v, right) * s, Vector3.Dot(v, up) * s, Vector3.Dot(v, fwd) * s);
        }

        /// <summary>
        /// Bauxitedev's reprojection without the angular term: a pixel at view-space `p` (x right, y up, z ahead, m) and the
        /// eye's travel `disp`: where the point sits on screen relative to the eye an exposure apart, as a uv offset
        /// (0.5 x the NDC difference). Taps go both ways along it, so its sign doesn't matter.
        /// </summary>
        public static Vector2 Offset(Vector3 p, Vector3 disp, float tanH, float tanV)
        {
            if (!(p.z > 1e-4f)) return Vector2.zero;
            Vector3 q = p + disp;
            if (!(q.z > 0.05f)) return Vector2.zero;
            float ax = p.x / (p.z * tanH), ay = p.y / (p.z * tanV);
            float bx = q.x / (q.z * tanH), by = q.y / (q.z * tanV);
            return new Vector2((bx - ax) * 0.5f, (by - ay) * 0.5f);
        }

        /// <summary>A uv offset no longer than `maxWidthShare` of the screen's width (the height counted in widths: aspect = w / h).</summary>
        public static Vector2 Clamp(Vector2 offUv, float aspect, float maxWidthShare)
        {
            float wx = offUv.x, wy = offUv.y / aspect;
            float len = Mathf.Sqrt(wx * wx + wy * wy);
            return len > maxWidthShare && len > 0 ? offUv * (maxWidthShare / len) : offUv;
        }
    }

    /// <summary>This frame's blur, set by the game (SurvivalGame.Fx20.cs) and read by the pass.</summary>
    public static class TravelBlur
    {
        /// <summary>The camera the blur is for (the main camera; never the viewmodel's overlay, a scene view or a preview).</summary>
        public static Camera Target;
        /// <summary>0..1 this frame (0: the pass isn't enqueued).</summary>
        public static float Strength;
        /// <summary>The eye's travel over the shutter in view space (x right, y up, z ahead, m).</summary>
        public static Vector3 Disp;
        /// <summary>The view's half-angle tangents this frame.</summary>
        public static float TanH, TanV;
        /// <summary>Time.frameCount when the values were set (a stale frame draws nothing).</summary>
        public static int Frame = -1;

        public static void Set(Camera cam, float strength, Vector3 disp, float tanH, float tanV)
        {
            Target = cam;
            Strength = strength;
            Disp = disp;
            TanH = tanH;
            TanV = tanV;
            Frame = Time.frameCount;
        }

        /// <summary>Should the pass run for `cam` this frame?</summary>
        public static bool On(Camera cam) => cam != null && cam == Target && Strength > 0 && Frame == Time.frameCount;
    }

    /// <summary>The URP renderer feature (PC_Renderer's list): the travel blur's pass on the main camera.</summary>
    public sealed class TravelBlurFeature : ScriptableRendererFeature
    {
        TravelBlurPass pass;
        Crt29Pass crt29;

        public override void Create()
        {
            pass = new TravelBlurPass();
            crt29 = new Crt29Pass();
            name = "Travel Blur";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            ref CameraData cd = ref renderingData.cameraData;
            if (!cd.isPreviewCamera && !cd.isSceneViewCamera) Ssao55.Enqueue(renderer); // NIGHT55 C ssao55 (Game/Ssao55.cs): the SSAO noise held still after the SSAO pass records (row off: nothing enqueued)
            Cap57.Enqueue(renderer, cd.camera); // NIGHT57 V colour56's world saturation cap / yellow56's dim yellow (Render/Cap57Pass.cs): the main camera (its event sorts it after the CRT) and, for the dim yellow, the viewmodel's overlay camera; rows off: nothing enqueued
            if (cd.isPreviewCamera || cd.isSceneViewCamera || cd.renderType != CameraRenderType.Base) return;
            if (TravelBlur.On(cd.camera) && pass.Ready()) renderer.EnqueuePass(pass);
            if (Crt29.On(cd.camera) && crt29.Ready()) renderer.EnqueuePass(crt29); // NIGHT29 item 4: the CRT overlay rides this feature, after the blur
        }

        protected override void Dispose(bool disposing)
        {
            pass?.Dispose();
            crt29?.Dispose();
            base.Dispose(disposing);
        }

        sealed class TravelBlurPass : ScriptableRenderPass
        {
            Material material;
            static readonly int DispId = Shader.PropertyToID("_TravelDisp");
            static readonly int ParamsId = Shader.PropertyToID("_TravelParams");
            static readonly int ClampId = Shader.PropertyToID("_TravelClamp");

            class PassData
            {
                public Material material;
                public TextureHandle source;
            }

            public TravelBlurPass()
            {
                profilingSampler = new ProfilingSampler("SKYGRIND.TravelBlur");
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing; // Cyanilux's README: after post, on the base camera
                requiresIntermediateTexture = true;
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }

            /// <summary>The material (SKYGRIND/TravelBlur, in a Resources folder); false when the shader isn't in the build.</summary>
            public bool Ready()
            {
                if (material != null) return true;
                Shader sh = Shader.Find("SKYGRIND/TravelBlur");
                if (sh == null) return false;
                material = CoreUtils.CreateEngineMaterial(sh);
                return true;
            }

            public void Dispose() => CoreUtils.Destroy(material);

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null) return;
                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                if (resources.isActiveTargetBackBuffer) return; // needs the intermediate colour (requiresIntermediateTexture)
                Vector3 d = TravelBlur.Disp;
                material.SetVector(DispId, new Vector4(d.x, d.y, d.z, TravelBlur.Strength));
                material.SetVector(ParamsId, new Vector4(TravelBlur.TanH, TravelBlur.TanV, UnityTuning.TravelBlurMaskIn, UnityTuning.TravelBlurMaskFull));
                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                material.SetVector(ClampId, new Vector4(UnityTuning.TravelBlurClamp, desc.height > 0 ? (float)desc.width / desc.height : 1, 0, 0));
                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;
                TextureHandle copy = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_TravelBlurCopy", false);

                // blur-pro-urp's copy pass (the active colour -> the temporary)
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("TravelBlur copy", out PassData data, profilingSampler))
                {
                    data.source = resources.activeColorTexture;
                    builder.UseTexture(data.source, AccessFlags.Read);
                    builder.SetRenderAttachment(copy, 0, AccessFlags.Write);
                    builder.SetRenderFunc(static (PassData p, RasterGraphContext ctx) => Blitter.BlitTexture(ctx.cmd, p.source, new Vector4(1, 1, 0, 0), 0, false));
                }

                // ... and its main pass (the temporary -> the active colour through the blur)
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("TravelBlur", out PassData data, profilingSampler))
                {
                    data.source = copy;
                    data.material = material;
                    builder.UseTexture(copy, AccessFlags.Read);
                    if (resources.cameraDepthTexture.IsValid()) builder.UseTexture(resources.cameraDepthTexture, AccessFlags.Read);
                    builder.UseAllGlobalTextures(true); // _CameraDepthTexture (SampleSceneDepth)
                    builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.Write);
                    builder.SetRenderFunc(static (PassData p, RasterGraphContext ctx) => Blitter.BlitTexture(ctx.cmd, p.source, new Vector4(1, 1, 0, 0), p.material, 0));
                }
            }
        }
    }
}
