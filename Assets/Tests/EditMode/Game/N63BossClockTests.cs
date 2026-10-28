// NIGHT63 builder V (slate B1's first deliverable; Logs/n63/slate.json B1 doneWhen 1): BossAttackClock63, the read-only view of a campaign boss's
// attack state, and the boss63 reel's registration and proof lines (Game/BossAttackClock63.cs, Game/FeelReel.N63V.cs). Pinned:
//   1. THE BRAIN (THE WICK, THE LOOM, THE CROWN run Boss37Brain): REVEAL -> Reveal, REST -> Idle, TELL -> Windup with stepsToHit = the steps until
//      the strike begins (the brain's TELL -> ACTIVE step, counted on a bare brain), ACTIVE -> Strike (0), RECOVER -> Recover, WINDOW -> Stagger
//      ("window"), LEGCHANGE -> Change, DYING -> Dying, OFF / DEAD -> None; the clock never steps or writes the brain (its state after the reads is
//      the state a twin brain stepped the same way has)
//   2. THE ICEBREAKER (Brawl40's own states): TELL -> Windup (stateLeft to the strike), ACTIVE -> Strike, STUCK -> Stagger ("stuck"; "dazed" after a
//      rush), WINDOW -> Stagger ("window"), IDLE -> Idle, PHASECHANGE -> Change, OFF / DEAD -> None
//   3. THE KILN WYRM (its attack log): a tell at step s of `seconds` hits at s + StepsFor(seconds): Windup before it (the next hit first), Strike
//      for UnityTuning.Boss63WyrmStrikeS after it, then Idle; "intro" -> Reveal, "beat" -> Change, "dying" -> Dying, staggered -> Stagger
//   4. the proof lines' text (the reel greps "[boss63] attack", "[boss63] flinch", "[boss63] stagger", "[boss63] rms")
//   5. boss63 is registered beside boss61 (FeelReel.PhaseN41T): no new line in FeelReel.N27.cs
// Written test first (NIGHT63.md section 4): RED on its commit (the clock's placeholders answer None and empty lines); the Editor run is the lead's.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;
using C = Skygrind.Game.BossAttackClock63;

namespace Skygrind.Game.Tests
{
    public class N63BossClockTests
    {
        const double Dt = 1.0 / 120;

        static Boss37Brain Brain()
        {
            var b = new Boss37Brain(new Boss37Params(), "wick", Dt, null, 7u);
            b.attacks.Add(Boss37Attack.Of("slam", Tell36Kind.Red, 0.5, 0.1, 0.2, Dt));
            return b;
        }

        static Boss37Context Ctx() => new Boss37Context { player = new Vec3(0, 0, 5), boss = Vec3.Zero, grounded = true, lineOfSight = true, hpFrac = 1 };

