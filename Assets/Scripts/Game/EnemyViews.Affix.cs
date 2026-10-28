// NIGHT19 U3-33.7 (builder E owns this file): the affixes' look as affixes.js draws it (rebuilt as it is), a new partial of
// SurvivalGame through the shared commit's hooks (N19AffixViewInit / N19AffixViewRender). SHIELDED tonight:
// - the icon: affixes.kinds.shielded.icon's 8 x 8 pixel rows ('#' lit) in its colour (affixes.kinds.shielded.color x 1.6,
//   the web's MeshBasicMaterial colour (1.6, 1.6, 1.6) over the canvas texture), affixes.iconSize m wide, affixes.iconHeight
//   m over the enemy's feet, facing the camera (the web billboards with the camera's quaternion), at most affixes.maxIcons.
//   The port draws the lit pixels as one flat mesh (no texture or alpha test: the neon material, opaque, as the web's
//   alphaTest 0.5 cut-out reads);
// - the plate breaking (affixBreak): affixes.js's particles.burst at +1.1 m, in the affix colour, 16 cubes, speed 7, life
//   0.4, size 1.2.
// Nothing draws while no enemy is shielded (the pool hides); with design.enemies.n19 off nothing is ever shielded.
// NIGHT20 U3-34.9 C3 (design.adds.affixes, Affixes.all): every affix's icon (affixes.kinds.<name>.icon in its colour x 1.6),
// a row of them when there are two (affixes.js update: offset (i - (n - 1) / 2) x iconSize x 1.2 along the camera's right);
// VOLATILE's tell (the web's telegraph.circle): a DANGER disc of affixes.kinds.volatile.radius filling over its fuse from
// the kill (affixVolatile), gone at the burst; the BLINKER's blink (telegraph.flash at both ends): a white flash 0.9 m wide
// at the spot it left and the spot it reached, fading over UnityTuning.AffixBlinkFlash s. Pooled; nothing per frame allocates.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        AffixView affixView;

        partial void N19AffixViewInit()
        {
            affixView = new AffixView(this);
            Color plate = Look.C(cfg, "affixes.kinds.shielded.color");
            events.On<AffixEvent>(EnemyHits.BreakEvent, e =>
            {
                // affixes.js filter: particles.burst(position + 1.1 up, shielded.color, 16, 7, 0.4, 1.2)
                foreach (Enemy en in enemies.all)
                {
                    if (en.id != e.id) continue;
                    particles?.Burst(new Vec3(en.position.x, en.position.y + 1.1, en.position.z), plate, 16, 7, 0.4f, 1.2f);
                    break;
                }
            });
        }

        partial void N19AffixViewRender(double alpha, double frameDt) => affixView?.Render(alpha, frameDt);
    }

    public sealed class AffixView
    {
        readonly SurvivalGame game;
        readonly Transform parent;
        readonly List<Transform> icons = new List<Transform>();
        readonly Mesh shieldMesh;
        readonly Material mat;
        readonly int maxIcons;
        readonly float iconHeight;

        // NIGHT20 C3: one icon pool per affix (affixes.js AFFIXES order), VOLATILE's discs, the blink flashes.
        readonly Mesh[] kindMesh = new Mesh[AffixMath.Names.Length];
        readonly Material[] kindMat = new Material[AffixMath.Names.Length];
        readonly List<Transform>[] kindIcons = new List<Transform>[AffixMath.Names.Length];
        readonly float iconSize;
        readonly Mesh disc;
        readonly Material discMat, flashMat;
        readonly List<Fuse> fuses = new List<Fuse>();
        readonly List<Flash> flashes = new List<Flash>();

        sealed class Fuse
        {
            public Transform t;
            public int id;
            public float age, time, radius;
            public bool on;
        }

        sealed class Flash
        {
            public Transform t;
            public float age;
            public bool on;
        }

        public AffixView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            parent = new GameObject("Affix icons").transform;
            maxIcons = cfg.Int("affixes.maxIcons");
            iconHeight = (float)cfg.Num("affixes.iconHeight");
            iconSize = (float)cfg.Num("affixes.iconSize");
            shieldMesh = IconMesh(cfg.Strs("affixes.kinds.shielded.icon"), (float)cfg.Num("affixes.iconSize"));
            mat = new Material(game.mats.neon) { name = "Affix SHIELDED" };
            mat.SetColor(Shader.PropertyToID("_BaseColor"), Look.C(cfg, "affixes.kinds.shielded.color", 1.6f));
            for (int k = 0; k < AffixMath.Names.Length; k++)
            {
                string name = AffixMath.Names[k];
                kindMesh[k] = name == "shielded" ? shieldMesh : IconMesh(cfg.Strs($"affixes.kinds.{name}.icon"), iconSize);
                kindMat[k] = name == "shielded" ? mat : new Material(game.mats.neon) { name = "Affix " + name.ToUpperInvariant() };
                if (name != "shielded") kindMat[k].SetColor(Shader.PropertyToID("_BaseColor"), Look.C(cfg, $"affixes.kinds.{name}.color", 1.6f));
                kindIcons[k] = new List<Transform>();
            }
            disc = Disc(32);
            discMat = new Material(game.mats.neon) { name = "Affix VOLATILE fuse" };
            discMat.SetColor(Shader.PropertyToID("_BaseColor"), Look.C(cfg, "colors.enemy", UnityTuning.AffixFuseGain)); // DANGER
            flashMat = new Material(game.mats.neon) { name = "Affix blink flash" };
            flashMat.SetColor(Shader.PropertyToID("_BaseColor"), Look.Hex(0xffffff, UnityTuning.AffixBlinkGain));
            game.events.On<AffixFxEvent>("affixVolatile", OnVolatile);
            game.events.On<AffixFxEvent>("affixBurst", OnBurst);
            game.events.On<AffixFxEvent>("affixBlink", OnBlink);
        }

        /// <summary>A flat disc of radius 1 in the xz plane (both faces), the fuse's circle.</summary>
        static Mesh Disc(int sides)
        {
            var verts = new Vector3[sides + 1];
            var tris = new int[sides * 6];
            verts[0] = Vector3.zero;
            for (int i = 0; i < sides; i++)
            {
                float a = i / (float)sides * Mathf.PI * 2;
                verts[i + 1] = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                int b = i + 1, c = (i + 1) % sides + 1;
                tris[i * 6] = 0; tris[i * 6 + 1] = c; tris[i * 6 + 2] = b;
                tris[i * 6 + 3] = 0; tris[i * 6 + 4] = b; tris[i * 6 + 5] = c;
            }
            var m = new Mesh { name = "affix fuse", vertices = verts, triangles = tris };
            m.RecalculateBounds();
            return m;
        }

        Transform Piece(string name, Mesh mesh, Material m)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            go.SetActive(false);
            return go.transform;
        }

        void OnVolatile(AffixFxEvent e)
        {
            if (game.Telegraphs != null) return; // NIGHT20 (the lead, builder HV): the web's telegraph circle draws the fuse (Telegraph20)
            Fuse f = null;
            foreach (Fuse x in fuses) if (!x.on) { f = x; break; }
            if (f == null) fuses.Add(f = new Fuse { t = Piece("volatile fuse", disc, discMat) });
            f.on = true;
            f.id = e.id;
            f.age = 0;
            f.time = (float)e.time;
            f.radius = (float)e.radius;
            f.t.position = WebSpace.ToUnity(new Vec3(e.position.x, e.position.y + 0.05, e.position.z));
            f.t.localScale = Vector3.zero;
            f.t.gameObject.SetActive(true);
        }

        void OnBurst(AffixFxEvent e)
        {
            foreach (Fuse f in fuses)
            {
                if (!f.on || f.id != e.id) continue;
                f.on = false;
                f.t.gameObject.SetActive(false);
            }
        }

        void OnBlink(AffixFxEvent e)
        {
            AddFlash(e.position);
            AddFlash(e.to);
        }

        void AddFlash(Vec3 at)
        {
            Flash f = null;
            foreach (Flash x in flashes) if (!x.on) { f = x; break; }
            if (f == null) flashes.Add(f = new Flash { t = Piece("blink flash", disc, flashMat) });
            f.on = true;
            f.age = 0;
            f.t.position = WebSpace.ToUnity(new Vec3(at.x, at.y + 1, at.z));
            f.t.gameObject.SetActive(true);
        }

        /// <summary>The icon's lit pixels as quads in one mesh, `size` m square, centred, facing +z (made once).</summary>
        static Mesh IconMesh(List<string> rows, float size)
        {
            int h = rows.Count, w = rows[0].Length;
            float px = size / w;
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (rows[y][x] != '#') continue;
                    float x0 = -size / 2 + x * px, y0 = size / 2 - (y + 1) * px; // row 0 on top
                    int v = verts.Count;
                    verts.Add(new Vector3(x0, y0, 0));
                    verts.Add(new Vector3(x0 + px, y0, 0));
                    verts.Add(new Vector3(x0 + px, y0 + px, 0));
                    verts.Add(new Vector3(x0, y0 + px, 0));
                    tris.AddRange(new[] { v, v + 2, v + 1, v, v + 3, v + 2, v, v + 1, v + 2, v, v + 2, v + 3 }); // both faces
                }
            }
            var m = new Mesh { name = "affix icon" };
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateBounds();
            return m;
        }

        Transform Make()
        {
            var go = new GameObject("shielded");
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = shieldMesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return go.transform;
        }

        public void Render(double alpha, double frameDt)
        {
            float fdt = (float)frameDt;
            foreach (Fuse f in fuses)
            {
                if (!f.on) continue;
                f.age += fdt;
                float r = f.radius * Mathf.Clamp01(f.time > 0 ? f.age / f.time : 1); // the circle fills over the fuse
                f.t.localScale = new Vector3(r, 1, r);
            }
            foreach (Flash f in flashes)
            {
                if (!f.on) continue;
                f.age += fdt;
                float k = 1 - f.age / UnityTuning.AffixBlinkFlash;
                if (k <= 0)
                {
                    f.on = false;
                    f.t.gameObject.SetActive(false);
                    continue;
                }
                float w = UnityTuning.AffixBlinkSize * 0.5f * k;
                f.t.localScale = new Vector3(w, 1, w);
                if (game.cam != null) f.t.rotation = game.cam.transform.rotation * Quaternion.Euler(90, 0, 0); // facing you
            }
            Affixes ax = game.sim?.affixes;
            if (ax != null && ax.all)
            {
                RenderAll(ax, alpha);
                return;
            }
            int n = 0;
            Camera cam = game.cam;
            foreach (Enemy e in game.enemies.all)
            {
                if (!e.alive || !e.Shielded || n >= maxIcons) continue;
                if (n >= icons.Count) icons.Add(Make());
                Transform t = icons[n++];
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                Vec3 p = e.previous + (e.position - e.previous) * alpha;
                t.position = WebSpace.ToUnity(new Vec3(p.x, p.y + iconHeight, p.z));
                if (cam != null) t.rotation = cam.transform.rotation; // billboard (the web copies the camera's quaternion)
            }
            for (int i = n; i < icons.Count; i++)
                if (icons[i].gameObject.activeSelf) icons[i].gameObject.SetActive(false);
        }

        /// <summary>NIGHT20 C3: affixes.js update: each live affixed enemy's icons, a row facing the camera.</summary>
        void RenderAll(Affixes ax, double alpha)
        {
            for (int i = 0; i < icons.Count; i++)
                if (icons[i].gameObject.activeSelf) icons[i].gameObject.SetActive(false);
            Camera cam = game.cam;
            Vector3 right = cam != null ? cam.transform.right : Vector3.right;

            for (int k = 0; k < counts.Length; k++) counts[k] = 0;
            IReadOnlyList<AffixRec> live = ax.Live;
            for (int i = 0; i < live.Count; i++)
            {
                AffixRec r = live[i];
                if (r.h == null || !r.h.alive) continue;
                Vec3 p = r.h.previous + (r.h.position - r.h.previous) * alpha;
                Vector3 head = WebSpace.ToUnity(new Vec3(p.x, p.y + iconHeight, p.z));
                for (int j = 0; j < r.names.Count; j++)
                {
                    int k = System.Array.IndexOf(AffixMath.Names, r.names[j]);
                    if (k < 0 || counts[k] >= maxIcons) continue;
                    List<Transform> pool = kindIcons[k];
                    if (counts[k] >= pool.Count) pool.Add(Piece(r.names[j], kindMesh[k], kindMat[k]));
                    Transform t = pool[counts[k]++];
                    if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                    float off = (j - (r.names.Count - 1) / 2f) * iconSize * 1.2f;
                    t.position = head + right * off;
                    if (cam != null) t.rotation = cam.transform.rotation;
                }
            }
            for (int k = 0; k < kindIcons.Length; k++)
                for (int i = counts[k]; i < kindIcons[k].Count; i++)
                    if (kindIcons[k][i].gameObject.activeSelf) kindIcons[k][i].gameObject.SetActive(false);
        }

        readonly int[] counts = new int[AffixMath.Names.Length];
    }
}
