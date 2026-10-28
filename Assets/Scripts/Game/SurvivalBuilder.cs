// Draws the Survival map from SurvivalMap (no colliders: the game collides on the C# web collider, GameWorld), after
// skygrind/src/world/arena.js: the stone (floor, map boxes, wedge ramps) with config.colors.pillarTop on tops and
// pillarSide on sides; the cyan neon (ride strips, the rims along rimKinds' top edges, pad rings, grapple nodes); the
// boundary barriers as a faint pale field; and the Grinder's pillars (CoreMesh, rebuilt as heights change).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class SurvivalMaterials
    {
        public Material stone;
        public Material neon;
        public Material field;
        public Material boundary;
        public Material lit;
        public Material sky;
        public Material acid; // the spitter's puddles (SKYGRIND/Acid, alpha-blended)
    }

    public static class SurvivalBuilder
    {
        public static GameObject Build(GameConfig cfg, SurvivalMap map, SurvivalMaterials mats)
        {
            var root = new GameObject("Survival map");
            Color top = Look.C(cfg, "colors.pillarTop"), side = Look.C(cfg, "colors.pillarSide");
            if (Ice40Look(cfg, map)) { top = Look.Hex(UnityTuning.Ice40TopHex); side = Look.Hex(UnityTuning.Ice40SideHex); } // NIGHT40 L2: n40-iceline's ice and cold stone (view only)
            var kit = new MeshKit();

            // Stone: the floor, every map box, the wedges (prisms).
            // NIGHT52 U look52 round 8 (Look52.cs Look52Tier): on spire / foundry values by the box's kind (ledges lifted; round 9: the floor as the walls' pair); off: one pair
            bool tier52 = Look52Tier.On(cfg, map);
            // NIGHT72 L5 (GG 2026-10-26 "textures overlapping that bug out"): each drawn box grows by a class chosen for this map (GrowPlan72) so no coplanar overlapping pair keeps one
            // plane; NIGHT71's index mod 3 left 82 pairs on the five levels. Off: -skygrindNoGrow72 (mod 3 again) or -skygrindNoBright71 (no growth). One proof line per map.
            int[] grow72 = Bright71.On && !GrowPlan72.Off ? GrowPlan72.Classes(map.boxes) : null;
            if (grow72 != null) Debug.Log(GrowPlan72.LogLine(map.name, GrowPlan72.Count(map.boxes, grow72)));
            if (tier52)
            {
                Look52Tier.Colours("floor", top, side, out Color ft, out Color fs);
                kit.Box(map.floor.min, map.floor.max, ft, fs);
                int gi = 0;
                foreach (MapBox b in map.boxes)
                {
                    Look52Tier.Colours(b.kind, top, side, out Color bt, out Color bs);
                    Vec3 bmin = b.min, bmax = b.max;
                    if (grow72 != null) Bright71.GrowClass(ref bmin, ref bmax, grow72[gi++]); else Bright71.Grow(ref bmin, ref bmax, gi++); // GG 2026-10-26: no z-fight between boxes sharing a face plane (drawn mesh only); NIGHT72: by the map's plan
                    kit.Box(bmin, bmax, bt, bs);
                }
            }
            else
            {
                kit.Box(map.floor.min, map.floor.max, top, side);
                int gi = 0;
                foreach (MapBox b in map.boxes)
                {
                    Vec3 bmin = b.min, bmax = b.max;
                    if (grow72 != null) Bright71.GrowClass(ref bmin, ref bmax, grow72[gi++]); else Bright71.Grow(ref bmin, ref bmax, gi++); // GG 2026-10-26: no z-fight between boxes sharing a face plane (drawn mesh only); NIGHT72: by the map's plan
                    kit.Box(bmin, bmax, top, side);
                }
            }
            foreach (WedgeSpec w in map.wedges) Wedge(kit, w, top, side);
            MeshKit.Object("Stone", root.transform, kit.Build("stone"), mats.stone);

            // Neon: strips, rims, pad rings, nodes.
            kit.Clear();
            foreach (MapBox b in map.strips) kit.Box(b.min, b.max, Color.white, Color.white);
            var rimKinds = new HashSet<string>(cfg.Strs("world.rimKinds"));
            double rw = cfg.Num("world.rimWidth"), lift = cfg.Num("world.rimLift");
            var rims54 = new MeshKit(); // NIGHT54 D (decor54, Decor54.cs): the rims on a decor kind's top (a room wall meets the ceiling there) in the level's accent; off: none
            foreach (MapBox b in map.boxes)
            {
                if (!rimKinds.Contains(b.kind)) continue;
                MeshKit rk = Decor54.RimDecor(cfg, map, b.kind) ? rims54 : kit;
                double y0 = b.max.y + lift - rw * 0.5, y1 = b.max.y + lift + rw * 0.5;
                rk.Box(new Vec3(b.min.x, y0, b.min.z - rw * 0.5), new Vec3(b.max.x, y1, b.min.z + rw * 0.5), Color.white, Color.white);
                rk.Box(new Vec3(b.min.x, y0, b.max.z - rw * 0.5), new Vec3(b.max.x, y1, b.max.z + rw * 0.5), Color.white, Color.white);
                rk.Box(new Vec3(b.min.x - rw * 0.5, y0, b.min.z), new Vec3(b.min.x + rw * 0.5, y1, b.max.z), Color.white, Color.white);
                rk.Box(new Vec3(b.max.x - rw * 0.5, y0, b.min.z), new Vec3(b.max.x + rw * 0.5, y1, b.max.z), Color.white, Color.white);
            }
            if (rims54.Vertices > 0) MeshKit.Object("Neon rims 54", root.transform, rims54.Build("neon rims54"), Decor54.RimMaterial(mats.neon, cfg), shadows: false);
            // NIGHT52 U look52 (Look52.cs): in a campaign map the node's cube bloomed into a flat cyan disc round GrappleNodes30's ring: the ring is the node
            bool cubes52 = !(Look52.On(cfg) && map.json != null && map.json.ContainsKey("campaign"));
            foreach (Vec3 node in map.grappleNodes)
            {
                if (!cubes52) { Look52.NodeCubesSkipped++; continue; }
                Vector3 p = WebSpace.ToUnity(node);
                kit.Cube(p, Vector3.one * 0.9f, Color.white);
            }
            MeshKit.Object("Neon", root.transform, kit.Build("neon"), mats.neon, shadows: false);

            // Pads: NIGHT27 builder P: a raised dark disc, glowing cyan top, chevrons toward the launch and an upward stream per pad (JumpPadView27); one object per
            // pad carries PadMarker (it sleeps while the pad does).
            for (int i = 0; i < map.pads.Count; i++) JumpPadView27.Build(cfg, map.pads[i], i, root.transform, mats);

            // The boundary: the barrier boxes' inner faces as a faint field.
            kit.Clear();
            foreach (MapBox b in map.barriers) kit.Box(b.min, b.max, Color.white, Color.white);
            MeshKit.Object("Boundary", root.transform, kit.Build("boundary"), mats.boundary, shadows: false);
            return root;
        }

        /// <summary>arena.js wedgeGeometry as a render prism (slope, back, two sides).</summary>
        static void Wedge(MeshKit kit, WedgeSpec w, Color top, Color side)
        {
            Vector3[] v = System.Array.ConvertAll(ArenaMap.WedgeCorners(w), WebSpace.ToUnity);
            Vector3 lowL = v[0], lowR = v[1], highL = v[2], highR = v[3], topL = v[4], topR = v[5];
            Vector3 slope = Vector3.Cross(lowR - lowL, topL - lowL).normalized;
            if (slope.y < 0) slope = -slope;
            kit.Quad(lowL, lowR, topR, topL, slope, top);
            Vector3 back = (highL + highR) * 0.5f - (lowL + lowR) * 0.5f;
            back.y = 0;
            kit.Quad(highL, highR, topR, topL, back.normalized, side);
            Vector3 across = (lowR - lowL).normalized;
            kit.Triangle(lowL, highL, topL, -across, side);
            kit.Triangle(lowR, highR, topR, across, side);
        }

        /// <summary>A flat ring (a band of quads) at center c, outer radius r.</summary>
        /// <summary>NIGHT40 L2 (view only): n40-iceline's stone colours (UnityTuning.Ice40*) while design.n40.iceline40 is on, no NIGHT40 off switch is
        /// given (N40Blocks.ForceOff, or any -skygrindOff19..40 on the command line: PlayerChecks sets ForceOff only after the first map is built), and
        /// the map is n40-iceline. Every other case: config.colors.pillarTop / pillarSide, as before.</summary>
        static bool Ice40Look(GameConfig cfg, SurvivalMap map)
        {
            if (N40Blocks.ForceOff || !N40Blocks.From(cfg).iceline40) return false;
            if (map == null || map.json == null || !map.json.TryGetValue("name", out object n) || !(n is string name) || name != "n40-iceline") return false;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int o = 19; o <= 40; o++) if (System.Array.IndexOf(args, "-skygrindOff" + o) >= 0) return false;
            return true;
        }

        public static void Ring(MeshKit kit, Vector3 c, float r, float band, float height, int segments = 40)
        {
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2 / segments, a1 = (i + 1) * Mathf.PI * 2 / segments;
                Vector3 o0 = c + new Vector3(Mathf.Cos(a0) * r, height, Mathf.Sin(a0) * r), o1 = c + new Vector3(Mathf.Cos(a1) * r, height, Mathf.Sin(a1) * r);
                Vector3 i0 = c + new Vector3(Mathf.Cos(a0) * (r - band), height, Mathf.Sin(a0) * (r - band)), i1 = c + new Vector3(Mathf.Cos(a1) * (r - band), height, Mathf.Sin(a1) * (r - band));
                kit.Quad(i0, o0, o1, i1, Vector3.up, Color.white);
            }
        }
    }

    /// <summary>Marks a pad's ring so the game can hide it while the pad sleeps (its target zone is locked).</summary>
    public sealed class PadMarker : MonoBehaviour
    {
        public int index;
    }

    /// <summary>The Grinder's pillars as one stone mesh, rebuilt when a height changes (interpolated per frame).</summary>
    public sealed class CoreMesh
    {
        readonly CoreGrid grid;
        readonly MeshKit kit = new MeshKit();
        readonly Mesh mesh = new Mesh();
        readonly Color top, side;
        readonly double minHeight;
        readonly float[] drawn;

        public CoreMesh(GameConfig cfg, CoreGrid grid, Transform parent, Material stone)
        {
            this.grid = grid;
            top = Look.C(cfg, "colors.pillarTop");
            side = Look.C(cfg, "colors.pillarSide");
            minHeight = cfg.movement.core.minHeight;
            drawn = new float[grid.Count];
            for (int i = 0; i < drawn.Length; i++) drawn[i] = -1;
            MeshKit.Object("Grinder core", parent, mesh, stone);
            Render(1);
        }

        public void Render(double alpha)
        {
            bool changed = false;
            for (int i = 0; i < grid.Count; i++)
            {
                float h = (float)(grid.PreviousHeight(i) + (grid.Height(i) - grid.PreviousHeight(i)) * alpha);
                if (h != drawn[i]) changed = true;
                drawn[i] = h;
            }
            if (!changed) return;
            kit.Clear();
            CoreSpec s = grid.spec;
            for (int i = 0; i < grid.Count; i++)
            {
                if (drawn[i] <= minHeight) continue;
                int k = i % s.cells, r = i / s.cells;
                double x0 = s.x0 + k * s.cellSize, z0 = s.z0 + r * s.cellSize;
                kit.Box(new Vec3(x0, 0, z0), new Vec3(x0 + s.cellSize, drawn[i], z0 + s.cellSize), top, side, skipBottom: true);
            }
            kit.Build("grinder core", mesh);
        }
    }

    /// <summary>The gate fields of every locked zone (zones.js applyLocks: one merged field mesh), rebuilt on a lock change.</summary>
    public sealed class ZonesView
    {
        readonly Zones zones;
        readonly MeshKit kit = new MeshKit();
        readonly Mesh mesh = new Mesh();
        int version = -1;

        public ZonesView(Zones zones, Transform parent, Material field)
        {
            this.zones = zones;
            MeshKit.Object("Gates (locked zones)", parent, mesh, field, shadows: false);
            Render();
        }

        public void Render()
        {
            if (zones.Version == version) return;
            version = zones.Version;
            kit.Clear();
            foreach (GateQuad q in zones.GateQuads())
            {
                kit.Quad(WebSpace.ToUnity(q.c0), WebSpace.ToUnity(q.c1), WebSpace.ToUnity(q.c2), WebSpace.ToUnity(q.c3), WebSpace.ToUnity(q.normal), Color.white);
            }
            kit.Build("gates", mesh);
        }
    }
}
