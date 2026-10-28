# Card a2: 1.2 KESTREL -> THE RANGE (builder R, NIGHT65; slot a2 of n65.foundry65)

Written 2026-10-23 by 0:25, revised by 0:30 (t0 13:58:02), before any geometry, from the red (Logs/n65/red/a2/), the shipped frames
(Logs/feel/n64-freeze/vf/verbs37 f0050-f0650 and f1000-f1150 are in a2: one deck, one ramp, three red stand-ins on the door's axis) and the n52
row. Coordinates as the plan: metres, x east, y up, z south (north is -z), rect [x0, z0, x1, z1]; yaw 0 faces north, +yaw turns west.

## The one verb, and why
**The KESTREL's charged shot that pierces a line (VERBS rows 25 / 27: hold RT 0.9 s, release): line the targets up, then one shot.**
- a2 is level 1's first fight and the gun's teach room (job36 teach; teach35 l1.fire and l1.charge; lessons shots 3 then husksDead 2). GG
  2026-10-13: "there's a time and a place for the tight rooms and keep them"; "they can't be too exploitable". Rule 14 (alternate hard and
  simple) and rule 11 (never every skill at once): the first room asks for one thing, the gun, and is simple and crowded.
- **What the red does** (f0050): the three stand-ins stand on the door's axis, so the door already lines them up: the charge is taught with no
  decision, and the room is an empty 14 x 20 m box (1 deck, 1 ramp, 0 props; R5 FAIL).
- **The new room teaches "line them up" by its shape:** THE RACK's three stand-ins stand on a diagonal across the room. From the door they are
  spread over about 30 degrees (three shots); from THE MARK, a cyan square on the floor inside l1.charge's strip, they are one line (one shot).
  Then wave 1's four husks come down THE FEED RAMP, a 2.5 m wide ramp that points at THE MARK: a file, the second pierce. Wave 2 steps out from
  behind THE CARTRIDGE STACK on the west side: the front shifts (rule 6) and you turn off the mark.
- **Why it is the move, mechanically (read in code):** a stand-in dies only to a charged KESTREL shot in a standIns31 room, or to a shot from a
  wall ride (Assets/Scripts/Core/Combat.cs:1464); the stand-ins hold wave 1 back for up to 8 s (design.n46.teach46.numbers.standInHoldS 8,
  Campaign24.cs:849-851): one pierce from the mark starts the fight at once; three charges from the door cost about 3 x 0.9 s plus the aim.
- **Not exploitable / cannot be skipped:** the door locks behind you; the mark is not a camp (the feed ramp's file walks straight at it, wave 2
  comes from the side). **Not checked:** taps kill the husks, so the pierce on the husks is rewarded, not forced; "the verb cannot be skipped":
  not checked (no pilot charges).

## The design against the rect (14 x 20 m: x -7..7, z -55..-35; entry d1 at z -35, exit d2 at z -55, both x -2..2; height 7, roofed)
Depth from the entry = -35 - z. Every piece is inside the rect (min x -7, max x 7, min z -55, max z -35, checked by hand).
**Planes: 0 (the floor) and 2.5 (THE BREECH): two planes** (rule 2).

| # | piece | plan row | job |
|---|---|---|---|
| 1 | THE BREECH | deck [2.5, 0, -55]..[7, 2.5, -49.5] | wave 1's ledge (four husks in view from the press's flank: opening `held`); the high ground after; a double jump (2.7 m) gets you up |
| 2 | THE FEED RAMP | ramp from [5.25, -43.5] to [5.25, -49.5], width 3.5 (x 3.5..7, flush with the east wall: no 1 m slot left beside it), rise 2.5 | the walkers' climb route onto the breech (enemies cannot climb) and the husks' likely way down, pointing at THE MARK 2 m south of its foot: the second pierce line. The breech also has 2.5 m drops (its west edge, x 2.5; its south edge x 2.5..3.5), so the file is likely, not forced (gate note) |
| 3 | THE PRESS | hull [-5, 0, -44]..[-2.5, 2.2, -40.5] (high cover) | **R5's foothold** (top 2.2 m; 7.5-11 m from the door's centre (0, -33), the checker's way; free-standing: x -5..-2.5 inside -6.4..6.4); the room's landmark (the cartridge press: hero face); the cover you step behind when wave 2 comes from the west |
| 4 | THE RACK | standIns / standIns31 / standIns36 (static) at (2.9, 0, -44), (0.9, 0, -46.5), (-1.1, 0, -49) | the lesson's targets on one diagonal (direction (-2, -2.5)): spread from the door, one line from the mark |
| 5 | THE MARK | prop floorStrip at [4.9, 0.02, -41.5], size [1.0, 0.02, 1.0] (z -42..-41: inside l1.charge's strip z -42..-39) | where the rack is one line (3.2, 6.4, 9.6 m along it, bearing 38.7 degrees west of north) and where the feed ramp's file walks at you |
| 6 | THE CARTRIDGE STACK | hull [-7, 0, -50]..[-4.5, 1.8, -48.5] (against the west wall) | hides wave 2 from THE MARK: both wave-2 nodes now stand in its shadow as seen from the mark (below), so wave 2 steps out from behind it on the west side (a front shift, not a spawn behind) |

The rack's line from the mark is clear of every solid (x stays >= -1.1 m; the press is x <= -2.5). From the door (0, 0, -37) the three
stand-ins are all visible: the lines to them pass the press on its east side.
**Loop (rule 4):** floor -> feed ramp -> breech -> drop at its west edge (x 2.5) -> the exit apron -> floor; round the press both ways. No dead end.
**I1:** the breech and ramp are east, the press and the stack west, the rack diagonal: overlap across x = 0 expected well under 60 % (the red 0 %).

## Enemies, climb routes, spawns
- **Waves (one kind; a teach room, LDB "1 type for a tutorial"):** wave 1 `[husk, husk, husk, husk]` on THE BREECH; wave 2 `[husk, husk]` from
  the west floor. 6 husks + 3 stand-ins (the red: 2 husks + 3 stand-ins). The lessons keep `husksDead 2`.
- **Nodes:** wave 1 (waveNodes43[0], highNodes): (3.5, 2.5, -54), (5, 2.5, -54), (6.5, 2.5, -54), (5, 2.5, -51.5): 17-20 m from the door, in
  front, seen from the press's east flank (about 24-35 degrees right of its look, over the ramp's top). Wave 2 (nodes): (-6.5, 0, -50.5)
  and (-6.5, 0, -51.6), just north of the stack: the line from THE MARK's eye (4.9, 1.6, -41.5) to each meets the stack's east face (x -4.5)
  at z -48.9 / -49.8, inside its z -50..-48.5, below its 1.8 m top for both the body (+1.0) and the head (+1.6) rays (hand arithmetic, the gate's
  fix; the round-1 room check measures it).
