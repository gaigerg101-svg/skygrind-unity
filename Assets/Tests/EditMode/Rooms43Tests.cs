// NIGHT43 E1 (design.n43.rooms43; NIGHT43.md E1): the room fields afterLesson43 / fallReturn43 and the door rules in Core/Campaign24.cs, on a fake
// world (no engine). With rooms43 off every rule below is NIGHT42's (asserted beside each rule).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Rooms43Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        const double Dt = 1.0 / 120;

        sealed class World : ICampaignWorld, ILessonSource
        {
            public Vec3 feet = new Vec3(0, 0, 14);
            public bool grounded = true;
            public int alive, spawned;
            public double damage;
            public int respawns;
            public Vec3 respawnAt;
            public readonly List<string> log = new List<string>();
            public Vec3 PlayerFeet => feet;
            public bool PlayerDead => false;
            public string ZoneAt(Vec3 p) => p.z >= -20 ? "r1" : p.z >= -26 ? "d1" : "r2";
            public int others, opens;
            public int EnemiesAlive => alive + others;
            public int RoomAlive43(int room) => alive;
            public bool Spawn(string kind, Vec3 at) { alive++; spawned++; return true; }
            public void KillAllEnemies() { alive = 0; }
            public void LockZone(string id) { log.Add("lock " + id); }
            public void OpenZone(string id) { log.Add("open " + id); if (id == "d1") opens++; }
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

        const string RoomJson = @"{ ""id"": ""t"", ""name"": ""T"", ""campaign"": { ""rooms"": [
              { ""id"": ""r1"", ""name"": ""ONE"", ""kind"": ""room"", ""par"": 20, ""rect"": [-10, -20, 10, 20], ""spawn"": { ""at"": [0,0,14], ""yawDeg"": 0 }, ""exit"": [0,0,-17],
                ""nodes"": [[-8,0,-10],[8,0,-10]], ""waves"": [[""husk"",""husk""]],
                ""lessons"": [ { ""kind"": ""reached"", ""rect"": [-4, -12, 4, -8], ""prompt"": ""GO"" } ],
                ""afterLesson43"": AFTER,
                ""fallReturn43"": { ""belowY"": -2, ""to"": [0, 1, 10], ""damage"": 10 } },
              { ""id"": ""r2"", ""name"": ""TWO"", ""kind"": ""room"", ""par"": 30, ""spawn"": { ""at"": [0,0,-30], ""yawDeg"": 0 }, ""exit"": [0,0,-63],
                ""nodes"": [[-8,0,-40]], ""waves"": [[""husk""]] } ],
              ""doors"": [ { ""id"": ""d1"", ""between"": [""r1"", ""r2""] } ] } }";

        static Campaign24 Make(World w, bool rooms43, string after = "0")
        {
            LevelDef level = LevelDef.From(PortData.Obj(JsJson.Parse(RoomJson.Replace("AFTER", after))));
            var c = new Campaign24(Cfg, new EventBus(), level, w, Dt) { rooms43 = rooms43 };
            c.Start(7);
            return c;
        }

        static void Steps(Campaign24 c, int n)
        {
            for (int i = 0; i < n; i++) c.Step();
        }

        [Test]
        public void Parse_ReadsTheTwoRoomFields()
        {
            LevelDef level = LevelDef.From(PortData.Obj(JsJson.Parse(RoomJson.Replace("AFTER", "[0]"))));
            RoomDef r = level.rooms[0];
            Assert.That(r.AfterLesson43(0), Is.EqualTo(0));
            Assert.That(r.AfterLesson43(1), Is.EqualTo(-1), "a wave past the array waits for nothing");
            Assert.That(r.hasFallReturn43, Is.True);
            Assert.That(r.fallBelowY43, Is.EqualTo(-2));
            Assert.That(r.fallDamage43, Is.EqualTo(10));
            Assert.That(r.fallTo43.z, Is.EqualTo(10));
            Assert.That(level.rooms[1].afterLesson43, Is.Null);
            Assert.That(level.rooms[1].hasFallReturn43, Is.False);
            LevelDef num = LevelDef.From(PortData.Obj(JsJson.Parse(RoomJson.Replace("AFTER", "0"))));
            Assert.That(num.rooms[0].AfterLesson43(0), Is.EqualTo(0), "a plain number: wave 0 waits for that lesson");
        }

        [Test]
        public void AfterLesson43_TheWaveWaitsForItsLesson_OnlyWithTheRowOn()
        {
            var off = new World();
            Campaign24 c0 = Make(off, false);
            Steps(c0, 240);
            Assert.That(off.spawned, Is.EqualTo(2), "rooms43 off: NIGHT42's room spawns its wave at once");

            var w = new World();
            Campaign24 c = Make(w, true);
            Steps(c, 600);
            Assert.That(c.room, Is.EqualTo(0));
            Assert.That(w.spawned, Is.EqualTo(0), "the wave waits while lesson 0 is not done");
            w.feet = new Vec3(0, 0, -10); // into the lesson's rect, grounded
            Steps(c, 120);
            Assert.That(c.lesson == null || c.lesson.Done, Is.True);
            Assert.That(w.spawned, Is.EqualTo(2), "the lesson done: the wave spawns");
        }

        [Test]
        public void Door_CountsOnlyTheRoomsOwn_AndAVerbLessonIsCapped()
        {
            double capS = Cfg.Num("design.n43.rooms43.lessonCapS");
            var off = new World { others = 3 };
            Campaign24 c0 = Make(off, false, "-1");
            Steps(c0, 60);
            Assert.That(c0.RoomLeft43(), Is.EqualTo(-1), "rooms43 off: no counter");
            off.alive = 0;
            Steps(c0, 120 * 60);
            Assert.That(off.opens, Is.EqualTo(0), "rooms43 off: the level's other bodies hold the door (NIGHT42)");

            var w = new World { others = 3 };
            var bus = new EventBus();
            int clears = 0;
            bus.On("roomClear43", _ => clears++);
            LevelDef level = LevelDef.From(PortData.Obj(JsJson.Parse(RoomJson.Replace("AFTER", "-1"))));
            var c = new Campaign24(Cfg, bus, level, w, Dt) { rooms43 = true };
            c.Start(7);
            Steps(c, 30);
            w.feet = new Vec3(0, 0, 10); // 4 m into the room: the first wave's commit gate (round 2)
            Steps(c, 30);
            Assert.That(w.spawned, Is.EqualTo(2), "committed: the wave came");
            Assert.That(c.RoomLeft43(), Is.EqualTo(2), "the counter: the room's two husks");
            w.alive = 0;
            int t = 0;
            while (w.opens == 0 && t < 120 * 60) { c.Step(); t++; }
            Assert.That(w.opens, Is.EqualTo(1), "the room's own are dead: the door opens");
            Assert.That(t * Dt, Is.EqualTo(capS).Within(0.1), "after lessonCapS (the verb lesson 'reached' was never done)");
            Assert.That(c.lessonCaps43, Is.EqualTo(1));
            Assert.That(clears, Is.EqualTo(1), "roomClear43");
        }

        [Test]
        public void Commit43_TheFirstWaveWaitsForThePlayerToComeIn()
        {
            double maxS = Cfg.Num("design.n43.rooms43.commitMaxS");
            var stay = new World();
            Campaign24 c = Make(stay, true, "-1");
            int t = 0;
            while (stay.spawned == 0 && t < 1200) { c.Step(); t++; }
            Assert.That(t * Dt, Is.EqualTo(maxS).Within(0.05), "standing at the spawn: the wave after commitMaxS");
            var walk = new World();
            Campaign24 c2 = Make(walk, true, "-1");
            Steps(c2, 12);
            walk.feet = new Vec3(0, 0, 10);
            Steps(c2, 2);
            Assert.That(walk.spawned, Is.EqualTo(2), "4 m in: the wave at once");
            var off = new World();
            Campaign24 c3 = Make(off, false, "-1");
            Steps(c3, 3);
            Assert.That(off.spawned, Is.EqualTo(2), "rooms43 off: the wave at the entry");
        }

        [Test]
        public void FallReturn43_BackToThePoint_WithTheDamage_NeverOff()
        {
            var off = new World();
            Campaign24 c0 = Make(off, false);
            Steps(c0, 10);
            off.feet = new Vec3(0, -3, 0);
            Steps(c0, 10);
            Assert.That(off.respawns, Is.EqualTo(0), "rooms43 off: no fallReturn43");

            var w = new World();
            var bus = new EventBus();
            LevelDef level = LevelDef.From(PortData.Obj(JsJson.Parse(RoomJson.Replace("AFTER", "-1"))));
            var c = new Campaign24(Cfg, bus, level, w, Dt) { rooms43 = true };
            c.Start(7);
            int evs = 0;
            bus.On<FallReturn43Event>("fallReturn43", e => { evs++; Assert.That(e.damage, Is.EqualTo(10)); });
            Steps(c, 10);
            w.feet = new Vec3(0, -1.5, 0);
            Steps(c, 5);
            Assert.That(w.respawns, Is.EqualTo(0), "above belowY: nothing");
            w.feet = new Vec3(0, -2.5, 0);
            Steps(c, 1);
            Assert.That(w.respawns, Is.EqualTo(1));
            Assert.That(w.respawnAt.z, Is.EqualTo(10));
            Assert.That(w.damage, Is.EqualTo(10));
            Assert.That(evs, Is.EqualTo(1));
            Assert.That(c.fallReturns43, Is.EqualTo(1));
            w.feet = new Vec3(30, -5, 0); // outside the room's rect
            Steps(c, 2);
            Assert.That(w.respawns, Is.EqualTo(1), "outside the room's rect: not this room's fall");
        }
    }
}
