// Port of skygrind/src/style/styleMeter.js (SPEC 2.0 §7, OVERNIGHT2 N1 pacing), NIGHT9 U3-07/U3-08: turns bus events
// into named awards (kills, bonuses, parries, tricks) through the pure scoring core (StyleCore, styleCore.js: the same
// one the headless pace simulation runs), fills a draining meter with six ranks and keeps the run's style total for
// the score. Fixed step only; the HUD reads the rank, the fill and 'styleAward'.
// Port notes: the Mover's movement tricks reach it as one "trick" event with the trick's name (SurvivalGame maps the
// Mover's events), slams and punches as "slamUse" / "punch"; the arsenal's FRESH comes through getFresh, as main.js
// passes it (null: the meter's own freshness). Unity-only (NIGHT8, off = the web's meter): PERFECT PARRY and the LAB.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class StyleAwardEvent
    {
        public string name;
        public double points;
        public bool quiet;
    }

    /// <summary>A named bonus from another system ('styleBonus': IMPACT from a momentum melee hit, TRAPPED, ...).</summary>
    public sealed class StyleBonusEvent
    {
        public string owner = "player";
        public string name; // IMPACT, TRAPPED, ...
        public double points; // the base
        public string weapon;
        public double assistPay = 1;
    }

    /// <summary>styleMeter.js: the scoring core's bus wiring for the player.</summary>
    public sealed class StyleMeter : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the scoring core's (StyleCore holds every number; the meter only wires it).
        public void Save(StateWriter w) => core.Save(w);

        public void Load(StateReader r) => core.Load(r);

        static readonly Dictionary<string, string> TrickNames = new Dictionary<string, string>
        {
            ["wallJump"] = "WALL JUMP", ["dashJump"] = "DASH JUMP", ["boostJump"] = "BOOST", ["slideJump"] = "SLIDE JUMP", ["padLaunch"] = "LAUNCH",
            ["rocketJump"] = "ROCKET JUMP", ["grenadeJump"] = "GRENADE JUMP", ["slingshot"] = "SLINGSHOT",
        };

        readonly StyleMath.S s;
        readonly Func<string> state;
        readonly Func<bool> enemyNear;
        /// <summary>The scoring core (the pace simulation and debug read its stats).</summary>
        public readonly StyleCore core;
        /// <summary>The arsenal's FRESH of a weapon (0..1), null when it doesn't track it (main.js getFresh).</summary>
        public Func<string, double?> getFresh = w => null;

        public StyleMath.S Params => s;

        public StyleMeter(GameConfig cfg, EventBus events, Func<string> state, Func<bool> enemyNear)
        {
            s = StyleMath.From(cfg);
            this.state = state;
            this.enemyNear = enemyNear;
            core = new StyleCore(s,
                (name, points, quiet) => events.Emit("styleAward", new StyleAwardEvent { name = name, points = points, quiet = quiet }),
                (rank, up) => events.Emit("rankChange", rank));
            events.On<KillEvent>("kill", e => { if (e.owner == "player") core.Kill(e.kind, e.weapon, state(), e.distance, e.melee, false, e.assistPay, getFresh(e.weapon)); });
            events.On<HitEvent>("hit", e => { if (e.owner == "player") core.Hit(e.weapon, state(), e.headshot, e.pointScale, e.melee, e.shot, e.assistPay, getFresh(e.weapon)); });
            // NIGHT8 U2-07: a PERFECT PARRY (design.parry, Unity-only) pays its own award (design.parry.perfectPoints) instead.
            events.On<HitEvent>("parry", e => { if (e.owner == "player") core.Parry(state(), getFresh("punch"), e.perfect); });
            // POWER P3: switching pressure pays: a cross-family hit, the named combos, a kill right after a swap.
            events.On<ArsenalEvent>("crossHit", e => { if (e.owner == "player") core.Cross(e.combo, e.weapon, state()); });
            events.On<ArsenalEvent>("swapKill", e => { if (e.owner == "player") core.SwapKill(e.weapon, state()); });
            // OVERNIGHT2: named bonuses from other systems (IMPACT from a momentum melee hit, TRAPPED, ...).
            events.On<StyleBonusEvent>("styleBonus", e => { if (e.owner == "player") core.Bonus(e.name, e.points, e.weapon, state(), e.assistPay); });
            events.On("trick", o =>
            {
                string name = (string)o;
                core.Trick(name, TrickNames.TryGetValue(name, out string l) ? l : name.ToUpperInvariant(), enemyNear(), state());
            });
            events.On("fire", o => core.Use((string)o, (string)o == "nailgun" ? s.nailgunUseScale : 1));
            events.On("slamUse", _ => core.UseKey("slam"));
            events.On("punch", _ => core.UseKey("punch"));
            events.On<HurtEvent>("hurt", e => { if (e.owner == "player") core.Hurt(); });
        }

        /// <summary>Start scoring (a run or match); resets everything.</summary>
        public void Start() => core.Start();

        public void Stop() => core.Stop();

        public bool enabled => core.enabled;
        public double meter => core.meter;
        public double total => core.total;
        public int rank => core.Rank;
        public int bestRank => core.bestRank;
        public string bestName => core.bestName;
        public double bestPoints => core.bestPoints;
        public Dictionary<string, double> breakdown => core.breakdown;
        public double time => core.time;

        /// <summary>The drain multiplier (camping doubles it).</summary>
        public double drainMult
        {
            get => core.drainMult;
            set => core.drainMult = value;
        }

        /// <summary>The FLOW perk (POWER P6): x drain, x meter gain.</summary>
        public void SetPerkMods(double? drain, double? gain) => core.SetPerkMods(drain, gain);

        // NIGHT8 U2-16 LAB (set at a run's start by Lab.Apply): FRESHNESS TEETH and STYLE RAMP; both off is the web's meter.
        public LabMath.P lab
        {
            get => core.lab;
            set => core.lab = value;
        }

        public bool labFreshness
        {
            get => core.labFreshness;
            set => core.labFreshness = value;
        }

        public bool labRamp
        {
            get => core.labRamp;
            set => core.labRamp = value;
        }

        public double Fill => core.Fill();

        public string RankName => s.ranks[core.Rank];

        public string FreshTier(string weapon)
        {
            string key = StyleMath.FreshKey(weapon);
            return key != null ? TierName(core.fresh[key]) : "";
        }

        /// <summary>A freshness value's tier name (FRESH / USED / STALE, and DULL with the LAB's FRESHNESS TEETH).</summary>
        public string TierName(double value) => core.labFreshness ? LabMath.FreshnessTierName(value, s, core.lab) : StyleMath.FreshnessTierName(value, s);

        /// <summary>A weapon's freshness (0..style.freshness.max), or -1 for one without a freshness key (the gauntlet).</summary>
        public double Freshness(string weapon)
        {
            string k = StyleMath.FreshKey(weapon);
            return k != null && core.fresh.TryGetValue(k, out double v) ? v : -1;
        }

        /// <summary>One fixed step: freshness regen, the hit-income refill, drain, the shown rank.</summary>
        public void Step(double dt)
        {
            if (!core.enabled) return;
            if (core.labRamp) core.RampState(state()); // the ground (or any other state) resets the ramp
            core.Step(dt);
        }
    }
}
