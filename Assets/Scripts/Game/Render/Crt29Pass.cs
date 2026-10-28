// NIGHT29 item 4 (builder V): the CRT overlay's render pass and the driver that feeds it (Research/2026-10-09-wave29/
// R4b-crt-lights.md: warp 2 % at the corners, scanlines, fixed-seed grain flicker, a slight chromatic split at the edges, a mild
// vignette, an optional pixel step; default strength 35 %). The pass follows Render/TravelBlurFeature.cs's render-graph recipe
// (a copy pass, then a main pass through the material) and rides that feature (PC_Renderer already lists it: no asset edit):
// TravelBlurFeature creates one Crt29Pass and enqueues it after the blur. It runs on the main (base) camera's colour after
// post-processing; the viewmodel's overlay camera and the Screen Space Overlay HUD are drawn later, so the gun and the text stay
// crisp. Skipped (not enqueued) when the strength is 0, the overlay is off, N29Fixes.CrtOn is false (-skygrindOff29 or
// design.n29.crt.enabled false) or the shader is missing (one Debug.LogWarning, then off). View only; no sim path.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    /// <summary>The CRT pass (enqueued by TravelBlurFeature, after the blur).</summary>
    public sealed class Crt29Pass : ScriptableRenderPass
    {
        Material material;
        bool warned;
        static readonly int P0 = Shader.PropertyToID("_Crt0");
        static readonly int P1 = Shader.PropertyToID("_Crt1");
        static readonly int P2 = Shader.PropertyToID("_Crt2");
        static readonly int P3 = Shader.PropertyToID("_Crt3"); // NIGHT30 item 8: x levels, y dither scale (px), z quantise strength (0 off), w grain-in-the-dark (0 off)
        static readonly int P4 = Shader.PropertyToID("_Crt4"); // NIGHT35 look35 / ridelook35: x virtual lines (0 off), y flat chroma split (uv), z quantise in sRGB, w tear amount
        static readonly int P5 = Shader.PropertyToID("_Crt5"); // NIGHT35 ridelook35: x the tear's step, y the sweep band's centre (uv y), z its strength (0 off), w its half height
        static readonly int P6 = Shader.PropertyToID("_Lit72"); // NIGHT72 lit72 (Lit72.cs): x luma added at black in the shadows (0 off), y shadow end, z lift end, w gain cap
        static readonly int P7 = Shader.PropertyToID("_Fix72"); // NIGHT72 fix72 (Fix72.cs): x 1 = the grain is added in display (sRGB) space, 0 = in linear light as before

        class PassData
        {
            public Material material;
            public TextureHandle source;
        }

        public Crt29Pass()
        {
            profilingSampler = new ProfilingSampler("SKYGRIND.Crt29");
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            requiresIntermediateTexture = true;
        }

        /// <summary>The material (SKYGRIND/Crt29, in a Resources folder); false (one warning) when the shader isn't in the build.</summary>
        public bool Ready()
        {
            if (material != null) return true;
            Shader sh = Shader.Find("SKYGRIND/Crt29");
            if (sh == null)
            {
                if (!warned)
                {
                    warned = true;
                    Debug.LogWarning("Crt29: shader SKYGRIND/Crt29 not found (Assets/Shaders/Resources/Crt29.shader); the CRT overlay stays off.");
                }
                return false;
            }
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
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            float w = desc.width, h = desc.height;
            // NIGHT38 H (design.n38.juice38, Juice38.cs): the boss entrance's flicker and the damage static ride this pass: added on top of the CRT's own
            // values (grain, the flat split, scanline depth), the tear and the sweep band as the larger of theirs and ridelook35's. All 0 with the row off.
            bool j = Juice38Crt.Live;
            float jScan = j ? Juice38Crt.Scan : 0f, jGrain = j ? Juice38Crt.Grain : 0f, jSplit = j ? Juice38Crt.SplitUv : 0f;
            bool jTear = j && Juice38Crt.Tear > Crt29.Tear35, jSweep = j && Juice38Crt.Sweep > Crt29.Sweep35;
            // a scanline depth needs scanlines: with the CRT's count at 0 (the pass forced on for juice alone) the look's 240 lines carry it
            float scanCount = Crt29.ScanCount > 0 ? Crt29.ScanCount : jScan > 0 ? UnityTuning.Look35ScanCount : 0f;
            // GG 2026-10-26 ("there's flickers"): a scanline count that is not a whole number of pixels per line beats against the pixel grid
            // (720 lines on a 1080p target = 1.5 px a line: a moire that crawls as the view moves). With Bright71 on, the count snaps to whole
            // pixels per line (at least 2), the nearest to the configured density.
            if (Bright71.On && scanCount > 0 && h > 0) scanCount = h / Mathf.Max(2f, Mathf.Round(h / scanCount));
            // NIGHT72 L2: the flicker meter's diagnostic (-skygrindFlickerOff72=<source>+...: Fix72.cs) takes one CRT source out for a ranking run; none named: every
            // multiplier is 1 and the values below are what they were. fix72 (the row, read every frame) puts the grain into display space (_Fix72.x).
            float offScan = Fix72.Off("scan") ? 0f : 1f, offChroma = Fix72.Off("chroma") ? 0f : 1f, offGrain = Fix72.Off("crtgrain") ? 0f : 1f, offQuant = Fix72.Off("quant") ? 0f : 1f;
            SurvivalGame fixGame = SurvivalGame.Instance;
            bool fix72 = fixGame != null && fixGame.cfg != null && NightRows.ViewOn(fixGame.cfg, Fix72.Row);
            material.SetVector(P0, new Vector4(Crt29.Warp, Mathf.Min(0.9f, Crt29.ScanIntensity + jScan) * offScan, scanCount, Crt29.ChromaUv * offChroma));
            material.SetVector(P1, new Vector4((Crt29.Grain + jGrain) * offGrain, Crt29.Vignette, j && Juice38Crt.Grain > 0 ? Crt29.GrainStep ^ Juice38Crt.TearStep : Crt29.GrainStep, Crt29.PixelStep));
            material.SetVector(P2, new Vector4(w, h, 0, 0));
            material.SetVector(P3, new Vector4(Crt29.QuantLevels, Crt29.QuantDitherScale, Crt29.QuantStrength * offQuant, Crt29.GrainDark));
            material.SetVector(P4, new Vector4(Crt29.Lines35, (Crt29.FlatChroma35 + jSplit) * offChroma, Crt29.Srgb35, jTear ? Juice38Crt.Tear : Crt29.Tear35));
            material.SetVector(P7, new Vector4(fix72 ? 1f : 0f, 0, 0, 0));
            material.SetVector(P5, new Vector4(jTear ? Juice38Crt.TearStep : Crt29.TearStep35, jSweep ? 0.5f : Crt29.SweepY35, jSweep ? Juice38Crt.Sweep : Crt29.Sweep35, jSweep ? Juice38Crt.SweepHalf : Crt29.SweepHalf35));
            material.SetVector(P6, Ice74View.PostVector(Lit72.Vector)); // NIGHT72 lit72: zero unless the row is ON in a boss room or level 5 (the shader's block is skipped at x = 0); NIGHT74 ice74: with the arena's numbers while the player is in THE ICEBREAKER's arena (Ice74.cs; lit72's own vector with the row off)
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            TextureHandle copy = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_Crt29Copy", false);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Crt29 copy", out PassData data, profilingSampler))
            {
                data.source = resources.activeColorTexture;
                builder.UseTexture(data.source, AccessFlags.Read);
                builder.SetRenderAttachment(copy, 0, AccessFlags.Write);
                builder.SetRenderFunc(static (PassData p, RasterGraphContext ctx) => Blitter.BlitTexture(ctx.cmd, p.source, new Vector4(1, 1, 0, 0), 0, false));
            }
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Crt29", out PassData data, profilingSampler))
            {
                data.source = copy;
                data.material = material;
                builder.UseTexture(copy, AccessFlags.Read);
                builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderFunc(static (PassData p, RasterGraphContext ctx) => Blitter.BlitTexture(ctx.cmd, p.source, new Vector4(1, 1, 0, 0), p.material, 0));
            }
        }
    }

    /// <summary>
    /// Feeds the pass once per frame without a hook in SurvivalGame: it installs itself after the first scene loads, finds the
    /// SurvivalGame, reads the CRT setting (VIDEO) and the CRT OVERLAY switch (ACCESS) from its settings, the look from
    /// design.n29.crt, and calls Crt29.Set. REDUCED MOTION holds the grain still.
    /// </summary>
    public sealed class Crt29Driver : MonoBehaviour
    {
        SurvivalGame game;
        int settingsSeen = -1;
        float strength = 0;
        bool overlayOn = true;
        float warp, scanI, scanN, chroma, grain, vig, pixel, defaultStrength;
        double grainHz;
        bool read;

        /// <summary>NIGHT38 H (juice38): after every Update (the camera's Juice38 step included), a live flicker / static keeps the pass on even with the
        /// CRT at 0 or its row off: the pass runs with every CRT value 0 and draws only juice38's adds (Crt29Pass.RecordRenderGraph).</summary>
        void LateUpdate()
        {
            Calm72.Tick(game); // NIGHT72 calm72: after Ui has written its vignettes this frame (every Update is done): the red borders held or put back
            // NIGHT72 lit72: a live lift keeps the pass on too (the CRT at 0 and the look off still get the arenas' shadows): the same epsilon add
            if (game == null || game.cam == null || !(Juice38Crt.Live || Lit72.Live || Ice74View.PostLive) || Crt29.On(game.cam)) return;
            Crt29.Set(game.cam, UnityTuning.Juice38LiveEpsilon, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Crt29Driver") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Crt29Driver>();
        }

        void Update()
        {
            if (game == null)
            {
                game = FindFirstObjectByType<SurvivalGame>();
                read = false;
                settingsSeen = -1;
            }
            if (game == null || game.cam == null || game.cfg == null) { Lit72.Tick(null); Crt29.Strength = 0; Crt29.SetLook30(0, 0, 0, 0); Crt29.SetLook35(0, 0, 0); return; }
            Lit72.Tick(game); // NIGHT72 lit72: this frame's weight and row state, before every early return below (the pass reads Lit72.Vector)
            // NIGHT30 item 8 (builder V, design.n30.look30): THE LOOK rides this pass: the quantise + Bayer dither, half the grain in the dark, PIXEL SIZE.
            // It runs with the CRT at 0 too (Crt29.On); -skygrindOff30 or the row off: NIGHT29's pass exactly.
            bool look = N30Fixes.ViewRowOn(game.cfg, "look30");
            if (look) Crt29.SetLook30(UnityTuning.Look30Levels, UnityTuning.Look30DitherScale, UnityTuning.Look30Strength, 1f);
            else Crt29.SetLook30(0, 0, 0, 0);
            float lookPixel = 0;
            if (look && game.settings != null && game.settings.store.Item(UnityTuning.Pixel30Setting) != null)
                lookPixel = Look30Math.PixelBlock(game.settings.Num(UnityTuning.Pixel30Setting), Screen.height);
            // NIGHT35 V (design.n35.look35; -skygrindOff35 or the row off: NIGHT30's pass exactly): the PS2 pass that shows: ~480 lines by point upscale,
            // 32 levels at full strength in sRGB with the Bayer dither on the virtual-pixel grid, and (below) scanlines / splits / grain that read at 1080p
            bool look35 = N35Fixes.ViewRowOn(game.cfg, "look35");
            if (look35)
            {
                Crt29.SetLook30(UnityTuning.Look35Levels, UnityTuning.Look35DitherCell, UnityTuning.Look35QuantStrength, 1f);
                float lines = Look35Math.Lines(UnityTuning.Look35Lines, Screen.height, lookPixel);
                Crt29.SetLook35(lines, Look35Math.FlatChromaUv(UnityTuning.Look35FlatChromaPx, lines, Screen.height > 0 ? (double)Screen.width / Screen.height : 0), 1f);
                lookPixel = 0; // the virtual lines draw the blocks (PIXEL SIZE folds into them)
                look = true;
            }
            else Crt29.SetLook35(0, 0, 0);
            if (!N29Fixes.CrtOn(game.cfg))
            {
                Crt29.Strength = 0;
                if (look) Crt29.Set(game.cam, 0, 0, 0, 0, 0, 0, 0, lookPixel, 0); // the look alone: every CRT effect at 0
                return;
            }
            if (!read)
            {
                read = true;
                GameConfig c = game.cfg;
                defaultStrength = (float)c.Num("design.n29.crt.defaultStrength");
                warp = (float)c.Num("design.n29.crt.warp");
                scanN = (float)c.Num("design.n29.crt.scanlineCount");
                scanI = (float)c.Num("design.n29.crt.scanlineIntensity");
                chroma = (float)c.Num("design.n29.crt.chromaUv");
                grain = (float)c.Num("design.n29.crt.grain");
                grainHz = c.Num("design.n29.crt.grainHz");
                vig = (float)c.Num("design.n29.crt.vignette");
                pixel = Crt29Math.PixelBlock(c.Num("design.n29.crt.pixelStep"), Screen.height);
            }
            GameSettings st = game.settings;
            if (st != null && st.version != settingsSeen)
            {
                settingsSeen = st.version;
                double s = st.store.Item(UnityTuning.CrtSetting) != null ? st.Num(UnityTuning.CrtSetting) : defaultStrength;
                overlayOn = st.store.Item(UnityTuning.CrtOverlaySetting) != null ? st.Bool(UnityTuning.CrtOverlaySetting) : UnityTuning.CrtOverlayDefault;
                strength = Crt29Math.Strength(s, overlayOn);
            }
            else if (st == null) strength = Crt29Math.Strength(defaultStrength, true);
            if (strength <= 0)
            {
                Crt29.Strength = 0;
                if (look) Crt29.Set(game.cam, 0, 0, 0, 0, 0, 0, 0, lookPixel, 0); // the CRT off (strength 0 or CRT OVERLAY off): the look alone
                return;
            }
            bool frozen = game.cfg.movement.settings.reducedMotion;
            float k = strength;
            if (look35)
            {
                // NIGHT35 look35: the CRT setting scales tonight's numbers around its default (the default = the look as tuned; 0 = none; capped at Look35GainMax)
                float g35 = Look35Math.Gain(strength, defaultStrength);
                Crt29.Set(game.cam, k, warp * g35, UnityTuning.Look35ScanIntensity * g35, UnityTuning.Look35ScanCount, UnityTuning.Look35ChromaUv * g35,
                    UnityTuning.Look35Grain * g35, vig * g35, 0, Crt29Math.GrainStep(FeelClock.UnscaledNow, grainHz, frozen));
                return;
            }
            Crt29.Set(game.cam, k, warp * k, scanI * k, scanN, chroma * k, grain * k, vig * k,
                lookPixel > 0 ? lookPixel : pixel, Crt29Math.GrainStep(FeelClock.UnscaledNow, grainHz, frozen));
        }
    }
}
