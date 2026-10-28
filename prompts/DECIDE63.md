# DECIDE63: GG's calls for NIGHT63 (the boss night, 8 hours), seeded by the forge FORGE63 (2026-10-22)

The run appends one D-id per call it makes for GG (the choice, the alternative rejected, why). GG answers in his own words; nothing here is
built or reverted for him. Untracked, like every DECIDE file.

## Open from NIGHT62 (DECIDE62.md; still yours)
- **Keep or drop: no yellow on the guns** (gunYellow61, ON since NIGHT62; play Builds/n62-freeze, the THUNDERCLAP on level 3, the TORCH on 5).
- **The boss rule line's timing:** NIGHT63's slate (P2) moves it to rise with the name card at or before 1.0 s, as its doneWhen always asked.
  If you would rather it come after the card (2.45 s), say so and the row's doneWhen changes instead.
- **Lights on fixtures (fixture61):** NIGHT62's spotter saw nothing. Keep it (NIGHT63 leftover 5 makes the housings bigger and lit) or drop it.
- The parked threads (T-133, T-134, T-135, T-139, T-142..T-147), the disk (Logs/ 200 GB, Builds/ 70 GB), the 56 OFF rows from NIGHT55 or older.

## New from the forge (FORGE63)
- **What "rigid" means to you:** NIGHT63 reads it as no wind-up, no follow-through, no flinch and no idle life (B1). If you meant the bosses'
  shapes or their attack patterns, say so: those are enemy design (CLAUDE.md rule 7: yours).
- **The colour roles:** NIGHT63 enforces CLAUDE.md rule 8's roles on effects (cyan yours, red-orange danger, magenta RIVAL, pale violet-white
  the boundary, yellow the parry tell). A new role (pickups, hit confirms) is yours to name.
