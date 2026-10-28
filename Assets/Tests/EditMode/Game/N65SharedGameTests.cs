// NIGHT65 (the lead, A1; the shared commit): look52's tiers key on a map's level id (Look52Tier.On: UnityTuning.Look52TierLevels), so level 1
// rebuilt as n65-foundry (design.n65.foundry65) keeps the tiers n52-foundry has. Pinned: n65-foundry is a tier level exactly when n52-foundry is.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N65SharedGameTests
    {
        [Test]
        public void Look52Tiers_ApplyOnN65Foundry_AsOnN52Foundry()
        {
            bool n52 = System.Array.IndexOf(UnityTuning.Look52TierLevels, "n52-foundry") >= 0;
            bool n65 = System.Array.IndexOf(UnityTuning.Look52TierLevels, "n65-foundry") >= 0;
            Assert.That(n52, Is.True, "n52-foundry is a tier level (NIGHT52's round 11)");
            Assert.That(n65, Is.EqualTo(n52), "n65-foundry is a tier level as n52-foundry is");
        }
    }
}
