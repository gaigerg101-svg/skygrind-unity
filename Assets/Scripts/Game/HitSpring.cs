// NIGHT18 U3-31.4 (builder R, the hit that lands; M5): a view-only damped spring a body hit kicks, and one hit slot's body
// reaction built on it (EnemyViews.Hit.cs reads it while FeelAB is on; off, NIGHT17's flinch draws).
// - HitSpring: Source's view punch (x'' = -k x - c x'; a hit adds velocity, not position, so the body moves on the hit's own
//   frame and overshoots once), advanced by the exact solution, so the path is the same at 60, 144 or 240 fps (the reel
//   captures at a fixed 60). Underdamped only (k > c^2 / 4; UnityTuning.HitReact's numbers are).
// - HitReactBody: two springs, a sideways kick along the shooter's right (m, signed; the view leans the top with it) and a
//   squash (a share of the height; the overshoot is the stretch), plus a stream's hold (0-1): hits on later frames inside
//   HitReactStreamWindow move both springs' rest out, so a fast gun holds the body back on one side instead of jerking
//   it; pellets in one step stack the kick up to the cap. REDUCED MOTION (and EFFECTS 0) never kick it. Its sizes are 12 m's;
//   pass 2: the view draws them x RangeScale(distance) (distance / 12 within [0.25, 2]) so the screen size holds.
// Pure value types, no allocation, no Unity calls (tested under EditMode and a CoreCLR runner).
using System;

namespace Skygrind.Game
{
    /// <summary>One axis of a view-only damped spring (Source's view punch: x'' = -k x - c x'; a hit adds velocity).</summary>
    public struct HitSpring
    {
        /// <summary>The displacement from rest and its rate (units per s).</summary>
        public float x, v;

        /// <summary>Close enough to rest to stop integrating (HitReactRestEps on both).</summary>
        public bool Resting => Math.Abs(x) < UnityTuning.HitReactRestEps && Math.Abs(v) < UnityTuning.HitReactRestEps;

        /// <summary>The damped angular rate (rad/s); a tiny floor keeps a (not used) critically damped pair finite.</summary>
        static double Wd(float k, float c)
        {
            double s = c * 0.5, w2 = k - s * s;
            return w2 > 1e-6 ? Math.Sqrt(w2) : 1e-3;
        }

        /// <summary>When a kick from rest peaks (s).</summary>
        public static float PeakTime(float k, float c)
        {
            double wd = Wd(k, c);
            return (float)(Math.Atan2(wd, c * 0.5) / wd);
        }

        /// <summary>The peak a unit velocity kick from rest reaches (units per unit/s): a kick of peak / this peaks at `peak`.</summary>
        public static float PeakPerVelocity(float k, float c)
        {
            double wd = Wd(k, c), tp = PeakTime(k, c);
            return (float)(Math.Exp(-c * 0.5 * tp) * Math.Sin(wd * tp) / wd);
        }

        /// <summary>Advance by `dt` s along the exact damped path (snapped to rest once it is there).</summary>
        public void Advance(float dt, float k, float c)
        {
            if (!(dt > 0)) return;
            if (Resting)
            {
                x = v = 0;
                return;
            }
            double s = c * 0.5, wd = Wd(k, c);
            double e = Math.Exp(-s * dt), co = Math.Cos(wd * dt), si = Math.Sin(wd * dt);
            double x0 = x, v0 = v;
            x = (float)(e * (x0 * co + (v0 + s * x0) / wd * si));
            v = (float)(e * (v0 * co - ((wd * wd + s * s) * x0 + s * v0) / wd * si));
        }

        /// <summary>
        /// A hit: add the velocity that from rest would peak at `peak` (signed). A spring on its way back stops first (the new
        /// hit pushes from where it is), and the peak it is heading for never passes `cap` (pellets in one step stack up to it).
        /// </summary>
        public void Kick(float peak, float cap, float k, float c)
        {
            if (peak == 0) return;
            float sgn = peak > 0 ? 1 : -1;
            float ppv = PeakPerVelocity(k, c);
            if (v * sgn < 0) v = 0;
            float ahead = x * sgn + v * sgn * ppv; // an upper bound on the peak it is heading for
            float add = Math.Min(Math.Abs(peak), Math.Max(0, cap - ahead));
            v += sgn * add / ppv;
        }
    }

