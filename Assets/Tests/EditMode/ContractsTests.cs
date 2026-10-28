// NIGHT12 U3-25.7: CONTRACTS (Core/Contracts.cs: challenges.js + unlocks.js + design.contracts). The web has no tests for
// challenges.js or unlocks.js (its tests/ import neither), so these are the port's: the board (the web's 24 first, 30-40
// in all, every unlock variety), progress from the port's events, completion emitting once (challengeDone for a web badge,
// contractDone for any), the unlock landing in the profile, a non-recording run changing nothing, Nearest / NextUnlock's
// order, the first unlock fitting rounds 1-2 by config, the end-of-run kinds, main.js's earn triggers with the board off,
// Reload after an in-place restore, and Step allocating nothing.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
#if UNITY_5_3_OR_NEWER
using UnityEngine.TestTools.Constraints; // AllocatingGCMemory() is an extension method here
using AllocIs = UnityEngine.TestTools.Constraints.Is;
using Is = NUnit.Framework.Is;
#endif

namespace Skygrind.Core.Tests
{
    public sealed class ContractsTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Profile profile = new Profile();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Mover mover;
            public readonly List<string> mutators = new List<string>();
            public readonly Contracts contracts;
            public readonly List<ContractEvent> done = new List<ContractEvent>();
            public readonly List<ChallengeEvent> challenges = new List<ChallengeEvent>();
            public readonly List<UnlockEvent> unlocks = new List<UnlockEvent>();

            public Rig(GameConfig c = null)
            {
                c ??= cfg;
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                mover = new Mover(c.movement, world);
                mover.Reset(0, 0, 0);
                contracts = new Contracts(c, events, profile, mover, () => mutators);
                events.On<ContractEvent>(MetaEvents.ContractDone, e => done.Add(e));
                events.On<ChallengeEvent>(MetaEvents.ChallengeDone, e => challenges.Add(e));
                events.On<UnlockEvent>(MetaEvents.UnlockEarned, e => unlocks.Add(e));
            }

            public void Kill(string weapon = "revolver", bool headshot = false, string owner = "player") =>
                events.Emit("kill", new KillEvent { owner = owner, weapon = weapon, kind = "husk", headshot = headshot });

