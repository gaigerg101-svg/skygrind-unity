// NIGHT17 U3-30.4 (builder M owns this file): one bar, a sprint and a partial boost, pure and tested. GG (2026-10-05): "you
// should start out with only one bar ... you should be able to Sprint and it uses your bar ... you can either choose the
// boost or hold a Sprint ... you can still boost even if the bar is not fully filled, it just won't be as much of a boost".
// Five rules, each behind its own flag in design.stamina (Design/unity-config.json, on by default), read once into Params:
// 1. bar: one continuous bar of `max` (1.0; VIGOR adds bonusBars) replaces the web's dash pips. Every other rule is a rule
//    of the bar: with `bar` off the Mover is the web's whatever the others say (Params.Any is the bar's flag).
// 2. boost: the boost (the web's dash, on the press) costs up to `cost`. With less it still fires, its DURATION scaled by
//    fill / cost (the speed stays dash.speed), never below `floor` of the full boost; a fill of fullAt x cost or more counts
//    as full. Its invulnerable steps are the dash's, so they scale with it. Off: a boost needs the whole cost (the web's
//    rule in bar units). A dash-jump costs dashJumpExtra more (in bar units whenever the bar is on).
// 3. sprint (STAMINA KEY: one key): holding the boost key after its press runs at sprintSpeed on the ground, draining
//    `drain` bar/s while held and moving; it ends by itself at empty (no lockout: the next press boosts and arms it again).
// 4. refill: `rate` bar/s once `delay` s have passed since the last spend (emptyDelay s if a sprint emptied the bar), never
//    while boosting or sprinting; x the Mover's regenMult (the style rank's reward). Off: the web's regen in bar units
//    (dash.regenPerSec / dash.maxPips of the whole per s, no delay, not while boosting).
// 5. rewards: a kill adds `kill`, a slam's landing `slam`, a parry `parry` (the owner calls Mover.StaminaReward).
// The Mover reads these through one new field (Mover.stamina, a State: the run's rules, the bar and its clocks), neutral
// by default: with Params.Neutral (the golden vectors' and replays' config) it is the web's mover bit for bit. Whole steps
// (MovementMath.StepsFor at the config's simHz), JsMath for min / max / hypot; no allocation.
using System;

namespace Skygrind.Core
{
    public static class Stamina
    {
        /// <summary>The port-only events for GG's sounds (SurvivalSim.Stamina.cs puts them on the bus): the bar refilled to
        /// full, a sprint began, a sprint ended, a boost fired under its full cost (payload: its percent of a full boost).</summary>
        public const string FullEvent = "staminaFull", SprintStartEvent = "sprintStart", SprintEndEvent = "sprintEnd", BoostPartialEvent = "boostPartial";

        /// <summary>The number of flags (bar, boost, sprint, refill, rewards: the tape's stamina byte, bit i = flag i).</summary>
        public const int FlagCount = 5;

        /// <summary>design.stamina, read once (every flag off when the block is absent: the web's config).</summary>
        public struct Params
        {
            public bool bar, boost, sprint, refill, rewards;
            /// <summary>NIGHT19 (design.stamina.n19.ownKey, set by the run when the block is on): the sprint has its own key
            /// (InputFrame.sprint) besides the boost key's hold. Not one of the five tape flags: the tape's NIGHT19 byte carries it.</summary>
            public bool ownKey;
            /// <summary>bar: the size at the start, and the bars VIGOR adds.</summary>
            public double max, bonusBars;
            /// <summary>boost: a full boost's cost, the floor (of the full boost), fullAt (of the cost), the dash-jump's extra.</summary>
            public double cost, floor, fullAt, dashJumpExtra;
            /// <summary>sprint: m/s on the ground, bar/s while held and moving.</summary>
            public double sprintSpeed, drain;
            /// <summary>refill: bar/s, the delays (s); webRate: the refill with its rule off (dash.regenPerSec / dash.maxPips).</summary>
            public double rate, delay, emptyDelay, webRate;
            /// <summary>rewards: bar added by a kill, a slam's landing, a parry.</summary>
            public double kill, slam, parry;
            /// <summary>The delays in whole steps at the config's simHz.</summary>
            public int delaySteps, emptyDelaySteps;

