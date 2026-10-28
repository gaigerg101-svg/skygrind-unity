// NIGHT56 builder G (G1 rank56, the contract written before the numbers): the style rank multiplies heal36's close-kill heal and RIDE KILL's
// refund in a headless SurvivalSim (SurvivalSim.N56.Rank.cs; Core/Rank56.cs).
//   row off: a close kill heals heal36's amount at every rank, a ride kill refunds one boost at every rank (NIGHT55 bit for bit);
//   row on:  the heal and the refund are x numbers.mult[rank] (the rank SHOWN when the kill lands: StyleCore.DebugSetRank shows it at once);
//   the numbers: six multipliers, DULL x1 (NIGHT55's payoff) up to SKYGRIND x2, never falling; the row ships off.
// The kill is a real player kill (Loadout.Damage: the bus's `kill`); the ride is the Mover's state at the kill (N53RideKillTests' method).
// No UnityEngine, so it also runs headless on the harness's mini NUnit:
//   dotnet run --project Tools/SimHarness -c Release -p:Extra=../../Assets/Tests/EditMode/Game/N56RankTests.cs -- n56rank selftest
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N56RankTests
    {
        static string Root => Path.GetFullPath("PortExport");

        /// <summary>A fresh headless survival run, rank56 forced (null: the config's own) with rideKill53 on, the player invulnerable at the spawn.</summary>
        static SurvivalSim Run(bool? rank56)
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            NightRowState st = sim.rowsRegistry.Defaults().Set("rideKill53", true);
            if (rank56.HasValue) st = st.Set("rank56", rank56.Value);
            sim.rowsOverride = st;
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            // NIGHT57 G (T-137): in the Editor this namespace runs under N36SharedTests' [SetUpFixture] N36TestBaseline, which sets
            // N36Blocks.ForceOff for EVERY test in Skygrind.Game.Tests: heal36 was off, so the heal read 0 ("Expected 10.0 But was 0.0" in RowOff;
            // RowOn passed only because 0 x mult = 0). The mini NUnit runs no SetUpFixture, so it passed there. heal36 is this row's subject: the
            // NIGHT36 blocks are the config's for this run's start (StartRun reads them), then the switch goes back for the next test.
            bool was36 = N36Blocks.ForceOff;
            N36Blocks.ForceOff = false;
            try { sim.StartRun(4711, null, null); }
            finally { N36Blocks.ForceOff = was36; }
            Assert.That(sim.n36.heal36, Is.True, "heal36 on for this run (the config ships it on)");
            sim.Respawn(map.playerSpawn, 0);
            sim.mover.invulnerable = true;
            return sim;
        }

        /// <summary>A player kill 2 m ahead of the feet at `rank` (shown at once), with the health at 50 and the bar empty: (heal36's heal, the bar
        /// after the kill). The heal is heal36's own event (other heals, if any, are not this row's).</summary>
        static (double heal, double bar) KillAt(SurvivalSim sim, int rank, bool onWall)
        {
            Vec3 p = sim.mover.position;
            Husk h = sim.husks.Spawn("husk", new Vec3(p.x, p.y, p.z - 2), p);
            Assert.That(h != null, Is.True, "the husk spawned");
            sim.style.core.DebugSetRank(rank);
            Assert.That(sim.style.rank, Is.EqualTo(rank), "the rank is shown");
            sim.health.hp = 50;
            sim.mover.stamina.bar = 0;
            sim.mover.pips = 0;
            sim.mover.state = onWall ? MoveState.WallRide : MoveState.Ground;
            double healed = 0;
            System.Action off = sim.events.On<Heal36Event>(Heal36.Event, e => healed += e.amount);
            sim.loadout.Damage(h, 1e6, false, "revolver", sim.Eye);
            off();
            Assert.That(h.alive, Is.False, "the kill happened");
            return (healed, sim.mover.stamina.rules.Any ? sim.mover.stamina.bar : sim.mover.pips);
        }

        static bool Near(double a, double b) => System.Math.Abs(a - b) <= 1e-9;

        [Test]
        public void TheNumbers_SixMultipliers_DullOne_SkygrindTwo_NeverFalling_RowOff()
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            Assert.That(cfg.Design("n56.rank56.enabled"), Is.False, "rank56 ships off (GG's call)");
            Rank56Params p = Rank56Params.From(cfg);
            Assert.That(p.mult.Length, Is.EqualTo(cfg.Arr("style.ranks").Count), "one multiplier per style rank");
            Assert.That(p.mult[0], Is.EqualTo(1.0), "DULL pays NIGHT55's payoff");
            Assert.That(p.mult[p.mult.Length - 1], Is.EqualTo(2.0), "SKYGRIND pays x2");
            for (int i = 1; i < p.mult.Length; i++) Assert.That(p.mult[i] >= p.mult[i - 1], Is.True, "never falling");
            Assert.That(Rank56.Mult(-1, p), Is.EqualTo(p.mult[0]), "below DULL reads DULL");
            Assert.That(Rank56.Mult(99, p), Is.EqualTo(p.mult[p.mult.Length - 1]), "past the top reads the top");
            Assert.That(Rank56.Mult(3, null), Is.EqualTo(1.0), "no numbers: x1");
        }

        [Test]
        public void RowOff_TheHealAndTheRefund_AreTheSameAtEveryRank()
        {
            SurvivalSim plain = Run(null);
            Assert.That(plain.Rank56On, Is.False, "the config ships the row off");
            SurvivalSim off = Run(false);
            double amount = off.cfg.Num("design.n36.heal36.amount"), cost = off.cfg.Num("design.stamina.boost.cost");
            (double heal0, double bar0) = KillAt(off, 0, true);
            Assert.That(heal0, Is.EqualTo(amount), "heal36's amount");
            for (int r = 1; r < 6; r++)
            {
                (double heal, double bar) = KillAt(off, r, true);
                Assert.That(heal, Is.EqualTo(heal0), "rank " + r + ": the heal unchanged");
                Assert.That(bar, Is.EqualTo(bar0), "rank " + r + ": the refund unchanged");
                (double hp, double bp) = KillAt(plain, r, true);
                Assert.That(hp, Is.EqualTo(heal0), "the config's own: the heal unchanged");
                Assert.That(bp, Is.EqualTo(bar0), "the config's own: the refund unchanged");
            }
            Assert.That(bar0 >= cost, Is.True, "the ride kill paid its one boost");
            for (int r = 0; r < 6; r++) Assert.That(off.Rank56Heals[r] + off.Rank56Refunds[r], Is.EqualTo(0), "nothing counted with the row off");
        }

        [Test]
        public void RowOn_TheHealAndTheRefund_ScaleByTheRankShown()
        {
            SurvivalSim off = Run(false);
            (double heal0, double bar0) = KillAt(off, 0, true);
            (double groundHeal0, double groundBar0) = KillAt(off, 0, false); // the kill's own refill (no ride kill on foot)
            double refund0 = bar0 - groundBar0;
            SurvivalSim on = Run(true);
            Assert.That(on.Rank56On, Is.True);
            Rank56Params p = Rank56Params.From(on.cfg);
            for (int r = 0; r < 6; r++)
            {
                (double heal, double bar) = KillAt(on, r, true);
                (double gh, double gb) = KillAt(on, r, false);
                Assert.That(Near(heal, heal0 * p.mult[r]), Is.True, "rank " + r + ": the heal x mult (" + heal + ")");
                Assert.That(Near(gh, groundHeal0 * p.mult[r]), Is.True, "rank " + r + ": the heal on foot x mult too (" + gh + ")");
                double max = on.mover.stamina.rules.Any ? on.mover.StaminaMax : on.mover.MaxPips;
                Assert.That(Near(bar - gb, System.Math.Min(max - gb, refund0 * p.mult[r])), Is.True, "rank " + r + ": the refund x mult, capped at the max (" + (bar - gb) + ")");
                Assert.That(on.Rank56Refunds[r], Is.EqualTo(1), "rank " + r + ": one ride kill counted");
            }
            Assert.That(Near(KillAt(on, 5, false).heal, 2 * heal0), Is.True, "SKYGRIND heals x2");
        }
    }
}
