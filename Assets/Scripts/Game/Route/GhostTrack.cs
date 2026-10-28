// NIGHT14 U3-27.11 (agent V2): the GHOST's track (design.ghost; port-only: the web has no ghost). A seed's best route as
// stored and replayed: the run's coarse points (RouteTrack's: feet x, y, z and speed, one every everySteps run steps from
// startStep) and the run step each round began at. Unity-free.
// Best (D-53's store rule): the higher round reached (the last round that BEGAN) wins; on the same round, the one that
// began it sooner (fewer run steps); a tie keeps the stored one; a run that began no round never replaces a best.
// Replay: Sample(runStep) interpolates the points by the run's clock (before the start: the first point; past the last
// point: none, the best ended there). Split(round, runStep): (runStep - the best's start of that round) / simHz s;
// negative = ahead.
// The file (little-endian, BinaryWriter): "SKGH" magic, the format version, seed, heat, savedTicks (UTC ticks: the store
// drops the oldest), everySteps, simHz, startStep, count, roundCount, then x, y, z, speed per point (float32) and (round,
// step) per round (int32). Read refuses another magic or version and a short file (null, never a throw).
using System;
using System.IO;

namespace Skygrind.Game
{
    public sealed class GhostTrack
    {
        public const int FileMagic = 0x48474B53; // "SKGH" as a little-endian int32
        public const int FileVersion = 1;
        const int MaxPoints = 1 << 22, MaxRounds = 1 << 16; // a garbled header never allocates more than this

        public uint seed;
        public int heat;
        public long savedTicks;
        public int everySteps;
        public double simHz;
        public int startStep, count, roundCount;
        public readonly float[] px, py, pz, speed;
        public readonly int[] roundNumber, roundStep;

        public GhostTrack(int points, int rounds)
        {
            px = new float[Math.Max(0, points)];
            py = new float[px.Length];
            pz = new float[px.Length];
            speed = new float[px.Length];
            roundNumber = new int[Math.Max(0, rounds)];
            roundStep = new int[roundNumber.Length];
        }

        /// <summary>The last round that began (0: none).</summary>
        public int ReachedRound => roundCount > 0 ? roundNumber[roundCount - 1] : 0;

        /// <summary>The run step it began at (int.MaxValue: none began).</summary>
        public int ReachedStep => roundCount > 0 ? roundStep[roundCount - 1] : int.MaxValue;

        /// <summary>The last point's run step.</summary>
        public int EndStep => startStep + Math.Max(0, count - 1) * everySteps;

        /// <summary>A copy of a run's route (its points and round starts), to store as its seed's best.</summary>
        public static GhostTrack From(RouteTrack t)
        {
            var g = new GhostTrack(t.count, t.roundCount)
            {
                seed = t.seed,
                heat = t.heat,
                everySteps = t.everySteps,
                simHz = t.simHz,
                startStep = t.startStep,
                count = t.count,
                roundCount = t.roundCount,
            };
            Array.Copy(t.px, g.px, t.count);
            Array.Copy(t.py, g.py, t.count);
            Array.Copy(t.pz, g.pz, t.count);
            Array.Copy(t.speed, g.speed, t.count);
            Array.Copy(t.roundNumber, g.roundNumber, t.roundCount);
            Array.Copy(t.roundStep, g.roundStep, t.roundCount);
            return g;
        }

        /// <summary>Does `candidate` beat `stored` as the seed's best (null: anything beats nothing)?</summary>
        public static bool Better(GhostTrack candidate, GhostTrack stored)
        {
            if (candidate == null) return false;
            if (stored == null) return true;
            int a = candidate.ReachedRound, b = stored.ReachedRound;
            if (a != b) return a > b;
            if (a == 0) return false; // neither began a round: keep the stored one
            return candidate.ReachedStep < stored.ReachedStep;
        }

