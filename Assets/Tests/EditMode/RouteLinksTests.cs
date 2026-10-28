// NIGHT20 (the shared commit places this empty fixture; N owns it): the extra-link layer (design.routes, Core/Nav/RouteLinks.cs): long drops <= 12 m; climb links 2.2-5 m (husks, spitters) and <= 10 m (swarmers), never chargers; per-kind masks; the cost 1 + speed x time; validated by flying a small mover; the web's grid export and the flowField golden vectors untouched (the layer off).
// U3-34.6 step 1 (builder N): the LONG DROP links on the real maps (Survival = the web's ARENA export, DRYDOCK = the Unity
// map): every walker may step off a ledge 5-12 m high (L4D's DeathDrop is ~10 m) where the web's grid stops at 5 m; the
// layer keeps every web link, in its order, and adds its own after them; off, the grid is the web's arrays themselves.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class RouteLinksTests
    {
        static GameConfig cfg;
        static SurvivalMap survival, drydock;

        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static SurvivalMap Survival => survival ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));

        static SurvivalMap Drydock => drydock ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "..", "Design", "maps", "drydock.json"))));

        static (NavGrid grid, RouteLayer layer) Build(SurvivalMap map)
        {
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(Cfg.movement));
            NavGrid grid = NavGrid.FromExport(map.navGrid);
            return (grid, RouteLayer.Build(Cfg, grid, map.navGrid, world.Geometry, map.name));
        }

        /// <summary>Nodes that can walk / jump / drop (over `mask`'s link kinds) to a ground node (y <= groundY).</summary>
        static int CanGetDown(NavGrid g, int mask, double groundY)
        {
            var down = new bool[g.count];
            var queue = new Queue<int>();
            for (int n = 0; n < g.count; n++)
            {
                if (g.y[n] <= groundY)
                {
                    down[n] = true;
                    queue.Enqueue(n);
                }
            }
            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                for (int r = g.revStart[v]; r < g.revStart[v + 1]; r++)
                {
                    int u = g.revFrom[r];
                    if (down[u] || !Link.Usable(g.linkKind[g.revLink[r]], mask)) continue;
                    down[u] = true;
                    queue.Enqueue(u);
                }
            }
            int k = 0;
            foreach (bool b in down) if (b) k++;
            return k;
        }

        [Test]
        public void TheLinkKinds_AndTheMasks()
        {
            Assert.That(new[] { Link.LongDrop, Link.Climb, Link.ClimbHigh, Link.Authored }, Is.EqualTo(new byte[] { 4, 5, 6, 7 }));
            // The web's kinds keep their meaning in every mask: a jumper takes all four, a walker (the CHARGER) walk and drop.
            for (byte k = 0; k < 4; k++)
            {
                Assert.That(Link.Usable(k, Link.MaskJumper), Is.True, $"jumper, kind {k}");
                Assert.That(Link.Usable(k, Link.MaskWalker), Is.EqualTo(k == Link.Walk || k == Link.Drop), $"walker, kind {k}");
            }
            Assert.That(Link.Usable(Link.LongDrop, Link.MaskWalker) && Link.Usable(Link.LongDrop, Link.MaskJumper) && Link.Usable(Link.LongDrop, Link.MaskSwarmer), Is.True, "every walker drops");
            Assert.That(Link.Usable(Link.Climb, Link.MaskWalker) || Link.Usable(Link.ClimbHigh, Link.MaskWalker) || Link.Usable(Link.Authored, Link.MaskWalker), Is.False, "chargers never climb");
            Assert.That(Link.Usable(Link.Climb, Link.MaskJumper) && Link.Usable(Link.Authored, Link.MaskJumper), Is.True, "husks and spitters climb (<= climbMax) and take the authored ladders");
            Assert.That(Link.Usable(Link.ClimbHigh, Link.MaskJumper), Is.False, "only swarmers climb past climbMax");
            Assert.That(Link.Usable(Link.ClimbHigh, Link.MaskSwarmer), Is.True);
        }

        [Test]
        public void TheCosts_AreOnePlusSpeedTimesTime()
        {
            var p = RouteParams.From(Cfg);
            // A 10 m drop: fall sqrt(2h / g) at the husk's gravity (GravityFor(1.5 m, 0.36 s) = 23.1 m/s2), then the stagger.
            double g = MovementMath.GravityFor(Cfg.movement.jump.height, Cfg.movement.jump.apexTime);
            double drop10 = 1 + Cfg.Num("husk.speed") * (Math.Sqrt(2 * 10 / g) + Cfg.Num("design.routes.dropStagger"));
            Assert.That(RouteCost.LongDrop(10, p), Is.EqualTo(drop10).Within(1e-9));
            Assert.That(RouteCost.LongDrop(10, p), Is.EqualTo(8.7).Within(0.05), "the research's 10 m drop: 8.7");
            // A 5 m climb: the tell, 5 / climbSpeed, the mantle.
            double climb5 = 1 + Cfg.Num("husk.speed") * (Cfg.Num("design.routes.climbTell") + 5 / Cfg.Num("design.routes.climbSpeed") + Cfg.Num("design.routes.mantle"));
            Assert.That(RouteCost.Climb(5, p, false), Is.EqualTo(climb5).Within(1e-9));
            Assert.That(RouteCost.Climb(5, p, false), Is.EqualTo(14.3).Within(0.05), "the research's 5 m climb: 14.3");
        }

        [Test]
        public void TheLayerOff_IsTheWebsGrid()
        {
            var (grid, layer) = Build(Survival);
            int[] linkTo = grid.linkTo, linkStart = grid.linkStart, revFrom = grid.revFrom;
            int version = grid.version;
            layer.Apply(grid, false);
            Assert.That(ReferenceEquals(grid.linkTo, linkTo) && ReferenceEquals(grid.linkStart, linkStart) && ReferenceEquals(grid.revFrom, revFrom), Is.True, "off: the web's arrays, untouched");
            Assert.That(grid.version, Is.EqualTo(version), "off from off: no recompute");
            layer.Apply(grid, true);
            Assert.That(grid.linkTo.Length, Is.GreaterThan(linkTo.Length), "on: the layer's links");
            Assert.That(grid.version, Is.Not.EqualTo(version), "a swap restarts the fields");
            layer.Apply(grid, false);
            Assert.That(ReferenceEquals(grid.linkTo, linkTo) && ReferenceEquals(grid.linkStart, linkStart) && ReferenceEquals(grid.revFrom, revFrom), Is.True, "back off: the web's arrays again");
        }

        [Test]
        public void TheLayerOn_KeepsEveryWebLink_InItsOrder()
        {
            var (grid, layer) = Build(Survival);
            int[] bStart = grid.linkStart, bTo = grid.linkTo;
            byte[] bKind = grid.linkKind;
            float[] bCost = grid.linkCost;
            layer.Apply(grid, true);
            int extra = 0;
            for (int a = 0; a < grid.count; a++)
            {
                int n = bStart[a + 1] - bStart[a];
                for (int i = 0; i < n; i++)
                {
                    int w = bStart[a] + i, o = grid.linkStart[a] + i;
                    Assert.That((grid.linkTo[o], grid.linkKind[o], grid.linkCost[o]), Is.EqualTo((bTo[w], bKind[w], bCost[w])), $"node {a}'s web link {i}");
                }
                for (int o = grid.linkStart[a] + n; o < grid.linkStart[a + 1]; o++)
                {
                    Assert.That(grid.linkKind[o], Is.GreaterThanOrEqualTo(Link.LongDrop), "the layer's links come after the web's");
                    extra++;
                }
            }
            Assert.That(extra, Is.EqualTo(grid.linkTo.Length - bTo.Length));
        }

        [Test]
        public void Build_IsDeterministic()
        {
            var (g1, l1) = Build(Survival);
            var (g2, l2) = Build(Survival);
            l1.Apply(g1, true);
            l2.Apply(g2, true);
            Assert.That(g1.linkTo, Is.EqualTo(g2.linkTo));
            Assert.That(g1.linkKind, Is.EqualTo(g2.linkKind));
            Assert.That(g1.linkCost, Is.EqualTo(g2.linkCost));
            Assert.That(g1.revLink, Is.EqualTo(g2.revLink));
        }

        [Test]
        public void EveryLongDrop_FallsBetweenTheWebsDropAndLongDrop_WithRoomToLand()
        {
            var p = RouteParams.From(Cfg);
            foreach (SurvivalMap map in new[] { Survival, Drydock })
            {
                var (grid, layer) = Build(map);
                layer.Apply(grid, true);
                var clear = PortData.Arr(PortData.Obj(map.navGrid["nodes"])["clear"]);
                int drops = 0;
                for (int a = 0; a < grid.count; a++)
                {
                    for (int li = grid.linkStart[a]; li < grid.linkStart[a + 1]; li++)
                    {
                        if (grid.linkKind[li] != Link.LongDrop) continue;
                        drops++;
                        int b = grid.linkTo[li];
                        double fall = (double)grid.y[a] - grid.y[b]; // the floats widened first (as the layer reads them)
                        Assert.That(fall, Is.GreaterThan(Cfg.Num("ai.grid.drop")).And.LessThanOrEqualTo(Cfg.Num("design.routes.longDrop") + 1e-6), $"{map.name} node {a} -> {b}");
                        int ca = grid.col[a], cb = grid.col[b];
                        int di = Math.Abs(ca % grid.size - cb % grid.size), dk = Math.Abs(ca / grid.size - cb / grid.size);
                        Assert.That(di + dk, Is.EqualTo(1), "orthogonal neighbours");
                        Assert.That(PortData.Num(clear[b]), Is.GreaterThanOrEqualTo(fall + Cfg.Num("ai.grid.bodyHeight") - 1e-4), "the fall's column is clear");
                        Assert.That((double)grid.linkCost[li], Is.EqualTo((double)(float)RouteCost.LongDrop(fall, p)), "1 + speed x (fall + stagger)");
                    }
                }
                Console.WriteLine($"{map.name}: {drops} long drops");
                Assert.That(drops, Is.GreaterThan(50), $"{map.name} has long drops");
            }
        }

        /// <summary>Nodes above `above` m a ground walker reaches over `mask`'s link kinds (forward from y <= groundY).</summary>
        static (int reached, int total) ReachedAbove(NavGrid g, int mask, double groundY, double above)
        {
            var seen = new bool[g.count];
            var queue = new Queue<int>();
            for (int n = 0; n < g.count; n++)
            {
                if (g.y[n] > groundY) continue;
                seen[n] = true;
                queue.Enqueue(n);
            }
            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                for (int li = g.linkStart[v]; li < g.linkStart[v + 1]; li++)
                {
                    int u = g.linkTo[li];
                    if (seen[u] || !Link.Usable(g.linkKind[li], mask)) continue;
                    seen[u] = true;
                    queue.Enqueue(u);
                }
            }
            int r = 0, t = 0;
            for (int n = 0; n < g.count; n++)
            {
                if (g.y[n] <= above) continue;
                t++;
                if (seen[n]) r++;
            }
            return (r, t);
        }

        [Test]
        public void EveryClimb_RisesUpAFace_WithinItsKindsLimit()
        {
            // U3-34.6 step 3: husks and spitters climb (jumpUp, climbMax] (L4D's ClimbUpHeight ~5.1 m), swarmers to
            // swarmerClimbMax (a tall climb, CLIMB HIGH), up a vertical face from the foot (on the lower floor, against the
            // face) to the lip (the upper node), each flown through the collider at build.
            var p = RouteParams.From(Cfg);
            foreach (SurvivalMap map in new[] { Survival, Drydock })
            {
                var (grid, layer) = Build(map);
                layer.Apply(grid, true);
                int climbs = 0, high = 0;
                for (int a = 0; a < grid.count; a++)
                {
                    for (int li = grid.linkStart[a]; li < grid.linkStart[a + 1]; li++)
                    {
                        byte k = grid.linkKind[li];
                        if (k != Link.Climb && k != Link.ClimbHigh) continue;
                        int b = grid.linkTo[li];
                        double rise = (double)grid.y[b] - grid.y[a];
                        if (k == Link.Climb)
                        {
                            climbs++;
                            Assert.That(rise, Is.GreaterThan(Cfg.Num("ai.grid.jumpUp")).And.LessThanOrEqualTo(Cfg.Num("design.routes.climbMax") + 1e-6), $"{map.name} climb {a} -> {b}");
                        }
                        else
                        {
                            high++;
                            Assert.That(rise, Is.GreaterThan(Cfg.Num("design.routes.climbMax")).And.LessThanOrEqualTo(Cfg.Num("design.routes.swarmerClimbMax") + 1e-6), $"{map.name} tall climb {a} -> {b}");
                        }
                        int ca = grid.col[a], cb = grid.col[b];
                        Assert.That(Math.Abs(ca % grid.size - cb % grid.size) + Math.Abs(ca / grid.size - cb / grid.size), Is.EqualTo(1), "orthogonal neighbours");
                        Assert.That((double)grid.linkCost[li], Is.EqualTo((double)(float)RouteCost.Climb(rise, p, k == Link.ClimbHigh)), "1 + speed x (tell + rise / climbSpeed + mantle)");
                        Assert.That(layer.LinkAt(li, out RouteLink rl), Is.True);
                        Assert.That(JsMath.Hypot(rl.foot.x - grid.NodeX(a), rl.foot.z - grid.NodeZ(a)), Is.LessThanOrEqualTo(0.8), "the foot is by the lower column, against the face (a face up to 1.2 m out)");
                        Assert.That(rl.foot.y, Is.EqualTo((double)grid.y[a]));
                        Assert.That((rl.lip.x, rl.lip.y, rl.lip.z), Is.EqualTo((grid.NodeX(b), (double)grid.y[b], grid.NodeZ(b))));
                        Assert.That(JsMath.Hypot(rl.normal.x, rl.normal.z), Is.EqualTo(1).Within(1e-9), "the face's outward normal, horizontal");
                        Assert.That(rl.normal.y, Is.EqualTo(0));
                    }
                }
                Console.WriteLine($"{map.name}: {climbs} climbs, {high} tall climbs (swarmers)");
                Assert.That(climbs, Is.GreaterThan(20), map.name);
            }
        }

        [Test]
        public void Climbs_TakeTheHordeUpTheMidFloors()
        {
            // Research 1.5's what-if (no collider checks): climbing to 5 m lifts ARENA's reach a little (69 -> 71 %) and
            // DRYDOCK's (79 -> 87 %); a swarmer's 10 m lifts ARENA's nodes over 5 m from 0 to 51 %, DRYDOCK's 26 -> 76 %.
            double groundY = Cfg.Num("design.routes.groundY");
            foreach (SurvivalMap map in new[] { Survival, Drydock })
            {
                var (grid, layer) = Build(map);
                var web = ReachedAbove(grid, Link.MaskJumper, groundY, 2.5);
                layer.Apply(grid, true);
                var husk = ReachedAbove(grid, Link.MaskJumper, groundY, 2.5);
                var swarm = ReachedAbove(grid, Link.MaskSwarmer, groundY, 2.5);
                var charger = ReachedAbove(grid, Link.MaskWalker, groundY, 2.5);
                Console.WriteLine($"{map.name}: nodes over 2.5 m reached: web {web.reached} / {web.total}, husks {husk.reached}, swarmers {swarm.reached}, chargers {charger.reached}");
                Assert.That(husk.reached, Is.GreaterThan(web.reached), $"{map.name}: climbs reach more");
                Assert.That(swarm.reached, Is.GreaterThan(husk.reached), $"{map.name}: swarmers climb higher");
                Assert.That(charger.reached, Is.LessThanOrEqualTo(web.reached), $"{map.name}: chargers never climb");
            }
        }

        [Test]
        public void AClimbLink_NeedsAFaceToTheLip_AndRoomToMantle()
        {
            // A 4 m ledge: climb links along its front face (z = 5, facing -z). The same ledge with a lip overhanging 0.6 m:
            // none up the front (the body can't mantle round it; its sides stay climbable). A 3 m step with an 8 m wall set
            // back 2 m behind it: no tall climb from the ground in front to the wall's top through the open air over the step.
            int Climbs(TriangleWorld w)
            {
                Dictionary<string, object> gj = NavGridBuilder.Build(Cfg, w, 40, null, "flat");
                NavGrid g = NavGrid.FromExport(gj);
                RouteLayer l = RouteLayer.Build(Cfg, g, gj, w, "test");
                int k = 0;
                foreach (RouteLink rl in l.links) if ((rl.kind == Link.Climb || rl.kind == Link.ClimbHigh) && rl.normal.z < -0.5 && rl.foot.z < 5) k++;
                return k;
            }
            var plain = new TriangleWorld();
            plain.AddBox(new Vec3(-20, -1, -20), new Vec3(20, 0, 20));
            plain.AddBox(new Vec3(-5, 0, 5), new Vec3(5, 4, 15));
            int along = Climbs(plain);
            var over = new TriangleWorld();
            over.AddBox(new Vec3(-20, -1, -20), new Vec3(20, 0, 20));
            over.AddBox(new Vec3(-5, 0, 5), new Vec3(5, 4, 15));
            over.AddBox(new Vec3(-5, 3.6, 4.4), new Vec3(5, 4, 5)); // the lip overhangs the face by 0.6 m
            int overhang = Climbs(over);
            var stepped = new TriangleWorld();
            stepped.AddBox(new Vec3(-20, -1, -20), new Vec3(20, 0, 20));
            stepped.AddBox(new Vec3(-5, 0, 5), new Vec3(5, 3, 7)); // a 3 m step, 2 m deep
            stepped.AddBox(new Vec3(-5, 0, 7), new Vec3(5, 8, 15)); // the 8 m wall behind it
            Dictionary<string, object> sj = NavGridBuilder.Build(Cfg, stepped, 40, null, "flat");
            NavGrid sg = NavGrid.FromExport(sj);
            RouteLayer sl = RouteLayer.Build(Cfg, sg, sj, stepped, "test");
            int fromGroundToTop = 0;
            foreach (RouteLink l in sl.links) if (l.kind == Link.ClimbHigh && l.foot.y < 0.5 && l.lip.y > 7.5 && l.foot.z < 5) fromGroundToTop++;
            Console.WriteLine($"front climb links: plain ledge {along}, overhung lip {overhang}, ground -> 8 m over the step {fromGroundToTop}");
            Assert.That(along, Is.GreaterThanOrEqualTo(8), "a link per column along the 10 m face");
            Assert.That(overhang, Is.EqualTo(0), "no mantle round an overhang");
            Assert.That(fromGroundToTop, Is.EqualTo(0), "no climb up open air");
        }

        [Test]
        public void EveryLeapUp_RisesPastTheWebsJump_OverAClearArc()
        {
            // U3-34.6 step 4's generic half: ARENA's walkways stand 2.5 m over open ground on a slab (no face to climb, over
            // the web's 2.2 m jump): a LEAP UP link (kind 8, husks / spitters / swarmers, never a charger) where the nav
            // jump's arc (ai.jumps.husk.apex over the higher floor, landing ai.jumpLandIn past the edge) is clear.
            Assert.That(Link.Usable(Link.LeapUp, Link.MaskJumper) && Link.Usable(Link.LeapUp, Link.MaskSwarmer), Is.True);
            Assert.That(Link.Usable(Link.LeapUp, Link.MaskWalker), Is.False, "chargers never leap");
            var (grid, layer) = Build(Survival);
            layer.Apply(grid, true);
            // Row 4: across up to leapUpCells columns (the spire's corner chain has a 1 m gap), and each one's way back down
            // (LEAP DOWN: the same kind, a negative rise) where the reversed arc clears.
            int ups = 0, downs = 0;
            var pairs = new HashSet<(int, int)>();
            for (int a = 0; a < grid.count; a++)
            {
                for (int li = grid.linkStart[a]; li < grid.linkStart[a + 1]; li++)
                {
                    if (grid.linkKind[li] != Link.LeapUp || (layer.LinkAt(li, out RouteLink rl) && rl.authored)) continue; // the generated ones (authored leaps: TheAuthoredLinks)
                    int b = grid.linkTo[li];
                    double rise = (double)grid.y[b] - grid.y[a];
                    int ca = grid.col[a], cb = grid.col[b];
                    int cells = Math.Abs(ca % grid.size - cb % grid.size) + Math.Abs(ca / grid.size - cb / grid.size);
                    Assert.That(cells, Is.InRange(1, (int)Cfg.Num("design.routes.leapUpCells")), "a straight line of columns");
                    Assert.That(Math.Abs(rise), Is.GreaterThan(Cfg.Num("ai.grid.jumpUp")).And.LessThanOrEqualTo(Cfg.Num("design.routes.leapUpMax") + 1e-6));
                    if (rise > 0)
                    {
                        ups++;
                        // As the layer flies it (row 4: the costs honest to the hand check): 1 + speed x (leapTell + the arc's flight).
                        Assert.That(layer.LinkAt(li, out RouteLink up), Is.True);
                        CombatMath.LeapVelocity(up.foot, up.land, Cfg.Num("ai.jumps.husk.apex"), MovementMath.GravityFor(Cfg.movement.jump.height, Cfg.movement.jump.apexTime), out double flight);
                        Assert.That((double)grid.linkCost[li], Is.EqualTo(1 + Cfg.Num("husk.speed") * (Cfg.Num("design.routes.leapTell") + flight)).Within(1e-4), "1 + speed x (the crouch + the flight)");
                    }
                    else
                    {
                        downs++;
                        pairs.Add((b, a));
                    }
                }
            }
            int backed = 0;
            for (int a = 0; a < grid.count; a++)
                for (int li = grid.linkStart[a]; li < grid.linkStart[a + 1]; li++)
                    if (grid.linkKind[li] == Link.LeapUp && grid.y[grid.linkTo[li]] > grid.y[a] && pairs.Contains((a, grid.linkTo[li]))) backed++;
            var walkway = ReachedAbove(grid, Link.MaskJumper, Cfg.Num("design.routes.groundY"), 2.4);
            Console.WriteLine($"survival: {ups} leap-ups, {downs} leap-downs ({backed} ups have their way back); nodes over 2.4 m reached by husks {walkway.reached} / {walkway.total}");
            Assert.That(ups, Is.GreaterThan(10), "the walkways' open edges and the spires' ledge chains");
            Assert.That(downs, Is.EqualTo(backed), "every leap-down is an up's way back");
            Assert.That(backed, Is.GreaterThan(0), "ups with their way back (measured 44 of 114: the others land where a drop or a walk goes down; THE CLIMB's R2b checks every node)");
        }

        [Test]
        public void TheAuthoredLinks_AllPlace_AndReachTheirPerches()
        {
            // U3-34.6 step 4: the perches no generic link reaches get ladders authored in design.routes.authored.<map>, each
            // validated as a climb (a face to the lip, the body flown up and over) at ladderSpeed, up to ladderMax.
            foreach ((SurvivalMap map, double perchY) in new[] { (Survival, 24.0), (Drydock, 11.0) })
            {
                List<AuthoredLink> list = RouteLayer.AuthoredOf(Cfg, map.name);
                var (grid, layer) = Build(map);
                Console.WriteLine($"{map.name}: {list.Count} authored, {layer.counts[Link.Authored]} placed; rejected: {string.Join("; ", layer.rejected)}");
                Assert.That(list.Count, Is.GreaterThan(0), $"{map.name} has authored links");
                Assert.That(layer.rejected, Is.Empty);
                int ladders = list.FindAll(e => e.verb != "leap").Count, leaps = list.Count - ladders;
                Assert.That(layer.counts[Link.Authored], Is.EqualTo(2 * ladders), "each ladder both ways: up it, down it (a walk-off)");
                int leapLinks = layer.links.FindAll(x => x.authored && x.kind == Link.LeapUp).Count;
                Assert.That(leapLinks, Is.InRange(leaps, 2 * leaps), "each authored leap, and back where its arc clears");
                foreach (RouteLink l in layer.links)
                {
                    if (l.kind != Link.Authored) continue;
                    Assert.That(l.authored, Is.True);
                    Assert.That(Math.Abs(l.rise), Is.GreaterThan(0).And.LessThanOrEqualTo(Cfg.Num("design.routes.ladderMax")));
                    double cost = l.rise > 0 ? RouteCost.Ladder(l.rise, RouteParams.From(Cfg)) : RouteCost.LongDrop(-l.rise, RouteParams.From(Cfg));
                    Assert.That((double)l.cost, Is.EqualTo((double)(float)cost));
                }
                layer.Apply(grid, true);
                var seen = new bool[grid.count];
                var queue = new Queue<int>();
                for (int n = 0; n < grid.count; n++) if (grid.y[n] <= Cfg.Num("design.routes.groundY")) { seen[n] = true; queue.Enqueue(n); }
                while (queue.Count > 0)
                {
                    int v = queue.Dequeue();
                    for (int li = grid.linkStart[v]; li < grid.linkStart[v + 1]; li++)
                    {
                        int u = grid.linkTo[li];
                        if (seen[u] || !Flow.Usable(grid.linkKind[li], true)) continue;
                        seen[u] = true;
                        queue.Enqueue(u);
                    }
                }
                int perch = 0, reached = 0;
                for (int n = 0; n < grid.count; n++)
                {
                    if (Math.Abs(grid.y[n] - perchY) > 0.05) continue;
                    perch++;
                    if (seen[n]) reached++;
                }
                Console.WriteLine($"{map.name}: the {perchY} m perches' nodes reached by a husk {reached} / {perch}");
                Assert.That(reached, Is.GreaterThan(perch / 2), $"{map.name}: a husk reaches the {perchY} m perches");
            }
        }

        [Test]
        public void TheAuthoredLinks_PlaceBehindLockedGates()
        {
            // The game builds the layer at a run's start, with every zone but the first locked (blocked nodes): the ladders
            // into locked zones must still place (THE CLIMB's first run found 2 of 6 placed: NavGrid.NodeAt skips the blocked).
            var events = new EventBus();
            var world = new GameWorld(Survival.BuildOctree(), Survival.CreateCore(Cfg.movement));
            var zones = new Zones(Cfg, Survival, world, events);
            NavGrid grid = NavGrid.FromExport(Survival.navGrid);
            grid.SetBlocked(zones.LockedTag);
            int blocked = 0;
            foreach (byte b in grid.blocked) blocked += b;
            RouteLayer layer = RouteLayer.Build(Cfg, grid, Survival.navGrid, world.Geometry, Survival.name);
            Console.WriteLine($"blocked nodes {blocked}; authored placed {layer.counts[Link.Authored]}; rejected [{string.Join("; ", layer.rejected)}]");
            Assert.That(blocked, Is.GreaterThan(0), "a run's start: zones locked");
            Assert.That(layer.rejected, Is.Empty);
            Assert.That(layer.counts[Link.Authored], Is.EqualTo(2 * RouteLayer.AuthoredOf(Cfg, Survival.name).FindAll(e => e.verb != "leap").Count));
        }

        [Test]
        public void LongDrops_LetTheUpperFloorsGetDown()
        {
            // Research/2026-10-06-enemy-routes.md 1.5 (a what-if without the collider checks): ARENA can get down 71 % -> 91 %
            // at 12 m; DRYDOCK 88 % -> 99 %. The real links (the column clearance, a clear step-off) a little lower.
            double groundY = Cfg.Num("design.routes.groundY");
            foreach ((SurvivalMap map, double atLeast) in new[] { (Survival, 0.86), (Drydock, 0.95) })
            {
                var (grid, layer) = Build(map);
                int before = CanGetDown(grid, Link.MaskJumper, groundY);
                layer.Apply(grid, true);
                int after = CanGetDown(grid, Link.MaskJumper, groundY);
                int walker = CanGetDown(grid, Link.MaskWalker, groundY);
                Console.WriteLine($"{map.name}: can get down {before} -> {after} of {grid.count} ({100.0 * before / grid.count:0.0} % -> {100.0 * after / grid.count:0.0} %); the CHARGER {100.0 * walker / grid.count:0.0} %");
                Assert.That((double)after / grid.count, Is.GreaterThanOrEqualTo(atLeast), map.name);
                Assert.That(after, Is.GreaterThan(before));
            }
        }
    }
}
