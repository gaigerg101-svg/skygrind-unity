// NIGHT9 U3-09: the impact wave's look, simple for tonight (fx/impactFx.js's full set of kinds and its additive pooled
// meshes are N12, with the melee weapons that make them). The Gauntlet / punch ring: a flat ring (impactFx.js
// RingGeometry 0.88..1) at the target's ground (feet.y + 0.08), expanding to the wave's radius as
// 0.2 + 0.8 * (1 - (1 - t)^2) over momentum.shock.ring.time (held at its size with reduced motion), in the kinetic
// family's colour x fx.impact.glow fading with (1 - t); a particle burst (18 x k cubes, speed 8, life 0.35, scale 1.1;
// NIGHT16 polish #51: k is the quality preset's impact detail, impactFx.js :48: 18 cubes, 9 on LOW). At most
// fx.impact.rings live; the oldest gives way. The ring uses the alpha-blended Acid material (the web blends additively:
// left as is, noted in polish #51).
// NIGHT19 U3-33.8 (builder V; NIGHT19 banned pattern 13): with tonight's look (FeelAB on) the ring is the Kenney ring sprite
// (circle_02, CC0, FxSprites.cs) laid flat at the same place and radius, additive (as the web blends it), its visible band
// at the wave's radius; FeelAB off (or the sprite not in the build) keeps the flat mesh ring.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class ImpactFx
    {
        sealed class Ring
        {
            public Transform t;
            public Renderer r;
            public double age, time, radius;
            public bool live;
            public long order;
            public Color c; // NIGHT49 X fx49: the ring's colour (kinetic; with fx49 the kit's owner hue)
        }

        readonly Color k49Player, k49Gauntlet; // NIGHT49 X fx49: the kit's hues at the heavy glow (Fx49.cs)

        readonly SurvivalGame game;
        readonly Ring[] rings;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color kinetic;
        readonly float glow;
        readonly double ringTime;
        readonly double detailLow, detailMedium, detailHigh; // polish #51: quality.presets.<level>.impact (impactFx.js :48 detail())
        long serial;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        readonly FxSprites ringSprites; // NIGHT19: the textured ring (FeelAB on)

        public ImpactFx(SurvivalGame game, Particles particles)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            kinetic = Look.C(cfg, "weapons.families.kinetic");
            glow = (float)cfg.Num("fx.impact.glow");
            ringTime = cfg.Num("momentum.shock.ring.time");
            detailLow = cfg.Num("quality.presets.low.impact");
            detailMedium = cfg.Num("quality.presets.medium.impact");
            detailHigh = cfg.Num("quality.presets.high.impact");
            Mesh mesh = RingMesh(0.88f, 1f, 48);
            var parent = new GameObject("Impact rings").transform;
            rings = new Ring[cfg.Int("fx.impact.rings")];
            ringSprites = new FxSprites(game.mats.neon, UnityTuning.KenneyRing, rings.Length);
            ringSprites.Warm(kinetic * glow * UnityTuning.ImpactRingHdr);
            k49Player = Fx49.Hue(Fx49Owner.Player) * Fx49.Cell(Fx49Owner.Player, Fx49Tier.Heavy).glow;
            k49Gauntlet = Fx49.Hue(Fx49Owner.Gauntlet) * Fx49.Cell(Fx49Owner.Gauntlet, Fx49Tier.Heavy).glow;
            ringSprites.Warm(k49Player);
            ringSprites.Warm(k49Gauntlet);
            for (int i = 0; i < rings.Length; i++)
            {
                var go = new GameObject($"Impact ring {i}", typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(parent, false);
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                var r = go.GetComponent<MeshRenderer>();
                r.sharedMaterial = game.acidMaterial;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                go.SetActive(false);
                rings[i] = new Ring { t = go.transform, r = r };
            }
            game.events.On<ImpactWaveEvent>("impactWave", e =>
            {
                if (e.owner != "player" || e.kind != "ring") return;
                // impactFx.js :48 / :59: k = the quality preset's impact detail (polish #51; the port scaled it by the momentum's
                // share over momentum.shock.at, so a wave just over the threshold burst about nothing)
                double k = FxGates.PresetValue(game.lightAndLens != null ? game.lightAndLens.Level : null, detailLow, detailMedium, detailHigh);
                bool k49 = Fx49.On(game.cfg); // NIGHT49 X fx49: the shock ring in its owner's kit hue (the gauntlet's pink, the fist's cyan)
                Color rc = k49 ? (Fx49.OwnerOf(e.weapon) == Fx49Owner.Gauntlet ? k49Gauntlet : k49Player) : kinetic * glow * UnityTuning.ImpactRingHdr;
                Spawn(new Vec3(e.position.x, e.feet.y + 0.08, e.position.z), e.radius, ringTime, rc);
                particles.Burst(e.position, k49 ? Fx49.Hue(Fx49.OwnerOf(e.weapon)) : kinetic, FxGates.ImpactBurstCount(k), UnityTuning.ImpactRingBurstSpeed, UnityTuning.ImpactRingBurstLife, UnityTuning.ImpactRingBurstScale); // impactFx.js :63 (18 x k cubes, scale 1.1)
            });
        }

        void Spawn(Vec3 at, double radius, double time, Color c)
        {
            Ring slot = null;
            foreach (Ring r in rings) if (!r.live) { slot = r; break; }
            if (slot == null)
            {
                slot = rings[0];
                foreach (Ring r in rings) if (r.order < slot.order) slot = r;
            }
            slot.live = true;
            slot.age = 0;
            slot.time = time;
            slot.radius = radius;
            slot.order = ++serial;
            slot.c = c;
            slot.t.position = WebSpace.ToUnity(at);
            slot.t.gameObject.SetActive(true);
        }

        /// <summary>Frame time (visual only); frozen with the sim during a hitstop.</summary>
        public void Render(double frameDt)
        {
            bool reduced = game.cfg.movement.settings.reducedMotion; // the live REDUCED MOTION (CameraFeel reads it the same way)
            bool sprite = FeelAB.On && ringSprites.Ready; // NIGHT19: the textured ring
            if (sprite) ringSprites.Begin();
            bool drew = false;
            foreach (Ring r in rings)
            {
                if (!r.live) continue;
                r.age += frameDt;
                double t = r.age / r.time;
                if (t >= 1)
                {
                    r.live = false;
                    r.t.gameObject.SetActive(false);
                    continue;
                }
                double radius = reduced ? r.radius : r.radius * (0.2 + 0.8 * (1 - (1 - t) * (1 - t)));
                if (sprite)
                {
                    if (r.t.gameObject.activeSelf) r.t.gameObject.SetActive(false);
                    // the sprite's ring band (KenneyRingInner..Outer of its half-size) ends at the wave's radius
                    ringSprites.Disc(r.t.position, Vector3.up, (float)radius / UnityTuning.KenneyRingOuter, (float)(r.order % 7), r.c, (float)(1 - t));
                    drew = true;
                    continue;
                }
                if (!r.t.gameObject.activeSelf) r.t.gameObject.SetActive(true);
                r.t.localScale = new Vector3((float)radius, 1, (float)radius);
                Color c = (Fx49.On(game.cfg) ? r.c / UnityTuning.ImpactRingHdr : kinetic * glow) * (float)(1 - t); // NIGHT49 X fx49: the ring's own colour
                c.a = (float)(1 - t);
                block.SetColor(BaseColor, c);
                r.r.SetPropertyBlock(block);
            }
            if (drew) ringSprites.Draw();
        }

        /// <summary>A flat ring (y up, both faces) from `inner` to `outer` in `segments` quads.</summary>
        static Mesh RingMesh(float inner, float outer, int segments)
        {
            var v = new Vector3[segments * 2];
            var tri = new int[segments * 12];
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                float cs = Mathf.Cos(a), sn = Mathf.Sin(a);
                v[i * 2] = new Vector3(cs * inner, 0, sn * inner);
                v[i * 2 + 1] = new Vector3(cs * outer, 0, sn * outer);
                int j = (i + 1) % segments, k = i * 12;
                int a0 = i * 2, a1 = i * 2 + 1, b0 = j * 2, b1 = j * 2 + 1;
                tri[k] = a0; tri[k + 1] = b0; tri[k + 2] = a1;
                tri[k + 3] = a1; tri[k + 4] = b0; tri[k + 5] = b1;
                tri[k + 6] = a0; tri[k + 7] = a1; tri[k + 8] = b0;
                tri[k + 9] = a1; tri[k + 10] = b1; tri[k + 11] = b0;
            }
            var m = new Mesh { name = "Impact ring", vertices = v, triangles = tri };
            m.RecalculateBounds();
            return m;
        }
    }
}
