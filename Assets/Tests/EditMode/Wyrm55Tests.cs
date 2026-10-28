// NIGHT56 builder G (G4; design.n55.wyrm55, Core/Enemies/Wyrm55.cs). Pinned:
//   1. the row ships off; its numbers hold ENCOUNTER keys only (Wyrm55Params.Keys: the pool, the chip, the window, the touch cooldown, the
//      phase gates, the barrage's pacing) and each has its _why: no attack's own number (a tell, a speed, a damage, a push, a radius, a count)
//   2. the attacks' own numbers in the config are the shipped ones (WyrmParams from design.n24.wyrm: contact damage, tells, spit / shard
//      count, speed, damage; the row never writes them) and the row's numbers read back as written
// The bot numbers (n47human --boss wyrm --level n52-kiln --seeds 40 --rows wyrm55) are in Research/2026-10-20-wave56/G.md.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Wyrm55Tests
    {
        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Path.GetFullPath("PortExport"), "config.json"));

        [Test]
        public void TheRow_ShipsOff_EncounterKeysOnly_EachWithAWhy()
        {
            GameConfig c = Cfg();
            // NIGHT57 freeze (D-9): wyrm55 ships ON; off unless a freeze turned it on with an "ON ..." reason
            Assert.That(!c.Design("n55.wyrm55.enabled") || (c.Get("design.n55.wyrm55.enabled_why") as string ?? "").StartsWith("ON", StringComparison.Ordinal), Is.True, "wyrm55: off, or ON with its reason");
            var nums = c.Obj("design.n55.wyrm55.numbers");
            int keys = 0;
            foreach (KeyValuePair<string, object> kv in nums)
            {
                if (kv.Key == "_why" || kv.Key.EndsWith("_why", StringComparison.Ordinal)) continue;
                keys++;
                Assert.That(Array.IndexOf(Wyrm55Params.Keys, kv.Key) >= 0, Is.True, kv.Key + " is an encounter key (Wyrm55Params.Keys)");
                Assert.That(nums.ContainsKey(kv.Key + "_why"), Is.True, kv.Key + "_why");
            }
            Assert.That(keys > 0, Is.True, "the row has numbers");
        }

        [Test]
        public void TheAttacks_KeepTheirOwnNumbers()
        {
            GameConfig c = Cfg();
            const string w = "design.n24.wyrm.";
            // the shipped attack numbers (design.n24.wyrm, NIGHT24; the row has no key for any of them)
            Assert.That(c.Num(w + "contact.body"), Is.EqualTo(5.0), "body contact damage");
            Assert.That(c.Num(w + "contact.head"), Is.EqualTo(18.0), "head contact damage");
            Assert.That(c.Num(w + "contact.lunge"), Is.EqualTo(35.0), "lunge contact damage");
            Assert.That(c.Num(w + "spit.count"), Is.EqualTo(10.0), "barrage count");
            Assert.That(c.Num(w + "spit.speed"), Is.EqualTo(16.0), "barrage speed");
            Assert.That(c.Num(w + "spit.damage"), Is.EqualTo(8.0), "barrage damage");
            Assert.That(c.Num(w + "shards.damage"), Is.EqualTo(10.0), "shard damage");
            Assert.That(c.Num(w + "tells.lunge"), Is.EqualTo(0.8), "the lunge's tell");
            foreach (string k in new[] { "contactBody", "contactHead", "contactLunge", "spitCount", "spitSpeed", "spitDamage", "shardDamage", "tells", "speed" })
                Assert.That(Array.IndexOf(Wyrm55Params.Keys, k) < 0, Is.True, k + " is not a key the row may hold");
            Wyrm55Params p = Wyrm55Params.From(c);
            var nums = c.Obj("design.n55.wyrm55.numbers");
            if (nums.ContainsKey("poolScale")) Assert.That(p.poolScale, Is.EqualTo(c.Num("design.n55.wyrm55.numbers.poolScale")));
            if (nums.ContainsKey("spitEvery2")) Assert.That(p.spitEvery2, Is.EqualTo(c.Num("design.n55.wyrm55.numbers.spitEvery2")));
        }
    }
}