        [Test]
        public void Brain_TellCountsTheStepsToTheStrike()
        {
            Boss37Brain b = Brain(), twin = Brain();
            Boss37Context ctx = Ctx();
            Assert.That(C.FromBrain(C.Wick, b).phase, Is.EqualTo(BossPhase63.None), "OFF: no fight");
            b.Begin(10, ctx.player);
            twin.Begin(10, ctx.player);
            Assert.That(C.FromBrain(C.Wick, b).phase, Is.EqualTo(BossPhase63.Reveal), "REVEAL");
            int guard = 0;
            bool sawIdle = false;
            while (b.state != Boss37State.Tell && guard++ < 2000)
            {
                b.Step(ctx);
                twin.Step(ctx);
                if (b.state == Boss37State.Rest) { sawIdle = true; Assert.That(C.FromBrain(C.Wick, b).phase, Is.EqualTo(BossPhase63.Idle), "REST"); }
            }
            Assert.That(sawIdle, Is.True, "the brain rested before its first attack");
            BossAttack63 a = C.FromBrain(C.Wick, b);
            Assert.That(a.phase, Is.EqualTo(BossPhase63.Windup), "TELL");
            Assert.That(a.attack, Is.EqualTo("slam"));
            Assert.That(a.boss, Is.EqualTo(C.Wick));
            Assert.That(a.windupSteps, Is.EqualTo(MovementMath.StepsFor(0.5, Dt)), "the wind-up's length");
            Assert.That(a.stepsToHit, Is.EqualTo(a.windupSteps), "at the tell's first step the whole wind-up is ahead");
            Assert.That(a.Windup01, Is.EqualTo(0).Within(1e-9));
            int predicted = a.stepsToHit, steps = 0;
            while (b.state == Boss37State.Tell && steps < 2000)
            {
                b.Step(ctx);
                twin.Step(ctx);
                steps++;
                BossAttack63 r = C.FromBrain(C.Wick, b);
                if (b.state == Boss37State.Tell) Assert.That(r.stepsToHit, Is.EqualTo(predicted - steps), "the count runs down one a step");
            }
            Assert.That(steps, Is.EqualTo(predicted), "the strike begins exactly stepsToHit steps after the read");
            BossAttack63 s = C.FromBrain(C.Wick, b);
            Assert.That(s.phase, Is.EqualTo(BossPhase63.Strike), "ACTIVE");
            Assert.That(s.stepsToHit, Is.EqualTo(0));
            Assert.That(s.attack, Is.EqualTo("slam"));
            while (b.state == Boss37State.Active) { b.Step(ctx); twin.Step(ctx); }
            Assert.That(C.FromBrain(C.Wick, b).phase, Is.EqualTo(BossPhase63.Recover), "RECOVER");
            Assert.That(b.state, Is.EqualTo(twin.state), "read only: the read brain and its unread twin agree");
            Assert.That(b.stateLeft, Is.EqualTo(twin.stateLeft));
            Assert.That(b.stagger, Is.EqualTo(twin.stagger));
            b.Stagger(b.stagger + 1);
            BossAttack63 w = C.FromBrain(C.Wick, b);
            Assert.That(w.phase, Is.EqualTo(BossPhase63.Stagger), "WINDOW");
            Assert.That(w.why, Is.EqualTo("window"));
            Assert.That(w.stepsToHit, Is.EqualTo(-1), "no hit coming in the window");
            b.NextLeg();
            Assert.That(C.FromBrain(C.Wick, b).phase, Is.EqualTo(BossPhase63.Stagger).Or.EqualTo(BossPhase63.Change), "a leg change from the window");
            b.Die(5);
            Assert.That(C.FromBrain(C.Wick, b).phase, Is.EqualTo(BossPhase63.Dying), "DYING");
            for (int i = 0; i < 10; i++) b.Step(ctx);
            Assert.That(C.FromBrain(C.Wick, b).phase, Is.EqualTo(BossPhase63.None), "DEAD");
        }

        [Test]
        public void Brain_LegChange()
        {
            Boss37Brain b = Brain();
            Boss37Context ctx = Ctx();
            b.Begin(2, ctx.player);
            for (int i = 0; i < 5; i++) b.Step(ctx);
            b.NextLeg();
            Assert.That(b.state, Is.EqualTo(Boss37State.LegChange));
            Assert.That(C.FromBrain(C.Loom, b).phase, Is.EqualTo(BossPhase63.Change), "LEGCHANGE");
        }

