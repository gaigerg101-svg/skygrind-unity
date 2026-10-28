# NIGHT35.md: DRAFT until GG has played n34-r4 (lead + builders as needed, about 4 hours)

Base: port/stage3 at the NIGHT34 close (tag u3-n34). Read CLAUDE.md, HANDOFF.md, NIGHT34-REPORT.md, NIGHT34-PROOF.md, FEEL-PLAYTEST.md first.
t0 = the moment GG says go. Box 4:00, hard cap 4:30.

## Step 0 (before anything): GG's words on n34-r4
Store GG's reaction verbatim (Research/<date>-wave35/GG-REQUEST.md), map each sentence to a NIGHT34 row (bend34 / split, bendLook, rail34,
nomark34, lines34, calm34, flow34, reach34, air34, combo34), apply his DECIDE34 answers (N-1..N-11, R-1) first, and plan only from them. If GG
has not played yet, run the items below in this order.

## Carried items from NIGHT34 (each Player-proven or explicitly SHORT)
1. **The bend in boss fights (N-11):** THE STAG died in 10.1 s in the Player with the bend (17.6 s without; band 15-22 s). Measure every boss
   with the bot using the bend, then apply GG's choice (leave it, a 0.5 world share in boss rooms, or only the short bend there).
2. **Levels 4-5 movement lines (N-9):** n34-span and n34-spire with nodes, ride walls and lanes; span with nodes walked 8/10 under NIGHT34's
   pilot rule, so start from Walk29Pilot's SwingAlso34 list and keep the walk 10/10.
3. **The six short rooms:** f5, f5b, f9 (lava-lesson lanes 13-15 m), f7 (the slide corridor, one floor), i2 / i5 (no nodes, scripted lessons).
4. **The bend's length in game seconds in the Player** (the reels measure wall time while writing frames): add a sim-clock length to the
   bend34 reel line, and give the reel a window longer than a 5 s extended bend (n34-r4's reel ended inside one: "ends 0").
5. **Leftovers not built:** the held-bumper weapon wheel that bends while held (Doom Eternal / ULTRAKILL; research/swap-hud.md).
6. **Config hygiene:** builders' config scripts rewrote the whole design.n34 block and conflicted at every cherry-pick; next night each builder
   patches only its own row's lines.

## Rules (unchanged from NIGHT34)
Every sim change behind a design.n35 row (tape format 18), fingerprint EQUAL with the rows off, golden + trajectories at every Core commit,
Mover.cs only in a named row's branches; a visual row is DONE only with a RED / GREEN Player frame pair the lead looked at; caps: builds 8, reel
invocations 20 (one combined RED per round), suite 2; builders in worktrees, lead alone on the Editor, builds, reels and git; commit every slice;
nothing deleted without GG.

## Lessons from NIGHT34 to apply
- A reel that needs a campaign level must boot one (`-skygrindMap <id>`): one reel was wasted on SURVIVAL.
- Grapple nodes change which bot pilot drives a room: walk every level after adding nodes.
- The bend changes boss pacing: any new verb that changes time needs the boss pilots re-run with the verb in use.
