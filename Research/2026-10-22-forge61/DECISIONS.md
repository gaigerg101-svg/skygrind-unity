# DECISIONS (FORGE61, 2026-10-22, the forge's calls; GG was not asked: FORGE61.md "decide and write each call")

## D-1 NIGHT61 is 6 h minimum, 7 h hard cap
FORGE61.md's numbers ("GG: change these two numbers here"). The close starts no earlier than 6:00 and must finish by 7:00; the goal has a
last escape at 8:00 with the report committed, so it can always be met. Rejected: a close allowed before 6:00 when the slate runs dry
(the dead-code pass and the beyond list always have work).

## D-2 The slate is the critics' never-fixed list, ranked by what a player sees
S1 exit doors, S2 level 3's fight in view, S3 boss rule lines, S4 light fixtures, S5 the style meter, S6 speed FOV, S7 kiln's presses;
bugs B1 (focus60 vs tours), B2 (Game M); D1 dead code. Each comes from RETRO.md section 4 (a critic complaint untouched since NIGHT55)
crossed with GG's repeated asks (retro-data.md F: lights and dark doors 4x, arenas with intent 8x, bosses 5x). Rejected: a fresh
idea-lab slate of new features: GG's ask is "real results"; six nights shipped 11 rows of which 6 a player can barely see, while these
complaints sat open. New ideas still enter through gg-proxy's start ranking and the beyond list.

## D-3 The visible rule is a blind SPOT run, not the gate critic's opinion
A fresh `skygrind-critic` in mode SPOT sees only two unlabeled frames (A / B by coin flip, a random folder name) and names the most
obvious difference; it must name the slice's `visible` change. Rejected: the GATE critic judging visibility (it knows what changed, so
it is not blind); a pixel-difference threshold (lamp60 changed pixels and still read as nothing).

## D-4 The guard: one Node script, exec form, `onFailure: block`; its own bugs fail open and are logged
Process-level failure (node missing, a timeout, a crash) blocks the call; an exception inside the rules allows it and writes
`Logs/guard-errors.log`. Rejected: fail-open everywhere (a guard that silently disappears is the "rule as a sentence" problem again);
fail-closed on its own bugs (one odd command shape could block a whole night). Self-test: 74 / 74 planted; live in this session: 9
planted commands blocked (git push in Bash and PowerShell, git add -A, a GG file, a heredoc, node -e writing, unity test, rm under
.claude/worktrees, a reel under 12 GB).

## D-5 The guard covers more than FORGE61's list, where CLAUDE.md already forbids it
Also blocked: merge / pull / rebase / reset --hard / checkout or restore of "." or GG's files / clean -f / stash / branch -D / tag -d or
-f / commit --amend / filter-branch, killing Unity.exe, `rm` of a whole project folder, `dotnet format` without `--verify-no-changes`,
git writes in the web repo, Write / Edit on GG's files or the web repo, `sed -i`, `perl -i`, `python -c` writes, redirects into docs or
code. Each maps to CLAUDE.md rules 1, 2, 9, 10, NIGHT60 D-3 or L-047, and none is used by this project's normal workflow (the self-test's
allow cases: explicit-path commits, cherry-picks, reverts, the TMP fallback restore, `git add -f Logs/...`, reels, builds). **Side effect
for GG:** Claude in this project now refuses to push, merge or switch to master even when GG asks; GG does those in his own terminal, or
deletes the hook block in `.claude/settings.json`. Rejected: hooks for rules never broken in w55-w60 beyond these (audio code, Mover.cs:
no evidence they need one).

