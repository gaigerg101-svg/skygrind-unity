// NIGHT27 (builder B): the small seams the NIGHT27 bosses share with the rest of the game.
//  - IBossBody27: a boss body whose damage rule lives in its system (EnemyHits.Filter asks it first; THE LANTERN and THE GLACIER implement it).
//  - IPulseKillSink27: where THE LANTERN's SENTRIES report a kill so it fills the PULSE's kill pips. Builder W owns Core/PulseCharge27.cs: its class
//    implements this ONE method and the lead wires it (lantern27.pulseSink = <W's object>); with none wired the kills still count on the boss (stats).
// Pure C#, no UnityEngine.
namespace Skygrind.Core
{
    /// <summary>A boss body that filters the damage it takes (its shield / window rule).</summary>
    public interface IBossBody27
    {
        /// <summary>The damage that lands from a hit of `amount` by `weapon` from `from`; blocked: nothing lands (no hit, no points).</summary>
        double Filter(double amount, string weapon, Vec3 from, out bool blocked);
    }

    /// <summary>The PULSE's kill pips, fed by a kill that is not the PULSE's own (builder W's PulseCharge27 implements it).</summary>
    public interface IPulseKillSink27
    {
        /// <summary>`kills` enemies died that count toward the PULSE's five pips (the lantern's sentries).</summary>
        void PulseKillFilled(int kills);
    }
}
