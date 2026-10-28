# NIGHT48.md: finish GG's 45 asks. The 18 SHORT rows of NIGHT47 closed as GREEN (built, measured AND seen on Player frames by the lead), the 8 R4 rows framed and judged, the blind pairs run, the baseline moved (lead + four builders; at least 6 h, hard cap 7 h 30 min)

GG's words of 2026-10-18 are at the top of `NIGHT47.md` and still outrank everything below; read them twice. NIGHT47 closed them as
**27 GREEN, 18 SHORT** (`Logs/n47/ASKS.md`, tag `u3-n47` = 1d74296d, play build `Builds/n47-r3`). Tonight there is one goal: **every
one of the 45 rows GREEN**, where GREEN means three things at once: (1) built behind its row, (2) its done-when measured with a number,
(3) its Player frames looked at by the lead and named in the ledger. "Built but not looked at" is not GREEN. A row that cannot reach its
number after two real attempts tonight is SHORT again, but only with a NEW measured reason, a third approach named, and its frames taken.

**GG will not answer during the night.** Every call goes in `DECIDE48.md` (untracked; write it with the Write tool, and never let a builder
commit it: NIGHT47's DECIDE47.md was committed by a builder by mistake). Base: `port/stage3` at `u3-n47`; check `git rev-parse u3-n47`
equals HEAD, else base on HEAD and log it. t0 = GG's go. The lead writes `date` on every log line.

## 0. What NIGHT47 taught (read before anything; each is a hard rule tonight)

| lesson (NIGHT47) | rule tonight |
|---|---|
| The usage limit stopped the whole session 03:28-06:28 with no stop point written at the time | **The lead writes a STOP POINT line at the top of NIGHT48-LOG.md and HANDOFF.md at every build and every 30 min** ("if the night stops now: last commit X, building Y, next step Z"), and commits it. A stop is then clean whenever it comes. |
| Builders stopped after every fix and waited to be resumed | each builder's prompt says: **work through your whole list without stopping; one report per round**; the lead resumes a stopped builder at once with its next item |
| "Built" rows were never looked at (visors, shield, swarmer, LOOM hull, 720p text) | **no row closes without its frames in the ledger**; the lead looks at every row's frames at its round's build; a reel whose frames cannot show the thing (the a847 lineup at 10 m behind the viewmodel) is fixed first |
| Reels drove the bot's input, not the player's (a747 "blasted NO" for 3 builds) | every reel that presses buttons uses `sim.UseInput(g.playerInput)` + the virtual pad as `Gamepad.current` (F's ceb58fc5 recipe); its line prints `sim input ReaderInput, pad current True` |
| The live Editor stopped answering at 03:30 and never came back | **the suite runs in batch mode** on the batch copy (`bash Tools/night/n27-batch-tests.sh <sha> <tag>`: 2194 tests in 656 s); the live Editor only for quick compiles; never wait on it |
| Two config conflicts came from builders adding rows at the same place | the lead pre-places EVERY new row tonight in the shared commit (section 5); a builder adds a row only by asking the lead |
| A blind pair was planned for every look and none ran | the blind pairs run in round 2, by the lead, with `Tools/night/n46-blind-pairs.cjs` and 3 fresh judge subagents; budgeted, not optional |
| docs edited by sed / heredoc (rule 9) twice | Write / Edit only for every doc, log and ledger, including Logs/n48/ASKS.md |

## 1. The rows to finish (18 SHORT + the open items; owner in brackets; each with tonight's done-when)

**THE WICK (builder W)**
- **G1 the climb's top third.** NIGHT47: bottom 97.4 % first tries (met), top third 58-78 % (target 80 %): every traced miss is a 150 ms
  late release running past a panel's end; coyote 0.25 s, catchBelow 2.6, 9.5 m panels and a 2.6 m rise all failed. Third approach:
  **a ledge lip at every top-third panel's end** (a 0.6 m catch shelf the late release lands on, then a short hop to the next panel), or
  the top third as panel -> ledge -> panel instead of panel -> panel. Done when: the sloppy pilot's top-third first tries >= 80 % on 40
  seeds, bottom still >= 95 %, 39+ / 40 reach the cap, the climb 25-45 s, and frames of each top panel from its start (the next hold in view).
- **G5 fight text at both resolutions.** Done when: a147 / a247 / a347 / a447 / a547 boss frames at **1280x720 and 1920x1080** (the reel
  takes both: `-screen-width` / `-screen-height`), each fight line >= 36 px at 1080p and its 720p twin readable (>= 24 px), one line at a
  time, no raw tags (a647's guard line `text objects 1, raw tags no`).
- **G9 W's three ideas** (wickRideParry47, wickIntro47, wickFinale47: built, OFF). Done when: each framed in a147 (W47-g9-intro,
  -rideparry, -finale, -finale-2), the lead's look written, verbs37 level 1 PASS with them on, then ON or retired with a reason.

**Enemy looks (builder F)**
- **G12 the WARDEN's shield, G13 visors, G14 the small robots.** NIGHT47's a847 lineup put the enemies small behind the viewmodel at 10 m,
  so off vs on could not be told. First: **a lineup reel that hides the viewmodel and the HUD** and stands each kind alone on a neutral
  plate, the camera at 3 m (close-up), 10, 20 and 30 m, front / side / 45 deg, off then on, colour + grayscale. Done when: the lead sees the
  shield centred on the chest and clear of the body in every pose (front / side / 45 / blocking / down), the angular visor band with temple
  points on every kind at 10 and 20 m, the swarmer reading as a pill on four pointed legs at 20 m; a 2 s gait strip of the swarmer; blind
  pair (old vs new) per kind picks the new one (3 judges; a tie keeps the new one ON and goes in the report).

**Weapons (builder F)**
- **G20 the LT blast.** Built (14.4 m throw, pink rings). Done when: the a747 blast contact sheet off / A "ring" / B "lance" with the husks
  frozen at 3 / 5 / 7 m and the wash meter GREEN (no frame's reticle box > +0.25 over f-2), and **the blind pair A vs B run** (3 judges:
  "which LT blast hits harder?"); the winner ON.
- **G24 every weapon heavier.** Done when: per gun (KESTREL tap + charge, THUNDERCLAP shot / slug / boost, PULSE punch + blast, the fist,
  the TORCH, the swap) a contact sheet -2 .. +30 off vs on, the "G24 ladder" line GREEN (no gun lighter than today: DECIDE47 D-14; the
  PULSE swing in its 1.5-2.5 deg band), and one blind pair per gun ("which hits harder?"), the new one picked or tied.
- **G43 the flame.** The TORCH (F) + THE CROWN's flame (L). Done when: contact sheets of both, the blind pair L47-fire-sweep vs -sweep-B
  (3 judges), the TORCH sheet off vs on; heat haze: decide in DECIDE48 whether a haze shader goes in (Resources folder + a Player frame
  check for magenta, as Column47 passed) or stays cut.

**Level 2 + 3 (builder B)**
- **G29 THE ICEBREAKER's animation, G30 its hit effects.** Done when: a motion strip (12 frames over 1 s) of each attack's wind-up ->
  strike -> settle from 9 m (anticipation and overshoot visible), a flinch strip on a hit, and per attack the impact frame showing its own
  tier (light / heavy / critical); the lead's look written per attack.
- **G31 the worm's head-charge tell (0.5 s).** Two approaches failed (the 0.4 s lean before the tell: 15 / 40 killed). Third approach: **change
  the counter, not the timing**: the head glows yellow for the whole 1.0 s tell and a punch ANYWHERE in it (or a parry) staggers, so a longer
  tell no longer makes the punch harder. Done when: every tell >= 0.8 s, the human pilot (with the boost policy) >= 38 / 40 killed, band held.
- **G32 the molten worm look.** Done when: a347 frames at emerge / coil / breach / damage flash (wyrmLook47 on vs off) looked at; blind pair
  "which looks like molten metal?".
- **G33 the arena rewards the boost.** The fixed-policy meter cannot move (5.3 vs 5.6). Change the measure to what GG asked ("incentivize"):
  **a human pilot that boosts only when it pays** (a breach under it, the core open above, an island in reach) and count boosts per minute in
  today's arena vs n47-kiln's; if still under 2x, make the islands pay more (the core's x3 only from an island or mid-boost; a health pickup
  on an island). Done when: >= 2x boosts per minute, the band held.
- **G34 two burrows a minute.** Every coil cut lost the fight because the pilot punches the head in the coil. Third approach: **the burrow
  as the punish**: after a stagger the worm always dives (it flees under the floor), so burrows rise with the player's success. Done when:
  >= 2.0 / min median, 40 / 40 fights see one, the band held.
- **G35 the worm earlier: the glimpse frame.** Done when: B47-beat-glimpse shows the worm's molten back in the slot (timed to the arc's top),
  plus the pass and cameo frames looked at; the walk 10 / 10 on n47-kiln.
- **The worm's min 46.8 s** (under 60): one lever that lifts the min without pushing the median past 150 (a cap per window on the
  share of the pool a single stagger can take).
