# NIGHT52.md: THE ARENA NIGHT. Rebuild the combat rooms so the movement is the fight (levels 3 and 1 first, then a room loop), with a polish lane beside it (lead + three builders; 8 h, hard cap 8 h, minimum 7 h from GG's go)

GG's words (2026-10-19 ~22:00, verbatim; they outrank everything below):
> can we plan next detailed goal overnight promptr that will go for 8 hours, please plan and build this pormpt for 30 mins also be honest and
> critical on where we still need to imporve and polish, follow best methods on how to prompt and get best results, please compare other open
> source porjects, copy, use skills and roleplay hard cap 8 minimun 7 and it all relate dto building and itterating.

GG's standing asks this night answers (docs/memory/OWNER.md, verbatim): "every single level needs a complete arena overhaul" (w28); "a lot of
the rooms to be a lot smaller and more action packed like ... Ultra Kill" (w28); "The rooms still don't incentivize wall-riding or creative use
of the weapons; many of them feel very boring" (w29); "the arenas don't incentivize creative movement at all" (w33); "random lava pits random
pillars that serve no purpose" (w35); level 3 "just isn't good ... encourage far more mechanics, have a new arena" (w38); "please dont just
randomly plce stuff it needs intension and personality" (w44); "There always really needs to be a route that enemies can climb up" (w19).

**Where we are (NIGHT51, play build `Builds/n51-r9`, commit d9484485; NIGHT51-REPORT.md, HANDOFF.md):** NIGHT51 made the world read as built
(panels, bevels, ceilings, outlines, scorch marks) and the kills hit harder. It did not touch the rooms. Research behind this prompt:
`Research/2026-10-19-wave52/RESEARCH.md` (arena rules with numbers, open-source sources to copy, prompting evidence). Read it at P0.

## 0. The honest read (why tonight is rooms, measured, not adjectives)
1. **The combat rooms are big and nearly empty.** `python -I Research/2026-10-19-wave52/density-baseline.py` (the plan files): pieces of
   geometry (boxes + ramps + pads + grapple anchors) per 100 m2 of combat room: **kiln 0.33-0.60** (kb3 is 40 x 46 m with 4 boxes and 2
   ramps), iceline 0.00-1.10, spire 0.95-1.67, span 1.39-1.70; boss rooms 0.00-0.41. **Not one jump pad in any level** (grapple anchors: span
   22, spire 5, none where the grapple is not owned). A room with nothing in it asks for no verb: you stand on the floor and shoot. That is
   GG's complaint, six times.
2. **Fifty nights of process, little play.** GG has played none of n44-n51. Nights 49-50 spent hours judging pixel deltas. Tonight's share of
   minutes that build is a number in the report (section 7), target >= 70 %.
3. **Still cheap or unfinished (the polish lane, section 4):** the HUD (GG: "looks so AI generated", "too generic and cheap"); the green health
   bar is outside the palette; the enemies read as glowing jelly up close (rim boost); the THUNDERCLAP / KESTREL viewmodels are flat blocks; the
   smoke fails four pad steps on this machine (a Bluetooth Pro Controller is paired: not proven to be the cause).
