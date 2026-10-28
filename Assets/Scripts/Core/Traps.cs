// Port of skygrind/src/survival/traps.js (OVERNIGHT2 N6 item 2; NIGHT11 U3-24.5).
// Traps (OVERNIGHT2 N6 item 2): points-priced, hold E at the trap's switch (a small cabinet beside it), then a cooldown.
// Four, each in its own spot of the map (map data: map.trapSpots):
//   PLATE (LANDING): an electric floor plate; everything standing on it for 8 s is shocked.
//   CRUSHER (SLAB GARDENS, north corridor): a slab drops four times over the corridor.
//   FAN (SLAB GARDENS, west corridor): a gravity fan throws whatever walks over it up into the air for 6 s.
//   PYLON (SLAB GARDENS, east corridor): an arc pylon zaps up to four enemies within 8 m every 0.5 s for 8 s.
// A trap kills a regular enemy outright, hurts an elite, only nicks a boss; every kill pays the kill's points plus the
// TRAPPED style bonus, so luring a crowd over one is the point. Traps never hurt you. Fixed step; the look on frame time.
//
// Port notes:
// - The meshes (buildBody, the switch's buildMachine cabinet and its CRT) and update(frameDt) are the Game's TrapViews;
//   this class keeps what the view reads per trap (id, spot, zone, active, cooldown, tick, the spec) and the pure
//   helpers update() uses (SlabDown, CooldownSeconds, Affordable, ZoneOpen).
// - traps.js's deps are delegates, so a checker can drive it with fakes: zones.zoneAt / zones.isOpen (`zoneAt`,
//   `isOpen`; IsOpenOf(zones, map) is Zones.IsOpen's rule without its per-call allocation), loadout.damage(e, amount,
//   false, 'trap', from, false, 1, 0) (`damage`, its bool is hit.killed; DamageBy(loadout) builds the exact call),
//   enemies.launch (`launch`; null: Launch below, husk.js / enemyKind.js launch for the ported kinds),
//   enemies.aimPoint (`aimPoint`; null: Enemy.AimPoint), e.isBoss (`isBoss`; null: no boss, none is ported),
//   particles.burst (`burst`) and arcs.spawn (`arcs`, a reused TrapArc; fx/arcs.js is not ported, the view draws the
//   zap simply). burst / arcs null: nothing drawn.
// - enemies.forEachAlive: the EnemyRegistry's list in its order (spawn order, as the recorded session's enemies). The
//   web game's forEachAlive walks system by system (the husks, then the swarmers, then the spitters); the port's
//   registry is one list in pool-slot order, so the two differ only when kinds mix over one trap at once (the order of
//   the kill events, and the pylon's pick between two at the very same distance).
// - Event payloads are reused (read them inside the handler): trap {id, on}, trapKill {id, kind}, trapSlam {id},
//   styleBonus {owner, name: 'TRAPPED', points: style.bonus.trapped, weapon: 'trap'} (StyleMeter pays it).
// - `thrown` is a Set on the web: here a HashSet and a List beside it for its insertion order (PORT-RULES 4).
// - NIGHT11 hook (port-only, design.power): `powered` (null: always powered, the web's rule); when set, a trap's switch
//   is for sale only while powered() is true. It is checked after the web's own conditions (isOpen is still called
//   first, as on the web). Nothing else changes.
// Step allocates nothing (reused payloads, lists and sets; the pylon's nearest-first order is a stable insertion sort).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>trap {id, on}: a trap was bought (on) or ran out (off). Reused: read it inside the handler.</summary>
    public sealed class TrapEvent
    {
        public string id;
        public bool on;
    }

    /// <summary>trapKill {id, kind}: a trap killed an enemy. Reused: read it inside the handler.</summary>
    public sealed class TrapKillEvent
    {
        public string id;
        public string kind;
    }

    /// <summary>trapSlam {id}: the crusher's slab landed. Reused: read it inside the handler.</summary>
    public sealed class TrapSlamEvent
    {
        public string id;
    }

    /// <summary>arcs.spawn({ from, to, hex, intensity, life, jag, width }): the pylon's zap. Reused: read it inside the callback.</summary>
    public sealed class TrapArc
    {
        public Vec3 from, to;
        public uint hex;
        public double intensity, life, jag, width;
    }

    /// <summary>config.traps.kinds.&lt;id&gt;, read once (steps through stepsFor at 1 / loop.simHz, as traps.js S()).</summary>
    public sealed class TrapSpec
    {
        public string id, name;
        public uint color;
        public double price, time, cooldown, elite, boss;
        public double every, drop, hold, rise, half, rest; // plate / crusher (and the pylon's every)
        public double launch, killAfter, radius; // fan
        public double range, height; // pylon
        public int targets; // pylon
        public double reachUp; // spec.reachUp ?? 2.5
        public bool square; // spec.half truthy: a square of half-size `half` (plate, crusher); else a circle of `area`
        public double area; // spec.range ?? spec.radius
        public int timeSteps, cooldownSteps, everySteps, dropSteps, killAfterSteps; // S(...) (0: not in the spec)
        public List<string> icon;

        /// <summary>traps.js :110: an area's height above the spot when the spec has no reachUp.</summary>
        public const double DefaultReachUp = 2.5;

        public static TrapSpec From(GameConfig cfg, string id, double dt)
        {
            string k = "traps.kinds." + id + ".";
            double N(string key) => cfg.Has(k + key) ? cfg.Num(k + key) : 0;
            int S(string key) => cfg.Has(k + key) ? MovementMath.StepsFor(cfg.Num(k + key), dt) : 0;
            var s = new TrapSpec
            {
                id = id, name = cfg.Str(k + "name"), color = (uint)cfg.Num(k + "color"), price = cfg.Num(k + "price"), time = cfg.Num(k + "time"),
                cooldown = cfg.Num(k + "cooldown"), elite = cfg.Num(k + "elite"), boss = cfg.Num(k + "boss"),
                every = N("every"), drop = N("drop"), hold = N("hold"), rise = N("rise"), half = N("half"), rest = N("rest"),
                launch = N("launch"), killAfter = N("killAfter"), radius = N("radius"), range = N("range"), height = N("height"),
                targets = (int)N("targets"),
                reachUp = cfg.Has(k + "reachUp") ? cfg.Num(k + "reachUp") : DefaultReachUp,
                timeSteps = S("time"), cooldownSteps = S("cooldown"), everySteps = S("every"), dropSteps = S("drop"), killAfterSteps = S("killAfter"),
                icon = cfg.Has(k + "icon") ? cfg.Strs(k + "icon") : new List<string>(),
            };
            s.square = s.half != 0 && !double.IsNaN(s.half);
            s.area = cfg.Has(k + "range") ? s.range : s.radius;
            return s;
        }
    }

    public sealed class Traps : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot (explicit, so the map's Load(null) stays unambiguous): each trap's timers, the
        // fan's thrown list and pending hurts (enemies by registry index; the pending list is never cleared, so dead
        // enemies' entries carry over as the web's do), and the session's stats. The traps (and their Buyables items) are
        // the map's, built by StartRun's Load(map.trapSpots): their ids are checked. Version is bumped.
        void IRunState.Save(StateWriter w)
        {
            var ids = new List<string>(items.Count);
            foreach (Trap it in items) ids.Add(it.id);
            w.Strings("items", ids);
            for (int i = 0; i < items.Count; i++)
            {
                Trap it = items[i];
                w.Begin(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.I("active", it.active);
                w.I("cooldown", it.cooldown);
                w.I("tick", it.tick);
                w.Ints("thrown", StateIO.EnemyIndices(enemies, it.thrown));
                var pe = new List<int>(it.pending.Count);
                var ps = new List<int>(it.pending.Count);
                foreach (Pending p in it.pending)
                {
                    pe.Add(StateIO.EnemyIndex(enemies, p.e));
                    ps.Add(p.steps);
                }
                w.Ints("pendingEnemy", pe);
                w.Ints("pendingSteps", ps);
                w.End();
            }
            w.SI("fired", stats.fired);
            w.Ints("stats", new[] { stats.kills, stats.eliteHits, stats.bossHits, stats.launched });
        }

        void IRunState.Load(StateReader r)
        {
            List<string> ids = r.Strings("items");
            bool same = ids.Count == items.Count;
            for (int i = 0; same && i < ids.Count; i++) same = ids[i] == items[i].id;
            if (!same) throw new StateException("snapshot: traps: the traps differ (restore after StartRun on the same map)");
            List<int> st = r.Ints("stats");
            if (st.Count != 4) throw new StateException("snapshot: traps.stats");
            var thrown = new List<List<Enemy>>();
            var pending = new List<List<Pending>>();
            var timers = new List<int[]>();
            for (int i = 0; i < items.Count; i++)
            {
                StateReader t = r.Section(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                var th = new List<Enemy>();
                foreach (int k in t.Ints("thrown")) th.Add(StateIO.EnemyAt(enemies, k) ?? throw new StateException("snapshot: traps: a null thrown enemy"));
                List<int> pe = t.Ints("pendingEnemy"), ps = t.Ints("pendingSteps");
                if (pe.Count != ps.Count) throw new StateException("snapshot: traps.pending");
                var pl = new List<Pending>();
                for (int k = 0; k < pe.Count; k++) pl.Add(new Pending { e = StateIO.EnemyAt(enemies, pe[k]) ?? throw new StateException("snapshot: traps: a null pending enemy"), steps = ps[k] });
                thrown.Add(th);
                pending.Add(pl);
                timers.Add(new[] { t.Int("active"), t.Int("cooldown"), t.Int("tick") });
            }
            r.SI("fired", stats.fired);
            for (int i = 0; i < items.Count; i++)
            {
                Trap it = items[i];
                it.active = timers[i][0];
                it.cooldown = timers[i][1];
                it.tick = timers[i][2];
                it.ClearThrown();
                foreach (Enemy e in thrown[i])
                {
                    it.thrownSet.Add(e);
                    it.thrown.Add(e);
                }
                it.pending.Clear();
                it.pending.AddRange(pending[i]);
            }
            stats.kills = st[0];
            stats.eliteHits = st[1];
            stats.bossHits = st[2];
            stats.launched = st[3];
            Version++;
        }

        /// <summary>A fan's thrown enemy waiting to be hurt (traps.js pending {e, steps}).</summary>
        public struct Pending
        {
            public Enemy e;
            public int steps;
        }

        /// <summary>One placed trap (traps.js `it`): its spot, switch, zone and timers (fixed steps).</summary>
        public sealed class Trap
        {
            public readonly string id;
            public readonly TrapSpec spec;
            public readonly TrapSpot spot;
            /// <summary>The switch's zone id (zones.zoneAt at the switch, 1 m up; "" outside every zone).</summary>
            public readonly string zone;
            /// <summary>Steps left running (0: off).</summary>
            public int active;
            /// <summary>Steps of cooldown left (0: ready).</summary>
            public int cooldown;
            /// <summary>Steps since it was fired (the plate's and the pylon's ticks, the crusher's cycle).</summary>
            public int tick;
            public Buyables.Item buyable;
            internal readonly HashSet<Enemy> thrownSet = new HashSet<Enemy>(); // the fan: thrown once per run of the trap
            internal readonly List<Enemy> thrown = new List<Enemy>(); // the same, in insertion order
            internal readonly List<Pending> pending = new List<Pending>(); // the fan: thrown, waiting to be hurt (never cleared)

            internal Trap(string id, TrapSpec spec, TrapSpot spot, string zone)
            {
                this.id = id;
                this.spec = spec;
                this.spot = spot;
                this.zone = zone;
            }

            public bool On => active > 0;

            /// <summary>The switch's feet (spot.sx, spot.y, spot.sz).</summary>
            public Vec3 Switch => new Vec3(spot.sx, spot.pos.y, spot.sz);

            /// <summary>it.thrown in insertion order (the fan's; cleared at a buy, a debug fire and the trap's end).</summary>
            public IReadOnlyList<Enemy> Thrown => thrown;

            /// <summary>it.pending in order (the fan's; never cleared: a pending kill resumes on the next run of the trap).</summary>
            public IReadOnlyList<Pending> PendingKills => pending;

            internal void ClearThrown()
            {
                thrownSet.Clear();
                thrown.Clear();
            }
        }

        /// <summary>traps.js stats: fired per trap id, kills, elite and boss hits, enemies thrown (never reset: a session's).</summary>
        public sealed class TrapStats
        {
            public readonly Dictionary<string, int> fired = new Dictionary<string, int>();
            public int kills, eliteHits, bossHits, launched;

            public int Fired(string id) => fired.TryGetValue(id, out int n) ? n : 0;
        }

        // traps.js :149 arcs.spawn({ from: top, to, hex: spec.color, intensity: 3, life: 0.16, jag: 0.14, width: 0.07 })
        public const double ArcIntensity = 3, ArcLife = 0.16, ArcJag = 0.14, ArcWidth = 0.07;

        readonly EventBus events;
        readonly Buyables buyables;
        readonly Func<Vec3, string> zoneAt;
        readonly Func<string, bool> isOpen;
        readonly EnemyRegistry enemies;
        readonly Economy economy; // the switch's screen: can you afford it (null: always)
        readonly Func<Enemy, double, Vec3, bool> damage;
        readonly Action<Enemy, double> launch;
        readonly Dictionary<string, TrapSpec> specs = new Dictionary<string, TrapSpec>();
        readonly string[] eliteKinds;
        readonly double kill, trapped, reach;
        readonly uint slamDust;
        public readonly double simHz;
        public readonly double switchScale; // traps.switchScale (the view's cabinet)
        public readonly List<Trap> items = new List<Trap>();
        public readonly TrapStats stats = new TrapStats();
        /// <summary>Bumped by every Load and Clear (the view relinks its bodies).</summary>
        public int Version { get; private set; }

        /// <summary>NIGHT11 (port-only, design.power): null = always powered (the web's rule); else a switch is for sale only while it says so.</summary>
        public Func<bool> powered;
        /// <summary>e.isBoss (null: no enemy is a boss; none is ported).</summary>
        public Func<Enemy, bool> isBoss;
        /// <summary>enemies.aimPoint (null: Enemy.AimPoint): where the pylon's zap lands.</summary>
        public Func<Enemy, Vec3> aimPoint;
        /// <summary>particles.burst(position, colorHex, count, speed, life, scale): the crusher's dust (null: none).</summary>
        public Action<Vec3, uint, int, double, double, double> burst;
        /// <summary>arcs.spawn: the pylon's zap (a reused TrapArc; null: none).</summary>
        public Action<TrapArc> arcs;

        // Reused per step (no allocation): the pylon's targets and their distances, the payloads.
        readonly List<Enemy> near = new List<Enemy>();
        readonly List<double> nearD = new List<double>();
        readonly TrapEvent trapEvent = new TrapEvent();
        readonly TrapKillEvent killEvent = new TrapKillEvent();
        readonly TrapSlamEvent slamEvent = new TrapSlamEvent();
        readonly StyleBonusEvent bonusEvent = new StyleBonusEvent { owner = "player", name = "TRAPPED", weapon = "trap" };
        readonly TrapArc arc = new TrapArc { intensity = ArcIntensity, life = ArcLife, jag = ArcJag, width = ArcWidth };

        /// <param name="zoneAt">zones.zoneAt(point): the zone id there ("" outside every zone)</param>
        /// <param name="isOpen">zones.isOpen(id) (IsOpenOf(zones, map) in the game)</param>
        /// <param name="damage">loadout.damage with weapon 'trap' (DamageBy): (enemy, amount, from) -> killed</param>
        /// <param name="launch">enemies.launch (null: Launch, the ported kinds' own)</param>
        /// <param name="economy">for the switch's screen only (null: always affordable)</param>
        public Traps(GameConfig cfg, EventBus events, Buyables buyables, Func<Vec3, string> zoneAt, Func<string, bool> isOpen, EnemyRegistry enemies,
            Func<Enemy, double, Vec3, bool> damage, Action<Enemy, double> launch = null, Economy economy = null)
        {
            this.events = events;
            this.buyables = buyables;
            this.zoneAt = zoneAt;
            this.isOpen = isOpen;
            this.enemies = enemies;
            this.damage = damage;
            this.launch = launch ?? Launch;
            this.economy = economy;
            simHz = cfg.movement.loop.simHz;
            double dt = 1 / simHz; // traps.js S: stepsFor(sec, 1 / config.loop.simHz)
            foreach (string id in cfg.Obj("traps.kinds").Keys) specs[id] = TrapSpec.From(cfg, id, dt);
            eliteKinds = cfg.Strs("powerUps.eliteKinds").ToArray();
            kill = cfg.Num("traps.kill");
            switchScale = cfg.Num("traps.switchScale");
            trapped = cfg.Num("style.bonus.trapped");
            reach = cfg.Num("perks.reach");
            slamDust = (uint)cfg.Num("colors.slamDust");
            bonusEvent.points = trapped;
        }

        /// <summary>loadout.damage(e, amount, false, 'trap', from, false, 1, 0) (traps.js hurt): the `damage` delegate.</summary>
        public static Func<Enemy, double, Vec3, bool> DamageBy(Loadout loadout) => (e, amount, from) => loadout.Damage(e, amount, false, "trap", from, false, 1, 0);

        /// <summary>
        /// zones.isOpen(id) with Zones.IsOpen's rule (the zone's index, then not locked) but without its per-call allocation
        /// (Zones.IsOpen finds the index with a closure; a switch's available() runs every step): each id's index is looked up
        /// once.
        /// </summary>
        public static Func<string, bool> IsOpenOf(Zones zones, SurvivalMap map)
        {
            var index = new Dictionary<string, int>();
            return id =>
            {
                if (id == null) return false;
                if (!index.TryGetValue(id, out int z)) index[id] = z = map.ZoneIndex(id);
                return z >= 0 && !zones.LockedTag(z);
            };
        }

        /// <summary>
        /// enemies.launch(e, up) for the ported kinds: husk.js launch (Husk.Launch: velocity.y = max(velocity.y, up), off the
        /// ground); the Swarmer and the Spitter go through enemyKind.js launch, the same with knockbackScale ?? 1 (neither has
        /// one) and no flier among them. Kinds without a launch ignore it.
        /// </summary>
        public static void Launch(Enemy e, double up)
        {
            if (e is Husk h) h.Launch(up);
        }

        public TrapSpec Spec(string id) => specs[id];

        /// <summary>The switch's screen: can you afford it (traps.js update: !economy || economy.canAfford(spec.price)).</summary>
        public bool Affordable(Trap it) => economy == null || economy.CanAfford(it.spec.price);

        /// <summary>traps.js update: !zones.isOpen(it.zone) is `locked` on the switch's screen.</summary>
        public bool ZoneOpen(Trap it) => isOpen(it.zone);

        /// <summary>The switch's screen while cooling down: Math.ceil(cooldown / simHz) s.</summary>
        public int CooldownSeconds(Trap it) => (int)Math.Ceiling(it.cooldown / simHz);

        /// <summary>
        /// traps.js update: the crusher's slab 0 (up) .. 1 (down) at this tick (the fall, the hold, the rise); the view puts
        /// the slab at rest - (rest - 0.55) * down^2. 99 s into the cycle (off): up.
        /// </summary>
        public double SlabDown(Trap it)
        {
            TrapSpec spec = it.spec;
            int cycle = spec.everySteps;
            double ph = it.active > 0 && cycle > 0 ? (it.tick % cycle) / simHz : 99;
            return ph < spec.drop ? ph / spec.drop : ph < spec.drop + spec.hold ? 1 : JsMath.Max(0, 1 - (ph - spec.drop - spec.hold) / spec.rise);
        }

        public void Clear()
        {
            // Everything a load built goes (the view drops its bodies and switches on Version).
            foreach (Trap it in items)
            {
                buyables.list.Remove(it.buyable);
                if (buyables.candidate == it.buyable) buyables.candidate = null; // buyables.js remove
            }
            items.Clear();
            Version++;
        }

        bool Hurt(Trap it, Enemy e, double amount)
        {
            TrapSpec spec = it.spec;
            var from = new Vec3(it.spot.pos.x, it.spot.pos.y + 0.5, it.spot.pos.z);
            if (isBoss != null && isBoss(e))
            {
                stats.bossHits++;
                return damage(e, spec.boss, from);
            }
            bool elite = Array.IndexOf(eliteKinds, e.kind) >= 0;
            if (elite) stats.eliteHits++;
            bool killed = damage(e, elite ? amount : kill, from);
            if (killed)
            {
                stats.kills++;
                killEvent.id = it.id;
                killEvent.kind = e.kind;
                events.Emit("trapKill", killEvent);
                bonusEvent.points = trapped;
                events.Emit("styleBonus", bonusEvent);
            }
            return killed;
        }

        const int Shock = 0, Throw = 1, Gather = 2; // what Inside does with each enemy (traps.js passes a callback)

        /// <summary>Enemies over a trap's area (a square for the plate / crusher, a circle for the fan / pylon).</summary>
        void Inside(Trap it, int what)
        {
            TrapSpec spec = it.spec;
            double x0 = it.spot.pos.x;
            double z0 = it.spot.pos.z;
            List<Enemy> all = enemies.all;
            for (int i = 0; i < all.Count; i++) // enemies.forEachAlive
            {
                Enemy e = all[i];
                if (!e.alive) continue;
                if (e.kind == "rival") continue;
                double dx = e.position.x - x0;
                double dz = e.position.z - z0;
                bool inArea = spec.square ? Math.Abs(dx) <= spec.half && Math.Abs(dz) <= spec.half : JsMath.Hypot(dx, dz) <= spec.area;
                if (inArea && e.position.y - it.spot.pos.y < spec.reachUp) Visit(it, e, what);
            }
        }

        void Visit(Trap it, Enemy e, int what)
        {
            if (what == Shock) Hurt(it, e, it.spec.elite);
            else if (what == Throw)
            {
                if (it.thrownSet.Contains(e)) return;
                it.thrownSet.Add(e);
                it.thrown.Add(e);
                stats.launched++;
                launch(e, it.spec.launch);
                it.pending.Add(new Pending { e = e, steps = it.spec.killAfterSteps });
            }
            else near.Add(e);
        }

        void StepTrap(Trap it)
        {
            TrapSpec spec = it.spec;
            if (it.cooldown > 0) it.cooldown--;
            if (it.active <= 0) return;
            it.active--;
            it.tick++;
            if (it.id == "plate")
            {
                if (it.tick % spec.everySteps == 0) Inside(it, Shock); // reachUp: only what stands on it
            }
            else if (it.id == "crusher")
            {
                // Down every `every` s: the slab falls (a fast drop), crushes, rises.
                int cycle = spec.everySteps;
                int phase = it.tick % cycle;
                if (phase == spec.dropSteps)
                {
                    Inside(it, Shock);
                    burst?.Invoke(new Vec3(it.spot.pos.x, it.spot.pos.y + 0.3, it.spot.pos.z), slamDust, 24, 8, 0.5, 1.6);
                    slamEvent.id = it.id;
                    events.Emit("trapSlam", slamEvent);
                }
            }
            else if (it.id == "fan")
            {
                Inside(it, Throw);
                List<Pending> pending = it.pending;
                for (int i = 0; i < pending.Count; i++)
                {
                    Pending p = pending[i];
                    p.steps--;
                    pending[i] = p;
                    if (p.steps == 0 && p.e.alive) Hurt(it, p.e, spec.elite);
                }
                int w = 0; // it.pending = it.pending.filter((p) => p.steps > 0), in place
                for (int i = 0; i < pending.Count; i++)
                {
                    if (pending[i].steps > 0) pending[w++] = pending[i];
                }
                pending.RemoveRange(w, pending.Count - w);
            }
            else if (it.id == "pylon")
            {
                if (it.tick % spec.everySteps == 0)
                {
                    near.Clear();
                    nearD.Clear();
                    Inside(it, Gather);
                    // near.sort by the 3D squared distance to (spot.x, 0, spot.z), nearest first (stable, as Array.sort).
                    for (int i = 0; i < near.Count; i++)
                    {
                        Vec3 q = near[i].position;
                        double dx = q.x - it.spot.pos.x, dy = q.y - 0, dz = q.z - it.spot.pos.z;
                        nearD.Add(dx * dx + dy * dy + dz * dz);
                    }
                    for (int i = 1; i < near.Count; i++)
                    {
                        Enemy e = near[i];
                        double d = nearD[i];
                        int j = i - 1;
                        while (j >= 0 && nearD[j] - d > 0)
                        {
                            near[j + 1] = near[j];
                            nearD[j + 1] = nearD[j];
                            j--;
                        }
                        near[j + 1] = e;
                        nearD[j + 1] = d;
                    }
                    int n = Math.Min(spec.targets, near.Count); // near.slice(0, spec.targets)
                    for (int i = 0; i < n; i++)
                    {
                        Enemy e = near[i];
                        var top = new Vec3(it.spot.pos.x, it.spot.pos.y + spec.height + 0.3, it.spot.pos.z);
                        Vec3 to = aimPoint != null ? aimPoint(e) : e.AimPoint;
                        if (arcs != null)
                        {
                            arc.from = top;
                            arc.to = to;
                            arc.hex = spec.color;
                            arcs(arc);
                        }
                        Hurt(it, e, spec.elite);
                    }
                    near.Clear();
                }
            }
            if (it.active == 0)
            {
                it.cooldown = spec.cooldownSteps;
                it.ClearThrown();
                trapEvent.id = it.id;
                trapEvent.on = false;
                events.Emit("trap", trapEvent);
            }
        }

        /// <summary>Place a map's traps + their switches (null or empty clears).</summary>
        public void Load(IList<TrapSpot> spots)
        {
            Clear();
            if (spots == null) return;
            for (int s = 0; s < spots.Count; s++)
            {
                TrapSpot spot = spots[s];
                string id = spot.trap;
                if (!specs.TryGetValue(id, out TrapSpec spec)) throw new KeyNotFoundException($"map trapSpots: no config.traps.kinds.{id}");
                string zone = zoneAt(new Vec3(spot.sx, spot.pos.y + 1, spot.sz));
                var it = new Trap(id, spec, spot, zone);
                it.buyable = new Buyables.Item
                {
                    id = "trap:" + id,
                    kind = "trap",
                    name = spec.name,
                    position = new Vec3(spot.sx, spot.pos.y, spot.sz),
                    reach = reach,
                    price = () => spec.price,
                    available = () => isOpen(zone) && it.active == 0 && it.cooldown == 0 && (powered == null || powered()),
                    buy = () =>
                    {
                        it.active = spec.timeSteps;
                        it.tick = 0;
                        it.ClearThrown();
                        stats.fired[id] = stats.Fired(id) + 1;
                        trapEvent.id = id;
                        trapEvent.on = true;
                        events.Emit("trap", trapEvent);
                    },
                };
                buyables.list.Add(it.buyable);
                items.Add(it);
            }
            Version++;
        }

        /// <summary>The placed trap with this id (null: none).</summary>
        public Trap Get(string id)
        {
            foreach (Trap it in items) if (it.id == id) return it;
            return null;
        }

        /// <summary>Debug: fire a trap now (no points).</summary>
        public bool DebugFire(string id)
        {
            Trap it = Get(id);
            if (it == null) return false;
            it.active = it.spec.timeSteps;
            it.tick = 0;
            it.cooldown = 0;
            it.ClearThrown();
            stats.fired[id] = stats.Fired(id) + 1;
            return true;
        }

        /// <summary>One fixed step of every trap (main.js simulate: after powerUps.step, before affixes). Allocates nothing.</summary>
        public void Step(double dt)
        {
            for (int i = 0; i < items.Count; i++) StepTrap(items[i]);
            _ = dt; // traps.js: void dt (the timers count steps)
        }
    }
}
