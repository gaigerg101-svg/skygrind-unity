// NIGHT14 U3-27.11 (agent V2): the GHOST's store (design.ghost; port-only). Each seed's best route (GhostTrack.Better: the
// higher round reached, then the sooner start of it) as <profile folder>/ghosts/<seed code>.bin, at most
// design.ghost.maxSeeds files: past that the oldest best (its header's savedTicks, the store's clock at the save) is
// deleted. A best is written to a .tmp file then swapped in, so a crash never leaves half a ghost. Unity-free.
// The meta rule (NIGHT14 rule 7, and the smoke): a run stores nothing when it is a bot's (metaSilent, then or at its start),
// a LAB run, a resumed run (its route is partial) or automation (noRecords) writing into the player's folder; automation may
// store into its own folder (FolderOverride, which the smoke sets to a temp folder and clears after).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class GhostStore
    {
        /// <summary>Automation (the smoke): ghosts go here instead of the profile's folder, and a noRecords run may store.</summary>
        public static string FolderOverride;

        readonly string profileFolder;
        public readonly int maxSeeds;
        /// <summary>The clock a save stamps (UTC; tests set it).</summary>
        public Func<DateTime> clock = () => DateTime.UtcNow;

        /// <summary>Bests stored since boot (automation reads it).</summary>
        public int Stored { get; private set; }

        public GhostStore(string profileFolder, int maxSeeds)
        {
            this.profileFolder = profileFolder ?? ".";
            this.maxSeeds = Math.Max(1, maxSeeds);
        }

        /// <summary>The folder ghosts live in now (FolderOverride, else the profile's ghosts folder).</summary>
        public string Folder => !string.IsNullOrEmpty(FolderOverride) ? FolderOverride : Path.Combine(profileFolder, UnityTuning.GhostFolderName);

        public string PathFor(uint seed) => Path.Combine(Folder, Seeds.SeedCode(seed) + ".bin");

        /// <summary>The seed's stored best, or null (none, unreadable, another format, another seed's file).</summary>
        public GhostTrack Load(uint seed)
        {
            string path = PathFor(seed);
            if (!File.Exists(path)) return null;
            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    GhostTrack g = GhostTrack.Read(fs);
                    return g != null && g.seed == seed ? g : null;
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <summary>Store `candidate` as its seed's best if it beats the stored one (then prune); true when stored.</summary>
        public bool Offer(GhostTrack candidate)
        {
            if (candidate == null || candidate.count <= 0) return false;
            if (!GhostTrack.Better(candidate, Load(candidate.seed))) return false;
            candidate.savedTicks = clock().Ticks;
            string folder = Folder;
            Directory.CreateDirectory(folder);
            string path = PathFor(candidate.seed), tmp = path + ".tmp";
            using (FileStream fs = File.Create(tmp)) candidate.Write(fs);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
            Stored++;
            Prune();
            return true;
        }

        /// <summary>At most maxSeeds bests: the oldest (savedTicks, then the name) deleted. Returns how many went.</summary>
        public int Prune()
        {
            string folder = Folder;
            if (!Directory.Exists(folder)) return 0;
            string[] files = Directory.GetFiles(folder, "*.bin");
            if (files.Length <= maxSeeds) return 0;
            var aged = new List<(long ticks, string path)>(files.Length);
            foreach (string f in files)
            {
                long ticks = long.MinValue; // unreadable: the first to go
                try
                {
                    using (FileStream fs = File.OpenRead(f)) ticks = GhostTrack.ReadSavedTicks(fs);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                aged.Add((ticks, f));
            }
            aged.Sort((a, b) =>
            {
                int c = a.ticks.CompareTo(b.ticks);
                return c != 0 ? c : string.CompareOrdinal(a.path, b.path);
            });
            int drop = aged.Count - maxSeeds, gone = 0;
            for (int i = 0; i < drop; i++)
            {
                try
                {
                    File.Delete(aged[i].path);
                    gone++;
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            return gone;
        }

        /// <summary>The meta rule: a bot's run, a LAB run, a resumed run, or automation outside its own folder stores nothing.</summary>
        public static bool MayStore(bool metaSilent, bool noRecords, bool lab, bool resumed, bool overrideFolder) =>
            !metaSilent && !lab && !resumed && (!noRecords || overrideFolder);

        /// <summary>The meta rule for a composition's ended run and its route.</summary>
        public static bool MayStore(SurvivalSim sim, RouteTrack track) =>
            MayStore(sim.metaSilent || (track != null && track.metaSilentAtStart), sim.noRecords, sim.lab.Any, track != null && track.resumed, !string.IsNullOrEmpty(FolderOverride));
    }
}
