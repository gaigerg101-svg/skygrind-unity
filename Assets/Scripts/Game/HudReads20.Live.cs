// NIGHT20 U3-34.10 (builder H): HudReads20's hooks filled against builder G's and builder E's real fields (port/stage3 at
// the merge: G's Core/Guns/Shockwave.cs and Zoom20.cs, E's Core/Health.cs Regen). Each read leaves its struct invalid when
// the system isn't live, so HudReads20 falls back (the bus, the blocks' numbers) only where a read has nothing to say.
// - The shockwave: Loadout.shockwave (Charges, MaxCharges, CooldownLeft: 0 with every charge back, Cooldown) while
//   Loadout.ShockwaveOn (design.shockwave and its numbers).
// - The zoom: Loadout.ZoomGun(id): the gun's magnification, 0 for a unique LT (THUNDERCLAP, COMET) or none: read as 1.
// - The regen: Regen.N20 (the run's design.regen.n20), SegmentHp, SegmentTop, RefillLeft.
// - E's special rounds (sim.specials, Core/Enemies/SpecialRounds.cs): THE ELITE's bar (EliteId's enemy, its kind, its first
//   two EliteAffixes), the NEXT line's HUNT (the swarm rounds with design.adds.hunt) and ELITE (its seeded Schedule).
// - The boss (A1 BULWARK, builder EN's Core/Enemies/Bulwark.cs; builder HV): ReadBoss from its BossBar (BossThreat20), the
//   boss found by its registry body; ReadBossPorted waits for EN's block 2 (the sim's kit and its row).
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class HudReads20
    {
        static partial void ReadShockwave(SurvivalGame g, ref Shock s)
        {
            Loadout lo = g.loadout;
            if (lo == null || !lo.ShockwaveOn) return;
            Shockwave sw = lo.shockwave;
            s.valid = true;
            s.charges = sw.Charges;
            s.max = System.Math.Max(1, sw.MaxCharges);
            s.left = sw.CooldownLeft;
            s.cooldown = sw.Cooldown;
        }

        static partial void ReadZoomOf(SurvivalGame g, string weapon, ref double m)
        {
            Loadout lo = g.loadout;
            if (lo == null) return;
            m = System.Math.Max(1, lo.ZoomGun(weapon)); // 0 (a unique LT, no zoom) reads as 1: no reticle
        }

        static partial void ReadRegen(SurvivalGame g, ref Seg s)
        {
            Regen r = g.regen;
            if (r == null || !r.N20) return;
            s.valid = r.SegmentHp > 0;
            s.segment = r.SegmentHp;
            s.top = r.SegmentTop;
            s.refillLeft = r.RefillLeft;
        }

        // --- E's special rounds (Core/Enemies/SpecialRounds.cs: sim.specials, design.adds.hunt / design.adds.elite) ----------

        static readonly System.Collections.Generic.Dictionary<string, string> upperKinds = new System.Collections.Generic.Dictionary<string, string>();

        /// <summary>A kind's name in caps, cached (the threat bar reads it every frame).</summary>
        static string KindName(string kind)
        {
            if (string.IsNullOrEmpty(kind)) return null;
            if (!upperKinds.TryGetValue(kind, out string n)) upperKinds[kind] = n = Debut20.Name(kind) ?? kind.ToUpperInvariant();
            return n;
        }

        /// <summary>THE ELITE while it lives: SpecialRounds.EliteId's enemy, its kind as the name, its first two affixes.</summary>
        static partial void ReadElite(SurvivalGame g, ref Threat t)
        {
            SpecialRounds sp = g.sim.specials;
            if (sp == null || sp.EliteId < 0 || g.enemies == null) return;
            System.Collections.Generic.List<Enemy> all = g.enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (e == null || e.id != sp.EliteId || !e.alive) continue;
                t.valid = true;
                t.enemy = e.id;
                t.name = KindName(e.kind);
                t.affixA = sp.EliteAffixes.Count > 0 ? sp.EliteAffixes[0] : null;
                t.affixB = sp.EliteAffixes.Count > 1 ? sp.EliteAffixes[1] : null;
                t.hp = e.hp;
                t.maxHp = e.maxHp;
                return;
            }
        }

        /// <summary>The NEXT line's special kinds: THE HUNT on the swarm rounds and THE ELITE on its scheduled rounds (the
        /// schedule is seeded at the run's first break), when their rows are live.</summary>
        static partial void ReadRoundKind(SurvivalGame g, int round, ref string kind)
        {
            SpecialRounds sp = g.sim.specials;
            if (sp == null) return;
            if (!ReferenceEquals(roundsCfg, g.cfg))
            {
                roundsCfg = g.cfg;
                rounds = DirectorMath.From(g.cfg);
            }
            if (sp.hunt && SpecialRoundMath.IsHunt(round, rounds)) kind = "hunt";
            else if (sp.elite && sp.Schedule.Contains(round)) kind = "elite";
        }

        // --- A1 the BULWARK (builder EN's Core/Enemies/Bulwark.cs, BossKit.cs; builder HV) -----------------------------------

        static Bulwark bossSeen;

        /// <summary>The live BULWARK: its registry body (BulwarkBody, in the enemies' list while it fights), cached while it
        /// lives (a type test a live enemy a frame while no boss fights; nothing allocated).</summary>
        public static Bulwark BulwarkNow(SurvivalGame g)
        {
            if (bossSeen != null && bossSeen.Alive) return bossSeen;
            bossSeen = null;
            System.Collections.Generic.List<Enemy> all = g != null && g.enemies != null ? g.enemies.all : null;
            if (all == null) return null;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] is BulwarkBody b && b.alive)
                {
                    bossSeen = b.system;
                    break;
                }
            }
            return bossSeen;
        }

        /// <summary>The boss bar while the BULWARK fights: its BossBar (bossKit.js bar(): the name, HP, the phase mark p2At, the
        /// plates as pips). The lead's read is the kit's (`g.sim.bossKit?.Bar()`, EN's block 2: the same Bar() of the running
        /// boss); until the kit is on the sim this finds the boss by its registry body.</summary>
        static partial void ReadBoss(SurvivalGame g, ref Threat t)
        {
            if (N30Fixes.On && g != null && g.sim != null && g.sim.BossBarHidden30) return; // NIGHT30 P0 d (builder S): a campaign boss's bar only in its boss room
            Bulwark b = BulwarkNow(g);
            if (b != null) BossThreat20.From(b.Bar(), ref t);
            // NIGHT25 W (boss-bar, GG: "there's no health bar"): THE KILN WYRM's bar, the same read as the BULWARK's (Wyrm.Bar(): name, HP, the 66 % mark, the pips)
            Wyrm w = g.sim.wyrm24;
            if (!t.valid && w != null && w.Alive) BossThreat20.From(w.Bar(), ref t);
            // NIGHT25 W (boss rooms): any other boss the kit is running (MONOLITH; the BULWARK in a room is found above by its registry body): its own Bar()
            if (!t.valid && g.sim.bossKit != null) BossThreat20.From(g.sim.bossKit.Bar(), ref t);
        }

        // ReadBossPorted (boss = g.sim.BossOn: the lead's line) waits for EN's block 2 (the sim's kit and design.adds.bulwark).
    }
}
