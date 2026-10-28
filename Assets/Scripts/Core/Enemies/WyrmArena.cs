// NIGHT24 (builder B): THE KILN WYRM's arena (Design/maps/wyrm-arena.json), read as plain boxes: a closed room 60 m across and 30 m
// high built only from axis-aligned boxes (the C# collider's kind), with pillars, platforms and tall walls. ARENA-LOCAL coordinates (the
// room's centre is 0, 0, 0; the layer-1 builder offsets the room). Not a SurvivalMap export: no nav grid and no zones, nothing walks here.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class WyrmArena
    {
        public readonly List<(Vec3 min, Vec3 max, string kind)> boxes = new List<(Vec3, Vec3, string)>();
        public Vec3 spawn;
        public double spawnYawDeg, half, height;

        public static WyrmArena Load(string path)
        {
            var j = PortData.Obj(PortData.LoadJson(path));
            var a = new WyrmArena { half = PortData.Num(j["half"]), height = PortData.Num(j["height"]) };
            var sp = PortData.Obj(j["playerSpawn"]);
            a.spawn = new Vec3(PortData.Num(sp["x"]), PortData.Num(sp["y"]), PortData.Num(sp["z"]));
            a.spawnYawDeg = PortData.Num(sp["yawDeg"]);
            foreach (object o in PortData.Arr(PortData.Obj(j["collision"])["boxes"]))
            {
                var b = PortData.Obj(o);
                a.boxes.Add((PortData.V3(b["min"]), PortData.V3(b["max"]), (string)b["kind"]));
            }
            return a;
        }

        /// <summary>The collider for the arena at an offset: every box once (TriangleWorld, 12 outward triangles each).</summary>
        public TriangleWorld BuildWorld(Vec3 offset)
        {
            var w = new TriangleWorld();
            foreach (var (min, max, kind) in boxes) w.AddBox(min + offset, max + offset);
            return w;
        }

        /// <summary>Is a point inside a solid box of `kinds` (null: any), grown by `pad`?</summary>
        public bool InSolid(Vec3 p, double pad, string kind = null)
        {
            foreach (var (min, max, k) in boxes)
            {
                if (kind != null && k != kind) continue;
                if (p.x > min.x - pad && p.x < max.x + pad && p.y > min.y - pad && p.y < max.y + pad && p.z > min.z - pad && p.z < max.z + pad) return true;
            }
            return false;
        }
    }
}
