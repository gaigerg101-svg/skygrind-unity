// NIGHT74 builder L (pads74; Game/Pad74Math.cs and Game/Pan74PadsMath.cs: the pure parts, no UnityEngine, so a console runs them too). Pinned:
//   1. the sprite's profile: 1 at the centre, exactly 0 at and past the quad's edge, never rising outward (no step at the border: the whole point against the shipped hard squares)
//   2. the sheet's brightness steps: dim at birth and death, whole in the middle, rising then falling, no step above a sixth of the peak
//   3. the sheet's bytes: the right size, the centre of each frame at its level, every frame's border rows and columns (nearly) 0 so no mip level carries one frame's light into the next
//   4. a seed per pad: fixed, never shared by two pads
//   5. the pad regions: Rect (the top-down flip, a point behind the camera, a rectangle off the picture), Corners, SegmentHitsBox (through, beside, short of, over, the eps),
//      Hidden (both sight lines must be blocked), the line and header text n74-flick.py's read_pads reads
// The view parts (the texture, the material, the steady twin, Apply) are in Pad74Tests.cs (they need the Editor).
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Pad74MathTests
    {
        const int Texels = 32, Frames = 32;
        const float FadeIn = 0.30f, FadeOut = 0.45f; // UnityTuning.N74PadFadeIn / FadeOut (Pad74Tests.TheNumbers_... pins that these are the tuned ones)

        [Test]
        public void Falloff_IsOneAtTheCentreAndNothingAtTheEdge()
        {
            Assert.AreEqual(1f, Pad74Math.Falloff(0f), 1e-6f);
            Assert.AreEqual(0f, Pad74Math.Falloff(1f), 1e-6f, "exactly 0 at the quad's edge: no step");
            Assert.AreEqual(0f, Pad74Math.Falloff(1.5f), 0f);
            float last = 2f;
            for (int i = 0; i <= 100; i++)
            {
                float v = Pad74Math.Falloff(i / 100f);
                Assert.GreaterOrEqual(v, 0f);
                Assert.LessOrEqual(v, last + 1e-6f, "never rises going out (r " + i / 100f + ")");
                last = v;
            }
        }

        [Test]
        public void FrameLevel_IsDimAtTheEndsWholeInTheMiddleAndSteady()
        {
            Assert.Less(Pad74Math.FrameLevel(0, Frames, FadeIn, FadeOut), 0.25f, "dim at birth");
            Assert.Less(Pad74Math.FrameLevel(Frames - 1, Frames, FadeIn, FadeOut), 0.25f, "dim at death");
            float peak = 0f;
            int peakAt = -1;
            for (int i = 0; i < Frames; i++)
            {
                float v = Pad74Math.FrameLevel(i, Frames, FadeIn, FadeOut);
                Assert.GreaterOrEqual(v, 0f);
                Assert.LessOrEqual(v, 1f);
                if (v > peak) { peak = v; peakAt = i; }
            }
            Assert.AreEqual(1f, peak, 1e-6f, "whole between the fades");
            Assert.GreaterOrEqual(peakAt, Frames / 6, "the peak is in the middle of the life, not at an end");
            Assert.LessOrEqual(peakAt, Frames * 3 / 4);
            float rise = 0f, fall = 0f;
            for (int i = 1; i < Frames; i++)
            {
                float step = Pad74Math.FrameLevel(i, Frames, FadeIn, FadeOut) - Pad74Math.FrameLevel(i - 1, Frames, FadeIn, FadeOut);
                if (i <= peakAt) { Assert.GreaterOrEqual(step, -1e-6f, "rising up to the peak (frame " + i + ")"); rise = Math.Max(rise, step); }
                else { Assert.LessOrEqual(step, 1e-6f, "falling after it (frame " + i + ")"); fall = Math.Max(fall, -step); }
            }
            Assert.Less(rise, 1f / 6f, "no brightness step above a sixth of the peak on the way up");
            Assert.Less(fall, 1f / 6f, "nor on the way down");
            Assert.AreEqual(0f, Pad74Math.FrameLevel(3, 0, FadeIn, FadeOut), 0f, "no frames, no light");
            Assert.AreEqual(Pad74Math.FrameLevel(0, Frames, FadeIn, FadeOut), Pad74Math.FrameLevel(-4, Frames, FadeIn, FadeOut), 0f, "an index below the sheet reads its first frame");
        }

        [Test]
        public void Sheet_HasTheRightBytesAndKeepsEachFrameToItself()
        {
            byte[] px = Pad74Math.Sheet(Texels, Frames, FadeIn, FadeOut);
            int w = Texels * Frames;
            Assert.AreEqual(w * Texels * 4, px.Length);
            for (int k = 0; k < Frames; k++)
            {
                float level = Pad74Math.FrameLevel(k, Frames, FadeIn, FadeOut);
                int centre = ((Texels / 2) * w + k * Texels + Texels / 2) * 4;
                float r = (float)Math.Sqrt(2.0) * 0.5f / (Texels * 0.5f);
                int expect = (int)Math.Round(level * Pad74Math.Falloff(r) * 255f);
                Assert.AreEqual(expect, px[centre], 1, "frame " + k + "'s centre");
                for (int y = 0; y < Texels; y++)
                {
                    Assert.LessOrEqual(px[(y * w + k * Texels) * 4], 2, "frame " + k + "'s first column, row " + y);
                    Assert.LessOrEqual(px[(y * w + k * Texels + Texels - 1) * 4], 2, "frame " + k + "'s last column, row " + y);
                    Assert.AreEqual(px[(y * w + k * Texels + 3) * 4], px[(y * w + k * Texels + 3) * 4 + 3], 0, "alpha is the same as rgb");
                }
                for (int x = 0; x < Texels; x++)
                {
                    Assert.LessOrEqual(px[(k * Texels + x) * 4], 2, "frame " + k + "'s first row, column " + x);
                    Assert.LessOrEqual(px[((Texels - 1) * w + k * Texels + x) * 4], 2, "frame " + k + "'s last row, column " + x);
                }
            }
        }

        [Test]
        public void SeedFor_IsFixedAndNeverSharedByTwoPads()
        {
            var seen = new HashSet<uint>();
            for (int i = 0; i < 64; i++) Assert.IsTrue(seen.Add(Pad74Math.SeedFor(7401u, i)), "pad " + i);
            Assert.AreEqual(Pad74Math.SeedFor(7401u, 3), Pad74Math.SeedFor(7401u, 3));
            Assert.AreEqual(7401u, Pad74Math.SeedFor(7401u, 0));
            Assert.AreEqual("pads74", Pad74Math.Row);
        }

        [Test]
        public void BeamFactor_IsSoftFarFadedNearAndNeverGone()
        {
            const float scale = 0.5f, near = 1.0f, far = 4.0f, floor = 0.4f; // UnityTuning.N74PadBeam* (Pad74Tests pins that these are the tuned ones)
            Assert.AreEqual(scale, Pad74Math.BeamFactor(scale, 50, near, far, floor), 1e-6f, "far: the softened beam, whole");
            Assert.AreEqual(scale, Pad74Math.BeamFactor(scale, far, near, far, floor), 1e-6f, "at the far distance");
            Assert.AreEqual(scale * floor, Pad74Math.BeamFactor(scale, near, near, far, floor), 1e-6f, "at the near distance: the floor");
            Assert.AreEqual(scale * floor, Pad74Math.BeamFactor(scale, 0, near, far, floor), 1e-6f, "on the axis: the floor, never less");
            float last = -1f;
            for (int i = 0; i <= 80; i++)
            {
                float v = Pad74Math.BeamFactor(scale, i * 0.1, near, far, floor);
                Assert.GreaterOrEqual(v, last - 1e-6f, "never dimmer farther out (" + i * 0.1 + " m)");
                Assert.GreaterOrEqual(v, scale * floor - 1e-6f);
                Assert.LessOrEqual(v, scale + 1e-6f);
                last = v;
            }
            Assert.Greater(Pad74Math.BeamFactor(scale, 2.5, near, far, floor), scale * floor, "between: a smooth rise");
            Assert.Less(Pad74Math.BeamFactor(scale, 2.5, near, far, floor), scale);
            Assert.AreEqual(2.2f * 0.7f, 2.2f * 0.7f * Pad74Math.BeamFactor(1f, 99, near, far, 1f), 1e-6f, "scale 1 and floor 1 is the shipped number");
        }

        [Test]
        public void Rect_FlipsTheRowsAndRefusesWhatIsBehindOrOffTheFrame()
        {
            var r = new int[4];
            // a box on the screen's lower left: Unity's y counts up from the bottom, the file's rows from the top
            Assert.IsTrue(Pan74PadsMath.Rect(new double[] { 100.2, 300.7 }, new double[] { 50.0, 200.5 }, new[] { false, false }, 1920, 1080, r));
            Assert.AreEqual(new[] { 100, 1080 - 201, 301, 1030 }, r, "x0, y0 (flipped), x1, y1: floor and ceiling");
            Assert.IsFalse(Pan74PadsMath.Rect(new double[] { 100, 300 }, new double[] { 50, 200 }, new[] { false, true }, 1920, 1080, r), "a corner behind the camera: no rectangle");
            Assert.IsFalse(Pan74PadsMath.Rect(new double[] { 2000, 2100 }, new double[] { 50, 200 }, new[] { false, false }, 1920, 1080, r), "right of the picture");
            Assert.IsFalse(Pan74PadsMath.Rect(new double[] { 100, 300 }, new double[] { -300, -20 }, new[] { false, false }, 1920, 1080, r), "below the picture");
            Assert.IsTrue(Pan74PadsMath.Rect(new double[] { -50, 40 }, new double[] { 500, 700 }, new[] { false, false }, 1920, 1080, r), "partly off the left edge still counts");
            Assert.AreEqual(-50, r[0], "unclipped (n74-flick.py clips to its grid)");
        }

        static bool OneOf(double v, double a, double b) => v == a || v == b;

        [Test]
        public void Corners_AreTheEightOfTheBox()
        {
            var o = new double[24];
            Pan74PadsMath.Corners(10, 2, -5, 1.5, 0, 4, o);
            var seen = new HashSet<string>();
            for (int i = 0; i < 8; i++)
            {
                Assert.IsTrue(OneOf(o[i * 3], 8.5, 11.5), "x " + o[i * 3]);
                Assert.IsTrue(OneOf(o[i * 3 + 1], 2.0, 6.0), "y " + o[i * 3 + 1]);
                Assert.IsTrue(OneOf(o[i * 3 + 2], -6.5, -3.5), "z " + o[i * 3 + 2]);
                seen.Add(o[i * 3] + "/" + o[i * 3 + 1] + "/" + o[i * 3 + 2]);
            }
            Assert.AreEqual(8, seen.Count, "eight distinct corners");
        }

        [Test]
        public void SegmentHitsBox_AndHidden()
        {
            double[] wall = { -1, 0, 4, 1, 5, 5 }; // a wall 2 m wide, 5 m high, 1 m thick, 4 m ahead
            Assert.IsTrue(Pan74PadsMath.SegmentHitsBox(0, 1, 0, 0, 1, 10, wall, 0.05), "straight through");
            Assert.IsFalse(Pan74PadsMath.SegmentHitsBox(3, 1, 0, 3, 1, 10, wall, 0.05), "beside it");
            Assert.IsFalse(Pan74PadsMath.SegmentHitsBox(0, 1, 0, 0, 1, 3, wall, 0.05), "stopping short of it");
            Assert.IsFalse(Pan74PadsMath.SegmentHitsBox(0, 6, 0, 0, 6, 10, wall, 0.05), "over the top");
            Assert.IsFalse(Pan74PadsMath.SegmentHitsBox(0, 1, 0, 0, 1, 4.02, wall, 0.05), "a pad against the wall's face is not hidden by it (the eps)");
            Assert.IsTrue(Pan74PadsMath.SegmentHitsBox(0, 1, 0, 0, 1, 4.2, wall, 0.05), "past the eps it is");
            var boxes = new List<double[]> { wall, new double[] { 0, 0, 0, 1, 1, 1, 1 } };
            Assert.IsTrue(Pan74PadsMath.Hidden(0, 1, 0, 0, 0, 10, 1.5, 3.9, boxes, 0.05), "both lines pass through the wall");
            var lowBox = new List<double[]> { new double[] { -1, 0, 4, 1, 2, 5 } }; // 2 m high: the eye at 1 m looks through it at the pad's foot but over it at the pad's top
            Assert.IsTrue(Pan74PadsMath.SegmentHitsBox(0, 1, 0, 0, 0, 10, lowBox[0], 0.05), "the low line is blocked");
            Assert.IsFalse(Pan74PadsMath.SegmentHitsBox(0, 1, 0, 0, 3.9, 10, lowBox[0], 0.05), "the high line is not");
            Assert.IsFalse(Pan74PadsMath.Hidden(0, 1, 0, 0, 0, 10, 0.0, 3.9, lowBox, 0.05), "only one line blocked: the pad counts as seen");
            Assert.IsFalse(Pan74PadsMath.Hidden(0, 1, 0, 0, 0, 10, 1.5, 3.9, new List<double[]>(), 0.05), "no boxes, nothing hidden");
        }

        [Test]
        public void TheLineAndTheHeader_AreWhatTheMeterReads()
        {
            Assert.AreEqual("P74PADS 1 1920 1080", Pan74PadsMath.Header(1920, 1080));
            Assert.AreEqual("f 12 3 1 10 20 30 40 11 21 29 39", Pan74PadsMath.Line(12, 3, true, new[] { 10, 20, 30, 40 }, new[] { 11, 21, 29, 39 }));
            Assert.AreEqual("f 0 1 0 0 0 0 0 0 0 0 0", Pan74PadsMath.Line(0, 1, false, new[] { 10, 20, 30, 40 }, new[] { 11, 21, 29, 39 }), "not seen: zeros");
        }
    }
}
