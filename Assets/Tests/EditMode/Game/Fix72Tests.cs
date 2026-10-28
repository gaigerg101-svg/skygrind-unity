// NIGHT72 builder L (L2; Game/Fix72.cs): pinned: the flicker diagnostic switch (-skygrindFlickerOff72=a+b, also the next-argument form and commas) keeps only the
// names it knows, lower-cased, once each; no switch or an unknown name leaves the list empty (so a run with none changes nothing).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Fix72Tests
    {
        [TearDown]
        public void TearDown() => Fix72.Reset(new string[0]);

        [Test]
        public void Parse_KnownNamesOnly()
        {
            Assert.AreEqual(new[] { "grain", "bloom" }, Fix72.Parse(new[] { "-skygrindFlickerOff72=Grain+bloom+nonsense+grain" }));
            Assert.AreEqual(new[] { "quant", "scan" }, Fix72.Parse(new[] { "-skygrindFlickerOff72", "quant,scan" }));
            Assert.AreEqual(new string[0], Fix72.Parse(new[] { "-skygrindFlickerOff72=nonsense" }));
            Assert.AreEqual(new string[0], Fix72.Parse(new[] { "-skygrindRows72", "fix72:1" }), "the row switch is not the diagnostic");
            Assert.AreEqual(new string[0], Fix72.Parse(null));
        }

        [Test]
        public void Off_ReadsTheParsedList()
        {
            Fix72.Reset(new[] { "-skygrindFlickerOff72=crtgrain+ssao" });
            Assert.IsTrue(Fix72.Off("crtgrain"));
            Assert.IsTrue(Fix72.Off("ssao"));
            Assert.IsFalse(Fix72.Off("grain"));
            Fix72.Reset(new string[0]);
            Assert.IsFalse(Fix72.Off("crtgrain"));
        }

        [Test]
        public void EverySourceIsNamedOnce()
        {
            var seen = new System.Collections.Generic.HashSet<string>(Fix72.Sources);
            Assert.AreEqual(Fix72.Sources.Length, seen.Count);
            Assert.AreEqual(new[] { "grain", "crtgrain", "bloom", "ssao", "quant", "scan", "chroma" }, Fix72.Sources);
        }
    }
}
