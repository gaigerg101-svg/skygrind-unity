// NIGHT20 C3 (builder V): the affixes' looks beside builder E's icons, VOLATILE disc and blink flashes (EnemyViews.Affix.cs),
// in each affix's own colour (config.affixes.kinds.<name>.color: the web's):
// - VOLATILE's burst, as affixes.js step draws it (:190-195) and E's view didn't: telegraph.flash at +0.8 m (radius x 0.6:
//   Telegraph20.Flash) and particles.burst(+0.8 m, its colour, 30, 11, 0.5, 1.8) (builder HV, banned 13: as textured
//   sparks, trace_06 streaks along their velocity with particles.js burst's rules, not the shared pool's cubes); and on its fuse (affixVolatile, the kill
//   to the burst: affixes.kinds.volatile.delay) a glow at the body growing from 0.25 to 0.6 of the burst's radius, pulsing
//   faster as the moment nears (the telegraph rim's 0.75 + 0.25 sin(t (6 + 18 k)) at 2 pi / 6 the rate, so a 0.4 s fuse
//   beats about twice; whole on it): "it's about to go"; and (builder HV, A1) affixes.js onKill's own telegraph.circle
//   (r radius, time delay) on the floor where it fell, through Telegraph20: the boss's language, full at the burst;
// - the BLINKER's path (affixBlink): a streak from where it left to where it landed (+1 m), with a few afterimage sparks
//   along it, fading over BlinkTrailLife: where it went reads at a glance (the web flashes the two ends only: E's);
// - REGENERATING while it heals (out of damage, under its spawn HP: the rule's own test): a soft breathing glow on the body
//   and motes rising off it;
// - SWIFT: two short streaks trailing from its shoulders by its speed (SwiftTrailTime s of its travel).
// The affixes are enemy tells (CLAUDE.md rule 7 keeps the web's design: these only show what the rules do). Visual only;
// world time; the records are the sim's (Affixes.Live, read, never written); pooled sprites; no allocation per frame.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The affixes' looks, pure (AffixFx20Tests).</summary>
    public static class AffixFxMath
    {
        static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;

        /// <summary>The fuse's pulse at `age` of `fuse` s: the telegraph rim's 0.75 + 0.25 sin(age (6 + 18 k)), 1 at the burst.</summary>
        public static double FusePulse(double age, double fuse)
        {
            double k = fuse > 0 ? Clamp01(age / fuse) : 1;
            return k >= 1 ? 1 : 0.75 + 0.25 * Math.Sin(age * (6 + 18 * k) * Math.PI * 2 / 6); // a full beat early, quickening
        }

        /// <summary>The fuse's glow size (m across): FuseGlowFrom .. FuseGlowTo of the burst's radius over the fuse.</summary>
        public static double FuseGlowSize(double age, double fuse, double radius)
        {
            double k = fuse > 0 ? Clamp01(age / fuse) : 1;
            return radius * (UnityTuning.FuseGlowFrom + (UnityTuning.FuseGlowTo - UnityTuning.FuseGlowFrom) * k * k);
        }

        // --- NIGHT20 row 4 (builder HV), banned 13: VOLATILE's burst as textured sparks, particles.js burst's rules ---------

        /// <summary>affixes.js :194 particles.burst(..., 30, 11, 0.5, 1.8): the count.</summary>
        public const int BurstCount = 30;

        /// <summary>particles.js burst: randomDir x speed x (1 - speedJitter x random).</summary>
        public static Vector3 BurstVelocity(Vector3 unit, double speed, double jitter, double r01) => unit * (float)(speed * (1 - jitter * r01));

        /// <summary>particles.js update: p.vel.y -= config.fx.gravity x dt (a burst's particle has gravity).</summary>
        public static Vector3 SparkFall(Vector3 vel, double gravity, double dt) => new Vector3(vel.x, (float)(vel.y - gravity * dt), vel.z);

        /// <summary>A spark's streak width (m): particles.js's cube (life / maxLife) x scale x fx.size, x AffixSparkWidth across trace_06.</summary>
        public static double SparkWidth(double life, double maxLife, double scale, double size) =>
            maxLife > 0 ? Clamp01(life / maxLife) * scale * size * UnityTuning.AffixSparkWidth : 0;

        /// <summary>REGENERATING is healing now (affixes.js step: calm 0 and under its spawn HP), alive.</summary>
        public static bool Healing(bool regenerating, int calm, double hp, double max, bool alive) => alive && regenerating && calm == 0 && hp < max;

        /// <summary>SWIFT's trail (m) at `speed` m/s: none under SwiftTrailFrom, SwiftTrailTime s of travel, at most SwiftTrailMax.</summary>
        public static double SwiftTrail(double speed) =>
            !(speed > UnityTuning.SwiftTrailFrom) ? 0 : Math.Min(UnityTuning.SwiftTrailMax, speed * UnityTuning.SwiftTrailTime);
    }

    /// <summary>The affixes' looks (SurvivalGame.AffixFx).</summary>
    public sealed class AffixFx20
    {
        /// <summary>NIGHT20 A1 (builder HV): VOLATILE's fuse as affixes.js onKill :212 draws it: telegraph.circle({x, y, z,
        /// r: k.radius, time: k.delay}) where it fell, the telegraph language's DANGER circle filling to the burst (whose
        /// flash OnBurst draws). The shape's id (0: no view).</summary>
        public static int VolatileTell(ITelegraph t, AffixFxEvent e) =>
            t == null || e == null ? 0 : t.Circle(e.position.x, e.position.y, e.position.z, e.radius, e.time);

        struct Fuse
        {
            public int id;
            public Vector3 at;
            public float age, time, radius;
            public bool live;
        }

        struct Trail
        {
            public Vector3 a, b;
            public float age;
            public bool live;
        }

        struct Mote
        {
            public Vector3 pos;
            public float age;
            public bool live;
        }

        struct Spark
        {
            public Vector3 pos, vel;
            public float life, maxLife, scale;
            public bool live;
        }

        readonly Spark[] sparks = new Spark[UnityTuning.AffixSparkPool];
        int nextSpark;
        Mulberry32 sparkRng;
        readonly double sparkJitter, sparkLifeJitter, sparkGravity, sparkSize;

        readonly SurvivalGame game;
        readonly FxSprites flare, trace, star;
        readonly Fuse[] fuses = new Fuse[8];
        readonly Trail[] trails = new Trail[8];
        readonly Mote[] motes = new Mote[32];
        int nextTrail, nextMote;
        double clock, moteBudget;
        readonly Color volatileC, blinkerC, regenC, swiftC;
        readonly float burstFlash;

        public AffixFx20(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            Material neon = game.mats.neon;
            flare = new FxSprites(neon, UnityTuning.KenneyFlare, UnityTuning.AffixFxPool);
            trace = new FxSprites(neon, UnityTuning.KenneyTrace, UnityTuning.AffixFxPool + UnityTuning.AffixSparkPool); // + the bursts' sparks
            star = new FxSprites(neon, UnityTuning.KenneyStar, UnityTuning.AffixFxPool);
            volatileC = Look.C(cfg, "affixes.kinds.volatile.color");
            blinkerC = Look.C(cfg, "affixes.kinds.blinker.color");
            regenC = Look.C(cfg, "affixes.kinds.regenerating.color");
            swiftC = Look.C(cfg, "affixes.kinds.swift.color");
            burstFlash = 0.6f; // affixes.js: telegraph.flash(..., k.radius * 0.6)
            sparkJitter = cfg.Num("fx.speedJitter");
            sparkLifeJitter = cfg.Num("fx.lifeJitter");
            sparkGravity = cfg.Num("fx.gravity");
            sparkSize = cfg.Num("fx.size");
            sparkRng = new Mulberry32(game.runSeed ^ UnityTuning.AffixSparkSalt);
            game.events.On<AffixFxEvent>("affixVolatile", OnVolatile);
            game.events.On<AffixFxEvent>("affixBurst", OnBurst);
            game.events.On<AffixFxEvent>("affixBlink", OnBlink);
            game.OnNewRun += () =>
            {
                for (int i = 0; i < fuses.Length; i++) fuses[i].live = false;
                for (int i = 0; i < trails.Length; i++) trails[i].live = false;
                for (int i = 0; i < motes.Length; i++) motes[i].live = false;
                for (int i = 0; i < sparks.Length; i++) sparks[i].live = false;
                sparkRng = new Mulberry32(game.runSeed ^ UnityTuning.AffixSparkSalt);
            };
        }

        void OnVolatile(AffixFxEvent e)
        {
            VolatileTell(game.Telegraphs, e); // NIGHT20 A1 (builder HV): the web's telegraph circle over the fuse
            int slot = 0;
            for (int i = 0; i < fuses.Length; i++)
                if (!fuses[i].live) { slot = i; break; }
            fuses[slot] = new Fuse { id = e.id, at = WebSpace.ToUnity(new Vec3(e.position.x, e.position.y + 0.8, e.position.z)), age = 0, time = (float)e.time, radius = (float)e.radius, live = true };
        }

        /// <summary>affixes.js :190-195: the burst's flash and its 30 particles (E's view only ends its disc).</summary>
        void OnBurst(AffixFxEvent e)
        {
            for (int i = 0; i < fuses.Length; i++)
                if (fuses[i].live && fuses[i].id == e.id) fuses[i].live = false;
            game.Telegraphs?.Flash(e.position.x, e.position.y + 0.8, e.position.z, e.radius * burstFlash);
            // affixes.js :194 particles.burst(+0.8 m, its colour, 30, 11, 0.5, 1.8), as textured sparks (banned 13: no cubes)
            Vector3 at = WebSpace.ToUnity(new Vec3(e.position.x, e.position.y + 0.8, e.position.z));
            for (int i = 0; i < AffixFxMath.BurstCount; i++)
            {
                ref Spark s = ref sparks[nextSpark];
                nextSpark = (nextSpark + 1) % sparks.Length;
                Vector3 dir = SprayMath.UnitSphere(sparkRng.Next(), sparkRng.Next());
                s.pos = at;
                s.vel = AffixFxMath.BurstVelocity(dir, 11, sparkJitter, sparkRng.Next());
                s.life = s.maxLife = (float)SprayMath.Life(0.5, sparkLifeJitter, sparkRng.Next());
                s.scale = 1.8f;
                s.live = true;
            }
        }

        void OnBlink(AffixFxEvent e)
        {
            ref Trail t = ref trails[nextTrail];
            nextTrail = (nextTrail + 1) % trails.Length;
            t.a = WebSpace.ToUnity(new Vec3(e.position.x, e.position.y + 1, e.position.z));
            t.b = WebSpace.ToUnity(new Vec3(e.to.x, e.to.y + 1, e.to.z));
            t.age = 0;
            t.live = true;
        }

        /// <summary>Frame (world time): the fuses, the blink paths, the healers' glow and motes, the swift's trails.</summary>
        public void Render(double alpha, double dt)
        {
            Camera cam = game.cam;
            if (cam == null || !flare.Ready || !trace.Ready || !star.Ready) return;
            Transform ct = cam.transform;
            Vector3 eye = ct.position;
            float fdt = (float)dt;
            clock += dt;
            flare.Begin();
            trace.Begin();
            star.Begin();
            bool drew = false;
            for (int i = 0; i < fuses.Length; i++)
            {
                ref Fuse f = ref fuses[i];
                if (!f.live) continue;
                f.age += fdt;
                if (f.age > f.time + 0.1f) f.live = false; // the burst's event ends it; a safety stop
                float size = (float)AffixFxMath.FuseGlowSize(f.age, f.time, f.radius);
                float k = (float)AffixFxMath.FusePulse(f.age, f.time) * UnityTuning.AffixFxAlpha;
                flare.Billboard(f.at, size, 0, ct, volatileC * UnityTuning.FuseGlowHdr, k);
                drew = true;
            }
            for (int i = 0; i < trails.Length; i++)
            {
                ref Trail t = ref trails[i];
                if (!t.live) continue;
                t.age += fdt;
                if (t.age >= UnityTuning.BlinkTrailLife)
                {
                    t.live = false;
                    continue;
                }
                float k = (1 - t.age / (float)UnityTuning.BlinkTrailLife) * UnityTuning.AffixFxAlpha;
                trace.Streak(t.a, t.b, UnityTuning.BlinkTrailWidth, eye, blinkerC, k);
                for (int s = 1; s <= UnityTuning.BlinkTrailEchoes; s++)
                {
                    float u = s / (float)(UnityTuning.BlinkTrailEchoes + 1);
                    star.Billboard(Vector3.Lerp(t.a, t.b, u), 0.35f * (1 - u * 0.5f), u * 3, ct, blinkerC, k * (1 - u * 0.6f));
                }
                drew = true;
            }
            Affixes ax = game.AffixesNow;
            if (ax != null && ax.all)
            {
                var live = ax.Live;
                bool anyHealing = false;
                for (int i = 0; i < live.Count; i++)
                {
                    AffixRec r = live[i];
                    Husk h = r.h;
                    if (h == null || !h.alive) continue;
                    Vec3 p = h.previous + (h.position - h.previous) * alpha;
                    Vector3 body = WebSpace.ToUnity(p) + Vector3.up * (float)(h.Height * 0.55);
                    if (AffixFxMath.Healing(r.regenerating, r.calm, h.hp, r.max, h.alive))
                    {
                        float breathe = (float)(0.6 + 0.4 * Math.Sin(clock * UnityTuning.RegenPulseHz * Math.PI * 2));
                        flare.Billboard(body, (float)UnityTuning.RegenGlowSize, 0, ct, regenC, breathe * UnityTuning.AffixFxAlpha * 0.6f);
                        drew = true;
                        if (!anyHealing && moteBudget <= 0)
                        {
                            ref Mote m = ref motes[nextMote];
                            nextMote = (nextMote + 1) % motes.Length;
                            m.pos = body + new Vector3((float)Math.Sin(clock * 13) * 0.3f, 0, (float)Math.Cos(clock * 11) * 0.3f);
                            m.age = 0;
                            m.live = true;
                        }
                        anyHealing = true;
                    }
                    if (r.swift)
                    {
                        Vector3 vel = WebSpace.ToUnity(h.velocity);
                        vel.y = 0;
                        float speed = vel.magnitude;
                        float len = (float)AffixFxMath.SwiftTrail(speed);
                        if (len > 0)
                        {
                            Vector3 back = -vel / speed, side = Vector3.Cross(Vector3.up, back) * (float)(h.Radius * 0.9);
                            for (int s = -1; s <= 1; s += 2)
                            {
                                Vector3 a = body + side * s + Vector3.up * 0.2f;
                                trace.Streak(a + back * len, a, UnityTuning.SwiftTrailWidth, eye, swiftC, UnityTuning.AffixFxAlpha);
                            }
                            drew = true;
                        }
                    }
                }
                moteBudget = anyHealing ? (moteBudget <= 0 ? UnityTuning.RegenMoteEvery : moteBudget - dt) : 0;
            }
            for (int i = 0; i < motes.Length; i++)
            {
                ref Mote m = ref motes[i];
                if (!m.live) continue;
                m.age += fdt;
                if (m.age >= UnityTuning.RegenMoteLife)
                {
                    m.live = false;
                    continue;
                }
                m.pos += Vector3.up * (float)(UnityTuning.RegenMoteRise * dt);
                float k = (1 - m.age / (float)UnityTuning.RegenMoteLife) * UnityTuning.AffixFxAlpha;
                star.Billboard(m.pos, 0.22f, m.age * 4, ct, regenC, k);
                drew = true;
            }
            for (int i = 0; i < sparks.Length; i++)
            {
                ref Spark s = ref sparks[i];
                if (!s.live) continue;
                s.life -= fdt;
                if (s.life <= 0)
                {
                    s.live = false;
                    continue;
                }
                s.vel = AffixFxMath.SparkFall(s.vel, sparkGravity, dt);
                s.pos += s.vel * fdt;
                float w = (float)AffixFxMath.SparkWidth(s.life, s.maxLife, s.scale, sparkSize);
                trace.Streak(s.pos - s.vel * UnityTuning.AffixSparkStreak, s.pos, Mathf.Max(w, 0.01f), eye, volatileC, s.life / s.maxLife);
                drew = true;
            }
            if (!drew) return;
            flare.Draw();
            trace.Draw();
            star.Draw();
        }
    }
}
