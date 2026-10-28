# D1 teach42: delete design.n42.teach42 and UiTeach42.cs (builder D, NIGHT71, 2026-10-26 ~11:31 MST, t0+0:45; base 78cf3f743)

Source: Research/2026-10-26-forge71/DEADCODE-candidates.md Part B.1 (score 7). Slate Logs/n71/slate.json D1.

## Never-delete checks
- HANDOFF.md, docs/memory/THREADS.md, OWNER.md (main tree): no hit for teach42.
- DECIDE42.md (main tree, untracked, read only) D-11: "The first-minute trim (teach42, G6) | OFF: the reel's walk showed 2 lines in level
  1's first 60 s with or without it ... | play the first minute and say what is too much (the room titles count too?)". The default OFF
  is the recorded verdict; the slate (D1 `why`) and DECISIONS D-9 item 4 class it as decided OFF, not open.
- **For the lead and the critic (not hidden):** DECIDE43.md line 3 says "DECIDE42's open rows carry (D-9 boost42, D-10 parry42, D-11
  teach42, D-12 arena42 ...)". No page after DECIDE43 carries D-11 (`grep -l -i teach42 DECIDE*.md`: DECIDE42, 43, 49; DECIDE49's hits are
  drone49 only). The row's header also quotes GG's G6 ("way too much instruction at the tsrat of the level"), as reticle42's quotes G8;
  the audit held reticle42 back as CANDIDATE for that reason and ranked teach42 DELETE. GG's G6 ask is served today by other rows (teach43
  ON: the centre card); this commit is one `git revert` if the lead reads D-11 as open.
- UiTeach42.cs.meta guid 0c25c8e03b2a8c66100ae471def0bbfc: `grep -rlI <guid> Assets --include=*.unity --include=*.prefab
  --include=*.asset` printed nothing (UiTeach42 is a partial of the Ui class, not a component). No serialized field (plain private fields
  of Ui), no `-skygrind*` flag, no audio event, no test names a Teach42 symbol (git grep in Assets/Tests: 0).

## The read before the edit (UiTeach42.cs, 71 lines, removed)
- `Teach42On()`: on first call reads `t42Read = N42View.RowOn(cfg, "teach42") ? 1 : 0`; with the row off t42Read = 0, nothing else is
  read and `game.OnNewRun` is not wired; returns `t42Read == 1` = false. (With game or cfg null it stays -1 and returns false.)
- `Teach42Hold(frameDt)`: `if (!Teach42On()) return false;` first: false, no clock, no counter.
- `Teach42Lesson(lesson, bossLine, teach)`: `if (!Teach42On()) return lesson;` first: the input unchanged.
So with the row OFF, UiHud33.Slot33's lines were `teach = false ? null : Teach35Want(frameDt, Teach35SlotNow)` and `lesson = lesson`:
the HUD is unchanged by the removal.

## What was removed
- Assets/Scripts/Game/UiTeach42.cs (71 lines) and its .meta.
- UiHud33.cs:772 is now `string teach = Teach35Want(frameDt, Teach35SlotNow);` (its comment keeps the teach35 half); line 773
  (`lesson = Teach42Lesson(...)`) dropped.
- FeelReel.N42.First.cs (the first42 reel): the `started0 / held0` counters (line 39), the line's `teach42 <row>` and `(teach42 started
  .., held frames ..)` terms, and the teach42 words in its header comment. No Tools script parses the first42 line (git grep first42 in
  Tools: 0). The reel line now reads `... foundry42 <bool> | lines started in the first 60 s: <n> | frames <n>/7 | ...`.
- the config block `design.n42.teach42` (11 lines) from both copies; `cmp` clean; JSON parses; design.n42 keeps its other 14 rows.
- comments that described teach42 as live: N42View.cs:1 (the view-row list), UiPinHint63.cs:2 (the hook's place), FeelReel.N52.Hud.cs:344.
- No test removed (no test named a Teach42 symbol).

## Checks (Research/2026-10-26-wave71/D/teach42-checks.txt)
- dn-check: 0 errors on the four projects. n71-gates --no-editor: config identical; golden 0 failing, 0 mismatches, 22 replays exact;
  fingerprints plain and --off57 .. --off67 EQUAL (12 / 12); "gates: all GREEN 11:33:20".
- Re-grep `git grep -n -i teach42 -- . ':!Research' ':!*.md'` after the edit: left as history: Core/N42Blocks.cs:5 (a Core comment; not
  touched: no Core edit tonight), the design.n42 `_why` prose (config line 6198, both copies), Tools/SimHarness/Harness.N42.Level.cs:1
  and Program.cs:235 (comments), and the NIGHT42 generators Tools/night/n42-config-block.cjs, n42-shared-patch.cjs, n42l1-teach42-row.cjs
  (not run). Every Teach42 / t42 symbol: 0 hits.
- Test classes touched: none. The first42 reel's line changes (the lead's next build; not run here).
