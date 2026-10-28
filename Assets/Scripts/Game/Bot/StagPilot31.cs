// NIGHT31 builder B (stag31): the scripted pilots of the NIGHT31 boss checks, shared by the harness (camp31) and the Player reels (FeelReel.N31.Bosses.cs).
//  - StagPilot31: THE STAG's moving pilot, a human-lag model on the real Mover: holds 8-12 m and plinks the body with the KESTREL, sees the charge tell after reactSeconds and
//    dodges sideways off the line (sprint + a dash; after the melt toward the rink's middle), runs behind an open back and punches it with the PULSE gauntlet, keeps off the melted banks.
//  - CampPilot31: a CAMPER (DESIGN31 E): holds its spot (walks back when pushed off), aims at the boss's aim point and fires its level's best weapon (the gauntlet inside its reach,
//    the THUNDERCLAP inside 12 m from level 3, the NAILGUN from level 4, else the KESTREL); a held trigger (the charge fills and auto-releases with design.n31.charge on).
// Numbers are bot-side: they shape one scripted opponent and change no rule.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class StagPilot31
    {
        public InputFrame Move;
        public CombatInput Combat;
        public int dodges, punches, follows, tellSeen = -1, side = 1;
        const double reactSeconds = 0.3, followAfterSeconds = 0.25;

        public void Step(SurvivalSim sim, int step)
        {
            Stag31 st = sim.stag31;
            double dt = sim.cfg.Dt;
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 c = st.box.c, d = c - eye;
            double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            Vec3 fwd = MovementMath.WishDirection(0, 1, wy), right = MovementMath.WishDirection(1, 0, wy);
            Vec3 want = default;
            bool sprint = false, dash = false, punch = false, inPunch = false;
            string gun = "revolver";
            double flat = JsMath.Hypot(c.x - me.x, c.z - me.z);
            if (st.Telling || st.Charging)
            {
                if (tellSeen < 0)
                {
                    tellSeen = step;
                    double cr = (me.x - c.x) * -st.chargeDir.z + (me.z - c.z) * st.chargeDir.x;
                    side = cr >= 0 ? 1 : -1;
                    if (st.melted)
                    {
                        Rink28Arena ra = sim.stag31Rink;
                        double inX = ra.p.cx - me.x, inZ = ra.p.cz - me.z;
                        side = -st.chargeDir.z * inX + st.chargeDir.x * inZ >= 0 ? 1 : -1; // after the melt: dodge toward the middle, off the water
                    }
                }
                if ((step - tellSeen) * dt >= reactSeconds)
                {
                    want = new Vec3(-st.chargeDir.z * side, 0, st.chargeDir.x * side);
                    sprint = true;
                    if ((step - tellSeen) * dt < reactSeconds + dt * 1.5) { dash = true; dodges++; }
                    // once the ram has gone past, follow it to where its line ends (a wall slam leaves its back toward the room for wallStunSeconds)
                    if (st.Charging && st.actSteps * dt > followAfterSeconds)
                    {
                        Vec3 stop = st.LineEnd - st.chargeDir * (st.p.halfL + 2.0);
                        double fx = stop.x - me.x, fz = stop.z - me.z, fl = JsMath.Hypot(fx, fz);
                        double across = (me.x - st.pos.x) * -st.chargeDir.z + (me.z - st.pos.z) * st.chargeDir.x;
                        if (fl > 1e-6 && Math.Abs(across) > st.p.chargeHalfWidth + 1.0) { want = new Vec3(fx / fl, 0, fz / fl); follows++; }
                    }
                }
            }
            else
            {
                tellSeen = -1;
                if (st.BackOpen)
                {
                    Vec3 behind = new Vec3(c.x - st.facing.x * (st.p.halfL + 0.9), me.y, c.z - st.facing.z * (st.p.halfL + 0.9));
                    double bx = behind.x - me.x, bz = behind.z - me.z, bl = JsMath.Hypot(bx, bz);
                    if (bl > 0.6) { want = new Vec3(bx / bl, 0, bz / bl); sprint = bl > 4; dash = bl > 5 && step % 60 == 0; }
                    gun = "gauntlet";
                    inPunch = st.Behind(me) && st.box.Touches(eye, sim.cfg.Num("weapons.gauntlet.range"));
                    punch = inPunch && step % 12 == 0;
                }
                else
                {
                    double toward = flat > 12 ? 1 : flat < 8 ? -1 : 0;
                    Vec3 dir = Vec3.Normalize(new Vec3(c.x - me.x, 0, c.z - me.z));
                    want = dir * toward + new Vec3(-dir.z, 0, dir.x) * (0.6 * ((step / 360) % 2 == 0 ? 1 : -1)); // the strafe swaps side every 3 s
                }
            }
            // after the melt a human keeps off the water: near a bank, steer toward the rink's middle line
            if (st.melted)
            {
                Rink28Arena a = sim.stag31Rink;
                a.math.Project(me.x, me.z, out double ps, out double plat);
                if (plat > a.latLaneOut - 2.5)
                {
                    a.math.PointAt(ps, plat, out double x0, out double z0, out _, out _);
                    a.math.PointAt(ps, plat - 1, out double x1, out double z1, out _, out _);
                    double ix = x1 - x0, iz = z1 - z0, il = JsMath.Hypot(ix, iz);
                    if (il > 1e-6) want = want * 0.3 + new Vec3(ix / il * 2.5, 0, iz / il * 2.5);
                }
            }
            double wl = JsMath.Hypot(want.x, want.z);
            Move = new InputFrame { yaw = wy, pitch = wp };
            if (wl > 1e-6)
            {
                Move.moveZ = (want.x * fwd.x + want.z * fwd.z) / Math.Max(1, wl);
                Move.moveX = (want.x * right.x + want.z * right.z) / Math.Max(1, wl);
            }
            Move.sprint = new Button { held = sprint, pressed = sprint && step % 60 == 0 };
            Move.dash = new Button { held = dash, pressed = dash };
            int gi = sim.loadout.owned.IndexOf(gun);
            // the press and the hold are FRAME-ROBUST (the Player reel merges two sim steps a frame and keeps the LAST step's held: a held on even steps only was never held when the
            // drive started on an odd step, so the n31-r4 reel never fired): the punch is held for 4 steps of every 12 while in position (a tap, never the 0.4 s heavy-swing hold), pressed on the first; the KESTREL taps in
            // pairs of steps (held 2, released 2), pressed on the pair's first step
            bool shoot = gun != "gauntlet" && !st.Telling && !st.Charging;
            bool held = gun == "gauntlet" ? inPunch && step % 12 < 4 : shoot && (step / 2) % 2 == 0, pressed = gun == "gauntlet" ? punch : shoot && step % 4 == 0;
            if (punch) punches++;
            Combat = new CombatInput { weapon = sim.loadout.Weapon != gun ? gi : -1, fire = new Button { held = held, pressed = pressed } };
        }
    }

    public sealed class CampPilot31
    {
        public InputFrame Move;
        public CombatInput Combat;
        public string gun = "revolver";
        public bool see;
        public double dist;

        /// <summary>One step: hold `spot`, aim at `aim`, fire when `ray` (the boss's ray: distance or -1) lands before the level collider.</summary>
        public void Step(SurvivalSim sim, Vec3 spot, Vec3 aim, Func<Vec3, Vec3, double> ray, int level, int step, bool fireOn)
        {
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = aim - eye;
            double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            Move = new InputFrame { yaw = wy, pitch = wp };
            double cx = spot.x - me.x, cz = spot.z - me.z, cl = JsMath.Hypot(cx, cz);
            if (cl > 1.0)
            {
                Vec3 fwd = MovementMath.WishDirection(0, 1, wy), right = MovementMath.WishDirection(1, 0, wy);
                Move.moveZ = (cx * fwd.x + cz * fwd.z) / cl;
                Move.moveX = (cx * right.x + cz * right.z) / cl;
            }
            dist = d.Length;
            Vec3 dn = Vec3.Normalize(d);
            double tHit = ray(eye, dn);
            see = tHit >= 0 && !(sim.world.RayIntersect(eye, dn, out RayHit wh) && wh.distance < tHit);
            double reach = sim.cfg.Num("weapons.gauntlet.range");
            gun = "revolver";
            if (level >= 4) gun = "nailgun";
            if (level >= 3 && dist < 12) gun = "shotgun";
            if (level >= 2 && tHit >= 0 && tHit <= reach) gun = "gauntlet";
            if (!sim.loadout.owned.Contains(gun)) gun = "revolver";
            int gi = sim.loadout.owned.IndexOf(gun);
            bool fire = fireOn && see;
            Combat = new CombatInput { weapon = sim.loadout.Weapon != gun ? gi : -1, fire = new Button { held = fire, pressed = fire && step % 2 == 0 } };
        }
    }
}
