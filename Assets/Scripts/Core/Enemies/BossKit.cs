// NIGHT20 U3-34.9 A1 (builder EN): src/enemies/bossKit.js ported as it is: "The boss framework (OVERNIGHT2 N5), shared by
// BULWARK, MONOLITH, RIVAL PRIME, ORBITER and HIVE: which boss a round brings (the schedule, the rotation + its mutators),
// HP / damage scaling (round, cycle, mutator, coop factor 1), the intro card and the phase change beat (events the HUD's
// boss card draws), the finisher slow-mo and the reward table (main listens to bossDying / bossDown), the shared telegraph
// primitives (fx/telegraph.js) and the boss bar. Every boss system has start(ctx), clear(), bar(), `alive` and the
// enemies facade's target API. Fixed step." The port has one boss so far (the BULWARK); the others are still the web's
// only (a round the schedule gives them stays the director's ordinary round: Survival20 asks ForRound only for a boss
// that is registered). Events keep the web's names and fields: bossIntro {boss, name, subtitle, mutator, time},
// bossBeat {boss, label, position}, bossDying {boss, position}, bossDown {boss, round, position, reward}, and the boss's
// kill {owner player, kind boss, weapon boss}.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The boss events' one payload (reused: read it inside the handler). The web's fields by event: bossIntro
    /// {boss, name, subtitle, mutator, time}; bossBeat {boss, label, position}; bossDying {boss, position}; bossDown {boss,
    /// round, position, reward}; bossRise {boss, name, hp, round}; bossPhase {boss, phase, hp, maxHp}; bossTell {boss, name,
    /// seconds}; bossPart {boss, part, reason, points, position}; bossHit {boss, damage, part, core, hp}; bossStun {boss,
    /// seconds}; bossPunish {boss}.</summary>
    public sealed class BossEvent
    {
        public string boss, name, subtitle, mutator, label, phase, part, reason;
        public double time, seconds, hp, maxHp, damage, points;
        public int round;
        public bool core;
        public Vec3 position;
        public BossReward reward;

        public BossEvent Reset(string bossName)
        {
            boss = bossName;
            name = subtitle = mutator = label = phase = part = reason = null;
            time = seconds = hp = maxHp = damage = points = 0;
            round = 0;
            core = false;
            position = default;
            reward = null;
            return this;
        }
    }

    /// <summary>The running boss's bar (bar(): {name, hp, max, phase, segments, pips}): H's boss bar reads it.</summary>
    public struct BossBar
    {
        public bool valid;
        public int id;
        public string name, phase;
        public double hp, max;
        /// <summary>segments[0]: the phase mark as a share of max HP (BULWARK: p2At 0.5).</summary>
        public double segment;
        /// <summary>pips: the plates (BULWARK: 5), how many are still on.</summary>
        public int pips, pipsOn;
    }

    /// <summary>What a boss aims at (main.js getTarget): the player's feet, health and mover impulse.</summary>
    public sealed class BossTarget
    {
        public Func<Vec3> position;
        public Health health;
        public Action<double, double, double> impulse;
    }

    /// <summary>startFight's ctx: {center, round, cycle, mutator, scale, kit}.</summary>
    public sealed class BossFight
    {
        public Vec3 center;
        public int round, cycle;
        public BossMutator mutator;
        public BossScale scale;
        public BossKit kit;
    }

    /// <summary>A boss system as the kit sees it (start, clear, bar, alive; frozen: the debug freeze).</summary>
    public interface IBossSystem
    {
        bool Alive { get; }
        bool Frozen { get; set; }
        void StartFight(BossFight ctx);
        void Clear();
        BossBar Bar();
    }

    public sealed class BossKit
    {
        public readonly BossKitParams k;
        readonly EventBus events;
        readonly BossTarget target;
        readonly double dt;
        readonly DirectorMath.D rounds;
        readonly BossEvent said = new BossEvent();
        public readonly Dictionary<string, IBossSystem> bosses = new Dictionary<string, IBossSystem>();
        public ITelegraph telegraph;
        string active; // the running boss's name
        BossFight info;

        public BossKit(GameConfig cfg, EventBus events, ITelegraph telegraph, BossTarget target)
        {
            k = BossKitParams.From(cfg);
            this.events = events;
            this.telegraph = telegraph ?? new NullTelegraph();
            this.target = target;
            dt = cfg.Dt;
            rounds = DirectorMath.From(cfg);
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt);

        public string Active => active;

        /// <summary>{boss, round, cycle, mutator, scale} of the running fight.</summary>
        public BossFight Info => info;

        /// <summary>Is a boss fight on (the boss is alive or still dying)?</summary>
        public bool Alive => active != null && bosses.TryGetValue(active, out IBossSystem b) && b.Alive;

        /// <summary>The boss a Survival round brings, or null.</summary>
        public BossDue? ForRound(int round) => BossMath.BossForRound(round, k, rounds);

        /// <summary>A boss's display name.</summary>
        public string NameOf(string boss) => k.names.TryGetValue(boss, out string n) ? n : boss.ToUpperInvariant();

        /// <summary>Start a boss fight in the Grinder: scale it, start it, show the intro card (it walks in, no damage meanwhile).</summary>
        public IBossSystem Start(string boss, int round = 0, int cycle = 0, BossMutator mutator = null, string displayName = null)
        {
            if (!bosses.TryGetValue(boss, out IBossSystem system)) return null;
            Clear();
            BossScale scale = BossMath.Scale(boss, round, cycle, mutator, k);
            system.Frozen = false; // a fight always starts live (a debug freeze never carries into the next one)
            active = boss;
            info = new BossFight { center = k.arena, round = round, cycle = cycle, mutator = mutator, scale = scale, kit = this };
            system.StartFight(info);
            said.Reset(boss);
            said.name = displayName ?? NameOf(boss); // NIGHT25 W: a variant that shares its boss key (the ELDER WYRM) names its own card
            said.subtitle = k.subtitles.TryGetValue(boss, out string sub) ? sub : "";
            said.mutator = mutator?.name;
            said.time = k.introTime;
            events.Emit("bossIntro", said);
            return system;
        }

        /// <summary>Is any boss alive (the kit's fight, or one a debug script started directly)?</summary>
        public bool AnyAlive()
        {
            foreach (var kv in bosses)
                if (kv.Value.Alive) return true;
            return false;
        }

        /// <summary>The running boss's bar (the HUD); valid false when none.</summary>
        public BossBar Bar() => Alive ? bosses[active].Bar() : default;

        /// <summary>Steps of the intro (the boss walks in and takes no damage).</summary>
        public int IntroSteps() => S(k.introTime);

        /// <summary>
        /// The phase change beat: a banner + a white flash at the boss and a knockback pulse away from it (no damage). The boss
        /// holds (invulnerable) for the returned number of steps.
        /// </summary>
        public int Beat(string boss, string label, Vec3 at)
        {
            said.Reset(boss);
            said.label = label;
            said.position = at;
            events.Emit("bossBeat", said);
            telegraph.Flash(at.x, at.y, at.z, 3);
            Vec3 t = target.position();
            double dx = t.x - at.x, dz = t.z - at.z;
            double d = JsMath.Hypot(dx, dz);
            if (d == 0) d = 1;
            target.impulse?.Invoke(dx / d * k.beatPush, k.beatPushUp, dz / d * k.beatPush);
            return S(k.beatTime);
        }

        /// <summary>The finisher: main slows time (bossKit.killSlowMo for killTime).</summary>
        public void Dying(string boss, Vec3 at)
        {
            said.Reset(boss);
            said.position = at;
            events.Emit("bossDying", said);
        }

        /// <summary>The kill: a boss kill event, then the reward (points, a free perk token, a power-up drop: main pays it).</summary>
        public void Down(string boss, int id, Vec3 at)
        {
            // a new payload (once a fight): kill listeners may keep it
            events.Emit("kill", new KillEvent { owner = "player", enemy = id, kind = "boss", weapon = "boss", distance = 20, position = at });
            said.Reset(boss);
            said.round = info?.round ?? 0;
            said.position = at;
            said.reward = k.rewards.TryGetValue(boss, out BossReward r) ? r : null;
            events.Emit("bossDown", said);
            telegraph.Clear();
        }

        /// <summary>End every fight (a restart, a mode switch).</summary>
        public void Clear()
        {
            foreach (var kv in bosses) kv.Value.Clear();
            telegraph.Clear();
            active = null;
            info = null;
        }

        /// <summary>One fixed step (after the enemies): a fight is over once its boss is gone (the boss steps itself, as the
        /// web's enemies facade steps it; bossKit.step's `tick` is a hook no ported boss has).</summary>
        public void Step(double stepDt)
        {
            if (active != null && !(bosses.TryGetValue(active, out IBossSystem b) && b.Alive)) active = null;
        }
    }
}
