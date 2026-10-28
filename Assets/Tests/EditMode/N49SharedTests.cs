// NIGHT49 shared commit (Tools/night/n49-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n49 row exists with a _why and a bool enabled, is off unless a freeze turned it on (its enabled_why then starts with "ON"),
//      and NightRows.ViewOn reads the same switch; the checker finds no problem in design
//   2. the off switch: ForceOffFrom 49 (-skygrindOff49 / --off49) turns every n49 view row off whatever the config says
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N49SharedTests
    {
        static readonly string[] ViewRows = { "charge49", "look49", "lights49", "fx49" };

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        /// <summary>NightRows.ViewOn with no off switch: Lesson25Tests leaves ForceOffFrom at FirstNight (46) and never restores it (L-082), and this check reads the config only.</summary>
        static bool ViewOnWithoutOff(GameConfig c, string row)
        {
            int was = NightRows.ForceOffFrom;
            try { NightRows.ForceOffFrom = NightRows.NoOff; return NightRows.ViewOn(c, row); }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n49._why"), Is.True, "design.n49._why");
            foreach (string row in ViewRows)
            {
                string p = "design.n49." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n49." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
                Assert.That(ViewOnWithoutOff(c, row), Is.EqualTo(on), row + ": NightRows.ViewOn reads the design.n49 switch");
            }
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void OffSwitch49_TurnsEveryViewRowOff()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 49; // -skygrindOff49 / --off49
                GameConfig c = Load();
                foreach (string row in ViewRows) Assert.That(NightRows.ViewOn(c, row), Is.False, "--off49: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