        [Test]
        public void Brawl_EveryState()
        {
            BossAttack63 t = C.FromBrawl(Brawl40State.Tell, Brawl40.Haymaker, 30, 108, 78);
            Assert.That(t.boss, Is.EqualTo(C.Brawl));
            Assert.That(t.phase, Is.EqualTo(BossPhase63.Windup));
            Assert.That(t.attack, Is.EqualTo(Brawl40.Haymaker));
            Assert.That(t.stepsToHit, Is.EqualTo(30), "the tell's steps left are the steps to the swing (Brawl40.Step: --stateLeft, then Active)");
            Assert.That(t.windupSteps, Is.EqualTo(108));
            Assert.That(t.Windup01, Is.EqualTo(78 / 108.0).Within(1e-9));
            BossAttack63 a = C.FromBrawl(Brawl40State.Active, Brawl40.Slam, 10, 12, 2);
            Assert.That(a.phase, Is.EqualTo(BossPhase63.Strike));
            Assert.That(a.stepsToHit, Is.EqualTo(0));
            BossAttack63 st = C.FromBrawl(Brawl40State.Stuck, Brawl40.Haymaker, 100, 200, 100);
            Assert.That(st.phase, Is.EqualTo(BossPhase63.Stagger));
            Assert.That(st.why, Is.EqualTo("stuck"));
            Assert.That(C.FromBrawl(Brawl40State.Stuck, Brawl40.Rush, 100, 200, 100).why, Is.EqualTo("dazed"));
            BossAttack63 w = C.FromBrawl(Brawl40State.Window, null, 100, 300, 200);
            Assert.That(w.phase, Is.EqualTo(BossPhase63.Stagger));
            Assert.That(w.why, Is.EqualTo("window"));
            Assert.That(C.FromBrawl(Brawl40State.Idle, null, 10, 100, 90).phase, Is.EqualTo(BossPhase63.Idle));
            Assert.That(C.FromBrawl(Brawl40State.Reveal, null, 10, 100, 90).phase, Is.EqualTo(BossPhase63.Reveal));
            Assert.That(C.FromBrawl(Brawl40State.Recover, Brawl40.Slam, 10, 100, 90).phase, Is.EqualTo(BossPhase63.Recover));
            Assert.That(C.FromBrawl(Brawl40State.PhaseChange, null, 10, 100, 90).phase, Is.EqualTo(BossPhase63.Change));
            Assert.That(C.FromBrawl(Brawl40State.Dying, null, 10, 100, 90).phase, Is.EqualTo(BossPhase63.Dying));
            Assert.That(C.FromBrawl(Brawl40State.Dead, null, 0, 0, 0).phase, Is.EqualTo(BossPhase63.None));
            Assert.That(C.FromBrawl(Brawl40State.Off, null, 0, 0, 0).phase, Is.EqualTo(BossPhase63.None));
        }

