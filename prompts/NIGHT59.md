# NIGHT59.md: SHIP WHAT IS READY. Level 3's exits fix, level 4's GAP NEST, yellow56's halo, kill55's marks (lead + three builders + rolling one-row critics; at least 2 h, hard cap 2 h 15 min)

GG, 2026-10-21 ~20:35, verbatim (this outranks everything below it):
> follow best methods to take learning and build next 2 hour prompt and spend 15 mins building prompt

**GG will not answer during the run.** Decide every call in `DECIDE59.md` (untracked; the forge wrote D-1..D-3) with its reason and the
alternative rejected. Base: `port/stage3` HEAD, which must hold tag `u3-n58` and commit 04d8f2f3 (`git log --oneline -1 04d8f2f3`), and
`git apply --check Research/2026-10-21-wave58/L0-cut4.patch` must print nothing. If any fails, log it as D-5 and fix that first (L-107: a
close can claim what the repo does not have). t0 = the first `date`. Print `date` and the elapsed time at the start of every turn.
Rules: CLAUDE.md. The shipping rule: NIGHT57.md section 0, unchanged (read it by its absolute path
`C:\Users\gaige\dev\SKYGRIND-unity\NIGHT57.md`). Builder brief: Research/2026-10-21-wave58/builder-brief.md with the n59 names (copy it to
Research/2026-10-21-wave59/builder-brief.md at A0).

## 0. Why this slate (NIGHT58-LOG.md close table + its EXT section; THREADS T-135, T-139; DECIDE58 D-13..D-17)
- NIGHT58 shipped **nothing**: 8 entries, 3 builders, one batched critic that died twice, so no row had a verdict at the freeze. Its EXT
  hour then showed the way: a slim one-row critic took 17 min (batched: 25-47 min) and the bot numbers for a level cut take minutes.
