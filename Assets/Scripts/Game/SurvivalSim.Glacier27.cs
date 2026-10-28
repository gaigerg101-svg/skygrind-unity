// NIGHT27 builder B (boss2): THE GLACIER as the pulse-only chase in the sim (design.n27.boss2; Core/Enemies/Glacier27.cs). A campaign boss room (plan boss id "glacier" with n27.boss2 on:
// SurvivalSim.Boss25.cs's BossRoomBegin routes here) on an endless straight of ICE: tile k of Design/maps/glacier-course.json sits at z = -40 k from the room's origin; a sliding window of tiles
// around the player is one world box layer ("glacier27Room"), rebuilt when the player's tile changes (NIGHT26's Glacier26 does the same; the two rooms have their own origins and layers).
// The ice and the boost strips are this room's own Surfaces26 (design.n26.surfaces numbers). Stepped from Boss27.cs's N27BossStep. Nothing here runs with the row off.
// No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Glacier27 glacier27;
        public GlacierCourse glacier27Course;
        public Vec3 glacier27Origin;
        readonly BoxLayer glacier27Layer = new BoxLayer();
        readonly List<(Vec3 min, Vec3 max)> glacier27Boxes = new List<(Vec3, Vec3)>();
        Surfaces26 glacier27Surf;
        // NIGHT28 (builder A2, design.n28.rink): THE GLACIER's room as a REAL curved rink of triangles (Core/Enemies/Rink28Arena.cs) with ONE ice rule. With the row off nothing here runs and the
        // NIGHT27 straight tile course is exactly as it was; with it on the tile window is never built, the world layer "glacier27Room" holds the rink's triangle soup and glacier27Surf holds the ONE
        // ice zone over the whole rink (no boost strip, no other zone). The boss builder (Glacier28) places the boss with Rink28Math / rink28.BossStartS.
        public Rink28Arena rink28;
        readonly BoxLayer rink28Layer = new BoxLayer();
        public bool? rink28Override;
        public bool Rink28On => rink28Override ?? Rink28Params.Enabled(cfg);
        /// <summary>The glacier room's surface rules (the skate view reads the ice zone from it); null outside the room.</summary>
        public Surfaces26 Glacier27Surf => glacier27Surf;
        int glacier27First = -999, glacier27Last = -999;
        public int glacier27Retries;
        public bool? glacier27Override;
        public bool Glacier27On => glacier27Override ?? (BossRoomsOn && n27.boss2 && cfg.Has("design.n27.boss2.course.paths"));
        public int Glacier27First => glacier27First;
        public int Glacier27Last => glacier27Last;

        void Glacier27Step(double dt)
        {
            if (glacier27 == null) return;
            if (!glacier27.Alive)
            {
                enemies.all.Remove(glacier27.body);
                if (glacier27Surf != null)
                {
                    glacier27Surf = null;
                    if (surfaces26 == null) mover.groundFrictionMult = 1;
                }
                return;
            }
            glacier27.Step(dt);
            if (!BossOn) bossKit.Step(dt);
            glacier27.WindowFor(glacier27.playerS, out int f, out int l);
            if (rink28 == null && (f != glacier27First || l != glacier27Last)) Glacier27Window(f, l);
            if (glacier27Surf != null && n26.surfaces) glacier27Surf.Step(mover);
        }

        void Glacier27Window(int first, int last)
        {
            glacier27First = first;
            glacier27Last = last;
            double len = glacier27.p.tileLen;
            glacier27Course.Window(first, last, len, glacier27Origin, glacier27Boxes);
            glacier27Layer.SetBoxes(glacier27Boxes);
            if (glacier27Surf == null) return;
            glacier27Surf.zones.RemoveAll(z => z.kind == SurfaceKind.Boost);
            for (int k = System.Math.Max(0, first); k <= last; k++)
                foreach (double[] b in glacier27Course.boosts)
                    glacier27Surf.zones.Add(new SurfaceZone { kind = SurfaceKind.Boost, x0 = glacier27Origin.x + b[0], z0 = glacier27Origin.z - len * k + b[3], x1 = glacier27Origin.x + b[2], z1 = glacier27Origin.z - len * k + b[1], dirX = 0, dirZ = -1 });
        }

        public Glacier27 Glacier27Ensure()
        {
            if (glacier27 != null) return glacier27;
            if (string.IsNullOrEmpty(Wyrm24Dir)) return null;
            string course = Path.Combine(Wyrm24Dir, cfg.Str("design.n27.boss2.course.paths"));
            if (!File.Exists(course)) return null;
            double[] o = cfg.Nums("design.n27.boss2.room.origin");
            glacier27Origin = new Vec3(o[0], o[1], o[2]);
            glacier27Course = GlacierCourse.Load(course);
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
            glacier27 = new Glacier27(cfg, Glacier27Params.From(cfg), events, target, enemies.NextId);
            glacier27.playerBody = () => mover.Body;
            glacier27.playerVelocity = () => mover.velocity;
            glacier27.pulseSink = pulseKillSink27;
            glacier27.playerAim = () => Aim().Item2;
            glacier27.pulseReady = () => n27.pulse27 && pulse27 != null ? pulse27.Ready : (fist24 == null || !fist24.Active || fist24.CanFlick); // NIGHT27: with pulse27 on the window opens only on a FULL glove off cooldown
            glacier27.teleport = v => Respawn(v, 0);
            return glacier27;
        }

        /// <summary>The player's spawn: the course's start, or the last checkpoint on a retry.</summary>
        public Vec3 Glacier27Spawn => rink28 != null ? Rink28SpawnPos : glacier27 == null ? default : (glacier27.checkpoint > 0 ? glacier27.CheckpointSpawn : glacier27Course.spawn + glacier27Origin);

        /// <summary>The player's spawn yaw (degrees): the rink's lane direction, or the straight course's.</summary>
        public double Glacier27SpawnYaw => rink28 != null ? Rink28SpawnYaw : glacier27Course.spawnYawDeg;

        Vec3 Rink28SpawnPos { get { rink28.Spawn(out Vec3 p, out _); return p; } }
        double Rink28SpawnYaw { get { rink28.Spawn(out _, out double y); return y; } }

        /// <summary>The rink's room: the arena from the config numbers, its triangles as the world layer, the ONE ice zone (design.n28.rink).</summary>
        void Rink28Room()
        {
            rink28 = Rink28Arena.Build(cfg);
            rink28.FillLayer(rink28Layer);
            glacier27Surf.zones.Clear();
            glacier27Surf.zones.Add(rink28.IceZone());
            glacier27Surf.Reset(mover);
            glacier27First = glacier27Last = -999;
            world.SetLayer("glacier27Room", rink28Layer, true);
        }

        public bool Glacier27Begin()
        {
            if (!Glacier27On || bossKit == null || Glacier27Ensure() == null) return false;
            if (glacier27.Alive)
            {
                glacier27Retries++;
                if (glacier27.RestorePhaseStart() || glacier27.phase == "dying") return true;
            }
            glacier27.pulseSink = pulseKillSink27;
            glacier27.arenaCenter = glacier27Origin;
            glacier27.NewRun(runSeed ^ (uint)cfg.Num("design.n27.boss2.room.salt"));
            glacier27Surf = new Surfaces26(SurfaceRules.From(cfg));
            if (Rink28On) Rink28Room();
            else
            {
                rink28 = null;
                glacier27Surf.zones.Add(new SurfaceZone { kind = SurfaceKind.Ice, x0 = glacier27Origin.x - 14, x1 = glacier27Origin.x + 14, z0 = glacier27Origin.z - 1e7, z1 = glacier27Origin.z + 20 });
                glacier27Surf.Reset(mover);
                Glacier27Window(0, glacier27.p.winAhead);
                world.SetLayer("glacier27Room", glacier27Layer, true);
            }
            bossKit.bosses["glacier"] = glacier27;
            if (!enemies.all.Contains(glacier27.body)) enemies.all.Add(glacier27.body);
            bossKit.Start("glacier", 0, 0, null, glacier27.p.name);
            return true;
        }
    }
}
