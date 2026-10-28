// NIGHT20 row 6 (builder V): MovementFx textured (NIGHT19's banned pattern 13: "untextured quads as particles"), on the
// Kenney Particle Pack's sprites (CC0; FxSprites.cs): soft dust where the web throws cubes, soft streaks where it draws lines.
// - A landing: as before (the web's cubes and NIGHT16's ring), plus LandPuffs20Min..Max smoke_04 puffs at the feet, more and
//   bigger the harder the landing, blooming outward and settling (the drift slows: v e^-kt).
// - The slide: the heel dust's cubes as before, plus SlidePuffRate20 puffs a second at full speed, low and behind.
// - The wall-ride's streaks and the slide's lines: the same lines (effects.js's rule, the web's numbers) drawn as the soft
//   trace_06 streak instead of a flat quad.
// - The climb (builder N's ClimbEvent on the bus, N20Events.Climb): "start" (the tell) grit at the foot of the face; "up"
//   grit falling from the climber's hands for ClimbUpTime s (its position read from the registry while it lives); "top"
//   a mantle puff at the lip; "knocked" (a punch knocked it off) a burst and the dirt_01 debris; "land" puffs at the foot.
//   The dust is colors.slamDust (neutral: the climb's DANGER tell is N's view).
// The switch: with -skygrindOff20 (or a sprite not in the build) every piece takes NIGHT19's path exactly (Trail20 returns
// false and the caller adds its FxLines line; no puff is laid). Visual only; world time; its dice are MovementFx's own
// Mulberry32 (a render-side stream); pooled (Puffs20, Trails20, ClimbStreams20), no allocation per frame.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The textured movement FX's pure rules (Textured20Tests).</summary>
    public static class Textured20Math
    {
        static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;

        static double Smooth(double e0, double e1, double x)
        {
            double t = Clamp01((x - e0) / (e1 - e0));
            return t * t * (3 - 2 * t);
        }

        /// <summary>A puff's size share over its life: PuffGrowFrom to PuffGrowTo on an ease-out (1 - (1 - t)^2).</summary>
        public static double PuffScale(double t01)
        {
            double t = Clamp01(t01);
            return UnityTuning.PuffGrowFrom + (UnityTuning.PuffGrowTo - UnityTuning.PuffGrowFrom) * (1 - (1 - t) * (1 - t));
        }

        /// <summary>A puff's brightness share over its life: in over PuffFadeIn, out from PuffFadeOut.</summary>
        public static double PuffAlpha(double t01) =>
            Smooth(0, UnityTuning.PuffFadeIn, t01) * (1 - Smooth(UnityTuning.PuffFadeOut, 1, t01));

        /// <summary>m a puff has drifted at `t` s, leaving at `v0` m/s against PuffDrag: v0 / k (1 - e^-kt).</summary>
        public static double Drift(double v0, double t) => v0 / UnityTuning.PuffDrag * (1 - Math.Exp(-UnityTuning.PuffDrag * Math.Max(0, t)));

        /// <summary>Puffs on a landing of `strength01` (0 at the puff speed, 1 at LandFullFallSpeed or harder).</summary>
        public static int LandPuffs(double strength01) =>
            (int)Math.Round(UnityTuning.LandPuffs20Min + (UnityTuning.LandPuffs20Max - UnityTuning.LandPuffs20Min) * Clamp01(strength01));

        /// <summary>A landing puff's size (m): LandPuffSize20, x 1.6 at the hardest.</summary>
        public static double LandPuffSize(double strength01) => UnityTuning.LandPuffSize20 * (1 + 0.6 * Clamp01(strength01));

        /// <summary>The puffs a climb phase raises at once (N's ClimbEvent.phase).</summary>
        public static int ClimbPuffs(string phase) =>
            phase == "start" ? UnityTuning.ClimbStartPuffs
            : phase == "top" ? UnityTuning.ClimbTopPuffs
            : phase == "knocked" ? UnityTuning.ClimbKnockedPuffs
            : phase == "land" ? UnityTuning.ClimbLandPuffs
            : 0;

        /// <summary>Does the phase stream grit along the face (the climb itself)?</summary>
        public static bool ClimbStreams(string phase) => phase == "up";
    }

    public sealed partial class MovementFx
    {
        struct Puff20
        {
            public Vector3 origin, vel;
            public float size, age, life, roll, spin, peak;
            public bool dirt, live;
        }

        struct Trail20
        {
            public Vector3 a, b;
            public Color c;
            public float width, age, life;
            public bool live;
        }

        struct ClimbStream20
        {
            public int enemy;
            public Vector3 foot, lip;
            public float age;
            public double budget;
            public bool live;
        }

        readonly Puff20[] puffs20 = new Puff20[UnityTuning.Puffs20];
        readonly Trail20[] trails20 = new Trail20[UnityTuning.Trails20];
        readonly ClimbStream20[] climbs20 = new ClimbStream20[UnityTuning.ClimbStreams20];
        int nextPuff20, nextTrail20;
        FxSprites smoke20, dirt20, trace20;
        double slidePuffBudget20;
        bool warm20;

        /// <summary>Tonight's textured look is on (not the A side, the sprites in the build).</summary>
        bool Textured20 => SpeedLines20.Live && smoke20 != null && smoke20.Ready && trace20.Ready;

        void Textured20Build()
        {
            Material neon = game.mats.neon;
            smoke20 = new FxSprites(neon, UnityTuning.KenneySmoke, puffs20.Length);
            dirt20 = new FxSprites(neon, UnityTuning.KenneyDirt, puffs20.Length);
            trace20 = new FxSprites(neon, UnityTuning.KenneyTrace, trails20.Length);
            game.events.On(N20Events.Climb, OnClimb20);
        }

        void Textured20Clear()
        {
            for (int i = 0; i < puffs20.Length; i++) puffs20[i].live = false;
            for (int i = 0; i < trails20.Length; i++) trails20[i].live = false;
            for (int i = 0; i < climbs20.Length; i++) climbs20[i].live = false;
            slidePuffBudget20 = 0;
            windOn = false; // the wisps (MovementFx.Wind20.cs)
            windSlide = windAge = windLeft = 0;
        }

        /// <summary>One puff (Unity space) drifting along `vel` (m/s at birth), `size` m across, for `life` s.</summary>
        void Puff(Vector3 at, Vector3 vel, double size, double life, bool dirt = false)
        {
            ref Puff20 p = ref puffs20[nextPuff20];
            nextPuff20 = (nextPuff20 + 1) % puffs20.Length;
            p.origin = at;
            p.vel = vel;
            p.size = (float)(size * (0.85 + 0.3 * rng.Next()));
            p.age = 0;
            p.life = (float)(life * (0.85 + 0.3 * rng.Next()));
            p.roll = (float)(rng.Next() * Math.PI * 2);
            p.spin = (float)((rng.Next() * 2 - 1) * 1.2);
            p.peak = UnityTuning.PuffAlpha;
            p.dirt = dirt;
            p.live = true;
        }

        /// <summary>A flat random direction (Unity space) at `speed`, with a little rise.</summary>
        Vector3 Spread(double speed, double rise)
        {
            double a = rng.Next() * Math.PI * 2;
            return new Vector3((float)(Math.Cos(a) * speed), (float)(rise * (0.5 + rng.Next())), (float)(Math.Sin(a) * speed));
        }

        /// <summary>A landing's puffs at the feet (web space), `t` its strength (0..1).</summary>
        void LandPuff20(Vec3 feet, double t)
        {
            if (!Textured20) return;
            Vector3 at = WebSpace.ToUnity(feet) + Vector3.up * 0.15f;
            int n = Textured20Math.LandPuffs(t);
            double size = Textured20Math.LandPuffSize(t);
            for (int i = 0; i < n; i++) Puff(at, Spread(1.6 + 2.4 * t, 0.4), size, UnityTuning.LandPuffLife20);
        }

        /// <summary>The slide's heel puffs while sliding faster than a walk: `k` its speed's share, `heel` (Unity space), `back` (unit, behind).</summary>
        void SlidePuffs20(double dt, double k, Vector3 heel, Vector3 back)
        {
            if (!Textured20)
            {
                slidePuffBudget20 = 0;
                return;
            }
            slidePuffBudget20 += UnityTuning.SlidePuffRate20 * k * dt;
            while (slidePuffBudget20 >= 1)
            {
                slidePuffBudget20--;
                Vector3 side = Vector3.Cross(Vector3.up, back) * (float)((rng.Next() * 2 - 1) * 0.6);
                Puff(heel + Vector3.up * 0.1f, back * 1.2f + side + Vector3.up * 0.25f, UnityTuning.SlidePuffSize20, UnityTuning.SlidePuffLife20);
            }
        }

        /// <summary>A line (effects.js's addLine) as the soft trace streak; false on NIGHT19's path (the caller adds its line).</summary>
        bool Trail20Add(Vector3 a, Vector3 b, Color c, float life, float width)
        {
            if (!Textured20) return false;
            ref Trail20 t = ref trails20[nextTrail20];
            nextTrail20 = (nextTrail20 + 1) % trails20.Length;
            t.a = a;
            t.b = b;
            t.c = c * UnityTuning.TrailBrightness20;
            t.width = Math.Max(width, (float)UnityTuning.TrailWidth20);
            t.age = 0;
            t.life = life;
            t.live = true;
            return true;
        }

        /// <summary>N's climb on the bus: each phase's dust.</summary>
        void OnClimb20(object payload)
        {
            if (!(payload is ClimbEvent e) || !Textured20) return;
            Vector3 foot = WebSpace.ToUnity(e.foot), lip = WebSpace.ToUnity(e.lip);
            int n = Textured20Math.ClimbPuffs(e.phase);
            Vector3 at = e.phase == "top" ? lip : e.phase == "knocked" ? ClimberAt(e.enemy, Vector3.Lerp(foot, lip, 0.5f)) : foot;
            for (int i = 0; i < n; i++) Puff(at + Vector3.up * 0.1f, Spread(1.2, 0.5), UnityTuning.ClimbPuffSize, UnityTuning.ClimbPuffLife);
            if (e.phase == "knocked") Puff(at, Vector3.down * 1.5f, UnityTuning.ClimbDebrisSize, UnityTuning.ClimbPuffLife, true);
            if (Textured20Math.ClimbStreams(e.phase))
            {
                int slot = -1;
                for (int i = 0; i < climbs20.Length; i++)
                {
                    if (climbs20[i].live && climbs20[i].enemy == e.enemy) { slot = i; break; }
                    if (slot < 0 && !climbs20[i].live) slot = i;
                }
                if (slot < 0) return;
                climbs20[slot] = new ClimbStream20 { enemy = e.enemy, foot = foot, lip = lip, age = 0, budget = 0, live = true };
            }
            else if (e.phase == "top" || e.phase == "knocked" || e.phase == "land")
            {
                for (int i = 0; i < climbs20.Length; i++)
                    if (climbs20[i].live && climbs20[i].enemy == e.enemy) climbs20[i].live = false;
            }
        }

        /// <summary>A climber's hands now (its interpolated position + its height, Unity space), or `fallback`.</summary>
        Vector3 ClimberAt(int id, Vector3 fallback)
        {
            EnemyRegistry reg = game.enemies;
            if (reg == null) return fallback;
            var all = reg.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy en = all[i];
                if (en.id != id) continue;
                if (!en.alive) return fallback;
                Vec3 p = en.previous + (en.position - en.previous) * game.Alpha;
                return WebSpace.ToUnity(p) + Vector3.up * (float)en.Height;
            }
            return fallback;
        }

        /// <summary>Frame (world time): the climbs' grit, the puffs' bloom and fade, the trails' fade; drawn.</summary>
        void Textured20Render(double dt)
        {
            if (smoke20 == null || !smoke20.Ready || !trace20.Ready) return;
            if (!warm20 && UiTheme.Ready)
            {
                warm20 = true; // the dust's and the streaks' materials made before their first use
                smoke20.Warm(dust);
                dirt20.Warm(dust);
                trace20.Warm(streakColor * UnityTuning.TrailBrightness20); // the wall-ride's
                trace20.Warm(usable * UnityTuning.SlideLineBrightness * UnityTuning.TrailBrightness20); // the slide's
                trace20.Warm(UiTheme.CyanPale); // the long slide's wisps (MovementFx.Wind20.cs)
            }
            Camera view = game.cam;
            if (view == null) return;
            float fdt = (float)dt;
            // the climbs: grit from the hands, falling along the face
            for (int i = 0; i < climbs20.Length; i++)
            {
                ref ClimbStream20 c = ref climbs20[i];
                if (!c.live) continue;
                c.age += fdt;
                if (c.age >= UnityTuning.ClimbUpTime)
                {
                    c.live = false;
                    continue;
                }
                c.budget += UnityTuning.ClimbUpRate * dt;
                while (c.budget >= 1)
                {
                    c.budget--;
                    Vector3 hands = ClimberAt(c.enemy, Vector3.Lerp(c.foot, c.lip, (float)(c.age / UnityTuning.ClimbUpTime)));
                    Puff(hands, Vector3.down * 1.8f + Spread(0.4, 0), UnityTuning.ClimbPuffSize * 0.6, UnityTuning.ClimbPuffLife);
                }
            }
            SettingsConfig st = game.cfg.movement.settings;
            float intensity = (float)Math.Max(0, Math.Min(game.cfg.movement.feel.effectsIntensityMax, st.effectsIntensity));
            smoke20.Begin();
            dirt20.Begin();
            trace20.Begin();
            Transform camT = view.transform;
            Vector3 eye = camT.position;
            bool drew = false;
            int live = 0;
            Bounds bounds = default;
            for (int i = 0; i < puffs20.Length; i++)
            {
                ref Puff20 p = ref puffs20[i];
                if (!p.live) continue;
                p.age += fdt;
                if (p.age >= p.life)
                {
                    p.live = false;
                    continue;
                }
                double t = p.age / p.life;
                Vector3 at = p.origin + p.vel * (float)Textured20Math.Drift(1, p.age);
                float k = (float)(p.peak * Textured20Math.PuffAlpha(t)) * intensity;
                if (k <= 0) continue;
                float size = (float)(p.size * Textured20Math.PuffScale(t));
                (p.dirt ? dirt20 : smoke20).Billboard(at, size, p.roll + p.spin * p.age, camT, dust, k);
                drew = true;
                if (live++ == 0) bounds = new Bounds(at, Vector3.one * size);
                else bounds.Encapsulate(new Bounds(at, Vector3.one * size));
            }
            for (int i = 0; i < trails20.Length; i++)
            {
                ref Trail20 tr = ref trails20[i];
                if (!tr.live) continue;
                tr.age += fdt;
                if (tr.age >= tr.life)
                {
                    tr.live = false;
                    continue;
                }
                float k = (1 - tr.age / tr.life) * intensity; // effects.js :277-288: colour x life / maxLife
                trace20.Streak(tr.a, tr.b, tr.width, eye, tr.c, k);
                drew = true;
            }
            if (live > 0 && FeelProbe.Recording) FeelProbe.EffectBounds(FeelFx.Sparks, bounds, UnityTuning.PuffAlpha, live); // the reel's movement dust
            if (SpeedLines20.Live && Wind20Frame(dt, eye, intensity)) drew = true; // the long slide's wisps (MovementFx.Wind20.cs)
            if (!drew) return;
            smoke20.Draw();
            dirt20.Draw();
            trace20.Draw();
        }
    }
}
