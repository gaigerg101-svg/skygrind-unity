// NIGHT72 shared commit (FORGE72 D-3; Game/Rows72.cs). Pinned:
//   1. a view row switched both ways by -skygrindRows72 <row>:<0|1> (NightRows.ViewOn reads the switch, the config's design left as it is)
//   2. the list form (-skygrindRows72=a:1+b:0) and the next-argument form (-skygrindRows72 a:1) set each row
//   3. a sim row ("sim": true) is refused: nothing set, and the line says so
//   4. an unknown row changes nothing and says so
//   5. -skygrindNo<Row>72 sets that row OFF (palette72 here, its design ON)
//   6. PlayerSwitch72: the switch and the n72 rows' No switches are player switches; -skygrindNo<Row>71 and a made-up 72 row are not
//   7. the design night key: "n" + the digits of the row's name (calm72 -> n72, foundry65 -> n65); a name with no digits has none
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using R = Skygrind.Game.Rows72;

namespace Skygrind.Game.Tests
{
    public class Rows72Tests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (System.Exception) { return Path.GetFullPath("."); }
        }

        /// <summary>A fresh config (nothing read yet, so nothing cached): the repo's config with every n72 row as the design has it (OFF).</summary>
        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static Dictionary<string, object> N72(GameConfig c) => PortData.Obj(PortData.Obj(c.root["design"])["n72"]);

        [SetUp]
        public void SetUp()
        {
            NightRows.ReelOverride.Clear();
            NightRows.ForceOffFrom = NightRows.NoOff;
        }

        [TearDown]
        public void TearDown()
        {
            NightRows.ReelOverride.Clear();
            NightRows.ForceOffFrom = NightRows.NoOff;
        }

        [Test]
        public void ViewRowSwitchedBothWays()
        {
            GameConfig c = Cfg();
            // NIGHT73: lit72 went ON in the repo at NIGHT72 (D-1), so the start state is the config's, whatever it is; the switch sets both ways
            Assert.That(NightRows.ViewOn(c, "lit72"), Is.EqualTo(PortData.Obj(N72(c)["lit72"])["enabled"]), "before the switch lit72 reads the repo's design");
            R.Apply(c, new[] { "-skygrindRows72", "lit72:1" });
            Assert.That(NightRows.ViewOn(c, "lit72"), Is.True, "lit72:1 turns it ON");
            Assert.That(PortData.Obj(N72(c)["lit72"])["enabled"], Is.EqualTo(true), "and the design block says so");

            c = Cfg();
            R.Apply(c, new[] { "-skygrindRows72", "lit72:0" });
            Assert.That(NightRows.ViewOn(c, "lit72"), Is.False, "lit72:0 turns it OFF");
        }

        [Test]
        public void ListFormAndNextArgumentFormSetEachRow()
        {
            GameConfig c = Cfg();
            List<string> lines = R.Apply(c, new[] { "-skygrindRows72=calm72:1+body72:1" });
            Assert.That(NightRows.ViewOn(c, "calm72"), Is.True, "= form, first pair");
            Assert.That(NightRows.ViewOn(c, "body72"), Is.True, "= form, second pair");
            Assert.That(lines, Does.Contain("[rows72] calm72=1"), "one line per row, printed");

            NightRows.ReelOverride.Clear();
            c = Cfg();
            R.Apply(c, new[] { "-skygrindRows72", "fix72:1" });
            Assert.That(NightRows.ViewOn(c, "fix72"), Is.True, "next-argument form");
        }

        [Test]
        public void SimRowIsRefused()
        {
            GameConfig c = Cfg();
            N72(c)["simrow72"] = new Dictionary<string, object> { { "enabled", false }, { "sim", true } };
            List<string> lines = R.Apply(c, new[] { "-skygrindRows72", "simrow72:1" });
            Assert.That(lines, Does.Contain("[rows72] refused simrow72: a sim row"), "the refusal is printed");
            Assert.That(NightRows.ReelOverride.ContainsKey("simrow72"), Is.False, "a sim row is never overridden");
            Assert.That(PortData.Obj(N72(c)["simrow72"])["enabled"], Is.EqualTo(false), "and its enabled stays as it was");
        }

        [Test]
        public void UnknownRowChangesNothingAndSaysSo()
        {
            GameConfig c = Cfg();
            List<string> lines = R.Apply(c, new[] { "-skygrindRows72", "nosuchrow72:1" });
            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.StartWith("[rows72] unknown row nosuchrow72"), "the unknown row is named");
            Assert.That(NightRows.ReelOverride.Count, Is.EqualTo(0), "no override set");
            Assert.That(R.Apply(c, new[] { "-skygrindRows72", "lit72:2" })[0], Does.StartWith("[rows72] bad pair 'lit72:2'"), "a value other than 0 / 1 is a bad pair");
        }

        [Test]
        public void NoSwitchTurnsARowOffWithItsDesignOn()
        {
            GameConfig c = Cfg();
            N72(c)["palette72"] = new Dictionary<string, object> { { "enabled", true } };
            Assert.That(NightRows.ViewOn(c, "palette72"), Is.True, "design ON before the switch");
            R.Apply(c, new[] { "-skygrindNoPalette72" });
            Assert.That(NightRows.ReelOverride["palette72"], Is.False, "-skygrindNoPalette72 sets palette72 false");
            Assert.That(NightRows.ViewOn(c, "palette72"), Is.False, "and ViewOn reads it OFF");
        }

        [Test]
        public void PlayerSwitchesAreTheSwitchAndTheNightsNoSwitches()
        {
            Assert.That(R.PlayerSwitch72("-skygrindRows72"), Is.True);
            Assert.That(R.PlayerSwitch72("-skygrindRows72=lit72:1"), Is.True);
            Assert.That(R.PlayerSwitch72("-skygrindNoPalette72"), Is.True);
            Assert.That(R.PlayerSwitch72("-skygrindNopalette72"), Is.True, "case ignored, as NightRows.HasSwitch reads them");
            Assert.That(R.PlayerSwitch72("-skygrindNoRoom71"), Is.False, "another night's switch is not this one");
            Assert.That(R.PlayerSwitch72("-skygrindNoMadeUp72"), Is.False, "a made-up 72 row is not a player switch");
            Assert.That(R.PlayerSwitch72("-skygrindRows72x"), Is.False);
            Assert.That(R.PlayerSwitch72(null), Is.False);
        }

        [Test]
        public void NightKeyIsTheDigitsOfTheRowName()
        {
            Assert.That(R.NightKeyOf("calm72"), Is.EqualTo("n72"));
            Assert.That(R.NightKeyOf("foundry65"), Is.EqualTo("n65"), "a level row reads its own night's block");
            Assert.That(R.NightKeyOf("nodigits"), Is.Null);
            Assert.That(R.NightKeyOf(null), Is.Null);
        }
    }
}
