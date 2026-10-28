// NIGHT14 U3-27.9 (agent V; the record structs from the shared types): WHAT GOT YOU's flight recorder (design.recap;
// DESIGN-PILLARS 3, every run is readable). A ring preallocated at construction for design.recap.seconds of fixed steps
// (6 s at 120 Hz: 720 frames) of view state, written after each step (SurvivalSim.StepEnded) while a run plays: the sim
// time, the player's feet, look and HP, every live enemy (id, kind, feet, yaw, its wind-up / stagger / flash bits; at
// most MaxEnemies a frame, the registry's order) and the marks (this step's hits, kills and hurts from the events, then
// the spitters' globs and puddles, the kill drops, the player's rockets and nails; at most MaxMarks a frame). The death's
// step is the last frame (the ring freezes in DOWN; a new run clears it).
// The killer: each player hurt is resolved to the enemy that dealt it, and the death latches the last one (Health emits
// "hurt" then "death" from the same Damage call). A husk's and a swarmer's blow come from their own feet (Husk.ResolveAttack,
// Swarmer's bite: `from` = position), so the enemy of the source's kind standing exactly there is the one (Exact); a glob
// carries its shooter (Spitter.Run's `from`, the hit's arc, matched exactly; else the live glob nearest you) and a puddle
// too (the one you stand in, Spitter.Run's test) (Shooter). Fallbacks, in order: the nearest live enemy of the source's
// kind to the blow's origin (NearestOfKind), the nearest live enemy to you (Nearest). THE FALL has none.
// Nothing is allocated after construction: fixed arrays of structs with no references (the GC never scans them; a kind is
// a code, RecapKind.Name gives its string back), cached delegates, indexed loops, no config read per step. The recorder
// never writes the sim (an observer: the fingerprint is unchanged). No UnityEngine use.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>An enemy's kind as a code (the rings hold no references: the GC never scans them).</summary>
    public static class RecapKind
    {
        public const int Husk = 0, Swarmer = 1, Spitter = 2, Other = 3;
        static readonly string[] Names = { "husk", "swarmer", "spitter", "other" };

        /// <summary>The kind's string (the enemies' own kind names; "other" for a kind the port draws as a husk).</summary>
        public static string Name(int kind) => kind >= 0 && kind < Names.Length ? Names[kind] : Names[Other];

        /// <summary>An enemy's code (type checks on the sealed kinds, the husk's string last).</summary>
        public static int Of(Enemy e) => e is Swarmer ? Swarmer : e is Spitter ? Spitter : e.kind == "husk" ? Husk : Other;
    }

    /// <summary>A recorded enemy's state bits.</summary>
    public static class RecapState
    {
        /// <summary>Its attack tell: a husk's wind-up, a swarmer's crouch, a spitter's tell (phase: the tell's share done).</summary>
        public const int Windup = 1;
        public const int Stagger = 2;
        /// <summary>A hit's white flash (the views' flash timer).</summary>
        public const int Flash = 4;
        /// <summary>Off the ground (a leap, a pounce).</summary>
        public const int Airborne = 8;
    }

    /// <summary>What a mark is.</summary>
    public static class RecapMarkKind
    {
        public const int Glob = 1, ReflectedGlob = 2, Puddle = 3, Nail = 4, Rocket = 5, Drop = 6, Hit = 7, Kill = 8, Hurt = 9;
    }

    /// <summary>How the killer was found (FlightRecorder.Resolve).</summary>
    public enum RecapKillerHow { None, Exact, Shooter, NearestOfKind, Nearest }

    /// <summary>One recorded enemy: its id, kind (RecapKind), feet, yaw and state (RecapState bits; phase 0..1 of a spitter's tell).</summary>
    public struct RecapEnemy
    {
        public int id;
        public int kind;
        public Vec3 position;
        public double yaw;
        public int state;
        public double phase;
    }

    /// <summary>One recorded projectile, pickup or hit: its kind (RecapMarkKind), where, and the enemy (a hit's target, a glob's shooter; -1 none).</summary>
    public struct RecapMark
    {
        public int kind;
        public Vec3 position;
        public int enemyId;
    }

    /// <summary>One recorded step: the sim step and time, the player's feet, look and HP, and the slices of the enemy / mark pools.</summary>
    public struct RecapFrame
    {
        public int step;
        public double time;
        public Vec3 feet;
        public double yaw, pitch;
        public double hp;
        public int enemyStart, enemyCount;
        public int markStart, markCount;
    }

    public sealed class FlightRecorder
    {
        readonly SurvivalSim sim;
        /// <summary>Frames the ring holds (design.recap.seconds of steps), enemy slots and mark slots a frame.</summary>
        public readonly int Capacity, MaxEnemies, MaxMarks;
        readonly RecapFrame[] frames;
        readonly RecapEnemy[] enemies; // frame slot s owns [s * MaxEnemies, +MaxEnemies)
        readonly RecapMark[] marks; // frame slot s owns [s * MaxMarks, +MaxMarks)
        readonly RecapMark[] pending; // this step's hits / kills / hurts (the events), flushed into its frame
        int pendingCount;
        int head; // the next slot written
        int count;
        readonly Action record, clear; // cached: a method group per subscription would allocate
        readonly Action<HurtEvent> onHurt;
        readonly Action<DeathEvent> onDeath;
        readonly Action<HitEvent> onHit;
        readonly Action<KillEvent> onKill;
        Action offHurt, offDeath, offHit, offKill;
        int lastHurtEnemy = -1;
        RecapKillerHow lastHurtHow;
        string lastHurtSource;

        /// <summary>A ring of `seconds` of steps with the shipped caps (UnityTuning.RecapMaxEnemies / RecapMaxMarks).</summary>
        public FlightRecorder(SurvivalSim sim, double seconds) : this(sim, seconds, UnityTuning.RecapMaxEnemies, UnityTuning.RecapMaxMarks)
        {
        }

        public FlightRecorder(SurvivalSim sim, double seconds, int maxEnemies, int maxMarks)
        {
            this.sim = sim;
            Capacity = Math.Max(2, (int)Math.Round(seconds / sim.step));
            MaxEnemies = Math.Max(1, maxEnemies);
            MaxMarks = Math.Max(1, maxMarks);
            frames = new RecapFrame[Capacity];
            enemies = new RecapEnemy[Capacity * MaxEnemies];
            marks = new RecapMark[Capacity * MaxMarks];
            pending = new RecapMark[MaxMarks];
            record = Record;
            clear = Clear;
            onHurt = NoteHurt;
            onDeath = NoteDeath;
            onHit = NoteHit;
            onKill = NoteKill;
            Clear();
        }

        /// <summary>design.recap.enabled: false, the game builds no recorder (the old DOWN).</summary>
        public static bool Enabled(GameConfig cfg) => cfg.Design("recap.enabled");

        /// <summary>Frames held (Capacity once full).</summary>
        public int Count => count;

        /// <summary>The death's step was recorded: nothing more until a new run (RunStarted clears).</summary>
        public bool Frozen { get; private set; }

        public bool Attached { get; private set; }

        public double OldestTime => count > 0 ? Frame(0).time : 0;

        public double NewestTime => count > 0 ? Frame(count - 1).time : 0;

        /// <summary>The run's death: the killer's id (-1: none, THE FALL), kind (RecapKind; -1 unknown), how it was found, the hurt's source and the recap's name.</summary>
        public int KillerId { get; private set; }
        public int KillerKind { get; private set; }
        public RecapKillerHow KillerHow { get; private set; }
        public string KillerSource { get; private set; }
        public string KillerName { get; private set; }
        public bool HasDeath { get; private set; }

        /// <summary>Deaths seen since construction (never cleared): the game plays one replay per death.</summary>
        public int DeathSerial { get; private set; }

        /// <summary>The enemy the last player hurt came from (-1 none) and how it was found (RecapPlayNow's killer before a death).</summary>
        public int LastHurtEnemy => lastHurtEnemy;
        public RecapKillerHow LastHurtHow => lastHurtHow;

        /// <summary>Subscribe to the sim: StepEnded (record), RunStarted (clear), and the hurt / death / hit / kill events.</summary>
        public void Attach()
        {
            if (Attached) return;
            Attached = true;
            sim.StepEnded += record;
            sim.RunStarted += clear;
            offHurt = sim.events.On("hurt", onHurt); // EventBus.On<T> wraps each once, here (never per event)
            offDeath = sim.events.On("death", onDeath);
            offHit = sim.events.On("hit", onHit);
            offKill = sim.events.On("kill", onKill);
        }

        /// <summary>Unsubscribe everything (RECAP turned off): the ring keeps what it had until Clear.</summary>
        public void Detach()
        {
            if (!Attached) return;
            Attached = false;
            sim.StepEnded -= record;
            sim.RunStarted -= clear;
            offHurt();
            offDeath();
            offHit();
            offKill();
            offHurt = offDeath = offHit = offKill = null;
        }

        /// <summary>Empty the ring and forget the death (a new run).</summary>
        public void Clear()
        {
            head = count = pendingCount = 0;
            Frozen = false;
            lastHurtEnemy = -1;
            lastHurtHow = RecapKillerHow.None;
            lastHurtSource = null;
            KillerId = -1;
            KillerKind = -1;
            KillerHow = RecapKillerHow.None;
            KillerSource = null;
            KillerName = null;
            HasDeath = false;
        }

        /// <summary>Record the state after a step (SurvivalSim.StepEnded): only while a run plays, and the death's step last.</summary>
        public void Record()
        {
            if (Frozen) return;
            Flow flow = sim.flow;
            if (flow != Flow.Playing && flow != Flow.Down)
            {
                pendingCount = 0;
                return;
            }
            if (flow == Flow.Down) Frozen = true; // the death's step: the ring's last frame
            int slot = head;
            ref RecapFrame f = ref frames[slot];
            f.step = sim.simStep;
            f.time = sim.SimTime;
            f.feet = sim.current;
            IPlayerInput input = sim.input;
            f.yaw = input.Yaw;
            f.pitch = input.Pitch;
            f.hp = sim.health.hp;

            int eBase = slot * MaxEnemies, n = 0;
            List<Enemy> all = sim.enemies.all;
            for (int i = 0; i < all.Count && n < MaxEnemies; i++)
            {
                Enemy e = all[i];
                if (!e.alive) continue;
                ref RecapEnemy r = ref enemies[eBase + n++];
                r.id = e.id;
                r.kind = RecapKind.Of(e);
                r.position = e.position;
                r.yaw = e.yaw;
                r.state = StateOf(e, out r.phase);
            }
            f.enemyStart = eBase;
            f.enemyCount = n;

            int mBase = slot * MaxMarks, m = 0;
            for (int i = 0; i < pendingCount && m < MaxMarks; i++) marks[mBase + m++] = pending[i];
            pendingCount = 0;
            SpitterAcid acid = sim.husks.acid;
            SpitterAcid.Glob[] globs = acid.globs;
            for (int i = 0; i < globs.Length && m < MaxMarks; i++)
            {
                SpitterAcid.Glob g = globs[i];
                if (g.alive) Put(mBase + m++, g.reflected ? RecapMarkKind.ReflectedGlob : RecapMarkKind.Glob, g.position, g.shooter != null ? g.shooter.id : -1);
            }
            SpitterAcid.Puddle[] puddles = acid.puddles;
            for (int i = 0; i < puddles.Length && m < MaxMarks; i++)
            {
                SpitterAcid.Puddle pd = puddles[i];
                if (pd.alive) Put(mBase + m++, RecapMarkKind.Puddle, pd.position, pd.shooter != null ? pd.shooter.id : -1);
            }
            PowerUps.Drop[] drops = sim.powerUps.drops;
            for (int i = 0; i < drops.Length && m < MaxMarks; i++)
                if (drops[i].active) Put(mBase + m++, RecapMarkKind.Drop, drops[i].position, -1);
            Projectiles.Projectile[] rockets = sim.projectiles.rockets;
            for (int i = 0; i < rockets.Length && m < MaxMarks; i++)
                if (rockets[i].alive) Put(mBase + m++, RecapMarkKind.Rocket, rockets[i].position, -1);
            Projectiles.Projectile[] nails = sim.projectiles.nails;
            for (int i = 0; i < nails.Length && m < MaxMarks; i++)
                if (nails[i].alive) Put(mBase + m++, RecapMarkKind.Nail, nails[i].position, -1);
            f.markStart = mBase;
            f.markCount = m;

            head = slot + 1 == Capacity ? 0 : slot + 1;
            if (count < Capacity) count++;
        }

        void Put(int index, int kind, Vec3 at, int enemyId)
        {
            ref RecapMark mk = ref marks[index];
            mk.kind = kind;
            mk.position = at;
            mk.enemyId = enemyId;
        }

        /// <summary>An enemy's state bits (and a spitter's tell share in `phase`).</summary>
        static int StateOf(Enemy e, out double phase)
        {
            phase = 0;
            int s = 0;
            if (e.windupSteps > 0) s |= RecapState.Windup;
            if (e.staggerSteps > 0) s |= RecapState.Stagger;
            if (e.flash > 0) s |= RecapState.Flash;
            if (e is Swarmer sw)
            {
                if (sw.state == "crouch") s |= RecapState.Windup;
            }
            else if (e is Spitter sp && sp.state == "tell")
            {
                s |= RecapState.Windup;
                phase = Math.Min(1, 1 - sp.stateSteps / (double)Math.Max(1, sp.tellSteps));
            }
            if (e is Husk h && !h.grounded) s |= RecapState.Airborne;
            return s;
        }

        int SlotOf(int i)
        {
            int s = head - count + i;
            return s < 0 ? s + Capacity : s;
        }

        /// <summary>Frame `i` (0 the oldest, Count - 1 the newest).</summary>
        public ref readonly RecapFrame Frame(int i) => ref frames[SlotOf(i)];

        /// <summary>A frame's k-th enemy.</summary>
        public ref readonly RecapEnemy Enemy(in RecapFrame f, int k) => ref enemies[f.enemyStart + k];

        /// <summary>A frame's k-th mark.</summary>
        public ref readonly RecapMark Mark(in RecapFrame f, int k) => ref marks[f.markStart + k];

        /// <summary>The index in frame `f` of enemy `id` (-1: not in it), searched from `hint` (the same index a frame earlier, usually).</summary>
        public int FindEnemy(in RecapFrame f, int id, int hint = 0)
        {
            int n = f.enemyCount;
            if (n == 0) return -1;
            if (hint < 0 || hint >= n) hint = 0;
            for (int k = 0; k < n; k++)
            {
                int i = hint + k;
                if (i >= n) i -= n;
                if (enemies[f.enemyStart + i].id == id) return i;
            }
            return -1;
        }

        /// <summary>The last frame at or before sim time `time` (clamped to the ring; -1 when empty).</summary>
        public int IndexAt(double time)
        {
            if (count == 0) return -1;
            int lo = 0, hi = count - 1;
            if (time <= Frame(0).time) return 0;
            if (time >= Frame(hi).time) return hi;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) >> 1;
                if (Frame(mid).time <= time) lo = mid;
                else hi = mid - 1;
            }
            return lo;
        }

        /// <summary>
        /// The enemy a player hurt came from (-1: none) and how it was found: Exact (a husk's / swarmer's feet), Shooter (a
        /// glob's or puddle's), NearestOfKind (the source's kind nearest the blow's origin), Nearest (any enemy, nearest
        /// you), None (THE FALL, or no enemy alive).
        /// </summary>
        public int Resolve(string source, Vec3 from, bool hasFrom, out RecapKillerHow how)
        {
            how = RecapKillerHow.None;
            if (source == "fall") return -1;
            Vec3 me = sim.mover.position;
            List<Enemy> all = sim.enemies.all;
            if (source == "spitter")
            {
                // Spitter.Run: the hit's `from` is the glob's position less 0.2 s of its level velocity (exact), else the
                // live glob nearest you (it touches your capsule).
                SpitterAcid.Glob[] globs = sim.husks.acid.globs;
                SpitterAcid.Glob near = null;
                double nearD = double.PositiveInfinity;
                for (int i = 0; i < globs.Length; i++)
                {
                    SpitterAcid.Glob g = globs[i];
                    if (!g.alive || g.reflected || g.shooter == null) continue;
                    if (hasFrom && GlobFrom(g) == from)
                    {
                        how = RecapKillerHow.Shooter;
                        return g.shooter.id;
                    }
                    double d = DistSq(g.position, me);
                    if (d < nearD)
                    {
                        nearD = d;
                        near = g;
                    }
                }
                if (near != null)
                {
                    how = RecapKillerHow.Shooter;
                    return near.shooter.id;
                }
            }
            else if (source == "acid")
            {
                // Spitter.Run's acid: the last live puddle (pool order) you stand in, else the nearest live one.
                SpitterAcid acid = sim.husks.acid;
                SpitterAcid.Puddle[] puddles = acid.puddles;
                SpitterAcid.Puddle inAcid = null, near = null;
                double nearD = double.PositiveInfinity, r = acid.p.puddleRadius;
                for (int i = 0; i < puddles.Length; i++)
                {
                    SpitterAcid.Puddle pd = puddles[i];
                    if (!pd.alive || pd.shooter == null) continue;
                    double dx = me.x - pd.position.x, dz = me.z - pd.position.z;
                    if (Math.Sqrt(dx * dx + dz * dz) <= r && Math.Abs(me.y - pd.position.y) < 1.2) inAcid = pd;
                    double d = DistSq(pd.position, me);
                    if (d < nearD)
                    {
                        nearD = d;
                        near = pd;
                    }
                }
                SpitterAcid.Puddle hit = inAcid ?? near;
                if (hit != null)
                {
                    how = RecapKillerHow.Shooter;
                    return hit.shooter.id;
                }
            }
            else if (source != null && hasFrom)
            {
                // Husk.ResolveAttack / the swarmer's bite: the blow comes from the attacker's own feet.
                for (int i = 0; i < all.Count; i++)
                {
                    Enemy e = all[i];
                    if (e.alive && e.position == from && e.kind == source)
                    {
                        how = RecapKillerHow.Exact;
                        return e.id;
                    }
                }
            }
            string kind = source == "acid" ? "spitter" : source;
            int id = kind != null ? Nearest(all, kind, hasFrom ? from : me) : -1;
            if (id >= 0)
            {
                how = RecapKillerHow.NearestOfKind;
                return id;
            }
            id = Nearest(all, null, me);
            if (id >= 0) how = RecapKillerHow.Nearest;
            return id;
        }

        /// <summary>The nearest live enemy to `at` (of `kind`, or any when null); -1 none.</summary>
        static int Nearest(List<Enemy> all, string kind, Vec3 at)
        {
            int best = -1;
            double bestD = double.PositiveInfinity;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (!e.alive || (kind != null && e.kind != kind)) continue;
                double d = DistSq(e.position, at);
                if (d < bestD)
                {
                    bestD = d;
                    best = e.id;
                }
            }
            return best;
        }

        static double DistSq(Vec3 a, Vec3 b)
        {
            double dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>A glob hit's `from` as Spitter.Run computes it (the damage-direction arc's origin).</summary>
        public static Vec3 GlobFrom(SpitterAcid.Glob g) => new Vec3(g.position.x - g.velocity.x * 0.2, g.position.y, g.position.z - g.velocity.z * 0.2);

        /// <summary>The "hurt" event: a player hurt resolved to its enemy (the killer if a death follows) and marked.</summary>
        public void NoteHurt(HurtEvent e)
        {
            if (e.owner != "player" || !(e.amount > 0)) return;
            lastHurtSource = e.source;
            lastHurtEnemy = Resolve(e.source, e.from, e.hasFrom, out lastHurtHow);
            AddPending(RecapMarkKind.Hurt, e.hasFrom ? e.from : sim.mover.position, lastHurtEnemy);
        }

        /// <summary>The "death" event (after its hurt, from the same Damage call): the killer latched, once a run.</summary>
        public void NoteDeath(DeathEvent e)
        {
            if (e.owner != "player" || HasDeath) return;
            HasDeath = true;
            DeathSerial++;
            KillerSource = e.source;
            if (e.source == lastHurtSource)
            {
                KillerId = lastHurtEnemy;
                KillerHow = lastHurtHow;
            }
            else
            {
                KillerId = Resolve(e.source, sim.mover.position, false, out RecapKillerHow how);
                KillerHow = how;
            }
            KillerKind = -1;
            List<Enemy> all = sim.enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].id != KillerId) continue;
                KillerKind = RecapKind.Of(all[i]);
                break;
            }
            KillerName = sim.lastDeath?.killer; // the recap's name (SurvivalSim's death handler ran first)
        }

        /// <summary>The "hit" event: the player's hit on an enemy, marked.</summary>
        public void NoteHit(HitEvent e)
        {
            if (e.owner == "player") AddPending(RecapMarkKind.Hit, e.position, e.enemy);
        }

        /// <summary>The "kill" event: the player's kill, marked.</summary>
        public void NoteKill(KillEvent e)
        {
            if (e.owner == "player") AddPending(RecapMarkKind.Kill, e.position, e.enemy);
        }

        void AddPending(int kind, Vec3 at, int enemyId)
        {
            if (pendingCount >= pending.Length) return;
            ref RecapMark mk = ref pending[pendingCount++];
            mk.kind = kind;
            mk.position = at;
            mk.enemyId = enemyId;
        }
    }
}