## D-6 Disk: the guard refuses builds and reel sets under 12 GB; GG frees space before NIGHT61
C: is at 11.4 GB (09:00). The night may delete only its own output, so it cannot free space it did not write. NIGHT61 handles it in the
bearings (prune its own frames; if still under 12 GB, do the non-Player slices and say so), and the hand-off asks GG first. Candidates
(GG's call, measured): `.claude/worktrees` (202 worktrees, 104-623 MB each in a sample of 8), NIGHT60's superseded builds
`Builds/n60-b1`..`n60-b8` (~3 GB; `n60-freeze` stays). Rejected: the forge deleting them itself (FORGE61 section 7 and the org rules:
deletion needs GG's go-ahead).

## D-7 Subagent caps in the project settings: 6 concurrent, 1 level deep
`CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS=6` (4 builders + 2 critics), `CLAUDE_CODE_MAX_SUBAGENT_SPAWN_DEPTH=1` (no sub-builders). Rejected:
the launch line (one more thing GG must type); the defaults 20 / 3 (L-003: the usage limit; nested spawns multiply tokens).

## D-8 Builder worktrees start at HEAD
`worktree.baseRef: "head"`: subagent worktrees otherwise branch from `main` (research-claude-code.md item 7), not port/stage3.

## D-9 Effort: the lead at the default, the judges high
Opus 5.5's default (`medium`) matched Opus 5 at `high` in fewer tokens (prompting-claude-opus-5-5); the usage limit cost 4:40 in w55-w60.
The critic, gg-proxy and the auditor carry `effort: high` in their files. Rejected: `--effort xhigh` for an 8 h lead (tokens -> the limit).

## D-10 The launch line appends nights/PROMPT-STOPS.md
`--append-system-prompt-file nights/PROMPT-STOPS.md` (the house's standing instruction against ending turns early; PROMPT-FORGE.md
section 5; the Opus 5.5 guide warns an unattended loop stops when a turn ends with only a status message).

## D-11 The forge writes tools, not game files
The `design.n61` block, its test and every row are NIGHT61's A1 (game config + tests). The forge made only tools: `n61-gates.sh`,
`n61-reels.sh`, `n61-prune.sh`, `n61-diskwatch.sh`, `n61-slate.cjs`, `n61-freeze-rows.cjs` (NIGHT60 spent 0:58 on tools at its start).
Checked: `fingerprint --off61` prints EQUAL (09:0x).

## D-12 The lead integrates; builders build; no third attempt at a SUBTLE change
The lead owns the windows, the Player, the critics and the two bug slices. A slice whose MAIN misses twice is SHORT with its numbers
(RETRO: rim56's three attempts took ~4 h for a change judged on 2x crops). Rejected: the lead taking a visible slice (the windows need it).

## D-13 The project skills get one step each
`skygrind-retro`: classify every shipped change VISIBLE / SUBTLE / INVISIBLE, and turn a rule broken twice into a hook. `skygrind-idea-lab`:
score "seen in the first minute" and drop ideas a blind SPOT could not name. Rejected: new skills (the gap is a step, not a skill).

## D-14 Not installed, needs GG's OK
`com.unity.testtools.codecoverage` (Editor-only coverage), JetBrains InspectCode (unused public members), Microsoft.Unity.Analyzers
(makes IDE0051 trustworthy on Unity code). NIGHT61's dead-code pass runs without them (grep + script-GUID census + compiler warnings).

## D-16 The /goal check (FORGE61 section 6.3)
Met without the work? The evaluator only reads printed lines; the condition needs the 6:00 date, the gates line, the freeze lines, the
slate tool's line, the tag and a clean `git status` of the close files: none can print without the step being run. A night where every
slice ends SHORT still meets it, honestly, after 6 hours (that is the report's job to say). Never met? Each step that can fail outside the
night's control (the Editor wedged, the disk under 12 GB, a failed build or smoke) has "or logged under Blocked"; the tag and commits
always work; the 8:00 escape needs only the committed report; a usage limit pauses the goal (research). Fixed in this pass: the "or
logged" clause now covers the build and verbs37 as well as the smoke, and the elapsed line names t0's place in NIGHT61-LOG.md (so it
survives a compaction). Not fixable here: a context overflow clears a goal (research-claude-code.md item 6).

## D-17 The red team (redteam.md, 72 findings): what changed and what did not
Fixed: the disk start state (bearings step 2: the watcher's line under today's free space, a declared no-Player night under 12 GB; GG
asked first in the hand-off) [1-3]; Editor timeouts read as "busy" before "unfocused", a 20-minute fallback to batch builds [6]; B2 never
runs Game M in the live Editor (batch copy only) [8-9]; every close step time-boxed with "log under Blocked and go on" [10, 66]; S2 judged
against `flow-base-a0.txt`, which `n61-gates.sh --flow-base` keeps [19]; `n61-gates.sh` deletes the stale flow file and checks a new one
was written [20]; `n61-slate.cjs` checks the verdict's first line `<id>: PASS` and a SPOT line naming a change [21]; SPOT moved to a new
`skygrind-spotter` agent that can only Read, one SPOT per slice per build, the coin flip logged, the pair differing only by the slice's
switch [22-25]; the GATE reads patches without commit messages (`git show --format=`) [26]; no sim row ON mid-night, S5 view-only [11];
the auditor checks UnityEvent `m_MethodName` targets and IDE0051 is a lead only [37-38]; S1 is a redesign and "misses twice tonight" [39];
S7's band in the level accent unless the press hurts [30]; `git diff --stat ProjectSettings` after builds, evidence force-added, "22
replays" explained, the commit-per-feature reading stated [58-62]; UI builders load `unity:ui` and use the theme fonts [63]; the clock: a
continuous integration block 1:00-5:30 with one build per ~45 min instead of three 45-minute windows, the harvest drafted by a builder at
the freeze (L-079) [64-67]; B1 moved to builder D, S6 to the next free builder [67-68]; bearings steps 4-6 first session only [16]; an
explicit end line [17]. Already fixed before it read them: the n61-freeze-rows name and the auditor agent [35-36]. Not changed: [57] the
order of authority (the org rules come first by design); [69] builders' worktree gates beside the lead's reels (the CPU cost is small
next to a reel; L-005 applies to benches); [71] reading LESSONS each session (119 lines, the rows are rules).

## D-18 The red team's second pass (redteam.md status section, 78 entries): fixed after the forge's first close
The watcher no longer starts under 12 GB (its line was above the free space, an exit loop) and the night re-checks the disk every hour;
the batch copy (NIGHT54's 6462b106, local changes, no remote) syncs only by fetch + detached checkout, else no batch fallback; the guard
lets a `-batchmode` Unity be stopped by PID (never the Editor) and checks the disk before batch builds and test runs (77 / 77 planted);
`n61-freeze-rows.cjs` refuses a row without a passed slate slice and a "<id>: PASS" verdict (planted: refused / accepted); a slice added
tonight is committed before it is measured; S2's SPOT pair is the builds either side of its pick; a dead or hung critic is replaced once,
then the row stays OFF; agent ids are logged for SendMessage; a red baseline gets 45 minutes; the dead-code pass protects --off57..--off61;
the close's boxes add up to 57 minutes, with the report and HANDOFF drafted by 6:00 and the Game suite the first step skipped.

## D-15 The dead-code pass never touches
`Mover.cs` and movement, the web-ported Core files kept line for line, the audio event names (CLAUDE.md rule 7), GG's files, and OFF rows
GG has not decided on (they go to a list for him).
