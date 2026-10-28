// NIGHT12 U3-25.7: CONTRACTS. A port of skygrind/src/modes/challenges.js (the 24 badges of config.challenges.list, their
// progress kept in the JSON profile instead of localStorage) and modes/unlocks.js (flags earned once and kept: the
// profile's flags), plus the port-only contracts of design.contracts (to 30-40 in all; Research/retention P3), each
// unlocking variety, never stats.
// - challenges.js: set (a value only rises, capped at the target, done once: challengeDone {id, name}), add, runAdd (per-run
//   counters), the step's top speed and ARCLINE distance, `revived` for UNBROKEN. The batched localStorage save becomes
//   `Dirty` (the Game saves the profile). Its handlers listen to the port's actual events: "kill" KillEvent, "hit"
//   HitEvent, "roundBegin" RoundEvent, "rankChange" (an int: the port's payload is the rank, so every change is passed to
//   set, which keeps only a rise: the same as the web's `e.up` test), "styleAward" StyleAwardEvent, "trapKill",
//   "powerUpGrab", "buy" SpentEvent (a wall buy is a buy whose `what` is a gun's catalog name: the port's SpentEvent has no
//   `kind`), "perk", "trick" "wallJump" (SurvivalGame turns the mover's wall jump into it), the mover's RopeAttach. A
//   badge whose event the port never emits (bosses, BOSS RUSH, affixes, the Thumper, the ARCLINE in Survival) is ported
//   but can't progress: Dormant below.
// - unlocks.js: Unlocked = profile.Flag, Earn = profile.SetFlag (emits unlockEarned the first time). main.js's earn()
//   triggers that the port can reach are here too, so the flags work with design.contracts off (the web's game): round 10
//   begun (round10), 3 guns forged in one run (forge3), THE PROTOCOL complete (protocol), the hidden relay (secretRelay),
//   (NIGHT24: the DAILY is gone; the flag "daily" stays as the id of the INTEREST mod's unlock, earned by the first contract). Their two literals are design.contracts.webEarn (main.js typed them; config.js has none).
//   BEAT MONOLITH (monolith) and a trial GOLD (trialGold) can't happen in the port: port-only contracts grant them.
// - The port-only contracts (design.contracts.list): {id, name, desc, kind, target, unlock?, weapon?, heat?, grade?}; the
//   kinds below are counted from the same events, EndRun judges the end-of-run kinds (exfil, grade).
// - recording false (a LAB run, an automation run): no progress, no flags, no profile writes.
// - Nothing is cached apart from the profile: rows mirror it and Reload() re-reads them (StartRun does; the smoke restores
//   the profile in place and calls it).
// - Step and the per-event handlers allocate nothing unless a contract completes (its payloads).
using System;
using System.Collections.Generic;
using System.IO;

namespace Skygrind.Core
{
    public sealed class Contracts : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the run's own counters (recording, the contracts done this run, the per-run
        // counts, the revive flag, the ARCLINE's air distance and last point, the guns forged this run, the profile's dirty
        // flag). The progress itself lives in the profile (written when the run is saved), so Load re-reads the rows
        // from it (Reload), as StartRun does.
        public void Save(StateWriter w)
        {
            w.B("recording", recording);
            w.B("dirty", Dirty);
            w.Strings("doneThisRun", doneThisRun);
            w.SD("run", run);
            w.B("revived", revived);
            w.D("airSwing", airSwing);
            w.D("lastX", lastX);
            w.D("lastZ", lastZ);
            w.I("forgedThisRun", forgedThisRun);
        }

        public void Load(StateReader r)
        {
            List<string> done = r.Strings("doneThisRun");
            recording = r.B("recording");
            Dirty = r.B("dirty");
            doneThisRun.Clear();
            doneThisRun.AddRange(done);
            r.SD("run", run);
            revived = r.B("revived");
            airSwing = r.D("airSwing");
            lastX = r.D("lastX");
            lastZ = r.D("lastZ");
            forgedThisRun = r.Int("forgedThisRun");
            Reload();
        }

