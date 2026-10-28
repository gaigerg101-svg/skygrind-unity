// NIGHT8 U2-17: THE FORGE (survival/forge.js). The web's tests (tests/night6.test.js, THE FORGE (B1)) on forgeMult, then
// the port's: once per gun, the "forge" event, the bonus hook through the loadout's damage (a punch counts as the
// GAUNTLET), the seeded status stream, the map's cabinet spot.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class ForgeTests
    {
        static GameConfig cfg;
        static ForgeMath.P f;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            f = ForgeMath.P.From(cfg);
        }

        [Test]
        public void AnUnforgedGunIsUntouched_AForgedOneDealsXDamage()
        {
            Assert.That(ForgeMath.Mult(false, false, false, false, f, "kinetic"), Is.EqualTo(1));
            Assert.That(ForgeMath.Mult(false, false, false, true, f, "kinetic"), Is.EqualTo(f.damage));
        }

        [Test]
        public void KeepsRound20TimeToKillAtRound10s()
        {
            double HpMult(int round) => 1 + Math.Max(0, round - cfg.Num("director.rounds.hpFromRound")) * cfg.Num("director.rounds.hpPerRound");
            Assert.That(HpMult(20) / f.damage, Is.EqualTo(HpMult(10)).Within(1e-6));
        }

        [Test]
        public void FamilyBonusesStackOnTheBase()
        {
            Assert.That(ForgeMath.Mult(true, false, false, true, f, "kinetic"), Is.EqualTo(f.damage * f.families["kinetic"].headshot).Within(1e-9));
            Assert.That(ForgeMath.Mult(false, true, false, true, f, "blast"), Is.EqualTo(f.damage * f.families["blast"].blast).Within(1e-9));
            Assert.That(ForgeMath.Mult(false, false, true, true, f, "pierce"), Is.EqualTo(f.damage * f.families["pierce"].tough).Within(1e-9));
            Assert.That(ForgeMath.Mult(false, false, false, true, f, "pierce"), Is.EqualTo(f.damage));
            Assert.That(ForgeMath.Mult(false, false, false, true, f, "impact"), Is.EqualTo(f.damage * f.families["impact"].damage).Within(1e-9));
        }

        [Test]
        public void EveryFamilyHasABonus()
        {
            foreach (var kv in cfg.Obj("weapons.families")) Assert.That(f.families.ContainsKey(kv.Key), Is.True, kv.Key);
        }

        [Test]
        public void OncePerGun_TheForgeEvent_ResetEndsIt()
        {
            var events = new EventBus();
            var forge = new Forge(cfg, events);
            var seen = new List<ForgeEvent>();
            events.On<ForgeEvent>("forge", e => seen.Add(e));
            forge.Reset(9);
            Assert.That(forge.Give("revolver", f.cost), Is.True);
            Assert.That(forge.Give("revolver", f.cost), Is.False, "once per gun");
            Assert.That(forge.Give("punch", f.cost), Is.False, "not a catalog gun");
            Assert.That(forge.IsForged("revolver"), Is.True);
            Assert.That(seen.Count, Is.EqualTo(1));
            Assert.That((seen[0].weapon, seen[0].price, seen[0].free), Is.EqualTo(("revolver", 5000.0, false)));
            forge.Reset(10);
            Assert.That(forge.IsForged("revolver"), Is.False, "a new run forgets it");
            Assert.That(forge.order, Is.Empty);
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Loadout loadout;
            public readonly Husks husks;
            public readonly Forge forge;
            public readonly List<double> dealt = new List<double>();

            public Rig()
            {
                world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
                var mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                var health = new Health(100, events, "player");
                loadout = new Loadout(cfg, events, enemies, world, mover, health, () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
                loadout.arsenal.enabled = false; // NIGHT9: base damage, as the web sandbox measures it (main.js: arsenal off); ArsenalTests covers the arsenal
                husks = new Husks(cfg, events, enemies, world, -30);
                forge = new Forge(cfg, events);
                forge.Reset(3);
                loadout.bonus = (e, w, head) => forge.Mult(w, head, e); // main.js runBonus (MODS out of it here)
                loadout.after = forge.After;
                events.On<HitEvent>("hit", e => dealt.Add(e.damage));
            }

            public Husk Husk() => husks.Spawn(new Vec3(0, 0, -5), new Vec3(0, 0, 0));
        }

        [Test]
        public void TheBonusHook_ThroughTheLoadoutsDamage_PunchCountsAsTheGauntlet()
        {
            var r = new Rig();
            Husk h = r.Husk();
            r.loadout.Damage(h, 1, true, "revolver", new Vec3(0, 1.6, 0));
            r.forge.Give("revolver", f.cost);
            r.loadout.Damage(h, 1, false, "revolver", new Vec3(0, 1.6, 0));
            r.loadout.Damage(h, 1, true, "revolver", new Vec3(0, 1.6, 0));
            r.loadout.Damage(h, 1, false, "punch", new Vec3(0, 1.6, 0));
            r.forge.Give("gauntlet", f.cost);
            r.loadout.Damage(h, 1, false, "punch", new Vec3(0, 1.6, 0));
            r.loadout.Damage(h, 1, false, "shotgun", new Vec3(0, 1.6, 0));
            double k = f.damage, head = f.families["kinetic"].headshot;
            Assert.That(r.dealt.Count, Is.EqualTo(6));
            Assert.That(r.dealt[0], Is.EqualTo(1), "unforged");
            Assert.That(r.dealt[1], Is.EqualTo(k), "a forged KESTREL body shot");
            Assert.That(r.dealt[2], Is.EqualTo(k * head).Within(1e-12), "and its headshot (kinetic)");
            Assert.That(r.dealt[3], Is.EqualTo(1), "a punch before the GAUNTLET is forged");
            Assert.That(r.dealt[4], Is.EqualTo(k), "a punch counts as the forged GAUNTLET");
            Assert.That(r.dealt[5], Is.EqualTo(1), "the THUNDERCLAP isn't forged");
        }

        [Test]
        public void AForgedGunsStatusRolls_DrawTheRunsSeededStream()
        {
            var r = new Rig();
            var statuses = new List<string>();
            r.forge.status = (e, name) => statuses.Add(name);
            Husk h = r.Husk();
            h.hp = h.maxHp = 1e9;
            uint seed = 77;
            r.forge.Reset(seed);
            for (int i = 0; i < 40; i++) r.loadout.Damage(h, 1, false, "shotgun", new Vec3(0, 1.6, 0));
            Assert.That(statuses, Is.Empty, "an unforged THUNDERCLAP rolls nothing");
            r.forge.Give("shotgun", f.cost);
            r.forge.Give("revolver", f.cost);
            for (int i = 0; i < 10; i++) r.loadout.Damage(h, 1, false, "revolver", new Vec3(0, 1.6, 0)); // kinetic: no status, no draw
            for (int i = 0; i < 40; i++) r.loadout.Damage(h, 1, false, "shotgun", new Vec3(0, 1.6, 0));
            var rng = new Mulberry32(seed ^ f.seedSalt);
            int expected = 0;
            for (int i = 0; i < 40; i++) if (rng.Next() < f.families["edge"].chance) expected++;
            Assert.That(statuses.Count, Is.EqualTo(expected));
            Assert.That(expected, Is.GreaterThan(0));
            Assert.That(statuses.TrueForAll(s => s == "stagger"), Is.True);
        }

        [Test]
        public void TheMapsForgeSpot_IsInTheGrinder_OnTheCore()
        {
            var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            Assert.That(map.forgeSpot.HasValue, Is.True);
            PerkSpot s = map.forgeSpot.Value;
            Assert.That((s.pos.x, s.pos.y, s.pos.z, s.core), Is.EqualTo((-22.0, 0.0, -22.0, true)));
            Assert.That(s.yaw, Is.EqualTo(Math.PI / 4).Within(1e-12));
            int zone = ZoneMath.ZoneIndexAt(map.zoneParts, s.pos.x, s.pos.y + 1, s.pos.z);
            Assert.That(zone, Is.GreaterThanOrEqualTo(0));
            TestContext.WriteLine($"forge zone {zone}: {map.zones[zone].id} {map.zones[zone].name}");
        }
    }
}
