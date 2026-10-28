// NIGHT31 builder G (item 1b / 1d; design.n31.grappleMark, a view row: no sim bit). THE PULL-IN GRAPPLE's look:
// - THE TARGET MARK: a hollow cyan diamond (rule 8: cyan is player-usable; never red-orange) on the point a grapple press would take THIS frame (an
//   enemy's aim point, a node, a surface: Arcline21.Peek, the pick's own tests), drawn only while the ARCLINE is owned and ready (rope idle, off its
//   cooldown). Its size is an angle from the eye, so it reads the same near and far; a node's diamond frames the node's ring (GrappleNodes30).
// - THE PULL: while a tap pulls you in (SurvivalSim.grappleTap31.Pulling) the camera's FOV kicks wider (UnityTuning.Grapple31FovKickDeg over
//   Grapple31FovInSeconds, x FOV KICK %, none with REDUCED MOTION), and bright beads run down the rope toward the player at the pull speed (the rope
//   itself is Arcline21View's: it shortens as you close in).
// View only; installs itself (no edit in the lead's SurvivalGame files: the FOV is added in LateUpdate, after SurvivalGame.Update's camera pass wrote it
// fresh this frame); Unity objects are made on first use (never in field initialisers). Off: the row off, or -skygrindOff31 (N31Blocks.ForceOff).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Grapple31View : MonoBehaviour
    {
        /// <summary>For the reels: the mark drawn this frame, what it marks and where (world), the FOV kick added (deg), the beads drawn.</summary>
        public static bool MarkShown;
        /// <summary>NIGHT34 nomark34: the diamond is actually drawn this frame (its renderer active). MarkShown stays the "would show" bit (the
        /// latch target exists), so the NIGHT31 grapple reel keeps its meaning; with design.n34.nomark34 on, MarkDrawn is always false.</summary>
        public static bool MarkDrawn;
        public static Arcline21Kind MarkKind;
        public static Vector3 MarkWorld;
        public static float FovKickNow;
        public static int BeadsShown;

        SurvivalGame game;
        Transform root, mark;
        Renderer markR;
        MeshFilter markFilter;
        readonly Dictionary<int, Mesh> markMeshes = new Dictionary<int, Mesh>(); // round 2: one outline mesh per band share (hundredths), made on first use
        Material mat;
        MaterialPropertyBlock block;
        int baseColorId;
        readonly List<LineRenderer> beads = new List<LineRenderer>();
        float kick, beadPhase, t;
        bool rowOn, rowRead, noMark34;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Grapple31View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Grapple31View>();
        }

        void Awake()
        {
            block = new MaterialPropertyBlock();
            baseColorId = Shader.PropertyToID("_BaseColor");
        }

        /// <summary>A hollow diamond (a rhombus outline) of half-size 1 in the XY plane, both faces.</summary>
        static Mesh Diamond(float band, string name = "grapple mark 31")
        {
            var k = new MeshKit();
            Vector3[] o = { new Vector3(0, 1), new Vector3(1, 0), new Vector3(0, -1), new Vector3(-1, 0) };
            float ri = 1 - band;
            for (int i = 0; i < 4; i++)
            {
                Vector3 o0 = o[i], o1 = o[(i + 1) % 4], i0 = o0 * ri, i1 = o1 * ri;
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
            beads.Clear();
            root = new GameObject("Grapple 31").transform;
            DontDestroyOnLoad(root.gameObject);
            if (mat != null) Destroy(mat);
            mat = FxDraw.Additive(game.mats.neon, "Grapple mark 31");
            var go = new GameObject("mark");
            go.transform.SetParent(root, false);
            markFilter = go.AddComponent<MeshFilter>();
            markFilter.sharedMesh = Diamond(UnityTuning.Grapple31MarkBand);
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            mark = go.transform;
            markR = r;
            go.SetActive(false);
            float w = (float)game.cfg.Num("arcline.ropeWidth") * UnityTuning.Arcline21RopeWidthMul * UnityTuning.Grapple31BeadWidthMul;
            for (int i = 0; i < UnityTuning.Grapple31Beads; i++)
            {
                var b = new GameObject("bead " + i).AddComponent<LineRenderer>();
                b.transform.SetParent(root, false);
                b.useWorldSpace = true;
                b.alignment = LineAlignment.View;
                b.positionCount = 2;
                b.widthMultiplier = w;
                b.sharedMaterial = mat;
                b.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                b.receiveShadows = false;
                b.enabled = false;
                beads.Add(b);
            }
            rowRead = false;
            return true;
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
            if (!rowRead)
            {
                rowOn = !N31Blocks.ForceOff && game.cfg.Design("n31.grappleMark.enabled");
                noMark34 = N34Fixes.ViewRowOn(game.cfg, "nomark34"); // NIGHT34: the node ring (GrappleNodes30) is the only grapple read
                rowRead = true;
            }
            SurvivalSim sim = game.sim;
            float dt = Time.deltaTime;
            t += dt;
            Arcline21 a = sim != null ? sim.arcline21 : null;
            bool live = rowOn && a != null && sim.n21.arcline && a.Owned && game.cam != null;
            Color cyan = Look.C(game.cfg, "colors.usable");

            // the mark
            Arcline21Kind kind = Arcline21Kind.None;
            Vec3 p = default;
            if (live) kind = a.Peek(new InputFrame { yaw = sim.input.Yaw, pitch = sim.input.Pitch }, out p);
            bool show = kind != Arcline21Kind.None;
            bool draw = show && !noMark34; // NIGHT34 nomark34: the target is still read (MarkShown / MarkKind / MarkWorld), never drawn
            if (mark.gameObject.activeSelf != draw) mark.gameObject.SetActive(draw);
            MarkShown = show;
            MarkDrawn = draw && markR != null && markR.enabled && mark.gameObject.activeInHierarchy;
            MarkKind = kind;
            if (show)
            {
                Vector3 eye = game.cam.transform.position;
                Vector3 at = WebSpace.ToUnity(p);
                Vector3 to = at - eye;
                float d = to.magnitude;
                if (d > 1e-3f)
                {
                    Vector3 dir = to / d;
                    float lift = Mathf.Min(UnityTuning.Grapple31MarkLift, d * 0.5f);
                    at -= dir * lift;
                    float scale = kind == Arcline21Kind.Node ? UnityTuning.Grapple31MarkNodeScale : kind == Arcline21Kind.Enemy ? UnityTuning.Grapple31MarkEnemyScale : 1f;
                    float half = (d - lift) * Mathf.Tan(UnityTuning.Grapple31MarkAngleDeg * Mathf.Deg2Rad) * scale;
                    if (kind == Arcline21Kind.Node) half = Mathf.Max(half, UnityTuning.Grapple31MarkNodeFrame); // round 2: around the ring, never inside it
                    // round 2: the outline keeps about the same thickness on screen (UnityTuning.Grapple31MarkLineDeg) whatever the diamond's size
                    float share = Mathf.Clamp((d - lift) * Mathf.Tan(UnityTuning.Grapple31MarkLineDeg * Mathf.Deg2Rad) / Mathf.Max(1e-3f, half), UnityTuning.Grapple31MarkBandMin, UnityTuning.Grapple31MarkBand);
                    int key = Mathf.RoundToInt(share * 100);
                    if (!markMeshes.TryGetValue(key, out Mesh want)) markMeshes[key] = want = Diamond(key / 100f, "grapple mark 31 " + key);
                    if (markFilter.sharedMesh != want) markFilter.sharedMesh = want;
                    mark.position = at;
                    mark.rotation = Quaternion.LookRotation(dir, game.cam.transform.up);
                    mark.localScale = new Vector3(half, half, half);
                }
                MarkWorld = at;
                float pulse = 1 + UnityTuning.Grapple31MarkPulseDepth * Mathf.Sin(t * Mathf.PI * 2 * UnityTuning.Grapple31MarkPulseHz);
                Tint(markR, cyan * (UnityTuning.Grapple31MarkGain * pulse));
            }

            // the pull: FOV kick and the beads
            bool pulling = rowOn && sim != null && sim.n31.grapplePull && sim.grappleTap31.Pulling && sim.mover.rope.phase != RopePhase.None;
            float target = pulling ? 1f : 0f;
            float rate = pulling ? 1f / UnityTuning.Grapple31FovInSeconds : 1f / UnityTuning.Grapple31FovOutSeconds;
            kick = Mathf.MoveTowards(kick, target, dt * rate);
            bool calm = game.cfg.movement.settings.reducedMotion;
            float add = calm ? 0f : kick * UnityTuning.Grapple31FovKickDeg * (float)CameraFeel.FovKickScale(game.settings);
            if (game.cam != null && add > 0) game.cam.fieldOfView += add;
            FovKickNow = add;

            int shown = 0;
            if (pulling && sim.mover.rope.phase == RopePhase.Attached && !Grapple47.On(game.cfg)) // NIGHT47 F grapple47: no beads (the ribbon's pulse says it)
            {
                Rope r = sim.mover.rope;
                Vector3 chest = game.cam.transform.position + Vector3.down * 0.4f;
                Vector3 anchor = WebSpace.ToUnity(r.anchor);
                Vector3 run = chest - anchor;
                float len = run.magnitude;
                if (len > 0.5f)
                {
                    Vector3 u = run / len;
                    float speed = (float)game.cfg.movement.arcline.zip.reelSpeed;
                    beadPhase = (beadPhase + dt * speed) % UnityTuning.Grapple31BeadSpacing;
                    for (int i = 0; i < beads.Count; i++)
                    {
                        float s0 = beadPhase + i * UnityTuning.Grapple31BeadSpacing; // metres from the anchor toward the chest
                        if (s0 > len - 0.3f) { beads[i].enabled = false; continue; }
                        float s1 = Mathf.Min(len - 0.3f, s0 + UnityTuning.Grapple31BeadLength);
                        beads[i].SetPosition(0, anchor + u * s0);
                        beads[i].SetPosition(1, anchor + u * s1);
                        beads[i].startColor = beads[i].endColor = Color.white;
                        Tint(beads[i], cyan * UnityTuning.Grapple31BeadGain);
                        beads[i].enabled = true;
                        shown++;
                    }
                }
            }
            for (int i = shown == 0 ? 0 : beads.Count; i < beads.Count; i++) beads[i].enabled = false;
            BeadsShown = shown;
        }
    }
}