        /// <summary>One contract as the CONTRACTS screen and the PAYOFF strip show it.</summary>
        public sealed class Row
        {
            public string id, name, desc;
            public double target, value;
            public bool done;
            public bool web; // one of challenges.js's 24
            public string unlock; // the flag it grants (null: none)
            public string unlockName; // what that flag opens, for the lock's label ("FINISH: GRAPHITE"), or null
            public double Fraction => target > 0 ? Math.Min(1, value / target) : 0;
            /// <summary>The port can count it (false: its event isn't emitted in this build: bosses, the ARCLINE...).</summary>
            public bool live;
            /// <summary>The design kind ("kills", "round", ...; the web's badges: "web").</summary>
            public string kind;
            /// <summary>Its place in display order.</summary>
            public int order;
        }

        /// <summary>The kinds design.contracts.list may use.</summary>
        public static readonly string[] Kinds =
        {
            "kills", "headshots", "weaponKills", "slideKills", "airKills", "round", "gates", "wallBuys", "perks", "boxPulls",
            "power", "protocol", "secretRelay", "forgeRun", "exfil", "grade",
        };

        /// <summary>
        /// The web's badges the port can't progress yet, and why: their events aren't emitted in this build. Ported as they
        /// are; a row turns live once it has any progress.
        /// </summary>
        public static readonly Dictionary<string, string> Dormant = new Dictionary<string, string>
        {
            ["quakeCrowd"] = "the Thumper isn't a gun the port fires, and KillEvent carries no shock / shot",
            ["skyWalker"] = "the ARCLINE gadget isn't ported (Survival's mover has ropeEnabled false)",
            ["returnToSender"] = "no bosses (a hit of kind 'boss')",
            ["bulwarkDown"] = "no bosses (bossDown)",
            ["monolithDown"] = "no bosses (bossDown)",
            ["rivalPrimeDown"] = "no bosses (bossDown)",
            ["orbiterDown"] = "no bosses (bossDown)",
            ["hiveDown"] = "no bosses (bossDown)",
            ["slingshots"] = "the ARCLINE isn't ported (slingshot is never emitted)",
            ["shieldBreaker"] = "no affixes (affixBreak)",
            ["bossRush"] = "no BOSS RUSH (bossRushDone)",
        };

        sealed class Def
        {
            public Row row;
            public string weapon, grade;
            public double heat;
        }

        /// <summary>The rows in display order (the web's 24 in config order, then design.contracts.list).</summary>
        public readonly List<Row> rows = new List<Row>();

        /// <summary>Contracts completed during the current run (ids, in order: the PAYOFF strip and RESULTS).</summary>
        public readonly List<string> doneThisRun = new List<string>();

        /// <summary>False for a LAB run or an automation run (noRecords): no progress, no unlocks, nothing to save.</summary>
        public bool recording;

        /// <summary>The profile changed since the Game last saved it.</summary>
        public bool Dirty;

        /// <summary>The first unlock, reachable in round 1-2 of a first run (design.contracts.first), or null.</summary>
        public readonly string FirstUnlockId;

        /// <summary>The player is sliding / airborne at a kill (slideKills, airKills). Null: read from the mover.</summary>
        public Func<bool> sliding, airborne;

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly Profile profile;
        readonly Mover mover;
        readonly Func<List<string>> mutators;
        readonly Dictionary<string, Row> byId = new Dictionary<string, Row>();
        readonly Dictionary<string, List<Def>> byKind = new Dictionary<string, List<Def>>();
        readonly Dictionary<string, string> unlockNames = new Dictionary<string, string>();
        readonly Dictionary<string, double> run = new Dictionary<string, double>(); // per-run counters
        readonly HashSet<string> gunNames = new HashSet<string>(); // the catalog's names: a "buy" of one is a wall buy
        readonly List<Def> kills, headshots, weaponKills, slideKills, airKills, roundDefs, gates, wallBuys, perks, boxPulls,
            power, protocol, secretRelay, forgeRun, exfil, grade;
        readonly Row headhunter, returnToSender, round10, round20, round30, mutated, unbroken, impacts, slingshots, trapper,
            lucky, shieldBreaker, arsenal, perkHoarder, wallRider, speed, skyWalker;
        readonly double earnRound10, earnForge3; // main.js: e.number >= 10, forge.forged.size >= 3 (design.contracts.webEarn)
        double airSwing; // m travelled on the ARCLINE since you last touched the ground
        double lastX, lastZ;
        bool revived;
        int forgedThisRun;

