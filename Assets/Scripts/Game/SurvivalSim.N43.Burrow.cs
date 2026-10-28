// NIGHT43 builder A1 (burrow43, a sim row: design.n43.burrow43, N43Blocks bit 10; NIGHT43.md card A1; G28): the run's BURROWER rules handed to
// the enemy pools at the run's start (Core/Enemies/Burrower43.cs; Husks.burrow43). With the row on, a level's wave kind "burrower" spawns a
// BURROWER (its own pool, stepped by Husks after the wardens); off (the row, --off43 and every older off switch, a tape without the bit) the
// pools get null and "burrower" stays a plain husk, as every unknown kind was: NIGHT42's game. The numbers are read once (design.n43.burrow43).
// The burrowers step with the other enemies (Husks.Step), their events are N43Events.BurrowTell / BurrowEmerge / BurrowCounter.
// N43BurrowStep (round 2, the lead's ask): E1's rooms43 ends a room's body that stays under its floor or out of its room for endS, and gives a
// still one the on-screen unstick tell. A BURROWER is under its floor by design (buried 6 m down: nothing reaches it) and never stuck (its head
// runs on floor rays), so right after E1's step (N43Step: Rooms, then Burrow) its counters are cleared: it is never ended, never told or moved.
// Only bodies that ARE Burrower43 are touched (none exist with burrow43 off: nothing changes).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Burrow43Params burrow43Rules;
        HuskParams burrow43Body;

        partial void N43BurrowStartRun()
        {
            if (n43.burrow43)
            {
                burrow43Rules ??= Burrow43Params.From(cfg);
                burrow43Body ??= burrow43Rules.Body(cfg, husks.p.killPlaneY);
                husks.burrow43 = burrow43Rules;
                husks.burrowBody43 = burrow43Body;
            }
            else
            {
                husks.burrow43 = null;
                husks.burrowBody43 = null;
            }
        }

        partial void N43BurrowStep(double dt)
        {
            if (husks.burrow43 == null) return;
            for (int i = 0; i < rm43Bodies.Count; i++)
            {
                Body43 b = rm43Bodies[i];
                if (!(b.h is Burrower43)) continue;
                b.outSteps = 0; // under its floor by design
                b.stillSteps = 0; // never stuck: no unstick tell, no relocation
                b.tellAt = 0;
                rm43Bodies[i] = b;
            }
        }
    }
}
