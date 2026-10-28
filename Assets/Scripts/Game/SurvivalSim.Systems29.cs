// NIGHT29 (builder S): the sim's half of the systems rows: design.n29.lockout (fire and melee cannot overlap; the rule is Core/Guns/Lockout29.cs),
// design.n29.shotgun (the THUNDERCLAP's slower interval), design.n29.tank (the level-2 tank's vulnerability) and design.n29.doorFree (the hold-RB
// dev toggle; the rule is Core/DoorFree29.cs). The hooks are the declaration-only ones in SurvivalSim.N29.cs (N29SystemsStartRun /
// N29SystemsStep). Every effect is gated on its n29 bit: with the bits off the loadout's fields keep their defaults (null / exactly 1) and
// NIGHT28's game runs bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT29 lockout: the object the loadout reads (created at the first run with the block on).</summary>
        public Lockout29 lockout29;

        /// <summary>NIGHT29 doorFree: the hold detector (created at the first run with the block on; null with it off).</summary>
        public DoorFree29 doorFree29;

        /// <summary>NIGHT29 doorFree: the right bumper (F8 on the keyboard) is held NOW. The game sets it every frame from the raw RB button / key, whatever the pad layout
        /// maps RB to (a press keeps doing its own job: punch / switch); the sim reads it once per fixed step. Ignored outside a campaign level.</summary>
        public bool doorFreeHeld;

        /// <summary>NIGHT29 doorFree: DOORS OPEN (the toggle is on): the HUD's corner tag reads this.</summary>
        public bool DoorFreeOn => doorFree29 != null && doorFree29.On;

        /// <summary>NIGHT29 doorFree: the hold's progress 0..1 once past the reveal time (the HUD's ring); 0 for a normal press.</summary>
        public double DoorFreeProgress => doorFree29 != null ? doorFree29.Progress : 0;

        /// <summary>NIGHT29 doorFree: the toggle was on at some point this level, so the run is UNRANKED: no rank letter, no level record, nothing saved (D-6).</summary>
        public bool Unranked29 => doorFree29 != null && doorFree29.Used;

        sealed class StandIn29
        {
            public Husk body;
            public bool strafe;
            public double zA, zB, dir = 1;
        }

        readonly System.Collections.Generic.List<StandIn29> standIns29 = new System.Collections.Generic.List<StandIn29>();

        /// <summary>NIGHT29 roomRules: one stand-in body (a husk that never thinks or attacks: Enemy.standIn29) at `at`; a strafing one walks between world z zA and zB at design.n29.roomRules.standInSpeed.</summary>
        void SpawnStandIn29(Vec3 at, bool strafe, double zA, double zB)
        {
            Husk h = husks.Spawn("husk", at, mover.position);
            if (h == null) return;
            h.standIn29 = true;
            standIns29.RemoveAll(s => s.body == h); // NIGHT31 (builder L's find): a reused husk body drops its old stand-in record (f5's strafer took over f8's pierce stand-in, so f8 never cleared)
            standIns29.Add(new StandIn29 { body = h, strafe = strafe, zA = zA, zB = zB });
        }

        partial void N29SystemsStartRun()
        {
            standIns29.Clear();
            if (n29.lockout)
            {
                lockout29 ??= new Lockout29();
                lockout29.Reset();
                loadout.lockout29 = lockout29;
            }
            else
            {
                lockout29?.Reset();
                loadout.lockout29 = null;
            }
            husks.tankHp = n29.tank ? cfg.Num("design.n29.tank.hp") : 0; // the level-2 TANK: a husk body with its own hp (Husks.Spawn("tank")); its vulnerability row is design.n26.weaponIdentity.vulnerability.kind.tank
            // The THUNDERCLAP's interval: a further multiplier after the web value, NIGHT20's retune and NIGHT24's (a separate field, so the
            // NIGHT24 hook's own assignment cannot undo it).
            loadout.sgIntervalMult29 = n29.shotgun ? cfg.Num("design.n29.shotgun.fireIntervalMult") : 1;
            // DOORFREE resets with every level load (a run start): off, unused (D-6).
            if (n29.doorFree)
            {
                doorFree29 ??= DoorFree29.From(cfg);
                doorFree29.Reset();
            }
            else doorFree29 = null;
            doorFreeHeld = false;
        }

        partial void N29SystemsStep(double dt)
        {
            if (standIns29.Count > 0)
            {
                double speed = cfg.Num("design.n29.roomRules.standInSpeed");
                foreach (StandIn29 st in standIns29)
                {
                    if (!st.strafe || !st.body.alive || !st.body.standIn29) continue;
                    Vec3 p = st.body.position;
                    double lo = System.Math.Min(st.zA, st.zB), hi = System.Math.Max(st.zA, st.zB);
                    double z = p.z + st.dir * speed * dt;
                    if (z >= hi) { z = hi; st.dir = -1; } else if (z <= lo) { z = lo; st.dir = 1; }
                    st.body.position = new Vec3(p.x, p.y, z);
                }
            }
            if (doorFree29 == null) return;
            // campaign levels only; the Campaign24 object is made at the first live step (campaign == null before it)
            if (!campaignOn || campaign == null)
            {
                doorFree29.Update(false, dt);
                return;
            }
            if (doorFree29.Update(doorFreeHeld, dt)) campaign.SetDoorFree29(doorFree29.On); // the hold completed: every door opens / the locks come back
        }
    }
}
