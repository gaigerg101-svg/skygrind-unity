# FORGE62 retro: NIGHT61 (and what it adds to FORGE61's retro of nights 55-60)

Numbers only from the files: NIGHT61-LOG.md, NIGHT61-REPORT.md, DECIDE61.md, Logs/n61/, docs/memory/ledger/wave-61.md, git log u3-n60..u3-n61.
FORGE61's RETRO.md (Research/2026-10-22-forge61/RETRO.md) covers nights 55-60 and still stands: of 11 rows ON in 55-60, 5 were visible in a
level's first minute.

## 1. What did GG actually get to see? Nothing.
- **0 rows turned ON on NIGHT61** (NIGHT61-REPORT.md section 2; `enabled` printed at the close, NIGHT61-LOG.md "Close" (g)).
- Builder hours by class (from NIGHT61-LOG's times): visible slices built OFF and never seen: S1 exit frame (V, ~70 min), S3 boss rule line
  (G, ~75), C1 one line of text (G, ~30), S4 light fixtures (V, ~65), C3 gun yellow (V, ~21): **about 4 h 20 of builder time, 0 seen.** Bug
  and dead code: B1 (D, ~55 min), D1 35 batches (D, ~1 h 50), S5 SHORT (G, ~20). Lead: B2's read (~30 min), a fresh review (~50 min, 2 real
  bugs in a proof reel, fixed 545e9fbc).
- **Why:** two preconditions failed before the first slice: C: had 8.3 GB free (the guard refuses builds under 12 GB) and the Editor's
  Pipeline never answered (10:21 to the close). Neither was the night's to fix (CLAUDE.md rule 10; deleting is GG's call).

## 2. Repeated mistakes, and whether a written rule stopped them
| mistake | earlier rule | NIGHT61 | now |
|---|---|---|---|
| The disk under the floor at the start | L-104 / L-113; FORGE61 D-6 asked GG to free space first | the night started at 8.3 GB and fell to 4.1 GB (builders' compile scratch 681 MB each, worktrees ~165 MB) | the guard refused every build and reel (it worked); the hand-off must make the space a precondition with a list (Logs/ 200,611 MB, Builds/ 70,465 MB by du) |
| Game M run synchronously in the live Editor | T-147 (since w41); NIGHT61's B2 said "never in the live Editor" | NIGHT60's close had left MapChoiceTests running in it (NIGHT60 HANDOFF line 25): the Editor was deaf all night | **a hook** (guard rule game-m-sync, FORGE62; 86 / 86 planted; live refusal shown) |
| A backgrounded `&&` chain loses its output | L-116 (written 14:49) | twice (12:36, 12:52) | broken again in FORGE62 at 16:57 (a syntax-check echo lost): **a hook candidate** (FORGE62 D-7) |
| A cherry-pick range from a copied sha | none | once (13:37; aborted, no bad commit) | L-117; the slate and the log name each builder's original shas |
| The Editor wake | the unfocused-autotick memory | one wake, set_autotick timed out while focused; the give-back needed the Alt-key unlock | L-118: one try only |

## 3. Where the hours went (the lead, t0 10:18:34 to the close 16:31:29)
- Bearings and the failed wake: 10:18-10:45 (27 min). A0 / A1 (flow base, gates, design.n61, the shared test): 10:45-11:15.
- Integration: 11:15-14:30: ten picks (B1, S1, S3, C1, D 1-22, D 23-31, D 32-36, S5's notes, S4, C3), each with a gate run (~4 min) and a combined
  compile (~2 min, 681 MB scratch) in the background; two config conflicts resolved by the builders' own row scripts.
- Writing (the ledger, the slate's states, LESSONS, the report, HANDOFF, the harvest): 14:30-15:05. The fresh review: 15:01-15:50. Waiting for the
  6:00 floor with nothing left to build: about 25 minutes (15:53-16:18) beyond the reel-timing fix.
- The close: 16:19-16:31 (12 min of a 60-minute box: every Player step was refused at once).
- No usage-limit stop. Agents: gg-proxy, the dead-code auditor, three builders, one reviewer.

## 4. What the critics and reports said was still bad and never got fixed
gg-proxy's ten (Logs/n61/gg-proxy-start.md) are unchanged in the game, because nothing turned on: empty boxy rooms; choppy movement and a camera
blind to speed; the text stack at room entry; bosses that never say what they want; dark rooms and floating lamps; the style meter at DULL; kills
not satisfying; the orange glow up close; the small dark exit door; yellow on the guns. Five of them now have code waiting behind OFF rows.

## 5. What was claimed and not proven
- Every NIGHT61 row's effect on screen (no frame exists).
- B1's GREEN (builder D's scratch harness re-implemented the checks: 48 / 48; the EditMode test never ran).
- D1's eleven touched test classes; verbs37 after the deletions (the fingerprints stayed EQUAL, which covers the sim only).
- The builders' scratch harness numbers (V 168 / 168 and 85 / 85; D 48 / 48): their own claims, the harnesses deleted.
- B2's mechanism and the Editor stall's cause (both hypotheses, though NIGHT60's handoff supports them).

## 6. What this means for NIGHT62
1. **Two preconditions decide the night:** 12 GB free and an Editor that answers. Check both first, before any plan; GG frees the space and
   restarts the Editor before the run (the hand-off says how much and where).
2. **P0 is NIGHT61's debt (T-148):** one Player build carrying the five OFF rows in its copy, each row's reels and proof line from its notes, a blind
   SPOT and a critic per row, ON for what passes. This is the cheapest path to something GG can see: the code is written and gated.
3. **The no-Player branch needs work that can still be proven:** a test path outside the Editor (the builders' harnesses show pure logic runs
   outside Unity; Port.Root reads Application.dataPath, so most tests need a one-line fallback first), the review's notes, T-146's items.
4. **Six hours, not seven:** NIGHT61's Player-less close took 12 minutes; a close with a build, five reels and a smoke is boxed at 57 minutes
   in NIGHT61.md section 9 (a plan, not a measurement: NIGHT60's close duration is not in its log as one number).
