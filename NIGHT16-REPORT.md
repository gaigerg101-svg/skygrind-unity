# NIGHT16-REPORT: the feel night, for GG (2026-10-05, from 2:18 PM)

Your asks: movement balanced so you can't spam slide, ULTRAKILL-style pauses and slow-mo on punches, headshots and big
hits, animations when you pull out the fist and the guns, and full polish on the UI. To feel it, play
`Builds/n15-before/SKYGRIND.exe` (last night's build) and then `Builds/Stage1/SKYGRIND.exe` (tonight's), following
`FEEL-PLAYTEST.md`. That's ten minutes, ending with the questions only you can answer. Everything is behind settings
you can switch off. The game's determinism still holds: golden vectors 0 mismatches, every trajectory replay exact, the
fingerprint EQUAL to tonight's baseline.

## 1. The slide is a skill now, not a stance (`design.move`; LAB tab rows, on by default)

Three rules:
- **The charge.** The burst to 11.6 m/s needs a charge, which comes back 2 s after a slide ends; a cyan tick under the
  crosshair shows it.
- **The decay.** On flat ground a slide eases down to a run after 0.6 s; downhill it still pays.
- **The falloff.** Chained slide-jumps pay x1.2, then x1.05, then x1.0.

Measured with `SimHarness spam`. "x walk" is how far you get against just walking:

| What you do | Before | Now |
| --- | --- | --- |
| Mash slide (any rate) on flat ground | 1.23-1.32 x walk | 1.00-1.06 x |
| Hold slide 10 s | 1.45 x walk | 1.04-1.06 x (8 m/s at 10 s) |
| Slide-hop chain from a walk | peaks at 24 m/s | peaks at 14.6 m/s |
| Dash-jump / first slide-jump / wall-ride climb / slipway / LAUNCH RAIL | 21.2 m / 8.9 m / 7.6 m / 28.8 m/s / 3 of 6 | unchanged |

Dash, wall-jump and slam are unchanged. Mashing them never beat the skilled line: dash mashed 1.40 x against a
dash-jump's 1.86 x.

The farm (100 arena runs, the AUTOPILOT): mean round 9.52 → 10.06, median 6 → 7.5. The bot rarely slides, so this shows
the runs taking different paths, not a balance shift.

## 2. Hits land: tiered freezes and a little slow-mo (`design.impact`; HIT FREEZE and SLOW-MO in ACCESSIBILITY)

**Freezes** (seconds):

| Moment | Freeze |
| --- | --- |
| Kill | 0.04 (the web's) |
| Heavy hit | 0.03 |
| Headshot | 0.05 |
| Headshot kill | 0.08 |
| Punch hit | 0.06 |
| Punch kill | 0.09 |
| Parry | 0.25 |
| **PERFECT PARRY** | **0.30** |

The PERFECT PARRY used to freeze *shorter* than a plain parry; that is fixed.

**Slow-mo:**
- The round's last kill: 0.3 x for half a second, with a soft vignette.
- The perfect parry's tail.
- A 3-kill shot.

**The rules that keep it from stuttering:**
- Your look and your inputs stay live through every freeze.
- A budget allows at most 0.12 s frozen in any second.
- Freezes start at least 0.5 s apart, unless a bigger moment needs one (a body kill can't swallow the headshot kill
  after it).
- No freeze lasts over 0.5 s.

**The ledger** (a 300 s AUTOPILOT run per map): about 1 s frozen per minute (1.7 %), never more than 2 freezes starting in
any second.

## 3. The hands

**Every gun has its own draw, inside the sim's swap timer.** It never delays a shot: firing cancels the flourish on the
spot.
- KESTREL: a flick and a cylinder spin.
- THUNDERCLAP: a pump rack.
- COMET: a heavy lift onto the shoulder.
- LONGWAVE: a charge sweep along the barrel.
- GAUNTLET: a clench and a gem flare.

**The fist and other ports from the web:**
- **The quick punch (F) shows your left fist.** It reaches full extension inside the punch's freeze, and the knuckles
  flash on a parry.
- A glint when a slow gun is ready.
- The web's FOV kicks on multi-kills and melee, and its small camera nod on a rank-up.
- A momentum punch or GAUNTLET hit drives the gun in through the freeze (the web's hold spring).
- Forging the gun in your hand kicks it and flashes its gem.

**Also new:**
- The gun leaves its holster toward the hip.
- A quick double swap flicks instead of fully lowering.
- The hands lift on take-off, land heavier from higher, and rest on the wall during a wall-ride.
- Idle for 8 s in a break, and the gun gets fidgeted with.

## 4. The UI

**Fixed:**
- The hotbar no longer cuts names ("KESTR" → KESTREL).
- The style plate no longer greets you with "D / DULL" before you've done anything.
- One style for the first-run cards.
- TITLE's MAP row says what each map is.
- The pad wheel has a backdrop, and a hold-Y hint for pad players.
- A distinct headshot marker.

**Added:**
- Fades on RESULTS, DOWN, MODS and the start of a run; they never delay a press.
- The menu focus frame slides between rows, and buttons dip when pressed.
- Landing dust grows with the fall, and slides throw heel dust.
- A one-time hint the first time a slide's burst is spent.

**From the web, never ported until tonight** (found by checking hud.js, player.js and viewmodel.js against the port):
- **"+60" points pops** at the crosshair, in your rank's colour; a round's bonus pops big and gold.
- **The damage vignette:** dark red edges that deepen with the HP you've lost and fade as it regenerates.
- **REGEN:** a pale sheen runs along the HP bar, and a REGEN tag shows while it refills.
- **EXPOSED** pulses a red-orange edge round the screen while you camp.
- **The buy prompt** turns orange-red when you can't afford it and shakes when a buy is refused.
- **The rank-up pop:** the letter pops and the name slides in.
- **The crosshair** opens on a dash and after a THUNDERCLAP shot, and names the ARSENAL's combos under it (PRIMED,
  CROSS, SWAP).
- **Zoom** gets the web's reticle: a fine ring; the LONGWAVE gets a full scope with a rangefinder, and its charge tints
  the screen's edge.
- **Under the HP:** your speed in m/s and, in the air, the wall-jumps you have left.
- **Hits:** sparks and blasts at the web's sizes (sparks were 2.5-3 x too big). GAUNTLET hits spray cyan sparks. An
  OVERLOAD combo now pops volt bursts on every enemy it reaches (it used to show nothing). The muzzle glow comes from
  the gun.
- **The style feed** reads "+ HEADSHOT 30" in your rank's colour, and the rank bar breathes faster as you climb. Mods
  show as chips by the perks.
- **Menus:** R restarts from PAUSED, restarts and quits fade in, and RESULTS says RETRY (R).

Measured live in the Editor (no capture left for them): the rank pop's scale 1.32 → 0.93 → 1.00 in 0.26 s, the pops
rising and gone at 0.8 s, the crosshair arm 6 px out and back in 0.2 s; EXPOSED's edge pulsing 0 to 0.70; the refused
buy shaking 0 / 2 / 4 / 2 px for 0.4 s; the REGEN sheen on a 0.8 s sweep. Everything from "The crosshair ... names the
ARSENAL's combos" down was never seen at all (tests only); FEEL-PLAYTEST.md section 6 says where to look.

**Also found:** the web's default HUD STYLE, ATTACHED (the weapon block rides the gun's sway), was never ported. The port
has no row for it and is always CLASSIC; it's for the next session.

