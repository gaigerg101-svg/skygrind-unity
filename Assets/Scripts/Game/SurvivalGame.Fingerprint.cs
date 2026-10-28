// NIGHT13 U3-26.2: the composed fingerprint's driver in the Player (SimFingerprint holds the hash and the input script;
// NIGHT13 U3-26.6: SurvivalSim.RunFingerprint runs it, the same code the headless FingerprintTests runs). Pinned so the
// Player, the Editor and the headless test agree: OnScreen answers false (the watchdog's camera read), the settings are
// their defaults (the store only: nothing is saved or applied), the profile is a fresh one (no unlocks: the MODS pool
// reads them; the sim's records follow it), no LAB, no records, no profile write; a plain Survival run at HEAT 0, every
// step in sequence within one frame (the frame loop's accumulator and hitstop are the frame's, not the composition's).
// Everything is restored after, and the game goes back to TITLE.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        static bool OffScreen(Vec3 p) => false;

        /// <summary>The last fingerprint run's end state in words (the log's line; not part of the hashes).</summary>
        public string FingerprintSummary { get; private set; }

        /// <summary>One fingerprint run: "step N hash H" lines, one per SimFingerprint.Every steps.</summary>
        public List<string> RunFingerprint(uint seed, int steps = SimFingerprint.Steps, int every = SimFingerprint.Every)
        {
            string savedSettings = settings.store.Save();
            settings.store.Load("");
            var savedProfile = new Profile();
            savedProfile.CopyFrom(profile);
            profile.CopyFrom(new Profile());
            sim.records = profile.records; // CopyFrom replaces profile.records; the MODS unlock and EndRun read the sim's field
            bool ro = profileReadOnly;
            profileReadOnly = true;
            // NIGHT14 review A-4: a live tape ends first (its onScreen tap removed, the camera's test back) and the fingerprint's
            // own run isn't taped; before, its tap wrapped OffScreen and the camera's test never came back for the session.
            if (tapeRecorder != null && tapeRecorder.Recording) tapeRecorder.Finish(TapeEnd.Abandoned);
            tapeSuspended = true;
            System.Func<Vec3, bool> onScreen = brain.onScreen;
            brain.onScreen = OffScreen;
            try
            {
                List<string> lines = sim.RunFingerprint(seed, steps, every, out string summary);
                FingerprintSummary = summary;
                return lines;
            }
            finally
            {
                tapeSuspended = false;
                brain.onScreen = onScreen;
                ToTitle();
                profile.CopyFrom(savedProfile);
                sim.records = profile.records; // the restored records (the object the profile saves)
                settings.store.Load(savedSettings);
                profileReadOnly = ro;
            }
        }
    }
}
