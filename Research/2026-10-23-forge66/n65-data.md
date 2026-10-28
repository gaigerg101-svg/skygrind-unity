# n65 data: what made a room's verb NEEDED in NIGHT65's sim, and what did not (FORGE66 data scout)

Read-only scout, 2026-10-23. Everything is read from saved outputs; nothing was re-run. The paired ratios and geo-means are my arithmetic on
the files' own seed lines (refused clear s / verb-aware clear s, per seed). No Player ran on NIGHT65 (branch C): every number is a bot's.

**File keys.** SK-rN = `Logs/n65/rN/verbs-skip.txt`; SK-red = `Research/2026-10-23-wave65/T/verb-skip-verbs65-red.txt` (n52-foundry,
verb65 planted, bot43 false); SK-fd = `.../T/verb-skip-verbs65-fd9c565b.txt`; R3V = `Research/2026-10-23-wave65/R-round3-verbs.txt`;
TAB-rN = `Logs/n65/rN/table.md`; AIR-red = `.../T/air-red.txt`; AIR-r1 = `.../T/air-latest-02d789cd.txt` (the r1 room files);
AIR-rN = `Logs/n65/rN/air.txt`; VP = `Tools/SimHarness/Harness.N65.VerbPilot.cs`; VB = `Tools/SimHarness/Harness.N65.Verb.cs`.
Format below: verb-aware / refused median s | STYLE above DULL on / off % | paired geo-mean.

