// NIGHT20 U3-34.9 rows B2 and D1 (builder G; Research/2026-10-06-additions-backlog.md rows 10 and 14), through the real
// loadout on a flat floor:
// - B2, named combos on screen: every combo (POP, LAUNCH + SNIPE, CONDUCTOR, NAILBOMB, SWAP-CANCEL, SENT) also lands a style
//   award (a styleBonus with the combo's name and design.adds.b2.points) for the style meter and the points; off: the
//   callout only (NIGHT19's);
// - D1, trap combos: an enemy you pushed (the shockwave's cone or blast, a punch, a gauntlet swing, a COMET launch) that a
//   live trap kills within design.adds.d1.window s is "SENT": a combo credited to you; outside the window, or never pushed:
//   a plain trap kill.
// Both rows sit behind design.adds (the lead's bit) and their own sub-blocks.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class G20CombosTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Husks husks;
            public readonly Mover mover;
            public readonly Loadout loadout;
            public Vec3 dir = new Vec3(0, 0, -1);
            public readonly List<string> combos = new List<string>();
            public readonly List<StyleBonusEvent> bonuses = new List<StyleBonusEvent>();

            public Rig(bool adds)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (Eye, dir));
                loadout.n19 = new N19Blocks { gust = true };
                loadout.n20 = new N20Blocks { shockwave = true, zoom = true, arsenal = true, adds = adds };
                loadout.Reset(new List<string> { "shotgun", "revolver" }, 1);
                events.On<ComboEvent>(N19Events.Combo, c => combos.Add(c.name));
                events.On<StyleBonusEvent>("styleBonus", b => bonuses.Add(new StyleBonusEvent { name = b.name, points = b.points, weapon = b.weapon }));
                for (int i = 0; i < 30; i++) Step(false);
            }

            public Vec3 Eye => new Vec3(mover.position.x, mover.position.y + cfg.movement.body.eyeHeight, mover.position.z);

            public void Step(bool lt)
            {
                mover.Step(default, cfg.Dt);
                loadout.Step(new CombatInput { weapon = -1, altFire = new Button { held = lt, pressed = lt } }, cfg.Dt);
                husks.Step(new Vec3(mover.position.x, mover.position.y, mover.position.z), new Health(1e9, events, "dummy"), cfg.Dt);
            }

            public void Wait(double s)
            {
                int n = MovementMath.StepsFor(s, cfg.Dt);
                for (int i = 0; i < n; i++) Step(false);
            }

            /// <summary>A live trap's blow (Traps.DamageBy: the loadout's own damage as "trap").</summary>
            public bool Trap(Enemy e) => loadout.Damage(e, 1e6, false, "trap", e.position, false, 1, 0);
        }

        static bool AddsOn(string row) => cfg.Design("adds." + row + ".enabled");

        [Test]
        public void TheRows_AreInTheDesignConfig()
        {
            Assert.That(AddsOn("b2"), Is.True, "design.adds.b2");
            Assert.That(AddsOn("d1"), Is.True, "design.adds.d1");
            Assert.That(cfg.Num("design.adds.d1.window"), Is.GreaterThan(0));
            Assert.That(cfg.Num("design.adds.b2.points.SENT"), Is.GreaterThan(0));
        }

        [Test]
        public void AnEnemyTheShockwavePushed_KilledByATrapWithinTheWindow_IsSent()
        {
            var r = new Rig(true);
            Swarmer s = (Swarmer)r.husks.Spawn("swarmer", new Vec3(2.5, 0, -5), r.mover.position); // in the cone, off the pellets' 10 deg
            r.Step(true); // the shockwave
            Assert.That(s.alive, Is.True, "pushed, not killed");
            r.Wait(1);
            Assert.That(r.Trap(s), Is.True, "the trap kills it");
            Assert.That(r.combos, Does.Contain("SENT"));
            StyleBonusEvent b = r.bonuses.Find(x => x.name == "SENT");
            Assert.That(b, Is.Not.Null, "the style award");
            Assert.That(b.points, Is.EqualTo(cfg.Num("design.adds.b2.points.SENT")));
        }

        [Test]
        public void ATrapKill_AfterTheWindow_OrOnAnEnemyNeverPushed_IsPlain()
        {
            var r = new Rig(true);
            Swarmer late = (Swarmer)r.husks.Spawn("swarmer", new Vec3(2.5, 0, -5), r.mover.position);
            r.Step(true);
            r.Wait(cfg.Num("design.adds.d1.window") + 0.5);
            r.Trap(late);
            Swarmer never = (Swarmer)r.husks.Spawn("swarmer", new Vec3(20, 0, 20), r.mover.position);
            r.Trap(never);
            Assert.That(r.combos, Does.Not.Contain("SENT"));
        }

        [Test]
        public void APunchedEnemy_IsSentToo()
        {
            var r = new Rig(true);
            Husk h = (Husk)r.husks.Spawn("husk", new Vec3(0, 0, -1.5), r.mover.position);
            r.mover.Step(default, cfg.Dt);
            r.loadout.Step(new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } }, cfg.Dt);
            r.Wait(0.5);
            Assert.That(h.alive, Is.True);
            r.Trap(h);
            Assert.That(r.combos, Does.Contain("SENT"));
        }

        [Test]
        public void WithTheAddsBitOff_NoSent_AndCombosPayNoStyleAward()
        {
            var r = new Rig(false);
            Swarmer s = (Swarmer)r.husks.Spawn("swarmer", new Vec3(2.5, 0, -5), r.mover.position);
            r.Step(true);
            r.Wait(1);
            r.Trap(s);
            Assert.That(r.combos, Does.Not.Contain("SENT"));
            Assert.That(r.bonuses.Find(x => x.name == "SENT"), Is.Null);
        }

        [Test]
        public void EveryNamedCombo_HasItsStyleAward()
        {
            foreach (string name in new[] { Combos19.Pop, Combos19.LaunchSnipe, Combos19.Conductor, Combos19.Nailbomb, Combos19.SwapCancel, "SENT" })
                Assert.That(cfg.Has("design.adds.b2.points." + name), Is.True, name);
        }
    }
}
