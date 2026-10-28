// NIGHT21 U3-35.3 (builder M): THE ARCLINE in Survival (design.n21.arcline). The pure half: ported line for line from
// skygrind/src/combat/arcline.js (the pick on a grapple press, the yank, the zip, the slingshot) and survival/gadgets.js (the
// 5500 terminal's rules). Survival never had the gadget (its mover's ropeEnabled was false); with the block on, ropeEnabled
// follows ownership: bought at the terminal for arcline.price, owned for the run.
//
// - One button, the web's rule (DECIDE21 D-2): the grapple press (LS / the grapple key) is the ARCLINE's; the SPECIAL button is
//   the gun's special (ads.js routeSpecial) and never fires the hook, so they do not share a key in the port.
// - The pick runs inside the Mover's step (Mover.aimHook, called only on a press with no target on the frame): a small enemy
//   near the crosshair (arcline.pullKinds) is yanked to you (no rope), a big enemy is a ZIP (the rope bites it and reels you
//   in, then lets go for a melee swing), else a GRAPPLE NODE in the cone, else any surface (GrappleAim.Choose).
// - The yank is crowd control, so it has a long cooldown (design.n21.arcline.yankCooldown, 15 s; GG's rule, CORE "Never
//   again"); while it runs a small enemy is not picked (the hook goes to a node or a surface instead). The swing keeps the
//   web's 0.4 s (arcline.cooldown).
// Events (the web's names): arclineOwned {owned}, arclinePull {id, kind, x, y, z}, arclineMiss, slingshot {owner, speed},
// gadget {id}; the mover's ropeFire / ropeAttach / ropeRelease go on the bus too, with owner "player", as the web's did.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The ARCLINE's numbers: the web's config.json `arcline` / `style.slingshotSpeed`, and design.n21.arcline's Unity-only two.</summary>
    public sealed class Arcline21Rules
    {
        public double price;
        public double enemyPull, enemyPullTime;
        public readonly List<string> pullKinds = new List<string>();
        public double slingshotSpeed;
        /// <summary>design.n21.arcline.yankCooldown (Unity-only: GG's long-cooldown rule for crowd control; the web had only the swing's 0.4 s).</summary>
        public double yankCooldown;
        /// <summary>design.n21.arcline.yankLift (Unity-only: arcline.js hardcodes 4 m/s in `enemies.launch?.(e, 4)`).</summary>
        public double yankLift;
        public double zipReelSpeed, zipArrive, zipMaxTime;

        public static Arcline21Rules From(GameConfig cfg)
        {
            var r = new Arcline21Rules
            {
                price = cfg.Num("arcline.price"),
                enemyPull = cfg.Num("arcline.enemyPull"),
                enemyPullTime = cfg.Num("arcline.enemyPullTime"),
                slingshotSpeed = cfg.Num("style.slingshotSpeed"),
                yankCooldown = cfg.Num("design.n21.arcline.yankCooldown"),
                yankLift = cfg.Num("design.n21.arcline.yankLift"),
                zipReelSpeed = cfg.Num("arcline.zip.reelSpeed"),
                zipArrive = cfg.Num("arcline.zip.arrive"),
                zipMaxTime = cfg.Num("arcline.zip.maxTime"),
            };
            r.pullKinds.AddRange(cfg.Strs("arcline.pullKinds"));
            return r;
        }

        /// <summary>Put the zip's three numbers on the Mover's config (the typed loader skips nested objects).</summary>
        public void Apply(MovementConfig mc)
        {
            mc.arcline.zip.reelSpeed = zipReelSpeed;
            mc.arcline.zip.arrive = zipArrive;
            mc.arcline.zip.maxTime = zipMaxTime;
        }
    }

    /// <summary>arclinePull's payload.</summary>
    public sealed class Arcline21PullEvent
    {
        public int id;
        public string kind;
        public double x, y, z;
    }

    /// <summary>slingshot's payload (the web's {owner, speed}).</summary>
    public sealed class Arcline21SlingshotEvent
    {
        public string owner;
        public double speed;
    }

    /// <summary>The mover's rope events as the web put them on the bus (ropeFire / ropeAttach / ropeRelease).</summary>
    public sealed class Arcline21RopeEvent
    {
        public string owner = "player";
        public double x, y, z, distance, length, speed;
        public bool node, zip, attached;
        public string reason;
    }

    /// <summary>What the pick bites, for the HUD's reticle (arcline.js reticle.kind) and the harness.</summary>
    public enum Arcline21Kind
    {
        None,
        Surface,
        Node,
        Enemy,
    }

    public sealed class Arcline21
    {
        public readonly Arcline21Rules rules;
        readonly EventBus events;
        readonly Mover mover;
        readonly EnemyRegistry enemies;
        readonly GrappleAim aim;
        readonly ICollisionWorld world;
        readonly Func<Vec3> eye;
        readonly double dt;
        readonly Func<Enemy, bool> yankOnCooldown;
        readonly Arcline21PullEvent pullEvent = new Arcline21PullEvent();
        readonly Arcline21SlingshotEvent slingshotEvent = new Arcline21SlingshotEvent { owner = "player" };
        readonly Arcline21RopeEvent ropeEvent = new Arcline21RopeEvent();
        Enemy zipEnemy;
        readonly MovementConfig mc33;
        int yankSteps;

        /// <summary>The run's counters (arcline.js stats): hooks fired, nodes, surfaces, yanks, zips, misses.</summary>
        public int fired, nodes, surfaces, pulls, zips, misses;

        public Arcline21(GameConfig cfg, EventBus events, Mover mover, EnemyRegistry enemies, GrappleAim aim, ICollisionWorld world, Func<Vec3> eye)
        {
            rules = Arcline21Rules.From(cfg);
            rules.Apply(cfg.movement);
            mc33 = cfg.movement; // NIGHT33 whip33: the live hook range (WhipRay33)
            this.events = events;
            this.mover = mover;
            this.enemies = enemies;
            this.aim = aim;
            this.world = world;
            this.eye = eye;
            dt = 1 / cfg.movement.loop.simHz;
            yankOnCooldown = e => !whip33 && yankSteps > 0 && IsPullKind(e.kind);
            mover.Events += OnMover;
        }

        /// <summary>NIGHT33 whip33 (DESIGN33 W, D-3; the sim sets it at a run's start, false with the row off): a small enemy is never skipped while the
        /// yank cools down: it becomes a zip target (you are pulled to it and stop short, SurvivalSim.Whip33.cs); a boss body (whip33BossKind) under the
        /// crosshair gives way to a node the hook could take (rings and boss joints stay pull targets). False: NIGHT32's pick bit for bit.</summary>
        public bool whip33;

        /// <summary>NIGHT33 whip33: the kinds that give way to a node (the sim's grapple32 bossKinds); null: none.</summary>
        public Func<string, bool> whip33BossKind;

        /// <summary>NIGHT33 whip33: the enemy the current zip is reeling you to (null when the last pick was not an enemy zip). Valid while
        /// mover.zipAnchor is set (the Mover clears it with the rope).</summary>
        public Enemy ZipEnemy => zipEnemy;

        /// <summary>NIGHT33 whip33: the run's small enemies taken as a zip because the yank was cooling (the harness reads it).</summary>
        public int whip33Zips;

        /// <summary>Is the gadget bought (ropeEnabled follows ownership)?</summary>
        public bool Owned => mover.ropeEnabled;

        /// <summary>Steps of the yank's cooldown left (the HUD chip's fill).</summary>
        public int YankSteps => yankSteps;

        /// <summary>The yank's cooldown as a 0..1 share left.</summary>
        public double YankShare => rules.yankCooldown <= 0 ? 0 : Math.Min(1, yankSteps * dt / rules.yankCooldown);

        public bool IsPullKind(string kind) => rules.pullKinds.Contains(kind);

        /// <summary>arcline.js reset + a run's start: not owned, no cooldown, the hook wired on the mover (or unwired: the block off).</summary>
        public void Reset(bool on)
        {
            yankSteps = 0;
            zipEnemy = null;
            fired = nodes = surfaces = pulls = zips = misses = 0;
            whip33Zips = whip33RayZips = 0;
            zipByRay33 = false;
            mover.ropeEnabled = false;
            mover.zipAnchor = null;
            mover.aimHook = on ? (Func<InputFrame, GrappleTarget?>)Pick : null;
            active = on;
        }

        bool active;

        /// <summary>arcline.js own(): the terminal's buy. Owned for the run.</summary>
        public void Own(bool on = true)
        {
            mover.ropeEnabled = on;
            events.Emit("arclineOwned", on);
        }

        /// <summary>One fixed step: the yank's cooldown runs.</summary>
        public void Step()
        {
            if (yankSteps > 0) yankSteps--;
        }

        /// <summary>
        /// arcline.js step(): a grapple press with nothing on the frame, rope idle and off cooldown (the Mover checked all three).
        /// A small enemy is yanked here (no rope, null); a big one is a zip target; else a node or a surface; null: nothing in range.
        /// </summary>
        GrappleTarget? Pick(InputFrame frame)
        {
            if (!active || !mover.ropeEnabled) return null;
            Vec3 e0 = eye();
            Vec3 dir = GrappleAim.AimDirection(frame.yaw, frame.pitch);
            Enemy enemy = aim.ChooseEnemy(e0, dir, world, enemies, yankOnCooldown);
            if (enemy != null && whip33 && whip33BossKind != null && whip33BossKind(enemy.kind))
            {
                // NIGHT33 whip33: a node in the cone (a ring, a boss joint) beats the boss body around it
                GrappleTarget? n = aim.Choose(e0, dir, world);
                if (n.HasValue && n.Value.node != 0 && n.Value.node != mover.rope.lastNode)
                {
                    fired++;
                    nodes++;
                    return n;
                }
            }
            if (enemy != null)
            {
                fired++;
                Vec3 at = enemy.AimPoint;
                if (IsPullKind(enemy.kind) && !(whip33 && yankSteps > 0))
                {
                    // Yank it to you.
                    pulls++;
                    enemy.Knockback(e0.x - enemy.position.x, e0.z - enemy.position.z, rules.enemyPull, rules.enemyPullTime, dt);
                    (enemy as Husk)?.Launch(rules.yankLift); // enemies.launch?.(e, 4)
                    yankSteps = MovementMath.StepsFor(rules.yankCooldown, dt);
                    pullEvent.id = enemy.id;
                    pullEvent.kind = enemy.kind;
                    pullEvent.x = at.x;
                    pullEvent.y = at.y;
                    pullEvent.z = at.z;
                    events.Emit("arclinePull", pullEvent);
                    return null;
                }
                zips++;
                if (whip33 && IsPullKind(enemy.kind)) whip33Zips++;
                zipEnemy = enemy;
                zipByRay33 = false;
                mover.zipAnchor = ZipPoint;
                return new GrappleTarget { point = at, node = 0, zip = true };
            }
            EnemyRayHit? wr = WhipRay33(e0, dir, out Vec3 wp);
            if (wr.HasValue && whip33BossKind != null && whip33BossKind(wr.Value.enemy.kind))
            {
                // a node in the cone (a ring, a boss joint) still beats the boss body the ray met
                GrappleTarget? n = aim.Choose(e0, dir, world);
                if (n.HasValue && n.Value.node != 0 && n.Value.node != mover.rope.lastNode)
                {
                    fired++;
                    nodes++;
                    return n;
                }
            }
            if (wr.HasValue)
            {
                // NIGHT33 whip33 round 4: nothing in the cone has a clear line to its aim point, but the crosshair's own ray meets an enemy's hit shapes
                // before any wall (THE KILN WYRM: its aim point sits over the head, which is mostly behind the room's cover or under the floor while
                // its exposed pieces are in plain sight): a zip to the point the ray met, carried with the body
                fired++;
                zips++;
                whip33RayZips++;
                Enemy re = wr.Value.enemy;
                if (IsPullKind(re.kind)) whip33Zips++;
                zipEnemy = re;
                zipByRay33 = true;
                zipPoint33 = wp; // the point the ray met, held in the world for the pull (a moving worm's pieces do not follow its head's offset)
                mover.zipAnchor = ZipPoint;
                return new GrappleTarget { point = wp, node = 0, zip = true };
            }
            GrappleTarget? t = aim.Choose(e0, dir, world);
            if (!t.HasValue)
            {
                misses++;
                events.Emit("arclineMiss");
                return null;
            }
            fired++;
            if (t.Value.node != 0) nodes++;
            else surfaces++;
            return t;
        }

        /// <summary>mover.js getPoint: where the zip's enemy is now (null once it is dead: the anchor stays).</summary>
        Vec3? ZipPoint() => zipEnemy != null && zipEnemy.alive ? (zipByRay33 ? zipPoint33 : zipEnemy.AimPoint) : (Vec3?)null;

        // NIGHT33 whip33 round 4: a zip taken by the crosshair's ray (WhipRay33): the point the ray met, held where it was
        bool zipByRay33;
        Vec3 zipPoint33;

        /// <summary>NIGHT33 whip33: the current zip's anchor is a point on the enemy's hit shapes (taken by the crosshair's ray), not its aim point.</summary>
        public bool ZipByRay33 => zipByRay33;

        /// <summary>NIGHT33 whip33: the run's zips taken by the crosshair's ray (the harness reads it).</summary>
        public int whip33RayZips;

        /// <summary>NIGHT33 whip33 round 4 (null with the row off): the enemy whose hit shapes the crosshair's ray meets first, in the hook's range
        /// (config.arcline.range, read live: grapple32 writes it per run) and at least 1 m out, with no wall before it; a small enemy only while the yank
        /// cools (a ready one is ChooseEnemy's yank). `point`: where the ray met it.</summary>
        EnemyRayHit? WhipRay33(Vec3 e0, Vec3 dir, out Vec3 point)
        {
            point = default;
            if (!whip33) return null;
            EnemyRayHit? h = enemies.Raycast(e0, dir, mc33.arcline.range);
            if (!h.HasValue || h.Value.distance < 1) return null;
            if (world.RayIntersect(e0, dir, out RayHit wall) && wall.distance < h.Value.distance) return null;
            if (IsPullKind(h.Value.enemy.kind) && yankSteps <= 0) return null;
            point = e0 + dir * h.Value.distance;
            return h;
        }

        /// <summary>The HUD's hook reticle (arcline.js update): what the crosshair has now, without firing anything.</summary>
        public Arcline21Kind Reticle(InputFrame look)
        {
            if (!mover.ropeEnabled) return Arcline21Kind.None;
            if (mover.rope.phase != RopePhase.None) return Arcline21Kind.None;
            Vec3 e0 = eye();
            Vec3 dir = GrappleAim.AimDirection(look.yaw, look.pitch);
            if (aim.ChooseEnemy(e0, dir, world, enemies, yankOnCooldown) != null) return Arcline21Kind.Enemy;
            GrappleTarget? t = aim.Choose(e0, dir, world);
            return !t.HasValue ? Arcline21Kind.None : t.Value.node != 0 ? Arcline21Kind.Node : Arcline21Kind.Surface;
        }

        /// <summary>
        /// NIGHT31 item 1b (builder G, design.n31.grappleMark): the point a press would take this frame (an enemy's aim point, a node, a surface) and its
        /// kind, for the cyan target mark; None when the rope is out, on its cooldown, or nothing is in reach. Reads only (the pick's own tests, no
        /// counters, no events); the yank's cooldown skips small enemies exactly as Pick does.
        /// </summary>
        public Arcline21Kind Peek(InputFrame look, out Vec3 point)
        {
            point = default;
            if (!active || !mover.ropeEnabled || mover.rope.phase != RopePhase.None || mover.rope.cooldownSteps > 0) return Arcline21Kind.None;
            Vec3 e0 = eye();
            Vec3 dir = GrappleAim.AimDirection(look.yaw, look.pitch);
            Enemy enemy = aim.ChooseEnemy(e0, dir, world, enemies, yankOnCooldown);
            if (enemy != null)
            {
                point = enemy.AimPoint;
                return Arcline21Kind.Enemy;
            }
            if (WhipRay33(e0, dir, out Vec3 wp).HasValue) // NIGHT33 whip33: the crosshair's ray on an enemy's hit shapes (null off)
            {
                point = wp;
                return Arcline21Kind.Enemy;
            }
            GrappleTarget? t = aim.Choose(e0, dir, world);
            if (!t.HasValue || (t.Value.node != 0 && t.Value.node == mover.rope.lastNode)) return Arcline21Kind.None; // the Mover refuses the same node too soon
            point = t.Value.point;
            return t.Value.node != 0 ? Arcline21Kind.Node : Arcline21Kind.Surface;
        }

        /// <summary>NIGHT22: the reticle as it would read from another eye point (the chip's NO TARGET scenario looks for a place where the hook has
        /// nothing to bite). Reads only; the rope phase is not checked.</summary>
        public Arcline21Kind ReticleFrom(Vec3 eyePoint, InputFrame look)
        {
            if (!mover.ropeEnabled) return Arcline21Kind.None;
            Vec3 dir = GrappleAim.AimDirection(look.yaw, look.pitch);
            if (aim.ChooseEnemy(eyePoint, dir, world, enemies, yankOnCooldown) != null) return Arcline21Kind.Enemy;
            GrappleTarget? t = aim.Choose(eyePoint, dir, world);
            return !t.HasValue ? Arcline21Kind.None : t.Value.node != 0 ? Arcline21Kind.Node : Arcline21Kind.Surface;
        }

        /// <summary>The mover's rope events on the bus (the web's ropeFire / ropeAttach / ropeRelease), and arcline.js's slingshot.</summary>
        void OnMover(MoverEvent e)
        {
            if (!active) return;
            string name;
            switch (e.type)
            {
                case MoverEventType.RopeFire: name = "ropeFire"; break;
                case MoverEventType.RopeAttach: name = "ropeAttach"; break;
                case MoverEventType.RopeRelease: name = "ropeRelease"; break;
                default: return;
            }
            ropeEvent.x = e.point.x;
            ropeEvent.y = e.point.y;
            ropeEvent.z = e.point.z;
            ropeEvent.distance = e.distance;
            ropeEvent.length = e.length;
            ropeEvent.node = e.node;
            ropeEvent.zip = e.zip;
            ropeEvent.speed = e.speed;
            ropeEvent.attached = e.attached;
            ropeEvent.reason = e.reason;
            events.Emit(name, ropeEvent);
            if (e.type == MoverEventType.RopeRelease && e.attached && e.speed > rules.slingshotSpeed)
            {
                slingshotEvent.speed = e.speed;
                events.Emit("slingshot", slingshotEvent);
                events.Emit("trick", "slingshot"); // styleMeter.js TRICK_EVENTS: slingshot is a trick
            }
        }
    }
}
