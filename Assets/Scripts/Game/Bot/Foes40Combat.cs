// NIGHT40 builder E3 (NIGHT40.md card E3, L-013): the walk bot's answer to the campaign roster's two melee threats, so a level's walk (n40-iceline's g2 THE SHIELD,
// g7 THE MIX, g8 THE REHEARSAL) clears them the way the counter does (n40foes ttk).
//   - the CHARGER: during its tell and its charge, off its locked line sideways (the line = its position along chargeYaw; the side the player is already on), facing
//     it; once it is stunned (a wall, or foes40's skid), to the spot F40BackM behind it and the shotgun (else the weapon in hand) into its back (x3: foes40's armour
//     leaves only the stunned back open).
//   - the DRONE: its orb parried (look at it inside 7 m, press inside 1.6 m: the gauntlet's swing or the punch), as L2's Walk40Iceline does on n40-iceline.
//   - the WARDEN: the nearest within F40WardenM with the gauntlet owned: walk at it, the gauntlet in hand, swing inside F40SwingM (foes40: only a melee knock opens
//     the shield; the gauntlet lands whole through it).
// Two dispatch lines (the lead's, in Walk29Pilot): SampleMove, after the Ice40 line (the parry first):
//     if (Foes40(out InputFrame f40e)) return Keep(f40e); // NIGHT40 E3: the CHARGER's dodge and back, the WARDEN's gauntlet (Foes40Combat.cs)
// SampleCombat, after the Ice40Combat line:
//     if (active && Foes40Combat(out CombatInput c40e)) return c40e; // NIGHT40 E3 (Foes40Combat.cs)
// Both are idempotent within a step and do nothing with no live charger / warden near (any level, any row state: the bot's own).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk29Pilot
    {
        // the bot's own distances (Unity-only bot numbers, no gameplay effect): a charger's line is dodged when the player is within F40LineM of it and ahead of
        // the charger; the stunned back is taken from F40BackM behind it within F40ChargerM; a warden is engaged within F40WardenM, swung at inside F40SwingM
        // (the gauntlet's reach is weapons.gauntlet.range 2.4 m)
        const double F40LineM = 3.5, F40BackM = 2.5, F40ChargerM = 16, F40WardenM = 14, F40SwingM = 2.2;
        // an orb is looked at inside 7 m of the eye and parried inside 1.6 m (orb.reflectRange is 2 m: L2's numbers, the same rule)
        const double F40OrbLookM = 7, F40OrbPressM = 1.6;

        // a melee target given up after this long with no hit (s), for this long (s): bot numbers
        const double F40NoHitS = 6, F40BackoffS = 4;
        int f40TargetId = -1, f40NoHitSteps, f40BackoffUntil;
        double f40TargetHp;
        int f40Step = -1;
        bool f40Ok, f40Fire, f40Parry;
        string f40Weapon;
        InputFrame f40Frame;

        /// <summary>NIGHT40 E3: this step's frame when a roster answer holds it, else false (the pilot's own). Idempotent within a step.</summary>
        internal bool Foes40(out InputFrame f)
        {
            if (f40Step == step) { f = f40Frame; return f40Ok; }
            f40Step = step;
            f40Fire = f40Parry = false;
            f40Weapon = null;
            f40Ok = F40Frame(out f);
            f40Frame = f;
            return f40Ok;
        }

        /// <summary>NIGHT40 E3: the combat for this step when Foes40 holds it (the swing, the back shot), else false.</summary>
        internal bool Foes40Combat(out CombatInput c)
        {
            c = default;
            c.weapon = -1;
            if (!Foes40(out _)) return false;
            if (f40Parry)
            {
                if (sim.loadout.Weapon == "gauntlet") c.fire = Press(true, true); // the gauntlet in hand drops the punch: its swing reflects (foes40)
                else c.punch = Press(true, true);
                return true;
            }
            if (f40Weapon != null && sim.loadout.Weapon != f40Weapon)
            {
                int k = sim.loadout.owned.IndexOf(f40Weapon);
                if (k >= 0) { c.weapon = k; return true; }
            }
            bool tap = f40Fire && step % 2 == 0; // a tap every other step (the KESTREL fires on the release, the THUNDERCLAP on the press)
            c.fire = Press(tap, tap);
            return true;
        }

        bool F40Frame(out InputFrame f)
        {
            f = default;
            if (sim.campaign == null || sim.campaign.phase != Campaign24.Phase.Fight) return false;
            Vec3 p = sim.mover.position;
            Vec3 eye = new Vec3(p.x, p.y + sim.cfg.movement.body.eyeHeight, p.z);
            // the DRONE's orb: the parry (L2's Walk40Iceline does the same on n40-iceline and runs first there): look at the nearest hostile orb inside
            // F40OrbLookM, press inside F40OrbPressM (the gauntlet's swing with it in hand, foes40's reflect; else the punch)
            DroneOrbs.Orb near = null;
            double nd = F40OrbLookM;
            foreach (DroneOrbs.Orb o in sim.husks.orbs.pool)
            {
                if (!o.alive || o.reflected) continue;
                double d = Vec3.DistanceTo(o.position, eye);
                if (d < nd) { nd = d; near = o; }
            }
            if (near != null)
            {
                f = F40Move(eye, near.position, 0, 0);
                f40Parry = nd <= F40OrbPressM;
                return true;
            }
            // the CHARGER
            Charger ch = null;
            double cd = F40ChargerM;
            foreach (Husk h in sim.husks.PoolOf("charger"))
            {
                if (!h.alive || !(h is Charger c0)) continue;
                double d = JsMath.Hypot(h.position.x - p.x, h.position.z - p.z);
                if (d < cd) { cd = d; ch = c0; }
            }
            if (ch != null)
            {
                Vec3 at = ch.AimPoint;
                if (ch.state == "tell" || ch.state == "charge")
                {
                    double fx = -JsMath.Sin(ch.chargeYaw), fz = -JsMath.Cos(ch.chargeYaw);
                    double rx = p.x - ch.position.x, rz = p.z - ch.position.z;
                    double ahead = rx * fx + rz * fz, side = rx * fz - rz * fx;
                    if (ahead > -1 && Math.Abs(side) < F40LineM)
                    {
                        // off the line: the side the player is on (a world direction), the look on the charger
                        double sx = fz * (side >= 0 ? 1 : -1), sz = -fx * (side >= 0 ? 1 : -1);
                        f = F40Move(eye, at, sx, sz);
                        f.jump = Press(true, sim.mover.grounded && ch.state == "charge"); // a jump over it as it comes (the web: 'a jump over it ... avoids it')
                        return true;
                    }
                }
                else if (ch.state == "stunned")
                {
                    double fx = -JsMath.Sin(ch.yaw), fz = -JsMath.Cos(ch.yaw);
                    double bx = ch.position.x - fx * F40BackM, bz = ch.position.z - fz * F40BackM;
                    double gx = bx - p.x, gz = bz - p.z, gl = JsMath.Hypot(gx, gz);
                    f = gl > 0.4 ? F40Move(eye, at, gx / gl, gz / gl) : F40Move(eye, at, 0, 0);
                    bool behind = (p.x - ch.position.x) * fx + (p.z - ch.position.z) * fz < 0;
                    f40Weapon = sim.loadout.owned.Contains("shotgun") ? "shotgun" : null;
                    f40Fire = behind;
                    return true;
                }
            }
            // the WARDEN (the gauntlet owned)
            if (!sim.loadout.owned.Contains("gauntlet")) return false;
            Husk w = null;
            double wd = F40WardenM;
            foreach (Husk h in sim.husks.PoolOf("warden"))
            {
                if (!h.alive) continue;
                double d = JsMath.Hypot(h.position.x - p.x, h.position.z - p.z);
                if (d < wd) { wd = d; w = h; }
            }
            if (w == null) return F40GauntletChase(p, eye, out f);
            if (!F40Engage(w)) return false;
            double wx = w.position.x - p.x, wz = w.position.z - p.z, wl = wd > 1e-6 ? wd : 1;
            f = wd > F40SwingM * 0.8 ? F40Move(eye, w.AimPoint, wx / wl, wz / wl) : F40Move(eye, w.AimPoint, 0, 0);
            f40Weapon = "gauntlet";
            f40Fire = wd <= F40SwingM;
            return true;
        }

        /// <summary>A melee target is worth walking at while the swings land: F40NoHitS with no hit point lost (a wall or a rail between: `n40foes level
        /// --trace g2 --first 2`, the bot 1.9 m from a warden 35 s, no hit) hands the room back to the autopilot for F40BackoffS.</summary>
        bool F40Engage(Husk target)
        {
            if (step < f40BackoffUntil) return false;
            if (target.id != f40TargetId)
            {
                f40TargetId = target.id;
                f40TargetHp = target.hp;
                f40NoHitSteps = 0;
            }
            if (target.hp < f40TargetHp)
            {
                f40TargetHp = target.hp;
                f40NoHitSteps = 0;
            }
            else if (++f40NoHitSteps >= MovementMath.StepsFor(F40NoHitS, sim.step))
            {
                f40NoHitSteps = 0;
                f40BackoffUntil = step + MovementMath.StepsFor(F40BackoffS, sim.step);
                return false;
            }
            return true;
        }

        /// <summary>With the gauntlet the only weapon owned (level 2's first half): the nearest walking enemy within F40WardenM on about the player's floor
        /// (|dy| under 1.5 m) is walked at and swung at inside F40SwingM (the autopilot alone circles a husk at range with a melee weapon: `n40foes level --trace
        /// g2`, a lone husk alive 25 s after both wardens died).</summary>
        bool F40GauntletChase(Vec3 p, Vec3 eye, out InputFrame f)
        {
            f = default;
            if (sim.loadout.owned.Count != 1 || sim.loadout.owned[0] != "gauntlet") return false;
            Husk best = null;
            double bd = F40WardenM;
            foreach (Enemy e in sim.enemies.all)
            {
                if (!e.alive || !(e is Husk h) || h is Drone || Math.Abs(h.position.y - p.y) > 1.5) continue;
                double d = JsMath.Hypot(h.position.x - p.x, h.position.z - p.z);
                if (d < bd) { bd = d; best = h; }
            }
            if (best == null || !F40Engage(best)) return false;
            double l = bd > 1e-6 ? bd : 1;
            f = bd > F40SwingM * 0.8 ? F40Move(eye, best.AimPoint, (best.position.x - p.x) / l, (best.position.z - p.z) / l) : F40Move(eye, best.AimPoint, 0, 0);
            f40Weapon = "gauntlet";
            f40Fire = bd <= F40SwingM;
            return true;
        }

        /// <summary>A frame looking at `look` and moving along the world direction (dx, dz) (0, 0: still).</summary>
        static InputFrame F40Move(Vec3 eye, Vec3 look, double dx, double dz)
        {
            Vec3 v = look - eye;
            double yaw = JsMath.Atan2(-v.x, -v.z), pitch = JsMath.Atan2(v.y, JsMath.Hypot(v.x, v.z));
            var f = new InputFrame { yaw = yaw, pitch = pitch };
            if (dx != 0 || dz != 0)
            {
                Vec3 fwd = MovementMath.WishDirection(0, 1, yaw), right = MovementMath.WishDirection(1, 0, yaw);
                f.moveZ = dx * fwd.x + dz * fwd.z;
                f.moveX = dx * right.x + dz * right.z;
            }
            return f;
        }
    }

    /// <summary>NIGHT40 E3: the harness's wrapper (n40foes walk): Walk29Pilot with Foes40's two lines first, for a branch whose Walk29Pilot has no dispatch line.</summary>
    public sealed class Foes40WalkInput : IPlayerInput
    {
        public readonly Walk29Pilot pilot;
        public Foes40WalkInput(Walk29Pilot pilot) => this.pilot = pilot;
        public double Yaw => pilot.Yaw;
        public double Pitch => pilot.Pitch;
        public bool Captured => pilot.Captured;
        public void SetLook(double y, double p) => pilot.SetLook(y, p);

        public InputFrame SampleMove(bool active)
        {
            InputFrame f = pilot.SampleMove(active); // the pilot's own step (its counters, the autopilot's)
            if (active && pilot.Foes40(out InputFrame f40))
            {
                pilot.SetLook(f40.yaw, f40.pitch);
                return f40;
            }
            return f;
        }

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = pilot.SampleCombat(active);
            if (active && pilot.Foes40Combat(out CombatInput c40)) return c40;
            return c;
        }
    }
}
