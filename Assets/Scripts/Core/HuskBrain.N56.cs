// NIGHT56 builder G (G3 tokens56, sim row design.n56.tokens56; NIGHT56.md 5 G3; enemies.md M1 / M2, DOOM 2016's attack tokens: "the demons in
// front of the player are the ones attacking", https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-): enemies come at the player
// from where they can be seen. On top of tokens41's two-wind-up cap (unchanged), the brain's two rules here, behind on56 (the sim sets it, and the
// look, each step: SurvivalSim.N56.Tokens.cs; off: every line is skipped, NIGHT55's brain bit for bit):
//   ON SCREEN FIRST  an enemy that wants a token while OUTSIDE the sim cone (the sim's look, half angle camera.fovDeg / 2 round it, from the
//                    player's eye to the enemy's aim point: never the player's FOV setting, rule 4) waits numbers.waitS of thinks first: it holds
//                    no token (gives back an uncommitted one) and keeps its ring, so the enemies in view take the tokens. After the wait it may
//                    attack from where it is (looking away is never a shield); its wait restarts when it commits. A stationary kind (the lancer)
//                    is left alone.
//   ADVANCE          an enemy with no token and no attack that has stood (horizontal speed under numbers.idleSpeed) for numbers.idleS walks to
//                    a loop node IN VIEW: of the current room's nodes (the sim hands them over at a room's start), the one inside the sim cone
//                    nearest the enemy and at least numbers.minM from it; it goes there ("approach") until within numbers.arriveM or for at most
//                    numbers.advanceMaxS, then the ring again.
// Ranged accuracy falling with the player's speed (M2) ALREADY ships: AimMath.HitChance (ai.aim.baseHit 0.7 standing -> minHit 0.25 at fullSpeed
// 24 m/s), rolled in Think for every ranged token holder: tokens56 adds nothing there (G.md has the measured rates).
// Whole thinks (dtThink), no randomness, no allocation per step (the node array is the sim's, filled at a room's start).
using System;

namespace Skygrind.Core
{
    /// <summary>design.n56.tokens56.numbers (each number's _why there) and camera.fovDeg (the cone).</summary>
    public sealed class Tokens56Params
    {
        public const string Row = "design.n56.tokens56.numbers.";
        public double cosHalf, waitS, idleS, idleSpeed, minM, arriveM, advanceMaxS;
        /// <summary>NIGHT57 G (G1's target): numbers.nearestIfNoneInView (1: an idle waiter with no loop node in view walks to the nearest node;
        /// 0 or absent: it stays, NIGHT56's rule).</summary>
        public bool nearestIfNoneInView;

        public static Tokens56Params From(GameConfig c) => new Tokens56Params
        {
            cosHalf = JsMath.Cos(c.Num("camera.fovDeg") * 0.5 * Math.PI / 180),
            waitS = c.Num(Row + "waitS"), idleS = c.Num(Row + "idleS"), idleSpeed = c.Num(Row + "idleSpeed"), minM = c.Num(Row + "minM"),
            arriveM = c.Num(Row + "arriveM"), advanceMaxS = c.Num(Row + "advanceMaxS"),
            nearestIfNoneInView = c.Has(Row + "nearestIfNoneInView") && c.Num(Row + "nearestIfNoneInView") > 0,
        };
    }

    public static class Tokens56
    {
        /// <summary>Is `at` inside the cone of half angle acos(cosHalf) round `look` (a unit vector) from `eye`? (A point at the eye is inside.)</summary>
        public static bool InCone(Vec3 eye, Vec3 look, Vec3 at, double cosHalf)
        {
            double dx = at.x - eye.x, dy = at.y - eye.y, dz = at.z - eye.z;
            double len = JsMath.Hypot(dx, dy, dz);
            if (len < 1e-9) return true;
            return (dx * look.x + dy * look.y + dz * look.z) / len >= cosHalf;
        }
    }

