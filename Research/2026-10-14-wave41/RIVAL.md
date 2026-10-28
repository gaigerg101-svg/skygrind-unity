# THE RIVAL: a one-page proposal for level 6's boss (NIGHT41 builder L6; no code)

GG owns enemy design: this page is for GG to react to, and nothing below gets built until he does (DECIDE41 D-3). The arena it fights in is
reserved empty in the greybox: room `mx` 6.9 THE MIRROR, 34 x 34 x 14 (Design/levels/n41/finale.beats.md).

**GG's words it starts from:** "THE RIVAL: a magenta V2-style mirror that rides, grapples and parries like you" (DECIDE37 D-1, 2026-10-12); "a
V2-style difficulty spike" (OWNER.md); bosses "one to two minutes, maybe three if you're slow" (2026-10-13); "an understanding of parry
mechanics"; bosses where "you just teleport into these random rooms ... hard to understand what to do" (2026-10-12, a complaint).
ULTRAKILL's V2 (as remembered, not fetched tonight) is a machine like the player: it slides, dashes, swaps guns, keeps its distance, and runs
when hurt. What makes it work is that you know every move it makes, because they are your own moves.

## The idea in one line
THE RIVAL is the only enemy that moves like you, and the only way to beat it is to use your own verbs better than it does. Every attack it
makes is one of your verbs, so every attack is one you already know how to answer.

## Body and look
- About the player's height: a lean blocky biped in the web's enemy style (CLAUDE.md rule 7: the blocky models), magenta and nothing else
  magenta in the game (PRINCIPLES 3). A cyan-free body: its usable things are never cyan.
- Its tells are YELLOW where they can be parried (as every parryable attack since tells36); its non-parryable moves have a red-orange tell.

## What it does (each one a player verb; the answer is a player verb)
| its move | the tell (one frame shows it) | the answer (VERBS row) |
|---|---|---|
| rides the pinwheel's panels and jumps off at you (row 9 / 11) | it plants on a panel, magenta trail | shoot it off the wall with the KESTREL's charge (27): a hit while it rides knocks it down, staggered |
| fires a YELLOW shot from range (its KESTREL) | the yellow flash at its hand, 0.6 s | parry it back (40): the returned shot staggers it |
| hooks the centre block's nodes to close in (row 17) | its rope, magenta | whip it out of the pull (20): the latch drags it to you and starts your bend (22) |
| a THUNDERCLAP slug at close range | a red-orange wind-up, 0.8 s, not parryable | dash through or aside (5), then punch its back |
| boosts onto the centre block (3.6 m) to shoot down | it jumps, fires down | boost up after it (29) or hook a node (17): the high ground is never only its |

## The rule that makes it readable: STAGGER, then HURT
Damage hurts it only while it is STAGGERED (DECIDE39 D-1: stagger before health). The four answers above each stagger it for about 2 s, and
during that time any weapon hurts it. So the fight is "answer its move, then punish": the same rhythm as THE ICEBREAKER's haymaker and
punish, but with your own verbs. A bar under its name fills with each answer and empties as it recovers. Camping does not work, because the
only thing that staggers it is answering one of its moves.

## Two phases (one to two minutes for a person: GG's band; the bot band about half, 30-60 s)
1. **Mirror** (100-50 %): one move at a time, with a pause between each, so you learn them in order: the ride, the shot, the hook.
2. **Rival** (under 50 %): it chains two moves (a ride into a jump-off slug, a hook into a shot) and it parries YOUR shots back once every
   8 s, yellow. You parry that one back too: a parry rally. That is the fight's peak, the "understanding of parry mechanics".
At 0 it does not explode. It kneels, magenta fading, and the exit opens. No cutscene and no teleport: the arena is the room you walked into.

## The arena (6.9 THE MIRROR, built only after GG's yes)
A square that looks the same from every side, because the fight is a mirror: four 10 m ride panels in a pinwheel, 6 m in from each rim
wall; a 6 x 6 x 3.6 m centre block (boost height, `noride`); two grapple nodes 9 m above its corners; no cover that blocks a hook. Every
surface THE RIVAL uses, you can use too. Room light warm; nothing magenta but THE RIVAL.

## What GG decides
1. Is this THE RIVAL (yes / change / no)? 2. Stagger first (the default) or plain health? 3. Its parry rally in phase 2: in or out?
4. One fight, or does it also appear in levels 1-5 first (a magenta figure that leaves before you reach it; view only, no fight)?
Not proposed: new player verbs, audio (GG's), a third phase.
