// NIGHT38 builder M (design.n38.ride38, Core/Ride38.cs): the hold ride's pitch drift, the row's numbers, the noride boxes, and the Mover's hold ride
// in a box world (TriangleWorld: the web's collider) with the row on (the height kept) and off (wallride35's sink). The harness rows (n38ride hold)
// measure the same rides at length.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Ride38Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));
        const double Dt = 1.0 / 120;

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        [Test]
        public void TheRow_HasEveryNumber_WithItsWhy()
        {
            Assert.That(Cfg.Has("design.n38.ride38._why"), Is.True);
            Assert.That(Cfg.Get("design.n38.ride38.enabled"), Is.InstanceOf<bool>());
            foreach (string k in new[] { "holdSink", "guard", "deadzoneDeg", "fullDeg", "driftMax", "topProbe" })
            {
                Assert.That(Cfg.Has("design.n38.ride38." + k), Is.True, k);
                Assert.That(Cfg.Has("design.n38.ride38." + k + "_why"), Is.True, k + "_why");
            }
            Assert.That(Ride38.Params.From(Cfg, false).on, Is.False, "the row's bit off: neutral");
        }

        [Test]
        public void TheDrift_HoldsInTheDeadZone_AndRampsToDriftMax()
        {
            Ride38.Params p = Ride38.Params.From(Cfg, true);
            double Rad(double d) => d * System.Math.PI / 180;
            Assert.That(Ride38.Drift(0, p), Is.EqualTo(-p.holdSink));
            Assert.That(Ride38.Drift(Rad(p.deadzoneDeg - 1), p), Is.EqualTo(-p.holdSink));
            Assert.That(Ride38.Drift(Rad(p.fullDeg), p), Is.EqualTo(p.driftMax).Within(1e-9));
            Assert.That(Ride38.Drift(Rad(-p.fullDeg), p), Is.EqualTo(-p.driftMax).Within(1e-9));
            Assert.That(Ride38.Drift(Rad(80), p), Is.EqualTo(p.driftMax).Within(1e-9), "clamped past fullDeg");
            double mid = (p.deadzoneDeg + p.fullDeg) / 2;
            Assert.That(Ride38.Drift(Rad(mid), p), Is.EqualTo(p.driftMax / 2).Within(1e-9));
        }

        [Test]
        public void NoRideFrom_ReadsOnlyTaggedCampaignBoxes()
        {
            List<object> V(double x, double y, double z) => new List<object> { x, y, z };
            var json = new Dictionary<string, object>
            {
                ["campaign"] = new Dictionary<string, object>
                {
                    ["boxes"] = new List<object>
                    {
                        new Dictionary<string, object> { ["kind"] = "wall", ["min"] = V(0, 0, 0), ["max"] = V(1, 5, 10), ["noride"] = true },
                        new Dictionary<string, object> { ["kind"] = "wall", ["min"] = V(5, 0, 0), ["max"] = V(6, 5, 10), ["ride"] = true },
                        new Dictionary<string, object> { ["kind"] = "wall", ["min"] = V(9, 0, 0), ["max"] = V(10, 5, 10), ["noride"] = false },
                    },
                },
            };
            Ride38.Box[] b = Ride38.NoRideFrom(json);
            Assert.That(b, Has.Length.EqualTo(1));
            Assert.That(b[0].max.y, Is.EqualTo(5));
            Assert.That(Ride38.NoRideFrom(new Dictionary<string, object>()), Is.Null);
            Assert.That(Ride38.NoRideBand(b, 1.05, 3, 0.35, 1.45, 0.12), Is.True, "a capsule's band beside the box's face");
            Assert.That(Ride38.NoRideBand(b, 3, 3, 0.35, 1.45, 0.12), Is.False);
        }

        /// <summary>A hold ride (jump held, look level) along a 60 m tall, 200 m long wall met at 3 m: the feet after `seconds` on the wall.</summary>
        static (bool rode, double attachY, double y, MoveState state) Hold(bool on, double seconds, double pitchDeg = 0, Ride38.Box[] noRide = null)
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-20, -1, -260), new Vec3(20, 0, 40));
            w.AddBox(new Vec3(3, 0, -200), new Vec3(4, 60, 0));
            var m = new Mover(Cfg.movement, w);
            m.SetFlow34(Flow34.Params.From(Cfg, Cfg.Design("n34.flow34.enabled")));
            m.SetRide35(Ride35.Params.From(Cfg, true));
            m.SetGrip36(Grip36.Params.From(Cfg, Cfg.Design("n36.wallgrip36.enabled")));
            m.SetRide38(on ? Ride38.Params.From(Cfg, true) : Ride38.Params.Neutral);
            m.SetNoRide38(noRide);
            m.Reset(3 - Cfg.movement.body.capsuleRadius - 0.02, 3, -1);
            m.velocity = new Vec3(1, 0, -8);
            int attach = -1;
            double attachY = 0;
            int n = (int)System.Math.Round(seconds / Dt);
            for (int s = 0; s < 240 + n; s++)
            {
                var f = new InputFrame { moveX = 0.5, moveZ = 1, pitch = pitchDeg * System.Math.PI / 180, jump = new Button { held = true, pressed = false } };
                m.Step(f, Dt);
                if (attach < 0 && m.state == MoveState.WallRide)
                {
                    attach = s;
                    attachY = m.position.y;
                }
                if (attach >= 0 && s - attach >= n) break;
            }
            return (attach >= 0, attachY, m.position.y, m.state);
        }

        [Test]
        public void AHoldRide_KeepsItsHeight_WithTheRowOn_AndSinksWithItOff()
        {
            var on = Hold(true, 12);
            Assert.That(on.rode, Is.True);
            Assert.That(on.state, Is.EqualTo(MoveState.WallRide), "past wallride35's 8 s safety cap");
            Assert.That(on.y, Is.EqualTo(on.attachY).Within(0.05));
            var off = Hold(false, 6);
            Assert.That(off.rode, Is.True);
            Assert.That(off.y, Is.LessThan(off.attachY - 0.3), "wallride35's 0.1 m/s sink");
        }

        [Test]
        public void ThePitch_DriftsTheRide_AndANoRideWall_StartsNoRide()
        {
            var up = Hold(true, 2, 45);
            Assert.That(up.y - up.attachY, Is.GreaterThan(3).And.LessThan(5.5), "about 2.5 m/s for 2 s once braked");
            var tagged = Hold(true, 1, 0, new[] { new Ride38.Box { min = new Vec3(3, 0, -200), max = new Vec3(4, 60, 0) } });
            Assert.That(tagged.rode, Is.False);
        }
    }
}
