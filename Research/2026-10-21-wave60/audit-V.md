# NIGHT60 audit pass, builder V: (h) UI text and the dusk palette, (i) pad-first

Read-only. No Player, no Editor.

**Sources:**
- FeelProbe's `ui[]` in 165 `telemetry.jsonl` files under `Logs/feel/n60-b1..n60-b7` (MAIN repo), every 10th line: 6555 frames sampled.
  Each entry has rect, alpha, colour, role, px (font size) and text.
- `docs/VERBS.md`, `Assets/Scripts/Core/Gamepad.cs` (the pad layouts), `Assets/Scripts/Game/Input/PadReader.cs`, and the UI files'
  keyboard reads.
- The scan scripts were scratch and are not committed. Every line below names its file and number.

**Top 3 (S):** A-V1, A-V3, A-V4.

- [ ] **A-V1 (S) The PULSE gauntlet's HUD pips are pink, next to RIVAL's magenta**: `UI/HUD 33/Weapon 52/Rail 34/PULSE pip 0-4` draw
  `#FF6FB0` (HSV h 333, S 0.56, V 1.0; `UnityTuning.F47.cs` `Hud47GauntletPinkHex`) in 4041 sampled frames (e.g.
  n60-b1/l0spawn/a157). Rule 8 has no pink family: magenta is RIVAL only and player-usable is cyan. F-cards 1c's OKLab argument (h 355
  vs RIVAL's 338) is not the HSV arc the meters use: n56-colour.py's magenta is 285-330, 3 deg away. Verify: the pips' rendered hue on
  one a157 frame with n56-colour.py's arcs, then GG's call (keep the pink or move the pips to cyan).
- [ ] **A-V2 (M) docs/VERBS.md's pad column disagrees with the code's pad layouts**: VERBS #5 dash "B", #17 grapple "LB tap", #34
  weapon swap "RB". `Core/Gamepad.cs` DefaultLayout has LB Dash, LS Grapple, Y NextWeapon; DefaultLayoutV2 has LB Dash, RB
  Special + Grapple, LS Sprint, RS Punch, Y NextWeapon. The design picks layout "v3" (`Design/unity-config.json:325`; its buttons not
  read here). Verify: read the v3 layout, rewrite the pad column from it, and check each verb with the pad reels (FeelReel.N27.Pad /
  N41.Pad). A verb with no button in v3 is keyboard-only: list it.
- [ ] **A-V3 (S) The round-end reveal pick (UiReveal20) reads only keys and clicks in its own code**: `UiReveal20.cs:155-159` takes a
  card on digit 1 / 2 / 3, and `:91` on `Button.onClick`. There is no pad read there. Whether UiNav moves pad focus onto the three cards
  was not checked. Verify: one Player run with the pad, or a PadY-style reel step that picks a card with A. Add a d-pad + A path if
  none exists.
- [ ] **A-V4 (S) The 16 px rule passes as font size; whether it means glyph height is undecided**: every on-screen text in the sample
  has a font px of at least 20 (20: 236 samples, 22: 15554, 24: 1201, 28: 13470, 32: 13570, 34: 2868, 36: 17788, 40: 1531, 56: 13474,
  60: 12310). None is under 16 except the teach card's pop-in (34 px x scale 0.05, `UI/HUD 33/Teach card 43/Verb/Text`,
  n60-b2/vp/verbs37), which is an animation, not a size. A 20 px font's capitals are about 14 px. Verify: the lead or GG names the
  measure. If it is glyph height, list the 20-22 px elements by path (not listed here) and measure their capitals on a frame.
- [ ] **A-V5 (S) The x-ray toggle has a keyboard read in EnemyViews; the pad's toggle is not traced**: `EnemyViews.cs:318-321` toggles
  `settings.xray` on `kb[xrayKey]`. `PadAction.Xray` is d-pad down in both layouts (`Core/Gamepad.cs`), but where the pad action
  toggles the same setting was not checked. Verify: grep `PadAction.Xray`'s consumer, or one pad press in a reel with the setting's
  value printed.
- [ ] **A-V6 (M) Two level accents sit near the role families on the frames**: n56-colour.py's palette lines (`Logs/n58/r1/colour-v1*`
  and NIGHT60's a357 runs) give:
  - spire's accent `#e67793` at h 345: 15 deg from the danger band's edge, 15 from magenta;
  - foundry's accent `#a267e6` at h 268: 17 deg from magenta.

  Rule 8 keeps magenta for RIVAL and red-orange for danger. Verify: per a357 frame (n60-b5), the share of world pixels inside the
  danger and magenta arcs on vs `-skygrindOff56`, with n56-colour.py's role masks; a move of more than 0.5 % of the frame is a finding.

