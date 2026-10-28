// Double-precision 3-vector for the pure movement math. The web game runs on 64-bit JS numbers and its tests check
// to 1e-10, which UnityEngine.Vector3 (32-bit floats) cannot hold, so Core stays in doubles and the controller
// converts at the edge. A struct: passing one never mutates the caller's copy, and nothing allocates.
using System;
using System.Globalization;

namespace Skygrind.Core
{
    [Serializable]
    public struct Vec3 : IEquatable<Vec3>
    {
        public double x;
        public double y;
        public double z;

        public static readonly Vec3 Zero = new Vec3(0, 0, 0);

        public Vec3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        // three.js Vector3 semantics (the web's mover.js calls these, not Math.hypot):
        /// <summary>three.js length(): sqrt(x² + y² + z²).</summary>
        public double Length => Math.Sqrt(x * x + y * y + z * z);

        /// <summary>three.js a.distanceTo(b): sqrt of the squared difference a − b.</summary>
        public static double DistanceTo(Vec3 a, Vec3 b)
        {
            double dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>three.js divideScalar(s): a multiply by 1 / s (rounds differently from dividing).</summary>
        public static Vec3 DivideScalar(Vec3 v, double s)
        {
            double inv = 1 / s;
            return new Vec3(v.x * inv, v.y * inv, v.z * inv);
        }

        /// <summary>three.js normalize(): divideScalar(length() || 1).</summary>
        public static Vec3 Normalize(Vec3 v)
        {
            double len = v.Length;
            return DivideScalar(v, len != 0 ? len : 1);
        }

        public static double Dot(Vec3 a, Vec3 b) => a.x * b.x + a.y * b.y + a.z * b.z;

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Vec3 operator -(Vec3 a) => new Vec3(-a.x, -a.y, -a.z);

        public static Vec3 operator *(Vec3 a, double k) => new Vec3(a.x * k, a.y * k, a.z * k);

        public static Vec3 operator *(double k, Vec3 a) => new Vec3(a.x * k, a.y * k, a.z * k);

        public static Vec3 operator /(Vec3 a, double k) => new Vec3(a.x / k, a.y / k, a.z / k);

        // Exact, like vitest's toEqual on {x, y, z}.
        public bool Equals(Vec3 other) => x == other.x && y == other.y && z == other.z;

        public override bool Equals(object obj) => obj is Vec3 other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(x, y, z);

        public static bool operator ==(Vec3 a, Vec3 b) => a.Equals(b);

        public static bool operator !=(Vec3 a, Vec3 b) => !a.Equals(b);

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0:R}, {1:R}, {2:R})", x, y, z);
    }
}
