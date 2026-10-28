// NIGHT74 builder B (leftover 3, design.n74.spring74, a VIEW row: NightRows.ViewOn(cfg, "spring74") EVERY frame, never `enabled` written here; off: -skygrindRows72 spring74:0, Rows72.cs). spring72's flinch
// (EnemyViews.Spring72.cs) read NOTHING OBVIOUS at hit + 1 on the close crops (NIGHT73 M5; THREADS T-159): it was a spring kicked by a VELOCITY, so its 17 % squash was reached about five frames after the hit, and
// crunch36's own 15 % pop (Crunch36Squash) is at its peak on the hit frame itself: on the frames a viewer looks at, the game's old pop was all there was. GG 2026-10-26 "animations ... I just haven't seen it";
// 2026-10-22 "boss animations are too rigid"; 2026-10-14 "hits and kills need to feel more satisfying".
// With the row ON a hit on an enemy body (the sim's health falling by at least Spring74MinShare of its maximum since the frame before, at least Spring74GapS after the last kick) SETS a second spring's
// displacement to -Spring74Squash (0.32 of the height; a graze less, down to 0.6 of it) on the frame the hit is drawn and HOLDS it there Spring74HoldS (0.03 s of frame time: the hit frame and the two after
// it, the strip's hit + 1 and + 2, at full depth: a free spring from the kick is already at -0.15 on hit + 1, which is why spring72's read as nothing); then the spring (Spring74Hz 9, z 0.4) lets it go through a
// stretch of about 8 % and is quiet by 0.2 s. The kick sets the displacement, it does not step it, so the hit frame shows the depth itself. The squash is VOLUME-PRESERVING (y by
// 1 + s, x and z by 1 / sqrt(1 + s)), on the feet, capped at +-Spring74Max, not drawn under REDUCED MOTION or EFFECTS 0 (the spring still runs), and multiplies with crunch36's pop, hitzone47's and
// spring72's: the body is about 58 % as tall and a third wider for the first frames. The same silhouette squashed (the pill language, CLAUDE.md rule 7: motion is builder B's, shapes are not).
// RENDER ONLY: no hit shape, no sim read for writing, no timer, no fingerprint input; frame time. THE SWITCH is read every frame (D1's contract: the spring runs the same with the row off, so the next frame on is
// the frame it would have drawn; only the application waits).
// Proof (L-014 / L-115; Player.log, row on or off): `[spring74] kick <n>: <kind> id <id>, hp <a> -> <b> (<p> % of its maximum), squash <x> set at frame <f> | row spring74 on|off | drawn yes|no` for the first
// Spring74KickLines kicks, and `[spring74] row spring74 on|off: <k> kicks, applied <a> body-frames, peak squash applied <p>, stretch <s> | volume product <lo>..<hi>` at a new run and the quit. The existing spring73
// reel's hit frames (its line 1 `telemetry frames f ...`) are the frames to crop: n74-squash.py measures the body's height there, ON against OFF.
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The pure maths of spring74 (no Unity object). EditMode: N74BSpringTests.</summary>
    public static class Spring74Math
    {
        /// <summary>The squash a hit sets (a positive share of the height; 0: not a hit): the health fell by `hpBefore - hpNow` of `maxHp`, `sinceKickS` after the last kick. A fall under Spring74MinShare of the
        /// maximum, or a kick inside Spring74GapS, is no hit; from the minimum share the squash is Spring74SmallMul of Spring74Squash, rising to all of it at Spring74BigShare.</summary>
        public static float Kick(double hpBefore, double hpNow, double maxHp, double sinceKickS)
        {
            if (double.IsNaN(hpBefore) || double.IsNaN(hpNow) || maxHp <= 0) return 0f;
            double share = (hpBefore - hpNow) / maxHp;
            if (share < UnityTuning.Spring74MinShare || sinceKickS < UnityTuning.Spring74GapS) return 0f;
            double t = (share - UnityTuning.Spring74MinShare) / System.Math.Max(1e-6, UnityTuning.Spring74BigShare - UnityTuning.Spring74MinShare);
            t = t < 0 ? 0 : t > 1 ? 1 : t;
            return (float)(UnityTuning.Spring74Squash * (UnityTuning.Spring74SmallMul + (1.0 - UnityTuning.Spring74SmallMul) * t));
        }

        /// <summary>The spring's displacement as the height's change drawn: held inside +-Spring74Max.</summary>
        public static float Applied(float x) => x < -UnityTuning.Spring74Max ? -UnityTuning.Spring74Max : x > UnityTuning.Spring74Max ? UnityTuning.Spring74Max : x;
    }

    /// <summary>What spring74 did since the last line (the log and the reels read it): the applied numbers are zero with the row off.</summary>
    public static class Spring74Probe
    {
        public static int Kicks, Bodies, Applied;
        public static float SquashPeak, StretchPeak, LastKickSquash;
        public static double ProductLo = 2, ProductHi = 0;
        public static bool On;
        public static void Reset() { Kicks = Bodies = Applied = 0; SquashPeak = StretchPeak = 0f; ProductLo = 2; ProductHi = 0; }
    }

    public sealed partial class EnemyLook
    {
        public const string Spring74Row = "spring74";
        bool wired74;

        /// <summary>From Spring72React, once a frame per body after its own springs: the hit squash spring (stepped first, then kicked), applied to `squash` when the row reads on and motion is allowed.</summary>
        void Spring74React(int id, S72 s, Enemy e, float dt, MovementConfig mc, ref Vector3 squash)
        {
            if (!wired74) Spring74Wire();
            bool on = NightRows.ViewOn(game.cfg, Spring74Row);
            Spring74Probe.On = on;
            float kick = Spring74Math.Kick(s.lastHp74, e.hp, e.maxHp, Time.time - s.lastKick74);
            double before = s.lastHp74;
            s.lastHp74 = e.hp;
            if (kick <= 0f)
            {
                if (s.hold74 > 0f) { s.hold74 -= dt; s.hit74X = -s.kick74; s.hit74V = 0f; } // the hold: the squash stays at its depth (hitstop-style) for Spring74HoldS of frame time
                else Spring72Math.Step(ref s.hit74X, ref s.hit74V, 0f, UnityTuning.Spring74Hz, UnityTuning.Spring74Zeta, dt); // then the spring lets it go
            }
            if (kick > 0f)
            {
                s.hit74X = -kick; // set, not stepped: the hit frame shows the depth itself
                s.hit74V = 0f;
                s.kick74 = kick;
                s.hold74 = UnityTuning.Spring74HoldS;
                s.lastKick74 = Time.time;
                Spring74Probe.Kicks++;
                Spring74Probe.LastKickSquash = kick;
                if (Spring74Probe.Kicks <= UnityTuning.Spring74KickLines)
                {
                    CultureInfo ci = CultureInfo.InvariantCulture;
                    Debug.Log("[spring74] kick " + Spring74Probe.Kicks + ": " + (e.kind ?? "?") + " id " + id + ", hp " + before.ToString("0.0", ci) + " -> " + e.hp.ToString("0.0", ci) + " ("
                        + (100.0 * (before - e.hp) / System.Math.Max(1e-6, e.maxHp)).ToString("0", ci) + " % of its maximum), squash " + kick.ToString("0.000", ci) + " set at frame " + Time.frameCount
                        + " | row spring74 " + (on ? "on" : "off") + " | drawn " + (on && FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity) ? "yes" : "no"));
                }
            }
            Spring74Probe.Bodies++;
            if (!on || !FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity)) return;
            float sy = Spring74Math.Applied(s.hit74X);
            if (System.Math.Abs(sy) < 1e-4f) return;
            float y = 1f + sy, xz = 1f / Mathf.Sqrt(y);
            squash = Vector3.Scale(squash, new Vector3(xz, y, xz));
            Spring74Probe.Applied++;
            if (-sy > Spring74Probe.SquashPeak) Spring74Probe.SquashPeak = -sy;
            if (sy > Spring74Probe.StretchPeak) Spring74Probe.StretchPeak = sy;
            double prod = (double)y * xz * xz;
            if (prod < Spring74Probe.ProductLo) Spring74Probe.ProductLo = prod;
            if (prod > Spring74Probe.ProductHi) Spring74Probe.ProductHi = prod;
        }

        void Spring74Wire()
        {
            wired74 = true;
            Application.quitting += () => { if (Spring74Probe.Kicks > 0 || Spring74Probe.Applied > 0) Spring74Say(); };
            game.OnNewRun += () => { if (Spring74Probe.Kicks > 0 || Spring74Probe.Applied > 0) Spring74Say(); Spring74Probe.Reset(); };
        }

        void Spring74Say()
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            Debug.Log("[spring74] row spring74 " + (Spring74Probe.On ? "on" : "off") + ": " + Spring74Probe.Kicks + " kicks, applied " + Spring74Probe.Applied + " body-frames of " + Spring74Probe.Bodies + ", peak squash applied "
                + Spring74Probe.SquashPeak.ToString("0.000", ci) + ", stretch " + Spring74Probe.StretchPeak.ToString("0.000", ci) + " | volume product "
                + (Spring74Probe.Applied > 0 ? Spring74Probe.ProductLo.ToString("0.0000", ci) + ".." + Spring74Probe.ProductHi.ToString("0.0000", ci) : "-"));
        }
    }
}
