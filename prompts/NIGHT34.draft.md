# NIGHT34.md: the playtest-answer night (lead + builders as needed, about 4 hours) — DRAFT until GG has played n33-r7

Base: port/stage3 at the NIGHT33 close (tag u3-n33). Read CLAUDE.md, HANDOFF.md, NIGHT33-REPORT.md, NIGHT33-PROOF.md, FEEL-PLAYTEST.md first.
t0 = the moment GG says go. Box 4:00, hard cap 4:30.

## Step 0 (before anything): GG's words on n33-r7
NIGHT33 shipped twelve rows ON with nobody having played them. Store GG's reaction verbatim (Research/<date>-wave34/GG-REQUEST.md), map each
sentence to a row (hud33, look33, kit33, arenas33, devices33, ai33, spawns33, bosses33, view33, whip33, pierce33, shop33), and plan only from
it. If GG has not played yet, run the items below in this order.

## Carried items from NIGHT33 (each one Player-proven or explicitly SHORT)
1. **Shop pad path in a fresh Player** (b8655b3c hardened the reel; the reel cap stopped the re-run): the shop33 reel alone on n33-foundry
   must PASS the pad steps; then decide a project-wide Input System "run in background" setting (it changes the shipped game: GG's call).
2. **The vent's new look** (4044e074: red-orange, translucent, framed from 4 m): the arenas33 reel f8 tell / burst frames.
3. **THE LOOM and THE CROWN Player kill times** with bosses33 + pierce33 + whip33 on (harness 19.3 / 18.1 s); band 15-22 s bot.
4. **Lava rooms' orange share** (57-63 % of saturated pixels by L's hue metric while the frames read dark): fix the metric to weight by
   brightness, or dim the braziers in lava rooms, then re-read f2 / f9b / f10.
5. **spawns33:** the 0.6 s hold-still tell is off (`tellStagger: false`: bot death loops); s3b's nearest spawn is 5.3 m (no far-side spot);
   n33 v6 has ~0.87 walkers pressing the lava edge per step (ai33 guard pushbacks 6245): R's catwalk route vs E's mask.
6. **The feel33 reel** passes alone but its ZOOM / PIERCE went RED after hud33 in one Player (focus loss silences the virtual pad): copy P's
   reel hardening (ignore focus, re-enable the pad before presses, clean front).
7. **s6** keeps 19 bot deaths (the deck spitters' acid on the entry corner: combat, not shape).

## Rules (unchanged from NIGHT33)
Every sim change behind a design.n34 row (tape format 17), fingerprint EQUAL with the rows off, golden + trajectories at every Core commit, never
Mover.cs; a visual row is DONE only with a RED / GREEN Player frame pair the lead looked at; caps: builds 8, reel invocations 20 (combine
scenarios per Player only when each scenario resets its own input / flow state), suite 2; builders in worktrees, lead alone on the Editor,
builds, reels and git; commit every slice; nothing deleted without GG.

## Lessons from NIGHT33 to apply
- The Editor never loads a campaign level in Play: the n33-r4 boot hang (Kit33Props recursion) only showed in the Player. After any level or
  view change, the first Player reel boots the changed level.
- A walk bot over every level after any spawn / encounter change: spawns33 soft-locked two levels while its own harness was GREEN.
- One Player per scenario that drives input, or harden the scenario: the shared-Player hudshop run lost the virtual pad.
