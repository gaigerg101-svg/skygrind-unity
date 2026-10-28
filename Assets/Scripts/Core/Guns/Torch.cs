// NIGHT20 U3-34.9 row B5 (builder G, design.adds.b5): the TORCH, ported line for line from skygrind/src/combat/guns.js
// (p17-night6) with the web's comments, as a part of the loadout (guns.js's `kit` is the loadout here):
// - TORCH (fire): a short flame cone, hold to spray, heat-limited, BURN (config weapons.torch). Alt: a fire wall along the
//   ground ahead (weapons.specials.torch). Variant: a longer reach (variantReach). Sliding: a wider, hotter cone.
// Port notes:
// - guns.js flame() / torchAlt() / stepWalls() and the TORCH's part of step(): the heat runs whatever is in hand; the trigger
//   ticks a flame every weapons.torch.tick s (fireInterval: / the QUICK perk) while held and not locked.
// - kit.groupShot('torch') is GroupTorchShot (the flame ticks within style.hitShotGroup s share a shot id, as the nails do).
// - kit.peekAim() / kit.aimShot() are getAim() / the assist's Aim().
// - The walls are a small reused pool (no allocation per wall); a wall's ends snap down to the floor under them (the
//   collider's ray, within 6 m), as the web's octree ray does.
// - No orbs (`hit.type === 'orb'`): nothing hits an orb in this port.
// Behind design.adds.b5 (the run's b5On): off, the TORCH is not a port (Fires refuses it, the trigger does nothing).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>NIGHT20 B5: the TORCH's and the VORTEX's events, the web's names (guns.js).</summary>
    public static class G20Events
    {
        /// <summary>guns.js flame(): a flame tick (payload FlameEvent).</summary>
        public const string Flame = "flame";
        /// <summary>guns.js torchAlt(): a fire wall laid (payload FireWallEvent).</summary>
        public const string FireWall = "fireWall";
        /// <summary>projectiles.js openVortex(): an orb stops and starts pulling (payload VortexEvent; the web's name).</summary>
        public const string VortexOpen = "vortexOpen";
        /// <summary>projectiles.js burstVortex(): the burst (payload BlastEvent {owner, position, radius, hits}; the web's name).</summary>
        public const string VortexBurst = "vortexBurst";
        /// <summary>Port-only (the web counts it in stats.collapses, no event): LT tapped, your orbs collapse now (payload: the orbs, an int).</summary>
        public const string VortexCollapse = "vortexCollapse";
        /// <summary>Port-only (the web counts it in stats.repels, no event): LT held past repelHold, your open orbs repel (payload: the orbs, an int).</summary>
        public const string VortexRepel = "vortexRepel";
    }

    /// <summary>guns.js flame's payload: the cone's reach (m) and angle (deg), sliding, the variant's reach mult.</summary>
    public sealed class FlameEvent
    {
        public string owner = "player";
        public double range, coneDeg, reach = 1;
        public bool sliding;
    }

    /// <summary>guns.js torchAlt's payload: the wall's ends on the floor, its life (s) and half width (m).</summary>
    public sealed class FireWallEvent
    {
        public string owner = "player";
        public Vec3 a, b;
        public double life, width;
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT20 B5: the guns ported tonight (guns.js TORCH and VORTEX; design.adds.b5).</summary>
        public static bool IsPort20(string weapon) => weapon == "torch" || weapon == "vortex"; // the VORTEX: Vortex.cs (builder G2)

        /// <summary>NIGHT20 B5: the design's b5 row is on (the last loadout built read it): the ports join Fires.</summary>
        static bool portsFire20;

        bool b5On; // design.adds.b5 this run (Torch20Reset)

        /// <summary>The run's B5 row (the sim gates the wall and the box with it).</summary>
        public bool B5On => b5On;

        sealed class FireWall
        {
            public Vec3 a, b;
            public int left, tick, shot;
            public bool live;
        }

        HeatState torchHeat;
        NailgunParams torchParams;
        double tRange, tConeDeg, tTick, tDamage, tPointScale, tSlideConeMult, tSlideDamageMult;
        double twLength, twAhead, twWidth, twLife, twDps, twTick, twCooldown, twHeat, tVariantReach;
        int torchSteps; // steps until the next flame tick
        int torchAltCooldown; // steps (guns.js altCooldowns.torch)
        int torchGroupId;
        double torchGroupAt;
        bool torchGroup;
        readonly FireWall[] walls = new FireWall[4];
        readonly FlameEvent flameEvent = new FlameEvent();
        readonly FireWallEvent fireWallEvent = new FireWallEvent();
        readonly List<Enemy> wallTargets = new List<Enemy>(); // a tick's live bodies (a kill mid-tick can't shift the walk)
        public int torchWalls; // guns.js stats.walls

        /// <summary>The TORCH's heat 0..1 and its overheat lock (the HUD, the viewmodel's nozzle glow).</summary>
        public double TorchHeat => torchHeat != null ? torchHeat.heat : 0;
        public bool TorchLocked => torchHeat != null && torchHeat.@lock > 0;

        /// <summary>The web's TORCH numbers, read once (the constructor; nothing without them: the golden sets).</summary>
        void Torch20Init(GameConfig cfg)
        {
            portsFire20 = cfg.Design("adds.enabled") && cfg.Design("adds.b5.enabled") && !N20Blocks.ForceOff && !N19Blocks.ForceOff;
            for (int i = 0; i < walls.Length; i++) walls[i] = new FireWall();
            if (!cfg.Has("weapons.torch.heatTime")) return;
            torchHeat = new HeatState();
            torchParams = NailgunParams.From(cfg, "weapons.torch");
            string w = "weapons.torch.", s = "weapons.specials.torch.";
            tRange = cfg.Num(w + "range");
            tConeDeg = cfg.Num(w + "coneDeg");
            tTick = cfg.Num(w + "tick");
            tDamage = cfg.Num(w + "damage");
            tPointScale = cfg.Num(w + "pointScale");
            tSlideConeMult = cfg.Num(w + "slideConeMult");
            tSlideDamageMult = cfg.Num(w + "slideDamageMult");
            twLength = cfg.Num(s + "length");
            twAhead = cfg.Num(s + "ahead");
            twWidth = cfg.Num(s + "width");
            twLife = cfg.Num(s + "life");
            twDps = cfg.Num(s + "dps");
            twTick = cfg.Num(s + "tick");
            twCooldown = cfg.Num(s + "cooldown");
            twHeat = cfg.Num(s + "heat");
            tVariantReach = cfg.Num(s + "variantReach");
        }

        /// <summary>A new run (Reset): the row's bit, a cool TORCH, no walls.</summary>
        void Torch20Reset()
        {
            b5On = n20.Add(cfg, "b5") && torchHeat != null;
            if (torchHeat != null)
            {
                torchHeat.heat = 0;
                torchHeat.@lock = 0;
            }
            torchSteps = 0;
            torchAltCooldown = 0;
            torchGroup = false;
            torchWalls = 0;
            for (int i = 0; i < walls.Length; i++) walls[i].live = false;
        }

        /// <summary>guns.js step's TORCH part, whatever is in hand: its alt cooldown, its heat, the live walls.</summary>
        void StepTorch(CombatInput input, double dt)
        {
            if (torchAltCooldown > 0) torchAltCooldown--;
            bool held = ready && input.fire.held;
            bool firing = held && Weapon == "torch" && torchHeat.@lock <= 0;
            if (WeaponMath.HeatStep(torchHeat, firing, dt, torchParams)) events.Emit("overheat", "torch");
            StepWalls(dt);
            if (Weapon != "torch") torchSteps = 0;
        }

        /// <summary>The trigger (guns.js step: case 'torch'): a flame tick every weapons.torch.tick s while held and not locked.</summary>
        void FireTorchIfReady(double dt)
        {
            if (torchHeat.@lock > 0) return;
            if (torchSteps <= 0)
            {
                torchSteps = Math.Max(1, MovementMath.StepsFor(tTick * TuneInterval("torch") / fireRateMult, dt));
                Flame(dt);
            }
            torchSteps--;
        }

        /// <summary>weapons.js groupShot('torch'): the flame ticks within style.hitShotGroup s share a shot.</summary>
        void GroupTorchShot()
        {
            if (torchGroup && clock - torchGroupAt < hitShotGroup) curShot = torchGroupId;
            else
            {
                torchGroupId = NewShot();
                torchGroupAt = clock;
                torchGroup = true;
            }
        }

        /// <summary>One Torch tick: every body in the flame cone with a clear line burns.</summary>
        void Flame(double dt)
        {
            GroupTorchShot();
            MarkShot("torch");
            var (origin, dir) = getAim();
            assisted = assist != null && assist.Aim();
            bool sliding = mover.state == MoveState.Slide;
            double cone = tConeDeg * (sliding ? tSlideConeMult : 1);
            double damage = tDamage * (sliding ? tSlideDamageMult : 1);
            double reach = Variant19("torch") ? tVariantReach : 1;
            double range = tRange * reach * Torch43Reach(); // NIGHT43 W1 torch43: the reach's ramp-in (Core/Torch43.cs; off: x1 exactly)
            events.Emit("fire", "torch");
            flameEvent.owner = owner;
            flameEvent.range = range;
            flameEvent.coneDeg = cone;
            flameEvent.sliding = sliding;
            flameEvent.reach = reach;
            events.Emit(G20Events.Flame, flameEvent);
            int hits = 0;
            foreach (TargetPick pick in CombatMath.SweepTargets(origin, dir, enemies.PunchTargets(), range, cone * Math.PI / 180, found))
            {
                var enemy = (Enemy)pick.target.enemy;
                if (!HasLos(origin, enemy.AimPoint)) continue;
                Damage(enemy, damage, false, "torch", origin, assisted, tPointScale);
                if (torch43 != null) Torch43Ignite(enemy); // NIGHT43 W1 torch43: it burns (Core/Torch43.cs; off: nothing)
                hits++;
                if (enemy.alive && arsenal.enabled) arsenal.Status(enemy, "burn");
            }
            if (arsenal.enabled) arsenal.Use("torch");
            if (hits > 0) events.Emit("shotHit", "torch");
            events.Emit("shotResult", new ShotEvent { owner = owner, weapon = "torch", origin = origin, dir = dir, hitCount = hits, rayCount = 0 });
        }

        /// <summary>TORCH: a fire wall along the ground ahead (from `ahead` m, `length` m long).</summary>
        void TorchAlt(double dt)
        {
            if (torchAltCooldown > 0 || torchHeat.@lock > 0) return;
            var (_, aim) = getAim(); // kit.peekAim()
            double fx = aim.x, fz = aim.z;
            double len = JsMath.Hypot(fx, fz);
            if (len * len < 1e-6)
            {
                fx = 0;
                fz = -1;
                len = 1;
            }
            fx /= len;
            fz /= len;
            Vec3 feet = mover.position;
            var a = new Vec3(feet.x + fx * twAhead, feet.y, feet.z + fz * twAhead);
            var b = new Vec3(a.x + fx * twLength, a.y, a.z + fz * twLength);
            // Down onto the floor under each end.
            a = OntoFloor(a);
            b = OntoFloor(b);
            FireWall slot = walls[0];
            for (int i = 0; i < walls.Length; i++)
            {
                if (!walls[i].live)
                {
                    slot = walls[i];
                    break;
                }
                if (walls[i].left < slot.left) slot = walls[i]; // every slot live: the oldest gives way
            }
            slot.a = a;
            slot.b = b;
            slot.left = MovementMath.StepsFor(twLife, dt);
            slot.tick = 0;
            slot.shot = NewShot();
            slot.live = true;
            torchHeat.heat = JsMath.Min(1, torchHeat.heat + twHeat);
            torchAltCooldown = MovementMath.StepsFor(twCooldown, dt);
            torchWalls++;
            if (arsenal.enabled) arsenal.Use("torch");
            fireWallEvent.owner = owner;
            fireWallEvent.a = a;
            fireWallEvent.b = b;
            fireWallEvent.life = twLife;
            fireWallEvent.width = twWidth;
            events.Emit(G20Events.FireWall, fireWallEvent);
        }

        Vec3 OntoFloor(Vec3 p)
        {
            if (world.RayIntersect(new Vec3(p.x, p.y + 2, p.z), new Vec3(0, -1, 0), out RayHit hit) && hit.distance < 6) return new Vec3(p.x, p.y + 2 - hit.distance, p.z);
            return p;
        }

        /// <summary>Burn every body standing in a live fire wall (ticks).</summary>
        void StepWalls(double dt)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                FireWall wl = walls[i];
                if (!wl.live) continue;
                if (--wl.left <= 0)
                {
                    wl.live = false;
                    continue;
                }
                if (--wl.tick > 0) continue;
                wl.tick = MovementMath.StepsFor(twTick, dt);
                double tx = wl.b.x - wl.a.x, tz = wl.b.z - wl.a.z;
                double len = JsMath.Hypot(tx, wl.b.y - wl.a.y, tz);
                double d = len != 0 ? len : 1;
                tx /= d;
                tz /= d;
                wallTargets.Clear();
                for (int k = 0; k < enemies.all.Count; k++) if (enemies.all[k].alive) wallTargets.Add(enemies.all[k]);
                for (int k = 0; k < wallTargets.Count; k++)
                {
                    Enemy e = wallTargets[k];
                    if (!e.alive) continue;
                    // Distance from the body to the wall's segment, on the ground plane.
                    double px = e.position.x - wl.a.x;
                    double pz = e.position.z - wl.a.z;
                    double along = JsMath.Max(0, JsMath.Min(len, px * tx + pz * tz));
                    double dx = px - tx * along;
                    double dz = pz - tz * along;
                    if (JsMath.Hypot(dx, dz) > twWidth || Math.Abs(e.position.y - wl.a.y) > 2) continue;
                    Damage(e, twDps * twTick, false, "torch", wl.a, false, tPointScale, wl.shot);
                    if (e.alive && arsenal.enabled) arsenal.Status(e, "burn");
                }
            }
        }
    }
}
