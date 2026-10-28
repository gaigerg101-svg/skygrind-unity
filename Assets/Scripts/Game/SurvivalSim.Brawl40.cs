// NIGHT40 builder B2 (brawl40; NIGHT40.md B2): THE ICEBREAKER's arena and fight in the sim (the brain: Core/Enemies/Brawl40.cs). Implements the
// lead's partials (SurvivalSim.N40.cs):
//   N40BrawlStartRun  a level whose campaign room row has `brawl40` ({ "centre": [x, y, z] }: n40-iceline's x9 THE BRAWL) gets the arena from
//                     level load: design.n40.brawl40.arena's boxes as world layer "brawl40Arena" at the row's centre (the entry gap open). It is
//                     level geometry (only n40-iceline has the row), so it stands whatever the rows say; with the row on, boss37WalkInHere: the
//                     fight is where the player walked in (Campaign24 keeps him there: Boss36WalkIn).
//   N40BrawlStep      the fight's step: the brain, the slab (closes walkin36.lockSeconds after the begin), the name card (walkin36.cardSeconds).
//   Brawl40Begin      the plan id "brawl40" starts the fight (or, with a live one, puts the player back at the room's spawn: a retry).
//                     The campaign reaches it through BossRoomBegin / BossRoomSystem: the lead's two hook lines (B2's report, round 1).
// Off (the row off / --off40): nothing here runs but the arena's geometry on a level that has the row. No UnityEngine: SimHarness compiles it.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Brawl40 brawl40;
        public WyrmArena brawl40Arena;
        /// <summary>The arena's centre (world, the floor) on this level, or null when the level has no brawl40 row.</summary>
        public Vec3? brawl40Centre;
        /// <summary>The harness / reel put the arena here on a map without the row (null: the level's row).</summary>
        public Vec3? brawl40CentreOverride;
        /// <summary>The arena file's folder when Wyrm24Dir is not set (the harness: the repo root).</summary>
        public static string Brawl40Dir;
        readonly BoxLayer brawl40Layer = new BoxLayer();
        bool brawl40Owns, brawl40SlabClosed;
        int brawl40SlabLeft, brawl40CardLeft;
        public int brawl40Begins, brawl40Retries, brawl40CardStep = -1;
        /// <summary>The entry slab is closed (the view draws it).</summary>
        public bool Brawl40SlabClosed => brawl40SlabClosed;

        partial void N40BrawlStartRun()
        {
            if (brawl40 != null)
            {
                brawl40.resume = false;
                brawl40.Clear();
                enemies.all.Remove(brawl40.body);
            }
            brawl40Owns = false;
            brawl40Line?.Clear();
            brawl40LastPunch = "";
            brawl40SlabClosed = false;
            brawl40SlabLeft = brawl40CardLeft = 0;
            brawl40Begins = brawl40Retries = 0;
            brawl40CardStep = -1;
            brawl40Centre = Brawl40RowCentre();
            if (brawl40Centre == null || !Brawl40Load())
            {
                brawl40Centre = null;
                world.SetLayer("brawl40Arena", null);
                return;
            }
            Brawl40Layer(false);
            if (n40.brawl40) boss37WalkInHere = true;
        }

        /// <summary>The reel's / harness's way onto a map without the row: the arena at `centre` now (the gap open), as a level row would put it.</summary>
        public bool Brawl40Place(Vec3 centre)
        {
            brawl40CentreOverride = centre;
            brawl40Centre = centre;
            if (!Brawl40Load()) { brawl40Centre = null; return false; }
            Brawl40Layer(false);
            if (n40.brawl40) boss37WalkInHere = true;
            return true;
        }

        bool Brawl40Load()
        {
            if (brawl40Arena != null) return true;
            string dir = !string.IsNullOrEmpty(Wyrm24Dir) ? Wyrm24Dir : Brawl40Dir;
            if (string.IsNullOrEmpty(dir) || !cfg.Has("design.n40.brawl40.arena")) return false;
            string file = Path.Combine(dir, cfg.Str("design.n40.brawl40.arena"));
            if (!File.Exists(file)) return false;
            brawl40Arena = WyrmArena.Load(file);
            var j = PortData.Obj(PortData.LoadJson(file));
            if (j.ContainsKey("slab"))
            {
                var s = PortData.Obj(j["slab"]);
                brawl40SlabMin = PortData.V3(s["min"]);
                brawl40SlabMax = PortData.V3(s["max"]);
            }
            if (j.ContainsKey("bossSpawn"))
            {
                var b = PortData.Obj(j["bossSpawn"]);
                brawl40BossSpawn = new Vec3(PortData.Num(b["x"]), PortData.Num(b["y"]), PortData.Num(b["z"]));
            }
            return true;
        }

        Vec3 brawl40SlabMin, brawl40SlabMax, brawl40BossSpawn = new Vec3(0, 0, -6);

        void Brawl40Layer(bool slab)
        {
            brawl40SlabClosed = slab;
            Vec3 c = brawl40Centre.Value;
            var list = new List<(Vec3, Vec3)>(brawl40Arena.boxes.Count + 1);
            foreach (var (min, max, _) in brawl40Arena.boxes) list.Add((min + c, max + c));
            if (slab) list.Add((brawl40SlabMin + c, brawl40SlabMax + c));
            brawl40Layer.SetBoxes(list);
            world.SetLayer("brawl40Arena", brawl40Layer, true);
        }

        /// <summary>The campaign room row's brawl40.centre in the level JSON (n40-iceline's x9), or null.</summary>
        Vec3? Brawl40RowCentre()
        {
            if (brawl40CentreOverride.HasValue) return brawl40CentreOverride;
            if (map?.json == null || !map.json.ContainsKey("campaign")) return null;
            var camp = PortData.Obj(map.json["campaign"]);
            if (!camp.ContainsKey("rooms")) return null;
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (!r.ContainsKey("brawl40")) continue;
                var row = PortData.Obj(r["brawl40"]);
                if (!row.ContainsKey("centre")) continue;
                return PortData.V3(row["centre"]);
            }
            return null;
        }

        /// <summary>BossRoomSystem's answer for "brawl40" (the lead's hook line calls it): the brawler while this run's fight is its.</summary>
        public IBossSystem Brawl40System(string id) => id == "brawl40" && brawl40Owns && brawl40 != null ? brawl40 : null;

        /// <summary>BossRoomBegin's answer for "brawl40" (the lead's hook line calls it): handled when the id is ours and the row is on.</summary>
        public bool Brawl40BeginFor(string id, out bool handled, ref Vec3 spawn, ref double yawDeg)
        {
            handled = id == "brawl40" && n40.brawl40 && brawl40Centre != null && bossKit != null;
            if (!handled) return false;
            return Brawl40Begin(out spawn, out yawDeg);
        }

        /// <summary>Start THE ICEBREAKER in its arena (or, with a live one, a retry: the player back at the room's spawn, the fight's progress
        /// kept, a rest). spawn / yaw: the room's spawn (Campaign24's retry point).</summary>
        public bool Brawl40Begin(out Vec3 spawn, out double yawDeg)
        {
            spawn = default;
            yawDeg = 0;
            if (brawl40Centre == null || brawl40Arena == null || bossKit == null) return false;
            Vec3 c = brawl40Centre.Value;
            RoomDef room = campaign?.Room;
            spawn = room != null && room.kind == "boss" ? room.spawn : brawl40Arena.spawn + c;
            yawDeg = room != null && room.kind == "boss" ? room.spawnYawDeg : brawl40Arena.spawnYawDeg;
            if (brawl40 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                brawl40 = new Brawl40(cfg, Brawl40Params.From(cfg), events, target, enemies.NextId, tells36);
                brawl40.playerBody = () => mover.Body;
                brawl40.playerGrounded = () => mover.grounded;
                brawl40.playerVelocity = () => mover.velocity;
                brawl40.parryHit = (dmg, at) => loadout.Damage(brawl40.body, dmg, false, Stag31.ParryWeapon, at);
            }
            Brawl40HookPunch();
            Brawl42Arm(); // NIGHT42 B1 (brawl42): the new attacks on or off for this fight (SurvivalSim.N42.Brawl.cs)
            brawl40.tells = tells36;
            brawl40.origin = c;
            brawl40.half = brawl40Arena.half;
            brawl40.pillars.Clear();
            foreach (var (min, max, kind) in brawl40Arena.boxes) if (kind == "pillar") brawl40.pillars.Add((min + c, max + c));
            brawl40Begins++;
            if (brawl40.Alive && brawl40Owns)
            {
                brawl40Retries++;
                brawl40.Retry();
                brawl40Line?.Say("entry", brawl40LineEntry, brawl40LineSeconds, null, 2); // once more after the first death (boss_rules 5)
                return true;
            }
            brawl40.seed = runSeed ^ (uint)cfg.Num("design.n40.brawl40.salt");
            if (!loadout.owned.Contains("gauntlet")) loadout.AddWeapon("gauntlet"); // level 2's weapon: the brawler is the PULSE GAUNTLET's fight
            bossKit.bosses["brawl40"] = brawl40;
            brawl40Owns = true;
            bossKit.Clear();
            brawl40.resume = false;
            brawl40.body.position = brawl40BossSpawn + c;
            brawl40.StartFight(new BossFight { center = c, kit = bossKit, scale = BossMath.Scale("brawl40", 0, 0, null, bossKit.k) });
            brawl40.Place(brawl40BossSpawn + c);
            brawl40CardLeft = Math.Max(1, MovementMath.StepsFor(cfg.Num("design.n36.walkin36.cardSeconds"), cfg.Dt));
            brawl40SlabLeft = Math.Max(1, MovementMath.StepsFor(cfg.Num("design.n36.walkin36.lockSeconds"), cfg.Dt));
            if (!enemies.all.Contains(brawl40.body)) enemies.all.Add(brawl40.body);
            Brawl40LineBegin();
            events.Emit("boss36Lock", new Brawl40Event { stage = "lock", at = c });
            events.Emit("boss36Reveal", new Brawl40Event { stage = "reveal", at = brawl40.Centre });
            return true;
        }

        partial void N40BrawlStep(double dt)
        {
            if (!n40.brawl40 || brawl40 == null || !brawl40Owns) return;
            if (brawl40SlabLeft > 0 && --brawl40SlabLeft == 0 && brawl40.Alive && !brawl40SlabClosed) Brawl40Layer(true);
            if (brawl40CardLeft > 0 && --brawl40CardLeft == 0 && brawl40.Alive)
            {
                brawl40.resume = true;
                bossKit.Start("brawl40", 0, 0, null, brawl40.p.name);
                brawl40.resume = false;
                brawl40CardStep = brawl40.stepNo;
            }
            if (brawl40.Alive)
            {
                brawl40.Step(dt);
                if (!BossOn) bossKit.Step(dt);
                Brawl40Lines();
            }
            brawl40Line?.Step();
            if (!brawl40.Alive)
            {
                enemies.all.Remove(brawl40.body);
                brawl40Line?.Clear();
                if (brawl40SlabClosed) Brawl40Layer(false); // the kill opens the gap
            }
        }

        // --- the punch (the block's parry) ---------------------------------------------------------------------------------------------------

        bool brawl40PunchHooked;
        /// <summary>The last punch's result ("block", "").</summary>
        public string brawl40LastPunch = "";

        void Brawl40HookPunch()
        {
            if (brawl40PunchHooked) return;
            brawl40PunchHooked = true;
            // the F punch (a gun in hand), and the PULSE GAUNTLET's swing (the gauntlet in hand: its fire)
            Action<MeleeEvent> hit = e =>
            {
                if (e.owner != "player" || brawl40 == null || !brawl40.Alive || !brawl40Owns || !n40.brawl40) return;
                var (eye, dir) = Aim();
                brawl40LastPunch = brawl40.Punch(eye, dir);
            };
            events.On<MeleeEvent>("punch", hit);
            events.On<MeleeEvent>("meleeSwing", e => { if (e.weapon == "gauntlet") hit(e); });
        }

        // --- the boss line (B5's BossLine40; boss_rules 5: the damage rule taught on entry, and every hit answers) ----------------------------

        public BossLine40 brawl40Line;
        string brawl40LineEntry, brawl40LineFar, brawl40LineStuck, brawl40LineBlock;
        double brawl40LineSeconds;
        int brawl40FarHits;

        void Brawl40LineBegin()
        {
            const string g = "design.n40.brawl40.line.";
            brawl40Line ??= new BossLine40(events, step);
            brawl40LineEntry = cfg.Str(g + "entry");
            brawl40LineFar = cfg.Str(g + "far");
            brawl40LineStuck = cfg.Str(g + "stuck");
            brawl40LineBlock = cfg.Str(g + "block");
            brawl40LineSeconds = cfg.Num(g + "seconds");
            Brawl47Lines(); // NIGHT47 B: brawl47's line set while the row is on (SurvivalSim.N47.Icebreaker.cs)
            brawl40FarHits = 0;
            string roomId = null;
            int room = -1;
            if (campaign != null && campaignLevel != null && campaign.room >= 0 && campaign.room < campaignLevel.rooms.Count)
            {
                room = campaign.room;
                roomId = campaignLevel.rooms[room].id;
            }
            brawl40Line.Begin("brawl40", roomId, room);
            brawl40Line.Say("entry", brawl40LineEntry, brawl40LineSeconds, null, 2);
            if (!brawl40HitHooked)
            {
                brawl40HitHooked = true;
                events.On<BossEvent>("bossHit", e =>
                {
                    if (brawl40 == null || !brawl40Owns || e.boss != "brawl40" || brawl40.LastRangeMult > 0.5) return;
                    if (++brawl40FarHits % 4 == 0) brawl40Line?.Say("far", brawl40LineFar, brawl40LineSeconds, null, 3); // shots from range: say why they barely land
                });
            }
        }

        bool brawl40HitHooked;

        void Brawl40Lines()
        {
            if (brawl40Line == null || brawl40.state == Brawl40State.Reveal) return;
            if (brawl40.entered && brawl40.state == Brawl40State.Stuck) brawl40Line.Say("stuck", brawl40LineStuck, brawl40LineSeconds * 0.5, null, 2);
            if (brawl40.entered && brawl40.Telling(Brawl40.Block)) brawl40Line.Say("block", brawl40LineBlock, brawl40LineSeconds * 0.5, null, 2);
        }
    }
}
