# DECIDE65: GG's calls for NIGHT65 (THE FLIGHT FOUNDRY, 7 hours), seeded by the forge FORGE65 (2026-10-23)

The run appends one D-id per call it makes for GG (the choice, the alternative rejected, why). GG answers in his own words; nothing here is
built or reverted for him. Untracked, like every DECIDE file.

## Before the night starts (yours, if you can)
- **Disk (needed before t0):** C: had about 14 GB free at the forge; **the night needs 20 GB** (it writes about 5 GB; the guard refuses
  builds, reels and the smoke under 12 GB). Builds/ holds NIGHT63's and NIGHT64's builds (about 380 MB each; n63-b1 .. b13, n63-freeze,
  n64-red, n64-b1 .. b3, n64-rc1 .. rc3 are superseded; keep n64-freeze, n64-abx, n64-aby). Deleting them is your call (T-149): free the space
  yourself, or write "yes, delete the superseded builds listed" here and the night deletes exactly those. Under 20 GB the night runs lean.
- **The room contract re-banded** (FORGE65 D-4): tonight's rooms may be 14-32 m on the long side (your "a lot smaller" and your "wide open
  arenas with a roof" both) with a ride wall of 10 m or more. Keep, or set your own band?

## Open from NIGHT64 (DECIDE64.md; still yours)
- **eye63's blind X / Y** (Builds/n64-abx, n64-aby; NIGHT64-REPORT.md section 2): which feels smoother, X or Y, or no difference?
- **D-1 the TV fringe on thin lines** (the spire exit's red line): keep the fringe, draw the goal column outside it, or judge only its colour?
- The next long nights' order (tonight takes the rooms); the crown's window shafts; bosses in the player's cyan; the magenta dashes.

## After the night (the play card in NIGHT65-REPORT.md section 2)
- **Level 1 new against old:** Builds/n65-freeze with the row on, then with `-skygrindOff65`: better, worse, or the same, and which room?

## The run's calls (D-1 onwards, written by NIGHT65)

### D-1 (0:05) The night runs branch C: no Player while C: is under 16 GB
- **Measured:** `node Tools/night/n65-clock.cjs` at 0:00: "disk: C: 13.2 GB free"; `df -BG /c`: 14G. NIGHT65.md section 1 step 3: A needs 20 GB,
  A-lean 16-20 GB, **C under 16 GB: no Player; rooms built and judged sim-side.** The disk was not freed before t0 (this page's first call).
- **Rejected:** deleting the superseded builds or the old agent worktrees myself. Both are yours (T-149; CLAUDE.md rule 10: only my own
  temporary output); `git worktree list` holds **232 worktrees** (`.claude/worktrees/agent-*`, samples 172, 191 and 623 MB each, so likely
  tens of GB), the biggest item after Builds/ (about 79 GB). Also rejected: running the Player path anyway at 13.2 GB (three builder worktrees
  take about 1-2 GB, the guard refuses every build and reel set under 12 GB, and a set writes frames before its prune).
- **Your call:** free space (the superseded builds this page lists, or the old worktrees) to 16 GB or more and the night switches to the
  Player path at the next round (re-measured at every round; logged). Or write "yes, delete the superseded builds listed" above and the night
  deletes exactly those.

### D-2 (0:06) What branch C changes, and what it keeps
- **Kept:** the six rooms, the cards and the card gate, three rounds per room, the reviewers per round, the GATE critics, slots65.json by a
  PASS, the report, the close's order.
- **Changed:** builder T's second job is **iso65** (a sim-side picture of a room: an isometric view from the entry and from the far corner and
  an elevation section, coloured by role), in place of T2 tour65 (tour65 is built third; its Player proof is "not checked"). Reviews, the SPOT
  and the GATE critic judge the plan PNG, the iso65 pictures and the sim files instead of Player frames. Every room's Player lines (tour65
  frames, verbs37 n65-foundry 27 / 27, the SPOT on the tour65 pose) read "not checked (branch C)"; the doneWhen lines are not loosened, so the
  critic decides what that means for each room.
- **The close keeps its Player steps if the guard allows them at 6:00:** this night's own builder worktrees are removed once their work is
  picked (my own temporary output; their branches kept), which should bring C: back to about 13 GB for the freeze build, its six-level
  verbs37 set, the smoke and Builds/n65-play-latest. If the guard refuses, each step is logged under Blocked.

### D-3 (0:47) a5 forces the slide into the launch, so two rooms in a row do not force the same verb
- **The card gate's note (Logs/n65/cardgate.md, non-blocking, "your call"):** a5 THE POUR and b1 THE KNOCKOUT YARD both force the launch,
  back to back (1.5 then 1.6), and no card forces the slide, the slam or the dash jump. Your 2026-10-13 brief: "one might encourage the dash
  jump another 1 encourage wall riding".
