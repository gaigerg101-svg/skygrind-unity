# NIGHT8.md: SKYGRIND in Unity, stage 2a (the Demo Day build), no human in the loop, about 8 h

You are Claude Code working alone in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP).
GG starts this run and leaves; the Editor is open on this project. Nobody answers questions and nobody playtests. This
file is tonight's job. The rules that always hold are in `CLAUDE.md` (in force: read it first). Where things stand is in
`HANDOFF.md`, and every work item is in `tasks.json`.

**Why tonight:**
- Demo Day 2 is **Wednesday Oct 7**: peers play SKYGRIND and give feedback.
- Research on 2026-10-02 (`DESIGN-RESEARCH.md`) found that the Unity slice's movement and verbs are right, but:
  - nothing contests the player's space (melee chasers only)
  - points stop mattering by round 6-8
  - the first 15 minutes have friction the web had fixed
  - feedback is thin
  - the director's valleys can vanish
- Tonight fixes those in that order of value. Co-op for Game 3 (Oct 28) starts the night after (tasks U4).

**Goal:** in the morning GG launches `Builds/Stage1/SKYGRIND.exe` (and, if U2-14 works, a Web build in Chrome) and plays a
run that a peer could play for 15 minutes:
- Settings first: sensitivity, FOV, comfort.
- Round 1 is husks; SPITTERS arrive on round 6 with parryable globs.
- Perk machines, kill drops and MODS drafts give the points something to buy.
- Shots shake and flash, the last enemies are marked, and the director breathes (PEAK FADE).
- A death shows what killed you; R restarts in under a second.

## 0. Tonight's rules (on top of CLAUDE.md, which is in force)

1. **Branch:** create `port/stage2` from the handoff docs commit (HANDOFF row 1). All of tonight's commits go there.
   Tag `u2a-demo` at the end. Leave `NIGHT8.md` untracked; log to `NIGHT8-LOG.md`, with times from `date` only.
2. **No questions, no waiting.** Pick the safe default, log why, and continue. Put a status line in the log at the end
   of every item.
3. **Which number goes where:**
   - Every number the web has comes from `PortExport/config.json`.
   - Every new gameplay number tonight (PERFECT PARRY, PEAK FADE, LAB) goes in `Design/unity-config.json` (U2-07 creates
     it), with a comment on why the web has none.
   - Never type a gameplay number into code.
4. **Defaults:**
   - PERFECT PARRY and PEAK FADE are on.
   - Every LAB toggle (U2-16) is off.
   - With every design flag off, the game is the web's: directorMath golden vectors bit-exact, the slice smoke
     unchanged.
5. **GG owns enemy visuals and sound** (CLAUDE.md rule 7). The SPITTER is the web's model and colours as they are, and
   there is no audio code.
6. **What each check proves:** a check proves only what it measured. Write "not checked" for the rest. `passing` in
   `tasks.json` needs its evidence: commit, output line, time.

## 1. Start (U2-01)

1. Read `CLAUDE.md`, then `HANDOFF.md`, then the U2A items in `tasks.json`.
2. Run HANDOFF section 1, every row. Write `RECEIVED` in NIGHT8-LOG.md with each result. If row 4 or 5 fails, fix that
   first (it is the baseline) and log it.
3. Load the skills HANDOFF section 10 names, when their item comes up.
4. Work the U2A items in the order of section 6 below. After each item:
   - set its status and evidence in `tasks.json`
   - commit (explicit paths)
   - log a status line

## 2. Definition of done (what GG plays in the morning)

1. Title: PLAY SURVIVAL, SETTINGS, AIM ASSIST, QUIT. SETTINGS persists across launches.
2. A run plays as stage 1 did (golden, replays and smoke green), plus:
   - spitters from round 6 (keep range, globs, puddles, parryable globs)
   - five perk machines
   - kill drops
   - MODS drafts on the web's draft rounds
   - PERFECT PARRY
   - PEAK FADE
   - markers on the last 3 enemies
   - the trauma shake, muzzle flashes, tracers and impacts
   - the freshness tier by the weapon name
3. Death shows the recap; R restarts in under 1 s; RESULTS opens on RESTART and shows personal bests.
4. Performance: the bench's median and worst 1% are recorded with the power plan (tasks U2-02). There is no fps floor
   tonight beyond "no worse than 9.35 ms median" (U1).
5. A Web build result: it runs in Chrome with N fps, or why it doesn't (U2-14).

## 3. Notes for tonight's items (decided; don't re-open them)

