#!/usr/bin/env python3
# NIGHT22 shared commit: one-time patch of the lead's files (exact-string replacements; each must match exactly once).
import io, sys

def patch(path, pairs):
    with io.open(path, 'r', encoding='utf-8', newline='') as f:
        s = f.read()
    crlf = '\r\n' in s
    for old, new in pairs:
        if crlf:
            old = old.replace('\r\n', '\n').replace('\n', '\r\n')
            new = new.replace('\r\n', '\n').replace('\n', '\r\n')
        n = s.count(old)
        if n != 1:
            print('FAIL', path, n, repr(old[:70]))
            sys.exit(1)
        s = s.replace(old, new)
    with io.open(path, 'w', encoding='utf-8', newline='') as f:
        f.write(s)
    print('ok', path)

G = 'Assets/Scripts/Game/'
patch(G + 'SurvivalSim.cs', [(
 "            N21Init(); // NIGHT21: the six NIGHT21 design blocks, read once (SurvivalSim.N21.cs)\n",
 "            N21Init(); // NIGHT21: the six NIGHT21 design blocks, read once (SurvivalSim.N21.cs)\n"
 "            N22Init(); // NIGHT22: the six NIGHT22 design blocks, read once (SurvivalSim.N22.cs)\n")])

patch(G + 'SurvivalSim.Run.cs', [
 ("            N21StartRun(); // NIGHT21: the run's NIGHT21 blocks (a tape's, or off with --off21 / --off20 / --off19), the same way (SurvivalSim.N21.cs)\n",
  "            N21StartRun(); // NIGHT21: the run's NIGHT21 blocks (a tape's, or off with --off21 / --off20 / --off19), the same way (SurvivalSim.N21.cs)\n"
  "            N22StartRun(); // NIGHT22: the run's NIGHT22 blocks (a tape's, or off with --off22 and older), the same way (SurvivalSim.N22.cs)\n"),
 ("            N21ReplayStep(step);\n",
  "            N21ReplayStep(step);\n"
  "            N22DirectedStep(step); // NIGHT22: the lead's hooks for the NIGHT22 rows (SurvivalSim.N22.cs; nothing without a body)\n"
  "            N22TrickStep(step);\n"
  "            N22DuskStep(step);\n"
  "            N22HapticsStep(step);\n"),
 ("            if (!FingerprintBaseline.N21OnPath) n21Override = N21Blocks.Off;\n            try\n",
  "            if (!FingerprintBaseline.N21OnPath) n21Override = N21Blocks.Off;\n"
  "            N22Blocks? n22Saved = n22Override; // NIGHT22: and every NIGHT22 block off until the close (FingerprintBaseline.N22OnPath)\n"
  "            if (!FingerprintBaseline.N22OnPath) n22Override = N22Blocks.Off;\n            try\n"),
 ("                n21Override = n21Saved;\n            }\n            return lines;",
  "                n21Override = n21Saved;\n                n22Override = n22Saved;\n            }\n            return lines;"),
])

patch(G + 'FingerprintBaseline.cs', [(
 "        public const bool N21OnPath = true;\n",
 "        public const bool N21OnPath = true;\n\n"
 "        /// <summary>NIGHT22: false unless the close (U3-36.11) records a run with a sim-changing NIGHT22 default on (the mutators\n"
 "        /// reader, directed steps, TRICK CHAIN, OVERDRIVE); until then the composed fingerprint runs with every NIGHT22 block off,\n"
 "        /// so its lines stay FingerprintBaseline's (NIGHT21's).</summary>\n"
 "        public const bool N22OnPath = false;\n")])

patch(G + 'Tape/RunTape.cs', [
 ("        public const int FormatVersion = 6;", "        public const int FormatVersion = 7;"),
 ("NIGHT21: 6 adds the NIGHT21 blocks (formats 1-5: off)", "NIGHT21: 6 adds the NIGHT21 blocks (formats 1-5: off); NIGHT22: 7 adds the NIGHT22 blocks (formats 1-6: off; the run mode is already in every format)"),
 ("        public int n21;\n", "        public int n21;\n        /// <summary>NIGHT22: the NIGHT22 design blocks the run read (N22Blocks.Bits; formats 1-6: 0, every block off, NIGHT21's game).</summary>\n        public int n22;\n"),
 ("            w.Byte((byte)n21); // format 6\n", "            w.Byte((byte)n21); // format 6\n            w.Byte((byte)n22); // format 7\n"),
 ("            t.n21 = version >= 6 ? r.Byte() : 0; // formats 1-5: recorded on NIGHT20's game (or before), so every NIGHT21 block OFF (explicitly)\n",
  "            t.n21 = version >= 6 ? r.Byte() : 0; // formats 1-5: recorded on NIGHT20's game (or before), so every NIGHT21 block OFF (explicitly)\n"
  "            t.n22 = version >= 7 ? r.Byte() : 0; // formats 1-6: recorded on NIGHT21's game (or before), so every NIGHT22 block OFF (explicitly)\n"),
])

