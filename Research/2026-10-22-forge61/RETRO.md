# RETRO: nights 55-60, honestly (FORGE61 section 2; 2026-10-22)

Evidence: `retro-data.md` in this folder (every line there cites a file and line; short forms `N60-LOG:88` = NIGHT60-LOG.md line 88).
This page draws the conclusions. Numbers are only from the logs; "not measured" where they are not.

## The one-paragraph truth
Six nights, ~29.5 h of lead clock. The lead measured for ~13:53 (47 %), waited ~6:59 (24 %, the usage limit alone 4:40), closed for
3:50 and built game changes for about **25 minutes in total** (retro-data B). Builders did all the building (~42 builder-hours of
windows). Eleven config rows and nine level-data changes shipped; by the first-minute test only **5 rows and 3 data changes are
VISIBLE**. Nobody who plays the game has seen any of it: the last play reaction is 2026-10-20 ~14:05, and every ledger since reads
"Reaction: (none yet)". The critics' top complaints from NIGHT55 (lights float, doors are dark, the style meter reads DULL, the camera
ignores speed, no boss says what it wants, rooms show 1-4 of the enemies the counter promises) were **never touched** in nights 56-60.

## 1. What GG actually got to see (first minute of the level the change touches)
| class | config rows | level data | examples (retro-data A, rows 1-20) |
|---|---|---|---|
| VISIBLE | 5: clear55 (room-clear card), kiln57 (level 3 rebuilt), brief55 (lesson text), colour56 (level colour keys), kill55 (the kill wound) | 3: L2 spawns in view (N56), B13 respawn faces the fight, B15 the blow pipe | kill55: "a near-black, ragged hole ... under the crosshair" (gate-b2.md:23) |
| SUBTLE | 4: wyrm55 (shorter worm fight, bot only), tokens56 (who attacks, bot only), grade55 (THE WICK's grade; a no-op on THE CROWN), rim56 (judged on 2x crops, "a few points over the run noise", gate-b7.md:37) | 4: iceline g7 / g5 re-cut, g8 lava strip, door lamps, L0 arch | lamp60 (NIGHT60, not shipped): the door's brightness moved 2-14 % |
| INVISIBLE | 2: wick55 ("mostly relabels the bare core as a window", gate-r3.md:162), focus60 (visible only on alt-tab) | 2: span p5 pad 1 m, B14 pocket ("Nothing I can see changed", gate-b14.md:57) | |

Hours by class: per-row times exist only for NIGHT60 (N60-LOG, w60:46-51): VISIBLE kill55 ~2:00, B13 / B15 ~0:50; SUBTLE rim56 ~4:10
over three attempts, lamp60 ~3:00 (not shipped); INVISIBLE focus60 ~2:00 (a real bug fix), yellow56 ~1:50 (blocked); bot-only level fixes
B14 ~1:45. **NIGHT60 spent ~11 of its ~16 builder+lead task hours on SUBTLE or INVISIBLE work.** Nights 55-59: not measured per row.

## 2. Repeated mistakes: did a written rule stop them? (retro-data C)
| mistake | rule written | came back? | fix in FORGE61 |
|---|---|---|---|
| disk runs out (reel frames) | L-080 w40, L-104 w57 | yes: w58 (20.7 GB unpruned), w60 (9.7 GB, L-113) | **hook**: guard.cjs refuses builds / reels under 12 GB; `Tools/night/n61-diskwatch.sh` wakes the lead under 15 GB |
| heredoc / node -e edits | CLAUDE.md rule 9, memory "patch-scripts-not-node-e" | yes: w60 L-114 (a builder) | **hook**: heredocs, `node -e` / `python -c` writes, `sed -i`, redirects into docs or code blocked |
| staging GG's files, `git add -A`, `checkout --` | CLAUDE.md rule 1, L-047 | not in w55-w60 (held by care) | **hook**: blocked, with GG's four fonts too |
| push / merge / master / gh auth switch | CLAUDE.md rule 1 | not in w55-w60 | **hook** |
| `unity test`, killing the Editor, deleting worktrees | CLAUDE.md workflow / rule 10 | not in w55-w60 | **hook** |
| the usage limit stops everyone | L-003, L-098 | yes: w55 2:28, w56 1:11, w60 1:01 | a rule, not a hook: commit per slice, the slate + progress file resume, at most 4 builders |
| a target the baseline already meets | L-100 | yes: w60 lamp60 (OFF already 2 / 5) | a slate field: `red` measured before `doneWhen` is final; the critic checks `doneWhen` against `red` |
| a simulated estimate over-reads | L-108 | yes: w59, w60 (3 rim56 estimates high) | rule: no row ON without a Player frame; estimates go in the log only |
| a claim with no frame or number | L-107, L-014 | yes: w57 (tag / 4 rows), w58 (yellow56 note), w59 (spawn note), N56's focus pause (EditMode only) | the slate's `passes` flips only on a critic's PASS with the frame named; the report's "Not checked" section |
| timing a planted action off a late log | L-115 | n/a (w60) | `n60-focus-probe.sh` fixed (228e860b) |
| the Game M suite hangs (MapChoiceTests) | memory "editor-suite-batches" | every night since NIGHT41 | a slice (T-147): read what it waits on; run the other 9 M classes by name |
| focus losses pause tools / smoke | L-015, L-099 | w55 (19 smoke steps), w60 (focus60 vs tours) | a slice (T-146): focus60 excludes automation runs |

## 3. Where the hours went (retro-data B)
Measuring 13:53, waiting 6:59, the close 3:50, setup 2:16, tools 2:13, lead building ~0:25, of ~29:35. Two patterns cost the most:
(a) **one-row critics and re-measuring SUBTLE rows** (rim56 took three attempts and ~4 h for a change judged on 2x crops); (b) **the usage
limit** (4:40 of dead time in three nights). N55's five builders produced 0 ON rows that night (their branches shipped two nights later).

## 4. Said to be bad, never fixed (retro-data D; the ones a player sees first)
1. Doors read as a dark screen (N55; lamps N56 "the spire door is still near-black"; lamp60 blocked N60). GG: doors "it's like dark" (2026-10-13).
2. Lights float (N55; P1-fixtures never started). GG: lights "placed and floating", "look crappy" (2026-10-11).
3. The style meter reads DULL in every frame (N55; rank56 retired without fixing it). GG: the style meter should "feel aweosme to combo" (2026-10-14).
4. The camera reacts to button presses, not to speed (N55; P1-fov never started). GG: movement should feel "more fluid, and less choppy" (2026-10-20).
5. No boss says what it wants (N55; P1-bossText never started). GG: bosses "hard to understand what to do" (w35).
6. Rooms show 1-4 of the enemies the counter promises; level 3 kb3c 3 / 6 and kb3d 2 / 5 at the door (N55 -> N60).
7. Kiln's presses read as "random pillars" (gate-r3b.md:130), GG's own named dislike.
8. Yellow is not only on parry tells (needs a mask: NIGHT60-PROPOSALS P1).
9. Nobody checked a person on the 4.5 m tiers in kb2 / kb2c / kb3; kill55's camera-jitter guard never ran with kill55 firing.
10. Levels 4 and 5 still have NIGHT52's rooms ("3 combat rooms each", w57:10); level 4's split waits on GG.

## 5. Claimed, not proven (retro-data E)
N57's report (tag and 7 rows: the repo had no tag and 4 rows OFF); N58's yellow56 play note (the row was OFF in the build); N59's spawn
play note (no frame existed; the spawn faced a wall); N56's focus pause "3 / 3" (EditMode only; a Player never paused); N56's g8 "makes
the wall ride the faster way" and p5's pad move (no number, no frame); N57's tokens56 / wick55 / grade55 claims (bot or no frame);
N60's B14 "open on foot" (no frame shows it). Pattern: a play note written from the plan or a bot number, not from a Player frame.

## What this means for NIGHT61
- **Build what a player sees, from the critics' list above, before anything else.** Each slice names its frame and passes a blind
  5-second spot test, or it does not turn ON (the visible rule). Bug fixes and dead code are allowed without it, under their own heading.
- **Turn the broken-again rules into hooks** (done in this forge: `.claude/hooks/guard.cjs`, tested 71 / 71 planted + 9 live blocks).
- **Measure less, once, at the end of a slice**; no third attempt at a SUBTLE row; the critic gets the frames, not the builder's story.
- **Builders build; the lead integrates on a clock.** The lead's own building budget is explicit (one slice).
- **The report opens with what is still bad**, from a GG proxy and the critics, whether or not the night touched it.
