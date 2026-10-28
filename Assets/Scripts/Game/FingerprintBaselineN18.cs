// NIGHT19 (the shared commit): NIGHT18's fingerprint baseline, kept for good as the off path's: with every NIGHT19 design
// block off (SimHarness `fingerprint --off19`, the Player's `-skygrindFingerprint -skygrindOff19`) the composed run must
// print these at every Core commit (NIGHT19.md rule 3), however FingerprintBaseline is re-recorded at the close.
// Copied from FingerprintBaseline.Hashes at u3-n18 (NIGHT17 U3-30.4's re-record; build 7 printed them EQUAL at 13:12).
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static class FingerprintBaselineN18
    {
        public static readonly string[] Hashes =
        {
            "step 600 hash 861ba038a88e3bae",
            "step 1200 hash e1c6664558c75aeb",
            "step 1800 hash 505e3f4d741037ff",
            "step 2400 hash 6db6616809b8acf8",
            "step 3000 hash eebaf72ba408a9db",
            "step 3600 hash 369086f6460fc0ab",
            "step 4200 hash 4ef1cd9cbc532db7",
            "step 4800 hash 1d1e5c149c0cf32f",
            "step 5400 hash 117d9786c5108a78",
            "step 6000 hash aee6f161460fc1a1",
            "step 6600 hash bcc0508633c6d6d0",
            "step 7200 hash 645ac080e7aa00cb",
        };

        /// <summary>The first checkpoint that differs from NIGHT18's baseline (-1: all equal; a length difference counts).</summary>
        public static int FirstDifference(IList<string> lines)
        {
            for (int i = 0; i < Hashes.Length || i < lines.Count; i++)
                if (i >= Hashes.Length || i >= lines.Count || Hashes[i] != lines[i]) return i;
            return -1;
        }
    }
}
