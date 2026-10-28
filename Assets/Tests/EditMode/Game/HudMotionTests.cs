// NIGHT18 U3-31.6 (builder H): the HUD's motion rules, measured the way the reel measures them (Tools/feel-schema.md M9, M2).
// - Every HUD entrance (HudEntrance18): its first drawn frame is never a pop-in (M9: >= 0.95 of rest scale AND alpha AND
//   < 40 px from rest), at any frame rate; it is whole in 90-300 ms; it overshoots about 10 %; it comes along the 12 deg
//   slant; the exit is 100 ms ease-in (IDENTITY.md's motion).
// - The kill marker (KillMarker18, GG's KILL MARKER switch ON): drawn on the kill's own frame at >= 48 px, an overshoot pop,
//   300-400 ms, never a pop-in.
// - UiTween.PopIn, the pop curve both use.
// No scene: pure functions only.
using System;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class HudMotionTests
    {
        const double E = 1e-6;

        // --- the entrance ---------------------------------------------------------------------------------------------------

        [Test]
        public void AnEntrancesFirstFrameIsNeverAPopIn()
        {
            foreach (double fps in new[] { 30.0, 60.0, 120.0, 144.0, 240.0 })
            {
                double age = HudEntrance18.FirstAge(1 / fps);
                float s = HudEntrance18.Scale(age), a = HudEntrance18.Alpha(age);
                float px = HudEntrance18.Slide(age) * UnityTuning.ThemeEnterDistance;
                Assert.IsFalse(HudEntrance18.PopIn(s, a, px), $"{fps} fps: scale {s}, alpha {a}, {px} px");
                Assert.Less(s, 0.95f, $"{fps} fps: the scale alone keeps it from popping (rest alpha may be low on a fading element)");
                Assert.Greater(a, 0.01f, $"{fps} fps: it is visible on its first frame (no added latency)");
            }
        }

        [Test]
        public void AnEntranceIsWholeIn90To300Ms()
        {
            double t = UnityTuning.Hud18EnterTime;
            Assert.GreaterOrEqual(t, 0.09);
            Assert.LessOrEqual(t, 0.30);
            Assert.AreEqual(1f, HudEntrance18.Scale(t), E);
            Assert.AreEqual(1f, HudEntrance18.Alpha(t), E);
            Assert.AreEqual(0f, HudEntrance18.Slide(t), E);
            Assert.IsTrue(HudEntrance18.Done(t));
            Assert.IsFalse(HudEntrance18.Done(t - 1 / 60.0));
            Assert.AreNotEqual(1f, HudEntrance18.Scale(t - 1 / 60.0), "still moving a frame before its end");
            Assert.AreEqual(1f, HudEntrance18.Scale(10), E, "at rest");
        }

        [Test]
        public void AnEntranceOvershootsAboutTenPercent()
        {
            float max = 0;
            for (int i = 0; i <= 300; i++) max = Math.Max(max, HudEntrance18.Scale(UnityTuning.Hud18EnterTime * i / 300.0));
            Assert.GreaterOrEqual(max, 1.05f, "an overshoot that shows");
            Assert.LessOrEqual(max, 1.15f, "not a bounce (too much is a failure too)");
            Assert.AreEqual(UnityTuning.Hud18EnterFrom, HudEntrance18.Scale(0), E);
            Assert.AreEqual(0f, HudEntrance18.Alpha(0), E);
            Assert.AreEqual(1f, HudEntrance18.Slide(0), E);
        }

        [Test]
        public void AnEntranceComesAlongTheSlant()
        {
            Vector2 v = HudEntrance18.Along(60);
            Assert.AreEqual(60f, v.magnitude, 1e-3);
            Assert.AreEqual(UnityTuning.ThemeSlantDeg, Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg, 1e-3, "12 deg off vertical, leaning forward");
            Assert.Greater(v.x, 0f);
            Assert.Greater(v.y, 0f);
        }

        [Test]
        public void AnExitIsAHundredMsEaseIn()
        {
            Assert.AreEqual(0.10, UnityTuning.Hud18ExitTime, E);
            Assert.AreEqual(1f, HudEntrance18.ExitAlpha(0), E);
            Assert.AreEqual(0f, HudEntrance18.ExitAlpha(UnityTuning.Hud18ExitTime), E);
            Assert.Greater(HudEntrance18.ExitAlpha(UnityTuning.Hud18ExitTime * 0.5), 0.5f, "ease-in: slow to start");
        }

        [Test]
        public void ThePopInRuleIsTheReels()
        {
            Assert.IsTrue(HudEntrance18.PopIn(1f, 1f, 0f), "full size, full alpha, in place");
            Assert.IsTrue(HudEntrance18.PopIn(1.04f, 1f, 0f), "within 5 % of rest");
            Assert.IsFalse(HudEntrance18.PopIn(1.6f, 1f, 0f), "round 3: the tool counts a first frame above 1.05x as an entrance (a slam)");
            Assert.IsFalse(HudEntrance18.PopIn(0.9f, 1f, 0f));
            Assert.IsFalse(HudEntrance18.PopIn(1f, 0.5f, 0f));
            Assert.IsFalse(HudEntrance18.PopIn(1f, 1f, 40f), "a 40 px slide is motion");
        }

        [Test]
        public void AFadeOnlyEntranceTakes90To300Ms()
        {
            // round 1 (build 3): the CLEAR's band and the rank-up streak only fade (the reel sees their rect, not their sweep):
            // NIGHT18's 60 ms fade read as a 33 ms entrance
            double first = HudEntrance18.FirstAge(1 / 60.0);
            Assert.Less(HudEntrance18.Fade(first), 0.95f * 0.95f, "its first frame is not a pop-in");
            Assert.Greater(HudEntrance18.Fade(first), 0.01f, "and visible");
            double at = -1;
            for (double t = 0; t < 1; t += 1 / 600.0)
                if (HudEntrance18.Fade(t) >= 0.95f)
                {
                    at = t;
                    break;
                }
            Assert.GreaterOrEqual(at, 0.09, "at 95 % no sooner than 90 ms");
            Assert.LessOrEqual(at, 0.30, "and by 300 ms");
            Assert.AreEqual(1f, HudEntrance18.Fade(UnityTuning.Hud18FadeInTime), E);
        }

        // --- the big banners' slam (round 2's picks: ROUND N CLEAR, the power-up banner) ------------------------------------

        [Test]
        public void ABigBannerSlamsDownAndIsAnEntranceOnItsFirstFrame()
        {
            // round 3's picks ("A1 nearly invisible"): drawn at age 0 on its first frame at about 1.6x and FULL alpha (M9 sees
            // an entrance by its scale: above 1.05x)
            Assert.AreEqual(UnityTuning.Hud18SlamFrom, HudSlam18.Scale(0), E);
            Assert.GreaterOrEqual(HudSlam18.Scale(0), 1.5f, "a slam from big");
            Assert.AreEqual(1f, HudSlam18.Alpha(0), E, "whole on its first frame");
            Assert.IsFalse(HudEntrance18.PopIn(HudSlam18.Scale(0), HudSlam18.Alpha(0), 0));
            Assert.IsTrue(HudSlam18.White(0) && HudSlam18.White(UnityTuning.Hud18SlamWhite - 0.001), "white-hot on its first frames");
            Assert.IsFalse(HudSlam18.White(UnityTuning.Hud18SlamWhite));
            float last = float.MaxValue;
            for (double t = 0; t <= UnityTuning.Hud18SlamTime; t += 1 / 240.0)
            {
                Assert.LessOrEqual(HudSlam18.Scale(t), last + 1e-6f, "straight down, no bounce");
                last = HudSlam18.Scale(t);
            }
            Assert.AreEqual(1f, HudSlam18.Scale(UnityTuning.Hud18SlamTime), E);
            Assert.AreEqual(1f, HudSlam18.Alpha(UnityTuning.Hud18SlamTime), E);
        }

        [Test]
        public void ABigBannerSettlesIn90To150Ms()
        {
            double settled = -1;
            for (double t = 0; t < 1; t += 1 / 1000.0)
                if (Math.Abs(HudSlam18.Scale(t) - 1f) < 0.05f && HudSlam18.Alpha(t) >= 0.95f)
                {
                    settled = t;
                    break;
                }
            Assert.GreaterOrEqual(settled, 0.09);
            Assert.LessOrEqual(settled, 0.15);
        }

        // --- the kill marker ------------------------------------------------------------------------------------------------

        [Test]
        public void TheKillMarkerKeepsItsCentreClearForA20mBody()
        {
            // round 2's picks ("at 20 m the marker hides the whole target"): the wedges' INK stroke starts outside ~40 px at
            // rest, and stays outside 30 px (a 60 px tall body's half) even on its smaller first frame
            float hollow = UnityTuning.Hud18KillMarkerInner - UnityTuning.Hud18KillMarkerOutline;
            Assert.GreaterOrEqual(hollow, 38f);
            Assert.GreaterOrEqual(hollow * KillMarker18.Scale(0), 30f);
            Assert.Greater(UnityTuning.Hud18KillMarkerOuter, UnityTuning.Hud18KillMarkerInner + 20f, "wedges long enough to read");
            Assert.GreaterOrEqual(UnityTuning.Hud18KillMarkerPx * 0.5f, UnityTuning.Hud18KillMarkerOuter * 0.70710678f + UnityTuning.Hud18KillMarkerOutline, "the tips and their stroke fit the box");
        }

        [Test]
        public void TheKillMarkersPopSettlesIn90To130Ms()
        {
            // round 1 (build 3): its scale was within 5 % of rest by 83 ms (an entrance under 90 ms)
            double settled = -1;
            for (double t = 0; t < UnityTuning.Hud18KillMarkerTime; t += 1 / 1000.0)
            {
                bool still = true;
                for (double u = t; u < UnityTuning.Hud18KillMarkerTime; u += 1 / 1000.0)
                    if (Math.Abs(KillMarker18.Scale(u) - 1f) >= 0.05f)
                    {
                        still = false;
                        break;
                    }
                if (still)
                {
                    settled = t;
                    break;
                }
            }
            Assert.GreaterOrEqual(settled, 0.09);
            Assert.LessOrEqual(settled, 0.13);
        }

        [Test]
        public void TheKillMarkerIsDrawnOnTheKillsFrameAtLeast48Px()
        {
            Assert.IsTrue(KillMarker18.Shown(0));
            Assert.GreaterOrEqual(UnityTuning.Hud18KillMarkerPx * KillMarker18.Scale(0), 48f, "its first frame");
            Assert.AreEqual(1f, KillMarker18.Alpha(0), E, "whole on its first frame");
            Assert.Less(KillMarker18.Scale(0), 0.95f, "and still not a pop-in: it grows");
            float min = float.MaxValue;
            for (double t = 0; KillMarker18.Shown(t); t += 1 / 240.0) min = Math.Min(min, UnityTuning.Hud18KillMarkerPx * KillMarker18.Scale(t));
            Assert.GreaterOrEqual(min, 48f, "never under 48 px while shown");
        }

        [Test]
        public void TheKillMarkerPopsPastItsSizeAndSettles()
        {
            float max = 0;
            double maxAt = 0;
            for (double t = 0; t < UnityTuning.Hud18KillMarkerIn; t += 1 / 1000.0)
            {
                float s = KillMarker18.Scale(t);
                if (s > max)
                {
                    max = s;
                    maxAt = t;
                }
            }
            Assert.GreaterOrEqual(max, 1.12f, "an overshoot pop");
            Assert.LessOrEqual(maxAt, 0.06, "its peak within the first few frames");
            Assert.AreEqual(1f, KillMarker18.Scale(UnityTuning.Hud18KillMarkerIn), E);
            Assert.AreEqual(1f, KillMarker18.Scale(UnityTuning.Hud18KillMarkerTime - 0.001), E, "at rest size while it fades (rest scale 1)");
            Assert.AreEqual(UnityTuning.Hud18KillMarkerTurn, KillMarker18.Turn(0), E);
            Assert.AreEqual(0f, KillMarker18.Turn(UnityTuning.Hud18KillMarkerIn), E);
        }

        [Test]
        public void TheKillMarkerLasts300To400Ms()
        {
            double life = UnityTuning.Hud18KillMarkerTime;
            Assert.GreaterOrEqual(life, 0.30);
            Assert.LessOrEqual(life, 0.40);
            Assert.IsTrue(KillMarker18.Shown(life - 0.001));
            Assert.IsFalse(KillMarker18.Shown(life));
            Assert.IsFalse(KillMarker18.Shown(-0.001));
            Assert.AreEqual(1f, KillMarker18.Alpha(life - UnityTuning.Hud18KillMarkerOut), E, "whole until its exit");
            Assert.Less(KillMarker18.Alpha(life - 0.001), 0.05f, "faded by its end");
            Assert.Greater(KillMarker18.Alpha(life - UnityTuning.Hud18KillMarkerOut * 0.5), 0.5f, "an ease-in exit");
        }

        // --- HUD STYLE ATTACHED (backlog 5; hud.js :819-826) -----------------------------------------------------------------

        [Test]
        public void OnlyTheAttachedStyleRides()
        {
            Assert.IsTrue(HudAttach18.Attached("attached"));
            Assert.IsFalse(HudAttach18.Attached("classic"));
            Assert.IsFalse(HudAttach18.Attached(null));
        }

        [Test]
        public void TheBlockMovesWithTheGunAtTheWebsStrength()
        {
            // hud.js: tx = x * hudAttach * attachPxPerMeter, ty = -y * k in CSS (y down): uGUI's y is up, so +y * k
            Vector2 o = HudAttach18.Offset(0.01, -0.02, 0.3, 1500);
            Assert.AreEqual(4.5f, o.x, 1e-4);
            Assert.AreEqual(-9f, o.y, 1e-4);
            Vector2 r = HudAttach18.Offset(0.0101, 0, 0.3, 1500);
            Assert.AreEqual(4.5f, r.x, 1e-4, "written at 0.1 px (the web's toFixed(1))");
            Assert.AreEqual(Vector2.zero, HudAttach18.Offset(0, 0, 0.3, 1500));
        }

        [Test]
        public void TheBlockRollsWithTheGunTheOtherWayRoundInUnity()
        {
            // hud.js: rotate(rollDeg * hudAttach) in CSS (clockwise +); uGUI's z rotation is counter-clockwise +
            Assert.AreEqual(-0.6f, HudAttach18.Roll(2, 0.3), 1e-4);
            Assert.AreEqual(0.6f, HudAttach18.Roll(-2, 0.3), 1e-4);
            Assert.AreEqual(-0.33f, HudAttach18.Roll(1.1, 0.3), 1e-4, "written at 0.01 deg (the web's toFixed(2))");
        }

        // --- the pop curve --------------------------------------------------------------------------------------------------

        [Test]
        public void PopInRisesToItsPeakThenSettles()
        {
            Assert.AreEqual(0.6f, UiTween.PopIn(0, 0.6f, 1.2f, 0.4), E);
            Assert.AreEqual(1.2f, UiTween.PopIn(0.4, 0.6f, 1.2f, 0.4), E);
            Assert.AreEqual(1f, UiTween.PopIn(1, 0.6f, 1.2f, 0.4), E);
            Assert.AreEqual(1f, UiTween.PopIn(2, 0.6f, 1.2f, 0.4), E, "clamped");
            Assert.AreEqual(0.6f, UiTween.PopIn(-1, 0.6f, 1.2f, 0.4), E, "clamped");
            float last = 0;
            for (int i = 0; i <= 40; i++)
            {
                float v = UiTween.PopIn(0.4 * i / 40, 0.6f, 1.2f, 0.4);
                Assert.GreaterOrEqual(v, last - 1e-6f, "rising to the peak");
                last = v;
            }
            for (int i = 1; i <= 60; i++)
            {
                float v = UiTween.PopIn(0.4 + 0.6 * i / 60, 0.6f, 1.2f, 0.4);
                Assert.LessOrEqual(v, last + 1e-6f, "settling from the peak");
                last = v;
            }
        }
    }
}
