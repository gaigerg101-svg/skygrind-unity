B18: PASS
(1) MAIN: the row on reads DONE True and the row off reads DONE False, in the same build (Builds/n60-b8).
   - ON, Logs/n60/b9/focus-n60-b8-on.txt and Logs/player/n60-focus-n60-b8-on.log: "11.34 s flow Playing -> Paused (cursor None, focused False)", focus losses 1, pauses 1 (1 with the window unfocused), "DONE: paused on the focus loss True".
   - The lead's manual run with the steal 8 s after the launch agrees (Logs/player/n60-focus-n60-b8-on8s.log): Paused at 11.66 s, pauses 1, DONE True.
   - OFF, the same build with -skygrindNoFocus60 (focus-n60-b8-off.txt and its log): "14.41 s focus LOST (flow Playing, cursor Locked)", cursor-lock drops 0, pauses 0, DONE False.
   - The old game reads the same as OFF: Builds/n60-b7 (focus-n60-b7-green.txt) shows focus LOST at 10.67 s with the cursor Locked, pauses 0, DONE False.
   - Noise: the measure is binary and every steal reached the Player ("took True", and each Player log has "focus losses 1"). ON paused 2 of 2 runs; OFF paused 0 of 2 (n60-b8 off, n60-b7 green). **The MAIN is met with no overlap.**
   - The MAIN was fixed before any number. Its text is in focus60's enabled_why, added in 0d3589c8 at 05:29:02; the runs were at 05:58-06:01.
   - The caveat: the first three n60-b8 runs (05:50-05:53) read "focus losses 0". Those runs missed the window; they are not counter-evidence. Their files were overwritten, so I could not read them. The script now prints MISSED in that case (228e860b), and none of the four runs above is a MISSED.
   PASS.
(2) GUARDS:
   - Planted (focus-n60-b8-planted.txt and its log): the start line reads "PLANTED ... focus60 row on". The run reads "set back to Playing by the planted probe 1", pauses 0, DONE False. So the probe can say False when the pause is undone.
   - The smoke with the row on (Logs/n56/smoke-steal-n60-b8.out and Logs/player/n56-smoke-steal-n60-b8.log on Builds/n60-b8): "228.7s DONE: all steps passed", both steals "took True", 0 "Exception" lines. SmokeGuard55 resolved the row's pause: "52.9s n55 guard: a pause the smoke did not make ... resolved: flow Playing, focused False, cursor Locked, round 2 Fight". The summary reads "dropped cursor lock 1, real pad START 0, MODS draft 0, reward reveal 0".
   - **Weak spot in the smoke guard:** it tested one steal, not two. After the first steal the Player never got the focus back: "foreground pid now 148880", and the summary reads "focus changes 1, unfocused 59858 of 80209 frames". So the second steal (+208 s) took the foreground from pid 148880 ("foreground pid before 148880"), not from the Player. It found no new focus loss.
   - The guard as written ("DONE", each steal "took True") holds. What the smoke proves is that it survives one focus60 pause. It does not prove it survives two.
   - NREs: 0 "Exception" lines in all ten Player logs I read (4 probe, 5 verbs37, 1 smoke).
   PASS, with the one-steal note.
(3) Nothing lost:
   - verbs37 x5 on Builds/n60-b8: vf8 27/27, vi8 29/29, vk8 31/31, vs8 31/31, vp8 31/31. RED rows [] and "NRE count: 0, exceptions: 0" on all five (Logs/feel/n60-b8/summary47.txt lines 70-73, 112-115, 154-157, 196-199, 238-241).
   - Those blocks ran 06:04-06:10, with the row on in the build's copy. No block header carries -skygrindNoFocus60 or -skygrindOff.
   - By design the reels skip the rule (FeelReel.Running), so verbs37 cannot see the pause. It does show the build boots and plays every verb with the row's getter running each frame.
   - Gates (Logs/n60/gates-b9b.txt, all GREEN 05:40:21, after c6a6e196 and 0d3589c8; 224d8338 changes a comment only):
     - config copies identical
     - golden 0 failing, 0 mismatches, 22 replays exact
     - fingerprints EQUAL at the base and at --off57 / --off58 / --off59 / --off60
     - Editor 0 errors
   - focus60 has no tape bit (N60FocusRowTests pins it), so the sim cannot read it.
   - The flow gate is not in gates-b9b. ba80cfee's message reports "flow --off60 EQUAL, gates GREEN 06:12:42" at a HEAD that includes the B9 commits. I did not open that file.
   PASS.