**Rounds** (assembly sha from `Logs/n65/rN/gen-latest.txt`; room commit by `git log -1 <sha> -- Design/levels/n65/rooms/<id>.json`):
- r1 0d163c65, r2 d602157a, r3 1367e92a ran the PLAIN pilot on 3 seeds. It never launches or parries on purpose (SK-r1's "NOTE the pilot
  never used the verb" for a5, b1, a4).
- r3b 0c73e732 added `--verbs65` and 5 seeds; r4 71f6577a added the exercised-seed rule (VB:509-534); r5 6789b672 is the density pass.
- **The three GREENs first appear with the instrument, not the geometry:** a5's room file is 73ac7a88 at both r3 and r3b, and reads RED
  28.9 / 31.7 with launch 0 (SK-r3), then GREEN 22.7 / 31.7 (SK-r3b).

## 1. Per room: the skip test round by round, what moved it, the lever

**b1 THE KNOCKOUT YARD (launch): the strongest GREEN, but the red was already GREEN under the same pilot.**
- red 28.1 / 36.7 | 52.9 / 33.0 | 1.25 (SK-red). r1-r3 plain: 44.0 / 44.0, then 32.2 / 32.2, launch 0 (SK-r1, SK-r2, SK-r3).
- r3b 27.6 / 39.1 | 79.1 / 41.4 | 1.29, launch on 4 / 5 seeds. r4 22.7 / 37.0 | 73.5 / 36.4 | 1.44, 5 / 5 exercised. r5 22.1 / 45.1 |
  71.9 / 45.8 | 1.74, launches 3 / 1 / 2 / 3 / 2 (SK-r3b, SK-r4, SK-r5).
- What moved it: a9e88161 GATE M closed (every pack through GATE W under the rail): 28.5 / 47.2 on S's branch (`.../cards/b1.md:9`);
  P16 THE SILL (19f6d4ab) brought refused back to 39.1 (cards/b1.md:18); P17 THE NORTH SILL (5f666b8f) took verb-aware 27.6 -> 22.7
  (SK-r3b -> SK-r4); P18 THE CASTING TROUGH (464750b8, 1.0 m cover facing the floor) took refused 37.0 -> 45.1 (SK-r4 -> SK-r5).
- Not checked: no rebuilt b1 ran the verb-aware skip before GATE M closed (no verb65 row before 4827ee3b).
- Why: the gates bunch the pack, the sills lift it 1 m into the punch, and the burst (60 in 3.5 m) beats a husk's 40 hp
  (`Research/2026-10-23-wave65/S.md:31`). On seeds 2, 4, 5 the plain pilot never pressed a punch at speed (presses refused 0, SK-r5): the
  slow side is the plain pilot's gun, not the refusal.
- **Lever: one gate per pack on the verb's line, plus low cover that blocks the floor gun's line but not the dash in.**

**a5 CRUCIBLE (launch + slide): GREEN by the bar, weak in substance.**
- red 22.6 / 27.5 (x1.22, RED) | 58.6 / 18.4 | 1.24 (SK-red). r1 / r2 plain 38.5 / 38.5, 34.2 / 34.2, launch 0 (SK-r1, SK-r2).
  Verb-aware at d602157a 24.6 / 28.3, GREEN only by one death (R3V:25); at 73ac7a88 22.7 / 31.7 (R3V:20).
- r3b 22.7 / 31.7 | 58.3 / 53.0 | 1.35, launch on 3 / 5 seeds (uses 0 / 3 / 1 / 1 / 0). r4 22.7 / 33.2, 4 / 5 exercised; 28e7a132 is a
  strip and a light, no geometry. r5 20.5 / 31.8 on 3 / 5 exercised (the minimum) | 73.1 / 59.1 | 1.47, launch on only 2 / 5 seeds.
- What moved it: 73ac7a88 put wave 2's apron husks on the slot's exit line AND made wave 1 six husks plus the spitter, in one commit:
  refused 28.3 -> 31.7, verb-aware 24.6 -> 22.7. The data cannot separate the two.
- My reading: the bigger wave 1 is the likelier lever, not wave 2.
  - The launch target is "always a wave-1 east husk that walked at the pilot" (R3V, the probes block).
  - T's `--wave 2 --slide` launch is RED, launch36 0, target alive (`.../T/verb-launch-a5-wave2-slide-4332130b.txt`).
  - No launched body has entered THE POUR in any run (`Logs/n65/r5/verbs.md`, "in lava no" twice).
  - r5's no-launch seeds still read 1.25 / 1.25 / 2.59 (SK-r5).
- **Lever: a pack bigger than a standing shooter clears fast, inside the area verb's reach. THE POUR has no measured job.**

**a4 THE SPITTING GALLERY (parry): GREEN since r4, and the least confounded pilot of the three.**
- red 10.6 / 11.8 RED, 4 parries in 5 seeds, geo 1.25 (SK-red). r1-r3 plain 29.3 / 29.3, 22.5 / 22.5 (SK-r1, SK-r2, SK-r3).
- r3b 17.6 / 25.2 GREEN on its face, but seed 1 parried 0 and still won 17.6 vs 29.5 (SK-r3b); the same room under the exercise rule:
  25.6 / 25.2 RED (SK-fd). r4 and r5 21.4 / 32.3 | 36.9 / 16.4 | 1.40, parries 2 / 1 / 1 / 1 / 2 (SK-r4, SK-r5).
- What moved it: fded0915 widened P1 THE TELL BLOCK to x -2.0, raised it to 3.6 m, and set P8 apexAbove 2 -> 0.8. Refused 25.2 -> 32.3,
  verb-aware 25.6 -> 21.4 (SK-fd -> SK-r4). S's cause: every death was a pilot at the door "trading shots with the gallery's spitter"
  (S.md:336-339); the block hides the door.
- Why: one parried glob kills a 60 hp spitter (24 on the hit, then a 40 burst; S.md:26-28). The layer's footprint is tiny: the trigger
  held 14 steps over 5 seeds (SK-r4), against 1654 in b1 (SK-r5).
- Caveat: "presses refused 0" on every seed (SK-r4). The plain pilot never punches near a glob (S.md:30), so "refused" means "no parry".
- **Lever: take away the gun's safe line to the shooter, so the glob's line is the cheap answer.**

**a2 KESTREL (pierce): RED in every round.** red 12.2 / 11.1, geo 0.85 (SK-red); r1 19.4 / 16.6 (SK-r1); r2 22.4 / 20.7 (SK-r2), with
verb-aware 20.0 / 20.7 (R3V:23); R's round 3 (73ac7a88: THE WEST RIDE lights, two breech nodes) 23.6 / 17.8 (R3V:18); r3b-r5
19.7 / 18.9 | 67.3 / 31.3 | 1.12, per seed 0.67-1.64, the same in SK-r3b / SK-r4 / SK-r5. The one move that helped, like for like:
8e37e8bf turned THE FEED RAMP onto THE RACK's line and added THE FEED STEP (median ratio 0.75 -> 0.96). Reverted probes: waves 6 + 4 husks
26.3 / 23.9; husksDead 7 23.6 / 21.2 (R3V:28-29).

**a3 RIDE (ride): RED in every round, and the more the pilot rides, the slower it is.** red 32.0 / 24.0, deaths 5 / 1, geo 0.66
(SK-red); r1 34.3 / 29.0 (SK-r1); r2 29.0 / 23.6 (SK-r2); r3 26.1 / 27.4, the only round where refusing was slower (plain, SK-r3); r3b-r5
33.7 / 24.2 | 35.7 / 16.6 | 0.71. Per seed (rides, ratio; SK-r3b): 3, 0.72; 4, 1.49; 7, 0.72; 12, 0.32; 10, 0.72. The red the same: 23
rides, 0.31 (SK-red). No round's change moved it: the pickup, THE RIDE'S END, the ladle rail, the sprue blocks in and out (6b2998a0).

**a8 THE SPILLWAY (pierce): RED, and worse than the red.** red 19.8 / 20.2, geo 0.99 (SK-red); r1 22.6 / 27.2 (SK-r1); r2 / r3 17.2 / 15.4
(SK-r2, SK-r3); r3b-r5 23.0 / 14.5 | 47.1 / 51.4 | 0.70, every seed slower with the verb (0.52-0.90). 4827ee3b moved P7's south face to
z -360 for nav drop links, so the file is in the lane by 2 s (S.md:269-272) for any shooter, a standing tapper included. The refused clear
fell 15.4 -> 14.5 s; the pilot changed between the two (plain 3 seeds -> verb-aware 5).

## 2. The rooms that stayed RED: what the pilot did, why refusing was faster, the levers

**a2.**
- What the pilot did: 26 charged shots in 5 seeds with the trigger held 6252 steps: 10.4 s a seed of a 19.7 s median, about 2.0 s of
  episode per charged shot (SK-r3b). Refused: taps only, 4393 presses refused (SK-r4).
- Why refusing is not slower:
  - two 25-dmg taps 0.4 s apart kill a 40 hp husk (`PortExport/config.json` husk.hp, weapons.revolver);
  - the charge spends 0.5 s with no fire, then a 2.5 s cooldown (`Design/unity-config.json` n35.charge35.revolver.cooldownS), for 225 a body;
  - the stand-ins hold wave 1 for at most 8 s (`Design/unity-config.json:7032`), and with no hold the bot stalls more (46.0 / 50.5 %,
    `Research/2026-10-23-wave65/R.md:119-121`).
- Instrument limits: the pilot never walks to THE MARK, where the staged pierce kills 3 / 3 (`Logs/n65/r5/verbs.md`). Its trigger counts a
  second body within 2.0 m of the line (VP:203), but a husk's radius is 0.4 m (config husk.radius). Bodies per charged shot are not printed:
  NOT CHECKED.
- Levers:
  - (a) lay THE FEED RAMP so the level shot from THE MARK's 3.8 m eye runs down the whole file; today it meets the ramp's bodies only on its
    top 1.0 m, 8.6-9.6 m out (`Logs/n65/r3b/review-ld.md`, a2 Breaks, derived);
  - (b) one body in the file that taps cannot clear cheaply: a 150 hp charger is 6 taps (2.0 s of fire), the only pierce gain measured all
    night (a8 16.1 vs 17.4 s, `.../cards/a8.md:25-28`);
  - (c) targets that hold on the line: the two rooms whose targets hold (a2's stand-ins, a4's spitters) are the two whose verb pilots pass
    (review-ld r3b, "Across the six").

**a3.**
- What the pilot did: 36 rides with the trigger held 2921 steps, 4.9 s a seed (SK-r3b). Refused: 3621 air jump holds (SK-r4).
- Why refusing is faster:
  - the ride runs 9.2 m/s against a walk of 8 m/s (AIR-r5's numbers line; config ground.walkSpeed), so a 19 m runner saves at most ~0.3 s;
  - RIDE KILL refunds a dash, not damage (`Design/unity-config.json:8398`);
  - the floor's walk to the exit is open (`Logs/n65/r3b/review-gg.md:65`);
  - more enemies made it worse: 8 swarmers + (3 husks, 4 swarmers) read 46.1 / 29.5 (R3V:30).
- Instrument limits: during a ride the look is set along the face (VP:286-287, 313-314), so **the ride pilot never shoots a body from the
  wall**. It re-rides after a 1 s rest whenever a face is in reach, steering to the farther end, not toward enemies (VP:222).
- Levers:
  - (a) a 2.4 m slag line (over the enemies' jumpUp 2.2) closing the floor's walk to the exit apron, so every route crosses THE LANDING
    (review-gg r3b:65). This is b1's move, the one rebuild where refused rose to 47.2 s (cards/b1.md:9);
  - (b) a target that only the ride's line reaches and that holds there: stand-ins die to any shot from a wall ride (`R.md:23-24`), or a
    wave released by a "reached" rect at the runner's start;
  - (c) a shoot-while-riding pilot before judging.

**a8.**
- What the pilot did: 24 charged shots with the trigger held 6025 steps, 10.0 s a seed, longer than the whole refused clear of 14.5 s
  (SK-r3b). Refused: 2287 presses refused.
- Why refusing is faster: four 40 hp husks are 8 taps, and the lane lines them up for a standing tapper too.
- Tried (cards/a8.md:15-28): 6-husk wave 1 19.5 / 16.2; a hatch gate 19.3 / 16.0; wave 1 in the lane 19.1 / 14.3; one charger leading
  16.1 / 17.4 (the only run where the pierce paid); two chargers 18.9 / 16.3.
- Instrument limits: the carded verb, "the pierce from the ride", never fired (`Logs/n65/verdict-a8.md`). The ride reaches the ledge at
  ~2.6-2.8 s, after the file has left it at 2 s (review-ld r3b, derived).
- Levers: (a) one charger at the head of the file, with deaths measured (a new type on level 1); (b) hold the file in the lane until P1's
  take-off with an afterLesson43 rect (a8 already gates a wave on a rect); (c) a pierce pilot posed on P1.

## 3. The instruments

**The skip test (`n65verb skip --verbs65`).**
- **Deterministic.** a2 / a3 / a8 print identical seed lines in r3b, r4 and r5, with no collider change between them (SK-r3b, SK-r4,
  SK-r5); a5 is identical r3b -> r4 across a light-only change. A change in it is the room's.
- **Its noise is chaos between seeds.**
  - A 14-step perturbation with no parry gave a ratio of 1.68 (a4 seed 1, SK-r3b).
  - The 9 unique seeds where the verb-aware run never performed the verb read 1.00, 1.02, 1.13, 1.25, 1.25, 1.31, 1.50, 1.68, 2.59
    (SK-red a4 s4, a5 s5; SK-r3b a5 s1, a5 s5, b1 s2, a4 s1; SK-r5 a5 s1, s3, s5). Median 1.25: **the GREEN bar (VB:531, `mOff >= mOn *
    1.25`) sits on the instrument's own null.**
  - Dropping one seed flipped a4 GREEN -> RED and moved b1's medians +3.6 / +3.2 s (SK-r3b vs SK-fd).
- **The control is confounded.** The allowed run is the verb-aware layer (VB:441-443); the refused run is the plain pilot through the
  filter, without the layer (VB:516). So the layer's routing counts as "the verb": dashes at husks, aim locks, and fire held off in launch and
  parry episodes (VP:330). Fix: put the refusal filter after V65VerbPilot, so both runs share the routing. A seed also "exercises" the rule
  if the plain pilot pressed once (VB:518): a5's r5 seed 5 counted with launch 0.
- **As a MAIN:** only with the fixed control, 10+ seeds, the per-seed paired ratio printed, and a bar above the null (about 1.5).

**The other measures.**
- **STYLE above DULL inside the skip runs:** a strong, deterministic separator (b1 71.9 / 45.8, a2 67.3 / 31.3, a4 36.9 / 16.4; a8
  47.1 / 51.4 the wrong way; SK-r5), confounded the same way. A co-MAIN once the control is fixed.
- **n46flow walk bot DULL / air / stall: a guard only.**
  - The bot never charges, rides, slides or launches (`R.md:218-219`) and parries only melee wind-ups (`S.md:30`); a4's 84.5 / 80.0
    (TAB-r5) is its inability to parry.
  - The same-geometry residue is ~8 pts (unchanged a3 read +8.3 / +4.3, `S.md:98-103`), and the A / B sets move 8-10 pts in opposite
    directions on a cover change (`R.md:241-243`).
- **n46flow `--verbs65` DULL:** the better STYLE read (b1 24.5 / 23.9 against the plain bot's 48.5 / 57.3, `S.md:310`). Compare it only
  to the red under the same `--rooms` pilot (a2 25.9 / 27.5 against 36.1 / 43.2, `R.md:200`); T's red flow is full-level, not like for like
  (`R-round2-verbs65.txt:5-6`).
- **The verb pilots** (launch / pierce / parry / ride): staged, seed-1 existence proofs from line65[0]. A guard: they show the line
  exists, not that a player reaches it (`Logs/n65/verdict-a2.md`).
- **air65:** a static design aid and a guard. It counts low-cover hops (THE MOULD CRATES +10 hand-offs, AIR-r4 -> AIR-r5) and counts a
  lights-only ride as 0 (a2's WEST RIDE). No enemies, no line of sight, no air control.

**Missing:** the fixed control; a shoot-while-riding pilot; a launch with a lava target (T's `--wave` / `--target` exist, the first try
RED); bodies per charged shot in the skip runs; a pilot that walks to the verb's spot (line65[0]) first; a pierce from a ride pose;
hand-off chains in the pilot's own path (it performs one verb per episode and never chains pieces); Player frames.

## 4. Air65 hand-offs, round by round (pieces / hand-offs / flights handed on / dead ends)

| room | red (AIR-red) | r1 (AIR-r1) | r2 (AIR-r2) | r3 (AIR-r3) | r3b-r4 (AIR-r3b, AIR-r4) | r5 (AIR-r5) |
|---|---|---|---|---|---|---|
| a2 | 1 / 0 / 0 of 0 / 1 | 3 / 6 / 0 of 0 / 0 | 4 / 10 / 1 of 1 / 0 | 5 / 17 / 1 of 1 / 0 | same | same |
| a3 | 9 / 45 / 3 of 3 / 0 | 4 / 11 / 1 of 1 / 0 | 6 / 19 / 2 of 2 / 0 | 5 / 12 / 2 of 2 / 0 | same | same |
| a4 | 1 / 0 / 0 of 0 / 1 | 2 / 2 / 0 of 0 / 0 | 2 / 2 / 0 of 0 / 0 | same | 3 / 3 / 1 of 1 / 0 | same |
| a5 | 7 / 18 / 4 of 4 / 0 | 7 / 24 / 2 of 2 / 0 | 7 / 23 / 2 of 2 / 0 | same | same | 8 / 33 / 2 of 2 / 0 |
| b1 | 6 / 9 / 3 of 4 / 0 | 8 / 31 / 2 of 3 / 0 | 11 / 58 / 3 of 4 / 0 | same | 11 / 59 / 3 of 4 / 0 | same |
| a8 | 2 / 2 / 2 of 2 / 0 | 3 / 2 / 1 of 2 / 1 (D2) | 4 / 4 / 2 of 3 / 1 (D3) | same | 5 / 4 / 2 of 4 / 2 (R2, D4) | same |

- **Added a hand-off:**
  - a2: THE PRESS PAD (00e66d11) is the only flight (to the breech, 13 m); THE FEED STEP deck (8e37e8bf) took it 10 -> 17.
  - a3: THE RETURN PAD (00e66d11 / 071a2141) took flights 1 -> 2. a4: P8 THE BELLOWS PAD (bdcd6ef8) is its first flight.
  - b1: THE NORTH SLAG LINE and P15 THE LADLE (245098bb / 33b8cbb7) took it 31 -> 58 and flights to 3 of 4, equal to the red.
  - a8: THE SLUICE as a 6 m ride (33b8cbb7) gave R1 <-> R3 wall jumps.
- **Lost one, or added a dead end:**
  - a3: cutting THE HOOK SHELF (the 4.0 m L3, 73ac7a88) took it 19 -> 12.
  - a5: flights stay 2 against the red's 4; THE MOULD CRATES' +10 are ledge hops only.
  - a8: P14 THE SLUICE's east skin (19f6d4ab) is R2, held 0.2 m over the pit, a dead end; with D4 that is 2 / 4 against the red's 0 / 2.
- **Read:** hand-offs do not track the skip (b1 59 GREEN, a5 33 weak GREEN, a2 17 RED, a3 12 RED, a8 4 RED, a4 3 GREEN). The pilot
  never chains pieces, so the skip test never sees air65.

## 5. Ranked design changes for NIGHT66's first round

**Do the instrument first** (the fixed control, 10 seeds, the paired ratio printed); otherwise every GREEN below sits on the 1.25 null
(section 3).

1. **a3: a 2.4 m slag line closing the swarm floor's walk to the exit apron, so THE LANDING is the only way to d3.** Refused 24.2 against
   the ride's 33.7 s (SK-r5), and a ride is only 15 % faster than a walk; b1's funnel is the one rebuild where refused rose (47.2 s,
   cards/b1.md:9). Judge: the skip at 10 seeds against geo 0.71 now / 0.66 red; guards levelgen walk proofs, deaths, stall band.
2. **a8: one charger at the head of the lane file, plus a reached-rect hold until P1's take-off.** The only measured pierce gain, 16.1 vs
   17.4 s; two chargers reversed it, 18.9 vs 16.3 (cards/a8.md:25-28); today's ratio is 0.70 (SK-r5). Judge: the skip, with deaths on both
   n46flow sets. A new enemy type on level 1 is the lead's or GG's call.
3. **a5: hold wave 2 on THE APRON at the slot's exit (a reached rect at the slot's mouth), or fill THE POUR.** No launch into lava in any
   run (`Logs/n65/r5/verbs.md`; T's wave-2 launch RED), and the launch fired on only 2 / 5 skip seeds (SK-r5). Judge: `n65verb launch
   --wave 2 --slide` printing "in lava yes", and the skip with 4+ of 5 seeds exercised.
4. **a2: THE FEED RAMP re-laid along THE MARK's level shot line, plus one 150 hp body in wave 1.** 8e37e8bf was the only a2 move that
   shifted the skip (0.75 -> 0.96, R3V:18 vs SK-r3b); the ramp meets the shot on only its top 1.0 m (review-ld r3b, derived), and two taps
   kill a husk. Judge: the pierce pilot's bodies per shot (needs the print), then the skip.
5. **a3: a target only the ride's line reaches, that holds there** (stand-ins at THE RIDE'S END, or a wave released on the runner's
   start). Holding targets are what the two clean verb rooms share (review-ld r3b, "Across the six"). Judge: a shoot-while-riding pilot
   (missing), then the skip.
6. **b1's P18 pattern elsewhere: low cover that blocks the floor gun line, not the verb's approach** (a8's lane mouth, a5's apron). For:
   b1's refused 37.0 -> 45.1 when P18 went in (SK-r4 -> SK-r5). Against: a2's 1.2 m floor wall broke a walk proof and worsened DULL
   (`R.md:179-180`). One point each way, inside seed chaos. Judge: the skip.
7. **a8: THE TAILRACE PAD to about (-2.5, -2.3, -347) at R2's north end, so R2 hands on.** air65 dead ends 2 / 4 against the red's 0 / 2
   (AIR-r5, AIR-red); gg-proxy BLOCKING (review-gg r3b:138). Judge: air65 and hold52 --pads.
8. **a4: a short east ride strip at z -94..-100 toward the spitters' eye line, ending short of d4.** The room has rides 0 (AIR-r5); the
   full-wall ride read I2 1.08, a FAIL (`S.md:279`). Judge: room check I2 >= 1.3, and the skip held at geo 1.40 or more (SK-r5). Keep the
   tell block that made the parry pay (fded0915).
