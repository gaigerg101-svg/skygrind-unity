// NIGHT21 (the lead, at the freeze): NIGHT20's fingerprint baseline, kept for good as the NIGHT21 off path's: with every NIGHT21
// design block off (SimHarness `fingerprint --off21`, the Player's `-skygrindFingerprint -skygrindOff21`) the composed run must
// print these at every Core commit, however FingerprintBaseline is re-recorded at the close (NIGHT21.md "Switches").
// Copied from FingerprintBaseline.Hashes at 12:4x on 2026-10-07 (FingerprintBaselineN20: u3-n20's re-record; round 4, 36 kills,
// 80 shots, 4775 earned, 8 alive at step 7200).
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static class FingerprintBaselineN20
    {
        public static readonly string[] Hashes =
        {
            "step 600 hash 31263654d2bba9e3",
            "step 1200 hash 1083e675b145833b",
            "step 1800 hash b2bef3b5d4a597da",
            "step 2400 hash 82f78264a324364c",
            "step 3000 hash 5e47a1efaf7815e1",
            "step 3600 hash e9c8f2afc3417f6c",
            "step 4200 hash 36b8cadc8676c84d",
            "step 4800 hash b73b13280d906e2b",
            "step 5400 hash 49e4d9fda592df02",
            "step 6000 hash 1ea8f27a5d543adf",
            "step 6600 hash 569e4baf1a9a9c19",
            "step 7200 hash eba403c524cc3cd0",
        };

        /// <summary>The first checkpoint that differs from NIGHT20's baseline (-1: all equal; a length difference counts).</summary>
        public static int FirstDifference(IList<string> lines)
        {
            for (int i = 0; i < Hashes.Length || i < lines.Count; i++)
                if (i >= Hashes.Length || i >= lines.Count || Hashes[i] != lines[i]) return i;
            return -1;
        }
    }
}
