// NIGHT12 U3-25.4: the kill confirm, after skygrind/src/fx/killConfirm.js (NIGHT5 F4). A thin ring blooms where a kill
// lands, in the killing weapon's family colour (weapons.families[weapons.catalog.<w>.family], else colors.usable: the
// player's side, never the enemy red-orange), bigger for a headshot, a melee kill or a multikill (fx.killConfirm.headshot
// / melee / multikill multiply; a multikill is a kill inside style.multikillWindow, counted in fixed steps so the tier
// never depends on the frame rate), and gone in under 0.3 s (fx.killConfirm.life x the tier, capped at x1.4). One
// instanced draw for every ring (SKYGRIND/Particle, additive, no depth write, both faces), faced to the camera. The
// 'kill' handler only queues (inside the sim step: nothing drawn, no randomness); frame code spawns the ring and emits
// fx:killConfirm (an audio hook, docs/AUDIO-HOOKS.md; one reused payload). Brightness x glow x FLASH INTENSITY
// (settings.flashIntensity; none at 0), capped at maxAlpha. The web's material isn't tone mapped (toneMapped: false);
// URP's post tone maps every camera colour, the rings too.
// NIGHT17 round 1 (builder X): build 2's Player drew nothing through Graphics.RenderMeshInstanced (no ring, no death-burst
// cube, no block), so each ring is now its own Graphics.RenderMesh (at most fx.killConfirm.pool a frame) with its own
// property block, on an additive copy of the neon material (URP Unlit: Blend One One, no depth write, both faces, the
// transparent queue: WeaponFx.Additive's recipe, which the muzzle star proved in the Player).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class KillConfirm
    {
        struct Ring
        {
            public double age, life, size;
            public Vector3 at; // Unity space
            public Color color; // linear (three.js setHex)
        }

        struct Pending
        {
            public Vec3 at; // web space, lifted
            public Color color;
            public double tier;
            public int chain, code;
        }

        const double LifeCap = 1.4; // killConfirm.js: a ring's life is life x its tier, at most x1.4 (a literal in the web module)
        readonly SurvivalGame game;
        readonly Ring[] rings;
        readonly Pending[] pending;
        int pendingCount, next, chain;
        double lastKillStep = double.NegativeInfinity;
        readonly double windowSteps, size, start, life, lift, glow, maxAlpha, headshot, melee, multikill;
        readonly Color usable;
        readonly MaterialPropertyBlock[] blocks; // NIGHT17: one per ring (RenderMesh each)
        readonly Mesh mesh;
        RenderParams rp;
        readonly bool drawable = true; // NIGHT17: the neon copy always exists
        readonly KillConfirmEvent payload = new KillConfirmEvent();
        int settingsVersion = -1;
        double flash = 1;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor"), SrcBlend = Shader.PropertyToID("_SrcBlend"),
            DstBlend = Shader.PropertyToID("_DstBlend"), ZWrite = Shader.PropertyToID("_ZWrite"), Cull = Shader.PropertyToID("_Cull");

        public KillConfirm(SurvivalGame g)
        {
            game = g;
            GameConfig cfg = g.cfg;
            int pool = cfg.Int("fx.killConfirm.pool");
            size = cfg.Num("fx.killConfirm.size");
            start = cfg.Num("fx.killConfirm.start");
            life = cfg.Num("fx.killConfirm.life");
            lift = cfg.Num("fx.killConfirm.lift");
            glow = cfg.Num("fx.killConfirm.glow");
            maxAlpha = cfg.Num("fx.killConfirm.maxAlpha");
            headshot = cfg.Num("fx.killConfirm.headshot");
            melee = cfg.Num("fx.killConfirm.melee");
            multikill = cfg.Num("fx.killConfirm.multikill");
            windowSteps = cfg.Num("style.multikillWindow") * cfg.movement.loop.simHz;
            usable = Look.C(cfg, "colors.usable");
            rings = new Ring[pool];
            for (int i = 0; i < pool; i++) rings[i].age = double.PositiveInfinity;
            pending = new Pending[pool];
            blocks = new MaterialPropertyBlock[pool];
            for (int i = 0; i < pool; i++) blocks[i] = new MaterialPropertyBlock();
            mesh = RingMesh(1 - (float)cfg.Num("fx.killConfirm.width"), 1, cfg.Int("fx.killConfirm.segments"));
            var m = new Material(g.mats.neon) { name = "KillConfirm", renderQueue = (int)RenderQueue.Transparent + UnityTuning.KillConfirmQueueOffset };
            m.SetFloat(SrcBlend, (float)BlendMode.One); // AdditiveBlending
            m.SetFloat(DstBlend, (float)BlendMode.One);
            m.SetFloat(ZWrite, 0); // depthWrite: false
            m.SetFloat(Cull, (float)CullMode.Off);
            rp = new RenderParams(m) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            g.events.On<KillEvent>("kill", OnKill);
            g.OnNewRun += Clear;
        }

        /// <summary>three.js RingGeometry(inner, outer, segments): a flat ring in the xy plane (one band of quads).</summary>
        static Mesh RingMesh(float inner, float outer, int segments)
        {
            var v = new Vector3[2 * (segments + 1)];
            var t = new int[6 * segments];
            for (int i = 0; i <= segments; i++)
            {
                float a = 2 * Mathf.PI * i / segments;
                float c = Mathf.Cos(a), s = Mathf.Sin(a);
                v[2 * i] = new Vector3(inner * c, inner * s, 0);
                v[2 * i + 1] = new Vector3(outer * c, outer * s, 0);
            }
            for (int i = 0; i < segments; i++)
            {
                int a = 2 * i;
                t[6 * i] = a;
                t[6 * i + 1] = a + 1;
                t[6 * i + 2] = a + 3;
                t[6 * i + 3] = a;
                t[6 * i + 4] = a + 3;
                t[6 * i + 5] = a + 2;
            }
            var m = new Mesh { name = "KillConfirmRing", vertices = v, triangles = t };
            m.RecalculateBounds();
            return m;
        }

        /// <summary>killConfirm.js 'kill': a player's kill queues a ring (its tier from the fixed-step multikill window).</summary>
        void OnKill(KillEvent e)
        {
            if (e.owner != "player") return;
            double step = game.simStep;
            chain = step - lastKillStep <= windowSteps ? chain + 1 : 1;
            lastKillStep = step;
            double tier = (e.headshot ? headshot : 1) * (e.melee ? melee : 1) * (chain > 1 ? multikill : 1);
            if (pendingCount >= pending.Length) return;
            // NIGHT17 round 6 (the lead: gold arcs above a THUNDERCLAP kill; gold is points only): the ring is white for every gun
            // (the web drew it in the gun's family colour: the THUNDERCLAP's edge family is 0xffb000, gold)
            Color c = Color.white;
            pending[pendingCount++] = new Pending
            {
                at = new Vec3(e.position.x, e.position.y + lift, e.position.z), color = c, tier = tier, chain = chain,
                code = chain > 1 ? 2 : e.headshot || e.melee ? 1 : 0, // KillConfirmEvent.tier: 0 plain, 1 head / melee, 2 multikill
            };
        }

        /// <summary>
        /// killConfirm.js update: spawn the queued rings (each emits fx:killConfirm), grow them (an ease-out bloom from
        /// `start` x their size) and fade them ((1 - u)^2 x flash x glow, capped at maxAlpha), faced to the camera. The web
        /// passes frame time (feelDt: "it blooms through the kill's hitstop").
        /// </summary>
        bool warmed; // NIGHT17 round 2: the ring path's pipeline state made on the first frame with a camera

        public void Render(double frameDt)
        {
            if (!warmed && game.cam != null)
            {
                warmed = true;
                blocks[0].SetVector(BaseColor, Vector4.zero);
                rp.matProps = blocks[0];
                rp.worldBounds = CrunchWarm.Bounds(game.cam);
                Graphics.RenderMesh(rp, mesh, 0, CrunchWarm.Matrix(game.cam));
            }
            if (game.flow == Flow.Title)
            {
                Clear(); // resetWorld: killConfirm.clear()
                return;
            }
            for (int k = 0; k < pendingCount; k++)
            {
                Pending p = pending[k];
                ref Ring r = ref rings[next];
                next = (next + 1) % rings.Length;
                r.age = 0;
                r.life = life * Math.Min(LifeCap, p.tier);
                r.size = size * p.tier;
                r.at = WebSpace.ToUnity(p.at);
                r.color = p.color.linear;
                payload.owner = "player";
                payload.tier = p.code;
                payload.chain = p.chain;
                payload.position = p.at;
                game.events.Emit(MetaEvents.KillConfirm, payload);
            }
            pendingCount = 0;
            if (settingsVersion != game.settings.version)
            {
                settingsVersion = game.settings.version;
                flash = Math.Max(0, Math.Min(1, game.settings.Num("flashIntensity")));
            }
            Quaternion q = game.cam.transform.rotation;
            bool draw = drawable && flash > 0 && !Fx49.On(game.cfg); // NIGHT49 X fx49: the kill's ring is the kit's kill burst (fx:killConfirm still fires for audio)
            for (int i = 0; i < rings.Length; i++)
            {
                ref Ring r = ref rings[i];
                if (r.age >= r.life) continue;
                r.age += frameDt;
                double u = Math.Min(1, r.age / r.life);
                if (u >= 1) continue;
                double w = 1 - u;
                double grow = 1 - w * w * w; // fast out, then it slows (an ease-out bloom)
                double fade = w * w * flash * glow;
                float s = (float)(r.size * (start + (1 - start) * grow));
                if (!draw) continue;
                // NIGHT17 round 1: one RenderMesh per ring; the colour is linear already (SetVector: no conversion)
                blocks[i].SetVector(BaseColor, (Vector4)(r.color * (float)Math.Min(maxAlpha, fade)));
                rp.matProps = blocks[i];
                rp.worldBounds = new Bounds(r.at, Vector3.one * (2 * s + 0.1f));
                Graphics.RenderMesh(rp, mesh, 0, Matrix4x4.TRS(r.at, q, new Vector3(s, s, s)));
                FeelProbe.EffectWorld(FeelFx.Ring, r.at, s, (float)Math.Min(maxAlpha, fade)); // NIGHT18: the reel's probe (M2; no-op outside a reel)
            }
        }

        /// <summary>killConfirm.js clear: no rings, nothing queued, the chain reset.</summary>
        void Clear()
        {
            for (int i = 0; i < rings.Length; i++) rings[i].age = double.PositiveInfinity;
            pendingCount = 0;
            chain = 0;
        }
    }
}
