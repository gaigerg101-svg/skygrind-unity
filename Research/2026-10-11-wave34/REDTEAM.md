# Wave 34 forge: the fresh reviewer's findings and what was done (2026-10-11, about 6 minutes, file paths only)

| # | finding | done |
|---|---|---|
| 1 | Instant swap is a Core change in `Loadout.StepSwap` (Core/Combat.cs:1105-1137); `swapSteps 0` would never pass the lower bound; GoldenCombat.SwapTimeline covers it; Viewmodel.Draw's HandsSwapTimeline unowned | fixed: B owns Combat.cs, sets the slot in the press step and calls SwapIn / SwapIn19 / weaponRaise; check (d) rewritten; H owns HandsSwapTimeline |
| 2 | The split slice is oversized; the hook table cannot live in Core; moving `brain.Step` breaks rule 4 | fixed: nothing reordered (world calls skipped in place on non-world steps); the table in Game; a 75-minute cap, global stays if SHORT |
| 3 | The reel cap of 20 is under-counted (RED runs, level boots) | fixed: a reel budget in lean_rules (one combined RED per round) |
| 4 | A1 oversized and blocked on M's distances | fixed: A measures its own; calm on all five, lines on foundry and kiln first, the other three leftovers |
| 5 | reach34's code sits in lead and B files (Boss30.cs:282-288 with `headReachMargin`, Combat.cs PunchTargets) | fixed: M writes the patches, owns Pulse25.cs |
| 6 | M1 oversized; the golden command unnamed | fixed: three techniques at most; `dotnet run --project Tools/CoreHarness -c Release` named for builders |
| 7 | Bend clocks and meter numbers ambiguous | fixed: real seconds, converted per mode; refill 0.25 a kill, x2 at S+, the meter starts full |
| 8 | Rule 5: render / UI numbers belong in UnityTuning | fixed: UnityTuning.Bend34.cs / Hud34.cs |
| 9 | Rule 8: "violet" bend reads as the boundary colour; one global ColorAdjustments cannot keep two hues | fixed: neutral cool grey, ColorCurves hueVsSat, the sky dimmed |
| 10 | The shared commit is too large for 0:15 | fixed: reels script and ID map moved to round 1; start box 0:20 |
| 11 | Player vs world block unclassified items (devices, whip arrive, lava, buyables) | fixed: the lists are in the card |
| 12 | The NIGHT31 grapple reel reads `MarkShown` | fixed: MarkShown stays the "would show" bit under nomark34 |
| 13 | Weakest card: a meter-gated bend leaves most grapples plain; the global default slows the player | fixed: a free 1 s short bend on every latch, the meter pays for the full bend; the default leans to split |

Rejected: none. Not re-reviewed after the fixes (the forge's 30-minute box).
