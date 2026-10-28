// NIGHT43 builder W1 parry43 (N43Blocks.parry43, design.n43.parry43). GG 2026-10-16 (G18): "the player currently has to wait until an incoming
// projectile is extremely close". CODE-FACTS-B G18: with parry42 off (NIGHT42's game) the DRONE orb is caught only within 2 m of the eye POINT
// and hurts at 0.70 m from the body's axis: a ~0.11 s window at 12 m/s; the gauntlet's swing opens no buffer at all.
// With the row on (the sim sets the numbers at a run's start, SurvivalSim.N43.Weapons.cs) an orb is caught when ANY of these holds:
//   1. parry42's volume: the orb's surface within reach42 (orb.reflectRange x reachMult) of the body's axis (NearestHostile42's test);
//   2. THE CATCH VOLUME (combat-web.md A1, ULTRAKILL's 3 m sphere 3 m ahead, sized to the gauntlet): the orb's surface inside a sphere of
//      catchRadius43 centred catchAhead43 ahead of the eye along the aim;
//   3. THE TIME WINDOW (combat-web.md A3: "express the window as time, not metres, so fast and slow orbs feel the same"): flying on as it
//      flies now, the orb touches the player's capsule within windowSteps43 steps (the same touch test DroneOrbs.Step makes).
// parry42's mercy (a touching orb waits before its damage) and the orb buffer (Core/Parry42.cs: a punch or a swing that sent nothing back keeps
// catching for bufferS; the swing opens it too, Foes40.SwingReflect40's miss) run with parry43's own numbers. Off (parry43 false): DroneOrbs.TryReflect
// as NIGHT42 has it, bit for bit. Counters (the harness): which test caught each orb.
namespace Skygrind.Core
{
    public sealed partial class DroneOrbs
    {
        /// <summary>parry43: on, the catch sphere (radius, centre ahead of the eye, m), the time window (steps), the eye's height over the feet.</summary>
        public bool parry43;
        public double catchRadius43, catchAhead43, eyeHeight43;
        public int windowSteps43;

        /// <summary>parry43's counters: orbs caught by parry42's body volume, by the sphere ahead, by the time window (in that order of tests).</summary>
        public int caughtBody43, caughtAhead43, caughtSoon43;

        /// <summary>parry43: a run's start (SurvivalSim.N43.Weapons.cs); off clears it.</summary>
        public void Parry43Set(bool on, double catchRadius, double catchAhead, int windowSteps, double eyeHeight)
        {
            parry43 = on;
            catchRadius43 = on ? catchRadius : 0;
            catchAhead43 = on ? catchAhead : 0;
            windowSteps43 = on ? windowSteps : 0;
            eyeHeight43 = on ? eyeHeight : 0;
            caughtBody43 = caughtAhead43 = caughtSoon43 = 0;
        }

        /// <summary>parry43: the live hostile orb nearest the eye that parry42's volume, the sphere ahead or the time window catches (null: none).</summary>
        public Orb NearestHostile43(Vec3 eye, Vec3 aim)
        {
            Orb best = null;
            int bestBy = 0;
            double bestD = double.PositiveInfinity;
            double drop = capsuleHeight - 2 * capsuleRadius;
            double cx = eye.x + aim.x * catchAhead43, cy = eye.y + aim.y * catchAhead43, cz = eye.z + aim.z * catchAhead43;
            double fy = eye.y - eyeHeight43; // the feet
            double contact = radius + capsuleRadius;
            foreach (Orb orb in pool)
            {
                if (!orb.alive || orb.reflected) continue;
                Vec3 p = orb.position;
                int by = 0;
                if (reach42 > 0 && PointSegmentDistance(p, eye.x, eye.y, eye.z, eye.x, eye.y - drop, eye.z) - radius <= reach42) by = 1;
                else if (JsMath.Hypot(p.x - cx, p.y - cy, p.z - cz) - radius <= catchRadius43) by = 2;
                else if (Soon43(orb, eye.x, fy, eye.z, contact)) by = 3;
                if (by == 0) continue;
                double d = Vec3.DistanceTo(p, eye);
                if (d < bestD)
                {
                    best = orb;
                    bestD = d;
                    bestBy = by;
                }
            }
            if (bestBy == 1) caughtBody43++;
            else if (bestBy == 2) caughtAhead43++;
            else if (bestBy == 3) caughtSoon43++;
            return best;
        }

