// NIGHT31 builder B (lantern31): how a good player breaks THE LANTERN's shield with the KESTREL's charge (design.n31.charge + design.n31.lantern31), wrapped round any rider's
// combat (LanternPilot30's) and shared by the harness (camp31 lantern31) and the Player reel (lantern31). The n31-r4 Player reel held the trigger only while riding: every ride
// ended before the 1.0 s charge was full, the release cancelled it, and the press of each new ride fired a plain shot (197 sparks, 0 breaks). Here:
//  - a ride starts the hold (the press's plain shot sparks, as it should); the hold is KEPT after the ride ends while the charge builds;
//  - the release comes when the charge is FULL, the shooter is on the wall (riding, or within 0.8 x rideGraceSeconds of the ride) and the view's ray lands on the lantern;
//  - a full charge that loses the wall or the aim is still held (n31.charge's auto-release fires it 0.5 s after full: the shield then reports why it rejected it);
//  - off the wall and not charging, or with the charge cooling, the rider's own fire as frame-robust taps (held 2 steps, released 2); with the core open, the same taps.
// Bot-side numbers only; no rule changes.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class LanternCharge31
    {
        int lastRide = -100000;
        bool holding;
        public int holds, releases;

        public CombatInput Fire(SurvivalSim sim, InputFrame look, CombatInput cb, int step)
        {
            Lantern27 l = sim.lantern27;
            Lantern31 l31 = sim.lantern31;
            ChargeShots27 c27 = sim.loadout.charge27;
            bool riding = sim.mover.state == MoveState.WallRide;
            if (riding) lastRide = step;
            bool tapWant = cb.fire.held || cb.fire.pressed;
            CombatInput Taps(bool want)
            {
                cb.fire = new Button { held = want && (step / 2) % 2 == 0, pressed = want && step % 4 == 0 };
                return cb;
            }
            if (l == null || l31 == null || c27 == null || !c27.active) return Taps(tapWant);
            if (l31.CoreOpen) { holding = false; return Taps(tapWant); }
            int grace = MovementMath.StepsFor(l31.p.rideGraceSeconds * 0.8, sim.cfg.Dt);
            bool wall = riding || step - lastRide <= grace;
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 fw = MovementMath.WishDirection(0, 1, look.yaw);
            double cp = JsMath.Cos(look.pitch);
            Vec3 dir = new Vec3(fw.x * cp, JsMath.Sin(look.pitch), fw.z * cp);
            double t = l.Raycast(eye, dir);
            bool aimOn = t >= 0 && !(sim.world.RayIntersect(eye, dir, out RayHit wh) && wh.distance < t);
            bool full = c27.charging && c27.Level(0) >= 1;
            if (holding)
            {
                if (full && wall && aimOn) { holding = false; releases++; cb.fire = default; return cb; }
                if (!c27.charging && !wall) { holding = false; return Taps(tapWant); } // the charge fired (auto-release) or was never started: back to the taps
                cb.fire = new Button { held = true, pressed = false };
                return cb;
            }
            if (riding && c27.CooldownShare(0) <= 0) { holding = true; holds++; cb.fire = new Button { held = true, pressed = true }; return cb; }
            return Taps(tapWant && !riding);
        }
    }
}
