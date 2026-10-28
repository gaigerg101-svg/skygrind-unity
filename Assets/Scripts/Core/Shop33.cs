// NIGHT33 P (DESIGN33 card P, D-6, row design.n33.shop33): campaign points that buy something. GG 2026-10-11: "why do we have points in the campaign if
// we're not gonna use them ... maybe you can upgrade your weapon or a skill tree". The pure half: the catalogue (eight upgrades from the design config),
// the prices, the owned / equipped encoding, the buy and equip rules, the bank (the WALLET carried across levels) and the effects a run applies.
//
// THE ENCODING (SurvivalSim.shop33Owned, Profile.shop33, the tape's shop33 VarI): bit i = item i OWNED, bit 8 + i = item i EQUIPPED, where i is the
// item's index in design.n33.shop33.items (append only: a saved profile or tape names items by index). A run applies only items that are both owned
// and equipped, and never more equipped in a slot than the slot holds (Sanitize). 0 = nothing owned: the run is unchanged.
//
// THE BANK (the lead's decision, 11:04): the WALLET (Profile.bank33) is a separate campaign currency. A run's own points stay NIGHT32's (the level
// starts at points.start; in-level buys spend them); at the level's end (layerClear) its earned points are banked into the wallet BEST-OF per level
// (Bank: only what beats that level's best earn so far, so a replay never earns twice); the SHOP spends the wallet between levels. The tape's bank33
// stays 0 (no run reads the wallet); only the owned / equipped upgrades change a run.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>One upgrade (design.n33.shop33.items[i]).</summary>
    public sealed class Shop33Item
    {
        public int index;
        public string id = "", name = "", line = "", slot = "";
        /// <summary>0 / 1 / 2: the price is tierMult[tier] x typicalEarn.</summary>
        public int tier;
        /// <summary>The effect's number (a multiplier or an addition; see Shop33Effects).</summary>
        public double value;
        public int price;
    }

    /// <summary>What a run's equipped upgrades change (the neutral value: nothing changes).</summary>
    public struct Shop33Effects
    {
        /// <summary>x the KESTREL's full-charge seconds (1 = unchanged).</summary>
        public double kestrelChargeMult;
        /// <summary>+ bodies the THUNDERCLAP's charged slug goes through after the first (0 = NIGHT31's one body).</summary>
        public int slugPierce;
        /// <summary>x the STITCHER's detonate damage (1 = unchanged).</summary>
        public double detonateMult;
        /// <summary>x the PULSE GAUNTLET's LT parry window (1 = unchanged).</summary>
        public double parryWindowMult;
        /// <summary>+ grapple charges (0 = unchanged).</summary>
        public int grappleCharges;
        /// <summary>x the dash's recharge time (the stamina refill delays x this, the rate / this; 1 = unchanged).</summary>
        public double dashRecoverMult;
        /// <summary>+ max health (0 = unchanged).</summary>
        public double maxHealth;
        /// <summary>HP a player's kill heals (0 = none).</summary>
        public double killHeal;

        public static Shop33Effects Neutral => new Shop33Effects { kestrelChargeMult = 1, detonateMult = 1, parryWindowMult = 1, dashRecoverMult = 1 };

        /// <summary>Does anything change?</summary>
        public bool Any => kestrelChargeMult != 1 || slugPierce != 0 || detonateMult != 1 || parryWindowMult != 1 || grappleCharges != 0 || dashRecoverMult != 1 || maxHealth != 0 || killHeal != 0;

        public override string ToString() =>
            $"kestrelCharge x{kestrelChargeMult}, slugPierce +{slugPierce}, detonate x{detonateMult}, parryWindow x{parryWindowMult}, grapple +{grappleCharges}, dashRecover x{dashRecoverMult}, maxHealth +{maxHealth}, killHeal {killHeal}";
    }

    /// <summary>The shop's catalogue and rules (design.n33.shop33), read once.</summary>
    public sealed class Shop33Catalogue
    {
        public const string Weapon = "weapon", Movement = "movement", Wildcard = "wildcard";
        /// <summary>The slots in the rack's order (the screen's columns).</summary>
        public static readonly string[] Slots = { Weapon, Movement, Wildcard };
        /// <summary>The encoding's room: 8 items (bits 0-7 owned, 8-15 equipped).</summary>
        public const int MaxItems = 8;

        public readonly List<Shop33Item> items = new List<Shop33Item>();
        public readonly Dictionary<string, int> slotSize = new Dictionary<string, int>();
        public double typicalEarn;

        /// <summary>design.n33.shop33 (an empty catalogue when absent: NIGHT32's config).</summary>
        public static Shop33Catalogue From(GameConfig cfg)
        {
            var c = new Shop33Catalogue();
            const string p = "design.n33.shop33.";
            if (!cfg.Has(p + "items")) return c;
            c.typicalEarn = cfg.Num(p + "typicalEarn");
            List<object> mult = cfg.Arr(p + "tierMult");
            double round = cfg.Num(p + "priceRound");
            foreach (string s in Slots) c.slotSize[s] = cfg.Int(p + "slots." + s);
            List<object> arr = cfg.Arr(p + "items");
            for (int i = 0; i < arr.Count && i < MaxItems; i++)
            {
                var o = (Dictionary<string, object>)arr[i];
                var it = new Shop33Item
                {
                    index = i,
                    id = (string)o["id"],
                    name = (string)o["name"],
                    line = (string)o["line"],
                    slot = (string)o["slot"],
                    tier = (int)PortData.Num(o["tier"]),
                    value = PortData.Num(o["value"]),
                };
                it.price = Price(c.typicalEarn, PortData.Num(mult[it.tier]), round);
                c.items.Add(it);
            }
            return c;
        }

        /// <summary>tierMult x typicalEarn, to the nearest `round` (half up).</summary>
        public static int Price(double typicalEarn, double tierMult, double round) =>
            (int)(Math.Floor(typicalEarn * tierMult / round + 0.5) * round);

        public int Count => items.Count;

        public Shop33Item Find(string id) => items.Find(x => x.id == id);

        /// <summary>The slot's size (0 for an unknown slot).</summary>
        public int SlotSize(string slot) => slot != null && slotSize.TryGetValue(slot, out int n) ? n : 0;

        // --- the encoding ------------------------------------------------------------------------------------------------

        public static bool Owns(int code, int i) => i >= 0 && i < MaxItems && (code & (1 << i)) != 0;
        public static bool IsEquipped(int code, int i) => i >= 0 && i < MaxItems && (code & (1 << (MaxItems + i))) != 0;

        /// <summary>Owned and equipped: what a run applies.</summary>
        public static bool Active(int code, int i) => Owns(code, i) && IsEquipped(code, i);

        /// <summary>Equipped items in `slot`.</summary>
        public int EquippedIn(int code, string slot)
        {
            int n = 0;
            foreach (Shop33Item it in items) if (it.slot == slot && Active(code, it.index)) n++;
            return n;
        }

        /// <summary>A code a run can trust: bits only for items in the catalogue, equipped only if owned, and no slot over its size (the
        /// highest-index extras are taken off). Pure: the same code in, the same code out.</summary>
        public int Sanitize(int code)
        {
            int mask = 0;
            for (int i = 0; i < Count; i++) mask |= 1 << i;
            int owned = code & mask;
            int equipped = (code >> MaxItems) & owned;
            int result = owned;
            var used = new Dictionary<string, int>();
            for (int i = 0; i < Count; i++)
            {
                if ((equipped & (1 << i)) == 0) continue;
                string s = items[i].slot;
                used.TryGetValue(s, out int n);
                if (n >= SlotSize(s)) continue;
                used[s] = n + 1;
                result |= 1 << (MaxItems + i);
            }
            return result;
        }

        /// <summary>Equip item i (owned only). When its slot is full, the slot's other equipped item with the lowest index comes off first.</summary>
        public int Equip(int code, int i)
        {
            code = Sanitize(code);
            if (i < 0 || i >= Count || !Owns(code, i) || IsEquipped(code, i)) return code;
            string slot = items[i].slot;
            while (EquippedIn(code, slot) >= SlotSize(slot) && SlotSize(slot) > 0)
            {
                int off = -1;
                for (int k = 0; k < Count; k++)
                    if (k != i && items[k].slot == slot && Active(code, k)) { off = k; break; }
                if (off < 0) break;
                code &= ~(1 << (MaxItems + off));
            }
            if (SlotSize(slot) <= 0) return code;
            return code | (1 << (MaxItems + i));
        }

        public int Unequip(int code, int i) => i < 0 || i >= MaxItems ? code : Sanitize(code & ~(1 << (MaxItems + i)));

        /// <summary>Equip if off, take off if on (the shop's A on an owned item).</summary>
        public int Toggle(int code, int i) => IsEquipped(Sanitize(code), i) ? Unequip(code, i) : Equip(code, i);

        /// <summary>Can the wallet buy item i (not owned yet, the price within it)?</summary>
        public bool CanBuy(int code, int wallet, int i) => i >= 0 && i < Count && !Owns(code, i) && wallet >= items[i].price;

        /// <summary>Buy item i: the price leaves the wallet, the item is owned and equipped (its slot's oldest comes off when full). False: nothing changed.</summary>
        public bool TryBuy(ref int code, ref int wallet, int i)
        {
            if (!CanBuy(code, wallet, i)) return false;
            wallet -= items[i].price;
            code = Equip(Sanitize(code) | (1 << i), i);
            return true;
        }

        /// <summary>The run's effects from a code (only owned and equipped items, after Sanitize).</summary>
        public Shop33Effects Effects(int code)
        {
            Shop33Effects e = Shop33Effects.Neutral;
            code = Sanitize(code);
            foreach (Shop33Item it in items)
            {
                if (!Active(code, it.index)) continue;
                switch (it.id)
                {
                    case "kestrelCharge": e.kestrelChargeMult *= it.value; break;
                    case "slugPierce": e.slugPierce += (int)it.value; break;
                    case "detonate": e.detonateMult *= it.value; break;
                    case "parryWindow": e.parryWindowMult *= it.value; break;
                    case "grappleCharge": e.grappleCharges += (int)it.value; break;
                    case "dashCooldown": e.dashRecoverMult *= it.value; break;
                    case "maxHealth": e.maxHealth += it.value; break;
                    case "killHeal": e.killHeal += it.value; break;
                }
            }
            return e;
        }
    }

    /// <summary>The bank, and the numbers the views read (the RESULTS screen's "points banked": builder U reads LastBanked / LastEarned / Wallet).</summary>
    public static class Shop33
    {
        /// <summary>A level's earned points, whole and never negative.</summary>
        public static int Whole(double earned) => (int)Math.Max(0, Math.Floor(earned + 0.5));

        /// <summary>The wallet after a level: the wallet before it plus the points banked (never negative, whole points).</summary>
        public static int WalletAfter(int walletBefore, double banked) => Math.Max(0, walletBefore) + Whole(banked);

        /// <summary>A cleared level banks its earned points into the wallet, BEST-OF: only what beats the level's best earn so far is added (the first clear
        /// banks it all; a replay banks only its improvement, so a replayed level never earns twice), and the best rises. Returns the points banked.</summary>
        public static int Bank(Profile p, string levelId, double earned)
        {
            if (p == null || string.IsNullOrEmpty(levelId)) return 0;
            int e = Whole(earned);
            int best = p.banked33.TryGetValue(levelId, out int b) ? b : 0;
            int add = Math.Max(0, e - best);
            if (e > best) p.banked33[levelId] = e;
            p.bank33 = WalletAfter(p.bank33, add);
            return add;
        }

        /// <summary>The points the last cleared campaign level added to the wallet (best-of: 0 on a replay that did not beat the level's best); -1 until a
        /// level with the row on is cleared. A view number (the RESULTS screen's "POINTS BANKED"), set by the game at layerClear.</summary>
        public static int LastBanked = -1;

        /// <summary>The points the last cleared level earned (its Economy.earned, whole); -1 until then. A view number.</summary>
        public static int LastEarned = -1;

        /// <summary>The wallet after the last cleared level (what the SHOP spends); -1 until a level with the row on is cleared. A view number.</summary>
        public static int Wallet = -1;
    }
}
