// NIGHT20 (the shared commit places this empty fixture; N owns it): the climb (design.routes): a DANGER tell climbTell s before the climb, climbSpeed m/s, a mantle; a punch knocks it off; the climb event's phases (start, up, top, knocked); the leap's approach point and its collider check (no leap into a ceiling); the field seeds the nearest reached node under you (the director always spawns on the 24 m spire).
// U3-34.6 steps 2 and 5 (builder N). The husk's leap (Husk.cs:242-251 on the web's rules: launched at the player's feet
// from wherever it stands, >= 1.5 m above and <= 10 m across, every 3 s, any height, no check) with design.routes on:
// a DANGER crouch (the climb event's "start", linkKind Link.Jump) leapTell s before it launches ("up"), the arc swept
// through the collider before it goes (no leap into a ceiling, none through the wall's lip), at most leapMaxRise m up; from
// a wall's foot (where the field's seeds park it) it backs off to an approach point leapApproachMin-Max m out with a clear
// arc. A long drop (more than the web's 5 m) lands with a dropStagger s stand ("land"). On a perch no ground husk can
// reach (the 24 m spire tops), the field seeds the nearest reached floor, so the director's spawns keep a route.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class ClimbTests
    {
        static GameConfig cfg;

        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static int Steps(string path) => MovementMath.StepsFor(Cfg.Num(path), Cfg.Dt);

        /// <summary>A box world with the web's nav grid built over it (NavGridBuilder, as the exporter builds a map's), the
        /// brain, the husk pool and, with `routes`, the NIGHT20 layer, its jumper and its field.</summary>
        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly List<ClimbEvent> climbs = new List<ClimbEvent>();
            public readonly List<int> climbSteps = new List<int>();
            public int leaps, step;
            public Health health;
            public Husks husks;
            public NavField nav;
            public HuskBrain brain;
            public RouteLayer layer;
            public RouteJumper jumper;

            public Rig()
            {
                world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            }

            public Rig Build(bool routes)
            {
                Dictionary<string, object> gridJson = NavGridBuilder.Build(Cfg, world, 60, null, "flat");
                nav = new NavField(Cfg, NavGrid.FromExport(gridJson));
                brain = new HuskBrain(Cfg, events, nav, world) { onScreen = _ => true };
                health = new Health(100000, events, "player");
                husks = new Husks(Cfg, events, enemies, world, -30) { jumper = new HuskNavJumper(Cfg, events) };
                if (routes)
                {
                    layer = RouteLayer.Build(Cfg, nav.grid, gridJson, world, "test");
                    nav.SetRoutes(layer, true);
                    jumper = new RouteJumper(Cfg, events, husks.jumper, layer);
                    husks.jumper = jumper;
                    brain.routes = jumper;
                }
                events.On<ClimbEvent>(N20Events.Climb, e =>
                {
                    climbs.Add(new ClimbEvent { enemy = e.enemy, kind = e.kind, foot = e.foot, lip = e.lip, phase = e.phase, linkKind = e.linkKind });
                    climbSteps.Add(step);
                });
                events.On("huskLeap", _ => leaps++);
                return this;
            }

            public Husk Spawn(Vec3 at, Vec3 player, bool withBrain = true)
            {
                Husk h = husks.Spawn(at, player);
                Assert.That(h, Is.Not.Null, $"a husk fits at {at}");
                if (withBrain) brain.Attach(h);
                return h;
            }

            public void Step(Vec3 player, bool withBrain = true)
            {
                step++;
                if (withBrain) brain.Step(Cfg.Dt, player, 0, 1, false);
                husks.Step(player, health, Cfg.Dt);
                if (withBrain) brain.Separate(husks);
            }

            public int Index(string phase, int from = 0)
            {
                for (int i = from; i < climbs.Count; i++) if (climbs[i].phase == phase) return i;
                return -1;
            }
        }

        [Test]
        public void TheLeap_FromAWallsFoot_BacksOffToAnApproachPoint_ThenLands()
        {
            // A 5.5 m ledge (over climbMax: a husk can't climb it; under leapMaxRise: it may leap it), its face at z = 10.
            var r = new Rig();
            r.world.AddBox(new Vec3(-12, 0, 10), new Vec3(12, 5.5, 22));
            r.Build(true);
            var player = new Vec3(0, 5.5, 12.5);
            Husk h = r.Spawn(new Vec3(0, 0, 9.4), player);
            double launchedAt = double.NaN;
            int upStep = -1;
            for (int i = 0; i < 10 * 120 && !(h.grounded && h.position.y > 5.4 && h.position.z > 10.2); i++)
            {
                r.Step(player);
                if (upStep < 0 && r.Index("up") >= 0)
                {
                    upStep = r.step;
                    launchedAt = 10 - h.position.z;
                }
            }
            Console.WriteLine($"landed y {h.position.y:0.00} at z {h.position.z:0.00} after {r.step / 120.0:0.00} s; launched {launchedAt:0.00} m from the face; climb events {r.climbs.Count}, huskLeap {r.leaps}");
            Assert.That(h.grounded && h.position.y > 5.4 && h.position.z > 10.2, Is.True, "the husk lands on the ledge");
            int start = r.Index("start"), up = r.Index("up");
            Assert.That(start, Is.GreaterThanOrEqualTo(0).And.LessThan(up), "a DANGER crouch, then the launch");
            Assert.That(r.climbs[start].linkKind, Is.EqualTo(Link.Jump), "the leap's climb events carry Link.Jump");
            // The crouch's first step emits "start" (as the web's nav jump counts its tell): leapTell steps crouched in all.
            Assert.That(r.climbSteps[up] - r.climbSteps[start], Is.GreaterThanOrEqualTo(Steps("design.routes.leapTell") - 1), "the crouch lasts leapTell");
            Assert.That(launchedAt, Is.GreaterThanOrEqualTo(Cfg.Num("design.routes.leapApproachMin") - 0.6), "it launched from an approach point, not the wall's foot");
            Assert.That(r.leaps, Is.EqualTo(1), "one leap (huskLeap, the web's event, at the launch)");
        }

        [Test]
        public void TheLeap_NeverGoesIntoACeiling()
        {
            // A 4 x 4 m slab 3 m up; the player on it, the husk under it: the old leap launched straight into the slab.
            var r = new Rig();
            r.world.AddBox(new Vec3(-2, 3, -2), new Vec3(2, 3.4, 2));
            r.Build(true);
            var player = new Vec3(0.3, 3.4, 0);
            Husk h = r.Spawn(new Vec3(-0.6, 0, 0), player);
            int underLaunch = 0;
            for (int i = 0; i < 8 * 120 && !(h.grounded && h.position.y > 3.3); i++)
            {
                int before = r.Index("up") >= 0 ? 1 : 0;
                r.Step(player);
                bool launchedNow = before == 0 && r.Index("up") >= 0;
                if (launchedNow && Math.Abs(h.position.x) < 2.4 && Math.Abs(h.position.z) < 2.4) underLaunch++;
            }
            Console.WriteLine($"husk at ({h.position.x:0.00}, {h.position.y:0.00}, {h.position.z:0.00}) after {r.step / 120.0:0.00} s; climb events {r.climbs.Count}");
            Assert.That(underLaunch, Is.EqualTo(0), "no launch from under the slab");
            Assert.That(h.grounded && h.position.y > 3.3, Is.True, "it went out from under and leapt onto the slab");
        }

        [Test]
        public void TheLeap_NeverRisesPastLeapMaxRise()
        {
            // A 10 m tower: over leapMaxRise. The web's leap fires anyway (any height); with the layer it doesn't.
            foreach (bool routes in new[] { false, true })
            {
                var r = new Rig();
                r.world.AddBox(new Vec3(-3, 0, -3), new Vec3(3, 10, 3));
                r.Build(routes);
                var player = new Vec3(0, 10, 0);
                Husk h = r.Spawn(new Vec3(5, 0, 0), player);
                for (int i = 0; i < 5 * 120; i++) r.Step(player);
                Console.WriteLine($"routes {routes}: huskLeap {r.leaps}, climb events {r.climbs.Count}, y {h.position.y:0.00}");
                if (routes) Assert.That(r.leaps + r.climbs.FindAll(c => c.phase == "up").Count, Is.EqualTo(0), "no leap up 10 m");
                else Assert.That(r.leaps, Is.GreaterThan(0), "the web's rule: it leaps at any height");
            }
        }

        [Test]
        public void ALongDrop_LandsWithAStagger_AShortOneDoesNot()
        {
            foreach (double height in new[] { 8.0, 3.0 })
            {
                var r = new Rig();
                r.world.AddBox(new Vec3(-5, 0, -5), new Vec3(5, height, 5));
                r.Build(true);
                var player = new Vec3(14, 0, 0);
                Husk h = r.Spawn(new Vec3(4.3, height, 0), player, false);
                int stagger = 0, down = 0;
                for (int i = 0; i < 4 * 120 && down < 30; i++)
                {
                    r.Step(player, false);
                    if (h.grounded && h.position.y < 0.5)
                    {
                        down++;
                        stagger = Math.Max(stagger, h.staggerSteps);
                    }
                }
                int land = r.Index("land");
                Console.WriteLine($"{height} m: stagger {stagger} steps, land events {(land >= 0 ? 1 : 0)}");
                if (height > Cfg.Num("ai.grid.drop"))
                {
                    Assert.That(land, Is.GreaterThanOrEqualTo(0), "a 'land' climb event");
                    Assert.That(r.climbs[land].linkKind, Is.EqualTo(Link.LongDrop));
                    Assert.That(stagger, Is.GreaterThanOrEqualTo(Steps("design.routes.dropStagger") - 2), "the landing stagger");
                }
                else
                {
                    Assert.That(land, Is.EqualTo(-1), "a drop the web's grid has: no stagger");
                    Assert.That(stagger, Is.EqualTo(0));
                }
            }
        }

        /// <summary>Spawn `kind` with the brain on a rig; step until it stands on the ledge (y over `top`) or `seconds` run out.</summary>
        static Husk ClimbUp(Rig r, string kind, Vec3 at, Vec3 player, double top, double seconds, List<double> ys = null)
        {
            Husk h = r.husks.Spawn(kind, at, player);
            Assert.That(h, Is.Not.Null, $"a {kind} fits at {at}");
            r.brain.Attach(h);
            for (int i = 0; i < seconds * 120 && !(h.grounded && h.position.y > top); i++)
            {
                r.Step(player);
                ys?.Add(h.position.y);
            }
            return h;
        }

        [Test]
        public void AHusk_ClimbsA4mFace_TellThenClimbThenMantle()
        {
            // U3-34.6 step 3: a 4 m ledge (over the web's 2.2 m jump), the player on it. The husk walks to the face, shows a
            // DANGER tell (climbTell), climbs at climbSpeed, mantles (mantle) and stands on top; the tell starts >= 0.5 s
            // before the top (the reel's climb scenario reads it).
            var r = new Rig();
            r.world.AddBox(new Vec3(-12, 0, 10), new Vec3(12, 4, 22));
            r.Build(true);
            var player = new Vec3(0, 4, 14);
            var ys = new List<double>();
            Husk h = ClimbUp(r, "husk", new Vec3(0, 0, 3), player, 3.9, 10, ys);
            int start = r.Index("start"), up = r.Index("up"), top = r.Index("top");
            Console.WriteLine($"husk at ({h.position.x:0.00}, {h.position.y:0.00}, {h.position.z:0.00}) after {r.step / 120.0:0.00} s; events {string.Join(" ", r.climbs.ConvertAll(c => c.phase + ":" + c.linkKind))}; leaps {r.leaps}");
            Assert.That(h.grounded && h.position.y > 3.9 && h.position.z > 10.2, Is.True, "it stands on the ledge");
            Assert.That(start >= 0 && up > start && top > up, Is.True, "start, up, top in order");
            Assert.That(r.climbs[start].linkKind, Is.EqualTo(Link.Climb));
            Assert.That(r.leaps, Is.EqualTo(0), "it climbed (the field's link), it didn't leap");
            Assert.That(r.climbSteps[up] - r.climbSteps[start], Is.GreaterThanOrEqualTo(Steps("design.routes.climbTell") - 1), "the tell");
            double climbTime = (r.climbSteps[top] - r.climbSteps[up]) / 120.0;
            double expect = 4 / Cfg.Num("design.routes.climbSpeed") + Cfg.Num("design.routes.mantle");
            Assert.That(climbTime, Is.EqualTo(expect).Within(0.15), "the climb at climbSpeed, then the mantle");
            Assert.That((r.climbSteps[top] - r.climbSteps[start]) / 120.0, Is.GreaterThanOrEqualTo(0.5), "the tell >= 0.5 s before the top");
            Assert.That(r.climbs[up].foot.z, Is.LessThan(10).And.GreaterThan(9), "the foot against the face");
        }

        [Test]
        public void APunch_KnocksAClimberOff_AndItStandsWhereItLands()
        {
            var r = new Rig();
            r.world.AddBox(new Vec3(-12, 0, 10), new Vec3(12, 4.5, 22));
            r.Build(true);
            var player = new Vec3(0, 4.5, 14);
            Husk h = r.Spawn(new Vec3(0, 0, 3), player);
            bool punched = false;
            int stagger = 0;
            for (int i = 0; i < 10 * 120; i++)
            {
                r.Step(player);
                if (!punched && h.jump != null && h.jump.verb == NavJumpState.Climb && h.position.y > 2)
                {
                    h.Knockback(0, -1, 6, 0.2, Cfg.Dt); // a punch from above: away from the face
                    punched = true;
                }
                if (punched && h.grounded && h.position.y < 0.5) stagger = Math.Max(stagger, h.staggerSteps);
                if (punched && stagger > 0 && i > 0 && h.staggerSteps == 0) break;
            }
            Console.WriteLine($"events {string.Join(" ", r.climbs.ConvertAll(c => c.phase))}; stagger {stagger} steps");
            Assert.That(punched, Is.True, "it was climbing");
            Assert.That(r.Index("knocked"), Is.GreaterThanOrEqualTo(0), "a 'knocked' climb event");
            Assert.That(stagger, Is.GreaterThanOrEqualTo(Steps("design.routes.dropStagger") - 2), "it stands where it lands");
        }

        [Test]
        public void ASwarmer_Scales9m_AHuskAndAChargerDont()
        {
            // A 9 m wall: over climbMax and leapMaxRise for a husk, under swarmerClimbMax (CLIMB HIGH: swarmers only).
            var r = new Rig();
            r.world.AddBox(new Vec3(-12, 0, 10), new Vec3(12, 9, 22));
            r.Build(true);
            var player = new Vec3(0, 9, 14);
            Husk s = ClimbUp(r, "swarmer", new Vec3(-2, 0, 3), player, 8.9, 12);
            Console.WriteLine($"swarmer at ({s.position.x:0.00}, {s.position.y:0.00}, {s.position.z:0.00}); events {string.Join(" ", r.climbs.ConvertAll(c => c.phase + ":" + c.linkKind))}");
            Assert.That(s.grounded && s.position.y > 8.9, Is.True, "the swarmer is up");
            Assert.That(r.climbs.Exists(c => c.phase == "up" && c.linkKind == Link.ClimbHigh && c.kind == "swarmer"), Is.True);
            var r2 = new Rig();
            r2.world.AddBox(new Vec3(-12, 0, 10), new Vec3(12, 9, 22));
            r2.Build(true);
            Husk h = ClimbUp(r2, "husk", new Vec3(2, 0, 3), player, 8.9, 8);
            Husk c = ClimbUp(r2, "charger", new Vec3(-3, 0, 3), player, 8.9, 1);
            Assert.That(h.position.y, Is.LessThan(1), "a husk can't");
            Assert.That(c.position.y, Is.LessThan(1), "a charger never climbs");
            Assert.That(r2.climbs.Exists(e => e.phase == "up"), Is.False);
        }

        [Test]
        public void AHusk_ClimbsAnAuthoredLadder_UpA12mTower()
        {
            // A 12 m tower (no generic link: over swarmerClimbMax) with a ladder authored on its south face: the husk walks to
            // it, tells, climbs at ladderSpeed and mantles onto the top (the climb events carry Link.Authored).
            var r = new Rig();
            r.world.AddBox(new Vec3(-4, 0, 10), new Vec3(4, 12, 18));
            var list = new List<AuthoredLink> { new AuthoredLink { a = new Vec3(0.5, 0, 9.5), b = new Vec3(0.5, 12, 10.5), verb = "ladder", why = "test" } };
            Dictionary<string, object> gridJson = NavGridBuilder.Build(Cfg, r.world, 60, null, "flat");
            r.nav = new NavField(Cfg, NavGrid.FromExport(gridJson));
            r.layer = RouteLayer.Build(Cfg, r.nav.grid, gridJson, r.world, "test", list);
            Assert.That(r.layer.rejected, Is.Empty, string.Join("; ", r.layer.rejected));
            Assert.That(r.layer.counts[Link.Authored], Is.EqualTo(2), "a ladder is two links: up it, and down (a walk-off)");
            r.nav.SetRoutes(r.layer, true);
            r.brain = new HuskBrain(Cfg, r.events, r.nav, r.world) { onScreen = _ => true };
            r.health = new Health(100000, r.events, "player");
            r.husks = new Husks(Cfg, r.events, r.enemies, r.world, -30) { jumper = new HuskNavJumper(Cfg, r.events) };
            r.jumper = new RouteJumper(Cfg, r.events, r.husks.jumper, r.layer);
            r.husks.jumper = r.jumper;
            r.brain.routes = r.jumper;
            r.events.On<ClimbEvent>(N20Events.Climb, e =>
            {
                r.climbs.Add(new ClimbEvent { enemy = e.enemy, kind = e.kind, foot = e.foot, lip = e.lip, phase = e.phase, linkKind = e.linkKind });
                r.climbSteps.Add(r.step);
            });
            var player = new Vec3(0, 12, 14);
            Husk h = ClimbUp(r, "husk", new Vec3(0, 0, 2), player, 11.9, 15);
            int up = r.Index("up"), top = r.Index("top");
            Console.WriteLine($"husk at ({h.position.x:0.00}, {h.position.y:0.00}, {h.position.z:0.00}) after {r.step / 120.0:0.00} s; events {string.Join(" ", r.climbs.ConvertAll(c => c.phase + ":" + c.linkKind))}");
            Assert.That(h.grounded && h.position.y > 11.9, Is.True, "on top of the tower");
            Assert.That(up >= 0 && top > up && r.climbs[up].linkKind == Link.Authored, Is.True);
            double expect = 12 / Cfg.Num("design.routes.ladderSpeed") + Cfg.Num("design.routes.mantle");
            Assert.That((r.climbSteps[top] - r.climbSteps[up]) / 120.0, Is.EqualTo(expect).Within(0.2), "up the ladder at ladderSpeed, then the mantle");
        }

        [Test]
        public void APerch_RaisesTheMeleeAndSwarmTokens()
        {
            // Row 4's R3 (THE CLIMB's perch tests: one-file climbers into a standing gun): with the route layer, while you
            // stand where no ground walker reaches on the web's own links (a climb, a ladder or a leap is needed), the melee
            // and swarm pools grow by design.routes.perchTokens; on the ground they are the web's.
            var r = new Rig();
            r.world.AddBox(new Vec3(-12, 0, 10), new Vec3(12, 4, 22));
            r.Build(true);
            int baseMelee = Cfg.Int("ai.tokens.base.melee"), baseSwarm = Cfg.Int("ai.tokens.base.swarm"), extra = Cfg.Int("design.routes.perchTokens");
            Assert.That(extra, Is.GreaterThan(0));
            r.brain.Step(Cfg.Dt, new Vec3(0, 0, 0), 0, 1, false);
            Assert.That((r.brain.pools.sizes["melee"], r.brain.pools.sizes["swarm"]), Is.EqualTo((baseMelee, baseSwarm)), "on the ground: the web's pools");
            r.brain.Step(Cfg.Dt, new Vec3(0, 4, 14), 0, 1, false);
            Assert.That((r.brain.pools.sizes["melee"], r.brain.pools.sizes["swarm"]), Is.EqualTo((baseMelee + extra, baseSwarm + extra)), "on the 4 m ledge (a climb up): more attackers");
            var off = new Rig();
            off.world.AddBox(new Vec3(-12, 0, 10), new Vec3(12, 4, 22));
            off.Build(false);
            off.brain.Step(Cfg.Dt, new Vec3(0, 4, 14), 0, 1, false);
            Assert.That(off.brain.pools.sizes["melee"], Is.EqualTo(baseMelee), "the layer off: the web's pools");
        }

        [Test]
        public void TheSpireTop_IsNoDeadPerch()
        {
            // ARENA's south-west spire top (24 m, x and z -48..-32): no ground husk reaches any node within the seed radius,
            // so on the web's field every spawn node is at Infinity (Director.PickNode spawns nothing, husks go "lost").
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            var events = new EventBus();
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(Cfg.movement));
            var zones = new Zones(Cfg, map, world, events);
            zones.OpenAll();
            var player = new Vec3(-40.5, 24, -40.5);
            foreach (bool routes in new[] { false, true })
            {
                var nav = new NavField(Cfg, NavGrid.FromExport(map.navGrid));
                nav.SetBlocked(zones.LockedTag);
                if (routes) nav.SetRoutes(RouteLayer.Build(Cfg, nav.grid, map.navGrid, world.Geometry, map.name), true);
                nav.UpdateNow(player.x, player.y, player.z);
                Assert.That(nav.goal, Is.GreaterThanOrEqualTo(0), "the spire top is a node");
                int ground = 0, routed = 0;
                foreach (Vec3 s in map.groundSpawns)
                {
                    if (s.y > 0.5) continue;
                    ground++;
                    if (nav.DistanceAt(s.x, s.y, s.z) < double.PositiveInfinity) routed++;
                }
                Console.WriteLine($"routes {routes}: {routed} of {ground} ground spawns have a route");
                if (routes) Assert.That(routed, Is.EqualTo(ground), "every ground spawn routes to the floor under the spire");
                else Assert.That(routed, Is.EqualTo(0), "the web's field: a dead perch");
            }
        }
    }
}
