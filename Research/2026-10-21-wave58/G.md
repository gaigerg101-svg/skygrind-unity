# NIGHT58 builder G: notes (kill55 after the flash, press57's chosen stillness)

Every bot number is n46flow's walk pilot (Walk38Pilot + the ported autopilot, "normal"). I ran no Editor and no Player (builders never do).
Logs: `Logs/n58/g/g2-{off,on}-s{1,11}.txt` (force-added: Logs is ignored).

## G2 press57: chosen stillness measured; the MAIN missed, so the row stays OFF (commit acd27b29)

**The measure (fixed in Logs/n58/slate.json before any number; DECIDE58 D-3).** CHOSEN STILLNESS is the share of combat steps where the
pilot's stick is not pushing (hypot(moveX, moveZ) == 0) AND the player's horizontal speed is under 0.5 m/s.
- Combat steps: n46flow's in-fight steps of the measured rooms (the same steps as r.fight: F56StyleStep -> F56TokStep -> F57PressStep).
- The stick: the frame that drove the Mover's step, after every pilot rule (press57's answer, lava, edge holds). It is read from
  `Mover.input` by reflection, read only.
- Where: `Tools/SimHarness/Harness.N58.Still.cs`, plus two hook lines in my `Harness.N57.Press.cs` (F57PressStep's first line,
  F57PressTable's first line).
- The flag: `n46flow ... --g58` prints one `STILL58` line per level. Without the flag it prints nothing.
- Checks: the off side reproduces kiln-base's SUMMARY line (stall 18.9 %, deaths 2, clear 177.0 s). Fingerprints plain / --off57 / --off58
  are EQUAL.

Command: `dotnet run --project Tools/SimHarness -c Release -- n46flow --level n52-foundry,n52-iceline,n57-kiln --seeds 10 --first {1|11}
[--rows press57] --g57 --g58`.

| bot, seeds 1-10 / 11-20 | foundry | iceline | n57-kiln |
|---|---|---|---|
| **MAIN chosen stillness, off** | 10.1 / 7.9 % | 20.0 / 19.3 % | 6.4 / 5.0 % |
| **MAIN chosen stillness, press57 on** | 7.7 / 9.1 % | 20.4 / 19.2 % | 5.9 / 4.8 % |
| change (want <= -5 points) | -2.4 / +1.2 | +0.4 / -0.1 | -0.5 / -0.2 |
| blocked (stick pushing, < 0.5 m/s), off / on | 11.7 -> 13.0 / 10.8 -> 11.1 % | 6.4 -> 6.5 / 4.9 -> 4.9 % | 5.5 -> 7.5 / 7.7 -> 6.6 % |
| deaths, off / on | 13 -> 11 / 15 -> 6 | 6 -> 4 / 6 -> 4 | 2 -> 6 / 6 -> 1 |
| clear, off / on (s) | 126.7 -> 129.4 (+2.1 %) / 126.6 -> 112.2 (-11.4 %) | 241.1 -> 245.3 (+1.7 %) / 253.0 -> 259.7 (+2.6 %) | 177.0 -> 173.1 / 169.9 -> 160.3 |
| spitter + acid HP per min while MOVING, off / on | 51.9 -> 33.5 / 73.9 -> 41.4 | 28.3 -> 14.7 / 25.8 -> 16.0 | 18.6 -> 6.0 / 27.8 -> 6.8 |

- **MAIN: MISSED on every level and both seed sets.** No level moves more than 2.4 points, and the seed sets disagree in sign on foundry.
  On n57-kiln, chosen stillness is only 5-6 % to begin with, so a 5-point drop is close to impossible.
- **GUARDS held.** Deaths stay inside the seed spread: kiln's 2 -> 6 matches the off side's own 2 / 6. Clear is at most +2.6 %. Hits taken
  while moving go DOWN on every level.
- **Verdict: press57 stays OFF, with these numbers.** The walk pilot stands still by choice on 5-20 % of combat steps, and a press on one
  ranged kind does not change that. What press57 does change, it changes on the moving player: a spitter's hits on a moving player halve.
- Rule 2 of the brief: I made no change inside press57's files, because the MAIN missed on the measure as defined, before any tuning.

## G1 kill55's marks after the flash: the a258 phase is built, but NOT a fix (commit 9a85ba45)

**Cause, as far as the code shows (file:line).** I read this in the code; none of it was run.
- **When the flash ends.** A hit starts the eased flash's age at `UnityTuning.CrunchHitFlashHold - N51HitFlashHoldS` (EnemyViews.Hit.cs:264).
  `CrunchMath.HitFlash(age, CrunchHitFlashHold 0.03, CrunchHitFlashFade 0.1)` (EnemyViews.Hit.cs:356 / 413) then gives white for
  N51HitFlashHoldS 0.07 s (UnityTuning.N51.cs:20), eased back over 0.1 s (UnityTuning.Crunch.cs:134-135). So the flash ends about 0.17 s of
  world time after the hit, about 10 frames at 60 Hz. The flash is white x _Flash51Hdr 2.4 (Enemy.shader:160/169).
- **When the marks fade.** They never fade. A mark stays alive until its husk dies (Kill55Damage.OnKill) or a 4th mark replaces it
  (Kill55MarksPerHusk 3). Its age only steps the hot core / rim's colour from hot to ember over Kill55MarkCoolS 0.35 s
  (Kill55Damage.cs Render, `cool`).
- **So the flash is not the whole story.** NIGHT57's a355 (Logs/feel/n57-r3/summary47.txt:256) read 0.02 % dark mark pixels at +0.5 s with
  the row ON against 0.09 % OFF, long after the flash, while it said "marks laid 3, holes 3, drawn 1". The r3 critic saw "at most a faint
  darker red patch near the crosshair". The marks are submitted, but they do not show as dark.
- **What I checked:**
  - The marks' geometry matches the drawn pill: ShellR = Mesh43.PillMesh_Profile x the body's 0.8 m scale, plus a 0.045 m lift.
  - Their pass is right: the copy disables UniversalForward (the ink hull). The colour pass is UniversalForwardOnly, Enemy.shader:127.
  - The root mapping is HuskView's own order (EnemyViews.cs:498-503).
- **Not checked, and the remaining candidates:**
  - an occluder over the torso (models40 / 43 parts);
  - the grade, fog or bloom lifting the dark (V 0.03-0.09) above the reel's V < 0.12;
  - the depth prepass.
  a258's probe sorts these: the mean V at the mark's projected centre. If V stays the body's, the mark is not seen (occluded). If V drops but
  stays over 0.12, it is the colour or the grade.
- **I changed no look,** because I had no Player to tell these apart. A blind lift or recolour is a guess.

**What is built:**
- `Assets/Scripts/Game/FeelReel.N58.Play.cs` (a258) runs a355's scenario (the chain ladder, debris, spaced kills, the damage victim) with one
  step added after the hit's next frame.
- That step grabs every frame and finds the flash's end on the pixels: the first frame where the white share (S < 0.2, V > 0.85) of the
  before frame's warm body pixels drops under 2 %.
- There it saves `shot-N55-K-flash-end.jpg` and prints:
  - the input: kill55 on, the hit, marks laid, the victim's mark (crack / hole), drawn count;
  - the end frame and time;
  - MARKS AT THE FLASH END (changed, V < 0.12, want >= 5 %);
  - the probe.
- GREEN = kill55 on AND the ladder ok AND MARKS >= 5 %.
- Supporting changes:
  - two hook lines in `FeelReel.N55.Kill.cs`;
  - `Kill55Damage.MarkAt` (each mark's last drawn position);
  - `UnityTuning.N58G.cs` (the probe's thresholds);
  - `-skygrindNoKill55` in `Kill55.On` turns off this row alone, for the off side on the same build.
- Checks: dn-check-wt 0 errors; fingerprints plain / --off57 / --off58 EQUAL.

**Reels (the build copy with n55.kill55 on):**
- `bash Tools/night/n47-reels.sh <tag> g1on,n52-foundry,a258`
- `bash Tools/night/n47-reels.sh <tag> g1off,n52-foundry,a258,-skygrindNoKill55`
- The ladder alone: `k1on,n52-foundry,a355` / `k1off,n52-foundry,a355,-skygrindNoKill55`.
- Frames: shot-N55-K-hit-before, hit-next, flash-end, chain.

**The GUARD a354 jitter WITH kill55 firing: not evidenced.** a354 (FeelReel.N54.cs) never reads kill55. Its KESTREL kills come while running
at 4 m/s or more, and NIGHT57's running gate (Kill55Hold.StepEnded's `running`) asks no extra hold there, so a354 cannot fire kill55's
ladder by construction. A354 would need to print kill55's records (FeelReel.N54.cs is not mine). The C-6 flash budget is unchanged: a258
adds no flash.

## G3 tokens56's idle on iceline: the cause, measured (no code; after the 15:12 pick)

Sources:
- the baseline IDLE line (`Logs/n58/flow-base.txt` / `-B`, tokens56 ON as shipped);
- the drone split from my G2 off runs, which ran with `--g57` (`Logs/n58/g/g2-off-s1.txt` / `-s11.txt`, the iceline PRESS57 line's tail).

| iceline, seeds 1-10 / 11-20 | |
|---|---|
| enemies idle > 20 % of their alive time | 113 of 615 (18.4 %) / 122 of 605 (20.2 %) |
| idle share of enemy-alive steps by kind | drone 32.6 / 32.9 %, warden 29.1 / 30.2 %, charger 13.7 / 16.2, swarmer 10.5 / 12.0, husk 10.2 / 8.8, spitter 6.1 / 7.7 % |
| drone idle steps: holding the ranged token / waiting | 16101 / 19599 (45 / 55 %) ; 22237 / 22599 (50 / 50 %) |
| drone idle steps inside the sim cone (in view) | 21267 of 35700 (60 %) ; 28594 of 44836 (64 %) |
| drone idle steps on their wait ring (within 2 m) | 1027 (2.9 %) ; 780 (1.7 %) |

- **The idle is two kinds: drones (~33 % of their steps) and wardens (~30 %).** Every other kind is at or under 16 %. The 18.4 % over the line
  is mostly these two.
- **Drones are not the NIGHT57 kiln case.** On kiln, waiting drones stalled against a wall OUT of view, and the nearest-node rule fixed
  that. On iceline, 60-64 % of drone idle steps are IN view, where ADVANCE (Advance56) has nothing to do, and almost none are on their ring.
  Half of the idle steps belong to drones that HOLD the ranged token: they hover in place between volleys. So a large part of this "idle"
  is a ranged holder firing from where it stands, which is by design.
- **Next fix (not built):**
  - First, split the meter: is a token holder between shots "idle"? If the holder's hover is excluded, the drone share roughly halves.
  - Then, for waiting drones in view, a strafe along the ring (a brain rule in HuskBrain.N56.cs, behind a new tokens56 number) rather than a
    hover.
  - Wardens need their own read: does the shield's stand count as idle? Not measured.
  - A target of under 10 % over the line needs both kinds to move. Expect a measure change plus a rule, not a number tweak.
