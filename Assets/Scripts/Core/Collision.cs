// The collision queries the mover needs, in the shape of the three.js Octree calls mover.js makes
// (getCapsuleTriangles + triangleCapsuleIntersect per candidate, rayIntersect). Two worlds implement it:
// TriangleWorld (an exact port of the three.js per-triangle test, the reference) and the Unity physics world
// (Physics.ComputePenetration + a contact-point lookup), so the same mover can run on both and be compared.

namespace Skygrind.Core
{
    /// <summary>A capsule: the segment start (bottom) to end (top), and its radius (three.js Capsule).</summary>
    public struct Capsule
    {
        public Vec3 start;
        public Vec3 end;
        public double radius;

        public void Translate(Vec3 d)
        {
            start += d;
            end += d;
        }

        public Vec3 Center => (start + end) * 0.5;
    }

    /// <summary>One contact: the push-out normal (unit), the contact point, and the depth (m).</summary>
    public struct CapsuleContact
    {
        public Vec3 normal;
        public Vec3 point;
        public double depth;
    }

    public struct RayHit
    {
        public double distance;
        public Vec3 normal; // the surface normal at the hit (three.js: the hit triangle's normal)
    }

    public static class WorldCollider
    {
        /// <summary>
        /// world/collider.js capsuleIntersect (three's games_fps push-out, what web enemies use): gather once, resolve
        /// every candidate in order on a copy of the capsule, return the net push as a unit normal and a depth.
        /// </summary>
        public static bool CapsuleIntersect(ICollisionWorld world, in Capsule capsule, out Vec3 normal, out double depth)
        {
            Capsule copy = capsule;
            int count = world.GatherCandidates(copy);
            bool hit = false;
            for (int i = 0; i < count; i++)
            {
                if (!world.CapsuleIntersect(copy, i, out CapsuleContact c)) continue;
                hit = true;
                copy.Translate(c.normal * c.depth);
            }
            normal = default;
            depth = 0;
            if (!hit) return false;
            // capsule.getCenter: (end + start) * 0.5
            Vec3 v = (copy.end + copy.start) * 0.5 - (capsule.end + capsule.start) * 0.5;
            depth = v.Length;
            normal = Vec3.Normalize(v);
            return true;
        }
    }

    public interface ICollisionWorld
    {
        /// <summary>octree.getCapsuleTriangles: loads the candidates that may touch the capsule; returns the count.</summary>
        int GatherCandidates(in Capsule capsule);

        /// <summary>octree.triangleCapsuleIntersect against candidate i of the last gather (the capsule as it is now).</summary>
        bool CapsuleIntersect(in Capsule capsule, int candidate, out CapsuleContact contact);

        /// <summary>octree.rayIntersect: the nearest hit along a unit direction (no range limit).</summary>
        bool RayIntersect(Vec3 origin, Vec3 direction, out RayHit hit);
    }
}
