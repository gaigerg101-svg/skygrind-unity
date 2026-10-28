// NIGHT17 U3-30.7 (builder H): the rebuilt HUD's pure rules. The pops (merge, sized by value, the 0 -> 1.3 -> 1 / hold /
// rise-and-fade envelope: PopMath), the style meter's cyan value ladder, its word and its knock-down (StyleLadder), the
// multiplier chip (StyleChip), the stamina bar's segments, its boost floor and the full edge (StaminaBarMath), HUD SCALE
// (HudScaleMath) and the round's hero number (RoundHeroMath). No scene: pure functions only.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Hud17Tests
    {
        const double E = 1e-9;

        // --- the pops ---------------------------------------------------------------------------------------------------------

        [Test]
        public void PopsAreSizedByWhatTheyPay()
        {
            // round 2's reviewer: 36 px a hit, 56 a kill, 80 a headshot or punch kill, on the type scale: S32 / S56 / S75
            Assert.AreEqual(32f, PopMath.SizeOfTier(0), "a hit");
            Assert.AreEqual(56f, PopMath.SizeOfTier(1), "a kill");
            Assert.AreEqual(75f, PopMath.SizeOfTier(2), "a headshot or punch kill");
            Assert.AreEqual(32f, PopMath.SizeOfTier(-1));
            Assert.AreEqual(75f, PopMath.SizeOfTier(9));
            Assert.AreEqual(0, PopMath.TierOf("hit"));
            Assert.AreEqual(1, PopMath.TierOf("kill"));
            Assert.AreEqual(0, PopMath.TierOf("parry"));
            Assert.AreEqual(0, PopMath.TierOf(null));
        }

        [Test]
        public void ABurstTakesItsBiggestGainsTier()
        {
            var s = new PopStack();
            s.Add(10, 0);
            Assert.AreEqual(0, s.tier);
            s.Add(60, 1);
            Assert.AreEqual(1, s.tier, "a kill in the burst: the kill's size");
            s.Add(10, 0);
            Assert.AreEqual(1, s.tier, "never smaller");
            s.Raise(2);
            Assert.AreEqual(2, s.tier, "the hit that made it a headshot kill raises it");
            Assert.AreEqual(80, s.sum, 1e-9);
        }

        [Test]
        public void AModChipShowsTheWholeWord()
        {
            // round 1's fix list: "BOU" was the web's three-letter cut; never cut a word
            Assert.AreEqual("BOUNCER", ModChip.Word("Bouncer", 1));
            Assert.AreEqual("VAMPIRE×2", ModChip.Word("VAMPIRE", 2));
            Assert.AreEqual("GLASS CANNON", ModChip.Word("Glass Cannon", 1));
            Assert.AreEqual("", ModChip.Word(null, 1));
        }

        [Test]
        public void TheStaminaBarKeepsItsWidthWhateverItsSegments()
        {
            // round 1's fix list: a clear 160 px bar; VIGOR's two bars split it, never shrink it to a sliver
            Assert.AreEqual(160f, StaminaBarMath.SegmentWidth(160, 1, 6), 1e-4);
            Assert.AreEqual(77f, StaminaBarMath.SegmentWidth(160, 2, 6), 1e-4);
            Assert.AreEqual(49.333f, StaminaBarMath.SegmentWidth(160, 3, 6), 1e-3);
            Assert.AreEqual(160f, StaminaBarMath.SegmentWidth(160, 0, 6), 1e-4, "never none");
        }

        [Test]
        public void APopMergesOnlyWhileYoung()
        {
            // round 5: pops landing within 150 ms become one summed number (two kills ~17 ms apart printed "+12025")
            Assert.IsTrue(PopMath.Merges(true, 0.0));
            Assert.IsTrue(PopMath.Merges(true, 0.017), "two kills a frame apart");
            Assert.IsTrue(PopMath.Merges(true, 0.149));
            Assert.IsFalse(PopMath.Merges(true, 0.15), "150 ms: a new pop");
            Assert.IsFalse(PopMath.Merges(false, 0.01), "nothing live to merge into");
        }

        [Test]
        public void ANewBurstSendsTheOldOneToTheCounter()
        {
            // round 3's reviewer ("+120" and "+125" overprinted): a pop still holding when the next burst starts flies at once
            Assert.AreEqual(UnityTuning.Hud17PopIn + UnityTuning.Hud17PopHold, PopMath.FlyNow(0.2, UnityTuning.Hud17PopHold), 1e-9, "skips the rest of its hold");
            Assert.AreEqual(0.6, PopMath.FlyNow(0.6, UnityTuning.Hud17PopHold), 1e-9, "already flying: unchanged");
            Assert.AreEqual(0, PopMath.Fly(PopMath.FlyNow(0.2, UnityTuning.Hud17PopHold)), 1e-9, "its flight starts this frame");
        }

        [Test]
        public void ABurstShowsOneRunningSum()
        {
            // round 1's reviewer: ONE running "+285" per burst, not "+150 x3"
            var s = new PopStack();
            s.Add(150);
            Assert.AreEqual(1, s.count);
            Assert.AreEqual(150, s.Shown, E);
            s.Add(95);
            s.Add(40);
            Assert.AreEqual(3, s.count);
            Assert.AreEqual(285, s.Shown, E, "+285");
            Assert.AreEqual(285, s.sum, E);
            var h = new PopStack();
            h.Add(75);
            h.Add(75);
            Assert.AreEqual(150, h.Shown, E, "a horde's +75 +75 reads +150 (no x2)");
        }

        [Test]
        public void APopPunchesInFromSmall()
        {
            // round 5: enter at 0.7x, overshoot to 1.3x, settle at 1.0x within 120 ms (no big-white-to-small jump)
            Assert.AreEqual(0.7, PopMath.Scale(0), 1e-6, "from 0.7x");
            Assert.AreEqual(1.3, PopMath.Scale(0.06), 1e-6, "the overshoot at half the entrance");
            Assert.AreEqual(1, PopMath.Scale(0.12), 1e-6, "settled at 120 ms");
            Assert.AreEqual(1, PopMath.Scale(0.5), E);
            Assert.Greater(PopMath.Scale(0.03), 0.7);
            Assert.Less(PopMath.Scale(0.03), 1.3);
            Assert.Greater(PopMath.Scale(0.09), 1.0, "on its way down from the overshoot");
        }

        [Test]
        public void APopHoldsThenFliesIntoTheCounter()
        {
            Assert.AreEqual(0.72, PopMath.Life, 1e-9, "120 + 350 + 250 ms");
            Assert.AreEqual(0, PopMath.Fly(0.4), E, "still in the hold");
            Assert.AreEqual(0, PopMath.Fly(0.47), 1e-9, "the hold's end");
            Assert.AreEqual(1, PopMath.Fly(0.72), 1e-9, "at the counter");
            double mid = PopMath.Fly(0.595);
            Assert.Greater(mid, 0);
            Assert.Less(mid, 0.5, "ease-in: slow off the hold, fast into the counter");
            Assert.IsFalse(PopMath.Arrived(0.71));
            Assert.IsTrue(PopMath.Arrived(0.72));
        }

        // --- the round's clear ------------------------------------------------------------------------------------------------

        [Test]
        public void TheClearSlamsInFromItsPlaceHoldsAndGoes()
        {
            // round 5: "ROUND 1 CLEAR" slams 1.6x -> 1.0x in 140 ms from its final place, held 1.2 s, then out (200 ms)
            Assert.AreEqual(1.6, RoundClearMath.Scale(0), 1e-6);
            Assert.AreEqual(1, RoundClearMath.Scale(0.14), 1e-6);
            Assert.Less(RoundClearMath.Scale(0.07), 1.6);
            Assert.Greater(RoundClearMath.Scale(0.07), 1.0, "a slam: no overshoot under 1");
            Assert.AreEqual(1, RoundClearMath.Alpha(1.34), 1e-9, "held 1.2 s after its entrance");
            double fading = RoundClearMath.Alpha(1.44);
            Assert.Greater(fading, 0);
            Assert.Less(fading, 1);
            Assert.AreEqual(0, RoundClearMath.Alpha(1.54), 1e-9);
            Assert.IsFalse(RoundClearMath.Done(1.53));
            Assert.IsTrue(RoundClearMath.Done(1.54));
        }

        // --- the style meter ------------------------------------------------------------------------------------------------

        [Test]
        public void TheRankLadderIsThreeCyanValueSteps()
        {
            // six ranks (D C B A S SS): pale, pale, cyan, cyan, hot, hot
            int[] want = { 0, 0, 1, 1, 2, 2 };
            for (int r = 0; r < 6; r++) Assert.AreEqual(want[r], StyleLadder.Step(r, 6), $"rank {r}");
            Assert.AreEqual(0, StyleLadder.Step(-1, 6));
            Assert.AreEqual(2, StyleLadder.Step(9, 6));
            Assert.AreEqual(0, StyleLadder.Step(0, 0));
        }

        [Test]
        public void TheRankUpSlamHoldsWhiteHotThenSettlesWithAnOvershoot()
        {
            // round 3's reviewer: 2.2x, white-hot, HELD 100 ms, then ease-out-back to 1.0 over 160 ms
            Assert.AreEqual(2.2, StyleLadder.SlamScale(0), 1e-6);
            Assert.AreEqual(2.2, StyleLadder.SlamScale(0.099), 1e-6, "held");
            Assert.IsTrue(StyleLadder.SlamWhite(0.05));
            Assert.IsFalse(StyleLadder.SlamWhite(0.1), "white only through the hold");
            Assert.Less(StyleLadder.SlamScale(0.2), 1.2, "most of the way down");
            Assert.AreEqual(1, StyleLadder.SlamScale(0.26), 1e-6, "settled at 260 ms");
            Assert.AreEqual(1, StyleLadder.SlamScale(double.MaxValue), 1e-6);
            double minS = 9;
            for (int i = 0; i <= 160; i++) minS = System.Math.Min(minS, StyleLadder.SlamScale(0.1 + i * 0.001));
            Assert.Less(minS, 1.0, "ease-out-back overshoots under 1");
        }

        [Test]
        public void OnlyNamedEventsMakeACallout()
        {
            // round 3's reviewer: never a plain "+ KILL"; DOUBLE and + MULTIKILL say the same thing: the streak's name stays
            Assert.IsFalse(StyleLadder.Named("KILL"));
            Assert.IsFalse(StyleLadder.Named("HIT"));
            Assert.IsFalse(StyleLadder.Named("MULTIKILL"));
            Assert.IsFalse(StyleLadder.Named(null));
            Assert.IsTrue(StyleLadder.Named("HEADSHOT"));
            Assert.IsTrue(StyleLadder.Named("CLOSE CALL"));
            Assert.IsTrue(StyleLadder.Named("WALLKILL"));
        }

        [Test]
        public void ARepeatedCalloutCountsInsteadOfListingTwice()
        {
            // round 5: "+ HEADSHOT" listed twice becomes "+ HEADSHOT x2": the live line of the same name is found
            string head = "+ HEADSHOT", wall = "+ WALLKILL";
            var lines = new[] { wall, head, null };
            Assert.AreEqual(1, StyleLadder.Repeats(lines, 2, head));
            Assert.AreEqual(0, StyleLadder.Repeats(lines, 2, wall));
            Assert.AreEqual(-1, StyleLadder.Repeats(lines, 1, head), "only the live lines count");
            Assert.AreEqual(-1, StyleLadder.Repeats(lines, 2, "+ SLAMDOWN"));
        }

        [Test]
        public void ACalloutSlidesInFromTheRight()
        {
            Assert.AreEqual(UnityTuning.Hud17CalloutSlide, StyleLadder.CalloutX(0), 1e-4, "from the right");
            Assert.AreEqual(0, StyleLadder.CalloutX(0.12), 1e-4, "in place at 120 ms");
            Assert.AreEqual(0, StyleLadder.CalloutX(1), 1e-4);
            Assert.Less(StyleLadder.CalloutX(0.06), UnityTuning.Hud17CalloutSlide * 0.5f, "ease-out: more than half way at half time");
        }

        [Test]
        public void TheWordShowsForSixHundredMsAfterARankUp()
        {
            Assert.IsTrue(StyleLadder.WordShown(0));
            Assert.IsTrue(StyleLadder.WordShown(0.59));
            Assert.IsFalse(StyleLadder.WordShown(0.6));
            Assert.IsFalse(StyleLadder.WordShown(double.MaxValue));
        }

        [Test]
        public void ADropKnocksTheLetterDownThenItSettles()
        {
            Assert.AreEqual(-UnityTuning.Hud17StyleKnockPx, StyleLadder.KnockY(0), 1e-6);
            Assert.AreEqual(0, StyleLadder.KnockY(UnityTuning.Hud17StyleKnockTime), 1e-6);
            Assert.AreEqual(0, StyleLadder.KnockY(double.MaxValue), E);
            double half = StyleLadder.KnockY(UnityTuning.Hud17StyleKnockTime * 0.5);
            Assert.Less(half, 0);
            Assert.Greater(half, -UnityTuning.Hud17StyleKnockPx * 0.5, "ease-out: more than half way back at half time");
        }

        [Test]
        public void TheChipShowsInAMultiplierStateAndLingersDraining()
        {
            Assert.IsTrue(StyleChip.Live(true, "AIR", 1.2));
            Assert.IsTrue(StyleChip.Live(true, "SLIDE", 1.2));
            Assert.IsFalse(StyleChip.Live(true, "GROUND", 1.2), "not a multiplier state");
            Assert.IsFalse(StyleChip.Live(true, "AIR", 1.0), "no multiplier to show");
            Assert.IsFalse(StyleChip.Live(false, "AIR", 1.2), "no meter");
            Assert.AreEqual(1, StyleChip.Drain(0), E);
            Assert.AreEqual(0.5, StyleChip.Drain(UnityTuning.Hud17ChipLinger * 0.5), 1e-9);
            Assert.AreEqual(0, StyleChip.Drain(UnityTuning.Hud17ChipLinger), E);
        }

        // --- U3-30.10 (b): kill streaks as named lines ----------------------------------------------------------------------

        [Test]
        public void KillsCountInsideTheMultikillWindow()
        {
            var times = new double[16];
            int n = 0;
            Assert.AreEqual(1, KillStreak.Push(times, ref n, 0.0, 1.0));
            Assert.AreEqual(2, KillStreak.Push(times, ref n, 0.5, 1.0));
            Assert.AreEqual(3, KillStreak.Push(times, ref n, 1.0, 1.0), "the window's edge counts (the core's <=)");
            Assert.AreEqual(2, KillStreak.Push(times, ref n, 1.6, 1.0), "the first two are out");
            Assert.AreEqual(1, KillStreak.Push(times, ref n, 9.0, 1.0), "a new streak");
        }

        [Test]
        public void TheStreakKeepsItsNewestWhenFull()
        {
            var times = new double[4];
            int n = 0;
            int c = 0;
            for (int i = 0; i < 7; i++) c = KillStreak.Push(times, ref n, i * 0.01, 1.0);
            Assert.AreEqual(4, c, "at most the buffer");
            Assert.AreEqual(0.06, times[n - 1], 1e-12, "the newest kept");
        }

        [Test]
        public void TheStreaksAreNamedAtTwoThreeAndFive()
        {
            Assert.IsNull(KillStreak.Name(1));
            Assert.AreEqual("DOUBLE", KillStreak.Name(2));
            Assert.AreEqual("TRIPLE", KillStreak.Name(3));
            Assert.IsNull(KillStreak.Name(4), "no line between TRIPLE and MASSACRE");
            Assert.AreEqual("MASSACRE", KillStreak.Name(5));
            Assert.IsNull(KillStreak.Name(6));
            Assert.AreEqual("MASSACRE", KillStreak.Name(10), "every fifth after");
        }

        // --- the stamina bar ------------------------------------------------------------------------------------------------

        [Test]
        public void TheBarHasASegmentPerBar()
        {
            Assert.AreEqual(1, StaminaBarMath.Segments(1), "one bar");
            Assert.AreEqual(2, StaminaBarMath.Segments(2), "VIGOR");
            Assert.AreEqual(3, StaminaBarMath.Segments(3), "the web's pips");
            Assert.AreEqual(1, StaminaBarMath.Segments(0), "never none");
            Assert.AreEqual(UnityTuning.Hud17StaminaMaxSegments, StaminaBarMath.Segments(99));
            Assert.AreEqual(2, StaminaBarMath.Segments(1.5), "a part bar gets its segment");
        }

        [Test]
        public void EachSegmentHoldsItsShare()
        {
            Assert.AreEqual(1, StaminaBarMath.SegmentFill(1.5, 0), E);
            Assert.AreEqual(0.5, StaminaBarMath.SegmentFill(1.5, 1), E);
            Assert.AreEqual(0, StaminaBarMath.SegmentFill(0.4, 1), E);
            Assert.AreEqual(0, StaminaBarMath.SegmentFill(-1, 0), E);
        }

        [Test]
        public void ItIsSolidFromTheBoostFloor()
        {
            Assert.IsFalse(StaminaBarMath.Armed(0.1, 0.125), "under the floor: dim and hollow");
            Assert.IsTrue(StaminaBarMath.Armed(0.125, 0.125), "at the floor: a boost fires");
            Assert.IsTrue(StaminaBarMath.Armed(1, 0.125));
            Assert.IsTrue(StaminaBarMath.Armed(0, 0), "no floor");
        }

        [Test]
        public void TheFullEdgeFiresOnceOnTheRefill()
        {
            Assert.IsTrue(StaminaBarMath.JustFull(0.98, 1, 1));
            Assert.IsFalse(StaminaBarMath.JustFull(1, 1, 1), "already full");
            Assert.IsFalse(StaminaBarMath.JustFull(0.5, 0.9, 1), "not full yet");
            Assert.IsTrue(StaminaBarMath.JustFull(2.9, 3, 3), "the pips");
        }

        // --- HUD SCALE ------------------------------------------------------------------------------------------------------

        [Test]
        public void HudScaleIsOneWithoutTheRowAndClampedWithIt()
        {
            Assert.AreEqual(1f, HudScaleMath.From(false, 0.5), "no row: 1");
            Assert.AreEqual(0.8f, HudScaleMath.From(true, 0.5), 1e-6);
            Assert.AreEqual(1.25f, HudScaleMath.From(true, 3), 1e-6);
            Assert.AreEqual(1.1f, HudScaleMath.From(true, 1.1), 1e-6);
            Assert.AreEqual(1f, HudScaleMath.From(true, double.NaN));
        }

        [Test]
        public void AScaledLayerStillCoversTheScreen()
        {
            // the layer's rect grows by size x (1 / s - 1), half each side, so x s it is the screen again
            Assert.AreEqual(0, HudScaleMath.Grow(1920, 1), 1e-4);
            Assert.AreEqual(-192, HudScaleMath.Grow(1920, 1.25f), 1e-3);
            Assert.AreEqual(240, HudScaleMath.Grow(1920, 0.8f), 1e-3);
        }

        // --- the round's hero number ------------------------------------------------------------------------------------------

        [Test]
        public void TheHeroHoldsThenFliesToItsCorner()
        {
            Assert.AreEqual(0, RoundHeroMath.Fly(0), E);
            Assert.AreEqual(0, RoundHeroMath.Fly(UnityTuning.Hud17RoundHold), E, "1.5 s at the centre");
            Assert.AreEqual(1, RoundHeroMath.Fly(UnityTuning.Hud17RoundHold + UnityTuning.Hud17RoundFly), E, "400 ms later in its corner");
            Assert.AreEqual(0.5, RoundHeroMath.Fly(UnityTuning.Hud17RoundHold + UnityTuning.Hud17RoundFly * 0.5), 1e-6, "ease-in-out: half way at half time");
            Assert.Less(RoundHeroMath.Fly(UnityTuning.Hud17RoundHold + UnityTuning.Hud17RoundFly * 0.25), 0.25, "not linear: slow out of the hold");
            Assert.IsFalse(RoundHeroMath.Done(UnityTuning.Hud17RoundHold));
            Assert.IsTrue(RoundHeroMath.Done(UnityTuning.Hud17RoundHold + UnityTuning.Hud17RoundFly));
        }

        [Test]
        public void TheHeroSlamsInThenShrinksToTheCornerSize()
        {
            Assert.AreEqual(UnityTuning.Hud17RoundSlamFrom, RoundHeroMath.Scale(0), 1e-6, "slams from big");
            Assert.AreEqual(1, RoundHeroMath.Scale(UnityTuning.Hud17RoundSlam), 1e-6, "1 after the slam");
            Assert.AreEqual(1, RoundHeroMath.Scale(1.0), 1e-6, "holds at the hero size");
            double end = UnityTuning.Hud17RoundCornerPx / UnityTuning.Hud17RoundHeroPx;
            Assert.AreEqual(end, RoundHeroMath.Scale(UnityTuning.Hud17RoundHold + UnityTuning.Hud17RoundFly), 1e-6, "the corner's size");
        }
    }
}
