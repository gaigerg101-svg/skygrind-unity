// NIGHT32 builder C (design.n32.boss5; DESIGN32 C): THE CROWN, level 5's boss, in the sim. Implements the lead's declaration-only partials N32CrownStartRun / N32CrownStep
// (SurvivalSim.N32.cs). Everything here acts only while the run's n32.boss5 block is on; with it off nothing runs (no boss, no layer, no parry hook: --off32 is NIGHT31's game).
// Level 5's boss room (F's plan: room v9, plan boss id "crown") starts the fight through Crown32Begin: the lead routes BossRoomBegin("crown") / BossRoomSystem("crown") here with
// Tools/night/n32-crown-wire.cjs. The ring is its own world layer "crown32Room" at design.n32.boss5.room.origin (like THE LANTERN's courtyard), refilled when the drop ring changes;
// the beat-2 grapple rings join the level's grapple nodes (map.grappleNodes, as THE LOOM's joints) while they are up; the crown's orbs join NIGHT31's boss parry (loadout.bossParry31,
// chained in front of the sim's own). No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        public Crown32 crown32;
        public Crown32Arena crown32Arena;
        public Vec3 crown32Origin;
        readonly BoxLayer crown32Layer = new BoxLayer();
        int crown32LaidMask = -1;
        int crown32NodeStart = -1;
        /// <summary>The damage the crown's parried orbs sent back this process (the harness reads it).</summary>
        public double crown32ParryDamage;

        partial void N32CrownStartRun()
        {
            if (crown32 != null)
            {
                crown32.Clear();
                enemies.all.Remove(crown32.body);
                world.SetLayer("crown32Room", null);
                crown32LaidMask = -1;
            }
            Crown32NodesRemove();
            // the crown's orbs go back with the gauntlet's parry (NIGHT31 ltAlways): in front of the sim's own hook, which N31WeaponsStartRun set just before (N31StartRun runs first)
            if (n32.boss5 && loadout.bossParry31 != null)
            {
                System.Func<Vec3, double, bool> prev = loadout.bossParry31;
                loadout.bossParry31 = (from, reach) => Crown32Parry(from, reach) || prev(from, reach);
            }
        }

        /// <summary>The player's spawn in the ring (spawnRadius m south of the centre on the +z side, facing the centre) and its yaw.</summary>
        public Vec3 Crown32Spawn => crown32Arena == null ? default : crown32Origin + new Vec3(0, 0.05, crown32Arena.p.spawnRadius);
        public double Crown32SpawnYawDeg => 0; // yaw 0 looks along -z: from +z at the centre

        /// <summary>Start THE CROWN (or, with a live one, a retry: the bar back to its beat's start). false: the boss5 block is off or no BossKit exists yet. Builds the ring's layer.</summary>
        public bool Crown32Begin(out Vec3 spawn, out double yawDeg)
        {
            spawn = default;
            yawDeg = 0;
            if (!n32.boss5 || bossKit == null) return false;
            if (crown32Arena == null)
            {
                crown32Arena = Crown32Arena.Build(cfg);
                double[] o = cfg.Nums("design.n32.boss5.room.origin");
                crown32Origin = new Vec3(o[0], o[1], o[2]);
            }
            if (crown32 == null)
            {
                var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
                crown32 = new Crown32(cfg, Crown32Params.From(cfg), crown32Arena, events, target, enemies.NextId) { origin = crown32Origin };
                crown32.playerVelocity = () => mover.velocity;
                crown32.playerWallRiding = () => mover.state == MoveState.WallRide;
                double stand = cfg.Num("body.capsuleHeight"), low = cfg.Num("slide.colliderHeight");
                crown32.playerHeight = () => mover.state == MoveState.Slide ? low : stand;
                // the NAILGUN's DETONATE as a plate answer: Projectiles.Detonate deals a detonate hit once per body per press (one shot id), so a boss full of nails took 14;
                // in the crown's ring every nail the press set off lands on the core as one "nailDetonate32" hit (Crown32.Filter: x kit.detonateMult)
                events.On<DetonateEvent>("nailDetonate", e =>
                {
                    if (!n32.boss5 || crown32 == null || !crown32.Hurtable || e.nails <= 1 || crown32Arena == null) return;
                    Vec3 l = mover.position - crown32Origin;
                    if (crown32Arena.WallDistance(l.x, l.z) < 0 || System.Math.Abs(l.y) > crown32Arena.p.height) return;
                    loadout.Damage(crown32.body, (e.nails - 1) * cfg.Num("weapons.specials.nailgun.detonateDamage"), false, "nailDetonate32", crown32.Centre, false, 1, -1, false, true);
                });
            }
            spawn = Crown32Spawn;
            yawDeg = Crown32SpawnYawDeg;
            if (crown32.Alive)
            {
                crown32.Retry();
                Crown32Lay(0);
                return true;
            }
            Crown32Lay(0);
            world.SetLayer("crown32Room", crown32Layer, true);
            crown32.NewRun(runSeed ^ (uint)cfg.Num("design.n32.boss5.room.salt"));
            bossKit.bosses["crown"] = crown32;
            if (!enemies.all.Contains(crown32.body)) enemies.all.Add(crown32.body);
            bossKit.Start("crown", 0, 0, null, crown32.p.name);
            return true;
        }

        /// <summary>Lay the ring with `mask` sectors down (only when it changed). A player standing in a pit that closes is lifted onto the risen floor first.</summary>
        void Crown32Lay(int mask)
        {
            if (mask == crown32LaidMask) return;
            int was = crown32LaidMask < 0 ? 0 : crown32LaidMask;
            Vec3 local = mover.position - crown32Origin;
            bool trapped = crown32LaidMask >= 0 && local.y < -0.02 && local.y > -crown32Arena.p.lavaDepth - 1.5 && crown32Arena.WallDistance(local.x, local.z) > 0 && !crown32Arena.InInner(local.x, local.z)
                && (was & ~mask & (1 << crown32Arena.SectorAt(local.x, local.z))) != 0;
            crown32Arena.Build(mask);
            crown32Arena.FillLayer(crown32Layer, crown32Origin);
            crown32LaidMask = mask;
            if (trapped) mover.position = new Vec3(mover.position.x, crown32Origin.y + 0.05, mover.position.z);
        }

        void Crown32NodesAdd()
        {
            if (crown32NodeStart >= 0 || crown32 == null) return;
            crown32NodeStart = map.grappleNodes.Count;
            for (int k = 0; k < crown32.p.ringCount; k++) map.grappleNodes.Add(crown32.RingAt(k));
        }

        void Crown32NodesRemove()
        {
            if (crown32NodeStart < 0 || crown32 == null) { crown32NodeStart = -1; return; }
            int n = crown32.p.ringCount;
            if (map.grappleNodes.Count >= crown32NodeStart + n) map.grappleNodes.RemoveRange(crown32NodeStart, n);
            crown32NodeStart = -1;
        }

        partial void N32CrownStep(double dt)
        {
            if (crown32 == null) return;
            if (!n32.boss5)
            {
                if (crown32.Alive) { crown32.Clear(); enemies.all.Remove(crown32.body); }
                Crown32NodesRemove();
                return;
            }
            if (!crown32.Alive)
            {
                enemies.all.Remove(crown32.body);
                Crown32NodesRemove();
                if (crown32LaidMask > 0) Crown32Lay(0); // the floor whole again after the kill
                return;
            }
            crown32.Step(dt);
            if (!BossOn) bossKit.Step(dt);
            Crown32Lay(crown32.downMask);
            if (crown32.RingsUp) Crown32NodesAdd();
            else Crown32NodesRemove();
        }

        /// <summary>NIGHT31's boss parry for the crown's orbs: the nearest live orb within `reach` of `from` is removed and the core takes design.n32.boss5.kit.parryDamage as a
        /// parry31 hit (Crown32.Filter takes it in full). True when one went back.</summary>
        bool Crown32Parry(Vec3 from, double reach)
        {
            if (!n32.boss5 || crown32 == null || !crown32.body.alive) return false;
            int idx = -1;
            double best = reach;
            for (int i = 0; i < crown32.orbs.Count; i++)
            {
                if (!crown32.orbs[i].alive) continue;
                double d = Vec3.DistanceTo(crown32.orbs[i].pos, from);
                if (d <= best) { best = d; idx = i; }
            }
            if (idx < 0) return false;
            Vec3 at = crown32.orbs[idx].pos;
            crown32.orbs[idx].alive = false;
            double dmg = crown32.p.orbParryDamage; // design.n32.boss5.kit.parryDamage: the return is a real plate answer (NIGHT31's x2 of a 12 hp orb was 24)
            crown32ParryDamage += dmg;
            loadout.Damage(crown32.body, dmg, false, Stag31.ParryWeapon, at);
            events.Emit("parry", new HitEvent { owner = "player", weapon = "gauntlet", kind = "crownOrb", enemy = crown32.body.id, damage = dmg, position = at });
            return true;
        }
    }
}
