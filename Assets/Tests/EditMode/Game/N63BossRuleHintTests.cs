// NIGHT63 builder G (P2b, the P2 critic's problem 5, Logs/n63/verdict-P2.md 6c: on THE ICEBREAKER's 1.0 s frame,
// Logs/feel/n63-b1/b61i/boss61/shot-N30-B61-brawl40-1000.png, the boss bar's hint "PARRY THE BLOCK. PUNISH STUCK FISTS" stands above the rule
// line: two instructions at once). The bar's hint is bossbar43's "Hint" label: hud47 hides it when the bar is built (UiHud47.Boss.cs Bar47Build),
// but Text33.Set shows its holder again on every new text (UiStyle33.cs:78), so it has been drawn since (NIGHT62's b61k telemetry: "PUNCH THE
// HEAD TO STAGGER IT" on the wyrm's bar). Pinned (BossRule61.HintShown, the row design.n61.bossRule61 as P2):
//   1. while the rule line holds the place (from the rise to the line's end): the bar's hint is not drawn, whatever its text
//   2. when the line does not hold it: drawn exactly when it has text (Text33.Set's own rule: NIGHT62's bar, unchanged outside the hold)
// Written test-first: RED on its own commit (the placeholder is NIGHT62's rule: drawn whenever it has text). Not run in Unity by its author.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N63BossRuleHintTests
    {
        [Test]
        public void WhileTheRuleLineHolds_TheBarsHintIsNotDrawn()
        {
            Assert.IsFalse(BossRule61.HintShown(true, "PARRY THE BLOCK. PUNISH STUCK FISTS"), "THE ICEBREAKER's 1.0 s frame: one instruction, the rule line");
            Assert.IsFalse(BossRule61.HintShown(true, "PUNCH THE HEAD TO STAGGER IT"), "THE KILN WYRM once its bar is up during the line");
            Assert.IsFalse(BossRule61.HintShown(true, ""), "no text");
            Assert.IsFalse(BossRule61.HintShown(true, null), "no text");
        }

        [Test]
        public void OutsideTheHold_TheBarsHintIsDrawnWhenItHasText()
        {
            Assert.IsTrue(BossRule61.HintShown(false, "PARRY THE BLOCK. PUNISH STUCK FISTS"), "after the line: the bar as before");
            Assert.IsFalse(BossRule61.HintShown(false, ""), "an empty hint hides its holder (Text33.Set)");
            Assert.IsFalse(BossRule61.HintShown(false, null));
        }
    }
}
