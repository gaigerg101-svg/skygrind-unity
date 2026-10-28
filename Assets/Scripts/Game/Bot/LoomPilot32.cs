// NIGHT32 builder B (design.n32.bosses32; the lead's 19:07 items 1 and 6): THE LOOM's moving pilot on the FULL sim (NIGHT31 only had the harness's standalone lag-bot model).
// Shared by the harness (boss32 loom, the pistol+fist bot) and the Player reel bosses32 (FeelReel.N32.Bosses.cs). A human-lag model on the real Mover, through inputs only:
// it runs from deck corner to deck corner (a new corner every cornerSeconds, so a flyer drop's mark is behind it), hops every hopSeconds (a sweep or a drop meets a grounded
// player), aims at the joint of the corner's leg it can see (else the nearest joint in sight; never the body under the deck) and fires the NAILGUN; in the nailgun's heat
// lock (heatTime of fire, then lockTime) it switches to the THUNDERCLAP. pistolFist: only the KESTREL and the PULSE gauntlet (the gauntlet when a joint is inside its reach).
// Buttons are frame-robust (the Player reel merges two sim steps a frame and keeps the LAST step's held): holds last whole runs of steps, presses are on 4-step boundaries.
// Numbers are bot-side: they shape one scripted opponent and change no rule.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class LoomPilot32
    {
        public InputFrame Move;
        public CombatInput Combat;
        public bool pistolFist;
        public int corner, hops, switches, lavaSteps, aimJoint = -1;
        public string gun = "nailgun";
        const double cornerSeconds = 3.0, hopSeconds = 1.75, inset = 3.5; // 1.8 m from the corner the sweep's knock put the pilot in the lava 7 times a fight
        int nextCorner = -1, fireSteps;

        public void Step(SurvivalSim sim, int step)
        {
            Loom30 l = sim.loom30;
            Move = default;
            Combat = new CombatInput { weapon = -1 };
            if (l == null || !l.Alive) return;
            double dt = sim.cfg.Dt;
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            if (nextCorner < 0 || step >= nextCorner) { corner = (corner + 1) % 4; nextCorner = step + MovementMath.StepsFor(cornerSeconds, dt); switches++; }
            Vec3 g = l.grips[corner];
            Vec3 inward = Vec3.Normalize(new Vec3(l.centre.x - g.x, 0, l.centre.z - g.z));
            Vec3 spot = new Vec3(g.x + inward.x * inset, g.y, g.z + inward.z * inset);
            // the target: the corner leg's joints first, then any joint in sight (the nearest)
            int pick = -1;
            double best = 1e9;
            for (int pass = 0; pass < 2 && pick < 0; pass++)
                for (int j = 0; j < l.joints.Length; j++)
                {
                    if (pass == 0 && j / 2 != corner) continue;
                    Vec3 d = l.joints[j] - eye;
                    double len = d.Length;
                    if (len < 1e-6 || len >= best) continue;
                    Vec3 dn = d / len;
                    double t = l.Raycast(eye, dn);
                    if (t < 0) continue;
                    if (sim.world.RayIntersect(eye, dn, out RayHit wh) && wh.distance < t) continue;
                    best = len; pick = j;
                }
            aimJoint = pick;
            Vec3 aim = pick >= 0 ? l.joints[pick] : l.joints[2 * corner + 1];
            Vec3 a = aim - eye;
            double wy = JsMath.Atan2(-a.x, -a.z), wp = JsMath.Atan2(a.y, JsMath.Hypot(a.x, a.z));
            Move = new InputFrame { yaw = wy, pitch = wp };
            double cx = spot.x - me.x, cz = spot.z - me.z, cl = JsMath.Hypot(cx, cz);
            if (cl > 0.6)
            {
                Vec3 fwd = MovementMath.WishDirection(0, 1, wy), right = MovementMath.WishDirection(1, 0, wy);
                Move.moveZ = (cx * fwd.x + cz * fwd.z) / cl;
                Move.moveX = (cx * right.x + cz * right.z) / cl;
                bool sprint = cl > 4;
                Move.sprint = new Button { held = sprint, pressed = sprint && step % 60 == 0 };
            }
            int hopEvery = MovementMath.StepsFor(hopSeconds, dt);
            bool hop = sim.mover.grounded && step % hopEvery < 4;
            bool hopPress = hop && step % hopEvery == 0;
            // the sweep: jump while the bar is live (a human sees the red band in the tell); knocked into the lava: run for the deck's middle and jump out
            bool inLava = me.y < l.centre.y - 0.3;
            if (l.SweepLive || inLava)
            {
                hop = sim.mover.grounded;
                hopPress = hop && step % 4 == 0;
            }
            if (inLava)
            {
                double ix = l.centre.x - me.x, iz = l.centre.z - me.z, il = JsMath.Hypot(ix, iz);
                Vec3 fwd = MovementMath.WishDirection(0, 1, wy), right = MovementMath.WishDirection(1, 0, wy);
                if (il > 1e-6) { Move.moveZ = (ix * fwd.x + iz * fwd.z) / il; Move.moveX = (ix * right.x + iz * right.z) / il; }
                lavaSteps++;
            }
            Move.jump = new Button { held = hop, pressed = hopPress };
            if (hopPress) hops++;
            // the weapon
            double reach = sim.cfg.Num("weapons.gauntlet.range");
            if (pistolFist) gun = pick >= 0 && best <= reach && sim.loadout.owned.Contains("gauntlet") ? "gauntlet" : "revolver";
            else
            {
                double heat = sim.cfg.Num("weapons.nailgun.heatTime"), lockT = sim.cfg.Num("weapons.nailgun.lockTime");
                int cycle = MovementMath.StepsFor(heat + lockT, dt), hot = MovementMath.StepsFor(heat, dt);
                gun = fireSteps % cycle < hot ? "nailgun" : "shotgun";
            }
            if (!sim.loadout.owned.Contains(gun)) gun = "revolver";
            int gi = sim.loadout.owned.IndexOf(gun);
            bool fire = pick >= 0 && l.Hurtable;
            if (fire) fireSteps++;
            bool held, pressed;
            if (gun == "nailgun") { held = fire; pressed = fire && step % 4 == 0; }
            else if (gun == "gauntlet") { held = fire && step % 12 < 4; pressed = fire && step % 12 == 0; }
            else { held = fire && (step / 2) % 2 == 0; pressed = fire && step % 4 == 0; }
            Combat = new CombatInput { weapon = sim.loadout.Weapon != gun ? gi : -1, fire = new Button { held = held, pressed = pressed } };
        }
    }
}
