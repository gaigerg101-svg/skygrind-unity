// NIGHT56 builder G (G1 rank56, sim row design.n56.rank56; Core/Rank56.cs): the style rank also multiplies heal36's close-kill heal and RIDE KILL's
// boost refund. Two call-site lines in shared files, one per payoff:
//   SurvivalSim.Core36.cs (heal36's kill handler):   heal36Core.OnKill(..., Rank56HealScale())   (Heal36.OnKill's `scale`, default 1)
//   SurvivalSim.N53.RideKill.cs (RideKill53OnKill):  RideKill53.Refund(before, unit * Rank56RefundScale(), ...)
// The rank is StyleMeter.rank (the rank SHOWN when the kill lands: the same index Economy reads for points.rankMult); the kill's own award shows
// on a later step. Off (the bit false): both scales are exactly 1 (x 1 keeps every double: NIGHT55 bit for bit) and nothing is counted.
// Counters (the harness's n56rank and the a256 reel): per rank index, the heals / HP scaled and the refunds / units scaled. Reset at run start.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Rank56Params rank56P;
        bool rank56On;

        /// <summary>Per rank index (DULL first): close-kill heals asked at that rank, ride-kill refunds asked at that rank (row on only).</summary>
        public readonly int[] Rank56Heals = new int[6], Rank56Refunds = new int[6];

        /// <summary>rank56 on for this run (the reels and the harness read it).</summary>
        public bool Rank56On => rank56On;

        partial void N56RankStartRun()
        {
            rank56On = rows.On(idRank56);
            for (int i = 0; i < Rank56Heals.Length; i++) Rank56Heals[i] = Rank56Refunds[i] = 0;
            if (rank56On) rank56P ??= Rank56Params.From(cfg);
        }

        int Rank56Index => style == null ? 0 : (style.rank < 0 ? 0 : style.rank > 5 ? 5 : style.rank);

        /// <summary>heal36's heal multiplier for a kill now: 1 off; the rank's numbers.mult on.</summary>
        double Rank56HealScale()
        {
            if (!rank56On || rank56P == null) return 1;
            Rank56Heals[Rank56Index]++;
            return Rank56.Mult(style.rank, rank56P);
        }

        /// <summary>RIDE KILL's refund unit multiplier for a ride kill now: 1 off; the rank's numbers.mult on.</summary>
        double Rank56RefundScale()
        {
            if (!rank56On || rank56P == null) return 1;
            Rank56Refunds[Rank56Index]++;
            return Rank56.Mult(style.rank, rank56P);
        }
    }
}
