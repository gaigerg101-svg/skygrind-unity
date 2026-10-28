// NIGHT20 A3 (builder P owns this file; Research/2026-10-06-additions-backlog.md row 16: Risk of Rain 2's Shrine of the
// Mountain, Deep Rock Galactic's Machine Events): THE BEACON on a run (design.adds.beacon; the rule is Core/Beacon20.cs).
// Here (not in a Game file of its own) because SimHarness compiles Game/SurvivalSim*.cs: the farm and a tape's replay play
// it. Made once per composition (SurvivalSim.Beacon20Ensure: the sim's constructor calls it, a test may call it first).
// - In a break the rule picks (Beacon20.Stands on the run seed and the round after the break; never in a break that offers
//   the exfil radio: its own ritual), it stands at a seeded open
//   spot (the map's HOLD ring spots whose zone is open, objectives.js's own test; Beacon20.SpotIndex) as a Buyables item:
//   hold BUY, free ("LIGHT THE BEACON · FREE" on the prompt). The item is in the buy list only while it stands (added at
//   its break, removed at the round's begin or a new run), so every other break's list is NIGHT19's.
// - Taken (`beaconTake`; Beacon20View's banner): at the next round's begin (`roundBegin`, after the director planned it and sent its
//   opening squads) escortSquads squads more are queued (copies of the round's own planned squads, in plan order) and
//   `elites` elites of `kind` are called (builder E's ELITE: Director.SpawnElite, a flagged telegraph out of view; it lands
//   with its affixes and name, and its kill drops a power-up and says `specialRound` elite "end", which earns builder H's
//   reward reveal, 1 of 3, in the next break) (`beaconEscort`).
// - Ignored: it goes with its break and nothing else changed (its draws are its own stream; nothing in the run read it).
// Off (the row or the adds bit; --off20): it never stands. No per-step work; handlers and lists made once; a beacon round
// allocates its escort squads' lists (rare).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT20 A3: THE BEACON (null until Beacon20Ensure).</summary>
        public Beacon20Run beacon20;

        /// <summary>THE BEACON for this composition, made once (the constructor's call; a test's).</summary>
        public Beacon20Run Beacon20Ensure() => beacon20 ??= new Beacon20Run(this);
    }

    /// <summary>NIGHT20 A3 (builder P): THE BEACON on a run.</summary>
    public sealed class Beacon20Run : IRunState
    {
        /// <summary>NIGHT20 fix list (the run snapshot, SAVE & QUIT at a beacon break): the run's beacon state. Its section goes
        /// before "buyables" (SurvivalSim.SnapshotSections): a standing beacon's item is listed again before Buyables checks
        /// the ids (appended, as at its stand: the list's order is the saved one).</summary>
        public void Save(StateWriter w)
        {
            w.B("on", On);
            w.B("standing", Standing);
            w.I("standFor", StandFor);
            w.I("takenFor", TakenFor);
            w.D("spotX", Spot.x);
            w.D("spotY", Spot.y);
            w.D("spotZ", Spot.z);
            w.I("escortAdded", EscortAdded);
            w.I("elitesCalled", ElitesCalled);
            w.I("stood", Stood);
            w.I("taken", Taken);
            w.B("listed", listed);
        }

        public void Load(StateReader r)
        {
            On = r.B("on");
            Standing = r.B("standing");
            StandFor = r.Int("standFor");
            TakenFor = r.Int("takenFor");
            Spot = new Vec3(r.D("spotX"), r.D("spotY"), r.D("spotZ"));
            Item.position = Spot;
            EscortAdded = r.Int("escortAdded");
            ElitesCalled = r.Int("elitesCalled");
            Stood = r.Int("stood");
            Taken = r.Int("taken");
            bool wasListed = listed;
            listed = r.B("listed");
            if (listed && !wasListed) sim.buyables.list.Add(Item);
            else if (!listed && wasListed) sim.buyables.Remove(Item);
        }

        /// <summary>The prompt's name (the buy prompt adds "· FREE").</summary>
        public const string Name = "LIGHT THE BEACON";

        /// <summary>Tests: the row counts as on whatever the adds bit says.</summary>
        public bool Force;

        /// <summary>The buy (hold BUY at the beacon, free): in the buy list only while it stands.</summary>
        public readonly Buyables.Item Item;

        readonly SurvivalSim sim;
        readonly Beacon20.P p;
        readonly DirectorMath.D d;
        readonly List<Vec3> open = new List<Vec3>();
        readonly BeaconEvent payload = new BeaconEvent();
        bool listed;

        /// <summary>This run has the beacon (the row and the adds bit, or Force).</summary>
        public bool On { get; private set; }

        /// <summary>A beacon stands now (in its break, not taken).</summary>
        public bool Standing { get; private set; }

        /// <summary>Where it stands (its last spot).</summary>
        public Vec3 Spot { get; private set; }

        /// <summary>The round the standing beacon is for (the round after its break; 0: none).</summary>
        public int StandFor { get; private set; }

        /// <summary>The round a taken beacon makes harder (0: none taken for a coming round).</summary>
        public int TakenFor { get; private set; }

        /// <summary>The escort squads queued and the elites called at the taken round's begin (-1: not yet this run).</summary>
        public int EscortAdded { get; private set; } = -1;
        public int ElitesCalled { get; private set; } = -1;

        /// <summary>This run's beacons: stood and taken (the farm's counts).</summary>
        public int Stood { get; private set; }
        public int Taken { get; private set; }

        public Beacon20Run(SurvivalSim sim)
        {
            this.sim = sim;
            p = Beacon20.P.From(sim.cfg);
            d = DirectorMath.From(sim.cfg);
            Item = new Buyables.Item
            {
                id = "beacon20", name = Name, kind = "beacon",
                price = () => 0, available = () => Standing && sim.run.state == SurvivalRun.State.Break, buy = Take,
            };
            sim.RunStarted += OnRunStarted;
            sim.events.On<RoundEvent>("roundBreak", OnBreak);
            sim.events.On<RoundEvent>("roundBegin", OnBegin);
        }

        bool RowOn() => Force || sim.n20.Add(sim.cfg, "beacon");

        void OnRunStarted()
        {
            On = RowOn();
            StandDown();
            TakenFor = 0;
            EscortAdded = ElitesCalled = -1;
            Stood = Taken = 0;
        }

        /// <summary>The beacon leaves (its round began, a new break or run): out of the buy list.</summary>
        void StandDown()
        {
            Standing = false;
            StandFor = 0;
            if (!listed) return;
            listed = false;
            sim.buyables.Remove(Item);
        }

        void OnBreak(RoundEvent e)
        {
            StandDown();
            if (e.number <= 1) // a new run's first break: nothing carried over (RunStarted comes after it)
            {
                On = RowOn();
                TakenFor = 0;
                EscortAdded = ElitesCalled = -1;
                Stood = Taken = 0;
            }
            if (!On || !Beacon20.Stands(sim.runSeed, e.number, p, d)) return;
            if (ExfilOffered(e.number)) return; // the break offers the exfil radio: a ritual of its own
            // objectives.js begin()'s open spots: spots.filter((s) => zones.isOpen(zones.zoneAt({x: s.x, y: s.y + 1, z: s.z})))
            open.Clear();
            foreach (Vec3 s in sim.map.holdSpots)
            {
                if (sim.zones.IsOpen(sim.zones.ZoneAt(new Vec3(s.x, s.y + 1, s.z)))) open.Add(s);
            }
            int i = Beacon20.SpotIndex(sim.runSeed, e.number, open.Count, p);
            if (i < 0) return;
            Spot = open[i];
            Item.position = Spot;
            Standing = true;
            StandFor = e.number;
            Stood++;
            sim.buyables.list.Add(Item);
            listed = true;
            Say(Beacon20.StandEvent, e.number, 0, 0);
        }

        /// <summary>The break before `round` offers the exfil radio (Exfil.CanCall's test for that round, read without its
        /// own break handler: the handlers' order doesn't matter).</summary>
        bool ExfilOffered(int round)
        {
            Exfil x = sim.exfil;
            return x != null && x.phase == ExfilPhase.Off && x.Outcome == null && x.IsExfilRound(round) && x.SiteFor(round) >= 0;
        }

        /// <summary>Hold BUY at it: the next round is harder (Buyables calls it after the hold; the item leaves at the round's begin).</summary>
        void Take()
        {
            if (!Standing) return;
            Standing = false;
            TakenFor = StandFor;
            Taken++;
            Say(Beacon20.TakeEvent, TakenFor, 0, 0); // Beacon20View says the banner
        }

        void OnBegin(RoundEvent e)
        {
            StandDown();
            if (!On || TakenFor != e.number) return;
            TakenFor = 0;
            // the escort: copies of the round's own planned squads (what the opening didn't send yet), in plan order
            IReadOnlyList<List<string>> plan = sim.director.Queue;
            var escort = new List<List<string>>(p.escortSquads);
            for (int k = 0; k < p.escortSquads; k++)
                escort.Add(plan.Count > 0 ? new List<string>(plan[k % plan.Count]) : new List<string> { "husk" }); // the planner's fallback kind
            sim.director.AddSquads(escort);
            EscortAdded = escort.Count;
            int called = 0;
            for (int k = 0; k < p.elites; k++)
                if (sim.director.SpawnElite(p.kind)) called++;
            ElitesCalled = called;
            Say(Beacon20.EscortEvent, e.number, EscortAdded, ElitesCalled);
        }

        void Say(string name, int round, int squads, int elites)
        {
            payload.round = round;
            payload.position = Spot;
            payload.squads = squads;
            payload.elites = elites;
            sim.events.Emit(name, payload);
        }
    }
}
