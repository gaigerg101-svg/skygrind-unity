// Port of skygrind/src/combat/arsenal.js (POWER.md P3), NIGHT9 U3-08: switching pressure for one loadout: every
// weapon's freshness, the primed swap, and cross combos with statuses. Owner-agnostic (a loadout opts in; RIVAL's
// doesn't). Fixed step only.
// - FRESH 0..1 per weapon: a shot / swing costs its freshCost; out of hand it recovers 0.25/s (the FLIP perk 2x).
//   Damage and points x lerp(0.65, 1.25, FRESH).
// - Primed: the first hit within 1 s of swapping a weapon in deals +35% and pops PRIMED; a kill within 2 s of a
//   swap with the weapon swapped in is a SWAP (style).
// - Cross: a hit from another family than the enemy's last hit (within 3 s) deals 1.5x (CROSS). Statuses from
//   weapons (BURN, SHOCK, PULL, STUCK, STAGGER) keep their family on the enemy while they last; PULL then a blast hit
//   is COLLAPSE (2x), SHOCK then a melee hit is OVERLOAD (1.75x + a lightning burst to up to 3 enemies nearby).
// Emits primed {weapon}, crossHit {weapon, family, combo, position}, swapKill {weapon}, overload {from, to[]}.
// Port notes: the web keys its records by the enemy object in a Map (insertion order, entries added during the step's
// walk are visited, deleted ones are not); here a list in that order plus an index by enemy, compacted after the walk.
// The math is ArsenalMath (arsenalMath.js, golden vectors). The statuses' visuals are the enemies' views' (logged).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The arsenal's events: primed, crossHit, swapKill, status, overload (the web's payload fields).</summary>
    public sealed class ArsenalEvent
    {
        public string owner = "player";
        public string weapon;
        public string family;
        public string combo; // null, COLLAPSE, OVERLOAD
        public Vec3 position;
        public int target; // status: the enemy id
        public string status;
        public double seconds;
        public readonly List<Vec3> to = new List<Vec3>(); // overload: the burst's ends
    }

    /// <summary>hit()'s reused result {mult, fresh, primed, cross, combo, family}.</summary>
    public sealed class ArsenalHit
    {
        public double mult = 1, fresh = 1;
        public double multNoFresh = 1; // NIGHT10 U3-13.9 (design.ultra.freshDamage off): primed x cross / combo, without FRESH
        public bool primed, cross;
        public string combo, family;
    }

    public sealed class Arsenal : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot (the loadout's "arsenal" part): FRESH per weapon (in its insertion order, the
        // recovery's walk), the last uses, the per-enemy records in their Map order (enemies by registry index; between
        // steps no record is a removed one), the clock, the primed swap, the last swap, FLIP's numbers, the stats, the
        // burn clock, OVERCLOCK and the switch.
        public void Save(StateWriter w)
        {
            var fv = new List<double>(freshIds.Count);
            foreach (string id in freshIds) fv.Add(fresh[id]);
            w.Strings("freshIds", freshIds);
            w.Doubles("fresh", fv);
            w.SD("lastUse", lastUse);
            w.Begin("records");
            w.I("n", records.Count);
            for (int i = 0; i < records.Count; i++)
            {
                Entry e = records[i];
                if (e.removed) throw new StateException("snapshot: an arsenal record removed mid-step");
                w.Begin(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.I("enemy", StateIO.EnemyIndex(enemies, e.enemy));
                w.I("id", e.r.id);
                w.S("family", e.r.family);
                w.D("at", e.r.at);
                var sn = new List<string>(e.r.statuses.Count);
                var su = new List<double>(e.r.statuses.Count);
                for (int k = 0; k < e.r.statuses.Count; k++)
                {
                    sn.Add(e.r.statuses.NameAt(k));
                    su.Add(e.r.statuses.UntilAt(k));
                }
                w.Strings("statusNames", sn);
                w.Doubles("statusUntil", su);
                w.End();
            }
            w.End();
            w.D("time", time);
            w.B("primed", primed != null);
            w.S("primedWeapon", primed?.weapon);
            w.D("primedAt", primed?.at ?? 0);
            w.B("primedUsed", primed?.used ?? false);
            w.S("lastSwapWeapon", lastSwapWeapon);
            w.D("lastSwapAt", lastSwapAt);
            w.D("modsRecover", modsRecover);
            w.D("modsPrimedBonus", modsPrimedBonus);
            w.Ints("stats", new[] { statPrimed, statCross, statCollapse, statOverload, statSwapKills, statBurnTicks });
            w.D("burnTick", burnTick);
            w.B("overclock", overclock);
            w.B("enabled", enabled);
            // NIGHT20 fix list G2: design.freshness's table (ULTRAKILL's 0-10 values per gun, in insertion order)
            var f19 = new List<double>(fresh19Ids.Count);
            for (int i = 0; i < fresh19Ids.Count; i++) f19.Add(fresh19[fresh19Ids[i]]);
            w.Strings("fresh19Ids", fresh19Ids);
            w.Doubles("fresh19", f19);
        }

        public void Load(StateReader r)
        {
            List<string> ids = r.Strings("freshIds");
            List<double> fv = r.Doubles("fresh");
            List<int> stats = r.Ints("stats");
            if (ids.Count != fv.Count || new HashSet<string>(ids).Count != ids.Count || stats.Count != 6) throw new StateException("snapshot: arsenal fresh / stats");
            StateReader rs = r.Section("records");
            int n = rs.Int("n");
            var newRecords = new List<Entry>(n);
            for (int i = 0; i < n; i++)
            {
                StateReader e = rs.Section(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Enemy enemy = StateIO.EnemyAt(enemies, e.Int("enemy")) ?? throw new StateException("snapshot: an arsenal record with no enemy");
                var rec = new ArsenalRecord { id = e.Int("id"), family = e.S("family"), at = e.D("at") };
                List<string> sn = e.Strings("statusNames");
                List<double> su = e.Doubles("statusUntil");
                if (sn.Count != su.Count) throw new StateException("snapshot: arsenal statuses");
                for (int k = 0; k < sn.Count; k++) rec.statuses.Set(sn[k], su[k]);
                newRecords.Add(new Entry { enemy = enemy, r = rec });
            }
            fresh.Clear();
            freshIds.Clear();
            for (int i = 0; i < ids.Count; i++)
            {
                freshIds.Add(ids[i]);
                fresh.Add(ids[i], fv[i]);
            }
            if (r.Has("fresh19Ids")) // NIGHT20 fix list G2 (absent in an older save: StartRun's table stays)
            {
                List<string> f19Ids = r.Strings("fresh19Ids");
                List<double> f19 = r.Doubles("fresh19");
                if (f19Ids.Count != f19.Count) throw new StateException("snapshot: arsenal fresh19");
                fresh19.Clear();
                fresh19Ids.Clear();
                for (int i = 0; i < f19Ids.Count; i++)
                {
                    fresh19Ids.Add(f19Ids[i]);
                    fresh19[f19Ids[i]] = f19[i];
                }
            }
            r.SD("lastUse", lastUse);
            records.Clear();
            recordOfEnemy.Clear();
            foreach (Entry e in newRecords)
            {
                if (recordOfEnemy.ContainsKey(e.enemy)) throw new StateException("snapshot: two arsenal records for one enemy");
                records.Add(e);
                recordOfEnemy[e.enemy] = e;
            }
            time = r.D("time");
            primed = r.B("primed") ? new PrimedSwap { weapon = r.S("primedWeapon"), at = r.D("primedAt"), used = r.B("primedUsed") } : null;
            lastSwapWeapon = r.S("lastSwapWeapon");
            lastSwapAt = r.D("lastSwapAt");
            modsRecover = r.D("modsRecover");
            modsPrimedBonus = r.D("modsPrimedBonus");
            statPrimed = stats[0];
            statCross = stats[1];
            statCollapse = stats[2];
            statOverload = stats[3];
            statSwapKills = stats[4];
            statBurnTicks = stats[5];
            burnTick = r.D("burnTick");
            overclock = r.B("overclock");
            enabled = r.B("enabled");
        }

        static readonly Dictionary<string, string> StatusFamily = new Dictionary<string, string>
        {
            ["burn"] = "fire", ["shock"] = "volt", ["pull"] = "gravity", ["stuck"] = "volt", ["stagger"] = "impact",
        };

        readonly ArsenalParams a;
        readonly WeaponCatalog catalog;
        readonly EventBus events;
        readonly string owner;
        readonly EnemyRegistry enemies;
        readonly Dictionary<string, double> fresh = new Dictionary<string, double>(); // catalog id -> 0..1 (missing = 1)
        readonly List<string> freshIds = new List<string>(); // fresh's keys in insertion order (the web's for...in)
        readonly Dictionary<string, double> lastUse = new Dictionary<string, double>(); // catalog id -> s of its last use (OVERNIGHT2 N1: no recovery for recoverDelay after it)
        sealed class Entry
        {
            public Enemy enemy;
            public ArsenalRecord r;
            public bool removed;
        }
        readonly List<Entry> records = new List<Entry>(); // enemy object -> {id, family, at, statuses: {name: until}}, in the Map's order
        readonly Dictionary<Enemy, Entry> recordOfEnemy = new Dictionary<Enemy, Entry>();
        double time;
        PrimedSwap primed; // {weapon, at, used}
        string lastSwapWeapon;
        double lastSwapAt = double.NegativeInfinity;
        double modsRecover = 1, modsPrimedBonus; // the FLIP perk
        Action<Enemy, double, string> damageFn; // the loadout's damage (burn ticks, the overload burst)
        readonly ArsenalHit hitOut = new ArsenalHit();
        readonly ArsenalRecord probe = new ArsenalRecord(); // the record crossResult reads (reused)
        public int statPrimed, statCross, statCollapse, statOverload, statSwapKills, statBurnTicks;
        double burnTick;
        bool overclock;

        /// <summary>Off in the debug sandbox (every legacy preset measures exact numbers); main turns it on per mode.</summary>
        public bool enabled = true;

        public Arsenal(GameConfig cfg, EventBus events, string owner, EnemyRegistry enemies)
        {
            a = ArsenalParams.From(cfg);
            catalog = WeaponCatalog.From(cfg);
            this.events = events;
            this.owner = owner;
            this.enemies = enemies;
            modsPrimedBonus = a.primedBonus;
            f19 = Freshness19Params.From(cfg, catalog); // NIGHT19 U3-33.6 (design.freshness; null without its numbers)
        }

        // --- NIGHT19 U3-33.6 (design.freshness, FRESHNESS switch POINTS): ULTRAKILL's table, points and style only ---------

        /// <summary>The run's design.freshness (the loadout sets it from n19.freshness at Reset): FRESH is ULTRAKILL's 0..max
        /// table (Freshness19) instead of the web's lerp, and it never touches damage (Hit: mult without it).</summary>
        public bool fresh19On;
        public readonly Freshness19Params f19;
        readonly Dictionary<string, double> fresh19 = new Dictionary<string, double>(); // catalog id -> 0..max (missing = max)
        readonly List<string> fresh19Ids = new List<string>(); // its keys in insertion order (the walks)
        /// <summary>Guns (catalog kind "gun") the owner holds: one gun means the FRESH floor (set by the loadout).</summary>
        public Func<int> gunsOwned;

        bool On19 => fresh19On && f19 != null;

        /// <summary>A gun's value on ULTRAKILL's 0..max scale (max until it has fired).</summary>
        public double Value19(string weapon) => fresh19.TryGetValue(ArsenalMath.WeaponOf(weapon), out double v) ? v : f19 != null ? f19.max : 0;

        /// <summary>The points multiplier of a gun's tier now (FRESH x1.5 .. DULL x0.25).</summary>
        public double Mult19(string weapon) => f19 != null ? Freshness19.Mult(Value19(weapon), f19) : 1;

        /// <summary>The tier's name (the HUD's tag: FRESH, USED, STALE, DULL).</summary>
        public string Tier19(string weapon) => f19 != null ? Freshness19.Tier(Value19(weapon), f19) : null;

        void Set19(string id, double v)
        {
            if (!fresh19.ContainsKey(id)) fresh19Ids.Add(id);
            fresh19[id] = v;
        }

        void Use19(string id, double scale)
        {
            WeaponInfo info = catalog.Get(id);
            if (info == null) return;
            double interval = f19.Interval(id);
            double floor = gunsOwned != null && gunsOwned() <= 1 ? f19.oneGunFloor : 0; // NIGHT20: design.freshness.oneGunFloor
            Set19(id, Freshness19.Spend(Value19(id), interval, scale, floor, f19));
            double gain = Freshness19.OtherGain(interval, f19) * scale;
            for (int i = 0; i < fresh19Ids.Count; i++)
            {
                string other = fresh19Ids[i];
                if (other != id) fresh19[other] = JsMath.Min(f19.max, fresh19[other] + gain);
            }
        }

        void Step19(double dt, string held)
        {
            for (int i = 0; i < fresh19Ids.Count; i++)
            {
                string id = fresh19Ids[i];
                if (id != held) fresh19[id] = JsMath.Min(f19.max, fresh19[id] + f19.holsterRegen * dt);
            }
        }

        public ArsenalParams Params => a;

        public WeaponCatalog Catalog => catalog;

        public double Time => time;

        ArsenalRecord RecordOf(Enemy enemy)
        {
            if (recordOfEnemy.TryGetValue(enemy, out Entry e) && !e.removed)
            {
                if (e.r.id == enemy.id) return e.r;
                e.r = new ArsenalRecord { id = enemy.id }; // Map.set on the same key keeps its place
                return e.r;
            }
            var n = new Entry { enemy = enemy, r = new ArsenalRecord { id = enemy.id } };
            records.Add(n);
            recordOfEnemy[enemy] = n;
            return n.r;
        }

        /// <summary>FRESH of a weapon (a catalog id, or a hit source like 'punch').</summary>
        public double Fresh(string weapon) => On19 ? Value19(weapon) / f19.max : FreshWeb(weapon); // NIGHT19: the table's share with design.freshness

        /// <summary>The web's FRESH 0..1 (arsenal.js), whatever design.freshness says (its own spend and recovery keep running).</summary>
        double FreshWeb(string weapon) => fresh.TryGetValue(ArsenalMath.WeaponOf(weapon), out double f) ? f : 1;

        /// <summary>The damage + points multiplier of a weapon right now.</summary>
        public double FreshMult(string weapon) => On19 ? Mult19(weapon) : ArsenalMath.FreshMult(FreshWeb(weapon), a);

        void SetFresh(string id, double value)
        {
            if (!fresh.ContainsKey(id)) freshIds.Add(id);
            fresh[id] = value;
        }

        /// <summary>OVERCLOCK (OVERNIGHT2 N6 power-up): every weapon full and none wears while it runs.</summary>
        public void SetOverclock(bool on)
        {
            overclock = on;
            if (on) foreach (string id in freshIds) fresh[id] = 1;
        }

        public bool Overclock => overclock;

        /// <summary>A shot / nail / round / swing (x `scale` of the weapon's cost).</summary>
        public void Use(string weapon, double scale = 1)
        {
            if (overclock) return;
            string id = ArsenalMath.WeaponOf(weapon);
            double? cost = catalog.Get(id)?.freshCost;
            if (cost == null) return;
            SetFresh(id, ArsenalMath.SpendFresh(FreshWeb(id), cost.Value * scale));
            lastUse[id] = time;
            if (On19) Use19(id, scale); // NIGHT19: ULTRAKILL's table
        }

        /// <summary>Debug (`__sky.setFresh`): put a weapon's FRESH at a value.</summary>
        public void DebugSetFresh(string weapon, double value) => SetFresh(ArsenalMath.WeaponOf(weapon), JsMath.Max(0, JsMath.Min(1, value)));

        /// <summary>The FLIP perk: faster recovery, a bigger primed bonus (null: off).</summary>
        public void SetMods(double? recover, double? primedBonus)
        {
            modsRecover = recover ?? 1;
            modsPrimedBonus = primedBonus ?? a.primedBonus;
        }

        /// <summary>The loadout's damage call (burn ticks and the overload burst deal damage through it).</summary>
        public void SetDamage(Action<Enemy, double, string> fn) => damageFn = fn;

        /// <summary>A weapon came into hand (the switch point of a swap).</summary>
        public void SwapIn(string weapon)
        {
            string id = ArsenalMath.WeaponOf(weapon);
            primed = new PrimedSwap { weapon = id, at = time, used = false };
            lastSwapWeapon = id;
            lastSwapAt = time;
        }

        /// <summary>Put a status on an enemy for `seconds` (default: its configured time).</summary>
        public void Status(Enemy enemy, string name) => Status(enemy, name, a.statusTime[name]);

        public void Status(Enemy enemy, string name, double seconds)
        {
            if (enemy == null || !enemy.alive) return;
            ArsenalRecord r = RecordOf(enemy);
            r.statuses.Set(name, JsMath.Max(r.statuses.Get(name), time + seconds));
            events.Emit("status", new ArsenalEvent { owner = owner, target = enemy.id, status = name, seconds = seconds });
        }

        /// <summary>Does an enemy have a status right now?</summary>
        public bool HasStatus(Enemy enemy, string name) =>
            recordOfEnemy.TryGetValue(enemy, out Entry e) && !e.removed && e.r.id == enemy.id && e.r.statuses.Get(name) > time;

        /// <summary>
        /// Before a hit lands: the multiplier (fresh x primed x cross / combo) and its tags. Updates the enemy's record.
        /// Returns a reused object {mult, fresh, primed, cross, combo, family}.
        /// </summary>
        public ArsenalHit Hit(Enemy enemy, string weapon)
        {
            string family = ArsenalMath.FamilyOf(weapon, catalog);
            string id = ArsenalMath.WeaponOf(weapon);
            hitOut.fresh = catalog.Has(id) ? (On19 ? Mult19(id) : ArsenalMath.FreshMult(FreshWeb(id), a)) : 1; // NIGHT19: the table's tier pays points
            double p = ArsenalMath.PrimedMult(primed, id, time, a.primedWindow, modsPrimedBonus);
            hitOut.primed = p > 1;
            if (hitOut.primed) primed.used = true;
            ArsenalRecord r = RecordOf(enemy);
            // The enemy's last family: its last hit within the window, else an active status's (a STUCK nail keeps the
            // volt tag on it: a setup for a blast CROSS).
            string lastFamily = time - r.at <= a.crossWindow ? r.family : null;
            if (lastFamily == null || lastFamily == family)
            {
                for (int i = 0; i < r.statuses.Count; i++)
                {
                    string name = r.statuses.NameAt(i);
                    if (r.statuses.UntilAt(i) > time && (StatusFamily.TryGetValue(name, out string f) ? f : null) != family)
                    {
                        lastFamily = StatusFamily.TryGetValue(name, out string sf) ? sf : null;
                        break;
                    }
                }
            }
            probe.family = lastFamily;
            probe.at = time;
            probe.statuses = r.statuses;
            CrossOutcome res = ArsenalMath.CrossResult(probe, family, weapon, time, a, catalog);
            hitOut.cross = res.cross;
            hitOut.combo = res.combo;
            hitOut.family = family;
            if (res.combo == ArsenalMath.Collapse) r.statuses.Set("pull", double.NegativeInfinity);
            if (res.combo == ArsenalMath.Overload) r.statuses.Set("shock", double.NegativeInfinity);
            hitOut.mult = On19 ? p * res.mult : hitOut.fresh * p * res.mult; // NIGHT19 (design.freshness): never on damage
            hitOut.multNoFresh = p * res.mult;
            if (family != null)
            {
                r.family = family;
                r.at = time;
            }
            return hitOut;
        }

        /// <summary>After a hit landed: stats, events, the SWAP kill and the OVERLOAD burst.</summary>
        public void After(Enemy enemy, string weapon, bool tagPrimed, bool tagCross, string tagCombo, string tagFamily, bool killed, Vec3 position)
        {
            string id = ArsenalMath.WeaponOf(weapon);
            if (tagPrimed)
            {
                statPrimed++;
                events.Emit("primed", new ArsenalEvent { owner = owner, weapon = id, position = position });
            }
            if (tagCross || tagCombo != null)
            {
                if (tagCombo == ArsenalMath.Collapse) statCollapse++;
                else if (tagCombo == ArsenalMath.Overload) statOverload++;
                else statCross++;
                events.Emit("crossHit", new ArsenalEvent { owner = owner, weapon = id, family = tagFamily, combo = tagCombo, position = position });
            }
            if (killed && lastSwapWeapon == id && time - lastSwapAt <= a.swapKillWindow)
            {
                statSwapKills++;
                lastSwapAt = double.NegativeInfinity; // one per swap
                events.Emit("swapKill", new ArsenalEvent { owner = owner, weapon = id });
            }
            if (tagCombo == ArsenalMath.Overload) Overload(enemy, position);
        }

        /// <summary>One fixed step: recovery out of hand, burn ticks, old records dropped.</summary>
        public void Step(double dt, string inHand)
        {
            time += dt;
            string held = ArsenalMath.WeaponOf(inHand);
            // Out of hand, and a moment after its last use (F punches wear the Gauntlet while a gun is in hand).
            for (int i = 0; i < freshIds.Count; i++)
            {
                string id = freshIds[i];
                double f = fresh[id];
                if (id != held && f < 1 && time - (lastUse.TryGetValue(id, out double u) ? u : double.NegativeInfinity) >= a.recoverDelay)
                    fresh[id] = ArsenalMath.RecoverFresh(f, dt, a.recover, modsRecover);
            }
            if (On19) Step19(dt, held); // NIGHT19: holstered guns regain holsterRegen / s
            burnTick += dt;
            bool tick = burnTick >= a.burnTick;
            if (tick) burnTick -= a.burnTick;
            for (int i = 0; i < records.Count; i++)
            {
                Entry e = records[i];
                if (e.removed) continue;
                Enemy enemy = e.enemy;
                ArsenalRecord r = e.r;
                if (!enemy.alive || r.id != enemy.id || (time - r.at > a.memory && !AnyActive(r)))
                {
                    e.removed = true;
                    recordOfEnemy.Remove(enemy);
                    continue;
                }
                if (tick && r.statuses.Get("burn") > time && damageFn != null)
                {
                    statBurnTicks++;
                    damageFn(enemy, a.burnDps * a.burnTick, "burn");
                }
            }
            int n = 0;
            for (int i = 0; i < records.Count; i++) if (!records[i].removed) records[n++] = records[i];
            records.RemoveRange(n, records.Count - n);
        }

        bool AnyActive(ArsenalRecord r)
        {
            for (int i = 0; i < r.statuses.Count; i++) if (r.statuses.UntilAt(i) > time) return true;
            return false;
        }

        /// <summary>A new run (or a mode switch): everything fresh, no records.</summary>
        public void Reset()
        {
            fresh.Clear();
            freshIds.Clear();
            lastUse.Clear();
            records.Clear();
            recordOfEnemy.Clear();
            primed = null;
            lastSwapWeapon = null;
            lastSwapAt = double.NegativeInfinity;
            time = 0;
            fresh19.Clear(); // NIGHT19: every gun FRESH at a run's start
            fresh19Ids.Clear();
        }

        /// <summary>OVERLOAD: a lightning burst from the target to up to 3 enemies nearby.</summary>
        void Overload(Enemy target, Vec3 position)
        {
            var e = new ArsenalEvent { owner = owner, position = position };
            int found = 0;
            var to = new List<Enemy>();
            foreach (Enemy o in enemies.all)
            {
                if (!o.alive || o == target || found >= a.overloadTargets) continue;
                if (Vec3.DistanceTo(o.AimPoint, position) <= a.overloadRadius)
                {
                    to.Add(o);
                    found++;
                }
            }
            foreach (Enemy o in to)
            {
                e.to.Add(o.AimPoint);
                damageFn?.Invoke(o, a.overloadDamage, "overload");
            }
            events.Emit("overload", e);
        }
    }
}
