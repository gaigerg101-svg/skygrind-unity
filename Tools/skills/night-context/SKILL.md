---
name: night-context
description: >
  Prints a one-page session-start brief for a game built in nightly autonomous waves (GG's latest words, open threads aged
  two nights or more, the last close table's SHORT rows, HANDOFF's open items, the lessons that match a card) and, at a
  night's close, checks that every SHORT and open item landed in docs/memory/THREADS.md and HANDOFF.md, lists open items a
  HANDOFF rewrite dropped, and flags an unharvested memory. Use at the start of any lead, builder or forge session ("brief
  me", "what did we forget", "what's open"), before writing a night prompt, and before declaring a night closed.
argument-hint: "brief [--card <id>] | close [--night <n>] [--handoff-ref <tag>]"
allowed-tools: Bash(node:*), Read, Grep
---

# night-context

A script, not a reading list: it reads the project's own files and prints what a session must not forget. It never writes.
It sits beside **wave-memory** (which writes docs/memory/) and **wave-forge** (which writes the next prompt); this skill
only reads their output and says what is missing.

Run from the project root (or pass `--root <dir>`; a worktree builder passes the main checkout, where the untracked
NIGHTnn.md lives):

```bash
node ~/.claude/skills/night-context/scripts/night-context.cjs brief --card <card id>   # session start
node ~/.claude/skills/night-context/scripts/night-context.cjs close                    # before the close is declared
```

## brief (every session start; builders pass their card)
Prints, in this order, about one page:
1. `!!` when a closed night has no ledger and no THREADS paragraph (the memory is stale; run `wave-memory harvest <n>` first).
2. GG's latest words: the newest `Research/*-waveNN/GG-REQUEST.md` (verbatim, cut), its G-rows, the last three OWNER.md rows.
3. Open threads aged 2+ nights from THREADS.md (age = this night - first seen; computed, not the table's stale age column),
   the 10+ night ones folded into one id line, untracked "also open" items, and how many tonight's NIGHTnn.md / DECIDEnn.md
   never name (wave-memory's rule: aged 2+ goes in the next P0 or is parked by GG).
4. The newest close table's SHORT rows and SHORT fates (deduplicated), then HANDOFF.md's open items.
5. The lessons whose words match the card (`--card <id>` reads the card from NIGHTnn.md; `--text "..."` for anything else).

Act on it: put each aged thread and SHORT row that touches your card into your notes before you build; read every
matched lesson's full row in LESSONS.md.

## close (the lead, before the close table is declared final)
Exit 1 and a `!!` line for each miss:
- every SHORT row / fate of the newest close table appears in THREADS.md AND in HANDOFF.md (matched by row names like
  `aim41`, test classes, `THE WICK`, file names; weak keys like `A11` or `suite` need a second shared word);
- every HANDOFF open item ("- Open:" or "## Not done") appears in THREADS.md;
- no open item of the committed HANDOFF (`--handoff-ref`, default HEAD; use the last freeze tag) vanished from both the
  working HANDOFF and THREADS (this caught THE WICK: dropped by the 2026-10-14 forge's HANDOFF rewrite);
- every G-row of the night's GG-REQUEST is named in the log;
- `docs/memory/ledger/wave-<n>.md` exists and CORE.md's state line names the night.

Loop: run close, fix THREADS.md / HANDOFF.md / run `wave-memory harvest`, run close again until `RESULT: every SHORT and
open item landed` or each remaining `!!` is written into the close table as SHORT with its reason.

## Options
`--root DIR` project root · `--night N` (default: the newest NIGHTnn / waveNN found) · `--prompt FILE` the night prompt ·
`--log FILE` a close log · `--aged 2` · `--ancient 10` · `--lessons 6` · `--width 160` · `--handoff-ref REF`.

## Wiring it in (optional, the owner's call)
A SessionStart hook can print the brief into every session: Claude Code adds a SessionStart hook's plain stdout to the
context (https://code.claude.com/docs/en/hooks). Command: `node ~/.claude/skills/night-context/scripts/night-context.cjs brief`.

## Files it reads
docs/memory/{THREADS,LESSONS,OWNER,CORE}.md, docs/memory/ledger/, Research/*/GG-REQUEST.md, NIGHTnn.md, NIGHTnn-LOG.md
("## Close table" + the fates line), DECIDEnn.md, HANDOFF.md (working copy and `git show <ref>:HANDOFF.md`).
