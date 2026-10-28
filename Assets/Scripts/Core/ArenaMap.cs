// Arena data shared by the Unity scene builder and the reference TriangleWorld, so both collide against the same
// geometry. The base is the web game's training map (skygrind/src/world/maps/training.js + config.arena), unchanged.
// Spike additions (not in the web training map) are marked: two 20° slide ramps (the Spire's ramp spec), grapple
// nodes (the training map has none), one aimed pad, an L of floor + wall (a concave corner), and a small Grinder core
// (the training map has none; the Spire and Survival do). Collision goes through three's Octree exactly as arena.js
// builds it (ThreeGeometry), so the faces three drops on the outer max-x / max-z planes are dropped here too.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public struct BoxSpec
    {
        public Vec3 min;
        public Vec3 max;
        public string kind; // floor | stone | feature

        public BoxSpec(double x0, double y0, double z0, double x1, double y1, double z1, string kind)
        {
            min = new Vec3(x0, y0, z0);
            max = new Vec3(x1, y1, z1);
            this.kind = kind;
        }
    }

    /// <summary>A slide ramp rising `rise` over `length` along `yaw` (0 = toward −z) from its low-end center (x, z).</summary>
    public struct WedgeSpec
    {
        public double x;
        public double z;
        public double yaw;
        public double length;
        public double width;
        public double rise;
    }

    public struct PadSpec
    {
        public Vec3 pos;
        public bool aimed;
        public Vec3 target;
        public double apexAbove;
    }

    public sealed class ArenaMap
    {
        public readonly List<BoxSpec> boxes = new List<BoxSpec>();
        public readonly List<WedgeSpec> wedges = new List<WedgeSpec>();
        public readonly List<PadSpec> pads = new List<PadSpec>();
        public readonly List<Vec3> grappleNodes = new List<Vec3>();
        /// <summary>Spike addition: an L of a floor slab and a wall (a concave floor/wall corner).</summary>
        public readonly List<BoxSpec> meshPiece = new List<BoxSpec>();
        /// <summary>Spike addition: a small Grinder core (null: none).</summary>
        public CoreSpec? core;
        public Vec3 spawn;
        public double spawnYawDeg;
        public double killPlaneY;
        public double floorSize;

        /// <summary>arena.js wedgeGeometry corners: lowL, lowR, highL, highR, topL, topR.</summary>
        public static Vec3[] WedgeCorners(WedgeSpec w)
        {
            double ax = -JsMath.Sin(w.yaw);
            double az = -JsMath.Cos(w.yaw);
            double rx = JsMath.Cos(w.yaw);
            double rz = -JsMath.Sin(w.yaw);
            double hw = w.width / 2;
            Vec3 P(double along, double across, double y) => new Vec3(w.x + ax * along + rx * across, y, w.z + az * along + rz * across);
            return new[]
            {
                P(0, -hw, 0), P(0, hw, 0),
                P(w.length, -hw, 0), P(w.length, hw, 0),
                P(w.length, -hw, w.rise), P(w.length, hw, w.rise),
            };
        }

        /// <summary>
        /// arena.js checkPads: launches a grounded mover standing on a pad (call after each mover step). Aimed pads
        /// land you on their target; plain pads go straight up to jumpPad.apexHeight.
        /// </summary>
        public bool CheckPads(Mover mover, MovementConfig config)
        {
            if (!mover.grounded || mover.velocity.y > 0) return false;
            JumpPadConfig pad = config.jumpPad;
            Vec3 p = mover.position;
            foreach (PadSpec spot in pads)
            {
                double dx = p.x - spot.pos.x;
                double dz = p.z - spot.pos.z;
                if (dx * dx + dz * dz > pad.radius * pad.radius) continue;
                if (Math.Abs(p.y - spot.pos.y) > pad.triggerHeight) continue;
                double gravity = MovementMath.GravityFor(config.jump.height, config.jump.apexTime);
                if (spot.aimed)
                {
                    // Aimed from where you actually stand on the disc, so any entry lands on the target.
                    Vec3 v = MovementMath.PadLaunchVelocity(new Vec3(p.x, spot.pos.y, p.z), spot.target, spot.apexAbove, gravity, config.jump.fallGravityMult);
                    mover.Launch(v.y, v.x, v.z);
                }
                else
                {
                    mover.Launch(MovementMath.PadLaunchSpeed(pad.apexHeight, gravity));
                }
                return true;
            }
            return false;
        }

        /// <summary>The static collider, as arena.js builds it: three's Octree over the boxes (then the L piece), then the wedges.</summary>
        public ThreeOctree BuildOctree()
        {
            var all = new List<BoxSpec>(boxes);
            all.AddRange(meshPiece);
            return ThreeGeometry.BuildOctree(all, wedges);
        }

        /// <summary>The core at its load preset ('flat', as arena.js load), or null without one.</summary>
        public CoreGrid CreateCore(MovementConfig config)
        {
            if (!core.HasValue) return null;
            var grid = new CoreGrid(config, core.Value);
            grid.SetHeights(CoreGrid.PresetHeights("flat", core.Value.cells, config.core.maxHeight, config.core.heightStep, new Mulberry32(1)));
            return grid;
        }

        /// <summary>The web game's collider for this map (world/collider.js): the static Octree, then the live core.</summary>
        public LayeredWorld BuildReferenceWorld(MovementConfig config, out CoreGrid grid)
        {
            grid = CreateCore(config);
            return new LayeredWorld(BuildOctree(), grid);
        }
    }

    /// <summary>A named start point: feet position and facing (yaw 0 looks toward −z).</summary>
    public struct Spot
    {
        public double x;
        public double y;
        public double z;
        public double yawDeg;

        public Spot(double x, double y, double z, double yawDeg)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.yawDeg = yawDeg;
        }
    }

    public static class TrainingMap
    {
        /// <summary>config.arena.testSpots (the ?debug=1 preset start points), plus the spike's own (marked).</summary>
        public static readonly Dictionary<string, Spot> TestSpots = new Dictionary<string, Spot>
        {
            ["lane"] = new Spot(0, 0, 28, 0),
            ["ledgeEdge"] = new Spot(-11, 1.5, 19, 0), // 1 m from the 1.5 m ledge's south edge
            ["gapStart"] = new Spot(9, 1, 21, -90), // near platform, facing the gap (+x)
            ["tunnelApproach"] = new Spot(-14, 0, 14, 0), // 6 m north of the tunnel mouth
            ["tunnelInside"] = new Spot(-14, 0, 4, 0), // mid-tunnel, under the 1.2 m roof
            ["pillarEdge"] = new Spot(-7.85, 4, -10, 90), // feet 0.15 m past the 4 m stair edge
            ["wall"] = new Spot(27.3, 0, -9, 0), // beside the east wall, facing along it
            ["corridorWallA"] = new Spot(-29.64, 4, -6.4, 0), // mid-air 1 cm from wall A's north end, facing down the corridor
            ["corridorApproach"] = new Spot(-27.5, 0, 2, 15), // north of the corridor, angled toward wall A
            ["deckEdge"] = new Spot(24, 4, -27, 180), // on the slam deck, 1 m from its north edge
            ["padApproach"] = new Spot(2.2, 0, 18.5, 0), // 3 m north of the lane pad
            ["ledgeTop"] = new Spot(-11, 1.5, 20, 180), // on the 1.5 m ledge, facing north (+z)
            ["ledgeHusk"] = new Spot(-11, 0, 26, 0), // on the floor 6 m north of the ledge top
            ["wallFace"] = new Spot(27.6, 0, -18, -90), // eye 0.4 m from the east wall, facing it (+x)
            // Spike additions
            ["ramps"] = new Spot(0, 0, 6, 0), // 8 m north of the slide hill's low end, facing it
            ["meshPiece"] = new Spot(19, 0, 0, -90), // 3 m west of the non-convex L piece, facing it (+x)
            ["towerTop"] = new Spot(18, 6, -18, 135), // on the 6 m tower at col 12, row 3, facing grapple node 5
            ["grinder"] = new Spot(11, 0, 20, 0), // 3 m north of the Grinder core, facing it
            ["grinderCell"] = new Spot(12, 0, 12, 0), // on Grinder cell (3, 3)
        };

        // config.arena
        const double CellSize = 4;
        const double FloorSize = 64;
        const double KillPlaneY = -20;
        const double FloorThickness = 2; // config.core.floorThickness

        // [col, row, height m] on the 16×16 grid. Cols 7–8 (x −4..4) stay clear as a run corridor.
        static readonly int[,] Pillars =
        {
            { 4, 0, 12 }, { 5, 0, 12 }, { 6, 0, 12 }, { 9, 0, 12 }, { 10, 0, 12 }, { 11, 0, 12 }, // back wall, corridor gap
            { 15, 1, 12 }, { 15, 2, 12 }, { 15, 3, 12 }, { 15, 4, 12 }, { 15, 5, 12 }, // 12 m east wall (z −28..−8)
            { 2, 5, 1 }, { 3, 5, 2 }, { 4, 5, 3 }, { 5, 5, 4 }, // stairs
            { 10, 7, 2 }, { 11, 7, 6 }, { 10, 8, 1 }, { 11, 8, 4 }, // cluster
            { 12, 3, 6 }, { 13, 10, 5 }, { 2, 10, 3 }, // towers
        };

        // Test features as boxes [xMin, yMin, zMin, xMax, yMax, zMax] in m.
        static readonly double[,] Features =
        {
            // 20 m lane (z 28 → 8 at x = 0): 1 m posts every 5 m on both sides
            { -3.65, 0, 27.85, -3.35, 1, 28.15 }, { 3.35, 0, 27.85, 3.65, 1, 28.15 },
            { -3.65, 0, 22.85, -3.35, 1, 23.15 }, { 3.35, 0, 22.85, 3.65, 1, 23.15 },
            { -3.65, 0, 17.85, -3.35, 1, 18.15 }, { 3.35, 0, 17.85, 3.65, 1, 18.15 },
            { -3.65, 0, 12.85, -3.35, 1, 13.15 }, { 3.35, 0, 12.85, 3.65, 1, 13.15 },
            { -3.65, 0, 7.85, -3.35, 1, 8.15 }, { 3.35, 0, 7.85, 3.65, 1, 8.15 },
            { -14, 0, 18, -8, 1.5, 22 }, // 1.5 m ledge: a standing jump makes it
            { -22, 0, 18, -16, 1.8, 22 }, // 1.8 m ledge: a single jump should miss
            { 8, 0, 18, 14, 1, 24 }, // double-jump gap, near platform
            { 24, 0, 18, 28, 1, 24 }, // far platform, 10 m gap
            { -16.5, 0, 0, -16, 1.6, 8 }, // tunnel west wall
            { -12, 0, 0, -11.5, 1.6, 8 }, // tunnel east wall
            { -16.5, 1.2, 0, -11.5, 1.6, 8 }, // tunnel roof, underside at 1.2 m
            { -30.5, 0, -26, -30, 10, -6 }, // wall-ride corridor, wall A (20 m long)
            { -25, 0, -26, -24.5, 10, -6 }, // wall-ride corridor, wall B (5 m from A)
            { 20, 0, -32, 28, 4, -26 }, // slam deck: 4 m drop north onto flat floor
            { -15.5, 0, 24, -15, 2.5, 30 }, // a low cover wall
        };

        public static ArenaMap Build()
        {
            var map = new ArenaMap
            {
                spawn = new Vec3(0, 0, 28),
                spawnYawDeg = 0,
                killPlaneY = KillPlaneY,
                floorSize = FloorSize,
            };
            double half = FloorSize / 2;
            map.boxes.Add(new BoxSpec(-half, -FloorThickness, -half, half, 0, half, "floor"));
            double origin = -FloorSize / 2;
            for (int i = 0; i < Pillars.GetLength(0); i++)
            {
                int col = Pillars[i, 0], row = Pillars[i, 1], height = Pillars[i, 2];
                map.boxes.Add(new BoxSpec(origin + col * CellSize, 0, origin + row * CellSize,
                    origin + (col + 1) * CellSize, height, origin + (row + 1) * CellSize, "stone"));
            }
            for (int i = 0; i < Features.GetLength(0); i++)
            {
                map.boxes.Add(new BoxSpec(Features[i, 0], Features[i, 1], Features[i, 2], Features[i, 3], Features[i, 4], Features[i, 5], "feature"));
            }
            // Jump pads [x, y, z] (surface center), plain: straight up to jumpPad.apexHeight.
            map.pads.Add(new PadSpec { pos = new Vec3(2.2, 0, 15.5) }); // on the lane, off the x = 0 run line
            map.pads.Add(new PadSpec { pos = new Vec3(26, 0, -19.5) }); // next to the 12 m east wall, can reach the slam deck

            // --- Spike additions -----------------------------------------------------------------------------------
            // A slide hill in the run corridor: two Spire-spec ramps (8 m, 4 m wide, 20°) back to back, peak at z −10.
            double rise = 8 * JsMath.Tan(20 * Math.PI / 180);
            map.wedges.Add(new WedgeSpec { x = 0, z = -2, yaw = 0, length = 8, width = 4, rise = rise }); // rises toward −z
            map.wedges.Add(new WedgeSpec { x = 0, z = -18, yaw = Math.PI, length = 8, width = 4, rise = rise }); // rises toward +z
            // Grapple nodes over the arena (the training map has none; Spire hangs them 28–34 m up).
            map.grappleNodes.Add(new Vec3(0, 24, -6));
            map.grappleNodes.Add(new Vec3(-16, 22, 10));
            map.grappleNodes.Add(new Vec3(16, 22, 10));
            map.grappleNodes.Add(new Vec3(-16, 22, -18));
            map.grappleNodes.Add(new Vec3(12, 22, -12));
            // One aimed pad: from the corridor floor onto the 6 m tower at col 12, row 3 (x 16..20, z −20..−16).
            map.pads.Add(new PadSpec { pos = new Vec3(6, 0, -6), aimed = true, target = new Vec3(18, 6, -18), apexAbove = 2 });
            // An L: an 8 × 8 m slab (0.5 m) with a 3 m wall along its east edge (a concave floor / wall corner).
            map.meshPiece.Add(new BoxSpec(22, 0, -4, 30, 0.5, 4, "mesh"));
            map.meshPiece.Add(new BoxSpec(29.5, 0.5, -4, 30, 3.5, 4, "mesh"));
            // A small Grinder: 6 × 6 cells of 2 m at x 5..17, z 5..17 (clear of the lane, the cluster and the platforms).
            map.core = new CoreSpec { x0 = 5, z0 = 5, cells = 6, cellSize = 2 };
            return map;
        }
    }
}
