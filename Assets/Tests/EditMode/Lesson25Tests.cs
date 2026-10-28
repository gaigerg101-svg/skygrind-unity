// NIGHT25 (builder L): the lesson rule (Core/Lesson25.cs) and the stage rules in Campaign24 (lessons gate the door, stage respawn, the top-up).
// No UnityEngine: SimHarness runs the same assertions (`campaign --selftest25`).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Lesson25Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        const double Dt = 1.0 / 120;

        static LessonDef D(string json) => LessonDef.From(PortData.Obj(JsJson.Parse(json)));

        static LessonSample S(int step, double x, double y, double z, bool grounded = true) => new LessonSample { step = step, feet = new Vec3(x, y, z), grounded = grounded };

        [Test]
        public void JumpedPit_ClearsOnlyStandingOnTheFarSide()
        {
            var l = new Lesson25(new List<LessonDef> { D(@"{""kind"":""jumpedPit"",""rect"":[-14,10,14,16],""minY"":-0.5}") }, 30);
            l.Begin(S(0, 0, 0, 30));
            Assert.That(l.Step(S(1, 0, 0, 30)), Is.False, "the near side is not the lesson");
            Assert.That(l.Step(S(2, 0, -3.5, 14)), Is.False, "in the pit (below minY)");
            Assert.That(l.Step(S(3, 0, 1, 14, grounded: false)), Is.False, "in the air over the far side");
            Assert.That(l.Step(S(4, 0, 0, 14)), Is.True);
            Assert.That(l.Done, Is.True);
        }

        [Test]
        public void SlidTunnel_NeedsTheSlideInsideThenTheFarSide()
        {
            var l = new Lesson25(new List<LessonDef> { D(@"{""kind"":""slidTunnel"",""via"":[-3,20,3,30],""rect"":[-14,10,14,16]}") }, 30);
            l.Begin(S(0, 0, 0, 40));
            Assert.That(l.Step(S(1, 0, 0, 14)), Is.False, "the far side without the slide (a way round) is not the lesson");
            var s = S(2, 0, 0, 25);
            s.sliding = true;
            Assert.That(l.Step(s), Is.False, "sliding inside is not yet the far side");
            Assert.That(l.Step(S(3, 0, 0, 14)), Is.True);
        }

        [Test]
        public void Counters_AreDifferences_AndCountLessonsCountFromTheStageStart()
        {
            var defs = new List<LessonDef> { D(@"{""kind"":""wallJumped"",""n"":2}"), D(@"{""kind"":""shots"",""n"":3}") };
            var l = new Lesson25(defs, 30);
            LessonSample b = S(0, 0, 0, 0);
            b.wallJumps = 5; b.fires = 10;
            l.Begin(b);
            b.fires = 12; b.step = 1;
            Assert.That(l.Step(b), Is.False);
            b.wallJumps = 7; b.step = 2;
            Assert.That(l.Step(b), Is.True, "two wall jumps since the stage began");
            b.fires = 13; b.step = 3;
            Assert.That(l.Step(b), Is.True, "three shots since the STAGE start (two were fired before the wall lesson cleared): no soft-lock");
            Assert.That(l.Done, Is.True);
        }

        [Test]
        public void PunchWhileFiring_NeedsBothInsideTheWindow()
        {
            var l = new Lesson25(new List<LessonDef> { D(@"{""kind"":""punchWhileFiring""}") }, 30);
            LessonSample s = S(0, 0, 0, 0);
            l.Begin(s);
            s.fires = 1; s.step = 10;
            Assert.That(l.Step(s), Is.False);
            s.punches = 1; s.step = 100; // 90 steps later: alternating, not together
            Assert.That(l.Step(s), Is.False);
            s.fires = 2; s.step = 110; // a shot 10 steps after that punch
            Assert.That(l.Step(s), Is.True);
        }

        [Test]
        public void UnavailableLessons_ArePassedOver()
        {
            var defs = new List<LessonDef> { D(@"{""kind"":""punchWhileFiring""}"), D(@"{""kind"":""shots"",""n"":1}") };
            var l = new Lesson25(defs, 30);
            l.Begin(S(0, 0, 0, 0), k => k != LessonKind.PunchWhileFiring);
            Assert.That(l.Current.kind, Is.EqualTo(LessonKind.Shots), "the block-off lesson is skipped at the start");
            var s = S(1, 0, 0, 0);
            s.fires = 1;
            Assert.That(l.Step(s, k => k != LessonKind.PunchWhileFiring), Is.True);
        }

        // --- the level order -------------------------------------------------------------------------------------------

        [Test]
        public void TheOrder_FollowsTheBlocks()
        {
            // NIGHT53 (T-126, n53-t126.cjs): the body forces older nights off; every static it sets is restored here, so no later test in the
            // domain reads the campaign with them forced off (NIGHT52's batch suite: N52SharedTests read the ON view rows as off)
            int wasFrom = NightRows.ForceOffFrom;
            bool w38 = N38Blocks.ForceOff, w40 = N40Blocks.ForceOff, w41 = N41Blocks.ForceOff, w42 = N42Blocks.ForceOff, w43 = N43Blocks.ForceOff, w45 = N45Levels.ForceOff;
            try { TheOrder_Body(); }
            finally
            {
                NightRows.ForceOffFrom = wasFrom;
                N38Blocks.ForceOff = w38; N40Blocks.ForceOff = w40; N41Blocks.ForceOff = w41; N42Blocks.ForceOff = w42; N43Blocks.ForceOff = w43; N45Levels.ForceOff = w45;
            }
        }

        static void TheOrder_Body()
        {
            GameConfig c = Cfg;
            N38Blocks.ForceOff = true; // NIGHT38: foundry38 / span38 / spire38 are ON in design (the n38 levels swap the campaign's ids); this test pins NIGHT37's and earlier orders, so NIGHT38 is forced off here (N38SharedTests pins its own swap; this namespace is not under N37TestBaseline)
            N45Levels.ForceOff = true; // NIGHT45 (L-027): the n45 level rows pin nothing older
            NightRows.ForceOffFrom = NightRows.FirstNight; // NIGHT46 rows (L-027): the NIGHT46+ level rows pin nothing older
            N43Blocks.ForceOff = true; // NIGHT43 (L-027): foundry43 / iceline43 / kiln43 swap their n43 levels in once ON; this test pins NIGHT39's and earlier orders (N43SharedTests pins its own swaps)
            N42Blocks.ForceOff = true; // NIGHT42 (L-027): foundry42 swaps n42-foundry in once ON; this test pins NIGHT39's and earlier orders (N42SharedTests pins its own swap)
            N41Blocks.ForceOff = true; // NIGHT41 (L-027): foundry41 / kiln41 / spire41 swap n41 ids in once ON; this test pins NIGHT39's and earlier orders, so NIGHT41 is forced off here (N41SharedTests pins its own swap)
            N40Blocks.ForceOff = true; // NIGHT40 (L-027): iceline40 / kiln40 / loom40 / crown40 swap n40 ids in and cut40 cuts level 6, all ON in design since the NIGHT40 freeze; this test pins NIGHT39's and earlier orders, so NIGHT40 is forced off here (N40SharedTests pins its own swap)
            bool was30 = N30Blocks.ForceOff;
            N30Blocks.ForceOff = true; // NIGHT30: design.n30.levels is ON by default (the n30 order); this test pins NIGHT29's order, so the NIGHT30 rows are forced off here
            try
            {
                Assert.That(Curriculum25.Order(c, N25Blocks.Off).Count == 1 && Curriculum25.Start(c, N25Blocks.Off) == "layer1", Is.True, "both blocks off: NIGHT24's layer 1 alone");
                var t = new N25Blocks { tutorial = true };
                Assert.That(Curriculum25.Start(c, t) == "n29-foundry" && Curriculum25.Next(c, t, "n29-foundry") == "n29-iceline" && Curriculum25.Next(c, t, "n29-kiln") == null, Is.True, "NIGHT29: the rebuilt levels 1-3 (the old plans stay in the tree, out of the order), nothing after them without the curriculum block");
                var all = new N25Blocks { tutorial = true, curriculum = true };
                Assert.That(Curriculum25.Order(c, all).Count == 11 && Curriculum25.Next(c, all, "n29-foundry") == "n29-iceline" && Curriculum25.Next(c, all, "n29-kiln") == "level3" && Curriculum25.Next(c, all, "level10") == null, Is.True);
            }
            finally { N30Blocks.ForceOff = was30; }
            // NIGHT30: with the n30 rows on, the order is the rebuilt four, then the old levels 3-10
            var all30 = new N25Blocks { tutorial = true, curriculum = true };
            bool was32 = N32Blocks.ForceOff;
            N32Blocks.ForceOff = true; // NIGHT32: the level rows (arenas32 / span32 swaps, level5) are ON by default; the NIGHT30 order is pinned with them forced off
            try { Assert.That(Curriculum25.Start(c, all30) == "n30-foundry" && Curriculum25.Next(c, all30, "n30-kiln") == "n30-span", Is.True, "NIGHT30: n30-foundry first, n30-span after n30-kiln"); }
            finally { N32Blocks.ForceOff = was32; }
            // NIGHT32: with the level rows on, the rebuilt files and LEVEL 5 right after level 4
            bool was33 = N33Blocks.ForceOff;
            N33Blocks.ForceOff = true; // NIGHT33: design.n33.kit33 (ON) swaps n32-* for the dressed n33-* files; NIGHT32's order is pinned with it forced off
            try
            {
                if (!N32Blocks.ForceOff)
                    Assert.That(Curriculum25.Start(c, all30) == "n32-foundry" && Curriculum25.Next(c, all30, "n32-kiln") == "n32-span" && Curriculum25.Next(c, all30, "n32-span") == "n32-spire", Is.True, "NIGHT32: n32-foundry first, n32-span after n32-kiln, n32-spire after n32-span");
            }
            finally { N33Blocks.ForceOff = was33; }
            bool was37 = N37Blocks.ForceOff;
            N37Blocks.ForceOff = true; // NIGHT37: the boss rows (loom37 / crown37 ON) swap n33-span / n33-spire for n37-span / n37-spire; NIGHT33's and NIGHT34's orders are pinned with them forced off
            try
            {
            // NIGHT33: with kit33 on, the same five levels as their n33 files, in the same order (NIGHT34's lines34 swap pinned off here)
            bool was34 = N34Blocks.ForceOff;
            N34Blocks.ForceOff = true; // NIGHT34: design.n34.lines34 (ON) swaps n33-foundry / iceline / kiln for the rebuilt n34-* files; NIGHT33's order is pinned with it forced off
            try
            {
                if (!N32Blocks.ForceOff && !N33Blocks.ForceOff && c.Design("n33.kit33.enabled"))
                    Assert.That(Curriculum25.Start(c, all30) == "n33-foundry" && Curriculum25.Next(c, all30, "n33-kiln") == "n33-span" && Curriculum25.Next(c, all30, "n33-span") == "n33-spire", Is.True, "NIGHT33: n33-foundry first, n33-span after n33-kiln, n33-spire after n33-span");
            }
            finally { N34Blocks.ForceOff = was34; }
            // NIGHT34: with lines34 on, levels 1-3 as their n34 files (span and spire keep their n33 files), in the same order (NIGHT35's campaign6 pinned off here)
            bool was35 = N35Blocks.ForceOff;
            N35Blocks.ForceOff = true; // NIGHT35: design.n35.campaign6 swaps levels 1-3 for the n35 files and adds LEVEL 6; NIGHT34's order is pinned with it forced off
            try
            {
            if (!N32Blocks.ForceOff && !N33Blocks.ForceOff && !N34Blocks.ForceOff && c.Design("n33.kit33.enabled") && c.Design("n34.lines34.enabled"))
                Assert.That(Curriculum25.Start(c, all30) == "n34-foundry" && Curriculum25.Next(c, all30, "n34-foundry") == "n34-iceline" && Curriculum25.Next(c, all30, "n34-kiln") == "n33-span" && Curriculum25.Next(c, all30, "n33-span") == "n33-spire", Is.True, "NIGHT34: n34-foundry first, n34-iceline next, n33-span after n34-kiln, n33-spire after n33-span");
            }
            finally { N35Blocks.ForceOff = was35; }
            }
            finally { N37Blocks.ForceOff = was37; }
            // NIGHT35: with campaign6 on, six levels: the swapped levels 1-5, then the finale, then nothing (the legacy levels leave the order)
            if (!N32Blocks.ForceOff && !N33Blocks.ForceOff && !N34Blocks.ForceOff && !N35Blocks.ForceOff && c.Design("n35.campaign6.enabled"))
            {
                var six = Curriculum25.Order(c, all30);
                Assert.That(six.Count == 6 && six[5] == (string)c.Get("design.n35.campaign6.finaleId") && Curriculum25.Next(c, all30, six[5]) == null, Is.True, "NIGHT35: six levels, the finale last: " + string.Join(",", six));
            }
            // NIGHT37: each boss row that is ON swaps its levelFrom for its levelTo in the order (and its levelFrom is gone); the six stay six
            if (!N32Blocks.ForceOff && !N33Blocks.ForceOff && !N34Blocks.ForceOff && !N35Blocks.ForceOff && !N36Blocks.ForceOff && !N37Blocks.ForceOff && c.Design("n35.campaign6.enabled"))
            {
                var order37 = Curriculum25.Order(c, all30);
                foreach (string row in new[] { "wick37", "stag37", "loom37", "crown37" })
                    if (c.Design("n37." + row + ".enabled"))
                        Assert.That(order37.Contains((string)c.Get("design.n37." + row + ".levelTo")) && !order37.Contains((string)c.Get("design.n37." + row + ".levelFrom")) && order37.Count == 6, Is.True, "NIGHT37: " + row + " swaps its level: " + string.Join(",", order37));
            }
        }

        // --- the stage rules in Campaign24 ----------------------------------------------------------------------------

        sealed class World : ICampaignWorld, ILessonSource
        {
            public Vec3 feet = new Vec3(0, 0, 14);
            public int alive, fires, kills;
            public readonly List<string> log = new List<string>();
            public bool dead;
            public Vec3 PlayerFeet => feet;
            public bool PlayerDead => dead;
            public string ZoneAt(Vec3 p) => p.z >= 0 ? "s1" : p.z >= -6 ? "d1" : "s2";
            public int EnemiesAlive => alive;
            public bool Spawn(string kind, Vec3 at) { alive++; log.Add("spawn " + kind); return true; }
            public void KillAllEnemies() { alive = 0; }
            public void LockZone(string id) { }
            public void OpenZone(string id) { log.Add("open " + id); }
            public void Respawn(Vec3 at, double yawDeg) { feet = at; log.Add("respawn"); }
            public void Revive() { dead = false; }
            public void SetLimits(int flags) { }
            public void AwardWeapon(string weapon) { }
            public bool BeginBoss(out Vec3 spawn, out double yawDeg) { spawn = default; yawDeg = 0; return false; }
            public bool BossDefeated => false;
            public LessonSample Sample(int step) => new LessonSample { step = step, feet = feet, grounded = true, fires = fires, kills = kills };
            public bool LessonAvailable(LessonKind kind) => true;
        }

        static LevelDef Level(string room1Extra) => LevelDef.From(PortData.Obj(JsJson.Parse(@"{ ""id"": ""t"", ""name"": ""T"", ""campaign"": { ""rooms"": [
          { ""id"": ""s1"", ""name"": ""ONE"", ""kind"": ""link"", ""spawn"": { ""at"": [0,0,14], ""yawDeg"": 0 }, ""exit"": [0,0,2], " + room1Extra + @" ""nodes"": [], ""waves"": [] },
          { ""id"": ""s2"", ""name"": ""TWO"", ""kind"": ""link"", ""spawn"": { ""at"": [0,0,-10], ""yawDeg"": 0 }, ""exit"": [0,0,-20] } ],
          ""doors"": [ { ""id"": ""d1"", ""between"": [""s1"", ""s2""] } ] } }")));

        [Test]
        public void ATaughtLink_ClearsOnItsLessons_NotOnReachingTheExit()
        {
            var w = new World();
            var events = new EventBus();
            var c = new Campaign24(Cfg, events, Level(@"""lessons"": [{ ""kind"": ""shots"", ""n"": 2 }],"), w, Dt);
            var seen = new List<string>();
            events.On("lessonBegin", _ => seen.Add("begin"));
            events.On("lessonClear", _ => seen.Add("clear"));
            c.Start(1);
            c.Step();
            Assert.That(c.room, Is.EqualTo(0));
            Assert.That(c.lesson == null, Is.False);
            w.feet = new Vec3(0, 0, 2); // standing on the exit: not the lesson
            for (int i = 0; i < 10; i++) c.Step();
            Assert.That(c.phase, Is.EqualTo(Campaign24.Phase.Fight), "the door stays shut until the lesson clears");
            w.fires = 2;
            c.Step();
            Assert.That(c.phase, Is.EqualTo(Campaign24.Phase.Cleared));
            Assert.That(seen.Count == 2 && seen[0] == "begin" && seen[1] == "clear", Is.True);
            Assert.That(w.log.Contains("open d1"), Is.True);
        }

        [Test]
        public void StageRespawn_IsNotADeath()
        {
            var w = new World();
            var c = new Campaign24(Cfg, new EventBus(), Level(@"""respawnY"": -3, ""lessons"": [{ ""kind"": ""shots"", ""n"": 99 }],"), w, Dt);
            c.Start(1);
            c.Step();
            w.feet = new Vec3(0, -3.5, 8); // the pit's floor
            c.Step();
            Assert.That(c.stageRespawns, Is.EqualTo(1));
            Assert.That(w.feet.z == 14, Is.True, "back at the stage's spawn");
            Assert.That(c.deaths, Is.EqualTo(0));
            Assert.That(c.retries, Is.EqualTo(0));
        }

        [Test]
        public void TheTopUp_IsCapped_AndThePendingEnemyLessonIsPassedOver()
        {
            var w = new World();
            var json = @"{ ""id"": ""t"", ""name"": ""T"", ""campaign"": { ""rooms"": [
              { ""id"": ""s1"", ""name"": ""ONE"", ""kind"": ""room"", ""spawn"": { ""at"": [0,0,14], ""yawDeg"": 0 }, ""exit"": [0,0,2], ""lessons"": [{ ""kind"": ""husksDead"", ""n"": 99 }],
                ""nodes"": [[10,0,10]], ""waves"": [[""husk""]] } ], ""doors"": [] } }";
            var c = new Campaign24(Cfg, new EventBus(), LevelDef.From(PortData.Obj(JsJson.Parse(json))), w, Dt);
            c.Start(1);
            for (int round = 0; round < 200 && !c.finished; round++)
            {
                for (int i = 0; i < 400 && w.alive == 0 && !c.finished; i++) c.Step();
                w.alive = 0; // the husks die at once and never reach 99 kills
                for (int i = 0; i < 5 && !c.finished; i++) c.Step();
            }
            Assert.That(c.finished, Is.True, "the room clears after the cap: no wave for ever");
            Assert.That(c.roomTopUps == 12, Is.True, "the cap is design.n25.lesson.maxTopUps");
        }

        [Test]
        public void ARoomWithAnEnemyLesson_TopsItsLastWaveUp_WhenTheEnemiesDieEarly()
        {
            var w = new World();
            var json = @"{ ""id"": ""t"", ""name"": ""T"", ""campaign"": { ""rooms"": [
              { ""id"": ""s1"", ""name"": ""ONE"", ""kind"": ""room"", ""spawn"": { ""at"": [0,0,14], ""yawDeg"": 0 }, ""exit"": [0,0,2], ""lessons"": [{ ""kind"": ""husksDead"", ""n"": 3 }],
                ""nodes"": [[10,0,10]], ""waves"": [[""husk"",""husk""]] } ], ""doors"": [] } }";
            var c = new Campaign24(Cfg, new EventBus(), LevelDef.From(PortData.Obj(JsJson.Parse(json))), w, Dt);
            c.Start(1);
            for (int i = 0; i < 400 && w.alive == 0; i++) c.Step();
            Assert.That(w.alive, Is.EqualTo(2), "wave 1 spawned");
            w.alive = 0; w.kills = 2; // both died: 2 kills, the lesson wants 3
            for (int i = 0; i < 400 && w.alive == 0; i++) c.Step();
            Assert.That(c.lessonTopUps, Is.EqualTo(1), "the wave is spawned again");
            Assert.That(c.phase, Is.EqualTo(Campaign24.Phase.Fight));
            w.alive = 0; w.kills = 4;
            for (int i = 0; i < 5; i++) c.Step();
            Assert.That(c.finished, Is.True, "the lesson is done and the room is empty: the last room ends the level");
        }
    }
}
