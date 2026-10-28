// NIGHT20 U3-34.10 (builder H): the ONE place tonight's HUD reads builder G's and builder E's sim (the shared-hook pattern of
// HudReads19). Each read is a partial hook filled in HudReads20.Live.cs against the real fields once G's / E's commits are
// in the branch (G: Loadout.shockwave with Charges / MaxCharges / CooldownLeft / Cooldown / Ready, Loadout.ZoomMag and
// Loadout.ZoomGun(id); E: Regen.N20, SegmentHp, SegmentTop, RefillLeft). Until a hook is filled, or when it reports
// nothing, the read falls back on what the bus and the design blocks already say:
// - the shockwave: the `shockwave` event's payload (ShockwaveEvent: chargesLeft, cooldownLeft) aged on the sim's clock, and
//   `shockwaveReady` (the charges now); design.shockwave's cooldown and charges (forgeCharges once a ready says so);
// - the zoom: design.zoom's magnifications (the web's ads.weapons.<gun>.zoom: KESTREL 1.5, LONGWAVE 3.5; the STITCHER's
//   design.zoom.nailgunMag 1.25; the THUNDERCLAP and the COMET keep unique LTs: none);
// - the regen: design.regen.n20.segment, the segment's top from the HP (HpSegments20.Top).
// Every read is gated by its NIGHT20 block (game.sim.n20): with -skygrindOff20 the HUD is NIGHT19's. Read every frame: no
// allocation (structs by ref; the config's numbers read once per run; the zoom table filled once per gun).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class HudReads20
    {
        /// <summary>The shockwave's charges: `charges` of `max` ready, `left` s to the next, each charge `cooldown` s.</summary>
        public struct Shock
        {
            public bool valid;
            public int charges, max;
            public double left, cooldown;
        }

        /// <summary>The NIGHT20 regen: segments of `segment` HP, the in-round regen's reach `top`, the refill's `refillLeft` s.</summary>
        public struct Seg
        {
            public bool valid;
            public double segment, top, refillLeft;
        }

        // --- the fallback's state (per game; a new run resets it) -----------------------------------------------------------

        static SurvivalGame bound;
        static double cfgCooldown, cfgSegment, cfgNailMag;
        static int cfgCharges, cfgForgeCharges;
        static int evCharges = -1, evMax;
        static double evLeft, evAt;
        static readonly Dictionary<string, double> zoomTable = new Dictionary<string, double>();

        /// <summary>Ui.BuildHud20: read the blocks' numbers once and listen on the bus (the fallback).</summary>
        public static void Bind(SurvivalGame g)
        {
            if (g == null || ReferenceEquals(g, bound)) return;
            bound = g;
            GameConfig cfg = g.cfg;
            cfgCooldown = cfg.Has("design.shockwave.cooldown") ? cfg.Num("design.shockwave.cooldown") : 0;
            cfgCharges = cfg.Has("design.shockwave.charges") ? cfg.Int("design.shockwave.charges") : 1;
            cfgForgeCharges = cfg.Has("design.shockwave.forgeCharges") ? cfg.Int("design.shockwave.forgeCharges") : cfgCharges;
            cfgSegment = cfg.Has("design.regen.n20.segment") ? cfg.Num("design.regen.n20.segment") : 0;
            cfgNailMag = cfg.Has("design.zoom.nailgunMag") ? cfg.Num("design.zoom.nailgunMag") : 1;
            zoomTable.Clear();
            g.events.On<ShockwaveEvent>(N20Events.Shockwave, e =>
            {
                if (e == null || e.owner != "player") return;
                evCharges = e.chargesLeft;
                evLeft = e.cooldownLeft;
                evAt = bound.sim.SimTime;
            });
            g.events.On(N20Events.ShockwaveReady, o =>
            {
                if (!(o is int n)) return;
                if (n > evMax) evMax = n;
                evCharges = n;
                evLeft = n >= evMax ? 0 : cfgCooldown;
                evAt = bound.sim.SimTime;
            });
            g.OnNewRun += ResetRun;
            ResetRun();
        }

        static void ResetRun()
        {
            evCharges = -1;
            evMax = cfgCharges;
            evLeft = evAt = 0;
        }

        /// <summary>The shockwave's charges (valid only while design.shockwave is live and the player owns the THUNDERCLAP).</summary>
        public static void Shockwave(SurvivalGame g, ref Shock s)
        {
            s.valid = false;
            s.charges = s.max = 0;
            s.left = s.cooldown = 0;
            if (g == null || g.sim == null || !g.sim.n20.shockwave || g.sim.n25.shockwaveOff || g.loadout == null || !g.loadout.owned.Contains(UnityTuning.Hud20ShockGun)) return;
            ReadShockwave(g, ref s);
            if (s.valid) return;
            // the fallback: the bus (a run starts with every charge ready)
            s.valid = cfgCooldown > 0;
            s.max = System.Math.Max(1, evMax);
            s.cooldown = cfgCooldown;
            if (evCharges < 0)
            {
                s.charges = s.max;
                return;
            }
            s.left = System.Math.Max(0, evLeft - (g.sim.SimTime - evAt));
            s.charges = evCharges;
            if (s.left <= 0 && s.charges < s.max) s.charges++; // the ready event is the truth; this only keeps the ring honest between them
        }

        /// <summary>The magnification `weapon` zooms to with design.zoom live (1: none, its LT is unique or it has no zoom).</summary>
        public static double ZoomOf(SurvivalGame g, string weapon)
        {
            if (g == null || g.sim == null || !g.sim.n20.zoom || string.IsNullOrEmpty(weapon)) return 1;
            double m = 0;
            ReadZoomOf(g, weapon, ref m);
            if (m > 0) return m;
            if (zoomTable.TryGetValue(weapon, out m)) return m;
            GameConfig cfg = g.cfg;
            string p = "ads.weapons." + weapon + ".zoom"; // once per gun
            m = weapon == "nailgun" ? cfgNailMag : cfg.Has(p) ? cfg.Num(p) : 1;
            zoomTable[weapon] = m;
            return m;
        }

        /// <summary>The NIGHT20 regen's segments (valid only while design.regen.n20 is live).</summary>
        public static void Regen(SurvivalGame g, ref Seg s)
        {
            s.valid = false;
            s.segment = s.top = s.refillLeft = 0;
            if (g == null || g.sim == null || !g.sim.n20.regen || g.health == null) return;
            ReadRegen(g, ref s);
            if (s.valid) return;
            Health h = g.health;
            s.valid = cfgSegment > 0;
            s.segment = cfgSegment;
            s.top = HpSegments20.Top(h.hp, h.Cap, cfgSegment);
        }

        // --- A1 / A4: the threat bar (the boss, the named elite) ------------------------------------------------------------

        /// <summary>A threat the bottom bar names: the boss (A1) or the round's named elite (A4): its enemy id, its name, up to two
        /// affixes, its HP and max, the boss's phase mark (a share of max HP, 0: none).</summary>
        public struct Threat
        {
            public bool valid, boss;
            public int enemy;
            public string name, affixA, affixB;
            public double hp, maxHp, phaseAt;
            public int pips, pipsOn; // NIGHT20 A1 (builder HV): the boss's plates (BossBar.pips / pipsOn); 0: none
        }

        /// <summary>The live threat: the boss when one fights, else the elite; invalid when neither (or E's reads aren't in).</summary>
        public static void ThreatNow(SurvivalGame g, ref Threat t)
        {
            t.valid = t.boss = false;
            t.enemy = -1;
            t.name = t.affixA = t.affixB = null;
            t.hp = t.maxHp = t.phaseAt = 0;
            t.pips = t.pipsOn = 0;
            if (g == null || g.sim == null) return;
            ReadBoss(g, ref t);
            if (t.valid)
            {
                t.boss = true;
                return;
            }
            ReadElite(g, ref t);
            t.boss = false;
            if (t.maxHp <= 0) t.valid = false;
        }

        // E (HudReads20.Live.cs): t.valid while the boss (A1 BULWARK) fights: its id, name, hp, maxHp and phase mark.
        static partial void ReadBoss(SurvivalGame g, ref Threat t);

        // E (HudReads20.Live.cs): t.valid while the round's named elite (A4) lives: its id, name, two affixes, hp and maxHp.
        static partial void ReadElite(SurvivalGame g, ref Threat t);

        // --- A8: a round's kind for the NEXT line ----------------------------------------------------------------------------

        static DirectorMath.D rounds;
        static GameConfig roundsCfg;

        /// <summary>Round `round`'s kind ("normal", "hold", "gold", "swarm", "boss", E's "hunt" / "elite" / twists): E's read
        /// (director.n20's special rounds) when filled, else the director's own schedule (NextLine20.Kind: no boss until one
        /// is ported, the objectives only while live). Read when the break's round changes, not per frame.</summary>
        public static string RoundKind(SurvivalGame g, int round)
        {
            if (g == null || g.sim == null || round <= 0) return "normal";
            string kind = null;
            ReadRoundKind(g, round, ref kind);
            if (kind != null) return kind;
            if (!ReferenceEquals(roundsCfg, g.cfg))
            {
                roundsCfg = g.cfg;
                rounds = DirectorMath.From(g.cfg);
            }
            bool boss = false;
            ReadBossPorted(g, ref boss);
            return NextLine20.Kind(round, rounds, g.sim.objectives != null && g.sim.objectives.Active, boss);
        }

        // E (HudReads20.Live.cs): kind = round `round`'s kind with director.n20's special rounds (THE HUNT, THE ELITE, twists).
        static partial void ReadRoundKind(SurvivalGame g, int round, ref string kind);

        // E (HudReads20.Live.cs): boss = true once a boss is ported and live (A1 BULWARK), so 'boss' rounds read as such.
        static partial void ReadBossPorted(SurvivalGame g, ref bool boss);

        // G (HudReads20.Live.cs): s.valid = true and the charges, max, left and cooldown from Loadout.shockwave.
        static partial void ReadShockwave(SurvivalGame g, ref Shock s);

        // G (HudReads20.Live.cs): m = the magnification `weapon` zooms to (Loadout.ZoomGun), 1 for a unique LT.
        static partial void ReadZoomOf(SurvivalGame g, string weapon, ref double m);

        // E (HudReads20.Live.cs): s.valid = true and the segment, its top and the refill's seconds from Regen (N20 true).
        static partial void ReadRegen(SurvivalGame g, ref Seg s);
    }
}
