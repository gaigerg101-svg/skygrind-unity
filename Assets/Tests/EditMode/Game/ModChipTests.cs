// NIGHT16 polish #60 (builder U): the owned mods' chips in the perk row (hud.js :139, :848-855): the 3-letter label with
// its stack, and the colour rule 8 allows (red-orange is danger only, magenta RIVAL only). Pure: ModChip (UiPerks.cs).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class ModChipTests
    {
        [Test]
        public void AChipIsTheNamesFirstThreeLettersAndItsStack()
        {
            // hud.js :855: d.name.slice(0, 3) + (k > 1 ? `×${k}` : '')
            Assert.AreEqual("VAM", ModChip.Label("VAMPIRE", 1));
            Assert.AreEqual("VAM×2", ModChip.Label("VAMPIRE", 2));
            Assert.AreEqual("GO", ModChip.Label("GO", 1), "a short name stays whole");
        }

        [Test]
        public void RedOrangeAndMagentaChipsFallBackToTheTextColour()
        {
            Assert.IsTrue(ModChip.Rule8Ok(0x19D3E6), "cyan (kinetic, AIR / WALL / SLIDE)");
            Assert.IsTrue(ModChip.Rule8Ok(0xFFE04A), "yellow (blast)");
            Assert.IsTrue(ModChip.Rule8Ok(0x3DDC84), "green (HEALTH)");
            Assert.IsTrue(ModChip.Rule8Ok(0x8F5BFF), "violet (gravity)");
            Assert.IsTrue(ModChip.Rule8Ok(0xFFC13D), "amber (STYLE)");
            Assert.IsFalse(ModChip.Rule8Ok(0xFF491C), "colors.enemy itself");
            Assert.IsFalse(ModChip.Rule8Ok(0xFF8C1A), "fire: orange at hue 30, reads as the danger colour");
            Assert.IsFalse(ModChip.Rule8Ok(0xFF2E88), "magenta (RIVAL)");
            Assert.IsTrue(ModChip.Rule8Ok(0xEAF8FF), "pale (pierce): too grey to read as either");
        }
    }
}
