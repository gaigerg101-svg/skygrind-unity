# NIGHT38 builder H: hud38 + juice38 (notes for the lead's frames)

Both rows are view rows and still `enabled: false`. Turn both on for frames: `node Tools/night/n38-hud-config.cjs --on` (writes only the
hud38 / juice38 rows of Design/unity-config.json and copies it to StreamingAssets; `--off` turns them off again). `-skygrindOff38` turns both off.

## What to look at (one cue per frame)

| cue | how to get it | the frame | what it should show | the static to read |
|---|---|---|---|---|
| bar charging | hold fire on the KESTREL (charge35) | ~0.3 s into the hold | three thin bone cells under the crosshair, about half filled | `Hud38.BarState == "CHARGING"`, `Hud38.BarFill` ~0.5 |
| bar full + flash | keep holding to full | the frame `Hud38.ReadyFlashes` goes up (FeelProbe event `hud38Ready`) | the cells white and 2 px taller | `Hud38.BarState == "FULL"` |
| bar at rest | 1.2 s later | any | the cells cyan at 55 % | |
| reload spin | fire the THUNDERCLAP (its pump is over 0.6 s) | ~0.4 s after the shot | the four ticks turned part of a full turn; the bar refilling in bone | `Hud38.SpinDeg` > 0 |
| snap | the pump ends | the frame `Hud38.Snaps` goes up | the ticks square, white, pushed out 3 px | |
| BLAST pips | the gauntlet in hand after 2 kills | any | five cells, two filled | `Hud38.BarCells == 5` |
| grapple | throw one grapple charge | right after the throw; then when it returns | the hook icon popped (x1.35), one pip empty; on return the pip white | `Hud38.GrappleFull` |
| boss entrance | any boss room (BossRoomBegin) | +0.02 s, +0.15 s, +0.6 s after `Juice38Probe.Entrances` goes up | a bright flicker pulse with a tear and a colour split, then the second pulse; the name card (56 px red-orange, above the reticle) and the shake still going | `Juice38Probe.Flicker`, `Ui.Card38Shown` |
| worm dig | THE KILN WYRM's fight | the frame `Juice38Probe.Moves` goes up | a shake and one flicker pulse | `Juice38Probe.LastCue == "boss38Move"` |
| hit taken | stand in front of a husk | the frame after `Juice38Probe.Hurts` goes up, and 0.3 s later | full-screen TV static, a red / blue split, torn bands; fading | `Juice38Probe.Static` 0.5 .. 0.8 |

## Measured (SimHarness, no Editor)

`n38hud cues` (each boss's walk-in through BossRoomBegin, the player standing still): every boss raises exactly one `bossRise` (THE WICK,
THE STAG, THE LOOM, THE CROWN, THE KILN WYRM), so each gets one entrance; the NIGHT36 / 37 resume start after the walk-in raises none.
`crown37Shatter` and the worm's `crack` / `dive` tells were seen; THE WICK's dive, THE STAG's crash (a parry) and THE LOOM's fall (its death)
were not reached by a still player (not checked). Hits taken in those runs: 14 .. 30 damage, so the static runs at about 0.58 .. 0.68.

## Settings that change what shows

- HUD FLICKER off (look30's ACCESS switch): no flicker and no static (the shake stays).
- FLASH INTENSITY 0, or the game's FlashGate full (three flashes a second): no brightness pulse (the tear, split and scanlines stay).
- REDUCED MOTION: no shake and no tear (the static stays). CAMERA SHAKE % and EFFECTS INTENSITY scale the shake.
- The CRT at 0 or its row off: the pass is kept on for juice38 alone (every CRT value 0, only juice38's adds).
