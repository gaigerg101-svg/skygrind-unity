// NIGHT30 builder V (item 2, design.n30.hud30): the weapon-state widget's rules (Game/HudWeapons30.cs: Slot30Rules). dt is the sim's 1/120 s.
using NUnit.Framework;
using W = Skygrind.Game.Slot30Rules;

namespace Skygrind.Game.Tests
{
    public class HudWeapons30Tests
    {
        const double Dt = 1.0 / 120;

        [Test]
        public void AGunCoolsWithADrainingRingThenTurnsReady()
        {
            // a 0.9 s shotgun cycle, 0.5 s left: COOLING, the ring at the share left, the icon dimmed, no seconds under 1 s
            Slot30Look c = W.Gun(60, 108, Dt, false, 0, 0, 0);
            Assert.That(c.state, Is.EqualTo(Slot30.Cooling));
            Assert.That(c.ring, Is.EqualTo(60f / 108f).Within(1e-6));
            Assert.That(c.icon, Is.EqualTo(UnityTuning.Hud30CoolIconAlpha));
            Assert.That(c.word, Is.Null, "no seconds at 0.5 s left");
            Assert.That(c.waiting, Is.True);
            // 1.8 s left of a 2.5 s swing: the seconds show
            Slot30Look p = W.Punch(216, 300, Dt);
            Assert.That(p.state, Is.EqualTo(Slot30.Cooling));
            Assert.That(p.word, Is.EqualTo("1.8"));
            // done: READY, full ring, bright, and that is a READY turn
            Slot30Look r = W.Gun(0, 0, Dt, false, 0, 0, 0);
            Assert.That(r.state, Is.EqualTo(Slot30.Ready));
            Assert.That(r.ring, Is.EqualTo(1f));
            Assert.That(r.icon, Is.EqualTo(1f));
            Assert.That(W.TurnedReady(c.waiting, r.state), Is.True);
        }

        [Test]
        public void AShortShotCycleNeverShowsCoolingSoThePistolDoesNotFlicker()
        {
            Slot30Look s = W.Gun(20, 24, Dt, false, 0, 0, 0); // a 0.2 s pistol cycle
            Assert.That(s.state, Is.EqualTo(Slot30.Ready));
            Assert.That(s.waiting, Is.False);
            Assert.That(W.TurnedReady(s.waiting, Slot30.Ready), Is.False);
        }

        [Test]
        public void AChargeShotFillsWithTicksAndItsCooldownKeepsTheIconBright()
        {
            Slot30Look ch = W.Gun(0, 0, Dt, true, 0.5, 0, 0);
            Assert.That(ch.state, Is.EqualTo(Slot30.Charging));
            Assert.That(ch.ring, Is.EqualTo(0.5f));
            Assert.That(ch.ticks, Is.EqualTo(UnityTuning.Hud30ChargeTicks));
            Assert.That(ch.word, Is.EqualTo(UnityTuning.Hud30WordCharging));
            Assert.That(W.TurnedReady(false, Slot30.Ready), Is.False, "a charge shot's release is not a READY turn");
            Slot30Look cd = W.Gun(0, 0, Dt, false, 0, 600, 1200); // the charged shot's 10 s cooldown, half gone: the normal shot still fires
            Assert.That(cd.state, Is.EqualTo(Slot30.Cooling));
            Assert.That(cd.icon, Is.EqualTo(1f));
            Assert.That(cd.word, Is.EqualTo("5.0"));
        }

        [Test]
        public void TheBlastSlotFillsItsPipsThenCoolsThenIsReady()
        {
            Slot30Look a = W.Blast(2, 5, 0, 1800, Dt);
            Assert.That(a.state, Is.EqualTo(Slot30.Charging));
            Assert.That(a.word, Is.EqualTo("2/5"));
            Assert.That(a.ticks, Is.EqualTo(5));
            Assert.That(a.ring, Is.EqualTo(0.4f).Within(1e-6));
            Assert.That(a.waiting, Is.True);
            Slot30Look b = W.Blast(5, 5, 900, 1800, Dt);
            Assert.That(b.state, Is.EqualTo(Slot30.Cooling));
            Assert.That(b.word, Is.EqualTo("7.5"));
            Slot30Look after = W.Blast(0, 5, 1700, 1800, Dt); // just after a blast: the glove is empty AND the cooldown runs: COOLING with its seconds
            Assert.That(after.state, Is.EqualTo(Slot30.Cooling));
            Assert.That(after.word, Is.EqualTo("14.2"));
            Assert.That(W.Blast(1, 5, 60, 1800, Dt).word, Is.EqualTo("1/5"), "under a second left: the pips word");
            Slot30Look c = W.Blast(5, 5, 0, 1800, Dt);
            Assert.That(c.state, Is.EqualTo(Slot30.Ready));
            Assert.That(W.TurnedReady(b.waiting, c.state), Is.True);
            Assert.That(W.TurnedReady(a.waiting, c.state), Is.True, "the fifth pip with the cooldown run out is a READY turn");
        }

        [Test]
        public void TheWordIsSwitchingThenReadyThenTheStateThenTheRestLabel()
        {
            Slot30Look r = W.Gun(0, 0, Dt, false, 0, 0, 0);
            Assert.That(W.Word(true, 0.2, r, "PUNCH"), Is.EqualTo(UnityTuning.Hud30WordSwitching));
            Assert.That(W.Word(false, 0.2, r, "PUNCH"), Is.EqualTo(UnityTuning.Hud30WordReady));
            Assert.That(W.Word(false, 1.2, r, "PUNCH"), Is.EqualTo("PUNCH"));
            Assert.That(W.Word(false, -1, r, null), Is.Null);
            Slot30Look c = W.Punch(216, 300, Dt);
            Assert.That(W.Word(false, -1, c, "PUNCH"), Is.EqualTo("1.8"));
            Assert.That(W.Name(Slot30.Cooling), Is.EqualTo("COOLING"));
        }
    }
}