- **The rooms and the movement** (gg-proxy's #1 and #2, seven asks each) are not tonight's lead; a level night and a movement night are the
  next candidates. Your order?

## The run's calls (D-1 onwards, written by NIGHT63)

### D-1 Yellow's role in the colour table (A1, 0:05)
**The call:** parry = a yellow role in the table (colours63's numbers), used only by the parry tell. No other effect, gun, pickup or UI
element may take the yellow arc while colours63 is on; C1's audit flags any yellow outside a parry tell as an offender.
**Rejected:** no yellow role at all (parry tells re-hued into the danger band): it would remove the one tell that says "this one you parry"
(yellow56's parry tells, CLAUDE.md rule 8's spirit, ULTRAKILL's parry flash being the same on every enemy, research-effect-colours.md);
and a yellow "pickup / reward" role: gunYellow61 (ON since NIGHT62) just took yellow off the guns for the parry tell's sake.
**Why:** the forge's default (NIGHT63.md section 1 step 5), it keeps NIGHT62's only PASS consistent, and one hue keeps one meaning.
**GG:** if yellow should mean something else (pickups, hit confirms), say so and the table's role changes.

### D-2 M1, the camera's chop: measured candidates, a proposal, no change tonight (0:14; rewritten 0:25 after M1's GATE FAIL)
**Measured** (Logs/n63/m1.md, round 2): (a) the frame pacing, in an unsynced bench on Builds/n62-freeze (the bench forces vsync off):
p50 3.70 ms, p99 10.10 ms, max 32.10 ms, 6.32 % of frames changing interval by more than 2x; the spikes are main-thread engine time, not
scripts, GC or the GPU. The game's own default is VSYNC on (move54 ON); how the spikes behave with VSYNC on is not measured. (b) On a fixed
clock the camera path is smooth between events (rms ~2 cm); its sharp moves are designed events: 13-16 eye-snap events per 30 s reel, mostly the slide's
eye drop and rise (0.70 m in 0.10 s, starting and stopping at full speed, plus the slide dip 0.15 m in 0.04 s), about a third of the
landing dips, the slam's stop and the rail swap (the "step-ups" of round 2 were the reel placing the player 0.2 m up: M1's GATE round 2).
Neither candidate is proven to be what GG feels. M1 itself is SHORT (two GATE rounds FAIL; the corrections are text only, not re-judged).
**The call:** propose, not change: a movement night opens with (1) a bench that keeps VSYNC and a Development Player's profile of the
engine spike, and (2) the eye's start and stop eased (a critically damped follow on the eye ease and the slide dip, same 0.10 s),
measured by this tool's residual at the slide events and landing dips (want each peak halved; the slam and the rail swap excluded, their
step is the feet's), then a blind paired A/B. **Rejected:** a slower crouch (0.70 m needs 0.24 s to
stay under 5 cm a frame: a different slide); changing anything tonight (GG led with bosses; a camera change needs GG's blind judgement,
L-001; the dips were tuned on NIGHT17-18).
**GG:** which do you feel as "choppy": the picture stuttering, or the camera's jolt on a slide / landing / step? Is your VSYNC on?

### D-3 P1 (the exit frame) is judged as built first; one brightness round only if its SPOT names the door (0:30)
**Measured** (Logs/n63/p1-red-n62pair.txt, n60-v-lamp.py on NIGHT62's s1on / s1spot pair, exitFrame61 on / off): the lamp's own gain on
the door 1.22 / 1.16 / 1.41 / 1.07 / 1.27x in kb2 / kb2c / kb3 / kb3c / kb3d: 0 of 5 at P1's 1.5x (it wants 4 of 5). kb3c's clear shot
stands 21 m from the door behind a ledge, so its box is mostly dark wall in both frames (a pose the lamp cannot fix).
**The call:** b1 judges P1 as built on D1's same-run pair, SPOT first (the rule); if the spotter names the door, one fix round raises the
passage light and the bars' glow (UnityTuning.N61V.cs, Unity-only numbers) and b2 re-measures; if the SPOT does not name the door, P1 is
SHORT. **Rejected:** guessing brighter numbers before b1 (a change judged by no frame, and the GUARD, the room's p95 outside the door,
could move); loosening the 1.5x (never).

### D-4 bossRule61 ON (P2 PASS), and what its critic found that stays open (0:56)
**The call:** P2 turned ON by the rule (critic PASS, Logs/n63/verdict-P2.md). Its SPOT pair was two-run (D1 not yet picked when b1 was
built); the critic counted 131 of the 20,382 changed px outside the text band and accepted it; recorded here and in the report.
**Open, for GG (the critic's problems 3 and 4):** THE KILN WYRM's line says "Swing the gauntlet LEFT CLICK at its charging head." (the
sim's rule: only the gauntlet's swing staggers it; NIGHT61 G.md, T-150), but on the reel's frame the player holds the THUNDERCLAP, and the
name card's old subtitle said "SHOOT ITS HEAD". Whether a real campaign run reaches level 3's boss holding the gauntlet: not checked. If it
does not, the wyrm's rule cannot be followed as written: a boss-design call (yours, CLAUDE.md rule 7).
**Open, a fix later tonight if time allows (problem 5):** on THE ICEBREAKER the boss bar's tag ("PARRY THE BLOCK. PUNISH STUCK FISTS")
stands above the new line, two instructions at once; the row hides the name card's subtitle while its line holds but not the bar's tag.
**Rejected:** holding the flip for these (the doneWhen is met as written; the line now arrives on time on all five bosses).
**Added 1:30 (P2b on n63-b3):** problem 5 is fixed (the bar's tag is held while the rule line stands: "[boss61] bar hint held / back"),
but on THE KILN WYRM the bar's own hint that comes back after the line is "PUNCH THE HEAD TO STAGGER IT", which contradicts the rule line
("Swing the gauntlet ... at its charging head"; the sim: only the gauntlet staggers it, T-150). Which is right for the player you want is
your call; the hint text lives in the wyrm's bar config (not changed tonight).

### D-5 D1 has not landed: B1 is judged on a two-run pair on b2; P1 takes its fix round on its measured MAIN (1:00)
**Measured on n63-b2:** D1's planted no-op pairs change 19 / 12 / 7 / 45,686 / 195,235 / 253,849 px (want 0) and the plant 122 / 117 /
117 / ... (want 100): the viewmodel, the crosshair, a hit flash and a full-screen noise still move in the held frame; on b63i1 (boss63
-skygrindSameRun63 n63.bossMotion63) the off capture is black (the HUD and the gauntlet only, no world). P1's own gain on its same-run
pair 1.21 / 1.16 / 1.37 / 1.07 / 1.23x (0 / 5; GUARD 5 / 5), as NIGHT62's two-run pair (1.22 / 1.16 / 1.41 / 1.07 / 1.27x).
**The calls:** (1) D1's own doneWhen is RED, so it has not "landed" in NIGHT63.md section 2's sense: rows are judged on two-run pairs
until it passes, each SPOT file and the report say so; D fixes it first (one round). B1's SPOT on b2 is the plain b63i vs
b63ioff (-skygrindNoBossMotion63) pair; the boss fight's start repeats closely between runs (P2's two-run pair: 131 px outside the text
band). (2) P1's fix round now, triggered by its measured MAIN rather than by a SPOT (D-3 waited for a SPOT; a same-run measurement of the
MAIN at 0 / 5 is the stronger reason): the exit frame's bars brighter (UnityTuning.N61V.cs ExitFrame61Glow 4 -> 8) and the passage light
three times (ExitFrame61LightIntensity 12 -> 36), Unity-only numbers, judged in b3 with its SPOT first. **Rejected:** a wider bar (it would
fall outside the door box the meter reads and could move the GUARD); SPOTting P1 on b2's dirty pair.

### D-6 P1 SHORT and its numbers reverted; D1 used for same-run pairs from b3 with a 0-16 px residual; V's changed assertion (1:25)
**P1:** after its fix round, on n63-b3's same-run pinned pair, own gain 1 / 5 at 1.5x and GUARD 4 / 5, and the SPOT named the player's
gun (lit cyan) before the door: SHORT. The brighter numbers are reverted (acbabae4) so the OFF row keeps NIGHT61's measured-clean GUARD.
For GG: the exit frame reads as "brighter by eye" but not 1.5x on a door that opens onto a lit room; a different design (a light the
player walks under, or a door that changes colour) is a later night's, not more glow.
**D1:** on n63-b3 its planted no-op pairs are 0-16 px and the plant 100-111 px (from up to 253,849): not the doneWhen's exact 0 / 100
(a residual source not found), but 4-5 orders cleaner than a two-run pair (139,791 px at kb3). From b3 on, SPOT pairs for the rows D1
claims are same-run, and each SPOT file states the no-op residual; D1 itself goes to its GATE with the numbers as they are (the critic
decides). **Rejected:** waiting for an exact 0 before using it (the residual is under 0.001 % of the frame).
**V's fix round changed one existing assertion** (N63BossClockTests, its own pick-1 test from tonight: "staggered with two tells pending
reads Stagger"), in the RED commit with the reason stated, because that pinned rule is the cause of the wyrm's late wind-up. Accepted as a
design change of tonight's clock, not a test edited to pass; the B1 GATE is told and judges it.

### D-8 A boss's weak point is the player's cyan on every boss (B3, builder D's call, logged 1:48)
**The call:** B3 (bossColours63, OFF until it passes) draws every boss's weak point cyan (the player-usable colour: "hit this"): THE
WICK's idle core (amber today), THE CROWN's gems and halos (red-orange), THE LOOM's knees, nails and seams (pale violet-white), THE KILN
WYRM's punchable head (a yellow punch mark), THE ICEBREAKER's stuck fists (yellow since NIGHT47); four of them already turn cyan when OPEN.
Attack effects go to the danger red-orange (the wick's ember edges, the crown's gold clap, ring edge and the yellow in its flame).
**Rejected:** yellow (D-1 keeps it for the parry tell), the pale violet-white (it means the arena's edge), danger (the weak point would
read as the attack). **GG:** if a weak point should read differently from "your colour", say so.

### D-11 B2's fix list follows the corrected measure; its test is made to compute, not pin (3:05)
**The call:** B2's fix list is recomputed by builder G's own tool from n63-b6's before logs (the corrected meter: the verdict by the sim's
tell kind, the reference from the previous hit): wyrm:spit (no cue) and crown37:crownFire (off the danger hue, 41 deg); beamCut and lowFire
read "verdict ok" before any fix, so they leave the list (the slate: attacks already fine are untouched). The test
`N63BossTellTests.TheFixList_IsTheOneComputedFromTheBeforeMeasure` hard-coded n63-b4's (buggy-meter) list and now fails; it is NOT edited
by the lead: builder G makes it compute the list from a committed fixture of the before cue lines with the same rule (test first, its
reason stated). **Rejected:** keeping b4's list (it fixes two attacks that were fine and misses crownFire); editing the pinned list by hand.
Also logged here: V's C2b changed one of its own assertions (N63NearFadeTests' hook test asserted `_Dissolve`, C2's old mechanism; the
screen-door replaces it), stated in its RED commit, as D-6 / D-9.

