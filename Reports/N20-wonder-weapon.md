# A wonder weapon for SKYGRIND: a proposal for GG (NIGHT20, builder G2; nothing built)

NIGHT20.md U3-34.8's beyond list and the additions backlog's row 19 (B4) ask for a wonder weapon. Its name and its
look are yours, so this page is a proposal. Nothing in it is in the code.

## Why the box needs one

- **CoD Zombies:** the Mystery Box's jackpot is a box-only gun with a verb no wall gun has (the Ray Gun, the
  Thundergun). In Black Ops 1 it is in the box's list 15 % of the time after round 5 and 20 % after round 10
  (`Research/2026-10-06-additions-backlog.md` 2.2; the CoD wiki's Ray Gun page). One per box move.
- **SKYGRIND's box today** (`Core/MysteryBox.cs`, `design.box`): 950 points, CoD's teddy ladder, and a uniform pick over
  every gun you don't own. Every pull is a coin flip between guns you can also buy on a wall. Nothing in it is worth
  chasing.
- **The risk** (NIGHT19's thesis, the backlog's "proven to fail" column): a jackpot gun becomes the one gun. Every
  number below is held to THE RANGE's S1 / S2 and the farm's kill share, as the other guns are.

## The base: the VORTEX, ported tonight

The web's VORTEX now fires in the port (`b108dc6`, `Core/Guns/Vortex.cs`, `G20VortexTests` 10 / 10), behind
`design.adds.b5`:
- a slow orb (14 m/s) that opens on the first body or wall it meets;
- open, it pulls every body within 7 m toward its middle for 1.5 s, then bursts (95 damage, 5 m);
- LT tapped collapses it early; LT held repels.

No other gun moves a crowd toward a point. Its row in THE RANGE (`range --gun vortex`, 10 seeds, the tree merged with
port/stage3 at `1215d54`; the score is the clear time in s + 0.1 x the HP taken, lower is better):

| C1 | C2 | C3 | C4 | C5 | C6 | C7 | C8 | C9 | C10 | C11 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 2.70 | 0.19 | 13.63 | 11.70 | 7.81 | 8.48 | 12.06 | 8.81 | 9.50 | 6.37 | 11.10 |

It clears every cell, but it is near-best (within 1.1 x the best gun) in none. Its closest are C6 (8.48 against the
STITCHER's 7.46) and C2 (0.19 against the BOUNCER's 0.02). A crowd puller with a slow cycle (2.4 s) is a support gun, not
a carry. That makes it a safe base: the wonder form adds a verb, not a damage multiplier.

## The proposal: W-1 (a working label; you name it)

| | |
| --- | --- |
| Where | **The box only**, never on a wall. One per box move: once offered, it leaves the list until the box moves. |
| Odds | Per box roll, after the teddy draw: **0 before round 5, 15 % rounds 5-9, 20 % from round 10.** A hit offers W-1; a miss is today's uniform pick. Drawn on the box's own Mulberry32, so a seed replays. |
| Verb | **The anchor.** Hold the trigger and the orb does not burst on its timer: it stays open while you hold, up to 4 s, and drifts to your crosshair at 6 m/s. Let go and it collapses. One orb at a time; a 3 s cooldown after the collapse. |
| Threat | It holds a crowd in place for you, it doesn't kill the crowd. The kill comes from your other gun, so swapping pays (S7's rotation and the PRIMED swap). |
| Counter | Proposed: the heavy kinds (CHARGER, WARDEN) and the BULWARK are not held, so W-1 never solves a boss or an elite. (The ported VORTEX follows the web: it skips only bosses, and the CHARGER's own knockback scale shortens its pull.) |
| Numbers | Proposed, in a new `design.adds.b4` block (rule 5): pull 7 m at the VORTEX's 9 m/s; burst 95 x 1.5 = 143; hold cap 4 s; drift 6 m/s; cooldown 3 s. |

### The build: three steps (a mid-size quest a player can finish in one run)

1. **Roll it.** The box offers W-1 (the odds above). A "WONDER" slab (H's announcer, F1) and its own box fanfare event.
2. **Feed it.** 30 kills of bodies it held, by any gun, within 4 s of the collapse. The core charges (5 pips on its HUD
   card). Measured on a scripted tape: about 3 rounds of normal play.
3. **Forge it.** At THE FORGE with the core full, the tier price (B3's tiers: 5,000 points for the first). The
   upgraded form: **two anchors** at once, and the collapse **launches** what survives 4 m up. Launched bodies are
   LAUNCH + SNIPE targets (x2 for the STITCHER and the KESTREL), so the combo system pays it.

### Events (port-only names, for your sounds)

`wonderOffer`, `wonderTake`, `wonderAnchor`, `wonderCollapse`, `wonderCharge` (a pip), `wonderForge`. Plus the
VORTEX's own `vortexOpen` / `vortexBurst`, and tonight's port-only `vortexCollapse` / `vortexRepel`.

### Look (yours)

The VORTEX's family colour is the web's gravity violet (`weapons.families.gravity`). Keep it off **magenta (RIVAL
only)** and off **the pale violet-white (the boundary)**. A deeper violet core with cyan rim light (the player's
colour) keeps the roles clean.

## How it is judged (the done-when if you say build it)

| Judge | Target |
| --- | --- |
| A box test over 100,000 seeded rolls | 0 before round 5; 15 % +/- 1 pt rounds 5-9; 20 % +/- 1 pt from 10; never twice in a box move |
| THE RANGE (its row, its forged row) | Best in the crowd cells (C3 swarm, C6 mixed) as a pair with any gun (its pair mode: W-1 sets up, the other gun pays off); best alone in <= 35 % of cells; S1 0 dominated and S2 hold for every other gun |
| The free farm | The owners' kill share for W-1 itself <= 45 %; the owners' median round <= 1.3 x the median one-gun farm round |
| The parity farm (`pfarm --gun`) | Within +20 % of the median gun alone (a jackpot, not a win button) |
| A scripted tape | The 3-step build finished in one run, on 2 seeds |
| The reel | A pull, an anchor drift and a collapse with a launch, each with its tell |

## Cost and order

L (two M slices): the box odds and the anchor first (S2 and the farm measured), then the build and the forged form.
It needs the VORTEX (done tonight) and B3's tiers (done, `71169c1`).

## Questions for you

1. Its name and its look.
2. Box only (CoD), or also a late wall buy at a high price?
3. Step 2's count (30 held kills): a quest most players finish, or a rare one?
