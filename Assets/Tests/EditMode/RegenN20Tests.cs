// NIGHT20 (the shared commit places this empty fixture; E owns it): regen at the round's end (design.regen.n20): the refill to full at roundClear over refillTime; in a round, passive regen only to the top of the current segment after segmentDelay at segmentRate; the earned heals (a close kill, a punch kill) ignore the segments; MEND's rate; NO REGEN = no segments, no refill; the roundHeal / earnedHeal events.
// Builder E (U3-34.5, tests first): the Core half (Core/Health.cs's Regen with the block's rules, RegenN20's pure math);
// the sim's wiring (roundClear -> the refill, the player's kills -> the earned heals) is Game/RegenN20SimTests.cs. The
// numbers are design.regen.n20's (Design/unity-config.json beside PortExport/), MEND's the web's perks.mend; the block's
// rules are handed to Regen with UseN20 (the run does it from its blocks), so these tests run whatever `enabled` says.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class RegenN20Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        static RegenN20.Params P => RegenN20.Params.From(Cfg);
        /// <summary>The SEGMENTS rule (passive regen to the segment's top), the switch's other choice and MEND's.</summary>
        static RegenN20.Params Seg
        {
            get
            {
                RegenN20.Params p = P;
                p.passive = true;
                return p;
            }
        }
        static double Dt => Cfg.Dt;

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Health health;
            public readonly Regen regen;
            public double speed;
            public readonly List<RoundHealEvent> roundHeals = new List<RoundHealEvent>();
            public readonly List<RoundHealEvent> earned = new List<RoundHealEvent>();

            public Rig(bool n20 = true, double max = 100, bool passive = true)
            {
                health = new Health(max, events, "player");
                regen = new Regen(Cfg, events, health, () => speed);
                regen.SetEnabled(true);
                if (n20) regen.UseN20(true, passive ? Seg : P);
                events.On<RoundHealEvent>(N20Events.RoundHeal, e => roundHeals.Add(new RoundHealEvent { kind = e.kind, amount = e.amount, seconds = e.seconds, round = e.round }));
                events.On<RoundHealEvent>(N20Events.EarnedHeal, e => earned.Add(new RoundHealEvent { kind = e.kind, amount = e.amount, seconds = e.seconds, round = e.round }));
            }

            /// <summary>Step the regen for `seconds` (whole steps).</summary>
            public void Run(double seconds)
            {
                int n = MovementMath.StepsFor(seconds, Dt);
                for (int i = 0; i < n; i++) regen.Step(Dt);
            }

            public void Hit(double amount) => health.Damage(amount, "husk");
        }

        [Test]
        public void TheBlocksNumbers_AreTheSwitchsDefaults()
        {
            RegenN20.Params p = P;
            Assert.That(p.refillTime, Is.EqualTo(2), "a full refill over 2 s at roundClear");
            Assert.That(p.segment, Is.EqualTo(25), "25 HP segments (4 at 100 HP)");
            Assert.That(p.delay, Is.EqualTo(5), "5 s after the last hit");
            Assert.That(p.rate, Is.EqualTo(10), "10 HP/s");
            Assert.That(p.passive, Is.False, "STRICT (fix list E1-E3): no passive HP in a round");
            Assert.That(p.closeKillHeal, Is.EqualTo(1), "THE MARATHON: +4 alone returned 0.48 of the damage taken");
            Assert.That(p.closeKillRange, Is.EqualTo(6));
            Assert.That(p.punchKillHeal, Is.EqualTo(10));
        }

        [Test]
        public void TheSegmentTop_IsTheTopOfTheSegmentYoureIn()
        {
            Assert.That(RegenN20.SegmentTop(60, 25, 100), Is.EqualTo(75));
            Assert.That(RegenN20.SegmentTop(75, 25, 100), Is.EqualTo(75), "on a line: that segment is full");
            Assert.That(RegenN20.SegmentTop(50, 25, 100), Is.EqualTo(50), "a hit that lands on a line keeps you there");
            Assert.That(RegenN20.SegmentTop(99, 25, 100), Is.EqualTo(100));
            Assert.That(RegenN20.SegmentTop(1, 25, 100), Is.EqualTo(25));
            Assert.That(RegenN20.SegmentTop(140, 25, 150), Is.EqualTo(150), "PLATE's 150: six segments");
            Assert.That(RegenN20.SegmentTop(80, 25, 90), Is.EqualTo(90), "capped at the cap (hard damage)");
            Assert.That(RegenN20.SegmentTop(0, 25, 100), Is.EqualTo(0), "dead: nothing");
        }

        [Test]
        public void InARound_PassiveRegen_ClimbsOnlyToTheSegmentsTop_AfterTheDelay_AtTheRate()
        {
            var r = new Rig();
            r.Hit(30); // 100 -> 70: the (50, 75] segment
            r.Run(4.9);
            Assert.That(r.health.hp, Is.EqualTo(70), "nothing before segmentDelay");
            Assert.That(r.regen.Regenerating, Is.False);
            r.Run(0.6); // 5.5 s: 0.5 s of regen at 10 HP/s
            Assert.That(r.health.hp, Is.EqualTo(75).Within(0.2), "10 HP/s for ~0.5 s: 70 -> 75 (the segment's top)");
            r.Run(10);
            Assert.That(r.health.hp, Is.EqualTo(75), "never past the segment's top in a round");
            Assert.That(r.regen.Regenerating, Is.False, "stopped at the top");
        }

        [Test]
        public void ARateOf10_IsTen_HpPerSecond()
        {
            var r = new Rig();
            r.Hit(24); // 100 -> 76: the (75, 100] segment, 24 HP to its top
            r.Run(5);
            double at5 = r.health.hp;
            r.Run(1);
            Assert.That(r.health.hp - at5, Is.EqualTo(10).Within(10 * Dt + 1e-9), "one second at segmentRate");
        }

        [Test]
        public void NoSpeedUp_ForMovingFast_OrDealingDamage()
        {
            var r = new Rig { speed = 30 };
            r.Hit(10);
            r.events.Emit("hit", new HitEvent { owner = "player", damage = 5 }); // the web's fastMult window after dealing damage
            r.Run(4.9);
            Assert.That(r.health.hp, Is.EqualTo(90), "the web halves the delay above 8 m/s or after a hit dealt; the segments don't");
        }

        [Test]
        public void AHit_RestartsTheDelay()
        {
            var r = new Rig();
            r.Hit(10); // 90
            r.Run(4);
            r.Hit(5); // 85, 4 s in
            r.Run(4.9);
            Assert.That(r.health.hp, Is.EqualTo(85), "the second hit restarts segmentDelay");
            r.Run(1);
            Assert.That(r.health.hp, Is.GreaterThan(85));
        }

        [Test]
        public void TheRoundEndRefill_FillsToFull_OverRefillTime_AndEmitsRoundHeal()
        {
            var r = new Rig();
            r.Hit(60); // 40: two segments lost
            double amount = r.regen.StartRefill(3);
            Assert.That(amount, Is.EqualTo(60), "the refill restores the missing HP");
            Assert.That(r.roundHeals.Count, Is.EqualTo(1));
            Assert.That(r.roundHeals[0].kind, Is.EqualTo("round"));
            Assert.That(r.roundHeals[0].amount, Is.EqualTo(60));
            Assert.That(r.roundHeals[0].seconds, Is.EqualTo(2));
            Assert.That(r.roundHeals[0].round, Is.EqualTo(3));
            Assert.That(r.regen.RefillLeft, Is.EqualTo(2).Within(Dt), "RefillLeft: the seconds left of the refill");
            r.Run(1);
            Assert.That(r.health.hp, Is.EqualTo(70).Within(1), "linear over refillTime: half way at 1 s");
            Assert.That(r.regen.RefillLeft, Is.EqualTo(1).Within(Dt));
            Assert.That(r.regen.Regenerating, Is.True, "the refill shows as regen (regenStart)");
            r.Run(1);
            Assert.That(r.health.hp, Is.EqualTo(100), "full at refillTime");
            Assert.That(r.regen.RefillLeft, Is.EqualTo(0));
            r.Run(0.1);
            Assert.That(r.regen.Regenerating, Is.False);
        }

        [Test]
        public void TheRefill_AtFullHp_EmitsNothing()
        {
            var r = new Rig();
            Assert.That(r.regen.StartRefill(1), Is.EqualTo(0));
            Assert.That(r.roundHeals, Is.Empty);
            Assert.That(r.regen.RefillLeft, Is.EqualTo(0));
        }

        [Test]
        public void AHitDuringTheRefill_DoesntStopIt()
        {
            var r = new Rig();
            r.Hit(60);
            r.regen.StartRefill(2);
            r.Run(1);
            r.Hit(10);
            r.Run(1);
            Assert.That(r.health.hp, Is.EqualTo(100), "the refill ends full whatever the break brings");
        }

        [Test]
        public void EarnedHeals_IgnoreTheSegments_AndEmitEarnedHeal()
        {
            var r = new Rig();
            r.Hit(30); // 70: the top is 75
            double got = r.regen.Earn(10, RegenN20.PunchKill);
            Assert.That(got, Is.EqualTo(10));
            Assert.That(r.health.hp, Is.EqualTo(80), "past the segment's line");
            Assert.That(r.earned.Count, Is.EqualTo(1));
            Assert.That(r.earned[0].kind, Is.EqualTo("punchKill"));
            Assert.That(r.earned[0].amount, Is.EqualTo(10));
            Assert.That(r.earned[0].seconds, Is.EqualTo(0), "at once");
            r.health.hp = 98;
            Assert.That(r.regen.Earn(4, RegenN20.CloseKill), Is.EqualTo(2), "capped at max");
            Assert.That(r.earned[1].amount, Is.EqualTo(2), "the payload carries what was restored");
            r.health.hp = 100;
            Assert.That(r.regen.Earn(4, RegenN20.CloseKill), Is.EqualTo(0));
            Assert.That(r.earned.Count, Is.EqualTo(2), "nothing restored: no event");
        }

        [Test]
        public void AKillsHeal_PunchTen_CloseFour_FarNothing()
        {
            RegenN20.Params p = P;
            Assert.That(RegenN20.KillHeal(p, "punch", 1.5, out string kind), Is.EqualTo(10));
            Assert.That(kind, Is.EqualTo(RegenN20.PunchKill), "a punch kill is the punch's +10, not +14");
            Assert.That(RegenN20.KillHeal(p, "revolver", 5.9, out kind), Is.EqualTo(p.closeKillHeal));
            Assert.That(kind, Is.EqualTo(RegenN20.CloseKill));
            Assert.That(RegenN20.KillHeal(p, "shotgun", 6, out kind), Is.EqualTo(p.closeKillHeal), "within 6 m, inclusive");
            Assert.That(RegenN20.KillHeal(p, "revolver", 6.01, out kind), Is.EqualTo(0));
            Assert.That(kind, Is.Null);
        }

        [Test]
        public void Mend_ScalesTheSegmentRegen_ToMendDelayAndMendRate()
        {
            var r = new Rig();
            r.regen.perk = new RegenMath.Perk { delay = Cfg.Num("perks.mend.delayMult"), rate = Cfg.Num("perks.mend.rateMult") };
            double mendDelay = Cfg.Num("design.regen.n20.mendDelay"), mendRate = Cfg.Num("design.regen.n20.mendRate");
            Assert.That(P.delay * Cfg.Num("perks.mend.delayMult"), Is.EqualTo(mendDelay).Within(1e-12), "the block's mendDelay is MEND's web multiplier on segmentDelay");
            Assert.That(P.rate * Cfg.Num("perks.mend.rateMult"), Is.EqualTo(mendRate).Within(1e-12), "the block's mendRate is MEND's web multiplier on segmentRate");
            r.Hit(24); // 76: 24 to the top
            r.Run(mendDelay - 0.1);
            Assert.That(r.health.hp, Is.EqualTo(76), "nothing before mendDelay");
            r.Run(0.1);
            double at = r.health.hp;
            r.Run(1);
            Assert.That(r.health.hp - at, Is.EqualTo(mendRate).Within(mendRate * Dt + 1e-9), "mendRate HP/s");
        }

        [Test]
        public void TheStyleRank_KeepsItsBonus()
        {
            var r = new Rig();
            r.regen.rank = () => 5; // x (1 + 0.08 x 5) = 1.4 on the rate, / on the delay (regenMath.js rankRegenPerk)
            double m = 1 + Cfg.Num("regen.rankPerRank") * 5;
            r.Hit(24);
            r.Run(P.delay / m - 0.05);
            Assert.That(r.health.hp, Is.EqualTo(76));
            r.Run(0.1);
            double at = r.health.hp;
            r.Run(1);
            Assert.That(r.health.hp - at, Is.EqualTo(P.rate * m).Within(P.rate * m * Dt + 1e-9));
        }

        [Test]
        public void Camping_PausesTheSegmentRegen()
        {
            var r = new Rig();
            r.regen.camping = () => true; // EXPOSED (regen.js campingSource): no regen
            r.Hit(10);
            r.Run(10);
            Assert.That(r.health.hp, Is.EqualTo(90));
        }

        [Test]
        public void NoRegen_NoSegments_NoRefill_TheEarnedHealsStay()
        {
            var r = new Rig();
            r.regen.SetEnabled(false); // HEAT's NO REGEN (mutators.js)
            r.Hit(10);
            r.Run(10);
            Assert.That(r.health.hp, Is.EqualTo(90), "no segment regen");
            Assert.That(r.regen.StartRefill(4), Is.EqualTo(0), "no refill");
            Assert.That(r.roundHeals, Is.Empty);
            r.Run(3);
            Assert.That(r.health.hp, Is.EqualTo(90));
            Assert.That(r.regen.Earn(10, RegenN20.PunchKill), Is.EqualTo(10), "earned heals only");
            Assert.That(r.health.hp, Is.EqualTo(100));
            Assert.That(r.regen.SegmentTop, Is.EqualTo(r.health.hp), "no segments: passive regen climbs nowhere");
        }

        [Test]
        public void Strict_NoPassiveHpInARound_TheRefillAndTheEarnedHealsStay()
        {
            var r = new Rig(passive: false);
            r.Hit(30); // 70
            r.Run(20);
            Assert.That(r.health.hp, Is.EqualTo(70), "STRICT: nothing comes back in a round");
            Assert.That(r.regen.SegmentHp, Is.EqualTo(0), "no segments to draw");
            Assert.That(r.regen.SegmentTop, Is.EqualTo(r.health.hp), "passive regen climbs nowhere");
            Assert.That(r.regen.Earn(10, RegenN20.PunchKill), Is.EqualTo(10), "the earned heals stay");
            Assert.That(r.regen.StartRefill(1), Is.EqualTo(20), "the round-end refill stays");
            r.Run(2.1);
            Assert.That(r.health.hp, Is.EqualTo(100));
        }

        [Test]
        public void Strict_Mend_BuysBackTheSegments()
        {
            var r = new Rig(passive: false);
            r.regen.perk = new RegenMath.Perk { delay = Cfg.Num("perks.mend.delayMult"), rate = Cfg.Num("perks.mend.rateMult") };
            Assert.That(r.regen.SegmentHp, Is.EqualTo(25), "MEND shows the segments");
            r.Hit(24); // 76: 24 to the top
            r.Run(Cfg.Num("design.regen.n20.mendDelay") + 1);
            Assert.That(r.health.hp, Is.EqualTo(76 + Cfg.Num("design.regen.n20.mendRate")).Within(Cfg.Num("design.regen.n20.mendRate") * Dt + 1e-9), "MEND: mendRate after mendDelay");
            r.Run(10);
            Assert.That(r.health.hp, Is.EqualTo(100), "to the segment's top");
        }

        [Test]
        public void TheHudContract()
        {
            var r = new Rig();
            Assert.That(r.regen.N20, Is.True);
            Assert.That(r.regen.SegmentHp, Is.EqualTo(25));
            r.Hit(40); // 60
            Assert.That(r.regen.SegmentTop, Is.EqualTo(75), "the HP passive regen climbs to now");
            Assert.That(r.regen.RefillLeft, Is.EqualTo(0), "0 when no refill runs");
            var off = new Rig(false);
            Assert.That(off.regen.N20, Is.False);
            Assert.That(off.regen.SegmentHp, Is.EqualTo(0), "no segments with the block off");
            off.Hit(40);
            Assert.That(off.regen.SegmentTop, Is.EqualTo(100), "the web's regen climbs to full");
        }

        [Test]
        public void TheBlockOff_IsTheWebsRegen()
        {
            var r = new Rig(false);
            r.Hit(50); // 50
            r.Run(3.9);
            Assert.That(r.health.hp, Is.EqualTo(50), "the web's 4 s delay");
            r.Run(3);
            Assert.That(r.health.hp, Is.EqualTo(100), "the web's 25 HP/s to full, past every line");
            r.Hit(50);
            Assert.That(r.regen.StartRefill(1), Is.EqualTo(0), "no refill");
            Assert.That(r.regen.Earn(10, RegenN20.PunchKill), Is.EqualTo(0), "no earned heals");
            Assert.That(r.roundHeals, Is.Empty);
            Assert.That(r.earned, Is.Empty);
            Assert.That(r.health.hp, Is.EqualTo(50));
        }

        [Test]
        public void UseN20Off_AfterOn_IsTheWebsRegenAgain()
        {
            var r = new Rig();
            r.regen.UseN20(false, P);
            Assert.That(r.regen.N20, Is.False);
            r.Hit(50);
            r.Run(7);
            Assert.That(r.health.hp, Is.EqualTo(100));
        }

        [Test]
        public void TheRunSnapshot_KeepsTheRefillAndTheDelay()
        {
            var r = new Rig();
            r.Hit(60);
            r.regen.StartRefill(2);
            r.Run(0.5);
            var w = new StateWriter();
            r.regen.Save(w);
            var r2 = new Rig();
            r2.health.hp = r.health.hp;
            r2.regen.Load(new StateReader(w.ToJson()));
            Assert.That(r2.regen.RefillLeft, Is.EqualTo(r.regen.RefillLeft));
            r.Run(1.5);
            r2.Run(1.5);
            Assert.That(r2.health.hp, Is.EqualTo(r.health.hp));
        }
    }
}
