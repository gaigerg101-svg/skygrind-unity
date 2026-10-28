# Where SKYGRIND is missing things (gap analysis, 2026-10-21, after NIGHT56; GG cannot play now)

GG (2026-10-21): "i cant play it now so i need you ti figure out, study game deisgn check doucments and see where we are missing, use
skills, look at github". Written by the lead with the skygrind-idea-lab skill run as a gap analysis. Our data: docs/PRINCIPLES.md,
docs/DESIGN-RULES.md, docs/ROOMS.md, docs/VERBS.md, docs/memory/ (CORE, OWNER, FEATURES, THREADS, ledgers w54-w56, IDEAS),
NIGHT56-REPORT.md, Design/levels/n52/*.plan.json. Outside evidence: research/design.md (game design, URLs) and research/github.md
(open source, licences). Every number below was read from those files or computed from the level plans; "not checked" where it was not.

## 1. Our data: what the game is, measured

### 1.1 The campaign is short, and it gets thinner as it goes
Combat rooms, waves and enemies per level, counted from `Design/levels/n52/*.plan.json` (`kind: "room"`, `waves`); bot clear times from
ledger/wave-56.md (the walk bot, not a person).

| level | combat rooms | enemies | enemy kinds | bot clear |
|---|---|---|---|---|
| 1 THE FOUNDRY | 6 (a2 a3 a4 a5 b1 a8) | 35 | 3: husk, swarmer, spitter | 122.6 s |
| 2 THE ICE LINE | 8 (g1-g8) | 67 | 7: tank, charger, warden, husk, swarmer, drone, spitter | 220.3 s |
| 3 THE KILN | 3 (kb2 kb2c kb3) | 33 | 6: + burrower | 92.2 s |
| 4 THE SPAN | 3 (p1 p3 p5) | 41 | 3: husk, swarmer, spitter (level 1's roster) | 86.8 s |
| 5 THE SPIRE | 3 (v2 v5 v6) | 38 | 5: + heavy | 135.1 s |

- 23 combat rooms in the whole campaign; levels 3-5 have three each, padded by links. The late game is shorter than level 2.
- The roster peaks at level 2 (7 kinds) and falls back to level 1's three at level 4. DESIGN-RULES R-2 (introduce -> reinforce ->
  combine -> escalate -> master) and P-2 ("the player gets stronger as the rooms get richer") are broken by the data, not by taste.
- Level 2 introduces four roles (tank, charger, warden, drone) against R-3's "at most one new role per room".

### 1.2 Almost nothing built recently reaches the game
- FEATURES.md: every row from w29 to w56 has GG's verdict "not played" (except the few he reacted to in OWNER.md).
- w55: 1 of 26 entries gated. w56: about 12 rows built, 1 shipped ON (clear55). The OFF shelf spans w46-w56 (THREADS T-77, T-85-T-87,
  T-90, T-103-T-105, T-135).
- The rule that keeps them off: a row ships only if every line of its critic passes; a 2.4 px overlap holds back a whole feature.

### 1.3 No human judge since 2026-10-20 ~14:05
- The last GG reaction (OWNER.md 2026-10-20 ~14:05) predates n54, n55, n56.
- The room numbers are the walk bot's (T-118); the "GG" verdicts since are critics role-playing him.
- The judges were calibrated against control pairs in w50 (NIGHT50.md P0), never against GG's real verdicts.

### 1.4 The rooms are patched, not designed
- 5 rooms fail the identity checks (a3, g3, g4, v2, kb2; T-127); 13 of 18 have no foothold by the door (T-134).
- w56 added a lava strip in g8 that the critic, as GG, called "a random lava pit" (T-136): GG's own named dislike (w35).
- docs/ROOMS.md is a 1250-line changelog of five generations of the same rooms (n42, n43, n45, n46, n47, n52); no single table says
  what ships now, which R-6 ("the room table is the memory") needs.

### 1.5 Breadth over depth
- Modes and systems carried since w20-w21 and unmeasured since w21 (T-SV): THE GRIND, WEEKLY, challenge codes, the BEACON, the HUNT,
  round twists, the FORGE, the CODEX, the shop. The campaign pivot (GG w24: "a LINEAR boomer shooter ... focuses on the boss fights")
  left them in place.
- docs/VERBS.md: 40+ verb rows; 31 performed by level 5. GG (w41): "way too much instruction at the start of the level".

### 1.6 Crunch and colour: built, not shipped
- Kill feel: kill55 OFF (hit marks 0.04 % of the body), feel46's kicks OFF (blind "same"), guns48 OFF. GG has asked since w16.
- Colour: colour56 OFF, THE CROWN's palette failed twice, look49 OFF, five colour keys waiting on GG (V.md).

### 1.7 The tools are fraying
- The live-Editor suite has not finished Game M-Z since 2026-10-14; on 2026-10-21 the Game M batch wedged the Editor's Pipeline.
- The usage limit has cut three nights in a row at about 3 h (L-003 / L-098).

## 2. Outside evidence (research/design.md, research/github.md; URLs there)

The research's top lessons, each against what SKYGRIND already has (grep of Assets/Scripts/Core and the ledgers):

| lesson (source) | SKYGRIND has it? |
|---|---|
| Every element needed by an enemy placement or the scored route, or cut (Neon White, Game Informer) | the rule (A-1, A-9) and checkers (n53-identity, n56-room); 5 rooms fail; g8's strip added against it |
| Standing still loses: delayed tracking attacks, aim worse with speed, an attack-token cap (ULTRAKILL Virtue; DOOM 2016 AI) | tokens56 built, OFF; delayed tracking attacks: not checked; aim vs player speed: not checked; bot stall 25-37 % |
| Hit stop 10-20 ms per hit, 0.1-0.25 s big moments; medium juice beats extreme (arXiv 2208.06155) | ladder C-2 in the rules; gun kill 0.05 s (move54), chains 0.083 s (kill55 OFF); marks 0.04 % of the body |
| First wave in view, cover just inside, a reward in the middle, a loop (the Door Problem; Level Design Book) | first wave in view ON (w56); foothold 5 / 18; loops: R2 dead-end check in n56-room.py |
| Survival tied to aggressive verbs (DOOM Eternal; ULTRAKILL) | close-kill heal ON; rank56's multiplier OFF |
| Weak points that switch off an attack, damage shown on the model (DOOM destructible demons) | WeakPoint41.cs exists; per-enemy use: not checked |
| One colour per meaning, the world under a saturation cap, a greyscale check (Valve) | rules V-1 / E-3; colour56 failed because the world out-saturates the tells (28 / 36 frames) |
| One telegraph language per boss; a longer wait earns a bigger opening | THE CROWN keeps 0.68 of its tells; THE WICK passes |
| Difficulty by the enemy mix, not HP; traversal between hard arenas (Hakita) | the mix peaks at level 2 and falls at level 4 (section 1.1) |
| Per-weapon freshness in the style meter (ULTRAKILL) | Core/Guns/Freshness19.cs exists |
| An endless mode beside the campaign (Cyber Grind) | THE GRIND (Endless21) built w21, "compiled, never seen" (FEATURES.md) |
| Bots check clearability and exploits, not fun; vision models catch 43.4 % of glitches; one fresh human per round beats any metric (arXiv 2312.05291, 2008.12937) | every recent verdict is a bot's or an uncalibrated critic's |

From GitHub (licences checked in research/github.md): Go-Explore's cell archive (Apache-2.0) fits our snapshot-able sim for a
reachability and skip-route finder; DotRecast (Zlib) and RVO2-CS (Apache-2.0) for enemy routes and surrounding (bit-exactness not
checked); NVlabs FLIP and PySceneDetect (BSD-3) for frame metrics; VideoGameQA-Bench / GlitchBench for scoring a critic before
trusting it; adobe/leonardo and DaltonLens for contrast-target palettes and colour-blind checks; NiloCat's URP toon outline (MIT) for
enemy silhouettes. No open-source DOOM token code, style meter or encounter designer exists.

**The finding:** the research mostly confirms rules we already wrote and systems we already built. The game is not short of knowledge;
it is short of shipping what it knows, of content in levels 3-5, and of a trustworthy judge.

## 3. The gaps, ranked (by how much they would change what a player feels)

1. **The shelf.** About 12 built features are OFF for near misses; the player gets none of them. Fix: ship a row ON when it beats the
   baseline on its main number and is no worse on its guard numbers; GG's play is the veto, not the gate.
2. **Levels 3-5 are three rooms each, and the roster falls back.** The game peaks at level 2. Fix: rebuild level 3 (GG 2026-10-13
   "just isn't good") from identity cards up to 5-6 combat rooms, then give levels 4-5 one new role each and a combine room.
3. **Standing still still wins** (bot stall 25-37 %). Fix: tokens56 ON under the new rule, plus pressure that punishes a still player
   (a delayed attack aimed where the player stands, aim that worsens with the player's speed). Attack behaviour is enemy design:
   GG approved it on 2026-10-21 ("2. yes", NIGHT57.md); looks and new kinds stay his (CLAUDE.md rule 7).
4. **Kills are not crunchy yet.** Fix: kill55 with hit marks that show (the research's "one white flinch frame", debris that stays),
   inside the medium-juice band; judged blind in pairs.
5. **Colour without direction.** Fix: cap the world's saturation first (V-1, measured on frames), then the per-level keys fit under
   the tells; check in greyscale and with a colour-blind simulation.
6. **No calibrated judge** (section 4).
7. **ROOMS.md is a changelog, not a table.** Fix: one "as shipped" table per level, generated from the plans.
8. **Breadth:** THE GRIND, WEEKLY, challenge codes, the BEACON and the HUNT are unmeasured since w21. Fix later: surface THE GRIND as
   the "one more round" mode (the Cyber Grind's role) or park the block (GG).
9. **The tools:** the Game M test hang wedges the Editor; the usage limit cuts nights at ~3 h. Fix: find the hanging class; three builders.

## 4. How to judge without GG
- **Calibrate the GG stand-in on GG's real verdicts.** Builds/ keeps the builds he reacted to: n29 ("looking much better overall"),
  n34 ("two steps forward and a step backward"), n35 ("the gameplay just feels awful", w35), n37 ("a little bit better"), n41-r7
  (the level 1 wall ride "awful"), n42-r4 (the wall ride "clunky and buggy", the UI "too generic and cheap"), n47 (G1-G45). Show the
  critic blind reels of those builds; it is trusted only if it predicts GG's direction (n37 over n35: "awful", then "a little bit better")
  and names his complaints (n41-r7's wall ride, n42-r4's UI) before it sees them. A critic that fails this gives no verdicts.
- **Bots for what bots are good at:** clearability, stuck spots, exploits, skip routes (Go-Explore on the sim), stall and deaths. Never
  "is it fun".
- **One fresh human per round** (the research's strongest finding): a friend or a Steam Playtest tester, 15 minutes, a fixed question
  sheet. GG decides who.
- **Frame metrics as floors:** FLIP / PySceneDetect beside feel-metrics.py, greyscale and colour-blind checks on every look row.

## 5. What the next wave should be (proposal; GG decides)
**SHIP AND DEEPEN**, 4-5 h, three builders, one batched critic per build.
- P0 (lead, first hour): the calibration of section 4 on the archived builds; the new shipping rule written into the night's prompt.
- Builder S (the shelf): one fix per near miss (kill55's marks, tokens56's idle, colour56 with the world capped, brief55's 2.4 px,
  rank56), each shipped or retired by the new rule.
- Builder L (level 3): THE KILN from identity cards to 5-6 combat rooms, every piece with a job (A-1, A-9), the role curve of R-2;
  a reachability / skip-route check on the sim.
- Builder T (the tools, small): the Game M hang found; ROOMS.md's as-shipped table generated.
- NOT this wave: new verbs, new modes, new enemy designs, the endless mode.
