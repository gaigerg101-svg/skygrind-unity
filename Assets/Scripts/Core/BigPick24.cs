// NIGHT24 (builder F, contract big-pick; design.n24.bigPick): the BIG PICK. SURVIVAL has no draft before round 5; after the clear of round 5,
// 10, 15 ... (first + k x every) the game pauses on a pick of `cards` big cards, never a percentage and never a reroll:
//   box     FREE MYSTERY BOX SPIN   the Mystery Box spins for nothing, cannot come up a teddy, and the gun is put in your hand when the reveal ends
//   points  +1000 POINTS            x2 from round 10, x3 from round 20 (design points / multRounds / multValues)
//   pack    WEAPON PACK:<gun>       the held gun gains a FORGE tier (a damage tier; Forge.Upgrade at price 0)
//   perk    PERK:<perk>             a free perk you do not own
//   heal    FULL HEAL + ARMOR       full health, and a timed shield from the next round's start (armorSeconds)
//   fist    FULL FIST CHARGE        the charged fist (design.n24.chargeFist) at 100 %
// A card is offered only while it can be given (a box with a gun left to roll, a gun that can be packed, a perk left, the fist block on). One press
// takes a card (keys 1-3, a click, pad A); a default pick (card `defaultCard`) is taken after `defaultSeconds` by the screen. The old percentage
// draft sits behind design.mods.legacy (off): with it on, or the block off, Mods drafts as NIGHT21 did.
// Mods owns the flow (Pending / Open / Pick, the modDraft and modPick events, the tape's DraftOpen / DraftPick); this class rolls the cards and
// applies the pick. A card id carries its payload ("pack:revolver", "perk:vigor") so a snapshot of an open draft needs nothing more.
// The rolls come from the run's Mulberry32 (runSeed ^ design seedSalt). Events (new names): bigPick {round, offer}, bigPickTake {id, round}.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n24.bigPick's numbers.</summary>
    public sealed class BigPick24Rules
    {
        public int first, every, cards, defaultCard;
        public double defaultSeconds, points, armorSeconds;
        public uint seedSalt;
        public readonly List<int> multRounds = new List<int>();
        public readonly List<double> multValues = new List<double>();

        public static BigPick24Rules From(GameConfig cfg)
        {
            const string p = "design.n24.bigPick.";
            var r = new BigPick24Rules
            {
                first = cfg.Int(p + "firstRound"),
                every = cfg.Int(p + "every"),
                cards = cfg.Int(p + "cards"),
                defaultCard = cfg.Int(p + "defaultCard"),
                defaultSeconds = cfg.Num(p + "defaultSeconds"),
                points = cfg.Num(p + "points"),
                armorSeconds = cfg.Num(p + "armorSeconds"),
                seedSalt = (uint)cfg.Num(p + "seedSalt"),
            };
            foreach (double d in cfg.Nums(p + "multRounds")) r.multRounds.Add((int)d);
            foreach (double d in cfg.Nums(p + "multValues")) r.multValues.Add(d);
            return r;
        }
    }

    public sealed class BigPickEvent
    {
        public int round;
        public string id;
        public List<string> offer;
    }

    public sealed class BigPick24
    {
        public const string Box = "box", Points = "points", Pack = "pack", Perk = "perk", Heal = "heal", Fist = "fist";

        /// <summary>The kinds in the order the rolls shuffle them.</summary>
        public static readonly string[] Kinds = { Box, Points, Pack, Perk, Heal, Fist };

        public readonly BigPick24Rules rules;
        readonly EventBus events;
        readonly double dt;
        Mulberry32 rng = new Mulberry32(1);
        bool active, armorPending;

        // The sim's hooks (SurvivalSim.Fist24.cs sets them; a hook left null makes its card unavailable).
        public Func<bool> boxAvailable;
        public Func<bool> boxSpin; // a free spin that cannot come up a teddy; the sim takes the gun when the reveal ends
        public Func<string> packTarget; // the held gun that can gain a tier, or null
        public Func<string, bool> pack;
        public Func<string> perkTarget; // a perk not owned yet, or null
        public Func<string, bool> perk;
        public Func<bool> fistAvailable;
        public Action fistFull;
        public Economy economy;
        public Health health;

        public BigPick24(GameConfig cfg, EventBus events)
        {
            rules = BigPick24Rules.From(cfg);
            this.events = events;
            dt = 1 / cfg.movement.loop.simHz;
            events.On<RoundEvent>("roundBegin", _ =>
            {
                if (!active || !armorPending) return;
                armorPending = false;
                health?.Shield(MovementMath.StepsFor(rules.armorSeconds, dt));
            });
        }

        /// <summary>The run snapshot (NIGHT24 audit f): the stream and the pending armour.</summary>
        public void SaveState(StateWriter w)
        {
            w.I("rng", rng.State);
            w.B("armorPending", armorPending);
        }

        public void LoadState(StateReader r)
        {
            rng.State = r.U("rng");
            armorPending = r.B("armorPending");
        }

        /// <summary>The draft flow is the BIG PICK's this run (the block on and the legacy switch off).</summary>
        public bool Active => active;

        /// <summary>Set by the sim at a run's start, before Mods.Start: the block is on for this run and design.mods.legacy is off.</summary>
        public bool WantsOn;

        /// <summary>A run's start: the stream from the run seed (Mods.Start calls it with the block on).</summary>
        public void Reset(uint runSeed)
        {
            rng = new Mulberry32((runSeed ^ rules.seedSalt) >> 0);
            armorPending = false;
            active = true;
        }

        /// <summary>The block off (or legacy on): the old draft.</summary>
        public void Off()
        {
            active = false;
            armorPending = false;
        }

        /// <summary>NIGHT25 (builder F): the screen's head for the pick that opened for `round` (0 = a campaign layer's end). One place, so the reel and a test read the same words.</summary>
        public static string Head(int round) => round <= 0 ? "BIG PICK  ·  LAYER CLEARED" : "BIG PICK  ·  ROUND " + round.ToString(System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>Is the clear of `round` a BIG PICK (round >= first and every `every` rounds from it)?</summary>
        public bool IsBigRound(int round) => round >= rules.first && rules.every > 0 && (round - rules.first) % rules.every == 0;

        /// <summary>The points multiplier at `round` (x1 below the first threshold, else the last threshold passed).</summary>
        public double PointsMult(int round)
        {
            double k = 1;
            for (int i = 0; i < rules.multRounds.Count; i++) if (round >= rules.multRounds[i]) k = rules.multValues[i];
            return k;
        }

        /// <summary>The cards for a pick: `cards` distinct kinds that can be given now, in a seeded order (fewer when fewer can be).</summary>
        public List<string> Roll()
        {
            var ids = new List<string>(Kinds.Length);
            foreach (string k in Kinds)
            {
                string id = Available(k);
                if (id != null) ids.Add(id);
            }
            for (int i = ids.Count - 1; i > 0; i--) // Fisher-Yates from the run's stream
            {
                int j = (int)Math.Floor(rng.Next() * (i + 1));
                string t = ids[i];
                ids[i] = ids[j];
                ids[j] = t;
            }
            if (ids.Count > rules.cards) ids.RemoveRange(rules.cards, ids.Count - rules.cards);
            return ids;
        }

        /// <summary>The card id of a kind if it can be given now (with its payload), else null.</summary>
        string Available(string kind)
        {
            switch (kind)
            {
                case Box: return boxAvailable != null && boxAvailable() ? Box : null;
                case Points: return economy != null ? Points : null;
                case Pack: { string g = packTarget?.Invoke(); return g != null ? Pack + ":" + g : null; }
                case Perk: { string p = perkTarget?.Invoke(); return p != null ? Perk + ":" + p : null; }
                case Heal: return health != null ? Heal : null;
                case Fist: return fistAvailable != null && fistAvailable() ? Fist : null;
            }
            return null;
        }

        public static string KindOf(string id)
        {
            int c = id.IndexOf(':');
            return c < 0 ? id : id.Substring(0, c);
        }

        public static string PayloadOf(string id)
        {
            int c = id.IndexOf(':');
            return c < 0 ? "" : id.Substring(c + 1);
        }

        /// <summary>A card's name (the card's title line).</summary>
        public string Name(string id, GameConfig cfg)
        {
            switch (KindOf(id))
            {
                case Box: return "FREE BOX SPIN";
                case Points: return "POINTS";
                case Pack: return "WEAPON PACK";
                case Perk: return cfg.Has("perks." + PayloadOf(id) + ".name") ? cfg.Str("perks." + PayloadOf(id) + ".name") : PayloadOf(id).ToUpperInvariant();
                case Heal: return "FULL HEAL + ARMOR";
                case Fist: return "FULL FIST CHARGE";
            }
            return id.ToUpperInvariant();
        }

        /// <summary>A card's hero text (the big line): the points' sum, else a short word.</summary>
        public string Hero(string id, int round)
        {
            switch (KindOf(id))
            {
                case Box: return "SPIN";
                case Points: return "+" + (rules.points * PointsMult(round)).ToString("0", System.Globalization.CultureInfo.InvariantCulture);
                case Pack: return "+1 TIER";
                case Perk: return "FREE";
                case Heal: return "FULL";
                case Fist: return "100 %";
            }
            return "";
        }

        /// <summary>A card's one finished line under the hero.</summary>
        public string Line(string id, GameConfig cfg)
        {
            switch (KindOf(id))
            {
                case Box: return "A MYSTERY BOX GUN, FREE, NEVER A TEDDY";
                case Points: return "TO SPEND ON THE NEXT BREAK";
                case Pack:
                {
                    string g = PayloadOf(id);
                    return cfg.Has("weapons.catalog." + g + ".name") ? "YOUR " + cfg.Str("weapons.catalog." + g + ".name") + " HITS HARDER" : "YOUR GUN HITS HARDER";
                }
                case Perk: return "ONE PERK, NO PRICE";
                case Heal: return "AND A SHIELD FOR " + rules.armorSeconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " S FROM THE NEXT ROUND";
                case Fist: return "THE NEXT PUNCH FLICKS YOU FAR";
            }
            return "";
        }

        /// <summary>The glyph (a UiModGlyph kind) for a card.</summary>
        public static string Glyph(string id)
        {
            switch (KindOf(id))
            {
                case Box: return "drop";
                case Points: return "ECONOMY";
                case Pack: return "edge";
                case Perk: return "STYLE";
                case Heal: return "HEALTH";
                case Fist: return "MELEE";
            }
            return "blast";
        }

        /// <summary>Give the picked card's reward. True: given (a card whose reward fails at the last moment pays points instead, so a pick is never empty).</summary>
        public bool Take(string id, int round)
        {
            bool ok;
            switch (KindOf(id))
            {
                case Box: ok = boxSpin != null && boxSpin(); break;
                case Pack: ok = pack != null && pack(PayloadOf(id)); break;
                case Perk: ok = perk != null && perk(PayloadOf(id)); break;
                case Heal:
                    health?.Heal(1e9);
                    armorPending = true;
                    ok = health != null;
                    break;
                case Fist:
                    fistFull?.Invoke();
                    ok = fistFull != null;
                    break;
                default: ok = false; break;
            }
            if (!ok || KindOf(id) == Points) economy?.Grant(rules.points * PointsMult(round), "bigPick");
            events.Emit("bigPickTake", new BigPickEvent { round = round, id = id });
            return true;
        }

        /// <summary>The offer was opened (Mods.Open emits modDraft too).</summary>
        public void Opened(int round, List<string> offer)
        {
            events.Emit("bigPick", new BigPickEvent { round = round, offer = new List<string>(offer) });
        }
    }
}
