// NIGHT37 builder W (wick37; NIGHT37.md W1, BOSSES.md "L1 THE WICK"): THE STACK and THE WICK in the sim. Implements the lead's partials (SurvivalSim.N37.cs):
//   N37WickStartRun  a level whose campaign room row has `wick37` (n37-foundry: Tools/night/gen-n36-foundry.cjs --n37) gets THE STACK from level load:
//                    Wick37Stack as world layer "wick37Stack" at the row's centre (level geometry: it stands whatever the rows say, like NIGHT36's
//                    hall). With wick37 on the boss fights here (boss37WalkInHere).
//   N37WickStep      the stack's own rules with the row on: the HOT WALL (a ride or a touch on the shaft's wall throws the player off and burns: only
//                    the cyan panels hold), the SLAG (rises while THE WICK attacks, still while it rests, capped under the checkpoint ledge; a touch
//                    costs `damage` and bounces the player onto the checkpoint ledge), the checkpoint (the highest ledge stood on); then THE WICK.
//   N37WickBegin     the plan id "wick": starts THE WICK (Core/Enemies/Wick37.cs on K's Boss37 brain) where the player walked in; a retry restarts it.
//   N37WickSystem    BossRoomSystem("wick") is THE WICK.
// Off (wick37 off / --off37): nothing here runs but the stack's geometry on an n37 level (which only the row's level swap loads). No UnityEngine.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Wick37Stack wick37Stack;
        public Wick37Slag wick37Slag;
        public Wick37 wick37;
        /// <summary>The stack's centre (world, on the floor) on this level, or null when the level has no wick37 row.</summary>
        public Vec3? wick37Centre;
        /// <summary>The harness / reel puts the stack here on a map without the row (null: the level's row).</summary>
        public Vec3? wick37CentreOverride;
        readonly BoxLayer wick37Layer = new BoxLayer();
        bool wick37Owns, wick37Hooked;
        int wick37HotCool, wick37DashSteps = 100000;
        /// <summary>The checkpoint floor's height (arena-local): ledge 1 until a higher ledge (or the cap) is stood on.</summary>
        public double wick37CheckpointY;
        /// <summary>Counts for the harness and the reels.</summary>
        public int wick37HotBurns, wick37SlagTouches, wick37Begins, wick37Retries;
        readonly Wick37Event w37 = new Wick37Event();

        /// <summary>The stack's rules run (the row on and the stack on this level).</summary>
        public bool Wick37On => n37.wick37 && wick37Centre != null && wick37Stack != null;

        partial void N37WickStartRun()
        {
            if (!wick37Hooked)
            {
                wick37Hooked = true;
                events.On<MeleeEvent>("punch", OnPunch37Wick);
            }
            wick37?.Clear();
            if (wick37 != null) enemies.all.Remove(wick37.body);
            wick37Owns = false;
            wick37HotCool = 0;
            wick37HotBurns = wick37SlagTouches = wick37Begins = wick37Retries = 0;
            wick37Centre = Wick37RowCentre();
            if (wick37Centre == null)
            {
                world.SetLayer("wick37Stack", null);
                return;
            }
            Wick37Build();
            if (n37.wick37) boss37WalkInHere = true;
        }

        void Wick37Build()
        {
            wick37Stack ??= new Wick37Stack(Wick37StackParams.From(cfg));
            wick37Slag ??= new Wick37Slag(Wick37SlagParams.From(cfg));
            wick37Slag.Reset();
            wick37CheckpointY = wick37Stack.p.ledges.Length > 0 ? wick37Stack.p.ledges[0] : 0;
            wick37Slag.SetCheckpoint(wick37CheckpointY);
            wick37Stack.FillLayer(wick37Layer, wick37Centre.Value);
            world.SetLayer("wick37Stack", wick37Layer, true);
        }

        /// <summary>The reel's fresh start: THE WICK cleared (a campaign run may have begun it), the slag and the checkpoint back to the start.</summary>
        public void Wick37Reset()
        {
            if (wick37 != null) { wick37.Clear(); enemies.all.Remove(wick37.body); }
            wick37Owns = false;
            if (wick37Slag != null && wick37Stack != null)
            {
                wick37Slag.Reset();
                wick37CheckpointY = wick37Stack.p.ledges.Length > 0 ? wick37Stack.p.ledges[0] : 0;
                wick37Slag.SetCheckpoint(wick37CheckpointY);
            }
        }

        /// <summary>The harness's and the reel's way onto a map without the row: the stack at `centre` now, as a level row would put it at run start.</summary>
        public void Wick37Place(Vec3 centre)
        {
            wick37CentreOverride = centre;
            wick37Centre = centre;
            Wick37Build();
            if (n37.wick37) boss37WalkInHere = true;
        }

        /// <summary>The campaign room row's wick37.centre in the level JSON, or null.</summary>
        Vec3? Wick37RowCentre()
        {
            if (wick37CentreOverride.HasValue) return wick37CentreOverride;
            if (map?.json == null || !map.json.ContainsKey("campaign")) return null;
            var camp = PortData.Obj(map.json["campaign"]);
            if (!camp.ContainsKey("rooms")) return null;
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (!r.ContainsKey("wick37")) continue;
                var row = PortData.Obj(r["wick37"]);
                if (!row.ContainsKey("centre")) continue;
                return PortData.V3(row["centre"]);
            }
            return null;
        }

        /// <summary>Arena-local feet of the player.</summary>
        public Vec3 Wick37Local => wick37Centre == null ? default : mover.position - wick37Centre.Value;

        /// <summary>Is the player inside the shaft's footprint (or on the cap) now?</summary>
        public bool Wick37InStack
        {
            get
            {
                if (wick37Centre == null || wick37Stack == null) return false;
                Vec3 l = Wick37Local;
                double r = JsMath.Hypot(l.x, l.z);
                return r < wick37Stack.p.capRadius + 0.5 && l.y > wick37Slag.y - 3 && l.y < wick37Stack.p.capY + wick37Stack.p.capWallHeight + 6;
            }
        }

        partial void N37WickSystem(string id, ref IBossSystem boss)
        {
            if (id == "wick" && wick37Owns && wick37 != null) boss = wick37;
        }

        partial void N37WickBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg)
        {
            if (id != "wick" || !n37.wick37 || wick37Centre == null) return;
            handled = true;
            ok = Wick37Begin(out spawn, out yawDeg);
        }

        /// <summary>Start THE WICK in the stack (or, with a live one, retry the leg). spawn / yaw: the room's spawn (the tunnel) for the campaign's retry.</summary>
        public bool Wick37Begin(out Vec3 spawn, out double yawDeg)
        {
            spawn = default;
            yawDeg = 0;
            if (wick37Centre == null || wick37Stack == null) return false;
            Vec3 c = wick37Centre.Value;
            RoomDef room = campaign?.Room;
            spawn = room != null && room.kind == "boss" ? room.spawn : new Vec3(c.x, c.y, c.z + wick37Stack.p.radius + 2);
            yawDeg = room != null && room.kind == "boss" ? room.spawnYawDeg : 0;
            if (wick37 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                wick37 = new Wick37(cfg, Wick37Params.From(cfg), events, target, enemies.NextId);
                wick37.playerBody = () => mover.Body;
                wick37.playerVel = () => mover.velocity;
                wick37.playerRiding = () => mover.state == MoveState.WallRide;
                wick37.playerGrounded = () => mover.grounded;
                wick37.parryHit = (dmg, at) => loadout.Damage(wick37.body, dmg, false, Stag31.ParryWeapon, at);
            }
            wick37.origin = c;
            wick37.stack = wick37Stack;
            wick37.slag = wick37Slag;
            wick37.tells = tells36;
            bool retry = wick37.Alive && wick37Owns;
            if (retry)
            {
                wick37Retries++;
                Vec3 at = wick37.RestartLeg(health);
                spawn = at;
                wick37Begins++;
                Wick43Began(true); // NIGHT43 B1 (wick43; SurvivalSim.N43.Wick.cs): the retry's short reveal (nothing with the row off)
                return true;
            }
            wick37Begins++;
            wick37Owns = true;
            if (bossKit != null) bossKit.Clear();
            wick37.seed = runSeed * 2654435761u + 37u;
            Wick43Arm(); // NIGHT43 B1 (wick43): the fight takes the row's bit and numbers (off: wick37.n43 false, NIGHT42's fight)
            Wick47Arm(); // NIGHT47 W (wick47; SurvivalSim.N47.Wick.cs): the fight takes the row's bit, numbers and vents (off: wick37.w47 false)
            Wick54Arm(); // NIGHT54 W (wick54; SurvivalSim.N54.Wick.cs): no FLARE, the easy encounter's numbers (off: wick37.w54 false)
            Wick55Arm(); // NIGHT55 B (wick55; SurvivalSim.N55.Wick.cs): the window pays, the hot wall's touch (off: wick37.w55 false)
            wick37.StartFight(new BossFight { center = c, kit = bossKit, scale = BossScale.One });
            Wick43Began(false); // NIGHT43 B1 (wick43): the walk-in's reveal (nothing with the row off)
            wick37.ReachedFloor(wick37CheckpointY);
            if (bossKit != null) bossKit.bosses["wick"] = wick37;
            if (!enemies.all.Contains(wick37.body)) enemies.all.Add(wick37.body);
            return true;
        }

        void OnPunch37Wick(MeleeEvent e)
        {
            if (wick37 == null || !wick37.Alive || !wick37Owns || e.owner != "player") return;
            var (eye, dir) = Aim();
            bool launch = mover.state == MoveState.Dash || mover.velocity.Length > 14 || wick37DashSteps <= MovementMath.StepsFor(cfg.Num("design.n37.wick37.boss.dive.launchGrace"), cfg.Dt);
            wick37.Punch(eye, dir, launch);
        }

        partial void N37WickStep(double dt)
        {
            if (!Wick37On) return;
            wick37DashSteps = mover.state == MoveState.Dash ? 0 : wick37DashSteps + 1;
            Wick37Rules(dt);
            if (wick37 == null || !wick37Owns) return;
            if (wick37.Alive)
            {
                wick37.Step(dt);
                if (wick37.stalled && (mover.state == MoveState.Dash || wick37DashSteps <= MovementMath.StepsFor(cfg.Num("design.n37.wick37.boss.dive.launchGrace"), cfg.Dt))) wick37.LaunchContact(Eye);
                if (!BossOn && bossKit != null) bossKit.Step(dt);
            }
            if (!wick37.Alive) enemies.all.Remove(wick37.body);
        }

        /// <summary>The stack's rules: the checkpoint, the hot wall, the slag.</summary>
        void Wick37Rules(double dt)
        {
            Wick37Stack s = wick37Stack;
            Vec3 o = wick37Centre.Value, l = mover.position - o;
            double r = JsMath.Hypot(l.x, l.z), capR = cfg.movement.body.capsuleRadius;
            // the checkpoint: the highest ledge (or the cap) stood on
            if (mover.grounded && r > s.p.radius - s.p.ledgeWidth - capR - 0.05)
            {
                double f = s.FloorUnder(l.y);
                if (f > wick37CheckpointY + 0.5 && Math.Abs(l.y - f) < 0.3)
                {
                    wick37CheckpointY = f;
                    wick37Slag.SetCheckpoint(f);
                    w37.stage = "checkpoint";
                    w37.y = f;
                    w37.at = mover.position;
                    events.Emit("wick37Checkpoint", w37);
                    if (wick37 != null && wick37Owns && wick37.Alive) wick37.ReachedFloor(f);
                }
            }
            // THE HOT WALL: the shaft's wall (not a panel, a ledge or the cap) throws the player off
            if (wick37HotCool > 0) wick37HotCool--;
            double reach = cfg.Num("design.n37.wick37.hotWall.reach");
            bool onShaft = r > s.p.radius - capR - reach && r < s.p.radius + 1 && l.y < s.p.capY - 0.2 && l.y > -2;
            if (onShaft && s.NearestPanel(l, out double pd) >= 0 && pd < capR + 0.6) onShaft = false; // at a panel (its ends stand close to the wall): the panel holds
            if (onShaft && Wick47OverStep(l)) onShaft = false; // NIGHT47 W (wick47): over a step (SurvivalSim.N47.Wick.cs)
            if (onShaft && Wick48OverLip(l)) onShaft = false; // NIGHT48 W (climbTop48): over a lip (SurvivalSim.N48.Wick.cs; false with no lips)
            if (onShaft && !InEntryGap(l) && (mover.state == MoveState.WallRide || !mover.grounded))
            {
                double nx = -l.x / Math.Max(1e-6, r), nz = -l.z / Math.Max(1e-6, r), push = cfg.Num("design.n37.wick37.hotWall.push");
                Vec3 v = mover.velocity;
                double vIn = v.x * nx + v.z * nz;
                if (mover.state == MoveState.WallRide || vIn < push * 0.5)
                {
                    double add = push - Math.Min(push, Math.Max(0, vIn));
                    bool ride55 = mover.state == MoveState.WallRide; // NIGHT55 B (wick55; SurvivalSim.N55.Wick.cs): the touch was a ride (read before the throw-off)
                    if (mover.state == MoveState.WallRide) mover.Launch(Math.Min(v.y, 0), v.x + nx * add, v.z + nz * add); // a ride on it: thrown off
                    else mover.Impulse(nx * add, 0, nz * add); // a brush in the air: pushed in, the leap's rise kept
                    if (wick37HotCool == 0 && Wick55Burns(ride55)) // NIGHT55 B (wick55): a brush may only push (always burns with the row off)
                    {
                        wick37HotCool = MovementMath.StepsFor(Wick55HotCooldown(cfg.Num("design.n37.wick37.hotWall.cooldown")), cfg.Dt); // NIGHT55 B: wick55's cooldown (NIGHT54's off)
                        health.Damage(Wick55HotDamage(cfg.Num("design.n37.wick37.hotWall.damage")), "boss"); // NIGHT55 B: wick55's damage (NIGHT54's off)
                        wick37HotBurns++;
                        w37.stage = "hotWall";
                        w37.at = mover.position;
                        w37.y = l.y;
                        events.Emit("wick37HotWall", w37);
                    }
                }
            }
            // THE STACK'S AIR: thick, hot air bleeds horizontal speed over airMaxSpeed (inside the shaft, under the cap), so every leap of the climb
            // flies the same arc at every lap (a chained climb would speed up 2 m/s a jump-off to the 20 m/s chain cap and sink: harness wick37 climb)
            double airMax = cfg.Num("design.n37.wick37.air.maxSpeed");
            if (r < s.p.radius && l.y < s.p.capY - 0.3 && l.y > -1 && mover.state != MoveState.Dash && Wick43AirClamp(airMax, dt)) { } // NIGHT43 B1 (wick43, SurvivalSim.N43.Wick.cs): the air eased, never on a floor
            else if (r < s.p.radius && l.y < s.p.capY - 0.3 && l.y > -1 && mover.state != MoveState.Dash)
            {
                Vec3 v = mover.velocity;
                double hs = JsMath.Hypot(v.x, v.z);
                if (hs > airMax) mover.velocity = new Vec3(v.x * airMax / hs, v.y, v.z * airMax / hs);
            }
            // THE SLAG
            wick37Slag.rising = wick37 != null && wick37Owns && wick37.Alive && wick37.SlagRising;
            wick37Slag.Step(dt);
            if (r < s.p.radius + 0.5 && wick37Slag.Touch(l.y, MovementMath.StepsFor(wick37Slag.p.cooldown, cfg.Dt)))
            {
                wick37SlagTouches++;
                health.Damage(wick37Slag.p.damage, "boss");
                // round 3 (harness wick37 band: with the ledges as arcs, a bounce toward the wall landed off them and fell back in, 17 touches a
                // fight): the slag throws the player back to the leg's CHECKPOINT (D-6: a fall restarts the leg, the boss keeps its progress), then
                // a small pop up; the slag's 30 is the fall's cost
                Vec3 at;
                if (wick37 != null && wick37Owns && wick37.Alive) at = wick37.RestartLeg(null);
                else
                {
                    int li = 0;
                    for (int k = 0; k < s.p.ledges.Length; k++) if (Math.Abs(s.p.ledges[k] - wick37CheckpointY) < 0.05) li = k;
                    double ca = s.ledgeAngle.Length > li ? s.ledgeAngle[li] : Math.PI / 2, rl = s.p.radius - s.p.ledgeWidth * 0.5;
                    at = o + new Vec3(JsMath.Cos(ca) * rl, wick37CheckpointY, JsMath.Sin(ca) * rl);
                }
                Respawn(new Vec3(at.x, at.y + 0.05, at.z), 0);
                mover.Launch(wick37Slag.p.bounceOut, null, null);
                w37.stage = "slag";
                w37.at = mover.position;
                w37.y = wick37Slag.y;
                events.Emit("wick37Slag", w37);
                if (wick37 != null && wick37Owns && wick37.Alive) wick37.SlagTouched();
            }
        }

        bool InEntryGap(Vec3 l) => l.z > 0 && Math.Abs(l.x) < wick37Stack.GapHalf + 0.2 && l.y < wick37Stack.p.gapHeight;
    }

    /// <summary>The payload of wick37Checkpoint / wick37HotWall / wick37Slag (pooled: read it, don't keep it).</summary>
    public sealed class Wick37Event
    {
        public string stage;
        public double y;
        public Vec3 at;
    }
}
