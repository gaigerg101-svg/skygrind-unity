# NIGHT50.md: ship the look. Calibrate the judges first, then put back the charge bar, ship the grade per level, turn the impact kit on, and put the charge on the gun (lead + three builders; 4 h, hard cap 4 h from GG's go)

GG's words (2026-10-19 ~11:20, verbatim; they outrank everything below; NIGHT49 did not meet them, so they stand):
> The charge bar under the AIM reticle needs to come back I don't know why it's gone also we just need a visual polish all across the
> game whether it's lighting whether it's design whether the arenas it's just the colors and stuff are way too bland The effects are
> inconsistent it all needs to be unified ... I'm very inspired by Ultra Kill and Doom and those types of games I wanted to have more of that effect

**Where we are (NIGHT49, tag u3-n49, play build Builds/n49-freeze, commit a9b1442d; NIGHT49-REPORT.md):** only the lights shipped (lights49 ON).
The charge bar is NOT back (arc40 beat three new candidates 3-0 each). look49 (grade) OFF: foundry and spire pass, kiln and span near, iceline
short; the sheet won or tied 2 of 5. fx49 (impact kit) OFF: its pair won 3-0 but the enemy kill's wash is +0.34 (the web death burst). G6 never built.
GG has played none of n44-n49. **Tonight's job is to turn built work into shipped work GG can react to, not to build a fourth round of options.**

**GG will not answer.** Every call goes in `DECIDE50.md` (untracked, Write tool). Base = HEAD of `port/stage3` (ed35f313 or later; log it).
t0 = GG's go; `date` on every log line. The research behind this prompt: `Research/2026-10-19-wave50/PROMPT-RESEARCH.md` (read it at P0).

**The row list is data:** `Tools/night/n50-rows.json` (tracked, written at G0 by the lead's shared script): one entry per row in section 1
with `id`, `check` (the command or meter that decides it), `evidence` (the frame / log path) and `passes: false`. Only the lead flips
`passes`, only after printing the check's output in the transcript, and never edits a row's `check` after G0 (Anthropic's long-running
harness: workers may only flip the flag; JSON is less likely to be rewritten than Markdown). The close table is generated from it.

## 0. Why NIGHT49 fell short, and the rules that answer it (hard rules tonight)
| what happened | rule tonight |
|---|---|
| Judges chose the familiar old arc40 three times; two pair sets were black or inverted; each judge saw a pair once | **Calibrate first (P0):** control pairs before any look; check every image's content (not black, right size) and the key's sha BEFORE judges see it (`Tools/night/n50-pair-check.py`). **Each judge sees every pair in both orders (A/B, then B/A, separate calls); a pick counts only if it is the same both times, else TIE** (the standard fix for position bias). A 3-0 consistent loss to the old retires the look. |
| Meters passed while pairs lost; the meter bands were hypotheses | The meter is a floor, not the verdict. Judges get a **rubric** (one question, three named criteria: readability, contrast, "which would a player of DOOM / ULTRAKILL call finished"), never "which is better". |
| Four builders, four rounds, open-ended options | **Three builders, two rounds, one candidate each.** A builder who loses a pair goes to the fallback below, not a new candidate. |
| Shipping decisions waited on GG, who does not answer | **Defaults decide.** Each row below names its default; the lead applies it and logs it. |
| Rows were "SHORT" with no path to GREEN | Every row has a **fallback that ships something** (below). A row ends GREEN, SHORT-with-the-fallback-shipped, or OFF-with-reason. |

## 1. Rows (G1-G6 carry GG's asks; each has an owner, a switch, a default and a fallback)
| id | row | owner | switch | what GREEN is | fallback that ships |
|---|---|---|---|---|---|
| G1 | **The charge bar back under the reticle** | H | `charge50` (view) | arc40 + meter42 re-gated ON (UiHud38.cs:339 / hud47 gate), **moved so its nearest pixel is >= 18 px from centre (outside the reticle box)** and >= 4 px tall at 1080p, the weapon's Look49 token colour, the BEND and STITCHER heat on it; pair vs NIGHT46's arc40 placement: wins or ties. | The judges already prefer arc40, GG asked for it back, so **the default is ship arc40 re-placed**; no new element. If the pair loses to old placement, ship the old placement. |
| G2 | **The charge on the gun, the second read** (ULTRAKILL's Railcannon: capacitors engage at quarter steps, a rear one glows at full; DOOM Eternal players called the plasma rifle's heat glow "hard to notice", so the gun never replaces G1) | H | `gunglow50` (view) | the viewmodel shows the charge in **four steps** (25 / 50 / 75 / 100 %) on KESTREL, THUNDERCLAP, PULSE, in the weapon's token colour; 0 at rest; one 2-frame flash at full; frames at 0 / 25 / 50 / 75 / 100 % at 1080p and 720p; the step change measurable (a printed pixel-luma delta per step in the gun's box); Player proof line RED once, GREEN once (L-014); the gun never moves (never-again list). | Ship KESTREL and THUNDERCLAP only; PULSE OFF with reason. |
| G3 | **Arenas and colours: look49 per level** | V | `look49` becomes `look49.levels` (a list of level ids; the row is ON when the list is non-empty) | per level: meter in band (dark >= 35 %, mid <= 55 %, saturated 10-30 %, 0 magenta) AND **the grayscale audit** ("most readability failures are value failures": on a grayscale combat frame, enemies and usable surfaces sit in a different value band from the walls; the meter prints the luma gap, logged before / after) AND the sheet won or tied. | **Ship look49 on the levels that already pass (foundry, spire, kiln), others OFF.** Iceline and span get ONE new attempt: lower saturation lift, darker floors, accent >= 20 deg from role hues; if it fails its pair it stays OFF. |
| G4 | **One impact kit, ON** | X | `fx49` (exists) | the 6 passing effects stay; **the enemy kill's wash <= +0.25**: the lead (not X) edits the web death burst in `SurvivalGame.Init.cs` behind `fx49` only (sizes and alpha, never an enemy colour); wash meter GREEN on all effects. | If the kill still fails, ship fx49 with the kill effect on its OLD path (per-effect gate), the other five on the kit. |
| G5 | **Lights stay** (lights49 ON) | the lead | `lights49` | still ON; verbs37 unchanged. | n/a |
| G6 | **The ULTRAKILL / DOOM effect, judged as one** | all | none | one before / after contact sheet per level (n49-freeze vs n50-freeze), judged blind by 3 judges on the rubric "which looks like a finished ULTRAKILL / DOOM-style shooter?"; counts as GREEN only if the new sheet wins 3 of 5 levels. | Report the count honestly. |

