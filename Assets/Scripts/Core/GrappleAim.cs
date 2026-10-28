// What the ARCLINE hook bites on a grapple press, after skygrind/src/combat/arcline.js choose(). The enemy branch is
// ChooseEnemy (NIGHT21, the ARCLINE in Survival); Choose is the rest (no enemies in the arena): a GRAPPLE NODE in range with a clear line, else any solid surface under the
// crosshair ray or a ring of 8 probe rays standing in for a 1.5 m sphere-cast; SwingMath.PickAnchor chooses.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class GrappleAim
    {
        readonly MovementConfig config;
        readonly List<Vec3> nodes;
        readonly List<AnchorCandidate> candidates = new List<AnchorCandidate>(24);
        readonly List<int> candidateNode = new List<int>(24);
        ICollisionWorld world;
        Vec3 eye;

        public GrappleAim(MovementConfig config, List<Vec3> nodes)
        {
            this.config = config;
            this.nodes = nodes;
        }

        /// <summary>One fixed step, before the mover: on a grapple press, put the anchor on the frame.</summary>
        public void Step(ref InputFrame frame, Mover mover, Vec3 eyePoint, ICollisionWorld collision)
        {
            frame.grappleTarget = null;
            if (!mover.ropeEnabled || !frame.grapple.pressed || mover.rope.phase != RopePhase.None || mover.rope.cooldownSteps > 0) return;
            frame.grappleTarget = Choose(eyePoint, AimDirection(frame.yaw, frame.pitch), collision);
        }

        /// <summary>The aim direction for a look (yaw 0 looks toward −z; + pitch looks up).</summary>
        public static Vec3 AimDirection(double yaw, double pitch)
        {
            double cp = JsMath.Cos(pitch);
            return new Vec3(-JsMath.Sin(yaw) * cp, JsMath.Sin(pitch), -JsMath.Cos(yaw) * cp);
        }

        /// <summary>
        /// arcline.js choose(), the enemy half (NIGHT21): "Enemies first: a small one is yanked to you, a big one is a zip." The live
        /// enemy nearest the crosshair inside half the cone, in range (and at least 1 m away) with a clear line to its aim point; null
        /// when none (then Choose picks a node or a surface). `skip` lets the owner leave kinds out (a yank on its cooldown).
        /// </summary>
        public Enemy ChooseEnemy(Vec3 eyePoint, Vec3 dir, ICollisionWorld collision, EnemyRegistry enemies, System.Func<Enemy, bool> skip = null)
        {
            ArclineConfig a = config.arcline;
            eye = eyePoint;
            world = collision;
            double cone = a.coneDeg * System.Math.PI / 180;
            // The angle test of the web (acos(dot / d) < bestAngle * 0.5, bestAngle = the last accepted angle) as a cosine test: cos(ang / 2) = sqrt((1 + cos ang) / 2), no acos.
            double bestCos = JsMath.Cos(cone * 0.5); // threshold: the web's bestAngle (the cone) halved
            Enemy best = null;
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive || (skip != null && skip(e))) continue;
                Vec3 aim = e.AimPoint;
                double d = Vec3.DistanceTo(aim, eye);
                if (d > a.range || d < 1) continue;
                double c = Vec3.Dot(aim - eye, dir) / d;
                if (c > bestCos && Sees(aim))
                {
                    bestCos = System.Math.Sqrt((1 + c) / 2); // bestAngle = ang, so the next must beat half of it: cos(ang / 2)
                    best = e;
                }
            }
            return best;
        }

        /// <summary>The anchor under the crosshair right now, or null.</summary>
        public GrappleTarget? Choose(Vec3 eyePoint, Vec3 dir, ICollisionWorld collision)
        {
            ArclineConfig a = config.arcline;
            eye = eyePoint;
            world = collision;
            double cone = a.coneDeg * System.Math.PI / 180;
            candidates.Clear();
            candidateNode.Clear();
            // GRAPPLE NODES in range with a clear line (a node is always a good swing).
            for (int i = 0; i < nodes.Count && candidates.Count < 24; i++)
            {
                Vec3 p = nodes[i];
                if (Vec3.DistanceTo(p, eye) > a.range || !Sees(p)) continue;
                candidates.Add(new AnchorCandidate { point = p, node = true });
                candidateNode.Add(i + 1);
            }
            // Surfaces: the crosshair ray, and a ring of rays around it (a 1.5 m sphere-cast at range).
            bool straightHit = Cast(dir, a.range, out double straight);
            if (straightHit) AddHit(dir, straight);
            Vec3 side = Cross(dir, new Vec3(0, 1, 0));
            if (Vec3.Dot(side, side) < 1e-6) side = new Vec3(1, 0, 0);
            side = Vec3.Normalize(side);
            Vec3 up = Cross(side, dir);
            up = Vec3.Normalize(up);
            double reach = straightHit ? straight : a.range * 0.6;
            double off = JsMath.Min(cone * 0.9, JsMath.Atan2(a.probeRadius, JsMath.Max(4, reach)));
            for (int i = 0; i < 8; i++)
            {
                double ang = (i / 8.0) * System.Math.PI * 2;
                Vec3 probe = dir + side * (JsMath.Cos(ang) * JsMath.Tan(off)) + up * (JsMath.Sin(ang) * JsMath.Tan(off));
                probe = Vec3.Normalize(probe);
                if (Cast(probe, a.range, out double d)) AddHit(probe, d);
            }
            int best = SwingMath.PickAnchor(candidates, eye, dir, cone, a.range);
            if (best < 0) return null;
            return new GrappleTarget { point = candidates[best].point, node = candidateNode[best] };
        }

        void AddHit(Vec3 d, double distance)
        {
            if (candidates.Count >= 24) return;
            candidates.Add(new AnchorCandidate { point = eye + d * distance, node = false });
            candidateNode.Add(0);
        }

        /// <summary>First world hit along a ray from the eye within range.</summary>
        bool Cast(Vec3 d, double range, out double distance)
        {
            distance = 0;
            if (!world.RayIntersect(eye, d, out RayHit hit) || hit.distance > range) return false;
            distance = hit.distance;
            return true;
        }

        bool Sees(Vec3 p)
        {
            Vec3 t = p - eye;
            double d = t.Length;
            if (!world.RayIntersect(eye, Vec3.DivideScalar(t, d != 0 ? d : 1), out RayHit hit) || hit.distance > d) return true;
            return hit.distance >= d - config.arcline.losSlack;
        }

        static Vec3 Cross(Vec3 a, Vec3 b) => new Vec3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }
}
