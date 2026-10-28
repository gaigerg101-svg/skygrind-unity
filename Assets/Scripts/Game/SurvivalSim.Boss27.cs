// NIGHT27 builder B (boss1 / boss2): THE LANTERN and THE GLACIER (pulse-only chase) in the sim. Implements the lead's declaration-only partials N27BossStartRun / N27BossStep
// (SurvivalSim.N27.cs). A campaign boss room asks for them through SurvivalSim.Boss25.cs's BossRoomBegin: with n27.boss1 on the plan's boss id "hulk" starts THE LANTERN (the
// Hulk stays on file), with n27.boss2 on the id "glacier" starts the NIGHT27 chase. Nothing here runs with the rows off. No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // --- THE LANTERN -------------------------------------------------------------------------------------------------------------------

        public Lantern27 lantern27;
        public WyrmArena lantern27Arena;
        public Vec3 lantern27Origin;
        readonly BoxLayer lantern27Layer = new BoxLayer();
        readonly List<(Vec3 min, Vec3 max)> lantern27Boxes = new List<(Vec3, Vec3)>();
        /// <summary>Boss-room retries that found the lantern alive (the harness and the scenario print it).</summary>
        public int lantern27Retries;
        /// <summary>The harness forces the row for one sim (null: the config's switch via n27; false: the RED run).</summary>
        public bool? lantern27Override;
        /// <summary>The wall segments in the world layer now (the view draws the same set).</summary>
        public int Lantern27Mask => lantern27 == null ? 0 : lantern27.retractedMask;
        /// <summary>The pulse's kill-pip sink (builder W's PulseCharge27 implements it; the lead wires it once per run).</summary>
        public IPulseKillSink27 pulseKillSink27;

        public bool Lantern27On => lantern27Override ?? (BossRoomsOn && n27.boss1 && cfg.Has("design.n27.boss1.file"));

        public Vec3 Lantern27Spawn => lantern27Arena == null ? default : lantern27Arena.spawn + lantern27Origin;

        partial void N27BossStartRun()
        {
            if (lantern27 != null)
            {
                lantern27.Clear();
                enemies.all.Remove(lantern27.body);
                world.SetLayer("lanternRoom", null);
            }
            if (glacier27 != null)
            {
                glacier27.Clear();
                enemies.all.Remove(glacier27.body);
                world.SetLayer("glacier27Room", null);
                glacier27Surf = null;
                rink28 = null; // NIGHT28
                glacier27First = glacier27Last = -999;
                if (mover != null && surfaces26 == null) mover.groundFrictionMult = 1;
            }
            Glacier28StartRun(); // NIGHT28 B2 (SurvivalSim.Glacier28.cs)
        }

        partial void N27BossStep(double dt)
        {
            if (lantern27 != null)
            {
                if (lantern27.Alive)
                {
                    lantern27.Step(dt);
                    if (!BossOn) bossKit.Step(dt);
                }
                else enemies.all.Remove(lantern27.body);
            }
            Glacier27Step(dt);
            Glacier28Step(dt); // NIGHT28 B2 (SurvivalSim.Glacier28.cs)
        }

        void Lantern27Rebuild()
        {
            if (Lantern30RingFill(lantern27Layer)) return; // NIGHT30 B (design.n30.lantern30): the ring of flat panels replaces the courtyard (SurvivalSim.Boss30.cs; false with the row off)
            lantern27Boxes.Clear();
            int mask = lantern27 == null ? 0 : lantern27.retractedMask;
            foreach (var (min, max, kind) in lantern27Arena.boxes)
            {
                if (kind.Length == 4 && kind.StartsWith("seg") && (mask & (1 << (kind[3] - '0'))) != 0) continue;
                lantern27Boxes.Add((min + lantern27Origin, max + lantern27Origin));
            }
            lantern27Layer.SetBoxes(lantern27Boxes);
        }

        /// <summary>Build the lantern and its courtyard once (the row's file found); null when it cannot be built.</summary>
        public Lantern27 Lantern27Ensure()
        {
            if (lantern27 != null) return lantern27;
            if (string.IsNullOrEmpty(Wyrm24Dir)) return null;
            string arena = Path.Combine(Wyrm24Dir, cfg.Str("design.n27.boss1.file"));
            if (!File.Exists(arena)) return null;
            double[] o = cfg.Nums("design.n27.boss1.room.origin");
            lantern27Origin = new Vec3(o[0], o[1], o[2]);
            lantern27Arena = WyrmArena.Load(arena);
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
            lantern27 = new Lantern27(cfg, Lantern27Params.From(cfg), events, target, enemies.NextId);
            lantern27.playerBody = () => mover.Body;
            lantern27.playerRiding = () => mover.state == MoveState.WallRide;
            lantern27.addEnemy = e => { if (!enemies.all.Contains(e)) enemies.all.Add(e); };
            lantern27.removeEnemy = e => enemies.all.Remove(e);
            lantern27.walls = Lantern27Rebuild;
            return lantern27;
        }

        /// <summary>Start the fight (or, with a live lantern, retry it: back to the START of its phase, the same stream). false: the row is off, the file is missing or no
        /// BossKit exists yet (no run has started).</summary>
        public bool Lantern27Begin()
        {
            if (!Lantern27On || bossKit == null || Lantern27Ensure() == null) return false;
            lantern27.pulseSink = pulseKillSink27;
            if (lantern27.Alive)
            {
                lantern27Retries++;
                if (lantern27.RestorePhaseStart() || lantern27.phase == "dying") return true;
            }
            lantern27.arenaCenter = lantern27Origin;
            lantern27.NewRun(runSeed ^ (uint)cfg.Num("design.n27.boss1.room.salt"));
            Lantern27Rebuild();
            world.SetLayer("lanternRoom", lantern27Layer, true);
            bossKit.bosses["lantern"] = lantern27;
            if (!enemies.all.Contains(lantern27.body)) enemies.all.Add(lantern27.body);
            bossKit.Start("lantern", 0, 0, null, lantern27.p.name);
            return true;
        }
    }
}
