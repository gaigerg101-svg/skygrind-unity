// NIGHT61 builder V (slate C3, no yellow on the guns; view row design.n61.gunYellow61, OFF; GunYellow61.N61V3.cs, its hooks in Viewmodel.cs). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff61 (or an older night's) and -skygrindNoGunYellow61; on otherwise (yellow56's own
//      switch, a later night's or another row's leaves it on)
//   2. the arc holds the config's yellow families only: edge #ffb000 (the THUNDERCLAP), fire #ff8c1a (the TORCH), blast #ffe14a (the BOUNCER); not
//      kinetic, volt (lime), gravity, impact, pierce, nor colors.enemy (the danger red-orange) or colors.usable
//   3. on: a gun whose family colour is in the arc takes colors.usable (the dusk palette's player-usable cyan), its alpha kept, outside the tells'
//      yellow band (45-70 deg, S > 0.5); every other family as it came; off (the row, -skygrindOff61, -skygrindNoGunYellow61): every family as it came
//   4. the tells' yellow untouched: the rule is called from Viewmodel.cs only (no enemy, tell or world file reads it)
//   5. the Player's proof line: "[gunyellow61] <weapon> #<from> -> #<to> (colors.usable)"
// Written test first (NIGHT61.md section 4): RED on its commit (GunYellow61's placeholders answer nothing); the Editor run is the lead's.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using G = Skygrind.Game.GunYellow61;

