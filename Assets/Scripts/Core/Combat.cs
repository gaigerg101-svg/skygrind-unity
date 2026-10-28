// The player's combat, ported from skygrind/src/combat: weapons.js (the loadout: slots in purchase order, the 1.5 swap
// timeline, the KESTREL's hitscan with headshots, hip spread and the zoomed ricochet, damage() and its hit / kill
// events), melee.js (the PULSE GAUNTLET's jab: nearest body in the cone with a clear line; a hit in a wind-up is a
// parry), punch.js (F: the fist, any gun, even mid-swap) and player/ads.js (right mouse zooms the Kestrel: no hip
// spread, x1.2 headshots, walk x0.85). Enemies live in EnemyRegistry; each kind gives its hit shapes.
// NIGHT9 U3-08: the arsenal (arsenal.js: FRESH, primed swaps, cross combos, statuses) is applied in damage() and fed
// where weapons.js / melee.js / punch.js feed it (use after a shot, swapIn at the raise, step before the swap).
// NIGHT10 U3-13.4: the ARMORY's STITCHER (nailgun: heat, nails, the detonate special, the pierce variant), COMET
// (rocket: rockets, the steer special, the fragments variant) and LONGWAVE (rail: charge, pierce, the overcharge
// special, the walls variant), as weapons.js fires them; nails and rockets fly in Projectiles (projectiles.js). The
// special button is ads.js routeSpecial: on a gun that does not zoom, right mouse is the special too. The shot ids follow
// weapons.js newShot / groupShot (nails close together share one, style.hitShotGroup).
// Not ported yet (logged): the Kestrel's special and momentum variant, the THUNDERCLAP's compress special and variant,
// the other 12 weapons.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public abstract class Enemy
    {
        public int id;
        public string kind;
        public bool alive;
        public double hp;
        public double maxHp;
        public Vec3 position; // feet
        public Vec3 previous;
        public Vec3 velocity;
        public double yaw;
        public int windupSteps;
        public int staggerSteps;
        public int knockSteps;
        public double flash; // s of hit flash left (visual)
        public bool dealtDamage;
        /// <summary>NIGHT29 design.n29.tank: this body is the level-2 TANK (a husk body with its own hp, its own colour and the `tank` vulnerability row); false for every body of the web.</summary>
        public bool tank29;
        /// <summary>NIGHT29 design.n29.roomRules: a wall-ride room's STAND-IN (a husk body that never moves by itself and never attacks); it takes damage only while the shooter wall rides.</summary>
        public bool standIn29;
        /// <summary>NIGHT41 (the shared commit): this body's weak point, the spot aim41 steers to (Core/WeakPoint41.cs); inactive unless a boss builder fills it.</summary>
        public WeakPoint41 weakPoint41;

        public abstract double Height { get; }
        public abstract double Radius { get; }
        public abstract double AimHeight { get; }

        // NIGHT19 (the shared commit): the combo hooks. Builder E sets them (its kinds, the launch, the affixes), builder W's
        // combos read them (LAUNCH + SNIPE, the plate, CONDUCTOR). Neutral by default; nothing sets or reads them with the
        // NIGHT19 blocks off, so NIGHT18's runs are unchanged (not part of the fingerprint's hash).
        /// <summary>Off the ground by a launch, or a flying kind (a DRONE): LAUNCH + SNIPE's target.</summary>
        public bool Airborne { get; set; }
        /// <summary>A SHIELDED front plate is up (a headshot or a melee hit breaks it).</summary>
        public bool Shielded { get; set; }
        /// <summary>Nails left in it (the STITCHER's): a conductor for a volt hit (CONDUCTOR) while above 0.</summary>
        public int ConductiveNails { get; set; }
        /// <summary>CONDUCTOR's battery: nails in it.</summary>
        public bool Conductive => ConductiveNails > 0;

        /// <summary>Distance along a unit ray to this enemy's hit shapes (-1: miss), and whether it hit the head.</summary>
        public abstract double RayHit(Vec3 o, Vec3 d, out bool headshot);

        public Vec3 AimPoint => new Vec3(position.x, position.y + AimHeight, position.z);

        /// <summary>Apply damage; true if this killed it.</summary>
        public virtual bool Damage(double amount, double flashTime)
        {
            if (!alive) return false;
            hp -= amount;
            flash = flashTime;
            if (hp <= 0)
            {
                alive = false;
                return true;
            }
            return false;
        }

        public virtual void Knockback(double dirX, double dirZ, double speed, double time, double dt)
        {
            double len = JsMath.Hypot(dirX, dirZ);
            if (len == 0) len = 1;
            velocity.x = dirX / len * speed;
            velocity.z = dirZ / len * speed;
            knockSteps = MovementMath.StepsFor(time, dt);
        }

        /// <summary>A parry: cancel the wind-up (no attack lands) and stagger.</summary>
        public virtual void Stagger(double time, double dt)
        {
            windupSteps = 0;
            staggerSteps = MovementMath.StepsFor(time, dt);
        }

        /// <summary>
        /// enemyKind.js stun (POWER P4; NIGHT9 U3-09: the impact wave's STAGGER): it stands still for `time` s (after any
        /// knockback slide); an attack it committed to is cancelled as a parry cancels it.
        /// </summary>
        public virtual void Stun(double time, double dt)
        {
            staggerSteps = Math.Max(staggerSteps, MovementMath.StepsFor(time, dt));
            if (windupSteps > 0) Stagger(time, dt);
        }
    }

    public struct EnemyRayHit
    {
        public Enemy enemy;
        public double distance;
        public bool headshot;
    }

    /// <summary>enemies.js: every live enemy behind one API (raycast, punch targets, within, aim points).</summary>
    public sealed class EnemyRegistry : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the id counter (ids continue across rounds; stuck nails, homing and the
        // records check them). The list itself (the pools' dead enemies, in creation order) is the "husks" section's.
        public void Save(StateWriter w) => w.I("nextId", nextId);

        public void Load(StateReader r) => nextId = r.Int("nextId");

        public readonly List<Enemy> all = new List<Enemy>();
        readonly List<BodyTarget> targets = new List<BodyTarget>();
        int nextId;

        public int NextId() => ++nextId;

        /// <summary>NIGHT14 U3-27.2 (port-only: a clean slate per run): no enemies and the id counter at 0, as a fresh composition
        /// starts. The web keeps both across runs in one page (its pools and ids carry over), so a second run of a seed differed
        /// from the first. The pools are emptied with it (Husks.ClearPools).</summary>
        public void Reset()
        {
            all.Clear();
            nextId = 0;
        }

        public int AliveCount
        {
            get
            {
                int n = 0;
                foreach (Enemy e in all) if (e.alive) n++;
                return n;
            }
        }

        public EnemyRayHit? Raycast(Vec3 origin, Vec3 dir, double maxDistance)
        {
            EnemyRayHit? best = null;
            foreach (Enemy e in all)
            {
                if (!e.alive) continue;
                double d = e.RayHit(origin, dir, out bool head);
                if (d >= 0 && d <= maxDistance && (!best.HasValue || d < best.Value.distance)) best = new EnemyRayHit { enemy = e, distance = d, headshot = head };
            }
            return best;
        }

        /// <summary>punchTargets: vertical capsules with the wind-up state (reused list).</summary>
        public List<BodyTarget> PunchTargets()
        {
            int k = 0;
            foreach (Enemy e in all)
            {
                if (!e.alive) continue;
                if (e is BulwarkBody) continue; // NIGHT20 A1 (the lead, from EN): bulwark.js punchTargets: () => [] (also keeps the cone guns off it)
                if (k >= targets.Count) targets.Add(new BodyTarget());
                BodyTarget t = targets[k++];
                t.enemy = e;
                t.basePos = e.position;
                t.height = e.Height;
                t.radius = e.Radius;
                t.windupSteps = e.windupSteps;
            }
            targets.RemoveRange(k, targets.Count - k);
            return targets;
        }

        public void Within(Vec3 point, double radius, List<Enemy> output)
        {
            output.Clear();
            foreach (Enemy e in all) if (e.alive && Vec3.DistanceTo(e.AimPoint, point) <= radius) output.Add(e);
        }

        public void Clear() => all.Clear();

        /// <summary>
        /// enemies.js hitEnemy (NIGHT19 U3-33.6, for the BOUNCER's grenades): the first live enemy whose body (a vertical
        /// capsule, husk.js hitSphere) overlaps the sphere at `point`, or null.
        /// </summary>
        public Enemy HitSphere(Vec3 point, double radius)
        {
            foreach (Enemy e in all)
            {
                if (!e.alive) continue;
                double y = Math.Max(e.position.y + e.Radius, Math.Min(e.position.y + e.Height - e.Radius, point.y));
                if (JsMath.Hypot(point.x - e.position.x, point.y - y, point.z - e.position.z) <= radius + e.Radius) return e;
            }
            return null;
        }
    }

    /// <summary>One fixed step of the player's combat input (the web InputFrame's fire / altFire / punch / weapon).</summary>
    public struct CombatInput
    {
        public Button fire;
        public Button altFire;
        public Button special; // the SPECIAL key (input.bindings.special); right mouse joins it on a gun that does not zoom (ads.js routeSpecial)
        public Button punch;
        public Button interact;
        public int weapon; // the slot asked for this step, -1 none
    }

    // NIGHT19 U3-33.6: partial (Core/Guns/Sandbox19.cs: the niches and combos; Zapper.cs and Bouncer.cs: the ports).
    public sealed partial class Loadout : IProjectileLoadout, IProjectileOwner, IRunState
    {
        /// <summary>NIGHT19 (the shared commit): the run's NIGHT19 blocks (SurvivalSim sets them at the run's start, before
        /// Reset). Every NIGHT19 rule in the loadout reads its block here; all off: NIGHT18's loadout.</summary>
        public N19Blocks n19;
        /// <summary>NIGHT20 (the shared commit): the run's NIGHT20 blocks (SurvivalSim sets them at the run's start, before
        /// Reset): design.shockwave, design.zoom, design.arsenal.n20 read here; all off: NIGHT19's loadout.</summary>
        public N20Blocks n20;
        /// <summary>NIGHT19 U3-33.5 (design.gust): the KESTREL's wind gust (Core/Guns/Gust.cs); used only with n19.gust on.</summary>
        public readonly KestrelGust gust;

        // NIGHT20 U3-34.8 (builder G): NIGHT19 W's sandbox (Sandbox19, the ports, AltUse) merged tonight runs with its own
        // NIGHT19 block (design.arsenal / .forge / .freshness, all off in NIGHT19's game) or with design.arsenal.n20, which
        // turns the three on together: with every NIGHT20 block off (--off20) the loadout is NIGHT19's bit for bit.
        /// <summary>The NIGHT19 sandbox's rules (design.arsenal or design.arsenal.n20).</summary>
        public bool Arsenal19On => n19.arsenal || n20.arsenal;
        /// <summary>The FORGE's sidegrade (design.forge or design.arsenal.n20).</summary>
        public bool Forge19On => n19.forge || n20.arsenal;
        /// <summary>ULTRAKILL's freshness table on points (design.freshness or design.arsenal.n20).</summary>
        public bool Fresh19On => n19.freshness || n20.arsenal;

        // NIGHT13 U3-26.10: the run snapshot: the stream, the guns owned (hotbar order) and the swap timeline, the per-gun
        // cooldowns (in their order), the punch, ADS, the shot ids and the nail group's clock, heat / charge / the specials,
        // the multipliers the perks (QUICK, DEADEYE) and HEAT's mutators set, a bought gun waiting for the hand, and the
        // arsenal, momentum and impact parts. Not saved: the boot-time flags (design.ultra, the perfect parry window), the
        // zoom cache and the wiring. Load also hands the projectiles their owner back (Projectiles.Adopt).
        public void Save(StateWriter w)
        {
            w.Rng("rng", rng);
            w.Strings("owned", owned);
            w.I("slot", slot);
            w.I("targetSlot", targetSlot);
            w.D("swapLoweredNow", swapLoweredNow);
            w.D("prevSwapLowered", prevSwapLowered);
            w.B("ready", ready);
            w.B("swapActive", swapActive);
            w.D("swapT", swapT);
            w.I("punchCooldown", punchCooldown);
            w.D("damageMult", damageMult);
            w.D("fireRateMult", fireRateMult);
            w.D("swapMult", swapMult);
            w.D("chargeMult", chargeMult);
            w.D("headshotMult", headshotMult);
            w.B("adsActive", adsActive);
            w.B("parryReady", parryReady);
            w.B("assisted", assisted);
            w.I("shotSeq", shotSeq);
            w.I("curShot", curShot);
            w.I("nailGroupId", nailGroupId);
            w.D("nailGroupAt", nailGroupAt);
            w.B("nailGroup", nailGroup);
            w.D("clock", clock);
            w.D("heat", heat.heat);
            w.D("heatLock", heat.@lock);
            w.D("railCharge", railCharge);
            w.D("overcharge", overcharge);
            w.I("nailSpecialCooldown", nailSpecialCooldown);
            w.I("railSpecialCooldown", railSpecialCooldown);
            w.Ints("specialStats", new[] { detonated, steered, overcharged, wallsPierced });
            w.I("pendingSlot", pendingSlot);
            var names = new List<string>(cooldowns.Count);
            var steps = new List<int>(cooldowns.Count);
            foreach (KeyValuePair<string, Cooldown> kv in cooldowns)
            {
                names.Add(kv.Key);
                steps.Add(kv.Value.steps);
            }
            w.Strings("cooldownNames", names);
            w.Ints("cooldownSteps", steps);
            w.Sub("arsenal", arsenal);
            w.Sub("momentum", momentum);
            w.Sub("impact", impact);
            w.Begin("guns20"); // NIGHT20 (builder G2): the NIGHT19 / NIGHT20 guns' state that crosses a break (Guns/Snapshot20.cs)
            SaveGuns20(w);
            w.End();
        }

        public void Load(StateReader r)
        {
            List<string> o = r.Strings("owned"), names = r.Strings("cooldownNames");
            List<int> steps = r.Ints("cooldownSteps"), stats = r.Ints("specialStats");
            if (names.Count != steps.Count || stats.Count != 4) throw new StateException("snapshot: loadout cooldowns / stats");
            arsenal.Load(r.Section("arsenal"));
            momentum.Load(r.Section("momentum"));
            impact.Load(r.Section("impact"));
            rng = r.Rng("rng");
            owned.Clear();
            owned.AddRange(o);
            slot = r.Int("slot");
            targetSlot = r.Int("targetSlot");
            swapLoweredNow = r.D("swapLoweredNow");
            prevSwapLowered = r.D("prevSwapLowered");
            ready = r.B("ready");
            swapActive = r.B("swapActive");
            swapT = r.D("swapT");
            punchCooldown = r.Int("punchCooldown");
            damageMult = r.D("damageMult");
            fireRateMult = r.D("fireRateMult");
            swapMult = r.D("swapMult");
            chargeMult = r.D("chargeMult");
            headshotMult = r.D("headshotMult");
            adsActive = r.B("adsActive");
            parryReady = r.B("parryReady");
            assisted = r.B("assisted");
            shotSeq = r.Int("shotSeq");
            curShot = r.Int("curShot");
            nailGroupId = r.Int("nailGroupId");
            nailGroupAt = r.D("nailGroupAt");
            nailGroup = r.B("nailGroup");
            clock = r.D("clock");
            heat.heat = r.D("heat");
            heat.@lock = r.D("heatLock");
            railCharge = r.D("railCharge");
            overcharge = r.D("overcharge");
            nailSpecialCooldown = r.Int("nailSpecialCooldown");
            railSpecialCooldown = r.Int("railSpecialCooldown");
            detonated = stats[0];
            steered = stats[1];
            overcharged = stats[2];
            wallsPierced = stats[3];
            pendingSlot = r.Int("pendingSlot");
            cooldowns.Clear();
            for (int i = 0; i < names.Count; i++) cooldowns.Add(names[i], new Cooldown { steps = steps[i] });
            if (r.Has("guns20")) LoadGuns20(r.Section("guns20")); // NIGHT20 (builder G2): absent in an older save (its run keeps StartRun's)
            projectiles?.Adopt(this, this);
        }

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly EnemyRegistry enemies;
        readonly ICollisionWorld world;
        readonly Mover mover;
        readonly Health health;
        readonly Func<(Vec3 origin, Vec3 dir)> getAim;
        sealed class Cooldown { public int steps; } // a box per weapon, so a step counts them down without copying the keys (U2-02)
        readonly Dictionary<string, Cooldown> cooldowns = new Dictionary<string, Cooldown>();
        readonly List<TargetPick> found = new List<TargetPick>();
        // Scratch reused by every shot (U2-02): the shotgun's per-enemy damage, the ricochet's candidates.
        readonly Dictionary<Enemy, double> perEnemy = new Dictionary<Enemy, double>();
        readonly List<Enemy> order = new List<Enemy>(), who = new List<Enemy>();
        readonly List<Vec3> points = new List<Vec3>();
        readonly Func<Vec3, Vec3, bool> hasLos;
        Mulberry32 rng = new Mulberry32(1);
        public readonly List<string> owned = new List<string>();
        public int slot;
        public int targetSlot;
        public double swapLoweredNow, prevSwapLowered;
        public bool ready = true;
        bool swapActive;
        double swapT;
        /// <summary>NIGHT34 B (design.n34.bend34): true while the player is inside a grapple BEND (the sim's hook sets it every step; false with
        /// the row off): a swap press in StepSwap puts the new gun in hand on that step (no lower / raise timeline).</summary>
        public bool bend34Instant;
        public int bend34InstantSwaps; // NIGHT34 B: instant swaps this run (the harness)

        /// <summary>NIGHT34 B (design.n34.combo34.whipReady): the punch's cooldown steps left (the harness's read).</summary>
        public int PunchCooldownSteps => punchCooldown;

        /// <summary>NIGHT34 B (design.n34.combo34.whipReady; ULTRAKILL's whip readies the punch, research/rea-ultrakill.md "The Whiplash"): the
        /// Pulse Gauntlet's punch (the punch key) and its swing in hand (the gauntlet's own cycle) wait at most `maxSteps` more. Called by the sim
        /// on a whip latch only with the sub-switch on.</summary>
        public void ReadyPunch34(int maxSteps)
        {
            if (punchCooldown > maxSteps) punchCooldown = maxSteps;
            if (cooldowns.TryGetValue("gauntlet", out Cooldown g) && g.steps > maxSteps) g.steps = maxSteps;
        }
        int punchCooldown;
        /// <summary>NIGHT24 design.n24.shotgunTune (SurvivalSim.Shotgun24.cs sets them at a run start): the THUNDERCLAP's interval and damage multipliers on top of the web value and the NIGHT20 retune; 1 = off, exact.</summary>
        public double sgIntervalMult24 = 1, sgDamageMult24 = 1;
        /// <summary>NIGHT29 design.n29.shotgun (SurvivalSim.Systems29.cs sets it at a run start): a further multiplier on the THUNDERCLAP's interval (after the web value, the NIGHT20 retune and the NIGHT24 one); 1 = off, exact.</summary>
        public double sgIntervalMult29 = 1;
        /// <summary>NIGHT29 design.n29.lockout (Core/Guns/Lockout29.cs): fire and melee cannot overlap, both ways; null = off (NIGHT28's rules).</summary>
        public Lockout29 lockout29;
        /// <summary>NIGHT29 lockout: steps left of the gun in hand's attack cycle (its cooldown) and of the melee swing's cycle (the harness and the reel read them).</summary>
        public int GunCycleLeft29 => CooldownOf(Weapon);
        public int MeleeCycleLeft29 => punchCooldown;
        /// <summary>NIGHT29 lockout: the longest attack cycle in steps (the melee swing's or the last gun cycle the loadout set): the punchWhileFiring lesson's window grows by it.</summary>
        public int LockoutWindowSteps29 => Math.Max(pCycleSteps29, lastGunCycle29);
        int pCycleSteps29, lastGunCycle29;
        public double damageMult = 1;
        public double fireRateMult = 1, swapMult = 1, chargeMult = 1; // the QUICK perk (weaponMath.js fireInterval: base / fireRate; swapTime: base / swap; chargeTime: base x charge)
        public bool adsActive;
        // NIGHT10 U3-13.9 (design.ultra, Ultra.cs): the web's rules by default; the game applies the flags at boot.
        public bool freshDamage = true, parryRefillsDash = true;
        public bool parryReady;
        public int shotSeq; // weapons.js shotSeq: the last trigger pull id handed out
        int curShot; // weapons.js loadout.shot: the trigger pull the next hits belong to (groupShot may reuse an older one)
        int nailGroupId; // groups.nailgun {id, at}
        double nailGroupAt;
        bool nailGroup;
        double clock; // s of this loadout's steps
        /// <summary>Nails and rockets in flight (weapons.js world.projectiles); null: none (the guns fire nothing).</summary>
        public Projectiles projectiles;
        // NIGHT10: the ARMORY guns' state (weapons.js heat, railCharge, overcharge, specialCooldowns, stats)
        readonly HeatState heat = new HeatState();
        readonly NailgunParams nailParams;
        double railCharge, overcharge; // 0..1 while charging
        int nailSpecialCooldown, railSpecialCooldown; // steps (specialCooldowns.nailgun / .rail)
        public int detonated, steered, overcharged, wallsPierced; // weapons.js specialStats
        readonly HashSet<Enemy> railHits = new HashSet<Enemy>();
        double rocketInterval; // NIGHT26 weaponIdentity: the rocket's reload (Loadout.Apply26)
        readonly double nailRate, nailSpread, railChargeTime, railCooldown, railDamage, railHeadMult, railRange, hitShotGroup, variantAt;
        readonly int railPierce;
        readonly double spNailCooldown, spRailOvercharge, spRailCooldownMult, spRailDamageMult, spRailSelfPush;
        readonly int spNailPierce, spRocketFragments, spRailWalls;
        readonly RailChargeEvent railChargeEvent = new RailChargeEvent();
        readonly RailFireEvent railFireEvent = new RailFireEvent();
        public AimAssist assist; // the player's (weapons.js owner.assist); null: none
        public readonly string owner; // weapons.js owner.id: every hit, kill, shot, swing and bonus carries it (NIGHT9 Players)
        public SpitterAcid acid; // the spitters' globs a punch can reflect (U2-10); null: none
        // weapons.js loadout.bonus (the run's FORGE and MODS; null: none): mult (enemy, weapon, headshot) -> x damage per hit,
        // and after (enemy, weapon) once the hit and kill events went out.
        public Func<Enemy, string, bool, double> bonus;
        public Action<Enemy, string> after;
        bool assisted; // this shot was bent by aim assist (aimShot)
        /// <summary>Freshness, primed swaps, cross combos (POWER P3; weapons.js loadout.arsenal). Its `enabled` is main.js's per mode.</summary>
        public readonly Arsenal arsenal;
        /// <summary>DEADEYE (POWER P6, weapons.js setPerkMods headshotMult): x every headshot.</summary>
        public double headshotMult = 1;
        /// <summary>Momentum damage (OVERNIGHT2 N2; NIGHT9 U3-09) and the melee impact shockwaves.</summary>
        public readonly Momentum momentum;
        public readonly Impact impact;
        /// <summary>Hit sources that are secondary damage (no momentum): burns, the overload burst, the aura, PHOENIX, parried orbs.</summary>
        static readonly HashSet<string> Secondary = new HashSet<string> { "burn", "overload", "static", "phoenix", "orb", "slam", "trap", "boss", "shockwave", "slide" /* NIGHT21 SLIDE BITES */ };
        readonly double eyeHeight, impactBonusAt, impactBonus, lunge, lungeTime; // aim.lunge / aim.lungeTime: the melee lunge

        // config, read once
        readonly double revDamage, revInterval, revHeadMult, revRange, adsHeadMult, hipSpreadDeg, losSlack;
        readonly double gDamage, gCycle, gRange, gCone, gKnock, gKnockTime, gParryPeek;
        /// <summary>NIGHT41 E6 heavy41 (the sim sets it at a run's start from sim.n41): the gauntlet's swing asks Heavy41 for a big enemy's answer.</summary>
        public bool heavy41;
        /// <summary>NIGHT41 E6 heavy41: the sim's hitstop (SurvivalSim.Hitstop, set at the sim's Init); a Heavy41 answer's hitstop goes through it.</summary>
        public System.Action<double, string> hitstop41;
        /// <summary>NIGHT41 E6 heavy41 (the body in E6's Core/Heavy41.cs): a gauntlet hit on `enemy`; a big kind may raise the knock-back
        /// (speed, time) and ask for a hitstop (s, 0: none). No body: the old knock, no hitstop. Called only while heavy41 is on.</summary>
        partial void Heavy41(Enemy enemy, ref double knock, ref double knockTime, ref double hitstop);
        readonly double pCycle, pRange, pCone, pDamage, pKnock, pKnockTime, pStagger, pParryHeal;
        readonly double swapTime, swapLowerFraction, swapFireFraction, ricochetRange, ricochetMult, walkMult, flashTime;
        readonly double sgInterval, sgCone, sgRange, sgPelletDamage, sgJump;
        readonly int sgPellets;
        public int perfectSteps; // PERFECT PARRY's window in steps (0: off)
        readonly HashSet<string> suspend;

        public string Weapon => owned.Count > 0 ? owned[Math.Min(slot, owned.Count - 1)] : "";

        public Loadout(GameConfig cfg, EventBus events, EnemyRegistry enemies, ICollisionWorld world, Mover mover, Health health, Func<(Vec3, Vec3)> getAim, string owner = "player")
        {
            this.cfg = cfg;
            this.events = events;
            this.enemies = enemies;
            this.world = world;
            this.mover = mover;
            this.health = health;
            this.getAim = getAim;
            this.owner = owner;
            hasLos = HasLos;
            revDamage = cfg.Num("weapons.revolver.damage");
            revInterval = cfg.Num("weapons.revolver.fireInterval");
            revHeadMult = cfg.Num("weapons.revolver.headshotMult");
            revRange = cfg.Num("weapons.revolver.range");
            adsHeadMult = cfg.Num("ads.headshotMult");
            hipSpreadDeg = cfg.Has("ads.hipSpreadDeg.revolver") ? cfg.Num("ads.hipSpreadDeg.revolver") : 0;
            losSlack = cfg.Num("weapons.losSlack");
            gDamage = cfg.Num("weapons.gauntlet.damage");
            gCycle = cfg.Num("weapons.gauntlet.cycle");
            gRange = cfg.Num("weapons.gauntlet.range");
            gCone = cfg.Num("weapons.gauntlet.coneDeg");
            gKnock = cfg.Num("weapons.gauntlet.knockback");
            gKnockTime = cfg.Num("weapons.gauntlet.knockbackTime");
            gParryPeek = cfg.Num("weapons.gauntlet.parryPeek");
            pCycle = cfg.Num("punch.cycleTime");
            pRange = cfg.Num("punch.range");
            pCone = cfg.Num("punch.coneHalfAngleDeg");
            pDamage = cfg.Num("punch.damage");
            pKnock = cfg.Num("punch.knockbackSpeed");
            pKnockTime = cfg.Num("punch.knockbackTime");
            pStagger = cfg.Num("punch.staggerTime");
            pParryHeal = cfg.Num("punch.parryHeal");
            swapTime = cfg.Num("weapons.swapTime");
            swapLowerFraction = cfg.Num("weapons.swapLowerFraction");
            swapFireFraction = cfg.Num("weapons.swapFireFraction");
            ricochetRange = cfg.Num("ads.ricochet.range");
            ricochetMult = cfg.Num("ads.ricochet.damageMult");
            walkMult = cfg.Num("ads.walkMult");
            flashTime = cfg.Num("husk.hitFlashTime");
            suspend = new HashSet<string>(cfg.Strs("ads.suspendStates"));
            sgInterval = cfg.Num("weapons.shotgun.fireInterval");
            sgCone = cfg.Num("weapons.shotgun.coneHalfAngleDeg");
            sgRange = cfg.Num("weapons.shotgun.range");
            sgPelletDamage = cfg.Num("weapons.shotgun.pelletDamage");
            sgJump = cfg.Num("weapons.shotgun.jumpSpeed");
            sgPellets = cfg.Int("weapons.shotgun.pellets");
            nailParams = NailgunParams.From(cfg);
            nailRate = cfg.Num("weapons.nailgun.rate");
            nailSpread = cfg.Num("weapons.nailgun.spreadDeg");
            rocketInterval = cfg.Num("weapons.rocket.fireInterval");
            railChargeTime = cfg.Num("weapons.rail.charge");
            railCooldown = cfg.Num("weapons.rail.cooldown");
            railDamage = cfg.Num("weapons.rail.damage");
            railHeadMult = cfg.Num("weapons.rail.headshotMult");
            railRange = cfg.Num("weapons.rail.range");
            railPierce = cfg.Int("weapons.rail.pierce");
            spNailCooldown = cfg.Num("weapons.specials.nailgun.cooldown");
            spNailPierce = cfg.Int("weapons.specials.nailgun.variantPierce");
            spRocketFragments = cfg.Int("weapons.specials.rocket.variantFragments");
            spRailOvercharge = cfg.Num("weapons.specials.rail.overchargeTime");
            spRailCooldownMult = cfg.Num("weapons.specials.rail.cooldownMult");
            spRailDamageMult = cfg.Num("weapons.specials.rail.damageMult");
            spRailSelfPush = cfg.Num("weapons.specials.rail.selfPush");
            spRailWalls = cfg.Int("weapons.specials.rail.variantWalls");
            hitShotGroup = cfg.Num("style.hitShotGroup");
            variantAt = cfg.Num("momentum.variantAt");
            perfectSteps = cfg.Design("parry.perfect") ? MovementMath.StepsFor(cfg.Num("design.parry.perfectWindow"), cfg.Dt) : 0;
            eyeHeight = cfg.Num("body.eyeHeight");
            impactBonusAt = cfg.Num("momentum.impactBonusAt");
            impactBonus = cfg.Num("style.bonus.impact");
            lunge = cfg.Num("aim.lunge");
            lungeTime = cfg.Num("aim.lungeTime");
            momentum = new Momentum(cfg, mover);
            impact = new Impact(cfg, events, enemies, mover, this);
            arsenal = new Arsenal(cfg, events, owner, enemies);
            // Burn ticks and the OVERLOAD burst deal damage as the loadout (from the enemy itself: no heal distance).
            // (Secondary damage belongs to no trigger pull: shot 0.)
            arsenal.SetDamage((e, amount, w) => Damage(e, amount, false, w, e.AimPoint, false, 1, 0));
            gust = new KestrelGust(GustParams.From(cfg)); // NIGHT19 U3-33.5 (design.gust; null params without its numbers)
            shockwave = new Shockwave(ShockwaveParams.From(cfg), cfg.Dt); // NIGHT20 U3-34.3 (design.shockwave; null params without its numbers)
            Zoom20Init(); // NIGHT20 U3-34.3 (design.zoom)
            Tune20Init(); // NIGHT20 U3-34.5 (design.arsenal.n20.tune: the retune's knobs)
            Combos20Init(); // NIGHT20 U3-34.9 B2 / D1 (design.adds.b2 / .d1)
            Torch20Init(cfg); // NIGHT20 U3-34.9 B5: the TORCH (design.adds.b5)
            Vortex20Init(cfg); // NIGHT20 U3-34.9 B5: the VORTEX (design.adds.b5)
            a19 = Arsenal19Params.From(cfg); // NIGHT19 U3-33.6 (design.arsenal; null without its numbers)
            InitZapper(cfg);
            InitBouncer(cfg);
            arsenal.gunsOwned = GunsOwned; // NIGHT19 design.freshness: one gun owned = the FRESH floor
            // The ARMORY wall and the box list the ports when the design's arsenal block is on (each run gates them by its
            // own Arsenal19On: SurvivalSim.Arsenal19, MysteryBox.ports); never under --off19 (ForceOff). NIGHT20: or
            // design.arsenal.n20, never under --off20.
            portsFire = (cfg.Design("arsenal.enabled") && !N19Blocks.ForceOff) || (cfg.Design("arsenal.n20.enabled") && !N20Blocks.ForceOff && !N19Blocks.ForceOff);
            Reset(cfg.Strs("weapons.survivalStart"), 1);
        }

        /// <summary>A new run: the start loadout, a fresh hip-spread stream from the run seed.</summary>
        public void Reset(List<string> start, uint seed)
        {
            owned.Clear();
            owned.AddRange(start);
            rng = new Mulberry32((seed ^ 0x6b357u) >> 0);
            SetSlot(0);
            punchCooldown = 0;
            adsActive = false;
            mover.walkMult = 1;
            // main.js startRun: loadout.arsenal.reset(); enabled = true (freshness, primed swaps, cross combos in every mode).
            arsenal?.Reset();
            if (arsenal != null) arsenal.enabled = true;
            momentum?.Reset(); // weapons.js reset: momentum and the impact cooldown
            impact?.Reset();
            gust?.Reset(); // NIGHT19 U3-33.5: both charges ready
            shockwave?.Reset(); // NIGHT20 U3-34.3: the base charges ready
            descoped = false;
            Combos20Reset(); // NIGHT20 U3-34.9 B2 / D1: the rows' bits, no pushes
            Torch20Reset(); // NIGHT20 U3-34.9 B5: the row's bit, a cool TORCH, no walls
            Vortex20RunReset(); // NIGHT20 U3-34.9 B5: no press, no repel, the dash clock, the stats
            Reset19(); // NIGHT19 U3-33.6: the compress, the launched, the swap clock, the ZAPPER's heat
            comboNails31 = slugBossHits31 = 0; // NIGHT31 item 2c's and the slug's run counters
            charge27?.Reset(charge27.active); // NIGHT27: whatever path restarts the loadout, the charge's hold / cooldown and the glove start empty
            pulse27?.Reset(pulse27.active);
        }

        public void SetSlot(int s)
        {
            slot = targetSlot = Math.Max(0, Math.Min(owned.Count - 1, s));
            swapActive = false;
            swapLoweredNow = prevSwapLowered = 0;
            ready = true;
            railCharge = 0; // weapons.js setSlot: the charge, the specials' cooldowns, the heat
            overcharge = 0;
            nailSpecialCooldown = railSpecialCooldown = 0;
            heat.heat = 0;
            heat.@lock = 0;
            foreach (Cooldown c in cooldowns.Values) c.steps = 0;
            Vortex20Reset(); // NIGHT20 B5: guns.js reset() (setSlot): the VORTEX's press and repel
        }

        readonly Dictionary<string, bool> zoomWeapons = new Dictionary<string, bool>(); // config.ads.weapons[w], per weapon (no string per step)

        bool ZoomWeapon(string w)
        {
            if (!zoomWeapons.TryGetValue(w, out bool z)) zoomWeapons[w] = z = cfg.Has($"ads.weapons.{w}");
            return z;
        }

        // --- weapons.js's shot ids (OVERNIGHT2 N1) and the projectiles' views of this loadout --------------------------

        /// <summary>A new trigger pull or swing: its hits share this id.</summary>
        public int NewShot()
        {
            curShot = ++shotSeq;
            return curShot;
        }

        /// <summary>A pull that groups with the last nail when it came within style.hitShotGroup s (groupShot('nailgun')).</summary>
        int GroupNailShot()
        {
            if (nailGroup && clock - nailGroupAt < hitShotGroup) curShot = nailGroupId;
            else
            {
                nailGroupId = NewShot();
                nailGroupAt = clock;
                nailGroup = true;
            }
            return curShot;
        }

        public int Shot => curShot;
        public bool ArsenalEnabled => arsenal.enabled;
        public void Status(Enemy enemy, string name) => arsenal.Status(enemy, name);
        bool IProjectileLoadout.Damage(Enemy enemy, double amount, bool headshot, string weapon, Vec3 from, bool wasAssisted, double pointScale, int shot, bool blast) =>
            Damage(enemy, amount, headshot, weapon, from, wasAssisted, pointScale, shot, false, blast);
        public string Id => owner;
        public Vec3 Position => mover.position;
        public void Impulse(double x, double y, double z) => mover.Impulse(x, y, z);

        /// <summary>Nailgun heat 0..1 and the overheat lock.</summary>
        public double Heat => heat.heat;
        public bool Overheated => heat.@lock > 0;

        /// <summary>Railcannon charge 0..1 (0 when not charging); the overcharge counts too (N2).</summary>
        public double RailCharge => JsMath.Max(railCharge, overcharge);

        /// <summary>weapons.js cooldownFraction(name): cooldown left (0..1 of the full cooldown), the HUD's rail / rocket readiness.</summary>
        public double CooldownFractionOf(string name)
        {
            double full = name == "rail" ? railCooldown : name == "rocket" ? rocketInterval : 0;
            return full > 0 ? JsMath.Min(1, CooldownOf(name) / (double)MovementMath.StepsFor(full, cfg.Dt)) : 0;
        }

        /// <summary>weapons.js meter(): the weapon panel's bar for the weapon in hand (heat, charge, reload); false: none.</summary>
        public bool Meter(out double value, out bool hot, out bool locked)
        {
            string name = Weapon;
            hot = locked = false;
            value = 0;
            if (name == "nailgun")
            {
                value = heat.heat;
                hot = heat.heat > 0.6 && heat.@lock <= 0;
                locked = heat.@lock > 0;
                return true;
            }
            if (name == "rail")
            {
                value = railCharge > 0 ? railCharge : 1 - CooldownFractionOf("rail");
                return true;
            }
            if (name == "rocket")
            {
                value = 1 - CooldownFractionOf("rocket");
                return true;
            }
            // NIGHT19 U3-33.6 (guns.js meter): the ZAPPER's heat, the BOUNCER's reload.
            if (name == "zapper")
            {
                value = zapHeat.heat;
                hot = zapHeat.heat > 0.6 && zapHeat.@lock <= 0;
                locked = zapHeat.@lock > 0;
                return true;
            }
            if (name == "torch" && torchHeat != null)
            {
                // NIGHT20 B5 (guns.js meter): the TORCH's heat
                value = torchHeat.heat;
                hot = torchHeat.heat > 0.6 && torchHeat.@lock <= 0;
                locked = torchHeat.@lock > 0;
                return true;
            }
            if (name == "vortex" && vortexOk) return VortexMeter(out value); // NIGHT20 B5 (guns.js meter): the reload
            if (name == "bouncer")
            {
                double full = MovementMath.StepsFor(bInterval / fireRateMult, cfg.Dt);
                value = 1 - JsMath.Min(1, CooldownOf("bouncer") / JsMath.Max(1, full));
                return true;
            }
            return false;
        }

        int CooldownOf(string w) => cooldowns.TryGetValue(w, out Cooldown c) ? c.steps : 0;

        void SetCooldown(string w, int steps)
        {
            if (!cooldowns.TryGetValue(w, out Cooldown c)) cooldowns[w] = c = new Cooldown();
            c.steps = steps;
            lastGunCycle29 = steps;
            rules30?.OnGunAction(w, steps); // NIGHT30 rules: a shot / heavy swing is an action of min(steps, actionSteps)
        }

        bool HasLos(Vec3 a, Vec3 b)
        {
            Vec3 d = b - a;
            double length = d.Length;
            if (length <= 0) return true;
            d = Vec3.DivideScalar(d, length);
            return !world.RayIntersect(a, d, out RayHit hit) || hit.distance >= length - losSlack;
        }

        /// <summary>One fixed step: swap, ADS, the punch, then fire.</summary>
        public void Step(CombatInput input, double dt)
        {
            if (pendingSlot >= 0 && input.weapon < 0) input.weapon = pendingSlot; // a bought weapon goes in hand
            pendingSlot = -1;
            if (rules30 != null)
            {
                // NIGHT30 rules: a switch waits for the ACTION in progress only (Core/Guns/Rules30.cs). An ask for the slot already in hand (or already coming up) is no
                // switch: it cancels a queued one and is never queued itself (a queued switch holds new actions: the smoke's step 3d asks its gun every frame while firing)
                if (input.weapon >= 0 && input.weapon == targetSlot)
                {
                    rules30.queuedSwitch = -1;
                    input.weapon = -1;
                }
                else input.weapon = rules30.FilterSwitch(input.weapon);
            }
            else if (lockout29 != null) input.weapon = lockout29.FilterSwitch(input.weapon, CooldownOf(Weapon) > 0 || punchCooldown > 0); // NIGHT29 lockout: a switch waits for the cycle in progress
            // ads.js routeSpecial (main.js, before the loadout): on a gun that does not zoom, right mouse is the special too.
            // NIGHT19: also on the guns whose alt is their LT job (AltIsSpecial: the KESTREL's gust, design.arsenal's).
            // NIGHT20 (design.zoom): LT zooms the zoom guns (ZoomGun > 1), every other gun's LT is its special (LtZooms).
            if (!LtZooms(Weapon))
            {
                input.special.held = input.special.held || input.altFire.held;
                input.special.pressed = input.special.pressed || input.altFire.pressed;
            }
            if (GustOn) gust.Step(mover); // NIGHT19 U3-33.5: the cooldown, the air charge back on landing (NIGHT20: off with the shockwave)
            if (ShockwaveOn) StepShockwave(); // NIGHT20 U3-34.3: the charge's clock, whatever is in hand
            clock += dt;
            momentum.Step(dt); // weapons.js step: momentum, the impact cooldown, the arsenal, then the swap
            impact.Step(dt);
            if (arsenal.enabled) arsenal.Step(dt, Weapon);
            StepSwap(input.weapon, dt);
            foreach (Cooldown c in cooldowns.Values) if (c.steps > 0) c.steps--;
            rules30?.Step(); // NIGHT30 rules: the actions run out beside the cooldowns
            StepAds(input);
            StepPunch(input, dt);
            if (rules30 != null)
            {
                if (rules30.FireBlocked)
                {
                    if (input.fire.held) rules30.droppedFireSteps++; // NIGHT30 rules: no shot during a punch's action; dropped, not buffered
                    input.fire = default;
                }
            }
            else if (lockout29 != null && Lockout29.FireBlocked(punchCooldown > 0))
            {
                lockout29.DropFire(input.fire.held); // NIGHT29 lockout: no shot while a melee swing runs; dropped, not buffered
                input.fire = default;
            }
            if (id26 != null) StepCook26(input, dt); // NIGHT26 weaponIdentity: the BOUNCER's cook (Core/Guns/Weapon26.cs)
            if (pulse25 && !Pulse27Live) StepPulse25(input, dt); // NIGHT27 pulse27: the glove (PulseCharge27.cs) replaces the release launch; NIGHT25 pulseCharge: the PULSE's release launch (Core/Guns/Pulse25.cs)
            string weapon = Weapon;
            // The gauntlet's gem flashes while a body in reach is in its parry window (melee.js step).
            parryReady = false;
            if (weapon == "gauntlet")
            {
                var (o, d) = getAim();
                foreach (TargetPick p in CombatMath.SweepTargets(o, d, GauntletTargets34(), gRange + gParryPeek, gCone * Math.PI / 180, found)) // NIGHT34 reach34: the boss margin (Pulse25.cs)
                    if (p.target.windupSteps > 0) parryReady = true;
            }
            // NIGHT10 U3-13.4 (weapons.js step :214-299): nailgun heat runs whatever is in hand: holding the trigger heats
            // it, anything else cools it.
            bool nailing = ready && weapon == "nailgun" && input.fire.held && heat.@lock <= 0;
            if (WeaponMath.HeatStep(heat, nailing, dt, nailParams)) events.Emit("overheat", "nailgun");
            if (weapon != "rail" || !input.fire.held || !ready)
            {
                // POLISH V2: zoomed, letting go of a full charge fires it (held on past full, it folds into the overcharge).
                if (railCharge >= 1 && weapon == "rail" && ready && CooldownOf("rail") == 0)
                {
                    railCharge = 0;
                    overcharge = 0;
                    SetCooldown("rail", MovementMath.StepsFor(railCooldown * TuneInterval("rail"), dt));
                    FireRail();
                }
                if (railCharge > 0) events.Emit("railCancel", owner);
                railCharge = 0;
            }
            if (nailSpecialCooldown > 0) nailSpecialCooldown--;
            if (railSpecialCooldown > 0) railSpecialCooldown--;
            bool zoomedCharge = weapon == "rail" && ready && input.fire.held && railCharge >= 1;
            if (weapon != "rail" || !ready || !(input.special.held || zoomedCharge)) overcharge = 0;
            bool arsenal19 = Arsenal19On && a19 != null;
            if (arsenal19)
            {
                StepZapperHeat(input); // NIGHT19 U3-33.6: guns.js step's ZAPPER heat and alt cooldown, whatever is in hand
                if (!ShockwaveOn && !shockwaveOff25) StepCompress(input, dt); // NIGHT25: shockwaveOff: the THUNDERCLAP's LT zooms, no compress; // the THUNDERCLAP's LT: hold to compress, let go for the double blast (NIGHT20: the shockwave's LT instead)
            }
            if (b5On) StepTorch(input, dt); // NIGHT20 B5: guns.js step's TORCH heat, alt cooldown and walls, whatever is in hand
            if (b5On && vortexOk) StepVortexMotion(dt); // NIGHT20 B5: weapons.js motion.sinceDash (the VORTEX's dash synergy)
            if (!ready) return;
            // NIGHT19 U3-33.5 (design.gust): the KESTREL's alt is the wind gust (the zoom is gone, StepAds).
            if (GustOn && weapon == "revolver" && input.special.pressed) FireGust(dt);
            // NIGHT20 U3-34.3 (design.shockwave): the THUNDERCLAP's LT (and SPECIAL) is the shockwave.
            if (ShockwaveOn && weapon == "shotgun" && input.special.pressed) FireShockwave(dt);
            if (b5On && weapon == "torch" && input.special.pressed) TorchAlt(dt); // NIGHT20 B5: the fire wall
            if (b5On && vortexOk && weapon == "vortex") VortexAltStep(input); // NIGHT20 B5: tap = collapse, hold = repel
            // NIGHT19 U3-33.6 (design.arsenal): the ports' alts (guns.js step: special pressed): the discharge, the airburst.
            if (arsenal19 && input.special.pressed)
            {
                if (weapon == "zapper") ZapperAlt(dt);
                else if (weapon == "bouncer") BouncerAlt();
            }
            // OVERNIGHT2 N2 specials of the loadout's own guns (the alt fire, the second mouse button).
            if (weapon == "nailgun" && input.special.pressed && (nailSpecialCooldown == 0 || Lt31On))
            {
                if (nailSpecialCooldown > 0) Deny31("COOLING"); // NIGHT31 ltAlways: a DETONATE press that cannot go says why on the slot
                else
                {
                    int n = projectiles != null ? projectiles.Detonate(this, this) : 0;
                    if (n > 0)
                    {
                        detonated += n;
                        nailSpecialCooldown = MovementMath.StepsFor(spNailCooldown, dt);
                        if (arsenal.enabled) arsenal.Use("nailgun", 3);
                    }
                    else if (Lt31On) Deny31("NO NAILS");
                }
            }
            else if (weapon == "rocket" && input.special.held)
            {
                // Steer: this loadout's rockets turn toward what the crosshair is on.
                var (o, d) = getAim();
                Vec3 steerPoint = CastRayEnd(o, d, 200);
                if (projectiles != null && projectiles.SteerRockets(this, steerPoint) > 0) steered++;
            }
            else if (weapon == "rail" && CooldownOf("rail") == 0 && ((input.special.held && railCharge == 0) || zoomedCharge))
            {
                // Hold SPECIAL to overcharge: a longer charge, 2x damage and a kick back (it fires by itself when full). POLISH
                // V2: zoomed, holding fire past a full charge folds into the same overcharge.
                if (overcharge == 0)
                {
                    railChargeEvent.owner = owner;
                    railChargeEvent.over = true;
                    events.Emit("railCharge", railChargeEvent);
                }
                overcharge = JsMath.Min(1, overcharge + dt / (spRailOvercharge * chargeMult));
                if (overcharge >= 1)
                {
                    overcharge = 0;
                    railCharge = 0; // a zoomed overcharge came out of a held full charge
                    SetCooldown("rail", MovementMath.StepsFor(railCooldown * TuneInterval("rail") * spRailCooldownMult, dt));
                    overcharged++;
                    FireRail(true);
                }
            }
            if (weapon == "nailgun")
            {
                if (nailing && heat.@lock <= 0 && CooldownOf("nailgun") == 0)
                {
                    SetCooldown("nailgun", Math.Max(1, MovementMath.StepsFor(1 / nailRate * TuneInterval("nailgun") / fireRateMult, dt)));
                    FireNail();
                }
                return;
            }
            if (weapon == "rail")
            {
                if (input.fire.held && CooldownOf("rail") == 0)
                {
                    if (railCharge == 0)
                    {
                        railChargeEvent.owner = owner;
                        railChargeEvent.over = false;
                        events.Emit("railCharge", railChargeEvent);
                    }
                    railCharge = JsMath.Min(1, railCharge + dt / (railChargeTime * TuneCharge("rail") * chargeMult));
                    // Unzoomed it fires by itself when full; zoomed it waits for the release (or folds into the overcharge).
                    if (railCharge >= 1 && !adsActive)
                    {
                        railCharge = 0;
                        SetCooldown("rail", MovementMath.StepsFor(railCooldown * TuneInterval("rail"), dt));
                        FireRail();
                    }
                }
                return;
            }
            if (weapon == "shotgun" && compress > 0) return; // compressing: the trigger waits (design.arsenal)
            if (b5On && (weapon == "torch" || weapon == "vortex"))
            {
                // NIGHT20 B5: the ports' triggers (guns.js step: case 'torch' / 'vortex').
                if (input.fire.held && weapon == "torch") FireTorchIfReady(dt);
                else if (input.fire.held && weapon == "vortex" && vortexOk) FireVortexIfReady(dt);
                return;
            }
            if (arsenal19 && (weapon == "zapper" || weapon == "bouncer"))
            {
                // NIGHT19 U3-33.6: the ports' triggers (guns.js step: case 'zapper' / 'bouncer').
                if (input.fire.held)
                {
                    if (weapon == "zapper") FireZapperIfReady(dt);
                    else FireBouncerIfReady(dt);
                }
                return;
            }
            if (charge27 != null && charge27.active && (weapon == "revolver" || weapon == "shotgun") && StepCharge27(weapon, input, dt)) return; // NIGHT27 chargeShots27: RT held charges the pistol / shotgun (Core/Guns/ChargeShots27.cs)
            if (!(input.fire.held || PulseLtSwing26(weapon, input)) || CooldownOf(weapon) > 0) return; // NIGHT26 pulseLT: LT held swings the PULSE too
            if (weapon == "rocket")
            {
                SetCooldown("rocket", MovementMath.StepsFor(rocketInterval * TuneInterval("rocket") / fireRateMult, dt));
                FireRocket();
            }
            else if (weapon == "revolver")
            {
                SetCooldown("revolver", MovementMath.StepsFor(revInterval * TuneInterval("revolver") / fireRateMult, dt));
                FireRevolver();
            }
            else if (weapon == "gauntlet")
            {
                SetCooldown("gauntlet", Pulse27Live ? pulse27.SwingSteps : MovementMath.StepsFor(gCycle / fireRateMult, dt)); // NIGHT27 pulse27: the heavy swing, one per 2.5 s
                SwingGauntlet(dt);
            }
            else if (weapon == "shotgun")
            {
                SetCooldown("shotgun", MovementMath.StepsFor(sgInterval * TuneInterval("shotgun") * sgIntervalMult24 * sgIntervalMult29 / fireRateMult, dt));
                if (arsenal19) FireShotgun19(dt); // NIGHT19 U3-33.6: point blank, the parry, the variant (Sandbox19.cs)
                else FireShotgun();
            }
        }

        /// <summary>
        /// The weapons this port's loadout can use (a wall buy, the Mystery Box's pool). NIGHT11's POWER guns join here
        /// and the box picks them up with no change.
        /// </summary>
        public static bool Fires(string weapon) =>
            weapon == "revolver" || weapon == "shotgun" || weapon == "gauntlet" || weapon == "nailgun" || weapon == "rocket" || weapon == "rail" ||
            (portsFire && IsPort19(weapon)) || (portsFire20 && IsPort20(weapon));

        /// <summary>NIGHT19 U3-33.6: the guns ported tonight (guns.js ZAPPER and BOUNCER; design.arsenal).</summary>
        public static bool IsPort19(string weapon) => weapon == "zapper" || weapon == "bouncer";

        /// <summary>NIGHT19: the design's arsenal block is on (the last loadout built read it): the ports join Fires.</summary>
        static bool portsFire;

        /// <summary>A wall buy (wallBuys.js): the weapon takes the next hotbar slot and goes in hand.</summary>
        public bool AddWeapon(string name)
        {
            if (owned.Contains(name)) return false;
            owned.Add(name);
            pendingSlot = owned.Count - 1;
            events.Emit("weaponBought", name);
            return true;
        }

        int pendingSlot = -1;

        // --- the THUNDERCLAP (weapons.js fireShotgun) -----------------------------------------------------------------

        void FireShotgun()
        {
            NewShot();
            var (origin, aim) = ShotAim39(); // NIGHT40 W1 charge40 (Charge35.cs): getAim() exactly, unless a ground release volley fires aimed from its press
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "shotgun");
            double half = sgCone * Math.PI / 180;
            perEnemy.Clear();
            order.Clear();
            Vec3 firstEnd = origin + aim * sgRange;
            for (int i = 0; i < sgPellets; i++)
            {
                Vec3 d = CombatMath.SampleCone(aim, half, rng.Next(), rng.Next());
                bool wallHit = world.RayIntersect(origin, d, out RayHit wall) && wall.distance <= sgRange;
                EnemyRayHit? hit = enemies.Raycast(origin, d, sgRange);
                if (hit.HasValue && (!wallHit || hit.Value.distance < wall.distance))
                {
                    Enemy e = hit.Value.enemy;
                    if (!perEnemy.ContainsKey(e)) order.Add(e);
                    perEnemy[e] = (perEnemy.TryGetValue(e, out double v) ? v : 0) + sgPelletDamage;
                    if (i == 0) firstEnd = origin + d * hit.Value.distance;
                }
                else if (wallHit && i == 0) firstEnd = origin + d * wall.distance;
            }
            foreach (Enemy e in order) Damage(e, perEnemy[e], false, "shotgun", origin, assisted);
            if (order.Count > 0) events.Emit("shotHit", "shotgun");
            // The shotgun jump: airborne only, once per shot, opposite the aim.
            ShotgunJump43(aim); // NIGHT42 boost42 (Core/Boost42.cs): the web's line with the row off; NIGHT43 boost43 (Core/Boost43.cs): a ground volley waits for a takeoff
            if (arsenal.enabled) arsenal.Use("shotgun");
            events.Emit("shotResult", new ShotEvent { owner = owner, weapon = "shotgun", origin = origin, dir = aim, end = firstEnd, surface = order.Count > 0 ? "enemy" : "world" });
        }

        // --- the STITCHER, COMET and LONGWAVE (weapons.js fireNail / fireRocket / fireRail, NIGHT10 U3-13.4) ----------

        /// <summary>Momentum along the aim at a trigger pull (0 without momentum) above momentum.variantAt: a momentum variant (N2).</summary>
        bool VariantNow() => (momentum.enabled ? AimMomentum() : 0) > variantAt;

        /// <summary>weapons.js castRay's end point: the nearest of the world (within range) and the enemies, else range out.</summary>
        Vec3 CastRayEnd(Vec3 from, Vec3 direction, double range)
        {
            bool any = false;
            double best = 0;
            if (world.RayIntersect(from, direction, out RayHit wall) && wall.distance <= range)
            {
                any = true;
                best = wall.distance;
            }
            EnemyRayHit? enemy = enemies.Raycast(from, direction, range);
            if (enemy.HasValue && (!any || enemy.Value.distance < best))
            {
                any = true;
                best = enemy.Value.distance;
            }
            double d = any ? best : range;
            return new Vec3(from.x + direction.x * d, from.y + direction.y * d, from.z + direction.z * d);
        }

        /// <summary>One nail along the aim with a little spread (a projectile: Projectiles flies it).</summary>
        void FireNail()
        {
            GroupNailShot();
            var (origin, aim) = getAim();
            assisted = assist != null && assist.Aim();
            Vec3 dir = CombatMath.SampleCone(aim, NailSpreadNow * Math.PI / 180, rng.Next(), rng.Next()); // NIGHT20: x0.5 zoomed (design.zoom)
            events.Emit("fire", "nailgun");
            MarkShot("nailgun"); // NIGHT19: SWAP-CANCEL's clock
            if (arsenal.enabled) arsenal.Use("nailgun");
            Projectiles.Projectile p = projectiles?.Fire("nail", this, this, origin, dir, assist?.HomingTarget(), assisted);
            if (p != null && Variant19("nailgun")) p.pierce = spNailPierce; // the Stitcher's momentum variant: nails pierce (NIGHT19: or forged)
        }

        void FireRocket()
        {
            NewShot();
            var (origin, aim) = getAim();
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "rocket");
            MarkShot("rocket"); // NIGHT19: SWAP-CANCEL's clock
            if (arsenal.enabled) arsenal.Use("rocket");
            Projectiles.Projectile p = projectiles?.Fire("rocket", this, this, origin, aim, assist?.HomingTarget(), assisted);
            if (p != null)
            {
                // N2: momentum grows the blast; the Comet's variant adds fragments that burst around it.
                double M = momentum.enabled ? AimMomentum() : 0;
                p.radiusMult = MomentumMath.MomentumRadius(M, momentum.Params);
                if (TuneOn) p.radiusMult *= TuneRadius("rocket"); // NIGHT20 U3-34.5: the retune's radius knob
                if (M > variantAt || (Forge19On && forgedVariant != null && forgedVariant("rocket"))) p.fragments = spRocketFragments; // NIGHT19: or forged
            }
        }

        /// <summary>
        /// The railcannon beam: every enemy along the aim up to the first wall (piercing), 150 each, heads x1.5. N2: the
        /// overcharge (alt) doubles it and kicks you back; the momentum variant goes through 2 walls.
        /// </summary>
        void FireRail(bool over = false)
        {
            NewShot();
            var (origin, dir) = getAim();
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "rail");
            MarkShot("rail"); // NIGHT19: SWAP-CANCEL's clock
            double damageMult = over ? spRailDamageMult : 1;
            // Walls: the variant passes through variantWalls of them before the beam stops.
            bool hasWall = world.RayIntersect(origin, dir, out RayHit wall);
            Vec3 wallNormal = hasWall ? wall.normal : Vec3.Zero;
            double wallDistance = hasWall ? wall.distance : 0;
            double length = hasWall && wall.distance <= railRange ? wall.distance : railRange;
            if (Variant19("rail"))
            {
                for (int k = 0; k < spRailWalls && hasWall && wallDistance <= railRange; k++)
                {
                    // Step through the wall: find where the ray leaves it, then the next wall past that.
                    double exit = 0;
                    for (double t = 0.05; t < 6; t += 0.25)
                    {
                        double at = length + t;
                        var o = new Vec3(origin.x + dir.x * at, origin.y + dir.y * at, origin.z + dir.z * at);
                        bool again = world.RayIntersect(o, dir, out RayHit againHit);
                        if (!again || againHit.distance > 0.3)
                        {
                            exit = length + t;
                            break;
                        }
                    }
                    if (exit == 0) break;
                    wallsPierced++;
                    var from = new Vec3(origin.x + dir.x * exit, origin.y + dir.y * exit, origin.z + dir.z * exit);
                    hasWall = world.RayIntersect(from, dir, out RayHit next);
                    if (hasWall) wallNormal = next.normal;
                    wallDistance = hasWall ? next.distance : 0;
                    length = hasWall && exit + next.distance <= railRange ? exit + next.distance : railRange;
                }
            }
            railHits.Clear();
            int hits = 0;
            Vec3 beamFrom = origin;
            double travelled = 0;
            for (int i = 0; i < railPierce && travelled < length; i++)
            {
                EnemyRayHit? hit = enemies.Raycast(beamFrom, dir, length - travelled);
                if (!hit.HasValue) break;
                travelled += hit.Value.distance;
                if (!railHits.Contains(hit.Value.enemy))
                {
                    railHits.Add(hit.Value.enemy);
                    Damage(hit.Value.enemy, railDamage * damageMult * (hit.Value.headshot ? railHeadMult : 1), hit.Value.headshot, "rail", origin, assisted);
                    hits++;
                }
                travelled += 0.9; // step through this body
                beamFrom = new Vec3(origin.x + dir.x * travelled, origin.y + dir.y * travelled, origin.z + dir.z * travelled);
            }
            var end = new Vec3(origin.x + dir.x * length, origin.y + dir.y * length, origin.z + dir.z * length);
            bool onWall = hasWall && length < railRange;
            if (over) mover.Impulse(-dir.x * spRailSelfPush, -dir.y * spRailSelfPush * 0.5, -dir.z * spRailSelfPush); // the kick back
            if (arsenal.enabled) arsenal.Use("rail");
            railFireEvent.owner = owner;
            railFireEvent.from = origin;
            railFireEvent.to = end;
            railFireEvent.hits = hits;
            railFireEvent.over = over;
            events.Emit("railFire", railFireEvent);
            if (hits > 0) events.Emit("shotHit", "rail");
            events.Emit("shotResult", new ShotEvent { owner = owner, weapon = "rail", origin = origin, dir = dir, end = end, normal = onWall ? wallNormal : -dir, surface = onWall ? "world" : "none", hitCount = hits });
        }

        void StepSwap(int want, double dt)
        {
            int total = MovementMath.StepsFor(swapTime / swapMult, dt);
            int lower = Math.Min(total - 1, Math.Max(1, (int)JsMath.Round(total * swapLowerFraction)));
            prevSwapLowered = swapLoweredNow;
            if (swapActive)
            {
                double before = swapT;
                swapT += 1;
                if (before < lower && swapT >= lower)
                {
                    slot = targetSlot;
                    if (arsenal.enabled) arsenal.SwapIn(Weapon); // the primed window starts as the new weapon comes up
                    if (Arsenal19On) SwapIn19(); // NIGHT19: SWAP-CANCEL remembers the gun left and its last shot
                    events.Emit("weaponRaise", Weapon);
                }
                if (swapT >= total) swapActive = false;
            }
            if (want >= 0 && want < owned.Count && want != targetSlot && bend34Instant)
            {
                // NIGHT34 B (design.n34.bend34): inside a BEND the press step is the raise step (GG: "switch weapons in real time"). swapT and the
                // lower bound are untouched (a zero-length timeline never passes the lower bound); the timeline simply does not run.
                targetSlot = want;
                events.Emit("weaponSwap", owned[want]);
                swapActive = false;
                swapT = 0;
                slot = targetSlot;
                if (arsenal.enabled) arsenal.SwapIn(Weapon);
                if (Arsenal19On) SwapIn19();
                events.Emit("weaponRaise", Weapon);
                bend34InstantSwaps++;
            }
            else if (want >= 0 && want < owned.Count && want != targetSlot)
            {
                int from = targetSlot;
                if (!swapActive)
                {
                    swapActive = true;
                    swapT = 0;
                }
                else if (swapT >= lower || want == slot) swapT = CombatMath.SwapMirror(swapT, lower, total);
                targetSlot = want;
                events.Emit("weaponSwap", owned[want]);
                _ = from;
            }
            swapLoweredNow = swapActive ? CombatMath.SwapLowered(swapT, lower, total) : 0;
            ready = !swapActive || CombatMath.SwapCanFire(swapT, lower, total, swapFireFraction);
        }

        /// <summary>
        /// NIGHT19: a gun whose alt-fire (LT, right mouse) is its special though config.ads lists it as a zoom gun: the
        /// KESTREL with design.gust (the gust; its zoom goes), and with design.arsenal the LONGWAVE (its overcharge on LT)
        /// and the ZAPPER (its discharge on LT; this port never zoomed either: StepAds zooms the KESTREL only).
        /// </summary>
        bool AltIsSpecial(string w) => (GustOn && w == "revolver") || (Arsenal19On && (w == "rail" || w == "zapper"));

        /// <summary>NIGHT19 U3-33.5: the KESTREL's wind gust from the eye along the aim (KestrelGust: charges, push, cone, event).</summary>
        void FireGust(double dt)
        {
            var (origin, dir) = getAim();
            gust.Fire(mover, enemies, origin, dir, hasLos, events, owner, dt);
        }

        void StepAds(CombatInput input)
        {
            if (ZoomOn)
            {
                StepAds20(input); // NIGHT20 U3-34.3: zoom on most guns (Core/Guns/Zoom20.cs)
                return;
            }
            // NIGHT19 U3-33.5: with design.gust the KESTREL's alt is the gust, so nothing zooms (each adsActive reader sees
            // false: the ricochet, the x1.2 zoomed headshot, the walk x0.85, AimAssist's ADS params, the viewmodel's zoom).
            bool zoomable = Weapon == "revolver" && ZoomWeapon("revolver") && !GustOn; // NIGHT11 U3-24.2: cached (cfg.Has splits the path: 4 allocations a step)
            bool want = zoomable && input.altFire.held;
            bool suspended = suspend.Contains(PortData.Name(mover.state));
            bool on = want && ready && !suspended;
            if (on == adsActive) return;
            adsActive = on;
            mover.walkMult = on ? walkMult : 1;
            events.Emit(on ? "adsStart" : "adsEnd", Weapon);
        }

        // --- the KESTREL --------------------------------------------------------------------------------------------

        void FireRevolver()
        {
            NewShot();
            var (origin, aim) = ShotAim39(); // NIGHT39 P1 (Charge35.cs): getAim() exactly, unless a p39 release tap fires aimed from its press
            assisted = assist != null && assist.Aim();
            Vec3 dir = aim;
            // NIGHT19 U3-33.5: with the zoom gone (design.gust) the primary is the precise one: design.gust.kestrelHipSpreadDeg.
            double hip = GustOn && gust.p != null ? gust.p.hipSpreadDeg : ZoomOn ? zoomP.kestrelHipSpreadDeg : hipSpreadDeg; // NIGHT20: design.zoom's (the web's 0.8)
            if (!adsActive && hip > 0) dir = CombatMath.SampleCone(aim, hip * Math.PI / 180, rng.Next(), rng.Next());
            events.Emit("fire", "revolver");
            bool wallHit = world.RayIntersect(origin, dir, out RayHit wall) && wall.distance <= revRange;
            EnemyRayHit? enemy = enemies.Raycast(origin, dir, revRange);
            var shot = new ShotEvent { owner = owner, weapon = "revolver", origin = origin, dir = dir, end = origin + dir * revRange };
            if (Arsenal19On && a19 != null)
            {
                // NIGHT19 U3-33.6 POP: a live rocket or grenade of yours in the line, nearer than what the shot would hit, bursts.
                MarkShot("revolver");
                double first = enemy.HasValue && (!wallHit || enemy.Value.distance < wall.distance) ? enemy.Value.distance : wallHit ? wall.distance : revRange;
                if (TryPop(origin, dir, first, shot))
                {
                    if (arsenal.enabled) arsenal.Use("revolver");
                    events.Emit("shotHit", "revolver");
                    events.Emit("shotResult", shot);
                    return;
                }
            }
            if (enemy.HasValue && (!wallHit || enemy.Value.distance < wall.distance))
            {
                EnemyRayHit h = enemy.Value;
                shot.end = origin + dir * h.distance;
                shot.surface = "enemy";
                Damage(h.enemy, revDamage * (h.headshot ? revHeadMult : 1), h.headshot, "revolver", origin, assisted);
                // NIGHT19: the Kestrel's momentum variant (weapons.js, N2; unported until tonight): the bullet goes through one enemy.
                if (a19 != null && (Arsenal19On || Forge19On) && Variant19("revolver")) RevolverPierce(origin, dir, h);
                events.Emit("shotHit", "revolver");
            }
            else if (wallHit)
            {
                shot.end = origin + dir * wall.distance;
                shot.surface = "world";
                shot.normal = wall.normal;
                if (adsActive && Ricochet(shot.end, wall.normal, origin)) events.Emit("shotHit", "revolver");
            }
            if (arsenal.enabled) arsenal.Use("revolver"); // a shot hits with the freshness it was fired at, then pays
            events.Emit("shotResult", shot);
        }

        /// <summary>POLISH V2: a zoomed shot off a surface within ricochet range of an enemy (clear line) bounces onto it.</summary>
        bool Ricochet(Vec3 point, Vec3 normal, Vec3 fromEye)
        {
            Vec3 from = point + normal * 0.05;
            points.Clear();
            who.Clear();
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive) continue;
                points.Add(e.AimPoint);
                who.Add(e);
            }
            int i = CombatMath.PickRicochetTarget(from, points, ricochetRange, hasLos);
            if (i < 0) return false;
            Damage(who[i], revDamage * ricochetMult, false, "ricochet", fromEye, assisted);
            events.Emit("ricochet", new ShotEvent { owner = owner, weapon = "ricochet", origin = from, end = points[i] });
            return true;
        }

        // --- the PULSE GAUNTLET and the punch ------------------------------------------------------------------------

        void SwingGauntlet(double dt)
        {
            if (swingReflect40 && SwingReflect40()) return; // NIGHT40 E3 foes40: the swing reflects a projectile in reach first (Core/Enemies/Foes40.cs; off: the old swing)
            NewShot();
            var (origin, dir) = getAim();
            assisted = assist != null && assist.Aim(); // melee.js: kit.aimShot()
            TargetPick? pick = null;
            foreach (TargetPick p in CombatMath.SweepTargets(origin, dir, GauntletTargets34(), gRange, gCone * Math.PI / 180, found)) // NIGHT34 reach34: the boss margin (Pulse25.cs)
            {
                if (HasLos(origin, ((Enemy)p.target.enemy).AimPoint))
                {
                    pick = p;
                    break;
                }
            }
            if (!pick.HasValue && fusion43 != null) pick = Fusion43Reach(origin, dir, GauntletTargets34(), gRange, gCone, true); // NIGHT43 W1 fusion43: a FUSION ready reaches reachBonus m farther (Core/Fusion43.cs; off: null)
            // melee.js assistLunge (POWER P2; NIGHT9 U3-10): a body just out of reach pulls you up to aim.lunge m in.
            bool lunged = false;
            if (!pick.HasValue && assist != null)
            {
                TargetPick? near = assist.LungeTarget(origin, dir, GauntletTargets34(), gRange, gCone * Math.PI / 180); // NIGHT34 reach34
                if (near.HasValue)
                {
                    LungeTo(near.Value.target, near.Value.distance - gRange);
                    List<TargetPick> reach = CombatMath.SweepTargets(origin, dir, GauntletTargets34(), gRange + lunge, gCone * Math.PI / 180, found); // NIGHT34 reach34
                    if (reach.Count > 0) pick = reach[0];
                    lunged = pick.HasValue;
                }
            }
            if (!pick.HasValue && fusion43 != null) Fusion43Arm(); // NIGHT43 W1 fusion43: the held swing waits for a body to come into reach (Core/Fusion43.cs; off: nothing)
            bool parry = false;
            int hits = 0;
            if (pick.HasValue)
            {
                var enemy = (Enemy)pick.Value.target.enemy;
                parry = CombatMath.IsParry(pick.Value.target.windupSteps);
                bool early36 = parry36 != null && !parry && Imminent36(enemy); // NIGHT36 P parry36: the body was about to wind up (the early buffer)
                if (early36) parry = true;
                if (parry) Stagger(enemy, pStagger, dt);
                bool launch = launch36 != null && !parry && launch36.Wants(mover, enemy); // NIGHT36 P launch36 (Launch36.cs)
                double knock41 = gKnock, knockTime41 = gKnockTime, hitstop41s = 0;
                if (heavy41) Heavy41(enemy, ref knock41, ref knockTime41, ref hitstop41s); // NIGHT41 E6 heavy41 (Core/Heavy41.cs): a big enemy's harder answer (off: the old knock, no hitstop)
                if (!launch) MeleeKnock40(enemy, enemy.position.x - origin.x, enemy.position.z - origin.z, knock41, knockTime41, dt); // NIGHT40 E3: a melee knock (Foes40.cs: the same knock; a WARDEN knows it is melee)
                if (hitstop41s > 0 && hitstop41 != null) hitstop41(hitstop41s, "heavy41"); // NIGHT41 E6 heavy41: the sim's hitstop (ImpactRules keeps a cause it does not know at the sim's length)
                Pushed20(enemy); // NIGHT20 D1: a push a trap can finish (SENT)
                Damage(enemy, (Pulse27Live ? gDamage * pulse27.p.damageMult : gDamage) * Fusion43Pre(enemy), false, "gauntlet", origin, assisted || lunged); // NIGHT27 pulse27: x damageMult; NIGHT43 W1 fusion43: x its damageMult on a FUSION (Core/Fusion43.cs; off: x1 exactly)
                if (fusion43 != null) Fusion43Post(enemy, "gauntlet", dir, dt); // NIGHT43 W1 fusion43: the knock, the stagger, the hitstop, N43Events.Fusion (off: nothing)
                hits = 1;
                if (parry) Parried(enemy, IsPerfect(pick.Value.target.windupSteps));
                if (parry36 != null) Melee36(enemy, parry, early36); // NIGHT36 P parry36
                if (launch && enemy.alive) launch36.Launch(enemy, dir.x, dir.z, dt, events);
            }
            if (arsenal.enabled) arsenal.Use("gauntlet"); // melee.js: fresh('gauntlet') after the swing's hits
            events.Emit("meleeSwing", new MeleeEvent { owner = owner, weapon = "gauntlet", hits = hits, parry = parry });
        }

        void StepPunch(CombatInput input, double dt)
        {
            if (punchCooldown > 0) punchCooldown--;
            if (!input.punch.pressed || punchCooldown > 0) return;
            if (rules30 != null)
            {
                if (rules30.MeleeBlocked(Weapon == "gauntlet"))
                {
                    rules30.droppedMelee++; // NIGHT30 rules: no melee during a gun-hand action, none with the gauntlet in hand
                    return;
                }
            }
            else if (lockout29 != null && Lockout29.MeleeBlocked(Weapon == "gauntlet", CooldownOf(Weapon) > 0))
            {
                lockout29.DropMelee(); // NIGHT29 lockout: no melee while a gun's cycle runs, and none with the gauntlet in hand (its swing is the melee)
                return;
            }
            punchCooldown = MovementMath.StepsFor(pCycle, dt);
            rules30?.OnPunch(punchCooldown); // NIGHT30 rules: the punch's action
            pCycleSteps29 = punchCooldown;
            if (arsenal.enabled) arsenal.Use("punch"); // F is the Pulse Gauntlet (POWER P3): punches wear its freshness
            var (origin, dir) = getAim();
            // punch.js tryReflect (the drone orb's parry; NIGHT8: the spitter's glob): a glob in reach goes back, homing.
            if (acid != null)
            {
                SpitterAcid.Glob glob = acid.TryReflect(origin, dir, enemies);
                if (glob != null)
                {
                    events.Emit("punch", new MeleeEvent { owner = owner, weapon = "punch", hits = 1, parry = true });
                    double healed = health.Heal(pParryHeal);
                    if (parryRefillsDash) mover.RestoreStamina(); // U3-13.9 design.ultra.parryRefillsDash (on: the web's full stamina)
                    events.Emit("parry", new HitEvent { owner = owner, weapon = "punch", kind = "orb", enemy = -1, damage = healed, position = glob.position });
                    if (parry36 != null) Parry36Glob(glob, false); // NIGHT36 P parry36: back at its shooter, the event (Parry36.cs)
                    return;
                }
                if (parry36 != null) parry36.bufferGlobSteps = parry36.bufferSteps; // NIGHT36 P parry36: a glob in reach within bufferS is still parried
                Parry42Miss("punch"); // NIGHT42 W1 parry42: the orbs' own longer buffer (Core/Parry42.cs; off: nothing)
            }
            List<BodyTarget> targets = GauntletTargets34(); // NIGHT34 reach34: the F punch is the Pulse Gauntlet's (POWER P3): the boss margin (Pulse25.cs)
            TargetPick? pick = CombatMath.PickPunchTarget(origin, dir, targets, pRange, pCone * Math.PI / 180);
            if (!pick.HasValue && fusion43 != null) pick = Fusion43Reach(origin, dir, targets, pRange, pCone, false); // NIGHT43 W1 fusion43: a FUSION ready reaches reachBonus m farther (Core/Fusion43.cs; off: null)
            bool lunged = false;
            if (!pick.HasValue && assist != null)
            {
                // Aim assist (POWER P2; NIGHT9 U3-10): a target just out of reach pulls you up to aim.lunge m toward it and the punch lands.
                TargetPick? near = assist.LungeTarget(origin, dir, targets, pRange, pCone * Math.PI / 180);
                if (near.HasValue)
                {
                    LungeTo(near.Value.target, near.Value.distance - pRange);
                    pick = near;
                    lunged = true;
                }
            }
            if (!pick.HasValue)
            {
                events.Emit("punch", new MeleeEvent { owner = owner, weapon = "punch" });
                return;
            }
            var enemy = (Enemy)pick.Value.target.enemy;
            bool parry = CombatMath.IsParry(pick.Value.target.windupSteps);
            bool early36 = parry36 != null && !parry && Imminent36(enemy); // NIGHT36 P parry36: the body was about to wind up (the early buffer)
            if (early36) parry = true;
            if (parry) Stagger(enemy, pStagger, dt);
            bool launch = launch36 != null && !parry && launch36.Wants(mover, enemy); // NIGHT36 P launch36: a moving punch launches instead of knocking back (Launch36.cs)
            if (!launch) MeleeKnock40(enemy, enemy.position.x - origin.x, enemy.position.z - origin.z, pKnock, pKnockTime, dt); // NIGHT40 E3: a melee knock (Foes40.cs)
            Pushed20(enemy); // NIGHT20 D1: a push a trap can finish (SENT)
            events.Emit("punch", new MeleeEvent { owner = owner, weapon = "punch", hits = 1, parry = parry });
            NewShot();
            Damage(enemy, pDamage * (launch ? 1 : Fusion43Pre(enemy)), false, "punch", origin, lunged); // NIGHT43 W1 fusion43: x its damageMult on a FUSION (Core/Fusion43.cs; off: x1 exactly); NIGHT43 lead: a punch that LAUNCHES (launch36, VERBS 41) keeps the launch: no FUSION judged on it (round 4 verbs: x2 killed body A before its launch)
            if (fusion43 != null) Fusion43Post(enemy, "punch", dir, dt); // NIGHT43 W1 fusion43: the knock, the stagger, the hitstop, N43Events.Fusion (off: nothing)
            if (parry) Parried(enemy, IsPerfect(pick.Value.target.windupSteps));
            if (parry36 != null) Melee36(enemy, parry, early36); // NIGHT36 P parry36: the event, or the early buffer (Parry36.cs)
            if (launch && enemy.alive) launch36.Launch(enemy, dir.x, dir.z, dt, events);
        }

        /// <summary>NIGHT36 P parry36: a melee hit's parry event, or (out of a wind-up) the body's early buffer.</summary>
        void Melee36(Enemy enemy, bool parry, bool early)
        {
            if (parry) Parry36Emit(enemy.kind, enemy.id, enemy.AimPoint, early);
            else if (enemy.alive)
            {
                parry36.bufferEnemy = enemy;
                parry36.bufferEnemySteps = parry36.bufferSteps;
            }
        }

        /// <summary>The melee lunge (punch.js lunge / melee.js lungeTo): a horizontal shove closing `gap` m toward a target over aim.lungeTime s.</summary>
        void LungeTo(BodyTarget target, double gap)
        {
            double dx = target.basePos.x - mover.position.x;
            double dz = target.basePos.z - mover.position.z;
            double len = JsMath.Hypot(dx, dz);
            if (len == 0 || double.IsNaN(len)) len = 1; // Math.hypot(dx, dz) || 1
            double speed = JsMath.Max(0, gap) / lungeTime;
            mover.Impulse(dx / len * speed, 0, dz / len * speed);
            events.Emit("lunge", JsMath.Round(gap * 100) / 100);
        }

        /// <summary>
        /// NIGHT8 U2-07 PERFECT PARRY (design.parry, Unity-only): a parry in the last design.parry.perfectWindow s of the
        /// wind-up (windupSteps counts the steps left). Off (the web's parry only) without the design flag.
        /// </summary>
        bool IsPerfect(int windupSteps) => perfectSteps > 0 && windupSteps > 0 && windupSteps <= perfectSteps;

        void Stagger(Enemy e, double time, double dt)
        {
            e.Stagger(time, dt);
            events.Emit("enemyStagger", e.id);
        }

        /// <summary>A parry: heal (to max HP when perfect), full stamina, the parry event (main.js: hitstop + flash; style).</summary>
        void Parried(Enemy e, bool perfect = false, string weapon = "punch")
        {
            double healed = health.Heal(perfect ? health.max - health.hp : pParryHeal);
            if (parryRefillsDash) mover.RestoreStamina(); // U3-13.9 design.ultra.parryRefillsDash (on: the web's full stamina)
            events.Emit("parry", new HitEvent { owner = owner, weapon = weapon, kind = e.kind, enemy = e.id, damage = healed, position = e.AimPoint, perfect = perfect });
        }

        /// <summary>
        /// weapons.js damage(): the arsenal's tags (POWER P3), the multipliers, the enemy's HP, then 'hit' (and 'kill'),
        /// the arsenal's after (stats, PRIMED / CROSS / SWAP events, the OVERLOAD burst), then the run's bonus after.
        /// `shot`: the trigger pull the hit belongs to (-1: the loadout's current one; 0: none, secondary damage).
        /// </summary>
        /// `shock`: an impact wave's share (no momentum, no new wave). NIGHT9 U3-09: momentum toward what you hit scales it.
        public bool Damage(Enemy enemy, double amount, bool headshot, string weapon, Vec3 from, bool wasAssisted = false, double pointScale = 1, int shot = -1, bool shock = false, bool blast = false)
        {
            if (enemy.standIn29 && mover.state != MoveState.WallRide && !(standInPierce31 && chargedShot31)) return false; // NIGHT29 roomRules: a stand-in dies only to a shot fired from the wall (NIGHT31: or, in a standIns31 room, to a charged KESTREL shot)
            int shotId = shot >= 0 ? shot : curShot;
            if (TuneOn) amount *= TuneDamage(weapon); // NIGHT20 U3-34.5: the retune's damage knob (Tune20.cs)
            if (sgDamageMult24 != 1 && weapon == "shotgun") amount *= sgDamageMult24; // NIGHT24 shotgunTune: per-pellet x1.30 (the sum of the pellets is scaled)
            if (nailComboMult31 != 1 && weapon == "nailgun") amount *= nailComboMult31; // NIGHT31 item 2c: a charged KESTREL shot set these nails off (ChargeShots27.cs)
            if (sgChargeMult27 != 1 && weapon == "shotgun")
            {
                // NIGHT27 chargeShots27: a charged volley (ChargeShots27.cs): x damageMult and a forward knock on each body it hits
                amount *= sgChargeMult27;
                var (ao, ad) = getAim();
                if (!(charge27 != null && charge27.p.knockHusksOnly && !(enemy is Husk))) // NIGHT31 R7 (design.n31.charge): the slug never knocks a boss
                    enemy.Knockback(ad.x, ad.z, gKnock * sgChargeKnock27, gKnockTime, dt26);
            }
            Vec3 at = enemy.AimPoint;
            double distance = JsMath.Hypot(at.x - from.x, at.y - from.y, at.z - from.z);
            bool melee = ArsenalMath.IsMelee(weapon, arsenal.Catalog);
            // OVERNIGHT2 N2: momentum toward what you hit: melee x lerp(1, 3, M), guns x lerp(1, 1.35, M); explosions and
            // secondary damage keep their numbers (a blast's radius grew when it was fired).
            double M = 0;
            if (!shock && !blast && !Secondary.Contains(weapon) && arsenal.Catalog.Has(ArsenalMath.WeaponOf(weapon)))
            {
                Vec3 p = mover.position;
                M = momentum.At(new Vec3(p.x, p.y + eyeHeight, p.z), at);
                amount *= MomentumMath.MomentumDamage(M, melee, momentum.Params);
            }
            // POWER P3: freshness x primed x cross / combo (the tags ride along on the hit for points, style and the HUD).
            // hit() returns a reused object: its tags are read here, before any event can land another hit.
            ArsenalHit tags = arsenal.enabled ? arsenal.Hit(enemy, weapon) : null;
            double freshMult = tags != null ? tags.fresh : 1;
            bool primed = tags != null && tags.primed;
            bool cross = tags != null && tags.cross;
            string combo = tags?.combo;
            string family = tags?.family;
            double tagMult = tags != null ? (freshDamage ? tags.mult : tags.multNoFresh) : 1; // U3-13.9 design.ultra.freshDamage off: FRESH pays points and style only
            // DEADEYE x headshots; POLISH V2: a zoomed headshot deals x1.2 more (the player only: the owner zooms).
            double zoomHead = headshot && adsActive ? adsHeadMult : 1;
            double bonusMult = bonus != null ? bonus(enemy, weapon, headshot) : 1; // NIGHT6: the FORGE and the MODS
            // NIGHT19 U3-33.6 (design.arsenal): LAUNCH + SNIPE (x airshotMult on a launched enemy still up) and SWAP-CANCEL.
            if (Arsenal19On && a19 != null)
            {
                if (enemy.Airborne) amount *= Airshot(enemy, weapon, at);
                amount *= SwapCancel(primed, weapon, at);
            }
            double dealt = (tags != null ? amount * tagMult : amount) * (headshot ? headshotMult * zoomHead : 1) * damageMult * bonusMult;
            // NIGHT19 (builder E; enemies.js damage(): the damageFilter, then the kind's damageMult): SHIELDED's plate and the
            // CHARGER's back weak point read the hit; a blocked hit lands nothing and emits no hit (weapons.js: no points, style,
            // regen or camp relief). The payload keeps `dealt`, as weapons.js's does. No plate, no charger: `dealt` unchanged.
            if (vuln26 != null) dealt *= vuln26.Mult(enemy.tank29 ? "tank" : enemy.kind, weapon, enemy.windupSteps > 0); // NIGHT26 weaponIdentity: this weapon on this kind (design.n26.vulnerability)
            double landed = EnemyHits.Filter(enemy, dealt, headshot, weapon, melee, from, events, out bool hitBlocked, M, vuln26 != null ? vuln26.ShieldFactor(weapon) : 0); // NIGHT20 A1 (the lead, from EN): momentum is the BULWARK's armour-piercing (the web's rule)
            if (hitBlocked) return false;
            bool killed = enemy.Damage(landed, flashTime);
            if (killed && d1On && weapon == "trap") TrapKill20(enemy, at); // NIGHT20 D1: a trap finished a push of yours: SENT
            double assistPay = wasAssisted && assist != null ? assist.Payout() : 1; // OVERNIGHT2 N1: lerp(1, 0.65) by strength
            var payload = new HitEvent
            {
                owner = owner, weapon = weapon, kind = enemy.kind, enemy = enemy.id, damage = dealt, killed = killed, headshot = headshot, melee = melee, position = at,
                shot = shotId, distance = distance, assisted = wasAssisted, assistPay = assistPay, pointScale = pointScale, freshMult = freshMult,
                primed = primed, cross = cross, combo = combo, momentum = M, shock = shock,
            };
            events.Emit("hit", payload);
            if (killed) events.Emit("kill", new KillEvent { owner = owner, weapon = weapon, kind = enemy.kind, enemy = enemy.id, headshot = headshot, melee = melee, position = at, distance = distance, assisted = wasAssisted, assistPay = assistPay });
            if (tags != null) arsenal.After(enemy, weapon, primed, cross, combo, family, killed, at);
            after?.Invoke(enemy, weapon);
            // NIGHT19 U3-33.6 CONDUCTOR: a volt hit (ZAPPER, LONGWAVE) on a body with STITCHER nails in it arcs to its neighbours.
            if (Arsenal19On && a19 != null && !conducting && (weapon == "zapper" || weapon == "rail")) Conduct(enemy, weapon);
            // A melee hit with momentum: an IMPACT (style) and, higher up, its weapon's shockwave (never from a wave's hit).
            if (melee && !shock)
            {
                if (M > impactBonusAt) events.Emit("styleBonus", new StyleBonusEvent { owner = owner, name = "IMPACT", points = impactBonus, weapon = weapon, assistPay = assistPay });
                impact.Trigger(weapon, enemy, M, dealt);
            }
            return killed;
        }

        /// <summary>Momentum 0..1 along the aim right now (a weapon's momentum variant, the crosshair arc).</summary>
        public double AimMomentum()
        {
            var (o, d) = getAim();
            return momentum.Along(o, d);
        }

        /// <summary>Seconds of cooldown left on the weapon in hand, as a share of its interval (the HUD).</summary>
        public double CooldownFraction(double dt)
        {
            string w = Weapon;
            double interval = w == "revolver" ? revInterval : w == "gauntlet" ? gCycle : 1;
            return CooldownOf(w) * dt / (interval / fireRateMult);
        }
    }

    public sealed class ShotEvent
    {
        public string owner = "player";
        public string weapon;
        public Vec3 origin;
        public Vec3 dir;
        public Vec3 end;
        public Vec3 normal;
        public string surface = "none"; // world | enemy | none
        public int hitCount; // shotResult hits.length (the projectiles' and the rail's; the KESTREL / THUNDERCLAP use surface)
        public int rayCount = 1; // shotResult rayCount: 0 for a projectile's hit (no tracer)
    }

    /// <summary>railCharge {owner, over} (reused): a charge starts (over: the overcharge special).</summary>
    public sealed class RailChargeEvent
    {
        public string owner;
        public bool over;
    }

    /// <summary>railFire {owner, from, to, hits} (reused): the beam (weaponFx draws it). `over` is the fire event's special.</summary>
    public sealed class RailFireEvent
    {
        public string owner;
        public Vec3 from, to;
        public int hits;
        public bool over;
    }

    public sealed class MeleeEvent
    {
        public string owner = "player";
        public string weapon;
        public int hits;
        public bool parry;
    }
}