            /// <summary>Every rule off: the web's pips.</summary>
            public static Params Neutral => default;

            /// <summary>The bar is on (the Mover's hooks run only then; every other rule is a rule of the bar).</summary>
            public bool Any => bar;

            /// <summary>design.stamina (the flags are the design defaults; a run sets its own with With).</summary>
            public static Params From(GameConfig cfg)
            {
                if (!cfg.Has("design.stamina")) return Neutral;
                MovementConfig mc = cfg.movement;
                double dt = 1 / mc.loop.simHz;
                var p = new Params
                {
                    bar = cfg.Design("stamina.bar.enabled"),
                    boost = cfg.Design("stamina.boost.enabled"),
                    sprint = cfg.Design("stamina.sprint.enabled"),
                    refill = cfg.Design("stamina.refill.enabled"),
                    rewards = cfg.Design("stamina.rewards.enabled"),
                    max = cfg.Num("design.stamina.bar.max"),
                    bonusBars = cfg.Num("design.stamina.bar.bonusBars"),
                    cost = cfg.Num("design.stamina.boost.cost"),
                    floor = cfg.Num("design.stamina.boost.floor"),
                    fullAt = cfg.Num("design.stamina.boost.fullAt"),
                    dashJumpExtra = cfg.Num("design.stamina.boost.dashJumpExtra"),
                    sprintSpeed = cfg.Num("design.stamina.sprint.speed"),
                    drain = cfg.Num("design.stamina.sprint.drain"),
                    rate = cfg.Num("design.stamina.refill.rate"),
                    delay = cfg.Num("design.stamina.refill.delay"),
                    emptyDelay = cfg.Num("design.stamina.refill.emptyDelay"),
                    webRate = mc.dash.regenPerSec / mc.dash.maxPips,
                    kill = cfg.Num("design.stamina.rewards.kill"),
                    slam = cfg.Num("design.stamina.rewards.slam"),
                    parry = cfg.Num("design.stamina.rewards.parry"),
                };
                p.delaySteps = MovementMath.StepsFor(p.delay, dt);
                p.emptyDelaySteps = MovementMath.StepsFor(p.emptyDelay, dt);
                return p;
            }

            /// <summary>These numbers with a run's flags (the design defaults, or a tape's).</summary>
            public Params With(bool bar, bool boost, bool sprint, bool refill, bool rewards)
            {
                Params p = this;
                p.bar = bar;
                p.boost = boost;
                p.sprint = sprint;
                p.refill = refill;
                p.rewards = rewards;
                return p;
            }

            /// <summary>Flag i (0 bar, 1 boost, 2 sprint, 3 refill, 4 rewards).</summary>
            public bool Flag(int i) => i == 0 ? bar : i == 1 ? boost : i == 2 ? sprint : i == 3 ? refill : i == 4 && rewards;
        }

        /// <summary>The Mover's stamina (Mover.stamina): the run's rules, the bar and its clocks, one struct so the run snapshot
        /// (MoverState) saves it whole.</summary>
        public struct State
        {
            public Params rules;
            public double bar; // 0 .. Max
            public int sinceSpend; // steps since the last spend (held 0 while boosting or sprinting)
            public bool emptied; // a sprint emptied the bar (the refill waits emptyDelay)
            public bool armed; // the boost key's press holds on: a hold after the boost sprints
            public bool sprinting; // a sprint is on (from its start on the ground to its end: key up, empty, standing still)
            public bool sprintStep; // this step sprints on the ground (the speed and the drain)
            public bool full; // the bar was full at the last step (staminaFull's edge)
            public double lastBoost; // the last boost's share of a full boost (1: full)
        }

        /// <summary>The bar's size: max, + bonusBars with VIGOR (bonusPips > 0).</summary>
        public static double Max(in Params p, int bonusPips) => p.max + (bonusPips > 0 ? p.bonusBars : 0);

        /// <summary>The least fill a boost fires with: floor x cost with the boost rule, the whole cost without.</summary>
        public static double FloorFill(in Params p) => p.boost ? p.floor * p.cost : p.cost;

        /// <summary>Can a boost fire with this bar?</summary>
        public static bool CanBoost(in Params p, double bar) => bar >= FloorFill(p);

