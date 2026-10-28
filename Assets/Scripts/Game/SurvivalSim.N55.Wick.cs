// NIGHT55 builder B (wick55, sim row; design.n55.wick55; Core/Enemies/Wick37.N55.cs): THE WICK as a read-the-tell, punish-the-window fight
// (critic-veteran #1; DECIDE55 D-7: encounter numbers only). One line in Wick37Begin (SurvivalSim.Wick37.cs), after Wick54Arm: the fight takes the
// row's bit and numbers. Two hooks in Wick37Rules (the hot wall's touch): whether this touch burns (Wick55Burns: a brush in the air only pushes
// when numbers.hot.brushBurns is false; the throw-off itself is unchanged) and the burn's cooldown / damage (Wick55HotCooldown / Wick55HotDamage).
// Off (-skygrindOff55 / --off55, or the row's enabled false): wick37.w55 false and every hook returns NIGHT54's value, bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        internal int idWick55 = -1;
        Wick55Params wick55P;

        /// <summary>wick55 on for the WICK's fight now (the reels and the harness read it).</summary>
        public bool Wick55On => wick37 != null && wick37.w55;

        void Wick55Arm()
        {
            if (wick37 == null) return;
            idWick55 = rowsRegistry.IndexOf("wick55"); // -1: this config lacks the row (rows.On(-1) is false: NIGHT54)
            bool on = rows.On(idWick55);
            wick37.w55 = on;
            if (!on) return;
            wick55P ??= Wick55Params.From(cfg);
            wick37.p55 = wick55P;
        }

        /// <summary>The hot wall's touch: does this one burn? Always with the row off (NIGHT54); with it on, a brush in the air (not a ride) burns
        /// only when numbers.hot.brushBurns is true.</summary>
        bool Wick55Burns(bool riding) => !Wick55Live || wick55P.hotBrushBurns || riding;

        /// <summary>The burn's cooldown (s): NIGHT54's `baseS` off, numbers.hot.cooldown on.</summary>
        double Wick55HotCooldown(double baseS) => Wick55Live && !double.IsNaN(wick55P.hotCooldown) ? wick55P.hotCooldown : baseS;

        /// <summary>The burn's damage: NIGHT54's `baseD` off, numbers.hot.damage on.</summary>
        double Wick55HotDamage(double baseD) => Wick55Live && !double.IsNaN(wick55P.hotDamage) ? wick55P.hotDamage : baseD;

        /// <summary>wick55 governs the stack's rules now: the row on for THE WICK's fight in this run (the stack's rules outside a fight stay NIGHT54's).</summary>
        bool Wick55Live => wick37 != null && wick37.w55 && wick55P != null && wick37Owns && wick37.Alive;
    }
}
