// NIGHT43 builder E1 (rooms43, iceline43; NIGHT43.md E1, GG G4 / G5 / G22 / G23): the encounter checks, declared and called by the lead's
// Program.cs (N43Rooms). Every number is measured on the real sim (a campaign run of the level) or on Core/Campaign24.cs with a fake world.
//   n43rooms selftest
//       Rooms43Tests' assertions without the Editor (Assets/Tests/EditMode/Rooms43Tests.cs): afterLesson43 holds a wave until its lesson,
//       fallReturn43 returns the player with the damage, both only with rooms43 on. Exit 0 when every check passes.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43Rooms(string cmd, string[] args, ref int code)
    {
        if (cmd != "n43rooms") return;
        string what = args.Length > 1 ? args[1] : "";
        if (what == "selftest") { code = N43RmSelfTest(); return; }
        N43RoomsMore(what, args, ref code);
        if (code != -1) return;
        Console.WriteLine("n43rooms: selftest | clear [--off] [--levels 1,2,3] [--seeds n] | still [--off] [--levels 1,2,3] [--seeds n] | iceline");
        code = 2;
    }

    /// <summary>The later slices' commands (clear, still, iceline) in their own partials.</summary>
    static partial void N43RoomsMore(string what, string[] args, ref int code);

    sealed class N43FakeWorld : ICampaignWorld, ILessonSource
    {
        public Vec3 feet = new Vec3(0, 0, 14);
        public bool grounded = true;
        public int alive, spawned, respawns, others, opens;
        public double damage;
        public Vec3 respawnAt;
        public Vec3 PlayerFeet => feet;
        public bool PlayerDead => false;
        public string ZoneAt(Vec3 p) => p.z >= -20 ? "r1" : p.z >= -26 ? "d1" : "r2";
        public int EnemiesAlive => alive + others; // `others`: bodies elsewhere in the level (another room's, a pit's)
        public int RoomAlive43(int room) => alive;
        public bool Spawn(string kind, Vec3 at) { alive++; spawned++; return true; }
        public void KillAllEnemies() { alive = 0; }
        public void LockZone(string id) { }
        public void OpenZone(string id) { if (id == "d1") opens++; }
        public void Respawn(Vec3 at, double yawDeg) { feet = at; respawns++; respawnAt = at; }
        public void Revive() { }
        public void SetLimits(int flags) { }
        public void AwardWeapon(string weapon) { }
        public bool BeginBoss(out Vec3 spawn, out double yawDeg) { spawn = default; yawDeg = 0; return false; }
        public bool BossDefeated => false;
        public void FallDamage43(double hp) { damage += hp; }
        public LessonSample Sample(int step) => new LessonSample { step = step, feet = feet, grounded = grounded };
        public bool LessonAvailable(LessonKind kind) => true;
    }

    const string N43RoomJson = @"{ ""id"": ""t"", ""name"": ""T"", ""campaign"": { ""rooms"": [
          { ""id"": ""r1"", ""name"": ""ONE"", ""kind"": ""room"", ""par"": 20, ""rect"": [-10, -20, 10, 20], ""spawn"": { ""at"": [0,0,14], ""yawDeg"": 0 }, ""exit"": [0,0,-17],
            ""nodes"": [[-8,0,-10],[8,0,-10]], ""waves"": [[""husk"",""husk""]],
            ""lessons"": [ { ""kind"": ""reached"", ""rect"": [-4, -12, 4, -8], ""prompt"": ""GO"" } ],
            ""afterLesson43"": AFTER,
            ""fallReturn43"": { ""belowY"": -2, ""to"": [0, 1, 10], ""damage"": 10 } },
          { ""id"": ""r2"", ""name"": ""TWO"", ""kind"": ""room"", ""par"": 30, ""spawn"": { ""at"": [0,0,-30], ""yawDeg"": 0 }, ""exit"": [0,0,-63],
            ""nodes"": [[-8,0,-40]], ""waves"": [[""husk""]] } ],
          ""doors"": [ { ""id"": ""d1"", ""between"": [""r1"", ""r2""] } ] } }";

    static Campaign24 N43RmMake(GameConfig cfg, N43FakeWorld w, bool on, string after, EventBus bus = null)
    {
        LevelDef level = LevelDef.From(PortData.Obj(JsJson.Parse(N43RoomJson.Replace("AFTER", after))));
        var c = new Campaign24(cfg, bus ?? new EventBus(), level, w, 1.0 / 120) { rooms43 = on };
        c.Start(7);
        return c;
    }

    static int N43RmSelfTest()
    {
        GameConfig cfg = Config();
        int fails = 0;
        void Check(bool ok, string what) { Console.WriteLine($"  {(ok ? "ok  " : "FAIL")} {what}"); if (!ok) fails++; }
        void Steps(Campaign24 c, int n) { for (int i = 0; i < n; i++) c.Step(); }

        LevelDef lv = LevelDef.From(PortData.Obj(JsJson.Parse(N43RoomJson.Replace("AFTER", "[0]"))));
        Check(lv.rooms[0].AfterLesson43(0) == 0 && lv.rooms[0].AfterLesson43(1) == -1, "parse: afterLesson43 [0] -> wave 0 waits for lesson 0, wave 1 for none");
        Check(lv.rooms[0].hasFallReturn43 && lv.rooms[0].fallBelowY43 == -2 && lv.rooms[0].fallDamage43 == 10 && lv.rooms[0].fallTo43.z == 10, "parse: fallReturn43 belowY -2, damage 10, to z 10");
        Check(lv.rooms[1].afterLesson43 == null && !lv.rooms[1].hasFallReturn43, "parse: a room without the fields has neither");

        var off = new N43FakeWorld();
        Campaign24 c0 = N43RmMake(cfg, off, false, "0");
        Steps(c0, 240);
        Check(off.spawned == 2, $"afterLesson43, rooms43 off: the wave spawns at once (spawned {off.spawned})");
        var w = new N43FakeWorld();
        Campaign24 c1 = N43RmMake(cfg, w, true, "0");
        Steps(c1, 600);
        Check(c1.room == 0 && w.spawned == 0, $"afterLesson43 on: 5 s in the room, lesson 0 not done: no spawn (spawned {w.spawned})");
        w.feet = new Vec3(0, 0, -10);
        Steps(c1, 120);
        Check(w.spawned == 2, $"afterLesson43 on: the lesson done -> the wave spawns (spawned {w.spawned})");

        var off2 = new N43FakeWorld();
        Campaign24 c2 = N43RmMake(cfg, off2, false, "-1");
        Steps(c2, 10);
        off2.feet = new Vec3(0, -3, 0);
        Steps(c2, 10);
        Check(off2.respawns == 0, "fallReturn43, rooms43 off: nothing");
        var w3 = new N43FakeWorld();
        var bus = new EventBus();
        int evs = 0;
        bus.On<FallReturn43Event>("fallReturn43", e => evs++);
        Campaign24 c3 = N43RmMake(cfg, w3, true, "-1", bus);
        Steps(c3, 10);
        w3.feet = new Vec3(0, -1.5, 0);
        Steps(c3, 5);
        Check(w3.respawns == 0, "fallReturn43 on: feet above belowY: nothing");
        w3.feet = new Vec3(0, -2.5, 0);
        Steps(c3, 1);
        Check(w3.respawns == 1 && w3.respawnAt.z == 10 && w3.damage == 10 && evs == 1 && c3.fallReturns43 == 1, $"fallReturn43 on: below -> back at z {w3.respawnAt.z}, damage {w3.damage}, events {evs}");
        w3.feet = new Vec3(30, -5, 0);
        Steps(c3, 2);
        Check(w3.respawns == 1, "fallReturn43 on: outside the room's rect: not this room's fall");

        // round 2: the first wave waits for the player to come in (commitM) or commitMaxS
        {
            double maxS = cfg.Num("design.n43.rooms43.commitMaxS");
            var wc = new N43FakeWorld();
            Campaign24 cc = N43RmMake(cfg, wc, true, "-1");
            int t = 0;
            while (wc.spawned == 0 && t < 1200) { cc.Step(); t++; }
            Check(Math.Abs(t / 120.0 - maxS) < 0.05, $"commit: the player stays at the spawn: the first wave at {t / 120.0:0.00} s (commitMaxS {maxS})");
            var wi = new N43FakeWorld();
            Campaign24 ci = N43RmMake(cfg, wi, true, "-1");
            Steps(ci, 12);
            wi.feet = new Vec3(0, 0, 10);
            t = 12;
            while (wi.spawned == 0 && t < 1200) { ci.Step(); t++; }
            Check(t / 120.0 < 0.5, $"commit: the player steps 4 m in at 0.1 s: the first wave at {t / 120.0:0.00} s");
            var wo = new N43FakeWorld();
            Campaign24 co = N43RmMake(cfg, wo, false, "-1");
            Steps(co, 3);
            Check(wo.spawned == 2, $"commit, rooms43 off: the wave at the entry (spawned {wo.spawned} after 3 steps)");
        }

        // the door: only the room's own bodies, and a verb lesson capped (lessonCapS) once they are dead
        double capS = cfg.Num("design.n43.rooms43.lessonCapS");
        foreach (bool on in new[] { false, true })
        {
            var wd = new N43FakeWorld { others = 3 };
            var bd = new EventBus();
            int clears43 = 0, left = -2;
            bd.On("roomClear43", _ => clears43++);
            Campaign24 cd = N43RmMake(cfg, wd, on, "-1", bd);
            Steps(cd, 30);
            wd.feet = new Vec3(0, 0, 10); // 4 m into the room: committed (round 2's gate)
            Steps(cd, 30);
            left = cd.RoomLeft43();
            wd.alive = 0; // the room's two husks die; three bodies elsewhere in the level stay
            int t = 0;
            while (wd.opens == 0 && t < MovementMath.StepsFor(60, 1.0 / 120)) { cd.Step(); t++; }
            if (!on) Check(wd.opens == 0 && left == -1, $"door, rooms43 off: the level's 3 other bodies hold it ({t / 120.0:0.0} s, no open; RoomLeft43 {left})");
            else Check(wd.opens == 1 && Math.Abs(t / 120.0 - capS) < 0.1 && clears43 == 1 && cd.lessonCaps43 == 1 && left == 2, $"door, rooms43 on: opens {t / 120.0:0.00} s after the last own kill (lessonCapS {capS}), roomClear43 {clears43}, RoomLeft43 before the kills {left}");
        }

        Console.WriteLine($"n43rooms selftest: {(fails == 0 ? "PASS" : $"{fails} FAIL(S)")}");
        return fails == 0 ? 0 : 1;
    }
}
