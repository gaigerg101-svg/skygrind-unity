// NIGHT55 builder B (wick55; Core/Enemies/Wick37.N55.cs): THE WICK's encounter as a punish-the-window fight, every attack exactly as wick54
// ships it (DECIDE55 D-7). Pinned:
//   1. row on: every attack equals the wick55-off fight's field for field (id, kind, wind-up / active / recover, cost, weight, distance band,
//      legs, habit) and its config's; the attacks' own numbers (Wick37Params, Wick43Params, every Wick47Params number but the encounter's chip,
//      gate and punches) equal the config's
//   2. row on: the encounter's numbers apply (the chip, the window's length, the stagger meter); a hit inside the stagger window lands
//      windowGain x full; the sim's cached Wick47Params is never edited
//   3. the bar tells the truth (B-1): with the row on the bare / cracked-open core reads OPEN (Open43) with its time; off it does not
//   4. row off: wick54's fight (its chip, the boss37 window, the kit's meter)
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Wick55Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Wick37 Make(bool on55, Wick47Params shared47 = null)
        {
            GameConfig c = Cfg;
            var events = new EventBus();
            var health = new Health(1e7, events, "player");
            Vec3 player = default;
            var stack = new Wick37Stack(Wick37StackParams.From(c));
            player = new Vec3(0, 0, stack.p.radius - 2.5);
            var target = new BossTarget { position = () => player, health = health, impulse = (x, y, z) => { } };
            int id = 0;
            var w = new Wick37(c, Wick37Params.From(c), events, target, () => ++id) { origin = Vec3.Zero, stack = stack };
            w.n43 = true;
            w.p43 = Wick43Params.From(c);
            w.w47 = true;
            w.p47 = shared47 ?? Wick47Params.From(c);
            w.w54 = true;
            w.p54 = Wick54Params.From(c);
            w.w55 = on55;
            w.p55 = Wick55Params.From(c);
            w.StartFight(new BossFight { center = Vec3.Zero, scale = BossScale.One });
            return w;
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
            Assert.That(a.legMask, Is.EqualTo(b.legMask), why + " legs");
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
        public void RowOn_EveryAttack_ExactlyAsWick54Ships()
        {
            Wick37 on = Make(true), off = Make(false);
            GameConfig c = Cfg;
            Assert.That(on.brain.attacks.Count, Is.EqualTo(off.brain.attacks.Count), "the same kit");
            var offBy = new Dictionary<string, Boss37Attack>();
            foreach (Boss37Attack a in off.brain.attacks) offBy[a.id] = a;
            var cfgAttack = new Dictionary<string, Boss37Attack>();
            foreach (string k in new[] { "ember", "flare", "slam", "dive" }) { Boss37Attack a = Boss37Attack.From(c, "design.n37.wick37.attacks." + k, c.Dt); cfgAttack[a.id] = a; }
            foreach (string k in new[] { "geyser", "curtain", "ring", "charge" }) { Boss37Attack a = Boss37Attack.From(c, "design.n47.wick47.numbers.attacks." + k, c.Dt); cfgAttack[a.id] = a; }
            // NIGHT56 G (the lead's Editor run: "wickSlam (on vs config) legs Expected: 7 But was: 1"): the TEST was wrong, not the row. wick47's kit
            // (Wick37.N47.cs StartFight47, shipping since NIGHT47, on and off alike) re-legs two NIGHT37 attacks: the DIVE leaves (legMask 0) and the
            // SLAM stays on the floor, P1 only (legMask 1). The config's design.n37 attacks keep legs 1-3; the fight's kit is the config plus that rule.
            cfgAttack[Wick37.Dive].legMask = 0;
            cfgAttack[Wick37.Slam].legMask = 1;
            foreach (Boss37Attack a in on.brain.attacks)
            {
                Assert.That(offBy.ContainsKey(a.id), Is.True, a.id + " is in wick54's kit");
                SameAttack(a, offBy[a.id], a.id + " (wick55 on vs off)");
                Assert.That(cfgAttack.ContainsKey(a.id), Is.True, a.id + " comes from the config");
                SameAttack(a, cfgAttack[a.id], a.id + " (on vs config)");
            }
            SameFields(on.p, Wick37Params.From(c), "Wick37Params");
            SameFields(on.p43, Wick43Params.From(c), "Wick43Params");
            SameFields(on.p47, Wick47Params.From(c), "Wick47Params", "chip", "gate", "punches"); // the encounter's (wick54's / wick55's)
            Assert.That(on.p47.gate, Is.EqualTo(off.p47.gate), "the gates: wick54's");
            Assert.That(on.p47.punches, Is.EqualTo(off.p47.punches), "P3's punches: wick54's");
            Assert.That(on.body.maxHp, Is.EqualTo(off.body.maxHp), "the pool: wick54's");
        }

        [Test]
        public void RowOn_TheEncounterNumbers_Apply_TheWindowPays()
        {
            var shared = Wick47Params.From(Cfg);
            double chipWas = shared.chip;
            Wick37 w = Make(true, shared);
            Wick55Params p = w.p55;
            if (!double.IsNaN(p.chip)) Assert.That(w.p47.chip, Is.EqualTo(p.chip), "the chip: wick55's");
            if (!double.IsNaN(p.windowSeconds)) Assert.That(w.brain.p.windowSeconds, Is.EqualTo(p.windowSeconds), "the window: wick55's");
            if (p.stagger != null) Assert.That(w.brain.stagger, Is.EqualTo(p.stagger[0]), "the stagger meter: wick55's");
            Assert.That(shared.chip, Is.EqualTo(chipWas), "the sim's cached Wick47Params is never edited");
            // inside the window: x windowGain
            int s = MovementMath.StepsFor(w.p.revealSeconds, Cfg.Dt) + MovementMath.StepsFor(1, Cfg.Dt);
            for (int i = 0; i < s; i++) w.Step(Cfg.Dt);
            Assert.That(w.brain.Fighting, Is.True, "the reveal ended");
            for (int k = 0; k < 20 && !w.brain.Open; k++) w.brain.Stagger(1, "test");
            Assert.That(w.brain.Open, Is.True, "the window opened");
            double gain = double.IsNaN(p.windowGain) ? 1 : p.windowGain;
            double share = w.Filter(10, "revolver", new Vec3(0, 0, 5), out bool blocked);
            Assert.That(blocked, Is.False);
            Assert.That(share, Is.EqualTo(10 * gain).Within(1e-9), "a hit inside the window lands windowGain x full");
            Assert.That(w.windowHits55, Is.EqualTo(1), "counted");
        }

        [Test]
        public void TheOpenCore_ReadsOpen_OnlyWithTheRow()
        {
            Wick37 on = Make(true), off = Make(false);
            int s = MovementMath.StepsFor(on.p.revealSeconds, Cfg.Dt) + MovementMath.StepsFor(1, Cfg.Dt);
            for (int i = 0; i < s; i++) { on.Step(Cfg.Dt); off.Step(Cfg.Dt); }
            for (int i = 0; i < on.crackLeft.Length; i++) { on.crackLeft[i] = 0; off.crackLeft[i] = 0; }
            Assert.That(on.brain.Open || off.brain.Open, Is.False, "no stagger window");
            on.coreOpen47 = off.coreOpen47 = 90;
            Assert.That(on.Open43, Is.True, "row on: the cracked-open core reads OPEN (it takes every hit in full)");
            Assert.That(on.CrackLeftMax, Is.EqualTo(90), "row on: the bar's time is the core's");
            Assert.That(off.Open43, Is.False, "row off: NIGHT54's bar (the core's state not on it)");
            Assert.That(off.CrackLeftMax, Is.EqualTo(0), "row off: no time");
            on.coreOpen47 = off.coreOpen47 = 0;
            on.bare47 = off.bare47 = 30;
            Assert.That(on.Open43, Is.True, "row on: the bare core after the PLATE RING reads OPEN");
            Assert.That(off.Open43, Is.False, "row off: as NIGHT54");
        }

        [Test]
        public void RowOff_Wick54sFight()
        {
            Wick37 w = Make(false);
            Assert.That(w.p47.chip, Is.EqualTo(Cfg.Num("design.n54.wick54.numbers.chip")), "the chip: wick54's");
            Boss37Params bp = Boss37Params.From(Cfg, "wick37");
            Assert.That(w.brain.p.windowSeconds, Is.EqualTo(bp.windowSeconds), "the window: the brain's own");
            Assert.That(w.brain.p.stagger, Is.EqualTo(bp.stagger), "the stagger meter: the kit's");
        }
    }
}
