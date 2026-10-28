// NIGHT47 builder B, row brawl47 (sim; G28-G30; Research/2026-10-18-wave47/B-cards.md section 3, THE ICEBREAKER card). Called from
// SurvivalSim.N47.Brawl.cs's hooks. While the row is on, the fight's numbers on the one Brawl40Params object (brawl40.p) take
// design.n47.brawl47.numbers (put back on a later run with the row off):
//   three phases: phase 2 at phase2At (NIGHT40 0.5 -> 0.66), phase 3 at phase3At (Brawl40.phase3At47, a second transition beat);
//   the tells E-4 asks for: the PROW RUSH rushWindupS (0.65 -> 0.95, the CHARGER's wind-up language), the BLOCK blockWindupS (0.75 -> 0.9);
//   the length levers for the human band (pools only go down: rests and the stagger meter, never HP): rest1S / rest2S, meter1 / meter2;
//   phase 3 (Brawl40.Phase3_47): THE STAMPEDE: p3Chargers chargers come in once from the arena's two gates (gateOffset m either side of its
//   centre), and a charge that runs into THE ICEBREAKER drains its meter by chargerDrain (as a gauntlet hit; one drain per charge; event
//   brawl47ChargerHit), so a charger baited into the boss opens it.
// The fight object is built once (SurvivalSim.Brawl40.cs) and lives across runs: the first step of a run with the row on writes the numbers
// (the values before are kept once), a run with the row off writes them back. No allocation per step (the meter array is made once a run).
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        const string Brawl47Root = "design.n47.brawl47.numbers.";
        bool b47On, b47Applied, b47Saved, b47P3Seen;
        Brawl40 b47For;
        double b47Phase2Was, b47RushWas, b47BlockWas, b47Rest1Was, b47Rest2Was, b47Rest42Was;
        double[] b47MeterWas;
        double b47Drain, b47HitR;
        readonly ChargerEvent b47Said = new ChargerEvent();
        readonly Husk[] b47Herd = new Husk[4]; // phase 3's chargers (p3Chargers <= 4)
        int b47HerdN;

        /// <summary>brawl47 is on this run (views and harnesses read it).</summary>
        public bool Brawl47On => b47On;
        /// <summary>The harness's counts this sim's life: chargers sent in at phase 3, charges that drained THE ICEBREAKER.</summary>
        public int brawl47Sent, brawl47ChargerHits;

        /// <summary>THE ICEBREAKER's line set while brawl47 is on (SurvivalSim.Brawl40.cs's Brawl40LineBegin calls it after reading NIGHT40's):
        /// sentence case with the glyphs (F's COPY47 rules); the armour's and the herd's lines are said by Brawl47Step.</summary>
        void Brawl47Lines()
        {
            if (!rows.On(idBrawl47)) return;
            brawl40LineEntry = cfg.Str(Brawl47Root + "lineEntry");
            brawl40LineFar = cfg.Str(Brawl47Root + "lineFar");
            brawl40LineStuck = cfg.Str(Brawl47Root + "lineStuck");
            brawl40LineBlock = cfg.Str(Brawl47Root + "lineBlock");
        }

        bool b47WasArmored;

        void Brawl47StartRun()
        {
            b47On = rows.On(idBrawl47);
            b47Applied = false;
            b47P3Seen = false;
            b47HerdN = 0;
            for (int i = 0; i < b47Herd.Length; i++) b47Herd[i] = null;
            if (!b47On) Brawl47Restore();
        }

        void Brawl47Restore()
        {
            if (!b47Saved || b47For == null) return;
            Brawl40Params p = b47For.p;
            p.phase2At = b47Phase2Was;
            p.phase3At47 = 0;
            p.rushWindup = b47RushWas;
            p.blockWindup = b47BlockWas;
            p.restSeconds = b47Rest1Was;
            p.restSeconds2 = b47Rest2Was;
            if (b47For.p42 != null) b47For.p42.rest2 = b47Rest42Was;
            p.stagger = b47MeterWas;
            p.chip47 = 0;
            p.armorChip47 = 0;
            p.armorMaxS47 = 0;
            p.armorP1_47 = false;
            p.armorP3Herd47 = false;
            p.crackWindowMult47 = 0;
            b47Saved = false;
        }

        void Brawl47Step()
        {
            if (!b47On || brawl40 == null) return;
            if (!b47Applied)
            {
                if (b47For != brawl40) { b47Saved = false; b47For = brawl40; }
                Brawl40Params p = brawl40.p;
                if (!b47Saved)
                {
                    b47Phase2Was = p.phase2At;
                    b47RushWas = p.rushWindup;
                    b47BlockWas = p.blockWindup;
                    b47Rest1Was = p.restSeconds;
                    b47Rest2Was = p.restSeconds2;
                    b47Rest42Was = brawl40.p42 != null ? brawl40.p42.rest2 : 0;
                    b47MeterWas = p.stagger;
                    b47Saved = true;
                }
                p.phase2At = cfg.Num(Brawl47Root + "phase2At");
                p.phase3At47 = cfg.Num(Brawl47Root + "phase3At");
                p.rushWindup = cfg.Num(Brawl47Root + "rushWindupS");
                p.blockWindup = cfg.Num(Brawl47Root + "blockWindupS");
                p.restSeconds = cfg.Num(Brawl47Root + "rest1S");
                p.restSeconds2 = cfg.Num(Brawl47Root + "rest2S");
                if (brawl40.p42 != null) brawl40.p42.rest2 = cfg.Num(Brawl47Root + "rest2S");
                p.stagger = new[] { cfg.Num(Brawl47Root + "meter1"), cfg.Num(Brawl47Root + "meter2") }; // once a run
                p.chip47 = cfg.Num(Brawl47Root + "chip");
                b47Drain = cfg.Num(Brawl47Root + "chargerDrain");
                b47HitR = p.radius + cfg.Num(Brawl47Root + "chargerHitPad");
                p.armorChip47 = cfg.Num(Brawl47Root + "armorChip");
                p.armorMaxS47 = cfg.Num(Brawl47Root + "armorMaxS");
                p.armorP1_47 = cfg.Num(Brawl47Root + "armorP1") > 0;
                p.armorP3Herd47 = cfg.Num(Brawl47Root + "armorP3Herd") > 0;
                p.crackWindowMult47 = cfg.Num(Brawl47Root + "crackWindowMult");
                // a fight that began before these numbers (a harness's Begin before its first step): phase 1's armour now
                if (p.armorP1_47 && brawl40.body.alive && brawl40.phase == 1 && brawl40.HpFrac >= 0.999) brawl40.Arm47();
                b47Applied = true;
            }
            if (!brawl40.body.alive) return;
            // the lines: the armour's when it comes on (a phase's start), the herd's when phase 3's gates open
            if (brawl40.Armored47 && !b47WasArmored && brawl40Line != null && !brawl40.Phase3_47) brawl40Line.Say("armour", cfg.Str(Brawl47Root + "lineArmour"), brawl40LineSeconds, null, 2);
            b47WasArmored = brawl40.Armored47;
            if (brawl40.Phase3_47 && !b47P3Seen && brawl40Line != null) brawl40Line.Say("herd", cfg.Str(Brawl47Root + "lineHerd"), brawl40LineSeconds, null, 2);
            if (brawl40.Phase3_47 && !b47P3Seen)
            {
                b47P3Seen = true;
                Vec3 c = brawl40Centre ?? brawl40.body.position;
                double off = cfg.Num(Brawl47Root + "gateOffset");
                int n = (int)cfg.Num(Brawl47Root + "p3Chargers");
                b47HerdN = 0;
                for (int i = 0; i < n; i++)
                {
                    double side = i % 2 == 0 ? -1 : 1;
                    Husk h = husks.Spawn("charger", new Vec3(c.x + side * off, c.y + 0.05, c.z), mover.position);
                    if (h != null) { brawl47Sent++; if (b47HerdN < b47Herd.Length) b47Herd[b47HerdN++] = h; }
                }
                events.Emit("brawl47Gates", b47Said);
            }
            // phase 3's armour (THE HERD's): broken when every charger the gates sent is dead (a charger run into it breaks it below)
            if (brawl40.Armored47 && brawl40.Phase3_47 && b47HerdN > 0)
            {
                bool any = false;
                for (int i = 0; i < b47HerdN; i++) if (b47Herd[i] != null && b47Herd[i].alive) any = true;
                if (!any) { brawl40.BreakArmor47(); b47HerdN = 0; }
            }
            if (b47Drain <= 0) return;
            var all = enemies.all;
            Vec3 bp = brawl40.body.position;
            for (int i = 0; i < all.Count; i++)
            {
                if (!(all[i] is Charger ch) || !ch.alive || ch.state != "charge" || ch.bossHitCharge47 == ch.chargeNo47) continue;
                double dx = ch.position.x - bp.x, dz = ch.position.z - bp.z, r = b47HitR + ch.Radius;
                if (dx * dx + dz * dz > r * r) continue;
                ch.bossHitCharge47 = ch.chargeNo47;
                brawl40.BreakArmor47(); // a charger baited into it cracks the ice (phase 3's herd armour, or any phase's)
                brawl40.Drain(b47Drain);
                brawl47ChargerHits++;
                b47Said.id = ch.id;
                b47Said.yaw = ch.yaw;
                events.Emit("brawl47ChargerHit", b47Said);
            }
        }
    }
}
