// NIGHT26 builder B (contract glacier): THE GLACIER in the sim (design.n26.bosses.glacier; Core/Enemies/Glacier26.cs). A campaign boss room (plan room `boss: "glacier"`,
// SurvivalSim.Boss25.cs routes BossRoomBegin / BossRoomSystem here) on an endless straight of ICE: tile k of Design/maps/glacier-course.json sits at z = -40 k from the room's
// origin; a sliding WINDOW of tiles around the player is one world box layer ("glacierRoom"), rebuilt when the player's tile changes (the tiles behind the player recycle ahead:
// no scrolling world, doubles, no drift). The ice and the boost strips are this room's own Surfaces26 (M's class, design.n26.surfaces numbers), stepped here. Nothing runs
// with the row off. No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Glacier glacier26;
        public GlacierCourse glacier26Course;
        public Vec3 glacier26Origin;
        readonly BoxLayer glacier26Layer = new BoxLayer();
        readonly List<(Vec3 min, Vec3 max)> glacier26Boxes = new List<(Vec3, Vec3)>();
        Surfaces26 glacier26Surf;
        int glacier26First = -999, glacier26Last = -999;
        public int glacier26Retries;
        public bool? glacier26Override;
        public bool Glacier26On => glacier26Override ?? (BossRoomsOn && n26.level2 && cfg.Has("design.n26.bosses.glacier.course.paths"));
        /// <summary>The tiles in the world layer now (the view draws the same window).</summary>
        public int Glacier26First => glacier26First;
        public int Glacier26Last => glacier26Last;

        void Glacier26StartRun()
        {
            if (glacier26 == null) return;
            glacier26.Clear();
            enemies.all.Remove(glacier26.body);
            world.SetLayer("glacierRoom", null);
            glacier26Surf = null;
            glacier26First = glacier26Last = -999;
            if (mover != null && surfaces26 == null) mover.groundFrictionMult = 1;
        }

        void Glacier26Step(double dt)
        {
            if (glacier26 == null) return;
            if (!glacier26.Alive)
            {
                enemies.all.Remove(glacier26.body);
                if (glacier26Surf != null)
                {
                    glacier26Surf = null;
                    if (surfaces26 == null) mover.groundFrictionMult = 1;
                }
                return;
            }
            glacier26.Step(dt);
            if (!BossOn) bossKit.Step(dt);
            glacier26.WindowFor(glacier26.playerS, out int f, out int l);
            if (f != glacier26First || l != glacier26Last) Glacier26Window(f, l);
            if (glacier26Surf != null && n26.surfaces) glacier26Surf.Step(mover);
        }

        void Glacier26Window(int first, int last)
        {
            glacier26First = first;
            glacier26Last = last;
            double len = glacier26.p.tileLen;
            glacier26Course.Window(first, last, len, glacier26Origin, glacier26Boxes);
            glacier26Layer.SetBoxes(glacier26Boxes);
            if (glacier26Surf == null) return;
            // the boost strips of the window's tiles (the ice zone stays)
            glacier26Surf.zones.RemoveAll(z => z.kind == SurfaceKind.Boost);
            for (int k = System.Math.Max(0, first); k <= last; k++)
                foreach (double[] b in glacier26Course.boosts)
                    glacier26Surf.zones.Add(new SurfaceZone { kind = SurfaceKind.Boost, x0 = glacier26Origin.x + b[0], z0 = glacier26Origin.z - len * k + b[3], x1 = glacier26Origin.x + b[2], z1 = glacier26Origin.z - len * k + b[1], dirX = 0, dirZ = -1 });
        }

        public Glacier Glacier26Ensure()
        {
            if (glacier26 != null) return glacier26;
            if (string.IsNullOrEmpty(Wyrm24Dir)) return null;
            string course = Path.Combine(Wyrm24Dir, cfg.Str("design.n26.bosses.glacier.course.paths"));
            if (!File.Exists(course)) return null;
            double[] o = cfg.Nums("design.n26.bosses.glacier.room.origin");
            glacier26Origin = new Vec3(o[0], o[1], o[2]);
            glacier26Course = GlacierCourse.Load(course);
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
            glacier26 = new Glacier(cfg, GlacierParams.From(cfg), events, target, enemies.NextId);
            glacier26.playerBody = () => mover.Body;
            glacier26.teleport = v => Respawn(v, 0);
            return glacier26;
        }

        /// <summary>The player's spawn: the course's start, or the last checkpoint on a retry.</summary>
        public Vec3 Glacier26Spawn => glacier26 == null ? default : (glacier26.checkpoint > 0 ? glacier26.CheckpointSpawn : glacier26Course.spawn + glacier26Origin);

        public bool Glacier26Begin()
        {
            if (!Glacier26On || bossKit == null || Glacier26Ensure() == null) return false;
            if (glacier26.Alive)
            {
                glacier26Retries++;
                if (glacier26.RestorePhaseStart() || glacier26.phase == "dying") return true;
            }
            glacier26.arenaCenter = glacier26Origin;
            glacier26.NewRun(runSeed ^ (uint)cfg.Num("design.n26.bosses.glacier.room.salt"));
            // the ice: one zone over the whole width and length; the boost strips follow the window
            glacier26Surf = new Surfaces26(SurfaceRules.From(cfg));
            glacier26Surf.zones.Add(new SurfaceZone { kind = SurfaceKind.Ice, x0 = glacier26Origin.x - 14, x1 = glacier26Origin.x + 14, z0 = glacier26Origin.z - 1e7, z1 = glacier26Origin.z + 20 });
            glacier26Surf.Reset(mover);
            Glacier26Window(0, glacier26.p.winAhead);
            world.SetLayer("glacierRoom", glacier26Layer, true);
            bossKit.bosses["glacier"] = glacier26;
            if (!enemies.all.Contains(glacier26.body)) enemies.all.Add(glacier26.body);
            bossKit.Start("glacier", 0, 0, null, glacier26.p.name);
            return true;
        }
    }
}
