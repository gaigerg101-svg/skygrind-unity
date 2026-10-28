# NIGHT60 proposals (L-size items: specs for GG, not built; NIGHT60.md section 2 step 5)

## P1 yellow56: keep the parry tells out of the cap with a mask, not a colour bound (from B3, 2026-10-22 01:36)
**Why:** yellow56 (OFF) dims non-parry yellow on screen so yellow reads as "parryable" only. Its cap is a full-screen pass that can only tell
pixels apart by colour. The Player scan tonight (Builds/n60-b3, a358 foundry + spire, Logs/n60/b3-*): with the keep bound sKeep at 0.85 /
0.75 the MAIN passes (0 / 12 frames over 0.5 % non-parry yellow) but a spitter's parry tell keeps only 59.6 / 64.7 % of its yellow (guard
>= 80 %); at 0.65 the tell keeps 82.2 % but spire's own world yellow passes too (7 / 12 frames over, worst 3.55 %). Off: 11 / 12 and 90.5 %.
The tell's pixels and spire's world yellow overlap in hue, saturation and value, so no colour rule separates them.
**The idea:** the materials that draw a parry tell (the husk / spitter / swarmer models40 parts in parry40's colour, the orbs) write a stencil
bit; Cap57Pass leaves stencilled pixels alone. Then the cap can be strict on the world (sKeep high) and the tells keep 100 % by construction.
**Size and risk:** L. URP: the cap runs after post-processing as a full-screen pass; it needs the camera's depth-stencil target bound at that
event (check the URP 17 render graph docs first) or an extra low-res "tell mask" texture drawn by the tells. Every tell material changes
(GG owns enemy looks: only their stencil state, not their colours). The meters already exist (n60-v-yellow.py, n59-v-halo.py, both planted).
**GG decides:** whether the "yellow = parryable" rule is worth a render-pipeline change; if not, yellow56 stays OFF (it is OFF now).

## P2 the pilots that climb THE WICK's stack and ride a wall in a Player (from audit-L.md A-L13; THREADS T-59 / T-66, 2026-10-22 06:1x)

**Why:** two Player proofs have never gone green, so two features have never been seen working in a Player. The headless sim says
they work.
- **T-59 (w37): THE WICK killed on a Player frame.** At w56 a155's FIGHT MODE pilot fired (shots 27) but never climbed n52-foundry's
  stack: 0 core punches, wick55 on = off (n56-r4 / r5).
- **T-66 (w43): move43's pilot never rides a wall in a Player.** w44 RED: the chimney took one press, then the pilot fell to the floor.
- **The common cause is L-071.** A reel pilot must step the sim once per sim step (`sim.UseInput`), never once per rendered frame. w44
  measured it: wick37's headless proof read 0 / 3 per frame and 3 / 3 per step (49.8-51.1 s), and the reel's panels ridden went
  [] → 15 / 15.

**What exists:**
- **`Wick37Pilot`** (`Assets/Scripts/Game/SurvivalSim.Wick37Pilot.cs:17`, `Step(sim, yawNow, tick)` at `:221`): the GROUND / RIDE /
  AIR / FIGHT climber. It reads `Wick37Stack`'s panel list, which is the arena's (arena-local).
- **The n52-foundry reels already step it per sim step.** `FeelReel.N55.Boss.cs:96-102` makes it the sim's input through
  `Wick56FightInput`. So T-59's remaining gap is the stack: the pilot plays the arena's panels while n52-foundry's WICK room has its own
  geometry.
- **move43's reel drives its pilot per rendered frame.** `FeelReel.N43.Move.cs:160-162` sets `g.forcedMove`, then
  `yield return null`, which is exactly L-071's mistake.

**The idea:**
- **T-59.** `Wick37Pilot` takes its panel list from the live level's WICK room (the boss room's stack in n52-foundry's data) instead of
  the arena's. The Player reel runs a454 / a155's FIGHT setup with that pilot as the sim's input.
- **T-66.** move43's chimney / wall steps become an `IPlayerInput` pilot given to `sim.UseInput` (Edge35Input already wraps one,
  `FeelReel.N43.Move.cs:100`). Its press / hold timing is counted in sim steps, not frames.

