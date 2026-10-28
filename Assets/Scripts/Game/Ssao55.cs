// NIGHT55 builder C ssao55 (design.n55.ssao55, a VIEW row: NightRows.ViewOn(cfg, "ssao55"); off / -skygrindOff55 = NIGHT54's SSAO exactly).
// Slate P0-ssao / OPEN O-05: SSAO's noise without TAA. The cause (read in URP 17's ScreenSpaceAmbientOcclusionPass.cs, Research/2026-10-20-
// wave55/github.md item 2): with AOMethod Blue Noise (PC_Renderer's) the pass moves to the next of its blue-noise textures EVERY frame and
// offsets it by Random.value twice (SetupKeywordsAndParameters, called from its RecordRenderGraph); nothing averages the per-frame pattern
// without TAA, so the occlusion crawls on a still camera. The fix here is a runtime one (no asset edit, no new variant): with the row on, a
// pass recorded AFTER the SSAO pass in the same camera's render graph (RenderPassEvent.BeforeRenderingOpaques; SSAO records at
// AfterRenderingPrePasses + 1, or BeforeRenderingTransparents with AfterOpaque) puts back the first blue-noise texture the SSAO material was
// given and a fixed offset (Ssao55NoiseOffset) into its _BlueNoiseTexture / _SSAOBlueNoiseParams.zw (the scale, .xy, is the pass's own for
// this camera). The render graph executes after the whole camera is recorded, so the draw reads the fixed values: the same noise per screen
// pixel every frame (what Interleaved Gradient Noise does, with the asset's own Blue Noise variant, radius, intensity and blur unchanged).
// Why not the two-feature switch (github.md's method): the IGN variant is stripped from the Player (PC_RPAsset m_PrefilterSSAOInterleaved 1,
// recomputed by URP's ShaderBuildPreprocessor from ACTIVE features only), so a second feature needs an asset edit in the live Editor before a
// build; this needs none, and keeps the look's numbers (no 0.3 -> 0.45 radius conversion).
// Hooked by one line in Render/TravelBlurFeature.cs AddRenderPasses (Ssao55.Enqueue). The a255 reel's SSAO part forces the three sides in one
// run (Force: FLOOR = the AO's intensity 0 after the record, the noise floor; OFF = untouched; ON = frozen).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    public static class Ssao55
    {
        public enum Mode { Row, Off, On, Floor }

        /// <summary>The reel's switch: Row (the config decides), or a forced side.</summary>
        public static Mode Force = Mode.Row;

        static readonly int NoiseTexId = Shader.PropertyToID("_BlueNoiseTexture");
        static readonly int NoiseParamsId = Shader.PropertyToID("_SSAOBlueNoiseParams");
        static readonly int ParamsId = Shader.PropertyToID("_SSAOParams");
        const string SsaoShader = "Hidden/Universal Render Pipeline/ScreenSpaceAmbientOcclusion";

        static FreezePass pass;
        static Material[] mats = new Material[0];
        static Texture[] fixedTex = new Texture[0];
        static int lookedFrame = -100000, rowFrame = -1;
        static bool rowOn;

        /// <summary>Telemetry (the reel): passes recorded that froze a blue-noise SSAO material, that zeroed one (FLOOR), the materials found.</summary>
        public static int Frozen, Floored, MaterialsFound;

        /// <summary>The row (once a frame): a game with its config and design.n55.ssao55 on (and no -skygrindOff55).</summary>
        public static bool RowOn
        {
            get
            {
                if (rowFrame != Time.frameCount)
                {
                    rowFrame = Time.frameCount;
                    SurvivalGame g = SurvivalGame.Instance;
                    rowOn = g != null && g.cfg != null && NightRows.ViewOn(g.cfg, "ssao55");
                }
                return rowOn;
            }
        }

        static Mode Now => Force != Mode.Row ? Force : Fix72.Off("ssao") ? Mode.Floor : RowOn ? Mode.On : Mode.Off; // NIGHT72 L2: -skygrindFlickerOff72=ssao takes the AO out (FLOOR) for the meter's ranking

        /// <summary>TravelBlurFeature.AddRenderPasses (every camera but the preview / scene ones): the after-SSAO pass when it has work.</summary>
        public static void Enqueue(ScriptableRenderer renderer)
        {
            Mode m = Now;
            if ((m == Mode.Off && !FeelClock.Holding) || renderer == null) return; // NIGHT63 D1: a same-run held frame replays the shot frame's noise
            if (pass == null) pass = new FreezePass();
            renderer.EnqueuePass(pass);
        }

        /// <summary>The SSAO materials (found by their shader once, then again only while none is found, at most once a second).</summary>
        static void Find()
        {
            bool any = false;
            foreach (Material m in mats) if (m != null) { any = true; break; }
            if (any || Time.frameCount - lookedFrame < 60) return;
            lookedFrame = Time.frameCount;
            var found = new System.Collections.Generic.List<Material>();
            foreach (Material m in Resources.FindObjectsOfTypeAll<Material>())
                if (m != null && m.shader != null && m.shader.name == SsaoShader) found.Add(m);
            mats = found.ToArray();
            fixedTex = new Texture[mats.Length];
            saved = new float[mats.Length];
            for (int i = 0; i < saved.Length; i++) saved[i] = float.NaN;
            MaterialsFound = mats.Length;
        }

        static float[] saved = new float[0];

        /// <summary>The reel, after a FLOOR pass: the intensity put back (the SSAO pass re-sets _SSAOParams only when its settings change, so a
        /// zeroed value would otherwise stay).</summary>
        public static void EndFloor()
        {
            Force = Force == Mode.Floor ? Mode.Row : Force;
            for (int i = 0; i < mats.Length && i < saved.Length; i++)
            {
                if (mats[i] == null || float.IsNaN(saved[i])) continue;
                Vector4 p = mats[i].GetVector(ParamsId);
                p.x = saved[i];
                mats[i].SetVector(ParamsId, p);
                saved[i] = float.NaN;
            }
        }

        /// <summary>After the SSAO pass recorded this camera: the noise put back (ON) or the occlusion zeroed (FLOOR).</summary>
        static void Apply()
        {
            Mode m = Now;
            if (m == Mode.Off)
            {
                if (FeelClock.Holding) Replay63();
                return;
            }
            Find();
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null) continue;
                if (m == Mode.Floor)
                {
                    Vector4 p = mat.GetVector(ParamsId);
                    if (float.IsNaN(saved[i])) saved[i] = p.x;
                    p.x = 0; // the intensity: the pass draws no occlusion (the noise floor's side)
                    mat.SetVector(ParamsId, p);
                    Floored++;
                    continue;
                }
                if (!mat.IsKeywordEnabled("_BLUE_NOISE")) continue; // Interleaved Gradient (static already) or not set up yet
                Texture t = mat.GetTexture(NoiseTexId);
                if (fixedTex[i] == null) fixedTex[i] = t;
                if (fixedTex[i] != null && t != fixedTex[i]) mat.SetTexture(NoiseTexId, fixedTex[i]);
                Vector4 bn = mat.GetVector(NoiseParamsId);
                bn.z = UnityTuning.Ssao55NoiseOffsetX;
                bn.w = UnityTuning.Ssao55NoiseOffsetY;
                mat.SetVector(NoiseParamsId, bn);
                Frozen++;
            }
        }

        // --- NIGHT63 builder D (D1, same-run SPOT pairs): the held frame's SSAO noise ---------------------------------------------------------
        // With the row off (NIGHT62's SSAO, the config today) the SSAO pass moves to its next blue-noise texture and two new Random offsets EVERY
        // frame, so a held frame's occlusion would crawl against its shot frame's. The reel snapshots each SSAO material's noise at the shot
        // frame's end (Snapshot63, before FeelClock.BeginHold) and the held frame's pass puts it back (Replay63, from Apply): the same noise.
        static Texture[] heldTex = new Texture[0];
        static Vector4[] heldParams = new Vector4[0];

        /// <summary>Held frames whose SSAO noise was put back (the reel's telemetry).</summary>
        public static int Replayed63;

        /// <summary>FeelReel.SameRun63, at the end of the shot's frame: each blue-noise SSAO material's texture and noise parameters now (the
        /// ones that frame drew with).</summary>
        public static void Snapshot63()
        {
            Rescan63();
            heldTex = new Texture[mats.Length];
            heldParams = new Vector4[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null || !mat.IsKeywordEnabled("_BLUE_NOISE")) continue;
                heldTex[i] = mat.GetTexture(NoiseTexId);
                heldParams[i] = mat.GetVector(NoiseParamsId);
            }
        }

        /// <summary>NIGHT63 D1 residual (n63-b3: the no-op pairs' 0-16 px and the plant's extra 0-11 px all on the viewmodel gun, as speckle on its
        /// surface: the SSAO noise of a camera the snapshot missed, a hypothesis): every SSAO material now, not Find's first set (Find looks again
        /// only while it has none, so a material made later, such as another renderer's for the viewmodel's overlay camera, was never snapshotted
        /// and its noise moved on between the two held frames). The list is rebuilt only when the set changed.</summary>
        static void Rescan63()
        {
            var found = new System.Collections.Generic.List<Material>();
            foreach (Material m in Resources.FindObjectsOfTypeAll<Material>())
                if (m != null && m.shader != null && m.shader.name == SsaoShader) found.Add(m);
            bool same = found.Count == mats.Length;
            for (int i = 0; same && i < found.Count; i++) same = ReferenceEquals(found[i], mats[i]);
            if (same) return;
            mats = found.ToArray();
            fixedTex = new Texture[mats.Length];
            saved = new float[mats.Length];
            for (int i = 0; i < saved.Length; i++) saved[i] = float.NaN;
            MaterialsFound = mats.Length;
            lookedFrame = Time.frameCount;
        }

        static void Replay63()
        {
            for (int i = 0; i < mats.Length && i < heldTex.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null || heldTex[i] == null) continue;
                mat.SetTexture(NoiseTexId, heldTex[i]);
                mat.SetVector(NoiseParamsId, heldParams[i]);
                Replayed63++;
            }
        }

        /// <summary>A pass that draws nothing: its RecordRenderGraph runs after the SSAO pass's in the camera's recording and sets the material.</summary>
        sealed class FreezePass : ScriptableRenderPass
        {
            public FreezePass()
            {
                profilingSampler = new ProfilingSampler("SKYGRIND.Ssao55");
                renderPassEvent = RenderPassEvent.BeforeRenderingOpaques; // after SSAO's AfterRenderingPrePasses + 1 (AfterOpaque 0 in PC_Renderer)
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) => Apply();
        }
    }
}
