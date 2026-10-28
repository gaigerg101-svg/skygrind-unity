---
name: skygrind-idea-lab
description: >
  SKYGRIND's research-and-invention step for a building wave. Research what the best movement shooters and horde games
  do (with sources), generate 30+ feature ideas through five lenses, rank them against SKYGRIND's pillars and GG's taste,
  and write Research/<date>-wave-<n>.md with a slate of 3-5 headline features plus a beyond list. Use it inside
  PROMPT-FORGE, or whenever GG asks for new feature ideas, innovation or "what should we build next".
---

# skygrind-idea-lab

**Why:** GG wants nights that build new things that fit. Ideas made up inside a run overrun, and a "go beyond" mechanic
that GG later reverted cost two nights each (playbook §3.6). This lab does the creative work up front, ties every idea
to a source or a player moment, and ranks it, so the wave prompt can stay short and decided.

## The pillars to fit (from the game itself; re-check them in CLAUDE.md and the reports)
First-person movement shooter at dusk (CoD Zombies meets ULTRAKILL): **speed you can see, crunchy kills, style that
pays, a horde that hunts you on every floor, a round loop with a reason to stay one more round.** The identity is the
dusk palette with its colour roles. The targets are Steam and the Steam Deck, pad first. GG's taste is in
`LEARNINGS.md` → "GG's taste" and `IDEAS.md` → "GG said".

## 1. Research (25 minutes, at most 2 subagents in parallel, web only)
- **Reference games, one mechanic each, with how players received it:** ULTRAKILL, CoD Zombies (BO3 and Cold War), Deep
  Rock Galactic, Risk of Rain 2, Hades, Titanfall 2, DOOM Eternal, Hi-Fi Rush, Neon White, Roboquest, Vampire Survivors,
  Left 4 Dead. Cite reviews, GDC talks or postmortems; mark anything you didn't verify.
- **Open-source implementations we could port** (MIT, CC0 or similar; licences named), and Unity URP techniques for
  the effects.
- **Our own data first:** what already exists (grep the code and `IDEAS.md`), so nothing proposed is already built.
- Never put personal data in a request. Every claim gets its URL.

## 2. Diverge (at least 30 ideas, five lenses, six or more each)
1. **Verbs:** a new thing the player *does* (a movement tech, a weapon interaction, a world interaction).
2. **Steal and twist:** a proven mechanic from the references, bent to fit SKYGRIND.
3. **Systems × systems:** two things we already have that don't yet touch (for example combos × the beacon, style ×
   the shop, routes × the shockwave).
4. **GG's words and the reviewers' complaints, turned into features** (not fixes).
5. **Wild cards:** 2-3 deliberately strange ideas, so the slate isn't only safe ones.

Each idea is one line: **the moment a player feels** (in the player's words), what it builds on, its switch name, and
how it would be measured. Then the rule check: no audio, no new enemy *design* (a proposal page only), no web edits.

## 3. Converge (score, then cut)
Score each idea:
- fit 1-5 (x3)
- wow in the first 10 minutes 1-5 (x3)
- buildable in one 5-6 h wave with at most 3 builders 1-5 (x2)
- measurable on a Player build 1-5 (x2)
- minus risk 0-3 (touches Core or the fingerprint, an asset licence, GG's past reversals)

**The first-minute gate (FORGE61, 2026-10-22):** before scoring, ask of each idea whether a blind SPOT run (a fresh critic shown two
unlabeled before / after frames) would name it within a few seconds. An idea that would read as "nothing obvious" is polish or a bug
fix, never a headline: w55-w60 spent hours on SUBTLE rows (rim56, lamp60) that GG could not have spotted (Research/2026-10-22-forge61/RETRO.md).

Pick the top 3-5 as headlines, each with its own switch. Add 3-6 beyond rows. Write the losers into `IDEAS.md` as
`new`.

**Check the slate as a whole:**
- At least one headline is a new verb, and at least one is pure crunch or feel.
- No two headlines fight over the same files (that makes builder ownership possible).
- One "moonshot" is allowed if its first slice stands alone in 90 minutes.

## 4. Write `Research/<date>-wave-<n>.md`
1. Our data: what exists, what GG said.
2. The outside evidence, with sources.
3. The 30+ ideas table with scores.
4. The slate: theme, headlines, beyond, NOT-this-wave.
5. For each headline: the player moment, the files it touches, its switch, its done-when (a command and the line it
   prints, or a reel scenario and what it must show), and its risks.

Commit it with an explicit path.

## Output
The slate (theme, 3-5 headlines with switch names, beyond rows), in at most 15 lines, for PROMPT-FORGE step 4.
