// NIGHT24 leftovers 1 and 2 (builder R). No UnityEngine: SimHarness runs the same assertions (`ranks --selftest`).
//  1. the extra LIMIT flags NO SLIDE, NO JUMP, MELEE ONLY (design.n24.campaign.limitFlags): a LIMIT frame removes exactly that verb's key and nothing
//     else; the flags are in force in the room and restored (0) at the door; the tape's Campaign24State.limits carries them; with the sub-block
//     off the three names are masked away on entry;
//  2. room ranks S / A / B / C (design.n24.campaign.rank): by time over par, a death drops a letter; the overall rank and the closing stamp text.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Campaign24LimitRankTests
    {
        const double Dt = 1.0 / 120;

        static GameConfig NewCfg() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class World : ICampaignWorld
        {
            public Vec3 feet = new Vec3(0, 0, 0);
            public bool dead;
            public int alive, limits;
            public readonly List<string> log = new List<string>();
            public Vec3 PlayerFeet => feet;
            public bool PlayerDead => dead;
            // rooms are strips along z: r1 z -20..20, d1 -26..-20, r2 -66..-26
            public string ZoneAt(Vec3 p) => p.z >= -20 ? "r1" : p.z >= -26 ? "d1" : "r2";
            public int EnemiesAlive => alive;
            public bool Spawn(string kind, Vec3 at) { alive++; return true; }
            public void KillAllEnemies() { alive = 0; }
            public void LockZone(string id) { }
            public void OpenZone(string id) { }
            public void Respawn(Vec3 at, double yawDeg) { feet = at; }
            public void Revive() { dead = false; }
            public void SetLimits(int flags) { limits = flags; log.Add("limits " + flags); }
            public void AwardWeapon(string weapon) { }
            public bool BeginBoss(out Vec3 spawn, out double yawDeg) { spawn = default; yawDeg = 0; return false; }
            public bool BossDefeated => false;
        }

        static LevelDef Level(string limitList)
        {
            var json = PortData.Obj(JsJson.Parse(@"{ ""id"": ""t"", ""name"": ""LAYER T"", ""campaign"": { ""rooms"": [
              { ""id"": ""r1"", ""name"": ""ONE"", ""kind"": ""room"", ""par"": 20, ""spawn"": { ""at"": [0,0,14], ""yawDeg"": 0 }, ""exit"": [0,0,-17],
                ""nodes"": [[-14,0,-10],[14,0,-10]], ""waves"": [[""husk""]] },
              { ""id"": ""r2"", ""name"": ""TWO"", ""kind"": ""room"", ""par"": 30, ""spawn"": { ""at"": [0,0,-30], ""yawDeg"": 0 }, ""exit"": [0,0,-63],
                ""nodes"": [[-14,0,-40],[14,0,-40]], ""waves"": [[""husk""]], ""limit"": " + limitList + @" } ],
              ""doors"": [ { ""id"": ""d1"", ""between"": [""r1"", ""r2""] } ] } }"));
            return LevelDef.From(json);
        }

        static Campaign24 Make(World w, EventBus events, LevelDef level, GameConfig cfg)
        {
            var c = new Campaign24(cfg, events, level, w, Dt);
            c.Start(7);
            return c;
        }

        static InputFrame Full() => new InputFrame
        {
            moveX = 0.5, moveZ = 1, yaw = 1.25, pitch = -0.1,
            jump = new Button { held = true, pressed = true }, dash = new Button { held = true, pressed = true }, slide = new Button { held = true, pressed = true },
            grapple = new Button { held = true }, reel = new Button { held = true }, sprint = new Button { held = true },
        };

        // --- 1. the flags -------------------------------------------------------------------------------------------------

        [Test]
        public void ALimitFrame_RemovesExactlyThatVerb_AndNothingElse()
        {
            var cases = new (int flag, string verb)[] { (Limit24.NoDash, "dash"), (Limit24.NoSlide, "slide"), (Limit24.NoJump, "jump") };
            foreach (var (flag, verb) in cases)
            {
                InputFrame f = Full();
                Limit24.Strip(ref f, flag);
                Assert.That(f.dash.held, Is.EqualTo(verb != "dash"), verb + ": dash");
                Assert.That(f.slide.held, Is.EqualTo(verb != "slide"), verb + ": slide");
                Assert.That(f.jump.held, Is.EqualTo(verb != "jump"), verb + ": jump");
                Assert.That(f.jump.pressed, Is.EqualTo(verb != "jump"));
                Assert.That((f.moveX, f.moveZ, f.yaw, f.pitch), Is.EqualTo((0.5, 1.0, 1.25, -0.1)), verb + ": the move and the look are kept");
                Assert.That(f.grapple.held && f.reel.held && f.sprint.held, Is.True, verb + ": grapple, reel and sprint are kept");
            }
            InputFrame none = Full();
            Limit24.Strip(ref none, Limit24.PistolOnly | Limit24.MeleeOnly);
            Assert.That(none.dash.held && none.slide.held && none.jump.held, Is.True, "the weapon flags strip no key");
            InputFrame all = Full();
            Limit24.Strip(ref all, Limit24.NoDash | Limit24.NoSlide | Limit24.NoJump);
            Assert.That(all.dash.held || all.slide.held || all.jump.held, Is.False);
            Assert.That(all.moveZ, Is.EqualTo(1.0));
        }

        [Test]
        public void TheNewFlags_ParseAndHaveBanners_InAFixedOrder()
        {
            int f = Limit24.Parse(JsJson.Parse(@"[""noJump"", ""meleeOnly"", ""noSlide""]"));
            Assert.That(f, Is.EqualTo(Limit24.NoJump | Limit24.MeleeOnly | Limit24.NoSlide));
            Assert.That(Limit24.Banner(f), Is.EqualTo("MELEE ONLY / NO SLIDE / NO JUMP"));
            Assert.That(Limit24.Banner(Limit24.PistolOnly | Limit24.NoDash), Is.EqualTo("PISTOL ONLY / NO DASH"), "NIGHT24's first banner is unchanged");
            Assert.That((Limit24.Extra & (Limit24.PistolOnly | Limit24.NoDash)), Is.EqualTo(0));
        }

        [Test]
        public void ANoSlideRoom_IsInForceInTheRoom_InTheTapesState_AndRestoredAtTheDoor() => FlagRoom("[\"noSlide\"]", Limit24.NoSlide);

        [Test]
        public void ANoJumpRoom_IsInForceInTheRoom_InTheTapesState_AndRestoredAtTheDoor() => FlagRoom("[\"noJump\"]", Limit24.NoJump);

        [Test]
        public void AMeleeOnlyRoom_IsInForceInTheRoom_InTheTapesState_AndRestoredAtTheDoor() => FlagRoom("[\"meleeOnly\"]", Limit24.MeleeOnly);

        static void FlagRoom(string list, int flag)
        {
            var w = new World { feet = new Vec3(0, 0, -30) };
            Campaign24 c = Make(w, new EventBus(), Level(list), NewCfg());
            c.room = 0; c.phase = Campaign24.Phase.Cleared;
            c.Step();
            Assert.That(c.room, Is.EqualTo(1));
            Assert.That(w.limits, Is.EqualTo(flag), "stripped on entry");
            Assert.That(c.State.limits, Is.EqualTo(flag), "the tape's state carries it");
            w.alive = 0;
            for (int i = 0; i < 3; i++) c.Step();
            Assert.That(c.phase, Is.EqualTo(Campaign24.Phase.Cleared));
            Assert.That(w.limits, Is.EqualTo(0), "restored at the door");
            Assert.That(c.State.limits, Is.EqualTo(0));
        }

        [Test]
        public void WithTheSubBlockOff_TheThreeNewNamesAreMaskedAway_TheFirstTwoStay()
        {
            GameConfig off = NewCfg();
            PortData.Obj(PortData.Obj(PortData.Obj(PortData.Obj(off.root["design"])["n24"])["campaign"])["limitFlags"])["enabled"] = false;
            var w = new World { feet = new Vec3(0, 0, -30) };
            Campaign24 c = Make(w, new EventBus(), Level("[\"noSlide\", \"noJump\", \"meleeOnly\", \"noDash\"]"), off);
            c.room = 0; c.phase = Campaign24.Phase.Cleared;
            c.Step();
            Assert.That(w.limits, Is.EqualTo(Limit24.NoDash), "only NO DASH survives the mask");
        }

        // --- 2. the ranks -------------------------------------------------------------------------------------------------

        [Test]
        public void ARoomsRank_ComesFromItsTimeOverPar_AndEachDeathDropsALetter()
        {
            Campaign24 c = Make(new World(), new EventBus(), Level("[]"), NewCfg());
            Assert.That(c.RankOf(20, 20, 0), Is.EqualTo('S'), "within par");
            Assert.That(c.RankOf(10, 20, 0), Is.EqualTo('S'));
            Assert.That(c.RankOf(20.01, 20, 0), Is.EqualTo('A'));
            Assert.That(c.RankOf(30, 20, 0), Is.EqualTo('A'), "1.5 x par");
            Assert.That(c.RankOf(30.01, 20, 0), Is.EqualTo('B'));
            Assert.That(c.RankOf(50, 20, 0), Is.EqualTo('B'), "2.5 x par");
            Assert.That(c.RankOf(50.01, 20, 0), Is.EqualTo('C'));
            Assert.That(c.RankOf(10, 20, 1), Is.EqualTo('A'), "a death: one letter");
            Assert.That(c.RankOf(10, 20, 3), Is.EqualTo('C'));
            Assert.That(c.RankOf(10, 20, 9), Is.EqualTo('C'), "never below C");
            Assert.That(c.RankOf(99, 0, 0), Is.EqualTo('S'), "no par: by deaths only");
            Assert.That(c.RankOf(99, 0, 1), Is.EqualTo('A'));
        }

        [Test]
        public void ClearingARoom_EmitsItsRank_AndTheLayersStampListsThemWithTheOverall()
        {
            var w = new World { feet = new Vec3(0, 0, 14) };
            var events = new EventBus();
            var ranks = new List<char>();
            events.On<RoomEvent>("roomClear", e => ranks.Add(e.rank));
            Campaign24 c = Make(w, events, Level("[]"), NewCfg());
            c.Step();
            Assert.That(c.room, Is.EqualTo(0));
            w.alive = 0;
            for (int i = 0; i < 400; i++) c.Step(); // ~3.3 s of game time, far under the 20 s par
            Assert.That(ranks, Is.EqualTo(new List<char> { 'S' }), "room one: S");
            Assert.That(c.roomRanks, Is.EqualTo(new List<char> { 'S' }));
            // the rest of the layer by hand: A then C -> overall (3 + 2 + 0) / 3 = 1.67 -> A
            c.roomRanks.Add('A');
            c.roomRanks.Add('C');
            Assert.That(c.OverallRank(), Is.EqualTo('A'));
            Assert.That(c.StampText().StartsWith("LAYER T CLEARED  S A C  OVERALL A  "), Is.True, c.StampText());
            c.roomRanks.Clear();
            c.roomRanks.AddRange(new[] { 'B', 'C' });
            Assert.That(c.OverallRank(), Is.EqualTo('B'), "(1 + 0) / 2 = 0.5 rounds up: the tie goes to the better letter");
        }

        [Test]
        public void WithTheRankSubBlockOff_NoRankIsGiven()
        {
            GameConfig off = NewCfg();
            PortData.Obj(PortData.Obj(PortData.Obj(PortData.Obj(off.root["design"])["n24"])["campaign"])["rank"])["enabled"] = false;
            var w = new World { feet = new Vec3(0, 0, 14) };
            var events = new EventBus();
            var ranks = new List<char>();
            events.On<RoomEvent>("roomClear", e => ranks.Add(e.rank));
            Campaign24 c = Make(w, events, Level("[]"), off);
            c.Step();
            w.alive = 0;
            for (int i = 0; i < 400; i++) c.Step();
            Assert.That(ranks, Is.EqualTo(new List<char> { '\0' }));
            Assert.That(c.roomRanks.Count, Is.EqualTo(0));
            Assert.That(c.StampText(), Is.EqualTo(""));
        }
    }
}
