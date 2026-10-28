# NIGHT45 forge notes (2026-10-17, the lead)

## How the focus was chosen (J1, J6)
- Facts first: a read-only audit of all 27 fight rooms against DESIGN-RULES (AUDIT.md) and a research agent on what gives an arena
  intention and personality (research/arena-identity.md: 22 sourced findings and the identity card), launched together.
- GG's arena words ("dont just randomly plce stuff it needs intension and personality") meet the audit's finding that no room has a place
  fantasy and every landmark is the same generic pylon pair: the night's center is A-9 (identity before geometry) applied to the three
  arenas GG meets next (b1, g7, k5); levels 4 / 5's templates are named for NIGHT46.
- "cut fluff and old stuff, dont cut anything meaningful": the biggest fluff is process (39-52 files per shared commit) and disk (220 GB of
  old logs); the prompts are history (moved, untracked, not deleted); superseded rows are documented, not deleted (old tapes need them).
- No new verbs or features tonight: GG has not played NIGHT43 / NIGHT44 yet.

## The spike (the riskiest premise, tried before the night)
- A level-only night with no sim bit and no tape bump: the tape records the campaign's level id (RunTape format 8), so a replay loads an
  n45 id directly; the n45 -> n43 base mapping can be unconditional. Tools/night/n45-levels.cjs written and dry-run twice in scratch
  worktrees of u3-n44: dn-check 0 errors in all four assemblies, golden 0 mismatches / 22 exact, fingerprint EQUAL plain and --off45.

## The reviewer's 15 findings and their fates
1. Five places compared the level id directly (level 3's boost lessons would vanish on n45-kiln): FIXED in the script (N45Levels.BaseId at
   each), dry-run again; the brief tells builders to do the same.
2. Builders could not see A-9, the audit or the research (uncommitted): FIXED (this forge commit).
3. FeelReel.N27.cs edited by three builders on one line: FIXED (the script pre-places FeelReel.N45.cs with one partial per builder).
4. Walk / fight gates hard-wired to n43 files; `n44kiln` does not exist: FIXED (each builder's own harness with `--level`; the walk gate is
   "clears the rebuilt room"; T-67 is not A1's gate).
5. P0's batch names: FIXED (the script's own split names).
6. S1 contradicted "no tape bump" and could not fit: FIXED (designed and unit-tested, unwired; NIGHT46's G0 bumps).
7. A3 was two cards; crumble is one event; indices fragile: FIXED (look45 to leftovers; one crumble; indices rewritten).
8. Light triggers and pours are not data: FIXED (the data state change is a wave from a new side; the rest "next night").
9. ROOMS.md collisions: FIXED (cards in a new section; C1 skips b1 / g3 / g7 / k5).
10. The archive impossible from a worktree and against CLAUDE.md rule 1: FIXED (the lead moves them into an untracked `nights/` at the close).
11. The final round could not hold a rebuild: FIXED (builds with rows OFF, frames with rows ON in the copy, rows-on in the repo without
    a rebuild; verbs boot -skygrindMap n45-*; n45-rows-on.cjs written).
12. G0 could wait on GG: FIXED (tell, not ask; a no-answer fallback).
13. arena42's pieces leave b1: logged as DECIDE45 D-6.
14. The /goal's clock: FIXED (`date` on every line and the table).
15. T-62 in five places: FIXED (all five named).
Most likely reversed by GG: the archive (now untracked, per rule 1) and stripping THE RING's dressing (now: world props keep a "world"
reason, V-4).

## Forge experiment
- forge experiment: identity before geometry (A-9) as the night's acceptance (an arena turns ON only with its card committed first and
  its entrance frame showing its own landmark); hypothesis: GG's next reaction names no rebuilt arena as "random"; measure: GG's words on
  b1 / g7 / k5 and the share of each rebuilt arena's solids with a reason (target 100 %).
