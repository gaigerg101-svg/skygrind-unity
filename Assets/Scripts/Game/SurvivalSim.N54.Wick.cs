// NIGHT54 builder W (wick54, sim row; design.n54.wick54; Core/Enemies/Wick37.N54.cs): THE WICK without its FLARE, the encounter made relatively
// easy (GG 2026-10-20, NIGHT54.md). One line in Wick37Begin (SurvivalSim.Wick37.cs), before StartFight, after Wick47Arm: the fight takes the
// row's bit and numbers. Off (-skygrindOff54 / --off54, or the row's enabled false): wick37.w54 false, NIGHT53's fight bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        internal int idWick54 = -1;
        Wick54Params wick54P;

        /// <summary>wick54 on for the WICK's fight now (the reels and the harness read it).</summary>
        public bool Wick54On => wick37 != null && wick37.w54;

        void Wick54Arm()
        {
            if (wick37 == null) return;
            idWick54 = rowsRegistry.IndexOf("wick54"); // -1: this config lacks the row (rows.On(-1) is false: NIGHT53)
            bool on = rows.On(idWick54);
            wick37.w54 = on;
            if (!on) return;
            wick54P ??= Wick54Params.From(cfg);
            wick37.p54 = wick54P;
        }
    }
}