namespace Skygrind.Game.Tests
{
    public class N61GunYellowTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        /// <summary>The repo's config with design.n61.gunYellow61 set in memory to `on` (whatever the file's row says).</summary>
        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            var design = (Dictionary<string, object>)c.root["design"];
            Dictionary<string, object> n61;
            if (design.TryGetValue("n61", out object n) && n is Dictionary<string, object> d61) n61 = d61;
            else
            {
                n61 = new Dictionary<string, object>();
                design["n61"] = n61;
            }
            n61[G.Row] = new Dictionary<string, object> { ["_why"] = "test", ["enabled"] = on, ["enabled_why"] = "test", ["numbers"] = new Dictionary<string, object>() };
            return c;
        }

        static Color Family(GameConfig c, string family) => Look.Hex((uint)c.Num("weapons.families." + family));

        static bool InTellsBand(Color c)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            return h * 360f >= 45f && h * 360f <= 70f && s > 0.5f && v >= 0.15f;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitch()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff; // T-126: whatever an earlier test left
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(G.OffFor(off, new string[0]), Is.True, "the row off: off");
                Assert.That(G.OffFor(on, new string[0]), Is.False, "the row on, no switch: on");
                Assert.That(G.OffFor(on, new[] { "-skygrindNoGunYellow61" }), Is.True, "-skygrindNoGunYellow61: this row alone off");
                Assert.That(G.OffFor(on, new[] { "-skygrindnogunyellow61" }), Is.True, "the one-row switch ignores case (Rim58.No's rule)");
                Assert.That(G.OffFor(on, new[] { "-skygrindOff61" }), Is.True, "-skygrindOff61: NIGHT60's game");
                Assert.That(G.OffFor(on, new[] { "-skygrindOff56" }), Is.True, "an older night's switch covers this night");
                Assert.That(G.OffFor(on, new[] { "-skygrindOff62" }), Is.False, "a later night's switch leaves it on");
                Assert.That(G.OffFor(on, new[] { "-skygrindNoYellow56", "-skygrindNoFixture61" }), Is.False, "yellow56's and another row's switch leave it on");
                Assert.That(G.OffReason(on, new string[0]), Is.Null, "on: no reason");
                Assert.That(G.OffReason(off, new string[0]), Is.Not.Null.And.Not.Empty, "off: a reason");
                NightRows.ForceOffFrom = 61; // the harnesses' and PlayerChecks' process switch (-skygrindOff61 at boot)
                Assert.That(G.OffFor(on, new string[0]), Is.True, "ForceOffFrom 61: off");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void TheArc_HoldsTheYellowFamiliesOnly()
        {
            GameConfig c = Cfg();
            foreach (string f in new[] { "edge", "fire", "blast" }) Assert.That(G.InArc(Family(c, f)), Is.True, f + " is a yellow family");
            foreach (string f in new[] { "kinetic", "volt", "gravity", "impact", "pierce" }) Assert.That(G.InArc(Family(c, f)), Is.False, f + " is not");
            Assert.That(G.InArc(Look.C(c, "colors.enemy")), Is.False, "the danger red-orange is not in the arc");
            Assert.That(G.InArc(Look.C(c, "colors.usable")), Is.False, "the usable cyan is not in the arc");
            Assert.That(c.Str("weapons.catalog.shotgun.family"), Is.EqualTo("edge"), "the THUNDERCLAP is an edge gun");
            Assert.That(c.Str("weapons.catalog.torch.family"), Is.EqualTo("fire"), "the TORCH is a fire gun");
        }

        [Test]
        public void On_TheYellowGunsTakeTheUsableCyan_OthersAsTheyCame()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig on = WithRow(true);
                Color usable = Look.C(on, "colors.usable");
                var guns = new Dictionary<string, string> { ["shotgun"] = "edge", ["torch"] = "fire", ["bouncer"] = "blast" };
                foreach (var kv in guns)
                {
                    Color from = Family(on, kv.Value);
                    from.a = 0.5f;
                    Color to = G.FamilyFor(on, new string[0], kv.Key, from);
                    Assert.That(to.r, Is.EqualTo(usable.r).Within(1e-6), kv.Key + ": colors.usable (r)");
                    Assert.That(to.g, Is.EqualTo(usable.g).Within(1e-6), kv.Key + ": colors.usable (g)");
                    Assert.That(to.b, Is.EqualTo(usable.b).Within(1e-6), kv.Key + ": colors.usable (b)");
                    Assert.That(to.a, Is.EqualTo(0.5f).Within(1e-6), kv.Key + ": its alpha kept");
                    Assert.That(InTellsBand(to), Is.False, kv.Key + ": out of the tells' yellow band");
                }
                foreach (var kv in new Dictionary<string, string> { ["revolver"] = "kinetic", ["nailgun"] = "volt", ["rocket"] = "gravity", ["rail"] = "pierce" })
                {
                    Color from = Family(on, kv.Value);
                    Color to = G.FamilyFor(on, new string[0], kv.Key, from);
                    Assert.That(to, Is.EqualTo(from), kv.Key + ": not a yellow family, as it came");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Off_EveryFamilyAsItCame()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig on = WithRow(true), off = WithRow(false);
                Color edge = Family(on, "edge");
                Assert.That(G.FamilyFor(off, new string[0], "shotgun", edge), Is.EqualTo(edge), "the row off");
                Assert.That(G.FamilyFor(on, new[] { "-skygrindNoGunYellow61" }, "shotgun", edge), Is.EqualTo(edge), "-skygrindNoGunYellow61");
                Assert.That(G.FamilyFor(on, new[] { "-skygrindOff61" }, "shotgun", edge), Is.EqualTo(edge), "-skygrindOff61");
                Assert.That(G.FamilyFor(on, new string[0], "shotgun", edge), Is.Not.EqualTo(edge), "on: it moves (the control)");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void TheTellsUntouched_OnlyTheViewmodelCallsTheRule()
        {
            string dir = Path.Combine(Repo(), "Assets", "Scripts");
            var callers = new List<string>();
            foreach (string f in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(f);
                if (name == "GunYellow61.N61V3.cs") continue;
                if (File.ReadAllText(f).Contains("GunYellow61.Family")) callers.Add(name); // a call of the rule (Family / FamilyFor), not a comment naming the file
            }
            callers.Sort(StringComparer.Ordinal);
            Assert.That(callers, Is.EqualTo(new List<string> { "Viewmodel.cs" }), "the rule reaches the viewmodel's guns only (no enemy, tell, pickup or world file)");
        }

        [Test]
        public void LogLine_ThePlayersProof()
        {
            Assert.That(G.LogLine("shotgun", Look.Hex(0xffb000), Look.Hex(0x19d3e6)), Is.EqualTo("[gunyellow61] shotgun #FFB000 -> #19D3E6 (colors.usable)"));
        }
    }
}
