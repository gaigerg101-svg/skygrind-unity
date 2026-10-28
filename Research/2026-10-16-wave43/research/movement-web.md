# Movement: wall ride / wall jump forgiveness and the jump + shot combine (wave 43 research, for G2 G3 G13 G26)

Web + GitHub research, 2026-10-16. Adds to wave 34 `movement-oss.md` (Celeste, Source, TF-recreation constants), wave 40
`feel-web.md` section 1 (attach rules, gravity curves) and wave 34 `rea-ultrakill.md` (ULTRAKILL wall jump 3 per airtime);
those numbers are not repeated. "[code]" = read in the source file this session; "[wiki]" = fetched wiki page;
"[comment]" = one forum / itch comment (weak). Decompiled or unlicensed code is read for numbers only, nothing copied.
SKYGRIND values quoted are the web base values in `PortExport/config.json` (Unity-side rows may override them: not checked).

## A. The chimney question (G2, G13): why repeated wall jumps "stop working"

1. **ULTRAKILL: hard cap 3 wall jumps per airtime; ONLY ground contact or an Enemy Step refills them.** "Wall jumps can be
   performed a maximum of three times in the air"; "The player must return to the ground or perform an Enemy Step to
   replenish their wall jumps." [wiki] https://ultrakill.wiki.gg/wiki/Movement
   - for SKYGRIND: our `wallJump.maxPerAirtime` = 3 is the same rule, and it IS the "after a certain number of jumps"
     wall in G13: the 4th press silently does nothing. ULTRAKILL gets away with it because it TELLS you ("(Max. 3
     times)" in 0-1's text) and plays a rising pitch per jump with a distinct 3rd sound; we tell nothing.
2. **Celeste: no wall-jump count at all.** `WallJump()` consumes the jump buffer, sets `forceMoveXTimer = 0.16 s` away
   from the wall, sets `Speed.X = MaxRun + 40` away and `Speed.Y = JumpSpeed`; there is no counter and no stamina cost.
   Stamina (110) is charged only for a climb-jump while GRABBING (`ClimbJumpCost = 110/4 = 27.5`), and a sideways input
   within 0.2 s of a climb-jump converts it into a free wall jump and refunds the stamina (`wallBoostTimer`). The wall
   check reaches 3 px (`WallJumpCheckDist`), 5 px for a super wall jump. [code, MIT]
   https://github.com/NoelFB/Celeste/blob/master/Source/Player/Player.cs (lines 56-61, 102-118, 682-697, 1731-1782)
   - for SKYGRIND: the climb-limiter in Celeste is "you cannot gain height on ONE wall forever", never "you may only
     bounce N times between two walls". Chimney bouncing between OPPOSITE walls is free in Celeste.
3. **Hollow Knight (Mantis Claw): unlimited wall jumps, vertical surfaces only, no stamina, no cooldown.** [wiki/guide]
   https://hollowknight.wiki/w/Mantis_Claw ; Super Mario 64 also has no wall-kick count, only a 5-frame contact window
   per wall (first frame gives extra speed). [wiki] https://ukikipedia.net/wiki/Special:MobileDiff/13332...19251
4. **Titanfall 2 (player report): a same-wall wall jump is allowed only while you are BELOW the point where you first
   latched onto that wall.** [comment] https://aaa-trsh.itch.io/trsh-mvmt/comments
   - for SKYGRIND: this is the cleanest anti-exploit rule that never breaks a chimney: you can't scale one wall by
     re-jumping it, but a different wall is always fresh.
5. **Per-wall refill is the common tutorial pattern.** Dave's Unity wall-run / climbing tutorial (copied into many repos)
   refills `wallRunJumpsLeft` / `climbJumpsLeft` whenever `newWall = hit.transform != lastWall || Angle(lastWallNormal,
   hit.normal) > minWallNormalAngleChange`, or on ground. [code, reference only, no licence checked]
   https://github.com/Dehaku/Zenthium-Heroes/blob/main/Assets/Scripts/DaveGD/WallRunning.cs (lines 80-95) and
   `Climbing.cs` (lines 118-134). A Celeste-inspired indie: "you can jump only one time on the same directional wall,
   but when you switch wall, you recover your walljump." [comment] https://itch.io/post/10901599 ; the Unreal answer
   stores `LastJumpActor` and clears it on landing: https://answers.unrealengine.com/questions/810701/view.html
