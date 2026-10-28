// NIGHT54 builder W (wick54; Core/Enemies/Wick37.N54.cs): THE WICK without its FLARE, the rest of its kit exactly the same. Pinned:
//   1. row on: no attack in the brain's list is wickFlare at the fight's start, nor after P2 and the cap (P3: wick43's cap flare re-arm runs
//      there) across 60 s of fighting; no FLARE tell or burn
//   2. row on: every other attack equals the row-off fight's attack field for field (id, kind, wind-up / active / recover, cost, weight,
//      distance band, legs, habit) and its config's design.n37 / design.n47 values; the attacks' own numbers (Wick37Params, and every
//      Wick47Params number but the encounter's chip and gate) equal the config's
//   3. row off: the old kit (the FLARE in legs 1-2, and on the cap with wick43's flareLeg3), the pool p.pool; the encounter's numbers apply
//      only with the row on
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Wick54Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static readonly string[] N37Ids = { "ember", "flare", "slam", "dive" };
        static readonly string[] N47Ids = { "geyser", "curtain", "ring", "charge" };

        sealed class Fight
        {
            public Wick37 w;
            public Vec3 player;
            public Wick37Stack stack;
        }

        static Fight Make(bool on, Wick47Params shared47 = null)
        {
            GameConfig c = Cfg;
            var f = new Fight();
            var events = new EventBus();
            var health = new Health(1e7, events, "player");
            var target = new BossTarget { position = () => f.player, health = health, impulse = (x, y, z) => { } };
            int id = 0;
            f.stack = new Wick37Stack(Wick37StackParams.From(c));
            f.player = new Vec3(0, 0, f.stack.p.radius - 2.5);
            f.w = new Wick37(c, Wick37Params.From(c), events, target, () => ++id) { origin = Vec3.Zero, stack = f.stack };
            f.w.n43 = true;
            f.w.p43 = Wick43Params.From(c);
            f.w.w47 = true;
            f.w.p47 = shared47 ?? Wick47Params.From(c); // the sim caches one Wick47Params across runs (shared47: the same)
            f.w.w54 = on;
            f.w.p54 = Wick54Params.From(c);
            f.w.StartFight(new BossFight { center = Vec3.Zero, scale = BossScale.One });
            return f;
        }

        static void Steps(Fight f, int n)
        {
            for (int i = 0; i < n && f.w.Alive; i++) f.w.Step(Cfg.Dt);
        }

        static bool HasFlare(Wick37 w)
        {
            foreach (Boss37Attack a in w.brain.attacks) if (a.id == Wick37.Flare) return true;
            return false;
        }

        [Test]
        public void RowOn_NoFlare_InAnyLegOrPhase()
        {
            Fight f = Make(true);
            Assert.That(HasFlare(f.w), Is.False, "P1: no wickFlare in the brain's list");
            int s = MovementMath.StepsFor(1, Cfg.Dt);
            Steps(f, MovementMath.StepsFor(f.w.p.revealSeconds, Cfg.Dt) + s);
            Assert.That(f.w.brain.Fighting, Is.True, "the reveal ended");
            Steps(f, 20 * s);
            // P2: the bar to wick47's P1 gate (Apply floors there; the gate starts P2 at the next step)
            f.w.Apply(f.w.body.maxHp);
            Steps(f, 2);
            Assert.That(f.w.brain.leg, Is.EqualTo(2), "P2 began");
            Assert.That(HasFlare(f.w), Is.False, "P2: no wickFlare");
            Steps(f, 20 * s);
            // P3: the player on the cap (wick43's cap flare re-arms after capFlareEvery charges there)
            f.player = new Vec3(0, f.stack.p.capY, (f.stack.p.radius + f.stack.p.capRadius) / 2);
            f.w.ReachedFloor(f.stack.p.capY);
            Assert.That(f.w.brain.leg, Is.EqualTo(3), "P3 began");
            Steps(f, 60 * s);
            Assert.That(HasFlare(f.w), Is.False, "P3: no wickFlare after the cap's charges");
            Assert.That(f.w.flareTells54, Is.EqualTo(0), "no FLARE tell");
            Assert.That(f.w.flareBurns54, Is.EqualTo(0), "no FLARE burn");
            Assert.That(f.w.flareHits, Is.EqualTo(0), "no FLARE hit");
            int picked = 0;
            foreach (Boss37Attack a in f.w.brain.attacks) picked += a.picks;
            Assert.That(picked, Is.GreaterThan(4), "the pick cycled through the others");
        }

        static void SameAttack(Boss37Attack a, Boss37Attack b, string why)
        {
            Assert.That(a.id, Is.EqualTo(b.id), why + " id");
            Assert.That(a.kind, Is.EqualTo(b.kind), why + " kind");
            Assert.That(a.windupSteps, Is.EqualTo(b.windupSteps), why + " windup");
            Assert.That(a.activeSteps, Is.EqualTo(b.activeSteps), why + " active");
            Assert.That(a.recoverSteps, Is.EqualTo(b.recoverSteps), why + " recover");
            Assert.That(a.cost, Is.EqualTo(b.cost), why + " cost");
            Assert.That(a.weight, Is.EqualTo(b.weight), why + " weight");
            Assert.That(a.minDist, Is.EqualTo(b.minDist), why + " minDist");
            Assert.That(a.maxDist, Is.EqualTo(b.maxDist), why + " maxDist");
            Assert.That(a.habit, Is.EqualTo(b.habit), why + " habit");
        }

        static void SameFields(object a, object b, string why, params string[] except)
        {
            foreach (FieldInfo fi in a.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Array.IndexOf(except, fi.Name) >= 0) continue;
                object x = fi.GetValue(a), y = fi.GetValue(b);
                if (x is double[] xa) Assert.That(xa, Is.EqualTo((double[])y), why + "." + fi.Name);
                else Assert.That(x, Is.EqualTo(y), why + "." + fi.Name);
            }
        }

        [Test]
        public void RowOn_EveryOtherAttack_ExactlyTheSame()
        {
            Fight on = Make(true), off = Make(false);
            GameConfig c = Cfg;
            var cfgAttack = new Dictionary<string, Boss37Attack>();
            foreach (string k in N37Ids) { Boss37Attack a = Boss37Attack.From(c, "design.n37.wick37.attacks." + k, c.Dt); cfgAttack[a.id] = a; }
            foreach (string k in N47Ids) { Boss37Attack a = Boss37Attack.From(c, "design.n47.wick47.numbers.attacks." + k, c.Dt); cfgAttack[a.id] = a; }
            var offBy = new Dictionary<string, Boss37Attack>();
            foreach (Boss37Attack a in off.w.brain.attacks) offBy[a.id] = a;
            Assert.That(on.w.brain.attacks.Count, Is.EqualTo(off.w.brain.attacks.Count - 1), "the row on: the kit less the FLARE");
            foreach (Boss37Attack a in on.w.brain.attacks)
            {
                Assert.That(offBy.ContainsKey(a.id), Is.True, a.id + " is in the row-off kit");
                SameAttack(a, offBy[a.id], a.id + " (on vs off)");
                Assert.That(a.legMask, Is.EqualTo(offBy[a.id].legMask), a.id + " legs (on vs off)");
                Assert.That(cfgAttack.ContainsKey(a.id), Is.True, a.id + " comes from the config");
                SameAttack(a, cfgAttack[a.id], a.id + " (on vs config)");
            }
            // the attacks' own numbers: damage, knock, radius, speed, timing (the encounter's chip, gates and P3 punch count may differ: wick54's)
            SameFields(on.w.p, Wick37Params.From(c), "Wick37Params");
            SameFields(on.w.p47, Wick47Params.From(c), "Wick47Params", "chip", "gate", "punches"); // the encounter's: the chip, the gates, how many crack-open punches P3 lasts
            SameFields(on.w.p43, Wick43Params.From(c), "Wick43Params");
        }

        [Test]
        public void RowOff_TheOldKit()
        {
            Fight f = Make(false);
            Boss37Attack flare = null;
            foreach (Boss37Attack a in f.w.brain.attacks) if (a.id == Wick37.Flare) flare = a;
            Assert.That(flare, Is.Not.Null, "the FLARE is in the row-off kit");
            Assert.That(flare.InLeg(1) && flare.InLeg(2), Is.True, "the FLARE in legs 1-2");
            Assert.That(flare.InLeg(3), Is.EqualTo(f.w.p43.flareLeg3), "the cap flare (wick43's flareLeg3)");
            Assert.That(f.w.body.maxHp, Is.EqualTo(JsMath.Round(f.w.p.pool)), "the pool: design.n37's");
            Assert.That(f.w.p47.chip, Is.EqualTo(Cfg.Num("design.n47.wick47.numbers.chip")), "the chip: design.n47's");
            Boss37Params bp = Boss37Params.From(Cfg, "wick37");
            Assert.That(f.w.brain.p.restSeconds, Is.EqualTo(bp.restSeconds), "the rest: design.n37's");
            Assert.That(f.w.brain.p.windowSeconds, Is.EqualTo(bp.windowSeconds), "the window: design.n37's");
            Assert.That(f.w.brain.p.stagger, Is.EqualTo(bp.stagger), "the stagger meter: design.n37's");
        }

        [Test]
        public void RowOn_TheEncounterNumbers_Apply()
        {
            var shared = Wick47Params.From(Cfg);
            double[] gateWas = (double[])shared.gate.Clone();
            Fight f = Make(true, shared);
            Wick54Params p = f.w.p54;
            if (!double.IsNaN(p.pool)) Assert.That(f.w.body.maxHp, Is.EqualTo(JsMath.Round(p.pool)), "the pool: wick54's");
            if (!double.IsNaN(p.chip)) Assert.That(f.w.p47.chip, Is.EqualTo(p.chip), "the chip: wick54's");
            if (p.gate != null) Assert.That(f.w.p47.gate, Is.EqualTo(p.gate), "the gates: wick54's");
            if (p.gate != null) Assert.That(f.w.hpLock, Is.EqualTo(p.gate[0]), "P1's lock: wick54's gate[0]");
            if (p.punches >= 1) Assert.That(f.w.p47.punches, Is.EqualTo(p.punches), "P3's punches: wick54's");
            if (!double.IsNaN(p.restSeconds)) Assert.That(f.w.brain.p.restSeconds, Is.EqualTo(p.restSeconds), "the rest: wick54's");
            if (!double.IsNaN(p.windowSeconds)) Assert.That(f.w.brain.p.windowSeconds, Is.EqualTo(p.windowSeconds), "the window: wick54's");
            if (p.burst >= 1) Assert.That(f.w.brain.p.burst, Is.EqualTo(p.burst), "the burst: wick54's");
            if (p.stagger != null) Assert.That(f.w.brain.stagger, Is.EqualTo(p.stagger[0]), "the stagger meter: wick54's");
            if (p.restByLeg != null)
            {
                Steps(f, 1);
                Assert.That(f.w.brain.p.restSeconds, Is.EqualTo(p.restByLeg[0]), "P1's rest: wick54's restByLeg[0]");
            }
            // the sim's cached Wick47Params is never edited: a row-off fight after a row-on one reads design.n47's chip
            Fight off = Make(false, shared);
            Assert.That(shared.chip, Is.EqualTo(Cfg.Num("design.n47.wick47.numbers.chip")), "the shared Wick47Params kept design.n47's chip");
            Assert.That(shared.gate, Is.EqualTo(gateWas), "the shared Wick47Params kept design.n47's gates");
            Assert.That(shared.punches, Is.EqualTo(Cfg.Int("design.n47.wick47.numbers.charge.punches")), "the shared Wick47Params kept design.n47's punches");
            Assert.That(off.w.p47.chip, Is.EqualTo(Cfg.Num("design.n47.wick47.numbers.chip")), "row off after row on: design.n47's chip");
            Assert.That(off.w.body.maxHp, Is.EqualTo(JsMath.Round(off.w.p.pool)), "row off after row on: design.n37's pool");
        }
    }
}
