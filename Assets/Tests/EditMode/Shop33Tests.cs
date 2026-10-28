// NIGHT33 P (DESIGN33 card P, D-6, row design.n33.shop33; Core/Shop33.cs): the shop's pure rules.
//   1. the catalogue reads eight upgrades in the three slot kinds, priced tierMult x typicalEarn (rounded to priceRound)
//   2. a buy spends the wallet, owns and equips the item; an owned or unaffordable item is refused, nothing changed
//   3. a full slot swaps: equipping another item there takes the lowest-index one off; Sanitize drops unowned / extra equips
//   4. the effects come only from owned AND equipped items; 0 is neutral (nothing changes)
//   5. the wallet after a level = the wallet before + what it banked; the bank is best-of per level (a replay banks only its improvement)
//   6. the profile keeps the wallet, the upgrades and the per-level bests through its JSON; a profile that never banked writes none of the keys
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public sealed class Shop33Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));

        static Shop33Catalogue Cat() => Shop33Catalogue.From(Cfg);

        static int Idx(Shop33Catalogue c, string id) => c.Find(id).index;

        [Test]
        public void TheCatalogue_EightUpgrades_PricedByTier()
        {
            Shop33Catalogue c = Cat();
            Assert.That(c.Count, Is.EqualTo(8));
            Assert.That(c.SlotSize(Shop33Catalogue.Weapon), Is.EqualTo(2));
            Assert.That(c.SlotSize(Shop33Catalogue.Movement), Is.EqualTo(1));
            Assert.That(c.SlotSize(Shop33Catalogue.Wildcard), Is.EqualTo(1));
            double round = Cfg.Num("design.n33.shop33.priceRound");
            foreach (Shop33Item it in c.items)
            {
                double mult = PortData.Num(Cfg.Arr("design.n33.shop33.tierMult")[it.tier]);
                Assert.That(it.price, Is.EqualTo(Shop33Catalogue.Price(c.typicalEarn, mult, round)), it.id);
                Assert.That(it.price % (int)round, Is.EqualTo(0), it.id);
                Assert.That(it.price, Is.GreaterThan(0), it.id);
            }
            Assert.That(Shop33Catalogue.Price(2650, 1, 50), Is.EqualTo(2650));
            Assert.That(Shop33Catalogue.Price(2683, 2, 50), Is.EqualTo(5350));
            Assert.That(Shop33Catalogue.Price(2683, 4, 100), Is.EqualTo(10700));
            foreach (string id in new[] { "kestrelCharge", "slugPierce", "detonate", "parryWindow", "grappleCharge", "dashCooldown", "maxHealth", "killHeal" })
                Assert.That(c.Find(id), Is.Not.Null, id);
        }

        [Test]
        public void ABuy_SpendsOwnsEquips_AndRefusesTwiceOrUnaffordable()
        {
            Shop33Catalogue c = Cat();
            int hp = Idx(c, "maxHealth");
            int code = 0, wallet = c.items[hp].price + 7;
            Assert.That(c.TryBuy(ref code, ref wallet, hp), Is.True);
            Assert.That(wallet, Is.EqualTo(7));
            Assert.That(Shop33Catalogue.Owns(code, hp) && Shop33Catalogue.IsEquipped(code, hp), Is.True);
            int before = code;
            Assert.That(c.TryBuy(ref code, ref wallet, hp), Is.False, "owned already");
            int heal = Idx(c, "killHeal");
            Assert.That(c.TryBuy(ref code, ref wallet, heal), Is.False, "7 points");
            Assert.That(code, Is.EqualTo(before));
            Assert.That(wallet, Is.EqualTo(7));
        }

        [Test]
        public void AFullSlot_Swaps_AndSanitizeDropsBadEquips()
        {
            Shop33Catalogue c = Cat();
            int g = Idx(c, "grappleCharge"), d = Idx(c, "dashCooldown");
            int code = 0, wallet = 1000000;
            c.TryBuy(ref code, ref wallet, g);
            c.TryBuy(ref code, ref wallet, d);
            Assert.That(Shop33Catalogue.Active(code, d), Is.True, "the new one is on");
            Assert.That(Shop33Catalogue.Active(code, g), Is.False, "the movement slot holds one");
            Assert.That(Shop33Catalogue.Owns(code, g), Is.True, "still owned");
            code = c.Toggle(code, g);
            Assert.That(Shop33Catalogue.Active(code, g) && !Shop33Catalogue.Active(code, d), Is.True, "A on the other swaps back");
            code = c.Toggle(code, g);
            Assert.That(c.EquippedIn(code, Shop33Catalogue.Movement), Is.EqualTo(0), "A on an equipped one takes it off");
            // weapon slot: two
            int k = Idx(c, "kestrelCharge"), s = Idx(c, "slugPierce"), n = Idx(c, "detonate");
            c.TryBuy(ref code, ref wallet, k);
            c.TryBuy(ref code, ref wallet, s);
            c.TryBuy(ref code, ref wallet, n);
            Assert.That(c.EquippedIn(code, Shop33Catalogue.Weapon), Is.EqualTo(2));
            Assert.That(Shop33Catalogue.Active(code, n), Is.True);
            // hand-made bad codes: equipped but not owned, three weapons equipped, bits past the catalogue
            int bad = (1 << (8 + k)) | 0xFF | (0xFF << 8) | (1 << 20);
            int clean = c.Sanitize(bad);
            Assert.That(c.EquippedIn(clean, Shop33Catalogue.Weapon), Is.EqualTo(2));
            Assert.That(c.EquippedIn(clean, Shop33Catalogue.Movement), Is.EqualTo(1));
            Assert.That(c.EquippedIn(clean, Shop33Catalogue.Wildcard), Is.EqualTo(1));
            Assert.That(clean >> 16, Is.EqualTo(0));
            Assert.That(c.Sanitize(1 << (8 + k)), Is.EqualTo(0), "equipped, not owned: dropped");
            Assert.That(c.Sanitize(clean), Is.EqualTo(clean), "idempotent");
        }

        [Test]
        public void TheEffects_OnlyOwnedAndEquipped_AndZeroIsNeutral()
        {
            Shop33Catalogue c = Cat();
            Assert.That(c.Effects(0).Any, Is.False);
            int all = 0, wallet = 1000000;
            for (int i = 0; i < c.Count; i++) c.TryBuy(ref all, ref wallet, i);
            // owned, nothing equipped: neutral
            Assert.That(c.Effects(all & 0xFF).Any, Is.False);
            foreach (Shop33Item it in c.items)
            {
                int one = (1 << it.index) | (1 << (8 + it.index));
                Shop33Effects e = c.Effects(one);
                Assert.That(e.Any, Is.True, it.id);
                switch (it.id)
                {
                    case "kestrelCharge": Assert.That(e.kestrelChargeMult, Is.EqualTo(it.value)); Assert.That(e.kestrelChargeMult, Is.LessThan(1)); break;
                    case "slugPierce": Assert.That(e.slugPierce, Is.EqualTo(1)); break;
                    case "detonate": Assert.That(e.detonateMult, Is.EqualTo(it.value)); Assert.That(e.detonateMult, Is.GreaterThan(1)); break;
                    case "parryWindow": Assert.That(e.parryWindowMult, Is.EqualTo(it.value)); Assert.That(e.parryWindowMult, Is.GreaterThan(1)); break;
                    case "grappleCharge": Assert.That(e.grappleCharges, Is.EqualTo(1)); break;
                    case "dashCooldown": Assert.That(e.dashRecoverMult, Is.EqualTo(it.value)); Assert.That(e.dashRecoverMult, Is.LessThan(1)); break;
                    case "maxHealth": Assert.That(e.maxHealth, Is.EqualTo(20)); break;
                    case "killHeal": Assert.That(e.killHeal, Is.EqualTo(5)); break;
                }
                Assert.That(c.Effects(1 << it.index).Any, Is.False, it.id + " owned only");
            }
        }

        [Test]
        public void TheWallet_AfterALevel()
        {
            Assert.That(Shop33.WalletAfter(0, 2683.4), Is.EqualTo(2683));
            Assert.That(Shop33.WalletAfter(1000, 2682.5), Is.EqualTo(3683));
            Assert.That(Shop33.WalletAfter(-5, -10), Is.EqualTo(0));
        }

        [Test]
        public void TheBank_IsBestOfPerLevel_AReplayNeverEarnsTwice()
        {
            var p = new Profile { bank33 = 100 };
            Assert.That(Shop33.Bank(p, "n32-foundry", 2683.4), Is.EqualTo(2683), "the first clear banks it all");
            Assert.That(p.bank33, Is.EqualTo(2783));
            Assert.That(Shop33.Bank(p, "n32-foundry", 2500), Is.EqualTo(0), "a worse replay banks nothing");
            Assert.That(Shop33.Bank(p, "n32-foundry", 2683), Is.EqualTo(0), "an equal replay banks nothing");
            Assert.That(Shop33.Bank(p, "n32-foundry", 3000), Is.EqualTo(317), "a better replay banks its improvement");
            Assert.That(p.bank33, Is.EqualTo(3100));
            Assert.That(p.banked33["n32-foundry"], Is.EqualTo(3000));
            Assert.That(Shop33.Bank(p, "n32-iceline", 3869), Is.EqualTo(3869), "another level is its own");
            Assert.That(p.bank33, Is.EqualTo(6969));
            Assert.That(Shop33.Bank(p, "", 500), Is.EqualTo(0));
            Assert.That(Shop33.Bank(null, "x", 500), Is.EqualTo(0));
        }

        [Test]
        public void TheProfile_KeepsWalletAndUpgrades()
        {
            var p = new Profile();
            string empty = p.ToJson();
            Assert.That(empty.Contains("bank33"), Is.False);
            Assert.That(empty.Contains("shop33"), Is.False);
            Assert.That(empty.Contains("banked33"), Is.False);
            p.bank33 = 4321;
            p.shop33 = 0b0000_0101_0000_0111;
            p.banked33["n32-kiln"] = 3751;
            Profile back = Profile.Read(p.ToJson());
            Assert.That(back.bank33, Is.EqualTo(4321));
            Assert.That(back.shop33, Is.EqualTo(0b0000_0101_0000_0111));
            Assert.That(back.banked33["n32-kiln"], Is.EqualTo(3751));
            var copy = new Profile();
            copy.CopyFrom(back);
            Assert.That(copy.bank33, Is.EqualTo(4321));
            Assert.That(copy.shop33, Is.EqualTo(back.shop33));
            Assert.That(copy.banked33["n32-kiln"], Is.EqualTo(3751));
            Assert.That(Profile.Read(empty).bank33, Is.EqualTo(0));
        }
    }
}
