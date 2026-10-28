// NIGHT63 builder V (leftover 4: R1's N3; NIGHT61's review Logs/n61/review-n61.md N3): PlayerChecks.FocusAutomation61Modes was a public mutable
// array (any code could overwrite an element and change what pauses on a focus loss; the list is MayPauseOnFocusLoss's whole definition). Pinned:
//   1. it is an IReadOnlyList<string> field, readonly, and not an array underneath (no caller can write an element)
//   2. its contents and order unchanged (the eleven modes after the focus probe's, in Attach's order)
// No behaviour change: MayPauseOnFocusLoss reads it with foreach (FocusAutomation61Tests unchanged). Written test first (RED on its commit: the
// field is still a string[]); the Editor run is the lead's.
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N63ModeListTests
    {
        [Test]
        public void TheModeList_IsReadOnly()
        {
            FieldInfo f = typeof(PlayerChecks).GetField("FocusAutomation61Modes", BindingFlags.Public | BindingFlags.Static);
            Assert.That(f, Is.Not.Null);
            Assert.That(f.IsInitOnly, Is.True, "readonly");
            Assert.That(f.FieldType, Is.EqualTo(typeof(IReadOnlyList<string>)), "exposed as IReadOnlyList<string>");
            object v = f.GetValue(null);
            Assert.That(v is string[], Is.False, "not an array a caller can write through a cast");
        }

        [Test]
        public void TheModeList_Unchanged()
        {
            IReadOnlyList<string> m = PlayerChecks.FocusAutomation61Modes;
            Assert.That(m, Is.EqualTo(new[] { "-skygrindSmoke", "-skygrindShots", "-skygrindReel", "-skygrindTile", "-skygrindStrip", "-skygrindHitch", "-skygrindGolden", "-skygrindFingerprint", "-skygrindFarm", "-skygrindReplay", "-skygrindBench" }));
        }
    }
}
