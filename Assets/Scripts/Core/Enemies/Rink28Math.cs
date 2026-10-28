// NIGHT28 (the lead): the closed-form centre line of THE GLACIER's stadium rink (Research/2026-10-09-wave28/GLACIER-REDESIGN.md section 3.1):
// two straights of length `L` joined by two half-circle caps of centre-line radius `rc`, travelled counter-clockwise (seen from +y with x right and z up in the sim's
// space), the long axis along x, centred on (cx, cz). `s` is the arc coordinate in [0, Lap): 0 = the start of the bottom straight (z = cz - rc, x = cx - L/2),
// the bottom straight runs +x, the east cap, the top straight runs -x, the west cap. `lateral` is the signed offset from the centre line, POSITIVE OUTWARD
// (toward the rim), negative toward the island. Pure math on JsMath (no System.Math Sin / Cos), no engine references: the boss AI, the arena generator and the
// view all read it, so they can never disagree about where the track is.
using System;

namespace Skygrind.Core
{
    public struct Rink28Math
    {
        public double L, rc, cx, cz;

        public Rink28Math(double straight, double centreRadius, double centreX, double centreZ)
        {
            L = straight;
            rc = centreRadius;
            cx = centreX;
            cz = centreZ;
        }

        public double CapLength => Math.PI * rc;

        /// <summary>The length of the centre line (2 L + 2 pi rc).</summary>
        public double Lap => 2 * L + 2 * Math.PI * rc;

        /// <summary>s wrapped into [0, Lap).</summary>
        public double Wrap(double s)
        {
            double lap = Lap;
            s %= lap;
            return s < 0 ? s + lap : s;
        }

        /// <summary>The signed shortest arc from `from` to `to` in (-Lap / 2, Lap / 2]: positive = `to` is ahead (counter-clockwise).</summary>
        public double SignedGap(double from, double to)
        {
            double lap = Lap, d = Wrap(to - from);
            return d > lap / 2 ? d - lap : d;
        }

        /// <summary>The point at arc s and lateral offset (outward positive), with the unit tangent of travel there.</summary>
        public void PointAt(double s, double lateral, out double x, out double z, out double tx, out double tz)
        {
            s = Wrap(s);
            double cap = CapLength;
            double ox, oz; // the outward unit vector
            if (s < L)
            {
                x = cx - L / 2 + s; z = cz - rc; tx = 1; tz = 0; ox = 0; oz = -1;
            }
            else if (s < L + cap)
            {
                double th = -Math.PI / 2 + (s - L) / rc;
                double c = JsMath.Cos(th), sn = JsMath.Sin(th);
                x = cx + L / 2 + rc * c; z = cz + rc * sn; tx = -sn; tz = c; ox = c; oz = sn;
            }
            else if (s < 2 * L + cap)
            {
                x = cx + L / 2 - (s - L - cap); z = cz + rc; tx = -1; tz = 0; ox = 0; oz = 1;
            }
            else
            {
                double ph = Math.PI / 2 + (s - 2 * L - cap) / rc;
                double c = JsMath.Cos(ph), sn = JsMath.Sin(ph);
                x = cx - L / 2 + rc * c; z = cz + rc * sn; tx = -sn; tz = c; ox = c; oz = sn;
            }
            x += ox * lateral;
            z += oz * lateral;
        }

        /// <summary>The nearest centre-line arc coordinate of (x, z) and the signed lateral offset (outward positive).</summary>
        public void Project(double x, double z, out double s, out double lateral)
        {
            double xe = cx + L / 2, xw = cx - L / 2;
            if (x >= xw && x <= xe)
            {
                if (z >= cz) { s = L + CapLength + (xe - x); lateral = (z - cz) - rc; }
                else { s = x - xw; lateral = (cz - z) - rc; }
                return;
            }
            if (x > xe)
            {
                double dx = x - xe, dz = z - cz, r = JsMath.Hypot(dx, dz);
                lateral = r - rc;
                double th = r < 1e-9 ? 0 : AsinBisect(dz / r); // cos >= 0 on the east half
                s = L + (th + Math.PI / 2) * rc;
                return;
            }
            {
                double dx = x - xw, dz = z - cz, r = JsMath.Hypot(dx, dz);
                lateral = r - rc;
                double psi = r < 1e-9 ? 0 : AsinBisect(-dz / r); // the west half mirrored: (cos psi, sin psi) = -(dx, dz) / r
                s = 2 * L + CapLength + (psi + Math.PI - Math.PI / 2) * rc;
            }
        }

        /// <summary>The yaw (degrees) that looks along the unit direction (tx, tz): the sim's yaw 0 looks toward -z, forward = (-sin yaw, -cos yaw). No System.Math trig (asin by bisection).</summary>
        public static double YawDegLooking(double tx, double tz)
        {
            double a = AsinBisect(-tx); // sin(yaw) = -tx
            double yaw = -tz >= 0 ? a : (a >= 0 ? Math.PI - a : -Math.PI - a); // cos(yaw) = -tz
            return yaw * 180 / Math.PI;
        }

        /// <summary>asin by bisection on JsMath.Sin over [-pi/2, pi/2] (deterministic, no System.Math trig).</summary>
        static double AsinBisect(double v)
        {
            if (v > 1) v = 1;
            if (v < -1) v = -1;
            double lo = -Math.PI / 2, hi = Math.PI / 2;
            for (int i = 0; i < 48; i++)
            {
                double mid = (lo + hi) / 2;
                if (JsMath.Sin(mid) < v) lo = mid; else hi = mid;
            }
            return (lo + hi) / 2;
        }
    }
}
