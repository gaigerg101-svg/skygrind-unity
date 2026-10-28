// Port of skygrind/src/survival/pointsMath.js (pure) and survival/economy.js (the wallet, SURVIVAL.md §2 S1): the
// player's hits, kills and parries turn into points (x the style rank, x the camping penalty); the round-clear bonus
// is flat; buys spend. Emits `points` {delta, total, reason} and `pointsSpent` {price, what, total}. NIGHT10 U3-13.8 adds
// Refund (Unity-only: the Mystery Box's teddy gives the price back).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The web's 'hit' payload (weapons.js damage()): who hit what, how, and whether it died.</summary>
    public sealed class HitEvent
    {
        public string owner = "player";
        public string weapon; // revolver, gauntlet, punch, ...
        public string kind; // the enemy kind
        public int enemy; // the enemy id
        public double damage;
        public bool killed;
        public bool headshot;
        public bool melee;
        public Vec3 position;
        public double pointScale = 1;
        public double freshMult = 1; // the arsenal's FRESH multiplier (NIGHT9 U3-08: points pay x it, as on the web)
        public bool primed, cross; // the arsenal's tags (POWER P3)
        public string combo; // null, COLLAPSE, OVERLOAD
        public double momentum; // M toward the target (OVERNIGHT2 N2; NIGHT9 U3-09)
        public bool shock; // an impact wave's share (no momentum, no new wave, no momentum hitstop)
        public int shot; // the trigger pull / swing this hit belongs to (style pays hit awards once per shot)
        public double distance; // from where the attack came to the hit
        public bool assisted; // the shot was bent by aim assist
        public double assistPay = 1; // x points and style (AimAssist.Payout)
        public bool perfect; // a parry: PERFECT PARRY (design.parry, Unity-only)
    }

    public sealed class KillEvent
    {
        public string owner = "player";
        public string weapon;
        public string kind;
        public int enemy;
        public bool headshot;
        public bool melee;
        public Vec3 position;
        public double distance;
        public bool assisted;
        public double assistPay = 1;
    }

    public sealed class PointsEvent
    {
        public double delta;
        public double total;
        public string reason;
        public int rank;
        public Vec3 position;
        public bool hasPosition;
    }

    public sealed class SpentEvent
    {
        public double price;
        public string what;
        public double total;
    }

    public static class PointsMath
    {
        public struct Params
        {
            public double start, hit, kill, headshotKill, punchKill, parry, roundClear, campMult;
            public double[] rankMult;
        }

        public static Params From(GameConfig cfg) => new Params
        {
            start = cfg.Num("points.start"),
            hit = cfg.Num("points.hit"),
            kill = cfg.Num("points.kill"),
            headshotKill = cfg.Num("points.headshotKill"),
            punchKill = cfg.Num("points.punchKill"),
            parry = cfg.Num("points.parry"),
            roundClear = cfg.Num("points.roundClear"),
            campMult = cfg.Num("points.campMult"),
            rankMult = cfg.Nums("points.rankMult"),
        };

        /// <summary>Base points for one combat event ('hit' or 'parry').</summary>
        public static double BasePoints(bool parry, bool killed, bool headshot, string weapon, bool melee, Params p)
        {
            if (parry) return p.parry;
            if (!killed) return p.hit;
            if (weapon == "punch" || melee) return p.punchKill; // POWER P4: every melee kill pays the punch-kill points
            if (headshot) return p.headshotKill;
            return p.kill;
        }

        public static double RankMultiplier(int rank, Params p) => p.rankMult[Math.Max(0, Math.Min(p.rankMult.Length - 1, rank))];

        /// <summary>Whole points after the rank and camping multipliers (Math.round: half up).</summary>
        public static double ApplyMultipliers(double basePoints, double rankMult, double campMult = 1) => JsMath.Round(basePoints * rankMult * campMult);

        public static double RoundClearPoints(int round, Params p) => p.roundClear * round;

        public static bool CanAfford(double total, double price) => price <= total;
    }

    public sealed class Economy : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the bank, the earnings by reason, the perks' numbers (BOUNTY, DEADEYE), a
        // pending REFUND's discount, the boosts (DOUBLE POINTS, HOLD, GOLD RUSH, the HEAT rung) and their product. The
        // boosts' iteration order is the product's order and SetBoost removes entries, so their slot layout is saved too
        // (DictLayout). rank / campMult are wiring.
        public void Save(StateWriter w)
        {
            w.B("enabled", enabled);
            w.D("total", total);
            w.D("earned", earned);
            w.D("spent", spent);
            w.D("boost", boost);
            w.D("perkPoints", perkPoints);
            w.D("perkHeadshot", perkHeadshot);
            w.D("discount", discount);
            w.SD("byReason", byReason);
            w.SD("boosts", boosts);
            w.Layout("boostSlots", boosts);
        }

        public void Load(StateReader r)
        {
            StateIO.ReadPairs(r, "boosts", out List<string> bk, out List<double> bv);
            r.Layout("boostSlots", out int slots, out List<int> chain);
            enabled = r.B("enabled");
            total = r.D("total");
            earned = r.D("earned");
            spent = r.D("spent");
            boost = r.D("boost");
            perkPoints = r.D("perkPoints");
            perkHeadshot = r.D("perkHeadshot");
            discount = r.D("discount");
            r.SD("byReason", byReason);
            DictLayout.Rebuild(boosts, bk, bv, slots, chain, i => "\u0001slot" + i);
        }

        readonly EventBus events;
        readonly PointsMath.Params p;
        public Func<int> rank; // the style rank (set by the owner once the meter exists)
        public Func<double> campMult; // x combat points while camping (the director's EXPOSED: points.campMult)
        readonly Dictionary<string, double> boosts = new Dictionary<string, double>();
        public bool enabled;
        public double total, earned, spent;
        public readonly Dictionary<string, double> byReason = new Dictionary<string, double>();
        public double boost = 1;
        // NIGHT9 U3-11 (economy.js perk): BOUNTY x every earning; DEADEYE + points per headshot hit (POWER P6).
        double perkPoints = 1, perkHeadshot;

        public Economy(GameConfig cfg, EventBus events, Func<int> rank = null, Func<double> campMult = null)
        {
            this.events = events;
            p = PointsMath.From(cfg);
            this.rank = rank ?? (() => 0);
            this.campMult = campMult ?? (() => 1);
            events.On<HitEvent>("hit", e =>
            {
                if (!enabled || e.owner != "player") return;
                double scale = e.killed ? 1 : e.pointScale;
                Combat(PointsMath.BasePoints(false, e.killed, e.headshot, e.weapon, e.melee, p) * scale, e.killed ? "kill" : "hit", e.position, e.freshMult, e.assistPay);
                if (e.headshot && perkHeadshot > 0) Combat(perkHeadshot, "deadeye", e.position, 1, e.assistPay); // DEADEYE
            });
            events.On<HitEvent>("parry", e =>
            {
                if (enabled && e.owner == "player") Combat(p.parry, "parry", null, 1);
            });
        }

        double Earn(double amount, string reason, Vec3? position = null)
        {
            if (!enabled || amount <= 0) return 0;
            amount = JsMath.Round(amount * perkPoints);
            total += amount;
            earned += amount;
            byReason[reason] = (byReason.TryGetValue(reason, out double r) ? r : 0) + amount;
            events.Emit("points", new PointsEvent { delta = amount, total = total, reason = reason, rank = rank(), position = position ?? default, hasPosition = position.HasValue });
            return amount;
        }

        double Combat(double basePoints, string reason, Vec3? position, double freshMult, double assist = 1) =>
            Earn(PointsMath.ApplyMultipliers(basePoints * assist * freshMult * boost, PointsMath.RankMultiplier(rank(), p), campMult()), reason, position);

        public void Start(double? bank = null)
        {
            enabled = true;
            total = bank ?? p.start;
            earned = spent = 0;
            byReason.Clear();
            events.Emit("points", new PointsEvent { delta = 0, total = total, reason = "start" });
        }

        public void Stop() => enabled = false;

        /// <summary>Perk bonuses (POWER P6): x every earning (BOUNTY), + points a headshot (DEADEYE); null: off.</summary>
        public void SetPerks(double? points, double? headshot)
        {
            perkPoints = points ?? 1;
            perkHeadshot = headshot ?? 0;
        }

        public bool CanAfford(double price) => enabled && PointsMath.CanAfford(total, price);

        public bool Spend(double price, string what)
        {
            if (!enabled || !PointsMath.CanAfford(total, price)) return false;
            total -= price;
            spent += price;
            events.Emit("pointsSpent", new SpentEvent { price = price, what = what, total = total });
            return true;
        }

        /// <summary>
        /// NIGHT10 U3-13.8 (new design; economy.js has no refund): give back what a buy spent (the Mystery Box's teddy).
        /// Not an earning: no BOUNTY, not in `earned` or `byReason`; `spent` goes down by it. Emits `points` {delta, total,
        /// reason} with no position.
        /// </summary>
        public double Refund(double amount, string reason)
        {
            if (!enabled || amount <= 0) return 0;
            total += amount;
            spent -= amount;
            events.Emit("points", new PointsEvent { delta = amount, total = total, reason = reason, rank = rank() });
            return amount;
        }

        public double RoundClear(int round) => Earn(PointsMath.RoundClearPoints(round, p), "round");

        public double Grant(double amount, string reason) => Earn(amount, reason);

        double discount = 1;

        /// <summary>REFUND: x the next purchase's price (1 clears it).</summary>
        public void SetDiscount(double mult) => discount = mult;

        /// <summary>A buyable's price after a pending REFUND (economy.js price).</summary>
        public double Price(double basePrice) => discount == 1 ? basePrice : JsMath.Round(basePrice * discount);

        public void SetBoost(string name, double mult)
        {
            if (mult == 1) boosts.Remove(name);
            else boosts[name] = mult;
            boost = 1;
            foreach (double b in boosts.Values) boost *= b;
        }
    }
}
