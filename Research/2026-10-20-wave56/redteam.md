# NIGHT56 red team (2026-10-20, a fresh reviewer, read-only, ~8 min)

Read: NIGHT56.md, Research/2026-10-20-wave56/*.md, HANDOFF.md (NIGHT55 block), NIGHT55-LOG.md (close table + timeline), CLAUDE.md,
docs/PRINCIPLES.md, docs/memory/OWNER.md + CORE.md. Checked with Glob / Grep / Read: Harness.N46.Flow.cs, Harness.N47.Human.cs,
n53-identity.py, Design/levels/n52/, N41Blocks.cs + unity-config.json (tokens41), FeelReel.N52.Rooms.cs, FeelProbe.cs, StyleSim.cs,
Freshness19.cs, PortExport/config.json (rank names), Tools/night/*, the NIGHT55 branch commits (all 13 exist), tag u3-n55 = HEAD c1a1a273,
C: 32 GB free. Ranked by the run time each would waste.

## 1. The usage-limit fallback cannot happen as written (stop; NIGHT56.md 186-188, header line 1, 148)
Section 11 says "the lead continues alone" at a 429. The limit is the account's: NIGHT55's own log says it "stopped every agent at once"
at 19:33, the lead included, and resumed at 22:01, past the cap (HANDOFF line 7, NIGHT55-LOG line 6). Tonight's agent count is close to
NIGHT55's 11: 3 builders + 4 batched critics + 7 L3 persona reviews + 1 L4 review + blind-pair judges. "At least 7 h" and "close table no
earlier than 6:50" then become unsatisfiable inside the 7:30 cap, and a /goal built on them can never be met.
**Fix:** a checkpoint freeze: after the r2 gate, tag `u3-n56-r2` as a fallback play build. Fold the persona reviews into the batched
critic (one agent per build, not one per room). Add one stop rule: "if the limit hits, the close runs at the resume; the 6:50 floor and
'at least 7 h' are waived; the cap still holds for new building."

## 2. L cannot hold L1-L5 in 5 h 10 min (sizing; NIGHT56.md 85-101)
- L1 gives 30 min to a walk graph with ride / pad / drop edges, line of sight, R5, and R8 job tags for "every solid". The five n52
  plans hold 363 boxes + ramps (counted), so the tagging alone is hours of hand data.
- Builders never run Players. L's frame conditions can only be gated at r2 (3:15) and r3 (5:00), because r1 is P0's build. With the
  two-attempts rule, L2 gets one retry and L3 / L4 / L5 get one shot each.
- L3 also needs 7 persona reviews, and L4 has 90 min.

**Fix:**
- L1 = R2 + R5 + in-view only. R8 tags only on the rooms L touches.
- Make L4 conditional on L2 PASS at r2.
- Move L5 (doorDark) to the seeded leftovers, since R's slice 1 arrives in P0 anyway.

## 3. P0 cannot fit in 70 min, and neither can A0 in 20 (sizing; NIGHT56.md 56-57, 75-81)
- NIGHT55 A0 to the last spawn took 45 min (17:05-17:50), and its r1 pick alone took 18 min (18:55-19:13).
- P0 has more:
  - 13 commits across 4 branches, including C's pair (5fe9b6cc + 283f3c41) and K's 12-file UI commit
  - gates
  - a build
  - about 14 reels: a155 x3 sides, a255, a355, a455, verbs37 x5, the steal smoke (about 4 min), plus the off sides
  - a critic

  A realistic estimate is 2-2.5 h, and it pushes r2.

**Fix:** A0 0:00-0:40, P0 0:40-2:30. Move P0's verbs37 x5 to r2 (they run there anyway), and set r2 at ~3:45 and r3 at ~5:15.

## 4. `n46flow --level all` measures the wrong levels (cannot be done as written; NIGHT56.md 91, 115)
`--level all` expands to `F1Levels = { "n45-foundry", "n45-iceline", "n45-kiln", "n40-span", "n41-spire" }` (Harness.N46.Flow.cs:62,
98, 107). The shipped levels are `n52-*` (a452 reels boot n52-foundry). There is also no NIGHT55 flow baseline: the newest table is
NIGHT52's Logs/n52/flow-freeze. So L2's stall / clear-time condition and G3's "deaths within +-20 % of NIGHT55's" would compare old
levels and could pass vacuously.
**Fix:** `n46flow --level n52-foundry,n52-iceline,n52-kiln,n52-span,n52-spire --seeds 10 --out Logs/n56/flow-base.txt`, run at u3-n55
during A0. Measured wall time is 70-153 s per level, so about 9 min per pass. Run it never beside a Player reel (L-005).

## 5. L2's Player condition has no meter, and the image cap breaks (hidden cost; NIGHT56.md 90, 143-145)
- a452 prints only frame paths and a done line, and it runs "god mode, nothing pressed" (FeelReel.N52.Rooms.cs:6-14). Nothing counts
  "enemies or tells visible" or "within 15 m". Without code, the critic counts 18 entry frames by eye.
- Image counts per critic:
  - P0 critic: 7 rows x <= 4 = 28 images
  - r2 / r3: L2 18 + V1 blind pairs (5 levels x both orders = 20) + L4 strip 4 + G2 pair 4 + V2 / V3, which is over the 40 cap

  NIGHT55's critics died at 26-36 images.

**Fix:**
- In the shared commit, add to a156 a per-entry-frame line: enemies in frustum, nearest / 2nd nearest distance, tells up. Use
  FeelProbe's enemyRect (FeelProbe.cs:116), so it is numbers, not images.
- Show V1's blind pairs for 2 levels per build.
- Split any critic at 40 images.

## 6. V's meters need a role mask that does not exist (hidden cost; NIGHT56.md 124-130)
- V1 needs "world pixels", V2 "the world's p90", and V3 "outside parry tells". colors.md:40 says "I did not check whether the project
  has a role-ID pass". There is none: the only per-object data is FeelProbe's enemy rects.
- A role-ID render pass is URP renderer work that V cannot see working (no Player), so it costs a full build round to find out.

**Fix:** state the method:
- world = frame minus HUD rects minus FeelProbe enemy rects minus the sky region (above the horizon row)
- tells = hue band 45-70 deg inside the boss rect

Add "role pass: out of scope".

## 7. V1 contradicts colors.md in four places (contradiction; NIGHT56.md 122-126 vs colors.md 63, 143-144, 226-263)
- (a) "The key colour on 1-5 % of the frame". In colors.md the key is the walls + fog (ice line walls and fog ultramarine; method 7: key
  50-70 %, accent 3-10 %). The prompt merges key and accent.
- (b) "The five keys pairwise >= 40 deg apart (or one achromatic)". colors.md proposes two near-neutral keys (kiln S <= 0.18, spire
  S <= 0.08), so its own script fails the literal test. Kiln's cobalt accent (H 222-230) also sits on ice line's key (H 215-225).
- (c) Kiln's key H 25-40 overlaps the red-orange band 5-35, so a hue-only check flags it.
- (d) "decor55's accents are the starting point". decor55 (283f3c41) put foundry's accent at 72 deg, next to yellow 45-70.
  colors.md's foundry key is 125-140.

**Fix:**
- Key = fog / sky / landmark hue, at any coverage. Accent = 1-5 %, checked pairwise >= 40 deg.
- Neutral keys pass by colors.md's own S / V rule (line 63).
- Kiln's key is checked by S (<= 0.18), not hue.
- Say which source wins for foundry (decor55 or colors.md).

## 8. L1 / L2's "in view" contradicts levels.md R5 (contradiction; NIGHT56.md 86-90 vs levels.md 65-78)
- R5's numbers:
  - "No enemy spawn point is within 8 m of the entry door, and none has line of sight into the entry corridor"
  - at the entry frame, ">= 3 enemies ... visible beyond the foothold, and none is closer than 8 m"
  - a full-height foothold 4-8 m inside the entry, which blocks the line of sight L2 asks for
- NIGHT56 asks for ">= 2 spawn nodes within 15 m and in line of sight of the entry point" and ">= 4 visible". Two engineers will place
  nodes differently.
- Also, most rooms spawn from `r.nodes` (n52-foundry has only 4 `waveNodes43` entries; Campaign24.cs:913). Moving `r.nodes` moves
  more than spawns: highNodes merge into the same list (Campaign24.cs:323-324).

**Fix:** "in view" = 8-15 m from the entry door, line of sight from the foothold's eye height, none into the corridor; one count (>= 3).
Add per-wave `waveNodes43` rather than moving `r.nodes`.

## 9. rank56's targets do not match the game's rank or meter (ambiguity + cannot be measured; NIGHT56.md 105-108)
- The ranks are DULL, COOL, BOLD, AIRBORNE, SAVAGE, SKYGRIND (6; PortExport/config.json ~208-233), not D / C / B / A / S.
  "Reaches A" and "D x1.0 ... S x2.5" have no mapping.
- Neither n46flow nor n47human reports style (no rank / style in either harness). The ported StyleSim.cs has golden-pinned pace targets
  ("varied: SAVAGE in 35-60 s"), and "A within 20 s" fights them.
- The "~100 % DULL" baseline comes from verbs37 reels (critic-ggproxy.md:85), not combat, so it is not a valid "before".

**Fix:**
- Name the mapping in the row's numbers, for example A = SAVAGE.
- Measure with a StyleSim profile plus a style column added to n46flow, baselined at u3-n55 with the row off.
- Keep StyleSim's golden records equal with the row off.

## 10. Worktree builders cannot read their planning (cannot be done as written; NIGHT56.md 9, 160-165, section 4)
- NIGHT56.md is untracked by rule (CLAUDE.md rule 1), and Research/2026-10-20-wave56/ is untracked. Neither exists in a builder's
  worktree.
- NIGHT55's shared commit 6a424cf5 committed its research + builder brief. Section 4 of NIGHT56 does not list them.
- `Tools/night/n55k-teach.cjs` (P0-gate55) exists only on K's branch (1b996462). It needs that pick first.

**Fix:** the shared commit adds Research/2026-10-20-wave56/{levels,enemies,colors,gameplay,IDEAS-POOL,builder-brief}.md, and the spawn
template reads NIGHT56.md by its absolute path C:\Users\gaige\dev\SKYGRIND-unity\NIGHT56.md.

## 11. G3 and seeded item 3 restate shipped systems (ambiguity / duplication; NIGHT56.md 112-116, 138)
- "At most two attack wind-ups at once" is already ON: unity-config.json design...tokens41 `enabled: true, cap: 2`. G3's new parts are
  only the on-screen preference, accuracy vs speed, and idle advance.
- fresh56 ("weapon freshness") duplicates Freshness19 (NIGHT19/20, design.freshness: FRESH / USED / STALE / DULL, the HUD tag). A second
  freshness row would double-pay style, and turning one off may lose a verb's HUD read (PRINCIPLES 1).

**Fix:** G3's doneWhen names only the three new rules. Seeded #3 becomes "retune design.freshness behind rank56", or it is dropped.

## 12. G4 runs on the old kiln (ambiguity; NIGHT56.md 117-118)
`n47human --boss wyrm` defaults to `--level n46-kiln` (Harness.N47.Human.cs:110). The shipped level 3 is n52-kiln, and NIGHT49's
122.6 s baseline is on n46-kiln.
**Fix:** `--level n52-kiln`, re-baselined at u3-n55 (40 seeds, about 2 min) before any wyrm55 change.

## The weakest new feature: V1 colour56 (the saturation cap and neutral keys)
The owner's words point the other way, three times:
- w16: colours "bland"
- 2026-10-19: "the colors and stuff are way too bland"
- 2026-10-20 14:05: decor "lack[s] strong color direction"

V1's doneWhen caps the world at 95 % of pixels with S <= 0.55 and none above 0.8. colors.md's script makes kiln a warm neutral
(S <= 0.18) and spire "achromatic ... the colour-starved climax" (S <= 0.08). Kiln is also the worm's level, where GG asked for "the same
yellow-red-orange color theme" (NIGHT47). A meter-passing V1 can make two of five levels greyer than tonight's build, and that is the most
likely reversal: "it's more bland."

**Fix:** measure the key's presence, not only a cap:
- each level's key or accent at S >= 0.35 on >= 3 % of the frame
- no level with a neutral key
- the blind pair asks "which has more colour direction", not "has a colour"

Runner-up: rank56. GG on 2026-10-20 ~14:05: "don't change the core mechanics either". It rescales heal36 and the RIDE KILL refund and
adds a rank loss per hit.