        public Contracts(GameConfig cfg, EventBus events, Profile profile, Mover mover, Func<List<string>> mutators)
        {
            this.cfg = cfg;
            this.events = events;
            this.profile = profile;
            this.mover = mover;
            this.mutators = mutators;
            foreach (string k in Kinds) byKind[k] = new List<Def>();
            foreach (var kv in cfg.Obj("weapons.catalog"))
                if (kv.Value is Dictionary<string, object> w && w.TryGetValue("name", out object n) && n is string s) gunNames.Add(s);

            // challenges.js: config.challenges.list, in order.
            foreach (object o in cfg.Arr("challenges.list"))
            {
                var c = PortData.Obj(o);
                AddRow(new Row
                {
                    id = (string)c["id"], name = (string)c["name"], desc = (string)c["desc"], target = PortData.Num(c["target"]), web = true, kind = "web",
                });
            }

            // design.contracts: the port-only list, the web badges' unlocks, main.js's literals, the first unlock.
            bool on = cfg.Design("contracts.enabled");
            earnRound10 = cfg.Has("design.contracts.webEarn.round10") ? cfg.Num("design.contracts.webEarn.round10") : double.PositiveInfinity;
            earnForge3 = cfg.Has("design.contracts.webEarn.forge3") ? cfg.Num("design.contracts.webEarn.forge3") : double.PositiveInfinity;
            if (on && cfg.Has("design.contracts.webUnlocks"))
                foreach (var kv in cfg.Obj("design.contracts.webUnlocks"))
                    if (byId.TryGetValue(kv.Key, out Row r)) r.unlock = (string)kv.Value;
            if (on && cfg.Has("design.contracts.list"))
            {
                foreach (object o in cfg.Arr("design.contracts.list"))
                {
                    var c = PortData.Obj(o);
                    string id = (string)c["id"], kind = (string)c["kind"];
                    if (byId.ContainsKey(id)) throw new InvalidDataException($"design.contracts.list: '{id}' is used twice");
                    if (!byKind.TryGetValue(kind, out List<Def> list)) throw new InvalidDataException($"design.contracts.list: '{id}' has an unknown kind '{kind}'");
                    var row = new Row
                    {
                        id = id, name = (string)c["name"], desc = (string)c["desc"], target = PortData.Num(c["target"]), kind = kind,
                        unlock = c.TryGetValue("unlock", out object u) ? u as string : null,
                    };
                    AddRow(row);
                    list.Add(new Def
                    {
                        row = row,
                        weapon = c.TryGetValue("weapon", out object w) ? w as string : null,
                        grade = c.TryGetValue("grade", out object g) ? g as string : null,
                        heat = c.TryGetValue("heat", out object h) ? PortData.Num(h) : 0,
                    });
                }
                if (cfg.Has("design.contracts.first"))
                {
                    FirstUnlockId = cfg.Str("design.contracts.first");
                    if (!byId.ContainsKey(FirstUnlockId)) throw new InvalidDataException($"design.contracts.first: no contract '{FirstUnlockId}'");
                }
            }
            if (on && cfg.Has("design.contracts.flagNames"))
                foreach (var kv in cfg.Obj("design.contracts.flagNames")) unlockNames[kv.Key] = Join((string)kv.Value, ListedName(kv.Key));
            foreach (Row r in rows) r.unlockName = UnlockName(r.unlock);

            kills = byKind["kills"];
            headshots = byKind["headshots"];
            weaponKills = byKind["weaponKills"];
            slideKills = byKind["slideKills"];
            airKills = byKind["airKills"];
            roundDefs = byKind["round"];
            gates = byKind["gates"];
            wallBuys = byKind["wallBuys"];
            perks = byKind["perks"];
            boxPulls = byKind["boxPulls"];
            power = byKind["power"];
            protocol = byKind["protocol"];
            secretRelay = byKind["secretRelay"];
            forgeRun = byKind["forgeRun"];
            exfil = byKind["exfil"];
            grade = byKind["grade"];
            headhunter = W("headhunter");
            returnToSender = W("returnToSender");
            round10 = W("round10");
            round20 = W("round20");
            round30 = W("round30");
            mutated = W("mutated");
            unbroken = W("unbroken");
            impacts = W("impacts");
            slingshots = W("slingshots");
            trapper = W("trapper");
            lucky = W("lucky");
            shieldBreaker = W("shieldBreaker");
            arsenal = W("arsenal");
            perkHoarder = W("perkHoarder");
            wallRider = W("wallRider");
            speed = W("speed");
            skyWalker = W("skyWalker");
            Reload();

            // challenges.js's handlers, in its order (the port's events and payloads).
            events.On<KillEvent>("kill", OnKill);
            events.On<HitEvent>("hit", e =>
            {
                if (Mine(e.owner) && e.weapon == "orb" && e.kind == "boss") Add(returnToSender);
            });
            // bossDown (the five boss badges): the port has no bosses and no bossDown payload yet (Dormant).
            events.On<RoundEvent>("roundBegin", e =>
            {
                Set(round10, e.number);
                Set(round20, e.number);
                Set(round30, e.number);
                List<string> m = mutators?.Invoke();
                if (m != null && m.Count >= 2) Set(mutated, e.number);
                for (int i = 0; i < roundDefs.Count; i++) Set(roundDefs[i].row, e.number);
                if (e.number >= earnRound10) Earn("round10"); // main.js: flow.mode === 'survival' && e.number >= 10 && earn('round10')
            });
            events.On("rankChange", o =>
            {
                if (!revived && o is int rank) Set(unbroken, rank);
            });
            events.On("phoenix", _ => revived = true);
            events.On<StyleAwardEvent>("styleAward", e =>
            {
                if (e.name == "IMPACT") Add(impacts);
            });
            events.On("slingshot", _ => Add(slingshots));
            events.On("trapKill", _ => Add(trapper));
            events.On("powerUpGrab", _ => Add(lucky));
            events.On("affixBreak", _ => Add(shieldBreaker));
            events.On<SpentEvent>("buy", e =>
            {
                if (e.what == null || !gunNames.Contains(e.what)) return; // buyables.js kind === 'weapon': a wall buy
                RunAdd(arsenal);
                for (int i = 0; i < wallBuys.Count; i++) Add(wallBuys[i].row);
            });
            events.On("perk", _ =>
            {
                RunAdd(perkHoarder);
                for (int i = 0; i < perks.Count; i++) Add(perks[i].row);
            });
            events.On("trick", o =>
            {
                if (o as string == "wallJump") RunAdd(wallRider);
            });
            if (mover != null) mover.Events += OnMover; // ropeAttach: where the swing's distance starts

            // The port-only kinds.
            events.On("gateOpen", _ =>
            {
                for (int i = 0; i < gates.Count; i++) Add(gates[i].row);
            });
            events.On<BoxOpenEvent>("boxOpen", e =>
            {
                if (!Mine(e.owner)) return;
                for (int i = 0; i < boxPulls.Count; i++) Add(boxPulls[i].row);
            });
            events.On("powerOn", _ =>
            {
                for (int i = 0; i < power.Count; i++) Set(power[i].row, 1);
            });
            events.On("protocolComplete", _ =>
            {
                for (int i = 0; i < protocol.Count; i++) Add(protocol[i].row);
                Earn("protocol"); // main.js: protocol's onComplete earn('protocol') (a no-op when the Game's callback earned it)
            });
            events.On("secretRelay", _ =>
            {
                for (int i = 0; i < secretRelay.Count; i++) Set(secretRelay[i].row, 1);
                Earn("secretRelay"); // main.js: protocol's onSecret earn('secretRelay')
            });
            events.On("forge", _ =>
            {
                forgedThisRun++; // forge.js emits once per gun per run: the count is forge.forged.size
                for (int i = 0; i < forgeRun.Count; i++) RunAdd(forgeRun[i].row);
                if (forgedThisRun >= earnForge3) Earn("forge3"); // main.js: forge.forged.size >= 3 && earn('forge3')
            });
        }