    public sealed partial class HuskBrain
    {
        /// <summary>tokens56 on for this run (the sim sets it, the params and the look every step; false: NIGHT55's brain).</summary>
        public bool on56;
        public Tokens56Params p56;
        /// <summary>The sim's eye and look direction (unit) at the last step's end: the cone's apex and axis.</summary>
        public Vec3 eye56, look56;
        /// <summary>The current room's loop nodes (the sim's array, its first nodeCount56 entries; 0: none, no advance).</summary>
        public Vec3[] nodes56;
        public int nodeCount56;
        /// <summary>Counters (the harness): thinks an enemy out of view waited, advances started.</summary>
        public int coneWaits56, advances56;

        bool InView56(Vec3 at) => Tokens56.InCone(eye56, look56, at, p56.cosHalf);

        /// <summary>ON SCREEN FIRST (Think, an enemy that wants a token and is not in its attack): false while it is out of view and has waited
        /// under waitS.</summary>
        bool Wants56(Rec r, double dtThink)
        {
            if (r.k.stationary) return true;
            if (InView56(r.e.AimPoint))
            {
                r.offCone56 = 0;
                return true;
            }
            r.offCone56 += dtThink;
            if (r.offCone56 >= p56.waitS) return true;
            coneWaits56++;
            return false;
        }

        /// <summary>ADVANCE (Think's ring-wait goal): an advance running keeps its node as the goal; an enemy idle for idleS starts one.</summary>
        void Advance56(Rec r, Husk e, bool holding, double dtThink, ref string phase)
        {
            if (r.k.stationary || r.k.flankOnly || holding || e.committed || e.windupSteps > 0)
            {
                // a token holder keeps its ring (a ranged holder's spot is its shot); an attack in progress is not idling; a flanker (the swarmer)
                // keeps its wait spot out of your front cone (the brain's own rule for it)
                r.adv56 = -1;
                r.idle56 = 0;
                return;
            }
            if (r.adv56 >= 0 && r.adv56 < nodeCount56)
            {
                Vec3 n = nodes56[r.adv56];
                r.advT56 += dtThink;
                if (JsMath.Hypot(n.x - e.position.x, n.z - e.position.z) <= p56.arriveM || r.advT56 > p56.advanceMaxS)
                {
                    r.adv56 = -1;
                    r.idle56 = 0;
                    return;
                }
                r.gx = n.x;
                r.gz = n.z;
                phase = "approach";
                return;
            }
            r.adv56 = -1;
            if (JsMath.Hypot(e.velocity.x, e.velocity.z) < p56.idleSpeed) r.idle56 += dtThink;
            else r.idle56 = 0;
            if (r.idle56 < p56.idleS || nodeCount56 <= 0 || nodes56 == null) return;
            r.idle56 = 0;
            int best = -1, near = -1;
            double bd = double.PositiveInfinity, nd = double.PositiveInfinity;
            for (int i = 0; i < nodeCount56; i++)
            {
                Vec3 n = nodes56[i];
                double d = JsMath.Hypot(n.x - e.position.x, n.z - e.position.z);
                if (d < p56.minM) continue;
                if (d < nd) { nd = d; near = i; }
                if (d >= bd) continue;
                if (!InView56(new Vec3(n.x, n.y + 1, n.z))) continue;
                best = i;
                bd = d;
            }
            // NIGHT57 G (G1's target, numbers.nearestIfNoneInView): no node in view: the nearest one, so a waiter pressed on a wall out of view (a drone
            // flying at its 24 m ring outside a kiln room) still moves (E-5); off: NIGHT56's rule (it stays until a node comes into view)
            if (best < 0 && p56.nearestIfNoneInView) best = near;
            if (best < 0) return;
            r.adv56 = best;
            r.advT56 = 0;
            r.gx = nodes56[best].x;
            r.gz = nodes56[best].z;
            phase = "approach";
            advances56++;
        }
    }
}
