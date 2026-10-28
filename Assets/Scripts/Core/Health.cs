// Port of skygrind/src/combat/health.js (owner-agnostic health, SPEC §4) and combat/regenMath.js + regen.js (health
// regen, SURVIVAL.md §2 S1). Damage respects dash i-frames and a timed shield, heal caps at max, death once. Regen:
// after damage a delay runs (regen.delay); it counts down regen.fastMult x as fast while moving above
// regen.fastSpeed or after dealing damage (regen.dealtWindow); then HP climbs at regen.rate. Camping (EXPOSED)
// pauses it. The style rank speeds it (regen.rankPerRank).
using System;

namespace Skygrind.Core
{
    public sealed class HurtEvent
    {
        public string owner;
        public double amount;
        public double hp;
        public string source; // 'husk', 'fall', ... (payload only)
        public Vec3 from; // where the blow came from (the HUD's damage arrow)
        public bool hasFrom;
    }

    public sealed class DeathEvent
    {
        public string owner;
        public string source;
    }

    public sealed class Health : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: HP, the max (PLATE), the taken multiplier (GLASS CANNON), ULTRA HEALTH's hard
        // damage and the timed shield.
        public void Save(StateWriter w)
        {
            w.D("hp", hp);
            w.D("max", max);
            w.D("takenMult", takenMult);
            w.D("hard", hard);
            w.I("shieldSteps", shieldSteps);
        }

        public void Load(StateReader r)
        {
            hp = r.D("hp");
            max = r.D("max");
            takenMult = r.D("takenMult");
            hard = r.D("hard");
            shieldSteps = r.Int("shieldSteps");
        }

        readonly EventBus events;
        readonly string owner;
        readonly Func<bool> isInvulnerable;
        int shieldSteps;
        public double hp;
        public double max;
        public double takenMult = 1;
        /// <summary>
        /// NIGHT11 U3-24.9 (the LAB's ULTRA HEALTH, Core/UltraHealth.cs; new design, not the web's): hard damage. Heal and
        /// Regen fill only to max - hard. 0 (always, unless the LAB toggle is on) is the web's health.js.
        /// </summary>
        public double hard;

        /// <summary>NIGHT47 F (design.n47.overheal47, a sim row): health above the max, filled by close kills at full health and drained back to 0
        /// over time (SurvivalSim.N47.Feel.cs); a hit takes it first. Always 0 with the row off (the older game exactly). Not in the run snapshot
        /// (a restored run starts it at 0).</summary>
        public double over;

        /// <summary>What a heal can fill to: max - hard (max itself while hard is 0, the web's cap).</summary>
        public double Cap => max - hard;

        public Health(double max, EventBus events, string owner, Func<bool> isInvulnerable = null)
        {
            this.max = hp = max;
            this.events = events;
            this.owner = owner;
            this.isInvulnerable = isInvulnerable ?? (() => false);
        }

        public bool Dead => hp <= 0;

        public bool Shielded => shieldSteps > 0;

        /// <summary>True if the damage landed.</summary>
        public bool Damage(double amount, string source = null, Vec3? from = null)
        {
            if (Dead || isInvulnerable() || shieldSteps > 0) return false;
            amount *= takenMult;
            if (over > 0) { double a = Math.Min(over, amount); over -= a; amount -= a; } // NIGHT47 F overheal47: the overheal takes a hit first (0 with the row off: NIGHT46 exactly)
            hp = Math.Max(0, hp - amount);
            events.Emit("hurt", new HurtEvent { owner = owner, amount = amount, hp = hp, source = source, from = from ?? default, hasFrom = from.HasValue });
            if (Dead) events.Emit("death", new DeathEvent { owner = owner, source = source });
            return true;
        }

        public double Heal(double amount)
        {
            if (Dead || amount <= 0) return 0;
            double restored = Math.Min(amount, Cap - hp); // NIGHT11: max - hard (hard 0: the web's max - hp)
            if (restored <= 0) return 0;
            hp += restored;
            events.Emit("heal", new HurtEvent { owner = owner, amount = restored, hp = hp });
            return restored;
        }

