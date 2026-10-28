E1 yellow56: FAIL the GUARD breaks: foundry a5-entry's parry halo keeps 20 % of its yellow pixels with yellow56 on (same-instant twin 4558 -> 918 px; the colour56-only control keeps 91 %)

The critic of NIGHT58 EXT gate X, 2026-10-21 (~20:0x). Read-only; scratch scripts in the session scratchpad only (pairdiff.py, twins.py, halo.py,
halo2.py, halo3.py: they import Tools/night/n56-colour.py / n58-v-yellow.py unchanged). 6 images read.

## The rule, clause by clause
- (1) MAIN: PASS. Frames over 0.5 % non-parry yellow: foundry 6 / 6 -> 0 / 6 (worst 2.45 % -> 0.20 %), spire 6 / 6 -> 0 / 6 (worst 12.82 %
  -> 0.23 %). Read from Logs/n58/freeze/yellow-v2off-foundry.txt, yellow-v2on-foundry.txt, yellow-v2off-spire.txt, yellow-v2on-spire.txt.
  My own same-instant check (each ON frame against its own `-nocap` twin, where yellow56 is hidden too; twins.py): 11 of 12 ON frames read
  0.01-0.23 % on the main frame and 1.62-12.06 % on the twin. The 12th, foundry b1-mid ON, does not test the rule: an enemy's body and a dark
  body fill the view (twin 0.15 %; outside the yellow arc 61 % of its pixels differ from the OFF frame). So foundry is 5 / 5 testable frames
  plus 1 confounded frame, not a clean 6 / 6.
- (2) GUARDS: FAIL.
  - spire v5-mid orbs 17.42 % -> 15.80 % (-9.3 % relative: in the 10 % band, near its edge). Read from the yellow-v2off-spire.txt /
    yellow-v2on-spire.txt v5-mid lines; the two runs' `-nocap` twins agree (13.63 / 13.64 %), so the scene matches.
  - foundry a5 parry halo (the husk's halo at x 560-650 y 540-700; builder V named it in Research/2026-10-21-wave58/V.md as "a parry
    tell: kept"): NOT kept. halo3.py, the halo's connected yellow in the twin vs the main frame, same instant: ON run 4558 -> 918 px (20 %
    kept), its V >= 0.85 core 1750 -> 918 (52 %); the OFF run (colour56's cap alone) keeps 91 % / 84 %. Across runs, main vs main:
    4846 -> 918 px (-81 %). The twin's yellow pixels in the halo box have median S 0.96 there and 0.45 in the main frame (halo2.py):
    the yellow56 cap. Cause, as read: the OFF meter line
    gives this halo's median V as 0.84 (yellow-v2off-foundry.txt, a5-entry, "2415 px at x 560-642 y 583-698 (H 50 S 0.99 V 0.84)"), at the
    new bound "value under 0.84 held, none from 0.89"; V.md read V 0.90 on n57-r3 stills.
  - foundry a5-mid orb 0.18 % -> 0.17 % (yellow-v2*-foundry.txt); its bright blob 3798 -> 3497 px same instant (-7.9 %, halo.py): in band.
- (3) Nothing lost: PASS. verbs37 vf 27 / 27, vi 29 / 29, vk 31 / 31, vs 31 / 31, vp 31 / 31, RED rows [] on each, NRE 0
  (Logs/feel/n58-freeze/summary47.txt) = NIGHT57's vf 27, vi 29, vk57 31, vs 31, vp 31 (Logs/feel/n57-freeze/summary47.txt).
- (4) The critic: does not confirm. The MAIN is real, but part of it is the parry halo itself: the meter counts a halo that fails its orb
  shape / V >= 0.85 rule as non-parry, so desaturating a parry tell lowers the MAIN. None of the 4 planted cases is a dim or hollow tell.

## Planted cases
`python -I Tools/night/n58-v-yellow.py planted`: PLANTED GREEN (4 / 4 as planted): orb + fringe 0.00 %; orb + far strip 0.58 % counted;
strip alone 1.54 % FAIL; strip into the ring 0.34 % far part counted. Each printed as designed.

## Re-run
`python -I Tools/night/n58-v-yellow.py Logs/feel/n58-freeze/v2on-spire/a358`: last line "NON-PARRY YELLOW: frames over 0.5 %: 0 / 6 (worst
0.23% ...spire-v5-mid.png)", identical to yellow-v2on-spire.txt's last line (its v5-mid line identical too).

## Whole-frame tint (pairdiff.py, ON vs OFF main frames)
None measured on the 11 comparable pairs: outside the yellow arc 40-76 deg, 1.5-7.2 % of pixels differ by more than 12 and the mean
|difference| is 3.7-8.8; mean R and G agree within 0.6, mean B moves -0.1 to +4.8 (the largest on spire: the desaturated yellows). The b1-mid pair differs by game state,
not a tint (image lines below).

## Images
- Logs/feel/n58-freeze/v2on-foundry/a358/a5/shot-N56-n52-foundry-a5-entry.png: lava trench edges orange, no yellow line; the left husk's
  halo reads orange-gold with a pale outer ring; no frame tint.
- Logs/feel/n58-freeze/v2off-foundry/a358/a5/shot-N56-n52-foundry-a5-entry.png: lava trench edges with yellow lines; the left husk's halo
  bright yellow, ring and fill. Same pose as ON.
- Logs/feel/n58-freeze/v2on-spire/a358/v5/shot-N56-n52-spire-v5-mid.png: two big yellow parry orbs kept; the THUNDERCLAP's top plate
  beige and its front triangle white; walls olive-grey; no frame tint.
- Logs/feel/n58-freeze/v2off-spire/a358/v5/shot-N56-n52-spire-v5-mid.png: the same orbs; the gun's top plate and triangle yellow; the
  walls and floor a little more olive-yellow.
- Logs/feel/n58-freeze/v2on-foundry/a358/b1/shot-N56-n52-foundry-b1-mid.png (my choice): a red-orange enemy body fills the right half and a
  dark body the centre; most of the room is hidden. This frame's 0.00 % comes from occlusion, not from the rule.
- Logs/feel/n58-freeze/v2off-foundry/a358/b1/shot-N56-n52-foundry-b1-mid.png: the room visible (stairs, lava floor glow, an enemy at the
  right edge with a yellow band); the different game state is why the b1-mid pair differs.

## Not checked
The gun plate's and triangle's role (the a358 line says "gun plates moved off the yellow arc 3"; I did not check whether any is a tell);
iceline / kiln / span frames (no a358 yellow ON / OFF pair for them in this extension); seed spread for the halo (one run per side).
