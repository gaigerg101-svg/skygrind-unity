// NIGHT43 builder W1: the pure parts of parry43 and fusion43 (Core/Parry43.cs, Core/Fusion43.cs, ImpactRules' Fusion43 tier). Every number
// is read from design.n43 (Design/unity-config.json) through GameConfig, never typed here. The scripted sim checks (the parry window by
// press time, the dash-in grid, THE ICEBREAKER's band) are the harness's: `n43weap parry | fusion | band` (Tools/SimHarness/Harness.N43.Weapons.cs).
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class W43WeaponsTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        const string P = "design.n43.parry43.", F = "design.n43.fusion43.";

        static DroneOrbs Orbs(bool parry43)
        {
            var orbs = new DroneOrbs(cfg, new EventBus());
            if (parry43)
            {
                orbs.parry42 = true;
                orbs.reach42 = cfg.Num("orb.reflectRange") * cfg.Num(P + "reachMult");
                orbs.Parry43Set(true, cfg.Num(P + "catchRadius"), cfg.Num(P + "catchAhead"), MovementMath.StepsFor(cfg.Num(P + "windowS"), cfg.Dt), cfg.movement.body.eyeHeight);
            }
            return orbs;
        }

        [Test]
        public void Parry43_TheSphereAheadCatchesAnOrbNightFortyTwoMisses()
        {
            var eye = new Vec3(0, 1.6, 0);
            var aim = new Vec3(0, 0, -1);
            // an orb 3.6 m ahead, still: outside NIGHT42's 2 m eye point and parry42's 3 m body reach, inside the 1.5 m sphere 2 m ahead (+ its radius)
            DroneOrbs off = Orbs(false), on = Orbs(true);
            off.Fire(new Vec3(0, 1.6, -3.6), new Vec3(0, 0, 1), null, 0.0001);
            on.Fire(new Vec3(0, 1.6, -3.6), new Vec3(0, 0, 1), null, 0.0001);
            Assert.That(off.NearestHostile(eye, cfg.Num("orb.reflectRange")), Is.Null, "NIGHT42's point test misses it");
            Assert.That(on.NearestHostile43(eye, aim), Is.Not.Null, "the sphere ahead catches it");
            Assert.That(on.caughtAhead43, Is.EqualTo(1));
            // behind the player, 3.6 m back and still: no test catches it
            DroneOrbs back = Orbs(true);
            back.Fire(new Vec3(0, 1.6, 3.6), new Vec3(0, 0, 1), null, 0.0001);
            Assert.That(back.NearestHostile43(eye, aim), Is.Null);
        }

        [Test]
        public void Parry43_TheTimeWindowCatchesAFastOrbFromTheSide()
        {
            var eye = new Vec3(0, 1.6, 0);
            var aim = new Vec3(0, 0, -1);
            double window = cfg.Num(P + "windowS");
            // from the side (+x), 6 m off the axis, flying at the player at 30 m/s: it touches in ~0.19 s, inside the window; at 12 m/s it does not
            DroneOrbs fast = Orbs(true), slow = Orbs(true);
            fast.Fire(new Vec3(6, 1.2, 0), new Vec3(-1, 0, 0), null, 30);
            slow.Fire(new Vec3(6, 1.2, 0), new Vec3(-1, 0, 0), null, 12);
            Assert.That((6 - 0.7) / 30, Is.LessThan(window));
            Assert.That(fast.NearestHostile43(eye, aim), Is.Not.Null, "a fast orb inside the time window is caught");
            Assert.That(fast.caughtSoon43, Is.EqualTo(1));
            Assert.That(slow.NearestHostile43(eye, aim), Is.Null, "a 12 m/s orb 6 m away is not yet");
        }

        [Test]
        public void Fusion43_TheImpactTierTakesTheSimsLengthAndWinsTheGauntletsTie()
        {
            double hs = cfg.Num(F + "hitstopS"), bs = cfg.Num(F + "bossHitstopS");
            var r = new ImpactRules(cfg) { impact42 = true };
            r.BeginFrame(5, false);
            r.ClearPending();
            r.OnGauntletHit42(new GauntletHit42Event { cls = GauntletHit42Event.Small, killed = true });
            r.OnSimHitstop(N43Events.Fusion);
            Assert.That(r.EndStep(false, hs), Is.True);
            Assert.That(r.granted, Is.EqualTo(ImpactTier.Fusion43));
            Assert.That(r.grantedSeconds, Is.EqualTo(hs).Within(1e-9));
            Assert.That(ImpactRules.TierNames[(int)ImpactTier.Fusion43], Is.EqualTo("fusion43"));
            Assert.That(bs, Is.GreaterThan(r.Gauntlet42Seconds(GauntletHit42Event.Boss, false)), "a boss FUSION freezes longer than the boss gauntlet hit");
            // without the FUSION's cause the same sim request is the unknown-cause Sim tier, as before
            var u = new ImpactRules(cfg) { impact42 = true };
            u.BeginFrame(5, false);
            u.ClearPending();
            u.OnSimHitstop("heavy41");
            u.EndStep(false, 0.1);
            Assert.That(u.granted, Is.EqualTo(ImpactTier.Sim));
        }

        [Test]
        public void Fusion43_TheWindowsCountInWholeSteps()
        {
            var f = new Fusion43(new EventBus());
            f.Set(true, cfg);
            Assert.That(f.dashAfterSteps, Is.EqualTo(MovementMath.StepsFor(cfg.Num(F + "dashAfterS"), cfg.Dt)));
            f.Step(true);
            Assert.That(f.sinceDash, Is.EqualTo(0));
            for (int i = 0; i < f.dashAfterSteps; i++) f.Step(false);
            Assert.That(f.sinceDash, Is.EqualTo(f.dashAfterSteps), "the last step of the window");
            f.WallJump();
            Assert.That(f.sinceWallJump, Is.EqualTo(0));
            f.Set(false, cfg);
            f.Step(true);
            Assert.That(f.on, Is.False);
            Assert.That(f.sinceDash, Is.GreaterThan(1000), "off: nothing counts");
        }
    }
}
