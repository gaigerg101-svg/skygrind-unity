// NIGHT43 builder A1 (burrow43; NIGHT43.md card A1, G28): the BURROWER headless, on the real Mover in a box world (TriangleWorld: the web's
// collider, no PhysX), the enemy pools as the game steps them (Husks.Step with the BURROWER's pool on: Husks.burrow43). `n43burrow [all]`:
//   stand    the player stands still: every emergence hurts and throws (want 100 %), the tell -> emergence time (want 0.80 s), the throw's up speed.
//   jump     a jump pressed k s after the tell (k = 0.00 .. 0.75): hurt or safe (a jump in time is safe; one too early lands into it).
//   walk     the player walks away at the tell (want 0 % hurt); dash: a dash at the tell (want 0 %).
//   plate    the player waits on a 0.45 m plate the whole time (want no tell at all: raised plates are safe ground).
//   counter  hits in the window (x2 and one BurrowCounter, it stays out longer), after it (x1), while buried (nothing); the THUNDERCLAP volleys
//            (64 each) to kill one with and without a counter.
//   repeat   the same scripted run twice: identical (deterministic).
// PASS when every want holds. Prints the events' counts (burrowTell43 / burrowEmerge43 / burrowCounter43).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43Burrow(string cmd, string[] args, ref int code)
    {
        if (cmd != "n43burrow") return;
        code = Burrow43Command();
    }

    sealed class Burrow43Rig
    {
        public GameConfig cfg;
        public TriangleWorld world;
        public EventBus events;
        public EnemyRegistry registry;
        public Husks husks;
        public Mover mover;
        public Health health;
        public double dt;
        public int tells, emerges, counters;
        public int lastTellStep = -1, lastEmergeStep = -1, step;
        public List<double> tellToEmerge = new List<double>();
        public double maxUp;
    }

    static Burrow43Rig Burrow43Make(bool plate)
    {
        var r = new Burrow43Rig { cfg = Config() };
        r.dt = 1 / r.cfg.movement.loop.simHz;
        r.world = new TriangleWorld();
        r.world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30)); // the arena's floor (y 0)
        r.world.AddBox(new Vec3(-30, 0, 18), new Vec3(30, 6, 19)); // a wall the head must not pass
        if (plate) r.world.AddBox(new Vec3(-2, 0, -2), new Vec3(2, 0.45, 2)); // a raised metal plate under the player
        r.events = new EventBus();
        r.registry = new EnemyRegistry();
        r.husks = new Husks(r.cfg, r.events, r.registry, r.world, -30);
        r.husks.burrow43 = Burrow43Params.From(r.cfg);
        r.husks.burrowBody43 = r.husks.burrow43.Body(r.cfg, -30);
        r.mover = N40Mover(r.cfg, r.world, r.cfg.Design("n40.ride40.enabled"), null);
        r.mover.Reset(0, plate ? 0.45 : 0, 0);
        r.health = new Health(100000, r.events, "player");
        Burrow43Rig rr = r;
        r.events.On<Burrow43Event>(N43Events.BurrowTell, e => { rr.tells++; rr.lastTellStep = rr.step; });
        r.events.On<Burrow43Event>(N43Events.BurrowEmerge, e =>
        {
            rr.emerges++;
            rr.lastEmergeStep = rr.step;
            if (rr.lastTellStep >= 0) rr.tellToEmerge.Add((rr.step - rr.lastTellStep) * rr.dt);
        });
        r.events.On<Burrow43Event>(N43Events.BurrowCounter, e => rr.counters++);
        return r;
    }

    /// <summary>Spawn one at `at` and run `seconds`; `input(rig, stepsSinceTell)` gives the player's frame (stepsSinceTell -1 before the first tell).</summary>
    static Burrower43 Burrow43Run(Burrow43Rig r, Vec3 at, double seconds, Func<Burrow43Rig, int, InputFrame> input, Action<Burrow43Rig, Burrower43> each = null)
    {
        var b = r.husks.Spawn("burrower", at, r.mover.position) as Burrower43;
        if (b == null) throw new Exception("n43burrow: no burrower spawned (the pool or the floor)");
        int n = (int)Math.Round(seconds / r.dt);
        for (r.step = 0; r.step < n; r.step++)
        {
            int since = r.lastTellStep < 0 ? -1 : r.step - r.lastTellStep;
            r.mover.Step(input(r, since), r.dt);
            b.targetMover = r.mover; // Husks sets it from Players in the game; the one-target step leaves it to us
            double vy = r.mover.velocity.y;
            r.husks.Step(r.mover.position, r.health, r.dt);
            r.maxUp = Math.Max(r.maxUp, r.mover.velocity.y - vy);
            each?.Invoke(r, b);
        }
        return b;
    }

    static InputFrame Burrow43Idle(Burrow43Rig r, int since) => new InputFrame();

    static int Burrow43Command()
    {
        GameConfig cfg0 = Config();
        Burrow43Params p = Burrow43Params.From(cfg0);
        Console.WriteLine($"n43burrow: tell {p.tellSeconds:0.00} s, window {p.windowSeconds:0.00} s, surfaced {p.surfacedSeconds:0.00} s, hunt {p.huntSpeed:0.0} m/s (walk {cfg0.movement.ground.walkSpeed:0.0}), trigger {p.triggerRange:0.0} m, emerge radius {p.emergeRadius:0.0} m, damage {p.damage:0}, hp {p.hp:0}, floor tolerance {p.floorTolerance:0.00} m");
        int red = 0;
        void Want(bool ok, string line)
        {
            Console.WriteLine($"  {line}: {(ok ? "PASS" : "FAIL")}");
            if (!ok) red++;
        }

        // stand: the player stands still 12 s (several attack cycles)
        {
            Burrow43Rig r = Burrow43Make(false);
            Burrower43 b = Burrow43Run(r, new Vec3(6, 0, 0), 12, Burrow43Idle);
            double t2e = r.tellToEmerge.Count > 0 ? r.tellToEmerge[0] : double.NaN;
            Want(r.emerges >= 2 && b.hitsOnPlayer == r.emerges, $"stand: tells {r.tells}, emergences {r.emerges}, hurt {b.hitsOnPlayer}/{r.emerges} (want all), health lost {100000 - r.health.hp:0}");
            Want(Math.Abs(t2e - p.tellSeconds) < r.dt + 1e-9, $"stand: tell -> emergence {t2e:0.000} s (want {p.tellSeconds:0.00})");
            Want(r.maxUp >= p.launchUp * 0.9, $"stand: the throw's up speed {r.maxUp:0.0} m/s (want ~{p.launchUp:0})");
        }

        // jump at k s after the tell
        {
            var line = new List<string>();
            int safeInTime = 0, inTime = 0;
            for (double k = 0; k <= 0.751; k += 0.05)
            {
                Burrow43Rig r = Burrow43Make(false);
                int at = (int)Math.Round(k / r.dt);
                bool pressed = false;
                Burrower43 b = Burrow43Run(r, new Vec3(6, 0, 0), 4.0, (rig, since) =>
                {
                    bool press = !pressed && since >= at;
                    if (press) pressed = true;
                    return new InputFrame { jump = new Button { pressed = press, held = press || (pressed && since < at + 30) } };
                });
                bool hurt = b.hitsOnPlayer > 0;
                line.Add($"{k:0.00}:{(hurt ? "HURT" : "safe")}");
                if (k >= 0.25 - 1e-9 && k <= 0.6 + 1e-9) { inTime++; if (!hurt) safeInTime++; }
            }
            Console.WriteLine("  jump k s after the tell: " + string.Join(" ", line));
            Want(safeInTime == inTime, $"jump: a jump 0.25-0.60 s after the tell is safe {safeInTime}/{inTime}");
        }

        // walk away at the tell; dash at the tell
        {
            Burrow43Rig r = Burrow43Make(false);
            Burrower43 b = Burrow43Run(r, new Vec3(6, 0, 0), 2.2, (rig, since) => since >= 0 ? new InputFrame { moveZ = 1 } : new InputFrame());
            Want(r.emerges >= 1 && b.hitsOnPlayer == 0, $"walk away at the tell: emergences {r.emerges}, hurt {b.hitsOnPlayer} (want 0)");
            Burrow43Rig r2 = Burrow43Make(false);
            bool dashed = false;
            Burrower43 b2 = Burrow43Run(r2, new Vec3(6, 0, 0), 2.2, (rig, since) =>
            {
                bool press = !dashed && since >= 0;
                if (press) dashed = true;
                return new InputFrame { moveX = 1, dash = new Button { pressed = press, held = press } };
            });
            Want(r2.emerges >= 1 && b2.hitsOnPlayer == 0, $"dash at the tell: emergences {r2.emerges}, hurt {b2.hitsOnPlayer} (want 0)");
        }

        // plate: never on its ground
        {
            Burrow43Rig r = Burrow43Make(true);
            Burrower43 b = Burrow43Run(r, new Vec3(6, 0, 0), 8, Burrow43Idle);
            Want(r.tells == 0 && b.hitsOnPlayer == 0, $"plate (0.45 m): tells {r.tells}, hurt {b.hitsOnPlayer} (want 0 and 0)");
        }

        // the wall: the head never crosses it (the player beyond it on the same floor height stays unreached)
        {
            Burrow43Rig r = Burrow43Make(false);
            r.mover.Reset(0, 0, 24);
            Burrower43 b = Burrow43Run(r, new Vec3(0, 0, 10), 6, Burrow43Idle);
            Want(b.trail.z < 18 && r.tells == 0, $"wall: the head stops at z {b.trail.z:0.00} (the wall at 18), tells {r.tells} (want 0)");
        }

        // counter
        {
            Burrow43Rig r = Burrow43Make(false);
            double hpWindow = double.NaN, hpAfter = double.NaN, hpBuried = double.NaN;
            bool buriedTook = true, hitWindow = false, hitAfter = false;
            int surfacedSteps = 0;
            Burrow43Run(r, new Vec3(6, 0, 0), 7, Burrow43Idle, (rig, b) =>
            {
                if (b.Buried && double.IsNaN(hpBuried) && b.state == Burrower43.Hunt)
                {
                    double h0 = b.hp;
                    buriedTook = b.Damage(64, 0.08) || b.hp != h0;
                    hpBuried = b.hp;
                }
                if (b.state == Burrower43.Emerge || b.state == Burrower43.Dive) surfacedSteps++;
                if (b.state == Burrower43.Emerge && b.windowSteps > 0 && !hitWindow)
                {
                    hitWindow = true;
                    double h0 = b.hp;
                    b.Damage(64, 0.08);
                    hpWindow = h0 - b.hp;
                }
                else if (hitWindow && !hitAfter && b.state == Burrower43.Emerge && b.windowSteps == 0)
                {
                    hitAfter = true;
                    double h0 = b.hp;
                    b.Damage(64, 0.08);
                    hpAfter = h0 - b.hp;
                }
            });
            Want(!buriedTook, $"counter: a hit while buried does nothing (took {(buriedTook ? "damage" : "none")})");
            Want(Math.Abs(hpWindow - 128) < 1e-9 && r.counters == 1, $"counter: a volley in the window deals {hpWindow:0} (want 128), counters {r.counters} (want 1)");
            Want(Math.Abs(hpAfter - 64) < 1e-9, $"counter: a volley after the window deals {hpAfter:0} (want 64)");
            double outS = surfacedSteps * r.dt, plain = p.surfacedSeconds + p.diveSeconds;
            Want(outS >= plain + p.counterStaggerSeconds - 2 * r.dt, $"counter: out of the ground {outS:0.00} s (plain {plain:0.00} + the stagger {p.counterStaggerSeconds:0.00})");
            int plainVolleys = (int)Math.Ceiling(p.hp / 64), withCounter = 1 + (int)Math.Ceiling(Math.Max(0, p.hp - 128) / 64);
            Console.WriteLine($"  volleys (64) to kill: {plainVolleys} plain, {withCounter} with the first in the window");
        }

        // repeat: deterministic
        {
            string Run()
            {
                Burrow43Rig r = Burrow43Make(false);
                Burrower43 b = Burrow43Run(r, new Vec3(5, 0, 3), 9, (rig, since) => new InputFrame { moveX = since >= 0 && since < 40 ? 1 : 0 });
                return $"{r.tells}/{r.emerges}/{b.hitsOnPlayer}/{b.trail.x:R}/{b.trail.z:R}/{r.mover.position.x:R}/{r.health.hp:R}";
            }
            string a = Run(), c = Run();
            Want(a == c, $"repeat: {a} == {c}");
        }

        Console.WriteLine(red == 0 ? "n43burrow: GREEN" : $"n43burrow: RED ({red} failing)");
        return red == 0 ? 0 : 1;
    }
}
