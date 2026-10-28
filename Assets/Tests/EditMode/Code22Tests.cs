// NIGHT22 U3-36.8 (builder W2): the CHALLENGE CODE's reader and the TITLE field's model (Core/Challenge21.cs). The same assertions run
// headless in SimHarness `code2 --selftest` (Tools/SimHarness/NunitMini.cs: Assert.That with Is.True / Is.False / Is.EqualTo only).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Code22Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static RunSetup SetupWith(params string[] ids)
        {
            var s = new RunSetup();
            s.mutators.AddRange(ids);
            return s;
        }

        [Test]
        public void ReaderAddsTheSetupsMutatorsToTheRun()
        {
            var into = new List<string>();
            int n = Challenge21.ReadSetupMutators(Cfg, SetupWith("speedDemon", "glassCannon"), into);
            Assert.That(n == 2 && into.Contains("speedDemon") && into.Contains("glassCannon"), Is.True);
        }

        [Test]
        public void ReaderChangesNothingWithoutAPick()
        {
            var into = new List<string> { "noRegen" };
            Assert.That(Challenge21.ReadSetupMutators(Cfg, SetupWith(), into) == 0 && into.Count == 1, Is.True);
        }

        [Test]
        public void ReaderSkipsRepeatsAndWhatThePortCannotPlay()
        {
            var into = new List<string> { "glassCannon" };
            int n = Challenge21.ReadSetupMutators(Cfg, SetupWith("glassCannon", "lowGravity", "nonsense"), into);
            Assert.That(n == 0 && into.Count == 1, Is.True);
        }

        [Test]
        public void ReaderIsOffWithTheForceOffSwitch()
        {
            bool was = N21Blocks.ForceOff;
            N21Blocks.ForceOff = true;
            try
            {
                var into = new List<string>();
                Assert.That(Challenge21.ReadSetupMutators(Cfg, SetupWith("speedDemon"), into) == 0 && into.Count == 0, Is.True);
                Assert.That(Challenge21.ResultsLine(Cfg, SetupWith(), 5u, 0) == null, Is.True);
            }
            finally { N21Blocks.ForceOff = was; }
        }

        [Test]
        public void ResultsLineIsTheRunsCode()
        {
            RunSetup s = SetupWith("noRegen");
            s.heat = 1;
            string line = Challenge21.ResultsLine(Cfg, s, 777u, 0);
            Assert.That(line != null && line.StartsWith("CODE "), Is.True);
            Assert.That(Challenge21.TryDecode(line.Substring(5), Cfg, out Challenge21.Spec d) && d.seed == 777u && d.heat == 1 && d.mutators.Contains("noRegen"), Is.True);
        }

        [Test]
        public void PadFillsACodeWithUpDownAndRight()
        {
            string code = Challenge21.Encode(new Challenge21.Spec { seed = 99, heat = 1, mode = RunSetup.Survival, target = 3, mutators = new List<string> { "speedDemon" } }, Cfg);
            string want = code.Replace("-", "");
            var e = new Challenge21.Entry(Cfg);
            for (int slot = 0; slot < e.Length; slot++)
            {
                int guard = 0;
                while (e.Text()[slot + slot / 4] != want[slot] && guard++ < 40) e.Cycle(+1);
                if (slot < e.Length - 1) e.Move(+1);
            }
            Assert.That(e.Text() == code, Is.True, e.Text());
            Assert.That(e.TryResult(Cfg, out Challenge21.Spec s) && s.seed == 99, Is.True);
        }

        [Test]
        public void DownWrapsFromBlankToTheLastCharacter()
        {
            var e = new Challenge21.Entry(Cfg);
            e.Cycle(-1);
            Assert.That(e.Text()[0] == 'Z', Is.True, e.Text());
            e.Cycle(+1);
            Assert.That(e.Text()[0] == '_', Is.True, e.Text());
        }

        [Test]
        public void TypedLookAlikesReadAndUnknownCharactersAreRefused()
        {
            var e = new Challenge21.Entry(Cfg);
            Assert.That(e.Type('o') && e.Type('l') && e.Type('-') && !e.Type('U'), Is.True);
            Assert.That(e.Text().StartsWith("01"), Is.True, e.Text());
        }


    }
}
