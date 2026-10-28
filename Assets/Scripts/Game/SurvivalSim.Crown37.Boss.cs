// NIGHT37 builder C (crown37): THE CROWN's fight in the sim (Core/Enemies/Crown37.cs on K's Boss37 brain; the pieces: SurvivalSim.Crown37.cs).
//   Crown37Begin    BossRoomBegin("crown37") on n37-spire with the row on: the Crown rises at the spire's summit where the player walked in
//                   (boss37WalkInHere: no Respawn), the leg checkpoints from the room's spire37 row, the TORCH handed over (crown.grantTorch),
//                   BossKit's card and bar. A retry (a death: Campaign24 calls it again) restarts the current leg (D-6: the boss's progress kept).
//   Crown37BossStep every step after the pieces: the fall rule (feet under the leg's floor: -fallDamage, back to the leg's checkpoint, the
//                   leg restarts), the Crown's step (the pass test on the chest's last step, the TORCH burning = the TORCH in hand with fire
//                   held within torchWindow), the punch offered to its yellow shards (the parry).
// No UnityEngine: SimHarness compiles this file.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Crown37 crown37;
        bool crown37Owns, crown37Hooked;
        int crown37TorchLeft;
        Boss37State crown37LegWas;
        /// <summary>The fight's falls this run (the harness, the reel) and begins / retries.</summary>
        public int crown37Falls, crown37Begins, crown37Retries, crown37LegDrops;

        void Crown37ClearBoss()
        {
            if (crown37 != null)
            {
                crown37.Clear();
                enemies.all.Remove(crown37.body);
            }
            crown37Owns = false;
            crown37Falls = crown37Begins = crown37Retries = crown37LegDrops = 0;
            crown37TorchLeft = 0;
        }

        bool Crown37Begin(out Vec3 spawn, out double yawDeg)
        {
            spawn = spire37Checkpoint[0];
            yawDeg = spire37CheckpointYaw[0];
            if (bossKit == null || orbit37 == null) return false;
            if (!crown37Hooked)
            {
                crown37Hooked = true;
                events.On<MeleeEvent>("punch", e =>
                {
                    if (crown37 == null || !crown37.Alive || !crown37Owns || e.owner != "player") return;
                    var (eye, dir) = Aim();
                    crown37.Punch(eye, dir);
                });
            }
            if (crown37 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                crown37 = new Crown37(cfg, Crown37Params.From(cfg), events, target, enemies.NextId, runSeed ^ 0x37C0u);
            }
            crown37.orbit = orbit37;
            crown37.axis = spire37Axis;
            crown37.plazaY = spire37Checkpoint[2].y;
            crown37.orbitCoreHalf = spire37Row.TryGetValue("coreHalf", out object ch) ? PortData.Num(ch) : 4;
            crown37.brain.tells = tells36;
            // NIGHT40 B5 (crown40): the ring's NIGHT40 rules (out of the core, in the room, no teleport, a fall's cost); null = NIGHT39's Crown
            if (n40.crown40)
            {
                var r40 = Crown40Params.From(cfg);
                if (!double.IsNaN(spire37Rect[0])) { r40.x0 = spire37Rect[0]; r40.z0 = spire37Rect[1]; r40.x1 = spire37Rect[2]; r40.z1 = spire37Rect[3]; }
                crown37.n40 = r40;
            }
            else crown37.n40 = null;
            crown37Begins++;
            if (crown37.Alive && crown37Owns)
            {
                // a retry (a death in the fight): the leg's checkpoint, the boss's progress kept (D-6)
                crown37Retries++;
                int leg = crown37.Leg;
                Vec3 at = crown37.FallRestart();
                spawn = at;
                yawDeg = spire37CheckpointYaw[leg - 1];
                return true;
            }
            for (int k = 0; k < 3; k++) crown37.brain.SetCheckpoint(k + 1, spire37Checkpoint[k], spire37CheckpointYaw[k]);
            if (crown37.p.grantTorch && !loadout.owned.Contains("torch")) loadout.AddWeapon("torch");
            bossKit.bosses["crown37"] = crown37;
            crown37Owns = true;
            Crown47Begin(); // NIGHT47 L (crown47; SurvivalSim.N47.Crown.cs): the new attacks, chip, window, the summit, the catch, in or out for this fight
            bossKit.Start("crown37", 0, 0, null, crown37.p.name);
            if (!enemies.all.Contains(crown37.body)) enemies.all.Add(crown37.body);
            return true;
        }

        void Crown37BossStep(double dt, Vec3 chest)
        {
            if (crown37 == null || !crown37Owns) return;
            if (!crown37.Alive)
            {
                enemies.all.Remove(crown37.body);
                return;
            }
            // the TORCH burning: in hand, fire held (the window keeps it lit a little either side of the pass)
            bool torchNow = loadout.Weapon == "torch" && lastCombat.fire.held && loadout.B5On && !loadout.TorchLocked;
            if (torchNow) crown37TorchLeft = Math.Max(1, MovementMath.StepsFor(crown37.p.torchWindow, step));
            else if (crown37TorchLeft > 0) crown37TorchLeft--;
            var inp = new Crown37Input
            {
                feet = mover.position, vel = mover.velocity, chestPrev = crown37HavePrev ? crown37PrevChest : chest, chest = chest,
                grounded = mover.grounded, riding = mover.state == MoveState.WallRide, torchBurning = crown37TorchLeft > 0,
            };
            crown37.Step(inp);
            if (!BossOn) bossKit.Step(dt);
            // a leg change ends with the player at the new leg's checkpoint unless he is already in the leg (the Crown dives; the fight goes down with it)
            if (crown37LegWas == Boss37State.LegChange && crown37.brain.state != Boss37State.LegChange && crown37.Alive)
            {
                int lg = Math.Max(1, Math.Min(3, crown37.Leg));
                double top = lg >= 2 ? spire37Floor[lg - 2] : 1e9;
                if (mover.position.y > top) { Respawn(spire37Checkpoint[lg - 1], spire37CheckpointYaw[lg - 1]); crown37LegDrops++; crown37HavePrev = false; }
            }
            crown37LegWas = crown37.brain.state;
            // the fall rule (BOSSES.md L5, D-6): feet under the leg's floor = back to the leg's checkpoint, -fallDamage, the leg restarts
            int leg = Math.Max(1, Math.Min(3, crown37.Leg));
            if (crown37.Alive && !crown37.Dying && mover.position.y < spire37Floor[leg - 1] && flow == Flow.Playing)
            {
                // a leg change drops the pieces under him: carried down to the new leg's checkpoint, no cost (he did not miss a jump)
                if (crown37.brain.state == Boss37State.LegChange) crown37LegDrops++;
                else { crown37Falls++; health.Damage(crown37.crown47 ? crown37.p47.fallHp : crown37.n40 != null ? crown37.n40.fallDamage : crown37.p.fallDamage, "fall"); } // NIGHT40 B5: crown40's fall cost; NIGHT47 L: crown47's
                Vec3 at = crown37.crown47 && crown37.brain.state != Boss37State.LegChange ? Crown47Catch(leg) : crown37.FallRestart(); // NIGHT47 L (crown47): caught, no leg restart
                Respawn(at, spire37CheckpointYaw[leg - 1]);
                crown37HavePrev = false;
            }
        }
    }
}
