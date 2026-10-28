// NIGHT20 B5's look (builder HV): the VORTEX's orbs, on builder G2's port (Core/Guns/Vortex.cs; the orbs fly in
// Projectiles.vortexes: alive, open, openSteps, repel, position / previous), as projectiles.js render (:520-545) draws
// them; nobody drew them before (ProjectileViews has no vortex). Its viewmodel is V's (Viewmodel.Ports20.cs).
// - In flight: a small icosahedron orb (weapons.vortex.radius), turning at 2 rad/s, with a soft glow behind it.
// - Open (the pull): a bigger core, radius x (1.6 + 0.8 t) x (1 + 0.08 sin(3 spin)) turning at 5 rad/s (t the pull's share,
//   openSteps / simHz / pullTime), two turning rings tilted 0.4 and -0.7 rad at ringRadius x (1 - 0.5 t) and x 1.4 (the
//   Kenney ring sprite laid on the web's tilt), and motes drawn in: one every weapons.vortex.moteInterval s from a squashed
//   sphere of pullRadius (y x 0.4) at pullRadius / 0.5 m/s (particles.homing), drawn as streaks along their travel (the
//   pull you can see). The port's: while the held alt repels (Projectile.repel), the motes are born near the core and fly
//   out instead, so a repel reads unlike a pull.
// - The burst (vortexBurst): a flat ring sprite running out to weapons.vortex.burstRadius over VortexBurstRingTime (the
//   blast's reach; the web's burst cubes are Projectiles' own).
// Cyan, the player's (CLAUDE.md rule 8: the web's gravity violet is the family's on the gun; the effects are the player's,
// NIGHT19's ZAPPER / tonight's TORCH call). Render-side random stream; pooled; no allocation per frame; world time.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>projectiles.js render's VORTEX placements, pure (VortexLook20Tests).</summary>
    public static class VortexLook20
    {
        /// <summary>viewmodel.js :481 the hand's orb: max(0.001, (1 - reload) x (1 + 0.12 sin(5 t))), reload = 1 - the meter
        /// (G2's VortexMeter: 1 = ready).</summary>
        public static double HandOrbScale(double meterReady, double time)
        {
            double reload = 1 - Math.Max(0, Math.Min(1, meterReady));
            return Math.Max(0.001, (1 - reload) * (1 + 0.12 * Math.Sin(time * 5)));
        }

        /// <summary>:528 size = open ? radius x (1.6 + 0.8 t) x (1 + 0.08 sin(spin x 3)) : radius.</summary>
        public static double OrbSize(bool open, double t, double spin, double radius) =>
            open ? radius * (1.6 + 0.8 * t) * (1 + 0.08 * Math.Sin(spin * 3)) : radius;

        /// <summary>:527 t = openSteps / config.loop.simHz / pullTime.</summary>
        public static double OpenShare(int openSteps, double simHz, double pullTime) =>
            simHz > 0 && pullTime > 0 ? openSteps / simHz / pullTime : 0;

        /// <summary>:526 p.spin += frameDt x (open ? 5 : 2).</summary>
        public static double SpinRate(bool open) => open ? 5 : 2;

        /// <summary>:532 r = ringRadius x (1 - 0.5 t) (the second ring x 1.4).</summary>
        public static double RingRadius(double t, double ringRadius) => ringRadius * (1 - 0.5 * t);

        /// <summary>:539-541 a mote's birth: u in -1..1, a in 0..2 pi; (s cos a R, u R 0.4, s sin a R) round the centre.</summary>
        public static Vector3 MoteFrom(Vector3 c, double pullRadius, double u01, double a01)
        {
            double u = u01 * 2 - 1, a = a01 * Math.PI * 2, s = Math.Sqrt(Math.Max(0, 1 - u * u));
            return c + new Vector3((float)(s * Math.Cos(a) * pullRadius), (float)(u * pullRadius * 0.4), (float)(s * Math.Sin(a) * pullRadius));
        }

        /// <summary>:543 particles.homing(..., pullRadius / 0.5, 0.6): its speed.</summary>
        public static double MoteSpeed(double pullRadius) => pullRadius / 0.5;

        /// <summary>:537 p.trail += frameDt; while (p.trail > moteInterval) one mote.</summary>
        public static int MoteEmits(ref double trail, double frameDt, double interval)
        {
            if (!(interval > 0)) return 0;
            trail += frameDt;
            int n = 0;
            while (trail > interval)
            {
                trail -= interval;
                n++;
            }
            return n;
        }

        /// <summary>A mote's velocity: toward the centre at `speed` (a pull), away from it while the orb repels (the port's).</summary>
        public static Vector3 MoteVelocity(Vector3 at, Vector3 c, double speed, bool repel)
        {
            Vector3 d = c - at;
            float len = d.magnitude;
            if (len < 1e-5f) return Vector3.zero;
            return d / len * (float)(repel ? -speed : speed);
        }
    }

    /// <summary>The VORTEX's orbs, rings, motes and burst rings (SurvivalGame.VortexFx).</summary>
    public sealed class VortexFx20
    {
        struct Mote
        {
            public Vector3 pos, vel;
            public float age, life;
            public bool live;
        }

        struct Burst
        {
            public Vector3 at;
            public float age;
            public bool live;
        }

        readonly SurvivalGame game;
        readonly FxSprites trace, ring, flare;
        readonly Mesh orb;
        readonly RenderParams orbRp;
        readonly MaterialPropertyBlock orbBlock = new MaterialPropertyBlock();
        readonly double radius, ringRadius, pullRadius, pullTime, moteInterval, burstRadius, simHz;
        readonly double[] spins = new double[UnityTuning.VortexFxOrbs], trails = new double[UnityTuning.VortexFxOrbs];
        readonly Mote[] motes = new Mote[UnityTuning.VortexFxMotes];
        readonly Burst[] bursts = new Burst[UnityTuning.VortexFxOrbs];
        int nextMote, nextBurst;
        Mulberry32 rng;
        Color core, cyan, pale;
        bool coloursSet;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public VortexFx20(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            string v = "weapons.vortex.";
            bool has = cfg.Has(v + "radius");
            radius = has ? cfg.Num(v + "radius") : 0;
            ringRadius = has ? cfg.Num(v + "ringRadius") : 0;
            pullRadius = has ? cfg.Num(v + "pullRadius") : 0;
            pullTime = has ? cfg.Num(v + "pullTime") : 1;
            moteInterval = has ? cfg.Num(v + "moteInterval") : 0;
            burstRadius = has ? cfg.Num(v + "burstRadius") : 0;
            simHz = 1 / cfg.Dt;
            Material neon = game.mats.neon;
            trace = new FxSprites(neon, UnityTuning.KenneyTrace, UnityTuning.VortexFxMotes);
            ring = new FxSprites(neon, UnityTuning.KenneyRing, UnityTuning.VortexFxOrbs * 3);
            flare = new FxSprites(neon, UnityTuning.KenneyFlare, UnityTuning.VortexFxOrbs);
            var kit = new MeshKit();
            HandsPortKit20.Icosahedron1(kit, 1f, Color.white); // three's IcosahedronGeometry(1, 1), scaled per orb
            orb = kit.Build("VORTEX orb");
            Material m = FxDraw.Additive(neon, "VORTEX orb");
            m.SetFloat("_Cull", 0);
            orbRp = FxDraw.Params(m);
            rng = new Mulberry32(game.runSeed ^ UnityTuning.VortexFxSalt);
            game.events.On<BlastEvent>(G20Events.VortexBurst, OnBurst);
            game.OnNewRun += () =>
            {
                rng = new Mulberry32(game.runSeed ^ UnityTuning.VortexFxSalt);
                for (int i = 0; i < motes.Length; i++) motes[i].live = false;
                for (int i = 0; i < bursts.Length; i++) bursts[i].live = false;
                for (int i = 0; i < trails.Length; i++) trails[i] = spins[i] = 0;
            };
        }

        void Colours()
        {
            if (coloursSet || !UiTheme.Ready) return;
            coloursSet = true;
            core = UiTheme.CyanHot * UnityTuning.VortexCoreGlow;
            cyan = UiTheme.Cyan * UnityTuning.VortexRingGlow;
            pale = UiTheme.CyanPale * UnityTuning.VortexMoteGlow;
            trace.Warm(pale);
            ring.Warm(cyan);
            flare.Warm(cyan);
        }

        void OnBurst(BlastEvent e)
        {
            if (e == null || e.owner != "player") return;
            ref Burst b = ref bursts[nextBurst];
            nextBurst = (nextBurst + 1) % bursts.Length;
            b.at = WebSpace.ToUnity(e.position) + Vector3.up * UnityTuning.VortexBurstLift;
            b.age = 0;
            b.live = true;
        }

        /// <summary>Frame (the world's time, `alpha` the sim's interpolation): the live orbs, their motes, the burst rings.</summary>
        public void Render(double alpha, double dt)
        {
            Projectiles pr = game.projectiles;
            Camera cam = game.cam;
            if (pr == null || pr.vortexes == null || cam == null || !trace.Ready || !ring.Ready || !flare.Ready) return;
            Colours();
            Transform ct = cam.transform;
            Vector3 eye = ct.position;
            float fdt = (float)dt;
            trace.Begin();
            ring.Begin();
            flare.Begin();
            bool drew = false;
            Projectiles.Projectile[] orbs = pr.vortexes;
            for (int i = 0; i < orbs.Length && i < spins.Length; i++)
            {
                Projectiles.Projectile p = orbs[i];
                if (p == null || !p.alive)
                {
                    trails[i] = 0;
                    continue;
                }
                Vec3 wp = p.previous + (p.position - p.previous) * alpha;
                Vector3 at = WebSpace.ToUnity(wp);
                spins[i] += dt * VortexLook20.SpinRate(p.open);
                double t = p.open ? Math.Min(1, VortexLook20.OpenShare(p.openSteps, simHz, pullTime)) : 0;
                float size = (float)VortexLook20.OrbSize(p.open, t, spins[i], radius);
                Quaternion q = Quaternion.AngleAxis((float)(-spins[i] * Mathf.Rad2Deg), Vector3.up); // the z flip turns it the other way
                orbBlock.SetColor(BaseColor, core);
                RenderParams rp = orbRp;
                rp.matProps = orbBlock;
                Graphics.RenderMesh(rp, orb, 0, Matrix4x4.TRS(at, q, new Vector3(size, size, size)));
                flare.Billboard(at, size * UnityTuning.VortexGlowSize, 0, ct, cyan, p.open ? 1f : 0.6f);
                drew = true;
                if (!p.open)
                {
                    trails[i] = 0;
                    continue;
                }
                // the two rings (:532-535): tilted about the forward axis, turning about their own normal
                float r = (float)VortexLook20.RingRadius(t, ringRadius) / UnityTuning.KenneyRingMid;
                ring.Disc(at, new Vector3(-Mathf.Sin(0.4f), Mathf.Cos(0.4f), 0), r, (float)spins[i], cyan, 1f);
                ring.Disc(at, new Vector3(Mathf.Sin(0.7f), Mathf.Cos(0.7f), 0), r * 1.4f, (float)(-spins[i] * 1.3), cyan, 0.8f);
                // the motes (:536-544)
                int n = VortexLook20.MoteEmits(ref trails[i], dt, moteInterval);
                double speed = VortexLook20.MoteSpeed(pullRadius);
                for (int k = 0; k < n; k++)
                {
                    ref Mote mo = ref motes[nextMote];
                    nextMote = (nextMote + 1) % motes.Length;
                    mo.pos = VortexLook20.MoteFrom(at, p.repel ? pullRadius * UnityTuning.VortexRepelBirth : pullRadius, rng.Next(), rng.Next());
                    mo.vel = VortexLook20.MoteVelocity(mo.pos, at, speed, p.repel);
                    mo.age = 0;
                    mo.life = (float)(p.repel ? pullRadius * (1 - UnityTuning.VortexRepelBirth) / speed : UnityTuning.VortexMoteLife);
                    mo.live = true;
                }
            }
            for (int i = 0; i < motes.Length; i++)
            {
                ref Mote mo = ref motes[i];
                if (!mo.live) continue;
                mo.age += fdt;
                float step = mo.vel.magnitude * fdt;
                if (mo.age >= mo.life || step <= 0)
                {
                    mo.live = false;
                    continue;
                }
                mo.pos += mo.vel * fdt;
                float k = Mathf.Min(1f, mo.age / UnityTuning.VortexMoteIn) * (1f - mo.age / mo.life * 0.5f);
                trace.Streak(mo.pos - mo.vel * UnityTuning.VortexMoteStreak, mo.pos, UnityTuning.VortexMoteWidth, eye, pale, k);
                drew = true;
            }
            for (int i = 0; i < bursts.Length; i++)
            {
                ref Burst b = ref bursts[i];
                if (!b.live) continue;
                b.age += fdt;
                if (b.age >= UnityTuning.VortexBurstRingTime)
                {
                    b.live = false;
                    continue;
                }
                float u = b.age / UnityTuning.VortexBurstRingTime;
                float rr = (float)burstRadius * (0.25f + 0.75f * UiTween.OutCubic(u)) / UnityTuning.KenneyRingMid;
                ring.Disc(b.at, Vector3.up, rr, 0, cyan, 1f - u);
                drew = true;
            }
            if (!drew) return;
            flare.Draw();
            ring.Draw();
            trace.Draw();
        }
    }
}