            public Contracts.Row Row(string id) => contracts.rows.Find(r => r.id == id);
        }

        static string GunName(string id) => cfg.Str($"weapons.catalog.{id}.name");

        [Test]
        public void TheBoard_TheWeb24First_30To40InAll_EveryUnlockIsVariety()
        {
            var rig = new Rig();
            List<object> web = cfg.Arr("challenges.list");
            Assert.That(web.Count, Is.EqualTo(24));
            Assert.That(rig.contracts.rows.Count, Is.InRange(30, 40));
            var ids = new HashSet<string>();
            for (int i = 0; i < rig.contracts.rows.Count; i++)
            {
                Contracts.Row r = rig.contracts.rows[i];
                Assert.That(ids.Add(r.id), Is.True, $"{r.id} twice");
                Assert.That(r.order, Is.EqualTo(i));
                Assert.That(r.target, Is.GreaterThan(0), r.id);
                Assert.That(string.IsNullOrEmpty(r.name) || string.IsNullOrEmpty(r.desc), Is.False, r.id);
                if (i < 24)
                {
                    Assert.That(r.web && r.id == (string)PortData.Obj(web[i])["id"], Is.True, $"row {i} is the web's {i}th badge");
                    continue;
                }
                Assert.That(r.web, Is.False, r.id);
                Assert.That(Array.IndexOf(Contracts.Kinds, r.kind), Is.GreaterThanOrEqualTo(0), r.id);
                if (r.unlock == null) continue;
                // Variety only: a finish, a locked MOD's flag or the DAILY; never a HEAT rung (Heat opens those).
                Assert.That(r.unlockName, Is.Not.Null.And.Not.Empty, $"{r.id}: '{r.unlock}' opens nothing known");
                Assert.That(r.unlock.StartsWith("heat", StringComparison.OrdinalIgnoreCase), Is.False, r.id);
            }
            // Every locked MOD of the web can be unlocked in the port (by main.js's own trigger or a contract).
            foreach (var kv in cfg.Obj("mods.list"))
            {
                var d = PortData.Obj(kv.Value);
                if (!d.TryGetValue("locked", out object l)) continue;
                string flag = (string)PortData.Obj(l)["flag"];
                Assert.That(rig.contracts.Grants(flag), Is.Not.Null, $"{kv.Key}: no contract grants '{flag}'");
            }
            Assert.That(rig.Row("firstFifteen").unlockName, Is.EqualTo("MOD: INTEREST")); // NIGHT24: the DAILY is gone; the flag "daily" is only the id of this unlock
            Assert.That(rig.Row("protocol").unlockName, Is.EqualTo("FINISH: PROTOCOL · MOD: WALLBANGER"));
            Assert.That(rig.Row("round10").unlockName, Is.EqualTo("MOD: STYLE TAX"));
            Assert.That(rig.Row("whiteHot").unlockName, Is.EqualTo("FINISH: MIRROR"));
        }

        [Test]
        public void ProgressFromKills_CompletionEmitsOnce_TheUnlockLandsInTheProfile()
        {
            var rig = new Rig();
            rig.contracts.StartRun(true);
            for (int i = 0; i < 14; i++) rig.Kill();
            Contracts.Row first = rig.Row("firstFifteen");
            Assert.That(first.value, Is.EqualTo(14));
            Assert.That(first.done, Is.False);
            Assert.That(rig.done, Is.Empty);
            rig.Kill("shotgun");
            Assert.That(first.done, Is.True);
            Assert.That(rig.done.Count, Is.EqualTo(1));
            ContractEvent e = rig.done[0];
            Assert.That((e.id, e.name, e.unlock, e.unlockName, e.web), Is.EqualTo(("firstFifteen", "FIRST FIFTEEN", "daily", "MOD: INTEREST", false)));
            Assert.That(rig.challenges, Is.Empty, "a port-only contract is no challengeDone");
            Assert.That(rig.unlocks.Count, Is.EqualTo(1));
            Assert.That((rig.unlocks[0].flag, rig.unlocks[0].name), Is.EqualTo(("daily", "MOD: INTEREST")));
            Assert.That(rig.contracts.doneThisRun, Is.EqualTo(new[] { "firstFifteen" }));
            // The profile holds it (and a save would keep it).
            Assert.That(rig.profile.Flag("daily") && rig.contracts.Unlocked("daily"), Is.True);
            Assert.That(rig.profile.Contract("firstFifteen").done, Is.True);
            Assert.That(rig.contracts.Dirty, Is.True);
            Profile back = Profile.Read(rig.profile.ToJson());
            Assert.That(back.Flag("daily") && back.Contract("firstFifteen").done && back.Contract("firstFifteen").v == 15, Is.True);
            // More kills: capped at the target, no second completion, no second unlock.
            for (int i = 0; i < 5; i++) rig.Kill();
            Assert.That(first.value, Is.EqualTo(15));
            Assert.That(rig.done.Count, Is.EqualTo(1));
            Assert.That(rig.unlocks.Count, Is.EqualTo(1));
            Assert.That(rig.contracts.Earn("daily"), Is.False, "earned once");
            // Another owner's kill counts for nothing.
            double v = rig.Row("fiveHundred").value;
            rig.Kill(owner: "rival");
            Assert.That(rig.Row("fiveHundred").value, Is.EqualTo(v));
        }

        [Test]
        public void AWebBadge_EmitsChallengeDoneAndContractDone_Round10EarnsItsFlagOnce()
        {
            var rig = new Rig();
            rig.contracts.StartRun(true);
            for (int n = 1; n <= 9; n++) rig.events.Emit("roundBegin", new RoundEvent { number = n });
            Assert.That(rig.Row("round10").value, Is.EqualTo(9));
            Assert.That(rig.Row("fiveDeep").done, Is.True);
            Assert.That(rig.profile.Flag("monolith"), Is.True, "FIVE DEEP grants SHRAPNEL's flag");
            rig.done.Clear();
            rig.unlocks.Clear();
            rig.events.Emit("roundBegin", new RoundEvent { number = 10 });
            Assert.That(rig.challenges.Count, Is.EqualTo(1));
            Assert.That((rig.challenges[0].id, rig.challenges[0].name), Is.EqualTo(("round10", "TEN DOWN")));
            Assert.That(rig.done.Count, Is.EqualTo(1));
            Assert.That((rig.done[0].id, rig.done[0].web, rig.done[0].unlock), Is.EqualTo(("round10", true, "round10")));
            Assert.That(rig.unlocks.Count, Is.EqualTo(1), "main.js's trigger and the badge earn the flag once");
            Assert.That(rig.unlocks[0].flag, Is.EqualTo("round10"));
            Assert.That(rig.Row("round20").value, Is.EqualTo(10));
            // A web badge from another event: HEADHUNTER (100 headshot kills).
            rig.challenges.Clear();
            for (int i = 0; i < 100; i++) rig.Kill(headshot: true);
            Assert.That(rig.challenges.Count, Is.EqualTo(1));
            Assert.That(rig.challenges[0].id, Is.EqualTo("headhunter"));
        }

        [Test]
        public void ANonRecordingRun_ChangesNothing()
        {
            var rig = new Rig();
            rig.profile.Contract("firstFifteen").v = 3;
            string before = rig.profile.ToJson();
            rig.contracts.Reload();
            rig.contracts.StartRun(false);
            for (int i = 0; i < 40; i++) rig.Kill(headshot: true);
            for (int n = 1; n <= 12; n++) rig.events.Emit("roundBegin", new RoundEvent { number = n });
            foreach (string w in new[] { "revolver", "shotgun", "rail" }) rig.events.Emit("forge", new ForgeEvent { weapon = w });
            rig.events.Emit("protocolComplete", new ProtocolCompleteEvent());
            rig.events.Emit("secretRelay", new SecretRelayEvent());
            rig.events.Emit("perk", new PerkEvent { id = "steady" });
            rig.events.Emit("buy", new SpentEvent { what = GunName("shotgun") });
            rig.events.Emit("boxOpen", new BoxOpenEvent());
            rig.events.Emit("powerOn", new PowerEvent());
            rig.events.Emit("gateOpen", new GateOpenEvent());
            rig.events.Emit("trick", "wallJump");
            rig.events.Emit("styleAward", new StyleAwardEvent { name = "IMPACT" });
            rig.events.Emit("trapKill", null);
            rig.events.Emit("powerUpGrab", new PowerUpEvent());
            rig.events.Emit("rankChange", 5);
            rig.mover.velocity = new Vec3(40, 0, 0);
            rig.contracts.Step(cfg.Dt);
            var stats = new RunStats { round = 12, exfil = Exfil.Win, heat = 3, grade = "S" };
            rig.contracts.EndRun(stats);
            Assert.That(rig.profile.ToJson(), Is.EqualTo(before));
            Assert.That(rig.done, Is.Empty);
            Assert.That(rig.unlocks, Is.Empty);
            Assert.That(rig.contracts.Dirty, Is.False);
            Assert.That(rig.contracts.doneThisRun, Is.Empty);
            Assert.That(stats.contractsDone, Is.Empty);
            Assert.That(rig.contracts.Earn("daily"), Is.False);
            Assert.That(rig.Row("firstFifteen").value, Is.EqualTo(3));
        }

        [Test]
        public void Nearest_HighestFractionFirst_TiesInDisplayOrder_NextUnlockIsTheClosestLock()
        {
            var rig = new Rig();
            rig.contracts.StartRun(true);
            rig.contracts.DebugSet("fiveHundred", 250); // 0.5, unlocks GRAPHITE
            rig.contracts.DebugSet("offTheWall", 4); // 0.8, no unlock
            rig.contracts.DebugSet("lowRider", 5); // 0.2
            rig.contracts.DebugSet("firstFifteen", 3); // 0.2, earlier in display order
            List<Contracts.Row> near = rig.contracts.Nearest(3);
            Assert.That(near.ConvertAll(r => r.id), Is.EqualTo(new[] { "offTheWall", "fiveHundred", "firstFifteen" }));
            Assert.That(rig.contracts.Nearest(4)[3].id, Is.EqualTo("lowRider"));
            // Dormant badges (no event in this build) never fill the strip; done ones neither.
            foreach (Contracts.Row r in rig.contracts.Nearest(100))
            {
                Assert.That(r.live && !r.done, Is.True, r.id);
                Assert.That(Contracts.Dormant.ContainsKey(r.id) && r.value == 0, Is.False, r.id);
            }
            Assert.That(rig.contracts.NextUnlock().id, Is.EqualTo("fiveHundred"));
            rig.contracts.Earn("finish.graphite"); // opened another way: no longer the next unlock
            Assert.That(rig.contracts.NextUnlock().id, Is.EqualTo("firstFifteen"));
            Assert.That(rig.contracts.Nearest(0), Is.Empty);
        }

        [Test]
        public void TheFirstUnlock_IsAKillsContract_ThatRounds1And2Fill()
        {
            var rig = new Rig();
            Assert.That(rig.contracts.FirstUnlockId, Is.EqualTo("firstFifteen"));
            Contracts.Row first = rig.Row(rig.contracts.FirstUnlockId);
            Assert.That(first.kind, Is.EqualTo("kills"));
            Assert.That(first.unlock == "daily" || first.unlock.StartsWith(Finishes.FlagPrefix, StringComparison.Ordinal), Is.True, first.unlock);
            DirectorMath.D d = DirectorMath.From(cfg);
            Assert.That(DirectorMath.RoundType(1, d), Is.EqualTo("normal"));
            Assert.That(DirectorMath.RoundType(2, d), Is.EqualTo("normal"));
            double rounds12 = DirectorMath.RoundBudget(1, d) + DirectorMath.RoundBudget(2, d);
            TestContext.WriteLine($"rounds 1-2 send {rounds12} enemies; the first unlock needs {first.target} kills");
            Assert.That(first.target, Is.LessThanOrEqualTo(rounds12));
            // It is the first unlock on the board's cadence: the cheapest contract with an unlock.
            foreach (Contracts.Row r in rig.contracts.rows)
                if (!r.web && r.unlock != null && r.kind == "kills") Assert.That(r.target, Is.GreaterThanOrEqualTo(first.target), r.id);
        }

        [Test]
        public void EndRun_JudgesExfil_OnAHeatRung_AndTheGrade()
        {
            var rig = new Rig();
            rig.contracts.StartRun(true);
            var lost = new RunStats { round = 8, exfil = null, heat = 3, grade = "A" };
            rig.contracts.EndRun(lost);
            Assert.That(rig.done, Is.Empty);
            rig.contracts.StartRun(true);
            var heat2 = new RunStats { round = 11, exfil = Exfil.Win, heat = 2, grade = "B" };
            rig.contracts.EndRun(heat2);
            Assert.That(rig.Row("wayOut").done, Is.True);
            Assert.That(rig.Row("whiteHot").done, Is.False, "an exfil on HEAT 2 isn't HEAT 3");
            Assert.That(rig.Row("topMarks").done, Is.False);
            Assert.That(rig.profile.Flag("finish.brass"), Is.True);
            Assert.That(heat2.contractsDone, Is.EqualTo(new[] { "wayOut" }));
            rig.contracts.StartRun(true);
            var heat3 = new RunStats { round = 21, exfil = Exfil.Win, heat = 3, grade = "S" };
            rig.contracts.EndRun(heat3);
            Assert.That(heat3.contractsDone, Is.EqualTo(new[] { "whiteHot", "topMarks" }));
            Assert.That(rig.profile.Flag("finish.mirror") && rig.profile.Flag("finish.arctic"), Is.True);
            rig.contracts.StartRun(true);
            rig.contracts.EndRun(new RunStats { exfil = Exfil.Lost, grade = "P" });
        }

        [Test]
        public void TheRunKinds_AndTheEventKinds_CountFromThePortsEvents()
        {
            var rig = new Rig();
            rig.contracts.StartRun(true);
            // Wall buys: a buy of a gun's catalog name (the box's TAKE or MYSTERY BOX isn't one).
            rig.events.Emit("buy", new SpentEvent { what = GunName("shotgun") });
            rig.events.Emit("buy", new SpentEvent { what = "MYSTERY BOX" });
            rig.events.Emit("buy", new SpentEvent { what = "TAKE" });
            Assert.That(rig.Row("offTheWall").value, Is.EqualTo(1));
            Assert.That(rig.Row("arsenal").value, Is.EqualTo(1));
            // Forge 3 guns in one run: THREE FORGED and main.js's forge3, once.
            rig.events.Emit("forge", new ForgeEvent { weapon = "revolver" });
            rig.events.Emit("forge", new ForgeEvent { weapon = "shotgun" });
            Assert.That(rig.profile.Flag("forge3"), Is.False);
            rig.events.Emit("forge", new ForgeEvent { weapon = "rail" });
            Assert.That(rig.Row("threeForged").done && rig.profile.Flag("forge3"), Is.True);
            Assert.That(rig.unlocks.FindAll(u => u.flag == "forge3").Count, Is.EqualTo(1));
            // Per-run counters start over (ARSENAL: 10 in ONE run).
            rig.contracts.StartRun(true);
            rig.events.Emit("buy", new SpentEvent { what = GunName("rail") });
            Assert.That(rig.Row("arsenal").value, Is.EqualTo(1), "a new run's counter starts at 0; the value only rises");
            Assert.That(rig.Row("offTheWall").value, Is.EqualTo(2), "across runs");
            for (int i = 0; i < 20; i++) rig.events.Emit("trick", "wallJump");
            Assert.That(rig.Row("wallRider").done, Is.True);
            rig.events.Emit("trick", "dashJump");
            // The rest of the port-only kinds.
            for (int i = 0; i < 10; i++) rig.events.Emit("boxOpen", new BoxOpenEvent());
            Assert.That(rig.Row("boxRegular").done && rig.profile.Flag("trialGold"), Is.True);
            rig.events.Emit("powerOn", new PowerEvent());
            Assert.That(rig.Row("powerOn").done, Is.True);
            rig.events.Emit("protocolComplete", new ProtocolCompleteEvent());
            Assert.That(rig.Row("protocol").done && rig.profile.Flag("protocol"), Is.True);
            rig.events.Emit("secretRelay", new SecretRelayEvent());
            Assert.That(rig.Row("hiddenRelay").done && rig.profile.Flag("secretRelay"), Is.True);
            bool slide = false, air = false;
            rig.contracts.sliding = () => slide;
            rig.contracts.airborne = () => air;
            rig.Kill();
            Assert.That(rig.Row("lowRider").value + rig.Row("airstrike").value, Is.EqualTo(0));
            slide = true;
            rig.Kill();
            slide = false;
            air = true;
            rig.Kill();
            rig.Kill();
            Assert.That((rig.Row("lowRider").value, rig.Row("airstrike").value), Is.EqualTo((1.0, 2.0)));
            // Web badges on other events.
            rig.events.Emit("styleAward", new StyleAwardEvent { name = "IMPACT" });
            rig.events.Emit("styleAward", new StyleAwardEvent { name = "DOUBLE" });
            rig.events.Emit("trapKill", null);
            rig.events.Emit("powerUpGrab", new PowerUpEvent());
            rig.events.Emit("perk", new PerkEvent { id = "steady" });
            Assert.That((rig.Row("impacts").value, rig.Row("trapper").value, rig.Row("lucky").value, rig.Row("perkHoarder").value), Is.EqualTo((1.0, 1.0, 1.0, 1.0)));
            rig.mutators.AddRange(new[] { "lowGravity", "noRegen" });
            rig.events.Emit("roundBegin", new RoundEvent { number = 5 });
            Assert.That(rig.Row("mutated").done, Is.True);
        }

        [Test]
        public void Unbroken_RisesWithTheRank_UntilARevive()
        {
            var rig = new Rig();
            rig.contracts.StartRun(true);
            rig.events.Emit("rankChange", 3);
            rig.events.Emit("rankChange", 2);
            Assert.That(rig.Row("unbroken").value, Is.EqualTo(3));
            rig.events.Emit("phoenix", null);
            rig.events.Emit("rankChange", 5);
            Assert.That(rig.Row("unbroken").value, Is.EqualTo(3));
            rig.contracts.StartRun(true); // a new run forgets the revive
            rig.events.Emit("rankChange", 5);
            Assert.That(rig.Row("unbroken").done, Is.True);
        }

        [Test]
        public void Step_TopSpeed_AndTheSwingDistance()
        {
            var rig = new Rig();
            rig.contracts.StartRun(true);
            rig.mover.velocity = new Vec3(18, 0, 24); // 30 m/s across the ground
            rig.contracts.Step(cfg.Dt);
            Assert.That(rig.Row("speed").done, Is.True);
            Assert.That(rig.challenges.Exists(c => c.id == "speed"), Is.True);
            Assert.That(rig.Row("skyWalker").value, Is.EqualTo(0), "no rope, on the ground");
        }

        [Test]
        public void MainJsEarnTriggers_WorkWithTheBoardOff()
        {
            GameConfig off = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            PortData.Obj(PortData.Obj(off.root["design"])["contracts"])["enabled"] = false;
            var rig = new Rig(off);
            Assert.That(rig.contracts.rows.Count, Is.EqualTo(24), "only the web's 24");
            Assert.That(rig.contracts.FirstUnlockId, Is.Null);
            Assert.That(rig.Row("round10").unlock, Is.Null);
            rig.contracts.StartRun(true);
            rig.events.Emit("roundBegin", new RoundEvent { number = 10 });
            foreach (string w in new[] { "revolver", "shotgun", "rail" }) rig.events.Emit("forge", new ForgeEvent { weapon = w });
            rig.events.Emit("protocolComplete", new ProtocolCompleteEvent());
            rig.events.Emit("secretRelay", new SecretRelayEvent());
            rig.contracts.EndRun(new RunStats());
            foreach (string f in new[] { "round10", "forge3", "protocol", "secretRelay" }) Assert.That(rig.profile.Flag(f), Is.True, f);
            Assert.That(rig.unlocks.Count, Is.EqualTo(4), "NIGHT24: the DAILY's main.js earn is gone");
            Assert.That(rig.unlocks.Find(u => u.flag == "protocol").name, Is.EqualTo("FINISH: PROTOCOL · MOD: WALLBANGER"));
        }

        [Test]
        public void TheKindsTheListDoesNotUseYet_CountToo_AnUnknownKindFailsTheLoad()
        {
            GameConfig more = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            List<object> list = PortData.Arr(PortData.Obj(PortData.Obj(more.root["design"])["contracts"])["list"]);
            Dictionary<string, object> C(string id, string kind, double target, string weapon = null)
            {
                var d = new Dictionary<string, object> { ["id"] = id, ["name"] = id.ToUpperInvariant(), ["desc"] = id, ["kind"] = kind, ["target"] = target };
                if (weapon != null) d["weapon"] = weapon;
                return d;
            }
            list.Add(C("heads", "headshots", 2));
            list.Add(C("punches", "weaponKills", 2, "gauntlet"));
            list.Add(C("doors", "gates", 2));
            list.Add(C("machines", "perks", 2));
            var rig = new Rig(more);
            rig.contracts.StartRun(true);
            rig.Kill("punch", headshot: true);
            rig.Kill("shotgun");
            rig.Kill("orb");
            rig.events.Emit("gateOpen", new GateOpenEvent { zone = "armory" });
            rig.events.Emit("perk", new PerkEvent { id = "steady" });
            rig.events.Emit("perk", new PerkEvent { id = "quick", free = true });
            Assert.That((rig.Row("heads").value, rig.Row("punches").value, rig.Row("doors").value, rig.Row("machines").value), Is.EqualTo((1.0, 2.0, 1.0, 2.0)));
            Assert.That(rig.Row("punches").done && rig.Row("machines").done, Is.True);
            list.Add(C("typo", "kils", 1));
            Assert.Throws<InvalidDataException>(() => new Rig(more));
        }

        [Test]
        public void Reload_RereadsAProfileRestoredInPlace()
        {
            var rig = new Rig();
            rig.contracts.StartRun(true);
            for (int i = 0; i < 5; i++) rig.Kill();
            string snapshot = rig.profile.ToJson();
            for (int i = 0; i < 20; i++) rig.Kill();
            Assert.That(rig.Row("firstFifteen").done, Is.True);
            // Restore in place (Profile.CopyFrom's shape: the same object, its dictionaries refilled).
            Profile saved = Profile.Read(snapshot);
            rig.profile.contracts.Clear();
            foreach (var kv in saved.contracts) rig.profile.contracts[kv.Key] = kv.Value;
            rig.profile.flags.Clear();
            rig.contracts.Reload();
            Assert.That(rig.Row("firstFifteen").value, Is.EqualTo(5));
            Assert.That(rig.Row("firstFifteen").done, Is.False);
            Assert.That(rig.contracts.Unlocked("daily"), Is.False);
            for (int i = 0; i < 10; i++) rig.Kill();
            Assert.That(rig.Row("firstFifteen").done, Is.True, "completes again from the restored value");
        }

        [Test]
        public void Step_AndAKill_AllocateNothing()
        {
            var rig = new Rig();
            rig.contracts.StartRun(true);
            var kill = new KillEvent { owner = "player", weapon = "revolver", kind = "husk" };
            double dt = cfg.Dt;
            int n = 1;
            TestDelegate step = () =>
            {
                for (int i = 0; i < n; i++) rig.contracts.Step(dt);
            };
            TestDelegate kills = () =>
            {
                for (int i = 0; i < n; i++) rig.events.Emit("kill", kill);
            };
            rig.mover.velocity = new Vec3(5, 0, 0);
            n = 600;
            kills(); // warm: every counted contract past its first write and its completion (FIRST FIFTEEN, FIVE HUNDRED)
            step();
            rig.mover.velocity = new Vec3(9, 0, 0); // a new top speed: a rise is a write, still no allocation
#if UNITY_5_3_OR_NEWER
            Assert.That(step, AllocIs.Not.AllocatingGCMemory());
            n = 100;
            Assert.That(kills, AllocIs.Not.AllocatingGCMemory());
#else
            long before = GC.GetAllocatedBytesForCurrentThread();
            step();
            n = 100;
            kills();
            Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.EqualTo(0), "Contracts.Step / a kill allocated");
#endif
        }
    }
}
