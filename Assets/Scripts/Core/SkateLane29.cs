// NIGHT29 builder B (boss2): THE SKATE LANE rule (DESIGN29 card G). A rule applied AFTER the Mover each step, outside it (Mover.cs is frozen): on an ice lane a grounded player above `snapSpeed`
// is eased to the nearest lane line (the wall ride's snap look: the position moves toward the line at `easeRate` m/s and the sideways velocity is zeroed, speed bleeds `bleedPerSecond` of itself per
// second); a ground dash sideways (the Mover's Dash state, entered from the ground, with at least `dashLateralFrac` of its speed across the lane) moves the line to the ADJACENT lane over
// `switchSeconds`; under snapSpeed or off the flat lane (the bank) the rule does nothing and the ice friction x0.25 (design.n26.surfaces) stays. The lane lines are `count` lines `spacing` m apart
// centred on the rink's centre line (Rink28Math: lateral positive outward). Pure maths on JsMath, whole-step timers, no randomness, no UnityEngine, no audio.
using System;

namespace Skygrind.Core
{
    public sealed class SkateLane29
    {
        public int count;
        public double spacing, snapSpeed, easeRate, switchSeconds, dashLateralFrac, bleedPerSecond;

        MoveState prevState = MoveState.Ground;
        bool prevGrounded;
        int switchLeft, switchTotal;
        double switchFrom, switchTo;
        /// <summary>True while the player is locked on a lane line (the view's lane glow), and the line's index (0 = most inward).</summary>
        public bool Locked { get; private set; }
        public int LaneIndex { get; private set; }
        public int switches, dashStarts;

        public static SkateLane29 From(GameConfig c)
        {
            const string g = "design.n29.boss2.lane.";
            return new SkateLane29
            {
                count = c.Int(g + "count"), spacing = c.Num(g + "spacing"), snapSpeed = c.Num(g + "snapSpeed"), easeRate = c.Num(g + "easeRate"),
                switchSeconds = c.Num(g + "switchSeconds"), dashLateralFrac = c.Num(g + "dashLateralFrac"), bleedPerSecond = c.Num(g + "bleedPerSecond"),
            };
        }

        /// <summary>The lateral offset (outward positive) of lane line i.</summary>
        public double LineLat(int i) => (i - (count - 1) * 0.5) * spacing;

        /// <summary>The index of the lane line nearest a lateral offset.</summary>
        public int NearestLine(double lat)
        {
            int i = (int)JsMath.Round(lat / spacing + (count - 1) * 0.5);
            return i < 0 ? 0 : i >= count ? count - 1 : i;
        }

        public void Reset()
        {
            prevState = MoveState.Ground;
            prevGrounded = false;
            switchLeft = switchTotal = 0;
            Locked = false;
            switches = 0;
        }

        static void Normal(Rink28Math rink, double s, out double nx, out double nz)
        {
            rink.PointAt(s, 0, out double x0, out double z0, out _, out _);
            rink.PointAt(s, 1, out double x1, out double z1, out _, out _);
            nx = x1 - x0;
            nz = z1 - z0;
        }

        /// <summary>One step, after the Mover. `active` = the player is on the ice lane of the fight (false: the rule forgets everything).</summary>
        /// <summary>NIGHT30 (builder S): design.n30.glide is on: the snap lanes are OFF (GG: "I don't like how the rails currently 'stick' you"); the sim sets it.</summary>
        public bool off30;

        public void Step(Mover m, Rink28Math rink, double laneHalf, bool active, double dt)
        {
            if (!active || count <= 0 || off30)
            {
                Reset();
                return;
            }
            Vec3 pos = m.position, v = m.velocity;
            rink.Project(pos.x, pos.z, out double s, out double lat);
            double speed = JsMath.Hypot(v.x, v.z);
            Normal(rink, s, out double nx, out double nz);
            bool flat = Math.Abs(lat) <= laneHalf + 0.25;
            MoveState st = m.state;
            if (st == MoveState.Dash && prevState != MoveState.Dash) dashStarts++;
            // a ground dash sideways switches to the adjacent line
            if (st == MoveState.Dash && prevState != MoveState.Dash && prevGrounded && flat && switchLeft == 0 && speed > 1e-6)
            {
                double vlat = v.x * nx + v.z * nz;
                if (Math.Abs(vlat) >= dashLateralFrac * speed)
                {
                    int from = NearestLine(lat), to = from + (vlat > 0 ? 1 : -1);
                    if (to >= 0 && to < count)
                    {
                        switchFrom = lat;
                        switchTo = LineLat(to);
                        switchTotal = switchLeft = Math.Max(1, MovementMath.StepsFor(switchSeconds, dt));
                        switches++;
                    }
                }
            }
            prevState = st;
            prevGrounded = m.grounded;
            if (switchLeft > 0)
            {
                switchLeft--;
                double u = 1.0 - (double)switchLeft / switchTotal, k = u * u * (3 - 2 * u);
                Place(m, rink, s, switchFrom + (switchTo - switchFrom) * k, pos.y);
                if (switchLeft == 0) RemoveLateral(m, nx, nz);
                Locked = true;
                LaneIndex = NearestLine(lat);
                return;
            }
            bool onLane = (st == MoveState.Ground || st == MoveState.Slide) && m.grounded && flat && speed > snapSpeed;
            Locked = onLane;
            if (!onLane) return;
            int idx = NearestLine(lat);
            LaneIndex = idx;
            double err = LineLat(idx) - lat, stp = easeRate * dt;
            double dl = Math.Max(-stp, Math.Min(stp, err));
            if (dl != 0) Place(m, rink, s, lat + dl, pos.y);
            RemoveLateral(m, nx, nz);
            double keep = 1.0 - bleedPerSecond * dt;
            m.velocity = new Vec3(m.velocity.x * keep, m.velocity.y, m.velocity.z * keep);
        }

        static void Place(Mover m, Rink28Math rink, double s, double lat, double y)
        {
            rink.PointAt(s, lat, out double x, out double z, out _, out _);
            m.position = new Vec3(x, y, z);
        }

        static void RemoveLateral(Mover m, double nx, double nz)
        {
            Vec3 v = m.velocity;
            double nl = nx * nx + nz * nz;
            if (nl < 1e-12) return;
            double a = (v.x * nx + v.z * nz) / nl;
            m.velocity = new Vec3(v.x - a * nx, v.y, v.z - a * nz);
        }
    }
}