        [Test]
        public void Wyrm_TheLogsTellsCountToTheirHits()
        {
            var log = new List<WyrmLog>
            {
                new WyrmLog { step = 40, name = "spit", seconds = 0.6, resolve = false },
                new WyrmLog { step = 112, name = "spit", seconds = 0, resolve = true },
                new WyrmLog { step = 100, name = "lunge", seconds = 0.8, resolve = false },
            };
            int hit = 100 + MovementMath.StepsFor(0.8, Dt);
            BossAttack63 a = C.FromWyrm(log, 150, "p2", false, Dt);
            Assert.That(a.boss, Is.EqualTo(C.Wyrm));
            Assert.That(a.phase, Is.EqualTo(BossPhase63.Windup), "a tell whose hit is ahead");
            Assert.That(a.attack, Is.EqualTo("lunge"));
            Assert.That(a.stepsToHit, Is.EqualTo(hit - 150));
            Assert.That(a.windupSteps, Is.EqualTo(MovementMath.StepsFor(0.8, Dt)));
            BossAttack63 s = C.FromWyrm(log, hit, "p2", false, Dt);
            Assert.That(s.phase, Is.EqualTo(BossPhase63.Strike), "the hit's step");
            Assert.That(s.attack, Is.EqualTo("lunge"));
            Assert.That(s.stepsToHit, Is.EqualTo(0));
            int strike = MovementMath.StepsFor(UnityTuning.Boss63WyrmStrikeS, Dt);
            Assert.That(C.FromWyrm(log, hit + strike - 1, "p2", false, Dt).phase, Is.EqualTo(BossPhase63.Strike), "the strike's last step");
            Assert.That(C.FromWyrm(log, hit + strike, "p2", false, Dt).phase, Is.EqualTo(BossPhase63.Idle), "after the strike");
            log.Add(new WyrmLog { step = 300, name = "roar", seconds = 1.0, resolve = false });
            log.Add(new WyrmLog { step = 310, name = "aggro", seconds = 0.5, resolve = false });
            BossAttack63 next = C.FromWyrm(log, 320, "p2", false, Dt);
            Assert.That(next.attack, Is.EqualTo("aggro"), "two tells ahead: the nearer hit first");
            Assert.That(next.stepsToHit, Is.EqualTo(310 + MovementMath.StepsFor(0.5, Dt) - 320));
            Assert.That(C.FromWyrm(log, 320, "intro", false, Dt).phase, Is.EqualTo(BossPhase63.Reveal), "intro");
            Assert.That(C.FromWyrm(log, 320, "beat", false, Dt).phase, Is.EqualTo(BossPhase63.Change), "a phase beat");
            Assert.That(C.FromWyrm(log, 320, "dying", false, Dt).phase, Is.EqualTo(BossPhase63.Dying), "dying");
            Assert.That(C.FromWyrm(log, 320, "dead", false, Dt).phase, Is.EqualTo(BossPhase63.None), "dead");
            // NIGHT63 fix round (b2's b63k RED): this case read Stagger at step 320 with two tells pending, the rule that hid the spit's wind-up;
            // a pending tell now outranks the stagger (Wyrm_ATellPendingInAStaggerShowsItsWindup), so the stagger is read where no tell is pending
            BossAttack63 g = C.FromWyrm(log, 600, "p2", true, Dt);
            Assert.That(g.phase, Is.EqualTo(BossPhase63.Stagger), "staggered (a head punch on a charge), no tell pending");
            Assert.That(g.why, Is.EqualTo("staggered"));
            Assert.That(C.FromWyrm(new List<WyrmLog>(), 5, "p1", false, Dt).phase, Is.EqualTo(BossPhase63.Idle), "no tell: idle");
        }

        /// <summary>NIGHT63 fix round (n63-b2's b63k: "attack spit windup 23592 hit 24492 lead 900 pose 192"): the wyrm was staggered 18000-24267 ms
        /// and the sim told the spit inside it; the clock read Stagger until the stagger ended, 225 ms before the hit. Pinned: a tell pending
        /// outranks the stagger (the worm attacks while staggered; the other four bosses never tell in a window or stuck), and a tell pending
        /// outranks another attack's strike once that strike has been read for Boss63WyrmStrikeOverS (the reel still sees each hit).</summary>
        [Test]
        public void Wyrm_ATellPendingInAStaggerShowsItsWindup()
        {
            var log = new List<WyrmLog> { new WyrmLog { step = 1000, name = "spit", seconds = 0.9, resolve = false } };
            int len = MovementMath.StepsFor(0.9, Dt), hit = 1000 + len;
            Assert.That(C.FromWyrm(log, 990, "p2", true, Dt).phase, Is.EqualTo(BossPhase63.Stagger), "staggered, the tell not yet given");
            for (int s = 1000; s < hit; s += 2)
            {
                BossAttack63 a = C.FromWyrm(log, s, "p2", true, Dt);
                Assert.That(a.phase, Is.EqualTo(BossPhase63.Windup), "step " + s + ": the spit's wind-up shows through the stagger");
                Assert.That(a.attack, Is.EqualTo("spit"));
                Assert.That(a.stepsToHit, Is.EqualTo(hit - s));
            }
            Assert.That(C.FromWyrm(log, hit, "p2", true, Dt).phase, Is.EqualTo(BossPhase63.Strike), "the hit, staggered");
            int strike = MovementMath.StepsFor(UnityTuning.Boss63WyrmStrikeS, Dt);
            BossAttack63 back = C.FromWyrm(log, hit + strike, "p2", true, Dt);
            Assert.That(back.phase, Is.EqualTo(BossPhase63.Stagger), "after the strike, still staggered: the slump again");
            Assert.That(back.why, Is.EqualTo("staggered"));

            // a tell pending while another attack's strike shows: the strike first (the reel reads the hit), then the pending wind-up
            var two = new List<WyrmLog>
            {
                new WyrmLog { step = 100, name = "lunge", seconds = 0.8, resolve = false },
                new WyrmLog { step = 150, name = "spit", seconds = 0.9, resolve = false },
            };
            int lungeHit = 100 + MovementMath.StepsFor(0.8, Dt), over = Math.Max(1, MovementMath.StepsFor(UnityTuning.Boss63WyrmStrikeOverS, Dt));
            Assert.That(over, Is.LessThan(strike), "the strike's read is cut short only when a tell is pending");
            BossAttack63 h = C.FromWyrm(two, lungeHit, "p2", false, Dt);
            Assert.That(h.phase, Is.EqualTo(BossPhase63.Strike));
            Assert.That(h.attack, Is.EqualTo("lunge"), "the lunge's hit is read");
            Assert.That(C.FromWyrm(two, lungeHit + over - 1, "p2", false, Dt).phase, Is.EqualTo(BossPhase63.Strike), "the strike's last read frame");
            BossAttack63 w = C.FromWyrm(two, lungeHit + over, "p2", false, Dt);
            Assert.That(w.phase, Is.EqualTo(BossPhase63.Windup), "then the pending spit's wind-up");
            Assert.That(w.attack, Is.EqualTo("spit"));
            var lone = new List<WyrmLog> { new WyrmLog { step = 100, name = "lunge", seconds = 0.8, resolve = false } };
            Assert.That(C.FromWyrm(lone, lungeHit + over, "p2", false, Dt).phase, Is.EqualTo(BossPhase63.Strike), "no tell pending: the full strike read");
        }

