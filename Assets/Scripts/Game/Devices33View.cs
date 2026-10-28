// NIGHT33 builder R (design.n33.devices33; DESIGN33 card R): what the player SEES of the arena devices (Core/Devices33.cs). View only: no collider, no
// sim read changes. Drawn whenever the run has devices (sim.devices33 non-null: the sim row on, the map's rooms carrying devices33 rows):
//  - THE UPDRAFT FAN: a stone housing lip round its grate, dark slats, a CYAN rim (player-usable, rule 8) and cyan motes rising through the column to its
//    top (the tell of the lift: you see where the column ends).
//  - THE HAZARD VENT: the same housing and slats; in its TELL the grate glows RED-ORANGE, rising from dim to bright and pulsing at 2.5 Hz (danger only,
//    under 3 a second); in its BURST a red-orange to gold column stands over the grate to the burst's height (design.n33.devices33.vent.burstHeight).
//  - THE MOVING PLATFORM: a stone slab at the sim's position, interpolated by the game's alpha, with cyan strips along its top edges.
// Installs itself after the first scene load (no line in the lead's SurvivalGame files) and finds the game each frame it has none (a scene reload makes a
// new one). Unity API only in Awake / Attach (L-035: no Unity API in field initializers).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Devices33View : MonoBehaviour
    {
        /// <summary>For the reel: devices drawn now, by kind; a vent in its tell / burst is on screen now.</summary>
        public static int FansShown, VentsShown, PlatformsShown;
        public static bool TellShown, BurstShown;

        SurvivalGame game;
        Transform root;
        Devices33 builtFor;
        int builtCount = -1;
        Mesh boxMesh;
        Material stoneMat, grateMat, addMat;
        MaterialPropertyBlock block;
        int baseColorId;
        Color cyan, danger;
        double burstHeight;

        sealed class Parts
        {
            public Device33 d;
            public readonly List<Renderer> rim = new List<Renderer>();
            public Renderer glow, burst;
            public Transform burstT, body;
            public readonly List<Transform> motes = new List<Transform>();
            public readonly List<Renderer> moteR = new List<Renderer>();
        }

        readonly List<Parts> parts = new List<Parts>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Devices33View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Devices33View>();
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
            root = new GameObject("NIGHT33 DEVICES").transform;
            DontDestroyOnLoad(root.gameObject);
            if (boxMesh == null)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
                Destroy(cube);
            }
            if (stoneMat == null) stoneMat = FxDraw.Opaque(game.mats.neon, "Devices33 stone", Look.Hex(UnityTuning.Devices33StoneHex));
            if (grateMat == null) grateMat = FxDraw.Opaque(game.mats.neon, "Devices33 grate", Look.Hex(UnityTuning.Devices33GrateHex));
            if (addMat == null) addMat = FxDraw.Additive(game.mats.neon, "Devices33 glow");
            cyan = Look.C(game.cfg, "colors.usable");
            danger = Look.Hex(UnityTuning.Devices33DangerHex);
            burstHeight = game.cfg.Has("design.n33.devices33.vent.burstHeight") ? game.cfg.Num("design.n33.devices33.vent.burstHeight") : 2.5;
            builtFor = null;
            builtCount = -1;
            parts.Clear();
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

        /// <summary>The housing (lip + slats) of a fan or vent grate; returns the four rim strips' renderers (the fan's cyan rim) when `rim`.</summary>
        void Grate(Transform t, Device33 d, Parts p, bool rim)
        {
            float w = (float)(d.x1 - d.x0), l = (float)(d.z1 - d.z0), lh = UnityTuning.Devices33LipHeight, lw = UnityTuning.Devices33LipWidth;
            Vector3 c = U((d.x0 + d.x1) / 2, d.y, (d.z0 + d.z1) / 2);
            Vector3[] at = { c + new Vector3(0, lh / 2, l / 2), c + new Vector3(0, lh / 2, -l / 2), c + new Vector3(w / 2, lh / 2, 0), c + new Vector3(-w / 2, lh / 2, 0) };
            Vector3[] sz = { new Vector3(w + lw, lh, lw), new Vector3(w + lw, lh, lw), new Vector3(lw, lh, l + lw), new Vector3(lw, lh, l + lw) };
            for (int k = 0; k < 4; k++)
            {
                Box(t, "lip", stoneMat, at[k], sz[k], out _);
                if (rim)
                {
                    Box(t, "rim", addMat, at[k] + new Vector3(0, lh / 2 + 0.01f, 0), new Vector3(sz[k].x * 0.98f, 0.02f, sz[k].z * 0.98f), out Renderer rr);
                    p.rim.Add(rr);
                }
            }
            int n = UnityTuning.Devices33Slats;
            for (int k = 0; k < n; k++)
            {
                float u = (k + 0.5f) / n - 0.5f;
                Box(t, "slat", grateMat, c + new Vector3(u * w, 0.012f, 0), new Vector3(w / n * 0.45f, 0.02f, l * 0.96f), out _);
            }
        }

        void Build(Devices33 dev)
        {
            foreach (Transform ch in root) Destroy(ch.gameObject);
            parts.Clear();
            for (int i = 0; i < dev.devices.Count; i++)
            {
                Device33 d = dev.devices[i];
                var p = new Parts { d = d };
                var t = new GameObject($"{d.kind} {d.room} #{i}").transform;
                t.SetParent(root, false);
                switch (d.kind)
                {
                    case Device33Kind.Fan:
                        Grate(t, d, p, true);
                        for (int k = 0; k < UnityTuning.Devices33Motes; k++)
                        {
                            float s = UnityTuning.Devices33MoteSize;
                            p.motes.Add(Box(t, "mote", addMat, Vector3.zero, new Vector3(s, s * 2.5f, s), out Renderer mr));
                            p.moteR.Add(mr);
                        }
                        break;
                    case Device33Kind.Vent:
                    {
                        Grate(t, d, p, false);
                        float w = (float)(d.x1 - d.x0), l = (float)(d.z1 - d.z0);
                        Box(t, "tell glow", addMat, U((d.x0 + d.x1) / 2, d.y + 0.03, (d.z0 + d.z1) / 2), new Vector3(w, 0.02f, l), out p.glow);
                        for (int k = 0; k < UnityTuning.Devices33ShimmerStrips; k++) // the tell's rising shimmer (thin, translucent)
                        {
                            p.motes.Add(Box(t, "shimmer", addMat, Vector3.zero, new Vector3(UnityTuning.Devices33ShimmerWidth, UnityTuning.Devices33ShimmerHeight, UnityTuning.Devices33ShimmerWidth), out Renderer sr));
                            p.moteR.Add(sr);
                        }
                        float bs = UnityTuning.Devices33BurstShare;
                        p.burstT = Box(t, "burst", addMat, U((d.x0 + d.x1) / 2, d.y + burstHeight / 2, (d.z0 + d.z1) / 2), new Vector3(w * bs, (float)burstHeight, l * bs), out p.burst);
                        break;
                    }
                    case Device33Kind.Platform:
                    {
                        Vector3 sz = new Vector3((float)d.size.x, (float)d.size.y, (float)d.size.z);
                        p.body = Box(t, "slab", stoneMat, Vector3.zero, sz, out _);
                        float e = UnityTuning.Devices33EdgeThick, hy = sz.y / 2 + e / 2;
                        Vector3[] at = { new Vector3(0, hy, sz.z / 2 - e / 2), new Vector3(0, hy, -sz.z / 2 + e / 2), new Vector3(sz.x / 2 - e / 2, hy, 0), new Vector3(-sz.x / 2 + e / 2, hy, 0) };
                        Vector3[] es = { new Vector3(sz.x, e, e), new Vector3(sz.x, e, e), new Vector3(e, e, sz.z), new Vector3(e, e, sz.z) };
                        for (int k = 0; k < 4; k++)
                        {
                            Box(p.body, "edge", addMat, Vector3.zero, Vector3.one, out Renderer er);
                            // the edge strips are children of the slab: undo its scale so the sizes are metres
                            er.transform.localPosition = new Vector3(at[k].x / sz.x, at[k].y / sz.y, at[k].z / sz.z);
                            er.transform.localScale = new Vector3(es[k].x / sz.x, es[k].y / sz.y, es[k].z / sz.z);
                            p.rim.Add(er);
                        }
                        break;
                    }
                }
                parts.Add(p);
            }
            builtFor = dev;
            builtCount = dev.devices.Count;
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (game.sim == null) { game = null; return; }
            Devices33 dev = game.sim.devices33;
            bool on = dev != null && dev.Count > 0;
            if (!on)
            {
                if (root.gameObject.activeSelf) root.gameObject.SetActive(false);
                FansShown = VentsShown = PlatformsShown = 0;
                TellShown = BurstShown = false;
                return;
            }
            if (!root.gameObject.activeSelf) root.gameObject.SetActive(true);
            // round 3 (the lead's Player reel n33-r5: "vent #1 in f8: tell drawn False, burst drawn False"): a new run's StartRun reuses the Devices33 object but
            // Load() makes NEW Device33 rows, so the view kept the first run's rows: their vent phase never left Idle (no tell, no burst) and a platform stood
            // still. Rebuild whenever the first row is not the one the parts hold.
            if (dev != builtFor || dev.Count != builtCount || parts.Count == 0 || parts[0].d != dev.devices[0]) Build(dev);
            float alpha = Mathf.Clamp01((float)game.Alpha), now = Time.time;
            int fans = 0, vents = 0, plats = 0;
            bool tell = false, burst = false;
            foreach (Parts p in parts)
            {
                Device33 d = p.d;
                switch (d.kind)
                {
                    case Device33Kind.Fan:
                    {
                        fans++;
                        foreach (Renderer r in p.rim) Tint(r, cyan, UnityTuning.Devices33FanRimGain);
                        double h = d.height > 0 ? d.height : 4, w = d.x1 - d.x0, l = d.z1 - d.z0;
                        for (int k = 0; k < p.motes.Count; k++)
                        {
                            // a fixed scatter over the grate (the golden ratio), each mote rising and wrapping at the column's top
                            double fx = (k * 0.618034) % 1, fz = (k * 0.381966 + 0.37) % 1;
                            double rise = (now * UnityTuning.Devices33MoteSpeed + k * h / p.motes.Count) % h;
                            p.motes[k].localPosition = U(d.x0 + w * (0.1 + 0.8 * fx), d.y + rise, d.z0 + l * (0.1 + 0.8 * fz));
                            float fade = 1f - (float)(rise / h);
                            Tint(p.moteR[k], cyan, UnityTuning.Devices33MoteGain * fade);
                        }
                        break;
                    }
                    case Device33Kind.Vent:
                    {
                        vents++;
                        // round 3 (the lead's Player frame n33-r6: a gold burst at gain 1.3 read as an opaque yellow wall): every vent colour is the danger
                        // red-orange; the shimmer and the column are translucent (gain <= 0.35); the grate glow carries the tell
                        bool warn = d.vent == Vent33Phase.Warn, hot = d.vent == Vent33Phase.Burst;
                        if (warn)
                        {
                            tell = true;
                            float pulse = 0.75f + 0.25f * Mathf.Sin(now * Mathf.PI * 2 * UnityTuning.Devices33TellHz);
                            Tint(p.glow, danger, Mathf.Lerp(UnityTuning.Devices33TellGainLo, UnityTuning.Devices33TellGainHi, (float)d.ventShare) * pulse);
                        }
                        else if (hot)
                        {
                            burst = true;
                            Tint(p.glow, danger, UnityTuning.Devices33TellGainHi);
                            Tint(p.burst, danger, UnityTuning.Devices33BurstGain * (1f - 0.6f * (float)d.ventShare));
                        }
                        p.glow.enabled = warn || hot;
                        p.burst.enabled = hot;
                        double bw = d.x1 - d.x0, bl = d.z1 - d.z0;
                        for (int k = 0; k < p.motes.Count; k++)
                        {
                            p.motes[k].gameObject.SetActive(warn);
                            if (!warn) continue;
                            double fx = (k * 0.618034) % 1, fz = (k * 0.381966 + 0.37) % 1, top = burstHeight;
                            double rise = (now * UnityTuning.Devices33ShimmerSpeed + k * top / p.motes.Count) % top;
                            p.motes[k].localPosition = U(d.x0 + bw * (0.15 + 0.7 * fx), d.y + rise + UnityTuning.Devices33ShimmerHeight / 2, d.z0 + bl * (0.15 + 0.7 * fz));
                            Tint(p.moteR[k], danger, UnityTuning.Devices33ShimmerGain * (float)d.ventShare * (1f - (float)(rise / top)));
                        }
                        break;
                    }
                    case Device33Kind.Platform:
                    {
                        plats++;
                        Vec3 a = d.prev, b = d.pos;
                        double cx = a.x + (b.x - a.x) * alpha, cy = a.y + (b.y - a.y) * alpha, cz = a.z + (b.z - a.z) * alpha;
                        p.body.localPosition = U(cx, cy - d.size.y / 2, cz);
                        foreach (Renderer r in p.rim) Tint(r, cyan, UnityTuning.Devices33EdgeGain);
                        break;
                    }
                }
            }
            FansShown = fans;
            VentsShown = vents;
            PlatformsShown = plats;
            TellShown = tell;
            BurstShown = burst;
        }

        void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
            if (stoneMat != null) Destroy(stoneMat);
            if (grateMat != null) Destroy(grateMat);
            if (addMat != null) Destroy(addMat);
        }
    }
}
