// NIGHT36 builder B (walkin36 + boss36; NIGHT36.md B1): the boss framework in the sim. Implements the lead's partials (SurvivalSim.N36.cs):
//   N36BossStartRun  a level whose campaign room row has `lantern36` (builder L's n36 foundry; Design/levels/n36/lantern-hall.spec.md) gets THE
//                    LANTERN's hall from level load: the ring (Lantern36Arena) as world layer "lantern36Hall" at the row's centre, the entry gap
//                    open. It is level geometry (only an n36 level has the row), so it stands whatever the rows say; with walkin36 on the boss
//                    fights here (boss36WalkInHere: Campaign24 keeps the player where they walked in).
//   N36BossBegin     with boss36 on, the plan id "hulk" in that level starts Lantern36 (Core/Enemies/Lantern36.cs on Core/Boss36.cs): the
//                    slab closes lockSeconds later (the doors lock), the reveal runs, then the fight. A retry restarts it (after the 50 % stun:
//                    at the stun's bar, phase 2). Returns the room's spawn (inside the gap) for Campaign24's retry.
//   N36BossSystem    BossRoomSystem("hulk") is Lantern36 while it owns the fight (BossRoomDefeated sees its death).
//   N36BossStep      steps it; a punch (event "punch") is offered to its yellow orbs (the parry, with ULTRAKILL's early buffer); a returned
//                    orb lands as a parry31 hit (Lantern36.Filter takes it in full); the kill opens the slab again.
// Off (boss36 off / --off36): nothing here runs but the hall's geometry on an n36 level. No UnityEngine: SimHarness compiles this file.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Lantern36 lantern36;
        public Lantern36Arena lantern36Arena;
        /// <summary>The hall's centre (world, on the floor) on this level, or null when the level has no lantern36 row.</summary>
        public Vec3? lantern36Centre;
        readonly BoxLayer lantern36Layer = new BoxLayer();
        bool lantern36Slab, lantern36Owns, boss36Hooked;
        int lantern36SlabLeft, lantern36CardLeft;
        /// <summary>The fight step the name card showed at (the harness checks it; -1 none yet).</summary>
        public int lantern36CardStep = -1;
        /// <summary>Begins and retries of the fight this run (the harness prints them).</summary>
        public int lantern36Begins, lantern36Retries;
        /// <summary>The player's feet when the last begin ran (the harness checks no Respawn moved them).</summary>
        public Vec3 lantern36BeginAt;
        /// <summary>The harness puts the hall here on a map without the row (null: the level's row).</summary>
        public Vec3? lantern36CentreOverride;
        /// <summary>The entry slab is closed now (the view draws it red).</summary>
        public bool Lantern36SlabClosed => lantern36Slab;

        partial void N36BossStartRun()
        {
            if (!boss36Hooked)
            {
                boss36Hooked = true;
                events.On<MeleeEvent>("punch", OnPunch36);
            }
            lantern36?.Clear();
            if (lantern36 != null) enemies.all.Remove(lantern36.body);
            lantern36Owns = false;
            lantern36Slab = false;
            lantern36SlabLeft = lantern36CardLeft = 0;
            lantern36CardStep = -1;
            lantern36Begins = lantern36Retries = 0;
            lantern36Centre = Lantern36RowCentre();
            if (lantern36Centre == null)
            {
                world.SetLayer("lantern36Hall", null);
                return;
            }
            lantern36Arena ??= new Lantern36Arena(Lantern36ArenaParams.From(cfg));
            Lantern36Layer(false);
            boss36WalkInHere = true;
        }

        /// <summary>The reel's way onto a map without the row: the hall at `centre` now (the gap open), as a level row would put it at run start.</summary>
        public void Lantern36Place(Vec3 centre)
        {
            lantern36CentreOverride = centre;
            lantern36Centre = centre;
            lantern36Arena ??= new Lantern36Arena(Lantern36ArenaParams.From(cfg));
            Lantern36Layer(false);
            boss36WalkInHere = true;
        }

        void Lantern36Layer(bool slab)
        {
            lantern36Slab = slab;
            lantern36Arena.FillLayer(lantern36Layer, lantern36Centre.Value, slab);
            world.SetLayer("lantern36Hall", lantern36Layer, true);
        }

        /// <summary>The campaign room row's lantern36.centre in the level JSON (builder L), or null.</summary>
        Vec3? Lantern36RowCentre()
        {
            if (lantern36CentreOverride.HasValue) return lantern36CentreOverride;
            if (map?.json == null || !map.json.ContainsKey("campaign")) return null;
            var camp = PortData.Obj(map.json["campaign"]);
            if (!camp.ContainsKey("rooms")) return null;
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (!r.ContainsKey("lantern36")) continue;
                var row = PortData.Obj(r["lantern36"]);
                if (!row.ContainsKey("centre")) continue;
                return PortData.V3(row["centre"]);
            }
            return null;
        }

        partial void N36BossSystem(string id, ref IBossSystem boss)
        {
            if (id == "hulk" && lantern36Owns && lantern36 != null) boss = lantern36;
        }

        partial void N36BossBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg)
        {
            if (id != "hulk" || !n36.boss36 || lantern36Centre == null || bossKit == null) return;
            handled = true;
            ok = Lantern36Begin(out spawn, out yawDeg);
        }

        /// <summary>Start THE LANTERN in its hall (or, with a live one, retry). spawn / yaw: the room's spawn (the retry point inside the gap).</summary>
        public bool Lantern36Begin(out Vec3 spawn, out double yawDeg)
        {
            spawn = default;
            yawDeg = 0;
            if (lantern36Centre == null || lantern36Arena == null || bossKit == null) return false;
            Vec3 c = lantern36Centre.Value;
            RoomDef room = campaign?.Room;
            spawn = room != null && room.kind == "boss" ? room.spawn : new Vec3(c.x, c.y, c.z + lantern36Arena.p.apothem - 3);
            yawDeg = room != null && room.kind == "boss" ? room.spawnYawDeg : 0;
            if (bossKit == null || lantern36Arena == null) return false;
            if (lantern36 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                lantern36 = new Lantern36(cfg, Lantern36Params.From(cfg), Boss36Params.From(cfg), events, target, enemies.NextId);
                lantern36.playerBody = () => mover.Body;
                lantern36.playerRiding = () => mover.state == MoveState.WallRide;
                lantern36.parryHit = (dmg, at) => loadout.Damage(lantern36.body, dmg, false, Stag31.ParryWeapon, at);
            }
            lantern36.origin = c;
            lantern36.brain.tells = tells36; // builder P's registry (Raise does nothing with tells36 off; the brain keeps its own timing)
            lantern36.arenaApothem = lantern36Arena.p.apothem;
            bool retry = lantern36.Alive && lantern36Owns;
            bool phase2 = retry && lantern36.brain.phase >= 2;
            if (retry) lantern36Retries++;
            lantern36Begins++;
            lantern36BeginAt = mover.position;
            bossKit.bosses["lantern36"] = lantern36;
            lantern36Owns = true;
            // the reveal starts now; the name card (BossKit.Start's bossIntro and the HUD bar) comes walkin36.cardSeconds later (hall spec step 4)
            bossKit.Clear();
            lantern36.resume = false;
            lantern36.StartFight(new BossFight { center = c, kit = bossKit, scale = BossMath.Scale("lantern36", 0, 0, null, bossKit.k) });
            lantern36CardLeft = Math.Max(1, MovementMath.StepsFor(cfg.Num("design.n36.walkin36.cardSeconds"), cfg.Dt));
            if (phase2)
            {
                // a retry after the stun: back to the stun's bar, phase 2 (no second stun)
                lantern36.body.hp = lantern36.body.maxHp * lantern36.brain.p.stunAt;
                lantern36.brain.stunDone = true;
                lantern36.brain.phase = 2;
            }
            if (!enemies.all.Contains(lantern36.body)) enemies.all.Add(lantern36.body);
            lantern36SlabLeft = Math.Max(1, MovementMath.StepsFor(cfg.Num("design.n36.walkin36.lockSeconds"), cfg.Dt));
            events.Emit("boss36Lock", new Lantern36Event { stage = "lock", at = c });
            events.Emit("boss36Reveal", new Lantern36Event { stage = "reveal", at = c });
            return true;
        }

        void OnPunch36(MeleeEvent e)
        {
            if (lantern36 == null || !lantern36.Alive || !lantern36Owns || e.owner != "player") return;
            var (eye, dir) = Aim();
            lantern36.Punch(eye, dir);
        }

        partial void N36BossStep(double dt)
        {
            if (lantern36Centre == null || lantern36 == null || !lantern36Owns) return;
            if (lantern36SlabLeft > 0 && --lantern36SlabLeft == 0 && lantern36.Alive && !lantern36Slab) Lantern36Layer(true);
            if (lantern36CardLeft > 0 && --lantern36CardLeft == 0 && lantern36.Alive)
            {
                lantern36.resume = true; // BossKit.Start's Clear and StartFight keep the fight under way: only the card and the bar start
                bossKit.Start("lantern36", 0, 0, null, lantern36.p.name);
                lantern36.resume = false;
                lantern36CardStep = lantern36.stepNo;
            }
            if (lantern36.Alive)
            {
                lantern36.Step(dt);
                if (!BossOn) bossKit.Step(dt);
            }
            if (!lantern36.Alive)
            {
                enemies.all.Remove(lantern36.body);
                if (lantern36Slab) Lantern36Layer(false); // the kill unlocks the door
            }
        }
    }

    /// <summary>NIGHT36 B: a scripted player for THE LANTERN's hall (the harness's band and the Player reel boss36). LanternPilot30's rider on the
    /// 64-facet ring: runs near the wall and jumps into rides (the Mover's own rule), chains rides while the floor tells or burns, drops 4 m in from
    /// the wall when a sweep beam is within reach, steps away from a camp decal, PUNCHES a yellow orb that comes within reach (the parry), and fires
    /// the KESTREL whenever its ray lands on the core. Bot-side numbers only; no rule changes.</summary>
    public sealed class Lantern36Pilot
    {
        public InputFrame Move;
        public CombatInput Combat;
        public int rides, jumps, punches, dodges;
        double yawL, pitchL;
        int stepNo = -1, jumpHold, jumpCool, dirSign = 1, rideSteps, punchCool, dodgeLeft;
        bool started;
        MoveState prev = MoveState.Ground;
        const double AimRate = 9.0, RunLane = 4.0, MinSpeed = 6.5;
        const int ChainAt = 120;
        public Mulberry32 hand;
        double jumpOff = 2.1, aimRate = AimRate, stuckX, stuckZ;
        int unstick;
        /// <summary>Off: the pilot never punches (the harness's no-parry control).</summary>
        public bool parry = true;
        /// <summary>Off: the pilot never fires (the harness's tells run: the fight's whole cycle, the bar moved by the harness).</summary>
        public bool shoot = true;
        /// <summary>On: the pilot keeps riding through the fan and parries from the wall (looks at the nearest orb, punches it in reach); off: it plants on
        /// the floor first (the round-1 pilot).</summary>
        public bool wallParry = true;

        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            Lantern36 l = sim.lantern36;
            Lantern36Arena a = sim.lantern36Arena;
            double dt = sim.step;
            if (!started) { started = true; yawL = yawNow; pitchL = 0; }
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 o = l.origin;
            double dx = me.x - o.x, dz = me.z - o.z;
            int f = a.FacetAt(dx, dz);
            Vec3 n = a.FacetNormal(f), tg = new Vec3(n.z, 0, -n.x) * dirSign;
            double nx = n.x, nz = n.z, tx = tg.x, tz = tg.z, off = a.WallDistance(dx, dz);
            double vAlong = sim.mover.velocity.x * tx + sim.mover.velocity.z * tz;
            MoveState st = sim.mover.state;
            if (st == MoveState.WallRide && prev != MoveState.WallRide) { rides++; rideSteps = 0; }
            prev = st;
            if (jumpCool > 0) jumpCool--;
            if (punchCool > 0) punchCool--;
            bool floorDanger = l.brain.attack == Lantern36.Floor && (l.brain.state == Boss36State.Tell || l.brain.state == Boss36State.Active);
            // a sweep beam within 60 deg: get 4 m in from the wall (wallBand 3)
            bool sweep = false;
            if (l.brain.attack == Lantern36.Sweep && (l.brain.state == Boss36State.Active || (l.brain.state == Boss36State.Tell && l.brain.Progress > 0.7)))
            {
                double ang = JsMath.Atan2(dz, dx);
                for (int k = 0; k < l.Beams; k++)
                {
                    double d = ang - (l.sweepAngle + k * 2 * Math.PI / l.Beams);
                    while (d > Math.PI) d -= 2 * Math.PI;
                    while (d < -Math.PI) d += 2 * Math.PI;
                    if (Math.Abs(d) < 60 * Math.PI / 180) sweep = true;
                }
            }
            if (sweep && dodgeLeft == 0 && off < 4.5) { dodgeLeft = 60; dodges++; }
            bool camp = l.brain.attack == Lantern36.Camp && l.brain.state == Boss36State.Tell;
            double wx, wz;
            bool jump = false;
            if (dodgeLeft > 0)
            {
                dodgeLeft--;
                wx = nx; wz = nz; // straight in from the wall
                if (st == MoveState.WallRide && jumpCool == 0) { jump = true; jumpCool = 30; }
            }
            else if (st == MoveState.WallRide)
            {
                wx = tx + (-nx) * 0.25;
                wz = tz + (-nz) * 0.25;
                rideSteps++;
                if (floorDanger && rideSteps >= ChainAt && jumpCool == 0) { jump = true; jumpCool = 30; }
            }
            else if (!sim.mover.grounded)
            {
                wx = tx * 0.6 + (-nx) * 0.9;
                wz = tz * 0.6 + (-nz) * 0.9;
            }
            else
            {
                bool ready = vAlong > MinSpeed && jumpCool == 0 && !sweep;
                if (ready)
                {
                    wx = tx * 0.85 - nx * 0.55;
                    wz = tz * 0.85 - nz * 0.55;
                    double vn = -(sim.mover.velocity.x * nx + sim.mover.velocity.z * nz);
                    if (off < 1.1 && vn < 4.0) jumpCool = 40;
                    else if (off < jumpOff && vn > 4.0)
                    {
                        jump = true;
                        jumpCool = floorDanger ? 45 : 150;
                        jumps++;
                        if (hand != null) { jumpOff = 1.95 + hand.Next() * 0.3; aimRate = AimRate * (0.75 + hand.Next() * 0.5); }
                    }
                }
                else
                {
                    wx = tx + nx * (off < RunLane ? 0.6 : 0) - nx * (off > RunLane + 1.5 ? 0.4 : 0);
                    wz = tz + nz * (off < RunLane ? 0.6 : 0) - nz * (off > RunLane + 1.5 ? 0.4 : 0);
                }
            }
            if (camp) { wx = tx; wz = tz; } // keep moving along: the decal lands where he camped
            // the fan: a parrying pilot steps off the wall and plants itself to meet the orbs (the fight's lesson: PARRY the yellow ones)
            bool fan = parry && l.brain.attack == Lantern36.Fan && (l.brain.state == Boss36State.Tell || l.brain.state == Boss36State.Active);
            if (fan && !wallParry && dodgeLeft == 0 && unstick == 0)
            {
                if (st == MoveState.WallRide) { wx = nx; wz = nz; if (jumpCool == 0) { jump = true; jumpCool = 30; } }
                else if (off < 3) { wx = nx; wz = nz; jump = false; }
                else { wx = wz = 0; jump = false; }
            }
            if (tick % 60 == 0)
            {
                if (Math.Abs(me.x - stuckX) + Math.Abs(me.z - stuckZ) < 2.0 && sim.mover.grounded) { dirSign = -dirSign; unstick = 50; }
                stuckX = me.x; stuckZ = me.z;
            }
            if (unstick > 0)
            {
                unstick--;
                double ul = Math.Max(1e-6, JsMath.Hypot(dx, dz));
                wx = -dx / ul; wz = -dz / ul;
                jump = false;
            }
            // aim: the nearest incoming yellow orb within 6 m (to punch it), else the core
            Vec3 aim = l.Centre;
            Lantern36Orb near = null;
            double nearD = 6;
            foreach (Lantern36Orb ob in l.orbs)
            {
                if (!ob.alive || ob.back) continue;
                double d = Vec3.DistanceTo(ob.pos, eye);
                if (d < nearD) { nearD = d; near = ob; }
            }
            if (parry && near != null) aim = near.pos;
            Vec3 dv = aim - eye;
            double wantYaw = JsMath.Atan2(-dv.x, -dv.z), wantPitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
            double stp = aimRate * dt * (near != null ? 2 : 1), dy = wantYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, wantPitch - pitchL));
            Vec3 fw = MovementMath.WishDirection(0, 1, yawL);
            double cp = JsMath.Cos(pitchL);
            Vec3 dir = new Vec3(fw.x * cp, JsMath.Sin(pitchL), fw.z * cp);
            double t = l.Raycast(eye, dir);
            bool fanNow = parry && l.brain.attack == Lantern36.Fan && (l.brain.state == Boss36State.Tell || l.brain.state == Boss36State.Active);
            bool fire = shoot && near == null && !fanNow && t >= 0 && !(sim.world.RayIntersect(eye, dir, out RayHit wh) && wh.distance < t);
            double ml = JsMath.Hypot(wx, wz);
            var fr = new InputFrame { yaw = yawL, pitch = pitchL };
            if (ml > 0.05)
            {
                wx /= Math.Max(1, ml);
                wz /= Math.Max(1, ml);
                Vec3 fwd = MovementMath.WishDirection(0, 1, yawL), right = MovementMath.WishDirection(1, 0, yawL);
                fr.moveZ = wx * fwd.x + wz * fwd.z;
                fr.moveX = wx * right.x + wz * right.z;
            }
            fr.sprint = new Button { held = true, pressed = tick % 60 == 0 };
            if (jump) { fr.jump = new Button { held = true, pressed = true }; jumpHold = 20; }
            else if (jumpHold > 0) { fr.jump = new Button { held = true }; jumpHold--; }
            Move = fr;
            int gun = sim.loadout.owned.IndexOf("revolver");
            Combat = new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1 };
            if (fire) Combat.fire = new Button { held = (tick / 2) % 2 == 0, pressed = tick % 4 == 0 };
            if (parry && near != null && nearD < 2.6 && punchCool == 0) { Combat.punch = new Button { held = true, pressed = true }; punchCool = 12; punches++; }
        }
    }
}
