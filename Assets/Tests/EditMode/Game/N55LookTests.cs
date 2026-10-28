// NIGHT55 builder C (camera and look; view rows step55, fov55): the pieces that run without a scene. Every assert is a boolean (Is.True) so the
// SimHarness's mini NUnit runs the same file (`n55look selftest`, Tools/SimHarness/Harness.N55.Look.cs).
//   step55 (Game/Move55.cs Move55Step) against move54 (Move54Step, NIGHT54's shipped numbers), driven with the render's own math (the
//   accumulator, alpha, drawn = prev + (cur - prev) x alpha) at 60 / 144 / 240 fps:
//     - a 0.35 m grounded snap down (collision.groundSnap) and a 0.229 m step up (the kiln kb2c lip `n55look step` measured): step55's largest
//       one-frame camera change <= 50 % of move54's at every rate (slate P0-step);
//     - the camera is back within 5 % of the snap 0.3 s after it, and at the same height at the same time whatever the frame rate;
//     - a staircase of snaps never holds more than Step55MaxOffset; a jump, a landing, a 0.06 m ramp step and a teleport hold nothing;
//     - the row is off in the shipped config (NIGHT54's camera).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N55LookTests
    {
        static string Root => Path.GetFullPath("PortExport");

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        const double Step = 1.0 / 120;

        /// <summary>The feet's height after each 120 Hz step (index 0 before the first), all grounded, with one change of `dy` at step 60.</summary>
        static List<double> Track(double dy, int steps = 180)
        {
            var y = new List<double> { 0 };
            for (int i = 1; i <= steps; i++) y.Add(y[i - 1] + (i == 60 ? dy : 0));
            return y;
        }

        /// <summary>The render's math at `fps`: the largest one-frame camera change for move54 (0) and step55 (1), and the camera height
        /// relative to the feet at `probeS` seconds after the step (1: step55).</summary>
        static void Render(List<double> y, double fps, out double off, out double on, double probeS, out double onAtProbe)
        {
            var m54 = new Move54Step(Cfg());
            var m55 = new Move55Step(Step);
            double frameDt = 1.0 / fps, acc = 0;
            int idx = 0;
            double lastOff = double.NaN, lastOn = double.NaN;
            off = on = 0;
            onAtProbe = double.NaN;
            while (idx < y.Count - 1)
            {
                acc += frameDt;
                while (acc >= Step && idx < y.Count - 1)
                {
                    idx++;
                    m54.StepEnded(true, y[idx - 1], y[idx]);
                    m55.StepEnded(true, y[idx - 1], y[idx]);
                    acc -= Step;
                }
                if (idx < 1) continue; // no step run yet (a frame shorter than a step)
                double alpha = acc / Step;
                double drawn = y[idx - 1] + (y[idx] - y[idx - 1]) * alpha;
                double cOff = drawn + m54.Frame(drawn, frameDt, true), cOn = drawn + m55.Draw(alpha);
                if (!double.IsNaN(lastOff)) { off = System.Math.Max(off, System.Math.Abs(cOff - lastOff)); on = System.Math.Max(on, System.Math.Abs(cOn - lastOn)); }
                lastOff = cOff;
                lastOn = cOn;
                double t = (idx - 60 + alpha) * Step;
                if (double.IsNaN(onAtProbe) && t >= probeS) onAtProbe = cOn - drawn;
            }
        }

        [Test]
        public void SnapDownAndStepUpHalveMove54AtEveryRate()
        {
            foreach (double dy in new[] { -0.35, 0.229 })
                foreach (double fps in new[] { 60.0, 144.0, 240.0 })
                {
                    Render(Track(dy), fps, out double off, out double on, 0.3, out _);
                    Assert.That(off > 0.01, Is.True, $"move54 moves the camera on the step ({dy} m at {fps} fps: {off:0.000})");
                    Assert.That(on <= 0.5 * off, Is.True, $"step55's largest one-frame change {on:0.000} <= 50 % of move54's {off:0.000} ({dy} m at {fps} fps)");
                }
        }

        [Test]
        public void ConvergesAndIsTheSameAtEveryRate()
        {
            double at60 = 0, at240 = 0;
            foreach (double fps in new[] { 60.0, 144.0, 240.0 })
            {
                Render(Track(-0.35), fps, out _, out _, 0.3, out double rest);
                Assert.That(System.Math.Abs(rest) <= 0.05 * 0.35, Is.True, $"back within 5 % 0.3 s after the snap at {fps} fps ({rest:0.0000} m)");
                Render(Track(-0.35), fps, out _, out _, 0.1, out double at);
                if (fps == 60.0) at60 = at; else if (fps == 240.0) at240 = at;
            }
            Assert.That(System.Math.Abs(at60 - at240) <= 0.01, Is.True, $"the same offset 0.1 s after the snap at 60 and 240 fps ({at60:0.000} vs {at240:0.000})");
        }

        [Test]
        public void StaircaseIsCappedAndNothingElseIsHeld()
        {
            var s = new Move55Step(Step);
            s.StepEnded(true, 0, 0);
            double y = 0, maxAbs = 0;
            for (int i = 0; i < 40; i++)
            {
                double dy = i % 4 == 0 ? -0.3 : 0; // a 0.3 m snap every 4 steps (a fast staircase)
                s.StepEnded(true, y, y + dy);
                y += dy;
                maxAbs = System.Math.Max(maxAbs, System.Math.Abs(s.Offset));
            }
            Assert.That(maxAbs <= UnityTuning.Step55MaxOffset + 1e-9, Is.True, $"a staircase holds at most {UnityTuning.Step55MaxOffset} m ({maxAbs:0.000})");

            var j = new Move55Step(Step);
            j.StepEnded(true, 0, 0);
            j.StepEnded(false, 0, 0.2); // a jump's first step: its end in the air
            j.StepEnded(true, 0.2, 0.0); // a landing: its start in the air
            j.StepEnded(true, 0.0, 0.06); // a ramp's step (10 m/s up a 30 degree ramp): under Step55StepMin
            j.StepEnded(true, 0.06, 5.0); // a teleport
            Assert.That(j.StepsHeld == 0 && System.Math.Abs(j.Offset) < 1e-12, Is.True, $"a jump, a landing, a ramp and a teleport hold nothing (held {j.StepsHeld}, offset {j.Offset})");
        }

        [Test]
        public void Step55IsOffInTheShippedConfig()
        {
            Assert.That(Move55.On(Cfg()), Is.False, "design.n55.step55 ships off (NIGHT54's camera)");
        }
    }
}