**Left:** the rank-up's screen-edge glow. It glows in the rank's colour, and S and SS are the danger red-orange and RIVAL
magenta, so an S rank-up would flash like taking damage. A neutral colour is your call.

**Ready but held back:** a cyan label on the focused button, dimmed and skipped disabled buttons, and the pad's buttons
on PAUSED. They touch the menu paths the smoke drives, and no build was left to try them, so they wait on a branch.

## 5. The proof (strips: consecutive frames stamped with real ms and the sim step; "HELD" = frozen)

`Logs/shots/n16-b2-strips/`:
- `n16-strip-punch.png`: the fist in from the lower left, held through a 0.09 s punch-kill freeze.
- `n16-strip-parry.png`: a perfect parry's 0.30 s freeze with the fist out.
- `n16-strip-draw.png`: the THUNDERCLAP and KESTREL draws, 40 frames each.

## 6. Questions only you can answer

1. **The slide:** keep all three rules? Is a 2 s recharge right? A slide timed exactly to the recharge still gains
   1.11-1.14 x a walk (a rhythm, not spam); a 3 s recharge makes it 1.09-1.12 x. An uphill slide still keeps the web's
   11.6 m/s floor: should it decay too? **And:** a slide held past its decay crawls at walking speed, crouched and
   unable to steer (a held slide restarts at once). I recommend ending a decayed slide until C is pressed again. That's
   a second Core rule, so it's your call (FEEL-PLAYTEST.md step 1).
2. **The freezes:** are the lengths right? At most two freezes start in a second, so the rest of a horde burst lands
   without a stop. Should an ordinary kill drop from 0.04 s to Vlambeer's ~0.02 s now that the big moments are bigger?
3. **Slow-mo on by default?**
4. **The GAUNTLET** sits low under the HUD's weapon plate, so its draw barely shows (its hold pose is the web's). Raise
   it, or move the plate?
5. Anything that felt worse tonight than last night: that's the next session's first job.
6. **Two calls from tonight's sweep of the web** (FEEL-DESIGN.md section 6):
   - The web's player-side effects use the rank colours (the aura, the world warming at S+, the rank-up glow), and at S
     and SS those are danger red-orange and RIVAL magenta. Should a cyan-family ramp stand in for them?
   - The muzzle flash, kill pop, rail flash and enemy hit flash aren't capped at three flashes a second the way
     tonight's are. Should they be?

**The NIGHT14 residual is found and fixed.** The smoke's steps 18 and 21 had failed on most builds since NIGHT14 (a
played run's fingerprint differing from a fresh one at checkpoint 6). Two values carried from one run into the next:
- the aim assist's scan clock: after an odd number of played steps, every scan in the next run came one step late;
- the director's damage and kill tallies from a run's last hit.

Both now reset when a run starts. A new headless check replays seven histories, and all seven match (two did before).
In tonight's final build both smoke steps pass in a Player for the first time since NIGHT14. The smoke's re-run failed
only the two pad steps, which need the window's focus, and someone was using the machine.
