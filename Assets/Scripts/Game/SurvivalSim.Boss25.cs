// NIGHT25 builder W: generic campaign boss rooms (levels 6 and 9 need bosses). A plan room `boss: "wyrm" | "bulwark" | "monolith"` fights that boss in its own
// room far from the map: Campaign24's world hook BeginBoss(id, ...) / BossIsDefeated(id) land here (SurvivalSim.Campaign24.cs CampaignWorld). The worm and MONOLITH
// keep their NIGHT24 files (SurvivalSim.Wyrm24.cs, SurvivalSim.Monolith24.cs); this file adds THE BULWARK's room (Design/maps/bulwark-arena.json at
// design.n25.bosses.bulwark.room.origin: the boss runs there with no hatch adds), the stepping of that fight outside a SURVIVAL boss round, and the phase-start
// retry for the BULWARK and MONOLITH under design.n25.bosses.enabled (the worm's is under design.n25.wyrmAim). No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>design.n25.bosses.enabled this run (not under --off25 / --off24); the harness sets bosses25Override to force it.</summary>
        public bool BossRoomsOn => bosses25Override ?? (!N25Blocks.ForceOff && !N24Blocks.ForceOff && cfg.Design("n25.bosses.enabled"));
        public bool? bosses25Override;
        /// <summary>design.n25.bosses.rival.enabled (its own row, default off) on top of the bosses row; the harness sets rival25Override to force it.</summary>
        public bool RivalOn => BossRoomsOn && (rival25Override ?? cfg.Design("n25.bosses.rival.enabled"));
        public bool? rival25Override;

        /// <summary>RIVAL PRIME's duel room (design.n25.bosses.rival): null until its first fight.</summary>
        public RivalPrime rival25;
        public WyrmArena rival25Arena;
        public Vec3 rival25Origin;
        readonly BoxLayer rival25Layer = new BoxLayer();

        public WyrmArena bulwark25Arena;
        public Vec3 bulwark25Origin;
        readonly BoxLayer bulwark25Layer = new BoxLayer();
        /// <summary>The BULWARK fights in a campaign room (its hatch adds are off there).</summary>
        bool bulwark25Room;
        /// <summary>Boss-room retries that found the BULWARK or MONOLITH alive (the phase-start rule's count).</summary>
        public int bossRoom25Retries;

        partial void N25WyrmStartRun()
        {
            if (rival25 != null)
            {
                rival25.Clear();
                enemies.all.Remove(rival25.body);
                world.SetLayer("rivalRoom", null);
            }
            if (bulwark != null && bulwark25Room)
            {
                bulwark25Room = false;
                bulwark.arenaCenter = null;
                enemies.all.Remove(bulwark.body);
                world.SetLayer("bulwarkRoom", null);
            }
        }

        partial void N25WyrmStep(double dt)
        {
            if (rival25 != null)
            {
                if (rival25.Alive) rival25.Step(dt);
                else enemies.all.Remove(rival25.body);
            }
            if (!bulwark25Room || bulwark == null) return;
            if (bulwark.Alive)
            {
                if (!BossOn)
                {
                    bulwark.Step(dt); // a SURVIVAL boss round steps it in Boss20Step; a room fight steps it here
                    bossKit.Step(dt);
                }
            }
            else enemies.all.Remove(bulwark.body);
        }

        /// <summary>Build RIVAL PRIME and its duel room once (design.n25.bosses.rival; the arena file found); false when it cannot be.</summary>
        bool Rival25Ensure()
        {
            if (rival25 != null) return true;
            if (!cfg.Has("design.n25.bosses.rival.arena") || string.IsNullOrEmpty(Wyrm24Dir)) return false;
            string arena = Path.Combine(Wyrm24Dir, cfg.Str("design.n25.bosses.rival.arena"));
            if (!File.Exists(arena)) return false;
            double[] o = cfg.Nums("design.n25.bosses.rival.room.origin");
            rival25Origin = new Vec3(o[0], o[1], o[2]);
            rival25Arena = WyrmArena.Load(arena);
            var boxes = new List<(Vec3 min, Vec3 max)>();
            foreach (var (min, max, kind) in rival25Arena.boxes) boxes.Add((min + rival25Origin, max + rival25Origin));
            rival25Layer.SetBoxes(boxes);
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
            rival25 = new RivalPrime(cfg, events, world, target, enemies.NextId);
            rival25.playerBody = () => mover.Body;
            rival25.playerVelocity = () => mover.velocity;
            return true;
        }

        /// <summary>Build the BULWARK's room once (the files found); false when it cannot be.</summary>
        bool Bulwark25Ensure()
        {
            if (bulwark25Arena != null) return true;
            if (string.IsNullOrEmpty(Wyrm24Dir) || bulwark == null) return false;
            string arena = Path.Combine(Wyrm24Dir, cfg.Str("design.n25.bosses.bulwark.arena"));
            if (!File.Exists(arena)) return false;
            double[] o = cfg.Nums("design.n25.bosses.bulwark.room.origin");
            bulwark25Origin = new Vec3(o[0], o[1], o[2]);
            bulwark25Arena = WyrmArena.Load(arena);
            var boxes = new List<(Vec3 min, Vec3 max)>();
            foreach (var (min, max, kind) in bulwark25Arena.boxes) boxes.Add((min + bulwark25Origin, max + bulwark25Origin));
            bulwark25Layer.SetBoxes(boxes);
            return true;
        }

        /// <summary>The room's boss system (null: unknown id, or not built yet).</summary>
        public IBossSystem BossRoomSystem(string id)
        {
            IBossSystem b36 = null;
            N36BossSystem(id, ref b36); // NIGHT36 B: boss36's boss for this id while it owns the fight (SurvivalSim.Boss36.cs)
            if (b36 == null) b36 = N37BossSystem(id); // NIGHT37 W / S / O / C (SurvivalSim.N37.cs)
            if (b36 == null) b36 = Brawl40System(id); // NIGHT40 B2: THE ICEBREAKER owns "brawl40" while brawl40 is on (SurvivalSim.Brawl40.cs)
            return b36 ?? BossRoomSystem25(id);
        }

        IBossSystem BossRoomSystem25(string id) => id == "crown" ? (n32.boss5 ? (IBossSystem)crown32 : null) : id == "loom" ? (IBossSystem)loom30 : id == "bulwark" ? bulwark : id == "monolith" ? (IBossSystem)monolith24 : id == "wyrm" ? (wyrmFinal ? null : wyrm24) : id == "final" ? (wyrmFinal ? wyrm24 : null) : id == "rival" ? (IBossSystem)rival25 : id == "hulk" ? (n27.boss1 && lantern27 != null ? (IBossSystem)lantern27 : hulk26) : id == "glacier" && n31.stag31 ? (IBossSystem)stag31 : id == "glacier" ? (Glacier28On && glacier28 != null ? (IBossSystem)glacier28 : n27.boss2 && glacier27 != null ? (IBossSystem)glacier27 : glacier26) : null; // NIGHT27 B: the plan's ids hulk / glacier read THE LANTERN / the pulse-only chase with the row on

        /// <summary>The room's arena and origin (null arena: not built).</summary>
        public WyrmArena BossRoomArena(string id, out Vec3 origin)
        {
            if (id == "rival") { origin = rival25Origin; return rival25Arena; }
            if (id == "hulk" && n27.boss1 && lantern27Arena != null) { origin = lantern27Origin; return lantern27Arena; } // NIGHT27 B: THE LANTERN's courtyard
            if (id == "hulk") { origin = hulk26Origin; return hulk26Arena; } // NIGHT26 B
            origin = id == "bulwark" ? bulwark25Origin : id == "monolith" ? monolith24Origin : wyrm24Origin;
            if ((id == "wyrm" && wyrmFinal) || (id == "final" && !wyrmFinal)) return null; // the worm and its ELDER variant share one slot
            return id == "bulwark" ? bulwark25Arena : id == "monolith" ? monolith24Arena : wyrm24Arena;
        }

        /// <summary>Campaign24's BeginBoss(id): start (or, with a live boss, retry) the room's fight. spawn / yaw: where the player is put. false: the row is off, the
        /// files are missing, or no BossKit exists yet.</summary>
        public bool BossRoomBegin(string id, out Vec3 spawn, out double yawDeg)
        {
            spawn = default;
            yawDeg = 0;
            { bool b40 = Brawl40BeginFor(id, out bool h40, ref spawn, ref yawDeg); if (h40) return b40; } // NIGHT40 B2: THE ICEBREAKER's begin (SurvivalSim.Brawl40.cs)
            if (N37BossBegin(id, ref spawn, ref yawDeg, out bool n37Ok)) return n37Ok; // NIGHT37 W / S / O / C: the four bosses own their ids (SurvivalSim.N37.cs)
            bool n36Handled = false, n36Ok = false;
            N36BossBegin(id, ref n36Handled, ref n36Ok, ref spawn, ref yawDeg); // NIGHT36 B: boss36 / walkin36 own the begin when they say so (SurvivalSim.Boss36.cs)
            if (n36Handled) return n36Ok;
            if (id == "crown") return Crown32Begin(out spawn, out yawDeg); // NIGHT32 C: THE CROWN, LEVEL 5's boss (SurvivalSim.Crown32.cs; false with design.n32.boss5 off)
            if (id == "loom") return Loom30Begin(out spawn, out yawDeg); // NIGHT30 B (design.n30.span): THE LOOM, LEVEL 4's boss (SurvivalSim.Boss30.cs; false with the block off)
            if (id == "final")
            {
                if (!Wyrm25FinalBegin()) return false;
                spawn = Wyrm24Spawn;
                yawDeg = wyrm24Arena.spawnYawDeg;
                return true;
            }
            if (id == "glacier" && n31.stag31) return Stag31Begin(out spawn, out yawDeg); // NIGHT31 B: THE STAG replaces the Glacier (SurvivalSim.Boss31.cs)
            if (id == "glacier" && Glacier28On) // NIGHT28 B2: THE GLACIER as the lap chase on the banked rink (design.n28.glacier.enabled)
            {
                if (!Glacier28Begin()) return false;
                spawn = Glacier28Spawn;
                yawDeg = Glacier28SpawnYawDeg;
                return true;
            }
            if (id == "glacier" && n27.boss2) // NIGHT27 B: THE GLACIER as the pulse-only chase
            {
                if (!Glacier27Begin()) return false;
                spawn = Glacier27Spawn;
                yawDeg = Glacier27SpawnYaw; // NIGHT28: the rink's lane direction with design.n28.rink on
                return true;
            }
            if (id == "hulk" && n27.boss1) // NIGHT27 B: THE LANTERN replaces the Hulk with boss1 on
            {
                if (!Lantern27Begin()) return false;
                spawn = Lantern27Spawn;
                yawDeg = lantern27Arena.spawnYawDeg;
                return true;
            }
            if (id == "glacier") // NIGHT26 B: THE GLACIER (level 2's chase)
            {
                if (!Glacier26Begin()) return false;
                spawn = Glacier26Spawn;
                yawDeg = glacier26Course.spawnYawDeg;
                return true;
            }
            if (id == "hulk") // NIGHT26 B: THE SLAG HULK (level 1's easy boss)
            {
                if (!Hulk26Begin()) return false;
                spawn = Hulk26Spawn;
                yawDeg = hulk26Arena.spawnYawDeg;
                return true;
            }
            if (id == "wyrm")
            {
                if (!Wyrm24Begin()) return false;
                spawn = Wyrm24Spawn;
                yawDeg = wyrm24Arena.spawnYawDeg;
                return true;
            }
            if (!BossRoomsOn || bossKit == null) return false;
            if (id == "bulwark")
            {
                if (!Bulwark25Ensure()) return false;
                spawn = bulwark25Arena.spawn + bulwark25Origin;
                yawDeg = bulwark25Arena.spawnYawDeg;
                if (bulwark.Alive && bulwark25Room)
                {
                    bossRoom25Retries++;
                    if (bulwark.RestorePhaseStart() || bulwark.phase == "dying") return true; // back to the start of its phase, or the finisher is under way
                }
                bulwark25Room = true;
                bulwark.arenaCenter = bulwark25Origin;
                bulwark.NewRun(runSeed ^ (uint)cfg.Num("design.n25.bosses.bulwark.room.salt"));
                world.SetLayer("bulwarkRoom", bulwark25Layer, true);
                if (!enemies.all.Contains(bulwark.body)) enemies.all.Add(bulwark.body);
                bossKit.Start("bulwark");
                return true;
            }
            if (id == "rival")
            {
                if (!RivalOn || !Rival25Ensure()) return false;
                spawn = rival25Arena.spawn + rival25Origin;
                yawDeg = rival25Arena.spawnYawDeg;
                if (rival25.Alive)
                {
                    bossRoom25Retries++;
                    if (rival25.RestorePhaseStart() || rival25.phase == "dying") return true;
                }
                rival25.arenaCenter = rival25Origin;
                rival25.NewRun(runSeed ^ (uint)cfg.Num("design.n25.bosses.rival.room.salt"));
                world.SetLayer("rivalRoom", rival25Layer, true);
                if (!enemies.all.Contains(rival25.body)) enemies.all.Add(rival25.body);
                bossKit.bosses["rival"] = rival25;
                bossKit.Start("rival", 0, 0, null, rival25.p.name);
                return true;
            }
            if (id == "monolith")
            {
                if (monolith24 != null && monolith24.Alive)
                {
                    bossRoom25Retries++;
                    if (monolith24.RestorePhaseStart() || monolith24.phase == "dying")
                    {
                        spawn = Monolith24Spawn;
                        yawDeg = monolith24Arena.spawnYawDeg;
                        return true;
                    }
                }
                if (!Monolith24Begin()) return false;
                spawn = Monolith24Spawn;
                yawDeg = monolith24Arena.spawnYawDeg;
                return true;
            }
            return false;
        }

        /// <summary>Campaign24's BossIsDefeated(id): the room's boss was started and is dead.</summary>
        public bool BossRoomDefeated(string id)
        {
            IBossSystem s = BossRoomSystem(id);
            return s != null && !s.Alive;
        }
    }
}
