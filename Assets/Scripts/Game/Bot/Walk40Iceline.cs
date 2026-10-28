// NIGHT40 builder L2 (NIGHT40.md card L2; the walk bot through every room, L-045): the walk pilot's script for n40-iceline (Design/levels/n40/iceline.beats.md,
// Tools/night/gen-n40-iceline.cjs). The rooms are fights on foot (no traversal gate): the autopilot fights, Walk29Pilot walks; this file holds only what the
// level's lessons need beyond that (E3's Foes40Combat may take the parry over when it lands):
//   - THE PARRY (g4 THE REACH, any room with a DRONE): the nearest live hostile orb within ParryLookM of the eye is looked at; inside ParryPressM the gauntlet's
//     swing is pressed (the punch when another weapon is in hand): foes40's swing reflects a projectile in reach (E3, Core/Enemies/Foes40.cs), and a
//     returned orb kills its drone (design.n40.foes40.drone.reflectKillsShooter). In g4 the pilot holds the bridge while a drone lives.
//   - THE BLAST (g3 THE SWARM, g5 THE COOLDOWN, g7 THE MIX): with BlastCount or more enemies within BlastNearM, the {altFire} blast is pressed at the nearest
//     (the PULSE gauntlet's LT: it goes only with a full glove and off its 15 s cooldown; a press without them does nothing).
//   - THE CANAL (g4): a fall into the 1 m channel: out north, hopping.
// Two ways in: the dispatch lines in Walk29Pilot (SampleMove: `if (Ice40(out InputFrame f40i)) return Keep(f40i);`, the lead's 4e1bf46e; SampleCombat: the lead's
// call, `if (Ice40Combat(out CombatInput c40)) return c40;` first), or the wrapper Walk40IcelineInput (the harness `n40ice walk`). Both idempotent within a step.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk29Pilot
    {
        // the bot's own reaction distances (Unity-only bot numbers, no gameplay effect): look at an orb inside 7 m of the eye, swing inside 1.6 m (E3's harness
        // presses at 1.6 m: the reflect reach is orb.reflectRange 2 m); the blast at 3 enemies within 5 m (the blast reaches design.n27.pulse27.blastRange 6 m)
        const double ParryLookM = 7, ParryPressM = 1.6, BlastNearM = 5;
        const int BlastCount = 3;
        static readonly string[] BlastKinds = { "swarmer", "husk" };
        internal static bool Ice40NoBlast; // the harness `n40ice walk --noblast` (a control: the blast script off)

        int i40LastStep = -1, i40BlastStep = -1000;
        bool i40LastOk, i40Press, i40Blast;
        InputFrame i40Last;

        bool Level40Iceline => sim.campaignLevel != null && sim.campaignLevel.id == "n40-iceline";

        /// <summary>NIGHT40 L2: the frame for one of n40-iceline's rooms, or false (the pilot's own frame). Idempotent within a step.</summary>
        internal bool Ice40(out InputFrame f)
        {
            f = default;
            if (!Level40Iceline || sim.campaign == null || sim.campaign.phase == Campaign24.Phase.None) return false;
            if (i40LastStep == step) { f = i40Last; return i40LastOk; }
            i40Press = i40Blast = false;
            i40LastOk = Ice40Frame(out f);
            i40LastStep = step;
            i40Last = f;
            return i40LastOk;
        }

        /// <summary>NIGHT40 L2: the combat for this step when a script of this file holds it (the parry's swing, the blast), else false (the pilot's own).</summary>
        internal bool Ice40Combat(out CombatInput c)
        {
            c = default;
            if (!Ice40(out _) || !(i40Press || i40Blast)) return false;
            c.weapon = -1;
            if (i40Blast)
            {
                int g = sim.loadout.owned.IndexOf("gauntlet");
                c.weapon = g >= 0 && sim.loadout.Weapon != "gauntlet" ? g : -1;
                c.altFire = Press(true, true);
                return true;
            }
            if (sim.loadout.Weapon == "gauntlet") c.fire = Press(true, true); // the gauntlet in hand drops the punch: its swing is the melee (E3)
            else c.punch = Press(true, true);
            return true;
        }

        bool Ice40Frame(out InputFrame f)
        {
            f = default;
            RoomDef rd = Room();
            if (rd == null) return false;
            Vec3 p = sim.mover.position;
            Vec3 eye = new Vec3(p.x, p.y + sim.cfg.movement.body.eyeHeight, p.z);
            // THE PARRY: the nearest live hostile orb
            DroneOrbs.Orb near = null;
            double nd = double.PositiveInfinity;
            foreach (DroneOrbs.Orb o in sim.husks.orbs.pool)
            {
                if (!o.alive || o.reflected) continue;
                double d = Vec3.DistanceTo(o.position, eye);
                if (d < nd) { nd = d; near = o; }
            }
            if (near != null && nd <= ParryLookM)
            {
                Vec3 v = near.position - eye;
                f = new InputFrame { yaw = JsMath.Atan2(-v.x, -v.z), pitch = JsMath.Atan2(v.y, JsMath.Hypot(v.x, v.z)) };
                i40Press = nd <= ParryPressM;
                return true;
            }
            // THE CANAL (g4): a fall into the channel: north out of it (its far lip), hopping
            if (rd.id == "g4" && p.y < -0.3)
            {
                f = new InputFrame { moveZ = 1, yaw = 0 };
                f.jump = Press(true, sim.mover.grounded && step % 10 == 0);
                return true;
            }
            // g4 while a drone lives: hold the bridge, face the nearest drone (its orb comes to you; the parry above answers it)
            if (rd.id == "g4" && sim.campaign.phase == Campaign24.Phase.Fight)
            {
                Husk dr = Nearest40("drone", p, 60);
                if (dr != null)
                {
                    double bx = 0.5 * (rd.x0 + rd.x1), bz = rd.z1 - 15; // the bridge's middle (lx 0, lz 15)
                    double dx = bx - p.x, dz = bz - p.z;
                    bool there = dx * dx + dz * dz < 1.0;
                    Vec3 v = dr.position - eye;
                    f = new InputFrame { yaw = there ? JsMath.Atan2(-v.x, -v.z) : Toward(p, bx, bz), pitch = there ? JsMath.Atan2(v.y, JsMath.Hypot(v.x, v.z)) : 0, moveZ = there ? 0 : 1 };
                    return true;
                }
            }
            // THE BLAST (g3, g5, g7): a pack within reach
            if (!Ice40NoBlast && (rd.id == "g3" || rd.id == "g5" || rd.id == "g7") && step - i40BlastStep > MovementMath.StepsFor(0.5, sim.step))
            {
                int n = 0;
                Husk first = null;
                double fd = double.PositiveInfinity;
                foreach (string k in BlastKinds)
                    foreach (Husk h in sim.husks.PoolOf(k))
                    {
                        if (!h.alive) continue;
                        double d = JsMath.Hypot(h.position.x - p.x, h.position.z - p.z);
                        if (d > BlastNearM) continue;
                        n++;
                        if (d < fd) { fd = d; first = h; }
                    }
                if (n >= BlastCount && first != null)
                {
                    Vec3 v = new Vec3(first.position.x, first.position.y + 0.8, first.position.z) - eye;
                    f = new InputFrame { yaw = JsMath.Atan2(-v.x, -v.z), pitch = JsMath.Atan2(v.y, JsMath.Hypot(v.x, v.z)) };
                    i40Blast = true;
                    i40BlastStep = step;
                    return true;
                }
            }
            return false;
        }

        /// <summary>The nearest live enemy of a kind within `range` m (on the floor plan), or null.</summary>
        Husk Nearest40(string kind, Vec3 p, double range)
        {
            Husk best = null;
            double bd = range;
            foreach (Husk h in sim.husks.PoolOf(kind))
            {
                if (!h.alive) continue;
                double d = JsMath.Hypot(h.position.x - p.x, h.position.z - p.z);
                if (d < bd) { bd = d; best = h; }
            }
            return best;
        }
    }

    /// <summary>NIGHT40 L2: Walk29Pilot with n40-iceline's room scripts (Walk29Pilot.Ice40 / Ice40Combat) while the dispatch lines are not in Walk29Pilot:
    /// the pilot steps as always, and where a script answers its frame replaces the pilot's.</summary>
    public sealed class Walk40IcelineInput : IPlayerInput
    {
        public readonly Walk29Pilot pilot;
        double yaw, pitch;
        bool mine;

        public Walk40IcelineInput(Walk29Pilot pilot) { this.pilot = pilot; }

        public double Yaw => mine ? yaw : pilot.Yaw;
        public double Pitch => mine ? pitch : pilot.Pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; pilot.SetLook(y, p); }
        public bool Captured => pilot.Captured;

        public InputFrame SampleMove(bool active)
        {
            InputFrame f = pilot.SampleMove(active);
            if (active && pilot.Ice40(out InputFrame g))
            {
                mine = true;
                yaw = g.yaw;
                pitch = g.pitch;
                return g;
            }
            mine = false;
            return f;
        }

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = pilot.SampleCombat(active);
            return active && pilot.Ice40Combat(out CombatInput c40) ? c40 : c;
        }
    }
}
