// NIGHT26 builder B (contract hulk): THE SLAG HULK in the sim (design.n26.bosses.hulk; Core/Enemies/Hulk26.cs). A campaign boss room (plan room `boss: "hulk"`,
// SurvivalSim.Boss25.cs routes BossRoomBegin / BossRoomSystem / BossRoomArena here) in its own closed room far from the map (Design/maps/hulk-arena.json at
// design.n26.bosses.hulk.room.origin), the room's boxes a world layer (rays on: shots stop at its walls), the hulk in the enemy registry while it fights
// (the shots find it through HulkBody; EnemyHits.Filter applies the part's multiplier). Hooks: Hulk26StartRun / Hulk26Step are called from Boss25.cs's
// N25WyrmStartRun / N25WyrmStep. Nothing here runs with the row off. No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>THE SLAG HULK (null until its first fight).</summary>
        public Hulk hulk26;
        public WyrmArena hulk26Arena;
        public Vec3 hulk26Origin;
        readonly BoxLayer hulk26Layer = new BoxLayer();
        /// <summary>Boss-room retries that found the hulk alive (the phase-start rule's count; the harness and the scenario print it).</summary>
        public int hulk26Retries;
        /// <summary>The harness forces the row for one sim (null: the config's switch; false: the RED run).</summary>
        public bool? hulk26Override;

        /// <summary>design.n26.bosses.hulk.enabled, the boss rooms on, and not forced off (--off26 / --off25 / --off24 turn the boss rooms off already).</summary>
        public bool Hulk26On => hulk26Override ?? (BossRoomsOn && n26.level1 && cfg.Has("design.n26.bosses.hulk.enabled") && cfg.Design("n26.bosses.hulk.enabled"));

        public Vec3 Hulk26Spawn => hulk26Arena == null ? default : hulk26Arena.spawn + hulk26Origin;

        void Hulk26StartRun()
        {
            if (hulk26 == null) return;
            hulk26.Clear();
            enemies.all.Remove(hulk26.body);
            world.SetLayer("hulkRoom", null);
        }

        void Hulk26Step(double dt)
        {
            if (hulk26 == null) return;
            if (hulk26.Alive)
            {
                if (!BossOn)
                {
                    hulk26.Step(dt);
                    bossKit.Step(dt);
                }
            }
            else enemies.all.Remove(hulk26.body);
        }

        /// <summary>Build the hulk and its room once (the row's files found); null when it cannot be built.</summary>
        public Hulk Hulk26Ensure()
        {
            if (hulk26 != null) return hulk26;
            if (string.IsNullOrEmpty(Wyrm24Dir)) return null;
            string paths = Path.Combine(Wyrm24Dir, cfg.Str("design.n26.bosses.hulk.paths")), arena = Path.Combine(Wyrm24Dir, cfg.Str("design.n26.bosses.hulk.arena"));
            if (!File.Exists(paths) || !File.Exists(arena)) return null;
            double[] o = cfg.Nums("design.n26.bosses.hulk.room.origin");
            hulk26Origin = new Vec3(o[0], o[1], o[2]);
            hulk26Arena = WyrmArena.Load(arena);
            var boxes = new List<(Vec3 min, Vec3 max)>();
            foreach (var (min, max, kind) in hulk26Arena.boxes) boxes.Add((min + hulk26Origin, max + hulk26Origin));
            hulk26Layer.SetBoxes(boxes);
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
            hulk26 = new Hulk(cfg, HulkParams.From(cfg, Hulk.LoadBoss(paths)), events, world, target, enemies.NextId);
            hulk26.playerBody = () => mover.Body;
            hulk26.arena = hulk26Arena;
            return hulk26;
        }

        /// <summary>Start the fight (or, with a live hulk, retry it: back to the START of its phase, the same stream). false: the row is off, the files are
        /// missing or no BossKit exists yet (no run has started).</summary>
        public bool Hulk26Begin()
        {
            if (!Hulk26On || bossKit == null || Hulk26Ensure() == null) return false;
            if (hulk26.Alive)
            {
                hulk26Retries++;
                if (hulk26.RestorePhaseStart() || hulk26.phase == "dying") return true;
            }
            hulk26.arenaCenter = hulk26Origin;
            hulk26.NewRun(runSeed ^ (uint)cfg.Num("design.n26.bosses.hulk.room.salt"));
            world.SetLayer("hulkRoom", hulk26Layer, true);
            bossKit.bosses["hulk"] = hulk26;
            if (!enemies.all.Contains(hulk26.body)) enemies.all.Add(hulk26.body);
            bossKit.Start("hulk", 0, 0, null, hulk26.p.name);
            return true;
        }
    }
}
