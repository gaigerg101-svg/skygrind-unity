// Port of skygrind/src/fx/kicks.js: finite kick curves for the camera (jump kick, slam punch). Every kick ends at
// exactly 0, so kicks never leave a drift. Frame time only (visual).
using System;

namespace Skygrind.Core
{
    public sealed class KickStack
    {
        readonly double[] amp;
        readonly double[] age;
        readonly double[] attack;
        readonly double[] recover;
        readonly bool[] live;

        public KickStack(int capacity)
        {
            amp = new double[capacity];
            age = new double[capacity];
            attack = new double[capacity];
            recover = new double[capacity];
            live = new bool[capacity];
        }

        /// <summary>Kicks still recovering.</summary>
        public int Active
        {
            get
            {
                int n = 0;
                for (int i = 0; i < live.Length; i++) n += live[i] ? 1 : 0;
                return n;
            }
        }

        /// <summary>
        /// One kick's shape: an eased rise to exactly 1 at `attack`, then a smoothstep fall that is exactly 0 from
        /// `recover` on (recover includes the attack).
        /// </summary>
        public static double KickCurve(double t, double attack, double recover)
        {
            double a = JsMath.Min(attack, recover);
            if (t >= recover || t < 0) return 0;
            if (t < a) return JsMath.Sin((Math.PI / 2) * (t / a));
            double u = (t - a) / (recover - a);
            return 1 - u * u * (3 - 2 * u);
        }

        /// <summary>A full stack replaces the kick that is furthest through its recovery.</summary>
        public void Add(double amplitude, double attackTime, double recoverTime)
        {
            int slot = -1;
            double furthest = -1;
            for (int i = 0; i < live.Length; i++)
            {
                if (!live[i])
                {
                    slot = i;
                    break;
                }
                double progress = age[i] / recover[i];
                if (progress > furthest)
                {
                    furthest = progress;
                    slot = i;
                }
            }
            amp[slot] = amplitude;
            age[slot] = 0;
            attack[slot] = attackTime;
            recover[slot] = recoverTime;
            live[slot] = true;
        }

        public void Advance(double dt)
        {
            for (int i = 0; i < live.Length; i++)
            {
                if (!live[i]) continue;
                age[i] += dt;
                if (age[i] >= recover[i]) live[i] = false;
            }
        }

        /// <summary>Sum of every live kick (exactly 0 when none are live).</summary>
        public double Value()
        {
            double sum = 0;
            for (int i = 0; i < live.Length; i++)
            {
                if (live[i]) sum += amp[i] * KickCurve(age[i], attack[i], recover[i]);
            }
            return sum;
        }

        public void Clear() => Array.Clear(live, 0, live.Length);
    }
}
