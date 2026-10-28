# Steam Deck row 1: RESULTS, SETTINGS and QUIT with the pad only (NIGHT20, builder P2)

**What this is.** A note for a later night. It says what the Player's pad-only smoke would need to do to cover the
controller checklist's row 1. No smoke code changed tonight (the lead's call: build 5 passed its smoke).

The row comes from `Research/2026-10-04-controller.md`, section 8, row 1: *"All content reachable on a pad from boot
(settings, rebinding, profile, RESULTS). Unplug mouse and keyboard; title -> run -> RESULTS -> settings -> quit with the
pad only."*

Everything below was read from the code at port/stage3 `b085161`. None of it has been run in a Player.

## 1. What the pad-only step covers today

`Game/SliceSmoke.Night15Pad.cs` (`PadOnly`, NIGHT15, builder K) drives a virtual pad. It adds a `Gamepad` and sends its
state events. It never uses the mouse, the keyboard or a forced input.

1. **TITLE.** A on PLAY starts a run.
2. **The weapon wheel.** NEXT WEAPON held opens it; the right stick points at the other gun; the release takes it.
3. **NEXT WEAPON.** A tap switches gun.
4. **PAUSED.** START pauses. The d-pad reaches SETTINGS and A opens it. B closes it back to PAUSED, and START resumes.
5. **Clean-up.** The pad is removed and the game goes back to TITLE.

It never visits:

- RESULTS
- TITLE's own SETTINGS (only PAUSED's is tested)
- the SETTINGS tabs past the first
- CONTROLS (rebinding)
- QUIT

## 2. The path the row asks for, as the screens are built

- **RESULTS** (`UiMenus.cs`, the RESULTS section). Its column is RETRY, TITLE, DETAILS, RETRY SAME SEED and
  COPY SCORE LINE (a developer's).
  - UiNav selects RESTART when the screen opens (`primaryIndex`; `restartButton` is RETRY).
  - LB / RB switch SUMMARY and DETAILS (`UiNav.PadTab19` -> `UiResults.ResultsTab19`, once the tally is done).
  - RETRY and RETRY SAME SEED start runs, so the step must never press A on them.
- **TITLE.** The screen has PLAY, then RESUME beside it (shown only with a save), then the run setup card under PLAY.
  The column then goes PROGRESS, SETTINGS, QUIT.
  - The setup card's MODE and HEAT change on left / right. LB / RB on TITLE cycle MODE (`SetupTab19`). So the step
    must move by up / down only.
- **SETTINGS.** Left / right change a row or switch the tab (`NavRow`, `NavSide`). B closes it.
- **QUIT.** It calls `UiMenus.Quit` -> `Application.Quit()`, so the Player exits.

## 3. What a pad-only step would need: `PadOnlyResults`, after `PadOnly`

Every step uses the existing helpers (`Press`, `PadOnlyWait` in real time, `PadOnlySelected`), and every line starts
"pad only:". The device used last (`PlayerDevices.Last`) must stay the pad throughout.

1. **Reach RESULTS without an input.** Drop the smoke's god mode and deal the death the main smoke deals in its step 4
   (`g.health.Damage(g.health.max * 2, "husk", ...)`). Then wait in real time for `Flow.Down`, then `Flow.Results`
   (`health.downTime`, 1.5 s, then the tally).
   - **Check:** the selection is RETRY.
   - **Check:** RETRY's key chip is hidden on the pad (`RetryLabel.For(pad: true)` gives "RETRY"). This is also
     row 2: no key glyph while the pad is in use.
2. **RESULTS tabs.** RB shows DETAILS and LB brings SUMMARY back.
   - **Check:** the details panel toggles both ways and the selection stays on the column. Wait for the tally to finish
     first, or the tab is ignored.
3. **RESULTS -> TITLE.** Press d-pad down until "TITLE" is selected (at most 6 presses), then A.
   - **Check:** `Flow.Title`, with PLAY selected.
   - Guard: press A only when the selection's name is TITLE. Never press A on RETRY or RETRY SAME SEED.
4. **TITLE -> SETTINGS.** Press d-pad down from PLAY until "SETTINGS" is selected (at most 8 presses), then A.
   - **Check:** `UiView.SettingsOpen`.
   - Guard: no left / right on TITLE (they would change MODE or HEAT).
5. **Inside SETTINGS** (rows 8-12 and 31).
   - **Every tab:** visit each in turn (RB, or right on the tab strip) and come back.
     **Check:** each tab opened, and the selection was never "none".
   - **One row:** d-pad right once on one row, then left once.
     **Check:** the value moved one step and back (single-press sliders, row 31).
   - **Restore** the settings snapshot afterwards, as `PadOnly` does with `store.Save()` / `Load()`.
6. **CONTROLS** (row 11, rebinding). A on the SETTINGS row labelled KEYS (`UnityTuning.KeysRowLabel`) opens CONTROLS
   (`UiSettings.SettingsOpenKeys`). Which tab holds that row: not checked.
   - **Check:** the page's hint draws glyphs (`ControlsPad20`: "<A> SELECT, THEN PRESS A PAD BUTTON · <B> BACK").
   - B closes it. **Check:** back on SETTINGS (`settingsControlsReturn`), focus on the KEYS row.
   - No rebind is made: that is the CONTROLS tests' job, and the smoke must leave the bindings as found.
7. **SETTINGS -> TITLE.** B closes SETTINGS.
   - **Check:** `Flow.Title`, with SETTINGS selected (focus kept, row 31).
8. **QUIT.** Press d-pad down until "QUIT" is selected.
   - **Check:** it is selected and interactable.
   - Pressing A ends the Player, so pick one of two options (a decision for the lead or GG):
     - **(a) Press A as the smoke's very last act.** The caller (`-skygrindSmoke`, whose log the lead reads) then
       checks that the process exited with code 0 and that the last log line is the smoke's "pad only: QUIT pressed".
       No game code changes.
     - **(b) A smoke hook.** `UiMenus.Quit` calls a static `Action QuitOverride` when the smoke sets it, and the smoke
       counts one call.
   - I recommend (a): it tests the real quit, Deck's own path (row 22: no dialog on the way out).

## 4. Still outside the step (not checked)

- **DOWN.** Needs no input; it is on the path above.
- **The MODS draft.** A pad pick (the bot's farm uses `Mods.Pick`, not the pad).
- **The reward reveal** (`UiReveal20`). A slot can't be taken mid-reveal (`Reveal20.Ready`).
- **TITLE's run setup.** The DAILY and WEEKLY picks.
- **The CODEX tab.**
- **PROGRESS.** CONTRACTS and WEAPONS.
- **SAVE & QUIT at a break, then RESUME on TITLE.**
- **ASSIST.** The SETTINGS > ACCESS row.
- **Rows 17-18.** Text size and layout at 1280x800 (a capture, not the smoke).

Each is a pad path that exists in the code. None of them is run by a smoke.

## 5. Cost (my estimate; nothing here was run)

- **Size:** about 120 lines in a new `SliceSmoke.Night20Pad.cs` partial, one call in the step list, no game code with
  option (a).
- **Run time:** about 10-15 s of real time. Most of it is RESULTS' tally and the waits; every press holds 0.06 s, then
  waits 0.06 s.
- **Risk:** the wait for RESULTS. It must allow the down time plus the tally (the main smoke allows 10 s).