- **L3's stall share 31.7 -> 40.3 %** (the close's flow table, past the band) and **L2's clear 184.8 -> 220.4 s**: find which rooms
  (n46flow per room) and either fix them or write the reason (L2's is GG's harder STAMPEDE: the lead may accept it in DECIDE48).

**Level 4 (builder L)**
- **G36 THE LOOM's design and colours, G37 its animation.** The bronze / brass shuttle hull (1de139da) was never framed. Done when: a447
  shuttle frames at 10 and 20 m show a long pointed bronze shuttle with brass bands and amber lamp eyes (no grey box), a leg-step strip
  (12 frames), an attack's anticipation strip, a hit flinch; the lead's look written; a blind pair n46 LOOM vs n47 LOOM.
- **kneeHold47** (L's R4 row): framed (L47-seam with the row on), judged, ON or retired.

**The R4 rows (all OFF at NIGHT47's freeze; DECIDE47 D-18)**: each gets its frames tonight and an ON / retire call: finale47, hitzone47 (F),
stampede47, trail47 (B: **stampede47's own check saw the SURVIVAL rounds stop advancing at round 6: find and fix that before anything else
of B's**), wickRideParry47 / wickIntro47 / wickFinale47 (W), kneeHold47 (L).

**The lead's open items**
- **DECIDE47 D-17:** `RowsBaseline.OnPathThrough` 46 -> 47 in `Assets/Scripts/Game/SurvivalSim.Rows.cs`, the composed fingerprint
  re-recorded (pulse47 / overheal47 change the SURVIVAL run), in its own commit at G0, with `fingerprint`, `--off46`, `--off47` checked
  and the diff of the hashes explained in the commit message (docs/NIGHT-ROWS.md rule 6). Then `--off48` EQUAL from the shared commit on.
- **T-88:** the n48 flow table at every build (copy n47-flow-table.sh / .cjs to n48 with the n47 levels as baseline).
- **G44 / G45:** at least 6 h of building by `date`; every new card cites research (Research/2026-10-19-wave48/<builder>.md, Verified /
  Reported per line).

## 2. Builders (four; same split as NIGHT47, the same files; each prompt: "work through your whole list without stopping")

| id | rows | read first |
|---|---|---|
| W | G1, G5 (the lines + the reel's two resolutions), G9 + its three R4 rows | Research/2026-10-18-wave47/W.md, W-cards.md; Tools/SimHarness/Harness.N47.Wick.cs; FeelReel.N47.Wick.cs |
| B | stampede47's round-6 stall FIRST, then G31, G34, G33, the worm's min, G35, G32, G29, G30, trail47, the L2 / L3 flow crosses | B.md, B-cards.md; Harness.N47.Brawl.cs; FeelReel.N47.Brawl.cs / .Wyrm.cs |
| L | G36, G37, kneeHold47, G43's boss flame pair inputs; then help: the n47-spire / n47-span depth from L-cards.md | L.md, L-cards.md; Harness.N47.Loom.cs / Spire.cs; FeelReel.N47.Loom.cs / .Crown.cs |
| F | the lineup reel FIRST (G12-G14), then G20, G24, G43 (TORCH), finale47 / hitzone47, G5's 720p check in a647 | F.md, F-cards.md, docs/COPY47.md; Harness.N47.Feel.cs; FeelReel.N47.Hud.cs / .Fx.cs / .Look.cs |

Builders keep every rule of NIGHT47-BRIEF.md (copy it to NIGHT48-BRIEF.md with n47 -> n48 and the rows of section 5), plus section 0's.

## 3. The lead's clock (by `date`; t0 = GG's go)

| time | the lead | builders |
|---|---|---|
| 0:00-0:30 | G0: read this file, NIGHT47-REPORT, Logs/n47/ASKS.md, DECIDE47, HANDOFF; `df`; the ledger to Logs/n48/ASKS.md (the 18 rows + the R4 rows + the open items, all "building"); **D-17 (the baseline flip + re-record, its own commit)**; the shared commit (section 5, dry run first); NIGHT48-BRIEF.md; spawn W / B / L / F; the first STOP POINT line | (spawned ~0:25) read; no cards needed for rows that have NIGHT47 cards: build at once |
| 0:30-2:00 | integrate + build `n48-r1` at ~1:30 (batch); every builder's reel; look at every frame; the STOP POINT at each build | round 1 |
| 2:00-3:30 | round 2: **the blind pairs** (G20, G24 x guns, G43 x2, G12-G14, G32, G36), each 3 judges; build `n48-r2` | round 2 |
| 3:30-5:00 | round 3: every row still SHORT gets its third approach or its final reason; build `n48-r3` | round 3 |
| 5:00-6:00 | the freeze: `n48-rows-on.cjs` (the rows whose frames + meters are GREEN, the R4 rows the lead judged ON); verbs37 levels 1-5; the smoke; **the batch suite**; the n48 flow table | fixes the lead names |
| 6:00-7:30 | the close: NIGHT48-LOG close table (all 45 rows + the R4 rows, each with its frame path), NIGHT48-REPORT (GG's page), HANDOFF, DECIDE48, THREADS consolidated (close T-83..T-88 or say why not), tag `u3-n48` | |

Cut order (depth only; no row dropped): extra frames beyond the done-when, then the second resolution for non-text rows, then a second
blind-pair judge set. Never cut: the frames each row needs, the blind pairs named in section 1, verbs37, the smoke, the batch suite, the
STOP POINT lines, the close.

## 4. Hard rules (CLAUDE.md, NIGHT47.md section 7, and section 0 above)
- Git: never commit to master, never push / merge, never `gh auth switch`; builders in worktrees, the lead cherry-picks; explicit paths
  + `.meta`; never stage GG's four files; the TMP fallback asset restored before a commit; night prompts and DECIDE48.md untracked.
- Movement exact: golden 0 / 22 exact at every Core commit with the new rows off; fingerprint plain / --off47 / --off48 EQUAL.
- Numbers from config, render numbers in UnityTuning; Mulberry32 only in sim; no audio (event names only, added to PORT-STATUS.md).
- Nothing lost: verbs37 on levels 1-5 after the rows go ON; no gun gets lighter (DECIDE47 D-14); pools only go down.
- A new runtime shader goes in a Resources folder or Always Included, and its Player frames are checked for magenta (Column47's check).
- Docs, logs and ledgers with the Write / Edit tools only.

## 5. The shared commit (the lead's, `Tools/night/n48-shared.cjs` from n47-shared.cjs; dry run first)
- `design.n48` with one row per piece of tonight's new work, all off, each with a _why naming its G row: `climbTop48` (W, G1's top-third
  ledges, level), `wyrmTell48` (B, G31's counter change, sim), `wyrmDive48` (B, G34's dive as the punish, sim), `kilnBoost48` (B, G33's
  island payoffs, sim), `loomHull48` (L, G36 / G37 fixes, view), `blast48` (F, G20's picked variant, view), `guns48` (F, G24, view),
  `foes48` (F, G12-G14 fixes, view), and a `lineup48` reel-only switch; the level rows `foundry48`, `kiln48` only if W / B need geometry
  (n47 -> n48 copies).
- `SurvivalSim.N48.cs` (N48W / N48B / N48L / N48F partials), `FeelReel.N48.cs` (a148-a848, routed from FeelReel.N27.cs),
  `N48SharedTests.cs` (with the Switches() leak fix from the start: clear / restore N38-N43Blocks.ForceOff), N48Campaign after N47Campaign.
- The n47 rows the builders keep improving stay in design.n47 (their numbers may change; their `enabled` does not without the lead).

## 6. The close (all of it)
`NIGHT48-LOG.md` close table: all 45 G rows (state, measured result, **frame path**, row, how to switch it off) and the 8 R4 rows;
`NIGHT48-REPORT.md` for GG in plain words (what changed since n47-r3, what he should look at first, what is still SHORT and why);
HANDOFF.md new top block with the STOP POINT line; DECIDE48.md; THREADS consolidated; tag `u3-n48`. `git status` shows only GG's four
files and untracked prompts.

## Kickoff (GG pastes this into a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`)

```
/effort high
You are the lead of NIGHT48. Read C:\Users\gaige\dev\SKYGRIND-unity\NIGHT48.md whole, then NIGHT47.md's top (GG's words), CLAUDE.md,
HANDOFF.md, Logs/n47/ASKS.md and docs/PRINCIPLES.md, and run the night exactly as written. GG's words outrank everything. I will not
answer during the night: decide every call in DECIDE48.md and keep going. Write a STOP POINT line at every build. Start with `date`.
```
