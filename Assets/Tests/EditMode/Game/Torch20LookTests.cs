// NIGHT20 B5's look (builder HV): the TORCH as the web draws it, on builder G's port (Core/Guns/Torch.cs):
// - the fire wall (gunFx.js update :129-142): flames lick up along its line at weaponFx.fireWallRate a second while it lasts,
//   each at lerp(a, b, r), along ((r - 0.5) 0.4, 1, (r' - 0.5) 0.4) normalized, at 3 + 2 r m/s; and (the port's, a Unity-only
//   look) a soft CYAN band on the floor along the line that marks where it burns, entering and leaving with its life;
// - the nozzle and the pilot (viewmodel.js :461-468): lit = (0.7 + 0.3 sin(23 t) sin(7 t)) x (1 + 1.5 kick) x (locked ? 0.3 :
//   1 + viewmodel.states.torchHeatGlow x heat), from the loadout's TorchHeat / TorchLocked.
using NUnit.Framework;
using UnityEngine;
using S = Skygrind.Game.SprayMath;

namespace Skygrind.Game.Tests
{
    public class Torch20LookTests
    {
        [Test]
        public void AFireWallLicksUpAlongItsLine_AtTheWebsRate()
        {
            // gunFx.js update: fw.carry += frameDt x wf.fireWallRate; one flame per whole carry
            double carry = 0;
            int n = 0;
            for (int f = 0; f < 180; f++) n += S.WallEmits(ref carry, 1 / 60.0, 60);
            Assert.That(n, Is.InRange(179, 180), "60 a second for 3 s");
            carry = 0;
            Assert.AreEqual(0, S.WallEmits(ref carry, 0.01, 60), "0.6 carried");
            Assert.AreEqual(1, S.WallEmits(ref carry, 0.01, 60), "1.2: one, 0.2 carried");
            Assert.That(carry, Is.EqualTo(0.2).Within(1e-9));
            Assert.AreEqual(6, S.WallEmits(ref carry, 0.1, 60), "a long frame emits its whole share");
            Vector3 a = new Vector3(0, 1, 0), b = new Vector3(7, 1, 0);
            Assert.That(Vector3.Distance(S.WallAt(a, b, 0.5), new Vector3(3.5f, 1, 0)), Is.LessThan(1e-5f), "lerp(a, b, r)");
            Assert.That(Vector3.Distance(S.WallAt(a, b, 1), b), Is.LessThan(1e-5f));
            for (int i = 0; i <= 10; i++)
            {
                Vector3 up = S.WallUp(i / 10.0, 1 - i / 10.0);
                Assert.That(up.magnitude, Is.EqualTo(1).Within(1e-5));
                Assert.That(Vector3.Angle(Vector3.up, up), Is.LessThanOrEqualTo(15.8f), "(+-0.2, 1, +-0.2) normalized: within atan(0.2 sqrt 2) of up");
            }
            Assert.That(S.WallSpeed(0), Is.EqualTo(3).Within(1e-9));
            Assert.That(S.WallSpeed(1), Is.EqualTo(5).Within(1e-9), "3 + 2 r");
        }

        [Test]
        public void AWallsFloorBand_ShowsItsBurnZone_ForItsLife()
        {
            Assert.That(FireWall20.Alpha(0, 3), Is.LessThan(0.3f), "an entrance, not a pop");
            Assert.That(FireWall20.Alpha(UnityTuning.FireWallBandIn, 3), Is.EqualTo(1).Within(1e-4));
            Assert.That(FireWall20.Alpha(1.5, 3), Is.EqualTo(1).Within(1e-6), "held while it burns");
            float late = FireWall20.Alpha(3 - UnityTuning.FireWallBandOut * 0.5, 3);
            Assert.That(late, Is.InRange(0.05f, 0.95f), "fading in its last part");
            Assert.AreEqual(0f, FireWall20.Alpha(3, 3), "gone with the wall");
            Assert.AreEqual(0f, FireWall20.Alpha(-0.1, 3));
            Assert.That(FireWall20.Alpha(2.0, 3), Is.GreaterThan(FireWall20.Alpha(2.8, 3)), "it fades out, never back up");
        }

        /// <summary>Banned 16 (row 4, builder HV): the aim's sprays (the TORCH's flame, the THUNDERCLAP's double blast) fly
        /// down the aim line and converge on the crosshair; within Spray20ClearInner px of the screen's centre (at 1080p) a
        /// streak isn't drawn, whole again from Spray20ClearOuter, smooth between; scaled with the screen's height.</summary>
        [Test]
        public void TheAimsSprays_KeepTheCrosshairClear()
        {
            Assert.AreEqual(0f, S.CrossClear(0, 1080), "on the crosshair: none");
            Assert.AreEqual(0f, S.CrossClear(UnityTuning.Spray20ClearInner, 1080), 1e-6);
            Assert.AreEqual(1f, S.CrossClear(UnityTuning.Spray20ClearOuter, 1080), 1e-6, "whole past the clear ring");
            Assert.AreEqual(1f, S.CrossClear(400, 1080));
            float mid = S.CrossClear((UnityTuning.Spray20ClearInner + UnityTuning.Spray20ClearOuter) * 0.5f, 1080);
            Assert.That(mid, Is.InRange(0.2f, 0.8f), "a soft edge, not a hole's rim");
            Assert.AreEqual(0f, S.CrossClear(UnityTuning.Spray20ClearInner * 2, 2160), 1e-6, "4K: the same share of the screen");
        }

        [Test]
        public void TheTorchsNozzle_GlowsWithItsHeat_AndDimsWhileLocked()
        {
            // viewmodel.js :466: lit = (0.7 + 0.3 sin(23 t) sin(7 t)) x (1 + 1.5 kick) x (locked ? 0.3 : 1 + torchHeatGlow x heat)
            Assert.That(TorchGlow20.Lit(0, 0, 0, false, 2.5), Is.EqualTo(0.7).Within(1e-9), "cool, t 0");
            Assert.That(TorchGlow20.Lit(0, 0, 1, false, 2.5), Is.EqualTo(0.7 * 3.5).Within(1e-9), "full heat: x (1 + 2.5)");
            Assert.That(TorchGlow20.Lit(0, 1, 0.5, false, 2.5), Is.EqualTo(0.7 * 2.5 * 2.25).Within(1e-9), "a kick at half heat");
            Assert.That(TorchGlow20.Lit(0, 0, 1, true, 2.5), Is.EqualTo(0.7 * 0.3).Within(1e-9), "locked: 0.3, whatever the heat");
            double t = 0.1;
            Assert.That(TorchGlow20.Lit(t, 0, 0, false, 2.5), Is.EqualTo(0.7 + 0.3 * System.Math.Sin(23 * t) * System.Math.Sin(7 * t)).Within(1e-12), "the pilot's flicker");
        }
    }
}