        void AddRow(Row r)
        {
            r.order = rows.Count;
            rows.Add(r);
            byId[r.id] = r;
        }

        Row W(string id) => byId.TryGetValue(id, out Row r) ? r : null;

        static bool Mine(string owner) => owner == null || owner == "player"; // challenges.js mine()

        bool Sliding() => sliding != null ? sliding() : mover != null && mover.state == MoveState.Slide;

        bool Airborne() => airborne != null ? airborne() : mover != null && !mover.grounded;

        void OnKill(KillEvent e)
        {
            if (!Mine(e.owner)) return;
            if (e.headshot) Add(headhunter);
            // quakeCrowd (weapon 'thumper' && e.shock, counted per shot): the port fires no Thumper and its KillEvent has
            // no shock / shot (Dormant).
            if (kills.Count + headshots.Count + weaponKills.Count + slideKills.Count + airKills.Count == 0) return;
            for (int i = 0; i < kills.Count; i++) Add(kills[i].row);
            if (e.headshot) for (int i = 0; i < headshots.Count; i++) Add(headshots[i].row);
            if (weaponKills.Count > 0)
            {
                string gun = ArsenalMath.WeaponOf(e.weapon); // a punch / orb is the GAUNTLET's, a ricochet the KESTREL's
                for (int i = 0; i < weaponKills.Count; i++) if (weaponKills[i].weapon == gun) Add(weaponKills[i].row);
            }
            if (slideKills.Count > 0 && Sliding()) for (int i = 0; i < slideKills.Count; i++) Add(slideKills[i].row);
            if (airKills.Count > 0 && Airborne()) for (int i = 0; i < airKills.Count; i++) Add(airKills[i].row);
        }

