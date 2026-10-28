// NIGHT43 builder B1 round 2 (wick43 now also covers "the boss bar state for every boss"; the lead's DESIGN-RULES B-1: every boss says what
// hurts it, when it is open and which phase it is in). With the wick43 bit on and THE WICK not the live fight, N43BossStateFill answers for
// the live boss (U1's bossbar43 draws it), and the step emits N43Events.BossOpen when that boss opens / closes and N43Events.BossLeg when its
// phase changes (F1's strongest hit read keys on BossOpen). No rule changes: every number below is read from the boss's own state.
//   THE ICEBREAKER (Brawl40, level 2): HP, phase of 2 (design.n40.brawl40 phase2At), OPEN in its CYAN window (the meter emptied, e.g. by a
//     parried BLOCK) or while its fists are STUCK after a haymaker / a rush / a leap (stuckMult), the stagger meter, a hint by its tell.
//   THE KILN WYRM (Wyrm, level 3): HP, phase of 3 (hp.phase2At / phase3At), OPEN while STAGGERED (Wyrm31: a head punch) or its CORE is
//     open, a hint (punch the head as it charges; the slug while staggered; shoot the core).
//   THE LOOM / THE CROWN (Boss37 brains): HP, leg of 3, OPEN in the CYAN window, the stagger meter; a hint only in the window.
// Off (wick43 off, --off43): nothing here runs; N43BossState stays invalid for these bosses (NIGHT42's bars41 draws them as before).
// No UnityEngine (SimHarness compiles this file); no allocation per step (cached strings, a pooled payload).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        string b43Window, b43BrawlStuck, b43BrawlBlock, b43BrawlHay, b43BrawlSweep, b43BrawlLeap, b43BrawlIdle;
        string b43WyrmStagger, b43WyrmCore, b43WyrmCharge, b43WyrmIdle;
        bool b43Read;
        readonly Boss43Event b43Beat = new Boss43Event();
        string b43Boss;
        int b43Leg;
        bool b43Open;
        /// <summary>Counts for the harness: BossOpen / BossLeg emitted for the bosses other than THE WICK.</summary>
        public int boss43Opens, boss43Legs;

        void Bosses43Read()
        {
            if (b43Read) return;
            b43Read = true;
            const string r = "design.n43.wick43.bosses.";
            b43Window = cfg.Str(r + "window");
            b43BrawlStuck = cfg.Str(r + "brawlStuck");
            b43BrawlBlock = cfg.Str(r + "brawlBlock");
            b43BrawlHay = cfg.Str(r + "brawlHaymaker");
            b43BrawlSweep = cfg.Str(r + "brawlSweep");
            b43BrawlLeap = cfg.Str(r + "brawlLeap");
            b43BrawlIdle = cfg.Str(r + "brawlIdle");
            b43WyrmStagger = cfg.Str(r + "wyrmStagger");
            b43WyrmCore = cfg.Str(r + "wyrmCore");
            b43WyrmCharge = cfg.Str(r + "wyrmCharge");
            b43WyrmIdle = cfg.Str(r + "wyrmIdle");
        }

        /// <summary>The live boss other than THE WICK (the first live of THE ICEBREAKER, THE KILN WYRM, THE LOOM, THE CROWN).</summary>
        void Bosses43Fill(ref Boss43State s)
        {
            Bosses43Read();
            if (brawl40 != null && (brawl40.Alive || brawl40.state == Brawl40State.Dying) && brawl40.state != Brawl40State.Off && brawl40.state != Brawl40State.Dead)
            {
                Brawl40 b = brawl40;
                bool dying = b.state == Brawl40State.Dying;
                s.valid = true;
                s.name = b.p.name;
                s.leg = b.phase;
                s.legs = 2;
                s.hp01 = dying ? 0 : Math.Max(0, Math.Min(1, b.HpFrac));
                s.open = !dying && (b.Open || b.StuckNow);
                s.openLeft = s.open ? b.stateLeft * cfg.Dt : 0;
                double full = b.StaggerFull;
                s.stagger01 = dying ? 0 : b.Open ? Math.Max(0, 1 - b.Progress) : full > 0 ? Math.Max(0, Math.Min(1, b.stagger / full)) : 0;
                s.hint = dying ? null : b.Open ? b43Window : b.StuckNow ? b43BrawlStuck : b.block.alive || b.Telling(Brawl40.Block) ? b43BrawlBlock
                    : b.Telling(Brawl40.Haymaker) ? b43BrawlHay : b.Telling(Brawl40.Sweep) ? b43BrawlSweep : b.Telling(Brawl40.Leap) ? b43BrawlLeap : b43BrawlIdle;
                return;
            }
            if (wyrm24 != null && wyrm24.Alive && wyrm24.phase != "dead" && wyrm24.phase != "intro")
            {
                Wyrm w = wyrm24;
                bool dying = w.phase == "dying";
                double hp = Math.Max(0, Math.Min(1, w.HpFrac));
                bool stag = wyrm31 != null && wyrm31.Staggered;
                s.valid = true;
                s.name = w.p.name;
                s.legs = 3;
                s.leg = hp > w.p.phase2At ? 1 : hp > w.p.phase3At ? 2 : 3;
                s.hp01 = dying ? 0 : hp;
                s.open = !dying && (stag || w.CoreOpen);
                s.openLeft = 0; // the worm's stagger / core clocks are its own (not read here)
                s.hint = dying ? null : stag ? b43WyrmStagger : w.CoreOpen ? b43WyrmCore : w.Charging30 ? b43WyrmCharge : b43WyrmIdle;
                return;
            }
            Boss37Brain br = null;
            Enemy body = null;
            string name = null;
            if (loom37 != null && loom37.Alive && loom37.brain != null) { br = loom37.brain; body = loom37.body; name = loom37.p.name; }
            else if (crown37 != null && crown37.Alive && crown37.brain != null) { br = crown37.brain; body = crown37.body; name = crown37.p.name; }
            if (br == null || br.state == Boss37State.Off || br.state == Boss37State.Dead) return;
            bool d37 = br.state == Boss37State.Dying;
            s.valid = true;
            s.name = name;
            s.leg = br.leg;
            s.legs = 3;
            s.hp01 = d37 || body.maxHp <= 0 ? 0 : Math.Max(0, Math.Min(1, body.hp / body.maxHp));
            s.open = !d37 && br.Open;
            s.openLeft = s.open ? br.stateLeft * cfg.Dt : 0;
            s.stagger01 = d37 ? 0 : br.Open ? Math.Max(0, 1 - br.Progress) : br.StaggerFrac;
            s.hint = s.open ? b43Window : null;
        }

        /// <summary>Every step with wick43 on: bossOpen43 / bossLeg43 for the live boss other than THE WICK (THE WICK's own come from Wick37.N43).</summary>
        void Bosses43Step()
        {
            if (wick37 != null && wick37Owns && wick37.Alive) { b43Boss = null; return; }
            Boss43State s = N43BossState;
            if (!s.valid) { b43Boss = null; return; }
            if (s.name != b43Boss) { b43Boss = s.name; b43Leg = s.leg; b43Open = s.open; return; }
            b43Beat.boss = s.name;
            b43Beat.legs = s.legs;
            b43Beat.leg = s.leg;
            if (s.leg != b43Leg)
            {
                b43Leg = s.leg;
                boss43Legs++;
                b43Beat.open = s.open;
                events.Emit(N43Events.BossLeg, b43Beat);
            }
            if (s.open != b43Open)
            {
                b43Open = s.open;
                if (s.open) boss43Opens++;
                b43Beat.open = s.open;
                events.Emit(N43Events.BossOpen, b43Beat);
            }
        }
    }
}
