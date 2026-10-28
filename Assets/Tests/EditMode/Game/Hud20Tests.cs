// NIGHT20 (the shared commit placed this empty fixture; H owns it): the HUD for tonight's things: the shockwave's cooldown ring (seconds left above 3 s, the ready flash), the zoom reticle, the segment ticks, the round-end refill, the climb's off-screen arrow, the announcer slabs, the NEXT line.
// NIGHT20 U3-34.10 (builder H): the pure parts (Hud20Look.cs): the ring's fraction, when the seconds show, the ready edge and
// its flash; the reticle's kind by magnification and its size; the tick positions and the regen's reach; the refill's promise;
// the special hint's zoom word and the cycle line from the live binding; the card with its chip stays out of S14's box.
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class Hud20Tests
    {
        // --- the shockwave chip ---------------------------------------------------------------------------------------------

        [Test]
        public void TheRing_FillsWithTheNextChargesProgress_FullWhenEveryChargeIsBack()
        {
            Assert.AreEqual(1f, ShockRing20.Fill(1, 1, 0, 18), 1e-6, "one charge, ready: a whole ring");
            Assert.AreEqual(0f, ShockRing20.Fill(0, 1, 18, 18), 1e-6, "just fired: empty");
            Assert.AreEqual(0.5f, ShockRing20.Fill(0, 1, 9, 18), 1e-6, "half way back");
            Assert.AreEqual(0.25f, ShockRing20.Fill(1, 2, 13.5, 18), 1e-6, "the FORGE's second charge: its own progress");
            Assert.AreEqual(1f, ShockRing20.Fill(2, 2, 0, 18), 1e-6);
            Assert.AreEqual(0f, ShockRing20.Fill(0, 1, 30, 18), 1e-6, "clamped");
            Assert.AreEqual(1f, ShockRing20.Fill(0, 1, 5, 0), 1e-6, "no cooldown read: never a NaN");
            Assert.IsTrue(ShockRing20.Ready(1));
            Assert.IsFalse(ShockRing20.Ready(0));
        }

        [Test]
        public void TheSeconds_ShowAboveThreeSeconds_OnlyWhileNoChargeIsReady()
        {
            Assert.AreEqual(18, ShockRing20.Seconds(0, 18));
            Assert.AreEqual(18, ShockRing20.Seconds(0, 17.2), "rounded up: 17.2 s left reads 18");
            Assert.AreEqual(4, ShockRing20.Seconds(0, 3.01));
            Assert.AreEqual(-1, ShockRing20.Seconds(0, 3.0), "at 3 s the glyph comes back: get ready");
            Assert.AreEqual(-1, ShockRing20.Seconds(0, 1.2));
            Assert.AreEqual(-1, ShockRing20.Seconds(1, 12), "a charge is ready: the glyph, not the next charge's seconds");
        }

        [Test]
        public void AChargeComingBack_Flashes_ThroughItsEnvelope()
        {
            Assert.IsTrue(ShockRing20.ChargeBack(0, 1), "the cooldown ended");
            Assert.IsTrue(ShockRing20.ChargeBack(1, 2), "the FORGE's second charge");
            Assert.IsFalse(ShockRing20.ChargeBack(1, 1));
            Assert.IsFalse(ShockRing20.ChargeBack(1, 0), "firing is no flash");
            Assert.IsFalse(ShockRing20.ChargeBack(-1, 1), "the first read (a run's start, a buy) is no flash");
            Assert.AreEqual(1f, ShockRing20.Flash(0, 1), 1e-6, "whole on its first frame");
            float mid = ShockRing20.Flash(UnityTuning.Hud20ShockFlash * 0.5, 1);
            Assert.Greater(mid, 0f);
            Assert.Less(mid, 0.5f, "an ease-out: under half way at half time");
            Assert.AreEqual(0f, ShockRing20.Flash(UnityTuning.Hud20ShockFlash, 1), 1e-6, "0.2 s");
            Assert.AreEqual(0.2, UnityTuning.Hud20ShockFlash, 1e-9, "NIGHT20.md U3-34.3: a 0.2 s cyan flash");
            Assert.AreEqual(0f, ShockRing20.Flash(0, 0), 1e-6, "FLASH INTENSITY 0: none");
            Assert.AreEqual(0.5f, ShockRing20.Flash(0, 0.5), 1e-6);
            Assert.Greater(ShockRing20.Punch(0, false), 1.1f, "the chip punches on a charge's return");
            Assert.AreEqual(1f, ShockRing20.Punch(UnityTuning.Hud20ShockPunchTime, false), 1e-6);
            Assert.AreEqual(1f, ShockRing20.Punch(0, true), 1e-6, "REDUCED MOTION: still");
        }

        // --- the zoom's reticle ---------------------------------------------------------------------------------------------

        [Test]
        public void TheReticle_ByMagnification_RingOrScope()
        {
            Assert.AreEqual(ZoomReticle20.None, ZoomReticle20.Kind(1));
            Assert.AreEqual(ZoomReticle20.None, ZoomReticle20.Kind(0), "no read");
            Assert.AreEqual(ZoomReticle20.Ring, ZoomReticle20.Kind(1.25), "the STITCHER");
            Assert.AreEqual(ZoomReticle20.Ring, ZoomReticle20.Kind(1.5), "the KESTREL");
            Assert.AreEqual(ZoomReticle20.Scope, ZoomReticle20.Kind(3.5), "the LONGWAVE");
            Assert.AreEqual(28.8f, ZoomReticle20.Radius(1.25), 1e-3);
            Assert.AreEqual(24f, ZoomReticle20.Radius(1.5), 1e-3, "the tighter zoom, the tighter ring");
            Assert.IsTrue(ZoomReticle20.Dot(1.5), "the precise gun has the dot");
            Assert.IsFalse(ZoomReticle20.Dot(1.25));
        }

        [Test]
        public void TheReticle_EntersWithTheZoom_AndStaysStillWithReducedMotion()
        {
            Assert.AreEqual(0f, ZoomReticle20.Alpha(0), 1e-6);
            Assert.AreEqual(1f, ZoomReticle20.Alpha(1), 1e-6);
            Assert.AreEqual(1f, ZoomReticle20.Alpha(3), 1e-6, "clamped");
            Assert.AreEqual(UnityTuning.Hud20ReticleFrom, ZoomReticle20.Scale(0, false), 1e-5, "it closes in from wide");
            Assert.AreEqual(1f, ZoomReticle20.Scale(1, false), 1e-6);
            Assert.Less(ZoomReticle20.Scale(0.5, false), (1f + UnityTuning.Hud20ReticleFrom) * 0.5f, "an ease-out: past half way at half the zoom");
            Assert.AreEqual(1f, ZoomReticle20.Scale(0, true), 1e-6, "REDUCED MOTION: no scale");
            Assert.AreEqual(UnityTuning.Hud20ScopeFrom, ZoomReticle20.ScopeScale(0, false), 1e-5);
            Assert.AreEqual(1f, ZoomReticle20.ScopeScale(1, false), 1e-6);
            Assert.AreEqual(1f, ZoomReticle20.ScopeScale(0.2, true), 1e-6);
        }

        [Test]
        public void TheScope_IsALensWithAMask_CoveringEveryCorner()
        {
            Assert.AreEqual(1080f * UnityTuning.Hud20ScopeRadiusShare, ZoomReticle20.ScopeRadius(1080), 1e-3);
            float r = ZoomReticle20.ScopeRadius(1080);
            Assert.Less(r, 540f, "the lens fits the screen's height: the mask shows above and below");
            float outer = ZoomReticle20.MaskOuter(1920, 1080);
            Assert.GreaterOrEqual(outer, (float)System.Math.Sqrt(960.0 * 960 + 540.0 * 540), "the mask reaches every corner");
            Assert.GreaterOrEqual(ZoomReticle20.MaskOuter(2520, 1080), (float)System.Math.Sqrt(1260.0 * 1260 + 540.0 * 540), "21:9 too (the canvas keeps 1080 tall)");
        }

        // --- the HP bar's segments ------------------------------------------------------------------------------------------

        [Test]
        public void TheTicks_SitEverySegment_AsTheBarGrows()
        {
            Assert.AreEqual(3, HpSegments20.Ticks(100, 25), "4 segments at 100 HP: the quarters NIGHT19 drew");
            Assert.AreEqual(5, HpSegments20.Ticks(150, 25), "the PLATE's 150: 6 segments");
            Assert.AreEqual(4, HpSegments20.Ticks(110, 25), "a 10 HP stub after the fourth tick");
            Assert.AreEqual(0, HpSegments20.Ticks(100, 0), "no segment read: no ticks");
            Assert.AreEqual(0, HpSegments20.Ticks(20, 25), "under one segment");
            Assert.AreEqual(UnityTuning.Hud20MaxTicks, HpSegments20.Ticks(10000, 25), "capped at the pool");
            Assert.AreEqual(0.25f, HpSegments20.TickAt(0, 100, 25), 1e-6);
            Assert.AreEqual(0.75f, HpSegments20.TickAt(2, 100, 25), 1e-6);
            Assert.AreEqual(0.5f, HpSegments20.TickAt(2, 150, 25), 1e-6);
        }

        [Test]
        public void TheRegensReach_IsTheTopOfTheCurrentSegment()
        {
            Assert.AreEqual(75, HpSegments20.Top(60, 100, 25), 1e-9, "60 HP climbs back to 75");
            Assert.AreEqual(75, HpSegments20.Top(75, 100, 25), 1e-9, "on a tick: that tick");
            Assert.AreEqual(100, HpSegments20.Top(76, 100, 25), 1e-9);
            Assert.AreEqual(80, HpSegments20.Top(77, 80, 25), 1e-9, "ULTRA HEALTH's cap stops it");
            Assert.AreEqual(0, HpSegments20.Top(0, 100, 25), 1e-9, "down: nothing");
            Assert.AreEqual(100, HpSegments20.Top(40, 100, 0), 1e-9, "no segments: the cap");
            Assert.AreEqual(1, HpSegments20.Crossed(0.2f, 0.55f, 3, 100, 25), "the highest tick crossed upward");
            Assert.AreEqual(-1, HpSegments20.Crossed(0.3f, 0.45f, 3, 100, 25), "none crossed");
            Assert.AreEqual(-1, HpSegments20.Crossed(0.8f, 0.2f, 3, 100, 25), "a hit crosses down: no light");
            Assert.AreEqual(0, HpSegments20.Crossed(0.1f, 0.25f, 3, 100, 25), "reaching a tick lights it");
        }

        [Test]
        public void TheRefill_PromisesItsTarget_ThenFadesOnceTheBarArrives()
        {
            var r = new Refill20();
            Assert.IsFalse(r.Active);
            Assert.AreEqual(0f, r.Alpha, 1e-6);
            r.Start(1.0, 2);
            Assert.IsTrue(r.Active);
            Assert.AreEqual(1f, r.Target, 1e-6);
            r.Step(1.0 / 60, 0.3);
            float a1 = r.Alpha;
            Assert.Greater(a1, 0f, "it enters on its first frame ...");
            Assert.Less(a1, 0.95f, "... not whole (M9)");
            for (int i = 0; i < 30; i++) r.Step(1.0 / 60, 0.3 + i * 0.02);
            Assert.AreEqual(1f, r.Alpha, 1e-6, "held while the bar climbs");
            for (int i = 0; i < 60; i++) r.Step(1.0 / 60, 1.0);
            Assert.IsFalse(r.Active, "gone once the bar arrived, lingered and faded");
            Assert.AreEqual(0f, r.Alpha, 1e-6);
            r.Start(0.9, 2);
            r.Step(1.0 / 60, 0.5);
            r.Reset();
            Assert.IsFalse(r.Active, "a new run forgets it");
            var stuck = new Refill20();
            stuck.Start(1.0, 2);
            for (int i = 0; i < 600; i++) stuck.Step(1.0 / 60, 0.4); // hit all through the break: the bar never arrives
            Assert.IsFalse(stuck.Active, "it ends after its seconds plus the linger even if the bar never got there");
        }

        // --- the special hint and the cycle line ---------------------------------------------------------------------------

        [Test]
        public void TheZoomWord_FollowsTheRealMagnification()
        {
            Assert.AreEqual("ZOOM ×1.5", SpecialHint20.ZoomWord(1.5));
            Assert.AreEqual("ZOOM ×1.25", SpecialHint20.ZoomWord(1.25), "the STITCHER's (design.zoom), not the web's table");
            Assert.AreEqual("ZOOM ×3.5", SpecialHint20.ZoomWord(3.5));
            Assert.IsNull(SpecialHint20.ZoomWord(1), "no zoom: no word");
            Assert.AreSame(SpecialHint20.ZoomWord(1.5), SpecialHint20.ZoomWord(1.5), "cached");
        }

        [Test]
        public void TheCycleLine_FollowsTheLiveBinding()
        {
            Assert.AreEqual(UnityTuning.HudPadCycleNeutral, SpecialHint20.CycleLine(GamepadMath.DefaultLayoutV2, GlyphSet.Neutral), "v2: Y NEXT, D-PAD LEFT BACK (the old fixed line)");
            var v3 = new PadLayout(new List<(PadButtonName, PadAction[])>
            {
                (PadButtonName.RB, new[] { PadAction.NextWeapon }),
                (PadButtonName.Y, new[] { PadAction.Special, PadAction.Grapple }),
                (PadButtonName.Left, new[] { PadAction.PrevWeapon }),
            });
            Assert.AreEqual("RB NEXT · D-PAD LEFT BACK", SpecialHint20.CycleLine(v3, GlyphSet.Neutral), "v3: RB scrolls");
            Assert.AreEqual("R1 NEXT · D-PAD LEFT BACK", SpecialHint20.CycleLine(v3, GlyphSet.PlayStation));
            var nextOnly = new PadLayout(new List<(PadButtonName, PadAction[])> { (PadButtonName.RB, new[] { PadAction.NextWeapon }) });
            Assert.AreEqual("RB NEXT", SpecialHint20.CycleLine(nextOnly, GlyphSet.Neutral), "an unbound half is left out");
            Assert.AreEqual("", SpecialHint20.CycleLine(new PadLayout(new List<(PadButtonName, PadAction[])>()), GlyphSet.Neutral));
        }

        // --- F1 the announcer --------------------------------------------------------------------------------------------------

        static int Words(string s)
        {
            int n = 0;
            foreach (string w in s.Split(' ')) if (w.Length > 0 && char.IsLetterOrDigit(w[0])) n++;
            return n;
        }

        [Test]
        public void EverySlab_IsThreeWordsAtMost_InTheStaticCharset()
        {
            foreach (string s in Announcer20.AllSlabs)
            {
                Assert.LessOrEqual(Words(s), 3, s);
                Assert.Greater(s.Length, 0);
                foreach (char c in s) Assert.LessOrEqual((int)c, 0xFF, $"'{c}' in {s} is outside the static atlas");
                Assert.AreEqual(s.ToUpperInvariant(), s, "slabs are caps");
            }
            Assert.GreaterOrEqual(Announcer20.AllSlabs.Count, 8, "the backlog's beats");
        }

        [Test]
        public void ASpecialRound_NamesItsSlab_ByPhase()
        {
            int p;
            Assert.AreEqual("THE HUNT", Announcer20.ForSpecial(new SpecialRoundEvent { name = "hunt", phase = "announce" }, out p));
            Assert.AreEqual(Announcer20.Special, p);
            Assert.AreEqual("ELITE INBOUND", Announcer20.ForSpecial(new SpecialRoundEvent { name = "elite", phase = "start" }, out p));
            Assert.AreEqual("BOSS INBOUND", Announcer20.ForSpecial(new SpecialRoundEvent { name = "boss", phase = "announce" }, out p));
            Assert.AreEqual(Announcer20.Boss, p, "the boss outranks everything");
            Assert.AreEqual("BOSS INBOUND", Announcer20.ForSpecial(new SpecialRoundEvent { name = "bulwark", phase = "announce" }, out p), "the web's boss by name");
            Assert.AreEqual("LAST ONE", Announcer20.ForSpecial(new SpecialRoundEvent { name = "hunt", phase = "last" }, out p));
            Assert.AreEqual(Announcer20.Last, p);
            Assert.AreEqual("BOSS DOWN", Announcer20.ForSpecial(new SpecialRoundEvent { name = "boss", phase = "end" }, out p));
            Assert.AreEqual("FOG ROUND", Announcer20.ForSpecial(new SpecialRoundEvent { name = "fog", phase = "announce" }, out p));
            Assert.IsNull(Announcer20.ForSpecial(new SpecialRoundEvent { name = "hunt", phase = "end" }, out p), "a hunt's end is the drop's moment, not a slab");
            Assert.IsNull(Announcer20.ForSpecial(new SpecialRoundEvent { name = "hunt", phase = "start" }, out p), "announced in the break, not again at the start");
            Assert.IsNull(Announcer20.ForSpecial(new SpecialRoundEvent { name = "something nobody named", phase = "announce" }, out p), "an unknown long name: no slab");
            Assert.AreEqual("TWIST", Announcer20.ForSpecial(new SpecialRoundEvent { name = "twist", phase = "announce" }, out p), "a short unknown name reads as itself");
            Assert.IsNull(Announcer20.ForSpecial(null, out p));
            Assert.IsTrue(Announcer20.Danger(Announcer20.BossInbound), "a threat wears DANGER");
            Assert.IsTrue(Announcer20.Danger(Announcer20.LastOne));
            Assert.IsFalse(Announcer20.Danger(Announcer20.ExfilOpen), "the player's way out is CYAN");
            Assert.IsFalse(Announcer20.Danger(Announcer20.GoldenRound), "GOLDEN is a gift (and gold is points only)");
            Assert.IsFalse(Announcer20.Danger(Announcer20.BossDown));
        }

        [Test]
        public void TheQueue_ShowsOneSlabAtATime_AtMostOnePerGap()
        {
            var q = new AnnouncerQueue20();
            q.Push("EXFIL OPEN", Announcer20.Exfil, 0);
            q.Push("LAST ONE", Announcer20.Last, 0);
            Assert.IsTrue(q.Step(0), "the first slab starts at once");
            Assert.AreEqual("EXFIL OPEN", q.Showing, "the higher priority first");
            Assert.IsFalse(q.Step(1.0));
            Assert.AreEqual("EXFIL OPEN", q.Showing, "one at a time");
            Assert.IsTrue(q.Step(UnityTuning.Hud20SlabGap + 0.01), "the next after the gap");
            Assert.AreEqual("LAST ONE", q.Showing);
            Assert.IsFalse(q.Step(UnityTuning.Hud20SlabGap + UnityTuning.Hud20SlabLife + 0.02));
            Assert.IsNull(q.Showing, "gone after its life");
        }

        [Test]
        public void TheQueue_LetsTheBossCutIn_DropsStaleBeats_AndMergesTwins()
        {
            var q = new AnnouncerQueue20();
            q.Push("LAST ONE", Announcer20.Last, 0);
            q.Step(0);
            q.Push("BOSS INBOUND", Announcer20.Boss, 0.3);
            Assert.IsTrue(q.Step(0.3), "a boss beat never waits");
            Assert.AreEqual("BOSS INBOUND", q.Showing);
            var s = new AnnouncerQueue20();
            s.Push("EXFIL OPEN", Announcer20.Exfil, 0);
            s.Step(0);
            s.Push("LAST ONE", Announcer20.Last, 0.1);
            s.Push("LAST ONE", Announcer20.Last, 0.2);
            Assert.AreEqual(1, s.Queued, "twins merge");
            s.Push("EXFIL OPEN", Announcer20.Exfil, 0.2);
            Assert.AreEqual(1, s.Queued, "the one showing isn't queued again");
            Assert.IsFalse(s.Step(UnityTuning.Hud20SlabMaxWait + 0.5), "a beat that waited too long is stale: dropped");
            Assert.AreEqual(0, s.Queued);
            var full = new AnnouncerQueue20();
            full.Push("A", Announcer20.Info, 0);
            full.Push("B", Announcer20.Info, 0);
            full.Push("C", Announcer20.Info, 0);
            full.Push("D", Announcer20.Info, 0);
            full.Push("THE HUNT", Announcer20.Special, 0);
            Assert.AreEqual(UnityTuning.Hud20SlabQueue, full.Queued);
            full.Step(0);
            Assert.AreEqual("THE HUNT", full.Showing, "a full queue makes room for a higher beat");
            full.Clear();
            Assert.AreEqual(0, full.Queued);
            Assert.IsNull(full.Showing);
        }

        [Test]
        public void ASlab_SlamsInWhole_HoldsAndFades()
        {
            Assert.Greater(Slab20.Scale(0), 1.05f, "a slam from above its size (an entrance, not an M9 pop-in)");
            Assert.AreEqual(1f, Slab20.Scale(0.3), 1e-3);
            Assert.AreEqual(1f, Slab20.Alpha(0.5), 1e-6, "held");
            Assert.Less(Slab20.Alpha(UnityTuning.Hud20SlabLife - 0.01), 0.1f, "an ease-in fade at its end");
            Assert.AreEqual(0f, Slab20.Alpha(UnityTuning.Hud20SlabLife), 1e-6);
            Assert.IsTrue(Slab20.White(0) && !Slab20.White(0.1), "white-hot for its first frames only");
        }

        [Test]
        public void LastOne_FiresOncePerRound_WhenTheRoundHadACrowd()
        {
            var l = new LastOne20();
            Assert.IsFalse(l.Step(3, true, 6), "six left");
            Assert.IsTrue(l.Step(3, true, 1), "the last one");
            Assert.IsFalse(l.Step(3, true, 1), "once");
            Assert.IsFalse(l.Step(3, true, 2), "a top-up and back: still once a round");
            Assert.IsFalse(l.Step(3, true, 1));
            Assert.IsFalse(l.Step(4, false, 0), "the break");
            Assert.IsFalse(l.Step(4, true, 1), "a round that starts with one isn't a 'last one' (no crowd seen)");
            var m = new LastOne20();
            Assert.IsFalse(m.Step(5, true, UnityTuning.Hud20LastOneFrom));
            Assert.IsTrue(m.Step(5, true, 1));
            m.Reset();
            Assert.IsFalse(m.Step(5, true, 1), "a new run forgets the crowd");
        }

        // --- A8 the NEXT line ------------------------------------------------------------------------------------------------

        /// <summary>The web's director.rounds schedule (PortExport/config.json): bosses every 5 from 5, swarms 8, 13, 18 ...,
        /// the objectives from 6 every 4 (HOLD on 2, GOLD on 3).</summary>
        static Skygrind.Core.DirectorMath.D WebRounds() => new Skygrind.Core.DirectorMath.D
        {
            bossEvery = 5, bossFrom = 5, swarmEvery = 5, swarmOffset = 3, swarmFrom = 8,
            objectiveFrom = 6, objectiveEvery = 4, holdOffset = 2, goldOffset = 3,
        };

        [Test]
        public void TheRoundKinds_FollowTheDirectorsSchedule_TheBossOnlyWhenOneIsPorted()
        {
            var d = WebRounds();
            for (int r = 1; r <= 40; r++)
            {
                string web = Skygrind.Core.DirectorMath.RoundType(r, d);
                string withBoss = NextLine20.Kind(r, d, true, true);
                Assert.AreEqual(web, withBoss, $"round {r}: the director's own RoundType");
                string noBoss = NextLine20.Kind(r, d, true, false);
                Assert.AreEqual(web == "boss" ? "normal" : web, noBoss, $"round {r}: no boss in the port yet: an ordinary round");
                string noObj = NextLine20.Kind(r, d, false, false);
                Assert.AreEqual(web == "boss" || web == "hold" || web == "gold" ? "normal" : web, noObj, $"round {r}: objectives off");
            }
            Assert.AreEqual("swarm", NextLine20.Kind(8, d, true, false));
            Assert.AreEqual("hold", NextLine20.Kind(6, d, true, false));
            Assert.AreEqual("gold", NextLine20.Kind(7, d, true, false));
        }

        [Test]
        public void TheNextLine_NamesTheNearestSpecial_InThreeWords()
        {
            Assert.AreEqual("NEXT: SWARM", NextLine20.Line(new[] { "swarm", "normal", "normal", "normal", "normal" }, false));
            Assert.AreEqual("HOLD IN 2", NextLine20.Line(new[] { "normal", "hold", "normal", "swarm", "normal" }, false), "the nearest");
            Assert.AreEqual("BOSS IN 5", NextLine20.Line(new[] { "normal", "normal", "normal", "normal", "boss" }, false));
            Assert.IsNull(NextLine20.Line(new[] { "normal", "normal", "normal", "normal", "normal" }, false), "nothing coming: the pips alone");
            Assert.AreEqual("EXFIL OPEN", NextLine20.Line(new[] { "normal", "hold", "normal", "normal", "normal" }, true), "the radio now outranks a forecast");
            Assert.AreEqual("NEXT: HUNT", NextLine20.Line(new[] { "hunt", "normal", "normal", "normal", "normal" }, false), "E's hunt (director.n20)");
            Assert.AreSame(NextLine20.Line(new[] { "normal", "hold" }, false), NextLine20.Line(new[] { "normal", "hold" }, false), "cached");
            foreach (string kind in new[] { "hold", "gold", "swarm", "hunt", "boss", "elite", "exfil", "fog", "lethal" })
            {
                for (int k = 1; k <= NextLine20.Pips; k++)
                {
                    var kinds = new[] { "normal", "normal", "normal", "normal", "normal" };
                    kinds[k - 1] = kind;
                    string line = NextLine20.Line(kinds, false);
                    Assert.IsNotNull(line, kind);
                    Assert.LessOrEqual(Words(line), 3, line);
                }
            }
            Assert.AreEqual(0, NextLine20.Tint("normal"));
            Assert.AreEqual(1, NextLine20.Tint("hold"), "an objective: CYAN");
            Assert.AreEqual(1, NextLine20.Tint("exfil"));
            Assert.AreEqual(2, NextLine20.Tint("swarm"), "a threat: DANGER");
            Assert.AreEqual(2, NextLine20.Tint("hunt"));
            Assert.AreEqual(3, NextLine20.Tint("boss"), "the boss: DANGER, tall");
            Assert.AreEqual(5, NextLine20.Pips, "five pips");
        }

        // --- C1 the debut cards ----------------------------------------------------------------------------------------------

        [Test]
        public void EveryDebutCard_NamesItsKind_AndOneCounterInFourWords()
        {
            foreach (string kind in new[] { "swarmer", "spitter", "charger", "drone", "lancer", "warden", "bulwark" })
            {
                Assert.IsTrue(Debut20.Has(kind), kind);
                string name = Debut20.Name(kind), counter = Debut20.Counter(kind);
                Assert.LessOrEqual(Words(name), 2, kind);
                Assert.LessOrEqual(Words(counter), 4, $"{kind}: '{counter}'");
                foreach (char c in name + counter) Assert.LessOrEqual((int)c, 0xFF, kind);
                Assert.AreEqual("debut20." + kind, Debut20.Flag(kind));
                Assert.AreSame(Debut20.Flag(kind), Debut20.Flag(kind), "cached");
            }
            Assert.IsFalse(Debut20.Has("husk"), "the husk is the first thing you meet: no card");
            Assert.IsFalse(Debut20.Has(null));
            Assert.AreEqual("SIDESTEP INTO WALLS", Debut20.Counter("charger"), "the web's counter: a charger stunned on a wall (charger.js)");
        }

        [Test]
        public void ADebutCard_WaitsForACalmMoment_OncePerProfile()
        {
            Assert.IsTrue(Debut20.CanShow(false, UnityTuning.Hud20DebutClear + 0.1, false, true));
            Assert.IsFalse(Debut20.CanShow(true, 30, false, true), "seen in this profile");
            Assert.IsFalse(Debut20.CanShow(false, UnityTuning.Hud20DebutClear - 0.1, false, true), "never with an enemy within 8 m");
            Assert.IsFalse(Debut20.CanShow(false, 30, true, true), "never during a boss intro");
            Assert.IsFalse(Debut20.CanShow(false, 30, false, false), "playing only");
            Assert.AreEqual(8.0, UnityTuning.Hud20DebutClear, 1e-9, "the backlog's 8 m");
            Assert.LessOrEqual(UnityTuning.Hud20DebutLife, 1.5, "the world slowed 1.5 s at most");
            Assert.Greater(Slab20.Scale(0), 1.05f, "it slams in as a slab does");
            Assert.AreEqual(0f, Debut20.Alpha(UnityTuning.Hud20DebutLife), 1e-6);
            Assert.AreEqual(1f, Debut20.Alpha(0.4), 1e-6, "held");
            Assert.Less(Debut20.Alpha(UnityTuning.Hud20DebutLife - 0.01), 0.1f);
        }

        // --- the climb's off-screen arrow ------------------------------------------------------------------------------------

        [Test]
        public void TheEdgeArrow_PinsAnOffScreenPoint_ToTheInsetEdge_PointingAtIt()
        {
            var half = new UnityEngine.Vector2(960, 540);
            float inset = UnityTuning.Hud20ClimbArrowInset;
            Assert.IsFalse(EdgeArrow20.Place(new UnityEngine.Vector2(100, 50), false, half, inset, out _, out _), "on screen: no arrow");
            Assert.IsTrue(EdgeArrow20.Place(new UnityEngine.Vector2(3000, 0), false, half, inset, out UnityEngine.Vector2 right, out float aRight));
            Assert.AreEqual(960 - inset, right.x, 1e-3, "pinned to the right edge");
            Assert.AreEqual(0, right.y, 1e-3);
            Assert.AreEqual(-90f, aRight, 1e-3, "pointing right (the chevron points up at 0 deg)");
            Assert.IsTrue(EdgeArrow20.Place(new UnityEngine.Vector2(0, 2000), false, half, inset, out UnityEngine.Vector2 up, out float aUp));
            Assert.AreEqual(540 - inset, up.y, 1e-3, "a climber above: the top edge");
            Assert.AreEqual(0f, aUp, 1e-3);
            Assert.IsTrue(EdgeArrow20.Place(new UnityEngine.Vector2(100, 50), true, half, inset, out UnityEngine.Vector2 back, out _), "behind the camera: always an arrow");
            Assert.Less(back.y, 0f, "behind: the point is flipped (it's below-left, not up-right)");
            Assert.IsTrue(EdgeArrow20.Place(new UnityEngine.Vector2(1920, 1080), false, half, inset, out UnityEngine.Vector2 corner, out _));
            Assert.LessOrEqual(System.Math.Abs(corner.x), 960 - inset + 1e-3);
            Assert.LessOrEqual(System.Math.Abs(corner.y), 540 - inset + 1e-3);
            Assert.IsTrue(System.Math.Abs(System.Math.Abs(corner.x) - (960 - inset)) < 1e-3 || System.Math.Abs(System.Math.Abs(corner.y) - (540 - inset)) < 1e-3, "on the inset rect's border");
            Assert.IsTrue(EdgeArrow20.Place(UnityEngine.Vector2.zero, true, half, inset, out UnityEngine.Vector2 dead, out _), "straight behind: still an arrow");
            Assert.AreEqual(-(540 - inset), dead.y, 1e-3, "straight behind reads as below");
        }

        [Test]
        public void TheClimbers_AreTrackedFromStartToTop_AndExpire()
        {
            var t = new ClimbTrack20();
            t.On(new ClimbEvent { enemy = 7, phase = "start", lip = new Vec3(1, 6, 2) }, 0);
            Assert.AreEqual(1, t.Count);
            t.On(new ClimbEvent { enemy = 7, phase = "up", lip = new Vec3(1, 6, 2) }, 0.3);
            Assert.AreEqual(1, t.Count, "the same climber");
            t.On(new ClimbEvent { enemy = 9, phase = "start", lip = new Vec3(0, 6, 0) }, 0.4);
            Assert.AreEqual(2, t.Count);
            t.On(new ClimbEvent { enemy = 7, phase = "top" }, 1.0);
            Assert.AreEqual(1, t.Count, "mantled: done");
            Assert.AreEqual(9, t.Id(0));
            t.On(new ClimbEvent { enemy = 9, phase = "knocked" }, 1.1);
            Assert.AreEqual(0, t.Count, "knocked off: done");
            t.On(new ClimbEvent { enemy = 3, phase = "up" }, 2);
            t.Step(2 + UnityTuning.Hud20ClimbArrowMax + 0.01);
            Assert.AreEqual(0, t.Count, "a climb never reported done expires");
            for (int i = 0; i < 20; i++) t.On(new ClimbEvent { enemy = 100 + i, phase = "start" }, 3);
            Assert.AreEqual(UnityTuning.Hud20ClimbArrows, t.Count, "capped at the arrows' pool");
            t.Clear();
            Assert.AreEqual(0, t.Count);
            t.On(null, 0);
            Assert.AreEqual(0, t.Count);
        }

        // --- the earned heal's "+N" -----------------------------------------------------------------------------------------

        [Test]
        public void AnEarnedHeal_PopsItsHp_MergingQuickOnes()
        {
            var p = new HealPop20();
            Assert.IsFalse(p.Shown);
            p.Add(4, 0);
            Assert.IsTrue(p.Shown);
            Assert.AreEqual(4, p.Amount);
            p.Add(10, UnityTuning.Hud20HealMerge * 0.5);
            Assert.AreEqual(14, p.Amount, "a second heal inside the window sums");
            Assert.AreEqual(0, p.Age, 1e-9, "and restarts the pop");
            p.Step(UnityTuning.Hud20HealLife + 0.01);
            Assert.IsFalse(p.Shown, "gone after its life");
            p.Add(4, 10);
            Assert.AreEqual(4, p.Amount, "a later heal starts a new sum");
            p.Add(0.4, 10.01);
            Assert.AreEqual(4, p.Amount, "rounded to whole HP");
            Assert.Greater(HealPop20.Scale(0), 1.05f, "it punches in");
            Assert.AreEqual(1f, HealPop20.Scale(0.3), 1e-3);
            Assert.AreEqual(1f, HealPop20.Alpha(0.2), 1e-6);
            Assert.AreEqual(0f, HealPop20.Alpha(UnityTuning.Hud20HealLife), 1e-6);
            Assert.Greater(HealPop20.Rise(UnityTuning.Hud20HealLife), HealPop20.Rise(0), "it drifts up as it fades");
            p.Reset();
            Assert.IsFalse(p.Shown);
        }

        // --- the boss bar and the elite's plate (bottom centre) ------------------------------------------------------------------

        [Test]
        public void TheThreatBar_HoldsTheChunkWhite_ThenDrainsToTheFill()
        {
            var b = new ThreatBar20();
            b.Step(1f, 1.0 / 60);
            Assert.AreEqual(1f, b.Trail, 1e-6);
            b.Step(0.8f, 1.0 / 60); // a hit
            Assert.AreEqual(1f, b.Trail, 1e-6, "the chunk shows");
            Assert.IsTrue(b.Chunk, "the chunk is WHITE for its hold");
            for (int i = 0; i < (int)(UnityTuning.Hud20ThreatTrailHold * 60) - 2; i++) b.Step(0.8f, 1.0 / 60);
            Assert.AreEqual(1f, b.Trail, 1e-6, "held");
            for (int i = 0; i < 120; i++) b.Step(0.8f, 1.0 / 60);
            Assert.AreEqual(0.8f, b.Trail, 1e-6, "drained to the fill, never under it");
            Assert.IsFalse(b.Chunk);
            b.Step(0.9f, 1.0 / 60);
            Assert.AreEqual(0.9f, b.Trail, 1e-6, "a heal raises it with the fill");
            b.Reset();
            b.Step(0.5f, 1.0 / 60);
            Assert.AreEqual(0.5f, b.Trail, 1e-6, "a new threat starts where it is");
            Assert.AreEqual(0f, ThreatBar20.Sweep(0), 1e-6, "the bar fills in as it enters");
            Assert.AreEqual(1f, ThreatBar20.Sweep(UnityTuning.Hud20ThreatSweep), 1e-6);
        }

        [Test]
        public void TheThreatLine_NamesTheEliteAndItsAffixes()
        {
            Assert.AreEqual("BULWARK", ThreatBar20.Line("BULWARK", null, null));
            Assert.AreEqual("CHARGER · VOLATILE · SWIFT", ThreatBar20.Line("CHARGER", "volatile", "swift"));
            Assert.AreEqual("WARDEN · SHIELDED", ThreatBar20.Line("WARDEN", "shielded", null));
            Assert.AreSame(ThreatBar20.Line("CHARGER", "volatile", "swift"), ThreatBar20.Line("CHARGER", "volatile", "swift"), "cached");
            Assert.IsNull(ThreatBar20.Line(null, "swift", null));
        }

        [Test]
        public void TheThreatBar_StaysClearOfTheVitalsTheCardAndTheCentre()
        {
            var cases = new[] { (1920f, 1f), (1920f, 1.25f), (1728f, 1.25f), (2560f, 1.25f), (1440f, 1f) };
            foreach (var (width, scale) in cases)
            {
                foreach (bool boss in new[] { true, false })
                {
                    HudRect bar = Hud20Layout.ThreatBar(width, boss, scale);
                    HudRect onScreen = new HudRect(width * 0.5f - (width * 0.5f - bar.x) * scale, bar.y * scale, bar.w * scale, bar.h * scale);
                    Assert.IsFalse(Hud19Layout.Overlaps(onScreen, Hud19Layout.CentreBox(width, 1080f)), $"the centre at {width} x {scale}");
                    Assert.IsFalse(Hud19Layout.Overlaps(onScreen, Hud19Layout.OnScreen(Hud19Layout.Plate(), scale, false, width)), $"the vitals at {width} x {scale}");
                    Assert.IsFalse(Hud19Layout.Overlaps(onScreen, Hud19Layout.OnScreen(Hud19Layout.CardRect(UnityTuning.Hud17StripCells, true, width), scale, true, width)), $"the card at {width} x {scale}");
                    Assert.Greater(bar.w, 200f, "a bar, not a stub");
                }
            }
            Assert.Greater(Hud20Layout.ThreatBar(1920, true, 1f).w, Hud20Layout.ThreatBar(1920, false, 1f).w, "the boss's bar is the wider");
        }

        // --- F3 the reward reveal ---------------------------------------------------------------------------------------------

        static readonly string[] PowerTypes = { "double", "overclock", "shockwave", "stasis", "refund" };

        [Test]
        public void TheRevealsOffers_AreOneOfEach_SeededByTheRunAndTheRound()
        {
            var a = new RevealOffer[3];
            var b = new RevealOffer[3];
            for (uint seed = 1; seed <= 50; seed++)
            {
                for (int round = 5; round <= 20; round += 5)
                {
                    Reveal20.Offers(seed, round, 977, 500, 100, PowerTypes, true, a);
                    Reveal20.Offers(seed, round, 977, 500, 100, PowerTypes, true, b);
                    int power = 0, mod = 0, points = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        Assert.AreEqual(a[i].kind, b[i].kind, "replays: the same run and round, the same offers");
                        Assert.AreEqual(a[i].type, b[i].type);
                        if (a[i].kind == Reveal20.PowerUp) { power++; Assert.Contains(a[i].type, PowerTypes); }
                        if (a[i].kind == Reveal20.Mod) mod++;
                        if (a[i].kind == Reveal20.Points) { points++; Assert.AreEqual(500 + 100 * round, a[i].points, "the round's points (whole tens)"); }
                    }
                    Assert.AreEqual(1, power, "a power-up");
                    Assert.AreEqual(1, mod, "a MODS pick");
                    Assert.AreEqual(1, points, "points");
                }
            }
            var orders = new HashSet<int>();
            for (uint seed = 1; seed <= 50; seed++)
            {
                Reveal20.Offers(seed, 10, 977, 500, 100, PowerTypes, true, a);
                orders.Add(a[0].kind * 9 + a[1].kind * 3 + a[2].kind);
            }
            Assert.Greater(orders.Count, 2, "the slots' order is shuffled by the seed");
        }

        [Test]
        public void WithoutMods_TheReveal_OffersTwoDifferentPowerUps()
        {
            var a = new RevealOffer[3];
            for (uint seed = 1; seed <= 30; seed++)
            {
                Reveal20.Offers(seed, 8, 977, 500, 100, PowerTypes, false, a);
                int power = 0;
                string first = null;
                for (int i = 0; i < 3; i++)
                {
                    Assert.AreNotEqual(Reveal20.Mod, a[i].kind, "no MODS pick with the mods off");
                    if (a[i].kind != Reveal20.PowerUp) continue;
                    power++;
                    if (first == null) first = a[i].type;
                    else Assert.AreNotEqual(first, a[i].type, "two different power-ups");
                }
                Assert.AreEqual(2, power);
            }
            Assert.AreEqual(1260, Reveal20.PointsFor(7, 555, 101), "555 + 7 x 101 = 1262: to the nearest ten");
            Assert.AreEqual(1270, Reveal20.PointsFor(7, 558, 101), "1265: half up");
        }

        [Test]
        public void TheReveal_ShowsItsSlotsInTurn_WithinPointEightSeconds()
        {
            Assert.Less(Reveal20.SlotIn(0, 0), 0.95f, "nothing whole on its first frame (M9)");
            Assert.AreEqual(0f, Reveal20.SlotIn(0.1, 2), 1e-6, "the third waits its turn");
            Assert.AreEqual(1f, Reveal20.SlotIn(UnityTuning.Hud20RevealTime, 2), 1e-6, "all three whole by 0.8 s");
            Assert.AreEqual(0.8, UnityTuning.Hud20RevealTime, 1e-9, "the backlog's 0.8 s");
            Assert.IsFalse(Reveal20.Ready(UnityTuning.Hud20RevealTime - 0.01), "no pick before the reveal ends (a jump's A never takes a slot)");
            Assert.IsTrue(Reveal20.Ready(UnityTuning.Hud20RevealTime));
            Assert.IsTrue(Reveal20.Triggers("hunt", "end"));
            Assert.IsTrue(Reveal20.Triggers("elite", "end"));
            Assert.IsTrue(Reveal20.Triggers("boss", "end"));
            Assert.IsFalse(Reveal20.Triggers("hunt", "announce"));
            Assert.IsFalse(Reveal20.Triggers("fog", "end"), "a twist's end pays nothing");
        }

        // --- E3 the CODEX ----------------------------------------------------------------------------------------------------

        [Test]
        public void TheCodex_ListsEveryKindAndEveryCombo_InFewWords()
        {
            Assert.AreEqual(new[] { "husk", "swarmer", "drone", "spitter", "charger", "lancer", "warden", "bulwark" }, Codex20.Enemies, "the web's roster in the order a run meets them");
            Assert.AreEqual(new[] { "POP", "LAUNCH + SNIPE", "CONDUCTOR", "NAILBOMB", "SWAP-CANCEL", "SENT" }, Codex20.Combos, "builder G's names");
            foreach (string k in Codex20.Enemies)
            {
                Assert.IsNotNull(Codex20.EnemyName(k), k);
                Assert.LessOrEqual(Words(Codex20.EnemyLine(k)), 4, k);
            }
            foreach (string c in Codex20.Combos)
            {
                Assert.LessOrEqual(Words(Codex20.ComboHow(c)), 6, c);
                foreach (char ch in Codex20.ComboHow(c)) Assert.LessOrEqual((int)ch, 0xFF, c);
            }
            Assert.AreEqual(Debut20.Counter("charger"), Codex20.EnemyLine("charger"), "an enemy's entry is its debut card");
        }

        [Test]
        public void TheCodex_RecordsOncePerProfile_AndCountsWhatsFound()
        {
            var p = new Profile();
            Assert.IsFalse(Codex20.EnemyMet(p, "charger"));
            Assert.IsFalse(Codex20.EnemyMet(p, "husk"), "a fresh profile has met nothing");
            Assert.IsTrue(Codex20.RecordEnemy(p, "charger"), "the first time: new");
            Assert.IsFalse(Codex20.RecordEnemy(p, "charger"), "once per profile");
            Assert.IsTrue(Codex20.EnemyMet(p, "charger"));
            Assert.IsFalse(Codex20.RecordEnemy(p, "rival"), "a kind outside the codex isn't recorded");
            p.SetFlag(Debut20.Flag("spitter"));
            Assert.IsTrue(Codex20.EnemyMet(p, "spitter"), "its debut card counts as met");
            p.runs = 1;
            Assert.IsTrue(Codex20.EnemyMet(p, "husk"), "a finished run met the husk");
            Assert.IsTrue(Codex20.RecordCombo(p, "POP"));
            Assert.IsFalse(Codex20.RecordCombo(p, "POP"));
            Assert.IsFalse(Codex20.RecordCombo(p, "NOT A COMBO"));
            Assert.IsTrue(Codex20.ComboFound(p, "POP"));
            Assert.IsFalse(Codex20.ComboFound(p, "SENT"));
            Assert.AreEqual(3, Codex20.EnemiesMet(p));
            Assert.AreEqual(1, Codex20.CombosFound(p));
            Assert.AreSame(Codex20.EnemyFlag("charger"), Codex20.EnemyFlag("charger"), "cached flags");
            Assert.AreEqual("ENEMIES 3 / 8", Codex20.EnemiesHead(3));
            Assert.AreEqual("COMBOS 1 / 6", Codex20.CombosHead(1));
        }

        [Test]
        public void TheCodex_ListsTheAffixes_AndRecordsThem()
        {
            Assert.AreEqual(new[] { "shielded", "volatile", "blinker", "regenerating", "swift" }, Codex20.Affixes, "affixes.js AFFIXES, the web's order");
            foreach (string a in Codex20.Affixes)
            {
                Assert.AreEqual(a.ToUpperInvariant(), Codex20.AffixName(a));
                Assert.LessOrEqual(Words(Codex20.AffixLine(a)), 5, a);
                foreach (char ch in Codex20.AffixLine(a)) Assert.LessOrEqual((int)ch, 0xFF, a);
            }
            var p = new Profile();
            Assert.IsFalse(Codex20.AffixMet(p, "swift"));
            Assert.IsTrue(Codex20.RecordAffix(p, "swift"));
            Assert.IsFalse(Codex20.RecordAffix(p, "swift"), "once per profile");
            Assert.IsFalse(Codex20.RecordAffix(p, "nope"));
            Assert.IsTrue(Codex20.AffixMet(p, "swift"));
            Assert.AreEqual(1, Codex20.AffixesMet(p));
            Assert.AreEqual("AFFIXES 1 / 5", Codex20.AffixesHead(1));
        }

        [Test]
        public void ATwist_NamesItsSlab_AndItsNextWord()
        {
            int p;
            Assert.AreEqual("FOG ROUND", Announcer20.ForSpecial(new SpecialRoundEvent { name = "twist:fog", phase = "announce" }, out p), "A5's twists arrive as twist:<id>");
            Assert.AreEqual("SWARMAGEDDON", Announcer20.ForSpecial(new SpecialRoundEvent { name = "twist:swarmageddon", phase = "start" }, out p), "named at its start too (once a round)");
            Assert.AreEqual("LETHAL ROUND", Announcer20.ForSpecial(new SpecialRoundEvent { name = "twist:lethal", phase = "announce" }, out p));
            Assert.AreEqual("GOLDEN ROUND", Announcer20.ForSpecial(new SpecialRoundEvent { name = "twist:golden", phase = "announce" }, out p));
            Assert.IsFalse(Announcer20.Danger("GOLDEN ROUND"));
            Assert.IsTrue(Announcer20.Danger("LETHAL ROUND"));
            Assert.IsNull(Announcer20.ForSpecial(new SpecialRoundEvent { name = "twist:fog", phase = "end" }, out p));
            Assert.AreEqual("FOG", NextLine20.Word("twist:fog"));
            Assert.AreEqual("NEXT: LETHAL", NextLine20.Line(new[] { "twist:lethal" }, false));
            Assert.AreEqual(2, NextLine20.Tint("twist:swarmageddon"), "a threat");
            Assert.AreEqual(1, NextLine20.Tint("twist:golden"), "a gift");
        }

        // --- F4's player-facing half: WHAT GOT YOU's tell ---------------------------------------------------------------------

        /// <summary>config.ui.recap.names (PortExport): the sources' display names DeathRecap's killer carries.</summary>
        static Dictionary<string, string> RecapNames() => new Dictionary<string, string>
        {
            ["husk"] = "HUSK", ["swarmer"] = "SWARMER", ["spitter"] = "SPITTER GLOB", ["acid"] = "ACID", ["charger"] = "CHARGER",
            ["lancer"] = "LANCER", ["warden"] = "WARDEN", ["drone"] = "DRONE ORB", ["rival"] = "RIVAL", ["fall"] = "THE FALL",
            ["boss"] = "BOSS", ["affix:volatile"] = "VOLATILE BLAST", ["affix"] = "AN AFFIX", ["unknown"] = "SOMETHING",
        };

        [Test]
        public void WhatGotYou_NamesTheKillersKind_AndItsTell_InFourWords()
        {
            var names = RecapNames();
            var tell = new DownTell20(names);
            Assert.AreEqual("CHARGER · SIDESTEP INTO WALLS", tell.Line("CHARGER"), "the kind and its debut card's counter");
            Assert.AreEqual("LANCER · OUTRUN THE LASER", tell.Line("LANCER"));
            Assert.AreEqual("SPITTER · PUNCH THE GLOBS BACK", tell.Line("SPITTER GLOB"), "the glob's source is the spitter");
            Assert.AreEqual("SPITTER · STAY OFF THE ACID", tell.Line("ACID"));
            Assert.AreEqual("DRONE · PUNCH THE ORBS BACK", tell.Line("DRONE ORB"));
            Assert.AreEqual("VOLATILE · BACK OFF ITS BURST", tell.Line("VOLATILE BLAST"));
            Assert.AreEqual("HUSK · PUNCH ITS SWING BACK", tell.Line("HUSK"));
            Assert.IsNull(tell.Line("THE FALL"), "not an enemy: no tell");
            Assert.IsNull(tell.Line("RIVAL"), "the RIVAL is magenta's: no tell");
            Assert.IsNull(tell.Line(null));
            Assert.IsNull(tell.Line("SOMETHING"));
            Assert.AreSame(tell.Line("CHARGER"), tell.Line("CHARGER"), "cached");
            foreach (string kind in new[] { "husk", "swarmer", "spitter", "acid", "charger", "lancer", "warden", "drone", "affix:volatile", "boss" })
            {
                string line = tell.Line(names[kind]);
                Assert.IsNotNull(line, kind);
                string after = line.Substring(line.IndexOf('·') + 1);
                Assert.LessOrEqual(Words(after), 4, $"{kind}: '{after}'");
            }
            foreach (string kind in new[] { "swarmer", "charger", "lancer", "warden" })
                Assert.IsTrue(tell.Line(names[kind]).EndsWith(Debut20.Counter(kind)), $"{kind}: the same counter as its debut card");
        }

        // --- banned 10 (row 4, builder HV): the reward reveal's keys are drawn key chips, not words -------------------------------

        [Test]
        public void TheRevealsKeys_AreDrawnChips_TheSkipAChipAndAVerb()
        {
            // the slots' 1 / 2 / 3 on drawn key caps (KeyChip: DOWN's retry, WHAT GOT YOU's skip), cached
            Assert.AreEqual("1", RevealPrompt20.SlotKey(0));
            Assert.AreEqual("3", RevealPrompt20.SlotKey(2));
            Assert.AreSame(RevealPrompt20.SlotKey(1), RevealPrompt20.SlotKey(1));
            Assert.IsNull(RevealPrompt20.SlotKey(3));
            // the mouse and keyboard: an ESC chip beside the verb (was "ESC  SKIP" as one muted word line)
            Assert.AreEqual("ESC", RevealPrompt20.SkipChip(false));
            Assert.AreEqual("SKIP", RevealPrompt20.SkipVerb(false, GlyphSet.Xbox));
            // a pad: no chip; the verb carries the pad's B glyph (a sprite), never the letters
            Assert.IsNull(RevealPrompt20.SkipChip(true));
            string pad = RevealPrompt20.SkipVerb(true, GlyphSet.Xbox);
            StringAssert.EndsWith("SKIP", pad);
            StringAssert.StartsWith(PromptText.Button(PadButtonName.B, GlyphSet.Xbox), pad);
            Assert.AreSame(pad, RevealPrompt20.SkipVerb(true, GlyphSet.Xbox), "cached per glyph set");
        }

        // --- A1: the BULWARK's plates on its bar (BossBar.pips / pipsOn: bulwark.js's five; a wall ram drops the weakest) -------

        [Test]
        public void TheBossBar_ShowsItsPlates_ALostOneFlashesThenGoesDark()
        {
            // 5 plates, 4 on: pips 0..3 on (DANGER: still armoured), pip 4 just lost (WHITE, punched), then dark (INK-SOFT)
            for (int i = 0; i < 4; i++) Assert.AreEqual(0, BossPips20.State(i, 4, 4, 0), $"pip {i} on");
            Assert.AreEqual(1, BossPips20.State(4, 4, 4, 0), "the plate just lost flashes");
            Assert.AreEqual(1, BossPips20.State(4, 4, 4, UnityTuning.Hud20PipFlash * 0.5));
            Assert.AreEqual(2, BossPips20.State(4, 4, 4, UnityTuning.Hud20PipFlash), "then dark");
            Assert.AreEqual(2, BossPips20.State(4, 4, -1, 0), "lost before this bar showed: dark at once");
            Assert.That(BossPips20.Scale(0), Is.GreaterThan(1.2f), "the lost pip punches");
            Assert.That(BossPips20.Scale(UnityTuning.Hud20PipFlash), Is.EqualTo(1f).Within(1e-6));
            Assert.That(BossPips20.Scale(UnityTuning.Hud20PipFlash * 0.5), Is.InRange(1f, BossPips20.Scale(0)), "settling");
            Assert.That(BossPips20.Scale(0, true), Is.EqualTo(1f), "REDUCED MOTION: no punch");
        }

        // --- B5: the TORCH's card (builder G's port): its role on the card and WEAPONS' role card, like every gun's -------------

        [Test]
        public void TheTorch_HasARoleOnItsCard_ShortAndInTheCharset()
        {
            string tag = WeaponRole19.Tag("torch"), good = WeaponRole19.GoodAgainst("torch"), combos = WeaponRole19.CombosWith("torch");
            Assert.IsNotNull(tag, "the card's tag");
            Assert.IsNotNull(good, "WEAPONS' GOOD AGAINST");
            Assert.IsNotNull(combos, "WEAPONS' COMBOS");
            Assert.LessOrEqual(Words(tag), 3, "3 words on the HUD (IDENTITY.md)");
            Assert.LessOrEqual(Words(good), 8);
            Assert.LessOrEqual(Words(combos), 8);
            foreach (char c in tag + good + combos) Assert.LessOrEqual((int)c, 0xFF, $"'{c}' is outside the static atlas");
            StringAssert.Contains("FIRE WALL", good + " " + combos, "its special named where it's taught (the hint is the teaching tier, gone at rest)");
        }

        [Test]
        public void TheVortex_HasARoleOnItsCard_ItsCollapseNamed()
        {
            string tag = WeaponRole19.Tag("vortex"), good = WeaponRole19.GoodAgainst("vortex"), combos = WeaponRole19.CombosWith("vortex");
            Assert.IsNotNull(tag, "the card's tag");
            Assert.IsNotNull(good);
            Assert.IsNotNull(combos);
            Assert.LessOrEqual(Words(tag), 3, "3 words on the HUD (IDENTITY.md)");
            Assert.LessOrEqual(Words(good), 8);
            Assert.LessOrEqual(Words(combos), 8);
            foreach (char c in tag + good + combos) Assert.LessOrEqual((int)c, 0xFF, $"'{c}' is outside the static atlas");
            StringAssert.Contains("COLLAPSE", combos, "arsenalMath.js: PULL then a blast hit is COLLAPSE");
        }

        // --- the LANCER's off-screen tell -------------------------------------------------------------------------------------

        [Test]
        public void TheLancersTell_GrowsWithItsAim_AndPeaksAtTheShot()
        {
            Assert.AreEqual(0f, LancerTell20.Alpha(-1), 1e-6, "not aiming: nothing");
            float start = LancerTell20.Alpha(0), mid = LancerTell20.Alpha(0.5), end = LancerTell20.Alpha(0.999);
            Assert.Greater(start, 0.3f, "it reads from the aim's first frame");
            Assert.Less(start, 0.6f, "... and enters (M9: <= 0.6 of its peak)");
            Assert.Greater(mid, start);
            Assert.AreEqual(1f, end, 0.01, "whole at the shot");
            Assert.AreEqual(1f, LancerTell20.Scale(0), 1e-6);
            Assert.Greater(LancerTell20.Scale(0.95), 1.15f, "it swells in the last of the 1.2 s");
            Assert.LessOrEqual(LancerTell20.Scale(1), UnityTuning.Hud20LancerSwell + 1e-6f);
            Assert.AreEqual(1f, LancerTell20.Scale(0.95, true), 1e-6, "REDUCED MOTION: no swell");
        }

        // --- the card with the chip stays out of the centre (S14) ----------------------------------------------------------

        [Test]
        public void TheChipsTab_SitsOnTheCardOverItsSlot_OutOfTheCentralBox()
        {
            // the tab stands on the card's top edge, centred over the THUNDERCLAP's slot (index i of `visible`, right-aligned)
            HudRect card = Hud19Layout.CardRect(3, false, 1920f);
            HudRect tab = Hud20Layout.ChipTab(2, 3, 1920f);
            Assert.AreEqual(card.y + Hud19Layout.CardHeight, tab.y, 1e-3, "on the card's top edge");
            float cellRight = 1920f - UnityTuning.ThemeSafeInset - UnityTuning.ThemeAccentBar - UnityTuning.Hud17StripPad;
            Assert.AreEqual(cellRight - UnityTuning.Hud17StripCellW * 0.5f, tab.x + tab.w * 0.5f, 1e-3, "the rightmost slot's centre");
            HudRect left = Hud20Layout.ChipTab(0, 3, 1920f);
            Assert.AreEqual(2 * (UnityTuning.Hud17StripCellW + UnityTuning.Hud17StripGap), tab.x - left.x, 1e-3, "two slots to the left");
            Assert.GreaterOrEqual(tab.h, UnityTuning.Hud20ShockChipD, "the ring fits its tab");
            // NIGHT19's card cases (Hud19Tests: 4:3 at HUD SCALE 1 only; at 4:3 x 1.25 the card alone reaches the box)
            var cases = new[] { (1920f, 1f), (1920f, 1.25f), (1728f, 1.25f), (2560f, 1.25f), (1440f, 1f) };
            foreach (var (width, scale) in cases)
            {
                HudRect box = Hud19Layout.CentreBox(width, 1080f);
                for (int visible = 1; visible <= UnityTuning.Hud17StripCells; visible++)
                {
                    for (int i = 0; i < visible; i++)
                        Assert.IsFalse(Hud19Layout.Overlaps(Hud19Layout.OnScreen(Hud20Layout.ChipTab(i, visible, width), scale, true, width), box), $"the tab over slot {i} of {visible} at {width}, HUD SCALE {scale}");
                }
            }
        }
    }
}
