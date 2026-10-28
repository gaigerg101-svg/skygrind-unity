// NIGHT45 builder S1 (rows as data; NIGHT45.md card S1): Core/NightRows.cs in isolation (the registry is unwired tonight). Every test builds
// its own design (no row in the real config is touched) except TheRealConfig_HasNoSimRows_AndNoProblems. Runs in Unity (EditMode) and
// headless: `dotnet run --project Tools/SimHarness -c Release -p:Extra=../../Assets/Tests/EditMode/NightRowsTests.cs -- n45rows selftest`.
// The rules pinned:
//   1. only `sim: true` rows of nights >= 46 whose name ends with the night are sim rows; order = nights, then names by ordinal (not JSON order)
//   2. the defaults follow `enabled`; -skygrindOffNN / --offNN turn off every row of night NN and later; a missing row reads off
//   3. the checker names every malformed row; the real config is clean and has no sim rows tonight
//   4. the tape record round-trips name -> bit; an old record (or an old format) replays the new rows off; a row ON that the build lacks is
//      refused; a bad record is refused
//   5. Through(n) (the composed fingerprint), level rows (swap + base id) and view rows follow the same off switch
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class NightRowsTests
    {
        const string Why = @"""_why"": ""test""";

        static Dictionary<string, object> D(string body) => PortData.Obj(JsJson.Parse("{" + body + "}"));

        static string configText;
        static GameConfig WithDesign(string body)
        {
            configText ??= File.ReadAllText(Path.Combine(Port.Root, "config.json"));
            return GameConfig.FromText(configText, @"{ ""design"": {" + body + "} }");
        }

        static readonly string Fixture =
            @"""n43"": { ""chimney43"": { ""enabled"": true } },
              ""n45"": { ""look45"": { ""enabled"": true } },
              ""n46"": { ""_why"": ""x"", ""wall46"": { ""sim"": true, ""enabled"": true, " + Why + @" }, ""look46"": { ""enabled"": true, " + Why + @" }, ""pour46"": { ""sim"": true, ""enabled"": false, " + Why + @" } },
              ""n47"": { ""grip47"": { ""sim"": true, ""enabled"": true, " + Why + @" }, ""calm47"": { ""sim"": false, ""enabled"": true, " + Why + @" } }";

        static void Throws<T>(Action a, string contains, string what) where T : Exception
        {
            try
            {
                a();
            }
            catch (T e)
            {
                Assert.That(contains == null || e.Message.Contains(contains), Is.True, what + ": the message names it (" + e.Message + ")");
                return;
            }
            Assert.That(false, Is.True, what + ": expected " + typeof(T).Name);
        }

        [Test]
        public void TheRegistry_ReadsSimRowsOnly_InNightThenNameOrder()
        {
            NightRows reg = NightRows.FromDesign(D(Fixture));
            Assert.That(reg.Count, Is.EqualTo(3), reg.ToString());
            Assert.That(reg.rows[0].name, Is.EqualTo("pour46"));
            Assert.That(reg.rows[1].name, Is.EqualTo("wall46"));
            Assert.That(reg.rows[2].name, Is.EqualTo("grip47"));
            Assert.That(reg.rows[2].night, Is.EqualTo(47));
            Assert.That(reg.IndexOf("look46"), Is.EqualTo(-1), "a view row is no sim row");
            Assert.That(reg.IndexOf("calm47"), Is.EqualTo(-1), "sim: false is no sim row");
            Assert.That(reg.IndexOf("chimney43"), Is.EqualTo(-1), "NIGHT43 keeps its N43Blocks");

            // the JSON's key order never changes a row's id
            NightRows rev = NightRows.FromDesign(D(@"""n47"": { ""grip47"": { ""sim"": true, ""enabled"": true, " + Why + @" } },
                ""n46"": { ""wall46"": { ""sim"": true, ""enabled"": true, " + Why + @" }, ""pour46"": { ""sim"": true, ""enabled"": false, " + Why + @" } }"));
            for (int i = 0; i < reg.Count; i++) Assert.That(rev.rows[i].name, Is.EqualTo(reg.rows[i].name), "row " + i);

            Assert.That(NightRows.FromDesign(null).Count, Is.EqualTo(0));
            Assert.That(NightRows.NightOfName("wall46"), Is.EqualTo(46));
            Assert.That(NightRows.NightOfName("wall"), Is.EqualTo(-1));
            Assert.That(NightRows.NightOfName("46"), Is.EqualTo(-1));
            Assert.That(NightRows.NightKey("n46"), Is.EqualTo(46));
            Assert.That(NightRows.NightKey("maps"), Is.EqualTo(-1));
        }

        [Test]
        public void TheDefaults_FollowTheDesign_AndTheOffSwitch()
        {
            NightRows reg = NightRows.FromDesign(D(Fixture));
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRowState s = reg.Defaults();
                Assert.That(s.On("wall46"), Is.True);
                Assert.That(s.On("pour46"), Is.False, "enabled false");
                Assert.That(s.On("grip47"), Is.True);
                Assert.That(s.On(-1), Is.False, "a row this config lacks reads off");
                Assert.That(s.On("nosuch46"), Is.False);

                NightRows.ForceOffFrom = 47; // -skygrindOff47: NIGHT46's game
                s = reg.Defaults();
                Assert.That(s.On("wall46"), Is.True, "--off47 keeps NIGHT46");
                Assert.That(s.On("grip47"), Is.False, "--off47 turns NIGHT47 off");

                foreach (int nn in new[] { 46, 45, 43, 19 }) // its own switch and every older one
                {
                    NightRows.ForceOffFrom = nn;
                    Assert.That(reg.Defaults().Any, Is.False, "--off" + nn + ": every row off");
                }

                // a replay's override wins over the design and the switch (a tape replays the run it recorded)
                NightRowState over = reg.AllOff().Set("pour46", true);
                NightRowState got = reg.Resolve(over);
                Assert.That(got.On("pour46"), Is.True);
                Assert.That(got.On("wall46"), Is.False);
                got.Set("wall46", true);
                Assert.That(over.On("wall46"), Is.False, "Resolve copies: the run never writes into the tape's rows");
                Throws<ArgumentException>(() => reg.AllOff().Set("typo46", true), "typo46", "an unknown name");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void TheOffSwitch_ParsesEveryNight()
        {
            Assert.That(NightRows.OffFrom(new[] { "-skygrindSmoke", "-skygrindOff52", "-skygrindOff47", "-logFile", "x" }, "-skygrindOff"), Is.EqualTo(47), "the smallest NN");
            Assert.That(NightRows.OffFrom(new[] { "fingerprint", "--off45" }, "--off"), Is.EqualTo(45));
            Assert.That(NightRows.OffFrom(new[] { "fingerprint" }, "--off"), Is.EqualTo(NightRows.NoOff));
            Assert.That(NightRows.OffFrom(null, "--off"), Is.EqualTo(NightRows.NoOff));
            Assert.That(NightRows.OffSwitchNight("-skygrindOff", "-skygrindOff"), Is.EqualTo(-1));
            Assert.That(NightRows.OffSwitchNight("-skygrindOffX", "-skygrindOff"), Is.EqualTo(-1));
            Assert.That(NightRows.OffSwitchNight("-skygrindOff4x", "-skygrindOff"), Is.EqualTo(-1));
            Assert.That(NightRows.OffSwitchNight("-skygrindOff26", "-skygrindOff"), Is.EqualTo(26));
            Assert.That(NightRows.OffSwitchNight("--offline", "--off"), Is.EqualTo(-1));
        }

        [Test]
        public void TheChecker_NamesEveryMalformedRow()
        {
            Assert.That(NightRows.Problems(D(Fixture)).Count, Is.EqualTo(0), string.Join("; ", NightRows.Problems(D(Fixture)).ToArray()));
            List<string> bad = NightRows.Problems(D(@"""n43"": { ""old43"": { ""sim"": true, ""enabled"": false, " + Why + @" } },
                ""n46"": { ""noname"": { ""sim"": true, ""enabled"": false, " + Why + @" }, ""noen46"": { ""sim"": true, " + Why + @" },
                           ""nowhy46"": { ""sim"": true, ""enabled"": false }, ""str46"": { ""sim"": ""yes"", ""enabled"": false, " + Why + @" } }"));
            string all = string.Join("; ", bad.ToArray());
            Assert.That(bad.Count, Is.EqualTo(5), all);
            Assert.That(all.Contains("old43"), Is.True, "a sim row before NIGHT46");
            Assert.That(all.Contains("noname"), Is.True, "a name without its night");
            Assert.That(all.Contains("noen46.enabled"), Is.True, "no enabled");
            Assert.That(all.Contains("nowhy46._why"), Is.True, "no _why");
            Assert.That(all.Contains("str46.sim"), Is.True, "sim not a bool");
            Assert.That(NightRows.FromDesign(D(@"""n46"": { ""noname"": { ""sim"": true, ""enabled"": true, " + Why + @" } }")).Count, Is.EqualTo(0), "a malformed row stays off (never a crash)");
        }

        [Test]
        public void TheRealConfig_HasNoSimRows_AndNoProblems()
        {
            GameConfig c = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            Assert.That(c.Has("design"), Is.True, "the design config loads");
            List<string> bad = NightRows.Problems(c.Obj("design"));
            Assert.That(bad.Count, Is.EqualTo(0), string.Join("; ", bad.ToArray()));
            // NIGHT46 (Tools/night/n46-shared.cjs): from NIGHT46 the real config has sim rows; NIGHT45's "no sim rows" pin is now: every row is a
            // well-formed NIGHT46+ row and its default round-trips through the tape record (no later night edits this test)
            NightRows reg = NightRows.From(c);
            foreach (NightRow r in reg.rows) Assert.That(r.night >= NightRows.FirstNight && NightRows.NightOfName(r.name) == r.night, Is.True, r.ToString());
            NightRowState d = reg.Defaults(), back = NightRowsTape.Decode(NightRowsTape.Encode(d), reg);
            for (int i = 0; i < reg.Count; i++) Assert.That(back.On(i), Is.EqualTo(d.On(i)), reg.rows[i].name);
        }

        [Test]
        public void TheTapeRecord_RoundTripsNameToBit()
        {
            NightRows reg = NightRows.FromDesign(D(Fixture));
            NightRowState s = reg.AllOff().Set("wall46", true).Set("grip47", true);
            byte[] rec = NightRowsTape.Encode(s);
            Assert.That((int)rec[0], Is.EqualTo((int)NightRowsTape.RecordVersion));
            NightRowState back = NightRowsTape.Decode(rec, reg);
            foreach (NightRow r in reg.rows) Assert.That(back.On(r.name), Is.EqualTo(s.On(r.name)), r.name);
            Assert.That(back.ToString(), Is.EqualTo("wall46, grip47"));

            // twenty rows in one night: the bits cross byte boundaries
            var sb = new System.Text.StringBuilder(@"""n48"": {");
            for (int i = 0; i < 20; i++) sb.Append((i > 0 ? ", " : "") + @"""r" + (char)('a' + i) + @"48"": { ""sim"": true, ""enabled"": " + (i % 3 == 0 ? "true" : "false") + ", " + Why + " }");
            sb.Append("}");
            NightRows big = NightRows.FromDesign(D(sb.ToString()));
            Assert.That(big.Count, Is.EqualTo(20));
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRowState d = big.Defaults();
                NightRowState d2 = NightRowsTape.Decode(NightRowsTape.Encode(d), big);
                for (int i = 0; i < 20; i++) Assert.That(d2.On(i), Is.EqualTo(i % 3 == 0), "row " + i);
            }
            finally { NightRows.ForceOffFrom = was; }

            Assert.That(NightRowsTape.Decode(NightRowsTape.Encode(NightRows.Empty.AllOff()), reg).Any, Is.False, "an empty record");
        }

        [Test]
        public void AnOldTape_ReplaysTheNewRowsOff()
        {
            NightRows then = NightRows.FromDesign(D(@"""n46"": { ""wall46"": { ""sim"": true, ""enabled"": true, " + Why + @" }, ""pour46"": { ""sim"": true, ""enabled"": false, " + Why + @" } }"));
            byte[] rec = NightRowsTape.Encode(then.AllOff().Set("wall46", true));
            // the build now has a NIGHT47 row ON in design and a late NIGHT46 row: the tape never ran them
            NightRows now = NightRows.FromDesign(D(@"""n46"": { ""wall46"": { ""sim"": true, ""enabled"": true, " + Why + @" }, ""pour46"": { ""sim"": true, ""enabled"": false, " + Why + @" }, ""late46"": { ""sim"": true, ""enabled"": true, " + Why + @" } },
                ""n47"": { ""grip47"": { ""sim"": true, ""enabled"": true, " + Why + @" } }"));
            NightRowState r = NightRowsTape.Decode(rec, now);
            Assert.That(r.On("wall46"), Is.True, "the recorded row as recorded");
            Assert.That(r.On("pour46"), Is.False);
            Assert.That(r.On("late46"), Is.False, "a row the record does not name replays off");
            Assert.That(r.On("grip47"), Is.False, "a later night's row replays off");

            Assert.That(NightRowsTape.ForTape(26, rec, now).Any, Is.False, "a format 26 tape has no record: every row off");
            Assert.That(NightRowsTape.ForTape(NightRowsTape.FirstTapeFormat, rec, now).On("wall46"), Is.True);
            Assert.That(NightRowsTape.Decode(null, now).Any, Is.False, "no record: every row off");
        }

        [Test]
        public void ARowTheBuildLacks_IsRefusedOnlyWhenItRan()
        {
            NightRows then = NightRows.FromDesign(D(@"""n46"": { ""wall46"": { ""sim"": true, ""enabled"": true, " + Why + @" }, ""pour46"": { ""sim"": true, ""enabled"": false, " + Why + @" } }"));
            NightRows now = NightRows.FromDesign(D(@"""n46"": { ""wall46"": { ""sim"": true, ""enabled"": true, " + Why + @" } }"));
            NightRowState ok = NightRowsTape.Decode(NightRowsTape.Encode(then.AllOff().Set("wall46", true)), now);
            Assert.That(ok.On("wall46"), Is.True, "pour46 ran off: the replay is the run");
            Throws<NightRowsFormatException>(() => NightRowsTape.Decode(NightRowsTape.Encode(then.AllOff().Set("pour46", true)), now), "pour46", "a row that ran ON and is gone");
        }

        [Test]
        public void ABadRecord_IsRefused()
        {
            NightRows reg = NightRows.FromDesign(D(Fixture));
            byte[] rec = NightRowsTape.Encode(reg.AllOff().Set("wall46", true));
            byte[] v2 = (byte[])rec.Clone();
            v2[0] = 2;
            Throws<NightRowsFormatException>(() => NightRowsTape.Decode(v2, reg), "version 2", "another record version");
            for (int cut = 1; cut < rec.Length; cut++)
            {
                var part = new byte[cut];
                Array.Copy(rec, part, cut);
                Throws<NightRowsFormatException>(() => NightRowsTape.Decode(part, reg), null, "truncated at " + cut);
            }
        }

        [Test]
        public void Through_TurnsLaterNightsOff()
        {
            NightRows reg = NightRows.FromDesign(D(Fixture));
            NightRowState s = reg.AllOff().Set("wall46", true).Set("grip47", true);
            Assert.That(s.Through(45).Any, Is.False, "nothing recorded yet: the composed fingerprint runs every row off");
            Assert.That(s.Through(46).On("wall46"), Is.True);
            Assert.That(s.Through(46).On("grip47"), Is.False);
            Assert.That(s.Through(47).On("grip47"), Is.True);
            Assert.That(s.On("grip47"), Is.True, "Through copies");
        }

        [Test]
        public void LevelAndViewRows_FollowTheSameSwitch()
        {
            GameConfig c = WithDesign(@"""n45"": { ""kiln45"": { ""enabled"": true, ""levelFrom"": ""n43-kiln"", ""levelTo"": ""n45-kiln"" } },
                ""n46"": { ""yard46"": { ""enabled"": true, ""levelFrom"": ""n45-foundry"", ""levelTo"": ""n46-foundry"" }, ""mix46"": { ""enabled"": false, ""levelFrom"": ""n43-iceline"", ""levelTo"": ""n46-iceline"" }, ""look46"": { ""enabled"": true } },
                ""n47"": { ""yard47"": { ""enabled"": true, ""levelFrom"": ""n46-foundry"", ""levelTo"": ""n47-foundry"" } }");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                var order = new List<string> { "n45-foundry", "n43-iceline", "n43-kiln" };
                NightRows.SwapLevels(c, order);
                Assert.That(order[0], Is.EqualTo("n47-foundry"), "n46 swaps n45 -> n46, then n47 swaps n46 -> n47 (nights in order)");
                Assert.That(order[1], Is.EqualTo("n43-iceline"), "mix46 off");
                Assert.That(order[2], Is.EqualTo("n43-kiln"), "NIGHT45's rows are N45Levels', not the registry's");
                Assert.That(NightRows.BaseId(c, "n47-foundry"), Is.EqualTo("n45-foundry"), "n47 -> n46 -> n45 (then N45Levels.BaseId continues)");
                Assert.That(NightRows.BaseId(c, "n46-iceline"), Is.EqualTo("n43-iceline"), "unconditional, as N45Levels.BaseId");
                Assert.That(NightRows.BaseId(c, "n43-kiln"), Is.EqualTo("n43-kiln"));
                Assert.That(NightRows.ViewOn(c, "look46"), Is.True);
                Assert.That(NightRows.ViewOn(c, "look45"), Is.False, "before NIGHT46: N45View's");

                NightRows.ForceOffFrom = 47;
                order = new List<string> { "n45-foundry" };
                NightRows.SwapLevels(c, order);
                Assert.That(order[0], Is.EqualTo("n46-foundry"), "--off47: NIGHT46's level");
                Assert.That(NightRows.ViewOn(c, "look46"), Is.True);

                NightRows.ForceOffFrom = 45;
                order = new List<string> { "n45-foundry" };
                NightRows.SwapLevels(c, order);
                Assert.That(order[0], Is.EqualTo("n45-foundry"), "--off45: no NIGHT46+ level");
                Assert.That(NightRows.ViewOn(c, "look46"), Is.False);
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
