// NIGHT42 builder K1 (arena42; G12; view only: no collider, no sim change). What the player SEES of the arena kit (Core/Arena42.cs):
//  - THE GRINDER (each `grinders42` rect of the run's Arena42): a dark burnt floor plate, spinning red-orange blade bars along its long side (danger
//    red-orange: rule 8), lighter red-orange rim strips round its edges. A body ground (event arena42Ground) throws a hot flare column out of it.
//  - BREAKABLE COVER (each `cover42` box): a dark slate block (not a wall's colour) with a cyan band along its top (player-usable: you can shoot it
//    apart) whose length is the hits it has left; a hit flashes the body white (arena42CoverHit); a break hides the block and throws shards
//    (arena42CoverBreak).
//  - The launch pad is the existing aimed jump pad (JumpPadView27 draws it).
// Installs itself after the first scene load (as LavaView30) and finds the game each frame it has none. Nothing is drawn while the run has no arena42
// (the row off, -skygrindOff42, or a level without the keys).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The view's pure parts (EditMode tests).</summary>
    public static class Arena42ViewMath
    {
        /// <summary>Blade hubs along a side of length `len` m (at least 1).</summary>
        public static int Hubs(double len) => Math.Max(1, (int)Math.Round(len / UnityTuning.Arena42BladeSpacing));

        /// <summary>The cyan band's share: hits left over max hits (0..1).</summary>
        public static float BandShare(int hits, int maxHits) => maxHits <= 0 ? 0f : Mathf.Clamp01(hits / (float)maxHits);
    }

    public sealed class Arena42View : MonoBehaviour
    {
        /// <summary>For the reels: grinders / blades / cover built for the run, flares and shatters shown.</summary>
        public static int GrindersBuilt, BladesBuilt, CoverBuilt, FlaresShown, ShattersShown, CoverHitsShown;

        SurvivalGame game;
        Arena42 builtFor;
        Transform root;
        Material floorMat, bladeMat, rimMat, coverMat, edgeMat, flareMat, shardMat;
        MaterialPropertyBlock block;
        static int BaseColor;
        Mesh cube;
        readonly List<GameObject> parts = new List<GameObject>();
        readonly List<Transform> blades = new List<Transform>();
        readonly List<float> bladeDir = new List<float>();
        readonly List<Transform> coverBody = new List<Transform>(), coverEdge = new List<Transform>();
        readonly List<Renderer> coverBodyR = new List<Renderer>();
        readonly List<float> coverFlash = new List<float>();
        readonly List<Vector3> edgeFull = new List<Vector3>();
        // flares
        readonly Transform[] flareT = new Transform[UnityTuning.Arena42FlarePool];
        readonly Renderer[] flareR = new Renderer[UnityTuning.Arena42FlarePool];
        readonly float[] flareAge = new float[UnityTuning.Arena42FlarePool];
        // shards (two shatters at once)
        const int ShardPool = UnityTuning.Arena42ShardCount * 2;
        readonly Transform[] shardT = new Transform[ShardPool];
        readonly Vector3[] shardV = new Vector3[ShardPool];
        readonly float[] shardAge = new float[ShardPool];
        readonly Queue<Vector3> flareQueue = new Queue<Vector3>();
        readonly Queue<int> shatterQueue = new Queue<int>(), hitQueue = new Queue<int>();
        Action unGround, unHit, unBreak;

        void Awake()
        {
            block = new MaterialPropertyBlock();
            BaseColor = Shader.PropertyToID("_BaseColor");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Arena42View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Arena42View>();
        }

        void OnDestroy() => Unsubscribe();

        void Unsubscribe()
        {
            unGround?.Invoke();
            unHit?.Invoke();
            unBreak?.Invoke();
            unGround = unHit = unBreak = null;
        }

        bool Attach()
        {
            if (game != null) return true;
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.cfg == null || game.mats == null || game.events == null) { game = null; return false; }
            Unsubscribe();
            Clear();
            if (root != null) Destroy(root.gameObject);
            foreach (Material m in new[] { floorMat, bladeMat, rimMat, coverMat, edgeMat, flareMat, shardMat }) if (m != null) Destroy(m);
            root = new GameObject("Arena 42").transform;
            DontDestroyOnLoad(root.gameObject);
            Material neon = game.mats.neon;
            floorMat = FxDraw.Opaque(neon, "Arena42 grinder floor", Look.Hex(UnityTuning.Arena42GrinderFloorHex));
            bladeMat = FxDraw.Opaque(neon, "Arena42 blade", Look.Hex(UnityTuning.Arena42GrinderBladeHex, UnityTuning.Arena42BladeGain));
            rimMat = FxDraw.Opaque(neon, "Arena42 rim", Look.Hex(UnityTuning.Arena42GrinderRimHex));
            coverMat = FxDraw.Opaque(neon, "Arena42 cover", Look.Hex(UnityTuning.Arena42CoverHex));
            edgeMat = FxDraw.Opaque(neon, "Arena42 cover edge", Look.Hex(UnityTuning.Arena42CoverEdgeHex, UnityTuning.Arena42CoverEdgeGain));
            flareMat = FxDraw.Additive(neon, "Arena42 flare");
            shardMat = FxDraw.Opaque(neon, "Arena42 shard", Look.Hex(UnityTuning.Arena42CoverHex, 1.6f));
            if (cube == null)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = c.GetComponent<MeshFilter>().sharedMesh;
                Destroy(c);
            }
            for (int i = 0; i < flareT.Length; i++) { flareR[i] = Part("flare " + i, flareMat, out flareT[i], false); flareAge[i] = -1; }
            for (int i = 0; i < shardT.Length; i++) { Part("shard " + i, shardMat, out shardT[i], false); shardAge[i] = -1; }
            unGround = game.events.On(Arena42.GroundEvent, o => { if (o is Arena42GroundEvent e) flareQueue.Enqueue(WebSpace.ToUnity(new Vec3(e.x, e.y, e.z))); });
            unHit = game.events.On(Arena42.CoverHitEvent, o => { if (o is Arena42CoverEvent e) hitQueue.Enqueue(e.index); });
            unBreak = game.events.On(Arena42.CoverBreakEvent, o => { if (o is Arena42CoverEvent e) shatterQueue.Enqueue(e.index); });
            return true;
        }

        MeshRenderer Part(string name, Material m, out Transform t, bool active, Transform parent = null)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent != null ? parent : root, false);
            g.AddComponent<MeshFilter>().sharedMesh = cube;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            g.SetActive(active);
            t = g.transform;
            return r;
        }

        void Clear()
        {
            foreach (GameObject g in parts) if (g != null) Destroy(g);
            parts.Clear();
            blades.Clear();
            bladeDir.Clear();
            coverBody.Clear();
            coverEdge.Clear();
            coverBodyR.Clear();
            coverFlash.Clear();
            edgeFull.Clear();
            builtFor = null;
            GrindersBuilt = BladesBuilt = CoverBuilt = 0;
        }

        void Build(Arena42 a)
        {
            Clear();
            builtFor = a;
            if (a == null) return;
            for (int gi = 0; gi < a.grinders.Count; gi++)
            {
                Arena42Grinder g = a.grinders[gi];
                float w = (float)(g.x1 - g.x0), d = (float)(g.z1 - g.z0);
                Vector3 c = WebSpace.ToUnity(new Vec3((g.x0 + g.x1) * 0.5, g.y, (g.z0 + g.z1) * 0.5));
                var holder = new GameObject("grinder " + gi);
                holder.transform.SetParent(root, false);
                parts.Add(holder);
                Part("floor", floorMat, out Transform fl, true, holder.transform);
                fl.position = c + Vector3.up * 0.01f;
                fl.localScale = new Vector3(w, 0.02f, d);
                float rw = UnityTuning.Arena42RimWidth, ry = UnityTuning.Arena42RimLift;
                Part("rim a", rimMat, out Transform r1, true, holder.transform); r1.position = c + new Vector3(0, ry, d * 0.5f - rw * 0.5f); r1.localScale = new Vector3(w, 0.03f, rw);
                Part("rim b", rimMat, out Transform r2, true, holder.transform); r2.position = c + new Vector3(0, ry, -d * 0.5f + rw * 0.5f); r2.localScale = new Vector3(w, 0.03f, rw);
                Part("rim c", rimMat, out Transform r3, true, holder.transform); r3.position = c + new Vector3(w * 0.5f - rw * 0.5f, ry, 0); r3.localScale = new Vector3(rw, 0.03f, d);
                Part("rim d", rimMat, out Transform r4, true, holder.transform); r4.position = c + new Vector3(-w * 0.5f + rw * 0.5f, ry, 0); r4.localScale = new Vector3(rw, 0.03f, d);
                // blade hubs along the long side; each hub a cross of two bars, its span the short side
                bool alongX = w >= d;
                float len = alongX ? w : d, span = Mathf.Min(alongX ? d : w, UnityTuning.Arena42BladeSpacing * 1.15f) * 0.92f;
                int hubs = Arena42ViewMath.Hubs(len);
                for (int k = 0; k < hubs; k++)
                {
                    float t = (k + 0.5f) / hubs - 0.5f;
                    Vector3 hub = c + (alongX ? new Vector3(t * w, 0, 0) : new Vector3(0, 0, t * d)) + Vector3.up * UnityTuning.Arena42BladeLift;
                    var h = new GameObject("hub " + k).transform;
                    h.SetParent(holder.transform, false);
                    h.position = hub;
                    Part("bar 1", bladeMat, out Transform b1, true, h); b1.localScale = new Vector3(span, UnityTuning.Arena42BladeThick, UnityTuning.Arena42BladeWide);
                    Part("bar 2", bladeMat, out Transform b2, true, h); b2.localScale = new Vector3(UnityTuning.Arena42BladeWide, UnityTuning.Arena42BladeThick, span);
                    blades.Add(h);
                    bladeDir.Add(k % 2 == 0 ? 1f : -1f); // neighbours counter-rotate: teeth that meet
                    BladesBuilt++;
                }
                GrindersBuilt++;
            }
            for (int ci = 0; ci < a.cover.Count; ci++)
            {
                Arena42Cover cv = a.cover[ci];
                Vector3 lo = WebSpace.ToUnity(cv.min), hi = WebSpace.ToUnity(cv.max);
                Vector3 c = (lo + hi) * 0.5f, size = new Vector3(Mathf.Abs(hi.x - lo.x), Mathf.Abs(hi.y - lo.y), Mathf.Abs(hi.z - lo.z));
                var body = Part("cover " + ci, coverMat, out Transform bt, true);
                bt.position = c;
                bt.localScale = size;
                parts.Add(bt.gameObject);
                Part("cover edge " + ci, edgeMat, out Transform et, true);
                et.position = c + Vector3.up * (size.y * 0.5f + UnityTuning.Arena42CoverEdgeHeight * 0.5f);
                Vector3 full = new Vector3(size.x * 1.02f, UnityTuning.Arena42CoverEdgeHeight, size.z * 1.02f);
                et.localScale = full;
                parts.Add(et.gameObject);
                coverBody.Add(bt);
                coverBodyR.Add(body);
                coverEdge.Add(et);
                edgeFull.Add(full);
                coverFlash.Add(0);
                CoverBuilt++;
            }
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (game == null) return;
            SurvivalSim sim = game.sim;
            Arena42 a = sim != null ? sim.arena42 : null;
            if (!ReferenceEquals(a, builtFor) || (a != null && (a.grinders.Count != GrindersBuilt || a.cover.Count != CoverBuilt))) Build(a);
            float dt = Time.deltaTime;
            bool reduced = game.settings != null && game.settings.View.reducedMotion;
            // the blades
            float turn = (reduced ? 0.25f : 1f) * UnityTuning.Arena42BladeTurnsPerSecond * 360f * dt;
            for (int i = 0; i < blades.Count; i++) if (blades[i] != null) blades[i].Rotate(0, turn * bladeDir[i], 0, Space.World);
            // the cover: standing or not, the band's length, the hit flash
            while (hitQueue.Count > 0)
            {
                int i = hitQueue.Dequeue();
                if (i >= 0 && i < coverFlash.Count) coverFlash[i] = UnityTuning.Arena42CoverHitFlashSeconds;
                CoverHitsShown++;
            }
            if (a != null)
                for (int i = 0; i < coverBody.Count && i < a.cover.Count; i++)
                {
                    Arena42Cover cv = a.cover[i];
                    bool up = !cv.broken;
                    if (coverBody[i].gameObject.activeSelf != up) coverBody[i].gameObject.SetActive(up);
                    if (coverEdge[i].gameObject.activeSelf != up) coverEdge[i].gameObject.SetActive(up);
                    if (!up) continue;
                    float share = Arena42ViewMath.BandShare(cv.hits, cv.maxHits);
                    Vector3 f = edgeFull[i];
                    bool alongX = f.x >= f.z;
                    coverEdge[i].localScale = alongX ? new Vector3(f.x * share, f.y, f.z) : new Vector3(f.x, f.y, f.z * share);
                    if (coverFlash[i] > 0)
                    {
                        coverFlash[i] -= dt;
                        block.Clear();
                        block.SetColor(BaseColor, Color.Lerp(Look.Hex(UnityTuning.Arena42CoverHex), Color.white, Mathf.Clamp01(coverFlash[i] / UnityTuning.Arena42CoverHitFlashSeconds)));
                        coverBodyR[i].SetPropertyBlock(block);
                    }
                    else coverBodyR[i].SetPropertyBlock(null);
                }
            while (shatterQueue.Count > 0)
            {
                int i = shatterQueue.Dequeue();
                if (i < 0 || i >= coverBody.Count) continue;
                Shatter(coverBody[i].position, coverBody[i].localScale);
                ShattersShown++;
            }
            Shards(dt);
            Flares(dt);
        }

        void Shatter(Vector3 c, Vector3 size)
        {
            int n = UnityTuning.Arena42ShardCount, used = 0;
            for (int s = 0; s < shardT.Length && used < n; s++)
            {
                if (shardAge[s] >= 0) continue;
                float ang = used * Mathf.PI * 2f / n, up = 0.4f + 0.6f * ((used * 7) % n) / n; // a fixed fan (the view: no randomness needed)
                shardV[s] = new Vector3(Mathf.Cos(ang), up, Mathf.Sin(ang)) * UnityTuning.Arena42ShardSpeed;
                shardT[s].position = c + new Vector3(Mathf.Cos(ang) * size.x * 0.3f, (up - 0.7f) * size.y * 0.5f, Mathf.Sin(ang) * size.z * 0.3f);
                shardT[s].localScale = Vector3.one * UnityTuning.Arena42ShardSize;
                shardT[s].rotation = Quaternion.Euler(used * 37f, used * 53f, 0);
                shardT[s].gameObject.SetActive(true);
                shardAge[s] = 0;
                used++;
            }
        }

        void Shards(float dt)
        {
            for (int s = 0; s < shardT.Length; s++)
            {
                if (shardAge[s] < 0) continue;
                shardAge[s] += dt;
                if (shardAge[s] >= UnityTuning.Arena42ShardSeconds)
                {
                    shardAge[s] = -1;
                    shardT[s].gameObject.SetActive(false);
                    continue;
                }
                shardV[s].y -= UnityTuning.Arena42ShardGravity * dt;
                shardT[s].position += shardV[s] * dt;
                shardT[s].Rotate(360f * dt, 220f * dt, 0, Space.Self);
                float k = 1f - shardAge[s] / UnityTuning.Arena42ShardSeconds;
                shardT[s].localScale = Vector3.one * (UnityTuning.Arena42ShardSize * (0.4f + 0.6f * k));
            }
        }

        void Flares(float dt)
        {
            while (flareQueue.Count > 0)
            {
                Vector3 at = flareQueue.Dequeue();
                int slot = -1;
                float oldest = -1;
                for (int i = 0; i < flareAge.Length; i++)
                {
                    if (flareAge[i] < 0) { slot = i; break; }
                    if (flareAge[i] > oldest) { oldest = flareAge[i]; slot = i; }
                }
                flareAge[slot] = 0;
                flareT[slot].position = at + Vector3.up * (UnityTuning.Arena42FlareHeight * 0.5f);
                flareT[slot].gameObject.SetActive(true);
                FlaresShown++;
            }
            for (int i = 0; i < flareAge.Length; i++)
            {
                if (flareAge[i] < 0) continue;
                flareAge[i] += dt;
                float u = flareAge[i] / UnityTuning.Arena42FlareSeconds;
                if (u >= 1)
                {
                    flareAge[i] = -1;
                    flareT[i].gameObject.SetActive(false);
                    continue;
                }
                float wv = UnityTuning.Arena42FlareWidth * (1f - 0.6f * u);
                flareT[i].localScale = new Vector3(wv, UnityTuning.Arena42FlareHeight * (0.6f + 0.4f * u), wv);
                block.Clear();
                block.SetColor(BaseColor, Look.Hex(UnityTuning.Arena42FlareHex, 2f * (1f - u)));
                flareR[i].SetPropertyBlock(block);
            }
        }
    }
}
