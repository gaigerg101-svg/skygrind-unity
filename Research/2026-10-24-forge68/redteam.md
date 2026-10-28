# FORGE68 red team: NIGHT68.md and slate-seed.json (2026-10-24)

Read: NIGHT68.md, the slate, GOAL.txt, history.md, prompt-methods.md, the brief and DECISIONS (grep), the heads of the n68 tools and
gen-slots / n65-slot-set / n65-play-build. Ran: n68-slate.cjs on the seed (exit 0) and close-check --selftest (PASS, 28 cases). Not read
(time): levels-data.md, research.md, code-scout.md, NIGHT67-REPORT.md, HANDOFF.md. Most severe first.

## 1. The slate has no verbs, and every tool that runs before r1 needs one (blocks the reds and r1)
- Line: slate-seed.json:110, 132, 154, 176, 198, 220 (`"verb": null` on all six rooms) and `_why` ("Each room's verb, mainBar and
  ceilings are written by the lead from its red"). NIGHT68.md never says when or how the verb is set.
- Evidence: n68-red.sh head (Tools/night/n68-red.sh:36-39) takes each room's verb from the slate, and "a verb65 / slate MISMATCH stops
  the red"; n68-verbs-of.cjs:7 counts "the slate has none" as MISMATCH when the row carries verb65 (kb3c's, g8's and kb3's r3 files do);
  n68-round.sh head (:92-93) "a MISMATCH stops the round before any run"; n68-red-bars.cjs:29 refuses when the red's planted verb differs
  from `s.verb`. With null verbs the reds plant nothing (no flow-V, no ride pilot), the bars cannot be written, and r1, the chain meant to
  compute through the stop, stops at its first step.
- Fix: in section 1, after the card critic (about 1:05) and before any red: `node Tools/night/n68-slate-set.cjs <id> verb=<the card's verb>`
  for the six rooms, then commit. Change `_why` to say the verb comes from the card, and the bar and ceilings from the red.

## 2. I4 cannot PASS before the reds, and the reds wait for I4's PASS (a loop)
- Line: NIGHT68.md:107 ("The reds on I1-I4 (after I4's PASS, in series, about 1:45)") against slate I4 doneWhen 3-4 ("every room of the
  slate re-red on I1-I4 ... committed before r1" and "one critic PASS for I4 with the reds").
- Evidence: each needs the other done first. DECISIONS.md D-7 queues the reds in the r1 chain, a third version.
- Fix: the reds start when I4 lands (pair GREEN), not at its PASS; its critic reads pair and reds together. Match D-7 to step 7.

## 3. Block 1 does not fit: the reds alone end after the predicted stop, so r1 cannot be chained on bars by 2:10
- Line: NIGHT68.md:111-112, :146, table :165.
- Evidence: I4 lands 1:45 and a critic takes 15-23 min (CORE L-103; history.md:31), so the PASS comes about 2:05. On NIGHT67 the reds took
  4-7 min and their 10-seed skips plus bars took 18 min (history.md:28, :32). At 20 seeds that is about 30-40 min, so the reds end about
  2:35-2:45. The stop is predicted 2:20-2:29 (history.md:43). The builders get 20 min on the final tools before "round 1 by about 2:05".
- Fix: start one background chain at I4's landing (about 1:45): reds, then `n68-red-bars.cjs --write`, then r1 for three levels, then the
  21-30 pre-runs. The lead picks round 1 before the chain starts, and commits the report skeleton, slate and verbs first (rule 6 forbids
  commits while it runs). The fallback at :112 (I1-I3 at 1:20 with two 10-seed runs) becomes the default when I4 has not landed by 1:30.

## 4. r2 can never reach a GATE before the close, so every PASS must come from round 1
- Line: NIGHT68.md:148 ("r2 after the reset (about 5:30)"), :149 (GATE packs "one at a time, never during a round"), :188 (--start
  7:00-7:15: "no GATE, slot or flip from here").
- Evidence: 5:00 resume, the r1 level review (~20 min), r1's packs (each a dotnet skip up to 900 s, n68-gate-pack.cjs:234) and critic.
  With one main-tree dotnet job (rule 6), r2 starts ~5:45. At 20 seeds a round is 2x NIGHT67's 4-8 min per level, 25-50 min, ending
  ~6:10-6:35. r2's review (+20) and packs, spotters and critic (+30-40) then land ~7:00-7:35, at or after --start. LV is due by 6:00.
  A first round has not passed before (L-150: FAIL, FAIL, PASS).
- Fix: (a) queue two variants per room into the stop chain (r1a / r1b), so block 2 GATEs whichever held; or (b) start r2 at the resume,
  before r1's packs, only for rooms with a named r1 cause, with r2's GATE started by 6:15. Size "Done well" to what one round can give.

## 5. The GATE's fresh seed set is 11-20 in the tool and one ROOM line, but 21-30 in M2: the GATE would judge seeds the builder tuned on
- Line: slate-seed.json:6 ("skip v2 also on seeds 11-20 (the GATE pack's fresh skip)") against :8 (M2: pooled 1-20, fresh 21-30).
- Evidence: n68-gate-pack.cjs:234 runs `--seed 11 --seeds 10`, and :4 / :232 say "seeds 11-20". The builder brief (:47) lets builders run
  seeds 1-20. If I4's gate-pack change (I4 doneWhen 2) slips, the GATE's "fresh" set is inside the tuned pool.
- Fix: slate :6 should read "seeds 21-30". Make I4 doneWhen 2's dry run print `--seed 21` in verbs65-skip.txt's command line. Have the
  critic brief refuse a pack whose fresh line names seed 11.

## 6. The 21-30 pre-runs computed through the stop are never used: the GATE pack has no way to read them
- Line: NIGHT68.md:146-148 (fresh-<id>.txt, "the GATE pack re-uses it only when the room file is unchanged since r1").
- Evidence: n68-gate-pack.cjs has no `fresh-` or `--fresh` path (grep empty). It always runs its own skip unless `--no-skip` (:203), and
  with `--no-skip` the fresh skip is a MISSING piece (:6, :238). No command for the pre-run is given either.
- Fix: add to I4 doneWhen 2 (T owns n68-gate-pack.cjs, builder-brief.md:14): `--fresh <file>` taken only when its header sha256 equals the
  round's room-files record. Write the pre-run command into section 4 step 4: `n65verb skip --level <round level> --room <id> --seed 21
  --seeds 10 --bars <slate-at-start>`.

## 7. The red's skip command is described but never given, and red-bars' regex may not read I4's pooled line
- Line: NIGHT68.md:109-110 ("each room's skip on its verb, seeds 1-20, planted with --plant-verb65 <id>:<verb>, into .../skip.txt").
- Evidence: n68-red.sh writes room-check, flow-A/B/V, pads, plan, ride, but no skip.txt (its head, :28-33). n68-red-bars.cjs:26 parses
  `M1 \w+ | room \S+, ratio ([\d.]+) vs bar.*?exercised (\d+)/(\d+)`. I4 adds a pooled median and set medians and renames the verdict
  to M2's line, and nobody is scheduled to update red-bars.
- Fix: give the exact command in step 7 (the level file path for n57-kiln / n52-iceline / n52-spire, `--seeds 20`, the plant, the
  header). Add to I4's doneWhen: "n68-red-bars.cjs reads the pooled ratio from a dry red (one room), printed".

## 8. The flow-base names disagree; LV's last line compares against a base that I3 makes stale
- Line: NIGHT68.md:96 and :105-106 (flow-base.txt, then flow-base-i3.txt "the --flow gate compares against it from then on"); slate LV
  doneWhen 4 ("after it, n46flow ... with --off67 equals Logs/n68/flow-base-a0.txt").
- Evidence: n68-gates.sh:19-20 writes flow-base.txt and copies it to flow-base-a0.txt only once. :25-29 always compares against
  flow-base.txt, and no option names flow-base-i3.txt. After I3 changes the walk bot, a0 cannot be EQUAL, so LV's doneWhen 4 cannot be met.
- Fix: say "after I3: `n68-gates.sh --flow-base` (re-writes flow-base.txt; a0 kept), then `cp` it to flow-base-i3.txt". In LV's doneWhen 4,
  replace flow-base-a0 with "the flow base current at the flip (flow-base-i3 or later)".

## 9. PC's method is impossible with the tools: a NOT PASSED room cannot be set "new" in a slots copy
- Line: NIGHT68.md:155-158 and D-8 ("every PASSed room and otherwise the night's best room by M2 set new in a copy of slots67.json").
- Evidence: n65-slot-set.cjs:1-6 refuses a slice with no PASS; gen-slots.cjs head: "a 'new' slot's file must be the one its PASS bound".
  PC's line also omits n65-play-build.sh's `--config` / `--add-level` / rows (its head).
- Fix: `gen-slots.cjs --level <L> --slots=<id>,<id> --run --out=Logs/n68/play/<L>` (the ids take their latest files, the rest the base),
  then `n65-play-build.sh n66-freeze n68-play <levels> --config Design/unity-config.json --add-level n67.<L>67 ...`.

## 10. The M2 STYLE rule has a cliff at 80 % that rewards a more stylish refusal; "within 0.10 reads UNSURE" is unmeasurable
- Line: slate-seed.json:8. Evidence: a refusal at 79.9 % needs the verb run at 94.9 %, while one at 80.0 % needs only 80.0 %, so lifting
  the refusal across 80 % lowers the room's own bar by 15 points. "Within 0.10 of its bar" does not say above or below, or if UNSURE fails.
- Fix (before any measurement, rule 8.4): verb STYLE >= min(refusal + 15, max(refusal, 95)); "a ride ratio in [mainBar, mainBar + 0.10) is
  UNSURE and does not PASS M2".

## 11. The worktrees hold modified tracked files, not only untracked ones (NIGHT68.md:99-100)
- `git -C .claude/worktrees/agent-a9d0ffb6d0dfdb2be status --short`: 9 tracked files ` M` (n67-iceline/spire.json, their StreamingAssets
  copies, plans, assemblies, openings67.json). agent-aeb9fc116ba05ef4d has the same for kiln / spire. They equal HEAD's blobs, so
  `switch -c` carries them dirty into n68-R / n68-S. Fix: "the ` M` generated files carry over; never stage them". Restoring them with
  `checkout --` needs a read of their diff first (L-047), and `stash` is banned (rule 4).

## 12. Smaller slips
- Rule 6 forbids commits while the chain runs, but rule 11 commits the report "about 2:10" (:81), when the chain starts (:146). Commit the
  report skeleton, STATE.md, the verbs and the bars first, then launch the chain.
- :124 gives T "harness only, in Tools/SimHarness", but builder-brief.md:14 also gives T n68-round-verbs.sh and n68-gate-pack.cjs, and I4
  needs both. List the same files in both, and name who updates n68-red-bars.cjs (finding 7).
- :140 `for L in ...; do L67=$L ROOMS="<its rooms>"` cannot vary ROOMS per level. Drop ROOMS: it defaults to the slate's rooms of that
  level (n68-round.sh head :94). :142: the round already writes area-<id>.txt and deco-<id>.txt (n68-round.sh:86-88), so say "read".
- The slate's kb3c / g8 / kb3 `red` says "NIGHT67 r3; re-red", against :111 ("never NIGHT67's r3 file"). Write "the shipped room".
- Rule 4 ("a deletion, a bulk change ... needs GG's own message") read literally blocks v2 "from its blank rect". Say that a room file's
  pieces are not a rule-4 deletion. LV's rc1 is "named n68-freeze" and the close's (b) builds n68-freeze again: name rc1 n68-rc1.

## 13. Against prompt-methods.md's ten rules
- Rule 2 / anti-pattern "a compaction defence that is only a sentence": `.claude/settings.json` has no SessionStart hook (grep empty).
  STATE.md is re-read only because the prompt says so, and NIGHT68.md is conversation text that compaction summarises. Add a
  SessionStart(compact) hook printing STATE.md and section 0 (GG's call), or say that D-10 measures the sentence-only defence.
- Rule 5 / rule 1: there is no closing stop paragraph (wanted stops: a guard refusal, the disk floor, a GG-only call), and the end
  condition is not repeated last; the file ends with the report order. Rule 4: rules 3, 4, 5, 8 and 11 have no bracketed reason (:50
  says each does).
- Rule 6: there is no glossary. M1-M4, GATE, red, r<n>, LV, PC, STYLE / DULL, line65 / verb65 / air65 / tour65, rc0 / rc1, A-lean and
  refusal are used undefined, so an agent spawned after a compaction meets them cold. Rule 7: the GATE critic and level reviewer get no
  calibration example (one past PASS, one past FAIL). E3: no `effort` per role is set (none in NIGHT68.md or the brief).

## Top 5
1. The slate's verbs are null and nothing sets them; the reds, red-bars and r1 all refuse without one (n68-verbs-of / n68-round).
2. I4's PASS needs the reds and the reds wait for I4's PASS; untangled, the 20-seed reds still end ~2:40, after the stop.
3. r2 starts ~5:45 and cannot reach a GATE before --start at 7:00-7:15, so every PASS must come from round 1.
4. The GATE's fresh set is seeds 11-20 (n68-gate-pack.cjs:234, slate :6), inside M2's tuned pool; the 21-30 pre-runs have no reader.
5. PC's "NOT PASSED room set new in a slots copy" is refused by n65-slot-set / gen-slots: use `gen-slots --slots=<ids>`.
