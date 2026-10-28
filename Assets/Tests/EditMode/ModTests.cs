// NIGHT8 U2-13: MODS. From the web's tests/night6.test.js "MODS (B2)": the draft rounds; distinct offers, never a maxed
// mod, two tags at least, the same for the same seed; a reroll leaves the last offer out; rares less often than commons;
// the pick rate; every mod well formed. Plus the run's flow here: a cleared draft round queues a draft of ported,
// unlocked mods; a pick stacks; one paid reroll; SKYDANCER, BLOOD PRICE, INTEREST and BOUNTY HUNTER do what they say.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class ModTests
    {
        static GameConfig cfg;
        static Mods mods;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            mods = new Mods(cfg, new EventBus());
        }

        [Test]
        public void Give_OwnsAModOutsideADraft_CappedAtItsMax_AFreePick()
        {
            // NIGHT12: mods.js give (the DAILY's first mod).
            var events = new EventBus();
            var m = new Mods(cfg, events);
            m.Start(7);
            var picks = new List<ModEvent>();
            events.On<ModEvent>("modPick", e => picks.Add(new ModEvent { id = e.id, stacks = e.stacks, free = e.free }));
            Assert.That(m.Give("skydancer"), Is.True);
            Assert.That(m.N("skydancer"), Is.EqualTo(1));
            Assert.That(picks.Count == 1 && picks[0].free && picks[0].stacks == 1, Is.True);
            int max = m.defs["skydancer"].max;
            Assert.That(m.Give("skydancer", max + 5), Is.True);
            Assert.That(m.N("skydancer"), Is.EqualTo(max));
            Assert.That(m.Give("noSuchMod"), Is.False);
            Assert.That(m.Give(null), Is.False);
        }

        [Test]
        public void Drafts_OnTheListedRounds_ThenEveryFewRounds()
        {
            var m = mods.m;
            foreach (int r in m.draftRounds) Assert.That(ModMath.IsDraftRound(r, m), Is.True, $"round {r}");
            Assert.That(ModMath.IsDraftRound(1, m), Is.False);
            Assert.That(ModMath.IsDraftRound(3, m), Is.False);
            Assert.That(ModMath.IsDraftRound(m.listUntil + m.everyAfter - ((m.listUntil + m.everyAfter) % m.everyAfter), m), Is.True);
        }

        [Test]
        public void Offers_AreDistinct_NeverMaxed_TwoTagsAtLeast_SameForTheSameSeed()
        {
            for (uint seed = 1; seed < 60; seed++)
            {
                var stacks = new Dictionary<string, int> { ["skydancer"] = mods.defs["skydancer"].max };
                var a = ModMath.RollDraft(new Mulberry32(seed).Next, mods.ids, mods.defs, stacks, mods.m);
                var b = ModMath.RollDraft(new Mulberry32(seed).Next, mods.ids, mods.defs, stacks, mods.m);
                Assert.That(a, Is.EqualTo(b));
                Assert.That(new HashSet<string>(a).Count, Is.EqualTo(mods.m.offers));
                Assert.That(a, Does.Not.Contain("skydancer"));
                var tags = new HashSet<string>();
                foreach (string id in a) tags.Add(mods.defs[id].tag);
                Assert.That(tags.Count, Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void AReroll_LeavesTheLastOfferOut_RaresComeUpLessThanCommons()
        {
            var first = ModMath.RollDraft(new Mulberry32(9).Next, mods.ids, mods.defs, new Dictionary<string, int>(), mods.m);
            var again = ModMath.RollDraft(new Mulberry32(10).Next, mods.ids, mods.defs, new Dictionary<string, int>(), mods.m, first);
            foreach (string id in again) Assert.That(first, Does.Not.Contain(id));
            var count = new Dictionary<string, double> { ["common"] = 0, ["rare"] = 0 };
            var rng = new Mulberry32(3);
            for (int i = 0; i < 2000; i++) foreach (string id in ModMath.RollDraft(rng.Next, mods.ids, mods.defs, new Dictionary<string, int>(), mods.m)) count[mods.defs[id].tier]++;
            double Share(string t) => count[t] / mods.ids.FindAll(id => mods.defs[id].tier == t).Count;
            Assert.That(Share("common"), Is.GreaterThan(Share("rare")));
        }

        [Test]
        public void PickRate_CountsOffersAndPicks()
        {
            var s = ModMath.RecordPick(new Dictionary<string, (int, int)>(), new List<string> { "a", "b", "c" }, "b");
            s = ModMath.RecordPick(s, new List<string> { "b", "d", "e" }, "d");
            Assert.That(ModMath.PickRate(s, "b"), Is.EqualTo(0.5));
            Assert.That(ModMath.PickRate(s, "a"), Is.EqualTo(0));
            Assert.That(ModMath.PickRate(s, "z"), Is.Null);
        }

        [Test]
        public void EveryMod_HasANameAKnownTagATierAMaxAndADescription()
        {
            var tags = new HashSet<string>();
            foreach (var kv in cfg.Obj("weapons.families")) tags.Add(kv.Key);
            foreach (var kv in cfg.Obj("mods.styleTags")) tags.Add(kv.Key);
            foreach (ModDef d in mods.defs.Values)
            {
                Assert.That(d.name, Is.Not.Empty, d.id);
                Assert.That(tags.Contains(d.tag), Is.True, $"{d.id} tag {d.tag}");
                Assert.That(mods.m.tierWeight[d.tier], Is.GreaterThan(0), d.id);
                Assert.That(d.max, Is.GreaterThan(0), d.id);
                Assert.That(d.desc, Is.Not.Empty, d.id);
            }
            Assert.That(ModMath.ModLine(mods.defs["skydancer"], 1), Is.EqualTo("Hits while airborne deal +20% damage"));
            Assert.That(ModMath.ModLine(mods.defs["interest"], 2), Is.EqualTo("+10% of your bank at each round clear (up to 1200)"));
        }

        [Test]
        public void ADraftRound_QueuesADraftOfPortedUnlockedMods_APickStacks_OnePaidReroll()
        {
            var events = new EventBus();
            var economy = new Economy(cfg, events);
            economy.Start(5000);
            var health = new Health(100, events, "player");
            bool air = false;
            var md = new Mods(cfg, events) { economy = economy, health = health, airborne = () => air };
            md.Start(77);
            Assert.That(md.Pending, Is.False);
            int round = mods.m.draftRounds[0];
            events.Emit("roundClear", new RoundEvent { number = round });
            Assert.That(md.Pending, Is.True, $"round {round} is a draft round");
            List<string> offer = md.Open();
            Assert.That(offer.Count, Is.EqualTo(md.m.offers));
            foreach (string id in offer)
            {
                Assert.That(Mods.Ported.Contains(id), Is.True, id);
                Assert.That(md.defs[id].lockFlag, Is.Null, $"{id}: a locked mod waits for its flag");
            }
            Assert.That(md.Reroll(), Is.True);
            Assert.That(economy.total, Is.EqualTo(5000 - md.m.rerollCost));
            Assert.That(md.Reroll(), Is.False, "one reroll a run");
            string picked = md.offer[0];
            Assert.That(md.Pick(0), Is.True);
            Assert.That(md.N(picked), Is.EqualTo(1));
            Assert.That(md.Pending, Is.False);

            // SKYDANCER while airborne; BLOOD PRICE heals on a kill; INTEREST pays at a round clear; BOUNTY HUNTER on a tough kind.
            var fx = new Mods(cfg, events) { economy = economy, health = health, airborne = () => air };
            fx.Start(5);
            fx.stacks["skydancer"] = 1;
            fx.stacks["bloodPrice"] = 1;
            fx.stacks["interest"] = 1;
            fx.stacks["bountyHunter"] = 1;
            Assert.That(fx.Mult(null), Is.EqualTo(1));
            air = true;
            Assert.That(fx.Mult(null), Is.EqualTo(1 + fx.defs["skydancer"].per));
            health.Damage(50, "test");
            events.Emit("kill", new KillEvent { kind = "spitter", weapon = "revolver" });
            Assert.That(health.hp, Is.EqualTo(50 + fx.defs["bloodPrice"].per), "BLOOD PRICE");
            double bank = economy.total;
            events.Emit("roundClear", new RoundEvent { number = 1 });
            Assert.That(economy.total - bank, Is.EqualTo(System.Math.Min(fx.defs["interest"].cap, System.Math.Floor(bank * fx.defs["interest"].per))), "INTEREST");
        }
    }
}