(4) The evidence is real and the comparison fair.
   - Right build: all ten Player logs load Builds/n60-b8 (the Mono path, line 1).
   - The build carries the B9 code: SKYGRIND.exe is from 05:43:08 and Skygrind.Game.dll from 05:43:11. That is after 224d8338 (05:41:49) and before 228e860b (06:02:42, a probe-script change only).
   - The row is on only in the build's copy. Builds/n60-b8/SKYGRIND_Data/StreamingAssets/Design/unity-config.json differs from the repo's copy at 10adcfc8 (HEAD before B7) in exactly one value: design.n60.focus60.enabled, true vs false. That is my JSON walk, with 1 equal to 1.0.
   - Two notes on that copy:
     - The file's mtime is 06:03:26, after the probes (06:00-06:01) and before verbs37 and the smoke. It writes 93 numbers as 1 where the repo writes 1.0 (the values are the same).
     - rim56 is OFF in it, while the repo has rim56 ON since ba80cfee. So rim56 ON and focus60 ON have not yet run together in one build.
   - The input fired. The start lines read "focus60 row on" (on, on8s, planted) and "focus60 row off" (off, with -skygrindNoFocus60). In the smoke, the pause at the focus loss with the cursor still Locked can only be focus60's: the lock rule needs the lock to drop.
   - The diff matches the brief:
     - PausesOnFocusLoss = Playing && focused last frame && unfocused now && !attract && !reel.
     - It is called after the lock line and only while FocusPause60On is set. That flag is read once and is off under -skygrindOff60 or older, -skygrindNoFocus60 and -skygrindBench.
     - FocusFrame60 runs at the start of Update, beside wasLocked.
   - N60FocusPauseTests + N60FocusRowTests RED 05:33 -> GREEN 9 / 9 05:37 is the lead's record (NIGHT60-LOG.md lines 173-176). I did not see it run.
   CONFIRMED.

Trap answer (read SurvivalGame.Flow.cs Pause / Resume, SliceSmoke.N55.cs Smoke55, the Resume callers):
- No pause outside play. Pause() returns unless flow == Playing, so a focus loss never pauses DOWN, RESULTS, the title, or a MODS draft or reward reveal (both already run under Paused: PickMod and UiReveal20 call Resume). The pause menu cannot re-pause either.
- The player can always leave it. Pause() frees the cursor. Then:
  - Esc again resumes, though not on the frame that paused (SurvivalGame.Debug.cs:43; a draft waits for its pick).
  - RESUME on the pause menu (UiMenus.cs:266).
  - The pad's START or B resumes after a fresh press (UiNav.cs:229 and 372).
  I found no pause the player cannot leave.
- The Editor cannot pause twice. The lock line and the focus line both call Pause(). Whichever fires second, in that frame or a later one, sees flow Paused and returns.
- No pause / resume loop in the smoke. The rule fires on the edge only (focused last frame, unfocused now). After SmokeGuard55 resumes a still-unfocused window, the next frame reads unfocused / unfocused and does not pause again (52.9 s in the smoke: one pause, resolved once).
- The edge rule leaves one gap (it does not trap the player). If the window loses the focus during DOWN, a draft or a reveal, and the player is still away when the run goes back to Playing, the run plays on unfocused. That is today's behaviour.
- Possible annoyance (taste, GG's call):
  - Any window that takes the foreground mid-fight pauses the run. That includes a notification or launcher pop-up, a chat app, or a click on a second monitor.
  - A flicker of a few frames also pauses, and the player must press Esc, START or RESUME.
  - Not checked: whether the click that brings the window back can land on a pause-menu button; the Steam overlay; how this behaves in the Editor when evals drive an unfocused Editor (the lock rule already pauses there).

For GG (play notes; focus60 is a view row, the sim never reads it):
- Alt-tab, or any other window taking the focus during a fight, now opens the pause screen with the mouse freed. Before, the run kept going behind your back (proved in a Player: n60-b7 kept Playing after the focus loss).
- To continue: Esc, the pad's START or B, or RESUME.
- Nothing pauses during DOWN, results, the title, a MODS draft or a reward reveal. Leaving during those and staying away keeps today's behaviour: the run goes on once it is back to Playing.
- A pop-up that steals the focus for a moment will also pause you mid-fight. Judge whether that annoys you in play.
- Off switch: design.n60.focus60.enabled. It is false in the repo's two config copies and true only in Builds/n60-b8's copy. Or launch with -skygrindNoFocus60 (this rule only) or -skygrindOff60.