- **The design config (U2-07):**
  - `Design/unity-config.json` at the repo root, one object under `"design"`.
  - GameConfig reads it as the `design.` prefix. It fails loudly if the web config ever has a `design` key.
  - Tests read the repo copy. `SKYGRIND/Sync Port Data` copies it to `Assets/StreamingAssets/Design/`.
- **The LAB (U2-16):**
  - Booleans in the settings JSON under `lab.*`, and their numbers in `Design/unity-config.json`.
  - Any LAB toggle on stamps "LAB" on the results. A LAB run never sets a personal best.
- **Allocation work (U2-02):**
  - Wrap each sim system in a `ProfilerMarker`.
  - Use `ProfilerRecorder` for the GC counters.
  - The EditMode allocation test uses `Is.Not.AllocatingGCMemory()` from `UnityEngine.TestTools.Constraints` over 600
    steps of a 30-enemy sim.
  - Movement must stay bit-exact: run golden + replays after every Core change.
- **The HUD (U2-03):**
  - `SetText` only when a value changed.
  - Split the canvases by change rate.
  - A static font atlas with every glyph the UI uses (or images for the pips). Done when a Play run leaves the TMP
    fallback asset unmodified.
- **The SPITTER (U2-10):**
  - Port `enemies/spitter.js` on the swarmer's pattern: a Husk-bodied class with its own step.
  - Its globs and puddles are pooled Core objects.
  - `ai/aim.js` goes in Core. The brain gets its `ranged` token pool (config.ai.tokens.base.ranged).
  - A punch reflects a glob: look at how the web's punch reflects drone orbs before choosing.
- **Perks / drops / MODS (U2-11..13):** port `survival/perks.js`, `powerUps.js` + `powerUpMath.js`, `modMath.js` +
  `mods.js` line for line, keeping the web's comments. If the web has tests for a pure module, port them as EditMode
  tests.
- **Feel (U2-06):**
  - `fx/trauma.js` is pure. Port it to Core, feed it as `player/player.js` does, and apply it at render time only, never
    to the aim.
  - Effects use shared Particle Systems with `Emit`, not a GameObject per effect.
- **The Web build (U2-14):**
  - **The data loader:** the Player reads `PortExport` with `File.ReadAllText` (`PortData.LoadJson`). That cannot work
    on WebGL, where StreamingAssets is a URL. Load the JSON text with `UnityWebRequest` at boot on WebGL only (a short
    loading state). `JsJson.Parse` already takes text. Keep the Windows path as it is.
  - **The build:** `switch_build_target --target WebGL --confirm true` (a full reimport; do it once), build to
    `Builds/Web/`, then switch back to `StandaloneWindows64` before the close.
  - **Compression and serving:** set compression to gzip with decompression fallback, so a plain local server works.
    Serve it with a small Node static server (a written file, e.g. `Tools/serve-web.cjs`).
  - **The check:** open it with the Playwright MCP tools (`browser_navigate`, then read `browser_console_messages`).
  - **The smoke flag:** add a `?skygrindSmoke` URL flag to PlayerChecks (`Application.absoluteURL`).
  - **Settings:** log every ProjectSettings change the target switch makes; commit only the WebGL player settings you
    set on purpose.

## 4. Testing budget (build, don't test)

