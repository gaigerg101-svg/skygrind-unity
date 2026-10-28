// NIGHT37 builder S (stag37): THE STAG's fight in the sim (the canal itself: SurvivalSim.Stag37.cs). With design.n37.stag37 on:
//   N37StagBegin   the plan id "stag37" starts THE STAG (Core/Enemies/Stag37.cs on Boss37) in THE CANAL where the player walked in: the reveal,
//                  the slab closes walkin36.lockSeconds later, the name card walkin36.cardSeconds later. A retry (a death) restarts the leg (D-6).
//   N37StagSystem  BossRoomSystem("stag37") is the Stag.
//   the step       the Stag; THE SLIPSTREAM (after the glide: the player's skating speed pushed toward slip.base x (1 + gain) while InSlip); the
//                  punch (event "punch": the chunk parry, the charge parry, the window's launch, the leg punch); the lane-cut barriers (layer
//                  stag37Cuts); leg 2's FLOES (a floe in your lane ahead pulses RED floes.tell, drops for floes.down); the FALL (feet under
//                  fallY in the canal: the leg restarts at its checkpoint, fallCost HP); the slab.
//   THE PACER      2.9 SLIPSTREAM's cyan skater (the room row pacer37): it bolts when you cross its line moving; its slipstream pushes you as the
//                  Stag's does; a punch within catch.range catches it (event stag37Pacer).
// Off (the row off / --off37): nothing here runs. No UnityEngine: SimHarness compiles this file.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Stag37 stag37;
        bool stag37Owns, stag37PunchHooked;
        int stag37SlabLeft, stag37CardLeft, stag37FloeTimer;
        public int stag37CardStep = -1, stag37Begins, stag37Retries, stag37Falls, stag37FloeDrops;
        /// <summary>The live barriers (arena-local box, steps left).</summary>
        public readonly List<(Vec3 lo, Vec3 hi, int left)> stag37Cuts = new List<(Vec3, Vec3, int)>();
        /// <summary>Steps the slipstream pushed the player this run (the harness), the last push's speed after.</summary>
        public int stag37SlipPushes;
        /// <summary>The player is in a slipstream now (the Stag's or the pacer's): the view's cyan streak.</summary>
        public bool Stag37Slipping;
        /// <summary>The last punch's result ("leg", "launch", "chunk", "charge", "window", "pacer", "").</summary>
        public string stag37LastPunch = "";

        partial void N37StagSystem(string id, ref IBossSystem boss)
        {
            if (id == "stag37" && stag37Owns && stag37 != null) boss = stag37;
        }

        partial void N37StagBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg)
        {
            if (id != "stag37" || !n37.stag37 || stag37Centre == null || bossKit == null) return;
            handled = true;
            ok = Stag37Begin(out spawn, out yawDeg);
        }

        /// <summary>Start THE STAG in THE CANAL (or, with a live one, restart its leg). spawn / yaw: the room's spawn (Campaign24's retry point).</summary>
        public bool Stag37Begin(out Vec3 spawn, out double yawDeg)
        {
            spawn = default;
            yawDeg = 0;
            if (stag37Centre == null || stag37Arena == null || bossKit == null) return false;
            Stag37HookPunch();
            Vec3 c = stag37Centre.Value;
            RoomDef room = campaign?.Room;
            spawn = room != null && room.kind == "boss" ? room.spawn : stag37Arena.GapPoint + c + new Vec3(0, 0, -1.5);
            yawDeg = room != null && room.kind == "boss" ? room.spawnYawDeg : 0;
            if (stag37 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                stag37 = new Stag37(cfg, Stag37Params.From(cfg), stag37Arena, events, target, enemies.NextId, tells36);
                stag37.playerVelocity = () => mover.velocity;
                stag37.playerGrounded = () => mover.grounded;
                stag37.playerRiding = () => mover.state == MoveState.WallRide;
                stag37.playerSpeedVerb = () => mover.state == MoveState.Dash || mover.state == MoveState.Slide || mover.state == MoveState.WallRide;
                stag37.playerBody = () => mover.Body;
                stag37.parryHit = (dmg, at) => loadout.Damage(stag37.body, dmg, false, Stag31.ParryWeapon, at);
                stag37.dropBarrier = (lo, hi) => { stag37Cuts.Add((lo, hi, MovementMath.StepsFor(stag37.p.cutSeconds, cfg.Dt))); Stag37CutsLayer(); };
            }
            stag37.origin = c;
            stag37.brain.tells = tells36;
            stag37Begins++;
            if (stag37.Alive && stag37Owns)
            {
                // a retry (a death): the leg starts again (D-6), the Stag's progress kept
                stag37Retries++;
                Vec3 at = stag37.RestartLeg();
                spawn = at;
                yawDeg = Stag37YawAt(at);
                Stag37ResetFloes();
                return true;
            }
            stag37.seed = runSeed ^ (uint)cfg.Num("design.n37.stag37.salt");
            if (!loadout.owned.Contains("gauntlet")) loadout.AddWeapon("gauntlet"); // level 2's weapon: THE STAG is the PULSE GAUNTLET's fight (it is earned in i1a; a reel / harness start gets it here)
            bossKit.bosses["stag37"] = stag37;
            stag37Owns = true;
            bossKit.Clear();
            stag37.resume = false;
            stag37.StartFight(new BossFight { center = c, kit = bossKit, scale = BossMath.Scale("stag37", 0, 0, null, bossKit.k) });
            stag37CardLeft = Math.Max(1, MovementMath.StepsFor(cfg.Num("design.n36.walkin36.cardSeconds"), cfg.Dt));
            stag37SlabLeft = Math.Max(1, MovementMath.StepsFor(cfg.Num("design.n36.walkin36.lockSeconds"), cfg.Dt));
            stag37FloeTimer = 0;
            stag37Cuts.Clear();
            Stag37CutsLayer();
            Stag37ResetFloes();
            if (!enemies.all.Contains(stag37.body)) enemies.all.Add(stag37.body);
            events.Emit("boss36Lock", new Stag37Event { stage = "lock", at = c });
            events.Emit("boss36Reveal", new Stag37Event { stage = "reveal", at = stag37.Centre });
            return true;
        }

        /// <summary>The path's heading at a world point, as a yaw in degrees (the Respawn's).</summary>
        double Stag37YawAt(Vec3 w)
        {
            Vec3 l = w - stag37Centre.Value;
            stag37Arena.Project(l.x, l.z, out double s, out _);
            stag37Arena.PointAt(s, 0, out Vec3 h);
            return JsMath.Atan2(-h.x, -h.z) * 180 / Math.PI;
        }

        void Stag37HookPunch()
        {
            if (stag37PunchHooked) return;
            stag37PunchHooked = true;
            // the F punch (a gun in hand), and the PULSE GAUNTLET's swing (the gauntlet in hand: its fire; NIGHT30 rules block the F punch then)
            Action<MeleeEvent> hit = e =>
            {
                if (e.owner != "player") return;
                var (eye, dir) = Aim();
                if (stag37 != null && stag37.Alive && stag37Owns)
                {
                    stag37LastPunch = stag37.Punch(eye, dir);
                    // a blow that landed on it readies the gauntlet's next swing (the PULSE's 2.5 s heavy swing would outlast the 2.5 s window it opens:
                    // the Combat's own hook, ReadyPunch34, as the whip's latch uses it)
                    if (stag37LastPunch == "leg" || stag37LastPunch == "chunk" || stag37LastPunch == "window") loadout.ReadyPunch34(MovementMath.StepsFor(stag37.p.catchRebound, cfg.Dt));
                }
                else if (stag37Pacer != null) Stag37PacerPunch(eye);
            };
            events.On<MeleeEvent>("punch", hit);
            events.On<MeleeEvent>("meleeSwing", e => { if (e.weapon == "gauntlet") hit(e); });
        }

        void Stag37CutsLayer()
        {
            if (stag37Centre == null) return;
            Vec3 c = stag37Centre.Value;
            var list = new List<(Vec3, Vec3)>(stag37Cuts.Count);
            foreach (var (lo, hi, _) in stag37Cuts) list.Add((lo + c, hi + c));
            stag37CutLayer.SetBoxes(list);
            world.SetLayer("stag37Cuts", stag37CutLayer, true);
        }

        void Stag37ResetFloes()
        {
            if (stag37FloeDown == null) return;
            bool any = false;
            for (int i = 0; i < stag37FloeDown.Length; i++) { any |= stag37FloeDown[i]; stag37FloeDown[i] = false; stag37FloeTell[i] = 0; stag37FloeDownLeft[i] = 0; }
            if (any) Stag37FloesLayer();
        }

        partial void N37StagStepFight(double dt)
        {
            Stag37PacerStep(dt);
            if (stag37 == null || !stag37Owns) { Stag37Slipping = Stag37Slipping && stag37Pacer != null && stag37Pacer.slip; return; }
            if (stag37SlabLeft > 0 && --stag37SlabLeft == 0 && stag37.Alive && !Stag37SlabClosed) Stag37CanalLayer(true);
            if (stag37CardLeft > 0 && --stag37CardLeft == 0 && stag37.Alive)
            {
                stag37.resume = true;
                bossKit.Start("stag37", 0, 0, null, stag37.p.name);
                stag37.resume = false;
                stag37CardStep = stag37.stepNo;
            }
            if (stag37.Alive)
            {
                stag37.Step(dt);
                if (!BossOn) bossKit.Step(dt);
            }
            Stag37Slipping = stag37.Alive && stag37.InSlip;
            if (Stag37Slipping) Stag37Push(dt);
            // the barriers age out
            bool changed = false;
            for (int i = stag37Cuts.Count - 1; i >= 0; i--)
            {
                var b = stag37Cuts[i];
                if (--b.left <= 0 || !stag37.Alive) { stag37Cuts.RemoveAt(i); changed = true; }
                else stag37Cuts[i] = b;
            }
            if (changed) Stag37CutsLayer();
            Stag37Floes();
            // the fall into the canal (D-6)
            Vec3 feet = mover.position;
            if (stag37.Alive && Stag37InFootprint(feet) && feet.y < stag37Centre.Value.y + stag37.p.fallY)
            {
                stag37Falls++;
                Vec3 at = stag37.RestartLeg();
                health.Damage(stag37.p.fallCost, "fall");
                Respawn(at, Stag37YawAt(at));
                Stag37ResetFloes();
                events.Emit("stag37Fall", new Stag37Event { stage = "fall", at = at });
            }
            if (!stag37.Alive)
            {
                enemies.all.Remove(stag37.body);
                if (Stag37SlabClosed) Stag37CanalLayer(false); // the kill opens the gap
                Stag37ResetFloes();
                if (stag37Cuts.Count > 0) { stag37Cuts.Clear(); Stag37CutsLayer(); }
            }
        }

        /// <summary>THE SLIPSTREAM's push: a grounded skater's horizontal speed toward slip.base x (1 + gain) at slip.accel.</summary>
        void Stag37Push(double dt)
        {
            if (!mover.grounded || !OnIce30(mover.position)) return;
            Stag37Params sp = stag37 != null ? stag37.p : Stag37Params.From(cfg);
            Vec3 v = mover.velocity;
            double h = JsMath.Hypot(v.x, v.z), cap = sp.slipBase * (1 + sp.slipGain);
            if (h < 1 || h >= cap) return;
            double nh = Math.Min(cap, h + sp.slipAccel * dt), k = nh / h;
            mover.velocity = new Vec3(v.x * k, v.y, v.z * k);
            stag37SlipPushes++;
        }

        void Stag37Floes()
        {
            if (stag37FloeDown == null || stag37Arena == null) return;
            Stag37Arena a = stag37Arena;
            Stag37Params sp = stag37.p;
            Stag37FloeTellSteps = MovementMath.StepsFor(sp.floeTell, cfg.Dt);
            bool changed = false;
            for (int i = 0; i < stag37FloeDown.Length; i++)
            {
                if (stag37FloeTell[i] > 0 && --stag37FloeTell[i] == 0)
                {
                    stag37FloeDown[i] = true;
                    stag37FloeDownLeft[i] = MovementMath.StepsFor(sp.floeDown, cfg.Dt);
                    stag37FloeDrops++;
                    changed = true;
                    events.Emit("stag37Floe", new Stag37Event { stage = "drop", count = i, lane = a.floes[i].lane, at = (a.floes[i].min + a.floes[i].max) * 0.5 + stag37Centre.Value });
                }
                else if (stag37FloeDown[i] && --stag37FloeDownLeft[i] <= 0)
                {
                    stag37FloeDown[i] = false;
                    changed = true;
                }
            }
            if (changed) Stag37FloesLayer();
            // leg 2: every floes.every seconds, a floe in the player's lane ahead of him (while he is on or nearing straight B), else the floe the
            // Stag just ran off in its lane (it cracks the ice behind it while it runs B): the arena changes even before you reach B
            if (!stag37.Alive || stag37.brain.leg != 2 || !stag37.brain.Fighting) return;
            if (stag37FloeTimer > 0) { stag37FloeTimer--; return; }
            double ps = stag37.ps, need = Math.Max(sp.floeAhead, JsMath.Hypot(mover.velocity.x, mover.velocity.z) * sp.floeLead);
            double toB = a.Ahead(ps, a.SB);
            bool nearB = a.OnStraightB(ps) || (toB > 0 && toB < 45);
            int lane = stag37.playerLane, pick = -1;
            double best = 1e9;
            for (int i = 0; nearB && i < a.floes.Count; i++)
            {
                if (a.floes[i].lane != lane || stag37FloeDown[i] || stag37FloeTell[i] > 0) continue;
                double ahead = a.Ahead(ps, a.floes[i].s0);
                if (ahead >= need && ahead < best && Stag37CanLeave(a, ps, a.floes[i].s0)) { best = ahead; pick = i; }
            }
            if (pick < 0 && a.OnStraightB(stag37.s))
            {
                int sl = a.LaneOf(stag37.lat);
                for (int i = 0; i < a.floes.Count; i++)
                {
                    if (a.floes[i].lane != sl || stag37FloeDown[i] || stag37FloeTell[i] > 0) continue;
                    double behindStag = a.Ahead(a.floes[i].s1, stag37.s), ahead = a.Ahead(ps, a.floes[i].s0);
                    bool passed = a.Ahead(ps, a.floes[i].s1) < -1; // wholly behind the player (never one he stands on)
                    if (behindStag >= 0 && behindStag < 8 && (passed || (ahead >= need && Stag37CanLeave(a, ps, a.floes[i].s0)))) { pick = i; lane = sl; break; }
                }
            }
            if (pick < 0) return;
            stag37FloeTell[pick] = Stag37FloeTellSteps;
            stag37FloeTimer = MovementMath.StepsFor(sp.floeEvery, cfg.Dt);
            events.Emit("stag37Floe", new Stag37Event { stage = "tell", count = pick, lane = lane, at = (a.floes[pick].min + a.floes[pick].max) * 0.5 + stag37Centre.Value });
        }

        /// <summary>A player at path s can still leave his lane before a floe that starts at s0: there are at least 6 m of lane with no lane wall
        /// between him and the floe (a floe in a walled stretch you are already in would drop under a player who cannot change lanes).</summary>
        bool Stag37CanLeave(Stag37Arena a, double s, double s0)
        {
            double len = a.Ahead(s, s0);
            if (len < 0) return false;
            int open = 0;
            for (double d = 0; d <= len - 1; d += 1)
            {
                if (a.LaneWallAt(s + d, 0.5)) open = 0;
                else if (++open >= 6) return true;
            }
            return false;
        }

        // --- the pacer (room s2 2.9 SLIPSTREAM) ------------------------------------------------------------------------------------------------

        public sealed class Stag37PacerState
        {
            public Vec3 from, to, pos, prev;
            public double startZ;
            public int phase; // 0 waiting at the start, 1 running, 2 waiting at the end / caught
            public int waitLeft, catches, runs;
            public bool slip;
        }

        public Stag37PacerState stag37Pacer;

        partial void N37StagPacerStartRun()
        {
            stag37Pacer = null;
            if (!n37.stag37 || map?.json == null || !map.json.ContainsKey("campaign")) return;
            var camp = PortData.Obj(map.json["campaign"]);
            if (!camp.ContainsKey("rooms")) return;
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (!r.ContainsKey("pacer37")) continue;
                var row = PortData.Obj(r["pacer37"]);
                Vec3 from = PortData.V3(row["from"]), to = PortData.V3(row["to"]);
                double startLz = row.ContainsKey("startLz") ? PortData.Num(row["startLz"]) : 6;
                stag37Pacer = new Stag37PacerState { from = from, to = to, pos = from, prev = from, startZ = from.z + (10 - startLz) };
                return;
            }
        }

        /// <summary>The harness's way to place a pacer on any map.</summary>
        public void Stag37PacerPlace(Vec3 from, Vec3 to, double startZ)
        {
            stag37Pacer = new Stag37PacerState { from = from, to = to, pos = from, prev = from, startZ = startZ };
            Stag37HookPunch();
        }

        void Stag37PacerStep(double dt)
        {
            Stag37PacerState pc = stag37Pacer;
            if (pc == null) return;
            pc.prev = pc.pos;
            Stag37Params sp = stag37?.p ?? (stag37PacerParams ??= Stag37Params.From(cfg));
            Vec3 me = mover.position;
            Vec3 d = pc.to - pc.from;
            double len = Math.Max(1e-6, d.Length);
            Vec3 u = d / len;
            pc.slip = false;
            switch (pc.phase)
            {
                case 0:
                    if (me.z < pc.startZ && me.z > pc.to.z && JsMath.Hypot(mover.velocity.x, mover.velocity.z) >= sp.pacerStartSpeed && Math.Abs(me.x - pc.from.x) < 8)
                    {
                        pc.phase = 1;
                        pc.runs++;
                        events.Emit("stag37Pacer", new Stag37Event { stage = "bolt", at = pc.pos });
                    }
                    break;
                case 1:
                    {
                        pc.pos = pc.pos + u * (sp.pacerSpeed * dt);
                        if (Vec3.Dot(pc.pos - pc.from, u) >= len) { pc.pos = pc.to; pc.phase = 2; pc.waitLeft = MovementMath.StepsFor(sp.pacerWait, cfg.Dt); }
                        Vec3 rel = pc.pos - me;
                        double along = Vec3.Dot(rel, u), side = Math.Abs(rel.x * u.z - rel.z * u.x);
                        pc.slip = along > 0.3 && along <= sp.slipRange && side <= sp.slipLaneHalf && mover.grounded;
                        break;
                    }
                case 2:
                    if (--pc.waitLeft <= 0) { pc.phase = 0; pc.pos = pc.prev = pc.from; }
                    break;
            }
            if (pc.slip) { Stag37Slipping = true; Stag37Push(dt); }
            else if (stag37 == null || !stag37Owns) Stag37Slipping = false;
        }

        Stag37Params stag37PacerParams;

        void Stag37PacerPunch(Vec3 eye)
        {
            Stag37PacerState pc = stag37Pacer;
            if (pc == null || pc.phase != 1) return;
            Stag37Params sp = stag37?.p ?? (stag37PacerParams ??= Stag37Params.From(cfg));
            if (Vec3.DistanceTo(eye, pc.pos + new Vec3(0, 1.0, 0)) > sp.catchRange) return;
            pc.catches++;
            pc.phase = 2;
            pc.waitLeft = MovementMath.StepsFor(sp.pacerWait, cfg.Dt);
            stag37LastPunch = "pacer";
            events.Emit("stag37Pacer", new Stag37Event { stage = "caught", at = pc.pos, count = pc.catches });
        }
    }
}