- **Chosen:** a5 keeps its pour (the launch into the channel), but the way to the pour's lip passes under a low beam only a slide clears, so
  its launch is the slide-punch (VERBS rows 6 and 41, both level 1). b1 keeps the gates' launch (level 1's exam).
- **Rejected:** a5 as a grapple room (the grapple is owned from level 4 only, grapple35, VERBS row 17); a slam room (the player's slam has no
  damage in the config, only design.stamina.rewards.slam and slamRefill, so a room cannot make it the answer); a dash-jump room (the
  dash-jump's gate meter belongs to n38-spire's flight rooms, VERBS row 8). Also rejected: re-verbing b1 (its geometry is the one card the
  gate found sound).
- **Revised at 1:12 (the re-gate, Logs/n65/cardgate-R2.md):** a slide-only way to the lip cannot be honest (closing THE TAP WALL's ends
  leaves the apron a pocket with no walk node, ai.grid.minClear 1.9 > the slot's 1.2 m), and launch36 fires on a dashing punch too. Ruled:
  the slide slot is the short, direct line to the pour's lip and the walk round is the long way (wave 2's husks moved east so the west end is
  at least 6 m from them). The slide is offered and rewarded, not forced: the report says so.

### D-4 (1:15) The card gate gets three passes; what is still open goes into round 1 as its BLOCKING list
- **Measured:** a8 and a4 failed three card passes (Logs/n65/cardgate.md, cardgate-S2.md, cardgate-S3.md). Every pass found real things,
  but by the third the open items are numbers the room checker measures on built geometry (a8: I2's ride ratio 1.28 against 1.30, from a
  1.91 m grid jump onto a ramp's side; a4: a ramp top inside the spitters' minRange 6 m (Spitter.cs:175) that sees every node). Every card
  fits its rect (the gate's hard rule, L-112), names its verb and its pieces' jobs.
- **Chosen:** a card that fails its third pass enters round 1 with that pass's BLOCKING items as round 1's own BLOCKING list, checked on
  the geometry by `n59-l-room.py` (I2, R5, IN VIEW) and by the round's reviewers; the GATE critic reads the three card-gate files as
  evidence. a2 and b1 passed (cardgate-R2.md, cardgate-S2.md).
- **Rejected:** a fourth and fifth card pass (about 10 minutes each, the rounds slipping past 1:45 with every room still on paper: the
  corner "checked more than it built", history.md section 2); dropping the card gate (it caught six real flaws, among them a3's dash-jump
  skip and a4's peek post).

### D-5 (1:19) a3's ride is offered and rewarded, not forced: the dash-jump crosses any gap the room can hold
- **Measured (Logs/n65/cardgate-R3.md, the third pass):** a dash-jump keeps the dash's 24 m/s (Mover.cs:1080-1088) and a ceiling removes
  only the velocity into it (Mover.cs ClipInto, :1903-1913), so a beam over the gap cannot stop it: under THE CRANE BEAM a double jump puts
  the feet at 3.56 m at the landing (2.5 m). A dash-jump returns to take-off height after about 25 m (37 m with a double jump); a3's slot is
  26 m long. No flat gap in this room forces the ride, short of changing the movement (your "don't change the core mechanics").
- **Chosen:** a3's job is your 2026-10-14 complaint ("awful and relaly difficult and not fun"): the ride made readable, fair and the better
  line (the rideKill53 bar, the landing's height and cover, the wave placed for the rider), with the dash-jump honestly the skilled
  alternative. Its MAIN: the ride pilot GREEN on a3's own wall (the red's GREEN rode a2's wall), DULL under the red. The report says "the
  ride is not forced (the dash-jump crosses)". a3 enters round 1 with cardgate-R3's items as its list (D-4).
- **Rejected:** a longer gap (the slot is 26 m); a ceiling or beam (ClipInto); changing the dash-jump (core mechanics, yours).
- **Your call:** should a level-1 room be able to force the ride? Only a movement change can (the dash-jump's carry, or a ride that
  gains height).

### D-6 (2:27) The disk keeps falling: this night's worktrees go as soon as their last pick lands
- **Measured (`node Tools/night/n65-clock.cjs` / Get-PSDrive):** C: 13.2 GB at t0, 11.2 at 0:13 (the three builder worktrees, about 685 MB
  each), 10.4 at 1:45, 9.9 at 2:10, 9.17 GB at 16:24 (2:26). About 2.5 GB of the 4 GB is accounted for (the worktrees, the Editor's
  Library about 0.3 GB, session transcripts under .claude/projects 0.12 GB); the rest is outside this night's files (searched: %TEMP%,
  Edge profiles, the pagefile (34.5 GB, last written 03:25), git objects, npm / pip / nuget caches, .codex). At this rate the close's build
  (refused under 10 GB) and reel sets (under 12 GB) may be refused.
- **Chosen:** builder T's worktree is removed right after its last pick (my own temporary output; its branch n65-T kept); R's and S's after
  their last picks (round 3, or a GATE fix round by 5:00). If the guard still refuses at the close, each Player step is logged under
  Blocked and the play card points at Builds/n64-freeze plus the level files.
- **Rejected:** deleting older nights' builds or worktrees (yours, T-149); deleting other sessions' files.
- **Your call, now if you see this:** freeing 3 GB or more (Builds/ holds about 79 GB of older nights' builds; .claude/worktrees about 232
  older worktrees) lets the close build and reel GG's play builds.
- **2:40: the guard refused** `git worktree remove` of builder T's worktree ("deleting .claude/worktrees is GG's call"). So this night's
  worktrees stay too; the mitigation above is off.

### D-7 (2:41) GG's play builds without a new build: n64-freeze's code with NIGHT65's data
- **Why:** at about 9 GB free the close's `n41-build.sh` (refused under 10 GB) and every reel set (under 12 GB) are likely refused, and GG
  must still be able to play the rooms (NIGHT65.md section 4 step 6: "so GG's hands judge the night's work even if no room passes").
- **Chosen:** Builds/n65-play-latest (and Builds/n65-play-passed if any room PASSes) as HARD-LINK copies of Builds/n64-freeze
  (`Tools/night/n65-play-build.sh`: about 0 bytes each), with their runtime config replaced by HEAD's (design.n65.foundry65 turned ON in the
  copy, the maps.list row) and Design/levels/n65-foundry.json added (the latest assembly, or slots65.json's). The level row and the
  maps.list row are data that NightRows.SwapLevels reads generically, so n64's code boots n65-foundry. **What n64's code lacks:** Look52's
  tier list is code (UnityTuning.U52.cs), so look52's stone tiers do not apply on n65-foundry in these copies (n52-foundry keeps them):
  the play card says so. Every NIGHT65 tool code (tour65, the bot opt-out) is dev-only and not needed to play.
- **Rejected:** no play build at all (GG's hands are the night's real judge); a byte copy (380 MB each).
- **3:12 answered by the integrated-diff review** (Logs/n65/review-n65.md): u3-n64's code boots n65-foundry from HEAD's config and level
  (read, not run).

### D-8 (3:56) Every room FAILed its GATE; one fix round for GG's play build, not for a slot
- **Measured:** Logs/n65/verdict-<id>.md, all six FAIL, SIM-SIDE FAIL too: a2, a3, b1, a4 miss their own cards' MAIN aims on the walk
  bot's DULL share (set at 0:50, before any measurement; the walk bot never uses the verbs); a2 / a4 / b1 break a GUARD; a8's verb is
  skippable; a5's SPOT reads the round as the emptier room and round 3 added no piece. Both critics ruled the branch-C question the same way:
  a Player line NOT CHECKED caps a room at UNSURE, so no room can PASS tonight without a Player. So slots65.json stays all old and the
  level row stays off.
- **Chosen:** one fix round (R and S, by 4:50) on the named, one-round causes only (a5 THE TAP BEAM / the slot's lane and the slide row's
  output; a2 THE WEST RIDE's two lights; a4 THE BELLOWS PAD's apex, the deaths; b1 THE NORTH SILL), and builder T fixes the skip test's
  flaw the S critic found (a "refused" run that never refused a press). Then a round-2 critic per set for the SIM-SIDE verdict (at best
  UNSURE). The rounds' rooms go to you in Builds/n65-play-latest as unreviewed rooms; your hands decide.
- **Rejected:** loosening a MAIN to the verb-aware pilot's numbers after the fact (the cards' aims were fixed first, L-103; the S critic
  refused it too); more rounds past 5:15 (the plan's stop).
- **Lesson (for LESSONS.md):** a room card's MAIN must be measured by an instrument that performs the verb. The walk bot's STYLE share
  cannot see a verb it never uses.

### D-9 (4:23) The builders' last 50 minutes: a density round for your play build
- **Why:** your words, "big empty boxes" (gg-proxy's #1 three nights running) and "don't just randomly plce stuff it needs intension and
  personality" (2026-10-17); tonight's blind spotter called the rebuilt a5 "mostly empty, with one ramp and a single pad arc" (twice) and
  a2's old room empty; a2 has 17 pieces, a4 14 (Logs/n65/gate/d9.txt).
- **Chosen:** R and S add dressing and cover with a job and a role colour each (Kit33Props / PropsView29 kinds; landmarks, route cues,
  light fixtures, cover that faces the floor and not the rider), never moving the verb's own pieces; the guards held (room check, n46flow
  clears 10 / 10, deaths and stall within the red's band, the verb pilots still GREEN); report by 5:05. The rooms go into
  Builds/n65-play-latest (unreviewed, as labelled); no GATE (no time; the GATE needs a Player anyway).
- **Rejected:** stopping the builders at 4:22 (50 minutes of the night's building share unused); another GATE (it caps at UNSURE).