        void OnMover(MoverEvent e)
        {
            if (e.type != MoverEventType.RopeAttach) return;
            lastX = mover.position.x;
            lastZ = mover.position.z;
        }

        // --- challenges.js's store -------------------------------------------------------------------------------------

        /// <summary>A contract's value in the profile (0 before any progress).</summary>
        public double Value(string id) => profile.contracts.TryGetValue(id, out Profile.Counter c) ? c.v : 0;

        /// <summary>challenges.js set: a value only rises (capped at the target); done once it reaches the target.</summary>
        void Set(Row c, double v)
        {
            if (c == null || !recording) return;
            if (v <= Value(c.id)) return;
            Profile.Counter rec = profile.Contract(c.id);
            rec.v = JsMath.Min(v, c.target);
            c.value = rec.v;
            Dirty = true;
            if (!rec.done && rec.v >= c.target)
            {
                rec.done = true;
                c.done = true;
                c.live = true;
                Complete(c);
            }
            else if (rec.v > 0) c.live = true;
        }

        void Add(Row c, double n = 1)
        {
            if (c != null) Set(c, Value(c.id) + n);
        }

        void RunAdd(Row c, double n = 1)
        {
            if (c == null || !recording) return;
            double v = (run.TryGetValue(c.id, out double r) ? r : 0) + n;
            run[c.id] = v;
            Set(c, v);
        }

        void Complete(Row c)
        {
            doneThisRun.Add(c.id);
            if (c.web) events.Emit(MetaEvents.ChallengeDone, new ChallengeEvent { id = c.id, name = c.name });
            events.Emit(MetaEvents.ContractDone, new ContractEvent { id = c.id, name = c.name, unlock = c.unlock, unlockName = c.unlockName, web = c.web });
            if (c.unlock != null) Earn(c.unlock);
        }

        /// <summary>Debug / the smoke: set a contract's value (challenges.js debugSet). Only while recording.</summary>
        public void DebugSet(string id, double v) => Set(W(id), v);

        /// <summary>Re-read every row from the profile (it may have been restored in place: Profile.CopyFrom). No events.</summary>
        public void Reload()
        {
            foreach (Row r in rows)
            {
                bool has = profile.contracts.TryGetValue(r.id, out Profile.Counter c);
                r.value = has ? c.v : 0;
                r.done = has && c.done;
                r.live = !Dormant.ContainsKey(r.id) || r.value > 0 || r.done;
            }
        }