4. **Not tonight (do not start):** new enemies or enemy models (GG's design), audio, a sixth level, boss rework, Core movement, new weapons.

## 1. The rows (data: `Tools/night/n52-rows.json`, written at P0, every row `passes: false`; only the lead flips `passes`, after printing its check; never edit a row's `check`)
| id | row | owner | switch | check that flips `passes` |
|---|---|---|---|---|
| P0 | baselines + shared commit | lead | - | density, flow table, verbs37 and campaign clear rates printed for kiln and foundry; the shared commit's gates green |
| K | **Level 3, THE KILN, rebuilt rooms** (kb2, kb2c, kb3 rebuilt to the ROOM CONTRACT; the boss room untouched; links re-cut only where a moved door needs it, keeping their lessons) | builder K | level row `kiln52` (n47-kiln -> n52-kiln) | every rebuilt room passes the ROOM CONTRACT (section 2) with its numbers printed; verbs37 25/25 on n52-kiln; levelgen report clean; the bot clears every room 10/10 seeds eventually, first-try 50-95 % |
| F | **Level 1, THE FOUNDRY, rebuilt combat rooms a5 (1.5 CRUCIBLE, 1.04) and b1 (1.6 THE YARD, 36 x 40 m, 0.42)**; the teach rooms a2-a4, a8 and the boss a9 keep their lessons and are exempt from the contract | builder F | level row `foundry52` (n48-foundry -> n52-foundry) | the same for a5 and b1, verbs37 26/26 on n52-foundry, a9 bit-equal to n48-foundry's |
| U | **The polish lane** (section 4: HUD, enemy surface, viewmodels) | builder U | view rows `hud52`, `skin52`, `guns52` | each item: before / after frames at 1080p looked at by the lead and logged, its Player proof line RED once and GREEN once (L-014), nothing in verbs37 lost |
| L | **The room loop** (section 5: the other levels' worst rooms, one room a cycle) | lead, then K and F after their rows | `iceline52`, `spire52`, `span52` (all created at P0) | per room: the ROOM CONTRACT printed and passing; the level's verbs37 unchanged |
**Fallback for K, F and L (they ship something):** each rebuilt room that passes ships in the new level id; a room that misses the contract
twice is put back to its n47 / n48 version inside the n52 level, and the row closes SHORT with "N of M rooms rebuilt". A level row goes ON
at the freeze only if at least one of its rooms was rebuilt and its verbs37 holds. R closes SHORT if a review did not run, with the reason.
| R | **The persona reviews** (section 6) | fresh subagents | - | each level row gets the two reviews; every concrete finding fixed or answered in DECIDE52 |
| Z | **Quiet freeze and close** | lead | - | section 8 |

## 2. THE ROOM CONTRACT (what "rebuilt" means; numbers are hypotheses from RESEARCH.md section A: change one with a logged reason, never silently)
A combat room passes when the lead prints all of these for it. Items marked (C) are computed by `Tools/night/n52-room-check.py <plan> <room>`
(written at P0 from `density-baseline.py`), by levelgen's report or by the bot; items marked (J) are the lead's judgement from the room's
card and its SVG, logged with the SVG path (no tool measures them yet; do not spend the night writing one). Teach rooms (a one-verb lesson
room, per ROOMS.md) and boss rooms are exempt.
1. **Size (C):** the room rect 20-32 m on its long side (2.5-4 s of a walk at 8 m/s; Quake's "large" room is ~26 m, RESEARCH A1); height
   10-16 m. A rebuilt room may shrink its rect: its doors move with it and the adjoining links are re-cut to meet them (links keep their
   lessons). Density divides by the rect's area. GG: "a lot smaller and more action packed".
2. **Density (C):** >= 2.5 pieces per 100 m2 (today's best combat room is span p5 at 1.70), and every piece carries a `job` and a `why` naming the
   verb it serves (the plan format has both). A piece with no verb is cut. No waist-high crates as cover (they reward standing: RESEARCH A11,
   "common mistakes"); pillars break lines of sight. GG: "it needs intension".
3. **Tiers:** exactly 3 standing planes (C: floor + two; a 4th adds nothing, RESEARCH A4), each reachable by the player by >= 2 different
   verbs (J), and **every perch reachable by at least one walking enemy type** (C: levelgen's tier row; no safe ledge, RESEARCH A12; GG w19).
4. **A circuit and 1-3 lanes (J):** a loop around a central pillar or mass the player can run without stopping (no dead end longer than 6 m),
   so movement is the defence (RESEARCH A6, A7); the entrance gated (the door locks behind, or a one-way drop: A14).
5. **Verbs asked (C for the pieces, J for the verb):** the room's ROOMS.md row names ONE verb it teaches or tests (no two rooms share their
   30 s problem) and the room holds at least: one ride wall >= 17 m long at ride height (1.48 s x ~11.7 m/s = 17.3 m, the ride verbs37
   measured on foundry's 20 m wall), one upward line by boost or a jump pad
   (the game has zero pads today; upward flow must be built, A5), and, where the grapple is owned, two anchors across the room.
6. **Pickups and spawns pull you through (C):** health pickups (`healthPickup` props; there is no ammo pickup kind) on the exposed part of
   the circuit (the best at the riskiest spot, A20); spawns on at least two tiers, never within 6 m of the entry door, and within 100 deg of
   the door's inward direction (never behind a player who enters looking forward: A18, the pad player).
7. **No camping spot (C):** `dwell` in the flow meter is seconds within 4 m of the entry door while an enemy lives
   (Tools/SimHarness/Harness.N46.Flow.cs:24): it must be no worse than before. Camping itself is measured by a **hold probe** builder K
   writes in round 1 (a SimHarness command: the bot stands 20 s on each tier's best spot and counts the hits it takes; a spot with 0 hits
   fails; GG w29-w30: "you can camp"). 2-3 enemy types per wave (A15).
8. **Proof:** a top-down SVG (`node Tools/night/level-svg.cjs <plan> <svg> --rooms <id>`), three Player frames (entry, mid-fight, the highest
   tier) looked at by the lead, and its ROOMS.md row updated. The bot (`campaign <plan> --seeds 10`) clears the room 10/10 eventually
   (never stuck), first-try 50-95 % (a hypothesis: logged, not a target to make the bot die), and its par recomputed.

**The pipelines (the forge's dry run; follow them exactly):**
- **Kiln:** copy `Design/levels/n47/kiln.plan.json` to `Design/levels/n52/kiln.plan.json`; edit; `dotnet run --project Tools/SimHarness -c
  Release -- levelgen Design/levels/n52/kiln.plan.json Design/levels/n52-kiln.json --walk`; then the worm arena's finish step, as n47-kiln
  had it (`node Tools/night/gen-n47-kiln.cjs --finish`: copy it to gen-n52-kiln.cjs pointed at n52); both map copies byte-identical.
- **Foundry:** its geometry is `Design/levels/n46/foundry.plan.json` (levelgen -> n46-foundry); n47-foundry and n48-foundry only add data to
  room a9 (gen-n47-foundry.cjs, gen-n48-foundry.cjs). So: copy the n46 plan to `Design/levels/n52/foundry.plan.json`, edit a5 and b1,
  levelgen to n52-foundry, then re-apply a9's keys with a gen-n52-foundry.cjs written from those two generators, asserting a9 bit-equal to
  n48-foundry's a9.
- **The loop's levels:** the same recipe from `Design/levels/n47/<level>.plan.json` (read each level's generator chain first: some add a
  finish step, as kiln does).

## 3. P0, the lead (0:00-0:45; `date` on every log line into NIGHT52-LOG.md)
1. `date`; `git log -1` (expect d9484485 or later on `port/stage3`); read this file, CLAUDE.md, HANDOFF.md (NIGHT51), NIGHT51-REPORT.md,
   Research/2026-10-19-wave52/RESEARCH.md, docs/DESIGN-RULES.md, docs/ROOMS.md (levels 1 and 3), Design/levels/LEVEL-FORMAT.md,
   docs/LEVEL-DESIGN-NOTES.md (how kits were measured), Tools/night/gen-n48-foundry.cjs and n48-shared.cjs (the level-row pattern).
2. **Baselines (printed, logged):** `python -I Research/2026-10-19-wave52/density-baseline.py` (all five levels, foundry from the n46 plan);
   `Tools/night/n52-flow-table.sh <level ids...>` written from n47-flow-table.sh (which hardcodes the n46 / n47 ids) so it takes ids as
   arguments, run on n47-kiln and n48-foundry; campaign clear rates and pars (10 seeds) for kiln and foundry rooms; the SVGs of kb2, kb2c,
   kb3, a5, b1 into `Logs/n52/svg/`. `git diff --stat` of the dirty files (L-047): GG's four, the four font SDF assets, anything else; log it.
3. **The shared commit** (`Tools/night/n52-shared.cjs`, from n48-shared.cjs: dry run, every anchor asserted once, refuses to run twice):
   `design.n52` with **all five level rows** `kiln52`, `foundry52`, `iceline52`, `spire52`, `span52` (`enabled: false`, `levelFrom` /
   `levelTo`, their `design.maps.list` rows; placeholders = byte copies of the current maps with only the name changed, both copies + the
   StreamingAssets .meta), so the loop never needs a second config commit while builders are out (L-049, L-065); the view rows `hud52`,
   `skin52`, `guns52` (off); `Tools/night/n52-rows.json`; `Tools/night/n52-room-check.py`; FeelReel hooks for the U items. Both config
   copies byte-identical; golden 0 mismatches / 22 exact; fingerprints plain / --off50 / --off52 EQUAL; compile clean.
   Commit, then spawn K, F, U (worktrees). The lead writes NIGHT52-BRIEF.md (untracked) from NIGHT49-BRIEF.md, the last brief on disk
   (there is no NIGHT50-BRIEF.md): 49 -> 52, `--off52` is NIGHT51's game, plus sections 2, 4 and 8 of this file.
4. **Skills (load each when its step starts, not before):** `unity:unity-cli` before driving the Editor (CLAUDE.md); `game-feel` for U's
   hit shake and heal pulse; `reverse-engineer-anything` only if a builder ports a technique from a binary or bundle (not expected tonight);
   `wave-memory` at the close (the harvest: ledger wave-52, LESSONS, THREADS). Port every copied source into docs/THIRD-PARTY.md with its
   licence (RESEARCH.md B lists the permissive ones and the GPL ones to avoid).
5. **Builders are personas with checks** (RESEARCH.md C: a role sets focus and tone, not skill; the checks decide). K and F are told:
   "You are a combat-arena designer in the school of DOOM Eternal's arenas and ULTRAKILL's layers: every piece of geometry exists for a verb,
   the room is small, the fight moves." U is told: "You are a UI and character-surface artist shipping an ULTRAKILL / DOOM-style HUD."

## 4. The polish lane (builder U; one candidate per item, two rounds, a fallback that ships)
| item | what | copy from (RESEARCH.md B; permissive licences only, logged in docs/THIRD-PARTY.md) | fallback |
|---|---|---|---|
| hud52 | the health and charge block rebuilt: health in the palette (bone / cyan, red-orange only below 30 %), a hard-edged ULTRAKILL-style panel with a hit shake and a heal pulse; the weapon block the same family | TMPEffects (MIT) for text punch, LitMotion (MIT) for tweens, UIEffect (MIT, already in the project) | ship the colour change only |
| skin52 | enemy surface: the rim cut close up (the jelly read), a ramp-lit body, ink51's outline gaps on flat-shaded corners closed | NiloCat URP toon lit (MIT) for the ramp; QuickOutline's smoothed normals in a UV channel (MIT) for the gaps | the rim's near cut only |
| guns52 | KESTREL and THUNDERCLAP viewmodels: chamfered parts, two-tone panels, a lit edge, no new silhouette (GG keeps the guns' shapes) | Cyanilux URP templates (CC0); stone51's face-sized bevel (already in the stone shader) | KESTREL only |
Every U item: a view row (off = NIGHT51's look, a golden test of the off state), frames at 1080p before / after, `-skygrindOff52` covers it.

## 5. The room loop (from about 4:30 until the freeze at 6:30; the lead runs it, K and F join when their rows pass)
A cycle is one room, at most 45 minutes: pick the lowest-density combat room left (the queue below), write its card (verb, 30 s problem, the
circuit, the tiers), build it in the plan, `levelgen`, the room check, the bot, three Player frames, keep or revert (a room that misses the
contract twice is reverted and logged SHORT). Commit every kept room.
Queue (seeded, lowest density first; extend it, never run out): iceline g6 (0.00), g1 (0.35), g7 (0.52), g5 (0.77), g4 (0.83); spire v5
(0.95), v6 (1.02); span p1 (1.39), p3 (1.56); foundry's remaining rooms. Parked for GG (DECIDE52, not tonight): a TrenchBroom / CSG blockout
pipeline (Scopa, Chisel; MIT) instead of hand-written plan rows. When the queue's rooms are all done, the leftovers list (section 9) is next. **The night does not end when a list ends.**

## 6. The persona reviews (fresh subagents, read-only, one call each; they propose, the checks decide)
At each level row's first green and again at the midpoint (~4:30), two reviewers get ONLY the level's SVGs, three frames per room and the
room-check output:
- **The P-rank runner** (an ULTRAKILL P-rank / DOOM Nightmare player): "Where does my speed die? Where is the loop? Where would I camp?"
- **GG** (given docs/memory/OWNER.md's lines verbatim and nothing else): "What would GG call random, boring or cheap here?"
Each returns at most five findings, each pointing at one piece of geometry by its id. The lead fixes or answers each in DECIDE52.md. A
reviewer never says GREEN; only the lead does, from the checks.

## 7. The clock (8 h; the hard cap is 8:00 from GG's go; the freeze starts by 6:30 and the close by 7:30 whatever is unfinished)
| time | lead | builders |
|---|---|---|
| 0:00-0:45 | P0 (baselines, shared commit, spawn) | |
| 0:45-2:15 | integrate as rooms land; Player reels on kiln / foundry; frames looked at | K round 1 (kb2, kb2c, the hold probe), F round 1 (a5), U round 1 (hud52, skin52) |
| 2:15-3:45 | persona reviews on round 1; orders from findings | K round 2 (kb3 + review fixes), F round 2 (b1 + fixes), U round 2 (guns52 + fixes) |
| 3:45-4:30 | **midpoint build `n52-mid`**: rows ON that pass; verbs37 x5; STOP POINT | finish, report |
| 4:30-6:30 | **the room loop** (section 5); persona review at 4:30 | K and F join the loop (one level each, file ownership by level) |
| 6:30-7:30 | **quiet freeze, builders stopped** (one Player at a time, in this order): rows ON by their checks; `n52-freeze`; verbs37 x5; flow table on the changed levels; FULL batch suite (15-19 min); smoke (log the paired controller's state first: `Get-PnpDevice -PresentOnly` for a "Pro Controller"); the before / after sheet | stopped |
| 7:30-8:00 | close: one fresh reviewer gets only n52-rows.json and the evidence paths and reports rows the evidence does not support (correctness only); fix; NIGHT52-LOG close table, NIGHT52-REPORT.md, HANDOFF, DECIDE52, THREADS, the wave-memory harvest, tag `u3-n52` | |
**Minimum 7 h:** if every row passes early, the loop and the leftovers continue until 6:30; the freeze and the close then run past 7:00. A STOP POINT line (the close table so far) goes at
the top of NIGHT52-LOG.md and HANDOFF.md at every build and every 30 min, committed. **Building share:** log each block as BUILD or CHECK;
the report prints the share (target >= 70 % BUILD).

## 8. Hard rules (CLAUDE.md in full, plus these; each has bitten a night before)
- Never commit master, push, merge; builders in worktrees, the lead cherry-picks (memory: builder-merge-blocked); explicit paths + `.meta`;
  never stage GG's four files or UnityConnectSettings; restore the TMP fallback asset before a commit; NIGHT52.md and DECIDE52.md untracked.
- Movement exact: golden 0 mismatches, 22 replays exact, fingerprints EQUAL at every commit that touches Core (tonight should touch none).
- A level change ships only as a NEW level id behind its level row (off = tonight's level file untouched, bit for bit; L-065 / L-077).
- A campaign level boots in a Player only via reel.sh + `-skygrindReelOnly` (L-062); walk every changed level with the bot (L-041) and boot
  it in a Player the same round (L-043).
- Numbers in data (plan files, config, UnityTuning with a comment); docs and code patches with Write / Edit only (NIGHT51 slipped once with
  an inline Python heredoc: don't).
- One Player at a time; never the suite or a bench beside reels (L-005); the reels that exercise kills are the survival kill6 / kill12
  scenarios, not a249 or verbs37 (memory: reel-kill-coverage).
- Palette: cyan usable, red-orange enemies and danger, magenta RIVAL only, pale violet-white the boundary; UI text >= 16 px at 1080p.

## 9. Leftovers (approved polish, in order; take the next one whenever a list ends; never stop for lack of work)
1. The scorch mark's hot core: a frame that shows it (or raise `_Glow`), NIGHT51.
2. The muzzle LIGHT in real time like star51 (LightPools.flashFrames).
3. Iceline's and span's grade (look49 OFF there: the saturated band; T-116).
4. The kill wash source (T-111) if the kit still sits at +0.27.
5. Teach lines that wait for the verb, not a timer (UiTeach35 lines that only time out).
6. ROOMS.md rows for every rebuilt room; docs/VERBS.md if a verb's check moved.
7. THREADS.md: T-111..T-116 written, NIGHT51's and NIGHT52's opened and closed.

## 10. Stops (Opus 5.5 ends unattended runs early on a text-only turn; these are not stops)
Not a stop: a summary that announces the next step without the tool call; an offer to continue; a list of decisions that block nothing; "a
good place to report"; a list that ran out (take the next queue item or leftover). The only stops: 8:00 by a printed `date`, or the close
table printed after 7:00 by a printed `date`. GG will not answer: every call goes in DECIDE52.md with the rejected alternative. If a usage
limit pauses the run, the clock keeps counting: resume from the STOP POINT line, do not redo committed work.

## 11. Done when
Every row in n52-rows.json is `passes: true`, or SHORT with its fallback shipped, or OFF with a reason, in the close table with evidence
paths; `n52-freeze` passes verbs37 on all five levels (26 / 20 / 25 / 30 / 30, nothing lost), the full suite green, golden / fingerprints
EQUAL; the report prints the density before / after for every rebuilt room and the building share; tag `u3-n52`; `git status` shows only the files that
were dirty at P0 and logged there (GG's four, the four font SDF assets) and untracked prompts.
**The forge experiment this night tests:** a ROOM CONTRACT with numbers instead of judge panels, and a seeded room loop for a minimum-length
run. Measured at the close: combat rooms rebuilt that pass the contract (target >= 8), and the building share (target >= 70 %).

## Closing reminder (the rules long nights have dropped)
Commit every slice (a usage stop has eaten hours before). Look at every frame you claim (path in the log). A list running out is not the end
of the night: the room queue and the leftovers are. Personas propose; checks decide; only the lead writes GREEN.

## Kickoff (paste into a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`)
```
/effort high
You are the lead of NIGHT52, THE ARENA NIGHT. Read NIGHT52.md whole (GG's words are at its top), then CLAUDE.md, HANDOFF.md, NIGHT51-REPORT.md
and Research/2026-10-19-wave52/RESEARCH.md, and run the night as written. Eight hours, hard cap 8 h from my go, minimum 7 h. I will not
answer: decide every call in DECIDE52.md and keep going. P0 first. Start with `date`.
```

## /goal (paste after the kickoff)
```
/goal Met when this conversation shows (a) NIGHT52-LOG.md's close table printed in full (rows P0, K, F, U, L, R, Z from Tools/night/n52-rows.json, each passes: true, SHORT with its fallback shipped, or OFF with its reason, with evidence paths) AND a printed `date` at least 7 h after the kickoff, and (b) the output of `git tag -l u3-n52`; or when a printed `date` shows 8 h since the kickoff (the close starts by 7:30 whatever is unfinished). Print `date` at the start of every turn. Run NIGHT52 nonstop as the lead; never ask me; every call in DECIDE52.md with the rejected alternative. Order: P0 baselines and the shared commit; builders K (kiln rooms), F (foundry rooms), U (HUD, enemy surface, viewmodels); every rebuilt room must pass the ROOM CONTRACT (size 20-32 m, density >= 2.5 pieces per 100 m2, 3 tiers enemies can reach, a circuit, its verbs, no camping spot) with its numbers printed (teach and boss rooms exempt); persona reviews propose, checks decide. From about 4:30 the room loop takes one room per cycle from the queue until 6:30; a finished list is not a stop. Quiet freeze 6:30-7:30 (verbs37 x5, flow table, full suite, smoke), close 7:30-8:00. STOP POINT line at every build and every 30 min, committed. Commit every slice.
```
