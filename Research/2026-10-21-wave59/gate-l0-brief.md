# NIGHT59 gate L0 brief (the lead; one slim critic; taste decides nothing; 15 min, <= 6 images)

Row: none flipped by this gate. L0 is level data behind kiln57 (ON). The question: does the kb2c spawn re-cut (L0-cut4.patch, applied
in the working tree and built into Builds/n59-b1) keep the rule's lines 1-4? Verdict to `Research/2026-10-21-wave59/gate-l0.md`, first line
`L0: PASS | FAIL <reason> | UNSURE <reason>`. Read-only otherwise. All paths under C:\Users\gaige\dev\SKYGRIND-unity.

## The rule (NIGHT57.md section 0)
(1) MAIN beats the baseline beyond seed noise; (2) GUARDS within band; (3) nothing lost; (4) evidence confirmed.

## MAIN: "put at the next room" per 10-seed set on n57-kiln
- HEAD (Logs/n58/kiln-base.txt, kiln-base-B.txt; seeds 1-10 / 11-20): 14 / 15. Seeds 21-30 at HEAD: 14 (Research/2026-10-21-wave58/L.md, the "before 21-30" column).
- Tonight's tree (Logs/n59/l0-A.txt, l0-B.txt, l0-C.txt; seeds 1-10 / 11-20 / 21-30): 1 / 2 / 5. Want <= 5 per set.
- Check: re-run ONE set yourself: `dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n46flow --level n57-kiln --seeds 10 --first 1`
  (~1.5 min). The working tree already holds the cut (Design/levels/n57-kiln.json). Compare its SUMMARY with l0-A.txt.

## GUARDS
- Deaths pooled over seeds 1-30: HEAD 2 + 6 + 1 = 9; tonight 1 + 7 + 1 = 9; band 10 % -> <= 9.9. (Seed set 11-20 went 6 -> 7.)
- Stall <= 23.65 % (HEAD 18.9 / 20.5 / 18.8; tonight 20.3 / 22.9 / 22.7).
- Clear 150-240 s (tonight 176.5 / 175.8 / 173.9).
- Contract (NIGHT58 Logs/n58/ext/contract-c4-kb2c.txt, contract-c4-kb3.txt): kb2c 14/14 PASS (dwell 0.70 -> 0.90, grace 0.5); kb3 14/14 PASS (dwell 1.10 -> 1.80, grace 0.8).
- In view at the doors (Logs/feel/n59-b1/summary47.txt vs Logs/feel/n58-freeze/summary47.txt, the a157 lines): kb3c seen 3 / 6 on and off; kb3d 2 / 5 on and off; kb2c seen 5 / 5 on and off. Equal: no worse.

## Nothing lost
- verbs37 on n57-kiln with the cut in the build: Logs/feel/n59-b1/summary47.txt "verbs37: total 31/31; RED rows []" (NIGHT57 and NIGHT58: 31). The build holds the cut: its StreamingAssets copy differs from n58-freeze's (cmp) and contains the spawn [-2,0,-168] yaw 45.

## Frames (<= 6)
Logs/feel/n59-b1/a157 shots (kb2c entry: shot-N29-L57-n57-kiln-kb2c-entry.png) vs Logs/feel/n58-freeze's a157 kb2c entry. Eye line: from the new spawn, can the player see where the fight starts (the near bridge, the south-west wave)? Say what you see; not checked: a person.

## Known caveats (say whether you agree they matter)
- The spawn's facing changed (yaw 0 -> 45): a person's first view differs; not checked in a Player beyond a157.
- Deaths on seeds 11-20 rose 6 -> 7 (inside the band); the lead's rule reads the pooled number.
