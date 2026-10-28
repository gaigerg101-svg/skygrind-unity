# Measuring flicker / shimmer / AO crawl with a moving camera (scout)

No standard "TAA flicker" metric or threshold turned up. NVIDIA FLIP is stills-only ([NVlabs/flip](https://github.com/NVlabs/flip): "image error metrics", no video mode found). Below is my synthesis of three published pieces: warping error ([Deep Video Prior, Eq. 6-7](https://ar5iv.arxiv.org/html/2010.11838)), a flicker penalty ([WCS](https://arxiv.org/html/2508.00144)), and a sign-reversal flicker test ([US6154258](https://patents.google.com/patent/US6154258A/en)). Items marked (mine) have no source.

## Recommended: motion-compensated flip-flop test

Input: both runs, same frame indices, PNG to display-coded luma Y in 0-1.

1. **Motion.** RAFT flow t to t+-1 (WCS names RAFT). If the replay can dump depth, reproject with the known camera instead: exact, and no flow noise on flickering frames (mine).
2. **Validity mask M.** Keep pixels passing the forward-backward check |w~+w^|^2 <= 0.01(|w~|^2+|w^|^2)+0.5 and the motion-boundary check |grad u^|^2+|grad v^|^2 <= 0.01|w^|^2+0.002 ([Ruder et al., Eq. 6-7](https://arxiv.org/html/1708.04538)). Also drop off-screen samples and dilate 2 px (mine).
3. **Residuals.** Warp t-1 and t+1 into frame t: r- = Y_t - W(Y_t-1), r+ = W(Y_t+1) - Y_t. Divide by max(Y, A0), A0 about 10 % of max (20-25 of 255): the patent's guard against dark-level blow-up, which matters at dusk.
4. **Flip-flop.** Flag a pixel when sign(r-) != sign(r+) and min(|r-|,|r+|) > tau. That is a thresholded motion-compensated second difference, |W(Y+) - 2Y + W(Y-)|. Opposite sign means "changed, then returned" ([patent](https://patents.google.com/patent/US6154258A/en)); it targets 3-30 Hz with delays up to 13 fields. Repeat at gaps 2 and 3 for slower flicker ([ReVQ](https://arxiv.org/html/2510.13349v1) uses gaps 1-3).
5. **Intended motion out.**
   - Fast particles fail the step-2 check and drop out.
   - A third replay with the VFX layer off; mask pixels where it differs from the full run (mine).
   - Require 2 or more flips in a 6-frame window; fades and slides are monotone (mine).
6. **Report.** Per frame: flagged fraction F_t and mean flagged amplitude. Over the path: p50, p95, max, plus a heat map of flagged-frame counts. Split by band (mine): low-pass (sigma about 6 px) flicker is AO crawl or lighting; the high-pass remainder is specular or edge shimmer. Headline is the candidate/baseline ratio.
7. **Scalar cross-check.** WCS's Flicker Penalty: mean over transitions of ||I_t+1 - warp(I_t)||_1 / ||I_t+1||_1. It suggests clamping or static regions only ([WCS](https://arxiv.org/html/2508.00144)).
## Fallback: no flow, block flip-flop (mine)

Downsample Y to 8x8 block means. A smooth pan changes block means near-linearly, so d2 = Y_t+1 - 2Y_t + Y_t-1 stays small, while a flip-flop gives |d2| about twice its amplitude. Flag blocks where |d2| > tau and consecutive first differences have opposite signs. Weak on fast pans.

Perceptual guard: `cvvdp --test cand_%05d.png --ref base_%05d.png --fps 60 --display standard_fhd` ([ColorVideoVDP](https://github.com/gfxdisp/ColorVideoVDP)); JOD 10 means identical. It has sustained and transient temporal channels ([paper](https://ar5iv.labs.arxiv.org/html/2401.11485)) but scores every difference, so it is a guard, not the verdict.

## Thresholds

- **Sourced:** the flow-check constants, A0, the 3-30 Hz band and the gaps 1-3, all above.
- **Unsourced:** ReVQ's detector is learned and gives no numeric cutoff. Start at tau = 0.02 relative, 2 flips in 6 frames, then calibrate with two controls: (a) baseline vs baseline, the no-op floor; (b) a copy with planted +-3 % alternating luma, which must fire. Set tau under the planted amplitude and over the no-op p95. Ship on the ratio, not an absolute number.
- Visibility depends on contrast and spatial frequency, not only flicker frequency ([Denes and Mantiuk 2020](https://www.cl.cam.ac.uk/~rkm38/pdfs/denes2020flicker_model.pdf), from its abstract; full text not read).
- Add thin-geometry crops as ROIs: DICE found AO flicker concentrates there when the camera moves ([EA](https://www.ea.com/frostbite/news/stable-ssao-in-battlefield-3-with-selective-temporal-filtering)).
