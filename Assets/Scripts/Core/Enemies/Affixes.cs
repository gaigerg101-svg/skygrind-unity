// NIGHT19 U3-33.7 (builder E): port of skygrind/src/enemies/affixes.js, SHIELDED first (the rest are U3-33.7's beyond
// list), behind design.enemies.n19 (the run's Affixes is null with the block off: nothing is rolled or filtered).
// affixes.js's header:
// Enemy affixes (OVERNIGHT2 N6 item 3): from round 8 a regular enemy may spawn with one, from round 16 with two.
//   SHIELDED: a front plate; hits from in front are stopped until a headshot or a melee hit breaks it.
//   VOLATILE: it bursts 0.4 s after it dies (a red-orange circle fills first): stay out of it.
//   BLINKER: every few seconds it blinks a short way (a white flash where it goes).
//   REGENERATING: out of damage for a moment, it heals back.
//   SWIFT: it moves 35% faster.
// An icon over its head per affix (a pixel icon in the affix's colour, facing you); an affixed kill pays bonus points.
// Fixed step; the icons on frame time. Pure rolls in rollAffixes (tested).
// The port: AffixMath.Roll is rollAffixes line for line on the run's own Mulberry32 (the web passes Math.random; the seed
// is the run's ^ design.enemies.n19.affixSalt). Only SHIELDED is ported tonight: a rolled VOLATILE, BLINKER,
// REGENERATING or SWIFT is dropped after the roll (the stream advances exactly as the web's), logged in PORT-STATUS by the
// lead. The plate is Enemy.Shielded (the shared hook W's combos read) with its half-angle on the Husk; EnemyHits.Filter is
// enemies.js damage()'s damageFilter (affixes.filter) followed by the kind's damageMult (charger.js: the back weak
// point), the hook weapons.js damage() runs on every player hit (Loadout.Damage: a blocked hit lands nothing and emits
// no hit event). Events keep the web's names: affixBlock / affixBreak {id} (AffixEvent), plus the shared commit's
// port-only plateBreak (the id) for builder W's combos. The bonus points on an affixed kill (economy.grant(bonusPoints x
// the affixes, 'affix')) are the run's (SurvivalSim.Enemies19.cs).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class AffixMath
    {
        /// <summary>affixes.js AFFIXES, in the web's order (the roll indexes it).</summary>
        public static readonly string[] Names = { "shielded", "volatile", "blinker", "regenerating", "swift" };

        /// <summary>The affixes the port plays (SHIELDED tonight).</summary>
        public static bool Ported(string name) => name == "shielded";

        /// <summary>config.affixes' roll numbers and the SHIELDED plate's half-angle.</summary>
        public struct P
        {
            public double from, chance, perRound, maxChance, twoFrom, twoChance, bonusPoints, shieldHalfDeg;

            public static P From(GameConfig c) => new P
            {
                from = c.Num("affixes.from"), chance = c.Num("affixes.chance"), perRound = c.Num("affixes.perRound"),
                maxChance = c.Num("affixes.maxChance"), twoFrom = c.Num("affixes.twoFrom"), twoChance = c.Num("affixes.twoChance"),
                bonusPoints = c.Num("affixes.bonusPoints"), shieldHalfDeg = c.Num("affixes.kinds.shielded.halfDeg"),
            };
        }

        /// <summary>
        /// rollAffixes: the affixes a regular enemy spawns with on round `round` (none before `from`; one with `chance` rising
        /// per round to `maxChance`; a second from `twoFrom` with `twoChance`). `rng` uniform [0, 1). Fills `output`.
        /// </summary>
        public static List<string> Roll(int round, Func<double> rng, in P a, List<string> output)
        {
            output.Clear();
            if (round < a.from) return output;
            double chance = Math.Min(a.maxChance, a.chance + a.perRound * (round - a.from));
            if (rng() < chance) output.Add(Names[(int)Math.Floor(rng() * Names.Length)]);
            if (output.Count > 0 && round >= a.twoFrom && rng() < a.twoChance)
            {
                // AFFIXES.filter((x) => x !== out[0]): the four others in order
                int skip = Array.IndexOf(Names, output[0]);
                int pick = (int)Math.Floor(rng() * (Names.Length - 1));
                output.Add(Names[pick >= skip ? pick + 1 : pick]);
            }
            return output;
        }

        /// <summary>frontHit: is `from` in front of an enemy at `pos` facing `yaw` (forward = -z at yaw 0), within the plate's half-angle?</summary>
        public static bool FrontHit(Vec3 pos, double yaw, Vec3 from, double halfDeg)
        {
            double fx = -JsMath.Sin(yaw);
            double fz = -JsMath.Cos(yaw);
            double dx = from.x - pos.x;
            double dz = from.z - pos.z;
            double d = JsMath.Hypot(dx, dz);
            if (d == 0) d = 1;
            return (dx * fx + dz * fz) / d >= JsMath.Cos(halfDeg * Math.PI / 180);
        }
    }

    /// <summary>affixBlock / affixBreak {id} (one reused payload).</summary>
    public sealed class AffixEvent
    {
        public int id;
    }

    /// <summary>
    /// enemies.js damage()'s filter chain for a player's hit (the hook weapons.js damage() runs before the enemy's HP):
    /// SHIELDED's plate (affixes.js filter), then the kind's damageMult (charger.js: the back weak point). Static and pure
    /// but for the plate it breaks and the events it emits; an enemy with no plate that is no charger gets `amount` back
    /// unchanged (NIGHT18's hits, bit for bit).
    /// </summary>
    public static class EnemyHits
    {
        public const string BlockEvent = "affixBlock", BreakEvent = "affixBreak";
        static readonly AffixEvent said = new AffixEvent();

        /// <summary>
        /// The damage that lands on `e` from a hit of `amount` (already every multiplier of weapons.js damage()), `blocked`
        /// when the plate stopped it (no hit at all: no points, style, regen or camp relief). `from`: where the hit came
        /// from (weapons.js info.from); `melee`: a punch or a melee weapon (ArsenalMath.IsMelee); `events`: the bus for
        /// affixBlock / affixBreak / plateBreak (null: none).
        /// </summary>
        public static double Filter(Enemy e, double amount, bool headshot, string weapon, bool melee, Vec3 from, EventBus events, out bool blocked, double momentum = 0, double wardenSoft = 0)
        {
            // NIGHT20 A1: the BULWARK's damage() (Core/Enemies/Bulwark.cs: the part's multiplier, a plate's HP, armour-piercing
            // by weapon or `momentum`, weapons.js's M; blocked in its intro, beat and finisher). bulwark.js punchTargets: () => []:
            // no punch or melee weapon reaches it on the web, so a melee hit that finds its body here lands nothing.
            if (e is BulwarkBody bb)
            {
                if (melee)
                {
                    blocked = true;
                    return 0;
                }
                return bb.system.Filter(amount, weapon, momentum, from, out blocked);
            }
            // NIGHT24 B: the KILN WYRM's damage (Core/Enemies/Wyrm.cs): the part's multiplier (body x1, head x2, exposed core x3); blocked in its intro, beat and finisher
            if (e is WyrmBody wyb) return wyb.system.Filter(amount, weapon, out blocked); // NIGHT31 B: the weapon reaches the worm (design.n31.wyrm31 filter31; null with the row off)
            if (e is IBossBody27 b27) return b27.Filter(amount, weapon, from, out blocked); // NIGHT27 B: THE LANTERN's shield (lit face only) and the pulse-only chase's window
            if (e is GlacierBody glb) return glb.system.Filter(amount, from, out blocked); // NIGHT26 B: THE GLACIER (hull x1, engine x2, the close-range bonus; blocked beyond the gap line)
            if (e is HulkBody hlb) return hlb.system.Filter(amount, from, out blocked); // NIGHT26 B: THE SLAG HULK (back and head x2, stunned x1.5; blocked in its intro, beat and finisher)
            if (e is RivalBody rvb) return rvb.system.Filter(amount, headshot, out blocked); // NIGHT25 W: RIVAL PRIME (blocked in its intro, beat and finisher)
            if (e is MonolithBody mnb) return mnb.system.Filter(amount, out blocked); // NIGHT24 leftover 8: MONOLITH (core x1.6, kneeling x3, body x0.5)
            blocked = false;
            if (e.Shielded && e is Husk h && AffixMath.FrontHit(h.position, h.yaw, from, h.plateHalfDeg))
            {
                said.id = e.id;
                if (headshot || melee)
                {
                    e.Shielded = false;
                    events?.Emit(BreakEvent, said);
                    events?.Emit("plateBreak", e.id); // the shared commit's port-only name (N19Events.PlateBreak)
                    if (!melee) return 0; // the headshot spends itself on the plate
                }
                else
                {
                    blocked = true;
                    events?.Emit(BlockEvent, said);
                    return 0;
                }
            }
            if (e is Warden w && w.Absorbs(from, weapon)) // NIGHT20 C4: warden.js damageMult 0 (enemyKind.js: enemyBlocked, no hit)
            {
                if (wardenSoft <= 0)
                {
                    blocked = true;
                    return 0;
                }
                amount *= wardenSoft < 1 ? wardenSoft * w.Attack.frontSoft40 : wardenSoft; // NIGHT26 weaponIdentity: the shield softens a front hit (x0.2; the rail and a punch land it whole), it no longer stops it; NIGHT40 E3 foes40: a softened hit x frontSoft40 (1 off: exact; the full weapons untouched)
            }
            if (e is Charger c) amount *= c.DamageMult(from) * c.WeaponMult47(weapon, from); // enemyKind.js damage: e.hp -= amount * mult; NIGHT47 B charger47: the weapon's side (1 off)
            return amount;
        }
    }

    /// <summary>NIGHT20 C3: one affixed enemy's record (affixes.js enemy.affix): its affixes in order (the icons' row), the
    /// calm and blink clocks (steps), its spawn HP (REGENERATING's cap).</summary>
    public sealed class AffixRec
    {
        public Husk h;
        public int id;
        public readonly List<string> names = new List<string>(2);
        public bool volatileOn, blinker, regenerating, swift;
        public int calm, blink;
        public double max;
    }

    /// <summary>NIGHT20 C3: affixBlink {id, position (from), to}, affixVolatile {id, position, radius, time (the fuse)},
    /// affixBurst {id, position, radius} (one reused payload: read it inside the handler).</summary>
    public sealed class AffixFxEvent
    {
        public int id;
        public Vec3 position, to;
        public double radius, time;
    }

    /// <summary>
    /// The run's affixes (affixes.js createAffixes' state, SHIELDED's part): the round (setRound), the roll's stream, the
    /// tracked enemies (id -> the affixes they spawned with, for the kill bonus), and the stats the harness reads.
    /// </summary>
    public sealed class Affixes
    {
        readonly AffixMath.P a;
        Mulberry32 rng = new Mulberry32(1);
        readonly Func<double> next;
        readonly List<string> rolled = new List<string>();
        /// <summary>The live affixed enemies: id -> their ported affixes' count (the kill bonus).</summary>
        readonly Dictionary<int, int> tracked = new Dictionary<int, int>();
        public int round;
        public int assigned, broken, dropped; // stats: plates assigned, plates broken (the bus counts them), rolls not ported

        public Affixes(GameConfig cfg)
        {
            a = AffixMath.P.From(cfg);
            next = () => rng.Next();
        }

        public AffixMath.P Params => a;

        /// <summary>A new run: the roll's stream from the run seed, nothing tracked, round 0 (main.js startSurvival: setRound(0)).</summary>
        public void NewRun(uint seed)
        {
            rng = new Mulberry32(seed);
            tracked.Clear();
            round = 0;
            assigned = broken = dropped = 0;
            for (int i = live.Count - 1; i >= 0; i--) Untrack(live[i].id); // NIGHT20 C3: nothing tracked, nothing armed
            bursts.Clear();
        }

        /// <summary>affixes.js onSpawn: roll (or force) this enemy's affixes; the ported ones are set on it.</summary>
        public void OnSpawn(Husk h, IList<string> forced = null)
        {
            h.Shielded = false;
            IList<string> list = forced ?? AffixMath.Roll(round, next, a, rolled);
            Assign(h, list);
        }

        /// <summary>affixes.js assign: SHIELDED raises the plate (its half-angle from config); the others are dropped (not ported).</summary>
        public void Assign(Husk h, IList<string> list)
        {
            if (all)
            {
                // NIGHT20 C3: every affix plays (affixes.js assign)
                if (recOf.ContainsKey(h.id)) Untrack(h.id);
                if (list.Count == 0)
                {
                    tracked.Remove(h.id);
                    return;
                }
                if (list.Contains("shielded"))
                {
                    h.Shielded = true;
                    h.plateHalfDeg = a.shieldHalfDeg;
                    assigned++;
                }
                if (Track(h, list).swift) h.paceMult = swiftMult; // affixes.js: e.speedMult = def.mult (brain.js's pace)
                tracked[h.id] = list.Count;
                return;
            }
            int n = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (!AffixMath.Ported(list[i]))
                {
                    dropped++;
                    continue;
                }
                n++;
                if (list[i] == "shielded")
                {
                    h.Shielded = true;
                    h.plateHalfDeg = a.shieldHalfDeg;
                    assigned++;
                }
            }
            if (n > 0) tracked[h.id] = n;
            else tracked.Remove(h.id);
        }

        /// <summary>NIGHT20 fix EN2 (the run snapshot, at a break): the roll's stream, the round, the stats, the tracked kill
        /// bonuses and the armed VOLATILE bursts (a burst armed by a round's last kill goes off in the break). The live records
        /// are the living affixed enemies' (none at a break).</summary>
        public void SaveState(StateWriter w)
        {
            w.I("rng", rng.State);
            w.I("round", round);
            w.I("assigned", assigned);
            w.I("broken", broken);
            w.I("dropped", dropped);
            var ids = new List<int>(tracked.Count);
            var counts = new List<int>(tracked.Count);
            foreach (var kv in tracked)
            {
                ids.Add(kv.Key);
                counts.Add(kv.Value);
            }
            w.Ints("trackedIds", ids);
            w.Ints("trackedCounts", counts);
            var at = new List<Vec3>(bursts.Count);
            var steps = new List<int>(bursts.Count);
            var bid = new List<int>(bursts.Count);
            foreach (Burst b in bursts)
            {
                at.Add(b.at);
                steps.Add(b.steps);
                bid.Add(b.id);
            }
            w.Vecs("burstAt", at);
            w.Ints("burstSteps", steps);
            w.Ints("burstIds", bid);
        }

        public void LoadState(StateReader r)
        {
            rng.State = r.U("rng");
            round = r.Int("round");
            assigned = r.Int("assigned");
            broken = r.Int("broken");
            dropped = r.Int("dropped");
            tracked.Clear();
            List<int> ids = r.Ints("trackedIds"), counts = r.Ints("trackedCounts");
            for (int i = 0; i < ids.Count; i++) tracked[ids[i]] = counts[i];
            bursts.Clear();
            List<Vec3> at = r.Vecs("burstAt");
            List<int> steps = r.Ints("burstSteps"), bid = r.Ints("burstIds");
            for (int i = 0; i < at.Count; i++) bursts.Add(new Burst { at = at[i], steps = steps[i], id = bid[i] });
        }

        /// <summary>affixes.js onKill: the bonus for an affixed kill (bonusPoints x its affixes; 0: none), and it's untracked.</summary>
        public double OnKill(int id)
        {
            if (all && recOf.TryGetValue(id, out AffixRec rec))
            {
                if (rec.volatileOn) Arm(rec); // NIGHT20 C3: VOLATILE bursts where it fell
                Untrack(id);
            }
            if (!tracked.TryGetValue(id, out int n)) return 0;
            tracked.Remove(id);
            return a.bonusPoints * n;
        }

        /// <summary>Is this enemy tracked (an affixed spawn still alive)?</summary>
        public bool Tracked(int id) => tracked.ContainsKey(id);

        // NIGHT20 U3-34.9 C3 (builder E; design.adds.affixes): the rest of affixes.js, every one ported as it is:
        // - VOLATILE: an affixed kill arms a burst where it fell (`affixVolatile` {id, position, radius, time}: the web's
        //   telegraph.circle, the tell), volatile.delay s later volatile.damage to a player within volatile.radius m (3D, from
        //   the burst's point to the player's feet, as the web's) and `affixBurst` {id, position};
        // - BLINKER: every blinker.every x (0.8 .. 1.2) s (the first within 0.3 .. 0.7) it blinks to a free spot a short way
        //   off (blinkTarget: forward up to min(distance x 0.6, d - 3), sideways distance x 0.8, nothing in the way, floor
        //   within 2.5 m) through enemies.teleport (Husk.Teleport): `affixBlink` {id, position (from), to};
        // - REGENERATING: out of damage for regenerating.delay s it heals regenerating.perSecond x its spawn HP a second back to
        //   that HP (affixes.filter's calm: every hit and every blocked hit restarts it);
        // - SWIFT: affixes.js sets e.speedMult = mult (1.35); brain.js's pace reads it: Husk.paceMult, x the brain's speed cap
        //   for that enemy only (1 at every spawn).
        // The rolls (the blink windows, the side) draw the affix stream as the web's assign / blinkTarget draw Math.random.
        // `all` off: NIGHT19's SHIELDED only (the rest dropped after the roll), nothing here runs.
        public bool all;
        double volRadius, volDamage, volDelay, blinkEvery, blinkDistance, regenPerSecond, regenDelay, swiftMult, dt = 1.0 / 120;
        EventBus bus;
        ICollisionWorld world;
        readonly List<AffixRec> live = new List<AffixRec>();
        readonly Dictionary<int, AffixRec> recOf = new Dictionary<int, AffixRec>();
        readonly Stack<AffixRec> spare = new Stack<AffixRec>();
        readonly List<Burst> bursts = new List<Burst>();
        readonly AffixFxEvent fx = new AffixFxEvent();

        struct Burst
        {
            public Vec3 at;
            public int steps, id;
        }

        /// <summary>The affixed enemies alive with the row on (the views: their icons, in order).</summary>
        public IReadOnlyList<AffixRec> Live => live;

        /// <summary>The bursts armed now (the views: the VOLATILE circles filling).</summary>
        public int BurstCount => bursts.Count;

        /// <summary>SWIFT's pace for an enemy (1: none): the brain's per-enemy line reads it once N adds it.</summary>
        public double SpeedMultOf(int id) => recOf.TryGetValue(id, out AffixRec r) && r.swift ? swiftMult : 1;

        /// <summary>The bus and the collider (the sim, once), and the kinds' numbers (config.affixes.kinds).</summary>
        public void Wire(EventBus events, ICollisionWorld collider, GameConfig cfg)
        {
            bus = events;
            world = collider;
            dt = cfg.Dt;
            volRadius = cfg.Num("affixes.kinds.volatile.radius");
            volDamage = cfg.Num("affixes.kinds.volatile.damage");
            volDelay = cfg.Num("affixes.kinds.volatile.delay");
            blinkEvery = cfg.Num("affixes.kinds.blinker.every");
            blinkDistance = cfg.Num("affixes.kinds.blinker.distance");
            regenPerSecond = cfg.Num("affixes.kinds.regenerating.perSecond");
            regenDelay = cfg.Num("affixes.kinds.regenerating.delay");
            swiftMult = cfg.Num("affixes.kinds.swift.mult");
            // affixes.filter: any damage restarts REGENERATING's calm (a landed hit, or one the plate blocked)
            events.On<HitEvent>("hit", e => Calm(e.enemy));
            events.On<AffixEvent>(EnemyHits.BlockEvent, e => Calm(e.id));
        }

        void Calm(int id)
        {
            if (all && recOf.TryGetValue(id, out AffixRec r)) r.calm = MovementMath.StepsFor(regenDelay, dt);
        }

        AffixRec Track(Husk h, IList<string> list)
        {
            AffixRec r = spare.Count > 0 ? spare.Pop() : new AffixRec();
            r.h = h;
            r.id = h.id;
            r.names.Clear();
            for (int i = 0; i < list.Count; i++) r.names.Add(list[i]);
            r.volatileOn = list.Contains("volatile");
            r.blinker = list.Contains("blinker");
            r.regenerating = list.Contains("regenerating");
            r.swift = list.Contains("swift");
            r.calm = 0;
            r.blink = MovementMath.StepsFor(blinkEvery * (0.3 + 0.4 * next()), dt); // affixes.js assign: rng() for every enemy assigned
            r.max = h.hp;
            Untrack(h.id);
            live.Add(r);
            recOf[h.id] = r;
            return r;
        }

        void Untrack(int id)
        {
            if (!recOf.TryGetValue(id, out AffixRec r)) return;
            recOf.Remove(id);
            live.Remove(r);
            r.h = null;
            spare.Push(r);
        }

        /// <summary>affixes.js blinkTarget: a free spot a short blink away (floor under it, nothing in the way).</summary>
        bool BlinkTarget(Husk e, Vec3 t, out Vec3 to)
        {
            double toX = t.x - e.position.x, toZ = t.z - e.position.z;
            double d = JsMath.Hypot(toX, toZ);
            if (d == 0) d = 1;
            double side = next() < 0.5 ? -1 : 1;
            double fwd = Math.Min(blinkDistance * 0.6, Math.Max(0, d - 3));
            to = new Vec3(e.position.x + toX / d * fwd - toZ / d * side * blinkDistance * 0.8, e.position.y + 1, e.position.z + toZ / d * fwd + toX / d * side * blinkDistance * 0.8);
            var from = new Vec3(e.position.x, e.position.y + 1, e.position.z);
            Vec3 dir = to - from;
            double len = dir.Length;
            dir = Vec3.DivideScalar(dir, len > 0 ? len : 1);
            if (world.RayIntersect(from, dir, out RayHit wall) && wall.distance < len) return false;
            if (!world.RayIntersect(to, new Vec3(0, -1, 0), out RayHit floor) || floor.distance > 2.5) return false;
            to = new Vec3(to.x, to.y - floor.distance, to.z);
            return true;
        }

        /// <summary>affixes.js step: the calm, REGENERATING, BLINKER; the armed VOLATILE bursts (the player at `target`).</summary>
        public void Step(Vec3 target, Health targetHealth, double stepDt)
        {
            if (!all) return;
            for (int i = live.Count - 1; i >= 0; i--)
            {
                AffixRec r = live[i];
                Husk e = r.h;
                if (!e.alive || e.id != r.id)
                {
                    Untrack(r.id);
                    continue;
                }
                if (r.calm > 0) r.calm--;
                if (r.regenerating && r.calm == 0 && e.hp < r.max) e.hp += Math.Min(r.max - e.hp, r.max * regenPerSecond * stepDt);
                if (r.blinker && --r.blink <= 0)
                {
                    r.blink = MovementMath.StepsFor(blinkEvery * (0.8 + 0.4 * next()), stepDt);
                    if (BlinkTarget(e, target, out Vec3 to))
                    {
                        fx.id = e.id;
                        fx.position = e.position;
                        fx.to = to;
                        fx.radius = 0;
                        fx.time = 0;
                        e.Teleport(to); // enemies.teleport
                        bus?.Emit("affixBlink", fx);
                    }
                }
            }
            for (int i = bursts.Count - 1; i >= 0; i--)
            {
                Burst b = bursts[i];
                if (--b.steps > 0)
                {
                    bursts[i] = b;
                    continue;
                }
                bursts.RemoveAt(i);
                if (JsMath.Hypot(target.x - b.at.x, target.y - b.at.y, target.z - b.at.z) <= volRadius && !targetHealth.Dead)
                    targetHealth.Damage(volDamage, "affix", b.at); // health.damage(k.damage, {type: 'affix', attack: 'volatile'})
                fx.id = b.id;
                fx.position = b.at;
                fx.to = b.at;
                fx.radius = volRadius;
                fx.time = 0;
                bus?.Emit("affixBurst", fx);
            }
        }

        /// <summary>VOLATILE's kill (affixes.js onKill): the tell and the burst armed where it fell.</summary>
        void Arm(AffixRec r)
        {
            Vec3 p = r.h.position;
            bursts.Add(new Burst { at = p, steps = MovementMath.StepsFor(volDelay, dt), id = r.id });
            fx.id = r.id;
            fx.position = p;
            fx.to = p;
            fx.radius = volRadius;
            fx.time = volDelay;
            bus?.Emit("affixVolatile", fx);
        }
    }
}
