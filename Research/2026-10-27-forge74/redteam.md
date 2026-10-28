# FORGE74 red team (2026-10-27, read-only; one dry run: `n72-close-check.cjs --night n74 --pre`)

## BLOCKER

1. **`CLOSE-CHECK n74: PASS`, MINIMUM PASS and `--tag` are all impossible: `Tools/night/n74-slate.cjs` does not exist.**
   - Evidence:
     - `n72-close-check.cjs:149` runs `Tools/night/<night>-slate.cjs` with no argument.
     - `n74-tools.cjs` writes no such file; its line 14 calls the slate tool "unchanged" and NIGHT74.md names only `n72-slate.cjs`.
     - The dry run printed `FAIL slate  the slate tool printed nothing (exit 1)`.
     - `minOk` (line 203) also needs `R.slate[0]`, and `--tag` stops on a PRE FAIL, so GOAL.txt can never be met.
   - Fix:
     - Add `n74-slate.cjs` to `n74-tools.cjs`: a copy of `n72-slate.cjs` with the default path `Logs/n74/slate.json`. Run it.
     - Change NIGHT74.md lines 31, 113 and 254 to call it.
     - Before the night, the forge runs `--pre` again and explains every FAIL line.

## HIGH

2. **Section 9 (b2) cannot produce a PASS close.**
   - Evidence:
     - The close-check reads only `Logs/feel/n74-freeze/summary47.txt` and `Logs/player/n56-smoke-steal-n74-freeze.log` (lines 126, 133). An `n74-freeze2` rebuild and its re-runs are never read.
     - A diagnostic re-run with `foundry65:0` appends under the same side name to `n74-freeze`'s summary, and the last section wins (line 128). That can turn a RED side GREEN with the row OFF.
     - A RED written under `## Blocked` does not satisfy `player` (it needs the guard's words, lines 125 and 134).
     - "RED at 7:25: `--start` ... `--tag --minimum`" fails. The start reads 7:25, which is before capMin-30 = 7:30, so `ateClose` is false (line 191) and PRE is FAIL.
   - Fix:
     - Diagnostic side names must be `vi-a`, `vi-b` and so on, never vf/vn/vi/vk/vs/vp.
     - Rebuild after `mv Builds/n74-freeze Builds/n74-freeze-red` so the fixed build is `n74-freeze` again. Re-run the sides and the smoke under that tag. Make the play build from it.
     - The RED fallback reads: wait (Monitor) until 7:30, `--start`, recommit, `--tag --minimum`.

3. **R0's doneWhen is impossible and contradicts the prompt.**
   - Evidence:
     - slate-seed.json:145 and builder-brief.md:16-17 want `slots65.json` b1 and a8 "new via n65-slot-set.cjs".
     - `n65-slot-set.cjs:3-5, 95` needs a slate slice named b1 or a8 with a PASS (default `Logs/n65/slate.json`, NIGHT65's). No such slice is in `Logs/n74/slate.json`, and builders cannot edit it.
     - NIGHT74.md section 5 step 2 and spike S1 say the slots stay "old".
   - Fix: delete that clause. Replace it with "slots65.json untouched (all old)". Strike "slots65.json" from the brief's R0 line.

4. **The two-build room pair has no "same pose".**
   - Evidence: tour65 poses come from the room row's `frames65`, with `frames36.mid` as the fallback (`FeelReel.N65.Tour.cs:7-9`). A rebuilt room brings new poses, and the old room in the earlier build has its own. DECISIONS D-3 and NIGHT74.md:149 never say how the pose is fixed. The spotter would compare different cameras.
   - Fix: pass `-skygrindTourPlant65=<room>:x/y/z/yaw/pitch` at the room's entry door (doors are KEPT) on both builds, and pair the "planted" frame.
   - Better, and not checked: copy the pre-pick level as `<id>-prev.json` into that train's build and tour both maps in ONE build. That is a same-build pair, with no other row confounding it and no build spent.

5. **Reels on shipped maps break once a level row is ON in the repo config (L-154).**
   - Evidence: only `vf` gets `:0`. The sides that load shipped maps by name get no flag:
     - the H-train and later boss63 sides on `n52-spire` / `n57-kiln` (NIGHT74.md:167-168; slate B1/B2/L3 `red`);
     - the OFF-side tours (line 167);
     - the play reels `bp,n52-spire,boss63` (line 250).
   - These run under ON rows. The preview frames GG gets show the pre-row boss room, not the room he will play.
   - LV-kiln67's doneWhen (kiln73 danger share with the row ON, slate-seed.json:72) needs boss63 on `n67-kiln`. No listed reel set runs it.
   - Fix:
     - Every side names its row flag. ON maps get `<row>:1` and n52 / n57 maps get `:0`.
     - The play reels become `bp,n67-spire,boss63,...` plus `bk,n67-kiln,boss63`.
     - Add `boss63` on `n67-kiln` to the H train.

## MEDIUM

6. **gg-proxy timing is off at both ends.**
   - 0:30 (lines 88, 173): `n74-h0` builds at about 0:25, and the tour and boss sides come after the six close sides in the same set (line 167). The frames exist at about 0:50. Order tour and boss first, and say "start when they land".
   - 7:05 (line 251): (d) runs after `--start` at 7:00, then the smoke (4 min), then 5 reel sides, so the frames land at about 7:12-7:20. The guard blocks agents at 7:15 (n74-clock.cjs:15, 435 min). Make the play build and its reels right after the freeze reel set, and run the smoke and gates after.

7. **The report headings are not named.** The `files` check needs `^## 2.` and `^## 8.` (close-check:106). NIGHT74.md:274-284 gives bare numbers. Add "headings `## 1.` to `## 8.` exactly, in the skeleton commit too".

8. **Rooms go in before they are proven.**
   - Step 2 regenerates and commits the room into the level file, which is ON, before the pair, SPOT and guards. That contradicts section 0 rule 3. The last pick (5:45) is judged at about 6:20, and the "out" regeneration must land by 6:30. Add a cut-off: no room pick after the 5:10 train unless it can be judged by 6:15.
   - `seen` (close-check:170): a room slice carries `on: n65.foundry65` (and n67.kiln67 / n67.spire67). It fails the close if its level row is OFF. Add "a room can PASS only while its level row is ON; else SHORT".

9. **Two guards in section 5 are weaker than they read.**
   - No EditMode run follows the level flips, and the guard blocks test runs from 7:15. N65SharedTests tolerates ON via `Assert.Ignore`. I did not check the Curriculum and MapChoice tests. Run `tests N65Shared`, `tests Curriculum` and `tests Rows72` (one substring each) after the first flip and again before 6:30.
   - The smoke looks level-agnostic: a grep of `SliceSmoke.cs` found no level selection, and the file uses `g.map.armoryWall*`. So "smoke DONE with the row ON" says nothing about levels 2, 3 and 5. Say verbs37 plus the tour carry those rows.

## LOW

10. Generated companions are left dirty after each regeneration. The commit lists omit them: `foundry.plan.json`, `foundry.json`, `foundry.assembly.json`, `openings65.json` / `openings67.json`, and `Reports/LEVEL-*-validate.md`. R0's later commit touches `foundry.plan.json`, so a dirty tree can block the pick. Commit them with the level files.

11. Loose ends a fresh lead would trip on:
    - `n72-slate-add.cjs` defaults to `Logs/n72/slate.json`. Pass `Logs/n74/slate.json` (NIGHT74.md:202).
    - `rows74.json` names `n74-hue.py`. Only `n73-hue.py` exists.
    - rows74.json labels reveal74 "R4", crown74 "L3" and flick74 "L2". The slate says L2, L3 and L4.
    - Slate B1's `red` says "bosses-d". That is a NIGHT73 sheet, not a scenario.
    - No train runs boss63 for THE WICK, THE LOOM or THE ICEBREAKER. weak74's brief covers them.
    - `n72-freeze-rows.cjs` and `n72-off-row.cjs` write "NIGHT72" labels into the config.
    - NIGHT74.md:121-122 does not list the paths of S0's commit: both config copies, `SameRun63Math.N63D.cs`, `FeelReel.N74.cs` and `.meta`, `FeelReel.N57.cs`.
    - NIGHT74.md:107 asks for "commit free memory" without a command.
    - gg-proxy's prompt is not recorded; only the previous output is.

12. D-5 says "6 trains fit". The prompt has the H train plus four. The lead is overloaded from 5:10 to 6:30: t4, the a0 drafts, the 5:50 reviewer and the freeze picks.

## Applied by the forge (2026-10-27 ~22:20)
1. n74-tools.cjs now writes n74-slate.cjs and n74-slate-add.cjs (default Logs/n74/slate.json); NIGHT74.md calls n74-slate.cjs. Re-run
   `--night n74 --pre`: the slate line now reads "cannot read Logs/n74/slate.json" (bearings creates it), not "printed nothing".
2. Section 9 (b2): diagnostic side names (vi-a ...); the red build and its feel folder are moved aside and the fix rebuilt as n74-freeze;
   the minimum fallback waits until 7:30 before `--start`.
3. R0's doneWhen and the brief: slots65.json untouched (list mode).
4. Rooms and level rows pair on a planted door pose (`-skygrindTourPlant65`), section 5 step 5, the H train and D-3.
5. Section 0: every side names its level rows; the H train adds boss63 on n67-kiln; the play reels use n67-spire / n67-kiln.
6. gg-proxy starts when its frames land (H train tour and boss sides first; the play build moved to (b)3, before the smoke and gates);
   its prompt is written down and saved.
7. The report headings `## 1.` to `## 8.` are named.
8. A room cut-off (no pick after the 5:10 train) and "a room PASSes only while its level row is ON".
9. Tests N65Shared / Curriculum / Rows72 after the first level flip and before 6:30; the smoke is said to be level-agnostic.
10. Generated companions committed with each regeneration.
11. n74-slate-add.cjs; rows74 labels fixed (L2 reveal74, L4 flick74); no n74-hue.py named; B1's red names wick37 / loom37; the S0 commit's
    paths listed; the commit-memory command given. n72-off-row.cjs takes no slate (corrected in (b2)). The "NIGHT72" labels the n72 flip
    tools write into the config are left as they are (cosmetic).
12. The reviewer moves to 5:20 and the drafts to 5:00; D-5's "6 trains" corrected.

## Checked and fine

- These commands exist and take the arguments given:
  - `n74-rows.cjs --dry` and `n72-slate.cjs` on the seed (20 slices, exit 0);
  - `n74-reels.sh` (the n56 smoke log path matches the close-check);
  - `n74-pair.cjs`, `n74-slate-set.cjs`, `n74-caps.cjs`;
  - `n72-freeze-rows.cjs --slate=`, `n72-off-row.cjs`, `n72-gates.sh --no-editor --flow`;
  - `-skygrindRows72` (it handles level rows by writing `design.<n>.<row>.enabled`, and `+` lists);
  - the guard's closeTags substring rule (`n74-freeze2` is allowed after 7:15).
- `close-check` sides: `vn` is required only when foundry65 is ON; vf, vi, vk, vs and vp always are. No n67 row changes the list.
- The fingerprint and golden gates do not depend on level rows (the fingerprint uses `survival.json`).
- The 7:00 start, the 7:30-8:00 stop window and `u3-n74` being absent all match the close-check for capMin 480.
