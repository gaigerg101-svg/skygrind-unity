// NIGHT12 U3-25.7 (NIGHT12 rule 6): the JSON profile (Core/Profile.cs): a round trip keeps every field, a missing, corrupt
// or other-version file starts empty, the PlayerPrefs records migrate once, the best grade per HEAT rung only rises.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public sealed class ProfileTests
    {
        static Profile Full()
        {
            var p = new Profile();
            p.records.Submit("survival", new Records.Run { round = 12, points = 34567, date = 1759500000000, exfil = true });
            p.recordsMigrated = true;
            p.SetFlag("protocol");
            p.SetFlag("round10");
            p.Contract("headhunter").v = 37;
            var done = p.Contract("round10");
            done.v = 10;
            done.done = true;
            p.SetMastery("shotgun", "kills", 41);
            p.SetMastery("shotgun", "fresh", 7);
            p.SetMastery("revolver", "kills", 3);
            p.finishes["shotgun"] = "graphite";
            p.SubmitGrade(0, "B");
            p.SubmitGrade(2, "S");
            p.heat = 2;
            p.tips["break"] = 2;
            p.tips["wallBuy \"quoted\""] = 1;
            p.onboarded = true;
            p.mutatorBest = 15;
            p.mutatorPicks.Add("lowGravity");
            p.runs = 4;
            return p;
        }

        [Test]
        public void ARoundTrip_KeepsEveryField_AndTheSameBytes()
        {
            Profile a = Full();
            string json = a.ToJson();
            Profile b = Profile.Read(json);
            Assert.That(b.ToJson(), Is.EqualTo(json));
            Assert.That(b.records.Best("survival").round, Is.EqualTo(12));
            Assert.That(b.records.Best("survival").exfil, Is.True);
            Assert.That(b.Flag("protocol") && b.Flag("round10") && !b.Flag("secretRelay"), Is.True);
            Assert.That(b.Contract("headhunter").v, Is.EqualTo(37));
            Assert.That(b.Contract("round10").done, Is.True);
            Assert.That(b.Mastery("shotgun", "kills"), Is.EqualTo(41));
            Assert.That(b.Mastery("rail", "kills"), Is.EqualTo(0));
            Assert.That(b.FinishOf("shotgun"), Is.EqualTo("graphite"));
            Assert.That(b.BestGrade(2), Is.EqualTo("S"));
            Assert.That(b.BestGrade(1), Is.Null);
            Assert.That(b.heat, Is.EqualTo(2));
            Assert.That(b.TipCount("wallBuy \"quoted\""), Is.EqualTo(1));
            Assert.That(b.onboarded, Is.True);
            Assert.That(b.mutatorPicks, Is.EqualTo(new[] { "lowGravity" }));
            Assert.That(b.runs, Is.EqualTo(4));
        }

        [Test]
        public void NIGHT24_ANight23ProfileWithADailyRecord_Loads_TheRecordIsIgnoredAndNotWritten()
        {
            // NIGHT23 wrote "daily":{...} (the DAILY's record); the DAILY is gone: the field is ignored and the rest loads as it was.
            string json = Full().ToJson();
            string night23 = json.Replace(",\"tips\":{", ",\"daily\":{\"day\":\"2026-10-03\",\"first\":7,\"best\":9,\"streak\":3,\"lastDay\":\"2026-10-03\"},\"tips\":{");
            Assert.That(night23, Is.Not.EqualTo(json), "the fixture added the old field");
            Profile p = Profile.Read(night23);
            Assert.That(p.runs, Is.EqualTo(4));
            Assert.That(p.heat, Is.EqualTo(2));
            Assert.That(p.Contract("headhunter").v, Is.EqualTo(37));
            Assert.That(p.ToJson(), Is.EqualTo(json), "no daily field is written back");
            Assert.That(p.ToJson(), Does.Not.Contain("daily"));
            Assert.That(Profile.Read("{\"v\":1,\"daily\":null}").runs, Is.EqualTo(0), "the old null form loads too");
        }

        [Test]
        public void AMissingCorruptOrOtherVersionProfile_StartsEmpty()
        {
            string empty = new Profile().ToJson();
            Assert.That(Profile.Read(null).ToJson(), Is.EqualTo(empty));
            Assert.That(Profile.Read("").ToJson(), Is.EqualTo(empty));
            Assert.That(Profile.Read("{not json").ToJson(), Is.EqualTo(empty));
            Assert.That(Profile.Read("[1,2]").ToJson(), Is.EqualTo(empty));
            Assert.That(Profile.Read(Full().ToJson().Replace("{\"v\":1,", "{\"v\":2,")).ToJson(), Is.EqualTo(empty));
            Assert.That(Profile.Load(Path.Combine(Path.GetTempPath(), "skygrind-no-such-profile.json")).ToJson(), Is.EqualTo(empty));
        }

        [Test]
        public void ThePlayerPrefsRecords_MigrateOnce_AndNeverOverwriteTheProfiles()
        {
            var old = new Records();
            old.Submit("survival", new Records.Run { round = 8, points = 9000, date = 1 });
            string raw = old.ToJson();

            var p = new Profile();
            Assert.That(p.MigrateRecords(raw), Is.True);
            Assert.That(p.records.Best("survival").round, Is.EqualTo(8));
            Assert.That(p.MigrateRecords(raw), Is.False, "once only");

            var q = new Profile();
            q.records.Submit("survival", new Records.Run { round = 3, points = 10, date = 2 });
            Assert.That(q.MigrateRecords(raw), Is.True);
            Assert.That(q.records.Best("survival").round, Is.EqualTo(3), "a profile's own record wins");

            var r = new Profile();
            Assert.That(r.MigrateRecords(""), Is.True);
            Assert.That(r.records.modes.Count, Is.EqualTo(0));
            Assert.That(Profile.Read(r.ToJson()).recordsMigrated, Is.True);
        }

        [Test]
        public void TheBestGradePerRung_OnlyRises_AndPBeatsS()
        {
            var p = new Profile();
            Assert.That(p.SubmitGrade(1, "C"), Is.True);
            Assert.That(p.SubmitGrade(1, "D"), Is.False);
            Assert.That(p.SubmitGrade(1, "C"), Is.False);
            Assert.That(p.SubmitGrade(1, "S"), Is.True);
            Assert.That(p.SubmitGrade(1, "P"), Is.True);
            Assert.That(p.SubmitGrade(1, "A"), Is.False);
            Assert.That(p.SubmitGrade(1, "X"), Is.False);
            Assert.That(p.BestGrade(1), Is.EqualTo("P"));
            Assert.That(p.BestGrade(0), Is.Null);
        }

        [Test]
        public void CopyFrom_RestoresAProfileInPlace()
        {
            Profile snapshot = Full();
            var live = Profile.Read(snapshot.ToJson());
            Profile same = live;
            live.SetFlag("daily");
            live.Contract("kills").v = 99;
            live.heat = 3;
            live.records.Submit("survival", new Records.Run { round = 40, points = 1, date = 3 });
            live.mutatorPicks.Clear();
            live.CopyFrom(snapshot);
            Assert.That(ReferenceEquals(live, same), Is.True);
            Assert.That(live.ToJson(), Is.EqualTo(snapshot.ToJson()));
        }

        [Test]
        public void SaveThenLoad_ReadsTheSameProfile_AndAFlagIsNewOnlyOnce()
        {
            Profile a = Full();
            Assert.That(a.SetFlag("protocol"), Is.False);
            Assert.That(a.SetFlag("secretRelay"), Is.True);
            string path = Path.Combine(Path.GetTempPath(), "skygrind-profile-test", Profile.FileName);
            try
            {
                Assert.That(a.Save(path), Is.True);
                Assert.That(a.Save(path), Is.True, "over an existing file");
                Assert.That(Profile.Load(path).ToJson(), Is.EqualTo(a.ToJson()));
                Assert.That(File.Exists(path + ".tmp"), Is.False);
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(path), true);
            }
        }
    }
}
