// Port of skygrind/src/combat/arsenalMath.js (p17-night6), line for line, the web's comments kept (NIGHT9 U3-07).
// Arsenal math (POWER.md P3). Pure: no three.js, no DOM, unit tested. `a` is config.weapons.arsenal and
// `catalog` config.weapons.catalog.
// - Freshness: every weapon has FRESH 0..1 (starts at 1). A shot or swing costs its weapon's freshCost; a weapon
//   not in hand recovers `recover` per second. Damage and points x lerp(0.65, 1.25, FRESH).
// - Primed swap: the first hit within 1 s of swapping a weapon in deals +35% (the FLIP perk raises it).
// - Cross combos: a hit whose family differs from the last hit's on that enemy within 3 s deals 1.5x. Statuses
//   (BURN, SHOCK, PULL, STUCK, STAGGER) set named combos: PULL then a blast hit is COLLAPSE (2x, in a radius);
//   SHOCK then a melee hit is OVERLOAD (1.75x + a lightning burst).
// Port notes:
// - `a` is an ArsenalParams and `catalog` a WeaponCatalog, both read once from GameConfig. A weapon / family / combo is
//   a string as on the web; the web's null / undefined is C# null.
// - The records the stateful arsenal (combat/arsenal.js) keeps are typed here for it: ArsenalRecord {id, family, at,
//   statuses} with StatusTimes, the statuses object {name: until} in JS key order (insertion order: arsenal.js's hit()
//   walks it with `for (const name in r.statuses)` and stops at the first match), and PrimedSwap {weapon, at, used}.
// - crossResult returns a CrossOutcome struct (the web allocates a fresh {mult, cross, combo} per call).
// Golden vectors: PortExport/vectors/arsenalMath.json (Golden/GoldenPorted.cs). Web tests: tests/arsenal.test.js.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Skygrind.Core
{
    /// <summary>One entry of config.weapons.catalog (the web reads catalog[id]?.family / .kind / .freshCost).</summary>
    public sealed class WeaponInfo
    {
        public string id;
        public string name;
        public string family; // null when the entry has none
        public string kind; // "gun" | "melee"
        public double? freshCost; // null: the web's undefined (arsenal.js use() then does nothing)
    }

    /// <summary>config.weapons.catalog: catalog id -> entry, with the ids in the config's order.</summary>
    public sealed class WeaponCatalog
    {
        public readonly List<string> order = new List<string>();
        readonly Dictionary<string, WeaponInfo> byId = new Dictionary<string, WeaponInfo>();

        /// <summary>catalog[id] (null for an unknown or null id).</summary>
        public WeaponInfo Get(string id) => id != null && byId.TryGetValue(id, out WeaponInfo w) ? w : null;

        public bool Has(string id) => Get(id) != null;

        public static WeaponCatalog From(GameConfig cfg, string path = "weapons.catalog")
        {
            var c = new WeaponCatalog();
            foreach (var kv in cfg.Obj(path))
            {
                var e = PortData.Obj(kv.Value);
                c.order.Add(kv.Key);
                c.byId[kv.Key] = new WeaponInfo
                {
                    id = kv.Key,
                    name = e.TryGetValue("name", out object n) ? n as string : null,
                    family = e.TryGetValue("family", out object f) ? f as string : null,
                    kind = e.TryGetValue("kind", out object k) ? k as string : null,
                    freshCost = e.TryGetValue("freshCost", out object fc) ? PortData.Num(fc) : (double?)null,
                };
            }
            return c;
        }
    }

    /// <summary>config.weapons.arsenal, read once.</summary>
    public sealed class ArsenalParams
    {
        public double freshMin, freshMax, recover, recoverDelay, staleAt, primedWindow, primedBonus, swapKillWindow;
        public double crossWindow, crossMult, collapseMult, collapseRadius, overloadMult, overloadRadius, overloadDamage, overloadTargets;
        public double memory, burnDps, burnTick;
        /// <summary>statusTime: status name -> s (burn, shock, pull, stuck, stagger), in the config's order.</summary>
        public readonly Dictionary<string, double> statusTime = new Dictionary<string, double>();
        public readonly List<string> statusOrder = new List<string>();

        public static ArsenalParams From(GameConfig cfg, string path = "weapons.arsenal")
        {
            double N(string k) => cfg.Num(path + "." + k);
            var a = new ArsenalParams
            {
                freshMin = N("freshMin"),
                freshMax = N("freshMax"),
                recover = N("recover"),
                recoverDelay = N("recoverDelay"),
                staleAt = N("staleAt"),
                primedWindow = N("primedWindow"),
                primedBonus = N("primedBonus"),
                swapKillWindow = N("swapKillWindow"),
                crossWindow = N("crossWindow"),
                crossMult = N("crossMult"),
                collapseMult = N("collapseMult"),
                collapseRadius = N("collapseRadius"),
                overloadMult = N("overloadMult"),
                overloadRadius = N("overloadRadius"),
                overloadDamage = N("overloadDamage"),
                overloadTargets = N("overloadTargets"),
                memory = N("memory"),
                burnDps = N("burnDps"),
                burnTick = N("burnTick"),
            };
            foreach (var kv in cfg.Obj(path + ".statusTime"))
            {
                a.statusOrder.Add(kv.Key);
                a.statusTime[kv.Key] = PortData.Num(kv.Value);
            }
            return a;
        }
    }

    /// <summary>
    /// An enemy's statuses {name: until (s of arsenal time)} with JS object semantics: a missing name reads as the web's
    /// `?? -Infinity`, a new name goes last, re-setting a name keeps its place. Allocation-free after the first sets.
    /// </summary>
    public sealed class StatusTimes
    {
        readonly List<string> names = new List<string>(5);
        readonly List<double> until = new List<double>(5);

        public int Count => names.Count;

        public string NameAt(int i) => names[i];

        public double UntilAt(int i) => until[i];

        public int IndexOf(string name)
        {
            for (int i = 0; i < names.Count; i++) if (names[i] == name) return i;
            return -1;
        }

        public bool Has(string name) => IndexOf(name) >= 0;

        /// <summary>statuses[name] ?? -Infinity.</summary>
        public double Get(string name)
        {
            int i = IndexOf(name);
            return i < 0 ? double.NegativeInfinity : until[i];
        }

        /// <summary>statuses[name] = value.</summary>
        public void Set(string name, double value)
        {
            int i = IndexOf(name);
            if (i >= 0) until[i] = value;
            else
            {
                names.Add(name);
                until.Add(value);
            }
        }

        public void Clear()
        {
            names.Clear();
            until.Clear();
        }
    }

    /// <summary>An enemy's arsenal record (arsenal.js recordOf): its last hit's family and time, and its statuses.</summary>
    public sealed class ArsenalRecord
    {
        public int id; // the enemy's id (a recycled enemy object with a new id starts a new record)
        public string family; // null: none
        public double at = double.NegativeInfinity;
        public StatusTimes statuses = new StatusTimes(); // arsenal.js's probe shares its record's object
    }

    /// <summary>The primed swap {weapon, at, used} (the weapon swapped in, when, and whether its first hit landed).</summary>
    public sealed class PrimedSwap
    {
        public string weapon;
        public double at;
        public bool used;
    }

    /// <summary>crossResult's {mult, cross, combo}.</summary>
    public struct CrossOutcome
    {
        public double mult;
        public bool cross;
        public string combo; // ArsenalMath.Collapse | ArsenalMath.Overload | null
    }

    public static class ArsenalMath
    {
        public const string Collapse = "COLLAPSE";
        public const string Overload = "OVERLOAD";

        /// <summary>Hit sources that aren't hotbar weapons map onto one (a ricochet bounce is the Kestrel's, F is the Gauntlet).</summary>
        static readonly Dictionary<string, string> Source = new Dictionary<string, string>
        {
            ["ricochet"] = "revolver",
            ["punch"] = "gauntlet",
            ["orb"] = "gauntlet",
            ["slam"] = "slam",
        };

        /// <summary>The catalog id a hit's weapon counts as.</summary>
        public static string WeaponOf(string weapon) => weapon != null && Source.TryGetValue(weapon, out string id) ? id : weapon;

        /// <summary>The family of a hit's weapon ('impact' for the slam; null for unknown sources).</summary>
        public static string FamilyOf(string weapon, WeaponCatalog catalog)
        {
            string id = WeaponOf(weapon);
            if (id == "slam") return "impact";
            return catalog.Get(id)?.family;
        }

        /// <summary>Is a hit's weapon a melee weapon (the punch counts)?</summary>
        public static bool IsMelee(string weapon, WeaponCatalog catalog) => catalog.Get(WeaponOf(weapon))?.kind == "melee";

        /// <summary>The damage + points multiplier at a freshness: lerp(min, max, fresh).</summary>
        public static double FreshMult(double fresh, ArsenalParams a)
        {
            double f = JsMath.Max(0, JsMath.Min(1, fresh));
            return a.freshMin + (a.freshMax - a.freshMin) * f;
        }

        /// <summary>FRESH after a use costing `cost` (never below 0).</summary>
        public static double SpendFresh(double fresh, double cost) => JsMath.Max(0, fresh - cost);

        /// <summary>FRESH after `dt` s out of hand (never above 1). `mult`: the FLIP perk's 2x.</summary>
        public static double RecoverFresh(double fresh, double dt, double rate, double mult = 1) => JsMath.Min(1, fresh + rate * mult * dt);

        /// <summary>
        /// The primed multiplier for a hit at time `now`: 1 + bonus when it is the first hit with `weapon` within
        /// `window` s of swapping it in. `primed` {weapon, at, used}.
        /// </summary>
        public static double PrimedMult(PrimedSwap primed, string weapon, double now, double window, double bonus)
        {
            if (primed == null || primed.used || primed.weapon != WeaponOf(weapon)) return 1;
            return now - primed.at <= window ? 1 + bonus : 1;
        }

        /// <summary>
        /// The cross combo for a hit on an enemy: `record` {family, at, statuses: {name: until}} (the enemy's last hit, or
        /// null). Returns {mult, cross, combo} where combo is 'COLLAPSE' | 'OVERLOAD' | null. Named combos take priority
        /// over a plain cross and consume their status.
        /// </summary>
        public static CrossOutcome CrossResult(ArsenalRecord record, string family, string weapon, double now, ArsenalParams a, WeaponCatalog catalog)
        {
            var @out = new CrossOutcome { mult = 1, cross = false, combo = null };
            if (record == null || string.IsNullOrEmpty(family)) return @out;
            StatusTimes st = record.statuses; // ?? {}: Status() reads a null as empty
            if (family == "blast" && Status(st, "pull") > now)
            {
                @out.mult = a.collapseMult;
                @out.combo = Collapse;
                return @out;
            }
            if (IsMelee(weapon, catalog) && Status(st, "shock") > now)
            {
                @out.mult = a.overloadMult;
                @out.combo = Overload;
                return @out;
            }
            if (!string.IsNullOrEmpty(record.family) && record.family != family && now - record.at <= a.crossWindow)
            {
                @out.mult = a.crossMult;
                @out.cross = true;
            }
            return @out;
        }

        /// <summary>st[name] ?? -Infinity, with a null `st` as the web's `{}`.</summary>
        static double Status(StatusTimes st, string name) => st == null ? double.NegativeInfinity : st.Get(name);

        /// <summary>
        /// The hotbar: keys 1-9 then 0 select the first ten weapons you own (in purchase order); anything past that is
        /// picked from the radial or the wheel. Returns the slot index for a digit key ('1'..'9', '0'), or -1.
        /// (For a digit key, Number(digit) - 1 is a whole number, so the index is returned as an int.)
        /// </summary>
        public static int SlotForDigit(string digit, int owned)
        {
            double i = digit == "0" ? 9 : JsNumber(digit) - 1;
            return i >= 0 && i < owned ? (int)i : -1;
        }

        /// <summary>JS Number(string) for decimal text: trimmed, "" is 0, anything unparsable NaN.</summary>
        static double JsNumber(string s)
        {
            if (s == null) return double.NaN;
            string t = s.Trim();
            if (t.Length == 0) return 0;
            return double.TryParse(t, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out double v) ? v : double.NaN;
        }

        /// <summary>The wheel: the next slot in `dir` (+1 / -1), wrapping over every weapon you own.</summary>
        public static int CycleSlot(int current, int owned, double dir)
        {
            if (owned <= 0) return 0;
            return (current + (dir > 0 ? 1 : -1) + owned) % owned;
        }

        /// <summary>
        /// The radial picker: which of `count` sectors a mouse offset (x right, y down, px) points at, or -1 inside the
        /// dead zone. Sector 0 is straight up, going clockwise.
        /// </summary>
        public static int RadialSector(double x, double y, int count, double deadZone)
        {
            if (count <= 0 || JsMath.Hypot(x, y) < deadZone) return -1;
            double angle = JsMath.Atan2(x, -y); // 0 = up, clockwise positive // JsMath: atan2
            double turn = ((angle / (Math.PI * 2)) + 1) % 1;
            return (int)(Math.Floor(turn * count + 0.5) % count);
        }
    }
}
