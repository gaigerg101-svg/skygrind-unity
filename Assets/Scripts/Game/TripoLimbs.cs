// PG3 (GG 2026-10-28: "enemies need walking or better animations for walking and attacking"; "keep building, improving animations ... with the new models"): the Tripo enemy models
// are single static meshes (no rig), so their limbs are cut out by position once per kind and swung procedurally. On a body's first model frame the model's mesh is read in the holder's
// space (feet at y = 0, +z forward) and its triangles are sorted by centroid into the body, the legs (below the hip; a biped's left / right, a quadruped's four quarters, the head end
// kept with the body) and, for the husk and the spitter, the arms (between the hip and the shoulder, outside the torso's width). Each limb becomes its own mesh around its pivot (the hip
// or the shoulder), a child of the model's holder, so the body's lean, bob, squash and flinch carry it. Enemy43View.Tripo.cs swings them: a walk cycle from the body's measured ground speed
// (bipeds' legs alternate and the arms counter-swing; quadrupeds' diagonal pairs move together), the arms raised in a wind-up and thrown forward on the release. The original renderer stops
// drawing. Meshes are made once per kind and shared. UnityTuning.Tripo* has every number. A kind with no cut (the drone) or an unreadable mesh: no limbs, the whole model moves as before.
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class TripoLimbs
    {
        sealed class Cut
        {
            public Mesh body;
            public Mesh[] legs, arms;
            public Vector3[] legPivots, armPivots;
            public bool quad;
        }

        static readonly Dictionary<string, Cut> cuts = new Dictionary<string, Cut>(System.StringComparer.Ordinal); // kind -> its cut (null: none)

        public Transform body;
        public Transform[] legs = new Transform[0], arms = new Transform[0];

        /// <summary>THE ICEBREAKER (and any view with its own bones): hand each cut part to the bone that already moves it, keeping where it is now, so the view's own walk, lean,
        /// haymaker and slam poses carry the model's pieces. A null bone leaves its part on the holder. Index 0 is the model's left (-x) side.</summary>
        public void Ride(Transform bodyBone, Transform legLeft, Transform legRight, Transform armLeft, Transform armRight)
        {
            if (body != null && bodyBone != null) body.SetParent(bodyBone, true);
            if (legs.Length > 0 && legs[0] != null && legLeft != null) legs[0].SetParent(legLeft, true);
            if (legs.Length > 1 && legs[1] != null && legRight != null) legs[1].SetParent(legRight, true);
            if (arms.Length > 0 && arms[0] != null && armLeft != null) arms[0].SetParent(armLeft, true);
            if (arms.Length > 1 && arms[1] != null && armRight != null) arms[1].SetParent(armRight, true);
        }
        public bool quad;

        /// <summary>The limbs of `kind`'s model under `holder` (TripoSlot.Model), or null when the kind has no cut or its mesh cannot be read.</summary>
        public static TripoLimbs Make(string kind, Transform holder)
        {
            if (holder == null) return null;
            MeshFilter mf = holder.GetComponentInChildren<MeshFilter>(true);
            Renderer src = mf != null ? mf.GetComponent<Renderer>() : null;
            if (mf == null || src == null || mf.sharedMesh == null) return null;
            if (!cuts.TryGetValue(kind, out Cut cut))
            {
                cut = mf.sharedMesh.isReadable ? Build(kind, mf.sharedMesh, holder.worldToLocalMatrix * mf.transform.localToWorldMatrix) : null;
                cuts[kind] = cut;
            }
            if (cut == null) return null;
            var l = new TripoLimbs { quad = cut.quad };
            l.body = Part(holder, "body", cut.body, Vector3.zero, src);
            l.legs = new Transform[cut.legs.Length];
            for (int i = 0; i < cut.legs.Length; i++) l.legs[i] = Part(holder, "leg " + i, cut.legs[i], cut.legPivots[i], src);
            l.arms = new Transform[cut.arms.Length];
            for (int i = 0; i < cut.arms.Length; i++) l.arms[i] = Part(holder, "arm " + i, cut.arms[i], cut.armPivots[i], src);
            src.enabled = false;
            return l;
        }

        static Transform Part(Transform holder, string name, Mesh mesh, Vector3 pivot, Renderer like)
        {
            var go = new GameObject(name);
            go.layer = like.gameObject.layer;
            Transform t = go.transform;
            t.SetParent(holder, false);
            t.localPosition = pivot;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterials = like.sharedMaterials;
            r.shadowCastingMode = like.shadowCastingMode;
            r.receiveShadows = like.receiveShadows;
            return t;
        }

        /// <summary>Sorts the triangles (in the holder's space through `toHolder`) into groups by the kind's rule and makes one mesh per group around its pivot.</summary>
        static Cut Build(string kind, Mesh src, Matrix4x4 toHolder)
        {
            bool brute = kind == "icebreaker"; // THE ICEBREAKER: its fists hang near the ground, so its arms are everything outside the torso's width at any height
            bool biped = kind == "husk" || kind == "spitter" || kind == "warden" || brute;
            bool quad = kind == "charger" || kind == "swarmer";
            if (!biped && !quad) return null;
            Vector3[] v = src.vertices;
            Vector3[] n = src.normals;
            Vector2[] uv = src.uv;
            int[] tri = src.triangles;
            var p = new Vector3[v.Length];
            var nn = new Vector3[v.Length];
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), max = -min;
            for (int i = 0; i < v.Length; i++)
            {
                p[i] = toHolder.MultiplyPoint3x4(v[i]);
                nn[i] = n.Length == v.Length ? toHolder.MultiplyVector(n[i]).normalized : Vector3.up;
                min = Vector3.Min(min, p[i]);
                max = Vector3.Max(max, p[i]);
            }
            float h = max.y - min.y, len = max.z - min.z, cx = 0.5f * (min.x + max.x), cz = 0.5f * (min.z + max.z);
            if (!(h > 1e-4f)) return null;
            bool arms = kind == "husk" || kind == "spitter" || brute;
            float hip = min.y + h * (quad ? (kind == "swarmer" ? UnityTuning.TripoBellySwarmer : UnityTuning.TripoBellyCharger) : kind == "spitter" ? UnityTuning.TripoHipSpitter : UnityTuning.TripoHip);
            float shoulderTop = min.y + h * UnityTuning.TripoShoulderTop;
            float armOut = 0f;
            if (arms)
            {
                float half = 0f;
                for (int i = 0; i < p.Length; i++) if (p[i].y > hip && p[i].y < shoulderTop) half = Mathf.Max(half, Mathf.Abs(p[i].x - cx));
                armOut = half * (kind == "spitter" ? UnityTuning.TripoArmOutSpitter : brute ? UnityTuning.TripoArmOutBrute : UnityTuning.TripoArmOut);
            }
            float headZ = max.z - len * UnityTuning.TripoQuadHeadShare; // a quadruped's head end stays with the body
            // group: 0 body; legs 1..(2 or 4); arms after the legs
            int legN = quad ? 4 : 2, armN = arms ? 2 : 0;
            int groups = 1 + legN + armN;
            var lists = new List<int>[groups];
            for (int g = 0; g < groups; g++) lists[g] = new List<int>();
            for (int t = 0; t < tri.Length; t += 3)
            {
                Vector3 c = (p[tri[t]] + p[tri[t + 1]] + p[tri[t + 2]]) / 3f;
                int g = 0;
                if (brute && c.y > min.y + h * UnityTuning.TripoBruteArmFloor && Mathf.Abs(c.x - cx) > armOut)
                    g = 1 + legN + (c.x < cx ? 0 : 1);
                else if (c.y < hip && (!quad || c.z < headZ))
                    g = quad ? 1 + (c.z >= cz ? 0 : 2) + (c.x < cx ? 0 : 1) : 1 + (c.x < cx ? 0 : 1);
                else if (arms && c.y >= hip && c.y < shoulderTop && Mathf.Abs(c.x - cx) > armOut)
                    g = 1 + legN + (c.x < cx ? 0 : 1);
                lists[g].Add(tri[t]); lists[g].Add(tri[t + 1]); lists[g].Add(tri[t + 2]);
            }
            var cut = new Cut { quad = quad, legs = new Mesh[legN], arms = new Mesh[armN], legPivots = new Vector3[legN], armPivots = new Vector3[armN] };
            cut.body = MakeMesh(kind + " body", lists[0], p, nn, uv, Vector3.zero);
            for (int i = 0; i < legN; i++)
            {
                Vector3 m = Mean(lists[1 + i], p);
                cut.legPivots[i] = new Vector3(m.x, hip + h * UnityTuning.TripoHipPivotLift, m.z); // a little inside the body: the seam slides less as the leg swings
                cut.legs[i] = MakeMesh(kind + " leg " + i, lists[1 + i], p, nn, uv, cut.legPivots[i]);
            }
            for (int i = 0; i < armN; i++)
            {
                Vector3 m = Mean(lists[1 + legN + i], p);
                cut.armPivots[i] = new Vector3(m.x, min.y + h * UnityTuning.TripoShoulderPivot, m.z);
                cut.arms[i] = MakeMesh(kind + " arm " + i, lists[1 + legN + i], p, nn, uv, cut.armPivots[i]);
            }
            return cut;
        }

        static Vector3 Mean(List<int> idx, Vector3[] p)
        {
            if (idx.Count == 0) return Vector3.zero;
            Vector3 s = Vector3.zero;
            for (int i = 0; i < idx.Count; i++) s += p[idx[i]];
            return s / idx.Count;
        }

        static Mesh MakeMesh(string name, List<int> idx, Vector3[] p, Vector3[] n, Vector2[] uv, Vector3 pivot)
        {
            var map = new Dictionary<int, int>();
            var vp = new List<Vector3>(); var vn = new List<Vector3>(); var vu = new List<Vector2>();
            var t = new int[idx.Count];
            for (int i = 0; i < idx.Count; i++)
            {
                int o = idx[i];
                if (!map.TryGetValue(o, out int k))
                {
                    k = vp.Count;
                    map[o] = k;
                    vp.Add(p[o] - pivot);
                    vn.Add(n[o]);
                    vu.Add(uv.Length == p.Length ? uv[o] : Vector2.zero);
                }
                t[i] = k;
            }
            var m = new Mesh { name = name };
            if (vp.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(vp); m.SetNormals(vn); m.SetUVs(0, vu); m.SetTriangles(t, 0);
            m.RecalculateBounds();
            m.RecalculateTangents();
            return m;
        }

        /// <summary>The frame's pose: `phase` the gait (radians), `walk` 0..1, `wind` the wind-up 0..1, `strike` the release 0..1.</summary>
        public void Pose(float phase, float walk, float wind, float strike, float breath = 0f)
        {
            float s = Mathf.Sin(phase);
            float leg = UnityTuning.TripoLegSwingDeg * walk * s;
            if (quad)
            {
                // 0 front-left, 1 front-right, 2 back-left, 3 back-right: the diagonal pairs move together
                for (int i = 0; i < legs.Length; i++)
                {
                    bool pairA = i == 0 || i == 3;
                    if (legs[i] != null) legs[i].localRotation = Quaternion.Euler(pairA ? leg : -leg, 0f, 0f);
                }
            }
            else
            {
                if (legs.Length > 0 && legs[0] != null) legs[0].localRotation = Quaternion.Euler(leg, 0f, 0f);
                if (legs.Length > 1 && legs[1] != null) legs[1].localRotation = Quaternion.Euler(-leg, 0f, 0f);
            }
            float arm = UnityTuning.TripoArmSwingDeg * walk * s + UnityTuning.TripoArmIdleDeg * breath;
            float attack = -UnityTuning.TripoArmRaiseDeg * wind - UnityTuning.TripoArmThrowDeg * strike;
            if (arms.Length > 0 && arms[0] != null) arms[0].localRotation = Quaternion.Euler(-arm + attack, 0f, 0f);
            if (arms.Length > 1 && arms[1] != null) arms[1].localRotation = Quaternion.Euler(arm + attack, 0f, 0f);
        }
    }
}
