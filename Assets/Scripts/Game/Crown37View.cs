// NIGHT37 builder C (crown37): what the player SEES of THE CROWN's descent. View only (no collider, no sim change). Drawn whenever the run has
// the pieces (sim.orbit37 non-null: the crown37 row on, a level carrying spire37 / fall37 rows):
//  - THE PIECES (Core/Orbit37.cs): a stone slab at the sim's position (interpolated by the game's alpha) with cyan top edges (usable footing,
//    rule 8); the orbit fragments darker. A TELL glows red-orange on the top, pulsing at 2.5 Hz, and shakes; a dropped piece falls away for
//    design.n37.crown37.pieces.dropSeconds, then hides; back up, it reappears.
//  - THE RING TARGETS (room v10's hoop37 rows): a steel ring of segments spinning round its facing; a pass flashes it cyan.
//  - THE CROWN (Core/Enemies/Crown37.cs): drawn by DrawCrown (SurvivalSim's crown37 while it owns the fight).
// Installs itself after the first scene load (no line in the lead's files) and finds the game each frame it has none. Unity API only in Awake /
// Attach (L-035: no Unity API in field initializers).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Crown37View : MonoBehaviour
    {
        /// <summary>For the reel: pieces drawn now (solid), pieces telling now, ring targets drawn.</summary>
        public static int PiecesShown, TellsShown, HoopsShown;

        SurvivalGame game;
        Transform root;
        Orbit37 builtFor;
        int builtCount = -1;
        Mesh boxMesh;
        Material stoneMat, orbitMat, addMat, ringMat;
        MaterialPropertyBlock block;
        int baseColorId;
        Color cyan, danger, ringTint;
        double dropSeconds;

        sealed class PieceParts
        {
            public Piece37 q;
            public Transform body;
            public Renderer glow;
            public readonly List<Renderer> edges = new List<Renderer>();
        }

        sealed class HoopParts
        {
            public Hoop37Target h;
            public Transform t;
            public readonly List<Renderer> segs = new List<Renderer>();
        }

        readonly List<PieceParts> pieces = new List<PieceParts>();
        readonly List<HoopParts> hoops = new List<HoopParts>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Crown37View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Crown37View>();
        }

        void Awake()
        {
            block = new MaterialPropertyBlock();
            baseColorId = Shader.PropertyToID("_BaseColor");
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.mats == null || game.cfg == null || game.sim == null) { game = null; return false; }
            if (root != null) Destroy(root.gameObject);
            root = new GameObject("NIGHT37 CROWN").transform;
            DontDestroyOnLoad(root.gameObject);
            if (boxMesh == null)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
                Destroy(cube);
            }
            if (stoneMat == null) stoneMat = FxDraw.Opaque(game.mats.neon, "Crown37 stone", Look.Hex(UnityTuning.Crown37StoneHex));
            if (orbitMat == null) orbitMat = FxDraw.Opaque(game.mats.neon, "Crown37 orbit", Look.Hex(UnityTuning.Crown37OrbitHex));
            if (ringMat == null) ringMat = FxDraw.Opaque(game.mats.neon, "Crown37 ring", Look.Hex(UnityTuning.Crown37RingHex));
            if (addMat == null) addMat = FxDraw.Additive(game.mats.neon, "Crown37 glow");
            cyan = Look.C(game.cfg, "colors.usable");
            danger = Look.Hex(UnityTuning.Crown37DangerHex);
            ringTint = Look.Hex(UnityTuning.Crown37RingHex);
            dropSeconds = game.cfg.Has("design.n37.crown37.pieces.dropSeconds") ? game.cfg.Num("design.n37.crown37.pieces.dropSeconds") : 1.2;
            builtFor = null;
            builtCount = -1;
            pieces.Clear();
            hoops.Clear();
            CrownReset();
            return true;
        }

        Transform Box(Transform parent, string name, Material mat, Vector3 centre, Vector3 size, out Renderer r)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = centre;
            g.transform.localScale = size;
            g.AddComponent<MeshFilter>().sharedMesh = boxMesh;
            var mr = g.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = mat == addMat ? ShadowCastingMode.Off : ShadowCastingMode.On;
            r = mr;
            return g.transform;
        }

        void Tint(Renderer r, Color c, float gain)
        {
            block.Clear();
            block.SetColor(baseColorId, c * gain);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(double x, double y, double z) => WebSpace.ToUnity(new Vec3(x, y, z));
        static Vector3 U(Vec3 v) => WebSpace.ToUnity(v);

        void Build(SurvivalSim sim)
        {
            foreach (Transform ch in root) Destroy(ch.gameObject);
            pieces.Clear();
            hoops.Clear();
            CrownReset();
            Orbit37 o = sim.orbit37;
            for (int i = 0; i < o.pieces.Count; i++)
            {
                Piece37 q = o.pieces[i];
                var p = new PieceParts { q = q };
                Vector3 sz = new Vector3((float)q.size.x, (float)q.size.y, (float)q.size.z);
                p.body = Box(root, $"{q.kind} {q.id}", q.kind == Piece37Kind.Orbit ? orbitMat : stoneMat, Vector3.zero, sz, out _);
                float e = UnityTuning.Crown37EdgeThick, hy = sz.y / 2 + e / 2;
                Vector3[] at = { new Vector3(0, hy, sz.z / 2 - e / 2), new Vector3(0, hy, -sz.z / 2 + e / 2), new Vector3(sz.x / 2 - e / 2, hy, 0), new Vector3(-sz.x / 2 + e / 2, hy, 0) };
                Vector3[] es = { new Vector3(sz.x, e, e), new Vector3(sz.x, e, e), new Vector3(e, e, sz.z), new Vector3(e, e, sz.z) };
                for (int k = 0; k < 4; k++)
                {
                    Box(p.body, "edge", addMat, Vector3.zero, Vector3.one, out Renderer er);
                    er.transform.localPosition = new Vector3(at[k].x / sz.x, at[k].y / sz.y, at[k].z / sz.z);
                    er.transform.localScale = new Vector3(es[k].x / sz.x, es[k].y / sz.y, es[k].z / sz.z);
                    p.edges.Add(er);
                }
                Box(p.body, "tell", addMat, Vector3.zero, Vector3.one, out p.glow);
                p.glow.transform.localPosition = new Vector3(0, 0.5f + 0.02f / sz.y, 0);
                p.glow.transform.localScale = new Vector3(0.98f, 0.04f / sz.y, 0.98f);
                pieces.Add(p);
            }
            foreach (Hoop37Target h in sim.hoops37)
            {
                var hp = new HoopParts { h = h };
                hp.t = new GameObject("ring target").transform;
                hp.t.SetParent(root, false);
                RingSegments(hp.t, game.cfg.Num("design.n37.crown37.crown.ringRadius"), hp.segs);
                hoops.Add(hp);
            }
            builtFor = o;
            builtCount = o.pieces.Count;
        }

        /// <summary>A ring of segments in the parent's local XY plane (its facing = local +Z), radius r.</summary>
        void RingSegments(Transform parent, double r, List<Renderer> into)
        {
            int n = UnityTuning.Crown37RingSegments;
            float len = (float)(2 * Mathf.PI * r / n) * 1.05f, th = UnityTuning.Crown37RingThick;
            for (int k = 0; k < n; k++)
            {
                float a = k * 2 * Mathf.PI / n;
                var seg = Box(parent, "seg", ringMat, new Vector3(Mathf.Cos(a) * (float)r, Mathf.Sin(a) * (float)r, 0), new Vector3(th, len, th), out Renderer sr);
                seg.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg);
                into.Add(sr);
            }
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (game.sim == null) { game = null; return; }
            BossTell63.Draw(game, root, BossAttackClock63.Crown); // NIGHT63 G bossTell63 (BossTell63.cs): the danger column through a non-parry wind-up (nothing with the row off)
            SurvivalSim sim = game.sim;
            Orbit37 o = sim.orbit37;
            bool on = o != null && (o.Count > 0 || sim.hoops37.Count > 0);
            if (!on)
            {
                if (root.gameObject.activeSelf) root.gameObject.SetActive(false);
                PiecesShown = TellsShown = HoopsShown = 0;
                return;
            }
            if (!root.gameObject.activeSelf) root.gameObject.SetActive(true);
            if (o != builtFor || o.Count != builtCount || (pieces.Count > 0 && pieces[0].q != o.pieces[0]) || hoops.Count != sim.hoops37.Count
                || (hoops.Count > 0 && hoops[0].h != sim.hoops37[0])) Build(sim);
            float alpha = Mathf.Clamp01((float)game.Move54WorldAlpha), now = Time.time; // NIGHT54 M move54: the split bend's world alpha (game.Alpha with the row off)
            int shown = 0, tells = 0;
            foreach (PieceParts p in pieces)
            {
                Piece37 q = p.q;
                Vec3 a = q.prev, b = q.pos;
                double cx = a.x + (b.x - a.x) * alpha, cy = a.y + (b.y - a.y) * alpha, cz = a.z + (b.z - a.z) * alpha;
                bool down = q.state == Piece37State.Down;
                double fall = 0;
                if (down)
                {
                    double t = (q.stateSteps + alpha) * sim.step;
                    if (t > dropSeconds) { if (p.body.gameObject.activeSelf) p.body.gameObject.SetActive(false); continue; }
                    fall = 0.5 * UnityTuning.Crown37DropGravity * t * t;
                }
                if (!p.body.gameObject.activeSelf) p.body.gameObject.SetActive(true);
                bool tell = q.state == Piece37State.Tell;
                Vector3 pos = U(cx, cy - q.size.y / 2 - fall, cz);
                if (tell)
                {
                    tells++;
                    float s = UnityTuning.Crown37TellShake;
                    pos += new Vector3(Mathf.Sin(now * 53f) * s, 0, Mathf.Cos(now * 47f) * s);
                    float pulse = 0.75f + 0.25f * Mathf.Sin(now * Mathf.PI * 2 * UnityTuning.Crown37TellHz);
                    Tint(p.glow, danger, Mathf.Lerp(UnityTuning.Crown37TellGainLo, UnityTuning.Crown37TellGainHi, (float)q.TellShare) * pulse);
                }
                p.glow.enabled = tell;
                p.body.localPosition = pos;
                foreach (Renderer r in p.edges) Tint(r, tell || down ? danger : cyan, UnityTuning.Crown37EdgeGain);
                if (!down) shown++;
            }
            foreach (HoopParts hp in hoops)
            {
                hp.t.position = U(hp.h.at);
                Vector3 n = U(hp.h.normal) - U(0, 0, 0);
                hp.t.rotation = Quaternion.LookRotation(n.sqrMagnitude > 1e-6f ? n.normalized : Vector3.forward) * Quaternion.Euler(0, 0, now * UnityTuning.Crown37HoopSpin * Mathf.Rad2Deg);
                float since = (sim.crown37Steps - hp.h.lastPassStep) * (float)sim.step;
                bool flash = since >= 0 && since < UnityTuning.Crown37PassFlashSeconds;
                foreach (Renderer r in hp.segs)
                {
                    if (flash) Tint(r, cyan, UnityTuning.Crown37PassFlashGain * (1f - since / UnityTuning.Crown37PassFlashSeconds) + 0.6f);
                    else if (!Hoop40Tint(sim, r)) Tint(r, ringTint, 1f); // NIGHT40 B5: cyan with crown40 on (Crown37View.Crown40.cs)
                }
            }
            PiecesShown = shown;
            TellsShown = tells;
            HoopsShown = hoops.Count;
            DrawCrown(sim, alpha, now);
        }

        partial void CrownReset();
        partial void DrawCrown(SurvivalSim sim, float alpha, float now);

        void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
            if (stoneMat != null) Destroy(stoneMat);
            if (orbitMat != null) Destroy(orbitMat);
            if (ringMat != null) Destroy(ringMat);
            if (addMat != null) Destroy(addMat);
        }
    }
}
