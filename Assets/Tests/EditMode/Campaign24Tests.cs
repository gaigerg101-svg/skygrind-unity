// NIGHT24 (builder L): THE CAMPAIGN's rules (Core/Campaign24.cs) and the door lock (Zones.Lock). No UnityEngine: SimHarness runs the
// same assertions (`campaign --selftest`). The rules run on a fake world; the lock runs on the web's survival map.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Campaign24Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        const double Dt = 1.0 / 120;

        // --- Zones.Lock -----------------------------------------------------------------------------------------------

        static Zones MakeZones(out EventBus events)
        {
            var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            events = new EventBus();
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(Cfg.movement));
            return new Zones(Cfg, map, world, events);
        }

        [Test]
        public void Lock_RelocksAnOpenedZone_AndTheGateComesBack()
        {
            Zones z = MakeZones(out EventBus events);
            int locks = 0, opens = 0;
            events.On("gateLock", _ => locks++);
            events.On("gateOpen", _ => opens++);
            Assert.That(z.IsOpen("armory"), Is.False);
            Assert.That(z.Lock("armory"), Is.False, "already locked: nothing to do");
            Assert.That(z.Open("armory", free: true), Is.True);
            int quadsOpen = z.GateQuads().Count;
            int version = z.Version;
            Assert.That(z.IsOpen("armory"), Is.True);
            Assert.That(z.Lock("armory"), Is.True);
            Assert.That(z.IsOpen("armory"), Is.False);
            Assert.That(z.Version > version, Is.True, "the view rebuilds its gate mesh");
            Assert.That(z.GateQuads().Count > 0, Is.True);
            Assert.That(locks, Is.EqualTo(1));
            Assert.That(opens, Is.EqualTo(1));
            Assert.That(z.Lock("armory"), Is.False, "a second lock is a no-op");
            Assert.That(z.Lock("no such zone"), Is.False);
            Assert.That(z.Open("armory", free: true), Is.True, "and it opens again");
        }

        [Test]
        public void Lock_SurvivesASaveAndLoad()
        {
            Zones a = MakeZones(out _);
            a.Open("armory", free: true);
            a.Open("slabs", free: true);
            a.Lock("armory");
            var w = new StateWriter();
            a.Save(w);
            Zones b = MakeZones(out _);
            b.Open("armory", free: true); // a different state than the save's
            b.Load(new StateReader(w.ToJson()));
            Assert.That(b.IsOpen("armory"), Is.False, "locked again as saved");
            Assert.That(b.IsOpen("slabs"), Is.EqualTo(a.IsOpen("slabs")));
            Assert.That(b.GateQuads().Count, Is.EqualTo(a.GateQuads().Count), "the force field is back in the collider");
        }

        // --- the rules ------------------------------------------------------------------------------------------------

        sealed class FakeWorld : ICampaignWorld
        {
            public Vec3 feet = new Vec3(0, 0, 0);
            public bool dead;
            public int alive;
            public readonly List<string> log = new List<string>();
            public readonly List<Vec3> spawns = new List<Vec3>();
            public readonly HashSet<string> open = new HashSet<string> { "r1" };
            public int limits;
            public readonly List<string> awarded = new List<string>();
            public Vec3 PlayerFeet => feet;
            public bool PlayerDead => dead;
            // rooms are strips along z: r1 z -20..20, d1 -26..-20, r2 -66..-26, d2 -72..-66, r3 -112..-72
            public string ZoneAt(Vec3 p) => p.z >= -20 ? "r1" : p.z >= -26 ? "d1" : p.z >= -66 ? "r2" : p.z >= -72 ? "d2" : "r3";
            public int EnemiesAlive => alive;
            public bool Spawn(string kind, Vec3 at) { alive++; spawns.Add(at); return true; }
            public void KillAllEnemies() { alive = 0; log.Add("killAll"); }
            public void LockZone(string id) { open.Remove(id); log.Add("lock " + id); }
            public void OpenZone(string id) { open.Add(id); log.Add("open " + id); }
            public void Respawn(Vec3 at, double yawDeg) { feet = at; log.Add("respawn"); }
            public void Revive() { dead = false; log.Add("revive"); }
            public void SetLimits(int flags) { limits = flags; log.Add("limits " + flags); }
            public void AwardWeapon(string weapon) { awarded.Add(weapon); log.Add("award " + weapon); }
            public bool bossAvailable = true, bossDead;
            public int bossBegins;
            public bool BeginBoss(out Vec3 spawn, out double yawDeg) { spawn = new Vec3(400, 0, 0); yawDeg = 90; if (!bossAvailable) return false; bossBegins++; bossDead = false; log.Add("bossBegin"); return true; }
            public bool BossDefeated => bossDead;
        }

        static LevelDef Level()
        {
            var json = PortData.Obj(JsJson.Parse(@"{ ""id"": ""t"", ""name"": ""T"", ""campaign"": { ""rooms"": [
              { ""id"": ""r1"", ""name"": ""ONE"", ""kind"": ""room"", ""par"": 20, ""spawn"": { ""at"": [0,0,14], ""yawDeg"": 0 }, ""exit"": [0,0,-17],
                ""nodes"": [[-14,0,-10],[14,0,-10],[-14,0,8],[14,0,8]], ""waves"": [[""husk"",""husk""],[""husk""]] },
              { ""id"": ""r2"", ""name"": ""TWO"", ""kind"": ""room"", ""par"": 30, ""spawn"": { ""at"": [0,0,-30], ""yawDeg"": 0 }, ""exit"": [0,0,-63],
                ""nodes"": [[-14,0,-40],[14,0,-40],[-14,0,-56]], ""waves"": [[""husk"",""husk"",""husk""]], ""limit"": [""pistolOnly"",""noDash""], ""award"": ""shotgun"" },
              { ""id"": ""r3"", ""name"": ""THREE"", ""kind"": ""room"", ""par"": 30, ""spawn"": { ""at"": [0,0,-76], ""yawDeg"": 0 }, ""exit"": [0,0,-109],
                ""nodes"": [[-14,0,-90]], ""waves"": [[""husk""]] } ],
              ""doors"": [ { ""id"": ""d1"", ""between"": [""r1"", ""r2""] }, { ""id"": ""d2"", ""between"": [""r2"", ""r3""] } ] } }"));
            return LevelDef.From(json);
        }

        static Campaign24 Make(FakeWorld w, EventBus events, uint seed = 7)
        {
            var c = new Campaign24(Cfg, events, Level(), w, Dt);
            c.Start(seed);
            return c;
        }

        static void Steps(Campaign24 c, int n)
        {
            for (int i = 0; i < n; i++) c.Step();
        }

        [Test]
        public void TheLevel_ReadsRoomsDoorsAndLimits()
        {
            LevelDef l = Level();
            Assert.That(l.rooms.Count, Is.EqualTo(3));
            Assert.That(l.doors.Count, Is.EqualTo(2));
            Assert.That(l.rooms[0].EnemyCount, Is.EqualTo(3));
            Assert.That(l.rooms[1].limits, Is.EqualTo(Limit24.PistolOnly | Limit24.NoDash));
            Assert.That(Limit24.Banner(l.rooms[1].limits), Is.EqualTo("PISTOL ONLY / NO DASH"));
            Assert.That(l.rooms[1].award, Is.EqualTo("shotgun"));
        }

        [Test]
        public void ARoom_SpawnsWaveByWave_OnlyAtItsOwnNodes_AndOpensTheDoorWhenCleared()
        {
            var w = new FakeWorld { feet = new Vec3(0, 0, 14) };
            var events = new EventBus();
            var cleared = new List<RoomEvent>();
            events.On<RoomEvent>("roomClear", e => cleared.Add(e));
            Campaign24 c = Make(w, events);
            c.Step();
            Assert.That(c.room, Is.EqualTo(0));
            Assert.That(w.alive, Is.EqualTo(2), "wave 1 of room 1");
            foreach (Vec3 s in w.spawns) Assert.That(Level().rooms[0].nodes.Exists(n => n.x == s.x && n.z == s.z), Is.True, "only room-local nodes");
            Steps(c, 400);
            Assert.That(w.alive, Is.EqualTo(2), "wave 2 waits for wave 1 to die");
            w.alive = 0;
            Steps(c, 2);
            Assert.That(w.alive, Is.EqualTo(0), "the wave gap is a breath");
            Steps(c, MovementMath.StepsFor(Cfg.Num("design.n24.campaign.waveGap"), Dt) + 2);
            Assert.That(w.alive, Is.EqualTo(1), "wave 2");
            Assert.That(w.open.Contains("d1"), Is.False, "the exit stays shut while enemies live");
            w.alive = 0;
            Steps(c, 2);
            Assert.That(c.phase, Is.EqualTo(Campaign24.Phase.Cleared));
            Assert.That(cleared.Count, Is.EqualTo(1));
            Assert.That(w.open.Contains("d1") && w.open.Contains("r2"), Is.True, "the exit door and the next room open free");
        }

        [Test]
        public void EnteringTheNextRoom_LocksTheDoorBehind_AndSetsTheCheckpoint()
        {
            var w = new FakeWorld { feet = new Vec3(0, 0, 14) };
            var events = new EventBus();
            int checkpoints = 0;
            events.On<RoomEvent>("checkpoint", _ => checkpoints++);
            Campaign24 c = Make(w, events);
            c.Step();
            w.alive = 0;
            Steps(c, MovementMath.StepsFor(2, Dt));
            w.alive = 0;
            Steps(c, 200);
            w.alive = 0;
            Steps(c, 4);
            Assert.That(c.phase, Is.EqualTo(Campaign24.Phase.Cleared));
            w.feet = new Vec3(0, 0, -23); // inside the door: not yet in the room
            c.Step();
            Assert.That(c.room, Is.EqualTo(0));
            w.feet = new Vec3(0, 0, -30);
            c.Step();
            Assert.That(c.room, Is.EqualTo(1));
            Assert.That(w.log.Contains("lock d1"), Is.True);
            Assert.That(c.checkpoint, Is.EqualTo(1));
            Assert.That(checkpoints, Is.EqualTo(1));
            Assert.That(c.State.room, Is.EqualTo(1));
            Assert.That(c.State.limits, Is.EqualTo(Limit24.PistolOnly | Limit24.NoDash), "the limits are in the tape's state");
        }

        [Test]
        public void ALimitRoom_StripsOnEntry_RestoresAtTheDoor_PaysOnce_AndAwardsTheWeapon()
        {
            var w = new FakeWorld { feet = new Vec3(0, 0, -30) };
            var events = new EventBus();
            var bonus = new List<StyleBonusEvent>();
            var earned = new List<WeaponEarnedEvent>();
            events.On<StyleBonusEvent>("styleBonus", e => bonus.Add(e));
            events.On<WeaponEarnedEvent>("weaponEarned", e => earned.Add(e));
            Campaign24 c = Make(w, events);
            // jump straight to room 2 as a checkpoint resume would: enter by position
            c.room = 0; c.phase = Campaign24.Phase.Cleared;
            c.Step();
            Assert.That(c.room, Is.EqualTo(1));
            Assert.That(w.limits, Is.EqualTo(Limit24.PistolOnly | Limit24.NoDash), "stripped on entry");
            Assert.That(w.alive, Is.EqualTo(3));
            w.alive = 0;
            Steps(c, 2);
            Assert.That(c.phase, Is.EqualTo(Campaign24.Phase.Cleared));
            Assert.That(w.limits, Is.EqualTo(0), "restored at the door");
            Assert.That(bonus.Count, Is.EqualTo(1));
            Assert.That(bonus[0].points, Is.EqualTo(Cfg.Num("design.n24.campaign.limitBonus")));
            Assert.That(earned.Count, Is.EqualTo(1));
            Assert.That(earned[0].name, Is.EqualTo("THUNDERCLAP"));
            Assert.That(w.awarded, Is.EqualTo(new List<string> { "shotgun" }));
            Assert.That(w.log.IndexOf("limits 0") < w.log.IndexOf("award shotgun"), Is.True, "the kit is whole again before the new gun joins it");
        }

        [Test]
        public void ADeath_RestartsTheRoomWithinAnAndAHalfSeconds_WithTheSameSeed()
        {
            var w = new FakeWorld { feet = new Vec3(0, 0, 14) };
            var events = new EventBus();
            Campaign24 c = Make(w, events, seed: 123);
            c.Step();
            var first = new List<Vec3>(w.spawns);
            w.alive = 0;
            w.spawns.Clear();
            w.dead = true;
            int steps = 0;
            while (w.dead && steps < 1000)
            {
                c.Step();
                steps++;
            }
            Assert.That(steps * Dt <= 1.5, Is.True, "within 1.5 s of game time");
            Assert.That(steps, Is.EqualTo(MovementMath.StepsFor(Cfg.Num("design.n24.campaign.retryDelay"), Dt)));
            Assert.That(w.log.Contains("respawn"), Is.True);
            Assert.That(c.retries, Is.EqualTo(1));
            c.Step();
            Assert.That(w.spawns.Count, Is.EqualTo(first.Count), "the same first wave again");
            for (int i = 0; i < first.Count; i++) Assert.That((w.spawns[i].x, w.spawns[i].z), Is.EqualTo((first[i].x, first[i].z)), "the same nodes: the same seed");
        }

        [Test]
        public void ALink_ClearsWhenThePlayerReachesItsExit_NotOnEntry()
        {
            var json = PortData.Obj(JsJson.Parse(@"{ ""id"": ""t"", ""name"": ""T"", ""campaign"": { ""rooms"": [
              { ""id"": ""r1"", ""kind"": ""link"", ""par"": 10, ""spawn"": { ""at"": [0,0,14] }, ""exit"": [0,0,-17] },
              { ""id"": ""r2"", ""kind"": ""room"", ""par"": 10, ""spawn"": { ""at"": [0,0,-30] }, ""exit"": [0,0,-63] } ],
              ""doors"": [ { ""id"": ""d1"", ""between"": [""r1"", ""r2""] } ] } }"));
            var w = new FakeWorld { feet = new Vec3(0, 0, 14) };
            var c = new Campaign24(Cfg, new EventBus(), LevelDef.From(json), w, Dt);
            c.Start(1);
            Steps(c, 50);
            Assert.That(c.phase, Is.EqualTo(Campaign24.Phase.Fight), "entering a link is not clearing it");
            w.feet = new Vec3(0, 0, -16);
            c.Step();
            Assert.That(c.phase, Is.EqualTo(Campaign24.Phase.Cleared));
            Assert.That(c.clearSteps[0] >= 50, Is.True, "its time is the crossing");
            Assert.That(w.open.Contains("d1"), Is.True);
        }

        static Campaign24 BossLevel(FakeWorld w, EventBus events)
        {
            var json = PortData.Obj(JsJson.Parse(@"{ ""id"": ""t"", ""name"": ""T"", ""campaign"": { ""rooms"": [
              { ""id"": ""r1"", ""kind"": ""link"", ""par"": 10, ""spawn"": { ""at"": [0,0,14] }, ""exit"": [0,0,-17] },
              { ""id"": ""r2"", ""kind"": ""boss"", ""par"": 100, ""spawn"": { ""at"": [0,0,-30] }, ""exit"": [0,0,-63] } ],
              ""doors"": [ { ""id"": ""d1"", ""between"": [""r1"", ""r2""] } ] } }"));
            var c = new Campaign24(Cfg, events, LevelDef.From(json), w, Dt);
            c.Start(1);
            c.room = 0;
            c.phase = Campaign24.Phase.Cleared;
            return c;
        }

        [Test]
        public void TheBossRoom_TakesThePlayerToTheFight_ClearsWhenTheBossIsDead_AndEndsTheLayer()
        {
            var w = new FakeWorld { feet = new Vec3(0, 0, -30) };
            Campaign24 c = BossLevel(w, new EventBus());
            c.Step();
            Assert.That(c.room, Is.EqualTo(1));
            Assert.That(w.log.Contains("bossBegin"), Is.True);
            Assert.That(w.feet.x, Is.EqualTo(400.0), "taken to the worm's room");
            Steps(c, 200);
            Assert.That(c.finished, Is.False, "the boss is alive");
            w.bossDead = true;
            c.Step();
            Assert.That(c.finished, Is.True);
        }

        [Test]
        public void ADeathInTheBossRoom_StartsTheWormOver()
        {
            var w = new FakeWorld { feet = new Vec3(0, 0, -30) };
            Campaign24 c = BossLevel(w, new EventBus());
            c.Step();
            w.dead = true;
            Steps(c, MovementMath.StepsFor(Cfg.Num("design.n24.campaign.retryDelay"), Dt));
            Assert.That(w.bossBegins, Is.EqualTo(2), "begun again");
            Assert.That(w.feet.x, Is.EqualTo(400.0));
            Assert.That(c.finished, Is.False);
        }

        [Test]
        public void NoBossAvailable_TheBossRoomClearsAtOnce()
        {
            var w = new FakeWorld { feet = new Vec3(0, 0, -30), bossAvailable = false };
            Campaign24 c = BossLevel(w, new EventBus());
            Steps(c, 3);
            Assert.That(c.finished, Is.True);
        }

        [Test]
        public void TheLastRoomCleared_FinishesTheLayer()
        {
            var w = new FakeWorld { feet = new Vec3(0, 0, -76) };
            var events = new EventBus();
            int layer = 0;
            events.On<RoomEvent>("layerClear", _ => layer++);
            Campaign24 c = Make(w, events);
            c.room = 1; c.phase = Campaign24.Phase.Cleared;
            c.Step();
            Assert.That(c.room, Is.EqualTo(2));
            w.alive = 0;
            Steps(c, 2);
            Assert.That(c.finished, Is.True);
            Assert.That(layer, Is.EqualTo(1));
        }
    }
}