- **Per item (about 3 min):** one `bash Tools/unity-loop.sh compile`, one `tests`, and at most one Play-mode check (one
  eval file, or one capture if it's visual).
- **Captures:** move them from `Assets/` to `Logs/shots/` at once.
- **The slice smoke** runs in Q, in the Editor and in the Player. Extend it as items land:
  - restart through R (U2-05)
  - a perk buy (U2-11)
  - a spitter round reached through a debug round skip (U2-10)
- **Never:** two Play sessions or two builds at once, or a Player build before U2-14 / U2-15.

## 5. Mistakes not to repeat (NIGHT7)

1. **Hidden compile output.** Never send compile or test output to `/dev/null`. The helper prints the ground truth
   (`compilationFailed`, `consoleErrors`); the console's tail keeps old error lines.
2. **Code patched through a heredoc** broke on quoting. Write `.cjs` / `.cs` patch files with the Write tool; never
   `node -e`, and never a heredoc. This handoff's own author slipped once on a doc edit through `node -e`: don't.
3. **An Editor-assembly MonoBehaviour can't be added at runtime.** Runtime components go in `Skygrind.Game`.
4. **Test enemies spawned before round 1 begins are cleared** by StartWave's NewRun. Wait for the round, then
   `director.Stop()`, then spawn.
5. **Unfocused stalls:**
   - Each eval timed out after 5 s while GG used the machine.
   - At night: `editor_focus` before every Play check.
   - If two commands in a row time out, `editor_stop`, then `editor_focus`.
6. **A placeholder commit hash in a report draft.** Write a hash only from `git log` output.
7. **Captures at point blank tell nothing.** For a visual check, set the scene up: stop the director, freeze the
   enemies, place them.
8. **Play mode rewrites the TMP fallback asset.** `git checkout --` it before every commit until U2-03 lands.

## 6. Order of work and budgets (the tasks.json items; budgets include their own checks)

| Order | Item | Budget | "Closer" if it lands early |
|---|---|---|---|
| 1 | U2-01 verify on arrival | 15 min | |
| 2 | U2-02 zero-allocation steps, instrumented bench | 60 min | profile a Development Player once and list the top 5 allocators |
| 3 | U2-03 HUD and static atlas | 40 min | |
| 4 | U2-04 SETTINGS | 60 min | the web's CONTROLS page (live bindings) |
| 5 | U2-05 restart, recap, damage direction, PBs | 45 min | |
| 6 | U2-10 SPITTER, globs, enemy aim | 90 min | the debut round as a NEW: SPITTER card |
| 7 | U2-11 perks (five) | 50 min | more of the web's perks |
| 8 | U2-12 kill drops | 35 min | |
| 9 | U2-13 MODS | 60 min | the remaining mods |
| 10 | U2-06 feel parity | 50 min | the dash tint |
| 11 | U2-07 design config + PERFECT PARRY | 25 min | |
| 12 | U2-08 PEAK FADE | 25 min | |
| 13 | U2-09 markers | 15 min | |
| 14 | U2-14 Web build | 45 min | |
| 15 | U2-15 close | 45 min | |
| if time | U2-16 LAB, then U2-17 FORGE | | |

**If a phase runs 25% over:**
- stop
- ship what works behind a design flag
- log it, with the item `in_progress` and what remains in its notes
- move on

## 7. Cut order and done list

**Cut from the bottom:**
1. U2-17, then U2-16
2. U2-13's later mods (keep the draft and the movement mods)
3. U2-09
4. U2-12
5. U2-06's speed lines and dash tint

**Never cut:** U2-01, U2-02, U2-03, U2-04, U2-05, U2-10, U2-14, U2-15.

**Done list:**
- `run_tests` green, with the count above 182 and every new test passing
- golden 0 mismatches; 16 / 16 replays
- the Player rebuilt and its smoke passing
- one bench with the plan polled
- the Web result written down

## 8. Close (U2-15): the handoff loop

1. The checks in tasks.json's `invariants`, with the results in the log.
2. `tasks.json`: every U2A item has a status and evidence. Never delete an item.
3. **Rewrite `HANDOFF.md`** with the `handoff` skill and HANDOFF's own structure. Rewrite it, don't append:
   - a new anchor
   - the verify block with tonight's expected values
   - next actions: U3 and U4-01, which start co-op research for the Oct 14 checkpoint
   - decisions made and pending
   - risks, dead ends, invisible state
   - a changelog line
4. Update `PORT-STATUS.md`: systems and the event list, with new event names marked port-only.
5. Write `NIGHT8-REPORT.md` for GG:
   - what shipped, with commits
   - the numbers
   - what was cut, and why
   - how to play (exe, Web), and how to toggle the LAB
   - a 10-minute Demo Day checklist with one question per new feel point
6. Promote any lesson seen twice to memory (one lesson per file, and its index line). Propose CLAUDE.md changes in
   HANDOFF's pending decisions; don't edit CLAUDE.md unattended.
7. Tag `u2a-demo`. `git status` shows only `UnityConnectSettings.asset` and the untracked `NIGHT7.md` / `NIGHT8.md`.
   Nothing pushed, nothing merged.

A standing instruction from the user, the person you are working for. It is about how your turns end. A message with no
tool call in it ends your turn, and the work stops there until you are asked to continue. The user has seen you end turns
in four ways while work they asked for was still owed, and does not want any of them. One: a long summary of what was
done that closes by announcing the next step and has no tool call, so the next thing never starts. Two: an offer to carry
on with something unless the user would prefer otherwise, which stops to wait for an answer the user was not going to
give. Three: a list of decisions for the user when, by your own account, none of them blocks the rest of the work. Four:
deciding that this is a good place to report, because the turn has been long or a milestone is done. Status notes are
welcome, and so are your recommendations on open decisions, but put them in the same message as your next tool call and
carry on with whatever does not depend on the user's answer. If you notice yourself inviting the user to redirect you or
offering to wait, delete it and do the next thing. The stops the user does want are the ones where nothing can move
without them, or where the thing blocking you is deliberately protected from you. This does not override the need for
confirmation on risky or destructive actions.
