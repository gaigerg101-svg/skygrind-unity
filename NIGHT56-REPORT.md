# NIGHT56 REPORT (for GG; LEVELS FIRST, 2026-10-20 23:57 - 2026-10-21 07:0x MST)

You asked what the night should focus on: levels specifically, then gameplay, enemies and colours. I ran NIGHT56.md as the lead with
three builders (L levels, G gameplay / enemies, V colours) and one batched critic per build. **The usage limit stopped everyone at 03:09
(3 h 12 min in); it reset at 04:20, I resumed the builders where they were, and the freeze ran at 05:25-05:37.**

**Your play build is now `Builds/n56-freeze`** (tag u3-n56, commit 04bef918). verbs37 on all five levels 27 / 21 / 31 / 31 / 31 (nothing
lost), the smoke DONE all steps passed (227.8 s, nobody at the machine), golden 0 mismatches, every fingerprint EQUAL.

**Honest headline:** a fresh critic passed 2 of the 18 NIGHT56 slate entries (the pick and the room checker) and 2 more of NIGHT55's (the
room-clear beat and the THUNDERCLAP reel bug). Everything else was built and measured but missed at least one of its own lines, so it
ships OFF. The rooms' spawn change (the night's main "levels" fix) ships anyway because it is level data (see below).

## What you see in the first minute of each level now (n56-freeze vs n55-freeze)
- **The fight is in the room when you walk in.** Each combat room's first wave now spawns on spots you can see from the door (per-wave
  spawn spots, level data). The Player room meter at each room's entry frame, now: foundry a3 3 of 4 enemies on screen (nearest 5.4 m), a5 5
  of 5, b1 3 of 3; iceline g2 4 of 4 (NIGHT55: 1 alive at the same frame), g5 4 of 5 (5.8 m); kiln kb2 6 of 6, kb2c 3 of 3 (6.2 m), kb3 5 of
  5; span p3 4 of 4 (6.2 m), p5 5 of 6; spire v2 5 of 5 (6.6 m), v5 4 of 4, v6 5 of 6. Look at
  `Logs/feel/n56-r5/l5i/a156/shot-N29-L56-n52-iceline-g2-entry.png` beside `Logs/feel/n56-r1/d2off/a452/shot-N29-K52-n52-iceline-g2-entry.png`.
  The bot clears every level faster (foundry 130 -> 123 s, iceline 233 -> 220, span 95 -> 87, spire 141 -> 135; kiln the same). The cost
  (bot): it stands still more on iceline (stall 32 -> 37 %) and dies more on foundry (15 -> 21 deaths in 10 walks).
- **The room-clear beat (ON):** the last kill of a combat room holds the world 0.5 s and shows "ROOM CLEAR · UNDER PAR | 16.3 S · PAR 30 S"
  (the par from each room's data). Frame: `Logs/feel/n56-r2/c55f/a455/shot-N29-C55-n52-foundry-a3-hold.png`.
- **Two rooms changed:** span p5's jump pad moved 1 m off the walkway; iceline g8 got a lava strip across its west lane (THE ASH PIT: it
  makes the wall ride the faster way through). **Your call on g8:** the critic, judging as you would, said "There's a strip of lava along the
  wall. Nothing tells me it's a firebox. It's a random lava pit, and it keeps killing me" (the bot died 7 times in g8 in 10 walks, 0 before).
  I tried to revert it before the freeze, but it conflicted with later level edits, so it is still in (DECIDE56 D-28). To remove it: drop
  `pits[5]` from Design/levels/n52/iceline.plan.json and regenerate iceline.