    /// <summary>The body's reaction to hits in one hit slot (a sideways kick, its lean, a squash and stretch, a stream's hold).</summary>
    public struct HitReactBody
    {
        /// <summary>The sideways kick (m along the shooter's right, from the hold's rest) and the squash (a share, from its rest).</summary>
        public HitSpring lateral, squash;
        /// <summary>The stream's hold (0-1), the side the body is kicked to (+1 the shooter's right, -1 left, 0 never kicked),
        /// and the frame time since the last kick (s).</summary>
        public float hold, side, sinceKick;

        const float Idle = 1e6f;
        const float K = UnityTuning.HitReactSpring, C = UnityTuning.HitReactDamping;

        /// <summary>A body no hit has touched.</summary>
        public static HitReactBody Rest => new HitReactBody { sinceKick = Idle };

        /// <summary>The sideways offset (m along the shooter's right at the last hit).</summary>
        public float Lateral => side * hold * UnityTuning.HitReactHoldLat + lateral.x;

        /// <summary>The squash (a share of the height: + the top drops and the body widens, - the stretch).</summary>
        public float Squash => hold * UnityTuning.HitReactHoldSquash + squash.x;

        /// <summary>The lean (deg; the top toward +Lateral), following the kick: HitReactLeanDeg at a full kick's peak.</summary>
        public float LeanDeg => UnityTuning.HitReactLeanDeg * Lateral / UnityTuning.HitReactLatPeak;

        /// <summary>NIGHT36 F crunch36 (view): the stagger, a slower spring (deg) that tips the body's top back away from the shooter (the hit reads as a
        /// blow, not a twitch; REVIEW.md root cause 3: "no stagger pose"). Kicked only with crunch36 on (EnemyViews.Hit.cs); 0 otherwise.</summary>
        public HitSpring stagger;

        /// <summary>NIGHT36: the stagger's tip (deg, + the top away from the shooter).</summary>
        public float Stagger36Deg => stagger.x;

        /// <summary>NIGHT36 crunch36 (the lead, n36-r2: a head-on back-tip doesn't read from the front): a squash (a share: the height x (1 - it), the
        /// width x (1 + it x Crunch36Bulge)) and a twist about the vertical (deg, signed by where the hit landed), on the fast hit spring, NOT range-scaled,
        /// so the read is the same at any distance and from the front.</summary>
        public HitSpring squash36, twist36;

        public float Squash36 => squash36.x;
        public float Twist36Deg => twist36.x;

        /// <summary>NIGHT36: kick the squash and the twist (`side` +1 / -1 from the hit's position). `motionOn` false: none.</summary>
        public void Pop36(float squashPeak, float twistDeg, int side, bool motionOn)
        {
            if (!motionOn) return;
            squash36.Kick(squashPeak, squashPeak, K, C);
            twist36.Kick(side * twistDeg, twistDeg, K, C);
            sinceKick = 0;
        }

        /// <summary>NIGHT36 crunch36: a stagger kick peaking at `deg` (capped at Stagger36CapDeg with any stagger still running). `motionOn` false: none.</summary>
        public void Stagger36(float deg, bool motionOn)
        {
            if (!motionOn || !(deg > 0)) return;
            stagger.Kick(deg, UnityTuning.Stagger36CapDeg, UnityTuning.Stagger36Spring, UnityTuning.Stagger36Damping);
            sinceKick = 0;
        }

        /// <summary>Nothing left to draw or integrate.</summary>
        public bool Resting => hold == 0 && lateral.Resting && squash.Resting && stagger.Resting && squash36.Resting && twist36.Resting;