- **Climb route:** THE FEED RAMP (2.5 m over 6 m) is the walkers' way onto the breech; validator row 4 expected PASS.
- **Spawn / retry:** (0, 0, -37), yaw 0 (kept: it is l1.fire's strip).

## The entry's view, the exit, dressing (each with a job and a role colour)
- **Entry view (frames65.entry):** THE RACK spread across the middle under its overhead rail, the press left, the breech and its ramp right,
  the cyan mark on the floor ahead-right, the exit lit at the end. No drop (the doors are at y 0); the foothold is the press.
- **The lit exit:** cyan pool [0, 0.025, -53] and a warm lintelLight [0, 4.7, -54.95] size [4, 0.2, 0.1] over d2.

| dressing / light | row | job | colour role |
|---|---|---|---|
| THE MARK | floorStrip (piece 5) | stand here to line them up | cyan (player-usable) |
| THE RACK RAIL | prop truss at [0.9, 6.3, -46.5], size [0.4, 0.4, 8.5], turned along the rack's line (38.7 degrees off north toward the west; the sign as PropsView29 reads yawDeg: checked on round 1's plan picture) | draws the line overhead so the rack reads as a line from the door | metal (structure) |
| THE BREECH LIP | prop hazardStripe at [4.75, 2.52, -49.7], size [4.5, 0.03, 0.4] | the edge the husks come over and down from | red-orange (danger: enemies) |
| THE PRESS DIE | prop hero at [-3.75, 1.4, -40.45], size [2.2, 1.6, 0.2] (the south face, toward the entry: rule 12) | the gun room's landmark: what this place makes | pale brass (landmark accent) |
| the breech lamp | light ceiling amber at [5, 6.95, -52.5], size [3, 3] | lights wave 1's ledge warm: the room's one landmark light | amber (neutral fixture) |
| the rack light | light ceiling white at [0.9, 6.95, -46.5], size [2, 4] | lights the targets | white (neutral) |
| the exit pool, the lintel | above | the way on | cyan / warm white |