- **Lamps by the doors:** wall lamps where the camera used to see a flat dark wall: flat frames in verbs37 7 -> 4 (spire's f1550 door still dark).

## Built tonight, OFF (each is a row; switch one on in your build's copy: `node Tools/night/n46-build-rows.cjs n56-freeze on n56.tokens56`)
| row | what it does | measured (bot = the walk / boss pilot, not a person) | why it is off |
|---|---|---|---|
| n56.tokens56 | enemies attack from where you can see them: out-of-view enemies wait, idle ones advance into view | bot: attacks that start on screen 45 % -> 84 % | per-enemy idling still over 20 % on every level; too few dash shots to measure the speed rule |
| n55.wyrm55 | THE KILN WYRM's pacing (encounter numbers only, every attack's own damage kept) | bot: 38/40 -> 40/40 killed, median 123 -> 87 s, p90 257 -> 178 s, deaths 68 -> 32 | deaths still 32 in 40 fights (10 the target) |
| n56.rank56 | the style rank multiplies the close-kill heal and RIDE KILL's refund: DULL x1.0, COOL x1.2, BOLD x1.4, AIRBORNE x1.6, SAVAGE x1.8, SKYGRIND x2.0 (**your call: the multipliers**) | Player reel: the heal at COOL 12 HP vs 10 | it does not change how often you leave DULL (it pays, it does not lift the rank); the refund pays 0 (a kill already refills the bar); one of its tests fails in the Editor |
| n55.kill55 | kills in a chain freeze longer (kills 3+ hold 0.083 s), shards stay, hit marks on enemies | Player reel: the chain ladder works; a blind judge picked ON as "the kill hits harder" in both orders | the hit marks barely show (0.04 % of the body); the camera jitter with it on 0.230 (0.191 the limit) |
| n56.colour56 | each level's own colour (a key frieze band + an accent), never a role colour | blind judge picked ON as "more colour direction" in both orders, foundry and kiln | the world's brightest colours still out-saturate the danger / parry tells on 28 of 36 frames |
| n55.grade55 | boss palettes: THE WICK cool violet shadows + a hot wall that reads as a state; THE WYRM amber-orange turns; THE CROWN | THE WICK and its burning wall pass; THE WYRM's turn lands at 22 deg | THE CROWN's grade hides a third of its ring tells (0.68 kept); THE WICK's ride panels show no cyan cue |
| n56.rim56 / n56.yellow56 | enemies the brightest thing; yellow only on parry things | rim: 1 of 30 enemies brighter than the world; yellow: spire still up to 24 % non-parry yellow | both missed their numbers |
| n55.wick55 | THE WICK as a punish-the-window fight | bot: damage in the windows 6 -> 57 %, median 46 -> 58 s, 7 deaths in 40 | the Player proof's pilot cannot climb the level's stack (0 core punches) |
| n55.brief55 | lesson lines as short sentences with your button | every line passes | a card overlaps the room counter by 2.4 px for 1 frame on kiln / spire |

## Your calls
- **Each level's colour key** (proposals, the row is off; GG's eye decides): foundry oxide green #6b8c73 with a temper-violet accent #a267e6;
  iceline ultramarine #385194 / pine green #86d9a2; kiln rose clay #944a5c / cobalt #5a82e0 (kept warm for the worm by saturation, not by a
  new hue); span indigo-violet #654a94 / signal lime #8dd957; spire absinthe #688c3f / rose #e67793 (Research/2026-10-20-wave56/V.md).
- **The showcase room** (a room rebuilt from scratch): not built. Its rule was "only if spawns-in-view passes", and that failed on the bot's
  stall line, which the old rooms already failed before tonight (26-32 % against a 25 % line).
- rank56's multipliers (above); g8's lava strip (above); THE CROWN's palette (two tries failed); whether THE WYRM's remaining deaths are OK.

## What is SHORT and why (the slate: Logs/n56/slate.json; NIGHT55's: Logs/n55/slate.json field n56)
- Levels: the room checker's stricter reading (13 of 18 rooms have no safe foothold by the door) drops "in view" to 4 of 18 under its own
  rule; the bot stall line; 5 rooms still fail the identity checks (a3 mirror, g3 / g4 / v2 ride shortcut, kb2 pad) and need re-cuts, not
  added pieces.
- Not started: boss text placement, FOV with speed, ceiling fixtures, edge washes, the boss-kill finish, the boss brief card, speed kills
  (they wait behind every earlier entry, and the usage-limit pause took 71 minutes).
- The smoke's last proof: a Player run that shows the game pausing when the window loses focus outside the smoke.

## The forge experiment (NIGHT56.md section 8)
At most three builders and one batched critic per build: **entries gated / attempted 13 / 14** for NIGHT56's slate (the 4 not attempted
were conditional or after-everything leftovers), and 17 of NIGHT55's 26 judged by a critic tonight (the other 9: never started, or
decided earlier); **the first gate at 1 h 26 min** (01:23) on 11 entries
(NIGHT55: 1 of 26, first gate at 1 h 10 min on 1 entry). The usage limit still hit, at 3 h 12 min (NIGHT55: 2 h 28 min), but two gates had
landed before it and every slice was committed. Five builds; critics on r1, r2 and r5 (r5 split in two: more than 30 images).

## Files
NIGHT56-LOG.md (the run and the close table), DECIDE56.md (D-1..D-30, untracked), the gates `Research/2026-10-20-wave56/gate-r1.md`,
`gate-r2.md`, `gate-r5a.md`, `gate-r5b.md`, the builders' notes `L.md`, `G.md`, `V.md`, tools `Tools/night/n56-*.{sh,cjs,py,ps1}`.
