// NIGHT72 builder N, slice N1 (verbs37 row 12, the bunny hop; RED "2 hops, speed kept 100 %" on n52-iceline and n57-kiln since ultra71): the row's WINDOW and
// its PLACES. The row needs three Jump events with the horizontal speed kept: a ground jump, then each landing's buffered jump. The ULTRAKILL profile's flat
// jump is 1.4 s and about 12 m, so three hops take about 3-5 s (the bare-Mover dry run `Dry` reads 4.4 s on kiln and spire, and 3 hops are about 26 m down the
// lane): the row's old 4 s window cannot hold them, and the lane Open35 picks (the first room whose longest clear lane is 14 m) can end in a wall.
// The BAR is unchanged (3 hops, each keeping >= 99 % of the speed). What grows: the window (three hops of the profile's air time plus a margin each, never below
// the old 4 s) and, when the row's first run misses the bar, a few more places (the longest clear lanes of the level's rooms) tried in turn, every attempt printed.
// Unity-free: the harness runs the same code (`n72lane`, Tools/SimHarness/Harness.N72.Lane.cs). The dry run is INDICATIVE only: the bare Mover has none of the
// sim's design rows and reads RED on foundry and span, which the Player passes (polish8): the Player's run decides.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class HopSite72
    {
        public struct Run
        {
            public int hops;       // Jump events with a speed before of at least Hop72MinSpeedMps, at most Hop72Hops
            public double minKept; // the smallest speedAfter / speedBefore of the counted hops, percent (0 with none)
            public double seconds; // game seconds the run lasted
            public bool Pass => hops >= UnityTuning.Hop72Hops && minKept >= UnityTuning.Hop72KeptPct;
        }

        /// <summary>The flat-jump air time (s) the profile gives: the rise to the apex and the fall back (the hang is not counted: the margin takes it).</summary>
        public static double AirTime(MovementConfig mc) => mc.jump.apexTime + mc.jump.apexTime / Math.Sqrt(Math.Max(1e-6, mc.jump.fallGravityMult));

        /// <summary>The game seconds the row runs for: the first press, three hops of the profile's air time with a margin each; at least the row's old 4 s.</summary>
        public static double Window(MovementConfig mc) =>
            Math.Max(UnityTuning.Hop72MinWindowS, UnityTuning.Hop72FirstPressS + UnityTuning.Hop72Hops * (AirTime(mc) + UnityTuning.Hop72MarginPerHopS));

        /// <summary>The row's press logic (FeelReel.N35.Verbs.cs row 12) on a fresh bare Mover over `world` from `at` facing `yaw`: placed, 0.35 s to settle, then forward
        /// held with the first press once the walk speed is up and each landing's jump pressed inside the buffer. Indicative only (no design rows).</summary>
        public static Run Dry(MovementConfig mc, ICollisionWorld world, Vec3 at, double yaw, double window)
        {
            double dt = 1 / mc.loop.simHz;
            var m = new Mover(mc, world);
            m.Reset(at.x, at.y, at.z);
            var idle = new InputFrame { yaw = yaw };
            for (int i = 0, n = (int)Math.Round(0.35 / dt); i < n; i++) m.Step(idle, dt);
            var kept = new List<double>();
            int step = 0;
            m.Events += e =>
            {
                if (e.type == MoverEventType.Jump && e.speedBefore > UnityTuning.Hop72MinSpeedMps && kept.Count < UnityTuning.Hop72Hops) kept.Add(100 * e.speedAfter / e.speedBefore);
            };
            bool first = true, pressedAir = false;
            double floorY = m.position.y, heldUntil = -1;
            for (; step * dt < window && kept.Count < UnityTuning.Hop72Hops; step++)
            {
                double t = step * dt;
                bool press = false;
                if (first && m.grounded && t > UnityTuning.Hop72FirstPressS) { press = true; first = false; }
                else if (!first && !m.grounded && m.velocity.y < 0 && !pressedAir && m.position.y - floorY <= -m.velocity.y * UnityTuning.Hop72BufferLeadS) { press = true; pressedAir = true; }
                else if (!first && m.grounded && !pressedAir && t >= heldUntil) press = true;
                if (m.grounded && !press) pressedAir = false;
                if (press) heldUntil = t + UnityTuning.Hop72HeldS;
                var f = new InputFrame { moveZ = 1, yaw = yaw };
                f.jump.pressed = press;
                f.jump.held = press || t < heldUntil;
                m.Step(f, dt);
            }
            double min = 0;
            if (kept.Count > 0) { min = 1e9; foreach (double k in kept) min = Math.Min(min, k); }
            return new Run { hops = kept.Count, minKept = min, seconds = step * dt };
        }

        /// <summary>Open35's lane scan for one room and heading (16 headings from the spawn): the clear lane (rays at 0.5 m and 1.5 m, at most `maxClear`; the floor within
        /// 1.6 m under it every 2 m): its length and the yaw.</summary>
        public static (double len, double yaw) RoomLane(ICollisionWorld world, Vec3 spawn, int heading, double maxClear)
        {
            double yaw = heading * Math.PI / 8, fx = -Math.Sin(yaw), fz = -Math.Cos(yaw), clear = maxClear;
            foreach (double h in new[] { 0.5, 1.5 })
                if (world.RayIntersect(new Vec3(spawn.x, spawn.y + h, spawn.z), new Vec3(fx, 0, fz), out RayHit hit)) clear = Math.Min(clear, hit.distance);
            double len = 0;
            for (double d = 2; d <= clear - 1.5; d += 2)
            {
                if (!world.RayIntersect(new Vec3(spawn.x + fx * d, spawn.y + 1.0, spawn.z + fz * d), new Vec3(0, -1, 0), out RayHit dh) || dh.distance > 1.6) break;
                len = d;
            }
            return (len, yaw);
        }

        /// <summary>Other places for the hop row, longest clear lane first: every room spawn in `spawns` (the non-boss rooms from the first award room, as Open35 walks them) and
        /// heading whose lane is at least Hop72MinLaneM, except the place Open35 picked (`skipAt`, `skipYaw`); at most Hop72Retries. Each is placed 0.2 m over its spawn, as Open35.</summary>
        public static List<(Vec3 at, double yaw, int room, double lane)> Candidates(ICollisionWorld world, IList<Vec3> spawns, Vec3 skipAt, double skipYaw)
        {
            var all = new List<(Vec3 at, double yaw, int room, double lane)>();
            for (int r = 0; r < spawns.Count; r++)
                for (int k = 0; k < 16; k++)
                {
                    var (len, yaw) = RoomLane(world, spawns[r], k, UnityTuning.Hop72ScanM);
                    var at = new Vec3(spawns[r].x, spawns[r].y + 0.2, spawns[r].z);
                    bool isOpen = Math.Abs(at.x - skipAt.x) < 0.01 && Math.Abs(at.z - skipAt.z) < 0.01 && Math.Abs(yaw - skipYaw) < 1e-6;
                    if (len >= UnityTuning.Hop72MinLaneM && !isOpen) all.Add((at, yaw, r, len));
                }
            all.Sort((a, b) => b.lane != a.lane ? b.lane.CompareTo(a.lane) : a.room != b.room ? a.room.CompareTo(b.room) : a.yaw.CompareTo(b.yaw));
            if (all.Count > UnityTuning.Hop72Retries) all.RemoveRange(UnityTuning.Hop72Retries, all.Count - UnityTuning.Hop72Retries);
            return all;
        }
    }
}
