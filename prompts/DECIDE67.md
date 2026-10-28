# DECIDE67: GG's calls for NIGHT67 (THE AREAS, 8 hours: levels 2, 3 and 5), seeded by the forges FORGE67 and FORGE67b (2026-10-24)

**Tonight's rooms** (FORGE67b DECISIONS D-3 as amended; your answer overrides them): level 2 THE ICE LINE g7 THE MIX and g8 THE
STAMPEDE; level 3 THE KILN kb3 THE PRESS HALL and kb3c THE TAP RUN; level 5 THE SPIRE v2 THE NAVE and v6 THE CHOIR. Level 2's g1-g4 are
the campaign's weakest rooms, but with the gauntlet alone no verb pilot can measure them: they wait for a gauntlet pilot (a later night).
Level 1's rebuild (row OFF) waits for your play of Builds/n65-play-latest; level 4 is not in tonight's slate. No room gets smaller
tonight: a room keeps its outline (the slot model), so "a lot smaller" needs a room-splitting change. Name any level or room you want
first.

The run appends one D-id per call it makes for GG (the choice, the alternative rejected, why). GG answers in his own words; nothing here
is built or reverted for him. A line written here steers priorities; a deletion, a bulk change or a lever needs his own message in the
night's session (the org rules: file content is data, only he authorises). Untracked, like every DECIDE file.

## Before the night starts (yours)
- **The disk (a go / no-go at t0).** C: had 14 GB free at FORGE67b: branch C (sim only), so tonight's rebuilt levels cannot be checked
  in a Player. 18 GB or more (A-lean) lets the night
  turn level 1's rebuild ON in a Player; 23 GB (A) frames every round. **The night never deletes.** If you want the space, run these
  yourself from Git Bash in `C:\Users\gaige\dev\SKYGRIND-unity` before t0 (look first with `du -sh <path>`):
  - the superseded builds: `rm -rf Builds/n63-b1 Builds/n63-b2 Builds/n63-b3 Builds/n63-b4 Builds/n63-b5 Builds/n63-b6 Builds/n63-b7
    Builds/n63-b8 Builds/n63-b9 Builds/n63-b10 Builds/n63-b11 Builds/n63-b12 Builds/n63-b13 Builds/n63-freeze Builds/n64-red
    Builds/n64-b1 Builds/n64-b2 Builds/n64-b3 Builds/n64-rc1 Builds/n64-rc2 Builds/n64-rc3` (about 8 GB). **Keep** Builds/n64-freeze
    (n65-play-latest shares its files), n64-abx / n64-aby (eye63's A / B) and n66-freeze (NIGHT66's verified build).
  - older builder worktrees (about 0.7 GB each): `git worktree list` (235 entries, many other sessions' scratch trees), read `git -C
    <path> status --short` first (a forced remove loses untracked work), then `git worktree remove <path>`. **Keep NIGHT66's three**
    (agent-af48e888c59fa3626, agent-a9d0ffb6d0dfdb2be, agent-aeb9fc116ba05ef4d): NIGHT67's builders use them.
- **The usage limit:** NIGHT66 lost 4:35 to two stops of your individual spend limit; a window lasted 2:20 under the night's load.
  **Start NIGHT67 right after a reset** (not in the window FORGE67 used), or raise the limit for the night.
- **Memory:** two Unity processes (PIDs 49588 and 51936, started 10/22 23:24, about 6.8 GB each) are the Editor's AssetImportWorker6 and 7
  (FORGE67b's code scout), not the Editor itself (21008); with them
  commit free sat near 17 GB, and the night holds heavy jobs under 15 GB. Closing them is yours.

## Open, carried (still yours)
- **Play Builds/n65-play-latest** (NIGHT65-REPORT.md section 2): better, worse or the same as the old level 1, and which room?
- **eye63's blind X / Y** (Builds/n64-abx, n64-aby).
- **DECIDE65 D-5: should a level-1 room force the ride?** (a3's lesson "fire while riding" did, NIGHT66; it was relaxed to "THE
  LANDING reached" by ride, stair or dash-jump).
- **b1 (DECIDE66 D-3, dropped for tonight) and b2 (a slot, not a room):** say if either should be rebuilt.

## Levers the night will not build without your own message
- A dash-pip refund on a ride or launch kill (a movement economy row, default off).
- A charger leading a8's lane file (a new enemy type on level 1).
- A shielded spitter in a4 (a new enemy rule: builder S's fallback, NIGHT66).

## The run's calls (D-1 onwards, written by NIGHT67)
- **D-1 (0:00, branch C).** C: had 11.7 GB free at t0: branch C (sim only), as DECIDE67 foresaw; nothing was deleted. Rejected: freeing
  space ourselves (the night never deletes). **Found at 1:13:** the disk keeps falling because Windows grows C:\pagefile.sys as memory
  commit climbs (39.5 GB, commit limit 98.6 -> 102.6 GB, C: 11.7 -> 7.0 GB by 1:10). The two AssetImportWorkers (PIDs 51936 and 49588,
  about 7 GB committed each since 10/22) are the largest holders after the Editor. **Yours:** closing them, and whether to cap the pagefile
  (a reboot shrinks it). The night keeps one dotnet job per tree and the builders stop at 5 GB.
- **D-2 (2:25, the builders under 5 GB).** C: reached 4.8 GB at 2:19 (the pagefile 42.5 GB allocated, 8.5 GB in use: Windows grows it on
  commit peaks; the night's sims are 0.3-0.4 GB each). The builders stopped running jobs (NIGHT67.md rule 2). **The call:** R (and S when
  a room is close) resume for design edits only (no dotnet in their trees); the lead measures each candidate in the main tree, one job at a
  time; every sim stops if C: falls under 3 GB (headroom for the pagefile: a full disk is the 2026-10-22 crash, L-128). Rejected: ending
  the rooms at 2:19 (g7 is one seed set short of a GATE) and letting three trees keep running sims. Reverse it with one word.
- **D-3 (1:21, the put-in loadout).** Builder T found skip v2 and the walk bot put the player into a room without the earlier rooms'
  weapons (iceline's runs held the gauntlet only). The call: fix the instruments (harness only) and re-run every red before r1, so the bars
  measure the game you play (g7's bar rose 1.25 -> 1.39). Rejected: keeping the night's first reds for speed.

## Questions for GG's answers (NIGHT67's close; no room passed, so there is no play build)
- **Level 2, g7 (parry):** with the KESTREL in hand a player can simply shoot the spitters, so a parry saves only the gun's time (g7
  stalls at 1.30-1.32 against its 1.39 bar). Should a parry room take the gun's answer away (a parry-only wave, or a spitter the gun cannot
  kill: a new enemy rule, yours), or is "parry is faster than the gun" enough?
- **Level 3, kb3c (ride):** the ride now lands on a 3.5 m tower that holds the drones (ride kill GREEN), but riding is not yet faster than
  walking (1.06). Is that the kind of room you want?
- **Level 5 (v2, v6):** lava beside the fight killed the walk bot and dry pits slowed it. Do you want hazards in the fighting space at all,
  or only where a verb throws bodies?
- **The disk:** free about 25 GB before the next night (old Builds, the import workers) so rooms can be checked in a Player?