        /// <summary>Regen: restore HP silently (no heal event per step).</summary>
        public double Regen(double amount)
        {
            if (Dead || amount <= 0) return 0;
            double restored = Math.Min(amount, Cap - hp); // NIGHT11: max - hard (hard 0: the web's max - hp)
            if (restored > 0) hp += restored;
            return Math.Max(0, restored);
        }

        public void Reset()
        {
            hp = max;
            hard = 0;
            over = 0; // NIGHT47 F overheal47 (0 already with the row off)
            shieldSteps = 0;
        }

        public void Shield(int steps) => shieldSteps = Math.Max(shieldSteps, steps);

        /// <summary>One fixed step (the timed invulnerability).</summary>
        public void Tick()
        {
            if (shieldSteps > 0) shieldSteps--;
        }
    }

    public sealed class RegenState
    {
        public double delay;
        public double sinceDealt = double.PositiveInfinity;
        public bool regenerating;
    }

    /// <summary>regenMath.js (pure).</summary>
    public static class RegenMath
    {
        public struct Params
        {
            public double delay, rate, fastSpeed, fastMult, dealtWindow;
        }

        public struct Perk
        {
            public double delay, rate;
            public static readonly Perk None = new Perk { delay = 1, rate = 1 };
        }

        public static Params From(GameConfig cfg) => new Params
        {
            delay = cfg.Num("regen.delay"),
            rate = cfg.Num("regen.rate"),
            fastSpeed = cfg.Num("regen.fastSpeed"),
            fastMult = cfg.Num("regen.fastMult"),
            dealtWindow = cfg.Num("regen.dealtWindow"),
        };

        /// <summary>The perk multipliers with the style rank folded in: x (1 + perRank x rank) on the rate, / on the delay.</summary>
        public static Perk RankRegenPerk(Perk perk, int rank, double perRank)
        {
            double m = 1 + perRank * Math.Max(0, rank);
            return new Perk { delay = perk.delay / m, rate = perk.rate * m };
        }

        public static void OnDamage(RegenState s, Params r, Perk perk)
        {
            s.delay = r.delay * perk.delay;
            s.regenerating = false;
        }

        public static void OnDealt(RegenState s) => s.sinceDealt = 0;

        public static double CountdownRate(double speed, double sinceDealt, Params r) => speed > r.fastSpeed || sinceDealt <= r.dealtWindow ? r.fastMult : 1;

        /// <summary>One fixed step: HP to restore this step (0 while waiting, camping or full).</summary>
        public static double Step(RegenState s, double dt, double speed, bool camping, double hp, double max, Params r, Perk perk)
        {
            s.sinceDealt += dt;
            if (camping)
            {
                s.regenerating = false;
                return 0;
            }
            if (s.delay > 0)
            {
                s.delay = Math.Max(0, s.delay - dt * CountdownRate(speed, s.sinceDealt, r));
                s.regenerating = false;
                return 0;
            }
            if (hp <= 0 || hp >= max)
            {
                s.regenerating = false;
                return 0;
            }
            s.regenerating = true;
            return Math.Min(max - hp, r.rate * perk.rate * dt);
        }
    }

    /// <summary>
    /// NIGHT20 U3-34.5 (builder E; design.regen.n20, GG: "the regen happens way too quickly and should only really be at the
    /// end of the round"; the REGEN switch SEGMENTS; Research/2026-10-06-balance-zoom-regen.md 7.3). The pure rules:
    /// - the round's end: a refill to full over refillTime s (KF2's trader-time heal, Brotato's wave reset);
    /// - in a round the bar is cut into `segment` HP segments and passive regen climbs only to the top of the one you're in
    ///   (Resistance's segments, NGS2's temporary / permanent split): `delay` s after the last hit, `rate` HP/s, no speed-up;
    /// - earned heals ignore the segments: a punch kill +punchKillHeal (ULTRAKILL's punch), any other kill within
    ///   closeKillRange m of the player's eye +closeKillHeal (the stamina rule's nearRange; ULTRAKILL's close-range heal).
    /// The web has no segments, no round-end refill and no kill heals (regen.js: a delay, then the rate to full), so every
    /// number is Unity-only (design.regen.n20).
    /// </summary>
    public static class RegenN20
    {
        /// <summary>RoundHealEvent.kind: the round-end refill, a close kill's heal, a punch kill's heal.</summary>
        public const string Round = "round", CloseKill = "closeKill", PunchKill = "punchKill";

        public struct Params
        {
            public double refillTime, segment, delay, rate, closeKillHeal, closeKillRange, punchKillHeal;
            /// <summary>NIGHT20 fix list E1-E3: passive regen in a round (the SEGMENTS rule); false: STRICT, the refill and the
            /// earned heals only (MEND buys the segments back).</summary>
            public bool passive;

            public static Params From(GameConfig cfg)
            {
                double N(string k) => cfg.Num("design.regen.n20." + k);
                return new Params
                {
                    refillTime = N("refillTime"),
                    segment = N("segment"),
                    delay = N("segmentDelay"),
                    rate = N("segmentRate"),
                    closeKillHeal = N("closeKillHeal"),
                    closeKillRange = N("closeKillRange"),
                    punchKillHeal = N("punchKillHeal"),
                    passive = !cfg.Has("design.regen.n20.passive") || cfg.Bool("design.regen.n20.passive"),
                };
            }
        }

        /// <summary>The top of the segment `hp` is in, capped at `cap` (on a line: that line; dead: 0).</summary>
        public static double SegmentTop(double hp, double segment, double cap)
        {
            if (hp <= 0) return 0;
            if (segment <= 0) return cap;
            return Math.Min(cap, Math.Ceiling(hp / segment) * segment);
        }

        /// <summary>A player's kill's earned heal: a punch kill's punchKillHeal (not added to the close kill's), a kill within
        /// closeKillRange m (inclusive) of the eye closeKillHeal, else 0 (kind null).</summary>
        public static double KillHeal(in Params p, string weapon, double distance, out string kind)
        {
            if (weapon == "punch" && p.punchKillHeal > 0)
            {
                kind = PunchKill;
                return p.punchKillHeal;
            }
            if (distance <= p.closeKillRange && p.closeKillHeal > 0)
            {
                kind = CloseKill;
                return p.closeKillHeal;
            }
            kind = null;
            return 0;
        }
    }

    /// <summary>regen.js: the pure rules on the fixed step for one owner; regenStart / regenEnd on the edges.</summary>
    public sealed class Regen : IRunState
    {
        // NIGHT20 U3-34.5 (builder E; design.regen.n20, RegenN20): with UseN20(true) (the run's regen block) the web's rules
        // give way to the segments, the round-end refill (StartRefill, at roundClear) and the earned heals (Earn); the
        // timers are whole steps (stepsFor). Off (the default) none of it runs: NIGHT19's regen, its snapshot unchanged.
        // The HUD's contract (H): N20, SegmentHp, SegmentTop, RefillLeft.
        RegenN20.Params n20p;
        int n20DelaySteps; // steps left of the segment delay after the last hit
        int refillSteps; // steps left of the round-end refill
        readonly double stepDt; // the fixed step (the n20 timers' unit)
        readonly RoundHealEvent roundHealPayload = new RoundHealEvent();
        readonly RoundHealEvent earnedHealPayload = new RoundHealEvent();

        /// <summary>NIGHT20: the block's rules live (the run's design.regen.n20).</summary>
        public bool N20 { get; private set; }

        /// <summary>NIGHT20: the segment's HP (design.regen.n20.segment; 0 with the block off: no segments).</summary>
        public double SegmentHp => N20 && Passive ? n20p.segment : 0;

        /// <summary>NIGHT20 fix list E1-E3: passive regen runs in a round: the SEGMENTS rule, or STRICT with MEND (perks.js's
        /// regen multipliers set: MEND buys the segments back at mendDelay / mendRate).</summary>
        bool Passive => n20p.passive || perk.delay != 1 || perk.rate != 1;

        /// <summary>NIGHT20: the HP passive regen climbs to now: the top of the segment you're in (the cap with the block off,
        /// your HP with NO REGEN: nowhere).</summary>
        public double SegmentTop => !N20 ? health.Cap : !enabled || !Passive ? health.hp : RegenN20.SegmentTop(health.hp, n20p.segment, health.Cap);

        /// <summary>NIGHT20: the seconds left of the round-end refill (0 when none runs).</summary>
        public double RefillLeft => refillSteps * stepDt;

        /// <summary>NIGHT20: the run's regen block (SurvivalSim.Regen20.cs at the run's start): on, the block's rules; off, the web's.</summary>
        public void UseN20(bool on, in RegenN20.Params p)
        {
            N20 = on;
            n20p = p;
            n20DelaySteps = 0;
            refillSteps = 0;
        }

        /// <summary>NIGHT20: the round-end refill (at roundClear): to full over refillTime, `roundHeal` with the missing HP.
        /// Nothing with the block off, with NO REGEN, dead or full. Returns the HP it will restore.</summary>
        public double StartRefill(int round)
        {
            if (!N20 || !enabled || health.Dead) return 0;
            double missing = health.Cap - health.hp;
            if (missing <= 0) return 0;
            refillSteps = MovementMath.StepsFor(n20p.refillTime, stepDt);
            roundHealPayload.kind = RegenN20.Round;
            roundHealPayload.amount = missing;
            roundHealPayload.seconds = n20p.refillTime;
            roundHealPayload.round = round;
            events.Emit(N20Events.RoundHeal, roundHealPayload);
            return missing;
        }

        /// <summary>NIGHT20: an earned heal (a close kill, a punch kill): at once, past the segments' lines, even with NO REGEN
        /// (the web's `heal`, as the parry's, then `earnedHeal` with what it restored). Nothing with the block off.</summary>
        public double Earn(double amount, string kind)
        {
            if (!N20) return 0;
            double got = health.Heal(amount);
            if (got > 0)
            {
                earnedHealPayload.kind = kind;
                earnedHealPayload.amount = got;
                earnedHealPayload.seconds = 0;
                events.Emit(N20Events.EarnedHeal, earnedHealPayload);
            }
            return got;
        }

        /// <summary>NIGHT20: one step of the block's rules: the refill first (linear to full over its steps left), else the
        /// segment regen (camping pauses it, as the web's; no speed-up; MEND and the style rank scale it as the web's).</summary>
        void StepN20(double dt)
        {
            if (n20DelaySteps > 0) n20DelaySteps--;
            double amount = 0;
            bool on;
            if (refillSteps > 0)
            {
                amount = (health.Cap - health.hp) / refillSteps;
                refillSteps--;
                on = true;
            }
            else if (!Passive || n20DelaySteps > 0 || camping() || health.hp <= 0) // STRICT: nothing in a round
            {
                on = false;
            }
            else
            {
                double top = RegenN20.SegmentTop(health.hp, n20p.segment, health.Cap);
                double rate = (rateOverride > 0 ? rateOverride : n20p.rate) * Effective().rate;
                amount = Math.Min(top - health.hp, rate * dt);
                on = amount > 0;
            }
            state.regenerating = on;
            if (amount > 0) health.Regen(amount);
            if (state.regenerating != shown)
            {
                shown = state.regenerating;
                events.Emit(shown ? "regenStart" : "regenEnd", new HurtEvent { owner = owner, hp = health.hp });
            }
        }

        // NIGHT13 U3-26.10: the run snapshot: the delay, the since-dealt clock (+Infinity before any hit: saved as its bits),
        // the edge flags, the switch (NO REGEN), MEND's multipliers and the LAB's rate.
        public void Save(StateWriter w)
        {
            w.D("delay", state.delay);
            w.D("sinceDealt", state.sinceDealt);
            w.B("regenerating", state.regenerating);
            w.B("shown", shown);
            w.B("enabled", enabled);
            w.D("perkDelay", perk.delay);
            w.D("perkRate", perk.rate);
            w.D("rateOverride", rateOverride);
            if (N20)
            {
                // NIGHT20: the block's two clocks, only while it's on (a block-off snapshot is NIGHT19's)
                w.D("n20DelaySteps", n20DelaySteps);
                w.D("refillSteps", refillSteps);
            }
        }

        public void Load(StateReader r)
        {
            state.delay = r.D("delay");
            state.sinceDealt = r.D("sinceDealt");
            state.regenerating = r.B("regenerating");
            shown = r.B("shown");
            enabled = r.B("enabled");
            perk = new RegenMath.Perk { delay = r.D("perkDelay"), rate = r.D("perkRate") };
            rateOverride = r.D("rateOverride");
            n20DelaySteps = r.Has("n20DelaySteps") ? (int)r.D("n20DelaySteps") : 0;
            refillSteps = r.Has("refillSteps") ? (int)r.D("refillSteps") : 0;
        }

        readonly EventBus events;
        readonly Health health;
        readonly Func<double> speed;
        public Func<int> rank; // the style rank (set by the owner once the meter exists)
        readonly string owner;
        readonly RegenMath.Params p;
        readonly double rankPerRank;
        readonly RegenState state = new RegenState();
        bool shown;
        public bool enabled;
        public RegenMath.Perk perk = RegenMath.Perk.None;
        public Func<bool> camping = () => false;
        public double rateOverride; // NIGHT8 U2-16 LAB CLOSE-KILL HEAL: HP/s in place of regen.rate (0: the web's)

        public Regen(GameConfig cfg, EventBus events, Health health, Func<double> speed, Func<int> rank = null, string owner = "player")
        {
            this.events = events;
            this.health = health;
            this.speed = speed;
            this.rank = rank ?? (() => 0);
            this.owner = owner;
            p = RegenMath.From(cfg);
            rankPerRank = cfg.Num("regen.rankPerRank");
            stepDt = cfg.Dt;
            events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner != owner) return;
                RegenMath.OnDamage(state, p, Effective());
                if (N20) n20DelaySteps = MovementMath.StepsFor(n20p.delay * Effective().delay, stepDt); // NIGHT20: the segment's delay
            });
            events.On<HitEvent>("hit", e =>
            {
                if (e.owner == owner) RegenMath.OnDealt(state);
            });
        }

        RegenMath.Perk Effective() => RegenMath.RankRegenPerk(perk, rank(), rankPerRank);

        public bool Regenerating => enabled && state.regenerating;

        public double DelayLeft => state.delay;

        public void SetEnabled(bool on)
        {
            enabled = on;
            Reset();
        }

        public void Reset()
        {
            state.delay = 0;
            state.sinceDealt = double.PositiveInfinity;
            state.regenerating = false;
            shown = false;
            n20DelaySteps = 0; // NIGHT20: no segment delay, no refill
            refillSteps = 0;
        }

        public void Step(double dt)
        {
            if (!enabled) return;
            if (N20)
            {
                StepN20(dt); // NIGHT20: design.regen.n20's rules in place of the web's
                return;
            }
            RegenMath.Params r = p;
            if (rateOverride > 0) r.rate = rateOverride;
            double amount = RegenMath.Step(state, dt, speed(), camping(), health.hp, health.Cap, r, Effective()); // NIGHT11: Cap = max - hard (max while hard is 0)
            if (amount > 0) health.Regen(amount);
            if (state.regenerating != shown)
            {
                shown = state.regenerating;
                events.Emit(shown ? "regenStart" : "regenEnd", new HurtEvent { owner = owner, hp = health.hp });
            }
        }
    }
}
