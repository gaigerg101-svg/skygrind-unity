// NIGHT22 U3-36.5 DEADLINE bar (builder S): the fuse event carries what a kill paid, for the HUD's separate "+N" pop
// (Core/Deadline21.cs DeadlineEvent.paid; numbers design.n21.deadline). Seen red once: the field did not exist (the harness line
// `deadline2 bar data` printed FAIL / did not compile). A picture-only field: the sim's state and the fingerprint are unchanged.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class Deadline22Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public readonly EventBus bus = new EventBus();
            public readonly Health hp;
            public readonly Deadline21 fuse;
            public bool sliding;
            public readonly List<double> refills = new List<double>();
            public readonly List<double> others = new List<double>();

            public Rig()
            {
                hp = new Health(Cfg.Num("health.max"), bus, "player");
                fuse = new Deadline21(Cfg, bus, hp, () => sliding);
                bus.On<DeadlineEvent>(Deadline21.Tick, e => (e.refill ? refills : others).Add(e.paid));
                bus.On<DeadlineEvent>(Deadline21.Burn, e => others.Add(e.paid));
                fuse.NewRun(true);
                bus.Emit(N20Events.SpecialRound, new SpecialRoundEvent { name = "twist:" + Deadline21.TwistId, round = 9, phase = "start" });
                for (int i = 0; i < MovementMath.StepsFor(6, Cfg.Dt); i++) fuse.Step();
            }
        }

        [Test]
        public void AKillsRefill_CarriesWhatItPaid_ThePlainTheSlideAndTheSlam()
        {
            var r = new Rig();
            r.bus.Emit("kill", new KillEvent { weapon = "revolver" });
            r.sliding = true;
            r.bus.Emit("kill", new KillEvent { weapon = "revolver" });
            r.sliding = false;
            r.bus.Emit("kill", new KillEvent { weapon = "slam" });
            Assert.That(r.refills.Count, Is.EqualTo(3));
            Assert.That(r.refills[0], Is.EqualTo(Cfg.Num("design.n21.deadline.killRefill")).Within(1e-9));
            Assert.That(r.refills[1], Is.EqualTo(Cfg.Num("design.n21.deadline.slideRefill")).Within(1e-9));
            Assert.That(r.refills[2], Is.EqualTo(Cfg.Num("design.n21.deadline.slamRefill")).Within(1e-9));
        }

        [Test]
        public void TheClockTicks_AndTheBurn_PayNothing()
        {
            var r = new Rig();
            for (int i = 0; i < MovementMath.StepsFor(30, Cfg.Dt); i++) r.fuse.Step();
            Assert.That(r.others.Count, Is.GreaterThan(3), "ticks and at least one burn");
            foreach (double p in r.others) Assert.That(p, Is.EqualTo(0));
        }
    }
}
