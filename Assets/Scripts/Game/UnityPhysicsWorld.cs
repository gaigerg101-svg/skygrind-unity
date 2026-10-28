// ICollisionWorld on Unity physics, standing in for the three.js Octree calls mover.js makes:
// - candidates: Physics.OverlapBox over the capsule's bounds (the Octree also returns a superset: its leaf boxes);
// - contact: Physics.ComputePenetration(capsule, collider) for the push-out normal and depth. Unity gives no contact
//   point, which mover.js needs (feet vs wall). Convex colliders: Collider.ClosestPoint, refined toward the capsule
//   axis. Non-convex MeshColliders (the per-triangle build, UnityColliders.FromTriangles) have no ClosestPoint, so the
//   point is placed the way three.js places a face contact: on the axis, where the pushed-out plane crosses it;
// - rays: Physics.Raycast (hits nothing it starts inside, like three.js with back-face culling).
// Differences from three.js are measured, not assumed: see the counters and the parity tests.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class UnityPhysicsWorld : ICollisionWorld
    {
        readonly CapsuleCollider query; // a probe collider on the Ignore Raycast layer, reshaped per query
        readonly int mask;
        readonly Collider[] candidates = new Collider[256];
        int count;
        float shapeRadius = -1;
        float shapeHeight = -1;

        // Counters (reset by the owner each step): queries made and where Unity could not give what mover.js uses.
        public int gathers;
        public int penetrationTests;
        public int contacts;
        public int pointFallbacks; // a non-convex MeshCollider: ClosestPoint unsupported, point derived from the MTD
        public int overflow; // more candidates than the buffer holds

        public UnityPhysicsWorld(CapsuleCollider query)
        {
            this.query = query;
            query.direction = 1; // Y
            query.center = Vector3.zero;
            mask = Physics.DefaultRaycastLayers & ~(1 << query.gameObject.layer);
        }

        public int GatherCandidates(in Capsule capsule)
        {
            gathers++;
            Vector3 s = WebSpace.ToUnity(capsule.start);
            Vector3 e = WebSpace.ToUnity(capsule.end);
            float r = (float)capsule.radius;
            Vector3 min = Vector3.Min(s, e) - new Vector3(r, r, r);
            Vector3 max = Vector3.Max(s, e) + new Vector3(r, r, r);
            count = Physics.OverlapBoxNonAlloc((min + max) * 0.5f, (max - min) * 0.5f, candidates, Quaternion.identity, mask, QueryTriggerInteraction.Ignore);
            if (count == candidates.Length) overflow++;
            return count;
        }

        public bool CapsuleIntersect(in Capsule capsule, int candidate, out CapsuleContact contact)
        {
            contact = default;
            penetrationTests++;
            Collider other = candidates[candidate];
            Vector3 s = WebSpace.ToUnity(capsule.start);
            Vector3 e = WebSpace.ToUnity(capsule.end);
            float r = (float)capsule.radius;
            Shape(r, Vector3.Distance(s, e) + 2 * r);
            Vector3 center = (s + e) * 0.5f;
            Transform t = other.transform;
            if (!Physics.ComputePenetration(query, center, Quaternion.identity, other, t.position, t.rotation, out Vector3 dir, out float depth)) return false;
            contacts++;
            contact.normal = WebSpace.FromUnity(dir);
            contact.depth = depth;
            contact.point = WebSpace.FromUnity(ContactPoint(other, s, e, r, dir, depth));
            return true;
        }

        public bool RayIntersect(Vec3 origin, Vec3 direction, out RayHit hit)
        {
            hit = default;
            if (!Physics.Raycast(WebSpace.ToUnity(origin), WebSpace.ToUnity(direction), out RaycastHit h, 10000f, mask, QueryTriggerInteraction.Ignore)) return false;
            hit.distance = h.distance;
            hit.normal = WebSpace.FromUnity(h.normal);
            return true;
        }

        void Shape(float radius, float height)
        {
            if (radius == shapeRadius && height == shapeHeight) return;
            query.radius = radius;
            query.height = height;
            shapeRadius = radius;
            shapeHeight = height;
        }

        /// <summary>
        /// The contact point mover.js classifies (feet: below the lower sphere's top; wall: on the cylinder section).
        /// Convex colliders: ClosestPoint on the collider, alternated with the nearest point on the capsule axis. A
        /// non-convex MeshCollider has no ClosestPoint, so the point comes from the MTD plane the way three.js places
        /// a face contact (on the axis, where the pushed-out plane crosses it).
        /// </summary>
        Vector3 ContactPoint(Collider other, Vector3 s, Vector3 e, float r, Vector3 n, float depth)
        {
            if (other is MeshCollider mesh && !mesh.convex)
            {
                pointFallbacks++;
                float ds = Vector3.Dot(n, s);
                float de = Vector3.Dot(n, e);
                float plane = Mathf.Min(ds, de) - r + depth;
                float d1 = ds - plane - r;
                float d2 = de - plane - r;
                float sum = Mathf.Abs(d1) + Mathf.Abs(d2);
                float delta = sum > 0 ? Mathf.Abs(d1 / sum) : 0.5f;
                return Vector3.Lerp(s, e, delta);
            }
            Vector3 q = other.ClosestPoint((s + e) * 0.5f);
            for (int i = 0; i < 2; i++) q = other.ClosestPoint(ClosestOnSegment(s, e, q));
            return q;
        }

        static Vector3 ClosestOnSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 == 0) return a;
            return a + ab * Mathf.Clamp01(Vector3.Dot(p - a, ab) / len2);
        }
    }
}
