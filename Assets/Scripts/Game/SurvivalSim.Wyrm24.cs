// NIGHT24 builder B: THE KILN WYRM in the sim (design.n24.wyrm; Core/Enemies/Wyrm.cs). The worm lives in its own room (Design/maps/wyrm-arena.json)
// placed far from the SURVIVAL map at design.n24.wyrm.room.origin; the room's boxes are a world layer (rays on: shots stop at its walls), the
// worm is registered as an enemy while it fights (the shots find it through WyrmBody; EnemyHits.Filter applies the part's multiplier). Nothing
// here runs with the row off. Layer 1's builder (L) starts the fight from the boss room's trigger with Wyrm24Begin(); the reel `wyrm` does it
// directly. No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The folder that holds Design/bosses/wyrm.json and Design/maps/wyrm-arena.json (the repo root in the Editor and the harness,
        /// StreamingAssets in a Player): set by the game before a run; null leaves the worm unbuilt.</summary>
        public static string Wyrm24Dir;

        /// <summary>THE KILN WYRM (null until the row is on and its files are found).</summary>
        public Wyrm wyrm24;
        /// <summary>NIGHT25 W: wyrm24 is THE ELDER KILN WYRM (design.n25.bosses.final: its own paths, arena, origin and params), built by Wyrm25FinalBegin; a new run goes back to the worm.</summary>
        public bool wyrmFinal;
        public WyrmArena wyrm24Arena;
        /// <summary>The room's origin in the world.</summary>
        public Vec3 wyrm24Origin;
        readonly BoxLayer wyrm24Layer = new BoxLayer();
        /// <summary>NIGHT25 W: the boss-room retries that found the worm alive (the phase-start rule's count; the scenario prints it).</summary>
        public int wyrm24Retries;
        /// <summary>NIGHT26 B: wyrm24 was built as level 3's worm (design.n26.bosses.wyrm26: its own paths and wall-ride arena, proximity aggro, body x0.5, the finale).</summary>
        public bool wyrm24Is26;
        /// <summary>Level 3's worm is wanted now: the n26 level3 block is on this run and its files are configured (the harness forces n26Override).</summary>
        public bool Wyrm26On => !wyrmFinal && n26.level3 && cfg.Has("design.n26.bosses.wyrm26.paths");

        /// <summary>The player's spawn in the room (world), facing the room's middle.</summary>
        public Vec3 Wyrm24Spawn => wyrm24Arena == null ? default : wyrm24Arena.spawn + wyrm24Origin;

        /// <summary>NIGHT25 (builder F, design.n25.hitchFix): parse the worm's boss and arena files once at boot and drop the results, so the first run start's Wyrm24Ensure
        /// finds the JSON parser and the loaders warm (N24StartRun was 17.7 ms cold in n25-hitch-4). Pure reads: no sim state, no id taken, nothing kept.</summary>
        public void WarmParse25()
        {
            if (!n24Design.wyrm || string.IsNullOrEmpty(Wyrm24Dir)) return;
            string paths = Path.Combine(Wyrm24Dir, cfg.Str("design.n24.wyrm.paths")), arena = Path.Combine(Wyrm24Dir, cfg.Str("design.n24.wyrm.arena"));
            if (!File.Exists(paths) || !File.Exists(arena)) return;
            Wyrm.LoadBoss(paths);
            WyrmArena.Load(arena);
        }

        partial void N24WyrmStartRun()
        {
            Monolith24StartRun(); // MONOLITH (SurvivalSim.Monolith24.cs)
            if (wyrm24 != null)
            {
                wyrm24.Clear();
                enemies.all.Remove(wyrm24.body);
                world.SetLayer("wyrm", null);
                if (wyrmFinal)
                {
                    wyrmFinal = false; // the ELDER WYRM was this sim's last fight: the next run is the KILN WYRM's again
                    wyrm24 = null;
                    wyrm24Arena = null;
                }
            }
            if (n24.wyrm) Wyrm24Ensure();
        }

        partial void N24WyrmStep(double dt)
        {
            Monolith24Step(dt);
            if (wyrm24 == null) return;
            if (wyrm24.Alive) wyrm24.Step(dt);
            else enemies.all.Remove(wyrm24.body);
        }

        /// <summary>Build the worm and its room once (the row on, the files found); null when it cannot be built.</summary>
        public Wyrm Wyrm24Ensure()
        {
            if (wyrm24 != null) return wyrm24;
            if (string.IsNullOrEmpty(Wyrm24Dir)) return null;
            string pfx = wyrmFinal ? "design.n25.bosses.final" : "design.n24.wyrm";
            bool w26 = Wyrm26On; // NIGHT26 B: level 3's worm reads its own paths and arena (the room's origin and salt stay the worm's)
            string paths = Path.Combine(Wyrm24Dir, cfg.Str(w26 ? "design.n26.bosses.wyrm26.paths" : pfx + ".paths")), arena = Path.Combine(Wyrm24Dir, w26 ? Wyrm39Arena(cfg.Str("design.n26.bosses.wyrm26.arena")) : cfg.Str(pfx + ".arena")); // NIGHT39 K4: the level's own worm arena (SurvivalSim.Kiln39.cs)
            if (!File.Exists(paths) || !File.Exists(arena)) return null;
            wyrm24Is26 = w26;
            double[] o = cfg.Nums(pfx + ".room.origin");
            wyrm24Origin = new Vec3(o[0], o[1], o[2]);
            wyrm24Arena = WyrmArena.Load(arena);
            var boxes = new List<(Vec3 min, Vec3 max)>();
            foreach (var (min, max, kind) in wyrm24Arena.boxes) boxes.Add((min + wyrm24Origin, max + wyrm24Origin));
            wyrm24Layer.SetBoxes(boxes);
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
            wyrm24 = new Wyrm(cfg, WyrmParams.From(cfg, Wyrm.LoadBoss(paths), wyrmFinal, w26), events, world, target, enemies.NextId);
            wyrm24.playerBody = () => mover.Body;
            return wyrm24;
        }

        /// <summary>Start the fight now: the room's layer on, the worm registered as an enemy, the BossKit's intro card. false: the row is off, the
        /// files are missing, or no BossKit exists yet (no run has started).</summary>
        public bool Wyrm24Begin() => Wyrm24BeginCore(false);

        /// <summary>NIGHT25 W (design.n25.bosses.final): start THE ELDER KILN WYRM in its own room (a live ELDER WYRM goes back to its phase start, as the worm does).
        /// The worm and its variant share the wyrm24 slot, so the view, the bar and the retry rule are the worm's; false: bosses off, files missing, no BossKit.</summary>
        public bool Wyrm25FinalBegin() => Wyrm24BeginCore(true);

        bool Wyrm24BeginCore(bool final)
        {
            if (final)
            {
                if (!BossRoomsOn || bossKit == null) return false;
                if (!wyrmFinal)
                {
                    if (wyrm24 != null)
                    {
                        wyrm24.Clear();
                        enemies.all.Remove(wyrm24.body);
                        world.SetLayer("wyrm", null);
                    }
                    wyrm24 = null;
                    wyrm24Arena = null;
                    wyrmFinal = true;
                }
            }
            else if (wyrmFinal) return false; // the worm's room is not this sim's while the ELDER WYRM holds the slot (a new run releases it)
            if (wyrm24 != null && !wyrmFinal && !wyrm24.Alive && (wyrm24Is26 != Wyrm26On || (wyrm24Is26 && Wyrm39ArenaStale(cfg.Str("design.n26.bosses.wyrm26.arena"))))) // NIGHT39 K4: or the level wants another arena file
            {
                // NIGHT26 B: the n26 level3 switch changed since the worm was built: build the other one
                enemies.all.Remove(wyrm24.body);
                world.SetLayer("wyrm", null);
                wyrm24 = null;
                wyrm24Arena = null;
            }
            if ((!final && !n24.wyrm) || bossKit == null || Wyrm24Ensure() == null) return false;
            if (!wyrm24.Alive)
            {
                wyrm24.aim = n25.wyrmAim; // NIGHT25 W: this run's switch, taken at the fight's start (a retry keeps it)
                wyrm24.arenaHalf = wyrm24Arena.half;
            }
            // NIGHT25 W (design.n25.wyrmAim; the lead's decision): Campaign24.Retry calls BeginBoss again after a player death while the worm lives: it goes back to the
            // START of its phase 2 / 3 (HP at the threshold, the same stream), not to full HP. A first entry finds no live worm, so this never fires there.
            if ((n25.wyrmAim || wyrmFinal) && wyrm24.Alive)
            {
                wyrm24Retries++;
                if (wyrm24.RestorePhaseStart()) return true;
                if (wyrm24.phase == "dying") return true; // the finisher is under way: the worm dies anyway
            }
            wyrm24.arenaCenter = wyrm24Origin;
            wyrm24.NewRun(runSeed ^ (uint)cfg.Num((wyrmFinal ? "design.n25.bosses.final" : "design.n24.wyrm") + ".room.salt"));
            world.SetLayer("wyrm", wyrm24Layer, true);
            bossKit.bosses["wyrm"] = wyrm24;
            if (!enemies.all.Contains(wyrm24.body)) enemies.all.Add(wyrm24.body);
            bossKit.Start("wyrm", 0, 0, null, wyrm24.p.introName);
            return true;
        }
    }
}
