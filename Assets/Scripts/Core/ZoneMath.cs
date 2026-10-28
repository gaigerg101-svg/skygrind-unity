// Port of skygrind/src/survival/zoneMath.js (pure zone geometry, SURVIVAL.md §3). A zone is a set of disjoint
// axis-aligned parts; a locked zone's parts are solid force fields, and the faces of locked parts that touch open
// space are the gates you see and buy. The web's maps bring their parts resolved (map.zoneParts, made by the web's
// resolveZones); NIGHT15 ported resolveZones too (ResolveZones below), for Unity-authored maps (NavGridBuilder.ExportFor).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public struct ZonePart
    {
        public int zone;
        public Vec3 min;
        public Vec3 max;
    }

    /// <summary>NIGHT15 U3-28.3: one zone as resolveZones reads it (map data.zones[i]: id, priority, boxes).</summary>
    public sealed class ZoneSource
    {
        public string id;
        public double priority;
        public readonly List<MapBox> boxes = new List<MapBox>();
    }

    public struct GateQuad
    {
        public int zone;
        public Vec3 c0, c1, c2, c3; // counter-clockwise seen from the open side
        public Vec3 normal;
    }

    public static class ZoneMath
    {
        static double Axis(Vec3 v, int i) => i == 0 ? v.x : i == 1 ? v.y : v.z;

        static Vec3 With(Vec3 v, int i, double value)
        {
            if (i == 0) v.x = value;
            else if (i == 1) v.y = value;
            else v.z = value;
            return v;
        }

        /// <summary>The zone index at a point (the part that contains it), or -1 outside every zone.</summary>
        public static int ZoneIndexAt(IReadOnlyList<ZonePart> parts, double x, double y, double z)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                ZonePart p = parts[i];
                if (x >= p.min.x && x < p.max.x && y >= p.min.y && y < p.max.y && z >= p.min.z && z < p.max.z) return p.zone;
            }
            return -1;
        }

        /// <summary>
        /// Gate quads: faces of locked parts where the space just outside belongs to an open zone, cut into cells of
        /// about `cell` m so a face that only partly touches open space shows only there.
        /// </summary>
        public static List<GateQuad> GateQuads(IReadOnlyList<ZonePart> parts, Func<int, bool> isLocked, double cell = 2)
        {
            var quads = new List<GateQuad>();
            const double probe = 0.05;
            foreach (ZonePart p in parts)
            {
                if (!isLocked(p.zone)) continue;
                for (int axis = 0; axis < 3; axis++)
                {
                    foreach (int side in new[] { -1, 1 })
                    {
                        double plane = side < 0 ? Axis(p.min, axis) : Axis(p.max, axis);
                        int u = (axis + 1) % 3;
                        int v = (axis + 2) % 3;
                        int nu = (int)Math.Max(1, JsMath.Round((Axis(p.max, u) - Axis(p.min, u)) / cell));
                        int nv = (int)Math.Max(1, JsMath.Round((Axis(p.max, v) - Axis(p.min, v)) / cell));
                        double du = (Axis(p.max, u) - Axis(p.min, u)) / nu;
                        double dv = (Axis(p.max, v) - Axis(p.min, v)) / nv;
                        for (int i = 0; i < nu; i++)
                        {
                            for (int j = 0; j < nv; j++)
                            {
                                Vec3 c = default;
                                c = With(c, axis, plane + side * probe);
                                c = With(c, u, Axis(p.min, u) + (i + 0.5) * du);
                                c = With(c, v, Axis(p.min, v) + (j + 0.5) * dv);
                                int outside = ZoneIndexAt(parts, c.x, c.y, c.z);
                                if (outside < 0 || isLocked(outside)) continue;
                                Vec3 Corner(double a, double b)
                                {
                                    Vec3 q = default;
                                    q = With(q, axis, plane);
                                    q = With(q, u, Axis(p.min, u) + a * du);
                                    q = With(q, v, Axis(p.min, v) + b * dv);
                                    return q;
                                }
                                var quad = new GateQuad { zone = p.zone, c0 = Corner(i, j), c1 = Corner(i + 1, j), c2 = Corner(i + 1, j + 1), c3 = Corner(i, j + 1) };
                                if (side < 0) (quad.c0, quad.c1, quad.c2, quad.c3) = (quad.c3, quad.c2, quad.c1, quad.c0); // counter-clockwise seen from outside
                                quad.normal = With(default, axis, side);
                                quads.Add(quad);
                            }
                        }
                    }
                }
            }
            return quads;
        }

        /// <summary>
        /// NIGHT15 U3-28.3 (builder N): resolveZones(zones, holes) (zoneMath.js :52-72): disjoint parts per zone; each zone
        /// keeps its boxes minus every higher-priority zone's boxes (and minus its own earlier boxes, so a zone's parts never
        /// overlap each other either); holes belong to no zone (solid stone that must never show a gate). Parts are tagged
        /// with the zone's index. Ported line for line with subtractBox / subtractAll / boxVolume (ZoneSubtractBox,
        /// ZoneSubtractAll, ZoneBoxVolume). NavGridBuilder.ExportFor reads a missing priority as NaN (JS undefined: no zone is
        /// higher or lower). The oracle: survival.json zoneParts (114 parts). A load-time call (it allocates).
        /// </summary>
        public static List<ZonePart> ResolveZones(IReadOnlyList<ZoneSource> zones, IReadOnlyList<MapBox> holes)
        {
            var parts = new List<ZonePart>();
            for (int index = 0; index < zones.Count; index++)
            {
                ZoneSource zone = zones[index];
                // [...zones.filter((z) => z.priority > zone.priority).flatMap((z) => z.boxes), ...holes] (holes = [] by default)
                var higher = new List<MapBox>();
                for (int j = 0; j < zones.Count; j++) if (zones[j].priority > zone.priority) higher.AddRange(zones[j].boxes);
                if (holes != null) for (int j = 0; j < holes.Count; j++) higher.Add(holes[j]);
                var own = new List<MapBox>();
                // own.push(...subtractAll(subtractAll([box], higher), own)): the argument (against own as it is) first.
                foreach (MapBox box in zone.boxes) own.AddRange(ZoneSubtractAll(ZoneSubtractAll(new List<MapBox> { box }, higher), own));
                own = own.FindAll(p => ZoneBoxVolume(p) > ZoneEps);
                foreach (MapBox p in own) parts.Add(new ZonePart { zone = index, min = p.min, max = p.max });
            }
            return parts;
        }

        // --- resolveZones' helpers (zoneMath.js EPS, overlaps, boxVolume, subtractBox, subtractAll) -------------------

        const double ZoneEps = 1e-6; // zoneMath.js EPS

        static bool ZoneOverlaps(MapBox a, MapBox b) =>
            a.min.x < b.max.x - ZoneEps && a.max.x > b.min.x + ZoneEps &&
            a.min.y < b.max.y - ZoneEps && a.max.y > b.min.y + ZoneEps &&
            a.min.z < b.max.z - ZoneEps && a.max.z > b.min.z + ZoneEps;

        /// <summary>Box volume (m³).</summary>
        static double ZoneBoxVolume(MapBox b) => (b.max.x - b.min.x) * (b.max.y - b.min.y) * (b.max.z - b.min.z);

        /// <summary>a minus b as disjoint boxes (up to 6): slabs below / above b on each axis in turn.</summary>
        static List<MapBox> ZoneSubtractBox(MapBox a, MapBox b)
        {
            if (!ZoneOverlaps(a, b)) return new List<MapBox> { a };
            var output = new List<MapBox>();
            Vec3 min = a.min;
            Vec3 max = a.max;
            for (int axis = 0; axis < 3; axis++)
            {
                if (Axis(b.min, axis) > Axis(min, axis) + ZoneEps)
                {
                    output.Add(new MapBox { min = min, max = With(max, axis, Axis(b.min, axis)) }); // lo
                    min = With(min, axis, Axis(b.min, axis));
                }
                if (Axis(b.max, axis) < Axis(max, axis) - ZoneEps)
                {
                    output.Add(new MapBox { min = With(min, axis, Axis(b.max, axis)), max = max }); // hi
                    max = With(max, axis, Axis(b.max, axis));
                }
            }
            return output; // what is left in [min, max] is inside b and dropped
        }

        /// <summary>Boxes minus every box in `holes`.</summary>
        static List<MapBox> ZoneSubtractAll(List<MapBox> boxes, List<MapBox> holes)
        {
            List<MapBox> parts = boxes;
            foreach (MapBox h in holes)
            {
                var next = new List<MapBox>();
                foreach (MapBox p in parts) next.AddRange(ZoneSubtractBox(p, h));
                parts = next;
            }
            return parts.FindAll(p => ZoneBoxVolume(p) > ZoneEps);
        }

        /// <summary>Can a zone be bought from here? It is locked and the buy spot stands in an open zone.</summary>
        public static bool SpotBuyable(IReadOnlyList<ZonePart> parts, Vec3 spot, int targetZone, Func<int, bool> isLocked)
        {
            if (!isLocked(targetZone)) return false;
            int here = ZoneIndexAt(parts, spot.x, spot.y + 1, spot.z);
            return here >= 0 && !isLocked(here);
        }
    }
}