patch(G + 'Tape/TapeReplay.cs', [(
 "            sim.n21Override = N21Blocks.FromBits(tape.n21); // NIGHT21: the NIGHT21 blocks the run read (formats 1-5: all off, NIGHT20's game)\n",
 "            sim.n21Override = N21Blocks.FromBits(tape.n21); // NIGHT21: the NIGHT21 blocks the run read (formats 1-5: all off, NIGHT20's game)\n"
 "            sim.n22Override = N22Blocks.FromBits(tape.n22); // NIGHT22: the NIGHT22 blocks the run read (formats 1-6: all off, NIGHT21's game)\n")])

patch(G + 'Tape/TapeScript.cs', [
 ("            N21Blocks? n21Saved = sim.n21Override; // NIGHT21: and every NIGHT21 block off until the close (N21OnPath)\n",
  "            N21Blocks? n21Saved = sim.n21Override; // NIGHT21: and every NIGHT21 block off until the close (N21OnPath)\n"
  "            N22Blocks? n22Saved = sim.n22Override; // NIGHT22: and every NIGHT22 block off until the close (N22OnPath)\n"),
 ("            if (!FingerprintBaseline.N21OnPath) sim.n21Override = N21Blocks.Off;\n",
  "            if (!FingerprintBaseline.N21OnPath) sim.n21Override = N21Blocks.Off;\n"
  "            if (!FingerprintBaseline.N22OnPath) sim.n22Override = N22Blocks.Off;\n"),
 ("                sim.n21Override = n21Saved;\n", "                sim.n21Override = n21Saved;\n                sim.n22Override = n22Saved;\n"),
])

patch(G + 'PlayerChecks.cs', [
 ("            if (Array.IndexOf(args, \"-skygrindOff19\") >= 0) N19Blocks.ForceOff = N20Blocks.ForceOff = N21Blocks.ForceOff = true;",
  "            if (Array.IndexOf(args, \"-skygrindOff19\") >= 0) N19Blocks.ForceOff = N20Blocks.ForceOff = N21Blocks.ForceOff = N22Blocks.ForceOff = true;"),
 ("            if (Array.IndexOf(args, \"-skygrindOff20\") >= 0) N20Blocks.ForceOff = N21Blocks.ForceOff = true;",
  "            if (Array.IndexOf(args, \"-skygrindOff20\") >= 0) N20Blocks.ForceOff = N21Blocks.ForceOff = N22Blocks.ForceOff = true;"),
 ("            if (Array.IndexOf(args, \"-skygrindOff21\") >= 0) N21Blocks.ForceOff = true;",
  "            if (Array.IndexOf(args, \"-skygrindOff21\") >= 0) N21Blocks.ForceOff = N22Blocks.ForceOff = true;"),
])
# the -skygrindOff21 line has a trailing comment; add the 22 switch on its own line after it
with io.open(G + 'PlayerChecks.cs', 'r', encoding='utf-8', newline='') as f:
    s = f.read()
marker = "// NIGHT21: every NIGHT21 design block off for every run (NIGHT20's game, the A side)"
i = s.index(marker)
j = s.index('\n', i)
nl = '\r\n' if s[j-1] == '\r' else '\n'
ins = nl + '            if (Array.IndexOf(args, "-skygrindOff22") >= 0) N22Blocks.ForceOff = true; // NIGHT22: every NIGHT22 design block off for every run (NIGHT21\'s game, the A side)'
s = s[:j - (1 if nl == '\r\n' else 0)] + ins + s[j - (1 if nl == '\r\n' else 0):]
with io.open(G + 'PlayerChecks.cs', 'w', encoding='utf-8', newline='') as f:
    f.write(s)
print('ok PlayerChecks off22 line')

patch(G + 'FeelReel.cs', [
 ("                list.AddRange(PhaseE); // NIGHT21 (FeelReel.N21.cs)\n",
  "                list.AddRange(PhaseE); // NIGHT21 (FeelReel.N21.cs)\n                list.AddRange(PhaseF); // NIGHT22 (FeelReel.N22.cs)\n"),
 ("                        res[0] = \"SKIPPED: unknown scenario\";\n",
  "                        if (IsPhaseF(s))\n                        {\n                            yield return N22Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT22 (FeelReel.N22.cs)\n                            yield break;\n                        }\n                        res[0] = \"SKIPPED: unknown scenario\";\n"),
])

patch('Tools/SimHarness/Program.cs', [
 ("        N21Blocks.ForceOff = off21;\n",
  "        N21Blocks.ForceOff = off21;\n"
  "        // NIGHT22: --off22 runs every command with every NIGHT22 design block off (NIGHT21's game; tonight's A side);\n"
  "        // --off21, --off20 and --off19 turn off the older nights' blocks as well\n"
  "        bool off22 = off21 || Array.IndexOf(args, \"--off22\") >= 0;\n"
  "        N22Blocks.ForceOff = off22;\n"),
 ("                if (off21) sim.n21Override = N21Blocks.Off; // NIGHT21: the same\n",
  "                if (off21) sim.n21Override = N21Blocks.Off; // NIGHT21: the same\n"
  "                if (off22) sim.n22Override = N22Blocks.Off; // NIGHT22: the same\n"),
 ("        N21(cmd, args, ref code);", "        N22(cmd, args, ref code); // NIGHT22: n22shared (the lead) and the rows' commands (Harness.N22.cs)\n        N21(cmd, args, ref code);"),
])
print('done')
