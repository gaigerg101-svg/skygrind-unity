// NIGHT31 builder B (wyrm31): THE KILN WYRM's punch-and-slug pilot, shared by the harness (camp31 wyrm31) and the Player reel (FeelReel.N31.Bosses.cs wyrm31). NIGHT30's puncher
// (walks up inside the aggro radius, faces the head, swings the PULSE when the charging head's box is in reach) plus: the THUNDERCLAP at the head while the worm is staggered, the
// KESTREL otherwise; a strafe that swaps side every 2 s (never still for the spit); a jump late in the tail sweep's tell. Numbers are bot-side and change no rule.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class WyrmPilot31
    {
        public InputFrame Move;
        public CombatInput Combat;
        bool was, swung;
        int swingHold;
        public int swings, hops;

        public void Step(SurvivalSim sim, int step)
        {
            Wyrm w = sim.wyrm24;
            double reach = sim.cfg.Num("weapons.gauntlet.range");
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = w.boxes[0].c - eye;
            double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            bool charging = w.Charging30;
            if (charging && !was) swung = false;
            was = charging;
            var mv = new InputFrame { yaw = wy, pitch = wp, moveX = (step / 240) % 2 == 0 ? 0.6 : -0.6 };
            bool hop = sim.wyrm31 != null && sim.wyrm31.SweepTelling && sim.wyrm31.SweepTellProgress > 0.55 && sim.mover.grounded; // a human jumps the tail sweep late in its tell
            if (hop) { mv.jump = new Button { held = true, pressed = true }; hops++; }
            if (!charging && !w.Recoiling30 && d.Length > 9) mv.moveZ = 1;
            bool swing = charging && !swung && w.boxes[0].Touches(eye, reach);
            if (swing) { swung = true; swings++; swingHold = 4; }
            bool stag = sim.wyrm31 != null && sim.wyrm31.Staggered;
            string gun = swing || charging ? "gauntlet" : stag ? "shotgun" : "revolver";
            // frame-robust (see StagPilot31): the swing is held 4 steps from its press; the guns tap in pairs of steps
            bool swingHeld = swingHold > 0;
            if (swingHold > 0) swingHold--;
            bool shoot = !charging && !swingHeld;
            bool held = swingHeld || (shoot && (step / 2) % 2 == 0), pressed = swing || (shoot && step % 4 == 0);
            if (swingHeld) gun = "gauntlet";
            Move = mv;
            Combat = new CombatInput { weapon = sim.loadout.Weapon != gun ? sim.loadout.owned.IndexOf(gun) : -1, fire = new Button { held = held, pressed = pressed } };
        }
    }
}
