// NIGHT34 builder B (leftover 3, design.n34.combo34, N34Blocks.combo34: only with bend34 on): THE NAMED BEND COMBO. A grapple latch starts a bend; kills
// inside that one bend (In or Hold) with two or more DIFFERENT guns stamp a name. A kill's gun is ArsenalMath.WeaponOf(kill.weapon) when the loadout owns
// it (a slot weapon); the whip's own hit, slams and other sources do not count. Each time the bend's count of different killing guns reaches a new step
// (2, 3, 4 or more) the sim emits bend34Combo {name, guns, kills, points} (GG's sound hook; builder H shows the name) and the style meter pays that
// step's bonus (StyleCore.Bonus, the name as its source, so the meter's own multipliers and diminishing apply). Called from B's hooks in
// SurvivalSim.Bend34.cs: Combo34StartRun (N34BendStartRun), Combo34BendStarted / Combo34Kill (N34BendStep, after the bend's own latch and per queued kill).
// Off (the row, bend34 off, --off34): nothing is counted, emitted or paid.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>bend34Combo's payload (reused: read it inside the handler).</summary>
    public sealed class Bend34ComboEvent
    {
        public string name, gunList;
        public int guns, kills;
        public double points;
    }

    /// <summary>design.n34.combo34's numbers.</summary>
    public sealed class Combo34Rules
    {
        public string[] names;
        public double[] stylePoints;

        /// <summary>The REA addendum's sub-switches (research/rea-ultrakill.md): ARSENAL, QUICKDRAW, the whip readying the punch; false when absent.</summary>
        public bool arsenal, quickdraw, whipReady;
        public double arsenalStyle, quickdrawS, quickdrawStyle, whipReadyS;

        public static Combo34Rules From(GameConfig cfg)
        {
            const string p = "design.n34.combo34.";
            var r = new Combo34Rules { names = cfg.Strs(p + "names").ToArray(), stylePoints = cfg.Nums(p + "stylePoints") };
            r.arsenal = cfg.Design("n34.combo34.arsenal");
            r.quickdraw = cfg.Design("n34.combo34.quickdraw");
            r.whipReady = cfg.Design("n34.combo34.whipReady");
            if (r.arsenal) r.arsenalStyle = cfg.Num(p + "arsenalStyle");
            if (r.quickdraw)
            {
                r.quickdrawS = cfg.Num(p + "quickdrawS");
                r.quickdrawStyle = cfg.Num(p + "quickdrawStyle");
            }
            if (r.whipReady) r.whipReadyS = cfg.Num(p + "whipReadyS");
            return r;
        }
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT34: design.n34.combo34's numbers (null until a run starts with the row on).</summary>
        public Combo34Rules combo34Rules;

        /// <summary>NIGHT34: combos stamped this run, and the last one's name (the harness, the reel).</summary>
        public int combo34Count;
        public string combo34Last;

        public bool Combo34On => n34.combo34 && n34.bend34 && combo34Rules != null;

        readonly List<string> c34Guns = new List<string>(8);
        int c34Kills, c34Step;
        readonly Bend34ComboEvent c34Event = new Bend34ComboEvent();

        void Combo34StartRun()
        {
            c34Guns.Clear();
            c34Kills = c34Step = 0;
            combo34Count = 0;
            combo34Last = null;
            c34Hitters.Clear();
            c34RaiseStep = -1;
            c34QuickArmed = false;
            arsenal34Count = quickdraw34Count = whipReady34Count = 0;
            if (!c34Wired)
            {
                events.On<HitEvent>("hit", C34OnHit);
                events.On("weaponRaise", C34OnRaise);
                events.On<Whip33Event>("whip33Latch", C34OnWhip);
                c34Wired = true;
            }
            if (!n34.combo34 || !n34.bend34) return;
            combo34Rules ??= Combo34Rules.From(cfg);
            c34QuickSteps = combo34Rules.quickdraw ? MovementMath.StepsFor(combo34Rules.quickdrawS, step) : 0;
            c34WhipSteps = combo34Rules.whipReady ? MovementMath.StepsFor(combo34Rules.whipReadyS, step) : 0;
        }

        // ---- the REA addendum (research/rea-ultrakill.md "Weapon switching", "The Whiplash"): ARSENAL, QUICKDRAW, the whip readying the punch.
        // Any time (not only inside a bend), each behind its sub-switch under the combo34 bit. Events arsenal34 / quickdraw34 (GG's sound hooks) carry
        // a Bend34ComboEvent payload (name ARSENAL / QUICKDRAW, guns, points). ----

        /// <summary>NIGHT34: ARSENAL kills, QUICKDRAW hits and whip-readied punches this run (the harness).</summary>
        public int arsenal34Count, quickdraw34Count, whipReady34Count;

        bool c34Wired, c34QuickArmed;
        int c34RaiseStep = -1, c34QuickSteps, c34WhipSteps;
        readonly Dictionary<int, int> c34Hitters = new Dictionary<int, int>(64); // enemy id -> the owned guns that hit it (bit = owned index)
        readonly Bend34ComboEvent c34Side = new Bend34ComboEvent();

        static int C34Bits(int m)
        {
            int n = 0;
            for (; m != 0; m &= m - 1) n++;
            return n;
        }

        void C34OnHit(HitEvent e)
        {
            if (!Combo34On || e.owner != "player") return;
            Combo34Rules r = combo34Rules;
            string gun = ArsenalMath.WeaponOf(e.weapon);
            int idx = gun != null && gun != Whip33Weapon ? loadout.owned.IndexOf(gun) : -1;
            if (r.arsenal)
            {
                int mask = c34Hitters.TryGetValue(e.enemy, out int m) ? m : 0;
                if (idx >= 0 && idx < 31) mask |= 1 << idx;
                if (e.killed)
                {
                    c34Hitters.Remove(e.enemy);
                    int guns = C34Bits(mask);
                    if (guns >= 2) C34Pay("ARSENAL", r.arsenalStyle, gun, guns, "arsenal34", ref arsenal34Count);
                }
                else if (mask != 0) c34Hitters[e.enemy] = mask;
            }
            if (r.quickdraw && c34QuickArmed && idx >= 0 && gun == loadout.Weapon && simStep - c34RaiseStep <= c34QuickSteps)
            {
                c34QuickArmed = false; // once per swap
                C34Pay("QUICKDRAW", r.quickdrawStyle, gun, 1, "quickdraw34", ref quickdraw34Count);
            }
        }

        void C34OnRaise(object _)
        {
            if (!Combo34On || !combo34Rules.quickdraw) return;
            c34RaiseStep = simStep; // the gun in hand changed on this step
            c34QuickArmed = true;
        }

        void C34OnWhip(Whip33Event _)
        {
            if (!Combo34On || !combo34Rules.whipReady) return;
            loadout.ReadyPunch34(c34WhipSteps);
            whipReady34Count++;
        }

        void C34Pay(string name, double points, string gun, int guns, string ev, ref int counter)
        {
            double paid = style.core.Bonus(name, points, gun, PortData.Name(mover.state));
            counter++;
            c34Side.name = name;
            c34Side.guns = guns;
            c34Side.kills = name == "ARSENAL" ? 1 : 0;
            c34Side.points = paid;
            c34Side.gunList = gun;
            events.Emit(ev, c34Side);
        }

        /// <summary>A new bend began (not an upgrade of a running one): the combo starts empty.</summary>
        void Combo34BendStarted()
        {
            c34Guns.Clear();
            c34Kills = c34Step = 0;
        }

        /// <summary>A player kill inside the bend with `weapon` (the kill event's).</summary>
        void Combo34Kill(string weapon)
        {
            if (!Combo34On) return;
            string gun = ArsenalMath.WeaponOf(weapon);
            if (gun == null || gun == Whip33Weapon || !loadout.owned.Contains(gun)) return;
            c34Kills++;
            if (!c34Guns.Contains(gun)) c34Guns.Add(gun);
            int n = c34Guns.Count;
            if (n < 2 || n - 1 <= c34Step) return;
            c34Step = n - 1;
            Combo34Rules r = combo34Rules;
            int i = System.Math.Min(n - 2, System.Math.Min(r.names.Length, r.stylePoints.Length) - 1);
            if (i < 0) return;
            double paid = style.core.Bonus(r.names[i], r.stylePoints[i], gun, PortData.Name(mover.state));
            combo34Count++;
            combo34Last = r.names[i];
            c34Event.name = r.names[i];
            c34Event.guns = n;
            c34Event.kills = c34Kills;
            c34Event.points = paid;
            c34Event.gunList = string.Join(",", c34Guns);
            events.Emit("bend34Combo", c34Event);
        }
    }
}
