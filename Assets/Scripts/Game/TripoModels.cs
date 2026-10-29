// PG3 tripo74 (GG 2026-10-28, the AME 294 PG3 assignment: 3D models made in Tripo replace the visuals of the KESTREL / THUNDERCLAP, the enemies and the bosses; CLAUDE.md rule 7's
// pill-language-only rule is lifted for these models; a view row, design.n74.tripo74, read by NightRows.ViewOn): the shared helper. VIEW ONLY: a model is a child of a view's transform with its
// colliders, rigid bodies, cameras, lights and audio stripped; no hit box, collider, sim number or step is touched. The FBX files live at Assets/Resources/Tripo/<id>.fbx (UnityTuning.TripoFolder);
// a missing file is one cached Resources.Load miss and then nothing at all: no model, no hidden part, no log.
//   TripoModels.Attach: instantiate a model under a parent, fitted by its bounds (a height, a length or its widest side), feet (or middle) on the parent's origin, recursive layer, shadows.
//   TripoSlot: one model on one view: attach once, hide the pill parts it replaces (forceRenderingOff, so bounds, property blocks and the death break still work on them), show / hide with the row
//   every frame (row off: the model hides and the pills draw again), the hit flash on the model's renderers.
//   TripoModels.Line(): the Player's "[tripo] ..." log line (ids loaded, instances made, ids missing).
// No Unity API in a field initializer (L-035). The hooks: Viewmodel.Tripo.cs, Enemy43View.Tripo.cs, Wick37View.Tripo.cs, Brawl40View.Tripo.cs, Crown37View.Tripo.cs, Loom37View.Tripo.cs.
using System;
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public static class TripoModels
    {
        /// <summary>What a model's size means: its height (y), its length (z, after it is turned), or its widest side.</summary>
        public enum Fit { Height, Length, Widest }

        static readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal); // id -> the asset, null: no file (a cached miss)
        static readonly Dictionary<string, int> made = new Dictionary<string, int>(StringComparer.Ordinal); // id -> instances made
        static readonly HashSet<string> warned = new HashSet<string>(StringComparer.Ordinal);
        static readonly Dictionary<string, Material> glowMats = new Dictionary<string, Material>(StringComparer.Ordinal); // id -> its glow material (one per id, shared by its instances); null: none
        static readonly Dictionary<Renderer, TripoSlot> flashFor = new Dictionary<Renderer, TripoSlot>(); // an enemy view's pill renderer -> the slot whose model flashes with it
        static int onFrame = -1;
        static GameConfig onCfg;
        static bool onNow;

        // a new Play session (Enter Play Mode without a domain reload keeps statics): nothing cached from the last one
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            prefabs.Clear();
            made.Clear();
            warned.Clear();
            flashFor.Clear();
            onFrame = -1;
            onCfg = null;
            onNow = false;
        }

        /// <summary>Is the tripo74 view row on this frame (design.n74.tripo74.enabled; absent: off)? Read every frame, once a frame per config.</summary>
        public static bool On(GameConfig cfg)
        {
            int f = Time.frameCount;
            if (f != onFrame || !ReferenceEquals(cfg, onCfg))
            {
                onFrame = f;
                onCfg = cfg;
                onNow = cfg != null && NightRows.ViewOn(cfg, UnityTuning.TripoRow);
            }
            return onNow;
        }

        /// <summary>The model asset (Resources/Tripo/&lt;id&gt;), or null when the file is missing; the answer, a miss too, is kept.</summary>
        public static GameObject Prefab(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (prefabs.TryGetValue(id, out GameObject p)) return p;
            p = Resources.Load<GameObject>(UnityTuning.TripoFolder + id);
            prefabs[id] = p;
            return p;
        }

        /// <summary>
        /// A model of `id` under `parent`: the returned holder sits on the parent's origin, the model inside it is turned by `yawDeg` (about its own up axis), scaled uniformly so its height is
        /// `targetHeight`, centred on x / z, its bottom at the origin. null when the file is missing. `layer` &gt;= 0 puts the model and every child on that layer; shadows follow `shadows`.
        /// </summary>
        public static GameObject Attach(Transform parent, string id, float targetHeight, float yawDeg, int layer = -1, bool shadows = true)
            => Attach(parent, id, Fit.Height, targetHeight, Quaternion.Euler(0, yawDeg, 0), false, false, layer, shadows);

        /// <summary>
        /// The general form: `fit` says what `size` measures; `orient` turns the model first; `alongZ` lays its longer horizontal side along z (a gun, a shuttle: after `orient`);
        /// `centre` puts the bounds' middle (not the bottom) on the holder's origin. Returns the holder (the model is two children down), or null.
        /// </summary>
        public static GameObject Attach(Transform parent, string id, Fit fit, float size, Quaternion orient, bool alongZ, bool centre, int layer, bool shadows)
        {
            GameObject prefab = Prefab(id);
            if (prefab == null || parent == null || !(size > 0f)) return null;
            var holder = new GameObject("tripo " + id);
            Transform ht = holder.transform;
            ht.SetParent(parent, false);
            Transform ft = new GameObject("fit").transform;
            ft.SetParent(ht, false);
            GameObject inst = UnityEngine.Object.Instantiate(prefab, ft, false);
            inst.name = id;
            Strip(inst);
            FixShaders(inst);
            Glow(id, inst);
            // measure in the holder's own space (the fit is still the identity), so a parent's squash or turn at this moment does not matter
            if (!Measure(ht, inst, out Bounds b))
            {
                Warn(id, "has no mesh to measure");
                UnityEngine.Object.Destroy(holder);
                return null;
            }
            Quaternion q = orient;
            Bounds rb = Rotated(b, q);
            if (alongZ && rb.extents.x > rb.extents.z * 1.05f)
            {
                q = Quaternion.Euler(0, 90, 0) * q;
                rb = Rotated(b, q);
            }
            float measured = fit == Fit.Height ? rb.size.y : fit == Fit.Length ? rb.size.z : Mathf.Max(rb.size.x, Mathf.Max(rb.size.y, rb.size.z));
            if (!(measured > 1e-5f))
            {
                Warn(id, "measures zero");
                UnityEngine.Object.Destroy(holder);
                return null;
            }
            float s = size / measured;
            ft.localRotation = q;
            ft.localScale = new Vector3(s, s, s);
            ft.localPosition = new Vector3(-s * rb.center.x, -s * (centre ? rb.center.y : rb.min.y), -s * rb.center.z);
            foreach (Renderer r in inst.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                r.receiveShadows = shadows;
            }
            if (layer >= 0) SetLayer(holder, layer);
            return holder;
        }

        /// <summary>The model's colliders, rigid bodies, cameras, lights and audio components removed (a model is a picture).</summary>
        static void Strip(GameObject go)
        {
            foreach (Collider c in go.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.Destroy(c);
            foreach (Rigidbody r in go.GetComponentsInChildren<Rigidbody>(true)) UnityEngine.Object.Destroy(r);
            foreach (Camera c in go.GetComponentsInChildren<Camera>(true)) { c.enabled = false; UnityEngine.Object.Destroy(c); }
            foreach (Light l in go.GetComponentsInChildren<Light>(true)) { l.enabled = false; UnityEngine.Object.Destroy(l); }
            foreach (AudioListener a in go.GetComponentsInChildren<AudioListener>(true)) UnityEngine.Object.Destroy(a);
            foreach (AudioSource a in go.GetComponentsInChildren<AudioSource>(true)) UnityEngine.Object.Destroy(a);
        }

        /// <summary>
        /// A material whose shader this pipeline cannot draw (a built-in Standard from the FBX, or a broken one) is swapped for URP Lit on THIS instance (a copy: the asset's materials are never
        /// written), with its texture and colour kept; a URP / Shader Graph / SKYGRIND material is left alone.
        /// </summary>
        static void FixShaders(GameObject inst)
        {
            Shader lit = null;
            foreach (Renderer r in inst.GetComponentsInChildren<Renderer>(true))
            {
                Material[] ms = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < ms.Length; i++)
                {
                    Material m = ms[i];
                    if (m == null) continue;
                    Shader sh = m.shader;
                    string n = sh != null ? sh.name : "";
                    bool fine = sh != null && sh.isSupported && (n.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal) || n.StartsWith("Shader Graphs/", StringComparison.Ordinal)
                        || n.StartsWith("SKYGRIND/", StringComparison.Ordinal));
                    if (fine) continue;
                    lit ??= Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Universal Render Pipeline/Unlit");
                    if (lit == null) continue;
                    var copy = new Material(lit) { name = m.name + " (tripo)" };
                    if (m.mainTexture != null) copy.SetTexture("_BaseMap", m.mainTexture);
                    if (m.HasProperty("_Color")) copy.SetColor("_BaseColor", m.GetColor("_Color"));
                    else if (m.HasProperty("_BaseColor")) copy.SetColor("_BaseColor", m.GetColor("_BaseColor"));
                    ms[i] = copy;
                    changed = true;
                }
                if (changed) r.sharedMaterials = ms;
            }
        }

        /// <summary>
        /// PG3 (GG 2026-10-28: the models "look very desaturated"): the model draws with a copy of the TripoGlow material (URP Lit, smoothness 0.3, emission on) carrying the model's own graded
        /// base map, normal map and accent emission map (Resources/Tripo/Textures/&lt;id&gt;_basecolor / _normal / _emission), the accents at TripoGlowRest. One copy per id. No glow material or
        /// no emission map for the id: the model keeps the FBX's material.
        /// </summary>
        static void Glow(string id, GameObject inst)
        {
            if (!glowMats.TryGetValue(id, out Material gm))
            {
                gm = null;
                Material template = Resources.Load<Material>(UnityTuning.TripoGlowMaterial);
                Texture2D em = Resources.Load<Texture2D>(UnityTuning.TripoFolder + "Textures/" + id + "_emission");
                Renderer r0 = inst.GetComponentInChildren<Renderer>(true);
                Material src = r0 != null ? r0.sharedMaterial : null;
                if (template != null && em != null && src != null)
                {
                    gm = new Material(template) { name = id + " (tripo glow)" };
                    if (src.HasProperty("_BaseMap") && src.GetTexture("_BaseMap") != null) gm.SetTexture("_BaseMap", src.GetTexture("_BaseMap"));
                    else if (src.mainTexture != null) gm.SetTexture("_BaseMap", src.mainTexture);
                    gm.SetTexture("_BumpMap", src.HasProperty("_BumpMap") ? src.GetTexture("_BumpMap") : null);
                    gm.SetTexture("_EmissionMap", em);
                    gm.SetColor("_EmissionColor", Color.white * UnityTuning.TripoGlowRest);
                }
                glowMats[id] = gm;
            }
            if (gm == null) return;
            foreach (Renderer r in inst.GetComponentsInChildren<Renderer>(true))
            {
                Material[] ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++) ms[i] = gm;
                r.sharedMaterials = ms;
            }
        }

        static void Warn(string id, string what)
        {
            if (warned.Add(id)) Debug.LogWarning("[tripo] " + id + " " + what + ": not drawn");
        }

        /// <summary>The union of the model's meshes' bounds in `space`'s local space (MeshFilter and SkinnedMeshRenderer meshes, every child, inactive ones too).</summary>
        static bool Measure(Transform space, GameObject go, out Bounds bounds)
        {
            bool any = false;
            bounds = default;
            Matrix4x4 toSpace = space.worldToLocalMatrix;
            foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>(true))
                if (mf.sharedMesh != null) Grow(ref bounds, ref any, toSpace * mf.transform.localToWorldMatrix, mf.sharedMesh.bounds);
            foreach (SkinnedMeshRenderer sm in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (sm.sharedMesh != null) Grow(ref bounds, ref any, toSpace * sm.transform.localToWorldMatrix, sm.sharedMesh.bounds);
            return any;
        }

        static void Grow(ref Bounds b, ref bool any, Matrix4x4 m, Bounds local)
        {
            Vector3 c = local.center, e = local.extents;
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = m.MultiplyPoint3x4(c + new Vector3((i & 1) == 0 ? -e.x : e.x, (i & 2) == 0 ? -e.y : e.y, (i & 4) == 0 ? -e.z : e.z));
                if (!any) { b = new Bounds(p, Vector3.zero); any = true; }
                else b.Encapsulate(p);
            }
        }

        static Bounds Rotated(Bounds b, Quaternion q)
        {
            Vector3 c = b.center, e = b.extents;
            Bounds r = default;
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = q * (c + new Vector3((i & 1) == 0 ? -e.x : e.x, (i & 2) == 0 ? -e.y : e.y, (i & 4) == 0 ? -e.z : e.z));
                if (i == 0) r = new Bounds(p, Vector3.zero);
                else r.Encapsulate(p);
            }
            return r;
        }

        /// <summary>`go` and every child on `layer` (Viewmodel.SetLayer sets one object; the overlay camera needs all of them).</summary>
        public static void SetLayer(GameObject go, int layer)
        {
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;
        }

        /// <summary>A part of a kit or a boss that is a solid body (an opaque MeshRenderer), not a glow, a decal or a light (those draw on the transparent queue, additively).</summary>
        public static bool IsSolid(Renderer r)
        {
            if (!(r is MeshRenderer)) return false;
            Material m = r.sharedMaterial;
            return m != null && m.renderQueue < (int)RenderQueue.Transparent;
        }

        internal static void Made(string id)
        {
            made.TryGetValue(id, out int n);
            made[id] = n + 1;
        }

        // --- the enemies' hit flash: the views tint their pill renderers (EnemyLook.Tint); a pill renderer registered here hands its flash to the slot's model ---

        /// <summary>Is any enemy pill registered (one bool in EnemyLook.Tint when none is)?</summary>
        public static bool FlashActive => flashFor.Count > 0;

        /// <summary>`pill` (an enemy view's renderer) drives the flash of `slot`'s model.</summary>
        public static void RegisterFlash(Renderer pill, TripoSlot slot)
        {
            if (pill != null) flashFor[pill] = slot;
        }

        /// <summary>EnemyLook.Tint's call: the flash share of a pill renderer's body, forwarded to its model (nothing for a renderer that is not registered).</summary>
        public static void EnemyTint(Renderer pill, float flash)
        {
            if (flashFor.TryGetValue(pill, out TripoSlot slot)) slot.Flash(flash);
        }

        /// <summary>The Player's log line: the row, the ids that loaded with their instance counts, the ids that were looked for and are missing.</summary>
        public static string Line()
        {
            var sb = new StringBuilder("[tripo] row ").Append(onNow ? "on" : "off").Append(" | loaded:");
            bool any = false;
            foreach (KeyValuePair<string, GameObject> kv in prefabs)
            {
                if (kv.Value == null) continue;
                made.TryGetValue(kv.Key, out int n);
                sb.Append(' ').Append(kv.Key).Append(" x").Append(n);
                any = true;
            }
            if (!any) sb.Append(" none");
            sb.Append(" | missing:");
            any = false;
            foreach (KeyValuePair<string, GameObject> kv in prefabs)
            {
                if (kv.Value != null) continue;
                sb.Append(' ').Append(kv.Key);
                any = true;
            }
            if (!any) sb.Append(" none");
            return sb.ToString();
        }
    }

    /// <summary>One model on one view (a gun rig, a pooled enemy body, a boss): see the file's header. Not thread safe; main thread only.</summary>
    public sealed class TripoSlot
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"), ColorId = Shader.PropertyToID("_Color");

        public readonly string id;
        /// <summary>The holder of the attached model (Unity-null when none, or when its parent rebuilt and destroyed it).</summary>
        public GameObject Model;
        bool probed, missing, shown;
        float lastFlash = -1f;
        Renderer[] rends;
        Color[] baseColors;
        MaterialPropertyBlock block;
        readonly List<Renderer> hidden = new List<Renderer>();
        readonly List<ShadowCastingMode> hiddenShadows = new List<ShadowCastingMode>();

        public TripoSlot(string id) => this.id = id;

        /// <summary>No file for this id (or the model could not be fitted): a view does nothing more for this slot, ever. The first call looks for the file (one Resources.Load for the id).</summary>
        public bool Missing
        {
            get
            {
                if (!probed)
                {
                    probed = true;
                    missing = TripoModels.Prefab(id) == null;
                }
                return missing;
            }
        }

        /// <summary>Is the model drawing now (the row on and attached)?</summary>
        public bool Shown => shown && Model != null;

        /// <summary>The slot has a file, no model yet (or a destroyed one) and the row is on: the view should call Attach now.</summary>
        public bool NeedsAttach(GameConfig cfg) => !Missing && Model == null && TripoModels.On(cfg);

        /// <summary>Make the model (TripoModels.Attach's general form) and keep its renderers for the flash; it stays hidden until Show(cfg). null: the file would not fit (the slot is then Missing).</summary>
        public GameObject Attach(Transform parent, TripoModels.Fit fit, float size, Quaternion orient, bool alongZ, bool centre, int layer, bool shadows)
        {
            hidden.Clear();
            hiddenShadows.Clear();
            shown = false;
            lastFlash = -1f;
            Model = TripoModels.Attach(parent, id, fit, size, orient, alongZ, centre, layer, shadows);
            if (Model == null)
            {
                missing = true;
                return null;
            }
            Model.SetActive(false);
            Rebind();
            TripoModels.Made(id);
            return Model;
        }

        /// <summary>Collect the model's drawing renderers again (after TripoLimbs cut it into parts: the flash and the glow then reach the parts).</summary>
        public void Rebind()
        {
            if (Model == null) return;
            var all = new List<Renderer>();
            foreach (Renderer x in Model.GetComponentsInChildren<Renderer>(true)) if (x.enabled) all.Add(x);
            rends = all.ToArray();
            lastFlash = -1f;
            lastGlow = -1f;
            baseColors = new Color[rends.Length];
            for (int i = 0; i < rends.Length; i++)
            {
                Material m = rends[i].sharedMaterial;
                baseColors[i] = m == null ? Color.white : m.HasProperty(BaseColorId) ? m.GetColor(BaseColorId) : m.HasProperty(ColorId) ? m.GetColor(ColorId) : Color.white;
            }
        }

        /// <summary>A pill part the model replaces: not drawn while the model shows (forceRenderingOff and no shadow; the renderer, its bounds and its property blocks stay), drawn again when it hides.</summary>
        public void Hide(Renderer r)
        {
            if (r == null || hidden.Contains(r)) return;
            hidden.Add(r);
            hiddenShadows.Add(r.shadowCastingMode);
            if (shown) Apply(hidden.Count - 1);
        }

        /// <summary>Every solid (opaque mesh) renderer under `root` is a part the model replaces, but not `keep` (a gun's gem) nor a renderer drawn with `keepMat`, nor the model itself.</summary>
        public void HideSolids(Transform root, Renderer keep = null, Material keepMat = null)
        {
            if (root == null) return;
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == keep || !TripoModels.IsSolid(r)) continue;
                if (Model != null && r.transform.IsChildOf(Model.transform)) continue;
                if (keepMat != null && r.sharedMaterial == keepMat) continue;
                Hide(r);
            }
        }

        /// <summary>This frame's answer from the row: show the model and hide the pill parts, or the reverse (nothing to do while it is the same answer as the last frame). `reassert`: another system
        /// also flips forceRenderingOff on these parts every frame (THE ICEBREAKER's nearFade63), so while the model shows the hidden parts are set off again each call.</summary>
        public void Show(GameConfig cfg, bool reassert = false)
        {
            if (Model == null) return;
            bool want = TripoModels.On(cfg);
            if (want == shown && Model.activeSelf == want)
            {
                if (reassert && shown)
                    for (int i = 0; i < hidden.Count; i++)
                        if (hidden[i] != null && !hidden[i].forceRenderingOff) hidden[i].forceRenderingOff = true;
                return;
            }
            shown = want;
            Model.SetActive(want);
            for (int i = 0; i < hidden.Count; i++) Apply(i);
            if (!want) lastFlash = -1f;
        }

        void Apply(int i)
        {
            Renderer r = hidden[i];
            if (r == null) return;
            r.forceRenderingOff = shown;
            r.shadowCastingMode = shown ? ShadowCastingMode.Off : hiddenShadows[i];
        }

        /// <summary>The hit flash (a share 0..1): the model's renderers' base colour x (1 + TripoFlashBoost x flash) through a property block; 0 clears it. Only while the model shows.</summary>
        float lastGlow = -1f;
        static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        /// <summary>A gun model's accents at `k` x the emission map (the charge: Viewmodel.TripoCharge). A gun's slot never flashes (Flash is the enemies'), so the block is this one's alone.</summary>
        public void Glow(float k)
        {
            if (!Shown || rends == null || Mathf.Abs(k - lastGlow) < 0.01f) return;
            lastGlow = k;
            block ??= new MaterialPropertyBlock();
            block.Clear();
            block.SetColor(EmissionColorId, Color.white * k);
            for (int i = 0; i < rends.Length; i++) if (rends[i] != null) rends[i].SetPropertyBlock(block);
        }

        public void Flash(float flash)
        {
            if (!shown || Model == null || rends == null || Mathf.Abs(flash - lastFlash) < 0.002f) return;
            lastFlash = flash;
            block ??= new MaterialPropertyBlock();
            float k = 1f + UnityTuning.TripoFlashBoost * Mathf.Clamp01(flash);
            for (int i = 0; i < rends.Length; i++)
            {
                Renderer r = rends[i];
                if (r == null) continue;
                if (flash <= 0.002f)
                {
                    r.SetPropertyBlock(null);
                    continue;
                }
                Color c = baseColors[i];
                var t = new Color(c.r * k, c.g * k, c.b * k, c.a);
                block.Clear();
                block.SetColor(BaseColorId, t);
                block.SetColor(ColorId, t);
                r.SetPropertyBlock(block);
            }
        }
    }
}
