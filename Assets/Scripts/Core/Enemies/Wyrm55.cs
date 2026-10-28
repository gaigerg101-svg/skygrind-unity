// NIGHT56 builder G (G4, THE KILN WYRM's long tail; sim row design.n55.wyrm55, NIGHT55's row: "the fight inside GG's 'one to two minutes, maybe
// three' by encounter numbers (wick54's method), every attack's own numbers pinned by a test"). The ENCOUNTER only: every attack keeps its tell,
// speed, damage, push, radius and count (Wyrm55Tests pins the row to the encounter keys below). What the
// row may change (design.n55.wyrm55.numbers, each with its _why; a key left out keeps what wyrm46 / wyrm47 set):
//   poolScale        x the fight's pool, once a fight (after NIGHT39's boss39 and wyrm46's poolScale): the phases are fractions of it
//   gunScale         the chip a gun lands outside a stagger (Wyrm31Params.gunScale; wyrm46 0.03 is "barely")
//   staggerSeconds   the OPEN window after a head punch (Wyrm31Params.staggerSeconds; wyrm47 10)
//   open             what every weapon lands while staggered (Wyrm31.open32; wyrm47 3)
//   contactCooldown  s between two contact hurts of the body / head (WyrmParams.contactCooldown 0.8): how often touching it can hurt, not
//                    how much (contact.body / head / lunge stay)
//   phase2At / phase3At  the bar shares where P2 (the barrage) and P3 (the squeeze) start (WyrmParams 0.66 / 0.33): the phase gates, as wick54
//                    moved THE WICK's
//   spitEvery2 / spitEvery3  s between two spit barrages in P2 / P3 (WyrmParams 3.2 / 2.2): the barrage's PACING, as wick54's rest per leg; the
//                    barrage itself (its count, speed, damage, life, radius, aim) stays
// The sim (SurvivalSim.N56.Wyrm.cs) sets them every step while the row is on (after wyrm46 / wyrm47, which also set theirs every step) and puts a
// value back at the next run's start only if it still holds the row's (the compare-before-restore of wyrm47). Pure numbers here.
namespace Skygrind.Core
{
    /// <summary>design.n55.wyrm55.numbers (each number's _why there). NaN: the key is absent (the lever is not changed).</summary>
    public sealed class Wyrm55Params
    {
        public const string Row = "design.n55.wyrm55.numbers.";

        /// <summary>The keys the row may hold: encounter numbers only (N56WyrmTests: no other key).</summary>
        public static readonly string[] Keys = { "poolScale", "gunScale", "staggerSeconds", "open", "contactCooldown", "phase2At", "phase3At", "spitEvery2", "spitEvery3" };

        public double poolScale = double.NaN, gunScale = double.NaN, staggerSeconds = double.NaN, open = double.NaN, contactCooldown = double.NaN, phase2At = double.NaN, phase3At = double.NaN,
            spitEvery2 = double.NaN, spitEvery3 = double.NaN;

        public static Wyrm55Params From(GameConfig c)
        {
            double N(string k) => c.Has(Row + k) ? c.Num(Row + k) : double.NaN;
            return new Wyrm55Params
            {
                poolScale = N("poolScale"), gunScale = N("gunScale"), staggerSeconds = N("staggerSeconds"), open = N("open"),
                contactCooldown = N("contactCooldown"), phase2At = N("phase2At"), phase3At = N("phase3At"), spitEvery2 = N("spitEvery2"), spitEvery3 = N("spitEvery3"),
            };
        }
    }
}
