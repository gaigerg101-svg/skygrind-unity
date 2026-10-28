// NIGHT20 U3-34.9 row B3 (builder G; Research/2026-10-06-additions-backlog.md row 11: BO6's Pack-a-Punch 5k / 15k / 30k,
// BO3's alternate ammo types, Hades' aspects): THE FORGE in tiers, as sidegrades (design.adds.b3). FORGE I (prices[0]) is
// NIGHT19's sidegrade forge (the gun's variant, a flat design.forge.mult); FORGE II (prices[1]) adds the gun's family from the
// web's forge.families: a status as a proc on procCooldown s (not a chance), or the family's multiplier (kinetic headshots,
// pierce on tough kinds, blast, impact); FORGE III (prices[2]) adds a finish and a flat finishMult, the same for every gun.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class G20ForgeTiersTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Forge Tiered(EventBus events, bool on = true)
        {
            var f = new Forge(cfg, events);
            f.Reset(1);
            f.n19Sidegrade = true; // FORGE I is NIGHT19's sidegrade
            f.tiered = on;
            return f;
        }

        [Test]
        public void TheTiers_AreInTheDesignConfig()
        {
            Assert.That(cfg.Design("adds.b3.enabled"), Is.True);
            double[] prices = cfg.Nums("design.adds.b3.prices");
            Assert.That(prices.Length, Is.EqualTo(3));
            Assert.That(cfg.Num("design.adds.b3.procCooldown"), Is.GreaterThan(0));
            Assert.That(cfg.Num("design.adds.b3.finishMult"), Is.GreaterThan(1));
        }

        [Test]
        public void AGun_GoesFromFORGEI_ToIII_AtTheTiersPrices_ThenNoMore()
        {
            var events = new EventBus();
            var forged = new List<int>();
            events.On<ForgeEvent>("forge", e => forged.Add(e.tier));
            Forge f = Tiered(events);
            Assert.That(f.Tier("shotgun"), Is.EqualTo(0));
            Assert.That(f.NextPrice("shotgun"), Is.EqualTo(cfg.Nums("design.adds.b3.prices")[0]));
            Assert.That(f.Upgrade("shotgun"), Is.True);
            Assert.That(f.NextPrice("shotgun"), Is.EqualTo(cfg.Nums("design.adds.b3.prices")[1]));
            Assert.That(f.Upgrade("shotgun"), Is.True);
            Assert.That(f.NextPrice("shotgun"), Is.EqualTo(cfg.Nums("design.adds.b3.prices")[2]));
            Assert.That(f.Upgrade("shotgun"), Is.True);
            Assert.That(f.Tier("shotgun"), Is.EqualTo(3));
            Assert.That(f.Upgrade("shotgun"), Is.False, "III is the top");
            Assert.That(f.IsForged("shotgun"), Is.True);
            Assert.That(forged, Is.EqualTo(new List<int> { 1, 2, 3 }), "a forge event per tier");
        }

        [Test]
        public void TheMult_IsFlatAtI_TheFamilysAtII_TheFinishAtIII()
        {
            var f = Tiered(new EventBus());
            double side = cfg.Num("design.forge.mult"), head = cfg.Num("forge.families.kinetic.headshot"), fin = cfg.Num("design.adds.b3.finishMult");
            f.Upgrade("revolver");
            Assert.That(f.Mult("revolver", true, null), Is.EqualTo(side), "I: the flat sidegrade");
            f.Upgrade("revolver");
            Assert.That(f.Mult("revolver", true, null), Is.EqualTo(side * head).Within(1e-12), "II: + the kinetic family's headshot");
            Assert.That(f.Mult("revolver", false, null), Is.EqualTo(side).Within(1e-12), "II: a body shot keeps the flat mult");
            f.Upgrade("revolver");
            Assert.That(f.Mult("revolver", false, null), Is.EqualTo(side * fin).Within(1e-12), "III: + the finish");
        }

        [Test]
        public void AtII_TheFamilysStatus_ProcsOnACooldown_NotAChance()
        {
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
            var husks = new Husks(cfg, events, enemies, world, -30);
            Husk h = (Husk)husks.Spawn("husk", new Vec3(0, 0, -5), new Vec3(0, 0, 0));
            var procs = new List<string>();
            Forge f = Tiered(events);
            f.status = (e, s) => procs.Add(s);
            f.Upgrade("shotgun"); // I: no status (the sidegrade)
            f.now = 0;
            f.After(h, "shotgun");
            Assert.That(procs.Count, Is.EqualTo(0), "FORGE I: no family status");
            f.Upgrade("shotgun"); // II
            f.After(h, "shotgun");
            Assert.That(procs, Is.EqualTo(new List<string> { cfg.Str("forge.families.edge.status") }), "the THUNDERCLAP's edge: STAGGER, every time the cooldown allows");
            f.now = cfg.Num("design.adds.b3.procCooldown") - 0.01;
            f.After(h, "shotgun");
            Assert.That(procs.Count, Is.EqualTo(1), "inside the cooldown: none");
            f.now = cfg.Num("design.adds.b3.procCooldown");
            f.After(h, "shotgun");
            Assert.That(procs.Count, Is.EqualTo(2), "the cooldown ran: again");
        }

        [Test]
        public void Untiered_TheForgeIsNight19s_OneBuyNoUpgrade()
        {
            var f = Tiered(new EventBus(), false);
            Assert.That(f.Upgrade("revolver"), Is.True, "the first forge");
            Assert.That(f.Upgrade("revolver"), Is.False, "untiered: one forge per gun");
            Assert.That(f.Tier("revolver"), Is.EqualTo(1));
            Assert.That(f.Mult("revolver", true, null), Is.EqualTo(cfg.Num("design.forge.mult")));
        }
    }
}
