# L10: press71, half built (after F4's RED; a play-build switch `-skygrindPress71`, no config row, OFF without the flag)

- **The change, plainly:** with the switch on, a KESTREL tap made during a hit freeze always fires after it, even right after a press-step shot (the charge cooling); without it, that tap is lost.
- **How (Game-side, Press71.cs + ReaderInput.SampleCombat):** fire's press is never delivered on two sim steps in a row; a press latched for the step right after a pressed step (inside a freeze that step started) waits one step (8.3 ms) and is delivered then. Every press kept, none doubled, order kept; forced inputs untouched; Core untouched (Charge35's n35-r3 rule stays).
- **Measured headless (taps71h-press71.txt, the real Press71 class):** taps inside freezes 6: fired 3 off, 6 on; delivered one step late: 3 (exactly the press-step ones).
- **Not built:** L10's other half, "a fresh press shortens the hit freeze": ImpactRules (Core) has no way to end a freeze and keep a slow-mo (Halt() ends both); hitstop is part of the web's loop (CLAUDE.md rule 4) and may cut against GG's "kills not crunchy": a sim decision first.
- **Off flag:** off unless `-skygrindPress71`; `-skygrindOff71` (or older) keeps it off. For the play build: launched with `-skygrindPress71`.
- **Reel (proof, the lead):** `bash Tools/night/n71-reels.sh <tag> tpoff,n52-foundry,taps71 tpon,n52-foundry,taps71,-skygrindPress71`: off `taps inside freezes N, fired M < N` (the press-step kind), on `M == N` (GREEN).
- **Gates:** gates.txt (golden 0 mismatches, 22 replays exact, fingerprints EQUAL x12: the gates run no reader). The tape records what the sim saw (TapeRecorder wraps the reader). EditMode: N71PressTests (5).
