# COPY47: every player-facing line, before and after (NIGHT47, builder F; G15 copy47 and G5's fight lines)

Owner tonight: builder F (the lead's round-4 hand-off). The source of truth for F's lines is `design.n47.copy47.numbers.map` in
`Design/unity-config.json` (written by `Tools/night/n47-f-rows.cjs`; `Game/Copy47.cs` applies it with the row on, off = NIGHT46's text exactly).
`Tools/night/n47-copy-check.cjs` checks each old line is found verbatim in a config, a level file or a C# string, and that no new line drops a
`{placeholder}`: 75 lines, 0 misses (run 2026-10-19 ~03:10). The other builders' lines (W, B, L) are their own; this file lists them against
the same rules and the fix for each one that breaks a rule.

## 1. The rules

From NIGHT47.md section 3, F-cards section 3 and XAG 101 (Research/2026-10-18-wave47/F.md R6):

- **Words:** a teach / lesson line at most 10 words; a **fight line** (a boss line while its bar is up, a banner, a boss hint) at most **6 words**.
  A `{glyph}` counts as one word (it is drawn as one).
- **The glyph shown:** a line that tells the player to press something names the button with its `{action}` placeholder (PromptText fills in
  the live pad glyph or key). Moves and aims with no single button need none ("Get up high.").
- **Sentence case:** lines in sentence case; a 1-2 word label may stay in capitals; weapon and boss NAMES stay in capitals (they are names:
  KESTREL, THUNDERCLAP, THE WICK).
- **Plain:** second person, a verb first where it fits, no internal names or jargon, it sounds like a person talking.

**How it draws (found tonight):** the HUD's labels force capitals (`UiStyle33.Label`'s caps: `FontStyles.UpperCase`), so a sentence-case line
read in capitals anyway on the teach card's head and the fight line (n47-r1e: the teach card showed "IN THE AIR, LOOK STRAIGHT DOWN"). With
copy47 on, `Ui.Case47` (UiHud47.cs) drops the forced capitals on a label showing a line of 3+ words (the card's head, the fight line); a 1-2
word label keeps them; a line typed in capitals still shows in capitals. **So every line below must be typed in sentence case to read in it.**

## 2. F's lines (copy47's map: before -> after)

Columns: before (verbatim, as the config / level / C# had it), after (copy47 on), words (a glyph = 1), the glyphs kept, the check. The two GG
named are the lead's picks (the gauntlet pickup variant A, the game start variant B); their first sentence is the card's head (a name: caps).

| before | after | words | glyphs | check |
|---|---|---|---|---|
| PRESS {fire} to PUNCH. {altFire} to PARRY. | THE PULSE GAUNTLET. Punch with {fire}. {altFire} parries. Five kills: it blasts. | 12 | {altFire} {fire} | ok (head + rule: the rule 9 words; the head is a name) |
| MOVE. LOOK. {jump} JUMP. | THE FOUNDRY. Stay fast. {jump} to jump, again in the air. | 11 | {jump} | ok (head + rule: the rule 9 words; the head is a name) |
| PRESS {dash} to DASH. Works in the air. | {dash} to dash. It works in the air too. | 9 | {dash} | ok |
| HOLD {slide} to SLIDE. You keep your speed. | Hold {slide} to slide. You keep all your speed. | 9 | {slide} | ok |
| PRESS {fire} to FIRE the KESTREL. | Shoot the KESTREL with {fire}. | 5 | {fire} | ok |
| HOLD {fire} to CHARGE. Release to fire. | Hold {fire} to charge, let go to fire. It pierces. | 10 | {fire} | ok |
| HOLD {fire} to CHARGE. The shot pierces. | Hold {fire} to charge, let go to fire. It pierces. | 10 | {fire} | ok |
| HOLD {altFire} to ZOOM. Aim on a wall slows time. | Hold {altFire} to zoom. On a wall, time slows. | 9 | {altFire} | ok |
| HOLD {jump} on a WALL to WALL RIDE. Let go to leap. | Hold {jump} on a wall to ride. Release to leap. | 10 | {jump} | ok (fixed tonight: was 12 words) |
| PRESS {jump} at a WALL to WALL RIDE. | Hold {jump} on a wall to ride it. | 8 | {jump} | ok |
| PRESS {jump} on a WALL to WALL JUMP. Chain them. | {jump} off a wall to wall jump. Chain them up. | 10 | {jump} | ok |
| HOLD {fire} to CHARGE a SLUG. Release to fire. | Hold {fire} for a slug. It cracks armour. | 8 | {fire} | ok |
| HOLD {fire} to CHARGE a SLUG. It cracks armour. | Hold {fire} for a slug. It cracks armour. | 8 | {fire} | ok |
| PRESS {grapple} to GRAPPLE. It pulls you to the hit. | {grapple} to grapple. It pulls you where you aim. | 9 | {grapple} | ok |
| TAP {grapple} to PULL. HOLD to SWING. | Tap {grapple} to pull. Hold it to swing. | 8 | {grapple} | ok |
| LATCH with {grapple} to BEND time. SWAP guns in it. | Hook an enemy with {grapple}: time slows. Swap guns then. | 10 | {grapple} | ok |
| PRESS {dash} then {jump} to JUMP DASH. | {dash}, then {jump}: a jump dash. Go far. | 8 | {dash} {jump} | ok |
| AIM {grapple} at an enemy to WHIP. Then PUNCH. | {grapple} an enemy to yank it close. Then punch it. | 10 | {grapple} | ok |
| THE THUNDERCLAP. PRESS {fire} to BLAST. | THE THUNDERCLAP. {fire} to fire. Shoot down to fly. | 9 | {fire} | ok (the head is a name) |
| THE STITCHER. HOLD {fire} to STREAM nails. | THE STITCHER. Hold {fire} to pour nails. | 7 | {fire} | ok (the head is a name) |
| THE TORCH. HOLD {fire} to BURN. | THE TORCH. Hold {fire} to burn them. | 7 | {fire} | ok (the head is a name) |
| STEP 1 · PRESS {jump} TO JUMP | {jump} to jump. | 3 | {jump} | ok |
| STEP 1 · IN THE AIR, LOOK STRAIGHT DOWN | In the air, look straight down. | 6 | - | ok |
| STEP 1 · PRESS {fire}: THE BLAST LIFTS YOU | {fire}: the blast lifts you. | 5 | {fire} | ok |
| STEP 2 · {jump} AND {fire} TOGETHER: HIGHER | {jump} and {fire} together go higher. | 6 | {fire} {jump} | ok |
| STEP 3 · AT THE TOP, PRESS {fire} AGAIN | At the top, {fire} again. | 5 | {fire} | ok |
| THE BOOST: JUMP, LOOK DOWN, FIRE, FIRE AGAIN | THE BOOST: jump, look down, fire, fire again. | 8 | - | ok (the head is a name) |
| THE FLUE · {fire}, {jump} AT THE TOP, {fire} AGAIN | THE FLUE: {fire}, {jump} at the top, {fire} again. | 9 | {fire} {fire} {jump} | ok (the head is a name) |
| HOLD {fire} ON THE GROUND: CHARGE A SLUG | On the ground, hold {fire} for a slug. | 8 | {fire} | ok |
| IT HUNTS THE FLOOR: JUMP, BOOST, OR A PLATE | Something hunts the floor. Get up high. | 7 | - | ok |
| HIT IT AS IT SURFACES: IT REELS | Hit it as it surfaces. | 5 | - | ok |
| HOLD {slide}: SLIDE UNDER | Hold {slide} to slide under. | 5 | {slide} | ok |
| HOLD {jump} ON A WALL: RIDE | Hold {jump} on a wall to ride it. | 8 | {jump} | ok |
| PUNCH {punch} THE YELLOW GLOB BACK | Yellow means punch it. {punch} sends it back. | 8 | {punch} | ok |
| HOLD {jump} ON THE WALL | Hold {jump} on the wall. | 5 | {jump} | ok |
| {jump} WALL TO WALL. {dash} ACROSS | {jump} wall to wall, then {dash} across. | 7 | {dash} {jump} | ok |
| LOOK UP. {jump} WALL TO WALL | Look up. {jump} from wall to wall. | 7 | {jump} | ok |
| HOLD {altFire}: SLOW. {fire}: PIERCE | Hold {altFire} to slow time. {fire} pierces. | 7 | {altFire} {fire} | ok |
| {fire}: ONE SWING FELLS A TANK | {fire}: one gauntlet swing drops a tank. | 7 | {fire} | ok |
| {fire} THE SHIELD, OR FLANK IT | Punch the shield with {fire}, or get behind it. | 9 | {fire} | ok |
| FIVE KILLS: {altFire} BLASTS THE PACK | Five kills charge it. {altFire} blasts the pack. | 8 | {altFire} | ok |
| {fire} AS THE YELLOW ORB ARRIVES | Punch the yellow orb back with {fire}. | 7 | {fire} | ok |
| SAVE {altFire} FOR THE SECOND PACK | Save {altFire} for the second pack. | 6 | {altFire} | ok |
| KESTREL BACK: {fire} THE FAR ONES | KESTREL back: {fire} the far ones. | 6 | {fire} | ok |
| {dash} INTO IT. {fire} THE GAUNTLET. | {dash} in, then {fire} the gauntlet. | 6 | {dash} {fire} | ok |
| {quickSwap}: KESTREL FAR, GLOVE CLOSE | {quickSwap}: KESTREL far, GAUNTLET close. | 5 | {quickSwap} | ok |
| {dash} ASIDE: IT HITS THE WALL | {dash} aside and it hits the wall. | 7 | {dash} | ok |
| {dash} ASIDE, THEN {fire} ITS BACK | {dash} aside, then {fire} its back. | 6 | {dash} {fire} | ok |
| THREE STEPS, EACH ONE HIGHER | Three steps, each one higher. | 5 | - | ok |
| UP ALL THREE, THEN DROP DOWN | Up all three, then drop. | 5 | - | ok |
| JUMP. SHOOT DOWN. JUMP. SHOOT DOWN. | Jump, shoot down, again. | 4 | - | ok |
| JUMP IN. SHOOT {fire} DOWN. FLY. | Jump in, {fire} down, fly. | 5 | {fire} | ok |
| CLIMB IT ON THE RUN. | Climb it at a run. | 5 | - | ok |
| {grapple} PULL TO THE RING. | {grapple} to pull to the ring. | 6 | {grapple} | ok |
| LOOK UP. {grapple} PULL. | Look up. {grapple} to pull. | 5 | {grapple} | ok |
| KILLS GIVE PULLS. | Kills give you more pulls. | 5 | - | ok |
| TWO CHARGES. TWO PULLS. | Two charges: two pulls. | 4 | - | ok |
| {grapple} AT AN ENEMY: WHIP. | {grapple} an enemy to yank it. | 6 | {grapple} | ok |
| {grapple} PULL. RIDE. PULL. | {grapple}: pull, ride, pull. | 4 | {grapple} | ok |
| {grapple} HOOK KNEE. {grapple} PULL AWAY. | {grapple} a lit knee, then pull away. | 7 | {grapple} | ok (a room lesson: <= 10) |
| THE TORCH: HOLD {fire} CLOSE | THE TORCH: hold {fire} up close. | 6 | {fire} | ok (the head is a name) |
| {dash} DASH, THEN {jump}: FLY | {dash}, then {jump}: fly. | 4 | {dash} {jump} | ok |
| {dash} + {jump}: THROUGH THE RING | {dash} + {jump} through the ring. | 6 | {dash} {jump} | ok |

**Boss lines (fight lines: <= 6 words):**

| before | after | words | glyphs | check |
|---|---|---|---|---|
| GET IN CLOSE: {fire} THE GAUNTLET HITS FULL | Get close: {fire} hits hard. | 5 | {fire} | ok (fixed tonight: was 7) |
| TOO FAR: SHOTS BARELY SCRATCH IT | Too far. Your shots barely scratch. | 6 | - | ok |
| ITS FIST IS STUCK: HIT IT NOW | Its fist is stuck. Hit it! | 6 | - | ok |
| YELLOW BLOCK: {punch} PUNCH IT BACK | Yellow block. {punch} it back. | 5 | {punch} | ok |
| PUNCH THE GLOWING HEAD. | {punch} the glowing head. | 4 | {punch} | ok (fixed tonight: the glyph added) |
| HOOK A LIT KNEE {grapple}. THEN {grapple} PULL AWAY. | {grapple} a lit knee. Pull away. | 6 | {grapple} | ok (fixed tonight: was 7) |
| HOOKED. NOW {grapple} PULL AWAY TO A THREAD. | Hooked! {grapple} away to tear it. | 6 | {grapple} | ok (fixed tonight: was 8) |
| OPEN: SHOOT THE CORE. EVERY GUN COUNTS. | It's open. Shoot the core! | 5 | - | ok (any gun: no one button) |
| FLY THROUGH THE RING: {dash} DASH + {jump} JUMP | {dash} + {jump} through the ring. | 6 | {dash} {jump} | ok |
| STAGGERED: EVERY HIT COUNTS | Stunned! Every hit counts. | 4 | - | ok |
| MOVE (C#: the message slot's danger word) | Keep moving! | 2 | - | ok |
| EXPOSED (C#: the message slot's danger word) | Standing still! Move. | 3 | - | ok |
| PUNCH THE CORE! (C#: UnityTuning.Hud47CoreLine, W's G7 hand-off) | {punch} the core! | 3 | {punch} | ok (fixed tonight: sentence case, the glyph; the fight line resolves it) |
| (new, R4 finale47) | <NAME> DOWN (the boss's name + "down", a title) | 2-3 | - | ok (a title: a name, caps) |

## 3. The other builders' fight lines (checked against the same rules; the fixes for the lead to pass on)

### W: THE WICK (W-cards.md section 1, "The fight's words")

| moment | W's line | words | check | fix |
|---|---|---|---|---|
| the walk-in reveal | THE WICK | 2 | ok (a name) | - |
| the reveal hint | PUNCH THE YELLOW EMBERS BACK | 5 | caps; no glyph | `{punch} the yellow embers back.` |
| the first crack (once) | SHOOT THROUGH THE CRACK | 4 | caps; no glyph | `{fire} through the crack.` |
| P1 -> P2 | IT'S CLIMBING. GET UP THERE. | 5 | caps | `It's climbing. Get up there.` |
| P2's first panel (once) | HOLD {jump} TO RIDE. LOOK UP. | 6 | caps | `Hold {jump} to ride. Look up.` |
| a fall to a catch ledge (once) | CAUGHT. KEEP CLIMBING. | 3 | caps | `Caught! Keep climbing.` |
| P2 -> P3 | NO WAY DOWN. FINISH IT. | 5 | caps | `No way down. Finish it.` |
| the first crack-open | PUNCH THE CORE | 3 | caps; no glyph | F's line now: `{punch} the core!` (UnityTuning.Hud47CoreLine); W need not post its own |

### B: the worm and THE ICEBREAKER (B's round-2 report, as the lead passed them)

| boss | B's line | words | check | fix |
|---|---|---|---|---|
| worm | Punch its head as it comes. | 6 | no glyph | `{punch} its head as it comes.` |
| worm | It's dazed. Blast it now. | 5 | ok (any gun) | - |
| worm | Get off the glowing floor. | 5 | ok | - |
| worm | It's going under. | 3 | ok | - |
| worm | Nowhere left to stand. | 4 | ok | - |
| ICEBREAKER | Punch it to crack the ice. | 6 | no glyph | `{punch} it to crack the ice.` |
| ICEBREAKER | It's open. Hit it hard. | 5 | ok | - |
| ICEBREAKER | It's breaking the ice. | 4 | ok | - |
| ICEBREAKER | The herd is coming. | 4 | ok | - |
| ICEBREAKER | Ram a charger into it. | 5 | "ram" is not a verb the player has (VERBS: the punch / the blast launch a body) | `{punch} a charger into it.` |

THE ICEBREAKER's lines seen in n47-r1e (Logs/n47/r1e-b.jpg, the fight line: "PARRY THE BLOCK. PUNISH STUCK FISTS", "{punch} THE BLOCK BACK",
"{dash} ASIDE, THEN HIT IT", "{dash} OUT OF THE CIRCLE", "{jump} OVER THE SWEEP") are typed in capitals: they read in capitals even with
Case47. Fix: sentence case (`Parry the block. Punish stuck fists.` 6; `{punch} the block back.`; `{dash} aside, then hit it.`; `{dash} out of
the circle.`; `{jump} over the sweep.`), or B's new lines above replace them (not checked which are live tonight).

### L: THE LOOM, THE CROWN, the level-5 lines (L-cards.md)

| where | L's line | words | check | fix |
|---|---|---|---|---|
| LOOM P1 -> P2 banner | IT CUT ITS FRAME. | 4 | caps | `It cut its frame.` |
| LOOM P2 -> P3 banner | IT'S FALLING. FINISH IT. | 4 | caps | `It's falling. Finish it.` |
| CROWN P1 -> P2 banner | DOWN THE SHAFT. | 3 | caps | `Down the shaft.` |
| CROWN banner | BREAK THE CROWN. | 3 | caps (THE CROWN is a name) | `Break THE CROWN.` |
| level-5 start | THE SPIRE. FOLLOW THE LIGHT DOWN. | 6 | caps (THE SPIRE is a name) | `THE SPIRE. Follow the light down.` |
| v9 FALLING STONES lesson (F's line for L) | THE STONES FALL. KEEP MOVING. | 5 | caps | `The stones fall. Keep moving.` |
| room banners | 4.10 THE NARROWS, 5.11 THE SPIRE | 2-3 | ok (labels; copy47's Title drops the number: "THE NARROWS") | - |

## 4. Open (not checked tonight)

- Which of THE ICEBREAKER's two line sets is live on n47-iceline (r1e's capitals or B's report's sentence case).
- The glyph each `{action}` resolves to on the live pad layout v4 (RB = the grapple): read in PromptText, not checked on a frame for every line.
- The fight line's word cap is checked here by count; `n47-copy-check.cjs` caps F's lines at 12 words and does not know which are fight lines.
