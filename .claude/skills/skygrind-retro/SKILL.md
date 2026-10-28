---
name: skygrind-retro
description: >
  SKYGRIND's memory-retention step. After a night (or at the start of PROMPT-FORGE), distill what the night measured and
  what GG said into LEARNINGS.md (process lessons with evidence, deduplicated and pruned) and IDEAS.md (the feature
  backlog with scores and GG's reactions), and fill the playbook's outcome row. Use it at every night's close, before
  writing any new night prompt, or when GG asks what we've learned.
---

# skygrind-retro

> **Defers to `wave-memory` (2026-10-07).** SKYGRIND's long-term memory lives in `docs/memory/` (CORE, LESSONS, OWNER,
> FEATURES, THREADS, ledger/, IDEAS), and CLAUDE.md imports its CORE. Run `wave-memory harvest <n>` after a night and
> `wave-memory react` when GG reacts. In this file, "LEARNINGS.md" means `docs/memory/LESSONS.md` and "IDEAS.md" means
> `docs/memory/IDEAS.md`. What this skill adds on top: the playbook outcome row, and the "seen k times -> propose a
> standing rule" step below.

**Why:** the auto-memory holds short facts, and HANDOFF holds state. Neither holds *why a night went the way it did*
in a form the next prompt writer reads before writing. NIGHT19 and NIGHT20 lost hours to the same two causes: the spend
limit with too many agents, and heavy jobs run side by side. This skill keeps that kind of lesson in one place, with its
evidence, so it's learned once.

## Inputs (read; don't re-derive)
- The night's `NIGHTn-LOG.md` (rounds, the close table, slips), `NIGHTn-REPORT.md`, the reviewers' decoded picks and
  their words, `HANDOFF.md`'s top block.
- GG's words after the night: his message, any `GG-NOTES*.md`, and his reactions to earlier features. Keep them
  verbatim; they're the strongest signal of what he keeps.
- The current `LEARNINGS.md` and `IDEAS.md` (create them if missing, with the headings below).

## LEARNINGS.md (tracked; at most ~150 lines; one lesson per bullet)
Sections: **Process** (how a night runs), **Measurement** (judges, reels, benches, reviewers), **Building** (code,
Unity, git), **GG's taste** (what he kept, what he reverted, in his words).

Each bullet follows the same form: `- <the lesson as an instruction>. Evidence: <night, the measured line>. (<date>; seen
<k> times)`.

How to update:
1. For each thing that went notably well or badly this night, find the bullet that already covers it. If one does, add
   the evidence and bump `seen k`. Write a new bullet only when none does.
2. A bullet seen 3+ times is a standing rule. Propose it for `CLAUDE.md` or the playbook's checklist in the reply. Don't
   edit `CLAUDE.md` yourself; that's GG's file.
3. Prune: merge near-duplicates; delete a bullet that a later night disproved, and note the deletion in the commit
   message.

## IDEAS.md (tracked; the feature backlog)
One table: `| id | idea (the player moment, one line) | source | fit 1-5 | wow 1-5 | build h | measurable by | status |
GG said |`.
- **Status:** `new`, `slated` (in a wave prompt), `built` (with its switch name), `kept` (GG kept it on), `dropped` (with
  GG's words).
- **Sources:** the additions backlog, the reviewers' words, GG's notes, the idea lab's research.
- Don't delete a dropped idea; its row stops the next forge from proposing it again.

## Visibility and hooks (FORGE61, 2026-10-22)
- **Classify every shipped change** VISIBLE (a player notices it in the first minute of the level it touches; a blind SPOT run names it
  from two unlabeled frames), SUBTLE (only a side-by-side shows it) or INVISIBLE (only a metric moved), and count the night's hours per
  class from the log. A night whose hours went mostly to SUBTLE or INVISIBLE work says so in its first line. Why: w55-w60 shipped 11
  rows, 5 of them VISIBLE, while the critics' top complaints stayed open (Research/2026-10-22-forge61/RETRO.md).
- **A rule broken again after it was written becomes a hook**, not another sentence: a rule in `.claude/hooks/guard.cjs` with a planted
  case in its `--selftest` (L-034), committed with the lesson. Why: L-104 (disk) and rule 9 (heredocs, L-114) came back after their rows.

## Also
- Fill the playbook's outcome. That's the library entry's Outcome plus one `OUTCOMES.md` row: time against the box, done
  / short counts, caps, GG's reaction, the lesson. Add to `PLAYBOOK.md` §2-§3 when this night taught something general.
- **Prune the auto-memory:** if a memory file repeats a LEARNINGS bullet, keep the memory as a one-line pointer.
- Commit `LEARNINGS.md` and `IDEAS.md` with explicit paths (CLAUDE.md rule 1).

## Output
A reply of at most 10 lines:
- the 3 lessons that most change the next prompt;
- the ideas whose status changed;
- any proposed standing rules.
