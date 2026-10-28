// NIGHT12 U3-25.10: finishes and MASTERY (Core/Mastery.cs). First the web's tests of finishes.js (tests/finishes.test.js:
// the medal counts, FACTORY as the default, a locked choice drawn as FACTORY, the GUN FINISH row, no enemy red-orange or
// RIVAL magenta tone; its model test is a view's: BlockyKit) and night6.test.js's THE PROTOCOL finish; then the port's:
// the port flag "finish.<id>" opening a medal finish, a track per gun the port fires, tier progress from kill events (FRESH
// or air, S rank), a tier waiting for the ones before it, the finish a tier unlocks returned by FinishFor / Choose, a
// non-recording run changing nothing, Reload after an in-place restore.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public sealed class MasteryTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        }

        // --- finishes.js (tests/finishes.test.js, night6.test.js) ----------------------------------------------------------

        /// <summary>Bests that earn `medals` on the first trials (in list order), nothing on the rest.</summary>
        static Func<string, double?> Bests(params string[] medals)
        {
            var output = new Dictionary<string, double>();
            List<object> list = cfg.Arr("trials.list");
            Finishes.MedalTimes ratios = Finishes.Ratios(cfg);
            for (int i = 0; i < list.Count && i < medals.Length; i++)
            {
                var def = PortData.Obj(list[i]);
                output[(string)def["id"]] = Finishes.MedalTimesOf(PortData.Num(def["par"]), ratios).Of(medals[i]);
            }
            return id => output.TryGetValue(id, out double t) ? t : (double?)null;
        }

        static readonly Func<string, double?> None = _ => null;

        [Test]
        public void Web_CountsEachMedalOrBetterPerTrial()
        {
            Finishes.Counts c = Finishes.MedalCounts(cfg, Bests("gold", "bronze", "silver"));
            Assert.That((c.bronze, c.silver, c.gold, c.ace), Is.EqualTo((3, 2, 1, 0)));
            c = Finishes.MedalCounts(cfg, None);
            Assert.That((c.bronze, c.silver, c.gold, c.ace), Is.EqualTo((0, 0, 0, 0)));
        }

        [Test]
        public void Web_FactoryIsAlwaysThereAndTheDefault_TheRestUnlockWithMedals()
        {
            List<string> order = Finishes.Order(cfg);
            Assert.That(order[0], Is.EqualTo("factory"));
            Assert.That(cfg.Str("settings.gunFinish"), Is.EqualTo("factory"));
            Finishes.Counts none = Finishes.MedalCounts(cfg, None);
            Assert.That(order.FindAll(id => Finishes.FinishUnlocked(cfg, id, none)), Is.EqualTo(new[] { "factory" }));
            Finishes.Counts one = Finishes.MedalCounts(cfg, Bests("bronze"));
            Assert.That(Finishes.FinishUnlocked(cfg, "graphite", one), Is.True);
            Assert.That(Finishes.FinishUnlocked(cfg, "arctic", one), Is.False);
            Assert.That(Finishes.FinishNeeds(cfg, "arctic", one), Is.EqualTo("3 SILVER"));
            Finishes.Counts three = Finishes.MedalCounts(cfg, Bests("gold", "gold", "gold"));
            Assert.That(new[] { "graphite", "arctic", "brass" }, Has.All.Matches<string>(id => Finishes.FinishUnlocked(cfg, id, three)));
            Assert.That(Finishes.FinishUnlocked(cfg, "mirror", three), Is.False);
            Assert.That(Finishes.FinishUnlocked(cfg, "mirror", Finishes.MedalCounts(cfg, Bests("ace"))), Is.True);
        }

        [Test]
        public void Web_ALockedChoiceDrawsAsFactory()
        {
            Assert.That(Finishes.ActiveFinish(cfg, "mirror", Finishes.MedalCounts(cfg, None)), Is.EqualTo("factory"));
            Assert.That(Finishes.ActiveFinish(cfg, "graphite", Finishes.MedalCounts(cfg, Bests("bronze"))), Is.EqualTo("graphite"));
            Assert.That(Finishes.ActiveFinish(cfg, "nonsense", Finishes.MedalCounts(cfg, None)), Is.EqualTo("factory"));
        }

        [Test]
        public void Web_TheGunFinishRowListsEveryFinish()
        {
            Dictionary<string, object> row = null;
            foreach (object o in cfg.Arr("ui.settings"))
                if (PortData.Obj(o).TryGetValue("key", out object k) && k as string == "gunFinish") row = PortData.Obj(o);
            Assert.That(row, Is.Not.Null);
            Assert.That(PortData.Arr(row["options"]).ConvertAll(x => (string)x), Is.EqualTo(Finishes.Order(cfg)));
            var labels = PortData.Obj(row["labels"]);
            foreach (string id in Finishes.Order(cfg)) Assert.That((string)labels[id], Is.EqualTo(Finishes.Name(cfg, id)));
        }

        /// <summary>three's Color.getHSL on an sRGB hex: (h 0..1, s 0..1).</summary>
        static (double h, double s) Hsl(uint hex)
        {
            double r = ((hex >> 16) & 255) / 255.0, g = ((hex >> 8) & 255) / 255.0, b = (hex & 255) / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            double l = (min + max) / 2, h = 0, s = 0;
            if (min != max)
            {
                double delta = max - min;
                s = l <= 0.5 ? delta / (max + min) : delta / (2 - max - min);
                if (max == r) h = (g - b) / delta + (g < b ? 6 : 0);
                else if (max == g) h = (b - r) / delta + 2;
                else h = (r - g) / delta + 4;
                h /= 6;
            }
            return (h, s);
        }

        [Test]
        public void Web_NoFinishToneIsTheEnemyRedOrangeOrRivalMagenta()
        {
            double enemy = Hsl((uint)cfg.Num("colors.enemy")).h * 360;
            double rival = Hsl(cfg.Has("bot.color") ? (uint)cfg.Num("bot.color") : 0xff2e88).h * 360;
            foreach (string id in Finishes.Order(cfg))
            {
                Finishes.Palette p = Finishes.PaletteOf(cfg, id);
                if (p == null) continue;
                foreach (uint hex in new[] { p.bone, p.gunmetal, p.dark, p.grip })
                {
                    (double h, double s) = Hsl(hex);
                    foreach (double bad in new[] { enemy, rival })
                    {
                        double gap = Math.Abs(((h * 360 - bad + 540) % 360) - 180);
                        Assert.That(s > 0.5 && gap < 15, Is.False, $"{id} {hex:x}");
                    }
                }
            }
        }

        [Test]
        public void Web_TheProtocolFinish_UnlocksWithTheProtocolFlag_SaysWhatItNeedsOtherwise()
        {
            Finishes.Counts none = Finishes.MedalCounts(cfg, None, _ => false);
            Assert.That(Finishes.FinishUnlocked(cfg, "protocol", none), Is.False);
            Assert.That(Finishes.FinishNeeds(cfg, "protocol", none), Is.EqualTo("COMPLETE THE PROTOCOL"));
            Assert.That(Finishes.FinishUnlocked(cfg, "protocol", Finishes.MedalCounts(cfg, None, f => f == "protocol")), Is.True);
        }

        [Test]
        public void Port_AMedalFinishOpensWithItsPortFlag_TheFlagFinishesDoNot()
        {
            Assert.That(Finishes.PortFlag(cfg, "graphite"), Is.EqualTo("finish.graphite"));
            Assert.That(Finishes.PortFlag(cfg, "protocol"), Is.Null, "PROTOCOL keeps the web's flag");
            Assert.That(Finishes.PortFlag(cfg, "factory"), Is.Null);
            Finishes.Counts flags = Finishes.MedalCounts(cfg, None, f => f == "finish.mirror" || f == "finish.protocol");
            Assert.That(Finishes.FinishUnlocked(cfg, "mirror", flags), Is.True);
            Assert.That(Finishes.FinishUnlocked(cfg, "protocol", flags), Is.False);
            Assert.That(Finishes.OpenedBy(cfg, "skylight", "secretRelay") && Finishes.OpenedBy(cfg, "brass", "finish.brass"), Is.True);
            Assert.That(Finishes.Name(cfg, "brass"), Is.EqualTo("DUSK BRASS"));
            Finishes.Palette g = Finishes.PaletteOf(cfg, "graphite");
            Assert.That((g.bone, g.gunmetal, g.dark, g.grip), Is.EqualTo(((uint)cfg.Num("weapons.finishes.list.graphite.palette.bone"),
                (uint)cfg.Num("weapons.finishes.list.graphite.palette.gunmetal"), (uint)cfg.Num("weapons.finishes.list.graphite.palette.dark"),
                (uint)cfg.Num("weapons.finishes.list.graphite.palette.grip"))));
            Assert.That(Finishes.PaletteOf(cfg, "factory"), Is.Null);
            Assert.That(Finishes.PaletteOf(cfg, "nonsense"), Is.Null);
        }

        // --- MASTERY -----------------------------------------------------------------------------------------------------

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Profile profile = new Profile();
            public readonly Mastery mastery;
            public readonly List<MasteryEvent> tiers = new List<MasteryEvent>();
            public bool air;
            public int rank;
            public double? fresh;

            public Rig()
            {
                mastery = new Mastery(cfg, events, profile, Loadout.Fires) { airborne = () => air, styleRank = () => rank, fresh = _ => fresh };
                events.On<MasteryEvent>(MetaEvents.MasteryTier, e => tiers.Add(e));
            }

            public void Kill(string weapon, int n = 1, string owner = "player")
            {
                for (int i = 0; i < n; i++) events.Emit("kill", new KillEvent { owner = owner, weapon = weapon, kind = "husk" });
            }
        }

        [Test]
        public void EveryGunThePortFires_HasThreeTiersInTheTens_InCatalogOrder()
        {
            var rig = new Rig();
            var expected = new List<string>();
            foreach (var kv in cfg.Obj("weapons.catalog")) if (Loadout.Fires(kv.Key)) expected.Add(kv.Key);
            List<Mastery.Gun> guns = rig.mastery.Guns();
            Assert.That(guns.ConvertAll(g => g.weapon), Is.EqualTo(expected));
            foreach (Mastery.Gun g in guns)
            {
                Assert.That(g.name, Is.EqualTo(cfg.Str($"weapons.catalog.{g.weapon}.name")));
                Assert.That(g.tracks.ConvertAll(t => t.id), Is.EqualTo(new[] { "kills", "fresh", "srank" }));
                Assert.That(g.tracks.ConvertAll(t => t.finish), Is.EqualTo(new[] { "graphite", "arctic", "brass" }));
                foreach (Mastery.Track t in g.tracks) Assert.That(t.target, Is.InRange(10, 99), $"{g.weapon} {t.id}: counts in the tens");
                Assert.That(g.tier, Is.EqualTo(0));
                Assert.That(g.finishes, Is.EqualTo(new[] { "factory" }));
                Assert.That(g.finish, Is.EqualTo("factory"));
            }
            Assert.That(rig.mastery.Of("thumper"), Is.Null, "a gun the port can't fire has no track");
            Assert.That(rig.mastery.SRank, Is.EqualTo(4), "the S letter's index in config.style.ranks");
        }

        [Test]
        public void TierProgressFromKills_TheFinishItUnlocks_FinishForAndChoose()
        {
            var rig = new Rig();
            rig.mastery.StartRun(true);
            Mastery.Gun shotgun = rig.mastery.Of("shotgun");
            double kills = shotgun.tracks[0].target;
            rig.Kill("shotgun", (int)kills - 1);
            Assert.That(shotgun.tier, Is.EqualTo(0));
            Assert.That(shotgun.tracks[0].value, Is.EqualTo(kills - 1));
            Assert.That(rig.tiers, Is.Empty);
            Assert.That(rig.mastery.Choose("shotgun", "graphite"), Is.False, "not unlocked yet");
            rig.Kill("shotgun");
            Assert.That(shotgun.tier, Is.EqualTo(1));
            Assert.That(rig.tiers.Count, Is.EqualTo(1));
            Assert.That((rig.tiers[0].weapon, rig.tiers[0].tier, rig.tiers[0].finish), Is.EqualTo(("shotgun", 1, "graphite")));
            Assert.That(rig.mastery.tierUpsThisRun, Is.EqualTo(new[] { ("shotgun", 1) }));
            Assert.That(rig.profile.Mastery("shotgun", "kills"), Is.EqualTo(kills));
            Assert.That(shotgun.runXp, Is.EqualTo(kills), "no FRESH, air or S-rank kills here");
            Assert.That(rig.mastery.Dirty, Is.True);
            // The finish: unlocked for this gun only, FACTORY until chosen.
            Assert.That(rig.mastery.Of("shotgun").finishes, Is.EqualTo(new[] { "factory", "graphite" }));
            Assert.That(rig.mastery.FinishFor("shotgun"), Is.EqualTo("factory"));
            Assert.That(rig.mastery.Choose("shotgun", "graphite"), Is.True);
            Assert.That(rig.mastery.FinishFor("shotgun"), Is.EqualTo("graphite"));
            Assert.That(rig.mastery.Of("shotgun").finish, Is.EqualTo("graphite"));
            Assert.That(rig.profile.FinishOf("shotgun"), Is.EqualTo("graphite"));
            Assert.That(Finishes.PaletteOf(cfg, rig.mastery.FinishFor("shotgun")), Is.Not.Null);
            Assert.That(rig.mastery.Choose("revolver", "graphite"), Is.False, "the KESTREL has no tier");
            Assert.That(rig.mastery.FinishFor("revolver"), Is.EqualTo("factory"));
            Assert.That(rig.mastery.Needs("revolver", "graphite"), Is.EqualTo($"TIER 1 · {kills.ToString(System.Globalization.CultureInfo.InvariantCulture)} KILLS"));
            Assert.That(rig.mastery.Needs("shotgun", "graphite"), Is.Empty);
            // A punch is the GAUNTLET's kill.
            rig.Kill("punch", 3);
            Assert.That(rig.profile.Mastery("gauntlet", "kills"), Is.EqualTo(3));
            // Another owner's kill, or a secondary source, counts for nothing.
            rig.Kill("rail", 2, "rival");
            rig.Kill("burn", 2);
            Assert.That(rig.profile.Mastery("rail", "kills"), Is.EqualTo(0));
        }

        [Test]
        public void FreshOrAirKills_AndSRankKills_Count_ATierWaitsForTheOnesBeforeIt()
        {
            var rig = new Rig();
            rig.mastery.StartRun(true);
            Mastery.Gun rail = rig.mastery.Of("rail");
            double killsT = rail.tracks[0].target, freshT = rail.tracks[1].target, srankT = rail.tracks[2].target;
            // FRESH by the meter's tier: 1.0 x max >= freshAt counts, 0.5 x max doesn't; air counts; rank S (4) counts, A (3) doesn't.
            rig.fresh = 0.5;
            rig.rank = 3;
            rig.Kill("rail");
            Assert.That((rail.tracks[0].value, rail.tracks[1].value, rail.tracks[2].value), Is.EqualTo((1.0, 0.0, 0.0)));
            rig.fresh = 1;
            rig.Kill("rail");
            rig.fresh = null;
            rig.air = true;
            rig.Kill("rail");
            rig.air = false;
            rig.rank = 4;
            rig.Kill("rail");
            rig.rank = 5;
            rig.Kill("rail");
            Assert.That((rail.tracks[0].value, rail.tracks[1].value, rail.tracks[2].value), Is.EqualTo((5.0, 2.0, 2.0)));
            // S-rank air kills until the S-RANK track is done: the tier still waits for KILLS and FRESH OR AIR.
            rig.air = true;
            rig.Kill("rail", (int)(srankT - 2));
            Assert.That(rail.tracks[2].Done, Is.True);
            Assert.That(rail.tier, Is.EqualTo(rail.tracks[0].Done ? (rail.tracks[1].Done ? 3 : 1) : 0));
            rig.rank = 0;
            while (!rail.tracks[0].Done) rig.Kill("rail");
            Assert.That(rail.tier, Is.EqualTo(rail.tracks[1].Done ? 3 : 1));
            while (!rail.tracks[1].Done) rig.Kill("rail");
            Assert.That(rail.tier, Is.EqualTo(3));
            Assert.That(rig.tiers.ConvertAll(t => (t.tier, t.finish)), Is.EqualTo(new[] { (1, "graphite"), (2, "arctic"), (3, "brass") }));
            Assert.That(rig.mastery.Of("rail").finishes, Is.EqualTo(new[] { "factory", "graphite", "arctic", "brass" }));
            // Capped at the targets; the run's XP is the progress gained.
            rig.Kill("rail", 5);
            Assert.That((rail.tracks[0].value, rail.tracks[1].value, rail.tracks[2].value), Is.EqualTo((killsT, freshT, srankT)));
            Assert.That(rail.runXp, Is.EqualTo(killsT + freshT + srankT));
            Assert.That(rig.tiers.Count, Is.EqualTo(3));
        }

        [Test]
        public void AFlagFinish_OpensForEveryGun()
        {
            var rig = new Rig();
            rig.profile.SetFlag("finish.mirror");
            rig.profile.SetFlag("protocol");
            foreach (Mastery.Gun g in rig.mastery.Guns()) Assert.That(g.finishes, Is.EqualTo(new[] { "factory", "mirror", "protocol" }), g.weapon);
            Assert.That(rig.mastery.Choose("revolver", "mirror"), Is.True, "a menu choice: no run needed");
            Assert.That(rig.mastery.FinishFor("revolver"), Is.EqualTo("mirror"));
            rig.profile.flags.Remove("finish.mirror");
            Assert.That(rig.mastery.FinishFor("revolver"), Is.EqualTo("factory"), "a choice no longer unlocked draws FACTORY");
        }

        [Test]
        public void ANonRecordingRun_ChangesNothing()
        {
            var rig = new Rig();
            string before = rig.profile.ToJson();
            rig.mastery.StartRun(false);
            rig.air = true;
            rig.rank = 5;
            rig.Kill("shotgun", 100);
            Assert.That(rig.profile.ToJson(), Is.EqualTo(before));
            Assert.That(rig.tiers, Is.Empty);
            Assert.That(rig.mastery.Dirty, Is.False);
            Assert.That(rig.mastery.Of("shotgun").runXp, Is.EqualTo(0));
            Assert.That(rig.mastery.tierUpsThisRun, Is.Empty);
        }

        [Test]
        public void StartRun_ClearsTheRun_Reload_RereadsARestoredProfile()
        {
            var rig = new Rig();
            rig.mastery.StartRun(true);
            rig.Kill("nailgun", 30);
            Mastery.Gun nail = rig.mastery.Of("nailgun");
            Assert.That(nail.tier, Is.EqualTo(1));
            rig.mastery.StartRun(true);
            Assert.That(nail.runXp, Is.EqualTo(0));
            Assert.That(rig.mastery.tierUpsThisRun, Is.Empty);
            Assert.That(nail.tier, Is.EqualTo(1), "tiers carry across runs");
            // Restore in place (the smoke's Profile.CopyFrom): the counters back to 0.
            rig.profile.mastery.Clear();
            rig.mastery.Reload();
            Assert.That(nail.tier, Is.EqualTo(0));
            Assert.That(nail.tracks[0].value, Is.EqualTo(0));
            Assert.That(nail.finishes, Is.EqualTo(new[] { "factory" }));
            Assert.That(rig.tiers.Count, Is.EqualTo(1), "Reload emits nothing");
        }
    }
}
