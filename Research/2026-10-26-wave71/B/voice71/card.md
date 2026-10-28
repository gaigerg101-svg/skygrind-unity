# voice71 (B1, design.n71.voice71; view row, OFF in the repo, ON in the play build) - code cfa64c526
**Visible change (one sentence, for the spotter):** while THE CROWN's window is open and the fight line or the boss bar says the window order, the cyan "OPEN: SHOOT IT" under the crosshair is gone (one order on screen, not two).
**Why:** T-155, gg-proxy rc2 (two orders at once on bp/crown37-stagger); GG 2026-10-14 "way too much instruction". No arbiter saw call40.
**Pieces:** Voice71.cs (switch, rule, counter), UiVoice71.cs (reads what the fight line / bar hint draw this frame), Crown37View.Voice71.cs (the
last write on call40, from Crown40Draw in LateUpdate, after the HUD's frame). "BLOCKED" never hidden; the call stands when no HUD order is drawn.
HUD look untouched (no font, size, colour, place or fade change). No sim, no Core: fingerprints unaffected by construction (not checked by me).
**Off:** the row off, -skygrindNoVoice71, -skygrindOff71 (or older, e.g. the play card's -skygrindOff65).
**Proof (the lead runs it; a build whose config has n71.voice71 ON, e.g. the play build):**
`bash Tools/night/n71-reels.sh <tag> vc,n52-spire,boss63,-skygrindSameRun63,n71.voice71 vcoff,n52-spire,boss63,-skygrindNoVoice71`
Read `[voice71] crown both drawn N frames` in Logs/player/n47-reel-<tag>-vc.log / -vcoff.log. GREEN: vc N = 0 with `would both` > 0
(the red in the same run) and `held ... both drawn` > 0 (the pair's off halves); vcoff N > 0. The pair: vc-on/ vs vc-off/ B63-crown-stagger.
(boss63 forces THE CROWN's stagger at 14 s if none came: a window inside the 25 s hold.) The verbs37 guard on the play build: unchanged.
**Numbers:** dn-check 0 errors (B/voice71/dn-check.txt). Player numbers: not checked (NEEDS-LEAD).
**EditMode:** N71VoiceTests (6 tests; not run by me).
