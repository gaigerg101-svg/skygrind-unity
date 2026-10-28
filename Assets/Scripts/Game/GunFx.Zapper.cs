// NIGHT19 U3-33.6 / U3-33.8 (builder V): the ZAPPER's arc, a port of the web's chained lightning (src/fx/arcs.js, drawn
// by src/combat/gunFx.js :86-94 on `zap` and :57-63 on `discharge`, with config.weaponFx.arc*): every `zap` draws an arc
// from the muzzle to the first point, then point to point along the chain (`a.copy(b)`); a `discharge` draws one from the
// muzzle to each stunned enemy (x1.5 intensity, x2 life, x1.6 jag, x1.4 width). An arc is a jagged camera-facing strip
// with a white core (arcs.js `core` 0.35 of the width), its kinks re-rolled every 60-90 ms so it crackles, its ends fixed
// (the kink envelope sin(pi t)), fading as min(1, life / maxLife x 1.5). Port-only: two pooled LineRenderers an arc (the
// glow and the core) on additive copies of the neon material with the Kenney lightning strip (spark_07, CC0) along them;
// the colour is the player's cyan (CLAUDE.md rule 8: the web's volt lime has no role here; one token, UnityTuning), and
// where an arc lands a few textured star sparks (the web bursts 4 volt cubes there). Visual only, frame time (0 in a
// hitstop: the arc holds), its dice its own xorshift (a render-side stream), no allocation per frame.
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>arcs.js's maths, pure (HandsN19Tests).</summary>
    public static class ZapArcMath
    {
        /// <summary>
        /// arcs.js update(): `segments` + 1 points from `from` to `to`, each kinked across the arc by its two `kinks`
        /// (-1..1) x jag x length x sin(pi t) (the ends stay put), into `into`.
        /// </summary>
        public static void Build(Vector3 from, Vector3 to, float[] kinks, int segments, float jag, Vector3[] into)
        {
            Vector3 axis = to - from;
            float length = axis.magnitude;
            if (length < 1e-4f)
            {
                for (int i = 0; i <= segments; i++) into[i] = from;
                return;
            }
            axis /= length;
            // u.set(|axis.y| < 0.9 ? 0 : 1, |axis.y| < 0.9 ? 1 : 0, 0).cross(axis).normalize(); v = axis x u
            Vector3 u = Vector3.Cross(Mathf.Abs(axis.y) < 0.9f ? new Vector3(0, 1, 0) : new Vector3(1, 0, 0), axis).normalized;
            Vector3 v = Vector3.Cross(axis, u);
            float amp = jag * length;
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float env = Mathf.Sin(Mathf.PI * t);
                if (i == 0 || i == segments) env = 0; // exactly the ends (sin(pi) is not 0 in floats)
                into[i] = from + axis * (length * t) + u * (kinks[i * 2] * amp * env) + v * (kinks[i * 2 + 1] * amp * env);
            }
        }

        /// <summary>arcs.js: k = min(1, life / maxLife x 1.5).</summary>
        public static float Fade(float life, float maxLife) => maxLife > 0 ? Mathf.Min(1, Mathf.Max(0, life / maxLife) * 1.5f) : 0;

        /// <summary>arcs.js kink(): every kink in -1..1, from a render-side xorshift (`state`, never a sim stream).</summary>
        public static void Kink(float[] kinks, int segments, ref uint state)
        {
            for (int i = 0; i <= segments * 2 + 1 && i < kinks.Length; i++) kinks[i] = Next(ref state) * 2 - 1;
        }

        /// <summary>A render-side xorshift32 in 0..1.</summary>
        public static float Next(ref uint state)
        {
            if (state == 0) state = 2463534242;
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (state & 0xffffff) / (float)0x1000000;
        }
    }

    public sealed partial class SurvivalGame
    {
        sealed class ZapArc
        {
            public bool alive;
            public Vector3 from, to;
            public float life, maxLife, intensity, jag, width, regen;
            public float[] kinks;
            public Vector3[] points;
            public LineRenderer glow, core;
            public Material glowMat, coreMat;
        }

        ZapArc[] zapArcs;
        int zapNext;
        int zapSegments;
        float zapWidth, zapLife, zapJag, zapIntensity;
        Color zapUsable; // colors.usable (before UiTheme is made)
        uint zapState = 0x7a617073; // "zaps": the render-side stream
        FxSprites zapSparks;
        struct ZapSpark
        {
            public Vector3 at;
            public float age, life, size, roll;
        }

        readonly ZapSpark[] zapSparkPool = new ZapSpark[UnityTuning.ZapArcPool * UnityTuning.ZapSparkCount];
        int zapSparkNext;
        static readonly int ZapBaseColor = Shader.PropertyToID("_BaseColor");

        partial void N19ZapperFxInit()
        {
            zapSegments = cfg.Int("weaponFx.arcSegments");
            zapWidth = (float)cfg.Num("weaponFx.arcWidth");
            zapLife = (float)cfg.Num("weaponFx.arcLife");
            zapJag = (float)cfg.Num("weaponFx.arcJag");
            zapIntensity = (float)cfg.Num("weaponFx.arcIntensity");
            zapUsable = Look.C(cfg, "colors.usable"); // read once (a cfg lookup allocates: PORT-RULES 6)
            Texture2D bolt = FxSprites.Load(UnityTuning.KenneyBolt);
            var root = new GameObject("Zapper arcs").transform;
            zapArcs = new ZapArc[UnityTuning.ZapArcPool];
            for (int i = 0; i < zapArcs.Length; i++)
            {
                var arc = new ZapArc
                {
                    kinks = new float[(zapSegments + 1) * 2],
                    points = new Vector3[zapSegments + 1],
                };
                arc.glowMat = ZapMaterial(bolt, "Zap arc glow");
                arc.coreMat = ZapMaterial(bolt, "Zap arc core");
                arc.glow = ZapLine(root, $"zap glow {i}", arc.glowMat);
                arc.core = ZapLine(root, $"zap core {i}", arc.coreMat);
                zapArcs[i] = arc;
            }
            zapSparks = new FxSprites(mats.neon, UnityTuning.KenneyStar, zapSparkPool.Length);
            events.On<ZapEvent>(N19Events.Zap, e => ZapChain(e, false));
            events.On<ZapEvent>(N19Events.Discharge, e => ZapChain(e, true));
            OnNewRun += ZapClear;
        }

        Material ZapMaterial(Texture2D tex, string name)
        {
            Material m = FxDraw.Additive(mats.neon, name);
            if (tex != null) m.SetTexture("_BaseMap", tex);
            m.SetFloat("_Cull", 0);
            return m;
        }

        LineRenderer ZapLine(Transform root, string name, Material m)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.sharedMaterial = m;
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;
            lr.numCapVertices = 0;
            lr.numCornerVertices = 0;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.positionCount = zapSegments + 1;
            lr.enabled = false;
            return lr;
        }

        void ZapClear()
        {
            if (zapArcs == null) return;
            foreach (ZapArc a in zapArcs)
            {
                a.alive = false;
                a.glow.enabled = a.core.enabled = false;
            }
            for (int i = 0; i < zapSparkPool.Length; i++) zapSparkPool[i].life = 0;
        }

        /// <summary>The muzzle in the world's view (the viewmodel's muzzle; the eye a little ahead without a rig).</summary>
        Vector3 ZapMuzzle()
        {
            if (viewmodel != null && viewmodel.MuzzleWorld(out Vector3 m)) return m;
            return cam != null ? cam.transform.position + cam.transform.forward * 0.5f : Vector3.zero;
        }

        /// <summary>gunFx.js `zap` (:86-94) and `discharge` (:57-63).</summary>
        void ZapChain(ZapEvent e, bool discharge)
        {
            if (e == null || e.owner != "player" || zapArcs == null) return;
            Vector3 a = ZapMuzzle();
            for (int i = 0; i < e.count && i < e.points.Length; i++)
            {
                Vector3 b = WebSpace.ToUnity(e.points[i]);
                if (discharge)
                {
                    ZapSpawn(a, b, zapIntensity * 1.5f, zapLife * 2, zapJag * 1.6f, zapWidth * 1.4f);
                    ZapBurst(b, 1.3f);
                }
                else
                {
                    ZapSpawn(a, b, zapIntensity, zapLife, zapJag, zapWidth);
                    ZapBurst(b, 1);
                    a = b; // a.copy(b): the chain goes point to point
                }
            }
        }

        /// <summary>arcs.js spawn(): a free arc first, else the next one round-robin; its kinks rolled now.</summary>
        void ZapSpawn(Vector3 from, Vector3 to, float intensity, float life, float jag, float width)
        {
            ZapArc arc = null;
            for (int i = 0; i < zapArcs.Length && arc == null; i++)
            {
                ZapArc c = zapArcs[(zapNext + i) % zapArcs.Length];
                if (!c.alive) arc = c;
            }
            if (arc == null) arc = zapArcs[zapNext];
            zapNext = (Array.IndexOf(zapArcs, arc) + 1) % zapArcs.Length;
            arc.alive = true;
            arc.from = from;
            arc.to = to;
            arc.intensity = intensity;
            arc.life = arc.maxLife = life;
            arc.jag = jag;
            arc.width = width;
            ZapKink(arc);
        }

        void ZapKink(ZapArc arc)
        {
            ZapArcMath.Kink(arc.kinks, zapSegments, ref zapState);
            arc.regen = 0.06f + ZapArcMath.Next(ref zapState) * 0.03f; // arcs.js regenMin 0.06 .. regenMax 0.09
        }

        void ZapBurst(Vector3 at, float scale)
        {
            for (int k = 0; k < UnityTuning.ZapSparkCount; k++)
            {
                ref ZapSpark s = ref zapSparkPool[zapSparkNext];
                zapSparkNext = (zapSparkNext + 1) % zapSparkPool.Length;
                Vector3 jitter = new Vector3(ZapArcMath.Next(ref zapState) - 0.5f, ZapArcMath.Next(ref zapState) - 0.5f, ZapArcMath.Next(ref zapState) - 0.5f) * 0.3f;
                s.at = at + jitter;
                s.age = 0;
                s.life = 0.15f * (0.7f + 0.6f * ZapArcMath.Next(ref zapState)); // the web's burst life 0.15
                s.size = UnityTuning.ZapSparkSize * scale * (0.6f + 0.8f * ZapArcMath.Next(ref zapState));
                s.roll = ZapArcMath.Next(ref zapState) * Mathf.PI;
            }
        }

        /// <summary>arcs.js update(frameDt): age, re-kink, rebuild; the landing sparks.</summary>
        partial void N19ZapperFxRender(double alpha, double frameDt)
        {
            if (zapArcs == null) return;
            float dt = (float)frameDt;
            Color cyan = UiTheme.Ready ? UiTheme.Cyan : zapUsable;
            Color coreTint = Color.Lerp(Color.white, cyan, 0.25f); // arcs.js: WHITE lerp(color, 0.25)
            foreach (ZapArc arc in zapArcs)
            {
                if (!arc.alive) continue;
                arc.life -= dt;
                if (arc.life <= 0)
                {
                    arc.alive = false;
                    arc.glow.enabled = arc.core.enabled = false;
                    continue;
                }
                arc.regen -= dt;
                if (arc.regen <= 0) ZapKink(arc);
                ZapArcMath.Build(arc.from, arc.to, arc.kinks, zapSegments, arc.jag, arc.points);
                float k = ZapArcMath.Fade(arc.life, arc.maxLife);
                arc.glowMat.SetColor(ZapBaseColor, cyan * (arc.intensity * k * UnityTuning.ZapArcHdr));
                arc.coreMat.SetColor(ZapBaseColor, coreTint * (arc.intensity * 1.2f * k * UnityTuning.ZapArcHdr));
                arc.glow.widthMultiplier = arc.width * 2; // arcs.js ribbon(w): w either side
                arc.core.widthMultiplier = arc.width * 2 * UnityTuning.ZapCoreShare;
                arc.glow.SetPositions(arc.points);
                arc.core.SetPositions(arc.points);
                arc.glow.enabled = arc.core.enabled = true;
            }
            if (zapSparks == null || !zapSparks.Ready || cam == null) return;
            zapSparks.Begin();
            bool any = false;
            Color spark = Color.Lerp(cyan, Color.white, 0.5f) * 2f;
            for (int i = 0; i < zapSparkPool.Length; i++)
            {
                ref ZapSpark s = ref zapSparkPool[i];
                if (!(s.life > 0)) continue;
                s.age += dt;
                if (s.age >= s.life)
                {
                    s.life = 0;
                    continue;
                }
                float u = s.age / s.life;
                zapSparks.Billboard(s.at, s.size * (1 - u * 0.5f), s.roll, cam.transform, spark, 1 - u);
                any = true;
            }
            if (any) zapSparks.Draw();
        }
    }
}
