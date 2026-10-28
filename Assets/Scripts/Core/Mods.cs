// NIGHT8 U2-13: MODS, pick 1 of 3 run upgrades. Ports of survival/modMath.js (pure: which rounds end in a draft, the draft
// roll weighted by tier with no duplicates, no maxed mod and at least two tags, the pick statistics) and mods.js (a cleared
// draft round queues a draft; the owner opens it as a screen that pauses the game until you pick; one paid reroll a run;
// two seeded streams per run: the drafts and the chance rolls; modLine's card text).
// The mods whose systems exist here are offered: SKYDANCER (AIR), WALLBANGER (WALL; locked behind the PROTOCOL flag as on
// the web), LANCE, STYLE TAX (locked: REACH ROUND 10), VARIETY, BLOOD PRICE, INTEREST (locked: FINISH A DAILY RUN),
// SCAVENGER, ADRENALINE, BOUNTY HUNTER. The other nine read systems not ported yet (the arsenal's statuses, PRIMED and
// CROSS shots, momentum, blasts): RAILGRINDER, CONDUCTOR, PYRE, KEEN EDGE, GRAVITY WELL, SHRAPNEL, MOMENTUM ROUNDS,
// FRESH START, CROSSFIRE (U3). Events: modDraft {round, offer}, modPick {id, stacks}, modReroll {offer}.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Skygrind.Core
{
    public sealed class ModDef
    {
        public string id, name, tag, tier, desc, lockFlag, lockLabel;
        public int max;
        public double per, @base, cap, range, below, fromRank, radius;
    }

    public static class ModMath
    {
        public sealed class M
        {
            public int offers, rerolls, listUntil, everyAfter;
            public double rerollCost;
            public uint seedSalt, rollSalt;
            public bool enabled;
            public readonly List<int> draftRounds = new List<int>();
            public readonly Dictionary<string, double> tierWeight = new Dictionary<string, double>();
        }

        /// <summary>isDraftRound: the listed rounds up to listUntil, then every everyAfter rounds.</summary>
        public static bool IsDraftRound(int round, M m)
        {
            if (round <= m.listUntil) return m.draftRounds.Contains(round);
            return round % m.everyAfter == 0;
        }

        /// <summary>
        /// rollDraft: m.offers distinct ids from `ids`, weighted by tier, skipping maxed mods; the last offer prefers a tag
        /// the others don't have (two tags at least when the pool allows). `exclude`: ids not to offer (a reroll).
        /// </summary>
        public static List<string> RollDraft(Func<double> rng, List<string> ids, Dictionary<string, ModDef> defs, Dictionary<string, int> stacks, M m, List<string> exclude = null)
        {
            int N(string id) => stacks.TryGetValue(id, out int k) ? k : 0;
            var pool = ids.FindAll(id => defs.ContainsKey(id) && N(id) < defs[id].max && (exclude == null || !exclude.Contains(id)));
            if (pool.Count < m.offers) pool = ids.FindAll(id => defs.ContainsKey(id) && N(id) < defs[id].max); // a thin pool: allow repeats of the last offer
            var pick = new List<string>();
            double Weight(string id) => m.tierWeight.TryGetValue(defs[id].tier, out double w) ? w : 1;
            for (int k = 0; k < m.offers; k++)
            {
                var cands = pool.FindAll(id => !pick.Contains(id));
                if (cands.Count == 0) break;
                if (k == m.offers - 1 && k > 0)
                {
                    var tags = new HashSet<string>();
                    foreach (string id in pick) tags.Add(defs[id].tag);
                    if (tags.Count < 2)
                    {
                        var other = cands.FindAll(id => !tags.Contains(defs[id].tag));
                        if (other.Count > 0) cands = other;
                    }
                }
                double total = 0;
                foreach (string id in cands) total += Weight(id);
                double r = rng() * total;
                string chosen = cands[cands.Count - 1];
                foreach (string id in cands)
                {
                    r -= Weight(id);
                    if (r < 0)
                    {
                        chosen = id;
                        break;
                    }
                }
                pick.Add(chosen);
            }
            return pick;
        }

        /// <summary>recordPick: a draft's offers and the pick into stats {id: (offered, picked)} (a new dictionary).</summary>
        public static Dictionary<string, (int offered, int picked)> RecordPick(Dictionary<string, (int offered, int picked)> stats, List<string> offered, string picked)
        {
            var output = new Dictionary<string, (int offered, int picked)>(stats);
            foreach (string id in offered) output[id] = ((output.TryGetValue(id, out var s) ? s.offered : 0) + 1, output.TryGetValue(id, out var s2) ? s2.picked : 0);
            if (picked != null) output[picked] = (output.TryGetValue(picked, out var p) ? p.offered : 1, (output.TryGetValue(picked, out var p2) ? p2.picked : 0) + 1);
            return output;
        }

        /// <summary>pickRate: picked / offered, or null before it was offered.</summary>
        public static double? PickRate(Dictionary<string, (int offered, int picked)> stats, string id) =>
            stats.TryGetValue(id, out var s) && s.offered > 0 ? (double)s.picked / s.offered : (double?)null;

        static string Js(double v) => v.ToString("R", CultureInfo.InvariantCulture);

        /// <summary>mods.js modLine: a mod's card text with its numbers at stack k.</summary>
        public static string ModLine(ModDef d, int k) => d.desc
            .Replace("{p}", Js(JsMath.Round(d.per * k * 100)))
            .Replace("{n}", Js(JsMath.Round(d.per * k)))
            .Replace("{r}", Js(d.@base + d.per * k))
            .Replace("{c}", Js(d.cap * k));
    }

    public sealed class ModEvent
    {
        public int round;
        public string id;
        public int stacks;
        public List<string> offer;
        public bool free; // NIGHT12: mods.js give's modPick (the DAILY's first mod: not a draft pick)
    }

    /// <summary>mods.js: the run's mods, the drafts, the effects the ported systems can feel.</summary>
    public sealed class Mods : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the stacks (in their order), an open draft and its round, the rerolls left,
        // the drafts waiting (a break's draft opens when the game resumes), the switch, both streams, VARIETY's last
        // family, the offered / picked stats. `version` is bumped (the HUD redraws), not saved.
        public void Save(StateWriter w)
        {
            w.SI("stacks", stacks);
            w.B("hasOffer", offer != null);
            w.Strings("offer", offer ?? new List<string>());
            w.I("offerRound", offerRound);
            w.I("rerollsLeft", rerollsLeft);
            w.Ints("pending", pending);
            w.B("active", active);
            w.Rng("draftRng", draftRng);
            w.Rng("rollRng", rollRng);
            w.S("lastFamily", lastFamily);
            var sk = new List<string>(stats.Count);
            var so = new List<int>(stats.Count);
            var sp = new List<int>(stats.Count);
            foreach (var kv in stats)
            {
                sk.Add(kv.Key);
                so.Add(kv.Value.offered);
                sp.Add(kv.Value.picked);
            }
            w.Strings("statIds", sk);
            w.Ints("statOffered", so);
            w.Ints("statPicked", sp);
        }

        public void Load(StateReader r)
        {
            List<string> sk = r.Strings("statIds");
            List<int> so = r.Ints("statOffered"), sp = r.Ints("statPicked"), pend = r.Ints("pending");
            if (so.Count != sk.Count || sp.Count != sk.Count) throw new StateException("snapshot: mods.stats");
            var st = new Dictionary<string, (int offered, int picked)>();
            for (int i = 0; i < sk.Count; i++) st.Add(sk[i], (so[i], sp[i]));
            List<string> o = r.Strings("offer");
            r.SI("stacks", stacks);
            offer = r.B("hasOffer") ? o : null;
            offerRound = r.Int("offerRound");
            rerollsLeft = r.Int("rerollsLeft");
            pending.Clear();
            pending.AddRange(pend);
            active = r.B("active");
            draftRng = r.Rng("draftRng");
            draftNext = draftRng.Next;
            rollRng = r.Rng("rollRng");
            lastFamily = r.S("lastFamily");
            stats = st;
            version++;
        }

        /// <summary>The mods whose effects run here (the rest need the arsenal, momentum or blasts: U3).</summary>
        public static readonly HashSet<string> Ported = new HashSet<string> { "skydancer", "wallbanger", "lance", "styleTax", "variety", "bloodPrice", "interest", "scavenger", "adrenaline", "bountyHunter" };

        readonly GameConfig cfg;
        readonly EventBus events;
        public readonly ModMath.M m = new ModMath.M();
        public readonly Dictionary<string, ModDef> defs = new Dictionary<string, ModDef>();
        public readonly List<string> ids = new List<string>(); // config order
        readonly HashSet<string> toughKinds = new HashSet<string>();
        readonly Dictionary<string, string> familyOf = new Dictionary<string, string>();
        public readonly Dictionary<string, int> stacks = new Dictionary<string, int>();
        public List<string> offer; // the open draft, or null
        public int offerRound, rerollsLeft;
        readonly List<int> pending = new List<int>();
        bool active;
        Mulberry32 draftRng = new Mulberry32(1), rollRng = new Mulberry32(2);
        Func<double> draftNext;
        string lastFamily;
        public Dictionary<string, (int offered, int picked)> stats = new Dictionary<string, (int, int)>();
        public int version; // + 1 on every change (the HUD)

        // The owner's hooks
        public Func<string, bool> isUnlocked = _ => false; // a locked mod's flag (no unlocks yet but REACH ROUND 10)
        public Func<bool> airborne = () => false;
        public Func<string> moveState = () => "GROUND";
        public Func<Vec3> eye = () => default;
        public Func<int> styleRank = () => 0;
        public Health health;
        public Economy economy;

        public Mods(GameConfig cfg, EventBus events)
        {
            this.cfg = cfg;
            this.events = events;
            m.enabled = cfg.Bool("mods.enabled");
            m.offers = cfg.Int("mods.offers");
            m.rerolls = cfg.Int("mods.rerolls");
            m.rerollCost = cfg.Num("mods.rerollCost");
            m.seedSalt = (uint)cfg.Num("mods.seedSalt");
            m.rollSalt = (uint)cfg.Num("mods.rollSalt");
            m.listUntil = cfg.Int("mods.listUntil");
            m.everyAfter = cfg.Int("mods.everyAfter");
            foreach (double r in cfg.Nums("mods.draftRounds")) m.draftRounds.Add((int)r);
            foreach (var kv in cfg.Obj("mods.tierWeight")) m.tierWeight[kv.Key] = PortData.Num(kv.Value);
            foreach (var kv in cfg.Obj("mods.list"))
            {
                var o = PortData.Obj(kv.Value);
                double N(string k) => o.TryGetValue(k, out object v) ? PortData.Num(v) : 0;
                var d = new ModDef
                {
                    id = kv.Key, name = (string)o["name"], tag = (string)o["tag"], tier = (string)o["tier"], desc = (string)o["desc"], max = (int)N("max"),
                    per = N("per"), @base = N("base"), cap = N("cap"), range = N("range"), below = N("below"), fromRank = N("fromRank"), radius = N("radius"),
                };
                if (o.TryGetValue("locked", out object lk) && lk is Dictionary<string, object> l)
                {
                    d.lockFlag = (string)l["flag"];
                    d.lockLabel = (string)l["label"];
                }
                defs[d.id] = d;
                ids.Add(d.id);
            }
            foreach (string k in cfg.Strs("forge.toughKinds")) toughKinds.Add(k);
            foreach (var kv in cfg.Obj("weapons.catalog"))
            {
                if (PortData.Obj(kv.Value).TryGetValue("family", out object f) && f is string fs) familyOf[kv.Key] = fs;
            }
            events.On<RoundEvent>("roundClear", e =>
            {
                if (!active || !m.enabled) return;
                // INTEREST pays first (on the bank you cleared the round with).
                if (N("interest") > 0) economy?.Grant(Math.Min(defs["interest"].cap * N("interest"), Math.Floor(economy.total * Per("interest"))), "interest");
                if (big != null && big.Active)
                {
                    if (big.IsBigRound(e.number)) pending.Add(e.number); // NIGHT24: only the BIG PICK's rounds draft
                    return;
                }
                if (ModMath.IsDraftRound(e.number, m)) pending.Add(e.number);
            });
            events.On<KillEvent>("kill", After);
        }

        /// <summary>NIGHT24 (builder F, design.n24.bigPick): the BIG PICK. Set by the sim at a run's start (null or inactive: the draft is NIGHT21's, bit for bit).
        /// Active: no draft but the BIG PICK's rounds (5, 10, 15 ...), whose cards are BigPick24's ids; no reroll, no stacks, no stats.</summary>
        public BigPick24 big;

        /// <summary>The open draft is a BIG PICK (its cards are BigPick24 ids, not mod ids).</summary>
        public bool BigOffer => offer != null && big != null && big.Active;

        public int N(string id) => stacks.TryGetValue(id, out int k) ? k : 0;

        double Per(string id) => defs[id].per * N(id);

        /// <summary>The pool a draft is rolled from: ported, and unlocked (a locked mod waits for its flag, as mods.js isUnlocked).</summary>
        public List<string> Pool() => ids.FindAll(id => Ported.Contains(id) && (defs[id].lockFlag == null || isUnlocked(defs[id].lockFlag)));

        /// <summary>A draft is waiting to open.</summary>
        public bool Pending => active && m.enabled && pending.Count > 0 && offer == null;

        /// <summary>A new run: no mods, the streams seeded from the run seed.</summary>
        public void Start(uint seed)
        {
            stacks.Clear();
            offer = null;
            pending.Clear();
            rerollsLeft = m.rerolls;
            draftRng = new Mulberry32((seed ^ m.seedSalt) >> 0);
            draftNext = draftRng.Next;
            rollRng = new Mulberry32((seed ^ m.rollSalt) >> 0);
            lastFamily = null;
            active = true;
            if (big != null && big.WantsOn) big.Reset(seed); // NIGHT24: the BIG PICK's stream from the run seed (the sim set `big` and its WantsOn before this)
            else big?.Off();
            version++;
        }

        public void Stop()
        {
            active = false;
            offer = null;
            pending.Clear();
            version++;
        }

        /// <summary>Debug / the smoke: queue a draft as if `round` had just been cleared.</summary>
        public void Queue(int round)
        {
            if (active) pending.Add(round);
        }

        /// <summary>NIGHT24 (builder F): the CAMPAIGN's BIG PICK at a layer's end (D-7): queued as round 0 (the screen reads LAYER CLEARED; the points card pays x1).</summary>
        public void QueueLayer()
        {
            if (active && big != null && big.Active) pending.Add(0);
        }

        /// <summary>A layer-end BIG PICK is waiting to open. The campaign never reaches a break, so the game opens it without one
        /// (SurvivalGame.Update: `run.state == Break || mods.LayerPick`).</summary>
        public bool LayerPick => Pending && pending[0] == 0 && big != null && big.Active;

        /// <summary>Open the next waiting draft (or keep the open one): the offer.</summary>
        public List<string> Open()
        {
            if (offer != null) return offer;
            if (!Pending) return null;
            offerRound = pending[0];
            bool bigPick = big != null && big.Active; // NIGHT24: the BIG PICK's cards, from its own stream
            offer = bigPick ? big.Roll() : ModMath.RollDraft(draftNext ?? (draftNext = draftRng.Next), Pool(), defs, stacks, m);
            if (bigPick) big.Opened(offerRound, offer);
            version++;
            events.Emit("modDraft", new ModEvent { round = offerRound, offer = new List<string>(offer) });
            return offer;
        }

        /// <summary>Take card `i` of the open draft.</summary>
        public bool Pick(int i)
        {
            if (offer == null || i < 0 || i >= offer.Count) return false;
            string id = offer[i];
            if (big != null && big.Active)
            {
                // NIGHT24: a BIG PICK card: its reward, no stack, no stats; modPick carries the card's id (the tape's DraftPick finds its index in the offer)
                big.Take(id, offerRound);
                offer = null;
                pending.RemoveAt(0);
                version++;
                events.Emit("modPick", new ModEvent { id = id, stacks = 1 });
                return true;
            }
            stacks[id] = N(id) + 1;
            stats = ModMath.RecordPick(stats, offer, id);
            offer = null;
            pending.RemoveAt(0);
            version++;
            events.Emit("modPick", new ModEvent { id = id, stacks = stacks[id] });
            return true;
        }

        /// <summary>NIGHT12: mods.js give: own a mod outside a draft (the DAILY's first mod), capped at its max; modPick with free.</summary>
        public bool Give(string id, int k = 1)
        {
            if (id == null || !defs.TryGetValue(id, out ModDef def)) return false;
            stacks[id] = Math.Min(def.max, N(id) + k);
            version++;
            events.Emit("modPick", new ModEvent { id = id, stacks = stacks[id], free = true });
            return true;
        }

        /// <summary>One paid reroll a run: new offers (the last ones left out when the pool allows).</summary>
        public bool Reroll()
        {
            if (big != null && big.Active) return false; // NIGHT24: the BIG PICK has no reroll
            if (offer == null || rerollsLeft <= 0 || economy == null || !economy.Spend(m.rerollCost, "modReroll")) return false;
            rerollsLeft--;
            offer = ModMath.RollDraft(draftNext, Pool(), defs, stacks, m, offer);
            version++;
            events.Emit("modReroll", new ModEvent { offer = new List<string>(offer) });
            return true;
        }

        /// <summary>SCAVENGER: x the power-up drop chance.</summary>
        public double DropMult() => 1 + (defs.ContainsKey("scavenger") ? Per("scavenger") : 0);

        /// <summary>The bonus hook (weapons.js loadout.bonus; mods.js mult): x damage of a hit on `e` (1 with no mods or outside a run).</summary>
        public double Mult(Enemy e)
        {
            if (!active) return 1;
            double k = 1;
            if (N("skydancer") > 0 && airborne()) k *= 1 + Per("skydancer");
            if (N("lance") > 0 && e != null)
            {
                Vec3 o = eye();
                if (JsMath.Hypot(e.position.x - o.x, e.position.y - o.y, e.position.z - o.z) > defs["lance"].range) k *= 1 + Per("lance");
            }
            if (N("styleTax") > 0) k *= 1 + Per("styleTax") * Math.Max(0, styleRank() - defs["styleTax"].fromRank);
            if (N("adrenaline") > 0 && health != null && health.hp < health.max * defs["adrenaline"].below) k *= 1 + Per("adrenaline");
            return k;
        }

        /// <summary>After a kill (mods.js after): points and heals by the ported mods.</summary>
        void After(KillEvent e)
        {
            if (!active || e.owner != "player") return;
            string state = moveState();
            if (N("wallbanger") > 0 && state == "WALLRIDE") economy?.Grant(Per("wallbanger"), "wallbanger");
            if (N("bloodPrice") > 0) health?.Regen(Per("bloodPrice"));
            if (N("bountyHunter") > 0 && toughKinds.Contains(e.kind)) economy?.Grant(Per("bountyHunter"), "bountyHunter");
            string family = e.weapon != null && familyOf.TryGetValue(e.weapon, out string f) ? f : null;
            if (N("variety") > 0 && family != null && lastFamily != null && family != lastFamily) economy?.Grant(Per("variety"), "variety");
            if (family != null) lastFamily = family;
        }

        /// <summary>A mod's card colour: its tag's weapon family colour, else the style tag's, else colors.usable.</summary>
        public uint TagColor(string tag) => cfg.Has($"weapons.families.{tag}") ? (uint)cfg.Num($"weapons.families.{tag}") : cfg.Has($"mods.styleTags.{tag}") ? (uint)cfg.Num($"mods.styleTags.{tag}") : (uint)cfg.Num("colors.usable");
    }
}
