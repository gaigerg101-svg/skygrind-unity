// NIGHT9 U3-08: the arsenal (combat/arsenal.js). The web's two system tests (tests/arsenal.test.js, "the arsenal system
// (POWER P3)"), then the port's: a session through the loadout (a swap primes, a family change CROSSes and the style
// meter pays CROSS), a forged THUNDERCLAP's STAGGER landing through the arsenal, FLIP's mods, OVERCLOCK, burn ticks.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class ArsenalTests
    {
        static GameConfig cfg;
        static ArsenalParams a;
        static WeaponCatalog catalog;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            a = ArsenalParams.From(cfg);
            catalog = WeaponCatalog.From(cfg);
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Mover mover;
            public readonly Loadout loadout;
            public readonly Husks husks;
            public readonly List<string> seen = new List<string>();

            public Rig()
            {
                world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
                husks = new Husks(cfg, events, enemies, world, -30);
                events.On<ArsenalEvent>("primed", e => seen.Add("primed"));
                events.On<ArsenalEvent>("crossHit", e => seen.Add(e.combo ?? "cross"));
                events.On<ArsenalEvent>("swapKill", e => seen.Add("swap"));
            }

            public Husk Husk(double z = -5)
            {
                Husk h = husks.Spawn(new Vec3(0, 0, z), new Vec3(0, 0, 0));
                h.hp = h.maxHp = 1e9;
                return h;
            }
        }

        static readonly Vec3 At = new Vec3(0, 0, 0);

        [Test, Description("arsenal.test.js: stacks fresh x primed x cross, and a STUCK nail sets up a blast cross")]
        public void StacksFreshPrimedCross_AndAStuckNailSetsUpABlastCross()
        {
            var r = new Rig();
            Arsenal ars = r.loadout.arsenal;
            Husk e = r.Husk();
            // Fresh revolver: 1.25x.
            Assert.That(ars.Hit(e, "revolver").mult, Is.EqualTo(1.25).Within(1e-9));
            ars.After(e, "revolver", false, false, null, null, false, At);
            // Swap to the shotgun: primed + a cross from kinetic to edge: 1.25 x 1.35 x 1.5.
            ars.SwapIn("shotgun");
            ArsenalHit tags = ars.Hit(e, "shotgun");
            Assert.That(tags.primed, Is.True);
            Assert.That(tags.cross, Is.True);
            Assert.That(tags.mult, Is.EqualTo(1.25 * 1.35 * 1.5).Within(1e-9));
            ars.After(e, "shotgun", tags.primed, tags.cross, tags.combo, tags.family, false, At);
            Assert.That(r.seen, Is.EqualTo(new[] { "primed", "cross" }));
            // STUCK keeps the volt tag: a rocket (gravity) 5 s later still crosses.
            Husk f = r.Husk(-6);
            ars.Status(f, "stuck");
            for (int i = 0; i < 5 * 120; i++) ars.Step(1.0 / 120, "rocket");
            Assert.That(ars.Hit(f, "rocket").cross, Is.True);
        }

        [Test, Description("arsenal.test.js: uses wear a weapon out; out of hand it recovers; a kill right after a swap is a SWAP")]
        public void UsesWear_OutOfHandRecovers_AKillRightAfterASwapIsASwap()
        {
            var r = new Rig();
            Arsenal ars = r.loadout.arsenal;
            double cost = catalog.Get("rail").freshCost.Value;
            for (int i = 0; i < 5; i++) ars.Use("rail");
            Assert.That(ars.Fresh("rail"), Is.EqualTo(System.Math.Max(0, 1 - 5 * cost)).Within(1e-9));
            for (int i = 0; i < 120; i++) ars.Step(1.0 / 120, "revolver"); // rail out of hand for 1 s (after recoverDelay)
            Assert.That(ars.Fresh("rail"), Is.EqualTo(System.Math.Min(1, System.Math.Max(0, 1 - 5 * cost) + a.recover * (1 - a.recoverDelay))).Within(0.005));
            for (int i = 0; i < 120; i++) ars.Step(1.0 / 120, "rail"); // in hand: no recovery
            double held = ars.Fresh("rail");
            for (int i = 0; i < 120; i++) ars.Step(1.0 / 120, "rail");
            Assert.That(ars.Fresh("rail"), Is.EqualTo(held));
            ars.SwapIn("rocket");
            Husk e = r.Husk();
            ArsenalHit tags = ars.Hit(e, "rocket");
            ars.After(e, "rocket", tags.primed, tags.cross, tags.combo, tags.family, true, At);
            Assert.That(r.seen.FindAll(s => s == "swap").Count, Is.EqualTo(1));
        }

        [Test, Description("through the loadout: the THUNDERCLAP swapped in primes and CROSSes a KESTREL-hit husk; HUD tags ride the hit; style pays CROSS")]
        public void ThroughTheLoadout_ASwapPrimes_AFamilyChangeCrosses_StylePaysCross()
        {
            var r = new Rig();
            var style = new StyleMeter(cfg, r.events, () => "GROUND", () => true);
            Arsenal ars = r.loadout.arsenal;
            style.getFresh = w => ars.enabled && ars.Catalog.Has(w == "punch" || w == "orb" ? "gauntlet" : w) ? ars.Fresh(w) : (double?)null;
            style.Start();
            var awards = new List<string>();
            r.events.On<StyleAwardEvent>("styleAward", e => { if (e.points > 0) awards.Add(e.name); });
            var hits = new List<HitEvent>();
            r.events.On<HitEvent>("hit", e => hits.Add(e));
            Husk h = r.Husk();
            Assert.That(r.loadout.Weapon, Is.EqualTo("revolver"));
            r.loadout.Damage(h, 10, false, "revolver", new Vec3(0, 1.6, 0));
            Assert.That(hits[0].freshMult, Is.EqualTo(1.25).Within(1e-9), "a fresh KESTREL: x1.25");
            Assert.That(hits[0].primed || hits[0].cross, Is.False);
            // Buy the THUNDERCLAP (it goes in hand) and step until it is raised: the arsenal's swapIn.
            r.loadout.AddWeapon("shotgun");
            for (int i = 0; i < 200 && r.loadout.Weapon != "shotgun"; i++) r.loadout.Step(new CombatInput { weapon = -1 }, cfg.Dt);
            Assert.That(r.loadout.Weapon, Is.EqualTo("shotgun"));
            r.loadout.Damage(h, 10, false, "shotgun", new Vec3(0, 1.6, 0));
            HitEvent sg = hits[1];
            Assert.That(sg.primed, Is.True, "the first hit within primedWindow of the raise");
            Assert.That(sg.cross, Is.True, "kinetic, then edge");
            Assert.That(sg.damage, Is.EqualTo(10 * 1.25 * (1 + a.primedBonus) * a.crossMult).Within(1e-9));
            Assert.That(r.seen, Is.EqualTo(new[] { "primed", "cross" }));
            Assert.That(awards.Contains("CROSS"), Is.True, "the style meter pays CROSS: " + string.Join(", ", awards));
        }

        [Test, Description("a forged THUNDERCLAP's edge bonus (STAGGER) lands on the enemy through the arsenal (forge.js loadout.arsenal.status)")]
        public void AForgedThunderclapsStagger_Lands()
        {
            var r = new Rig();
            var forge = new Forge(cfg, r.events);
            forge.Reset(77);
            r.loadout.after = forge.After;
            forge.status = r.loadout.arsenal.Status;
            var statuses = new List<ArsenalEvent>();
            r.events.On<ArsenalEvent>("status", e => statuses.Add(e));
            Husk h = r.Husk();
            forge.Give("shotgun", 0);
            for (int i = 0; i < 40 && statuses.Count == 0; i++) r.loadout.Damage(h, 1, false, "shotgun", new Vec3(0, 1.6, 0));
            Assert.That(statuses.Count, Is.EqualTo(1));
            Assert.That(statuses[0].status, Is.EqualTo("stagger"));
            Assert.That(statuses[0].target, Is.EqualTo(h.id));
            Assert.That(statuses[0].seconds, Is.EqualTo(a.statusTime["stagger"]));
            Assert.That(r.loadout.arsenal.HasStatus(h, "stagger"), Is.True);
        }

        [Test, Description("FLIP (perkEffects arsenal): a bigger primed bonus and x recover out of hand; off again with no FLIP")]
        public void Flip_RaisesThePrimedBonus_AndTheRecovery()
        {
            var r = new Rig();
            Arsenal ars = r.loadout.arsenal;
            PerkEffects fx = PerkMath.Effects(new List<string> { "flip" }, cfg);
            ars.SetMods(fx.arsenalRecover, fx.arsenalPrimedBonus);
            Husk h = r.Husk();
            ars.SwapIn("revolver");
            Assert.That(ars.Hit(h, "revolver").mult, Is.EqualTo(1.25 * (1 + fx.arsenalPrimedBonus.Value)).Within(1e-9));
            // From empty (no use recorded, so no recoverDelay), half a second out of hand: neither run reaches 1.
            ars.DebugSetFresh("rail", 0);
            for (int i = 0; i < 60; i++) ars.Step(1.0 / 120, "revolver");
            double withFlip = ars.Fresh("rail");
            var r2 = new Rig();
            r2.loadout.arsenal.DebugSetFresh("rail", 0);
            for (int i = 0; i < 60; i++) r2.loadout.arsenal.Step(1.0 / 120, "revolver");
            double without = r2.loadout.arsenal.Fresh("rail");
            Assert.That(without, Is.EqualTo(a.recover * 0.5).Within(1e-9));
            Assert.That(withFlip, Is.EqualTo(without * fx.arsenalRecover.Value).Within(1e-9));
            ars.SetMods(null, null);
            ars.SwapIn("shotgun");
            Assert.That(ars.Hit(r.Husk(-7), "shotgun").primed, Is.True);
        }

        [Test, Description("OVERCLOCK (powerUps.js -> arsenal.setOverclock): every weapon full, no wear while it runs")]
        public void Overclock_FillsEveryWeapon_AndNothingWears()
        {
            var r = new Rig();
            Arsenal ars = r.loadout.arsenal;
            for (int i = 0; i < 4; i++) ars.Use("shotgun");
            Assert.That(ars.Fresh("shotgun"), Is.LessThan(1));
            ars.SetOverclock(true);
            Assert.That(ars.Fresh("shotgun"), Is.EqualTo(1));
            ars.Use("shotgun");
            Assert.That(ars.Fresh("shotgun"), Is.EqualTo(1));
            ars.SetOverclock(false);
            ars.Use("shotgun");
            Assert.That(ars.Fresh("shotgun"), Is.LessThan(1));
        }

        [Test, Description("BURN ticks burnDps x burnTick through the loadout's damage every burnTick s while it lasts (shot 0: no hit award)")]
        public void Burn_TicksThroughTheLoadoutsDamage()
        {
            var r = new Rig();
            Arsenal ars = r.loadout.arsenal;
            var burns = new List<HitEvent>();
            r.events.On<HitEvent>("hit", e => { if (e.weapon == "burn") burns.Add(e); });
            Husk h = r.Husk();
            ars.Status(h, "burn");
            int steps = MovementMath.StepsFor(a.statusTime["burn"] + 1, cfg.Dt);
            for (int i = 0; i < steps; i++) ars.Step(cfg.Dt, "revolver");
            Assert.That(burns.Count, Is.GreaterThan(0));
            Assert.That(burns.Count, Is.LessThanOrEqualTo((int)System.Math.Ceiling(a.statusTime["burn"] / a.burnTick) + 1));
            Assert.That(burns.TrueForAll(b => b.shot == 0 && b.freshMult == 1), Is.True);
            Assert.That(burns[0].damage, Is.EqualTo(a.burnDps * a.burnTick).Within(1e-9));
        }
    }
}
