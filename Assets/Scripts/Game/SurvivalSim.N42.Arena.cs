// NIGHT42 builder K1 (arena42; G12): the arena elements in the sim (the rule: Core/Arena42.cs; the numbers: design.n42.arena42; the level data:
// `grinders42` and `cover42` in a level JSON, L1's n42-foundry). With arena42 off, or a level with neither key: nothing is built, nothing subscribed,
// nothing stepped (NIGHT41's run bit for bit).
// - Run start: the elements from map.json; the grinders' nav mask OR-ed into NavGrid.hazard (ai33's lava mask, if any, kept; SetBlocked re-masks);
//   the cover boxes as the world layer "arena42Cover" (they stop the player, the enemies and every ray).
// - Every step (after N41Step): the edge guard (a walker the player did not knock, whose feet crossed into a grinder's rect this step low enough to
//   fall in, goes back to its feet at the step's start with no horizontal speed), then the grinders (Arena42.StepGrinders), then the cover layer
//   rebuilt when a box broke.
// - A player shot that ends on a standing cover box (`shotResult`, surface world) takes one hit off it (Arena42.Hit).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT42 K1: the run's arena elements (null with arena42 off or a level without them).</summary>
        public Arena42 arena42;
        Arena42 arena42Built;
        readonly BoxLayer arena42Layer = new BoxLayer();
        readonly List<(Vec3 min, Vec3 max)> arena42Scratch = new List<(Vec3 min, Vec3 max)>();
        bool arena42Hooked, arena42LayerSet;
        System.Func<double, bool> arena42Hurt;
        System.Action<double> arena42Knock;
        System.Func<Enemy, double, Vec3, bool> arena42Kill;
        System.Func<Enemy, bool> arena42Knocked;

        /// <summary>Checks only (Tools/SimHarness `n42arena`; the Player reel `arena42`, handed back after it): force the arena42 bit for every run started
        /// (null: the run's own). Never set by the game's own code.</summary>
        public static bool? Force42Arena;

        partial void N42ArenaStartRun()
        {
            if (Force42Arena.HasValue) n42.arena42 = Force42Arena.Value;
            arena42 = null;
            if (arena42LayerSet)
            {
                world.SetLayer("arena42Cover", null);
                arena42LayerSet = false;
            }
            if (!n42.arena42) return;
            arena42Built ??= new Arena42(Arena42Params.From(cfg), step);
            arena42Built.Load(map.json);
            if (!arena42Built.Any) return;
            arena42 = arena42Built;
            arena42.Reset();
            arena42Hurt ??= amount => health.Damage(amount, Arena42.PlayerSource);
            arena42Knock ??= vy => mover.Launch(vy); // the jump pad's path (Lava30's): ends a dash / slide / ride, airborne; no Mover edit
            arena42Kill ??= (e, amount, from) => loadout.Damage(e, amount, false, Arena42.Weapon, from, false, 1, 0); // the traps' kill path (Traps.DamageBy)
            arena42Knocked ??= Arena42Knocked;
            if (!arena42Hooked)
            {
                arena42Hooked = true;
                events.On<ShotEvent>("shotResult", Arena42Shot); // only ever subscribed with the row on and a level with elements (off: the bus is NIGHT41's)
            }
            // the grinders' nav mask, OR-ed into ai33's (lava) mask
            byte[] mine = arena42.NavMask(nav.grid);
            if (mine != null)
            {
                byte[] h = nav.grid.hazard;
                if (h == null || h.Length != mine.Length)
                {
                    nav.grid.hazard = mine;
                    nav.grid.hazardGoalLine = cfg.Design("n33.ai33.goalLine"); // ai33's goal seeds: no walker parks on the far rim staring at the player
                }
                else for (int i = 0; i < h.Length; i++) if (mine[i] != 0) h[i] = 1;
                if (nav.grid.hazardGoalLine) nav.grid.hazardLink = nav.grid.HazardLinks();
                nav.SetBlocked(zones.LockedTag);
            }
            Arena42Layer();
        }

        void Arena42Layer()
        {
            arena42.FillLayer(arena42Layer, arena42Scratch);
            world.SetLayer("arena42Cover", arena42Layer.Count > 0 ? arena42Layer : null, true);
            arena42LayerSet = arena42Layer.Count > 0;
        }

        partial void N42ArenaStep(double dt)
        {
            if (arena42 == null) return;
            if (n34HoldWorld) return; // NIGHT34 B: a held world step (the split bend) holds the world's systems
            if (arena42.grinders.Count > 0)
            {
                Arena42Guard();
                arena42.StepGrinders(mover.position, health.max, flow == Flow.Playing, arena42Hurt, arena42Knock, enemies.all, null, arena42Kill, arena42Knocked, events);
            }
            if (arena42.coverDirty) Arena42Layer();
        }

        /// <summary>The body went in knocked or in the air: the player's push (knockback still running), a launch (Launch36, the fan), or a fall (a husk
        /// off the ground and not in its own leap: the edge guard stops a walker at the rim, so a body falling past it was pushed).</summary>
        static bool Arena42Knocked(Enemy e)
        {
            if (e.knockSteps > 0) return true;
            if (e is Husk h) return h.launched || (!h.grounded && !h.leaping && !(h is Drone));
            return false;
        }

        /// <summary>The edge guard: a walker the player did not knock (no knockback, not launched) whose feet entered a grinder's guard band this step
        /// (within guardMargin m of the rect, at most guardAbove m over its floor: Arena42.InGuard) goes back to its feet at the step's start, with no
        /// horizontal speed.</summary>
        void Arena42Guard()
        {
            List<Enemy> all = enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (!e.alive || e.knockSteps > 0 || !(e is Husk h) || h.launched || h is Drone) continue;
                if (!arena42.InGuard(e.position) || arena42.InGuard(e.previous)) continue;
                h.position = e.previous;
                h.velocity.x = 0;
                h.velocity.z = 0;
                arena42.spared++;
            }
        }

        void Arena42Shot(ShotEvent ev)
        {
            if (arena42 == null || arena42.cover.Count == 0 || ev.owner != "player" || ev.surface != "world") return;
            int i = arena42.CoverAt(ev.end);
            if (i >= 0) arena42.Hit(i, mover.position, enemies.all, arena42Kill, step, events);
        }
    }
}
