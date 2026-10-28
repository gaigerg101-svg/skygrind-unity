// NIGHT22 U3-36.3 / U3-36.8 (builder M): the pure rules behind the ARCLINE chip (Core/Arcline21Chip.cs) and the HAPTICS patterns
// (Core/Haptics22.cs). One focused test per rule. No UnityEngine: SimHarness runs the same assertions (`arcline2 --selftest`).
// The Unity half (Game/UiArcline22.cs, Game/Haptics22View.cs, Rumble's CanPlay) is not covered here: it was not compiled when this
// file was written.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Arcline22Tests
    {
        [Test]
        public void Chip_NotOwned_IsHidden()
        {
            Assert.That(Arcline22Chip.Resolve(false, 0, Arcline21Kind.Node), Is.EqualTo(Arcline22Look.Hidden));
            Assert.That(Arcline22Chip.Resolve(false, 100, Arcline21Kind.None), Is.EqualTo(Arcline22Look.Hidden));
        }

        [Test]
        public void Chip_OffCooldownWithATarget_IsReady()
        {
            Assert.That(Arcline22Chip.Resolve(true, 0, Arcline21Kind.Enemy), Is.EqualTo(Arcline22Look.Ready));
            Assert.That(Arcline22Chip.Resolve(true, 0, Arcline21Kind.Node), Is.EqualTo(Arcline22Look.Ready));
            Assert.That(Arcline22Chip.Resolve(true, 0, Arcline21Kind.Surface), Is.EqualTo(Arcline22Look.Ready));
        }

        [Test]
        public void Chip_CooldownBeatsNoTarget_SoCooldownAndOutOfRangeAreDifferentLooks()
        {
            Assert.That(Arcline22Chip.Resolve(true, 1800, Arcline21Kind.None), Is.EqualTo(Arcline22Look.Cooldown));
            Assert.That(Arcline22Chip.Resolve(true, 1800, Arcline21Kind.Node), Is.EqualTo(Arcline22Look.Cooldown));
            Assert.That(Arcline22Chip.Resolve(true, 0, Arcline21Kind.None), Is.EqualTo(Arcline22Look.NoTarget));
        }

        [Test]
        public void Reticle_DimsOnlyWhenOwnedAndNothingToHook()
        {
            Assert.That(Arcline22Chip.ReticleDims(true, Arcline21Kind.None), Is.True);
            Assert.That(Arcline22Chip.ReticleDims(true, Arcline21Kind.Surface), Is.False);
            Assert.That(Arcline22Chip.ReticleDims(false, Arcline21Kind.None), Is.False);
        }

        [Test]
        public void Seconds_RoundUp_FifteenAtThePressOneAtTheEnd()
        {
            double dt = 1.0 / 120;
            Assert.That(Arcline22Chip.Seconds(1800, dt), Is.EqualTo(15));
            Assert.That(Arcline22Chip.Seconds(1799, dt), Is.EqualTo(15));
            Assert.That(Arcline22Chip.Seconds(1680, dt), Is.EqualTo(14));
            Assert.That(Arcline22Chip.Seconds(1, dt), Is.EqualTo(1));
            Assert.That(Arcline22Chip.Seconds(0, dt), Is.EqualTo(0));
        }
    }

    public class Haptics22Tests
    {
        sealed class Recorder : IHapticsSink
        {
            public bool can = true;
            public double strength = 1;
            public readonly List<double[]> played = new List<double[]>();
            public bool CanPlay => can;
            public double Strength => strength;
            public void Play(double strong, double weak, double ms) => played.Add(new[] { strong, weak, ms });
        }

        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static void RunOut(Haptics22 h, double seconds)
        {
            for (double t = 0; t < seconds; t += 1.0 / 60) h.Tick(1.0 / 60);
        }

        [Test]
        public void EveryPatternIsInTheConfigWithWholeTriples()
        {
            foreach (string n in Haptics22.Names)
            {
                double[] p = new Haptics22(Cfg, new Recorder()).Pattern(n);
                Assert.That(p != null && p.Length >= 3 && p.Length % 3 == 0, Is.True, n + " is whole [strong, weak, ms] triples");
            }
        }

        [Test]
        public void PatternsAreDistinctFromEachOtherAndFromTheWebsThree()
        {
            var h = new Haptics22(Cfg, new Recorder());
            var seen = new List<string>();
            foreach (string n in Haptics22.Names) seen.Add(string.Join(",", h.Pattern(n)));
            for (int i = 0; i < seen.Count; i++)
                for (int j = i + 1; j < seen.Count; j++)
                    Assert.That(seen[i] == seen[j], Is.False, Haptics22.Names[i] + " and " + Haptics22.Names[j] + " are the same pattern");
            foreach (string web in new[] { "hurt", "kill", "parry" })
                Assert.That(seen.Contains(string.Join(",", Cfg.Nums("gamepad.vibration." + web))), Is.False, "a pattern equals the web's " + web);
        }

        [Test]
        public void Fire_PlaysEachStepInOrderAndLogsThem()
        {
            var rec = new Recorder();
            var h = new Haptics22(Cfg, rec);
            double[] want = h.Pattern("shockwave");
            Assert.That(h.Fire("shockwave"), Is.True);
            RunOut(h, 1.0);
            Assert.That(rec.played.Count, Is.EqualTo(want.Length / 3));
            for (int i = 0; i < rec.played.Count; i++)
            {
                Assert.That(rec.played[i][0], Is.EqualTo(want[i * 3]));
                Assert.That(rec.played[i][2], Is.EqualTo(want[i * 3 + 2]));
            }
            Assert.That(h.Playing, Is.False);
            Assert.That(h.Log.Count, Is.EqualTo(want.Length / 3));
            Assert.That(h.LogLine().StartsWith("haptics: shockwave 0.85/0.35 80ms;"), Is.True, h.LogLine());
        }

        [Test]
        public void VibrationOff_SilencesEveryPattern()
        {
            var rec = new Recorder { can = false };
            var h = new Haptics22(Cfg, rec);
            foreach (string n in Haptics22.Names) Assert.That(h.Fire(n), Is.False, n);
            RunOut(h, 1.0);
            Assert.That(rec.played.Count, Is.EqualTo(0));
            Assert.That(h.Log.Count, Is.EqualTo(0));
            Assert.That(h.LogLine(), Is.EqualTo("haptics: none"));
        }

        [Test]
        public void VibrationTurnedOffMidPattern_StopsAtTheNextStep()
        {
            var rec = new Recorder();
            var h = new Haptics22(Cfg, rec);
            h.Fire("down");
            int first = rec.played.Count;
            rec.can = false;
            RunOut(h, 2.0);
            Assert.That(rec.played.Count, Is.EqualTo(first));
            Assert.That(h.Playing, Is.False);
        }

        [Test]
        public void ADownIsNotCutByAKill_ButAKillIsReplacedByADown()
        {
            var rec = new Recorder();
            var h = new Haptics22(Cfg, rec);
            h.Fire("down");
            Assert.That(h.Fire("kill"), Is.False);
            Assert.That(h.RunningName, Is.EqualTo("down"));
            var h2 = new Haptics22(Cfg, new Recorder());
            h2.Fire("kill");
            Assert.That(h2.Fire("down"), Is.True);
            Assert.That(h2.RunningName, Is.EqualTo("down"));
        }

        [Test]
        public void RumbleStrengthScalesTheLoggedMotors()
        {
            var rec = new Recorder { strength = 0.5 };
            var h = new Haptics22(Cfg, rec);
            h.Fire("shockwave");
            Assert.That(h.Log[0].low, Is.EqualTo(0.425));
        }

        [Test]
        public void ForEvent_MapsTheWebsEventNames_PlayerOnly()
        {
            Assert.That(Haptics22.ForEvent("shockwave", "player"), Is.EqualTo("shockwave"));
            Assert.That(Haptics22.ForEvent("ropeAttach", "player"), Is.EqualTo("attach"));
            Assert.That(Haptics22.ForEvent("arclinePull", null), Is.EqualTo("yank"));
            Assert.That(Haptics22.ForEvent("kill", "player"), Is.EqualTo("kill"));
            Assert.That(Haptics22.ForEvent("death", "player"), Is.EqualTo("down"));
            Assert.That(Haptics22.ForEvent("death", "husk"), Is.Null);
            Assert.That(Haptics22.ForEvent("hurt", "player"), Is.Null);
        }
    }
}
