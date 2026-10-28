// NIGHT40 builder B2 (brawl40): scripted fighters for THE ICEBREAKER (the harness's `n40brawl` and the Player reel brawl40). Bot-side numbers
// only (no rule changes); its hand from its own Mulberry32 (the side it circles and dodges to, the casual pilot's misses).
//   Mixed (the SKILLED pilot): the KESTREL in hand up close (inside range.close), circling the boss (never standing: boss_rules 3); the PULSE
//            GAUNTLET's heavy swing whenever it is ready and the body is in reach (swap, swing, swap back); the F punch with the gun in hand when
//            in punch reach; on the haymaker's tell it dashes sideways out of the cone in its last ~0.25 s (after the boss commits), then closes
//            on the stuck fist; it jumps the slam's ring as it arrives; it punches the YELLOW block back; it side-dashes the rush.
//   Casual   (the real-person question, L-058): the same verbs, worse: it reacts to a haymaker / ring / rush 70 % of the time and later, parries
//            a block 50 % of the time, aims at half the rate, clicks slower, and circles wider (3.5 m).
//   Range    (the pistol-from-range control): the KESTREL alone from ~14 m, kiting round the arena's centre (never melee, never parries); it
//            jumps rings and side-dashes rushes as the skilled one: the card's ">= 3x a mixed fight" check.
// No UnityEngine.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Brawl40Pilot // NIGHT42 B1: partial (brawl42's answers in Brawl40Pilot.N42.cs)
    {
        public enum Style { Mixed, Casual, Range }

        public InputFrame Move;
        public CombatInput Combat;
        public Style style = Style.Mixed;
        public Mulberry32 hand;
        public int swings, shots, punches, dashes, dodges, jumps, blockPunches, stillSteps, missedReacts;
        /// <summary>Swings that hit something (the meleeSwing event's hits).</summary>
        public int swingHits;
        double yawL, pitchL, side = 1;
        int stepNo = -1, swingCool, shotCool, punchCool, dashCool, swapBack, sideLeft, jumpHold;
        bool started, dodgedThis, swung, fireHeld, ringJumped, reactThis = true, blockTry = true;
        string lastAttack;
        int lastTellStep = -1;

        bool Casual => style == Style.Casual;

        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            Brawl40 g = sim.brawl40;
            double dt = sim.step;
            if (!started)
            {
                started = true;
                yawL = yawNow;
                side = hand != null && hand.Next() < 0.5 ? -1 : 1;
                sim.events.On<MeleeEvent>("meleeSwing", e => { if (e.owner == "player" && e.weapon == "gauntlet") { swung = true; if (e.hits > 0) swingHits++; } });
            }
            if (swingCool > 0) swingCool--;
            if (shotCool > 0) shotCool--;
            if (punchCool > 0) punchCool--;
            if (dashCool > 0) dashCool--;
            if (sideLeft > 0) sideLeft--;
            Vec3 me = sim.mover.position, eye = sim.Eye;
            Vec3 vel = sim.mover.velocity;
            if (sim.mover.grounded && JsMath.Hypot(vel.x, vel.z) < 1) stillSteps++;
            bool fight = g != null && g.Alive;
            Vec3 boss = fight ? g.Feet : me;
            Vec3 o = g != null ? g.origin : me;
            double surf = fight ? g.RangeToPlayer() : 99;
            Vec3 toBoss = new Vec3(boss.x - me.x, 0, boss.z - me.z);
            double tb = JsMath.Hypot(toBoss.x, toBoss.z);
            Vec3 u = tb > 1e-6 ? toBoss / tb : new Vec3(0, 0, -1);
            Vec3 tang = new Vec3(-u.z, 0, u.x) * side;
            string atk = fight ? g.attack : null;
            bool tell = fight && g.state == Brawl40State.Tell;
            // a new attack: roll this pilot's reaction once (the casual pilot misses some)
            if (tell && (atk != lastAttack || g.stateSteps < lastTellStep))
            {
                reactThis = !Casual || hand == null || hand.Next() < 0.7;
                blockTry = !Casual || hand == null || hand.Next() < 0.5;
                if (!reactThis) missedReacts++;
                dodgedThis = false;
                ringJumped = false;
                sweepJumped = leapDashed = false; sweepAirJumps = 0; // NIGHT42 B1
            }
            lastAttack = tell ? atk : null;
            lastTellStep = tell ? g.stateSteps : -1;
            bool punish = fight && (g.state == Brawl40State.Stuck || g.state == Brawl40State.Window);
            // where to stand: mixed = a ring ~2.5 m off its surface (casual 3.5 m), circling; in a punish, onto it; range = ~14 m round the centre
            double want = style == Style.Range ? 14 : punish ? 0.6 : Casual ? 3.5 : 2.5;
            Vec3 wish = (u * (surf - want)) + tang * (style == Style.Range ? 1.2 : 0.6);
            if (style == Style.Range)
            {
                // kite round the arena's centre: hold ~11 m from it on the far side from the boss
                Vec3 fromC = new Vec3(me.x - o.x, 0, me.z - o.z);
                double rc = JsMath.Hypot(fromC.x, fromC.z);
                if (rc > 1e-6) wish = wish + fromC / rc * (11 - rc) * 0.5;
            }
            if (sideLeft == 0 && hand != null && hand.Next() < 0.004) { side = -side; sideLeft = 240; }
            double lim = (g != null ? g.half : 18) - 2.5;
            if (me.x - o.x > lim) wish = wish + new Vec3(-2, 0, 0);
            if (me.x - o.x < -lim) wish = wish + new Vec3(2, 0, 0);
            if (me.z - o.z > lim) wish = wish + new Vec3(0, 0, -2);
            if (me.z - o.z < -lim) wish = wish + new Vec3(0, 0, 2);
            // phase 2's floes: off a RED or open floe (where it stands, or 1.5 m along its wish), toward the floe's outside
            if (fight && g.floeTellLeft.Length > 0 && reactThis)
            {
                double wl0 = JsMath.Hypot(wish.x, wish.z);
                Vec3 ahead = wl0 > 1e-6 ? me + wish / wl0 * 1.5 : me;
                for (int qi = 0; qi < 2; qi++)
                {
                    int fi = g.FloeAt(qi == 0 ? me : ahead);
                    if (fi < 0 || (g.floeTellLeft[fi] == 0 && g.floeOpenLeft[fi] == 0)) continue;
                    g.FloeRect(fi, out double x0, out double z0, out double x1, out double z1);
                    Vec3 away = new Vec3(me.x - (x0 + x1) * 0.5, 0, me.z - (z0 + z1) * 0.5);
                    double al = JsMath.Hypot(away.x, away.z);
                    wish = al > 1e-6 ? away / al * 4 + u * 0.5 : wish + tang * 4;
                    break;
                }
            }
            bool dash = false, jump = false;
            double late = Casual ? 0.15 : 0.25;
            // the haymaker / the rush: in their last moments (committed), dash sideways off the line
            bool lineAttack = atk == Brawl40.Rush || (atk == Brawl40.Haymaker && surf < g.p.hayReach + g.p.hayLunge + 1.5);
            if (tell && !dodgedThis && reactThis && lineAttack && g.stateLeft * dt <= late)
            {
                Vec3 f = atk == Brawl40.Rush ? g.rushDir : g.facing;
                Vec3 perp = new Vec3(-f.z, 0, f.x);
                if ((me.x - boss.x) * perp.x + (me.z - boss.z) * perp.z < 0) perp = -perp;
                wish = perp * 3 - f * 0.5;
                if (sim.mover.pips >= 1 && dashCool == 0) { dash = true; dashCool = 40; dashes++; }
                else jump = sim.mover.grounded;
                dodges++;
                dodgedThis = true;
            }
            // the slam's ring: jump as its front arrives
            if (fight && g.ringOn && !ringJumped && reactThis && sim.mover.grounded)
            {
                double d = JsMath.Hypot(me.x - g.ringAt.x, me.z - g.ringAt.z);
                double eta = (d - g.ringR) / Math.Max(1, g.p.slamSpeed);
                if (eta > 0 && eta <= (Casual ? 0.12 : 0.18)) { jump = true; ringJumped = true; jumps++; }
            }
            if (fight && g.on42) Answer42(sim, g, me, surf, tell, atk, tang, dt, ref wish, ref dash, ref jump); // NIGHT42 B1: the sweep and the leap
            if (jump) jumpHold = 14;
            double wl = JsMath.Hypot(wish.x, wish.z);
            double wx = wl > 1e-6 ? wish.x / wl : 0, wz = wl > 1e-6 ? wish.z / wl : 0;
            // aim: the block coming in, else its chest
            Vec3 aim = fight ? g.Centre : boss + new Vec3(0, 1.5, 0);
            bool blockNow = false;
            if (fight && g.block.alive && !g.block.back && style != Style.Range && blockTry)
            {
                double bd = Vec3.DistanceTo(g.block.pos, eye);
                if (bd < 8) { aim = g.block.pos; blockNow = bd < g.p.blockParryReach - 0.4; }
            }
            Vec3 dv = aim - eye;
            double aimRate = Casual ? 6 : 12;
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
            if (jump) fr.jump = new Skygrind.Core.Button { held = true, pressed = true };
            else if (jumpHold > 0) { fr.jump = new Skygrind.Core.Button { held = true }; jumpHold--; }
            Move = fr;
            // the hands
            int gi = sim.loadout.owned.IndexOf("gauntlet"), ri = sim.loadout.owned.IndexOf("revolver");
            string inHand = sim.loadout.Weapon;
            Combat = new CombatInput { weapon = -1 };
            bool aimed = Math.Abs(dy) < 0.12;
            if (!fight || g.state == Brawl40State.Reveal || g.state == Brawl40State.PhaseChange || g.state == Brawl40State.Dying) return;
            // the block: punch it back (the F punch with the gun in hand; the swing with the gauntlet in hand)
            if (blockNow && aimed)
            {
                if (inHand == "gauntlet" && swingCool == 0) { Combat.fire = new Skygrind.Core.Button { held = true, pressed = true }; blockPunches++; return; }
                if (inHand != "gauntlet" && punchCool == 0) { Combat.punch = new Skygrind.Core.Button { held = true, pressed = true }; punchCool = 44; punches++; blockPunches++; return; }
            }
            bool melee = style != Style.Range;
            bool swingNow = melee && gi >= 0 && swingCool == 0 && surf <= 1.6 && aimed;
            if (swingNow && inHand != "gauntlet") { Combat.weapon = gi; return; }
            if (inHand == "gauntlet")
            {
                if (swung)
                {
                    // the swing went off (the meleeSwing event): its cooldown runs, then the gun back after a few steps
                    swung = false;
                    // NIGHT42 B1: with any NIGHT42 row on, the gauntlet's LIVE cycle (the cooldown its swing just set: gauntlet42's 1.8 s when on);
                    // every row off: NIGHT41's pilot (design.n27.pulse27.swingSeconds)
                    swingCool = sim.n42.Any ? sim.loadout.GunCycleLeft29 + 2 : (int)Math.Ceiling(sim.cfg.Num("design.n27.pulse27.swingSeconds") / dt) + 2;
                    swings++;
                    swapBack = 8;
                    return;
                }
                if (swingCool == 0 && surf <= 2.2)
                {
                    // hold the trigger until the raised gauntlet can swing (a switch's fire fraction drops a press made too early)
                    Combat.fire = new Skygrind.Core.Button { held = true, pressed = !fireHeld };
                    fireHeld = true;
                    return;
                }
                fireHeld = false;
                if (swapBack > 0 && --swapBack > 0) return;
                if (ri >= 0) Combat.weapon = ri;
                return;
            }
            fireHeld = false;
            if (inHand != "revolver") { if (ri >= 0) Combat.weapon = ri; return; }
            // the KESTREL: a click every ~0.5 s (casual ~0.7 s; the hold charges under p39: clicks only)
            if (shotCool == 0 && aimed && (style == Style.Range || surf <= g.p.rangeClose + 1))
            {
                Combat.fire = new Skygrind.Core.Button { held = true, pressed = true };
                shotCool = Casual ? 84 : 60;
                shots++;
            }
            else if (melee && punchCool == 0 && surf <= 1.0 && aimed)
            {
                Combat.punch = new Skygrind.Core.Button { held = true, pressed = true };
                punchCool = 44;
                punches++;
            }
        }
    }
}
