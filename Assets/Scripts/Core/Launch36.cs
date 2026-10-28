// NIGHT36 builder P (launch36; NIGHT36.md card P1, PLAN.md section 1 "speed pays"; GG 2026-10-12: "explosions"): a punch thrown while
// dashing, sliding, wall riding or pulled / swung by the grapple (or within graceS of one) LAUNCHES the body it hits along the punch: a
// horizontal speed plus a lift instead of the punch's knockback. A launched body that meets another live body, or a wall, BURSTS:
// burstDamage to every enemy within burstRadius of the point (the flier too), never the player. A flight that lands or runs out of
// flightS ends with no burst. Only husk-shaped bodies fly (Husk and its kinds; never a stand-in, a tank or a boss body).
// Event "launch36" (the launch) and "launch36Burst" (Launch36BurstEvent) for GG's sounds.
// Numbers: design.n36.launch36 (never here). Off (Loadout.launch36 null): the punch's knockback as before, bit for bit.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>A burst (launch36Burst, parry36Burst): where, how many bodies it hit and killed, and whether a wall set it off.</summary>
    public sealed class Launch36BurstEvent
    {
        public Vec3 position;
        public int hits, kills;
        public bool wall;
    }

    public sealed class Launch36
    {
        public const string LaunchEvent = "launch36", BurstEvent = "launch36Burst";

        public sealed class Flight
        {
            public Husk body;
            public double dx, dz; // the launch's horizontal unit direction
            public int age;
            public Vec3 from, last; // where it was launched from, and its position at the last check (the swept body test)
            public double minGap; // the closest any other body's surface came (m; the reel prints it)
        }

        public readonly double speed, lift, burstDamage, burstRadius, hitPad, wallProbe, wallMaxNormalY, wallProbeLift;
        public readonly int flightSteps, graceSteps, armSteps;

        public readonly List<Flight> flights = new List<Flight>();
        readonly List<Flight> pool = new List<Flight>();
        readonly List<Enemy> scratch = new List<Enemy>();

        /// <summary>Steps left of the grace after a speed verb ended.</summary>
        public int graceLeft;

        /// <summary>Counters (the harness, the reels).</summary>
        public int launches, bursts, wallBursts, burstHits, burstKills, landed;

        public Vec3 lastBurst;

        /// <summary>The last flight's end: "body", "wall", "landed", "timeout" or "dead"; how far it flew (m, flat) and its closest gap to another body (m).</summary>
        public string lastEnd = "";
        public double lastThrown, lastMinGap;

        public Launch36(GameConfig cfg)
        {
            const string p = "design.n36.launch36.";
            speed = cfg.Num(p + "speed");
            lift = cfg.Num(p + "lift");
            burstDamage = cfg.Num(p + "burstDamage");
            burstRadius = cfg.Num(p + "burstRadius");
            hitPad = cfg.Num(p + "hitPad");
            wallProbe = cfg.Num(p + "wallProbe");
            wallMaxNormalY = cfg.Num(p + "wallMaxNormalY");
            wallProbeLift = cfg.Num(p + "wallProbeLift");
            flightSteps = MovementMath.StepsFor(cfg.Num(p + "flightS"), cfg.Dt);
            graceSteps = MovementMath.StepsFor(cfg.Num(p + "graceS"), cfg.Dt);
            armSteps = MovementMath.StepsFor(cfg.Num(p + "armS"), cfg.Dt);
        }

        public void Reset()
        {
            foreach (Flight f in flights)
            {
                f.body = null;
                pool.Add(f);
            }
            flights.Clear();
            graceLeft = 0;
            launches = bursts = wallBursts = burstHits = burstKills = landed = 0;
            lastBurst = default;
            lastEnd = "";
            lastThrown = lastMinGap = 0;
        }

        /// <summary>Is the mover in a speed verb right now (a dash, a slide, a wall ride, the grapple's pull or swing)?</summary>
        public static bool SpeedVerb(Mover m) =>
            m.state == MoveState.Dash || m.state == MoveState.Slide || m.state == MoveState.WallRide || m.state == MoveState.Swing || m.rope.phase == RopePhase.Attached;

        /// <summary>Once per step after the punch (SurvivalSim.Core36): the grace clock.</summary>
        public void Track(Mover m)
        {
            if (SpeedVerb(m)) graceLeft = graceSteps;
            else if (graceLeft > 0) graceLeft--;
        }

        /// <summary>Does a punch landing on `e` now launch it?</summary>
        public bool Wants(Mover m, Enemy e) => e is Husk h && !h.standIn29 && !h.tank29 && (SpeedVerb(m) || graceLeft > 0);

        public bool Flying(Enemy e)
        {
            for (int i = 0; i < flights.Count; i++) if (flights[i].body == e) return true;
            return false;
        }

        /// <summary>Launch a body along (dirX, dirZ) (the punch's aim, flattened).</summary>
        public void Launch(Enemy e, double dirX, double dirZ, double dt, EventBus events)
        {
            if (!(e is Husk h) || !h.alive) return;
            double len = JsMath.Hypot(dirX, dirZ);
            if (len == 0 || double.IsNaN(len)) return;
            dirX /= len;
            dirZ /= len;
            h.Knockback(dirX, dirZ, speed, flightSteps * dt, dt);
            h.Launch(lift);
            Flight f = null;
            for (int i = 0; i < flights.Count; i++) if (flights[i].body == h) f = flights[i];
            if (f == null)
            {
                if (pool.Count > 0)
                {
                    f = pool[pool.Count - 1];
                    pool.RemoveAt(pool.Count - 1);
                }
                else f = new Flight();
                flights.Add(f);
            }
            f.body = h;
            f.dx = dirX;
            f.dz = dirZ;
            f.age = 0;
            f.from = f.last = h.position;
            f.minGap = double.PositiveInfinity;
            launches++;
            events.Emit(LaunchEvent, Boxes.Int(h.id));
        }

        /// <summary>One step after the enemies moved (SurvivalSim.Core36): each flight meets a body (burst), a wall (burst), lands or times out.</summary>
        public void Step(ICollisionWorld world, EnemyRegistry enemies, Func<Enemy, double, Vec3, bool> damage, EventBus events)
        {
            for (int i = flights.Count - 1; i >= 0; i--)
            {
                Flight f = flights[i];
                Husk h = f.body;
                f.age++;
                bool end = h == null || !h.alive;
                string why = "dead";
                if (!end)
                {
                    Enemy other = Touching(h, enemies, f);
                    if (other != null)
                    {
                        Vec3 a = h.AimPoint, b = other.AimPoint;
                        Burst(new Vec3((a.x + b.x) * 0.5, (a.y + b.y) * 0.5, (a.z + b.z) * 0.5), false, enemies, damage, events);
                        end = true;
                        why = "body";
                    }
                    else if (f.age > armSteps && HitsWall(world, h, f))
                    {
                        Burst(h.AimPoint, true, enemies, damage, events);
                        end = true;
                        why = "wall";
                    }
                    else if (f.age >= flightSteps || (f.age > armSteps && h.grounded && h.knockSteps == 0))
                    {
                        landed++;
                        end = true;
                        why = f.age >= flightSteps ? "timeout" : "landed";
                    }
                    f.last = h.position;
                }
                if (!end) continue;
                lastEnd = why;
                lastThrown = h != null ? JsMath.Hypot(f.last.x - f.from.x, f.last.z - f.from.z) : 0;
                lastMinGap = f.minGap;
                f.body = null;
                flights.RemoveAt(i);
                pool.Add(f);
            }
        }

        /// <summary>The first other live body the flier's capsule touched this step (swept: the flat segment from its last position to now
        /// within r1 + r2 + hitPad of the other's axis, overlapping heights).</summary>
        Enemy Touching(Husk h, EnemyRegistry enemies, Flight f)
        {
            double sx = h.position.x - f.last.x, sz = h.position.z - f.last.z, len2 = sx * sx + sz * sz;
            for (int i = 0; i < enemies.all.Count; i++)
            {
                Enemy o = enemies.all[i];
                if (o == h || !o.alive) continue;
                double reach = h.Radius + o.Radius + hitPad;
                double t = len2 > 0 ? ((o.position.x - f.last.x) * sx + (o.position.z - f.last.z) * sz) / len2 : 1;
                t = Math.Max(0, Math.Min(1, t));
                double flat = JsMath.Hypot(o.position.x - (f.last.x + sx * t), o.position.z - (f.last.z + sz * t));
                f.minGap = Math.Min(f.minGap, flat - h.Radius - o.Radius);
                if (flat > reach) continue;
                if (o.position.y > h.position.y + h.Height + hitPad || h.position.y > o.position.y + o.Height + hitPad) continue;
                return o;
            }
            return null;
        }

        /// <summary>A wall ahead along the flight: the body's capsule moved wallProbe m along it touches a face steeper than wallMaxNormalY.</summary>
        bool HitsWall(ICollisionWorld world, Husk h, Flight f)
        {
            Capsule c = h.Capsule;
            var off = new Vec3(f.dx * wallProbe, wallProbeLift, f.dz * wallProbe); // lifted off the floor: a sliding body's floor contact would bend the normal up
            c.start = c.start + off;
            c.end = c.end + off;
            return WorldCollider.CapsuleIntersect(world, c, out Vec3 n, out _) && Math.Abs(n.y) < wallMaxNormalY;
        }

        void Burst(Vec3 at, bool wall, EnemyRegistry enemies, Func<Enemy, double, Vec3, bool> damage, EventBus events)
        {
            bursts++;
            if (wall) wallBursts++;
            lastBurst = at;
            enemies.Within(at, burstRadius, scratch);
            int hits = 0, kills = 0;
            for (int i = 0; i < scratch.Count; i++)
            {
                Enemy e = scratch[i];
                if (!e.alive) continue;
                hits++;
                if (damage(e, burstDamage, at)) kills++;
            }
            scratch.Clear();
            burstHits += hits;
            burstKills += kills;
            events.Emit(BurstEvent, new Launch36BurstEvent { position = at, hits = hits, kills = kills, wall = wall });
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT36 P launch36 (SurvivalSim.Core36 sets it at a run's start; null: off, the punch's knockback).</summary>
        public Launch36 launch36;

        /// <summary>launch36's burst damage, as the loadout ("launch36": no momentum, no catalog weapon).</summary>
        public bool Launch36Damage(Enemy e, double amount, Vec3 at) => Damage(e, amount, false, "launch36", at);
    }
}
