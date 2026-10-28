// NIGHT40 builder E3 (models40, a view row: design.n40.models40.enabled, read once when a view is made; NIGHT40.md card E3): the silhouette parts each enemy
// view adds to its body (UnityTuning.Models40.cs holds the parts, their roles and the rules). A view with the row on gives every body it makes an Enemy40Parts
// (children of the body's root, so they follow its move, lean, flinch and squash: EnemyLook.React) and poses it each frame from the enemy's state: the
// wind-up (the tell) lights the DANGER / PARRY parts and tips / grows the parts that have a tell pose, a stun pulses the WEAK parts; a dead body's root is
// hidden by its view with the parts on it (their own death pose: not built tonight). With the row off no part is made: the views draw what they drew.
// No collider, no shadow; no Unity API in a field initializer (L-035).
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Enemy40Parts // PG3 tripo74: partial (the Tripo model hides these parts: Enemy43View.Tripo.cs)
    {
        public readonly Transform[] parts;
        public readonly Renderer[] rends;
        /// <summary>NIGHT41 E6 models41: each part's mesh and its colour as last posed (the death break-apart throws them as blocks: EnemyLook.DieParts41).</summary>
        public readonly Mesh[] meshes;
        public readonly Color[] colors;
        readonly int[] role, group;
        readonly Quaternion[] rest;
        readonly Vector3[] restScale;
        readonly float[] tellPitch, tellGrow;
        int shownGroup = -1;
        /// <summary>NIGHT43 A1 models43: built from Mesh43 meshes (Make43): its roles use the models43 values, its parts break on a death (EnemyLook.Die).</summary>
        public bool kit43;
        /// <summary>NIGHT47 F: the kind the models43 parts were made for (Make43; a847's visor meter), null for models40.</summary>
        public string kind47;
        bool hidden; // NIGHT43 A1: Show(false) (models43 swaps these parts out while it draws)

        /// <summary>Is the models40 view row on (design.n40.models40.enabled)?</summary>
        public static bool On(Skygrind.Core.GameConfig cfg) => cfg.Has("design.n40.models40.enabled") && cfg.Bool("design.n40.models40.enabled");

        /// <summary>The kind's parts (UnityTuning.Models40*), or null for a kind with none.</summary>
        public static float[] Spec(string kind) => kind == "husk" ? UnityTuning.Models40Husk : kind == "swarmer" ? UnityTuning.Models40Swarmer
            : kind == "spitter" ? UnityTuning.Models40Spitter : kind == "charger" ? UnityTuning.Models40Charger : kind == "warden" ? UnityTuning.Models40Warden
            : kind == "drone" ? UnityTuning.Models40Drone : null;

        Enemy40Parts(int n)
        {
            parts = new Transform[n];
            rends = new Renderer[n];
            meshes = new Mesh[n];
            colors = new Color[n];
            role = new int[n];
            group = new int[n];
            rest = new Quaternion[n];
            restScale = new Vector3[n];
            tellPitch = new float[n];
            tellGrow = new float[n];
        }

        /// <summary>Build the kind's parts under `root` (`height`: the kind's height in m; the DRONE's parts are placed in m from its centre). `look` attaches the
        /// enemy material (null in a test: the primitive's default).</summary>
        public static Enemy40Parts Make(string kind, Transform root, EnemyLook look, float height, bool models41 = false)
        {
            float[] s = Spec(kind);
            if (s == null) return null;
            // NIGHT41 E6 models41 (a view row): the HUSK's own feature (UnityTuning.Models41Husk) after its NIGHT40 parts; off: NIGHT40's parts only
            if (models41 && kind == "husk")
            {
                var all = new float[s.Length + UnityTuning.Models41Husk.Length];
                s.CopyTo(all, 0);
                UnityTuning.Models41Husk.CopyTo(all, s.Length);
                s = all;
            }
            int stride = UnityTuning.Models40Stride, n = s.Length / stride;
            var m = new Enemy40Parts(n);
            float hy = kind == "drone" ? 1 : height;
            for (int i = 0; i < n; i++)
            {
                int o = i * stride;
                PrimitiveType prim = s[o] == 1 ? PrimitiveType.Sphere : s[o] == 2 ? PrimitiveType.Cylinder : PrimitiveType.Cube;
                var go = GameObject.CreatePrimitive(prim);
                go.name = kind + " part40 " + i;
                Collider col = go.GetComponent<Collider>();
                if (Application.isPlaying) Object.Destroy(col);
                else Object.DestroyImmediate(col);
                Transform t = go.transform;
                t.SetParent(root, false);
                t.localPosition = new Vector3(s[o + 1], s[o + 2] * hy, s[o + 3]);
                // a cylinder primitive is 2 m tall: its y scale is half the part's height
                t.localScale = new Vector3(s[o + 4], prim == PrimitiveType.Cylinder ? s[o + 5] * 0.5f : s[o + 5], s[o + 6]);
                t.localRotation = Quaternion.Euler(s[o + 7], s[o + 8], s[o + 9]);
                m.parts[i] = t;
                m.rest[i] = t.localRotation;
                m.restScale[i] = t.localScale;
                m.role[i] = (int)s[o + 10];
                m.group[i] = (int)s[o + 11];
                m.tellPitch[i] = s[o + 12];
                m.tellGrow[i] = s[o + 13];
                Renderer r = go.GetComponent<Renderer>();
                m.meshes[i] = go.GetComponent<MeshFilter>().sharedMesh;
                r.shadowCastingMode = ShadowCastingMode.Off;
                look?.Attach(r, false, false);
                m.rends[i] = r;
            }
            return m;
        }

        /// <summary>NIGHT43 A1 models43: an extra part set from Mesh43 meshes (UnityTuning.Models43.cs's layout). `anchors[k]` is the view's renderer
        /// k's transform (anchor k + 1); anchor 0 is `root` (y x `height`); 100 + j an earlier extra part. `look` attaches the enemy material (null in a test).</summary>
        public static Enemy40Parts Make43(string kind, float[] s, Transform root, Transform[] anchors, EnemyLook look, float height)
        {
            if (s == null) return null;
            int stride = UnityTuning.Models43Stride, n = s.Length / stride;
            var m = new Enemy40Parts(n) { kit43 = true, kind47 = kind };
            m.meshId47 = new int[n]; // NIGHT47 F visor47: each part's Mesh43 id (the swap's key)
            for (int i = 0; i < n; i++) m.meshId47[i] = (int)s[i * stride];
            for (int i = 0; i < n; i++)
            {
                int o = i * stride;
                int anchor = (int)s[o + 1];
                var go = new GameObject(kind + " part43 " + i);
                Transform t = go.transform;
                Transform parent = anchor == 0 ? root : anchor >= 100 ? m.parts[anchor - 100] : anchors[anchor - 1];
                t.SetParent(parent, false);
                t.localPosition = new Vector3(s[o + 2], s[o + 3] * (anchor == 0 ? height : 1), s[o + 4]);
                t.localScale = new Vector3(s[o + 5], s[o + 6], s[o + 7]);
                t.localRotation = Quaternion.Euler(s[o + 8], s[o + 9], s[o + 10]);
                Mesh mesh = Mesh43.Get((int)s[o]);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                Renderer r = go.AddComponent<MeshRenderer>();
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
                look?.Attach(r, false, false);
                m.parts[i] = t;
                m.rest[i] = t.localRotation;
                m.restScale[i] = t.localScale;
                m.role[i] = (int)s[o + 11];
                m.group[i] = (int)s[o + 12];
                m.tellPitch[i] = s[o + 13];
                m.tellGrow[i] = s[o + 14];
                m.meshes[i] = mesh;
                m.rends[i] = r;
            }
            return m;
        }

        /// <summary>Show group 1 (a husk) or 2 (a tank) beside the always-shown parts (the husk view draws both on one body).</summary>
        public void ShowGroup(int g)
        {
            if (g == shownGroup) return;
            shownGroup = g;
            if (hidden) return; // NIGHT43 A1: hidden parts stay hidden (Show(true) applies the group)
            for (int i = 0; i < parts.Length; i++)
                if (group[i] != 0) parts[i].gameObject.SetActive(group[i] == g);
        }

        /// <summary>NIGHT43 A1 models43: every part shown (its group's rule kept) or hidden (models43 draws its own set; the reel turns it on / off live).</summary>
        public void Show(bool on)
        {
            hidden = !on;
            for (int i = 0; i < parts.Length; i++)
            {
                bool want = on && (group[i] == 0 || shownGroup < 0 || group[i] == shownGroup);
                if (parts[i].gameObject.activeSelf != want) parts[i].gameObject.SetActive(want);
            }
        }

        /// <summary>NIGHT43 A1: the parts are shown (not swapped out by models43).</summary>
        public bool Shown => !hidden;

        /// <summary>The frame's pose: `body` the body's colour as the view drew it, `tell` 0..1 the wind-up (0: none), `stunned` its weak point open,
        /// `danger` the red-orange, `parry` the yellow, `t` s (the pulse's clock).</summary>
        // NIGHT47 F visor47 (Mesh43.Visor47.cs): the band visors swapped for the angular glasses while the row is on (back when it is off)
        int[] meshId47;
        int visor47Shown; // NIGHT48 F: 0 the base meshes, 1 visor47's bands, 3 visor47 + foes48's lens bands (Mesh43.Visor48.cs)

        void Visor47Swap(EnemyLook look)
        {
            if (meshId47 == null || look == null) return;
            bool want = Visor47.On(look.Cfg);
            int state = want ? (Foes48.On(look.Cfg) ? 3 : 1) : 0;
            if (state == visor47Shown) return;
            visor47Shown = state;
            for (int i = 0; i < parts.Length; i++)
            {
                if (role[i] != UnityTuning.Visor47Role) continue;
                Mesh m = state == 3 ? Mesh43.Visor48Of(meshId47[i], kind47, Matrix4x4.TRS(parts[i].localPosition, rest[i], restScale[i])) : null; // NIGHT48 F foes48 (G13): the lens visors' bands
                if (m == null) m = want ? Mesh43.Visor47Of(meshId47[i]) : Mesh43.Get(meshId47[i]);
                if (m == null) m = Mesh43.Get(meshId47[i]); // a lens visor without its band (foes48 off again): the base mesh back
                if (m == null) continue;
                parts[i].GetComponent<MeshFilter>().sharedMesh = m;
                meshes[i] = m;
            }
        }

        public void Pose(EnemyLook look, MaterialPropertyBlock block, Color body, float flash, float tell, bool stunned, Color danger, Color parry, float t)
        {
            if (hidden) return; // NIGHT43 A1: swapped out by models43
            if (kit43) Visor47Swap(look); // NIGHT47 F visor47
            for (int i = 0; i < parts.Length; i++)
            {
                if (!parts[i].gameObject.activeSelf) continue;
                Color c;
                switch (role[i])
                {
                    case 5:
                        // NIGHT43 A1 models43: the VISOR: always lit, hotter (and toward white) in the wind-up, guttering while stunned
                        c = Color.Lerp(Look.Hex(UnityTuning.Models43VisorHex), Color.white, UnityTuning.Models43VisorTellWhite * tell)
                            * Mathf.Lerp(UnityTuning.Models43VisorRest, UnityTuning.Models43VisorTell, tell);
                        if (stunned) c *= UnityTuning.Models43VisorStun * (0.6f + 0.4f * Mathf.Abs(Mathf.Sin(t * UnityTuning.Models43VisorStunHz)));
                        c *= 1 + look.NavFlicker43; // NIGHT43 A1 round 2: E1's stuck tell strobes it (EnemyLook.React set it for this enemy)
                        break;
                    case 6: c = body * UnityTuning.Models43Trim; break; // NIGHT43 A1: TRIM, the plates' edges
                    case 1: c = Color.Lerp(body, Color.white, UnityTuning.Models40Bright); break;
                    case 2: c = danger * Mathf.Lerp(UnityTuning.Models40GlowRest, UnityTuning.Models40GlowTell, tell); break;
                    case 3: c = parry * Mathf.Lerp(UnityTuning.Models40GlowRest, UnityTuning.Models40GlowTell, tell); break;
                    case 4:
                        c = Look.Hex(UnityTuning.Models40WeakHex) * (stunned ? 1.2f + 0.8f * Mathf.Abs(Mathf.Sin(t * UnityTuning.Models40WeakPulseHz)) : 0.7f);
                        break;
                    default: c = body * (kit43 ? UnityTuning.Models43Dark : UnityTuning.Models40Dark); break;
                }
                c.a = 1;
                colors[i] = c;
                if (role[i] == 5 && kit43 && Visor47Probe.Recording) Visor47Probe.Note(kind47, rends[i].bounds); // NIGHT47 F G13: a847's visor meter
                if (tellPitch[i] != 0) parts[i].localRotation = rest[i] * Quaternion.Euler(tellPitch[i] * tell, 0, 0);
                if (tellGrow[i] != 0) parts[i].localScale = restScale[i] * (1 + tellGrow[i] * tell);
                look.Tint(block, rends[i], c, role[i] >= 2 && role[i] != 6 ? 0 : flash, 1); // NIGHT43 A1: TRIM (armour) flashes with the body
            }
        }
    }
}
