// NIGHT43 builder A1 (burrow43, a sim row: design.n43.burrow43, N43Blocks bit 10; NIGHT43.md card A1; GG's G28: "worm-like enemies ... burrow
// underneath the player and emerge aggressively", a timing window with a special reaction; Research/2026-10-16-wave43/research/encounters-arenas-
// web.md section B: Dune's sandworm hunts ground contact, Isaac's lump before the head and a tell re-shown per attack, the derived worm rules
// 0.7-0.9 s of tell, a 0.4-0.6 s exposed apex). The BURROWER, a port-only kind (the web has none):
//   dig     spawned on the floor (spawn33's tells bring it in), it screws itself into the ground (visible and hittable) for digSeconds;
//   hunt    under the floor (untouchable: buried buryDepth below it, so no ray, punch, blast or sphere reaches it): its head (`trail`, on the
//           floor's height: the raised trail the view draws) runs at huntSpeed toward the player's last GROUND CONTACT (the player's feet within
//           floorTolerance of its floor while grounded: airborne or on a raised plate the player is safe and it waits under the last contact);
//   tell    within triggerRange of a grounded player it locks the spot under the player NOW (tellSeconds: the red-orange ring + the rumble;
//           N43Events.BurrowTell) and gathers under it: a jump, a dash out of the ring or a step onto a plate avoids it;
//   emerge  it bursts out at the locked spot (N43Events.BurrowEmerge): a grounded player within emergeRadius takes `damage` and is thrown up
//           (launchUp) and out (launchOut); it stands exposed surfacedSeconds; the first windowSeconds are the COUNTER window: every hit in it
//           deals x counterMult, the first one staggers it (it lies out counterStaggerSeconds longer, N43Events.BurrowCounter);
//   dive    it sinks back (diveSeconds, still hittable), then rests buried restSeconds and hunts again.
// Deterministic: no randomness at all (no Mulberry32 needed: the spot is the player's own contact), whole-step timers (StepsFor). The head never
// moves where there is no floor at its height (a pit, lava, a wall: two rays per step) so it stays in its room. Knockback, launches, parries and
// the watchdog's relocation do nothing to it (it is rooted in the ground). Spawned only while burrow43 is on (Husks.burrow43; off, the kind
// "burrower" is a plain husk, as every unknown kind was). Its numbers: design.n43.burrow43 (Design/unity-config.json, each with its _why).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n43.burrow43's numbers (read once at the sim's init).</summary>
    public sealed class Burrow43Params
    {
        public double hp, radius, height, huntSpeed, digSeconds, tellSeconds, windowSeconds, surfacedSeconds, diveSeconds, restSeconds;
        public double triggerRange, emergeRadius, damage, launchUp, launchOut, floorTolerance, counterMult, counterStaggerSeconds, buryDepth;
        public int maxCount;

        const string Row = "design.n43.burrow43.";

        public static Burrow43Params From(GameConfig c) => new Burrow43Params
        {
            hp = c.Num(Row + "hp"), radius = c.Num(Row + "radius"), height = c.Num(Row + "height"), huntSpeed = c.Num(Row + "huntSpeed"),
            digSeconds = c.Num(Row + "digSeconds"), tellSeconds = c.Num(Row + "tellSeconds"), windowSeconds = c.Num(Row + "windowSeconds"),
            surfacedSeconds = c.Num(Row + "surfacedSeconds"), diveSeconds = c.Num(Row + "diveSeconds"), restSeconds = c.Num(Row + "restSeconds"),
            triggerRange = c.Num(Row + "triggerRange"), emergeRadius = c.Num(Row + "emergeRadius"), damage = c.Num(Row + "damage"),
            launchUp = c.Num(Row + "launchUp"), launchOut = c.Num(Row + "launchOut"), floorTolerance = c.Num(Row + "floorTolerance"),
            counterMult = c.Num(Row + "counterMult"), counterStaggerSeconds = c.Num(Row + "counterStaggerSeconds"), buryDepth = c.Num(Row + "buryDepth"),
            maxCount = c.Int(Row + "maxCount"),
        };

        /// <summary>Its body through enemyKind.js's base (no head; lift 0.05; aim at 0.6 of its height; flash 0.08), like the other port kinds.</summary>
        public HuskParams Body(GameConfig c, double killPlaneY) => new HuskParams
        {
            kind = "burrower", hp = hp, speed = huntSpeed, radius = radius, height = height, aimHeight = height * 0.6, spawnLift = 0.05, hitFlashTime = 0.08,
            edgeNormalY = c.Num("ai.edgeNormalY"), edgeRiseSpeed = c.Num("ai.edgeRiseSpeed"), supportedDrop = c.Num("ai.supportedDrop"),
            floorMinNormalY = c.movement.surface.floorMinNormalY, killPlaneY = killPlaneY,
        };
    }

    public sealed class Burrower43 : Husk
    {
        public const string Dig = "dig", Hunt = "hunt", Tell = "tell", Emerge = "emerge", Dive = "dive", Rest = "rest";

        /// <summary>dig | hunt | tell | emerge | dive | rest.</summary>
        public string state = Dig;
        public int stateSteps, stateTotal;
        /// <summary>The emergence's counter window left (steps).</summary>
        public int windowSteps;
        /// <summary>Countered in this emergence (the first hit in the window).</summary>
        public bool countered;
        /// <summary>Its floor's height (the spawn's feet): the trail runs on it, the player's ground contact is measured against it.</summary>
        public double floorY;
        /// <summary>Its head under the floor (on floorY), and last step's (the view interpolates).</summary>
        public Vec3 trail, trailPrevious;
        /// <summary>The spot the tell locked (the emergence's centre).</summary>
        public Vec3 tellAt;
        /// <summary>The player's last ground contact on its floor (valid once `hasContact`).</summary>
        public Vec3 contact;
        public bool hasContact;
        /// <summary>The target's mover this step (the throw; its grounded state); null: none (the tests' one-target step).</summary>
        public Mover targetMover;
        /// <summary>Emergences, hits on the player, counters (the harness's counts).</summary>
        public int emerges, hitsOnPlayer, counters;
        public double drawAge; // s alive (visual)

        readonly Burrow43Params b;
        readonly EventBus events;
        readonly Burrow43Event said = new Burrow43Event();

        public Burrower43(HuskParams h, Burrow43Params b, EventBus events) : base(h)
        {
            this.b = b;
            this.events = events;
        }

        public Burrow43Params Rules => b;
        public override bool HasHead => false;

        /// <summary>Under the floor: nothing reaches it (rays, punches, blasts, spheres) and nothing it does is seen but its trail.</summary>
        public bool Buried => state == Hunt || state == Tell || state == Rest;

        /// <summary>0..1 of the current state's time gone (the view's rise / sink).</summary>
        public double StateProgress => stateTotal <= 0 ? 1 : 1 - stateSteps / (double)stateTotal;

        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            headshot = false;
            if (Buried) return -1;
            return CombatMath.RayVerticalCapsule(o, d, position, h.height, h.radius);
        }

        public override bool Spawn(int newId, Vec3 pos, Vec3 target, ICollisionWorld world)
        {
            if (!base.Spawn(newId, pos, target, world)) return false;
            floorY = pos.y;
            trail = trailPrevious = new Vec3(pos.x, floorY, pos.z);
            tellAt = trail;
            contact = trail;
            hasContact = false;
            countered = false;
            windowSteps = 0;
            emerges = hitsOnPlayer = counters = 0;
            drawAge = 0;
            Enter(Dig, b.digSeconds);
            return true;
        }

        /// <summary>The sim's step (set by every Tick; the fixed 120 Hz loop's before the first one).</summary>
        double stepDt = 1.0 / 120;

        void Enter(string s, double seconds)
        {
            state = s;
            stateSteps = stateTotal = MovementMath.StepsFor(seconds, stepDt);
        }

        void EnterDt(string s, double seconds, double dt)
        {
            state = s;
            stateSteps = stateTotal = MovementMath.StepsFor(seconds, dt);
        }

        void Say(string name, Vec3 at, double seconds)
        {
            said.enemy = id;
            said.position = at;
            said.seconds = seconds;
            events.Emit(name, said);
        }

        /// <summary>Buried: the body far under the floor (no query reaches it); out: standing on the floor at the head.</summary>
        void Place(bool buried)
        {
            double y = buried ? Math.Max(floorY - b.buryDepth, h.killPlaneY + 2) : floorY;
            position = new Vec3(trail.x, y, trail.z);
            velocity = default;
        }

        /// <summary>The player stands on its ground: grounded (its mover's word when it has one) with the feet within floorTolerance of its floor.</summary>
        public bool OnItsGround(Vec3 feet)
        {
            if (targetMover != null && !targetMover.grounded) return false;
            return Math.Abs(feet.y - floorY) <= b.floorTolerance;
        }

        /// <summary>Floor under `p` at its height and no wall between `from` and `p` (two rays): the head never leaves its room's floor.</summary>
        static bool FloorAt(ICollisionWorld world, Vec3 from, Vec3 p, double floorY)
        {
            var o = new Vec3(p.x, floorY + 0.6, p.z);
            if (!world.RayIntersect(o, new Vec3(0, -1, 0), out RayHit down) || down.distance < 0.4 || down.distance > 0.9) return false;
            double dx = p.x - from.x, dz = p.z - from.z, len = JsMath.Hypot(dx, dz);
            if (len < 1e-9) return true;
            var side = new Vec3(from.x, floorY + 0.4, from.z);
            return !(world.RayIntersect(side, new Vec3(dx / len, 0, dz / len), out RayHit wall) && wall.distance < len + 0.3);
        }

        public override void Teleport(Vec3 spot) { } // the watchdog's relocation: it moves under the floor, never stuck

        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { } // rooted in the ground

        public override void Launch(double up) { }

        public override void Stagger(double time, double dt) { } // a parry: it has no wind-up to cancel

        public override void Stun(double time, double dt)
        {
            if (Buried) return;
            staggerSteps = Math.Max(staggerSteps, MovementMath.StepsFor(time, dt));
        }

        /// <summary>Damage: none while buried; inside the counter window x counterMult, the first such hit a COUNTER (a stagger: it lies out
        /// longer; N43Events.BurrowCounter).</summary>
        public override bool Damage(double amount, double flashTime)
        {
            if (!alive || Buried) return false;
            if (state == Emerge && windowSteps > 0)
            {
                amount *= b.counterMult;
                if (!countered)
                {
                    countered = true;
                    counters++;
                    int lie = MovementMath.StepsFor(b.counterStaggerSeconds, stepDt);
                    stateSteps += lie;
                    stateTotal += lie;
                    staggerSteps = Math.Max(staggerSteps, lie);
                    Say(N43Events.BurrowCounter, position, b.counterStaggerSeconds);
                }
            }
            return base.Damage(amount, flashTime);
        }

        public override void Tick(NavJumper jumper, Vec3 tp, Health target, IReadOnlyList<Enemy> pool, ICollisionWorld world, EventBus bus, double dt, double gravity)
        {
            trailPrevious = trail;
            stepDt = dt;
            if (staggerSteps > 0) staggerSteps--;
            if (windowSteps > 0) windowSteps--;
            bool ground = target != null && !target.Dead && OnItsGround(tp);
            if (ground)
            {
                contact = new Vec3(tp.x, floorY, tp.z);
                hasContact = true;
            }
            double dx = tp.x - trail.x, dz = tp.z - trail.z;
            if (JsMath.Hypot(dx, dz) > 0.01) yaw = JsMath.Atan2(-dx, -dz);
            committed = state == Tell;
            switch (state)
            {
                case Dig:
                    Place(false);
                    if (--stateSteps <= 0)
                    {
                        EnterDt(Rest, 0, dt);
                        Place(true);
                    }
                    break;
                case Rest:
                    Place(true);
                    if (--stateSteps <= 0) state = Hunt;
                    break;
                case Hunt:
                    {
                        if (hasContact) MoveHead(world, contact, dt);
                        Place(true);
                        double d = JsMath.Hypot(tp.x - trail.x, tp.z - trail.z);
                        if (ground && d <= b.triggerRange)
                        {
                            tellAt = new Vec3(tp.x, floorY, tp.z);
                            EnterDt(Tell, b.tellSeconds, dt);
                            Say(N43Events.BurrowTell, tellAt, b.tellSeconds);
                        }
                        break;
                    }
                case Tell:
                    MoveHead(world, tellAt, dt);
                    Place(true);
                    if (--stateSteps <= 0) Burst(tp, target, dt);
                    break;
                case Emerge:
                    Place(false);
                    if (--stateSteps <= 0) EnterDt(Dive, b.diveSeconds, dt);
                    break;
                case Dive:
                    Place(false);
                    if (--stateSteps <= 0)
                    {
                        EnterDt(Rest, b.restSeconds, dt);
                        Place(true);
                    }
                    break;
            }
        }

        /// <summary>The head toward `to` at huntSpeed, only onto floor at its height (no pit, no wall between).</summary>
        void MoveHead(ICollisionWorld world, Vec3 to, double dt)
        {
            double dx = to.x - trail.x, dz = to.z - trail.z, d = JsMath.Hypot(dx, dz);
            if (d < 1e-9) return;
            double step = Math.Min(d, b.huntSpeed * dt);
            var next = new Vec3(trail.x + dx / d * step, floorY, trail.z + dz / d * step);
            if (FloorAt(world, trail, next, floorY)) trail = next;
        }

        /// <summary>The emergence at the locked spot: a grounded player inside emergeRadius is hurt and thrown.</summary>
        void Burst(Vec3 tp, Health target, double dt)
        {
            trail = tellAt;
            Place(false);
            emerges++;
            countered = false;
            windowSteps = MovementMath.StepsFor(b.windowSeconds, dt);
            EnterDt(Emerge, b.surfacedSeconds, dt);
            double dx = tp.x - tellAt.x, dz = tp.z - tellAt.z, d = JsMath.Hypot(dx, dz);
            if (target != null && !target.Dead && d <= b.emergeRadius && OnItsGround(tp))
            {
                bool hit = target.Damage(b.damage, "burrower", position);
                if (hit)
                {
                    dealtDamage = true;
                    hitsOnPlayer++;
                    if (targetMover != null)
                    {
                        double ux = d > 1e-6 ? dx / d : 0, uz = d > 1e-6 ? dz / d : 0;
                        targetMover.Impulse(ux * b.launchOut, b.launchUp, uz * b.launchOut);
                    }
                }
            }
            Say(N43Events.BurrowEmerge, tellAt, b.windowSeconds);
        }
    }
}
