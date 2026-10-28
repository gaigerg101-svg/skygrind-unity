// NIGHT48 builder F (design.n48.blast48, a view row over fx47; NIGHT48.md G20: "the a747 blast contact sheet off / A "ring" / B "lance", husks
// frozen at 3 / 5 / 7 m, the wash meter GREEN (no frame's reticle box > +0.25 over f-2), and the blind pair A vs B run; the winner ON").
// n47-r3's a747 (Logs/player/n47-reel-n47-r3-a747s.log) washed 6 frames (A: f3 +0.30, f4 +0.27, f18 +0.34; B: f4 +0.26, f18 +0.26, f30 +0.35).
// Read in code tonight, two of the blast's own parts reach the reticle's centre box:
//   - the AIM RING (both variants) and the LANCE's streaks (B) stand off the aim by Fx47AimRingClear 0.12 x their distance, and the ring sprite's
//     inner edge is 0.857 of its radius (KenneyRingMid's band 0.54-0.72): 0.103 x the distance; the centre box (10 % of the width and the
//     height) is +-0.134 x the distance across at a 74 deg field of view (16:9), so the ring's band crossed the box's sides; and both were drawn
//     on the aim AT THE PRESS while the camera kicked 3.4 deg up after it, so the ring slid down across the box at f2-f6;
//   - a launch36 BURST shell (two thrown bodies meeting) is a sphere up to 20 deg wide (Fx47ShellMaxShare) at 4-6 m, right on the aim line the
//     bodies fly down: f18 / f30's wash (shells 1-2 on those frames).
// With blast48 on: the ring and the streaks stand off by Blast48AimRingClear (their inner edge outside the box's corners) round the camera's
// CURRENT forward, the blast's own kick included (they stay round the reticle as the view kicks); a burst shell whose silhouette covers the
// screen's centre is drawn at Blast48ShellCentreAlpha (its rim, sparks and chunks still read). The variant: design.n48.blast48.numbers.variant
// (0 A "ring", 1 B "lance": the lead sets it from the blind pair) unless the a748 reel drives Fx47Probe.Variant. Off: fx47's blast as NIGHT47.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>blast48's switch (a748 forces it per pass) and its picked variant.</summary>
    public static class Blast48
    {
        public static bool? Force;
        /// <summary>The a748 reel drives the variant through Fx47Probe.Variant (true while it runs).</summary>
        public static bool ReelVariant;
        static GameConfig readFor;
        static bool on;
        static int variant;

        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            Read(cfg);
            return on;
        }

        /// <summary>The variant to draw: the reel's, else design.n48.blast48.numbers.variant (0 when the number is missing).</summary>
        public static int Variant(GameConfig cfg)
        {
            if (ReelVariant) return Fx47Probe.Variant;
            Read(cfg);
            return variant;
        }

        static void Read(GameConfig cfg)
        {
            if (cfg == null || ReferenceEquals(cfg, readFor)) return;
            readFor = cfg;
            on = NightRows.ViewOn(cfg, "blast48");
            variant = cfg.Has("design.n48.blast48.numbers.variant") ? (int)cfg.Num("design.n48.blast48.numbers.variant") : 0;
        }

        /// <summary>A burst shell's alpha factor: Blast48ShellCentreAlpha when its silhouette (radius `r` at `at`) covers the screen's centre
        /// box seen from `cam`, else 1.</summary>
        public static float ShellCentreK(Camera cam, Vector3 at, float r)
        {
            if (cam == null) return 1f;
            Transform t = cam.transform;
            Vector3 to = at - t.position;
            float d = to.magnitude;
            if (d < 1e-3f) return UnityTuning.Blast48ShellCentreAlpha;
            float ang = Vector3.Angle(t.forward, to) * Mathf.Deg2Rad;
            float sil = Mathf.Asin(Mathf.Clamp01(r / d));
            float box = Mathf.Atan(UnityTuning.Blast48BoxHalfTan * Mathf.Max(1f, cam.aspect / (16f / 9f)));
            return ang < sil + box ? UnityTuning.Blast48ShellCentreAlpha : 1f;
        }
    }

    /// <summary>guns48's switch (G24: per-gun weight on the ladder, no gun lighter than today: DECIDE47 D-14). Round 1 is the meter (a748's
    /// sheets of every action, L-001: measure first); the per-gun changes follow from its numbers.</summary>
    public static class Guns48
    {
        public static bool? Force;
        /// <summary>For the a748 line: guns48's FOV punch added this frame (deg).</summary>
        public static float FovNow;
        static GameConfig readFor;
        static bool on;

        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor)) { readFor = cfg; on = NightRows.ViewOn(cfg, "guns48"); }
            return on;
        }
    }

    public sealed partial class Fx47View
    {
        /// <summary>The aim axis the ring and the lance stand round: fx47's (the aim at the press) or blast48's (the camera's forward now, after
        /// the blast's own kick `kickDeg` this frame).</summary>
        void Blast48Axis(Transform ct, bool b48, float kickDeg, out Vector3 o, out Vector3 d, out float clear)
        {
            if (!b48)
            {
                o = eye0;
                d = aim0;
                clear = UnityTuning.Fx47AimRingClear;
                return;
            }
            o = ct.position;
            d = (ct.rotation * Quaternion.Euler(-kickDeg, 0, 0)) * Vector3.forward;
            clear = UnityTuning.Blast48AimRingClear;
        }

        /// <summary>Round 2 (n48-r1 a748: the ON passes' one wash frame, f4 +0.33 / +0.35 with no halo and no shell: the frame shows the PUNCH AURA,
        /// the three-tone spheres and the 2.5x halo disc, on the 3 m body's hit point in the reticle's box): with blast48 on, a gauntlet hit while
        /// the blast's first Blast48NoAuraS run draws no aura (the blast's own picture, the shockwave and the throw, is its read).</summary>
        bool Blast48NoAura(HitEvent e) => e.weapon == "gauntlet" && age >= 0 && age < UnityTuning.Blast48NoAuraS && Blast48.On(game.cfg);

        /// <summary>The auras spawned since the last draw (age 0: this step's) freed when the blast's event comes (its hits are emitted first).</summary>
        void Blast48DropAuras()
        {
            if (!Blast48.On(game.cfg)) return;
            for (int i = 0; i < auras.Length; i++)
                if (auras[i].age == 0 && auras[i].pink) auras[i].age = -1;
        }

        /// <summary>The blast's camera kick at age `t` (the same curve as Render's), so blast48 can stand its ring round the view it will draw.</summary>
        float BlastKick48(float t)
        {
            if (Calm() || t < 0) return 0f;
            float up = Mathf.Clamp01(t / UnityTuning.Fx47KickRiseS);
            float down = Mathf.Clamp01((t - UnityTuning.Fx47KickRiseS) / UnityTuning.Fx47KickSettleS);
            float ei = (float)System.Math.Max(0, System.Math.Min(1, game.cfg.movement.settings.effectsIntensity));
            return UnityTuning.Fx47KickDeg * up * (1f - down) * (1f - down) * ei;
        }
    }
}
