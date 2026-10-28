METER: SOUND WITH A BOUND fold a 4-connected yellow component (rects in, hud out, whole frame) only when >= 90 % of its pixels lie within TELL_PX (24 px at 1080p) of an enemy / boss rect, then fold only its part outside the rects

# NIGHT60 B3 review: Tools/night/n60-v-yellow.py (dee75beb), fresh reviewer (L-106), 2026-10-22

Read-only: no git writes, no builds, no Players. Scratch scripts (not in the repo) were run from the session scratchpad: rev_tell.py, rev_bound.py, rev_local.py.
They import n60-v-yellow.py / n58-v-yellow.py unchanged. Frames: Logs/feel/n59-b2/{v2off,v2offsp,v2on,v2onsp}/a358 (no swarmer / spitter rects)
and tonight's Logs/feel/n60-b3/{yoff,yoffsp,y85on,y85onsp}/a358 (with the new reporters).

## Checks

1. Planted, re-run (`python -I Tools/night/n60-v-yellow.py planted`): GREEN 8 / 8, the same bytes as Logs/n60/V/yellow60-planted.txt. The four old cases
   match n58's own run: non-parry 0.00 / 0.58 / 1.54 / 0.34 %, fringe 0.54 / 0.54 / 0.00 / 0.55 %, PASS / FAIL / FAIL / PASS. The new ones do what
   their names say: 5 ring folded (0.18 -> 0.00 %, tell 0.18 %), 6 far strip still FAIL (0.76 -> 0.58 %), 7 far part counted (0.39 -> 0.20 %), 8 plate
   not touching stays 0.33 %.
2. The planted set misses the case that matters: world yellow inside or behind an enemy rect. Any yellow inside the rect seeds the fold, including floor,
   wall or gun pixels that show through the box corners around the pill. Adversarial cases on the builder's 960 x 540 frame (rect 80 x 160, 12 px ring),
   from rev_local.py:
   - D, floor band (H 68 S 0.65 V 0.35) behind the enemy: n58 0.56 % FAIL -> n60 0.47 % PASS (tell 0.08 %). The MAIN passes for the wrong reason.
   - B, tell-coloured plate (H 51 S 1.0 V 0.78) behind the enemy: n58 0.69 % FAIL -> n60 0.49 % PASS (tell 0.21 %). Same.
   - C, one enemy in front of a yellow wall: n60 folds 0.74 % of the frame, more than the whole 0.5 % budget, from one rect.
   - A, long floor band: 2.55 -> 2.41 % (still FAIL).
3. The diff's mechanics: no bug found.
   - Radius: round(24 * H / 1080), 12 px at 540p, the same as n58's fringe. Rects are in frame pixels (sidecar w / h 1920 x 1080 = the PNGs).
   - Connectivity: 4-connected, as in n58.
   - No double counting: tell = ... & yel & ~orb & ~fringe, rest = yel & ~orb & ~fringe & ~tell.
   - HUD: out of both yel and yel_all, so nothing connects through it. --sim is honoured in both masks (Y.SIM is global).
   - The builder's n59 numbers reproduce exactly (v2off 5 / 6, v2offsp 6 / 6, v2on 0 / 6, v2onsp 0 / 6).
4. On the real frames, which pixels move from non-parry to "tell" (rev_tell.py, per-frame share; crops viewed):
   - ON frames (8 runs x 6): no verdict changes. The largest fold is spire v5-mid's 0.099 % (n58 0.27 / 0.28 % PASS already). The other folds are
     <= 0.006 %, and they are the real tells: y85on foundry a5-entry, the spitter halo above its rect, 68 px H 53 S 0.98 V 0.84; y85onsp spire v5-entry,
     131 px H 50 S 0.99 V 0.39. Today the MAIN does not pass because of the fold.
   - OFF, world floor and gun plate folded:
     - spire v2-entry, 0.288 % (n59) / 0.339 % (n60). Around the left husk: world floor H 68 S 0.66 V 0.35, in 24 px wide comps (x 633-657, x 1271-1295:
       clipped by the ring). Under the right husk's rect: the gun's yellow plate, 3610 px at x 1319-1555 y 823-863, H 64 S 1.00 V 0.73, inside gun rect
       [1081.9, 668.9, 1009.3, 959.8].
     - spire v6-mid (n60), 0.243 %: the gun plate again, 3890 px at y 882-906, H 51 S 1.00 V 0.70: the 24 px band above enemy rect
       [1286.9, 906.8, 779.8, 1441.1].
   - OFF, the builder's 0.64 % on spire v5-mid is NOT world yellow. It is slices of one giant parry orb (H 58-60 S 1.0 V 0.99) cut by two enemy rects.
     n58 runs the orb finder after the rect mask, so a slice fails the shape test. It is parry yellow, folded under the wrong label.
   - OFF, the real halo: yoff foundry a5-entry, 973 px at x 578-637 y 543-567, H 67 S 0.71 V 0.35. This is the spitter's dim outer halo glow (crop viewed).
5. Colour bounds don't work. With S >= 0.85, hue 44-60 and the gun rect out (rev_bound.py), every OFF world / gun fold goes to 0 and every ON fold is
   kept. But it drops the real halo glow on OFF frames (yoff a5-entry 0.051 -> 0.004 %; the glow is S 0.71, the same colour as spire's floor). By
   construction, it would still fold case B (H 51 S 1.0; not run). Hue alone fails the same way: Tell36YellowHex 0xFFD23C is H 46, rendered tells are
   H 50-54, and foundry's dim world yellow is H 45-48.
6. The bound in the verdict, measured (rev_local.py, FRAC 0.9):
   - Adversarial: A / B / C / D all FAIL again (2.55 / 0.69 / 2.47 / 0.56 %, tell 0).
   - Builder cases 5, 6 and 8 are unchanged. Case 7 changes to ring + strip all counted: 0.39 % PASS, tell 0. This only over-counts, so it can fail the
     row but never pass it. Case 7's expectation must be rewritten.
   - ON frames: every tell fold is kept (y85on a5-entry 0.004 %, y85onsp v5-entry 0.006 %, v6-entry 0.001 %). The v5-mid orb slice is no longer folded
     (0.099 -> 0; the MAIN is 0.27 % PASS either way).
   - OFF frames: the spitter's halo is kept (yoff a5-entry 0.051 %). The world / gun / orb folds shrink: spire v2-entry 0.339 -> 0.048 %, v6-mid
     0.243 -> 0.004 %, v5-mid 0.638 -> 0.000 %.
   - Add B and D as planted cases (both must FAIL).
7. Side findings, not caused by this diff (n58 has them too):
   - Neither meter masks the gun rect, so the gun's yellow plate counts as non-parry wherever no enemy rect covers it.
   - spire v6-mid (yoffsp) reports enemy rects far outside the frame. Two are fully off-screen and clip to nothing. [1286.9, 906.8, 779.8, 1441.1] masks
     x 1287-1920 y 907-1080, 5.3 % of the frame (gun included), out of both meters' count. With the probe cap now 32 and swarmers / spitters reporting,
     such rects may be more common. Whether that enemy is behind or next to the camera: not checked. A sanity filter on rects (taller than the frame, or
     anchor off-screen) is worth a look.
