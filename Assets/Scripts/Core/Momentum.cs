// Port of skygrind/src/combat/momentum.js (OVERNIGHT2 N2), NIGHT9 U3-09: momentum for one owner's loadout: how fast you
// are closing on what you hit, and how hard you just accelerated. Fixed step: it keeps a 60 ms smoothed velocity and a
// 0.35 s ring of "gain" speeds (horizontal speed plus any upward speed, so a dash, a standing slide, a boost jump, a slam
// rebound or a grapple release is a surge, but falling is not). Pure math in MomentumMath (momentumMath.js, golden).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Momentum : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot (the loadout's "momentum" part): the smoothed velocity, the gain ring and its
        // place, this step's gain, the last aim read, the switch and a debug force.
        public void Save(StateWriter w)
        {
            w.V("smooth", smooth);
            w.Doubles("ring", ring);
            w.I("at", at);
            w.I("filled", filled);
            w.D("gain", gain);
            w.D("aimM", aimM);
            w.B("enabled", enabled);
            w.B("forced", force.HasValue);
            w.D("force", force ?? 0);
        }

        public void Load(StateReader r)
        {
            List<double> rg = r.Doubles("ring");
            if (rg.Count != ring.Length) throw new StateException($"snapshot: momentum ring of {rg.Count}, this config's {ring.Length}");
            smooth = r.V("smooth");
            rg.CopyTo(ring);
            at = r.Int("at");
            filled = r.Int("filled");
            gain = r.D("gain");
            aimM = r.D("aimM");
            enabled = r.B("enabled");
            force = r.B("forced") ? r.D("force") : (double?)null;
        }

        readonly MomentumParams m;
        readonly Mover mover;
        Vec3 smooth;
        readonly int size;
        readonly double[] ring;
        int at;
        int filled;
        double gain; // this step's gain speed
        double aimM; // M along the aim (the crosshair arc), frame-read

        /// <summary>Enabled per loadout (the player's; RIVAL's stays off).</summary>
        public bool enabled = true;
        /// <summary>Debug (presets, `__sky`): a fixed M for every hit and trigger pull, or null.</summary>
        public double? force;

        public Momentum(GameConfig cfg, Mover mover)
        {
            m = MomentumParams.From(cfg);
            this.mover = mover;
            size = (int)JsMath.Max(1, JsMath.Round(cfg.Num("momentum.surgeWindow") * cfg.movement.loop.simHz));
            ring = new double[size];
        }

        public MomentumParams Params => m;

        static double GainSpeed(Vec3 v) => JsMath.Hypot(v.x, v.z) + JsMath.Max(0, v.y);

        /// <summary>The surge now: the gain over the ring (0 until the ring has a sample).</summary>
        double Surge() => filled == 0 ? 0 : MomentumMath.SurgeOf(ring, filled < size ? filled : size, gain);

        /// <summary>One fixed step: smooth the velocity, remember the gain speed.</summary>
        public void Step(double dt)
        {
            MomentumMath.SmoothVelocity(ref smooth, mover.velocity, dt, m.smoothTime);
            gain = GainSpeed(mover.velocity);
            ring[at] = gain;
            at = (at + 1) % size;
            filled = Math.Min(size, filled + 1);
        }

        /// <summary>Momentum 0..1 for a hit from `from` on a point `to`.</summary>
        public double At(Vec3 from, Vec3 to)
        {
            if (!enabled) return 0;
            if (force.HasValue) return force.Value;
            return MomentumMath.MomentumOf(MomentumMath.ClosingSpeed(smooth, from, to), Surge(), m);
        }

        /// <summary>Momentum 0..1 along a unit aim direction from `origin` (the crosshair arc, a weapon's momentum variant).</summary>
        public double Along(Vec3 origin, Vec3 dir)
        {
            if (!enabled) return 0;
            if (force.HasValue) return aimM = force.Value;
            var eye = new Vec3(origin.x, origin.y, origin.z);
            var ahead = new Vec3(origin.x + dir.x, origin.y + dir.y, origin.z + dir.z);
            aimM = MomentumMath.MomentumOf(MomentumMath.ClosingSpeed(smooth, eye, ahead), Surge(), m);
            return aimM;
        }

        /// <summary>The last `Along` result (the HUD arc).</summary>
        public double Aim => aimM;

        public bool Surging => Surge() > m.surgeMin;

        /// <summary>Debug: the smoothed velocity and the surge.</summary>
        public (Vec3 smooth, double surge, double gain) DebugState() => (smooth, Surge(), gain);

        /// <summary>A respawn / new run.</summary>
        public void Reset()
        {
            force = null;
            smooth = default;
            Array.Clear(ring, 0, ring.Length);
            at = 0;
            filled = 0;
            gain = 0;
            aimM = 0;
        }
    }
}
