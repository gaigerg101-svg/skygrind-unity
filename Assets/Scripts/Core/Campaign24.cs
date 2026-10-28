// NIGHT24 (builder L): THE CAMPAIGN's rules, pure (no engine). A level is a line of ROOMS joined by DOORS (Design/levels/LEVEL-FORMAT.md,
// read from the map JSON's "campaign" block by LevelDef.From). Entering a room locks the door behind (Zones.Lock), spawns its
// encounter script (a wave list, enemies only at the room's own nodes, from a Mulberry32 seeded by the run seed and the room), and
// opens the exit door free (Zones.Open(id, free: true)) when the last wave is dead; the door is the checkpoint. A death restarts the
// ROOM within design.n24.campaign.retryDelay s of game time with the same seed. A LIMIT room strips a weapon set or a verb for
// that room only (PISTOL ONLY, NO DASH) and pays a style bonus when cleared; a room can award a weapon at its end (weaponEarned).
// The world (zones, spawns, the player) is behind ICampaignWorld, so the rules run in EditMode tests and in SimHarness.
// Ported from nothing: the web has no campaign (Unity-only; numbers in design.n24.campaign).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>LIMIT room flags (bit i; the tape's Campaign24State.limits carries them). PistolOnly and MeleeOnly strip the weapon set (the
    /// owned list is kept and restored at the door); NoDash, NoSlide and NoJump strip ONE verb from the input frame and nothing else
    /// (Strip). NIGHT24 leftover 1: NoSlide, NoJump and MeleeOnly (design.n24.campaign.limitFlags).</summary>
    public static class Limit24
    {
        public const int PistolOnly = 1, NoDash = 2, NoSlide = 4, NoJump = 8, MeleeOnly = 16;

        /// <summary>The flags the NIGHT24 leftover added: masked off when design.n24.campaign.limitFlags.enabled is false.</summary>
        public const int Extra = NoSlide | NoJump | MeleeOnly;

        public static int Parse(object list)
        {
            int f = 0;
            if (list is List<object> l)
                foreach (object o in l)
                {
                    string s = o as string;
                    if (s == "pistolOnly") f |= PistolOnly;
                    else if (s == "noDash") f |= NoDash;
                    else if (s == "noSlide") f |= NoSlide;
                    else if (s == "noJump") f |= NoJump;
                    else if (s == "meleeOnly") f |= MeleeOnly;
                }
            return f;
        }

        /// <summary>The banner text for the flags in force ("" for none), the words joined by " / " in the order PISTOL ONLY, MELEE ONLY, NO DASH, NO SLIDE, NO JUMP.</summary>
        public static string Banner(int flags)
        {
            var parts = new List<string>();
            if ((flags & PistolOnly) != 0) parts.Add("PISTOL ONLY");
            if ((flags & MeleeOnly) != 0) parts.Add("MELEE ONLY");
            if ((flags & NoDash) != 0) parts.Add("NO DASH");
            if ((flags & NoSlide) != 0) parts.Add("NO SLIDE");
            if ((flags & NoJump) != 0) parts.Add("NO JUMP");
            return string.Join(" / ", parts);
        }

        /// <summary>The input frame with the limited verbs removed: exactly the dash key for NoDash, the slide key for NoSlide, the jump key for
        /// NoJump, and nothing else (the Mover is untouched; it just never sees the key).</summary>
        public static void Strip(ref InputFrame frame, int flags)
        {
            if ((flags & NoDash) != 0) frame.dash = default;
            if ((flags & NoSlide) != 0) frame.slide = default;
            if ((flags & NoJump) != 0) frame.jump = default;
        }
    }

    public sealed class RoomDef
    {
        public string id, name, kind = "room", award;
        /// <summary>NIGHT25 W: a boss room's boss ("wyrm" when the plan names none).</summary>
        public string boss = "wyrm";
        public double par;
        /// <summary>The floor plan [x0, z0, x1, z1] (the plan's rect; the lighting kit's trim runs along it).</summary>
        public double x0, z0, x1, z1;
        public Vec3 spawn, exit;
        public double spawnYawDeg;
        public readonly List<Vec3> nodes = new List<Vec3>();
        public readonly List<List<string>> waves = new List<List<string>>();
        public int limits;
        /// <summary>NIGHT27 (builder A): why a LIMIT stays in this room (the banner's words; empty: no reason, the validator rejects it in a rebuilt level).</summary>
        public string limitReason = "";
        /// <summary>NIGHT27 (builder A): the room's floor HAZARD strip [x0, z0, x1, z1] (the plan's `hazard.rect`); hasHazard false: none.</summary>
        public bool hasHazard, hazardSafe;
        public double hx0, hz0, hx1, hz1;
        /// <summary>NIGHT25: the stage's ordered lessons (a tutorial stage's door opens when they are all cleared), and its respawn line: feet below
        /// respawnY (a pit's floor) return the player to the room's spawn with no death (no fail state).</summary>
        public readonly List<LessonDef> lessons = new List<LessonDef>();
        public readonly List<StandInDef> standIns = new List<StandInDef>();
        /// <summary>NIGHT31 W (builder L's plan rows, design.n31.unlocks): the room's rows for the new weapon order; LevelDef.UseUnlocks31 swaps them in
        /// (null: the room has none, its NIGHT30 rows stay).</summary>
        public List<LessonDef> lessons31;
        public List<StandInDef> standIns31;
        public List<List<string>> waves31;
        public bool hasRespawnY;
        public double respawnY;
        /// <summary>NIGHT43 E1 (rooms43; read only with the row on, Campaign24.rooms43): the room's `"afterLesson43"`: wave w waits until lesson
        /// afterLesson43[w] of this room is done (-1: no wait). In the level file: a number (wave 0 waits for that lesson) or an array per wave.
        /// Null: no wave waits (every room before NIGHT43).</summary>
        public int[] afterLesson43;
        /// <summary>NIGHT43 E1 (rooms43): the room's `"fallReturn43": {"belowY": y, "to": [x, y, z], "damage": hp, "yawDeg"?: deg}`: the feet
        /// below belowY inside the room's rect -> back at `to` (facing yawDeg, else the room's spawn yaw), `damage` health taken (never a death),
        /// the event fallReturn43 (the view's short fade). DECIDE43 D-5: THE GANTRY's lava, 10 health and back to the lip.</summary>
        public bool hasFallReturn43;
        public double fallBelowY43, fallDamage43, fallYawDeg43;
        public Vec3 fallTo43;
        /// <summary>NIGHT43 E1 round 2 (L1's THE GANTRY: two pits, each with its own lip): fallReturn43's `"by": [{"rect": [x0, z0, x1, z1], "to":
        /// [x, y, z], "yawDeg"?}]`: the first rect that holds the feet at the fall picks the return point (and the facing); none: `to`. Null: none.</summary>
        public List<FallBy43> fallBy43;
        public int WaveCount => waves.Count;
        /// <summary>NIGHT43 E1: the lesson wave w waits for (-1: none).</summary>
        public int AfterLesson43(int w) => afterLesson43 != null && w >= 0 && w < afterLesson43.Length ? afterLesson43[w] : -1;
        /// <summary>NIGHT43 E1 round 2 (rooms43): the room's `"waveNodes43": [null, [[x, y, z], ...], ...]`: wave w's own spawn spots (null or absent:
        /// the room's nodes, or a spawns33 trigger's). The FUSION husk on the far side of g6's dash lane; L1 / K1 may use it for any staged wave.</summary>
        public List<List<Vec3>> waveNodes43;
        public List<Vec3> WaveNodes43(int w) => waveNodes43 != null && w >= 0 && w < waveNodes43.Count && waveNodes43[w] != null && waveNodes43[w].Count > 0 ? waveNodes43[w] : null;
        public int EnemyCount
        {
            get
            {
                int n = 0;
                foreach (List<string> w in waves) n += w.Count;
                return n;
            }
        }
    }

    /// <summary>NIGHT43 E1 round 2: one entry of a room's fallReturn43 `by` list (RoomDef.fallBy43).</summary>
    public struct FallBy43
    {
        public double x0, z0, x1, z1, yawDeg; // yawDeg NaN: the fallReturn43's own yaw (else the room's spawn yaw)
        public Vec3 to;
        public bool Holds(Vec3 f) => f.x >= x0 && f.x <= x1 && f.z >= z0 && f.z <= z1;
    }

    /// <summary>NIGHT29 round 2: a plan prop row with `pickup: true` (weaponPickup / healthPickup): collectable in a campaign level.</summary>
    public sealed class PickupDef
    {
        public string id, room, kind, weapon;
        public Vec3 at;
    }

    /// <summary>NIGHT29 round 2: a room's stand-in (the wall-ride room's example targets): `at`, strafing between lz a and b (local z: metres from the room's z1 edge toward z0).</summary>
    public sealed class StandInDef
    {
        public Vec3 at;
        public bool strafe;
        public double lzA, lzB;
    }

    public sealed class DoorDef
    {
        public string id, a, b;
        public bool checkpoint;
    }

    public sealed class LevelDef
    {
        public string id, name;
        public readonly List<RoomDef> rooms = new List<RoomDef>();
        public readonly List<DoorDef> doors = new List<DoorDef>();
        public readonly List<PickupDef> pickups = new List<PickupDef>();
        /// <summary>NIGHT31 W: the rooms' `pickup31` rows (i1a's gauntlet), joined to `pickups` by UseUnlocks31.</summary>
        public readonly List<PickupDef> pickups31 = new List<PickupDef>();
        /// <summary>NIGHT31 W: UseUnlocks31 has run on this level object.</summary>
        public bool unlocks31Used;

        /// <summary>NIGHT31 W (design.n31.unlocks; the sim calls it at a campaign run's start, before the first room is entered): each room's lessons31 /
        /// standIns31 / waves31 replace its lessons / standIns / waves, and the pickup31 rows join the pickups. Once per level object.</summary>
        public void UseUnlocks31()
        {
            if (unlocks31Used) return;
            unlocks31Used = true;
            foreach (RoomDef r in rooms)
            {
                if (r.lessons31 != null) { r.lessons.Clear(); r.lessons.AddRange(r.lessons31); }
                if (r.standIns31 != null) { r.standIns.Clear(); r.standIns.AddRange(r.standIns31); }
                if (r.waves31 != null) { r.waves.Clear(); r.waves.AddRange(r.waves31); }
            }
            pickups.AddRange(pickups31);
        }
        /// <summary>NIGHT26 (builder L): the plan's `rank` block (TIME / KILLS / STYLE thresholds); null: the defaults from the pars (RankFor).</summary>
        public RankTable26 rank;

        /// <summary>The sum of the rooms' pars (the default time cuts scale with it).</summary>
        public double ParSum
        {
            get
            {
                double s = 0;
                foreach (RoomDef r in rooms) s += r.par;
                return s;
            }
        }

        /// <summary>Enemies the plan's waves spawn in all rooms (the KILLS rank's denominator; a boss is not counted).</summary>
        public int PlannedEnemies
        {
            get
            {
                int n = 0;
                foreach (RoomDef r in rooms) n += r.EnemyCount;
                return n;
            }
        }

        /// <summary>The thresholds the level is ranked by: its own block, else the config's defaults from its pars.</summary>
        public RankTable26 RankFor(Rank26Params p) => rank ?? p.DefaultsFor(ParSum);

        /// <summary>The weapon the level's last room awards (null: none): the level select's "unlocks" column when Design/levels/unlocks.json names none.</summary>
        public string LastAward
        {
            get
            {
                for (int i = rooms.Count - 1; i >= 0; i--) if (rooms[i].award != null) return rooms[i].award;
                return null;
            }
        }

        /// <summary>The door that leads out of room i (into room i + 1); null for the last room.</summary>
        public DoorDef DoorAfter(int i) => i >= 0 && i < doors.Count ? doors[i] : null;

        static Vec3 V(object o)
        {
            List<object> a = PortData.Arr(o);
            return new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]));
        }

        /// <summary>A room's stand-in rows (NIGHT29 standIns; NIGHT31 standIns31) into `into`.</summary>
        static void StandIns(List<object> rows, List<StandInDef> into)
        {
            foreach (object so in rows)
            {
                Dictionary<string, object> sd = PortData.Obj(so);
                var si = new StandInDef { at = V(sd["at"]), strafe = sd.TryGetValue("mode", out object sm) && sm as string == "strafe" };
                if (sd.TryGetValue("spanLz", out object sp2)) { List<object> sa = PortData.Arr(sp2); si.lzA = PortData.Num(sa[0]); si.lzB = PortData.Num(sa[1]); }
                into.Add(si);
            }
        }

        /// <summary>NIGHT43 E1: a room row's rooms43 fields (afterLesson43, fallReturn43). Read always, used only with rooms43 on.</summary>
        static void ReadRooms43(Dictionary<string, object> r, RoomDef room)
        {
            if (r.TryGetValue("afterLesson43", out object al) && al != null)
            {
                if (al is List<object> all)
                {
                    room.afterLesson43 = new int[all.Count];
                    for (int i = 0; i < all.Count; i++) room.afterLesson43[i] = all[i] == null ? -1 : (int)PortData.Num(all[i]);
                }
                else room.afterLesson43 = new[] { (int)PortData.Num(al) };
            }
            if (r.TryGetValue("waveNodes43", out object wn) && wn is List<object> wnl)
            {
                room.waveNodes43 = new List<List<Vec3>>();
                foreach (object w in wnl)
                {
                    if (!(w is List<object> pts)) { room.waveNodes43.Add(null); continue; }
                    var l = new List<Vec3>();
                    foreach (object pt in pts) l.Add(V(pt));
                    room.waveNodes43.Add(l);
                }
            }
            if (r.TryGetValue("fallReturn43", out object fo) && fo is Dictionary<string, object> fd && fd.TryGetValue("belowY", out object fy) && fd.TryGetValue("to", out object ft))
            {
                room.hasFallReturn43 = true;
                room.fallBelowY43 = PortData.Num(fy);
                room.fallTo43 = V(ft);
                room.fallDamage43 = fd.TryGetValue("damage", out object fdm) ? PortData.Num(fdm) : 0;
                room.fallYawDeg43 = fd.TryGetValue("yawDeg", out object fyw) ? PortData.Num(fyw) : double.NaN;
                if (fd.TryGetValue("by", out object byo) && byo is List<object> byl)
                {
                    room.fallBy43 = new List<FallBy43>();
                    foreach (object bo in byl)
                    {
                        if (!(bo is Dictionary<string, object> bd) || !bd.TryGetValue("rect", out object brc) || !bd.TryGetValue("to", out object bto)) continue;
                        List<object> q = PortData.Arr(brc);
                        double ax = PortData.Num(q[0]), az = PortData.Num(q[1]), bx = PortData.Num(q[2]), bz = PortData.Num(q[3]);
                        room.fallBy43.Add(new FallBy43
                        {
                            x0 = Math.Min(ax, bx), z0 = Math.Min(az, bz), x1 = Math.Max(ax, bx), z1 = Math.Max(az, bz), to = V(bto),
                            yawDeg = bd.TryGetValue("yawDeg", out object byw) ? PortData.Num(byw) : double.NaN,
                        });
                    }
                }
            }
        }

        /// <summary>A map JSON's (or a plan's) rooms and doors: the map JSON keeps them under "campaign".</summary>
        public static LevelDef From(Dictionary<string, object> json)
        {
            Dictionary<string, object> c = json.ContainsKey("campaign") ? PortData.Obj(json["campaign"]) : json;
            var level = new LevelDef { id = json.TryGetValue("id", out object id) ? id as string : "", name = json.TryGetValue("name", out object nm) ? nm as string : "" };
            if (level.id == null || level.id == "") level.id = json.TryGetValue("name", out object n2) ? (n2 as string ?? "").ToLowerInvariant() : "";
            if (c.TryGetValue("rank", out object rk)) level.rank = RankTable26.From(rk, null); // NIGHT26: the plan's per-level thresholds (the generator copies `rank` into `campaign`)
            foreach (object o in PortData.Arr(c["rooms"]))
            {
                Dictionary<string, object> r = PortData.Obj(o);
                var room = new RoomDef
                {
                    id = (string)r["id"], name = r.TryGetValue("name", out object rn) ? (string)rn : (string)r["id"], kind = r.TryGetValue("kind", out object k) ? (string)k : "room",
                    par = r.TryGetValue("par", out object p) ? PortData.Num(p) : 0, exit = V(r["exit"]),
                    award = r.TryGetValue("award", out object aw) ? aw as string : null,
                    boss = r.TryGetValue("boss", out object bs) && bs is string bsn && bsn != "" ? bsn : "wyrm",
                    limits = r.TryGetValue("limit", out object lim) ? Limit24.Parse(lim) : 0,
                };
                if (r.TryGetValue("limitReason", out object lr) && lr is string lrs) room.limitReason = lrs;
                if (r.TryGetValue("hazard", out object hzo) && hzo is Dictionary<string, object> hzd && hzd.TryGetValue("rect", out object hzr))
                {
                    List<object> hq = PortData.Arr(hzr);
                    room.hasHazard = true;
                    room.hazardSafe = hzd.TryGetValue("safe", out object hzs) && hzs is bool hzb && hzb; // the tutorial's strip: telegraphs, never hurts
                    room.hx0 = PortData.Num(hq[0]);
                    room.hz0 = PortData.Num(hq[1]);
                    room.hx1 = PortData.Num(hq[2]);
                    room.hz1 = PortData.Num(hq[3]);
                }
                if (r.TryGetValue("rect", out object rc))
                {
                    List<object> rr = PortData.Arr(rc);
                    room.x0 = PortData.Num(rr[0]);
                    room.z0 = PortData.Num(rr[1]);
                    room.x1 = PortData.Num(rr[2]);
                    room.z1 = PortData.Num(rr[3]);
                }
                Dictionary<string, object> sp = PortData.Obj(r["spawn"]);
                room.spawn = V(sp["at"]);
                room.spawnYawDeg = sp.TryGetValue("yawDeg", out object y) ? PortData.Num(y) : 0;
                if (r.TryGetValue("nodes", out object nodes)) foreach (object n in PortData.Arr(nodes)) room.nodes.Add(V(n));
                if (r.TryGetValue("highNodes", out object hn)) foreach (object n in PortData.Arr(hn)) room.nodes.Add(V(n));
                if (r.TryGetValue("lessons", out object les)) foreach (object lo in PortData.Arr(les)) room.lessons.Add(LessonDef.From(PortData.Obj(lo)));
                if (r.TryGetValue("standIns", out object sio) && sio is List<object> sil) StandIns(sil, room.standIns);
                if (r.TryGetValue("respawnY", out object ry)) { room.hasRespawnY = true; room.respawnY = PortData.Num(ry); }
                ReadRooms43(r, room);
                if (r.TryGetValue("waves", out object waves))
                    foreach (object w in PortData.Arr(waves)) room.waves.Add(PortData.Arr(w).ConvertAll(x => (string)x));
                // NIGHT31 W: builder L's rows for design.n31.unlocks (used only through UseUnlocks31)
                if (r.TryGetValue("lessons31", out object les31) && les31 is List<object> l31)
                {
                    room.lessons31 = new List<LessonDef>();
                    foreach (object lo in l31) room.lessons31.Add(LessonDef.From(PortData.Obj(lo)));
                }
                if (r.TryGetValue("standIns31", out object si31) && si31 is List<object> s31) StandIns(s31, room.standIns31 = new List<StandInDef>());
                if (r.TryGetValue("waves31", out object w31) && w31 is List<object> wl31)
                {
                    room.waves31 = new List<List<string>>();
                    foreach (object w in wl31) room.waves31.Add(PortData.Arr(w).ConvertAll(x => (string)x));
                }
                if (r.TryGetValue("pickup31", out object pk31) && pk31 is Dictionary<string, object> pd31)
                    level.pickups31.Add(new PickupDef
                    {
                        id = pd31.TryGetValue("id", out object pid) ? pid as string : room.id + "-P31", room = room.id,
                        kind = pd31.TryGetValue("kind", out object pkd) ? pkd as string : "weaponPickup", at = V(pd31["at"]),
                        weapon = pd31.TryGetValue("weapon", out object pw31) ? pw31 as string : null,
                    });
                level.rooms.Add(room);
            }
            foreach (object o in PortData.Arr(c["doors"]))
            {
                Dictionary<string, object> d = PortData.Obj(o);
                List<object> between = PortData.Arr(d["between"]);
                level.doors.Add(new DoorDef { id = (string)d["id"], a = (string)between[0], b = (string)between[1], checkpoint = !d.TryGetValue("checkpoint", out object cp) || !(cp is bool bb) || bb });
            }
            if (c.TryGetValue("props", out object pro) && pro is List<object> prl)
                foreach (object po in prl)
                {
                    Dictionary<string, object> pd = PortData.Obj(po);
                    if (!(pd.TryGetValue("pickup", out object pk) && pk is bool pkb && pkb)) continue;
                    level.pickups.Add(new PickupDef
                    {
                        id = (string)pd["id"], room = pd.TryGetValue("room", out object prm) ? prm as string : "", kind = (string)pd["kind"], at = V(pd["at"]),
                        weapon = pd.TryGetValue("weapon", out object pw) ? pw as string : null,
                    });
                }
            return level;
        }
    }

    /// <summary>What the rules need from the sim composition (Game/SurvivalSim.Campaign24.cs implements it; the tests use a fake).</summary>
    public interface ICampaignWorld
    {
        Vec3 PlayerFeet { get; }
        bool PlayerDead { get; }
        /// <summary>The id of the zone the point is in ("" outside every zone).</summary>
        string ZoneAt(Vec3 p);
        int EnemiesAlive { get; }
        /// <summary>Place one enemy of `kind` at `at`; false when it could not be placed.</summary>
        bool Spawn(string kind, Vec3 at);
        void KillAllEnemies();
        /// <summary>NIGHT29 roomRules: a stand-in body at `at` (strafing between world z zA and zB when `strafe`). Default: none.</summary>
        void SpawnStandIn(Vec3 at, bool strafe, double zA, double zB) { }
        /// <summary>NIGHT29 pickups: heal `fraction` of max health; false when already full (the pickup stays). Default: nothing.</summary>
        bool Heal(double fraction) => false;
        /// <summary>NIGHT29 roomRules: a fall costs `fraction` of max health (never below 1). Default: nothing.</summary>
        void FallCost(double fraction) { }
        void LockZone(string id);
        void OpenZone(string id);
        void Respawn(Vec3 at, double yawDeg);
        void Revive();
        void SetLimits(int flags);
        void AwardWeapon(string weapon);
        /// <summary>A boss room's fight starts (THE KILN WYRM lives in its own room far from the map: the player is taken to `spawn`). False: no boss can be fought now (the wyrm row is off or its files are missing).</summary>
        bool BeginBoss(out Vec3 spawn, out double yawDeg);
        /// <summary>The boss the last BeginBoss started is dead.</summary>
        bool BossDefeated { get; }
        /// <summary>NIGHT25 W: the boss room's own boss (a plan room's `boss`: "wyrm", "bulwark", "monolith"). A world that knows only the worm keeps these defaults.</summary>
        bool BeginBoss(string boss, out Vec3 spawn, out double yawDeg) => BeginBoss(out spawn, out yawDeg);
        bool BossIsDefeated(string boss) => BossDefeated;
        /// <summary>NIGHT36 (walkin36): the boss fights in the room the player walked into, so its begin keeps the player where they are
        /// (no Respawn into a far arena). Default false: every boss before NIGHT36 takes the player to its own arena.</summary>
        bool BossWalkIn => false;
        /// <summary>NIGHT43 E1 (rooms43's fallReturn43): a fall back to the lip costs `hp` health, never below 1 (DECIDE43 D-5: never a death). Default: nothing.</summary>
        void FallDamage43(double hp) { }
        /// <summary>NIGHT43 E1 (rooms43): the enemies alive that room `room` spawned (its waves, top-ups and stand-ins), not the whole level's.
        /// Default: the whole level's (NIGHT42's door rule).</summary>
        int RoomAlive43(int room) => EnemiesAlive;
        /// <summary>NIGHT43 E1 (rooms43): the room's stand-ins alive (they hold its next wave for at most standInHoldS, never its door). Default 0.</summary>
        int RoomStandIns43(int room) => 0;
    }

    /// <summary>NIGHT43 E1: event "fallReturn43" (a rooms43 fallReturn43 fired: the player is back at `to`, `damage` taken; the view's short fade).</summary>
    public sealed class FallReturn43Event
    {
        public int room;
        public string id = "";
        public double damage, x, y, z;
    }

    /// <summary>NIGHT25: a world that can feed the lesson rule (the sim implements it next to ICampaignWorld; a world that does not has no lessons).</summary>
    public interface ILessonSource
    {
        LessonSample Sample(int step);
        /// <summary>Can this lesson's verb be done in this run (a NIGHT25 block off makes it unavailable: the lesson is passed over, never a soft-lock)?</summary>
        bool LessonAvailable(LessonKind kind);
    }

    /// <summary>The run's IRoundSpawner in every mode: it hands the Director's calls through unchanged, except in a CAMPAIGN run (Campaign
    /// true), where the rooms' own encounter script spawns and the round never clears (the run stays in its fight). SurvivalSim.cs
    /// builds SurvivalRun with it instead of the Director (one line: `spawner = campaignSpawner = new Campaign24Spawner(director)`).</summary>
    public sealed class Campaign24Spawner : IRoundSpawner
    {
        readonly IRoundSpawner inner;
        public bool Campaign;

        public Campaign24Spawner(IRoundSpawner inner) => this.inner = inner;

        public void StartWave(int round, uint seed)
        {
            if (!Campaign) inner.StartWave(round, seed);
        }

        public void Step(double dt)
        {
            if (!Campaign) inner.Step(dt);
        }

        public bool Done => !Campaign && inner.Done;

        public void Clear() => inner.Clear();
    }

    public sealed class RoomEvent
    {
        public int room;
        public string id, name, kind;
        public double par, time;
        public int limits;
        public bool withinPar;
        /// <summary>NIGHT27 (builder A): on limitOn, the room's limitReason (the banner's second line: why the loadout is cut); empty otherwise.</summary>
        public string reason = "";
        public char rank; // S / A / B / C on roomClear with design.n24.campaign.rank on, else '\0'
    }

    /// <summary>NIGHT29 pickups: event "pickup29": a plan pickup was collected (the view hides the prop with the same id).</summary>
    public sealed class PickupEvent29
    {
        public string id, kind, weapon;
    }

    public sealed class WeaponEarnedEvent
    {
        public string weapon, name;
        public int room;
    }

    public sealed class Campaign24Params
    {
        public double retryDelay, waveGap, minSpawnDist, limitBonus, parMultiple;
        /// <summary>design.n24.campaign.rank: a room's rank letter by time over par and deaths (NIGHT24 leftover 2).</summary>
        public bool rankOn;
        public double rankS, rankA, rankB; // time / par at most these for S, A, B (else C)
        public int rankDeathSteps; // letters dropped per death in the room
        /// <summary>design.n24.campaign.limitFlags.enabled (absent: on): NO SLIDE / NO JUMP / MELEE ONLY are honoured; off: masked away.</summary>
        public bool limitFlagsOn;
        /// <summary>design.n25.lesson.punchFireSteps: a punch and a shot this many fixed steps apart count as together (the punchWhileFiring lesson).</summary>
        public int lessonPunchSteps;
        /// <summary>design.n25.lesson.maxTopUps: the most times a room respawns its last wave for an enemy-dependent lesson (0 or absent: no cap).</summary>
        public int maxTopUps;
        public static Campaign24Params From(GameConfig cfg) => new Campaign24Params
        {
            maxTopUps = cfg.Has("design.n25.lesson.maxTopUps") ? (int)cfg.Num("design.n25.lesson.maxTopUps") : 0,
            lessonPunchSteps = cfg.Has("design.n25.lesson.punchFireSteps") ? (int)cfg.Num("design.n25.lesson.punchFireSteps") : 1,
            retryDelay = cfg.Num("design.n24.campaign.retryDelay"), waveGap = cfg.Num("design.n24.campaign.waveGap"),
            minSpawnDist = cfg.Num("design.n24.campaign.minSpawnDist"), limitBonus = cfg.Num("design.n24.campaign.limitBonus"),
            parMultiple = cfg.Num("design.n24.campaign.parMultiple"),
            rankOn = cfg.Has("design.n24.campaign.rank.enabled") && cfg.Design("n24.campaign.rank.enabled"),
            rankS = cfg.Has("design.n24.campaign.rank.sShare") ? cfg.Num("design.n24.campaign.rank.sShare") : 1,
            rankA = cfg.Has("design.n24.campaign.rank.aShare") ? cfg.Num("design.n24.campaign.rank.aShare") : 1.5,
            rankB = cfg.Has("design.n24.campaign.rank.bShare") ? cfg.Num("design.n24.campaign.rank.bShare") : 2.5,
            rankDeathSteps = cfg.Has("design.n24.campaign.rank.deathDrop") ? (int)cfg.Num("design.n24.campaign.rank.deathDrop") : 1,
            limitFlagsOn = !cfg.Has("design.n24.campaign.limitFlags.enabled") || cfg.Design("n24.campaign.limitFlags.enabled"),
        };
    }

    public sealed class Campaign24
    {
        public enum Phase { None, Fight, Cleared }

        /// <summary>The run mode's id (RunSetup.mode, the tape's mode string).</summary>
        public const string Mode = "campaign";

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly LevelDef level;
        readonly ICampaignWorld world;
        readonly Campaign24Params p;
        readonly double dt;
        uint seed;
        Mulberry32 rng = new Mulberry32(1);

        public int room = -1; // the room the player is in (-1: not yet entered)
        public int checkpoint;
        public Phase phase = Phase.None;
        public int limits; // the LIMIT flags in force now
        public bool finished;
        /// <summary>NIGHT26: the unlock table (null: the plan's room awards only); the sim sets it from Design/levels/unlocks.json.</summary>
        public Unlocks26 unlocks;
        public int deaths, retries;
        public int wave; // waves spawned in this room
        public int steps, roomSteps;
        public readonly List<int> clearSteps = new List<int>();
        readonly double linkReach; // design.n24.campaign.links.reach (m): a link's exit counts when the player is this close (horizontal and vertical)
        /// <summary>Deaths per room index (the layer's per-room clear rate: a room cleared with 0 deaths was cleared first try).</summary>
        public readonly List<int> roomDeaths = new List<int>();
        /// <summary>The rank letter of each cleared room (S/A/B/C; filled with design.n24.campaign.rank on, in the order the rooms were cleared).</summary>
        public readonly List<char> roomRanks = new List<char>();
        /// <summary>NIGHT25: the lesson rule of the room the player is in (null: no lessons here), and how many stage respawns the run had.</summary>
        public Lesson25 lesson;
        public int stageRespawns, lessonTopUps, roomTopUps;
        /// <summary>NIGHT29 DOORFREE (design.n29.doorFree; the sim sets it through SetDoorFree29): every door stands open, a room is entered from any room, no door locks behind the player. `unranked29`: the toggle has been on this level: no rank, no record.</summary>
        public bool doorFree29, unranked29;
        /// <summary>NIGHT29 round 2 (the sim sets them at the campaign's start): design.n29.pickups on / design.n29.roomRules on.</summary>
        public bool pickups29, roomRules29;
        /// <summary>NIGHT30 teach (design.n30.teach; the sim sets it at the campaign's start): a room's lesson line is cleared (a `lessonClear`) when the
        /// room is CLEARED or LEFT (another room entered, a boss room included), so no line from one room stays on screen in the next (GG's frame:
        /// i6's "TANKS FIRST. PUNCH THEM." over the Glacier fight). Off: NIGHT29's rule (a line stays until the next lessonBegin).</summary>
        public bool teach30;
        /// <summary>NIGHT30: the room the player is in is a BOSS room (the boss bar's gate, P0 d).</summary>
        public bool InBossRoom => room >= 0 && room < level.rooms.Count && level.rooms[room].kind == "boss";

        /// <summary>NIGHT30 teach: the line of the lesson still running (not done) is cleared now, and the lesson dropped.</summary>
        void DropLesson30()
        {
            if (!teach30 || lesson == null || room < 0 || room >= level.rooms.Count) return;
            if (!lesson.Done) EmitLesson("lessonClear", level.rooms[room], lesson.Current);
            lesson = null;
        }

        /// <summary>NIGHT30 teach (the r6 frame: s1's line over THE LOOM after a respawn into s8): the player stands in another room than the campaign's current one
        /// (a respawn, DOORFREE, a boss path): the current room's lesson line goes and its lesson stops (it would begin the line again). True when it cleared one.</summary>
        public bool DropLessonFor30(string roomId)
        {
            if (!teach30 || lesson == null || room < 0 || room >= level.rooms.Count || level.rooms[room].id != roomId) return false;
            bool emitted = !lesson.Done;
            DropLesson30();
            return emitted;
        }
        /// <summary>The plan pickups collected this run (ids): the view reads IsPickupTaken. Weapons they gave: the unlock / award routes do not give them twice.</summary>
        public readonly HashSet<string> takenPickups = new HashSet<string>();
        readonly HashSet<string> givenByPickup = new HashSet<string>();
        public bool IsPickupTaken(string id) => takenPickups.Contains(id);
        /// <summary>NIGHT31 W (design.n31.unlocks; the sim sets it, SurvivalSim.Weapons31.cs): the run's loadout is the n31 table's, and a plan
        /// pickup of a weapon the run was already given (a room award: Award31) is taken without a second award. Off: NIGHT30's pickups.</summary>
        public bool unlocks31;
        /// <summary>NIGHT31 W: has this run been given `weapon` (a pickup or Award31)? The room award, the level's unlock and a pickup skip it then.</summary>
        public bool Given31(string weapon) => givenByPickup.Contains(weapon);
        /// <summary>NIGHT31 W: mark `weapon` as given without an award (the loadout already holds it, e.g. after a Reset).</summary>
        public void MarkGiven31(string weapon) => givenByPickup.Add(weapon);
        /// <summary>NIGHT31 W (design.n31.unlocks): a room's award on entry (level 2's PULSE gauntlet in i1): the weapon through the loadout, then
        /// "weaponEarned" (the banner and the teaching line), once a run (the pickup in that room, the room award and the level's unlock skip it).</summary>
        public void Award31(string weapon)
        {
            if (string.IsNullOrEmpty(weapon) || givenByPickup.Contains(weapon)) return;
            givenByPickup.Add(weapon);
            world.AwardWeapon(weapon);
            events.Emit("weaponEarned", new WeaponEarnedEvent { weapon = weapon, name = cfg.Has("weapons.names." + weapon) ? (string)cfg.Get("weapons.names." + weapon) : weapon.ToUpperInvariant(), room = room });
        }
        bool bossOn;
        Vec3 bossSpawn;
        double bossYaw;
        int gapLeft, retryLeft;
        int spawnFails;

        public LevelDef Level => level;
        public RoomDef Room => room >= 0 ? level.rooms[room] : null;
        public Campaign24(GameConfig cfg, EventBus events, LevelDef level, ICampaignWorld world, double dt)
        {
            this.cfg = cfg;
            this.events = events;
            this.level = level;
            this.world = world;
            this.dt = dt;
            p = Campaign24Params.From(cfg);
            linkReach = cfg.Num("design.n24.campaign.links.reach");
            const string R43 = "design.n43.rooms43."; // NIGHT43 E1: read always, used only with rooms43 on
            if (cfg.Has(R43 + "maxTopUps")) maxTopUps43 = (int)cfg.Num(R43 + "maxTopUps");
            if (cfg.Has(R43 + "lessonCapS")) lessonCapSteps43 = Math.Max(1, MovementMath.StepsFor(cfg.Num(R43 + "lessonCapS"), dt));
            commitSteps43 = cfg.Has(R43 + "commitMaxS") ? Math.Max(1, MovementMath.StepsFor(cfg.Num(R43 + "commitMaxS"), dt)) : 0; // absent: no wait
            commitM43 = cfg.Has(R43 + "commitM") ? cfg.Num(R43 + "commitM") : 0;
            if (cfg.Has(R43 + "standInHoldS")) standInHoldSteps43 = Math.Max(1, MovementMath.StepsFor(cfg.Num(R43 + "standInHoldS"), dt));
            if (cfg.Has(R43 + "afterLessonMaxS")) afterLessonMaxSteps43 = Math.Max(1, MovementMath.StepsFor(cfg.Num(R43 + "afterLessonMaxS"), dt));
        }

        /// <summary>The tape's state (levelId, room, checkpoint, limits).</summary>
        public Campaign24State State => new Campaign24State { levelId = level.id, room = Math.Max(0, room), checkpoint = checkpoint, limits = limits };

        /// <summary>A new run: nothing entered, the stream seeded from the run seed.</summary>
        public void Start(uint runSeed)
        {
            seed = runSeed;
            room = -1;
            checkpoint = 0;
            phase = Phase.None;
            limits = 0;
            finished = false;
            doorFree29 = unranked29 = false;
            takenPickups.Clear();
            givenByPickup.Clear();
            deaths = retries = wave = steps = roomSteps = gapLeft = retryLeft = spawnFails = 0;
            clearSteps.Clear();
            roomDeaths.Clear();
            roomRanks.Clear();
        }

        /// <summary>The rooms' par times: the bot must clear a room within parMultiple x par.</summary>
        public double ParOf(int i) => level.rooms[i].par;

        void SeedRoom(int i) => rng = new Mulberry32((double)unchecked(seed + (uint)(i + 1) * 2654435761u)); // rule 6: the run's seed and the room, the same on every retry

        void Enter(int i)
        {
            DropLesson30(); // NIGHT30 teach: the room left takes its line with it
            room = i;
            RoomDef r = level.rooms[i];
            if (i > 0 && !doorFree29) world.LockZone(level.doors[i - 1].id); // (NIGHT29 DOORFREE: none locks) the door behind locks: the player is past it (inside the room's zone)
            checkpoint = i;
            roomSteps = 0;
            wave = 0;
            gapLeft = 0;
            spawnFails = 0;
            SeedRoom(i);
            SetLimits(p.limitFlagsOn ? r.limits : r.limits & ~Limit24.Extra);
            events.Emit("roomEnter", new RoomEvent { room = i, id = r.id, name = r.name, kind = r.kind, par = r.par, limits = r.limits });
            if (i > 0 && level.doors[i - 1].checkpoint) events.Emit("checkpoint", new RoomEvent { room = i, id = r.id, name = r.name });
            phase = Phase.Fight; // a link (no waves) clears on its first fight step
            lesson = null;
            roomTopUps = 0;
            lessonIdle43 = 0;
            if (r.lessons.Count > 0 && world is ILessonSource ls)
            {
                lesson = new Lesson25(r.lessons, p.lessonPunchSteps);
                lesson.Begin(ls.Sample(steps), ls.LessonAvailable);
                EmitLesson("lessonBegin", r, null);
            }
            if (roomRules29 && (!rooms43 || commitOff43 || StandInsNeeded43(r))) // NIGHT43 E1 (G4): with rooms43 a room's stand-ins stand only where a lesson shoots at them
                foreach (StandInDef si in r.standIns)
                    world.SpawnStandIn(si.at, si.strafe, r.z1 - si.lzA, r.z1 - si.lzB); // NIGHT29: the wall-ride room's example targets
            bossOn = false;
            if (r.kind == "boss")
            {
                bossOn = world.BeginBoss(r.boss, out bossSpawn, out bossYaw);
                if (bossOn && !world.BossWalkIn) world.Respawn(bossSpawn, bossYaw); // the fight is in its own room (NIGHT36 walkin36: or here, where the player walked in)
            }
        }

        void SetLimits(int flags)
        {
            if (flags == limits) return;
            limits = flags;
            world.SetLimits(flags);
            events.Emit(flags != 0 ? "limitOn" : "limitOff", new RoomEvent { room = room, limits = flags, name = Limit24.Banner(flags), reason = flags != 0 && room >= 0 && room < level.rooms.Count ? level.rooms[room].limitReason : "" });
        }

        /// <summary>One fixed step (after the enemies' step in the sim).</summary>
        public void Step()
        {
            steps++;
            if (finished) return;
            if (world.PlayerDead && retryLeft == 0)
            {
                retryLeft = Math.Max(1, MovementMath.StepsFor(p.retryDelay, dt));
                deaths++;
                int at = Math.Max(0, room);
                while (roomDeaths.Count <= at) roomDeaths.Add(0);
                roomDeaths[at]++;
            }
            if (retryLeft > 0)
            {
                if (--retryLeft == 0) Retry();
                return;
            }
            bool fell43 = rooms43 && StepFallReturn43(); // NIGHT43 E1: a room's fallReturn43 answers first (the respawnY rule is not asked on that step)
            if (!fell43 && room >= 0 && phase != Phase.None && level.rooms[room].hasRespawnY)
            {
                RoomDef sr = level.rooms[room];
                if (world.PlayerFeet.y < sr.respawnY && world.ZoneAt(world.PlayerFeet) != level.rooms[Math.Min(level.rooms.Count - 1, room + 1)].id) // NIGHT25: no fail state: a miss (the pit's floor) is back at the stage's start, in a cleared stage too (the open door does not strand a player in the pit)
                {
                    stageRespawns++;
                    if (roomRules29) world.FallCost(cfg.Num("design.n29.roomRules.fallCostFraction")); // NIGHT29: a fall costs a share of health (DESIGN29 A)
                    world.Respawn(sr.spawn, sr.spawnYawDeg);
                    events.Emit("stageRespawn", new RoomEvent { room = room, id = sr.id, name = sr.name });
                }
            }
            if (pickups29 && level.pickups.Count > 0) StepPickups();
            if (doorFree29)
            {
                // NIGHT29 DOORFREE: the zone the player stands in decides: any room ahead (a boss room from the first room) is entered, its wave spawns as usual
                int zi = RoomOfZone(world.ZoneAt(world.PlayerFeet));
                bool normal = zi == room + 1 && (phase == Phase.None || phase == Phase.Cleared);
                if (zi > room && !normal)
                {
                    world.KillAllEnemies(); // the room left behind stays unfinished: its enemies do not follow into the next one
                    Enter(zi);
                }
            }
            if (phase == Phase.None || (phase == Phase.Cleared && room + 1 < level.rooms.Count))
            {
                int next = room + 1;
                if (next < level.rooms.Count && world.ZoneAt(world.PlayerFeet) == level.rooms[next].id) Enter(next);
            }
            if (phase == Phase.Fight) StepFight();
        }

        /// <summary>NIGHT29 pickups: walking onto a plan pickup (within design.n29.pickups.reachMetres of its centre, its feet band) takes it: the weapon through the loadout and
        /// weaponEarned (once: the room award and the unlock table skip it), or a heal; then "pickup29" fires and IsPickupTaken is true.</summary>
        void StepPickups()
        {
            Vec3 f = world.PlayerFeet;
            double reach = cfg.Num("design.n29.pickups.reachMetres"), below = cfg.Num("design.n29.pickups.reachBelow"), above = cfg.Num("design.n29.pickups.reachAbove");
            foreach (PickupDef pk in level.pickups)
            {
                if (takenPickups.Contains(pk.id)) continue;
                double dx = f.x - pk.at.x, dz = f.z - pk.at.z;
                if (dx * dx + dz * dz > reach * reach || f.y < pk.at.y - below || f.y > pk.at.y + above) continue;
                string w = null;
                if (pk.kind == "weaponPickup")
                {
                    w = WeaponId(pk.weapon);
                    if (w == null) continue;
                    if (!(unlocks31 && givenByPickup.Contains(w))) // NIGHT31 W: a weapon the run already got (Award31) is taken without a second award
                    {
                        givenByPickup.Add(w);
                        world.AwardWeapon(w);
                        events.Emit("weaponEarned", new WeaponEarnedEvent { weapon = w, name = cfg.Has("weapons.names." + w) ? (string)cfg.Get("weapons.names." + w) : w.ToUpperInvariant(), room = room });
                    }
                }
                else if (!world.Heal(cfg.Num("design.n29.pickups.healFraction"))) continue;
                takenPickups.Add(pk.id);
                events.Emit("pickup29", new PickupEvent29 { id = pk.id, kind = pk.kind, weapon = w });
            }
        }

        /// <summary>A plan's weapon word (a loadout id or the weapon's name: "thunderclap") as the loadout id; null when unknown.</summary>
        string WeaponId(string word)
        {
            if (string.IsNullOrEmpty(word)) return null;
            if (cfg.Has("weapons.names." + word)) return word;
            foreach (var kv in cfg.Obj("weapons.names"))
                if (kv.Value is string nm && string.Equals(nm, word, StringComparison.OrdinalIgnoreCase)) return kv.Key;
            return null;
        }

        int RoomOfZone(string zone)
        {
            if (string.IsNullOrEmpty(zone)) return -1;
            for (int i = 0; i < level.rooms.Count; i++) if (level.rooms[i].id == zone) return i;
            return -1;
        }

        /// <summary>
        /// NIGHT29 DOORFREE (the sim calls it when the hold toggles; Core/DoorFree29.cs): ON opens every door zone and every room zone of the level (and the run is
        /// unranked from here); OFF puts the locks back as the normal flow has them (a door is open only right after its room is cleared; a room ahead is locked),
        /// except a zone the player stands in (a lock never closes on a body).
        /// </summary>
        public void SetDoorFree29(bool on)
        {
            doorFree29 = on;
            if (on)
            {
                unranked29 = true;
                foreach (DoorDef d in level.doors) world.OpenZone(d.id);
                foreach (RoomDef r in level.rooms) world.OpenZone(r.id);
                return;
            }
            string here = world.ZoneAt(world.PlayerFeet);
            for (int j = 0; j < level.doors.Count; j++)
            {
                bool open = j == room && phase == Phase.Cleared;
                if (!open && level.doors[j].id != here) world.LockZone(level.doors[j].id);
            }
            for (int k = 0; k < level.rooms.Count; k++)
            {
                bool open = k <= Math.Max(room, 0) || (k == room + 1 && phase == Phase.Cleared);
                if (!open && level.rooms[k].id != here) world.LockZone(level.rooms[k].id);
            }
        }

        void EmitLesson(string name, RoomDef r, LessonDef d)
        {
            LessonDef cur = d ?? lesson?.Current;
            events.Emit(name, new LessonEvent
            {
                room = room, id = r.id, index = lesson != null ? lesson.index : 0, count = r.lessons.Count, time = roomSteps * dt,
                kind = cur != null ? LessonDef.KindName(cur.kind) : "", prompt = cur != null && cur.kind == LessonKind.PunchWhileFiring && lesson != null && lesson.lockoutOrder && cfg.Has("design.n29.lockout.lessonPrompt") ? cfg.Str("design.n29.lockout.lessonPrompt") : cur?.prompt ?? "", promptPad = cur?.promptPad ?? "",
            });
        }

        /// <summary>The stage's lessons for one step; true when they are all cleared (or the room has none).</summary>
        bool StepLessons(RoomDef r)
        {
            if (lesson == null) return true;
            var ls = (ILessonSource)world;
            if (lesson.Step(ls.Sample(steps), ls.LessonAvailable))
            {
                EmitLesson("lessonClear", r, lesson.Cleared);
                if (!lesson.Done) EmitLesson("lessonBegin", r, null);
            }
            return lesson.Done;
        }

        void StepFight()
        {
            roomSteps++;
            RoomDef r = level.rooms[room];
            if (r.kind == "boss") // the boss room clears when the boss is dead (no boss to fight: at once)
            {
                if (!bossOn || world.BossIsDefeated(r.boss)) Clear(r);
                return;
            }
            bool lessonsDone = StepLessons(r);
            int alive = (rooms43 ? world.RoomAlive43(room) : world.EnemiesAlive) + (pending33 != null ? pending33() : 0); // NIGHT33 E: enemies inside their spawn tell count as alive; NIGHT43 E1: only this room's own
            if (rooms43 && alive > 0) lessonIdle43 = 0;
            if (wave < r.WaveCount)
            {
                // NIGHT43 E1: the room's stand-ins (a lesson's example targets) hold its next wave as in NIGHT42 (the KESTREL room: shoot them, then the
                // wave), but at most standInHoldS after the entry; they never hold the door (RoomAlive43 leaves them out)
                if (rooms43 && roomSteps < Hold46(Hold46StandIn, standInHoldSteps43)) alive += world.RoomStandIns43(room);
                bool ready = (waveReady33 == null || waveReady33(r, wave)) && (waveReady40 == null || waveReady40(r, wave)) // NIGHT33 E: a triggered wave waits for its line; NIGHT40 S1: the start grace
                    && (!rooms43 || LessonReached43(r.AfterLesson43(wave)) || roomSteps >= Hold46(Hold46AfterLesson, afterLessonMaxSteps43)) // NIGHT43 E1: a wave with afterLesson43 waits for its lesson (at most afterLessonMaxS in the room)
                    && (!rooms43 || wave > 0 || Committed43(r)); // NIGHT43 E1 (G4, round 2): a room's first wave waits until the player has come into it
                bool early = ready && alive > 0 && ((waveEarly33 != null && waveEarly33(r, wave)) || (waveRelease46 != null && waveRelease46(r, wave, alive))); // NIGHT46 F1 (overlap46: waveRelease46; null = NIGHT45); NIGHT33 E: an early wave comes in on top of the living
                if (!ready) { }
                else if (alive == 0 || early)
                {
                    if (gapLeft > 0 && !early) gapLeft--;
                    else
                    {
                        SpawnWave(r, wave);
                        wave++;
                        gapLeft = Math.Max(0, MovementMath.StepsFor(p.waveGap, dt));
                    }
                }
            }
            else if (!lessonsDone && alive == 0 && r.WaveCount > 0 && lesson.PendingNeedsEnemy && !(rooms43 && maxTopUps43 >= 0 && roomTopUps >= maxTopUps43)) // NIGHT43 E1: past its top-ups the room goes to the lesson cap below (NIGHT42's cap skipped nothing when a verb lesson came first, and fired every step)
            {
                // the enemies are dead and a lesson that needs one is not done (the husks died early): the last wave again, never a soft-lock
                if (gapLeft > 0) gapLeft--;
                else if (p.maxTopUps > 0 && roomTopUps >= p.maxTopUps)
                {
                    lesson.SkipPendingEnemyLessons(); // the guard: a room never respawns its last wave forever; the lessons that need an enemy are passed over (logged)
                    events.Emit("lessonTopUpCap", new RoomEvent { room = room, id = r.id, name = r.name });
                }
                else
                {
                    SpawnWave(r, r.WaveCount - 1);
                    lessonTopUps++;
                    roomTopUps++;
                    gapLeft = Math.Max(0, MovementMath.StepsFor(p.waveGap, dt));
                }
            }
            else if (rooms43 && !lessonsDone && alive == 0 && r.WaveCount > 0)
            {
                // NIGHT43 E1: the room's enemies are dead and a lesson (a verb) is not done: after lessonCapS the lessons are marked done and the door
                // opens on the next step (a verb lesson never holds a fight room's door forever; GG G5)
                if (++lessonIdle43 >= Hold46(Hold46LessonCap, lessonCapSteps43))
                {
                    EmitLesson("lessonClear", r, lesson.Current);
                    lesson.index = lesson.defs.Count;
                    lessonCaps43++;
                    events.Emit("lessonCap43", new RoomEvent { room = room, id = r.id, name = r.name });
                }
            }
            else if (alive == 0 && r.kind == "link" && lesson != null)
            {
                if (lessonsDone) Clear(r); // a taught link is cleared by its lessons (the last one stands the player on the far side)
            }
            else if (alive == 0 && r.kind == "link")
            {
                Vec3 f = world.PlayerFeet; // a traversal link is cleared by reaching its exit (its time is the crossing)
                double dx = f.x - r.exit.x, dz = f.z - r.exit.z;
                if (dx * dx + dz * dz <= linkReach * linkReach && Math.Abs(f.y - r.exit.y) <= linkReach) Clear(r);
            }
            else if (alive == 0 && r.kind != "boss" && lessonsDone) Clear(r);
        }

        void SpawnWave(RoomDef r, int w)
        {
            Vec3 feet = world.PlayerFeet;
            List<Vec3> nodes = (rooms43 ? r.WaveNodes43(w) : null) ?? waveNodes33?.Invoke(r, w) ?? r.nodes; // NIGHT33 E: a triggered wave's own spots (null: the room's); NIGHT43 E1: a wave's own waveNodes43 first
            foreach (string kind in r.waves[w])
            {
                if (nodes.Count == 0) break;
                bool placed = false;
                int start = (int)(rng.Next() * nodes.Count);
                for (int t = 0; t < nodes.Count && !placed; t++)
                {
                    Vec3 n = nodes[(start + t) % nodes.Count];
                    double dx = n.x - feet.x, dz = n.z - feet.z;
                    if (t < nodes.Count - 1 && Math.Sqrt(dx * dx + dz * dz) < p.minSpawnDist) continue; // the farthest-first fallback: the last try ignores the distance
                    placed = spawn33 != null ? spawn33(kind, n) : world.Spawn(kind, n); // NIGHT33 E: through the spawn tell
                }
                if (!placed) spawnFails++;
            }
        }

        // ---- NIGHT33 E (design.n33.spawns33; DESIGN33 E, D-7): trigger spawns and the spawn tell. The sim sets these while the row is on (Game/SurvivalSim.Spawns33.cs);
        // all null (the row off) = NIGHT32's rooms bit for bit.
        /// <summary>May wave w of room r spawn now (its trigger line crossed)? Null: always.</summary>
        public Func<RoomDef, int, bool> waveReady33;
        /// <summary>NIGHT40 S1 (start40): a wave waits while this says false (null: never waits; NIGHT39's game). Set by S1's sim partial.</summary>
        public Func<RoomDef, int, bool> waveReady40;
        /// <summary>Does wave w come in as soon as it is ready, on top of the living (ULTRAKILL's early waves)? Null: never.</summary>
        public Func<RoomDef, int, bool> waveEarly33;
        /// <summary>Wave w's own spawn spots (the trigger's far side), or null for the room's nodes.</summary>
        public Func<RoomDef, int, List<Vec3>> waveNodes33;
        /// <summary>In place of world.Spawn: queue the enemy behind its spawn tell (true: queued). Null: at once.</summary>
        public Func<string, Vec3, bool> spawn33;
        /// <summary>Enemies inside their tell (counted as alive: a room never clears or moves on under a tell). Null: none.</summary>
        public Func<int> pending33;

        // ---- NIGHT46 (Tools/night/n46-shared.cjs; design.n46 sim rows, set by builder F1 in SurvivalSim.N46.Flow.cs's N46FlowCampaign). All null = NIGHT45's
        // rooms bit for bit (the fingerprint's and every older tape's).
        /// <summary>NIGHT46 retry46: after a death, the wave the room restarts at (room, waves spawned so far); null: 0 (wave 1).</summary>
        public Func<RoomDef, int, int> retryWave46;
        /// <summary>NIGHT46 overlap46: release the next wave now, on top of the living (room, next wave, alive)? Null: never (only at 0 alive).</summary>
        public Func<RoomDef, int, int, bool> waveRelease46;
        /// <summary>NIGHT46 teach46: a teaching hold's steps (which: Hold46StandIn / Hold46AfterLesson / Hold46LessonCap; rooms43's steps); null: rooms43's.</summary>
        public Func<int, int, int> teachHold46;
        public const int Hold46StandIn = 0, Hold46AfterLesson = 1, Hold46LessonCap = 2;
        int Hold46(int which, int steps43) => teachHold46 != null ? teachHold46(which, steps43) : steps43;

        // ---- NIGHT43 E1 (design.n43.rooms43; NIGHT43.md E1, GG G4 / G5 / G1, DECIDE43 D-5). The sim sets rooms43 at the campaign's start
        // (SurvivalSim.Campaign24.cs); false = NIGHT42's rooms bit for bit (nothing below is asked).
        /// <summary>NIGHT43 E1: the rooms43 rules are on for this run (afterLesson43, fallReturn43, the door rules).</summary>
        public bool rooms43;
        /// <summary>NIGHT43 E1: fallReturn43s fired this run (the harness's count).</summary>
        public int fallReturns43;
        /// <summary>NIGHT43 E1: lesson caps this run (a verb lesson marked done lessonCapS after its room's last kill).</summary>
        public int lessonCaps43;
        readonly FallReturn43Event fall43Ev = new FallReturn43Event();
        int lessonIdle43;
        // design.n43.rooms43: maxTopUps (-1 when absent: NIGHT42's design.n25.lesson.maxTopUps), lessonCapS, afterLessonMaxS (in steps; huge when absent)
        readonly int maxTopUps43 = -1, lessonCapSteps43 = int.MaxValue, afterLessonMaxSteps43 = int.MaxValue, standInHoldSteps43 = int.MaxValue, commitSteps43;
        readonly double commitM43;

        /// <summary>NIGHT43 E1 (G4 round 2; the harness's control only): the commit gate and the stand-in rule off while rooms43's other rules stay on.</summary>
        public bool commitOff43;

        /// <summary>NIGHT43 E1 (G4 "teach, confirm, then spawn"; CODE-FACTS-A B7: arena36 rooms bypass spawns33's trigger lines, so a room's
        /// waves started at its entry, in view of a player still in the doorway): has the player come into room r: the feet commitM (3 m) or more
        /// from the room's spawn (the doorway's side), or commitMaxS (3 s) in the room? The first wave waits for it.</summary>
        bool Committed43(RoomDef r)
        {
            if (commitOff43 || roomSteps >= commitSteps43) return true;
            Vec3 f = world.PlayerFeet;
            double dx = f.x - r.spawn.x, dz = f.z - r.spawn.z;
            return dx * dx + dz * dz >= commitM43 * commitM43;
        }

        /// <summary>NIGHT43 E1 (G4 round 2): does a lesson of room r shoot at its stand-ins (shots, targets hit, the ride's piercing shot)?
        /// Without one the stand-ins are not placed (THE CHARGE's three stood in view for a boost lesson that never shoots them).</summary>
        static bool StandInsNeeded43(RoomDef r)
        {
            foreach (LessonDef l in r.lessons)
                if (l.kind == LessonKind.Shots || l.kind == LessonKind.TargetsHit || l.kind == LessonKind.WallRiddenAndFired) return true;
            return false;
        }

        /// <summary>NIGHT43 E1 (U1's counter43 through SurvivalSim.N43RoomLeft): the enemies left in the fight room the player is in: alive (its own),
        /// inside a spawn tell, and in the waves still to come; -1 when there is no locked fight (rooms43 off, a link, a boss room, cleared).</summary>
        public int RoomLeft43()
        {
            if (!rooms43 || phase != Phase.Fight || room < 0 || room >= level.rooms.Count) return -1;
            RoomDef r = level.rooms[room];
            if (r.kind == "boss" || r.WaveCount == 0) return -1;
            int n = world.RoomAlive43(room) + (pending33 != null ? pending33() : 0);
            for (int w = wave; w < r.WaveCount; w++) n += r.waves[w].Count;
            return n;
        }

        /// <summary>NIGHT43 E1: is lesson `index` of the room in progress done (index &lt; 0, no lesson rule here, or the stage past it)?</summary>
        bool LessonReached43(int index) => index < 0 || lesson == null || lesson.Done || lesson.index > index;

        /// <summary>NIGHT43 E1: the current room's fallReturn43: the feet below belowY inside the room's rect (the zone when the room has no
        /// rect) -> back at `to`, the damage, the event. True when it fired this step.</summary>
        bool StepFallReturn43()
        {
            if (room < 0 || room >= level.rooms.Count || phase == Phase.None) return false;
            RoomDef r = level.rooms[room];
            if (!r.hasFallReturn43) return false;
            Vec3 f = world.PlayerFeet;
            if (f.y >= r.fallBelowY43) return false;
            bool inside = r.x1 > r.x0 || r.z1 > r.z0 ? f.x >= Math.Min(r.x0, r.x1) && f.x <= Math.Max(r.x0, r.x1) && f.z >= Math.Min(r.z0, r.z1) && f.z <= Math.Max(r.z0, r.z1) : world.ZoneAt(f) == r.id;
            if (!inside) return false;
            fallReturns43++;
            Vec3 to = r.fallTo43;
            double yaw = double.IsNaN(r.fallYawDeg43) ? r.spawnYawDeg : r.fallYawDeg43;
            if (r.fallBy43 != null)
                for (int i = 0; i < r.fallBy43.Count; i++)
                    if (r.fallBy43[i].Holds(f)) // round 2: the pit the feet are in picks its own lip
                    {
                        to = r.fallBy43[i].to;
                        if (!double.IsNaN(r.fallBy43[i].yawDeg)) yaw = r.fallBy43[i].yawDeg;
                        break;
                    }
            world.Respawn(to, yaw);
            if (r.fallDamage43 > 0) world.FallDamage43(r.fallDamage43);
            fall43Ev.room = room;
            fall43Ev.id = r.id;
            fall43Ev.damage = r.fallDamage43;
            fall43Ev.x = to.x;
            fall43Ev.y = to.y;
            fall43Ev.z = to.z;
            events.Emit("fallReturn43", fall43Ev);
            return true;
        }

        /// <summary>NIGHT33 E: the world's own spawn (the tell's end).</summary>
        public bool SpawnNow33(string kind, Vec3 at) => world.Spawn(kind, at);

        /// <summary>NIGHT33 E: the wave the room spawns next (the trigger hooks read it).</summary>
        public int Wave33 => wave;

        /// <summary>A boss room is cleared by the boss's own end (the wyrm's builder calls this when it dies).</summary>
        public void BossDown()
        {
            if (room >= 0 && level.rooms[room].kind == "boss" && phase == Phase.Fight) Clear(level.rooms[room]);
        }

        void Clear(RoomDef r)
        {
            DropLesson30(); // NIGHT30 teach: a cleared room's line goes
            phase = Phase.Cleared;
            clearSteps.Add(roomSteps);
            double time = roomSteps * dt;
            bool withinPar = r.par <= 0 || time <= r.par;
            char rank = p.rankOn && !unranked29 ? RankOf(time, r.par, DeathsIn(room)) : '\0'; // NIGHT29 DOORFREE: an unranked run has no letters
            if (rank != '\0') roomRanks.Add(rank);
            int hadLimits = limits;
            SetLimits(0); // restored at the door
            if (hadLimits != 0) events.Emit("styleBonus", new StyleBonusEvent { owner = "player", name = "LIMIT " + Limit24.Banner(hadLimits), points = p.limitBonus });
            events.Emit("roomClear", new RoomEvent { room = room, id = r.id, name = r.name, kind = r.kind, par = r.par, time = time, limits = hadLimits, withinPar = withinPar, rank = rank });
            if (rooms43 && r.WaveCount > 0 && r.kind != "boss") events.Emit(N43Events.RoomClear, r.id); // NIGHT43 E1: a fight room's door opens (payload: the room id)
            if (r.award != null && !givenByPickup.Contains(r.award))
            {
                world.AwardWeapon(r.award);
                events.Emit("weaponEarned", new WeaponEarnedEvent { weapon = r.award, name = cfg.Has("weapons.names." + r.award) ? (string)cfg.Get("weapons.names." + r.award) : r.award.ToUpperInvariant(), room = room });
            }
            DoorDef d = level.DoorAfter(room);
            if (d == null)
            {
                // NIGHT26: the level's own unlock (Design/levels/unlocks.json: builder W's table, set by the sim) is earned with the last room,
                // unless that room's `award` already gave it
                string earned = unlocks?.Earned(level.id);
                if (earned != null && earned != r.award && !givenByPickup.Contains(earned))
                {
                    world.AwardWeapon(earned);
                    events.Emit("weaponEarned", new WeaponEarnedEvent { weapon = earned, name = cfg.Has("weapons.names." + earned) ? (string)cfg.Get("weapons.names." + earned) : earned.ToUpperInvariant(), room = room });
                }
                finished = true;
                events.Emit("layerClear", new RoomEvent { room = room, id = level.id, name = level.name, time = TotalSeconds });
                return;
            }
            world.OpenZone(d.id);
            world.OpenZone(level.rooms[room + 1].id);
        }

        /// <summary>Deaths in room i so far.</summary>
        int DeathsIn(int i) => i >= 0 && i < roomDeaths.Count ? roomDeaths[i] : 0;

        static readonly char[] Letters = { 'S', 'A', 'B', 'C' };

        /// <summary>A room's rank: S when the time is at most sShare x par, A to aShare, B to bShare, else C (par 0: S by time); each death drops
        /// rankDeathSteps letters, never below C.</summary>
        public char RankOf(double seconds, double par, int roomDeathCount)
        {
            int i = par <= 0 || seconds <= par * p.rankS ? 0 : seconds <= par * p.rankA ? 1 : seconds <= par * p.rankB ? 2 : 3;
            return Letters[Math.Min(3, i + Math.Max(0, roomDeathCount) * p.rankDeathSteps)];
        }

        /// <summary>The layer's overall rank: the rooms' letters averaged (S 3, A 2, B 1, C 0), rounded to the nearest letter, ties to the better.</summary>
        public char OverallRank()
        {
            if (roomRanks.Count == 0) return '\0';
            double sum = 0;
            foreach (char c in roomRanks) sum += 3 - Array.IndexOf(Letters, c);
            return Letters[3 - (int)Math.Floor(sum / roomRanks.Count + 0.5)];
        }

        /// <summary>The closing stamp's text: "LAYER 1 CLEARED  S A B  OVERALL A  12:34" (the rooms' letters in the order cleared); empty with the rank block off.</summary>
        public string StampText()
        {
            if (roomRanks.Count == 0) return "";
            double t = TotalSeconds;
            return level.name + " CLEARED  " + string.Join(" ", roomRanks) + "  OVERALL " + OverallRank() + "  " + (int)(t / 60) + ":" + ((int)t % 60).ToString("00");
        }

        /// <summary>Game seconds spent in cleared rooms plus the one in progress.</summary>
        public double TotalSeconds
        {
            get
            {
                int s = phase == Phase.Fight ? roomSteps : 0;
                foreach (int c in clearSteps) s += c;
                return s * dt;
            }
        }

        /// <summary>DEATH: back to the room's start, the room's script from its first wave, the same stream.</summary>
        void Retry()
        {
            RoomDef r = level.rooms[Math.Max(0, room)];
            retries++;
            world.KillAllEnemies();
            if (r.kind == "boss" && phase == Phase.Fight && bossOn) bossOn = world.BeginBoss(r.boss, out bossSpawn, out bossYaw); // the boss starts over (a live boss goes back to its phase start: the sim's BeginBoss)
            if (r.kind == "boss" && bossOn) world.Respawn(bossSpawn, bossYaw);
            else world.Respawn(r.spawn, r.spawnYawDeg);
            world.Revive();
            if (phase == Phase.Fight)
            {
                lessonIdle43 = 0;
                roomSteps = 0;
                wave = retryWave46 != null ? retryWave46(r, wave) : 0; // NIGHT46 F1 (retry46): the wave to restart at (null: wave 1, NIGHT45)
                gapLeft = 0;
                SeedRoom(Math.Max(0, room));
            }
            if (room < 0) { room = 0; phase = Phase.Fight; checkpoint = 0; }
            events.Emit("roomRetry", new RoomEvent { room = room, id = r.id, name = r.name, limits = limits });
        }
    }
}
