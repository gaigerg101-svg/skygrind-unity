# NIGHT57 gate r3b brief (the lead; level 3 only; build Builds/n57-freeze from port/stage3 5e3d69c2, kiln57 ON in its copy (and wick55,
# grade55 pending); template: Research/2026-10-21-wave57/critic-gate.md: read it first. Your taste decides nothing.)

All paths under C:\Users\gaige\dev\SKYGRIND-unity. Write Research/2026-10-21-wave57/gate-r3b.md.

## E1 L3-rooms / kiln57 (a level row: level 3 as n57-kiln; GG: rebuild level 3 from scratch, "keep the worm AI the same")
- MAIN: >= 5 combat rooms of which >= 3 are NEW and pass their room checks; the bot's clear 150-240 s. GUARDS: stall, deaths vs n52-kiln.
- Bot logs: Logs/n57/L/flow-n57-kiln-r3d.txt (seeds 1-10) and flow-n57-kiln-r3d-B.txt (seeds 11-20), the final level (5 new rooms);
  n52-kiln: Logs/n57/flow-base.txt's n52-kiln lines (seeds 1-10) and Logs/n57/L/ (a seeds 11-20 n52 file, if present; else say so).
- The level file: Design/levels/n57-kiln.json is the generated level; `git log --oneline -12 -- Design/levels/n57/` names the commits.
- Nothing lost, in the Player (the freeze build), Logs/feel/n57-freeze/summary47.txt:
  - `== vk57` (verbs37 on n57-kiln) vs `== vk` in Logs/feel/n57-r1/summary47.txt (n52-kiln, kiln57 off, r1): compare the row list.
  - `== vk52` (verbs37 on n52-kiln with -skygrindOff57): must equal r1's n52-kiln result (31/31).
  - The staging change behind it (D-16 in C:\Users\gaige\dev\SKYGRIND-unity\DECIDE57.md; the diff: `git show 0c4e2286 -- Assets/Scripts/Game/FeelReel.N35.Verbs.cs`):
    judge whether it is a fair fix (verbs staged where a player already holds the level's gun) or a change that hides a lost verb.
  - r3's earlier RED run (`== vk57` in Logs/feel/n57-r3/summary47.txt, before the fix) for comparison.
- The worm's lead-in beats on n57-kiln: the `a157` block in the freeze summary (it prints the worm's beat counters) vs n52-kiln.
- Frames (<= 10): Logs/feel/n57-freeze/l2on/a452/ (find shot-*-n57-kiln-<room>-entry / mid / high for kb2, kb2c, kb3, kb3c, kb3d).
  Eye lines: each piece reads as having a job (GG's dislike: "random lava pits random pillars that serve no purpose"; check kb3's four
  presses and kb2's stack); the first wave visible at the entry; red-orange only on enemies / danger; no whole-frame tint.
