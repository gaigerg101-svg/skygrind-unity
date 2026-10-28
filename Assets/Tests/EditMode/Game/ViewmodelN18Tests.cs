// NIGHT18 U3-31.9 (builder M): the viewmodel's NIGHT18 rules (FeelAB on), pure, at the reel's 60 fps:
// #1 the kill flourish captured and readable: any kill of the player's arms it; it starts after
//    UnityTuning.HandsFlourishQuietN18 s of no shot with the trigger let go (in the reel's kill strips it is near its peak by
//    the flipbook's +400 ms frame); a held trigger never starts it; it shows the gun off (HandsFlourish.Present: up, in, canted).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class ViewmodelN18Tests
    {
        const double Frame = 1.0 / 60;

        [Test]
        public void AnyKillOfThePlayersArmsTheFlourish()
        {
            Assert.That(HandsFlourish.IsKillTrigger("player"), Is.True);
            Assert.That(HandsFlourish.IsKillTrigger("rival"), Is.False);
            Assert.That(HandsFlourish.IsKillTrigger(null), Is.False);
        }

        [Test]
        public void TheFlourishStartsAfterTheQuietWithTheTriggerLetGo()
        {
            double dur = 0.45;
            var f = new HandsFlourish();
            f.Cancel(); // the killing shot
            f.Arm();
            double t = 0, startedAt = -1;
            int starts = 0;
            for (; t < 1.0; t += Frame)
            {
                f.Frame(false, Frame, dur, UnityTuning.HandsFlourishQuietN18, triggerHeld: false);
                if (f.started)
                {
                    starts++;
                    startedAt = t;
                }
            }
            Assert.That(starts, Is.EqualTo(1));
            Assert.That(startedAt, Is.InRange(UnityTuning.HandsFlourishQuietN18 - Frame, UnityTuning.HandsFlourishQuietN18 + 2 * Frame));
            Assert.That(HandsFlourish.Present(0.4 - startedAt, dur, 1).y, Is.GreaterThan(0.9 * UnityTuning.HandsFlourishPresentY), "near its peak at +400 ms");
        }

        [Test]
        public void AHeldTriggerNeverStartsIt()
        {
            var f = new HandsFlourish();
            f.Cancel();
            f.Arm();
            for (double t = 0; t < 1.5; t += Frame)
            {
                f.Frame(false, Frame, 0.45, UnityTuning.HandsFlourishQuietN18, triggerHeld: true); // a slow gun's trigger held between shots
                Assert.That(f.started, Is.False);
            }
            f.Frame(false, Frame, 0.45, UnityTuning.HandsFlourishQuietN18, triggerHeld: false);
            Assert.That(f.started, Is.True, "let go: it starts (the quiet has passed)");
        }

        [Test]
        public void TheQuickdrawSheenTravelsAlongTheGunsOwnParts()
        {
            // the band at the pass's place: that part at the pass's full amount, a far part at the base share, none outside the window
            Assert.That(HandsQuickdraw.Sheen(0.8, 0.5, 0.5), Is.EqualTo(0.8).Within(1e-9));
            Assert.That(HandsQuickdraw.Sheen(0.8, 0.0, 1.0), Is.EqualTo(0.8 * UnityTuning.HandsQuickdrawSheenBase).Within(1e-9));
            Assert.That(HandsQuickdraw.Sheen(0, 0.5, 0.5), Is.EqualTo(0));
            Assert.That(HandsQuickdraw.Sheen(1, 0.3, 0.4), Is.GreaterThan(HandsQuickdraw.Sheen(1, 0.3, 0.6)), "brightest at the band");
            // over the window the back part peaks before the muzzle part (back to front)
            double backPeakAt = -1, frontPeakAt = -1, backPeak = 0, frontPeak = 0;
            for (double t = 0; t < UnityTuning.HandsQuickdrawWindow / UnityTuning.HandsQuickdrawSweeps; t += Frame)
            {
                double a = HandsQuickdraw.Amount(t), along = HandsQuickdraw.Along(t);
                double b = HandsQuickdraw.Sheen(1, along, 0.1), f = HandsQuickdraw.Sheen(1, along, 0.9);
                if (b > backPeak) { backPeak = b; backPeakAt = t; }
                if (f > frontPeak) { frontPeak = f; frontPeakAt = t; }
                Assert.That(HandsQuickdraw.Sheen(a, along, 0.5), Is.InRange(0.0, 1.0));
            }
            Assert.That(backPeakAt, Is.LessThan(frontPeakAt));
            Assert.That(HandsQuickdraw.Place(-0.1, -0.1, 0.2), Is.EqualTo(0).Within(1e-9));
            Assert.That(HandsQuickdraw.Place(0.2, -0.1, 0.2), Is.EqualTo(1).Within(1e-9));
            Assert.That(HandsQuickdraw.Place(0.05, -0.1, 0.2), Is.EqualTo(0.5).Within(1e-9));
            Assert.That(HandsQuickdraw.Place(0.05, 0.05, 0.05), Is.EqualTo(0.5).Within(1e-9), "one part: the middle");
        }

        /// <summary>One shot's kick on the gun in hand, sampled at 60 fps: the peaks (pitch deg, back m, roll deg, signed at
        /// their largest), when the pitch peaks (s), and when every channel is back under 10 % of its peak for good (s).</summary>
        public static double[] KickSignature(string weapon)
        {
            HandsKickSpec spec = HandsKick.SpecFor(weapon);
            var bank = new Skygrind.Core.SpringBank(UnityTuning.HandsKickSlots, HandsKick.Snaps());
            var scratch = new double[HandsKick.Channels];
            HandsKick.Add(bank, spec, 0, 1, scratch);
            double pitch = 0, back = 0, roll = 0, tPeak = 0, settle = 0;
            var trace = new double[3, 120];
            for (int f = 0; f < 120; f++)
            {
                bank.Update(Frame);
                double p = bank.Sample(0), b = bank.Sample(1), r = bank.Sample(3);
                trace[0, f] = p;
                trace[1, f] = b;
                trace[2, f] = r;
                if (System.Math.Abs(p) > System.Math.Abs(pitch)) { pitch = p; tPeak = (f + 1) * Frame; }
                if (System.Math.Abs(b) > System.Math.Abs(back)) back = b;
                if (System.Math.Abs(r) > System.Math.Abs(roll)) roll = r;
            }
            double[] peaks = { pitch, back, roll };
            for (int f = 0; f < 120; f++)
                for (int c = 0; c < 3; c++)
                    if (System.Math.Abs(trace[c, f]) > 0.1 * System.Math.Abs(peaks[c]) && peaks[c] != 0) settle = (f + 1) * Frame;
            return new[] { pitch, back, roll, tPeak, settle };
        }

        [Test]
        public void EachGunsKickIsDistinct()
        {
            string[] guns = { "revolver", "shotgun", "nailgun", "rocket", "rail" };
            var sig = new double[guns.Length][];
            for (int i = 0; i < guns.Length; i++)
            {
                sig[i] = KickSignature(guns[i]);
                System.Console.WriteLine($"KICK {guns[i],-8} pitch {sig[i][0],6:0.0} deg  back {sig[i][1] * 100,5:0.0} cm  roll {sig[i][2],5:0.0} deg  peak {sig[i][3] * 1000,4:0} ms  settle {sig[i][4] * 1000,4:0} ms");
            }
            // each pair apart by at least one step, the steps being (my judgement of) a difference you'd see on the gun:
            // 5 deg of pitch, 5 cm back, 3 deg of roll, 30 ms to the peak, 100 ms to settle (Euclidean, in steps)
            double[] step = { 5, 0.05, 3, 0.03, 0.1 };
            for (int i = 0; i < guns.Length; i++)
                for (int j = i + 1; j < guns.Length; j++)
                {
                    double d = 0;
                    for (int c = 0; c < step.Length; c++) d += System.Math.Pow((sig[i][c] - sig[j][c]) / step[c], 2);
                    Assert.That(System.Math.Sqrt(d), Is.GreaterThanOrEqualTo(1.0), $"{guns[i]} vs {guns[j]}");
                }
        }

        [Test]
        public void TheLandingWeighsOnTheGun()
        {
            // round 1 ("land: Same" on both sides): the reel's 3 m drop (13.5 m/s) weighs nearly fully on the gun (NIGHT17: 0.24)
            Assert.That(HandsMove.LandWeightN18(13.5), Is.GreaterThanOrEqualTo(0.9));
            Assert.That(HandsMove.LandWeight(13.5), Is.LessThan(0.3), "NIGHT17's");
            Assert.That(HandsMove.LandWeightN18(3), Is.EqualTo(0), "a step down: none");
            Assert.That(HandsMove.LandWeightN18(30), Is.EqualTo(1));
            Assert.That(HandsMove.LandWeightN18(8), Is.InRange(0.2, 0.6), "by fall speed");
            Assert.That(UnityTuning.HandsLandDropYN18, Is.GreaterThanOrEqualTo(2 * UnityTuning.HandsLandDropY), "its own drop, visibly more");
        }

        [Test]
        public void TheFlourishPresentsTheGun()
        {
            double dur = 0.45;
            HandsDrawPose a = HandsFlourish.Present(0, dur, 1), mid = HandsFlourish.Present(dur / 2, dur, 1), end = HandsFlourish.Present(dur, dur, 1);
            Assert.That(a.y, Is.EqualTo(0).Within(1e-9));
            Assert.That(end.y, Is.EqualTo(0).Within(1e-9));
            Assert.That(mid.y, Is.EqualTo(UnityTuning.HandsFlourishPresentY).Within(1e-6), "up");
            Assert.That(mid.x, Is.LessThan(0), "in toward the middle");
            Assert.That(mid.yawDeg, Is.GreaterThan(0), "canted in");
            HandsDrawPose half = HandsFlourish.Present(dur / 2, dur, 0.5);
            Assert.That(half.y, Is.EqualTo(mid.y * 0.5).Within(1e-9), "x motion");
        }
    }
}
