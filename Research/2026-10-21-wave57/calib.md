# NIGHT57 P0-calib: the GG stand-in judge scored on GG's real verdicts (2026-10-21 09:5x; NIGHT57.md section 5 P0; DECIDE57 D-5, D-8)

**Result: FAIL.** The tonight's gate critic judges evidence and eye-only lines only, never "would GG like it"; the blind-preference
MAINs (kill55, colour56) use their measured fallbacks (NIGHT57.md section 0 rule 5).

## Method
`Tools/night/n57-calib.py` copied renamed frames into three folders OUTSIDE the repo (the session scratchpad, `calib57/a1`, `a2`, `b`),
each with only a rubric; `claude -p "<rubric>" --allowedTools Read` ran in each folder (no CLAUDE.md, no project memory, no repo access;
the parent folders' CLAUDE.md files checked: none exist). The key is `Logs/n57/calib-key.json`; the answers are `Logs/n57/calib-a1.txt`,
`calib-a2.txt`, `calib-b.txt`.

## Part A: which build is the better game (GG: n35 "the gameplay just feels awful", n37 "a little bit better")
| order | SET1 | SET2 | the judge's pick | GG's direction |
|---|---|---|---|---|
| a1 | n35 | n37 | SET1 = n35 | n37: WRONG |
| a2 | n37 | n35 | SET2 = n35 | n37: WRONG |
The judge picked n35 in both orders (consistent, not position bias). Its reasons: n35's frames "show an actual arena ... lava pits and
cyan-edged walls ... orange capsule enemies"; n37's are "mostly long, empty straight corridors ... tutorial text boxes cover the middle".

## Part B: name the complaint before seeing GG's words (match words fixed in the key before the run)
| set | GG said | the judge said | match words hit |
|---|---|---|---|
| P = n41-r7 level 1 | the wall ride section "awful and relaly difficult" | "narrow, empty pink hallways with nowhere to build speed or wall-ride" | "wall-ride": yes (but the substance differs: GG found the ride hard, the judge found none) |
| Q = n42-r4 UI | "too generic and cheap, almost like basic placeholder or default AI-generated UI" | "The Pulse Gauntlet is a huge flat brown brick ... looks like placeholder art" | "placeholder": yes (but about the gauntlet model, not the HUD) |
By the letter 2 of 2; on substance 0 of 2 exact.

## What it means (the limits, D-8)
- The frame sets were unequal: n37-r1's level36 frames are its teaching corridors with captions; n35-r2's are combat rooms. The judge
  judged what the stills show, and GG judged by playing. So part A shows that a stills judge rewards arena-looking frames and punishes
  tutorial corridors, whatever the build's feel: exactly why it cannot stand in for GG on taste.
- Consequence tonight: the batched critic checks evidence (right build, row on, frames match the numbers), guards and eye-only lines
  (overlap, tint, a tell readable in one frame). Taste lines are labelled "critic's opinion, uncalibrated" and never decide a row.
- Next time: calibrate on PLAYED evidence (reels of the same room on both builds) and on more than one GG verdict pair.
