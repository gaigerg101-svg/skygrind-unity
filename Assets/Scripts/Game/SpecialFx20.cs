// NIGHT20 A2 / A4 (builder V): the special rounds' looks, in DANGER (colors.enemy: red-orange is enemies and danger, rule 8).
// - A2 THE HUNT (E's specialRound "hunt"; its squads come from a ring around you, design.adds.hunt.ringMin .. ringMax m, each
//   through the spawner's own telegraph beam, TelegraphView): from the announce, ripples on the floor close in from ringMax
//   to ringMin around you, HuntRingRipples out of step, every HuntRingPeriod s (CoD's hellhound rounds announce the ring
//   the dogs come from with fog and strikes all around; here the band itself, contracting); held 4 s past the round's start
//   (its first squads out), faded over 1 s, gone at its end. With HuntSky20's dusk shift and H's slab.
// - A4 THE ELITE (E's SpecialRounds.EliteId, its name plate is H's): a turning DANGER rim on the floor under it (the Kenney
//   ring sprite), a breathing glow behind its body and embers rising off it, so the named enemy reads across the arena.
// Visual only (the director decides both); frame time for the ring (a slow-mo doesn't slow a warning), world time for the
// aura; pooled; no allocation per frame.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The special rounds' looks, pure (SpecialFx20Tests).</summary>
    public static class SpecialFxMath
    {
        static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;

        static double Smooth(double e0, double e1, double x)
        {
            double t = Clamp01((x - e0) / (e1 - e0));
            return t * t * (3 - 2 * t);
        }

        static double Phase(double t, int ripple)
        {
            double u = t / UnityTuning.HuntRingPeriod + ripple / (double)Math.Max(1, UnityTuning.HuntRingRipples);
            return u - Math.Floor(u);
        }

        /// <summary>Ripple `ripple`'s radius at `t` s: from ringMax to ringMin over its period, gathering speed (u^2).</summary>
        public static double RippleRadius(double t, int ripple, double ringMin, double ringMax)
        {
            double u = Phase(t, ripple);
            return ringMax - (ringMax - ringMin) * u * u;
        }

        /// <summary>A ripple's brightness share: in over its first 15 %, out over its last 25 %.</summary>
        public static double RippleAlpha(double t, int ripple)
        {
            double u = Phase(t, ripple);
            return Smooth(0, 0.15, u) * (1 - Smooth(0.75, 1, u));
        }

        /// <summary>The ring's share: in over HuntRingIn from the announce (-1: none seen), held HuntRingHold past the start (-1:
        /// not started), out over HuntRingOut; 0 at the end.</summary>
        public static double HuntShare(double sinceAnnounce, double sinceStart, bool ended)
        {
            if (ended || (sinceAnnounce < 0 && sinceStart < 0)) return 0;
            double fin = sinceAnnounce >= 0 ? Clamp01(sinceAnnounce / UnityTuning.HuntRingIn) : 1;
            double fout = sinceStart >= 0 ? 1 - Clamp01((sinceStart - UnityTuning.HuntRingHold) / UnityTuning.HuntRingOut) : 1;
            return fin * fout;
        }
    }

    /// <summary>THE HUNT's ring and THE ELITE's aura (SurvivalGame.SpecialFx).</summary>
    public sealed class SpecialFx20
    {
        readonly SurvivalGame game;
        readonly double ringMin, ringMax;
        readonly Mesh ring;
        readonly RenderParams rp;
        readonly MaterialPropertyBlock[] blocks = new MaterialPropertyBlock[UnityTuning.HuntRingRipples];
        readonly Color danger;
        double clock, announceAt = -1, startAt = -1;
        bool ended = true;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        // the elite
        readonly FxSprites rim, glow, ember;
        struct Ember
        {
            public Vector3 pos;
            public float age;
            public bool live;
        }
        readonly Ember[] embers = new Ember[24];
        int nextEmber;
        double emberBudget, auraClock;

        public SpecialFx20(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            ringMin = cfg.Has("design.adds.hunt.ringMin") ? cfg.Num("design.adds.hunt.ringMin") : 12;
            ringMax = cfg.Has("design.adds.hunt.ringMax") ? cfg.Num("design.adds.hunt.ringMax") : 20;
            danger = Look.C(cfg, "colors.enemy");
            ring = FxDraw.Ring(1 - UnityTuning.HuntRingWidth, 1, UnityTuning.HuntRingSegments, "hunt ring");
            Material m = FxDraw.Additive(game.mats.neon, "Hunt ring");
            m.SetFloat("_Cull", 0);
            rp = FxDraw.Params(m);
            for (int i = 0; i < blocks.Length; i++) blocks[i] = new MaterialPropertyBlock();
            rim = new FxSprites(game.mats.neon, UnityTuning.KenneyRing, 4);
            glow = new FxSprites(game.mats.neon, UnityTuning.KenneyFlare, 4);
            ember = new FxSprites(game.mats.neon, UnityTuning.KenneyStar, embers.Length);
            game.events.On(N20Events.SpecialRound, OnSpecial);
            game.OnNewRun += () =>
            {
                announceAt = startAt = -1;
                ended = true;
                for (int i = 0; i < embers.Length; i++) embers[i].live = false;
            };
        }

        void OnSpecial(object payload)
        {
            if (!(payload is SpecialRoundEvent e) || e.name != SpecialRounds.Hunt) return;
            switch (e.phase)
            {
                case "announce":
                    announceAt = clock;
                    startAt = -1;
                    ended = false;
                    break;
                case "start":
                    if (ended) announceAt = -1; // a start with no announce seen: shown at once
                    startAt = clock;
                    ended = false;
                    break;
                case "end":
                    ended = true;
                    break;
            }
        }

        /// <summary>Frame: the ring (frame time), the elite's aura (world time).</summary>
        public void Render(double alpha, double frameDt, double worldDt)
        {
            clock += frameDt;
            if (game.cam == null || game.mover == null) return;
            double share = SpecialFxMath.HuntShare(announceAt >= 0 ? clock - announceAt : -1, startAt >= 0 ? clock - startAt : -1, ended);
            if (share > 0 && game.Active)
            {
                Vector3 feet = WebSpace.ToUnity(game.mover.position) + Vector3.up * 0.05f;
                for (int r = 0; r < UnityTuning.HuntRingRipples; r++)
                {
                    float rad = (float)SpecialFxMath.RippleRadius(clock, r, ringMin, ringMax);
                    float a = (float)(SpecialFxMath.RippleAlpha(clock, r) * share) * UnityTuning.HuntRingAlpha;
                    if (a <= 0) continue;
                    blocks[r].SetColor(BaseColor, danger * a);
                    RenderParams p = rp;
                    p.matProps = blocks[r];
                    Graphics.RenderMesh(p, ring, 0, new Matrix4x4(new Vector4(rad, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, rad, 0), new Vector4(feet.x, feet.y, feet.z, 1)));
                }
            }
            Aura(alpha, worldDt);
        }

        void Aura(double alpha, double dt)
        {
            if (!rim.Ready || !glow.Ready || !ember.Ready) return;
            SpecialRounds sp = game.SpecialsNow;
            Enemy elite = null;
            if (sp != null && sp.EliteId >= 0 && game.enemies != null)
            {
                var all = game.enemies.all;
                for (int i = 0; i < all.Count; i++)
                    if (all[i].id == sp.EliteId && all[i].alive) { elite = all[i]; break; }
            }
            Transform ct = game.cam.transform;
            auraClock += dt;
            rim.Begin();
            glow.Begin();
            ember.Begin();
            bool drew = false;
            if (elite != null)
            {
                Vec3 p = elite.previous + (elite.position - elite.previous) * alpha;
                Vector3 feet = WebSpace.ToUnity(p) + Vector3.up * 0.06f;
                float breathe = (float)(0.75 + 0.25 * Math.Sin(auraClock * UnityTuning.EliteAuraPulseHz * Math.PI * 2));
                float r = (float)(elite.Radius * UnityTuning.EliteAuraSize) / UnityTuning.KenneyRingOuter;
                rim.Disc(feet, Vector3.up, r, (float)(auraClock * UnityTuning.EliteAuraSpin), danger * 2, UnityTuning.EliteAuraAlpha * breathe);
                Vector3 body = feet + Vector3.up * (float)(elite.Height * 0.5);
                glow.Billboard(body, (float)(elite.Height * UnityTuning.EliteGlowSize), 0, ct, danger, UnityTuning.EliteGlowAlpha * breathe);
                emberBudget += dt / UnityTuning.EliteEmberEvery;
                while (emberBudget >= 1)
                {
                    emberBudget--;
                    ref Ember em = ref embers[nextEmber];
                    nextEmber = (nextEmber + 1) % embers.Length;
                    double a = auraClock * 7.3 + nextEmber * 2.1;
                    em.pos = feet + new Vector3((float)(Math.Cos(a) * elite.Radius), (float)(elite.Height * 0.2), (float)(Math.Sin(a) * elite.Radius));
                    em.age = 0;
                    em.live = true;
                }
                drew = true;
            }
            else emberBudget = 0;
            for (int i = 0; i < embers.Length; i++)
            {
                ref Ember em = ref embers[i];
                if (!em.live) continue;
                em.age += (float)dt;
                if (em.age >= UnityTuning.EliteEmberLife)
                {
                    em.live = false;
                    continue;
                }
                em.pos += Vector3.up * (float)(UnityTuning.EliteEmberRise * dt);
                ember.Billboard(em.pos, 0.18f, em.age * 5, ct, danger * 2, 1 - em.age / (float)UnityTuning.EliteEmberLife);
                drew = true;
            }
            if (!drew) return;
            rim.Draw();
            glow.Draw();
            ember.Draw();
        }
    }
}
