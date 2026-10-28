// NIGHT20 U3-34.9 A2 + A4 (builder E owns this file): THE HUNT and THE ELITE, the additions backlog's rows A2 and A4
// (Research/2026-10-06-additions-backlog.md), cadence over the web's kinds (no new kind: CLAUDE.md rule 7), each behind
// its row (design.adds.hunt, design.adds.elite; the adds block's bit too). Port-only: the web's director has no rituals.
// - THE HUNT (CoD's hellhound rounds, DRG's Swarmageddon): the web's swarm round (directorMath.isSwarmRound: 8, 13, 18 ...)
//   announced with countTime s left in the break before it (`specialRound` hunt "announce": H's slab, V's dusk shift), its
//   squads telegraphed in a ring around you (Director.huntRingOn: ground spawn nodes ringMin .. ringMax m away, routed,
//   never inside a solid; spawner.telegraphTime still warns), "start" with the round's count, "last" once when one enemy
//   is left (LAST ONE), "end" at its roundClear. The swarm round's drop stays design.zombies.swarmDrop's (SwarmDrop.cs).
// - THE ELITE (CoD's Panzersoldat / Brutus, Gunfire Reborn's elites): from round `from`, then every gapMin .. gapMax rounds
//   (a stream from the run seed ^ salt; a boss or hunt round pushes it on), one `kind` (the CHARGER) mid-round (when half
//   the round's squads are out: "announce" with its telegraph, Telegraph.elite) with `affixes` distinct affixes from
//   affixes.js's list (affixes.onSpawn's `forced`: the ported ones play), named by them (EliteName, H's plate); it lands:
//   "start"; its kill always drops a power-up from the run's stream (powerUps.js spawn with no type) and says "end".
// - A5 THE ROUND TWISTS (DRG's warnings / anomalies, B4B's corruption cards; design.adds.twists): every `every`th ordinary
//   round from `from` (no boss, hunt or elite round) draws one twist from `ids` (the run seed ^ salt; never the same twist
//   twice in a row), named in the break before it (`specialRound` "twist:<id>" "announce"), "start" at its roundBegin, "end"
//   at its roundClear; never on a HEAT or a DAILY run (they bring their own twists). Each from a system that exists:
//   FOG (the view's: the dusk thickens; nothing in the sim), SWARMAGEDDON (a trickle of trickleSquad swarmers every
//   trickleEvery s while the round's own squads still come: Director.AddSquads), LETHAL (the player's damage taken and the
//   points boost x lethalMult: Health.takenMult, Economy.SetBoost "twist"), GOLDEN (the kill-drop chance x goldenMult:
//   PowerUps.dropMult); each set back at the round's clear (or a new run).
// The handlers are made once; a payload is reused (read it inside the handler); nothing allocates per step.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The special rounds' pure rules.</summary>
    public static class SpecialRoundMath
    {
        /// <summary>design.adds.hunt.</summary>
        public struct HuntP
        {
            public double countTime, ringMin, ringMax;

            public static HuntP From(GameConfig c) => new HuntP
            {
                countTime = c.Num("design.adds.hunt.countTime"), ringMin = c.Num("design.adds.hunt.ringMin"), ringMax = c.Num("design.adds.hunt.ringMax"),
            };
        }

        /// <summary>design.adds.elite.</summary>
        public struct EliteP
        {
            public int from, gapMin, gapMax, affixes;
            public string kind;
            public uint salt;

            public static EliteP From(GameConfig c) => new EliteP
            {
                from = c.Int("design.adds.elite.from"), gapMin = c.Int("design.adds.elite.gapMin"), gapMax = c.Int("design.adds.elite.gapMax"),
                affixes = c.Int("design.adds.elite.affixes"), kind = c.Str("design.adds.elite.kind"), salt = (uint)c.Num("design.adds.elite.salt"),
            };
        }

        /// <summary>A hunt round: the web's swarm round.</summary>
        public static bool IsHunt(int round, DirectorMath.D d) => DirectorMath.IsSwarmRound(round, d);

        /// <summary>A round an elite may come on: neither a boss round nor a hunt round.</summary>
        public static bool EliteEligible(int round, DirectorMath.D d) => !DirectorMath.IsBossRound(round, d) && !DirectorMath.IsSwarmRound(round, d);

        /// <summary>The elite rounds up to `upTo`: the first eligible round from `from`, then + gapMin .. gapMax (seeded), each
        /// pushed to the next eligible round.</summary>
        public static List<int> EliteRounds(uint seed, int upTo, in EliteP p, DirectorMath.D d)
        {
            var rng = new Mulberry32(seed);
            var list = new List<int>();
            int r = p.from;
            while (r <= upTo)
            {
                while (!EliteEligible(r, d)) r++;
                if (r > upTo) break;
                list.Add(r);
                r += p.gapMin + (int)Math.Floor(rng.Next() * (p.gapMax - p.gapMin + 1));
            }
            return list;
        }

        /// <summary>`n` distinct affixes from affixes.js's list (each pick among the ones left, in the web's order).</summary>
        public static List<string> EliteAffixes(Func<double> rng, int n, List<string> output)
        {
            output.Clear();
            string[] names = AffixMath.Names;
            for (int k = 0; k < n && output.Count < names.Length; k++)
            {
                int pick = (int)Math.Floor(rng() * (names.Length - output.Count));
                for (int i = 0; i < names.Length; i++)
                {
                    if (output.Contains(names[i])) continue;
                    if (pick-- == 0)
                    {
                        output.Add(names[i]);
                        break;
                    }
                }
            }
            return output;
        }

        /// <summary>A hunt spawn's distance band (flat m from you): ringMin .. ringMax.</summary>
        public static bool InRing(double dist, in HuntP h) => dist >= h.ringMin && dist <= h.ringMax;

        /// <summary>design.adds.twists.</summary>
        public struct TwistP
        {
            public int from, every, trickleSquad;
            public string[] ids;
            public double lethalMult, goldenMult, trickleEvery;
            public uint salt;

            public static TwistP From(GameConfig c) => new TwistP
            {
                from = c.Int("design.adds.twists.from"), every = c.Int("design.adds.twists.every"), ids = c.Strs("design.adds.twists.ids").ToArray(),
                lethalMult = c.Num("design.adds.twists.lethalMult"), goldenMult = c.Num("design.adds.twists.goldenMult"),
                trickleEvery = c.Num("design.adds.twists.trickleEvery"), trickleSquad = c.Int("design.adds.twists.trickleSquad"),
                salt = (uint)c.Num("design.adds.twists.salt"),
            };
        }

        /// <summary>The twist rounds up to `upTo`: every `every`th ordinary round from `from` (no boss, hunt or elite round), each
        /// drawing one of `ids` (seeded), never the last one again.</summary>
        public static List<(int round, string id)> TwistRounds(uint seed, int upTo, in TwistP p, DirectorMath.D d, List<int> elites, string extraId = null, double extraWeight = 1)
        {
            if (extraId != null) return TwistRoundsWeighted(seed, upTo, p, d, elites, extraId, extraWeight);
            var rng = new Mulberry32(seed);
            var list = new List<(int, string)>();
            int count = 0;
            string last = null;
            for (int r = p.from; r <= upTo; r++)
            {
                if (DirectorMath.IsBossRound(r, d) || DirectorMath.IsSwarmRound(r, d) || (elites != null && elites.Contains(r))) continue;
                if (count++ % p.every != 0) continue;
                int pick = (int)Math.Floor(rng.Next() * (p.ids.Length - (last != null ? 1 : 0)));
                string id = null;
                foreach (string x in p.ids)
                {
                    if (x == last) continue;
                    if (pick-- == 0)
                    {
                        id = x;
                        break;
                    }
                }
                list.Add((r, id));
                last = id;
            }
            return list;
        }

        /// <summary>NIGHT21 U3-35.6: the same draw with one more twist, `extraId` (DEADLINE), at `extraWeight` (the others weigh 1;
        /// never the last one again). Only with the row on: the draw above is NIGHT20's bit for bit.</summary>
        static List<(int round, string id)> TwistRoundsWeighted(uint seed, int upTo, in TwistP p, DirectorMath.D d, List<int> elites, string extraId, double extraWeight)
        {
            var rng = new Mulberry32(seed);
            var list = new List<(int, string)>();
            int count = 0;
            string last = null;
            var pool = new List<(string id, double w)>();
            for (int r = p.from; r <= upTo; r++)
            {
                if (DirectorMath.IsBossRound(r, d) || DirectorMath.IsSwarmRound(r, d) || (elites != null && elites.Contains(r))) continue;
                if (count++ % p.every != 0) continue;
                pool.Clear();
                foreach (string x in p.ids) if (x != last) pool.Add((x, 1));
                if (extraId != last) pool.Add((extraId, extraWeight));
                double total = 0;
                foreach (var e in pool) total += e.w;
                double pick = rng.Next() * total;
                string id = pool[pool.Count - 1].id;
                foreach (var e in pool)
                {
                    if (pick < e.w)
                    {
                        id = e.id;
                        break;
                    }
                    pick -= e.w;
                }
                list.Add((r, id));
                last = id;
            }
            return list;
        }
    }

    /// <summary>THE HUNT and THE ELITE for one run (the sim wires it: SurvivalSim.Director20.cs).</summary>
    public sealed class SpecialRounds
    {
        public const string Hunt = "hunt", Elite = "elite";

        readonly EventBus events;
        readonly Director director;
        readonly PowerUps powerUps;
        readonly Func<double> breakLeft;
        readonly Func<uint> runSeed;
        readonly Action<Husk, List<string>> affix;
        readonly DirectorMath.D d;
        readonly double dt;
        readonly SpecialRoundMath.HuntP hp;
        readonly SpecialRoundMath.EliteP ep;
        readonly bool known;
        readonly SpecialRoundEvent payload = new SpecialRoundEvent();
        Mulberry32 eliteRng = new Mulberry32(1);

        /// <summary>This run's rows (set at its start; both off: nothing happens).</summary>
        public bool hunt, elite;
        /// <summary>The elite rounds (seeded at the run's first break).</summary>
        public readonly List<int> Schedule = new List<int>();
        /// <summary>The elite's affixes, its name (H's plate) and id while it lives (-1: none).</summary>
        public readonly List<string> EliteAffixes = new List<string>();
        public string EliteName = "";
        public int EliteId = -1;
        /// <summary>The round running as a hunt / an elite round (0: none), and this round's planned squads.</summary>
        public int HuntRound, EliteRound, PlannedSquads;
        int breakFor;
        bool announced, lastSaid, eliteCalled;
        List<List<string>> trickleList => trickleOne ??= new List<List<string>> { trickle };
        List<List<string>> trickleOne;

        // NIGHT20 A5: the twists.
        readonly Health health;
        readonly Economy economy;
        readonly Func<bool> twistsAllowed;
        readonly SpecialRoundMath.TwistP tp;
        readonly bool twistKnown;
        /// <summary>This run's twist row.</summary>
        public bool twists;
        /// <summary>NIGHT21 U3-35.6: DEADLINE is among the twists this run (design.n21.deadline, set by the sim after NewRun: false
        /// is NIGHT20's draw), its draw weight and its points multiplier (design.n21.deadline.weight / .pointsMult).</summary>
        public bool deadline;
        public double deadlineWeight = 1, deadlinePoints = 1;
        /// <summary>The twist rounds (seeded at the run's first break) and a test's forced ones.</summary>
        public readonly List<(int round, string id)> TwistSchedule = new List<(int, string)>();
        readonly Dictionary<int, string> forced = new Dictionary<int, string>();
        /// <summary>The twist named for the coming / running round (null: none), its round, and the swarmer squads it added.</summary>
        public string TwistId;
        public int TwistRound, Trickled;
        bool twistOn;
        int trickleSteps;
        double savedTaken;
        Func<double> savedDrop;
        readonly List<string> trickle = new List<string>();

        /// <summary>NIGHT20 fix EN2 (the run snapshot, at a break): the elite's stream, the break it is in and what it said,
        /// the last hunt / elite, the twist named for the coming round. The schedules are the run seed's (seeded again at a
        /// restored run's first break); a twist never runs across a break.</summary>
        public void SaveState(StateWriter w)
        {
            w.I("eliteRng", eliteRng.State);
            w.I("breakFor", breakFor);
            w.B("announced", announced);
            w.B("lastSaid", lastSaid);
            w.B("eliteCalled", eliteCalled);
            w.I("huntRound", HuntRound);
            w.I("eliteRound", EliteRound);
            w.I("plannedSquads", PlannedSquads);
            w.I("eliteId", EliteId);
            w.S("eliteName", EliteName ?? "");
            w.Strings("eliteAffixes", EliteAffixes);
            w.S("twistId", TwistId ?? "");
            w.I("twistRound", TwistRound);
            w.I("trickled", Trickled);
            w.I("trickleSteps", trickleSteps);
        }

        public void LoadState(StateReader r)
        {
            eliteRng.State = r.U("eliteRng");
            breakFor = r.Int("breakFor");
            announced = r.B("announced");
            lastSaid = r.B("lastSaid");
            eliteCalled = r.B("eliteCalled");
            HuntRound = r.Int("huntRound");
            EliteRound = r.Int("eliteRound");
            PlannedSquads = r.Int("plannedSquads");
            EliteId = r.Int("eliteId");
            EliteName = r.S("eliteName");
            EliteAffixes.Clear();
            EliteAffixes.AddRange(r.Strings("eliteAffixes"));
            string twist = r.S("twistId");
            TwistId = twist.Length > 0 ? twist : null;
            TwistRound = r.Int("twistRound");
            Trickled = r.Int("trickled");
            trickleSteps = r.Int("trickleSteps");
        }

        /// <summary>A test's hook: round `round` twists with `id` (the HEAT / DAILY rule still holds).</summary>
        public void ForceTwist(int round, string id) => forced[round] = id;

        public SpecialRounds(GameConfig cfg, EventBus events, Director director, PowerUps powerUps, Func<double> breakLeft, Func<uint> runSeed, Action<Husk, List<string>> affix,
            Health health = null, Economy economy = null, Func<bool> twistsAllowed = null)
        {
            this.health = health;
            this.economy = economy;
            this.twistsAllowed = twistsAllowed ?? (() => true);
            twistKnown = cfg.Has("design.adds.twists.ids");
            if (twistKnown) tp = SpecialRoundMath.TwistP.From(cfg);
            for (int i = 0; i < (twistKnown ? tp.trickleSquad : 0); i++) trickle.Add("swarmer");
            this.events = events;
            this.director = director;
            this.powerUps = powerUps;
            this.breakLeft = breakLeft;
            this.runSeed = runSeed;
            this.affix = affix;
            d = DirectorMath.From(cfg);
            dt = cfg.Dt;
            known = cfg.Has("design.adds.hunt.ringMin") && cfg.Has("design.adds.elite.from");
            if (known)
            {
                hp = SpecialRoundMath.HuntP.From(cfg);
                ep = SpecialRoundMath.EliteP.From(cfg);
            }
            director.huntRing = hp;
            director.n20OnElite = OnEliteLanded;
            events.On<RoundEvent>("roundBreak", OnBreak);
            events.On<RoundEvent>("roundBegin", OnBegin);
            events.On<RoundEvent>("roundClear", OnClear);
            events.On<KillEvent>("kill", OnKill);
        }

        /// <summary>A new run: its rows; nothing running (the schedule is seeded at its first break, when the seed is known).</summary>
        public void NewRun(bool huntRow, bool eliteRow, bool twistRow = false)
        {
            EndTwist(); // a run left mid-twist: every system back
            twists = twistRow && twistKnown;
            deadline = false; // NIGHT21: the sim turns it on after this (SurvivalSim.Deadline21.cs)
            TwistSchedule.Clear();
            forced.Clear();
            TwistId = null;
            TwistRound = Trickled = 0;
            hunt = huntRow && known;
            elite = eliteRow && known;
            Schedule.Clear();
            EliteAffixes.Clear();
            EliteName = "";
            EliteId = -1;
            HuntRound = EliteRound = PlannedSquads = breakFor = 0;
            announced = lastSaid = eliteCalled = false;
            director.huntRingOn = false;
        }

        void Say(string name, int round, string phase, int count)
        {
            payload.name = name;
            payload.round = round;
            payload.phase = phase;
            payload.count = count;
            events.Emit(N20Events.SpecialRound, payload);
        }

        void OnBreak(RoundEvent e)
        {
            breakFor = e.number;
            announced = false;
            // the ring from the break on: Director.StartWave sends the opening squads before roundBegin is said
            director.huntRingOn = hunt && SpecialRoundMath.IsHunt(e.number, d);
            if (e.number == 1 && elite)
            {
                uint seed = runSeed() ^ ep.salt;
                Schedule.AddRange(SpecialRoundMath.EliteRounds(seed, 400, ep, d));
                eliteRng = new Mulberry32(seed ^ 0x9e3779b9u);
            }
            if (e.number == 1 && twists) TwistSchedule.AddRange(SpecialRoundMath.TwistRounds(runSeed() ^ tp.salt, 400, tp, d, elite ? Schedule : null, deadline ? Deadline21.TwistId : null, deadlineWeight));
            // A5: the twist named in the break before its round
            TwistId = null;
            TwistRound = 0;
            if (!twists || !twistsAllowed()) return;
            string id = null;
            if (forced.TryGetValue(e.number, out string f)) id = f;
            else foreach (var t in TwistSchedule) if (t.round == e.number) id = t.id;
            if (id == null) return;
            TwistId = id;
            TwistRound = e.number;
            Say("twist:" + id, e.number, "announce", 0);
        }

        void StartTwist()
        {
            twistOn = true;
            Trickled = 0;
            trickleSteps = MovementMath.StepsFor(tp.trickleEvery, dt);
            if (TwistId == "lethal")
            {
                if (health != null)
                {
                    savedTaken = health.takenMult;
                    health.takenMult = savedTaken * tp.lethalMult;
                }
                economy?.SetBoost("twist", tp.lethalMult);
            }
            else if (TwistId == "golden")
            {
                savedDrop = powerUps.dropMult;
                Func<double> was = savedDrop;
                double mult = tp.goldenMult;
                powerUps.dropMult = () => was() * mult;
            }
            else if (TwistId == Deadline21.TwistId && deadline)
            {
                economy?.SetBoost("twist", deadlinePoints); // NIGHT21 DEADLINE: the round's points double (the fuse is Deadline21's)
            }
            Say("twist:" + TwistId, TwistRound, "start", 0);
        }

        /// <summary>The twist's systems back as they were (its round cleared, or a new run).</summary>
        void EndTwist()
        {
            if (!twistOn) return;
            twistOn = false;
            if (TwistId == "lethal")
            {
                if (health != null) health.takenMult = savedTaken;
                economy?.SetBoost("twist", 1);
            }
            else if (TwistId == "golden" && savedDrop != null)
            {
                powerUps.dropMult = savedDrop;
                savedDrop = null;
            }
            else if (TwistId == Deadline21.TwistId) economy?.SetBoost("twist", 1);
        }

        void OnBegin(RoundEvent e)
        {
            PlannedSquads = director.Queue.Count + director.squadsSpawned;
            HuntRound = hunt && SpecialRoundMath.IsHunt(e.number, d) ? e.number : 0;
            director.huntRingOn = HuntRound > 0;
            lastSaid = false;
            if (HuntRound > 0) Say(Hunt, HuntRound, "start", director.Remaining);
            EliteRound = elite && Schedule.Contains(e.number) ? e.number : 0;
            eliteCalled = false;
            if (TwistId != null && TwistRound == e.number) StartTwist();
        }

        void OnClear(RoundEvent e)
        {
            if (HuntRound > 0 && e.number == HuntRound) Say(Hunt, HuntRound, "end", 0);
            if (twistOn && TwistRound == e.number)
            {
                EndTwist();
                Say("twist:" + TwistId, TwistRound, "end", 0);
                TwistId = null;
                TwistRound = 0;
            }
            HuntRound = 0;
            EliteRound = 0;
            director.huntRingOn = false;
        }

        void OnKill(KillEvent k)
        {
            if (EliteId < 0 || k.enemy != EliteId) return;
            int round = director.Round;
            EliteId = -1;
            powerUps.Spawn(k.position); // the type from the run's stream (powerUps.js spawn(at) with no type)
            Say(Elite, round, "end", 0);
        }

        void OnEliteLanded(Husk h)
        {
            EliteId = h.id;
            SpecialRoundMath.EliteAffixes(eliteRng.Next, ep.affixes, EliteAffixes);
            affix?.Invoke(h, EliteAffixes);
            EliteName = string.Join(" ", EliteAffixes).ToUpperInvariant() + " " + h.kind.ToUpperInvariant();
            Say(Elite, director.Round, "start", 1);
        }

        /// <summary>One step: the hunt's count in the break, LAST ONE, the elite's call mid-round.</summary>
        public void Step()
        {
            if (hunt && !announced && breakFor > 0 && SpecialRoundMath.IsHunt(breakFor, d))
            {
                double left = breakLeft();
                if (left > 0 && left <= hp.countTime + 1e-9)
                {
                    announced = true;
                    Say(Hunt, breakFor, "announce", 0);
                }
            }
            if (HuntRound > 0 && !lastSaid && director.Remaining == 1)
            {
                lastSaid = true;
                Say(Hunt, HuntRound, "last", 1);
            }
            if (EliteRound > 0 && !eliteCalled && director.squadsSpawned * 2 >= PlannedSquads && director.SpawnElite(ep.kind))
            {
                eliteCalled = true;
                Say(Elite, EliteRound, "announce", 1);
            }
            // A5 SWARMAGEDDON: a trickle of swarmers while the round's own squads still come (so the round can end)
            if (twistOn && TwistId == "swarmageddon" && director.Queue.Count > 0 && --trickleSteps <= 0)
            {
                trickleSteps = MovementMath.StepsFor(tp.trickleEvery, dt);
                director.AddSquads(trickleList);
                Trickled++;
            }
        }
    }
}