        /// <summary>A boost's share of a full boost at this bar: 1 at fullAt x cost or more (or without the boost rule), else
        /// bar / cost, never below floor.</summary>
        public static double BoostFraction(in Params p, double bar) =>
            !p.boost || p.cost <= 0 || bar >= p.fullAt * p.cost ? 1 : JsMath.Max(p.floor, JsMath.Min(1, bar / p.cost));

        /// <summary>What a boost takes from the bar: up to its cost.</summary>
        public static double BoostSpend(in Params p, double bar) => JsMath.Max(0, JsMath.Min(bar, p.cost));

        /// <summary>A boost's steps: the full duration's, or the duration x fraction's (whole steps).</summary>
        public static int BoostSteps(double duration, double fraction, double dt) =>
            MovementMath.StepsFor(fraction >= 1 ? duration : duration * fraction, dt);

        /// <summary>One step of the refill (see the file's rule 4); busy: boosting or sprinting this step.</summary>
        public static double Refill(in Params p, double bar, double max, double regenMult, int sinceSpend, bool emptied, bool busy, double dt)
        {
            if (busy || bar >= max) return JsMath.Min(bar, max);
            if (!p.refill) return JsMath.Min(max, bar + p.webRate * regenMult * dt);
            if (sinceSpend < (emptied ? p.emptyDelaySteps : p.delaySteps)) return bar;
            return JsMath.Min(max, bar + p.rate * regenMult * dt);
        }

        /// <summary>One sprint step's drain, never below 0.</summary>
        public static double Drain(in Params p, double bar, double dt) => JsMath.Max(0, bar - p.drain * dt);

        /// <summary>A reward (or any refill outside the step): bar + amount, never above max.</summary>
        public static double Add(double bar, double amount, double max) => JsMath.Min(max, bar + amount);

        /// <summary>The sprint's wish speed: sprintSpeed x the stick's length (at most 1), as the walk's.</summary>
        public static double SprintWish(in Params p, double moveX, double moveZ) => p.sprintSpeed * JsMath.Min(1, JsMath.Hypot(moveX, moveZ));

        /// <summary>
        /// NIGHT19 U3-33.7 (builder E; design.stamina.n19, read once; the run applies it only with the block on: SurvivalSim.
        /// Stamina.cs): "start with less, earn more" (GG 2026-10-06; Research/2026-10-06-weapon-sandbox.md 23-24). The web has
        /// no sprint and no bar, so none of these numbers: drain replaces sprint.drain (0.3 bar/s: a full bar sprints 3.3 s);
        /// a kill within nearRange m of the player's eye refills killNear, beyond farRange m killFar, between them rewards.kill;
        /// slamOnHit: a slam's landing refills rewards.slam only with an enemy inside the slam's shockwave (slam.
        /// shockwaveRadius, the web's within: an aim point within radius + the enemy's radius).
        /// </summary>
        public struct N19Params
        {
            public bool slamOnHit;
            public double drain, nearRange, killNear, farRange, killFar, slamRadius;

            /// <summary>design.stamina.n19 (every number 0 when the block is absent: the run never applies it then).</summary>
            public static N19Params From(GameConfig cfg)
            {
                if (!cfg.Has("design.stamina.n19.drain")) return default;
                return new N19Params
                {
                    drain = cfg.Num("design.stamina.n19.drain"),
                    nearRange = cfg.Num("design.stamina.n19.nearRange"),
                    killNear = cfg.Num("design.stamina.n19.killNear"),
                    farRange = cfg.Num("design.stamina.n19.farRange"),
                    killFar = cfg.Num("design.stamina.n19.killFar"),
                    slamOnHit = cfg.Design("stamina.n19.slamOnHit"),
                    slamRadius = cfg.Num("slam.shockwaveRadius"),
                };
            }
        }

        /// <summary>NIGHT19: a kill's refill by its distance from the player's eye: killNear within nearRange, killFar beyond
        /// farRange, the flat reward (rewards.kill) between them.</summary>
        public static double KillReward(in N19Params n, double flatKill, double distance) =>
            distance <= n.nearRange ? n.killNear : distance > n.farRange ? n.killFar : flatKill;
    }
}
