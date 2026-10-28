// NIGHT56 builder G (G1 rank56, sim row design.n56.rank56; NIGHT56.md 5 G1; gameplay.md P1 "RANK PAYS", ULTRAKILL's rank-scaled Hard Damage
// recovery: https://ultrakill.wiki.gg/wiki/Hard_Damage): the style rank also multiplies the two payoffs SKYGRIND already gives for aggression,
// heal36's close-kill heal (Core/Heal36.cs) and RIDE KILL's boost refund (Core/RideKill53.cs). What already ships is unchanged: the rank multiplies
// points (points.rankMult), regen and stamina; a hit costs style. rank56 adds ONLY the two multipliers, read from the SAME rank index as
// points.rankMult (StyleMeter.rank, the rank shown when the kill lands: DULL 0 .. SKYGRIND 5), clamped to the table's ends as
// Economy.RankMultiplier clamps. The web has no such numbers (it has no heal36 and no ride kill): design.n56.rank56.numbers.mult.
// Off (the row's bit false, --off56 / -skygrindOff56): the sim passes x1 (heal amount x 1 and unit x 1 are the same doubles: NIGHT55 bit for bit).
// Pure, no allocation per call.
namespace Skygrind.Core
{
    /// <summary>design.n56.rank56.numbers, read once (the sim caches it).</summary>
    public sealed class Rank56Params
    {
        public const string Row = "design.n56.rank56.numbers.";

        /// <summary>The multiplier per rank index (DULL first); heal36's heal and RIDE KILL's refund unit are multiplied by it.</summary>
        public double[] mult;

        public static Rank56Params From(GameConfig c) => new Rank56Params { mult = c.Nums(Row + "mult") };
    }

    public static class Rank56
    {
        /// <summary>The payoff multiplier at `rank` (clamped to the table: below DULL reads DULL, past the top reads the top). An empty or missing
        /// table reads 1 (the row cannot pay what it was not given).</summary>
        public static double Mult(int rank, Rank56Params p)
        {
            if (p == null || p.mult == null || p.mult.Length == 0) return 1;
            int i = rank < 0 ? 0 : rank >= p.mult.Length ? p.mult.Length - 1 : rank;
            return p.mult[i];
        }
    }
}
