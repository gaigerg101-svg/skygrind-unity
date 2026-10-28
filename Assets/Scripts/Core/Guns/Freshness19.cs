// NIGHT19 U3-33.6 (builder W, design.freshness; FRESHNESS switch POINTS): ULTRAKILL's freshness table (its Style page;
// Research/2026-10-06-weapon-sandbox.md 22) on points and style only, never damage. A 0..max scale per gun: FRESH from
// freshAt (x freshMult), USED from usedAt (x usedMult), STALE from staleAt (x staleMult), DULL below (x dullMult: GG's
// switch, not ULTRAKILL's x0). A shot costs shotCost x the gun's interval in s (so every gun goes FRESH -> STALE after
// about the same seconds of fire, whatever its rate); every other gun gains otherGain x min(1, the shot's interval /
// otherGainRef) (ULTRAKILL weights a fast gun's regen down: nails 0.1x); a holstered gun gains holsterRegen per s; with
// one gun owned the floor is FRESH (ULTRAKILL's floor rule: a new player's lone KESTREL never goes stale). The arsenal
// (Arsenal.cs) keeps the values and pays its hits' points by the tier; the damage multiplier never reads it.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Freshness19Params
    {
        public double max, freshAt, usedAt, staleAt, freshMult, usedMult, staleMult, dullMult, shotCost, otherGain, otherGainRef, holsterRegen;
        /// <summary>NIGHT20 (builder G, S7): the value a lone gun never drops under (design.freshness.oneGunFloor; NIGHT19's
        /// freshAt without it).</summary>
        public double oneGunFloor;
        /// <summary>Each catalog gun's interval between shots in s (the cost's base), from the web's weapons.* numbers.</summary>
        public readonly Dictionary<string, double> interval = new Dictionary<string, double>();

        public static Freshness19Params From(GameConfig cfg, WeaponCatalog catalog)
        {
            if (!cfg.Has("design.freshness.shotCost")) return null;
            string s = "design.freshness.";
            var p = new Freshness19Params
            {
                max = cfg.Num(s + "max"), freshAt = cfg.Num(s + "freshAt"), usedAt = cfg.Num(s + "usedAt"), staleAt = cfg.Num(s + "staleAt"),
                freshMult = cfg.Num(s + "freshMult"), usedMult = cfg.Num(s + "usedMult"), staleMult = cfg.Num(s + "staleMult"), dullMult = cfg.Num(s + "dullMult"),
                shotCost = cfg.Num(s + "shotCost"), otherGain = cfg.Num(s + "otherGain"), otherGainRef = cfg.Num(s + "otherGainRef"), holsterRegen = cfg.Num(s + "holsterRegen"),
            };
            p.oneGunFloor = cfg.Has(s + "oneGunFloor") ? cfg.Num(s + "oneGunFloor") : p.freshAt;
            foreach (string id in catalog.order) p.interval[id] = IntervalOf(cfg, id);
            return p;
        }

        /// <summary>A gun's interval between shots (s): its fireInterval; the STITCHER 1 / rate; the LONGWAVE its charge +
        /// cooldown; the GAUNTLET its cycle; anything else 1.</summary>
        static double IntervalOf(GameConfig cfg, string id)
        {
            if (id == "nailgun") return 1 / cfg.Num("weapons.nailgun.rate");
            if (id == "rail") return cfg.Num("weapons.rail.charge") + cfg.Num("weapons.rail.cooldown");
            if (id == "gauntlet") return cfg.Num("weapons.gauntlet.cycle");
            return cfg.Has($"weapons.{id}.fireInterval") ? cfg.Num($"weapons.{id}.fireInterval") : 1;
        }

        public double Interval(string id) => id != null && interval.TryGetValue(id, out double v) ? v : 1;
    }

    public static class Freshness19
    {
        /// <summary>The tier's points multiplier at `value`.</summary>
        public static double Mult(double value, Freshness19Params p) =>
            value >= p.freshAt ? p.freshMult : value >= p.usedAt ? p.usedMult : value >= p.staleAt ? p.staleMult : p.dullMult;

        /// <summary>The tier's name at `value` (FRESH, USED, STALE, DULL: the HUD's tag).</summary>
        public static string Tier(double value, Freshness19Params p) =>
            value >= p.freshAt ? "FRESH" : value >= p.usedAt ? "USED" : value >= p.staleAt ? "STALE" : "DULL";

        /// <summary>A shot of a gun with `interval` s x `scale`: its value after the cost, never under `floor`.</summary>
        public static double Spend(double value, double interval, double scale, double floor, Freshness19Params p) =>
            JsMath.Max(floor, JsMath.Max(0, value - p.shotCost * interval * scale));

        /// <summary>What every other gun gains from one shot of a gun with `interval` s.</summary>
        public static double OtherGain(double interval, Freshness19Params p) => p.otherGain * JsMath.Min(1, interval / p.otherGainRef);
    }
}