## 2. P0, the lead (before any builder is spawned, ~0:00-0:50)
1. `date`; HEAD check; read NIGHT49-REPORT.md, DECIDE49.md D-16 to D-19, Research/2026-10-19-wave49/ (CODE-FACTS, UNIFY, SPIKE), the n49-pair.py.
2. **Judge calibration:** write `Tools/night/n50-pair-check.py` (reject an image that is black, has the wrong size, or whose A/B key sha does not match;
   print mean pixel values) and `Tools/night/n50-pair.py` (the rubric, a seeded shuffle, both orders per judge, the key written once, the verdict
   per judge = the pick if both orders agree, else TIE). Run 3 control pairs (same frame twice; arc40 vs arc40 moved 6 px; arc40 vs a plainly worse
   frame). Judges must call the same twice a TIE and the worse frame a loss. If they do not, fix the protocol before any look is judged. Log it.
   Then **re-judge NIGHT49's four deciding pairs** (H c3 vs arc40, the V sheet, the L sheet, the X sheets) through the new protocol, from the saved
   frames, before anything is built: if a NIGHT49 verdict flips, that row's default changes and DECIDE50 says so.
3. Config: the lead's shared script (`Tools/night/n50-shared.cjs`, from n49-shared.cjs; dry run, every anchor asserted once) adds `design.n50`
   rows `charge50`, `gunglow50` (both off), the `look49.levels` list, a per-effect gate under `fx49`, FeelReel `a150` (H) and tests,
   and `Tools/night/n50-rows.json` (every row `passes: false`); both
   config copies byte-identical; golden 0 / 22 exact; fingerprints plain / --off49 / --off50 EQUAL. Commit it, then spawn H, V, X.
4. The lead's own task while they build: the kill-wash edit (G4) and the **wave-memory harvest 47-49** (T-102) at the end of the quiet window.

## 3. Builders (three; worktrees; one owner per file; each works through its list without stopping, one report per round)
| id | owns | first read |
|---|---|---|
| H | UiHud38 / UiHud33 / UiRail34 (charge element), Viewmodel emissive (gunglow50), UnityTuning.H50.cs | NIGHT49 DECIDE49 D-16, D-18; Research ULTRAKILL.md "weapon charge"; DOOM.md HUD; CODE-FACTS 1 |
| V | UnityTuning.V49.cs per-level values, PostFx.ApplyTheme33, fog / ambient / key, stone vertex colours | SPIKE.md; UNIFY.md; DECIDE49 D-18, D-19; the r3 meter lines |
| X | Fx49.cs, the effect files' reads of the kit; never `SurvivalGame.Init.cs` (the lead's) | CODE-FACTS 5; N49FxTests; DECIDE49 D-17 |
NIGHT50-BRIEF.md = NIGHT49-BRIEF.md with n49 -> n50, DECIDE49 -> DECIDE50, NIGHT49 -> NIGHT50, `--off49 is NIGHT48's game` -> `--off50 is NIGHT49's game`, and section 0 above.
Round 1 (~0:50-1:40): one candidate each, frames, meters. Lead builds `n50-r1`, ONE reel script, looks at every frame (path in the log), runs the pairs
through the checked protocol. Round 2 (~2:00-2:50): fallbacks and fixes only. No round 3.

