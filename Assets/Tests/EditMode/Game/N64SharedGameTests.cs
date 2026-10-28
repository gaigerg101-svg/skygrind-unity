// NIGHT64 (the lead, A1; the shared commit): the same-run toggle claims the night's three rows (SameRun63Math.FrameRows, A1's one edit of that
// line so no two builders edit it: Research/2026-10-23-forge64/redteam.md 11). Pinned:
//   1. goal64, order64 and fightLine64 are Frame rows (their views re-read the row every frame, by their builders' contract), so a same-run
//      pair's OFF half shows the row off in the held frame
//   2. the option names each by its night: `-skygrindSameRun63 n64.goal64` is goal64; `n63.goal64` is refused (the night is not the row's own)
using NUnit.Framework;
using S = Skygrind.Game.SameRun63Math;

namespace Skygrind.Game.Tests
{
    public class N64SharedGameTests
    {
        static readonly string[] Rows = { "goal64", "order64", "fightLine64" };

        [Test]
        public void TheNightsThreeRows_AreFrameRows()
        {
            foreach (string r in Rows) Assert.That(S.KindOf(r), Is.EqualTo(S.Kind.Frame), r);
        }

        [Test]
        public void Option_NamesEachRowByItsNight()
        {
            foreach (string r in Rows)
            {
                Assert.That(S.Row(new[] { "x.exe", "-skygrindSameRun63", "n64." + r }, out string why), Is.EqualTo(r), r);
                Assert.That(why, Is.Null, r);
            }
            Assert.That(S.Row(new[] { "-skygrindSameRun63", "n63.goal64" }, out string bad), Is.Null);
            Assert.That(bad, Is.Not.Null, "another night than the row's own is refused with a reason");
        }
    }
}
