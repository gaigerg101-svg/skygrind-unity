// NIGHT74 builder L (R's finding: Decor54.DecorKind recoloured every cyan "Props flat floorStrip" renderer to the level accent, so the player-usable floor marks, THE MARK a2-P1, THE RIDE'S END a3-RE and
// THE SLIDE LINE a5-P2, drew dark purple). The fix: PropsView29Math.FlatKey names a floorStrip row that has a job and no `decor: true` "flat floorMark", which Decor54.DecorKind does not match. Pinned, both cases:
//   1. DECOR keeps its name and its recolour: a floorStrip with no job (the n29 - n33 plans' strips, any colour), or with `decor: true` (even with a job), is "flat floorStrip..." and DecorKind says "prop"
//   2. A USABLE MARK (kind floorStrip, a job, no decor flag) is "flat floorMark..." and DecorKind says null: never moved; padMarker and frost keep their keys and DecorKind's null as before
//   3. nothing else moves: the pillar collar and the console panel are still "prop", the roof and door lines still "roof" / "door"
//   4. the data: every floorStrip row of the n29 foundry plan (8: 3 cyan, 3 amber, 2 white; all decor), and of every plan file under Design/levels/n*/ (79 in 22 files), is not a mark; every floorStrip row of the n65 room files a2 / a3 / a5 is
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class Decor74MarkTests
    {
        static Dictionary<string, object> Row(string kind, string job = null, bool? decor = null, string colour = null)
        {
            var r = new Dictionary<string, object> { ["kind"] = kind, ["collider"] = false };
            if (job != null) r["job"] = job;
            if (decor.HasValue) r["decor"] = decor.Value;
            if (colour != null) r["color"] = colour;
            return r;
        }

        [Test]
        public void DecorFloorStrips_KeepTheirNameAndTheirRecolour()
        {
            foreach (string colour in new string[] { null, "cyan", "white", "amber", "green", "orange" })
            {
                var plain = Row("floorStrip", null, null, colour);
                Assert.IsFalse(PropsView29Math.IsUsableMark("floorStrip", plain), "no job: decor (" + colour + ")");
                string key = PropsView29Math.FlatKey("floorStrip", plain, colour);
                Assert.AreEqual("flat floorStrip" + (colour ?? ""), key);
                Assert.AreEqual("prop", Decor54.DecorKind("Props " + key), "recoloured as before (" + colour + ")");
            }
            var decorWithJob = Row("floorStrip", "line", true);
            Assert.IsFalse(PropsView29Math.IsUsableMark("floorStrip", decorWithJob), "decor: true wins over a job");
            Assert.AreEqual("prop", Decor54.DecorKind("Props " + PropsView29Math.FlatKey("floorStrip", decorWithJob, null)));
            var emptyJob = Row("floorStrip", "");
            Assert.IsFalse(PropsView29Math.IsUsableMark("floorStrip", emptyJob), "an empty job names nothing");
            Assert.IsFalse(PropsView29Math.IsUsableMark("floorStrip", null));
            Assert.AreEqual("flat floorStrip", PropsView29Math.FlatKey("floorStrip", null, null), "no row: the old key");
        }

        [Test]
        public void UsableMarks_AreNamedApart_AndNeverRecoloured()
        {
            var mark = Row("floorStrip", "line");
            Assert.IsTrue(PropsView29Math.IsUsableMark("floorStrip", mark));
            Assert.AreEqual("flat floorMark", PropsView29Math.FlatKey("floorStrip", mark, null));
            Assert.IsNull(Decor54.DecorKind("Props flat floorMark"), "THE MARK stays the player's cyan");
            Assert.IsNull(Decor54.DecorKind("Props flat floorMarkwhite"), "whatever its colour field");
            Assert.AreEqual("flat floorMarkamber", PropsView29Math.FlatKey("floorStrip", Row("floorStrip", "marker", false, "amber"), "amber"));
            // the other flat kinds keep their keys, with or without a job
            Assert.IsFalse(PropsView29Math.IsUsableMark("padMarker", Row("padMarker", "line")));
            Assert.AreEqual("flat padMarker", PropsView29Math.FlatKey("padMarker", Row("padMarker", "line"), null));
            Assert.AreEqual("flat frost", PropsView29Math.FlatKey("frost", Row("frost", "line"), null));
            Assert.IsNull(Decor54.DecorKind("Props flat padMarker"));
            Assert.IsNull(Decor54.DecorKind("Props flat frost"));
        }

        [Test]
        public void TheOtherDecorKinds_AreUntouched()
        {
            Assert.AreEqual("prop", Decor54.DecorKind("Props pillar collar"));
            Assert.AreEqual("prop", Decor54.DecorKind("Props console panel"));
            Assert.AreEqual("roof", Decor54.DecorKind("Strips boundary"));
            Assert.AreEqual("roof", Decor54.DecorKind("Strips cyan"));
            Assert.AreEqual("door", Decor54.DecorKind("Door frame neon 29"));
            Assert.IsNull(Decor54.DecorKind("Props stone"));
            Assert.IsNull(Decor54.DecorKind("Stone"));
        }

        /// <summary>The project root, from this source file's own path (Assets/Tests/EditMode/Game/Decor74MarkTests.cs: four folders up): no Application, so the logic also runs in a console.</summary>
        static string Root([CallerFilePath] string here = "") => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here), "..", "..", "..", ".."));

        static List<Dictionary<string, object>> FloorStrips(string relPath)
        {
            string path = Path.GetFullPath(Path.Combine(Root(), relPath));
            var d = (Dictionary<string, object>)JsJson.Parse(File.ReadAllText(path));
            object props;
            if (!d.TryGetValue("props", out props) && d.TryGetValue("campaign", out object camp) && camp is Dictionary<string, object> cd) cd.TryGetValue("props", out props);
            var res = new List<Dictionary<string, object>>();
            if (props is List<object> list)
                foreach (object o in list)
                    if (o is Dictionary<string, object> p && p.TryGetValue("kind", out object k) && k as string == "floorStrip") res.Add(p);
            return res;
        }

        [Test]
        public void TheData_OldPlansStripsAreDecor_TheNight65RoomsMarksAreMarks()
        {
            // the n29 foundry plan alone: 8 floorStrips (3 cyan, 3 amber, 2 white), none with a job. (The first version of this test expected 78 + 42 + 8 from it: those are the totals over EVERY plan and level file, 47 files, the
            // generated level files counting each plan's strips again. The code was right, the test's premise was wrong: Research/2026-10-27-wave74/L/decor74-mark-test.txt.)
            var old = FloorStrips("Design/levels/n29/foundry.plan.json");
            Assert.GreaterOrEqual(old.Count, 5, "the n29 foundry plan's floor strips");
            var colours = new HashSet<string>();
            foreach (var p in old)
            {
                Assert.IsFalse(PropsView29Math.IsUsableMark("floorStrip", p), "a decor strip of the old plan: " + (p.TryGetValue("id", out object oid) ? oid : "(no id)"));
                colours.Add(p.TryGetValue("color", out object co) && co is string cs ? cs : "(default)");
            }
            Assert.GreaterOrEqual(colours.Count, 2, "more than one colour field read: the old plans' strips are cyan, amber and white, and they all stay decor");
            // every plan file the repo keeps (Design/levels/n*/*.plan.json: 22 files hold 79 floorStrips today): none is a usable mark, so no old level's decor changes colour
            int strips = 0, plans = 0;
            foreach (string dir in Directory.GetDirectories(Path.Combine(Root(), "Design", "levels"), "n*"))
                foreach (string file in Directory.GetFiles(dir, "*.plan.json"))
                {
                    var rel = file.Substring(Root().Length + 1);
                    var rows = FloorStrips(rel);
                    if (rows.Count > 0) plans++;
                    foreach (var p in rows)
                    {
                        strips++;
                        Assert.IsFalse(PropsView29Math.IsUsableMark("floorStrip", p), rel + ": " + (p.TryGetValue("id", out object pid2) ? pid2 : "(no id)") + " would be taken for a mark");
                    }
                }
            Assert.GreaterOrEqual(plans, 10, "the sweep found the plan files");
            Assert.GreaterOrEqual(strips, 40, "and their floorStrips (79 in 22 files at the time of writing)");
            // NIGHT74 lead (the freeze): R's final a3 (ecd0e9f01) made THE RIDE'S END a padMarker too, so the n65 rooms may hold no floorStrip at
            // all; any floorStrip they do hold must still be a usable mark (the planted rows above keep the mark case covered).
            foreach (string room in new[] { "a2", "a3", "a5" })
            {
                foreach (var p in FloorStrips("Design/levels/n65/rooms/" + room + ".json"))
                    Assert.IsTrue(PropsView29Math.IsUsableMark("floorStrip", p), "a usable mark: " + (p.TryGetValue("id", out object pid) ? pid : "(no id)"));
            }
        }
    }
}
