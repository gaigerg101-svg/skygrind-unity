// NIGHT10 U3-13.9: the ULTRAKILL alignment flags (design.ultra, Core/Ultra.cs), each on and off. New design (D-17): no
// web test exists. Off (no design.ultra, as the golden sets and replays read config.json) every rule is the web's.
// - freshDamage off: a KESTREL hit deals base x primed x cross, the arsenal's FRESH stays on the hit for points and
//   style (HitEvent.freshMult); on: FRESH multiplies the damage too (arsenal.js).
// - styleBreakPause: the drain pauses in a break or with no enemy near; off: never.
// - parryRefillsDash: a parry refills the dash pips (the web's punch.js does too); false: the pips stay spent.
// - parryHitstop: 0.25 s on a plain parry; off: punch.parryHitstop; a perfect parry keeps its own either way.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class UltraTests
    {
        static GameConfig cfg, web;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            web = GameConfig.Load(Path.Combine(Port.Root, "config.json"), Path.Combine(Port.Root, "no-such-design.json")); // no design config: the web's
        }

        [Test]
        public void TheFlags_ReadFromDesignUltra_AndTheWebsRulesWithoutIt()
        {
            Ultra on = Ultra.From(cfg);
            Assert.That(on.freshDamage, Is.False);
            Assert.That(on.styleBreakPause, Is.True);
            Assert.That(on.parryRefillsDash, Is.True);
            Assert.That(on.parryHitstop, Is.EqualTo(0.25));
            Ultra off = Ultra.From(web);
            Assert.That(off.freshDamage, Is.True, "the web: FRESH scales damage");
            Assert.That(off.styleBreakPause, Is.False);
            Assert.That(off.parryRefillsDash, Is.True, "the web's parry restores stamina (punch.js :39)");
            Assert.That(off.parryHitstop, Is.EqualTo(0));
        }

        static HitEvent KestrelHit(bool freshDamage)
        {
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            var mover = new Mover(cfg.movement, world);
            mover.Reset(0, 0, 0);
            var husks = new Husks(cfg, events, enemies, world, -30);
            Husk h = husks.Spawn(new Vec3(0, 0, -6), new Vec3(0, 0, 0));
            h.hp = h.maxHp = 1e6;
            Vec3 eye = new Vec3(0, 1.6, 0);
            Vec3 dir = Vec3.Normalize(new Vec3(0, h.AimPoint.y - 0.3 - 1.6, -6)); // the chest, not the head
            var loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (eye, dir)) { freshDamage = freshDamage };
            HitEvent hit = null;
            events.On<HitEvent>("hit", e => hit = e);
            loadout.Step(new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } }, cfg.Dt);
            Assert.That(hit, Is.Not.Null, "the KESTREL hit");
            return hit;
        }

        [Test]
        public void FreshDamageOff_FreshPaysPointsAndStyle_NotDamage()
        {
            HitEvent on = KestrelHit(true), off = KestrelHit(false);
            Assert.That(on.freshMult, Is.Not.EqualTo(1), "the arsenal is on: a FRESH multiplier rides on the hit");
            Assert.That(off.freshMult, Is.EqualTo(on.freshMult), "points and style still read FRESH");
            Assert.That(on.damage, Is.EqualTo(off.damage * on.freshMult).Within(1e-9), "on: x FRESH; off: without it");
            Assert.That(off.damage, Is.EqualTo(cfg.Num("weapons.revolver.damage")).Within(1e-9), "a first, unprimed KESTREL body shot: the base damage");
        }

        [Test]
        public void StyleBreakPause_PausesInABreakOrWithNoEnemyNear_OffNever()
        {
            Ultra on = Ultra.From(cfg), off = Ultra.From(web);
            Assert.That(on.DrainPaused(true, true), Is.True, "a break");
            Assert.That(on.DrainPaused(false, false), Is.True, "nothing near");
            Assert.That(on.DrainPaused(false, true), Is.False, "fighting: it drains");
            Assert.That(off.DrainPaused(true, false), Is.False, "off: the web drains always");
            // A paused meter keeps its value: the drain is x drainMult (0 while paused).
            var events = new EventBus();
            var style = new StyleMeter(cfg, events, () => "GROUND", () => true); // a trick pays with an enemy near
            style.Start();
            events.Emit("trick", "wallJump");
            double before = style.meter;
            Assert.That(before, Is.GreaterThan(0));
            style.drainMult = 0;
            for (int i = 0; i < 120; i++) style.Step(cfg.Dt);
            Assert.That(style.meter, Is.EqualTo(before), "paused: no drain");
            style.drainMult = 1;
            for (int i = 0; i < 120; i++) style.Step(cfg.Dt);
            Assert.That(style.meter, Is.LessThan(before), "running: it drains");
        }

        static double PipsAfterAParry(bool refill)
        {
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            var mover = new Mover(cfg.movement, world);
            mover.Reset(0, 0, 0);
            var husks = new Husks(cfg, events, enemies, world, -30);
            Husk h = husks.Spawn(new Vec3(0, 0, -1.6), new Vec3(0, 0, 0));
            h.windupSteps = 30; // mid wind-up, before any perfect window
            Vec3 eye = new Vec3(0, 1.6, 0), dir = Vec3.Normalize(new Vec3(0, h.AimHeight - 1.6, -1.6));
            var loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (eye, dir)) { parryRefillsDash = refill };
            var parries = new List<HitEvent>();
            events.On<HitEvent>("parry", e => parries.Add(e));
            mover.pips = 0; // dashed out
            loadout.Step(new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } }, cfg.Dt);
            Assert.That(parries.Count, Is.EqualTo(1), "the punch was a parry");
            return mover.pips;
        }

        [Test]
        public void ParryRefillsDash_OnRefillsThePips_FalseLeavesThemSpent()
        {
            var mover = new Mover(cfg.movement, new TriangleWorld());
            Assert.That(PipsAfterAParry(true), Is.EqualTo(mover.MaxPips));
            Assert.That(PipsAfterAParry(false), Is.EqualTo(0));
        }

        [Test]
        public void ParryHitstop_Is0_25OnAPlainParry_ThePerfectKeepsItsOwn_OffIsTheWebs()
        {
            Ultra on = Ultra.From(cfg), off = Ultra.From(web);
            double webStop = cfg.Num("punch.parryHitstop"), perfect = cfg.Num("design.parry.perfectHitstop");
            Assert.That(on.ParryHitstop(false, perfect, webStop), Is.EqualTo(0.25));
            Assert.That(on.ParryHitstop(true, perfect, webStop), Is.EqualTo(perfect));
            Assert.That(off.ParryHitstop(false, perfect, webStop), Is.EqualTo(webStop));
        }
    }
}
