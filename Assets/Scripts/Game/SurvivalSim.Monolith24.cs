// NIGHT24 leftover 8 (builder B): MONOLITH in the sim (design.n24.monolith; Core/Enemies/Monolith.cs), standalone like the worm: its own flat room
// (Design/maps/monolith-arena.json) placed at design.n24.monolith.room.origin, the room's boxes and the boss's body as world layers, the boss in the
// enemy registry while it fights. Read directly (no N24Blocks bit), gated by --off24. The hooks are inside SurvivalSim.Wyrm24.cs's N24WyrmStartRun /
// N24WyrmStep (one line each). No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>MONOLITH (null until the row is on and its room is found).</summary>
        public Monolith monolith24;
        public WyrmArena monolith24Arena;
        public Vec3 monolith24Origin;
        readonly BoxLayer monolith24Room = new BoxLayer();

        /// <summary>design.n24.monolith.enabled this run, and not --off24.</summary>
        public bool Monolith24On => (!N24Blocks.ForceOff && (monolith24Force || cfg.Design("n24.monolith.enabled"))) || BossRoomsOn; // NIGHT25 W: also under design.n25.bosses

        /// <summary>The harness switches the row on for one sim (the config stays off).</summary>
        public bool monolith24Force;

        public Vec3 Monolith24Spawn => monolith24Arena == null ? default : monolith24Arena.spawn + monolith24Origin;

        void Monolith24StartRun()
        {
            if (monolith24 == null) return;
            monolith24.Clear();
            enemies.all.Remove(monolith24.body);
            world.SetLayer("monolithRoom", null);
            world.SetLayer("monolith", null);
        }

        void Monolith24Step(double dt)
        {
            if (monolith24 == null) return;
            if (monolith24.Alive) monolith24.Step(dt);
            else enemies.all.Remove(monolith24.body);
        }

        public Monolith Monolith24Ensure()
        {
            if (monolith24 != null) return monolith24;
            if (string.IsNullOrEmpty(Wyrm24Dir)) return null;
            string arena = Path.Combine(Wyrm24Dir, cfg.Str("design.n24.monolith.arena"));
            if (!File.Exists(arena)) return null;
            double[] o = cfg.Nums("design.n24.monolith.room.origin");
            monolith24Origin = new Vec3(o[0], o[1], o[2]);
            monolith24Arena = WyrmArena.Load(arena);
            var boxes = new List<(Vec3 min, Vec3 max)>();
            foreach (var (min, max, kind) in monolith24Arena.boxes) boxes.Add((min + monolith24Origin, max + monolith24Origin));
            monolith24Room.SetBoxes(boxes);
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
            monolith24 = new Monolith(cfg, events, world, target, enemies.NextId);
            monolith24.playerBody = () => mover.Body;
            monolith24.playerVelocity = () => mover.velocity;
            return monolith24;
        }

        /// <summary>Start the fight now: the room's layer on, the boss registered, the BossKit's intro card. false: the row is off, the files are missing or
        /// no BossKit exists yet (no run has started).</summary>
        public bool Monolith24Begin()
        {
            if (!Monolith24On || bossKit == null || Monolith24Ensure() == null) return false;
            monolith24.arenaCenter = monolith24Origin;
            monolith24.NewRun(runSeed ^ (uint)monolith24.b.salt);
            world.SetLayer("monolithRoom", monolith24Room, true);
            world.SetLayer("monolith", monolith24.layer, false);
            bossKit.bosses["monolith"] = monolith24;
            if (!enemies.all.Contains(monolith24.body)) enemies.all.Add(monolith24.body);
            bossKit.Start("monolith");
            return true;
        }
    }
}
