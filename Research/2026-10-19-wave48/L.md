# NIGHT48 builder L: research lines (G36 / G37 THE LOOM's look and gait, G43 the boss flame's pair, kneeHold47)

Each line: Verified (the page was read tonight) or Reported (seen only in a search summary or a NIGHT47 line). Cited from the code as
[L48 §n]. NIGHT47's lines (Research/2026-10-18-wave47/L.md) still stand; these add to them.

## 1. The shuttle's shape (G36: "a long pointed bronze shuttle with brass bands")
- Verified (https://en.wikipedia.org/wiki/Flying_shuttle): the flying shuttle's ends are "bullet-shaped and metal-capped"; the weft is
  wound on a pirn, "a long, conical, one-ended, non-turning bobbin". Used: loomHull48's spindle with brass caps on its points
  (Hull48CapShare) and the brass hub at its middle (the pirn / spool the threads come off).
- Verified (https://en.wikipedia.org/wiki/Shuttle_(weaving)): shuttles are "thrown or passed back and forth through the shed, between the
  yarn threads of the warp", i.e. ACROSS the warp. Used: the hull now lies across the well (world x), not pointed at the player; NIGHT47's
  hull faced the player and was seen end-on on every frame (read: Loom37View.Look47.cs `face = LookRotation(fwd)`; the r2 frames).

## 2. The gait (G37: "the stepping legs judged on a motion strip")
- Verified (https://digitalproduction.com/2026/08/28/spider-walker-automates-multi-leg-animation/, the Spider Walker add-on): the body's
  velocity is estimated each frame and each leg's target "shifted ahead of the body" (its Prediction Distance); a step begins when the
  planted foot "may drift from its current target" past Step Distance; the swing has "minimum and maximum step times". Used: the lead
  (Hull48Lead) and the quick step (a step's time shrinks with its goal's speed, never under a minimum).
- Reported (search summary of a footstep-prediction slide deck, scribd "HowToWalk"; not opened): predict where the foot lands from the
  time to the next footfall; damp the prediction when the speed changes. Used as the reason the lead's velocity is smoothed
  (Hull48LeadSmooth).
- Reported (search summary of SIGGRAPH 2021 talk "The Right Foot in the Wrong Place", Heuvel; not opened): future footsteps predicted by
  advancing the path by the distance travelled before the next landing.
- Measured tonight (n48loom gait, a model of loomLook47's own step rule on the sim's foot goals): the goals move far faster than the gait
  can follow (phase 3 p99 32 m/s, max 37 m/s vs the gait's own pace 1.5 m / 0.35 s = 4.29 m/s), so a planted foot can trail its goal
  by up to 30.8 m (5 % of planted frames over 2.5 m). The lead alone does not fix it (4-7 %); the leash (a foot more than 2.5 m behind
  steps even while the other group lifts) takes it to 0.00 % on 10 seeds. The numbers are in the report and the commit.

## 3. The hit flinch and flash (G37: "a hit flinch")
- Reported (NIGHT47's uhiyama-lab line, Verified then, not re-read tonight): flash 80 ms; hitstop tiers. Used: Hull48FlashS 0.08.
- NIGHT47 section 3 (read): "a hit reaction on every hit (a 60-100 ms flinch spring)". Used: a 6 Hz, damping 0.4 spring (its
  out-and-back half cycle ~91 ms, its peak ~33 ms after the kick: worked from the spring's damped frequency, not measured on a frame).

## 4. The flame pair (G43 boss part)
- DECIDE48 D-11 (read): a pair is "judged on the same moment, frozen at the same frame". Measured on NIGHT47's r3 frames (read the PNGs):
  L47-fire-sweep and -sweep-B were two different sweeps, and in -B the player stood in the cone (the hurt vignette over the frame), seen
  from 9 m in front of the ring (the ring fills the frame). So a548 freezes one sweep (Crown37.Frozen) and takes A then B on it, from
  16 m to the side of the cone.

## 5. kneeHold47 (R4)
- NIGHT47 L.md (Reported there): Celeste's forgiveness windows. Measured tonight: see the report (n48loom hold).
