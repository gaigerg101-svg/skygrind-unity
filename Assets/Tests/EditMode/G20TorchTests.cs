// NIGHT20 U3-34.9 row B5 (builder G; Research/2026-10-06-additions-backlog.md row 25): the TORCH, ported from the web's
// guns.js (flame() :96-117, torchAlt() :346-370, stepWalls() :373-400, the heat and the trigger in step()) with the web's
// numbers (weapons.torch, weapons.specials.torch), through the real loadout on a flat floor, behind design.adds.b5:
// - the trigger: a flame tick every weapons.torch.tick s while held: every body in the 18 deg cone within 7 m with a clear
//   line takes weapons.torch.damage and BURNS; nothing past the range or outside the cone;
// - the heat: weapons.torch.heatTime s of flame overheats it (the `overheat` event), locked lockTime s: no flame;
// - LT / SPECIAL: the fire wall, a 7 m strip from 2.5 m ahead along the floor, live 3 s: bodies standing in it take
//   dps x tick every 0.25 s and burn; `fireWall` event; 4 s cooldown; +0.35 heat;
// - off (no design.adds.b5): the TORCH fires nothing (NIGHT19's loadout: not a port).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class G20TorchTests
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
            public readonly List<HitEvent> hits = new List<HitEvent>();
            public readonly List<string> log = new List<string>();
            public FireWallEvent wall;

            public Rig(bool b5 = true)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (Eye, dir));
                loadout.n19 = new N19Blocks { gust = true };
                loadout.n20 = new N20Blocks { shockwave = true, zoom = true, arsenal = true, adds = b5 };
                loadout.freshDamage = false;
                loadout.Reset(new List<string> { "torch", "revolver" }, 1);
                events.On<HitEvent>("hit", h => hits.Add(new HitEvent { weapon = h.weapon, damage = h.damage, enemy = h.enemy }));
                foreach (string e in new[] { "fire", "flame", "overheat" })
                {
                    string name = e;
                    events.On(name, _ => log.Add(name));
                }
                events.On<FireWallEvent>(G20Events.FireWall, w => wall = new FireWallEvent { a = w.a, b = w.b, life = w.life, width = w.width });
                for (int i = 0; i < 30; i++) Step(false, false);
            }

            public Vec3 Eye => new Vec3(mover.position.x, mover.position.y + cfg.movement.body.eyeHeight, mover.position.z);

            public void Step(bool fire, bool lt)
            {
                mover.Step(default, cfg.Dt);
                loadout.Step(new CombatInput { weapon = -1, fire = new Button { held = fire, pressed = fire }, altFire = new Button { held = lt, pressed = lt } }, cfg.Dt);
                // the husks are not stepped: they stand where they spawned
            }

            public Husk Husk(double x, double z)
            {
                var h = (Husk)husks.Spawn("husk", new Vec3(x, 0, z), new Vec3(1000, 0, 1000));
                h.hp = h.maxHp = 1e6;
                return h;
            }

            public int Count(string name)
            {
                int n = 0;
                foreach (string s in log) if (s == name) n++;
                return n;
            }

            public double DamageOn(Enemy e)
            {
                double d = 0;
                foreach (HitEvent h in hits) if (h.enemy == e.id && h.weapon == "torch") d += h.damage; // the flame and the wall, not the burn's ticks
                return d;
            }
        }

        [Test]
        public void TheFlame_BurnsEveryBodyInItsCone_WithinItsRange_EveryTick()
        {
            var r = new Rig();
            Husk near = r.Husk(0.5, -5); // in the cone, 5 m
            Husk far = r.Husk(0, -10); // past the 7 m range
            Husk wide = r.Husk(4, -4); // 45 deg off: outside the 18 deg cone
            int steps = MovementMath.StepsFor(1, cfg.Dt);
            for (int i = 0; i < steps; i++) r.Step(true, false);
            int ticks = r.Count("flame");
            Assert.That(ticks, Is.InRange(9, 11), "a tick every 0.1 s");
            double tick = cfg.Num("weapons.torch.damage") * r.loadout.TuneDamage("torch"); // x the retune's damage knob (design.arsenal.n20.tune.torch)
            Assert.That(r.DamageOn(near), Is.EqualTo(ticks * tick).Within(ticks * tick * 0.35), "each tick's damage (x the momentum / freshness rules)");
            Assert.That(r.DamageOn(far), Is.EqualTo(0), "past the range");
            Assert.That(r.DamageOn(wide), Is.EqualTo(0), "outside the cone");
            Assert.That(r.loadout.arsenal.HasStatus(near, "burn"), Is.True, "it burns");
        }

        [Test]
        public void HeldPastItsHeatTime_ItOverheats_AndTheLockStopsTheFlame()
        {
            var r = new Rig();
            int steps = MovementMath.StepsFor(cfg.Num("weapons.torch.heatTime") + 0.2, cfg.Dt);
            for (int i = 0; i < steps; i++) r.Step(true, false);
            Assert.That(r.Count("overheat"), Is.EqualTo(1));
            Assert.That(r.loadout.TorchLocked, Is.True);
            int before = r.Count("flame");
            for (int i = 0; i < 60; i++) r.Step(true, false);
            Assert.That(r.Count("flame"), Is.EqualTo(before), "locked: no flame");
        }

        [Test]
        public void LT_LaysTheFireWall_ItBurnsWhatStandsInIt_ThenTheCooldownHolds()
        {
            var r = new Rig();
            Husk inWall = r.Husk(0.3, -6); // 2.5 .. 9.5 m ahead, 0.3 m off the line
            Husk beside = r.Husk(3, -6); // 3 m off: outside the 1.2 m width
            r.Step(false, true);
            Assert.That(r.wall, Is.Not.Null, "the fireWall event");
            Assert.That(r.wall.a.z, Is.EqualTo(-cfg.Num("weapons.specials.torch.ahead")).Within(0.05));
            Assert.That(r.wall.b.z, Is.EqualTo(-cfg.Num("weapons.specials.torch.ahead") - cfg.Num("weapons.specials.torch.length")).Within(0.05));
            Assert.That(r.loadout.TorchHeat, Is.EqualTo(cfg.Num("weapons.specials.torch.heat")).Within(0.02), "+0.35 heat");
            int life = MovementMath.StepsFor(cfg.Num("weapons.specials.torch.life"), cfg.Dt);
            for (int i = 0; i < life + 30; i++) r.Step(false, false);
            double perTick = cfg.Num("weapons.specials.torch.dps") * cfg.Num("weapons.specials.torch.tick") * r.loadout.TuneDamage("torch"); // x the retune's knob
            double got = r.DamageOn(inWall);
            Assert.That(got, Is.GreaterThanOrEqualTo(perTick * 10), $"about 12 ticks of {perTick} in 3 s, got {got}");
            Assert.That(got, Is.LessThanOrEqualTo(perTick * 13 * 1.4));
            Assert.That(r.DamageOn(beside), Is.EqualTo(0), "outside the width");
            r.wall = null;
            r.Step(false, true); // 3.25 s after the first: inside the 4 s cooldown
            Assert.That(r.wall, Is.Null, "the cooldown holds");
            for (int i = 0; i < 120; i++) r.Step(false, false);
            r.Step(false, true);
            Assert.That(r.wall, Is.Not.Null, "after the cooldown: another wall");
        }

        [Test]
        public void WithoutB5_TheTorchFiresNothing()
        {
            var r = new Rig(false);
            Husk near = r.Husk(0.5, -5);
            for (int i = 0; i < 60; i++) r.Step(true, false);
            r.Step(false, true);
            Assert.That(r.Count("flame"), Is.EqualTo(0));
            Assert.That(r.wall, Is.Null);
            Assert.That(r.DamageOn(near), Is.EqualTo(0));
        }

        [Test]
        public void TheTorch_IsAPort_ItsLTIsTheFireWall_NotAZoom()
        {
            var r = new Rig();
            Assert.That(Loadout.IsPort20("torch"), Is.True);
            Assert.That(r.loadout.ZoomGun("torch"), Is.EqualTo(0));
            Assert.That(G20Events.Flame, Is.EqualTo("flame"));
            Assert.That(G20Events.FireWall, Is.EqualTo("fireWall"));
        }
    }
}
