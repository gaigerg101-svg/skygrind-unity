// NIGHT20 (builder V): the web's gunFx.js sprays for tonight's guns (src/combat/gunFx.js :65-84; particles.js spray() :79-94):
// - `doubleBlast` (the THUNDERCLAP's double blast, builder G's N20Events.DoubleBlast): 14 particles sprayed from the muzzle
//   along the aim, spread 0.5, speed 18, life 0.25, scale 1.2, carrying the mover's velocity (gunFx.js :71-75);
// - `coneWave` (its shock cone, N20Events.ConeWave): five bursts of 5 particles (speed 6, life 0.3, scale 0.9) standing
//   1.5 + 1.3 i m out along the cone, 0.3 m low (gunFx.js :65-70);
// - `flame` (the TORCH, builder G's port): config.weaponFx.flameCount particles at flameSpread / flameSpeed /
//   flameLife / flameScale / flameRise, carrying the mover's velocity (gunFx.js :77-84);
// - `fireWall` (the TORCH's special, builder HV): flames licking up along the wall's line at weaponFx.fireWallRate a second
//   while it lasts (gunFx.js :41 and update :129-142), and the port's own floor band (trace_06 laid flat, CYAN, as wide as
//   the burn: FireWall20) so the line it burns reads from anywhere.
// The aim's sprays (the flame, the double blast) converge on the crosshair down the aim line: within Spray20ClearInner px of
// the screen's centre a streak isn't drawn (smooth to Spray20ClearOuter; banned 16: effects over the crosshair).
// The web's colours are its families' (edge amber, the flame's gold-orange); here the player's cyan ladder (CLAUDE.md rule 8:
// cyan is the player's; red-orange is danger only; NIGHT19's ZAPPER took the same call for the volt lime): CYAN-HOT, CYAN,
// CYAN-PALE in the web's alternation. The web's cubes become soft trace_06 streaks laid along their velocity (NIGHT19's
// banned pattern 13: no untextured particles), x fx.size; no gravity (spray's p.gravity = false). The THUNDERCLAP's air,
// rings and the AIR streaks' burst are MovementFx's and SpeedLines20's; this is the web's own spray on top. Visual only;
// world time (frozen in a hitstop); its dice its own Mulberry32 (a render-side stream); pooled, no allocation per frame.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>particles.js spray() and gunFx.js's placements, pure (Spray20Tests).</summary>
    public static class SprayMath
    {
        /// <summary>
        /// particles.js spray's velocity: (dir + randomDir x sin(spread) x r).normalize() x speed x (1 - speedJitter x jit),
        /// then + rise on y, + the inherited velocity.
        /// </summary>
        public static Vector3 Velocity(Vector3 dir, Vector3 unitRandom, double spread, double r01, double speed, double speedJitter, double jit01, double rise, Vector3 inherit)
        {
            Vector3 v = (dir + unitRandom * (float)(Math.Sin(spread) * r01)).normalized * (float)(speed * (1 - speedJitter * jit01));
            v.y += (float)rise;
            return v + inherit;
        }

        /// <summary>A particle's life: life x (1 - lifeJitter x r).</summary>
        public static double Life(double life, double lifeJitter, double r01) => life * (1 - lifeJitter * r01);

        /// <summary>gunFx.js coneWave's burst i: 1.5 + 1.3 i m along the cone from its origin, 0.3 m low.</summary>
        public static Vector3 ConeBurstAt(Vector3 origin, Vector3 dir, int i)
        {
            float d = 1.5f + i * 1.3f;
            return origin + dir * d - new Vector3(0, 0.3f, 0);
        }

        // --- NIGHT20 B5 (builder HV): the TORCH's fire wall, gunFx.js update :129-142 -------------------------------------

        /// <summary>fw.carry += frameDt x fireWallRate; while (carry >= 1) one flame: the flames this frame.</summary>
        public static int WallEmits(ref double carry, double frameDt, double rate)
        {
            carry += frameDt * rate;
            int n = 0;
            while (carry >= 1)
            {
                carry -= 1;
                n++;
            }
            return n;
        }

        /// <summary>a.lerpVectors(fw.a, fw.b, random): a flame's foot on the line.</summary>
        public static Vector3 WallAt(Vector3 a, Vector3 b, double r01) => Vector3.LerpUnclamped(a, b, (float)r01);

        /// <summary>flat.set((random - 0.5) x 0.4, 1, (random - 0.5) x 0.4).normalize(): a flame's way up.</summary>
        public static Vector3 WallUp(double r1, double r2) => new Vector3((float)((r1 - 0.5) * 0.4), 1, (float)((r2 - 0.5) * 0.4)).normalized;

        /// <summary>3 + random x 2 m/s.</summary>
        public static double WallSpeed(double r01) => 3 + r01 * 2;

        /// <summary>NIGHT20 row 4 (builder HV), banned 16: an aim spray's streak `pxFromCentre` px from the crosshair on a screen
        /// `screenHeight` px tall: 0 within Spray20ClearInner (at 1080p), 1 from Spray20ClearOuter, smoothstep between.</summary>
        public static float CrossClear(float pxFromCentre, float screenHeight)
        {
            float d = screenHeight > 0 ? pxFromCentre * 1080f / screenHeight : pxFromCentre;
            float t = Mathf.Clamp01((d - UnityTuning.Spray20ClearInner) / (UnityTuning.Spray20ClearOuter - UnityTuning.Spray20ClearInner));
            return t * t * (3f - 2f * t);
        }

        /// <summary>particles.js randomDir: uniform on the unit sphere (u, v in 0..1).</summary>
        public static Vector3 UnitSphere(double u01, double v01)
        {
            double u = u01 * 2 - 1, phi = v01 * Math.PI * 2, s = Math.Sqrt(Math.Max(0, 1 - u * u));
            return new Vector3((float)(s * Math.Cos(phi)), (float)u, (float)(s * Math.Sin(phi)));
        }
    }

    /// <summary>
    /// NIGHT20 B5 (builder HV): a live fire wall's floor band, the port's look beside the web's flames (the web draws the
    /// flames only; a band says where it burns: Torch.cs burns a body within the wall's width of its line). It comes up
    /// over FireWallBandIn (ease-out), holds, fades over the wall's last FireWallBandOut (ease-in), gone with the wall.
    /// </summary>
    public static class FireWall20
    {
        public static float Alpha(double age, double life)
        {
            if (!(age >= 0) || age >= life) return 0f;
            float k = age < UnityTuning.FireWallBandIn ? UiTween.OutCubic(age / UnityTuning.FireWallBandIn) : 1f;
            double left = life - age;
            if (left < UnityTuning.FireWallBandOut) k = Mathf.Min(k, 1f - UiTween.InCubic(1 - left / UnityTuning.FireWallBandOut));
            return k;
        }
    }

    /// <summary>The sprays' pool and draw (SurvivalGame.GunSprays).</summary>
    public sealed class GunSpray20
    {
        struct P
        {
            public Vector3 pos, vel;
            public float life, maxLife, scale;
            public int colour;
            public bool live;
            public bool aim; // down the aim line (the flame, the double blast): kept off the crosshair (banned 16)
        }

        readonly SurvivalGame game;
        readonly P[] parts = new P[UnityTuning.Spray20Pool];
        int next;
        readonly FxSprites trace;
        Mulberry32 rng;
        readonly double speedJitter, lifeJitter;
        readonly float size;
        readonly int flameCount;
        readonly double flameSpeed, flameLife, flameSpread, flameScale, flameRise;
        int flameColour;
        readonly Color[] ladder = new Color[3];
        bool ladderSet;
        Color bandColour;

        // NIGHT20 B5 (builder HV): the TORCH's live fire walls (gunFx.js's fireWalls: {a, b, age, life, carry}), a fixed pool
        // the size of Torch.cs's (the oldest gives way, as the sim's does)
        struct Wall
        {
            public Vector3 a, b;
            public float age, life, width;
            public double carry;
            public bool live;
        }

        readonly Wall[] walls = new Wall[UnityTuning.FireWall20Slots];
        readonly double wallRate;

        public GunSpray20(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            trace = new FxSprites(game.mats.neon, UnityTuning.KenneyTrace, parts.Length);
            rng = new Mulberry32(game.runSeed ^ UnityTuning.Spray20SeedSalt);
            speedJitter = cfg.Num("fx.speedJitter");
            lifeJitter = cfg.Num("fx.lifeJitter");
            size = (float)cfg.Num("fx.size");
            flameCount = cfg.Int("weaponFx.flameCount");
            flameSpeed = cfg.Num("weaponFx.flameSpeed");
            flameLife = cfg.Num("weaponFx.flameLife");
            flameSpread = cfg.Num("weaponFx.flameSpread");
            flameScale = cfg.Num("weaponFx.flameScale");
            flameRise = cfg.Num("weaponFx.flameRise");
            wallRate = cfg.Num("weaponFx.fireWallRate");
            game.events.On(N20Events.DoubleBlast, OnDoubleBlast);
            game.events.On(N20Events.ConeWave, OnConeWave);
            game.events.On(G20Events.Flame, OnFlame);
            game.events.On(G20Events.FireWall, OnFireWall);
            game.OnNewRun += () =>
            {
                rng = new Mulberry32(game.runSeed ^ UnityTuning.Spray20SeedSalt);
                for (int i = 0; i < parts.Length; i++) parts[i].live = false;
                for (int i = 0; i < walls.Length; i++) walls[i].live = false;
            };
        }

        Color Ladder(int i)
        {
            if (!ladderSet && UiTheme.Ready)
            {
                ladderSet = true;
                ladder[0] = UiTheme.CyanHot;
                ladder[1] = UiTheme.Cyan;
                ladder[2] = UiTheme.CyanPale;
                for (int k = 0; k < 3; k++) trace.Warm(ladder[k] * UnityTuning.Spray20Glow);
                bandColour = UiTheme.Cyan * UnityTuning.FireWallBandGlow; // the fire wall's floor band: the player's cyan
                trace.Warm(bandColour);
            }
            return ladderSet ? ladder[i % 3] : Color.white;
        }

        Vector3 Muzzle(Vector3 eye, Vector3 aim) =>
            game.viewmodel != null && game.viewmodel.MuzzleWorld(out Vector3 m) ? m : eye + aim * 0.5f;

        void Spray(Vector3 at, Vector3 dir, double spread, int colour, double speed, double life, double scale, double rise, Vector3 inherit, bool aim = false)
        {
            ref P p = ref parts[next];
            next = (next + 1) % parts.Length;
            Vector3 rnd = SprayMath.UnitSphere(rng.Next(), rng.Next());
            p.pos = at;
            p.vel = SprayMath.Velocity(dir, rnd, spread, rng.Next(), speed, speedJitter, rng.Next(), rise, inherit);
            p.life = p.maxLife = (float)SprayMath.Life(life, lifeJitter, rng.Next());
            p.scale = (float)scale;
            p.colour = colour;
            p.live = true;
            p.aim = aim;
        }

        void Burst(Vector3 at, int count, double speed, double life, double scale, int colour)
        {
            for (int i = 0; i < count; i++)
            {
                ref P p = ref parts[next];
                next = (next + 1) % parts.Length;
                p.pos = at;
                p.vel = SprayMath.UnitSphere(rng.Next(), rng.Next()) * (float)(speed * (1 - speedJitter * rng.Next()));
                p.life = p.maxLife = (float)SprayMath.Life(life, lifeJitter, rng.Next());
                p.scale = (float)scale;
                p.colour = colour;
                p.live = true;
                p.aim = false;
            }
        }

        static bool Mine(object payload) => payload is ShockwaveEvent e && e.owner == "player";

        /// <summary>gunFx.js :71-75: the double blast's spray from the muzzle (the web alternates edge and 0xfff0c0).</summary>
        void OnDoubleBlast(object payload)
        {
            if (!Mine(payload) || game.cam == null || !SpeedLines20.Live) return;
            Transform c = game.cam.transform;
            Vector3 aim = c.forward, at = Muzzle(c.position, aim), inherit = WebSpace.ToUnity(game.mover.velocity);
            for (int i = 0; i < 14; i++) Spray(at, aim, 0.5, i % 2 == 1 ? 1 : 2, 18, 0.25, 1.2, 0, inherit, aim: true);
        }

        /// <summary>gunFx.js :65-70: the cone wave's five bursts along the cone.</summary>
        void OnConeWave(object payload)
        {
            if (!Mine(payload) || !SpeedLines20.Live) return;
            var e = (ShockwaveEvent)payload;
            Vector3 o = WebSpace.ToUnity(e.origin), d = WebSpace.ToUnity(e.dir).normalized;
            for (int i = 0; i < 5; i++) Burst(SprayMath.ConeBurstAt(o, d, i), 5, 6, 0.3, 0.9, 1);
        }

        /// <summary>gunFx.js :77-84: the TORCH's flame (the player's own: its gun in hand is the TORCH).</summary>
        void OnFlame(object payload)
        {
            if (game.cam == null || game.loadout == null || game.loadout.Weapon != "torch") return;
            Transform c = game.cam.transform;
            Vector3 aim = c.forward, at = Muzzle(c.position, aim), inherit = WebSpace.ToUnity(game.mover.velocity);
            for (int i = 0; i < flameCount; i++) Spray(at, aim, flameSpread, flameColour++ % 3, flameSpeed, flameLife, flameScale, flameRise, inherit, aim: true);
        }

        /// <summary>gunFx.js :41: a fire wall laid (the player's): its line on the floor (Torch.cs snapped its ends), its life.</summary>
        void OnFireWall(object payload)
        {
            if (!(payload is FireWallEvent e) || e.owner != "player") return;
            int slot = -1;
            float oldest = -1;
            for (int i = 0; i < walls.Length; i++)
            {
                if (!walls[i].live)
                {
                    slot = i;
                    break;
                }
                if (walls[i].age > oldest)
                {
                    oldest = walls[i].age;
                    slot = i;
                }
            }
            ref Wall w = ref walls[slot];
            w.a = WebSpace.ToUnity(e.a);
            w.b = WebSpace.ToUnity(e.b);
            w.age = 0;
            w.life = (float)e.life;
            w.width = (float)e.width;
            w.carry = 0;
            w.live = true;
        }

        /// <summary>gunFx.js update :129-142: each live wall's flames licking up along its line (spray(a, flat, 0.3, colour, 1,
        /// 3 + 2 r, 0.45, flameScale, flameRise, no carry): the web's literals), at fireWallRate a second.</summary>
        void StepWalls(double dt)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                ref Wall w = ref walls[i];
                if (!w.live) continue;
                w.age += (float)dt;
                if (w.age >= w.life)
                {
                    w.live = false;
                    continue;
                }
                int n = SprayMath.WallEmits(ref w.carry, dt, wallRate);
                for (int k = 0; k < n; k++)
                {
                    Vector3 at = SprayMath.WallAt(w.a, w.b, rng.Next());
                    Vector3 up = SprayMath.WallUp(rng.Next(), rng.Next());
                    Spray(at, up, 0.3, flameColour++ % 3, SprayMath.WallSpeed(rng.Next()), 0.45, flameScale, flameRise, Vector3.zero, aim: false);
                }
            }
        }

        /// <summary>The port's floor band under each live wall: trace_06 laid flat along the line, as wide as the burn.</summary>
        bool DrawWallBands()
        {
            bool drew = false;
            for (int i = 0; i < walls.Length; i++)
            {
                ref Wall w = ref walls[i];
                if (!w.live) continue;
                float k = FireWall20.Alpha(w.age, w.life) * UnityTuning.FireWallBandAlpha;
                if (!(k > 0)) continue;
                Vector3 along = w.b - w.a;
                Vector3 side = Vector3.Cross(Vector3.up, along);
                if (side.sqrMagnitude < 1e-8f) continue;
                side = side.normalized * (w.width * UnityTuning.FireWallBandWidth);
                Vector3 mid = (w.a + w.b) * 0.5f + Vector3.up * UnityTuning.FireWallBandLift;
                trace.Quad(mid, side, along * (0.5f * UnityTuning.FireWallBandLength), bandColour, k);
                drew = true;
            }
            return drew;
        }

        /// <summary>Frame (world time): move, fade (particles.js update: no gravity for a spray), draw as streaks along their velocity.</summary>
        public void Render(double dt)
        {
            if (!trace.Ready || game.cam == null) return;
            float fdt = (float)dt;
            Camera cam = game.cam;
            Vector3 eye = cam.transform.position;
            StepWalls(dt);
            Ladder(0); // the colours warmed (the band's too) before the first draw
            trace.Begin();
            bool drew = DrawWallBands();
            for (int i = 0; i < parts.Length; i++)
            {
                ref P p = ref parts[i];
                if (!p.live) continue;
                p.life -= fdt;
                if (p.life <= 0)
                {
                    p.live = false;
                    continue;
                }
                p.pos += p.vel * fdt;
                float k = p.life / p.maxLife;
                if (p.aim) k *= CrossClearAt(cam, p.pos); // banned 16: the crosshair stays clear of the aim's sprays
                if (!(k > 0.002f)) continue;
                float w = size * p.scale * (p.life / p.maxLife); // particles.js: (life / maxLife) x scale x the cube's size
                Vector3 tail = p.pos - p.vel * UnityTuning.Spray20StreakTime;
                trace.Streak(tail, p.pos, Mathf.Max(w, 0.01f), eye, Ladder(p.colour) * UnityTuning.Spray20Glow, k);
                drew = true;
            }
            if (drew) trace.Draw();
        }

        /// <summary>An aim spray's brightness share at `at` (SprayMath.CrossClear on its screen distance from the centre).</summary>
        static float CrossClearAt(Camera cam, Vector3 at)
        {
            Vector3 sp = cam.WorldToScreenPoint(at);
            if (sp.z <= 0) return 1f; // behind the eye: not drawn over anything
            float dx = sp.x - cam.pixelWidth * 0.5f, dy = sp.y - cam.pixelHeight * 0.5f;
            return SprayMath.CrossClear(Mathf.Sqrt(dx * dx + dy * dy), cam.pixelHeight);
        }
    }
}
