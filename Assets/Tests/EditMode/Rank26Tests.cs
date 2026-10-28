// NIGHT26 (builder L): the ranks (Rank26: TIME / KILLS / STYLE, the aggregate, restarts, P), the profile's per-level best (an old profile loads with none), the
// level select's slots and cursor (LevelSelect26), and the campaign's "no BIG PICK" row. No UnityEngine: SimHarness can run the same assertions.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Rank26Tests
    {
        static Rank26Params P() => new Rank26Params { aggregateMin = new[] { 15, 12, 9, 6 }, timeShares = new[] { 1.0, 1.5, 2.5, 4.0 }, killsDefault = new double[] { 100, 80, 60, 40 }, styleDefault = new double[] { 3000, 2000, 1000, 400 } };

        static RankTable26 T() => new RankTable26 { time = new double[] { 120, 180, 300, 480 }, kills = new double[] { 100, 85, 65, 40 }, style = new double[] { 4000, 2500, 1200, 500 } };

        [Test]
        public void ColumnLettersFollowTheCuts()
        {
            RankTable26 t = T();
            Assert.AreEqual('S', Rank26.Letter(120, t.time, true));
            Assert.AreEqual('A', Rank26.Letter(120.01, t.time, true));
            Assert.AreEqual('C', Rank26.Letter(480, t.time, true));
            Assert.AreEqual('D', Rank26.Letter(480.5, t.time, true));
            Assert.AreEqual('S', Rank26.Letter(100, t.kills, false));
            Assert.AreEqual('B', Rank26.Letter(70, t.kills, false));
            Assert.AreEqual('D', Rank26.Letter(10, t.kills, false));
            Assert.AreEqual('A', Rank26.Letter(2500, t.style, false));
        }

        [Test]
        public void AggregateIsTheSumOfLettersMinusRestarts()
        {
            Rank26Params p = P();
            LevelRank26 a = Rank26.Compute(T(), p, 150, 90, 3000, 0); // A A A... time A (4), kills A (4), style A (4) = 12
            Assert.AreEqual(12, a.points);
            Assert.AreEqual('A', a.aggregate);
            LevelRank26 b = Rank26.Compute(T(), p, 150, 90, 3000, 1); // 12 - 1 = 11: B
            Assert.AreEqual(11, b.points);
            Assert.AreEqual('B', b.aggregate);
            LevelRank26 d = Rank26.Compute(T(), p, 9999, 0, 0, 0); // D D D = 3
            Assert.AreEqual(3, d.points);
            Assert.AreEqual('D', d.aggregate);
            LevelRank26 floor = Rank26.Compute(T(), p, 9999, 0, 0, 9); // never below 0
            Assert.AreEqual(0, floor.points);
        }

        [Test]
        public void PIsSInAllThreeWithNoRestart()
        {
            Rank26Params p = P();
            LevelRank26 s = Rank26.Compute(T(), p, 100, 100, 5000, 0);
            Assert.IsTrue(s.p);
            Assert.AreEqual('P', s.aggregate);
            Assert.AreEqual(15, s.points);
            LevelRank26 restarted = Rank26.Compute(T(), p, 100, 100, 5000, 1); // all S but one restart: 14 points, an A, not P
            Assert.IsFalse(restarted.p);
            Assert.AreEqual(14, restarted.points);
            Assert.AreEqual('A', restarted.aggregate);
            LevelRank26 almost = Rank26.Compute(T(), p, 100, 100, 3000, 0); // style A: no P
            Assert.IsFalse(almost.p);
            Assert.AreEqual('A', almost.aggregate); // 14 points
        }

        [Test]
        public void DefaultsComeFromTheParsWhenAPlanHasNoRankBlock()
        {
            RankTable26 d = P().DefaultsFor(100);
            Assert.AreEqual(100, d.time[0]);
            Assert.AreEqual(150, d.time[1]);
            Assert.AreEqual(400, d.time[3]);
            Assert.AreEqual(80, d.kills[1]);
        }

        [Test]
        public void APlanRankBlockIsRead()
        {
            var json = PortData.Obj(JsJson.Parse(@"{ ""id"": ""t"", ""campaign"": { ""rooms"": [
              { ""id"": ""r1"", ""kind"": ""room"", ""par"": 20, ""spawn"": { ""at"": [0,0,14], ""yawDeg"": 0 }, ""exit"": [0,0,-17], ""award"": ""shotgun"" } ],
              ""doors"": [], ""rank"": { ""time"": [60,90,120,180], ""kills"": [100,80,60,40], ""style"": [900,600,300,100] } } }"));
            LevelDef lv = LevelDef.From(json);
            Assert.NotNull(lv.rank);
            Assert.AreEqual(90, lv.rank.time[1]);
            Assert.AreEqual(900, lv.rank.style[0]);
            Assert.AreEqual("shotgun", lv.LastAward);
            Assert.AreSame(lv.rank, lv.RankFor(P()));
        }

        [Test]
        public void ProfileKeepsTheBestOfEachColumnAndRoundTrips()
        {
            var prof = new Profile();
            Rank26Params p = P();
            prof.SubmitLevel("tutorial", Rank26.Compute(T(), p, 150, 90, 3000, 0)); // A A A
            prof.SubmitLevel("tutorial", Rank26.Compute(T(), p, 110, 60, 600, 2)); // time S, kills C, style D, aggregate lower
            LevelBest26 b = prof.LevelBest("tutorial");
            Assert.AreEqual(2, b.clears);
            Assert.AreEqual('S', b.time); // the better time letter replaced it
            Assert.AreEqual('A', b.kills); // the first run's kills letter stays
            Assert.AreEqual('A', b.aggregate);
            Assert.AreEqual(110, b.seconds, 1e-9);
            Assert.AreEqual(90, b.killPct, 1e-9);
            Profile back = Profile.Read(prof.ToJson());
            LevelBest26 r = back.LevelBest("tutorial");
            Assert.NotNull(r);
            Assert.AreEqual(b.ToJson(), r.ToJson());
            Assert.IsNull(back.LevelBest("layer1"));
        }

        [Test]
        public void AnOldProfileLoadsWithNoRanksAndWritesNoRankKey()
        {
            var old = new Profile { runs = 3, heat = 1 };
            string json = old.ToJson();
            StringAssert.DoesNotContain("levelBest", json); // a profile that cleared no level is the bytes it was before NIGHT26
            Profile loaded = Profile.Read(json);
            Assert.AreEqual(0, loaded.levelBest.Count);
            Assert.AreEqual(3, loaded.runs);
            Assert.AreEqual(json, loaded.ToJson());
            Profile copy = new Profile();
            var prof = new Profile();
            prof.SubmitLevel("tutorial", Rank26.Compute(T(), P(), 100, 100, 5000, 0));
            copy.CopyFrom(prof);
            Assert.AreEqual('P', copy.LevelBest("tutorial").aggregate);
        }

        [Test]
        public void TheSelectLocksLevelsFourUpAndPutsTheCursorOnTheNextOne()
        {
            var order = new List<string> { "tutorial", "level2", "layer1", "level3", "level4" };
            var prof = new Profile();
            var par = new LevelSelect26Params { slots = 10, playable = 3, sequential = false };
            var maps = new HashSet<string>(order);
            System.Func<string, bool> has = id => maps.Contains(id);
            System.Func<string, string> label = id => id == "tutorial" ? "LEVEL 1 THE FOUNDRY" : id == "level2" ? "LEVEL 2 THE ICE LINE" : id == "layer1" ? "LEVEL 3 THE KILN" : "LEVEL 4 X";
            System.Func<string, string> unlock = id => id == "tutorial" ? "shotgun" : id == "level2" ? "nailgun" : id == "layer1" ? "rattler" : null;
            List<LevelSlot26> s = LevelSelect26.Slots(order, prof, par, has, label, unlock);
            Assert.AreEqual(10, s.Count);
            Assert.AreEqual("THE FOUNDRY", s[0].name);
            Assert.IsTrue(s[0].playable && s[1].playable && s[2].playable);
            Assert.IsFalse(s[3].playable, "level 4 exists as a file but is LOCKED");
            Assert.IsFalse(s[9].exists);
            Assert.AreEqual("rattler", s[2].unlock);
            Assert.AreEqual(0, LevelSelect26.Cursor(s));
            prof.SubmitLevel("tutorial", Rank26.Compute(T(), P(), 150, 90, 3000, 0));
            prof.SubmitLevel("level2", Rank26.Compute(T(), P(), 250, 70, 1300, 0));
            s = LevelSelect26.Slots(order, prof, par, has, label, unlock);
            Assert.AreEqual(2, LevelSelect26.Cursor(s));
            Assert.AreEqual("layer1", s[LevelSelect26.Cursor(s)].id);
            Assert.AreEqual(2, LevelSelect26.Step(s, 1, 1));
            Assert.AreEqual(2, LevelSelect26.Step(s, 2, 1), "a down press past the last playable level stays");
            prof.SubmitLevel("layer1", Rank26.Compute(T(), P(), 150, 90, 3000, 0));
            s = LevelSelect26.Slots(order, prof, par, has, label, unlock);
            Assert.AreEqual(2, LevelSelect26.Cursor(s), "all three cleared: the cursor rests on the last playable level");
            var seq = new LevelSelect26Params { slots = 10, playable = 3, sequential = true };
            s = LevelSelect26.Slots(order, new Profile(), seq, has, label, unlock);
            Assert.IsTrue(s[0].playable);
            Assert.IsFalse(s[1].playable, "sequential: level 2 needs level 1 cleared");
        }

        [Test]
        public void UnlocksFileReadsTheThreeShapes()
        {
            Assert.AreEqual("shotgun", UnlocksFile26.Read(@"{ ""unlocks"": { ""tutorial"": ""shotgun"" } }")["tutorial"]);
            Assert.AreEqual("nailgun", UnlocksFile26.Read(@"{ ""level2"": ""nailgun"", ""_why"": ""x"" }")["level2"]);
            Assert.AreEqual("rattler", UnlocksFile26.Read(@"{ ""unlocks"": [ { ""level"": ""layer1"", ""weapon"": ""rattler"" } ] }")["layer1"]);
            Assert.AreEqual(0, UnlocksFile26.Read("not json").Count);
        }

        [Test]
        public void TheCampaignOffersNoBigPick()
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            Assert.IsFalse(cfg.Design("n24.bigPick.campaign"), "GG: no end-of-level BIG PICK in the campaign (the survival modes keep it)");
        }
    }
}
