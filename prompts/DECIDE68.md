# DECIDE68: the calls for NIGHT68 (THE BETTER LINE)

GG, 2026-10-24: "you make the calls". FORGE68 made each below, with its reason; the night acts on them. Any line can be reversed with
one word from GG (write it under the line); the night reads this page at its bearings.

## Before the night
- **Q0 the disk: no deletion.** C: has about 13 GB free, so the night runs sim-only (branch C) unless space is freed before t0. Neither
  the forge nor the night deletes anything (CLAUDE.md rule 10, the org rules). If GG wants Player checks tonight: delete old Builds/
  folders he no longer needs and close the two Unity AssetImportWorker processes (about 7 GB of memory each, which the pagefile follows),
  to reach about 25 GB.
- **Q5 the prompt method: E1 YES; E2 NO; E3 NO.** E1 is built: .claude/hooks/night-resume.cjs, a SessionStart hook (compact|resume)
  that prints Logs/<night>/STATE.md and NIGHT68.md's sections 0 and 10 after each compaction or resume while a night clock runs, and
  nothing otherwise (dry-run 2026-10-24: a running planted clock prints the card; a stopped or stale clock prints 0 bytes; the guard
  self-test 110 / 110). Reason: the lead's session compacted 4 times over NIGHT66-67, and an idle session compacts after about 54
  minutes (prompt-methods.md). E2 (a Stop hook) is rejected: it could block the exact final message the /goal needs. E3 (updating
  Claude Code) is rejected for the night of a run.

## The game
- **Q1 g7: YES, melee only (option a).** The game's own room rule `"limit": ["meleeOnly"]` (Campaign24.cs:35, 299; shipped levels 2,
  07 and 10 use it; gen-slots.cjs:110 accepts it) in g7's room file, behind the OFF row, with a `limitReason` the player reads. With
  the KESTREL out of reach in that room, the returned glob is the ranged answer. No enemy code changes. It runs behind a probe (the
  slate's g7). Rejected: (b) a homing glob (new enemy code: GG's design area, 2 h); (c) leaving g7 (NIGHT67 showed geometry cannot lift it).
- **Q2 a ride speed boost: NO tonight.** It is a Mover.cs change: every golden vector and the 16 trajectory replays move (CLAUDE.md rule
  3), and every ride in the game changes. Rooms first; revisit when GG has played the night's build.
- **Q3 a pit rule (enemies die, the player is returned with capped damage): NO tonight.** A new game rule the bots cannot judge fairly.
  Pits go only on the launch's throw line, lit at the rim.
- **Q4 the play card: YES.** A labelled play build of the best room per level, PASS or NOT PASSED, every night (PC).

## The run's calls (NIGHT68, written during the night; NIGHT68-LOG.md has each)
- **D-1 (1:00) g8 stays a ride room**, against R's card (launch): the card critic showed launch never beat the refusal on any g8 file
  (0.98 / 0.97 / 0.76) and the 0.84 ride reading was the pilot I1 fixed.
- **D-2 (4:00) after the usage stop (~1:13-4:00)** the night resumed by itself, as you asked; r1 had run on NIGHT67's old room files as a
  baseline (the builders' files were not picked before the stop).
- **D-3 (4:20) kb2 keeps its GALLERY label** although its P11 lint rule then cannot pass in that slot; relabelling a room to switch a
  rule off was rejected. kb3c stayed ride (no verb change without a new red).
- **D-4 (4:24) the STYLE rule stands** (a builder asked to change it: it asks 91-94 % where the verb runs reach 79-82 % on levels 3 and 5).
  A rule is never loosened after a measurement; it is a finding for the next forge.
- **D-5 (5:30) every red re-made on the final harness** before r3, because the critic showed the launch reds predated the launch fix.
- **D-6 (5:38) g7 cannot pass tonight**: its leak fix refuses more than the parry (the critic), and a melee-only room's stall can't meet
  a ceiling set from the gun room.

## Questions for GG from tonight (each needs your word; nothing happens without it)
- **Q6 kb3d's wave 3:** 740 of its 1060 hp is a rooted burrower or a charger the punch throws at 0.3, so no geometry can make the launch
  the faster way. May its wave change (enemy design is yours)?
- **Q7 melee-only rooms (g7):** keep the melee-only parry room idea? If yes, its guards need their own baseline (a melee-only room's walk
  bot stands next to the enemy to swing), which is a rule change for the next forge.
- **Q8 the STYLE rule:** the forge set "verb STYLE >= min(refusal + 15, max(refusal, 95))"; on levels 3 and 5 it asks for 91-94 %. Lower
  it for the next night (a rule change before any measurement)?

## Still open from before (GG's)
- Five minutes on the night's play build; Builds/n65-play-latest (level 1's rebuilt rooms); eye63's blind X / Y.