        /// <summary>parry43 (round 2, the globs): the step length, the capsule's numbers, the body volume's drop (SpitterAcid.Parry43Glob reads them).</summary>
        internal double Dt43 => dt;
        internal double CapsuleRadius43 => capsuleRadius;
        internal double BodyDrop43 => capsuleHeight - 2 * capsuleRadius;

        /// <summary>parry43 round 2: the body volume (test 1) for any projectile's centre p of radius r: its surface within reach42 of the body's
        /// axis below the eye.</summary>
        internal bool InBody43(Vec3 p, double r, Vec3 eye) =>
            reach42 > 0 && PointSegmentDistance(p, eye.x, eye.y, eye.z, eye.x, eye.y - BodyDrop43, eye.z) - r <= reach42;

        /// <summary>parry43: does the orb, flying on at its velocity, touch the capsule standing at feet (fx, fy, fz) within windowSteps43 steps?
        /// The touch is DroneOrbs.Step's (the orb's centre within radius + capsuleRadius of the capsule's axis).</summary>
        bool Soon43(Orb orb, double fx, double fy, double fz, double contact)
        {
            double y0 = fy + capsuleRadius, y1 = fy + capsuleHeight - capsuleRadius;
            for (int k = 0; k <= windowSteps43; k++)
            {
                double t = k * dt;
                var q = new Vec3(orb.position.x + orb.velocity.x * t, orb.position.y + orb.velocity.y * t, orb.position.z + orb.velocity.z * t);
                if (PointSegmentDistance(q, fx, y0, fz, fx, y1, fz) <= contact) return true;
            }
            return false;
        }
    }

    // parry43 round 2 (DESIGN-RULES C-5: the same catch rules for every parryable thing): the SPITTER's glob gets the orb's three tests with
    // the orb's numbers (the sim sets them on DroneOrbs; the glob reads them through `orbs`): the body volume, the sphere ahead, and the time
    // window flown as the glob flies (its gravity) against the glob's own touch test (SpitterAcid.Step: feet + 0.3 .. feet + 1.5 m, globRadius
    // + capsuleRadius). Today a glob is caught only within orb.reflectRange (2 m) of the eye point: at globSpeed 16 m/s ~0.09 s. Off (no orbs or
    // parry43 off): null, and TryReflect's 2 m point test runs as NIGHT42's.
    public sealed partial class SpitterAcid
    {
        /// <summary>parry43 round 2: the run's globs caught by the body volume, the sphere ahead, the time window (the harness reads them).</summary>
        public int globBody43, globAhead43, globSoon43;

        /// <summary>parry43 round 2: the live hostile glob nearest the eye that one of the three tests catches (null: none, or parry43 off).</summary>
        Glob Parry43Glob(Vec3 eye, Vec3 aim)
        {
            DroneOrbs o = orbs;
            if (o == null || !o.parry43) return null;
            Glob best = null;
            int bestBy = 0;
            double bestD = double.PositiveInfinity;
            double r = p.globRadius;
            double cx = eye.x + aim.x * o.catchAhead43, cy = eye.y + aim.y * o.catchAhead43, cz = eye.z + aim.z * o.catchAhead43;
            double fy = eye.y - o.eyeHeight43, touch = r + capsuleRadius, sdt = o.Dt43;
            foreach (Glob g in globs)
            {
                if (!g.alive || g.reflected) continue;
                Vec3 q = g.position;
                int by = 0;
                if (o.InBody43(q, r, eye)) by = 1;
                else if (JsMath.Hypot(q.x - cx, q.y - cy, q.z - cz) - r <= o.catchRadius43) by = 2;
                else
                {
                    // the time window: the glob's own flight (its gravity), step by step, against its own touch test
                    Vec3 pos = q, vel = g.velocity;
                    for (int k = 0; k <= o.windowSteps43 && by == 0; k++)
                    {
                        double dy = pos.y - System.Math.Max(fy + 0.3, System.Math.Min(fy + 1.5, pos.y));
                        if (JsMath.Hypot(pos.x - eye.x, dy, pos.z - eye.z) <= touch) by = 3;
                        vel.y -= gravity * sdt;
                        pos = pos + vel * sdt;
                    }
                }
                if (by == 0) continue;
                double d = Vec3.DistanceTo(q, eye);
                if (d < bestD)
                {
                    best = g;
                    bestD = d;
                    bestBy = by;
                }
            }
            if (bestBy == 1) globBody43++;
            else if (bestBy == 2) globAhead43++;
            else if (bestBy == 3) globSoon43++;
            return best;
        }
    }
}