## Settled from the code (05:20-05:40, read-only; port/stage3 3755f51d's tree)

### A-V3: can a pad player take the round-end card? YES in code, but no automated run has ever done it

The chain, link by link:
1. **The reveal is a menus-layer screen built last.** `UiHud20.cs:22` calls `BuildReveal20()` ("built last: the top one").
   `UiReveal20.cs:72` makes it with `Screen("Reward reveal", ...)`, which parents it under `menus`, inactive (`UiKit.cs:49-59`).
2. **Each card is a uGUI Button whose click takes it.** `UiReveal20.cs:88-91`: `AddComponent<UI.Button>()`, `transition = None`
   ("UiNav's focus frame shows it"), `onClick.AddListener(() => Reveal20Take(idx))`. On a pad the digit chips are hidden: "the pad
   picks with its focus" (`UiReveal20.cs:200`).
3. **Opening it pauses the game and shows the screen.** `UiReveal20.cs:206` is `reveal20.SetActive(true)` and `:214` is
   `game.Pause()`. `Ui.cs:849` keeps PAUSED hidden while the reveal is open.
4. **UiNav focuses the first card.**
   - `UiNav.cs:376-384`: `TopScreen()` is the last active child of `menus`.
   - `UiNav.cs:233-249`: on a new top screen it relinks and calls `es.SetSelectedGameObject(PrimaryOf(screen))`.
   - `UiNav.cs:386-397`: for a screen that is not TITLE / PAUSED / RESULTS / SETTINGS, `PrimaryOf` is `navList[0]`, the first card.
   - Only the MODS draft waits for a d-pad move first (`navDraftWaits`, `:245-247`).
   - `UiNav.cs:256-264`: a lost selection returns to it.
5. **The pad's buttons drive the UI module** (`UiNav.cs:144`, the scene's `InputSystemUIInputModule`):
   - Navigate: `<Gamepad>/dpad` and `<Gamepad>/leftStick` (`:168`, `:171`);
   - Submit: `<Gamepad>/buttonSouth` (`:176`), wired as `navModule.submit` (`:187`);
   - Cancel: `<Gamepad>/buttonEast` (`:179`).
6. **A on the focused card takes it.** A fires `Button.onClick` → `Reveal20Take(idx)`, accepted once `Reveal20.Ready` (the 0.8 s
   turn-over, `UiReveal20.cs:220`). An A pressed mid-reveal does nothing, so a jump's A never takes one. D-pad left / right moves
   between the three cards by UiNav's row links.

**Never exercised.**
- No reel or smoke step calls `Reveal20Take` (a grep finds no caller outside `UiReveal20.cs`).
- The slice smoke's guard resolves a reveal with `game.Resume()`, which makes the reveal skip itself (`SmokeGuard55.cs:168`).
- It resolves a MODS draft with `game.PickMod(0)` directly (`SmokeGuard55.cs:167`), not through any device.
- So neither the reveal's pad pick nor the draft's has ever run.

**No fix needed (step 2 does not apply).**

**The Player check that would prove it** (L-089, not built):
- a reel step opens a reveal (a pending special-round end, then a break), waits past the 0.8 s turn-over, and presses the smoke's virtual
  pad A (`buttonSouth`);
- it must print the press firing (the pad's submit on the frame), the `rewardPick` event with slot >= 0, the slot taken (= the UiNav
  focus, card 0 unless the d-pad moved), and the game resumed.
- A twin step for the MODS draft: d-pad right, then A, `PickMod` with that index.

### A-V5: can the pad toggle the x-ray? YES

- **The button.** D-pad down is `PadAction.Xray` in the default layout (`Core/Gamepad.cs:261`) and in DefaultLayoutV2 (`:290`). V3, V4
  and V5 are built only by swapping RB / Y and RB / RS (`:389`, `:438`, `:494`), so d-pad down stays the x-ray in every version.
- **The action.** `PadReader.cs:397` sets `commands.xray = true`.
- **The toggle.** `SurvivalGame.cs:236-241` (`RoutePad`, while Playing and not the attract run) calls
  `settings.Set("xray", !settings.Bool("xray"))`.
- **The draw.** `EnemyViews.cs:322-331` sees the settings version change and turns the x-ray pass on or off.
- **The keyboard read is its own path.** `EnemyViews.cs:317-321` is the keyboard's path; `SurvivalGame.cs:231-233` says each device
  keeps its own read, so nothing toggles twice.
- **Dead flag, harmless.** `CombatReader.cs:143`'s `Commands.xray` has no reader.

**A-V5 is closed.**
