// NIGHT74 builder L (pads74; the lead's 2:37 job): the jump pad's particle stream steadied, view only. NIGHT74's moving-camera flicker meter (Pan74.cs, Tools/night/n74-flick.py)
// put the pad columns among the hottest blocks of the n74-t1 / t2 heat maps. The shipped stream (JumpPadView27.MakeStream, UnityTuning.Pad27Stream*) is hard-edged squares of the
// additive HDR cyan neon material (URP Unlit ignores particle colours and the quads have no texture): 0.13 m at the biggest, 0.026 m at birth, shrinking to nothing at death, so at
// 15-30 m each is 1-4 px, and a quad that small gains and loses pixel centres as the camera turns. THE STEADY TWIN (a second ParticleSystem on every pad) has
//   - fewer motes (5 a second, not 12), bigger (0.34 m), slower (1.5 m/s, the same 3 m of rise over 2 s),
//   - a soft sprite sheet (a strip of 32 frames of one blob, each frame dimmer or brighter than the last: Pad74Math.FrameLevel) instead of a flat square, so the edge has no step,
//   - a size that never goes below 0.6 of its peak (the light fades through the sheet, not through the size), so a mote is never sub-pixel,
//   - a fixed random seed per pad: the same motes in the same order every run (two runs of one build draw the same stream; the shipped stream's seed is random).
// THE SWITCH is read every frame (NightRows.ViewOn, row pads74; the row belongs in SameRun63Math.FrameRows for a same-run pair). Both streams simulate all the time and the launch
// burst feeds both; only one renders: the row off, the shipped stream's renderer is on and the twin's is off, so the picture is exactly the shipped one. Nothing here touches the sim or
// the sim's timers; no audio; no new gameplay number (UnityTuning.N74Pad*, each with its reason). The pure parts are Pad74Math.cs.
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>The view parts of pads74: the sprite sheet's texture, the steady material and stream, and the per-frame choice of which stream renders.</summary>
    public static class Pad74View
    {
        static Texture2D sheet;
        static Material cachedFor, steadyMat;

        /// <summary>The reel switch that turns the BEAM part of the row off while the stream part stays (a diagnostic for the decomposition: one ON side with it, one without).</summary>
        public const string NoBeamSwitch = "-skygrindPad74NoBeam";

        static int beamPart = -1;

        /// <summary>Whether the row's beam softening applies when the row is on (the command line read once: -skygrindPad74NoBeam turns it off).</summary>
        public static bool BeamPart
        {
            get
            {
                if (beamPart < 0) beamPart = Skygrind.Core.NightRows.HasSwitch(System.Environment.GetCommandLineArgs(), NoBeamSwitch) ? 0 : 1;
                return beamPart == 1;
            }
        }

        /// <summary>The sprite sheet's texture (made once): RGBA32 linear (the profile is light, not a colour), trilinear, clamped, with its mip chain (mip 5 is exactly one frame), readable (a test reads it).</summary>
        public static Texture2D Sheet()
        {
            if (sheet != null) return sheet;
            int n = UnityTuning.N74PadSpriteTexels, f = UnityTuning.N74PadSpriteFrames;
            var t = new Texture2D(n * f, n, TextureFormat.RGBA32, true, true) { name = "Pad stream sheet 74", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, anisoLevel = 0 };
            t.SetPixelData(Pad74Math.Sheet(n, f, UnityTuning.N74PadFadeIn, UnityTuning.N74PadFadeOut), 0);
            t.Apply(true, false);
            sheet = t;
            return t;
        }

        /// <summary>The steady stream's material: a copy of the pad's additive glow material (its cyan HDR colour kept, times the tint) with the sprite sheet as its base map. One per glow material.</summary>
        public static Material StreamMaterial(Material glow)
        {
            if (glow == null) return null;
            if (cachedFor == glow && steadyMat != null) return steadyMat;
            var m = new Material(glow) { name = "Pad stream steady 74", enableInstancing = true };
            m.SetTexture("_BaseMap", Sheet());
            Color c = glow.HasProperty("_BaseColor") ? glow.GetColor("_BaseColor") : Color.white;
            float k = UnityTuning.N74PadStreamTint;
            m.SetColor("_BaseColor", new Color(c.r * k, c.g * k, c.b * k, 1f));
            cachedFor = glow;
            steadyMat = m;
            return m;
        }

        /// <summary>The steady twin of the pad's stream (JumpPadView27.MakeStream's shape: a cone of angle 0 pointing up from the chevrons' height, local space), built stopped and then played. Its renderer starts OFF.</summary>
        public static ParticleSystem MakeSteady(Transform parent, float r, int index, Material glow)
        {
            var go = new GameObject("Stream steady 74");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, UnityTuning.Pad27ChevronY, 0);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true; main.playOnAwake = true;
            ps.useAutoRandomSeed = false; // (a ParticleSystem member, not the main module's; set while stopped)
            ps.randomSeed = Pad74Math.SeedFor(UnityTuning.N74PadSeed, index);
            main.startLifetime = UnityTuning.N74PadStreamLife;
            main.startSpeed = UnityTuning.N74PadStreamSpeed;
            main.startSize = UnityTuning.N74PadStreamSize;
            main.maxParticles = UnityTuning.N74PadStreamMax;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startColor = Color.white;
            var em = ps.emission; em.rateOverTime = UnityTuning.N74PadStreamRate;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 0f; sh.radius = r * 0.7f; sh.rotation = new Vector3(-90f, 0f, 0f);
            var sz = ps.sizeOverLifetime; sz.enabled = true;
            float floor = UnityTuning.N74PadSizeFloor;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, floor), new Keyframe(0.25f, 1f), new Keyframe(1f, floor)));
            var tsa = ps.textureSheetAnimation;
            tsa.enabled = true;
            tsa.mode = ParticleSystemAnimationMode.Grid;
            tsa.numTilesX = UnityTuning.N74PadSpriteFrames;
            tsa.numTilesY = 1;
            tsa.animation = ParticleSystemAnimationType.WholeSheet;
            tsa.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
            tsa.startFrame = 0f;
            tsa.cycleCount = 1;
            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = StreamMaterial(glow);
            rend.shadowCastingMode = ShadowCastingMode.Off;
            rend.receiveShadows = false;
            rend.enabled = false; // the row decides, every frame (Apply)
            ps.Play();
            return ps;
        }

        /// <summary>Which stream renders this frame: the steady twin when the row is on, the shipped stream when it is off. Both keep simulating (a disabled renderer does not stop a ParticleSystem), so a frame the row flips on or off draws the stream's state as it was.</summary>
        public static void Apply(bool on, ParticleSystemRenderer shipped, ParticleSystemRenderer steady)
        {
            if (shipped != null && shipped.enabled == on) shipped.enabled = !on;
            if (steady != null && steady.enabled != on) steady.enabled = on;
        }
    }
}
