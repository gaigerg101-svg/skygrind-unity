// NIGHT9 U3-07: the oracle, whole. Every file in PortExport/vectors/ is checked by its Skygrind.Core Golden checker
// (the same code the built Player runs under -skygrindGolden), and a vector file with no checker fails the build of
// trust: only V8 makes vectors (port-export-p18), and every set it makes is consumed here.
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class OracleGoldenTests
    {
        static GoldenData data;

        public static GoldenData Data => data ??= new GoldenData(Port.Root);

        static IEnumerable<string> Sets() => Data.VectorSets();

        [Test, Description("every file in PortExport/vectors/ has a Golden checker (an orphan set fails)")]
        public void Golden_EveryVectorFileHasAChecker()
        {
            List<string> orphans = Golden.Orphans(Data);
            TestContext.WriteLine($"{Data.VectorSets().Length} vector files, {Golden.Sets.Count} checkers; orphans: [{string.Join(", ", orphans)}]");
            Assert.IsEmpty(orphans, "vector files with no consuming checker: " + string.Join(", ", orphans));
            var stale = Golden.Sets.Keys.Where(k => !Data.VectorSets().Contains(k)).ToList();
            Assert.IsEmpty(stale, "checkers with no vector file: " + string.Join(", ", stale));
        }

        [TestCaseSource(nameof(Sets)), Description("every record of the vector set reproduced bit for bit by the C# port (Skygrind.Core Golden)")]
        public void Golden_Set(string set)
        {
            GoldenResult r = Golden.Check(Data, set);
            TestContext.WriteLine(r.Summary);
            Assert.IsTrue(r.Ok, r.Summary + "\n" + string.Join("\n", r.mismatches.Take(40)));
        }
    }
}
