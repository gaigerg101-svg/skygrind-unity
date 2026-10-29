// GG 2026-10-26 quick fixes (Game/MovementTuning71.cs). Pinned: with design.ultra71 on, the ULTRAKILL profile's values are written absolutely
// (jump 5.07 m in 0.702 s, no fall multiplier, the dash, slide, slam and wall-jump numbers); with -skygrindNoUltra71 the jump is the web's
// 1.5 m x design.movementTuning.jumpHeightMult (1.5); every key of `set` names a real movement field; the web's own config (PortExport, read by
// the golden vectors) is never changed by a fresh load.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using R = Skygrind.Game.MovementTuning71;

namespace Skygrind.Game.Tests
{
    public class MovementTuning71Tests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        [TearDown]
        public void WebBaselineAgain() => FingerprintBaseline.Ultra71 = false; // Apply sets the static; later fingerprint tests judge the web's run

        [Test]
        public void TheProfileSelectsItsOwnFingerprintBaseline()
        {
            R.Apply(Cfg(), new string[0]);
            Assert.That(FingerprintBaseline.Active, Is.SameAs(FingerprintBaseline.Ultra71Hashes));
            R.Apply(Cfg(), new[] { R.UltraOffFlag });
            Assert.That(FingerprintBaseline.Active, Is.SameAs(FingerprintBaseline.Hashes));
        }

        [Test]
        public void TheUltraProfileSetsTheJumpAndTheMoves()
        {
            GameConfig c = Cfg();
            double h = R.Apply(c, new string[0]);
            Assert.That(R.UltraOn, Is.True);
            Assert.That(h, Is.EqualTo(4.0).Within(1e-9)); // GG 2026-10-28: the jump lower (was 5.07)
            Assert.That(c.movement.jump.apexTime, Is.EqualTo(0.6235).Within(1e-9));
            Assert.That(c.movement.jump.fallGravityMult, Is.EqualTo(1.0).Within(1e-9));
            Assert.That(c.movement.dash.speed, Is.EqualTo(25.46).Within(1e-9));
            Assert.That(c.movement.dash.duration, Is.EqualTo(0.2).Within(1e-9));
            Assert.That(c.movement.slam.speed, Is.EqualTo(51.43).Within(1e-9));
            Assert.That(c.movement.jump.doubleJumps, Is.EqualTo(1), "the double jump is kept (PRINCIPLES rule 1)");
        }

        [Test]
        public void EveryProfileKeyIsAMovementField()
        {
            GameConfig c = Cfg();
            var set = (Dictionary<string, object>)c.Get("design.ultra71.set");
            foreach (string k in set.Keys) Assert.That(R.SetField(new MovementConfig(), k, 1.0), Is.True, k + " is not a MovementConfig field");
        }

        [Test]
        public void TheOffSwitchKeepsTheEarlierJumpMultiplier()
        {
            GameConfig c = Cfg();
            double h = R.Apply(c, new[] { R.UltraOffFlag });
            Assert.That(R.UltraOn, Is.False);
            Assert.That(R.JumpHeightMult(c), Is.EqualTo(1.5).Within(1e-12));
            Assert.That(h, Is.EqualTo(1.5 * 1.5).Within(1e-12));
            Assert.That(c.movement.jump.apexTime, Is.EqualTo(0.36).Within(1e-12), "the time to apex is unchanged");
        }

        [Test]
        public void TheWebsOwnConfigIsNeverChanged()
        {
            R.Apply(Cfg(), new string[0]);
            Assert.That(Cfg().movement.jump.height, Is.EqualTo(1.5).Within(1e-12), "a fresh load of the web config is still 1.5 m");
        }
    }
}
