// NIGHT57 builder V (slate V1-colour56: "cap the world first, THEN the per-level key / accent under it"; GAPS.md gap 5; research/design.md lesson 7,
// Valve's Left 4 Dead: "We did that with a saturation threshold. Anything below that moves towards gray"). THE WORLD SATURATION CAP, part of the
// colour56 row: while colour56 is on (and no -skygrindOff57 or lower: NIGHT56's colour56 has no cap), each level's cap
// (design.n56.colour56.numbers.cap.levels.<key>, keyed as decor54's) is applied by SKYGRIND/Cap57 (Assets/Shaders/Resources/Cap57.shader) to the
// main camera's colour after its post-processing and the CRT pass: a pixel whose hue is OUTSIDE the role families (numbers.cap.roleArcs: danger and
// yellow, cyan, magenta / FUSION) and whose saturation is over the cap moves to the cap at its own hue and luminance; the role families keep theirs, so
// the tells, the usable cyan and RIVAL stay the most saturated things on screen. Hue and luminance kept: never a tint (GG's "teal shift" and "one
// olive-green on every surface" were tints); the viewmodel's overlay camera and the HUD draw after it (untouched). The level's key and accent are
// colour56's own non-role hues, so they sit under the cap too. Row off or no cap for the level: the pass is never enqueued (NIGHT56's frame bit
// for bit). Hooked by ONE line in Render/TravelBlurFeature.cs AddRenderPasses (Cap57.Enqueue, after the CRT's line); the level set by Colour56's
// Update on each new map (Cap57.ForMap). The proof: the a357 reel (FeelReel.N57.Colour.cs) shoots each frame and its "-nocap" twin one frame later
// (Cap57.Hide), and n56-colour.py reads both (the cap's exact pixels by the difference); Cap57Math is the shader's maths in C# for N57ColourTests.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    /// <summary>The cap's maths (Cap57.shader's Frag, n57-v-cap.py's cap_rgb), on an sRGB colour 0..1.</summary>
    public static class Cap57Math
    {
        /// <summary>1 deep inside the arc [lo, hi] (deg, wrapping through 0), 0 outside, a smoothstep over f deg inside each edge.</summary>
        public static double ArcWeight(double h, double lo, double hi, double f)
        {
            double span = (hi - lo + 720.0) % 360.0, d = (h - lo + 720.0) % 360.0;
            if (d > span) return 0;
            double t = System.Math.Min(d, span - d) / System.Math.Max(f, 1e-4);
            t = t < 0 ? 0 : t > 1 ? 1 : t;
            return t * t * (3 - 2 * t);
        }

        /// <summary>The protection 0..1 of a hue: the largest of the arcs' weights (arcs = lo0, hi0, lo1, hi1, ...).</summary>
        public static double Protect(double h, double[] arcs, double f)
        {
            double p = 0;
            for (int i = 0; i + 1 < arcs.Length; i += 2) p = System.Math.Max(p, ArcWeight(h, arcs[i], arcs[i + 1], f));
            return p;
        }

        /// <summary>One sRGB colour (0..1) through the cap: returns the new colour (hue and luminance kept; unchanged under the cap or in a role arc).</summary>
        public static void Apply(double r, double g, double b, double cap, double[] arcs, double f, out double r2, out double g2, out double b2) =>
            Apply(r, g, b, cap, arcs, f, null, out r2, out g2, out b2);

        /// <summary>As above, plus yellow56's DIM yellow (NIGHT57 V4; null: off): y = { the S cap, the value it is whole at or under, the value it is
        /// none at or over, the arc's lo, hi, feather (deg)[, the keep bound: S at or over it untouched (NIGHT59 V2; absent or <= 0: none)[, the keep
        /// bound's value floor: kept only at or over this value]] }.</summary>
        public static void Apply(double r, double g, double b, double cap, double[] arcs, double f, double[] y, out double r2, out double g2, out double b2)
        {
            r2 = r; g2 = g; b2 = b;
            double mx = System.Math.Max(r, System.Math.Max(g, b)), mn = System.Math.Min(r, System.Math.Min(g, b)), d = mx - mn;
            double lo = System.Math.Min(cap, y != null ? y[0] : 1);
            if (lo >= 1 || mx <= 1e-6 || d <= 1e-6) return;
            double s = d / mx;
            if (s <= lo) return;
            double h = r >= mx ? (g - b) / d : g >= mx ? 2 + (b - r) / d : 4 + (r - g) / d;
            h = h / 6.0;
            h = (h - System.Math.Floor(h)) * 360.0;
            double st = s;
            if (s > cap)
            {
                double p = Protect(h, arcs, f);
                st = s - (1 - p) * (s - cap);
            }
            if (y != null && st > y[0] && !(y.Length > 6 && y[6] > 0 && s >= y[6] && (y.Length < 8 || mx >= y[7])))
            {
                double t = (mx - y[1]) / System.Math.Max(y[2] - y[1], 1e-6);
                t = t < 0 ? 0 : t > 1 ? 1 : t;
                double wy = ArcWeight(h, y[3], y[4], y[5]) * (1 - t * t * (3 - 2 * t));
                st = st - wy * (st - y[0]);
            }
            if (st >= s) return;
            // toward the grey of the same luminance (Rec. 709 on the sRGB value): k solves S(L + k (c - L)) = st
            double L = Luma(r, g, b);
            double k = st * L / System.Math.Max(d - st * (mx - L), 1e-6);
            k = k < 0 ? 0 : k > 1 ? 1 : k;
            r2 = L + k * (r - L);
            g2 = L + k * (g - L);
            b2 = L + k * (b - L);
        }

        /// <summary>Rec. 709 luminance of an sRGB value (n56-colour.py's lum; the greyscale check's).</summary>
        public static double Luma(double r, double g, double b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;

        /// <summary>HSV hue (deg) and saturation of an sRGB colour (for the tests).</summary>
        public static void Hs(double r, double g, double b, out double h, out double s)
        {
            double mx = System.Math.Max(r, System.Math.Max(g, b)), mn = System.Math.Min(r, System.Math.Min(g, b)), d = mx - mn;
            s = mx > 0 ? d / mx : 0;
            h = 0;
            if (d <= 1e-9) return;
            h = r >= mx ? (g - b) / d : g >= mx ? 2 + (b - r) / d : 4 + (r - g) / d;
            h = h / 6.0;
            h = (h - System.Math.Floor(h)) * 360.0;
        }
    }

    /// <summary>The cap's state this map (set by Colour56's Update through ForMap) and its one hook (TravelBlurFeature.AddRenderPasses).</summary>
    public static class Cap57
    {
        const string P = "design.n56.colour56.numbers.cap.";

        /// <summary>The cap this map (>= 1: off, the pass never enqueued).</summary>
        public static float Value = 1f;
        public static string Level = "";
        /// <summary>The role arcs (deg; lo, hi pairs: the shader takes three) and the feather (deg).</summary>
        public static Vector4 ArcsA, ArcsB;
        public static float Feather;
        /// <summary>yellow56 (NIGHT57 V4): the dim yellow's cap on (design.n56.yellow56.numbers.dimYellow; the row on, no -skygrindOff57) and its
        /// numbers: Yellow = (the S cap, the value it is whole at or under, the value it is none at or over, 1), YellowArc = (lo, hi, feather, 0).</summary>
        public static bool YellowOn;
        public static Vector4 Yellow, YellowArc;
        public static string YellowLine = "";
        /// <summary>The a357 reel's A/B: the frame without the cap (one frame, then back).</summary>
        public static bool Hide;
        /// <summary>Telemetry (the reel's proof the input fired): passes recorded on the main camera, and the last frame one was.</summary>
        public static int Recorded, LastFrame = -1;
        public static string Line = "";

        static Cap57Pass pass;

        /// <summary>NIGHT56's colour56 had no cap: any off switch at 57 or lower turns the cap off (colour56's own switch, 56, turns both off).</summary>
        static bool OffSwitch() => Off57();

        /// <summary>Any NIGHT57 change inside colour56 / yellow56 (the cap, the frieze's luminance, the walls' luminance, the accents' channel) is off
        /// with -skygrindOff57 or lower (or the process switch): NIGHT56's rows exactly.</summary>
        public static bool Off57()
        {
            if (NightRows.ForceOffFrom <= 57) return true;
            try { return NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 57; }
            catch (System.Exception) { return false; }
        }

        /// <summary>Colour56's Update, once per new map: the level's cap when colour56 is on and the level has one; else off.</summary>
        public static void ForMap(GameConfig cfg, string levelId)
        {
            Value = 1f;
            YellowOn = false;
            Yellow = YellowArc = Vector4.zero;
            string key = Decor54.KeyOf(levelId ?? "");
            Level = levelId ?? "";
            if (cfg == null || !Colour56.On(cfg)) Line = $"[cap57] {Level}: colour56 off: no cap (NIGHT56's frame)";
            else if (OffSwitch()) Line = $"[cap57] {Level}: -skygrindOff57 or lower: no cap (NIGHT56's colour56)";
            else if (!cfg.Has(P + "levels." + key)) Line = $"[cap57] {Level}: colour56 on, no cap for '{key}' (nothing capped)";
            else
            {
                double[] arcs = cfg.Nums(P + "roleArcs");
                ArcsA = new Vector4((float)arcs[0], (float)arcs[1], (float)arcs[2], (float)arcs[3]);
                ArcsB = new Vector4((float)arcs[4], (float)arcs[5], 0, 0);
                Feather = (float)cfg.Num(P + "featherDeg");
                Value = (float)cfg.Num(P + "levels." + key);
                Line = $"[cap57] {Level}: colour56 on: the world's saturation capped at S {Value:0.00} outside the role arcs {arcs[0]:0}-{arcs[1]:0}, {arcs[2]:0}-{arcs[3]:0}, {arcs[4]:0}-{arcs[5]:0} deg (feather {Feather:0} deg): hue and luminance kept (the greyscale frame unchanged)";
            }
            Debug.Log(Line);
            // NIGHT57 V4 yellow56: the dim yellow (design.n56.yellow56.numbers.dimYellow), its own row
            const string Y = "design.n56.yellow56.numbers.dimYellow.";
            if (cfg == null || !Yellow56.On(cfg)) YellowLine = $"[cap57] {Level}: yellow56 off: no dim-yellow cap";
            else if (OffSwitch()) YellowLine = $"[cap57] {Level}: -skygrindOff57 or lower: no dim-yellow cap (NIGHT56's yellow56)";
            else if (!cfg.Has(Y + "sMax")) YellowLine = $"[cap57] {Level}: yellow56 on, no dimYellow numbers";
            else
            {
                YellowOn = true;
                Yellow = Rim58.Yellow58(cfg, new Vector4((float)cfg.Num(Y + "sMax"), (float)cfg.Num(Y + "vFull"), (float)cfg.Num(Y + "vNone"), 1)); // NIGHT58 V2: dimYellow58's bounds
                YellowArc = Yellow59.Keep(cfg, new Vector4((float)cfg.Num(Y + "hueLo"), (float)cfg.Num(Y + "hueHi"), (float)cfg.Num(Y + "featherDeg"), 0)); // NIGHT59 V2: w = dimYellow58.sKeep
                YellowLine = $"[cap57] {Level}: yellow56 on: a DIM yellow (hue {YellowArc.x:0}-{YellowArc.y:0}, value under {Yellow.y:0.00}, none from {Yellow.z:0.00}) held at S {Yellow.x:0.00}: the bright parry tells keep theirs{Yellow59.LineOf(YellowArc)}";
            }
            Debug.Log(YellowLine);
        }

        /// <summary>yellow56's numbers as Cap57Math.Apply takes them (null: off).</summary>
        public static double[] YellowArgs() => YellowOn ? new double[] { Yellow.x, Yellow.y, Yellow.z, YellowArc.x, YellowArc.y, YellowArc.z, YellowArc.w, Yellow59.VMin } : null;

        /// <summary>The cap runs this frame for this camera (the game's main camera only; never the viewmodel's overlay, a scene view or a preview).</summary>
        public static bool Live(Camera cam)
        {
            if ((Value >= 1f && !YellowOn) || Hide || cam == null) return false;
            SurvivalGame g = SurvivalGame.Instance;
            return g != null && cam == g.cam;
        }

        /// <summary>TravelBlurFeature.AddRenderPasses (base cameras), after the CRT's line: the cap is the last pass on the world's colour.</summary>
        public static void Enqueue(ScriptableRenderer renderer, Camera cam)
        {
            if (renderer == null || cam == null || Hide) return;
            if (Live(cam))
            {
                if (pass == null) pass = new Cap57Pass(false);
                if (pass.Ready()) renderer.EnqueuePass(pass);
                return;
            }
            // NIGHT57 V4 yellow56: the viewmodel's overlay camera draws the gun after the main camera's pass (n56-r5 spire: the gun's dim yellow
            // 0.13-4.27 % of the frame, lit by the level's gold light); while the dim yellow is on, the same pass runs once more on that camera with
            // the dim yellow only (_Cap57.x = 1: colour56's world cap never touches the gun), on the stack's target after the gun is drawn (the world
            // already capped: both rules are idempotent). The HUD's overlay canvas draws after every camera.
            SurvivalGame g = SurvivalGame.Instance;
            if (!YellowOn || g == null || g.viewmodel == null || cam != g.viewmodel.Camera) return;
            if (vmPass == null) vmPass = new Cap57Pass(true);
            if (vmPass.Ready()) renderer.EnqueuePass(vmPass);
        }

        static Cap57Pass vmPass;
        /// <summary>Telemetry: passes recorded on the viewmodel's overlay camera (yellow56's dim yellow on the gun).</summary>
        public static int RecordedVm;
    }

    /// <summary>The cap's render pass (Crt29Pass's render-graph recipe: a copy pass, then the main pass through the material).</summary>
    public sealed class Cap57Pass : ScriptableRenderPass
    {
        Material material;
        bool warned;
        static readonly int P0 = Shader.PropertyToID("_Cap57");
        static readonly int PA = Shader.PropertyToID("_Cap57A");
        static readonly int PB = Shader.PropertyToID("_Cap57B");
        static readonly int PY = Shader.PropertyToID("_Cap57Y");
        static readonly int PYH = Shader.PropertyToID("_Cap57YH");

        class PassData
        {
            public Material material;
            public TextureHandle source;
        }

        readonly bool yellowOnly;

        /// <summary>yellowOnly: the viewmodel camera's pass (yellow56's dim yellow alone). The event is one after AfterRenderingPostProcessing, so on
        /// the main camera the cap runs after the travel blur and the CRT (enqueued at AfterRenderingPostProcessing) whatever the enqueue order.</summary>
        public Cap57Pass(bool yellowOnly)
        {
            this.yellowOnly = yellowOnly;
            profilingSampler = new ProfilingSampler(yellowOnly ? "SKYGRIND.Cap57.Viewmodel" : "SKYGRIND.Cap57");
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing + 1;
            requiresIntermediateTexture = true;
        }

        /// <summary>The material (SKYGRIND/Cap57, in a Resources folder); false (one warning) when the shader isn't in the build.</summary>
        public bool Ready()
        {
            if (material != null) return true;
            Shader sh = Shader.Find("SKYGRIND/Cap57");
            if (sh == null)
            {
                if (!warned)
                {
                    warned = true;
                    Debug.LogWarning("Cap57: shader SKYGRIND/Cap57 not found (Assets/Shaders/Resources/Cap57.shader); the world saturation cap stays off.");
                }
                return false;
            }
            material = CoreUtils.CreateEngineMaterial(sh);
            return true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null) return;
            UniversalResourceData resources = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (resources.isActiveTargetBackBuffer) return; // needs the intermediate colour (requiresIntermediateTexture)
            material.SetVector(P0, new Vector4(yellowOnly ? 1f : Cap57.Value, Cap57.Feather, UnityTuning.Cap57Srgb, Yellow59.VMin)); // w: NIGHT59 V2's keep floor
            material.SetVector(PA, Cap57.ArcsA);
            material.SetVector(PB, Cap57.ArcsB);
            material.SetVector(PY, Cap57.YellowOn ? Cap57.Yellow : Vector4.zero);
            material.SetVector(PYH, Cap57.YellowArc);
            if (yellowOnly) Cap57.RecordedVm++;
            else
            {
                Cap57.Recorded++;
                Cap57.LastFrame = Time.frameCount;
            }
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            TextureHandle copy = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_Cap57Copy", false);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Cap57 copy", out PassData data, profilingSampler))
            {
                data.source = resources.activeColorTexture;
                builder.UseTexture(data.source, AccessFlags.Read);
                builder.SetRenderAttachment(copy, 0, AccessFlags.Write);
                builder.SetRenderFunc(static (PassData p, RasterGraphContext ctx) => Blitter.BlitTexture(ctx.cmd, p.source, new Vector4(1, 1, 0, 0), 0, false));
            }
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Cap57", out PassData data, profilingSampler))
            {
                data.source = copy;
                data.material = material;
                builder.UseTexture(copy, AccessFlags.Read);
                builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderFunc(static (PassData p, RasterGraphContext ctx) => Blitter.BlitTexture(ctx.cmd, p.source, new Vector4(1, 1, 0, 0), p.material, 0));
            }
        }
    }
}
