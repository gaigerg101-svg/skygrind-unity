# FORGE71: GG's request and calls (2026-10-26, ~09:00 MST, right after NIGHT70's close)

## The request (verbatim)
"how is everything looking, use best methods online to anazlize what happened to  this pormpt (OPUS 5.5) follows those methods and what you think is best and
  the same methods used to build next prompt that doenst repeat same mistakes, i want amjroity building but wnat some time spent to kill dead code or things that
  we might have moved past (be careful with this) first you are makeing detailed prompt taht will do this"

Reading: [anazlize = analyze; pormpt = prompt; doenst = doesn't; amjroity = majority; wnat = want; makeing = making; taht = that]. A NIGHT70
post-mortem against current prompt methods (online), then NIGHT71: mostly building, a bounded and careful dead-code / moved-past slice.

## GG's four calls (AskUserQuestion, ~09:20 MST, the options GG picked, verbatim labels)
1. Disk: "Clean old worktrees (Recommended)": `git worktree remove` (no --force) on worktrees whose last commit is older than NIGHT70 with no
   tracked changes; every branch and commit kept; dirty ones listed, not removed. (Done in the forge: worktrees-removed.tsv.)
2. Room rule: "Bots guard, I judge (Recommended)": the ratio is printed on each room's card but no longer blocks it; GG gets a play build with
   a room select and a short play card. (Retires the GATE as the room verdict, NIGHT65-70's 0 / 22.)
3. Build focus (multi): "Rooms as action blocks", "Bosses and colour", "Combat feel", "Movement and camera feel".
4. Cleanup: "Dead code + old OFF rows (Recommended)": code proven unreached (the NIGHT61 auditor's rules), plus OFF rows from NIGHT49 or older
   that no open thread or DECIDE question names; one row per commit, each revertable, gates GREEN after every commit.
