// NIGHT19 U3-33.8 (builder H): the vitals cluster's rules (S14: the centre is clear; the stamina pips respond within 2
// frames of a boost). Pure: Hud19Layout's rects (canvas px at 1080p, origin bottom left) and StaminaPips' state.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Hud19Tests
    {
        const double Frame = 1.0 / 60;

        // --- the layout --------------------------------------------------------------------------------------------------------

        [Test]
        public void APipIsChunky_OneBarIs120Px_TheRowShrinksPastTwo()
        {
            Assert.AreEqual(120f, Hud19Layout.PipWidth(1), 1e-3, "one bar at a run's start: one 120 px pip");
            Assert.AreEqual(120f, Hud19Layout.PipWidth(2), 1e-3, "VIGOR: two 120 px pips");
            Assert.AreEqual((268f - 16f) / 3f, Hud19Layout.PipWidth(3), 1e-3, "the web's three pips share the row");
            Assert.AreEqual((268f - 24f) / 4f, Hud19Layout.PipWidth(4), 1e-3);
            Assert.AreEqual(128f, Hud19Layout.PipX(1, 2), 1e-3, "pip 1 starts a pip and a gap in");
        }

        [Test]
        public void ThePipsAndTheSlideCell_SitUnderTheHpBar_InsideThePlate()
        {
            for (int n = 1; n <= StaminaPips.Max; n++)
            {
                float right = Hud19Layout.PipX(n - 1, n) + Hud19Layout.PipWidth(n);
                Assert.LessOrEqual(right, UnityTuning.Hud19PipRowW + 1e-3f, $"{n} pips fit their room");
            }
            Assert.AreEqual(UnityTuning.Hud17HpBarWidth, Hud19Layout.SlideX + UnityTuning.Hud19SlideW, 1e-3, "the slide cell ends under the HP bar's end");
            Assert.GreaterOrEqual(UnityTuning.Hud19PipsY, UnityTuning.Hud19HpBarY + UnityTuning.Hud17HpBarHeight + 4, "the pips under the HP bar, a gap between");
            Assert.LessOrEqual(UnityTuning.Hud19PipsY + UnityTuning.Hud19PipH + UnityTuning.Hud19MoveRowShift, Hud19Layout.PlateHeight, "the pips and the moved wall-jump ticks inside the plate");
        }

        [Test]
        public void ACostCutsAPipIntoBoosts()
        {
            Assert.AreEqual(1, Hud19Layout.Slits(0.5), "one bar = two boosts: one slit");
            Assert.AreEqual(0, Hud19Layout.Slits(1), "the web's pip: one boost each");
            Assert.AreEqual(3, Hud19Layout.Slits(0.25));
            Assert.AreEqual(3, Hud19Layout.Slits(0.3), "0.3, 0.6, 0.9");
            Assert.AreEqual(0, Hud19Layout.Slits(0), "no cost: no slit");
            Assert.AreEqual(0.6, Hud19Layout.SlitAt(1, 0.3), 1e-9);
        }

        [TestCase(1920f, 1f)]
        [TestCase(1920f, 0.8f)]
        [TestCase(1920f, 1.25f)]
        [TestCase(1440f, 1.25f)]
        [TestCase(1728f, 1.25f)]
        [TestCase(2560f, 1.25f)]
        public void TheVitalsCluster_NeverEntersTheCentralBox(float width, float hudScale)
        {
            HudRect box = Hud19Layout.CentreBox(width, 1080f);
            Assert.AreEqual(width * 0.3f, box.x, 1e-3);
            Assert.AreEqual(1080f * 0.4f, box.h, 1e-3);
            for (int n = 1; n <= StaminaPips.Max; n++)
            {
                for (int i = 0; i < n; i++)
                    Assert.IsFalse(Hud19Layout.Overlaps(Hud19Layout.OnScreen(Hud19Layout.PipRect(i, n), hudScale, false, width), box), $"pip {i} of {n} at {width} x 1080, HUD SCALE {hudScale}");
            }
            Assert.IsFalse(Hud19Layout.Overlaps(Hud19Layout.OnScreen(Hud19Layout.SlideRect(), hudScale, false, width), box), "the slide cell");
            Assert.IsFalse(Hud19Layout.Overlaps(Hud19Layout.OnScreen(Hud19Layout.Plate(), hudScale, false, width), box), "the plate");
        }

        [Test]
        public void OnScreen_ScalesTowardItsCorner()
        {
            var r = new HudRect(100, 50, 40, 20);
            HudRect left = Hud19Layout.OnScreen(r, 1.25f, false, 1920f);
            Assert.AreEqual(125f, left.x, 1e-3);
            Assert.AreEqual(62.5f, left.y, 1e-3);
            Assert.AreEqual(50f, left.w, 1e-3);
            // a block anchored bottom right: its distance from the right edge scales
            var rr = new HudRect(1920f - 140f, 50, 40, 20);
            HudRect right = Hud19Layout.OnScreen(rr, 1.25f, true, 1920f);
            Assert.AreEqual(1920f - 175f, right.x, 1e-3);
            Assert.IsTrue(Hud19Layout.Overlaps(new HudRect(0, 0, 10, 10), new HudRect(5, 5, 10, 10)));
            Assert.IsFalse(Hud19Layout.Overlaps(new HudRect(0, 0, 10, 10), new HudRect(10, 0, 10, 10)), "touching edges don't overlap");
        }

        // --- the pips' state ------------------------------------------------------------------------------------------------------

        static StaminaPips Rested(double fill, double max, double cost = 0.5)
        {
            var p = new StaminaPips();
            for (int i = 0; i < 30; i++) p.Step(fill, max, cost, cost * 0.25, Frame, true);
            return p;
        }

        [Test]
        public void ABoost_ShowsOnItsOwnFrame_WithTheDrainFlash()
        {
            StaminaPips p = Rested(1, 1);
            Assert.AreEqual(1, p.count);
            Assert.AreEqual(1.0, p.fill[0], 1e-9);
            p.Step(0.5, 1, 0.5, 0.125, Frame, true); // the boost's frame
            Assert.AreEqual(0.5, p.fill[0], 1e-9, "the fill drops on the boost's own frame (S14: within 2 frames)");
            Assert.Greater(p.drainFlash, 0, "the spent half flashes");
            Assert.AreEqual(1.0, p.trail[0], 1e-9, "the spent part is held as the trail");
            for (int i = 0; i < 6; i++) p.Step(0.5, 1, 0.5, 0.125, Frame, true);
            Assert.AreEqual(0, p.drainFlash, 1e-9, "the flash is short");
            for (int i = 0; i < 60; i++) p.Step(0.5, 1, 0.5, 0.125, Frame, true);
            Assert.AreEqual(0.5, p.trail[0], 0.01, "the trail drains down to the fill");
        }

        [Test]
        public void NoFlash_WithFlashesOff_OrOnASprintFrame_OrTheFirstFrame()
        {
            var first = new StaminaPips();
            first.Step(0.2, 1, 0.5, 0.125, Frame, true);
            Assert.AreEqual(0, first.drainFlash, "the first frame has nothing to compare");
            StaminaPips off = Rested(1, 1);
            off.Step(0.5, 1, 0.5, 0.125, Frame, false);
            Assert.AreEqual(0, off.drainFlash, "FLASH INTENSITY 0");
            Assert.AreEqual(0.5, off.fill[0], 1e-9, "the fill still drops");
            StaminaPips sprint = Rested(0.9, 1);
            sprint.Step(0.895, 1, 0.5, 0.125, Frame, true); // 0.3 bar/s at 60 fps
            Assert.AreEqual(0, sprint.drainFlash, "a sprint's drain is no chunk");
        }

        [Test]
        public void ARefill_TicksTheCellItCompletes()
        {
            StaminaPips p = Rested(0.45, 1);
            Assert.IsFalse(p.Ticking(0));
            p.Step(0.52, 1, 0.5, 0.125, Frame, true); // crosses the first boost's cost
            Assert.IsTrue(p.Ticking(0), "the pip ticks the frame its first boost is back");
            Assert.Greater(p.PipScale(0), 1f, "the tick punches the pip");
            Assert.IsTrue(p.TickWhite(0));
            for (int i = 0; i < 20; i++) p.Step(0.52, 1, 0.5, 0.125, Frame, true);
            Assert.IsFalse(p.Ticking(0));
            Assert.AreEqual(1f, p.PipScale(0), 1e-6, "back to rest");
        }

        [Test]
        public void AKillsRefill_TicksThePipItLandsIn_TheTrickleDoesnt()
        {
            StaminaPips p = Rested(0.0, 1);
            p.Step(0.25, 1, 0.5, 0.125, Frame, true); // a kill within 6 m: +0.25 in one frame (design.stamina.n19)
            Assert.IsTrue(p.Ticking(0), "a reward's chunk shows: the pip it lands in ticks");
            StaminaPips q = Rested(0.2, 1);
            q.Step(0.2 + 0.5 / 60, 1, 0.5, 0.125, Frame, true); // the passive refill, 0.5 bar/s
            Assert.IsFalse(q.Ticking(0), "the passive trickle doesn't tick");
            StaminaPips v = Rested(1.0, 2);
            v.Step(1.3, 2, 0.5, 0.125, Frame, true);
            Assert.IsTrue(v.Ticking(1) && !v.Ticking(0), "VIGOR: the second pip, where it landed");
        }

        [Test]
        public void VigorsSecondBar_IsTheSecondPip_AndTicksAlone()
        {
            StaminaPips p = Rested(1.4, 2);
            Assert.AreEqual(2, p.count);
            Assert.AreEqual(1.0, p.fill[0], 1e-9);
            Assert.AreEqual(0.4, p.fill[1], 1e-9);
            p.Step(1.55, 2, 0.5, 0.125, Frame, true); // 1.5 = the second bar's first boost
            Assert.IsTrue(p.Ticking(1));
            Assert.IsFalse(p.Ticking(0), "only the pip whose cell completed");
            StaminaPips pips = Rested(1.9, 3, 1); // the web's pips (rules off): cost 1
            pips.Step(2.05, 3, 1, 1, Frame, true);
            Assert.IsTrue(pips.Ticking(1), "a whole pip back ticks that pip");
            Assert.AreEqual(3, pips.count);
        }

        [Test]
        public void Empty_UnderTheFloor_PulsesTheTracks()
        {
            StaminaPips p = Rested(0.1, 1);
            Assert.IsTrue(p.empty, "under the boost's floor a press fails");
            StaminaPips q = Rested(0.2, 1);
            Assert.IsFalse(q.empty);
            Assert.AreEqual(0, Rested(1, 1).TrackPulse, 1e-9, "no pulse when a boost fires");
        }

        // --- the weapon card ----------------------------------------------------------------------------------------------------

        static readonly string[] Reachable = { "revolver", "shotgun", "nailgun", "rocket", "rail", "zapper", "bouncer", "gauntlet" };

        static int Words(string s)
        {
            int n = 0;
            foreach (string w in s.Split(' ')) if (w.Length > 0 && char.IsLetter(w[0])) n++;
            return n;
        }

        [Test]
        public void EveryReachableGun_HasARole_ShortAndInTheCharset()
        {
            foreach (string id in Reachable)
            {
                string tag = WeaponRole19.Tag(id), good = WeaponRole19.GoodAgainst(id), combos = WeaponRole19.CombosWith(id);
                Assert.IsNotNull(tag, id);
                Assert.IsNotNull(good, id);
                Assert.IsNotNull(combos, id);
                Assert.LessOrEqual(Words(tag), 3, $"{id}'s tag: 3 words on the HUD (IDENTITY.md)");
                Assert.LessOrEqual(Words(good), 8, id);
                Assert.LessOrEqual(Words(combos), 8, id);
                foreach (char c in tag + good + combos) Assert.LessOrEqual((int)c, 0xFF, $"{id}: '{c}' is outside the static atlas");
            }
            Assert.IsNull(WeaponRole19.Tag("nope"), "a gun without a role shows none");
            Assert.AreEqual("POINT BLANK", WeaponRole19.Tag("shotgun"));
        }

        [Test]
        public void TheCardHeader_ShowsTheNameOnASwap_ThenTheRole()
        {
            Assert.IsTrue(CardHead19.NameShown(0), "the swap's own frame");
            Assert.IsTrue(CardHead19.NameShown(UnityTuning.Hud17SwapNameTime - 0.01));
            Assert.IsFalse(CardHead19.NameShown(UnityTuning.Hud17SwapNameTime + 0.01));
            Assert.IsFalse(CardHead19.NameShown(double.MaxValue), "at rest: the role");
            Assert.AreEqual(0f, CardHead19.TagAlpha(0.1), 1e-6, "never both: the role hides under the name");
            Assert.AreEqual(1f, CardHead19.TagAlpha(double.MaxValue), 1e-6);
            float back = CardHead19.TagAlpha(UnityTuning.Hud17SwapNameTime + 0.02);
            Assert.Greater(back, 0f, "the role comes back...");
            Assert.Less(back, 0.95f, "...entering, not popping in (M9)");
            Assert.Greater(CardHead19.NameScale(0), 1.05f, "the name slams in from above its size (not an M9 pop-in)");
            Assert.AreEqual(1f, CardHead19.NameScale(0.3), 1e-3);
        }

        [TestCase(1920f, 1f)]
        [TestCase(1920f, 1.25f)]
        [TestCase(1728f, 1.25f)]
        [TestCase(2560f, 1.25f)]
        [TestCase(1440f, 1f)]
        public void TheWeaponCard_WithItsHeader_StaysOutOfTheCentralBox(float width, float hudScale)
        {
            HudRect box = Hud19Layout.CentreBox(width, 1080f);
            for (int slots = 1; slots <= UnityTuning.Hud17StripCells; slots++)
            {
                HudRect card = Hud19Layout.CardRect(slots, true, width);
                Assert.Greater(card.h, UnityTuning.Hud17StripCellH + UnityTuning.Hud19CardHeadH, "the card holds the header over the slots");
                Assert.IsFalse(Hud19Layout.Overlaps(Hud19Layout.OnScreen(card, hudScale, true, width), box), $"{slots} slots at {width}, HUD SCALE {hudScale}");
            }
        }

        // --- the combo callouts --------------------------------------------------------------------------------------------------

        [Test]
        public void AComboLine_SlamsInWhole_AndOutlivesAnAwardLine()
        {
            Assert.Greater(ComboCallout19.Scale(0), 1.05f, "a slam from above its size (an entrance, not an M9 pop-in)");
            Assert.AreEqual(1f, ComboCallout19.Scale(0.3), 1e-3, "settled");
            Assert.AreEqual(1f, ComboCallout19.Alpha(0, true), 1e-6, "whole from its first frame (a slam)");
            Assert.AreEqual(1f, ComboCallout19.Alpha(UnityTuning.Hud19ComboFadeAt - 0.01, true), 1e-6, "held");
            Assert.Less(ComboCallout19.Alpha(UnityTuning.Hud19ComboLife - 0.01, true), 0.1f, "nearly gone 10 ms before its end (an ease-in fade: IDENTITY.md's out)");
            Assert.AreEqual(0f, ComboCallout19.Alpha(UnityTuning.Hud19ComboLife, true), 1e-6, "faded by its end");
            Assert.IsFalse(ComboCallout19.Expired(1.0, true, UnityTuning.Hud17CalloutLife), "a combo still shows at 1 s ...");
            Assert.IsTrue(ComboCallout19.Expired(1.0, false, UnityTuning.Hud17CalloutLife), "... an award's line is gone");
            Assert.IsTrue(ComboCallout19.Expired(UnityTuning.Hud19ComboLife, true, UnityTuning.Hud17CalloutLife));
            Assert.IsTrue(ComboCallout19.White(0) && !ComboCallout19.White(0.1), "white-hot for its first frames only");
        }

        [Test]
        public void AComboNamesItself_ThePlayersOnly()
        {
            Assert.AreEqual("POP", ComboCallout19.Line(new Skygrind.Core.ComboEvent { name = "POP" }));
            Assert.AreEqual("LAUNCH + SNIPE", ComboCallout19.Line(new Skygrind.Core.ComboEvent { name = "LAUNCH + SNIPE" }));
            Assert.IsNull(ComboCallout19.Line(new Skygrind.Core.ComboEvent { name = "" }), "no name, no line");
            Assert.IsNull(ComboCallout19.Line(new Skygrind.Core.ComboEvent { name = "POP", owner = "rival" }), "the player's combos only");
            Assert.IsNull(ComboCallout19.Line(null));
        }

        // --- the WEAPONS screen's cascade --------------------------------------------------------------------------------------

        [Test]
        public void TheWeaponRows_CascadeIn_NeverWholeOnTheirFirstFrame()
        {
            Assert.AreEqual(0, RowCascade19.Wait(0), 1e-9);
            Assert.AreEqual(UnityTuning.Hud19RowStagger, RowCascade19.Wait(1), 1e-9);
            Assert.AreEqual(UnityTuning.Hud19RowStaggerMax, RowCascade19.Wait(40), 1e-9, "a long list doesn't wait forever");
            Assert.AreEqual(0f, RowCascade19.Alpha(0, 0), 1e-6, "the screen's first frame: nothing whole (M9)");
            Assert.AreEqual(0f, RowCascade19.Alpha(RowCascade19.Wait(3) - 0.001, 3), 1e-6, "a row waits its turn");
            Assert.AreEqual(1f, RowCascade19.Alpha(RowCascade19.Wait(3) + UnityTuning.ThemeInTime, 3), 1e-6);
            UnityEngine.Vector2 from = RowCascade19.Offset(0, 0, false);
            Assert.Less(from.x, -UnityTuning.ThemeEnterDistance * 0.9f, "it enters from down-left along the slant");
            Assert.Less(from.y, 0f);
            Assert.AreEqual(0f, RowCascade19.Offset(1, 0, false).magnitude, 1e-4, "at rest");
            Assert.AreEqual(0f, RowCascade19.Offset(0, 0, true).magnitude, 1e-6, "REDUCED MOTION: no slide");
            Assert.Less(RowCascade19.TagScale(RowCascade19.Wait(2) + UnityTuning.Hud19TagPopAfter, 2), 0.95f, "the tag pops in from small");
            Assert.AreEqual(1f, RowCascade19.TagScale(10, 2), 1e-4);
            Assert.IsFalse(RowCascade19.Done(0.1, 6));
            Assert.IsTrue(RowCascade19.Done(1.0, 6));
        }

        [Test]
        public void ANewRun_ForgetsTheLastFrame()
        {
            StaminaPips p = Rested(0.2, 1);
            p.Reset();
            p.Step(1, 1, 0.5, 0.125, Frame, true);
            Assert.IsFalse(p.Ticking(0), "a new run's full bar is no refill");
            Assert.AreEqual(1.0, p.trail[0], 1e-9);
        }
    }
}
