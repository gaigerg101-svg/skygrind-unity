# NIGHT68 builder T: the instruments' open points (for the next forge)

Line numbers are at n68-T c70838aa. Minutes are code plus the planted pair, with no critic. Each fix changes a pilot or a refusal, so the
reds it touches must be run again (L-150).

1. **The parry-swing leak** (g7 melee only, seed 8, both refused runs). Harness.N68.Glob.cs:22-62 tests only the globs alive at the press.
   Harness.N65.Verb.cs:788 then lets the swing through. It misses and opens parry42's orb buffer, which catches a glob 15 steps later
   (Logs/n68/g7leak/seed8-trace.txt). Fix: also refuse a gauntlet press while a spitter in range is in its spit tell, or its glob cooldown
   ends inside bufferS. Planted: the seed-8 trace shows 0 sent back. 40 min.
2. **A launched body that dies in a pit or in lava is not a launch kill.** Harness.N65.Verb.cs:953 counts only Launch36BurstEvent's
   kills. Fix: keep the launched ids from Launch36.LaunchEvent, and count a KillEvent on one of them inside its flight (or 1 s after) as a
   launch kill, with its weapon (lava, the kill plane). Planted: kb3 r1's pit with a body put on the throw line. 30 min.
3. **The parry pilot presses once per episode.** Harness.N65.VerbPilot.cs:521 (`parryThrown`). In g7, 7 of 27 swing presses did nothing,
   because they landed mid-cycle of the walker's own swings (cycle 0.42 s; verdict-g7tool.md:90). Fix: press again each step while the glob
   is in reach and not reflected, until the episode's 6 steps run out, or wait for the weapon's cycle. The pilot under-states the verb
   today (a conservative bias). 20 min, plus the g7 pair (5 min).
4. **The ride fallback on rooms without line65 holds the trigger.** VerbPilot.cs:398 arms again 4 m from the trigger point, and the ride
   itself covers 4 m. Shipped g8 held the trigger 0-4537 steps a seed (deaths 9 vs 4), so g8's red of 0.63 is held down by the pilot
   (verdict-I1.md:78-90). Fix: arm again only after the feet leave the face's strip (more than 4 m out, or past its ends), as the line run
   does (:390-396). 20 min, plus the reds of every ride room without line65.
5. **Exercised seeds depend on the walker's route.** VerbPilot.cs:379-399 triggers only on the line's run, and the pilot never walks to it.
   kb3c is 7/10 and g8 5/10; on main g8 is 10/20 at r1, so its M2 VERB is UNSURE by construction. Separately, g8 seeds 7 and 9 leaned on the
   line face's own box for 485 and 284 steps with 0 triggers (verdict-I1.md:70-75). The cause is not checked: outside the run window, or
   above top - 1.5. Fix:
   - (a) Print why each step near the face did not trigger (10 min).
   - (b) An approach episode: when the run's start is within 12 m and the grid path to it is clear of lava, walk to it, then ride (3 s at
     most). The refused runs skip it. 60-90 min.
6. **The punch is refused within 8 m of any glob.** Harness.N65.Verb.cs:765, 778 (`GlobInReach`, `V65ParryRefuseM`). The same over-wide
   rule g7 had for the swing: a parry room with a gun drops every punch near a spitter. Fix: the punch through V68GlobCatchable, with item 1
   applied. 15 min, plus the parry rooms' reds.
7. **The gauntlet pilot** (code-scout.md (e)): rooms g1-g4 hold the gauntlet alone, and the launch / parry pilots punch, which is dropped
   there. Fix: hand-aware episodes (launch = dash then swing; parry = swing or LT at a glob or drone orb); a `blast` verb (altFire when
   PulseCharge27 is ready and N bodies are in its zone); V65Filter refusals for blast and the orb parry; counters (swings,
   swingReflects40, ltParry31Return, pulse47Blast); `ride --swing`. 120-150 min, plus the g1-g4 reds.
8. **The launch bound only rises.** VerbPilot.cs:209, 305 (the run's largest non-parry punch). One large FUSION or tag-boosted punch early
   would put every 40 hp husk under it for the rest of the run. Not seen tonight (the skips print bound 30). Fix: the bound per target from
   its kind's punch history, or the median of the run's non-parry punches. 20 min.
9. **The I2 parry hold inside the pilot's 0.8 s episode** (VerbPilot.cs:453). A held punch can time out without being pressed. This was not
   measured on a skip v2 launch room; the I2 fix round ran the probe only. Fix: first measure the episodes that end held, on kb3 / v2
   at r1 (15 min), then let a held episode run to 1.5 s (10 min).
10. **The M3 probe tracks only its chosen target.** Harness.N65.Verb.cs:284. On shipped kb3 seed 3 the punch went to another husk, which was
    launched, and the line reads "no punch landed on the target". Fix: follow the body the punch's HitEvent names, and print both. 15 min.
11. **The glob test's path is a straight line.** Harness.N68.Glob.cs:62 ignores gravity; the 0.5 m margin covers about 0.4 s. An LT press
    opens a 0.25 s window (LtAlways31.cs:6 parryWindowSeconds), longer than the 0.15 s buffer the test looks over. Fix: test altFire over
    max(bufferS, parryWindowSeconds), with the glob's gravity from the spitter's config. 20 min.
12. **The ride refusal's count is folded into "the inner pilot's presses dropped".** Harness.N65.Verb.cs:730 adds the wall contacts to
    refusedPresses, so `exercised` (b.refused > 0) is met by any wall contact (verdict-I1.md:96-98). Fix: count the contacts on their own,
    and let a ride seed be exercised when the refused run blocked a contact the verb-aware run rode. 15 min.
13. **flow-V (n46flow --verbs65) on the ride rooms now runs under noride outside the line episode.** It was never compared before and after
    I1 (verdict-I1.md:99-102). Fix: one before / after flow-V on kb3c and g8 at r1. 10 min.
14. **The noride slab's margins have no planted geometry case.** Harness.N68.Ride.cs:121 (-0.3 / +0.5 m). Fix: a pure case over
    Mover.NoRide38's probe point (the capsule's start, less normal x radius) at the face, 0.4 m out, and behind a thin wall's other face.
    25 min.
15. **I3's bridge bound rises for feet at 0-1 m.** Harness.N68.Lava.cs:32 uses max(1, feet + 1). With the feet on a 0.5 m step, a box top
    at 1.5 m counts as a bridge. No walk showed it (only iceline changed). Fix if a room shows it: 1 m for feet at or under 1 m, else
    feet + 1. 5 min.
16. **Seen, not the instruments':** v2's stall (24.3 % vs its 19.0 ceiling) is not the deck holds (I3 cut them 1860 -> 227 steps; the
    stall moved 0.4 pt). Shipped kb3 has no pit (a forge error). A dash punch into an attack-ready husk was a parry on 4 of 6 kb3 runs: a
    launch room should offer bodies outside their attack window (a design point for R / GG, not a pilot one).