        public int DoneCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < rows.Count; i++) if (rows[i].done) n++;
                return n;
            }
        }

        /// <summary>challenges.js startRun: the per-run counters start over; `recording` says if this run counts.</summary>
        public void StartRun(bool recording)
        {
            this.recording = recording;
            doneThisRun.Clear();
            run.Clear();
            revived = false;
            airSwing = 0;
            forgedThisRun = 0;
            Reload();
        }

        /// <summary>challenges.js step (fixed step): top speed, the ARCLINE distance off the ground.</summary>
        public void Step(double dt)
        {
            if (mover == null) return;
            Set(speed, Math.Floor(mover.Speed));
            if (mover.rope.phase == RopePhase.Attached || (airSwing > 0 && !mover.grounded))
            {
                airSwing += JsMath.Hypot(mover.position.x - lastX, mover.position.z - lastZ);
                Set(skyWalker, Math.Floor(airSwing));
            }
            else if (mover.grounded) airSwing = 0;
            lastX = mover.position.x;
            lastZ = mover.position.z;
        }

        /// <summary>
        /// The run ended (RunStats filled): the end-of-run contracts (an exfil, on a HEAT rung; a grade)
        /// judge it. Copies doneThisRun into stats.contractsDone.
        /// </summary>
        public void EndRun(RunStats stats)
        {
            if (stats == null) return;
            if (recording)
            {
                bool win = stats.exfil == Exfil.Win;
                for (int i = 0; i < exfil.Count; i++) if (win && stats.heat >= exfil[i].heat) Add(exfil[i].row);
                int g = Profile.GradeRank(stats.grade);
                for (int i = 0; i < grade.Count; i++)
                {
                    int want = Profile.GradeRank(grade[i].grade);
                    if (want >= 0 && g >= want) Set(grade[i].row, 1);
                }
            }
            stats.contractsDone.Clear();
            stats.contractsDone.AddRange(doneThisRun);
        }

        /// <summary>The `n` unfinished contracts closest to done (highest fraction; ties in display order): the PAYOFF strip.</summary>
        public List<Row> Nearest(int n)
        {
            var list = new List<Row>();
            foreach (Row r in rows) if (!r.done && r.live) list.Add(r);
            list.Sort((a, b) =>
            {
                int c = b.Fraction.CompareTo(a.Fraction);
                return c != 0 ? c : a.order.CompareTo(b.order); // a stable order: display order breaks ties
            });
            if (n < 0) n = 0;
            if (list.Count > n) list.RemoveRange(n, list.Count - n);
            return list;
        }

        /// <summary>The unfinished contract with an unlock that is closest to done (the PAYOFF strip's NEXT UNLOCK), or null.</summary>
        public Row NextUnlock()
        {
            Row best = null;
            foreach (Row r in rows)
            {
                if (r.done || !r.live || r.unlock == null || Unlocked(r.unlock)) continue;
                if (best == null || r.Fraction > best.Fraction) best = r;
            }
            return best;
        }

        /// <summary>The contract that grants `flag` (the lock's "how": a finish or a mod), or null.</summary>
        public Row Grants(string flag)
        {
            if (flag == null) return null;
            foreach (Row r in rows) if (r.unlock == flag) return r;
            return null;
        }

        /// <summary>A contract by id, or null.</summary>
        public Row Of(string id) => id != null ? W(id) : null;

        // --- unlocks.js -------------------------------------------------------------------------------------------------

        /// <summary>unlocks.js has.</summary>
        public bool Unlocked(string flag) => profile.Flag(flag);

        /// <summary>unlocks.js set: earn a flag (true the first time; emits unlockEarned). Only while recording.</summary>
        public bool Earn(string flag)
        {
            if (!recording || string.IsNullOrEmpty(flag)) return false;
            if (!profile.SetFlag(flag)) return false;
            Dirty = true;
            events.Emit(MetaEvents.UnlockEarned, new UnlockEvent { flag = flag, name = UnlockName(flag) });
            return true;
        }

        /// <summary>
        /// What a flag opens, for a lock's label: design.contracts.flagNames' text, then every finish it unlocks (its config
        /// flag, or the port's "finish.&lt;id&gt;") and every MOD it unlocks (config.mods.list.&lt;id&gt;.locked.flag), joined
        /// with " · " ("THE DAILY · MOD: INTEREST"); null when it opens nothing known.
        /// </summary>
        public string UnlockName(string flag)
        {
            if (flag == null) return null;
            if (unlockNames.TryGetValue(flag, out string s)) return s;
            return unlockNames[flag] = ListedName(flag);
        }

        string ListedName(string flag)
        {
            string text = null;
            foreach (string id in Finishes.OrderView(cfg))
                if (Finishes.OpenedBy(cfg, id, flag)) text = Join(text, "FINISH: " + Finishes.Name(cfg, id));
            foreach (var kv in cfg.Obj("mods.list"))
            {
                if (!(kv.Value is Dictionary<string, object> d) || !d.TryGetValue("locked", out object l) || !(l is Dictionary<string, object> locked)) continue;
                if (locked.TryGetValue("flag", out object f) && f as string == flag) text = Join(text, "MOD: " + (string)d["name"]);
            }
            return text;
        }

        static string Join(string a, string b) => a == null ? b : b == null ? a : a + " · " + b;
    }
}