        /// <summary>A hit's share of the full reaction: damage over `fullDamage` within [HitReactMinScale, 1], x the melee or heavy weight.</summary>
        public static float Scale(double damage, double fullDamage, bool melee, bool heavy)
        {
            float s = fullDamage > 0 ? (float)Math.Min(1, Math.Max(UnityTuning.HitReactMinScale, damage / fullDamage)) : 1;
            return s * (melee ? UnityTuning.HitReactMelee : heavy ? UnityTuning.HitReactHeavy : 1);
        }

        /// <summary>
        /// NIGHT18 pass 2: the drawn size's share at `distance` m (level, eye to the hit): distance / HitReactRefDistance within
        /// [HitReactRangeMin, HitReactRangeMax], so the reaction's screen size holds across range (none or NaN: the close size).
        /// </summary>
        public static float RangeScale(float distance) =>
            distance > 0 ? Math.Min(UnityTuning.HitReactRangeMax, Math.Max(UnityTuning.HitReactRangeMin, distance / UnityTuning.HitReactRefDistance)) : UnityTuning.HitReactRangeMin;

        /// <summary>
        /// A hit of `scale` (Scale). `want`: the side the shot pushes it to (+1 / -1), 0 when the shot was centred; `fallback`:
        /// a fresh body's side for a centred shot (the enemy id's parity). Inside a stream the side sticks. `motionOn` false
        /// (REDUCED MOTION, EFFECTS 0): no kick.
        /// </summary>
        public void Hit(int want, int fallback, float scale, bool motionOn)
        {
            if (!motionOn || !(scale > 0)) return;
            bool live = sinceKick < UnityTuning.HitReactStreamWindow; // includes 0: another pellet of this step
            bool stream = live && sinceKick > 0;
            float next = side == 0 ? (want != 0 ? want : fallback >= 0 ? 1 : -1) : !live && want != 0 ? want : side;
            if (next != side)
            {
                lateral.x += (side - next) * hold * UnityTuning.HitReactHoldLat; // the hold's rest flips sides: the drawn pose doesn't jump
                side = next;
            }
            if (stream)
            {
                float h0 = hold;
                hold = Math.Min(1, hold + UnityTuning.HitReactHoldStep * scale);
                float dh = hold - h0;
                lateral.x -= side * dh * UnityTuning.HitReactHoldLat; // the rest moved out; the springs follow it
                squash.x -= dh * UnityTuning.HitReactHoldSquash;
            }
            lateral.Kick(side * UnityTuning.HitReactLatPeak * scale, UnityTuning.HitReactLatCap - hold * UnityTuning.HitReactHoldLat, K, C);
            squash.Kick(UnityTuning.HitReactSquashPeak * scale, UnityTuning.HitReactSquashCap - hold * UnityTuning.HitReactHoldSquash, K, C);
            sinceKick = 0;
        }

        /// <summary>Advance by `dt` s of frame time (0 while paused): the hold decays (its rest comes back), the springs run.</summary>
        public void Advance(float dt)
        {
            if (!(dt > 0)) return;
            sinceKick = Math.Min(Idle, sinceKick + dt);
            if (Resting)
            {
                lateral = default;
                squash = default;
                stagger = default;
                squash36 = default;
                twist36 = default;
                return;
            }
            stagger.Advance(dt, UnityTuning.Stagger36Spring, UnityTuning.Stagger36Damping); // NIGHT36 (0 at rest: nothing runs with crunch36 off)
            squash36.Advance(dt, K, C);
            twist36.Advance(dt, K, C);
            if (hold > 0)
            {
                float h0 = hold;
                hold *= (float)Math.Exp(-UnityTuning.HitReactHoldDecay * dt);
                if (hold < UnityTuning.HitReactRestEps) hold = 0;
                float dh = h0 - hold;
                lateral.x += side * dh * UnityTuning.HitReactHoldLat; // the rest came back in; the drawn pose doesn't jump
                squash.x += dh * UnityTuning.HitReactHoldSquash;
            }
            lateral.Advance(dt, K, C);
            squash.Advance(dt, K, C);
        }
    }
}