- Three entries are one step from shipping, each with a measured before / after already in the repo:
  - **L0 level 3's exits (cut 4, `L0-cut4.patch`):** put at the next room 14 / 15 / 14 -> 1 / 2 / 5, deaths pooled 9 -> 9, stall in band,
    kb2c and kb3 contracts 14/14, room table PASS. Missing: verbs37 + a157 on n57-kiln in a Player, and a critic.
  - **L1 level 4's THE GAP NEST** (built in `gen-n58-span.cjs` slotP2, out of NEW): walks 10/10, deaths 0 -> 0, stall 14.5 -> 13.4 %.
    Missing: n58-l-room / identity / contract (the tools do not exist yet: n57's hard-code 'kiln'), the cause of put-at 0 -> 3, a Player.
    The forge's spike: its 61 falls are the span's normal recovery (n52-span already logs 107 falls per 10 seeds: p3 55, p5 42).
  - **V2 yellow56:** MAIN met (12 / 12 -> 0 / 12 frames over 0.5 %) but the critic FAILED a guard: the foundry a5 parry halo keeps 20 % of
    its yellow because its V (0.84) sits on `dimYellow58.vFull` (0.84, "at or under which the cap is whole"). Value cannot split it (the
    plate 0.82 and the lava edge 0.81 must stay capped); saturation can: the halo S 0.99, the lava edge 0.64, the plate 0.56 (gate-x.md).
- **G1 kill55** is the one deeper fix: marks at the flash end 0.69 % (want >= 5 %); the probe says drawn but lifted by fog / grade (G.md).
**Measured timings (NIGHT58 + EXT):** gen + levelgen + 30 bot seeds in parallel ~4 min a cut; a room contract ~2.5 min; a slim critic
17 min; builder L's new room 29 min; a Player build 1-3 min; verbs37 x6 7-8 min; the smoke 4 min; reel frames ~10 GB a set (prune).

## 1. Scope
- **In:** the four entries above. **Out:** enemy looks / new kinds (GG's), audio, new verbs, new modes, rim56 and press57 (their next step
  is a cause hunt, not a ship: seeded leftovers), level 5+. Game M's suite only after the tag.

## 2. Clock (t0 = your first `date`; close table no earlier than 1:55; hard cap 2:15)
| when | who | what |
|---|---|---|
| 0:00-0:10 | lead | A0 (the base checks above; `bash Tools/unity-loop.sh status`; `df`: >= 15 GB or prune with n58-prune.sh); copy the brief to Research/2026-10-21-wave59/ and **commit it with void-rooms.md before the first spawn** (worktrees branch from HEAD); `Logs/n59/slate.json` (MAIN / GUARDS written BEFORE any number); spawn L, V, G one at a time |
| 0:10-0:35 | lead | **L0 first:** `git apply` the patch, the run order in gen-n57-kiln.cjs's header, n46flow `--level n57-kiln` seeds 1 / 11 / 21 in parallel (~2 min; compare with L0-ext.md's cut 4 column), `bash Tools/night/n58-gates.sh` (no new row), build `Builds/n59-b1`, reels `bash Tools/night/n56-reels.sh n59-b1 l0on,n57-kiln,verbs37 l0on,n57-kiln,a157`, the off side `bash Tools/night/n56-reels.sh n58-freeze l0off,n57-kiln,a157` (same code, HEAD's level data); prune; **critic C1 (L0 only)** at ~0:35 |
| 0:10-0:50 | L, V, G | their entries, one slice at a time, commit every slice; report Player-ready slices by 0:50 (builders stop at 1:05) |
| ~0:55 | lead | pick what is ready, gates, build `Builds/n59-b2` (rows on in its copy: n46-build-rows.cjs); reels ENTRY BY ENTRY (on / off by one-row switches, n56-reels.sh), and each entry's **slim critic starts the moment its own reels land** (at most two alive at once); a late slice gets b3 at ~1:10 |
| 0:55-1:40 | lead | while critics read: commit L0 into the level data if C1 passed (else `git checkout --` the generated files, keep the patch); write `Tools/night/n59-freeze-rows.cjs` (a copy of n58's with "NIGHT59"); the Core suite only if Core changed |
| 1:40-1:55 | lead | rows ON by verdict (n59-freeze-rows.cjs), gates, build `n59-freeze` from HEAD at 1:40 exactly, verbs37 x5 (vk is n57-kiln) + n58-span, the smoke (`bash Tools/night/n56-smoke-steal.sh n59-freeze`, no agent running) |
| 1:55-2:15 | lead | commit, tag `u3-n59`, the close (section 6). A row with no verdict by 1:40 stays OFF |

## 3. The slate (MAIN beats the baseline beyond noise, GUARDS within band, nothing lost)

### L0 level 3's exits (the lead; data behind kiln57, already ON)
- The fix: `git apply Research/2026-10-21-wave58/L0-cut4.patch`, then gen-n57-kiln.cjs, levelgen `--walk`, `--finish`, then the three
  n46flow sets on tonight's tree (the rule's numbers are measured on the tree that ships; L0-ext.md's cut 4 column is the expectation).
- MAIN = put at the next room 14 / 15 / 14 -> <= 5 per set (measured 1 / 2 / 5). GUARDS = deaths pooled 1-30 <= 9 x 1.10 (measured 9);
  stall <= 23.65 %; clear 150-240 s; kb2c / kb3 contract 14/14; **verbs37 on n57-kiln 31 / 31** (n58-freeze's vk: 31 / 31); a157's in view
  at the door kb3c >= 3 / 6, kb3d >= 2 / 5 (n57-freeze's numbers, no worse).
- The critic's frames: kb2c's spawn view on b1 (a157's kb2c entry shot) vs n58-freeze's; the far bridge arch (kb2c) and the gallery arch
  (kb3) from the floor. Eye line: does the player at the new spawn (facing the near bridge, south-west) see where the fight is?

### L1 level 4's THE GAP NEST (builder L; span58 OFF until its verdict)
- Write `Tools/night/n58-l-room.py` and `Tools/night/n58-l-contract.sh` from the n57 ones with the level as an argument (run each on
  n57-kiln first and print that its table equals the n57 tool's: the planted case). Run them on n58-span built with `--slots=p2`.
- The put-at 0 -> 3: print where (a one-line print at the counter in Harness.N46.Flow.cs in the worktree only, NOT committed, as NIGHT58's
  L did) before cutting anything; up to 3 cuts, each with its numbers.
- MAIN = span's combat rooms 3 -> 4 with p2 passing n58-l-room / identity / the contract 14/14 / walk 10/10 (then p2 goes into NEW).
  GUARDS (written now, from the forge's spike): span stall <= 14.5 % + band; deaths in band (0 on seeds 1-10 and 11-20); put at the next
  room <= 1 per set; **p2 falls per 10 seeds within the level's void rooms' range (p3 52-55, p5 42-50)**; verbs37 on n58-span 31 / 31.
- The lead: b2 with span58 ON in its copy, a158 (FeelReel.N58.Level.cs: L writes it from FeelReel.N57.Level.cs; p2's entry and mid
  shots) + verbs37 on n58-span. Read `Research/2026-10-21-wave59/void-rooms.md` (the forge's research) for catch ledges / rails.

### V2 yellow56's halo (builder V)
- Cause is known: the halo's V 0.84 sits on `dimYellow58.vFull` 0.84 (whole cap at or under it), so raising vFull dims it MORE. Split by
  saturation: a new Unity-side `sKeep` bound in the `dimYellow58` block (with its `_why`, CLAUDE.md rule 5; ~0.9: yellow at or above it is
  untouched; the halo S 0.99, the lava edge 0.64, the plate 0.56), set with n58-v-rows.cjs, read by the cap shader. FIRST a planted case:
  a pixel at H 48, S 0.99, V 0.84 stays yellow, one at S 0.64, V 0.81 is capped (RED on the current shader, GREEN after).
- The halo meter: commit `Tools/night/n59-v-halo.py` (with a planted case): connected yellow pixels in the box x 560-650, y 540-700 of
  foundry a5-entry, the main frame against its `-nocap` twin at the same instant (gate-x.md's method; its halo3.py is not in the repo).
- MAIN = frames over 0.5 % non-parry yellow 12 / 12 (off) -> <= 1 / 12 (on), foundry a5 + spire v5 (a358). GUARDS = the halo keeps
  >= 80 % of its yellow pixels in the ON run (n59-v-halo.py; NIGHT58: 20 %, the OFF control 91 %); the spire v5-mid orbs within 10 %;
  verbs37 x5 equal.

### G1 kill55's marks (builder G; stop at 1:05 whatever the state)
- The probe (G.md): the mark is drawn but fog / the grade lifts it. Draw the marks after the fog and the grade (or exempt their material),
  behind kill55. MAIN (NIGHT57.md section 0 line 5; G.md) = marks at the flash end >= 5 % (0.41 % off -> 0.69 % on at NIGHT58, a258 on
  Builds/n58-freezek) AND the chain-hold ladder ok on a355 (reels `k1on,n52-foundry,a355` and `k1off,n52-foundry,a355,-skygrindNoKill55`
  in the same batch as a258). GUARDS = no whole-
  frame change outside the marks (n56-colour.py cover lines), C-6 flash budget <= 3 a second, verbs37 equal.

### Seeded leftovers (after every entry above had its first attempt)
1. rim56: find the trims' draw path (V.md: the accent dim never reached the frame). 2. A second span room (the card's next slot).
3. The exit lamp's meter (Exit58On, V3). 4. After the tag: Game M per class.

## 4. The gate: rolling one-row critics (the forge experiment)
A critic reads ONE entry from a slim brief (model: Research/2026-10-21-wave58/gate-x-brief.md: the rule, MAIN / GUARDS with the files,
re-run one meter, <= 6 images, 15 min, first line `<id>: PASS | FAIL <reason> | UNSURE <reason>`). Start it the moment that entry's
Player evidence is in; at most two critics alive at once. A critic that dies: resume once by SendMessage, then one fresh critic with the
same brief; no verdict by 1:40 = OFF. Taste decides nothing (L-105). The forge's own red-team review took 14.5 min on a 6-min budget:
give each critic 15 min and expect 17.

## 5. If something goes wrong
- The Editor does not answer: never kill it; batch-mode builds from SKYGRIND-unity-batch; say so. A Player: close it by its PID.
- The API drops (NIGHT58 lost 30 min): everything is committed per slice; at the resume after 1:45, go straight to the close.
- Disk: prune after every reel batch; a reel set with no verdict line is a refusal, not a pass (L-104).

## 6. The close (from 1:55, the close table no earlier than 1:55; hard cap 2:15)
NIGHT59-LOG.md close table (each entry ON / OFF / SHORT with MAIN and GUARDS before / after and the evidence path), NIGHT59-REPORT.md
for GG (plain words: what changed in level 3 and level 4, what is ON, a three-line PLAY note per ON entry), HANDOFF.md, DECIDE59.md,
wave-memory harvest 59 (ledger, THREADS <= 60 lines, CORE), the forge experiment scored. Explicit paths; tag `u3-n59`; nothing pushed.
Last step: the seed for the next prompt (the close table's SHORT rows with their next action) in HANDOFF's Next steps.

## 7. The forge experiment
Rolling one-row critics (section 4) instead of one batched critic per build. Hypothesis: more rows get a verdict before the freeze.
Measure: rows with a verdict by 1:40 / rows whose numbers passed (NIGHT58: 0 / 1), and rows ON.

## 8. Builder spawn template
```
You are builder <L|V|G> on NIGHT59 of SKYGRIND (Unity 6000.3.7f1, URP; a pad-first movement shooter at dusk), C:\Users\gaige\dev\SKYGRIND-unity.
t0 <time>; you stop at 1:05 after t0 (G: whatever the state). Objective: <one sentence>. Read first: CLAUDE.md,
C:\Users\gaige\dev\SKYGRIND-unity\NIGHT59.md (untracked) sections 0, 3, 4, NIGHT57.md section 0 (the shipping rule),
Research/2026-10-21-wave59/builder-brief.md, and your NIGHT58 notes (Research/2026-10-21-wave58/<L|V|G>.md).
Your entry: <id with MAIN / GUARDS>. Your files: <paths>. You work in your own git worktree; the lead integrates by cherry-pick.
You may not: run the Unity Editor, a build, a Player, a reel or run_tests, push, merge, or commit outside your branch.
After EVERY commit: `dotnet run --project Tools/SimHarness -c Release -- fingerprint` (and --off58) prints EQUAL; after any
Assets/Scripts/Core commit: CoreHarness "0 failing, 0 mismatches". Commit every slice (explicit paths + .meta; trailer
Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>). Report the moment a slice is Player-ready, and at the end: every sha, MAIN /
GUARDS before / after, the reel specs and the frames the critic must see. Last line: N59-<letter>: built <ids> · commits <shas>.
```

## 9. Hard rules (CLAUDE.md restated)
No push / merge / master; explicit paths; never stage GG's four files or the TMP font assets; restore the TMP fallback asset before
commits. Only the lead drives the Editor, builds, Players, reels and git on port/stage3. Movement exact (golden 0 mismatches, fingerprints
EQUAL with the rows off). Numbers from config; Mulberry32 only. Docs with the Write / Edit tools only (NIGHT58's close broke this once).

## Kickoff (paste into a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`)
```
Read CLAUDE.md, HANDOFF.md, NIGHT59.md, DECIDE59.md and NIGHT57.md section 0, and run NIGHT59 exactly as written as the lead. At least
2 hours, hard cap 2 h 15 min from your first `date`. I will not answer: decide every call in DECIDE59.md and keep going. Start with `date`.
```

## /goal (paste after the kickoff)
```
/goal Met when this conversation shows NIGHT59-LOG.md's close table printed in full (L0, L1, V2, G1 each ON, OFF or SHORT with MAIN and GUARD numbers before / after or "n/a" and the evidence path) and `git tag -l u3-n59` printing exactly u3-n59 and a printed `date` at least 1 h 55 min after the first `date`; or when a printed `date` shows 2 h 15 min since the first `date` (start the close by 1:55 whatever is unfinished). Print `date` and elapsed time at the start of every turn. Run NIGHT59 nonstop as the lead; never ask me; decide every call in DECIDE59.md. Order: A0 base checks, spawn builders L (level 4 checks), V (yellow56 halo), G (kill55 marks); L0 first by the lead (apply L0-cut4.patch, build n59-b1, verbs37 + a157 on n57-kiln, a one-row critic); b2 at ~0:55 with rows on in its copy; reels entry by entry, each entry's slim critic starting when its reels land, at most two at once; no verdict by 1:40 = OFF; prune reel frames after every batch. Ship rows by NIGHT57.md section 0's rule. Commit every slice with explicit paths. Close with the close table, NIGHT59-REPORT.md, HANDOFF, DECIDE59, wave-memory harvest, and tag u3-n59.
```
