// NIGHT8 U2-11: perk machines. Ports of combat/weaponMath.js perkPrice + perkEffects (pure: all 14 perks' numbers, as
// the web's tests check them) and survival/perks.js's state (owned in purchase order, the stack tax, free tokens, give,
// reset; the effects applied through the owner's hook). The machines that ship are the perks whose systems exist here:
// NIGHT8's starter five, VIGOR (+1 stamina pip), MEND (faster regen), PLATE (150 max HP), QUICK (fire rate x1.15, swaps
// x2) and GRIP (+0.6 s wall ride, +1 wall jump); NIGHT9's FLIP (the arsenal's recovery and primed bonus, swaps), BOUNTY
// (x points), DEADEYE (x headshot damage, + headshot points), FLOW (x style drain, x gain) and SLIPSTREAM (slide speed,
// friction, jump cap). STEADY, ANVIL, STATIC and PHOENIX wait for their systems (N12: their numbers are computed here,
// their machines aren't placed). Events: perk {id, free, price}.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>weaponMath.js perkEffects' result: a perk set as plain numbers.</summary>
    public sealed class PerkEffects
    {
        public int bonusPips;
        public RegenMath.Perk? regen; // MEND: x delay, x rate
        public double? maxHp; // PLATE
        public double fireRate = 1, swap = 1, charge = 1; // QUICK (FLIP's swap)
        public double bonusRideTime;
        public int bonusWallJumps;
        public double? assistCone, assistLockRange; // STEADY
        public double recoil = 1;
        public double? arsenalRecover, arsenalPrimedBonus; // FLIP
        public double points = 1; // BOUNTY
        public double? slideSpeed, slideFriction, slideJumpCap; // SLIPSTREAM
        public double slamDamage = 1, slamRadius = 1; // ANVIL
        public double? headshotMult, headshotPoints; // DEADEYE
        public double styleDrain = 1, styleGain = 1; // FLOW
        public bool staticAura, phoenix;
    }

    public static class PerkMath
    {
        /// <summary>perkPrice: base x (1 + tax per perk already owned), to the nearest 10; a free token makes it 0.</summary>
        public static double PerkPrice(double basePrice, int ownedCount, double tax, bool free = false)
        {
            if (free) return 0;
            return JsMath.Round(basePrice * (1 + tax * ownedCount) / 10) * 10;
        }

        /// <summary>perkEffects(owned, config.perks, config.weapons.swapTime).</summary>
        public static PerkEffects Effects(IList<string> owned, GameConfig cfg)
        {
            bool Has(string id) => owned.Contains(id);
            double P(string path) => cfg.Num("perks." + path);
            double baseSwapTime = cfg.Num("weapons.swapTime");
            var fx = new PerkEffects();
            // Swap speed: QUICK doubles it; FLIP makes a swap take flip.swapTime at most (the faster one wins).
            fx.swap = Math.Max(Has("quick") ? P("quick.swapMult") : 1, Has("flip") ? baseSwapTime / P("flip.swapTime") : 1);
            fx.bonusPips = Has("vigor") ? (int)P("vigor.pips") : 0;
            if (Has("mend")) fx.regen = new RegenMath.Perk { delay = P("mend.delayMult"), rate = P("mend.rateMult") };
            if (Has("plate")) fx.maxHp = P("plate.maxHp");
            fx.fireRate = Has("quick") ? P("quick.fireRateMult") : 1;
            fx.charge = Has("quick") ? P("quick.chargeMult") : 1;
            fx.bonusRideTime = Has("grip") ? P("grip.rideTime") : 0;
            fx.bonusWallJumps = Has("grip") ? (int)P("grip.wallJumps") : 0;
            if (Has("steady"))
            {
                fx.assistCone = P("steady.cone");
                fx.assistLockRange = P("steady.lockRange");
                fx.recoil = P("steady.recoil");
            }
            if (Has("flip"))
            {
                fx.arsenalRecover = P("flip.recover");
                fx.arsenalPrimedBonus = P("flip.primedBonus");
            }
            fx.points = Has("bounty") ? P("bounty.points") : 1;
            if (Has("slipstream"))
            {
                fx.slideSpeed = P("slipstream.slideSpeed");
                fx.slideFriction = P("slipstream.slideFriction");
                fx.slideJumpCap = P("slipstream.slideJumpCap");
            }
            if (Has("anvil"))
            {
                fx.slamDamage = P("anvil.slamDamage");
                fx.slamRadius = P("anvil.slamRadius");
            }
            if (Has("deadeye"))
            {
                fx.headshotMult = P("deadeye.headshotMult");
                fx.headshotPoints = P("deadeye.headshotPoints");
            }
            if (Has("flow"))
            {
                fx.styleDrain = P("flow.drain");
                fx.styleGain = P("flow.gain");
            }
            fx.staticAura = Has("static");
            fx.phoenix = Has("phoenix");
            return fx;
        }
    }

    public sealed class PerkEvent
    {
        public string id;
        public bool free;
        public double price;
    }

    /// <summary>perks.js: the owned perks of a run, the prices, tokens; `apply` receives the effects on every change.</summary>
    public sealed class Perks : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the perks owned (purchase order) and the free tokens; `fx` is recomputed.
        // `apply` is NOT called: each effect (the mover's pips, regen, the loadout, the economy, the style, max HP) is a
        // field of the system it changed, restored by that system's own section.
        public void Save(StateWriter w)
        {
            w.Strings("owned", owned);
            w.I("tokens", tokens);
        }

        public void Load(StateReader r)
        {
            List<string> o = r.Strings("owned");
            foreach (string id in o) if (id == null || !basePrice.ContainsKey(id)) throw new StateException($"snapshot: an unknown perk '{id}'");
            owned.Clear();
            owned.AddRange(o);
            tokens = r.Int("tokens");
            fx = PerkMath.Effects(owned, cfg);
        }

        /// <summary>The perks whose effects Unity applies (their machines are placed; STEADY, ANVIL, STATIC, PHOENIX wait for N12).</summary>
        public static readonly HashSet<string> Ported = new HashSet<string> { "vigor", "mend", "plate", "quick", "grip", "flip", "bounty", "deadeye", "flow", "slipstream" }; // NIGHT9: FLIP (U3-08), BOUNTY / DEADEYE / FLOW / SLIPSTREAM (U3-11)

        readonly GameConfig cfg;
        readonly EventBus events;
        public readonly List<string> owned = new List<string>(); // in purchase order (the HUD row)
        public int tokens; // free perks (a boss kill; no boss yet)
        public PerkEffects fx;
        public Action<PerkEffects> apply = _ => { };
        readonly double tax;
        readonly Dictionary<string, double> basePrice = new Dictionary<string, double>(); // config.perks.<id>.price (read per step: no path strings)

        public Perks(GameConfig cfg, EventBus events)
        {
            this.cfg = cfg;
            this.events = events;
            tax = cfg.Num("perks.stackTax");
            foreach (string id in cfg.Strs("perks.order")) basePrice[id] = cfg.Num($"perks.{id}.price");
            fx = PerkMath.Effects(owned, cfg);
        }

        public double Price(string id) => PerkMath.PerkPrice(basePrice[id], owned.Count, tax, tokens > 0);

        public bool Owns(string id) => owned.Contains(id);

        /// <summary>Own a perk (a buy spends a token first when there is one).</summary>
        public bool Give(string id, double price = 0)
        {
            if (owned.Contains(id) || !basePrice.ContainsKey(id)) return false;
            bool free = tokens > 0;
            if (free) tokens--;
            owned.Add(id);
            fx = PerkMath.Effects(owned, cfg);
            apply(fx);
            events.Emit("perk", new PerkEvent { id = id, free = free, price = price });
            return true;
        }

        /// <summary>Drop every perk and token (a new run).</summary>
        public void Reset()
        {
            owned.Clear();
            tokens = 0;
            fx = PerkMath.Effects(owned, cfg);
            apply(fx);
        }
    }
}
