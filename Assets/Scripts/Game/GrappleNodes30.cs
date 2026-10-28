// NIGHT30 builder V (item 10's view; design.n30.span; the lead's r5 frame s2: "G GRAPPLE THE RING." with no ring to see). THE ARCLINE's GRAPPLE NODES in a
// campaign level as RINGS: every node of the sim's own list (sim.map.grappleNodes: the level JSON's `grappleNodes`, plus THE LOOM's moving joints, which B adds
// to the list for the fight and moves each step) gets a cyan ring (player-usable, rule 8) about 1.2 m across, turned to face the camera, with a soft wider glow
// ring behind it; brighter when in the ARCLINE's reach (movement.arcline.range), brightest on the one the crosshair would hook (inside half the cone: the web's
// choose() test, without the line-of-sight ray). NIGHT21's Arcline21View draws a small cube per node, built ONCE at the scene's start and only while the gadget
// is owned: it never followed the Loom's joints and showed nothing a new player reads as "a ring": this view adds the rings and leaves that view as it is.
// View only; installs itself (no edit in the lead's SurvivalGame files); Unity objects are made in Awake / on first use (never in field initialisers).
// Off: -skygrindOff30, or the run's span block off, or a map without a campaign block.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class GrappleNodes30 : MonoBehaviour
    {
        /// <summary>For the reels: rings drawn this frame, the ones in reach, and whether one is aimed at.</summary>
        public static int RingsShown, RingsInReach;
        public static bool RingAimed;

        SurvivalGame game;
        Transform root;
        Mesh ringMesh, glowMesh;
        Material mat;
        MaterialPropertyBlock block;
        int baseColorId;
        bool thin31; // NIGHT31 G: the thin ring with design.n31.grappleMark on
        readonly List<Transform> ringT = new List<Transform>();
        readonly List<Renderer> ringR = new List<Renderer>(), glowR = new List<Renderer>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("GrappleNodes30") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<GrappleNodes30>();
        }

        void Awake()
        {
            block = new MaterialPropertyBlock();
            baseColorId = Shader.PropertyToID("_BaseColor");
        }

        /// <summary>An annulus in the XY plane (facing +z / -z), `segments` steps.</summary>
        static Mesh Annulus(float rIn, float rOut, int segments, string name)
        {
            var k = new MeshKit();
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2 / segments, a1 = (i + 1) * Mathf.PI * 2 / segments;
                Vector3 i0 = new Vector3(Mathf.Cos(a0), Mathf.Sin(a0)) * rIn, o0 = new Vector3(Mathf.Cos(a0), Mathf.Sin(a0)) * rOut;
                Vector3 i1 = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1)) * rIn, o1 = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1)) * rOut;
                k.Quad(i0, o0, o1, i1, Vector3.back, Color.white);
                k.Quad(i1, o1, o0, i0, Vector3.forward, Color.white);
            }
            return k.Build(name);
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.mats == null || game.cfg == null) { game = null; return false; }
            if (root != null) Destroy(root.gameObject);
            ringT.Clear(); ringR.Clear(); glowR.Clear();
            root = new GameObject("Grapple rings 30").transform;
            DontDestroyOnLoad(root.gameObject);
            if (mat != null) Destroy(mat);
            mat = FxDraw.Additive(game.mats.neon, "Grapple ring 30");
            // NIGHT31 G (design.n31.grappleMark; the lead's r2 frame ring31 f0028: at 4.5 m the node read as "a cyan rounded block", not a ring): with the mark's row on
            // the ring is a THIN outline (UnityTuning.Grapple31Ring*: a narrower band, a short faint glow, a lower aimed gain), and Grapple31View's diamond frames it.
            thin31 = N31Fixes.ViewRowOn(game.cfg, "grappleMark");
            float band = thin31 ? UnityTuning.Grapple31RingBand : UnityTuning.Grapple30RingBand, grow = thin31 ? UnityTuning.Grapple31RingGlowGrow : UnityTuning.Grapple30GlowGrow;
            ringMesh ??= Annulus(UnityTuning.Grapple30RingRadius - band, UnityTuning.Grapple30RingRadius, UnityTuning.Grapple30RingSteps, "grapple ring 30");
            glowMesh ??= Annulus(UnityTuning.Grapple30RingRadius - band, UnityTuning.Grapple30RingRadius + grow, UnityTuning.Grapple30RingSteps, "grapple glow 30");
            return true;
        }

        void Grow(int n)
        {
            while (ringT.Count < n)
            {
                var go = new GameObject("ring " + ringT.Count);
                go.transform.SetParent(root, false);
                go.AddComponent<MeshFilter>().sharedMesh = ringMesh;
                var r = go.AddComponent<MeshRenderer>();
                r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var g = new GameObject("glow");
                g.transform.SetParent(go.transform, false);
                g.AddComponent<MeshFilter>().sharedMesh = glowMesh;
                var gr = g.AddComponent<MeshRenderer>();
                gr.sharedMaterial = mat;
                gr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                ringT.Add(go.transform);
                ringR.Add(r);
                glowR.Add(gr);
            }
        }

        void Tint(Renderer r, Color c)
        {
            block.Clear();
            block.SetColor(baseColorId, c);
            r.SetPropertyBlock(block);
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            SurvivalSim sim = game.sim;
            List<Vec3> nodes = sim != null && sim.map != null ? sim.map.grappleNodes : null;
            // a campaign MAP (its json carries `campaign`), whatever the run's mode: the reels walk levels in other modes too (r5's s2 frame had no node drawn at all)
            bool campaignMap = game.map != null && game.map.json != null && game.map.json.ContainsKey("campaign");
            bool on = N30Fixes.On && sim != null && sim.n30.span && campaignMap && nodes != null && nodes.Count > 0 && game.cam != null;
            int n = on ? nodes.Count : 0;
            if (n > ringT.Count) Grow(n);
            RingsShown = n;
            RingsInReach = 0;
            RingAimed = false;
            if (on)
            {
                ArclineConfig a = game.cfg.movement.arcline;
                Vector3 eye = game.cam.transform.position, fwd = game.cam.transform.forward;
                double cosHalf = Math.Cos(a.coneDeg * Math.PI / 180 * 0.5);
                int aimed = -1;
                double best = cosHalf;
                for (int i = 0; i < n; i++)
                {
                    Vector3 p = WebSpace.ToUnity(nodes[i]);
                    Vector3 to = p - eye;
                    float d = to.magnitude;
                    if (d > a.range || d < 0.5f) continue;
                    double c = Vector3.Dot(to / d, fwd);
                    if (c > best) { best = c; aimed = i; }
                }
                Color cyan = Look.C(game.cfg, "colors.usable");
                float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * Mathf.PI * 2 * UnityTuning.Grapple30PulseHz);
                for (int i = 0; i < n; i++)
                {
                    Transform t = ringT[i];
                    if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                    Vector3 p = WebSpace.ToUnity(nodes[i]);
                    t.position = p;
                    Vector3 face = p - eye;
                    if (face.sqrMagnitude > 1e-6f) t.rotation = Quaternion.LookRotation(face.normalized, Vector3.up);
                    bool reach = (p - eye).magnitude <= a.range;
                    if (reach) RingsInReach++;
                    float g = i == aimed ? (thin31 ? UnityTuning.Grapple31RingAimedGain : UnityTuning.Grapple30AimedGain) : reach ? UnityTuning.Grapple30ReachGain : UnityTuning.Grapple30FarGain;
                    Tint(ringR[i], cyan * (g * pulse));
                    Tint(glowR[i], cyan * (g * (thin31 ? UnityTuning.Grapple31RingGlowShare : UnityTuning.Grapple30GlowShare) * pulse));
                }
                RingAimed = aimed >= 0;
            }
            for (int i = n; i < ringT.Count; i++) if (ringT[i].gameObject.activeSelf) ringT[i].gameObject.SetActive(false);
        }
    }
}
