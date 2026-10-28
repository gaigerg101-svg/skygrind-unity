// NIGHT37 builder C (crown37): a scripted player for THE CROWN's descent (the harness's band; the Player reel may use it). Bot-side numbers only;
// no rule changes. It plays the fight's one damage rule: from a piece, when the Crown hovers on the line toward the landing it offers
// (Crown37.landHint), DASH at the ring's centre and JUMP inside the dash (the jump-dash), hold the TORCH's trigger through the pass, then
// steer in the air (air.accel: holding a wish against the flight) to land on that piece where it will be (an orbit's At at the landing's
// step). It punches a yellow gem shard that comes within reach (the parry), walks onto the core's top through the reveal (the summit
// shatters), leaves a piece that tells (a beam cut or the habit drop), and keeps its torch in hand.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Crown37Pilot
    {
        public InputFrame Move;
        public CombatInput Combat;
        public int launches, punches, landings, missed;
        public Mulberry32 hand;
        /// <summary>NIGHT40 B5 (L-058: a CASUAL player next to the skilled one): no parry, no slam, the TORCH's trigger held on half the passes,
        /// half the blade rings dodged, a launch only after 0.5-1.0 s on a piece with a looser line (cos 0.9, 2.5-8.5 m) and an aim off the ring's
        /// centre by up to casualAim m sideways. Bot-side numbers only; false = NIGHT37's pilot exactly (the hand draws nothing extra).</summary>
        public bool casual;
        public double casualAim = 0.8;
        bool casualFire, casualDodge, casualDodgeDecided;
        double casualOff;
        double yawL, pitchL;
        int stepNo = -1, groundSteps, launchK = -1, flightSteps, punchCool, jumpHold;
        bool started, flying;
        double launchJitter;
        int dodgeK = -1, dodgePiece = -1;
        public int dodges;
        int launchTarget = -1;
        Vec3 ringAt, ringN;

        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            if (!started) { started = true; yawL = yawNow; }
            Crown37 c = sim.crown37;
            Orbit37 o = sim.orbit37;
            Mover m = sim.mover;
            double dt = sim.step;
            Vec3 me = m.position, chest = new Vec3(me.x, me.y + 1.0, me.z), eye = sim.Eye;
            if (punchCool > 0) punchCool--;
            Move = new InputFrame { yaw = yawL, pitch = pitchL, sprint = new Button { held = true } };
            int torch = sim.loadout.owned.IndexOf("torch");
            Combat = new CombatInput { weapon = torch >= 0 && sim.loadout.Weapon != "torch" ? torch : -1 };
            if (c == null || !c.Alive || o == null) return;

            groundSteps = m.grounded ? groundSteps + 1 : 0;
            // the parry: a yellow shard in reach
            Crown37Shard near = null;
            double nd = 2.8;
            foreach (Crown37Shard s in c.shards) { if (!s.alive || s.back || s.red) continue; double d = Vec3.DistanceTo(s.pos, eye); if (d < nd) { nd = d; near = s; } }
            if (near != null && launchK < 0 && !casual)
            {
                Look(eye, near.pos, 4);
                if (punchCool == 0 && nd < 2.6) { Combat.punch = new Button { held = true, pressed = true }; punchCool = 12; punches++; }
                return;
            }

            if (launchK >= 0)
            {
                // the jump-dash: the dash now (toward the ring's centre, flat), the jump 3 steps later, the jump held
                FaceFlat(me, casual ? Off(me, c.centre) : c.centre, 1);
                Move.moveZ = 1;
                if (launchK == 0) Move.dash = new Button { held = true, pressed = true };
                if (launchK == 3) { Move.jump = new Button { held = true, pressed = true }; jumpHold = 20; flying = true; flightSteps = 0; }
                launchK++;
                if (launchK > 3) launchK = -1;
                Fire(sim);
                return;
            }
            if (jumpHold > 0) { jumpHold--; Move.jump = new Button { held = true }; }
            // the blade ring's dodge: a jump 0.12 s before the cut, the double jump 0.2 s later; in the air, back over the piece where it will be
            if (dodgeK >= 0)
            {
                dodgeK++;
                if (dodgeK == 24) { Move.jump = new Button { held = true, pressed = true }; jumpHold = 10; }
                if (dodgePiece >= 0 && dodgePiece < o.pieces.Count)
                {
                    Vec3 at = o.At(o.pieces[dodgePiece], o.steps + 6);
                    double ddx = at.x - me.x, ddz = at.z - me.z;
                    if (JsMath.Hypot(ddx, ddz) > 0.4) { FaceFlat(me, at, 1); Move.moveZ = 1; }
                }
                if (m.grounded && dodgeK > 10) dodgeK = -1;
                else if (dodgeK < 200) return;
                else dodgeK = -1;
            }
            bool bladeTell = c.brain.state == Boss37State.Tell && c.brain.Current != null && c.brain.Current.id == "bladeRing";
            if (casual && !bladeTell) casualDodgeDecided = false;
            if (casual && bladeTell && !casualDodgeDecided) { casualDodgeDecided = true; casualDodge = hand != null && hand.Next() < 0.5; }
            if (m.grounded && launchK < 0 && bladeTell && c.brain.stateLeft * dt <= 0.12 && (!casual || casualDodge))
            {
                dodgeK = 0;
                dodges++;
                dodgePiece = o.standing;
                Move.jump = new Button { held = true, pressed = true };
                jumpHold = 20;
                return;
            }

            if (flying || !m.grounded)
            {
                flightSteps++;
                if (flightSteps < 60) Fire(sim);
                Steer(sim, c, o, me);
                if (m.grounded && flightSteps > 5 && m.state != MoveState.Slam)
                {
                    flying = false;
                    if (o.standing >= 0 || me.y > -0.5 || Math.Abs(me.y - sim.spire37Checkpoint[2].y) < 0.3 || Math.Abs(me.y - sim.spire37Checkpoint[1].y) < 0.3) landings++;
                    else missed++;
                }
                return;
            }

            Boss37Brain b = c.brain;
            if (b.state == Boss37State.Reveal || b.state == Boss37State.Off)
            {
                // the reveal: onto the core's top (it stays when the summit shatters)
                WalkTo(me, new Vec3(c.axis.x, me.y, c.axis.z), 1.0);
                return;
            }
            bool pieceTells = o.standing >= 0 && o.pieces[o.standing].state == Piece37State.Tell;
            int waitSteps = casual ? 60 + (int)(launchJitter * 60) : 18 + (int)(launchJitter * 30);
            bool can = (b.Fighting || b.state == Boss37State.LegChange) && c.HasLanding && groundSteps >= waitSteps && m.BoostReady(1, true);
            if (can)
            {
                double fx = c.centre.x - me.x, fz = c.centre.z - me.z, fl = JsMath.Hypot(fx, fz);
                double lx = c.landHint.x - me.x, lz = c.landHint.z - me.z, ll = JsMath.Hypot(lx, lz);
                double cosA = fl > 1e-6 && ll > 1e-6 ? (fx * lx + fz * lz) / (fl * ll) : 0;
                double dy = c.centre.y - chest.y;
                double hd = c.HoverDistance; // NIGHT37's 5 m: the band 3-7.5 m (casual 2.5-8.5 m), kept relative for crown40's distance
                bool lined = casual ? fl >= hd - 2.5 && fl <= hd + 3.5 && cosA > 0.9 && dy > -1.0 && dy < 2.6
                    : fl >= hd - 2.0 && fl <= hd + 2.5 && cosA > 0.97 && dy > -0.6 && dy < 2.2;
                if (lined || (pieceTells && fl < hd + 4))
                {
                    launchK = 0;
                    launches++;
                    launchTarget = c.passTarget;
                    ringAt = c.centre;
                    ringN = c.normal;
                    launchJitter = hand != null ? hand.Next() : 0.5;
                    if (casual)
                    {
                        casualOff = hand != null ? (hand.Next() * 2 - 1) * casualAim : 0;
                        casualFire = hand != null && hand.Next() < 0.5;
                    }
                    FaceFlat(me, casual ? Off(me, c.centre) : c.centre, 1);
                    Move.moveZ = 1;
                    Move.dash = new Button { held = true, pressed = true };
                    launchK = 1;
                    return;
                }
            }
            // wait: stay near the middle of the piece (or the ledge / plaza checkpoint), looking at the Crown
            Vec3 home = o.standing >= 0 ? o.pieces[o.standing].pos : me;
            if (Vec3.DistanceTo(new Vec3(home.x, me.y, home.z), me) > 1.0) WalkTo(me, new Vec3(home.x, me.y, home.z), 0.6);
            else Look(eye, c.centre, 3);
        }

        void Fire(SurvivalSim sim)
        {
            if (sim.loadout.Weapon == "torch" && (!casual || casualFire)) Combat.fire = new Button { held = true, pressed = false };
        }

        /// <summary>The casual aim: the ring's centre moved casualOff m sideways (flat, square to the line from the feet).</summary>
        Vec3 Off(Vec3 me, Vec3 at)
        {
            double dx = at.x - me.x, dz = at.z - me.z, l = JsMath.Hypot(dx, dz);
            if (l < 1e-6) return at;
            return new Vec3(at.x - dz / l * casualOff, at.y, at.z + dx / l * casualOff);
        }

        /// <summary>In the air. Until the chest is past the ring (or 0.35 s): no new wish (the wish along the flight: the takeoff's lurch keeps the line
        /// through the ring). Then the landing: the offered piece where it will be; trim toward it with the air's accel and SLAM (the slide key in the
        /// air: straight down) when the feet are over it.</summary>
        void Steer(SurvivalSim sim, Crown37 c, Orbit37 o, Vec3 me)
        {
            Vec3 v = sim.mover.velocity;
            double vf = JsMath.Hypot(v.x, v.z);
            if (sim.mover.state == MoveState.Slam) return;
            Vec3 chest = new Vec3(me.x, me.y + 1, me.z);
            double side = (chest.x - ringAt.x) * ringN.x + (chest.y - ringAt.y) * ringN.y + (chest.z - ringAt.z) * ringN.z;
            bool past = side < 0 || flightSteps > 42;
            if (!past)
            {
                if (vf > 1) { yawL = JsMath.Atan2(-v.x / vf, -v.z / vf); Move.yaw = yawL; }
                Move.moveZ = 1;
                return;
            }
            int ti = launchTarget;
            if (ti < 0 || !o.pieces[ti].Solid || o.pieces[ti].pos.y > me.y - 0.2)
            {
                // the offer is gone: the nearest solid piece of the leg under the flight
                int best = -1;
                double bd = 1e9;
                for (int i = 0; i < o.pieces.Count; i++)
                {
                    Piece37 q = o.pieces[i];
                    if (!q.Solid || q.pos.y > me.y - 0.2 || q.leg != c.Leg || q.kind == Piece37Kind.Summit || q.kind == Piece37Kind.Fall) continue;
                    double d = JsMath.Hypot(q.pos.x - me.x, q.pos.z - me.z);
                    if (d < bd) { bd = d; best = i; }
                }
                ti = best;
            }
            Vec3 target;
            if (ti >= 0)
            {
                double h0 = JsMath.Hypot(o.pieces[ti].pos.x - me.x, o.pieces[ti].pos.z - me.z);
                target = o.At(o.pieces[ti], o.steps + (int)(h0 / Math.Max(4, vf) / sim.step) + 8);
            }
            else if (c.Leg >= 3) target = c.landHint;
            else return;
            double dx = target.x - me.x, dz = target.z - me.z, dl = JsMath.Hypot(dx, dz);
            // slam when the feet are over the piece (its half size less a margin), or at the closest approach
            double half = ti >= 0 ? Math.Min(o.pieces[ti].size.x, o.pieces[ti].size.z) / 2 - 0.6 : 2.0;
            bool closing = dx * v.x + dz * v.z > 0;
            if (!casual && me.y > target.y + 0.3 && (dl <= Math.Max(0.8, half) || (!closing && dl < half + 1.5)))
            {
                Move.slide = new Button { held = true, pressed = true };
                return;
            }
            
            double nx = dx / Math.Max(1e-6, dl), nz = dz / Math.Max(1e-6, dl);
            // the wish straight at the landing (the takeoff's lurch, when still armed, turns the flight onto it; the air's accel after)
            yawL = JsMath.Atan2(-nx, -nz);
            Move.yaw = yawL;
            Move.moveZ = 1;
        }

        void FaceFlat(Vec3 me, Vec3 at, double gain)
        {
            double dx = at.x - me.x, dz = at.z - me.z;
            if (JsMath.Hypot(dx, dz) < 1e-6) return;
            yawL = JsMath.Atan2(-dx, -dz);
            pitchL = 0;
            Move.yaw = yawL;
            Move.pitch = pitchL;
        }

        void Look(Vec3 eye, Vec3 at, double gain)
        {
            Vec3 dv = at - eye;
            yawL = JsMath.Atan2(-dv.x, -dv.z);
            pitchL = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
            Move.yaw = yawL;
            Move.pitch = pitchL;
        }

        void WalkTo(Vec3 me, Vec3 to, double speedShare)
        {
            FaceFlat(me, to, 1);
            double d = JsMath.Hypot(to.x - me.x, to.z - me.z);
            Move.moveZ = d > 0.4 ? speedShare : 0;
        }
    }
}