        /// <summary>The best's feet at a run step (interpolated); false past its last point (or with no points).</summary>
        public bool Sample(double runStep, out double x, out double y, out double z)
        {
            x = y = z = 0;
            if (count <= 0 || everySteps <= 0) return false;
            double f = (runStep - startStep) / everySteps;
            if (f > count - 1) return false;
            if (f < 0) f = 0;
            int i = (int)Math.Floor(f);
            if (i >= count - 1)
            {
                x = px[count - 1];
                y = py[count - 1];
                z = pz[count - 1];
                return true;
            }
            double t = f - i;
            x = px[i] + (px[i + 1] - (double)px[i]) * t;
            y = py[i] + (py[i + 1] - (double)py[i]) * t;
            z = pz[i] + (pz[i + 1] - (double)pz[i]) * t;
            return true;
        }

        /// <summary>The run step the best began `round` at.</summary>
        public bool RoundStep(int round, out int step)
        {
            for (int i = 0; i < roundCount; i++)
            {
                if (roundNumber[i] != round) continue;
                step = roundStep[i];
                return true;
            }
            step = 0;
            return false;
        }

        /// <summary>This run's start of `round` (at runStep) against the best's, in s (negative: ahead).</summary>
        public bool Split(int round, int runStep, out double seconds)
        {
            seconds = 0;
            if (!(simHz > 0) || !RoundStep(round, out int best)) return false;
            seconds = (runStep - best) / simHz;
            return true;
        }

        public void Write(Stream s)
        {
            using (var w = new BinaryWriter(s, System.Text.Encoding.UTF8, true))
            {
                w.Write(FileMagic);
                w.Write(FileVersion);
                w.Write(seed);
                w.Write(heat);
                w.Write(savedTicks);
                w.Write(everySteps);
                w.Write(simHz);
                w.Write(startStep);
                w.Write(count);
                w.Write(roundCount);
                for (int i = 0; i < count; i++)
                {
                    w.Write(px[i]);
                    w.Write(py[i]);
                    w.Write(pz[i]);
                    w.Write(speed[i]);
                }
                for (int i = 0; i < roundCount; i++)
                {
                    w.Write(roundNumber[i]);
                    w.Write(roundStep[i]);
                }
            }
        }

        /// <summary>A ghost from a stream, or null (another magic or version, a garbled header, a short file).</summary>
        public static GhostTrack Read(Stream s)
        {
            try
            {
                using (var r = new BinaryReader(s, System.Text.Encoding.UTF8, true))
                {
                    if (r.ReadInt32() != FileMagic || r.ReadInt32() != FileVersion) return null;
                    uint seed = r.ReadUInt32();
                    int heat = r.ReadInt32();
                    long saved = r.ReadInt64();
                    int every = r.ReadInt32();
                    double hz = r.ReadDouble();
                    int start = r.ReadInt32(), count = r.ReadInt32(), rounds = r.ReadInt32();
                    if (count < 0 || count > MaxPoints || rounds < 0 || rounds > MaxRounds || every <= 0) return null;
                    var g = new GhostTrack(count, rounds)
                    {
                        seed = seed, heat = heat, savedTicks = saved, everySteps = every, simHz = hz, startStep = start, count = count, roundCount = rounds,
                    };
                    for (int i = 0; i < count; i++)
                    {
                        g.px[i] = r.ReadSingle();
                        g.py[i] = r.ReadSingle();
                        g.pz[i] = r.ReadSingle();
                        g.speed[i] = r.ReadSingle();
                    }
                    for (int i = 0; i < rounds; i++)
                    {
                        g.roundNumber[i] = r.ReadInt32();
                        g.roundStep[i] = r.ReadInt32();
                    }
                    return g;
                }
            }
            catch (EndOfStreamException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        /// <summary>Only the header's savedTicks (the store's age order); long.MinValue when unreadable.</summary>
        public static long ReadSavedTicks(Stream s)
        {
            try
            {
                using (var r = new BinaryReader(s, System.Text.Encoding.UTF8, true))
                {
                    if (r.ReadInt32() != FileMagic || r.ReadInt32() != FileVersion) return long.MinValue;
                    r.ReadUInt32();
                    r.ReadInt32();
                    return r.ReadInt64();
                }
            }
            catch (IOException)
            {
                return long.MinValue;
            }
        }
    }
}
