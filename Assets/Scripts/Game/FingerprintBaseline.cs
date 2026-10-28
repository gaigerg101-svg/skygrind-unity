// NIGHT13 U3-26.2: the composed fingerprint's baseline (SimFingerprint, seed SimFingerprint.Seed), recorded by the
// Player's -skygrindFingerprint on the unchanged u3-n12 composition (build 1). Every later refactor must print these.
// A deliberate behaviour change (a new flag's default, a Core port that changes a run) records a new baseline in its
// own commit, with the reason in the log.
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static class FingerprintBaseline
    {
        /// <summary>NIGHT19: false until the close re-records this baseline with the NIGHT19 blocks as configured; until then the
        /// composed fingerprint runs with every NIGHT19 block off (SurvivalSim.RunFingerprint), so Hashes stay NIGHT18's
        /// (FingerprintBaselineN18, the off path's baseline for good). NIGHT20's close (FingerprintBaselineN20) set it true.</summary>
        public const bool N19OnPath = true;

        /// <summary>NIGHT21: false until the close (U3-35.11) records the run with the NIGHT21 blocks as configured; until then the
        /// composed fingerprint runs with every NIGHT21 block off, so its lines stay FingerprintBaselineN20's.</summary>
        public const bool N21OnPath = true;

        /// <summary>NIGHT22: false unless the close (U3-36.11) records a run with a sim-changing NIGHT22 default on (the mutators
        /// reader, directed steps, TRICK CHAIN, OVERDRIVE); until then the composed fingerprint runs with every NIGHT22 block off,
        /// so its lines stay FingerprintBaseline's (NIGHT21's).</summary>
        public const bool N22OnPath = false;

        /// <summary>NIGHT24: false unless the freeze (U3-38.7) records a run with a sim-changing NIGHT24 default on (shotgunTune, bigPick,
        /// chargeFist); until then the composed fingerprint runs with every NIGHT24 block off, so its lines stay FingerprintBaseline's.</summary>
        public const bool N24OnPath = false;

        /// <summary>NIGHT25: false unless the freeze (U3-39.7) records a run with a sim-changing NIGHT25 default on (meleeFire, pulseCharge,
        /// shockwaveOff); until then the composed fingerprint runs with every NIGHT25 block off.</summary>
        public const bool N25OnPath = false;

        /// <summary>NIGHT26: false unless the freeze (U3-40.4) records a run with a sim-changing NIGHT26 default on (pulseLT, weaponIdentity);
        /// until then the composed fingerprint runs with every NIGHT26 block off.</summary>
        public const bool N26OnPath = false;

        /// <summary>NIGHT27: false unless the freeze records a run with a sim-changing NIGHT27 default on (pulse27, chargeShots27);
        /// until then the composed fingerprint runs with every NIGHT27 block off.</summary>
        public const bool N27OnPath = false;

        /// <summary>NIGHT29: false unless the freeze records a run with a sim-changing NIGHT29 default on; until then the composed
        /// fingerprint runs with every NIGHT29 block off.</summary>
        public const bool N29OnPath = false;

        /// <summary>NIGHT30: false unless the freeze records a run with a sim-changing NIGHT30 default on; until then the composed
        /// fingerprint runs with every NIGHT30 block off.</summary>
        public const bool N30OnPath = false;

        /// <summary>NIGHT31: false unless the freeze records a run with a sim-changing NIGHT31 default on; until then the composed
        /// fingerprint runs with every NIGHT31 block off.</summary>
        public const bool N31OnPath = false;

        /// <summary>NIGHT32: false unless the freeze records a run with a sim-changing NIGHT32 default on; until then the composed
        /// fingerprint runs with every NIGHT32 block off.</summary>
        public const bool N32OnPath = false;

        /// <summary>NIGHT33: false unless the freeze records a run with a sim-changing NIGHT33 default on; until then the composed
        /// fingerprint runs with every NIGHT33 block off.</summary>
        public const bool N33OnPath = false;

        /// <summary>NIGHT34: false unless the freeze records a run with a sim-changing NIGHT34 default on; until then the composed
        /// fingerprint runs with every NIGHT34 block off.</summary>
        public const bool N34OnPath = false;

        /// <summary>NIGHT35: false unless the freeze records a run with a sim-changing NIGHT35 default on; until then the composed
        /// fingerprint runs with every NIGHT35 block off.</summary>
        public const bool N35OnPath = false;

        /// <summary>NIGHT36: false unless the freeze records a run with a sim-changing NIGHT36 default on; until then the composed
        /// fingerprint runs with every NIGHT36 block off.</summary>
        public const bool N36OnPath = false;

        /// <summary>NIGHT37: false unless the freeze records a run with a sim-changing NIGHT37 default on; until then the composed
        /// fingerprint runs with every NIGHT37 block off.</summary>
        public const bool N37OnPath = false;

        /// <summary>NIGHT38: false unless the freeze records a run with a sim-changing NIGHT38 default on; until then the composed
        /// fingerprint runs with every NIGHT38 block off.</summary>
        public const bool N38OnPath = false;

        /// <summary>NIGHT39: false unless the freeze records a run with a sim-changing NIGHT39 default on; until then the composed
        /// fingerprint runs with every NIGHT39 block off.</summary>
        public const bool N39OnPath = false;

        /// <summary>NIGHT40: false unless the freeze records a run with a sim-changing NIGHT40 default on; until then the composed
        /// fingerprint runs with every NIGHT40 block off.</summary>
        public const bool N40OnPath = false;

        /// <summary>NIGHT41: false unless the freeze records a run with a sim-changing NIGHT41 default on; until then the composed
        /// fingerprint runs with every NIGHT41 block off.</summary>
        public const bool N41OnPath = false;

        /// <summary>NIGHT42: false unless the freeze records a run with a sim-changing NIGHT42 default on; until then the composed
        /// fingerprint runs with every NIGHT42 block off.</summary>
        public const bool N42OnPath = false;

        /// <summary>NIGHT43: false unless the freeze records a run with a sim-changing NIGHT43 default on; until then the composed
        /// fingerprint runs with every NIGHT43 block off.</summary>
        public const bool N43OnPath = false;

        // NIGHT20 U3-34.12 (2026-10-07, the lead, FingerprintBaselineN20): re-recorded once, in its own commit, at the code
        // freeze, because NIGHT20.md rule 3 asks the close to record the run with every NIGHT19 and NIGHT20 design block as
        // configured (NIGHT19's close never re-recorded, so until now the composed run set both nights' blocks aside and
        // printed NIGHT18's lines). The blocks change the run from checkpoint 1 on (the stamina / regen / director / arsenal
        // rows of both nights). SimHarness `fingerprint --print` twice in two processes and `twice` in one printed these
        // twelve; at step 7200: round 4, 36 kills, 80 shots, 4775 earned, 8 alive (the off path: round 3, 34 kills, 4200
        // earned, 2 alive). The off path keeps NIGHT18's lines (FingerprintBaselineN18): SimHarness `fingerprint --off19`
        // and the Player's second [fingerprint] line. The lines before this re-record are FingerprintBaselineN18's.
        // NIGHT17 U3-30.4 (2026-10-05, builder M): re-recorded once, in its own commit, because design.stamina (one bar, the
        // sprint, the partial boost; on by default) and the fingerprint script's dash key now held 1 s after each 420-step press
        // (SimFingerprint.Script: the web's mover never reads dash.held, so with the stamina flags off the run still prints
        // NIGHT16's baseline: step 600 42c74be7ac21a5b3, 1200 6414a1c4a91332b5, 1800 4fcd0751e397c250, 2400 37add29651dfef0e,
        // 3000 6cbfd07eb89e8565, 3600 955171e5a7dc6f79, 4200 0a262ce4f5872f9e, 4800 bb4bd264c0200b29, 5400 7a4c86c5af09e8a5,
        // 6000 b7782966502eb16e, 6600 982f23d3f19f2eac, 7200 89091d1f7b7c2e4b) change the run from checkpoint 1 on: each boost
        // runs on into a sprint (17 boosts, 16 sprints, 18 refills to full). At step 7200: round 3, 34 kills, 80 shots, 4200
        // earned, 2 alive (NIGHT16: round 4, 36 kills, 5102 earned, 14 alive). Tapes recorded before carry the old baseline id
        // and format 2 (replayed with the stamina flags off).
        // NIGHT16 U3-29.4 (2026-10-05): re-recorded once, in its own commit, because design.move's three slide rules (on by
        // default) change the fingerprint run from checkpoint 5 (step 3000) on: SimHarness (rules on) and the Player's
        // -skygrindFingerprint print these; with the move flags off the run still prints NIGHT13's baseline (the old
        // lines: steps 600-2400 as below, then 3000 251ab8c0f0fc93de, 3600 2519cbcb71ee0afe, 4200 8fd3b0859edb8945, 4800
        // bdaa42a571bd78e6, 5400 c64d7d6353de82fd, 6000 ccb6869a8959117d, 6600 0975fb42265f5c89, 7200 7b02ca5c9fa6bb0e).
        // At step 7200: round 4, 36 kills, 80 shots, 5102 earned, 14 alive (NIGHT13: 4310 earned, 0 alive). Tapes recorded
        // before carry the old baseline id (RunTape) and format 1 (replayed with the move flags off).
        // NIGHT13 (build 1, 2026-10-04 15:27, Release, Mono): two Player runs identical (Logs/player/fp-b1-1.log, fp-b1-2.log),
        // and the Editor's Play-mode run printed the same twelve; at step 7200: round 4, 36 kills, 80 shots, 4310 earned.
        // NIGHT21 U3-35.11 (2026-10-07, the lead, FingerprintBaselineN21): re-recorded once, in its own commit, at the code freeze,
        // because NIGHT21.md asks the close to record the run with every NIGHT21 design block as configured (arcline, slideBites,
        // stamp, deadline, grind, challenge on; replay off: its view is not built). The blocks change the run from checkpoint 2
        // (step 1200) on: SimHarness `fingerprint --print` in two processes printed the same twelve lines (identical md5) and `twice`
        // in one process agreed; at step 7200: round 3, 35 kills, 80 shots, 4808 earned, 1 alive (NIGHT20: round 4, 36 kills,
        // 4775 earned, 8 alive). The NIGHT20 lines are FingerprintBaselineN20 (SimHarness `fingerprint --off21`, the Player's
        // `-skygrindOff21`); NIGHT18's are FingerprintBaselineN18 (`--off19`). The lines before this re-record are FingerprintBaselineN20's.
        public static readonly string[] Hashes =
        {
            "step 600 hash 31263654d2bba9e3",
            "step 1200 hash 75cd33e37e4249a2",
            "step 1800 hash feea7f10b252552e",
            "step 2400 hash ae66f4d54643c81d",
            "step 3000 hash caa3aeabb58955d4",
            "step 3600 hash 795421973e895a5d",
            "step 4200 hash b315c316c21ac97c",
            "step 4800 hash d6fc4b03670e521f",
            "step 5400 hash 63eec3432182172c",
            "step 6000 hash d0467ed502e7c465",
            "step 6600 hash 1d20763b5c9ea6d1",
            "step 7200 hash 24fa89755eab6825",
        };

        /// <summary>GG 2026-10-26 ("re-record the smoke baselines"): true when the Player's movement is the ULTRAKILL profile (MovementTuning71 sets
        /// it at boot). The web's movement (SimHarness, the gates, the Core tests) never sets it, so they keep `Hashes`.</summary>
        public static bool Ultra71;

        // GG 2026-10-26: the composed run with design.ultra71's movement (the ULTRAKILL profile), recorded by the Player's -skygrindFingerprint.
        // NIGHT72 N5b (ground.friction and ground.accel 31.07 in design.ultra71.set, ULTRAKILL's walk lerp at the 120 Hz step): re-recorded from
        // Builds/n72-t2 (built at ba47bb280): the lead's two runs identical (Logs/player/n72-t2-fp-1.log, -2.log; only the wall-time field differs).
        // The twelve hashes were handed to Tools/night/n72-fp-update.cjs as the lead sent them (Research/2026-10-27-wave72/N/evidence/n5-t2-fp-hashes-from-the-lead.txt).
        // The n71 baseline they replace (Builds/n71-polish7, HEAD 4ed57e207's sim; Logs/player/n72-fp-ultra-1.log, -2.log) began step 600 hash 3f8358638cf2c309, ended
        // step 7200 hash 92c635b17cf30663 (round 4, 36 kills, 80 shots, 4303 earned, 0 alive). A deliberate behaviour change, recorded in its own commit.
        public static readonly string[] Ultra71Hashes =
        {
            "step 600 hash dfdd05762df49c52",
            "step 1200 hash 66593a513b073a85",
            "step 1800 hash e6f4baade691903f",
            "step 2400 hash 8385df3a777362a6",
            "step 3000 hash 56aff577da384163",
            "step 3600 hash 8c86b054fb55f4d8",
            "step 4200 hash 0fb6df20c66ae457",
            "step 4800 hash ff1f9e21b80d1090",
            "step 5400 hash 742c0a6d6bfd5db0",
            "step 6000 hash f16cbbc486f72835",
            "step 6600 hash 21f7e178e3281112",
            "step 7200 hash 3e3796b62d40accb",
        };

        /// <summary>The baseline the running game is judged by: Ultra71Hashes under the ULTRAKILL profile, else the web movement's Hashes.</summary>
        public static string[] Active => Ultra71 ? Ultra71Hashes : Hashes;

        /// <summary>The first checkpoint that differs from the baseline (-1: all equal; a length difference counts).</summary>
        public static int FirstDifference(IList<string> lines)
        {
            string[] h = Active;
            for (int i = 0; i < h.Length || i < lines.Count; i++)
                if (i >= h.Length || i >= lines.Count || h[i] != lines[i]) return i;
            return -1;
        }
    }
}