## 4. The clock
| time | lead | builders |
|---|---|---|
| 0:00-0:50 | P0 (calibration, NIGHT49 pairs re-judged, shared commit), spawn | |
| 0:50-1:40 | kill-wash edit; verbs37 on the shipped rows; STOP POINT | round 1 |
| 1:40-2:10 | integrate, `n50-r1`, reels, look, pairs | (report) |
| 2:10-2:50 | round 2 orders from frames and votes; integrate; `n50-r2` | round 2 |
| 2:50-3:35 | **quiet freeze, builders stopped:** rows ON by default/fallback; `n50-freeze`; verbs37 x5; smoke; FULL batch suite (T-109); flow table incl. n48-foundry (write `n50-flow-table.sh` if absent, T-106); the G6 sheet and its 3 judges | stopped |
| 3:35-4:00 | close: **a fresh reviewer subagent** gets only n50-rows.json, the close table draft and the evidence paths, and reports rows whose evidence does not support their state (correctness gaps only, not style); the lead fixes those; then NIGHT50-LOG close table, NIGHT50-REPORT.md (outcome first, one frame per level before / after, the switch list), HANDOFF, DECIDE50, THREADS, ledger, tag `u3-n50` | |
A STOP POINT line with the close table so far goes at the top of NIGHT50-LOG.md and HANDOFF.md at every build and every 30 min, committed.

## 5. Hard rules (CLAUDE.md in full, plus)
- Never commit master, push, merge; builders in worktrees, the lead cherry-picks; explicit paths + `.meta`; never stage GG's four files; restore the TMP fallback asset before a commit; `NIGHT50.md` and `DECIDE50.md` untracked.
- Movement exact: golden 0 mismatches, 22 replays exact; fingerprints EQUAL at every Core commit. Tonight is view only except the lead's one `fx49`-gated edit.
- Numbers from config/UnityTuning, each with a comment; colours from Look49's tokens; cyan = usable, red-orange = enemies, magenta = RIVAL, pale violet-white = boundary; UI text >= 16 px; no audio; no enemy model change; no new full-screen shader.
- Docs with Write / Edit only (NIGHT49 slipped once with heredocs); the lead looks at every frame; builders report BUILT / MEASURED, only the lead writes GREEN.
- One Player at a time; the smoke, suite and flow table only in the quiet window; check CPU first (L-005).

## 6. Done when
G1-G6 each GREEN, SHORT-with-fallback-shipped, or OFF-with-reason in the close table, with frame paths; `n50-freeze` passes verbs37 (nothing lost vs 26 / 20 / 25 / 30 / 30), smoke DONE, full suite green, golden / fingerprints EQUAL; tag `u3-n50`; `git status` shows GG's four files and untracked prompts only.
**The experiment this night tests (forge):** calibrated judges plus a shipping default per row. Measure at the close: rows that ended shipped (ON) out of G1-G5, versus NIGHT49's 1 of 5.

## Kickoff (paste into a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`)
```
/effort high
You are the lead of NIGHT50. Read NIGHT50.md whole (GG's words are at its top), then CLAUDE.md, HANDOFF.md, NIGHT49-REPORT.md, Research/2026-10-19-wave50/PROMPT-RESEARCH.md and
DECIDE49.md D-16..D-19, and run the night exactly as written. Four hours, hard cap 4 h from my go. I will not answer: decide every call
in DECIDE50.md using each row's default and fallback, and keep going. P0 (judge calibration) comes first. Start with `date`.
```

## /goal (paste after the kickoff)
```
/goal Met when this conversation shows (a) NIGHT50-LOG.md's close table printed in full (G1-G6, each GREEN, SHORT with its fallback shipped, or OFF with its reason, and a frame path where one exists), generated from Tools/night/n50-rows.json, (b) the output of `git tag -l u3-n50` naming the tag, and (c) the reviewer subagent's report on the close table with every gap it found fixed or answered; or when a printed `date` shows 4 h since the kickoff (the close starts by 3:35 whatever is unfinished). Print `date` at the start of every turn. Run NIGHT50 nonstop as the lead: never ask me; decide every call in DECIDE50.md, applying each row's default and fallback. Order: P0 judge calibration (control pairs, content and key checks, both orders per judge, NIGHT49's four pairs re-judged), the shared commit, then builders H (charge bar re-placed + charge on the gun), V (look49 per level), X (impact kit); the lead edits the kill wash behind fx49. Every look ends in a checked blind pair with a rubric; the lead looks at every frame and logs its path. Smoke, full batch suite and flow table run once in the quiet freeze with builders stopped. STOP POINT line with the close table so far at the top of NIGHT50-LOG.md and HANDOFF.md at every build and every 30 min, committed. Commit every slice. Close with the close table, NIGHT50-REPORT.md, HANDOFF, DECIDE50, THREADS and tag u3-n50.
```