## The teach beat kept (PRINCIPLES 1, VERBS)
teach35 `l1.fire` [-7, -39, 7, -35] and `l1.charge` [-7, -42, 7, -39] (room a2, minY -1) unchanged; lessons and lessons31 unchanged
(`shots 3`, `husksDead 2`); standIns, standIns31 (the pierce rule) and standIns36 (job marker, why) at the rack's new spots; job36 `teach`; verb36
"fire (tap), charge (hold: pierces): line the rack up from the mark, then the feed ramp's file"; `bot43: false`.

## frames65 and line65 (feet x, y, z, yaw deg, pitch rad)
- frames65: entry [0, 0, -36, 0, 0]; verb [4.9, 0, -41.5, 38.7, 0] (on the mark, down the rack's line); mid [4.9, 0, -41.5, 0, 0.12] (on the
  mark, up the feed ramp); high [5, 2.5, -53, 165, -0.2] (on the breech, back over the room to the door).
- line65 (8 waypoints): [0, 0, -36], [1.5, 0, -38], [3.5, 0, -40], [4.9, 0, -41.5], [4.9, 0, -41.5] (the charge, looking down the rack),
  [4.9, 0, -42] (turned north at the ramp's file), [2.5, 0, -46], [0, 0, -53] (the exit).

## MAIN against the red (set before any measurement; Logs/n65/red/a2/)
| measure | red (A seeds 1-10 / B 11-20) | aim (both sets) |
|---|---|---|
| **STYLE DULL share** (n46flow --rooms a2) | 21.1 / 23.6 % | **below the red by more than the noise band on each set: < 18.6 % on A and < 21.1 % on B** (the band: the red's own A-B spread, 2.5 pts; the card gate's fix, 0:47) |
| **kill gap, median** (kgap med) | 2.8 / 3.7 s | **below the red on each set: < 2.8 / < 3.7 s** (the husks come in files of four: kills closer together) |
| top rank (median) | BOLD / BOLD | a guard: BOLD or higher |
| air % | 73.9 / 78.8 (the bot hopping over an open 280 m2 floor) | a guard here, not the aim: >= 60 % (a2 is the gun's room, not a flight room; the lead: "a2's air is high only because it is open floor") |
| pads touched | none in the rect | none by design |
| room check | FAIL R5 (no foothold); IN VIEW not measured | **R5 PASS; IN VIEW `held` PASS (>= 4 of wave 1 seen from the foothold's flank)**; R2, R8, I1 PASS |

GUARDS: levelgen --walk 0 ERROR; clears 10 / 10 on both sets with deaths <= 1 in all (the red 0 / 0); clear median <= 25 s (the red 14.7 /
13.0 s with 2 husks; par set from round 1's median); stall <= 15 % (the red 12.4 / 11.9). openings65.json (the lead's) gets a2 `held` (a3 is
`ambushed`: I7's no two in a row kept). **Not checked at this card:** every number (no geometry yet); the truss prop's yaw sign; whether a
charged shot from the mark kills all three stand-ins in the Player (the pierce's reach and width: tour65's line65 frames show the line, not the kill).