6. **A same-wall lockout by normal + time, not a count.** cr-parkour (FiveM, Lua, no licence: reference only) denies a
   wall run when `now - lastEnd < sameWallCooldown (900 ms)` AND `dot(lastNormal, wall.normal) >= sameWallDot (0.9)`;
   opposite walls (dot ~ -1) are never blocked. Same config: `lostGraceMs = 120` (keep riding 120 ms after the wall ray
   misses), `maxApproachAngle = 40 deg`, `minGroundClearance = 0.35 m`, `stickGain = 8 /s` (pull toward the wall),
   `minSpeed 4.2 / endSpeed 2.8 m/s`, `gravityScale 0.28`, `maxDuration 1600 ms`. Every rejection returns a named reason
   (`WALLRUN_DENIED_SAME_WALL`, `_LOW_SPEED`, `_BAD_ANGLE`, `_TOO_LOW`...). [code]
   https://github.com/stefanocessa/cr-parkour/blob/HEAD/client/abilities/wallrun.lua (lines 25-46) and `shared/config.lua`
   - for SKYGRIND: copy the SHAPE: a reason code per refused attach / refused wall jump, logged by the bot and shown in
     a debug overlay, answers G3's "the player should understand why they attached or detached".

**Answer for SKYGRIND (derived from 1-6):** never let a wall jump fail because of a count when the player is alternating
walls. Rule: a wall jump is refused only if (a) it is the SAME wall (normal dot >= 0.9 and same collider) AND (b) the
player is higher than where they latched on it (TF2) or within ~0.9 s of leaving it. Any wall whose normal differs by
more than ~25-30 deg (dot < 0.9), or the ground, an enemy step, or a pad refills the allowance. If a hard cap must stay
for balance (ULTRAKILL's 3), show it: three pips by the reticle that empty per jump and a different sound / flash on the
last one, and print the cap in the tutorial line. The boss arena (G13) and the 3-wall climb (G2) then work by
construction instead of by tuning.

## B. Attach forgiveness numbers (G2, G3)

7. **Doom Eternal: dashing into a climbable wall grabs it automatically** (no grab timing). [guide]
   https://twinfinite.net/guides/doom-eternal-climb-grab-walls/ ; Hugo Martin's early wall climb "had to get it at just
   the right angle, and sometimes it would reject me" (the fix was forgiveness; wave 42 arenas-web).
   - for SKYGRIND: a dash (LB) that ends within the attach probe of a ridable wall should attach, even at a steep angle.
8. **Ghostrunner: one air dash per jump, refilled by touching a wall or the ground**; guides: plain jumps between walls
   can "come up short"; the dash "is the safest way to make contact". [guide/search summary]
   https://primagames.com/?p=314223 (403 on fetch; search text only)
   - for SKYGRIND: refill the dash charge on wall attach in the ride sections so "dash to the next wall" is always legal
     (G1 asks for dash in the ride section).
9. **Probe distance in other controllers vs ours.** Celeste reaches 3 px of an 8 px tile (0.375 tile, about 20 % of
   its 11 px-wide body) [code]; cr-parkour rides at `wallDistance 0.42 m` with 0.03 tolerance [code]; the TF-style
   student controller sphere-casts 35 u with radius 12 u (~0.67 m, 0.23 m) (wave 40). SKYGRIND `wallRide.probeDistance`
   = 0.12 m.
   - for SKYGRIND: 0.12 m is the tightest probe of any source read. At 16 m/s one 120 Hz step moves 0.13 m, so a
     diagonal approach can step from "outside the probe" to "pushed out by collision" with no frame inside it. Use a
     swept probe (cast along this step's velocity, not just sideways from the end position) and widen it to 0.35-0.6 m
     for ATTACH while keeping 0.12 for STAY.
10. **Speed-scaled magnetism.** cr-parkour pulls toward the wall at 8 /s of the offset (`stickGain`) and tolerates
    120 ms of lost contact [code]. Celeste keeps horizontal speed for 0.06 s after hitting a wall (`WallSpeedRetentionTime`)
    so a jump just after contact still carries momentum [code].
    - for SKYGRIND: a "lost wall" grace of 0.1-0.12 s (12-14 steps) before detaching, and a wall-jump coyote of the same
      length after a detach, cover G3's "falls off awkwardly". Our ground coyote is 0.1 s; the wall needs its own.
11. **Wall jump forces the stick away for a short time.** Celeste 0.16 s (`WallJumpForceTime`), super wall jump 0.2 s
    [code]; Karlson fork re-arms a run 0.1 s after leaving (wave 40).
    - for SKYGRIND: our `wallRide.rejoinLockTime` 0.15 s matches; apply it to the SAME wall only, so it never blocks the
      opposite wall in a narrow chimney (at 8 m/s out-speed a 2.5 m gap is crossed in ~0.3 s; a global lock of 0.15 s
      plus a 0.12 m probe is where the "fly past the next wall" in G2 comes from, by my reading).
12. **Hang before the decision.** An indie dev who replaced a Celeste-style wall with a stamina-limited wall jump: "should
    have kept a moment of wall hang to give the player a split second to decide". [comment] https://itch.io/post/10633403
    - for SKYGRIND: on attach, zero vertical speed for ~0.1 s (ULTRAKILL's cling also overrides fall speed on (re)attach,
      wiki above) so the next jump is readable.

## C. The jump + downward shot combine (G26)

13. **TF2: self-blast push is doubled when airborne.** `tf_damageforcescale_self_soldier_rj = 10.0` in the air vs
    `tf_damageforcescale_self_soldier_badrj = 5.0` on the ground; force = damage x scale (for the standard 48x48x82 hull),
    capped at 1000 u/s. [code, TF2Classic copy of Valve's tf_player.cpp, Source SDK licence: reference only]
    https://github.com/danielmm8888/TF2Classic/blob/master/src/game/server/tf/tf_player.cpp (lines 89-91, 3421-3431, 4122-4145)
    Wiki: "simultaneously jump and fire a rocket at the ground"; "The sooner the explosion is made after jumping, the
    farther and faster the Soldier flies"; Soldier takes 40 % less self-damage in the air. [wiki]
    https://wiki.teamfortress.com/wiki/Rocket_jump
    - for SKYGRIND: this is exactly G26's bug class. If the shot resolves on a step where the player is still grounded
      (the jump press and the shot in the same frame, or the shot 1-2 steps before the jump), the game must treat it as
      the airborne case. Rule: a down-shot within a +-0.15 s window of a jump press (either order) gets the full
      "boost jump" impulse, and its vertical velocity REPLACES (max) rather than adds to a negative fall speed.
14. **Quake 3: knockback = 1000 x min(damage, 200) / 200 u/s; self damage halved; for 50-200 ms after the hit
    (`t = knockback * 2`, clamped) ground friction is off and air acceleration is used** (`PMF_TIME_KNOCKBACK`). [code, GPL-2.0]
    https://github.com/ioquake/ioq3/blob/main/code/game/g_combat.c (lines 878-912) and `bg_pmove.c` (lines 196-201, 769-787)
    - for SKYGRIND: during a boost window, skip ground friction and ground snapping, or a grounded down-shot is eaten by
      the floor in its first step. Our `boostJump.window` = 0.15 s with `v0Mult` 1.8 is the right shape; the NIGHT42
      row boost42 (lift 11, left OFF) is the one to check against this list.
15. **Crouch / ctap = closer to the blast = higher.** Crouch-jump reaches 72 HU vs 50 HU; ctap rocket jumps are higher
    "because crouching puts you closer to the ground". [wiki] https://wiki.teamfortress.com/wiki/Rocket_jump
    - for SKYGRIND: do not make height depend on the player's distance to the blast point in the first frames: make the
      combine a fixed, readable impulse (the TF2 skill ceiling is the opposite of what G26 asks for).
16. **Teach small first.** Wave 35 `teach.md` (ULTRAKILL 0-1 one verb per room; Portal "introduce slowly, step by
    step"; two new skills at once "too much"). G26 asks for small jumps, then bigger.
    - for SKYGRIND: three ledges at 40 %, 70 %, 100 % of the combined jump's measured apex, each with a lava / pit
      consequence and a 2 s retry, before the first challenge that needs the full height.

## Not found
Titanfall 2's official wall-run time and wall-jump rules (fandom wiki returned 402); Celeste's jump buffer length (Input.cs
not in the repo); Neon White has no wall run as such (cards give double jump / dash); Mirror's Edge "once per wall" rule:
not found; Ghostrunner wall-run duration: not found.
