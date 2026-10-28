# PULSE diagnosis (NIGHT27, builder W, 2026-10-09)

GG: the PULSE "isn't working" on his real gamepad; NIGHT26's reel (virtual pad) passed. Method: a static read of the code path from the device to the sim,
no Editor, no real pad. Line numbers are as of builder W's worktree after the hysteresis edit.

## The path, and what was checked

| Step | Where | Finding |
|---|---|---|
| 1. device read | `Game/Input/PadReader.cs:439-463` `Fill`, `ReadButton` (502) | `Gamepad.current` (else the first pad) -> `leftTrigger.ReadValue()`. LT is a standard `ButtonControl`; no inversion or remap. OK. |
| 2. threshold | `Core/Gamepad.cs:593` `TriggerDown = 0.35`; `ButtonDown` (668) | Pressed from 0.35. **No release threshold**: the value must stay above 0.35 every frame. See cause 4. |
| 3. layout | `Core/Gamepad.cs:248-266` and every default (v1-v5) | LT -> `AltFire` in all five default layouts. A stored custom layout could differ: **not checked** (needs GG's PlayerPrefs). |
| 4. merge | `Game/Input/ReaderInput.cs:239-250` `SampleCombat`, `Core/Gamepad.cs:850-865` | `pad.held` ORs into `combat.altFire.held`. The pad merges whether or not the cursor is locked. OK. |
| 5. loadout routing | `Core/Combat.cs:683` | With the PULSE in hand `LtZooms("gauntlet")` is false, so LT is copied to `special` (a no-op for the gauntlet). `ZoomGun("gauntlet")` is 0 (`ads.weapons` has no gauntlet; `Zoom20.cs:73-83`). **No zoom conflict on the PULSE**; the zoom is a different gun's LT. |
| 6. swing | `Core/Guns/Weapon26.cs:143` `PulseLtSwing26`, `Combat.cs:838` | LT held swings the gauntlet **only while `Weapon == "gauntlet"`**. With any other gun in hand LT is that gun's zoom/alt and the PULSE does nothing, silently. |
| 7. launch | `Core/Guns/Pulse25.cs:55-62` | Fires on the **release** of LT, **and** only if the gauntlet is in hand (58), the fist is active (58), a notch is full (`CanFlick`, 59: charge >= 0.33 = 7 landed hits, `Fist24.cs:166`) and the 3 s lockout is over. Every failed condition is a silent `return`: no sound, no number, no glow. |
| 8. charge | `Core/Fist24.cs:202-215` `OnHit`, `232-238` `OnHurt` | Only LANDED gauntlet hits feed it (+0.048 each: `Design/unity-config.json` `pulseCharge.gain.hit`, 7 per notch, about 3 s of unbroken hitting). **Every hit you take removes a third of it** (`hurtLoss` 0.3333, config line 1053; `OnHurt` multiplies the charge by 2/3). |
| 9. feedback | `Game/UiReticleArc24.cs:138,161-171`, `UnityTuning.N24F.cs:18` | The charge is drawn as three 5 px cells in a 140 px arc under the crosshair at **alpha 0.2 at rest, 0.4 while holding a charge**, and only with `design.n24.reticleArc` on. Nothing says "hold LT", "release", "need 7 hits" or "you took a hit and lost a third". |
| 10. the reel | `Game/FeelReel.N26.Pulse.cs:65,92,118` | The reel drives `g.forcedCombat`, which **replaces** the whole input (`ReaderInput.cs:248`). It never touched steps 1-4. Its target was a husk pinned 1.6 m ahead, with the player not hurt (no `hurtLoss`), held 12 s. It proved the sim rule, not the pad and not play. |

## Likely causes (ranked by what the code supports)

1. **Silent gates, no feedback (code-certain).** Steps 6-9: the blast needs the gauntlet in hand, 7 landed hits, no hit taken in between, a release, and no lockout,
   and when any one is missing nothing happens and nothing is shown. A player holding LT with 3 hits banked, or with the KESTREL still in hand, sees "nothing".
   This is the strongest candidate: it is what "isn't working" looks like when the rule is invisible. Fixed in the new row: the glove's five pips, a lit glove,
   "PULSE READY" (24 px), and a refusal's reason ("NEED n KILLS", "COOLDOWN n S", 18 px, never red) on LT press (`UiWeapons.N27.cs`, `PulseCharge27.Deny`).
2. **The earning rule is hostile to play (code-certain).** Hits (not kills) are needed, in a row, against a body in a 2.4 m / 35 degree cone, and `OnHurt` takes a third
   of the charge for every hit received. In a fight against several enemies the charge rarely reaches the first notch. The reel never saw this (no hurt, a pinned bag).
   Fixed in the new row: KILLS fill five pips, a hit taken costs nothing, the pips are only spent by the blast (`PulseCharge27.cs`).
3. **The reel did not test the real input path (code-certain).** See step 10. NIGHT26's PASS cannot clear steps 1-4. The harness `weapons --pulse27` also drives the sim, not the pad.
4. **Trigger chatter at the 0.35 threshold (plausible, NOT verified).** `TriggerDown` has no release threshold, so an analog trigger resting near 0.35 (a worn Xbox or
   DualSense trigger, a half-held pull, Steam Input's trigger curve) reads press-release-press. For the old LT-held rule that makes the "release" fire early (with a notch it
   launches at a random moment and spends the charge; without one nothing happens), and for LT-as-zoom it makes the zoom flicker. **Fixed defensively** (a code change I
   could not test on a pad): `GamepadMath.TriggerUp = 0.25` and `PadReader.ReadButton` holds a pressed trigger until the value falls below it. Pure `ButtonDown` is unchanged.
5. **Wrong device or stored layout (possible, NOT verified).** Steam Input remapping, a second pad, or a stored `skygrind.padLayout.v*` with LT moved. Not checkable from code.
6. **The 3 s lockout eating a release (ruled out as the first-use cause).** The lockout (`Fist24.cs:282`) starts only after a launch; it cannot be the reason the first
   release does nothing. After a launch it does eat the next one inside 3 s; that is by design, and the new row replaces it with a 15 s cooldown with a visible count.
7. **Zoom conflict (ruled out for the PULSE).** Step 5. The PULSE has no zoom. The conflict GG may feel is that LT does a different thing on every gun (zoom on the KESTREL and
   THUNDERCLAP, detonate on the STITCHER, steer on the COMET); the start loadout is KESTREL then PULSE (`weapons.survivalStart`), and with the KESTREL in hand LT zooms and the
   PULSE blast does nothing. That is working as written, and invisible (cause 1).

## What I could not verify (needs the real pad, or GG)

- Whether GG's trigger actually chatters around 0.35 (cause 4), and whether the hysteresis fixes his pad.
- The layout stored on his machine (cause 5) and whether Steam Input sits between the pad and Unity.
- That the glove widget draws where intended (`UiWeapons.N27.cs` is dn-check compiled, not run; Ui.cs needs the two wiring lines named in the report).
- The felt result of the 2.5 s swing and the five-kill gate: a harness proves the rule, not the feel (L-001).

## Changes made for the diagnosis

- `Core/Gamepad.cs`: `TriggerUp` 0.25 (Unity-only; the web had one threshold). `Game/Input/PadReader.cs` `ReadButton`: hysteresis (defensive, untested on a pad).
- pulse27 row (default off until the lead turns it on): replaces causes 1 and 2 (a visible gate, kills not hits, no loss on being hit, a 15 s cooldown with a countdown).
- With `design.n27.pulse27` off the NIGHT26 rule is unchanged, bit for bit.
