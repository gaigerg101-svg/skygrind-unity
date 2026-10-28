// NIGHT33 builder E (row design.n33.ai33; DESIGN33 E "Lava", A4 (a); GG 2026-10-11: enemies "run into the lava to pass on me"): enemies never walk
// into lava on their own.
// 1. The nav mask: at the run's start every nav node on a lava surface (its column's centre in a lava rect, at most maskAbove m over the surface:
//    a walkway higher up stays open) is OR-ed into NavGrid.SetBlocked through NavGrid.hazard (Core/NavField.cs), then the fields are marked dirty.
//    Every query already skips blocked nodes (NodeAt and so LineWalkable's straight run, the Dijkstra, BestLink, GridPath, Relocate), so no path,
//    no straight "direct" run and no flow-field link crosses lava.
// 2. The edge guard, for what the mask cannot see (the near-goal straight steer at the player, a swarmer's pounce, the CHARGER's 22 m/s dash):
//    Lava30.spare33 is asked before a touch starts a burn; a walker that was NOT knocked by the player (knockSteps 0, not launched) and stood on the
//    bank at the step's start (its `previous` feet outside every lava rect) goes back there with no horizontal speed, and a charging CHARGER is
//    stunned at the edge (its own wall-slam stun). Knockback by the player still throws enemies in (DESIGN33: "that stays fun").
// 3. The crowd: N33AiStep keeps each walker's feet before brain.Separate; N33AfterCrowd puts back a walker the crowd's push carried into a lava
//    rect (it would start the next step inside, where the guard can no longer find a bank).
// Off (the row off, --off33, no lava): no mask (NavGrid.hazard null), no guard (spare33 null): NIGHT32's run bit for bit.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>Harness only (Tools/SimHarness `ai33`): force the ai33 bit for every run this process starts (null: the run's own bits). Never set by the game.</summary>
        public static bool? Force33Ai;

        /// <summary>Harness only (`ai33 --nogoalline`): the round-1 guard without the goal-line seeds (the stall's RED). Never set by the game.</summary>
        public static bool Probe33NoGoalLine;

        const string Ai33Root = "design.n33.ai33.";

        bool ai33On, ai33Hooked;

        /// <summary>The harness's counts this run: lava nav nodes masked; steps the guard put a walker back after the step's moves; spawns moved off the lava.</summary>
        public int ai33Masked, ai33CrowdBacks, ai33SpawnMoves, ai33LinksCut;

        partial void N33AiStartRun()
        {
            if (Force33Ai.HasValue) n33.ai33 = Force33Ai.Value;
            ai33On = n33.ai33 && lava30 != null && lava30.zones.Count > 0;
            ai33Masked = ai33CrowdBacks = ai33SpawnMoves = 0;
            if (ai33On && !ai33Hooked)
            {
                ai33Hooked = true;
                events.On<SpawnEvent>("enemySpawn", Ai33Spawned); // only ever subscribed with the row on (off: the bus is NIGHT32's)
            }
            if (lava30 != null) lava30.spare33 = ai33On ? Ai33Spare : (System.Func<Enemy, int, bool>)null;
            byte[] mask = ai33On ? lava30.NavMask(nav.grid, cfg.Num(Ai33Root + "maskAbove")) : null;
            if (mask != null) for (int i = 0; i < mask.Length; i++) ai33Masked += mask[i];
            if (mask == null && nav.grid.hazard == null) return; // the row off: the web's mask, untouched
            nav.grid.hazard = mask;
            nav.grid.hazardGoalLine = mask != null && !Probe33NoGoalLine && cfg.Design("n33.ai33.goalLine");
            nav.grid.hazardLink = nav.grid.hazardGoalLine ? nav.grid.HazardLinks() : null;
            ai33LinksCut = 0;
            if (nav.grid.hazardLink != null) for (int i = 0; i < nav.grid.hazardLink.Length; i++) ai33LinksCut += nav.grid.hazardLink[i];
            nav.SetBlocked(zones.LockedTag); // re-mask now (and every gate / zone event after ORs the lava in), the fields recompute
        }

        /// <summary>A walker the player did not knock: its own step (or the dash, the pounce) carried it in. Not one already over the lava at the step's start.</summary>
        static bool Ai33Walker(Enemy e, out Husk h)
        {
            h = e as Husk;
            if (h == null || h is Drone || !e.alive) return false;
            return e.knockSteps == 0 && !h.launched;
        }

        bool Ai33InRect(double x, double z)
        {
            for (int i = 0; i < lava30.zones.Count; i++)
            {
                Lava30Zone zn = lava30.zones[i];
                if (x >= zn.x0 && x <= zn.x1 && z >= zn.z0 && z <= zn.z1) return true;
            }
            return false;
        }

        /// <summary>Feet over a lava rect and less than walkwayAbove m over its surface (it falls in from there; a walkway higher up is safe ground).</summary>
        bool Ai33OverLava(Vec3 p)
        {
            double up = cfg.Num(Ai33Root + "walkwayAbove");
            for (int i = 0; i < lava30.zones.Count; i++)
            {
                Lava30Zone zn = lava30.zones[i];
                if (p.x >= zn.x0 && p.x <= zn.x1 && p.z >= zn.z0 && p.z <= zn.z1 && p.y < zn.y + up)
                    return nav.grid.NodeAt(p.x, p.y, p.z, 0.7, 0.7) < 0; // an open (non-lava) nav node under the feet is solid ground: an island, a ledge, the bank's half-cell
            }
            return false;
        }

        /// <summary>Lava30.spare33: put the walker back on the bank it stood on at the step's start (true), or let it burn (false: knocked, or it was over the lava already).</summary>
        bool Ai33Spare(Enemy e, int zone)
        {
            if (!Ai33Walker(e, out Husk h)) return false;
            Vec3 back = e.previous;
            if (Ai33OverLava(back)) return false;
            Ai33Back(h, back);
            return true;
        }

        void Ai33Back(Husk h, Vec3 back)
        {
            h.position = back;
            h.velocity.x = 0;
            h.velocity.z = 0;
            if (h is Charger c && c.state == "charge")
            {
                c.state = "stunned"; // the charger's own wall slam (Charger.cs: hitWall -> stunned for charger.stunTime)
                c.stateSteps = MovementMath.StepsFor(cfg.Num("charger.stunTime"), step);
            }
        }

        /// <summary>A walker that appears over lava (a room's spawn nodes are pooled before the mask: A4 (b)) is moved to the nearest open nav node off the lava.</summary>
        void Ai33Spawned(SpawnEvent ev)
        {
            if (!ai33On) return;
            List<Enemy> all = enemies.all;
            Enemy e = null;
            for (int i = 0; i < all.Count; i++) if (all[i].id == ev.id) { e = all[i]; break; }
            // round 2: not only over the lava but anywhere over a lava rect (an island or a ledge in the pit: spire v6's swarmers spawned on one and were pinned there
            // for the whole fight, pressing at the edge toward the player)
            if (!(e is Husk) || e is Drone || !Ai33InRect(e.position.x, e.position.z)) return;
            NavGrid g = nav.grid;
            for (int r = 2; r <= 16; r += 2)
            {
                int n = g.NearestNode(e.position.x, e.position.y, e.position.z, r);
                if (n < 0) continue;
                Vec3 at = new Vec3(g.NodeX(n), g.y[n], g.NodeZ(n));
                if (Ai33InRect(at.x, at.z)) continue;
                e.position = e.previous = at;
                ai33SpawnMoves++;
                return;
            }
        }


        /// <summary>After every move of the step (its own, a dash, a pounce, the crowd's push): an un-knocked walker that stood off the lava at the step's start and is now over
        /// a lava rect (below a walkway) goes back to where it stood, with no horizontal speed (a charging CHARGER stuns there).</summary>
        partial void N33AfterCrowd(double dt)
        {
            if (!ai33On) return;
            List<Enemy> all = enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (!Ai33Walker(e, out Husk h) || lava30.Burning(e.id)) continue;
                if (!Ai33OverLava(e.position) || Ai33OverLava(e.previous)) continue;
                Ai33Back(h, e.previous);
                ai33CrowdBacks++;
            }
        }
    }
}
