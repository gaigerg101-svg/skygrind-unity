// NIGHT13 U3-26.12: the run log's pure parts (RunLogMath, JsonLine: file names, which old files go, automation, the JSON
// lines and the settings summary) and COPY REPORT's text helpers (ReportText: the log's tail, the user folder scrubbed),
// and the version stamp's line (BuildStamp). The file writes and the clipboard need the Player (the smoke checks them).
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class RunLogTests
    {
        [Test]
        public void FileNameIsUtcStartAndSeed()
        {
            Assert.AreEqual("run-20261004-031507-12345.jsonl", RunLogMath.FileName(new DateTime(2026, 10, 4, 3, 15, 7, DateTimeKind.Utc), 12345));
            Assert.AreEqual("run-20270101-000000-4294967295.jsonl", RunLogMath.FileName(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), uint.MaxValue));
        }

        [Test]
        public void PruneKeepsTheNewestAndIgnoresOtherFiles()
        {
            var names = new List<string>();
            for (int i = 0; i < 23; i++) names.Add(RunLogMath.FileName(new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(i), (uint)(100 - i)));
            names.Reverse(); // the order on disk doesn't matter
            names.Add("notes.txt");
            names.Add("run-old.json");
            List<string> gone = RunLogMath.ToPrune(names, 20);
            Assert.AreEqual(3, gone.Count);
            CollectionAssert.AreEquivalent(new[]
            {
                RunLogMath.FileName(new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), 100),
                RunLogMath.FileName(new DateTime(2026, 10, 1, 0, 1, 0, DateTimeKind.Utc), 99),
                RunLogMath.FileName(new DateTime(2026, 10, 1, 0, 2, 0, DateTimeKind.Utc), 98),
            }, gone);
            Assert.AreEqual(0, RunLogMath.ToPrune(names.GetRange(0, 5), 20).Count, "fewer files than kept: none go");
            Assert.AreEqual(0, RunLogMath.ToPrune(new List<string>(), 20).Count);
        }

        [Test]
        public void PruneTakesPathsAndKeepsThemWhole()
        {
            var paths = new List<string>
            {
                @"C:\data\runs\run-20261001-000000-1.jsonl",
                @"C:\data\runs\run-20261002-000000-1.jsonl",
                @"C:\data\runs\run-20261003-000000-1.jsonl",
            };
            CollectionAssert.AreEqual(new[] { @"C:\data\runs\run-20261001-000000-1.jsonl" }, RunLogMath.ToPrune(paths, 2));
        }

        [Test]
        public void AutomationIsAPlayerCheckFlag()
        {
            Assert.IsTrue(RunLogMath.IsAutomation(new[] { "SKYGRIND.exe", "-skygrindSmoke", "-logFile", "s.log" }));
            Assert.IsTrue(RunLogMath.IsAutomation(new[] { "SKYGRIND.exe", "-skygrindBench", "60", "play" }));
            Assert.IsTrue(RunLogMath.IsAutomation(new[] { "SKYGRIND.exe", "-skygrindFingerprint" }));
            Assert.IsTrue(RunLogMath.IsAutomation(new[] { "SKYGRIND.exe", "-skygrindGolden" }));
            Assert.IsFalse(RunLogMath.IsAutomation(new[] { "SKYGRIND.exe" }));
            Assert.IsFalse(RunLogMath.IsAutomation(new[] { "SKYGRIND.exe", "-skygrindDev" }), "the dev keys are not automation");
            Assert.IsFalse(RunLogMath.IsAutomation(null));
        }

        [Test]
        public void JsonStringsAreEscaped()
        {
            var sb = new System.Text.StringBuilder();
            RunLogMath.AppendString(sb, "a\"b\\c\nd\te\u0001 · ROUND");
            Assert.AreEqual("\"a\\\"b\\\\c\\nd\\te\\u0001 · ROUND\"", sb.ToString());
            sb.Clear();
            RunLogMath.AppendString(sb, null);
            Assert.AreEqual("null", sb.ToString());
        }

        [Test]
        public void JsonNumbersAreInvariantAndFinite()
        {
            var sb = new System.Text.StringBuilder();
            void Is(string expected, double v)
            {
                sb.Clear();
                RunLogMath.AppendNumber(sb, v);
                Assert.AreEqual(expected, sb.ToString(), v.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }
            Is("0", 0);
            Is("1.5", 1.5);
            Is("-0.25", -0.25);
            Is("12.346", 12.3456);
            Is("1000000000", 1e9);
            Is("null", double.NaN);
            Is("null", double.PositiveInfinity);
            var was = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE"); // a comma culture
                Is("1.5", 1.5);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = was;
            }
        }

        [Test]
        public void ALineIsOneJsonObject()
        {
            string line = new JsonLine().Begin(12.5, "kill").Add("weapon", "kestrel").Add("kind", (string)null).Add("head", true).Add("round", 3).Add("price", 1250.0).End();
            Assert.AreEqual("{\"t\":12.5,\"e\":\"kill\",\"weapon\":\"kestrel\",\"kind\":null,\"head\":true,\"round\":3,\"price\":1250}", line);
            var o = (Dictionary<string, object>)JsJson.Parse(line);
            Assert.AreEqual("kill", o["e"]);
            Assert.AreEqual(12.5, (double)o["t"]);
            // A JsonLine is reused: Begin starts a fresh line.
            var j = new JsonLine();
            j.Begin(0, "roundBegin").Add("round", 1).End();
            Assert.AreEqual("{\"t\":0,\"e\":\"roundBegin\",\"round\":2}", j.Begin(0, "roundBegin").Add("round", 2).End());
            string list = new JsonLine().Begin(0, "header").Add("mutators", new List<string> { "lowGravity", "a\"b" }).Add("none", (IList<string>)null).End();
            Assert.AreEqual("{\"t\":0,\"e\":\"header\",\"mutators\":[\"lowGravity\",\"a\\\"b\"],\"none\":[]}", list);
            JsJson.Parse(list);
        }

        static SettingItem Item(string key) => new SettingItem { key = key, tab = "aim", label = key.ToUpperInvariant(), type = "range" };

        [Test]
        public void SettingsSummaryListsOnlyTheChanged()
        {
            var items = new List<SettingItem> { Item("sensitivity"), Item("fovDeg"), Item("invertY"), Item("aimAssist") };
            var defaults = new Dictionary<string, object> { { "sensitivity", 0.0014 }, { "fovDeg", 90.0 }, { "invertY", false }, { "aimAssist", "snap" } };
            var values = new Dictionary<string, object>(defaults);
            Assert.AreEqual("all default", RunLogMath.SettingsSummary(items, values, defaults));
            values["fovDeg"] = 100.0;
            values["invertY"] = true;
            values["sensitivity"] = 0.0018;
            Assert.AreEqual("sensitivity=0.0018, fovDeg=100, invertY=true", RunLogMath.SettingsSummary(items, values, defaults));
            values.Remove("aimAssist"); // a row with no value (or no default) is skipped, never thrown
            Assert.AreEqual("sensitivity=0.0018, fovDeg=100, invertY=true", RunLogMath.SettingsSummary(items, values, defaults));
        }

        [Test]
        public void TailKeepsTheLastLines()
        {
            Assert.AreEqual("b\nc", ReportText.Tail("a\nb\nc\n", 2));
            Assert.AreEqual("b\nc", ReportText.Tail("a\r\nb\r\nc", 2));
            Assert.AreEqual("a\nb\nc", ReportText.Tail("a\nb\nc", 50));
            Assert.AreEqual("", ReportText.Tail("", 50));
            Assert.AreEqual("", ReportText.Tail(null, 50));
        }

        [Test]
        public void ScrubHidesTheUserFolder()
        {
            const string home = @"C:\Users\gaige";
            Assert.AreEqual(@"%USERPROFILE%\AppData\LocalLow\x\Player.log", ReportText.Scrub(@"C:\Users\gaige\AppData\LocalLow\x\Player.log", home));
            Assert.AreEqual("%USERPROFILE%/AppData/LocalLow", ReportText.Scrub("C:/Users/gaige/AppData/LocalLow", home), "Unity writes forward slashes");
            Assert.AreEqual(@"%USERPROFILE%\dev", ReportText.Scrub(@"c:\users\GAIGE\dev", home), "Windows paths ignore case");
            Assert.AreEqual("no paths here", ReportText.Scrub("no paths here", home));
            Assert.AreEqual(@"C:\Users\gaige\x", ReportText.Scrub(@"C:\Users\gaige\x", null), "no home folder: unchanged");
        }

        [Test]
        public void StampLineAndHashFile()
        {
            Assert.AreEqual("SKYGRIND 0.1.0 (abc1234)", BuildStamp.Format("0.1.0", "abc1234"));
            Assert.AreEqual("SKYGRIND 0.1.0 (editor)", BuildStamp.Format("0.1.0", "editor"));
            Assert.AreEqual("abc1234-dirty", BuildStamp.ParseHash("abc1234-dirty\r\n"));
            Assert.AreEqual("abc1234", BuildStamp.ParseHash("\n  abc1234  \nextra"));
            Assert.AreEqual("unknown", BuildStamp.ParseHash(""));
            Assert.AreEqual("unknown", BuildStamp.ParseHash(null));
        }
    }
}
