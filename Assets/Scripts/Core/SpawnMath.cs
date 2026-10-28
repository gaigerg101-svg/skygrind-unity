// Port of skygrind/src/enemies/spawnMath.js (p17-night6), line for line, the web's comments kept (NIGHT9 U3-07).
// Pure spawner math (SPEC 2.0 §5.1): wave budget and mix, spawn groups, spawn-point choice and the
// stuck rule. No three.js, no scene access, unit tested. Vectors are plain {x, y, z}.
// - mulberry32 is already ported as the Mulberry32 class (CoreGrid.cs): reused here, not duplicated. The functions
//   that draw take the rng as a Func<double> (Director.cs's convention); pass a cached `rng.Next` delegate.
// - `sp` (config.spawner) is a SpawnParams read once from GameConfig.
// - pickSpawnPoint returns the chosen point's index in `points` (-1 for the web's null), so the caller keeps its own
//   spawn records; `far` is counted instead of filtered into a new array (the same draw picks the same point).
// - stuckStep's `track` {x, y, z, time} is a StuckTrack (mutated, as on the web).
// Golden vectors: PortExport/vectors/spawnMath.json (Golden/GoldenPorted.cs). Web tests: tests/spawner.test.js.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>config.spawner: the numbers spawnMath.js reads.</summary>
    public sealed class SpawnParams
    {
        public double budgetBase, budgetPerWave, huskCost, droneCost, dronesFromWave, droneMaxShare, droneMinFill;
        public double groupMin, groupMax, seedStride;

        public static SpawnParams From(GameConfig cfg, string path = "spawner") => new SpawnParams
        {
            budgetBase = cfg.Num(path + ".budgetBase"),
            budgetPerWave = cfg.Num(path + ".budgetPerWave"),
            huskCost = cfg.Num(path + ".huskCost"),
            droneCost = cfg.Num(path + ".droneCost"),
            dronesFromWave = cfg.Num(path + ".dronesFromWave"),
            droneMaxShare = cfg.Num(path + ".droneMaxShare"),
            droneMinFill = cfg.Num(path + ".droneMinFill"),
            groupMin = cfg.Num(path + ".groupMin"),
            groupMax = cfg.Num(path + ".groupMax"),
            seedStride = cfg.Num(path + ".seedStride"),
        };
    }

    /// <summary>waveComposition's result.</summary>
    public struct WaveMix
    {
        public double budget;
        public double husks;
        public double drones;
    }

    /// <summary>The stuck rule's anchor and timer (spawner.js keeps one per enemy).</summary>
    public sealed class StuckTrack
    {
        public double x, y, z, time;
    }

    public static class SpawnMath
    {
        // mulberry32(seed): new Mulberry32(seed) (CoreGrid.cs).

        /// <summary>Wave budget = base + perWave × wave.</summary>
        public static double WaveBudget(double wave, SpawnParams sp) => sp.budgetBase + sp.budgetPerWave * wave;

        /// <summary>
        /// The wave's mix. Waves before dronesFromWave are Husks only; after that Drones take a seeded share
        /// of the budget, never more than droneMaxShare of it (and at least one Drone).
        /// </summary>
        public static WaveMix WaveComposition(double wave, Func<double> rng, SpawnParams sp)
        {
            double budget = WaveBudget(wave, sp);
            double drones = 0;
            if (wave >= sp.dronesFromWave)
            {
                double maxDrones = Math.Floor((budget * sp.droneMaxShare) / sp.droneCost);
                double fill = sp.droneMinFill + (1 - sp.droneMinFill) * rng();
                drones = JsMath.Min(maxDrones, JsMath.Max(1, JsMath.Round(maxDrones * fill)));
            }
            double husks = Math.Floor((budget - drones * sp.droneCost) / sp.huskCost);
            return new WaveMix { budget = budget, husks = husks, drones = drones };
        }

        /// <summary>
        /// Deal `count` enemies into groups of groupMin..groupMax (as even as possible, seeded group count).
        /// A count below groupMin is one group. Returns the group sizes.
        /// </summary>
        public static List<int> GroupSizes(int count, Func<double> rng, SpawnParams sp)
        {
            if (count <= 0) return new List<int>();
            double fewest = Math.Ceiling(count / sp.groupMax);
            double most = JsMath.Max(fewest, Math.Floor(count / sp.groupMin));
            double groups = fewest + Math.Floor(rng() * (most - fewest + 1));
            double @base = Math.Floor(count / groups);
            double extra = count % groups;
            var sizes = new List<int>((int)groups);
            for (int i = 0; i < groups; i++) sizes.Add((int)(@base + (i < extra ? 1 : 0)));
            return sizes;
        }

        /// <summary>Fisher–Yates shuffle with a seeded rng (returns a new array).</summary>
        public static List<T> Shuffled<T>(IReadOnlyList<T> list, Func<double> rng)
        {
            var @out = new List<T>(list);
            for (int i = @out.Count - 1; i > 0; i--)
            {
                int j = (int)Math.Floor(rng() * (i + 1));
                (@out[i], @out[j]) = (@out[j], @out[i]);
            }
            return @out;
        }

        public const string Husk = "husk";
        public const string Drone = "drone";

        /// <summary>The wave's spawn groups: the mix shuffled and dealt into groups ("husk" | "drone").</summary>
        public static List<List<string>> WaveGroups(double wave, double seed, SpawnParams sp)
        {
            var next = new Mulberry32(seed + wave * sp.seedStride);
            Func<double> rng = next.Next;
            WaveMix mix = WaveComposition(wave, rng, sp);
            var all = new List<string>();
            for (int i = 0; i < mix.husks; i++) all.Add(Husk);
            for (int i = 0; i < mix.drones; i++) all.Add(Drone);
            List<string> kinds = Shuffled(all, rng);
            var groups = new List<List<string>>();
            int at = 0;
            foreach (int size in GroupSizes(kinds.Count, rng, sp))
            {
                groups.Add(kinds.GetRange(at, size));
                at += size;
            }
            return groups;
        }

        /// <summary>
        /// A spawn point at least minDist (horizontal) from the player; the farthest one if none is.
        /// Returns its index in `points`, or -1 (the web's null) when there are none.
        /// </summary>
        public static int PickSpawnPoint(IReadOnlyList<Vec3> points, Vec3 player, double minDist, Func<double> rng)
        {
            if (points.Count == 0) return -1;
            // const far = points.filter((p) => Math.hypot(p.x - player.x, p.z - player.z) >= minDist);
            int far = 0;
            for (int i = 0; i < points.Count; i++) if (IsFar(points[i], player, minDist)) far++;
            if (far > 0)
            {
                // far[Math.floor(rng() * far.length)]
                int k = (int)Math.Floor(rng() * far);
                for (int i = 0; i < points.Count; i++)
                {
                    if (!IsFar(points[i], player, minDist)) continue;
                    if (k-- == 0) return i;
                }
            }
            int best = 0;
            double bestD = -1;
            for (int i = 0; i < points.Count; i++)
            {
                Vec3 p = points[i];
                double d = JsMath.Hypot(p.x - player.x, p.z - player.z);
                if (d > bestD)
                {
                    best = i;
                    bestD = d;
                }
            }
            return best;
        }

        static bool IsFar(Vec3 p, Vec3 player, double minDist) => JsMath.Hypot(p.x - player.x, p.z - player.z) >= minDist;

        /// <summary>
        /// Stuck rule: an enemy that neither moves `moveDist` from its anchor nor deals damage for `timeout`
        /// seconds is stuck. Mutates `track` {x, y, z, time}.
        /// Returns true when it should be teleported (the track resets).
        /// </summary>
        public static bool StuckStep(StuckTrack track, Vec3 pos, bool dealtDamage, double dt, double moveDist, double timeout)
        {
            double moved = JsMath.Hypot(pos.x - track.x, pos.y - track.y, pos.z - track.z);
            if (moved > moveDist || dealtDamage)
            {
                track.x = pos.x;
                track.y = pos.y;
                track.z = pos.z;
                track.time = 0;
                return false;
            }
            track.time += dt;
            if (track.time < timeout) return false;
            track.time = 0;
            return true;
        }
    }
}
