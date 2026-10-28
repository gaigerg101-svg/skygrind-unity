// NIGHT11 U3-24.5: the traps' look, after skygrind/src/survival/traps.js buildBody + update (frame time, draw only; the
// sim is Core's Traps) and assets/machineModels.js buildMachine for each switch:
// - the bodies (traps.js :33-65): the PLATE (a steel slab, half x 2 square, 0.08 thick, and its glow grid 0.3 smaller
//   that hums while on), the CRUSHER (four steel posts rest + 1 tall, the slab half x 2 square, 1.1 thick, at `rest`,
//   with a glowing edge under it; it falls, holds and rises with the cycle: Traps.SlabDown), the FAN (a steel disc of
//   `radius` and three glowing blades that spin x18 rad/s while on, x0.6 idle), the PYLON (a 0.7 m steel column `height`
//   tall and a 0.45 m orb over it that pulses while on); colours from traps.kinds.*.color (the crusher's red-orange is
//   the web's: a danger), steel 0x3a3f4a and the slab 0x6b625c as traps.js;
// - each switch (buildMachine at perks.machineWidth / Height / Depth x traps.switchScale, built from boxes like
//   PerkMachines: the cabinet, plinth, cap, bezel, the CRT with the trap's pixel icon and a TextMeshPro label of its
//   name and the price / ACTIVE / the cooldown's seconds, the trim in the trap's colour and the floor ring), at (sx, y,
//   sz), its screen facing the trap (rotation atan2 as traps.js :181); the text is set only when it changes (traps.js
//   update's key: label, locked, affordable); dimmed when the zone is locked (trim x0.25, the screen at 0.35), x0.45
//   while on or cooling down;
// - the pylon's zaps: fx/arcs.js is not ported, so each zap is a LineRenderer zig-zag (arcs.js's 10 segments, kinks of
//   jag x length that re-kink every 60-90 ms, ends pinned) in the trap's colour x intensity, fading over its 0.16 s life.
// The glow materials are additive copies of the neon (URP Unlit) material, as WeaponFx's (three's additive, opacity
// folded into the colour). Bodies and switches are built once per trap id and relinked when Traps.Version changes (a
// new run's Load).
//
// UnityTuning (render / layout numbers the web doesn't have; the lead moves these into UnityTuning.cs):
//     // NIGHT11 U3-24.5: the trap switches' screen text (world-space TextMeshPro size: the web's CRT draws the name at 0.13
//     // and the price at 0.12 of its 0.86 m screen, letters about 0.11 m), and how many pylon zaps show at once (fx/arcs.js
//     // isn't ported; its one pool of 24 is shared by every effect: the pylon makes at most 4 per 0.5 s, each 0.16 s).
//     public const float TrapLabelFontSize = 1.1f;
//     public const int TrapZapPool = 8;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class TrapViews
    {
        sealed class View
        {
            public string id;
            public Traps.Trap trap;
            public Transform body, sw; // the trap's body and its switch cabinet
            public Transform slab, blades, orb;
            public Material grid, trim, ring, icon;
            public TextMeshPro label;
            public Color color;
            public string nameHex; // the name's colour on the CRT (css(hex))
            public double time;
            public double bladeAngle;
            public int shownMode = -1, shownSecs = -1; // 0 the price, 1 ACTIVE, 2 the cooldown's seconds
            public bool shownLocked, shownAffordable;
        }

        sealed class Zap
        {
            public LineRenderer line;
            public Material mat;
            public bool alive;
            public Vector3 from, to;
            public Color color;
            public float intensity, life, maxLife, jag, width, regen;
            public readonly float[] kinks = new float[(Segments + 1) * 2];
        }

        // traps.js literals
        static readonly Color Steel = Look.Hex(0x3a3f4a), SlabColor = Look.Hex(0x6b625c);
        const float GlowOpacity = 0.85f; // traps.js glow(): opacity 0.85
        // machineModels.js's CRT price line colours: a label (ACTIVE / the cooldown), locked, affordable, not affordable
        const string LabelHex = "#ffd23a", LockedHex = "#8f8a80", AffordableHex = "#ffffff", PoorHex = "#ff6a5a";
        const float LockedDim = 0.35f; // the CRT's globalAlpha when locked
        // fx/arcs.js: segments 10, a re-kink every 60-90 ms
        const int Segments = 10;
        const float RegenMin = 0.06f, RegenMax = 0.09f;

        readonly SurvivalGame game;
        readonly Traps traps;
        readonly GameConfig cfg;
        readonly Transform root;
        readonly Dictionary<string, View> views = new Dictionary<string, View>();
        readonly List<View> shown = new List<View>();
        readonly Zap[] zaps;
        readonly Vector3[] points = new Vector3[Segments + 1];
        readonly Material steel, slabMat, gunmetal, dark, bone, crt;
        readonly float glow;
        int version = -1, nextZap;
        uint hash = 2166136261;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public TrapViews(SurvivalGame game, Traps traps)
        {
            this.game = game;
            this.traps = traps;
            cfg = game.cfg;
            glow = (float)cfg.Num("perks.glow");
            root = new GameObject("Traps").transform;
            steel = new Material(game.mats.lit) { color = Steel };
            slabMat = new Material(game.mats.lit) { color = SlabColor };
            gunmetal = new Material(game.mats.lit) { color = BlockyKit.Gunmetal };
            dark = new Material(game.mats.lit) { color = BlockyKit.Dark };
            bone = new Material(game.mats.lit) { color = BlockyKit.Bone };
            crt = new Material(game.mats.neon);
            crt.SetColor(BaseColor, new Color(0.05f, 0.05f, 0.09f, 1)); // the CRT's dark ground (PerkMachines' icon ground)
            zaps = new Zap[UnityTuning.TrapZapPool];
            for (int i = 0; i < zaps.Length; i++)
            {
                var z = new Zap { mat = Additive(game.mats.neon, "Trap zap") };
                z.line = new GameObject("Trap zap").AddComponent<LineRenderer>();
                z.line.transform.SetParent(root, false);
                z.line.useWorldSpace = true;
                z.line.positionCount = Segments + 1;
                z.line.sharedMaterial = z.mat;
                z.line.shadowCastingMode = ShadowCastingMode.Off;
                z.line.receiveShadows = false;
                z.line.enabled = false;
                zaps[i] = z;
            }
            traps.arcs = Spawn; // arcs.spawn (made once: the sim calls it per zap)
        }

        // --- the bodies and the switches ---------------------------------------------------------------------------

        static Transform Part(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool shadows = true)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            return go.transform;
        }

        static Transform Pivot(string name, Transform parent, Vector3 pos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = pos;
            return t;
        }

        /// <summary>traps.js glow(hex, k): the colour x k, additive, opacity 0.85.</summary>
        Material Glow(Color c, float k)
        {
            Material m = Additive(game.mats.neon, "Trap glow");
            Color g = c * k * GlowOpacity;
            g.a = 1;
            m.SetColor(BaseColor, g);
            return m;
        }

        /// <summary>A copy of the neon (URP Unlit) material drawn additively: Blend One One, no depth write, transparent queue (WeaponFx's).</summary>
        static Material Additive(Material neon, string name)
        {
            var m = new Material(neon) { name = name, renderQueue = (int)RenderQueue.Transparent };
            m.SetFloat("_SrcBlend", (float)BlendMode.One);
            m.SetFloat("_DstBlend", (float)BlendMode.One);
            m.SetFloat("_ZWrite", 0);
            return m;
        }

        View Build(Traps.Trap t)
        {
            TrapSpec spec = t.spec;
            var v = new View { id = t.id, color = Look.Hex(spec.color) };
            v.nameHex = "#" + ColorUtility.ToHtmlStringRGB(v.color);
            // traps.js buildBody (web local coordinates; z flips to Unity's)
            v.body = new GameObject($"Trap {t.id}").transform;
            v.body.SetParent(root, false);
            float half = (float)spec.half, rest = (float)spec.rest, radius = (float)spec.radius, height = (float)spec.height;
            if (t.id == "plate")
            {
                Part(PrimitiveType.Cube, v.body, Vector3.zero, new Vector3(half * 2, 0.08f, half * 2), steel);
                v.grid = Glow(v.color, 0.5f);
                Part(PrimitiveType.Cube, v.body, new Vector3(0, 0.06f, 0), new Vector3(half * 2 - 0.3f, 0.02f, half * 2 - 0.3f), v.grid, shadows: false);
            }
            else if (t.id == "crusher")
            {
                foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) })
                    Part(PrimitiveType.Cube, v.body, new Vector3(x * (half + 0.2f), (rest + 1) / 2, -z * (half + 0.2f)), new Vector3(0.3f, rest + 1, 0.3f), steel);
                v.slab = Pivot("slab", v.body, new Vector3(0, rest, 0));
                Part(PrimitiveType.Cube, v.slab, Vector3.zero, new Vector3(half * 2, 1.1f, half * 2), slabMat);
                Part(PrimitiveType.Cube, v.slab, new Vector3(0, -0.55f, 0), new Vector3(half * 2 + 0.05f, 0.12f, half * 2 + 0.05f), Glow(v.color, 1.2f), shadows: false);
            }
            else if (t.id == "fan")
            {
                Part(PrimitiveType.Cylinder, v.body, new Vector3(0, 0.1f, 0), new Vector3(radius * 2, 0.1f, radius * 2), steel); // a unit cylinder is 2 m tall
                v.blades = Pivot("blades", v.body, new Vector3(0, 0.25f, 0));
                Material blade = Glow(v.color, 0.8f);
                for (int i = 0; i < 3; i++)
                {
                    Transform b = Part(PrimitiveType.Cube, v.blades, Vector3.zero, new Vector3(radius * 1.8f, 0.05f, 0.35f), blade, shadows: false);
                    b.localRotation = Quaternion.Euler(0, -i / 3f * 180f, 0); // rotation.y = (i / 3) * PI
                }
            }
            else if (t.id == "pylon")
            {
                Part(PrimitiveType.Cube, v.body, new Vector3(0, height / 2, 0), new Vector3(0.7f, height, 0.7f), steel);
                v.orb = Part(PrimitiveType.Sphere, v.body, new Vector3(0, height + 0.3f, 0), Vector3.one * 0.9f, Glow(v.color, 1.2f), shadows: false); // radius 0.45
            }
            BuildSwitch(v, t);
            return v;
        }

        /// <summary>machineModels.js buildMachine at the perk machine's size x traps.switchScale (its boxes; front at local -z).</summary>
        void BuildSwitch(View v, Traps.Trap t)
        {
            TrapSpec spec = t.spec;
            float s = (float)traps.switchScale;
            float W = (float)cfg.Num("perks.machineWidth") * s, H = (float)cfg.Num("perks.machineHeight") * s, D = (float)cfg.Num("perks.machineDepth") * s;
            float R = (float)cfg.Num("perks.ringRadius") * s;
            v.sw = new GameObject($"Trap switch {t.id}").transform;
            v.sw.SetParent(root, false);
            Part(PrimitiveType.Cube, v.sw, new Vector3(0, 0.15f + (H - 0.3f) / 2, 0), new Vector3(W, H - 0.3f, D), gunmetal); // the cabinet
            Part(PrimitiveType.Cube, v.sw, new Vector3(0, 0.08f, 0), new Vector3(W + 0.12f, 0.16f, D + 0.12f), dark); // the plinth
            Part(PrimitiveType.Cube, v.sw, new Vector3(0, H - 0.08f, 0), new Vector3(W + 0.1f, 0.16f, D + 0.1f), bone); // the cap
            Part(PrimitiveType.Cube, v.sw, new Vector3(0, H * 0.64f, -(D / 2 + 0.02f)), new Vector3(W * 0.84f, 1.02f, 0.08f), dark); // the bezel
            // The neon trim: the front corners and the cap line (setTrim dims it).
            v.trim = new Material(game.mats.neon);
            Part(PrimitiveType.Cube, v.sw, new Vector3(W / 2 + 0.005f, H / 2, -(D / 2 + 0.005f)), new Vector3(0.045f, H - 0.45f, 0.045f), v.trim, shadows: false);
            Part(PrimitiveType.Cube, v.sw, new Vector3(-W / 2 - 0.005f, H / 2, -(D / 2 + 0.005f)), new Vector3(0.045f, H - 0.45f, 0.045f), v.trim, shadows: false);
            Part(PrimitiveType.Cube, v.sw, new Vector3(0, H - 0.17f, -(D / 2 + 0.06f)), new Vector3(W + 0.14f, 0.035f, 0.035f), v.trim, shadows: false);
            // The CRT (0.72 W x 0.86 m at 0.64 H): the icon at 0.34 of its height from the top, the name and the price line under it.
            float sw = W * 0.72f, sh = 0.86f, cy = H * 0.64f, front = -(D / 2 + 0.065f);
            Part(PrimitiveType.Quad, v.sw, new Vector3(0, cy, front), new Vector3(sw, sh, 1), crt, shadows: false); // a Quad faces -z: the front
            v.icon = new Material(game.mats.neon) { mainTexture = IconTexture(spec.icon, v.color) };
            v.icon.SetTexture("_BaseMap", v.icon.mainTexture);
            v.icon.SetColor(BaseColor, Color.white);
            float iconSize = sw / 13f * (spec.icon.Count + 2); // drawPixelIcon: pixels of floor(w / 13) (the texture has a 1-pixel ground border)
            Part(PrimitiveType.Quad, v.sw, new Vector3(0, cy + sh * (0.5f - 0.34f), front - 0.005f), new Vector3(iconSize, iconSize, 1), v.icon, shadows: false);
            var labelGo = new GameObject("Label", typeof(TextMeshPro));
            labelGo.transform.SetParent(v.sw, false);
            labelGo.transform.localPosition = new Vector3(0, cy + sh * (0.5f - 0.83f), front - 0.01f); // between the name (0.74) and the price (0.92)
            v.label = labelGo.GetComponent<TextMeshPro>();
            v.label.alignment = TextAlignmentOptions.Center;
            v.label.fontSize = UnityTuning.TrapLabelFontSize;
            v.label.enableWordWrapping = false;
            v.label.rectTransform.sizeDelta = new Vector2(sw, sh * 0.4f);
            // The floor ring (a flat disc like PerkMachines'), 0.2 m out in front.
            v.ring = Additive(game.mats.neon, "Trap switch ring");
            Part(PrimitiveType.Cylinder, v.sw, new Vector3(0, 0.03f, -0.2f), new Vector3(R * 2, 0.005f, R * 2), v.ring, shadows: false);
        }

        /// <summary>blockyKit.js drawPixelIcon: '#' pixels in the trap's colour on the CRT's dark ground, point-filtered (PerkMachines').</summary>
        static Texture2D IconTexture(List<string> rows, Color color)
        {
            int n = rows.Count;
            var tex = new Texture2D(n + 2, n + 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var ground = new Color(0.05f, 0.05f, 0.09f, 1);
            Color lit = color * 1.6f;
            lit.a = 1;
            for (int y = 0; y < n + 2; y++)
            {
                for (int x = 0; x < n + 2; x++)
                {
                    int row = n - y, col = x - 1; // texture rows go up; the icon's first string is the top row
                    bool on = row >= 0 && row < n && col >= 0 && col < rows[row].Length && rows[row][col] == '#';
                    tex.SetPixel(x, y, on ? lit : ground);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>A new Load (Traps.Version): each placed trap gets its view (built once per id) at its spot; the rest hide.</summary>
        void Relink()
        {
            version = traps.Version;
            foreach (View v in shown)
            {
                v.body.gameObject.SetActive(false);
                v.sw.gameObject.SetActive(false);
                v.trap = null;
            }
            shown.Clear();
            foreach (Traps.Trap t in traps.items)
            {
                if (!views.TryGetValue(t.id, out View v)) views[t.id] = v = Build(t);
                v.trap = t;
                v.time = 0;
                v.bladeAngle = 0;
                v.shownMode = -1; // redraw the screen
                v.body.gameObject.SetActive(true);
                v.sw.gameObject.SetActive(true);
                v.body.position = WebSpace.ToUnity(t.spot.pos);
                v.sw.position = WebSpace.ToUnity(t.Switch);
                // traps.js :181 model.group.rotation.y = atan2(x - sx, z - sz): the screen faces the trap. The web model's front
                // is +z; here it is local -z, so the Unity yaw is the web's negated (as PerkMachines).
                double yaw = System.Math.Atan2(t.spot.pos.x - t.spot.sx, t.spot.pos.z - t.spot.sz);
                v.sw.rotation = Quaternion.Euler(0, -(float)(yaw * Mathf.Rad2Deg), 0);
                shown.Add(v);
            }
        }

        // --- frame time ---------------------------------------------------------------------------------------------

        /// <summary>
        /// traps.js update(frameDt): the plate hums, the slab falls and rises, the blades spin, the pylon's orb pulses; the
        /// switch's CRT, trim and ring by state; the zaps. `frameDt` is the world's frame time (main.js traps.update(worldDt)).
        /// </summary>
        public void Render(double frameDt)
        {
            if (version != traps.Version) Relink();
            for (int i = 0; i < shown.Count; i++)
            {
                View v = shown[i];
                Traps.Trap it = v.trap;
                TrapSpec spec = it.spec;
                bool on = it.active > 0;
                v.time += frameDt;
                if (v.grid != null)
                {
                    Color g = v.color * (float)(on ? 2 + System.Math.Sin(v.time * 40) : 0.35) * GlowOpacity;
                    g.a = 1;
                    v.grid.SetColor(BaseColor, g);
                }
                if (v.slab != null)
                {
                    double down = traps.SlabDown(it);
                    v.slab.localPosition = new Vector3(0, (float)(spec.rest - (spec.rest - 0.55) * (down * down)), 0);
                }
                if (v.blades != null)
                {
                    v.bladeAngle += frameDt * (on ? 18 : 0.6);
                    v.blades.localRotation = Quaternion.Euler(0, -(float)(v.bladeAngle * Mathf.Rad2Deg), 0);
                }
                if (v.orb != null) v.orb.localScale = Vector3.one * (float)(0.9 * (on ? 1 + 0.25 * System.Math.Sin(v.time * 30) : 0.8));
                // The switch's CRT: the price when ready, ACTIVE while it runs, the cooldown's seconds after.
                bool locked = !traps.ZoneOpen(it);
                int mode = on ? 1 : it.cooldown > 0 ? 2 : 0;
                int secs = mode == 2 ? traps.CooldownSeconds(it) : 0;
                bool affordable = traps.Affordable(it);
                if (mode != v.shownMode || secs != v.shownSecs || locked != v.shownLocked || affordable != v.shownAffordable)
                {
                    v.shownMode = mode;
                    v.shownSecs = secs;
                    v.shownLocked = locked;
                    v.shownAffordable = affordable;
                    float k = locked ? 0.25f : on || it.cooldown > 0 ? 0.45f : 1; // setTrim
                    Color tc = v.color * glow * k;
                    tc.a = 1;
                    v.trim.SetColor(BaseColor, tc);
                    // `label` (ACTIVE / the cooldown) replaces the price line (machineModels.js draw).
                    string line = mode == 1 ? "ACTIVE" : mode == 2 ? secs + " S" : locked ? "LOCKED" : spec.price.ToString("R", CultureInfo.InvariantCulture); // String(state.price)
                    string lineHex = mode != 0 ? LabelHex : locked ? LockedHex : affordable ? AffordableHex : PoorHex;
                    v.label.text = $"<b><color={v.nameHex}>{spec.name}</color></b>\n<color={lineHex}>{line}</color>";
                    v.label.alpha = locked ? LockedDim : 1;
                    Color ic = Color.white * (locked ? LockedDim : 1);
                    ic.a = 1;
                    v.icon.SetColor(BaseColor, ic);
                }
                Color rc = v.color * (locked ? 0.3f : glow * (on ? 1.4f : 0.8f));
                rc.a = 1;
                v.ring.SetColor(BaseColor, rc);
            }
            RenderZaps((float)frameDt);
        }

        // --- the pylon's zaps (fx/arcs.js, simply) --------------------------------------------------------------------

        float H()
        {
            hash ^= hash << 13;
            hash ^= hash >> 17;
            hash ^= hash << 5;
            return (hash & 0xffffff) / (float)0x1000000;
        }

        void Kink(Zap z)
        {
            for (int i = 0; i < z.kinks.Length; i++) z.kinks[i] = H() * 2 - 1;
            z.regen = RegenMin + H() * (RegenMax - RegenMin);
        }

        /// <summary>arcs.spawn: a free zap first, else the next one round-robin (the payload is reused: copied here).</summary>
        void Spawn(TrapArc a)
        {
            Zap z = null;
            for (int i = 0; i < zaps.Length && z == null; i++)
            {
                Zap c = zaps[(nextZap + i) % zaps.Length];
                if (!c.alive) z = c;
            }
            if (z == null) z = zaps[nextZap];
            nextZap = (System.Array.IndexOf(zaps, z) + 1) % zaps.Length;
            z.alive = true;
            z.from = WebSpace.ToUnity(a.from);
            z.to = WebSpace.ToUnity(a.to);
            z.color = Look.Hex(a.hex);
            z.intensity = (float)a.intensity;
            z.life = z.maxLife = (float)a.life;
            z.jag = (float)a.jag;
            z.width = (float)a.width;
            Kink(z);
        }

        void RenderZaps(float frameDt)
        {
            foreach (Zap z in zaps)
            {
                if (!z.alive) continue;
                z.life -= frameDt;
                if (z.life <= 0)
                {
                    z.alive = false;
                    z.line.enabled = false;
                    continue;
                }
                z.regen -= frameDt;
                if (z.regen <= 0) Kink(z);
                Vector3 axis = z.to - z.from;
                float length = axis.magnitude;
                if (length < 1e-4f) continue;
                axis /= length;
                // Two unit vectors across the zap for the kinks; the ends stay put (sin envelope).
                Vector3 u = Vector3.Cross(Mathf.Abs(axis.y) < 0.9f ? Vector3.up : Vector3.right, axis).normalized;
                Vector3 w = Vector3.Cross(axis, u);
                float amp = z.jag * length;
                for (int i = 0; i <= Segments; i++)
                {
                    float t = i / (float)Segments;
                    float env = Mathf.Sin(Mathf.PI * t);
                    points[i] = z.from + axis * (length * t) + u * (z.kinks[i * 2] * amp * env) + w * (z.kinks[i * 2 + 1] * amp * env);
                }
                z.line.SetPositions(points);
                z.line.widthMultiplier = z.width * 2; // arcs.js's ribbon half-width
                float k = Mathf.Min(1, z.life / z.maxLife * 1.5f);
                Color c = z.color * (z.intensity * k);
                c.a = 1;
                z.mat.SetColor(BaseColor, c);
                z.line.enabled = true;
            }
        }
    }
}
