// NIGHT33 L (design.n33.look33; DESIGN33 L round 3; L1 section 3 "Cheap stylized VFX"): the campaign level's ambient effects. View only: no collider, no sim read
// beyond the map JSON, no gameplay randomness (Unity's particle seeds are render-only; rule 6 covers sim code). Built once per level by CampaignLighting24 when
// look33 is on:
//  - EMBERS over every lava pit: a box emitter on the lava surface, Look33EmberMax quads rising 0.5-1.2 m/s with a sway, 3-5 s, orange HDR fading to dark red, up to
//    ~6 m only (danger colour: lava only);
//  - DUST MOTES in each room's key light pool: one emitter under the room's ceiling panel, 40-80 tiny pale quads drifting 0.05-0.15 m/s, very low brightness;
//  - SPARKS at every brazier (the hazards' lamps): a burst of 8-12 stretched sparks every 1.5-3 s, 0.3 s, gravity;
//  - the DANGER FLICKER: the brazier lights (LightKit29's slots following a brazier) and the orange fixture materials at +-15 % (perlin, Look33FlickerHz); steady with
//    REDUCED MOTION. Cyan fixtures stay steady (L1).
// Not built (SHORT, said in the report): the kiln heat haze (needs the camera's Opaque Texture, which PerfSwitches turns off) and the scrolling emissive panels (a shader).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Vfx33 : MonoBehaviour
    {
        SurvivalGame game;
        LightKit29 kit;
        readonly List<Material> danger = new List<Material>();
        readonly List<Color> dangerBase = new List<Color>();
        /// <summary>For the reel line: emitters built.</summary>
        public int embers, motes, sparks;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        static Vector3 U(double x, double y, double z) => new Vector3((float)x, (float)y, (float)-z); // web -> Unity (z flips)

        public static Vfx33 Create(SurvivalGame g, Transform parent, LightPlan33.Plan plan, LightKit29 kit, List<Material> dangerMats)
        {
            if (g == null || plan == null || g.map == null) return null;
            var go = new GameObject("Vfx 33");
            go.transform.SetParent(parent, false);
            var v = go.AddComponent<Vfx33>();
            v.Build(g, plan, kit, dangerMats);
            return v;
        }

        static Material Additive(Color c)
        {
            Shader sh = Shader.Find("SKYGRIND/Particle");
            if (sh == null) return null;
            var m = new Material(sh);
            m.SetFloat("_SrcBlend", 1); m.SetFloat("_DstBlend", 1); m.SetFloat("_ZWrite", 0); m.SetFloat("_Cull", 0);
            m.SetColor(BaseColorId, c);
            m.renderQueue = 3000;
            return m;
        }

        void Build(SurvivalGame g, LightPlan33.Plan plan, LightKit29 k, List<Material> dangerMats)
        {
            game = g;
            kit = k;
            if (dangerMats != null)
                foreach (Material m in dangerMats) { danger.Add(m); dangerBase.Add(m.GetColor(BaseColorId)); }
            LightPlan33.GreenFor(g.map.json, g.cfg, N30Fixes.ViewRowOn(g.cfg, "look30"), out LightPlan33.Level lv);
            if (lv == null) return;
            Color enemy = Look.C(g.cfg, "colors.enemy");
            Material ember = Additive(enemy * UnityTuning.Look33EmberHdr);
            Material mote = Additive(Look.Hex((uint)g.cfg.Num("design.n27.roof27.boundaryHex")) * UnityTuning.Look33MoteGain);
            Material spark = Additive(Color.Lerp(enemy, Look.Hex(UnityTuning.Look33SparkGoldHex), 0.5f) * UnityTuning.Look33SparkHdr);
            if (ember == null || mote == null || spark == null) return; // the particle shader is missing from the build: no effects (said by the reel's counts)
            // embers: every lava pit
            foreach (LightPlan33.Pit p in lv.pits)
            {
                if (!p.lava) continue;
                double w = p.x1 - p.x0, d = p.z1 - p.z0;
                int max = Mathf.Clamp(Mathf.RoundToInt((float)(w * d * UnityTuning.Look33EmbersPerM2)), UnityTuning.Look33EmberMin, UnityTuning.Look33EmberMax);
                ParticleSystem ps = Emitter("Embers 33", ember, U((p.x0 + p.x1) * 0.5, p.floorY + 0.05, (p.z0 + p.z1) * 0.5), new Vector3((float)w, 0.05f, (float)d), max,
                    UnityTuning.Look33EmberLife, UnityTuning.Look33EmberSize);
                var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
                vel.x = new ParticleSystem.MinMaxCurve(0f, 0f); vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                vel.y = new ParticleSystem.MinMaxCurve(UnityTuning.Look33EmberRise.x, UnityTuning.Look33EmberRise.y);
                var noise = ps.noise; noise.enabled = true; noise.strength = UnityTuning.Look33EmberSway; noise.frequency = 0.6f; noise.scrollSpeed = 0.3f;
                var col = ps.colorOverLifetime; col.enabled = true;
                var gr = new Gradient();
                gr.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.45f, 0.08f, 0.05f), 1f) }, new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
                col.color = gr;
                ps.Play();
                embers++;
            }
            // dust motes: each room's key pool (under its ceiling panel)
            foreach (LightPlan33.Lit l in plan.lights)
            {
                if (l.kind != "room") continue;
                LightPlan33.Room r = lv.rooms.Find(q => q.id == l.room);
                if (r == null) continue;
                float sx = Mathf.Min((float)(r.x1 - r.x0), UnityTuning.Look33MotePool), sz = Mathf.Min((float)(r.z1 - r.z0), UnityTuning.Look33MotePool), sy = Mathf.Max(1f, (float)l.y - 0.5f);
                if (N35Fixes.ViewRowOn(g.cfg, "lights35")) sy = Mathf.Min(sy, UnityTuning.Look35MoteTop); // NIGHT35 lights35: dust near the floor, never dots against the ceiling
                int max = Mathf.Clamp(Mathf.RoundToInt(sx * sz * UnityTuning.Look33MotesPerM2), UnityTuning.Look33MoteMin, UnityTuning.Look33MoteMax);
                ParticleSystem ps = Emitter("Dust 33 " + r.id, mote, U(l.x, sy * 0.5f + 0.3f, l.z), new Vector3(sx, sy, sz), max, UnityTuning.Look33MoteLife, UnityTuning.Look33MoteSize);
                var main = ps.main; main.prewarm = true;
                var noise = ps.noise; noise.enabled = true; noise.strength = UnityTuning.Look33MoteDrift; noise.frequency = 0.2f; noise.scrollSpeed = 0.05f;
                var col = ps.colorOverLifetime; col.enabled = true;
                var gr = new Gradient();
                gr.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }, new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
                col.color = gr;
                ps.Play();
                motes++;
            }
            // sparks: every brazier's ember core
            foreach (LightPlan33.Fix f in plan.fixes)
            {
                if (f.kind != "lens" || f.type != "brazier") continue;
                Vector3 at = U((f.box.x0 + f.box.x1) * 0.5, f.box.y1, (f.box.z0 + f.box.z1) * 0.5);
                ParticleSystem ps = Emitter("Sparks 33 " + f.id, spark, at, new Vector3(0.2f, 0.02f, 0.2f), UnityTuning.Look33SparkBurst.y, UnityTuning.Look33SparkLife, UnityTuning.Look33SparkSize);
                var em = ps.emission; em.rateOverTime = 0f;
                em.SetBursts(new[] { new ParticleSystem.Burst(0.5f, (short)UnityTuning.Look33SparkBurst.x, (short)UnityTuning.Look33SparkBurst.y, 0, UnityTuning.Look33SparkEvery) });
                var main = ps.main; main.startSpeed = new ParticleSystem.MinMaxCurve(UnityTuning.Look33SparkSpeed.x, UnityTuning.Look33SparkSpeed.y); main.gravityModifier = 1f;
                var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 35f; sh.rotation = new Vector3(-90f, 0f, 0f);
                var rend = ps.GetComponent<ParticleSystemRenderer>(); rend.renderMode = ParticleSystemRenderMode.Stretch; rend.velocityScale = 0.06f; rend.lengthScale = 1f;
                ps.Play();
                sparks++;
            }
        }

        ParticleSystem Emitter(string name, Material m, Vector3 centre, Vector3 size, int max, Vector2 life, Vector2 sz)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = centre;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = max; main.startLifetime = new ParticleSystem.MinMaxCurve(life.x, life.y);
            main.startSize = new ParticleSystem.MinMaxCurve(sz.x, sz.y); main.startSpeed = 0f; main.startColor = Color.white;
            var em = ps.emission; em.enabled = true; em.rateOverTime = max / Mathf.Max(0.1f, (life.x + life.y) * 0.5f);
            var sh = ps.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = size;
            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = m; rend.renderMode = ParticleSystemRenderMode.Billboard;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; rend.receiveShadows = false;
            return ps;
        }

        void LateUpdate()
        {
            if (game == null) return;
            bool reduced = game.settings != null && game.settings.View.reducedMotion;
            bool l54 = Light54.On(game.cfg); // NIGHT54 light54: the brazier's noise slower and shallower (an 11 Hz +-15 % perlin read as a flicker)
            float hz = l54 ? Light54.F(game.cfg, "flickerHz", 2.2) : UnityTuning.Look33FlickerHz;
            float depth = l54 ? Light54.F(game.cfg, "flickerDepth", 0.06) : UnityTuning.Look33FlickerDepth;
            float t = Time.time * hz;
            float k = reduced ? 1f : 1f + depth * (Mathf.PerlinNoise(t, 0.37f) * 2f - 1f); // +-15 %, danger fixtures only
            for (int i = 0; i < danger.Count; i++) if (danger[i] != null) danger[i].SetColor(BaseColorId, dangerBase[i] * k);
            if (kit == null || kit.Lights == null) return;
            Light[] ls = kit.Lights;
            for (int i = 0; i < ls.Length; i++)
                if (ls[i] != null && kit.LightIsDanger(i)) ls[i].intensity = kit.BaseIntensity(i) * (reduced ? 1f : 1f + depth * (Mathf.PerlinNoise(t, 1.7f + i) * 2f - 1f));
        }
    }
}
