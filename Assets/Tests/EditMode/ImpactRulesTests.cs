// NIGHT16 U3-29.5 (builder I): ImpactRules (Core/ImpactRules.cs), the impact layer's rules: the tier each moment earns (from
// the bus: hit, kill, parry, roundClear and the sim's own hitstop), the longest request wins, each tier's cooldown, the
// budget (a rolling second and a rolling minute; parries exempt), HIT FREEZE off / light, and the slow-mos (never stacked,
// the strongest wins, at most one start in any minGap s, none while DOWN, off with SLOW-MO off). Every number is read from
// design.impact (Design/unity-config.json) through GameConfig, never typed here. Real time is driven by BeginFrame, as
// SurvivalGame's loop does. The input-latch test (a jump pressed and released inside a freeze still jumps) is
// Tests/EditMode/Game/ImpactLatchTests.cs: it needs the Game's input reader.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class ImpactRulesTests
    {
        static GameConfig cfg;
        const double Frame = 1.0 / 120;
        const double Eps = 1e-9;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static double F(string key) => cfg.Num("design.impact." + key);

        /// <summary>A fresh rules object whose clock has run well past every cooldown and slow-mo gap.</summary>
        static ImpactRules Fresh()
        {
            var r = new ImpactRules(cfg);
            Run(r, 10);
            return r;
        }

        /// <summary>Real time passes in 1/120 s frames (no step in them: the frames of a freeze, or of nothing).</summary>
        static void Run(ImpactRules r, double seconds, bool blocked = false)
        {
            int n = (int)System.Math.Round(seconds / Frame);
            for (int i = 0; i < n; i++) r.BeginFrame(Frame, blocked);
        }

        static HitEvent Hit(string weapon, bool head = false, bool killed = false, int shot = 1, double damage = 10, double distance = 5, bool melee = false) =>
            new HitEvent { owner = "player", weapon = weapon, kind = "husk", headshot = head, killed = killed, shot = shot, damage = damage, distance = distance, melee = melee };

        static KillEvent Kill(string weapon = "revolver", bool head = false) => new KillEvent { owner = "player", weapon = weapon, kind = "husk", headshot = head };

        /// <summary>One step in which `moments` land: the bus's order (hit, then kill for a killing hit), then the loop's resolve.</summary>
        static bool Step(ImpactRules r, params object[] moments)
        {
            foreach (object m in moments)
            {
                if (m is HitEvent h)
                {
                    r.OnHit(h);
                    if (h.killed) r.OnKill(Kill(h.weapon, h.headshot));
                }
                else if (m is string s && s == "roundClear") r.OnRoundClear();
                else if (m is string c) r.OnSimHitstop(c);
            }
            return r.EndStep(false);
        }

        [Test]
        public void TierChoice_EachMomentEarnsItsTier()
        {
            void Expect(string what, ImpactTier tier, string key, params object[] moments)
            {
                ImpactRules r = Fresh();
                Assert.That(Step(r, moments), Is.True, what + ": a freeze began");
                Assert.That(r.granted, Is.EqualTo(tier), what);
                Assert.That(r.grantedSeconds, Is.EqualTo(F("freeze." + key)).Within(Eps), what + ": design.impact.freeze." + key);
                Assert.That(r.FreezeLeft, Is.EqualTo(r.grantedSeconds).Within(Eps), what + ": the loop's freeze");
                Assert.That(r.grantedSeconds, Is.LessThanOrEqualTo(F("budget.maxFreeze")), what + ": within the hard cap (design.impact.budget.maxFreeze)");
            }

            Expect("a body kill", ImpactTier.Kill, "kill", Hit("revolver", killed: true));
            Expect("a headshot", ImpactTier.Headshot, "headshot", Hit("revolver", head: true));
            Expect("a headshot kill", ImpactTier.HeadshotKill, "headshotKill", Hit("revolver", head: true, killed: true));
            Expect("a punch that connects", ImpactTier.PunchHit, "punchHit", Hit("punch", melee: true));
            Expect("a gauntlet hit", ImpactTier.PunchHit, "punchHit", Hit("gauntlet", melee: true));
            Expect("a punch kill", ImpactTier.PunchKill, "punchKill", Hit("punch", killed: true, melee: true));
            Expect("a rail hit", ImpactTier.HeavyHit, "heavyHit", Hit("rail"));
            double pellet = cfg.Num("weapons.shotgun.pelletDamage"), heavy = F("freeze.heavyPellets");
            Expect("a shotgun shot landing heavyPellets pellets in one body", ImpactTier.HeavyHit, "heavyHit", Hit("shotgun", damage: pellet * heavy));
            Expect("a rocket's direct hit", ImpactTier.HeavyHit, "heavyHit", Hit("rocket", damage: 60, distance: 0.5));

            ImpactRules none = Fresh();
            Assert.That(Step(none, Hit("revolver")), Is.False, "a plain body hit: no freeze");
            Assert.That(Step(none, Hit("shotgun", damage: pellet * (heavy - 1))), Is.False, "fewer pellets than heavyPellets: no freeze");
            Assert.That(Step(none, Hit("rocket", damage: 20, distance: cfg.Num("weapons.rocket.splashRadius") * 0.9)), Is.False, "a rocket's splash at the edge: no freeze");
            Assert.That(none.FreezeLeft, Is.EqualTo(0));
        }

        [Test]
        public void Parries_ThePerfectOneFreezesLonger_AndGetsItsSlowMoTailAfterTheFreeze()
        {
            ImpactRules r = Fresh();
            r.OnParry(new HitEvent { owner = "player", weapon = "punch", perfect = false });
            r.OnSimHitstop("parry"); // the sim's own request (design.ultra.parryHitstop): the parry tier covers it
            Assert.That(r.EndStep(false), Is.True);
            Assert.That((r.granted, r.grantedSeconds), Is.EqualTo((ImpactTier.Parry, F("freeze.parry"))));
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.None), "a plain parry: no slow-mo");

            r = Fresh();
            r.OnParry(new HitEvent { owner = "player", weapon = "punch", perfect = true });
            r.OnSimHitstop("parry"); // the sim's design.parry.perfectHitstop (0.15) stays its own request: the longer tier wins
            Assert.That(r.EndStep(false), Is.True);
            Assert.That((r.granted, r.grantedSeconds), Is.EqualTo((ImpactTier.PerfectParry, F("freeze.perfectParry"))));
            Assert.That(r.grantedSeconds, Is.GreaterThan(F("freeze.parry")), "the inversion fixed: a PERFECT PARRY freezes longer than a parry");
            Assert.That(r.grantedSeconds, Is.GreaterThan(cfg.Num("design.parry.perfectHitstop")), "longer than the sim's own perfect request");
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.PerfectParry));
            double freeze = r.grantedSeconds, scale = F("slowMo.perfectParry.scale");
            r.BeginFrame(Frame, false);
            Assert.That(r.Scale, Is.EqualTo(0), "the freeze first");
            Run(r, freeze - Frame + Frame / 2);
            r.BeginFrame(Frame, false);
            Assert.That(r.Scale, Is.EqualTo(scale).Within(1e-6), "then the tail at design.impact.slowMo.perfectParry.scale");
        }

        [Test]
        public void LongestWins_OneFreezePerStep()
        {
            ImpactRules r = Fresh();
            r.OnHit(Hit("revolver", head: true, killed: true));
            r.OnKill(Kill("revolver", true));
            r.OnSimHitstop("kill");
            r.OnHit(Hit("rail"));
            r.OnParry(new HitEvent { owner = "player", weapon = "punch" });
            Assert.That(r.EndStep(false), Is.True);
            Assert.That((r.granted, r.grantedSeconds), Is.EqualTo((ImpactTier.Parry, F("freeze.parry"))), "the parry is the longest request");
            int total = 0;
            for (int t = 0; t < ImpactRules.TierCount; t++) total += r.freezes[t];
            Assert.That(total, Is.EqualTo(1), "the shorter requests merged into the one freeze");
            Assert.That(r.skippedMask, Is.EqualTo(0), "merged, not skipped");
        }

        [Test]
        public void Cooldown_AKillFreezeAtMostEveryCooldownSeconds_TheSkipIsLogged()
        {
            ImpactRules r = Fresh();
            double cd = F("cooldown.kill"), kill = F("freeze.kill");
            Assert.That(Step(r, Hit("revolver", killed: true)), Is.True);
            Run(r, kill); // the freeze runs out
            Run(r, cd - kill - 2 * Frame); // still inside the cooldown
            Assert.That(Step(r, Hit("revolver", killed: true)), Is.False, "inside design.impact.cooldown.kill: skipped");
            Assert.That(r.skippedMask & (1 << (int)ImpactTier.Kill), Is.Not.EqualTo(0), "the skip is logged");
            Assert.That(r.skips[(int)ImpactTier.Kill], Is.EqualTo(1));
            Run(r, System.Math.Max(3 * Frame, F("cooldown.any") - cd + 3 * Frame)); // past the tier's cooldown and the shared gap
            Assert.That(Step(r, Hit("revolver", killed: true)), Is.True, "past the cooldown: a freeze again");
            Assert.That(r.freezes[(int)ImpactTier.Kill], Is.EqualTo(2));
        }

        [Test]
        public void MomentumFloor_ThePunchTierIsLonger_AnotherMeleeWeaponKeepsTheSimsFloor()
        {
            ImpactRules r = Fresh();
            Assert.That(Step(r, Hit("punch", melee: true), "momentum"), Is.True);
            Assert.That(r.granted, Is.EqualTo(ImpactTier.PunchHit), "the punch tier is longer than feel.momentumHitstop");
            r = Fresh();
            Assert.That(Step(r, Hit("sunder", melee: true), "momentum"), Is.True, "a melee weapon without a tier: the momentum floor");
            Assert.That((r.granted, r.grantedSeconds), Is.EqualTo((ImpactTier.Momentum, cfg.Num("feel.momentumHitstop"))));
        }

        // --- NIGHT16 review fixes (finding 1: the 3-hitch chop; 3: parry chains; 2: automation's settings; 5: TITLE) ------------

        /// <summary>The most freeze starts in any rolling second (half-open) and the shortest start-to-start gap, over `starts`.</summary>
        static (int most, double gap) Rhythm(System.Collections.Generic.List<double> starts)
        {
            int most = 0;
            double gap = double.PositiveInfinity;
            for (int i = 0; i < starts.Count; i++)
            {
                int n = 0;
                for (int j = i; j < starts.Count && starts[j] < starts[i] + 1 - 1e-9; j++) n++;
                most = System.Math.Max(most, n);
                if (i > 0) gap = System.Math.Min(gap, starts[i] - starts[i - 1]);
            }
            return (most, gap);
        }

        [Test]
        public void AnyGap_AutomaticHeadshots_NoSecondWithThreeFreezes()
        {
            ImpactRules r = Fresh();
            var starts = new System.Collections.Generic.List<double>();
            double any = F("cooldown.any");
            Assert.That(any, Is.GreaterThan(0), "design.impact.cooldown.any");
            for (int shot = 1; shot <= 60; shot++) // a headshot every 0.1 s for 6 s, each its own trigger pull
            {
                if (Step(r, Hit("revolver", head: true, shot: shot))) starts.Add(r.Clock);
                Run(r, 0.1);
            }
            (int most, double gap) = Rhythm(starts);
            Assert.That(starts.Count, Is.GreaterThan(4), "the headshots still freeze");
            Assert.That(most, Is.LessThanOrEqualTo(2), "no second with three freezes (the reviewer's 3-hitch chop)");
            Assert.That(gap, Is.GreaterThanOrEqualTo(any - 1e-9), "design.impact.cooldown.any between any two budgeted freezes");
        }

        [Test]
        public void AnyGap_AHordesKills_NoSecondWithThreeFreezes_AndTheSkipsAreLogged()
        {
            ImpactRules r = Fresh();
            var starts = new System.Collections.Generic.List<double>();
            for (int i = 0; i < 120; i++) // a kill every 0.05 s for 6 s
            {
                if (Step(r, Hit("revolver", killed: true, shot: 100 + i))) starts.Add(r.Clock);
                Run(r, 0.05);
            }
            Assert.That(Rhythm(starts).most, Is.LessThanOrEqualTo(2));
            Assert.That(r.skips[(int)ImpactTier.Kill], Is.GreaterThan(0));
        }

        [Test]
        public void AnyGap_AHigherTierMayStartInsideIt_ASameTierMayNot()
        {
            ImpactRules r = Fresh();
            Assert.That(Step(r, Hit("revolver", killed: true, shot: 1)), Is.True, "a body kill");
            Run(r, 0.3);
            Assert.That(Step(r, Hit("revolver", head: true, killed: true, shot: 2)), Is.True, "a headshot kill 0.3 s later: a higher tier starts inside the gap");
            Assert.That(r.granted, Is.EqualTo(ImpactTier.HeadshotKill));

            r = Fresh();
            Assert.That(Step(r, Hit("revolver", head: true, killed: true, shot: 1)), Is.True);
            Run(r, 0.3);
            Assert.That(Step(r, Hit("revolver", head: true, killed: true, shot: 2)), Is.False, "the same tier 0.3 s later: skipped");
            Assert.That(r.skippedMask & (1 << (int)ImpactTier.HeadshotKill), Is.Not.EqualTo(0), "the skip is logged");
            Run(r, 0.1);
            Assert.That(Step(r, Hit("revolver", killed: true, shot: 3)), Is.False, "a lower tier inside the gap: skipped");
        }

        [Test]
        public void AnyGap_TheBypassRestartsTheGap_AndNeverMakesThreeStartsInASecond()
        {
            ImpactRules r = Fresh();
            Assert.That(Step(r, Hit("revolver", killed: true, shot: 1)), Is.True);
            Run(r, 0.3);
            Assert.That(Step(r, Hit("revolver", head: true, killed: true, shot: 2)), Is.True, "the bypass");
            Run(r, 0.3);
            Assert.That(Step(r, Hit("punch", killed: true, melee: true, shot: 3)), Is.False, "a still higher tier 0.3 s after the bypass: at most one bypass per gap (two starts in the second already)");
            Run(r, 0.3);
            Assert.That(Step(r, Hit("revolver", killed: true, shot: 4)), Is.False, "0.6 s after the bypass but 0.9 s after the first: a third start in a second, skipped");
            Run(r, 0.15);
            Assert.That(Step(r, Hit("revolver", killed: true, shot: 5)), Is.True, "past a second after the first: a kill again");

            // A mixed horde: a kill, a headshot kill or a punch kill every 0.1 s for 8 s.
            r = Fresh();
            var starts = new System.Collections.Generic.List<double>();
            for (int i = 0; i < 80; i++)
            {
                HitEvent h = i % 3 == 0 ? Hit("revolver", killed: true, shot: 10 + i) : i % 3 == 1 ? Hit("revolver", head: true, killed: true, shot: 10 + i) : Hit("punch", killed: true, melee: true, shot: 10 + i);
                if (Step(r, h)) starts.Add(r.Clock);
                Run(r, 0.1);
            }
            Assert.That(starts.Count, Is.GreaterThan(8));
            Assert.That(Rhythm(starts).most, Is.LessThanOrEqualTo(2), "no second with three freezes, bypasses included");
        }

        [Test]
        public void AnyGap_DoesNotHoldBackAParry()
        {
            ImpactRules r = Fresh();
            Assert.That(Step(r, Hit("revolver", killed: true)), Is.True);
            Run(r, 0.1);
            r.OnParry(new HitEvent { owner = "player", weapon = "punch" });
            Assert.That(r.EndStep(false), Is.True, "a parry is exempt: the shared gap doesn't skip it");
            Assert.That(r.granted, Is.EqualTo(ImpactTier.Parry));
        }

        [Test]
        public void HeadshotAndHeavyHit_OnlyTheShotsFirstHit()
        {
            ImpactRules r = Fresh();
            Assert.That(Step(r, Hit("rail", shot: 5), Hit("rail", head: true, shot: 5)), Is.True);
            Assert.That(r.granted, Is.EqualTo(ImpactTier.HeavyHit), "the rail's first hit (a body) earns the tier; its second (a head) doesn't");
            r = Fresh();
            Step(r, Hit("revolver", head: true, shot: 9));
            Run(r, 1);
            Assert.That(Step(r, Hit("revolver", head: true, shot: 9)), Is.False, "a later hit of the same shot: no headshot freeze");
            Assert.That(Step(r, Hit("revolver", head: true, killed: true, shot: 9)), Is.True, "a kill keeps its tier whatever hit it is");
            Assert.That(r.granted, Is.EqualTo(ImpactTier.HeadshotKill));
        }

        [Test]
        public void ParryChain_TheCooldownGivesARepeatTheWebsShortParry_TheFrozenShareStaysLow()
        {
            ImpactRules r = Fresh();
            double cd = F("cooldown.parry"), webParry = cfg.Num("punch.parryHitstop");
            Assert.That(cd, Is.GreaterThan(F("freeze.perfectParry") + 0.35), "a chain at the punch's 0.35 s cycle lands inside the cooldown");
            r.OnParry(new HitEvent { owner = "player", weapon = "punch" });
            r.OnSimHitstop("parry");
            r.EndStep(false);
            Assert.That((r.granted, r.grantedSeconds), Is.EqualTo((ImpactTier.Parry, F("freeze.parry"))));
            Run(r, r.grantedSeconds + 0.35);
            r.OnParry(new HitEvent { owner = "player", weapon = "punch" });
            r.OnSimHitstop("parry");
            Assert.That(r.EndStep(false), Is.True);
            Assert.That((r.granted, r.grantedSeconds), Is.EqualTo((ImpactTier.ParryRepeat, webParry)), "a repeat inside the cooldown: the web's punch.parryHitstop");

            // A chain: a (perfect) parry every punch cycle after its freeze, for 20 s.
            r = Fresh();
            double t0 = r.Clock, frozen = 0;
            while (r.Clock - t0 < 20)
            {
                r.OnParry(new HitEvent { owner = "player", weapon = "punch", perfect = true });
                if (r.EndStep(false)) frozen += r.grantedSeconds;
                Run(r, r.FreezeLeft + 0.35);
            }
            Assert.That(frozen / (r.Clock - t0), Is.LessThan(0.3), "a parry chain freezes well under the reviewer's 46 % of real time");
        }

        [Test]
        public void Settings_AutomationPlaysTheDesignDefaults_APlayerTheirOwn()
        {
            ImpactRules r = Fresh();
            r.ApplySettings(true, FeelKeys.HitFreezeOff, false, true);
            Assert.That((r.hitFreeze, r.slowMoOn), Is.EqualTo((ImpactRules.ParseHitFreeze(cfg.Str("design.impact.settings.hitFreezeDefault")), cfg.Bool("design.impact.settings.slowMoDefault"))),
                "automation (noLab, not the attract): design.impact.settings' defaults, REDUCED MOTION ignored");
            r.ApplySettings(false, FeelKeys.HitFreezeOff, true, false);
            Assert.That((r.hitFreeze, r.slowMoOn), Is.EqualTo((HitFreezeMode.Off, true)), "a player: HIT FREEZE as saved");
            r.ApplySettings(false, FeelKeys.HitFreezeLight, true, true);
            Assert.That((r.hitFreeze, r.slowMoOn), Is.EqualTo((HitFreezeMode.Light, false)), "a player's REDUCED MOTION turns the slow-mo off");
        }

        [Test]
        public void Halt_TheWayToTitle_EndsTheFreezeAndTheSlowMo()
        {
            ImpactRules r = Fresh();
            Step(r, Hit("revolver", head: true, killed: true), "roundClear");
            Assert.That(r.FreezeLeft, Is.GreaterThan(0));
            Assert.That(r.SlowMoActive, Is.True);
            r.Halt();
            Assert.That(r.FreezeLeft, Is.EqualTo(0));
            Assert.That(r.SlowMoActive, Is.False);
            Assert.That(r.BeginFrame(Frame, false), Is.EqualTo(1), "the next frame runs at real time");
        }

        [Test]
        public void Budget_ARollingSecond_TheRequestOverItIsCutOrSkipped_ParriesExempt()
        {
            ImpactRules r = Fresh();
            double perSecond = F("budget.perSecond"), cd = F("cooldown.kill");
            int granted = 0, skippedOrCut = 0;
            double t0 = r.Clock;
            for (int i = 0; i < 12; i++) // a kill every cooldown for over a second
            {
                bool froze = Step(r, Hit("revolver", killed: true));
                if (froze) granted++;
                if (!froze || r.cut) skippedOrCut++;
                Run(r, cd + Frame);
            }
            Assert.That(r.FrozenBetween(t0, t0 + 0.5), Is.LessThanOrEqualTo(perSecond + Eps));
            for (double a = t0; a < r.Clock - 1; a += Frame)
                Assert.That(r.FrozenBetween(a, a + 1), Is.LessThanOrEqualTo(perSecond + Eps), $"the rolling second from {a - t0:0.000}");
            Assert.That(skippedOrCut, Is.GreaterThan(0), "a kill every 0.1 s asks for more than the budget");
            Assert.That(granted, Is.GreaterThan(0));

            // The budget is spent: a parry still freezes in full (design.impact.budget.parryExempt).
            Assert.That(cfg.Bool("design.impact.budget.parryExempt"), Is.True);
            Step(r, Hit("revolver", killed: true));
            r.OnParry(new HitEvent { owner = "player", weapon = "punch" });
            Assert.That(r.EndStep(false), Is.True);
            Assert.That((r.granted, r.grantedSeconds), Is.EqualTo((ImpactTier.Parry, F("freeze.parry"))));
        }

        [Test]
        public void Budget_ARollingMinute_AHordeNeverPassesItsShare()
        {
            ImpactRules r = Fresh();
            double perMinute = F("budget.perMinute") * 60, cd = F("cooldown.kill");
            double t0 = r.Clock;
            int steps = 0;
            while (r.Clock - t0 < 150)
            {
                // A horde: a kill and a headshot kill on alternating steps, as often as the cooldowns let them through.
                Step(r, steps % 2 == 0 ? Hit("revolver", killed: true) : Hit("revolver", head: true, killed: true));
                steps++;
                Run(r, cd / 2);
            }
            double worst = 0;
            for (double a = t0; a + 60 <= r.Clock; a += 0.25) worst = System.Math.Max(worst, r.FrozenBetween(a, a + 60));
            Assert.That(worst, Is.LessThanOrEqualTo(perMinute + Eps), "design.impact.budget.perMinute of any rolling minute");
            Assert.That(worst, Is.GreaterThan(perMinute * 0.5), "the horde did freeze (the budget, not silence)");
        }

        [Test]
        public void HitFreeze_OffFreezesNothing_LightScalesEveryFreeze()
        {
            ImpactRules r = Fresh();
            r.hitFreeze = HitFreezeMode.Off;
            Assert.That(Step(r, Hit("revolver", head: true, killed: true), "kill"), Is.False, "OFF: no freeze, the sim's own request included");
            r.OnParry(new HitEvent { owner = "player", weapon = "punch", perfect = true });
            Assert.That(r.EndStep(false), Is.False, "OFF: not even a parry");
            Assert.That(r.FreezeLeft, Is.EqualTo(0));

            r = Fresh();
            r.hitFreeze = HitFreezeMode.Light;
            Assert.That(Step(r, Hit("revolver", killed: true)), Is.True);
            Assert.That(r.grantedSeconds, Is.EqualTo(F("freeze.kill") * F("settings.lightScale")).Within(Eps), "LIGHT: x design.impact.settings.lightScale");

            Assert.That(ImpactRules.ParseHitFreeze(FeelKeys.HitFreezeOff), Is.EqualTo(HitFreezeMode.Off));
            Assert.That(ImpactRules.ParseHitFreeze(FeelKeys.HitFreezeLight), Is.EqualTo(HitFreezeMode.Light));
            Assert.That(ImpactRules.ParseHitFreeze(FeelKeys.HitFreezeFull), Is.EqualTo(HitFreezeMode.Full));
            Assert.That(ImpactRules.ParseHitFreeze(null), Is.EqualTo(HitFreezeMode.Full), "unset: the design default (full)");
        }

        [Test]
        public void TheFreezeIsExact_ItsLastFrameRunsTheRemainder()
        {
            ImpactRules r = Fresh();
            Step(r, Hit("revolver", head: true, killed: true));
            double frozen = 0, dt = 1.0 / 144; // a 144 Hz display: the freeze is not a whole number of frames
            for (int i = 0; i < 40; i++)
            {
                double share = r.BeginFrame(dt, false);
                frozen += (1 - share) * dt;
            }
            Assert.That(frozen, Is.EqualTo(F("freeze.headshotKill")).Within(1e-9), "frozen real time == the freeze granted");
        }

        [Test]
        public void SlowMo_TheRoundsLastKill_HoldsThenEasesBack()
        {
            ImpactRules r = Fresh();
            double scale = F("slowMo.lastKill.scale"), hold = F("slowMo.lastKill.hold"), ease = F("slowMo.lastKill.ease");
            Step(r, Hit("revolver", killed: true), "roundClear");
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.LastKill));
            Assert.That((r.slowMoScale, r.slowMoHold), Is.EqualTo((scale, hold)));
            Run(r, r.grantedSeconds); // the kill's freeze first
            r.BeginFrame(Frame, false);
            Assert.That(r.Scale, Is.EqualTo(scale).Within(1e-9), "held at design.impact.slowMo.lastKill.scale");
            Run(r, hold - 2 * Frame);
            Assert.That(r.Scale, Is.EqualTo(scale).Within(1e-9), "still held");
            Run(r, ease / 2);
            Assert.That(r.Scale, Is.GreaterThan(scale).And.LessThan(1), "easing back");
            Run(r, ease);
            Assert.That(r.Scale, Is.EqualTo(1), "back to real time");
            Assert.That(r.SlowMoActive, Is.False);

            ImpactRules none = Fresh();
            Step(none, "roundClear");
            Assert.That(none.slowMoStarted, Is.EqualTo(SlowMoCause.None), "a round cleared without a kill of yours in that step: none");
        }

        [Test]
        public void SlowMo_AMultiKillFromOneShot()
        {
            ImpactRules r = Fresh();
            int n = (int)F("slowMo.multiKill.count");
            var moments = new object[n];
            for (int i = 0; i < n; i++) moments[i] = Hit("rocket", killed: true, shot: 7, distance: 3);
            Step(r, moments);
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.MultiKill));
            Assert.That(r.slowMoScale, Is.EqualTo(F("slowMo.multiKill.scale")));

            ImpactRules two = Fresh();
            var spread = new object[n];
            for (int i = 0; i < n; i++) spread[i] = Hit("revolver", killed: true, shot: 10 + i); // one kill per shot
            Step(two, spread);
            Assert.That(two.slowMoStarted, Is.EqualTo(SlowMoCause.None), "kills from different shots: no multi-kill");
        }

        [Test]
        public void SlowMo_NeverStacked_TheStrongestWins_AtMostOneInAnyMinGap()
        {
            ImpactRules r = Fresh();
            int n = (int)F("slowMo.multiKill.count");
            var moments = new object[n + 1];
            for (int i = 0; i < n; i++) moments[i] = Hit("rocket", killed: true, shot: 3, distance: 3);
            moments[n] = "roundClear";
            Step(r, moments);
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.LastKill), "last kill (0.3) beats multi-kill (0.5) in one step: the strongest");
            Assert.That(r.slowMos[(int)SlowMoCause.LastKill] + r.slowMos[(int)SlowMoCause.MultiKill], Is.EqualTo(1));

            double gap = F("slowMo.minGap");
            Run(r, 1.5);
            r.OnParry(new HitEvent { owner = "player", weapon = "punch", perfect = true });
            r.EndStep(false);
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.None), "inside design.impact.slowMo.minGap: no second slow-mo");
            Assert.That(r.slowMoSkipped, Is.True, "the skip is logged");
            Assert.That(r.granted, Is.EqualTo(ImpactTier.PerfectParry), "the parry's freeze still lands");
            Run(r, gap);
            r.OnParry(new HitEvent { owner = "player", weapon = "punch", perfect = true });
            r.EndStep(false);
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.PerfectParry), "after the gap: a slow-mo again");
        }

        [Test]
        public void SlowMo_NoneWhileDown_AndDownEndsOne_AndOffMeansOff()
        {
            ImpactRules r = Fresh();
            r.OnKill(Kill());
            r.OnRoundClear();
            r.EndStep(true); // DOWN (or paused, or a menu): the loop says so
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.None), "none while DOWN");

            r = Fresh();
            Step(r, Hit("revolver", killed: true), "roundClear");
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.LastKill));
            Run(r, r.grantedSeconds + Frame);
            Assert.That(r.Scale, Is.LessThan(1));
            r.BeginFrame(Frame, true); // DOWN arrives mid slow-mo
            Assert.That(r.Scale, Is.EqualTo(1), "DOWN ends the slow-mo at once");
            Assert.That(r.SlowMoActive, Is.False);

            r = Fresh();
            r.slowMoOn = false; // SLOW-MO off, or REDUCED MOTION on
            Step(r, Hit("revolver", killed: true), "roundClear");
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.None), "SLOW-MO off: none");
            Assert.That(r.granted, Is.EqualTo(ImpactTier.Kill), "the freeze stays");
        }

        [Test]
        public void TheSimsOwnRequests_MapToTiers_AnUnknownCauseKeepsItsLength()
        {
            ImpactRules r = Fresh();
            Assert.That(Step(r, "kill"), Is.True);
            // NIGHT17 round 3: the kill tier (design.impact.freeze.kill, 0.05) now outlasts the web's feel.hitstopTime (0.04);
            // the longest request wins, so the grant is the longer of the two (the two were equal until tonight).
            Assert.That((r.granted, r.grantedSeconds), Is.EqualTo((ImpactTier.Kill, System.Math.Max(cfg.Num("feel.hitstopTime"), F("freeze.kill")))), "the web's kill hitstop, through the budget, the longest request winning");
            r = Fresh();
            r.OnSimHitstop("someday");
            Assert.That(r.EndStep(false, 0.07), Is.True);
            Assert.That((r.granted, r.grantedSeconds), Is.EqualTo((ImpactTier.Sim, 0.07)));
        }

        [Test]
        public void NewRun_ForgetsTheCooldownsTheBudgetAndTheSlowMo()
        {
            ImpactRules r = Fresh();
            Step(r, Hit("revolver", killed: true), "roundClear");
            r.NewRun();
            Assert.That(r.FreezeLeft, Is.EqualTo(0));
            Assert.That(r.SlowMoActive, Is.False);
            Assert.That(Step(r, Hit("revolver", killed: true), "roundClear"), Is.True, "no cooldown carried over");
            Assert.That(r.slowMoStarted, Is.EqualTo(SlowMoCause.LastKill), "no slow-mo gap carried over");
        }
    }
}
