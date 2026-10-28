// NIGHT42 builder B1 (brawl42): the pilots' answers to THE ICEBREAKER's new attacks (Core/Enemies/Brawl40.N42.cs). Bot-side only; nothing here
// runs unless the fight has brawl42 on (g.on42), so the NIGHT40 / NIGHT41 pilots are unchanged with the row off.
//   SWEEP  it jumps as the arm comes: in the tell's last 0.12 s (the casual pilot: 0.04 s, and only on the 70 % of tells it reacts to), when
//          it is inside the arm's reach (+1 m). The arm reaches it ~0.2 s after the tell ends; a jump holds the feet over 0.5 m for ~0.5 s.
//   LEAP   once the circle has locked (+0.1 s to see it; the casual pilot +0.3 s), it runs out of the circle (+1.5 m) and dashes once if a pip
//          is free, and keeps out until the landing; then the punish (its STUCK fists) is the NIGHT40 pilot's own close-in.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Brawl40Pilot
    {
        bool sweepJumped, leapDashed;
        int sweepAirJumps;
        /// <summary>NIGHT42 answers taken (the harness's line).</summary>
        public int sweepJumps, leapEscapes;

        void Answer42(SurvivalSim sim, Brawl40 g, Vec3 me, double surf, bool tell, string atk, Vec3 tang, double dt, ref Vec3 wish, ref bool dash, ref bool jump)
        {
            if (atk == Brawl40.Sweep && tell && !sweepJumped && reactThis && sim.mover.grounded && surf <= g.p42.sweepReach + 1.0
                && g.stateLeft * dt <= (Casual ? 0.04 : 0.12))
            {
                jump = true;
                sweepJumped = true;
                jumps++;
                sweepJumps++;
            }
            // phase 2's second spin: as the arm comes round again (and the feet are coming down), jump again (the double jump in the air)
            if (g.Acting(Brawl40.Sweep) && reactThis && surf <= g.p42.sweepReach + 1.0 && sweepAirJumps < g.sweepSpins - 1)
            {
                double bear = JsMath.Atan2(me.x - g.Feet.x, me.z - g.Feet.z), rel = bear - g.sweepArm;
                while (rel < 0) rel += 2 * System.Math.PI;
                while (rel >= 2 * System.Math.PI) rel -= 2 * System.Math.PI;
                double eta = rel / g.SweepRate, sinceStart = g.stateSteps * dt;
                if (sinceStart > g.p42.sweepActive * 0.6 && eta <= (Casual ? 0.06 : 0.12) && me.y - g.origin.y < 0.9)
                {
                    jump = true;
                    sweepAirJumps++;
                    jumps++;
                    sweepJumps++;
                }
            }
            if (atk != Brawl40.Leap || !reactThis) return;
            bool locked = tell ? g.leapLocked && g.stateSteps * dt >= g.p42.leapWindup * g.p42.leapTrack + (Casual ? 0.3 : 0.1) : g.state == Brawl40State.Active;
            if (!locked) return;
            Vec3 away = new Vec3(me.x - g.leapTo.x, 0, me.z - g.leapTo.z);
            double ad = JsMath.Hypot(away.x, away.z);
            if (ad >= g.p42.leapRadius + 1.5) return;
            wish = ad > 1e-6 ? away / ad * 3 : tang * 3;
            if (!leapDashed)
            {
                leapDashed = true;
                leapEscapes++;
                dodges++;
                if (sim.mover.pips >= 1 && dashCool == 0) { dash = true; dashCool = 40; dashes++; }
            }
        }
    }
}
