// NIGHT20 U3-34.6 (builder N owns this file): design.routes in the run (Core/Nav/RouteLinks.cs). At a run's start
// (N20RoutesStartRun, from N20StartRun): with the block on, the extra-link layer (built once, on the first run that wants
// it, from the map's exported grid and its static collider: world.Geometry, what the grid was built over) is swapped into
// the nav grid (NavField.SetRoutes: its links after the web's, its reach, the fallback seeds), the husk pool runs the
// layer's jumper (the fixed leap, the landing stagger, the climbs) and the brain asks it; with the block off the web's
// grid, the web's HuskNavJumper and no brain hook (NIGHT19's nav, bit for bit). N20RoutesStep has nothing to do yet: the
// fields and the jumper step with the brain and the pool.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The map's route layer (null until a run with design.routes on builds it).</summary>
        public RouteLayer routeLayer;
        RouteJumper routeJumper;
        NavJumper webJumper; // the pool's HuskNavJumper (the web's nav jumps)

        // NIGHT25 (builder F, design.n25.hitchFix): the first run start spent 138 ms (cold; ~0 warm) building the route layer. PrebuildRoutes25 builds the SAME
        // layer at boot (the Game calls it from Prewarm24.Start); the first run start takes it when the map and the nav grid are the ones it was built over, and
        // builds as before when they are not. Build reads the cfg, the grid, the map's grid JSON and the static collider and touches no stream: the sim's state is
        // identical (FingerprintTests / Hitch25Tests).
        RouteLayer prebuiltRoutes25;
        string prebuiltRoutesMap25;
        NavGrid prebuiltRoutesGrid25;

        /// <summary>Build the route layer now (boot) when the block is on by default and no layer exists; returns true if one was built.</summary>
        public bool PrebuildRoutes25()
        {
            if (routeLayer != null || prebuiltRoutes25 != null || !n20Design.routes) return false;
            prebuiltRoutes25 = RouteLayer.Build(cfg, nav.grid, map.navGrid, world.Geometry, map.name);
            prebuiltRoutesMap25 = map.name;
            prebuiltRoutesGrid25 = nav.grid;
            return true;
        }

        RouteLayer TakePrebuiltRoutes25()
        {
            RouteLayer l = prebuiltRoutes25;
            bool same = l != null && prebuiltRoutesMap25 == map.name && ReferenceEquals(prebuiltRoutesGrid25, nav.grid);
            prebuiltRoutes25 = null;
            return same ? l : null;
        }

        partial void N20RoutesStartRun()
        {
            bool on = n20.routes;
            webJumper ??= husks.jumper;
            if (on && routeLayer == null) routeLayer = TakePrebuiltRoutes25() ?? RouteLayer.Build(cfg, nav.grid, map.navGrid, world.Geometry, map.name);
            if (on && routeJumper == null) routeJumper = new RouteJumper(cfg, events, webJumper, routeLayer);
            if (routeLayer != null) nav.SetRoutes(routeLayer, on);
            husks.jumper = on ? routeJumper : webJumper;
            brain.routes = on ? routeJumper : null;
        }
    }
}
