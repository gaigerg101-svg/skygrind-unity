// NIGHT18 U3-31.4 round 1 fix (builder R; build 3's reviewers saw the shove, but at the hit point "only the white flash"
// next to NIGHT17's small orange chip): a crisp impact at the bullet's hit point on the hit's own frame, FeelAB on.
// - A flare: a white-hot diamond facing the camera, in front of the struck surface, full on the hit's frame, then
//   shrinking to nothing (gone by 80 ms); x FLASH INTENSITY (0: no flare). A second pellet on the same enemy in the same
//   frame adds a chip, not a second flare.
// - 3-6 chips (HitSparkMath.ChipCount of the hit's scale): hard-edged cubes thrown up and off the struck side in the
//   screen plane (a fan leaning toward where the shot landed across the body; a centred shot: a symmetric fan), with a
//   share along the shot, never toward the camera; white-hot on their first drawn frame, then the enemy's own red-orange
//   (colors.enemy); tumbling, falling, full-size until 60 % of their 0.15-0.25 s, then shrinking to nothing.
// - Sizes and speeds are 12 m's, x the hit's RangeScale (the same screen size from 3 m to 24 m, as the shove). A hit's
//   spark stays well under a kill's burst: no ring, no body blocks, a 19 px flare at 12 m against a 113 px body.
// - Drawn as two CubeBatch meshes (white, red-orange) through Graphics.RenderMesh on copies of the neon material (the
//   web-hit Particles' path the Player draws; never the instanced one). Reported to the reel as FeelFx.HitFlash. EFFECTS 0:
//   none. REDUCED MOTION keeps them (they are not camera motion). Frame time: they play through a hit's 0-2 held steps.
// Pooled, fixed arrays, hashed variety (no UnityEngine.Random, view only): no allocation per frame.
// NIGHT19 U3-33.8 (builder V; NIGHT19 banned pattern 13, untextured quads as particles): textured from the Kenney Particle
// Pack (CC0, FxSprites.cs): the flare is a sharp four-point star (star_06) facing the camera, HitStarScale x the diamond's
// size, turned by a hash per hit (its thin rays carry less light than the solid diamond did: the kill's frame-1 lead, M1,
// only grows); each chip is a soft streak (trace_06) along its flight, HitChipStreak s of travel long, white-hot on its
// first drawn frames, then the enemy's red-orange. The same pools, timings, fan and probe; the cubes stay the fallback
// while the sprites are not in the build.
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>The hit spark's pure rules (the chip count, the fan, the flare's and a chip's size over time).</summary>
    public static class HitSparkMath
    {
        /// <summary>The chips a hit of `scale` (HitReactBody.Scale) throws: HitSparkChips x it within [Min, Max].</summary>
        public static int ChipCount(float scale)
        {
            if (!(scale > 0)) return UnityTuning.HitSparkChipsMin;
            int n = (int)Math.Round(UnityTuning.HitSparkChips * Math.Min(scale, 100f));
            return Math.Max(UnityTuning.HitSparkChipsMin, Math.Min(UnityTuning.HitSparkChipsMax, n));
        }

        /// <summary>
        /// Chip `k` of `n`'s angle in the screen plane (deg from the shooter's right toward up): the fan, centred straight up,
        /// turned HitSparkFanBias toward the struck `side` (+1 right, -1 left, 0 centred), spread evenly over HitSparkFanSpread,
        /// +- HitSparkFanJitter / 2 by `jitter01`, never under HitSparkFanMinDeg above the horizon on either side.
        /// </summary>
        public static float ChipAngleDeg(int k, int n, int side, float jitter01)
        {
            float centre = 90f - Math.Sign(side) * UnityTuning.HitSparkFanBias;
            float u = n > 1 ? (float)k / (n - 1) : 0.5f;
            float a = centre + (u - 0.5f) * UnityTuning.HitSparkFanSpread + (jitter01 - 0.5f) * UnityTuning.HitSparkFanJitter;
            return Math.Max(UnityTuning.HitSparkFanMinDeg, Math.Min(180f - UnityTuning.HitSparkFanMinDeg, a));
        }

        /// <summary>A chip's throw (unit): the screen-plane angle on (`right`, `up`), HitSparkChipAway of it along `away` (the shot).</summary>
        public static Vector3 ChipDir(float angleDeg, Vector3 right, Vector3 up, Vector3 away)
        {
            float r = angleDeg * Mathf.Deg2Rad;
            Vector3 s = right * (float)Math.Cos(r) + up * (float)Math.Sin(r);
            return (s * (1 - UnityTuning.HitSparkChipAway) + away * UnityTuning.HitSparkChipAway).normalized;
        }

        /// <summary>The flare's size share at `age` s: 1 through HitSparkFlareHold, then down to 0 over HitSparkFlareFade.</summary>
        public static float FlareShare(float age)
        {
            if (age <= UnityTuning.HitSparkFlareHold) return 1;
            if (age >= UnityTuning.HitSparkFlareHold + UnityTuning.HitSparkFlareFade) return 0;
            return 1 - (age - UnityTuning.HitSparkFlareHold) / UnityTuning.HitSparkFlareFade;
        }

        /// <summary>A chip's size share at `age` of `life` s: 1 until HitSparkChipShrinkFrom of it, then down to 0 at `life`.</summary>
        public static float ChipShare(float age, float life)
        {
            if (!(life > 0) || age >= life) return 0;
            float from = life * UnityTuning.HitSparkChipShrinkFrom;
            return age <= from ? 1 : (life - age) / (life - from);
        }
    }

    /// <summary>The hit sparks (EnemyLook owns one; FeelAB on spawns them from OnHit): pooled flares and chips, two RenderMesh draws.</summary>
    public sealed class HitSparks
    {
        struct Flare
        {
            public Vector3 at;
            public float age, size, roll;
            public int enemy;
            public bool alive;
        }

        struct Chip
        {
            public Vector3 pos, vel, axis;
            public float age, life, size, gravity;
            public int drawn;
            public bool alive;
        }

        readonly Flare[] flares = new Flare[UnityTuning.HitSparkFlarePool];
        readonly Chip[] chips = new Chip[UnityTuning.HitSparkChipPool];
        int nextFlare, nextChip;
        readonly CubeBatch white, red;
        readonly RenderParams whiteRp, redRp;
        uint hash = 2463534242;
        bool warmed;
        readonly FxSprites stars, streaks; // NIGHT19: the textured look (Ready false: the cubes)
        readonly Color starColor, whiteHot, enemyHot;

        readonly SurvivalGame game; // NIGHT49 X fx49: the row's reader

        public HitSparks(SurvivalGame game)
        {
            this.game = game;
            stars = new FxSprites(game.mats.neon, UnityTuning.KenneyStar, UnityTuning.HitSparkFlarePool);
            streaks = new FxSprites(game.mats.neon, UnityTuning.KenneyTrace, UnityTuning.HitSparkChipPool);
            starColor = Color.white * UnityTuning.HitStarHdr;
            whiteHot = Color.white * UnityTuning.HitChipHdr;
            enemyHot = Look.C(game.cfg, "colors.enemy") * UnityTuning.HitChipHdr; // the enemy's own red-orange
            stars.Warm(starColor);
            streaks.Warm(whiteHot);
            streaks.Warm(enemyHot);
            var wm = new Material(game.mats.neon) { name = "Hit spark white" };
            wm.SetColor("_BaseColor", Color.white * UnityTuning.HitSparkGlow);
            wm.SetFloat("_Cull", 0); // both faces
            var rm = new Material(game.mats.neon) { name = "Hit spark chips" };
            rm.SetColor("_BaseColor", Look.C(game.cfg, "colors.enemy") * UnityTuning.HitSparkGlow); // the enemy's own red-orange
            rm.SetFloat("_Cull", 0);
            white = new CubeBatch(UnityTuning.HitSparkFlarePool + UnityTuning.HitSparkChipPool, "Hit spark white");
            red = new CubeBatch(UnityTuning.HitSparkChipPool, "Hit spark chips");
            whiteRp = new RenderParams(wm) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            redRp = new RenderParams(rm) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
        }

        float H()
        {
            hash ^= hash << 13;
            hash ^= hash >> 17;
            hash ^= hash << 5;
            return (hash & 0xffffff) / (float)0x1000000;
        }

        /// <summary>Every spark goes (TITLE, a new run).</summary>
        public void Clear()
        {
            for (int i = 0; i < flares.Length; i++) flares[i].alive = false;
            for (int i = 0; i < chips.Length; i++) chips[i].alive = false;
        }

        /// <summary>
        /// A hit on enemy `enemy` at `at` (Unity space): `shot` the eye-to-hit axis (unit), `away` the level shot (unit), `right`
        /// the shooter's right (unit), `struck` the side it landed on (+1 right, -1 left, 0 centred), `scale` the hit's size,
        /// `range` its RangeScale, `flash` FLASH INTENSITY (0-1: the flare's size; 0 none).
        /// </summary>
        public void Spawn(int enemy, Vector3 at, Vector3 shot, Vector3 away, Vector3 right, int struck, float scale, float range, float flash)
        {
            if (game != null && Fx49.On(game.cfg)) return; // NIGHT49 X fx49: the hit's picture is the kit's one burst (this white star + red chips was one of four)
            bool flare = flash > 0;
            for (int i = 0; i < flares.Length && flare; i++)
                if (flares[i].alive && flares[i].enemy == enemy && flares[i].age == 0) flare = false; // a pellet of this frame's shot
            if (flare)
            {
                float fs = UnityTuning.HitSparkFlareSize * range * flash;
                flares[nextFlare] = new Flare { at = at - shot * (fs * 0.5f), size = fs, enemy = enemy, alive = true, roll = (H() - 0.5f) * 0.5f };
                nextFlare = (nextFlare + 1) % flares.Length;
            }
            int n = flare ? HitSparkMath.ChipCount(scale) : UnityTuning.HitSparkExtraChips;
            float size = UnityTuning.HitSparkChipSize * range;
            Vector3 from = at - shot * size;
            for (int k = 0; k < n; k++)
            {
                Vector3 d = HitSparkMath.ChipDir(HitSparkMath.ChipAngleDeg(k, n, struck, H()), right, Vector3.up, away);
                float speed = UnityTuning.HitSparkChipSpeed * range * (1 + (H() * 2 - 1) * UnityTuning.HitSparkChipSpeedJitter);
                Vector3 axis = new Vector3(H() * 2 - 1, H() * 2 - 1, H() * 2 - 1);
                chips[nextChip] = new Chip
                {
                    pos = from, vel = d * speed, axis = axis.sqrMagnitude > 1e-4f ? axis.normalized : Vector3.up,
                    life = UnityTuning.HitSparkChipLife + (H() * 2 - 1) * UnityTuning.HitSparkChipLifeJitter,
                    size = size, gravity = UnityTuning.HitSparkChipGravity * range, alive = true,
                };
                nextChip = (nextChip + 1) % chips.Length;
            }
        }

        /// <summary>Once a frame: age by `dt` (frame time; 0 while paused), move, and draw (two RenderMesh calls), reporting HitFlash.</summary>
        public void Frame(float dt, Camera cam)
        {
            if (cam == null) return;
            white.Begin();
            red.Begin();
            if (!warmed)
            {
                // the path's pipeline state (neon copies, both faces, a positions-only mesh) made on the first frame, not the first hit
                warmed = true;
                white.Add(CrunchWarm.Point(cam), UnityTuning.CrunchWarmScale);
                white.Draw(whiteRp);
                white.Begin();
                red.Add(CrunchWarm.Point(cam), UnityTuning.CrunchWarmScale);
                red.Draw(redRp);
                red.Begin();
            }
            Quaternion facing = cam.transform.rotation * Quaternion.Euler(0, 0, 45); // a diamond, face on
            bool sprites = stars.Ready && streaks.Ready; // NIGHT19: the textured look
            if (sprites)
            {
                stars.Begin();
                streaks.Begin();
            }
            Vector3 eye = cam.transform.position;
            for (int i = 0; i < flares.Length; i++)
            {
                if (!flares[i].alive) continue;
                flares[i].age += dt;
                float s = flares[i].size * HitSparkMath.FlareShare(flares[i].age);
                if (!(s > 0))
                {
                    flares[i].alive = false;
                    continue;
                }
                if (sprites) stars.Billboard(flares[i].at, s * UnityTuning.HitStarScale, flares[i].roll, cam.transform, starColor, 1);
                else white.Add(Matrix4x4.TRS(flares[i].at, facing, new Vector3(s, s, s * UnityTuning.HitSparkFlareDepth)));
                FeelProbe.EffectWorld(FeelFx.HitFlash, flares[i].at, s * 0.5f);
            }
            for (int i = 0; i < chips.Length; i++)
            {
                if (!chips[i].alive) continue;
                Chip c = chips[i];
                c.age += dt;
                if (c.age >= c.life)
                {
                    chips[i].alive = false;
                    continue;
                }
                c.vel.y -= c.gravity * dt;
                c.pos += c.vel * dt;
                float s = c.size * HitSparkMath.ChipShare(c.age, c.life);
                if (s > 0)
                {
                    c.drawn++;
                    bool hot = c.drawn <= UnityTuning.HitSparkChipWhiteFrames;
                    if (sprites)
                    {
                        // a streak along its flight: HitChipStreak s of travel, at least two chip sizes
                        float speed = c.vel.magnitude;
                        Vector3 along = speed > 1e-4f ? c.vel / speed : Vector3.up;
                        float len = Mathf.Max(2 * s, speed * UnityTuning.HitChipStreak);
                        streaks.Streak(c.pos - along * len, c.pos, s * UnityTuning.HitChipStreakWidth, eye, hot ? whiteHot : enemyHot, 1);
                    }
                    else
                    {
                        Matrix4x4 m = Matrix4x4.TRS(c.pos, Quaternion.AngleAxis(UnityTuning.HitSparkChipSpin * c.age, c.axis), new Vector3(s, s, s));
                        (hot ? white : red).Add(m);
                    }
                    FeelProbe.EffectWorld(FeelFx.HitFlash, c.pos, s * 0.5f);
                }
                chips[i] = c;
            }
            white.Draw(whiteRp);
            red.Draw(redRp);
            if (sprites)
            {
                stars.Draw();
                streaks.Draw();
            }
        }
    }
}
