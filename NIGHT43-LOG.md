# NIGHT43 log (the cohesion night; NIGHT43.md; GG's thirty asks in Research/2026-10-16-wave43/GG-REQUEST.md)

## G0 (the lead, 16:00-16:25)
- 15:59 GG's message (after playing n42-r4 to level 3), saved verbatim with a row map G1-G30. The brief (night-context): THREADS aged 2+
  (T-46..T-58, 29 older), the NIGHT42 SHORT rows (K1 arena kit), HANDOFF's open items T-59..T-64. Disk 47 GB free. The Editor was in Play
  mode at the start (left from GG's session); stopped by the shared commit's compile at 16:18.
- Three read-only agents: CODE-FACTS-A (movement, level 1, doors), CODE-FACTS-B (weapons, bosses, enemies, UI, level 3), research (five
  files with URLs). What they found that explains GG's words (read, not measured):
  - G13 "after a certain number of jumps": wallJump.maxPerAirtime 3, refilled only by a landing; THE STACK has 6-panel sections.
  - G5 the door: EnemiesAlive counts the whole level; a husk knocked into a dry pit never dies (kill plane y -30); a verb lesson can hold a
    clear forever.
  - G4 the floating enemies: a8 PIERCE's three stand-in husks spawned at entry, floating at y 1.8 over the pit.
  - G2 THE FLUE: panels 7 m apart overlapping 1 m along z: the along-wall drift during the flight passes the next panel.
  - G3 B: B just after a ride is a slam (velocity (0,-48,0)).
  - G10 the pink: the boss door's violet-white + its violet light + the entrance flicker's red / blue split; the "brick wall": a 0.12 s
    hitstop at entry and the shaft's 12.4 m/s air clamp past a 4.4 m tunnel.
  - G26 the boost: a ground press before the jump fires on the ground (no boost); a jump release halves a boost (releaseMult 0.5).
  - G18 the orb parry: ~0.11 s between the 2 m eye sphere and the 0.7 m body contact; no buffer on the gauntlet swing.
  - G14 the bar: hidden while THE WICK's body is off; HP locks read as "not taking damage".
  - No drones in level 3; "heavy" is a plain 40 HP husk; no worm enemy; no blur; no locked-room counter.
- 16:18 shared commit a9da30f6: N43Blocks (12 sim rows), design.n43 (+ 8 view rows), --off43, tape 26, n43-foundry / n43-iceline / n43-kiln
  placeholders, the hooks (N43RoomLeft, N43BossState, N43Events). Gates: golden 0 mismatches / 22 exact; fingerprint EQUAL plain and
  --off43; Editor compile 0 errors; shared tests 89 / 89; TheOrder_FollowsTheBlocks 1 / 1.
- 16:25 nine builders launched (M1, L1, W1, F1, U1, E1, K1, A1, B1), each in its worktree from a9da30f6. Integration ~17:40, ~18:40, ~19:40.
- Grounding: Google Drive and Linear searched for "SKYGRIND": nothing (the project's material is this repo).
- 16:28-16:30 the befores on n42-r4 (Tools/night/n43-reels.sh; Logs/feel/n42-r4/summary43.txt; 0 NREs): first42 GREEN (2 lines in the
  first 60 s; THE GANTRY frames lip / mid / g2), wall42 GREEN (the jump press beside a wall 8.0 -> 0.0 m/s back with wallfling42), weapons42
  GREEN but **the parry missed** (pressed at -1.00 m of the eye, orb not reflected: G18's complaint on a frame), ui42 GREEN. Seen on the
  frames: GG's pad is Nintendo-layout (the prompt reads "HOLD (B): CHAIN THE WALLS": B = jump), so G3's "pressing B" may be the jump
  button (passed to M1); "+0 WALL JUMP" still shows (T-64, passed to U1); an unexplained full-width cyan bar at the top in the first 15 s.
## Round 1 (16:47-17:11): picks as builders reported (every row still off in the repo)
- 16:47 F1 (cb52a64d..b4008a5e) feel43; 16:53 U1 (bad9568c..8f6e1bcb) hud43 / counter43 / teach43 / bossbar43 / screens43; 16:54 W1
  (744122c6..18be9ce6) gauntlet43 (1.8 -> 1.25 s; ICEBREAKER bot p50 46.2 -> 44.3 s, 10/10), parry43 (presses in the last 200 ms 11/24 ->
  24/24; the earliest parry 1.94 -> 5.61 m), fusion43 (dash-in hits 0 -> 100 % FUSION, x2); 16:58 the lead's 07b5949d (slug43 / torch43
  bits for W1's round 2); 17:04 A1 (fa236e99..011cbda1) models43 + burrow43 (n43burrow GREEN); 17:06 M1 (ae78623a..ce16231d): chimney43
  (a 4.5 m chimney by presses 5.58 m -> 13.53 m top), ride43 (B 0.1 s after a ride: a drop keeping 15.97 m/s, was a slam; held + stick
  along 0/30 -> 30/30; near misses 0/12 -> 8/12 held), boost43 (jump + shot within +-0.15 s 1.48-5.08 m -> 6.33-7.98 m; level 3 G1 RED
  +10 %: kb1b's ceiling, to K1); E1 (d7118826..d0b1c9f6): rooms43 (doors open after every kill path 17/64 -> 64/64; the longest still
  enemy 6.3 -> 2.7 s), iceline43 (chargers before THE BRAWL 2 -> 4), the fields afterLesson43 / fallReturn43; F1 round 2 (eb73f7d6..52338bf8)
  look43 (found: the NIGHT26 / 36 ride strips are drawn mirrored across z = 0 on every campaign level); 17:07 B1 (734d3f51..4edf0bea): T-59's
  cause (the climb pilot turned its stick off the wall: ride40 refused every attach since NIGHT40; the bot fixed: wick37 band kills 10/10),
  wick43 (bar valid 5980/5980 live steps, leg beats, the reveal ease 34.4 -> 0 deg, the door without the red / blue split, the shaft's worst
  one-step speed loss 5.6 -> 0.7 m/s; skilled p90 63.6 s SHORT of 60). Gates GREEN after every pick; NIGHT43 tests 52/52 in the Editor.
- n43-r1 (16:59, F1 + U1 + W1) reels, rows on in the build copy only: ui43 RED (the hud43 after-image check) and the frames show the results
  screen broken (no rank letter, an empty panel), the health number overlapping the cells, "MOVE. LOOK...." truncated, the shop's slant
  strokes through item text (all to U1); the shop over its blur and the boss bar (THE WICK, LOCKED, LEG 1/3, the hint) read well. feel43 RED:
  the FUSION word and ring show but the burst washes out on level 2's pale ice; the TORCH shows almost no flame at 1 s (to F1). weapons43:
  the reel's scripted parry press never fired in either pass (pressed at -1.00 m: a reel bug, not the parry).
- Round 2 cards: W1 slug43 / torch43 / parry consistency / the dash-in buffer; F1 the FUSION and TORCH fixes, then look43's frames; U1 the
  frame fixes + the fallReturn43 fade; A1 navTell43's view, burning's view, the bosses' look; M1 THE STACK's re-ride refusal (a same-facing
  panel straight above is refused until a landing: the suspected root of G13); E1 waves wait for the player to commit, the FUSION lane,
  THE MIX's role budget, ROOMS.md level 2; B1 the band, the bar state for every boss, OPEN hits, G13 re-measured.

## The spend limit (17:15-18:50)
- 17:15 the monthly spend limit (429) stopped eight builders mid-card at once (L-003 again); every committed slice was safe. The lead
  resumed each by message at 18:51 with the reels' findings and a new clock (cards end ~20:15). About 1.6 h lost.

## Rounds 2-3 (18:50-19:29)
- Picks: U1 round 2 (results rebuilt, the health number box, teach never truncates, the fallReturn43 blink), L1 (n43-foundry: THE GANTRY
  over lava (gate 1 one wall 100 % first try; gate 2 chain + dash gap 90 % / 100 % in two), THE FLUE, PIERCE with afterLesson43), F1 round
  3-4 (the FUSION screen layer readable on dark and bright floors; the TORCH stream; the cyan wash on the charged release found:
  Viewmodel.Charge35's muzzle sphere filling 5961 % of the screen on the release frame), W1 round 2 (slug43, torch43, the held FUSION swing
  35/35, glob parries 9/24 -> 24/24), A1 round 2-3 (navTell43 drawn, burning, the greyscale read, THE WICK's cage, THE RING's look + THE KILN
  MOUTH), B1 round 2 (band p90 57.4 s, the bar state for every boss, OPEN flinch), E1 round 2 (waves wait for the player, fallReturn43.by,
  the FUSION lane 12/12, THE MIX 5 -> 3 roles, ROOMS level 2), K1 (n43-kiln THE STEPS, compositions by role, THE ASH BED; THE RING partial),
  U1 round 3 (menus, the style meter), M1 round 2 (Move43.SameWall; B1's casual WICK regression bisected to ride43 (e), turned off; THE
  FLUE's chain lever), L1 round 3 (THE FLUE 3.5 m from the door: held 87.5 %, taps 75 %, presses straight across 100 %).
- Lead fixes at integration: the SimHarness N43Arg clash (L1 / E1), W1's sync commit duplicating slug43 / torch43 rows, the ChargeShots27
  conflict (W1's slug43 + M1's boost43 lines, both kept), the burrower's budget cost (K1's levelgen error), the respawn clearing NIGHT38's
  flight meter under rooms43 (M1 traced: the GANTRY inherits flight38, so the dash after a lava fall had a 0 bar).
- Frames looked at (n43-r2 / r3): ui43 GREEN after one iteration (results with the rank letter and tiles; the health bar's after-image);
  feel43 GREEN on levels 1 and 2 (on vs off 0.07-0.09 in the centre); weapons43 GREEN (an orb PARRIED at 3.62 m; FUSION at 22.8 m/s, 955
  damage); burrow43 GREEN (the line-up; the greyscale read at 30 m measured); wick43 GREEN (the reveal, the bar LOCKED with its hint);
  look43 GREEN. RED and why: move43 (the reel's pilot never reached a wall: a reel fault, M1 fixed the boost press; not re-proven),
  foundry43 0/11 frames on n43-r3 (the player placed before the campaign started; L1 fixed the reel), rooms43 (the reel's pit body fell to
  the kill plane before the 2 s rule; E1 fixed the reel; the harness pins 3/3).
- 19:22 the freeze: 22 rows ON (d621d570), each enabled_why naming its frames and numbers; gates GREEN; NIGHT43 tests 56 / 56; shared
  tests 89 / 89; Campaign24 22 / 22; Lesson25 fails only Lesson25TextTests' 7-word prompt (T-62, unchanged).

- docs/DESIGN-RULES.md (G12 / G9 / G29: the numbered rules M / C / E / R / A / T / P / U / V / B and the new-room checklist), linked from
  PRINCIPLES 9; DECIDE43 (D-1..D-9).

## Round 4 in the Player (n43-r4 19:29, n43-r5 19:43; the 22 rows ON in the repo config; 0 NRE / 0 exceptions in every reel + the smoke)
- n43-r4 verbs37: level 2 20 / 20; levels 1 / 3 / 4 / 5 RED on row 41 (launch: fusion43's x2 killed body A before its launch36 launch):
  fixed (a punch that launches is not judged a FUSION, d621d570's successor) -> n43-r5 verbs37 **26 / 26 (n43-foundry), 25 / 25
  (n43-kiln; T-61's row 13 PASS too), 30 / 30 (n40-span), 30 / 30 (n41-spire)**; level 2's 20 / 20 from n43-r4 (the fix touches the punch
  path only).
- n43-r4: rooms43 GREEN (the counter 3 -> 1, the door, a pit body ended by the rule at 1.98 s); weapons43 GREEN (parried at 3.62 m;
  FUSION 22.8 m/s, 955); brawl40casual GREEN (killed 56.7 s, 5 tell kinds); burrow43 -skygrindA1Ring GREEN (THE RING built: 293 plates,
  52 hot, the kiln mouth, the slag plate); foundry43 10 / 11 frames (RED on the missing gantry-land frame; the pilot rode 0 walls: the
  frames lip / mid / dash, the flue, PIERCE's five looked at: lava, the ride strip, the counter "3 LEFT", the spawn tell); kiln43 RED
  (counters 0 in the ash bed; step 1 landed; THE STEPS frame looked at: three lit steps, "STEP 1 · PRESS B TO JUMP"); move43 RED (the
  reel's pilot: the chimney took 1 press both passes); wick37 space GREEN, fight RED (panels ridden []: the Player reel's pilot; T-59).
  Seen and not fixed: the gantry-lip frame still shows level 1's first card "MOVE. LOOK. B JUMP" (the reel's teleport or a stale card:
  not checked).
- n43-r5 weapons43 GREEN after the launch fix. **The smoke on n43-r5: "DONE: all steps passed" (228 s). Freeze 19:54, play build
  Builds/n43-r5.**

## Close table (19:58)
| card | state | measured |
|---|---|---|
| G0 the lead | DONE | facts A / B, research (5 files), the shared commit a9da30f6, DESIGN-RULES / ROOMS / VERBS marks, 5 builds, ~45 reels + the smoke, every pick gated |
| M1 movement | DONE / SHORT (its reel) | chimney43 / ride43 / boost43 ON on harness numbers (chimney 5.58 -> 13.53 m; B keeps 15.97 m/s; boost 1.5-5 -> 6.3-8 m), SameWall, the WICK regression found and fixed ((e) off); move43's Player pilot never rode a wall |
| L1 level 1 | DONE / SHORT (walk bot) | n43-foundry ON: THE GANTRY 90 / 100 % people, THE FLUE held 87.5 % / taps 75 %, PIERCE in order, G11 table; the walk bot 0/10 through b2 (its W3 -> W4 hop) |
| W1 weapons | DONE | gauntlet43 1.25 s, parry43 (5.61 m), fusion43 (100 % dash-in), slug43, torch43, glob parries 24/24; all ON |
| F1 feel | DONE | feel43 + look43 ON after two iterations (on vs off 0.07-0.09); the cyan wash's cause fixed |
| U1 UI | DONE | hud43, counter43, teach43, screens43 (shop, results, pause, title, levels), bossbar43, the style meter: ON after two iterations |
| E1 encounters | DONE | rooms43 (doors 17 -> 64 / 64), G22 (6.3 -> 2.7 s), afterLesson43 / fallReturn43 (+by) / waveNodes43, iceline43 (chargers 2 -> 4, THE MIX 5 -> 3 roles): ON |
| K1 level 3 | DONE / SHORT (THE RING geometry) | n43-kiln ON: THE STEPS, compositions by role, THE ASH BED, headroom (G1 +56 %); THE RING only a slag plate + the landmark data |
| A1 enemies | DONE | models43 ON (greyscale read measured), burrow43 ON, burning, navTell43, THE WICK's cage, THE RING's look + THE KILN MOUTH |
| B1 THE WICK | DONE / SHORT (T-59 in the Player) | wick43 ON: the bar for every boss, the reveal, leg beats, the door; T-59's harness cause fixed (10/10); the Player fight reel still RED |
| verbs | PASS | 26 / 26, 20 / 20, 25 / 25, 30 / 30, 30 / 30 |
| suite | SHORT | the whole suite not run at the NIGHT43 freeze (T-70; time); NIGHT43 56 / 56, shared 89 / 89, Campaign24 22 / 22 run |
| memory | SHORT | the wave-memory harvest for w40, w41, w42 not run (carried since NIGHT42: building came first); w43 has ledger/wave-43.md and CORE's state line; THREADS has w40-w43 paragraphs |
Fates of every GG ask: NIGHT43-REPORT.md (G1-G30 each with its state). In short: G1 ON (foundry43 THE GANTRY), G2 ON (THE FLUE), G3 ON
(ride43), G4 ON (PIERCE), G5 ON (rooms43 + counter43), G6 ON (hud43 / screens43 menus), G7 ON (teach43), G8 ON (look43), G9 DONE
(DESIGN-RULES A, ROOMS.md), G10 ON (wick43's door), G11 ON (one-way rooms), G12 DONE (DESIGN-RULES), G13 ON (chimney43 + SameWall + B1's
pilot), G14 ON (bossbar43 + wick43 state for every boss), G15 ON (the reveal; sound: event names only, GG's), G16 ON (leg beats, OPEN
flinch), G17 ON (gauntlet43 1.25 s), G18 ON (parry43), G19 ON (fusion43 + feel43 + the FUSION lane), G20 ON (feel43, slug43, torch43,
rumble), G21 ON (models43), G22 ON (rooms43's watchdog + navTell43), G23 ON (iceline43 chargers 2 -> 4), G24 ON (screens43 shop / results),
G25 ON (kiln43 THE STEPS), G26 ON (boost43 + the steps), G27 ON (kiln43 / iceline43 compositions by role), G28 ON (burrow43, THE ASH BED,
THE RING's look) / SHORT (THE RING's geometry, T-65), G29 DONE (DESIGN-RULES V, look43), G30 DONE (the night: built, framed, iterated).
