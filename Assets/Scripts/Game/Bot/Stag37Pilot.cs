// NIGHT37 builder S (stag37): a scripted CHASER for THE STAG (the harness's band and the Player reel stag37). It skates the canal's path (steers at
// the path a few metres ahead on its chosen lane), pumps dashes on the ice, sits in the Stag's lane (its slipstream), leaves the lane for a shard
// kick's or a kick back's wind-up and for a floe that pulses RED ahead, jumps a lane-cut barrier, punches the ice chunk back, punches a hind leg
// once it is within reach, dash-punches the fallen Stag (the launch) in the CYAN window, and in leg 3 meets the head-on charge with a punch.
// Bot-side numbers only (no rule changes); its hand (aim speed, the lane it dodges to) from its own Mulberry32.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Stag37Pilot
    {
        public InputFrame Move;
        public CombatInput Combat;
        public int punches, dashes, jumps, dodges, laneChanges;
        public Mulberry32 hand;
        /// <summary>Off: it never punches (the control).</summary>
        public bool punch = true;
        /// <summary>A CASUAL player (the lead's real-person question): sprint-glides on the ice (the sprint held), no dash pumping; a dash only to
        /// close on the fallen Stag in its window. Everything else (lanes, dodges, the parries, the punches) as the skilled pilot.</summary>
        public bool casual;
        /// <summary>Steps it spent standing (grounded, under 1 m/s): the reel / band print it.</summary>
        public int stillSteps;
        double yawL, pitchL, aimRate = 10;
        int stepNo = -1, dashCool, punchCool, jumpHold, sinceDash = 1000, dodgeLane = -1, dodgeLeft;
        bool started;
        double laneLat;
        /// <summary>The lane line it steers for now (the harness's trace).</summary>
        public double LaneLatNow => laneLat;

        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            Stag37 g = sim.stag37;
            Stag37Arena a = sim.stag37Arena;
            double dt = sim.step;
            if (!started) { started = true; yawL = yawNow; pitchL = 0; }
            Vec3 me = sim.mover.position, eye = sim.Eye, o = sim.stag37Centre.Value, l = me - o;
            a.Project(l.x, l.z, out double s, out double lat);
            Vec3 vel = sim.mover.velocity;
            double spd = JsMath.Hypot(vel.x, vel.z);
            if (sim.mover.grounded && spd < 1) stillSteps++;
            if (dashCool > 0) dashCool--;
            if (punchCool > 0) punchCool--;
            sinceDash++;
            bool fight = g != null && g.Alive;
            Boss37Brain b = fight ? g.brain : null;
            string atk = fight ? g.AttackId : null;
            bool tell = b != null && (b.state == Boss37State.Tell || b.state == Boss37State.Active);
            int myLane = a.LaneOf(lat);
            // the lane: the Stag's (its slipstream); dodge out of it for a shard kick / kick back; away from a RED floe ahead
            int want = fight ? a.LaneOf(g.lat) : 1;
            if (fight && tell && (atk == Stag37.ShardKick || atk == Stag37.KickBack))
            {
                if (dodgeLeft == 0) { dodgeLane = Safe(sim, a, s, myLane, Other(a, a.LaneOf(g.lat), s), spd); dodgeLeft = 150; dodges++; }
            }
            if (dodgeLeft > 0) { dodgeLeft--; want = dodgeLane; }
            // a floe RED or gone ahead: stay in my lane if it is clear, else move ONE lane over (never across the bad lane)
            if (FloeBad(sim, a, s, want, spd) || (want != myLane && Math.Abs(want - myLane) > 1 && FloeBad(sim, a, s, (want + myLane) / 2, spd)))
            {
                if (!FloeBad(sim, a, s, myLane, spd)) want = myLane;
                else { int alt = SafeLane(sim, a, s, myLane, spd); if (alt >= 0) want = alt; }
            }
            if (a.LaneWallAt(s, 1.0)) want = myLane; // between lane walls: stay
            if (want != a.LaneOf(laneLat)) laneChanges++;
            laneLat = a.LaneLat(want);
            bool window = b != null && b.Open;
            bool leg3 = fight && g.leg3 != Stag37Leg3.None;
            // where to go: the path ahead on the lane; in the window, the fallen Stag; in leg 3's charge, straight at its antlers
            Vec3 goal = a.PointAt(s + 7, laneLat) + o;
            if (window || (leg3 && g.leg3 == Stag37Leg3.Charge)) goal = new Vec3(g.Feet.x, me.y, g.Feet.z);
            else if (leg3 && g.leg3 == Stag37Leg3.Turned && Math.Abs(g.gap) < 30)
            {
                // meet it head-on, but not before its charge: hold 10 m off its antlers on its lane (keep moving: circle the point)
                Vec3 meet = g.Feet + g.Facing * 10;
                goal = new Vec3(meet.x, me.y, meet.z);
                if (Vec3.DistanceTo(me, goal) < 3) goal = me + new Vec3(-g.Facing.z, 0, g.Facing.x) * 4;
            }
            Vec3 d = goal - me;
            double wx = d.x, wz = d.z, wl = JsMath.Hypot(wx, wz);
            if (wl > 1e-6) { wx /= wl; wz /= wl; }
            // too fast past it, or onto the fallen Stag: brake (the input against the slide: the Mover's own braking)
            double toIt = fight ? Vec3.DistanceTo(me, g.Feet) : 99;
            if (fight && spd > 14 && ((window && toIt < 12) || (!window && !leg3 && g.gap < -2)))
            {
                wx = -vel.x / spd; wz = -vel.z / spd;
            }
            bool jump = false, dash = false;
            // a barrier ahead in my lane: jump it
            foreach (var (lo, hi, _) in sim.stag37Cuts)
            {
                Vec3 c = (lo + hi) * 0.5 + o;
                Vec3 to = c - me;
                double along = to.x * vel.x + to.z * vel.z;
                if (along > 0 && JsMath.Hypot(to.x, to.z) < Math.Max(2.5, spd * 0.18) && sim.mover.grounded) { jump = true; jumps++; }
            }
            // the pump: dash forward on the ice when the Stag is not right on top of us
            double gap = fight ? g.gap : 99;
            // ahead of it (it is behind us): ease off into another lane and let it pass, then take its slipstream
            if (fight && !window && !leg3 && gap < -2 && dodgeLeft == 0) { want = Safe(sim, a, s, myLane, Other(a, a.LaneOf(g.lat), s), spd); laneLat = a.LaneLat(want); }
            bool reveal = b != null && b.state == Boss37State.Reveal;
            if (sim.mover.grounded && dashCool == 0 && sim.mover.pips >= 1 && !jump && (gap > 6 || window || (gap > -2 && !leg3 && gap > 4)) && !reveal && sim.OnIce30(me) && !casual)
            {
                dash = true;
                dashCool = (int)(0.35 / dt);
                sinceDash = 0;
                dashes++;
            }
            // aim: the chunk coming in; the antlers in the charge; the hind legs when close; the fallen Stag in the window; else the path
            Vec3 aim = goal + new Vec3(0, 1.4, 0);
            bool punchNow = false;
            if (fight)
            {
                Stag37Chunk ch = g.chunk;
                if (ch.alive && !ch.back && Vec3.DistanceTo(ch.pos, eye) < 7) { aim = ch.pos; if (Vec3.DistanceTo(ch.pos, eye) < g.p.chunkParryReach - 0.3) punchNow = true; }
                else if (g.leg3 == Stag37Leg3.Charge && b.state == Boss37State.Active) { aim = g.Head; if (Vec3.DistanceTo(g.Head, eye) < g.p.chargeParryReach + 1.0) punchNow = true; }
                else if (window) { aim = g.Centre; if (Vec3.DistanceTo(g.Centre, eye) < g.p.launchReach && sinceDash < 30) punchNow = true; if (Vec3.DistanceTo(g.Centre, eye) < 7 && dashCool == 0 && sim.mover.pips >= 1) { dash = true; dashCool = 30; sinceDash = 0; } }
                else if (!leg3 && g.gap > -1 && g.gap < 9) { aim = g.Hind; if (Vec3.DistanceTo(new Vec3(me.x, me.y + 1, me.z), g.Hind) < g.p.catchRange - 0.3 && spd >= g.p.catchMinSpeed) punchNow = true; }
            }
            Vec3 dv = aim - eye;
            double wantYaw = JsMath.Atan2(-dv.x, -dv.z), wantPitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
            double stp = aimRate * dt, dy = wantYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, wantPitch - pitchL));
            var fr = new InputFrame { yaw = yawL, pitch = pitchL };
            Vec3 fwd = MovementMath.WishDirection(0, 1, yawL), right = MovementMath.WishDirection(1, 0, yawL);
            fr.moveZ = wx * fwd.x + wz * fwd.z;
            fr.moveX = wx * right.x + wz * right.z;
            if (dash) fr.dash = new Skygrind.Core.Button { held = true, pressed = true };
            if (jump) { fr.jump = new Skygrind.Core.Button { held = true, pressed = true }; jumpHold = 12; }
            else if (jumpHold > 0) { fr.jump = new Skygrind.Core.Button { held = true }; jumpHold--; }
            if (casual) fr.sprint = new Skygrind.Core.Button { held = true, pressed = tick % 240 == 0 };
            Move = fr;
            int gi = sim.loadout.owned.IndexOf("gauntlet");
            Combat = new CombatInput { weapon = gi >= 0 && sim.loadout.Weapon != "gauntlet" ? gi : -1 }; // the level's weapon in hand (the PULSE GAUNTLET)
            if (punch && punchNow && punchCool == 0)
            {
                // the gauntlet in hand: its swing is the fire button (the F punch is blocked with it in hand); otherwise the F punch
                if (sim.loadout.Weapon == "gauntlet") Combat.fire = new Skygrind.Core.Button { held = true, pressed = true };
                else Combat.punch = new Skygrind.Core.Button { held = true, pressed = true };
                punchCool = 12;
                punches++;
            }
        }

        int Other(Stag37Arena a, int lane, double s)
        {
            int n = a.p.lanes;
            int pick = (lane + 1 + (hand != null && hand.Next() < 0.5 ? 0 : 1)) % n;
            if (pick == lane) pick = (lane + 1) % n;
            return pick;
        }

        static bool FloeBad(SurvivalSim sim, Stag37Arena a, double s, int lane, double spd)
        {
            if (sim.stag37FloeDown == null) return false;
            for (int i = 0; i < a.floes.Count; i++)
            {
                if (a.floes[i].lane != lane) continue;
                double ahead = a.Ahead(s, a.floes[i].s0), behind = a.Ahead(s, a.floes[i].s1);
                bool near = (ahead > -1 && ahead < Math.Max(20, spd * 2.5)) || (ahead <= 0 && behind > 0);
                if (near && (sim.stag37FloeDown[i] || sim.stag37FloeTell[i] > 0)) return true;
            }
            return false;
        }

        /// <summary>The lane to move to, or my own when the move would cross or end on a floe that is RED or gone ahead.</summary>
        static int Safe(SurvivalSim sim, Stag37Arena a, double s, int mine, int to, double spd)
        {
            int step = to > mine ? 1 : -1;
            for (int c = mine + step; to != mine && c != to + step; c += step)
                if (FloeBad(sim, a, s, c, spd)) return mine;
            return to;
        }

        static int SafeLane(SurvivalSim sim, Stag37Arena a, double s, int from, double spd)
        {
            for (int k = 1; k < 2; k++) // one lane over only
                foreach (int c in new[] { from - k, from + k })
                    if (c >= 0 && c < a.p.lanes && !FloeBad(sim, a, s, c, spd)) return c;
            return -1;
        }
    }
}