**The Player proof** (L-089, it prints that its input fired):
- **T-59.** One a155 line per run: `pilot per step: steps N, presses P | panels ridden [ids] of K | core punches C | THE WICK killed
  yes/no at t s`. GREEN needs panels ridden >= 1, core punches >= 1, and killed yes.
- **T-66.** One move43 line: `pilot per step | chimney: ride steps R (>= the harness's band), wall jumps J, apex y | floor touches F
  during the climb`. GREEN needs R > 0 and no floor touch mid-climb.

**Size and risk:** L.
- Two reel pilots, plus one data hook for the live stack.
- No game rule changes, and the fingerprints stay EQUAL: reel-only.
- **Risk.** n52-foundry's WICK room geometry may not fit `Wick37Pilot`'s panel model (it assumes the arena's stacked panels). Read its
  `RiddenPanel` (`:45`) against the room's boxes first.

**GG decides:** whether these two proofs are worth a builder night. THE WICK and move43 stay as they are either way; this only measures
them in a Player.

## P3 the reel step that takes a round-end card and a MODS draft card on the virtual pad (from B19 / audit A-V3, 2026-10-22 06:1x)

**Why:** pad-first is GG's stated target (Steam, and hopefully Switch). The code path exists (audit-V.md A-V3, 6b8b5834):
- UiNav selects the reveal's first card (`UiNav.cs:233-249`, `:386-397`);
- the pad's A is the UI module's Submit (`UiNav.cs:176`, `:187`);
- the cards are Buttons whose `onClick` calls `Reveal20Take` (`UiReveal20.cs:88-91`), accepted after the 0.8 s turn-over (`:220`).

But no run has ever taken a card on a pad. No reel calls `Reveal20Take`. The slice smoke's guard skips a reveal with `game.Resume()`
(`SmokeGuard55.cs:168`) and takes a MODS draft card with `game.PickMod(0)` directly (`:167`), without any device.

**The idea:** a new reel scenario in a new partial `Assets/Scripts/Game/FeelReel.N61.PadPick.cs`, with its scenario id routed from the
next night's FeelReel router (the id is the lead's to assign). Its steps:
1. **IgnoreFocus first** (T-100). `InputSystem.settings.backgroundBehavior = IgnoreFocus` for the reel's length, restored at the end, as
   `FeelReel.N33.Shop.cs:76-102`. A virtual pad, `InputSystem.AddDevice<Gamepad>("Smoke pick pad")`, gets state events as in
   `SliceSmoke.Night13.cs:140`, and is enabled before every press.
2. **A reveal.** Set a pending reveal (the special-round end that `Reveal20.Triggers` accepts), finish the round to its break, and wait
   for `rewardReveal` (the reveal opens and pauses).
3. **The pick.** Wait past the turn-over (`Reveal20.Ready`, 0.8 s) and press the pad's A for one frame.
4. **The line.** It prints:
   - the press: the pad device, A down on frame F;
   - UiNav's selected card before the press;
   - the `rewardPick` event, with slot >= 0 equal to that card, its kind and points;
   - the flow back to Playing.

   GREEN needs all four. A RED control press A at 0.4 s, inside the turn-over: no pick, the reveal still open.
5. **The twin for the MODS draft.** Let a draft open (`game.DraftOpen`). UiNav selects nothing until a move (`navDraftWaits`,
   `UiNav.cs:245-247`), so press d-pad right, then A. It prints the selected card, `PickMod`'s index (= that card) and the draft closing.

**What it proves:** a pad player can take both kinds of card. It is the first run of that path on any device. Its RED / GREEN pair
follows L-014 / L-089.

**Size and risk:** M.
- One reel partial and one router line. No game change.
- **Risk.** Opening a reveal on demand needs a special round's end. Use the reel's existing round control, or the reveal's pending-round
  setter if one is exposed; adding one is reel-only.

**GG decides:** whether pad-first proof of the round-end choices runs before the Switch work. Nothing in the game changes.