### D-10 M1's proposal (2) is built tonight as an OFF row, for your A/B (2:12; amends D-2)
**The call:** builder D builds the eased eye (a critically damped follow on the eye ease and the slide dip, the same 0.10 s; view only,
behind a new row eye63, OFF) and measures it with Tools/night/n63-m1-chop.cjs on verbs37 telemetry on vs off (want each slide / landing
event's peak residual halved, the slam and rail swap excluded). It turns ON only by the night's rule (a SPOT naming it and a GATE PASS);
otherwise it waits OFF for your blind A/B (L-001). **Why the change from D-2:** a builder was idle with the boss list in hand; an OFF row
ships nothing, and the measurement exists. **Rejected:** a slower crouch (a different slide).

### D-9 C1's fix round changes a routing rule its own tests pinned (2:10)
**The call:** accepted as a spec change, as D-6 for V: builder D's C1b (n63-C1b 4134e403) replaces two of tonight's own brightness
assertions (C1's gold -> danger and B3's routed parts) because they pinned an overflow: a route kept the original's brightness and pushed a
channel past full (x1.8), so the screen clipped the lava's "danger" gold back to amber. The new rule keeps the original's brightest channel
when the plain rule would overflow; a new test pins it. Stated in the RED commit; the C1 and B3 GATEs are told. **Not a fix for:**
iceline's blue and spire's teal: they are lit surfaces (the level's key light), not routed source colours; moving them means re-lighting
the levels (a desaturated key light: "bland"; or a role's hue: a false meaning). So C1's MAIN can reach 3 of 5 levels at most.
**GG:** iceline's blue light and spire's teal under its exit: keep (the level's identity) or re-light toward the palette?

### D-12 B1's fix round raises THE WICK's guard and changes two of tonight's own motion pins (4:08)
**The call:** accepted as a spec change, as D-6 / D-9 / D-11: builder V's B1d (n63-B1d f3f09659) gives THE WICK its own stagger slump
(sink 0.85 m, keel 40 deg, squash 0.30, a new 1.19 m slide sideways the way the top keels) under a guard raised from 0.6 to 0.9 of its
1.7 m collider radius and from 30 to 45 deg, because B1's GATE (Logs/n63/verdict-B1.md) found every boss's slump pinned at the old guard:
the cap, not the design, set the size (wick stagger 40,503 px, under rule 8's 41,472). Two assertions in N63BossMotionTests (V's own pins
from tonight's pick 2) change with it: the guard's range per boss (share up to 0.95, tilt up to 45 deg) and the wick's "sink >= 0.9 m"
(now sink >= 0.7 m and sink + slide >= 1.2 m in the screen's plane); "within the guard" and "the offset within THE WICK's limit" stay
pinned. Stated in the GREEN commit and in the test's comments (not the RED commit: noted), the B1 GATE is told. **Rejected:** keeping the
0.6 cap and a still slump (the GATE's FAIL as it stands). **For GG:** the drawn centre stays inside the collider, but the cage reaches past
the hit sphere on the slid side (as it already did at 0.6): a shot at the drawn cage edge can miss. Not checked in a Player.
**Added 4:44, bossMotion63 ON (B1 GATE round 2 PASS, Logs/n63/verdict-B1.md):** the critic accepted D-12 with three problems: the
builder raised the guard that caps its own slump; the widened test range applies to every boss; and the GREEN commit also fixed a third
count in its own new test (6 -> 5 damage events, the RED commit's miscount), which this entry first left out. **Open for you:** (1) during
THE WICK's OPEN window the drawn orb can sit up to 1.53 m from the centre of its 1.7 m hit sphere (s14 / s15 read the full cap), so a
shot at the drawn orb's edge can miss: "a shot finds what you see" is not checked in a Player; if you feel it, `-skygrindNoBossMotion63`
turns the layer off; (2) THE KILN WYRM barely moves on screen (its drawn boxes are its hitboxes, so its guard is 0.375 m: flinch 1,076 px,
stagger 16,649 px at 29.4 m); THE LOOM's stagger 40,650 px (under rule 8's bar; not its SPOT pose); (3) the same-run reel mode changes
the reel's own fight (the crown takes 35 damage events in it vs 49 in a plain run): a reel artefact, the plain run reads 49 / 49 drawn.

### D-13 B3's fix round restates three of its own hook pins (4:15)
**The call:** accepted as a spec change, as D-9: builder D's B3c (n63-B3c ab4d391f, a commit of its own before the fix, reason stated)
changes three of tonight's B3b hook assertions: the wyrm's open head and core go through the grade's cyan compensation (LitRim / PartRim)
because B3's GATE (Logs/n63/verdict-B3.md) found the wyrm's weak point never cyan under the fight's sulphur grade, the punch window's head
joins them, and the crown's tell is exactly 5 sites (D's own RED commit 904d3b21 miscounted 6: the NIGHT38 gem cubes never take the tell
colour). The fix (d3e37b78): a danger route keeps its brightest channel and caps the others, so an HDR gain no longer clips the crown's red
tell to amber (hue ~37); the per-boss role meter (Tools/night/n63-b3-roles.py) judges each boss's changed pixels by role. **Rejected:**
keeping B3b's wrappers (the weak point stays lime under the grade). **For GG (D-8 again):** the crown's gems now share the pass ring's
cyan, so "hit this" and "go through this" read as one shape.

### D-16 The close at CAP: no freeze build; Builds/n63-b13 is the play build; the tag goes on the close commit (9:05)
**What happened:** the usage limit (a monthly spend cap, HTTP 429) stopped the session from ~05:23 to 07:41, so the close window
(7:00-8:00) passed with nothing run; at 9:05 the clock guard allows no build, reel, smoke, test or agent. **The call:** do NIGHT63.md
section 9's "at 8:00 log what is left under Blocked, then (h)": the final flow gate, the n63-freeze build, its verbs37 and the smoke are
logged under Blocked (NIGHT63-LOG.md); `u3-n63` tags the close commit. **Builds/n63-b13** (built 03:50:39 from 0848b181) is code-identical
to the tag (every later commit is a doc or a log) and carries verbs37 x5 27 / 29 / 31 / 31 / 31, NRE 0: play that one. **Rejected:**
running the build, reels or smoke past CAP (the guard refuses them, and the night's rules end at 8:00); tagging without a build at all
(b13 exists and matches). **Not checked:** the smoke ("DONE: all steps passed") on any NIGHT63 build; gg-proxy's close ranking.
**GG:** if you want the smoke and a freeze build before you play, the next session runs `bash Tools/night/n41-build.sh n63-freeze` and
`bash Tools/night/n56-smoke-steal.sh n63-freeze` first.
**Added 08:18 (after the close, on your "continue"):** done. Builds/n63-freeze from the tag: verbs37 x5 27 / 29 / 31 / 31 / 31, NRE 0;
the smoke "DONE: all steps passed" with both planted focus steals taking the foreground; the flow gate "gates: all GREEN 08:17:31".
**Play Builds/n63-freeze** (it replaces b13 as the play build; same code). The tag stays on 0ec78408.

### D-15 bossTell63 ON (B2 PASS), a narrow pass with its critic's open points (4:54)
**The call:** bossTell63 turned ON by the rule (Logs/n63/verdict-B2.md, on n63-b10). What it does: on the two attacks the before-measure
found unreadable (THE KILN WYRM's spit: no cue before, 225 ms of a pale riser; THE CROWN's crownFire: its strongest cue a pale warm white,
hue 41), a red-orange see-through column rises around the boss through the wind-up and the boss's own white-hot glows are pulled to red;
every other attack is untouched (hit frames and cue lines identical before vs after on all five bosses). **Open, for you (the critic's
points, not fixed tonight):** (1) the spit's fix is never on screen in a reel frame (the wyrm's tell shot faces a wall: 12 px); (2) the
spit's cue clears the 300 ms bar by 125-192 ms only (the column fades in over the first ~400 ms of a 900 ms wind-up); (3) crownFire
already had a faint red cue 1,283 ms early before the fix (a dashed ring); the fix makes the strongest cue red; (4) the wyrm frame shows
"STAGGERED: HOLD LEFT CLICK, SLUG IT" during the spit's wind-up (the stagger ends at 23,600 ms, the wind-up starts at 23,592): not B2's
change; **cause found (builder G, 5:00, Research/2026-10-22-wave63/G-FIX.md): the sim's own state**: the hint reads
`wyrm31.Staggered` (SurvivalSim.N43.Bosses.cs:73; Wyrm31.cs:123), and the worm keeps telling and spitting while staggered (its attack
log never reads the stagger); the reel's "stagger end 23,600" is BossAttackClock63 letting the pending spit outrank the stagger (D-6's
B1 fix). **For you (enemy design, rule 7):** should a staggered worm spit at all, or should the hint give way to a dodge cue while a tell
is pending? Nothing changed; (5) the meter reads drawn colours, not pixels (no particles, no vertex colours). **Rejected:** holding the flip
(every doneWhen line met as written). **GG:** if the red column reads as clutter, `-skygrindNoBossTell63` turns it off.

### D-14 text63 ON (T63 PASS round 2), with its critic's open points (4:40)
**The call:** text63 turned ON by the rule (Logs/n63/verdict-T63.md, round 2 on n63-b10): on kiln one lesson per technique (the first
wording the run reaches teaches it; "Midair ...", "Fire ... lift off" and "Boost ... twice" withheld; the step lines kept), no lesson
dropped while it waits for a room title, no "(LATE" line in verbs37 x5. **Open, not fixed tonight (the critic's new points):** (1) the
queue is not cleared on entering a new room, so a lesson could show in the next room or after a boss fight (not seen on b10; not checked
in a Player); (2) "still waiting at run end" has never printed in a Player (silent when nothing waits); (3) the Midair withholding logs
twice in one run; (4) which wording a player gets depends on their pacing (not checked); (5) the pair shows the withheld repeat, not a line
waiting for the title. **Rejected:** holding the flip for these (both doneWhen lines met as written). **GG:** if a lesson ever shows in
the wrong room, `-skygrindNoText63` turns the row off; tell me where.
**Added 5:15, a fix tried and reverted:** builder G's T63c dropped (with a log line) a queued lesson when its room is left or a boss
fight begins. On n63-b12 it lost iceline's only gauntlet lesson ("One LEFT CLICK swing drops a tank.", shown on b10), so it is reverted
(f93c2de0, 829b8394) and text63 ships as its GATE judged it on b10. The trade for you: a lesson that waited too long either shows in the
next room (tonight's text63) or is dropped (T63c). Which do you want?

### D-7 text61 ON (P3 PASS), with its critic's open points for you (1:35)
**The call:** text61 turned ON by the rule (critic PASS, Logs/n63/verdict-P3.md; judged on a two-run pair with the hint pinned, so the two
runs differ only by text61). **Open, not fixed tonight:** (1) its 3-second fallback still stacks an instruction under a room title when the
title outlasts 3 s (4 times in the verbs37 runs); (2) "once per verb" is really once per line of text: kiln still teaches 10 lines in a run,
several the same aim-down air shot in different words, so "way too much instruction" is only partly answered; (3) kiln's card shows once
(kb1) and never again in the level. A text night would cut the kiln lines to one per verb. **Rejected:** holding the flip (its doneWhen is
met as written).
