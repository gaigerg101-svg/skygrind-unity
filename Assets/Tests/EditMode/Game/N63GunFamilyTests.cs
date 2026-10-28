// NIGHT63 builder V (leftover 4: R1's N11; NIGHT61's review Logs/n61/review-n61.md N11): the viewmodel built the KESTREL's and the gauntlet's
// family colour straight from weapons.families, outside gunYellow61's rule (GunYellow61.Family), so a later family change to a yellow family would
// bypass it. Both are `kinetic` (cyan) today: nothing on screen changes. Pinned:
//   1. Viewmodel.cs routes the KESTREL's family (the revolver's) and the gauntlet's through GunYellow61.Family, as the THUNDERCLAP and FamilyOf do
//   2. with the row on, both colours come back exactly as they came (kinetic is not in the yellow arc): no pixel moves today
// N61GunYellowTests' rule (only Viewmodel.cs calls GunYellow61.Family) still holds. Written test first (RED on its commit: the two builds still
// read weapons.families directly); the Editor run is the lead's.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N63GunFamilyTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static string LineWith(string src, string needle)
        {
            foreach (string l in src.Split('\n')) if (l.Contains(needle)) return l;
            return null;
        }

        [Test]
        public void TheKestrelAndTheGauntlet_PassGunYellow61sRule()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "Viewmodel.cs"));
            string kestrel = LineWith(src, "weapons.catalog.revolver.family");
            Assert.That(kestrel, Is.Not.Null, "the KESTREL's family line");
            Assert.That(kestrel.Contains("GunYellow61.Family(cfg, \"revolver\""), Is.True, "the KESTREL's family through the rule: " + kestrel.Trim());
            string gauntlet = LineWith(src, "BuildGauntlet(kit,");
            Assert.That(gauntlet, Is.Not.Null, "the gauntlet's build line");
            Assert.That(gauntlet.Contains("GunYellow61.Family(cfg, \"gauntlet\""), Is.True, "the gauntlet's family through the rule: " + gauntlet.Trim());
        }

        [Test]
        public void Kinetic_ComesBackAsItCame_RowOnOrOff()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig c = Cfg();
                var design = (Dictionary<string, object>)c.root["design"];
                var n61 = (Dictionary<string, object>)design["n61"];
                foreach (bool on in new[] { true, false })
                {
                    n61[GunYellow61.Row] = new Dictionary<string, object> { ["_why"] = "test", ["enabled"] = on, ["enabled_why"] = "test", ["numbers"] = new Dictionary<string, object>() };
                    foreach (string w in new[] { "revolver", "gauntlet" })
                    {
                        Color fam = Look.Hex((uint)c.Num("weapons.families." + c.Str("weapons.catalog." + w + ".family")));
                        Assert.That(GunYellow61.FamilyFor(c, new string[0], w, fam), Is.EqualTo(fam), w + " (row " + (on ? "on" : "off") + "): as it came");
                    }
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
