// NIGHT43 builder B1 (wick43; NIGHT43.md card B1 G16, GG 2026-10-16: "meaningful phases, evolving attacks, and readable transitions ... I like
// that the current attacks force the player to parry, so preserve and build upon that"; research/bosses-web.md B.7). THE WICK's legs as beats,
// behind the wick43 bit (the sim sets `n43` and `p43` before StartFight; off: every line here is skipped and the fight is NIGHT42's bit for bit).
//   THE LEG BEAT  when the leg changes (the player stood on the next ledge / the cap: ReachedFloor -> the brain's NextLeg) THE WICK recoils
//                 (kicked up recoilUp m and back recoilBack m from the player, its follow brings it back), the slag drops slagDrop m, and
//                 N43Events.BossLeg fires (Boss43Event: the leg of 3). The brain's LEGCHANGE (legChangeSeconds, no attacks, no damage) is the
//                 roar / pose second (bosses-web B.7).
//   THE GIFT      when that LEGCHANGE ends, the plate facing the player cracks (crackSeconds, as a parried ember's crack): a free ride-shot to
//                 restart momentum at every new leg (bosses-web B.7 "a gift window").
//   LEG 2         EMBER fires a SECOND volley volleyGap s after the first (the same yellow globs, aimed again): more parries, the same language.
//   LEG 3         the FLARE comes back on the cap (design.n37's flare is legs 1-2): its burning half narrowed to capFlareHalfDeg (the catwalk is
//                 4 m wide: run round or dash across the hole), the same red tell.
//   bossOpen43    N43Events.BossOpen (Boss43Event.open) when Open43 changes: the CYAN window, or a cracked plate (full damage through it).
// Pure (no UnityEngine), whole steps, no randomness, no allocation per step (the payload is pooled). Numbers: design.n43.wick43.legs.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n43.wick43.legs (each number's _why there).</summary>
    public sealed class Wick43Params
    {
        public double volleyGap, capFlareHalfDeg, capFlareWeight, slagDrop, recoilSeconds, recoilUp, recoilBack;
        public int leg2Volleys, capFlareEvery;
        public bool flareLeg3, giftCrack;

        public static Wick43Params From(GameConfig c)
        {
            const string g = "design.n43.wick43.legs.";
            return new Wick43Params
            {
                volleyGap = c.Num(g + "volleyGap"), leg2Volleys = c.Int(g + "leg2Volleys"), capFlareEvery = c.Int(g + "capFlareEvery"), flareLeg3 = c.Bool(g + "flareLeg3"),
                capFlareHalfDeg = c.Num(g + "capFlareHalfDeg"), capFlareWeight = c.Num(g + "capFlareWeight"), slagDrop = c.Num(g + "slagDrop"), recoilSeconds = c.Num(g + "recoilSeconds"),
                recoilUp = c.Num(g + "recoilUp"), recoilBack = c.Num(g + "recoilBack"), giftCrack = c.Bool(g + "giftCrack"),
            };
        }
    }

    public sealed partial class Wick37
    {
        /// <summary>wick43 on for this fight (the sim sets it, and p43, before StartFight).</summary>
        public bool n43;
        public Wick43Params p43;
        /// <summary>Steps left of the leg beat's recoil (the view reads it: the cage shudders); the leg the last beat was for.</summary>
        public int recoil43, beatLeg43;
        /// <summary>Counts for the harness and the reels.</summary>
        public int volleys43, gifts43, legBeats43, opens43, capFlares43;
        int volley43Left, dives43Since;

        void SetFlareLeg3(bool on)
        {
            if (w54) return; // NIGHT54 W (wick54): the FLARE is out of the kit; the cap flare cannot re-arm it
            foreach (Boss37Attack a in brain.attacks)
                if (a.id == Flare) { if (on) a.legMask |= 1 << 2; else a.legMask &= ~(1 << 2); }
        }
        int lastLeg43 = 1;
        Boss37State last43 = Boss37State.Off;
        bool open43;
        readonly Boss43Event beat43 = new Boss43Event();

        /// <summary>Full damage now: the CYAN window, or a cracked plate (a charged ride shot through it lands in full).</summary>
        public bool Open43 => brain != null && body.alive && brain.Fighting && (brain.Open || CrackedCount > 0 || Core55); // NIGHT55 B (wick55): + the bare / cracked-open core (Wick37.N55.cs; false off)

        /// <summary>The longest crack left, in steps (0: none).</summary>
        public int CrackLeftMax
        {
            get { int m = 0; for (int i = 0; i < Plates; i++) if (crackLeft[i] > m) m = crackLeft[i]; return Math.Max(m, CoreLeft55); } // NIGHT55 B (wick55): the open core's time too (0 off)
        }

        /// <summary>An ember is flying at the player now (not one already returned).</summary>
        public bool EmbersIncoming
        {
            get { for (int i = 0; i < embers.Count; i++) if (embers[i].alive && !embers[i].back) return true; return false; }
        }

        double FlareHalfDeg43 => n43 && p43 != null && brain != null && brain.leg == 3 ? p43.capFlareHalfDeg : p.flareHalfDeg;

        void StartFight43()
        {
            volley43Left = dives43Since = 0;
            recoil43 = 0;
            beatLeg43 = 0;
            volleys43 = gifts43 = legBeats43 = opens43 = capFlares43 = 0;
            lastLeg43 = brain != null ? brain.leg : 1;
            last43 = brain != null ? brain.state : Boss37State.Off;
            open43 = false;
            if (!n43 || p43 == null || brain == null) return;
            // leg 3: the flare comes back on the cap (its id and tell unchanged)
            if (p43.flareLeg3) foreach (Boss37Attack a in brain.attacks) if (a.id == Flare) a.legMask |= 1 << 2;
        }

        void Volley43Arm()
        {
            if (!n43 || p43 == null || brain == null || brain.leg != 2 || p43.leg2Volleys < 2) return;
            volley43Left = Math.Max(1, S(p43.volleyGap));
        }

        void Step43(Vec3 pw)
        {
            if (p43 == null || brain == null || !body.alive) return;
            if (recoil43 > 0) recoil43--;
            // leg 2's second volley
            if (volley43Left > 0 && --volley43Left == 0 && brain.Fighting && !brain.Open) { FireEmbers(pw); volleys43++; }
            // the leg beat
            if (brain.leg != lastLeg43)
            {
                lastLeg43 = brain.leg;
                legBeats43++;
                beatLeg43 = brain.leg;
                recoil43 = Math.Max(1, S(p43.recoilSeconds));
                volley43Left = dives43Since = 0;
                Vec3 away = Centre - pw;
                double h = JsMath.Hypot(away.x, away.z);
                if (h > 1e-6) local = new Vec3(local.x + away.x / h * p43.recoilBack, local.y + p43.recoilUp, local.z + away.z / h * p43.recoilBack);
                else local = new Vec3(local.x, local.y + p43.recoilUp, local.z);
                if (slag != null) slag.y = Math.Max(slag.p.startY, slag.y - p43.slagDrop);
                // leg 3: the cap flare weighs capFlareWeight in the pick (the DIVE's stall is leg 3's only opening: the flare must not crowd it out)
                if (brain.leg == 3 && p43.flareLeg3) foreach (Boss37Attack a in brain.attacks) if (a.id == Flare) a.weight = p43.capFlareWeight;
                beat43.boss = p.name;
                beat43.leg = brain.leg;
                beat43.legs = 3;
                beat43.open = false;
                events.Emit(N43Events.BossLeg, beat43);
            }
            // the gift: the leg change's end cracks the plate facing the player
            if (p43.giftCrack && last43 == Boss37State.LegChange && brain.state != Boss37State.LegChange && brain.Fighting)
            {
                Vec3 c = Centre;
                int plate = PlateAt(JsMath.Atan2(pw.z - c.z, pw.x - c.x));
                crackLeft[plate] = S(p.crackSeconds);
                gifts43++;
                fx.stage = "gift";
                fx.plate = plate;
                fx.at = c;
                events.Emit("wick37Crack", fx);
            }
            if (brain.state == Boss37State.Tell && brain.entered && brain.leg == 3)
            {
                // round 2 (the band SHORT: the cap flare took the DIVE's turns, p90 63.6 s): after a cap flare it leaves the pick until
                // capFlareEvery dives have come (the stall after a dive is leg 3's only opening)
                if (AttackId == Flare) { capFlares43++; dives43Since = 0; SetFlareLeg3(false); }
                else if ((AttackId == Dive || (w47 && AttackId == Charge47)) && ++dives43Since >= p43.capFlareEvery && p43.flareLeg3) SetFlareLeg3(true); // NIGHT47 W (wick47): the CHARGE counts as the DIVE
            }
            last43 = brain.state;
            bool o = Open43;
            if (o != open43)
            {
                open43 = o;
                if (o) opens43++;
                beat43.boss = p.name;
                beat43.leg = brain.leg;
                beat43.legs = 3;
                beat43.open = o;
                events.Emit(N43Events.BossOpen, beat43);
            }
        }
    }
}