        [Test]
        public void ProofLines_TheReelsText()
        {
            Assert.That(C.AttackLine("brawlHaymaker", 1234.4, 2234.6, 1800.2), Is.EqualTo("[boss63] attack brawlHaymaker windup 1234 hit 2235 lead 1000 pose 434"));
            Assert.That(C.AttackLine("slam", 100, 700, -1), Is.EqualTo("[boss63] attack slam windup 100 hit 700 lead 600 pose off"));
            Assert.That(C.FlinchLine(5012.6, 800, 790.5, 1), Is.EqualTo("[boss63] flinch 5013 hp 800.0 -> 790.5 drawn +1 frames"));
            Assert.That(C.FlinchLine(5012.6, 800, 790.5, -1), Is.EqualTo("[boss63] flinch 5013 hp 800.0 -> 790.5 drawn off"));
            Assert.That(C.StaggerLine(14000, "window", true), Is.EqualTo("[boss63] stagger 14000 window FORCED"));
            Assert.That(C.StaggerLine(9000.4, "stuck", false), Is.EqualTo("[boss63] stagger 9000 stuck"));
            Assert.That(C.StaggerEndLine(16500, 2500), Is.EqualTo("[boss63] stagger end 16500 after 2500"));
            Assert.That(C.RmsLine("wick", 3, 0.0421, 4, 0.11, 0.55), Is.EqualTo("[boss63] rms wick s3 0.0421 m parts 4 max 0.110 m limit 0.55"));
        }

        [Test]
        public void Boss63_RegisteredBesideBoss61()
        {
            Assert.That(Array.IndexOf(FeelReel.PhaseN41T, "boss61"), Is.GreaterThanOrEqualTo(0), "boss61 is there (the control)");
            Assert.That(Array.IndexOf(FeelReel.PhaseN41T, "boss63"), Is.GreaterThanOrEqualTo(0), "boss63 is a T2 proof reel (FeelReel.N41.cs)");
            Assert.That(C.Bosses, Is.EqualTo(new[] { "wick", "brawl40", "wyrm", "loom37", "crown37" }), "the five campaign bosses by their bossRise ids");
        }
    }
}
